using DG.Tweening;
using mygame.sdk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualClaimBoosterUnlimited : VisualClaimBase
{
    [SerializeField] List<RES_type> listRestype;
    public override bool CanVisual(RES_type rES_Type)
    {
        return listRestype.Contains(rES_Type);
    }
    public override void Init(DataResource dataResource, Sprite bgDf = null, Sprite iconDf = null, byte star = 0)
    {
        base.Init(dataResource, bgDf, iconDf, star);
        int hour = dataResource.amount / 3600;
        int min = (dataResource.amount % 3600) / 60;
        if (hour > 0 && min > 0)
        {
            txtAmount.text = $"{hour}h{min.ToString("D2")}m";
        }
        else if (hour > 0)
        {
            txtAmount.text = $"{hour}h";
        }
        else
        {
            txtAmount.text = $"{min}m";
        }
        switch (txtAmount.text.Length)
        {
            case >= 7:
                txtAmount.fontSize = 32;
                return;
            case >= 6:
                txtAmount.fontSize = 36;
                break;
            case >= 5:
                txtAmount.fontSize = 40;
                break;
            default:
                txtAmount.fontSize = 44;
                break;
        }
    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
