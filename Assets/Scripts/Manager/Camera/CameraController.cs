using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening; 

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public static bool IsGameplayBlocking = false;
    public static bool IsCameraInputBlocked = false;
    
    public static bool IsCameraGestureActive = false; 
    public static event System.Action OnIntroFinished;

    [Header("Zoom Settings (Gameplay)")]
    public float zoomSpeedPC = 5f;
    public float zoomSpeedMobile = 0.01f;
    public float minZoom = 5f;
    public float maxZoom = 30f; 
    public float zoomSmoothTime = 0.1f;

    [Header("Auto Fit & Intro Settings (Cinematic)")]
    public float autoFitPadding = 8f;
    public float defaultGameplayZoom = 40f; 
    public float overviewWaitTime = 1f;
    [SerializeField, Min(0f)] private float returnToDefaultZoomDuration = 0.6f;

    [Header("Pan Settings")]
    public bool useLimits = true;
    public Vector2 minPosition;
    public Vector2 maxPosition;
    public float dragThreshold = 5f;

    [Header("Auto Limits From Level")]
    [Tooltip("Tự động tính vùng giới hạn camera theo Bounds của màn chơi (từ LevelDataSO).")]
    public bool autoComputeLimitsFromLevel = true;
    [Tooltip("Mở rộng vùng giới hạn lớn hơn Bounds của màn chơi (world units / grid units).")]
    public float limitsPadding = 6f;
    [Tooltip("Clamp theo kích thước khung nhìn hiện tại (orthographicSize & aspect) để không kéo camera vượt khỏi bounds.")]
    public bool limitsConsiderCurrentZoom = true;

    private bool _hasLevelLimitBounds = false;
    private Vector2 _levelLimitMin;
    private Vector2 _levelLimitMax;

    [Header("Inertia Settings")]
    public bool useInertia = true;
    public float dampingFactor = 8f;

    private Camera cam;
    private float targetZoom;
    private float zoomVelocity; 
    private float gameZoom; 
    
    private Vector3 initialPosition; 
    private Vector3 panVelocity;
    private Vector3 lastPanScreenPos;
    
    private bool isEndGame = false;
    private Coroutine _introRoutine;
    private bool wasZoomingLastFrame = false;

    private Tween _focusTween;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void SetLevelLimitBounds(Bounds levelBounds)
    {
        float pad = Mathf.Max(0f, limitsPadding);
        levelBounds.Expand(new Vector3(pad * 2f, pad * 2f, 0f));

        _levelLimitMin = new Vector2(levelBounds.min.x, levelBounds.min.y);
        _levelLimitMax = new Vector2(levelBounds.max.x, levelBounds.max.y);
        _hasLevelLimitBounds = true;

        // Keep legacy inspector fields in sync for debugging/visibility.
        minPosition = _levelLimitMin;
        maxPosition = _levelLimitMax;
    }

    private Vector3 ClampToLimits(Vector3 worldPos)
    {
        if (!useLimits) return worldPos;

        Vector2 min = _hasLevelLimitBounds ? _levelLimitMin : minPosition;
        Vector2 max = _hasLevelLimitBounds ? _levelLimitMax : maxPosition;

        float halfH = 0f;
        float halfW = 0f;
        if (limitsConsiderCurrentZoom && cam != null && cam.orthographic)
        {
            halfH = cam.orthographicSize;
            halfW = cam.orthographicSize * cam.aspect;
        }

        float minX = min.x + halfW;
        float maxX = max.x - halfW;
        float minY = min.y + halfH;
        float maxY = max.y - halfH;

        // If bounds are smaller than view, fall back to clamping camera center to raw bounds.
        if (minX > maxX)
        {
            minX = min.x;
            maxX = max.x;
        }
        if (minY > maxY)
        {
            minY = min.y;
            maxY = max.y;
        }

        worldPos.x = Mathf.Clamp(worldPos.x, minX, maxX);
        worldPos.y = Mathf.Clamp(worldPos.y, minY, maxY);
        return worldPos;
    }

    private bool TryComputeLevelBoundsFromData(LevelDataSO data, out Bounds bounds)
    {
        bounds = default;
        if (data == null) return false;

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        bool hasAny = false;

        if (data.snakes != null)
        {
            foreach (var snakeData in data.snakes)
            {
                if (snakeData == null || snakeData.segmentPositions == null) continue;
                foreach (Vector2Int gridPos in snakeData.segmentPositions)
                {
                    if (gridPos.x < minX) minX = gridPos.x;
                    if (gridPos.x > maxX) maxX = gridPos.x;
                    if (gridPos.y < minY) minY = gridPos.y;
                    if (gridPos.y > maxY) maxY = gridPos.y;
                    hasAny = true;
                }
            }
        }

        if (data.keycards != null)
        {
            foreach (var k in data.keycards)
            {
                if (k.position.x < minX) minX = k.position.x;
                if (k.position.x > maxX) maxX = k.position.x;
                if (k.position.y < minY) minY = k.position.y;
                if (k.position.y > maxY) maxY = k.position.y;
                hasAny = true;
            }
        }

        if (data.gates != null)
        {
            foreach (var g in data.gates)
            {
                if (g.position.x < minX) minX = g.position.x;
                if (g.position.x > maxX) maxX = g.position.x;
                if (g.position.y < minY) minY = g.position.y;
                if (g.position.y > maxY) maxY = g.position.y;
                hasAny = true;
            }
        }

        if (data.revealWaveButtons != null)
        {
            foreach (var b in data.revealWaveButtons)
            {
                if (b.position.x < minX) minX = b.position.x;
                if (b.position.x > maxX) maxX = b.position.x;
                if (b.position.y < minY) minY = b.position.y;
                if (b.position.y > maxY) maxY = b.position.y;
                hasAny = true;
            }
        }

        if (data.portals != null)
        {
            foreach (var p in data.portals)
            {
                if (p.entrance.x < minX) minX = p.entrance.x;
                if (p.entrance.x > maxX) maxX = p.entrance.x;
                if (p.entrance.y < minY) minY = p.entrance.y;
                if (p.entrance.y > maxY) maxY = p.entrance.y;

                if (p.exit.x < minX) minX = p.exit.x;
                if (p.exit.x > maxX) maxX = p.exit.x;
                if (p.exit.y < minY) minY = p.exit.y;
                if (p.exit.y > maxY) maxY = p.exit.y;

                hasAny = true;
            }
        }

        if (data.deflectors != null)
        {
            foreach (var d in data.deflectors)
            {
                if (d.position.x < minX) minX = d.position.x;
                if (d.position.x > maxX) maxX = d.position.x;
                if (d.position.y < minY) minY = d.position.y;
                if (d.position.y > maxY) maxY = d.position.y;
                hasAny = true;
            }
        }

        if (data.countdownBlocks != null)
        {
            foreach (var block in data.countdownBlocks)
            {
                if (block.position.x < minX) minX = block.position.x;
                if (block.position.x > maxX) maxX = block.position.x;
                if (block.position.y < minY) minY = block.position.y;
                if (block.position.y > maxY) maxY = block.position.y;
                hasAny = true;
            }
        }

        if (data.stopBlocks != null)
        {
            foreach (var block in data.stopBlocks)
            {
                if (block.position.x < minX) minX = block.position.x;
                if (block.position.x > maxX) maxX = block.position.x;
                if (block.position.y < minY) minY = block.position.y;
                if (block.position.y > maxY) maxY = block.position.y;
                hasAny = true;
            }
        }

        if (data.turnStateBlocks != null)
        {
            foreach (var block in data.turnStateBlocks)
            {
                if (block.position.x < minX) minX = block.position.x;
                if (block.position.x > maxX) maxX = block.position.x;
                if (block.position.y < minY) minY = block.position.y;
                if (block.position.y > maxY) maxY = block.position.y;
                hasAny = true;
            }
        }

        if (data.blackHoles != null)
        {
            foreach (var block in data.blackHoles)
            {
                if (block.position.x < minX) minX = block.position.x;
                if (block.position.x > maxX) maxX = block.position.x;
                if (block.position.y < minY) minY = block.position.y;
                if (block.position.y > maxY) maxY = block.position.y;
                hasAny = true;
            }
        }

        if (!hasAny) return false;

        float width = Mathf.Max(0f, maxX - minX);
        float height = Mathf.Max(0f, maxY - minY);
        Vector3 center = new Vector3(minX + width / 2f, minY + height / 2f, 0f);
        Vector3 size = new Vector3(Mathf.Max(1f, width), Mathf.Max(1f, height), 0f);
        bounds = new Bounds(center, size);
        return true;
    }

    public void FocusOnWorldPosition(Vector3 worldPosition, float duration = 0.35f, bool blockCameraInput = true, Ease ease = Ease.InOutSine)
    {
        if (cam == null) cam = GetComponent<Camera>();

        Vector3 target = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
        target = ClampToLimits(target);

        if (_focusTween != null && _focusTween.IsActive()) _focusTween.Kill();

        panVelocity = Vector3.zero;
        IsCameraGestureActive = false;

        if (blockCameraInput) IsCameraInputBlocked = true;

        _focusTween = transform.DOMove(target, duration)
            .SetEase(ease)
            .OnComplete(() =>
            {
                if (blockCameraInput) IsCameraInputBlocked = false;
            })
            .SetLink(gameObject);
    }

    public void StartIntro()
    {
        if (_introRoutine != null)
        {
            StopCoroutine(_introRoutine);
        }

        ResetEndGameState();
        _introRoutine = StartCoroutine(CameraIntroSequence());
    }

    public void PrepareDefaultForLevel(LevelDataSO levelData)
    {
        if (levelData == null) return;
        if (cam == null) cam = GetComponent<Camera>();

        if (!TryComputeLevelBoundsFromData(levelData, out Bounds levelBounds))
        {
            ResetEndGameState();
            if (cam != null) cam.orthographicSize = defaultGameplayZoom;
            targetZoom = defaultGameplayZoom;
            return;
        }

        ResetEndGameState();

        if (autoComputeLimitsFromLevel)
        {
            SetLevelLimitBounds(levelBounds);
        }

        float width = levelBounds.size.x;
        float height = levelBounds.size.y;
        initialPosition = new Vector3(levelBounds.center.x, levelBounds.center.y, transform.position.z);

        float sizeByHeight = height / 2f;
        float sizeByWidth = (width / 2f) / cam.aspect;

        gameZoom = Mathf.Max(sizeByHeight, sizeByWidth) + autoFitPadding;
        maxZoom = Mathf.Max(maxZoom, gameZoom);

        transform.position = initialPosition;
        cam.orthographicSize = defaultGameplayZoom;
        targetZoom = defaultGameplayZoom;
        zoomVelocity = 0f;
        panVelocity = Vector3.zero;
    }

    private void ResetEndGameState()
    {
        isEndGame = false;
        targetZoom = defaultGameplayZoom;
        zoomVelocity = 0f;
        panVelocity = Vector3.zero;
    }

    private IEnumerator CameraIntroSequence()
    {
        IsGameplayBlocking = true; 

        LevelLoader loader = FindObjectOfType<LevelLoader>();
        
        if (loader == null || loader.levelToPlay == null || loader.levelToPlay.snakes == null)
        {
            if (cam == null) cam = GetComponent<Camera>();
            ResetEndGameState();
            if (cam != null) cam.orthographicSize = defaultGameplayZoom;
            targetZoom = defaultGameplayZoom;
            IsGameplayBlocking = false;
            OnIntroFinished?.Invoke();
            yield break;
        }

        if (cam == null) cam = GetComponent<Camera>();

        if (!TryComputeLevelBoundsFromData(loader.levelToPlay, out Bounds levelBounds))
        {
            ResetEndGameState();
            if (cam != null) cam.orthographicSize = defaultGameplayZoom;
            targetZoom = defaultGameplayZoom;
            IsGameplayBlocking = false;
            OnIntroFinished?.Invoke();
            yield break;
        }

        if (autoComputeLimitsFromLevel)
        {
            SetLevelLimitBounds(levelBounds);
        }

        float width = levelBounds.size.x;
        float height = levelBounds.size.y;
        initialPosition = new Vector3(levelBounds.center.x, levelBounds.center.y, transform.position.z);

        float sizeByHeight = height / 2f;
        float sizeByWidth = (width / 2f) / cam.aspect;

        gameZoom = Mathf.Max(sizeByHeight, sizeByWidth) + autoFitPadding;
        maxZoom = Mathf.Max(maxZoom, gameZoom);

        transform.position = initialPosition;
        
        //cam.orthographicSize = defaultGameplayZoom / 3; 
        cam.DOKill();

        // ==========================================
        // KÍCH HOẠT UI CINEMATIC ĐỒNG BỘ VỚI CAMERA
        // ==========================================
        GameCanvas canvas = FindObjectOfType<GameCanvas>();
        bool shouldReturnToDefaultZoom = loader.levelToPlay.returnToDefaultZoomAfterIntro;
        if (canvas != null)
        {
            // Tính toán thời gian UI nằm trên màn hình:
            // = zoom to overview + hold overview + optional return to gameplay zoom.
            float totalHoldTime = 1f + overviewWaitTime + (shouldReturnToDefaultZoom ? returnToDefaultZoomDuration : 0f);
            
            // Nếu chỉ muốn hiện cho level Hard:
            // if (loader.levelToPlay.levelDifficulty == LevelDifficulty.Hard)
            canvas.PlayCinematicIntro(loader.levelToPlay.levelDifficulty, totalHoldTime);
        }
        
        // 1. Camera Zoom ra nhìn toàn cảnh
        Tween overviewTween = cam.DOOrthoSize(gameZoom, 1f).SetEase(Ease.InOutSine);
        yield return overviewTween.WaitForCompletion();

        // 2. Chờ người chơi nhìn bao quát (Lúc này UI đang rung nhè nhẹ)
        yield return new WaitForSeconds(overviewWaitTime);

        // 3. Quay lại zoom gameplay mặc định nếu level yêu cầu.
        if (shouldReturnToDefaultZoom && returnToDefaultZoomDuration > 0f)
        {
            Tween defaultZoomTween = cam.DOOrthoSize(defaultGameplayZoom, returnToDefaultZoomDuration).SetEase(Ease.InOutSine);
            yield return defaultZoomTween.WaitForCompletion();
        }
        else if (shouldReturnToDefaultZoom)
        {
            cam.orthographicSize = defaultGameplayZoom;
        }

        targetZoom = shouldReturnToDefaultZoom ? defaultGameplayZoom : gameZoom;
        cam.orthographicSize = targetZoom;
        zoomVelocity = 0f;

        IsGameplayBlocking = false;
        OnIntroFinished?.Invoke();
    }
    
    void LateUpdate() 
    {
        if (IsGameplayBlocking || IsCameraInputBlocked)
        {
            IsCameraGestureActive = false;
            return; 
        }

        if (isEndGame)
        {
            IsCameraGestureActive = false;
            HandleEndGame();
            return;
        }

        HandleInput();
        ApplyMovementAndZoom();
    }

    private void HandleInput()
    {
        if (Input.touchCount > 0) HandleTouchInput();
        else HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            targetZoom -= scroll * zoomSpeedPC;
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            lastPanScreenPos = Input.mousePosition;
            IsCameraGestureActive = false;
        }

        if (Input.GetMouseButton(1))
        {
            ProcessPan(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(1))
        {
            IsCameraGestureActive = false;
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0 && EventSystem.current != null)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;
        }

        if (Input.touchCount >= 2)
        {
            wasZoomingLastFrame = true;
            IsCameraGestureActive = true;

            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 t0Prev = t0.position - t0.deltaPosition;
            Vector2 t1Prev = t1.position - t1.deltaPosition;

            float prevMag = (t0Prev - t1Prev).magnitude;
            float curMag = (t0.position - t1.position).magnitude;

            targetZoom -= (curMag - prevMag) * zoomSpeedMobile;
            
            lastPanScreenPos = (t0.position + t1.position) * 0.5f;
        }
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (wasZoomingLastFrame)
            {
                lastPanScreenPos = touch.position;
                wasZoomingLastFrame = false;
                return;
            }

            if (touch.phase == TouchPhase.Began)
            {
                lastPanScreenPos = touch.position;
                IsCameraGestureActive = false;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                ProcessPan(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                IsCameraGestureActive = false;
            }
        }
        else
        {
            wasZoomingLastFrame = false;
        }
    }

    private void ProcessPan(Vector3 currentScreenPos)
    {
        if (!IsCameraGestureActive)
        {
            if (Vector3.Distance(currentScreenPos, lastPanScreenPos) > dragThreshold)
            {
                IsCameraGestureActive = true;
                lastPanScreenPos = currentScreenPos;
            }
        }

        if (IsCameraGestureActive)
        {
            Vector3 worldDelta = cam.ScreenToWorldPoint(lastPanScreenPos) - cam.ScreenToWorldPoint(currentScreenPos);
            transform.position += worldDelta;

            if (useInertia) panVelocity = worldDelta / Time.deltaTime;

            lastPanScreenPos = currentScreenPos;
        }
    }

    private void ApplyMovementAndZoom()
    {
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothTime);

        if (useInertia && !IsCameraGestureActive && panVelocity.sqrMagnitude > 0.0001f)
        {
            transform.position += panVelocity * Time.deltaTime;
            panVelocity = Vector3.Lerp(panVelocity, Vector3.zero, dampingFactor * Time.deltaTime);
        }

        if (useLimits)
        {
            transform.position = ClampToLimits(transform.position);
        }
    }

    private void HandleEndGame()
    {
        transform.position = Vector3.Lerp(transform.position, initialPosition, Time.deltaTime * 2f);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, gameZoom, Time.deltaTime * 2f);
    }

    public void ZoomToEndGame()
    {
        isEndGame = true;
        targetZoom = gameZoom;
        panVelocity = Vector3.zero;
    }
}
