using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using mygame.sdk;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIBuyMoney : MonoBehaviour
{
    private PackageData.PackageInfo baseItem;
    
    [SerializeField] private LogEvent.IAP_ShowPosition showPosition;
    [SerializeField] private LogEvent.IAP_ShowType showType;
    [SerializeField] private Text titleText;
    [SerializeField] private UIPackage.LogType logType = UIPackage.LogType.LogIfFirst;
    
    [SerializeField] private Text[] amountText;
    [SerializeField] private Image[] itemIcon;
    [SerializeField] private Text priceText;
    [SerializeField] private Text iapPriceText;
    [SerializeField] private Text iapPriceNoSaleText;
    [SerializeField] private Text adsPriceText;
    [SerializeField] private GameObject firstObject;
    [SerializeField] private Text firstAmount;
    [SerializeField] private Image tradeIcon;
    [SerializeField] private Button buyButton;
    [SerializeField] private Transform receivetarget;

    private bool isX2IfFirst;
    private bool isWaitLogIfVisible = true;
    private Coroutine logCoroutine;

    private bool isFirstBuy
    {
        get => PlayerPrefsBase.Instance().getInt("first_buy_" + baseItem.skuId, 1) == 1;
        set => PlayerPrefsBase.Instance().setInt("first_buy_" + baseItem.skuId, value ? 1 : 0);
    }
    private void Start()
    {
        buyButton.onClick.AddListener(OnClickBuy);
    }
    
    public void Initialized(PackageData.PackageInfo item, bool isFirstCall = false)
    {
        baseItem = item;
        isX2IfFirst = item.isX2IfFirst;
        for (int i = 0; i < baseItem.allItems.Length; i++)
        {
            if (i < amountText.Length)
            {
                var color = Color.white;
                if (baseItem.allItems[i].itemType == RES_type.Ticket)
                {
                    color = new Color(.95f, .56f, 1f, 1);
                }
                else if(baseItem.allItems[i].itemType == RES_type.GOLD)
                {
                    color = new Color(1, .8f, 0.5f, 1);
                }
                
                var hexCC = ColorUtility.ToHtmlStringRGB(color);
                if (isX2IfFirst && isFirstBuy)
                {
                    amountText[i].text = $"{baseItem.allItems[i].itemAmount}+<color=#{hexCC}>{baseItem.allItems[i].itemAmount}</color>";

                }
                else
                {
                    amountText[i].text = $"{baseItem.allItems[i].itemAmount}";

                }

            }
            
            if (i < itemIcon.Length)
            {
                itemIcon[i].sprite = baseItem.allItems[i].previewIcon;
            }
        }

        if (firstAmount != null)
        {
            if (isX2IfFirst && isFirstBuy)
            {
                firstAmount.text = $"+{baseItem.allItems[0].itemAmount * 2}";
                firstObject.SetActive(true);
            }
            else
            {
                firstObject.SetActive(false);
            }
        }
        
        if (titleText != null) titleText.text = baseItem.tile;
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

        if (isFirstCall)
        {
            if (logType == UIPackage.LogType.LogIfFirst)
            {
                LogShowIAP();
            }
            else if(logType == UIPackage.LogType.LogIfVisible && gameObject.activeSelf)
            {
                logCoroutine = StartCoroutine(CheckLogIfVisible());
            }
        }
    }
    
    private IEnumerator CheckLogIfVisible()
    {
        var rect = GetComponent<RectTransform>();
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(.1f);
        yield return new WaitForEndOfFrame();

        while (true)
        {
            if (IsRectTransformInView(rect, UIManager.Instance.canvasScreen.worldCamera) && logType == UIPackage.LogType.LogIfVisible && isWaitLogIfVisible)
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

    public void WaitLogIfVisible()
    {
        isWaitLogIfVisible = true;
        if (logCoroutine != null) StopCoroutine(logCoroutine);
        logCoroutine = StartCoroutine(CheckLogIfVisible());
    }
    
    public void LogShowing()
    {
        //LogEvent.IAPShow(showType, showPosition, baseItem.skuId);
    }
    
    private void OnClickBuy()
    {
        if (baseItem.buyType == BuyType.Trade)
        {
            if (baseItem.price <= GameRes.getRes(baseItem.priceType))
            {
                OnBuySuccess(isFirstBuy && isX2IfFirst ? 2:1);
                isFirstBuy = false;
                Initialized(baseItem);
                DataManager.Instance.ReceiveGift(false, 0, "ui_buy_money", LogEvent.ReasonItem.purchase, false, -1, new ItemInfo(null, baseItem.priceType, -baseItem.price));
            }
        }
        else if (baseItem.buyType == BuyType.Iap)
        {
            LogEvent.IAPClick(showType, showPosition, showAction, baseItem.skuId);
            InappHelper.Instance.BuyPackage(baseItem.skuId, "shop", callback =>
            {
                if (callback.status == 1)
                {
                    OnBuySuccess(isFirstBuy && isX2IfFirst ? 2:1);
                    isFirstBuy = false;
                    Initialized(baseItem);
                    LogEvent.IAPBuy(showType, showPosition, showAction, baseItem.skuId);
                }
            });
        }
        else if (baseItem.buyType == BuyType.Ads)
        {
            AudioManager.Instance.SetCacheAudio();
            var ss = AdsHelper.Instance.showGift("ui_shop", GameRes.GetLevel(), false, state =>
            {
                if (state == AD_State.AD_REWARD_OK || state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 || state == AD_State.AD_SHOW_MISS_CB || state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
                {
                    AudioManager.Instance.ResetAudio();
                }
                if (state == AD_State.AD_REWARD_OK)
                {
                    OnBuySuccess(isFirstBuy && isX2IfFirst ? 2:1);
                    isFirstBuy = false;
                    Initialized(baseItem);
                }
            });
            
            if (ss < 0) UIManager.Instance.NotifyContent(content: "No Ads Available!", key: "_no_ads_available");
            //LogEventManager.Instance.LogAds("gift", "ui_shop", ss >= 0 ? 1 : 0);
        }
    }

    private void OnBuySuccess(int buyRate)
    {
        DataManager.Instance.ReceiveGift(false, .75f, "ui_buy_money", LogEvent.ReasonItem.purchase, true, -1, baseItem.allItems);
        //PopupManager.Instance.effectReceive.Initialized(baseItem.allItems, transform.transform.position, receivetarget);
        DataResource[] dataResources = new DataResource[baseItem.allItems.Length];
        for(int i = 0; i < dataResources.Length; i++)
        {
            dataResources[i] = baseItem.allItems[i].ToDataResource();
        }
        if (dataResources.Length == 1)
        {
            if(dataResources[0].resType == RES_type.Ticket)
            {
                RewardReceivedHub.Instance.TicketFly(UIManager.Instance.canvas.worldCamera.WorldToScreenPoint(transform.position), null, dataResources[0].amount,null);
            }
            else if (dataResources[0].resType == RES_type.GOLD)
            {
                RewardReceivedHub.Instance.CoinFly(UIManager.Instance.canvas.worldCamera.WorldToScreenPoint(transform.position), null, dataResources[0].amount, null);
            }
            else
            {
                RewardReceivedHub.Instance.ShowRewardGroupImmediate(dataResources);
            }
        }
        else
        {
            RewardReceivedHub.Instance.ShowRewardGroupImmediate(dataResources);
        }
        //ScreenManager.Instance.RefreshBannerArea();
        
    }

    public void SetShowPosition(LogEvent.IAP_ShowPosition showPosition)
    {
        this.showPosition = showPosition;        
    }
    private LogEvent.IAP_ShowAction showAction;
    public void IAPShowAction(LogEvent.IAP_ShowAction showAction, LogEvent.IAP_ShowPosition showPosition)
    {
        this.showAction = showAction;
        this.showPosition = showPosition;  
        //LogShowIAP();
    }
    bool isShow = false;
    private void LogShowIAP()
    {
        if (isShow) return;
        isShow = true;
        DOVirtual.DelayedCall(0.1f, () =>
        {
            LogEvent.IAPShow(showType, showPosition, showAction, baseItem.skuId);
        });
    }
}
