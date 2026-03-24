using UnityEngine;
using UnityEngine.EventSystems; 
using DG.Tweening;
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

    private void Awake()
    {
        parentScript = GetComponentInParent<SnakeBlock>();

        if (FindObjectOfType<LevelEditor>() != null)
        {
            this.enabled = false;
            return;
        }
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
        if (CameraController.IsDragging) return;
        if (parentScript != null && parentScript.IsMoving) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float dist = Vector2.Distance(transform.position, mousePos);

        if (dist > clickRadius) return;
        if (!IsClosestToClick(mousePos)) return;

        if (EraseManager.Instance != null && EraseManager.Instance.IsEraseModeActive)
        {
            CameraController.IsGameplayBlocking = true;
            EraseManager.Instance.ExecuteErase(parentScript);
            return; 
        }

        // =========================================================
        // DẬP TẮT HINT NGAY KHI NGƯỜI CHƠI CHẠM VÀO MỘT CON RẮN BẤT KỲ
        // =========================================================
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

        bool willMove = false;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Vector2.Distance(transform.position, mousePos) <= clickRadius)
        {
            willMove = true;
            if (parentScript != null)
            {
                AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowTap, 0.8f);
                parentScript.OnHeadClicked();
                
                if (useHaptics) 
                {
                    MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.Selection);
                }
            }
        }

        if (parentScript != null && !willMove)
        {
            parentScript.SetFocusColor(false, duration);
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
                if (parentScript != null)
                {
                    parentScript.SetFocusColor(false, duration);
                }
            }
            else
            {
                if (isHolding) 
                {
                    if (_guidelineCache != null) _guidelineCache.SetLineActive(true);
                }
                
                if (parentScript != null)
                {
                    parentScript.SetFocusColor(true, duration);
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