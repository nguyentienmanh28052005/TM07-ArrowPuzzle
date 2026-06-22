using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_ANDROID
using mygame.plugin.Android;
#endif
namespace mygame.sdk
{
    public class AdsAdmobMyBridge : MonoBehaviour
    {
        public static event Action<string, string, string> onOpenAdLoaded;
        public static event Action<string, string, string> onOpenAdLoadFail;
        public static event Action<string, string, string, string> onOpenAdFailedToShow;
        public static event Action<string, string, string> onOpenAdShowed;
        public static event Action<string, string, string> onOpenAdClick;
        public static event Action<string, string, string> onOpenAdImpresstion;
        public static event Action<string, string, string> onOpenAdDismissed;
        public static event Action<string, string, string, int, string, long> onOpenAdPaid;

        public static event Action<string, string, string> onBNLoaded;
        public static event Action<string, string, string> onBNLoadFail;
        public static event Action<string, string, string> onBNOpen;
        public static event Action<string, string, string> onBNClick;
        public static event Action<string, string, string> onBNImpression;
        public static event Action<string, string, string> onBNClose;
        public static event Action<string, string, string, int, string, long> onBNPaid;

        public static event Action<string, string, string> onBNClLoaded;
        public static event Action<string, string, string> onBNClLoadFail;
        public static event Action<string, string, string> onBNClOpen;
        public static event Action<string, string, string> onBNClClick;
        public static event Action<string, string, string> onBNClImpression;
        public static event Action<string, string, string> onBNClClose;
        public static event Action<string, string, string, int, string, long> onBNClPaid;

        public static event Action<string, string, string> onBNRectLoaded;
        public static event Action<string, string, string> onBNRectLoadFail;
        public static event Action<string, string, string> onBNRectOpen;
        public static event Action<string, string, string> onBNRectClick;
        public static event Action<string, string, string> onBNRectImpression;
        public static event Action<string, string, string> onBNRectClose;
        public static event Action<string, string, string, int, string, long> onBNRectPaid;

        public static event Action<string, string, string> onBNNativeLoaded;
        public static event Action<string, string, string> onBNNativeLoadFail;
        public static event Action<string, string, string> onBNNativeOpen;
        public static event Action<string, string, string> onBNNativeClick;
        public static event Action<string, string, string> onBNNativeImpression;
        public static event Action<string, string, string> onBNNativeClose;
        public static event Action<string, string, string, int, string, long> onBNNativePaid;

        public static event Action<string, string, string> onNativeClLoaded;
        public static event Action<string, string, string> onNativeClLoadFail;
        public static event Action<string, string, string, string> onNativeClFailedToShow;
        public static event Action<string, string, string> onNativeClShowed;
        public static event Action<string, string, string> onNativeClClick;
        public static event Action<string, string, string> onNativeClImpresstion;
        public static event Action<string, string, string> onNativeClDismissed;
        public static event Action<string, string, string, int, string, long> onNativeClPaid;

        public static event Action<string, string, string> onRectNativeLoaded;
        public static event Action<string, string, string> onRectNativeLoadFail;
        public static event Action<string, string, string> onRectNativeClick;
        public static event Action<string, string, string> onRectNativeImpression;
        public static event Action<string, string, string, int, string, long> onRectNativePaid;

        public static event Action<string, string, string> onNativeFullLoaded;
        public static event Action<string, string, string> onNativeFullLoadFail;
        public static event Action<string, string, string, string> onNativeFullFailedToShow;
        public static event Action<string, string, string> onNativeFullShowed;
        public static event Action<string, string, string> onNativeFullClick;
        public static event Action<string, string, string> onNativeFullImpresstion;
        public static event Action<string, string, string> onNativeFullDismissed;
        public static event Action<string, string, string> onNativeFullFinishShow;
        public static event Action<string, string, string, int, string, long> onNativeFullPaid;

        public static event Action<string, string, string> onNativeIcFullLoaded;
        public static event Action<string, string, string> onNativeIcFullLoadFail;
        public static event Action<string, string, string, string> onNativeIcFullFailedToShow;
        public static event Action<string, string, string> onNativeIcFullShowed;
        public static event Action<string, string, string> onNativeIcFullClick;
        public static event Action<string, string, string> onNativeIcFullImpresstion;
        public static event Action<string, string, string> onNativeIcFullDismissed;
        public static event Action<string, string, string> onNativeIcFullFinishShow;
        public static event Action<string, string, string, int, string, long> onNativeIcFullPaid;

        public static event Action<string, string, string> onNativePgFullLoaded;
        public static event Action<string, string, string> onNativePgFullLoadFail;
        public static event Action<string, string, string, string> onNativePgFullFailedToShow;
        public static event Action<string, string, string> onNativePgFullShowed;
        public static event Action<string, string, string> onNativePgFullClick;
        public static event Action<string, string, string> onNativePgFullImpresstion;
        public static event Action<string, string, string> onNativePgFullDismissed;
        public static event Action<string, string, string> onNativePgFullFinishShow;
        public static event Action<string, string, string, int, string, long> onNativePgFullPaid;

        public static event Action<string, string, string> onInterstitialLoaded;
        public static event Action<string, string, string> onInterstitialLoadFail;
        public static event Action<string, string, string, string> onInterstitialFailedToShow;
        public static event Action<string, string, string> onInterstitialShowed;
        public static event Action<string, string, string> onInterstitialClick;
        public static event Action<string, string, string> onInterstitialImpresstion;
        public static event Action<string, string, string> onInterstitialDismissed;
        public static event Action<string, string, string> onInterstitialFinishShow;
        public static event Action<string, string, string, int, string, long> onInterstitialPaid;

        public static event Action<string, string, string> onInterRwInterLoaded;
        public static event Action<string, string, string> onInterRwInterLoadFail;
        public static event Action<string, string, string, string> onInterRwInterFailedToShow;
        public static event Action<string, string, string> onInterRwInterImpresstion;
        public static event Action<string, string, string> onInterRwInterShowed;
        public static event Action<string, string, string> onInterRwInterClick;
        public static event Action<string, string, string> onInterRwInterDismissed;
        public static event Action<string, string, string> onInterRwInterFinishShow;
        public static event Action<string, string, string> onInterRwInterReward;
        public static event Action<string, string, string, int, string, long> onInterRwInterPaid;

        public static event Action<string, string, string> onInterRwRwLoaded;
        public static event Action<string, string, string> onInterRwRwLoadFail;
        public static event Action<string, string, string, string> onInterRwRwFailedToShow;
        public static event Action<string, string, string> onInterRwRwImpresstion;
        public static event Action<string, string, string> onInterRwRwShowed;
        public static event Action<string, string, string> onInterRwRwClick;
        public static event Action<string, string, string> onInterRwRwDismissed;
        public static event Action<string, string, string> onInterRwRwFinishShow;
        public static event Action<string, string, string> onInterRwRwReward;
        public static event Action<string, string, string, int, string, long> onInterRwRwPaid;

        public static event Action<string, string, string> onRewardLoaded;
        public static event Action<string, string, string> onRewardLoadFail;
        public static event Action<string, string, string, string> onRewardFailedToShow;
        public static event Action<string, string, string> onRewardShowed;
        public static event Action<string, string, string> onRewardClick;
        public static event Action<string, string, string> onRewardImpresstion;
        public static event Action<string, string, string> onRewardDismissed;
        public static event Action<string, string, string> onRewardFinishShow;
        public static event Action<string, string, string> onRewardReward;
        public static event Action<string, string, string, int, string, long> onRewardPaid;

        public static event Action<string, string, string> onNativeGiftLoaded;
        public static event Action<string, string, string> onNativeGiftLoadFail;
        public static event Action<string, string, string, string> onNativeGiftFailedToShow;
        public static event Action<string, string, string> onNativeGiftShowed;
        public static event Action<string, string, string> onNativeGiftClick;
        public static event Action<string, string, string> onNativeGiftImpresstion;
        public static event Action<string, string, string> onNativeGiftDismissed;
        public static event Action<string, string, string> onNativeGiftFinishShow;
        public static event Action<string, string, string, int, string, long> onNativeGiftPaid;

        private AdsAdmobMyIF adsAdmobIF;

        public static AdsAdmobMyBridge Instance { get; private set; }

#if UNITY_ANDROID
#endif

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                gameObject.name = "AdsAdmobMyBridge";
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
#if UNITY_ANDROID
adsAdmobIF = new AdsAdmobMyAndroid();
#else
adsAdmobIF = new AdsAdmobMyiOS();
#endif
#endif
            }
            else
            {
                //if (this != Instance) Destroy(gameObject);
            }
        }


        #region IGoogleMobileAdsInterstitialClient implementation
        #region Common Function
        public void Initialize()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.Initialize();
#endif
        }
        public void targetingAdContent(bool isChild, bool isUnderAgeConsent, int rating)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.targetingAdContent(isChild, isUnderAgeConsent, rating);
#endif
        }
        public void setLog(bool isLog)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.setLog(isLog);
#endif
        }
        public void cfBnRefresh(string config)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.cfBnRefresh(config);
#endif
        }
        public void trackingTiktok(int flagTrackingTiktok)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.trackingTiktok(flagTrackingTiktok);
#endif
        }

        public void cfBnAreaTouchex(int w, int h)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.cfBnAreaTouchex(w, h);
#endif
        }

        public void setCfNtFull(int v1, int v2, int v3, int v4, int v5, int v6)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.setCfNtFull(v1, v2, v3, v4, v5, v6);
#endif
        }
        public void setCfNtdayClick(string cfdayclick)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.setCfNtdayClick(cfdayclick);
#endif
        }
        public void setCfNtFullFbExcluse(int rows, int columns, string areaExcluse)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.setCfNtFullFbExcluse(rows, columns, areaExcluse);
#endif
        }
        public void setCfNtCl(int v1, int v2, int v3, int v4)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.setCfNtCl(v1, v2, v3, v4);
#endif
        }
        public void setCfNtClFbExcluse(int rows, int columns, string areaExcluse, int levelFlick)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.setCfNtClFbExcluse(rows, columns, areaExcluse, levelFlick);
#endif
        }
        public void setTypeBnnt(bool isShowMedia)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.setTypeBnnt(isShowMedia);
#endif
        }
        public void setTestDevices(string testDevices)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
#if UNITY_ANDROID
            ((AdsAdmobMyAndroid)adsAdmobIF).setTestDevices(testDevices);
#endif
#endif
        }
        #endregion
        #region Format Open
        public void loadOpenAd(string placement, string adsId)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.loadOpenAd(placement, adsId);
#endif
        }
        public bool showOpenAd(string placement)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
             return adsAdmobIF.showOpenAd(placement, 0); 
#else
            return false;
#endif
        }
        #endregion
        #region Format Banner
        public bool showBanner(string placement, int pos, int width, int maxH, int orien, float dxCenter)
        {
            bool iPad = false;
            if (AppConfig.isBannerIpad)
            {
                iPad = SdkUtil.isiPad();
            }
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            return adsAdmobIF.showBanner(placement, pos, width, maxH, orien, iPad, dxCenter);
#endif
            return false;
        }
        public void loadBanner(string placement, string adsId)
        {
            bool iPad = false;
            if (AppConfig.isBannerIpad)
            {
                iPad = SdkUtil.isiPad();
            }
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.loadBanner(placement, adsId, iPad);
#endif
        }
        public void hideBanner()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.hideBanner();
#endif
        }
        public void destroyBanner()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.destroyBanner();
#endif
        }

        public bool showBannerCl(string placement, int pos, int width, int maxH, int orien, float dxCenter)
        {
            bool iPad = false;
            if (AppConfig.isBannerIpad)
            {
                iPad = SdkUtil.isiPad();
            }
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            return adsAdmobIF.showBannerCl(placement, pos, width, maxH, orien, iPad, dxCenter);
#endif
            return false;
        }
        public void loadBannerCl(string placement, string adsId)
        {
            bool iPad = false;
            if (AppConfig.isBannerIpad)
            {
                iPad = SdkUtil.isiPad();
            }
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.loadBannerCl(placement, adsId, iPad);
#endif
        }
        public void hideBannerCl()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.hideBannerCl();
#endif
        }
        public void destroyBannerCl()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.destroyBannerCl();
#endif
        }

        public bool showBannerRect(string placement, int pos, float width, int maxH, float dxCenter, float dyVertical)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            return adsAdmobIF.showBannerRect(placement, pos, width, maxH, dxCenter, dyVertical);
#endif
            return false;
        }
        public void loadBannerRect(string placement, string adsId)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.loadBannerRect(placement, adsId);
#endif
        }
        public void hideBannerRect()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.hideBannerRect();
#endif
        }
        public void destroyBannerRect()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.destroyBannerRect();
#endif
        }
        #endregion
        #region Format Banner Native
        public bool showBnNt(string placement, int pos, int width, int maxH, int orien, float dxCenter, int trefresh)
        {
            bool iPad = false;
            if (AppConfig.isBannerIpad)
            {
                iPad = SdkUtil.isiPad();
            }
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            return adsAdmobIF.showBnNt(placement, pos, width, maxH, orien, iPad, dxCenter, 0, trefresh);
#endif
            return false;
        }
        public void loadBnNt(string placement, string adsId)
        {
            bool iPad = false;
            if (AppConfig.isBannerIpad)
            {
                iPad = SdkUtil.isiPad();
            }
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.loadBnNt(placement, adsId, iPad);
#endif
        }
        public void hideBnNt()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.hideBnNt();
#endif
        }
        public void destroyBnNt()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.destroyBnNt();
#endif
        }
        #endregion
        #region Format Native Collapse
        public void loadNativeCl(string placement, string adsId, int index, int orient)
        {
            bool iPad = false;
            if (AppConfig.isBannerIpad)
            {
                iPad = SdkUtil.isiPad();
            }
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.loadNativeCl(placement, adsId, index, orient);
#endif
        }
        public bool showNativeCl(string placement, int pos, int width, float dxCenter, bool isHideBtClose, bool isLouWhenick)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            return adsAdmobIF.showNativeCl(placement, pos, width, dxCenter, isHideBtClose, isLouWhenick);
#else
            return false;
#endif
        }
        public void hideNativeCl()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.hideNativeCl();
#endif
        }
        #endregion
        #region Format Native Rect
        public bool showRectNt(string placement, int pos, int orient, float width, float height, float dx, float dy)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            return adsAdmobIF.showRectNt(placement, pos, orient, width, height, dx, dy);
#endif
            return false;
        }
        public void loadRectNt(string placement, string adsId)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.loadRectNt(placement, adsId);
#endif
        }
        public void hideRectNt()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.hideRectNt();
#endif
        }
        #endregion
        #region Format Native Full
        public void loadNativeFull(string placement, string adsId, int index, int orient)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.loadNativeFull(placement, adsId, index, orient);
#endif
        }
        public bool showNativeFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick, int layput, bool isAd2)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
             return adsAdmobIF.showNativeFull(placement, isNham, timeShowBtClose, timeDelay, isAutoCloseWhenClick, layput, isAd2);
#else
            return false;
#endif
        }
        //
        public void loadNativeIcFull(string placement, string adsId, int index, int orient)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.loadNativeIcFull(placement, adsId, index, orient);
#endif
        }
        public bool showNativeIcFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick, int layput, bool isAd2)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
             return adsAdmobIF.showNativeIcFull(placement, isNham, timeShowBtClose, timeDelay, isAutoCloseWhenClick, layput, isAd2);
#else
            return false;
#endif
        }
        //
        public void loadNativePgFull(string placement, int adPos, string adsId, int index)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.loadNativePgFull(placement, adPos, adsId, index);
#endif
        }
        public bool showNativePgFull(string placement, bool isNham, int timeShowBtClose, int timeDelay)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
             return adsAdmobIF.showNativePgFull(placement, isNham, timeShowBtClose, timeDelay);
#else
            return false;
#endif
        }
        #endregion
        #region Format Intertitial
        public void clearCurrFull(string placement)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.clearCurrFull(placement);
#endif
        }
        public void loadFull(string placement, string adsId, int index)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.loadFull(placement, adsId, index);
#endif
        }
        public bool showFull(string placement)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
             return adsAdmobIF.showFull(placement, 0); 
#else
            return false;
#endif
        }
        #endregion
        #region Format Full Rewarded Intertitial
        public void loadFullRwInter(string placement, string adsId)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.loadFullRwInter(placement, adsId);
#endif
        }
        public bool showFullRwInter(string placement)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
             return adsAdmobIF.showFullRwInter(placement, 0); 
#else
            return false;
#endif
        }
        #endregion
        #region Format Full Rewarded
        public void loadFullRwRw(string placement, string adsId)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.loadFullRwRw(placement, adsId);
#endif
        }
        public bool showFullRwRw(string placement)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
             return adsAdmobIF.showFullRwRw(placement, 0); 
#else
            return false;
#endif
        }
        #endregion
        #region Format Rewarded
        public void clearCurrGift(string placement)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.clearCurrGift(placement);
#endif
        }
        public void loadGift(string placement, string adsId, int index)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.loadGift(placement, adsId, index); 
#endif
        }
        public bool showGift(string placement)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            return adsAdmobIF.showGift(placement); 
#else
            return false;
#endif
        }
        #endregion
        #region Format Gift Native
        public void loadNativeGift(string placement, int adPos, string adsId, int index)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.loadNativeGift(placement, adPos, adsId, index);
#endif
        }
        public bool showNativeGift(string placement, bool isNham, int timeShowBtClose, int timeDelay)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
             return adsAdmobIF.showNativeGift(placement, isNham, timeShowBtClose, timeDelay);
#else
            return false;
#endif
        }

        public void loadGiftFull(string placement, string adsId, int index)
        {

        }
        public bool showGiftFull(string placement)
        {
            return false;
        }
        #endregion
        #endregion
#if UNITY_ANDROID

        #region Callbacks from UnityInterstitialAdListener.
        #region CB Open
        public void onOpenLoaded(string placement, string adsId, string adnet)
        {
            if (onOpenAdLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onOpenAdLoaded(placement, adsId, adnet);
                });
            }
        }
        public void onOpenLoadFail(string placement, string adsId, string err)
        {
            if (onOpenAdLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onOpenAdLoadFail(placement, adsId, err);
                });
            }
        }
        public void onOpenFailedToShow(string placement, string adsId, string adnet, string err)
        {
            if (onOpenAdFailedToShow != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onOpenAdFailedToShow(placement, adsId, adnet, err);
                });
            }
        }
        public void onOpenShowed(string placement, string adsId, string adnet)
        {
            if (onOpenAdShowed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onOpenAdShowed(placement, adsId, adnet);
                });
            }
        }
        public void onOpenDismissed(string placement, string adsId, string adnet)
        {
            if (onOpenAdDismissed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onOpenAdDismissed(placement, adsId, adnet);
                });
            }
        }
        public void onOpenImpresstion(string placement, string adsId, string adnet)
        {
            if (onOpenAdImpresstion != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onOpenAdImpresstion(placement, adsId, adnet);
                });
            }
        }
        public void onOpenClick(string placement, string adsId, string adnet)
        {
            if (onOpenAdClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onOpenAdClick(placement, adsId, adnet);
                //});
            }
        }
        public void onOpenPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onOpenAdPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onOpenAdPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        #endregion
        #region CB Banner
        public void onBannerLoaded(string placement, string adsId, string adnet)
        {
            if (onBNLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNLoaded(placement, adsId, adnet);
                });
            }
        }
        public void onBannerLoadFail(string placement, string adsId, string err)
        {
            if (onBNLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNLoadFail(placement, adsId, err);
                });
            }
        }
        public void onBannerClose(string placement, string adsId, string adnet)
        {
            if (onBNClose != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNClose(placement, adsId, adnet);
                });
            }
        }
        public void onBannerOpen(string placement, string adsId, string adnet)
        {
            if (onBNOpen != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNOpen(placement, adsId, adnet);
                });
            }
        }
        public void onBannerClick(string placement, string adsId, string adnet)
        {
            if (onBNClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onBNClick(placement, adsId, adnet);
                //});
            }
        }
        public void onBannerImpression(string placement, string adsId, string adnet)
        {
            if (onBNImpression != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNImpression(placement, adsId, adnet);
                });
            }
        }
        public void onBannerPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onBNPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        #endregion
        #region CB Banner Collapse
        public void onBannerClLoaded(string placement, string adsId, string adnet)
        {
            if (onBNClLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNClLoaded(placement, adsId, adnet);
                });
            }
        }
        public void onBannerClLoadFail(string placement, string adsId, string err)
        {
            if (onBNClLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNClLoadFail(placement, adsId, err);
                });
            }
        }
        public void onBannerClClose(string placement, string adsId, string adnet)
        {
            if (onBNClClose != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNClClose(placement, adsId, adnet);
                });
            }
        }
        public void onBannerClOpen(string placement, string adsId, string adnet)
        {
            if (onBNClOpen != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNClOpen(placement, adsId, adnet);
                });
            }
        }
        public void onBannerClClick(string placement, string adsId, string adnet)
        {
            if (onBNClClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onBNClClick(placement, adsId, adnet);
                //});
            }
        }
        public void onBannerClImpression(string placement, string adsId, string adnet)
        {
            if (onBNClImpression != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNClImpression(placement, adsId, adnet);
                });
            }
        }
        public void onBannerClPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onBNClPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNClPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        #endregion
        #region CB Banner Rect
        public void onBannerRectLoaded(string placement, string adsId, string adnet)
        {
            if (onBNRectLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNRectLoaded(placement, adsId, adnet);
                });
            }
        }
        public void onBannerRectLoadFail(string placement, string adsId, string err)
        {
            if (onBNRectLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNRectLoadFail(placement, adsId, err);
                });
            }
        }
        public void onBannerRectClose(string placement, string adsId, string adnet)
        {
            if (onBNRectClose != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNRectClose(placement, adsId, adnet);
                });
            }
        }
        public void onBannerRectOpen(string placement, string adsId, string adnet)
        {
            if (onBNRectOpen != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNRectOpen(placement, adsId, adnet);
                });
            }
        }
        public void onBannerRectClick(string placement, string adsId, string adnet)
        {
            if (onBNRectClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onBNRectClick(placement, adsId, adnet);
                //});
            }
        }
        public void onBannerRectImpression(string placement, string adsId, string adnet)
        {
            if (onBNRectImpression != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNRectImpression(placement, adsId, adnet);
                });
            }
        }
        public void onBannerRectPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onBNRectPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNRectPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        #endregion
        #region CB Banner Native
        public void onBNNTLoaded(string placement, string adsId, string adnet)
        {
            if (onBNNativeLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNNativeLoaded(placement, adsId, adnet);
                });
            }
        }
        public void onBNNTLoadFail(string placement, string adsId, string err)
        {
            if (onBNNativeLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNNativeLoadFail(placement, adsId, err);
                });
            }
        }
        public void onBNNTClose(string placement, string adsId, string adnet)
        {
            if (onBNNativeClose != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNNativeClose(placement, adsId, adnet);
                });
            }
        }
        public void onBNNTOpen(string placement, string adsId, string adnet)
        {
            if (onBNNativeOpen != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNNativeOpen(placement, adsId, adnet);
                });
            }
        }
        public void onBNNTClick(string placement, string adsId, string adnet)
        {
            if (onBNNativeClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onBNNativeClick(placement, adsId, adnet);
                //});
            }
        }
        public void onBNNTImpression(string placement, string adsId, string adnet)
        {
            if (onBNNativeImpression != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNNativeImpression(placement, adsId, adnet);
                });
            }
        }
        public void onBNNTPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onBNNativePaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNNativePaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        #endregion
        #region CB Native collapse
        public void onNtClLoaded(string placement, string adsId, string adnet)
        {
            if (onNativeClLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeClLoaded(placement, adsId, adnet);
                });
            }
        }
        public void onNtClLoadFail(string placement, string adsId, string err)
        {
            if (onNativeClLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeClLoadFail(placement, adsId, err);
                });
            }
        }
        public void onNtClFailedToShow(string placement, string adsId, string adnet, string err)
        {
            if (onNativeClFailedToShow != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeClFailedToShow(placement, adsId, adnet, err);
                });
            }
        }
        public void onNtClShowed(string placement, string adsId, string adnet)
        {
            if (onNativeClShowed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeClShowed(placement, adsId, adnet);
                });
            }
        }
        public void onNtClDismissed(string placement, string adsId, string adnet)
        {
            if (onNativeClDismissed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeClDismissed(placement, adsId, adnet);
                });
            }
        }
        public void onNtClImpresstion(string placement, string adsId, string adnet)
        {
            if (onNativeClImpresstion != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeClImpresstion(placement, adsId, adnet);
                });
            }
        }
        public void onNtClClick(string placement, string adsId, string adnet)
        {
            if (onNativeClClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onNativeClClick(placement, adsId, adnet);
                //});
            }
        }
        public void onNtClPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onNativeClPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeClPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        #endregion
        #region CB Rect Native 
        public void onRectNTLoaded(string placement, string adsId, string adnet)
        {
            if (onBNNativeLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRectNativeLoaded(placement, adsId, adnet);
                });
            }
        }
        public void onRectNTLoadFail(string placement, string adsId, string err)
        {
            if (onBNNativeLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRectNativeLoadFail(placement, adsId, err);
                });
            }
        }
        public void onRectNTClick(string placement, string adsId, string adnet)
        {
            if (onBNNativeClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onRectNativeClick(placement, adsId, adnet);
                //});
            }
        }
        public void onRectNTImpression(string placement, string adsId, string adnet)
        {
            if (onBNNativeImpression != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRectNativeImpression(placement, adsId, adnet);
                });
            }
        }
        public void onRectNTPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onBNNativePaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRectNativePaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        #endregion
        #region CB Native Full
        public void onNtFullLoaded(string placement, string adsId, string adnet)
        {
            if (onNativeFullLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullLoaded(placement, adsId, adnet);
                });
            }
        }
        public void onNtFullLoadFail(string placement, string adsId, string err)
        {
            if (onNativeFullLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullLoadFail(placement, adsId, err);
                });
            }
        }
        public void onNtFullFailedToShow(string placement, string adsId, string adnet, string err)
        {
            if (onNativeFullFailedToShow != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullFailedToShow(placement, adsId, adnet, err);
                });
            }
        }
        public void onNtFullShowed(string placement, string adsId, string adnet)
        {
            if (onNativeFullShowed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullShowed(placement, adsId, adnet);
                });
            }
        }
        public void onNtFullDismissed(string placement, string adsId, string adnet)
        {
            if (onNativeFullDismissed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullDismissed(placement, adsId, adnet);
                });
            }
        }
        public void onNtFullImpresstion(string placement, string adsId, string adnet)
        {
            if (onNativeFullImpresstion != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullImpresstion(placement, adsId, adnet);
                });
            }
        }
        public void onNtFullClick(string placement, string adsId, string adnet)
        {
            if (onNativeFullClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onNativeFullClick(placement, adsId, adnet);
                //});
            }
        }
        public void onNtFullPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onNativeFullPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        #endregion
        #region CB Native IC Full
        public void onNtIcFullLoaded(string placement, string adsId, string adnet)
        {
            if (onNativeIcFullLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeIcFullLoaded(placement, adsId, adnet);
                });
            }
        }
        public void onNtIcFullLoadFail(string placement, string adsId, string err)
        {
            if (onNativeIcFullLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeIcFullLoadFail(placement, adsId, err);
                });
            }
        }
        public void onNtIcFullFailedToShow(string placement, string adsId, string adnet, string err)
        {
            if (onNativeIcFullFailedToShow != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeIcFullFailedToShow(placement, adsId, adnet, err);
                });
            }
        }
        public void onNtIcFullShowed(string placement, string adsId, string adnet)
        {
            if (onNativeIcFullShowed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeIcFullShowed(placement, adsId, adnet);
                });
            }
        }
        public void onNtIcFullDismissed(string placement, string adsId, string adnet)
        {
            if (onNativeIcFullDismissed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeIcFullDismissed(placement, adsId, adnet);
                });
            }
        }
        public void onNtIcFullImpresstion(string placement, string adsId, string adnet)
        {
            if (onNativeIcFullImpresstion != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeIcFullImpresstion(placement, adsId, adnet);
                });
            }
        }
        public void onNtIcFullClick(string placement, string adsId, string adnet)
        {
            if (onNativeIcFullClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onNativeIcFullClick(placement, adsId, adnet);
                //});
            }
        }
        public void onNtIcFullPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onNativeIcFullPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeIcFullPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        #endregion
        #region CB Native Pg Full
        public void onNtPgFullLoaded(string placement, string adsId, string adnet)
        {
            if (onNativePgFullLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativePgFullLoaded(placement, adsId, adnet);
                });
            }
        }
        public void onNtPgFullLoadFail(string placement, string adsId, string err)
        {
            if (onNativePgFullLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativePgFullLoadFail(placement, adsId, err);
                });
            }
        }
        public void onNtPgFullFailedToShow(string placement, string adsId, string adnet, string err)
        {
            if (onNativePgFullFailedToShow != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativePgFullFailedToShow(placement, adsId, adnet, err);
                });
            }
        }
        public void onNtPgFullShowed(string placement, string adsId, string adnet)
        {
            if (onNativePgFullShowed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativePgFullShowed(placement, adsId, adnet);
                });
            }
        }
        public void onNtPgFullDismissed(string placement, string adsId, string adnet)
        {
            if (onNativePgFullDismissed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativePgFullDismissed(placement, adsId, adnet);
                });
            }
        }
        public void onNtPgFullImpresstion(string placement, string adsId, string adnet)
        {
            if (onNativePgFullImpresstion != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativePgFullImpresstion(placement, adsId, adnet);
                });
            }
        }
        public void onNtPgFullClick(string placement, string adsId, string adnet)
        {
            if (onNativePgFullClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onNativePgFullClick(placement, adsId, adnet);
                //});
            }
        }
        public void onNtPgFullPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onNativePgFullPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativePgFullPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        #endregion
        #region CB Full
        public void onFullLoaded(string placement, string adsId, string adnet)
        {
            if (onInterstitialLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialLoaded(placement, adsId, adnet);
                });
            }
        }
        public void onFullLoadFail(string placement, string adsId, string err)
        {
            if (onInterstitialLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialLoadFail(placement, adsId, err);
                });
            }
        }
        public void onFullFailedToShow(string placement, string adsId, string adnet, string err)
        {
            if (onInterstitialFailedToShow != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialFailedToShow(placement, adsId, adnet, err);
                });
            }
        }
        public void onFullShowed(string placement, string adsId, string adnet)
        {
            if (onInterstitialShowed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialShowed(placement, adsId, adnet);
                });
            }
        }
        public void onFullDismissed(string placement, string adsId, string adnet)
        {
            if (onInterstitialDismissed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialDismissed(placement, adsId, adnet);
                });
            }
        }
        public void onFullImpresstion(string placement, string adsId, string adnet)
        {
            if (onInterstitialImpresstion != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialImpresstion(placement, adsId, adnet);
                });
            }
        }
        public void onFullClick(string placement, string adsId, string adnet)
        {
            if (onInterstitialClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onInterstitialClick(placement, adsId, adnet);
                //});
            }
        }
        public void onFullPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onInterstitialPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        #endregion
        #region CB Full RW Inter
        public void onFullRwInterLoaded(string placement, string adsId, string adnet)
        {
            if (onInterRwInterLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterRwInterLoaded(placement, adsId, adnet);
                });
            }
        }
        public void onFullRwInterLoadFail(string placement, string adsId, string err)
        {
            if (onInterRwInterLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterRwInterLoadFail(placement, adsId, err);
                });
            }
        }
        public void onFullRwInterFailedToShow(string placement, string adsId, string adnet, string err)
        {
            if (onInterRwInterFailedToShow != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterRwInterFailedToShow(placement, adsId, adnet, err);
                });
            }
        }
        public void onFullRwInterShowed(string placement, string adsId, string adnet)
        {
            if (onInterRwInterShowed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterRwInterShowed(placement, adsId, adnet);
                });
            }
        }
        public void onFullRwInterDismissed(string placement, string adsId, string adnet)
        {
            if (onInterRwInterDismissed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterRwInterDismissed(placement, adsId, adnet);
                });
            }
        }
        public void onFullRwInterImpresstion(string placement, string adsId, string adnet)
        {
            if (onInterRwInterImpresstion != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterRwInterImpresstion(placement, adsId, adnet);
                });
            }
        }
        public void onFullRwInterClick(string placement, string adsId, string adnet)
        {
            if (onInterRwInterClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onInterRwInterClick(placement, adsId, adnet);
                //});
            }
        }
        public void onFullRwInterReward(string placement, string adsId, string adnet)
        {
            if (onInterRwInterReward != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterRwInterReward(placement, adsId, adnet);
                });
            }
        }
        public void onFullRwInterPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onInterRwInterPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterRwInterPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        #endregion
        #region CB Full RW RW
        public void onFullRwRwLoaded(string placement, string adsId, string adnet)
        {
            if (onInterRwRwLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterRwRwLoaded(placement, adsId, adnet);
                });
            }
        }
        public void onFullRwRwLoadFail(string placement, string adsId, string err)
        {
            if (onInterRwRwLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterRwRwLoadFail(placement, adsId, err);
                });
            }
        }
        public void onFullRwRwFailedToShow(string placement, string adsId, string adnet, string err)
        {
            if (onInterRwRwFailedToShow != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterRwRwFailedToShow(placement, adsId, adnet, err);
                });
            }
        }
        public void onFullRwRwShowed(string placement, string adsId, string adnet)
        {
            if (onInterRwRwShowed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterRwRwShowed(placement, adsId, adnet);
                });
            }
        }
        public void onFullRwRwDismissed(string placement, string adsId, string adnet)
        {
            if (onInterRwRwDismissed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterRwRwDismissed(placement, adsId, adnet);
                });
            }
        }
        public void onFullRwRwImpresstion(string placement, string adsId, string adnet)
        {
            if (onInterRwRwImpresstion != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterRwRwImpresstion(placement, adsId, adnet);
                });
            }
        }
        public void onFullRwRwClick(string placement, string adsId, string adnet)
        {
            if (onInterRwRwClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onInterRwRwClick(placement, adsId, adnet);
                //});
            }
        }
        public void onFullRwRwReward(string placement, string adsId, string adnet)
        {
            if (onInterRwRwReward != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterRwRwReward(placement, adsId, adnet);
                });
            }
        }
        public void onFullRwRwPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onInterRwRwPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterRwRwPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        #endregion
        #region CB Gift
        public void onGiftLoaded(string placement, string adsId, string adnet)
        {
            if (onRewardLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRewardLoaded(placement, adsId, adnet);
                });
            }
        }
        public void onGiftLoadFail(string placement, string adsId, string err)
        {
            if (onRewardLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRewardLoadFail(placement, adsId, err);
                });
            }
        }
        public void onGiftFailedToShow(string placement, string adsId, string adnet, string err)
        {
            if (onRewardFailedToShow != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRewardFailedToShow(placement, adsId, adnet, err);
                });
            }
        }
        public void onGiftShowed(string placement, string adsId, string adnet)
        {
            if (onRewardShowed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRewardShowed(placement, adsId, adnet);
                });
            }
        }
        public void onGiftDismissed(string placement, string adsId, string adnet)
        {
            if (onRewardDismissed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRewardDismissed(placement, adsId, adnet);
                });
            }
        }
        public void onGiftImpresstion(string placement, string adsId, string adnet)
        {
            if (onRewardImpresstion != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRewardImpresstion(placement, adsId, adnet);
                });
            }
        }
        public void onGiftClick(string placement, string adsId, string adnet)
        {
            if (onRewardClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onRewardClick(placement, adsId, adnet);
                //});
            }
        }
        public void onGiftReward(string placement, string adsId, string adnet)
        {
            if (onRewardReward != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRewardReward(placement, adsId, adnet);
                });
            }
        }
        public void onGiftPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onRewardPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRewardPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        #endregion
        #region CB Native Gift
        public void onNtGiftLoaded(string placement, string adsId, string adnet)
        {
            if (onNativeGiftLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeGiftLoaded(placement, adsId, adnet);
                });
            }
        }
        public void onNtGiftLoadFail(string placement, string adsId, string err)
        {
            if (onNativeGiftLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeGiftLoadFail(placement, adsId, err);
                });
            }
        }
        public void onNtGiftFailedToShow(string placement, string adsId, string adnet, string err)
        {
            if (onNativeGiftFailedToShow != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeGiftFailedToShow(placement, adsId, adnet, err);
                });
            }
        }
        public void onNtGiftShowed(string placement, string adsId, string adnet)
        {
            if (onNativeGiftShowed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeGiftShowed(placement, adsId, adnet);
                });
            }
        }
        public void onNtGiftDismissed(string placement, string adsId, string adnet)
        {
            if (onNativeGiftDismissed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeGiftDismissed(placement, adsId, adnet);
                });
            }
        }
        public void onNtGiftImpresstion(string placement, string adsId, string adnet)
        {
            if (onNativeGiftImpresstion != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeGiftImpresstion(placement, adsId, adnet);
                });
            }
        }
        public void onNtGiftClick(string placement, string adsId, string adnet)
        {
            if (onNativeGiftClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onNativeGiftClick(placement, adsId, adnet);
                //});
            }
        }
        public void onNtGiftPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onNativeGiftPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeGiftPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        #endregion
        #endregion

#elif UNITY_IOS || UNITY_IPHONE

        #region Callbacks from ios.
        #region CB OpenAd
        public void iOSCBOpenAdLoaded(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onOpenAdLoaded != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onOpenAdLoaded(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBonOpenAdLoadFail(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onOpenAdLoadFail != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onOpenAdLoadFail(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBOpenAdFailedToShow(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 4)
            {
                if (onOpenAdFailedToShow != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onOpenAdFailedToShow(arr[0], arr[1], arr[2], arr[3]);
                    });
                }
            }
        }
        public void iOSCBOpenAdShowed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onOpenAdShowed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onOpenAdShowed(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBOpenAdDismissed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onOpenAdDismissed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onOpenAdDismissed(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBOpenAdImpresstion(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onOpenAdImpresstion != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onOpenAdImpresstion(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBOpenAdClick(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onOpenAdClick != null)
                {
                    //AdsProcessCB.Instance().Enqueue(() =>
                    //{
                        onOpenAdClick(arr[0], arr[1], arr[2]);
                    //});
                }
            }
        }
        public void iOSCBOpenAdPaid(string param)
        {
            SdkUtil.logd("admobmy full paid=" + param);
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 6)
            {
                if (onOpenAdPaid != null)
                {
                    int precisionType = int.Parse(arr[3]);
                    string currencyCode = arr[4];
                    long valueMicros = long.Parse(arr[5]);
                    onOpenAdPaid(arr[0], arr[1], arr[2], precisionType, currencyCode, valueMicros);
                }
            }
        }
        #endregion
        #region CB Banner
        public void iOSCBBannerLoaded(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNLoaded != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNLoaded(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBannerLoadFail(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNLoadFail != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNLoadFail(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBannerClose(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNClose != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNClose(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBannerOpen(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNOpen != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNOpen(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBannerClick(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNClick != null)
                {
                    //AdsProcessCB.Instance().Enqueue(() =>
                    //{
                        onBNClick(arr[0], arr[1], arr[2]);
                    //});
                }
            }
        }
        public void iOSCBBannerImpression(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNImpression != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNImpression(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBannerPaid(string param)
        {
            SdkUtil.logd("admobmy banner paid=" + param);
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 6)
            {
                int precisionType = int.Parse(arr[3]);
                string currencyCode = arr[4];
                long valueMicros = long.Parse(arr[5]);
                if (onBNPaid != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNPaid(arr[0], arr[1], arr[2], precisionType, currencyCode, valueMicros);
                    });
                }
            }
        }
        #endregion
        #region CB Collaspe
        public void iOSCBBannerClLoaded(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNClLoaded != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNClLoaded(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBannerClLoadFail(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNClLoadFail != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNClLoadFail(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBannerClClose(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNClClose != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNClClose(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBannerClOpen(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNClOpen != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNClOpen(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBannerClClick(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNClClick != null)
                {
                    //AdsProcessCB.Instance().Enqueue(() =>
                    //{
                        onBNClClick(arr[0], arr[1], arr[2]);
                    //});
                }
            }
        }
        public void iOSCBBannerClImpression(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNClImpression != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNClImpression(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBannerClPaid(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 6)
            {
                int precisionType = int.Parse(arr[3]);
                string currencyCode = arr[4];
                long valueMicros = long.Parse(arr[5]);
                if (onBNClPaid != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNClPaid(arr[0], arr[1], arr[2], precisionType, currencyCode, valueMicros);
                    });
                }
            }
        }
        #endregion
        #region CB Rect
        public void iOSCBBannerRectLoaded(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNRectLoaded != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNRectLoaded(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBannerRectLoadFail(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNRectLoadFail != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNRectLoadFail(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBannerRectClose(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNRectClose != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNRectClose(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBannerRectOpen(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNRectOpen != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNRectOpen(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBannerRectClick(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNRectClick != null)
                {
                    //AdsProcessCB.Instance().Enqueue(() =>
                    //{
                        onBNRectClick(arr[0], arr[1], arr[2]);
                    //});
                }
            }
        }
        public void iOSCBBannerRectImpression(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNRectImpression != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNRectImpression(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBannerRectPaid(string param)
        {
            SdkUtil.logd("admobmy bannerRect paid=" + param);
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 6)
            {
                int precisionType = int.Parse(arr[3]);
                string currencyCode = arr[4];
                long valueMicros = long.Parse(arr[5]);
                onBNRectPaid(arr[0], arr[1], arr[2], precisionType, currencyCode, valueMicros);
            }
        }
        #endregion
        #region CB Bnnt
        public void iOSCBBNNTLoaded(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNNativeLoaded != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNNativeLoaded(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBNNTLoadFail(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNNativeLoadFail != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNNativeLoadFail(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBNNTShowed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNNativeOpen != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNNativeOpen(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBNNTImpresstion(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNNativeImpression != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNNativeImpression(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBNNTClick(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNNativeClick != null)
                {
                    //AdsProcessCB.Instance().Enqueue(() =>
                    //{
                        onBNNativeClick(arr[0], arr[1], arr[2]);
                    //});
                }
            }
        }
        public void iOSCBBNNTDismissed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onBNNativeClose != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onBNNativeClose(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBBNNTPaid(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 6)
            {
                int precisionType = int.Parse(arr[3]);
                string currencyCode = arr[4];
                long valueMicros = long.Parse(arr[5]);
                onBNNativePaid(arr[0], arr[1], arr[2], precisionType, currencyCode, valueMicros);
            }
        }
        #endregion
        #region CB Native collapse
        public void iOSCBNtClLoaded(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeClLoaded != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeClLoaded(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtClLoadFail(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeClLoadFail != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeClLoadFail(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtClFailedToShow(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 4)
            {
                if (onNativeClFailedToShow != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeClFailedToShow(arr[0], arr[1], arr[2], arr[3]);
                    });
                }
            }
        }
        public void iOSCBNtClShowed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeClShowed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeClShowed(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtClDismissed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeClDismissed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeClDismissed(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtClImpresstion(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeClClick != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeClClick(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtClClick(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeClImpresstion != null)
                {
                    //AdsProcessCB.Instance().Enqueue(() =>
                    //{
                        onNativeClImpresstion(arr[0], arr[1], arr[2]);
                    //});
                }
            }
        }
        public void iOSCBNtClPaid(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 6)
            {
                int precisionType = int.Parse(arr[3]);
                string currencyCode = arr[4];
                long valueMicros = long.Parse(arr[5]);
                if (onNativeClPaid != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeClPaid(arr[0], arr[1], arr[2], precisionType, currencyCode, valueMicros);
                    });
                }
            }
        }
        #endregion
        #region CB Native Full
        public void iOSCBNtFullLoaded(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeFullLoaded != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullLoaded(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtFullLoadFail(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeFullLoadFail != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullLoadFail(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtFullFailedToShow(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 4)
            {
                if (onNativeFullFailedToShow != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullFailedToShow(arr[0], arr[1], arr[2], arr[3]);
                    });
                }
            }
        }
        public void iOSCBNtFullShowed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeFullShowed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullShowed(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtFullDismissed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeFullDismissed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullDismissed(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtFullFinishShow(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeFullFinishShow != null)
                {
                    onNativeFullFinishShow(arr[0], arr[1], arr[2]);
                }
            }
        }
        public void iOSCBNtFullImpresstion(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeFullImpresstion != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullImpresstion(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtFullClick(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeFullClick != null)
                {
                    //AdsProcessCB.Instance().Enqueue(() =>
                    //{
                        onNativeFullClick(arr[0], arr[1], arr[2]);
                    //});
                }
            }
        }
        public void iOSCBNtFullPaid(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 6)
            {
                int precisionType = int.Parse(arr[3]);
                string currencyCode = arr[4];
                long valueMicros = long.Parse(arr[5]);
                if (onNativeFullPaid != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullPaid(arr[0], arr[1], arr[2], precisionType, currencyCode, valueMicros);
                    });
                }
            }
        }
        #endregion
        #region CB Native Pg Full
        public void iOSCBNtPgFullLoaded(string param)
        {
            //onNativePgFullLoaded(placement, adsId, adnet);
        }
        public void iOSCBNtPgFullLoadFail(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativePgFullLoadFail != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativePgFullLoadFail(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtPgFullFailedToShow(string param)
        {
            //onNativePgFullFailedToShow(placement, adsId, adnet, err);
        }
        public void iOSCBNtPgFullShowed(string param)
        {
            //onNativePgFullShowed(placement, adsId, adnet);
        }
        public void iOSCBNtPgFullDismissed(string param)
        {
            //onNativePgFullDismissed(placement, adsId, adnet);
        }
        public void iOSCBNtPgFullImpresstion(string param)
        {
            //onNativePgFullImpresstion(placement, adsId, adnet);
        }
        public void iOSCBNtPgFullClick(string param)
        {
            //onNativePgFullClick(placement, adsId, adnet);
        }
        public void iOSCBNtPgFullPaid(string param)
        {
            //onNativePgFullPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
        }
        #endregion
        #region CB Full
        public void iOSCBFullLoaded(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onInterstitialLoaded != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onInterstitialLoaded(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBonFullLoadFail(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onInterstitialLoadFail != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onInterstitialLoadFail(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBFullFailedToShow(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 4)
            {
                if (onInterstitialFailedToShow != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onInterstitialFailedToShow(arr[0], arr[1], arr[2], arr[3]);
                    });
                }
            }
        }
        public void iOSCBFullShowed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onInterstitialShowed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onInterstitialShowed(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBFullDismissed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onInterstitialDismissed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onInterstitialDismissed(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBFullFinishShow(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onInterstitialFinishShow != null)
                {
                    onInterstitialFinishShow(arr[0], arr[1], arr[2]);
                }
            }
        }
        public void iOSCBFullImpresstion(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onInterstitialImpresstion != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onInterstitialImpresstion(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBFullClick(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onInterstitialClick != null)
                {
                    //AdsProcessCB.Instance().Enqueue(() =>
                    //{
                        onInterstitialClick(arr[0], arr[1], arr[2]);
                    //});
                }
            }
        }
        public void iOSCBFullPaid(string param)
        {
            SdkUtil.logd("admobmy full paid=" + param);
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 6)
            {
                int precisionType = int.Parse(arr[3]);
                string currencyCode = arr[4];
                long valueMicros = long.Parse(arr[5]);
                onInterstitialPaid(arr[0], arr[1], arr[2], precisionType, currencyCode, valueMicros);
            }
        }
        #endregion
        #region CB gift
        public void iOSCBGiftLoaded(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onRewardLoaded != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onRewardLoaded(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBGiftLoadFail(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onRewardLoadFail != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onRewardLoadFail(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBGiftFailedToShow(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 4)
            {
                if (onRewardFailedToShow != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onRewardFailedToShow(arr[0], arr[1], arr[2], arr[3]);
                    });
                }
            }
        }
        public void iOSCBGiftShowed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onRewardShowed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onRewardShowed(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBGiftDismissed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onRewardDismissed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onRewardDismissed(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBGiftFinishShow(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onRewardFinishShow != null)
                {
                    onRewardFinishShow(arr[0], arr[1], arr[2]);
                }
            }
        }
        public void iOSCBGiftImpresstion(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onRewardImpresstion != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onRewardImpresstion(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBGiftClick(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onRewardClick != null)
                {
                    //AdsProcessCB.Instance().Enqueue(() =>
                    //{
                        onRewardClick(arr[0], arr[1], arr[2]);
                    //});
                }
            }
        }
        public void iOSCBGiftReward(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onRewardReward != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onRewardReward(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBGiftPaid(string param)
        {
            SdkUtil.logd("admobmy gift paid=" + param);
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 6)
            {
                int precisionType = int.Parse(arr[3]);
                string currencyCode = arr[4];
                long valueMicros = long.Parse(arr[5]);
                onRewardPaid(arr[0], arr[1], arr[2], precisionType, currencyCode, valueMicros);
            }
        }
        #endregion
        #region CB Native Gift
        public void iOSCBNtGiftLoaded(string param)
        {
            //onNativeGiftLoaded(placement, adsId, adnet);
        }
        public void iOSCBNtGiftLoadFail(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeGiftLoadFail != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeGiftLoadFail(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtGiftFailedToShow(string param)
        {
            //onNativeGiftFailedToShow(placement, adsId, adnet, err);
        }
        public void iOSCBNtGiftShowed(string param)
        {
            //onNativeGiftShowed(placement, adsId, adnet);
        }
        public void iOSCBNtGiftDismissed(string param)
        {
            //onNativeGiftDismissed(placement, adsId, adnet);
        }
        public void iOSCBNtGiftImpresstion(string param)
        {
            //onNativeGiftImpresstion(placement, adsId, adnet);
        }
        public void iOSCBNtGiftClick(string param)
        {
            //onNativeGiftClick(placement, adsId, adnet);
        }
        public void iOSCBNtGiftPaid(string param)
        {
            //onNativeGiftPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
        }
        #endregion
        #endregion
#endif
    }
}
