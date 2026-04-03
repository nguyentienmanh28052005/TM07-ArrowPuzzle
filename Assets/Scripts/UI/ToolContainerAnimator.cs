using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))] // Tự động gắn thêm CanvasGroup để làm mờ
public class ToolContainerAnimator : MonoBehaviour
{
    [Header("Slide In Settings")]
    public float delayTime = 1.5f; 
    public float slideDuration = 0.8f;
    public float offscreenOffset = -800f; 

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    
    private Vector2 originalPos;
    private bool isInitialized = false;

    private void Awake()
    {
        InitializeData();
    }

    private void InitializeData()
    {
        if (!isInitialized)
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            
            // LƯU TỌA ĐỘ ĐÚNG 1 LẦN DUY NHẤT LÚC ĐANG Ở VỊ TRÍ ĐẸP
            originalPos = rectTransform.anchoredPosition;
            isInitialized = true;
        }
    }

    // OnEnable đảm bảo cứ mỗi lần bật HUD (hoặc qua màn) là hiệu ứng sẽ chạy lại
    private void OnEnable()
    {
        InitializeData();

        // 1. NGẮT MỌI ANIMATION CŨ (Chống lỗi spam click)
        rectTransform.DOKill();
        canvasGroup.DOKill();
        
        // 2. GIẤU CỤM NÚT ĐI: Đẩy ra ngoài rìa và làm mờ tịt (Alpha = 0)
        rectTransform.anchoredPosition = new Vector2(originalPos.x + offscreenOffset, originalPos.y);
        canvasGroup.alpha = 0f;

        // 3. KÍCH HOẠT CHUỖI ANIMATION JUICY
        Sequence seq = DOTween.Sequence();
        seq.SetDelay(delayTime); // Chờ chữ bay xong
        
        // Trượt lố qua rồi nảy lại (OutBack)
        seq.Append(rectTransform.DOAnchorPosX(originalPos.x, slideDuration).SetEase(Ease.OutBack));
        
        // Cùng lúc đó sáng rực lên
        seq.Join(canvasGroup.DOFade(1f, slideDuration * 0.8f).SetEase(Ease.InOutQuad));
        
        seq.SetLink(gameObject); // Chống lỗi rác bộ nhớ khi xóa Object
    }
}