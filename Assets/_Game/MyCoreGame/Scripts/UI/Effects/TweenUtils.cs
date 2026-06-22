using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class TweenUtils
{
    /// <summary>
    /// Scale nẩy như punch, nhưng cho phép set scale cuối.
    /// </summary>
    /// <param name="t">Transform</param>
    /// <param name="punchAmount">Độ phình lên thêm (ví dụ Vector3.one * 0.1f)</param>
    /// <param name="duration">Thời gian tổng punch (ví dụ 0.3f)</param>
    /// <param name="finalScale">Scale cuối cùng mong muốn</param>
    /// <returns>Sequence tween</returns>
    public static Sequence DOPunchScaleTo(this Transform t, Vector3 punchAmount, float duration, Vector3 finalScale)
    {
        var originalScale = t.localScale;
        var punchScale = originalScale + punchAmount;

        var seq = DOTween.Sequence();
        seq.Append(t.DOScale(punchScale, duration * 0.5f).SetEase(Ease.OutQuad));
        seq.Append(t.DOScale(finalScale, duration * 0.5f).SetEase(Ease.OutQuad));
        return seq;
    }
    
    public static Sequence DOPunchScaleTo(this Transform t, Vector3 punchAmount, float duration, float punchRate, Vector3 finalScale)
    {
        var originalScale = t.localScale;
        var punchScale = originalScale + punchAmount;

        var seq = DOTween.Sequence();
        seq.Append(t.DOScale(punchScale, duration * punchRate).SetEase(Ease.OutQuad));
        seq.Append(t.DOScale(finalScale, duration * (1 - punchRate)).SetEase(Ease.OutQuad));
        return seq;
    }
    public static Sequence DOPunchScaleToTarget(this Transform t, Vector3 punchAmount, float duration, Vector3 finalScale)
    {
        var punchScale = punchAmount;

        var seq = DOTween.Sequence();
        seq.Append(t.DOScale(punchScale, duration * 0.5f).SetEase(Ease.Linear));
        seq.Append(t.DOScale(finalScale, duration * 0.5f).SetEase(Ease.Linear));
        return seq;
    }
    
    public static Tweener DOFade(this Shadow shadow, float targetAlpha, float duration)
    {
        if (shadow == null) return null;

        float startAlpha = shadow.effectColor.a;

        return DOTween.To(
            () => startAlpha,
            x =>
            {
                startAlpha = x;
                Color c = shadow.effectColor;
                c.a = startAlpha;
                shadow.effectColor = c;
            },
            targetAlpha,
            duration
        ).SetTarget(shadow);
    }
}

