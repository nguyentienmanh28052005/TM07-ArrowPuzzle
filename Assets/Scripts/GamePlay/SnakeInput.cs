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
    private LevelEditor _levelEditor;
    private bool _isMemoryMode = false;

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
        parentScript = GetComponent<SnakeBlock>();
        if (parentScript == null) parentScript = GetComponentInParent<SnakeBlock>();
    }

    private void Start()
    {
        if (parentScript != null) 
        {
            _guidelineCache = parentScript.GetComponent<ArrowGuideline>();
        }
        
        _levelEditor = FindObjectOfType<LevelEditor>();

        LevelDataSO currentLevelData = PlaytestSession.GetActiveLevelData();
        if (currentLevelData != null)
        {
            _isMemoryMode = currentLevelData.gameMode == GameMode.Memory;
        }
    }

    private void Update()
    {
        if (!PlaytestSession.IsActive && _levelEditor != null && _levelEditor.gameObject.activeInHierarchy) return;
        if (Time.timeScale == 0f) return;

        if (CameraController.IsCameraGestureActive)
        {
            if (isPressed) HandleInputUp(true); 
            return; 
        }

        if (IsPointerOverUI()) 
        { 
            if (isPressed) HandleInputUp(true); 
            return; 
        }

        if (Input.GetMouseButtonDown(0)) HandleInputDown();
        if (Input.GetMouseButtonUp(0)) HandleInputUp(false);
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

    // =========================================================
    // LÕI TOÁN HỌC: ĐO KHOẢNG CÁCH TỪ CHUỘT ĐẾN TOÀN BỘ THÂN RẮN
    // =========================================================
    public float GetMinDistanceFromMouse(Vector2 mousePos)
    {
        if (parentScript == null || parentScript.LogicNodes == null || parentScript.LogicNodes.Count == 0) return float.MaxValue;
        
        float minDist = float.MaxValue;
        List<Vector3> nodes = parentScript.LogicNodes;
        
        if (nodes.Count == 1) return Vector2.Distance(nodes[0], mousePos);

        for (int i = 0; i < nodes.Count - 1; i++)
        {
            float dist = DistancePointToSegment(mousePos, nodes[i], nodes[i + 1]);
            if (dist < minDist) minDist = dist;
        }
        return minDist;
    }

    private float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float sqrMag = ab.sqrMagnitude;
        if (sqrMag == 0) return Vector2.Distance(p, a);

        float t = Vector2.Dot(p - a, ab) / sqrMag;
        t = Mathf.Clamp01(t); // Giới hạn hình chiếu nằm gọn trong đoạn thẳng AB
        Vector2 projection = a + t * ab;
        return Vector2.Distance(p, projection);
    }
    // =========================================================

    private void HandleInputDown()
    {
        if (CameraController.IsGameplayBlocking) return;
        if (BoosterTutorialManager.Instance != null && BoosterTutorialManager.Instance.IsBlockingArrowInput) return;
        if (parentScript != null && parentScript.IsMoving) return;
        if (EraseManager.Instance != null && EraseManager.Instance.IsExecutingErase) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // BẢN VÁ: Đo khoảng cách với toàn thân rắn thay vì chỉ Head
        float myDist = GetMinDistanceFromMouse(mousePos);

        if (myDist > clickRadius) return;
        if (!IsClosestToClick(mousePos, myDist)) return;

        if (EraseManager.Instance != null && EraseManager.Instance.IsEraseModeActive)
        {
            CameraController.IsGameplayBlocking = true;
            EraseManager.Instance.ExecuteErase(parentScript);
            return; 
        }

        if (HintManager.Instance != null)
        {
            HintManager.Instance.StopHintImmediate();
        }

        isPressed = true;
        isHolding = false; 

        if (parentScript != null)
        {
            parentScript.SetFocusColor(true, duration);
        }

        if (_isMemoryMode)
        {
            CameraController.IsGameplayBlocking = true;
            if (holdCoroutine != null) StopCoroutine(holdCoroutine);
            holdCoroutine = StartCoroutine(WaitAndScale());
        }
    }

    private void HandleInputUp(bool isCanceledByCamera)
    {
        if (!isPressed) return;

        isPressed = false;
        
        if (_isMemoryMode)
        {
            CameraController.IsGameplayBlocking = false;
        }

        if (holdCoroutine != null) StopCoroutine(holdCoroutine);

        if (parentScript != null)
        {
            parentScript.SetFocusEffect(false, 1f, duration);
            parentScript.SetFocusColor(false, duration); 
        }

        if (_guidelineCache != null)
        {
            _guidelineCache.SetLineActive(false);
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // BẢN VÁ: Đo lại khoảng cách lúc nhả chuột với toàn thân
        if (!isCanceledByCamera && !CameraController.IsCameraGestureActive && GetMinDistanceFromMouse(mousePos) <= clickRadius)
        {
            if (parentScript != null)
            {
                if (!isHolding) 
                {
                    bool success = parentScript.OnHeadClicked();

                    // Tutorial: hide hand/instructions after the first arrow press.
                    if (TutorialManager.Instance != null)
                    {
                        TutorialManager.Instance.NotifyFirstArrowPressed();
                    }

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
        }
        
        isHolding = false;
    }

    private void LateUpdate()
    {
        if (isPressed)
        {
            if (_isMemoryMode)
            {
                CameraController.IsGameplayBlocking = true;
            }

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            bool isInside = GetMinDistanceFromMouse(mousePos) <= clickRadius;

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

    private bool IsClosestToClick(Vector2 mousePos, float myDist)
    {
        foreach (var other in AllInputs)
        {
            if (other != null && other != this && other.gameObject.activeInHierarchy)
            {
                // BẢN VÁ: Hỏi khoảng cách đến thân của các con rắn khác để tranh quyền click
                float otherDist = other.GetMinDistanceFromMouse(mousePos);
                
                if (otherDist <= other.clickRadius)
                {
                    if (otherDist < myDist) return false; 
                    
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
        if (isPressed && _isMemoryMode) CameraController.IsGameplayBlocking = false;
    }
}
