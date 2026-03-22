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
    public UnityEvent onClick; // Sự kiện xuất ra ngoài để gắn hàm (giống Button gốc của Unity)

    [Header("Visual Settings")]
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _pressedSprite;
    [SerializeField] private Sprite _disabledSprite; // Hình ảnh khi nút bị khóa
    
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
        
        if (_defaultSprite == null) _defaultSprite = _image.sprite;
        
        UpdateVisualState();
    }

    // Hàm public để khóa/mở nút qua code
    public void SetInteractable(bool state)
    {
        interactable = state;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (!interactable)
        {
            if (_disabledSprite != null) _image.sprite = _disabledSprite;
            else _image.color = Color.gray; // Làm xám nếu không có hình disabled
            
            transform.DOKill();
            transform.localScale = _originalScale;
        }
        else
        {
            _image.color = Color.white;
            _image.sprite = _defaultSprite;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable) return;
        _isHovered = true;
        
        // DOKill là BẮT BUỘC để hủy tween cũ đang chạy dở
        transform.DOKill();
        transform.DOScale(_originalScale * _hoverScale, _tweenDuration)
            .SetEase(_tweenEase)
            .SetUpdate(true); // SetUpdate(true) giúp UI vẫn chạy animation khi Pause Game (Time.timeScale = 0)
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!interactable) return;
        _isHovered = false;
        _isPressed = false;
        
        _image.sprite = _defaultSprite;
        transform.DOKill();
        transform.DOScale(_originalScale, _tweenDuration).SetEase(_tweenEase).SetUpdate(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable) return;
        _isPressed = true;
        
        if (_pressedSprite != null) _image.sprite = _pressedSprite;
        
        transform.DOKill();
        transform.DOScale(_originalScale * _clickScale, _tweenDuration).SetEase(_tweenEase).SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactable) return;
        _isPressed = false;
        
        _image.sprite = _defaultSprite;
        
        transform.DOKill();
        // Trả về hoverScale nếu chuột vẫn đang nằm trên nút, ngược lại trả về originalScale
        float targetScale = _isHovered ? _hoverScale : 1f;
        transform.DOScale(_originalScale * targetScale, _tweenDuration).SetEase(_tweenEase).SetUpdate(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable) return;
        
        // Gọi âm thanh và rung ở đây (Tích hợp hệ thống của bạn)
        // AudioManager.Instance.PlaySfx(...);
        // MOST_HapticFeedback.Generate(...);
        
        // Kích hoạt sự kiện thực tế
        AudioManager.Instance.PlaySfx(AudioManager.Instance.btnClick, 1f);
        onClick?.Invoke();
    }
}