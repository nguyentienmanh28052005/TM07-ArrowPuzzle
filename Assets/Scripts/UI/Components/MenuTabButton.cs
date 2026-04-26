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

    private void Awake()
    {
        if (iconContainer != null) _originalY = iconContainer.anchoredPosition.y;
    }

    public void SetSelected(bool isSelected, bool instant = false)
    {
        if (iconContainer == null) return;

        float targetY = isSelected ? _originalY + selectedYOffset : _originalY;
        float targetScale = isSelected ? selectedScale : 1f;
        float targetAlpha = isSelected ? 1f : 0f;

        // Dọn dẹp Tween để không bị xung đột nếu bấm liên tục
        iconContainer.DOKill();
        if (textGroup != null) textGroup.DOKill();

        // THAY ĐỔI TRẠNG THÁI GAMEOBJECT TRỰC TIẾP
        if (unselectedObject != null) unselectedObject.SetActive(!isSelected);
        if (selectedObject != null) selectedObject.SetActive(isSelected);

        if (instant)
        {
            iconContainer.anchoredPosition = new Vector2(iconContainer.anchoredPosition.x, targetY);
            iconContainer.localScale = Vector3.one * targetScale;
            if (textGroup != null) textGroup.alpha = targetAlpha;
        }
        else
        {
            // Icon Container nảy lên
            iconContainer.DOAnchorPosY(targetY, animDuration).SetEase(Ease.OutBack);
            iconContainer.DOScale(targetScale, animDuration).SetEase(Ease.OutBack);
            
            // Hiện/Ẩn chữ
            if (textGroup != null) textGroup.DOFade(targetAlpha, animDuration * 0.8f);
        }
    }

    public void PlaySelectAnimation()
    {
        // Hàm này gọi khi tab vừa được click vào.
        // Gợi ý: Nếu trong selectedObject có Particle System, có thể GetComponent và gọi .Play() ở đây.
    }
}