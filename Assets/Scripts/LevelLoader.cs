using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [Header("Data")]
    public LevelDataSO levelToPlay; // Kéo file Level_1 đã save vào đây

    [Header("Prefabs")]
    public GameObject headPrefab;   // Kéo Prefab Head vào
    public GameObject bodyPrefab;   // Kéo Prefab Body vào
    
    [Header("Container")]
    public Transform gameContainer; // Kéo 1 empty object làm cha vào

    private void Start()
    {
        LoadGame();
    }

    void LoadGame()
    {
        if (levelToPlay == null) return;

        // Xóa sạch map cũ (nếu có)
        foreach (Transform child in gameContainer) Destroy(child.gameObject);

        // Duyệt từng con rắn trong Data
        foreach (var snakeData in levelToPlay.snakes)
        {
            if (snakeData.segmentPositions.Count == 0) continue;

            // 1. Tạo Object cha chứa con rắn
            GameObject snakeObj = new GameObject("Snake");
            snakeObj.transform.parent = gameContainer;
            
            // Gắn script điều khiển
            SnakeBlock snakeScript = snakeObj.AddComponent<SnakeBlock>();
            
            // Setup layer để check va chạm
            snakeScript.obstacleLayer = LayerMask.GetMask("Block");

            List<Transform> spawnedSegments = new List<Transform>();

            // 2. Sinh ra từng đốt (Đầu + Thân)
            for (int i = 0; i < snakeData.segmentPositions.Count; i++)
            {
                Vector2Int pos = snakeData.segmentPositions[i];
                Vector3 worldPos = new Vector3(pos.x, pos.y, 0);

                GameObject prefab = (i == 0) ? headPrefab : bodyPrefab;
                GameObject segment = Instantiate(prefab, worldPos, Quaternion.identity, snakeObj.transform);
                
                spawnedSegments.Add(segment.transform);

                // Setup hướng hình ảnh cho cái ĐẦU
                if (i == 0)
                {
                    // Tìm object con tên "Arrow" để xoay visual
                    Transform arrowVis = segment.transform.Find("Arrow");
                    if (arrowVis)
                    {
                        // Gán tạm vào script để hàm Initialize bên dưới nó tự xoay
                        // (Hoặc bạn có thể set xoay thủ công ngay tại đây)
                        // Lưu ý: Code SnakeBlock mình gửi bạn cần biến arrowVisual là public hoặc SerializeField
                        // Để đơn giản, ta xoay luôn ở đây:
                        float angle = 0;
                        switch (snakeData.direction)
                        {
                            case ArrowDir.Up: angle = 0; break;
                            case ArrowDir.Down: angle = 180; break;
                            case ArrowDir.Left: angle = 90; break;
                            case ArrowDir.Right: angle = -90; break;
                        }
                        arrowVis.localRotation = Quaternion.Euler(0, 0, angle);
                    }
                }
            }

            // 3. Nạp dữ liệu vào script để Rắn sống dậy
            snakeScript.Initialize(snakeData.direction, spawnedSegments);
        }
    }
}