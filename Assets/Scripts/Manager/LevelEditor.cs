using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EditorToolType { Draw, Erase, Paint, Select, Portal, Keycard, Gate }

public class LevelEditor : MonoBehaviour
{
    [Header("Assets (Data-Driven)")]
    public GameObject snakePrefab;
    public GameObject selectionGlowPrefab;
    public GameObject portalPrefab;
    public GameObject keycardPrefab;
    public GameObject gatePrefab;
    public Color highlightColor = Color.yellow;

    [Header("Data")]
    public LevelDataSO currentData;
    public Transform levelContainer;

    [Header("Level Selector UI")]
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

    private bool isPlacingPortalExit = false;
    private Vector2Int draftPortalEntrance;
    private ArrowDir draftPortalEntranceDir;
    private List<PortalData> currentDraftPortals = new List<PortalData>();
    private List<GameObject> spawnedPortalVisuals = new List<GameObject>();

    private Camera mainCam;
    private Vector2Int lastCalculatedGridPos = new Vector2Int(-9999, -9999);

    private void Start()
    {
        mainCam = Camera.main;
        LoadLevelToEdit();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) UI_SetTool(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) UI_SetTool(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UI_SetTool(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) UI_SetTool(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) UI_SetTool(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) UI_SetTool(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) UI_SetTool(6);

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) UI_SetDirection(0);
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) UI_SetDirection(1);
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) UI_SetDirection(2);
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) UI_SetDirection(3);

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
            else if (currentTool == EditorToolType.Portal) HandlePortalClick();
            else if (currentTool == EditorToolType.Keycard) HandleObjectPlacement<GridKeycard>(keycardPrefab);
            else if (currentTool == EditorToolType.Gate) HandleObjectPlacement<GridLaserGate>(gatePrefab);
        }
        else if (Input.GetMouseButton(0)) 
        {
            if (currentTool == EditorToolType.Draw) HandleLeftDrag();
            else if (currentTool == EditorToolType.Erase) HandleEraseClick();
        }
    }

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
    }

    public void UI_SetTool(int toolIndex) 
    { 
        currentTool = (EditorToolType)toolIndex; 
        UpdateToolText(); 
        if (currentTool != EditorToolType.Select) ClearSelectionHighlight();
        if (currentTool != EditorToolType.Portal) isPlacingPortalExit = false;
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
        if (GetSnakeAtGridPos(pos) != null) return true;
        foreach (Transform child in levelContainer)
        {
            if (child.TryGetComponent(out GridKeycard k) && Mathf.RoundToInt(child.position.x) == pos.x && Mathf.RoundToInt(child.position.y) == pos.y) return true;
            if (child.TryGetComponent(out GridLaserGate g) && Mathf.RoundToInt(child.position.x) == pos.x && Mathf.RoundToInt(child.position.y) == pos.y) return true;
        }
        return false;
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

    private void HandleObjectPlacement<T>(GameObject prefab) where T : MonoBehaviour
    {
        Vector2Int gridPos = GetMouseGridPosition();
        if (IsPositionOccupied(gridPos) || prefab == null) return;

        GameObject obj = Instantiate(prefab, new Vector3(gridPos.x, gridPos.y, 0), Quaternion.identity, levelContainer);
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = currentColor;

        if (obj.TryGetComponent(out GridKeycard k)) k.keyColor = currentColor;
        if (obj.TryGetComponent(out GridLaserGate g)) g.gateColor = currentColor;
        
        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
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
            return;
        }

        foreach (Transform child in levelContainer)
        {
            if ((child.GetComponent<GridKeycard>() != null || child.GetComponent<GridLaserGate>() != null) && Mathf.RoundToInt(child.position.x) == gridPos.x && Mathf.RoundToInt(child.position.y) == gridPos.y)
            {
                Destroy(child.gameObject);
                lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                return;
            }
        }

        for (int i = currentDraftPortals.Count - 1; i >= 0; i--)
        {
            if (currentDraftPortals[i].entrance == gridPos || currentDraftPortals[i].exit == gridPos)
            {
                currentDraftPortals.RemoveAt(i);
                RefreshPortalVisuals();
                lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                return;
            }
        }
    }

    private void HandlePaintClick()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        EditorSnakeVisual sb = GetSnakeAtGridPos(gridPos);
        if (sb != null) sb.SetColorImmediatePublic(currentColor);

        foreach (Transform child in levelContainer)
        {
            if (Mathf.RoundToInt(child.position.x) == gridPos.x && Mathf.RoundToInt(child.position.y) == gridPos.y)
            {
                if (child.TryGetComponent(out GridKeycard k)) { k.keyColor = currentColor; child.GetComponent<SpriteRenderer>().color = currentColor; }
                if (child.TryGetComponent(out GridLaserGate g)) { g.gateColor = currentColor; child.GetComponent<SpriteRenderer>().color = currentColor; }
            }
        }
        
        for (int i = 0; i < currentDraftPortals.Count; i++)
        {
            if (currentDraftPortals[i].entrance == gridPos || currentDraftPortals[i].exit == gridPos)
            {
                currentDraftPortals[i].portalColor = currentColor;
                RefreshPortalVisuals();
                return;
            }
        }
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

    private void HandlePortalClick()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        if (IsPositionOccupied(gridPos)) return;

        if (!isPlacingPortalExit)
        {
            draftPortalEntrance = gridPos;
            draftPortalEntranceDir = currentDir;
            isPlacingPortalExit = true;
        }
        else
        {
            if (gridPos == draftPortalEntrance) return;
            PortalData newPortal = new PortalData();
            newPortal.entrance = draftPortalEntrance;
            newPortal.entranceDir = draftPortalEntranceDir;
            newPortal.exit = gridPos;
            newPortal.exitDir = currentDir;
            newPortal.portalColor = currentColor;
            currentDraftPortals.Add(newPortal);
            isPlacingPortalExit = false;
            RefreshPortalVisuals();
        }
        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    private void RefreshPortalVisuals()
    {
        foreach(var obj in spawnedPortalVisuals) Destroy(obj);
        spawnedPortalVisuals.Clear();
        if (portalPrefab == null) return;
        for (int i = 0; i < currentDraftPortals.Count; i++)
        {
            var p = currentDraftPortals[i];
            string pairLabel = GetPortalPairLabel(i);
            GameObject inObj = Instantiate(portalPrefab, new Vector3(p.entrance.x, p.entrance.y, 0), GetRotationForDir(p.entranceDir), levelContainer);
            GameObject outObj = Instantiate(portalPrefab, new Vector3(p.exit.x, p.exit.y, 0), GetRotationForDir(p.exitDir), levelContainer);
            SpriteRenderer inSr = inObj.GetComponent<SpriteRenderer>();
            if(inSr) inSr.color = p.portalColor;
            SpriteRenderer outSr = outObj.GetComponent<SpriteRenderer>();
            if(outSr) outSr.color = p.portalColor;

            AttachPortalPairLabel(inObj, pairLabel);
            AttachPortalPairLabel(outObj, pairLabel);
            spawnedPortalVisuals.Add(inObj);
            spawnedPortalVisuals.Add(outObj);
        }
    }

    private static Quaternion GetRotationForDir(ArrowDir dir)
    {
        float angle = 0f;
        switch (dir)
        {
            case ArrowDir.Up: angle = 0f; break;
            case ArrowDir.Down: angle = 180f; break;
            case ArrowDir.Left: angle = 90f; break;
            case ArrowDir.Right: angle = -90f; break;
        }
        return Quaternion.Euler(0f, 0f, angle);
    }

    private static void AttachPortalPairLabel(GameObject portalObj, string pairLabel)
    {
        if (portalObj == null) return;

        GameObject labelObj = new GameObject("PortalPairLabel");
        labelObj.transform.SetParent(portalObj.transform, false);
        labelObj.transform.localPosition = new Vector3(0f, 0f, -0.05f);
        // Keep text upright in world space (do NOT rotate with the portal).
        labelObj.transform.rotation = Quaternion.identity;
        labelObj.transform.localScale = Vector3.one;

        TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
        tmp.text = pairLabel;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 6.5f;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        MeshRenderer mr = labelObj.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            SpriteRenderer sr = portalObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                mr.sortingLayerID = sr.sortingLayerID;
                mr.sortingOrder = sr.sortingOrder + 1;
            }
        }
    }

    private static string GetPortalPairLabel(int indexZeroBased)
    {
        // 0->A, 1->B, ... 25->Z, 26->AA ...
        int n = indexZeroBased;
        if (n < 0) return "?";

        string s = string.Empty;
        do
        {
            int r = n % 26;
            s = (char)('A' + r) + s;
            n = (n / 26) - 1;
        } while (n >= 0);

        return s;
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
        if (currentTool == EditorToolType.Portal && isPlacingPortalExit)
        {
            isPlacingPortalExit = false;
            lastCalculatedGridPos = new Vector2Int(-9999, -9999);
            return;
        }

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
        else if (currentTool == EditorToolType.Portal || currentTool == EditorToolType.Keycard || currentTool == EditorToolType.Gate) 
            previewCursor.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.8f);
    }

    private Vector2Int GetMouseGridPosition()
    {
        Vector3 pos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
    }

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
        currentData.keycards.Clear();
        currentData.gates.Clear();

        foreach (Transform s in levelContainer) {
            EditorSnakeVisual sb = s.GetComponent<EditorSnakeVisual>();
            if (sb != null && sb.gameObject != currentSelectionGlowObj && sb.LogicNodes != null && sb.LogicNodes.Count > 0) {
                SnakeSaveData d = new SnakeSaveData { direction = sb.direction, arrowColor = sb.snakeColor, segmentPositions = new List<Vector2Int>(sb.LogicNodes) };
                currentData.snakes.Add(d);
            }

            if (s.TryGetComponent(out GridKeycard k))
                currentData.keycards.Add(new KeycardSaveData { position = new Vector2Int((int)s.position.x, (int)s.position.y), color = k.keyColor });
            
            if (s.TryGetComponent(out GridLaserGate g))
                currentData.gates.Add(new GateSaveData { position = new Vector2Int((int)s.position.x, (int)s.position.y), color = g.gateColor });
        }

        currentData.portals = new List<PortalData>(currentDraftPortals);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(currentData); 
        UnityEditor.AssetDatabase.SaveAssets();
#endif
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

        if (currentData.keycards != null) {
            foreach (var kData in currentData.keycards) {
                GameObject k = Instantiate(keycardPrefab, new Vector3(kData.position.x, kData.position.y, 0), Quaternion.identity, levelContainer);
                if (k.TryGetComponent(out GridKeycard script)) script.keyColor = kData.color;
                k.GetComponent<SpriteRenderer>().color = kData.color;
            }
        }

        if (currentData.gates != null) {
            foreach (var gData in currentData.gates) {
                GameObject g = Instantiate(gatePrefab, new Vector3(gData.position.x, gData.position.y, 0), Quaternion.identity, levelContainer);
                if (g.TryGetComponent(out GridLaserGate script)) script.gateColor = gData.color;
                g.GetComponent<SpriteRenderer>().color = gData.color;
            }
        }

        currentDraftPortals.Clear();
        if (currentData.portals != null)
        {
            foreach(var p in currentData.portals) currentDraftPortals.Add(p);
        }
        RefreshPortalVisuals();
    }
}