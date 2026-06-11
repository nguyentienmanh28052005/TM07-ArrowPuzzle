using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Solo.MOST_IN_ONE;
using TMPro;

public class HintManager : MonoBehaviour
{
    public static HintManager Instance;

    [Header("Hint Settings")]
    public Color hintGlowColor = Color.yellow;
    [SerializeField] private float hintDuration = 2f;
    [SerializeField] private float hintGlowScale = 1.18f;
    [SerializeField] private float hintGlowPulseDuration = 0.4f;
    [SerializeField] private float hintGlowRestoreDuration = 0.15f;

    [Header("Hint Camera")]
    [SerializeField] private bool moveCameraToHintTarget = true;
    [SerializeField] private float hintCameraMoveDuration = 0.35f;
    
    private bool _isHinting = false;
    private Sequence _currentHintSeq; 

    [SerializeField] private TextMeshProUGUI hintCountText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TriggerHint()
    {
        if (_isHinting || Time.timeScale == 0f) return; 
        if (BoosterTutorialManager.Instance != null &&
            (BoosterTutorialManager.Instance.IsWaitingForBoosterRewardClaim || !BoosterTutorialManager.Instance.IsHintUnlocked)) return;
        
        if (EraseManager.Instance != null && EraseManager.Instance.IsExecutingErase) return;

        if(CurrencyManager.Instance.SpendHintTool(1))
        {
            SnakeBlock bestMoveSnake = FindBestMove();
            if (bestMoveSnake != null)
            {
                PlayHintAnimation(bestMoveSnake);
            }
            else
            {
                Debug.Log("HINT: Không tìm thấy nước đi nào tốt hơn!");
            }

            if (BoosterTutorialManager.Instance != null)
                BoosterTutorialManager.Instance.NotifyHintTriggered();
        }
    }

    private SnakeBlock FindBestMove()
    {
        BoardState board = BoardState.Active;
        if (board == null || !board.IsValid) return null;

        IReadOnlyList<SnakeBlock> allSnakes = SnakeBlock.ActiveSnakes;
        for (int i = 0; i < allSnakes.Count; i++)
        {
            SnakeBlock snake = allSnakes[i];
            if (snake == null || snake.IsMoving || snake.IsStoppedByStopBlock || snake.LogicNodes == null || snake.LogicNodes.Count == 0) continue;

            MoveResult result = PathScanner.Scan(
                board,
                snake,
                PathScanner.ToGridCell(snake.HeadPosition),
                snake.direction,
                50,
                stopAtBlockers: true,
                boundaryAbs: 100);

            if (result.CanRelease) return snake;
        }
        
        return null;
    }
    private void PlayHintAnimation(SnakeBlock snake)
    {
        _isHinting = true;

        if (moveCameraToHintTarget)
        {
            CameraController camController = FindObjectOfType<CameraController>();
            if (camController != null)
            {
                camController.FocusOnWorldPosition(snake.HeadPosition, hintCameraMoveDuration, blockCameraInput: true);
            }
        }
        
        var guideline = snake.GetComponent<ArrowGuideline>();
        Color originalColor = snake.snakeColor;

        if (_currentHintSeq != null && _currentHintSeq.IsActive()) _currentHintSeq.Kill();

        float safePulseDuration = Mathf.Max(0.05f, hintGlowPulseDuration);
        float halfPulseDuration = safePulseDuration * 0.5f;
        int loopCount = Mathf.Max(1, Mathf.CeilToInt(hintDuration / safePulseDuration));

        if (guideline != null) guideline.SetLineActive(true);
        snake.BeginHintGlowVisual(hintGlowScale, halfPulseDuration);

        _currentHintSeq = DOTween.Sequence();

        _currentHintSeq.Append(DOVirtual.Color(originalColor, hintGlowColor, halfPulseDuration, (c) => snake.SetColorImmediate(c)).SetEase(Ease.InOutSine));
        _currentHintSeq.Append(DOVirtual.Color(hintGlowColor, originalColor, halfPulseDuration, (c) => snake.SetColorImmediate(c)).SetEase(Ease.InOutSine));

        _currentHintSeq.SetLoops(loopCount);
        
        _currentHintSeq.OnKill(() => {
            _isHinting = false; 
            if (snake != null)
            {
                if (guideline != null) guideline.SetLineActive(false);
                snake.EndHintGlowVisual(originalColor, hintGlowRestoreDuration);
            }
        });
        
        _currentHintSeq.SetLink(snake.gameObject);
    }

    public void StopHintImmediate()
    {
        if (_currentHintSeq != null && _currentHintSeq.IsActive())
        {
            _currentHintSeq.Kill();
        }
    }

    private Vector3 GetDirVector(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: return Vector3.up;
            case ArrowDir.Down: return Vector3.down;
            case ArrowDir.Left: return Vector3.left;
            case ArrowDir.Right: return Vector3.right;
            default: return Vector3.zero;
        }
    }
}
