//#define ENABLE_ADS_IRON
//#define use_old_api
#define use_load_all

using System;
using UnityEngine;
using System.Collections;

#if ENABLE_ADS_IRON && use_ver_840
using com.unity3d.mediation;
#endif

namespace mygame.sdk
{
    partial class AdsIron
    {
#if use_ver_840

#if ENABLE_ADS_IRON
        LevelPlayBannerAd bannerAd;
        LevelPlayInterstitialAd interstitialAd;
#endif
        private static bool isInitAds = false;
        int posBnCurr = -1;
        private bool isAdsInited = false;
        private static bool isCallInit = false;
        private bool isAllowInit = false;
        private string plWaitFull = "aabc";
        private string plWaitGift = "aabc";

        public override void InitAds()
        {
#if ENABLE_ADS_IRON
            isEnable = true;
            if (!isInitAds)
            {
                isInitAds = true;
                isAdsInited = false;
                isCallInit = false;
                isAllowInit = true;
                Debug.Log($"mysdk: ads iron init ads");
                IronSource.Agent.setMetaData("do_not_sell", "false");

                int memage = PlayerPrefs.GetInt("mem_age_child", 14);
                if (memage >= 13)
                {
                    IronSource.Agent.setMetaData("is_child_directed", "false");
                }
                else if (memage < 13 && memage > 5)
                {
                    IronSource.Agent.setMetaData("is_child_directed", "true");
                    IronSource.Agent.setMetaData("is_deviceid_optout", "true");
                    IronSource.Agent.setConsent(false);
                }

                IronSource.Agent.shouldTrackNetworkState(true);
                int memss = PlayerPrefs.GetInt("mem_ss_consent_ir", -1);
                if (memss != -1)
                {
                    if (memss == 1)
                    {
                        IronSource.Agent.setConsent(true);
                    }
                    else
                    {
                        IronSource.Agent.setConsent(false);
                    }
                    checkInit();
                }
                else
                {
                    int showss = PlayerPrefs.GetInt("mem_show_CMP", 0);
                    if (showss == 1)
                    {
                        checkInit();
                    }
                    else
                    {
                        StartCoroutine(WaitInit());
                    }
                }

                //banner
                dicPLBanner.Clear();
                AdPlacementBanner plbn = new AdPlacementBanner();
                dicPLBanner.Add(PLBnDefault, plbn);
                plbn.placement = PLBnDefault;
                plbn.adECPM.idxHighPriority = -1;
                plbn.adECPM.listFromDstring(bannerId);
                //full
                dicPLFull.Clear();
                AdPlacementFull plfull = new AdPlacementFull();
                dicPLFull.Add(PLFullDefault, plfull);
                plfull.placement = PLFullDefault;
                plfull.adECPM.idxHighPriority = -1;
                plfull.adECPM.listFromDstring(fullId);
                //gift
                dicPLGift.Clear();
                AdPlacementFull plgift = new AdPlacementFull();
                dicPLGift.Add(PLGiftDefault, plgift);
                plgift.placement = PLGiftDefault;
                plgift.adECPM.idxHighPriority = -1;
                plgift.adECPM.listFromDstring(giftId);
            }
#endif
        }

        IEnumerator WaitInit()
        {
            yield return new WaitForSeconds(30);
            checkInit();
        }

        private void checkInit()
        {
            Debug.Log($"mysdk: ads iron checkInit isCallInit=" + isCallInit);
            if (!isCallInit)
            {
                isCallInit = true;
#if ENABLE_ADS_IRON
                Debug.Log($"mysdk: ads iron init ironsdk");
                addEvent();
#if use_old_api
                IronSource.Agent.init(appId);
#else
                IronSource.Agent.validateIntegration();
                LevelPlayAdFormat[] legacyAdFormats = new[] { LevelPlayAdFormat.REWARDED };
                LevelPlay.Init(appId, SystemInfo.deviceUniqueIdentifier, legacyAdFormats);
#endif

#endif
            }
        }

        public override void AdsAwake()
        {
#if ENABLE_ADS_IRON
            GameAdsHelperBridge.CBRequestGDPR += onShowCmp;
            isEnable = true;
#endif
        }

        public void onShowCmp(int state, string des)
        {
#if ENABLE_ADS_IRON
            if (state == 0)
            {
                Debug.Log($"mysdk: ads iron onshow cmp");
                if (des != null && des.CompareTo("0") == 0)
                {
                    if (isAllowInit)
                    {
                        checkInit();
                    }
                    else
                    {
                        StartCoroutine(Wait4AllowInit());
                    }
                }
            }
            else if (state == 1)
            {
                if (des != null && des.Length > 5)
                {
                    PlayerPrefs.SetInt("mem_ss_consent_ir", 1);
                    IronSource.Agent.setConsent(true);
                }
                else
                {
                    PlayerPrefs.SetInt("mem_ss_consent_ir", 0);
                    IronSource.Agent.setConsent(false);
                }
                if (isAllowInit)
                {
                    checkInit();
                }
                else
                {
                    StartCoroutine(Wait4AllowInit());
                }
            }
#endif
        }

        IEnumerator Wait4AllowInit()
        {
            yield return new WaitForSeconds(0.5f);
            checkInit();
        }

        public override string getname()
        {
            return "iron";
        }

        private void Start()
        {

        }

        private void addEvent()
        {
#if ENABLE_ADS_IRON

            LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
            LevelPlay.OnInitFailed += (error => Debug.Log("Initialization error: " + error));

            IronSourceEvents.onImpressionDataReadyEvent += irOnImpressionDataReadyEvent;

            IronSourceRewardedVideoEvents.onAdAvailableEvent += ReWardedVideoOnAdAvailableEvent;
            IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
            IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoAdOpenedEvent;
            IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoAdClosedEvent;
            IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;
            IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoAdRewardedEvent;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoAdShowFailedEvent;

#endif
        }

#if ENABLE_ADS_IRON
        private void SdkInitializationCompletedEvent(LevelPlayConfiguration configuration)
        {
            Debug.Log($"mysdk: ads iron SdkInitializationCompletedEvent");
            isAdsInited = true;
            AdsProcessCB.Instance().Enqueue(() =>
            {
                if ("aabc".CompareTo(plWaitFull) != 0)
                {
                    loadFull(plWaitFull, null);
                }
                if ("aabc".CompareTo(plWaitGift) != 0)
                {
                    loadGift(plWaitGift, null);
                }
                advhelper.onAdsInitSuc();
            }, 0.1f);
        }
#endif

        void OnApplicationPause(bool isPaused)
        {
#if ENABLE_ADS_IRON
            IronSource.Agent.onApplicationPause(isPaused);
#endif
        }

        protected override void tryLoadBanner(AdPlacementBanner adpl)
        {
#if ENABLE_ADS_IRON
            if (adpl != null)
            {
                if (adpl.adECPM.idxCurrEcpm >= adpl.adECPM.list.Count)
                {
                    adpl.adECPM.idxCurrEcpm = 0;
                }
                adpl.isLoading = true;
                adpl.isloaded = false;
                adpl.isRealShow = false;

                string idload = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
                SdkUtil.logd($"ads bn {adpl.getPlacement} iron bn tryLoadBanner id={idload}");
                com.unity3d.mediation.LevelPlayBannerPosition pos;
                if (adpl.posBanner == 0)
                {
                    pos = com.unity3d.mediation.LevelPlayBannerPosition.TopCenter;
                }
                else
                {
                    pos = com.unity3d.mediation.LevelPlayBannerPosition.BottomCenter;
                }
                com.unity3d.mediation.LevelPlayAdSize bns = com.unity3d.mediation.LevelPlayAdSize.BANNER;
                if (bnWidth <= -2)
                {
                    bns = com.unity3d.mediation.LevelPlayAdSize.CreateAdaptiveAdSize();
                }
                bannerAd = new LevelPlayBannerAd(idload, bns, pos, adpl.getPlacement, false, false);
                bannerAd.OnAdLoaded += BannerOnAdLoadedEvent;
                bannerAd.OnAdLoadFailed += BannerOnAdLoadFailedEvent;
                bannerAd.OnAdDisplayed += BannerOnAdDisplayedEvent;
                bannerAd.OnAdDisplayFailed += BannerOnAdDisplayFailedEvent;
                bannerAd.OnAdClicked += BannerOnAdClickedEvent;
                bannerAd.OnAdCollapsed += BannerOnAdCollapsedEvent;
                bannerAd.OnAdLeftApplication += BannerOnAdLeftApplicationEvent;
                bannerAd.OnAdExpanded += BannerOnAdExpandedEvent;
                bannerAd.LoadAd();
                AdsHelper.onAdLoad(adpl.getPlacement, "banner", idload, "iron");
            }
            else
            {
                SdkUtil.logd($"ads bn {adpl.getPlacement} iron bn tryLoadBanner not pl");
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads bn {adpl.getPlacement} iron bn tryLoadBanner not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }

        IEnumerator waitLoadBannerWhenDestroy(string placement, AdCallBack cb)
        {
            AdPlacementBanner adpl = getPlBanner(placement, 0);
            if (adpl != null)
            {
                adpl.cbLoad = cb;
                if (!adpl.isLoading)
                {
                    adpl.isLoading = true;
                    yield return new WaitForSeconds(0.25f);
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadBanner(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads bn {adpl.getPlacement} iron bn waitLoadBannerWhenDestroy isloading");
                }
            }
            else
            {
                yield return null;
            }
        }
        public override void loadBanner(string placement, AdCallBack cb)
        {
            if (!isAdsInited)
            {
                SdkUtil.logd($"ads bn {placement} iron loadBanner not init");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            AdPlacementBanner adpl = getPlBanner(placement, 0);
            if (adpl != null)
            {
                SdkUtil.logd($"ads bn {adpl.getPlacement} iron bn loadBanner isloading={adpl.isLoading}");
                adpl.cbLoad = cb;
                if (!adpl.isLoading && !adpl.isloaded)
                {
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadBanner(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads bn {adpl.getPlacement} iron bn loadBanner isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                    if (adpl.isloaded)
                    {
                        if (cb != null)
                        {
                            cb(AD_State.AD_LOAD_OK);
                        }
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads bn {adpl.getPlacement} iron bn loadBanner not pl");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
            }
        }
        public override bool showBanner(string placement, int pos, int width, int maxH, AdCallBack cb, float dxCenter, bool highP = false)
        {
            if (!isAdsInited)
            {
                SdkUtil.logd($"ads bn {placement} iron showBanner not init");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return false;
            }
            AdPlacementBanner adpl = getPlBanner(placement, 0);
#if ENABLE_ADS_IRON
            if (adpl != null)
            {
                SdkUtil.logd($"ads bn {adpl.getPlacement} iron bn showBanner isloading={adpl.isLoading}");
                bool isDestroyBanner = false;
                if (posBnCurr != -1 && posBnCurr != pos)
                {
                    isDestroyBanner = true;
                    destroyBanner();
                }
                adpl.isShow = true;
                adpl.posBanner = pos;
                adpl.setSetPlacementShow(placement);
                bnWidth = width;
                if (adpl.isloaded && bannerAd != null)
                {
                    if (!adpl.isRealShow)
                    {
                        adpl.isRealShow = true;
                        bannerAd.ShowAd();
                    }
                    advhelper.hideOtherBanner(adsType);
                    return true;
                }
                else
                {
                    SdkUtil.logd($"ads bn {adpl.getPlacement} iron bn showBanner not show and load isloading={adpl.isLoading}");
                    posBnCurr = pos;
                    if (isDestroyBanner)
                    {
                        StartCoroutine(waitLoadBannerWhenDestroy(placement, cb));
                    }
                    else
                    {
                        loadBanner(placement, cb);
                    }
                    return false;
                }
            }
            else
            {
                SdkUtil.logd($"ads bn {adpl.getPlacement} iron bn tryLoadBanner not pl");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return false;
            }
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads bn {placement} iron bn tryLoadBanner not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
            return false;
#endif
        }
        public override void hideBanner()
        {
#if ENABLE_ADS_IRON
            SdkUtil.logd($"ads bn iron bn hideBanner");
            foreach (var adi in dicPLBanner)
            {
                adi.Value.isShow = false;
                adi.Value.isRealShow = false;
            }
            if (bannerAd != null)
            {
                bannerAd.HideAd();
            }
#endif
        }
        public override void destroyBanner()
        {
#if ENABLE_ADS_IRON
            SdkUtil.logd($"ads bn iron bn destroyBanner");
            foreach (var adi in dicPLBanner)
            {
                adi.Value.isLoading = false;
                adi.Value.isloaded = false;
                adi.Value.isShow = false;
                adi.Value.isRealShow = false;
            }
            posBnCurr = -1;
            if (bannerAd != null)
            {
                bannerAd.DestroyAd();
                bannerAd = null;
            }
#endif
        }
        //Native

        //
        public override void clearCurrFull(string placement)
        {
#if ENABLE_ADS_IRON
            if (getFullLoaded(placement) == 1)
            {
                AdPlacementFull adpl = getPlFull(placement, true);
                if (adpl != null)
                {
                    adpl.isloaded = false;
                }
            }
#endif
        }
        public override int getFullLoaded(string placement)
        {
#if ENABLE_ADS_IRON
            AdPlacementFull adpl = getPlFull(placement, true);
            if (adpl == null)
            {
                SdkUtil.logd($"ads full {adpl.getPlacement} iron getFullLoaded not pl");
                return 0;
            }
            else
            {
#if use_load_all
                for (int i = 0; i < adpl.adECPM.list.Count; i++)
                {
                    if (adpl.adECPM.list[i].isLoaded)
                    {
                        fullAdNetwork = adpl.adECPM.list[i].adnetname;
                        return 1;
                    }
                }
#else
                if (adpl.isloaded && interstitialAd != null)
                {
                    return 1;
                }
#endif
            }
#endif
            return 0;
        }
        protected override void tryLoadFull(AdPlacementFull adpl)
        {
#if ENABLE_ADS_IRON

#if use_load_all
            adpl.isLoading = true;
            adpl.isloaded = false;
            adpl.countLoad = 0;
            for (int i = 0; i < adpl.adECPM.list.Count; i++)
            {
                if (!adpl.adECPM.list[i].isLoading && !adpl.adECPM.list[i].isLoaded)
                {
                    string idload = adpl.adECPM.list[i].adsId;
                    SdkUtil.logd($"ads full {adpl.getPlacement}-{adpl.placement} iron tryLoadFull load all id={idload}");
                    adpl.adECPM.list[i].isLoading = true;
                    adpl.countLoad++;
                    interstitialAd = new LevelPlayInterstitialAd(idload);
                    adpl.adECPM.list[i].adObject = interstitialAd;

                    interstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
                    interstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
                    interstitialAd.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
                    interstitialAd.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
                    interstitialAd.OnAdClicked += InterstitialOnAdClickedEvent;
                    interstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;
                    interstitialAd.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;

                    //interstitialAd.OnAdLoaded += (adInfo) => { InterstitialOnAdLoadedEvent(idload, adInfo); };
                    //interstitialAd.OnAdLoadFailed += (error) => { InterstitialOnAdLoadFailedEvent(idload, error); };
                    //interstitialAd.OnAdDisplayed += (adInfo) => { InterstitialOnAdDisplayedEvent(idload, adInfo); };
                    //interstitialAd.OnAdDisplayFailed += (infoError) => { InterstitialOnAdDisplayFailedEvent(idload, infoError); };
                    //interstitialAd.OnAdClicked += (adInfo) => { InterstitialOnAdClickedEvent(idload, adInfo); };
                    //interstitialAd.OnAdClosed += (adInfo) => { InterstitialOnAdClosedEvent(idload, adInfo); };
                    //interstitialAd.OnAdInfoChanged += (adInfo) => { InterstitialOnAdInfoChangedEvent(idload, adInfo); };

                    interstitialAd.LoadAd();
                    AdsHelper.onAdLoad(adpl.getPlacement, "interstitial", idload, "iron");
                }
                else
                {
                    if (adpl.adECPM.list[i].isLoading)
                    {
                        adpl.countLoad++;
                    }
                    if (adpl.adECPM.list[i].isLoaded)
                    {
                        adpl.isloaded = true;
                    }
                    SdkUtil.logd($"ads full {adpl.getPlacement}-{adpl.placement} iron tryLoadFull id={adpl.adECPM.list[i].adsId} loading={adpl.adECPM.list[i].isLoading} loaded={adpl.adECPM.list[i].isLoaded}");
                }
            }
            if (adpl.countLoad == 0)
            {
                adpl.isLoading = false;
            }
#else
            if (adpl.adECPM.idxCurrEcpm >= adpl.adECPM.list.Count)
            {
                adpl.adECPM.idxCurrEcpm = 0;
            }
            adpl.isLoading = true;
            adpl.isloaded = false;
            string idload = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
            SdkUtil.logd($"ads full {adpl.getPlacement} iron tryLoadFull id={idload}");
            interstitialAd = new LevelPlayInterstitialAd(idload);
            interstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
            interstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
            interstitialAd.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
            interstitialAd.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
            interstitialAd.OnAdClicked += InterstitialOnAdClickedEvent;
            interstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;
            interstitialAd.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;
            interstitialAd.LoadAd();
            AdsHelper.onAdLoad(adpl.getPlacement, "interstitial", idload, "iron");
#endif

#else
            if (adpl != null && adpl.cbLoad != null)
            {
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadFull(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_IRON
            if (!isAdsInited)
            {
                SdkUtil.logd($"ads full {placement} iron loadFull not init");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            AdPlacementFull adpl = getPlFull(placement, false);
            if (adpl == null)
            {
                SdkUtil.logd($"ads full {placement} iron loadFull not placement");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            else
            {
#if use_load_all
                if (true) //(!adpl.isLoading)
#else
                if (!adpl.isloaded && !adpl.isLoading)
#endif
                {
                    SdkUtil.logd($"ads full {adpl.getPlacement}-{adpl.placement} iron loadFull type=" + adsType);
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.cbLoad = cb;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadFull(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads full {adpl.getPlacement}-{adpl.placement} iron loadFull isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                }
            }
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
#if ENABLE_ADS_IRON
            if (!isAdsInited)
            {
                SdkUtil.logd($"ads full {placement} iron showFull not init");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return false;
            }
            AdPlacementFull adpl = getPlFull(placement, true);
            if (adpl != null)
            {
                adpl.cbShow = null;
                int ss = getFullLoaded(adpl.placement);
                if (ss > 0)
                {
                    adpl.countLoad = 0;
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    string idShow = "";
                    string netShow = "";
#if use_load_all
                    for (int i = 0; i < adpl.adECPM.list.Count; i++)
                    {
                        if (adpl.adECPM.list[i].isLoaded)
                        {
                            idShow = adpl.adECPM.list[i].adsId;
                            netShow = adpl.adECPM.list[i].adnetname;
                            fullAdNetwork = adpl.adECPM.list[i].adnetname;
                            interstitialAd = (LevelPlayInterstitialAd)adpl.adECPM.list[i].adObject;
                            break;
                        }
                    }
#else
                    idShow = fullIdLoaded;
                    netShow = fullAdNetwork;
#endif
                    if (timeDelay > 0)
                    {
                        SdkUtil.logd($"ads full {adpl.getPlacement}-{adpl.placement} iron showFull show net={netShow} timeDelay={timeDelay}");
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            interstitialAd.ShowAd(placement);
                        }, timeDelay);
                        return true;
                    }
                    else
                    {
                        SdkUtil.logd($"ads full {adpl.getPlacement}-{adpl.placement} iron showFull show net={netShow}");
                        interstitialAd.ShowAd(placement);
                        return true;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads full {adpl.getPlacement}-{adpl.placement} iron showFull show not loaded");
                }
            }
            else
            {
                SdkUtil.logd($"ads full {placement} iron showFull not pl");
            }
#endif
            return false;
        }

        //------------------------------------------------
        public override void clearCurrGift(string placement)
        {
#if ENABLE_ADS_IRON
            if (getGiftLoaded(placement) == 1)
            {
                AdPlacementFull adpl = getPlGift(placement);
                if (adpl != null)
                {
                    adpl.isloaded = false;
                }
            }
#endif
        }
        public override int getGiftLoaded(string placement)
        {
#if ENABLE_ADS_IRON
            AdPlacementFull adpl = getPlGift(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads gift {adpl.getPlacement} iron getGiftLoaded not pl");
                return 0;
            }
            else
            {
                if (adpl.isloaded && IronSource.Agent.isRewardedVideoAvailable())
                {
                    return 1;
                }
            }

#endif
            return 0;
        }
        protected override void tryloadGift(AdPlacementFull adpl)
        {
#if ENABLE_ADS_IRON
            if (adpl.adECPM.idxCurrEcpm >= adpl.adECPM.list.Count)
            {
                adpl.adECPM.idxCurrEcpm = 0;
            }
            adpl.isLoading = true;
            adpl.isloaded = false;
            SdkUtil.logd($"ads gift {adpl.getPlacement}-{adpl.placement} iron tryloadGift");
            adpl.isLoading = true;
            //IronSource.Agent.loadRewardedVideo();
            StartCoroutine(waitGiftReady(adpl));
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        IEnumerator waitGiftReady(AdPlacementFull adpl)
        {
#if ENABLE_ADS_IRON
            if (adpl != null)
            {
                int count = 0;
                while (!adpl.isloaded && adpl.isLoading && count < 40)
                {
                    yield return new WaitForSeconds(0.5f);
                    count++;
                }
                adpl.isLoading = false;
                if (adpl.cbLoad != null)
                {
                    if (adpl.isloaded)
                    {
                        SdkUtil.logd($"ads gift {adpl.getPlacement}-{adpl.placement} iron waitGiftReady ok");
                        var tmpcb = adpl.cbLoad;
                        adpl.cbLoad = null;
                        tmpcb(AD_State.AD_LOAD_OK);
                    }
                    else
                    {
                        SdkUtil.logd($"ads gift {adpl.getPlacement}-{adpl.placement} iron waitGiftReady fail");
                        var tmpcb = adpl.cbLoad;
                        adpl.cbLoad = null;
                        tmpcb(AD_State.AD_LOAD_FAIL);
                    }
                }
                else
                {
                    SdkUtil.logd($"ads gift {adpl.getPlacement}-{adpl.placement} iron waitGiftReady cb null");
                }
            }
            else
            {
                yield return null;
            }
#else
            yield return null;
#endif
        }
        public override void loadGift(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_IRON
            if (!isAdsInited)
            {
                SdkUtil.logd($"ads full {placement} iron loadFull not init");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            AdPlacementFull adpl = getPlGift(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads gift {adpl.getPlacement} iron loadGift not placement");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            else
            {
                if (!adpl.isloaded && !adpl.isLoading)
                {
                    SdkUtil.logd($"ads gift {adpl.getPlacement}-{adpl.placement} iron loadGift type=" + adsType);
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.cbLoad = cb;
                    adpl.setSetPlacementLoad(placement);
                    tryloadGift(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads gift {adpl.getPlacement}-{adpl.placement} iron loadGift isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                }
            }
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showGift(string placement, float timeDelay, AdCallBack cb)
        {
#if ENABLE_ADS_IRON
            AdPlacementFull adpl = getPlGift(placement);
            if (adpl != null)
            {
                if (getGiftLoaded(placement) > 0)
                {
                    adpl.cbShow = cb;
                    adpl.isloaded = false;
                    adpl.isAddCondition = true;
                    adpl.setSetPlacementShow(placement);
                    string idShow = giftId;
                    string netShow = giftAdNetwork;
                    if (timeDelay > 0)
                    {
                        SdkUtil.logd($"ads gift {adpl.getPlacement}-{adpl.placement} iron showGift show net={netShow} timeDelay={timeDelay}");
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            adpl.isloaded = false;
                            IronSource.Agent.showRewardedVideo(placement);
                        }, timeDelay);
                        return true;
                    }
                    else
                    {
                        SdkUtil.logd($"ads gift {adpl.getPlacement}-{adpl.placement} iron showGift show net={netShow}");
                        adpl.isloaded = false;
                        IronSource.Agent.showRewardedVideo(placement);
                        return true;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads gift {placement}-{adpl.placement} iron showGift not load");
                }
            }
            else
            {

                SdkUtil.logd($"ads gift {placement} iron showGift not pl");
            }
#endif
            return false;
        }
        //-------------------------------------------------------------------------------
#if ENABLE_ADS_IRON
        private void irOnImpressionDataReadyEvent(IronSourceImpressionData impressionData)
        {
            if (impressionData != null)
            {
                string adFormat = "";
#if use_old_api
                adFormat = impressionData.adUnit;
                SdkUtil.logd($"ads iron imp adFormat={adFormat} va={impressionData.revenue} pl={impressionData.placement} net={impressionData.adNetwork}");
                FIRhelper.logEventAdsPaidIron(appId, impressionData.adNetwork, adFormat, impressionData.instanceName, (double)impressionData.revenue, impressionData.country, impressionData.placement);
#else
                adFormat = impressionData.adFormat;
                SdkUtil.logd($"ads iron imp adFormat={adFormat} va={impressionData.revenue} pl={impressionData.placement} net={impressionData.adNetwork} mid={impressionData.mediationAdUnitId} mnane={impressionData.mediationAdUnitName}");
                FIRhelper.logEventAdsPaidIron(impressionData.mediationAdUnitId, impressionData.adNetwork, adFormat, impressionData.instanceName, (double)impressionData.revenue, impressionData.country, impressionData.placement);
#endif
                if (adFormat.Contains("banner"))
                {
                    SdkUtil.logd($"ads bn iron onpaid adNetwork={adFormat}-{impressionData.mediationAdUnitId}-{impressionData.revenue}");
                    FIRhelper.logEvent("show_ads_bn");
                    FIRhelper.logEvent("show_ads_bn_nm_3");
                    if (impressionData.revenue != null && impressionData.revenue.HasValue)
                    {
                        float realValue = (float)impressionData.revenue.Value;
                        AdsHelper.onAdImpresstion(SDKManager.Instance.currPlacement, impressionData.mediationAdUnitId, "banner", "iron", impressionData.adNetwork, realValue);
                    }
                }
                else if (adFormat.Contains("interstitial"))
                {
                    if (!isFull2)
                    {
                        SdkUtil.logd($"ads full iron onpaid not isFull2 adNetwork={adFormat}-{impressionData.mediationAdUnitId}-{impressionData.revenue}");
                        FIRhelper.logEvent("show_ads_total_imp");
                        FIRhelper.logEvent("show_ads_full_imp");
                        FIRhelper.logEvent("show_ads_full_imp_3");
                    }
                    else
                    {
                        SdkUtil.logd($"ads full iron onpaid isFull2 adNetwork={adFormat}-{impressionData.mediationAdUnitId}-{impressionData.revenue}");
                    }
                    if (dicPLFull.ContainsKey(PLFullDefault))
                    {
                        if (impressionData.revenue != null && impressionData.revenue.HasValue)
                        {
                            AdPlacementFull adpl = dicPLFull[PLFullDefault];
                            float realValue = (float)impressionData.revenue.Value;
                            AdsHelper.onAdImpresstion(adpl.getPlacement, impressionData.mediationAdUnitId, "interstitial", "iron", impressionData.adNetwork, realValue);
                        }
                    }
                }
                else
                {
                    SdkUtil.logd($"ads gift iron onpaid adNetwork={adFormat}-{impressionData.mediationAdUnitId}-{impressionData.revenue}");
                    FIRhelper.logEvent("show_ads_total_imp");
                    FIRhelper.logEvent("show_ads_reward_imp");
                    FIRhelper.logEvent("show_ads_reward_imp_3");
                    if (dicPLGift.ContainsKey(PLGiftDefault))
                    {
                        if (impressionData.revenue != null && impressionData.revenue.HasValue)
                        {
                            AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                            float realValue = (float)impressionData.revenue.Value;
                            AdsHelper.onAdImpresstion(adpl.getPlacement, impressionData.mediationAdUnitId, "rewarded", "iron", impressionData.adNetwork, realValue);
                        }
                    }
                }
            }
        }

        #region BANNER AD EVENTS
        void BannerOnAdLoadedEvent(LevelPlayAdInfo adInfo)
        {
            LevelPlayAdSize size = adInfo.adSize;
            int width = size.Width;
            int height = size.Height;
            if (dicPLBanner.ContainsKey(PLBnDefault))
            {
                AdPlacementBanner adpl = dicPLBanner[PLBnDefault];
                SdkUtil.logd($"ads bn {adpl.getPlacement} iron bn BannerOnAdLoadedEvent adNetwork={adInfo.adNetwork}-{adInfo.adFormat}-{adInfo.adUnitId}");
                AdsHelper.onAdLoadResult(adpl.getPlacement, "banner", adInfo.adUnitId, "iron", adInfo.adNetwork, true);
                adpl.isloaded = true;
                adpl.isLoading = false;
                adpl.countLoad = 0;
                if (adpl.isShow)
                {
                    SdkUtil.logd($"ads bn {adpl.getPlacement} iron bn BannerAdLoadedEvent show");
                    if (!adpl.isRealShow && advhelper.isShowBanner)
                    {
                        adpl.isRealShow = true;
                        bannerAd.ShowAd();
                    }
                    if (advhelper.bnCurrShow == adsType)
                    {
                        SdkUtil.logd($"ads bn {adpl.getPlacement} iron bn BannerAdLoadedEvent hide other");
                        advhelper.hideOtherBanner(adsType);
                    }
                }
                else
                {
                    SdkUtil.logd($"ads bn {adpl.getPlacement} iron bn BannerAdLoadedEvent hide");
                    adpl.isRealShow = false;
                    bannerAd.HideAd();
                }

                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
                if (advhelper != null)
                {
                    advhelper.onBannerLoadOk(adsType);
                }
            }
            else
            {
                SdkUtil.logd($"ads bn iron bn BannerAdLoadedEvent not pl adNetwork={adInfo.adNetwork}-{adInfo.adFormat}-{adInfo.adUnitId}");
            }
        }
        void BannerOnAdLoadFailedEvent(LevelPlayAdError ironSourceError)
        {
            if (dicPLBanner.ContainsKey(PLBnDefault))
            {
                AdPlacementBanner adpl = dicPLBanner[PLBnDefault];
                SdkUtil.logd($"ads bn {adpl.getPlacement} iron bn BannerOnAdLoadFailedEvent AdUnitId={ironSourceError.AdUnitId}-{ironSourceError.ErrorCode}-{ironSourceError.ErrorMessage}");
                AdsHelper.onAdLoadResult(adpl.getPlacement, "banner", ironSourceError.AdUnitId, "iron", "", false);
                if (adpl.isLoading)
                {
                    SdkUtil.logd($"ads bn {adpl.getPlacement} iron bn BannerAdLoadFailedEvent isloading");
                    adpl.adECPM.idxCurrEcpm++;
                    if (adpl.adECPM.idxCurrEcpm < adpl.adECPM.list.Count)
                    {
                        tryLoadBanner(adpl);
                    }
                    else
                    {
                        adpl.isLoading = false;
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            if (adpl.cbLoad != null)
                            {
                                var tmpcb = adpl.cbLoad;
                                adpl.cbLoad = null;
                                tmpcb(AD_State.AD_LOAD_FAIL);
                            }
                            if (advhelper != null)
                            {
                                advhelper.onBannerLoadFail(adsType);
                            }
                        });
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads bn iron bn BannerAdLoadFailedEvent not pl");
            }
        }
        void BannerOnAdClickedEvent(LevelPlayAdInfo adInfo)
        {
            SdkUtil.logd($"ads bn iron bn BannerAdClickedEvent={adInfo.adUnitId} net={adInfo.adNetwork}");
            SDKManager.Instance.onClickAd();
            AdsHelper.onAdClick(SDKManager.Instance.currPlacement, "banner", "iron", adInfo.adUnitId);
        }
        void BannerOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
        {
            SdkUtil.logd($"ads bn iron bn BannerOnAdDisplayedEvent={adInfo.adUnitId} net={adInfo.adNetwork}");
            if (advhelper.bnCurrShow == adsType)
            {
                SdkUtil.logd($"ads bn iron bn BannerOnAdDisplayedEvent hide other");
                advhelper.hideOtherBanner(adsType);
            }
        }
        void BannerOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError adInfoError)
        {
            SdkUtil.logd($"ads bn iron bn BannerOnAdDisplayFailedEvent");
        }
        void BannerOnAdCollapsedEvent(LevelPlayAdInfo adInfo)
        {
            SdkUtil.logd($"ads bn iron bn BannerOnAdCollapsedEvent={adInfo.adUnitId}");
        }
        void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo adInfo)
        {
            SdkUtil.logd($"ads bn iron bn BannerOnAdLeftApplicationEvent={adInfo.adUnitId}");
        }
        void BannerOnAdExpandedEvent(LevelPlayAdInfo adInfo)
        {
            SdkUtil.logd($"ads bn iron bn BannerOnAdExpandedEvent={adInfo.adUnitId}");
        }
        #endregion

        #region INTERSTITIAL AD EVENTS
        void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo)
        {
            string AdUnitId = adInfo.adUnitId;
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                SdkUtil.logd($"ads full {adpl.getPlacement} iron HandleInterstitialAdDidLoad adNetwork={adInfo.adNetwork}-{adInfo.adFormat}-{AdUnitId}-{adInfo.revenue}");
                AdsHelper.onAdLoadResult(adpl.getPlacement, "interstitial", AdUnitId, "iron", adInfo.adNetwork, true);
                fullAdNetwork = adInfo.adNetwork;
#if use_load_all
                adpl.countLoad--;
                adpl.isloaded = true;
                adpl.setStateAd4Id(AdUnitId, false, true, fullAdNetwork, adInfo.revenue);
                if (adpl.isLoading)
                {
                    adpl.isLoading = false;
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        if (adpl.cbLoad != null)
                        {
                            var tmpcb = adpl.cbLoad;
                            adpl.cbLoad = null;

                            SdkUtil.logd($"ads full {adpl.getPlacement} iron HandleInterstitialAdDidLoad=" + AdUnitId + " -> cb ok");
                            tmpcb(AD_State.AD_LOAD_OK);
                        }
                    });
                }
#else
                adpl.countLoad = 0;
                adpl.isLoading = false;
                adpl.isloaded = true;
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
#endif
            }
            else
            {
                SdkUtil.logd($"ads full iron HandleInterstitialAdDidLoad not pl");
            }
        }
        void InterstitialOnAdLoadFailedEvent(LevelPlayAdError error)
        {
            string AdUnitId = error.AdUnitId;
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                SdkUtil.logd($"ads full {adpl.getPlacement} iron InterstitialAdShowFailedEvent err={AdUnitId}-{error}");
                AdsHelper.onAdLoadResult(adpl.getPlacement, "interstitial", AdUnitId, "iron", "", false);
#if use_load_all
                adpl.countLoad--;
                adpl.setStateAd4Id(AdUnitId, false, false, "", null);
                adpl.setObjectAd4Id(AdUnitId, null);
                if (adpl.isLoading)
                {
                    adpl.isLoading = false;
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        if (adpl.cbLoad != null)
                        {
                            var tmpcb = adpl.cbLoad;
                            adpl.cbLoad = null;

                            SdkUtil.logd($"ads full {adpl.getPlacement} iron InterstitialAdShowFailedEvent {AdUnitId} -> {adpl.isloaded}");
                            if (adpl.isloaded)
                            {
                                tmpcb(AD_State.AD_LOAD_OK);
                            }
                            else
                            {
                                tmpcb(AD_State.AD_LOAD_FAIL);
                            }
                        }
                    });
                }
#else
                adpl.isLoading = false;
                adpl.isloaded = false;
                adpl.adECPM.idxCurrEcpm++;
                if (adpl.adECPM.idxCurrEcpm < adpl.adECPM.list.Count)
                {
                    tryLoadFull(adpl);
                }
                else
                {
                    if (adpl.cbLoad != null)
                    {
                        var tmpcb = adpl.cbLoad;
                        adpl.cbLoad = null;
                        tmpcb(AD_State.AD_LOAD_FAIL);
                    }
                }
#endif
            }
            else
            {
                SdkUtil.logd($"ads full iron HandleInterstitialAdDidFailWithError not pl");
            }
        }
        void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
        {
            string AdUnitId = adInfo.adUnitId;
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                SdkUtil.logd($"ads full {adpl.getPlacement} iron InterstitialOnAdDisplayedEvent={AdUnitId}-{adInfo.adNetwork}");
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads full iron InterstitialOnAdDisplayedEvent not pl={AdUnitId}-{adInfo.adNetwork}");
            }
        }
        void InterstitialOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError infoError)
        {
            string AdUnitId = infoError.LevelPlayError.AdUnitId;
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                SdkUtil.logd($"ads full {adpl.getPlacement} iron InterstitialAdShowFailedEvent err=" + infoError.ToString());
#if !use_load_all
                adpl.isloaded = false;
                adpl.isLoading = false;
#endif
                adpl.setStateAd4Id(AdUnitId, false, false, "", null);
                advhelper.onCloseFullGift(true);
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                }
            }
            else
            {
                SdkUtil.logd($"ads full iron InterstitialAdShowFailedEvent not pl err=" + infoError.ToString());
            }
            onFullClose(PLFullDefault);
        }
        void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo)
        {
            string AdUnitId = adInfo.adUnitId;
            SdkUtil.logd($"ads full iron InterstitialOnAdClickedEvent={AdUnitId}-{adInfo.adNetwork}");
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                AdsHelper.onAdClick(adpl.getPlacement, "interstitial", "iron", AdUnitId);
            }
            else
            {
                AdsHelper.onAdClick(SDKManager.Instance.currPlacement, "interstitial", "iron", AdUnitId);
            }
        }
        void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo)
        {
            string AdUnitId = adInfo.adUnitId;
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                SdkUtil.logd($"ads full {adpl.getPlacement} iron InterstitialOnAdClosedEvent={AdUnitId}-{adInfo.adNetwork}");
#if !use_load_all
                adpl.isloaded = false;
                adpl.isLoading = false;
#endif
                adpl.setStateAd4Id(AdUnitId, false, false, "", null);
                advhelper.onCloseFullGift(true);
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });
                }
                adpl.countLoad = 0;
            }
            else
            {
                SdkUtil.logd($"ads full iron InterstitialOnAdClosedEvent not pl={AdUnitId}-{adInfo.adNetwork}");
            }
            onFullClose(PLFullDefault);
        }
        void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
        {
            string AdUnitId = adInfo.adUnitId;
            SdkUtil.logd($"ads full iron InterstitialOnAdInfoChangedEvent={AdUnitId}-{adInfo.adNetwork}");
        }
        #endregion

        #region REWARDED VIDEO AD EVENTS
        private void ReWardedVideoOnAdAvailableEvent(IronSourceAdInfo adInfo)
        {
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                SdkUtil.logd($"ads gift {adpl.getPlacement}-{adpl.placement} iron OnAdLoaded adNetwork={adInfo.adNetwork}");
                AdsHelper.onAdLoadResult(adpl.getPlacement, "rewarded", appId, "iron", adInfo.adNetwork, true);
                adpl.countLoad = 0;
                adpl.isLoading = false;
                adpl.isloaded = true;
                giftAdNetwork = adInfo.adNetwork;
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
            }
            else
            {
                SdkUtil.logd($"ads gift iron OnAdLoaded not pl adNetwork={adInfo.adNetwork}");
            }
        }
        private void RewardedVideoOnAdUnavailable()
        {
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                AdsHelper.onAdLoadResult(adpl.getPlacement, "rewarded", appId, "iron", "", false);
                if (adpl.isLoading)
                {
                    SdkUtil.logd($"ads gift {adpl.getPlacement}-{adpl.placement} iron OnAdLoadFailed loading");
                    adpl.isLoading = false;
                    adpl.isloaded = false;
                }
                else
                {
                    SdkUtil.logd($"ads gift {adpl.getPlacement} iron OnAdLoadFailed not loading");
                }
            }
            else
            {
                SdkUtil.logd($"ads gift iron OnAdLoadFailed not pl");
            }
        }
        private void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
        {
            SdkUtil.logd($"ads gift iron RewardedVideoOnAdClickedEvent");
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                AdsHelper.onAdClick(adpl.getPlacement, "rewarded", "iron", appId);
            }
            else
            {
                AdsHelper.onAdClick(SDKManager.Instance.currPlacement, "rewarded", "iron", appId);
            }
        }
        private void RewardedVideoAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
        {
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                if (!adpl.isAddCondition)
                {
                    SdkUtil.logd($"ads gift {adpl.getPlacement}-{adpl.placement} iron RewardedVideoAdRewardedEvent was rcv onclose and will call close");
                    if (adpl.cbShow != null)
                    {
                        AdCallBack tmpcb = adpl.cbShow;
                        AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_OK); });
                        AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });
                        isRewardCom = false;
                        adpl.cbShow = null;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads gift {adpl.getPlacement}-{adpl.placement} iron RewardedVideoAdRewardedEvent not rcv onclose");
                    adpl.isAddCondition = false;
                    isRewardCom = true;
                }
            }
            else
            {
                SdkUtil.logd($"ads gift iron RewardedVideoAdRewardedEvent not pl");
                isRewardCom = true;
            }
        }
        private void RewardedVideoAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
        {
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                SdkUtil.logd($"ads gift {adpl.getPlacement}-{adpl.placement} iron OnAdDisplayFailed");
                advhelper.onCloseFullGift(false);
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    SdkUtil.logd($"ads gift {adpl.getPlacement} iron _cbAD fail");
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_FAIL); });
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                }
                adpl.isAddCondition = false;
                adpl.countLoad = 0;
                adpl.cbShow = null;
            }
            else
            {
                SdkUtil.logd($"ads gift iron OnAdDisplayFailed not pl");
            }
            onGiftClose(PLGiftDefault);
        }
        private void RewardedVideoAdOpenedEvent(IronSourceAdInfo adInfo)
        {
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                SdkUtil.logd($"ads gift {adpl.getPlacement}-{adpl.placement} iron OnAdDisplayed={adInfo.adNetwork}");
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads gift iron OnAdDisplayed not pl={adInfo.adNetwork}");
            }
        }
        private void RewardedVideoAdClosedEvent(IronSourceAdInfo adInfo)
        {
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                advhelper.onCloseFullGift(false);
                if (adpl.cbShow != null)
                {
                    if (!adpl.isAddCondition)
                    {
                        AdCallBack tmpcb = adpl.cbShow;
                        if (isRewardCom)
                        {
                            AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_OK); });
                        }
                        else
                        {
                            AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_FAIL); });
                        }
                        AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });
                        adpl.cbShow = null;
                    }
                    else
                    {
                        adpl.isAddCondition = false;
                        SdkUtil.logd($"ads gift {adpl.getPlacement}-{adpl.placement} iron HandleRewardBasedVideoClosed isRewardCom={isRewardCom} not rcv reward");
                    }
                }
                else
                {
                    SdkUtil.logd($"ads gift {adpl.getPlacement}-{adpl.placement} iron HandleRewardBasedVideoClosed isRewardCom={isRewardCom} not cb");
                }
                adpl.countLoad = 0;
            }
            else
            {
                SdkUtil.logd($"ads gift iron HandleRewardBasedVideoClosed not pl");
            }
            onGiftClose(PLGiftDefault);
            isRewardCom = false;
        }
        #endregion
#endif

#endif //!use_ver_840
    }
}