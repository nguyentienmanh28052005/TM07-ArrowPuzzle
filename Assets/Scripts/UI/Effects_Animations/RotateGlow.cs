using UnityEngine;
using DG.Tweening;
using UnityEngine.UI; // BẮT BUỘC phải có để điều khiển độ mờ của Image TMP

[RequireComponent(typeof(Image))]
public class RotateGlow : MonoBehaviour
{
    [Header("Rotation Settings (Xoay)")]
    [Tooltip("Thời gian (giây) để quay hết 1 vòng 360 độ. Càng nhỏ xoay càng nhanh.")]
    public float rotationDuration = 10f; 
    public bool clockwise = true;

    [Header("Pulse Settings (Nhịp đập Scale)")]
    public bool enableScalePulse = true;
    [Tooltip("Độ phình to tối đa của vòng sáng")]
    public float scalePulseAmount = 1.15f;
    [Tooltip("Thời gian cho 1 nhịp phình to/thu nhỏ")]
    public float pulseDuration = 2f;

    [Header("Blink Settings (Nhấp nháy mờ sáng dần)")]
    public bool enableAlphaBlink = true;
    [Tooltip("Độ trong suốt khi mờ nhất (0: tàng hình, 1: sáng rõ)")]
    [Range(0f, 1f)] public float blinkMinAlpha = 0.4f; 
    [Tooltip("Độ trong suốt khi sáng nhất")]
    [Range(0f, 1f)] public float blinkMaxAlpha = 1f;   
    [Tooltip("Thời gian cho 1 nhịp mờ dần/sáng dần")]
    public float blinkDuration = 2f; 

    private Image _glowImage;

    private void Awake()
    {
        _glowImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        // 1. Reset trạng thái gốc mỗi khi Popup hiện lên
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        if(_glowImage != null)
        {
            Color c = _glowImage.color;
            // Ép màu hào quang về trạng thái sáng nhất lúc ban đầu
            c.a = blinkMaxAlpha; 
            _glowImage.color = c;
        }

        // 2. TẠO CHUYỂN ĐỘNG XOAY VÔ TẬN
        float direction = clockwise ? -360f : 360f;
        transform.DORotate(new Vector3(0, 0, direction), rotationDuration, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart) // Loop vô hạn
            .SetRelative(true)              // Xoay tương đối
            .SetEase(Ease.Linear)           // Xoay đều, không bị khựng (Quan trọng)
            .SetUpdate(true);               // Chạy bất chấp timeScale = 0

        // 3. TẠO HIỆU ỨNG NHỊP THỞ SCALE
        if (enableScalePulse)
        {
            transform.DOScale(scalePulseAmount, pulseDuration)
                .SetLoops(-1, LoopType.Yoyo) // Phình ra -> Xẹp lại vô hạn
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }

        // 4. MỚI: TẠO HIỆU ỨNG NHẤP NHÁY MỜ SÁNG DẦN (ALPHA BLINK)
        if (enableAlphaBlink && _glowImage != null)
        {
            // DOTween DOFade logic on the Image component
            _glowImage.DOFade(blinkMinAlpha, blinkDuration)
                .SetLoops(-1, LoopType.Yoyo) // Sáng rõ -> Mờ dần -> Sáng rõ vô hạn
                .SetEase(Ease.InOutSine)      // Chuyển mờ mượt mà
                .SetUpdate(true);            // Chạy bất chấp timeScale = 0
        }
    }

    private void OnDisable()
    {
        // Giải phóng Tween của cả Transform và Image
        transform.DOKill();
        if(_glowImage != null) _glowImage.DOKill(); 
    }

    private void OnDestroy()
    {
        transform.DOKill();
        if(_glowImage != null) _glowImage.DOKill();
    }
}