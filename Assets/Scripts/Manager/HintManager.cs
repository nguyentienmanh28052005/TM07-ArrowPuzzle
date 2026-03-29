using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Solo.MOST_IN_ONE;

public class HintManager : MonoBehaviour
{
    public static HintManager Instance;

    [Header("Hint Settings")]
    public Color hintGlowColor = Color.yellow;
    [SerializeField] private float hintDuration = 2f;
    
    private bool _isHinting = false;
    private Sequence _currentHintSeq; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TriggerHint()
    {
        if (_isHinting || Time.timeScale == 0f) return; 
        
        // Nếu có các tool cản trở click, hãy mở khóa các dòng if này:
        // if (EraseManager.Instance != null && EraseManager.Instance.IsExecutingErase) return;

        SnakeBlock targetToHint = FindBestMove();

        if (targetToHint != null)
        {
            PlayHintAnimation(targetToHint);
        }
        else
        {
            Debug.Log("<color=red>HINT: Không tìm thấy nước đi hợp lệ. Bàn cờ kẹt cứng!</color>");
        }
    }

    /// <summary>
    /// Thuật toán tìm đường hoàn toàn mới sử dụng Grid Logic thay vì BoxCast Vật lý.
    /// </summary>
    private SnakeBlock FindBestMove()
    {
        SnakeBlock[] allSnakes = FindObjectsOfType<SnakeBlock>();

        foreach (var snake in allSnakes)
        {
            if (snake == null || snake.IsMoving || snake.bodySegments.Count == 0) continue;

            Vector3 moveDir = GetDirVector(snake.direction);
            Vector2Int headPos = new Vector2Int(Mathf.RoundToInt(snake.bodySegments[0].position.x), Mathf.RoundToInt(snake.bodySegments[0].position.y));
            Vector2Int step = new Vector2Int(Mathf.RoundToInt(moveDir.x), Mathf.RoundToInt(moveDir.y));

            bool isBlocked = false;

            // Quét tối đa 50 ô trên Grid (Logic giống hệ thống di chuyển của rắn)
            for (int d = 1; d < 50; d++)
            {
                Vector2Int checkPos = headPos + (step * d);
                
                // Tràn viền map nghĩa là lối thoát thông thoáng
                if (Mathf.Abs(checkPos.x) > 100 || Mathf.Abs(checkPos.y) > 100) 
                {
                    break;
                }

                SnakeBlock obstacle = GridManager.Instance.GetSnakeAt(checkPos);
                
                if (obstacle != null && obstacle != snake)
                {
                    isBlocked = true; 
                    break; // Gặp vật cản -> Bỏ qua con rắn này
                }
            }

            // Nếu vòng for chạy xong mà không bị block -> Con rắn này có thể thoát!
            if (!isBlocked) return snake;
        }
        
        return null;
    }

    private void PlayHintAnimation(SnakeBlock snake)
    {
        _isHinting = true;
        
        // Tùy thuộc vào việc bạn còn dùng ArrowGuideline hay không
        // var guideline = snake.GetComponent<ArrowGuideline>();
        
        Color originalColor = snake.snakeColor;

        if (_currentHintSeq != null && _currentHintSeq.IsActive()) _currentHintSeq.Kill();

        _currentHintSeq = DOTween.Sequence();
        
        // _currentHintSeq.AppendCallback(() => { if (guideline != null) guideline.SetLineActive(true); });

        // Tween đổi màu nịnh mắt
        _currentHintSeq.Append(DOVirtual.Color(originalColor, hintGlowColor, 0.2f, (c) => snake.SetColorImmediate(c)).SetEase(Ease.InOutSine));
        _currentHintSeq.Append(DOVirtual.Color(hintGlowColor, originalColor, 0.2f, (c) => snake.SetColorImmediate(c)).SetEase(Ease.InOutSine));
        
        int loopCount = Mathf.RoundToInt(hintDuration / 0.4f);
        _currentHintSeq.SetLoops(loopCount);
        
        _currentHintSeq.OnKill(() => {
            _isHinting = false; 
            if (snake != null)
            {
                // if (guideline != null) guideline.SetLineActive(false);
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