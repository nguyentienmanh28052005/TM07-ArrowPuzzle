using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject objectToSpawn;
    public Vector3 centerPoint = Vector3.zero;
    public int cols = 5;
    public int rows = 5;
    public float cellSize = 1.0f;

    [Header("Debug")]
    public bool showGizmos = true;

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            SpawnObjectAtMouse();
        }
    }

    void SpawnObjectAtMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePos);
        worldPoint.z = 0;

        Vector3? snappedPos = GetSnappedPosition(worldPoint);

        if (snappedPos.HasValue)
        {
            Instantiate(objectToSpawn, snappedPos.Value, Quaternion.identity);
        }
        else
        {
            Debug.Log("Chuột nằm ngoài vùng Grid!");
        }
    }

    Vector3? GetSnappedPosition(Vector3 inputPos)
    {
        float totalWidth = cols * cellSize;
        float totalHeight = rows * cellSize;
        Vector3 bottomLeft = centerPoint - new Vector3(totalWidth / 2f, totalHeight / 2f, 0);

        Vector3 offset = inputPos - bottomLeft;

        int colIndex = Mathf.FloorToInt(offset.x / cellSize);
        int rowIndex = Mathf.FloorToInt(offset.y / cellSize);

        if (colIndex < 0 || colIndex >= cols || rowIndex < 0 || rowIndex >= rows)
        {
            return null;
        }

        float centerX = bottomLeft.x + (colIndex * cellSize) + (cellSize / 2f);
        float centerY = bottomLeft.y + (rowIndex * cellSize) + (cellSize / 2f);

        return new Vector3(centerX, centerY, 0);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.gray;
        float totalW = cols * cellSize;
        float totalH = rows * cellSize;
        Vector3 bottomLeft = centerPoint - new Vector3(totalW / 2f, totalH / 2f, 0);

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