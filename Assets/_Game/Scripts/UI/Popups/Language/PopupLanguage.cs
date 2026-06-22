using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupLanguage : PopupUI
{
    [SerializeField] ButtonLanguage prefabBtn;
    [SerializeField] RectTransform contentHolder;
    [SerializeField] ScrollRect scrollRect;
    List<ButtonLanguage> buttonLanguages = new List<ButtonLanguage>();

    public static List<(string langCode, string langName)> allLangs = new List<(string langCode, string langName)>
{
    ("default", "English"),
    ("vi", "Vietnamese"),
    ("ar", "Arabic"),
    ("it", "Italian"),
    ("de", "German"),
    ("fr", "French"),
    ("he", "Hebrew"),
    ("ru", "Russian"),
    ("pt", "Portuguese"),
    ("ja", "Japanese"),
    ("ko", "Korean")
};
    private void Awake()
    {
        prefabBtn.gameObject.SetActive(false);
    }
    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        allLangs.Sort((x, y) => x.langName[0].CompareTo(y.langName[0]));
        string lset = PlayerPrefs.GetString("mem_set_lang", "");
        lset = lset == "" ? MutilLanguage.Instance().languageCode4mutil : lset;
        ButtonLanguage selectbtn = null;
        for (int i = 0; i < allLangs.Count; i++)
        {
            var btnLang = Instantiate(prefabBtn, contentHolder);
            buttonLanguages.Add(btnLang);
            btnLang.Initialize(allLangs[i].langCode, allLangs[i].langName);
            if (allLangs[i].langCode == lset)
            {
                btnLang.OnSelect();
                selectbtn = btnLang;
            }
            else
            {
                btnLang.OnDeSelect();
            }
            btnLang.OnClickAction += () =>
            {
                ChooseBtn(btnLang);
            };
            btnLang.gameObject.SetActive(true);
        }
        if(selectbtn != null)
        {
            Canvas.ForceUpdateCanvases();
            ScrollTo(selectbtn.GetComponent<RectTransform>());
        }
    }
    public void ChooseBtn(ButtonLanguage buttonLanguage)
    {
        for (int i = 0; i < buttonLanguages.Count; i++)
        {
            buttonLanguages[i].OnDeSelect();
        }
        buttonLanguage.OnSelect();
        string lset = PlayerPrefs.GetString("mem_set_lang", "");
        lset = lset == "" ? MutilLanguage.Instance().languageCode4mutil : lset;

        for (int i = 0; i < buttonLanguages.Count; i++)
        {
            if (buttonLanguages[i].IsSelected)
            {
                if (lset != allLangs[i].langCode)
                {
                    MutilLanguage.Instance().setLang(allLangs[i].langCode);
                }
            }
        }
    }
    public override void Hide()
    {
        
        base.Hide();
    }
    public void ScrollTo(RectTransform target)
    {
        float contentHeight = contentHolder.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;

        float itemCenterY = Mathf.Abs(target.anchoredPosition.y) + (target.rect.height / 2f);

        float targetPos = itemCenterY - (viewportHeight / 2f);

        float normalizedPos = 1f - (targetPos / (contentHeight - viewportHeight));
        normalizedPos = Mathf.Clamp01(normalizedPos);

        scrollRect.verticalNormalizedPosition = normalizedPos;
    }
}
