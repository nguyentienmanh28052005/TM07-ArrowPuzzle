using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Pixelplacement;

public class TutorialManager : Singleton<TutorialManager>
{
    public CanvasGroup tutorialCanvasGroup;
    public RectTransform handPointer;
    public TextMeshProUGUI instructionText;

    [Header("Tutorial Timing")]
    [Min(0f)] public float delayBeforeShowingStep = 0f;

    [Header("Basic Move Tutorial")]
    public RectTransform basicMoveTarget;
    public bool useCustomBasicMovePosition = false;
    public Vector2 customBasicMoveAnchoredPos = Vector2.zero;

    [Header("Portal Tutorial")]
    public RectTransform portalTarget;
    public bool useCustomPortalPosition = false;
    public Vector2 customPortalAnchoredPos = Vector2.zero;

    [Header("Gate Tutorial")]
    public RectTransform gateTarget;
    public bool useCustomGatePosition = false;
    public Vector2 customGateAnchoredPos = Vector2.zero;

    [Header("Memory Tutorial")]
    public RectTransform memoryHoldTarget;
    public bool useCustomMemoryHoldPosition = false;
    public Vector2 customMemoryHoldAnchoredPos = Vector2.zero;

    private bool _isTutorialActive = false;
    private bool _isWaitingForIntro = false;
    private bool _introFinished = false;
    private bool _completedByFirstPress = false;
    private LevelDataSO _pendingLevelData;
    private Coroutine _tutorialRoutine;

    private void OnEnable()
    {
        CameraController.OnIntroFinished += HandleIntroFinished;
    }

    private void OnDisable()
    {
        CameraController.OnIntroFinished -= HandleIntroFinished;
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

        // Tutorial is informational only; do not block gameplay input.
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

        if (tutorialCanvasGroup != null)
        {
            // Keep hidden until a tutorial step starts (so hand + text appear together)
            tutorialCanvasGroup.DOKill();
            tutorialCanvasGroup.alpha = 0;
            tutorialCanvasGroup.blocksRaycasts = false;
        }

        if (handPointer != null) handPointer.gameObject.SetActive(false);
        if (instructionText != null) instructionText.gameObject.SetActive(false);

        if (_tutorialRoutine != null) StopCoroutine(_tutorialRoutine);
        _tutorialRoutine = StartCoroutine(TutorialFlowRoutine(levelData));
    }

    private void ShowOverlayWithHandTap(Vector2 anchoredPos)
    {
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.DOKill();
            tutorialCanvasGroup.alpha = 0f;
            tutorialCanvasGroup.blocksRaycasts = false;
            tutorialCanvasGroup.DOFade(1f, 0.25f);
        }

        if (instructionText != null) instructionText.gameObject.SetActive(true);

        if (handPointer == null) return;

        handPointer.DOKill();
        handPointer.gameObject.SetActive(true);
        handPointer.anchoredPosition = anchoredPos;
        handPointer.localScale = Vector3.one;

        Vector2 pressOffset = new Vector2(0f, -20f);
        Sequence tapSeq = DOTween.Sequence();
        tapSeq.Append(handPointer.DOScale(0.88f, 0.12f).SetEase(Ease.OutQuad));
        tapSeq.Join(handPointer.DOAnchorPos(anchoredPos + pressOffset, 0.12f).SetEase(Ease.OutQuad));
        tapSeq.Append(handPointer.DOScale(1f, 0.14f).SetEase(Ease.OutQuad));
        tapSeq.Join(handPointer.DOAnchorPos(anchoredPos, 0.14f).SetEase(Ease.OutQuad));
        tapSeq.AppendInterval(0.35f);
        tapSeq.SetLoops(-1, LoopType.Restart);
    }

    private void ShowOverlayWithHandHold(Vector2 anchoredPos)
    {
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.DOKill();
            tutorialCanvasGroup.alpha = 0f;
            tutorialCanvasGroup.blocksRaycasts = false;
            tutorialCanvasGroup.DOFade(1f, 0.25f);
        }

        if (instructionText != null) instructionText.gameObject.SetActive(true);

        if (handPointer == null) return;

        handPointer.DOKill();
        handPointer.gameObject.SetActive(true);
        handPointer.anchoredPosition = anchoredPos;
        handPointer.localScale = Vector3.one;

        Vector2 pressOffset = new Vector2(0f, -14f);
        Vector2 dragOffset = new Vector2(75f, 0f);

        float pressDuration = 0.18f;
        float holdDuration = 2f;
        float dragDuration = 0.28f;
        float holdAfterDragDuration = 1f;
        float releaseDuration = 0.18f;
        float restDuration = 0.25f;

        Vector2 pressedPos = anchoredPos + pressOffset;
        Vector2 draggedPos = pressedPos + dragOffset;

        Sequence holdSeq = DOTween.Sequence();
        holdSeq.Append(handPointer.DOScale(0.88f, pressDuration).SetEase(Ease.OutQuad));
        holdSeq.Join(handPointer.DOAnchorPos(pressedPos, pressDuration).SetEase(Ease.OutQuad));
        holdSeq.AppendInterval(holdDuration);
        holdSeq.Append(handPointer.DOAnchorPos(draggedPos, dragDuration).SetEase(Ease.InOutSine));
        holdSeq.AppendInterval(holdAfterDragDuration);
        holdSeq.Append(handPointer.DOScale(1f, releaseDuration).SetEase(Ease.OutQuad));
        holdSeq.Join(handPointer.DOAnchorPos(anchoredPos, releaseDuration).SetEase(Ease.OutQuad));
        holdSeq.AppendInterval(restDuration);
        holdSeq.SetLoops(-1, LoopType.Restart);
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

        if (handPointer != null)
        {
            handPointer.DOKill();
            handPointer.gameObject.SetActive(false);
        }

        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(false);
        }
    }

    private IEnumerator TutorialFlowRoutine(LevelDataSO levelData)
    {
        // Give UI a frame to settle after intro, then optionally delay.
        yield return null;
        if (delayBeforeShowingStep > 0f)
            yield return new WaitForSeconds(delayBeforeShowingStep);

        if(levelData.gameMode == GameMode.Memory)
        {
            PlayMemoryHoldTutorial();
        }
        else if (levelData.portals.Count > 0)
        {
            PlayPortalTutorial();
        }
        else if (levelData.keycards.Count > 0)
        {
            PlayGateTutorial();
        }
        else
        {
            PlayBasicMoveTutorial(levelData.snakes[0].direction);
        }
    }

    private void PlayBasicMoveTutorial(ArrowDir dir)
    {
        if (instructionText != null) instructionText.text = "Nhấn vào mũi tên để giải phóng";

        if (useCustomBasicMovePosition)
        {
            ShowOverlayWithHandTap(customBasicMoveAnchoredPos);
            return;
        }

        if (TryGetHandAnchoredPosFromTargetRect(basicMoveTarget, out Vector2 anchoredPosFromTarget))
        {
            ShowOverlayWithHandTap(anchoredPosFromTarget);
            return;
        }
        
        SnakeBlock firstSnake = FindObjectOfType<SnakeBlock>();
        CameraController camController = FindObjectOfType<CameraController>();
        
        if (firstSnake == null || camController == null) return;

        Camera playCam = camController.GetComponent<Camera>();
        if (playCam == null) playCam = Camera.main;
        if (playCam == null) return;

        Vector2 screenPos = playCam.WorldToScreenPoint(firstSnake.transform.position);

        Canvas canvas = tutorialCanvasGroup != null ? tutorialCanvasGroup.GetComponentInParent<Canvas>() : null;
        Camera uiCamera = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        }

        RectTransform parentRect = handPointer != null ? handPointer.parent as RectTransform : null;
        if (parentRect == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, uiCamera, out Vector2 localPos);

        ShowOverlayWithHandTap(localPos);
    }

    private void PlayMemoryHoldTutorial()
    {
        if (instructionText != null) instructionText.text = "Giữ để hiện mũi tên và di ra chỗ khác để hủy";

        if (useCustomMemoryHoldPosition)
        {
            ShowOverlayWithHandHold(customMemoryHoldAnchoredPos);
            return;
        }

        if (memoryHoldTarget != null)
        {
            Canvas canvas = tutorialCanvasGroup != null ? tutorialCanvasGroup.GetComponentInParent<Canvas>() : null;
            Camera uiCamera = null;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCamera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
            }

            Vector2 targetScreenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, memoryHoldTarget.position);
            if (TryGetHandAnchoredPosFromScreenPoint(targetScreenPos, out Vector2 anchoredPos))
            {
                ShowOverlayWithHandHold(anchoredPos);
                return;
            }
        }

        SnakeBlock firstSnake = FindObjectOfType<SnakeBlock>();
        CameraController camController = FindObjectOfType<CameraController>();

        if (firstSnake == null || camController == null)
        {
            ShowOverlayWithHandHold(Vector2.zero);
            return;
        }

        Camera playCam = camController.GetComponent<Camera>();
        if (playCam == null) playCam = Camera.main;
        if (playCam == null)
        {
            ShowOverlayWithHandHold(Vector2.zero);
            return;
        }

        Vector2 screenPos = playCam.WorldToScreenPoint(firstSnake.transform.position);

        if (TryGetHandAnchoredPosFromScreenPoint(screenPos, out Vector2 anchoredPosFromSnake))
        {
            ShowOverlayWithHandHold(anchoredPosFromSnake);
        }
        else
        {
            ShowOverlayWithHandHold(Vector2.zero);
        }
    }

    private void PlayGateTutorial()
    {
        if (instructionText != null) instructionText.text = "Nhấn nút để mở các cửa cùng màu";
        if (useCustomGatePosition)
        {
            ShowOverlayWithHandTap(customGateAnchoredPos);
            return;
        }

        if (TryGetHandAnchoredPosFromTargetRect(gateTarget, out Vector2 anchoredPosFromTarget))
        {
            ShowOverlayWithHandTap(anchoredPosFromTarget);
            return;
        }

        ShowOverlayWithHandTap(Vector2.zero);
    }

    private void PlayPortalTutorial()
    {
        if (instructionText != null) instructionText.text = "Mũi tên vào hố đen sẽ ra ở cổng cùng màu, theo hướng của cổng ra";
        if (useCustomPortalPosition)
        {
            ShowOverlayWithHandTap(customPortalAnchoredPos);
            return;
        }

        if (TryGetHandAnchoredPosFromTargetRect(portalTarget, out Vector2 anchoredPosFromTarget))
        {
            ShowOverlayWithHandTap(anchoredPosFromTarget);
            return;
        }

        ShowOverlayWithHandTap(Vector2.zero);
    }

    private Vector2 GetSwipeVector(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: return Vector2.up;
            case ArrowDir.Down: return Vector2.down;
            case ArrowDir.Left: return Vector2.left;
            case ArrowDir.Right: return Vector2.right;
            default: return Vector2.up;
        }
    }

    public void CompleteTutorial()
    {
        if (!_isTutorialActive) return;

        _isTutorialActive = false;
        _isWaitingForIntro = false;
        _pendingLevelData = null;
        _tutorialRoutine = null;
        CameraController.IsCameraInputBlocked = false;
        if (handPointer != null) handPointer.DOKill();
        
        if (tutorialCanvasGroup == null) return;

        tutorialCanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
        {
            tutorialCanvasGroup.blocksRaycasts = false;
            if (handPointer != null) handPointer.gameObject.SetActive(false);
            if (instructionText != null) instructionText.gameObject.SetActive(false);
        });
    }

    // Called from gameplay input when the player taps an arrow for the first time.
    public void NotifyFirstArrowPressed()
    {
        if (!_isTutorialActive) return;
        if (_completedByFirstPress) return;

        _completedByFirstPress = true;
        CompleteTutorial();
    }
}