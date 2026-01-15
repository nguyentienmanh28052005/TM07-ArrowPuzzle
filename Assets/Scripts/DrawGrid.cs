using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawGrid : MonoBehaviour
{
    public Vector3 origin = Vector3.zero; // Điểm gốc
    public int width = 10;                // Số lượng cột (chiều ngang)
    public int height = 20;               // Số lượng hàng (chiều dọc)
    public float cellSize = 1.0f;         // Kích thước mỗi ô
    public Color gridColor = Color.green; // Màu sắc

    private void OnDrawGizmos()
    {
        Gizmos.color = gridColor;
        DrawGridCentered(origin, width, height, cellSize);
    }

    // Hàm vẽ Grid bạn yêu cầu
    public void DrawGridCentered(Vector3 center, int width, int height, float size)
    {
        // 1. Tính kích thước tổng thể
        float totalWidth = width * size;
        float totalHeight = height * size;

        // 2. Tìm điểm góc dưới cùng bên trái (Bottom-Left) từ tâm
        // Ta lùi lại 1/2 chiều rộng (trục X) và 1/2 chiều cao (trục Y)
        Vector3 bottomLeft = center - new Vector3(totalWidth / 2f, totalHeight / 2f, 0);

        // 3. Vẽ các đường dọc
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = bottomLeft + new Vector3(x * size, 0, 0);
            Vector3 end = start + new Vector3(0, totalHeight, 0);
            Gizmos.DrawLine(start, end);
        }

        // 4. Vẽ các đường ngang
        for (int y = 0; y <= height; y++)
        {
            Vector3 start = bottomLeft + new Vector3(0, y * size, 0);
            Vector3 end = start + new Vector3(totalWidth, 0, 0);
            Gizmos.DrawLine(start, end);
        }

        // (Tuỳ chọn) Vẽ một quả cầu nhỏ để đánh dấu tâm cho dễ nhìn
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(center, size * 0.1f);
    }
}
