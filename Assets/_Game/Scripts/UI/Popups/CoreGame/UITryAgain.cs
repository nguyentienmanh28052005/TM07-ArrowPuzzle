using mygame.sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UITryAgain : PopupUI
{
    [SerializeField] Button btnPlay;
    [SerializeField] Text txtLevel;
    [SerializeField] Text txtMode;
    [SerializeField] RectTransform safeAreaRect;
    [SerializeField] private AutoChangeTheme levelTypeModifier;

    public static bool CanShowAds
    {
        get
        {
            var config = ConfigManager.Instance.GetConfigShowAdsFull(DataManager.Level);
            var checkLevel = PlayerPrefsUtil.CFLevelFailNoAds < DataManager.Level;
            var checkLoseFirstTime = config.CFShowFullFirstTimeFail > 0 && PlayerPrefsUtil.LastIndexFailShowAds == 0;
            var checkLoseLevel = config.CFShowFullFailSpaceByLevel <= 0 || DataManager.Instance.ConsecutiveLose - (PlayerPrefsUtil.LastIndexFailShowAds/* - config.CFShowFullFirstTimeFail*/) >= config.CFShowFullFailSpaceByLevel;

            Debug.Log($"Show full Fail check Level {checkLevel} checkFirstTime {checkLoseFirstTime} checkLoseLevel {checkLoseLevel}");
            Debug.Log($"Show full Fail CFShowFullFirstTimeFail {config.CFShowFullFirstTimeFail} CFShowFullFailSpaceByLevel {config.CFShowFullFailSpaceByLevel} ConsecutiveLose{DataManager.Instance.ConsecutiveLose} LastFailShowFull{PlayerPrefsUtil.LastIndexFailShowAds} Space{DataManager.Instance.ConsecutiveLose - (PlayerPrefsUtil.LastIndexFailShowAds/* - config.CFShowFullFirstTimeFail*/) >= config.CFShowFullFailSpaceByLevel}");
            return (checkLevel && checkLoseLevel) || (checkLoseFirstTime && checkLevel);
        }
    }

    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        btnPlay.onClick.AddListener(OnClickPlayLevel);

        // if (safeAreaRect != null)
        // {
        //     if (Screen.safeArea.yMax < Screen.height)
        //     {
        //         // bgLevel.anchorMin = Vector2.one;
        //         // bgLevel.anchorMax = Vector2.one;
        //         //bgLevel.anchoredPosition = new Vector2(0, 0);
        //         // bgLevel.GetComponent<Image>().enabled = false;
        //         
        //         var safeArea = Screen.safeArea;
        //         var screen = new Vector2(Screen.width, Screen.height);
        //
        //         // 1. Setup and apply safe area
        //         var h = (screen.y - (screen.y - safeArea.yMax)) / screen.y;
        //         var heightArea = h + (1 - h) / 2.25f;
        //         safeAreaRect.anchorMax = new Vector2(1, heightArea);
        //     }
        // }
    }

    public override void Show(Action onClose)
    {
        base.Show(onClose);
        var curLevel = GameRes.GetLevel();
        var currentLevelType = LevelManager.GetLevelType(curLevel);
        if (levelTypeModifier != null)
        {
            levelTypeModifier.ApplyTheme(currentLevelType);
        }
        AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Lose);

        var arr = new string[3]
        {
            "",
            "HARD",
            "CRAZY",
        };

        txtMode.text = arr[(int) currentLevelType];
        txtLevel.SetText("_level_x", StateCapText.FirstCap, FormatText.F_String, GameRes.GetLevel().ToString());
        if ((GameRes.GetLevel() < PlayerPrefsUtil.CF_FirstLevelShowMain ||
             UserDataManager.Instance.UserData.section_login < PlayerPrefsUtil.CF_SectionShowMain) &&
            PlayerPrefsUtil.CF_DisableMainIfLowerLevel)
        {
            ButtonClose.gameObject.SetActive(false);
        }
        else
        {
            ButtonClose.gameObject.SetActive(true);
        }
        btnPlay.enabled = true;
        uiManager.GetScreenActive<UIInGame>().SetActiveGoldDisplay(false);
    }

    private void OnClickPlayLevel()
    {
        btnPlay.enabled = false;
        HeartManager.Instance.IsNoMoreLives(() =>
        {
            gameObject.SetActive(false);

            UIManager.Instance.ShowPopup<UIGuildMoreLives>(() =>
            {
                if (HeartManager.Instance.CurrentHeart <= 0)
                {
                    gameObject.SetActive(true);
                    btnPlay.enabled = true;
                }
                else
                {
                    PlayGame();
                    Hide();
                }
            });
        }, () =>
        {
            if (!CanShowAds)
            {
                PlayGame();
                Hide();
            }
            else
            {
                AudioManager.Instance.SetCacheAudio();
                AdsHelperWrapper.ShowFull("ui_try_again_play", (isShowAds) =>
                    {
                        if (isShowAds)
                        {
                            PlayerPrefsUtil.LastIndexFailShowAds = DataManager.Instance.ConsecutiveLose;
                        }
                    },
                    onPopupBreakAdsClose: () =>
                    {
                        PlayGame();
                        Hide();
                    });
            }
        });

        return;

        void PlayGame()
        {
            HeartManager.Instance.AddHeart(-1, "play_game", LogEvent.ReasonItem.use);
            GameManager.CurrentPlayType = "retry";
            GameManager.Instance.RestartGame();
            FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_retry");

            // if (PlayerPrefsUtil.CFLevelFailNoAds >= DataManager.Level )
            // {
            //     Hide();
            //     Debug.LogError("CFLevelFailNoAds");
            // }
            // else
            // {
            //     AudioManager.Instance.SetCacheAudio();
            //     AdsHelperWrapper.ShowFull("ui_try_again_play", isShowAds =>
            //     {
            //         if (!isShowAds)
            //         {
            //             PlayerPrefsUtil.LastIndexFailShowAds = DataManager.Instance.ConsecutiveLose;
            //         }
            //     }, onPopupBreakAdsClose: () =>
            //     {
            //         Hide();
            //         Debug.LogError("onPopupBreakAdsClose");
            //     });
            //    
            //     //var ss = AdsHelper.Instance.showFull("ui_try_again_play", GameRes.GetLevel(), DataManager.Instance.ConsecutiveLose, 0, 0, false, false, true, state =>
            //     //{
            //     //    if (state == AD_State.AD_REWARD_OK || state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 || state == AD_State.AD_SHOW_MISS_CB || state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
            //     //    {
            //     //        AudioManager.Instance.ResetAudio();
            //     //    }
            //     //    if (state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 || state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
            //     //    {
            //     //        Hide();
            //     //    }
            //     //});
            //     //if (ss == false)
            //     //{
            //     //    Hide();
            //     //}
            //     //else
            //     //{
            //     //    PlayerPrefsUtil.LastIndexFailShowAds = DataManager.Instance.ConsecutiveLose;
            //     //}
            // }
        }
    }

    public override void OnClickClose()
    {
        FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_lose_home");
        //AudioManager.Instance.SetCacheAudio();
        //GameManager.Instance.TransitionBackToHome(ReasonBackToHome.Lose, base.OnClickClose);
        if (!CanShowAds)
        {
            GameManager.Instance.TransitionBackToHome(ReasonBackToHome.Lose, base.OnClickClose);
        }
        else
        {
            AdsHelperWrapper.ShowFull("ui_try_again_close", (isShowAds) =>
                {
                    if (isShowAds)
                    {
                        PlayerPrefsUtil.LastIndexFailShowAds = DataManager.Instance.ConsecutiveLose;
                    }
                },
                onPopupBreakAdsClose: () =>
                {
                    GameManager.Instance.TransitionBackToHome(ReasonBackToHome.Lose, base.OnClickClose);
                });
            //var ss = AdsHelper.Instance.showFull("ui_try_again_close", GameRes.GetLevel(), DataManager.Instance.ConsecutiveLose, 0, 0, false, false, cb: (state) =>
            //{
            //    if (state == AD_State.AD_REWARD_OK || state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 || state == AD_State.AD_SHOW_MISS_CB || state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
            //    {
            //        AudioManager.Instance.ResetAudio();
            //    }
            //    if (state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 || state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
            //    {
            //        GameManager.Instance.TransitionBackToHome(ReasonBackToHome.Lose, base.OnClickClose);
            //    }
            //}); 
            //if (ss == false)
            //{
            //    GameManager.Instance.TransitionBackToHome(ReasonBackToHome.Lose, base.OnClickClose);
            //}
            //else
            //{
            //    PlayerPrefsUtil.LastIndexFailShowAds = DataManager.Instance.ConsecutiveLose;
            //}
        }
    }

    public override void Hide()
    {
        base.Hide();
    }
}