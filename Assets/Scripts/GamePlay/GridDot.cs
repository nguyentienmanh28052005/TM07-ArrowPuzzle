using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridDot : MonoBehaviour
{
    // BÍ QUYẾT TỐI ƯU O(1): Cuốn sổ đăng ký tọa độ của tất cả các Dot trên map
    public static Dictionary<Vector2Int, GridDot> GridMap = new Dictionary<Vector2Int, GridDot>();

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Color originalColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        
        // Lưu lại màu nguyên thủy của Dot để lát nữa phục hồi
        originalColor = spriteRenderer.color; 
    }

    void OnEnable()
    {
        // Ghi danh vào sổ khi được bật lên
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        GridMap[pos] = this;
    }

    void OnDisable()
    {
        // Xóa tên khỏi sổ khi bị tắt đi để chống lỗi Null
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        if (GridMap.ContainsKey(pos) && GridMap[pos] == this)
        {
            GridMap.Remove(pos);
        }
    }

    public void PlayWinAnimation(Color targetColor, float delay, float scaleAmount, float duration)
    {
        StartCoroutine(CoWinAnimation(targetColor, delay, scaleAmount, duration));
    }

    private IEnumerator CoWinAnimation(Color targetColor, float delay, float scaleAmount, float totalDuration)
    {
        if (delay > 0) yield return new WaitForSeconds(delay);

        float halfDuration = totalDuration / 2f;
        float elapsed = 0f;
        Color startColor = spriteRenderer.color;
        Vector3 targetScaleVec = originalScale * scaleAmount;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            transform.localScale = Vector3.Lerp(originalScale, targetScaleVec, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            transform.localScale = Vector3.Lerp(targetScaleVec, Vector3.zero, t);
            spriteRenderer.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        transform.localScale = Vector3.zero;
        spriteRenderer.color = targetColor;
    }

    // --- LOGIC MỚI: HIỆU ỨNG KHI ĐUÔI RỜI KHỎI (TRAIL EFFECT) ---
    public void PlayLeaveEffect(float scaleAmount = 1.8f, float totalDuration = 0.4f)
    {
        // Phải ngắt các hiệu ứng đang chạy lỡ dở để không bị xung đột co giật hình ảnh
        StopAllCoroutines(); 
        transform.localScale = originalScale;
        spriteRenderer.color = originalColor;
        
        StartCoroutine(CoLeaveEffect(scaleAmount, totalDuration));
    }

    private IEnumerator CoLeaveEffect(float scaleAmount, float totalDuration)
    {
        float halfDuration = totalDuration / 2f;
        float elapsed = 0f;
        
        Vector3 targetScaleVec = originalScale * scaleAmount;
        Color targetColor = Color.white; // Sáng bừng lên màu trắng

        // Pha 1: To dần ra và sáng lên màu trắng
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            transform.localScale = Vector3.Lerp(originalScale, targetScaleVec, t);
            spriteRenderer.color = Color.Lerp(originalColor, targetColor, t);
            yield return null;
        }

        // Pha 2: Nhỏ lại về kích thước ban đầu và trả lại màu ban đầu
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            transform.localScale = Vector3.Lerp(targetScaleVec, originalScale, t);
            spriteRenderer.color = Color.Lerp(targetColor, originalColor, t);
            yield return null;
        }

        // Khóa chốt an toàn ở frame cuối cùng
        transform.localScale = originalScale;
        spriteRenderer.color = originalColor;
    }
}