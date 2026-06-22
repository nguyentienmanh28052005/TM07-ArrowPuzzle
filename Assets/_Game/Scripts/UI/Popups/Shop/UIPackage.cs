using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using mygame.sdk;
using UnityEngine;
using UnityEngine.UI;

public class UIPackage : MonoBehaviour
{
    public enum LogType
    {
        None,
        LogIfFirst,
        LogIfVisible
    }

    public PackageData.PackageInfo baseItem;

    [SerializeField] private LogEvent.IAP_ShowPosition showPosition;
    [SerializeField] private LogEvent.IAP_ShowType showType;
    [SerializeField] private LogType logType = LogType.LogIfFirst;
    [SerializeField] private LogEvent.IAP_ShowAction showAction;

    [SerializeField] private int id = -1;
    [SerializeField] private Text titleText;
    [SerializeField] private Text descText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text levelUnlockText;

    [SerializeField] private Text buyCountText;
    [SerializeField] private UIItemInfo itemInfo;

    [SerializeField] private Text priceText;
    [SerializeField] private Text iapPriceText;
    [SerializeField] private Text iapPriceNoSaleText;
    [SerializeField] private Text adsPriceText;
    [SerializeField] private GameObject firstObject;
    [SerializeField] private Text firstAmount;
    [SerializeField] private Image tradeIcon;
    [SerializeField] private Button buyButton;

    private UIShopPopup uishop;
    [SerializeField] private RectTransform boosterContainer;
    [SerializeField] private RectTransform itemContainer;
    [SerializeField] private RectTransform heartContainer;

    [SerializeField] private Image goldIcon;
    [SerializeField] private Image bg;
    [SerializeField] private Image buttonHolder;
    [SerializeField] private Text goldAmount;
    [SerializeField] private Text boosterAmount;
    [SerializeField] private Text itemAmount;
    [SerializeField] private Text heartAmount;

    [SerializeField] private GameObject timeAmount;
    [SerializeField] private Text textTimeAmount;

    public GameObject tagBanner;
    [SerializeField] private Image[] itemHolderBG;
    [SerializeField] private Image[] itemLoopBG;
    [SerializeField] private Text texttagBanner;
    [SerializeField] private bool isSoloPack;
    [SerializeField] private float delayLogIfVisible;

    private bool isX2IfFirst;
    private bool isWaitLogIfVisible = true;
    public bool NoCreateItem;
    private Coroutine coroutine;
    public UIShopPopup shopPopup;
    public Action onBuySuccess;
    public Action onDonePopup;
    private List<UIItemInfo> allItems = new List<UIItemInfo>();
    private PackageData.ItemBuyInfo[] allItemBuyInfo;

    private PackageData.ItemBuyInfo[] bonusItem;

    private bool isFirstBuy
    {
        get => PlayerPrefsBase.Instance().getInt("first_buy_" + baseItem.skuId, 1) == 1;
        set => PlayerPrefsBase.Instance().setInt("first_buy_" + baseItem.skuId, value ? 1 : 0);
    }

    private Coroutine logCoroutine;

    protected virtual void Start()
    {
        if (isSoloPack)
        {
            SetUp(id);
        }
    }

    public void SetUp(int idPackage, PackageData.ItemBuyInfo[] bonus = null, params PackageData.ItemBuyInfo[] subtractGift)
    {
        this.id = idPackage;
        buyButton.onClick.AddListener(OnClickBuy);
        if (id >= 0)
        {
            Initialized(DataManager.Instance.packageData.FindPackage(id), bonus: bonus, subtractGift: subtractGift);
            if (logType == LogType.LogIfFirst)
            {
                LogShowIAP();
            }
            else if (logType == LogType.LogIfVisible && gameObject.activeInHierarchy)
            {
                logCoroutine = StartCoroutine(CheckLogIfVisible());
            }
        }

        Refresh();
    }

    private IEnumerator CheckLogIfVisible()
    {
        var rect = GetComponent<RectTransform>();
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(.1f);
        yield return new WaitForSeconds(delayLogIfVisible);
        yield return new WaitForEndOfFrame();
        while (true)
        {
            if (IsRectTransformInView(rect, UIManager.Instance.canvas.worldCamera) && logType == LogType.LogIfVisible &&
                isWaitLogIfVisible)
            {
                isShow = false;
                LogShowIAP();
                isWaitLogIfVisible = false;
            }

            yield return new WaitForSeconds(.1f);
        }
    }

    private bool IsRectTransformInView(RectTransform rectTransform, Camera cam)
    {
        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);

        foreach (Vector3 corner in worldCorners)
        {
            Vector3 viewportPoint = cam.WorldToViewportPoint(corner);
            if (viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                viewportPoint.z > 0) // Kiểm tra Z để tránh điểm nằm phía sau camera
            {
                return true;
            }
        }

        return false;
    }

    public void SetBG(Sprite bgSprite, Sprite btnSprite, Color color, Sprite holderSprite, Sprite loopSprite)
    {
        if (bg != null) bg.sprite = bgSprite;
        if (buttonHolder != null) buttonHolder.sprite = btnSprite;
        if (titleText != null)
        {
            var shadows = titleText.GetComponents<Shadow>();
            for (int i = 0; i < shadows.Length; i++)
            {
                shadows[i].effectColor = color;
            }

            //titleText.GetComponent<Shadow>().effectColor = color; 
            titleText.GetComponent<Outline>().effectColor = color;
        }

        if (itemHolderBG != null)
        {
            foreach (var item in itemHolderBG)
            {
                item.sprite = holderSprite;
            }
        }

        if (itemLoopBG != null)
        {
            foreach (var item in itemLoopBG)
            {
                if (loopSprite != null)
                {
                    item.sprite = loopSprite;
                    item.gameObject.SetActive(true);
                }
                else
                {
                    item.gameObject.SetActive(false);
                }
            }
        }
    }

    public void SetTagBanner(string text)
    {
        if (tagBanner)
        {
            tagBanner.SetActive(true);
        }

        if (texttagBanner)
        {
            texttagBanner.SetValue(text);
        }
    }

    public void Refresh()
    {
        if (baseItem == null || this == null) return;
        if (shopPopup)
        {
            DOVirtual.DelayedCall(0.5f, () => { shopPopup.RefreshLayout(); }).SetId(this);
            if (AdsHelper.isRemoveAds(0))
            {
                shopPopup.DisableAdsPack();
            }
        }

        buyButton.interactable = baseItem.isBuy;
        var scrollSnap = GetComponentInParent<AutoSwiftSnapScroll>(true);
        if (scrollSnap != null)
        {
            if (!baseItem.isActive)
            {
                scrollSnap.Remove(transform.GetSiblingIndex());
            }
        }
        else
        {
            gameObject.SetActive(baseItem.isActive);
        }
    }

    public void Initialized(PackageData.PackageInfo item, bool setupGift = true, PackageData.ItemBuyInfo[] bonus = null, params PackageData.ItemBuyInfo[] subtractGift)
    {
        bonusItem = bonus;
        baseItem = item;
        allItemBuyInfo = new PackageData.ItemBuyInfo[baseItem.allItems.Length];
        Array.Copy(baseItem.allItems, allItemBuyInfo, allItemBuyInfo.Length);

        if (subtractGift != null)
        {
            for (int i = 0; i < allItemBuyInfo.Length; i++)
            {
                var it = subtractGift.SingleOrDefault(x => x.itemType == allItemBuyInfo[i].itemType);
                if (it != null)
                {
                    var it2 = new PackageData.ItemBuyInfo(allItemBuyInfo[i].Icon, allItemBuyInfo[i].itemType, allItemBuyInfo[i].itemAmount);
                    it2.previewIcon = allItemBuyInfo[i].previewIcon;
                    it2.itemAmount -= it.itemAmount;
                    allItemBuyInfo[i] = it2;
                }
            }   
        }
        
        id = baseItem.id;
        if (setupGift && !NoCreateItem)
        {
            SetupGift(allItemBuyInfo);
        }

        if (firstAmount != null)
        {
            if (isX2IfFirst && isFirstBuy)
            {
                firstAmount.text = $"+{allItemBuyInfo[0].itemAmount * 2}";
                firstObject.SetActive(true);
            }
            else
            {
                firstObject.SetActive(false);
            }
        }

        if (titleText != null) titleText.SetText(baseItem.tile);
        if (descText != null) descText.text = baseItem.desc;
        if (levelUnlockText != null) levelUnlockText.text = levelUnlockText.text = $"Level {baseItem.levelUnlock}";
        if (iapPriceText != null) iapPriceText.gameObject.SetActive(item.buyType == BuyType.Iap);
        if (iapPriceNoSaleText != null) iapPriceNoSaleText.gameObject.SetActive(item.buyType == BuyType.Iap);
        if (tradeIcon != null) tradeIcon.gameObject.SetActive(item.buyType == BuyType.Trade);
        if (priceText != null) priceText.gameObject.SetActive(item.buyType == BuyType.Trade);
        if (adsPriceText != null) adsPriceText.gameObject.SetActive(item.buyType == BuyType.Ads);

        if (item.buyType == BuyType.Trade)
        {
            tradeIcon.sprite = baseItem.priceIcon;
            priceText.text = baseItem.price.ToString();
        }
        else if (item.buyType == BuyType.Iap)
        {
            iapPriceText.text = InappHelper.Instance.getPrice(baseItem.skuId);
            if (iapPriceNoSaleText != null)
            {
                iapPriceNoSaleText.text = InappHelper.Instance.getPrice(baseItem.skuId);
            }
        }
        else if (item.buyType == BuyType.Ads)
        {
            adsPriceText.text = $"{baseItem.price} Free";
        }

        buyButton.interactable = baseItem.isBuy;
        if (baseItem.resetType != PackageData.ResetType.None && timeText != null)
        {
            if (coroutine != null) StopCoroutine(coroutine);
            coroutine = StartCoroutine(RefreshTime());
            if (buyCountText != null) buyCountText.text = $"{baseItem.buyAmount}/{baseItem.count}";
        }
    }

    private void SetupGift(PackageData.ItemBuyInfo[] giftData)
    {
        if (itemInfo == null || giftData == null) return;

        goldAmount.text = string.Empty;

        if (timeAmount)
        {
            timeAmount.SetActive(false);
        }

        if (heartContainer)
        {
            heartContainer.parent.gameObject.SetActive(false);
        }

        if (itemAmount)
        {
            itemAmount.gameObject.SetActive(true);
        }

        foreach (var item in allItems)
        {
            item.gameObject.SetActive(false);
            item.transform.SetParent(itemContainer, false);
        }

        int itemIndex = 0;

        for (int i = 0; i < giftData.Length; i++)
        {
            var data = giftData[i];
            if (data.itemAmount <= 0) continue;
            var itemType = data.itemType;
            int itemTypeInt = (int)itemType;

            if (itemType == RES_type.GOLD)
            {
                if (goldIcon)
                {
                    goldIcon.sprite = data.previewIcon;
                }

                goldAmount.text = data.itemAmount.ToString();
                continue;
            }

            UIItemInfo item;
            if (itemIndex < allItems.Count)
            {
                item = allItems[itemIndex];
            }
            else
            {
                item = Instantiate(itemInfo);
                allItems.Add(item);
            }

            item.gameObject.SetActive(true);

            item.Initialized(data.itemType, data.previewIcon, data.itemAmount);
            item.SetUpData(data);

            if (itemTypeInt == 9 || itemTypeInt == 10)
            {
                item.transform.SetParent(itemContainer, false);

                itemAmount.gameObject.SetActive(false);
                item.infinitySymbol.SetActive(true);

                textTimeAmount.text = FormatTime(data.itemAmount);
                timeAmount.SetActive(true);
            }
            else if (itemTypeInt == 8)
            {
                item.transform.SetParent(itemContainer, false);
                itemAmount.text = "x" + data.itemAmount;
            }
            else if (itemTypeInt == 6)
            {
                heartContainer.parent.gameObject.SetActive(true);

                item.transform.SetParent(heartContainer, false);
                heartAmount.text = FormatTime(data.itemAmount);
            }
            else
            {
                item.transform.SetParent(boosterContainer, false);
                if (boosterAmount)
                {
                    boosterAmount.text = "x" + data.itemAmount;
                }
            }

            itemIndex++;
        }

        for (int i = itemIndex; i < allItems.Count; i++)
        {
            allItems[i].gameObject.SetActive(false);
        }
    }


    string FormatTime(int seconds)
    {
        if (seconds >= 3600)
        {
            int hours = seconds / 3600;
            return $"{hours}h";
        }
        else if (seconds >= 60)
        {
            int minutes = seconds / 60;
            return $"{minutes}m";
        }
        else
        {
            return $"{seconds}s";
        }
    }

    public void UpdateLayout()
    {
        boosterContainer.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(326, 265);
        itemContainer.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(194, 265);
        heartContainer.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(139, 265);
    }

    public void LogShowing()
    {
        //LogEvent.IAPShow(showType, showPosition, baseItem.skuId);
    }

    public void WaitLogIfVisible()
    {
        isShow = false;
        isWaitLogIfVisible = true;
        if (logCoroutine != null) StopCoroutine(logCoroutine);
        logCoroutine = StartCoroutine(CheckLogIfVisible());
    }

    public void SetPrice()
    {
        if (baseItem.buyType == BuyType.Trade)
        {
            tradeIcon.sprite = baseItem.priceIcon;
            priceText.text = baseItem.price.ToString();
        }
        else if (baseItem.buyType == BuyType.Iap)
        {
            iapPriceText.text = InappHelper.Instance.getPrice(baseItem.skuId);
            if (iapPriceNoSaleText != null)
            {
                iapPriceNoSaleText.text = InappHelper.Instance.getPrice(baseItem.skuId);
            }
        }
        else if (baseItem.buyType == BuyType.Ads)
        {
            adsPriceText.text = $"{baseItem.price} Free";
        }
    }

    private IEnumerator RefreshTime()
    {
        while (true)
        {
            var timeSpan = baseItem.ResetDuration();
            if (timeSpan.Days > 0)
            {
                timeText.text = $"{timeSpan.Days}D {timeSpan.Hours % 24}H";
            }
            else if (timeSpan.Hours > 0)
            {
                timeText.text = $"{timeSpan.Hours}H {timeSpan.Minutes % 60}M";
            }
            else
            {
                timeText.text = $"{timeSpan.Minutes:00}M {timeSpan.Seconds % 60}S";
            }

            baseItem.CheckReset();
            yield return new WaitForSeconds(1);
        }
    }

    private void OnClickBuy()
    {
        if (!baseItem.isBuy) return;
        if (baseItem.buyType == BuyType.Trade)
        {
            if (baseItem.price <= GameRes.getRes(baseItem.priceType))
            {
                DataManager.Instance.ReceiveGift(false, 0, "ui_package", LogEvent.ReasonItem.purchase, false, -1, null,
                    new ItemInfo(null, baseItem.priceType, -baseItem.price));
                OnBuySuccess(isFirstBuy && isX2IfFirst ? 2 : 1);
                isFirstBuy = false;
                Initialized(baseItem);
                Refresh();
            }
        }
        else if (baseItem.buyType == BuyType.Iap)
        {
            LogEvent.IAPClick(showType, showPosition, showAction, baseItem.skuId);
            InappHelper.Instance.BuyPackage(baseItem.skuId, "shop", callback =>
            {
                if (callback.status == 1)
                {
                    OnBuySuccess(isFirstBuy && isX2IfFirst ? 2 : 1);
                    isFirstBuy = false;
                    Initialized(baseItem);
                    Refresh();
                    LogEvent.IAPBuy(showType, showPosition, showAction, baseItem.skuId);
                }
            });
        }
        else if (baseItem.buyType == BuyType.Ads)
        {
            AudioManager.Instance.SetCacheAudio();
            var ss = AdsHelper.Instance.showGift("ui_shop", GameRes.GetLevel(), false, state =>
            {
                if (state == AD_State.AD_REWARD_OK || state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 ||
                    state == AD_State.AD_SHOW_MISS_CB || state == AD_State.AD_SHOW_FAIL ||
                    state == AD_State.AD_SHOW_FAIL2)
                {
                    AudioManager.Instance.ResetAudio();
                }

                if (state == AD_State.AD_REWARD_OK)
                {
                    OnBuySuccess(isFirstBuy && isX2IfFirst ? 2 : 1);
                    isFirstBuy = false;
                    Initialized(baseItem);
                    Refresh();
                }
            });

            if (ss < 0) UIManager.Instance.NotifyContent(content: "No Ads Available!", key: "_no_ads_available");
            //LogEventManager.Instance.LogAds("gift", "ui_shop", ss >= 0 ? 1 : 0);
        }
    }

    private void OnBuySuccess(int buyRate)
    {
        var receiver = new ItemInfo[baseItem.allItems.Length];
        Array.Copy(baseItem.allItems, receiver, receiver.Length);

        if (bonusItem != null)
        {
            for (int i = 0; i < baseItem.allItems.Length; i++)
            {
                var receiverItem = receiver[i];
                var it = bonusItem.SingleOrDefault(x =>x.itemType == receiverItem.itemType);
                if (it != null)
                {
                    
                    if (receiverItem.itemType == it.itemType)
                    {
                        receiverItem = new PackageData.ItemBuyInfo(receiver[i].Icon, receiver[i].itemType, receiver[i].itemAmount);
                        receiverItem.itemAmount += it.itemAmount;
                    }
                }
            }
        }
       
        DataManager.Instance.ReceiveGift(true, 0, "ui_package", LogEvent.ReasonItem.purchase, true, -1, receiver);
        baseItem.BuyPackage();
        if (baseItem.skuId == "remove_ads_week")
        {
            AdsHelper.setRemoveAds(8, 24 * 3);
        }

        if (baseItem.skuId == "premium_battle_pass")
        {
            // EventBattlePassManager.Instance.UnlockVip() removed - event system not available
            shopPopup.bpPack.SetActive(false);
        }

        if (shopPopup)
        {
            shopPopup.CheckRefresShop();
        }

        onBuySuccess?.Invoke();
    }

    public void IAPShowAction(LogEvent.IAP_ShowAction showAction, LogEvent.IAP_ShowPosition showPosition,
        bool isLog = false)
    {
        this.showAction = showAction;
        this.showPosition = showPosition;
        if (isLog) LogShowIAP();
    }

    bool isShow = false;

    private void LogShowIAP()
    {
        if (isShow) return;
        isShow = true;
        DOVirtual.DelayedCall(0.1f, () => { LogEvent.IAPShow(showType, showPosition, showAction, baseItem.skuId); })
            .SetId(this);
    }

    private void OnDestroy()
    {
        if (logCoroutine != null) StopCoroutine(logCoroutine);
        if (coroutine != null) StopCoroutine(coroutine);
        DOTween.Kill(this);
    }
}