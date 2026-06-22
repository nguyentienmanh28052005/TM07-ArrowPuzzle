using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartFly : ItemFlyBase
{
    [SerializeField] Image icon;
    public override void Initialize()
    {
        base.Initialize();
        Color c = icon.color;
        c.a = 0;
        icon.color = c;
        transform.localScale = Vector3.zero;
    }
    public override Tween AnimAppear()
    {
        Sequence sequence = DOTween.Sequence().SetId(this);
        sequence.Append(icon.DOFade(1, .35f).SetId(this));
        sequence.Join(transform.DOScale(1, .35f));
        return sequence;
    }
    public override Tween MoveTo(Vector3 destination, float time)
    {
        Sequence sequence = DOTween.Sequence().SetId(this);
        Vector3 direction = destination - transform.position;
        direction = direction.normalized;

        sequence.Append(transform.DOMove(destination, .35f).SetId(this).SetEase(Ease.InQuad));
        sequence.Insert(0.05f, transform.DOScale(1.3f, .1f).SetId(this).SetEase(Ease.OutQuad));
        sequence.Insert(.2f, transform.DOScale(1, .15f).SetId(this).SetEase(Ease.OutQuad));
        return sequence;
    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
