#if FIRBASE_ENABLE
using Firebase.Analytics;
#endif
using master;
using mygame.sdk;
using System;
using DG.Tweening;
using UnityEngine;

public class AdsHelperWrapper : MonoBehaviour
{
    public static int countShowFull = 0;
    public static int AdsPace { get; set; } = 15;
    public static bool IsShowCollapse { get; set; }
    public static bool IsRewardOK { get; private set; }

    public static int ShowGift(string location, Action<AD_State> onSuccess, bool isAutoCloseWhenClcik, int level = 0)
    {
        if (level <= 0)
        {
            //level = DataManager.Level;
        }
        countShowFull = 0;
        IsRewardOK = false;
        AudioManager.Instance.SetCacheAudio();
        int stateShow = AdsHelper.Instance.showGift(location, level, false, (state) =>
        {
            if (state == AD_State.AD_REWARD_OK || state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 ||
                state == AD_State.AD_SHOW_MISS_CB || state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
            {
                AudioManager.Instance.ResetAudio();
            }
            if (state == AD_State.AD_REWARD_OK)
            {
                IsRewardOK = true;
            }
            if (state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_CLOSE || state == AD_State.AD_SHOW_FAIL2 || state == AD_State.AD_CLOSE2)
            {
                onAdDone();
            }
        }, true, isAutoCloseWhenClcik);
        if (stateShow != 0)
        {
            UIManager.Instance.NotifyContent(content: "No Ads Available!", key: "_no_ads_available");
            //LogEventManager.Instance.LogAds("video_reward", location, 0);
        }
        return stateShow;
        void onAdDone()
        {
            if (onSuccess != null)
            {
                var value = IsRewardOK ? AD_State.AD_REWARD_OK : AD_State.AD_REWARD_FAIL;
                onSuccess.Invoke(value);
                onSuccess = null;
                if (countShowFull > 1)
                {
                    //LogEventHub.LogAdsInterstitial_Banner(LogEvent.all_action_show_inter, location, level, countShowFull - 1);
                }
            }
        }

    }
    public static bool ShowFull(string location, Action<bool> onAdsClose = null, int countLose = -1, int level = -1, Action onPopupBreakAdsClose = null)
    {
        //var com_RemoveAds24h = GameManager.Instance.GetManagerComponent<AbsIAPManager>().GetComponentIAP<ComIAP_RemoveAds24h>();
        //if (com_RemoveAds24h.IsRemoveAdsFull())
        //{
        //    onAdsClose?.Invoke();
        //    return false;
        //}
        if (level <= -1)
        {
            level = DataManager.Level;
        }
        if (PlayerPrefsUtil.CF_ShowBreakAds)
        {
            PopupUI popup = null;
            var ss = AdsHelper.Instance.showFull(location, level, countLose, 0, 0, true, false, true, state =>
            {
                if (state == AD_State.AD_SHOW || state == AD_State.AD_SHOW2)
                {
                    Debug.Log("---------------------AD_SHOW--------------------");
                    // DOVirtual.DelayedCall(1, () =>
                    // {
                    //     if (popup != null)
                    //     {
                    //         popup.Hide();
                    //     }
                    // });
                }
                
                if (state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 || state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
                {
                    onAdsClose?.Invoke(true);
                    popup?.Hide();
                }
                if (state == AD_State.AD_REWARD_OK || state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 || state == AD_State.AD_SHOW_MISS_CB || state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
                {
                    AudioManager.Instance.ResetAudio();
                }
            });
          
            
            if (ss == false)
            {
                onPopupBreakAdsClose?.Invoke();
                onAdsClose?.Invoke(false);
            }
            else
            {
                popup = UIManager.Instance.ShowPopup<PopupBreakAds>(() =>
                {
                    countShowFull = 0;
                    onPopupBreakAdsClose?.Invoke();
                });
            }

            return ss;
        }
        else
        {
            var ss = AdsHelper.Instance.showFull(location, level, -1, 0, 0, false, false, true, state =>
            {
                if (state == AD_State.AD_REWARD_OK || state == AD_State.AD_CLOSE ||
                    state == AD_State.AD_CLOSE2 || state == AD_State.AD_SHOW_MISS_CB ||
                    state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
                {
                    AudioManager.Instance.ResetAudio();
                }
                if (state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 || state == AD_State.AD_SHOW_FAIL || state == AD_State.AD_SHOW_FAIL2)
                {
                    onAdsClose?.Invoke(true);
                    onPopupBreakAdsClose?.Invoke();

                }
            });
            if (ss == false)
            {
                onAdsClose?.Invoke(false);
                onPopupBreakAdsClose?.Invoke();
            }
            return ss;
        }
    }

    public static void SetBannerShowState(bool showState, string location = "full_game")
    {
        if (showState)
        {
            if (!PlayerPrefsUtil.CFEnableBanner) return;
            if (UserDataManager.Instance.UserData.section_login <= PlayerPrefsUtil.CFSectionShowBanner) return;
            if (DataManager.Level < PlayerPrefsUtil.CFLevelShowBanner) return;
            if (SdkUtil.isiPad())
            {
                AdsHelper.Instance.showBanner(location, AD_BANNER_POS.BOTTOM, App_Open_ad_Orien.Orien_Portraid, 0, -2, 90);
            }
            else
            {
                AdsHelper.Instance.showBanner(location, AD_BANNER_POS.BOTTOM, App_Open_ad_Orien.Orien_Portraid, 0, -2, 60);
            }
        }
        else
        {
            AdsHelper.Instance.hideBanner(1);
        }
    }

    public static void ShowRectBanner(string place, AD_BANNER_POS bannerPos = AD_BANNER_POS.TOP, bool isCheckShowConfig = false)
    {
        //AdsHelper.Instance.showBannerRect(place, bannerPos, -1, 0, 0f, 1);
    }
    public static void HideRectBanner()
    {
        AdsHelper.Instance.hideBannerRect();
    }
}
