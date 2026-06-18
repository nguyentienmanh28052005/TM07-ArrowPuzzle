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
            currentData = currentData,
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
        HandleKeyboardShortcuts();
        UpdatePreviewCursor();
        RunContinuousDeadlockCheck();
        HandleMouseInput();
    }

    private void HandleKeyboardShortcuts()
    {
        if (Input.GetKeyDown(KeyCode.F5)) UI_Playtest();
        if (Input.GetKeyDown(KeyCode.F6)) UI_CheckDeadlock();

        if (Input.GetKeyDown(KeyCode.Alpha1)) UI_SetTool(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) UI_SetTool(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UI_SetTool(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) UI_SetTool(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) UI_SetTool(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) UI_SetTool(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) UI_SetTool(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) UI_SetTool(7);
        if (Input.GetKeyDown(KeyCode.Alpha9)) UI_SetTool(8);
        if (Input.GetKeyDown(KeyCode.Alpha0)) UI_SetTool((int)EditorToolType.StopBlock);
        if (Input.GetKeyDown(KeyCode.B)) UI_SetTool((int)EditorToolType.ArrowShadow);
        if (Input.GetKeyDown(KeyCode.T)) UI_SetTool((int)EditorToolType.TurnStateBlock);
        if (Input.GetKeyDown(KeyCode.H)) UI_SetTool((int)EditorToolType.BlackHole);
        if (Input.GetKeyDown(KeyCode.V)) UI_SetTool((int)EditorToolType.RevealWaveButton);

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) UI_SetDirection(0);
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) UI_SetDirection(1);
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) UI_SetDirection(2);
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) UI_SetDirection(3);

        if (Input.GetKeyDown(KeyCode.Space)) UI_FinishSnake();
        if (Input.GetKeyDown(KeyCode.R)) RotateDirection();
        if (Input.GetKeyDown(KeyCode.Z)) UndoLastSegment();
    }

    private void HandleMouseInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandlePrimaryMouseDown();
            return;
        }

        if (Input.GetMouseButton(0))
        {
            HandlePrimaryMouseHold();
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
    }

    private void HandlePrimaryMouseHold()
    {
        if (currentTool == EditorToolType.Draw) HandleLeftDrag();
        else if (currentTool == EditorToolType.Erase) HandleEraseClick();
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
        currentTool = (EditorToolType)toolIndex; 
        UpdateToolText(); 
        if (currentTool != EditorToolType.Select) ClearSelectionHighlight();
        if (currentTool != EditorToolType.Portal) isPlacingPortalExit = false;
        if (currentTool != EditorToolType.ElectricWall) isPlacingElectricWallEnd = false;
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
        if (obj.TryGetComponent(out GridElectricButton eb)) eb.SetColor(currentColor);
        if (obj.TryGetComponent(out GridRevealWaveButton rwb)) rwb.SetColor(currentColor);
        
        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    private void HandleDeflectorPlacement()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        if (IsPositionOccupied(gridPos) || deflectorPrefab == null) return;

        GameObject obj = Instantiate(deflectorPrefab, new Vector3(gridPos.x, gridPos.y, 0), GetRotationForDir(currentDir), levelContainer);
        GridDeflector deflector = obj.GetComponentInChildren<GridDeflector>();
        if (deflector != null) deflector.SetDirection(currentDir);

        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    private void HandleCountdownBlockPlacement()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        if (IsPositionOccupied(gridPos) || countdownBlockPrefab == null) return;

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

        snake.SetArrowShadowEnabled(!snake.HasArrowShadow);
        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    private void HandleTurnStateBlockPlacement()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        if (IsPositionOccupied(gridPos) || turnStateBlockPrefab == null) return;

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
                existingBlackHole.SetDirection(currentDir);
                child.rotation = GetRotationForDir(currentDir);
                lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                return;
            }
        }

        if (IsPositionOccupied(gridPos) || blackHolePrefab == null) return;

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
            draftPortalColor = currentColor;
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
        serializer.Save(CreateContext());
    }

    private void LoadLevelToEdit()
    {
        ClearSelectionHighlight();
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
}
