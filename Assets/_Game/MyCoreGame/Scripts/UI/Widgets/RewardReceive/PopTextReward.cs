using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PopTextReward : MonoBehaviour
{
    [SerializeField] Text txtVisual;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Image icon;
    [SerializeField] RectTransform canvasGroupRectF;
    private RectTransform rectTransform;
    public RectTransform RectTF
    {
        get
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
            return rectTransform;
        }
    }
    public void Initialize(DataResource dataResource)
    {
        txtVisual.SetValue($"+{dataResource.amount}");
        canvasGroupRectF.anchoredPosition = Vector2.zero;
        Sequence sequence = DOTween.Sequence().SetId(this);
        sequence.Insert(0, canvasGroupRectF.DOAnchorPosY(300, .9f).SetEase(Ease.Linear));
        sequence.Insert(0, icon.DOFade(1, .25f).SetEase(Ease.OutQuart));
        sequence.Insert(0, txtVisual.DOFadeAllShadow(1, .25f).SetEase(Ease.OutQuart));
        sequence.Insert(.65f, icon.DOFade(0, .25f).SetEase(Ease.InQuart));
        sequence.Insert(.65f, txtVisual.DOFadeAllShadow(0, .25f).SetEase(Ease.InQuart));
        sequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
