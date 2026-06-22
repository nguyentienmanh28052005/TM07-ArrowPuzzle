using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class RewardUI : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] VisualResGeneral visualRes;
    [SerializeField] Text txtNumber;
    RectTransform rectTF;
    public RectTransform RectTF {
        get
        {
            if(rectTF == null)
            {
                rectTF = GetComponent<RectTransform>();
            }
            return rectTF;
        }
        
    }
    public void Initialize(DataResource dataResource)
    {
        visualRes.Init(dataResource);
        string res = $"{dataResource.amount}";
        if(dataResource.resType == mygame.sdk.RES_type.UnlimitedHeart || dataResource.resType == mygame.sdk.RES_type.UnlimitedMagnet 
            || dataResource.resType == mygame.sdk.RES_type.UnlimitedMutilColorBox || dataResource.resType == mygame.sdk.RES_type.DoubleReward)
        {
            int hour = dataResource.amount / 3600;
            int min = (dataResource.amount % 3600) / 60;
            if (hour > 0 && min > 0)
            {
                res = $"{hour}h{min.ToString("D2")}m";
            }
            else if (hour > 0)
            {
                res = $"{hour}h";
            }
            else
            {
                res = $"{min}m";
            }
        }
        res = "+" + res;
        txtNumber.text = res;
    }
    public void Fly()
    {
        RectTransform rectTF = canvasGroup.GetComponent<RectTransform>();
        canvasGroup.transform.localScale = Vector3.zero;
        Sequence sequence = DOTween.Sequence().SetId(this);
        sequence.Append(canvasGroup.transform.DOScale(1, .3f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Claim);
        }));
        sequence.Insert(0f, rectTF.DOAnchorPosY(30, .3f).SetEase(Ease.Linear));
        var text = canvasGroup.GetComponentInChildren<Text>();
        Shadow shadow = null;
        BetterOutline outline = null;
        if (text != null)
        {
            shadow = text.gameObject.GetComponent<Shadow>();
            outline = text.gameObject.GetComponent<BetterOutline>();
        }
        
        sequence.Insert(.7f,canvasGroup.DOFade(0, .5f).SetEase(Ease.Linear).OnUpdate(() =>
        {
            if (text != null && shadow != null && outline != null)
            {
                var shadowColor = shadow.effectColor;
                shadowColor.a = canvasGroup.alpha / 3;
                var outlineColor = outline.effectColor;
                outlineColor.a = canvasGroup.alpha / 3;
                shadow.effectColor = shadowColor;
                outline.effectColor = outlineColor;
            }
        }));
        sequence.Insert(.5f, rectTF.DOAnchorPosY(100, .5f).SetEase(Ease.InQuad));
        sequence.Play();
    }

    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
