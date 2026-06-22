using UnityEngine;
using UnityEngine.UI;

public class VisualResGeneral : VisualResUIBase
{
    [SerializeField] private Image bg;
    [SerializeField] private Image icon;
    public override Image BG { get => bg; set => bg = value; }
    public override Image Icon { get => icon; set => icon = value; }

    public override void Init(DataResource type, Sprite bgDf = null, Sprite iconDf = null, byte star = 0)
    {
        short idBg = type.GetIdBg();
        short idIcon = type.GetIdIcon();
        if (bg != null)
        {
            if (bgDf == null)
            {
                if (idBg <= 0)
                {
                    bg.sprite = SpriteResourceManager.Instance.GetBg(type.resType);
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
                    icon.sprite = SpriteResourceManager.Instance.GetIcon(type.resType);
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
}
