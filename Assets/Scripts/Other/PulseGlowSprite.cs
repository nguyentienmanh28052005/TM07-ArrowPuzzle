using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class PulseGlowSprite : MonoBehaviour
{
    [Header("Pulse Settings")]
    public bool enableScalePulse = true;
    public float scalePulseAmount = 1.15f;
    public float pulseDuration = 2f;

    [Header("Blink Settings")]
    public bool enableAlphaBlink = true;
    [Range(0f, 1f)] public float blinkMinAlpha = 0.4f;
    [Range(0f, 1f)] public float blinkMaxAlpha = 1f;
    public float blinkDuration = 2f;

    private SpriteRenderer _glowSprite;

    private void Awake()
    {
        _glowSprite = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        transform.localScale = Vector3.one;

        if(_glowSprite != null)
        {
            Color c = _glowSprite.color;
            c.a = blinkMaxAlpha;
            _glowSprite.color = c;
        }

        if (enableScalePulse)
        {
            transform.DOScale(scalePulseAmount, pulseDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }

        if (enableAlphaBlink && _glowSprite != null)
        {
            _glowSprite.DOFade(blinkMinAlpha, blinkDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }
    }

    private void OnDisable()
    {
        transform.DOKill();
        if(_glowSprite != null) _glowSprite.DOKill();
    }

    private void OnDestroy()
    {
        transform.DOKill();
        if(_glowSprite != null) _glowSprite.DOKill();
    }
}