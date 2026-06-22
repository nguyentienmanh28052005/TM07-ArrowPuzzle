using System;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum TooltipDirection
{
    Auto,
    Up,
    Right,
    Down,
    Left
}
public class GameTooltip : master.Singleton<GameTooltip>
{

    private Camera currentCamera;
    private Camera UICamera
    {
        get { if(currentCamera == null) currentCamera = UIManager.Instance.canvas.worldCamera; return currentCamera; }
    }

    private IList<DataResource> items;
    private DataResource specialItem;
    [SerializeField] private GameObject holder;
    [SerializeField] private RectTransform holderContent;
    [SerializeField] private RectTransform rectView;  
    
    [Header("Special Text")]
    [SerializeField] private Text bpTextPrefab;
    private Text currentSpecialText;
    
    [Header("Special")]
    [SerializeField] private TooltipResUI specialResUI;

    [Header("Normal")]
    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private Vector2 offsetDefault = new(100, 100);
    [SerializeField] private PoolTooltipResUI PoolTooltipResUI;
    [SerializeField] Color defaultOutlineColor;

    public static bool BlockHide;

    private bool isShow;
    private Vector2 offsetPreview;
    private Vector2 offsetArrow;
    private Vector2 offset;
    private TooltipDirection tooltipDirection;
    private Tween twScale;

    private void Start()
    {
        if (rectView == null)
        {
            rectView = UIManager.Instance.canvas.GetComponent<RectTransform>();
        }
    }

    private TooltipDirection ChooseDirection(Transform anchor, RectTransform parent)
    {
        var previewWorldPos = anchor.position;
        float parentMidY;
        if (parent == null)
        {
            if (UICamera == null) return TooltipDirection.Up;
            var screenPos = UICamera.WorldToScreenPoint(previewWorldPos);
            parentMidY = Screen.height / 2f;
            return screenPos.y < parentMidY ? TooltipDirection.Up : TooltipDirection.Down;
        }
        var parentWorldPos = parent.position;
        parentMidY = parentWorldPos.y;
        return previewWorldPos.y < parentMidY ? TooltipDirection.Up : TooltipDirection.Down;
    }

    /// <summary>
    /// Show tooltip for game
    /// </summary>
    /// <param name="anchor">position to show tooltip</param>
    /// <param name="parent"></param>
    /// <param name="iListItem"></param>
    /// <param name="specialItem"></param>
    /// <param name="key">text key</param>
    /// <param name="value">text value</param>
    /// <param name="param"> text param</param>
    public void ShowTooltip(Transform anchor, RectTransform parent = null, TooltipDirection tooltipdirection = TooltipDirection.Auto, Vector2 offset = default, IList<DataResource> iListItem = null, DataResource specialItem = null, string key = "", string value = "", string param = "")
    {
        if(isShow) return;
        SetOutlineColor(defaultOutlineColor);
        isShow = true;
        this.tooltipDirection = tooltipdirection == TooltipDirection.Auto ? ChooseDirection(anchor, parent) : tooltipdirection;
        if (offset == default)
        {
            SetUpOffsetDefault();
        }
        else
        {
            this.offset = offset;
        }

        SetUpDirection();
        SetAnchorPreview(anchor);
        Initialized(iListItem, specialItem);
        switch (param.ToLower())
        {
            case "screw pass":
                isShow = false;
                ShowTooltipSpecial(anchor, parent, tooltipdirection, offset, key, value, param); 
                break;
            default:
                SetNotice(key, value, param);
                break;
        }
        
    }

    public void ShowTooltip(Transform anchor, RectTransform parent = null, TooltipDirection tooltipdirection = TooltipDirection.Auto, Vector2 offset = default, string key = "", string value = "", string param = "")
    {
        if(isShow) return;
        SetOutlineColor(defaultOutlineColor);
        isShow = true;
        this.tooltipDirection = tooltipdirection == TooltipDirection.Auto ? ChooseDirection(anchor, parent) : tooltipdirection;
        if (offset == default)
        {
            SetUpOffsetDefault();
        }
        else
        {
            this.offset = offset;
        }
        SetUpDirection();
        SetAnchorPreview(anchor);
        Initialized(null);
        switch (param.ToLower())
        {
            case "screw pass":
                isShow = false;
                ShowTooltipSpecial(anchor, parent, tooltipdirection, offset, key, value, param); 
                break;
            default:
                SetNotice(key, value, param);
                break;
        }
    }

    public void ShowTooltipSpecial(Transform anchor, RectTransform parent = null, TooltipDirection tooltipdirection = TooltipDirection.Auto, Vector2 offset = default, string key = "", string value = "", string param = "")
    {
        if(isShow) return;
        SetOutlineColor(defaultOutlineColor);
        isShow = true;
        this.tooltipDirection = tooltipdirection == TooltipDirection.Auto ? ChooseDirection(anchor, parent) : tooltipdirection;
        if (offset == default)
        {
            SetUpOffsetDefault();
        }
        else
        {
            this.offset = offset;
        }
        SetUpDirection();
        SetAnchorPreview(anchor);
        Initialized(null);
        SetNotice(key, value, param);
        switch (param.ToLower())
        {
            case "screw pass":
                currentSpecialText = Instantiate(bpTextPrefab, textContent.transform);
                break;
            default:
                currentSpecialText = Instantiate(bpTextPrefab, textContent.transform);
                break;
        }
        CopyTextComponent(textContent, currentSpecialText);
        var color = ToHex(currentSpecialText.color);
        string val;
        if (!string.IsNullOrEmpty(key))
        {
            val = MutilLanguage.getStringWithKey(key, mygame.sdk.StateCapText.None, mygame.sdk.FormatText.F_String, param);
        }
        else
        {
            val = value.Replace("{0}",param );
            textContent.text = val;
        }
        currentSpecialText.color = new Color(1, 1, 1, 0);
        currentSpecialText.text = val.Replace("#ffffff00", $"{color}");
    }
    public static string ToHex(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
    }
    void CopyTextComponent(Text source, Text target)
    {
        if (source == null || target == null) return;
        
        target.fontSize = source.fontSize;
        target.fontStyle = source.fontStyle;
        target.alignment = source.alignment;
        target.lineSpacing = source.lineSpacing;
        target.supportRichText = source.supportRichText;
        target.resizeTextForBestFit = source.resizeTextForBestFit;
        target.resizeTextMinSize = source.resizeTextMinSize;
        target.resizeTextMaxSize = source.resizeTextMaxSize;
        target.horizontalOverflow = source.horizontalOverflow;
        target.verticalOverflow = source.verticalOverflow;
        target.raycastTarget = source.raycastTarget;
    }
    

    private void SetUpOffsetDefault()
    {
        offset = offsetDefault;
        if (tooltipDirection == TooltipDirection.Down || tooltipDirection == TooltipDirection.Up)
        {
            offset.x = 0;
            offset.y = tooltipDirection == TooltipDirection.Up ? offset.y : -offset.y;
        }
        else
        {
            offset.y = 0;
            offset.x = tooltipDirection == TooltipDirection.Right ? offset.x : -offset.x;
        }
    }

    [Serializable]
    public class Arrow
    {
        public GameObject arrowObject;
        public Vector2 offset;
    }
    [Header("Arrow")]
    [SerializeField] private Arrow[] arrows;
    private int currentArrow = -1;
    private void SetUpDirection()
    {
        if (currentArrow >= 0)
        {
            arrows[currentArrow].arrowObject.SetActive(false);
        }
        switch (tooltipDirection)
        {
            case TooltipDirection.Up:
                currentArrow = 0;
                holderContent.anchorMin = new Vector2(0.5f, 0f);
                holderContent.anchorMax = new Vector2(0.5f, 0f);
                holderContent.pivot = new Vector2(0.5f, 0f);
                offsetPreview = new Vector2(offset.x, offset.y);
                holderContent.anchoredPosition = new Vector2(0, arrows[0].offset.y);
                break;
            case TooltipDirection.Right:
                currentArrow = 1;
                holderContent.anchorMin = new Vector2(0f, 0.5f);
                holderContent.anchorMax = new Vector2(0f, 0.5f);
                holderContent.pivot = new Vector2(0f, 0.5f);
                offsetPreview = new Vector2(offset.x, offset.y);
                holderContent.anchoredPosition = new Vector2(arrows[1].offset.x, 0);
                break;
            case TooltipDirection.Down:
                currentArrow = 2;
                holderContent.anchorMin = new Vector2(0.5f, 1f);
                holderContent.anchorMax = new Vector2(0.5f, 1f);
                holderContent.pivot = new Vector2(0.5f, 1f);
                offsetPreview = new Vector2(offset.x, offset.y);
                holderContent.anchoredPosition = new Vector2(0, arrows[2].offset.y);
                break;
            case TooltipDirection.Left:
                currentArrow = 3;
                holderContent.anchorMin = new Vector2(1f, 0.5f);
                holderContent.anchorMax = new Vector2(1f, 0.5f);
                holderContent.pivot = new Vector2(1f, 0.5f);
                offsetPreview = new Vector2(offset.x, offset.y);
                holderContent.anchoredPosition = new Vector2(arrows[3].offset.x, 0);
                break;
        }
        arrows[currentArrow].arrowObject.SetActive(true);
    }
    public void SetAnchorPreview(Transform anchor)
    {
        transform.position = anchor.position;
        GetComponent<RectTransform>().anchoredPosition += offsetPreview;
    }

    private List<TooltipResUI> cacheResUI = new();
    public void Initialized(IList<DataResource> iListItem, DataResource specialItem = null)
    {
        items = iListItem;
        this.specialItem = specialItem;
        ClickPreview();
    }

    public void InitNormal()
    {
        cacheResUI.Clear();
        
        if (items is not null && items.Count > 0)
        {
            if (items?.Count >= 5 || (specialItem is not null && items?.Count > 3))
            {
                gridLayoutGroup.constraintCount = 2;
                initItem();
                cacheResUI[(items.Count - 1) / 2].HidePlusSymbols();
                cacheResUI[items.Count - 1].HidePlusSymbols();
            }
            else
            {
                gridLayoutGroup.constraintCount = 1;
                initItem();
                cacheResUI[items.Count - 1].HidePlusSymbols();
            }

            gridLayoutGroup.gameObject.SetActive(true);
        }
        else
        {
            gridLayoutGroup.gameObject.SetActive(false);
        }

        void initItem()
        {
            for (var i = 0; i < items.Count; i++)
            {
                TooltipResUI resUI = PoolTooltipResUI.Pool.Get();
                resUI.transform.localScale = Vector3.one;
                cacheResUI.Add(resUI);
                resUI.Init(items[i], items.Count >= 5 && items.Count % 2 == 1 && i > items.Count / 2);
                resUI.ShowPlusSymbols();
            }
        }
    }
    Coroutine fixPosCoroutine;
    public void InitSpecial()
    {
        if (specialItem != null)
        {
            specialResUI.Init(specialItem);
            specialResUI.gameObject.SetActive(true);
        }
        else
        {
            specialResUI.gameObject.SetActive(false);
        }
    }
    private void ClickPreview()
    {
        if (rectView == null)
        {
            rectView = UIManager.Instance.canvas.GetComponent<RectTransform>();
        }

        InitNormal();
        InitSpecial();

        if (twScale != null)
        {
            twScale.Kill();
        }
        holder.transform.localScale = Vector3.zero;

        holder.SetActive(true);
        fixPosCoroutine = StartCoroutine(IEFixPos());



        return;
        IEnumerator IEFixPos()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(holderContent);
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(holderContent);
            if (tooltipDirection != TooltipDirection.Left && tooltipDirection != TooltipDirection.Right)
            {
                
                Vector2 vector2 = holderContent.anchoredPosition;
                vector2.x = 0;
                holderContent.anchoredPosition = vector2;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectView, UICamera.WorldToScreenPoint(holderContent.transform.position), UICamera, out var localPoint);
                float over1 = localPoint.x + holderContent.rect.width / 2 - rectView.rect.width / 2 + verticalLayoutGroup.padding.right - 20;
                float over2 = localPoint.x - holderContent.rect.width / 2 + rectView.rect.width / 2 - verticalLayoutGroup.padding.left - 20;

                if (over1 > 0)
                {
                    vector2 = holderContent.anchoredPosition;
                    vector2.x -= over1;
                    holderContent.anchoredPosition = vector2;
                }
                if (over2 < 0)
                {
                    vector2 = holderContent.anchoredPosition;
                    vector2.x -= over2;
                    holderContent.anchoredPosition = vector2;
                }

            }
            if (tooltipDirection != TooltipDirection.Up && tooltipDirection != TooltipDirection.Down)
            {
                Vector2 vector2 = holderContent.anchoredPosition;
                vector2.y = 0;
                holderContent.anchoredPosition = vector2;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectView, UICamera.WorldToScreenPoint(holderContent.transform.position), UICamera, out var localPoint);
                float over1 = localPoint.y + holderContent.rect.height / 2 - rectView.rect.height / 2 + verticalLayoutGroup.padding.top - 20;
                float over2 = localPoint.y - holderContent.rect.height / 2 + rectView.rect.height / 2 - verticalLayoutGroup.padding.bottom - 20;

                if (over1 > 0)
                {
                    vector2 = holderContent.anchoredPosition;
                    vector2.y -= over1;
                    holderContent.anchoredPosition = vector2;
                }
                if (over2 < 0)
                {
                    vector2 = holderContent.anchoredPosition;
                    vector2.y -= over2;
                    holderContent.anchoredPosition = vector2;
                }

            }
            twScale = holder.transform.DOScale(Vector3.one, 0.5f).OnStart(() => AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Tool_Tip)).SetEase(Ease.OutBack).SetId(this);
        }


    }
    [FormerlySerializedAs("txtMove")]
    [Header("Text")]
    [SerializeField]
    private Text textContent;
    public void SetNotice(string key, string value, string param = "")
    {
        if(currentSpecialText) Destroy(currentSpecialText.gameObject);
        if (key.Length > 0)
        {
            textContent.SetText(key, mygame.sdk.StateCapText.None, mygame.sdk.FormatText.F_String, param);
        }
        else
        {
            textContent.SetValue(value);
        }
        if (key.Length == 0 && value.Length == 0)
        {
            textContent.gameObject.SetActive(false);
        }
        else
        {
            textContent.gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if(!isShow) return;
        isShow = false;
        BlockHide = false;
        holder.SetActive(false);
        if(currentSpecialText) Destroy(currentSpecialText.gameObject);
        foreach (var r in cacheResUI)
        {
            PoolTooltipResUI.Pool.Release(r);
        }
    }
    public void SetOutlineColor(Color c)
    {
        Shadow[] shadows = GetComponentsInChildren<Shadow>();
        foreach(var s  in shadows)
        {
            s.effectColor = c;
        }
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && holder.activeSelf && !BlockHide)
        {
            Hide();
        }
    }
    private void OnDestroy()
    {
        this.DOKill();
        if (fixPosCoroutine != null)
        {
            StopCoroutine(fixPosCoroutine);
        }
    }
}
