using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public enum EditorToolType { Draw, Erase, Paint, Select, Portal, Keycard, Gate, Deflector, CountdownBlock, ElectricButton, ElectricWall }

public class LevelEditor : MonoBehaviour
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
    public GameObject electricWallPrefab;
    public GameObject deflectorPrefab;
    public GameObject countdownBlockPrefab;
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

    private class DeadlockSnake
    {
        public int index;
        public string label;
        public ArrowDir direction;
        public Color color;
        public List<Vector2Int> cells = new List<Vector2Int>();
        public bool released;
        public string lastBlockedReason;
    }

    private class DeadlockElectricWall
    {
        public int index;
        public ElectricWallSaveData data;
        public Color color;
        public bool active = true;
        public readonly List<Vector2Int> cells = new List<Vector2Int>();
    }

    private class DeadlockPortalLink
    {
        public Vector2Int exit;
        public ArrowDir exitDir;
    }

    private class DeadlockCheckState
    {
        public readonly List<DeadlockSnake> snakes = new List<DeadlockSnake>();
        public readonly Dictionary<Vector2Int, int> snakeByCell = new Dictionary<Vector2Int, int>();
        public readonly Dictionary<Vector2Int, Color> keycards = new Dictionary<Vector2Int, Color>();
        public readonly Dictionary<Vector2Int, Color> gates = new Dictionary<Vector2Int, Color>();
        public readonly Dictionary<Vector2Int, Color> electricButtons = new Dictionary<Vector2Int, Color>();
        public readonly List<DeadlockElectricWall> electricWalls = new List<DeadlockElectricWall>();
        public readonly Dictionary<Vector2Int, List<int>> electricWallIdsByCell = new Dictionary<Vector2Int, List<int>>();
        public readonly Dictionary<Vector2Int, int> countdownBlocks = new Dictionary<Vector2Int, int>();
        public readonly Dictionary<Vector2Int, DeadlockPortalLink> portals = new Dictionary<Vector2Int, DeadlockPortalLink>();
        public readonly Dictionary<Vector2Int, ArrowDir> deflectors = new Dictionary<Vector2Int, ArrowDir>();
        public int releasedCount;
    }

    private class DeadlockPathResult
    {
        public bool canExit;
        public string blockedReason;
        public readonly List<Color> collectedKeyColors = new List<Color>();
        public readonly List<Color> pressedButtonColors = new List<Color>();
    }

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

    private void Update()
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

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) UI_SetDirection(0);
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) UI_SetDirection(1);
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) UI_SetDirection(2);
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) UI_SetDirection(3);

        if (Input.GetKeyDown(KeyCode.Space)) UI_FinishSnake();
        if (Input.GetKeyDown(KeyCode.R)) RotateDirection();
        if (Input.GetKeyDown(KeyCode.Z)) UndoLastSegment();

        UpdatePreviewCursor();
        RunContinuousDeadlockCheck();

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
            else if (currentTool == EditorToolType.Deflector) HandleDeflectorPlacement();
            else if (currentTool == EditorToolType.CountdownBlock) HandleCountdownBlockPlacement();
            else if (currentTool == EditorToolType.ElectricButton) HandleObjectPlacement<GridElectricButton>(electricButtonPrefab);
            else if (currentTool == EditorToolType.ElectricWall) HandleElectricWallClick();
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
            GridElectricWall ew = child.GetComponent<GridElectricWall>();
            if (ew != null && ew.ContainsCell(pos)) return true;
            GridDeflector d = child.GetComponentInChildren<GridDeflector>();
            if (d != null && Mathf.RoundToInt(d.transform.position.x) == pos.x && Mathf.RoundToInt(d.transform.position.y) == pos.y) return true;
            if (child.TryGetComponent(out GridCountdownBlock cb) && Mathf.RoundToInt(child.position.x) == pos.x && Mathf.RoundToInt(child.position.y) == pos.y) return true;
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
                        sb.Initialize(sb.direction, new List<Vector2Int>(sb.LogicNodes), sb.snakeColor);
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
            if ((child.GetComponent<GridKeycard>() != null || child.GetComponent<GridLaserGate>() != null || child.GetComponent<GridElectricButton>() != null || deflector != null || child.GetComponent<GridCountdownBlock>() != null)
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
        return start.x == end.x || start.y == end.y;
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
        if (!IsElectricWallAligned(wall.start, wall.end)) return false;

        if (wall.start.x == wall.end.x)
        {
            if (cell.x != wall.start.x) return false;
            int minY = Mathf.Min(wall.start.y, wall.end.y);
            int maxY = Mathf.Max(wall.start.y, wall.end.y);
            return cell.y >= minY && cell.y <= maxY;
        }

        if (cell.y != wall.start.y) return false;
        int minX = Mathf.Min(wall.start.x, wall.end.x);
        int maxX = Mathf.Max(wall.start.x, wall.end.x);
        return cell.x >= minX && cell.x <= maxX;
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

    private static Vector2Int GetDirStep(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: return new Vector2Int(0, 1);
            case ArrowDir.Down: return new Vector2Int(0, -1);
            case ArrowDir.Left: return new Vector2Int(-1, 0);
            case ArrowDir.Right: return new Vector2Int(1, 0);
            default: return Vector2Int.zero;
        }
    }

    private static int GetStepKey(Vector2Int step)
    {
        if (step.y > 0) return 0;
        if (step.y < 0) return 1;
        if (step.x < 0) return 2;
        return 3;
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
        else if (currentTool == EditorToolType.Portal || currentTool == EditorToolType.Keycard || currentTool == EditorToolType.Gate || currentTool == EditorToolType.Deflector || currentTool == EditorToolType.CountdownBlock || currentTool == EditorToolType.ElectricButton || currentTool == EditorToolType.ElectricWall) 
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

    public LevelDataSO PrepareCurrentLevelForPlaytest()
    {
        if (currentData == null)
        {
            Debug.LogWarning("[LevelEditor] PrepareCurrentLevelForPlaytest aborted: currentData is null.");
            return null;
        }

        // Commit any in-progress edit state so the saved asset matches what you see.
        if (currentSnakeObj != null && currentDraftNodes != null && currentDraftNodes.Count > 0)
        {
            UI_FinishSnake();
        }

        // Cancel unfinished portal/electric-wall placement to avoid partial data.
        if (currentTool == EditorToolType.Portal && isPlacingPortalExit)
        {
            isPlacingPortalExit = false;
        }

        if (currentTool == EditorToolType.ElectricWall && isPlacingElectricWallEnd)
        {
            isPlacingElectricWallEnd = false;
        }

        SaveLevel();
        return currentData;
    }

    public void UI_Playtest()
    {
        LevelDataSO levelData = PrepareCurrentLevelForPlaytest();
        if (levelData == null)
        {
            Debug.LogWarning("[LevelEditor] UI_Playtest aborted: currentData is null.");
            return;
        }

        PlaytestSession.StartPlaytest(
            levelData,
            SceneManager.GetActiveScene().name,
            SceneManager.GetActiveScene().path,
            "GameScene");

#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            EditorSceneManager.LoadSceneInPlayMode(GameScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            return;
        }
#endif

        TransitionManager transition = FindObjectOfType<TransitionManager>();
        if (transition != null)
        {
            transition.TransitionToScreen(ScreenType.Gameplay);
            return;
        }

        ScreenManager screenManager = FindObjectOfType<ScreenManager>();
        if (screenManager != null)
        {
            screenManager.ShowScreen(ScreenType.Gameplay);
            return;
        }

        SceneManager.LoadScene("GameScene");
        return;
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
        DeadlockCheckState state = BuildDeadlockCheckState();
        if (state.snakes.Count == 0)
        {
            resultMessage = "Deadlock check skipped: no snakes in current editor level.";
            if (logResult) Debug.LogWarning($"[LevelEditor] {resultMessage}");
            return false;
        }

        List<int> releaseOrder = new List<int>(state.snakes.Count);
        bool madeProgress = true;

        while (state.releasedCount < state.snakes.Count && madeProgress)
        {
            madeProgress = false;

            for (int i = 0; i < state.snakes.Count; i++)
            {
                DeadlockSnake snake = state.snakes[i];
                if (snake.released) continue;

                DeadlockPathResult pathResult = CheckSnakeExitPath(snake, state);
                snake.lastBlockedReason = pathResult.blockedReason;
                if (!pathResult.canExit) continue;

                ApplyDeadlockRelease(snake, pathResult, state);
                releaseOrder.Add(snake.index);
                madeProgress = true;
            }
        }

        bool solved = state.releasedCount == state.snakes.Count;
        if (solved)
        {
            string orderText = BuildReleaseOrderText(releaseOrder);
            resultMessage = $"No deadlock. Release order: {orderText}";
            if (logResult) Debug.Log($"[LevelEditor] Deadlock check OK. {resultMessage}");
            return false;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Deadlock detected. Stuck snakes:");
        for (int i = 0; i < state.snakes.Count; i++)
        {
            DeadlockSnake snake = state.snakes[i];
            if (snake.released) continue;

            builder.Append("- ");
            builder.Append(snake.label);
            builder.Append(": ");
            builder.AppendLine(string.IsNullOrEmpty(snake.lastBlockedReason) ? "no exit path" : snake.lastBlockedReason);
        }

        resultMessage = builder.ToString().TrimEnd();
        if (logResult) Debug.LogWarning($"[LevelEditor] {resultMessage}");
        return true;
    }

    private DeadlockCheckState BuildDeadlockCheckState()
    {
        DeadlockCheckState state = new DeadlockCheckState();

        foreach (Transform child in levelContainer)
        {
            if (child == null) continue;
            if (currentSnakeObj != null && child.gameObject == currentSnakeObj) continue;
            if (currentSelectionGlowObj != null && child.gameObject == currentSelectionGlowObj) continue;

            EditorSnakeVisual snakeVisual = child.GetComponent<EditorSnakeVisual>();
            if (snakeVisual == null || snakeVisual.LogicNodes == null || snakeVisual.LogicNodes.Count == 0) continue;

            AddDeadlockSnake(state, snakeVisual.direction, snakeVisual.snakeColor, snakeVisual.LogicNodes, $"Snake #{state.snakes.Count + 1}");
        }

        if (currentDraftNodes != null && currentDraftNodes.Count > 0)
        {
            AddDeadlockSnake(state, currentDir, currentColor, currentDraftNodes, $"Draft Snake #{state.snakes.Count + 1}");
        }

        foreach (Transform child in levelContainer)
        {
            if (child == null) continue;
            if (currentSelectionGlowObj != null && child.gameObject == currentSelectionGlowObj) continue;

            Vector2Int childCell = new Vector2Int(Mathf.RoundToInt(child.position.x), Mathf.RoundToInt(child.position.y));

            if (child.TryGetComponent(out GridKeycard keycard))
            {
                state.keycards[childCell] = keycard.keyColor;
            }

            if (child.TryGetComponent(out GridLaserGate gate))
            {
                state.gates[childCell] = gate.gateColor;
            }

            if (child.TryGetComponent(out GridElectricButton button))
            {
                state.electricButtons[childCell] = button.buttonColor;
            }

            GridDeflector deflector = child.GetComponentInChildren<GridDeflector>();
            if (deflector != null)
            {
                Vector2Int deflectorCell = new Vector2Int(Mathf.RoundToInt(deflector.transform.position.x), Mathf.RoundToInt(deflector.transform.position.y));
                state.deflectors[deflectorCell] = deflector.direction;
            }

            if (child.TryGetComponent(out GridCountdownBlock countdownBlock))
            {
                state.countdownBlocks[childCell] = Mathf.Max(1, countdownBlock.count);
            }
        }

        for (int i = 0; i < currentDraftPortals.Count; i++)
        {
            PortalData portal = currentDraftPortals[i];
            state.portals[portal.entrance] = new DeadlockPortalLink { exit = portal.exit, exitDir = portal.exitDir };
            state.portals[portal.exit] = new DeadlockPortalLink { exit = portal.entrance, exitDir = portal.entranceDir };
        }

        for (int i = 0; i < currentDraftElectricWalls.Count; i++)
        {
            AddDeadlockElectricWall(state, currentDraftElectricWalls[i]);
        }

        return state;
    }

    private void AddDeadlockSnake(DeadlockCheckState state, ArrowDir direction, Color color, List<Vector2Int> cells, string label)
    {
        DeadlockSnake snake = new DeadlockSnake
        {
            index = state.snakes.Count,
            label = label,
            direction = direction,
            color = color
        };

        for (int i = 0; i < cells.Count; i++)
        {
            Vector2Int cell = cells[i];
            snake.cells.Add(cell);
            state.snakeByCell[cell] = snake.index;
        }

        state.snakes.Add(snake);
    }

    private void AddDeadlockElectricWall(DeadlockCheckState state, ElectricWallSaveData wallData)
    {
        if (!IsElectricWallAligned(wallData.start, wallData.end)) return;

        DeadlockElectricWall wall = new DeadlockElectricWall
        {
            index = state.electricWalls.Count,
            data = wallData,
            color = wallData.color
        };

        int stepX = wallData.start.x == wallData.end.x ? 0 : (wallData.start.x < wallData.end.x ? 1 : -1);
        int stepY = wallData.start.y == wallData.end.y ? 0 : (wallData.start.y < wallData.end.y ? 1 : -1);
        int length = Mathf.Max(Mathf.Abs(wallData.end.x - wallData.start.x), Mathf.Abs(wallData.end.y - wallData.start.y));

        for (int i = 0; i <= length; i++)
        {
            Vector2Int cell = new Vector2Int(wallData.start.x + stepX * i, wallData.start.y + stepY * i);
            wall.cells.Add(cell);

            if (!state.electricWallIdsByCell.TryGetValue(cell, out List<int> wallIds))
            {
                wallIds = new List<int>(1);
                state.electricWallIdsByCell[cell] = wallIds;
            }

            wallIds.Add(wall.index);
        }

        state.electricWalls.Add(wall);
    }

    private DeadlockPathResult CheckSnakeExitPath(DeadlockSnake snake, DeadlockCheckState state)
    {
        DeadlockPathResult result = new DeadlockPathResult();
        if (snake.cells == null || snake.cells.Count == 0)
        {
            result.blockedReason = "empty snake";
            return result;
        }

        Vector2Int currentPos = snake.cells[0];
        ArrowDir currentDir = snake.direction;
        Vector2Int step = GetDirStep(currentDir);
        if (step == Vector2Int.zero)
        {
            result.blockedReason = "invalid direction";
            return result;
        }

        HashSet<Vector3Int> visitedStates = new HashSet<Vector3Int>();
        HashSet<Vector2Int> locallyOpenedGateCells = new HashSet<Vector2Int>();
        HashSet<int> locallyDisabledWallIds = new HashSet<int>();

        int scanLimit = Mathf.Max(16, deadlockScanLimit);
        for (int scan = 1; scan <= scanLimit; scan++)
        {
            Vector3Int stateKey = new Vector3Int(currentPos.x, currentPos.y, GetStepKey(step));
            if (!visitedStates.Add(stateKey))
            {
                result.canExit = true;
                return result;
            }

            Vector2Int checkPos = currentPos + step;

            if (state.snakeByCell.TryGetValue(checkPos, out int blockerSnakeIndex)
                && blockerSnakeIndex != snake.index
                && blockerSnakeIndex >= 0
                && blockerSnakeIndex < state.snakes.Count
                && !state.snakes[blockerSnakeIndex].released)
            {
                result.blockedReason = $"blocked by {state.snakes[blockerSnakeIndex].label} at {FormatCell(checkPos)}";
                return result;
            }

            if (state.countdownBlocks.TryGetValue(checkPos, out int countdown) && countdown > 0)
            {
                result.blockedReason = $"blocked by countdown block ({countdown}) at {FormatCell(checkPos)}";
                return result;
            }

            if (state.gates.TryGetValue(checkPos, out Color gateColor) && !locallyOpenedGateCells.Contains(checkPos))
            {
                result.blockedReason = $"blocked by gate {FormatColor(gateColor)} at {FormatCell(checkPos)}";
                return result;
            }

            if (TryGetActiveElectricWallAt(checkPos, state, locallyDisabledWallIds, out DeadlockElectricWall wall))
            {
                result.blockedReason = $"blocked by electric wall {FormatColor(wall.color)} at {FormatCell(checkPos)}";
                return result;
            }

            if (state.keycards.TryGetValue(checkPos, out Color keyColor))
            {
                result.collectedKeyColors.Add(keyColor);
                MarkMatchingGatesOpened(state, keyColor, locallyOpenedGateCells);
            }

            if (state.electricButtons.TryGetValue(checkPos, out Color buttonColor))
            {
                result.pressedButtonColors.Add(buttonColor);
                MarkMatchingElectricWallsDisabled(state, buttonColor, locallyDisabledWallIds);
            }

            if (state.portals.TryGetValue(checkPos, out DeadlockPortalLink portalLink))
            {
                currentPos = portalLink.exit;
                currentDir = portalLink.exitDir;
                step = GetDirStep(currentDir);
                if (step == Vector2Int.zero)
                {
                    result.blockedReason = $"portal exits with invalid direction at {FormatCell(checkPos)}";
                    return result;
                }
                continue;
            }

            if (state.deflectors.TryGetValue(checkPos, out ArrowDir deflectedDir))
            {
                currentPos = checkPos;
                currentDir = deflectedDir;
                step = GetDirStep(currentDir);
                if (step == Vector2Int.zero)
                {
                    result.blockedReason = $"deflector has invalid direction at {FormatCell(checkPos)}";
                    return result;
                }
                continue;
            }

            currentPos = checkPos;
        }

        result.canExit = true;
        return result;
    }

    private void ApplyDeadlockRelease(DeadlockSnake snake, DeadlockPathResult pathResult, DeadlockCheckState state)
    {
        snake.released = true;
        state.releasedCount++;

        for (int i = 0; i < snake.cells.Count; i++)
        {
            Vector2Int cell = snake.cells[i];
            if (state.snakeByCell.TryGetValue(cell, out int snakeIndex) && snakeIndex == snake.index)
            {
                state.snakeByCell.Remove(cell);
            }
        }

        for (int i = 0; i < pathResult.collectedKeyColors.Count; i++)
        {
            RemoveMatchingGates(state, pathResult.collectedKeyColors[i]);
        }

        for (int i = 0; i < pathResult.pressedButtonColors.Count; i++)
        {
            DisableMatchingElectricWalls(state, pathResult.pressedButtonColors[i]);
        }

        DecrementCountdownBlocks(state);
    }

    private void MarkMatchingGatesOpened(DeadlockCheckState state, Color keyColor, HashSet<Vector2Int> locallyOpenedGateCells)
    {
        foreach (KeyValuePair<Vector2Int, Color> gate in state.gates)
        {
            if (ColorsMatch(keyColor, gate.Value))
            {
                locallyOpenedGateCells.Add(gate.Key);
            }
        }
    }

    private void MarkMatchingElectricWallsDisabled(DeadlockCheckState state, Color buttonColor, HashSet<int> locallyDisabledWallIds)
    {
        for (int i = 0; i < state.electricWalls.Count; i++)
        {
            DeadlockElectricWall wall = state.electricWalls[i];
            if (wall.active && ColorsMatch(buttonColor, wall.color))
            {
                locallyDisabledWallIds.Add(wall.index);
            }
        }
    }

    private void RemoveMatchingGates(DeadlockCheckState state, Color keyColor)
    {
        List<Vector2Int> gatesToRemove = new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, Color> gate in state.gates)
        {
            if (ColorsMatch(keyColor, gate.Value)) gatesToRemove.Add(gate.Key);
        }

        for (int i = 0; i < gatesToRemove.Count; i++)
        {
            state.gates.Remove(gatesToRemove[i]);
        }
    }

    private void DisableMatchingElectricWalls(DeadlockCheckState state, Color buttonColor)
    {
        for (int i = 0; i < state.electricWalls.Count; i++)
        {
            DeadlockElectricWall wall = state.electricWalls[i];
            if (wall.active && ColorsMatch(buttonColor, wall.color))
            {
                wall.active = false;
            }
        }
    }

    private void DecrementCountdownBlocks(DeadlockCheckState state)
    {
        if (state.countdownBlocks.Count == 0) return;

        List<Vector2Int> cells = new List<Vector2Int>(state.countdownBlocks.Keys);
        for (int i = 0; i < cells.Count; i++)
        {
            Vector2Int cell = cells[i];
            int nextCount = state.countdownBlocks[cell] - 1;
            if (nextCount <= 0) state.countdownBlocks.Remove(cell);
            else state.countdownBlocks[cell] = nextCount;
        }
    }

    private bool TryGetActiveElectricWallAt(Vector2Int cell, DeadlockCheckState state, HashSet<int> locallyDisabledWallIds, out DeadlockElectricWall wall)
    {
        wall = null;
        if (!state.electricWallIdsByCell.TryGetValue(cell, out List<int> wallIds)) return false;

        for (int i = 0; i < wallIds.Count; i++)
        {
            int wallIndex = wallIds[i];
            if (wallIndex < 0 || wallIndex >= state.electricWalls.Count) continue;
            if (locallyDisabledWallIds.Contains(wallIndex)) continue;

            DeadlockElectricWall candidate = state.electricWalls[wallIndex];
            if (!candidate.active) continue;

            wall = candidate;
            return true;
        }

        return false;
    }

    private string BuildReleaseOrderText(List<int> releaseOrder)
    {
        if (releaseOrder == null || releaseOrder.Count == 0) return "(none)";

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < releaseOrder.Count; i++)
        {
            if (i > 0) builder.Append(" -> ");
            builder.Append(releaseOrder[i] + 1);
        }

        return builder.ToString();
    }

    private static bool ColorsMatch(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.1f
            && Mathf.Abs(a.g - b.g) < 0.1f
            && Mathf.Abs(a.b - b.b) < 0.1f;
    }

    private static string FormatCell(Vector2Int cell)
    {
        return $"({cell.x}, {cell.y})";
    }

    private static string FormatColor(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGB(color)}";
    }

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
        currentData.electricButtons.Clear();
        currentData.electricWalls.Clear();
        currentData.deflectors.Clear();
        currentData.countdownBlocks.Clear();

        foreach (Transform s in levelContainer) {
            EditorSnakeVisual sb = s.GetComponent<EditorSnakeVisual>();
            if (sb != null && sb.gameObject != currentSelectionGlowObj && sb.LogicNodes != null && sb.LogicNodes.Count > 0) {
                SnakeSaveData e = new SnakeSaveData { direction = sb.direction, arrowColor = sb.snakeColor, segmentPositions = new List<Vector2Int>(sb.LogicNodes) };
                currentData.snakes.Add(e);
            }

            if (s.TryGetComponent(out GridKeycard k))
                currentData.keycards.Add(new KeycardSaveData { position = new Vector2Int((int)s.position.x, (int)s.position.y), color = k.keyColor });
            
            if (s.TryGetComponent(out GridLaserGate g))
                currentData.gates.Add(new GateSaveData { position = new Vector2Int((int)s.position.x, (int)s.position.y), color = g.gateColor });

            if (s.TryGetComponent(out GridElectricButton eb))
                currentData.electricButtons.Add(new ElectricButtonSaveData { position = new Vector2Int((int)s.position.x, (int)s.position.y), color = eb.buttonColor });

            GridDeflector d = s.GetComponentInChildren<GridDeflector>();
            if (d != null)
                currentData.deflectors.Add(new DeflectorSaveData { position = new Vector2Int((int)d.transform.position.x, (int)d.transform.position.y), direction = d.direction });

            if (s.TryGetComponent(out GridCountdownBlock cb))
                currentData.countdownBlocks.Add(new CountdownBlockSaveData { position = new Vector2Int((int)s.position.x, (int)s.position.y), count = cb.count });
        }

        currentData.portals = new List<PortalData>(currentDraftPortals);
        currentData.electricWalls = new List<ElectricWallSaveData>(currentDraftElectricWalls);

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

        if (currentData.electricButtons != null && electricButtonPrefab != null) {
            foreach (var bData in currentData.electricButtons) {
                GameObject b = Instantiate(electricButtonPrefab, new Vector3(bData.position.x, bData.position.y, 0), Quaternion.identity, levelContainer);
                if (b.TryGetComponent(out GridElectricButton script)) script.SetColor(bData.color);
            }
        }

        if (currentData.deflectors != null && deflectorPrefab != null) {
            foreach (var dData in currentData.deflectors) {
                GameObject d = Instantiate(deflectorPrefab, new Vector3(dData.position.x, dData.position.y, 0), GetRotationForDir(dData.direction), levelContainer);
                GridDeflector script = d.GetComponentInChildren<GridDeflector>();
                if (script != null) script.SetDirection(dData.direction);
            }
        }

        if (currentData.countdownBlocks != null && countdownBlockPrefab != null) {
            foreach (var cbData in currentData.countdownBlocks) {
                GameObject cb = Instantiate(countdownBlockPrefab, new Vector3(cbData.position.x, cbData.position.y, 0), Quaternion.identity, levelContainer);
                GridCountdownBlock script = cb.GetComponent<GridCountdownBlock>();
                if (script != null) script.SetCount(cbData.count);
            }
        }

        currentDraftPortals.Clear();
        if (currentData.portals != null)
        {
            foreach(var p in currentData.portals) currentDraftPortals.Add(p);
        }
        RefreshPortalVisuals();

        currentDraftElectricWalls.Clear();
        if (currentData.electricWalls != null)
        {
            for (int i = 0; i < currentData.electricWalls.Count; i++)
            {
                currentDraftElectricWalls.Add(currentData.electricWalls[i]);
            }
        }
        RefreshElectricWallVisuals();
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
        bool hasPosition = false;

        if (currentData.snakes != null)
        {
            for (int i = 0; i < currentData.snakes.Count; i++)
            {
                SnakeSaveData snake = currentData.snakes[i];
                if (snake.segmentPositions == null)
                {
                    continue;
                }

                for (int j = 0; j < snake.segmentPositions.Count; j++)
                {
                    AddBoundsPoint(snake.segmentPositions[j], ref min, ref max, ref hasPosition);
                }
            }
        }

        if (currentData.keycards != null)
        {
            for (int i = 0; i < currentData.keycards.Count; i++)
            {
                AddBoundsPoint(currentData.keycards[i].position, ref min, ref max, ref hasPosition);
            }
        }

        if (currentData.gates != null)
        {
            for (int i = 0; i < currentData.gates.Count; i++)
            {
                AddBoundsPoint(currentData.gates[i].position, ref min, ref max, ref hasPosition);
            }
        }

        if (currentData.electricButtons != null)
        {
            for (int i = 0; i < currentData.electricButtons.Count; i++)
            {
                AddBoundsPoint(currentData.electricButtons[i].position, ref min, ref max, ref hasPosition);
            }
        }

        if (currentData.deflectors != null)
        {
            for (int i = 0; i < currentData.deflectors.Count; i++)
            {
                AddBoundsPoint(currentData.deflectors[i].position, ref min, ref max, ref hasPosition);
            }
        }

        if (currentData.countdownBlocks != null)
        {
            for (int i = 0; i < currentData.countdownBlocks.Count; i++)
            {
                AddBoundsPoint(currentData.countdownBlocks[i].position, ref min, ref max, ref hasPosition);
            }
        }

        if (currentData.portals != null)
        {
            for (int i = 0; i < currentData.portals.Count; i++)
            {
                AddBoundsPoint(currentData.portals[i].entrance, ref min, ref max, ref hasPosition);
                AddBoundsPoint(currentData.portals[i].exit, ref min, ref max, ref hasPosition);
            }
        }

        if (currentData.electricWalls != null)
        {
            for (int i = 0; i < currentData.electricWalls.Count; i++)
            {
                AddBoundsPoint(currentData.electricWalls[i].start, ref min, ref max, ref hasPosition);
                AddBoundsPoint(currentData.electricWalls[i].end, ref min, ref max, ref hasPosition);
            }
        }

        return hasPosition;
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
