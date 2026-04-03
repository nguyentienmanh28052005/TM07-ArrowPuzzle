using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EditorToolType { Draw, Erase, Paint, Select }

public class LevelEditor : MonoBehaviour
{
    [Header("Assets (Data-Driven)")]
    public GameObject snakePrefab;
    public GameObject selectionGlowPrefab;
    public Color highlightColor = Color.yellow;

    [Header("Data")]
    public LevelDataSO currentData;
    public Transform levelContainer;

    [Header("Level Selector UI (New)")]
    public GameObject levelButtonPrefab; 
    public Transform levelScrollContent; 
    public GameObject levelSelectorPanel; 

    [Header("Preview & Tools")]
    public SpriteRenderer previewCursor;
    public EditorToolType currentTool = EditorToolType.Draw;
    [SerializeField] private TextMeshProUGUI textCurrentTool;
    public ArrowDir currentDir = ArrowDir.Up;
    public Color currentColor = Color.white;
    public Image colorPreviewImage;

    [Header("Validation Settings")]
    public int minDistanceBetweenSnakes = 2;

    [Header("Metadata UI")]
    public TMP_InputField inputTimeLimit;
    public TMP_InputField inputRewardCoins;
    public TMP_InputField inputRewardDiamonds;

    private GameObject currentSnakeObj;
    private EditorSnakeVisual currentSnakeScript;
    private EditorSnakeVisual selectedSnakeToModify;
    private GameObject currentSelectionGlowObj; 
    private EditorSnakeVisual currentSelectionGlowScript;
    private List<Vector2Int> currentDraftNodes = new List<Vector2Int>();
    private Stack<GameObject> finishedSnakesHistory = new Stack<GameObject>();

    private Camera mainCam;
    private Vector2Int lastCalculatedGridPos = new Vector2Int(-9999, -9999);

    private void Start()
    {
        mainCam = Camera.main;
        LoadLevelToEdit();
    }

    private void Update()
    {
        // 1. PHÍM TẮT CHỌN TOOL (1, 2, 3, 4)
        if (Input.GetKeyDown(KeyCode.Alpha1)) UI_SetTool(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) UI_SetTool(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UI_SetTool(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) UI_SetTool(3);

        // 2. PHÍM TẮT HƯỚNG (W-A-S-D hoặc Arrow Keys)
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) UI_SetDirection(0);
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) UI_SetDirection(1);
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) UI_SetDirection(2);
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) UI_SetDirection(3);

        // 3. CÁC PHÍM CHỨC NĂNG
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
            else if (currentTool == EditorToolType.Select) HandleSelectClick();
        }
        else if (Input.GetMouseButton(0)) 
        {
            if (currentTool == EditorToolType.Draw) HandleLeftDrag();
            else if (currentTool == EditorToolType.Erase) HandleEraseClick();
        }
    }

    // --- LOGIC CHỌN LEVEL TỪ RESOURCES ---
    public void UI_OpenLevelSelector()
    {
        if (levelSelectorPanel != null) levelSelectorPanel.SetActive(true);
        
        foreach (Transform child in levelScrollContent) Destroy(child.gameObject);

        LevelDataSO[] allLevels = Resources.LoadAll<LevelDataSO>("Levels");
        var sortedLevels = allLevels.OrderBy(l => l.name).ToList();

        foreach (LevelDataSO level in sortedLevels)
        {
            GameObject btnObj = Instantiate(levelButtonPrefab, levelScrollContent);
            LevelSelectItem itemScript = btnObj.GetComponent<LevelSelectItem>();
            if (itemScript != null) itemScript.Setup(level, this);
        }
    }

    public void SelectLevelFromUI(LevelDataSO selectedLevel)
    {
        currentData = selectedLevel;
        if (levelSelectorPanel != null) levelSelectorPanel.SetActive(false);
        LoadLevelToEdit();
        Debug.Log($"<color=cyan>[Editor] Đã tải Level: {selectedLevel.name}</color>");
    }

    // --- CÁC HÀM UI SETTINGS ---
    public void UI_SetTool(int toolIndex) 
    { 
        currentTool = (EditorToolType)toolIndex; 
        UpdateToolText(); 
        if (currentTool != EditorToolType.Select) ClearSelectionHighlight();
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
        else if (currentTool == EditorToolType.Select && selectedSnakeToModify != null)
        {
            selectedSnakeToModify.direction = currentDir;
            selectedSnakeToModify.UpdateVisualRotation();
            if (currentSelectionGlowScript != null)
            {
                currentSelectionGlowScript.direction = currentDir;
                currentSelectionGlowScript.UpdateVisualRotation();
            }
        }
    }

    public void UI_SetColor(Color newColor)
    {
        currentColor = newColor;
        if (colorPreviewImage != null) colorPreviewImage.color = currentColor;
        if (currentSnakeScript != null) currentSnakeScript.SetColorImmediatePublic(currentColor);
        else if (currentTool == EditorToolType.Select && selectedSnakeToModify != null)
        {
            selectedSnakeToModify.SetColorImmediatePublic(currentColor);
            if (currentSelectionGlowScript != null) currentSelectionGlowScript.SetColorImmediatePublic(currentColor);
        }
    }

    public void UI_FinishSnake()
    {
        if (currentSnakeObj == null || currentDraftNodes.Count == 0) return;
        currentSnakeScript.Initialize(currentDir, new List<Vector2Int>(currentDraftNodes), currentColor);
        finishedSnakesHistory.Push(currentSnakeObj);
        currentSnakeObj = null; currentSnakeScript = null; currentDraftNodes.Clear();
        lastCalculatedGridPos = new Vector2Int(-9999, -9999); 
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

    // --- LOGIC XỬ LÝ RẮN VÀ VA CHẠM ---
    private EditorSnakeVisual GetSnakeAtGridPos(Vector2Int pos)
    {
        foreach (Transform snakeParent in levelContainer)
        {
            EditorSnakeVisual sb = snakeParent.GetComponent<EditorSnakeVisual>();
            if (sb != null && sb.gameObject != currentSnakeObj && sb.gameObject != currentSelectionGlowObj && sb.LogicNodes != null)
            {
                foreach (Vector2Int node in sb.LogicNodes)
                {
                    if (node.x == pos.x && node.y == pos.y) return sb;
                }
            }
        }
        return null;
    }

    private bool IsPositionOccupied(Vector2Int pos)
    {
        foreach (var node in currentDraftNodes) if (node == pos) return true;
        return GetSnakeAtGridPos(pos) != null;
    }

    private bool IsTooCloseToOtherSnakes(Vector2Int pos)
    {
        if (minDistanceBetweenSnakes <= 1) return false;
        foreach (Transform snakeParent in levelContainer)
        {
            EditorSnakeVisual sb = snakeParent.GetComponent<EditorSnakeVisual>();
            if (sb != null && sb.gameObject != currentSnakeObj && sb.gameObject != currentSelectionGlowObj && sb.LogicNodes != null)
            {
                foreach (Vector2Int node in sb.LogicNodes)
                {
                    int dist = Mathf.Abs(node.x - pos.x) + Mathf.Abs(node.y - pos.y);
                    if (dist < minDistanceBetweenSnakes) return true;
                }
            }
        }
        return false;
    }

    private void HandleLeftClick()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        if (IsPositionOccupied(gridPos) || IsTooCloseToOtherSnakes(gridPos)) return;
        if (currentSnakeObj == null) CreateHead(gridPos);
        else 
        {
            Vector2Int lastPos = currentDraftNodes[currentDraftNodes.Count - 1];
            if ((Mathf.Abs(gridPos.x - lastPos.x) + Mathf.Abs(gridPos.y - lastPos.y)) == 1) CreateBodySegment(gridPos);
        }
        lastCalculatedGridPos = new Vector2Int(-9999, -9999); 
    }

    private void HandleLeftDrag()
    {
        if (currentSnakeObj == null || currentDraftNodes.Count == 0) return;
        Vector2Int gridPos = GetMouseGridPosition();
        Vector2Int lastPos = currentDraftNodes[currentDraftNodes.Count - 1];
        if (gridPos == lastPos) return; 
        if ((Mathf.Abs(gridPos.x - lastPos.x) + Mathf.Abs(gridPos.y - lastPos.y)) == 1 && !IsPositionOccupied(gridPos) && !IsTooCloseToOtherSnakes(gridPos))
        {
            CreateBodySegment(gridPos);
            lastCalculatedGridPos = new Vector2Int(-9999, -9999); 
        }
    }

    private void HandleEraseClick()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        EditorSnakeVisual sb = GetSnakeAtGridPos(gridPos);
        if (sb != null)
        {
            if (sb.gameObject == currentSnakeObj) { currentSnakeObj = null; currentSnakeScript = null; currentDraftNodes.Clear(); }
            Destroy(sb.gameObject);
            lastCalculatedGridPos = new Vector2Int(-9999, -9999);
        }
    }

    private void HandlePaintClick()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        EditorSnakeVisual sb = GetSnakeAtGridPos(gridPos);
        if (sb != null) sb.SetColorImmediatePublic(currentColor);
    }

    private void HandleSelectClick()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        EditorSnakeVisual sb = GetSnakeAtGridPos(gridPos);
        if (sb != null)
        {
            selectedSnakeToModify = sb;
            currentDir = sb.direction;
            currentColor = sb.snakeColor;
            UpdateToolText();
            if (colorPreviewImage != null) colorPreviewImage.color = currentColor;
            UpdateSelectionHighlight(sb);
        }
        else { selectedSnakeToModify = null; ClearSelectionHighlight(); }
    }

    private void UpdateSelectionHighlight(EditorSnakeVisual target)
    {
        ClearSelectionHighlight();
        if (selectionGlowPrefab == null) return;
        currentSelectionGlowObj = Instantiate(selectionGlowPrefab, target.transform.position, Quaternion.identity, levelContainer);
        currentSelectionGlowScript = currentSelectionGlowObj.GetComponent<EditorSnakeVisual>();
        if (currentSelectionGlowScript != null)
        {
            currentSelectionGlowScript.Initialize(target.direction, new List<Vector2Int>(target.LogicNodes), highlightColor);
            currentSelectionGlowObj.transform.position += new Vector3(0, 0, 0.01f);
        }
    }

    private void ClearSelectionHighlight()
    {
        if (currentSelectionGlowObj != null) { Destroy(currentSelectionGlowObj); currentSelectionGlowObj = null; currentSelectionGlowScript = null; }
    }

    private void CreateHead(Vector2Int pos)
    {
        currentSnakeObj = Instantiate(snakePrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, levelContainer);
        currentSnakeScript = currentSnakeObj.GetComponent<EditorSnakeVisual>();
        currentDraftNodes.Clear(); currentDraftNodes.Add(pos);
        currentSnakeScript.direction = currentDir;
        currentSnakeScript.SetColorImmediatePublic(currentColor);
        currentSnakeScript.UpdateVisualRotation(); 
        UpdateSnakeLinePreview();
    }

    private void CreateBodySegment(Vector2Int pos) { currentDraftNodes.Add(pos); UpdateSnakeLinePreview(); }

    public void UndoLastSegment()
    {
        if (currentSnakeObj != null && currentDraftNodes.Count > 0)
        {
            currentDraftNodes.RemoveAt(currentDraftNodes.Count - 1);
            if (currentDraftNodes.Count == 0) { Destroy(currentSnakeObj); currentSnakeObj = null; }
            else UpdateSnakeLinePreview(); 
        }
        else if (finishedSnakesHistory.Count > 0) Destroy(finishedSnakesHistory.Pop());
        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    private void UpdateSnakeLinePreview()
    {
        if (currentSnakeScript != null && currentDraftNodes.Count > 0)
        {
            LineRenderer lr = currentSnakeScript.GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.positionCount = currentDraftNodes.Count;
                for (int i = 0; i < currentDraftNodes.Count; i++)
                    lr.SetPosition(i, new Vector3(currentDraftNodes[i].x, currentDraftNodes[i].y, 0));
            }
        }
    }

    private void UpdatePreviewCursor()
    {
        if (previewCursor == null) return;
        Vector2Int gridPos = GetMouseGridPosition();
        previewCursor.transform.position = new Vector3(gridPos.x, gridPos.y, -1f); 
        if (gridPos == lastCalculatedGridPos) return;
        lastCalculatedGridPos = gridPos; 
        if (currentTool == EditorToolType.Draw)
        {
            bool invalid = IsPositionOccupied(gridPos) || IsTooCloseToOtherSnakes(gridPos);
            previewCursor.color = invalid ? new Color(1, 0, 0, 0.5f) : new Color(currentColor.r, currentColor.g, currentColor.b, 0.5f);
        }
        else if (currentTool == EditorToolType.Erase) previewCursor.color = new Color(1, 0, 0, 0.5f);
        else if (currentTool == EditorToolType.Select) previewCursor.color = new Color(1, 1, 0, 0.3f);
    }

    private Vector2Int GetMouseGridPosition()
    {
        Vector3 pos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
    }

    // --- LƯU VÀ TẢI LEVEL ---
    public void UI_SaveLevel() { SaveLevel(); }
    public void UI_LoadLevel() { LoadLevelToEdit(); }

    private void SaveLevel()
    {
        if (currentData == null) return;
        ClearSelectionHighlight();
        if (inputTimeLimit != null) float.TryParse(inputTimeLimit.text, out currentData.timeLimit);
        if (inputRewardCoins != null) float.TryParse(inputRewardCoins.text, out currentData.rewardCoins);
        if (inputRewardDiamonds != null) float.TryParse(inputRewardDiamonds.text, out currentData.rewardDiamonds);
        currentData.snakes.Clear();
        foreach (Transform s in levelContainer) {
            EditorSnakeVisual sb = s.GetComponent<EditorSnakeVisual>();
            if (sb != null && sb.gameObject != currentSelectionGlowObj && sb.LogicNodes != null && sb.LogicNodes.Count > 0) {
                SnakeSaveData d = new SnakeSaveData { direction = sb.direction, arrowColor = sb.snakeColor, segmentPositions = new List<Vector2Int>(sb.LogicNodes) };
                currentData.snakes.Add(d);
            }
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(currentData); AssetDatabase.SaveAssets();
#endif
        Debug.Log("<color=green>Đã lưu Level!</color>");
    }

    private void LoadLevelToEdit()
    {
        if (currentData == null) return;
        for (int i = levelContainer.childCount - 1; i >= 0; i--) DestroyImmediate(levelContainer.GetChild(i).gameObject);
        finishedSnakesHistory.Clear(); ClearSelectionHighlight();
        if (inputTimeLimit != null) inputTimeLimit.text = currentData.timeLimit.ToString();
        if (inputRewardCoins != null) inputRewardCoins.text = currentData.rewardCoins.ToString();
        if (inputRewardDiamonds != null) inputRewardDiamonds.text = currentData.rewardDiamonds.ToString();
        foreach (var d in currentData.snakes) {
            GameObject s = Instantiate(snakePrefab, levelContainer);
            EditorSnakeVisual sb = s.GetComponent<EditorSnakeVisual>();
            sb.Initialize(d.direction, d.segmentPositions, d.arrowColor);
            finishedSnakesHistory.Push(s);
        }
    }
}