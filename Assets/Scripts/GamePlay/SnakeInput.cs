using UnityEngine;
using UnityEngine.EventSystems; 
using DG.Tweening;
using System.Collections.Generic;
using Solo.MOST_IN_ONE;

public class SnakeInput : MonoBehaviour
{
    [Header("Effect Settings")]
    public float scaleFactor = 1.3f;
    public float duration = 0.2f;
    public float holdThreshold = 2f;

    [Header("Input Settings")]
    public float clickRadius = 0.8f;
    public bool useHaptics = true;
    [SerializeField] private bool boostMobileTouchArea = true;
    [SerializeField, Min(1f)] private float mobileTouchRadiusMultiplier = 1.6f;
    [SerializeField, Min(0f)] private float mobileMinimumTouchRadiusPixels = 120f;
    [SerializeField, Min(1f)] private float releasableInputDistanceDivisor = 3f;

    private bool isPressed = false;
    private bool isHolding = false;
    
    private SnakeBlock parentScript;
    private Coroutine holdCoroutine;
    private ArrowGuideline _guidelineCache;
    private LevelEditor _levelEditor;
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
    }

    private void OnDisable()
    {
        if (AllInputs.Contains(this)) AllInputs.Remove(this);
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
        
        _levelEditor = FindObjectOfType<LevelEditor>();

        LevelDataSO currentLevelData = PlaytestSession.GetActiveLevelData();
        if (currentLevelData != null)
        {
            _isMemoryMode = currentLevelData.gameMode == GameMode.Memory;
        }
    }

    private void Update()
    {
        if (!PlaytestSession.IsActive && _levelEditor != null && _levelEditor.gameObject.activeInHierarchy) return;
        if (Time.timeScale == 0f) return;

        if (CameraController.IsCameraGestureActive)
        {
            if (isPressed) HandleInputUp(true); 
            return; 
        }

        if (IsPointerOverUI()) 
        { 
            if (isPressed) HandleInputUp(true); 
            return; 
        }

        if (Input.GetMouseButtonDown(0)) HandleInputDown();
        if (Input.GetMouseButtonUp(0)) HandleInputUp(false);
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
            }
        }
        
        return EventSystem.current.IsPointerOverGameObject();
    }

    // =========================================================
    // LÕI TOÁN HỌC: ĐO KHOẢNG CÁCH TỪ CHUỘT ĐẾN TOÀN BỘ THÂN RẮN
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
        t = Mathf.Clamp01(t); // Giới hạn hình chiếu nằm gọn trong đoạn thẳng AB
        Vector2 projection = a + t * ab;
        return Vector2.Distance(p, projection);
    }
    // =========================================================

    private void HandleInputDown()
    {
        if (CameraController.IsGameplayBlocking) return;
        if (BoosterTutorialManager.Instance != null && BoosterTutorialManager.Instance.IsBlockingArrowInput) return;
        if (parentScript != null && parentScript.IsMoving) return;
        if (EraseManager.Instance != null && EraseManager.Instance.IsExecutingErase) return;

        Vector2 mousePos = GetCurrentPointerWorldPosition();
        float activeClickRadius = GetActiveClickRadius();
        
        // BẢN VÁ: Đo khoảng cách với toàn thân rắn thay vì chỉ Head
        float myDist = GetMinDistanceFromMouse(mousePos);

        if (myDist > activeClickRadius) return;
        if (!IsClosestToClick(mousePos)) return;

        if (EraseManager.Instance != null && EraseManager.Instance.IsEraseModeActive)
        {
            CameraController.IsGameplayBlocking = true;
            EraseManager.Instance.ExecuteErase(parentScript);
            return; 
        }

        if (HintManager.Instance != null)
        {
            HintManager.Instance.StopHintImmediate();
        }

        isPressed = true;
        isHolding = false; 

        if (parentScript != null)
        {
            parentScript.SetFocusColor(true, duration);
        }

        if (_isMemoryMode)
        {
            CameraController.IsGameplayBlocking = true;
            if (holdCoroutine != null) StopCoroutine(holdCoroutine);
            holdCoroutine = StartCoroutine(WaitAndScale());
        }
    }

    private void HandleInputUp(bool isCanceledByCamera)
    {
        if (!isPressed) return;

        isPressed = false;
        
        if (_isMemoryMode)
        {
            CameraController.IsGameplayBlocking = false;
        }

        if (holdCoroutine != null) StopCoroutine(holdCoroutine);

        if (parentScript != null)
        {
            parentScript.SetFocusEffect(false, 1f, duration);
            parentScript.SetFocusColor(false, duration); 
        }

        if (_guidelineCache != null)
        {
            _guidelineCache.SetLineActive(false);
        }

        Vector2 mousePos = GetCurrentPointerWorldPosition();
        float activeClickRadius = GetActiveClickRadius();
        
        // BẢN VÁ: Đo lại khoảng cách lúc nhả chuột với toàn thân
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

    private void LateUpdate()
    {
        if (isPressed)
        {
            if (_isMemoryMode)
            {
                CameraController.IsGameplayBlocking = true;
            }

            Vector2 mousePos = GetCurrentPointerWorldPosition();
            bool isInside = GetMinDistanceFromMouse(mousePos) <= GetActiveClickRadius();

            if (!isInside)
            {
                if (_guidelineCache != null) _guidelineCache.SetLineActive(false);
                if (parentScript != null) parentScript.SetFocusColor(false, duration);
            }
            else
            {
                if (isHolding && _guidelineCache != null) _guidelineCache.SetLineActive(true);
                if (parentScript != null) parentScript.SetFocusColor(true, duration);
            }
        }
    }

    private bool IsClosestToClick(Vector2 mousePos)
    {
        bool useReleaseBias = EraseManager.Instance == null || !EraseManager.Instance.IsEraseModeActive;
        return GetSelectionWinner(mousePos, useReleaseBias) == this;
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
        return input != null
            && input.enabled
            && input.gameObject.activeInHierarchy
            && input.parentScript != null
            && !input.parentScript.IsMoving;
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
        float radius = clickRadius;
        if (!boostMobileTouchArea || Input.touchCount <= 0) return radius;

        radius *= Mathf.Max(1f, mobileTouchRadiusMultiplier);
        if (mobileMinimumTouchRadiusPixels > 0f)
        {
            radius = Mathf.Max(radius, ScreenPixelsToWorldRadius(mobileMinimumTouchRadiusPixels));
        }

        return radius;
    }

    private Vector2 GetCurrentPointerWorldPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return Vector2.zero;

        Vector3 screenPos = GetCurrentPointerScreenPosition();
        if (!mainCamera.orthographic)
            screenPos.z = GetCameraDistanceToWorldZ(mainCamera, 0f);

        return mainCamera.ScreenToWorldPoint(screenPos);
    }

    private Vector3 GetCurrentPointerScreenPosition()
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

    private float GetCameraDistanceToWorldZ(Camera camera, float worldZ)
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
        if (isPressed && _isMemoryMode) CameraController.IsGameplayBlocking = false;
    }
}
