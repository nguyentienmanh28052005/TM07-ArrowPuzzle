using UnityEngine;
using DG.Tweening;

public class AttentionSeeker : MonoBehaviour
{
    public enum AnimationStyle
    {
        Pulse,
        Wobble,
        Float,
        Heartbeat,
        Squish,
        Shake,
        Pop,
        JumpShake,
        RubberBand,
        Spin,
        Tada
    }

    [Header("Settings")]
    public AnimationStyle style = AnimationStyle.Pulse;
    public float duration = 0.8f;
    public float power = 1.1f;
    public float startDelay = 0f;

    private Tween _currentTween;
    private Vector3 _originalScale;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

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
        _currentTween?.Kill();

        switch (style)
        {
            case AnimationStyle.Pulse:
                _currentTween = _rectTransform.DOScale(_originalScale * power, duration)
                    .SetDelay(startDelay).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
                break;

            case AnimationStyle.Wobble:
                _currentTween = _rectTransform.DOLocalRotate(new Vector3(0, 0, power * 10f), duration)
                    .SetDelay(startDelay).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
                break;

            case AnimationStyle.Float:
                _currentTween = _rectTransform.DOAnchorPosY(_originalPosition.y + (power * 10f), duration)
                    .SetDelay(startDelay).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
                break;

            case AnimationStyle.Heartbeat:
                Sequence hbSeq = DOTween.Sequence();
                hbSeq.Append(_rectTransform.DOScale(_originalScale * power, duration * 0.2f).SetEase(Ease.OutQuad));
                hbSeq.Append(_rectTransform.DOScale(_originalScale, duration * 0.2f).SetEase(Ease.InQuad));
                hbSeq.Append(_rectTransform.DOScale(_originalScale * (power * 0.8f), duration * 0.15f).SetEase(Ease.OutQuad));
                hbSeq.Append(_rectTransform.DOScale(_originalScale, duration * 0.15f).SetEase(Ease.InQuad));
                hbSeq.AppendInterval(duration * 0.3f);
                hbSeq.SetDelay(startDelay).SetLoops(-1).SetUpdate(true);
                _currentTween = hbSeq;
                break;

            case AnimationStyle.Squish:
                _currentTween = _rectTransform.DOScale(new Vector3(_originalScale.x * power, _originalScale.y / power, _originalScale.z), duration)
                    .SetDelay(startDelay).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
                break;

            case AnimationStyle.Shake:
                Sequence shSeq = DOTween.Sequence();
                shSeq.Append(_rectTransform.DOAnchorPosX(_originalPosition.x + (power * 10f), duration * 0.1f));
                shSeq.Append(_rectTransform.DOAnchorPosX(_originalPosition.x - (power * 10f), duration * 0.1f));
                shSeq.Append(_rectTransform.DOAnchorPosX(_originalPosition.x + (power * 5f), duration * 0.1f));
                shSeq.Append(_rectTransform.DOAnchorPosX(_originalPosition.x - (power * 5f), duration * 0.1f));
                shSeq.Append(_rectTransform.DOAnchorPosX(_originalPosition.x, duration * 0.1f));
                shSeq.AppendInterval(duration * 0.5f);
                shSeq.SetDelay(startDelay).SetLoops(-1).SetUpdate(true);
                _currentTween = shSeq;
                break;

            case AnimationStyle.Pop:
                Sequence popSeq = DOTween.Sequence();
                popSeq.Append(_rectTransform.DOScale(_originalScale * power, duration * 0.3f).SetEase(Ease.OutBack));
                popSeq.Append(_rectTransform.DOScale(_originalScale, duration * 0.5f).SetEase(Ease.OutBounce));
                popSeq.AppendInterval(duration * 0.2f);
                popSeq.SetDelay(startDelay).SetLoops(-1).SetUpdate(true);
                _currentTween = popSeq;
                break;

            case AnimationStyle.JumpShake:
                Sequence jsSeq = DOTween.Sequence();
                
                jsSeq.Append(_rectTransform.DOAnchorPosY(_originalPosition.y + (power * 30f), duration * 0.25f).SetEase(Ease.OutQuad));
                jsSeq.Join(_rectTransform.DOScale(_originalScale * power, duration * 0.25f).SetEase(Ease.OutQuad));

                Sequence airShake = DOTween.Sequence();
                
                airShake.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, 15f), duration * 0.03f));
                airShake.Join(_rectTransform.DOAnchorPosX(_originalPosition.x - 8f, duration * 0.03f));

                airShake.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, -15f), duration * 0.06f));
                airShake.Join(_rectTransform.DOAnchorPosX(_originalPosition.x + 8f, duration * 0.06f));

                airShake.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, 15f), duration * 0.06f));
                airShake.Join(_rectTransform.DOAnchorPosX(_originalPosition.x - 8f, duration * 0.06f));

                airShake.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, -15f), duration * 0.06f));
                airShake.Join(_rectTransform.DOAnchorPosX(_originalPosition.x + 8f, duration * 0.06f));

                airShake.Append(_rectTransform.DOLocalRotate(Vector3.zero, duration * 0.03f));
                airShake.Join(_rectTransform.DOAnchorPosX(_originalPosition.x, duration * 0.03f));

                jsSeq.Append(airShake);

                jsSeq.Append(_rectTransform.DOAnchorPosY(_originalPosition.y, duration * 0.2f).SetEase(Ease.InQuad));
                jsSeq.Join(_rectTransform.DOScale(_originalScale, duration * 0.2f).SetEase(Ease.InQuad));
                
                jsSeq.AppendInterval(duration * 0.4f);
                jsSeq.SetDelay(startDelay).SetLoops(-1).SetUpdate(true);
                
                _currentTween = jsSeq;
                break;

            case AnimationStyle.Tada:
                Sequence tadaSeq = DOTween.Sequence();
                tadaSeq.Append(_rectTransform.DOScale(_originalScale * 0.9f, duration * 0.15f));
                tadaSeq.Join(_rectTransform.DOLocalRotate(new Vector3(0, 0, -5f), duration * 0.15f));
                tadaSeq.Append(_rectTransform.DOScale(_originalScale * power, duration * 0.15f));
                tadaSeq.Join(_rectTransform.DOLocalRotate(new Vector3(0, 0, 5f), duration * 0.15f));
                tadaSeq.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, -5f), duration * 0.1f).SetLoops(4, LoopType.Yoyo));
                tadaSeq.Append(_rectTransform.DOScale(_originalScale, duration * 0.2f).SetEase(Ease.OutBack));
                tadaSeq.Join(_rectTransform.DOLocalRotate(Vector3.zero, duration * 0.2f));
                tadaSeq.AppendInterval(duration * 0.5f);
                tadaSeq.SetDelay(startDelay).SetLoops(-1).SetUpdate(true);
                _currentTween = tadaSeq;
                break;

            case AnimationStyle.RubberBand:
                Sequence rbSeq = DOTween.Sequence();
                rbSeq.Append(_rectTransform.DOScale(new Vector3(_originalScale.x * 1.25f, _originalScale.y * 0.75f, _originalScale.z), duration * 0.15f));
                rbSeq.Append(_rectTransform.DOScale(new Vector3(_originalScale.x * 0.75f, _originalScale.y * 1.25f, _originalScale.z), duration * 0.15f));
                rbSeq.Append(_rectTransform.DOScale(new Vector3(_originalScale.x * 1.15f, _originalScale.y * 0.85f, _originalScale.z), duration * 0.15f));
                rbSeq.Append(_rectTransform.DOScale(new Vector3(_originalScale.x * 0.95f, _originalScale.y * 1.05f, _originalScale.z), duration * 0.15f));
                rbSeq.Append(_rectTransform.DOScale(_originalScale, duration * 0.1f));
                rbSeq.AppendInterval(duration * 0.5f);
                rbSeq.SetDelay(startDelay).SetLoops(-1).SetUpdate(true);
                _currentTween = rbSeq;
                break;

            case AnimationStyle.Spin:
                _currentTween = _rectTransform.DOLocalRotate(new Vector3(0, 0, -360f), duration, RotateMode.FastBeyond360)
                    .SetRelative(true)
                    .SetDelay(startDelay)
                    .SetEase(Ease.InOutBack)
                    .SetLoops(-1, LoopType.Restart)
                    .SetUpdate(true);
                break;
        }
    }

    public void StopAndReset()
    {
        _currentTween?.Kill();
        _rectTransform.localScale = _originalScale;
        _rectTransform.anchoredPosition = _originalPosition;
        _rectTransform.localRotation = _originalRotation;
    }
}