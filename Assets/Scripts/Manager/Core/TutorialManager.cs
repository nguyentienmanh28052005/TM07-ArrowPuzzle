using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Pixelplacement;

public interface ITutorialCondition
{
    bool IsApplicable(LevelDataSO levelData);
}

public enum TutorialTrigger
{
    Always,
    MemoryMode,
    HasPortals,
    HasKeycards,
    HasSnakes,
    HasDeflectors,
}

public enum TutorialHandMode
{
    Tap,
    Hold
}

public enum TutorialPositionSource
{
    ManualWorldPosition = 0,
    FirstSnake = 1
}

[System.Serializable]
public class TutorialStepConfig
{
    public string id;
    public TutorialTrigger trigger = TutorialTrigger.Always;
    public MonoBehaviour conditionProvider;
    [TextArea] public string instruction;
    public TutorialHandMode handMode = TutorialHandMode.Tap;
    
    [Header("Targeting / Position")]
    public TutorialPositionSource positionSource = TutorialPositionSource.ManualWorldPosition;
    public Vector2 customWorldPosition = Vector2.zero;
}

public class TutorialManager : Singleton<TutorialManager>
{
    [Header("UI References")]
    public CanvasGroup tutorialCanvasGroup;
    public Transform handPointer;
    public TextMeshProUGUI instructionText;

    [Header("Tutorial Timing")]
    [Min(0f)] public float delayBeforeShowingStep = 0f;

    [Header("World Hand Animation")]
    [SerializeField] private Vector3 tapPressOffset = new Vector3(0f, -0.2f, 0f);
    [SerializeField] private Vector3 holdPressOffset = new Vector3(0f, -0.14f, 0f);
    [SerializeField] private Vector3 holdDragOffset = new Vector3(0.75f, 0f, 0f);
    [SerializeField, Min(0f)] private float handDisappearDuration = 0.25f;
    [SerializeField, Min(0f)] private float handDisappearScaleMultiplier = 0.65f;

    [Header("Tutorial Steps List")]
    public List<TutorialStepConfig> tutorialSteps = new List<TutorialStepConfig>();

    private bool _isTutorialActive = false;
    private bool _isWaitingForIntro = false;
    private bool _introFinished = false;
    private bool _completedByFirstPress = false;
    private LevelDataSO _pendingLevelData;
    private Coroutine _tutorialRoutine;
    private Sequence _handTapSequence;
    private Sequence _handHoldSequence;
    private Sequence _handDisappearSequence;
    private DG.Tweening.Tween _overlayFadeTween;
    private Vector3 _handBaseScale = Vector3.one;
    private bool _hasHandBaseScale;

    private void OnEnable()
    {
        CameraController.OnIntroFinished += HandleIntroFinished;
    }

    private void OnDisable()
    {
        CameraController.OnIntroFinished -= HandleIntroFinished;
        StopTutorialImmediate();
    }

    private void StopTutorialImmediate()
    {
        _isTutorialActive = false;
        _isWaitingForIntro = false;
        _pendingLevelData = null;
        _introFinished = false;
        _completedByFirstPress = false;

        CameraController.IsCameraInputBlocked = false;

        if (_tutorialRoutine != null)
        {
            StopCoroutine(_tutorialRoutine);
            _tutorialRoutine = null;
        }

        KillOverlayTweens();

        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.DOKill();
            tutorialCanvasGroup.alpha = 0f;
            tutorialCanvasGroup.blocksRaycasts = false;
        }

        if (handPointer != null) handPointer.gameObject.SetActive(false);

        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(false);
        }
    }

    private void HandleIntroFinished()
    {
        _introFinished = true;

        if (_isTutorialActive && _isWaitingForIntro && _pendingLevelData != null)
        {
            _isWaitingForIntro = false;
            StartTutorialNow(_pendingLevelData);
        }
    }

    public void CheckAndStartTutorial(LevelDataSO levelData)
    {
        if (levelData == null || levelData.levelDifficulty != LevelDifficulty.Tutorial)
        {
            StopTutorialImmediate();
            CameraController.IsCameraInputBlocked = false;
            
            if (tutorialCanvasGroup != null)
            {
                tutorialCanvasGroup.DOKill();
                tutorialCanvasGroup.alpha = 0;
                tutorialCanvasGroup.blocksRaycasts = false;
            }
            if (instructionText != null) instructionText.gameObject.SetActive(false);
            return;
        }

        StopTutorialImmediate();

        _pendingLevelData = levelData;
        _completedByFirstPress = false;
        _isTutorialActive = true;

        // Block camera interaction during tutorial (until first arrow press).
        CameraController.IsCameraInputBlocked = true;

        KillOverlayTweens();
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.DOKill();
            tutorialCanvasGroup.alpha = 0;
            tutorialCanvasGroup.blocksRaycasts = false;
        }
        if (handPointer != null) handPointer.gameObject.SetActive(false);
        if (instructionText != null) instructionText.gameObject.SetActive(false);

        // If intro is still running, wait for CameraController.OnIntroFinished.
        _introFinished = !CameraController.IsGameplayBlocking;
        if (_introFinished)
        {
            StartTutorialNow(levelData);
        }
        else
        {
            _isWaitingForIntro = true;
        }
    }

    private void StartTutorialNow(LevelDataSO levelData)
    {
        if (!_isTutorialActive || levelData == null) return;

        KillOverlayTweens();
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.DOKill();
            tutorialCanvasGroup.alpha = 0;
            tutorialCanvasGroup.blocksRaycasts = false;
        }

        if (handPointer != null) handPointer.gameObject.SetActive(false);
        if (instructionText != null) instructionText.gameObject.SetActive(false);

        if (_tutorialRoutine != null) StopCoroutine(_tutorialRoutine);
        _tutorialRoutine = StartCoroutine(TutorialFlowRoutine(levelData));
    }

    private IEnumerator TutorialFlowRoutine(LevelDataSO levelData)
    {
        // Give UI a frame to settle after intro, then optionally delay.
        yield return null;
        if (delayBeforeShowingStep > 0f)
            yield return new WaitForSeconds(delayBeforeShowingStep);
            
        // GỌI DUY NHẤT HỆ THỐNG MỚI
        TryPlayConfiguredStep(levelData);
    }

    private bool TryPlayConfiguredStep(LevelDataSO levelData)
    {
        if (tutorialSteps == null || tutorialSteps.Count == 0 || levelData == null) return false;

        for (int i = 0; i < tutorialSteps.Count; i++)
        {
            TutorialStepConfig step = tutorialSteps[i];
            if (step == null) continue;
            
            // Check điều kiện
            if (!IsStepApplicable(step, levelData)) continue;

            PlayStep(step);
            return true; // Chỉ play 1 step đầu tiên thỏa mãn
        }

        return false;
    }

    private bool IsStepApplicable(TutorialStepConfig step, LevelDataSO levelData)
    {
        // Ưu tiên chạy Strategy Pattern nếu có
        if (step.conditionProvider != null)
        {
            ITutorialCondition condition = step.conditionProvider as ITutorialCondition;
            return condition != null && condition.IsApplicable(levelData);
        }

        // Fallback về các Enum chung
        switch (step.trigger)
        {
            case TutorialTrigger.Always:
                return true;
            case TutorialTrigger.MemoryMode:
                return levelData.gameMode == GameMode.Memory;
            case TutorialTrigger.HasPortals:
                return levelData.portals != null && levelData.portals.Count > 0;
            case TutorialTrigger.HasKeycards:
                return levelData.keycards != null && levelData.keycards.Count > 0;
            case TutorialTrigger.HasSnakes:
                return levelData.snakes != null && levelData.snakes.Count > 0;
            case TutorialTrigger.HasDeflectors:
                return levelData.deflectors != null && levelData.deflectors.Count > 0;
            default:
                return false;
        }
    }

    private void PlayStep(TutorialStepConfig step)
    {
        if (instructionText != null) instructionText.text = step.instruction;

        switch (step.positionSource)
        {
            case TutorialPositionSource.FirstSnake:
                if (TryGetWorldPosFromFirstSnake(out Vector3 worldPosFromSnake))
                {
                    ShowOverlayForStep(step, worldPosFromSnake);
                    return;
                }
                ShowStepAtWorldPosition(step, step.customWorldPosition);
                return;

            case TutorialPositionSource.ManualWorldPosition:
            default:
                ShowStepAtWorldPosition(step, step.customWorldPosition);
                return;
        }
    }

    private void ShowStepAtWorldPosition(TutorialStepConfig step, Vector2 worldPosition)
    {
        ShowOverlayForStep(step, GetHandWorldPosition(worldPosition));
    }

    private Vector3 GetHandWorldPosition(Vector2 worldPosition)
    {
        float z = handPointer != null ? handPointer.position.z : 0f;
        return new Vector3(worldPosition.x, worldPosition.y, z);
    }

    private void ShowOverlayForStep(TutorialStepConfig step, Vector3 worldPosition)
    {
        if (step.handMode == TutorialHandMode.Hold)
        {
            ShowOverlayWithHandHold(worldPosition);
        }
        else
        {
            ShowOverlayWithHandTap(worldPosition);
        }
    }

    private void ShowOverlayWithHandTap(Vector3 worldPosition)
    {
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.DOKill();
            tutorialCanvasGroup.alpha = 0f;
            tutorialCanvasGroup.blocksRaycasts = false;
            _overlayFadeTween = tutorialCanvasGroup.DOFade(1f, 0.25f).SetUpdate(true).SetLink(tutorialCanvasGroup.gameObject);
        }

        if (instructionText != null) instructionText.gameObject.SetActive(true);
        if (handPointer == null) return;

        KillHandTweens();
        handPointer.gameObject.SetActive(true);
        handPointer.position = worldPosition;
        handPointer.localScale = GetHandBaseScale();
        SetHandAlpha(1f);

        Vector3 pressedPosition = worldPosition + tapPressOffset;
        _handTapSequence = DOTween.Sequence();
        _handTapSequence.Append(handPointer.DOScale(0.88f, 0.12f).SetEase(Ease.OutQuad));
        _handTapSequence.Join(handPointer.DOMove(pressedPosition, 0.12f).SetEase(Ease.OutQuad));
        _handTapSequence.Append(handPointer.DOScale(1f, 0.14f).SetEase(Ease.OutQuad));
        _handTapSequence.Join(handPointer.DOMove(worldPosition, 0.14f).SetEase(Ease.OutQuad));
        _handTapSequence.AppendInterval(0.35f);
        _handTapSequence.SetLoops(-1, LoopType.Restart);
        _handTapSequence.SetUpdate(true);
        _handTapSequence.SetTarget(handPointer);
        _handTapSequence.SetLink(handPointer.gameObject);
    }

    private void ShowOverlayWithHandHold(Vector3 worldPosition)
    {
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.DOKill();
            tutorialCanvasGroup.alpha = 0f;
            tutorialCanvasGroup.blocksRaycasts = false;
            _overlayFadeTween = tutorialCanvasGroup.DOFade(1f, 0.25f).SetUpdate(true).SetLink(tutorialCanvasGroup.gameObject);
        }

        if (instructionText != null) instructionText.gameObject.SetActive(true);
        if (handPointer == null) return;

        KillHandTweens();
        handPointer.gameObject.SetActive(true);
        handPointer.position = worldPosition;
        handPointer.localScale = GetHandBaseScale();
        SetHandAlpha(1f);

        float pressDuration = 0.18f;
        float holdDuration = 2f;
        float dragDuration = 0.28f;
        float holdAfterDragDuration = 1f;
        float releaseDuration = 0.18f;
        float restDuration = 0.25f;

        Vector3 pressedPos = worldPosition + holdPressOffset;
        Vector3 draggedPos = pressedPos + holdDragOffset;

        _handHoldSequence = DOTween.Sequence();
        _handHoldSequence.Append(handPointer.DOScale(0.88f, pressDuration).SetEase(Ease.OutQuad));
        _handHoldSequence.Join(handPointer.DOMove(pressedPos, pressDuration).SetEase(Ease.OutQuad));
        _handHoldSequence.AppendInterval(holdDuration);
        _handHoldSequence.Append(handPointer.DOMove(draggedPos, dragDuration).SetEase(Ease.InOutSine));
        _handHoldSequence.AppendInterval(holdAfterDragDuration);
        _handHoldSequence.Append(handPointer.DOScale(1f, releaseDuration).SetEase(Ease.OutQuad));
        _handHoldSequence.Join(handPointer.DOMove(worldPosition, releaseDuration).SetEase(Ease.OutQuad));
        _handHoldSequence.AppendInterval(restDuration);
        _handHoldSequence.SetLoops(-1, LoopType.Restart);
        _handHoldSequence.SetUpdate(true);
        _handHoldSequence.SetTarget(handPointer);
        _handHoldSequence.SetLink(handPointer.gameObject);
    }

    private bool TryGetWorldPosFromFirstSnake(out Vector3 worldPos)
    {
        worldPos = Vector3.zero;

        SnakeBlock firstSnake = FindObjectOfType<SnakeBlock>();
        if (firstSnake == null) return false;

        worldPos = firstSnake.transform.position;
        if (handPointer != null) worldPos.z = handPointer.position.z;
        return true;
    }

    public void CompleteTutorial()
    {
        if (!_isTutorialActive) return;

        _isTutorialActive = false;
        _isWaitingForIntro = false;
        _pendingLevelData = null;
        _tutorialRoutine = null;
        CameraController.IsCameraInputBlocked = false;
        KillHandTweens();
        KillOverlayFadeTween();
        PlayHandDisappear();

        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.DOKill();
            _overlayFadeTween = tutorialCanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
            {
                tutorialCanvasGroup.blocksRaycasts = false;
                if (instructionText != null) instructionText.gameObject.SetActive(false);
            });
        }
        else if (instructionText != null)
        {
            instructionText.gameObject.SetActive(false);
        }
    }

    private void PlayHandDisappear()
    {
        if (handPointer == null) return;

        float duration = Mathf.Max(0f, handDisappearDuration);
        if (duration <= 0f)
        {
            handPointer.gameObject.SetActive(false);
            return;
        }

        handPointer.DOKill();
        Vector3 baseScale = GetHandBaseScale();
        _handDisappearSequence = DOTween.Sequence();
        _handDisappearSequence.Join(handPointer.DOScale(baseScale * handDisappearScaleMultiplier, duration).SetEase(Ease.InBack));

        SpriteRenderer[] spriteRenderers = handPointer.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null) continue;
            _handDisappearSequence.Join(spriteRenderers[i].DOFade(0f, duration).SetEase(Ease.InQuad));
        }

        Graphic[] graphics = handPointer.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            if (graphics[i] == null) continue;
            _handDisappearSequence.Join(graphics[i].DOFade(0f, duration).SetEase(Ease.InQuad));
        }

        _handDisappearSequence
            .SetUpdate(true)
            .SetTarget(handPointer)
            .SetLink(handPointer.gameObject)
            .OnComplete(() => handPointer.gameObject.SetActive(false));
    }

    private void SetHandAlpha(float alpha)
    {
        if (handPointer == null) return;

        SpriteRenderer[] spriteRenderers = handPointer.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null) continue;
            Color color = spriteRenderers[i].color;
            color.a = alpha;
            spriteRenderers[i].color = color;
        }

        Graphic[] graphics = handPointer.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            if (graphics[i] == null) continue;
            Color color = graphics[i].color;
            color.a = alpha;
            graphics[i].color = color;
        }
    }

    private Vector3 GetHandBaseScale()
    {
        if (handPointer == null) return Vector3.one;

        if (!_hasHandBaseScale)
        {
            _handBaseScale = handPointer.localScale;
            _hasHandBaseScale = true;
        }

        return _handBaseScale;
    }

    private void KillHandTweens()
    {
        if (_handTapSequence != null && _handTapSequence.IsActive())
        {
            _handTapSequence.Kill();
        }
        _handTapSequence = null;

        if (_handHoldSequence != null && _handHoldSequence.IsActive())
        {
            _handHoldSequence.Kill();
        }
        _handHoldSequence = null;

        if (_handDisappearSequence != null && _handDisappearSequence.IsActive())
        {
            _handDisappearSequence.Kill();
        }
        _handDisappearSequence = null;

        if (handPointer != null)
        {
            handPointer.DOKill();
        }
    }

    private void KillOverlayTweens()
    {
        KillHandTweens();
        KillOverlayFadeTween();
    }

    private void KillOverlayFadeTween()
    {
        if (_overlayFadeTween != null && _overlayFadeTween.IsActive())
        {
            _overlayFadeTween.Kill();
        }
        _overlayFadeTween = null;
    }

    public void NotifyFirstArrowPressed()
    {
        if (!_isTutorialActive) return;
        if (_completedByFirstPress) return;

        _completedByFirstPress = true;
        CompleteTutorial();
    }
}
