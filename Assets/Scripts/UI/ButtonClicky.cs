using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class ButtonClicky : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("State")]
    public bool interactable = true;
    public UnityEvent onClick;

    [Header("Visual Settings")]
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _pressedSprite;
    [SerializeField] private Sprite _disabledSprite;
    
    [Header("Animation Settings")]
    [SerializeField] private float _hoverScale = 1.05f;
    [SerializeField] private float _clickScale = 0.9f;
    [SerializeField] private float _tweenDuration = 0.15f;
    [SerializeField] private Ease _tweenEase = Ease.OutQuad;

    private Image _image;
    private Vector3 _originalScale;
    private Color _originalColor; // THÊM BIẾN NÀY ĐỂ LƯU MÀU GỐC
    private bool _isPressed = false;
    private bool _isHovered = false;

    /// <summary>
    /// Lưu trữ tỷ lệ và MÀU SẮC gốc từ Inspector.
    /// </summary>
    private void Awake()
    {
        _image = GetComponent<Image>();
        _originalScale = transform.localScale;
        
        // BẮT BUỘC: Lưu lại cái màu mà bạn đã chỉnh trên component Image
        _originalColor = _image.color; 
        
        UpdateVisualState();
    }

    /// <summary>
    /// Kích hoạt hoặc vô hiệu hóa nút bấm.
    /// </summary>
    public void SetInteractable(bool state)
    {
        interactable = state;
        UpdateVisualState();
    }

    /// <summary>
    /// Cho phép các script khác (như LevelEditor) đổi màu của nút này.
    /// </summary>
    public void SetColor(Color newColor)
    {
        _originalColor = newColor;
        UpdateVisualState();
    }

    /// <summary>
    /// Xử lý logic hiển thị Sprite và Color dựa trên trạng thái tương tác.
    /// </summary>
    private void SetSpriteDisplay(Sprite spriteToDisplay, bool isDisabledState = false)
    {
        if (spriteToDisplay == null)
        {
            _image.sprite = null;
            // Giữ nguyên RGB của màu gốc, chỉ chọc thủng kênh Alpha về 0
            Color transparentColor = _originalColor;
            transparentColor.a = 0f;
            _image.color = transparentColor; 
        }
        else
        {
            _image.sprite = spriteToDisplay;
            // SỬA LỖI Ở ĐÂY: Trả lại màu gốc thay vì ép thành Color.white
            // Nếu nút bị disable, nhân màu gốc với màu xám để tạo độ tối tự nhiên
            _image.color = isDisabledState ? (_originalColor * Color.gray) : _originalColor;
        }
    }

    private void UpdateVisualState()
    {
        if (!interactable)
        {
            transform.DOKill();
            transform.localScale = _originalScale;
            
            Sprite targetSprite = _disabledSprite != null ? _disabledSprite : _defaultSprite;
            SetSpriteDisplay(targetSprite, true);
        }
        else
        {
            SetSpriteDisplay(_defaultSprite, false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable) return;
        _isHovered = true;
        
        transform.DOKill();
        transform.DOScale(_originalScale * _hoverScale, _tweenDuration).SetEase(_tweenEase).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!interactable) return;
        _isHovered = false;
        _isPressed = false;
        
        SetSpriteDisplay(_defaultSprite, false);
        
        transform.DOKill();
        transform.DOScale(_originalScale, _tweenDuration).SetEase(_tweenEase).SetUpdate(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable) return;
        _isPressed = true;
        
        Sprite targetSprite = _pressedSprite != null ? _pressedSprite : _defaultSprite;
        SetSpriteDisplay(targetSprite, false);
        
        transform.DOKill();
        transform.DOScale(_originalScale * _clickScale, _tweenDuration).SetEase(_tweenEase).SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactable) return;
        _isPressed = false;
        
        SetSpriteDisplay(_defaultSprite, false);
        
        transform.DOKill();
        float targetScale = _isHovered ? _hoverScale : 1f;
        transform.DOScale(_originalScale * targetScale, _tweenDuration).SetEase(_tweenEase).SetUpdate(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable) return;
        
        AudioManager.Instance.PlaySfx(AudioManager.Instance.btnClick, 1f);
        onClick?.Invoke();
    }
}