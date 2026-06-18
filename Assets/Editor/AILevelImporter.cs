using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AILevelImporter : EditorWindow
{
    [MenuItem("ArrowPuzzle/AI Level Importer")]
    public static void ShowWindow()
    {
        GetWindow<AILevelImporter>("AI Level Importer");
    }

    private string levelFileName = "Level_AI_Generated";

    private void OnGUI()
    {
        GUILayout.Label("AI Level Generation Tool", EditorStyles.boldLabel);
        
        levelFileName = EditorGUILayout.TextField("Tên file Level:", levelFileName);

        if (GUILayout.Button("Tạo Level Data Từ AI", GUILayout.Height(40)))
        {
            GenerateLevel();
        }
    }

    private void GenerateLevel()
    {
        string path = $"Assets/Resources/Levels/{levelFileName}.asset"; 
        System.IO.Directory.CreateDirectory(Application.dataPath + "/Resources/Levels");

        // Tải file cũ lên nếu có, không có thì tạo mới
        LevelDataV2 levelData = AssetDatabase.LoadAssetAtPath<LevelDataV2>(path);
        if (levelData == null)
        {
            levelData = ScriptableObject.CreateInstance<LevelDataV2>();
            AssetDatabase.CreateAsset(levelData, path);
        }

        // Khởi tạo lại list để tránh dồn data cũ
        LevelDataV2Writer.ClearContent(levelData);

        // Nhồi data vào
        BuildLevelData(levelData);

        // ÉP UNITY PHẢI LƯU DATA XUỐNG Ổ CỨNG
        EditorUtility.SetDirty(levelData); 
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // DÒNG LOG MỚI SẼ HIỂN THỊ SỐ LƯỢNG RẮN
        Debug.Log($"[Thành Công] Đã cập nhật file Level '{levelFileName}' với {LevelDataV2Queries.GetArrowCount(levelData)} con rắn!");
        Selection.activeObject = levelData;
    }

    private SnakeSaveData CreateSnake(ArrowDir dir, Color color, params Vector2Int[] points)
    {
        SnakeSaveData snake = new SnakeSaveData();
        snake.direction = dir;
        snake.arrowColor = color;
        snake.segmentPositions = new List<Vector2Int>(points);
        return snake;
    }

    private void AddPath(SnakeSaveData snake, params Vector2Int[] corners)
    {
        snake.segmentPositions = new List<Vector2Int>();
        for (int i = 0; i < corners.Length - 1; i++)
        {
            Vector2Int start = corners[i];
            Vector2Int end = corners[i + 1];

            Vector2Int dir = new Vector2Int(
                System.Math.Sign(end.x - start.x),
                System.Math.Sign(end.y - start.y)
            );

            Vector2Int current = start;
            while (current != end)
            {
                if (!snake.segmentPositions.Contains(current))
                    snake.segmentPositions.Add(current);
                current += dir;
            }
        }
        if (corners.Length > 0 && !snake.segmentPositions.Contains(corners[corners.Length - 1]))
        {
            snake.segmentPositions.Add(corners[corners.Length - 1]);
        }
    }

    // ==========================================
    // DATA LEVEL ĐÃ ĐƯỢC AI DỊCH
    // ==========================================
    private void BuildLevelData(LevelDataV2 level)
    {
        // ==========================================
        // KHU VỰC TRUNG TÂM (Lấy điểm tròn xanh làm mốc 0,0)
        // ==========================================
        
        // 1. Rắn Đỏ (Đường thẳng đứng khu vực trung tâm phải)
        SnakeSaveData redVertical = CreateSnake(ArrowDir.Up, Color.red);
        AddPath(redVertical, new Vector2Int(5, -2), new Vector2Int(5, 4));
        LevelDataV2Writer.AddSnake(level, redVertical);

        // 2. Rắn Cam (Cụm xoắn trung tâm)
        SnakeSaveData orangeCenter = CreateSnake(ArrowDir.Left, new Color(1f, 0.5f, 0f));
        AddPath(orangeCenter, new Vector2Int(4, -1), new Vector2Int(4, -3), new Vector2Int(1, -3), new Vector2Int(1, -1), new Vector2Int(-2, -1));
        LevelDataV2Writer.AddSnake(level, orangeCenter);

        // 3. Rắn Hồng (Chạy ngang qua tâm 0,0)
        SnakeSaveData pinkMid = CreateSnake(ArrowDir.Right, Color.magenta);
        AddPath(pinkMid, new Vector2Int(-6, -2), new Vector2Int(2, -2), new Vector2Int(2, 2), new Vector2Int(3, 2));
        LevelDataV2Writer.AddSnake(level, pinkMid);

        // ==========================================
        // KHU VỰC VIỀN NGOÀI (Định hình khung bóng đèn)
        // ==========================================

        // 4. Rắn Cyan (Xanh lơ - Cạnh ngoài cùng bên trái)
        SnakeSaveData cyanLeftEdge = CreateSnake(ArrowDir.Down, Color.cyan);
        AddPath(cyanLeftEdge, new Vector2Int(-12, 12), new Vector2Int(-12, -1));
        LevelDataV2Writer.AddSnake(level, cyanLeftEdge);

        // 5. Rắn Tím Magenta (Cạnh thẳng đứng khu vực phải)
        SnakeSaveData magentaRightEdge = CreateSnake(ArrowDir.Up, Color.magenta);
        AddPath(magentaRightEdge, new Vector2Int(10, -2), new Vector2Int(10, 6));
        LevelDataV2Writer.AddSnake(level, magentaRightEdge);

        // 6. Rắn Vàng (Đỉnh đầu bóng đèn)
        SnakeSaveData yellowTop = CreateSnake(ArrowDir.Right, Color.yellow);
        AddPath(yellowTop, new Vector2Int(-1, 11), new Vector2Int(6, 11));
        LevelDataV2Writer.AddSnake(level, yellowTop);

        // 7. Rắn Xanh Lá (Viền phải bo góc)
        SnakeSaveData greenRightEdge = CreateSnake(ArrowDir.Left, Color.green);
        AddPath(greenRightEdge, new Vector2Int(10, -5), new Vector2Int(11, -5), new Vector2Int(11, 2), new Vector2Int(8, 2));
        LevelDataV2Writer.AddSnake(level, greenRightEdge);

        // ==========================================
        // KHU VỰC CHUÔI ĐÈN (Đáy bản đồ)
        // ==========================================

        // 8. Rắn Nâu (Lõi của chuôi đèn)
        SnakeSaveData brownBottom = CreateSnake(ArrowDir.Right, new Color(0.6f, 0.3f, 0f));
        AddPath(brownBottom, new Vector2Int(-3, -15), new Vector2Int(-3, -17), new Vector2Int(2, -17), new Vector2Int(2, -15));
        LevelDataV2Writer.AddSnake(level, brownBottom);

        // 9. Rắn Hồng (Nét ngang dưới cùng)
        SnakeSaveData pinkBottomEdge = CreateSnake(ArrowDir.Right, Color.magenta);
        AddPath(pinkBottomEdge, new Vector2Int(-1, -18), new Vector2Int(2, -18));
        LevelDataV2Writer.AddSnake(level, pinkBottomEdge);

        // 10. Rắn Đỏ (Chữ U ngược nhỏ ở chuôi)
        SnakeSaveData redBottomSmall = CreateSnake(ArrowDir.Left, Color.red);
        AddPath(redBottomSmall, new Vector2Int(6, -12), new Vector2Int(6, -14), new Vector2Int(3, -14));
        LevelDataV2Writer.AddSnake(level, redBottomSmall);

        // 11. Rắn Xanh Lá Lợt (Bo ngoài chuôi đèn trái)
        SnakeSaveData lightGreenBottom = CreateSnake(ArrowDir.Down, Color.green);
        AddPath(lightGreenBottom, new Vector2Int(-4, -11), new Vector2Int(-4, -16));
        LevelDataV2Writer.AddSnake(level, lightGreenBottom);
    }
}
