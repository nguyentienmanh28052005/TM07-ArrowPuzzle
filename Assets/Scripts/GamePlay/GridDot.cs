using UnityEngine;
using System.Collections.Generic;
using DG.Tweening; // BẮT BUỘC PHẢI CÓ

public class GridDot : MonoBehaviour
{
    public static Dictionary<Vector2Int, GridDot> GridMap = new Dictionary<Vector2Int, GridDot>();

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Color originalColor;
    
    // Cờ bảo vệ nội bộ: Nếu Dot này đã bắt đầu chuỗi Win, cấm mọi tương tác khác
    private bool _isWinning = false; 

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        if (spriteRenderer != null) originalColor = spriteRenderer.color; 
    }

    void OnEnable()
    {
        _isWinning = false;
        
        // Ghi danh vào sổ
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        GridMap[pos] = this;
        
        // Reset sạch sẽ trạng thái (Phòng trường hợp dùng Object Pooling)
        transform.localScale = originalScale;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    void OnDisable()
    {
        transform.DOKill();
        if (spriteRenderer != null) spriteRenderer.DOKill();

        // Xóa sổ
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        if (GridMap.ContainsKey(pos) && GridMap[pos] == this)
        {
            GridMap.Remove(pos);
        }
    }

    // ==========================================
    // 1. HIỆU ỨNG KHI WIN GAME (SÓNG DOMINO)
    // ==========================================
    public void PlayWinAnimation(Color targetColor, float delay, float scaleAmount, float duration)
    {
        _isWinning = true;

        // BÓP PHANH: Giết chết mọi animation "Đi qua" đang chạy dở
        transform.DOKill();
        if (spriteRenderer != null) spriteRenderer.DOKill();

        // QUAN TRỌNG NHẤT: Ép cục Dot thu về trạng thái gốc NGAY LẬP TỨC. 
        // Tránh tình trạng nó bị kẹt ở size khổng lồ trong lúc chờ biến 'delay'
        transform.localScale = originalScale;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        float halfDuration = duration / 2f;

        // Dùng Sequence ghép chuỗi animation cực kỳ gọn gàng
        Sequence winSeq = DOTween.Sequence();
        
        // 1. Chờ đến lượt (Domino Delay)
        if (delay > 0) winSeq.AppendInterval(delay);

        // 2. Phóng to ra (Pha 1)
        winSeq.Append(transform.DOScale(originalScale * scaleAmount, halfDuration).SetEase(Ease.OutQuad));
        
        // 3. Thu nhỏ về 0 và đổi màu (Pha 2)
        winSeq.Append(transform.DOScale(Vector3.zero, halfDuration).SetEase(Ease.InBack));
        winSeq.Join(spriteRenderer.DOColor(targetColor, halfDuration)); // Join chạy song song với Append trên
        
        // An toàn chống văng lỗi nếu Dot bị Destroy giữa chừng
        winSeq.SetLink(gameObject); 
    }

    // ==========================================
    // 2. HIỆU ỨNG KHI MŨI TÊN ĐI QUA (TRAIL EFFECT)
    // ==========================================
    public void PlayLeaveEffect(float scaleAmount = 2.5f, float totalDuration = 0.5f)
    {
        // BỨC TƯỜNG THÉP: Cấm chạy nếu Game đã kết thúc hoặc Dot này đang chạy Win
        if ((GameManager.Instance != null && GameManager.Instance.isGameOver) || _isWinning) return;

        transform.DOKill();
        if (spriteRenderer != null) spriteRenderer.DOKill();

        float halfDuration = totalDuration / 2f;

        Sequence leaveSeq = DOTween.Sequence();
        
        // Pha 1: To dần ra và sáng lên màu trắng
        leaveSeq.Append(transform.DOScale(originalScale * scaleAmount, halfDuration).SetEase(Ease.OutQuad));
        leaveSeq.Join(spriteRenderer.DOColor(Color.white, halfDuration));

        // Pha 2: Trở về như cũ
        leaveSeq.Append(transform.DOScale(originalScale, halfDuration).SetEase(Ease.InQuad));
        leaveSeq.Join(spriteRenderer.DOColor(originalColor, halfDuration));

        leaveSeq.SetLink(gameObject);
    }
}