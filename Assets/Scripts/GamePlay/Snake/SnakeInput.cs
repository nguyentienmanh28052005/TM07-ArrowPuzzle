using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Solo.MOST_IN_ONE;

public class SnakeInput : MonoBehaviour
{
    [Header("Effect Settings")]
    public float scaleFactor = 1.3f;
    public float duration = 0.2f;
    public float colorChangeDuration = 0.2f;
    public float holdThreshold = 2f;

    [Header("Input Settings")]
    public float clickRadius = 0.8f;
    public float clickRadiusCannotEscape = 0.8f;
    public bool useHaptics = true;
    [SerializeField] private bool boostMobileTouchArea = true;
    [SerializeField, Min(1f)] private float mobileTouchRadiusMultiplier = 1.6f;
    [SerializeField, Min(0f)] private float mobileMinimumTouchRadiusPixels = 120f;
    [SerializeField, Min(1f)] private float releasableInputDistanceDivisor = 3f;

    private bool isPressed = false;
    private bool isHolding = false;
    private Vector2 _lastPointerWorldPosition;
    
    private SnakeBlock parentScript;
    private Coroutine holdCoroutine;
    private ArrowGuideline _guidelineCache;
    private bool _isMemoryMode = false;

    public static List<SnakeInput> AllInputs = new List<SnakeInput>();

    private struct ClickCandidate
    {
        public SnakeInput input;
        public float rawDistance;
        public bool canRelease;
    }

    private static readonly List<ClickCandidate> _clickCandidatesCache = new List<ClickCandidate>(64);
    private static int _selectionCacheFrame = -1;
    private static Vector2 _selectionCachePointer;
    private static bool _selectionCacheUsesReleaseBias;
    private static SnakeInput _selectionCacheWinner;

    private void OnEnable()
    {
        if (!AllInputs.Contains(this)) AllInputs.Add(this);
        SnakeInputManager.EnsureExists();
    }

    private void OnDisable()
    {
        if (AllInputs.Contains(this)) AllInputs.Remove(this);
        if (isPressed)
        {
            isPressed = false;
            isHolding = false;
            if (_isMemoryMode) GameplayInputLock.SetLock(GameplayLockReason.MemoryModeHold, false);
            if (_guidelineCache != null) _guidelineCache.SetLineActive(false);
        }
    }

    private void Awake()
    {
        parentScript = GetComponent<SnakeBlock>();
        if (parentScript == null) parentScript = GetComponentInParent<SnakeBlock>();
    }

    private void Start()
    {
        if (parentScript != null) 
        {
            _guidelineCache = parentScript.GetComponent<ArrowGuideline>();
        }

        LevelDataV2 currentLevelData = PlaytestSession.GetActiveLevelData();
        if (currentLevelData != null)
        {
            _isMemoryMode = currentLevelData.gameMode == GameMode.Memory;
        }
    }

    // =========================================================
    // LÕI TOÁN H?C: ÐO KHO?NG CÁCH T? CHU?T Ð?N TOÀN B? THÂN R?N
    // =========================================================
    public float GetMinDistanceFromMouse(Vector2 mousePos)
    {
        if (parentScript == null || parentScript.LogicNodes == null || parentScript.LogicNodes.Count == 0) return float.MaxValue;
        
        float minDist = float.MaxValue;
        List<Vector3> nodes = parentScript.LogicNodes;
        
        if (nodes.Count == 1) return Vector2.Distance(nodes[0], mousePos);

        for (int i = 0; i < nodes.Count - 1; i++)
        {
            float dist = DistancePointToSegment(mousePos, nodes[i], nodes[i + 1]);
            if (dist < minDist) minDist = dist;
        }
        return minDist;
    }

    private float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float sqrMag = ab.sqrMagnitude;
        if (sqrMag == 0) return Vector2.Distance(p, a);

        float t = Vector2.Dot(p - a, ab) / sqrMag;
        t = Mathf.Clamp01(t); // Gi?i h?n hình chi?u n?m g?n trong do?n th?ng AB
        Vector2 projection = a + t * ab;
        return Vector2.Distance(p, projection);
    }
    // =========================================================

    public bool TryHandleInputDown(Vector2 mousePos)
    {
        if (!PlaytestSession.IsActive && LevelEditor.Instance != null && LevelEditor.Instance.gameObject.activeInHierarchy) return false;
        if (GameplayInputLock.IsLocked) return false;
        if (BoosterTutorialManager.Instance != null && BoosterTutorialManager.Instance.IsBlockingArrowInput) return false;
        if (parentScript != null && parentScript.IsMoving) return false;
        if (parentScript != null && parentScript.IsStoppedByStopBlock && (EraseManager.Instance == null || !EraseManager.Instance.IsEraseModeActive)) return false;
        if (EraseManager.Instance != null && EraseManager.Instance.IsExecutingErase) return false;

        float activeClickRadius = GetActiveClickRadius();
        
        // B?N VÁ: Ðo kho?ng cách v?i toàn thân r?n thay vì ch? Head
        float myDist = GetMinDistanceFromMouse(mousePos);

        if (myDist > activeClickRadius) return false;
        if (!IsClosestToClick(mousePos)) return false;

        if (EraseManager.Instance != null && EraseManager.Instance.IsEraseModeActive)
        {
            GameplayInputLock.SetLock(GameplayLockReason.EraseMode, true);
            EraseManager.Instance.ExecuteErase(parentScript);
            return false; 
        }

        if (HintManager.Instance != null)
        {
            HintManager.Instance.StopHintImmediate();
        }

        isPressed = true;
        isHolding = false; 

        if (parentScript != null)
        {
            parentScript.SetFocusColor(true, colorChangeDuration);
        }

        if (_isMemoryMode)
        {
            GameplayInputLock.SetLock(GameplayLockReason.MemoryModeHold, true);
            if (holdCoroutine != null) StopCoroutine(holdCoroutine);
            holdCoroutine = StartCoroutine(WaitAndScale());
        }

        return true;
    }

    public void HandleInputUp(bool isCanceledByCamera)
    {
        if (!isPressed) return;

        isPressed = false;
        
        if (_isMemoryMode)
        {
            GameplayInputLock.SetLock(GameplayLockReason.MemoryModeHold, false);
        }

        if (holdCoroutine != null) StopCoroutine(holdCoroutine);

        if (parentScript != null)
        {
            parentScript.SetFocusEffect(false, 1f, duration);
            parentScript.SetFocusColor(false, colorChangeDuration); 
        }

        if (_guidelineCache != null)
        {
            _guidelineCache.SetLineActive(false);
        }

        Vector2 mousePos = isCanceledByCamera ? GetCurrentPointerWorldPosition() : _lastPointerWorldPosition;
        float activeClickRadius = GetActiveClickRadius();
        
        // B?N VÁ: Ðo l?i kho?ng cách lúc nh? chu?t v?i toàn thân
        if (!isCanceledByCamera && !CameraController.IsCameraGestureActive && GetMinDistanceFromMouse(mousePos) <= activeClickRadius)
        {
            if (parentScript != null)
            {
                if (!isHolding) 
                {
                    bool success = parentScript.OnHeadClicked();

                    // Tutorial: hide hand/instructions after the first arrow press.
                    if (TutorialManager.Instance != null)
                    {
                        TutorialManager.Instance.NotifyFirstArrowPressed();
                    }

                    if (success)
                    {
                        if (AudioManager.Instance != null) 
                        {
                            AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowTap, 0.8f);
                        }
                        if (useHaptics && SettingManager.Instance != null) 
                        {
                            SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.Selection);
                        }
                    }
                }
            }
        }
        
        isHolding = false;
    }

    public void ManagedLateUpdate(Vector2 mousePos)
    {
        _lastPointerWorldPosition = mousePos;
        if (isPressed)
        {
            if (_isMemoryMode)
            {
                GameplayInputLock.SetLock(GameplayLockReason.MemoryModeHold, true);
            }

            bool isInside = GetMinDistanceFromMouse(mousePos) <= GetActiveClickRadius();

            if (!isInside)
            {
                if (_guidelineCache != null) _guidelineCache.SetLineActive(false);
                if (parentScript != null) parentScript.SetFocusColor(false, colorChangeDuration);
            }
            else
            {
                if (isHolding && _guidelineCache != null) _guidelineCache.SetLineActive(true);
                if (parentScript != null) parentScript.SetFocusColor(true, colorChangeDuration);
            }
        }
    }

    private bool IsClosestToClick(Vector2 mousePos)
    {
        return GetInputAtPointer(mousePos) == this;
    }

    public static SnakeInput GetInputAtPointer(Vector2 mousePos)
    {
        bool useReleaseBias = EraseManager.Instance == null || !EraseManager.Instance.IsEraseModeActive;
        return GetSelectionWinner(mousePos, useReleaseBias);
    }

    private static SnakeInput GetSelectionWinner(Vector2 mousePos, bool useReleaseBias)
    {
        if (_selectionCacheFrame == Time.frameCount
            && _selectionCacheUsesReleaseBias == useReleaseBias
            && (_selectionCachePointer - mousePos).sqrMagnitude < 0.000001f)
        {
            return _selectionCacheWinner;
        }

        _selectionCacheFrame = Time.frameCount;
        _selectionCachePointer = mousePos;
        _selectionCacheUsesReleaseBias = useReleaseBias;
        _selectionCacheWinner = null;
        _clickCandidatesCache.Clear();

        bool hasReleasableCandidate = false;
        bool hasBlockedCandidate = false;

        foreach (var input in AllInputs)
        {
            if (!IsValidSelectionCandidate(input)) continue;

            float distance = input.GetMinDistanceFromMouse(mousePos);
            float clickRadius = input.GetActiveClickRadius();
            if (distance > clickRadius) continue;

            bool canRelease = useReleaseBias && input.CanReleaseForInputSelection();
            if (useReleaseBias)
            {
                if (canRelease) hasReleasableCandidate = true;
                else hasBlockedCandidate = true;
            }

            _clickCandidatesCache.Add(new ClickCandidate
            {
                input = input,
                rawDistance = distance,
                canRelease = canRelease
            });
        }

        bool shouldBiasReleasable = useReleaseBias && hasReleasableCandidate && hasBlockedCandidate;
        float bestRankDistance = float.MaxValue;
        int bestInstanceId = int.MaxValue;

        for (int i = 0; i < _clickCandidatesCache.Count; i++)
        {
            ClickCandidate candidate = _clickCandidatesCache[i];
            float rankDistance = GetRankDistance(candidate, shouldBiasReleasable);
            int instanceId = candidate.input.GetInstanceID();

            if (rankDistance < bestRankDistance
                || (Mathf.Abs(rankDistance - bestRankDistance) < 0.0001f && instanceId < bestInstanceId))
            {
                bestRankDistance = rankDistance;
                bestInstanceId = instanceId;
                _selectionCacheWinner = candidate.input;
            }
        }

        return _selectionCacheWinner;
    }

    private static bool IsValidSelectionCandidate(SnakeInput input)
    {
        bool isEraseMode = EraseManager.Instance != null && EraseManager.Instance.IsEraseModeActive;
        return input != null
            && input.enabled
            && input.gameObject.activeInHierarchy
            && input.parentScript != null
            && !input.parentScript.IsMoving
            && (!input.parentScript.IsStoppedByStopBlock || isEraseMode);
    }

    private bool CanReleaseForInputSelection()
    {
        return parentScript != null && parentScript.CanReleaseNow();
    }

    private static float GetRankDistance(ClickCandidate candidate, bool shouldBiasReleasable)
    {
        if (shouldBiasReleasable && candidate.canRelease)
        {
            float divisor = Mathf.Max(1f, candidate.input.releasableInputDistanceDivisor);
            return candidate.rawDistance / divisor;
        }

        return candidate.rawDistance;
    }

    private float GetActiveClickRadius()
    {
        bool canRelease = parentScript != null && parentScript.CanReleaseNow();
        float radius = canRelease ? clickRadius : clickRadiusCannotEscape;
        
        if (!boostMobileTouchArea || Input.touchCount <= 0) return radius;

        radius *= Mathf.Max(1f, mobileTouchRadiusMultiplier);
        if (mobileMinimumTouchRadiusPixels > 0f)
        {
            radius = Mathf.Max(radius, ScreenPixelsToWorldRadius(mobileMinimumTouchRadiusPixels));
        }

        return radius;
    }

    public static Vector2 GetCurrentPointerWorldPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return Vector2.zero;

        Vector3 screenPos = GetCurrentPointerScreenPosition();
        if (!mainCamera.orthographic)
            screenPos.z = GetCameraDistanceToWorldZ(mainCamera, 0f);

        return mainCamera.ScreenToWorldPoint(screenPos);
    }

    private static Vector3 GetCurrentPointerScreenPosition()
    {
        if (Input.touchCount > 0)
            return Input.GetTouch(0).position;

        return Input.mousePosition;
    }

    private float ScreenPixelsToWorldRadius(float screenPixels)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null || screenPixels <= 0f) return 0f;

        if (mainCamera.orthographic)
            return screenPixels * mainCamera.orthographicSize * 2f / Mathf.Max(1, Screen.height);

        Vector3 screenPos = GetCurrentPointerScreenPosition();
        screenPos.z = GetCameraDistanceToWorldZ(mainCamera, 0f);
        Vector3 worldA = mainCamera.ScreenToWorldPoint(screenPos);
        screenPos.x += screenPixels;
        Vector3 worldB = mainCamera.ScreenToWorldPoint(screenPos);
        return Vector2.Distance(worldA, worldB);
    }

    private static float GetCameraDistanceToWorldZ(Camera camera, float worldZ)
    {
        if (camera == null) return 0f;

        float forwardZ = camera.transform.forward.z;
        if (Mathf.Abs(forwardZ) > 0.0001f)
            return Mathf.Max(camera.nearClipPlane, (worldZ - camera.transform.position.z) / forwardZ);

        return Mathf.Max(camera.nearClipPlane, Mathf.Abs(worldZ - camera.transform.position.z));
    }

    private System.Collections.IEnumerator WaitAndScale()
    {
        yield return new WaitForSeconds(holdThreshold);
        if (isPressed && parentScript != null)
        {
            isHolding = true; 
            parentScript.SetFocusEffect(true, scaleFactor, duration);
            
            if (_guidelineCache != null)
            {
                _guidelineCache.SetLineActive(true);
            }
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();
        if (isPressed && _isMemoryMode) GameplayInputLock.SetLock(GameplayLockReason.MemoryModeHold, false);
    }
}

public class SnakeInputManager : MonoBehaviour
{
    private static SnakeInputManager _instance;

    private SnakeInput _pressedInput;

    public static void EnsureExists()
    {
        if (_instance != null) return;

        GameObject managerObject = new GameObject("SnakeInputManager_Runtime");
        _instance = managerObject.AddComponent<SnakeInputManager>();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        if (CameraController.IsCameraGestureActive)
        {
            ReleasePressedInput(true);
            return;
        }

        if (IsPointerOverUI())
        {
            ReleasePressedInput(true);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandlePointerDown();
        }

        if (Input.GetMouseButtonUp(0))
        {
            ReleasePressedInput(false);
        }
    }

    private void LateUpdate()
    {
        if (_pressedInput == null || !_pressedInput.isActiveAndEnabled)
        {
            _pressedInput = null;
            return;
        }

        Vector2 pointerWorldPosition = SnakeInput.GetCurrentPointerWorldPosition();
        _pressedInput.ManagedLateUpdate(pointerWorldPosition);
    }

    private void HandlePointerDown()
    {
        Vector2 pointerWorldPosition = SnakeInput.GetCurrentPointerWorldPosition();
        SnakeInput targetInput = SnakeInput.GetInputAtPointer(pointerWorldPosition);
        if (targetInput == null) return;

        if (targetInput.TryHandleInputDown(pointerWorldPosition))
        {
            _pressedInput = targetInput;
        }
    }

    private void ReleasePressedInput(bool isCanceledByCamera)
    {
        if (_pressedInput == null)
        {
            _pressedInput = null;
            return;
        }

        SnakeInput input = _pressedInput;
        _pressedInput = null;
        if (!input.isActiveAndEnabled) return;

        input.HandleInputUp(isCanceledByCamera);
    }

    private static bool IsPointerOverUI()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
            {
                return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId);
            }
        }

        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
}
