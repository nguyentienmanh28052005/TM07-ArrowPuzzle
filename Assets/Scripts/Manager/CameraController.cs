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
    public float maxZoom = 20f;
    public float zoomSmoothTime = 0.1f;

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
    private float zoomVelocity; // Dùng cho SmoothDamp
    
    private Vector3 initialPosition;
    private Vector3 panVelocity;
    private Vector3 lastPanScreenPos;
    
    private bool isEndGame = false;
    private bool wasZoomingLastFrame = false;

    void Start()
    {
        cam = GetComponent<Camera>();
        targetZoom = cam.orthographicSize;
        initialPosition = transform.position;
    }

    void LateUpdate() // Dùng LateUpdate để camera mượt hơn sau khi các logic khác đã chạy
    {
        if (IsGameplayBlocking || isEndGame)
        {
            IsDragging = false;
            if (isEndGame) HandleEndGame();
            return;
        }

        HandleInput();
        ApplyMovementAndZoom();
    }

    private void HandleInput()
    {
        // Ưu tiên xử lý Touch trên Mobile
        if (Input.touchCount > 0)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }
    }

    private void HandleMouseInput()
    {
        // 1. Zoom PC
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            targetZoom -= scroll * zoomSpeedPC;
        }

        // 2. Pan PC (Chuột phải hoặc Chuột giữa tùy bạn, ở đây dùng chuột phải - 1)
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
        // Chống xuyên qua UI
        if (Input.touchCount > 0 && EventSystem.current != null)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;
        }

        if (Input.touchCount >= 2)
        {
            // --- LOGIC ZOOM ---
            wasZoomingLastFrame = true;
            IsDragging = true;

            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 t0Prev = t0.position - t0.deltaPosition;
            Vector2 t1Prev = t1.position - t1.deltaPosition;

            float prevMag = (t0Prev - t1Prev).magnitude;
            float curMag = (t0.position - t1.position).magnitude;

            targetZoom -= (curMag - prevMag) * zoomSpeedMobile;
            
            // Cập nhật điểm neo liên tục để khi thả 1 ngón không bị giật
            lastPanScreenPos = (t0.position + t1.position) * 0.5f;
        }
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            // FIX GIẬT: Nếu vừa thả ngón thứ 2 ra, reset điểm neo và bỏ qua frame đó
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
            // Dùng ScreenToWorldPoint để tốc độ di chuyển tỉ lệ thuận với mức Zoom
            Vector3 worldDelta = cam.ScreenToWorldPoint(lastPanScreenPos) - cam.ScreenToWorldPoint(currentScreenPos);
            transform.position += worldDelta;

            if (useInertia)
                panVelocity = worldDelta / Time.deltaTime;

            lastPanScreenPos = currentScreenPos;
        }
    }

    private void ApplyMovementAndZoom()
    {
        // 1. Thực hiện Zoom mượt với SmoothDamp (tốt hơn Lerp cho camera)
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothTime);

        // 2. Thực hiện Quán tính (Inertia)
        if (useInertia && !IsDragging && panVelocity.sqrMagnitude > 0.0001f)
        {
            transform.position += panVelocity * Time.deltaTime;
            panVelocity = Vector3.Lerp(panVelocity, Vector3.zero, dampingFactor * Time.deltaTime);
        }

        // 3. Giới hạn vùng di chuyển
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
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, maxZoom, Time.deltaTime * 2f);
    }

    public void ZoomToMax()
    {
        isEndGame = true;
        targetZoom = maxZoom;
        panVelocity = Vector3.zero;
    }
}