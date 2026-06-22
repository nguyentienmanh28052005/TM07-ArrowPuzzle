using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Crystal;
using DG.Tweening;
using master;
using mygame.sdk;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIRevive : PopupUI
{
    [Serializable]
    public class ReviveInfo
    {
        public BoosterType type;
        public ReviveType reviveType;
        public string desc;
        public string header_desc;
        public GameObject info;
    }

    [SerializeField] private Button getIfAdsBtn;
    [SerializeField] private Button getIfAdsBtnGray;
    [SerializeField] private Button getIfMoneyBtn;
    [SerializeField] private Button getIfMoneyBtnLarge;

    [SerializeField] private Text descText;
    [SerializeField] private Text headerText;
    [SerializeField] private Text priceValue;
    [SerializeField] private Text priceValueLarge;
    [SerializeField] private Text txtAmountAds;
    [SerializeField] private RectTransform revivePanel;
    [SerializeField] private ReviveInfo[] reviveInfos;
    [SerializeField] RectTransform rectRevive;
    [SerializeField] RectTransform rectMain;
    [SerializeField] GameObject visualNormal;
    [SerializeField] GameObject visualHeart;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] EventTrigger eventTriggerPanel;
    [SerializeField] UIPackage revivePackage;
    [SerializeField] private Animation openAnim;
    Shadow[] shadows;
    SkeletonGraphic[] skeletonGraphics;
    bool isFadeIn = true;
    private bool isShowGift;

    private static int CF_Price_Revive => PlayerPrefs.GetInt("cf_revive_price", 900);

    private static int CF_Price_Revive_Lose_Increase => PlayerPrefs.GetInt("cf_revive_price_lose_increase", 0);

    private static int LEVEL_REMOVE_ADS_BUTTON => AdsRewardConfig.GetConfig().levelRemoveAdsRevive;
    private static int LEVEL_ACTIVE_ADS_REVIVE => AdsRewardConfig.GetConfig().levelActiveAdsRevive;

    static int Price_Revive => CF_Price_Revive + CF_Price_Revive_Lose_Increase * LevelManager.Count_Revive;

    public static int Count_Revive_Ads
    {
        get => PlayerPrefs.GetInt("count_revive_ads", 0);
        set => PlayerPrefs.SetInt("count_revive_ads", value);
    }
    private ReviveInfo boosterInfo;
    private Action<ReviveType> onBuySuccess;

    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        getIfAdsBtn.onClick.AddListener(OnClickReviveIfAds);
        getIfMoneyBtn.onClick.AddListener(OnClickReviveIfMoney);
        getIfMoneyBtnLarge.onClick.AddListener(OnClickReviveIfMoney);
        getIfAdsBtnGray.onClick.AddListener(() =>
        {
            UIManager.Instance.NotifyContent("", "_reach_limit_ads");
        });

        // if (!EventBattlePassManager.Instance.IsEventActive() || EventBattlePassManager.Instance.IsActiveVip()) y -= 115;

        //rectRevive.anchoredPosition = new Vector2(0, y);

        //rectMain.anchoredPosition = new Vector2(0, 1300);
        //rectMain.DOLocalMoveY(0, .665f).SetEase(Ease.OutBack);

        eventTriggerPanel.triggers.Clear();

        EventTrigger.Entry downEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        downEntry.callback.AddListener((data) =>
        {
            FadeOut();
        });
        eventTriggerPanel.triggers.Add(downEntry);

        EventTrigger.Entry upEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        upEntry.callback.AddListener((data) =>
        {
            FadeIn();
        });
        eventTriggerPanel.triggers.Add(upEntry);
        revivePackage.onBuySuccess += () =>
        {
            gameObject.SetActive(false);
        };
        revivePackage.onBuySuccess += OnRevive;
        //EventTrigger.Entry exitEntry = new EventTrigger.Entry
        //{
        //    eventID = EventTriggerType.PointerExit
        //};
        //exitEntry.callback.AddListener((data) =>
        //{
        //    FadeIn();
        //});
        //eventTriggerPanel.triggers.Add(exitEntry);
    }
    public override void Show(Action onClose)
    {
        base.Show(onClose);
        visualNormal.gameObject.SetActive(true);
        visualHeart.gameObject.SetActive(false);

        if (BoosterManager.IsOverAds)
        {
            Debug.Log("Over Ads Level");
        }
        if (BoosterManager.IsOverAdsDaily)
        {
            Debug.Log("Over Ads Daily");
        }
        if (Count_Revive_Ads >= PlayerPrefsUtil.CF_NumReviveShowAds)
        {
            Debug.Log("Over Ads Revive");
        }
        if ((DataManager.Level >= LEVEL_REMOVE_ADS_BUTTON && LEVEL_REMOVE_ADS_BUTTON > 0) || DataManager.Level < LEVEL_ACTIVE_ADS_REVIVE)
        {
            Debug.Log("Over ads Level Revive");
        }
        bool isOverAdsRevive = false;
        if (Count_Revive_Ads >= PlayerPrefsUtil.CF_NumReviveShowAds && PlayerPrefsUtil.CF_NumReviveShowAds > 0)
        {
            isOverAdsRevive = true;
            Debug.Log("Over Ads Revive");
        }
        bool isOverAds = BoosterManager.IsOverAds || BoosterManager.IsOverAdsDaily;
        bool isShowAdsBtn = (DataManager.Level < LEVEL_REMOVE_ADS_BUTTON || LEVEL_REMOVE_ADS_BUTTON <= 0) && DataManager.Level > LEVEL_ACTIVE_ADS_REVIVE && !isOverAdsRevive && !isOverAds;

        getIfAdsBtn.gameObject.SetActive(isShowAdsBtn);
        getIfAdsBtnGray.gameObject.SetActive(/*isOverAds && isShowAdsBtn*/ false);
        getIfMoneyBtn.gameObject.SetActive(isShowAdsBtn);
        //getIfAdsBtnLarge.gameObject.SetActive(!isOverAds && isShowAdsBtn);
        //getIfAdsBtnLargeGray.gameObject.SetActive(isOverAds && isShowAdsBtn);
        getIfMoneyBtnLarge.gameObject.SetActive(!isShowAdsBtn);
        if (BoosterManager.NumWatchAdsDailyConfig > 0)
        {
            txtAmountAds.gameObject.SetActive(true);
            txtAmountAds.SetValue($"{BoosterManager.NumWatchAdsDaily}/{BoosterManager.NumWatchAdsDailyConfig}");
        }
        else
        {
            txtAmountAds.gameObject.SetActive(false);
        }

        if (Screen.height / (float)Screen.width > 1.8f)
        {
            openAnim.Play("PopupRevive");
        }
        else
        {
            openAnim.Play("PopupRevive_Short");
        }
    }

    public void Initialized(ReviveType type, Action<ReviveType> success)
    {
        var info = GetInfo(type);
        for (int i = 0; i < reviveInfos.Length; i++)
        {
            reviveInfos[i].info.SetActive(reviveInfos[i].reviveType == type);
        }
        descText.SetText(info.desc);
        headerText.SetText(info.header_desc);
        // iconPrice.sprite = DataManager.Instance.GetIcon(info.price.resType);
        priceValue.text = Price_Revive.ToString();
        priceValueLarge.text = Price_Revive.ToString();
        boosterInfo = info;
        onBuySuccess = success;
    }
    public void SetTextHeader(string key)
    {
        headerText.SetText(key);
    }
    private ReviveInfo GetInfo(ReviveType type)
    {
        return reviveInfos.SingleOrDefault(x => x.reviveType == type);
    }



    public void OnRevive()
    {
        //BoosterManager.Instance.AddBooster(boosterInfo.type, 1, "ui_buy_booster", LogEvent.ReasonItem.exchange);
        LogEvent.LevelSecondChance(DataManager.Instance.ConsecutiveLose, boosterInfo.type.ToString(), LevelManager.Instance.GetProgress(), (int)((Time.time - LevelManager.Instance.timeStartLevel) * 1000), bonusAmount:1);
        FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_revive");
        FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_revive_buy_pack");
        onBuySuccess?.Invoke(boosterInfo.reviveType);
        onBuySuccess = null;
        Hide();
    }
    private void OnClickReviveIfMoney()
    {
        if (isShowGift) return;
        if (GameRes.getRes(RES_type.GOLD) >= Price_Revive)
        {
            var logName = LogEvent.GetBoosterLogName(boosterInfo.type);
            string positionLog = "ui_buy_" + logName;

            DataManager.Instance.ReceiveGift(false, 0, "ui_revive", LogEvent.ReasonItem.purchase, false, -1, new ItemInfo(null, RES_type.GOLD, -Price_Revive));
            if (boosterInfo.type != BoosterType.None) BoosterManager.Instance.AddBooster(boosterInfo.type, 1, positionLog, LogEvent.ReasonItem.exchange);
            LogEvent.LevelSecondChance(DataManager.Instance.ConsecutiveLose, boosterInfo.type.ToString(), LevelManager.Instance.GetProgress(), (int)((Time.time - LevelManager.Instance.timeStartLevel) * 1000), bonusAmount:1);
            FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_revive");
            FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_revive_ticket");
            onBuySuccess?.Invoke(boosterInfo.reviveType);
            onBuySuccess = null;
            Hide();
        }
        else
        {
            gameObject.SetActive(false);
            var UIShopPopup = UIManager.Instance.ShowPopup<UIShopPopup>(() =>
            {
                gameObject.SetActive(true);
            });
            UIShopPopup.GetComponent<SafeAreaPanel>().enabled = true;
            UIShopPopup.SetShowPosition(LogEvent.IAP_ShowPosition.shop_popup);
            UIShopPopup.ScrollToTarget();
        }
    }

    private void OnClickReviveIfAds()
    {
        AudioManager.Instance.SetCacheAudio();
        var ss = AdsHelper.Instance.showGift("ui_revive", GameRes.GetLevel(), false, state =>
        {
            if (state == AD_State.AD_REWARD_OK || state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 || state == AD_State.AD_SHOW_MISS_CB || state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
            {
                AudioManager.Instance.ResetAudio();
            }

            if (state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2)
            {
                isShowGift = false;
            }

            if (state == AD_State.AD_REWARD_OK)
            {
                BoosterManager.NumWatchAds++;
                BoosterManager.NumWatchAdsDaily++;

                var logName = LogEvent.GetBoosterLogName(boosterInfo.type);
                string positionLog = "ui_buy_" + logName;

                if (boosterInfo.type != BoosterType.None) BoosterManager.Instance.AddBooster(boosterInfo.type, 1, positionLog, LogEvent.ReasonItem.exchange);
                LogEvent.LevelSecondChance(DataManager.Instance.ConsecutiveLose, boosterInfo.type.ToString(), LevelManager.Instance.GetProgress(), (int)((Time.time - LevelManager.Instance.timeStartLevel) * 1000), bonusAmount:1);
                FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_revive");
                FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_revive_ads");
                onBuySuccess?.Invoke(boosterInfo.reviveType);
                onBuySuccess = null;
                Count_Revive_Ads++;
                Hide();
            }
        });

        isShowGift = ss >= 0;
        if (ss < 0) UIManager.Instance.NotifyContent(content: "No Ads Available!", key: "_no_ads_available");
        //LogEventManager.Instance.LogAds("gift", "ui_revive", ss >= 0 ? 1 : 0);
        DOVirtual.DelayedCall(2f, () => isShowGift = false);
    }

    public override void OnClickClose()
    {
        if (visualNormal.gameObject.activeSelf && PlayerPrefsUtil.CF_Revive2Time)
        {
            visualNormal.gameObject.SetActive(false);
            visualHeart.gameObject.SetActive(true);
        }
        else
        {
            onQuit();
        }
        return;



        void onQuit()
        {
            LevelManager.Instance.OnFail();
            AudioManager.Instance.SetCacheAudio();
            base.OnClickClose();
            //var ss = AdsHelper.Instance.showFull("ui_revive", GameRes.GetLevel(), DataManager.Instance.ConsecutiveLose, 0, 0, false, false, cb: state =>
            //{
            //    if (state == AD_State.AD_REWARD_OK || state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 || state == AD_State.AD_SHOW_MISS_CB || state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
            //    {
            //        AudioManager.Instance.ResetAudio();
            //    }

            //    if (state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 || state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
            //    {
            //        //GameManager.Instance.TransitionBackToHome(ReasonBackToHome.Lose, () => { });
            //    }
            //});
            //if (!ss)
            //{
            //    base.OnClickClose();
            //}
        }
    }
    Tween tweenShadow;
    Sequence sequence;
    Tween delayFade;
    void FadeOut()
    {
        if (!isFadeIn)
        {
            return;
        }
     
        if (canvasGroup != null)
        {
            isFadeIn = false;
            if (delayFade != null) delayFade.Kill();
            delayFade = DOVirtual.DelayedCall(.1f, () =>
            {
                canvasGroup.DOKill();
                canvasGroup.DOFade(0, .3f).SetEase(Ease.Linear).SetId(this);
                if (tweenShadow != null)
                {
                    tweenShadow.Kill();
                }
                shadows = GetComponentsInChildren<Shadow>();
                skeletonGraphics = GetComponentsInChildren<SkeletonGraphic>();
                if (sequence != null)
                {
                    sequence.Kill();
                }
                sequence = DOTween.Sequence().SetId(this);
                foreach (var s in shadows)
                {
                    sequence.Insert(0, s.DOFade(0, .2f).SetEase(Ease.Linear));
                }
                foreach (var s in skeletonGraphics)
                {
                    sequence.Insert(0, s.DOFade(0, .3f).SetEase(Ease.OutQuad));
                }
            }).SetId(this);


        }
    }
    void FadeIn()
    {
        if (isFadeIn)
        {
            return;
        }
        if (canvasGroup != null)
        {
            isFadeIn = true;
            if (delayFade != null) { delayFade.Kill(); }

            canvasGroup.DOKill();
            canvasGroup.DOFade(1, .35f).SetEase(Ease.Linear).SetId(this);
            if (tweenShadow != null)
            {
                tweenShadow.Kill();
            }
            if (sequence != null)
            {
                sequence.Kill();
            }
            sequence = DOTween.Sequence().SetId(this);
            shadows = GetComponentsInChildren<Shadow>();
            skeletonGraphics = GetComponentsInChildren<SkeletonGraphic>();
            foreach (var s in shadows)
            {
                sequence.Insert(0, s.DOFade(1, .175f).SetEase(Ease.Linear));
            }
            foreach (var s in skeletonGraphics)
            {
                sequence.Insert(0, s.DOFade(1, .25f).SetEase(Ease.InQuad));
            }

        }
    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}

public enum ReviveType
{
    None,
    ExtraSlot,
}
