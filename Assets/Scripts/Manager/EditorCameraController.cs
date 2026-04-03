using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class EditorCameraController : MonoBehaviour
{
    public static bool IsCameraGestureActive = false; 

    [Header("Zoom Settings (Editor)")]
    public float zoomSpeedPC = 5f;
    public float zoomSpeedMobile = 0.01f;
    public float minZoom = 2f;
    public float maxZoom = 50f; 
    public float zoomSmoothTime = 0.1f;

    [Header("Pan Settings")]
    public bool useLimits = false;
    public Vector2 minPosition = new Vector2(-50, -50);
    public Vector2 maxPosition = new Vector2(50, 50);
    public float dragThreshold = 5f;

    [Header("Inertia Settings")]
    public bool useInertia = true;
    public float dampingFactor = 8f;

    private Camera cam;
    private float targetZoom;
    private float zoomVelocity; 
    
    private Vector3 panVelocity;
    private Vector3 lastPanScreenPos;
    private bool wasZoomingLastFrame = false;

    private void Start()
    {
        cam = GetComponent<Camera>();
        targetZoom = cam.orthographicSize;
    }

    void LateUpdate() 
    {
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
        }
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (wasZoomingLastFrame)
            {
                lastPanScreenPos = touch.position;
                wasZoomingLastFrame = false;
                IsCameraGestureActive = true; 
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
            IsCameraGestureActive = false;
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

        if (IsCameraGestureActive && !wasZoomingLastFrame && Input.touchCount <= 1)
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

        if (useInertia && !IsCameraGestureActive && Input.touchCount == 0 && panVelocity.sqrMagnitude > 0.0001f)
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
}