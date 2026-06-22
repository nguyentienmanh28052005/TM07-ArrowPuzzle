using DG.Tweening;
using mygame.sdk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualClaimGold : VisualClaimBase
{
    [SerializeField] List<RES_type> listRestype;
    [SerializeField] Sprite[] listSprite;
    [SerializeField] int[] milestones;
    [SerializeField] RectTransform bgText;
    public int timeDuration=1;
    public override bool CanVisual(RES_type rES_Type)
    {
        return listRestype.Contains(rES_Type);
    }
    public override void Init(DataResource dataResource, Sprite bgDf = null, Sprite iconDf = null, byte star = 0)
    {
        base.Init(dataResource, bgDf, iconDf, star);
        Sprite spriteIcon=null;
        for (int i = 0; i < milestones.Length; i++)
        {
            if (dataResource.amount >= milestones[i])
            {
                spriteIcon = listSprite[i];
                if (i >= listSprite.Length)
                {
                    break;
                }
            }
        }
        icon.sprite = spriteIcon;
        if(dataResource.GetIdIcon() == 10 && dataResource.resType == RES_type.GOLD)
        {
            icon.sprite = listSprite[2];
        }
        //if (dataResource.amount <= 1)
        //{
        //    if (bgText != null)
        //    {
        //        bgText.gameObject.SetActive(false);
        //    }
        //    txtAmount.gameObject.SetActive(false);
        //}
        //else
        { 
            if (bgText != null)
            {
                bgText.gameObject.SetActive(true);
            }
            txtAmount.gameObject.SetActive(true);
            txtAmount.text = ($"{prefix}{dataResource.amount}");
        }
        int num=0;
        DOTween.To(() => num, x => num = x, dataResource.amount, timeDuration).SetDelay(Mathf.Min(dataResource.amount*0.02f ,.2f)).OnUpdate(() => { txtAmount.SetValue($"{prefix}{num}"); ; }).SetId(this);
    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
