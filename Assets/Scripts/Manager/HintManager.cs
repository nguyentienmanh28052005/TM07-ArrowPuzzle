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
        }
    }

    private SnakeBlock FindBestMove()
    {
        SnakeBlock[] allSnakes = FindObjectsOfType<SnakeBlock>();

        foreach (var snake in allSnakes)
        {
            // ĐÃ SỬA: Check LogicNodes.Count thay vì bodySegments.Count
            if (snake == null || snake.IsMoving || snake.LogicNodes == null || snake.LogicNodes.Count == 0) continue;

            Vector3 moveDir = GetDirVector(snake.direction);
            
            // ĐÃ SỬA: Lấy HeadPosition cực lẹ
            Vector2Int headPos = new Vector2Int(Mathf.RoundToInt(snake.HeadPosition.x), Mathf.RoundToInt(snake.HeadPosition.y));
            Vector2Int step = new Vector2Int(Mathf.RoundToInt(moveDir.x), Mathf.RoundToInt(moveDir.y));

            bool isBlocked = false;

            for (int d = 1; d < 50; d++)
            {
                Vector2Int checkPos = headPos + (step * d);
                
                if (Mathf.Abs(checkPos.x) > 100 || Mathf.Abs(checkPos.y) > 100) 
                {
                    break;
                }

                SnakeBlock obstacle = GridManager.Instance.GetSnakeAt(checkPos);
                
                if (obstacle != null && obstacle != snake)
                {
                    isBlocked = true; 
                    break; 
                }
            }

            if (!isBlocked) return snake;
        }
        
        return null;
    }

    private void PlayHintAnimation(SnakeBlock snake)
    {
        _isHinting = true;
        
        var guideline = snake.GetComponent<ArrowGuideline>();
        Color originalColor = snake.snakeColor;

        if (_currentHintSeq != null && _currentHintSeq.IsActive()) _currentHintSeq.Kill();

        _currentHintSeq = DOTween.Sequence();
        
        _currentHintSeq.AppendCallback(() => { if (guideline != null) guideline.SetLineActive(true); });

        _currentHintSeq.Append(DOVirtual.Color(originalColor, hintGlowColor, 0.2f, (c) => snake.SetColorImmediate(c)).SetEase(Ease.InOutSine));
        _currentHintSeq.Append(DOVirtual.Color(hintGlowColor, originalColor, 0.2f, (c) => snake.SetColorImmediate(c)).SetEase(Ease.InOutSine));
        
        int loopCount = Mathf.RoundToInt(hintDuration / 0.4f);
        _currentHintSeq.SetLoops(loopCount);
        
        _currentHintSeq.OnKill(() => {
            _isHinting = false; 
            if (snake != null)
            {
                if (guideline != null) guideline.SetLineActive(false);
                snake.SetColorImmediate(originalColor);
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