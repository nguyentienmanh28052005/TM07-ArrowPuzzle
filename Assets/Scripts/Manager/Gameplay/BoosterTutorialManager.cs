using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class BoosterTutorialManager : MonoBehaviour
{
    public static BoosterTutorialManager Instance;

    [Header("UI")]
    [SerializeField] private CanvasGroup tutorialCanvasGroup;
    [SerializeField] private RectTransform handPointer;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private GameObject overlayPanel;

    [Header("Targets")]
    [SerializeField] private RectTransform hintButton;
    [SerializeField] private RectTransform eraseButton;
    [SerializeField] private RectTransform eraseStep2Target;
    [SerializeField] private RectTransform dashButton;
    [SerializeField] private RectTransform spinButton;
    [SerializeField] private RectTransform themeButton;

    [Header("Fallback Anchors")]
    [SerializeField] private Vector2 hintFallbackAnchoredPos = new Vector2(0f, -250f);
    [SerializeField] private Vector2 eraseFallbackAnchoredPos = new Vector2(0f, -250f);
    [SerializeField] private Vector2 eraseSnakeFallbackAnchoredPos = new Vector2(0f, 0f);
    [SerializeField] private Vector2 dashFallbackAnchoredPos = new Vector2(0f, -250f);
    [SerializeField] private Vector2 spinFallbackAnchoredPos = new Vector2(0f, -250f);
    [SerializeField] private Vector2 themeFallbackAnchoredPos = new Vector2(0f, 250f);

    [Header("Theme Tutorial Hand")]
    [SerializeField] private float themeHandRotationZ = 180f;
    [SerializeField] private Vector2 themeHandPressOffset = new Vector2(0f, 20f);

    [Header("Texts")]
    [TextArea] [SerializeField] private string hintStepText = "Tap HINT de xem goi y.";
    [TextArea] [SerializeField] private string eraseStep1Text = "Tap ERASER de bat che do tay.";
    [TextArea] [SerializeField] private string eraseStep2Text = "Tap vao con ran de xoa.";
    [TextArea] [SerializeField] private string dashStep1Text = "Tap DASH de chon huong.";
    [TextArea] [SerializeField] private string spinStepText = "Tap SPIN de quay va giai phong nhieu mui ten.";
    [TextArea] [SerializeField] private string themeStepText = "Tap nut THEME de doi giao dien.";

    [Header("Timing")]
    [SerializeField, Min(0f)] private float delayAfterIntroFinished = 0.5f;
    [SerializeField, Min(0f)] private float delayBeforeShowingStep = 0.5f;

    [Header("Instruction Text Effect")]
    [SerializeField] private bool enableInstructionTextEffect = true;
    [SerializeField, Min(0.01f)] private float instructionTextIntroDuration = 0.28f;
    [SerializeField, Range(0.5f, 1f)] private float instructionTextStartScale = 0.86f;
    [SerializeField, Range(1f, 1.2f)] private float instructionTextPulseScale = 1.04f;
    [SerializeField, Min(0.1f)] private float instructionTextPulseHalfDuration = 0.55f;

    [Header("Triggers")]
    [SerializeField] private int hintTutorialLevelIndex = 2;
    [SerializeField] private int eraseTutorialLevelIndex = 3;
    [SerializeField] private int dashTutorialLevelIndex = 4;
    [SerializeField] private int spinTutorialLevelIndex = 17;
    [SerializeField] private int themeTutorialLevelIndex = 8;
    [SerializeField] private bool disableInTutorialDifficulty = true;

    [Header("Unlocks")]
    [SerializeField] private bool lockBoosterButtonsUntilTutorial = true;
    [SerializeField, Min(0)] private int tutorialRewardAmount = 5;

    [Header("Reward Claim Panel")]
    [SerializeField] private bool autoCreateRewardClaimPanel = true;
    [SerializeField] private CanvasGroup rewardClaimPanel;
    [SerializeField] private Transform rewardClaimContent;
    [SerializeField] private TextMeshProUGUI rewardClaimTitleText;
    [SerializeField] private Image rewardClaimIconImage;
    [SerializeField] private TextMeshProUGUI rewardClaimAmountText;
    [SerializeField] private TextMeshProUGUI rewardClaimDescriptionText;
    [SerializeField] private Button rewardClaimButton;
    [SerializeField] private ButtonClicky rewardClaimButtonClicky;
    [SerializeField] private string rewardClaimTitleFormat = "{0} Booster";
    [SerializeField] private string rewardClaimAmountFormat = "x{0}";
    [SerializeField] private string rewardClaimDescriptionFormat = "Nhan {0} {1} booster mien phi.";
    [SerializeField, Min(0f)] private float rewardClaimTweenDuration = 0.25f;

    [Header("Reward Claim Icons")]
    [SerializeField] private Sprite hintRewardIcon;
    [SerializeField] private Sprite eraseRewardIcon;
    [SerializeField] private Sprite dashRewardIcon;
    [SerializeField] private Sprite spinRewardIcon;

    [Header("Progress")]
    [SerializeField] private bool usePersistentProgress = true;
    [SerializeField] private bool ignoreSavedProgressInEditor = true;

    private const int HINT_TUTORIAL_DONE = 310;
    private const int ERASE_TUTORIAL_DONE = 311;
    private const int DASH_TUTORIAL_DONE = 312;
    private const int SPIN_TUTORIAL_DONE = 313;
    private const int THEME_TUTORIAL_DONE = 314;
    private const int HINT_TUTORIAL_REWARD_GRANTED = 320;
    private const int ERASE_TUTORIAL_REWARD_GRANTED = 321;
    private const int DASH_TUTORIAL_REWARD_GRANTED = 322;
    private const int SPIN_TUTORIAL_REWARD_GRANTED = 323;

    private enum BoosterType { None, Hint, Erase, Dash, Spin, Theme }

    private BoosterType _activeBooster = BoosterType.None;
    private BoosterType _pendingBooster = BoosterType.None;
    private int _stepIndex = -1;
    private bool _isActive;
    private bool _isWaitingForIntro;
    private bool _blockArrowInput;

    private Sequence _handTapSequence;
    private Tween _overlayFadeTween;
    private Tween _rewardClaimPanelTween;
    private Tween _rewardClaimContentTween;
    private Sequence _instructionTextSequence;
    private Coroutine _introFinishedDelayRoutine;
    private Coroutine _showDelayRoutine;
    private Quaternion _handBaseRotation = Quaternion.identity;
    private Vector3 _instructionTextBaseScale = Vector3.one;
    private Color _instructionTextBaseColor = Color.white;
    private Vector3 _rewardClaimContentBaseScale = Vector3.one;
    private readonly HashSet<BoosterType> _sessionRewardGranted = new HashSet<BoosterType>();
    private int _currentLevelIndex = -1;
    private bool _isHintUnlocked;
    private bool _isEraseUnlocked;
    private bool _isDashUnlocked;
    private bool _isSpinUnlocked;
    private bool _isWaitingForRewardClaim;

    public bool IsBlockingArrowInput => _blockArrowInput;
    public bool IsWaitingForSpinButtonPress => _isActive && !_isWaitingForRewardClaim && _activeBooster == BoosterType.Spin && _stepIndex == 0;
    public bool IsWaitingForBoosterRewardClaim => _isWaitingForRewardClaim;
    public bool IsHintUnlocked => _isHintUnlocked;
    public bool IsEraseUnlocked => _isEraseUnlocked;
    public bool IsDashUnlocked => _isDashUnlocked;
    public bool IsSpinUnlocked => _isSpinUnlocked;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (handPointer != null)
            _handBaseRotation = handPointer.localRotation;

        if (instructionText != null)
        {
            _instructionTextBaseScale = instructionText.rectTransform.localScale;
            _instructionTextBaseColor = instructionText.color;
        }

        if (rewardClaimContent == null && rewardClaimPanel != null)
            rewardClaimContent = rewardClaimPanel.transform;

        if (rewardClaimContent != null)
            _rewardClaimContentBaseScale = rewardClaimContent.localScale;
    }

    private void Start()
    {
        HideRewardClaimPanelImmediate();
        RefreshBoosterButtonLocks(GameManager.Instance != null ? GameManager.Instance.level : -1);
    }

    private void OnEnable()
    {
        CameraController.OnIntroFinished += HandleIntroFinished;
        RegisterRewardClaimButton();
    }

    private void OnDisable()
    {
        CameraController.OnIntroFinished -= HandleIntroFinished;
        UnregisterRewardClaimButton();
        StopTutorialImmediate();
    }

    public void CheckAndStartBoosterTutorial(LevelDataV2 levelData)
    {
        if (levelData == null) return;

        _currentLevelIndex = levelData.levelIndex;
        RefreshBoosterButtonLocks(_currentLevelIndex);

        if (disableInTutorialDifficulty && levelData.levelDifficulty == LevelDifficulty.Tutorial) return;

        BoosterType boosterToPlay = GetBoosterForLevel(levelData);
        if (boosterToPlay == BoosterType.None) return;

        if (CameraController.IsGameplayBlocking)
        {
            _pendingBooster = boosterToPlay;
            _isWaitingForIntro = true;
            return;
        }

        ScheduleBoosterTutorialAfterIntroDelay(boosterToPlay);
    }

    private void HandleIntroFinished()
    {
        if (!_isWaitingForIntro || _pendingBooster == BoosterType.None) return;

        _isWaitingForIntro = false;
        ScheduleBoosterTutorialAfterIntroDelay(_pendingBooster);
    }

    private void ScheduleBoosterTutorialAfterIntroDelay(BoosterType booster)
    {
        if (booster == BoosterType.None) return;

        _pendingBooster = booster;

        if (_introFinishedDelayRoutine != null)
        {
            StopCoroutine(_introFinishedDelayRoutine);
            _introFinishedDelayRoutine = null;
        }

        if (delayAfterIntroFinished > 0f)
        {
            _introFinishedDelayRoutine = StartCoroutine(StartPendingBoosterAfterIntroDelay());
            return;
        }

        StartPendingBoosterTutorial();
    }

    private IEnumerator StartPendingBoosterAfterIntroDelay()
    {
        yield return new WaitForSecondsRealtime(delayAfterIntroFinished);
        _introFinishedDelayRoutine = null;
        StartPendingBoosterTutorial();
    }

    private void StartPendingBoosterTutorial()
    {
        if (_pendingBooster == BoosterType.None) return;

        BoosterType booster = _pendingBooster;
        _pendingBooster = BoosterType.None;
        StartBoosterTutorial(booster);
    }

    private BoosterType GetBoosterForLevel(LevelDataV2 levelData)
    {
        int levelIndex = levelData.levelIndex;

        if (levelIndex == hintTutorialLevelIndex && !IsBoosterCompleted(BoosterType.Hint))
            return BoosterType.Hint;
        if (levelIndex == eraseTutorialLevelIndex && !IsBoosterCompleted(BoosterType.Erase))
            return BoosterType.Erase;
        if (levelIndex == dashTutorialLevelIndex && !IsBoosterCompleted(BoosterType.Dash))
            return BoosterType.Dash;
        if (levelIndex == spinTutorialLevelIndex && !IsBoosterCompleted(BoosterType.Spin))
            return BoosterType.Spin;
        if (levelIndex == themeTutorialLevelIndex && !IsBoosterCompleted(BoosterType.Theme))
            return BoosterType.Theme;

        return BoosterType.None;
    }

    private void RefreshBoosterButtonLocks(int levelIndex)
    {
        bool hintUnlocked = !lockBoosterButtonsUntilTutorial || HasReachedTutorialLevel(BoosterType.Hint, levelIndex);
        bool eraseUnlocked = !lockBoosterButtonsUntilTutorial || HasReachedTutorialLevel(BoosterType.Erase, levelIndex);
        bool dashUnlocked = !lockBoosterButtonsUntilTutorial || HasReachedTutorialLevel(BoosterType.Dash, levelIndex);
        bool spinUnlocked = !lockBoosterButtonsUntilTutorial || HasReachedTutorialLevel(BoosterType.Spin, levelIndex);

        _isHintUnlocked = hintUnlocked;
        _isEraseUnlocked = eraseUnlocked;
        _isDashUnlocked = dashUnlocked;
        _isSpinUnlocked = spinUnlocked;

        SetBoosterButtonInteractable(hintButton, hintUnlocked);
        SetBoosterButtonInteractable(eraseButton, eraseUnlocked);
        SetBoosterButtonInteractable(dashButton, dashUnlocked);
        SetBoosterButtonInteractable(spinButton, spinUnlocked);
    }

    private bool HasReachedTutorialLevel(BoosterType booster, int levelIndex)
    {
        int tutorialLevelIndex = GetTutorialLevelIndex(booster);
        return tutorialLevelIndex >= 0 && levelIndex >= tutorialLevelIndex;
    }

    private int GetTutorialLevelIndex(BoosterType booster)
    {
        switch (booster)
        {
            case BoosterType.Hint: return hintTutorialLevelIndex;
            case BoosterType.Erase: return eraseTutorialLevelIndex;
            case BoosterType.Dash: return dashTutorialLevelIndex;
            case BoosterType.Spin: return spinTutorialLevelIndex;
            case BoosterType.Theme: return themeTutorialLevelIndex;
            default: return -1;
        }
    }

    private void SetBoosterButtonInteractable(RectTransform target, bool isUnlocked)
    {
        if (target == null) return;

        ButtonClicky clicky = target.GetComponent<ButtonClicky>();
        if (clicky == null) clicky = target.GetComponentInParent<ButtonClicky>();
        if (clicky == null) clicky = target.GetComponentInChildren<ButtonClicky>(true);
        if (clicky != null)
        {
            if (clicky.isActiveAndEnabled)
                clicky.SetInteractable(isUnlocked);
            else
                clicky.interactable = isUnlocked;
        }

        Button unityButton = target.GetComponent<Button>();
        if (unityButton == null) unityButton = target.GetComponentInParent<Button>();
        if (unityButton == null) unityButton = target.GetComponentInChildren<Button>(true);
        if (unityButton != null) unityButton.interactable = isUnlocked;
    }

    private void GrantTutorialRewardIfNeeded(BoosterType booster)
    {
        if (tutorialRewardAmount <= 0 || HasTutorialRewardBeenGranted(booster)) return;
        if (CurrencyManager.Instance == null) return;

        switch (booster)
        {
            case BoosterType.Hint:
                CurrencyManager.Instance.AddHintTool(tutorialRewardAmount);
                break;
            case BoosterType.Erase:
                CurrencyManager.Instance.AddEraseTool(tutorialRewardAmount);
                break;
            case BoosterType.Dash:
                CurrencyManager.Instance.AddDashTool(tutorialRewardAmount);
                break;
            case BoosterType.Spin:
                CurrencyManager.Instance.AddSpinTool(tutorialRewardAmount);
                break;
            default:
                return;
        }

        SetTutorialRewardGranted(booster);
    }

    private bool HasTutorialRewardBeenGranted(BoosterType booster)
    {
        if (!IsRewardBooster(booster)) return true;
        if (ShouldIgnoreSavedProgress()) return _sessionRewardGranted.Contains(booster);
        if (SaveDataPlayer.Instance == null) return false;

        int key = GetBoosterRewardSaveKey(booster);
        return key != 0 && SaveDataPlayer.Instance.Value(key) > 0f;
    }

    private void SetTutorialRewardGranted(BoosterType booster)
    {
        if (!IsRewardBooster(booster)) return;

        _sessionRewardGranted.Add(booster);

        if (ShouldIgnoreSavedProgress()) return;
        if (SaveDataPlayer.Instance == null) return;

        int key = GetBoosterRewardSaveKey(booster);
        if (key != 0) SaveDataPlayer.Instance.Save(key, 1f);
    }

    private bool IsRewardBooster(BoosterType booster)
    {
        return booster == BoosterType.Hint
            || booster == BoosterType.Erase
            || booster == BoosterType.Dash
            || booster == BoosterType.Spin;
    }

    private int GetBoosterRewardSaveKey(BoosterType booster)
    {
        switch (booster)
        {
            case BoosterType.Hint: return HINT_TUTORIAL_REWARD_GRANTED;
            case BoosterType.Erase: return ERASE_TUTORIAL_REWARD_GRANTED;
            case BoosterType.Dash: return DASH_TUTORIAL_REWARD_GRANTED;
            case BoosterType.Spin: return SPIN_TUTORIAL_REWARD_GRANTED;
            default: return 0;
        }
    }

    private void RegisterRewardClaimButton()
    {
        if (rewardClaimButton != null)
            rewardClaimButton.onClick.AddListener(ClaimBoosterTutorialReward);

        if (rewardClaimButtonClicky != null)
        {
            if (rewardClaimButtonClicky.onClick == null)
                rewardClaimButtonClicky.onClick = new UnityEngine.Events.UnityEvent();

            rewardClaimButtonClicky.onClick.AddListener(ClaimBoosterTutorialReward);
        }
    }

    private void UnregisterRewardClaimButton()
    {
        if (rewardClaimButton != null)
            rewardClaimButton.onClick.RemoveListener(ClaimBoosterTutorialReward);

        if (rewardClaimButtonClicky != null)
            rewardClaimButtonClicky.onClick.RemoveListener(ClaimBoosterTutorialReward);
    }

    private void StartBoosterTutorial(BoosterType booster)
    {
        StopTutorialImmediate();

        _activeBooster = booster;
        _isActive = true;
        _stepIndex = 0;
        _blockArrowInput = true;

        CameraController.IsCameraInputBlocked = true;

        if (ShouldShowRewardClaimPanel(booster))
        {
            _isWaitingForRewardClaim = true;
            ShowRewardClaimPanel(booster);
            return;
        }

        GrantTutorialRewardIfNeeded(booster);
        BeginBoosterTutorialSteps();
    }

    private bool ShouldShowRewardClaimPanel(BoosterType booster)
    {
        if (rewardClaimPanel == null && autoCreateRewardClaimPanel)
            CreateDefaultRewardClaimPanel();

        return rewardClaimPanel != null
            && tutorialRewardAmount > 0
            && IsRewardBooster(booster)
            && !HasTutorialRewardBeenGranted(booster);
    }

    private void BeginBoosterTutorialSteps()
    {
        if (!_isActive || _activeBooster == BoosterType.None) return;

        if (delayBeforeShowingStep > 0f)
        {
            _showDelayRoutine = StartCoroutine(ShowCurrentStepAfterDelay());
            return;
        }

        ShowCurrentStep();
    }

    private void ShowRewardClaimPanel(BoosterType booster)
    {
        UpdateRewardClaimPanelText(booster);
        UpdateRewardClaimPanelIcon(booster);

        rewardClaimPanel.DOKill();
        if (rewardClaimContent != null) rewardClaimContent.DOKill();

        rewardClaimPanel.gameObject.SetActive(true);
        rewardClaimPanel.transform.SetAsLastSibling();
        rewardClaimPanel.alpha = 0f;
        rewardClaimPanel.interactable = true;
        rewardClaimPanel.blocksRaycasts = true;

        if (rewardClaimContent != null)
            rewardClaimContent.localScale = _rewardClaimContentBaseScale * 0.85f;

        float duration = Mathf.Max(0.01f, rewardClaimTweenDuration);
        _rewardClaimPanelTween = rewardClaimPanel
            .DOFade(1f, duration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .SetLink(rewardClaimPanel.gameObject);

        if (rewardClaimContent != null)
        {
            _rewardClaimContentTween = rewardClaimContent
                .DOScale(_rewardClaimContentBaseScale, duration)
                .SetEase(Ease.OutBack, 1.6f)
                .SetUpdate(true)
                .SetLink(rewardClaimContent.gameObject);
        }
    }

    private void CreateDefaultRewardClaimPanel()
    {
        Canvas canvas = tutorialCanvasGroup != null ? tutorialCanvasGroup.GetComponentInParent<Canvas>() : null;
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        GameObject panelObject = new GameObject("BoosterRewardClaimPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        panelObject.transform.SetParent(canvas.transform, false);
        panelObject.transform.SetAsLastSibling();

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.68f);

        rewardClaimPanel = panelObject.GetComponent<CanvasGroup>();

        GameObject contentObject = new GameObject("Content", typeof(RectTransform), typeof(Image));
        contentObject.transform.SetParent(panelObject.transform, false);
        rewardClaimContent = contentObject.transform;
        _rewardClaimContentBaseScale = Vector3.one;

        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(520f, 360f);

        Image contentImage = contentObject.GetComponent<Image>();
        contentImage.color = new Color(0.08f, 0.09f, 0.12f, 0.96f);

        rewardClaimTitleText = CreateDefaultRewardText(contentRect, "Title", new Vector2(0f, 126f), new Vector2(440f, 50f), 32f, Color.white, FontStyles.Bold);
        rewardClaimIconImage = CreateDefaultRewardIcon(contentRect);
        rewardClaimAmountText = CreateDefaultRewardText(contentRect, "Amount", new Vector2(0f, -16f), new Vector2(440f, 60f), 44f, new Color(1f, 0.86f, 0.28f, 1f), FontStyles.Bold);
        rewardClaimDescriptionText = CreateDefaultRewardText(contentRect, "Description", new Vector2(0f, -72f), new Vector2(420f, 48f), 20f, new Color(0.86f, 0.9f, 0.96f, 1f), FontStyles.Normal);

        rewardClaimButton = CreateDefaultRewardClaimButton(contentRect);
        rewardClaimButton.onClick.AddListener(ClaimBoosterTutorialReward);

        HideRewardClaimPanelImmediate();
    }

    private Image CreateDefaultRewardIcon(RectTransform parent)
    {
        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(parent, false);

        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 60f);
        rect.sizeDelta = new Vector2(86f, 86f);

        Image image = iconObject.GetComponent<Image>();
        image.preserveAspect = true;
        image.color = Color.white;
        return image;
    }

    private TextMeshProUGUI CreateDefaultRewardText(RectTransform parent, string name, Vector2 anchoredPosition, Vector2 size, float fontSize, Color color, FontStyles fontStyle)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.text = string.Empty;
        return text;
    }

    private Button CreateDefaultRewardClaimButton(RectTransform parent)
    {
        GameObject buttonObject = new GameObject("ClaimButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -125f);
        rect.sizeDelta = new Vector2(240f, 68f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.18f, 0.68f, 0.36f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        TextMeshProUGUI label = CreateDefaultRewardText(rect, "Label", Vector2.zero, new Vector2(220f, 54f), 28f, Color.white, FontStyles.Bold);
        label.text = "NHAN";

        return button;
    }

    private void UpdateRewardClaimPanelText(BoosterType booster)
    {
        string boosterName = GetBoosterDisplayName(booster);

        if (rewardClaimTitleText != null)
            rewardClaimTitleText.text = string.Format(rewardClaimTitleFormat, boosterName);

        if (rewardClaimAmountText != null)
            rewardClaimAmountText.text = string.Format(rewardClaimAmountFormat, tutorialRewardAmount);

        if (rewardClaimDescriptionText != null)
            rewardClaimDescriptionText.text = string.Format(rewardClaimDescriptionFormat, tutorialRewardAmount, boosterName);
    }

    private void UpdateRewardClaimPanelIcon(BoosterType booster)
    {
        if (rewardClaimIconImage == null) return;

        Sprite icon = GetRewardIcon(booster);
        rewardClaimIconImage.sprite = icon;
        rewardClaimIconImage.enabled = icon != null;
        rewardClaimIconImage.preserveAspect = true;
    }

    private Sprite GetRewardIcon(BoosterType booster)
    {
        Sprite configuredIcon = GetConfiguredRewardIcon(booster);
        if (configuredIcon != null) return configuredIcon;

        RectTransform target = GetBoosterButtonTarget(booster);
        if (target == null) return null;

        ButtonClicky clicky = target.GetComponent<ButtonClicky>();
        if (clicky == null) clicky = target.GetComponentInParent<ButtonClicky>();
        if (clicky == null) clicky = target.GetComponentInChildren<ButtonClicky>(true);
        if (clicky != null && clicky.DefaultSprite != null) return clicky.DefaultSprite;

        Image image = target.GetComponent<Image>();
        if (image == null) image = target.GetComponentInChildren<Image>(true);
        return image != null ? image.sprite : null;
    }

    private Sprite GetConfiguredRewardIcon(BoosterType booster)
    {
        switch (booster)
        {
            case BoosterType.Hint: return hintRewardIcon;
            case BoosterType.Erase: return eraseRewardIcon;
            case BoosterType.Dash: return dashRewardIcon;
            case BoosterType.Spin: return spinRewardIcon;
            default: return null;
        }
    }

    private RectTransform GetBoosterButtonTarget(BoosterType booster)
    {
        switch (booster)
        {
            case BoosterType.Hint: return hintButton;
            case BoosterType.Erase: return eraseButton;
            case BoosterType.Dash: return dashButton;
            case BoosterType.Spin: return spinButton;
            case BoosterType.Theme: return themeButton;
            default: return null;
        }
    }

    private void HideRewardClaimPanel(System.Action onComplete)
    {
        if (rewardClaimPanel == null)
        {
            onComplete?.Invoke();
            return;
        }

        rewardClaimPanel.DOKill();
        if (rewardClaimContent != null) rewardClaimContent.DOKill();

        rewardClaimPanel.interactable = false;
        rewardClaimPanel.blocksRaycasts = false;

        float duration = Mathf.Max(0.01f, rewardClaimTweenDuration);
        _rewardClaimPanelTween = rewardClaimPanel
            .DOFade(0f, duration)
            .SetEase(Ease.InQuad)
            .SetUpdate(true)
            .SetLink(rewardClaimPanel.gameObject)
            .OnComplete(() =>
            {
                HideRewardClaimPanelImmediate();
                onComplete?.Invoke();
            });

        if (rewardClaimContent != null)
        {
            _rewardClaimContentTween = rewardClaimContent
                .DOScale(_rewardClaimContentBaseScale * 0.92f, duration)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .SetLink(rewardClaimContent.gameObject);
        }
    }

    private void HideRewardClaimPanelImmediate()
    {
        if (rewardClaimPanel == null) return;

        if (_rewardClaimPanelTween != null && _rewardClaimPanelTween.IsActive())
            _rewardClaimPanelTween.Kill();
        _rewardClaimPanelTween = null;

        if (_rewardClaimContentTween != null && _rewardClaimContentTween.IsActive())
            _rewardClaimContentTween.Kill();
        _rewardClaimContentTween = null;

        rewardClaimPanel.DOKill();
        rewardClaimPanel.alpha = 0f;
        rewardClaimPanel.interactable = false;
        rewardClaimPanel.blocksRaycasts = false;
        rewardClaimPanel.gameObject.SetActive(false);

        if (rewardClaimContent != null)
        {
            rewardClaimContent.DOKill();
            rewardClaimContent.localScale = _rewardClaimContentBaseScale;
        }
    }

    private string GetBoosterDisplayName(BoosterType booster)
    {
        switch (booster)
        {
            case BoosterType.Hint: return "Hint";
            case BoosterType.Erase: return "Eraser";
            case BoosterType.Dash: return "Dash";
            case BoosterType.Spin: return "Spin";
            case BoosterType.Theme: return "Theme";
            default: return "Booster";
        }
    }

    private IEnumerator ShowCurrentStepAfterDelay()
    {
        yield return new WaitForSecondsRealtime(delayBeforeShowingStep);
        _showDelayRoutine = null;

        if (_isActive)
            ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        if (!_isActive) return;

        _blockArrowInput = _stepIndex == 0;

        switch (_activeBooster)
        {
            case BoosterType.Hint:
                if (_stepIndex == 0)
                {
                    overlayPanel.gameObject.SetActive(true);
                    ShowStepOnTarget(hintButton, hintFallbackAnchoredPos, hintStepText);
                }
                else
                    CompleteTutorial();
                break;

            case BoosterType.Erase:
                if (_stepIndex == 0)
                {
                    overlayPanel.gameObject.SetActive(true);
                    ShowStepOnTarget(eraseButton, eraseFallbackAnchoredPos, eraseStep1Text);
                }
                else if (_stepIndex == 1)
                {
                    overlayPanel.gameObject.SetActive(false);
                    ShowStepOnFirstSnake(eraseSnakeFallbackAnchoredPos, eraseStep2Text);
                }
                else
                    CompleteTutorial();
                break;

            case BoosterType.Dash:
                if (_stepIndex == 0)
                {
                    overlayPanel.gameObject.SetActive(true);
                    ShowStepOnTarget(dashButton, dashFallbackAnchoredPos, dashStep1Text);
                }
                else
                    CompleteTutorial();
                break;

            case BoosterType.Spin:
                if (_stepIndex == 0)
                {
                    overlayPanel.gameObject.SetActive(true);
                    ShowStepOnTarget(spinButton, spinFallbackAnchoredPos, spinStepText);
                }
                else
                    CompleteTutorial();
                break;

            case BoosterType.Theme:
                if (_stepIndex == 0)
                {
                    overlayPanel.gameObject.SetActive(true);
                    ShowStepOnTarget(themeButton, themeFallbackAnchoredPos, themeStepText, themeHandRotationZ, themeHandPressOffset);
                }
                else
                    CompleteTutorial();
                break;
        }
    }

    private void ShowStepOnTarget(RectTransform target, Vector2 fallbackAnchoredPos, string text)
    {
        ShowStepOnTarget(target, fallbackAnchoredPos, text, 0f, new Vector2(0f, -20f));
    }

    private void ShowStepOnTarget(RectTransform target, Vector2 fallbackAnchoredPos, string text, float handRotationZ, Vector2 pressOffset)
    {
        ShowInstructionText(text);

        Vector2 anchoredPos;
        if (!TryGetHandAnchoredPosFromTargetRect(target, out anchoredPos))
        {
            anchoredPos = fallbackAnchoredPos;
        }

        ShowOverlayWithHandTap(anchoredPos, handRotationZ, pressOffset);
    }

    private void ShowStepOnFirstSnake(Vector2 fallbackAnchoredPos, string text)
    {
        //overlayPanel.gameObject.SetActive(false);
        ShowInstructionText(text);

        Vector2 anchoredPos;
        if (!TryGetHandAnchoredPosFromTargetRect(eraseStep2Target, out anchoredPos) &&
            !TryGetAnchoredPosFromFirstSnake(out anchoredPos))
        {
            anchoredPos = fallbackAnchoredPos;
        }

        ShowOverlayWithHandTap(anchoredPos);
    }

    private void ShowInstructionText(string text)
    {
        if (instructionText == null)
        {
            return;
        }

        KillInstructionTextTweens();

        instructionText.text = text;
        instructionText.gameObject.SetActive(true);

        RectTransform rect = instructionText.rectTransform;
        rect.localScale = _instructionTextBaseScale;
        instructionText.color = _instructionTextBaseColor;

        if (!enableInstructionTextEffect)
        {
            return;
        }

        rect.localScale = _instructionTextBaseScale * instructionTextStartScale;
        Color hiddenColor = _instructionTextBaseColor;
        hiddenColor.a = 0f;
        instructionText.color = hiddenColor;

        float introDuration = Mathf.Max(0.01f, instructionTextIntroDuration);
        _instructionTextSequence = DOTween.Sequence()
            .SetUpdate(true)
            .SetLink(instructionText.gameObject);

        _instructionTextSequence
            .Append(instructionText.DOFade(_instructionTextBaseColor.a, introDuration).SetEase(Ease.OutQuad))
            .Join(rect.DOScale(_instructionTextBaseScale, introDuration).SetEase(Ease.OutBack, 1.5f));

        if (instructionTextPulseScale > 1f)
        {
            _instructionTextSequence.Append(
                rect.DOScale(_instructionTextBaseScale * instructionTextPulseScale, instructionTextPulseHalfDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo));
        }
    }

    private void ShowOverlayWithHandTap(Vector2 anchoredPos)
    {
        ShowOverlayWithHandTap(anchoredPos, 0f, new Vector2(0f, -20f));
    }

    private void ShowOverlayWithHandTap(Vector2 anchoredPos, float handRotationZ, Vector2 pressOffset)
    {
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.DOKill();
            tutorialCanvasGroup.alpha = 0f;
            tutorialCanvasGroup.blocksRaycasts = false;
            _overlayFadeTween = tutorialCanvasGroup.DOFade(1f, 0.25f).SetUpdate(true).SetLink(tutorialCanvasGroup.gameObject);
        }

        if (handPointer == null) return;

        KillHandTweens();

        handPointer.gameObject.SetActive(true);
        handPointer.anchoredPosition = anchoredPos;
        handPointer.localScale = Vector3.one;
        handPointer.localRotation = _handBaseRotation * Quaternion.Euler(0f, 0f, handRotationZ);

        _handTapSequence = DOTween.Sequence();
        _handTapSequence.Append(handPointer.DOScale(0.88f, 0.12f).SetEase(Ease.OutQuad));
        _handTapSequence.Join(handPointer.DOAnchorPos(anchoredPos + pressOffset, 0.12f).SetEase(Ease.OutQuad));
        _handTapSequence.Append(handPointer.DOScale(1f, 0.14f).SetEase(Ease.OutQuad));
        _handTapSequence.Join(handPointer.DOAnchorPos(anchoredPos, 0.14f).SetEase(Ease.OutQuad));
        _handTapSequence.AppendInterval(0.35f);
        _handTapSequence.SetLoops(-1, LoopType.Restart);
        _handTapSequence.SetUpdate(true);
        _handTapSequence.SetTarget(handPointer);
        _handTapSequence.SetLink(handPointer.gameObject);
    }

    private bool TryGetHandAnchoredPosFromTargetRect(RectTransform target, out Vector2 anchoredPos)
    {
        anchoredPos = Vector2.zero;
        if (target == null) return false;

        Canvas canvas = tutorialCanvasGroup != null ? tutorialCanvasGroup.GetComponentInParent<Canvas>() : null;
        Camera uiCamera = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        }

        Vector2 targetScreenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, target.position);
        return TryGetHandAnchoredPosFromScreenPoint(targetScreenPos, out anchoredPos);
    }

    private bool TryGetHandAnchoredPosFromScreenPoint(Vector2 screenPos, out Vector2 anchoredPos)
    {
        anchoredPos = Vector2.zero;

        RectTransform parentRect = handPointer != null ? handPointer.parent as RectTransform : null;
        if (parentRect == null) return false;

        Canvas canvas = tutorialCanvasGroup != null ? tutorialCanvasGroup.GetComponentInParent<Canvas>() : null;
        Camera uiCamera = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        }

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, uiCamera, out anchoredPos);
    }

    private bool TryGetAnchoredPosFromFirstSnake(out Vector2 anchoredPos)
    {
        anchoredPos = Vector2.zero;

        SnakeBlock firstSnake = null;
        if (SnakeBlock.ActiveSnakes != null)
        {
            foreach (SnakeBlock snake in SnakeBlock.ActiveSnakes)
            {
                if (snake == null || !snake.gameObject.activeInHierarchy) continue;
                firstSnake = snake;
                break;
            }
        }

        if (firstSnake == null)
            firstSnake = FindObjectOfType<SnakeBlock>();

        CameraController camController = FindObjectOfType<CameraController>();

        if (firstSnake == null || camController == null) return false;

        Camera playCam = camController.GetComponent<Camera>();
        if (playCam == null) playCam = Camera.main;
        if (playCam == null) return false;

        Vector2 screenPos = playCam.WorldToScreenPoint(firstSnake.HeadPosition);
        return TryGetHandAnchoredPosFromScreenPoint(screenPos, out anchoredPos);
    }

    private void KillHandTweens()
    {
        if (_handTapSequence != null && _handTapSequence.IsActive())
        {
            _handTapSequence.Kill();
        }
        _handTapSequence = null;

        if (handPointer != null) handPointer.DOKill();
    }

    private void KillOverlayTweens()
    {
        overlayPanel.gameObject.SetActive(false);
        KillHandTweens();

        if (_overlayFadeTween != null && _overlayFadeTween.IsActive())
        {
            _overlayFadeTween.Kill();
        }
        _overlayFadeTween = null;
    }

    private void KillInstructionTextTweens()
    {
        if (_instructionTextSequence != null && _instructionTextSequence.IsActive())
        {
            _instructionTextSequence.Kill();
        }
        _instructionTextSequence = null;

        if (instructionText == null)
        {
            return;
        }

        instructionText.DOKill();
        instructionText.rectTransform.DOKill();
        instructionText.rectTransform.localScale = _instructionTextBaseScale;
        instructionText.color = _instructionTextBaseColor;
    }

    private void StopTutorialImmediate()
    {
        if (_introFinishedDelayRoutine != null)
        {
            StopCoroutine(_introFinishedDelayRoutine);
            _introFinishedDelayRoutine = null;
        }

        if (_showDelayRoutine != null)
        {
            StopCoroutine(_showDelayRoutine);
            _showDelayRoutine = null;
        }

        _isActive = false;
        _isWaitingForIntro = false;
        _isWaitingForRewardClaim = false;
        _pendingBooster = BoosterType.None;
        _activeBooster = BoosterType.None;
        _stepIndex = -1;
        _blockArrowInput = false;

        CameraController.IsCameraInputBlocked = false;
        overlayPanel.gameObject.SetActive(false);
        KillOverlayTweens();
        KillInstructionTextTweens();
        HideRewardClaimPanelImmediate();

        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.DOKill();
            tutorialCanvasGroup.alpha = 0f;
            tutorialCanvasGroup.blocksRaycasts = false;
        }

        if (handPointer != null) handPointer.gameObject.SetActive(false);
        if (instructionText != null) instructionText.gameObject.SetActive(false);
    }

    private void CompleteTutorial()
    {
        if (!_isActive) return;

        SetBoosterCompleted(_activeBooster);
        StopTutorialImmediate();
    }

    private bool IsBoosterCompleted(BoosterType booster)
    {
        if (ShouldIgnoreSavedProgress()) return false;
        if (SaveDataPlayer.Instance == null) return false;
        int key = GetBoosterSaveKey(booster);
        if (key == 0) return false;
        return SaveDataPlayer.Instance.Value(key) > 0f;
    }

    private void SetBoosterCompleted(BoosterType booster)
    {
        if (ShouldIgnoreSavedProgress()) return;
        if (SaveDataPlayer.Instance == null) return;
        int key = GetBoosterSaveKey(booster);
        if (key == 0) return;
        SaveDataPlayer.Instance.Save(key, 1f);
    }

    private bool ShouldIgnoreSavedProgress()
    {
        if (!usePersistentProgress) return true;
        if (ignoreSavedProgressInEditor && Application.isEditor) return true;
        return false;
    }

    private int GetBoosterSaveKey(BoosterType booster)
    {
        switch (booster)
        {
            case BoosterType.Hint: return HINT_TUTORIAL_DONE;
            case BoosterType.Erase: return ERASE_TUTORIAL_DONE;
            case BoosterType.Dash: return DASH_TUTORIAL_DONE;
            case BoosterType.Spin: return SPIN_TUTORIAL_DONE;
            case BoosterType.Theme: return THEME_TUTORIAL_DONE;
            default: return 0;
        }
    }

    public void NotifyHintTriggered()
    {
        if (!_isActive || _activeBooster != BoosterType.Hint || _stepIndex != 0) return;
        _stepIndex++;
        CompleteTutorial();
    }

    public void ClaimBoosterTutorialReward()
    {
        if (!_isActive || !_isWaitingForRewardClaim || _activeBooster == BoosterType.None) return;

        _isWaitingForRewardClaim = false;
        GrantTutorialRewardIfNeeded(_activeBooster);
        HideRewardClaimPanel(BeginBoosterTutorialSteps);
    }

    public void NotifyEraseModeActivated()
    {
        if (!_isActive || _activeBooster != BoosterType.Erase || _stepIndex != 0) return;
        _stepIndex = 1;
        ShowCurrentStep();
    }

    public void NotifyEraseExecuted()
    {
        if (!_isActive || _activeBooster != BoosterType.Erase || _stepIndex != 1) return;
        _stepIndex++;
        CompleteTutorial();
    }

    public void NotifyDashTriggered()
    {
        if (!_isActive || _activeBooster != BoosterType.Dash || _stepIndex != 0) return;
        _stepIndex++;
        CompleteTutorial();
    }

    public void NotifySpinTriggered()
    {
        if (!_isActive || _activeBooster != BoosterType.Spin || _stepIndex != 0) return;
        _stepIndex++;
        CompleteTutorial();
    }

    public void NotifyThemeTriggered()
    {
        if (!_isActive || _activeBooster != BoosterType.Theme || _stepIndex != 0) return;
        _stepIndex++;
        CompleteTutorial();
    }

    public void NotifyDashDirectionSelected(ArrowDir dir)
    {
        return;
    }
}
