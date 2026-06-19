using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public enum EditorToolType { Draw, Erase, Paint, Select, Portal, Keycard, Gate, Deflector, CountdownBlock, ElectricButton, ElectricWall, StopBlock, ArrowShadow, TurnStateBlock, BlackHole, RevealWaveButton }

public partial class LevelEditor : MonoBehaviour
{
    public static LevelEditor Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private const string BootstrapScenePath = "Assets/Scenes/Boostrap.unity";
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";

    [Header("Assets (Data-Driven)")]
    public GameObject snakePrefab;
    public GameObject selectionGlowPrefab;
    public GameObject portalPrefab;
    public GameObject keycardPrefab;
    public GameObject gatePrefab;
    public GameObject electricButtonPrefab;
    public GameObject revealWaveButtonPrefab;
    public GameObject electricWallPrefab;
    public GameObject deflectorPrefab;
    public GameObject countdownBlockPrefab;
    public GameObject stopBlockPrefab;
    public GameObject turnStateBlockPrefab;
    public GameObject blackHolePrefab;
    public Color highlightColor = Color.yellow;

    [Header("Data")]
    public LevelDataV2 currentData;
    public LevelDataV2 editingData;
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
    [SerializeField] private bool checkDeadlockContinuously = true;
    [SerializeField, Min(0.05f)] private float deadlockCheckInterval = 0.5f;
    [SerializeField] private int deadlockScanLimit = 512;

    [Header("Camera Framing")]
    [SerializeField] private bool frameLevelOnLoad = true;
    [SerializeField] private float levelFramePadding = 2f;
    [SerializeField] private float minimumEditorCameraSize = 8f;

    [Header("Metadata UI")]
    public TMP_InputField inputTimeLimit;
    public TMP_InputField inputRewardCoins;
    public TMP_InputField inputRewardDiamonds;

    [Header("Countdown Block")]
    public int editorCountdownValue = 3;
    public TMP_InputField inputCountdownValue;

    private GameObject currentSnakeObj;
    private EditorSnakeVisual currentSnakeScript;
    private EditorSnakeVisual selectedSnakeToModify;
    private GameObject currentSelectionGlowObj; 
    private EditorSnakeVisual currentSelectionGlowScript;
    private Transform selectionOverlayContainer;
    
    private List<Vector2Int> currentDraftNodes = new List<Vector2Int>();
    private Stack<GameObject> finishedSnakesHistory = new Stack<GameObject>();

    private bool isPlacingPortalExit = false;
    private Vector2Int draftPortalEntrance;
    private ArrowDir draftPortalEntranceDir;
    private Color draftPortalColor = Color.white;
    private List<PortalData> currentDraftPortals = new List<PortalData>();
    private List<GameObject> spawnedPortalVisuals = new List<GameObject>();

    private bool isPlacingElectricWallEnd = false;
    private Vector2Int draftElectricWallStart;
    private Color draftElectricWallColor = Color.white;
    private List<ElectricWallSaveData> currentDraftElectricWalls = new List<ElectricWallSaveData>();
    private List<GameObject> spawnedElectricWallVisuals = new List<GameObject>();

    private Camera mainCam;
    private Vector2Int lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    private EditorToolType previousToolBeforeRMB = EditorToolType.Draw;
    private bool isRmbHoldingErase = false;
    public EditorHistoryService HistoryService { get; set; }
    private float _nextDeadlockCheckTime;
    private bool _hasContinuousDeadlockState;
    private bool _lastContinuousDeadlockState;
    private string _lastContinuousDeadlockMessage = string.Empty;
    private readonly LevelEditorSerializer serializer = new LevelEditorSerializer();
    private readonly LevelEditorDeadlockStateBuilder deadlockStateBuilder = new LevelEditorDeadlockStateBuilder();
    private readonly LevelEditorDeadlockValidator deadlockValidator = new LevelEditorDeadlockValidator();
    private readonly LevelEditorPlaytestBridge playtestBridge = new LevelEditorPlaytestBridge();

    private void Start()
    {
        mainCam = GetCameraInMyScene();
        EnsureSelectionOverlayContainer();
        LoadLevelToEdit();
    }

    private void EnsureSelectionOverlayContainer()
    {
        if (selectionOverlayContainer != null) return;

        GameObject go = new GameObject("SelectionOverlay");
        selectionOverlayContainer = go.transform;
        // Keep it outside levelContainer so SaveLevel() never picks it up as level content.
        selectionOverlayContainer.SetParent(transform, false);
        selectionOverlayContainer.localPosition = Vector3.zero;
        selectionOverlayContainer.localRotation = Quaternion.identity;
        selectionOverlayContainer.localScale = Vector3.one;
    }

    private Camera GetCameraInMyScene()
    {
        // Avoid Camera.main during scene transitions: Buffer/loading scenes can temporarily provide
        // a MainCamera-tagged camera. When Buffer unloads, cached references become destroyed.
        if (mainCam != null && mainCam.gameObject.scene == gameObject.scene)
            return mainCam;

        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera camera in cameras)
        {
            if (camera == null) continue;
            if (!camera.enabled) continue;
            if (camera.gameObject.scene != gameObject.scene) continue;
            mainCam = camera;
            return mainCam;
        }

        // Fallback (should be rare).
        mainCam = Camera.main;
        return mainCam;
    }

    private LevelEditorContext CreateContext()
    {
        return new LevelEditorContext
        {
            currentData = editingData != null ? editingData : currentData,
            levelContainer = levelContainer,
            snakePrefab = snakePrefab,
            portalPrefab = portalPrefab,
            keycardPrefab = keycardPrefab,
            gatePrefab = gatePrefab,
            electricButtonPrefab = electricButtonPrefab,
            revealWaveButtonPrefab = revealWaveButtonPrefab,
            electricWallPrefab = electricWallPrefab,
            deflectorPrefab = deflectorPrefab,
            countdownBlockPrefab = countdownBlockPrefab,
            stopBlockPrefab = stopBlockPrefab,
            turnStateBlockPrefab = turnStateBlockPrefab,
            blackHolePrefab = blackHolePrefab,
            currentSnakeObj = currentSnakeObj,
            currentSnakeScript = currentSnakeScript,
            currentSelectionGlowObj = currentSelectionGlowObj,
            currentDraftNodes = currentDraftNodes,
            currentDraftPortals = currentDraftPortals,
            spawnedPortalVisuals = spawnedPortalVisuals,
            currentDraftElectricWalls = currentDraftElectricWalls,
            spawnedElectricWallVisuals = spawnedElectricWallVisuals,
            finishedSnakesHistory = finishedSnakesHistory,
            inputTimeLimit = inputTimeLimit,
            inputRewardCoins = inputRewardCoins,
            inputRewardDiamonds = inputRewardDiamonds,
            currentDir = currentDir,
            currentColor = currentColor
        };
    }

    private void Update()
    {
        UpdatePreviewCursor();
        RunContinuousDeadlockCheck();
        HandleMouseInput();
    }

    public void TriggerUndo()
    {
        bool isDrawingOrPlacingDraft = (currentSnakeObj != null && currentDraftNodes.Count > 0)
            || (currentTool == EditorToolType.Portal && isPlacingPortalExit)
            || (currentTool == EditorToolType.ElectricWall && isPlacingElectricWallEnd);

        if (isDrawingOrPlacingDraft)
        {
            UndoLastSegment();
        }
        else if (HistoryService != null && HistoryService.CanUndo(this))
        {
            HistoryService.Undo(this);
        }
        else
        {
            UndoLastSegment();
        }
    }

    public void TriggerRedo()
    {
        if (HistoryService != null && HistoryService.CanRedo(this))
        {
            HistoryService.Redo(this);
        }
    }

    public void RotateDirectionPublic()
    {
        RotateDirection();
    }


    private void HandleMouseInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // Right Mouse Button down (Erase start)
        if (Input.GetMouseButtonDown(1))
        {
            previousToolBeforeRMB = currentTool;
            isRmbHoldingErase = true;
            UI_SetTool((int)EditorToolType.Erase);
            HandleEraseClick();
            return;
        }

        // Right Mouse Button hold (Erase drag)
        if (Input.GetMouseButton(1))
        {
            if (isRmbHoldingErase)
            {
                HandleEraseClick();
                return;
            }
        }

        // Right Mouse Button up (Erase end)
        if (Input.GetMouseButtonUp(1))
        {
            if (isRmbHoldingErase)
            {
                UI_SetTool((int)previousToolBeforeRMB);
                isRmbHoldingErase = false;
                return;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandlePrimaryMouseDown();
            return;
        }

        if (Input.GetMouseButton(0))
        {
            HandlePrimaryMouseHold();
        }

        // Left Mouse Button up (Auto-finish snake drawing)
        if (Input.GetMouseButtonUp(0))
        {
            if (currentTool == EditorToolType.Draw && currentSnakeObj != null && currentDraftNodes.Count > 0)
            {
                UI_FinishSnake();
            }
        }
    }

    private void HandlePrimaryMouseDown()
    {
        if (currentTool == EditorToolType.Draw) HandleLeftClick();
        else if (currentTool == EditorToolType.Erase) HandleEraseClick();
        else if (currentTool == EditorToolType.Paint) HandlePaintClick();
        else if (currentTool == EditorToolType.Select) HandleSelectClick();
        else if (currentTool == EditorToolType.Portal) HandlePortalClick();
        else if (currentTool == EditorToolType.Keycard) HandleObjectPlacement<GridKeycard>(keycardPrefab);
        else if (currentTool == EditorToolType.Gate) HandleObjectPlacement<GridLaserGate>(gatePrefab);
        else if (currentTool == EditorToolType.Deflector) HandleDeflectorPlacement();
        else if (currentTool == EditorToolType.CountdownBlock) HandleCountdownBlockPlacement();
        else if (currentTool == EditorToolType.ElectricButton) HandleObjectPlacement<GridElectricButton>(electricButtonPrefab);
        else if (currentTool == EditorToolType.ElectricWall) HandleElectricWallClick();
        else if (currentTool == EditorToolType.StopBlock) HandleStopBlockPlacement();
        else if (currentTool == EditorToolType.ArrowShadow) HandleArrowShadowClick();
        else if (currentTool == EditorToolType.TurnStateBlock) HandleTurnStateBlockPlacement();
        else if (currentTool == EditorToolType.BlackHole) HandleBlackHolePlacement();
        else if (currentTool == EditorToolType.RevealWaveButton) HandleObjectPlacement<GridRevealWaveButton>(revealWaveButtonPrefab);

        OnManipulationComplete();
    }

    private void HandlePrimaryMouseHold()
    {
        if (currentTool == EditorToolType.Draw) HandleLeftDrag();
        else if (currentTool == EditorToolType.Erase) HandleEraseClick();

        OnManipulationComplete();
    }

    public void UI_OpenLevelSelector()
    {
        if (levelSelectorPanel != null) levelSelectorPanel.SetActive(true);
        foreach (Transform child in levelScrollContent) Destroy(child.gameObject);
        LevelDataV2[] allLevels = Resources.LoadAll<LevelDataV2>("Levels");
        var sortedLevels = allLevels.OrderBy(l => l.name).ToList();
        foreach (LevelDataV2 level in sortedLevels)
        {
            GameObject btnObj = Instantiate(levelButtonPrefab, levelScrollContent);
            LevelSelectItem itemScript = btnObj.GetComponent<LevelSelectItem>();
            if (itemScript != null) itemScript.Setup(level, this);
        }
    }

    public void SelectLevelFromUI(LevelDataV2 selectedLevel)
    {
        currentData = selectedLevel;
        if (levelSelectorPanel != null) levelSelectorPanel.SetActive(false);
        LoadLevelToEdit();
    }

    public void UI_SetTool(int toolIndex) 
    { 
        EditorToolType newTool = (EditorToolType)toolIndex;

        // Auto-finish current draft if switching away
        if (currentTool == EditorToolType.Draw && newTool != EditorToolType.Draw)
        {
            if (currentSnakeObj != null && currentDraftNodes.Count > 0)
            {
                UI_FinishSnake();
            }
        }

        // If switching to Draw tool and we have a selected snake, convert it to draft mode
        if (newTool == EditorToolType.Draw && selectedSnakeToModify != null)
        {
            currentSnakeObj = selectedSnakeToModify.gameObject;
            currentSnakeScript = selectedSnakeToModify;
            currentDraftNodes = new List<Vector2Int>(selectedSnakeToModify.LogicNodes);
            currentDir = selectedSnakeToModify.direction;
            currentColor = selectedSnakeToModify.snakeColor;

            RemoveFromFinishedHistory(currentSnakeObj);

            selectedSnakeToModify = null;
            ClearSelectionHighlight();

            UpdateSnakeLinePreview();
        }

        currentTool = newTool; 
        UpdateToolText(); 
        if (currentTool != EditorToolType.Select) ClearSelectionHighlight();
        if (currentTool != EditorToolType.Portal) isPlacingPortalExit = false;
        if (currentTool != EditorToolType.ElectricWall) isPlacingElectricWallEnd = false;
    }

    private void RemoveFromFinishedHistory(GameObject obj)
    {
        if (finishedSnakesHistory == null || finishedSnakesHistory.Count == 0) return;

        List<GameObject> temp = new List<GameObject>(finishedSnakesHistory);
        temp.RemoveAll(item => item == obj || item == null);
        
        finishedSnakesHistory.Clear();
        for (int i = temp.Count - 1; i >= 0; i--)
        {
            finishedSnakesHistory.Push(temp[i]);
        }
    }

    public void UI_SetDirection(int dirIndex)
    {
        if (currentSnakeScript != null || (currentTool == EditorToolType.Select && selectedSnakeToModify != null))
        {
            HistoryService?.RecordState(CaptureSnapshot());
        }
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
        OnManipulationComplete();
    }

    public void UI_SetColor(Color newColor)
    {
        if (currentSnakeScript != null || (currentTool == EditorToolType.Select && selectedSnakeToModify != null))
        {
            HistoryService?.RecordState(CaptureSnapshot());
        }
        currentColor = newColor;
        if (colorPreviewImage != null) colorPreviewImage.color = currentColor;
        if (currentSnakeScript != null) currentSnakeScript.SetColorImmediatePublic(currentColor);
        else if (currentTool == EditorToolType.Select && selectedSnakeToModify != null)
        {
            selectedSnakeToModify.SetColorImmediatePublic(currentColor);
            if (currentSelectionGlowScript != null) currentSelectionGlowScript.SetColorImmediatePublic(currentColor);
        }
        OnManipulationComplete();
    }

    public void UI_FinishSnake()
    {
        if (currentSnakeObj == null || currentDraftNodes.Count == 0) return;
        HistoryService?.RecordState(CaptureSnapshot());
        currentSnakeScript.Initialize(currentDir, new List<Vector2Int>(currentDraftNodes), currentColor);
        finishedSnakesHistory.Push(currentSnakeObj);
        currentSnakeObj = null; currentSnakeScript = null; currentDraftNodes.Clear();
        lastCalculatedGridPos = new Vector2Int(-9999, -9999); 
        OnManipulationComplete();
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

        for (int i = 0; i < currentDraftElectricWalls.Count; i++)
        {
            if (IsCellOnElectricWall(pos, currentDraftElectricWalls[i])) return true;
        }

        foreach (Transform child in levelContainer)
        {
            if (child.TryGetComponent(out GridKeycard k) && Mathf.RoundToInt(child.position.x) == pos.x && Mathf.RoundToInt(child.position.y) == pos.y) return true;
            if (child.TryGetComponent(out GridLaserGate g) && Mathf.RoundToInt(child.position.x) == pos.x && Mathf.RoundToInt(child.position.y) == pos.y) return true;
            if (child.TryGetComponent(out GridElectricButton eb) && Mathf.RoundToInt(child.position.x) == pos.x && Mathf.RoundToInt(child.position.y) == pos.y) return true;
            if (child.TryGetComponent(out GridRevealWaveButton rwb) && Mathf.RoundToInt(child.position.x) == pos.x && Mathf.RoundToInt(child.position.y) == pos.y) return true;
            GridElectricWall ew = child.GetComponent<GridElectricWall>();
            if (ew != null && ew.ContainsCell(pos)) return true;
            GridDeflector d = child.GetComponentInChildren<GridDeflector>();
            if (d != null && Mathf.RoundToInt(d.transform.position.x) == pos.x && Mathf.RoundToInt(d.transform.position.y) == pos.y) return true;
            if (child.TryGetComponent(out GridCountdownBlock cb) && Mathf.RoundToInt(child.position.x) == pos.x && Mathf.RoundToInt(child.position.y) == pos.y) return true;
            if (child.TryGetComponent(out GridStopBlock sb) && Mathf.RoundToInt(child.position.x) == pos.x && Mathf.RoundToInt(child.position.y) == pos.y) return true;
            if (child.TryGetComponent(out GridTurnStateBlock tb) && Mathf.RoundToInt(child.position.x) == pos.x && Mathf.RoundToInt(child.position.y) == pos.y) return true;
            if (child.TryGetComponent(out GridBlackHole bh) && Mathf.RoundToInt(child.position.x) == pos.x && Mathf.RoundToInt(child.position.y) == pos.y) return true;
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
        if (currentSnakeObj == null)
        {
            HistoryService?.RecordState(CaptureSnapshot());
            CreateHead(gridPos);
        }
        else 
        {
            Vector2Int headPos = currentDraftNodes[0];
            Vector2Int lastPos = currentDraftNodes[currentDraftNodes.Count - 1];

            int distToTail = Mathf.Abs(gridPos.x - lastPos.x) + Mathf.Abs(gridPos.y - lastPos.y);
            int distToHead = Mathf.Abs(gridPos.x - headPos.x) + Mathf.Abs(gridPos.y - headPos.y);

            if (distToTail == 1)
            {
                CreateBodySegment(gridPos);
                UpdateAutoDirection();
            }
            else if (distToHead == 1)
            {
                CreateHeadSegment(gridPos);
                UpdateAutoDirection();
            }
        }
        lastCalculatedGridPos = new Vector2Int(-9999, -9999); 
    }

    private void HandleLeftDrag()
    {
        if (currentSnakeObj == null || currentDraftNodes.Count == 0) return;
        Vector2Int gridPos = GetMouseGridPosition();
        Vector2Int headPos = currentDraftNodes[0];
        Vector2Int lastPos = currentDraftNodes[currentDraftNodes.Count - 1];
        if (gridPos == lastPos || gridPos == headPos) return; 

        // 1. Check for Drag-to-Retract
        if (currentDraftNodes.Count >= 2)
        {
            if (gridPos == currentDraftNodes[currentDraftNodes.Count - 2])
            {
                RetractTailSegment();
                return;
            }
            if (gridPos == currentDraftNodes[1])
            {
                RetractHeadSegment();
                return;
            }
        }

        // 2. Otherwise, check for Drag-to-Extend (Head or Tail)
        int distToTail = Mathf.Abs(gridPos.x - lastPos.x) + Mathf.Abs(gridPos.y - lastPos.y);
        int distToHead = Mathf.Abs(gridPos.x - headPos.x) + Mathf.Abs(gridPos.y - headPos.y);

        if (distToTail <= distToHead)
        {
            // Draw from tail
            if (distToTail == 1)
            {
                if (!IsPositionOccupied(gridPos) && !IsTooCloseToOtherSnakes(gridPos))
                {
                    CreateBodySegment(gridPos);
                    UpdateAutoDirection();
                    lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                }
            }
            else if (distToTail > 1)
            {
                List<Vector2Int> path = GetInterpolatedPath(lastPos, gridPos);
                bool addedAny = false;
                foreach (Vector2Int step in path)
                {
                    CreateBodySegment(step);
                    addedAny = true;
                }
                if (addedAny)
                {
                    UpdateAutoDirection();
                    lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                }
            }
        }
        else
        {
            // Draw from head
            if (distToHead == 1)
            {
                if (!IsPositionOccupied(gridPos) && !IsTooCloseToOtherSnakes(gridPos))
                {
                    CreateHeadSegment(gridPos);
                    UpdateAutoDirection();
                    lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                }
            }
            else if (distToHead > 1)
            {
                List<Vector2Int> path = GetInterpolatedPath(headPos, gridPos);
                bool addedAny = false;
                foreach (Vector2Int step in path)
                {
                    CreateHeadSegment(step);
                    addedAny = true;
                }
                if (addedAny)
                {
                    UpdateAutoDirection();
                    lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                }
            }
        }
    }

    private List<Vector2Int> GetInterpolatedPath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = start;
        int maxSteps = 50;
        int steps = 0;

        while (current != end && steps < maxSteps)
        {
            steps++;
            int dx = end.x - current.x;
            int dy = end.y - current.y;

            Vector2Int nextStep = current;
            if (Mathf.Abs(dx) >= Mathf.Abs(dy))
            {
                nextStep.x += (int)Mathf.Sign(dx);
            }
            else
            {
                nextStep.y += (int)Mathf.Sign(dy);
            }

            if (IsPositionOccupied(nextStep) || IsTooCloseToOtherSnakes(nextStep))
            {
                break;
            }

            path.Add(nextStep);
            current = nextStep;
        }
        return path;
    }

    private void UpdateAutoDirection()
    {
        if (currentDraftNodes == null || currentDraftNodes.Count < 2) return;

        Vector2Int head = currentDraftNodes[0];
        Vector2Int neck = currentDraftNodes[1];
        Vector2Int diff = head - neck;

        ArrowDir newDir = currentDir;
        if (diff == Vector2Int.up) newDir = ArrowDir.Up;
        else if (diff == Vector2Int.down) newDir = ArrowDir.Down;
        else if (diff == Vector2Int.left) newDir = ArrowDir.Left;
        else if (diff == Vector2Int.right) newDir = ArrowDir.Right;

        if (newDir != currentDir)
        {
            currentDir = newDir;
            UpdateToolText();
            if (currentSnakeScript != null)
            {
                currentSnakeScript.direction = currentDir;
                currentSnakeScript.UpdateVisualRotation();
            }
        }
    }

    private void HandleObjectPlacement<T>(GameObject prefab) where T : MonoBehaviour
    {
        Vector2Int gridPos = GetMouseGridPosition();
        if (IsPositionOccupied(gridPos) || prefab == null) return;

        HistoryService?.RecordState(CaptureSnapshot());
        GameObject obj = Instantiate(prefab, new Vector3(gridPos.x, gridPos.y, 0), Quaternion.identity, levelContainer);
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = currentColor;

        if (obj.TryGetComponent(out GridKeycard k)) k.keyColor = currentColor;
        if (obj.TryGetComponent(out GridLaserGate g)) g.gateColor = currentColor;
        if (obj.TryGetComponent(out GridElectricButton eb)) eb.SetColor(currentColor);
        if (obj.TryGetComponent(out GridRevealWaveButton rwb)) rwb.SetColor(currentColor);
        
        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    private void HandleDeflectorPlacement()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        if (IsPositionOccupied(gridPos) || deflectorPrefab == null) return;

        HistoryService?.RecordState(CaptureSnapshot());
        GameObject obj = Instantiate(deflectorPrefab, new Vector3(gridPos.x, gridPos.y, 0), GetRotationForDir(currentDir), levelContainer);
        GridDeflector deflector = obj.GetComponentInChildren<GridDeflector>();
        if (deflector != null) deflector.SetDirection(currentDir);

        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    private void HandleCountdownBlockPlacement()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        if (IsPositionOccupied(gridPos) || countdownBlockPrefab == null) return;

        HistoryService?.RecordState(CaptureSnapshot());
        if (inputCountdownValue != null)
            int.TryParse(inputCountdownValue.text, out editorCountdownValue);
        if (editorCountdownValue < 1) editorCountdownValue = 1;

        GameObject obj = Instantiate(countdownBlockPrefab, new Vector3(gridPos.x, gridPos.y, 0), Quaternion.identity, levelContainer);
        GridCountdownBlock block = obj.GetComponent<GridCountdownBlock>();
        if (block != null) block.SetCount(editorCountdownValue);

        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    private void HandleStopBlockPlacement()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        if (IsPositionOccupied(gridPos) || stopBlockPrefab == null) return;

        HistoryService?.RecordState(CaptureSnapshot());
        if (inputCountdownValue != null)
            int.TryParse(inputCountdownValue.text, out editorCountdownValue);
        if (editorCountdownValue < 1) editorCountdownValue = 1;

        GameObject obj = Instantiate(stopBlockPrefab, new Vector3(gridPos.x, gridPos.y, 0), Quaternion.identity, levelContainer);
        GridStopBlock block = obj.GetComponent<GridStopBlock>();
        if (block != null) block.SetCount(editorCountdownValue);

        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    private void HandleArrowShadowClick()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        EditorSnakeVisual snake = GetSnakeAtGridPos(gridPos);
        if (snake == null) return;

        HistoryService?.RecordState(CaptureSnapshot());
        snake.SetArrowShadowEnabled(!snake.HasArrowShadow);
        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    private void HandleTurnStateBlockPlacement()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        if (IsPositionOccupied(gridPos) || turnStateBlockPrefab == null) return;

        HistoryService?.RecordState(CaptureSnapshot());
        GameObject obj = Instantiate(turnStateBlockPrefab, new Vector3(gridPos.x, gridPos.y, 0), Quaternion.identity, levelContainer);
        GridTurnStateBlock block = obj.GetComponent<GridTurnStateBlock>();
        if (block != null) block.SetInitialState(ShouldUseRedTurnState());

        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    private void HandleBlackHolePlacement()
    {
        Vector2Int gridPos = GetMouseGridPosition();

        foreach (Transform child in levelContainer)
        {
            if (child.TryGetComponent(out GridBlackHole existingBlackHole)
                && Mathf.RoundToInt(child.position.x) == gridPos.x
                && Mathf.RoundToInt(child.position.y) == gridPos.y)
            {
                HistoryService?.RecordState(CaptureSnapshot());
                existingBlackHole.SetDirection(currentDir);
                child.rotation = GetRotationForDir(currentDir);
                lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                return;
            }
        }

        if (IsPositionOccupied(gridPos) || blackHolePrefab == null) return;

        HistoryService?.RecordState(CaptureSnapshot());
        GameObject obj = Instantiate(blackHolePrefab, new Vector3(gridPos.x, gridPos.y, 0), GetRotationForDir(currentDir), levelContainer);
        GridBlackHole blackHole = obj.GetComponent<GridBlackHole>();
        if (blackHole != null) blackHole.SetDirection(currentDir);

        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    private bool ShouldUseRedTurnState()
    {
        float redDistance = ColorDistanceSqr(currentColor, Color.red);
        float greenDistance = ColorDistanceSqr(currentColor, Color.green);
        return redDistance <= greenDistance;
    }

    private static float ColorDistanceSqr(Color a, Color b)
    {
        float dr = a.r - b.r;
        float dg = a.g - b.g;
        float db = a.b - b.b;
        return dr * dr + dg * dg + db * db;
    }

    private void HandleEraseClick()
    {
        Vector2Int gridPos = GetMouseGridPosition();

        // Erase behavior:
        // - Click: trim from clicked node to tail (current behavior, but partial).
        // - Shift + Click: trim from head to clicked node.
        bool trimFromHead = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // If we're currently drawing a snake, allow trimming it from the clicked node onward.
        if (currentSnakeObj != null && currentDraftNodes != null && currentDraftNodes.Count > 0)
        {
            int draftIndex = currentDraftNodes.FindIndex(n => n == gridPos);
            if (draftIndex >= 0)
            {
                HistoryService?.RecordState(CaptureSnapshot());
                if (trimFromHead)
                {
                    currentDraftNodes.RemoveRange(0, draftIndex + 1);
                }
                else
                {
                    int removeCount = currentDraftNodes.Count - draftIndex;
                    currentDraftNodes.RemoveRange(draftIndex, removeCount);
                }

                if (currentDraftNodes.Count == 0)
                {
                    Destroy(currentSnakeObj);
                    currentSnakeObj = null;
                    currentSnakeScript = null;
                }
                else
                {
                    // If we trimmed from the head, move the snake object and arrow to the new head.
                    if (trimFromHead)
                    {
                        Vector2Int newHead = currentDraftNodes[0];
                        currentSnakeObj.transform.position = new Vector3(newHead.x, newHead.y, 0);
                        if (currentSnakeScript != null) currentSnakeScript.SetArrowWorldPosition(newHead);
                    }
                    UpdateSnakeLinePreview();
                }

                lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                return;
            }
        }

        EditorSnakeVisual sb = GetSnakeAtGridPos(gridPos);
        if (sb != null)
        {
            // Trim finished snakes instead of deleting the entire snake.
            if (sb.LogicNodes != null)
            {
                int index = sb.LogicNodes.FindIndex(n => n == gridPos);
                if (index >= 0)
                {
                    HistoryService?.RecordState(CaptureSnapshot());
                    if (trimFromHead)
                    {
                        sb.LogicNodes.RemoveRange(0, index + 1);
                    }
                    else
                    {
                        int removeCount = sb.LogicNodes.Count - index;
                        sb.LogicNodes.RemoveRange(index, removeCount);
                    }

                    if (sb.LogicNodes.Count == 0)
                    {
                        if (selectedSnakeToModify == sb) { selectedSnakeToModify = null; ClearSelectionHighlight(); }
                        Destroy(sb.gameObject);
                    }
                    else
                    {
                        sb.Initialize(sb.direction, new List<Vector2Int>(sb.LogicNodes), sb.snakeColor, sb.HasArrowShadow);
                        if (selectedSnakeToModify == sb)
                        {
                            UpdateSelectionHighlight(sb);
                        }
                    }

                    lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                    return;
                }
            }

            // Fallback: if something unexpected happened, keep old behavior.
            if (selectedSnakeToModify == sb) { selectedSnakeToModify = null; ClearSelectionHighlight(); }
            Destroy(sb.gameObject);
            lastCalculatedGridPos = new Vector2Int(-9999, -9999);
            return;
        }

        foreach (Transform child in levelContainer)
        {
            GridDeflector deflector = child.GetComponentInChildren<GridDeflector>();
            if ((child.GetComponent<GridKeycard>() != null || child.GetComponent<GridLaserGate>() != null || child.GetComponent<GridElectricButton>() != null || child.GetComponent<GridRevealWaveButton>() != null || deflector != null || child.GetComponent<GridCountdownBlock>() != null || child.GetComponent<GridStopBlock>() != null || child.GetComponent<GridTurnStateBlock>() != null || child.GetComponent<GridBlackHole>() != null)
                && Mathf.RoundToInt(child.position.x) == gridPos.x && Mathf.RoundToInt(child.position.y) == gridPos.y)
            {
                HistoryService?.RecordState(CaptureSnapshot());
                Destroy(child.gameObject);
                lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                return;
            }
        }

        if (TryRemoveElectricWallAtPos(gridPos))
        {
            lastCalculatedGridPos = new Vector2Int(-9999, -9999);
            return;
        }

        for (int i = currentDraftPortals.Count - 1; i >= 0; i--)
        {
            if (currentDraftPortals[i].entrance == gridPos || currentDraftPortals[i].exit == gridPos)
            {
                HistoryService?.RecordState(CaptureSnapshot());
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

        bool hasLinkedGroupColor = false;
        Color linkedGroupColor = Color.white;

        bool willPaint = sb != null;
        if (!willPaint)
        {
            foreach (Transform child in levelContainer)
            {
                if (Mathf.RoundToInt(child.position.x) == gridPos.x && Mathf.RoundToInt(child.position.y) == gridPos.y)
                {
                    willPaint = true;
                    break;
                }
            }
        }
        if (!willPaint)
        {
            for (int i = 0; i < currentDraftPortals.Count; i++)
            {
                if (currentDraftPortals[i].entrance == gridPos || currentDraftPortals[i].exit == gridPos)
                {
                    willPaint = true;
                    break;
                }
            }
        }
        if (!willPaint)
        {
            for (int i = 0; i < currentDraftElectricWalls.Count; i++)
            {
                if (IsCellOnElectricWall(gridPos, currentDraftElectricWalls[i]))
                {
                    willPaint = true;
                    break;
                }
            }
        }

        if (willPaint)
        {
            HistoryService?.RecordState(CaptureSnapshot());
        }

        foreach (Transform child in levelContainer)
        {
            if (Mathf.RoundToInt(child.position.x) == gridPos.x && Mathf.RoundToInt(child.position.y) == gridPos.y)
            {
                if (child.TryGetComponent(out GridTurnStateBlock turnStateBlock))
                {
                    turnStateBlock.SetInitialState(ShouldUseRedTurnState());
                    return;
                }

                if (child.TryGetComponent(out GridKeycard k))
                {
                    hasLinkedGroupColor = true;
                    linkedGroupColor = k.keyColor;
                    break;
                }

                if (child.TryGetComponent(out GridElectricButton eb))
                {
                    hasLinkedGroupColor = true;
                    linkedGroupColor = eb.buttonColor;
                    break;
                }

                if (child.TryGetComponent(out GridRevealWaveButton revealWaveButton))
                {
                    revealWaveButton.SetColor(currentColor);
                    return;
                }

                if (child.TryGetComponent(out GridLaserGate g))
                {
                    hasLinkedGroupColor = true;
                    linkedGroupColor = g.gateColor;
                    break;
                }

                GridElectricWall ew = child.GetComponent<GridElectricWall>();
                if (ew != null)
                {
                    hasLinkedGroupColor = true;
                    linkedGroupColor = ew.wallColor;
                    break;
                }
            }
        }

        if (hasLinkedGroupColor)
        {
            foreach (Transform child in levelContainer)
            {
                if (child.TryGetComponent(out GridKeycard k) && AreColorsEquivalent(k.keyColor, linkedGroupColor))
                {
                    k.keyColor = currentColor;
                    SpriteRenderer keySr = child.GetComponent<SpriteRenderer>();
                    if (keySr != null) keySr.color = currentColor;
                }

                if (child.TryGetComponent(out GridElectricButton eb) && AreColorsEquivalent(eb.buttonColor, linkedGroupColor))
                {
                    eb.SetColor(currentColor);
                }

                if (child.TryGetComponent(out GridLaserGate g) && AreColorsEquivalent(g.gateColor, linkedGroupColor))
                {
                    g.gateColor = currentColor;
                    SpriteRenderer gateSr = child.GetComponent<SpriteRenderer>();
                    if (gateSr != null) gateSr.color = currentColor;
                }

                GridElectricWall ew = child.GetComponent<GridElectricWall>();
                if (ew != null && AreColorsEquivalent(ew.wallColor, linkedGroupColor))
                {
                    ew.SetColor(currentColor);
                }
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

        for (int i = 0; i < currentDraftElectricWalls.Count; i++)
        {
            if (IsCellOnElectricWall(gridPos, currentDraftElectricWalls[i]))
            {
                ElectricWallSaveData wall = currentDraftElectricWalls[i];
                wall.color = currentColor;
                currentDraftElectricWalls[i] = wall;
                RefreshElectricWallVisuals();
                return;
            }
        }
    }

    private static bool AreColorsEquivalent(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.01f
            && Mathf.Abs(a.g - b.g) < 0.01f
            && Mathf.Abs(a.b - b.b) < 0.01f
            && Mathf.Abs(a.a - b.a) < 0.01f;
    }

    private void HandleSelectClick()
    {
        Vector2Int gridPos = GetMouseGridPosition();

        // 1. Check if it's a snake/arrow
        EditorSnakeVisual sb = GetSnakeAtGridPos(gridPos);
        if (sb != null)
        {
            selectedSnakeToModify = sb;
            currentDir = sb.direction;
            currentColor = sb.snakeColor;
            UpdateToolText();
            if (colorPreviewImage != null) colorPreviewImage.color = currentColor;
            UpdateSelectionHighlight(sb);
            
            // Automatically switch to Draw tool for editing!
            UI_SetTool((int)EditorToolType.Draw);
            return;
        }

        // 2. Check other object types under levelContainer
        foreach (Transform child in levelContainer)
        {
            if (Mathf.RoundToInt(child.position.x) == gridPos.x && Mathf.RoundToInt(child.position.y) == gridPos.y)
            {
                if (child.GetComponent<GridKeycard>() != null) { UI_SetTool((int)EditorToolType.Keycard); return; }
                if (child.GetComponent<GridLaserGate>() != null) { UI_SetTool((int)EditorToolType.Gate); return; }
                if (child.GetComponentInChildren<GridDeflector>() != null) { UI_SetTool((int)EditorToolType.Deflector); return; }
                if (child.GetComponent<GridCountdownBlock>() != null) { UI_SetTool((int)EditorToolType.CountdownBlock); return; }
                if (child.GetComponent<GridElectricButton>() != null) { UI_SetTool((int)EditorToolType.ElectricButton); return; }
                if (child.GetComponent<GridStopBlock>() != null) { UI_SetTool((int)EditorToolType.StopBlock); return; }
                if (child.GetComponent<GridTurnStateBlock>() != null) { UI_SetTool((int)EditorToolType.TurnStateBlock); return; }
                if (child.GetComponent<GridBlackHole>() != null) { UI_SetTool((int)EditorToolType.BlackHole); return; }
                if (child.GetComponent<GridRevealWaveButton>() != null) { UI_SetTool((int)EditorToolType.RevealWaveButton); return; }
            }

            // ElectricWall check
            GridElectricWall ew = child.GetComponent<GridElectricWall>();
            if (ew != null && ew.ContainsCell(gridPos))
            {
                UI_SetTool((int)EditorToolType.ElectricWall);
                return;
            }
        }

        // 3. Check Portals (Draft)
        for (int i = 0; i < currentDraftPortals.Count; i++)
        {
            if (currentDraftPortals[i].entrance == gridPos || currentDraftPortals[i].exit == gridPos)
            {
                UI_SetTool((int)EditorToolType.Portal);
                return;
            }
        }

        // 4. Check ElectricWalls (Draft)
        for (int i = 0; i < currentDraftElectricWalls.Count; i++)
        {
            if (IsCellOnElectricWall(gridPos, currentDraftElectricWalls[i]))
            {
                UI_SetTool((int)EditorToolType.ElectricWall);
                return;
            }
        }

        // If clicked on empty space, deselect
        selectedSnakeToModify = null; 
        ClearSelectionHighlight();
    }

    private void HandlePortalClick()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        if (IsPositionOccupied(gridPos)) return;

        if (!isPlacingPortalExit)
        {
            draftPortalEntrance = gridPos;
            draftPortalEntranceDir = currentDir;
            draftPortalColor = currentColor;
            isPlacingPortalExit = true;
        }
        else
        {
            if (gridPos == draftPortalEntrance) return;
            HistoryService?.RecordState(CaptureSnapshot());
            PortalData newPortal = new PortalData();
            newPortal.entrance = draftPortalEntrance;
            newPortal.entranceDir = draftPortalEntranceDir;
            newPortal.exit = gridPos;
            newPortal.exitDir = currentDir;
            newPortal.portalColor = draftPortalColor;
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
            GameObject inObj = Instantiate(portalPrefab, new Vector3(p.entrance.x, p.entrance.y, 0), GetRotationForDir(p.entranceDir), levelContainer);
            GameObject outObj = Instantiate(portalPrefab, new Vector3(p.exit.x, p.exit.y, 0), GetRotationForDir(p.exitDir), levelContainer);
            SpriteRenderer inSr = inObj.GetComponent<SpriteRenderer>();
            if(inSr) inSr.color = p.portalColor;
            SpriteRenderer outSr = outObj.GetComponent<SpriteRenderer>();
            if(outSr) outSr.color = p.portalColor;
            spawnedPortalVisuals.Add(inObj);
            spawnedPortalVisuals.Add(outObj);
        }
    }

    private void HandleElectricWallClick()
    {
        Vector2Int gridPos = GetMouseGridPosition();

        if (!isPlacingElectricWallEnd)
        {
            if (IsPositionOccupied(gridPos)) return;
            draftElectricWallStart = gridPos;
            draftElectricWallColor = currentColor;
            isPlacingElectricWallEnd = true;
        }
        else
        {
            if (gridPos == draftElectricWallStart) return;
            if (!IsElectricWallAligned(draftElectricWallStart, gridPos)) { isPlacingElectricWallEnd = false; return; }
            if (!IsElectricWallPathClear(draftElectricWallStart, gridPos)) { isPlacingElectricWallEnd = false; return; }

            HistoryService?.RecordState(CaptureSnapshot());
            ElectricWallSaveData newWall = new ElectricWallSaveData
            {
                start = draftElectricWallStart,
                end = gridPos,
                color = draftElectricWallColor
            };

            currentDraftElectricWalls.Add(newWall);
            isPlacingElectricWallEnd = false;
            RefreshElectricWallVisuals();
        }

        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    private void RefreshElectricWallVisuals()
    {
        foreach (var obj in spawnedElectricWallVisuals) Destroy(obj);
        spawnedElectricWallVisuals.Clear();
        if (electricWallPrefab == null) return;

        for (int i = 0; i < currentDraftElectricWalls.Count; i++)
        {
            ElectricWallSaveData w = currentDraftElectricWalls[i];
            GameObject obj = Instantiate(electricWallPrefab, Vector3.zero, Quaternion.identity, levelContainer);
            GridElectricWall wall = obj.GetComponent<GridElectricWall>();
            if (wall != null) wall.Initialize(w.start, w.end, w.color, false);
            spawnedElectricWallVisuals.Add(obj);
        }
    }

    private bool TryRemoveElectricWallAtPos(Vector2Int gridPos)
    {
        for (int i = currentDraftElectricWalls.Count - 1; i >= 0; i--)
        {
            if (IsCellOnElectricWall(gridPos, currentDraftElectricWalls[i]))
            {
                HistoryService?.RecordState(CaptureSnapshot());
                currentDraftElectricWalls.RemoveAt(i);
                RefreshElectricWallVisuals();
                return true;
            }
        }
        return false;
    }

    private static bool IsElectricWallAligned(Vector2Int start, Vector2Int end)
    {
        return LevelEditorRuntimeHelpers.IsElectricWallAligned(start, end);
    }

    private bool IsElectricWallPathClear(Vector2Int start, Vector2Int end)
    {
        if (!IsElectricWallAligned(start, end)) return false;

        int stepX = start.x == end.x ? 0 : (start.x < end.x ? 1 : -1);
        int stepY = start.y == end.y ? 0 : (start.y < end.y ? 1 : -1);

        int length = Mathf.Max(Mathf.Abs(end.x - start.x), Mathf.Abs(end.y - start.y));
        for (int i = 0; i <= length; i++)
        {
            Vector2Int cell = new Vector2Int(start.x + stepX * i, start.y + stepY * i);
            if (IsPositionOccupied(cell)) return false;
        }
        return true;
    }

    private static bool IsCellOnElectricWall(Vector2Int cell, ElectricWallSaveData wall)
    {
        return LevelEditorRuntimeHelpers.IsCellOnElectricWall(cell, wall);
    }

    private static Quaternion GetRotationForDir(ArrowDir dir)
    {
        return LevelEditorRuntimeHelpers.GetRotationForDir(dir);
    }

    private void UpdateSelectionHighlight(EditorSnakeVisual target)
    {
        ClearSelectionHighlight();
        if (selectionGlowPrefab == null) return;
        EnsureSelectionOverlayContainer();
        currentSelectionGlowObj = Instantiate(selectionGlowPrefab, target.transform.position, Quaternion.identity, selectionOverlayContainer);
        currentSelectionGlowScript = currentSelectionGlowObj.GetComponent<EditorSnakeVisual>();
        if (currentSelectionGlowScript != null)
        {
            currentSelectionGlowScript.Initialize(target.direction, new List<Vector2Int>(target.LogicNodes), highlightColor);
            currentSelectionGlowObj.transform.position += new Vector3(0, 0, 0.01f);
        }
    }

    private void ClearSelectionHighlight()
    {
        if (currentSelectionGlowObj != null)
        {
            // Detach and deactivate first so SaveLevel() can't accidentally serialize it in the same frame
            // (Destroy() is end-of-frame in play mode).
            currentSelectionGlowObj.transform.SetParent(null, true);
            currentSelectionGlowObj.SetActive(false);
            Destroy(currentSelectionGlowObj);
            currentSelectionGlowObj = null;
            currentSelectionGlowScript = null;
        }
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

    private void CreateHeadSegment(Vector2Int pos)
    {
        currentDraftNodes.Insert(0, pos);
        if (currentSnakeScript != null)
        {
            currentSnakeScript.SetArrowWorldPosition(pos);
        }
        if (currentSnakeObj != null)
        {
            currentSnakeObj.transform.position = new Vector3(pos.x, pos.y, 0f);
        }
        UpdateSnakeLinePreview();
    }

    private void RetractHeadSegment()
    {
        if (currentDraftNodes == null || currentDraftNodes.Count == 0) return;

        currentDraftNodes.RemoveAt(0);
        if (currentDraftNodes.Count == 0)
        {
            if (currentSnakeObj != null) Destroy(currentSnakeObj);
            currentSnakeObj = null;
            currentSnakeScript = null;
        }
        else
        {
            Vector2Int newHead = currentDraftNodes[0];
            if (currentSnakeScript != null)
            {
                currentSnakeScript.SetArrowWorldPosition(newHead);
            }
            if (currentSnakeObj != null)
            {
                currentSnakeObj.transform.position = new Vector3(newHead.x, newHead.y, 0f);
            }
            UpdateSnakeLinePreview();
            UpdateAutoDirection();
        }
        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    private void RetractTailSegment()
    {
        if (currentDraftNodes == null || currentDraftNodes.Count == 0) return;

        currentDraftNodes.RemoveAt(currentDraftNodes.Count - 1);
        if (currentDraftNodes.Count == 0)
        {
            if (currentSnakeObj != null) Destroy(currentSnakeObj);
            currentSnakeObj = null;
            currentSnakeScript = null;
        }
        else
        {
            UpdateSnakeLinePreview();
            UpdateAutoDirection();
        }
        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    public void UndoLastSegment()
    {
        if (currentTool == EditorToolType.Portal && isPlacingPortalExit)
        {
            isPlacingPortalExit = false;
            lastCalculatedGridPos = new Vector2Int(-9999, -9999);
            return;
        }

        if (currentTool == EditorToolType.ElectricWall && isPlacingElectricWallEnd)
        {
            isPlacingElectricWallEnd = false;
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
        else if (currentTool == EditorToolType.ArrowShadow) previewCursor.color = new Color(0.15f, 0.75f, 1f, 0.45f);
        else if (currentTool == EditorToolType.TurnStateBlock) previewCursor.color = ShouldUseRedTurnState() ? new Color(1f, 0.1f, 0.1f, 0.8f) : new Color(0.1f, 1f, 0.35f, 0.8f);
        else if (currentTool == EditorToolType.BlackHole) previewCursor.color = new Color(0.05f, 0.05f, 0.08f, 0.75f);
        else if (currentTool == EditorToolType.RevealWaveButton) previewCursor.color = new Color(0.35f, 0.9f, 1f, 0.8f);
        else if (currentTool == EditorToolType.Portal || currentTool == EditorToolType.Keycard || currentTool == EditorToolType.Gate || currentTool == EditorToolType.Deflector || currentTool == EditorToolType.CountdownBlock || currentTool == EditorToolType.ElectricButton || currentTool == EditorToolType.ElectricWall || currentTool == EditorToolType.StopBlock) 
            previewCursor.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.8f);
    }

    private Vector2Int GetMouseGridPosition()
    {
        Camera camera = GetCameraInMyScene();
        if (camera == null)
        {
            return lastCalculatedGridPos;
        }

        Vector3 pos = camera.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
    }

    public void UI_SaveLevel() { SaveLevel(); }
    public void UI_LoadLevel() { LoadLevelToEdit(); }
    public void UI_CheckDeadlock() { CheckDeadlockInCurrentEditorLevel(true, out _); }

    public LevelDataV2 PrepareCurrentLevelForPlaytest()
    {
        return playtestBridge.PrepareCurrentLevelForPlaytest(
            CreateContext(),
            isPlacingPortalExit,
            isPlacingElectricWallEnd,
            UI_FinishSnake,
            () => isPlacingPortalExit = false,
            () => isPlacingElectricWallEnd = false,
            SaveLevel);
    }

    public void UI_Playtest()
    {
        LevelDataV2 levelData = PrepareCurrentLevelForPlaytest();
        if (levelData == null)
        {
            Debug.LogWarning("[LevelEditor] UI_Playtest aborted: currentData is null. Create a LevelData V2 asset via Create > ArrowPuzzle > LevelData V2, then assign it to LevelEditor.currentData before pressing F5.");
            return;
        }

        playtestBridge.StartPlaytest(
            levelData,
            SceneManager.GetActiveScene().name,
            SceneManager.GetActiveScene().path,
            "GameScene",
            GameScenePath);
    }

    private void RunContinuousDeadlockCheck()
    {
        if (!checkDeadlockContinuously) return;

        float interval = Mathf.Max(0.05f, deadlockCheckInterval);
        if (Time.unscaledTime < _nextDeadlockCheckTime) return;

        _nextDeadlockCheckTime = Time.unscaledTime + interval;

        bool hasDeadlock = CheckDeadlockInCurrentEditorLevel(false, out string message);
        bool stateChanged = !_hasContinuousDeadlockState || _lastContinuousDeadlockState != hasDeadlock;
        bool deadlockMessageChanged = hasDeadlock && _lastContinuousDeadlockMessage != message;

        if (hasDeadlock && (stateChanged || deadlockMessageChanged))
        {
            Debug.LogWarning($"[LevelEditor] {message}");
        }
        else if (!hasDeadlock && _hasContinuousDeadlockState && _lastContinuousDeadlockState)
        {
            Debug.Log($"[LevelEditor] Deadlock cleared. {message}");
        }

        _hasContinuousDeadlockState = true;
        _lastContinuousDeadlockState = hasDeadlock;
        _lastContinuousDeadlockMessage = message;
    }

    private bool CheckDeadlockInCurrentEditorLevel(bool logResult, out string resultMessage)
    {
        LevelEditorDeadlockState state = deadlockStateBuilder.Build(CreateContext());
        LevelEditorDeadlockResult result = deadlockValidator.Validate(state, deadlockScanLimit);
        resultMessage = result.message;

        if (logResult)
        {
            if (result.hasDeadlock)
            {
                Debug.LogWarning($"[LevelEditor] {resultMessage}");
            }
            else
            {
                Debug.Log($"[LevelEditor] {resultMessage}");
            }
        }

        return result.hasDeadlock;
    }

    private void SaveLevel()
    {
        ClearSelectionHighlight();
        if (editingData != null && currentData != null)
        {
            serializer.SyncToData(CreateContext());
            LevelDataV2Cloner.CopyData(editingData, currentData);
        }
        LevelEditorContext saveContext = CreateContext();
        saveContext.currentData = currentData;
        serializer.Save(saveContext);
    }

    private void LoadLevelToEdit()
    {
        ClearSelectionHighlight();
        if (currentData != null)
        {
            editingData = LevelDataV2Cloner.Clone(currentData);
        }
        else
        {
            editingData = null;
        }
        serializer.Load(CreateContext());
        FrameLevelInEditorCamera();
    }

    private void FrameLevelInEditorCamera()
    {
        if (!frameLevelOnLoad || currentData == null)
        {
            return;
        }

        Camera camera = GetCameraInMyScene();
        if (camera == null)
        {
            return;
        }

        if (!TryGetCurrentLevelBounds(out Vector2 min, out Vector2 max))
        {
            return;
        }

        Vector2 center = (min + max) * 0.5f;
        Vector3 cameraPosition = camera.transform.position;
        camera.transform.position = new Vector3(center.x, center.y, cameraPosition.z);

        if (camera.orthographic)
        {
            float paddedWidth = Mathf.Max(1f, max.x - min.x + 1f + levelFramePadding * 2f);
            float paddedHeight = Mathf.Max(1f, max.y - min.y + 1f + levelFramePadding * 2f);
            float aspect = Mathf.Max(0.1f, camera.aspect);
            camera.orthographicSize = Mathf.Max(minimumEditorCameraSize, paddedHeight * 0.5f, paddedWidth * 0.5f / aspect);
        }

        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    private bool TryGetCurrentLevelBounds(out Vector2 min, out Vector2 max)
    {
        min = Vector2.zero;
        max = Vector2.zero;
        if (currentData == null || !LevelDataV2Queries.TryGetBounds(currentData, out Bounds bounds))
        {
            return false;
        }

        Vector3 boundsMin = bounds.min;
        Vector3 boundsMax = bounds.max;
        min = new Vector2(boundsMin.x, boundsMin.y);
        max = new Vector2(boundsMax.x, boundsMax.y);
        return true;
    }

    private static void AddBoundsPoint(Vector2Int point, ref Vector2 min, ref Vector2 max, ref bool hasPosition)
    {
        if (!hasPosition)
        {
            min = point;
            max = point;
            hasPosition = true;
            return;
        }

        min = Vector2.Min(min, point);
        max = Vector2.Max(max, point);
    }

    public void SetCurrentDataAndLoad(LevelDataV2 data)
    {
        currentData = data;
        LoadLevelToEdit();
    }

    public string BuildEditorStateDigest()
    {
        return LevelEditorStateDigest.Build(CreateContext(), editingData != null ? editingData : currentData);
    }

    public LevelDataV2 GetCurrentLevelData()
    {
        return currentData;
    }

    public void SetCurrentLevelKey(string key)
    {
        if (editingData != null)
        {
            editingData.name = key;
        }
        if (currentData != null)
        {
            currentData.name = key;
        }
    }

    public void ClearCurrentLevelData()
    {
        if (levelContainer != null)
        {
            foreach (Transform child in levelContainer)
            {
                Destroy(child.gameObject);
            }
        }
        currentDraftNodes.Clear();
        finishedSnakesHistory.Clear();
        currentDraftPortals.Clear();
        spawnedPortalVisuals.Clear();
        currentDraftElectricWalls.Clear();
        spawnedElectricWallVisuals.Clear();
        isPlacingPortalExit = false;
        isPlacingElectricWallEnd = false;
        selectedSnakeToModify = null;
        ClearSelectionHighlight();
        currentSnakeObj = null;
        currentSnakeScript = null;
        
        if (editingData != null)
        {
            editingData.timeLimit = 0f;
            editingData.rewardCoins = 0f;
            editingData.rewardDiamonds = 0f;
            editingData.arrows.Clear();
            editingData.cells.Clear();
            editingData.links.Clear();
        }
    }

    public void SetCurrentDifficulty(LevelDifficulty difficulty)
    {
        if (editingData != null)
        {
            editingData.levelDifficulty = difficulty;
        }
        if (currentData != null)
        {
            currentData.levelDifficulty = difficulty;
        }
        OnManipulationComplete();
    }

    public bool HasEditableContent()
    {
        if (levelContainer != null && levelContainer.childCount > 0)
        {
            foreach (Transform child in levelContainer)
            {
                if (currentSelectionGlowObj != null && child.gameObject == currentSelectionGlowObj) continue;
                return true;
            }
        }
        if (currentDraftNodes != null && currentDraftNodes.Count > 0) return true;
        if (currentDraftPortals != null && currentDraftPortals.Count > 0) return true;
        if (currentDraftElectricWalls != null && currentDraftElectricWalls.Count > 0) return true;
        return false;
    }

    public EditorSnapshot CaptureSnapshot()
    {
        serializer.SyncToData(CreateContext());

        EditorSnapshot snapshot = new EditorSnapshot();
        if (editingData != null)
        {
            snapshot.levelDataClone = LevelDataV2Cloner.Clone(editingData);
            snapshot.levelKey = editingData.name;
            snapshot.difficulty = editingData.levelDifficulty;
        }
        else if (currentData != null)
        {
            snapshot.levelDataClone = LevelDataV2Cloner.Clone(currentData);
            snapshot.levelKey = currentData.name;
            snapshot.difficulty = currentData.levelDifficulty;
        }

        snapshot.currentTool = currentTool;
        snapshot.currentDir = currentDir;
        snapshot.currentColor = currentColor;

        snapshot.currentDraftNodes = new List<Vector2Int>(currentDraftNodes);
        
        snapshot.currentDraftPortals = new List<PortalData>();
        if (currentDraftPortals != null)
        {
            foreach (var p in currentDraftPortals)
            {
                snapshot.currentDraftPortals.Add(new PortalData
                {
                    entrance = p.entrance,
                    entranceDir = p.entranceDir,
                    exit = p.exit,
                    exitDir = p.exitDir,
                    portalColor = p.portalColor
                });
            }
        }

        snapshot.currentDraftElectricWalls = new List<ElectricWallSaveData>();
        if (currentDraftElectricWalls != null)
        {
            foreach (var w in currentDraftElectricWalls)
            {
                snapshot.currentDraftElectricWalls.Add(new ElectricWallSaveData
                {
                    start = w.start,
                    end = w.end,
                    color = w.color
                });
            }
        }

        snapshot.isPlacingPortalExit = isPlacingPortalExit;
        snapshot.draftPortalEntrance = draftPortalEntrance;
        snapshot.draftPortalEntranceDir = draftPortalEntranceDir;
        snapshot.draftPortalColor = draftPortalColor;

        snapshot.isPlacingElectricWallEnd = isPlacingElectricWallEnd;
        snapshot.draftElectricWallStart = draftElectricWallStart;
        snapshot.draftElectricWallColor = draftElectricWallColor;

        return snapshot;
    }

    public void RestoreSnapshot(EditorSnapshot snapshot)
    {
        if (snapshot == null) return;

        if (snapshot.levelDataClone != null && editingData != null)
        {
            LevelDataV2Cloner.CopyData(snapshot.levelDataClone, editingData);
            editingData.name = snapshot.levelKey;
            editingData.levelDifficulty = snapshot.difficulty;
        }

        currentTool = snapshot.currentTool;
        currentDir = snapshot.currentDir;
        currentColor = snapshot.currentColor;

        currentDraftNodes = new List<Vector2Int>(snapshot.currentDraftNodes);

        currentDraftPortals = new List<PortalData>();
        if (snapshot.currentDraftPortals != null)
        {
            foreach (var p in snapshot.currentDraftPortals)
            {
                currentDraftPortals.Add(new PortalData
                {
                    entrance = p.entrance,
                    entranceDir = p.entranceDir,
                    exit = p.exit,
                    exitDir = p.exitDir,
                    portalColor = p.portalColor
                });
            }
        }

        currentDraftElectricWalls = new List<ElectricWallSaveData>();
        if (snapshot.currentDraftElectricWalls != null)
        {
            foreach (var w in snapshot.currentDraftElectricWalls)
            {
                currentDraftElectricWalls.Add(new ElectricWallSaveData
                {
                    start = w.start,
                    end = w.end,
                    color = w.color
                });
            }
        }

        isPlacingPortalExit = snapshot.isPlacingPortalExit;
        draftPortalEntrance = snapshot.draftPortalEntrance;
        draftPortalEntranceDir = snapshot.draftPortalEntranceDir;
        draftPortalColor = snapshot.draftPortalColor;

        isPlacingElectricWallEnd = snapshot.isPlacingElectricWallEnd;
        draftElectricWallStart = snapshot.draftElectricWallStart;
        draftElectricWallColor = snapshot.draftElectricWallColor;

        if (currentSnakeObj != null)
        {
            Destroy(currentSnakeObj);
            currentSnakeObj = null;
            currentSnakeScript = null;
        }

        ClearSelectionHighlight();
        serializer.Load(CreateContext());
        RefreshPortalVisuals();
        RefreshElectricWallVisuals();

        if (currentDraftNodes.Count > 0)
        {
            List<Vector2Int> tempNodes = new List<Vector2Int>(currentDraftNodes);
            CreateHead(tempNodes[0]);
            for (int i = 1; i < tempNodes.Count; i++)
            {
                CreateBodySegment(tempNodes[i]);
            }
        }

        if (colorPreviewImage != null) colorPreviewImage.color = currentColor;
        UpdateToolText();
        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    public bool TryValidateCurrentEditorLevel(out string message)
    {
        bool hasDeadlock = CheckDeadlockInCurrentEditorLevel(false, out message);
        return !hasDeadlock;
    }

    public void SaveCurrentEditorLevel()
    {
        SaveLevel();
    }

    private void OnManipulationComplete()
    {
        if (serializer != null)
        {
            serializer.SyncToData(CreateContext());
        }
    }
}
