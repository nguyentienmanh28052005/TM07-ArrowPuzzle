using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

// public enum EditorToolType { Draw, Erase, Paint, Select, Portal, KeycardGate, Deflector, CountdownBlock, ElectricCircuit, StopBlock, ArrowShadow, TurnStateBlock, BlackHole, RevealWaveButton }

public partial class LevelEditorWorkspace : MonoBehaviour
{
    private readonly Dictionary<EditorToolType, EditorStateBase> stateCache = new Dictionary<EditorToolType, EditorStateBase>();

    public EditorStateBase GetCachedState(EditorToolType toolType)
    {
        if (stateCache.TryGetValue(toolType, out EditorStateBase state)) return state;
        return null;
    }

    private void InitializeStateCache()
    {
        stateCache[EditorToolType.Draw] = new DrawSnakeState(this);
        stateCache[EditorToolType.Erase] = new EraseState(this);
        stateCache[EditorToolType.Paint] = new PaintState(this);
        stateCache[EditorToolType.Select] = new SelectState(this);
        stateCache[EditorToolType.Portal] = new PlacePortalState(this);
        stateCache[EditorToolType.KeycardGate] = new PlaceKeycardGateState(this);
        stateCache[EditorToolType.Deflector] = new PlaceDeflectorState(this);
        stateCache[EditorToolType.CountdownBlock] = new PlaceCountdownState(this);
        stateCache[EditorToolType.StopBlock] = new PlaceStopBlockState(this);
        stateCache[EditorToolType.TurnStateBlock] = new PlaceTurnStateBlockState(this);
        stateCache[EditorToolType.BlackHole] = new PlaceBlackHoleState(this);
        stateCache[EditorToolType.RevealWaveButton] = new PlaceRevealWaveButtonState(this);
        stateCache[EditorToolType.ElectricCircuit] = new PlaceElectricCircuitState(this);
        stateCache[EditorToolType.ArrowShadow] = new PlaceArrowShadowState(this);
    }

    public static LevelEditorWorkspace Instance { get; private set; }

    public LevelEditorObjectPool Pool { get; private set; }

    [Header("Module References")]
    [SerializeField] public LevelSelectorPopupView levelSelectorView;
    [SerializeField] public EditorCameraController cameraController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Pool = new LevelEditorObjectPool();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        if (activePreviewObject != null)
        {
            Destroy(activePreviewObject);
            activePreviewObject = null;
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
    private static LevelDataV2 tempEditingData;

    [Header("Preview & Tools")]
    public ColorPaletteAsset editorPalette;
    public SpriteRenderer previewCursor;
    public EditorToolType currentTool = EditorToolType.Draw;
    [SerializeField] private TextMeshProUGUI textCurrentTool;
    public ArrowDir currentDir = ArrowDir.Up;
    public Color currentColor = Color.white;
    public Image colorPreviewImage;

    [Header("Validation Settings")]
    [SerializeField] private bool checkDeadlockContinuously = true;
    [SerializeField, Min(0.05f)] private float deadlockCheckInterval = 0.5f;
    [SerializeField] private int deadlockScanLimit = 512;

    [Header("Metadata UI")]
    public TMP_InputField inputTimeLimit;
    public TMP_InputField inputRewardCoins;
    public TMP_InputField inputRewardDiamonds;

    [Header("Countdown Block")]
    public int editorCountdownValue = 3;
    public TMP_InputField inputCountdownValue;

    internal GameObject currentSnakeObj;
    internal EditorSnakeVisual currentSnakeScript;
    internal EditorSnakeVisual selectedSnakeToModify;
    internal GameObject currentSelectionGlowObj; 
    internal EditorSnakeVisual currentSelectionGlowScript;
    internal Transform selectionOverlayContainer;
    
    internal List<Vector2Int> currentDraftNodes = new List<Vector2Int>();
    internal Stack<GameObject> finishedSnakesHistory = new Stack<GameObject>();

    internal bool isPlacingPortalExit = false;
    internal Vector2Int draftPortalEntrance;
    internal ArrowDir draftPortalEntranceDir;
    internal Color draftPortalColor = Color.white;
    internal List<PortalData> currentDraftPortals = new List<PortalData>();
    internal List<GameObject> spawnedPortalVisuals = new List<GameObject>();

    internal bool isPlacingElectricWallEnd = false;
    internal Vector2Int draftElectricWallStart;
    internal Color draftElectricWallColor = Color.white;
    internal List<ElectricWallSaveData> currentDraftElectricWalls = new List<ElectricWallSaveData>();
    internal List<GameObject> spawnedElectricWallVisuals = new List<GameObject>();

    private Camera mainCam;
    internal Vector2Int lastCalculatedGridPos = new Vector2Int(-9999, -9999);
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

    private GameObject ghostPortalPreview;
    private GameObject ghostPortalEntrancePreview;
    private GameObject ghostElectricWallPreview;
    private GameObject activePreviewObject;
    private EditorToolType lastObservedPreviewTool = (EditorToolType)(-1);

    internal EditorStateBase currentState;
    private string lastSavedDigest = string.Empty;
    private readonly Dictionary<Vector2Int, GameObject> gridOccupantsCache = new Dictionary<Vector2Int, GameObject>();

    public bool IsPlacingPortalExit => isPlacingPortalExit;
    public bool IsPlacingElectricWallEnd => isPlacingElectricWallEnd;

    public event Action<Color> OnColorChanged;

    private void Start()
    {
        if (editorPalette == null)
        {
            editorPalette = Resources.Load<ColorPaletteAsset>("Palettes/DefaultPalette");
        }
        InitializeStateCache();
        mainCam = GetCameraInMyScene();
        EnsureSelectionOverlayContainer();
        if (colorPreviewImage != null)
        {
            colorPreviewImage.color = currentColor;
        }
        if (levelSelectorView != null)
        {
            levelSelectorView.Initialize(this);
        }
        LoadLevelToEdit();
        if (stateCache.TryGetValue(EditorToolType.Draw, out EditorStateBase drawState))
        {
            currentState = drawState;
        }
        else
        {
            currentState = new DrawSnakeState(this);
        }
        currentState.OnEnter();
    }

    private void EnsureSelectionOverlayContainer()
    {
        if (selectionOverlayContainer != null) return;

        GameObject go = new GameObject("SelectionOverlay");
        selectionOverlayContainer = go.transform;
        selectionOverlayContainer.SetParent(transform, false);
        selectionOverlayContainer.localPosition = Vector3.zero;
        selectionOverlayContainer.localRotation = Quaternion.identity;
        selectionOverlayContainer.localScale = Vector3.one;
    }

    private Camera GetCameraInMyScene()
    {
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
            currentColor = currentColor,
            isPlacingPortalExit = isPlacingPortalExit,
            draftPortalEntrance = draftPortalEntrance,
            draftPortalEntranceDir = draftPortalEntranceDir,
            draftPortalColor = draftPortalColor,
            isPlacingElectricWallEnd = isPlacingElectricWallEnd,
            draftElectricWallStart = draftElectricWallStart,
            draftElectricWallColor = draftElectricWallColor
        };
    }

    private void UpdateGhostPreviews()
    {
        if (!isPlacingPortalExit)
        {
            if (ghostPortalPreview != null)
            {
                Destroy(ghostPortalPreview);
                ghostPortalPreview = null;
            }
            if (ghostPortalEntrancePreview != null)
            {
                Destroy(ghostPortalEntrancePreview);
                ghostPortalEntrancePreview = null;
            }
        }
        else
        {
            if (ghostPortalEntrancePreview == null && portalPrefab != null)
            {
                EnsureSelectionOverlayContainer();
                ghostPortalEntrancePreview = Instantiate(portalPrefab, selectionOverlayContainer);
            }

            if (ghostPortalEntrancePreview != null)
            {
                ghostPortalEntrancePreview.transform.position = new Vector3(draftPortalEntrance.x, draftPortalEntrance.y, -0.05f);
                ghostPortalEntrancePreview.transform.rotation = GetRotationForDir(draftPortalEntranceDir);
                
                SpriteRenderer sr = ghostPortalEntrancePreview.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color previewColor = draftPortalColor;
                    previewColor.a = 0.8f;
                    sr.color = previewColor;
                }
            }

            if (ghostPortalPreview == null && portalPrefab != null)
            {
                EnsureSelectionOverlayContainer();
                ghostPortalPreview = Instantiate(portalPrefab, selectionOverlayContainer);
            }

            if (ghostPortalPreview != null)
            {
                Vector2Int gridPos = GetMouseGridPosition();
                ghostPortalPreview.transform.position = new Vector3(gridPos.x, gridPos.y, -0.05f);
                ghostPortalPreview.transform.rotation = GetRotationForDir(currentDir);
                
                SpriteRenderer sr = ghostPortalPreview.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color previewColor = draftPortalColor;
                    previewColor.a = 0.4f;
                    sr.color = previewColor;
                }
            }
        }

        if (!isPlacingElectricWallEnd)
        {
            if (ghostElectricWallPreview != null)
            {
                Destroy(ghostElectricWallPreview);
                ghostElectricWallPreview = null;
            }
        }
        else
        {
            if (ghostElectricWallPreview == null && electricWallPrefab != null)
            {
                EnsureSelectionOverlayContainer();
                ghostElectricWallPreview = Instantiate(electricWallPrefab, selectionOverlayContainer);
            }

            if (ghostElectricWallPreview != null)
            {
                Vector2Int mousePos = GetMouseGridPosition();
                GridElectricWall wall = ghostElectricWallPreview.GetComponent<GridElectricWall>();
                if (wall != null)
                {
                    bool isValid = IsElectricWallAligned(draftElectricWallStart, mousePos) && IsElectricWallPathClear(draftElectricWallStart, mousePos);
                    Color previewColor = isValid ? draftElectricWallColor : Color.red;
                    previewColor.a = 0.5f;
                    
                    wall.Initialize(draftElectricWallStart, mousePos, previewColor, false);
                    
                    LightningConnector lightning = ghostElectricWallPreview.GetComponent<LightningConnector>();
                    if (lightning != null)
                    {
                        lightning.SetColor(previewColor);
                        lightning.SetActive(true);
                    }
                }
            }
        }
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        return Pool != null ? Pool.Spawn(prefab, position, rotation, parent) : null;
    }

    public void Recycle(GameObject obj)
    {
        if (Pool != null) Pool.Recycle(obj);
    }

    internal T PlaceGridObject<T>(GameObject prefab, Vector2Int gridPos) where T : MonoBehaviour
    {
        if (IsPositionOccupied(gridPos) || prefab == null) return null;

        HistoryService?.RecordState(CaptureSnapshot());
        GameObject obj = Spawn(prefab, new Vector3(gridPos.x, gridPos.y, 0), Quaternion.identity, levelContainer);
        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
        RebuildOccupantsCache();
        return obj.GetComponent<T>();
    }

    private void Update()
    {
        UpdatePreviewCursor();
        RunContinuousDeadlockCheck();
        HandleMouseInput();
        UpdateGhostPreviews();
    }

    public void TriggerUndo()
    {
        bool isDrawingOrPlacingDraft = (currentSnakeObj != null && currentDraftNodes.Count > 0)
            || isPlacingPortalExit
            || isPlacingElectricWallEnd;

        if (isDrawingOrPlacingDraft)
        {
            if (currentState != null) currentState.Cancel();
            else UndoLastSegment();
        }
        else if (HistoryService != null && HistoryService.CanUndo(this))
        {
            HistoryService.Undo(this);
        }
        else
        {
            if (currentState != null) currentState.Cancel();
            else UndoLastSegment();
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

        Vector2Int gridPos = GetMouseGridPosition();

        if (Input.GetMouseButtonDown(1))
        {
            previousToolBeforeRMB = currentTool;
            isRmbHoldingErase = true;
            UI_SetTool((int)EditorToolType.Erase);
            currentState?.HandleMouseDown(gridPos);
            return;
        }

        if (Input.GetMouseButton(1))
        {
            if (isRmbHoldingErase)
            {
                currentState?.HandleMouseDown(gridPos);
                return;
            }
        }

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
            currentState?.HandleMouseDown(gridPos);
            return;
        }

        if (Input.GetMouseButton(0))
        {
            currentState?.HandleMouseHold(gridPos);
        }
    }

    public void UI_OpenLevelSelector()
    {
        if (levelSelectorView != null) levelSelectorView.UI_OpenLevelSelector();
    }

    public bool HasUnsavedChanges()
    {
        return BuildEditorStateDigest() != lastSavedDigest;
    }

    public void SelectLevelFromUI(LevelDataV2 selectedLevel)
    {
        if (levelSelectorView != null) levelSelectorView.SelectLevelFromUI(selectedLevel);
    }

    public void UI_SetTool(int toolIndex) 
    { 
        EditorToolType newTool = (EditorToolType)toolIndex;

        if (currentTool == EditorToolType.Draw && newTool != EditorToolType.Draw)
        {
            if (currentSnakeObj != null && currentDraftNodes.Count > 0)
            {
                UI_FinishSnake();
            }
        }

        if (newTool == EditorToolType.Draw && selectedSnakeToModify != null)
        {
            currentSnakeObj = selectedSnakeToModify.gameObject;
            currentSnakeScript = selectedSnakeToModify;
            currentDraftNodes = new List<Vector2Int>(selectedSnakeToModify.LogicNodes);
            currentDir = selectedSnakeToModify.direction;
            currentColor = selectedSnakeToModify.snakeColor;
            currentColor.a = 1.0f;

            RemoveFromFinishedHistory(currentSnakeObj);

            selectedSnakeToModify = null;
            ClearSelectionHighlight();

            UpdateSnakeLinePreview();
            OnColorChanged?.Invoke(currentColor);
        }

        currentState?.OnExit();

        currentTool = newTool; 
        UpdateToolText(); 

        if (stateCache.TryGetValue(currentTool, out EditorStateBase cachedState))
        {
            currentState = cachedState;
        }
        else
        {
            currentState = null;
        }

        currentState?.OnEnter();

        if (currentTool != EditorToolType.Select) ClearSelectionHighlight();
        if (currentTool != EditorToolType.Portal) isPlacingPortalExit = false;
        
        if (currentTool != EditorToolType.ElectricCircuit)
        {
            isPlacingElectricWallEnd = false;
        }
        else
        {
            var electricState = GetCachedState(EditorToolType.ElectricCircuit) as PlaceElectricCircuitState;
            if (electricState == null || electricState.CurrentSubMode != PlaceElectricCircuitState.SubMode.Wall)
            {
                isPlacingElectricWallEnd = false;
            }
        }
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
        if (currentTool == EditorToolType.Draw) return; // Block manual direction changes when Draw tool is active

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
        currentColor.a = 1.0f;
        if (colorPreviewImage != null) colorPreviewImage.color = currentColor;
        if (currentSnakeScript != null) currentSnakeScript.SetColorImmediatePublic(currentColor);
        else if (currentTool == EditorToolType.Select && selectedSnakeToModify != null)
        {
            selectedSnakeToModify.SetColorImmediatePublic(currentColor);
            if (currentSelectionGlowScript != null) currentSelectionGlowScript.SetColorImmediatePublic(currentColor);
        }
        else if (currentTool == EditorToolType.KeycardGate || currentTool == EditorToolType.ElectricCircuit || currentTool == EditorToolType.Portal)
        {
            currentState?.HandleColorSelected(currentColor);
        }
        else
        {
            UI_SetTool((int)EditorToolType.Paint);
        }
        OnManipulationComplete();
        OnColorChanged?.Invoke(currentColor);
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
        Vector2Int hoverGridPos = GetMouseGridPosition();
        
        foreach (Transform child in levelContainer)
        {
            if (child == null) continue;
            
            GridDeflector deflector = child.GetComponentInChildren<GridDeflector>();
            if (deflector != null && Mathf.RoundToInt(child.position.x) == hoverGridPos.x && Mathf.RoundToInt(child.position.y) == hoverGridPos.y)
            {
                HistoryService?.RecordState(CaptureSnapshot());
                ArrowDir nextDir = (ArrowDir)(((int)deflector.direction + 1) % 4);
                deflector.SetDirection(nextDir);
                OnManipulationComplete();
                return;
            }
            
            GridBlackHole blackHole = child.GetComponent<GridBlackHole>();
            if (blackHole != null && Mathf.RoundToInt(child.position.x) == hoverGridPos.x && Mathf.RoundToInt(child.position.y) == hoverGridPos.y)
            {
                HistoryService?.RecordState(CaptureSnapshot());
                ArrowDir nextDir = (ArrowDir)(((int)blackHole.direction + 1) % 4);
                blackHole.SetDirection(nextDir);
                OnManipulationComplete();
                return;
            }
        }
        
        for (int i = 0; i < currentDraftPortals.Count; i++)
        {
            var portal = currentDraftPortals[i];
            if (portal.entrance == hoverGridPos)
            {
                HistoryService?.RecordState(CaptureSnapshot());
                portal.entranceDir = (ArrowDir)(((int)portal.entranceDir + 1) % 4);
                RefreshPortalVisuals();
                OnManipulationComplete();
                return;
            }
            else if (portal.exit == hoverGridPos)
            {
                HistoryService?.RecordState(CaptureSnapshot());
                portal.exitDir = (ArrowDir)(((int)portal.exitDir + 1) % 4);
                RefreshPortalVisuals();
                OnManipulationComplete();
                return;
            }
        }

        if (currentTool == EditorToolType.Draw) return; // Do not rotate active drawing brush

        int nextBrushDir = (int)currentDir + 1;
        UI_SetDirection(nextBrushDir > 3 ? 0 : nextBrushDir);
    }

    internal void UpdateToolText()
    {
        if (textCurrentTool != null)
        {
            string statusText = currentState != null ? currentState.GetToolStatusText() : string.Empty;
            if (!string.IsNullOrEmpty(statusText))
            {
                textCurrentTool.text = $"{currentTool} {statusText} - {currentDir}";
            }
            else
            {
                textCurrentTool.text = $"{currentTool} - {currentDir}";
            }
        }
    }

    public bool HasKeycardWithColor(Color color)
    {
        if (levelContainer == null) return false;
        foreach (Transform child in levelContainer)
        {
            if (child == null) continue;
            GridKeycard keycard = child.GetComponent<GridKeycard>();
            if (keycard != null && LevelEditorRuntimeHelpers.ColorsMatch(keycard.keyColor, color))
            {
                return true;
            }
        }
        return false;
    }

    public Color GetNextUnusedKeycardColor()
    {
        if (editorPalette == null || editorPalette.colors == null || editorPalette.colors.Count == 0)
        {
            return currentColor;
        }

        List<Color> usedColors = new List<Color>();
        if (levelContainer != null)
        {
            foreach (Transform child in levelContainer)
            {
                if (child == null) continue;
                GridKeycard keycard = child.GetComponent<GridKeycard>();
                if (keycard != null)
                {
                    usedColors.Add(keycard.keyColor);
                }
            }
        }

        foreach (Color color in editorPalette.colors)
        {
            bool matched = false;
            foreach (Color used in usedColors)
            {
                if (LevelEditorRuntimeHelpers.ColorsMatch(color, used))
                {
                    matched = true;
                    break;
                }
            }
            if (!matched)
            {
                return color;
            }
        }

        return editorPalette.colors[0];
    }

    public bool HasElectricButtonWithColor(Color color)
    {
        if (levelContainer == null) return false;
        foreach (Transform child in levelContainer)
        {
            if (child == null) continue;
            GridElectricButton button = child.GetComponent<GridElectricButton>();
            if (button != null && LevelEditorRuntimeHelpers.ColorsMatch(button.buttonColor, color))
            {
                return true;
            }
        }
        return false;
    }

    public Color GetNextUnusedElectricButtonColor()
    {
        if (editorPalette == null || editorPalette.colors == null || editorPalette.colors.Count == 0)
        {
            return currentColor;
        }

        List<Color> usedColors = new List<Color>();
        if (levelContainer != null)
        {
            foreach (Transform child in levelContainer)
            {
                if (child == null) continue;
                GridElectricButton button = child.GetComponent<GridElectricButton>();
                if (button != null)
                {
                    usedColors.Add(button.buttonColor);
                }
            }
        }

        foreach (Color color in editorPalette.colors)
        {
            bool matched = false;
            foreach (Color used in usedColors)
            {
                if (LevelEditorRuntimeHelpers.ColorsMatch(color, used))
                {
                    matched = true;
                    break;
                }
            }
            if (!matched)
            {
                return color;
            }
        }

        return editorPalette.colors[0];
    }

    public bool HasPortalWithColor(Color color)
    {
        if (currentDraftPortals == null) return false;
        foreach (PortalData portal in currentDraftPortals)
        {
            if (LevelEditorRuntimeHelpers.ColorsMatch(portal.portalColor, color))
            {
                return true;
            }
        }
        return false;
    }

    public Color GetNextUnusedPortalColor()
    {
        if (editorPalette == null || editorPalette.colors == null || editorPalette.colors.Count == 0)
        {
            return currentColor;
        }

        List<Color> usedColors = new List<Color>();
        if (currentDraftPortals != null)
        {
            foreach (PortalData portal in currentDraftPortals)
            {
                usedColors.Add(portal.portalColor);
            }
        }

        foreach (Color color in editorPalette.colors)
        {
            bool matched = false;
            foreach (Color used in usedColors)
            {
                if (LevelEditorRuntimeHelpers.ColorsMatch(color, used))
                {
                    matched = true;
                    break;
                }
            }
            if (!matched)
            {
                return color;
            }
        }

        return editorPalette.colors[0];
    }

    internal EditorSnakeVisual GetSnakeAtGridPos(Vector2Int pos)
    {
        if (gridOccupantsCache.TryGetValue(pos, out GameObject go) && go != null)
        {
            if (go == currentSnakeObj || go == currentSelectionGlowObj) return null;
            return go.GetComponent<EditorSnakeVisual>();
        }
        return null;
    }

    internal void RebuildOccupantsCache()
    {
        gridOccupantsCache.Clear();
        if (levelContainer == null) return;

        foreach (Transform child in levelContainer)
        {
            if (child == null || child.gameObject == currentSelectionGlowObj) continue;

            var snake = child.GetComponent<EditorSnakeVisual>();
            if (snake != null && snake.LogicNodes != null)
            {
                foreach (var node in snake.LogicNodes)
                {
                    gridOccupantsCache[node] = child.gameObject;
                }
                continue;
            }

            var wall = child.GetComponent<GridElectricWall>();
            if (wall != null)
            {
                Vector2Int start, end;
                if (wall.TryGetEndpoints(out start, out end))
                {
                    int stepX = start.x == end.x ? 0 : (start.x < end.x ? 1 : -1);
                    int stepY = start.y == end.y ? 0 : (start.y < end.y ? 1 : -1);
                    int length = Mathf.Max(Mathf.Abs(end.x - start.x), Mathf.Abs(end.y - start.y));
                    for (int i = 0; i <= length; i++)
                    {
                        Vector2Int cell = new Vector2Int(start.x + stepX * i, start.y + stepY * i);
                        gridOccupantsCache[cell] = child.gameObject;
                    }
                }
                continue;
            }

            Vector2Int pos = new Vector2Int(Mathf.RoundToInt(child.position.x), Mathf.RoundToInt(child.position.y));
            gridOccupantsCache[pos] = child.gameObject;
        }
    }

    internal bool IsPositionOccupied(Vector2Int pos)
    {
        foreach (var node in currentDraftNodes) if (node == pos) return true;
        
        if (gridOccupantsCache.TryGetValue(pos, out GameObject go) && go != null && go != currentSnakeObj) return true;

        for (int i = 0; i < currentDraftElectricWalls.Count; i++)
        {
            if (LevelEditorRuntimeHelpers.IsCellOnElectricWall(pos, currentDraftElectricWalls[i])) return true;
        }

        return false;
    }



    internal List<Vector2Int> GetInterpolatedPath(Vector2Int start, Vector2Int end)
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

            if (IsPositionOccupied(nextStep))
            {
                break;
            }

            path.Add(nextStep);
            current = nextStep;
        }
        return path;
    }

    internal void UpdateAutoDirection()
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

    internal bool ShouldUseRedTurnState()
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

    internal void RefreshPortalVisuals()
    {
        foreach(var obj in spawnedPortalVisuals) Recycle(obj);
        spawnedPortalVisuals.Clear();
        if (portalPrefab == null) return;
        for (int i = 0; i < currentDraftPortals.Count; i++)
        {
            var p = currentDraftPortals[i];
            GameObject inObj = Spawn(portalPrefab, new Vector3(p.entrance.x, p.entrance.y, 0), GetRotationForDir(p.entranceDir), levelContainer);
            GameObject outObj = Spawn(portalPrefab, new Vector3(p.exit.x, p.exit.y, 0), GetRotationForDir(p.exitDir), levelContainer);
            SpriteRenderer inSr = inObj.GetComponent<SpriteRenderer>();
            if(inSr) inSr.color = p.portalColor;
            SpriteRenderer outSr = outObj.GetComponent<SpriteRenderer>();
            if(outSr) outSr.color = p.portalColor;
            spawnedPortalVisuals.Add(inObj);
            spawnedPortalVisuals.Add(outObj);
        }
    }

    internal void RefreshElectricWallVisuals()
    {
        foreach (var obj in spawnedElectricWallVisuals) Recycle(obj);
        spawnedElectricWallVisuals.Clear();
        if (electricWallPrefab == null) return;

        for (int i = 0; i < currentDraftElectricWalls.Count; i++)
        {
            ElectricWallSaveData w = currentDraftElectricWalls[i];
            GameObject obj = Spawn(electricWallPrefab, Vector3.zero, Quaternion.identity, levelContainer);
            GridElectricWall wall = obj.GetComponent<GridElectricWall>();
            if (wall != null) wall.Initialize(w.start, w.end, w.color, false);
            spawnedElectricWallVisuals.Add(obj);
        }
    }

    internal bool TryRemoveElectricWallAtPos(Vector2Int gridPos)
    {
        for (int i = currentDraftElectricWalls.Count - 1; i >= 0; i--)
        {
            if (LevelEditorRuntimeHelpers.IsCellOnElectricWall(gridPos, currentDraftElectricWalls[i]))
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

    internal bool IsElectricWallPathClear(Vector2Int start, Vector2Int end)
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

    internal void UpdateSelectionHighlight(EditorSnakeVisual target)
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

    internal void ClearSelectionHighlight()
    {
        if (currentSelectionGlowObj != null)
        {
            currentSelectionGlowObj.transform.SetParent(null, true);
            currentSelectionGlowObj.SetActive(false);
            Destroy(currentSelectionGlowObj);
            currentSelectionGlowObj = null;
            currentSelectionGlowScript = null;
        }
    }

    internal void CreateHead(Vector2Int pos)
    {
        currentSnakeObj = Spawn(snakePrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, levelContainer);
        currentSnakeScript = currentSnakeObj.GetComponent<EditorSnakeVisual>();
        currentDraftNodes.Clear(); currentDraftNodes.Add(pos);
        currentSnakeScript.direction = currentDir;
        currentSnakeScript.SetColorImmediatePublic(currentColor);
        currentSnakeScript.UpdateVisualRotation(); 
        UpdateSnakeLinePreview();
    }

    internal void CreateBodySegment(Vector2Int pos) { currentDraftNodes.Add(pos); UpdateSnakeLinePreview(); }

    internal void CreateHeadSegment(Vector2Int pos)
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

    internal void RetractHeadSegment()
    {
        if (currentDraftNodes == null || currentDraftNodes.Count == 0) return;

        currentDraftNodes.RemoveAt(0);
        if (currentDraftNodes.Count == 0)
        {
            if (currentSnakeObj != null) Recycle(currentSnakeObj);
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

    internal void RetractTailSegment()
    {
        if (currentDraftNodes == null || currentDraftNodes.Count == 0) return;

        currentDraftNodes.RemoveAt(currentDraftNodes.Count - 1);
        if (currentDraftNodes.Count == 0)
        {
            if (currentSnakeObj != null) Recycle(currentSnakeObj);
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

        if (currentTool == EditorToolType.ElectricCircuit && isPlacingElectricWallEnd)
        {
            isPlacingElectricWallEnd = false;
            lastCalculatedGridPos = new Vector2Int(-9999, -9999);
            return;
        }

        if (currentSnakeObj != null && currentDraftNodes.Count > 0)
        {
            currentDraftNodes.RemoveAt(currentDraftNodes.Count - 1);
            if (currentDraftNodes.Count == 0) { Recycle(currentSnakeObj); currentSnakeObj = null; }
            else UpdateSnakeLinePreview(); 
        }
        else if (finishedSnakesHistory.Count > 0) Recycle(finishedSnakesHistory.Pop());
        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    public void CancelActivePlacement()
    {
        bool changed = false;

        if (currentTool == EditorToolType.Portal && isPlacingPortalExit)
        {
            isPlacingPortalExit = false;
            lastCalculatedGridPos = new Vector2Int(-9999, -9999);
            changed = true;
        }

        if (currentTool == EditorToolType.ElectricCircuit && isPlacingElectricWallEnd)
        {
            isPlacingElectricWallEnd = false;
            lastCalculatedGridPos = new Vector2Int(-9999, -9999);
            changed = true;
        }

        if (currentSnakeObj != null)
        {
            Recycle(currentSnakeObj);
            currentSnakeObj = null;
            currentSnakeScript = null;
            currentDraftNodes.Clear();
            lastCalculatedGridPos = new Vector2Int(-9999, -9999);
            changed = true;
        }

        if (changed)
        {
            UpdateGhostPreviews();
            OnManipulationComplete();
        }
    }

    internal void UpdateSnakeLinePreview()
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

    private void CreateActivePreviewObject()
    {
        if (activePreviewObject != null)
        {
            Destroy(activePreviewObject);
            activePreviewObject = null;
        }

        GameObject prefab = GetPrefabForTool(currentTool);
        if (prefab == null)
        {
            if (previewCursor != null) previewCursor.gameObject.SetActive(true);
            return;
        }

        if (previewCursor != null) previewCursor.gameObject.SetActive(false);

        EnsureSelectionOverlayContainer();
        activePreviewObject = Instantiate(prefab, selectionOverlayContainer);
        
        MonoBehaviour[] scripts = activePreviewObject.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (MonoBehaviour script in scripts)
        {
            if (script == null) continue;
            
            if (script is IGridOccupant || script is IPreviewDisableable)
            {
                script.enabled = false;
            }
        }
    }

    private GameObject GetPrefabForTool(EditorToolType tool)
    {
        switch (tool)
        {
            case EditorToolType.KeycardGate: 
                {
                    var state = GetCachedState(EditorToolType.KeycardGate) as PlaceKeycardGateState;
                    bool isKeycard = state == null || state.CurrentSubMode == PlaceKeycardGateState.SubMode.Keycard;
                    return isKeycard ? keycardPrefab : gatePrefab;
                }
            case EditorToolType.Deflector: return deflectorPrefab;
            case EditorToolType.CountdownBlock: return countdownBlockPrefab;
            case EditorToolType.ElectricCircuit: 
                {
                    var state = GetCachedState(EditorToolType.ElectricCircuit) as PlaceElectricCircuitState;
                    bool isButton = state == null || state.CurrentSubMode == PlaceElectricCircuitState.SubMode.Button;
                    return isButton ? electricButtonPrefab : electricWallPrefab;
                }
            case EditorToolType.StopBlock: return stopBlockPrefab;
            case EditorToolType.TurnStateBlock: return turnStateBlockPrefab;
            case EditorToolType.BlackHole: return blackHolePrefab;
            case EditorToolType.RevealWaveButton: return revealWaveButtonPrefab;
            case EditorToolType.Portal: return portalPrefab;
            default: return null;
        }
    }

    private void UpdateActivePreviewVisuals()
    {
        if (activePreviewObject == null) return;

        if (currentTool == EditorToolType.Portal && isPlacingPortalExit)
        {
            activePreviewObject.SetActive(false);
            return;
        }
        else
        {
            activePreviewObject.SetActive(true);
        }

        Vector2Int gridPos = GetMouseGridPosition();
        activePreviewObject.transform.position = new Vector3(gridPos.x, gridPos.y, -0.5f);

        if (currentTool == EditorToolType.Deflector || currentTool == EditorToolType.BlackHole || currentTool == EditorToolType.Portal)
        {
            activePreviewObject.transform.rotation = GetRotationForDir(currentDir);
        }
        else
        {
            activePreviewObject.transform.rotation = Quaternion.identity;
        }

        Color baseColor = currentColor;
        baseColor.a = 0.6f;

        if (currentTool == EditorToolType.CountdownBlock || currentTool == EditorToolType.StopBlock)
        {
            SetObjectSpritesColor(activePreviewObject, new Color(1f, 1f, 1f, 0.6f));
            
            int countVal = editorCountdownValue;
            if (inputCountdownValue != null) int.TryParse(inputCountdownValue.text, out countVal);
            if (countVal < 1) countVal = 1;
            
            var tmps = activePreviewObject.GetComponentsInChildren<TMPro.TMP_Text>(true);
            foreach (var tmp in tmps)
            {
                tmp.text = countVal.ToString();
                tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, 0.6f);
            }
        }
        else if (currentTool == EditorToolType.TurnStateBlock)
        {
            Color turnColor = ShouldUseRedTurnState() ? Color.red : new Color(0.1f, 1f, 0.35f, 1f);
            turnColor.a = 0.6f;
            SetObjectSpritesColor(activePreviewObject, turnColor);
        }
        else if (currentTool == EditorToolType.Deflector || currentTool == EditorToolType.BlackHole)
        {
            var srs = activePreviewObject.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in srs)
            {
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.6f);
            }
        }
        else
        {
            SetObjectSpritesColor(activePreviewObject, baseColor);
        }
    }

    private void SetObjectSpritesColor(GameObject obj, Color color)
    {
        var srs = obj.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in srs)
        {
            sr.color = color;
        }
    }

    private int lastObservedKeycardGateSubMode = -1;
    private int lastObservedElectricCircuitSubMode = -1;

    private void UpdatePreviewCursor()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        if (previewCursor != null)
        {
            previewCursor.transform.position = new Vector3(gridPos.x, gridPos.y, -1f); 
        }
        
        bool submodeChanged = false;
        if (currentTool == EditorToolType.KeycardGate)
        {
            var state = GetCachedState(EditorToolType.KeycardGate) as PlaceKeycardGateState;
            int currentSub = state != null ? (int)state.CurrentSubMode : 0;
            if (currentSub != lastObservedKeycardGateSubMode)
            {
                lastObservedKeycardGateSubMode = currentSub;
                submodeChanged = true;
            }
        }
        if (currentTool == EditorToolType.ElectricCircuit)
        {
            var state = GetCachedState(EditorToolType.ElectricCircuit) as PlaceElectricCircuitState;
            int currentSub = state != null ? (int)state.CurrentSubMode : 0;
            if (currentSub != lastObservedElectricCircuitSubMode)
            {
                lastObservedElectricCircuitSubMode = currentSub;
                submodeChanged = true;
            }
        }

        if (lastObservedPreviewTool != currentTool || submodeChanged)
        {
            lastObservedPreviewTool = currentTool;
            CreateActivePreviewObject();
        }

        UpdateActivePreviewVisuals();

        if (gridPos == lastCalculatedGridPos) return;
        lastCalculatedGridPos = gridPos; 

        if (previewCursor != null)
        {
            if (currentTool == EditorToolType.Draw)
            {
                bool invalid = IsPositionOccupied(gridPos);
                previewCursor.color = invalid ? new Color(1, 0, 0, 0.5f) : new Color(currentColor.r, currentColor.g, currentColor.b, 0.5f);
            }
            else if (currentTool == EditorToolType.Erase) previewCursor.color = new Color(1, 0, 0, 0.5f);
            else if (currentTool == EditorToolType.Select) previewCursor.color = new Color(1, 1, 0, 0.3f);
            else if (currentTool == EditorToolType.ArrowShadow) previewCursor.color = new Color(0.15f, 0.75f, 1f, 0.45f);
            else if (currentTool == EditorToolType.TurnStateBlock) previewCursor.color = ShouldUseRedTurnState() ? new Color(1f, 0.1f, 0.1f, 0.8f) : new Color(0.1f, 1f, 0.35f, 0.8f);
            else if (currentTool == EditorToolType.BlackHole) previewCursor.color = new Color(0.05f, 0.05f, 0.08f, 0.75f);
            else if (currentTool == EditorToolType.RevealWaveButton) previewCursor.color = new Color(0.35f, 0.9f, 1f, 0.8f);
            else if (currentTool == EditorToolType.Portal || currentTool == EditorToolType.KeycardGate || currentTool == EditorToolType.Deflector || currentTool == EditorToolType.CountdownBlock || currentTool == EditorToolType.ElectricCircuit || currentTool == EditorToolType.StopBlock) 
                previewCursor.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.8f);
        }
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
            SyncLevelDataToTemp);
    }

    private void SyncLevelDataToTemp()
    {
        ClearSelectionHighlight();
        if (editingData != null)
        {
            serializer.SyncToData(CreateContext());
            tempEditingData = LevelDataV2Cloner.Clone(editingData);
        }
    }

    public void UI_Playtest()
    {
        LevelDataV2 levelData = PrepareCurrentLevelForPlaytest();
        if (levelData == null)
        {
            Debug.LogWarning("[LevelEditorWorkspace] UI_Playtest aborted: currentData is null.");
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
            Debug.LogWarning($"[LevelEditorWorkspace] {message}");
        }
        else if (!hasDeadlock && _hasContinuousDeadlockState && _lastContinuousDeadlockState)
        {
            Debug.Log($"[LevelEditorWorkspace] Deadlock cleared. {message}");
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
                Debug.LogWarning($"[LevelEditorWorkspace] {resultMessage}");
            }
            else
            {
                Debug.Log($"[LevelEditorWorkspace] {resultMessage}");
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
        
        lastSavedDigest = BuildEditorStateDigest();
    }

    private void LoadLevelToEdit()
    {
        ClearSelectionHighlight();
        bool restored = false;
        if (currentData != null)
        {
            if (tempEditingData != null && tempEditingData.name == currentData.name)
            {
                editingData = tempEditingData;
                tempEditingData = null;
                restored = true;
            }
            else
            {
                editingData = LevelDataV2Cloner.Clone(currentData);
            }
        }
        else
        {
            editingData = null;
            tempEditingData = null;
        }
        serializer.Load(CreateContext());
        RebuildOccupantsCache();
        FrameLevelInEditorCamera();

        if (!restored)
        {
            lastSavedDigest = BuildEditorStateDigest();
        }
    }

    private void FrameLevelInEditorCamera()
    {
        if (cameraController != null)
        {
            cameraController.FrameLevel(currentData);
        }
        lastCalculatedGridPos = new Vector2Int(-9999, -9999);
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
            int childCount = levelContainer.childCount;
            if (childCount > 0)
            {
                Transform[] children = new Transform[childCount];
                for (int i = 0; i < childCount; i++)
                {
                    children[i] = levelContainer.GetChild(i);
                }
                foreach (Transform child in children)
                {
                    Recycle(child.gameObject);
                }
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
        
        gridOccupantsCache.Clear();

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
        currentColor.a = 1.0f;

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
        RebuildOccupantsCache();
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
        OnColorChanged?.Invoke(currentColor);
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
        RebuildOccupantsCache();
    }

    public void SaveLevelPublic() => SaveLevel();
    public void LoadLevelToEditPublic() => LoadLevelToEdit();
    public void SetCurrentData(LevelDataV2 data) => currentData = data;

    public GameObject GetPrefabByCellTypeId(string cellTypeId)
    {
        switch (cellTypeId)
        {
            case CellTypeIds.Portal: return portalPrefab;
            case CellTypeIds.Deflector: return deflectorPrefab;
            case CellTypeIds.BlackHole: return blackHolePrefab;
            case CellTypeIds.CountdownBlock: return countdownBlockPrefab;
            case CellTypeIds.StopBlock: return stopBlockPrefab;
            case CellTypeIds.ElectricWall: return electricWallPrefab;
            case CellTypeIds.ElectricButton: return electricButtonPrefab;
            case CellTypeIds.RevealWaveButton: return revealWaveButtonPrefab;
            case CellTypeIds.Keycard: return keycardPrefab;
            case CellTypeIds.Gate: return gatePrefab;
            default: return null;
        }
    }

    public Sprite GetSpriteByCellTypeId(string cellTypeId)
    {
        GameObject prefab = GetPrefabByCellTypeId(cellTypeId);
        if (prefab == null) return null;
        SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>();
        return sr != null ? sr.sprite : null;
    }
}
