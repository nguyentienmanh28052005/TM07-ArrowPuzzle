using System;
using UnityEngine;
using UnityEngine.UI;

public class AutoChangeTheme : MonoBehaviour
{
    [Serializable]
    public class ThemeConfig
    {
        [Serializable]
        public class ThemeData
        {
            public LevelType levelType;

            [Space, Header("======Sprites======"), Space]
            public Sprite sprBGMain;
            public Sprite sprBGFrame;
            public Sprite sprBGHeader;
            public Sprite sprBGButton;
            public Sprite sprBGCoinBar;
            public Sprite sprBGStretch;
            public Sprite sprBGLight;

            [Space, Header("======Colors======"), Space]
            public Color outlineColor = Color.white;
            public Color textColor = Color.white;
        }

        [SerializeField] private ThemeData[] themes;

        public ThemeData GetTheme(LevelType levelType)
        {
            for (int i = 0; i < themes.Length; i++)
            {
                if (themes[i].levelType == levelType)
                {
                    return themes[i];
                }
            }

            return themes.Length > 0 ? themes[0] : null;
        }
    }
    
    [SerializeField] ThemeConfig themeConfig;

    [Space, Header("======BG Images======"), Space]
    [SerializeField] Image imgBGMain;
    [SerializeField] Image imgBGFrame;
    [SerializeField] Image imgBGHeader;
    [SerializeField] Image[] imgBGButton;
    [SerializeField] Image imgBGCoinBar;
    [SerializeField] Image imgBGStretch;
    [SerializeField] Image imgBGLight;

    [Space, Header("======Outline======"), Space]
    [SerializeField] Shadow[] outlines;

    [Space, Header("======Text======"), Space]
    [SerializeField] Text[] texts;

    public void ApplyTheme(LevelType levelType)
    {
        if (themeConfig == null) return;

        var theme = themeConfig.GetTheme(levelType);
        if (theme == null) return;

        ApplyImage(imgBGMain, theme.sprBGMain);
        ApplyImage(imgBGFrame, theme.sprBGFrame);
        ApplyImage(imgBGHeader, theme.sprBGHeader);
        for (int i = 0; i < imgBGButton.Length; i++)
        {
            ApplyImage(imgBGButton[i], theme.sprBGButton);
        }
        ApplyImage(imgBGCoinBar, theme.sprBGCoinBar);
        ApplyImage(imgBGStretch, theme.sprBGStretch);
        ApplyImage(imgBGLight, theme.sprBGLight);

        for (int i = 0; i < outlines.Length; i++)
        {
            var tmp = outlines[i].GetComponents<Shadow>();
            for (int k = 0; k < tmp.Length; k++)
            {
                if (tmp[k] != null)
                {
                    tmp[k].effectColor = theme.outlineColor;
                }
            }
        }

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null)
            {
                texts[i].color = theme.textColor;
            }
        }
    }

    private void ApplyImage(Image img, Sprite spr)
    {
        if (img == null || spr == null) return;
        img.sprite = spr;
    }
}
