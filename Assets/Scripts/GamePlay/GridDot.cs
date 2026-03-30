using UnityEngine;
using System.Collections.Generic;
using DG.Tweening; 

public class GridDot : MonoBehaviour
{
    public static Dictionary<Vector2Int, GridDot> GridMap = new Dictionary<Vector2Int, GridDot>();

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Color originalColor;
    
    private bool _isWinning = false; 

    /// <summary>
    /// Lấy tham chiếu Component và lưu lại các thông số kích thước, màu sắc ban đầu.
    /// </summary>
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        if (spriteRenderer != null) originalColor = spriteRenderer.color; 
    }

    /// <summary>
    /// Đăng ký tọa độ của Dot vào hệ thống lưới toàn cục và reset trạng thái an toàn.
    /// </summary>
    void OnEnable()
    {
        _isWinning = false;
        
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        GridMap[pos] = this;
        
        transform.localScale = originalScale;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    /// <summary>
    /// Dọn dẹp Animation và gỡ đăng ký khỏi hệ thống lưới khi bị vô hiệu hóa.
    /// </summary>
    void OnDisable()
    {
        transform.DOKill();
        if (spriteRenderer != null) spriteRenderer.DOKill();

        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        if (GridMap.ContainsKey(pos) && GridMap[pos] == this)
        {
            GridMap.Remove(pos);
        }
    }

    /// <summary>
    /// Kích hoạt chuỗi hiệu ứng sóng Domino kết liễu bàn cờ khi người chơi giành chiến thắng.
    /// </summary>
    public void PlayWinAnimation(Color targetColor, float delay, float scaleAmount, float duration)
    {
        _isWinning = true;

        transform.DOKill();
        if (spriteRenderer != null) spriteRenderer.DOKill();

        transform.localScale = originalScale;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        float halfDuration = duration / 2f;

        Sequence winSeq = DOTween.Sequence();
        
        if (delay > 0) winSeq.AppendInterval(delay);

        winSeq.Append(transform.DOScale(originalScale * scaleAmount, halfDuration).SetEase(Ease.OutQuad));
        winSeq.Append(transform.DOScale(Vector3.zero, halfDuration).SetEase(Ease.InBack));
        winSeq.Join(spriteRenderer.DOColor(targetColor, halfDuration)); 
        
        winSeq.SetLink(gameObject); 
    }

    /// <summary>
    /// Kích hoạt hiệu ứng đàn hồi (Yoyo) khi có một con rắn trượt ngang qua nốt này.
    /// </summary>
    public void PlayLeaveEffect(float scaleAmount = 2f, float totalDuration = 0.4f)
    {
        if ((GameManager.Instance != null && GameManager.Instance.isGameOver) || _isWinning) return;

        transform.DOKill();
        if (spriteRenderer != null) spriteRenderer.DOKill();

        transform.localScale = originalScale;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        float halfDuration = totalDuration / 2f;

        transform.DOScale(originalScale * scaleAmount, halfDuration)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo) 
            .SetLink(gameObject);

        if (spriteRenderer != null)
        {
            spriteRenderer.DOColor(Color.white, halfDuration)
                .SetEase(Ease.OutQuad)
                .SetLoops(2, LoopType.Yoyo)
                .SetLink(gameObject);
        }
    }
}