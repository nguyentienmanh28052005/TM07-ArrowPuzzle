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
    public bool sfx;
    public UnityEvent onClick;

    [Header("Visual Settings")]
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _pressedSprite;
    [SerializeField] private Sprite _disabledSprite;
    [SerializeField] private GameObject _vfx;
    
    [Header("Animation Settings")]
    [SerializeField] private float _hoverScale = 1.05f;
    [SerializeField] private float _clickScale = 0.9f;
    [SerializeField] private float _tweenDuration = 0.15f;
    [SerializeField] private Ease _tweenEase = Ease.OutQuad;

    private Image _image;
    private Vector3 _originalScale;
    private Color _originalColor; 
    private bool _isPressed = false;
    private bool _isHovered = false;
    
    // Tích hợp đồng bộ với AttentionSeeker
    private AttentionSeeker _attentionSeeker; 

    private void Awake()
    {
        _image = GetComponent<Image>();
        _originalScale = transform.localScale;
        _originalColor = _image.color;
        
        // Tự động tìm xem nút này có gắn AttentionSeeker không
        _attentionSeeker = GetComponent<AttentionSeeker>(); 
        
        UpdateVisualState();
    }

    public void SetInteractable(bool state)
    {
        interactable = state;
        UpdateVisualState();
    }

    public void SetColor(Color newColor)
    {
        _originalColor = newColor;
        UpdateVisualState();
    }

    private void SetSpriteDisplay(Sprite spriteToDisplay, bool isDisabledState = false)
    {
        if (spriteToDisplay == null)
        {
            _image.sprite = null;
            Color transparentColor = _originalColor;
            transparentColor.a = 0f;
            _image.color = transparentColor; 
        }
        else
        {
            _image.sprite = spriteToDisplay;
            _image.color = isDisabledState ? (_originalColor * Color.gray) : _originalColor;
        }
    }

    private void UpdateVisualState()
    {
        if (!interactable)
        {
            if (_vfx != null) _vfx.SetActive(false);
            if (_attentionSeeker != null) _attentionSeeker.StopAndReset(); // Khóa nút thì dừng vẫy gọi

            transform.DOKill();
            transform.localScale = _originalScale;
            
            Sprite targetSprite = _disabledSprite != null ? _disabledSprite : _defaultSprite;
            SetSpriteDisplay(targetSprite, true);
        }
        else
        {
            SetSpriteDisplay(_defaultSprite, false);
            // Nếu nút được mở khóa lại, tiếp tục vẫy gọi
            if (_attentionSeeker != null && !gameObject.activeInHierarchy == false) 
            {
                _attentionSeeker.PlayAnimation();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable) return;
        _isHovered = true;
        
        // Vừa đưa chuột vào -> Bắt AttentionSeeker dừng lại
        if (_attentionSeeker != null) _attentionSeeker.StopAndReset(); 
        
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
        // Thu về kích thước gốc. Khi OnComplete (đã thu xong) -> Hồi sinh AttentionSeeker
        transform.DOScale(_originalScale, _tweenDuration).SetEase(_tweenEase).SetUpdate(true).OnComplete(() => {
            if (_attentionSeeker != null) _attentionSeeker.PlayAnimation();
        });
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable) return;
        _isPressed = true;
        
        // Bấm chuột xuống -> Bắt AttentionSeeker dừng lại
        if (_attentionSeeker != null) _attentionSeeker.StopAndReset(); 
        
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
        transform.DOScale(_originalScale * targetScale, _tweenDuration).SetEase(_tweenEase).SetUpdate(true).OnComplete(() => {
            // (Dành cho Mobile) Chạm xong nhả tay ra luôn (không còn Hover) -> Hồi sinh AttentionSeeker
            if (!_isHovered && _attentionSeeker != null) _attentionSeeker.PlayAnimation();
        });
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable) return;
        if (sfx && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(AudioManager.Instance.btnClick, 1f);
        }
        onClick?.Invoke();
    }
}