//#define ENABLE_ADS_IRON
//#define use_ir_ver_9x
#define use_load_all

using System;
using UnityEngine;
using System.Collections;

#if ENABLE_ADS_IRON && use_ir_ver_9x
using Unity.Services.LevelPlay;
using System.Collections.Generic;
#endif

namespace mygame.sdk
{
    public partial class AdsIron : AdsBase
    {
#if use_ir_ver_9x

#if ENABLE_ADS_IRON
        LevelPlayBannerAd bannerAd;
        LevelPlayInterstitialAd interstitialAd;
        LevelPlayRewardedAd rewardedAd;
#endif
        private static bool isInitAds = false;
        int posBnCurr = -1;
        private bool isAdsInited = false;
        private static bool isCallInit = false;
        private bool isAllowInit = false;
        private string plWaitFull = "aabc";
        private string plWaitGift = "aabc";
        private string plFullShow = "";
        private string plGiftShow = "";

        bool adsIsClick = false;

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
                LevelPlay.SetMetaData("do_not_sell", "false");

                int memage = PlayerPrefs.GetInt("mem_age_child", 14);
                if (memage >= 13)
                {
                    LevelPlay.SetMetaData("is_child_directed", "false");
                }
                else if (memage < 13 && memage > 5)
                {
                    LevelPlay.SetMetaData("is_child_directed", "true");
                    LevelPlay.SetMetaData("is_deviceid_optout", "true");
                    LevelPlay.SetConsent(false);
                }

                int memss = PlayerPrefs.GetInt("mem_ss_consent_ir", -1);
                if (memss != -1)
                {
                    if (memss == 1)
                    {
                        LevelPlay.SetConsent(true);
                    }
                    else
                    {
                        LevelPlay.SetConsent(false);
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
                LevelPlay.ValidateIntegration();
                LevelPlay.Init(appId, SystemInfo.deviceUniqueIdentifier);
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
                    LevelPlay.SetConsent(true);
                }
                else
                {
                    PlayerPrefs.SetInt("mem_ss_consent_ir", 0);
                    LevelPlay.SetConsent(false);
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

            LevelPlay.OnImpressionDataReady += OnImpressionDataReady;
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
                advhelper.onAdsInitSuc(3);
            }, 0.1f);
        }
#endif

        void OnApplicationPause(bool isPaused)
        {
#if ENABLE_ADS_IRON
            //LevelPlay.OnApplicationPause(isPaused);
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
                SdkUtil.logd($"ads bn {adpl.loadPl} iron bn tryLoadBanner id={idload}");
                var configBuilder = new LevelPlayBannerAd.Config.Builder();
                configBuilder.SetSize(LevelPlayAdSize.BANNER);
                if (adpl.posBanner == 0)
                {
                    configBuilder.SetPosition(LevelPlayBannerPosition.TopCenter);
                }
                else
                {
                    configBuilder.SetPosition(LevelPlayBannerPosition.BottomCenter);
                }
                configBuilder.SetDisplayOnLoad(false);
                configBuilder.SetRespectSafeArea(false); // Only relevant for Android
                configBuilder.SetPlacementName(adpl.loadPl);
                configBuilder.SetBidFloor(1.0); // Minimum bid price in USD
                var bannerConfig = configBuilder.Build();
                bannerAd = new LevelPlayBannerAd(idload, bannerConfig);
                bannerAd.OnAdLoaded += BannerOnAdLoadedEvent;
                bannerAd.OnAdLoadFailed += BannerOnAdLoadFailedEvent;
                bannerAd.OnAdDisplayed += BannerOnAdDisplayedEvent;
                bannerAd.OnAdDisplayFailed += BannerOnAdDisplayFailedEvent;
                bannerAd.OnAdClicked += BannerOnAdClickedEvent;
                bannerAd.OnAdCollapsed += BannerOnAdCollapsedEvent;
                bannerAd.OnAdLeftApplication += BannerOnAdLeftApplicationEvent;
                bannerAd.OnAdExpanded += BannerOnAdExpandedEvent;
                bannerAd.LoadAd();
                AdsHelper.onAdLoad(adpl.loadPl, "banner", idload, "iron");
            }
            else
            {
                SdkUtil.logd($"ads bn {adpl.loadPl} iron bn tryLoadBanner not pl");
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads bn {adpl.loadPl} iron bn tryLoadBanner not enable");
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
                    SdkUtil.logd($"ads bn {adpl.loadPl} iron bn waitLoadBannerWhenDestroy isloading");
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
                SdkUtil.logd($"ads bn {adpl.loadPl} iron bn loadBanner isloading={adpl.isLoading}");
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
                    SdkUtil.logd($"ads bn {adpl.loadPl} iron bn loadBanner isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
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
                SdkUtil.logd($"ads bn {adpl.loadPl} iron bn loadBanner not pl");
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
                SdkUtil.logd($"ads bn {adpl.showPl} iron bn showBanner isloading={adpl.isLoading}");
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
                    SdkUtil.logd($"ads bn {adpl.showPl} iron bn showBanner not show and load isloading={adpl.isLoading}");
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
                SdkUtil.logd($"ads bn {adpl.showPl} iron bn tryLoadBanner not pl");
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
                SdkUtil.logd($"ads full {adpl.showPl} iron getFullLoaded not pl");
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
                    SdkUtil.logd($"ads full {adpl.loadPl}-{adpl.placement} iron tryLoadFull load all id={idload}");
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
                    AdsHelper.onAdLoad(adpl.loadPl, "interstitial", idload, "iron");
                    FIRhelper.logAdEvent("ads_full_ir_load");
                    AppsFlyerHelperScript.logAdEvent("ads_load", "", "interstitial", idload, "iron", "");
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
                    SdkUtil.logd($"ads full {adpl.loadPl}-{adpl.placement} iron tryLoadFull id={adpl.adECPM.list[i].adsId} loading={adpl.adECPM.list[i].isLoading} loaded={adpl.adECPM.list[i].isLoaded}");
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
            FIRhelper.logAdEvent("ads_full_ir_load");
            AppsFlyerHelperScript.logAdEvent("ads_load", "", "interstitial", idload, "iron", "");
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
                    SdkUtil.logd($"ads full {adpl.loadPl}-{adpl.placement} iron loadFull type=" + adsType);
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.cbLoad = cb;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadFull(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads full {adpl.loadPl}-{adpl.placement} iron loadFull isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
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
                        SdkUtil.logd($"ads full {adpl.showPl}-{adpl.placement} iron showFull show net={netShow} timeDelay={timeDelay}");
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            plFullShow = placement;
                            AdsHelper.onAdShowStart(placement, "interstitial", "iron", interstitialAd.AdUnitId);
                            interstitialAd.ShowAd(placement);
                        }, timeDelay);
                        return true;
                    }
                    else
                    {
                        SdkUtil.logd($"ads full {adpl.showPl}-{adpl.placement} iron showFull show net={netShow}");
                        plFullShow = placement;
                        AdsHelper.onAdShowStart(placement, "interstitial", "iron", interstitialAd.AdUnitId);
                        interstitialAd.ShowAd(placement);
                        return true;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads full {adpl.showPl}-{adpl.placement} iron showFull show not loaded");
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
                SdkUtil.logd($"ads gift {adpl.showPl} iron getGiftLoaded not pl");
                return 0;
            }
            else
            {
#if use_load_all
                for (int i = 0; i < adpl.adECPM.list.Count; i++)
                {
                    if (adpl.adECPM.list[i].isLoaded)
                    {
                        giftAdNetwork = adpl.adECPM.list[i].adnetname;
                        return 1;
                    }
                }
#else

                if (adpl.isloaded && rewardedAd != null && rewardedAd.IsAdReady())
                {
                    return 1;
                }
#endif
            }

#endif
            return 0;
        }
        protected override void tryloadGift(AdPlacementFull adpl)
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
                    SdkUtil.logd($"ads gift {adpl.loadPl} iron tryloadGift gift={idload}");
                    adpl.adECPM.list[i].isLoading = true;
                    adpl.countLoad++;
                    var rewardedAd = new LevelPlayRewardedAd(idload);
                    adpl.adECPM.list[i].adObject = rewardedAd;

                    rewardedAd.OnAdLoaded += OnAdLoaded;
                    rewardedAd.OnAdLoadFailed += OnAdLoadFailed;
                    rewardedAd.OnAdDisplayed += OnAdDisplayed;
                    rewardedAd.OnAdDisplayFailed += OnAdDisplayFailed;
                    rewardedAd.OnAdRewarded += OnAdRewarded;
                    rewardedAd.OnAdClosed += OnAdClosed;
                    // Optional
                    rewardedAd.OnAdClicked += OnAdClicked;
                    rewardedAd.OnAdInfoChanged += OnAdInfoChanged;

                    //rewardedAd.OnAdLoaded += (adinfo) => { OnAdLoaded(idload, adinfo); };
                    //rewardedAd.OnAdLoadFailed += (adError) => {  OnAdLoadFailed(idload, adError); };
                    //rewardedAd.OnAdDisplayed += (adinfo) => { OnAdDisplayed(idload, adinfo); };
                    //rewardedAd.OnAdDisplayFailed += (adinfoError) => { OnAdDisplayFailed(idload, adinfoError); };
                    //rewardedAd.OnAdRewarded += (adinfo, adReward) => { OnAdRewarded(idload, adinfo, adReward); };
                    //rewardedAd.OnAdClosed += (adinfo) => { OnAdClosed(idload, adinfo); };
                    //// Optional
                    //rewardedAd.OnAdClicked += (adinfo) => { OnAdClicked(idload, adinfo); };
                    //rewardedAd.OnAdInfoChanged += (adinfo) => { OnAdInfoChanged(idload, adinfo); };

                    rewardedAd.LoadAd();
                    AdsHelper.onAdLoad(adpl.loadPl, "rewarded", idload, "iron");
                    FIRhelper.logAdEvent("ads_gift_ir_load");
                    AppsFlyerHelperScript.logAdEvent("ads_load", "", "rewarded", idload, "iron", "");
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
                    SdkUtil.logd($"ads gift {adpl.loadPl}-{adpl.placement} iron tryloadGift id={adpl.adECPM.list[i].adsId} loading={adpl.adECPM.list[i].isLoading} loaded={adpl.adECPM.list[i].isLoaded}");
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
            SdkUtil.logd($"ads gift {adpl.getPlacement} iron tryloadGift gift={idload}");
            rewardedAd = new LevelPlayRewardedAd(idload);
            rewardedAd.OnAdLoaded += OnAdLoaded;
            rewardedAd.OnAdLoadFailed += OnAdLoadFailed;
            rewardedAd.OnAdDisplayed += OnAdDisplayed;
            rewardedAd.OnAdDisplayFailed += OnAdDisplayFailed;
            rewardedAd.OnAdRewarded += OnAdRewarded;
            rewardedAd.OnAdClosed += OnAdClosed;
            // Optional
            rewardedAd.OnAdClicked += OnAdClicked;
            rewardedAd.OnAdInfoChanged += OnAdInfoChanged;
            rewardedAd.LoadAd();
            AdsHelper.onAdLoad(adpl.getPlacement, "rewarded", idload, "iron");
            FIRhelper.logAdEvent("ads_gift_ir_load");
            AppsFlyerHelperScript.logAdEvent("ads_load", "", "rewarded", idload, "iron", "");
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
                        SdkUtil.logd($"ads gift {adpl.loadPl}-{adpl.placement} iron waitGiftReady ok");
                        var tmpcb = adpl.cbLoad;
                        adpl.cbLoad = null;
                        tmpcb(AD_State.AD_LOAD_OK);
                    }
                    else
                    {
                        SdkUtil.logd($"ads gift {adpl.loadPl}-{adpl.placement} iron waitGiftReady fail");
                        var tmpcb = adpl.cbLoad;
                        adpl.cbLoad = null;
                        tmpcb(AD_State.AD_LOAD_FAIL);
                    }
                }
                else
                {
                    SdkUtil.logd($"ads gift {adpl.loadPl}-{adpl.placement} iron waitGiftReady cb null");
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
                SdkUtil.logd($"ads gift {adpl.loadPl} iron loadGift not placement");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            else
            {
#if use_load_all
                if (true)//(!adpl.isLoading)
#else
                if (!adpl.isloaded && !adpl.isLoading)
#endif
                {
                    SdkUtil.logd($"ads gift {adpl.loadPl}-{adpl.placement} iron loadGift type=" + adsType);
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.cbLoad = cb;
                    adpl.setSetPlacementLoad(placement);
                    tryloadGift(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads gift {adpl.loadPl}-{adpl.placement} iron loadGift isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
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
                    adpl.setSetPlacementShow(placement);
                    adpl.isAddCondition = true;
                    string idShow = "";
                    string netShow = "";
#if use_load_all
                    for (int i = 0; i < adpl.adECPM.list.Count; i++)
                    {
                        if (adpl.adECPM.list[i].isLoaded)
                        {
                            idShow = adpl.adECPM.list[i].adsId;
                            giftAdNetwork = adpl.adECPM.list[i].adnetname;
                            netShow = adpl.adECPM.list[i].adnetname;
                            rewardedAd = (LevelPlayRewardedAd)adpl.adECPM.list[i].adObject;
                            break;
                        }
                    }
#else
                    idShow = giftIdLoaded;
                    netShow = giftAdNetwork;
#endif
                    if (timeDelay > 0)
                    {
                        SdkUtil.logd($"ads gift {adpl.showPl}-{adpl.placement} iron showGift show net={netShow} timeDelay={timeDelay}");
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            plGiftShow = placement;
                            AdsHelper.onAdShowStart(placement, "rewarded", "iron", rewardedAd.AdUnitId);
                            rewardedAd.ShowAd(placement);
                        }, timeDelay);
                        return true;
                    }
                    else
                    {
                        SdkUtil.logd($"ads gift {adpl.showPl}-{adpl.placement} iron showGift show net={netShow}");
                        plGiftShow = placement;
                        AdsHelper.onAdShowStart(placement, "rewarded", "iron", rewardedAd.AdUnitId);
                        rewardedAd.ShowAd(placement);
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
        private void OnImpressionDataReady(LevelPlayImpressionData impressionData)
        {
            if (impressionData != null)
            {
                string adFormat = "";
                string adPl = "";
                adFormat = impressionData.AdFormat;
                if (adFormat != null)
                {
                    adFormat = adFormat.ToLower();
                }
                if (adFormat.Contains("banner"))
                {
                    SdkUtil.logd($"ads bn iron onpaid={adFormat}-{impressionData.AdNetwork}-{impressionData.MediationAdUnitId}-{impressionData.Revenue}");
                    FIRhelper.logEvent("show_ads_bn");
                    FIRhelper.logEvent("show_ads_bn_nm_3");
                    if (impressionData.Revenue != null && impressionData.Revenue.HasValue)
                    {
                        float realValue = (float)impressionData.Revenue.Value;
                        string adsource = FIRhelper.getAdsourceIron(impressionData.AdNetwork);
                        AdsHelper.onAdImpression(SDKManager.Instance.currPlacement, impressionData.MediationAdUnitId, "banner", "iron", adsource, realValue);
                    }
                    adFormat = "banner";
                    adPl = SDKManager.Instance.currPlacement;
                }
                else if (adFormat.Contains("interstitial"))
                {
                    adsIsClick = false;
                    if (!isFull2)
                    {
                        SdkUtil.logd($"ads full iron onpaid not isFull2 adNetwork={adFormat}-{impressionData.MediationAdUnitId}-{impressionData.Revenue}");
                        FIRhelper.logEvent("show_ads_total_imp");
                        FIRhelper.logEvent("show_ads_full_imp");
                        FIRhelper.logEvent("show_ads_full_imp_3");
                    }
                    else
                    {
                        SdkUtil.logd($"ads full iron onpaid isFull2 adNetwork={adFormat}-{impressionData.MediationAdUnitId}-{impressionData.Revenue}");
                    }
                    if (dicPLFull.ContainsKey(PLFullDefault))
                    {
                        if (impressionData.Revenue != null && impressionData.Revenue.HasValue)
                        {
                            AdPlacementFull adpl = dicPLFull[PLFullDefault];
                            float realValue = (float)impressionData.Revenue.Value;
                            string adsource = FIRhelper.getAdsourceIron(impressionData.AdNetwork);
                            AdsHelper.onAdImpression(adpl.showPl, impressionData.MediationAdUnitId, "interstitial", "iron", adsource, realValue);
                        }
                    }
                    adFormat = "interstitial";
                    adPl = plFullShow;
                    FIRhelper.logAdEvent("ads_full_ir_imp");
                    AppsFlyerHelperScript.logAdEvent("ads_impression", adPl, "interstitial", impressionData.MediationAdUnitId, "iron", "");
                }
                else if (adFormat.Contains("rewarded"))
                {
                    SdkUtil.logd($"ads gift iron onpaid adNetwork={adFormat}-{impressionData.MediationAdUnitId}-{impressionData.Revenue}");
                    FIRhelper.logEvent("show_ads_total_imp");
                    FIRhelper.logEvent("show_ads_reward_imp");
                    FIRhelper.logEvent("show_ads_reward_imp_3");
                    adsIsClick = false;
                    if (dicPLGift.ContainsKey(PLGiftDefault))
                    {
                        if (impressionData.Revenue != null && impressionData.Revenue.HasValue)
                        {
                            AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                            float realValue = (float)impressionData.Revenue.Value;
                            string adsource = FIRhelper.getAdsourceIron(impressionData.AdNetwork);
                            AdsHelper.onAdImpression(adpl.showPl, impressionData.MediationAdUnitId, "rewarded", "iron", adsource, realValue);
                        }
                    }
                    adPl = plGiftShow;
                    adFormat = "rewarded";
                    FIRhelper.logAdEvent("ads_gift_ir_imp");
                    AppsFlyerHelperScript.logAdEvent("ads_impression", adPl, "rewarded", impressionData.MediationAdUnitId, "iron", "");
                }
                else
                {
                    SdkUtil.logd($"ads {adFormat} iron onpaid adNetwork={adFormat}-{impressionData.MediationAdUnitId}-{impressionData.Revenue}");
                }
                adFormat = adFormat.ToLower();
                SdkUtil.logd($"ads iron imp adFormat={adFormat} va={impressionData.Revenue} pl={impressionData.Placement} net={impressionData.AdNetwork} mid={impressionData.MediationAdUnitId} mnane={impressionData.MediationAdUnitName}");
                FIRhelper.logEventAdsPaidIron(adPl, adFormat, impressionData.AdNetwork, impressionData.MediationAdUnitId, (double)impressionData.Revenue, impressionData.Country);
                
                double rvalue = 0;
                double lva = 0;
                if (impressionData.revenue != null && impressionData.revenue.HasValue)
                {
                    rvalue = impressionData.revenue.Value;
                }
                if (impressionData.lifetimeRevenue != null && impressionData.lifetimeRevenue.HasValue)
                {
                    lva = impressionData.lifetimeRevenue.Value;
                }
                IronSourceImpressionData ida = impressionData;
                TiktokBusiness.logAdRevenueIron(ida.auctionId, ida.adFormat, ida.adNetwork, ida.instanceName, ida.instanceId, ida.country, ida.placement, rvalue, ida.precision, ida.ab, ida.segmentName, lva, ida.encryptedCPM, ida.CreativeId);
            }
        }

        #region BANNER AD EVENTS
        void BannerOnAdLoadedEvent(LevelPlayAdInfo adInfo)
        {
            LevelPlayAdSize size = adInfo.AdSize;
            int width = size.Width;
            int height = size.Height;
            if (dicPLBanner.ContainsKey(PLBnDefault))
            {
                AdPlacementBanner adpl = dicPLBanner[PLBnDefault];
                SdkUtil.logd($"ads bn {adpl.loadPl} iron bn BannerOnAdLoadedEvent adNetwork={adInfo.AdNetwork}-{adInfo.AdFormat}-{adInfo.AdUnitId}");
                string adsource = FIRhelper.getAdsourceIron(adInfo.AdNetwork);
                AdsHelper.onAdLoadResult(adpl.loadPl, "banner", adInfo.AdUnitId, "iron", adsource, true);
                adpl.isloaded = true;
                adpl.isLoading = false;
                adpl.countLoad = 0;
                if (adpl.isShow)
                {
                    SdkUtil.logd($"ads bn {adpl.loadPl} iron bn BannerAdLoadedEvent show");
                    if (!adpl.isRealShow && advhelper.isShowBanner)
                    {
                        adpl.isRealShow = true;
                        bannerAd.ShowAd();
                    }
                    if (advhelper.bnCurrShow == adsType)
                    {
                        SdkUtil.logd($"ads bn {adpl.loadPl} iron bn BannerAdLoadedEvent hide other");
                        advhelper.hideOtherBanner(adsType);
                    }
                }
                else
                {
                    SdkUtil.logd($"ads bn {adpl.loadPl} iron bn BannerAdLoadedEvent hide");
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
                SdkUtil.logd($"ads bn iron bn BannerAdLoadedEvent not pl adNetwork={adInfo.AdNetwork}-{adInfo.AdFormat}-{adInfo.AdUnitId}");
            }
        }
        void BannerOnAdLoadFailedEvent(LevelPlayAdError ironSourceError)
        {
            if (dicPLBanner.ContainsKey(PLBnDefault))
            {
                AdPlacementBanner adpl = dicPLBanner[PLBnDefault];
                SdkUtil.logd($"ads bn {adpl.loadPl} iron bn BannerOnAdLoadFailedEvent AdUnitId={ironSourceError.AdUnitId}-{ironSourceError.ErrorCode}-{ironSourceError.ErrorMessage}");
                AdsHelper.onAdLoadResult(adpl.loadPl, "banner", ironSourceError.AdUnitId, "iron", "", false);
                if (adpl.isLoading)
                {
                    SdkUtil.logd($"ads bn {adpl.loadPl} iron bn BannerAdLoadFailedEvent isloading");
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
            SdkUtil.logd($"ads bn iro bn BannerAdClickedEvent={adInfo.AdUnitId} net={adInfo.AdNetwork}");
            SDKManager.Instance.onClickAd();
            string adsource = FIRhelper.getAdsourceIron(adInfo.AdNetwork);
            AdsHelper.onAdClick(SDKManager.Instance.currPlacement, "banner", "iron", adsource, adInfo.AdUnitId);
        }
        void BannerOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
        {
            SdkUtil.logd($"ads bn iron bn BannerOnAdDisplayedEvent={adInfo.AdUnitId} net={adInfo.AdNetwork}");
            if (advhelper.bnCurrShow == adsType)
            {
                SdkUtil.logd($"ads bn iron bn BannerOnAdDisplayedEvent hide other");
                advhelper.hideOtherBanner(adsType);
            }
        }
        void BannerOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError adError)
        {
            SdkUtil.logd($"ads bn iron bn BannerOnAdDisplayFailedEvent");
        }
        void BannerOnAdCollapsedEvent(LevelPlayAdInfo adInfo)
        {
            SdkUtil.logd($"ads bn iron bn BannerOnAdCollapsedEvent={adInfo.AdUnitId}");
        }
        void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo adInfo)
        {
            SdkUtil.logd($"ads bn iron bn BannerOnAdLeftApplicationEvent={adInfo.AdUnitId}");
        }
        void BannerOnAdExpandedEvent(LevelPlayAdInfo adInfo)
        {
            SdkUtil.logd($"ads bn iron bn BannerOnAdExpandedEvent={adInfo.AdUnitId}");
        }
        #endregion

        #region INTERSTITIAL AD EVENTS
        void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo)
        {
            string AdUnitId = adInfo.AdUnitId;
            FIRhelper.logAdEvent("ads_full_ir_load_1");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "interstitial", AdUnitId, "iron", "1");
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                SdkUtil.logd($"ads full {adpl.loadPl} iron HandleInterstitialAdDidLoad adNetwork={adInfo.AdNetwork}-{adInfo.AdFormat}-{AdUnitId}-{adInfo.Revenue}");
                string adsource = FIRhelper.getAdsourceIron(adInfo.AdNetwork);
                AdsHelper.onAdLoadResult(adpl.loadPl, "interstitial", AdUnitId, "iron", adsource, true);
                fullAdNetwork = adInfo.AdNetwork;
#if use_load_all
                adpl.countLoad--;
                adpl.isloaded = true;
                adpl.setStateAd4Id(AdUnitId, false, true, fullAdNetwork, adInfo.Revenue);
                if (adpl.isLoading)
                {
                    adpl.isLoading = false;
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        if (adpl.cbLoad != null)
                        {
                            var tmpcb = adpl.cbLoad;
                            adpl.cbLoad = null;

                            SdkUtil.logd($"ads full {adpl.loadPl} iron HandleInterstitialAdDidLoad=" + AdUnitId + " -> cb ok");
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
            FIRhelper.logAdEvent("ads_full_ir_load_0");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "interstitial", AdUnitId, "iron", "0");
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                SdkUtil.logd($"ads full {adpl.loadPl} iron InterstitialAdShowFailedEvent err={AdUnitId}-{error}");
                AdsHelper.onAdLoadResult(adpl.loadPl, "interstitial", AdUnitId, "iron", "", false);
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

                            SdkUtil.logd($"ads full {adpl.loadPl} iron InterstitialAdShowFailedEvent {AdUnitId} -> {adpl.isloaded}");
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
            string AdUnitId = adInfo.AdUnitId;
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                SdkUtil.logd($"ads full {adpl.showPl} iron InterstitialOnAdDisplayedEvent={AdUnitId}-{adInfo.AdNetwork}");
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads full iron InterstitialOnAdDisplayedEvent not pl={AdUnitId}-{adInfo.AdNetwork}");
            }
        }
        void InterstitialOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError adError)
        {
            string AdUnitId = adInfo.AdUnitId;
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                SdkUtil.logd($"ads full {adpl.showPl} iron InterstitialAdShowFailedEvent err=" + adError.ToString());
                AdsHelper.onAdShowEnd(adpl.showPl, "interstitial", "iron", adInfo.AdNetwork, AdUnitId, false, adError.ToString());
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
                SdkUtil.logd($"ads full iron InterstitialAdShowFailedEvent not pl err=" + adError.ToString());
            }
            onFullClose(PLFullDefault);
        }
        void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo)
        {
            string AdUnitId = adInfo.AdUnitId;
            string spl = "";
            SdkUtil.logd($"ads full iron InterstitialOnAdClickedEvent={AdUnitId}-{adInfo.AdNetwork}");
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                string adsource = FIRhelper.getAdsourceIron(adInfo.AdNetwork);
                AdsHelper.onAdClick(adpl.showPl, "interstitial", "iron", adsource, AdUnitId);
                spl = adpl.showPl;
            }
            else
            {
                AdsHelper.onAdClick(SDKManager.Instance.currPlacement, "interstitial", "iron", adInfo.AdNetwork, AdUnitId);
                spl = SDKManager.Instance.currPlacement;
            }
            if (!adsIsClick)
            {
                adsIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "interstitial", AdUnitId, "iron", "");
            }
        }
        void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo)
        {
            string AdUnitId = adInfo.AdUnitId;
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                SdkUtil.logd($"ads full {adpl.showPl} iron InterstitialOnAdClosedEvent={AdUnitId}-{adInfo.AdNetwork}");
                AdsHelper.onAdShowEnd(adpl.showPl, "interstitial", "iron", adInfo.AdNetwork, AdUnitId, true, "");
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
                SdkUtil.logd($"ads full iron InterstitialOnAdClosedEvent not pl={AdUnitId}-{adInfo.AdNetwork}");
            }
            onFullClose(PLFullDefault);
        }
        void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
        {
            string AdUnitId = adInfo.AdUnitId;
            SdkUtil.logd($"ads full iron InterstitialOnAdInfoChangedEvent={AdUnitId}-{adInfo.AdNetwork}");
        }
        #endregion

        #region REWARDED VIDEO AD EVENTS
        void OnAdLoaded(LevelPlayAdInfo adInfo)
        {
            string AdUnitId = adInfo.AdUnitId;
            FIRhelper.logAdEvent("ads_gift_ir_load_1");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "rewarded", AdUnitId, "iron", "1");
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                SdkUtil.logd($"ads gift {adpl.loadPl} iron OnAdLoaded adNetwork={adInfo.AdNetwork}-{AdUnitId}-{adInfo.Revenue}");
                string adsource = FIRhelper.getAdsourceIron(adInfo.AdNetwork);
                AdsHelper.onAdLoadResult(adpl.loadPl, "rewarded", AdUnitId, "iron", adsource, true);
#if use_load_all
                adpl.countLoad--;
                adpl.isloaded = true;
                adpl.setStateAd4Id(AdUnitId, false, true, adInfo.AdNetwork, adInfo.Revenue);
                if (adpl.isLoading)
                {
                    adpl.isLoading = false;
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        if (adpl.cbLoad != null)
                        {
                            var tmpcb = adpl.cbLoad;
                            adpl.cbLoad = null;
                            SdkUtil.logd($"ads gift {adpl.loadPl} iron OnAdLoaded={AdUnitId} -> cb ok");
                            tmpcb(AD_State.AD_LOAD_OK);
                        }
                    });
                }
#else
                adpl.countLoad = 0;
                adpl.isLoading = false;
                adpl.isloaded = true;
                giftAdNetwork = adInfo.AdNetwork;
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
                SdkUtil.logd($"ads gift iron OnAdLoaded not pl adNetwork={adInfo.AdNetwork}-{AdUnitId}");
            }
        }
        void OnAdLoadFailed(LevelPlayAdError adError)
        {
            string AdUnitId = adError.AdUnitId;
            FIRhelper.logAdEvent("ads_gift_ir_load_0");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "rewarded", AdUnitId, "iron", "0");
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                SdkUtil.logd($"ads gift {adpl.loadPl} iron OnAdLoadFailed err={adError}");
                AdsHelper.onAdLoadResult(adpl.loadPl, "rewarded", AdUnitId, "iron", "", false);
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

                            SdkUtil.logd($"ads gift {adpl.loadPl} iron OnAdLoadFailed {AdUnitId} -> {adpl.isloaded}");
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
                    tryloadGift(adpl);
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
                SdkUtil.logd($"ads gift iron OnAdLoadFailed not pl err={adError}");
            }
        }
        void OnAdDisplayed(LevelPlayAdInfo adInfo)
        {
            string AdUnitId = adInfo.AdUnitId;
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                SdkUtil.logd($"ads gift {adpl.showPl} iron OnAdDisplayed={adInfo.AdNetwork}-{AdUnitId}");
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads gift iron OnAdDisplayed not pl={adInfo.AdNetwork}-{AdUnitId}");
            }
        }
        void OnAdDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError adError)
        {
            string AdUnitId = adInfo.AdUnitId;
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                SdkUtil.logd($"ads gift {adpl.showPl} iron OnAdDisplayFailed={adError}");
                AdsHelper.onAdShowEnd(adpl.showPl, "rewarded", "iron", adInfo.AdNetwork, AdUnitId, false, adError.ToString());
                advhelper.onCloseFullGift(false);
                adpl.setStateAd4Id(AdUnitId, false, false, "", null);
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    SdkUtil.logd($"ads gift {adpl.showPl} iron _cbAD fail");
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_FAIL); });
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                }
#if !use_load_all
                adpl.isloaded = false;
#endif
                adpl.isAddCondition = false;
                adpl.cbShow = null;
            }
            else
            {
                SdkUtil.logd($"ads gift iron OnAdDisplayFailed not pl err={adError}");
            }
            onGiftClose(PLGiftDefault);
        }
        void OnAdClicked(LevelPlayAdInfo adInfo)
        {
            string AdUnitId = adInfo.AdUnitId;
            SdkUtil.logd($"ads gift iron OnAdClicked={adInfo.AdNetwork}-{AdUnitId}");
            string adsource = FIRhelper.getAdsourceIron(adInfo.AdNetwork);
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                spl = adpl.showPl;
            }
            AdsHelper.onAdClick(spl, "rewarded", "iron", adsource, AdUnitId);
            if (!adsIsClick)
            {
                adsIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "rewarded", AdUnitId, "iron", "");
            }
        }
        void OnAdRewarded(LevelPlayAdInfo adInfo, LevelPlayReward adReward)
        {
            string AdUnitId = adInfo.AdUnitId;
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                if (!adpl.isAddCondition)
                {
                    SdkUtil.logd($"ads gift iron RewardedVideoAdRewardedEvent was rcv onclose and will call close {adInfo.AdNetwork}-{AdUnitId}");
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
                    SdkUtil.logd($"ads gift iron RewardedVideoAdRewardedEvent not rcv onclose {adInfo.AdNetwork}-{AdUnitId}");
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
        void OnAdClosed(LevelPlayAdInfo adInfo)
        {
            string AdUnitId = adInfo.AdUnitId;
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                AdsHelper.onAdShowEnd(adpl.showPl, "rewarded", "iron", adInfo.AdNetwork, AdUnitId, true, "");
                advhelper.onCloseFullGift(false);
                adpl.setStateAd4Id(AdUnitId, false, false, "", null);
                if (!adpl.isAddCondition)
                {
                    if (adpl.cbShow != null)
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
                        SdkUtil.logd($"ads gift {adpl.showPl} iron HandleRewardBasedVideoClosed isRewardCom={isRewardCom} not cb {adInfo.AdNetwork}-{AdUnitId}");
                    }
                }
                else
                {
                    adpl.isAddCondition = false;
                    SdkUtil.logd($"ads gift {adpl.showPl} iron HandleRewardBasedVideoClosed isRewardCom={isRewardCom} not rcv reward {adInfo.AdNetwork}-{AdUnitId}");
                }
#if !use_load_all
                adpl.isloaded = false;
#endif
            }
            else
            {
                SdkUtil.logd($"ads gift iron HandleRewardBasedVideoClosed not pl");
            }
            onGiftClose(PLGiftDefault);
            isRewardCom = false;
        }
        void OnAdInfoChanged(LevelPlayAdInfo adInfo)
        {
            string AdUnitId = adInfo.AdUnitId;
            SdkUtil.logd($"ads gift iron OnAdInfoChanged={adInfo.AdNetwork}-{AdUnitId}");
        }
        #endregion
        //------
#endif

#endif //use_ir_ver_9x
    }
}