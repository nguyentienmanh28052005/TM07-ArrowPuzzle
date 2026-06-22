using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using Crystal;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using mygame.sdk;
using time;



public class UIGuildMoreLives : PopupUI
{
    [Space, Header("UI")] [SerializeField] Button btn_Close;
    [SerializeField] Button btn_FreeLives;
    [SerializeField] Button btn_FreeLivesAds;
    [SerializeField] Button btn_Refill;
    [SerializeField] Text txt_HeartCurrent;
    [SerializeField] Text txt_des;
    [SerializeField] Text txt_TimeRecoverHeart;
    [SerializeField] Text txt_NotifyHeart;
    [SerializeField] Text txt_gold;
    [SerializeField] Text txt_refill;
    [SerializeField] Text txt_AdsFree;
    [SerializeField] GameObject topBar;
    [SerializeField] GameObject obj_Infinity;
    [SerializeField] GameObject obj_NotifyHeart;
    [SerializeField] RectTransform rectMoreLive;
    [SerializeField] RectTransform rectBG;
    [SerializeField] GameObject offerObject;
    private IDisposable subHeartReceive;
    private bool isInGame;

    private void Start()
    {
        subHeartReceive = HeartManager.HeartReceive.Subscribe(x =>
        {
            txt_NotifyHeart.SetValue(x.listDonater.Count);
            obj_NotifyHeart.SetActive(x.listDonater.Count > 0);
        });
    }

    private void OnDestroy()
    {
        subHeartReceive.Dispose();
    }

    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        btn_Close.onClick.AddListener(Hide);
        btn_FreeLives.onClick.AddListener(FreeLives);
        btn_Refill.onClick.AddListener(Refill);
        btn_FreeLivesAds.onClick.AddListener(FreeLivesAds);
        GameManager.Instance.HideBanner();
    }

    public override void Show(Action onClose)
    {
        base.Show(onClose);
        isInGame = uiManager.GetScreenActive<UIInGame>() != null;
        var cannotAddHeart = HeartManager.Instance.CannotAddHeart;
        var config = AdsRewardConfig.GetConfig();
        var maxAds = Mathf.Max(HeartManager.CountWatchAdsRefillHeartEachDay, config.countWatchAdsRefillHeartEachDay);
        bool isShowAds = !cannotAddHeart &&  HeartManager.CountWatchAdsRefillHeartEachDay < maxAds && (config.levelRemoveAdsBuyHeart == 0 || config.levelRemoveAdsBuyHeart > DataManager.Level) && DataManager.Level > config.levelActiveAdsBuyHeart;
        topBar.SetActive(UIManager.Instance.CurrentScreen is UIInGame);
        if (isShowAds)
        {
            rectBG.sizeDelta = new Vector2(rectBG.sizeDelta.x, 1380);
        }
        else
        {
            rectBG.sizeDelta = new Vector2(rectBG.sizeDelta.x, 1125);
        }
        
        var sizeRate = Screen.height / (float)Screen.width;
        if (SdkUtil.isiPad())
        {
            offerObject.SetActive(!isShowAds);
            rectMoreLive.anchoredPosition = new Vector2(0, !isShowAds ? 175 : 0);
        }
        else if (sizeRate > 2f)
        {
            offerObject.SetActive(true);
            rectMoreLive.anchoredPosition = new Vector2(0, 140);
        }
        else if (sizeRate > 1.8f)
        {
            offerObject.SetActive(true);
            rectMoreLive.anchoredPosition = new Vector2(0, 170);
        }
        else
        {
            offerObject.SetActive(false);
            rectMoreLive.anchoredPosition = Vector2.zero;
        }

        btn_FreeLives.gameObject.SetActive(isShowAds);
        btn_FreeLivesAds.gameObject.SetActive(isShowAds);
        txt_gold.SetValue(HeartManager.CF_GoldBuyHeart);
        txt_refill.SetText("_refill");
        txt_AdsFree.text = $"{HeartManager.CountWatchAdsRefillHeartEachDay}/{maxAds}";
        LayoutRebuilder.ForceRebuildLayoutImmediate(txt_gold.transform.parent.GetComponent<RectTransform>());
    }

    private void Update()
    {
        isInGame = uiManager.GetScreenActive<UIInGame>() != null;
        if (HeartManager.Instance.IsUnlimitedTime)
        {
            obj_Infinity.SetActive(true);
            txt_HeartCurrent.gameObject.SetActive(false);
            txt_des.SetText("unlimited_lives_in");
        }
        else
        {
            txt_des.SetText("time_to_next_live");
            obj_Infinity.SetActive(false);
            txt_HeartCurrent.gameObject.SetActive(true);
            if (isInGame)
            {
                txt_HeartCurrent.SetValue(Math.Clamp(HeartManager.Heart + 1, 0, HeartManager.MAX_HEART));
            }
            else
            {
                txt_HeartCurrent.SetValue(HeartManager.Heart);
            }
        }

        txt_TimeRecoverHeart.SetValue(
            HeartManager.Instance.GetTimeRemaningText(isInGame
                ? Mathf.Min(HeartManager.Heart + 1, HeartManager.MAX_HEART)
                : -1));
    }

    public void FreeLivesAds()
    {
        var heart = isInGame ? HeartManager.HeartInGame : HeartManager.Heart;
        if (heart >= HeartManager.MAX_HEART)
        {
            UIManager.Instance.NotifyContent("", key: "noti_full_receive_heart");
            return;
        }

        AudioManager.Instance.SetCacheAudio();
        var ss = AdsHelper.Instance.showGift("ui_more_lives", GameRes.GetLevel(), false, state =>
        {
            if (state == AD_State.AD_REWARD_OK || state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 ||
                state == AD_State.AD_SHOW_MISS_CB || state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
            {
                AudioManager.Instance.ResetAudio();
            }

            if (state == AD_State.AD_REWARD_OK)
            {
                HeartManager.CountWatchAdsRefillHeartEachDay++;
                HeartManager.Instance.AddHeart(1, "ui_more_lives", LogEvent.ReasonItem.watch_ads);
                FIRhelper.logEvent($"heart_get_ads");
                FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_get_heart");
                FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_get_heart_ads");
                Hide();
            }
        });

        if (ss < 0) UIManager.Instance.NotifyContent(content: "No Ads Available!", key: "_no_ads_available");
    }

    public void FreeLives()
    {
        // Guild module removed - GuildManager not available
        UIManager.Instance.NotifyContent("", "coming_soon");
    }

    /*public override void Hide()
    {
        if(GameManager.GameSate != GameSate.None)
        GameManager.Instance.ShowBanner();
        base.Hide();
    }*/

    public void Refill()
    {
        if (HeartManager.Instance.IsUnlimitedTime)
        {
            UIManager.Instance.NotifyContent("", "noti_infinity_heart");
            return;
        }

        int cf_Gold = HeartManager.CF_GoldBuyHeart;
        var inGame = GameManager.GameState != GameState.None;
        if ((inGame ? HeartManager.Heart + 1 : HeartManager.Heart) < HeartManager.MAX_HEART)
        {
            if (DataManager.Gold >= cf_Gold)
            {
                DataManager.Instance.OnSinkResource(LogEvent.ReasonItem.use, "refill_heart",
                    new DataResource[] { new() { resType = RES_type.GOLD, amount = -cf_Gold } }, DataManager.Level);
                IResourceTarget.OnItemChange?.Invoke(RES_type.GOLD, .5f);
                HeartManager.Instance.AddHeart(HeartManager.MAX_HEART - HeartManager.Heart, "ui_more_lives",
                    LogEvent.ReasonItem.exchange);
                Hide();
            }
            else
            {
                gameObject.SetActive(false);
                var UIShopPopup = UIManager.Instance.ShowPopup<UIShopPopup>(() => { gameObject.SetActive(true); });
                UIShopPopup.GetComponent<SafeAreaPanel>().enabled = true;
                UIShopPopup.SetShowPosition(LogEvent.IAP_ShowPosition.shop_popup);
                UIShopPopup.ScrollToTarget();
                // show shop (or mini shop)
            }
        }
        else
        {
            UIManager.Instance.NotifyContent("", "noti_full_receive_heart");
        }
    }
}