using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using EventGame;
using mygame.sdk;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class UILevelComplete : PopupUI
{
    [SerializeField] private Button nextLevelBtn;
    [SerializeField] private Button nextLevelBtnLarge;
    [SerializeField] private Button claimX2Btn;
    [SerializeField] private Text levelText;
    [SerializeField] private Text txtGoldNextLevel;
    [SerializeField] private Text txtGoldX2;
    [SerializeField] Text txtMode;
    [SerializeField] private Text txtGoldNextLevelLarge;
    [SerializeField] SkeletonGraphic skeletonTitle;
    [SerializeField] SkeletonGraphic levelChestSkeleton;
    [SerializeField] SkeletonGraphic skeletonChest;
    [SerializeField] BarController barController;
    [SerializeField] Text txt_Claimx2;
    [SerializeField] Button skipBtn;
    [SerializeField] ItemDisplay goldDisplay;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Slider chessProgressSlider;
    [SerializeField] RectTransform btnFake;
    [SerializeField] RectTransform claimChestPanel;
    [SerializeField] private Text txtProgress;
    [SerializeField] private GameObject fxChest;
    [SerializeField] private GameObject blockPanel;
    [SerializeField] RectTransform holderRect;
    [SerializeField] private AutoChangeTheme levelTypeModifier;
    private DataResource[] rewards;

    private bool isShowGift;
    private int level;
    private int multiplier;
    private event Action cbOnSkip;
    public static int cacheCoinGet;

    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        nextLevelBtn.onClick.AddListener(OnClickNextLevel);
        nextLevelBtnLarge.onClick.AddListener(OnClickNextLevel);
        claimX2Btn.onClick.AddListener(OnClickX2Reward);
        skipBtn.onClick.AddListener(SkipButton);
        claimChestPanel.gameObject.SetActive(false);

        if (Screen.safeArea.yMax < Screen.height)
        {
            //     // bgLevel.anchorMin = Vector2.one;
            //     // bgLevel.anchorMax = Vector2.one;
            //     //bgLevel.anchoredPosition = new Vector2(0, 0);
            //     // bgLevel.GetComponent<Image>().enabled = false;
            //
            //     var myTransform = GetComponent<RectTransform>();
            //     var safeArea = Screen.safeArea;
            //     var screen = new Vector2(Screen.width, Screen.height);
            //
            //     // 1. Setup and apply safe area
            //     var h = (screen.y - (screen.y - safeArea.yMax)) / screen.y;
            //     var heightArea = h + (1 - h) / 2f;
            //     myTransform.anchorMax = new Vector2(1, heightArea);
            goldDisplay.GetComponent<RectTransform>().anchoredPosition = new Vector2(-25, -38);
        }
    }

    private void Awake()
    {
        skeletonChest.AnimationState.Event += HandleSpineEvent;
    }

    public override void Show(Action onClose)
    {
        base.Show(onClose);
        LogEvent.ScreenGo(LogEvent.ScreenName.LevelWin);
        level = GameRes.GetLevel() - 1;
        skipBtn.gameObject.SetActive(false);
        levelChestSkeleton.Initialize(true);
        levelChestSkeleton.AnimationState.ClearTracks();
        levelChestSkeleton.Skeleton.SetSlotsToSetupPose();
        skeletonTitle.Initialize(false);
        skeletonTitle.AnimationState.ClearTracks();
        skeletonChest.Initialize(false);
        skeletonChest.AnimationState.ClearTracks();
        skeletonChest.Skeleton.SetSlotsToSetupPose();
        skeletonTitle.Skeleton.SetSlotsToSetupPose();
        skeletonTitle.AnimationState.SetAnimation(0, "action", false);
        //skeletonTitle.AnimationState.AddAnimation(0, "idle", false, 0);
        skeletonChest.AnimationState.SetAnimation(0, "Idle", false);
        skeletonChest.AnimationState.AddAnimation(0, "Idle2", true, 0);
        canvasGroup.alpha = 1;
        nextLevelBtn.gameObject.SetActive(true);
        claimX2Btn.gameObject.SetActive(true);
        claimX2Btn.interactable = true;
        nextLevelBtn.interactable = true;
        nextLevelBtnLarge.interactable = true;
        fxChest.SetActive(false);
        var config = AdsRewardConfig.GetConfig();
        bool isShowAdsBtn = (config.levelRemoveAdsWinUI == 0 || config.levelRemoveAdsWinUI > level) &&
                            level > config.levelActiveAdsWinUI;
        var levelType = LevelManager.GetLevelType(level);
        var indexMulti = Math.Clamp((int)levelType, 0, config.rewardAdsWinMulti.Length - 1);
        multiplier = config.rewardAdsWinMulti[indexMulti];
        nextLevelBtnLarge.gameObject.SetActive(!isShowAdsBtn);
        nextLevelBtn.gameObject.SetActive(isShowAdsBtn);
        claimX2Btn.gameObject.SetActive(isShowAdsBtn);
        skeletonTitle.gameObject.SetActive(false);
        //GetComponent<UnAnimation>().Play();
        //nextLevelBtn.GetComponent<CanvasGroup>().alpha = 1;
        //claimX2Btn.GetComponent<CanvasGroup>().alpha = 1;
        if (DataManager.Level == PlayerPrefsUtil.CF_LevelShowRate)
        {
            SDKManager.Instance.showRate(() => {});
        }

        var offsetMin = holderRect.offsetMin;
        offsetMin.y = -200;
        holderRect.offsetMin = offsetMin;
        // var coinPiggy = LevelManager.Instance.SessionGold;
        // if (coinPiggy > 0)
        // {
        //     RewardReceivedHub.Instance.CoinFly(new Vector2(Screen.width/2,Screen.height/2), null, Mathf.Min(coinPiggy,3), null, coinPiggy);
        //     RewardReceivedHub.AddCacheValue(RES_type.GOLD, -coinPiggy);
        //     LevelManager.Instance.SessionGold = 0;
        // }
    }


    public void Initialized(DataResource[] dataResources)
    {
        rewards = new DataResource[dataResources.Length];
        Array.Copy(dataResources, rewards, dataResources.Length);
        var goldIdx = dataResources.IndexOf(x => x.resType == RES_type.GOLD);
        var gold = new DataResource(rewards[goldIdx]);
        rewards[goldIdx] = gold;

        level = GameRes.GetLevel() - 1;

        var currentLevelType = LevelManager.GetLevelType(level);
        if (levelTypeModifier != null)
        {
            levelTypeModifier.ApplyTheme(currentLevelType);
        }
        var arr = new string[3]
        {
            "",
            "HARD",
            "CRAZY",
        };

        txtMode.text = arr[(int) currentLevelType];

        txtGoldNextLevel.text = gold.amount.ToString();
        txtGoldNextLevelLarge.text = gold.amount.ToString();
        //goldX2Btn.text = (gold.itemAmount * 2).ToString();
        levelText.text = $"Level {level}";
        //barController.StartArrow(txtGoldX2, txt_Claimx2, gold.amount);
        txt_Claimx2.SetText("_claim_x", StateCapText.None, FormatText.F_String, multiplier, false);
        isShowGift = false;
        var config = AdsRewardConfig.GetConfig();

        bool isShowAdsBtn = (config.levelRemoveAdsWinUI == 0 || config.levelRemoveAdsWinUI > level) &&
                            level > config.levelActiveAdsWinUI;
        barController.gameObject.SetActive(!barController.isOnlyValue() && isShowAdsBtn);

        if (!barController.gameObject.activeSelf)
        {
            var offsetMin = holderRect.offsetMin;
            offsetMin.y = 0;
            holderRect.offsetMin = offsetMin;
        }
    }

    private void OnClickNextLevel()
    {
        if (isShowGift) return;
        fxChest.SetActive(false);
        skipBtn.gameObject.SetActive(true);
        barController.isStop = true;
        FIRhelper.logEvent($"level_{level:0000}_complete_next");
        //GameEvent.OnReceiveResource?.Invoke(LogEvent.ReasonItem.reward, "ui_complete", rewards, level);
        var goldIdx = rewards.IndexOf(x => x.resType == RES_type.GOLD);
        if (cacheCoinGet > 0)
        {
            RewardReceivedHub.AddCacheValue(RES_type.GOLD, -cacheCoinGet);
            cacheCoinGet = 0;
        }

        int goldVisual = rewards[goldIdx].amount;
        if (nextLevelBtn.gameObject.activeSelf)
        {
            RewardReceivedHub.Instance.CoinFly(
                UIManager.Instance.canvas.worldCamera.WorldToScreenPoint(nextLevelBtn.transform.position), null, 6,
                null, goldVisual);
            //RewardReceivedHub.Instance.ShowPopTextReward(rewards[goldIdx], UIManager.Instance.canvas.worldCamera.WorldToScreenPoint(nextLevelBtn.transform.position + Vector3.up), 1);
        }
        else
        {
            RewardReceivedHub.Instance.CoinFly(
                UIManager.Instance.canvas.worldCamera.WorldToScreenPoint(nextLevelBtnLarge.transform.position), null, 6,
                null, goldVisual);
            //RewardReceivedHub.Instance.ShowPopTextReward(rewards[goldIdx], UIManager.Instance.canvas.worldCamera.WorldToScreenPoint(nextLevelBtnLarge.transform.position + Vector3.up), 1);
        }

        AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Pop_Reward);
        nextLevelBtn.interactable = false;
        claimX2Btn.interactable = false;
        nextLevelBtnLarge.interactable = false;
        //nextLevelBtn.GetComponent<CanvasGroup>().alpha = .5f;
        //claimX2Btn.GetComponent<CanvasGroup>().alpha = .5f;
        //nextLevelBtn.gameObject.SetActive(false);
        //claimX2Btn.gameObject.SetActive(false);
        cbOnSkip = CompleteAction;
        DOVirtual.DelayedCall(2f, () =>
        {
            skipBtn.gameObject.SetActive(false);
            CompleteAction();
        }).SetId(this);

        void CompleteAction()
        {
            if (GameRes.GetLevel() < PlayerPrefsUtil.CF_FirstLevelShowMain ||
                UserDataManager.Instance.UserData.section_login < PlayerPrefsUtil.CF_SectionShowMain)
            {
                HeartManager.Instance.IsNoMoreLives(
                    () => { GameManager.Instance.TransitionBackToHome(ReasonBackToHome.Win, Hide); }, () =>
                    {
                        PlayGame();
                        Hide();
                    });

                return;

                void PlayGame()
                {
                    if (cacheCoinGet > 0)
                    {
                        RewardReceivedHub.AddCacheValue(RES_type.GOLD, -cacheCoinGet);
                        cacheCoinGet = 0;
                    }

                    IResourceTarget.OnItemChange?.Invoke(RES_type.GOLD, 0);
                    HeartManager.Instance.AddHeart(-1, "play_game", LogEvent.ReasonItem.use);
                    GameManager.Instance.NextLevel();
                    if (PlayerPrefsUtil.CF_ShowAdsComplete == 1)
                    {
                        AudioManager.Instance.SetCacheAudio();
                        AdsHelperWrapper.ShowFull("ui_complete_at");
                        //var ss = AdsHelper.Instance.showFull("ui_complete_at", level, -1,0, 0, false, false, true, state =>
                        //{
                        //    if (state == AD_State.AD_REWARD_OK || state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 || state == AD_State.AD_SHOW_MISS_CB || state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
                        //    {
                        //        AudioManager.Instance.ResetAudio();
                        //    }
                        //});
                    }
                }
            }
            else
            {
                // cacheCoinGet = rewards[goldIdx].amount;
                // RewardReceivedHub.AddCacheValue(RES_type.GOLD, cacheCoinGet);
                if (PlayerPrefsUtil.CF_ShowAdsComplete == 1)
                {
                    AudioManager.Instance.SetCacheAudio();
                    AdsHelperWrapper.ShowFull("ui_complete_at",
                        (isShowAds) => { GameManager.Instance.TransitionBackToHome(ReasonBackToHome.Win, Hide); },
                        onPopupBreakAdsClose: () =>
                        {
                            GameManager.Instance.TransitionBackToHome(ReasonBackToHome.Win, Hide);
                        });
                    //var ss = AdsHelper.Instance.showFull("ui_complete_at", level, -1,0, 0, false, false,cb: (state) =>
                    //{
                    //    if (state == AD_State.AD_REWARD_OK || state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 || state == AD_State.AD_SHOW_MISS_CB || state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
                    //    {
                    //        AudioManager.Instance.ResetAudio();
                    //    }
                    //    if(state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 || state == AD_State.AD_SHOW_FAIL|| state == AD_State.AD_SHOW_FAIL2)
                    //    {
                    //        GameManager.Instance.TransitionBackToHome(ReasonBackToHome.Win, Hide);
                    //    }
                    //});
                    //if (ss == false)
                    //{
                    //    GameManager.Instance.TransitionBackToHome(ReasonBackToHome.Win, Hide);
                    //}
                }
                else
                {
                    GameManager.Instance.TransitionBackToHome(ReasonBackToHome.Win, Hide);
                }
            }
        }
    }

    private void OnClickX2Reward()
    {
        barController.isStop = true;
        bool CheckOk = false;
        AudioManager.Instance.SetCacheAudio();
        var ss = AdsHelper.Instance.showGift("ui_complete", level, false, state =>
        {
            if (state == AD_State.AD_REWARD_OK || state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 ||
                state == AD_State.AD_SHOW_MISS_CB || state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
            {
                AudioManager.Instance.ResetAudio();
            }

            if (state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2)
            {
                isShowGift = false;
                if (!CheckOk)
                {
                    barController.isStop = false;
                }
            }

            if (state == AD_State.AD_REWARD_OK)
            {
                CheckOk = true;
                var goldIdx = rewards.IndexOf(x => x.resType == RES_type.GOLD);
                var goldVisual = rewards[goldIdx].amount * multiplier;
                rewards[goldIdx].amount *= (multiplier - 1);
                fxChest.SetActive(false);
                skipBtn.gameObject.SetActive(true);
                GameEvent.OnReceiveResource?.Invoke(LogEvent.ReasonItem.watch_ads, "ui_complete", rewards, level);
                //DataManager.Instance.ReceiveGift(false, 0, "ui_complete", LogEvent.ReasonItem.reward, true, level, rewards);
                //goldDisplay.gameObject.SetActive(true);
                //PopupManager.Instance.effectReceive.Initialized(rewards, claimX2Btn.transform.position, receivetarget);

                //canvasGroup.DOFade(1, 1);
                //RewardReceivedHub.Instance.CoinFly(PopupManager.Instance.canvas.worldCamera.WorldToScreenPoint(claimX2Btn.transform.position), goldDisplay.GetTransform() as RectTransform, 5, (index, total) =>
                //{-
                //    if (index == 0)
                //    {
                //        IResourceTarget.UpdateAmount(RES_type.GOLD);
                //    }
                //    goldDisplay.UpdateVisual();
                //});
                //RewardReceivedHub.Instance.ShowPopTextReward(new DataResource(RES_type.GOLD ,goldVisual), UIManager.Instance.canvas.worldCamera.WorldToScreenPoint(claimX2Btn.transform.position+Vector3.up),1);
                if (cacheCoinGet > 0)
                {
                    RewardReceivedHub.AddCacheValue(RES_type.GOLD, -cacheCoinGet);
                    cacheCoinGet = 0;
                }

                RewardReceivedHub.Instance.CoinFly(
                    UIManager.Instance.canvas.worldCamera.WorldToScreenPoint(claimX2Btn.transform.position), null, 6,
                    null, goldVisual);

                AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Pop_Reward);

                nextLevelBtn.interactable = false;
                claimX2Btn.interactable = false;
                nextLevelBtnLarge.interactable = false;
                //nextLevelBtn.gameObject.SetActive(false);
                //claimX2Btn.gameObject.SetActive(false);
                cbOnSkip = CompleteAction;
                DOVirtual.DelayedCall(2f, () =>
                {
                    skipBtn.gameObject.SetActive(false);
                    CompleteAction();
                }).SetId(this);

                void CompleteAction()
                {
                    if (GameRes.GetLevel() < PlayerPrefsUtil.CF_FirstLevelShowMain ||
                        UserDataManager.Instance.UserData.section_login < PlayerPrefsUtil.CF_SectionShowMain)
                    {
                        HeartManager.Instance.IsNoMoreLives(
                            () => { Hide(); UIManager.Instance.ShowPopup<UIGuildMoreLives>(null);}, () =>
                            {
                                PlayGame();
                                Hide();
                            });

                        return;

                        void PlayGame()
                        {
                            if (cacheCoinGet > 0)
                            {
                                RewardReceivedHub.AddCacheValue(RES_type.GOLD, -cacheCoinGet);
                                cacheCoinGet = 0;
                            }

                            IResourceTarget.OnItemChange?.Invoke(RES_type.GOLD, 0);
                            HeartManager.Instance.AddHeart(-1, "play_game", LogEvent.ReasonItem.use);
                            GameManager.Instance.NextLevel();
                        }
                    }
                    else
                    {
                        //var lastCacheCoin = cacheCoinGet;
                        //cacheCoinGet += rewards[goldIdx].amount;
                        //RewardReceivedHub.AddCacheValue(RES_type.GOLD, cacheCoinGet - lastCacheCoin);
                        GameManager.Instance.TransitionBackToHome(ReasonBackToHome.Win, Hide);
                    }
                }

                FIRhelper.logEvent($"level_{level:0000}_complete_x2_reward");
            }
        });
        isShowGift = ss >= 0;
        if (ss < 0)
        {
            UIManager.Instance.NotifyContent(content: "No Ads Available!", key: "_no_ads_available");
        }

        if (!isShowGift)
        {
            barController.isStop = false;
        }

        //LogEventManager.Instance.LogAds("gift", "ui_complete", isShowGift ? 1 : 0, level);
        DOVirtual.DelayedCall(2f, () => isShowGift = false).SetId(this);
    }


    private void HandleSpineEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (e.Data.Name == "Jum")
        {
            ActiveJum();
        }
    }

    private void ActiveJum()
    {
        AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Chest_Jump);
    }

    public void SkipButton()
    {
        cbOnSkip?.Invoke();
        DOTween.Kill(this);
    }

    public override void Hide()
    {
        DOTween.Kill(this);
        cbOnSkip = null;
        base.Hide();
    }

    private void OnDestroy()
    {
        skeletonChest.AnimationState.Event -= HandleSpineEvent;
        DOTween.Kill(this);
    }
}