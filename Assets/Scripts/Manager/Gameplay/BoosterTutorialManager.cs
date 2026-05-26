using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;

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
    [SerializeField] private RectTransform dashButton;
    [SerializeField] private RectTransform spinButton;

    [Header("Fallback Anchors")]
    [SerializeField] private Vector2 hintFallbackAnchoredPos = new Vector2(0f, -250f);
    [SerializeField] private Vector2 eraseFallbackAnchoredPos = new Vector2(0f, -250f);
    [SerializeField] private Vector2 eraseSnakeFallbackAnchoredPos = new Vector2(0f, 0f);
    [SerializeField] private Vector2 dashFallbackAnchoredPos = new Vector2(0f, -250f);
    [SerializeField] private Vector2 spinFallbackAnchoredPos = new Vector2(0f, -250f);

    [Header("Texts")]
    [TextArea] [SerializeField] private string hintStepText = "Tap HINT de xem goi y.";
    [TextArea] [SerializeField] private string eraseStep1Text = "Tap ERASER de bat che do tay.";
    [TextArea] [SerializeField] private string eraseStep2Text = "Tap vao con ran de xoa.";
    [TextArea] [SerializeField] private string dashStep1Text = "Tap DASH de chon huong.";
    [TextArea] [SerializeField] private string spinStepText = "Tap SPIN de quay va giai phong nhieu mui ten.";

    [Header("Timing")]
    [SerializeField, Min(0f)] private float delayAfterIntroFinished = 0.5f;
    [SerializeField, Min(0f)] private float delayBeforeShowingStep = 0.5f;

    [Header("Triggers")]
    [SerializeField] private int hintTutorialLevelIndex = 2;
    [SerializeField] private int eraseTutorialLevelIndex = 3;
    [SerializeField] private int dashTutorialLevelIndex = 4;
    [SerializeField] private int spinTutorialLevelIndex = 17;
    [SerializeField] private bool disableInTutorialDifficulty = true;

    [Header("Progress")]
    [SerializeField] private bool usePersistentProgress = true;
    [SerializeField] private bool ignoreSavedProgressInEditor = true;

    private const int HINT_TUTORIAL_DONE = 310;
    private const int ERASE_TUTORIAL_DONE = 311;
    private const int DASH_TUTORIAL_DONE = 312;
    private const int SPIN_TUTORIAL_DONE = 313;

    private enum BoosterType { None, Hint, Erase, Dash, Spin }

    private BoosterType _activeBooster = BoosterType.None;
    private BoosterType _pendingBooster = BoosterType.None;
    private int _stepIndex = -1;
    private bool _isActive;
    private bool _isWaitingForIntro;
    private bool _blockArrowInput;

    private Sequence _handTapSequence;
    private Tween _overlayFadeTween;
    private Coroutine _introFinishedDelayRoutine;
    private Coroutine _showDelayRoutine;

    public bool IsBlockingArrowInput => _blockArrowInput;
    public bool IsWaitingForSpinButtonPress => _isActive && _activeBooster == BoosterType.Spin && _stepIndex == 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        CameraController.OnIntroFinished += HandleIntroFinished;
    }

    private void OnDisable()
    {
        CameraController.OnIntroFinished -= HandleIntroFinished;
        StopTutorialImmediate();
    }

    public void CheckAndStartBoosterTutorial(LevelDataSO levelData)
    {
        if (levelData == null) return;
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

    private BoosterType GetBoosterForLevel(LevelDataSO levelData)
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

        return BoosterType.None;
    }

    private void StartBoosterTutorial(BoosterType booster)
    {
        StopTutorialImmediate();

        _activeBooster = booster;
        _isActive = true;
        _stepIndex = 0;
        _blockArrowInput = true;

        CameraController.IsCameraInputBlocked = true;

        if (delayBeforeShowingStep > 0f)
        {
            _showDelayRoutine = StartCoroutine(ShowCurrentStepAfterDelay());
            return;
        }

        ShowCurrentStep();
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
        }
    }

    private void ShowStepOnTarget(RectTransform target, Vector2 fallbackAnchoredPos, string text)
    {
        if (instructionText != null)
        {
            instructionText.text = text;
            instructionText.gameObject.SetActive(true);
        }

        Vector2 anchoredPos;
        if (!TryGetHandAnchoredPosFromTargetRect(target, out anchoredPos))
        {
            anchoredPos = fallbackAnchoredPos;
        }

        ShowOverlayWithHandTap(anchoredPos);
    }

    private void ShowStepOnFirstSnake(Vector2 fallbackAnchoredPos, string text)
    {
        //overlayPanel.gameObject.SetActive(false);
        if (instructionText != null)
        {
            instructionText.text = text;
            instructionText.gameObject.SetActive(true);
        }

        Vector2 anchoredPos;
        if (!TryGetAnchoredPosFromFirstSnake(out anchoredPos))
        {
            anchoredPos = fallbackAnchoredPos;
        }

        ShowOverlayWithHandTap(anchoredPos);
    }

    private void ShowOverlayWithHandTap(Vector2 anchoredPos)
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

        Vector2 pressOffset = new Vector2(0f, -20f);
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

        SnakeBlock firstSnake = FindObjectOfType<SnakeBlock>();
        CameraController camController = FindObjectOfType<CameraController>();

        if (firstSnake == null || camController == null) return false;

        Camera playCam = camController.GetComponent<Camera>();
        if (playCam == null) playCam = Camera.main;
        if (playCam == null) return false;

        Vector2 screenPos = playCam.WorldToScreenPoint(firstSnake.transform.position);
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
        _pendingBooster = BoosterType.None;
        _activeBooster = BoosterType.None;
        _stepIndex = -1;
        _blockArrowInput = false;

        CameraController.IsCameraInputBlocked = false;
        overlayPanel.gameObject.SetActive(false);
        KillOverlayTweens();

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
            default: return 0;
        }
    }

    public void NotifyHintTriggered()
    {
        if (!_isActive || _activeBooster != BoosterType.Hint || _stepIndex != 0) return;
        _stepIndex++;
        CompleteTutorial();
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

    public void NotifyDashDirectionSelected(ArrowDir dir)
    {
        return;
    }
}
