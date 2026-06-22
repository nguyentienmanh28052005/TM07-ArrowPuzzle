using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageDropdown : MonoBehaviour
{
    [SerializeField] Dropdown dropdown; 
    [SerializeField] OnEnableDrop enableDrop; 
    void Awake()
    {
        dropdown.ClearOptions();
        PopupLanguage.allLangs.Sort((x, y) => x.langName[0].CompareTo(y.langName[0]));

        List<string> options = new List<string>();
        foreach (var lang in PopupLanguage.allLangs)
        {
            options.Add(lang.langName);
        }

        dropdown.AddOptions(options);
        dropdown.onValueChanged.AddListener(OnDropdownChanged);
        enableDrop.EnableAction += () =>
        {
           StartCoroutine(ScrollToOptionCenter(dropdown.value));
        };
    }
    private void OnEnable()
    {
        var allLangs = PopupLanguage.allLangs;
        string lset = PlayerPrefs.GetString("mem_set_lang", "");
        lset = lset == "" ? MutilLanguage.Instance().languageCode4mutil : lset;
        dropdown.value = allLangs.FindIndex(x => x.langCode == lset);
    }

    void OnDropdownChanged(int index)
    {
        var lang = PopupLanguage.allLangs[index];
        string lset = PlayerPrefs.GetString("mem_set_lang", "");
        lset = lset == "" ? MutilLanguage.Instance().languageCode4mutil : lset;


        if (lset != lang.langCode)
        {
            MutilLanguage.Instance().setLang(lang.langCode);
        }
    }
    IEnumerator ScrollToOptionCenter(int index)
    {
        yield return new WaitForEndOfFrame();
        var scrollRect = dropdown.GetComponentInChildren<ScrollRect>();
        if (scrollRect == null) yield break;

        var content = scrollRect.content;
        int total = dropdown.options.Count;
        if (total <= 1) yield break;

        // L?y kích th??c item m?u (gi? s? t?t c? item ??u cùng chi?u cao)
        if (content.childCount == 0) yield break;
        RectTransform firstItem = content.GetChild(0) as RectTransform;
        if (firstItem == null) yield break;

        float itemHeight = firstItem.rect.height;

        // L?y spacing/padding n?u dùng VerticalLayoutGroup
        var layout = content.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        float spacing = layout != null ? layout.spacing : 0f;
        float paddingTop = layout != null ? layout.padding.top : 0f;
        float paddingBottom = layout != null ? layout.padding.bottom : 0f;

        // L?y viewport height
        RectTransform viewport = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();
        float viewportHeight = viewport.rect.height;

        // S? item có th? hi?n ra trong viewport (ít nh?t 1)
        int visibleCount = Mathf.Max(1, Mathf.FloorToInt((viewportHeight + spacing) / (itemHeight + spacing)));

        // N?u toàn b? item v?a trong viewport, không c?n scroll
        if (total <= visibleCount)
        {
            scrollRect.verticalNormalizedPosition = 1f; // top
            yield break;
        }

        // Tính firstVisibleIndex mong mu?n ?? index ? gi?a viewport
        int maxFirstIndex = total - visibleCount;
        float desiredFirstIndexFloat = index - (visibleCount - 1) * 0.5f; // c? g?ng ??t index lên gi?a
        int desiredFirstIndex = Mathf.Clamp(Mathf.RoundToInt(desiredFirstIndexFloat), 0, maxFirstIndex);

        // Chuy?n thành normalized (1 = top, 0 = bottom)
        float normalized = (maxFirstIndex <= 0) ? 1f : 1f - (desiredFirstIndex / (float)maxFirstIndex);
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(normalized);
    }
}
