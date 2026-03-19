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
        LevelDataSO levelData = AssetDatabase.LoadAssetAtPath<LevelDataSO>(path);
        if (levelData == null)
        {
            levelData = ScriptableObject.CreateInstance<LevelDataSO>();
            AssetDatabase.CreateAsset(levelData, path);
        }

        // Khởi tạo lại list để tránh dồn data cũ
        levelData.snakes = new List<SnakeSaveData>();

        // Nhồi data vào
        BuildLevelData(levelData);

        // ÉP UNITY PHẢI LƯU DATA XUỐNG Ổ CỨNG
        EditorUtility.SetDirty(levelData); 
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // DÒNG LOG MỚI SẼ HIỂN THỊ SỐ LƯỢNG RẮN
        Debug.Log($"[Thành Công] Đã cập nhật file Level '{levelFileName}' với {levelData.snakes.Count} con rắn!");
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
    private void BuildLevelData(LevelDataSO level)
{
    level.snakes = new List<SnakeSaveData>();

    // 1. Vàng (yellow) - đường viền ngoài lớn nhất, hình chữ nhật lớn bao quanh
    SnakeSaveData yellowOuterFrame = CreateSnake(ArrowDir.Right, new Color(1f, 0.92f, 0.1f));
    AddPath(yellowOuterFrame,
        new Vector2Int(-12, 14), new Vector2Int(12, 14),     // ngang trên
        new Vector2Int(12, -10), new Vector2Int(-12, -10),   // ngang dưới
        new Vector2Int(-12, -10), new Vector2Int(-12, 14)    // đóng dọc trái (hướng lên)
    );
    level.snakes.Add(yellowOuterFrame);

    // 2. Tím (purple) - đường dọc bên trái + ngoặt vuông nhiều tầng phần trên
    SnakeSaveData purpleLeftComplex = CreateSnake(ArrowDir.Down, new Color(0.72f, 0.1f, 1f));
    AddPath(purpleLeftComplex,
        new Vector2Int(-9, 13), new Vector2Int(-9, 5),
        new Vector2Int(-5, 5), new Vector2Int(-5, 11),
        new Vector2Int(-1, 11), new Vector2Int(-1, 7),
        new Vector2Int(-7, 7), new Vector2Int(-7, 1)
    );
    level.snakes.Add(purpleLeftComplex);

    // 3. Xanh lá (green) - đường dọc trung tâm + nhiều ngoặt ngang
    SnakeSaveData greenCenterVertical = CreateSnake(ArrowDir.Up, new Color(0f, 0.82f, 0.22f));
    AddPath(greenCenterVertical,
        new Vector2Int(0, -9), new Vector2Int(0, 13),
        new Vector2Int(-3, 13), new Vector2Int(-3, 3),
        new Vector2Int(3, 3), new Vector2Int(3, 9),
        new Vector2Int(1, 9), new Vector2Int(1, -3)
    );
    level.snakes.Add(greenCenterVertical);

    // 4. Cam (orange) - đường ngang dài giữa + ngoặt xuống phải dưới
    SnakeSaveData orangeMidHorizontal = CreateSnake(ArrowDir.Right, new Color(1f, 0.58f, 0.05f));
    AddPath(orangeMidHorizontal,
        new Vector2Int(-11, 9), new Vector2Int(11, 9),
        new Vector2Int(11, 1), new Vector2Int(7, 1),
        new Vector2Int(7, -7), new Vector2Int(3, -7)
    );
    level.snakes.Add(orangeMidHorizontal);

    // 5. Đỏ (red) - đường dọc bên phải + ngoặt ngang ngắn dưới
    SnakeSaveData redRightVertical = CreateSnake(ArrowDir.Down, new Color(0.98f, 0.12f, 0.12f));
    AddPath(redRightVertical,
        new Vector2Int(9, 11), new Vector2Int(9, -13),
        new Vector2Int(5, -13), new Vector2Int(5, -5),
        new Vector2Int(7, -5)
    );
    level.snakes.Add(redRightVertical);

    // 6. Xanh dương (cyan/blue) - đường ngang dưới cùng + ngoặt lên trái
    SnakeSaveData cyanBottomBase = CreateSnake(ArrowDir.Left, new Color(0.05f, 0.68f, 1f));
    AddPath(cyanBottomBase,
        new Vector2Int(9, -15), new Vector2Int(-9, -15),
        new Vector2Int(-9, -7), new Vector2Int(1, -7),
        new Vector2Int(1, -11)
    );
    level.snakes.Add(cyanBottomBase);

    // 7. Hồng (pink) - đường dọc trái dưới + ngoặt ngang ngắn
    SnakeSaveData pinkLowerLeft = CreateSnake(ArrowDir.Down, new Color(1f, 0.35f, 0.75f));
    AddPath(pinkLowerLeft,
        new Vector2Int(-5, -1), new Vector2Int(-5, -11),
        new Vector2Int(-1, -11), new Vector2Int(-1, -5)
    );
    level.snakes.Add(pinkLowerLeft);

    // 8. Nâu (brown) - đường ngắn ngang phải giữa, hình chữ L ngược
    SnakeSaveData brownShortRight = CreateSnake(ArrowDir.Right, new Color(0.68f, 0.42f, 0.08f));
    AddPath(brownShortRight,
        new Vector2Int(5, 5), new Vector2Int(9, 5),
        new Vector2Int(9, -1), new Vector2Int(7, -1)
    );
    level.snakes.Add(brownShortRight);

    Debug.Log($"Đã tạo level theo ảnh grid mới - {level.snakes.Count} snakes");
}
}