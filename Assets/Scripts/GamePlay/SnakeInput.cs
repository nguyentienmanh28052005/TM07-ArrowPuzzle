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

    private bool isPressed = false;
    private bool isHolding = false;
    
    private SnakeBlock parentScript;
    private Coroutine holdCoroutine;
    private ArrowGuideline _guidelineCache;

    // ĐỘT PHÁ: Quản lý toàn bộ Input trong Scene mà không cần Collider
    public static List<SnakeInput> AllInputs = new List<SnakeInput>();

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
        parentScript = GetComponentInParent<SnakeBlock>();
    }

    private void Start()
    {
        if (parentScript != null) 
        {
            _guidelineCache = parentScript.GetComponent<ArrowGuideline>();
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        if (IsPointerOverUI()) 
        { 
            if (isPressed) HandleInputUp(); 
            return; 
        }

        if (Input.GetMouseButtonDown(0)) HandleInputDown();
        if (Input.GetMouseButtonUp(0)) HandleInputUp();
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

    private void HandleInputDown()
    {
        if (CameraController.IsGameplayBlocking) return;
        if (CameraController.IsDragging) return;
        if (parentScript != null && parentScript.IsMoving) return;
        if (EraseManager.Instance != null && EraseManager.Instance.IsExecutingErase) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float myDist = Vector2.Distance(transform.position, mousePos);

        // 1. Kiểm tra xem chuột có nằm trong vùng Click của mình không
        if (myDist > clickRadius) return;
        
        // 2. SO SÁNH TOÁN HỌC: Xác định xem mình có phải là kẻ GẦN CHUỘT NHẤT không?
        if (!IsClosestToClick(mousePos, myDist)) return;

        // Nếu đang bật chế độ Tẩy (Erase)
        if (EraseManager.Instance != null && EraseManager.Instance.IsEraseModeActive)
        {
            CameraController.IsGameplayBlocking = true;
            EraseManager.Instance.ExecuteErase(parentScript);
            return; 
        }

        // Tắt Hint nếu đang bật
        if (HintManager.Instance != null)
        {
            HintManager.Instance.StopHintImmediate();
        }

        isPressed = true;
        isHolding = false; 
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
        isHolding = false;
        CameraController.IsGameplayBlocking = false;

        if (holdCoroutine != null) StopCoroutine(holdCoroutine);

        if (parentScript != null)
        {
            parentScript.SetFocusEffect(false, 1f, duration);
        }

        if (_guidelineCache != null)
        {
            _guidelineCache.SetLineActive(false);
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        if (Vector2.Distance(transform.position, mousePos) <= clickRadius)
        {
            if (parentScript != null)
            {
                bool success = parentScript.OnHeadClicked();
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
        else
        {
            if (parentScript != null) parentScript.SetFocusColor(false, duration);
        }
    }

    private void LateUpdate()
    {
        if (isPressed)
        {
            CameraController.IsGameplayBlocking = true;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            bool isInside = Vector2.Distance(transform.position, mousePos) <= clickRadius;

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

    // ==========================================
    // THUẬT TOÁN TÌM KẺ GẦN NHẤT (O(N) SIÊU NHẸ)
    // ==========================================
    private bool IsClosestToClick(Vector2 mousePos, float myDist)
    {
        foreach (var other in AllInputs)
        {
            if (other != null && other != this && other.gameObject.activeInHierarchy)
            {
                float otherDist = Vector2.Distance(other.transform.position, mousePos);
                
                // Nếu con rắn khác cũng nằm trong tầm click của nó
                if (otherDist <= other.clickRadius)
                {
                    // Nếu nó gần chuột hơn mình -> Mình nhường quyền Click cho nó
                    if (otherDist < myDist) 
                    {
                        return false; 
                    }
                    
                    // Xử lý xung đột: Nếu 2 con rắn nằm đè lên nhau trùng khớp 100% tọa độ
                    // Dùng InstanceID để ưu tiên chọn 1 đứa duy nhất, tránh bấm 1 phát chạy cả 2 con
                    if (Mathf.Abs(otherDist - myDist) < 0.0001f && other.GetInstanceID() < this.GetInstanceID())
                    {
                        return false;
                    }
                }
            }
        }
        
        return true;
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
        if (isPressed) CameraController.IsGameplayBlocking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, clickRadius);
    }
}