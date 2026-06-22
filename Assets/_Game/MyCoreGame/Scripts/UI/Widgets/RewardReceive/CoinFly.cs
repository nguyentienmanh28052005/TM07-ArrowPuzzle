using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CoinFly : ItemFlyBase
{
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRendererCoin;
    [SerializeField] SpriteRenderer spriteRendererShadow;
    [SerializeField] SortingGroup sortingGroup;
    public override void Initialize()
    {
        base.Initialize();
        SetAlpha(0);
        transform.localScale = Vector3.zero;
        spriteRendererShadow.transform.localPosition = new Vector3(0.5f, -.5f, 0);
    }
    public override Tween AnimAppear()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(FadeSprites(spriteRendererCoin, 1, .35f).OnStart(() =>
        {
            animator.Play("Coin");
        }));
        sequence.Join(transform.DOScale(1, .35f));
        sequence.Join(FadeSprites(spriteRendererShadow, .25f, .25f));
        return sequence;
    }
    public void SetAlpha(float alpha)
    {

        Color color = spriteRendererCoin.color;
        color.a = alpha;
        spriteRendererCoin.color = color;
        spriteRendererShadow.color = new Color(0,0,0,alpha);

    }
    public Tween MixShadow(float time)
    {
        return spriteRendererShadow.transform.DOLocalMove(Vector3.zero, time);
    }
    public Tween FadeSprites(SpriteRenderer spriteRenderer,float targetAlpha, float duration)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(spriteRenderer.DOFade(targetAlpha, duration).SetEase(Ease.Linear));

        return sequence;

    }
    public override Tween MoveTo(Vector3 destination, float time)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(base.MoveTo(destination, time).SetEase(Ease.InOutQuart));
        sequence.Join(MixShadow(time).SetEase(Ease.InOutQuart));
        return sequence;
    }
    public override void SetUpLayer(int sortingOrder)
    {
        sortingGroup.sortingOrder = sortingOrder;
        base.SetUpLayer(sortingOrder);
    }

}
