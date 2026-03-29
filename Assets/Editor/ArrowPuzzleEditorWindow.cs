using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public enum EditorToolType { Draw, Erase, Paint }

public class ArrowPuzzleEditorWindow : EditorWindow
{
    // ==========================================
    // CÁC BIẾN CẤU HÌNH 
    // ==========================================
    private GameObject headPrefab;
    private GameObject bodyPrefab;
    private LevelDataSO currentData;
    private Transform levelContainer;

    private EditorToolType currentTool = EditorToolType.Draw;
    private ArrowDir currentDir = ArrowDir.Up;
    private Color currentColor = Color.cyan;

    private GameObject currentSnakeObj;
    private SnakeBlock currentSnakeScript;
    private List<Transform> currentSegments = new List<Transform>();

    // ==========================================
    // HỆ THỐNG BẢNG MÀU TÙY CHỈNH (CUSTOM PALETTE)
    // ==========================================
    private Color[] myPalette = new Color[16];
    private bool isEditingPalette = false; 

    [MenuItem("Tools/Arrow Puzzle Editor")]
    public static void ShowWindow()
    {
        GetWindow<ArrowPuzzleEditorWindow>("Level Editor");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        LoadPalette(); 
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    // ==========================================
    // VẼ GIAO DIỆN (UI) TRONG TAB EDITOR
    // ==========================================
    private void OnGUI()
    {
        GUILayout.Label("ARROW PUZZLE - LEVEL MAKER", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 1. Vùng kéo thả Reference
        GUILayout.Label("References", EditorStyles.boldLabel);
        currentData = (LevelDataSO)EditorGUILayout.ObjectField("Level Data SO", currentData, typeof(LevelDataSO), false);
        levelContainer = (Transform)EditorGUILayout.ObjectField("Level Container", levelContainer, typeof(Transform), true);
        headPrefab = (GameObject)EditorGUILayout.ObjectField("Head Prefab", headPrefab, typeof(GameObject), false);
        bodyPrefab = (GameObject)EditorGUILayout.ObjectField("Body Prefab", bodyPrefab, typeof(GameObject), false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // 2. Bảng Công cụ (Tools)
        GUILayout.Label("Tools", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Toggle(currentTool == EditorToolType.Draw, "DRAW", "Button")) currentTool = EditorToolType.Draw;
        if (GUILayout.Toggle(currentTool == EditorToolType.Erase, "ERASE", "Button")) currentTool = EditorToolType.Erase;
        if (GUILayout.Toggle(currentTool == EditorToolType.Paint, "PAINT", "Button")) currentTool = EditorToolType.Paint;
        GUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // ==========================================
        // 3. VÙNG Ô CHỌN MÀU (CÁC NÚT VUÔNG)
        // ==========================================
        currentDir = (ArrowDir)EditorGUILayout.EnumPopup("Arrow Direction", currentDir);
        
        EditorGUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Snake Color Palette", EditorStyles.boldLabel);
        
        if (isEditingPalette) GUI.backgroundColor = Color.green;
        if (GUILayout.Button(isEditingPalette ? "Xong (Lưu Bảng Màu)" : "⚙ Cài đặt Bảng Màu", GUILayout.Width(150)))
        {
            isEditingPalette = !isEditingPalette;
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        
        int cols = 8; 
        GUILayout.BeginVertical("box"); 
        for (int row = 0; row < myPalette.Length; row += cols)
        {
            GUILayout.BeginHorizontal();
            for (int col = 0; col < cols; col++)
            {
                int index = row + col;
                if (index < myPalette.Length)
                {
                    if (isEditingPalette)
                    {
                        Color newCol = EditorGUILayout.ColorField(GUIContent.none, myPalette[index], false, false, false, GUILayout.Width(35), GUILayout.Height(35));
                        if (newCol != myPalette[index])
                        {
                            myPalette[index] = newCol;
                            SavePalette(); 
                        }
                    }
                    else
                    {
                        Rect rect = GUILayoutUtility.GetRect(35, 35, GUILayout.ExpandWidth(false));
                        EditorGUI.DrawRect(rect, myPalette[index]);

                        if (currentColor == myPalette[index])
                        {
                            DrawOutline(rect, Color.white, 3);
                            DrawOutline(new Rect(rect.x+3, rect.y+3, rect.width-6, rect.height-6), Color.black, 1);
                        }

                        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                        {
                            currentColor = myPalette[index];
                            UpdateCurrentSnakeColor(); 
                            Event.current.Use();
                            Repaint(); 
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2); 
        }
        GUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // 4. Các Nút Hành Động
        if (GUILayout.Button("FINISH SNAKE (Space)", GUILayout.Height(30))) FinishSnake();
        if (GUILayout.Button("UNDO LAST SEGMENT (Z)", GUILayout.Height(30))) UndoLastSegment();

        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("LOAD LEVEL", GUILayout.Height(40))) LoadLevel();
        if (GUILayout.Button("SAVE LEVEL", GUILayout.Height(40))) SaveLevel();
        GUILayout.EndHorizontal();
    }

    private void SavePalette()
    {
        for (int i = 0; i < myPalette.Length; i++)
            EditorPrefs.SetString("ArrowPuzzle_Palette_" + i, ColorUtility.ToHtmlStringRGBA(myPalette[i]));
    }

    private void LoadPalette()
    {
        Color[] defaultColors = { Color.cyan, Color.magenta, Color.yellow, Color.red, Color.green, Color.blue, new Color(1f, 0.5f, 0f), new Color(0.6f, 0.2f, 0.8f), Color.gray, Color.white, Color.black, new Color(0.6f, 0.4f, 0.2f), new Color(1f, 0.8f, 0.9f), new Color(0.2f, 0.6f, 0.4f), new Color(0.8f, 0.8f, 0.2f), new Color(0.4f, 0.4f, 1f) };

        for (int i = 0; i < myPalette.Length; i++)
        {
            string hex = EditorPrefs.GetString("ArrowPuzzle_Palette_" + i, "");
            if (!string.IsNullOrEmpty(hex) && ColorUtility.TryParseHtmlString("#" + hex, out Color col))
                myPalette[i] = col;
            else
                myPalette[i] = i < defaultColors.Length ? defaultColors[i] : Color.white;
        }
    }

    private void DrawOutline(Rect rect, Color color, int thickness)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), color);
    }

    // ==========================================
    // LOGIC VẼ VÀ KIỂM TRA
    // ==========================================
    private void OnSceneGUI(SceneView sceneView)
    {
        if (levelContainer == null) return;

        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Vector3 worldPos = ray.origin;
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));

        DrawGhostCursor(gridPos);

        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        if (e.type == EventType.Layout) HandleUtility.AddDefaultControl(controlID);

        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
        {
            if (currentTool == EditorToolType.Draw) HandleDraw(gridPos);
            else if (currentTool == EditorToolType.Erase) HandleErase(worldPos);
            else if (currentTool == EditorToolType.Paint) HandlePaint(worldPos);
            
            e.Use(); 
        }

        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Space) { FinishSnake(); e.Use(); }
            if (e.keyCode == KeyCode.Z) { UndoLastSegment(); e.Use(); }
            if (e.keyCode == KeyCode.R) 
            { 
                int nextDir = (int)currentDir + 1;
                currentDir = (ArrowDir)(nextDir > 3 ? 0 : nextDir);
                UpdateCurrentSnakeColor(); 
                Repaint(); 
                e.Use(); 
            }
        }
    }

    private bool IsPositionOccupied(Vector2Int pos)
    {
        foreach (Transform snake in levelContainer)
        {
            foreach (Transform segment in snake)
                if (Mathf.RoundToInt(segment.position.x) == pos.x && Mathf.RoundToInt(segment.position.y) == pos.y)
                    return true;
        }
        return false;
    }

    private void HandleDraw(Vector2Int gridPos)
    {
        if (IsPositionOccupied(gridPos)) return;

        if (currentSnakeObj == null) CreateHead(gridPos);
        else
        {
            Transform lastSeg = currentSegments[currentSegments.Count - 1];
            Vector2Int lastPos = new Vector2Int(Mathf.RoundToInt(lastSeg.position.x), Mathf.RoundToInt(lastSeg.position.y));
            int manhattanDist = Mathf.Abs(gridPos.x - lastPos.x) + Mathf.Abs(gridPos.y - lastPos.y);
            if (manhattanDist == 1) CreateBodySegment(gridPos);
        }
    }

    private void CreateHead(Vector2Int pos)
    {
        currentSnakeObj = new GameObject("Snake_" + pos);
        currentSnakeObj.transform.parent = levelContainer;
        Undo.RegisterCreatedObjectUndo(currentSnakeObj, "Create Snake Head");

        currentSnakeScript = currentSnakeObj.AddComponent<SnakeBlock>();
        currentSnakeScript.direction = currentDir;
        
        // ĐÃ XÓA DÒNG OBSTACLE LAYER VẬT LÝ Ở ĐÂY

        GameObject headObj = (GameObject)PrefabUtility.InstantiatePrefab(headPrefab, currentSnakeObj.transform);
        headObj.transform.position = new Vector3(pos.x, pos.y, 0);
        
        currentSegments.Clear();
        currentSegments.Add(headObj.transform);
        UpdateCurrentSnakeColor();
    }

    private void CreateBodySegment(Vector2Int pos)
    {
        GameObject bodyObj = (GameObject)PrefabUtility.InstantiatePrefab(bodyPrefab, currentSnakeObj.transform);
        bodyObj.transform.position = new Vector3(pos.x, pos.y, 0);
        Undo.RegisterCreatedObjectUndo(bodyObj, "Create Snake Body");
        currentSegments.Add(bodyObj.transform);
        UpdateCurrentSnakeColor();
    }

    private void FinishSnake()
    {
        if (currentSnakeObj == null) return;
        currentSnakeScript.bodySegments = new List<Transform>(currentSegments);
        currentSnakeObj = null;
        currentSnakeScript = null;
        currentSegments.Clear();
        Debug.Log("Chốt rắn thành công!");
    }

    private void UndoLastSegment()
    {
        if (currentSnakeObj != null && currentSegments.Count > 0)
        {
            int lastIndex = currentSegments.Count - 1;
            Transform lastSeg = currentSegments[lastIndex];
            currentSegments.RemoveAt(lastIndex);
            Undo.DestroyObjectImmediate(lastSeg.gameObject);

            if (currentSegments.Count == 0)
            {
                Undo.DestroyObjectImmediate(currentSnakeObj);
                currentSnakeObj = null;
                currentSnakeScript = null;
            }
        }
    }

    private SnakeBlock GetSnakeAtPosition(Vector3 worldPos)
    {
        Vector2Int clickPos = new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
        foreach (Transform snake in levelContainer)
        {
            foreach (Transform segment in snake)
                if (Mathf.RoundToInt(segment.position.x) == clickPos.x && Mathf.RoundToInt(segment.position.y) == clickPos.y)
                    return snake.GetComponent<SnakeBlock>();
        }
        return null;
    }

    private void HandleErase(Vector3 worldPos)
    {
        SnakeBlock sb = GetSnakeAtPosition(worldPos);
        if (sb != null)
        {
            if (sb.gameObject == currentSnakeObj)
            {
                currentSnakeObj = null;
                currentSnakeScript = null;
                currentSegments.Clear();
            }
            Undo.DestroyObjectImmediate(sb.gameObject);
        }
    }

    private void HandlePaint(Vector3 worldPos)
    {
        SnakeBlock sb = GetSnakeAtPosition(worldPos);
        if (sb != null)
        {
            Undo.RecordObject(sb, "Change Snake Color");
            sb.snakeColor = currentColor;
            LineRenderer lr = sb.GetComponent<LineRenderer>();
            if (lr) { lr.startColor = currentColor; lr.endColor = currentColor; }
            SpriteRenderer[] renderers = sb.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in renderers) sr.color = currentColor;
            EditorUtility.SetDirty(sb); 
        }
    }

    private void UpdateCurrentSnakeColor()
    {
        if (currentSnakeScript == null) return;
        currentSnakeScript.direction = currentDir;
        currentSnakeScript.snakeColor = currentColor;
        LineRenderer lr = currentSnakeScript.GetComponent<LineRenderer>();
        if (lr) { lr.startColor = currentColor; lr.endColor = currentColor; }
        SpriteRenderer[] renderers = currentSnakeScript.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers) sr.color = currentColor;

        if (currentSegments.Count > 0)
        {
            Transform arrowVis = currentSegments[0].Find("Arrow");
            if (arrowVis)
            {
                float angle = currentDir switch { ArrowDir.Up => 0, ArrowDir.Down => 180, ArrowDir.Left => 90, ArrowDir.Right => -90, _ => 0 };
                arrowVis.localRotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    private void DrawGhostCursor(Vector2Int gridPos)
    {
        Color cursorColor = currentColor;
        if (currentTool == EditorToolType.Erase) cursorColor = Color.red;
        if (IsPositionOccupied(gridPos) && currentTool == EditorToolType.Draw) cursorColor = Color.red;

        Handles.color = new Color(cursorColor.r, cursorColor.g, cursorColor.b, 0.4f);
        Handles.DrawSolidRectangleWithOutline(
            new Rect(gridPos.x - 0.5f, gridPos.y - 0.5f, 1f, 1f), 
            new Color(cursorColor.r, cursorColor.g, cursorColor.b, 0.2f), 
            cursorColor
        );
        SceneView.RepaintAll();
    }

    // ==========================================
    // SAVE & LOAD
    // ==========================================
    private void SaveLevel()
    {
        if (currentData == null) { Debug.LogError("Chưa gắn LevelDataSO!"); return; }
        Undo.RecordObject(currentData, "Save Level Data");
        currentData.snakes.Clear();

        foreach (Transform snakeParent in levelContainer)
        {
            SnakeBlock sb = snakeParent.GetComponent<SnakeBlock>();
            if (sb != null)
            {
                SnakeSaveData data = new SnakeSaveData();
                data.direction = sb.direction;
                data.arrowColor = sb.snakeColor;

                foreach (Transform seg in snakeParent)
                {
                    if (seg != null && seg.name.Contains("Unit")) 
                        data.segmentPositions.Add(new Vector2Int(Mathf.RoundToInt(seg.position.x), Mathf.RoundToInt(seg.position.y)));
                }
                currentData.snakes.Add(data);
            }
        }

        EditorUtility.SetDirty(currentData);
        AssetDatabase.SaveAssets(); 
        Debug.Log("Lưu Level thành công!");
    }

    private void LoadLevel()
    {
        if (currentData == null || levelContainer == null) return;
        for (int i = levelContainer.childCount - 1; i >= 0; i--) Undo.DestroyObjectImmediate(levelContainer.GetChild(i).gameObject);

        foreach (var data in currentData.snakes)
        {
            if (data.segmentPositions.Count == 0) continue;
            GameObject snakeObj = new GameObject("Snake_Loaded");
            snakeObj.transform.parent = levelContainer;
            Undo.RegisterCreatedObjectUndo(snakeObj, "Load Snake");

            SnakeBlock sb = snakeObj.AddComponent<SnakeBlock>();
            sb.direction = data.direction;
            sb.snakeColor = data.arrowColor;
            
            // ĐÃ XÓA DÒNG OBSTACLE LAYER VẬT LÝ Ở ĐÂY NỮA

            List<Transform> loadedSegments = new List<Transform>();
            for (int i = 0; i < data.segmentPositions.Count; i++)
            {
                Vector2Int pos = data.segmentPositions[i];
                GameObject prefab = (i == 0) ? headPrefab : bodyPrefab;
                GameObject seg = (GameObject)PrefabUtility.InstantiatePrefab(prefab, snakeObj.transform);
                seg.transform.position = new Vector3(pos.x, pos.y, 0);
                loadedSegments.Add(seg.transform);
                
                SpriteRenderer[] renderers = seg.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer sr in renderers) sr.color = data.arrowColor;
            }
            sb.bodySegments = loadedSegments;
            
            Transform arrowVis = loadedSegments[0].Find("Arrow");
            if (arrowVis)
            {
                float angle = data.direction switch { ArrowDir.Up => 0, ArrowDir.Down => 180, ArrowDir.Left => 90, ArrowDir.Right => -90, _ => 0 };
                arrowVis.localRotation = Quaternion.Euler(0, 0, angle);
            }
        }
        Debug.Log("Tải Level thành công!");
    }
}