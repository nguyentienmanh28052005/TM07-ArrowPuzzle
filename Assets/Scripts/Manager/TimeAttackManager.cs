using UnityEngine;
using TMPro;
using DG.Tweening;

public class TimeAttackManager : MonoBehaviour
{
    public static TimeAttackManager Instance;

    public TextMeshProUGUI timerText; 
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    public Color bonusColor = Color.green;

    public float bonusTimePerCombo = 3f;

    private float _currentTime;
    private bool _isRunning = false;
    private bool _isTimeAttackMode = false;
    private int _lastDisplayedSecond = -1; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        CameraController.OnIntroFinished += StartTimer;
    }

    private void OnDisable()
    {
        CameraController.OnIntroFinished -= StartTimer;
    }

    public void InitializeTimer(float timeLimit)
    {
        _isTimeAttackMode = true;
        _currentTime = timeLimit;
        _isRunning = false;
        _lastDisplayedSecond = -1; 
        
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            UpdateTimerUI();
        }
    }

    public void DisableTimer()
    {
        _isTimeAttackMode = false;
        _isRunning = false;
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
    }

    private void StartTimer()
    {
        if (_isTimeAttackMode && _currentTime > 0)
        {
            _isRunning = true;
        }
    }

    private void Update()
    {
        if (!_isRunning || Time.timeScale == 0f) return;

        _currentTime -= Time.deltaTime;

        if (_currentTime <= 0f)
        {
            _currentTime = 0f;
            UpdateTimerUI();
            TriggerTimeOutLose();
        }
        else
        {
            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int totalSeconds = Mathf.CeilToInt(_currentTime);

        if (totalSeconds != _lastDisplayedSecond)
        {
            _lastDisplayedSecond = totalSeconds;
            
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        if (_currentTime <= 10f && timerText.color != warningColor)
        {
            timerText.color = warningColor;
            timerText.transform.DOKill();
            timerText.transform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
        }
    }

    public void AddBonusTime()
    {
        if (!_isTimeAttackMode || _currentTime <= 0) return;

        _currentTime += bonusTimePerCombo;
        _lastDisplayedSecond = -1; 
        UpdateTimerUI();

        if (timerText != null)
        {
            timerText.transform.DOKill(true);
            timerText.transform.localScale = Vector3.one;
            timerText.transform.DOPunchScale(Vector3.one * 0.4f, 0.3f, 10, 1);
            timerText.DOColor(bonusColor, 0.2f).OnComplete(() => {
                timerText.color = _currentTime <= 10f ? warningColor : normalColor;
                
                if (_currentTime > 10f) 
                {
                    timerText.transform.DOKill();
                    timerText.transform.localScale = Vector3.one;
                }
            });
        }
    }

    private void TriggerTimeOutLose()
    {
        _isRunning = false;
        CameraController.IsGameplayBlocking = true; 
        
        GameCanvas canvas = FindObjectOfType<GameCanvas>();
        if (canvas != null)
        {
            canvas.ShowLosePopup(null); 
        }
    }
}