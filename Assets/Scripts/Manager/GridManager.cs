using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    // SỔ CÁI: Lưu trữ [Tọa độ Grid] -> [Thuộc về con rắn nào]
    public Dictionary<Vector2Int, SnakeBlock> GridMap = new Dictionary<Vector2Int, SnakeBlock>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Đóng dấu chủ quyền lên tất cả các ô mà thân rắn đang đè lên (Thuần Dữ Liệu)
    /// </summary>
    public void RegisterSnake(SnakeBlock snake)
    {
        if (snake == null || snake.LogicNodes == null) return;
        
        // Vòng lặp siêu nhẹ: Chỉ đọc con số, không đụng tới GameObject
        foreach (Vector3 nodePos in snake.LogicNodes)
        {
            Vector2Int pos = new Vector2Int(Mathf.RoundToInt(nodePos.x), Mathf.RoundToInt(nodePos.y));
            GridMap[pos] = snake; 
        }
    }

    /// <summary>
    /// Xóa toàn bộ dấu vết của con rắn khỏi Sổ cái
    /// </summary>
    public void UnregisterSnake(SnakeBlock snake)
    {
        if (snake == null) return;
        List<Vector2Int> keysToRemove = new List<Vector2Int>();
        foreach (var kvp in GridMap)
        {
            if (kvp.Value == snake) keysToRemove.Add(kvp.Key);
        }

        foreach (var key in keysToRemove) GridMap.Remove(key);
    }

    /// <summary>
    /// Tra cứu xem có ai đang đứng ở ô này không.
    /// </summary>
    public SnakeBlock GetSnakeAt(Vector2Int pos)
    {
        if (GridMap.TryGetValue(pos, out SnakeBlock snake)) return snake;
        return null;
    }
}