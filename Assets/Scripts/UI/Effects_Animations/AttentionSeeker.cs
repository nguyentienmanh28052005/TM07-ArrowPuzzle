using UnityEngine;
using DG.Tweening;

public class AttentionSeeker : MonoBehaviour
{
    public enum AnimationStyle
    {
        Pulse, Wobble, Float, Heartbeat, Squish, 
        Shake, Pop, JumpShake, RubberBand, Spin, Tada,
        Swing, Jello, BellRing, Breathe, Shiver, 
        Hiccup, ZoomWobble, StretchSnap, Pendulum, Dizzy
    }

    [Header("Settings")]
    public AnimationStyle style = AnimationStyle.Pulse;
    
    [Tooltip("Thời gian thực hiện HIỆU ỨNG (Giây)")]
    public float duration = 0.8f;
    public float power = 1.1f;
    
    [Tooltip("THỜI GIAN NGHỈ giữa 2 vòng lặp (Giây)")]
    public float loopDelay = 1.0f;

    [Header("Advanced Synchronization")]
    [Tooltip("Đồng bộ với đồng hồ hệ thống để các object nhảy múa đều nhau.")]
    public bool syncWithGlobalTime = true;
    [Tooltip("Nếu BẬT Sync: Đây là độ lệch nhịp (Nút B nhảy sau Nút A).\nNếu TẮT Sync: Đây là thời gian chờ trước khi bắt đầu hiệu ứng.")]
    public float startDelay = 0f;

    private Tween _currentTween;
    private Vector3 _originalScale;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private RectTransform _rectTransform;
    private bool _isInitialized = false;

    private void Awake()
    {
        InitData();
    }

    private void InitData()
    {
        if (_isInitialized) return;
        _rectTransform = GetComponent<RectTransform>();
        _originalScale = _rectTransform.localScale;
        _originalPosition = _rectTransform.anchoredPosition;
        _originalRotation = _rectTransform.localRotation;
        _isInitialized = true;
    }

    private void OnEnable() => PlayAnimation();
    
    private void OnDisable() => StopAndReset();

    public void PlayAnimation()
    {
        InitData();
        _currentTween?.Kill();
        _rectTransform.localScale = _originalScale;
        _rectTransform.anchoredPosition = _originalPosition;
        _rectTransform.localRotation = _originalRotation;

        float halfDuration = duration * 0.5f;
        Tween effectTween = null;

        // BƯỚC 1: Xây dựng 1 chu kỳ hiệu ứng dài CHÍNH XÁC "duration" giây
        switch (style)
        {
            case AnimationStyle.Pulse:
                Sequence pulseSeq = DOTween.Sequence();
                pulseSeq.Append(_rectTransform.DOScale(_originalScale * power, halfDuration).SetEase(Ease.InOutSine));
                pulseSeq.Append(_rectTransform.DOScale(_originalScale, halfDuration).SetEase(Ease.InOutSine));
                effectTween = pulseSeq;
                break;

            case AnimationStyle.Wobble:
                Sequence wobSeq = DOTween.Sequence();
                wobSeq.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, power * 10f), halfDuration).SetEase(Ease.InOutSine));
                wobSeq.Append(_rectTransform.DOLocalRotate(Vector3.zero, halfDuration).SetEase(Ease.InOutSine));
                effectTween = wobSeq;
                break;

            case AnimationStyle.Float:
                Sequence floatSeq = DOTween.Sequence();
                floatSeq.Append(_rectTransform.DOAnchorPosY(_originalPosition.y + (power * 10f), halfDuration).SetEase(Ease.InOutSine));
                floatSeq.Append(_rectTransform.DOAnchorPosY(_originalPosition.y, halfDuration).SetEase(Ease.InOutSine));
                effectTween = floatSeq;
                break;

            case AnimationStyle.Heartbeat:
                Sequence hbSeq = DOTween.Sequence();
                hbSeq.Append(_rectTransform.DOScale(_originalScale * power, duration * 0.2f).SetEase(Ease.OutQuad));
                hbSeq.Append(_rectTransform.DOScale(_originalScale, duration * 0.2f).SetEase(Ease.InQuad));
                hbSeq.Append(_rectTransform.DOScale(_originalScale * (power * 0.8f), duration * 0.15f).SetEase(Ease.OutQuad));
                hbSeq.Append(_rectTransform.DOScale(_originalScale, duration * 0.15f).SetEase(Ease.InQuad));
                hbSeq.AppendInterval(duration * 0.3f);
                effectTween = hbSeq;
                break;

            case AnimationStyle.Squish:
                Sequence squishSeq = DOTween.Sequence();
                squishSeq.Append(_rectTransform.DOScale(new Vector3(_originalScale.x * power, _originalScale.y / power, _originalScale.z), halfDuration).SetEase(Ease.InOutSine));
                squishSeq.Append(_rectTransform.DOScale(_originalScale, halfDuration).SetEase(Ease.InOutSine));
                effectTween = squishSeq;
                break;

            case AnimationStyle.Shake:
                Sequence shSeq = DOTween.Sequence();
                shSeq.Append(_rectTransform.DOAnchorPosX(_originalPosition.x + (power * 10f), duration * 0.1f));
                shSeq.Append(_rectTransform.DOAnchorPosX(_originalPosition.x - (power * 10f), duration * 0.1f));
                shSeq.Append(_rectTransform.DOAnchorPosX(_originalPosition.x + (power * 5f), duration * 0.1f));
                shSeq.Append(_rectTransform.DOAnchorPosX(_originalPosition.x - (power * 5f), duration * 0.1f));
                shSeq.Append(_rectTransform.DOAnchorPosX(_originalPosition.x, duration * 0.1f));
                shSeq.AppendInterval(duration * 0.5f);
                effectTween = shSeq;
                break;

            case AnimationStyle.Pop:
                Sequence popSeq = DOTween.Sequence();
                popSeq.Append(_rectTransform.DOScale(_originalScale * power, duration * 0.3f).SetEase(Ease.OutBack));
                popSeq.Append(_rectTransform.DOScale(_originalScale, duration * 0.5f).SetEase(Ease.OutBounce));
                popSeq.AppendInterval(duration * 0.2f);
                effectTween = popSeq;
                break;

            case AnimationStyle.JumpShake:
                Sequence jsSeq = DOTween.Sequence();
                jsSeq.Append(_rectTransform.DOAnchorPosY(_originalPosition.y + (power * 30f), duration * 0.25f).SetEase(Ease.OutQuad));
                jsSeq.Join(_rectTransform.DOScale(_originalScale * power, duration * 0.25f).SetEase(Ease.OutQuad));
                Sequence airShake = DOTween.Sequence();
                airShake.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, 15f), duration * 0.05f));
                airShake.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, -15f), duration * 0.05f));
                airShake.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, 15f), duration * 0.05f));
                airShake.Append(_rectTransform.DOLocalRotate(Vector3.zero, duration * 0.05f));
                jsSeq.Append(airShake);
                jsSeq.Append(_rectTransform.DOAnchorPosY(_originalPosition.y, duration * 0.2f).SetEase(Ease.InQuad));
                jsSeq.Join(_rectTransform.DOScale(_originalScale, duration * 0.2f).SetEase(Ease.InQuad));
                jsSeq.AppendInterval(duration * 0.35f);
                effectTween = jsSeq;
                break;

            case AnimationStyle.Tada:
                Sequence tadaSeq = DOTween.Sequence();
                tadaSeq.Append(_rectTransform.DOScale(_originalScale * 0.9f, duration * 0.15f));
                tadaSeq.Join(_rectTransform.DOLocalRotate(new Vector3(0, 0, -5f), duration * 0.15f));
                tadaSeq.Append(_rectTransform.DOScale(_originalScale * power, duration * 0.15f));
                tadaSeq.Join(_rectTransform.DOLocalRotate(new Vector3(0, 0, 5f), duration * 0.15f));
                Sequence wobble = DOTween.Sequence();
                wobble.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, -5f), duration * 0.05f));
                wobble.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, 5f), duration * 0.05f));
                wobble.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, -5f), duration * 0.05f));
                wobble.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, 5f), duration * 0.05f));
                tadaSeq.Append(wobble);
                tadaSeq.Append(_rectTransform.DOScale(_originalScale, duration * 0.2f).SetEase(Ease.OutBack));
                tadaSeq.Join(_rectTransform.DOLocalRotate(Vector3.zero, duration * 0.2f));
                tadaSeq.AppendInterval(duration * 0.3f);
                effectTween = tadaSeq;
                break;

            case AnimationStyle.RubberBand:
                Sequence rbSeq = DOTween.Sequence();
                rbSeq.Append(_rectTransform.DOScale(new Vector3(_originalScale.x * 1.25f, _originalScale.y * 0.75f, _originalScale.z), duration * 0.15f));
                rbSeq.Append(_rectTransform.DOScale(new Vector3(_originalScale.x * 0.75f, _originalScale.y * 1.25f, _originalScale.z), duration * 0.15f));
                rbSeq.Append(_rectTransform.DOScale(new Vector3(_originalScale.x * 1.15f, _originalScale.y * 0.85f, _originalScale.z), duration * 0.15f));
                rbSeq.Append(_rectTransform.DOScale(new Vector3(_originalScale.x * 0.95f, _originalScale.y * 1.05f, _originalScale.z), duration * 0.15f));
                rbSeq.Append(_rectTransform.DOScale(_originalScale, duration * 0.1f));
                rbSeq.AppendInterval(duration * 0.3f);
                effectTween = rbSeq;
                break;

            case AnimationStyle.Spin:
                effectTween = _rectTransform.DOLocalRotate(new Vector3(0, 0, -360f), duration, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.InOutBack);
                break;

            case AnimationStyle.Swing:
                Sequence swingSeq = DOTween.Sequence();
                swingSeq.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, power * 20f), halfDuration).SetEase(Ease.InOutSine));
                swingSeq.Append(_rectTransform.DOLocalRotate(Vector3.zero, halfDuration).SetEase(Ease.InOutSine));
                effectTween = swingSeq;
                break;

            case AnimationStyle.Jello:
                Sequence jlSeq = DOTween.Sequence();
                jlSeq.Append(_rectTransform.DOScale(new Vector3(_originalScale.x * 1.1f, _originalScale.y * 0.9f, _originalScale.z), duration * 0.1f));
                jlSeq.Append(_rectTransform.DOScale(new Vector3(_originalScale.x * 0.9f, _originalScale.y * 1.1f, _originalScale.z), duration * 0.1f));
                jlSeq.Append(_rectTransform.DOScale(new Vector3(_originalScale.x * 1.05f, _originalScale.y * 0.95f, _originalScale.z), duration * 0.1f));
                jlSeq.Append(_rectTransform.DOScale(new Vector3(_originalScale.x * 0.95f, _originalScale.y * 1.05f, _originalScale.z), duration * 0.1f));
                jlSeq.Append(_rectTransform.DOScale(_originalScale, duration * 0.1f));
                jlSeq.AppendInterval(duration * 0.5f);
                effectTween = jlSeq;
                break;

            case AnimationStyle.BellRing:
                Sequence brSeq = DOTween.Sequence();
                float rot = power * 25f;
                brSeq.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, rot), duration * 0.1f));
                brSeq.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, -rot * 0.8f), duration * 0.1f));
                brSeq.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, rot * 0.6f), duration * 0.1f));
                brSeq.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, -rot * 0.4f), duration * 0.1f));
                brSeq.Append(_rectTransform.DOLocalRotate(Vector3.zero, duration * 0.1f));
                brSeq.AppendInterval(duration * 0.5f);
                effectTween = brSeq;
                break;

            case AnimationStyle.Breathe:
                Sequence brthSeq = DOTween.Sequence();
                brthSeq.Append(_rectTransform.DOScale(_originalScale * 1.04f, halfDuration).SetEase(Ease.InOutSine));
                brthSeq.Append(_rectTransform.DOScale(_originalScale, halfDuration).SetEase(Ease.InOutSine));
                effectTween = brthSeq;
                break;

            case AnimationStyle.Shiver:
                effectTween = _rectTransform.DOShakeAnchorPos(duration, power * 5f, 30, 90f, false, true);
                break;

            case AnimationStyle.Hiccup:
                Sequence hicSeq = DOTween.Sequence();
                hicSeq.Append(_rectTransform.DOAnchorPosY(_originalPosition.y + 15f, duration * 0.1f).SetEase(Ease.OutQuint));
                hicSeq.Join(_rectTransform.DOScale(new Vector3(_originalScale.x * 1.1f, _originalScale.y * 0.9f, 1f), duration * 0.1f));
                hicSeq.Append(_rectTransform.DOAnchorPosY(_originalPosition.y, duration * 0.3f).SetEase(Ease.OutBounce));
                hicSeq.Join(_rectTransform.DOScale(_originalScale, duration * 0.3f).SetEase(Ease.OutBounce));
                hicSeq.AppendInterval(duration * 0.6f);
                effectTween = hicSeq;
                break;

            case AnimationStyle.ZoomWobble:
                Sequence zwSeq = DOTween.Sequence();
                zwSeq.Append(_rectTransform.DOScale(_originalScale * power, duration * 0.2f));
                zwSeq.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, 10f), duration * 0.1f));
                zwSeq.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, -10f), duration * 0.1f));
                zwSeq.Append(_rectTransform.DOLocalRotate(Vector3.zero, duration * 0.1f));
                zwSeq.Append(_rectTransform.DOScale(_originalScale, duration * 0.3f).SetEase(Ease.InBack));
                zwSeq.AppendInterval(duration * 0.2f);
                effectTween = zwSeq;
                break;

            case AnimationStyle.StretchSnap:
                Sequence snapSeq = DOTween.Sequence();
                snapSeq.Append(_rectTransform.DOScale(new Vector3(_originalScale.x * 0.6f, _originalScale.y * 1.5f, _originalScale.z), duration * 0.4f).SetEase(Ease.InExpo));
                snapSeq.Append(_rectTransform.DOScale(_originalScale, duration * 0.6f).SetEase(Ease.OutElastic));
                effectTween = snapSeq;
                break;

            case AnimationStyle.Pendulum:
                Sequence penSeq = DOTween.Sequence();
                penSeq.Append(_rectTransform.DOAnchorPosX(_originalPosition.x + (power * 30f), duration * 0.25f).SetEase(Ease.OutSine));
                penSeq.Join(_rectTransform.DOLocalRotate(new Vector3(0, 0, -15f), duration * 0.25f).SetEase(Ease.OutSine));
                penSeq.Append(_rectTransform.DOAnchorPosX(_originalPosition.x - (power * 30f), halfDuration).SetEase(Ease.InOutSine));
                penSeq.Join(_rectTransform.DOLocalRotate(new Vector3(0, 0, 15f), halfDuration).SetEase(Ease.InOutSine));
                penSeq.Append(_rectTransform.DOAnchorPosX(_originalPosition.x, duration * 0.25f).SetEase(Ease.InSine));
                penSeq.Join(_rectTransform.DOLocalRotate(Vector3.zero, duration * 0.25f).SetEase(Ease.InSine));
                effectTween = penSeq;
                break;

            case AnimationStyle.Dizzy:
                Sequence dizzySeq = DOTween.Sequence();
                dizzySeq.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, 360f), duration, RotateMode.FastBeyond360).SetEase(Ease.Linear));
                Sequence dizzyScale = DOTween.Sequence();
                dizzyScale.Append(_rectTransform.DOScale(_originalScale * power, halfDuration).SetEase(Ease.InOutSine));
                dizzyScale.Append(_rectTransform.DOScale(_originalScale, halfDuration).SetEase(Ease.InOutSine));
                dizzySeq.Join(dizzyScale);
                effectTween = dizzySeq;
                break;
        }

        // BƯỚC 2: Gói vào Master Sequence và tính toán đồng bộ pha
        if (effectTween != null)
        {
            Sequence masterSeq = DOTween.Sequence();
            masterSeq.Append(effectTween); 
            
            if (loopDelay > 0f)
            {
                masterSeq.AppendInterval(loopDelay); 
            }

            masterSeq.SetLoops(-1); 
            masterSeq.SetUpdate(true);
            _currentTween = masterSeq;

            if (syncWithGlobalTime)
            {
                _currentTween.Play(); 
                float totalCycleTime = duration + Mathf.Max(0f, loopDelay);
                
                // Thuật toán chống lỗi số âm khi startDelay lớn hơn Time.unscaledTime
                float offsetTime = Time.unscaledTime - startDelay;
                float playheadPosition = offsetTime % totalCycleTime;
                if (playheadPosition < 0) 
                {
                    playheadPosition += totalCycleTime;
                }
                
                _currentTween.Goto(playheadPosition, true);
            }
            else
            {
                // Hành vi mặc định nếu không bật đồng bộ
                if (startDelay > 0f)
                {
                    _currentTween.SetDelay(startDelay);
                }
            }
        }
    }

    public void StopAndReset()
    {
        InitData();
        _currentTween?.Kill();
        _rectTransform.localScale = _originalScale;
        _rectTransform.anchoredPosition = _originalPosition;
        _rectTransform.localRotation = _originalRotation;
    }
}