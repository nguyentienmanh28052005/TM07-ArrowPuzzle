using UnityEngine;
using DG.Tweening;

// Script này gắn vào cùng GameObject với ButtonClicky.cs trên từng cái nút Home/Shop/Setting
public class MenuTabButton : MonoBehaviour
{
    // Kéo cái GameObject "Icon" (child) vào đây
    [SerializeField] private RectTransform iconRoot; 

    [Header("Animation Settings")]
    [SerializeField] private float punchScaleAmount = 0.2f; // Độ phình ra khi được chọn
    [SerializeField] private float duration = 0.3f;        // Thời gian animation

    // Hàm này sẽ được GameMenuCanvas gọi khi Slider trượt tới nút này
    public void PlaySelectAnimation()
    {
        if (iconRoot == null) return;

        // BẮT BUỘC: Dừng animation cũ để tránh lỗi spam click
        iconRoot.DOKill();
        
        // Reset lại scale về chuẩn
        iconRoot.localScale = Vector3.one; 

        // Hiệu ứng "Punch": phình ra rồi thu lại (giống nhịp tim)
        // Dùng trục Vector3.one để phình đều cả X và Y
        iconRoot.DOPunchScale(Vector3.one * punchScaleAmount, duration, 10, 1)
            .SetUpdate(true); // SetUpdate(true) để chạy ngay cả khi game Pause
    }
}