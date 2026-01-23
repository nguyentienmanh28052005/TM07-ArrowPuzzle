using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static bool IsGameplayBlocking = false;
    public static bool IsDragging = false;

    [Header("Zoom Settings")]
    public float zoomSpeedPC = 5f;
    public float zoomSpeedMobile = 0.5f;
    public float minZoom = 5f;
    public float maxZoom = 20f;
    public float smoothSpeed = 10f;

    private float targetZoom;
    private Camera cam;

    [Header("Pan Settings")]
    public bool useLimits = true;
    public Vector2 minPosition;
    public Vector2 maxPosition;

    private float dragThreshold = 10f;
    private Vector3 dragOriginScreen;
    private Vector3 dragOriginWorld;
    private bool isEndGame = false;
    private Vector3 initialPosition;

    void Start()
    {
        cam = GetComponent<Camera>();
        targetZoom = cam.orthographicSize;
        initialPosition = transform.position;
    }

    void Update()
    {
        if (IsGameplayBlocking || isEndGame)
        {
            IsDragging = false;
            if (!isEndGame) return;
        }

        if (!isEndGame)
        {
            if (Input.touchCount > 0)
                HandleTouchInput();
            else
                HandleMouseInput();
        }

        float currentSmooth = isEndGame ? 2f : smoothSpeed;

        if (Mathf.Abs(cam.orthographicSize - targetZoom) > 0.01f)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * currentSmooth);
        }

        if (isEndGame)
        {
            if (Vector3.Distance(transform.position, initialPosition) > 0.01f)
            {
                transform.position = Vector3.Lerp(transform.position, initialPosition, Time.deltaTime * currentSmooth);
            }
        }
        else if (useLimits)
        {
            Vector3 clampedPos = transform.position;
            clampedPos.x = Mathf.Clamp(clampedPos.x, minPosition.x, maxPosition.x);
            clampedPos.y = Mathf.Clamp(clampedPos.y, minPosition.y, maxPosition.y);
            transform.position = clampedPos;
        }
    }

    void HandleMouseInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            targetZoom -= scroll * zoomSpeedPC;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        if (Input.GetMouseButtonDown(1))
        {
            dragOriginScreen = Input.mousePosition;
            dragOriginWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            IsDragging = false;
        }

        if (Input.GetMouseButton(1))
        {
            float dist = Vector3.Distance(Input.mousePosition, dragOriginScreen);
            if (dist > dragThreshold) IsDragging = true;

            if (IsDragging)
            {
                Vector3 currentPos = cam.ScreenToWorldPoint(Input.mousePosition);
                Vector3 difference = dragOriginWorld - currentPos;
                transform.position += difference;
            }
        }

        if (Input.GetMouseButtonUp(1)) IsDragging = false;
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 2)
        {
            IsDragging = true;
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            targetZoom -= difference * zoomSpeedMobile * Time.deltaTime;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                dragOriginScreen = touch.position;
                dragOriginWorld = cam.ScreenToWorldPoint(touch.position);
                IsDragging = false;
            }

            if (touch.phase == TouchPhase.Moved)
            {
                float dist = Vector3.Distance(touch.position, dragOriginScreen);
                if (dist > dragThreshold) IsDragging = true;

                if (IsDragging)
                {
                    Vector3 currentPos = cam.ScreenToWorldPoint(touch.position);
                    Vector3 difference = dragOriginWorld - currentPos;
                    transform.position += difference;
                }
            }

            if (touch.phase == TouchPhase.Ended) IsDragging = false;
        }
    }

    public void ZoomToMax()
    {
        isEndGame = true;
        targetZoom = maxZoom;
    }
}