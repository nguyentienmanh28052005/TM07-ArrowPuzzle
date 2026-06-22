using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualClaimDefault : VisualClaimBase
{
    public override void Init(DataResource dataResource, Sprite bgDf = null, Sprite iconDf = null, byte star = 0)
    {
        base.Init(dataResource, bgDf, iconDf, star);
        // if (dataResource.amount <= 1)
        // {
        //     txtAmount.gameObject.SetActive(false);
        // }
        // else
        // {
        txtAmount.gameObject.SetActive(true);
        txtAmount.SetValue($"{prefix}{dataResource.amount}");
        // }
    }
}
