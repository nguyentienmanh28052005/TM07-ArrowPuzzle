using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupBreakAds : PopupUI
{
    public float timeBreakAds = 2;
    [SerializeField] SkeletonGraphic skeletonGraphic;
    [SerializeField] CanvasGroup canvasGroup;
    float timeCur;
    public override void Show(Action onClose)
    {
        base.Show(onClose);
        timeCur = timeBreakAds;
        skeletonGraphic.AnimationState.SetAnimation(0, "appear", false);
        canvasGroup.alpha = 0;
        skeletonGraphic.color = new Color(1,1,1,0);
        canvasGroup.DOFade(1, 1f).SetEase(Ease.Linear).SetId(this);
        skeletonGraphic.DOFade(1, 1f).SetEase(Ease.Linear).SetId(this);
    }
     
    public override void Hide()
    {
        //skeletonGraphic.AnimationState.SetAnimation(0, "end", false);
        //skeletonGraphic.DOFade(0, 1f).SetId(this).SetEase(Ease.Linear);
        //canvasGroup.DOFade(0, 1f).SetId(this).SetUpdate(true).SetEase(Ease.Linear).OnComplete(base.Hide);
        OnHide?.Invoke();
        OnHide = null;
        OnHideInPlace?.Invoke();
        OnHideInPlace = null;
        base.Hide();
    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
