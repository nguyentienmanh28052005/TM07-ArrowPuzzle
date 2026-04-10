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

    [Header("Pan Settings")]
    public bool useLimits = true;
    public Vector2 minPosition;
    public Vector2 maxPosition;
    public float dragThreshold = 5f;

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
    private bool wasZoomingLastFrame = false;

    private Tween _focusTween;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    public void FocusOnWorldPosition(Vector3 worldPosition, float duration = 0.35f, bool blockCameraInput = true, Ease ease = Ease.InOutSine)
    {
        if (cam == null) cam = GetComponent<Camera>();

        Vector3 target = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);

        if (useLimits)
        {
            target.x = Mathf.Clamp(target.x, minPosition.x, maxPosition.x);
            target.y = Mathf.Clamp(target.y, minPosition.y, maxPosition.y);
        }

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
        StartCoroutine(CameraIntroSequence());
    }

    private IEnumerator CameraIntroSequence()
    {
        IsGameplayBlocking = true; 

        // =======================================================
        // BẢN VÁ DATA-DRIVEN: ĐỌC THẲNG TỪ DATA (KHÔNG TÌM GAMEOBJECT)
        // =======================================================
        LevelLoader loader = FindObjectOfType<LevelLoader>();
        
        // Nếu không có LevelLoader hoặc chưa có Data, thoát luôn để chống kẹt
        if (loader == null || loader.levelToPlay == null || loader.levelToPlay.snakes == null)
        {
            IsGameplayBlocking = false;
            OnIntroFinished?.Invoke();
            yield break;
        }

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        bool hasNodes = false;

        // Vòng lặp duyệt thuần con số toán học (Siêu nhẹ cho CPU)
        foreach (var snakeData in loader.levelToPlay.snakes)
        {
            if (snakeData.segmentPositions == null) continue;
            foreach (Vector2Int gridPos in snakeData.segmentPositions)
            {
                if (gridPos.x < minX) minX = gridPos.x;
                if (gridPos.x > maxX) maxX = gridPos.x;
                if (gridPos.y < minY) minY = gridPos.y;
                if (gridPos.y > maxY) maxY = gridPos.y;
                hasNodes = true;
            }
        }

        if (!hasNodes) 
        {
            IsGameplayBlocking = false;
            OnIntroFinished?.Invoke();
            yield break;
        }

        // Tính toán kích thước bàn cờ dựa trên con số Min/Max
        float width = maxX - minX;
        float height = maxY - minY;
        initialPosition = new Vector3(minX + width / 2f, minY + height / 2f, transform.position.z);

        float sizeByHeight = height / 2f;
        float sizeByWidth = (width / 2f) / cam.aspect;
        
        gameZoom = Mathf.Max(sizeByHeight, sizeByWidth) + autoFitPadding;
        maxZoom = Mathf.Max(maxZoom, gameZoom + 5f);

        transform.position = initialPosition;
        
        // 0. Bắt đầu ở góc nhìn mặc định
        cam.orthographicSize = defaultGameplayZoom / 3; 
        cam.DOKill();
        
        // 1. Zoom đến gameZoom (để nhìn vừa vặn toàn bộ bàn cờ)
        cam.DOOrthoSize(gameZoom, 1f).SetEase(Ease.InOutSine);
        yield return new WaitForSeconds(1f);

        // Đợi 1 chút để người chơi quan sát tổng thể map
        yield return new WaitForSeconds(overviewWaitTime);

        // 2. Zoom ngược lại về defaultGameplayZoom để bắt đầu chơi
        cam.DOOrthoSize(defaultGameplayZoom, 1.2f).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(1.2f);

        targetZoom = defaultGameplayZoom;
        cam.orthographicSize = defaultGameplayZoom; 
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
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
            pos.y = Mathf.Clamp(pos.y, minPosition.y, maxPosition.y);
            transform.position = pos;
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