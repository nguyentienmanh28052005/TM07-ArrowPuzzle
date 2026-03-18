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
    public float maxZoom = 30f; // Nên để maxZoom khá lớn để làm khoảng lùi cho Intro
    public float zoomSmoothTime = 0.1f;

    [Header("Auto Fit Settings")]
    [Tooltip("Khoảng cách viền lề (Padding) để mũi tên không dính sát mép màn hình")]
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
    
    // Biến gameZoom giờ đây được tính toán tự động ngầm, không cần nhập tay
    private float gameZoom; 
    
    private Vector3 initialPosition;
    private Vector3 panVelocity;
    private Vector3 lastPanScreenPos;
    
    private bool isEndGame = false;
    private bool wasZoomingLastFrame = false;

    private IEnumerator Start()
    {
        cam = GetComponent<Camera>();

        // Đợi 1 frame để đảm bảo LevelLoader đã sinh ra toàn bộ các đốt rắn trên Scene
        yield return new WaitForEndOfFrame(); 

        AutoFitMap();
    }

    private void AutoFitMap()
    {
        // 1. Quét toàn bộ khối rắn trong màn chơi
        SnakeBlock[] allSnakes = FindObjectsOfType<SnakeBlock>();
        
        if (allSnakes.Length == 0) return;

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        bool hasNodes = false;

        // 2. Tìm ra 4 điểm cực đại (Biên giới của bản đồ)
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

        // 3. Tính toán Chiều rộng, Chiều cao và Tâm điểm của bản đồ
        float width = maxX - minX;
        float height = maxY - minY;
        
        // Cập nhật initialPosition (Tâm của camera) nhưng PHẢI GIỮ NGUYÊN trục Z của camera
        initialPosition = new Vector3(minX + width / 2f, minY + height / 2f, transform.position.z);

        // 4. Tính toán Orthographic Size vừa khít với màn hình
        float sizeByHeight = height / 2f;
        float sizeByWidth = (width / 2f) / cam.aspect;
        
        // Lấy giá trị lớn hơn để đảm bảo không bị cắt góc, cộng thêm padding lề
        gameZoom = Mathf.Max(sizeByHeight, sizeByWidth) + autoFitPadding;

        // Đảm bảo maxZoom luôn lớn hơn gameZoom một chút để người chơi còn không gian kéo thả
        maxZoom = Mathf.Max(maxZoom, gameZoom + 5f);

        // 5. Khởi tạo Hiệu ứng Intro
        transform.position = initialPosition;
        cam.orthographicSize = maxZoom; // Bắt đầu từ rất xa
        targetZoom = gameZoom;          // Trượt mượt mà về gameZoom
        
        IsGameplayBlocking = true;
        Invoke(nameof(UnlockGameplay), 1f); 
    }

    private void UnlockGameplay()
    {
        IsGameplayBlocking = false;
    }

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

    private void HandleInput()
    {
        if (Input.touchCount > 0)
            HandleTouchInput();
        else
            HandleMouseInput();
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

            if (useInertia)
                panVelocity = worldDelta / Time.deltaTime;

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
        // Khi thắng game, Camera tự động chạy về Tâm và Zoom vừa khít map như lúc đầu
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