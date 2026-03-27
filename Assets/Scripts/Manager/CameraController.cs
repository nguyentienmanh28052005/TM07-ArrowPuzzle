using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening; // BẮT BUỘC PHẢI CÓ ĐỂ CHẠY DOTWEEN

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public static bool IsGameplayBlocking = false;
    public static bool IsDragging = false;

    [Header("Zoom Settings (Gameplay)")]
    public float zoomSpeedPC = 5f;
    public float zoomSpeedMobile = 0.01f;
    public float minZoom = 5f;
    public float maxZoom = 30f; 
    public float zoomSmoothTime = 0.1f;

    [Header("Auto Fit & Intro Settings (Cinematic)")]
    public float autoFitPadding = 8f;
    [Tooltip("Độ zoom cận cảnh đồng bộ lúc bắt đầu chơi")]
    public float defaultGameplayZoom = 40f; 
    [Tooltip("Thời gian dừng hình để nhìn toàn cảnh trước khi zoom vào")]
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

    /// <summary>
    /// Khởi tạo tham chiếu và gọi kịch bản Intro Camera.
    /// </summary>
    private IEnumerator Start()
    {
        cam = GetComponent<Camera>();
        yield return new WaitForEndOfFrame(); 
        yield return StartCoroutine(CameraIntroSequence());
    }

    /// <summary>
    /// KỊCH BẢN ĐẠO DIỄN BẰNG DOTWEEN (Siêu Mượt)
    /// </summary>
    private IEnumerator CameraIntroSequence()
    {
        IsGameplayBlocking = true; 

        SnakeBlock[] allSnakes = FindObjectsOfType<SnakeBlock>();
        if (allSnakes.Length == 0)
        {
            IsGameplayBlocking = false;
            yield break;
        }

        // TÍNH TOÁN KÍCH THƯỚC BẢN ĐỒ
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        bool hasNodes = false;

        foreach (var snake in allSnakes)
        {
            if (snake.bodySegments == null) continue;
            foreach (Transform seg in snake.bodySegments)
            {
                if (seg != null)
                {
                    Vector3 pos = seg.position;
                    if (pos.x < minX) minX = pos.x;
                    if (pos.x > maxX) maxX = pos.x;
                    if (pos.y < minY) minY = pos.y;
                    if (pos.y > maxY) maxY = pos.y;
                    hasNodes = true;
                }
            }
        }

        if (!hasNodes) 
        {
            IsGameplayBlocking = false;
            yield break;
        }

        float width = maxX - minX;
        float height = maxY - minY;
        initialPosition = new Vector3(minX + width / 2f, minY + height / 2f, transform.position.z);

        float sizeByHeight = height / 2f;
        float sizeByWidth = (width / 2f) / cam.aspect;
        
        gameZoom = Mathf.Max(sizeByHeight, sizeByWidth) + autoFitPadding;
        maxZoom = Mathf.Max(maxZoom, gameZoom + 5f);

        // ==========================================
        // THỰC THI KỊCH BẢN ĐẠO DIỄN DOTWEEN
        // ==========================================

        // NHỊP 1: Set cứng vị trí ở tít trên cao
        transform.position = initialPosition;
        cam.orthographicSize = maxZoom; 
        
        // Dùng DOTween lướt mượt mà vào Toàn Cảnh (gameZoom) trong 1.5 giây với gia tốc InOutSine
        cam.DOKill();
        cam.DOOrthoSize(gameZoom, 1f).SetEase(Ease.InOutSine);
        yield return new WaitForSeconds(1.5f);

        // NHỊP 2: Dừng hình hoàn toàn để người chơi quét mắt nhìn map
        yield return new WaitForSeconds(overviewWaitTime);

        // NHỊP 3: Dùng DOTween lướt tiếp vào Cận Cảnh (defaultGameplayZoom) cực êm
        cam.DOOrthoSize(defaultGameplayZoom, 1.2f).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(1.2f);

        // ĐỒNG BỘ: Chốt chặn an toàn để khi nhả DOTween ra, vật lý không bị giật lùi
        targetZoom = defaultGameplayZoom;
        cam.orthographicSize = defaultGameplayZoom; 
        zoomVelocity = 0f;

        // NHỊP 4: Hoàn tất Intro, cắm điện trả lại hệ thống kéo/vuốt cho người chơi
        IsGameplayBlocking = false;
    }

    /// <summary>
    /// Xử lý cập nhật chuyển động ở cuối mỗi khung hình.
    /// </summary>
    void LateUpdate() 
    {
        // KHI ĐANG CHẠY INTRO: Ngắt điện hệ thống SmoothDamp, nhường sân khấu 100% cho DOTween
        if (IsGameplayBlocking)
        {
            IsDragging = false;
            return; 
        }

        if (isEndGame)
        {
            IsDragging = false;
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
            IsDragging = false;
        }

        if (Input.GetMouseButton(1))
        {
            ProcessPan(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(1))
        {
            IsDragging = false;
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
            IsDragging = true;

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
                IsDragging = false;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                ProcessPan(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                IsDragging = false;
            }
        }
        else
        {
            wasZoomingLastFrame = false;
        }
    }

    private void ProcessPan(Vector3 currentScreenPos)
    {
        if (!IsDragging)
        {
            if (Vector3.Distance(currentScreenPos, lastPanScreenPos) > dragThreshold)
            {
                IsDragging = true;
                lastPanScreenPos = currentScreenPos;
            }
        }

        if (IsDragging)
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

        if (useInertia && !IsDragging && panVelocity.sqrMagnitude > 0.0001f)
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