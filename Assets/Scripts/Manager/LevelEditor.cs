using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EditorToolType { Draw, Erase, Paint }

public class LevelEditor : MonoBehaviour
{
    #region [ VARIABLES & REFERENCES ]
    [Header("Assets")]
    public GameObject headPrefab;
    public GameObject bodyPrefab;

    [Header("Data")]
    public LevelDataSO currentData;
    public Transform levelContainer;

    [Header("Preview (Ghost Cursor)")]
    public SpriteRenderer previewCursor;

    [Header("Current Editor State")]
    public EditorToolType currentTool = EditorToolType.Draw;
    [SerializeField] private TextMeshProUGUI textCurrentTool;
    public ArrowDir currentDir = ArrowDir.Up;
    public Color currentColor = Color.white;
    public Image colorPreviewImage;

    private GameObject currentSnakeObj;
    private SnakeBlock currentSnakeScript;
    private List<Transform> currentSegments = new List<Transform>();

    private Stack<GameObject> finishedSnakesHistory = new Stack<GameObject>();
    #endregion

    private void Start()
    {
        LoadLevelToEdit();
    }

    #region [ MAIN LOOP ]
    /// <summary>
    /// Vòng lặp chính quản lý phím tắt và tương tác chuột trên Editor.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) UI_FinishSnake();
        if (Input.GetKeyDown(KeyCode.R)) RotateDirection();
        if (Input.GetKeyDown(KeyCode.Z)) UndoLastSegment();

        UpdatePreviewCursor();

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0)) 
        {
            if (currentTool == EditorToolType.Draw) HandleLeftClick();
            else if (currentTool == EditorToolType.Erase) HandleEraseClick();
            else if (currentTool == EditorToolType.Paint) HandlePaintClick();
        }
        else if (Input.GetMouseButton(0)) 
        {
            if (currentTool == EditorToolType.Draw) HandleLeftDrag();
            else if (currentTool == EditorToolType.Erase) HandleEraseClick();
        }
    }
    #endregion

    #region [ UI & SHORTCUT ACTIONS ]
    public void UI_SetTool(int toolIndex)
    {
        currentTool = (EditorToolType)toolIndex;
        UpdateToolText();
    }

    public void UI_SetDirection(int dirIndex)
    {
        currentDir = (ArrowDir)dirIndex;
        UpdateToolText();
        
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
            // BẢN VÁ: Dùng API đổi màu mới của hệ thống thay vì sửa LineRenderer trực tiếp
            currentSnakeScript.SetColorImmediatePublic(currentColor);
        }
    }

    /// <summary>
    /// Hoàn tất việc vẽ con rắn hiện tại và đưa vào ngăn xếp Undo.
    /// </summary>
    public void UI_FinishSnake()
    {
        if (currentSnakeObj == null) return;

        // BẢN VÁ: Gọi hàm Initialize mới của SnakeBlock để nó sinh tọa độ chuẩn
        currentSnakeScript.Initialize(currentDir, new List<Transform>(currentSegments), 9, currentColor);

        finishedSnakesHistory.Push(currentSnakeObj);

        currentSnakeObj = null;
        currentSnakeScript = null;
        currentSegments.Clear();
        Debug.Log("Đã chốt con rắn. Sẵn sàng vẽ con mới.");
    }

    private void RotateDirection()
    {
        int nextDir = (int)currentDir + 1;
        UI_SetDirection(nextDir > 3 ? 0 : nextDir);
    }

    private void UpdateToolText()
    {
        if (textCurrentTool != null) textCurrentTool.text = $"{currentTool} - {currentDir}";
    }
    #endregion

    #region [ MOUSE INTERACTIONS (PATCHED FOR MATH INSTEAD OF PHYSICS) ]
    
    // HÀM TRỢ GIÚP MỚI: Quét bằng Toán học thay vì Vật lý 2D
    private SnakeBlock GetSnakeAtGridPos(Vector2Int pos)
    {
        foreach (Transform snakeParent in levelContainer)
        {
            SnakeBlock sb = snakeParent.GetComponent<SnakeBlock>();
            if (sb != null && sb.bodySegments != null)
            {
                foreach (Transform seg in sb.bodySegments)
                {
                    if (seg != null && Mathf.RoundToInt(seg.position.x) == pos.x && Mathf.RoundToInt(seg.position.y) == pos.y)
                    {
                        return sb;
                    }
                }
            }
        }
        return null;
    }

    private bool IsPositionOccupied(Vector2Int pos)
    {
        // 1. Check con rắn đang vẽ dở (chưa finish)
        foreach (var seg in currentSegments)
        {
            if (seg != null && Mathf.RoundToInt(seg.position.x) == pos.x && Mathf.RoundToInt(seg.position.y) == pos.y) return true;
        }

        // 2. Check các con rắn đã vẽ trên Scene
        return GetSnakeAtGridPos(pos) != null;
    }

    private void HandleLeftClick()
    {
        Vector2Int gridPos = GetMouseGridPosition();

        if (IsPositionOccupied(gridPos))
        {
            Debug.LogWarning("Vị trí này đã có vật thể! Không thể đè lên.");
            return;
        }

        if (currentSnakeObj == null) 
        {
            CreateHead(gridPos);
        }
        else 
        {
            Transform lastSeg = currentSegments[currentSegments.Count - 1];
            Vector2Int lastPos = new Vector2Int(Mathf.RoundToInt(lastSeg.position.x), Mathf.RoundToInt(lastSeg.position.y));
            
            int manhattanDist = Mathf.Abs(gridPos.x - lastPos.x) + Mathf.Abs(gridPos.y - lastPos.y);

            if (manhattanDist != 1)
            {
                Debug.LogWarning("Phải đặt sát cạnh đốt trước! Không thể đi chéo hoặc nhảy cóc.");
                return; 
            }

            CreateBodySegment(gridPos);
        }
    }

    private void HandleLeftDrag()
    {
        if (currentSnakeObj == null || currentSegments.Count == 0) return;

        Vector2Int gridPos = GetMouseGridPosition();
        Transform lastSeg = currentSegments[currentSegments.Count - 1];
        Vector2Int lastPos = new Vector2Int(Mathf.RoundToInt(lastSeg.position.x), Mathf.RoundToInt(lastSeg.position.y));

        if (gridPos == lastPos) return; 

        int manhattanDist = Mathf.Abs(gridPos.x - lastPos.x) + Mathf.Abs(gridPos.y - lastPos.y);
        
        if (manhattanDist == 1 && !IsPositionOccupied(gridPos))
        {
            CreateBodySegment(gridPos);
        }
    }

    private void HandleEraseClick()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        SnakeBlock sb = GetSnakeAtGridPos(gridPos); // Quét bằng Toán học

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

    private void HandlePaintClick()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        SnakeBlock sb = GetSnakeAtGridPos(gridPos); // Quét bằng Toán học

        if (sb != null)
        {
            sb.snakeColor = currentColor;
            sb.SetColorImmediatePublic(currentColor);
        }
    }
    #endregion

    #region [ CORE CREATION & UNDO ]
    private void CreateHead(Vector2Int pos)
    {
        currentSnakeObj = new GameObject("Snake_" + pos);
        currentSnakeObj.transform.parent = levelContainer;
        currentSnakeScript = currentSnakeObj.AddComponent<SnakeBlock>();

        GameObject headParams = Instantiate(headPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, currentSnakeObj.transform);
        currentSegments.Clear();
        currentSegments.Add(headParams.transform);

        currentSnakeScript.direction = currentDir;
        currentSnakeScript.snakeColor = currentColor; // Lưu màu vào script

        LineRenderer lr = currentSnakeScript.GetComponent<LineRenderer>();
        if (lr) { lr.startColor = currentColor; lr.endColor = currentColor; }

        SpriteRenderer[] srs = headParams.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in srs) sr.color = currentColor;

        Transform arrowVis = headParams.transform.Find("Arrow");
        if (arrowVis)
        {
            float angle = currentDir switch
            {
                ArrowDir.Up => 0,
                ArrowDir.Down => 180,
                ArrowDir.Left => 90,
                ArrowDir.Right => -90,
                _ => 0
            };
            arrowVis.localRotation = Quaternion.Euler(0, 0, angle);
        }

        UpdateSnakeLinePreview();
    }

    private void CreateBodySegment(Vector2Int pos)
    {
        GameObject body = Instantiate(bodyPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, currentSnakeObj.transform);
        currentSegments.Add(body.transform);

        SpriteRenderer[] srs = body.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in srs) sr.color = currentColor;

        UpdateSnakeLinePreview();
    }

    public void UndoLastSegment()
    {
        if (currentSnakeObj != null && currentSegments.Count > 0)
        {
            int lastIndex = currentSegments.Count - 1;
            Transform lastSeg = currentSegments[lastIndex];

            currentSegments.RemoveAt(lastIndex);
            if (lastSeg != null) Destroy(lastSeg.gameObject);

            if (currentSegments.Count == 0) 
            {
                Destroy(currentSnakeObj);
                currentSnakeObj = null;
                currentSnakeScript = null;
            }
            else
            {
                UpdateSnakeLinePreview(); 
            }
        }
        else if (finishedSnakesHistory.Count > 0)
        {
            GameObject lastFinishedSnake = finishedSnakesHistory.Pop();
            if (lastFinishedSnake != null)
            {
                Destroy(lastFinishedSnake);
                Debug.Log("Đã hoàn tác (Undo) con rắn trước đó.");
            }
        }
    }
    #endregion

    #region [ VISUAL FEEDBACKS ]
    private void UpdateSnakeLinePreview()
    {
        if (currentSnakeScript != null && currentSegments.Count > 0)
        {
            LineRenderer lr = currentSnakeScript.GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.positionCount = currentSegments.Count;
                for (int i = 0; i < currentSegments.Count; i++)
                {
                    lr.SetPosition(i, currentSegments[i].position);
                }
            }
        }
    }

    private void UpdatePreviewCursor()
    {
        if (previewCursor == null) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            previewCursor.enabled = false;
            return;
        }

        Vector2Int gridPos = GetMouseGridPosition();
        previewCursor.transform.position = new Vector3(gridPos.x, gridPos.y, -1f); 

        if (currentTool == EditorToolType.Draw)
        {
            previewCursor.enabled = true;
            Color ghostColor = currentColor; ghostColor.a = 0.5f;
            Color errorColor = new Color(1f, 0f, 0f, 0.5f);

            if (IsPositionOccupied(gridPos))
            {
                previewCursor.color = errorColor;
            }
            else if (currentSnakeObj != null && currentSegments.Count > 0)
            {
                Transform lastSeg = currentSegments[currentSegments.Count - 1];
                int manhattanDist = Mathf.Abs(gridPos.x - Mathf.RoundToInt(lastSeg.position.x)) + Mathf.Abs(gridPos.y - Mathf.RoundToInt(lastSeg.position.y));
                previewCursor.color = (manhattanDist == 1) ? ghostColor : errorColor;
            }
            else
            {
                previewCursor.color = ghostColor;
            }
        }
        else if (currentTool == EditorToolType.Erase)
        {
            previewCursor.enabled = true;
            previewCursor.color = new Color(1f, 0f, 0f, 0.5f);
        }
        else if (currentTool == EditorToolType.Paint)
        {
            previewCursor.enabled = true;
            Color paintColor = currentColor; paintColor.a = 0.8f;
            previewCursor.color = paintColor;
        }
        else
        {
            previewCursor.enabled = false;
        }
    }

    private Vector2Int GetMouseGridPosition()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y));
    }
    #endregion

    #region [ SAVE & LOAD SYSTEM ]
    public void UI_SaveLevel() { SaveLevel(); }
    public void UI_LoadLevel() { LoadLevelToEdit(); }

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

                // BẢN VÁ: Lấy màu gốc từ script thay vì LineRenderer
                data.arrowColor = sb.snakeColor;

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
        Debug.Log("Đã lưu Level thành công!");
    }

    [ContextMenu("Load Level To Edit")]
    private void LoadLevelToEdit()
    {
        if (currentData == null)
        {
            Debug.LogError("Chưa có Level Data!");
            return;
        }

        for (int i = levelContainer.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(levelContainer.GetChild(i).gameObject);
        }

        finishedSnakesHistory.Clear(); 

        foreach (var data in currentData.snakes)
        {
            if (data.segmentPositions.Count == 0) continue;

            GameObject snakeObj = new GameObject("Snake_Loaded");
            snakeObj.transform.parent = levelContainer;

            SnakeBlock sb = snakeObj.AddComponent<SnakeBlock>();

            List<Transform> loadedSegments = new List<Transform>();

            for (int i = 0; i < data.segmentPositions.Count; i++)
            {
                Vector2Int pos = data.segmentPositions[i];
                GameObject prefab = (i == 0) ? headPrefab : bodyPrefab;
                GameObject seg = Instantiate(prefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, snakeObj.transform);
                loadedSegments.Add(seg.transform);
            }

            // BẢN VÁ: Gọi Initialize mới
            sb.Initialize(data.direction, loadedSegments, 9, data.arrowColor);

            finishedSnakesHistory.Push(snakeObj); 
        }
        Debug.Log("Đã tải Level.");
    }
    #endregion

    #region [ EDITOR GIZMOS & HANDLES ]
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        float lineThickness = 10f; 

        if (currentSegments != null && currentSegments.Count > 1)
        {
            Handles.color = Color.red;
            Gizmos.color = Color.red;
            for (int i = 0; i < currentSegments.Count - 1; i++)
            {
                if (currentSegments[i] != null && currentSegments[i + 1] != null)
                {
                    Handles.DrawAAPolyLine(lineThickness, currentSegments[i].position, currentSegments[i + 1].position);
                    Gizmos.DrawSphere(currentSegments[i].position, 0.1f);
                }
            }
        }

        if (levelContainer != null)
        {
            Handles.color = Color.yellow;
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
                            Handles.DrawAAPolyLine(lineThickness, sb.bodySegments[i].position, sb.bodySegments[i + 1].position);
                            Gizmos.DrawSphere(sb.bodySegments[i].position, 0.1f);
                        }
                    }
                    if (sb.bodySegments[sb.bodySegments.Count - 1] != null)
                        Gizmos.DrawSphere(sb.bodySegments[sb.bodySegments.Count - 1].position, 0.1f);
                }
            }
        }
#endif
    }
    #endregion
}