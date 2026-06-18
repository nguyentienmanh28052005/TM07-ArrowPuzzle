using DG.Tweening;
using Solo.MOST_IN_ONE;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvas : MonoBehaviour, IScreenLifecycle
{
    public enum PopupState { None, Pause, Complete, GameOver }
    public enum LoseReason { OutOfHearts, TimeOut }

    private PopupState _currentPopup = PopupState.None;
    private LoseReason _lastLoseReason = LoseReason.OutOfHearts;

    #region [ REFERENCES & SETTINGS ]
    [Header("Core UI")]
    [SerializeField] private GameObject gameContainer;
    [SerializeField] private Transform healthContainer;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private CanvasGroup overlayBg;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI currentDifficultyText;
    [SerializeField] private Image currentDifficultyTag;


    [Header("Pause Pop-up")]
    [SerializeField] private CanvasGroup pausePanel;
    [SerializeField] private Transform pauseContent;

    [Header("Complete Pop-up")]
    [SerializeField] private CanvasGroup completePanel;
    [SerializeField] private Transform completeContent;
    [SerializeField] private ParticleSystem completeParticle;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI diamondText;
    [SerializeField] private RectTransform rewardCoinIcon;
    [SerializeField] private RectTransform rewardDiamondIcon;
    [SerializeField] private RectTransform[] starFills;

    [Header("Complete Pop-up Win Streak")]
    [SerializeField] private Slider completeWinStreakSlider;
    [SerializeField] private TextMeshProUGUI completeWinStreakProgressText;
    [SerializeField] private RectTransform completeWinStreakRewardIcon;
    [SerializeField] private float completeWinStreakFillDuration = 0.35f;
    [SerializeField] private float completeWinStreakRewardResetDelay = 0.25f;

    [Header("Game Over Pop-up")]
    [SerializeField] private CanvasGroup gameOverPanel;
    [SerializeField] private Transform gameOverContent;

    [SerializeField] private GameObject hearthRevive1;
    [SerializeField] private GameObject hearthRevive2;
    [SerializeField] private GameObject timeRevive1;
    [SerializeField] private GameObject timeRevive2;

    [Header("Currency Burst Effect")]
    [SerializeField] private GameObject flyingCoinPrefab;     
    [SerializeField] private GameObject flyingDiamondPrefab;  
    [SerializeField] private int maxFlyingItems = 12;         

    [Header("Juice Settings")]
    [SerializeField] private float popupAnimDuration = 0.3f;
    [SerializeField] private float overlayAlpha = 0.75f;
    
    [Header("Currency Top Bar")]
    [SerializeField] TextMeshProUGUI currentCoinText;
    [SerializeField] TextMeshProUGUI currentDiamondText;
    [SerializeField] private TextMeshProUGUI currentCoinTextLose;
    [SerializeField] private RectTransform currentCoinIcon;
    [SerializeField] private RectTransform currentDiamondIcon;

    [Header("Tools")]
    [SerializeField] private TextMeshProUGUI currentEraseToolText;
    [SerializeField] private TextMeshProUGUI currentHintToolText;
    [SerializeField] private TextMeshProUGUI currentDashToolText;
    [SerializeField] private TextMeshProUGUI currentSpinToolText;
    [SerializeField] private GameObject dashToolPanel;

    [Header("Cinematic Level Intro")]
    [SerializeField] private CanvasGroup cinematicIntroPanel;
    [SerializeField] private RectTransform cinematicIntroIcon;
    [SerializeField] private RectTransform cinematicIntroText;
    [SerializeField] private TextMeshProUGUI cinematicTextComponent;

    private bool _isShowing = false;
    private List<GameObject> hearts;
    private int countHeart;
    private Vector2[] _heartOriginalAnchoredPositions;
    private Vector3 _pauseOriginalScale;
    private Vector3 _completeOriginalScale;
    private Vector3 _gameOverOriginalScale; // Thêm bi?n luu scale g?c c?a Game Over
    private bool _isTransitioning = false;
    
    private Vector2[] _starOriginalPositions;
    private Transform _flyingItemsRoot;
    private Coroutine _flyingCoinRoutine;
    private Coroutine _flyingDiamondRoutine;
    private Coroutine _winRewardSequenceRoutine;
    private Coroutine _winStreakCoinRoutine;
    private bool _hasPendingWinStreakState;
    private WinStreakManager.StreakState _pendingWinStreakBeforeState;
    private WinStreakManager.StreakState _pendingWinStreakAfterState;
    #endregion

    #region [ INITIALIZATION & LIFECYCLE ]
    private void Awake()
    {
        if (pauseContent != null) _pauseOriginalScale = pauseContent.localScale;
        if (completeContent != null) _completeOriginalScale = completeContent.localScale;
        if (gameOverContent != null) _gameOverOriginalScale = gameOverContent.localScale; 

        if (starFills != null)
        {
            _starOriginalPositions = new Vector2[starFills.Length];
            for (int i = 0; i < starFills.Length; i++)
            {
                if (starFills[i] != null)
                    _starOriginalPositions[i] = starFills[i].anchoredPosition;
            }
        }

        InitializeHearts();
        InitializePopups();
        InitializeTools();
        ResolveCompleteWinStreakReferences();
    }

    public void OnScreenShow()
    {
        SetupLevelInfo();
        RefreshCurrencyUI();
        UpdateToolCountText(null);
        ResetHeartsState();
        _isTransitioning = TransitionManager.Instance != null && TransitionManager.Instance.IsTransitioning;
    }

    public void OnScreenHide()
    {
        _currentPopup = PopupState.None;
        _lastLoseReason = LoseReason.OutOfHearts;
        _isShowing = false;
        _hasPendingWinStreakState = false;
        StopAllCoroutines();
        ResetScreenJuice();
        ClearFlyingItems();
        HideFeedbackText();
        HidePanelImmediate(overlayBg);
        HidePanelImmediate(pausePanel);
        HidePanelImmediate(completePanel);
        HidePanelImmediate(gameOverPanel);
    }

    private void InitializeTools()
    {
        UpdateToolCountText(null);
    }

    private void RefreshCurrencyUI()
    {
        if (CurrencyManager.Instance == null) return;

        if (currentCoinText != null)
        {
            currentCoinText.text = Mathf.RoundToInt(CurrencyManager.Instance.Coins).ToString();
        }

        if (currentDiamondText != null)
        {
            currentDiamondText.text = Mathf.RoundToInt(CurrencyManager.Instance.Diamonds).ToString();
        }

        if (currentCoinTextLose != null)
        {
            currentCoinTextLose.text = Mathf.RoundToInt(CurrencyManager.Instance.Coins).ToString();
        }
    }

    private void EnsureFlyingItemsRoot()
    {
        if (_flyingItemsRoot != null) return;
        GameObject root = new GameObject("FlyingItemsRoot");
        root.transform.SetParent(transform, false);
        _flyingItemsRoot = root.transform;
    }

    private void InitializeHearts()
    {
        if (healthContainer != null)
        {
            // 1. Ép Unity tính toán và x?p th?ng hàng các trái tim NGAY L?P T?C
            RectTransform containerRect = healthContainer.GetComponent<RectTransform>();
            if (containerRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
            }

            // 2. QUA C?U RÚT VÁN: T?t Component LayoutGroup di d? nó không bao gi? phá DOTween n?a!
            HorizontalLayoutGroup layout = healthContainer.GetComponent<HorizontalLayoutGroup>();
            if (layout != null) layout.enabled = false;

            countHeart = healthContainer.childCount;
            hearts = new List<GameObject>(countHeart);
            _heartOriginalAnchoredPositions = new Vector2[countHeart];

            int idx = 0;
            foreach (Transform child in healthContainer)
            {
                hearts.Add(child.gameObject);

                RectTransform rect = child.GetComponent<RectTransform>();
                _heartOriginalAnchoredPositions[idx] = rect != null ? rect.anchoredPosition : Vector2.zero;
                idx++;
            }
        }
    }

    private void ResetHeartsState()
    {
        LevelDataV2 currentLevel = PlaytestSession.GetActiveLevelData();
        if (currentLevel != null)
        {
            SetupModeUI(currentLevel.gameMode);
        }

        if (healthContainer == null) return;

        if (hearts == null || hearts.Count == 0 || hearts.Count != healthContainer.childCount)
        {
            InitializeHearts();
        }

        if (hearts == null || hearts.Count == 0) return;

        countHeart = hearts.Count;
        for (int i = 0; i < hearts.Count; i++)
        {
            GameObject heart = hearts[i];
            if (heart == null) continue;

            DOTween.Kill(heart);
            heart.SetActive(true);

            RectTransform rect = heart.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.DOKill();
                rect.localScale = Vector3.one;
                if (_heartOriginalAnchoredPositions != null && i < _heartOriginalAnchoredPositions.Length)
                {
                    rect.anchoredPosition = _heartOriginalAnchoredPositions[i];
                }
            }

            Image img = heart.GetComponent<Image>();
            if (img != null)
            {
                img.DOKill();
                Color c = img.color;
                c.a = 1f;
                img.color = c;
            }
        }
    }

    private void HideFeedbackText()
    {
        if (feedbackText == null) return;
        feedbackText.DOKill();
        feedbackText.transform.DOKill();
        feedbackText.alpha = 0f;
        feedbackText.gameObject.SetActive(false);
    }

    public void SetupLevelInfo()
    {
        LevelDataV2 currentLevel = PlaytestSession.GetActiveLevelData();
        if (currentLevelText != null)
        {
            currentLevelText.text = PlaytestSession.IsPlaytesting ? "Playtest" : $"Level {GameManager.Instance.level}";
        }

        if (currentDifficultyText != null && currentLevel != null) currentDifficultyText.text = $"{currentLevel.levelDifficulty} Level";
        if (currentDifficultyTag != null)
        {
            Color tagColor = Color.white;
            LevelDifficulty difficulty = currentLevel != null ? currentLevel.levelDifficulty : LevelDifficulty.Easy;
            switch (difficulty)
            {
                case LevelDifficulty.Easy: tagColor = new Color(0.4f, 0.8f, 1f); break; 
                case LevelDifficulty.Medium: tagColor = new Color(1f, 0.8f, 0.4f); break; 
                case LevelDifficulty.Hard: tagColor = new Color(1f, 0.4f, 0.4f); break; 
            }
            currentDifficultyTag.color = tagColor;
        }
    }

    public void SetupModeUI(GameMode mode)
    {
        SetReviveButtonsForLoseReason(LoseReason.OutOfHearts);
        if (healthContainer != null)
        {
            healthContainer.gameObject.SetActive(mode == GameMode.Classic || mode == GameMode.Memory);
        }
    }

    private void UpdateToolCountText(object data)
    {
        if (currentEraseToolText != null)
        {
            if(CurrencyManager.Instance.EraseToolCount > 99)
            {
                currentEraseToolText.text = "99+";
            }
            else
            {
                currentEraseToolText.text = CurrencyManager.Instance.EraseToolCount.ToString();
            }
        }

        if (currentHintToolText != null)
        {
            if(CurrencyManager.Instance.HintToolCount > 99)
            {
                currentHintToolText.text = "99+";
            }
            else
            {
                currentHintToolText.text = CurrencyManager.Instance.HintToolCount.ToString();
            }
        }

        if (currentDashToolText != null)
        {
            if(CurrencyManager.Instance.DashToolCount > 99)
            {
                currentDashToolText.text = "99+";
            }
            else
            {
                currentDashToolText.text = CurrencyManager.Instance.DashToolCount.ToString();
            }
        }

        if (currentSpinToolText != null)
        {
            if(CurrencyManager.Instance.SpinToolCount > 99)
            {
                currentSpinToolText.text = "99+";
            }
            else
            {
                currentSpinToolText.text = CurrencyManager.Instance.SpinToolCount.ToString();
            }
        }
    }

    private void ResolveCompleteWinStreakReferences()
    {
        Transform searchRoot = completeContent != null ? completeContent : completePanel != null ? completePanel.transform : null;
        if (searchRoot == null) return;

        Transform streakRoot = FindDeepChild(searchRoot, "WinStreak");
        if (streakRoot == null)
        {
            streakRoot = FindDeepChild(searchRoot, "WinSteak");
        }

        if (streakRoot == null) return;

        if (completeWinStreakSlider == null)
        {
            completeWinStreakSlider = streakRoot.GetComponentInChildren<Slider>(true);
        }

        if (completeWinStreakProgressText == null)
        {
            Transform progressText = FindDeepChild(streakRoot, "ProgressText");
            if (progressText != null)
            {
                completeWinStreakProgressText = progressText.GetComponent<TextMeshProUGUI>();
            }
        }

        if (completeWinStreakRewardIcon == null)
        {
            Transform rewardIcon = FindDeepChild(streakRoot, "RewardIcon");
            if (rewardIcon != null)
            {
                completeWinStreakRewardIcon = rewardIcon as RectTransform;
            }
        }
    }

    private void UpdateCompleteWinStreakUI(WinStreakManager.StreakState state, bool animate)
    {
        int goal = Mathf.Max(1, state.goal);
        int progress = Mathf.Clamp(state.progress, 0, goal);
        float normalized = (float)progress / goal;

        if (completeWinStreakProgressText != null)
        {
            completeWinStreakProgressText.text = progress + "/" + goal;
        }

        if (completeWinStreakSlider != null)
        {
            completeWinStreakSlider.DOKill();
            if (animate && gameObject.activeInHierarchy && completeWinStreakFillDuration > 0f)
            {
                DOTween.To(() => completeWinStreakSlider.value, value => completeWinStreakSlider.value = value, normalized, completeWinStreakFillDuration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true)
                    .SetLink(completeWinStreakSlider.gameObject);
            }
            else
            {
                completeWinStreakSlider.value = normalized;
            }
        }
    }

    private void PreparePendingWinStreak()
    {
        ResolveCompleteWinStreakReferences();

        _pendingWinStreakBeforeState = WinStreakManager.GetState();
        UpdateCompleteWinStreakUI(_pendingWinStreakBeforeState, false);

        if (PlaytestSession.IsActive)
        {
            _hasPendingWinStreakState = false;
            _pendingWinStreakAfterState = _pendingWinStreakBeforeState;
            return;
        }

        _pendingWinStreakAfterState = WinStreakManager.RegisterWin();
        _hasPendingWinStreakState = true;
    }

    private IEnumerator AnimateCompleteWinStreakProgress(int fromProgress, int toProgress, int goal)
    {
        goal = Mathf.Max(1, goal);
        fromProgress = Mathf.Clamp(fromProgress, 0, goal);
        toProgress = Mathf.Clamp(toProgress, 0, goal);

        float startValue = (float)fromProgress / goal;
        float targetValue = (float)toProgress / goal;

        if (completeWinStreakSlider == null || completeWinStreakFillDuration <= 0f)
        {
            SetCompleteWinStreakProgress(toProgress, goal);
            yield break;
        }

        bool completed = false;
        completeWinStreakSlider.DOKill();
        completeWinStreakSlider.value = startValue;
        SetCompleteWinStreakProgress(fromProgress, goal);

        Tween tween = DOTween.To(
                () => completeWinStreakSlider.value,
                value =>
                {
                    completeWinStreakSlider.value = value;
                    int displayProgress = Mathf.Clamp(Mathf.RoundToInt(value * goal), 0, goal);
                    SetCompleteWinStreakProgress(displayProgress, goal);
                },
                targetValue,
                completeWinStreakFillDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .SetLink(completeWinStreakSlider.gameObject)
            .OnComplete(() => completed = true);

        while (!_isTransitioning && !completed && tween.IsActive())
        {
            yield return null;
        }

        SetCompleteWinStreakProgress(toProgress, goal);
    }

    private void SetCompleteWinStreakProgress(int progress, int goal)
    {
        goal = Mathf.Max(1, goal);
        progress = Mathf.Clamp(progress, 0, goal);

        if (completeWinStreakProgressText != null)
        {
            completeWinStreakProgressText.text = progress + "/" + goal;
        }
    }

    private static Transform FindDeepChild(Transform root, string childName)
    {
        if (root == null || string.IsNullOrEmpty(childName)) return null;

        if (root.name == childName) return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            Transform match = FindDeepChild(child, childName);
            if (match != null) return match;
        }

        return null;
    }

    private void InitializePopups()
    {
        if (feedbackText != null) { feedbackText.alpha = 0f; feedbackText.gameObject.SetActive(false); }
        HidePanelImmediate(overlayBg);
        HidePanelImmediate(pausePanel);
        HidePanelImmediate(completePanel);
        HidePanelImmediate(gameOverPanel); // Ð?m b?o tàng hình lúc m?i vào game

        if (starFills != null)
        {
            foreach (var star in starFills)
            {
                if (star != null) star.localScale = Vector3.zero;
            }
        }

        _currentPopup = PopupState.None;
    }

    private void OnEnable()
    {
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnTakeDamage, DecreaseHeart);
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnComplete, ShowCompletePopup);
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnHintToolChanged, UpdateToolCountText);
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnEraseToolChanged, UpdateToolCountText);
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnDashToolChanged, UpdateToolCountText);
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnSpinToolChanged, UpdateToolCountText);
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnSelectDashDirection, SetDashToolPanelActive);

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TransitionStateChanged += HandleTransitionStateChanged;
        }
    }

    private void OnDisable()
    {
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnTakeDamage, DecreaseHeart);
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnComplete, ShowCompletePopup);
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnHintToolChanged, UpdateToolCountText); 
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnEraseToolChanged, UpdateToolCountText);
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnDashToolChanged, UpdateToolCountText);
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnSpinToolChanged, UpdateToolCountText);
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnSelectDashDirection, SetDashToolPanelActive);

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TransitionStateChanged -= HandleTransitionStateChanged;
        }
    }

    private void HandleTransitionStateChanged(bool isTransitioning)
    {
        _isTransitioning = isTransitioning;
    }

    #region [ CINEMATIC INTRO ]
    public void PlayCinematicIntro(LevelDifficulty difficulty, float holdDuration)
    {
        if (cinematicIntroPanel == null) return;

        // ==========================================
        // NGU?I GÁC C?NG: CH? CHO PHÉP MÀN HARD ÐU?C DI?N
        // ==========================================
        if (difficulty != LevelDifficulty.Hard)
        {
            cinematicIntroPanel.gameObject.SetActive(false);
            cinematicIntroPanel.alpha = 0f;
            return;
        }

        // N?u là Hard, b?t d?u thi?t l?p k?ch b?n
        if (cinematicTextComponent != null)
        {
            cinematicTextComponent.text = "HARD LEVEL";
            cinematicTextComponent.color = new Color(1f, 0.3f, 0.3f); 
        }

        cinematicIntroPanel.gameObject.SetActive(true);
        cinematicIntroPanel.alpha = 0f;

        // D?n d?p các Tween cu d? tránh xung d?t
        cinematicIntroIcon.DOKill();
        cinematicIntroText.DOKill();
        cinematicIntroPanel.DOKill();

        cinematicIntroIcon.localScale = Vector3.zero;
        cinematicIntroText.localScale = Vector3.zero;

        // Setup góc nghiêng l?y dà
        cinematicIntroIcon.localRotation = Quaternion.Euler(0, 0, -25f);
        cinematicIntroText.localRotation = Quaternion.Euler(0, 0, 15f);

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); 

        // 1. N?n t?i hi?n lên
        seq.Append(cinematicIntroPanel.DOFade(1f, 0.18f));

        // ==========================================
        // NH?P 1: BUNG L?A (IMPACT)
        // ==========================================
        seq.Insert(0f, cinematicIntroIcon.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack, 2.2f));
        seq.Insert(0f, cinematicIntroIcon.DORotate(Vector3.zero, 0.35f).SetEase(Ease.OutBack, 1.8f));

        seq.Insert(0.04f, cinematicIntroText.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack, 2.2f));
        seq.Insert(0.04f, cinematicIntroText.DORotate(Vector3.zero, 0.35f).SetEase(Ease.OutBack, 1.8f));

        // Gi? cú nh?n th? giác, ch? b? haptic.
        seq.Insert(0.35f, cinematicIntroIcon.DOPunchScale(new Vector3(0.15f, -0.1f, 0), 0.25f, 5, 1));

        // ==========================================
        // NH?P 2: LO L?NG (BREATHE)
        // ==========================================
        float breatheTime = holdDuration > 0.4f ? holdDuration - 0.4f : 0.5f;

        seq.Append(cinematicIntroIcon.DOScale(Vector3.one * 1.08f, breatheTime * 0.5f).SetEase(Ease.InOutSine).SetLoops(2, LoopType.Yoyo));
        seq.Join(cinematicIntroIcon.DOAnchorPosY(cinematicIntroIcon.anchoredPosition.y + 15f, breatheTime * 0.5f).SetEase(Ease.InOutSine).SetLoops(2, LoopType.Yoyo));
        seq.Join(cinematicIntroText.DOScale(Vector3.one * 1.05f, breatheTime * 0.5f).SetEase(Ease.InOutSine).SetLoops(2, LoopType.Yoyo));

        // ==========================================
        // NH?P 3: RÚT L? (EXIT)
        // ==========================================
        seq.Append(cinematicIntroIcon.DOScale(Vector3.one * 1.25f, 0.15f).SetEase(Ease.OutQuad));
        seq.Join(cinematicIntroIcon.DORotate(new Vector3(0, 0, 15f), 0.15f).SetEase(Ease.OutQuad));
        
        seq.Append(cinematicIntroIcon.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack, 2.0f));
        seq.Join(cinematicIntroText.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack, 2.0f));
        seq.Join(cinematicIntroPanel.DOFade(0f, 0.2f).SetDelay(0.1f));

        seq.OnComplete(() => {
            cinematicIntroPanel.gameObject.SetActive(false);
            cinematicIntroIcon.anchoredPosition = new Vector2(cinematicIntroIcon.anchoredPosition.x, cinematicIntroIcon.anchoredPosition.y);
            cinematicIntroIcon.localRotation = Quaternion.identity;
            cinematicIntroText.localRotation = Quaternion.identity;
        });
    }
    #endregion

    public void SetDashToolPanelActive(object data)
    {
        if (data is bool isActive)
        {
            if (!isActive && DashManager.Instance != null)
                DashManager.Instance.BeginQueuedDashRelease();

            if (dashToolPanel != null) dashToolPanel.SetActive(isActive);
        }
    }

    private void Update()
    {
        // During editor playtest, Escape is reserved for exiting back to the level editor.
        if (PlaytestSession.IsActive) return;

        if (Input.GetKeyDown(KeyCode.Escape) && !_isTransitioning)
        {
            if (_currentPopup == PopupState.Pause) ClosePause();
            else if (_currentPopup == PopupState.None) ShowPause();
        }
    }
    #endregion

    #region [ POP-UP MANAGEMENT ]
    public void ShowPause()
    {
        if (_currentPopup != PopupState.None || _isTransitioning) return;
        
        _currentPopup = PopupState.Pause;
        Time.timeScale = 0f; 
        
        ShowOverlay(true);
        OpenPopupTween(pausePanel, pauseContent, _pauseOriginalScale);
    }

    public void ClosePause()
    {
        if (_currentPopup != PopupState.Pause || _isTransitioning) return;
        
        ClosePopupTween(pausePanel, pauseContent, () => 
        {
            _currentPopup = PopupState.None;
            ShowOverlay(false);
            Time.timeScale = 1f; 
        });
    }

    public void ShowCompletePopup(object data)
    {
        if (_currentPopup != PopupState.None || _isTransitioning) return;
        if (TimeAttackManager.Instance != null) TimeAttackManager.Instance.StopTimer();

        int earnedCoins = 0;
        int earnedDiamonds = 0;

        if (data is object[] rewardData && rewardData.Length >= 2)
        {
            LevelDataV2 levelData = rewardData[0] as LevelDataV2;
            bool isFullCombo = (bool)rewardData[1];

            if (levelData != null)
            {
                earnedCoins = (int)levelData.rewardCoins;
                earnedDiamonds = isFullCombo ? (int)levelData.rewardDiamonds : 0;

                if (coinText != null) coinText.text = $"x{earnedCoins}";
                if (diamondText != null) diamondText.text = $"x{earnedDiamonds}";
            }
        }
        else if (data is LevelDataV2 levelDataFallback)
        {
            earnedCoins = (int)levelDataFallback.rewardCoins;
            earnedDiamonds = (int)levelDataFallback.rewardDiamonds;

            if (coinText != null) coinText.text = $"x{earnedCoins}";
            if (diamondText != null) diamondText.text = $"x{earnedDiamonds}";
        }

        float oldCoins = CurrencyManager.Instance.Coins - earnedCoins;
        float oldDiamonds = CurrencyManager.Instance.Diamonds - earnedDiamonds;

        if (currentCoinText != null) currentCoinText.text = Mathf.RoundToInt(oldCoins).ToString();
        if (currentDiamondText != null) currentDiamondText.text = Mathf.RoundToInt(oldDiamonds).ToString();
        PreparePendingWinStreak();

        _currentPopup = PopupState.Complete;
        AudioManager.Instance.PlaySfx(AudioManager.Instance.winSound);
        ShowOverlay(true);
        OpenPopupTween(completePanel, completeContent, _completeOriginalScale);

        if (completeParticle != null)
        {
            completeParticle.gameObject.SetActive(true);
            completeParticle.Play(true);
        }

        PlayWinSequenceEffect(earnedCoins, earnedDiamonds, oldCoins, oldDiamonds);
    }

    // ÐÃ HOÀN THI?N: Logic Bung Popup Game Over chu?n form
    public void ShowLosePopup(object data)
    {
        if (_currentPopup != PopupState.None || _isTransitioning) return;

        if (!PlaytestSession.IsActive)
        {
            WinStreakManager.ResetProgress();
        }

        _currentPopup = PopupState.GameOver;
        _lastLoseReason = ResolveLoseReason(data);
        SetReviveButtonsForLoseReason(_lastLoseReason);

        if (TimeAttackManager.Instance != null) TimeAttackManager.Instance.StopTimer();

        if (currentCoinTextLose != null) currentCoinTextLose.text = CurrencyManager.Instance.Coins.ToString();
    
        // Rung nh? màn hình m?t phát d? tang d? cay cú khi thua (Game Feel)
        if (SettingManager.Instance != null) 
        {
            SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.HeavyImpact);
        }
        AudioManager.Instance.PlaySfx(AudioManager.Instance.loseSound);
        ShowOverlay(true);
        OpenPopupTween(gameOverPanel, gameOverContent, _gameOverOriginalScale);
    }
    #endregion

    #region [ WIN SEQUENCE & CURRENCY BURST ]
    private Vector3 GetTrueWorldCenter(RectTransform rect)
    {
        if (rect == null) return Vector3.zero;
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return (corners[0] + corners[1] + corners[2] + corners[3]) / 4f;
    }

    private void PlayWinSequenceEffect(int earnedCoins, int earnedDiamonds, float oldCoins, float oldDiamonds)
    {
        foreach (var star in starFills)
        {
            if (star != null) 
            {
                star.localScale = Vector3.one * 5f; 
                
                Image img = star.GetComponent<Image>();
                if (img != null)
                {
                    Color c = img.color;
                    c.a = 0f; 
                    img.color = c;
                }
            }
        }

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); 
        seq.AppendInterval(0.4f);

        for (int i = 0; i < countHeart; i++)
        {
            if (i >= starFills.Length) break; 
            RectTransform star = starFills[i];
            if (star == null) continue;

            Image img = star.GetComponent<Image>();

            seq.Append(star.DOScale(Vector3.one, 0.2f).SetEase(Ease.InExpo));
            if (img != null)
            {
                seq.Join(img.DOFade(1f, 0.2f).SetEase(Ease.InExpo));
            }

            seq.AppendCallback(() => {
                if (_isTransitioning) return;

                star.DOKill();
                star.localScale = Vector3.one;
                star.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 1).SetUpdate(true);
                AudioManager.Instance.PlaySfx(AudioManager.Instance.starHit, 1f);

                completeContent.DOKill(false);
                completeContent.DOPunchPosition(new Vector3(0, -15f, 0), 0.2f, 10, 1).SetUpdate(true);
            });

            if (i < countHeart - 1)
                seq.AppendInterval(0.15f);
        }

        seq.OnComplete(() => {
            if (_isTransitioning) return;

            _winRewardSequenceRoutine = StartCoroutine(PlayRewardFlightsThenWinStreak(earnedCoins, earnedDiamonds, oldCoins, oldDiamonds));
        });
    }

    private IEnumerator PlayRewardFlightsThenWinStreak(int earnedCoins, int earnedDiamonds, float oldCoins, float oldDiamonds)
    {
        bool coinDone = earnedCoins <= 0 || flyingCoinPrefab == null || rewardCoinIcon == null || currentCoinIcon == null;
        bool diamondDone = earnedDiamonds <= 0 || flyingDiamondPrefab == null || rewardDiamondIcon == null || currentDiamondIcon == null;

        if (coinDone && currentCoinText != null && earnedCoins > 0)
        {
            currentCoinText.text = Mathf.RoundToInt(oldCoins + earnedCoins).ToString();
        }

        if (diamondDone && currentDiamondText != null && earnedDiamonds > 0)
        {
            currentDiamondText.text = Mathf.RoundToInt(oldDiamonds + earnedDiamonds).ToString();
        }

        if (!coinDone)
        {
            _flyingCoinRoutine = StartCoroutine(SpawnFlyingItemsAndNotify(
                flyingCoinPrefab,
                rewardCoinIcon,
                currentCoinIcon,
                Mathf.Min(earnedCoins, maxFlyingItems),
                earnedCoins,
                oldCoins,
                currentCoinText,
                () => coinDone = true));
        }

        if (!diamondDone)
        {
            _flyingDiamondRoutine = StartCoroutine(SpawnFlyingItemsAndNotify(
                flyingDiamondPrefab,
                rewardDiamondIcon,
                currentDiamondIcon,
                Mathf.Min(earnedDiamonds, maxFlyingItems),
                earnedDiamonds,
                oldDiamonds,
                currentDiamondText,
                () => diamondDone = true));
        }

        while (!_isTransitioning && (!coinDone || !diamondDone))
        {
            yield return null;
        }

        if (_isTransitioning) yield break;

        _flyingCoinRoutine = null;
        _flyingDiamondRoutine = null;

        yield return StartCoroutine(PlayCompleteWinStreakSequence());
        _winRewardSequenceRoutine = null;
    }

    private IEnumerator SpawnFlyingItemsAndNotify(
        GameObject prefab,
        RectTransform startIcon,
        RectTransform targetIcon,
        int spawnCount,
        int totalEarned,
        float oldValue,
        TextMeshProUGUI textUI,
        System.Action onComplete)
    {
        yield return StartCoroutine(SpawnFlyingItems(prefab, startIcon, targetIcon, spawnCount, totalEarned, oldValue, textUI));
        onComplete?.Invoke();
    }

    private IEnumerator PlayCompleteWinStreakSequence()
    {
        if (!_hasPendingWinStreakState) yield break;

        ResolveCompleteWinStreakReferences();

        WinStreakManager.StreakState beforeState = _pendingWinStreakBeforeState;
        WinStreakManager.StreakState afterState = _pendingWinStreakAfterState;
        _hasPendingWinStreakState = false;

        int goal = Mathf.Max(1, beforeState.goal);
        int startProgress = Mathf.Clamp(beforeState.progress, 0, goal);

        UpdateCompleteWinStreakUI(beforeState, false);
        yield return new WaitForSecondsRealtime(0.15f);

        int targetProgress = afterState.rewardClaimed ? goal : Mathf.Clamp(afterState.progress, 0, goal);
        yield return StartCoroutine(AnimateCompleteWinStreakProgress(startProgress, targetProgress, goal));

        if (!afterState.rewardClaimed)
        {
            UpdateCompleteWinStreakUI(afterState, false);
            yield break;
        }

        if (completeWinStreakRewardIcon != null)
        {
            completeWinStreakRewardIcon.DOKill();
            completeWinStreakRewardIcon.localScale = Vector3.one;
            completeWinStreakRewardIcon
                .DOPunchScale(Vector3.one * 0.18f, 0.35f, 6, 0.8f)
                .SetUpdate(true)
                .SetLink(completeWinStreakRewardIcon.gameObject);
        }

        float oldCoinsBeforeStreakReward = CurrencyManager.Instance.Coins - afterState.rewardCoins;
        float oldDiamondsBeforeStreakReward = CurrencyManager.Instance.Diamonds - afterState.rewardDiamonds;

        if (currentCoinText != null)
        {
            currentCoinText.text = Mathf.RoundToInt(oldCoinsBeforeStreakReward).ToString();
        }

        if (afterState.rewardCoins > 0 && flyingCoinPrefab != null && completeWinStreakRewardIcon != null && currentCoinIcon != null)
        {
            _winStreakCoinRoutine = StartCoroutine(SpawnFlyingItems(
                flyingCoinPrefab,
                completeWinStreakRewardIcon,
                currentCoinIcon,
                Mathf.Min(afterState.rewardCoins, maxFlyingItems),
                afterState.rewardCoins,
                oldCoinsBeforeStreakReward,
                currentCoinText));

            yield return _winStreakCoinRoutine;
            _winStreakCoinRoutine = null;
        }
        else if (currentCoinText != null)
        {
            currentCoinText.text = Mathf.RoundToInt(CurrencyManager.Instance.Coins).ToString();
        }

        if (afterState.rewardDiamonds > 0 && currentDiamondText != null)
        {
            currentDiamondText.text = Mathf.RoundToInt(oldDiamondsBeforeStreakReward + afterState.rewardDiamonds).ToString();
        }

        yield return new WaitForSecondsRealtime(completeWinStreakRewardResetDelay);
        UpdateCompleteWinStreakUI(afterState, false);
    }

    private IEnumerator SpawnFlyingItems(GameObject prefab, RectTransform startIcon, RectTransform targetIcon, int spawnCount, int totalEarned, float oldValue, TextMeshProUGUI textUI)
    {
        if (spawnCount <= 0 || totalEarned <= 0)
        {
            if (textUI != null)
            {
                textUI.text = Mathf.RoundToInt(oldValue + totalEarned).ToString();
            }
            yield break;
        }

        EnsureFlyingItemsRoot();

        Vector3 startPos = GetTrueWorldCenter(startIcon);
        Vector3 targetPos = GetTrueWorldCenter(targetIcon);
        targetPos.z = startPos.z;

        float scatterOffset = Vector3.Distance(startPos, targetPos) * 0.15f; 
        float valuePerHit = (float)totalEarned / spawnCount;
        int itemsHit = 0;

        for (int i = 0; i < spawnCount; i++)
        {
            // [B?N VÁ] N?u dã b?m Next/Replay, H?Y LUÔN vi?c d? thêm xu!
            if (_isTransitioning) yield break; 

            GameObject item = Instantiate(prefab, _flyingItemsRoot); 
            item.transform.SetAsLastSibling(); 

            RectTransform rect = item.GetComponent<RectTransform>();
            if (rect != null) rect.pivot = new Vector2(0.5f, 0.5f);

            item.transform.position = startPos;
            item.transform.localScale = Vector3.zero;

            Vector3 jumpPos = startPos + new Vector3(Random.Range(-scatterOffset, scatterOffset), Random.Range(-scatterOffset, scatterOffset), 0f);

            Sequence seq = DOTween.Sequence();
            seq.SetUpdate(true);

            seq.Append(item.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad));
            seq.Join(item.transform.DOMove(jumpPos, 0.3f).SetEase(Ease.OutQuad));
            seq.AppendInterval(Random.Range(0f, 0.15f));
            seq.Append(item.transform.DOMove(targetPos, 0.5f).SetEase(Ease.InOutSine));

            seq.OnComplete(() => {
                // [B?N VÁ] Ch?n d?ng ti?ng Ting Ting c?a nh?ng d?ng xu dang bay l? d?
                if (!_isTransitioning)
                {
                    AudioManager.Instance.PlaySfx(AudioManager.Instance.coinHit, 0.5f);
                    
                    targetIcon.DOKill();
                    targetIcon.localScale = Vector3.one;
                    targetIcon.DOPunchScale(Vector3.one * 0.2f, 0.15f, 10, 1).SetUpdate(true);
                }
                
                Destroy(item);
                
                itemsHit++;
                if (textUI != null)
                {
                    float displayValue = (itemsHit == spawnCount) ? (oldValue + totalEarned) : (oldValue + valuePerHit * itemsHit);
                    textUI.text = Mathf.RoundToInt(displayValue).ToString();
                }
            });

            yield return new WaitForSecondsRealtime(0.08f); 
        }

        while (!_isTransitioning && itemsHit < spawnCount)
        {
            yield return null;
        }
    }

    private void ClearFlyingItems()
    {
        if (_winRewardSequenceRoutine != null)
        {
            StopCoroutine(_winRewardSequenceRoutine);
            _winRewardSequenceRoutine = null;
        }

        if (_flyingCoinRoutine != null)
        {
            StopCoroutine(_flyingCoinRoutine);
            _flyingCoinRoutine = null;
        }

        if (_flyingDiamondRoutine != null)
        {
            StopCoroutine(_flyingDiamondRoutine);
            _flyingDiamondRoutine = null;
        }

        if (_winStreakCoinRoutine != null)
        {
            StopCoroutine(_winStreakCoinRoutine);
            _winStreakCoinRoutine = null;
        }

        if (_flyingItemsRoot == null) return;
        for (int i = _flyingItemsRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = _flyingItemsRoot.GetChild(i);
            if (child == null) continue;
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        if (flyingCoinPrefab != null || flyingDiamondPrefab != null)
        {
            string coinName = flyingCoinPrefab != null ? flyingCoinPrefab.name + "(Clone)" : null;
            string diamondName = flyingDiamondPrefab != null ? flyingDiamondPrefab.name + "(Clone)" : null;

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child == null || child == _flyingItemsRoot) continue;

                if ((coinName != null && child.name == coinName) || (diamondName != null && child.name == diamondName))
                {
                    child.gameObject.SetActive(false);
                    Destroy(child.gameObject);
                }
            }
        }
    }
    #endregion

    #region [ ANIMATION ENGINE ]
    private void OpenPopupTween(CanvasGroup panel, Transform content, Vector3 targetScale)
    {
        panel.gameObject.SetActive(true);
        panel.blocksRaycasts = true; 
        panel.alpha = 0f;
        
        content.DOKill();
        content.localScale = targetScale * 0.6f;

        Sequence seq = DOTween.Sequence();
        seq.Join(panel.DOFade(1f, popupAnimDuration * 0.5f));
        seq.Join(content.DOScale(targetScale * 1.05f, popupAnimDuration * 0.7f).SetEase(Ease.OutQuad));
        seq.Append(content.DOScale(targetScale, popupAnimDuration * 0.3f).SetEase(Ease.InOutSine));
        
        seq.SetUpdate(true);
        seq.SetLink(content.gameObject);
    }

    private void ClosePopupTween(CanvasGroup panel, Transform content, System.Action onComplete)
    {
        panel.blocksRaycasts = false; 

        content.DOKill();
        Sequence seq = DOTween.Sequence();
        seq.Join(content.DOScale(content.localScale * 1.02f, popupAnimDuration * 0.3f).SetEase(Ease.OutQuad));
        seq.Append(content.DOScale(Vector3.zero, popupAnimDuration * 0.7f).SetEase(Ease.InBack));
        seq.Join(panel.DOFade(0f, popupAnimDuration * 0.5f).SetDelay(popupAnimDuration * 0.5f));
        
        seq.SetUpdate(true);
        seq.SetLink(content.gameObject);
        seq.OnComplete(() => 
        {
            panel.gameObject.SetActive(false);
            onComplete?.Invoke();   
        });
    }
    #endregion

    #region [ SCENE TRANSITIONS ]
    public void RestartGame()
    {
        if (_isTransitioning) return;
        RequestScreen(ScreenType.Gameplay, true);
    }

    public void NextLevel()
    {
        if (_isTransitioning) return;
        RequestScreen(ScreenType.Gameplay, true);
    }

    public void Replay()
    {
        if (_isTransitioning) return;
        GameManager.Instance.level = Mathf.Max(1, GameManager.Instance.level - 1);
        SaveDataPlayer.Instance.Save(1, GameManager.Instance.level);
        RequestScreen(ScreenType.Gameplay, true);
    }

    public void OutLevel()
    {
        if (_isTransitioning) return;

        if (PlaytestSession.IsActive)
        {
            PlaytestExitListener.ExitPlaytest();
            return;
        }

        RequestScreen(ScreenType.MainMenu);
    }

    private void RequestScreen(ScreenType type, bool force = false)
    {
        _isTransitioning = true;
        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllSfx();
        }

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TransitionToScreen(type, force);
            return;
        }

        if (ScreenManager.Instance != null)
        {
            ScreenManager.Instance.ShowScreen(type, force);
            _isTransitioning = false;
        }
    }
    #endregion

    #region [ UTILITY LOGIC ]
    private void ShowOverlay(bool isShow)
    {
        if (overlayBg == null) return;
        
        overlayBg.DOKill();
        if (isShow)
        {
            overlayBg.gameObject.SetActive(true);
            overlayBg.blocksRaycasts = true;
            overlayBg.DOFade(overlayAlpha, popupAnimDuration).SetUpdate(true);
        }
        else
        {
            overlayBg.blocksRaycasts = false;
            overlayBg.DOFade(0f, popupAnimDuration).SetUpdate(true).OnComplete(() => overlayBg.gameObject.SetActive(false));
        }
    }

    private void HidePanelImmediate(CanvasGroup panel)
    {
        if (panel == null) return;
        panel.alpha = 0f;
        panel.blocksRaycasts = false;
        panel.gameObject.SetActive(false);
    }
    #endregion

    #region [ HEART & FEEDBACK TEXT ]
    public void ShowWinText()
    {
        string message = countHeart >= 3 ? "Perfect!" : (countHeart == 2 ? "Great!" : "Good!");
        ShowText(message, Color.yellow);
    }

    // Thêm tham s? System.Action onComplete = null
    public void ShowText(string content, Color textColor, System.Action onComplete = null)
    {
        if (feedbackText != null)
        {
            feedbackText.DOKill();
            feedbackText.transform.DOKill();

            feedbackText.text = content;
            feedbackText.color = textColor;
            feedbackText.alpha = 0f; 

            feedbackText.gameObject.SetActive(true);
            feedbackText.transform.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            seq.SetUpdate(true); 

            seq.Append(feedbackText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
            seq.Join(feedbackText.DOFade(1f, 0.25f));
            seq.AppendInterval(0.75f); 
            seq.Append(feedbackText.DOFade(0f, 0.15f));
            seq.Join(feedbackText.transform.DOScale(1.2f, 0.15f).SetEase(Ease.InQuad));
            
            seq.OnComplete(() => {
                feedbackText.gameObject.SetActive(false);
                onComplete?.Invoke(); 
            });
        }
        else
        {
            onComplete?.Invoke();
        }
    }


    public void DecreaseHeart(object data)
    {
        if (PlaytestSession.IsActive) return;

        LevelDataV2 currentLevelData = PlaytestSession.GetActiveLevelData();
        if (currentLevelData != null && currentLevelData.gameMode == GameMode.TimeAttack) return;

        if (countHeart <= 0 || _currentPopup != PopupState.None) return;

        countHeart--;
        GameObject heartObj = hearts[countHeart];
        PlayHeartLossEffect(heartObj);

        if (countHeart <= 0)
        {
            if (data is SnakeBlock sb) sb.ForceResetToOrigin();
            CameraController.IsGameplayBlocking = true;
            StartCoroutine(SequenceGameOver());
        }
    }

    private IEnumerator SequenceGameOver()
    {
        yield return new WaitForSeconds(0.1f);
    
        // ShowOverlay(true); 

        // ShowText("Game Over", Color.yellow);
        
        // yield return new WaitForSeconds(2f);
        ShowLosePopup(LoseReason.OutOfHearts); 
    }

    private void PlayHeartLossEffect(GameObject heart)
    {
        if (heart == null) return;

        DOTween.Kill(heart);
        RectTransform rect = heart.GetComponent<RectTransform>();
        Image img = heart.GetComponent<Image>();
        Vector2 originalPos = rect.anchoredPosition;

        rect.DOKill();
        if (img != null) img.DOKill();

        Sequence seq = DOTween.Sequence();
    seq.SetId(heart);
    seq.SetLink(heart);
        seq.Append(rect.DOShakeAnchorPos(0.4f, 15f, 20, 90, false));
        if (img != null) seq.Join(img.DOColor(Color.gray, 0.2f));
        
        seq.Append(rect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
        if (img != null) seq.Join(img.DOFade(0f, 0.3f));

        seq.OnComplete(() =>
        {
            heart.SetActive(false);
            rect.localScale = Vector3.one;
            rect.anchoredPosition = originalPos;
            if (img != null) img.color = Color.white;
        });
    }

    public void ShowAllPaths()
    {
        _isShowing = !_isShowing;
        MessageManager.Instance.SendMessage(ManhMessageType.OnShowAllPaths, _isShowing);
    }
    #endregion

    #region [ REVIVE SYSTEM ]
    public void Btn_ReviveWithAd()
    {
        if (_isTransitioning) return;
        
        OnReviveSuccess(1, false);
    }

    public void Btn_ReviveWithCoins()
    {
        if (_isTransitioning) return;
        
        int cost = 1000;

        if (CurrencyManager.Instance.Coins >= cost)
        {
            CurrencyManager.Instance.SpendCoins(cost); 
            if (currentCoinText != null) currentCoinText.text = Mathf.RoundToInt(CurrencyManager.Instance.Coins).ToString();
            
            OnReviveSuccess(3, true);
        }
        else
        {
            if (SettingManager.Instance != null) SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.Failure);
            if (currentCoinText != null)
            {
                currentCoinText.transform.DOKill(true);
                currentCoinText.transform.DOShakePosition(0.3f, new Vector3(10f, 0, 0), 20, 90);
                currentCoinText.DOColor(Color.red, 0.15f).SetLoops(2, LoopType.Yoyo).OnComplete(() => currentCoinText.color = Color.white);
            }
        }
    }

    private void OnReviveSuccess(int heartsToAdd, bool isFullTimeRevive)
    {
        LevelDataV2 currentLevelData = PlaytestSession.GetActiveLevelData();

        ResetScreenJuice();

        if (_lastLoseReason == LoseReason.TimeOut)
        {
            float timeToAdd = isFullTimeRevive && currentLevelData != null ? currentLevelData.timeLimit : 30f;
            
            AddTimeToGame(timeToAdd);
            if (TimeAttackManager.Instance != null) TimeAttackManager.Instance.StopTimer();

            ClosePopupTween(gameOverPanel, gameOverContent, () => 
            {
                _currentPopup = PopupState.None;
                ShowOverlay(false);
                CameraController.IsGameplayBlocking = false;
                if (TimeAttackManager.Instance != null) TimeAttackManager.Instance.ResumeTimer();
            });
            return;
        }

        if (hearts == null || hearts.Count == 0) return;

        int previousCount = countHeart;
        countHeart = Mathf.Min(countHeart + heartsToAdd, hearts.Count);

        for (int i = 0; i < hearts.Count; i++)
        {
            GameObject heart = hearts[i];
            if (heart == null) continue;

            bool shouldBeActive = i < countHeart;
            bool isNewlyRestored = shouldBeActive && i >= previousCount;

            DOTween.Kill(heart);

            RectTransform rect = heart.GetComponent<RectTransform>();
            Image img = heart.GetComponent<Image>();

            if (rect != null)
            {
                rect.DOKill();
                if (_heartOriginalAnchoredPositions != null && i < _heartOriginalAnchoredPositions.Length)
                {
                    rect.anchoredPosition = _heartOriginalAnchoredPositions[i];
                }
            }
            if (img != null) img.DOKill();

            if (!shouldBeActive)
            {
                heart.SetActive(false);
                continue;
            }

            heart.SetActive(true);

            if (img != null) img.color = Color.white;

            if (rect != null)
            {
                if (isNewlyRestored)
                {
                    rect.localScale = Vector3.zero;
                    rect.DOScale(Vector3.one, 0.4f)
                        .SetEase(Ease.OutBack)
                        .SetDelay(i * 0.5f)
                        .SetUpdate(true)
                        .SetLink(heart);
                }
                else rect.localScale = Vector3.one;
            }

            if (img != null)
            {
                if (isNewlyRestored)
                {
                    img.DOFade(1f, 0.1f)
                        .SetDelay(i * 0.5f)
                        .SetUpdate(true)
                        .SetLink(heart);
                }
                else
                {
                    Color c = img.color; c.a = 1f; img.color = c;
                }
            }
        }

        ClosePopupTween(gameOverPanel, gameOverContent, () => 
        {
            _currentPopup = PopupState.None;
            ShowOverlay(false);
            CameraController.IsGameplayBlocking = false;
            if (TimeAttackManager.Instance != null) TimeAttackManager.Instance.ResumeTimer();
        });
    }

    private void AddTimeToGame(float amount)
    {
        if (TimeAttackManager.Instance != null)
        {
            TimeAttackManager.Instance.AddTime(amount);
        }
    }

    private LoseReason ResolveLoseReason(object data)
    {
        return data is LoseReason reason ? reason : LoseReason.OutOfHearts;
    }

    private void SetReviveButtonsForLoseReason(LoseReason reason)
    {
        bool isTimeOut = reason == LoseReason.TimeOut;

        if (hearthRevive1 != null) hearthRevive1.SetActive(!isTimeOut);
        if (hearthRevive2 != null) hearthRevive2.SetActive(!isTimeOut);
        if (timeRevive1 != null) timeRevive1.SetActive(isTimeOut);
        if (timeRevive2 != null) timeRevive2.SetActive(isTimeOut);
    }

    private void ResetScreenJuice()
    {
        ScreenJuiceManager juiceManager = FindObjectOfType<ScreenJuiceManager>();
        if (juiceManager != null)
        {
            juiceManager.ClearJuiceImmediate(true);
        }
    }
    #endregion
}
