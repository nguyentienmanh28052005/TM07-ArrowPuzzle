using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;


#if UNITY_EDITOR
using UnityEditor;
#endif

// THÊM MỚI: Bổ sung công cụ Paint vào Enum
public enum EditorToolType { Draw, Erase, Paint }

public class LevelEditor : MonoBehaviour
{
    [Header("Assets")]
    public GameObject headPrefab;
    public GameObject bodyPrefab;

    [Header("Data")]
    public LevelDataSO currentData;
    public Transform levelContainer;

    [Header("Current Editor State")]
    public EditorToolType currentTool = EditorToolType.Draw;

    [SerializeField] private TextMeshProUGUI textCurrentTool;

    public ArrowDir currentDir = ArrowDir.Up;
    public Color currentColor = Color.white; 

    public Image colorPreviewImage;

    private GameObject currentSnakeObj;
    private SnakeBlock currentSnakeScript;
    private List<Transform> currentSegments = new List<Transform>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) UI_FinishSnake();
        if (Input.GetKeyDown(KeyCode.R)) 
        {
            int nextDir = (int)currentDir + 1;
            UI_SetDirection(nextDir > 3 ? 0 : nextDir);
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (currentTool == EditorToolType.Draw) HandleLeftClick();
            else if (currentTool == EditorToolType.Erase) HandleEraseClick();
            else if (currentTool == EditorToolType.Paint) HandlePaintClick(); // Gọi hàm tô màu
        }

        if (Input.GetMouseButtonDown(1))
        {
            //HandleRightClick();
        }
    }

    // ==========================================
    // CÁC HÀM DÀNH CHO NÚT BẤM UI
    // ==========================================

    public void UI_SetTool(int toolIndex)
    {
        currentTool = (EditorToolType)toolIndex;
        if (textCurrentTool != null) textCurrentTool.text = $"{currentTool} - {currentDir}";
        Debug.Log("Công cụ hiện tại: " + currentTool);
    }

    public void UI_SetDirection(int dirIndex)
    {
        currentDir = (ArrowDir)dirIndex;
        if (textCurrentTool != null) textCurrentTool.text = $"{currentTool} - {currentDir}";
        if (currentSnakeScript != null)
        {
            currentSnakeScript.direction = currentDir;
            currentSnakeScript.UpdateVisualRotation();
        }
    }

    public void UI_SetColor(Color newColor)
    {
        currentColor = newColor;
        if (colorPreviewImage != null) colorPreviewImage.color = currentColor;
        
        if (currentSnakeScript != null)
        {
            LineRenderer lr = currentSnakeScript.GetComponent<LineRenderer>();
            if (lr)
            {
                lr.startColor = currentColor;
                lr.endColor = currentColor;
            }

            SpriteRenderer[] renderers = currentSnakeScript.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in renderers) sr.color = currentColor;
        }
        Debug.Log("Màu hiện tại: " + currentColor);
    }

    public void UI_FinishSnake()
    {
        if (currentSnakeObj == null) return;

        currentSnakeScript.bodySegments = new List<Transform>(currentSegments);
        currentSnakeScript.obstacleLayer = LayerMask.GetMask("Block");

        currentSnakeObj = null;
        currentSnakeScript = null;
        currentSegments.Clear();
        Debug.Log("Đã hoàn tất rắn!");
    }

    public void UI_SaveLevel() { SaveLevel(); }
    public void UI_LoadLevel() { LoadLevelToEdit(); }

    // ==========================================
    // LOGIC VẼ, XÓA VÀ TÔ MÀU (CORE)
    // ==========================================

    private void HandleLeftClick()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y));

        if (IsPositionOccupied(gridPos))
        {
            Debug.LogWarning("Vị trí này đã có vật thể! Không thể đặt chồng lên.");
            return;
        }

        if (currentSnakeObj == null) CreateHead(gridPos);
        else CreateBodySegment(gridPos);
    }

    private void HandleEraseClick()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos, LayerMask.GetMask("Block"));

        if (hit != null)
        {
            SnakeBlock sb = hit.GetComponentInParent<SnakeBlock>();
            if (sb != null)
            {
                if (sb.gameObject == currentSnakeObj)
                {
                    currentSnakeObj = null;
                    currentSnakeScript = null;
                    currentSegments.Clear();
                }
                Destroy(sb.gameObject);
            }
        }
    }

    // THÊM MỚI: Hàm xử lý khi dùng Thùng Sơn click vào rắn cũ
    private void HandlePaintClick()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos, LayerMask.GetMask("Block"));

        if (hit != null)
        {
            SnakeBlock sb = hit.GetComponentInParent<SnakeBlock>();
            if (sb != null)
            {
                // 1. Đổi màu đường thẳng
                LineRenderer lr = sb.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    lr.startColor = currentColor;
                    lr.endColor = currentColor;
                }

                // 2. Đổi màu toàn bộ hình ảnh (Đầu mũi tên, Thân)
                SpriteRenderer[] renderers = sb.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer sr in renderers)
                {
                    sr.color = currentColor;
                }

                Debug.Log($"Đã tô màu mới cho rắn: {sb.gameObject.name}");
            }
        }
    }

    private void HandleRightClick()
    {
        if (currentSnakeObj != null)
        {
            Destroy(currentSnakeObj);
            currentSnakeObj = null;
            currentSegments.Clear();
        }
    }

    private bool IsPositionOccupied(Vector2Int pos)
    {
        Vector2 checkPos = new Vector2(pos.x, pos.y);
        Collider2D hit = Physics2D.OverlapPoint(checkPos, LayerMask.GetMask("Block"));
        return hit != null;
    }

    private void CreateHead(Vector2Int pos)
    {
        currentSnakeObj = new GameObject("Snake_" + pos);
        currentSnakeObj.transform.parent = levelContainer;
        currentSnakeScript = currentSnakeObj.AddComponent<SnakeBlock>();

        GameObject headParams = Instantiate(headPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, currentSnakeObj.transform);

        currentSegments.Clear();
        currentSegments.Add(headParams.transform);

        currentSnakeScript.direction = currentDir;
        
        LineRenderer lr = currentSnakeScript.GetComponent<LineRenderer>();
        if (lr)
        {
            lr.startColor = currentColor;
            lr.endColor = currentColor;
        }

        // Đổ màu ngay cho Head Prefab khi vừa tạo
        SpriteRenderer[] srs = headParams.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in srs) sr.color = currentColor;

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

    private void CreateBodySegment(Vector2Int pos)
    {
        Transform lastSeg = currentSegments[currentSegments.Count - 1];

        if (Vector3.Distance(lastSeg.position, new Vector3(pos.x, pos.y, 0)) > 1.1f)
        {
            Debug.LogWarning("Phải đặt cạnh đốt trước!");
            return;
        }

        GameObject body = Instantiate(bodyPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, currentSnakeObj.transform);
        currentSegments.Add(body.transform);

        // Đổ màu ngay cho Body Prefab khi vừa tạo
        SpriteRenderer[] srs = body.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in srs) sr.color = currentColor;
    }

    // ==========================================
    // LƯU VÀ TẢI DỮ LIỆU
    // ==========================================

    [ContextMenu("Save Level")]
    private void SaveLevel()
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

                LineRenderer lr = sb.GetComponent<LineRenderer>();
                if (lr != null) data.arrowColor = lr.startColor;
                else data.arrowColor = Color.white;

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
        Debug.Log("Đã lưu Level!");
    }

    [ContextMenu("Load Level To Edit")]
    private void LoadLevelToEdit()
    {
        if (currentData == null)
        {
            Debug.LogError("Chưa có Level Data!");
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
            sb.Initialize(data.direction, loadedSegments, 9, data.arrowColor);
            sb.UpdateVisualRotation();
        }
        Debug.Log("Đã tải Level.");
    }

    private void OnDrawGizmos()
    {
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
                        Gizmos.DrawSphere(sb.bodySegments[sb.bodySegments.Count - 1].position, 0.1f);
                }
            }
        }
    }
}