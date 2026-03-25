using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Solo.MOST_IN_ONE;

public class HintManager : MonoBehaviour
{
    public static HintManager Instance;

    [Header("Hint Settings")]
    [Tooltip("Layer chứa các vật cản (Tường, Mũi tên khác)")]
    public LayerMask obstacleLayer;
    
    [Tooltip("Màu sắc khi mũi tên nhấp nháy gợi ý")]
    public Color hintGlowColor = Color.yellow;

    [Tooltip("Thời gian tồn tại của một gợi ý (giây)")]
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
        if (_isHinting) return; 
        if (Time.timeScale == 0f) return; 
        
        // BỌC THÉP: Cấm bật Hint khi Cục tẩy đang chạy làm nhiệm vụ
        if (EraseManager.Instance != null && EraseManager.Instance.IsExecutingErase) return;

        SnakeBlock targetToHint = FindBestMove();

        if (targetToHint != null)
        {
            PlayHintAnimation(targetToHint);
        }
        else
        {
            Debug.Log("Không tìm thấy nước đi hợp lệ. Bàn cờ kẹt cứng!");
        }
    }

    private SnakeBlock FindBestMove()
    {
        SnakeBlock[] allSnakes = FindObjectsOfType<SnakeBlock>();

        foreach (var snake in allSnakes)
        {
            if (snake == null || snake.IsMoving || snake.bodySegments.Count == 0) continue;

            Vector3 headPos = snake.bodySegments[0].position;
            Vector3 moveDir = GetDirVector(snake.direction);

            Vector2 boxSize = new Vector2(0.8f, 0.8f);
            RaycastHit2D[] hits = Physics2D.BoxCastAll(headPos, boxSize, 0f, moveDir, 100f, obstacleLayer);

            bool isBlocked = false;
            
            foreach (var hit in hits)
            {
                if (hit.collider != null && !IsColliderBelongToSnake(hit.collider, snake))
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
        
        _currentHintSeq.AppendCallback(() => {
            if (guideline != null) guideline.SetLineActive(true);
        });

        _currentHintSeq.Append(DOVirtual.Color(originalColor, hintGlowColor, 0.2f, (c) => snake.SetColorImmediatePublic(c)).SetEase(Ease.InOutSine));
        _currentHintSeq.Append(DOVirtual.Color(hintGlowColor, originalColor, 0.2f, (c) => snake.SetColorImmediatePublic(c)).SetEase(Ease.InOutSine));
        
        int loopCount = Mathf.RoundToInt(hintDuration / 0.4f);
        _currentHintSeq.SetLoops(loopCount);
        
        _currentHintSeq.OnKill(() => {
            _isHinting = false; 

            if (snake != null)
            {
                if (guideline != null) guideline.SetLineActive(false);
                snake.SetColorImmediatePublic(originalColor);
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

    private bool IsColliderBelongToSnake(Collider2D col, SnakeBlock snake)
    {
        Collider2D[] myColliders = snake.GetComponentsInChildren<Collider2D>();
        foreach (var c in myColliders)
        {
            if (c == col) return true;
        }
        return false;
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