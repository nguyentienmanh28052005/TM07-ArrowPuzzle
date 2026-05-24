using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using Pixelplacement;

public class ComboManager : Singleton<ComboManager>
{
    [System.Serializable]
    public struct ComboSettings
    {
        public int minComboThreshold;
        public Material comboMaterial;
        public float fontSizeMultiplier;
    }

    [System.Serializable]
    public struct FeedbackSetting
    {
        public int comboThreshold;
        public List<string> words;
        public AudioClip feedbackSound;
        public Material feedbackMaterial;
        public Color textColor;
        public float sizeMultiplier;
    }

    [Header("Combo State")]
    public int currentCombo = 0;
    [SerializeField] private float comboTimeout = 2.5f;
    [SerializeField] private int minComboToShow = 2;
    private float _lastHitTime;
    private bool _isFullComboActive = false;

    [Header("Visual References")]
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private RectTransform comboTextRect;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private RectTransform feedbackTextRect;
    [SerializeField] private ParticleSystem comboParticle;

    [Header("Juice Settings")]
    [SerializeField] private float maxRotationTilt = 15f;
    [SerializeField] private float maxAllowedSizeMultiplier = 2.8f;
    [SerializeField] private List<ComboSettings> colorTierSettings;
    [SerializeField] private List<FeedbackSetting> feedbackTierSettings;

    [Header("Rainbow (Full Combo)")]
    [SerializeField] private float rainbowSpeed = 3f;
    [SerializeField] private AudioClip fullComboSound;

    [Header("Dot Feedback Effect (Inward Wave)")]
    [SerializeField] private bool enableDotFlashOnFeedback = true;
    [SerializeField] private float dotFlashScaleAmount = 1.35f;
    [SerializeField] private float dotFlashDuration = 0.22f;
    [SerializeField] private int waveBatchSize = 15;
    [SerializeField] private float waveDelay = 0.05f;

    private Vector2 _originalTextPos;
    private Vector2 _originalFeedbackPos;
    private Sequence _activeSequence;
    private Sequence _feedbackSequence;
    private Material _defaultMaterial;
    private SnakeBlock _lastComboSource;
    private int _maxComboForCurrentLevel = 999;
    private bool _hasShownFullCombo = false;
    private bool _hasInitializedVisuals = false;

    private void Start()
    {
        if (comboText != null)
        {
            comboText.raycastTarget = false; 
            comboText.alpha = 0f;
            comboText.gameObject.SetActive(false);
            comboTextRect.localScale = Vector3.zero;
            _originalTextPos = comboTextRect.anchoredPosition;
            _defaultMaterial = comboText.fontSharedMaterial;
        }

        if (feedbackText != null)
        {
            feedbackText.raycastTarget = false;
            feedbackText.alpha = 0f;
            feedbackText.gameObject.SetActive(false);
            feedbackTextRect.localScale = Vector3.zero;
            _originalFeedbackPos = feedbackTextRect.anchoredPosition;
        }

        _hasInitializedVisuals = true;
    }

    private void Update()
    {
        if (_isFullComboActive && comboText.gameObject.activeSelf)
        {
            float hue = Mathf.Repeat(Time.time * rainbowSpeed, 1f);
            comboText.color = Color.HSVToRGB(hue, 0.6f, 1f);
        }
    }

    public void AddCombo(SnakeBlock comboSource = null)
    {
        if (currentCombo > 0 && Time.time - _lastHitTime > comboTimeout)
        {
            StopCombo();
        }

        _lastComboSource = comboSource;
        currentCombo++;
        _lastHitTime = Time.time;
        // if(currentCombo % 5 == 0 && currentCombo != 0)
        // {
        //     if (ScreenJuiceManager.Instance != null)
        //     {
        //         ScreenJuiceManager.Instance.PlayComboJuice(currentCombo);
        //     }
        // }
        if (currentCombo >= minComboToShow)
        {
            PlayBlockBlastFeedback();
        }
    }

    public void StopCombo()
    {
        if (currentCombo == 0) return;

        currentCombo = 0;
        _isFullComboActive = false;
        _hasShownFullCombo = false;
        _lastComboSource = null;

        if (comboText != null && comboText.gameObject.activeSelf)
        {
            _activeSequence?.Kill();
            comboText.DOKill();
            comboTextRect.DOKill();
            
            _activeSequence = DOTween.Sequence();
            _activeSequence.Append(comboText.DOFade(0f, 0.15f));
            _activeSequence.Join(comboTextRect.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack));
            _activeSequence.OnComplete(() => comboText.gameObject.SetActive(false));
        }

        if (feedbackText != null && feedbackText.gameObject.activeSelf)
        {
            _feedbackSequence?.Kill();
            feedbackText.DOKill();
            feedbackTextRect.DOKill();
            
            _feedbackSequence = DOTween.Sequence();
            _feedbackSequence.Append(feedbackText.DOFade(0f, 0.1f));
            _feedbackSequence.Join(feedbackTextRect.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InBack));
            _feedbackSequence.OnComplete(() => feedbackText.gameObject.SetActive(false));
        }
    }

    private int GetMaxComboForCurrentLevel()
    {
        if (_maxComboForCurrentLevel > 0)
        {
            return _maxComboForCurrentLevel;
        }

        if (GameManager.Instance != null && GameManager.Instance.GetCurrentLevelData() != null)
        {
            return GameManager.Instance.GetCurrentLevelData().snakes.Count;
        }
        return 999;
    }

    public void ResetForLevel(LevelDataSO levelData)
    {
        _maxComboForCurrentLevel = levelData != null && levelData.snakes != null ? levelData.snakes.Count : 999;
        ResetComboImmediate();
    }

    public void ResetComboImmediate()
    {
        currentCombo = 0;
        _lastHitTime = 0f;
        _isFullComboActive = false;
        _hasShownFullCombo = false;
        _lastComboSource = null;

        _activeSequence?.Kill();
        _feedbackSequence?.Kill();

        if (comboText != null)
        {
            comboText.DOKill();
            comboText.alpha = 0f;
            comboText.color = Color.white;
            comboText.fontSharedMaterial = _defaultMaterial != null ? _defaultMaterial : comboText.fontSharedMaterial;
            comboText.gameObject.SetActive(false);
        }

        if (comboTextRect != null)
        {
            comboTextRect.DOKill();
            if (_hasInitializedVisuals) comboTextRect.anchoredPosition = _originalTextPos;
            comboTextRect.localScale = Vector3.zero;
            comboTextRect.localRotation = Quaternion.identity;
        }

        if (feedbackText != null)
        {
            feedbackText.DOKill();
            feedbackText.alpha = 0f;
            feedbackText.gameObject.SetActive(false);
        }

        if (feedbackTextRect != null)
        {
            feedbackTextRect.DOKill();
            if (_hasInitializedVisuals) feedbackTextRect.anchoredPosition = _originalFeedbackPos;
            feedbackTextRect.localScale = Vector3.zero;
            feedbackTextRect.localRotation = Quaternion.identity;
        }
    }

    private void PlayBlockBlastFeedback()
    {
        if (comboText == null || comboTextRect == null) return;

        int maxCombo = GetMaxComboForCurrentLevel();
        _isFullComboActive = maxCombo > 0 && currentCombo == maxCombo;

        float sizeMult = 1.2f;
        Material targetMat = null;

        if (_isFullComboActive)
        {
            comboText.text = "FULL COMBO!";
            sizeMult = maxAllowedSizeMultiplier;
            if (!_hasShownFullCombo)
            {
                _hasShownFullCombo = true;
                PlayFeedbackSound(fullComboSound);
            }
        }
        else
        {
            comboText.text = $"Combo x{currentCombo}";
            if (colorTierSettings != null)
            {
                foreach (var setting in colorTierSettings)
                {
                    if (currentCombo >= setting.minComboThreshold)
                    {
                        targetMat = setting.comboMaterial;
                        sizeMult = setting.fontSizeMultiplier;
                    }
                }
            }
            sizeMult += (currentCombo * 0.08f); 
        }

        comboText.fontSharedMaterial = targetMat != null ? targetMat : _defaultMaterial;
        sizeMult = Mathf.Min(sizeMult, maxAllowedSizeMultiplier);

        _activeSequence?.Kill();
        comboText.DOKill();
        comboTextRect.DOKill();

        comboText.gameObject.SetActive(true);
        comboText.DOFade(1f, 0f); 
        comboText.color = Color.white; 
        comboTextRect.anchoredPosition = _originalTextPos;
        comboTextRect.localScale = Vector3.one * 0.4f;

        CheckAndShowFeedback();

        if (comboParticle != null)
        {
            comboParticle.Stop();
            comboParticle.Play();
        }

        _activeSequence = DOTween.Sequence();
        _activeSequence.SetUpdate(true);

        float randomTilt = Random.Range(-maxRotationTilt, maxRotationTilt);
        comboTextRect.localRotation = Quaternion.Euler(0f, 0f, randomTilt);

        _activeSequence.Append(comboTextRect.DOScale(Vector3.one * sizeMult, 0.3f).SetEase(Ease.OutBack, 3f));
        _activeSequence.AppendInterval(0.15f);
        
        float floatDist = _isFullComboActive ? 180f : 100f;
        _activeSequence.Append(comboTextRect.DOAnchorPosY(_originalTextPos.y + floatDist, 0.5f).SetEase(Ease.InSine));
        _activeSequence.Join(comboText.DOFade(0f, 0.4f));
        _activeSequence.OnComplete(() => comboText.gameObject.SetActive(false));
    }

    private void CheckAndShowFeedback()
    {
        if (feedbackTierSettings == null) return;

        foreach (var setting in feedbackTierSettings)
        {
            if (currentCombo == setting.comboThreshold)
            {
                ScreenJuiceManager.Instance.PlayComboJuice(currentCombo);
                string word = setting.words[Random.Range(0, setting.words.Count)];
                TriggerFeedback(word, setting.feedbackMaterial, setting.sizeMultiplier, setting.textColor);
                PlayFeedbackSound(setting.feedbackSound);
                TriggerDotFlashEffect();
                break;
            }
        }
    }

    private void PlayFeedbackSound(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySfx(clip, 0.5f);
    }

    private void TriggerDotFlashEffect()
    {
        if (!enableDotFlashOnFeedback) return;
        if (GridDot.GridMap == null || GridDot.GridMap.Count == 0) return;

        StartCoroutine(InwardWaveRoutine());
    }

    private System.Collections.IEnumerator InwardWaveRoutine()
    {
        List<GridDot> allDots = new List<GridDot>(GridDot.GridMap.Values);
        
        Vector3 center = Vector3.zero;
        int validDotsCount = 0;
        foreach (var dot in allDots)
        {
            if (dot != null)
            {
                center += dot.transform.position;
                validDotsCount++;
            }
        }
        
        if (validDotsCount == 0) yield break;
        center /= validDotsCount;

        // Prefer the head of the snake that triggered this combo as the wave center.
        if (_lastComboSource != null)
        {
            center = _lastComboSource.HeadPosition;
        }

        allDots.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            float distA = Vector3.Distance(a.transform.position, center);
            float distB = Vector3.Distance(b.transform.position, center);
            return distA.CompareTo(distB); 
        });

        int currentBatch = 0;

        foreach (var dot in allDots)
        {
            if (dot != null)
            {
                dot.PlayLeaveEffect(dotFlashScaleAmount, dotFlashDuration);
                currentBatch++;

                if (currentBatch >= waveBatchSize)
                {
                    currentBatch = 0;
                    yield return new WaitForSecondsRealtime(waveDelay);
                }
            }
        }
    }

    public void TriggerFeedback(string message, Material customMat = null, float sizeMultiplier = 1f, Color? customColor = null)
    {
        if (feedbackText == null || feedbackTextRect == null) return;

        _feedbackSequence?.Kill();
        feedbackText.DOKill();
        feedbackTextRect.DOKill();

        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;
        feedbackText.fontSharedMaterial = customMat != null ? customMat : _defaultMaterial;
        
        feedbackText.color = customColor ?? Color.white;
        feedbackText.alpha = 1f;

        feedbackTextRect.anchoredPosition = _originalFeedbackPos;
        feedbackTextRect.localScale = Vector3.zero; 
        feedbackTextRect.localRotation = Quaternion.identity;

        Vector2 targetPos = _originalFeedbackPos + new Vector2(Random.Range(-90f, 90f), Random.Range(30f, 80f));
        float targetTilt = Random.Range(-maxRotationTilt * 2f, maxRotationTilt * 2f);

        _feedbackSequence = DOTween.Sequence();
        _feedbackSequence.SetUpdate(true);

        _feedbackSequence.AppendInterval(0.15f);

        _feedbackSequence.Append(feedbackTextRect.DOScale(Vector3.one * sizeMultiplier, 0.35f).SetEase(Ease.OutBack, 3f));
        _feedbackSequence.Join(feedbackTextRect.DOAnchorPos(targetPos, 0.35f).SetEase(Ease.OutCirc));
        _feedbackSequence.Join(feedbackTextRect.DORotate(new Vector3(0f, 0f, targetTilt), 0.35f).SetEase(Ease.OutBack));

        _feedbackSequence.Append(feedbackTextRect.DOAnchorPosY(targetPos.y + 20f, 0.3f).SetEase(Ease.InOutSine));

        _feedbackSequence.Append(feedbackTextRect.DOAnchorPosY(targetPos.y + 120f, 0.4f).SetEase(Ease.InBack));
        _feedbackSequence.Join(feedbackText.DOFade(0f, 0.35f));
        _feedbackSequence.Join(feedbackTextRect.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack));

        _feedbackSequence.OnComplete(() => feedbackText.gameObject.SetActive(false));
    }
}
