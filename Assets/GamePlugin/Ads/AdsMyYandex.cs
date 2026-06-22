//#define ENABLE_ADS_YANDEX

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if ENABLE_ADS_YANDEX

#endif

namespace mygame.sdk
{
    public class AdsMyYandex : AdsBase
    {
#if ENABLE_ADS_YANDEX

#endif
        long timeShowBanner = 0;
        float _membnDxCenter;
        bool __isInit = false;

        string bnidLoad = "";
        string fullidLoad = "";
        string giftidLoad = "";

        bool adsIsClick = false;

        public override void InitAds()
        {
#if ENABLE_ADS_YANDEX
            AdsMyYandexBridge.Instance.Initialize();
            isEnable = true;
            //AdsMyYandexBridge.Instance.setTestMode(true);
#endif
        }

        public override void AdsAwake()
        {
#if ENABLE_ADS_YANDEX
            isEnable = true;
#endif
        }

        private void Start()
        {
#if ENABLE_ADS_YANDEX
            if (advhelper == null || advhelper.currConfig == null)
            {
                return;
            }
            initBanner();
            initFull();
            initGift();
            __isInit = true;
            fullAdNetwork = "yandex";
            giftAdNetwork = "yandex";

            AdsMyYandexBridge.onBNLoaded += OnBannerAdLoadedEvent;
            AdsMyYandexBridge.onBNLoadFail += OnBannerAdLoadFailedEvent;
            AdsMyYandexBridge.onBNPaid += OnBannerAdPaidEvent;
            AdsMyYandexBridge.onBNClick += OnBannerAdClickEvent;

            AdsMyYandexBridge.onInterstitialLoaded += OnInterstitialLoadedEvent;
            AdsMyYandexBridge.onInterstitialLoadFail += OnInterstitialFailedEvent;
            AdsMyYandexBridge.onInterstitialShowed += OnInterstitialDisplayedEvent;
            AdsMyYandexBridge.onInterstitialFailedToShow += onInterstitialFailedToShow;
            AdsMyYandexBridge.onInterstitialClick += onInterstitialClickEvent;
            AdsMyYandexBridge.onInterstitialDismissed += OnInterstitialDismissedEvent;
            AdsMyYandexBridge.onInterstitialPaid += OnInterstitialAdPaidEvent;

            AdsMyYandexBridge.onRewardLoaded += OnRewardedAdLoadedEvent;
            AdsMyYandexBridge.onRewardLoadFail += OnRewardedAdFailedEvent;
            AdsMyYandexBridge.onRewardFailedToShow += OnRewardedAdFailedToDisplayEvent;
            AdsMyYandexBridge.onRewardShowed += OnRewardedAdDisplayedEvent;
            AdsMyYandexBridge.onRewardClick += OnRewardedAdClickEvent;
            AdsMyYandexBridge.onRewardDismissed += OnRewardedAdDismissedEvent;
            AdsMyYandexBridge.onRewardReward += OnRewardedAdReceivedRewardEvent;
            AdsMyYandexBridge.onRewardPaid += OnRewardedAdPaidEvent;
#endif
        }

        private void OnDestroy()
        {
#if ENABLE_ADS_YANDEX
            if (__isInit)
            {
                __isInit = false;
                AdsMyYandexBridge.onBNLoaded -= OnBannerAdLoadedEvent;
                AdsMyYandexBridge.onBNLoadFail -= OnBannerAdLoadFailedEvent;
                AdsMyYandexBridge.onBNPaid -= OnBannerAdPaidEvent;

                AdsMyYandexBridge.onInterstitialLoaded -= OnInterstitialLoadedEvent;
                AdsMyYandexBridge.onInterstitialLoadFail -= OnInterstitialFailedEvent;
                AdsMyYandexBridge.onInterstitialShowed -= OnInterstitialDisplayedEvent;
                AdsMyYandexBridge.onInterstitialFailedToShow -= onInterstitialFailedToShow;
                AdsMyYandexBridge.onInterstitialDismissed -= OnInterstitialDismissedEvent;
                AdsMyYandexBridge.onInterstitialPaid -= OnInterstitialAdPaidEvent;

                AdsMyYandexBridge.onRewardLoaded -= OnRewardedAdLoadedEvent;
                AdsMyYandexBridge.onRewardLoadFail -= OnRewardedAdFailedEvent;
                AdsMyYandexBridge.onRewardFailedToShow -= OnRewardedAdFailedToDisplayEvent;
                AdsMyYandexBridge.onRewardShowed -= OnRewardedAdDisplayedEvent;
                AdsMyYandexBridge.onRewardDismissed -= OnRewardedAdDismissedEvent;
                AdsMyYandexBridge.onRewardReward -= OnRewardedAdReceivedRewardEvent;
                AdsMyYandexBridge.onRewardPaid -= OnRewardedAdPaidEvent;
            }
#endif
        }

        public void initBanner()
        {
            try
            {
                SdkUtil.logd($"ads bn myYandex stepFloorECPMBanner=" + advhelper.currConfig.yandexStepFloorECPMBanner);
                if (advhelper.currConfig.yandexStepFloorECPMBanner.Length <= 2)
                {
                    advhelper.currConfig.yandexStepFloorECPMBanner = AdIdsConfig.YandexBnFloor;
                }
#if TEST_AD_RU
                if (!advhelper.currConfig.yandexStepFloorECPMBanner.Contains("demo-banner-yandex"))
                {
                    advhelper.currConfig.yandexStepFloorECPMBanner += ";demo-banner-yandex";
                }
#endif
                if (advhelper.currConfig.yandexStepFloorECPMBanner.Length > 0)
                {
                    dicPLBanner.Clear();
                    AdPlacementBanner plbn = new AdPlacementBanner();
                    dicPLBanner.Add(PLBnDefault, plbn);
                    plbn.placement = PLBnDefault;
                    plbn.adECPM.idxHighPriority = -1;
                    plbn.adECPM.listFromDstring(advhelper.currConfig.yandexStepFloorECPMBanner);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: bn myYandex initBanner ex=" + ex.ToString());
            }
        }

        public void initFull()
        {
            try
            {
                SdkUtil.logd($"ads full myYandex stepFloorECPMFull=" + advhelper.currConfig.yandexStepFloorECPMFull);
                if (advhelper.currConfig.yandexStepFloorECPMFull.Length <= 2)
                {
                    advhelper.currConfig.yandexStepFloorECPMFull = AdIdsConfig.YandexFullFloor;
                }
#if TEST_AD_RU
                if (!advhelper.currConfig.yandexStepFloorECPMFull.Contains("demo-interstitial-yandex"))
                {
                    advhelper.currConfig.yandexStepFloorECPMFull += ";demo-interstitial-yandex";
                }
#endif
                if (advhelper.currConfig.yandexStepFloorECPMFull.Length > 0)
                {
                    dicPLFull.Clear();
                    AdPlacementFull plfull = new AdPlacementFull();
                    dicPLFull.Add(PLFullDefault, plfull);
                    plfull.placement = PLFullDefault;
                    plfull.adECPM.idxHighPriority = -1;
                    plfull.adECPM.listFromDstring(advhelper.currConfig.yandexStepFloorECPMFull);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: full myYandex initFull ex=" + ex.ToString());
            }
        }

        public void initGift()
        {
            try
            {
                SdkUtil.logd($"ads gift myYandex stepFloorECPMGift=" + advhelper.currConfig.yandexStepFloorECPMGift);
                if (advhelper.currConfig.yandexStepFloorECPMGift.Length <= 2)
                {
                    advhelper.currConfig.yandexStepFloorECPMGift = AdIdsConfig.YandexGiftFloor;
                }
#if TEST_AD_RU
                if (!advhelper.currConfig.yandexStepFloorECPMGift.Contains("demo-rewarded-yandex"))
                {
                    advhelper.currConfig.yandexStepFloorECPMGift += ";demo-rewarded-yandex";
                }
#endif
                if (advhelper.currConfig.yandexStepFloorECPMGift.Length > 0)
                {
                    dicPLGift.Clear();
                    AdPlacementFull plgift = new AdPlacementFull();
                    dicPLGift.Add(PLGiftDefault, plgift);
                    plgift.placement = PLGiftDefault;
                    plgift.adECPM.idxHighPriority = -1;
                    plgift.adECPM.listFromDstring(advhelper.currConfig.yandexStepFloorECPMGift);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: gift myYandex initGift ex=" + ex.ToString());
            }
        }

        public override string getname()
        {
            return "myYandex";
        }

        protected override void tryLoadBanner(AdPlacementBanner adpl)
        {
#if ENABLE_ADS_YANDEX
            if (adpl != null)
            {
                if (adpl.adECPM.list.Count <= 0)
                {
                    if (adpl.cbLoad != null)
                    {
                        SdkUtil.logd($"ads bn myYandex tryLoadBanner not id");
                        var tmpcb = adpl.cbLoad;
                        adpl.cbLoad = null;
                        tmpcb(AD_State.AD_LOAD_FAIL);
                    }
                }
                else
                {
                    string idload = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
                    SdkUtil.logd($"ads bn myYandex tryLoadBanner = " + idload + ", idxCurrEcpm=" + adpl.adECPM.idxCurrEcpm);
                    AdsHelper.onAdLoad(adpl.loadPl, "banner", idload, "yandex");
                    adpl.isloaded = false;
                    adpl.isLoading = true;
                    bnidLoad = idload;
                    if (AppConfig.isBannerIpad)
                    {
                        AdsMyYandexBridge.Instance.showBanner(idload, adpl.posBanner, bnWidth, (int)advhelper.bnOrien, SdkUtil.isiPad(), _membnDxCenter);
                    }
                    else
                    {
                        AdsMyYandexBridge.Instance.showBanner(idload, adpl.posBanner, bnWidth, (int)advhelper.bnOrien, false, _membnDxCenter);
                    }
                    SdkUtil.logd($"ads bn myYandex tryLoadBanner1 _membnDxCenter=" + _membnDxCenter);
                }
            }
            else
            {
                SdkUtil.logd($"ads bn myYandex tryLoadBanner not pl");
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads bn myYandex tryLoadBanner not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadBanner(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_YANDEX
            AdPlacementBanner adpl = getPlBanner(placement, 0);
            if (adpl != null)
            {
                SdkUtil.logd($"ads bn myYandex loadBanner");
                adpl.cbLoad = cb;
                if (!adpl.isLoading)
                {
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadBanner(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads bn myYandex loadBanner isProcessShow");
                }
            }
            else
            {
                if (cb != null)
                {
                    SdkUtil.logd($"ads bn myYandex loadBanner not pl");
                    cb(AD_State.AD_LOAD_FAIL);
                }
            }
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads bn myYandex loadBanner not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showBanner(string placement, int pos, int width, int maxH, AdCallBack cb, float dxCenter, bool highP = false)
        {
#if ENABLE_ADS_YANDEX
            AdPlacementBanner adpl = getPlBanner(placement, 0);
            if (adpl != null)
            {
                if (!adpl.isLoading)
                {
                    adpl.adECPM.idxCurrEcpm = 0;
                }
                SdkUtil.logd($"ads bn myYandex showBanner pos=" + pos + ", dxCenter=" + dxCenter + ", idxCurrEcpm=" + adpl.adECPM.idxCurrEcpm + ", countecpm=" + adpl.adECPM.list.Count);
                adpl.isShow = true;
                adpl.posBanner = pos;
                adpl.setSetPlacementShow(placement);
                bnWidth = width;
                _membnDxCenter = dxCenter;
                if (!adpl.isLoading)
                {
                    int idxsh = -10;
                    for (int i = 0; i < adpl.adECPM.list.Count; i++)
                    {
                        AdECPMItem bnec = adpl.adECPM.list[i];
                        if (bnec.isLoaded)
                        {
                            adpl.isRealShow = true;
                            string idload = bnec.adsId;
                            SdkUtil.logd($"ads bn myYandex showBanner show pre loaded adsid=" + bnec.adsId + ", idx=" + i + ", dxCenter=" + dxCenter);
                            if (AppConfig.isBannerIpad)
                            {
                                AdsMyYandexBridge.Instance.showBanner(idload, adpl.posBanner, width, (int)advhelper.bnOrien, SdkUtil.isiPad(), dxCenter);
                            }
                            else
                            {
                                AdsMyYandexBridge.Instance.showBanner(idload, adpl.posBanner, width, (int)advhelper.bnOrien, false, dxCenter);
                            }
                            if (cb != null)
                            {
                                cb(AD_State.AD_SHOW);
                            }
                            idxsh = i;
                            break;
                        }
                    }

                    if (idxsh != -10)
                    {
                        if (cb != null)
                        {
                            cb(AD_State.AD_SHOW);
                        }
                        return true;
                    }
                    else
                    {
                        loadBanner(placement, cb);
                        return false;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads bn myYandex showBanner isprocess show dxCenter=" + dxCenter);
                    AdsMyYandexBridge.Instance.setBannerPos(pos, width, dxCenter);
                    return false;
                }
            }
            else
            {
                SdkUtil.logd($"ads bn myYandex showBanner not pl");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return false;
            }
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads myYandex tryLoadBanner not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
            return false;
#endif
        }
        public override void hideBanner()
        {
#if ENABLE_ADS_YANDEX
            SdkUtil.logd($"ads bn myYandex hideBanner");
            foreach (var adi in dicPLBanner)
            {
                adi.Value.isShow = false;
                adi.Value.isRealShow = false;
            }
            AdsMyYandexBridge.Instance.hideBanner();
#endif
        }
        public override void destroyBanner()
        {
#if ENABLE_ADS_YANDEX
            foreach (var adi in dicPLBanner)
            {
                adi.Value.isShow = false;
                adi.Value.isRealShow = false;
                adi.Value.isloaded = false;
            }
            AdsMyYandexBridge.Instance.hideBanner();
#endif
        }

        //Native

        //
        public override void clearCurrFull(string placement)
        {
#if ENABLE_ADS_YANDEX
            if (getFullLoaded(placement) == 1)
            {
                AdsMyYandexBridge.Instance.clearCurrFull();
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
#if ENABLE_ADS_YANDEX
            SdkUtil.logd($"ads full myYandex getFullLoaded");
            AdPlacementFull adpl = getPlFull(placement, true);
            if (adpl == null)
            {
                SdkUtil.logd($"ads full {placement} myYandex getFullLoaded not pl");
                return 0;
            }
            else
            {
                if (adpl.isloaded)
                {

                    return 1;
                }
            }
#endif
            return 0;
        }
        protected override void tryLoadFull(AdPlacementFull adpl)
        {
#if ENABLE_ADS_YANDEX
            string idLoad = "";
            if (adpl.adECPM.idxCurrEcpm >= adpl.adECPM.list.Count)
            {
                adpl.adECPM.idxCurrEcpm = 0;
            }
            idLoad = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
            SdkUtil.logd($"ads full myYandex tryLoadFull=" + idLoad + ", idxCurrEcpmFull=" + adpl.adECPM.idxCurrEcpm);
            int tryload = adpl.countLoad;
            if (tryload >= toTryLoad)
            {
                SdkUtil.logd($"ads full myYandex tryLoadFull over try");
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            SdkUtil.logd($"ads myYandex tryLoadFull load idLoad=" + idLoad);
            if (idLoad != null)
            {
                AdsHelper.onAdLoad(adpl.loadPl, "interstitial", idLoad, "yandex");
                FIRhelper.logAdEvent("ads_full_yandex_load");
                AppsFlyerHelperScript.logAdEvent("ads_load", "", "interstitial", idLoad, "yandex", "");
                adpl.isLoading = true;
                adpl.isloaded = false;
                fullidLoad = idLoad;
                AdsMyYandexBridge.Instance.loadFull(idLoad);
            }
            else
            {
                SdkUtil.logd($"ads myYandex tryLoadFull id not correct");
                adpl.isloaded = false;
                adpl.isLoading = false;
            }
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
#if ENABLE_ADS_YANDEX
            AdPlacementFull adpl = getPlFull(placement, false);
            if (adpl == null)
            {
                SdkUtil.logd($"ads full {placement} myYandex loadFull not placement");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            else
            {
                SdkUtil.logd($"ads full {placement} myYandex loadFull type=" + adsType);
                if (!adpl.isloaded && !adpl.isLoading)
                {
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.cbLoad = cb;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadFull(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads full myYandex loadFull isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
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
#if ENABLE_ADS_YANDEX
            SdkUtil.logd($"ads full myYandex showFull type={adsType} timeDelay={timeDelay}");
            AdPlacementFull adpl = getPlFull(placement, true);
            if (adpl != null)
            {
                adpl.cbShow = null;
                int ss = getFullLoaded(adpl.placement);
                if (ss > 0)
                {
                    SdkUtil.logd($"ads full myYandex showFull type=" + adsType);
                    adpl.countLoad = 0;
                    adpl.cbShow = cb;
                    adpl.isloaded = false;
                    adpl.setSetPlacementShow(placement);
                    if (timeDelay > 0)
                    {
                        AdsProcessCB.Instance().Enqueue(() => {
                            AdsHelper.onAdShowStart(placement, "interstitial", "yandex", "");
                            AdsMyYandexBridge.Instance.showFull();
                        }, timeDelay);
                        return true;
                    }
                    else
                    {
                        AdsHelper.onAdShowStart(placement, "interstitial", "yandex", "");
                        AdsMyYandexBridge.Instance.showFull();
                        return true;
                    }
                }
            }
#endif
            return false;
        }

        //
        public override void clearCurrGift(string placement)
        {
            if (getGiftLoaded(placement) == 1)
            {
#if ENABLE_ADS_YANDEX
                AdsMyYandexBridge.Instance.clearCurrGift();
                AdPlacementFull adpl = getPlGift(placement);
                if (adpl != null)
                {
                    adpl.isloaded = false;
                }
#endif
            }
        }
        public override int getGiftLoaded(string placement)
        {
#if ENABLE_ADS_YANDEX
            SdkUtil.logd($"ads gift myYandex getGiftLoaded");
            AdPlacementFull adpl = getPlGift(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads gift {placement} myYandex getGiftLoaded not pl");
                return 0;
            }
            else
            {
                if (adpl.isloaded)
                {

                    return 1;
                }
            }
#endif
            return 0;
        }
        protected override void tryloadGift(AdPlacementFull adpl)
        {
#if ENABLE_ADS_YANDEX
            string idLoad = "";
            if (adpl.adECPM.idxCurrEcpm >= adpl.adECPM.list.Count)
            {
                adpl.adECPM.idxCurrEcpm = 0;
            }
            idLoad = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
            SdkUtil.logd($"ads gift myYandex tryloadGift =" + idLoad + ", idxCurrEcpmGift=" + adpl.adECPM.idxCurrEcpm);
            if (adpl.countLoad >= toTryLoad)
            {
                SdkUtil.logd($"ads gift myYandex tryloadGift over try");
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            AdsHelper.onAdLoad(adpl.loadPl, "rewarded", idLoad, "yandex");
            FIRhelper.logAdEvent("ads_gift_yandex_load");
            AppsFlyerHelperScript.logAdEvent("ads_load", "", "rewarded", idLoad, "yandex", "");
            adpl.isLoading = true;
            adpl.isloaded = false;
            giftidLoad = idLoad;
            AdsMyYandexBridge.Instance.loadGift(idLoad);
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadGift(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_YANDEX
            SdkUtil.logd($"ads gift myYandex loadGift");
            AdPlacementFull adpl = getPlGift(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads gift {placement} myYandex loadGift not placement");
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
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.cbLoad = cb;
                    adpl.setSetPlacementLoad(placement);
                    tryloadGift(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads gift myYandex loadGift isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                }
            }
#else
#endif
        }
        public override bool showGift(string placement, float timeDelay, AdCallBack cb)
        {
#if ENABLE_ADS_YANDEX
            SdkUtil.logd($"ads gift myYandex showGift timeDelay={timeDelay}");
            AdPlacementFull adpl = getPlGift(placement);
            if (adpl != null)
            {
                adpl.cbShow = null;
                int ss = getGiftLoaded(adpl.placement);
                if (ss > 0)
                {
                    SdkUtil.logd($"ads gift myYandex showGift type=" + adsType);
                    adpl.countLoad = 0;
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    adpl.isloaded = false;
                    if (timeDelay > 0)
                    {
                        AdsProcessCB.Instance().Enqueue(() => {
                            AdsHelper.onAdShowStart(placement, "rewarded", "yandex", "");
                            AdsMyYandexBridge.Instance.showGift();
                        }, timeDelay);
                        return true;
                    }
                    else
                    {
                        AdsHelper.onAdShowStart(placement, "rewarded", "yandex", "");
                        AdsMyYandexBridge.Instance.showGift();
                        return true;
                    }
                }
            }
#endif
            return false;
        }

        //-------------------------------------------------------------------------------
#if ENABLE_ADS_YANDEX

        #region BANNER AD EVENTS
        public void OnBannerAdLoadedEvent()
        {
            if (dicPLBanner.ContainsKey(PLBnDefault))
            {
                SdkUtil.logd($"ads bn myYandex OnBannerAdLoadedEvent");
                AdPlacementBanner adpl = dicPLBanner[PLBnDefault];
                AdsHelper.onAdLoadResult(adpl.loadPl, "banner", bnidLoad, "yandex", "yandex", true);
                if (adpl.isLoading)
                {
                    adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].isLoaded = true;
                }
                adpl.isloaded = true;
                adpl.isLoading = false;
                adpl.countLoad = 0;

                if (adpl.isShow)
                {
                    if (advhelper.bnCurrShow == adsType)
                    {
                        SdkUtil.logd($"ads bn mytarget OnBannerAdLoadedEvent hide other");
                        advhelper.hideOtherBanner(adsType);
                    }
                }

                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }

            }
            else
            {
                SdkUtil.logd($"ads bn myYandex notpl OnBannerAdLoadedEvent");
            }
            if (advhelper != null)
            {
                advhelper.onBannerLoadOk(adsType);
            }
        }

        private void OnBannerAdLoadFailedEvent(string err)
        {
            if (dicPLBanner.ContainsKey(PLBnDefault))
            {
                SdkUtil.logd($"ads bn myYandex OnBannerAdLoadFailedEvent=" + err);
                AdPlacementBanner adpl = dicPLBanner[PLBnDefault];
                AdsHelper.onAdLoadResult(adpl.loadPl, "banner", bnidLoad, "yandex", "", false);
                if (adpl.isLoading)
                {
                    if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                    {
                        adpl.adECPM.idxCurrEcpm++;
                        if (!adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].isLoaded)
                        {
                            tryLoadBanner(adpl);
                        }
                        else
                        {
                            SdkUtil.logd($"ads bn myYandex OnBannerAdLoadFailedEvent finish1 load-{adpl.adECPM.idxCurrEcpm}-{adpl.adECPM.list.Count}");
                            adpl.adECPM.idxCurrEcpm = 0;
                            adpl.isLoading = false;
                            AdsProcessCB.Instance().Enqueue(() =>
                            {
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
                            });
                        }
                    }
                    else
                    {
                        SdkUtil.logd($"ads bn myYandex OnBannerAdLoadFailedEvent finish2 load-{adpl.adECPM.idxCurrEcpm}-{adpl.adECPM.list.Count}");
                        adpl.adECPM.idxCurrEcpm = 0;
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
                else
                {
                    SdkUtil.logd($"ads bn myYandex notpl OnBannerAdLoadFailedEvent not loading");
                }
            }
            else
            {
                SdkUtil.logd($"ads bn myYandex notpl OnBannerAdLoadFailedEvent=" + err);
            }
        }

        private void OnBannerAdPaidEvent(int precisionType, string currencyCode, long valueMicros)
        {
            //
        }
        private void OnBannerAdClickEvent()
        {
            AdsHelper.onAdClick(SDKManager.Instance.currPlacement, "banner", "yandex", "yandex", bnidLoad);
        }
        #endregion

        #region INTERSTITIAL AD EVENTS
        private void OnInterstitialLoadedEvent()
        {
            FIRhelper.logAdEvent("ads_full_yandex_load_1");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "interstitial", fullidLoad, "yandex", "1");
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                SdkUtil.logd($"ads full myYandex HandleInterstitialLoaded idxCurrEcpmFull=" + dicPLFull[PLFullDefault].adECPM.idxCurrEcpm);
                AdsHelper.onAdLoadResult(dicPLFull[PLFullDefault].loadPl, "interstitial", fullidLoad, "yandex", "yandex", true);
                dicPLFull[PLFullDefault].countLoad = 0;
                dicPLFull[PLFullDefault].isLoading = false;
                dicPLFull[PLFullDefault].isloaded = true;
                if (dicPLFull[PLFullDefault].cbLoad != null)
                {
                    var tmpcb = dicPLFull[PLFullDefault].cbLoad;
                    dicPLFull[PLFullDefault].cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
            }
            SdkUtil.logd($"ads full myYandex HandleInterstitialLoaded 1");
        }
        private void OnInterstitialFailedEvent(string err)
        {
            SdkUtil.logd($"ads full myYandex HandleInterstitialFailedToLoad=" + err);
            FIRhelper.logAdEvent("ads_full_yandex_load_0");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "interstitial", fullidLoad, "yandex", "0");
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                AdsHelper.onAdLoadResult(dicPLFull[PLFullDefault].loadPl, "interstitial", fullidLoad, "yandex", "", false);
                dicPLFull[PLFullDefault].isLoading = false;
                dicPLFull[PLFullDefault].isloaded = false;
                if (dicPLFull[PLFullDefault].adECPM.idxCurrEcpm < (dicPLFull[PLFullDefault].adECPM.list.Count - 1))
                {
                    dicPLFull[PLFullDefault].adECPM.idxCurrEcpm++;
                    tryLoadFull(dicPLFull[PLFullDefault]);
                }
                else
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        dicPLFull[PLFullDefault].countLoad++;
                        tryLoadFull(dicPLFull[PLFullDefault]);
                    }, 1.0f);
                }
            }
            SdkUtil.logd($"ads full myYandex HandleInterstitialFailedToLoad 1");
        }
        private void OnInterstitialDisplayedEvent()
        {
            SdkUtil.logd($"ads full myYandex HandleInterstitialOpened");
            adsIsClick = false;
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                spl = dicPLFull[PLFullDefault].showPl;
                if (dicPLFull[PLFullDefault].cbShow != null)
                {
                    AdCallBack tmpcb = dicPLFull[PLFullDefault].cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            FIRhelper.logAdEvent("ads_full_yandex_imp");
            AppsFlyerHelperScript.logAdEvent("ads_impression", spl, "interstitial", fullidLoad, "yandex", "");
        }
        private void onInterstitialFailedToShow(string err)
        {
            SdkUtil.logd($"ads full myYandex onInterstitialFailedToShow=" + err);
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                dicPLFull[PLFullDefault].isloaded = false;
                dicPLFull[PLFullDefault].isLoading = false;
                spl = dicPLFull[PLFullDefault].showPl;
                if (dicPLFull[PLFullDefault].cbShow != null)
                {
                    advhelper.onCloseFullGift(true);
                    AdCallBack tmpcb = dicPLFull[PLFullDefault].cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                }
            }
            AdsHelper.onAdShowEnd(spl, "interstitial", "yandex", "yandex", fullidLoad, false, err);
            onFullClose(PLFullDefault);
        }
        private void onInterstitialClickEvent()
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = PLFullDefault;
            }
            AdsHelper.onAdClick(spl, "interstitial", "yandex", "yandex", fullidLoad);
            if (!adsIsClick)
            {
                adsIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "interstitial", fullidLoad, "yandex", "");
            }
        }
        private void OnInterstitialDismissedEvent()
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                dicPLFull[PLFullDefault].isloaded = false;
                dicPLFull[PLFullDefault].isLoading = false;
                spl = dicPLFull[PLFullDefault].showPl;
                if (dicPLFull[PLFullDefault].cbShow != null)
                {
                    advhelper.onCloseFullGift(true);
                    AdCallBack tmpcb = dicPLFull[PLFullDefault].cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });
                }
                dicPLFull[PLFullDefault].cbShow = null;
                dicPLFull[PLFullDefault].countLoad = 0;
            }
            AdsHelper.onAdShowEnd(spl, "interstitial", "yandex", "yandex", fullidLoad, true, "");
            onFullClose(PLFullDefault);
            SdkUtil.logd($"ads full myYandex HandleInterstitialClosed 1");
        }
        private void OnInterstitialAdPaidEvent(int precisionType, string currencyCode, long valueMicros)
        {
            //
        }
        #endregion

        #region REWARDED VIDEO AD EVENTS
        private void OnRewardedAdLoadedEvent()
        {
            FIRhelper.logAdEvent("ads_gift_yandex_load_1");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "rewarded", giftidLoad, "yandex", "1");
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                SdkUtil.logd($"ads gift myYandex HandleRewardBasedVideoLoaded");
                AdsHelper.onAdLoadResult(dicPLGift[PLGiftDefault].loadPl, "rewarded", giftidLoad, "yandex", "yandex", true);
                dicPLGift[PLGiftDefault].countLoad = 0;
                dicPLGift[PLGiftDefault].isLoading = false;
                dicPLGift[PLGiftDefault].isloaded = true;
                if (dicPLGift[PLGiftDefault].cbLoad != null)
                {
                    var tmpcb = dicPLGift[PLGiftDefault].cbLoad;
                    dicPLGift[PLGiftDefault].cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
            }
            SdkUtil.logd($"ads gift myYandex HandleRewardBasedVideoLoaded 1");
        }
        private void OnRewardedAdFailedEvent(string err)
        {
            SdkUtil.logd($"ads gift myYandex HandleRewardBasedVideoFailedToLoad=" + err);
            FIRhelper.logAdEvent("ads_gift_yandex_load_0");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "rewarded", giftidLoad, "yandex", "0");
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                AdsHelper.onAdLoadResult(dicPLGift[PLGiftDefault].loadPl, "rewarded", giftidLoad, "yandex", "", false);
                dicPLGift[PLGiftDefault].isLoading = false;
                dicPLGift[PLGiftDefault].isloaded = false;
                if (dicPLGift[PLGiftDefault].adECPM.idxCurrEcpm < (dicPLGift[PLGiftDefault].adECPM.list.Count - 1))
                {
                    SdkUtil.logd($"ads gift myYandex OnRewardedAdFailedEvent load other ecpm idxCurrEcpmGift=" + dicPLGift[PLGiftDefault].adECPM.idxCurrEcpm + ", count=" + dicPLGift[PLGiftDefault].adECPM.list.Count);
                    dicPLGift[PLGiftDefault].adECPM.idxCurrEcpm++;
                    tryloadGift(dicPLGift[PLGiftDefault]);
                }
                else
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        dicPLGift[PLGiftDefault].countLoad++;
                        tryloadGift(dicPLGift[PLGiftDefault]);
                    }, 1.0f);
                }
            }
            SdkUtil.logd($"ads gift myYandex HandleRewardBasedVideoFailedToLoad 1");
        }
        private void OnRewardedAdDisplayedEvent()
        {
            adsIsClick = false;
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                SdkUtil.logd($"ads gift myYandex HandleRewardBasedVideoOpened");
                spl = dicPLGift[PLGiftDefault].showPl;
                dicPLGift[PLGiftDefault].countLoad = 0;
                if (dicPLGift[PLGiftDefault].cbShow != null)
                {
                    var tmpcb = dicPLGift[PLGiftDefault].cbShow;
                    SdkUtil.logd($"ads gift myYandex HandleRewardBasedVideoOpened1");
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            SdkUtil.logd($"ads gift myYandex HandleRewardBasedVideoOpened2");
            FIRhelper.logAdEvent("ads_gift_yandex_imp");
            AppsFlyerHelperScript.logAdEvent("ads_impression", spl, "rewarded", giftidLoad, "yandex", "");
        }
        private void OnRewardedAdFailedToDisplayEvent(string err)
        {
            SdkUtil.logd($"ads gift myYandex HandleRewardBasedVideoFailToShow=" + err);
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                dicPLGift[PLGiftDefault].isLoading = false;
                dicPLGift[PLGiftDefault].isloaded = false;
                spl = dicPLGift[PLGiftDefault].showPl;
                if (dicPLGift[PLGiftDefault].cbShow != null)
                {
                    advhelper.onCloseFullGift(false);
                    SdkUtil.logd($"ads gift myYandex _cbAD fail");
                    AdCallBack tmpcb = dicPLGift[PLGiftDefault].cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_FAIL); });
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                }
            }
            AdsHelper.onAdShowEnd(spl, "rewarded", "yandex", "yandex", giftidLoad, false, err);
            onGiftClose(PLGiftDefault);
        }
        private void OnRewardedAdClickEvent()
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = PLGiftDefault;
            }
            AdsHelper.onAdClick(spl, "rewarded", "yandex", "yandex", giftidLoad);
            if (!adsIsClick)
            {
                adsIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "rewarded", giftidLoad, "yandex", "");
            }
        }
        private void OnRewardedAdReceivedRewardEvent()
        {
            SdkUtil.logd($"ads gift myYandex HandleRewardBasedVideoRewarded");
            isRewardCom = true;
        }
        private void OnRewardedAdDismissedEvent()
        {
            SdkUtil.logd($"ads gift myYandex HandleRewardBasedVideoClosed");
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                dicPLGift[PLGiftDefault].isLoading = false;
                dicPLGift[PLGiftDefault].isloaded = false;
                spl = dicPLGift[PLGiftDefault].showPl;
                if (dicPLGift[PLGiftDefault].cbShow != null)
                {
                    advhelper.onCloseFullGift(false);
                    SdkUtil.logd($"ads gift myYandex _cbAD != null");
                    AdCallBack tmpcb = dicPLGift[PLGiftDefault].cbShow;
                    if (isRewardCom)
                    {
                        SdkUtil.logd($"ads gift myYandex _cbAD reward");
                        AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_OK); });
                    }
                    else
                    {
                        SdkUtil.logd($"ads gift myYandex _cbAD fail");
                        AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_FAIL); });
                    }
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });
                    dicPLGift[PLGiftDefault].countLoad = 0;
                    dicPLGift[PLGiftDefault].cbShow = null;
                }
            }
            AdsHelper.onAdShowEnd(spl, "rewarded", "yandex", "yandex", giftidLoad, true, "");
            onGiftClose(PLGiftDefault);
            isRewardCom = false;
            SdkUtil.logd($"ads gift myYandex HandleRewardBasedVideoClosed 3");
        }
        private void OnRewardedAdPaidEvent(int precisionType, string currencyCode, long valueMicros)
        {
            //
        }
        #endregion

#endif

    }
}