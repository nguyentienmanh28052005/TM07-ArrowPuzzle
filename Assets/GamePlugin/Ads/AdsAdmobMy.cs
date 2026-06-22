//#define ENABLE_ADS_ADMOB
//#define USE_NATIVE_UNITY

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
using UnityEngine.UI;
using GoogleMobileAds.Api;
#endif

namespace mygame.sdk
{
    public partial class AdsAdmobMy : AdsBase
    {

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY

#endif
        long timeShowBanner = 0;
        float bnDxCenter;
        float bnClDxCenter;
        float bnRectDxCenter;
        float bnRectDyVertical;
        bool isShowHighPriorityBanner = false;

        private bool isBannerHigh = false;
        private bool isGiftHigh = false;

        float tShowBannerNm = -1;
        float tShowBannerCl = -1;
        float tShowBannerRect = -1;
        float tChangeCl2Nm = -1;
        int flagChangecl2Nm = 0;
        bool isShowingCollapse = false;
        int statusShowCl = 0;
        int StatusClViewShow = 0;// not over all case

        bool openadisnew = true;
        bool bnnmisnew = true;
        bool bnntisnew = true;
        bool bnntclisnew = true;
        bool bnclisnew = true;
        bool bnrectisnew = true;
        bool rectntisnew = true;
        bool nativefullisnew = true;
        bool nativeicfullisnew = true;
        bool ntpgfullisnew = true;
        bool fullisnew = true;
        bool fullRwInterisnew = true;
        bool fullRwRwisnew = true;
        bool giftisnew = true;
        bool giftnativeisnew = true;
        bool giftfullisnew = true;

        static bool isdoinit = true;
        public static int timeDeltaLoad = 0;
        public static int flagLoadAll = 0xff;

        public override void InitAds()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            isEnable = true;
#endif
            if (isdoinit)
            {
                Debug.Log("mysdk: admob InitAds");
                isdoinit = false;
                //#if ADMOB_MAIN_AD || UNITY_IOS || UNITY_IPHONE
                AdsAdmobMyBridge.Instance.Initialize();
                //#endif
                StartCoroutine(waitInitSuc());
            }
            else
            {
                Debug.Log("mysdk: admob InitAds but inited");
            }
        }

        private void initData()
        {
            Debug.Log("mysdk: admob initData");
            AdsAdmobMyBridge.Instance.setLog(SdkUtil.isLog());
            //AdsAdmobMyBridge.Instance.setTestDevices("8FCCEB68875BBA9C2EF3AEEEA2D953D8");//d8
            string memtargeting = PlayerPrefs.GetString("cf_admob_targeting", "0,0,3");
            targetTing(memtargeting);

            dicPLOpenAd.Clear();
            dicPLBanner.Clear();
            dicPLNativeCl.Clear();
            dicPLCl.Clear();
            dicPLRect.Clear();
            dicPLBnNt.Clear();
            dicPLRectNt.Clear();
            dicPLNative.Clear();
            dicPLNtFull.Clear();
            dicPLNtIcFull.Clear();
            dicPLNtPgFull.Clear();
            dicPLFull.Clear();
            dicPLFullRwInter.Clear();
            dicPLFullRwRw.Clear();
            dicPLGift.Clear();
            dicPLNtGift.Clear();

            initOpenAd();
            initBannerNm();
            initBnNt();
            initNativeCl();
            initBannerCl();
            initBannerRect();
            initRectNt();
            initNative();
            initNativeFull();
            initNativeIcFull();
            initNativePgFull();
            initFull();
            initFullRwInter();
            initFullRwRw();
            initGift();
            initNativeGift();
            fullAdNetwork = "admob";
            fullRwInterAdNetwork = "admob";
            fullRwRwAdNetwork = "admob";
            giftAdNetwork = "admob";

            timeDeltaLoad = PlayerPrefs.GetInt("cf_deltime_load_admob2id", AppConfig.AdmobDeltaTimeLoadNext);
            flagLoadAll = PlayerPrefs.GetInt("cf_admob_load_all", AppConfig.AdmobLoadAll);

            configBannerRefresh();
            configTouchexcludeBn();
            int flagTrackingTiktok = PlayerPrefs.GetInt("cf_trackingtiktok", AppConfig.AdmobTrckingTiktok);
            AdsAdmobMyBridge.Instance.trackingTiktok(flagTrackingTiktok);
        }

        IEnumerator waitInitSuc()
        {
            yield return new WaitForSeconds(1.5f);
            Debug.Log("mysdk: admob waitInitSuc");
            advhelper.onAdsInitSuc(0);
        }

        public void targetTing(string cfdata)
        {
            if (cfdata != null && cfdata.Length > 3)
            {
                string[] arrtargeting = cfdata.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (arrtargeting.Length == 3)
                {
                    int child = 0;
                    int underage = 0;
                    int rating = 3;
                    int.TryParse(arrtargeting[0], out child);
                    int.TryParse(arrtargeting[1], out underage);
                    int.TryParse(arrtargeting[2], out rating);
                    AdsAdmobMyBridge.Instance.targetingAdContent(child == 1, underage == 1, rating);
                }
            }
        }

        public override void AdsAwake()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            isEnable = true;
#endif
        }

        private void Start()
        {
#if ENABLE_ADS_ADMOB && (ENABLE_ADS_ADMOB_NATIVE || USE_NATIVE_UNITY)
            //Debug.Log("mysdk: admob version:" + MobileAds.GetPlatformVersion().ToString());
#endif

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdsAdmobMyBridge.onOpenAdLoaded += OnOpenAdLoadedEvent;
            AdsAdmobMyBridge.onOpenAdLoadFail += OnOpenAdFailedEvent;
            AdsAdmobMyBridge.onOpenAdShowed += OnOpenAdDisplayedEvent;
            AdsAdmobMyBridge.onOpenAdImpresstion += onOpenAdImpresstionEvent;
            AdsAdmobMyBridge.onOpenAdClick += OnOpenAdClickEvent;
            AdsAdmobMyBridge.onOpenAdFailedToShow += onOpenAdFailedToShow;
            AdsAdmobMyBridge.onOpenAdDismissed += OnOpenAdDismissedEvent;
            AdsAdmobMyBridge.onOpenAdPaid += OnOpenAdAdPaidEvent;

            AdsAdmobMyBridge.onBNLoaded += OnBannerAdLoadedEvent;
            AdsAdmobMyBridge.onBNLoadFail += OnBannerAdLoadFailedEvent;
            AdsAdmobMyBridge.onBNImpression += OnBannerImpression;
            AdsAdmobMyBridge.onBNClick += OnBannerClickEvent;
            AdsAdmobMyBridge.onBNPaid += OnBannerAdPaidEvent;

            AdsAdmobMyBridge.onBNClLoaded += OnBannerClAdLoadedEvent;
            AdsAdmobMyBridge.onBNClLoadFail += OnBannerClAdLoadFailedEvent;
            AdsAdmobMyBridge.onBNClImpression += OnBannerClImpression;
            AdsAdmobMyBridge.onBNClOpen += OnBannerClOpen;
            AdsAdmobMyBridge.onBNClClick += OnBannerClClickEvent;
            AdsAdmobMyBridge.onBNClClose += OnBannerClClose;
            AdsAdmobMyBridge.onBNClPaid += OnBannerClAdPaidEvent;

            AdsAdmobMyBridge.onBNRectLoaded += OnBannerRectAdLoadedEvent;
            AdsAdmobMyBridge.onBNRectLoadFail += OnBannerRectAdLoadFailedEvent;
            AdsAdmobMyBridge.onBNRectImpression += OnBannerRectImpression;
            AdsAdmobMyBridge.onBNRectClick += OnBannerRectClickEvent;
            AdsAdmobMyBridge.onBNRectPaid += OnBannerRectAdPaidEvent;

            AdsAdmobMyBridge.onBNNativeLoaded += OnBnNtAdLoadedEvent;
            AdsAdmobMyBridge.onBNNativeLoadFail += OnBnNtAdLoadFailedEvent;
            AdsAdmobMyBridge.onBNNativeImpression += OnBnNtImpression;
            AdsAdmobMyBridge.onBNNativeClick += OnBnNtClickEvent;
            AdsAdmobMyBridge.onBNNativePaid += OnBnNtAdPaidEvent;

            AdsAdmobMyBridge.onNativeClLoaded += OnNativeClLoadedEvent;
            AdsAdmobMyBridge.onNativeClLoadFail += OnNativeClFailedEvent;
            AdsAdmobMyBridge.onNativeClShowed += OnNativeClDisplayedEvent;
            AdsAdmobMyBridge.onNativeClImpresstion += OnNativeClImpresstionEvent;
            AdsAdmobMyBridge.onNativeClClick += OnNativeClClickEvent;
            AdsAdmobMyBridge.onNativeClFailedToShow += OnNativeClFailedToShow;
            AdsAdmobMyBridge.onNativeClDismissed += OnNativeClDismissedEvent;
            AdsAdmobMyBridge.onNativeClPaid += OnNativeClAdPaidEvent;

            AdsAdmobMyBridge.onRectNativeLoaded += OnRectNtAdLoadedEvent;
            AdsAdmobMyBridge.onRectNativeLoadFail += OnRectNtAdLoadFailedEvent;
            AdsAdmobMyBridge.onRectNativeImpression += OnRectNtImpression;
            AdsAdmobMyBridge.onRectNativeClick += OnRectNtClickEvent;
            AdsAdmobMyBridge.onRectNativePaid += OnRectNtAdPaidEvent;

            AdsAdmobMyBridge.onNativeFullLoaded += OnNativeFullLoadedEvent;
            AdsAdmobMyBridge.onNativeFullLoadFail += OnNativeFullFailedEvent;
            AdsAdmobMyBridge.onNativeFullShowed += OnNativeFullDisplayedEvent;
            AdsAdmobMyBridge.onNativeFullImpresstion += OnNativeFullImpresstionEvent;
            AdsAdmobMyBridge.onNativeFullClick += OnNativeFullClickEvent;
            AdsAdmobMyBridge.onNativeFullFailedToShow += onNativeFullFailedToShow;
            AdsAdmobMyBridge.onNativeFullDismissed += OnNativeFullDismissedEvent;
            AdsAdmobMyBridge.onNativeFullFinishShow += OnNativeFullFinishShowEvent;
            AdsAdmobMyBridge.onNativeFullPaid += OnNativeFullAdPaidEvent;

            AdsAdmobMyBridge.onNativeIcFullLoaded += OnNativeIcFullLoadedEvent;
            AdsAdmobMyBridge.onNativeIcFullLoadFail += OnNativeIcFullFailedEvent;
            AdsAdmobMyBridge.onNativeIcFullShowed += OnNativeIcFullDisplayedEvent;
            AdsAdmobMyBridge.onNativeIcFullImpresstion += OnNativeIcFullImpresstionEvent;
            AdsAdmobMyBridge.onNativeIcFullClick += OnNativeIcFullClickEvent;
            AdsAdmobMyBridge.onNativeIcFullFailedToShow += onNativeIcFullFailedToShow;
            AdsAdmobMyBridge.onNativeIcFullDismissed += OnNativeIcFullDismissedEvent;
            AdsAdmobMyBridge.onNativeIcFullFinishShow += OnNativeIcFullFinishShowEvent;
            AdsAdmobMyBridge.onNativeIcFullPaid += OnNativeIcFullAdPaidEvent;

            AdsAdmobMyBridge.onNativePgFullLoaded += OnNativePgFullLoadedEvent;
            AdsAdmobMyBridge.onNativePgFullLoadFail += OnNativePgFullFailedEvent;
            AdsAdmobMyBridge.onNativePgFullShowed += OnNativePgFullDisplayedEvent;
            AdsAdmobMyBridge.onNativePgFullImpresstion += OnNativePgFullImpresstionEvent;
            AdsAdmobMyBridge.onNativePgFullClick += OnNativePgFullClickEvent;
            AdsAdmobMyBridge.onNativePgFullFailedToShow += OnNativePgFullFailedToShow;
            AdsAdmobMyBridge.onNativePgFullDismissed += OnNativePgFullDismissedEvent;
            AdsAdmobMyBridge.onNativePgFullFinishShow += OnNativePgFullFinishShowEvent;
            AdsAdmobMyBridge.onNativePgFullPaid += OnNativePgFullAdPaidEvent;

            AdsAdmobMyBridge.onInterstitialLoaded += OnInterstitialLoadedEvent;
            AdsAdmobMyBridge.onInterstitialLoadFail += OnInterstitialFailedEvent;
            AdsAdmobMyBridge.onInterstitialShowed += OnInterstitialDisplayedEvent;
            AdsAdmobMyBridge.onInterstitialImpresstion += OnInterstitialImpresstionEvent;
            AdsAdmobMyBridge.onInterstitialClick += OnInterstitialClickEvent;
            AdsAdmobMyBridge.onInterstitialFailedToShow += onInterstitialFailedToShow;
            AdsAdmobMyBridge.onInterstitialDismissed += OnInterstitialDismissedEvent;
            AdsAdmobMyBridge.onInterstitialFinishShow += OnInterstitialFinishShowEvent;
            AdsAdmobMyBridge.onInterstitialPaid += OnInterstitialAdPaidEvent;

            AdsAdmobMyBridge.onInterRwInterLoaded += OnInterRwInterLoadedEvent;
            AdsAdmobMyBridge.onInterRwInterLoadFail += OnInterRwInterLoadFailEvent;
            AdsAdmobMyBridge.onInterRwInterFailedToShow += OnInterRwInterFailedToShowEvent;
            AdsAdmobMyBridge.onInterRwInterImpresstion += OnInterRwInterImpresstionEvent;
            AdsAdmobMyBridge.onInterRwInterShowed += OnInterRwInterShowedEvent;
            AdsAdmobMyBridge.onInterRwInterClick += OnInterRwInterClickEvent;
            AdsAdmobMyBridge.onInterRwInterDismissed += OnInterRwInterDismissedEvent;
            AdsAdmobMyBridge.onInterRwInterFinishShow += OnInterRwInterFinishShowEvent;
            AdsAdmobMyBridge.onInterRwInterReward += OnInterRwInterRewardEvent;
            AdsAdmobMyBridge.onInterRwInterPaid += OnInterRwInterPaidEvent;

            AdsAdmobMyBridge.onInterRwRwLoaded += OnInterRwRwLoadedEvent;
            AdsAdmobMyBridge.onInterRwRwLoadFail += OnInterRwRwLoadFailEvent;
            AdsAdmobMyBridge.onInterRwRwFailedToShow += OnInterRwRwFailedToShowEvent;
            AdsAdmobMyBridge.onInterRwRwImpresstion += OnInterRwRwImpresstionEvent;
            AdsAdmobMyBridge.onInterRwRwShowed += OnInterRwRwShowedEvent;
            AdsAdmobMyBridge.onInterRwRwClick += OnInterRwRwClickEvent;
            AdsAdmobMyBridge.onInterRwRwDismissed += OnInterRwRwDismissedEvent;
            AdsAdmobMyBridge.onInterRwRwFinishShow += OnInterRwRwFinishShowEvent;
            AdsAdmobMyBridge.onInterRwRwReward += OnInterRwRwRewardEvent;
            AdsAdmobMyBridge.onInterRwRwPaid += OnInterRwRwPaidEvent;

            AdsAdmobMyBridge.onRewardLoaded += OnRewardedAdLoadedEvent;
            AdsAdmobMyBridge.onRewardLoadFail += OnRewardedAdFailedEvent;
            AdsAdmobMyBridge.onRewardFailedToShow += OnRewardedAdFailedToDisplayEvent;
            AdsAdmobMyBridge.onRewardImpresstion += OnRewardedImpresstionEvent;
            AdsAdmobMyBridge.onRewardShowed += OnRewardedAdDisplayedEvent;
            AdsAdmobMyBridge.onRewardClick += OnRewardedAdClickEvent;
            AdsAdmobMyBridge.onRewardDismissed += OnRewardedAdDismissedEvent;
            AdsAdmobMyBridge.onRewardFinishShow += OnRewardedAdFinishShowEvent;
            AdsAdmobMyBridge.onRewardReward += OnRewardedAdReceivedRewardEvent;
            AdsAdmobMyBridge.onRewardPaid += OnRewardedAdPaidEvent;

            AdsAdmobMyBridge.onNativeGiftLoaded += OnNativeGiftLoadedEvent;
            AdsAdmobMyBridge.onNativeGiftLoadFail += OnNativeGiftFailedEvent;
            AdsAdmobMyBridge.onNativeGiftShowed += OnNativeGiftDisplayedEvent;
            AdsAdmobMyBridge.onNativeGiftImpresstion += OnNativeGiftImpresstionEvent;
            AdsAdmobMyBridge.onNativeGiftClick += OnNativeGiftClickEvent;
            AdsAdmobMyBridge.onNativeGiftFailedToShow += onNativeGiftFailedToShow;
            AdsAdmobMyBridge.onNativeGiftDismissed += OnNativeGiftDismissedEvent;
            AdsAdmobMyBridge.onNativeGiftFinishShow += OnNativeGiftFinishShowEvent;
            AdsAdmobMyBridge.onNativeGiftPaid += OnNativeGiftAdPaidEvent;

            initData();
#endif
        }

        private void OnDestroy()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdsAdmobMyBridge.onOpenAdLoaded -= OnOpenAdLoadedEvent;
            AdsAdmobMyBridge.onOpenAdLoadFail -= OnOpenAdFailedEvent;
            AdsAdmobMyBridge.onOpenAdShowed -= OnOpenAdDisplayedEvent;
            AdsAdmobMyBridge.onOpenAdImpresstion -= onOpenAdImpresstionEvent;
            AdsAdmobMyBridge.onOpenAdClick -= OnOpenAdClickEvent;
            AdsAdmobMyBridge.onOpenAdFailedToShow -= onOpenAdFailedToShow;
            AdsAdmobMyBridge.onOpenAdDismissed -= OnOpenAdDismissedEvent;
            AdsAdmobMyBridge.onOpenAdPaid -= OnOpenAdAdPaidEvent;

            AdsAdmobMyBridge.onBNLoaded -= OnBannerAdLoadedEvent;
            AdsAdmobMyBridge.onBNLoadFail -= OnBannerAdLoadFailedEvent;
            AdsAdmobMyBridge.onBNImpression -= OnBannerImpression;
            AdsAdmobMyBridge.onBNClick -= OnBannerClickEvent;
            AdsAdmobMyBridge.onBNPaid -= OnBannerAdPaidEvent;

            AdsAdmobMyBridge.onBNClLoaded -= OnBannerClAdLoadedEvent;
            AdsAdmobMyBridge.onBNClLoadFail -= OnBannerClAdLoadFailedEvent;
            AdsAdmobMyBridge.onBNClImpression -= OnBannerClImpression;
            AdsAdmobMyBridge.onBNClOpen -= OnBannerClOpen;
            AdsAdmobMyBridge.onBNClClick -= OnBannerClClickEvent;
            AdsAdmobMyBridge.onBNClClose -= OnBannerClClose;
            AdsAdmobMyBridge.onBNClPaid -= OnBannerClAdPaidEvent;

            AdsAdmobMyBridge.onBNRectLoaded -= OnBannerRectAdLoadedEvent;
            AdsAdmobMyBridge.onBNRectLoadFail -= OnBannerRectAdLoadFailedEvent;
            AdsAdmobMyBridge.onBNRectImpression -= OnBannerRectImpression;
            AdsAdmobMyBridge.onBNRectClick -= OnBannerRectClickEvent;
            AdsAdmobMyBridge.onBNRectPaid -= OnBannerRectAdPaidEvent;

            AdsAdmobMyBridge.onBNNativeLoaded -= OnBnNtAdLoadedEvent;
            AdsAdmobMyBridge.onBNNativeLoadFail -= OnBnNtAdLoadFailedEvent;
            AdsAdmobMyBridge.onBNNativeImpression -= OnBnNtImpression;
            AdsAdmobMyBridge.onBNNativeClick -= OnBnNtClickEvent;
            AdsAdmobMyBridge.onBNNativePaid -= OnBnNtAdPaidEvent;

            AdsAdmobMyBridge.onNativeClLoaded -= OnNativeClLoadedEvent;
            AdsAdmobMyBridge.onNativeClLoadFail -= OnNativeClFailedEvent;
            AdsAdmobMyBridge.onNativeClShowed -= OnNativeClDisplayedEvent;
            AdsAdmobMyBridge.onNativeClImpresstion -= OnNativeClImpresstionEvent;
            AdsAdmobMyBridge.onNativeClClick -= OnNativeClClickEvent;
            AdsAdmobMyBridge.onNativeClFailedToShow -= OnNativeClFailedToShow;
            AdsAdmobMyBridge.onNativeClDismissed -= OnNativeClDismissedEvent;
            AdsAdmobMyBridge.onNativeClPaid -= OnNativeClAdPaidEvent;

            AdsAdmobMyBridge.onRectNativeLoaded -= OnRectNtAdLoadedEvent;
            AdsAdmobMyBridge.onRectNativeLoadFail -= OnRectNtAdLoadFailedEvent;
            AdsAdmobMyBridge.onRectNativeImpression -= OnRectNtImpression;
            AdsAdmobMyBridge.onRectNativeClick -= OnRectNtClickEvent;
            AdsAdmobMyBridge.onRectNativePaid -= OnRectNtAdPaidEvent;

            AdsAdmobMyBridge.onNativeFullLoaded -= OnNativeFullLoadedEvent;
            AdsAdmobMyBridge.onNativeFullLoadFail -= OnNativeFullFailedEvent;
            AdsAdmobMyBridge.onNativeFullShowed -= OnNativeFullDisplayedEvent;
            AdsAdmobMyBridge.onNativeFullImpresstion -= OnNativeFullImpresstionEvent;
            AdsAdmobMyBridge.onNativeFullClick -= OnNativeFullClickEvent;
            AdsAdmobMyBridge.onNativeFullFailedToShow -= onNativeFullFailedToShow;
            AdsAdmobMyBridge.onNativeFullDismissed -= OnNativeFullDismissedEvent;
            AdsAdmobMyBridge.onNativeFullFinishShow -= OnNativeFullFinishShowEvent;
            AdsAdmobMyBridge.onNativeFullPaid -= OnNativeFullAdPaidEvent;

            AdsAdmobMyBridge.onNativeIcFullLoaded -= OnNativeIcFullLoadedEvent;
            AdsAdmobMyBridge.onNativeIcFullLoadFail -= OnNativeIcFullFailedEvent;
            AdsAdmobMyBridge.onNativeIcFullShowed -= OnNativeIcFullDisplayedEvent;
            AdsAdmobMyBridge.onNativeIcFullImpresstion -= OnNativeIcFullImpresstionEvent;
            AdsAdmobMyBridge.onNativeIcFullClick -= OnNativeIcFullClickEvent;
            AdsAdmobMyBridge.onNativeIcFullFailedToShow -= onNativeIcFullFailedToShow;
            AdsAdmobMyBridge.onNativeIcFullDismissed -= OnNativeIcFullDismissedEvent;
            AdsAdmobMyBridge.onNativeIcFullFinishShow -= OnNativeIcFullFinishShowEvent;
            AdsAdmobMyBridge.onNativeIcFullPaid -= OnNativeIcFullAdPaidEvent;

            AdsAdmobMyBridge.onNativePgFullLoaded -= OnNativePgFullLoadedEvent;
            AdsAdmobMyBridge.onNativePgFullLoadFail -= OnNativePgFullFailedEvent;
            AdsAdmobMyBridge.onNativePgFullShowed -= OnNativePgFullDisplayedEvent;
            AdsAdmobMyBridge.onNativePgFullImpresstion -= OnNativePgFullImpresstionEvent;
            AdsAdmobMyBridge.onNativePgFullClick -= OnNativePgFullClickEvent;
            AdsAdmobMyBridge.onNativePgFullFailedToShow -= OnNativePgFullFailedToShow;
            AdsAdmobMyBridge.onNativePgFullDismissed -= OnNativePgFullDismissedEvent;
            AdsAdmobMyBridge.onNativePgFullFinishShow -= OnNativePgFullFinishShowEvent;
            AdsAdmobMyBridge.onNativePgFullPaid -= OnNativePgFullAdPaidEvent;

            AdsAdmobMyBridge.onInterstitialLoaded -= OnInterstitialLoadedEvent;
            AdsAdmobMyBridge.onInterstitialLoadFail -= OnInterstitialFailedEvent;
            AdsAdmobMyBridge.onInterstitialShowed -= OnInterstitialDisplayedEvent;
            AdsAdmobMyBridge.onInterstitialImpresstion -= OnInterstitialImpresstionEvent;
            AdsAdmobMyBridge.onInterstitialClick -= OnInterstitialClickEvent;
            AdsAdmobMyBridge.onInterstitialFailedToShow -= onInterstitialFailedToShow;
            AdsAdmobMyBridge.onInterstitialDismissed -= OnInterstitialDismissedEvent;
            AdsAdmobMyBridge.onInterstitialFinishShow -= OnInterstitialFinishShowEvent;
            AdsAdmobMyBridge.onInterstitialPaid -= OnInterstitialAdPaidEvent;

            AdsAdmobMyBridge.onInterRwInterLoaded -= OnInterRwInterLoadedEvent;
            AdsAdmobMyBridge.onInterRwInterLoadFail -= OnInterRwInterLoadFailEvent;
            AdsAdmobMyBridge.onInterRwInterFailedToShow -= OnInterRwInterFailedToShowEvent;
            AdsAdmobMyBridge.onInterRwInterImpresstion -= OnInterRwInterImpresstionEvent;
            AdsAdmobMyBridge.onInterRwInterShowed -= OnInterRwInterShowedEvent;
            AdsAdmobMyBridge.onInterRwInterClick -= OnInterRwInterClickEvent;
            AdsAdmobMyBridge.onInterRwInterDismissed -= OnInterRwInterDismissedEvent;
            AdsAdmobMyBridge.onInterRwInterFinishShow -= OnInterRwInterFinishShowEvent;
            AdsAdmobMyBridge.onInterRwInterReward -= OnInterRwInterRewardEvent;
            AdsAdmobMyBridge.onInterRwInterPaid -= OnInterRwInterPaidEvent;

            AdsAdmobMyBridge.onInterRwRwLoaded -= OnInterRwRwLoadedEvent;
            AdsAdmobMyBridge.onInterRwRwLoadFail -= OnInterRwRwLoadFailEvent;
            AdsAdmobMyBridge.onInterRwRwFailedToShow -= OnInterRwRwFailedToShowEvent;
            AdsAdmobMyBridge.onInterRwRwImpresstion -= OnInterRwRwImpresstionEvent;
            AdsAdmobMyBridge.onInterRwRwShowed -= OnInterRwRwShowedEvent;
            AdsAdmobMyBridge.onInterRwRwClick -= OnInterRwRwClickEvent;
            AdsAdmobMyBridge.onInterRwRwDismissed -= OnInterRwRwDismissedEvent;
            AdsAdmobMyBridge.onInterRwRwFinishShow -= OnInterRwRwFinishShowEvent;
            AdsAdmobMyBridge.onInterRwRwReward -= OnInterRwRwRewardEvent;
            AdsAdmobMyBridge.onInterRwRwPaid -= OnInterRwRwPaidEvent;

            AdsAdmobMyBridge.onRewardLoaded -= OnRewardedAdLoadedEvent;
            AdsAdmobMyBridge.onRewardLoadFail -= OnRewardedAdFailedEvent;
            AdsAdmobMyBridge.onRewardFailedToShow -= OnRewardedAdFailedToDisplayEvent;
            AdsAdmobMyBridge.onRewardImpresstion -= OnRewardedImpresstionEvent;
            AdsAdmobMyBridge.onRewardShowed -= OnRewardedAdDisplayedEvent;
            AdsAdmobMyBridge.onRewardClick -= OnRewardedAdClickEvent;
            AdsAdmobMyBridge.onRewardDismissed -= OnRewardedAdDismissedEvent;
            AdsAdmobMyBridge.onRewardFinishShow -= OnRewardedAdFinishShowEvent;
            AdsAdmobMyBridge.onRewardReward -= OnRewardedAdReceivedRewardEvent;
            AdsAdmobMyBridge.onRewardPaid -= OnRewardedAdPaidEvent;

            AdsAdmobMyBridge.onNativeGiftLoaded -= OnNativeGiftLoadedEvent;
            AdsAdmobMyBridge.onNativeGiftLoadFail -= OnNativeGiftFailedEvent;
            AdsAdmobMyBridge.onNativeGiftShowed -= OnNativeGiftDisplayedEvent;
            AdsAdmobMyBridge.onNativeGiftImpresstion -= OnNativeGiftImpresstionEvent;
            AdsAdmobMyBridge.onNativeGiftClick -= OnNativeGiftClickEvent;
            AdsAdmobMyBridge.onNativeGiftFailedToShow -= onNativeGiftFailedToShow;
            AdsAdmobMyBridge.onNativeGiftDismissed -= OnNativeGiftDismissedEvent;
            AdsAdmobMyBridge.onNativeGiftFinishShow -= OnNativeGiftFinishShowEvent;
            AdsAdmobMyBridge.onNativeGiftPaid -= OnNativeGiftAdPaidEvent;
#endif
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
        private void Update()
        {
            if (tShowBannerNm >= 0 && advhelper.currConfig.timeAutoReloadBanner >= 5)
            {
                var adpl = getPlBanner(advhelper.memPlacementBn, 0);
                if (adpl != null && adpl.isRealShow)
                {
                    tShowBannerNm += Time.deltaTime * Time.timeScale;
                    if (tShowBannerNm >= advhelper.currConfig.timeAutoReloadBanner)
                    {
                        SdkUtil.logd($"ads bn admobmy autoreload");
                        tShowBannerNm = -1;
                        StartCoroutine(waitLoadNextBanner(adpl));
                    }
                }
            }
            if (flagChangecl2Nm == 2 && tChangeCl2Nm >= 0 && advhelper.currConfig.timeChangeCl2Banner >= 5)
            {
                AdPlacementBanner adpl = getPlBanner(advhelper.memPlacementBn, 1);
                if (adpl != null && adpl.isRealShow)
                {
                    tChangeCl2Nm += Time.deltaTime * Time.timeScale;
                    if (tChangeCl2Nm >= advhelper.currConfig.timeChangeCl2Banner)
                    {
                        SdkUtil.logd($"ads bncl admobmy change collapse to banner");
                        tChangeCl2Nm = -1;
                        tShowBannerCl = -1;
                        advhelper.hideBannerCollapse();
                        if (adpl.posBanner == 0)
                        {
                            advhelper.showBanner(advhelper.memPlacementBn, AD_BANNER_POS.TOP, advhelper.bnOrien, 0, bnClWidth, advhelper.bnMaxH, bnClDxCenter);
                        }
                        else
                        {
                            advhelper.showBanner(advhelper.memPlacementBn, AD_BANNER_POS.BOTTOM, advhelper.bnOrien, 0, bnClWidth, advhelper.bnMaxH, bnClDxCenter);
                        }
                    }
                }
            }
            if (tShowBannerCl >= 0 && advhelper.currConfig.timeAutoReloadBannerCl >= 5)
            {
                AdPlacementBanner adpl = getPlBanner(advhelper.memPlacementCl, 1);
                if (adpl != null)
                {
                    tShowBannerCl += Time.deltaTime * Time.timeScale;
                    if (tShowBannerCl >= advhelper.currConfig.timeAutoReloadBannerCl)
                    {
                        SdkUtil.logd($"ads bncl admobmy autoreload");
                        tShowBannerCl = -1;
                        StartCoroutine(waitLoadNextBannerCl(adpl));
                    }
                }
            }
            if (tShowBannerRect >= 0 && advhelper.currConfig.timeAutoReloadBanner >= 5)
            {
                AdPlacementBanner adpl = getPlBanner(advhelper.memPlacementRect, 2);
                if (adpl.isRealShow)
                {
                    tShowBannerRect += Time.deltaTime * Time.timeScale;
                    if (tShowBannerRect >= advhelper.currConfig.timeAutoReloadBanner)
                    {
                        SdkUtil.logd($"ads rect admobmy autoreload");
                        tShowBannerRect = -1;
                        StartCoroutine(waitLoadNextBannerRect(adpl));
                    }
                }
            }
        }
#endif
        public void checkFloorInitRemote()
        {
            openadisnew = false;
            bnnmisnew = false;
            bnntisnew = false;
            bnntclisnew = false;
            bnclisnew = false;
            bnrectisnew = false;
            rectntisnew = false;
            nativefullisnew = false;
            nativeicfullisnew = false;
            ntpgfullisnew = false;
            fullRwInterisnew = false;
            fullRwRwisnew = false;
            fullisnew = false;
            giftisnew = false;
            giftnativeisnew = false;
            giftfullisnew = false;

            AdsHelper.Instance.isNtfullNewId = 0;

            //================openad
            bool isContain = true;
            if (advhelper.currConfig.adCfPlacementOpen.Length > 0)
            {
                string[] listpl = advhelper.currConfig.adCfPlacementOpen.Split(new char[] { '#' });
                foreach (string plitem in listpl)
                {
                    string[] plcf = plitem.Split(new char[] { ',' });
                    if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
                    {
                        string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string ikey in arrkeys)
                        {
                            if (dicPLOpenAd.ContainsKey(ikey))
                            {
                                string[] ids = plcf[2].Split(new char[] { ';' });
                                foreach (string adid in ids)
                                {
                                    isContain = dicPLOpenAd[ikey].adECPM.isContain(adid);
                                    if (!isContain)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (ikey != null && ikey.Length > 2)
                                {
                                    isContain = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!isContain)
            {
                openadisnew = true;
            }

            //================Banner
            isContain = true;
            if (advhelper.currConfig.adCfPlacementBanner.Length > 0)
            {
                string[] listpl = advhelper.currConfig.adCfPlacementBanner.Split(new char[] { '#' });
                foreach (string plitem in listpl)
                {
                    string[] plcf = plitem.Split(new char[] { ',' });
                    if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
                    {
                        string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string ikey in arrkeys)
                        {
                            if (dicPLBanner.ContainsKey(ikey))
                            {
                                string[] ids = plcf[2].Split(new char[] { ';' });
                                foreach (string adid in ids)
                                {
                                    isContain = dicPLBanner[ikey].adECPM.isContain(adid);
                                    if (!isContain)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (ikey != null && ikey.Length > 2)
                                {
                                    isContain = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!isContain)
            {
                bnnmisnew = true;
            }

            //================Banner native
            isContain = true;
            if (advhelper.currConfig.adCfPlacementBnNt.Length > 0)
            {
                string[] listpl = advhelper.currConfig.adCfPlacementBnNt.Split(new char[] { '#' });
                foreach (string plitem in listpl)
                {
                    string[] plcf = plitem.Split(new char[] { ',' });
                    if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
                    {
                        string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string ikey in arrkeys)
                        {
                            if (dicPLBnNt.ContainsKey(ikey))
                            {
                                string[] ids = plcf[2].Split(new char[] { ';' });
                                foreach (string adid in ids)
                                {
                                    isContain = dicPLBnNt[ikey].adECPM.isContain(adid);
                                    if (!isContain)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (ikey != null && ikey.Length > 2)
                                {
                                    isContain = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!isContain)
            {
                bnntisnew = true;
            }

            //================Banner native cl
            isContain = true;
            if (advhelper.currConfig.adCfPlacementNattiveCl.Length > 0)
            {
                string[] listpl = advhelper.currConfig.adCfPlacementNattiveCl.Split(new char[] { '#' });
                foreach (string plitem in listpl)
                {
                    string[] plcf = plitem.Split(new char[] { ',' });
                    if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
                    {
                        string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string ikey in arrkeys)
                        {
                            if (dicPLNativeCl.ContainsKey(ikey))
                            {
                                string[] ids = plcf[2].Split(new char[] { ';' });
                                foreach (string adid in ids)
                                {
                                    isContain = dicPLNativeCl[ikey].adECPM.isContain(adid);
                                    if (!isContain)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (ikey != null && ikey.Length > 2)
                                {
                                    isContain = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!isContain)
            {
                bnntclisnew = true;
            }

            //================Collapse
            isContain = true;
            if (advhelper.currConfig.adCfPlacementCollapse.Length > 0)
            {
                string[] listpl = advhelper.currConfig.adCfPlacementCollapse.Split(new char[] { '#' });
                foreach (string plitem in listpl)
                {
                    string[] plcf = plitem.Split(new char[] { ',' });
                    if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
                    {
                        string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string ikey in arrkeys)
                        {
                            if (dicPLCl.ContainsKey(ikey))
                            {
                                string[] ids = plcf[2].Split(new char[] { ';' });
                                foreach (string adid in ids)
                                {
                                    isContain = dicPLCl[ikey].adECPM.isContain(adid);
                                    if (!isContain)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (ikey != null && ikey.Length > 2)
                                {
                                    isContain = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!isContain)
            {
                bnclisnew = true;
            }

            //================Rect
            isContain = true;
            if (advhelper.currConfig.adCfPlacementRect.Length > 0)
            {
                string[] listpl = advhelper.currConfig.adCfPlacementRect.Split(new char[] { '#' });
                foreach (string plitem in listpl)
                {
                    string[] plcf = plitem.Split(new char[] { ',' });
                    if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
                    {
                        string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string ikey in arrkeys)
                        {
                            if (dicPLRect.ContainsKey(ikey))
                            {
                                string[] ids = plcf[2].Split(new char[] { ';' });
                                foreach (string adid in ids)
                                {
                                    isContain = dicPLRect[ikey].adECPM.isContain(adid);
                                    if (!isContain)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (ikey != null && ikey.Length > 2)
                                {
                                    isContain = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!isContain)
            {
                bnrectisnew = true;
            }

            //================Rect Native
            isContain = true;
            if (advhelper.currConfig.adCfPlacementRectNt.Length > 0)
            {
                string[] listpl = advhelper.currConfig.adCfPlacementRectNt.Split(new char[] { '#' });
                foreach (string plitem in listpl)
                {
                    string[] plcf = plitem.Split(new char[] { ',' });
                    if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
                    {
                        string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string ikey in arrkeys)
                        {
                            if (dicPLRectNt.ContainsKey(ikey))
                            {
                                string[] ids = plcf[2].Split(new char[] { ';' });
                                foreach (string adid in ids)
                                {
                                    isContain = dicPLRectNt[ikey].adECPM.isContain(adid);
                                    if (!isContain)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (ikey != null && ikey.Length > 2)
                                {
                                    isContain = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!isContain)
            {
                rectntisnew = true;
            }

            //================NtFull
            isContain = true;
            if (advhelper.currConfig.adCfPlacementNtFull.Length > 0)
            {
                string[] listpl = advhelper.currConfig.adCfPlacementNtFull.Split(new char[] { '#' });
                foreach (string plitem in listpl)
                {
                    string[] plcf = plitem.Split(new char[] { ',' });
                    if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
                    {
                        string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string ikey in arrkeys)
                        {
                            if (dicPLNtFull.ContainsKey(ikey))
                            {
                                string[] ids = plcf[2].Split(new char[] { ';' });
                                foreach (string adid in ids)
                                {
                                    isContain = dicPLNtFull[ikey].adECPM.isContain(adid);
                                    if (!isContain)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (ikey != null && ikey.Length > 2)
                                {
                                    isContain = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!isContain)
            {
                nativefullisnew = true;
                AdsHelper.Instance.isNtfullNewId = 1;
            }

            //================NticFull
            isContain = true;
            if (advhelper.currConfig.adCfPlacementNtIcFull.Length > 0)
            {
                string[] listpl = advhelper.currConfig.adCfPlacementNtIcFull.Split(new char[] { '#' });
                foreach (string plitem in listpl)
                {
                    string[] plcf = plitem.Split(new char[] { ',' });
                    if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
                    {
                        string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string ikey in arrkeys)
                        {
                            if (dicPLNtIcFull.ContainsKey(ikey))
                            {
                                string[] ids = plcf[2].Split(new char[] { ';' });
                                foreach (string adid in ids)
                                {
                                    isContain = dicPLNtIcFull[ikey].adECPM.isContain(adid);
                                    if (!isContain)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (ikey != null && ikey.Length > 2)
                                {
                                    isContain = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!isContain)
            {
                nativeicfullisnew = true;
            }

            //================Nt Pg Full
            isContain = true;
            if (advhelper.currConfig.adCfPlacementNtPgFull.Length > 0)
            {
                string[] listpl = advhelper.currConfig.adCfPlacementNtPgFull.Split(new char[] { '#' });
                foreach (string plitem in listpl)
                {
                    string[] plcf = plitem.Split(new char[] { ',' });
                    if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
                    {
                        string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string ikey in arrkeys)
                        {
                            if (dicPLNtPgFull.ContainsKey(ikey))
                            {
                                string[] adpos = plcf[2].Split(new char[] { ';' });
                                string[] ids = adpos[0].Split(new char[] { '|' });
                                foreach (string adid in ids)
                                {
                                    isContain = dicPLNtPgFull[ikey].adECPM.isContain(adid);
                                    if (!isContain)
                                    {
                                        break;
                                    }
                                }
                                if (adpos.Length > 1 && isContain)
                                {
                                    string[] ids2 = adpos[1].Split(new char[] { '|' });
                                    foreach (string adid in ids2)
                                    {
                                        isContain = dicPLNtPgFull[ikey].adECPM2.isContain(adid);
                                        if (!isContain)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (ikey != null && ikey.Length > 2)
                                {
                                    isContain = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!isContain)
            {
                ntpgfullisnew = true;
            }

            //================Full
            isContain = true;
            if (advhelper.currConfig.adCfPlacementFull.Length > 0)
            {
                string[] listpl = advhelper.currConfig.adCfPlacementFull.Split(new char[] { '#' });
                foreach (string plitem in listpl)
                {
                    string[] plcf = plitem.Split(new char[] { ',' });
                    if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
                    {
                        string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string ikey in arrkeys)
                        {
                            if (dicPLFull.ContainsKey(ikey))
                            {
                                string[] ids = plcf[2].Split(new char[] { ';' });
                                foreach (string adid in ids)
                                {
                                    isContain = dicPLFull[ikey].adECPM.isContain(adid);
                                    if (!isContain)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (ikey != null && ikey.Length > 2)
                                {
                                    isContain = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!isContain)
            {
                fullisnew = true;
            }

            //================Full Rw inter
            isContain = true;
            if (advhelper.currConfig.adCfPlacementFullRwInter.Length > 0)
            {
                string[] listpl = advhelper.currConfig.adCfPlacementFullRwInter.Split(new char[] { '#' });
                foreach (string plitem in listpl)
                {
                    string[] plcf = plitem.Split(new char[] { ',' });
                    if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
                    {
                        string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string ikey in arrkeys)
                        {
                            if (dicPLFullRwInter.ContainsKey(ikey))
                            {
                                string[] ids = plcf[2].Split(new char[] { ';' });
                                foreach (string adid in ids)
                                {
                                    isContain = dicPLFullRwInter[ikey].adECPM.isContain(adid);
                                    if (!isContain)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (ikey != null && ikey.Length > 2)
                                {
                                    isContain = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!isContain)
            {
                fullRwInterisnew = true;
            }

            //================Full Rw Rw
            isContain = true;
            if (advhelper.currConfig.adCfPlacementFullRwRw.Length > 0)
            {
                string[] listpl = advhelper.currConfig.adCfPlacementFullRwRw.Split(new char[] { '#' });
                foreach (string plitem in listpl)
                {
                    string[] plcf = plitem.Split(new char[] { ',' });
                    if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
                    {
                        string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string ikey in arrkeys)
                        {
                            if (dicPLFullRwRw.ContainsKey(ikey))
                            {
                                string[] ids = plcf[2].Split(new char[] { ';' });
                                foreach (string adid in ids)
                                {
                                    isContain = dicPLFullRwRw[ikey].adECPM.isContain(adid);
                                    if (!isContain)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (ikey != null && ikey.Length > 2)
                                {
                                    isContain = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!isContain)
            {
                fullRwRwisnew = true;
            }

            //================Gift
            isContain = true;
            if (advhelper.currConfig.adCfPlacementGift.Length > 0)
            {
                string[] listpl = advhelper.currConfig.adCfPlacementGift.Split(new char[] { '#' });
                foreach (string plitem in listpl)
                {
                    string[] plcf = plitem.Split(new char[] { ',' });
                    if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
                    {
                        string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string ikey in arrkeys)
                        {
                            if (dicPLGift.ContainsKey(ikey))
                            {
                                string[] ids = plcf[2].Split(new char[] { ';' });
                                foreach (string adid in ids)
                                {
                                    isContain = dicPLGift[ikey].adECPM.isContain(adid);
                                    if (!isContain)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (ikey != null && ikey.Length > 2)
                                {
                                    isContain = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!isContain)
            {
                giftisnew = true;
            }

            //================Gift nt
            isContain = true;
            if (advhelper.currConfig.adCfPlacementNtGift.Length > 0)
            {
                string[] listpl = advhelper.currConfig.adCfPlacementNtGift.Split(new char[] { '#' });
                foreach (string plitem in listpl)
                {
                    string[] plcf = plitem.Split(new char[] { ',' });
                    if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
                    {
                        string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string ikey in arrkeys)
                        {
                            if (dicPLNtGift.ContainsKey(ikey))
                            {
                                string[] adpos = plcf[2].Split(new char[] { ';' });
                                string[] ids = adpos[0].Split(new char[] { '|' });
                                foreach (string adid in ids)
                                {
                                    isContain = dicPLNtGift[ikey].adECPM.isContain(adid);
                                    if (!isContain)
                                    {
                                        break;
                                    }
                                }
                                if (adpos.Length > 1 && isContain)
                                {
                                    string[] ids2 = adpos[1].Split(new char[] { '|' });
                                    foreach (string adid in ids2)
                                    {
                                        isContain = dicPLNtGift[ikey].adECPM2.isContain(adid);
                                        if (!isContain)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (ikey != null && ikey.Length > 2)
                                {
                                    isContain = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!isContain)
            {
                giftnativeisnew = true;
            }

            //================Gift full
            isContain = true;
            if (advhelper.currConfig.adCfPlacementGiftFull.Length > 0)
            {
                string[] listpl = advhelper.currConfig.adCfPlacementGiftFull.Split(new char[] { '#' });
                foreach (string plitem in listpl)
                {
                    string[] plcf = plitem.Split(new char[] { ',' });
                    if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
                    {
                        string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string ikey in arrkeys)
                        {
                            if (dicPLGiftFull.ContainsKey(ikey))
                            {
                                string[] ids = plcf[2].Split(new char[] { ';' });
                                foreach (string adid in ids)
                                {
                                    isContain = dicPLGiftFull[ikey].adECPM.isContain(adid);
                                    if (!isContain)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (ikey != null && ikey.Length > 2)
                                {
                                    isContain = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!isContain)
            {
                giftfullisnew = true;
            }

            //=======================
            if (openadisnew)
            {
                Debug.Log("mysdk: ads admob rcv new open");
                initOpenAd();
            }
            if (bnnmisnew)
            {
                Debug.Log("mysdk: ads admob rcv new bn");
                initBannerNm();
            }
            if (bnntisnew)
            {
                Debug.Log("mysdk: ads admob rcv new bnnt");
                initBnNt();
            }
            if (bnntclisnew)
            {
                Debug.Log("mysdk: ads admob rcv new ntcl");
                initNativeCl();
            }
            if (bnclisnew)
            {
                Debug.Log("mysdk: ads admob rcv new open cl");
                initBannerCl();
            }
            if (bnrectisnew)
            {
                Debug.Log("mysdk: ads admob rcv new rect");
                initBannerRect();
            }
            if (rectntisnew)
            {
                Debug.Log("mysdk: ads admob rcv new rect native");
                initRectNt();
            }
            initNative();
            if (nativefullisnew)
            {
                Debug.Log("mysdk: ads admob rcv new ntfull");
                initNativeFull();
            }
            if (nativeicfullisnew)
            {
                Debug.Log("mysdk: ads admob rcv new nticfull");
                initNativeIcFull();
            }
            if (ntpgfullisnew)
            {
                Debug.Log("mysdk: ads admob rcv new native pg full");
                initNativePgFull();
            }
            if (fullisnew)
            {
                Debug.Log("mysdk: ads admob rcv new full");
                initFull();
            }
            if (fullRwInterisnew)
            {
                Debug.Log("mysdk: ads admob rcv new full Rw Inter");
                initFullRwInter();
            }
            if (fullRwRwisnew)
            {
                Debug.Log("mysdk: ads admob rcv new full Rw Rw");
                initFullRwRw();
            }
            if (giftisnew)
            {
                Debug.Log("mysdk: ads admob rcv new gift");
                initGift();
            }
            if (giftnativeisnew)
            {
                Debug.Log("mysdk: ads admob rcv new native gift");
                initNativeGift();
            }
            if (giftfullisnew)
            {
                Debug.Log("mysdk: ads admob rcv new gift full");
                initGiftFull();
            }
        }
        public void setCfNtFull(string scfntfullClick)
        {
            if (scfntfullClick != null && scfntfullClick.Length > 0)
            {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
                string[] arrcfntfull = scfntfullClick.Split(new char[] { ',' });
                if (arrcfntfull != null && arrcfntfull.Length >= 5)
                {
                    int v1, v2, v3, v4, v5, v6 = 1;
                    if (!int.TryParse(arrcfntfull[0], out v1))
                    {
                        v1 = 30;
                    }
                    if (!int.TryParse(arrcfntfull[1], out v2))
                    {
                        v2 = 105;
                    }
                    if (!int.TryParse(arrcfntfull[2], out v3))
                    {
                        v3 = 70;
                    }
                    if (!int.TryParse(arrcfntfull[3], out v4))
                    {
                        v4 = 2;
                    }
                    if (!int.TryParse(arrcfntfull[4], out v5))
                    {
                        v5 = 10;
                    }
                    if (arrcfntfull.Length >= 6 && !int.TryParse(arrcfntfull[5], out v6))
                    {
                        v6 = 1;
                    }
                    AdsAdmobMyBridge.Instance.setCfNtFull(v1, v2, v3, v4, v5, v6);
                }
#endif
            }
        }
        public void setCfNtdayClick(string cfdayclick)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdsAdmobMyBridge.Instance.setCfNtdayClick(cfdayclick);
#endif
        }
        public void setCfNtFullFbExcluse(string scf)
        {
            if (scf != null && scf.Length > 0)
            {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
                string[] arrCf = scf.Split(new char[] { ';' });
                if (arrCf != null && arrCf.Length >= 3)
                {
                    int v1, v2;
                    if (!int.TryParse(arrCf[0], out v1))
                    {
                        v1 = 4;
                    }
                    if (!int.TryParse(arrCf[1], out v2))
                    {
                        v2 = 5;
                    }
                    AdsAdmobMyBridge.Instance.setCfNtFullFbExcluse(v1, v2, arrCf[2]);
                    AdsFbMyBridge.Instance.setCfNtFullFbExcluse(v1, v2, arrCf[2]);
                }
#endif
            }
        }
        public void setCfNtCl(string scfntclClick)
        {
            if (scfntclClick != null && scfntclClick.Length > 0)
            {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
                string[] arrcfntfull = scfntclClick.Split(new char[] { ',' });
                if (arrcfntfull != null && arrcfntfull.Length >= 4)
                {
                    int v1, v2, v3, v4;
                    if (!int.TryParse(arrcfntfull[0], out v1))
                    {
                        v1 = 20;
                    }
                    if (!int.TryParse(arrcfntfull[1], out v2))
                    {
                        v2 = 85;
                    }
                    if (!int.TryParse(arrcfntfull[2], out v3))
                    {
                        v3 = 2;
                    }
                    if (!int.TryParse(arrcfntfull[3], out v4))
                    {
                        v4 = 50;
                    }
                    AdsAdmobMyBridge.Instance.setCfNtCl(v1, v2, v3, v4);
                }
#endif
            }
        }
        public void setCfNtClFbExcluse(string scf)
        {
            if (scf != null && scf.Length > 0)
            {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
                string[] arrCf = scf.Split(new char[] { ';' });
                if (arrCf != null && arrCf.Length >= 4)
                {
                    int v1, v2, v3;
                    if (!int.TryParse(arrCf[0], out v1))
                    {
                        v1 = 6;
                    }
                    if (!int.TryParse(arrCf[1], out v2))
                    {
                        v2 = 5;
                    }
                    if (!int.TryParse(arrCf[3], out v3))
                    {
                        v3 = 2;
                    }
                    AdsAdmobMyBridge.Instance.setCfNtClFbExcluse(v1, v2, arrCf[2], v3);
                }
#endif
            }
        }
        public void setTypeBnnt(int flagShowBnntMedia)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdsAdmobMyBridge.Instance.setTypeBnnt(flagShowBnntMedia == 1);
#endif
        }

        public void configBannerRefresh()
        {
            string bncfrefresh = PlayerPrefs.GetString("cf_admob_bn_refresh", "");
            AdsAdmobMyBridge.Instance.cfBnRefresh(bncfrefresh);
        }
        public void configTouchexcludeBn()
        {
            string cfdata = PlayerPrefs.GetString("cf_admob_bn_areatouchex", "0,0");
            string[] arrCf = cfdata.Split(new char[] { ',' });
            int w = 0, h = 0;
            if (arrCf != null && arrCf.Length >= 2)
            {
                int.TryParse(arrCf[0], out w);
                int.TryParse(arrCf[1], out h);
            }
            AdsAdmobMyBridge.Instance.cfBnAreaTouchex(w, h);
        }
        public override string getname()
        {
            return "adsmobMy";
        }
    }
}