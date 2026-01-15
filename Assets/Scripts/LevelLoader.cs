using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [Header("Data")]
    public LevelDataSO levelToPlay;

    [Header("Prefabs")]
    public GameObject headPrefab;   // Vẫn cần để tạo Đầu
    public GameObject bodyPrefab;   // Vẫn cần để tạo các nốt Chính (Gốc)

    [Header("Container")]
    public Transform gameContainer;

    [Header("Resolution Settings")]
    [Range(0, 20)]
    public int subNodesCount = 3; // Số lượng nốt rỗng chèn vào giữa

    private void Start()
    {
        LoadGame();
    }

    [ContextMenu("Reload Level")]
    public void LoadGame()
    {
        if (levelToPlay == null) return;

        // 1. Dọn dẹp map cũ (Xóa sạch sẽ)
        if (gameContainer != null)
        {
            int childCount = gameContainer.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(gameContainer.GetChild(i).gameObject);
            }
        }

        // 2. Duyệt từng con rắn
        foreach (var snakeData in levelToPlay.snakes)
        {
            if (snakeData.segmentPositions.Count == 0) continue;

            // Tạo vỏ bọc cha
            GameObject snakeObj = new GameObject("Snake");
            if (gameContainer != null) snakeObj.transform.parent = gameContainer;

            SnakeBlock snakeScript = snakeObj.AddComponent<SnakeBlock>();
            snakeScript.obstacleLayer = LayerMask.GetMask("Block");

            List<Transform> allSegments = new List<Transform>();
            int dataCount = snakeData.segmentPositions.Count;

            // --- VÒNG LẶP TỐI ƯU ---
            for (int i = 0; i < dataCount; i++)
            {
                // A. SINH NỐT CHÍNH (Dùng Prefab thật)
                // Để đảm bảo logic va chạm hoặc hình ảnh mốc (nếu cần) vẫn còn
                Vector2Int pos = snakeData.segmentPositions[i];
                Vector3 currentPos = new Vector3(pos.x, pos.y, 0);

                GameObject prefab = (i == 0) ? headPrefab : bodyPrefab;
                GameObject mainSeg = Instantiate(prefab, currentPos, Quaternion.identity, snakeObj.transform);

                mainSeg.name = (i == 0) ? "Head" : $"Main_{i}";
                allSegments.Add(mainSeg.transform);

                // Xử lý xoay mũi tên cho ĐẦU
                if (i == 0)
                {
                    Transform arrowVis = mainSeg.transform.Find("Arrow");
                    if (arrowVis)
                    {
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

                // B. SINH NỐT PHỤ (Dùng Empty Object - Siêu nhẹ)
                if (i < dataCount - 1)
                {
                    Vector2Int nextPosData = snakeData.segmentPositions[i + 1];
                    Vector3 nextPos = new Vector3(nextPosData.x, nextPosData.y, 0);

                    for (int j = 1; j <= subNodesCount; j++)
                    {
                        float t = (float)j / (subNodesCount + 1);
                        Vector3 subPos = Vector3.Lerp(currentPos, nextPos, t);

                        // --- TỐI ƯU TẠI ĐÂY ---
                        // Thay vì Instantiate prefab, ta chỉ tạo một GameObject rỗng
                        GameObject subNode = new GameObject($"Sub_{i}_{j}");

                        subNode.transform.position = subPos;
                        subNode.transform.parent = snakeObj.transform;

                        allSegments.Add(subNode.transform);
                    }
                }
            }

            // 3. Nạp vào script
            snakeScript.Initialize(snakeData.direction, allSegments);
        }

        Debug.Log($"Load xong! Đã tạo các nốt rỗng (Empty Nodes) để tối ưu hiệu năng.");
    }
}