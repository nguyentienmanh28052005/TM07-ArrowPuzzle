using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LevelEditor : MonoBehaviour
{
    [Header("Assets")]
    public GameObject headPrefab; // Prefab đầu rắn
    public GameObject bodyPrefab; // Prefab thân rắn

    [Header("Data")]
    public LevelDataSO currentData; // Kéo file Level muốn sửa vào đây
    public Transform levelContainer; // Object cha chứa các con rắn

    // --- TRẠNG THÁI VẼ ---
    public GameObject currentSnakeObj;
    private SnakeBlock currentSnakeScript;
    private List<Transform> currentSegments = new List<Transform>();
    private ArrowDir currentDir = ArrowDir.Up;

    private void Update()
    {
        // 1. Phím R: Xoay hướng (Chỉ tác dụng khi đang vẽ)
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentDir++;
            if ((int)currentDir > 3) currentDir = 0;
            // Debug.Log("Direction: " + currentDir);
            if (currentSnakeScript != null)
            {
                currentSnakeScript.direction = currentDir;
                currentSnakeScript.UpdateVisualRotation();
            }
        }

        // 2. Phím Space: HOÀN TẤT con rắn đang vẽ
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FinishCurrentSnake();
        }

        // 3. Chuột Trái: VẼ
        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick();
        }

        // 4. Chuột Phải: XÓA rắn
        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }

    // --- XỬ LÝ CLICK ---

    void HandleLeftClick()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y));

        // [MỚI] KIỂM TRA TRÙNG LẶP: Nếu vị trí này đã có object rồi thì không cho vẽ đè lên
        if (IsPositionOccupied(gridPos))
        {
            Debug.LogWarning("Vị trí này đã có vật thể! Không thể đặt chồng lên.");
            return;
        }

        if (currentSnakeObj == null) CreateHead(gridPos);
        else CreateBodySegment(gridPos);
    }

    // [MỚI] Hàm kiểm tra xem ô lưới có trống không
    bool IsPositionOccupied(Vector2Int pos)
    {
        Vector2 checkPos = new Vector2(pos.x, pos.y);
        // Kiểm tra va chạm tại điểm đó với Layer "Block"
        Collider2D hit = Physics2D.OverlapPoint(checkPos, LayerMask.GetMask("Block"));

        // Nếu chạm trúng thứ gì đó -> Đã bị chiếm (return true)
        return hit != null;
    }

    void HandleRightClick()
    {
        // Hủy vẽ nếu đang vẽ dở
        if (currentSnakeObj != null)
        {
            Destroy(currentSnakeObj);
            currentSnakeObj = null;
            currentSegments.Clear();
            return;
        }

        // Xóa rắn đã có trên màn hình
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos, LayerMask.GetMask("Block"));

        if (hit != null)
        {
            SnakeBlock sb = hit.GetComponentInParent<SnakeBlock>();

            if (sb != null)
            {
                Destroy(sb.gameObject);
                Debug.Log("Đã xóa rắn: " + sb.gameObject.name);
            }
        }
    }

    // --- LOGIC VẼ ---

    void CreateHead(Vector2Int pos)
    {
        currentSnakeObj = new GameObject("Snake_" + pos);
        currentSnakeObj.transform.parent = levelContainer;
        currentSnakeScript = currentSnakeObj.AddComponent<SnakeBlock>();

        GameObject headParams = Instantiate(headPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, currentSnakeObj.transform);

        currentSegments.Clear();
        currentSegments.Add(headParams.transform);

        currentSnakeScript.direction = currentDir;
        Transform arrowVis = headParams.transform.Find("Arrow");
        if (arrowVis)
        {
            float angle = 0;
            switch (currentDir)
            {
                case ArrowDir.Up: angle = 0; break;
                case ArrowDir.Down: angle = 180; break;
                case ArrowDir.Left: angle = 90; break;
                case ArrowDir.Right: angle = -90; break;
            }
            arrowVis.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void CreateBodySegment(Vector2Int pos)
    {
        Transform lastSeg = currentSegments[currentSegments.Count - 1];

        // Kiểm tra khoảng cách: Phải nằm cạnh ô trước đó
        if (Vector3.Distance(lastSeg.position, new Vector3(pos.x, pos.y, 0)) > 1.1f)
        {
            Debug.LogWarning("Phải đặt cạnh đốt trước!");
            return;
        }

        GameObject body = Instantiate(bodyPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, currentSnakeObj.transform);
        currentSegments.Add(body.transform);
    }

    void FinishCurrentSnake()
    {
        if (currentSnakeObj == null) return;

        // Gán list segment vào script để Gizmos vẽ và để lưu data
        currentSnakeScript.bodySegments = new List<Transform>(currentSegments);
        currentSnakeScript.obstacleLayer = LayerMask.GetMask("Block");

        currentSnakeObj = null;
        currentSnakeScript = null;
        currentSegments.Clear();
        Debug.Log("Đã hoàn tất rắn!");
    }

    // --- SAVE & LOAD ---

    [ContextMenu("Save Level")]
    public void SaveLevel()
    {
        if (currentData == null) return;
        currentData.snakes.Clear();

        foreach (Transform snakeParent in levelContainer)
        {
            SnakeBlock sb = snakeParent.GetComponent<SnakeBlock>();
            if (sb != null)
            {
                SnakeSaveData data = new SnakeSaveData();
                data.direction = sb.direction;

                List<Transform> segmentsToSave = sb.bodySegments;
                if (segmentsToSave == null || segmentsToSave.Count == 0)
                {
                    segmentsToSave = new List<Transform>();
                    foreach (Transform child in snakeParent) segmentsToSave.Add(child);
                }

                foreach (Transform seg in segmentsToSave)
                {
                    if (seg != null)
                        data.segmentPositions.Add(new Vector2Int(Mathf.RoundToInt(seg.position.x), Mathf.RoundToInt(seg.position.y)));
                }
                currentData.snakes.Add(data);
            }
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(currentData);
#endif
        Debug.Log("Đã lưu đè lên file Level cũ!");
    }

    [ContextMenu("Load Level To Edit")]
    public void LoadLevelToEdit()
    {
        if (currentData == null)
        {
            Debug.LogError("Chưa kéo file Level Data vào ô Current Data!");
            return;
        }

        var children = new List<GameObject>();
        foreach (Transform child in levelContainer) children.Add(child.gameObject);
        children.ForEach(child => DestroyImmediate(child));

        foreach (var data in currentData.snakes)
        {
            if (data.segmentPositions.Count == 0) continue;

            GameObject snakeObj = new GameObject("Snake_Loaded");
            snakeObj.transform.parent = levelContainer;

            SnakeBlock sb = snakeObj.AddComponent<SnakeBlock>();
            sb.obstacleLayer = LayerMask.GetMask("Block");

            List<Transform> loadedSegments = new List<Transform>();

            for (int i = 0; i < data.segmentPositions.Count; i++)
            {
                Vector2Int pos = data.segmentPositions[i];
                Vector3 worldPos = new Vector3(pos.x, pos.y, 0);

                GameObject prefab = (i == 0) ? headPrefab : bodyPrefab;
                GameObject seg = Instantiate(prefab, worldPos, Quaternion.identity, snakeObj.transform);
                loadedSegments.Add(seg.transform);
            }

            sb.bodySegments = loadedSegments;
            sb.Initialize(data.direction, loadedSegments, 9);
            sb.UpdateVisualRotation();
        }

        Debug.Log("Đã tải lại dữ liệu level.");
    }

    // --- GIZMOS: VẼ LINE ---
    private void OnDrawGizmos()
    {
        // 1. Rắn đang vẽ (Màu Đỏ)
        if (currentSegments != null && currentSegments.Count > 1)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < currentSegments.Count - 1; i++)
            {
                if (currentSegments[i] != null && currentSegments[i + 1] != null)
                {
                    Gizmos.DrawLine(currentSegments[i].position, currentSegments[i + 1].position);
                    Gizmos.DrawSphere(currentSegments[i].position, 0.1f);
                }
            }
        }

        // 2. Rắn đã xong (Màu Vàng)
        if (levelContainer != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform snake in levelContainer)
            {
                SnakeBlock sb = snake.GetComponent<SnakeBlock>();
                if (sb != null && sb.bodySegments != null && sb.bodySegments.Count > 1)
                {
                    for (int i = 0; i < sb.bodySegments.Count - 1; i++)
                    {
                        if (sb.bodySegments[i] != null && sb.bodySegments[i + 1] != null)
                        {
                            Gizmos.DrawLine(sb.bodySegments[i].position, sb.bodySegments[i + 1].position);
                            Gizmos.DrawSphere(sb.bodySegments[i].position, 0.1f);
                        }
                    }
                    if (sb.bodySegments[sb.bodySegments.Count - 1] != null)
                    {
                        Gizmos.DrawSphere(sb.bodySegments[sb.bodySegments.Count - 1].position, 0.1f);
                    }
                }
            }
        }
    }
}