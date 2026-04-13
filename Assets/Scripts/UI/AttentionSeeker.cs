using UnityEngine;
using DG.Tweening;

public class AttentionSeeker : MonoBehaviour
{
    public enum AnimationStyle
    {
        Pulse,      // Phóng to thu nhỏ (Như nhịp tim)
        Wobble,     // Lắc lư trái phải (Như chuông gõ)
        Float       // Bay lên lơ lửng rồi hạ xuống
    }

    [Header("Settings")]
    public AnimationStyle style = AnimationStyle.Pulse;
    public float duration = 0.8f;      // Thời gian hoàn thành 1 nhịp
    public float power = 1.1f;         // Độ to/Độ nghiêng/Độ cao
    public float startDelay = 0f;      // Độ trễ trước khi bắt đầu (Tránh các nút giật cùng lúc)

    private Tween _currentTween;
    private Vector3 _originalScale;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        
        // Lưu lại trạng thái gốc để Reset
        _originalScale = _rectTransform.localScale;
        _originalPosition = _rectTransform.anchoredPosition;
        _originalRotation = _rectTransform.localRotation;
    }

    private void OnEnable()
    {
        PlayAnimation();
    }

    private void OnDisable()
    {
        StopAndReset();
    }

    public void PlayAnimation()
    {
        // Luôn kill tween cũ trước khi tạo tween mới để chống giật lag
        _currentTween?.Kill();

        switch (style)
        {
            case AnimationStyle.Pulse:
                _currentTween = _rectTransform.DOScale(_originalScale * power, duration)
                    .SetDelay(startDelay)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true); // Chạy ngay cả khi Time.timeScale = 0 (Pause game)
                break;

            case AnimationStyle.Wobble:
                _currentTween = _rectTransform.DOLocalRotate(new Vector3(0, 0, power), duration)
                    .SetDelay(startDelay)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);
                break;

            case AnimationStyle.Float:
                _currentTween = _rectTransform.DOAnchorPosY(_originalPosition.y + power, duration)
                    .SetDelay(startDelay)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);
                break;
        }
    }

    public void StopAndReset()
    {
        _currentTween?.Kill();
        
        // Trả UI về nguyên trạng thái ban đầu
        _rectTransform.localScale = _originalScale;
        _rectTransform.anchoredPosition = _originalPosition;
        _rectTransform.localRotation = _originalRotation;
    }
}