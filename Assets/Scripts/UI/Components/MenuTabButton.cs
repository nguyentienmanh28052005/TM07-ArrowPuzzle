using UnityEngine;
using DG.Tweening;

public class MenuTabButton : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Object Cha bao bọc Icon. Chính nó sẽ thực hiện lệnh nảy lên/thu nhỏ.")]
    public RectTransform iconContainer; 
    [Tooltip("Cục Text tên tab (chỉ hiện khi được chọn)")]
    public CanvasGroup textGroup;  

    [Header("State Objects (GameObjects)")]
    [Tooltip("Kéo Object hiển thị khi KHÔNG ĐƯỢC CHỌN vào đây")]
    public GameObject unselectedObject;
    [Tooltip("Kéo Object hiển thị khi ĐƯỢC CHỌN (có viền sáng, hiệu ứng...) vào đây")]
    public GameObject selectedObject;

    [Header("Settings")]
    public float selectedYOffset = 25f; // Độ cao bật lên khi chọn
    public float selectedScale = 1.15f;  // Phóng to icon
    public float animDuration = 0.3f;
    
    private float _originalY;
    private Vector3 _baseIconScale = Vector3.one;
    private Vector3 _baseUnselectedScale = Vector3.one;
    private Vector3 _baseSelectedScale = Vector3.one;

    private void Awake()
    {
        if (iconContainer != null)
        {
            _originalY = iconContainer.anchoredPosition.y;
            _baseIconScale = iconContainer.localScale;
        }

        if (unselectedObject != null)
        {
            _baseUnselectedScale = unselectedObject.transform.localScale;
        }

        if (selectedObject != null)
        {
            _baseSelectedScale = selectedObject.transform.localScale;
        }
    }

    public void SetSelected(bool isSelected, bool instant = false)
    {
        if (iconContainer == null) return;

        float targetY = isSelected ? _originalY + selectedYOffset : _originalY;
        float targetAlpha = isSelected ? 1f : 0f;
        bool hasStateObjects = unselectedObject != null || selectedObject != null;
        Vector3 selectedTargetScale = new Vector3(
            _baseSelectedScale.x * selectedScale,
            _baseSelectedScale.y * selectedScale,
            _baseSelectedScale.z * selectedScale);

        // Dọn dẹp Tween để không bị xung đột nếu bấm liên tục
        iconContainer.DOKill();
        if (textGroup != null) textGroup.DOKill();
        if (unselectedObject != null) unselectedObject.transform.DOKill();
        if (selectedObject != null) selectedObject.transform.DOKill();

        if (instant)
        {
            iconContainer.anchoredPosition = new Vector2(iconContainer.anchoredPosition.x, targetY);
            iconContainer.localScale = hasStateObjects
                ? _baseIconScale
                : _baseIconScale * (isSelected ? selectedScale : 1f);

            if (textGroup != null) textGroup.alpha = targetAlpha;

            if (hasStateObjects)
            {
                ApplyIconStateInstant(isSelected, selectedTargetScale);
            }

            return;
        }

        // Icon Container nảy lên
        iconContainer.DOAnchorPosY(targetY, animDuration).SetEase(Ease.OutBack);

        if (hasStateObjects)
        {
            AnimateIconState(isSelected, selectedTargetScale);
        }
        else
        {
            iconContainer.DOScale(_baseIconScale * (isSelected ? selectedScale : 1f), animDuration).SetEase(Ease.OutBack);
        }

        // Hiện/Ẩn chữ
        if (textGroup != null)
        {
            textGroup.DOFade(targetAlpha, animDuration * 0.8f);
        }
    }

    private void ApplyIconStateInstant(bool isSelected, Vector3 selectedTargetScale)
    {
        if (unselectedObject != null)
        {
            unselectedObject.transform.localScale = _baseUnselectedScale;
            unselectedObject.SetActive(!isSelected);
        }

        if (selectedObject != null)
        {
            selectedObject.transform.localScale = isSelected ? selectedTargetScale : _baseSelectedScale;
            selectedObject.SetActive(isSelected);
        }
    }

    private void AnimateIconState(bool isSelected, Vector3 selectedTargetScale)
    {
        if (isSelected)
        {
            // 1. KHI ĐƯỢC CHỌN: TẮT NGAY LẬP TỨC unselectedObject
            if (unselectedObject != null)
            {
                unselectedObject.SetActive(false);
                unselectedObject.transform.localScale = _baseUnselectedScale;
            }

            // 2. Bật và nảy selectedObject từ 0 -> bự
            if (selectedObject != null)
            {
                Transform selectedTransform = selectedObject.transform;
                selectedObject.SetActive(true);
                selectedTransform.localScale = Vector3.zero;
                selectedTransform.DOScale(selectedTargetScale, animDuration).SetEase(Ease.OutBack);
            }
        }
        else
        {
            // KHI BỎ CHỌN: CHỈ CHẠY ANIMATION NẾU NÓ ĐANG ĐƯỢC CHỌN (TRÁNH LỖI SPAM)
            if (selectedObject != null && selectedObject.activeSelf)
            {
                Transform selectedTransform = selectedObject.transform;
                selectedTransform.localScale = selectedTargetScale;
                
                // Chạy Tween thu nhỏ
                selectedTransform.DOScale(Vector3.zero, animDuration * 0.5f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    if (selectedObject == null) return; 
                    
                    selectedObject.SetActive(false);
                    selectedTransform.localScale = _baseSelectedScale;

                    // Sau khi thu nhỏ xong, bật unselectedObject lên và cho nảy
                    if (unselectedObject != null)
                    {
                        unselectedObject.SetActive(true);
                        unselectedObject.transform.localScale = Vector3.zero;
                        unselectedObject.transform.DOScale(_baseUnselectedScale, 0.1f * 0.5f);
                    }
                });
            }
            else 
            {
                // NẾU NÓ ĐÃ TẮT SẴN RỒI: ÉP TRẠNG THÁI TĨNH (KHÔNG ANIMATION)
                if (selectedObject != null)
                {
                    selectedObject.SetActive(false);
                    selectedObject.transform.localScale = _baseSelectedScale;
                }

                if (unselectedObject != null)
                {
                    unselectedObject.SetActive(true);
                    unselectedObject.transform.localScale = _baseUnselectedScale;
                }
            }
        }
    }

    public void PlaySelectAnimation()
    {
    }
}