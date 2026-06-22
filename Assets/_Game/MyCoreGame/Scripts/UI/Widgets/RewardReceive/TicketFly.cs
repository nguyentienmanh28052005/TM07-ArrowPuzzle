using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicketFly : ItemFlyBase
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
    public override void SetUpLayer(int layer)
    {

    }
    public override Tween AnimAppear()
    {
        Sequence sequence = DOTween.Sequence().SetId(this);
        sequence.Append(icon.DOFade(1, .35f).SetId(this));
        sequence.Join(transform.DOScale(1,.35f));
        return sequence;
    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
