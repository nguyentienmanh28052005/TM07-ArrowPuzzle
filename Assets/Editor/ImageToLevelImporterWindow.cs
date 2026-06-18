using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Image-to-Level Importer EditorWindow
/// Chuyển đổi ảnh pixel art thành LevelDataV2 có đầy đủ rắn và validate solvability.
///
/// WORKFLOW:
///  1. Kéo ảnh pixel art vào field "Source Image"
///  2. Cấu hình màu nền (Background Color) và độ nhạy (Tolerance)
///  3. Cấu hình bảng màu nhận diện
///  4. Nhấn "Preview" để xem kết quả nhận diện và tự do chỉnh sửa màu, hướng đầu rắn, xóa nhiễu
///  5. Nhấn "Generate & Validate" để tạo LevelDataV2 và kiểm tra giải được
/// </summary>
public class ImageToLevelImporterWindow : EditorWindow
{
    // ==========================================
    // HẰNG SỐ & CẤU HÌNH
    // ==========================================
    private const int   MAX_SOLVER_STEPS = 10000;         // giới hạn bước giải
    private const string SAVE_PATH_KEY   = "ImgLevel_SavePath";
    private const string DEFAULT_SAVE_PATH = "Assets/Resources/Levels";

    // ==========================================
    [SerializeField] private Texture2D sourceImage;
    [SerializeField] private bool       autoGridSize = true;
    [SerializeField] private float      detailLevelPercent = 100f; // 5% to 100%
    [SerializeField] private int        gridWidth  = 10;
    [SerializeField] private int        gridHeight = 10;
    [SerializeField] private int        minFeatureSize = 2; // Độ dài tối thiểu để giữ lại rắn (lọc chi tiết nhỏ/nhiễu)
    [SerializeField] private int        targetSnakeCount = 0; // Số lượng rắn tối đa muốn giữ lại (0 = lấy tất cả)
    [SerializeField] private bool       useThinning = true; // Bật/tắt thuật toán co xương (thinning)
    [SerializeField] private bool       useSilhouetteMode = false; // Trộn tất cả màu thành một silhouette duy nhất
    [SerializeField] private bool       colorizeFromPalette = true; // Tô màu ngẫu nhiên/khác biệt từ Palette
    [SerializeField] private int        maxSnakeLength = 7; // Độ dài tối đa của rắn khi phân rã tự động
    [SerializeField] private float      windingRate = 0.6f; // Tỉ lệ uốn lượn (0-1)
    [SerializeField] private int        randomSeed = 42; // Seed ngẫu nhiên để thay đổi mẫu gen
    [SerializeField] private bool       fillGaps = true; // Tự động điền đầy khoảng trống
    [SerializeField] private bool       allowShortSnakes = true; // Cho phép rắn ngắn (độ dài >= 2) để lấp đầy góc hẹp
    [SerializeField] private string     levelFileName = "Level_FromImage";
    [SerializeField] private int        levelIndex    = 0;
    [SerializeField] private GameMode   gameMode      = GameMode.Classic;
    [SerializeField] private LevelDifficulty difficulty = LevelDifficulty.Easy;

    private bool isPaintingDirty = false;

    // Background & Tolerances
    [SerializeField] private Color backgroundColor = Color.white;
    [SerializeField] private float backgroundTolerance = 0.2f;
    [SerializeField] private float colorMatchTolerance = 0.3f;
    [SerializeField] private int   minNeighborDenoise = 1; // 0 = Tắt, 1-8 = Lọc nhiễu lân cận

    // Bảng màu dùng để nhận diện rắn
    private List<Color> palette = new List<Color>
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.cyan,
        Color.magenta,
        new Color(1f, 0.5f, 0f),   // Cam
        new Color(0.5f, 0f, 0.5f), // Tím
        new Color(0.6f, 0.3f, 0f), // Nâu
        new Color(1f, 0.75f, 0.8f) // Hồng nhạt
    };

    // Kết quả preview & vẽ tương tác
    private Texture2D previewTex;
    private Color?[,] editGrid;
    private Color selectedPaintColor = Color.red;
    private bool isEraserMode = false;
    private List<SnakeSaveData> previewSnakes = new List<SnakeSaveData>();
    private string statusMessage = "";
    private Color  statusColor   = Color.white;
    private bool   isValidated   = false;
    private bool   isSolvable    = false;
    private Vector2 scrollPos;
    private bool   showPaletteSettings = false;
    private bool   showAdvancedSettings = false;

    // ==========================================
    // MENU & KHỞI TẠO
    // ==========================================
    [MenuItem("ArrowPuzzle/Image → Level Importer")]
    public static void ShowWindow()
    {
        var win = GetWindow<ImageToLevelImporterWindow>("Image → Level");
        win.minSize = new Vector2(460, 680);
    }

    private void OnEnable()
    {
        // Khởi tạo lại nếu cần
    }

    // ==========================================
    // GIAO DIỆN CHÍNH (OnGUI)
    // ==========================================
    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawHeader();
        EditorGUILayout.Space(6);

        DrawImageInput();
        EditorGUILayout.Space(6);

        DrawGridSettings();
        EditorGUILayout.Space(6);

        DrawAdvancedSettings();
        EditorGUILayout.Space(6);

        DrawPaletteSection();
        EditorGUILayout.Space(6);

        DrawOutputSettings();
        EditorGUILayout.Space(6);

        DrawActionButtons();
        EditorGUILayout.Space(6);

        DrawStatusBox();
        EditorGUILayout.Space(6);

        DrawPreview();

        EditorGUILayout.EndScrollView();
    }

    // ─── Header ───
    private void DrawHeader()
    {
        var style = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 16,
            alignment = TextAnchor.MiddleCenter
        };
        GUILayout.Label("🖼  Image → Level Importer", style, GUILayout.Height(28));
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }

    // ─── Ảnh đầu vào ───
    private void DrawImageInput()
    {
        GUILayout.Label("1. Ảnh Pixel Art Đầu Vào", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        sourceImage = (Texture2D)EditorGUILayout.ObjectField(
            "Source Image", sourceImage, typeof(Texture2D), false);
        if (EditorGUI.EndChangeCheck())
        {
            ResetPreview();
            if (sourceImage != null)
            {
                EnsureReadable(sourceImage);
                backgroundColor = sourceImage.GetPixel(0, sourceImage.height - 1);
                InitializeEditGridFromImage();
            }
        }

        if (sourceImage != null)
        {
            EditorGUILayout.HelpBox(
                $"Kích thước gốc: {sourceImage.width} × {sourceImage.height} px\n" +
                $"Ảnh sẽ được chia thành lưới {gridWidth} × {gridHeight}.",
                MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Kéo ảnh pixel art vào đây để bắt đầu.",
                MessageType.None);
        }
    }

    // ─── Cài đặt grid ───
    private void DrawGridSettings()
    {
        GUILayout.Label("2. Độ Chi Tiết & Kích Thước Grid", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUI.BeginChangeCheck();
        autoGridSize = EditorGUILayout.Toggle("Tự động tính Grid theo ảnh", autoGridSize);
        
        if (autoGridSize)
        {
            EditorGUI.BeginDisabledGroup(sourceImage == null);
            detailLevelPercent = EditorGUILayout.Slider("Độ chi tiết (%)", detailLevelPercent, 5f, 100f);
            
            if (sourceImage != null)
            {
                gridWidth = Mathf.Clamp(Mathf.RoundToInt(sourceImage.width * (detailLevelPercent / 100f)), 3, 100);
                gridHeight = Mathf.Clamp(Mathf.RoundToInt(sourceImage.height * (detailLevelPercent / 100f)), 3, 1000);
            }
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            gridWidth  = EditorGUILayout.IntField("Grid Width",  gridWidth);
            gridHeight = EditorGUILayout.IntField("Grid Height", gridHeight);
            EditorGUILayout.EndHorizontal();

            gridWidth  = Mathf.Clamp(gridWidth,  3, 100);
            gridHeight = Mathf.Clamp(gridHeight, 3, 1000);
        }
        bool gridDimensionsChanged = EditorGUI.EndChangeCheck();

        EditorGUI.BeginChangeCheck();
        minFeatureSize = EditorGUILayout.IntSlider("Độ dài tối thiểu rắn", minFeatureSize, 2, 20);
        maxSnakeLength = EditorGUILayout.IntSlider("Độ dài tối đa rắn", maxSnakeLength, 3, 20);
        useThinning = EditorGUILayout.Toggle("Chỉ lấy khung xương (Thinning)", useThinning);
        useSilhouetteMode = EditorGUILayout.Toggle("Chế độ Silhouette (Đơn sắc)", useSilhouetteMode);
        colorizeFromPalette = EditorGUILayout.Toggle("Tô màu ngẫu nhiên từ Palette", colorizeFromPalette);

        if (!useThinning)
        {
            windingRate = EditorGUILayout.Slider("Mức uốn lượn (Winding)", windingRate, 0f, 1f);
            randomSeed = EditorGUILayout.IntField("Seed ngẫu nhiên", randomSeed);
            fillGaps = EditorGUILayout.Toggle("Điền đầy khoảng trống (Fill)", fillGaps);
            if (fillGaps)
            {
                allowShortSnakes = EditorGUILayout.Toggle("  Cho phép rắn ngắn (>=2)", allowShortSnakes);
            }
        }

        targetSnakeCount = EditorGUILayout.IntField("Số lượng rắn mong muốn (0 = Tất cả)", targetSnakeCount);
        targetSnakeCount = Mathf.Max(0, targetSnakeCount);
        bool snakeParamsChanged = EditorGUI.EndChangeCheck();

        if (gridDimensionsChanged)
        {
            InitializeEditGridFromImage();
        }
        else if (snakeParamsChanged)
        {
            RebuildSnakesFromEditedGrid();
        }

        string targetStr = targetSnakeCount > 0 ? $"{targetSnakeCount} con" : "Tất cả";
        string modeStr = useThinning ? "Chỉ lấy khung xương" : (useSilhouetteMode ? "Silhouette đặc" : "Phân rã diện tích");
        EditorGUILayout.LabelField($"Kích thước Grid: {gridWidth} × {gridHeight} ({modeStr} | Rắn {minFeatureSize}-{maxSnakeLength} ô | Tối đa: {targetStr})");
        EditorGUILayout.EndVertical();
    }

    // ─── Cài đặt nhận diện nâng cao ───
    private void DrawAdvancedSettings()
    {
        showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "3. Nhận Diện Nâng Cao (Bộ Lọc Nền)", true);
        if (!showAdvancedSettings) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        backgroundColor = EditorGUILayout.ColorField("Màu Nền (Background Color)", backgroundColor);
        EditorGUI.BeginDisabledGroup(sourceImage == null);
        if (GUILayout.Button("Hút màu từ ảnh", GUILayout.Width(110)))
        {
            EnsureReadable(sourceImage);
            backgroundColor = sourceImage.GetPixel(0, sourceImage.height - 1);
            backgroundTolerance = 0.15f;
            SetStatus("Đã tự động lấy màu nền từ góc trên-trái của ảnh mẫu.", Color.green);
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        backgroundTolerance = EditorGUILayout.Slider("Độ Nhạy Nền (BG Tolerance)", backgroundTolerance, 0.01f, 1f);
        colorMatchTolerance = EditorGUILayout.Slider("Độ Nhạy Rắn (Color Tolerance)", colorMatchTolerance, 0.01f, 1f);
        if (EditorGUI.EndChangeCheck())
        {
            InitializeEditGridFromImage();
        }
        EditorGUILayout.EndVertical();
    }

    // ─── Bảng màu ───
    private void DrawPaletteSection()
    {
        showPaletteSettings = EditorGUILayout.Foldout(showPaletteSettings, "4. Bảng Màu Rắn Nhận Diện", true);
        if (!showPaletteSettings) return;

        EditorGUILayout.HelpBox(
            $"Có {palette.Count} màu trong bảng. Mỗi pixel không phải nền sẽ được gán vào màu gần nhất trong bảng này.",
            MessageType.None);

        EditorGUI.BeginChangeCheck();
        
        bool paletteModified = false;
        for (int i = 0; i < palette.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            palette[i] = EditorGUILayout.ColorField($"Màu {i + 1}", palette[i]);
            if (GUILayout.Button("✕", GUILayout.Width(24)) && palette.Count > 2)
            {
                palette.RemoveAt(i);
                paletteModified = true;
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Thêm Màu"))
        {
            palette.Add(new Color(Random.value, Random.value, Random.value));
            paletteModified = true;
        }
        if (GUILayout.Button("Reset Mặc Định"))
        {
            ResetPalette();
            paletteModified = true;
        }
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck() || paletteModified)
        {
            RebuildSnakesFromEditedGrid();
        }
    }

    // ─── Cài đặt đầu ra ───
    private void DrawOutputSettings()
    {
        GUILayout.Label("5. Thông Tin Level SO", EditorStyles.boldLabel);
        levelFileName = EditorGUILayout.TextField("Tên File",   levelFileName);
        levelIndex    = EditorGUILayout.IntField("Level Index", levelIndex);
        gameMode      = (GameMode)EditorGUILayout.EnumPopup("Game Mode", gameMode);
        difficulty    = (LevelDifficulty)EditorGUILayout.EnumPopup("Độ Khó",   difficulty);
    }

    // ─── Nút hành động ───
    private void DrawActionButtons()
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        EditorGUI.BeginDisabledGroup(sourceImage == null);

        if (GUILayout.Button("🔍  Preview (Nhận diện ảnh & Trích xuất xương)", GUILayout.Height(38)))
            RunPreview();

        EditorGUILayout.Space(4);

        EditorGUI.BeginDisabledGroup(previewSnakes.Count == 0);
        if (GUILayout.Button("✅  Generate & Validate (Tạo + Kiểm Tra)", GUILayout.Height(38)))
            RunGenerateAndValidate();
        EditorGUI.EndDisabledGroup();

        EditorGUI.EndDisabledGroup();
    }

    // ─── Hộp trạng thái ───
    private void DrawStatusBox()
    {
        if (string.IsNullOrEmpty(statusMessage)) return;

        var old = GUI.backgroundColor;
        GUI.backgroundColor = statusColor * 0.7f + Color.gray * 0.3f;
        EditorGUILayout.HelpBox(statusMessage,
            statusColor == Color.green ? MessageType.Info :
            statusColor == Color.red   ? MessageType.Error : MessageType.Warning);
        GUI.backgroundColor = old;
    }

    // ─── Preview ảnh nhận diện và danh sách chỉnh sửa ───
    private void DrawPreview()
    {
        if (editGrid == null) return;

        DrawPaintingToolbar();
        EditorGUILayout.Space(4);

        GUILayout.Label($"Xem Trước Canvas ({gridWidth}x{gridHeight}) & Nhấp chuột để Vẽ/Xóa:", EditorStyles.boldLabel);
        DrawInteractivePixelGrid();

        // Danh sách rắn cho phép tùy biến trực quan
        EditorGUILayout.Space(6);
        GUILayout.Label("Tinh Chỉnh Rắn Từng Con:", EditorStyles.boldLabel);

        for (int i = 0; i < previewSnakes.Count; i++)
        {
            var s = previewSnakes[i];
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            
            // Ô màu visual
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = s.arrowColor;
            GUILayout.Box("", GUILayout.Width(18), GUILayout.Height(18));
            GUI.backgroundColor = oldColor;

            GUILayout.Label($"Rắn {i + 1} ({s.segmentPositions.Count} ô):", GUILayout.Width(100));

            // Cho phép đổi màu trực tiếp
            EditorGUI.BeginChangeCheck();
            s.arrowColor = EditorGUILayout.ColorField(GUIContent.none, s.arrowColor, false, false, false, GUILayout.Width(50));
            
            // Cho phép đổi hướng đầu rắn trực tiếp
            s.direction = (ArrowDir)EditorGUILayout.EnumPopup(s.direction, GUILayout.Width(70));
            if (EditorGUI.EndChangeCheck())
            {
                isSolvable = IsConfigurationSolvable(previewSnakes, gridWidth, gridHeight);
                previewTex = BuildPreviewTexture(previewSnakes, gridWidth, gridHeight);
            }

            GUILayout.FlexibleSpace();

            // Nút Xóa nhiễu / Xóa rắn thừa
            if (GUILayout.Button("✕", GUILayout.Width(22), GUILayout.Height(18)))
            {
                previewSnakes.RemoveAt(i);
                previewTex = BuildPreviewTexture(previewSnakes, gridWidth, gridHeight);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        if (isValidated)
        {
            EditorGUILayout.Space(6);
            var style = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13 };
            style.normal.textColor = isSolvable ? Color.green : new Color(1f, 0.4f, 0f);
            GUILayout.Label(isSolvable
                ? "✅ Level CÓ THỂ giải được (Auto-Solver xác nhận)"
                : "⚠️ Level CHƯA xác nhận giải được — hãy tinh chỉnh hướng đầu rắn hoặc sửa đổi trong EditorScene",
                style);
        }
    }

    // ==========================================
    // LOGIC THỰC THI CHÍNH
    // ==========================================

    private void RunPreview()
    {
        ResetPreview();
        if (sourceImage == null) return;

        InitializeEditGridFromImage();

        if (previewSnakes.Count == 0)
        {
            SetStatus("Không nhận diện được rắn nào. Hãy thử điều chỉnh màu nền hoặc bộ lọc nhiễu.", Color.red);
            return;
        }

        if (isSolvable)
        {
            SetStatus($"Nhận diện thành công {previewSnakes.Count} rắn và TỰ ĐỘNG TÌM ĐƯỢC hướng giải được! Bạn có thể nhấn giữ chuột trái để vẽ thêm màu hoặc chuột phải để xóa nhiễu.", Color.green);
        }
        else
        {
            SetStatus($"Nhận diện được {previewSnakes.Count} rắn nhưng CHƯA TÌM ĐƯỢC hướng giải. Bạn có thể nhấn chuột trái để vẽ thêm màu, chuột phải để xóa hoặc xoay hướng đầu rắn ở dưới.", Color.yellow);
        }
        Repaint();
    }

    private void RunGenerateAndValidate()
    {
        if (previewSnakes.Count == 0) return;

        // 1. Kiểm tra snake hợp lệ về cấu trúc
        string structureErr = ValidateSnakeStructure(previewSnakes, gridWidth, gridHeight);
        if (structureErr != null)
        {
            SetStatus("Lỗi cấu trúc rắn:\n" + structureErr, Color.red);
            return;
        }

        // 2. Kiểm tra solvability lần cuối bằng bộ mô phỏng BFS đầy đủ
        bool solvable = SimulateSolve(previewSnakes, gridWidth, gridHeight, out string solveLog);
        isValidated = true;
        isSolvable  = solvable;

        // 3. Tạo file asset
        string saveDir  = EditorPrefs.GetString(SAVE_PATH_KEY, DEFAULT_SAVE_PATH);
        string fullPath = $"{saveDir}/{levelFileName}.asset";
        System.IO.Directory.CreateDirectory(Application.dataPath.Replace("Assets", saveDir));

        LevelDataV2 existingAsset = AssetDatabase.LoadAssetAtPath<LevelDataV2>(fullPath);
        LevelDataV2 levelData;
        if (existingAsset != null)
        {
            levelData = existingAsset;
        }
        else
        {
            levelData = ScriptableObject.CreateInstance<LevelDataV2>();
            AssetDatabase.CreateAsset(levelData, fullPath);
        }

        // Gán data
        LevelDataV2Writer.ClearContent(levelData);
        LevelDataV2Writer.SetSnakes(levelData, previewSnakes);
        levelData.levelIndex     = levelIndex;
        levelData.gameMode       = gameMode;
        levelData.levelDifficulty = difficulty;
        levelData.timeLimit       = 60f;

        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = levelData;

        string msg = $"Base level đã lưu tại: {fullPath}\n" +
                     $"Số rắn: {LevelDataV2Queries.GetArrowCount(levelData)}\n" +
                     $"Trạng thái giải: {(solvable ? "GIẢI ĐƯỢC ✅" : "CHƯA THỂ GIẢI ⚠️")}\n" +
                     (solvable ? "" : $"\nChi tiết solver:\n{solveLog}");
        SetStatus(msg, solvable ? Color.green : Color.yellow);
        Repaint();
    }

    private void InitializeEditGridFromImage()
    {
        if (sourceImage == null)
        {
            editGrid = new Color?[gridWidth, gridHeight];
            return;
        }

        EnsureReadable(sourceImage);
        editGrid = new Color?[gridWidth, gridHeight];

        for (int gy = 0; gy < gridHeight; gy++)
        {
            for (int gx = 0; gx < gridWidth; gx++)
            {
                Color matched = GetDominantColor(sourceImage, gx, gy, gridWidth, gridHeight);
                if (matched != Color.clear)
                {
                    editGrid[gx, gy] = matched;
                }
            }
        }
        
        ApplyDenoisePass();
        RebuildSnakesFromEditedGrid();
    }

    private void ApplyDenoisePass()
    {
        if (minNeighborDenoise <= 0 || editGrid == null) return;

        int w = editGrid.GetLength(0);
        int h = editGrid.GetLength(1);
        Color?[,] temp = (Color?[,])editGrid.Clone();

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (!editGrid[x, y].HasValue) continue;

                int neighbors = 0;
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                        {
                            if (editGrid[nx, ny].HasValue)
                            {
                                neighbors++;
                            }
                        }
                    }
                }

                if (neighbors < minNeighborDenoise)
                {
                    temp[x, y] = null;
                }
            }
        }
        editGrid = temp;
    }

    private void RebuildSnakesFromEditedGrid()
    {
        if (editGrid == null) return;

        int cols = editGrid.GetLength(0);
        int rows = editGrid.GetLength(1);

        previewSnakes = TraceSnakesFromColorGrid(editGrid, cols, rows);

        bool solvable = OptimizeDirections(previewSnakes, cols, rows);
        isValidated = true;
        isSolvable  = solvable;

        previewTex = BuildPreviewTexture(previewSnakes, cols, rows);
    }

    private List<SnakeSaveData> TraceSnakesFromColorGrid(Color?[,] cellColors, int cols, int rows)
    {
        if (useSilhouetteMode)
        {
            var silhouetteColors = new Color?[cols, rows];
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (cellColors[x, y].HasValue)
                    {
                        silhouetteColors[x, y] = Color.white;
                    }
                }
            }
            cellColors = silhouetteColors;
        }

        var result = new List<SnakeSaveData>();

        if (useThinning)
        {
            // ─── CHẾ ĐỘ 1: THINNING (CO XƯƠNG MỎNG 1PX) ───
            var thinnedColors = new Color?[cols, rows];
            var activeColors = new HashSet<Color>();

            for (int gy = 0; gy < rows; gy++)
            for (int gx = 0; gx < cols; gx++)
            {
                if (cellColors[gx, gy].HasValue)
                    activeColors.Add(cellColors[gx, gy].Value);
            }

            foreach (var color in activeColors)
            {
                bool[,] binaryGrid = new bool[cols, rows];
                for (int gy = 0; gy < rows; gy++)
                for (int gx = 0; gx < cols; gx++)
                {
                    if (cellColors[gx, gy] == color)
                        binaryGrid[gx, gy] = true;
                }

                bool[,] thinnedGrid = ThinBinaryGrid(binaryGrid, cols, rows);

                for (int gy = 0; gy < rows; gy++)
                for (int gx = 0; gx < cols; gx++)
                {
                    if (thinnedGrid[gx, gy])
                    {
                        thinnedColors[gx, gy] = color;
                    }
                }
            }

            var colorGroups = new Dictionary<Color, List<Vector2Int>>();
            for (int gy = 0; gy < rows; gy++)
            for (int gx = 0; gx < cols; gx++)
            {
                if (thinnedColors[gx, gy].HasValue)
                {
                    Color c = thinnedColors[gx, gy].Value;
                    if (!colorGroups.ContainsKey(c))
                        colorGroups[c] = new List<Vector2Int>();
                    colorGroups[c].Add(new Vector2Int(gx, gy));
                }
            }

            foreach (var kvp in colorGroups)
            {
                Color color = kvp.Key;
                List<Vector2Int> pixels = kvp.Value;

                var components = GetConnectedComponents(pixels);
                foreach (var comp in components)
                {
                    if (comp.Count < minFeatureSize) continue;

                    List<Vector2Int> ordered = TracePathFromComponent(comp);
                    if (ordered == null || ordered.Count < minFeatureSize) continue;

                    ArrowDir dir = ComputeHeadDirection(ordered);

                    var snake = new SnakeSaveData
                    {
                        arrowColor       = color,
                        direction        = dir,
                        hasArrowShadow   = false,
                        segmentPositions = ordered
                    };
                    result.Add(snake);
                }
            }
        }
        else
        {
            // ─── CHẾ ĐỘ 2: PHÂN RÃ DIỆN TÍCH (GREEDY PATH COVER) ───
            var colorGroups = new Dictionary<Color, List<Vector2Int>>();
            for (int gy = 0; gy < rows; gy++)
            for (int gx = 0; gx < cols; gx++)
            {
                if (cellColors[gx, gy].HasValue)
                {
                    Color c = cellColors[gx, gy].Value;
                    if (!colorGroups.ContainsKey(c))
                        colorGroups[c] = new List<Vector2Int>();
                    colorGroups[c].Add(new Vector2Int(gx, gy));
                }
            }

            foreach (var kvp in colorGroups)
            {
                Color color = kvp.Key;
                List<Vector2Int> pixels = kvp.Value;

                var components = GetConnectedComponents(pixels);
                foreach (var comp in components)
                {
                    if (comp.Count < minFeatureSize) continue;

                    // Phân rã cụm pixel thành các đường đi không chồng lấp
                    var decomposedPaths = DecomposeIntoPaths(comp, minFeatureSize, maxSnakeLength);

                    foreach (var path in decomposedPaths)
                    {
                        ArrowDir dir = ComputeHeadDirection(path);

                        var snake = new SnakeSaveData
                        {
                            arrowColor       = color,
                            direction        = dir,
                            hasArrowShadow   = false,
                            segmentPositions = path
                        };
                        result.Add(snake);
                    }
                }
            }
        }

        // Sắp xếp các con rắn theo độ dài giảm dần (ưu tiên giữ lại các con to vẽ rõ nét nhất)
        var sortedSnakes = result.OrderByDescending(s => s.segmentPositions.Count).ToList();
        List<SnakeSaveData> finalSnakes;
        if (targetSnakeCount > 0 && sortedSnakes.Count > targetSnakeCount)
        {
            finalSnakes = sortedSnakes.Take(targetSnakeCount).ToList();
        }
        else
        {
            finalSnakes = sortedSnakes;
        }

        if (colorizeFromPalette)
        {
            ApplyColoringFromPalette(finalSnakes, cols, rows);
        }

        return finalSnakes;
    }

    private void ApplyColoringFromPalette(List<SnakeSaveData> snakes, int cols, int rows)
    {
        if (snakes.Count == 0 || palette.Count == 0) return;

        // 1. Xây dựng bản đồ chiếm dụng để tìm lân cận nhanh hơn
        int[,] grid = new int[cols, rows];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                grid[x, y] = -1;
            }
        }

        for (int i = 0; i < snakes.Count; i++)
        {
            foreach (var p in snakes[i].segmentPositions)
            {
                if (p.x >= 0 && p.x < cols && p.y >= 0 && p.y < rows)
                {
                    grid[p.x, p.y] = i;
                }
            }
        }

        // 2. Tìm danh sách các con rắn kề nhau (neighbors)
        var neighbors = new List<HashSet<int>>();
        for (int i = 0; i < snakes.Count; i++)
        {
            neighbors.Add(new HashSet<int>());
        }

        for (int i = 0; i < snakes.Count; i++)
        {
            foreach (var p in snakes[i].segmentPositions)
            {
                var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                foreach (var d in dirs)
                {
                    int nx = p.x + d.x;
                    int ny = p.y + d.y;
                    if (nx >= 0 && nx < cols && ny >= 0 && ny < rows)
                    {
                        int other = grid[nx, ny];
                        if (other != -1 && other != i)
                        {
                            neighbors[i].Add(other);
                            neighbors[other].Add(i);
                        }
                    }
                }
            }
        }

        // 3. Sử dụng thuật toán tô màu Greedy
        int[] colorsAssigned = new int[snakes.Count];
        for (int i = 0; i < snakes.Count; i++) colorsAssigned[i] = -1;

        int[] colorUseCount = new int[palette.Count];

        for (int i = 0; i < snakes.Count; i++)
        {
            var neighborColors = new HashSet<int>();
            foreach (var nb in neighbors[i])
            {
                if (colorsAssigned[nb] != -1)
                {
                    neighborColors.Add(colorsAssigned[nb]);
                }
            }

            int bestColorIdx = -1;
            int minUse = int.MaxValue;

            for (int c = 0; c < palette.Count; c++)
            {
                if (!neighborColors.Contains(c))
                {
                    if (colorUseCount[c] < minUse)
                    {
                        minUse = colorUseCount[c];
                        bestColorIdx = c;
                    }
                }
            }

            if (bestColorIdx == -1)
            {
                int minNeighborUse = int.MaxValue;
                for (int c = 0; c < palette.Count; c++)
                {
                    if (colorUseCount[c] < minNeighborUse)
                    {
                        minNeighborUse = colorUseCount[c];
                        bestColorIdx = c;
                    }
                }
            }

            colorsAssigned[i] = bestColorIdx;
            colorUseCount[bestColorIdx]++;
            snakes[i].arrowColor = palette[bestColorIdx];
        }
    }

    private List<List<Vector2Int>> DecomposeIntoPaths(List<Vector2Int> pixels, int minLen, int maxLen)
    {
        var paths = new List<List<Vector2Int>>();
        var remaining = new HashSet<Vector2Int>(pixels);
        var rand = new System.Random(randomSeed);

        // Danh sách các điểm start bị thất bại (để tránh lặp vô hạn)
        var failedStarts = new HashSet<Vector2Int>();

        while (remaining.Count > 0)
        {
            // Lọc các điểm start chưa từng thất bại
            var candidates = remaining.Where(p => !failedStarts.Contains(p)).ToList();
            if (candidates.Count == 0)
            {
                // Nếu tất cả các điểm còn lại đều đã thử làm start và thất bại,
                // chúng ta dừng vòng lặp chính và chuyển sang bước xử lý gom nhóm/điền đầy
                break;
            }

            // Tìm điểm bắt đầu tốt nhất trong số ứng viên
            Vector2Int start = candidates.First();
            int minNeighbors = 9;
            foreach (var p in candidates)
            {
                int n = CountNeighborsInSet(p, remaining);
                if (n < minNeighbors)
                {
                    minNeighbors = n;
                    start = p;
                }
            }

            var path = new List<Vector2Int> { start };
            remaining.Remove(start);

            Vector2Int current = start;
            Vector2Int lastDir = Vector2Int.zero;

            while (path.Count < maxLen)
            {
                var neighbors = Get4Neighbors(current).Where(n => remaining.Contains(n)).ToList();
                if (neighbors.Count == 0)
                {
                    neighbors = Get8Neighbors(current).Where(n => remaining.Contains(n)).ToList();
                    if (neighbors.Count == 0) break;
                }

                Vector2Int next;
                if (lastDir != Vector2Int.zero)
                {
                    Vector2Int straightNeighbor = current + lastDir;
                    bool hasStraight = neighbors.Contains(straightNeighbor);
                    var turnNeighbors = neighbors.Where(n => n != straightNeighbor).ToList();

                    bool chooseTurn = false;
                    if (turnNeighbors.Count > 0)
                    {
                        if (!hasStraight)
                            chooseTurn = true;
                        else
                            chooseTurn = rand.NextDouble() < windingRate;
                    }

                    if (chooseTurn && turnNeighbors.Count > 0)
                        next = turnNeighbors[rand.Next(turnNeighbors.Count)];
                    else if (hasStraight)
                        next = straightNeighbor;
                    else
                        next = neighbors[rand.Next(neighbors.Count)];
                }
                else
                {
                    next = neighbors[rand.Next(neighbors.Count)];
                }

                lastDir = next - current;
                path.Add(next);
                remaining.Remove(next);
                current = next;
            }

            if (path.Count >= minLen)
            {
                paths.Add(path);
            }
            else
            {
                // Trả lại các pixel vào remaining
                foreach (var p in path)
                {
                    remaining.Add(p);
                }
                // Đánh dấu start này là đã thử và thất bại
                failedStarts.Add(start);
            }
        }

        // ─── PHẦN 2: GAP FILLING (ĐIỀN ĐẦY KHOẢNG TRỐNG) ───
        if (fillGaps && remaining.Count > 0)
        {
            bool progress = true;
            while (progress && remaining.Count > 0)
            {
                progress = false;
                foreach (var p in remaining.ToList())
                {
                    // Tìm xem ô p có kề với đầu hoặc đuôi của con rắn nào không
                    foreach (var path in paths)
                    {
                        if (path.Count >= maxLen) continue;

                        Vector2Int head = path[0];
                        Vector2Int tail = path[path.Count - 1];

                        // Kiểm tra khoảng cách Manhattan kề cạnh (4 hướng)
                        if (Mathf.Abs(p.x - head.x) + Mathf.Abs(p.y - head.y) == 1)
                        {
                            path.Insert(0, p);
                            remaining.Remove(p);
                            progress = true;
                            break;
                        }
                        else if (Mathf.Abs(p.x - tail.x) + Mathf.Abs(p.y - tail.y) == 1)
                        {
                            path.Add(p);
                            remaining.Remove(p);
                            progress = true;
                            break;
                        }
                    }
                    if (progress) break; // Khởi động lại vòng lặp duyệt để đảm bảo tính nhất quán
                }
            }

            // Nếu vẫn còn thừa pixel và cho phép rắn ngắn, tạo rắn từ phần còn lại
            if (allowShortSnakes && remaining.Count > 0)
            {
                var leftoverComponents = GetConnectedComponents(remaining.ToList());
                foreach (var comp in leftoverComponents)
                {
                    if (comp.Count >= 2)
                    {
                        var ordered = TracePathFromComponent(comp);
                        if (ordered != null && ordered.Count >= 2)
                        {
                            paths.Add(ordered);
                            foreach (var p in ordered)
                            {
                                remaining.Remove(p);
                            }
                        }
                    }
                }
            }
        }

        return paths;
    }

    private int CountNeighborsInSet(Vector2Int p, HashSet<Vector2Int> set)
    {
        int count = 0;
        foreach (var d in Get8Neighbors(p))
        {
            if (set.Contains(d)) count++;
        }
        return count;
    }

    private List<Vector2Int> Get4Neighbors(Vector2Int p)
    {
        return new List<Vector2Int>
        {
            p + Vector2Int.up,
            p + Vector2Int.down,
            p + Vector2Int.left,
            p + Vector2Int.right
        };
    }

    private List<Vector2Int> Get8Neighbors(Vector2Int p)
    {
        return new List<Vector2Int>
        {
            p + Vector2Int.up,
            p + Vector2Int.down,
            p + Vector2Int.left,
            p + Vector2Int.right,
            p + new Vector2Int(1, 1),
            p + new Vector2Int(1, -1),
            p + new Vector2Int(-1, 1),
            p + new Vector2Int(-1, -1)
        };
    }

    private void DrawPaintingToolbar()
    {
        GUILayout.Label("Công Cụ Vẽ Bản Đồ (Paint Tools)", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        var oldColor = GUI.backgroundColor;
        if (isEraserMode) GUI.backgroundColor = Color.red;
        if (GUILayout.Button(isEraserMode ? "🧹 Eraser (BẬT)" : "🧹 Eraser Mode", GUILayout.Width(140), GUILayout.Height(26)))
        {
            isEraserMode = !isEraserMode;
        }
        GUI.backgroundColor = oldColor;

        GUILayout.Label("Chọn Màu Vẽ:", GUILayout.Width(90));
        
        for (int i = 0; i < Mathf.Min(palette.Count, 8); i++)
        {
            var color = palette[i];
            
            GUI.backgroundColor = color;
            string label = (!isEraserMode && selectedPaintColor == color) ? "●" : "";
            if (GUILayout.Button(label, GUILayout.Width(26), GUILayout.Height(26)))
            {
                selectedPaintColor = color;
                isEraserMode = false;
            }
        }
        GUI.backgroundColor = oldColor;

        EditorGUILayout.EndHorizontal();
    }

    private void DrawInteractivePixelGrid()
    {
        if (editGrid == null) return;

        int cols = editGrid.GetLength(0);
        int rows = editGrid.GetLength(1);

        float maxW = position.width - 24;
        float maxH = 500f; // Giới hạn chiều cao hiển thị tối đa để tránh tràn giao diện
        float aspect = (float)cols / rows;

        float dispW, dispH;
        if (aspect > 1f)
        {
            dispW = Mathf.Min(maxW, 500f);
            dispH = dispW / aspect;
            if (dispH > maxH)
            {
                dispH = maxH;
                dispW = dispH * aspect;
            }
        }
        else
        {
            dispH = Mathf.Min(maxH, 500f);
            dispW = dispH * aspect;
            if (dispW > maxW)
            {
                dispW = maxW;
                dispH = dispW / aspect;
            }
        }

        Rect gridRect = GUILayoutUtility.GetRect(dispW, dispH);
        
        if (previewTex != null)
        {
            GUI.DrawTexture(gridRect, previewTex, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUI.DrawRect(gridRect, new Color(0.1f, 0.1f, 0.1f, 1f));
        }

        DrawOutline(gridRect, Color.grey, 2);

        Event e = Event.current;
        if (gridRect.Contains(e.mousePosition))
        {
            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            {
                float cellW = gridRect.width / cols;
                float cellH = gridRect.height / rows;

                int gx = Mathf.FloorToInt((e.mousePosition.x - gridRect.x) / cellW);
                int gy = rows - 1 - Mathf.FloorToInt((e.mousePosition.y - gridRect.y) / cellH);

                if (gx >= 0 && gx < cols && gy >= 0 && gy < rows)
                {
                    bool changed = false;
                    Color? newColor = null;

                    if (e.button == 0)
                    {
                        if (isEraserMode)
                        {
                            if (editGrid[gx, gy] != null)
                            {
                                editGrid[gx, gy] = null;
                                newColor = null;
                                changed = true;
                            }
                        }
                        else
                        {
                            if (editGrid[gx, gy] != selectedPaintColor)
                            {
                                editGrid[gx, gy] = selectedPaintColor;
                                newColor = selectedPaintColor;
                                changed = true;
                            }
                        }
                    }
                    else if (e.button == 1)
                    {
                        if (editGrid[gx, gy] != null)
                        {
                            editGrid[gx, gy] = null;
                            newColor = null;
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        e.Use();
                        PaintPreviewCell(gx, gy, newColor);
                        isPaintingDirty = true;
                        Repaint();
                    }
                }
            }
        }

        // Kích hoạt Rebuild khi nhả chuột hoặc chuột rời khỏi khung vẽ
        if (isPaintingDirty && (e.type == EventType.MouseUp || e.rawType == EventType.MouseUp || e.type == EventType.MouseLeaveWindow))
        {
            isPaintingDirty = false;
            RebuildSnakesFromEditedGrid();
            Repaint();
        }
    }

    private void PaintPreviewCell(int gx, int gy, Color? color)
    {
        if (previewTex == null) return;
        int cols = editGrid.GetLength(0);
        int rows = editGrid.GetLength(1);
        int scale = Mathf.Clamp(1024 / Mathf.Max(cols, rows), 2, 32);

        Color fill = color.HasValue ? color.Value : new Color(0.12f, 0.12f, 0.12f, 1f);
        int px = gx * scale;
        int py = gy * scale;

        // Tô màu ô lưới
        for (int dy = 1; dy < scale - 1; dy++)
        {
            for (int dx = 1; dx < scale - 1; dx++)
            {
                previewTex.SetPixel(px + dx, py + dy, fill);
            }
        }

        // Vẽ viền ô
        Color border = color.HasValue ? Color.Lerp(color.Value, Color.black, 0.4f) : new Color(0.12f, 0.12f, 0.12f, 1f);
        for (int d = 0; d < scale; d++)
        {
            previewTex.SetPixel(px + d, py,           border);
            previewTex.SetPixel(px + d, py + scale-1, border);
            previewTex.SetPixel(px,           py + d, border);
            previewTex.SetPixel(px + scale-1, py + d, border);
        }

        previewTex.Apply();
    }

    private void DrawOutline(Rect rect, Color color, int thickness)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), color);
    }

    // ─── Lấy màu nổi bật nhất trong ô Grid ───
    private Color GetDominantColor(Texture2D tex, int gx, int gy, int cols, int rows)
    {
        int texW = tex.width;
        int texH = tex.height;

        int x0 = Mathf.FloorToInt((float)gx / cols * texW);
        int x1 = Mathf.FloorToInt((float)(gx + 1) / cols * texW);
        int y0 = Mathf.FloorToInt((float)gy / rows * texH);
        int y1 = Mathf.FloorToInt((float)(gy + 1) / rows * texH);

        x1 = Mathf.Clamp(x1, x0 + 1, texW);
        y1 = Mathf.Clamp(y1, y0 + 1, texH);

        var colorCounts = new Dictionary<Color, int>();
        int totalPixels = 0;
        int backgroundPixels = 0;

        for (int py = y0; py < y1; py++)
        {
            for (int px = x0; px < x1; px++)
            {
                Color c = tex.GetPixel(px, py);
                totalPixels++;

                if (IsPixelBackground(c))
                {
                    backgroundPixels++;
                }
                else
                {
                    Color matched = MatchPalette(c);
                    if (colorCounts.ContainsKey(matched))
                        colorCounts[matched]++;
                    else
                        colorCounts[matched] = 1;
                }
            }
        }

        if (totalPixels == 0) return Color.clear;

        // Nếu quá bán số pixel là nền thì coi như ô này là nền
        if ((float)backgroundPixels / totalPixels > 0.5f)
        {
            return Color.clear;
        }

        Color dominant = Color.clear;
        int maxCount = 0;

        foreach (var kvp in colorCounts)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                dominant = kvp.Key;
            }
        }

        if (maxCount > 0)
        {
            return dominant;
        }

        return Color.clear;
    }

    // ─── Kiểm tra pixel có phải màu nền ───
    private bool IsPixelBackground(Color c)
    {
        if (c.a < 0.1f) return true;
        float dist = ColorDistance(c, backgroundColor);
        return dist < (backgroundTolerance * backgroundTolerance * 195075f);
    }

    // ─── Khớp màu trong Palette (Perceptual Redmean) ───
    private Color MatchPalette(Color c)
    {
        Color best  = palette[0];
        float bestD = ColorDistance(c, palette[0]);
        for (int i = 1; i < palette.Count; i++)
        {
            float d = ColorDistance(c, palette[i]);
            if (d < bestD) { bestD = d; best = palette[i]; }
        }
        return best;
    }

    // ─── Khoảng cách màu Perceptual Redmean ───
    private float ColorDistance(Color a, Color b)
    {
        float rmean = (a.r + b.r) * 255f / 2f;
        float r = (a.r - b.r) * 255f;
        float g = (a.g - b.g) * 255f;
        float bl = (a.b - b.b) * 255f;
        float weightR = 2f + rmean / 256f;
        float weightG = 4f;
        float weightB = 2f + (255f - rmean) / 256f;
        return weightR * r * r + weightG * g * g + weightB * bl * bl;
    }

    // ==========================================
    // BƯỚC 2: THINNING & PATH TRACING
    // ==========================================

    private bool[,] ThinBinaryGrid(bool[,] originalGrid, int w, int h)
    {
        // Thêm viền đệm 1px để không bị lỗi cắt góc mép biên
        int pw = w + 2;
        int ph = h + 2;
        bool[,] padded = new bool[pw, ph];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                padded[x + 1, y + 1] = originalGrid[x, y];
            }
        }

        bool[,] thinnedPadded = ZhangSuenThinning(padded, pw, ph);

        bool[,] result = new bool[w, h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                result[x, y] = thinnedPadded[x + 1, y + 1];
            }
        }
        return result;
    }

    private bool[,] ZhangSuenThinning(bool[,] temp, int w, int h)
    {
        bool changed = true;
        var toDelete = new List<Vector2Int>();

        while (changed)
        {
            changed = false;

            // Sub-iteration 1
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    if (!temp[x, y]) continue;

                    int p2 = temp[x, y + 1] ? 1 : 0;
                    int p3 = temp[x + 1, y + 1] ? 1 : 0;
                    int p4 = temp[x + 1, y] ? 1 : 0;
                    int p5 = temp[x + 1, y - 1] ? 1 : 0;
                    int p6 = temp[x, y - 1] ? 1 : 0;
                    int p7 = temp[x - 1, y - 1] ? 1 : 0;
                    int p8 = temp[x - 1, y] ? 1 : 0;
                    int p9 = temp[x - 1, y + 1] ? 1 : 0;

                    int b = p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9;
                    int a = TransitionCount(p2, p3, p4, p5, p6, p7, p8, p9);

                    if (b >= 2 && b <= 6 && a == 1 && (p2 * p4 * p6 == 0) && (p4 * p6 * p8 == 0))
                    {
                        toDelete.Add(new Vector2Int(x, y));
                    }
                }
            }
            if (toDelete.Count > 0)
            {
                foreach (var p in toDelete) temp[p.x, p.y] = false;
                toDelete.Clear();
                changed = true;
            }

            // Sub-iteration 2
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    if (!temp[x, y]) continue;

                    int p2 = temp[x, y + 1] ? 1 : 0;
                    int p3 = temp[x + 1, y + 1] ? 1 : 0;
                    int p4 = temp[x + 1, y] ? 1 : 0;
                    int p5 = temp[x + 1, y - 1] ? 1 : 0;
                    int p6 = temp[x, y - 1] ? 1 : 0;
                    int p7 = temp[x - 1, y - 1] ? 1 : 0;
                    int p8 = temp[x - 1, y] ? 1 : 0;
                    int p9 = temp[x - 1, y + 1] ? 1 : 0;

                    int b = p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9;
                    int a = TransitionCount(p2, p3, p4, p5, p6, p7, p8, p9);

                    if (b >= 2 && b <= 6 && a == 1 && (p2 * p4 * p8 == 0) && (p2 * p6 * p8 == 0))
                    {
                        toDelete.Add(new Vector2Int(x, y));
                    }
                }
            }
            if (toDelete.Count > 0)
            {
                foreach (var p in toDelete) temp[p.x, p.y] = false;
                toDelete.Clear();
                changed = true;
            }
        }
        return temp;
    }

    private int TransitionCount(int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9)
    {
        int count = 0;
        if (p2 == 0 && p3 == 1) count++;
        if (p3 == 0 && p4 == 1) count++;
        if (p4 == 0 && p5 == 1) count++;
        if (p5 == 0 && p6 == 1) count++;
        if (p6 == 0 && p7 == 1) count++;
        if (p7 == 0 && p8 == 1) count++;
        if (p8 == 0 && p9 == 1) count++;
        if (p9 == 0 && p2 == 1) count++;
        return count;
    }

    // Tách các pixel của xương thành các cụm kết nối
    private List<List<Vector2Int>> GetConnectedComponents(List<Vector2Int> pixels)
    {
        var set       = new HashSet<Vector2Int>(pixels);
        var visited   = new HashSet<Vector2Int>();
        var result    = new List<List<Vector2Int>>();
        var dirs4     = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var p in pixels)
        {
            if (visited.Contains(p)) continue;

            var comp  = new List<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(p);
            visited.Add(p);

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                comp.Add(cur);
                foreach (var d in dirs4)
                {
                    var nb = cur + d;
                    if (set.Contains(nb) && !visited.Contains(nb))
                    {
                        visited.Add(nb);
                        queue.Enqueue(nb);
                    }
                }
            }
            result.Add(comp);
        }
        return result;
    }

    // Khôi phục tuần tự đường đi của xương
    private List<Vector2Int> TracePathFromComponent(List<Vector2Int> comp)
    {
        if (comp.Count < 2) return null;

        var set = new HashSet<Vector2Int>(comp);
        var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        var degree = new Dictionary<Vector2Int, int>();
        foreach (var p in comp)
        {
            int deg = 0;
            foreach (var d in dirs)
                if (set.Contains(p + d)) deg++;
            degree[p] = deg;
        }

        var endpoints = comp.Where(p => degree[p] == 1).ToList();
        Vector2Int start = endpoints.Count > 0 ? endpoints[0] : comp[0];

        var path = new List<Vector2Int> { start };
        var visited = new HashSet<Vector2Int> { start };
        Vector2Int current = start;

        while (true)
        {
            Vector2Int next = Vector2Int.zero;
            bool found = false;

            // Tìm hàng xóm gần nhất (Manhattan)
            foreach (var d in dirs)
            {
                Vector2Int nb = current + d;
                if (set.Contains(nb) && !visited.Contains(nb))
                {
                    next = nb;
                    found = true;
                    break;
                }
            }

            // Nếu không có, cho phép đi chéo dự phòng phòng nhiễu
            if (!found)
            {
                var dirs8 = new[] {
                    Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
                    new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 1)
                };
                foreach (var d in dirs8)
                {
                    Vector2Int nb = current + d;
                    if (set.Contains(nb) && !visited.Contains(nb))
                    {
                        next = nb;
                        found = true;
                        break;
                    }
                }
            }

            if (!found) break;

            path.Add(next);
            visited.Add(next);
            current = next;
        }

        return path;
    }

    private ArrowDir ComputeHeadDirection(List<Vector2Int> path)
    {
        if (path.Count < 2) return ArrowDir.Right;
        Vector2Int delta = path[1] - path[0];

        if (delta == Vector2Int.up)    return ArrowDir.Up;
        if (delta == Vector2Int.down)  return ArrowDir.Down;
        if (delta == Vector2Int.left)  return ArrowDir.Left;
        if (delta == Vector2Int.right) return ArrowDir.Right;

        return ArrowDir.Right;
    }

    // ==========================================
    // BƯỚC 3: KIỂM TRA HỢP LỆ
    // ==========================================

    private string ValidateSnakeStructure(List<SnakeSaveData> snakes, int cols, int rows)
    {
        var occupied = new Dictionary<Vector2Int, int>();

        for (int si = 0; si < snakes.Count; si++)
        {
            var snake = snakes[si];
            if (snake.segmentPositions == null || snake.segmentPositions.Count < 2)
                return $"Rắn {si + 1} phải có từ 2 đốt trở lên.";

            var segs = snake.segmentPositions;
            for (int i = 0; i < segs.Count; i++)
            {
                var p = segs[i];
                if (p.x < 0 || p.x >= cols || p.y < 0 || p.y >= rows)
                    return $"Rắn {si + 1}: Điểm {p} vượt ra ngoài lưới.";

                if (occupied.TryGetValue(p, out int other))
                    return $"Rắn {si + 1} và rắn {other + 1} đang bị đè lên cùng ô {p}.";
                occupied[p] = si;

                if (i > 0)
                {
                    int md = Mathf.Abs(p.x - segs[i - 1].x) + Mathf.Abs(p.y - segs[i - 1].y);
                    if (md > 2) // Cho phép cả bước chéo nhỏ để tăng tỷ lệ import thành công
                        return $"Rắn {si + 1}: Các đốt không liền kề nhau.";
                }
            }
        }
        return null;
    }

    // ==========================================
    // BƯỚC 4: BỘ GIẢI ĐỐ TỰ ĐỘNG (SOLVER SIMULATOR)
    // ==========================================

    private bool SimulateSolve(List<SnakeSaveData> snakes, int cols, int rows, out string logMsg)
    {
        var sb = new System.Text.StringBuilder();
        var timer = System.Diagnostics.Stopwatch.StartNew();
        long timeoutMs = 1000; // Giới hạn 1 giây để không treo Editor Unity khi lưới lớn

        var initState = new SolveState(snakes, cols, rows);
        var visited   = new HashSet<string>();
        var queue     = new Queue<SolveState>();
        queue.Enqueue(initState);
        visited.Add(initState.GetKey());

        int steps = 0;
        while (queue.Count > 0 && steps < MAX_SOLVER_STEPS)
        {
            steps++;

            if (steps % 100 == 0 && timer.ElapsedMilliseconds > timeoutMs)
            {
                sb.AppendLine($"Hết thời gian giả lập ({timeoutMs}ms) - Level quá phức tạp hoặc có thể chưa giải được.");
                logMsg = sb.ToString();
                return false;
            }

            var state = queue.Dequeue();
            if (state.IsGoal())
            {
                sb.AppendLine($"Tìm thấy cách giải sau {steps} bước ({timer.ElapsedMilliseconds} ms)!");
                logMsg = sb.ToString();
                return true;
            }

            foreach (var next in state.GetMoves())
            {
                string key = next.GetKey();
                if (!visited.Contains(key))
                {
                    visited.Add(key);
                    queue.Enqueue(next);
                }
            }
        }

        sb.AppendLine(steps >= MAX_SOLVER_STEPS
            ? $"Đã quét hết giới hạn {MAX_SOLVER_STEPS} bước."
            : "Bộ giải không tìm thấy đáp án hợp lệ.");
        logMsg = sb.ToString();
        return false;
    }

    private class SolveState
    {
        public readonly List<List<Vector2Int>> Snakes;
        public readonly List<ArrowDir>         Dirs;
        private readonly int cols, rows;

        public SolveState(List<SnakeSaveData> data, int c, int r)
        {
            cols = c; rows = r;
            Snakes = data.Select(s => new List<Vector2Int>(s.segmentPositions)).ToList();
            Dirs   = data.Select(s => s.direction).ToList();
        }

        private SolveState(List<List<Vector2Int>> snakes, List<ArrowDir> dirs, int c, int r)
        {
            cols = c; rows = r;
            Snakes = snakes.Select(s => new List<Vector2Int>(s)).ToList();
            Dirs   = new List<ArrowDir>(dirs);
        }

        public bool IsGoal() => Snakes.All(s => s.Count == 0);

        public string GetKey()
        {
            var parts = new System.Text.StringBuilder();
            for (int i = 0; i < Snakes.Count; i++)
            {
                parts.Append((int)Dirs[i]);
                parts.Append(':');
                foreach (var p in Snakes[i])
                    parts.Append($"{p.x},{p.y};");
                parts.Append('|');
            }
            return parts.ToString();
        }

        public IEnumerable<SolveState> GetMoves()
        {
            var occupied = new HashSet<Vector2Int>();
            foreach (var s in Snakes)
                foreach (var p in s)
                    occupied.Add(p);

            for (int i = 0; i < Snakes.Count; i++)
            {
                if (Snakes[i].Count == 0) continue;

                Vector2Int head = Snakes[i][0];
                Vector2Int step = DirToVec(Dirs[i]);
                Vector2Int newHead = head + step;

                Vector2Int tail = Snakes[i][Snakes[i].Count - 1];
                bool headConflict;

                if (IsOutside(newHead))
                {
                    headConflict = false;
                }
                else
                {
                    headConflict = occupied.Contains(newHead) && newHead != tail;
                }

                if (headConflict) continue;

                var newSnakes = Snakes.Select(s => new List<Vector2Int>(s)).ToList();
                var newDirs   = new List<ArrowDir>(Dirs);

                List<Vector2Int> snakeCopy = newSnakes[i];
                snakeCopy.RemoveAt(snakeCopy.Count - 1); // Đuôi co lại 1 nấc

                if (!IsOutside(newHead))
                    snakeCopy.Insert(0, newHead);
                else
                    snakeCopy.Clear(); // Bò ra hẳn ngoài

                yield return new SolveState(newSnakes, newDirs, cols, rows);
            }
        }

        private bool IsOutside(Vector2Int p)
            => p.x < 0 || p.x >= cols || p.y < 0 || p.y >= rows;

        private static Vector2Int DirToVec(ArrowDir dir) => dir switch
        {
            ArrowDir.Up    => Vector2Int.up,
            ArrowDir.Down  => Vector2Int.down,
            ArrowDir.Left  => Vector2Int.left,
            ArrowDir.Right => Vector2Int.right,
            _              => Vector2Int.up
        };
    }

    // ==========================================
    // BƯỚC 5: TẠO PREVIEW TEXTURE
    // ==========================================

    private Texture2D BuildPreviewTexture(List<SnakeSaveData> snakes, int cols, int rows)
    {
        // Điều chỉnh động độ phân giải vẽ preview để tránh lag với lưới lớn
        int scale = Mathf.Clamp(1024 / Mathf.Max(cols, rows), 2, 32);

        var tex = new Texture2D(cols * scale, rows * scale, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // Điền nền tối
        Color bg = new Color(0.12f, 0.12f, 0.12f, 1f);
        var bgPixels = Enumerable.Repeat(bg, cols * scale * rows * scale).ToArray();
        tex.SetPixels(bgPixels);

        foreach (var snake in snakes)
        {
            Color c = snake.arrowColor;
            for (int si = 0; si < snake.segmentPositions.Count; si++)
            {
                var p   = snake.segmentPositions[si];
                bool isHead = si == 0;
                Color fill  = isHead ? Color.Lerp(c, Color.white, 0.45f) : c;

                int px = p.x * scale;
                int py = p.y * scale;

                // Tô phần thân bên trong
                for (int dy = 1; dy < scale - 1; dy++)
                for (int dx = 1; dx < scale - 1; dx++)
                    tex.SetPixel(px + dx, py + dy, fill);

                // Tô viền
                Color border = Color.Lerp(c, Color.black, 0.4f);
                for (int d = 0; d < scale; d++)
                {
                    tex.SetPixel(px + d, py,           border);
                    tex.SetPixel(px + d, py + scale-1, border);
                    tex.SetPixel(px,           py + d, border);
                    tex.SetPixel(px + scale-1, py + d, border);
                }

                // Nếu là đầu, vẽ mũi tên nhỏ hướng đi
                if (isHead && scale >= 6)
                {
                    DrawArrowOnPreview(tex, px, py, scale, snake.direction);
                }
            }
        }

        tex.Apply();
        return tex;
    }

    private void DrawArrowOnPreview(Texture2D tex, int px, int py, int scale, ArrowDir dir)
    {
        Color arrowColor = Color.black;
        int mid = scale / 2;

        // Vẽ mũi tên hướng đi cơ bản dựa trên tâm ô
        for (int i = 2; i < scale - 2; i++)
        {
            switch (dir)
            {
                case ArrowDir.Up:
                    tex.SetPixel(px + mid, py + i, arrowColor);
                    if (i >= scale / 2)
                    {
                        tex.SetPixel(px + mid - (scale - 1 - i), py + i, arrowColor);
                        tex.SetPixel(px + mid + (scale - 1 - i), py + i, arrowColor);
                    }
                    break;
                case ArrowDir.Down:
                    tex.SetPixel(px + mid, py + i, arrowColor);
                    if (i <= scale / 2)
                    {
                        tex.SetPixel(px + mid - i, py + i, arrowColor);
                        tex.SetPixel(px + mid + i, py + i, arrowColor);
                    }
                    break;
                case ArrowDir.Left:
                    tex.SetPixel(px + i, py + mid, arrowColor);
                    if (i <= scale / 2)
                    {
                        tex.SetPixel(px + i, py + mid - i, arrowColor);
                        tex.SetPixel(px + i, py + mid + i, arrowColor);
                    }
                    break;
                case ArrowDir.Right:
                    tex.SetPixel(px + i, py + mid, arrowColor);
                    if (i >= scale / 2)
                    {
                        tex.SetPixel(px + i, py + mid - (scale - 1 - i), arrowColor);
                        tex.SetPixel(px + i, py + mid + (scale - 1 - i), arrowColor);
                    }
                    break;
            }
        }
    }

    // ==========================================
    // TIỆN ÍCH
    // ==========================================

    private void ResetPreview()
    {
        previewTex    = null;
        previewSnakes = new List<SnakeSaveData>();
        isValidated   = false;
        isSolvable    = false;
        statusMessage = "";
        Repaint();
    }

    private void ResetPalette()
    {
        palette = new List<Color>
        {
            Color.red, Color.green, Color.blue, Color.yellow,
            Color.cyan, Color.magenta,
            new Color(1f, 0.5f, 0f),
            new Color(0.5f, 0f, 0.5f),
            new Color(0.6f, 0.3f, 0f),
            new Color(1f, 0.75f, 0.8f)
        };
    }

    private void SetStatus(string msg, Color color)
    {
        statusMessage = msg;
        statusColor   = color;
        Repaint();
    }

    // ==========================================
    // TỰ ĐỘNG TỐI ƯU HƯỚNG GIẢI ĐƯỢC (DIRECTION OPTIMIZER)
    // ==========================================

    private int backtrackCount = 0;
    private const int MAX_BACKTRACK_STEPS = 5000;

    private List<ArrowDir> GetPreferredDirections(SnakeSaveData snake, int[] occupiedGrid, int cols, int rows)
    {
        var dirs = new List<ArrowDir> { ArrowDir.Up, ArrowDir.Down, ArrowDir.Left, ArrowDir.Right };
        var scores = new Dictionary<ArrowDir, (int blockCount, int exitDist)>();

        Vector2Int head = snake.segmentPositions[0];

        foreach (var dir in dirs)
        {
            Vector2Int step = DirToVec(dir);
            Vector2Int curr = head + step;
            int blockCount = 0;
            int exitDist = 0;

            while (curr.x >= 0 && curr.x < cols && curr.y >= 0 && curr.y < rows)
            {
                exitDist++;
                int idx = curr.y * cols + curr.x;
                if (occupiedGrid[idx] > 0)
                {
                    if (!snake.segmentPositions.Contains(curr))
                    {
                        blockCount++;
                    }
                }
                curr += step;
            }

            scores[dir] = (blockCount, exitDist);
        }

        dirs.Sort((a, b) =>
        {
            var scoreA = scores[a];
            var scoreB = scores[b];
            int cmp = scoreA.blockCount.CompareTo(scoreB.blockCount);
            if (cmp != 0) return cmp;
            return scoreA.exitDist.CompareTo(scoreB.exitDist);
        });

        return dirs;
    }

    private bool BacktrackOptimizeHeuristic(List<SnakeSaveData> snakes, List<List<ArrowDir>> preferredDirs, int index, int cols, int rows)
    {
        if (index >= snakes.Count)
        {
            return IsConfigurationSolvable(snakes, cols, rows);
        }

        backtrackCount++;
        if (backtrackCount > MAX_BACKTRACK_STEPS)
        {
            return false;
        }

        var originalDir = snakes[index].direction;
        var candDirs = preferredDirs[index];

        foreach (var dir in candDirs)
        {
            snakes[index].direction = dir;
            if (BacktrackOptimizeHeuristic(snakes, preferredDirs, index + 1, cols, rows))
            {
                return true;
            }
        }

        snakes[index].direction = originalDir;
        return false;
    }

    private bool OptimizeDirections(List<SnakeSaveData> snakes, int cols, int rows)
    {
        if (snakes.Count == 0) return true;

        int[] occupiedGrid = new int[cols * rows];
        foreach (var s in snakes)
        {
            foreach (var p in s.segmentPositions)
            {
                int idx = p.y * cols + p.x;
                if (idx >= 0 && idx < occupiedGrid.Length)
                    occupiedGrid[idx]++;
            }
        }

        var preferredDirs = new List<List<ArrowDir>>();
        for (int i = 0; i < snakes.Count; i++)
        {
            preferredDirs.Add(GetPreferredDirections(snakes[i], occupiedGrid, cols, rows));
        }

        for (int i = 0; i < snakes.Count; i++)
        {
            snakes[i].direction = preferredDirs[i][0];
        }

        if (IsConfigurationSolvable(snakes, cols, rows))
            return true;

        backtrackCount = 0;
        if (BacktrackOptimizeHeuristic(snakes, preferredDirs, 0, cols, rows))
        {
            return true;
        }

        var random = new System.Random(42);
        for (int attempt = 0; attempt < 500; attempt++)
        {
            for (int i = 0; i < snakes.Count; i++)
            {
                var cand = preferredDirs[i];
                snakes[i].direction = cand[random.Next(cand.Count)];
            }

            if (IsConfigurationSolvable(snakes, cols, rows))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsConfigurationSolvable(List<SnakeSaveData> snakes, int cols, int rows)
    {
        int snakeCount = snakes.Count;
        if (snakeCount == 0) return true;

        // Xây dựng ma trận lưới chiếm dụng
        int[] occupied = new int[cols * rows];
        for (int i = 0; i < snakeCount; i++)
        {
            var snake = snakes[i];
            foreach (var p in snake.segmentPositions)
            {
                int idx = p.y * cols + p.x;
                if (idx >= 0 && idx < occupied.Length)
                    occupied[idx]++;
            }
        }

        bool[] remaining = new bool[snakeCount];
        for (int i = 0; i < remaining.Length; i++) remaining[i] = true;

        int remainingCount = snakeCount;
        while (remainingCount > 0)
        {
            bool foundMovableSnake = false;

            for (int i = 0; i < snakeCount; i++)
            {
                if (!remaining[i]) continue;

                var snake = snakes[i];
                
                // Tạm thời nhấc các đốt của con rắn này ra khỏi lưới chiếm dụng
                foreach (var p in snake.segmentPositions)
                {
                    int idx = p.y * cols + p.x;
                    if (idx >= 0 && idx < occupied.Length) occupied[idx]--;
                }

                // Kiểm tra xem tia đi thẳng ra ngoài của đầu rắn có bị chặn không
                if (IsExitRayClear(snake, occupied, cols, rows))
                {
                    remaining[i] = false;
                    remainingCount--;
                    foundMovableSnake = true;
                    break;
                }

                // Đặt lại các ô chiếm dụng nếu chưa thể đi ra
                foreach (var p in snake.segmentPositions)
                {
                    int idx = p.y * cols + p.x;
                    if (idx >= 0 && idx < occupied.Length) occupied[idx]++;
                }
            }

            if (!foundMovableSnake)
            {
                return false; // Bị kẹt cứng (Deadlock)
            }
        }

        return true; // Tất cả đã thoát ra được
    }

    private bool IsExitRayClear(SnakeSaveData snake, int[] occupied, int cols, int rows)
    {
        if (snake.segmentPositions == null || snake.segmentPositions.Count == 0) return true;
        Vector2Int head = snake.segmentPositions[0];
        Vector2Int step = DirToVec(snake.direction);

        Vector2Int curr = head + step;
        while (curr.x >= 0 && curr.x < cols && curr.y >= 0 && curr.y < rows)
        {
            int idx = curr.y * cols + curr.x;
            if (occupied[idx] > 0)
            {
                return false;
            }
            curr += step;
        }
        return true;
    }

    private static Vector2Int DirToVec(ArrowDir dir) => dir switch
    {
        ArrowDir.Up    => Vector2Int.up,
        ArrowDir.Down  => Vector2Int.down,
        ArrowDir.Left  => Vector2Int.left,
        ArrowDir.Right => Vector2Int.right,
        _              => Vector2Int.up
    };

    private static void EnsureReadable(Texture2D tex)
    {
        string path = AssetDatabase.GetAssetPath(tex);
        if (string.IsNullOrEmpty(path)) return;
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
            Debug.Log($"[ImageImporter] Đã tự động kích hoạt Read/Write cho '{path}'.");
        }
    }
}
