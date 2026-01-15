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
            Debug.Log("Direction: " + currentDir);
            if (currentSnakeScript != null)
            {
                currentSnakeScript.direction = currentDir;
                currentSnakeScript.UpdateVisualRotation();
            }
        }

        // 2. Phím Space: HOÀN TẤT con rắn đang vẽ
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("hi");
            FinishCurrentSnake();
        }

        // 3. Chuột Trái: VẼ
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("hi");

            HandleLeftClick();
        }

        // 4. Chuột Phải: XÓA rắn (Tính năng mới để sửa level)
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("hi");

            HandleRightClick();
        }
    }

    // --- XỬ LÝ CLICK ---

    void HandleLeftClick()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y));

        if (currentSnakeObj == null) CreateHead(gridPos);
        else CreateBodySegment(gridPos);
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

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // --- SỬA ĐỔI: Dùng OverlapPoint thay vì Raycast ---
        // OverlapPoint kiểm tra xem điểm chuột có nằm TRONG một collider nào không
        Collider2D hit = Physics2D.OverlapPoint(mousePos, LayerMask.GetMask("Block"));

        if (hit != null)
        {
            // Tìm script ở cha của cái collider vừa bấm trúng
            SnakeBlock sb = hit.GetComponentInParent<SnakeBlock>();
            
            if (sb != null)
            {
                Destroy(sb.gameObject);
                Debug.Log("Đã xóa rắn: " + sb.gameObject.name);
            }
            else
            {
                Debug.Log("Trúng Collider nhưng không tìm thấy script SnakeBlock ở cha.");
            }
        }
        else
        {
            // Debug xem chuột đang bấm vào tọa độ nào
            Debug.Log($"Click trượt! Tọa độ chuột: {mousePos}. Kiểm tra lại Layer 'Block' hoặc Setting 'Queries Hit Triggers'.");
        }
    }

    // --- LOGIC VẼ (Giữ nguyên) ---

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
            // Gán tạm vào script để hàm Initialize bên dưới nó tự xoay
            // (Hoặc bạn có thể set xoay thủ công ngay tại đây)
            // Lưu ý: Code SnakeBlock mình gửi bạn cần biến arrowVisual là public hoặc SerializeField
            // Để đơn giản, ta xoay luôn ở đây:
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
        //currentSnakeScript.UpdateVisualRotation(); // Yêu cầu bạn đã sửa SnakeBlock như bài trước
    }

    void CreateBodySegment(Vector2Int pos)
    {
        Transform lastSeg = currentSegments[currentSegments.Count - 1];
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
        currentSnakeScript.bodySegments = new List<Transform>(currentSegments);
        currentSnakeScript.obstacleLayer = LayerMask.GetMask("Block"); 
        
        currentSnakeObj = null;
        currentSnakeScript = null;
        currentSegments.Clear();
        Debug.Log("Đã xong 1 con rắn!");
    }

    // --- SAVE & LOAD (QUAN TRỌNG ĐỂ SỬA LEVEL) ---

    [ContextMenu("Save Level")]
    public void SaveLevel()
    {
        if (currentData == null) return;
        currentData.snakes.Clear(); // Xóa dữ liệu cũ trong file

        foreach (Transform snakeParent in levelContainer)
        {
            SnakeBlock sb = snakeParent.GetComponent<SnakeBlock>();
            if (sb != null)
            {
                SnakeSaveData data = new SnakeSaveData();
                data.direction = sb.direction;
                foreach (Transform seg in sb.bodySegments)
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
        // 1. Kiểm tra xem đã kéo file data vào chưa
        if (currentData == null) 
        {
            Debug.LogError("Chưa kéo file Level Data vào ô Current Data!");
            return;
        }
        
        // 2. Xóa sạch các object cũ đang có trên màn hình (để tránh bị chồng lấn)
        // Dùng vòng lặp ngược hoặc tạo list tạm để xóa an toàn trong Editor
        var children = new List<GameObject>();
        foreach (Transform child in levelContainer) children.Add(child.gameObject);
        children.ForEach(child => DestroyImmediate(child)); // DestroyImmediate dùng cho chế độ Edit

        // 3. Bắt đầu đọc dữ liệu từ file và tái tạo
        foreach (var data in currentData.snakes)
        {
            // Bỏ qua nếu dữ liệu rác (không có đốt nào)
            if (data.segmentPositions.Count == 0) continue;

            // A. Tạo Object cha (Vỏ)
            GameObject snakeObj = new GameObject("Snake_Loaded");
            snakeObj.transform.parent = levelContainer;
            
            // B. Gắn script SnakeBlock
            // (Lưu ý: Hàm Awake() của SnakeBlock sẽ chạy ngay khi AddComponent, 
            // nên LineRenderer sẽ được tự động setup ở bước này)
            SnakeBlock sb = snakeObj.AddComponent<SnakeBlock>();
            
            // Setup layer để tí nữa bạn click chuột phải xóa nó còn nhận diện được
            sb.obstacleLayer = LayerMask.GetMask("Block"); 

            List<Transform> loadedSegments = new List<Transform>();

            // C. Sinh ra từng đốt (Đầu + Thân) đúng vị trí cũ
            for (int i = 0; i < data.segmentPositions.Count; i++)
            {
                Vector2Int pos = data.segmentPositions[i];
                Vector3 worldPos = new Vector3(pos.x, pos.y, 0);

                // Chọn prefab Đầu hoặc Thân
                GameObject prefab = (i == 0) ? headPrefab : bodyPrefab;
                
                // Sinh ra object
                GameObject seg = Instantiate(prefab, worldPos, Quaternion.identity, snakeObj.transform);
                loadedSegments.Add(seg.transform);
            }

            // D. QUAN TRỌNG: Nạp dữ liệu vào script SnakeBlock
            // Để nó biết nó dài bao nhiêu, hướng nào, và tự vẽ dây (LineRenderer)
            sb.Initialize(data.direction, loadedSegments);
            
            // E. Cập nhật lại visual mũi tên cho đúng hướng ngay lập tức
            sb.UpdateVisualRotation();
        }

        Debug.Log("Đã tải lại dữ liệu từ file: " + currentData.name);
    }
}