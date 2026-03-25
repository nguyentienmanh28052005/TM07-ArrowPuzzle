using UnityEngine;
using DG.Tweening;

public class MenuTabButton : MonoBehaviour
{
    [SerializeField] private RectTransform iconRoot; 

    [Header("Animation Settings")]
    [SerializeField] private float punchScaleAmount = 0.2f; 
    [SerializeField] private float duration = 0.3f;        

    /// <summary>
    /// Kích hoạt hoạt ảnh nảy (Punch) khi Tab này được chọn trên Menu.
    /// </summary>
    public void PlaySelectAnimation()
    {
        if (iconRoot == null) return;

        iconRoot.DOKill();
        iconRoot.localScale = Vector3.one; 

        iconRoot.DOPunchScale(Vector3.one * punchScaleAmount, duration, 10, 1)
            .SetUpdate(true); 
    }
}