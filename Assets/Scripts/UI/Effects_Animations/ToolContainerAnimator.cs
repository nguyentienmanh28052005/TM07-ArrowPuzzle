using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class ToolContainerAnimator : MonoBehaviour
{
    public enum SlideDirection
    {
        FromLeft,
        FromRight,
        FromTop,
        FromBottom,
        CustomOffset
    }

    [Header("Animation Direction")]
    [Tooltip("Chọn hướng mà UI này sẽ bay ra")]
    public SlideDirection direction = SlideDirection.FromLeft;
    
    [Tooltip("Khoảng cách bay (bằng pixel). Không dùng cho CustomOffset.")]
    public float slideDistance = 800f;
    
    [Tooltip("Chỉ hoạt động nếu chọn CustomOffset. Điền tọa độ khởi đầu tùy ý.")]
    public Vector2 customStartOffset;

    [Header("Timing & Easing")]
    public float delayTime = 1.5f; 
    public float slideDuration = 0.8f;
    public Ease slideEase = Ease.OutBack;

    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    
    private Vector2 _originalPos;
    private bool _isInitialized = false;

    private void Awake()
    {
        InitializeData();
    }

    private void InitializeData()
    {
        if (!_isInitialized)
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            
            // Lưu tọa độ "đích đến" (vị trí lúc thiết kế trên Scene)
            _originalPos = _rectTransform.anchoredPosition;
            _isInitialized = true;
        }
    }

    private void OnEnable()
    {
        InitializeData();

        // 1. Dọn dẹp Tween cũ
        _rectTransform.DOKill();
        _canvasGroup.DOKill();
        
        // 2. Tính toán vị trí xuất phát dựa trên Enum
        Vector2 startPos = _originalPos;
        switch (direction)
        {
            case SlideDirection.FromLeft:   startPos.x -= slideDistance; break;
            case SlideDirection.FromRight:  startPos.x += slideDistance; break;
            case SlideDirection.FromTop:    startPos.y += slideDistance; break;
            case SlideDirection.FromBottom: startPos.y -= slideDistance; break;
            case SlideDirection.CustomOffset: startPos += customStartOffset; break;
        }

        // 3. Khởi tạo trạng thái ban đầu (Giấu đi)
        _rectTransform.anchoredPosition = startPos;
        _canvasGroup.alpha = 0f;
        
        // Chặn người chơi click bậy bạ lúc UI đang tàng hình
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        // 4. Kích hoạt chuỗi Animation
        Sequence seq = DOTween.Sequence();
        seq.SetDelay(delayTime); 
        
        // Dùng DOAnchorPos để di chuyển đồng thời cả X và Y (hỗ trợ hướng chéo nếu dùng Custom)
        seq.Append(_rectTransform.DOAnchorPos(_originalPos, slideDuration).SetEase(slideEase));
        seq.Join(_canvasGroup.DOFade(1f, slideDuration * 0.8f).SetEase(Ease.InOutQuad));
        
        // Mở khóa tương tác khi Animation chạy xong
        seq.OnComplete(() => {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        });

        seq.SetLink(gameObject); 
    }
}