using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualClaimBase : MonoBehaviour
{
    [SerializeField] protected Image bg;
    [SerializeField] protected Image icon;
    [SerializeField] protected Text txtAmount;
    public Image BG { get => bg; set => bg = value; }
    public Image Icon { get => icon; set => icon = value; }
    public string prefix;

    public virtual void Init(DataResource dataResource, Sprite bgDf = null, Sprite iconDf = null, byte star = 0)
    {
        short idBg = dataResource.GetIdBg();
        short idIcon = dataResource.GetIdIcon();
        if (bg != null)
        {
            if (bgDf == null)
            {
                if (idBg <= 0)
                {
                    bg.sprite = SpriteResourceManager.Instance.GetBg(dataResource.resType);
                }
                else
                {
                    bg.sprite = SpriteResourceManager.Instance.GetBg(idBg);
                }

            }
            else
            {
                bg.sprite = bgDf;
            }
        }
        if (icon != null)
        {
            if (iconDf == null)
            {
                if (idIcon <= 0)
                {
                    icon.sprite = SpriteResourceManager.Instance.GetIcon(dataResource.resType);
                }
                else
                {
                    icon.sprite = SpriteResourceManager.Instance.GetIcon(idIcon);
                }

            }
            else
            {
                icon.sprite = iconDf;
            }
        }

    }
    public virtual bool CanVisual(mygame.sdk.RES_type rES_Type)
    {
        return true;
    }
}
