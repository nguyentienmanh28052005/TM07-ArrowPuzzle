using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class PopText : MonoBehaviour
{
    [SerializeField] Text txtVisual;
    [SerializeField] Canvas canvas;
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
    public void Initialize(string msg,int layer = 160)
    {
        canvas.sortingOrder = layer;
        txtVisual.SetValue(msg);
        txtVisual.rectTransform.anchoredPosition = Vector2.zero;
        Sequence sequence = DOTween.Sequence().SetId(this);
        sequence.Insert(0,txtVisual.rectTransform.DOAnchorPosY(300,1.2f).SetEase(Ease.InQuart));
        sequence.Insert(0,txtVisual.DOFadeAllShadow(1,.3f).SetEase(Ease.OutQuart));
        sequence.Insert(.9f,txtVisual.DOFadeAllShadow(0,.3f).SetEase(Ease.InQuart));
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
