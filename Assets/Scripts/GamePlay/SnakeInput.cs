using UnityEngine;
using UnityEngine.EventSystems; // BẮT BUỘC: Thư viện xử lý sự kiện UI
using DG.Tweening;
using Solo.MOST_IN_ONE;

public class SnakeInput : MonoBehaviour
{
    [Header("Effect Settings")]
    public float scaleFactor = 1.3f;
    public float duration = 0.2f;
    public float holdThreshold = 0.15f;

    [Header("Input Settings")]
    public float clickRadius = 0.8f;
    public bool useHaptics = true;

    private bool isPressed = false;
    private SnakeBlock parentScript;
    private Coroutine holdCoroutine;

    private void Awake()
    {
        parentScript = GetComponentInParent<SnakeBlock>();

        if (FindObjectOfType<LevelEditor>() != null)
        {
            this.enabled = false;
            return;
        }
    }

    private void Update()
    {
        // ==========================================
        // LÁ CHẮN UI CHỐNG CLICK XUYÊN TÁO
        // ==========================================
        
        // 1. Nếu game đang tạm dừng (TimeScale = 0), ngắt hoàn toàn thao tác
        if (Time.timeScale == 0f) return;

        // 2. Nếu người chơi đang chạm vào bất kỳ UI nào (Nút Pause, Màn đen Overlay)
        if (IsPointerOverUI())
        {
            // Bắt buộc phải nhả trạng thái Pressed ra nếu đang đè chuột rồi trượt vào UI
            if (isPressed) HandleInputUp(); 
            return; // Thoát ngay lập tức, bỏ qua mọi lệnh click bên dưới
        }

        // ==========================================
        
        if (Input.GetMouseButtonDown(0)) HandleInputDown();
        if (Input.GetMouseButtonUp(0)) HandleInputUp();
    }

    // Hàm phụ trợ bọc thép kiểm tra UI trên cả Máy tính lẫn Điện thoại
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // Xử lý riêng cho cảm ứng trên điện thoại Mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
            }
        }
        
        // Xử lý cho Chuột máy tính (hoặc Simulator)
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void HandleInputDown()
    {
        if (CameraController.IsDragging) return;
        
        // Chặn click nếu mũi tên đang di chuyển hoặc đang lùi
        if (parentScript != null && parentScript.IsMoving) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float dist = Vector2.Distance(transform.position, mousePos);

        if (dist > clickRadius) return;
        if (!IsClosestToClick(mousePos)) return;

        isPressed = true;
        CameraController.IsGameplayBlocking = true;

        if (parentScript != null)
        {
            parentScript.SetFocusColor(true, duration);
        }

        if (holdCoroutine != null) StopCoroutine(holdCoroutine);
        holdCoroutine = StartCoroutine(WaitAndScale());
    }

    private void HandleInputUp()
    {
        if (!isPressed) return;

        isPressed = false;
        CameraController.IsGameplayBlocking = false;

        if (holdCoroutine != null) StopCoroutine(holdCoroutine);

        if (parentScript != null)
        {
            parentScript.SetFocusEffect(false, 1f, duration);
        }

        bool willMove = false;

        if (!CameraController.IsDragging)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(transform.position, mousePos) <= clickRadius)
            {
                willMove = true;
                if (parentScript != null)
                {
                    AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowTap, 0.8f);
                    parentScript.OnHeadClicked();
                    
                    if (useHaptics) // Cập nhật nhỏ: Bọc điều kiện useHaptics
                    {
                        MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.Selection);
                    }
                }
            }
        }

        if (parentScript != null)
        {
            if (!willMove)
            {
                parentScript.SetFocusColor(false, duration);
            }
        }
    }

    private void LateUpdate()
    {
        if (isPressed)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(transform.position, mousePos) > clickRadius)
            {
                isPressed = false;
                CameraController.IsGameplayBlocking = false;

                if (holdCoroutine != null) StopCoroutine(holdCoroutine);

                if (parentScript != null)
                {
                    parentScript.SetFocusEffect(false, 1f, duration);
                    parentScript.SetFocusColor(false, duration);
                }
            }
        }
    }

    private bool IsClosestToClick(Vector2 clickPos)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(clickPos, clickRadius);
        float myDistance = Vector2.Distance(transform.position, clickPos);
        foreach (var hit in hits)
        {
            SnakeInput other = hit.GetComponent<SnakeInput>();
            if (other != null && other != this && Vector2.Distance(other.transform.position, clickPos) < myDistance) return false;
        }
        return true;
    }

    private System.Collections.IEnumerator WaitAndScale()
    {
        yield return new WaitForSeconds(holdThreshold);
        if (isPressed && parentScript != null)
        {
            parentScript.SetFocusEffect(true, scaleFactor, duration);
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();
        if (isPressed) CameraController.IsGameplayBlocking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, clickRadius);
    }
}