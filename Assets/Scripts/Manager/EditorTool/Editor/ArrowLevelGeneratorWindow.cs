using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ArrowLevelGeneratorWindow : EditorWindow
{
    private const string DefaultOutputFolder = "Assets/Resources/Levels/Generated";
    private const int MaxGridDimension = 300;
    private static readonly Color32 PlacementMaskEnabledColor = new Color32(51, 217, 89, 255);
    private static readonly Color32 PlacementMaskDisabledColor = new Color32(64, 64, 64, 255);
    private static readonly Color PlacementMaskBackgroundColor = new Color(0.08f, 0.08f, 0.08f, 1f);
    private static readonly Color PlacementMaskGridLineColor = new Color(0.08f, 0.08f, 0.08f, 0.85f);

    private int levelIndex = 1000;
    private GameMode gameMode = GameMode.Classic;
    private LevelDifficulty levelDifficulty = LevelDifficulty.Medium;
    private bool returnToDefaultZoomAfterIntro = true;
    private float timeLimit = 60f;
    private float rewardCoins = 100f;
    private float rewardDiamonds = 5f;

    private int width = 300;
    private int height = 300;
    private bool centerGridOnOrigin = true;
    private bool centerGeneratedBounds = true;
    private int originX = -4;
    private int originY = -4;
    private bool usePaintedPlacementArea = false;
    private bool[] placementMask;
    private int placementMaskWidth;
    private int placementMaskHeight;
    private Vector2 windowScroll;
    private Vector2 placementMaskScroll;
    private float placementMaskZoom = 0.5f;
    private int placementMaskViewportWidth = 760;
    private int placementMaskViewportHeight = 720;
    private Texture2D placementMaskTexture;
    private Color32[] placementMaskTexturePixels;
    private bool placementMaskTextureDirty = true;
    private int cachedEnabledPlacementCells;
    private bool placementEnabledCountDirty = true;
    private int placementBrushSize = 1;
    private bool useSquarePlacementBrush;
    private int squarePlacementSide = 5;
    private bool isPaintingPlacementMask;
    private bool placementPaintValue;
    private int lastPaintedPlacementIndex = -1;
    private int lastPaintedPlacementX = -1;
    private int lastPaintedPlacementY = -1;

    private int targetArrowCount = 24;
    private int minSnakeLength = 3;
    private int maxSnakeLength = 31;
    private int maxAttemptsPerArrow = 512;
    private int bodyAttemptsPerCandidate = 8;
    private int minDistanceBetweenSnakes = 2;
    private int minStraightCellsPerSegment = 3;
    private bool fillAvailableArea = true;
    private bool requireFullFill = false;
    private int fillSearchAttempts = 2048;
    private int fillLayoutAttempts = 12;
    private bool allowBentSnakes = true;
    private int turnChancePercent = 65;
    private bool useRandomSeed = true;
    private int seed = 12345;
    private bool useMonochromeColor = false;
    private Color monochromeColor = Color.white;

    private string outputFolder = DefaultOutputFolder;
    private string filePrefix = "A";

    private LevelGeneratorCore.Result lastResult;
    private string lastAssetPath;

    private void OnEnable()
    {
        EnsurePlacementMaskSize();
    }

    private void OnDisable()
    {
        ReleasePlacementMaskTexture();
    }

    [MenuItem("Tools/Arrow Escape/Procedural Level Generator")]
    public static void Open()
    {
        ArrowLevelGeneratorWindow window = GetWindow<ArrowLevelGeneratorWindow>("Arrow Level Generator");
        window.minSize = new Vector2(380f, 520f);
        window.Show();
    }

    private void OnGUI()
    {
        windowScroll = EditorGUILayout.BeginScrollView(windowScroll);
        DrawLevelDataSection();
        EditorGUILayout.Space(8f);
        DrawGridSection();
        EditorGUILayout.Space(8f);
        DrawGenerationSection();
        EditorGUILayout.Space(8f);
        DrawOutputSection();
        EditorGUILayout.Space(12f);
        DrawActions();
        EditorGUILayout.Space(8f);
        DrawResult();
        EditorGUILayout.EndScrollView();
    }

    private void DrawLevelDataSection()
    {
        EditorGUILayout.LabelField("Level Data", EditorStyles.boldLabel);
        levelIndex = EditorGUILayout.IntField("Level Index", levelIndex);
        gameMode = (GameMode)EditorGUILayout.EnumPopup("Game Mode", gameMode);
        levelDifficulty = (LevelDifficulty)EditorGUILayout.EnumPopup("Difficulty", levelDifficulty);
        returnToDefaultZoomAfterIntro = EditorGUILayout.Toggle("Return To Default Zoom", returnToDefaultZoomAfterIntro);
        timeLimit = EditorGUILayout.FloatField("Time Limit", timeLimit);
        rewardCoins = EditorGUILayout.FloatField("Reward Coins", rewardCoins);
        rewardDiamonds = EditorGUILayout.FloatField("Reward Diamonds", rewardDiamonds);
    }

    private void DrawGridSection()
    {
        EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
        width = Mathf.Clamp(EditorGUILayout.IntField("Width", width), 1, MaxGridDimension);
        height = Mathf.Clamp(EditorGUILayout.IntField("Height", height), 1, MaxGridDimension);
        EnsurePlacementMaskSize();
        centerGridOnOrigin = EditorGUILayout.Toggle("Center Grid On Origin", centerGridOnOrigin);

        if (centerGridOnOrigin)
        {
            originX = -width / 2;
            originY = -height / 2;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField("Origin X", originX);
                EditorGUILayout.IntField("Origin Y", originY);
            }
        }
        else
        {
            originX = EditorGUILayout.IntField("Origin X", originX);
            originY = EditorGUILayout.IntField("Origin Y", originY);
        }

        centerGeneratedBounds = EditorGUILayout.Toggle("Center Generated Bounds", centerGeneratedBounds);
        EditorGUILayout.Space(4f);
        DrawPlacementMaskEditor();
    }

    private void DrawPlacementMaskEditor()
    {
        usePaintedPlacementArea = EditorGUILayout.Toggle("Use Painted Area", usePaintedPlacementArea);
        if (!usePaintedPlacementArea)
        {
            EditorGUILayout.HelpBox("When disabled, the generator uses the full rectangle.", MessageType.None);
            return;
        }

        int enabledCells = CountEnabledPlacementCells();
        EditorGUILayout.LabelField("Painted Cells", enabledCells.ToString());
        placementMaskZoom = EditorGUILayout.Slider("Paint Zoom", placementMaskZoom, 0.1f, 8f);
        placementMaskViewportWidth = Mathf.Clamp(EditorGUILayout.IntField("Paint View Width", placementMaskViewportWidth), 160, 3000);
        placementMaskViewportHeight = Mathf.Clamp(EditorGUILayout.IntField("Paint View Height", placementMaskViewportHeight), 160, 3000);
        using (new EditorGUI.DisabledScope(useSquarePlacementBrush))
        {
            placementBrushSize = Mathf.Clamp(EditorGUILayout.IntField("Brush Size", placementBrushSize), 1, 9);
        }

        useSquarePlacementBrush = EditorGUILayout.Toggle("Square Brush", useSquarePlacementBrush);
        using (new EditorGUI.DisabledScope(!useSquarePlacementBrush))
        {
            squarePlacementSide = Mathf.Clamp(EditorGUILayout.IntField("Square Side", squarePlacementSide), 1, Mathf.Max(width, height));
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Fill All"))
            {
                SetAllPlacementCells(true);
            }

            if (GUILayout.Button("Clear"))
            {
                SetAllPlacementCells(false);
            }

            if (GUILayout.Button("Invert"))
            {
                InvertPlacementCells();
            }
        }

        EditorGUILayout.HelpBox(useSquarePlacementBrush
            ? "Click a start cell to paint/erase a square using Square Side. The clicked cell is the lower-left corner."
            : "Click and drag to paint. Start on an enabled cell to erase, or a disabled cell to fill. Larger Brush Size paints thicker strokes.",
            MessageType.None);

        float cellSize = Mathf.Max(2f, Mathf.Round(22f * placementMaskZoom));
        const float gridPadding = 4f;
        float gridWidth = width * cellSize;
        float gridHeight = height * cellSize;
        Rect viewportRect = GUILayoutUtility.GetRect(
            placementMaskViewportWidth,
            placementMaskViewportHeight,
            GUILayout.Width(placementMaskViewportWidth),
            GUILayout.Height(placementMaskViewportHeight));
        Rect contentRect = new Rect(0f, 0f, gridWidth + gridPadding * 2f, gridHeight + gridPadding * 2f);
        Rect gridRect = new Rect(gridPadding, gridPadding, gridWidth, gridHeight);

        placementMaskScroll = GUI.BeginScrollView(viewportRect, placementMaskScroll, contentRect);
        EditorGUI.DrawRect(contentRect, PlacementMaskBackgroundColor);
        DrawPlacementMaskTexture(gridRect, cellSize);
        HandlePlacementMaskInput(gridRect, cellSize);
        GUI.EndScrollView();

        Event currentEvent = Event.current;
        if (currentEvent.type == EventType.MouseUp || currentEvent.rawType == EventType.MouseUp)
        {
            isPaintingPlacementMask = false;
            lastPaintedPlacementIndex = -1;
            lastPaintedPlacementX = -1;
            lastPaintedPlacementY = -1;
        }
    }

    private void DrawPlacementMaskTexture(Rect gridRect, float cellSize)
    {
        EnsurePlacementMaskTexture();

        if (placementMaskTexture != null)
        {
            GUI.DrawTexture(gridRect, placementMaskTexture, ScaleMode.StretchToFill, false);
        }

        if (cellSize >= 6f)
        {
            DrawPlacementMaskGridLines(gridRect, cellSize);
        }
    }

    private void DrawPlacementMaskGridLines(Rect gridRect, float cellSize)
    {
        for (int x = 0; x <= width; x++)
        {
            float lineX = gridRect.x + x * cellSize;
            EditorGUI.DrawRect(new Rect(lineX, gridRect.y, 1f, gridRect.height), PlacementMaskGridLineColor);
        }

        for (int y = 0; y <= height; y++)
        {
            float lineY = gridRect.y + y * cellSize;
            EditorGUI.DrawRect(new Rect(gridRect.x, lineY, gridRect.width, 1f), PlacementMaskGridLineColor);
        }
    }

    private void HandlePlacementMaskInput(Rect gridRect, float cellSize)
    {
        Event currentEvent = Event.current;
        if (!TryGetPlacementMaskCellAtPosition(gridRect, cellSize, currentEvent.mousePosition, out int x, out int y))
        {
            return;
        }

        int index = y * width + x;
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
        {
            placementPaintValue = !placementMask[index];
            if (useSquarePlacementBrush)
            {
                PaintPlacementSquare(x, y, placementPaintValue);
                currentEvent.Use();
                return;
            }

            isPaintingPlacementMask = true;
            PaintPlacementMaskStroke(x, y);
            currentEvent.Use();
            return;
        }

        if (!useSquarePlacementBrush && isPaintingPlacementMask && currentEvent.type == EventType.MouseDrag)
        {
            PaintPlacementMaskStroke(x, y);
            currentEvent.Use();
        }
    }

    private bool TryGetPlacementMaskCellAtPosition(Rect gridRect, float cellSize, Vector2 mousePosition, out int x, out int y)
    {
        x = -1;
        y = -1;
        if (cellSize <= 0f || !gridRect.Contains(mousePosition))
        {
            return false;
        }

        int localX = Mathf.FloorToInt((mousePosition.x - gridRect.x) / cellSize);
        int topRow = Mathf.FloorToInt((mousePosition.y - gridRect.y) / cellSize);
        if (localX < 0 || localX >= width || topRow < 0 || topRow >= height)
        {
            return false;
        }

        x = localX;
        y = height - 1 - topRow;
        return true;
    }

    private void EnsurePlacementMaskTexture()
    {
        EnsurePlacementMaskSize();
        int requiredSize = width * height;
        bool textureMissing = placementMaskTexture == null
            || placementMaskTexture.width != width
            || placementMaskTexture.height != height;

        if (textureMissing)
        {
            ReleasePlacementMaskTexture();
            placementMaskTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            placementMaskTexturePixels = new Color32[requiredSize];
            placementMaskTextureDirty = true;
        }
        else if (placementMaskTexturePixels == null || placementMaskTexturePixels.Length != requiredSize)
        {
            placementMaskTexturePixels = new Color32[requiredSize];
            placementMaskTextureDirty = true;
        }

        if (!placementMaskTextureDirty)
        {
            return;
        }

        for (int i = 0; i < requiredSize; i++)
        {
            placementMaskTexturePixels[i] = placementMask[i]
                ? PlacementMaskEnabledColor
                : PlacementMaskDisabledColor;
        }

        placementMaskTexture.SetPixels32(placementMaskTexturePixels);
        placementMaskTexture.Apply(false, false);
        placementMaskTextureDirty = false;
    }

    private void ReleasePlacementMaskTexture()
    {
        if (placementMaskTexture != null)
        {
            DestroyImmediate(placementMaskTexture);
            placementMaskTexture = null;
        }

        placementMaskTexturePixels = null;
        placementMaskTextureDirty = true;
    }

    private void PaintPlacementMaskStroke(int x, int y)
    {
        if (lastPaintedPlacementX >= 0 && lastPaintedPlacementY >= 0)
        {
            PaintPlacementLine(lastPaintedPlacementX, lastPaintedPlacementY, x, y);
        }
        else
        {
            PaintPlacementBrush(x, y);
        }

        lastPaintedPlacementX = x;
        lastPaintedPlacementY = y;
    }

    private void PaintPlacementLine(int fromX, int fromY, int toX, int toY)
    {
        int dx = Mathf.Abs(toX - fromX);
        int dy = Mathf.Abs(toY - fromY);
        int steps = Mathf.Max(dx, dy);
        if (steps <= 0)
        {
            PaintPlacementBrush(toX, toY);
            return;
        }

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            int x = Mathf.RoundToInt(Mathf.Lerp(fromX, toX, t));
            int y = Mathf.RoundToInt(Mathf.Lerp(fromY, toY, t));
            PaintPlacementBrush(x, y);
        }
    }

    private void PaintPlacementBrush(int centerX, int centerY)
    {
        int radius = Mathf.Max(0, placementBrushSize / 2);
        for (int y = centerY - radius; y <= centerY + radius; y++)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                PaintPlacementMaskCell(x, y);
            }
        }
    }

    private void PaintPlacementSquare(int startX, int startY, bool value)
    {
        for (int y = startY; y < startY + squarePlacementSide; y++)
        {
            for (int x = startX; x < startX + squarePlacementSide; x++)
            {
                PaintPlacementMaskCell(x, y, value);
            }
        }

        Repaint();
    }

    private void PaintPlacementMaskCell(int x, int y)
    {
        PaintPlacementMaskCell(x, y, placementPaintValue);
    }

    private void PaintPlacementMaskCell(int x, int y, bool value)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return;
        }

        int index = y * width + x;
        if (index == lastPaintedPlacementIndex)
        {
            return;
        }

        if (placementMask[index] == value)
        {
            lastPaintedPlacementIndex = index;
            return;
        }

        placementMask[index] = value;
        lastPaintedPlacementIndex = index;
        if (!placementEnabledCountDirty)
        {
            cachedEnabledPlacementCells += value ? 1 : -1;
        }

        placementMaskTextureDirty = true;
        Repaint();
    }

    private void DrawGenerationSection()
    {
        EditorGUILayout.LabelField("Generation", EditorStyles.boldLabel);
        fillAvailableArea = EditorGUILayout.Toggle("Fill Placement Area", fillAvailableArea);
        using (new EditorGUI.DisabledScope(fillAvailableArea))
        {
            targetArrowCount = Mathf.Max(1, EditorGUILayout.IntField("Target Arrow Count", targetArrowCount));
        }

        minSnakeLength = MakeOddAtLeast(EditorGUILayout.IntField("Min Snake Length", minSnakeLength), 3);
        maxSnakeLength = MakeOddAtLeast(EditorGUILayout.IntField("Max Snake Length", maxSnakeLength), minSnakeLength);
        maxAttemptsPerArrow = Mathf.Max(32, EditorGUILayout.IntField("Max Attempts Per Arrow", maxAttemptsPerArrow));
        bodyAttemptsPerCandidate = Mathf.Clamp(EditorGUILayout.IntField("Body Attempts", bodyAttemptsPerCandidate), 1, 64);
        using (new EditorGUI.DisabledScope(!fillAvailableArea))
        {
            requireFullFill = EditorGUILayout.Toggle("Require Full Fill", requireFullFill);
            fillSearchAttempts = Mathf.Max(maxAttemptsPerArrow, EditorGUILayout.IntField("Fill Search Attempts", fillSearchAttempts));
            fillLayoutAttempts = Mathf.Clamp(EditorGUILayout.IntField("Fill Layout Attempts", fillLayoutAttempts), 1, 64);
        }

        minDistanceBetweenSnakes = Mathf.Max(2, EditorGUILayout.IntField("Snake Spacing", minDistanceBetweenSnakes));
        minStraightCellsPerSegment = MakeOddAtLeast(EditorGUILayout.IntField("Min Cells Per Turn Segment", minStraightCellsPerSegment), 3);
        allowBentSnakes = EditorGUILayout.Toggle("Allow Bent Snakes", allowBentSnakes);
        using (new EditorGUI.DisabledScope(!allowBentSnakes))
        {
            turnChancePercent = EditorGUILayout.IntSlider("Turn Chance", turnChancePercent, 0, 100);
        }
        useMonochromeColor = EditorGUILayout.Toggle("Use Monochrome Color", useMonochromeColor);
        if (useMonochromeColor)
        {
            monochromeColor = EditorGUILayout.ColorField("Monochrome Color", monochromeColor);
        }
        useRandomSeed = EditorGUILayout.Toggle("Random Seed", useRandomSeed);

        using (new EditorGUI.DisabledScope(useRandomSeed))
        {
            seed = EditorGUILayout.IntField("Seed", seed);
        }
    }

    private void DrawOutputSection()
    {
        EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
        outputFolder = EditorGUILayout.TextField("Folder", outputFolder);
        filePrefix = EditorGUILayout.TextField("File Prefix", filePrefix);
    }

    private void DrawActions()
    {
        using (new EditorGUI.DisabledScope(!CanGenerate()))
        {
            if (GUILayout.Button("Generate Level Asset", GUILayout.Height(34f)))
            {
                GenerateAndSave();
            }
        }
    }

    private void DrawResult()
    {
        if (lastResult == null)
        {
            return;
        }

        MessageType type = lastResult.success ? MessageType.Info : MessageType.Warning;
        EditorGUILayout.HelpBox(lastResult.message, type);
        EditorGUILayout.LabelField("Placed Arrows", lastResult.placedArrowCount.ToString());
        EditorGUILayout.LabelField("Bent Arrows", lastResult.bentArrowCount.ToString());
        EditorGUILayout.LabelField("Direction Types", lastResult.directionTypeCount.ToString());
        EditorGUILayout.LabelField("Shape Types", lastResult.shapeTypeCount.ToString());
        EditorGUILayout.LabelField("Shape Mix", FormatShapeMix(lastResult));
        EditorGUILayout.LabelField("Occupied Cells", lastResult.occupiedCellCount.ToString());
        if (lastResult.placementAreaCellCount > 0)
        {
            float fillPercent = (float)lastResult.occupiedCellCount / lastResult.placementAreaCellCount * 100f;
            EditorGUILayout.LabelField("Placement Fill", $"{fillPercent:0.0}%");
        }

        if (!string.IsNullOrEmpty(lastAssetPath))
        {
            EditorGUILayout.SelectableLabel(lastAssetPath, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        }
    }

    private bool CanGenerate()
    {
        return width > 0
            && height > 0
            && targetArrowCount > 0
            && minSnakeLength > 0
            && maxSnakeLength >= minSnakeLength
            && (!usePaintedPlacementArea || CountEnabledPlacementCells() > 0)
            && !string.IsNullOrWhiteSpace(outputFolder)
            && !string.IsNullOrWhiteSpace(filePrefix);
    }

    private void GenerateAndSave()
    {
        int effectiveSeed = useRandomSeed ? unchecked((int)System.DateTime.UtcNow.Ticks) : seed;

        LevelGeneratorCore.Settings settings = new LevelGeneratorCore.Settings
        {
            width = width,
            height = height,
            targetArrowCount = targetArrowCount,
            minSnakeLength = minSnakeLength,
            maxSnakeLength = maxSnakeLength,
            maxAttemptsPerArrow = maxAttemptsPerArrow,
            bodyAttemptsPerCandidate = bodyAttemptsPerCandidate,
            minDistanceBetweenSnakes = minDistanceBetweenSnakes,
            minStraightCellsPerSegment = minStraightCellsPerSegment,
            fillAvailableArea = fillAvailableArea,
            requireFullFill = requireFullFill,
            fillSearchAttempts = fillSearchAttempts,
            fillLayoutAttempts = fillLayoutAttempts,
            allowBentSnakes = allowBentSnakes,
            turnChancePercent = turnChancePercent,
            seed = effectiveSeed,
            originX = originX,
            originY = originY,
            placementMask = usePaintedPlacementArea ? (bool[])placementMask.Clone() : null,
            useMonochromeColor = useMonochromeColor,
            monochromeColor = monochromeColor
        };

        lastResult = LevelGeneratorCore.Generate(settings);
        seed = effectiveSeed;
        if (requireFullFill && (lastResult == null || !lastResult.success))
        {
            lastAssetPath = string.Empty;
            return;
        }

        if (centerGeneratedBounds && lastResult != null)
        {
            CenterSnakesOnOrigin(lastResult.snakes);
        }

        LevelDataV2 level = CreateInstance<LevelDataV2>();
        level.levelIndex = levelIndex;
        level.gameMode = gameMode;
        level.levelDifficulty = levelDifficulty;
        level.returnToDefaultZoomAfterIntro = returnToDefaultZoomAfterIntro;
        level.timeLimit = Mathf.Max(0f, timeLimit);
        level.rewardCoins = Mathf.Max(0f, rewardCoins);
        level.rewardDiamonds = Mathf.Max(0f, rewardDiamonds);
        LevelDataV2Writer.ClearContent(level);
        LevelDataV2Writer.SetSnakes(level, lastResult.snakes);

        string folder = NormalizeAssetFolder(outputFolder);
        EnsureAssetFolder(folder);

        string safePrefix = MakeSafeFileName(filePrefix);
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{safePrefix}_{levelIndex}.asset");

        AssetDatabase.CreateAsset(level, assetPath);
        EditorUtility.SetDirty(level);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = level;
        EditorGUIUtility.PingObject(level);
        lastAssetPath = assetPath;
    }

    private static void CenterSnakesOnOrigin(List<SnakeSaveData> snakes)
    {
        if (snakes == null || snakes.Count == 0)
        {
            return;
        }

        bool hasPosition = false;
        int minX = 0;
        int maxX = 0;
        int minY = 0;
        int maxY = 0;

        for (int i = 0; i < snakes.Count; i++)
        {
            List<Vector2Int> positions = snakes[i].segmentPositions;
            if (positions == null)
            {
                continue;
            }

            for (int j = 0; j < positions.Count; j++)
            {
                Vector2Int position = positions[j];
                if (!hasPosition)
                {
                    minX = maxX = position.x;
                    minY = maxY = position.y;
                    hasPosition = true;
                    continue;
                }

                minX = Mathf.Min(minX, position.x);
                maxX = Mathf.Max(maxX, position.x);
                minY = Mathf.Min(minY, position.y);
                maxY = Mathf.Max(maxY, position.y);
            }
        }

        if (!hasPosition)
        {
            return;
        }

        int centerX = Mathf.FloorToInt((minX + maxX) * 0.5f);
        int centerY = Mathf.FloorToInt((minY + maxY) * 0.5f);
        Vector2Int offset = new Vector2Int(-centerX, -centerY);
        if (offset == Vector2Int.zero)
        {
            return;
        }

        for (int i = 0; i < snakes.Count; i++)
        {
            List<Vector2Int> positions = snakes[i].segmentPositions;
            if (positions == null)
            {
                continue;
            }

            for (int j = 0; j < positions.Count; j++)
            {
                positions[j] += offset;
            }
        }
    }

    private void EnsurePlacementMaskSize()
    {
        width = Mathf.Clamp(width, 1, MaxGridDimension);
        height = Mathf.Clamp(height, 1, MaxGridDimension);

        if (width <= 0 || height <= 0)
        {
            return;
        }

        int requiredSize = width * height;
        if (placementMask != null
            && placementMask.Length == requiredSize
            && placementMaskWidth == width
            && placementMaskHeight == height)
        {
            return;
        }

        bool[] oldMask = placementMask;
        int oldWidth = placementMaskWidth;
        int oldHeight = placementMaskHeight;
        placementMask = new bool[requiredSize];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool value = true;
                if (oldMask != null && x < oldWidth && y < oldHeight)
                {
                    value = oldMask[y * oldWidth + x];
                }

                placementMask[y * width + x] = value;
            }
        }

        placementMaskWidth = width;
        placementMaskHeight = height;
        placementMaskTextureDirty = true;
        placementEnabledCountDirty = true;
    }

    private int CountEnabledPlacementCells()
    {
        EnsurePlacementMaskSize();
        if (!placementEnabledCountDirty)
        {
            return cachedEnabledPlacementCells;
        }

        int count = 0;
        for (int i = 0; i < placementMask.Length; i++)
        {
            if (placementMask[i])
            {
                count++;
            }
        }

        cachedEnabledPlacementCells = count;
        placementEnabledCountDirty = false;
        return count;
    }

    private void SetAllPlacementCells(bool value)
    {
        EnsurePlacementMaskSize();
        for (int i = 0; i < placementMask.Length; i++)
        {
            placementMask[i] = value;
        }

        cachedEnabledPlacementCells = value ? placementMask.Length : 0;
        placementEnabledCountDirty = false;
        placementMaskTextureDirty = true;
    }

    private void InvertPlacementCells()
    {
        EnsurePlacementMaskSize();
        int enabledCount = 0;
        for (int i = 0; i < placementMask.Length; i++)
        {
            placementMask[i] = !placementMask[i];
            if (placementMask[i])
            {
                enabledCount++;
            }
        }

        cachedEnabledPlacementCells = enabledCount;
        placementEnabledCountDirty = false;
        placementMaskTextureDirty = true;
    }

    private static string NormalizeAssetFolder(string folder)
    {
        folder = folder.Replace('\\', '/').Trim();
        if (folder.EndsWith("/"))
        {
            folder = folder.Substring(0, folder.Length - 1);
        }

        return folder.StartsWith("Assets") ? folder : DefaultOutputFolder;
    }

    private static void EnsureAssetFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string[] parts = folder.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }

    private static string MakeSafeFileName(string value)
    {
        string fileName = value.Trim();
        char[] invalidChars = Path.GetInvalidFileNameChars();
        for (int i = 0; i < invalidChars.Length; i++)
        {
            fileName = fileName.Replace(invalidChars[i], '_');
        }

        return string.IsNullOrEmpty(fileName) ? "GeneratedLevel" : fileName;
    }

    private static int MakeOddAtLeast(int value, int minimum)
    {
        value = Mathf.Max(minimum, value);
        return (value & 1) == 0 ? value + 1 : value;
    }

    private static string FormatShapeMix(LevelGeneratorCore.Result result)
    {
        return $"Straight {result.straightShapeCount}, L {result.lShapeCount}, U {result.uShapeCount}, Zigzag {result.zigzagShapeCount}, Random {result.randomBentShapeCount}";
    }
}
