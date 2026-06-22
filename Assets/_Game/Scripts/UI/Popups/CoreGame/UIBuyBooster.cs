using System;
using System.Collections;
using System.Collections.Generic;
using Crystal;
using DG.Tweening;
using master;
using mygame.sdk;
using UnityEngine;
using UniRx;
using Observer = master.Observer;

using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIBuyBooster : PopupUI
{
    [SerializeField] private Button getIfAdsBtn;
    [SerializeField] private Button getIfAdsBtnGray;
    [SerializeField] private Button getIfAdsBtnLarge;
    [SerializeField] private Button getIfAdsBtnLargeGray;
    [SerializeField] private Button getIfMoneyBtn;
    [SerializeField] private Button getIfMoneyBtnLarge;
    [SerializeField] private Button btnFree;
    [SerializeField] private Image icon;
    [SerializeField] private Image priceIcon;
    [SerializeField] private Text nameText;
    [SerializeField] private Text valueText;
    [SerializeField] private Text descText;
    [SerializeField] private Text priceValue;
    [SerializeField] private Text priceValueLarge;
    [SerializeField] private Text txtAmountAds;
    [SerializeField] private RectTransform boosterPanel;
    [SerializeField] RectTransform rect;
    [SerializeField] UIPackage packStarter;

    private BoosterInfo boosterInfo;
    private Action onBuySuccess;
    public static bool isFreeNow;
    
    private static int LEVEL_REMOVE_ADS_BUTTON
    {
        get
        {
            return AdsRewardConfig.GetConfig().levelRemoveAdsBuyBooster;
        }
    }
    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        getIfAdsBtn.onClick.AddListener(OnClickBuyIfAds);
        getIfMoneyBtn.onClick.AddListener(OnClickBuyIfMoney);
        getIfAdsBtnLarge.onClick.AddListener(OnClickBuyIfAds);
        getIfMoneyBtnLarge.onClick.AddListener(OnClickBuyIfMoney);
        btnFree.onClick.AddListener(FreeButton);
        getIfAdsBtnGray.onClick.AddListener(() =>
        {
            UIManager.Instance.NotifyContent("", "_reach_limit_ads");
        });
        getIfAdsBtnLargeGray.onClick.AddListener(() =>
        {
            UIManager.Instance.NotifyContent("", "_reach_limit_ads");
        });

    }
    public override void Show(Action onClose)
    {
        base.Show(onClose);
        packStarter.gameObject.SetActive(DataManager.Instance.packageData.FindPackage(1).isActive);
        rect.anchoredPosition = new Vector2(0, 1300);
        rect.DOLocalMoveY(-200f, .665f).SetEase(Ease.OutBack).SetId(this);
        packStarter.transform.localScale = Vector3.zero;
        packStarter.transform.DOScale(1, .665f).SetEase(Ease.OutBack).SetId(this);
    }
    public void Initialized(BoosterInfo info, Action success)
    {
        onBuySuccess = success;
        boosterInfo = info;
        icon.sprite = info.bigIcon;
        nameText.SetText(info.name);
        descText.SetText(info.desc);
        priceValue.text = $"{BoosterManager.Instance.GetPrice(info.type)}";
        priceValueLarge.text = $"{BoosterManager.Instance.GetPrice(info.type)}";

        var amount = BoosterManager.Instance.GetNumBooster(info.type);
        valueText.text = amount > 1 ? $"x{BoosterManager.Instance.GetNumBooster(info.type)}" : "";
        boosterInfo = info;
        onBuySuccess = success;
        SetFree(info.type == BoosterType.ExtraSlot && isFreeNow);

    }
    void SetFree(bool isFree)
    {
        if (isFree)
        {
            btnFree.gameObject.SetActive(true);
            getIfAdsBtn.gameObject.SetActive(false);
            getIfAdsBtnGray.gameObject.SetActive(false);
            getIfMoneyBtn.gameObject.SetActive(false);
            getIfAdsBtnLarge.gameObject.SetActive(false);
            getIfAdsBtnLargeGray.gameObject.SetActive(false);
            getIfMoneyBtnLarge.gameObject.SetActive(false);
        }
        else
        {
            btnFree.gameObject.SetActive(false);
            var boosterPriceConfig = BoosterManager.Instance.GetBoosterPriceConfig(boosterInfo.type);
            if (BoosterManager.IsOverAds)
            {
                Debug.Log("Over Ads Level");
            }
            if (BoosterManager.IsOverAdsDaily)
            {
                Debug.Log("Over Ads Daily");
            }
            if ((DataManager.Level >= LEVEL_REMOVE_ADS_BUTTON && LEVEL_REMOVE_ADS_BUTTON > 0) || DataManager.Level < AdsRewardConfig.GetConfig().levelActiveAdsBuyBooster)
            {
                Debug.Log("Over ads Level buy booster");
            }
            bool isOverAds = false;
            bool isOverAds2 = BoosterManager.IsOverAds || BoosterManager.IsOverAdsDaily;
            bool isShowAdsBtn = (DataManager.Level < LEVEL_REMOVE_ADS_BUTTON || LEVEL_REMOVE_ADS_BUTTON <= 0) && DataManager.Level > AdsRewardConfig.GetConfig().levelActiveAdsBuyBooster && !isOverAds2;
         
            getIfAdsBtn.gameObject.SetActive(boosterPriceConfig.typeBuy == 0 && !isOverAds && isShowAdsBtn);
            getIfAdsBtnGray.gameObject.SetActive(boosterPriceConfig.typeBuy == 0 && isOverAds && isShowAdsBtn);
            getIfMoneyBtn.gameObject.SetActive(boosterPriceConfig.typeBuy == 0 && isShowAdsBtn);
            getIfAdsBtnLarge.gameObject.SetActive(boosterPriceConfig.typeBuy == 1 && !isOverAds && isShowAdsBtn);
            getIfAdsBtnLargeGray.gameObject.SetActive(boosterPriceConfig.typeBuy == 1 && isOverAds && isShowAdsBtn);
            getIfMoneyBtnLarge.gameObject.SetActive(boosterPriceConfig.typeBuy == 2 || !isShowAdsBtn);
            if (BoosterManager.NumWatchAdsDailyConfig > 0)
            {
                txtAmountAds.gameObject.SetActive(true);
                txtAmountAds.SetValue($"{BoosterManager.NumWatchAdsDaily}/{BoosterManager.NumWatchAdsDailyConfig}");
            }
            else
            {
                txtAmountAds.gameObject.SetActive(false);
            }
        }
    }
    public void FreeButton()
    {
        isFreeNow = false;
        var amount = BoosterManager.Instance.GetNumBooster(boosterInfo.type);
        var logName = LogEvent.GetBoosterLogName(boosterInfo.type);
        string positionLog = "ui_buy_" + logName;
        BoosterManager.Instance.AddBooster(boosterInfo.type, amount, positionLog, LogEvent.ReasonItem.purchase);
        onBuySuccess?.Invoke();
        Hide();
    }
    private void OnClickBuyIfMoney()
    {
        int price = BoosterManager.Instance.GetPrice(boosterInfo.type);
        if (GameRes.getRes(RES_type.GOLD) >= price)
        {
            var logName = LogEvent.GetBoosterLogName(boosterInfo.type);
            string positionLog = "ui_buy_" + logName;

            var amount = BoosterManager.Instance.GetNumBooster(boosterInfo.type);
            DataManager.Instance.ReceiveGift(false, 0, positionLog, LogEvent.ReasonItem.purchase, false, -1, new ItemInfo(null, RES_type.GOLD, -price));
            BoosterManager.Instance.AddBooster(boosterInfo.type, amount, positionLog, LogEvent.ReasonItem.exchange);
            onBuySuccess?.Invoke();
            
            FIRhelper.logEvent($"booster_get_{logName}");
            FIRhelper.logEvent($"booster_get_{logName}_ticket");
            FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_get_{logName}");
            FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_get_{logName}_ticket");
            Hide();
        }
        else
        {
            gameObject.SetActive(false);
            UIShopPopup shop = UIManager.Instance.ShowPopup<UIShopPopup>(() =>
            {
                gameObject.SetActive(true);
            });
            shop.GetComponent<SafeAreaPanel>().enabled = true;
            shop.ScrollToTarget();
        }
    }

    private void OnClickBuyIfAds()
    {
        AudioManager.Instance.SetCacheAudio();
        var ss = AdsHelper.Instance.showGift("ui_booster", GameRes.GetLevel(), false, state =>
        {
            if (state == AD_State.AD_REWARD_OK || state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 || state == AD_State.AD_SHOW_MISS_CB || state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
            {
                AudioManager.Instance.ResetAudio();
            }
            if (state == AD_State.AD_REWARD_OK)
            {
                var logName = LogEvent.GetBoosterLogName(boosterInfo.type);
                string positionLog = "ui_buy_" + logName;

                BoosterManager.NumWatchAds++;
                BoosterManager.NumWatchAdsDaily++;
                var amount = BoosterManager.Instance.GetNumBooster(boosterInfo.type);
                BoosterManager.Instance.AddBooster(boosterInfo.type, amount, positionLog, LogEvent.ReasonItem.watch_ads);
                onBuySuccess?.Invoke();
                
                FIRhelper.logEvent($"booster_get_{logName}");
                FIRhelper.logEvent($"booster_get_{logName}_ads");

                FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_get_{logName}");
                FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_get_{logName}_ads");
                Hide();
            }
        });

        if (ss < 0) UIManager.Instance.NotifyContent(content: "No Ads Available!", key: "_no_ads_available");
        //LogEventManager.Instance.LogAds("gift", "ui_booster", ss >= 0 ? 1 : 0);
    }


    public override void Hide()
    {
        DOTween.Kill(this);
        base.Hide();
    }
}