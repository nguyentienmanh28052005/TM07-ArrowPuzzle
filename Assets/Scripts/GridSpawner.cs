using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject objectToSpawn;   // Prefab vật thể muốn tạo (ví dụ: thanh gỗ, viên gạch)
    public Vector3 centerPoint = Vector3.zero; // Tâm của Grid
    public int cols = 5;               // Số cột
    public int rows = 5;               // Số hàng
    public float cellSize = 1.0f;      // Kích thước ô

    [Header("Debug")]
    public bool showGizmos = true;

    private void Update()
    {
        // Khi nhấn chuột trái
        if (Input.GetMouseButton(0))
        {
            SpawnObjectAtMouse();
        }
    }

    void SpawnObjectAtMouse()
    {
        // 1. Lấy vị trí chuột trong thế giới game (World Position)
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z; // Khoảng cách từ cam đến mặt phẳng 0
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePos);
        worldPoint.z = 0; // Đảm bảo luôn nằm trên mặt phẳng 2D

        // 2. Tính toán vị trí "Snap" vào lưới
        Vector3? snappedPos = GetSnappedPosition(worldPoint);

        // 3. Nếu vị trí hợp lệ (nằm trong lưới) thì tạo Object
        if (snappedPos.HasValue)
        {
            Instantiate(objectToSpawn, snappedPos.Value, Quaternion.identity);
        }
        else
        {
            Debug.Log("Chuột nằm ngoài vùng Grid!");
        }
    }

    // Hàm cốt lõi để tính toán vị trí Snap
    Vector3? GetSnappedPosition(Vector3 inputPos)
    {
        // A. Tính toán góc dưới cùng bên trái của cả Grid (như bài trước)
        float totalWidth = cols * cellSize;
        float totalHeight = rows * cellSize;
        Vector3 bottomLeft = centerPoint - new Vector3(totalWidth / 2f, totalHeight / 2f, 0);

        // B. Tính khoảng cách từ điểm click so với góc dưới trái
        Vector3 offset = inputPos - bottomLeft;

        // C. Tính chỉ số cột (Column) và hàng (Row)
        // Mathf.FloorToInt giúp làm tròn xuống để biết đang ở ô thứ mấy
        int colIndex = Mathf.FloorToInt(offset.x / cellSize);
        int rowIndex = Mathf.FloorToInt(offset.y / cellSize);

        // D. Kiểm tra xem có click ra ngoài lưới không
        if (colIndex < 0 || colIndex >= cols || rowIndex < 0 || rowIndex >= rows)
        {
            return null; // Nằm ngoài lưới
        }

        // E. Tính toạ độ tâm của ô đó
        // Công thức: Góc trái + (Chỉ số * Kích thước) + (Nửa kích thước để vào tâm)
        float centerX = bottomLeft.x + (colIndex * cellSize) + (cellSize / 2f);
        float centerY = bottomLeft.y + (rowIndex * cellSize) + (cellSize / 2f);

        return new Vector3(centerX, centerY, 0);
    }

    // Vẽ Grid để dễ nhìn (Copy từ bài trước)
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.gray;
        float totalW = cols * cellSize;
        float totalH = rows * cellSize;
        Vector3 bottomLeft = centerPoint - new Vector3(totalW / 2f, totalH / 2f, 0);

        // Vẽ ô lưới
        for (int x = 0; x <= cols; x++)
        {
            Vector3 s = bottomLeft + new Vector3(x * cellSize, 0, 0);
            Vector3 e = s + new Vector3(0, totalH, 0);
            Gizmos.DrawLine(s, e);
        }
        for (int y = 0; y <= rows; y++)
        {
            Vector3 s = bottomLeft + new Vector3(0, y * cellSize, 0);
            Vector3 e = s + new Vector3(totalW, 0, 0);
            Gizmos.DrawLine(s, e);
        }
    }
}