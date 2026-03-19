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
        // 1. Tạo một ScriptableObject mới
        LevelDataSO newLevel = ScriptableObject.CreateInstance<LevelDataSO>();
        newLevel.snakes = new List<SnakeSaveData>();

        // ==============================================================
        // 2. CHỖ NÀY LÀ NƠI AI (TÔI) SẼ ĐIỀN CODE VÀO CHO BẠN MỖI LẦN
        // ==============================================================
        
        BuildLevelData(newLevel);

        // ==============================================================

        // 3. Lưu thành file .asset cứng trong ổ đĩa
        string path = $"Assets/Resources/Levels/{levelFileName}.asset"; // Sửa lại đường dẫn này theo dự án của bạn
        
        // Đảm bảo thư mục tồn tại
        System.IO.Directory.CreateDirectory(Application.dataPath + "/Resources/Levels");

        AssetDatabase.CreateAsset(newLevel, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[Thành Công] Đã tạo file Level tại: {path}");
        Selection.activeObject = newLevel; // Tự động focus vào file vừa tạo
    }

    // Hàm tiện ích giúp AI viết code ngắn gọn hơn
    private SnakeSaveData CreateSnake(ArrowDir dir, Color color, params Vector2Int[] points)
    {
        SnakeSaveData snake = new SnakeSaveData();
        snake.direction = dir;
        snake.arrowColor = color;
        snake.segmentPositions = new List<Vector2Int>(points);
        return snake;
    }

    // Hàm tiện ích giúp AI nối các đường thẳng tự động (chỉ cần nhập các điểm GÓC CUA)
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
        // Thêm điểm cuối cùng
        if (corners.Length > 0 && !snake.segmentPositions.Contains(corners[corners.Length - 1]))
        {
            snake.segmentPositions.Add(corners[corners.Length - 1]);
        }
    }

    // ==========================================
    // KHU VỰC CHỨA DATA CỦA AI
    // ==========================================
    private void BuildLevelData(LevelDataSO level)
    {
        // 1. Rắn Vàng (Khung bao ngoài)
        SnakeSaveData yellow = CreateSnake(ArrowDir.Up, new Color(1f, 0.97f, 0f));
        AddPath(yellow, 
            new Vector2Int(-1,9), new Vector2Int(-1,-3), new Vector2Int(1,-3), 
            new Vector2Int(1,9), new Vector2Int(7,9), new Vector2Int(7,-9), 
            new Vector2Int(-7,-9), new Vector2Int(-7,-7)
        );
        level.snakes.Add(yellow);

        // 2. Rắn Đỏ (Góc phải)
        SnakeSaveData red = CreateSnake(ArrowDir.Right, new Color(1f, 0.21f, 0.21f));
        AddPath(red, 
            new Vector2Int(5,7), new Vector2Int(3,7), new Vector2Int(3,5), 
            new Vector2Int(5,5), new Vector2Int(5,-7), new Vector2Int(1,-7), 
            new Vector2Int(1,-5), new Vector2Int(-3,-5)
        );
        level.snakes.Add(red);

        // 3. Rắn Xanh Chuối (Giữa)
        SnakeSaveData greenLight = CreateSnake(ArrowDir.Up, new Color(0.53f, 0.72f, 0f));
        AddPath(greenLight, new Vector2Int(3,3), new Vector2Int(3,-5));
        level.snakes.Add(greenLight);

        // 4. Rắn Xanh Lá Đậm (Góc trái)
        SnakeSaveData greenDark = CreateSnake(ArrowDir.Right, new Color(0f, 0.46f, 0.2f));
        AddPath(greenDark, 
            new Vector2Int(-1,-7), new Vector2Int(-5,-7), new Vector2Int(-5,-5), 
            new Vector2Int(-7,-5), new Vector2Int(-7,9), new Vector2Int(-3,9), 
            new Vector2Int(-3,7), new Vector2Int(-5,7)
        );
        level.snakes.Add(greenDark);

        // 5. Rắn Tím (Bên trái lõi)
        SnakeSaveData purple = CreateSnake(ArrowDir.Right, new Color(0.88f, 0.11f, 1f));
        AddPath(purple, 
            new Vector2Int(-3,5), new Vector2Int(-5,5), new Vector2Int(-5,-3), new Vector2Int(-3,-3), new Vector2Int(-3,3)
        );
        level.snakes.Add(purple);
    }
}