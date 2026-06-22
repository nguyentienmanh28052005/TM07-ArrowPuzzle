//#define ENABLE_ADS_TOPON
//#define USE_AUTO_FULL
//#define USE_AUTO_GIFT
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if ENABLE_ADS_TOPON
using AnyThinkAds.Api;
using AnyThinkAds.ThirdParty.LitJson;
#endif

namespace mygame.sdk
{
#if ENABLE_ADS_TOPON
    public class AdsToponMe : AdsBase, ATSDKInitListener
#else
    public class AdsToponMe : AdsBase
#endif
    {
#if ENABLE_ADS_TOPON

#endif
        private static bool isInitAds = false;
        int posBnCurr = -1;
        private bool isAdsInited = false;

        private bool isSplashLoading = false;

        [Header("Topon iOS")]
        public string appkey = "40e8a258ddb892ee993cb96d89ddfdca";
        public string splashAdId = "b651e2d86977f6";

        public override void InitAds()
        {
#if ENABLE_ADS_TOPON
            isEnable = true;
            if (!isInitAds)
            {
                isInitAds = true;
                isAdsInited = true;
                //ATSDKAPI.setLogDebug(true);
                //ATSDKAPI.initSDK(appId, appkey, this);
            }
#endif
        }

        public override void AdsAwake()
        {
#if ENABLE_ADS_TOPON
            isEnable = true;
#endif
        }

        public override string getname()
        {
            return "topon";
        }

        private void Start()
        {
#if ENABLE_ADS_TOPON
            isInitAds = true;
            isAdsInited = true;
            fullAdNetwork = "toponme";
            giftAdNetwork = "toponme";
            ATSDKAPI.setLogDebug(true);
            ATSDKAPI.initSDK(appId, appkey, this);

            ATBannerAd.Instance.client.onAdAutoRefreshEvent += onAdAutoRefresh;
            ATBannerAd.Instance.client.onAdAutoRefreshFailureEvent += onAdAutoRefreshFail;
            ATBannerAd.Instance.client.onAdClickEvent += onAdClick;
            ATBannerAd.Instance.client.onAdCloseEvent += onAdClose;
            ATBannerAd.Instance.client.onAdCloseButtonTappedEvent += onAdCloseButtonTapped;
            ATBannerAd.Instance.client.onAdImpressEvent += onAdImpress;
            ATBannerAd.Instance.client.onAdLoadEvent += onAdLoad;
            ATBannerAd.Instance.client.onAdLoadFailureEvent += onAdLoadFail;
            ATBannerAd.Instance.client.onAdSourceAttemptEvent += startLoadingADSource;
            ATBannerAd.Instance.client.onAdSourceFilledEvent += finishLoadingADSource;
            ATBannerAd.Instance.client.onAdSourceLoadFailureEvent += failToLoadADSource;
            ATBannerAd.Instance.client.onAdSourceBiddingAttemptEvent += startBiddingADSource;
            ATBannerAd.Instance.client.onAdSourceBiddingFilledEvent += finishBiddingADSource;
            ATBannerAd.Instance.client.onAdSourceBiddingFailureEvent += failBiddingADSource;

            //
            //#if !USE_AUTO_FULL
            ATInterstitialAd.Instance.client.onAdLoadEvent += onAdFullLoad;
            ATInterstitialAd.Instance.client.onAdClickEvent += onAdFullClick;
            ATInterstitialAd.Instance.client.onAdCloseEvent += onAdFullClose;
            ATInterstitialAd.Instance.client.onAdShowEvent += onFullShow;
            ATInterstitialAd.Instance.client.onAdLoadFailureEvent += onAdFullLoadFail;
            ATInterstitialAd.Instance.client.onAdShowFailureEvent += onAdFullShowFail;
            ATInterstitialAd.Instance.client.onAdVideoStartEvent += fullStartVideoPlayback;
            ATInterstitialAd.Instance.client.onAdVideoEndEvent += fullEndVideoPlayback;
            ATInterstitialAd.Instance.client.onAdVideoFailureEvent += fullFailVideoPlayback;
            ATInterstitialAd.Instance.client.onAdSourceAttemptEvent += fullStartLoadingADSource;
            ATInterstitialAd.Instance.client.onAdSourceFilledEvent += fullFinishLoadingADSource;
            ATInterstitialAd.Instance.client.onAdSourceLoadFailureEvent += fullFailToLoadADSource;
            ATInterstitialAd.Instance.client.onAdSourceBiddingAttemptEvent += fullStartBiddingADSource;
            ATInterstitialAd.Instance.client.onAdSourceBiddingFilledEvent += fullFinishBiddingADSource;
            ATInterstitialAd.Instance.client.onAdSourceBiddingFailureEvent += fullFailBiddingADSource;
            //#else
            ATInterstitialAutoAd.Instance.client.onAdLoadEvent += onAdFullLoad;
            ATInterstitialAutoAd.Instance.client.onAdClickEvent += onAdFullClick;
            ATInterstitialAutoAd.Instance.client.onAdCloseEvent += onAdFullClose;
            ATInterstitialAutoAd.Instance.client.onAdShowEvent += onFullShow;
            ATInterstitialAutoAd.Instance.client.onAdLoadFailureEvent += onAdFullLoadFail;
            ATInterstitialAutoAd.Instance.client.onAdShowFailureEvent += onAdFullShowFail;
            ATInterstitialAutoAd.Instance.client.onAdVideoStartEvent += fullStartVideoPlayback;
            ATInterstitialAutoAd.Instance.client.onAdVideoEndEvent += fullEndVideoPlayback;
            ATInterstitialAutoAd.Instance.client.onAdVideoFailureEvent += fullFailVideoPlayback;
            //#endif

            //
            //#if !USE_AUTO_GIFT
            ATRewardedVideo.Instance.client.onAdVideoStartEvent += onAdGiftVideoStartEvent;
            ATRewardedVideo.Instance.client.onAdVideoEndEvent += onAdGiftVideoEndEvent;
            ATRewardedVideo.Instance.client.onAdVideoCloseEvent += onAdGiftVideoClosedEvent;
            ATRewardedVideo.Instance.client.onAdClickEvent += onAdGiftClick;
            ATRewardedVideo.Instance.client.onRewardEvent += onGiftReward;
            ATRewardedVideo.Instance.client.onPlayAgainStart += onRewardedVideoAdAgainPlayStart;
            ATRewardedVideo.Instance.client.onPlayAgainFailure += onRewardedVideoAdAgainPlayFail;
            ATRewardedVideo.Instance.client.onPlayAgainEnd += onRewardedVideoAdAgainPlayEnd;
            ATRewardedVideo.Instance.client.onPlayAgainClick += onRewardedVideoAdAgainPlayClicked;
            ATRewardedVideo.Instance.client.onPlayAgainReward += onAgainReward;
            ATRewardedVideo.Instance.client.onAdSourceAttemptEvent += giftStartLoadingADSource;
            ATRewardedVideo.Instance.client.onAdSourceFilledEvent += giftFinishLoadingADSource;
            ATRewardedVideo.Instance.client.onAdSourceLoadFailureEvent += giftFailToLoadADSource;
            ATRewardedVideo.Instance.client.onAdSourceBiddingAttemptEvent += giftStartBiddingADSource;
            ATRewardedVideo.Instance.client.onAdSourceBiddingFilledEvent += giftFinishBiddingADSource;
            ATRewardedVideo.Instance.client.onAdSourceBiddingFailureEvent += giftFailBiddingADSource;
            //#else
            ATRewardedAutoVideo.Instance.client.onAdLoadEvent += onAdGiftLoad;
            ATRewardedAutoVideo.Instance.client.onAdLoadFailureEvent += onAdGiftLoadFail;
            ATRewardedAutoVideo.Instance.client.onAdVideoStartEvent += onAdGiftVideoStartEvent;
            ATRewardedAutoVideo.Instance.client.onAdVideoEndEvent += onAdGiftVideoEndEvent;
            ATRewardedAutoVideo.Instance.client.onAdVideoFailureEvent += onAdGiftVideoPlayFail;
            ATRewardedAutoVideo.Instance.client.onAdClickEvent += onAdGiftClick;
            ATRewardedAutoVideo.Instance.client.onRewardEvent += onGiftReward;
            ATRewardedAutoVideo.Instance.client.onAdVideoCloseEvent += onAdGiftVideoClosedEvent;
            //#endif
#endif
        }

        private void OnDestroy()
        {
#if ENABLE_ADS_TOPON
            ATSplashAd.Instance.client.onAdLoadEvent -= onAdSplashLoad;
            ATSplashAd.Instance.client.onAdLoadTimeoutEvent -= onAdSplashLoadTimeout;
            ATSplashAd.Instance.client.onAdLoadFailureEvent -= onAdSplashLoadFailed;
            ATSplashAd.Instance.client.onAdCloseEvent -= onAdSplashClose;

            ATBannerAd.Instance.client.onAdAutoRefreshEvent -= onAdAutoRefresh;
            ATBannerAd.Instance.client.onAdAutoRefreshFailureEvent -= onAdAutoRefreshFail;
            ATBannerAd.Instance.client.onAdClickEvent -= onAdClick;
            ATBannerAd.Instance.client.onAdCloseEvent -= onAdClose;
            ATBannerAd.Instance.client.onAdCloseButtonTappedEvent -= onAdCloseButtonTapped;
            ATBannerAd.Instance.client.onAdImpressEvent -= onAdImpress;
            ATBannerAd.Instance.client.onAdLoadEvent -= onAdLoad;
            ATBannerAd.Instance.client.onAdLoadFailureEvent -= onAdLoadFail;
            ATBannerAd.Instance.client.onAdSourceAttemptEvent -= startLoadingADSource;
            ATBannerAd.Instance.client.onAdSourceFilledEvent -= finishLoadingADSource;
            ATBannerAd.Instance.client.onAdSourceLoadFailureEvent -= failToLoadADSource;
            ATBannerAd.Instance.client.onAdSourceBiddingAttemptEvent -= startBiddingADSource;
            ATBannerAd.Instance.client.onAdSourceBiddingFilledEvent -= finishBiddingADSource;
            ATBannerAd.Instance.client.onAdSourceBiddingFailureEvent -= failBiddingADSource;

            //
            //#if !USE_AUTO_FULL
            ATInterstitialAd.Instance.client.onAdLoadEvent -= onAdFullLoad;
            ATInterstitialAd.Instance.client.onAdClickEvent -= onAdFullClick;
            ATInterstitialAd.Instance.client.onAdCloseEvent -= onAdFullClose;
            ATInterstitialAd.Instance.client.onAdShowEvent -= onFullShow;
            ATInterstitialAd.Instance.client.onAdLoadFailureEvent -= onAdFullLoadFail;
            ATInterstitialAd.Instance.client.onAdShowFailureEvent -= onAdFullShowFail;
            ATInterstitialAd.Instance.client.onAdVideoStartEvent -= fullStartVideoPlayback;
            ATInterstitialAd.Instance.client.onAdVideoEndEvent -= fullEndVideoPlayback;
            ATInterstitialAd.Instance.client.onAdVideoFailureEvent -= fullFailVideoPlayback;
            ATInterstitialAd.Instance.client.onAdSourceAttemptEvent -= fullStartLoadingADSource;
            ATInterstitialAd.Instance.client.onAdSourceFilledEvent -= fullFinishLoadingADSource;
            ATInterstitialAd.Instance.client.onAdSourceLoadFailureEvent -= fullFailToLoadADSource;
            ATInterstitialAd.Instance.client.onAdSourceBiddingAttemptEvent -= fullStartBiddingADSource;
            ATInterstitialAd.Instance.client.onAdSourceBiddingFilledEvent -= fullFinishBiddingADSource;
            ATInterstitialAd.Instance.client.onAdSourceBiddingFailureEvent -= fullFailBiddingADSource;
            //#else
            ATInterstitialAutoAd.Instance.client.onAdLoadEvent -= onAdFullLoad;
            ATInterstitialAutoAd.Instance.client.onAdClickEvent -= onAdFullClick;
            ATInterstitialAutoAd.Instance.client.onAdCloseEvent -= onAdFullClose;
            ATInterstitialAutoAd.Instance.client.onAdShowEvent -= onFullShow;
            ATInterstitialAutoAd.Instance.client.onAdLoadFailureEvent -= onAdFullLoadFail;
            ATInterstitialAutoAd.Instance.client.onAdShowFailureEvent -= onAdFullShowFail;
            ATInterstitialAutoAd.Instance.client.onAdVideoStartEvent -= fullStartVideoPlayback;
            ATInterstitialAutoAd.Instance.client.onAdVideoEndEvent -= fullEndVideoPlayback;
            ATInterstitialAutoAd.Instance.client.onAdVideoFailureEvent -= fullFailVideoPlayback;
            //#endif
            //
            //#if !USE_AUTO_GIFT
            ATRewardedVideo.Instance.client.onAdVideoStartEvent -= onAdGiftVideoStartEvent;
            ATRewardedVideo.Instance.client.onAdVideoEndEvent -= onAdGiftVideoEndEvent;
            ATRewardedVideo.Instance.client.onAdVideoCloseEvent -= onAdGiftVideoClosedEvent;
            ATRewardedVideo.Instance.client.onAdClickEvent -= onAdGiftClick;
            ATRewardedVideo.Instance.client.onRewardEvent -= onGiftReward;
            ATRewardedVideo.Instance.client.onPlayAgainStart -= onRewardedVideoAdAgainPlayStart;
            ATRewardedVideo.Instance.client.onPlayAgainFailure -= onRewardedVideoAdAgainPlayFail;
            ATRewardedVideo.Instance.client.onPlayAgainEnd -= onRewardedVideoAdAgainPlayEnd;
            ATRewardedVideo.Instance.client.onPlayAgainClick -= onRewardedVideoAdAgainPlayClicked;
            ATRewardedVideo.Instance.client.onPlayAgainReward -= onAgainReward;
            ATRewardedVideo.Instance.client.onAdSourceAttemptEvent -= giftStartLoadingADSource;
            ATRewardedVideo.Instance.client.onAdSourceFilledEvent -= giftFinishLoadingADSource;
            ATRewardedVideo.Instance.client.onAdSourceLoadFailureEvent -= giftFailToLoadADSource;
            ATRewardedVideo.Instance.client.onAdSourceBiddingAttemptEvent -= giftStartBiddingADSource;
            ATRewardedVideo.Instance.client.onAdSourceBiddingFilledEvent -= giftFinishBiddingADSource;
            ATRewardedVideo.Instance.client.onAdSourceBiddingFailureEvent -= giftFailBiddingADSource;
            //#else
            ATRewardedAutoVideo.Instance.client.onAdLoadEvent -= onAdGiftLoad;
            ATRewardedAutoVideo.Instance.client.onAdLoadFailureEvent -= onAdGiftLoadFail;
            ATRewardedAutoVideo.Instance.client.onAdVideoStartEvent -= onAdGiftVideoStartEvent;
            ATRewardedAutoVideo.Instance.client.onAdVideoEndEvent -= onAdGiftVideoEndEvent;
            ATRewardedAutoVideo.Instance.client.onAdVideoFailureEvent -= onAdGiftVideoPlayFail;
            ATRewardedAutoVideo.Instance.client.onAdClickEvent -= onAdGiftClick;
            ATRewardedAutoVideo.Instance.client.onRewardEvent -= onGiftReward;
            ATRewardedAutoVideo.Instance.client.onAdVideoCloseEvent -= onAdGiftVideoClosedEvent;
            //#endif
#endif
        }

#if ENABLE_ADS_TOPON
        public void initSuccess()
        {
            Debug.Log("mysdk: ads Topon initSuccess");
            ATSplashAd.Instance.client.onAdLoadEvent += onAdSplashLoad;
            ATSplashAd.Instance.client.onAdLoadTimeoutEvent += onAdSplashLoadTimeout;
            ATSplashAd.Instance.client.onAdLoadFailureEvent += onAdSplashLoadFailed;
            ATSplashAd.Instance.client.onAdCloseEvent += onAdSplashClose;
            loadSplashAds();

#if USE_AUTO_FULL
            StartCoroutine(waiAutoFull());
#endif

#if USE_AUTO_GIFT
            StartCoroutine(waiAutoGift());
#endif
        }

        public void initFail(string msg)
        {
            Debug.Log("mysdk: ads Topon SDK initFail:" + msg);
        }
#endif

        IEnumerator waiAutoFull()
        {
            yield return new WaitForSeconds(0.5f);
#if ENABLE_ADS_TOPON
            string[] jsonFullList = { fullId };
            ATInterstitialAutoAd.Instance.addAutoLoadAdPlacementID(jsonFullList);
#endif
        }

        IEnumerator waiAutoGift()
        {
            yield return new WaitForSeconds(0.5f);
#if ENABLE_ADS_TOPON
            string[] jsonGiftList = { giftId };
            ATRewardedAutoVideo.Instance.addAutoLoadAdPlacementID(jsonGiftList);
#endif
        }

        public void loadSplashAds()
        {
#if ENABLE_ADS_TOPON
            SdkUtil.logd("ads Topon Splash load Ads");
            if (!isSplashLoading)
            {
                isSplashLoading = true;
                var splashAd = ATSplashAd.Instance;
                splashAd.loadSplashAd(splashAdId, new Dictionary<string, object>());
            }
#endif
        }
        public bool showSplashAds()
        {
#if ENABLE_ADS_TOPON
            SdkUtil.logd("ads full Topon Splash show Ads");
            var splashAd = ATSplashAd.Instance;
            if (splashAd.hasSplashAdReady(splashAdId))
            {
                splashAd.showSplashAd(splashAdId, new Dictionary<string, object>());
                return false;
            }
            else
            {
                loadSplashAds();
                return false;
            }
#else
            return false;
#endif
        }

        protected override void tryLoadBanner(AdPlacementBanner adpl)
        {
#if ENABLE_ADS_TOPON
            SdkUtil.logd("ads bn Topon tryLoadBanner");
            if (!isAdsInited)
            {
                SdkUtil.logd($"ads bn Topon tryLoadBanner not init");
                var tmpcb = cbBanner;
                cbBanner = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
                return;
            }
            if (BNTryLoad >= toTryLoad)
            {
                SdkUtil.logd("ads bn Topon tryLoadBanner over trycount");
                if (cbBanner != null)
                {
                    var tmpcb = cbBanner;
                    cbBanner = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                if (advhelper != null)
                {
                    advhelper.onBannerLoadFail(adsType);
                }
                return;
            }
            SdkUtil.logd("ads bn Topon tryLoadBanner load");
            bannerInfo.isBNLoading = true;
            bannerInfo.isBNloaded = false;
            bannerInfo.isBNRealShow = false;
            Dictionary<string, object> jsonMap = new Dictionary<string, object>();
            ATSize bnSize = new ATSize(Screen.width, 50);
            jsonMap.Add(ATBannerAdLoadingExtra.kATBannerAdLoadingExtraBannerAdSizeStruct, bnSize);

            ATBannerAd.Instance.loadBannerAd(bannerId, jsonMap);
#else
            SdkUtil.logd($"ads bn Topon tryLoadBanner not enable");
            var tmpcb = adpl.cbLoad;
            adpl.cbLoad = null;
            tmpcb(AD_State.AD_LOAD_FAIL);
#endif
        }
        public override void loadBanner(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_TOPON
            SdkUtil.logd("ads bn Topon loadBanner");
            cbBanner = cb;
            if (!bannerInfo.isBNLoading)
            {
                BNTryLoad = 0;
                tryLoadBanner(placement);
            }
            else
            {
                SdkUtil.logd("ads bn Topon loadBanner isloading");
            }
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showBanner(string placement, int pos, int width, int maxH, AdCallBack cb, float dxCenter, bool highP = false)
        {
#if ENABLE_ADS_TOPON
            SdkUtil.logd("ads bn Topon showBanner");
            bannerInfo.isBNShow = true;
            bannerInfo.posBanner = pos;
            bnWidth = width;
            posBnCurr = pos;
            if (bannerInfo.isBNloaded)
            {
                if (advhelper.isShowBNAdsMobHigh)
                {
                    Debug.Log("mysdk: ads Topon showBanner hide 4 amob high");
                    bannerInfo.isBNRealShow = false;
                    ATBannerAd.Instance.hideBannerAd(bannerId);
                }
                else
                {
                    if (!bannerInfo.isBNRealShow)
                    {
                        bannerInfo.isBNRealShow = true;
                        advhelper.hideOtherBanner(12);
                        if (pos == 0)
                        {
                            ATBannerAd.Instance.showBannerAd(bannerId, ATBannerAdLoadingExtra.kATBannerAdShowingPisitionTop);
                        }
                        else
                        {
                            ATBannerAd.Instance.showBannerAd(bannerId, ATBannerAdLoadingExtra.kATBannerAdShowingPisitionBottom);
                        }
                    }
                }
                return true;
            }
            else
            {
                SdkUtil.logd("ads bn Topon showBanner not show and load");
                loadBanner(cb);
                return false;
            }
#else
            if (cb != null)
            {
                SdkUtil.logd("ads bn Topon tryLoadBanner not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
            return false;
#endif
        }
        public override void hideBanner()
        {
#if ENABLE_ADS_TOPON
            AdPlacementBanner adpl = getPlBanner(PLBnDefault, 0);
            if (adpl != null && adpl.cbLoad != null)
            {
                adpl.isShow = false;
                adpl.isRealShow = false;
            }
            SdkUtil.logd("ads bn Topon hideBanner");
            ATBannerAd.Instance.hideBannerAd(bannerId);
#endif
        }
        public override void destroyBanner()
        {
#if ENABLE_ADS_TOPON
            SdkUtil.logd("ads bn Topon destroyBanner");
            AdPlacementBanner adpl = getPlBanner(PLBnDefault, 0);
            if (adpl != null && adpl.cbLoad != null)
            {
                adpl.isShow = false;
                adpl.isRealShow = false;
                adpl.isloaded = false;
                adpl.isLoading = false;
            }
            posBnCurr = -1;
            ATBannerAd.Instance.cleanBannerAd(bannerId);
#endif
        }
        //Native

        //
        public override void clearCurrFull(string placement)
        {
            if (getFullLoaded(placement) == 1)
            {
#if ENABLE_ADS_TOPON
                isFullLoaded = false;
#endif
            }
        }
        public override int getFullLoaded(string placement)
        {
#if ENABLE_ADS_TOPON
#if USE_AUTO_FULL
            if (ATInterstitialAutoAd.Instance.autoLoadInterstitialAdReadyForPlacementID(fullId))
            {
                return 1;
            }
            else
            {
                return 0;
            }
#else
            if (ATInterstitialAd.Instance.hasInterstitialAdReady(fullId))
            {
                return 1;
            }
            else
            {
                return 0;
            }
#endif
#else
            return 0;
#endif
        }
        protected override void tryLoadFull(AdPlacementFull adpl)
        {
#if ENABLE_ADS_TOPON
            SdkUtil.logd("ads full Topon tryLoadFull =" + fullId);
            if (!isAdsInited)
            {
                SdkUtil.logd($"ads full Topon tryLoadFull not init");
                if (cbFullLoad != null)
                {
                    var tmpcb = cbFullLoad;
                    cbFullLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            if (FullTryLoad >= toTryLoad)
            {
                SdkUtil.logd("ads full Topon tryLoadFull over try");
                if (cbFullLoad != null)
                {
                    var tmpcb = cbFullLoad;
                    cbFullLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            isFullLoading = true;
            isFullLoaded = false;
            Dictionary<string, object> jsonmap = new Dictionary<string, object>();
            if (PlayerPrefs.GetInt("cf_topon_full", 0) == 0)
            {
                jsonmap.Add(AnyThinkAds.Api.ATConst.USE_REWARDED_VIDEO_AS_INTERSTITIAL, AnyThinkAds.Api.ATConst.USE_REWARDED_VIDEO_AS_INTERSTITIAL_NO);
            }
            else
            {
                jsonmap.Add(AnyThinkAds.Api.ATConst.USE_REWARDED_VIDEO_AS_INTERSTITIAL, AnyThinkAds.Api.ATConst.USE_REWARDED_VIDEO_AS_INTERSTITIAL_YES);
            }
            ATInterstitialAd.Instance.loadInterstitialAd(fullId, jsonmap);
#else
            var tmpcb = adpl.cbLoad;
            adpl.cbLoad = null;
            tmpcb(AD_State.AD_LOAD_FAIL);
#endif
        }
        public override void loadFull(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_TOPON
            SdkUtil.logd("ads full Topon loadFull");
#if USE_AUTO_FULL
            cbFullLoad = cb;
            if (cbFullLoad != null)
            {
                SdkUtil.logd("ads full Topon loadFull topon autoload");
                var tmpcb = cbFullLoad;
                cbFullLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#else
            if (!isFullLoading && !isFullLoaded)
            {
                SdkUtil.logd("ads full Topon loadFull load");
                FullTryLoad = 0;
                tryLoadFull(placement);
            }
            else
            {
                SdkUtil.logd("ads full Topon loadFull loading={isFullLoading} or loaded={isFullLoaded}");
            }
#endif
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showFull(string placement, float timeDelay, bool isShow2, AdCallBack cb)
        {
            isFull2 = isShow2;
#if ENABLE_ADS_TOPON
            SdkUtil.logd($"ads full Topon showFull timeDelay={timeDelay}");
            cbFullShow = null;
            if (getFullLoaded(placement) > 0)
            {
                SdkUtil.logd("ads full Topon showFull show");
                FullTryLoad = 0;
                cbFullShow = cb;
                if (timeDelay > 0)
                {
                    AdsProcessCB.Instance().Enqueue(() => {
#if USE_AUTO_FULL
                        ATInterstitialAutoAd.Instance.showAutoAd(fullId);
#else
                        ATInterstitialAd.Instance.showInterstitialAd(fullId);
#endif
                    }, timeDelay);
                    return true;
                }
                else
                    {
#if USE_AUTO_FULL
                    ATInterstitialAutoAd.Instance.showAutoAd(fullId);
#else
                    ATInterstitialAd.Instance.showInterstitialAd(fullId);
#endif
                    return true;
                }
            }
#endif
            return false;
        }

        //------------------------------------------------
        public override void clearCurrGift(string placement)
        {
#if ENABLE_ADS_TOPON
            if (getGiftLoaded(placement) == 1)
            {
            }
#endif
        }
        public override int getGiftLoaded(string placement)
        {
#if ENABLE_ADS_TOPON
#if USE_AUTO_GIFT
            if (ATRewardedAutoVideo.Instance.autoLoadRewardedVideoReadyForPlacementID(giftId))
            {
                return 1;
            }
            else
            {
                return 0;
            }
#else
            if (ATRewardedVideo.Instance.hasAdReady(giftId))
            {
                return 1;
            }
            else
            {
                return 0;
            }
#endif
#else
            return 0;
#endif
        }
        protected override void tryloadGift(AdPlacementFull adpl)
        {
#if ENABLE_ADS_TOPON
            SdkUtil.logd("ads gift Topon tryloadGift =" + giftId);
            if (!isAdsInited)
            {
                SdkUtil.logd($"ads gift Topon tryloadGift not init");
                if (cbGiftLoad != null)
                {
                    var tmpcb = cbGiftLoad;
                    cbGiftLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            if (GiftTryLoad >= toTryLoad)
            {
                SdkUtil.logd("ads gift Topon tryloadGift over try");
                if (cbGiftLoad != null)
                {
                    var tmpcb = cbGiftLoad;
                    cbGiftLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            isGiftLoading = true;
            isGiftLoaded = false;

            Dictionary<string, string> jsonmap = new Dictionary<string, string>();
            ATRewardedVideo.Instance.loadVideoAd(giftId, jsonmap);
#else
            var tmpcb = adpl.cbLoad;
            adpl.cbLoad = null;
            tmpcb(AD_State.AD_LOAD_FAIL);
#endif
        }
        public override void loadGift(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_TOPON
            SdkUtil.logd("ads gift Topon loadGift");
#if USE_AUTO_GIFT
            if (cbGiftLoad != null)
            {
                SdkUtil.logd("ads gift Topon loadGift auto load gift");
                var tmpcb = cbGiftLoad;
                cbGiftLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#else
            cbGiftLoad = cb;
            if (!isGiftLoading && !isGiftLoaded)
            {
                SdkUtil.logd("ads gift Topon loadGift load");
                GiftTryLoad = 0;
                tryloadGift(placement);
            }
            else
            {
                SdkUtil.logd("ads gift Topon loadGift loading={isGiftLoading} or loaded={isGiftLoaded}");
            }
#endif
#else
            AdPlacementFull adpl = getPlGift(placement);
            if (adpl != null && adpl.cbLoad != null)
            {
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showGift(string placement, float timeDelay, AdCallBack cb)
        {
#if ENABLE_ADS_TOPON
            SdkUtil.logd($"ads gift Topon showGift timeDelay={timeDelay}");
            if (getGiftLoaded() > 0)
            {
                SdkUtil.logd("ads gift Topon showGift show");
                cbGiftShow = cb;
                isGiftLoaded = false;
                if (timeDelay > 0)
                {
                    AdsProcessCB.Instance().Enqueue(() => {
#if USE_AUTO_GIFT
                        ATRewardedAutoVideo.Instance.showAutoAd(giftId);
#else
                        ATRewardedVideo.Instance.showAd(giftId);
#endif
                    }, timeDelay);
                    return true;
                }
                else
                {
#if USE_AUTO_GIFT
                    ATRewardedAutoVideo.Instance.showAutoAd(giftId);
#else
                    ATRewardedVideo.Instance.showAd(giftId);
#endif
                    return true;
                }
            }
#endif
            return false;
        }

        //-------------------------------------------------------------------------------
#if ENABLE_ADS_TOPON
        //private void irOnImpressionDataReadyEvent(IronSourceImpressionData impressionData)
        //{
        //    if (impressionData != null)
        //    {
        //        FIRhelper.logEventAdsPaidIron(appId, impressionData.adNetwork, impressionData.adUnit, impressionData.instanceName, (double)impressionData.revenue, impressionData.country, impressionData.placement);
        //        if (impressionData.adUnit.Contains("interstitial"))
        //        {
        //            countFullTo++;
        //            if (impressionData.adNetwork.Contains("admob"))
        //            {
        //                countFullAdmob++;
        //            }
        //        }
        //        else if (impressionData.adUnit.Contains("rewarded_video"))
        //        {
        //            countGiftTo++;
        //            if (impressionData.adNetwork.Contains("admob"))
        //            {
        //                countGiftAdmob++;
        //            }
        //        }
        //        else if (impressionData.adUnit.Contains("banner"))
        //        {
        //            countBnTo++;
        //            if (impressionData.adNetwork.Contains("admob"))
        //            {
        //                countBnAdmob++;
        //            }
        //        }
        //        if (advhelper.isShowBNAdsMobHigh)
        //        {
        //            Debug.Log("mysdk: ads Topon irOnImpressionDataReadyEvent hide 4 amob high");
        //            bannerInfo.isBNRealShow = false;
        //            IronSource.Agent.hideBanner();
        //        }
        //    }
        //}
        //===================================================================================
        #region SPLASH AD
        public void onAdSplashLoad(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads Topon Splash onAdLoad");
            isSplashLoading = false;
            if (erg != null && erg.isTimeout)
            {

            }
            else
            {
                //Loading timed out, no longer call show to display the ad
            }
        }
        public void onAdSplashLoadTimeout(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads Topon Splash onAdLoadTimeout");
            isSplashLoading = false;
        }
        public void onAdSplashLoadFailed(object sender, ATAdErrorEventArgs erg)
        {
            if (erg != null)
            {
                SdkUtil.logd("ads Topon Splash onAdSplashLoadFailed: " + erg.placementId + "--erg.errorCode:" + erg.errorCode + "--msg:" + erg.errorMessage);
            }
            else
            {
                SdkUtil.logd("ads Topon Splash onAdSplashLoadFailed: ");
            }
        }
        public void onAdSplashClose(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads Topon Splash onAdSplashClose");
            loadSplashAds();
        }
        #endregion

        #region BANNER AD EVENTS
        public void onAdAutoRefresh(object sender, ATAdEventArgs erg)
        {
            if (erg != null)
            {
                SdkUtil.logd("ads bn Topon onAdAutoRefresh :" + erg.placementId);
            }
            else
            {
                SdkUtil.logd("ads bn Topon onAdAutoRefresh");
            }
        }
        public void onAdAutoRefreshFail(object sender, ATAdErrorEventArgs erg)
        {
            if (erg != null)
            {
                SdkUtil.logd("ads bn Topon onAdAutoRefreshFail : " + erg.placementId + "--erg.errorCode:" + erg.errorCode + "--msg:" + erg.errorMessage);
            }
            else
            {
                SdkUtil.logd("ads bn Topon onAdAutoRefreshFail");
            }
        }
        public void onAdClick(object sender, ATAdEventArgs erg)
        {
            if (erg != null)
            {
                SdkUtil.logd("ads bn Topon onAdClick :" + erg.placementId);
            }
            else
            {
                SdkUtil.logd("ads bn Topon onAdClick :");
            }
        }
        public void onAdClose(object sender, ATAdEventArgs erg)
        {
            if (erg != null)
            {
                SdkUtil.logd("ads bn Topon onAdClose :" + erg.placementId);
            }
            else
            {
                SdkUtil.logd("ads bn Topon onAdClose :");
            }
        }
        public void onAdCloseButtonTapped(object sender, ATAdEventArgs erg)
        {
            if (erg != null)
            {
                SdkUtil.logd("ads bn Topon onAdCloseButtonTapped :" + erg.placementId);
            }
            else
            {
                SdkUtil.logd("ads bn Topon onAdCloseButtonTapped :");
            }

        }
        public void onAdImpress(object sender, ATAdEventArgs erg)
        {
            Debug.Log("mysdk: ads bn Topon onAdImpress");
            if (advhelper.isShowBNAdsMobHigh)
            {
                Debug.Log("mysdk: ads bn Topon onAdImpress hide 4 amob high");
                bannerInfo.isBNRealShow = false;
                ATBannerAd.Instance.hideBannerAd(erg.placementId);
            }
        }
        public void onAdLoad(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads bn Topon onAdLoad");
            bannerInfo.isBNloaded = true;
            bannerInfo.isBNLoading = false;
            BNTryLoad = 0;

            if (bannerInfo.isBNShow)
            {
                SdkUtil.logd("ads bn Topon onAdLoad show");
                if (!advhelper.isShowBNAdsMobHigh)
                {
                    if (!bannerInfo.isBNRealShow)
                    {
                        bannerInfo.isBNRealShow = true;
                        advhelper.hideOtherBanner(12);
                        if (bannerInfo.posBanner == 0)
                        {
                            ATBannerAd.Instance.showBannerAd(bannerId, ATBannerAdLoadingExtra.kATBannerAdShowingPisitionTop);
                        }
                        else
                        {
                            ATBannerAd.Instance.showBannerAd(bannerId, ATBannerAdLoadingExtra.kATBannerAdShowingPisitionBottom);
                        }
                    }
                }
                else
                {
                    Debug.Log("mysdk: ads bn Topon onAdLoad hide 4 amob high");
                    bannerInfo.isBNRealShow = false;
                    ATBannerAd.Instance.hideBannerAd(erg.placementId);
                }
            }
            else
            {
                SdkUtil.logd("ads bn Topon onAdLoad hide");
                bannerInfo.isBNRealShow = false;
                ATBannerAd.Instance.hideBannerAd(erg.placementId);
            }

            if (cbBanner != null)
            {
                var tmpcb = cbBanner;
                cbBanner = null;
                tmpcb(AD_State.AD_LOAD_OK);
            }
            if (advhelper != null)
            {
                advhelper.onBannerLoadOk(adsType);
            }
        }
        public void onAdLoadFail(object sender, ATAdErrorEventArgs erg)
        {
            SdkUtil.logd("ads bn Topon onAdLoadFail");
            if (!bannerInfo.isBNloaded)
            {
                SdkUtil.logd("ads bn Topon onAdLoadFail 1");
                bannerInfo.isBNloaded = false;
                bannerInfo.isBNLoading = false;
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    BNTryLoad++;
                    tryLoadBanner();
                }, 1.0f);
            }
        }
        public void startLoadingADSource(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads bn Topon startLoadingADSource------");
        }
        public void finishLoadingADSource(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads bn Topon finishLoadingADSource------");
        }
        public void failToLoadADSource(object sender, ATAdErrorEventArgs erg)
        {
            if (erg != null)
            {
                SdkUtil.logd("ads bn Topon failToLoadADSource------erg.errorCode:" + erg.errorCode + "---erg.errorMessage:" + erg.errorMessage);
            }
            else
            {
                SdkUtil.logd("ads bn Topon failToLoadADSource------erg.errorCode:");
            }
        }
        public void startBiddingADSource(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads bn Topon startBiddingADSource------");

        }
        public void finishBiddingADSource(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads bn Topon finishBiddingADSource------");

        }
        public void failBiddingADSource(object sender, ATAdErrorEventArgs erg)
        {
            if (erg != null)
            {
                SdkUtil.logd("ads bn Topon failBiddingADSource------erg.errorCode:" + erg.errorCode + "---erg.errorMessage:" + erg.errorMessage);
            }
            else
            {
                SdkUtil.logd("ads bn Topon failBiddingADSource------erg.errorCode:");
            }
        }
        #endregion

        #region INTERSTITIAL AD EVENTS
        public void onAdFullLoad(object sender, ATAdEventArgs erg)
        {
            if (erg != null)
            {
                SdkUtil.logd("ads full Topon onAdFullLoad :" + erg.placementId);
            }
            else
            {
                SdkUtil.logd("ads full Topon onAdFullLoad :");
            }
#if !USE_AUTO_FULL
            FullTryLoad = 0;
            isFullLoading = false;
            isFullLoaded = true;
            if (cbFullLoad != null)
            {
                var tmpcb = cbFullLoad;
                cbFullLoad = null;
                tmpcb(AD_State.AD_LOAD_OK);
            }
#endif

        }
        public void onAdFullLoadFail(object sender, ATAdErrorEventArgs erg)
        {
            if (erg != null)
            {
                SdkUtil.logd("ads full Topon onAdFullLoadFail : " + erg.placementId + "--erg.errorCode:" + erg.errorCode + "--msg:" + erg.errorMessage);
            }
            else
            {
                SdkUtil.logd("ads full Topon onAdFullLoadFail : ");
            }
#if !USE_AUTO_FULL
            isFullLoading = false;
            isFullLoaded = false;
            AdsProcessCB.Instance().Enqueue(() =>
            {
                FullTryLoad++;
                tryLoadFull();
            }, 1.0f);
#endif
        }
        public void onFullShow(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads full Topon onFullShow");
            if (cbFullShow != null)
            {
                AdCallBack tmpcb = cbFullShow;
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    tmpcb(AD_State.AD_SHOW);
                });
            }
        }
        public void onAdFullClick(object sender, ATAdEventArgs erg)
        {
            if (erg != null)
            {
                SdkUtil.logd("ads full Topon onAdFullClick :" + erg.placementId);
            }
            else
            {
                SdkUtil.logd("ads full Topon onAdFullClick :");
            }

        }
        public void onAdFullShowFail(object sender, ATAdErrorEventArgs erg)
        {
            SdkUtil.logd("ads full Topon onAdFullShowFail 1");
            isFullLoaded = false;
            if (cbFullShow != null)
            {
                advhelper.onCloseFullGift(true);
                AdCallBack tmpcb = cbFullShow;
                AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
            }
            onFullClose();
        }
        public void onAdFullClose(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads full Topon onAdFullClose");
            isFullLoaded = false;
            if (cbFullShow != null)
            {
                advhelper.onCloseFullGift(true);
                AdCallBack tmpcb = cbFullShow;
                AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });
            }
            onFullClose();

            FullTryLoad = 0;
            cbFullShow = null;
        }
        public void fullStartVideoPlayback(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads full Topon StartVideoPlayback------");
        }
        public void fullEndVideoPlayback(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads full Topon EndVideoPlayback------");
        }
        public void fullFailVideoPlayback(object sender, ATAdErrorEventArgs erg)
        {
            SdkUtil.logd("ads full Topon FailVideoPlayback------");
        }
        public void fullStartLoadingADSource(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads full Topon StartLoadingADSource------");

        }
        public void fullFinishLoadingADSource(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads full Topon FinishLoadingADSource------");

        }
        public void fullFailToLoadADSource(object sender, ATAdErrorEventArgs erg)
        {
            if (erg != null)
            {
                SdkUtil.logd("ads full Topon FailToLoadADSource------erg.errorCode:" + erg.errorCode + "---erg.errorMessage:" + erg.errorMessage);
            }
            else
            {
                SdkUtil.logd("ads full Topon FailToLoadADSource------erg.errorCode:");
            }


        }
        public void fullStartBiddingADSource(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads full Topon StartBiddingADSource------");

        }
        public void fullFinishBiddingADSource(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads full Topon FinishBiddingADSource------");

        }
        public void fullFailBiddingADSource(object sender, ATAdErrorEventArgs erg)
        {
            if (erg != null)
            {
                SdkUtil.logd("ads full Topon FailBiddingADSource------erg.errorCode:" + erg.errorCode + "---erg.errorMessage:" + erg.errorMessage);
            }
            else
            {
                SdkUtil.logd("ads full Topon FailBiddingADSource------erg.errorCode:");
            }
        }
        #endregion

        #region REWARDED VIDEO AD EVENTS
        public void onAdGiftLoad(object sender, ATAdEventArgs erg)
        {
            if (erg != null)
            {
                SdkUtil.logd("ads gift Topon onAdGiftLoad :" + erg.placementId);
            }
            else
            {
                SdkUtil.logd("ads gift Topon onAdGiftLoad :");
            }
        }
        public void onAdGiftLoadFail(object sender, ATAdErrorEventArgs erg)
        {
            if (erg != null)
            {
                SdkUtil.logd("ads gift Topon onAdGiftLoadFail : " + erg.placementId + "--erg.errorCode:" + erg.errorCode + "--msg:" + erg.errorMessage);
            }
            else
            {
                SdkUtil.logd("ads gift Topon onAdGiftLoadFail : ");
            }
        }
        public void giftStartLoadingADSource(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads gift Topon StartLoadingADSource------");
        }
        public void giftFinishLoadingADSource(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads gift Topon FinishLoadingADSource------");
#if !USE_AUTO_GIFT
            GiftTryLoad = 0;
            isGiftLoading = false;
            isGiftLoaded = true;
            AdsProcessCB.Instance().Enqueue(() =>
            {
                if (cbGiftLoad != null)
                {
                    var tmpcb = cbGiftLoad;
                    cbGiftLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
            });
#endif
        }
        public void giftFailToLoadADSource(object sender, ATAdErrorEventArgs erg)
        {
            SdkUtil.logd("ads gift Topon FailToLoadADSource------erg.errorCode:" + erg.errorCode + "---erg.errorMessage:" + erg.errorMessage);
#if !USE_AUTO_GIFT
            isGiftLoading = false;
            isGiftLoaded = false;
            AdsProcessCB.Instance().Enqueue(() =>
            {
                GiftTryLoad++;
                tryloadGift();
            }, 1.0f);
#endif
        }
        public void onAdGiftVideoStartEvent(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads gift Topon onAdGiftVideoStartEvent");
            if (cbGiftShow != null)
            {
                AdCallBack tmpcb = cbGiftShow;
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    tmpcb(AD_State.AD_SHOW);
                });
            }
        }
        public void onAdGiftClick(object sender, ATAdEventArgs erg)
        {
            if (erg != null)
            {
                SdkUtil.logd("ads gift Topon onAdGiftClick :" + erg.placementId);
            }
            else
            {
                SdkUtil.logd("ads gift Topon onAdGiftClick :");
            }

        }
        public void onAdGiftVideoEndEvent(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads gift Topon onAdGiftVideoEndEvent------");
        }
        public void onAdGiftVideoPlayFail(object sender, ATAdErrorEventArgs erg)
        {
            SdkUtil.logd("ads gift Topon onAdGiftVideoPlayFail err=" + erg.errorMessage);
            isGiftLoaded = false;
            if (cbGiftShow != null)
            {
                advhelper.onCloseFullGift(false);
                AdCallBack tmpcb = cbGiftShow;
                AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_FAIL); });
                AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
            }
            onGiftClose();
        }
        public void onGiftReward(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads gift Topon onGiftReward");
            isRewardCom = true;
        }
        public void onAdGiftVideoClosedEvent(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads gift Topon onAdGiftVideoClosedEvent");
            isGiftLoaded = false;
            if (cbGiftShow != null)
            {
                advhelper.onCloseFullGift(false);
                AdCallBack tmpcb = cbGiftShow;
                if (isRewardCom)
                {
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_OK); });
                }
                else
                {
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_FAIL); });
                }
                AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });
            }
            onGiftClose();

            isRewardCom = false;
            GiftTryLoad = 0;
            cbGiftShow = null;
        }
        public void giftStartBiddingADSource(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads gift Topon StartBiddingADSource------");

        }
        public void giftFinishBiddingADSource(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads gift Topon FinishBiddingADSource------");

        }
        public void giftFailBiddingADSource(object sender, ATAdErrorEventArgs erg)
        {
            SdkUtil.logd("ads gift Topon FailBiddingADSource------erg.errorCode:" + erg.errorCode + "---erg.errorMessage:" + erg.errorMessage);

        }
        public void onRewardedVideoAdAgainPlayStart(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads gift Topon onRewardedVideoAdAgainPlayStart------");
        }
        public void onRewardedVideoAdAgainPlayFail(object sender, ATAdErrorEventArgs erg)
        {
            SdkUtil.logd("ads gift Topon onRewardedVideoAdAgainPlayFail : " + erg.placementId + "--erg.errorCode:" + erg.errorCode + "--msg:" + erg.errorMessage);
        }
        public void onRewardedVideoAdAgainPlayEnd(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads gift Topon onRewardedVideoAdAgainPlayEnd------");
        }
        public void onRewardedVideoAdAgainPlayClicked(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads gift Topon onRewardedVideoAdAgainPlayClicked------");
        }
        public void onAgainReward(object sender, ATAdEventArgs erg)
        {
            SdkUtil.logd("ads gift Topon onAgainReward------");
        }
        #endregion

#endif

    }
}