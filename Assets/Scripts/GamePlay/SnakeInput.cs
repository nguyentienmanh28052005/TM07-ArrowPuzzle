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
        parentScript = GetComponentInParent<SnakeBlock>();
    }

    private void Start()
    {
        if (parentScript != null) 
        {
            _guidelineCache = parentScript.GetComponent<ArrowGuideline>();
        }
        
        _levelEditor = FindObjectOfType<LevelEditor>();

        if (GameManager.Instance != null && GameManager.Instance.GetCurrentLevelData() != null)
        {
            _isMemoryMode = GameManager.Instance.GetCurrentLevelData().gameMode == GameMode.Memory;
        }
    }

    private void Update()
    {
        if (_levelEditor != null && _levelEditor.gameObject.activeInHierarchy) return;
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

    private void HandleInputDown()
    {
        if (CameraController.IsGameplayBlocking) return;
        if (parentScript != null && parentScript.IsMoving) return;
        if (EraseManager.Instance != null && EraseManager.Instance.IsExecutingErase) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float myDist = Vector2.Distance(transform.position, mousePos);

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
        
        if (!isCanceledByCamera && !CameraController.IsCameraGestureActive && Vector2.Distance(transform.position, mousePos) <= clickRadius)
        {
            if (parentScript != null)
            {
                if (!isHolding) 
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

    private bool IsClosestToClick(Vector2 mousePos, float myDist)
    {
        foreach (var other in AllInputs)
        {
            if (other != null && other != this && other.gameObject.activeInHierarchy)
            {
                float otherDist = Vector2.Distance(other.transform.position, mousePos);
                
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, clickRadius);
    }
}