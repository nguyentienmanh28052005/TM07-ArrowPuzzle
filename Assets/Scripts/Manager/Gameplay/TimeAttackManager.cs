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

    [Header("Bonus Time Feedback")]
    [SerializeField] private TextMeshProUGUI bonusTimeFeedbackText;
    [SerializeField] private RectTransform bonusTimeFeedbackRect;
    [SerializeField] private string bonusTimeFeedbackFormat = "+{0:0}s";
    [SerializeField] private float bonusTimeFeedbackPopScale = 1.25f;
    [SerializeField] private float bonusTimeFeedbackFloatDistance = 80f;

    [Header("Last 10 Seconds Flash")]
    [SerializeField] private bool playWarningFlash = true;
    [SerializeField] private int warningFlashStartSecond = 10;
    [SerializeField] private float warningFlashFadeDuration = 0.16f;
    [SerializeField] private Color warningFlashColor = new Color(1f, 0f, 0f, 0.16f);

    [Header("Last 10 Seconds Audio")]
    [SerializeField] private AudioClip lastSecondsWarningSound;
    [SerializeField] private AudioClip finalSecondWarningSound;
    [SerializeField, Range(0f, 1f)] private float lastSecondsWarningSoundVolume = 1f;
    [SerializeField, Range(0.1f, 3f)] private float lastSecondsWarningSoundPitch = 1f;

    private float _currentTime;
    private bool _isRunning = false;
    private bool _isTimeAttackMode = false;
    private int _lastDisplayedSecond = -1;
    private int _lastWarningFlashSecond = -1;
    private int _lastWarningSoundSecond = -1;
    private Vector2 _bonusTimeFeedbackStartPosition;
    private Sequence _bonusTimeFeedbackSequence;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InitializeBonusTimeFeedback();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
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
        if (PlaytestSession.IsActive)
        {
            ResetTimer();
            return;
        }

        _isTimeAttackMode = true;
        _currentTime = Mathf.Max(0f, timeLimit);
        _isRunning = false;
        _lastDisplayedSecond = -1;
        _lastWarningFlashSecond = -1;
        _lastWarningSoundSecond = -1;
        
        if (timerText != null)
        {
            ResetTimerVisual(false);
            timerText.gameObject.SetActive(true);
            UpdateTimerUI(false);
        }
    }

    public void DisableTimer()
    {
        ResetTimer();
    }

    public void ResetTimer()
    {
        _isTimeAttackMode = false;
        _isRunning = false;
        _currentTime = 0f;
        _lastDisplayedSecond = -1;
        _lastWarningFlashSecond = -1;
        _lastWarningSoundSecond = -1;
        ResetTimerVisual(true);
    }

    public void StopTimer()
    {
        _isRunning = false;
    }

    public void ResumeTimer()
    {
        if (_isTimeAttackMode && _currentTime > 0f)
        {
            _isRunning = true;
        }
    }

    private void ResetTimerVisual(bool hideTimer)
    {
        if (timerText != null)
        {
            timerText.DOKill(false);
            timerText.transform.DOKill(false);
            timerText.transform.localScale = Vector3.one;
            timerText.color = normalColor;
            timerText.text = "00:00";
            timerText.gameObject.SetActive(!hideTimer);
        }
    }

    private void StartTimer()
    {
        if (_isTimeAttackMode && _currentTime > 0)
        {
            _isRunning = true;
            PlayLastSecondsWarningFlash(Mathf.CeilToInt(_currentTime));
            PlayLastSecondsWarningSoundForSecond(Mathf.CeilToInt(_currentTime));
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

    private void UpdateTimerUI(bool allowWarningFlash = true)
    {
        if (timerText == null) return;

        int totalSeconds = Mathf.CeilToInt(_currentTime);

        if (totalSeconds != _lastDisplayedSecond)
        {
            _lastDisplayedSecond = totalSeconds;
            
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            if (allowWarningFlash && _isRunning)
            {
                PlayLastSecondsWarningFlash(totalSeconds);
                PlayLastSecondsWarningSoundForSecond(totalSeconds);
            }
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
        ResetWarningFlashIfOutsideWarningWindow();
        UpdateTimerUI(false);

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

        PlayBonusTimeFeedback(bonusTimePerCombo);
    }

    private void TriggerTimeOutLose()
    {
        if (PlaytestSession.IsActive)
        {
            _isRunning = false;
            return;
        }

        _isRunning = false;
        CameraController.IsGameplayBlocking = true; 
        
        GameCanvas canvas = FindObjectOfType<GameCanvas>();
        if (canvas != null)
        {
            canvas.ShowLosePopup(GameCanvas.LoseReason.TimeOut); 
        }
    }

    public void AddTime(float amount)
    {
        if (!_isTimeAttackMode) return;

        _currentTime += amount;
        
        _isRunning = true; 
        _lastDisplayedSecond = -1;
        ResetWarningFlashIfOutsideWarningWindow();
        UpdateTimerUI(false);

        if (timerText != null)
        {
            timerText.transform.DOKill(true);
            timerText.transform.localScale = Vector3.one;
            timerText.transform.DOPunchScale(Vector3.one * 0.5f, 0.5f, 10, 1).SetUpdate(true);
            timerText.DOColor(bonusColor, 0.3f).SetUpdate(true).OnComplete(() => {
                
                if (_currentTime > 10f) 
                {
                    timerText.color = normalColor;
                    timerText.transform.DOKill();
                    timerText.transform.localScale = Vector3.one;
                }
                else 
                {
                    timerText.color = warningColor;
                    timerText.transform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
                }
            });
        }

        PlayBonusTimeFeedback(amount);
    }

    private void InitializeBonusTimeFeedback()
    {
        if (bonusTimeFeedbackText == null) return;

        if (bonusTimeFeedbackRect == null)
        {
            bonusTimeFeedbackRect = bonusTimeFeedbackText.rectTransform;
        }

        if (bonusTimeFeedbackRect != null)
        {
            _bonusTimeFeedbackStartPosition = bonusTimeFeedbackRect.anchoredPosition;
            bonusTimeFeedbackRect.localScale = Vector3.zero;
        }

        bonusTimeFeedbackText.raycastTarget = false;
        bonusTimeFeedbackText.alpha = 0f;
        bonusTimeFeedbackText.gameObject.SetActive(false);
    }

    private void PlayBonusTimeFeedback(float amount)
    {
        if (bonusTimeFeedbackText == null) return;
        if (bonusTimeFeedbackRect == null) bonusTimeFeedbackRect = bonusTimeFeedbackText.rectTransform;
        if (bonusTimeFeedbackRect == null) return;

        _bonusTimeFeedbackSequence?.Kill();
        bonusTimeFeedbackText.DOKill();
        bonusTimeFeedbackRect.DOKill();

        bonusTimeFeedbackText.gameObject.SetActive(true);
        bonusTimeFeedbackText.text = string.Format(bonusTimeFeedbackFormat, amount);
        bonusTimeFeedbackText.color = bonusColor;
        bonusTimeFeedbackText.alpha = 1f;

        bonusTimeFeedbackRect.anchoredPosition = _bonusTimeFeedbackStartPosition;
        bonusTimeFeedbackRect.localScale = Vector3.zero;
        bonusTimeFeedbackRect.localRotation = Quaternion.identity;

        _bonusTimeFeedbackSequence = DOTween.Sequence().SetUpdate(true);
        _bonusTimeFeedbackSequence.Append(bonusTimeFeedbackRect.DOScale(Vector3.one * bonusTimeFeedbackPopScale, 0.22f).SetEase(Ease.OutBack, 3f));
        _bonusTimeFeedbackSequence.AppendInterval(0.15f);
        _bonusTimeFeedbackSequence.Append(bonusTimeFeedbackRect.DOAnchorPosY(_bonusTimeFeedbackStartPosition.y + bonusTimeFeedbackFloatDistance, 0.45f).SetEase(Ease.InSine));
        _bonusTimeFeedbackSequence.Join(bonusTimeFeedbackText.DOFade(0f, 0.35f));
        _bonusTimeFeedbackSequence.Join(bonusTimeFeedbackRect.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InBack));
        _bonusTimeFeedbackSequence.OnComplete(() => bonusTimeFeedbackText.gameObject.SetActive(false));
    }

    private void PlayLastSecondsWarningFlash(int totalSeconds)
    {
        if (!playWarningFlash) return;

        int startSecond = Mathf.Max(1, warningFlashStartSecond);
        if (totalSeconds <= 0 || totalSeconds > startSecond)
        {
            _lastWarningFlashSecond = -1;
            return;
        }

        if (totalSeconds == _lastWarningFlashSecond) return;
        _lastWarningFlashSecond = totalSeconds;

        ScreenJuiceManager juiceManager = ScreenJuiceManager.Instance;
        if (juiceManager == null) juiceManager = FindObjectOfType<ScreenJuiceManager>();

        if (juiceManager != null)
        {
            juiceManager.PlayFlashOverlay(warningFlashColor, warningFlashFadeDuration);
        }
    }

    private void PlayLastSecondsWarningSoundForSecond(int totalSeconds)
    {
        int startSecond = Mathf.Max(1, warningFlashStartSecond);
        if (totalSeconds <= 0 || totalSeconds > startSecond)
        {
            _lastWarningSoundSecond = -1;
            return;
        }

        if (totalSeconds == _lastWarningSoundSecond) return;
        _lastWarningSoundSecond = totalSeconds;

        AudioClip clip = totalSeconds == 1 ? finalSecondWarningSound : lastSecondsWarningSound;
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySfx(clip, lastSecondsWarningSoundVolume, lastSecondsWarningSoundPitch);
    }

    private void ResetWarningFlashIfOutsideWarningWindow()
    {
        if (_currentTime > Mathf.Max(1, warningFlashStartSecond))
        {
            _lastWarningFlashSecond = -1;
            _lastWarningSoundSecond = -1;
        }
    }
}
