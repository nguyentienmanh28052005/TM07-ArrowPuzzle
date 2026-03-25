using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public static bool IsGameplayBlocking = false;
    public static bool IsDragging = false;

    [Header("Zoom Settings")]
    public float zoomSpeedPC = 5f;
    public float zoomSpeedMobile = 0.01f;
    public float minZoom = 5f;
    public float maxZoom = 30f; 
    public float zoomSmoothTime = 0.1f;

    [Header("Auto Fit Settings")]
    public float autoFitPadding = 3f;

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
    /// Khởi tạo tham chiếu và gọi tính toán AutoFit sau khi các Entity đã được sinh ra.
    /// </summary>
    private IEnumerator Start()
    {
        cam = GetComponent<Camera>();
        yield return new WaitForEndOfFrame(); 
        AutoFitMap();
    }

    /// <summary>
    /// Quét toàn bộ khối rắn để tìm điểm cực đại, tự động căn giữa và định cỡ Zoom cho bản đồ.
    /// </summary>
    private void AutoFitMap()
    {
        SnakeBlock[] allSnakes = FindObjectsOfType<SnakeBlock>();
        if (allSnakes.Length == 0) return;

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

        if (!hasNodes) return;

        float width = maxX - minX;
        float height = maxY - minY;
        
        initialPosition = new Vector3(minX + width / 2f, minY + height / 2f, transform.position.z);

        float sizeByHeight = height / 2f;
        float sizeByWidth = (width / 2f) / cam.aspect;
        
        gameZoom = Mathf.Max(sizeByHeight, sizeByWidth) + autoFitPadding;
        maxZoom = Mathf.Max(maxZoom, gameZoom + 5f);

        transform.position = initialPosition;
        cam.orthographicSize = maxZoom; 
        targetZoom = gameZoom;          
        
        IsGameplayBlocking = true;
        Invoke(nameof(UnlockGameplay), 1f); 
    }

    /// <summary>
    /// Mở khóa tương tác cho người chơi sau khi hiệu ứng Intro kết thúc.
    /// </summary>
    private void UnlockGameplay()
    {
        IsGameplayBlocking = false;
    }

    /// <summary>
    /// Xử lý cập nhật chuyển động và bắt Input ở cuối mỗi khung hình.
    /// </summary>
    void LateUpdate() 
    {
        if (IsGameplayBlocking || isEndGame)
        {
            IsDragging = false;
            if (isEndGame) HandleEndGame();
            if (!isEndGame) ApplyMovementAndZoom(); 
            return;
        }

        HandleInput();
        ApplyMovementAndZoom();
    }

    /// <summary>
    /// Phân luồng xử lý Input giữa chuột và cảm ứng đa điểm.
    /// </summary>
    private void HandleInput()
    {
        if (Input.touchCount > 0) HandleTouchInput();
        else HandleMouseInput();
    }

    /// <summary>
    /// Xử lý thao tác kéo (Pan) bằng chuột phải và thu phóng (Zoom) bằng con lăn.
    /// </summary>
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

    /// <summary>
    /// Xử lý thao tác kéo bằng 1 ngón và thu phóng bằng 2 ngón tay (Pinch to Zoom) trên Mobile.
    /// </summary>
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

    /// <summary>
    /// Tính toán vector dịch chuyển dựa trên vị trí màn hình và tạo quán tính (Inertia).
    /// </summary>
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

    /// <summary>
    /// Áp dụng nội suy mượt mà cho biến đổi Transform và khóa ranh giới Camera.
    /// </summary>
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

    /// <summary>
    /// Xử lý hoạt ảnh tự động đưa Camera về trung tâm bàn cờ khi chiến thắng.
    /// </summary>
    private void HandleEndGame()
    {
        transform.position = Vector3.Lerp(transform.position, initialPosition, Time.deltaTime * 2f);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, gameZoom, Time.deltaTime * 2f);
    }

    /// <summary>
    /// Khóa quyền điều khiển và ra lệnh cho Camera chạy quy trình EndGame.
    /// </summary>
    public void ZoomToEndGame()
    {
        isEndGame = true;
        targetZoom = gameZoom;
        panVelocity = Vector3.zero;
    }
}