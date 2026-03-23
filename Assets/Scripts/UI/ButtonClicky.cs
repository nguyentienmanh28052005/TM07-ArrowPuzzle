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
    private bool _isPressed = false;
    private bool _isHovered = false;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _originalScale = transform.localScale;
        
        // ĐÃ XÓA dòng ép lấy sprite mặc định ở đây để tôn trọng quyết định để trống của bạn
        
        UpdateVisualState();
    }

    public void SetInteractable(bool state)
    {
        interactable = state;
        UpdateVisualState();
    }

    // ==========================================
    // CƠ CHẾ ẨN HIỆN & ĐỔ MÀU (CẬP NHẬT MỚI)
    // ==========================================
    private void SetSpriteDisplay(Sprite spriteToDisplay, bool isDisabledState = false)
    {
        if (spriteToDisplay == null)
        {
            // KHÔNG CÓ HÌNH -> Tàng hình hoàn toàn (Alpha = 0)
            _image.sprite = null;
            _image.color = new Color(1f, 1f, 1f, 0f); 
        }
        else
        {
            // CÓ HÌNH -> Hiện hình (Alpha = 1) và đổ màu xám nếu nút bị khóa
            _image.sprite = spriteToDisplay;
            _image.color = isDisabledState ? Color.gray : Color.white;
        }
    }

    private void UpdateVisualState()
    {
        if (!interactable)
        {
            transform.DOKill();
            transform.localScale = _originalScale;
            
            // Fallback: Ưu tiên dùng DisabledSprite, nếu để trống thì lấy DefaultSprite làm xám
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
        
        // Fallback: Ưu tiên dùng PressedSprite, nếu để trống thì giữ nguyên DefaultSprite
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