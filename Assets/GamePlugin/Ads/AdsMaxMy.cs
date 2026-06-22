//#define USE_ADSMAX_MY

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if USE_ADSMAX_MY
#endif

namespace mygame.sdk
{
    public class AdsMaxMy : AdsBase
    {

#if USE_ADSMAX_MY

#endif
        bool openadisnew = true;
        bool bnnmisnew = true;
        bool bnclisnew = true;
        bool bnrectisnew = true;
        bool nativefullisnew = true;
        bool fullisnew = true;
        bool giftisnew = true;

        static bool isdoinit = true;

        bool openIsClick = false;
        bool fullntIsClick = false;

        public override void InitAds()
        {
#if USE_ADSMAX_MY
            isEnable = true;
#endif
            if (isdoinit)
            {
                isdoinit = false;
#if !ENABLE_ADS_MAX && USE_ADSMAX_MY
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    SdkUtil.logd($"ads fullnt maxmy InitAds");
                    AdsMaxMyBridge.Instance.Initialize(appId);
                }, 0.5f);
#endif

                dicPLOpenAd.Clear();
                dicPLNtFull.Clear();

                initOpenAd();
                initNativeFull();
                fullAdNetwork = "max";
                giftAdNetwork = "max";
            }
        }

        public override void AdsAwake()
        {
#if USE_ADSMAX_MY
            isEnable = true;
#endif
        }

        private void Start()
        {
#if USE_ADSMAX_MY
            AdsMaxMyBridge.onOpenAdLoaded += OnOpenAdLoadedEvent;
            AdsMaxMyBridge.onOpenAdLoadFail += OnOpenAdFailedEvent;
            AdsMaxMyBridge.onOpenAdShowed += OnOpenAdDisplayedEvent;
            AdsMaxMyBridge.onOpenAdImpresstion += onOpenAdImpresstionEvent;
            AdsMaxMyBridge.onOpenAdClick += OnOpenAdClickEvent;
            AdsMaxMyBridge.onOpenAdFailedToShow += onOpenAdFailedToShow;
            AdsMaxMyBridge.onOpenAdDismissed += OnOpenAdDismissedEvent;
            AdsMaxMyBridge.onOpenAdPaid += OnOpenAdAdPaidEvent;

            AdsMaxMyBridge.onNativeFullLoaded += OnNativeFullLoadedEvent;
            AdsMaxMyBridge.onNativeFullLoadFail += OnNativeFullFailedEvent;
            AdsMaxMyBridge.onNativeFullShowed += OnNativeFullDisplayedEvent;
            AdsMaxMyBridge.onNativeFullImpresstion += OnNativeFullImpresstionEvent;
            AdsMaxMyBridge.onNativeFullClick += OnNativeFullClickEvent;
            AdsMaxMyBridge.onNativeFullFailedToShow += onNativeFullFailedToShow;
            AdsMaxMyBridge.onNativeFullDismissed += OnNativeFullDismissedEvent;
            AdsMaxMyBridge.onNativeFullFinishShow += OnNativeFullFinishShowEvent;
            AdsMaxMyBridge.onNativeFullPaid += OnNativeFullAdPaidEvent;

            InitAds();
            AdsMaxMyBridge.Instance.setTestDevices("8FCCEB68875BBA9C2EF3AEEEA2D953D8");//d8
#endif
        }

        private void OnDestroy()
        {
#if USE_ADSMAX_MY
            AdsMaxMyBridge.onOpenAdLoaded -= OnOpenAdLoadedEvent;
            AdsMaxMyBridge.onOpenAdLoadFail -= OnOpenAdFailedEvent;
            AdsMaxMyBridge.onOpenAdShowed -= OnOpenAdDisplayedEvent;
            AdsMaxMyBridge.onOpenAdImpresstion -= onOpenAdImpresstionEvent;
            AdsMaxMyBridge.onOpenAdClick -= OnOpenAdClickEvent;
            AdsMaxMyBridge.onOpenAdFailedToShow -= onOpenAdFailedToShow;
            AdsMaxMyBridge.onOpenAdDismissed -= OnOpenAdDismissedEvent;
            AdsMaxMyBridge.onOpenAdPaid -= OnOpenAdAdPaidEvent;

            AdsMaxMyBridge.onNativeFullLoaded -= OnNativeFullLoadedEvent;
            AdsMaxMyBridge.onNativeFullLoadFail -= OnNativeFullFailedEvent;
            AdsMaxMyBridge.onNativeFullShowed -= OnNativeFullDisplayedEvent;
            AdsMaxMyBridge.onNativeFullImpresstion -= OnNativeFullImpresstionEvent;
            AdsMaxMyBridge.onNativeFullClick -= OnNativeFullClickEvent;
            AdsMaxMyBridge.onNativeFullFailedToShow -= onNativeFullFailedToShow;
            AdsMaxMyBridge.onNativeFullDismissed -= OnNativeFullDismissedEvent;
            AdsMaxMyBridge.onNativeFullPaid -= OnNativeFullAdPaidEvent;
#endif
        }

        public void checkFloorInitRemote()
        {

        }
        private void initOpenAd()
        {
            try
            {
                SdkUtil.logd($"ads full maxmy openad=" + nativeId);
                dicPLOpenAd.Clear();
                AdPlacementFull plopen = new AdPlacementFull();
                dicPLOpenAd.Add(PLOpenDefault, plopen);
                plopen.placement = PLOpenDefault;
                plopen.adECPM.idxHighPriority = -1;
                plopen.adECPM.listFromDstring(nativeId);
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: maxmy init open ex=" + ex.ToString());
            }
        }
        private void initNativeFull()
        {
            try
            {
                SdkUtil.logd($"ads fullnt maxmy full init id=" + nativeFullId);
                dicPLNtFull.Clear();
                AdPlacementFull plfullnt = new AdPlacementFull();
                dicPLNtFull.Add(PLFullDefault, plfullnt);
                plfullnt.placement = PLFullDefault;
                plfullnt.adECPM.idxHighPriority = -1;
                plfullnt.adECPM.listFromDstring(nativeFullId);

                string memcfntfull = PlayerPrefs.GetString("mem_cf_ntfull_lic", "30,50,60,2,10");
                setCfNtFull(memcfntfull);
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: maxmy init fullnt ex=" + ex.ToString());
            }
        }
        public void setCfNtFull(string scfntfullClick)
        {
            if (scfntfullClick != null && scfntfullClick.Length > 0)
            {
#if USE_ADSMAX_MY
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
                        v2 = 50;
                    }
                    if (!int.TryParse(arrcfntfull[2], out v3))
                    {
                        v3 = 60;
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
                    AdsMaxMyBridge.Instance.setCfNtFull(v1, v2, v3, v4, v5, v6);
                }
#endif
            }
        }

        public override string getname()
        {
            return "adsmaxMy";
        }
        //===============================================================================
        protected override void tryLoadBanner(AdPlacementBanner adpl)
        {
            if (adpl != null)
            {
                if (adpl.cbLoad != null)
                {
                    adpl.cbLoad(AD_State.AD_LOAD_FAIL);
                }
            }
        }
        public override void loadBanner(string placement, AdCallBack cb)
        {
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
        }
        public override bool showBanner(string placement, int pos, int width, int maxH, AdCallBack cb, float dxCenter, bool highP = false)
        {
            return false;
        }
        public override void hideBanner()
        {
        }
        public override void destroyBanner()
        {
        }

        //===============================================================================
        public override int getOpenAdLoaded(string placement)
        {
            AdPlacementFull adpl = getPlOpenAd(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads full openad {placement} maxmy getOpenAdLoaded not pl");
                return 0;
            }
            int re = 0;
            if (adpl.isloaded)
            {
                re = 1;
            }
            else
            {
                SdkUtil.logd($"ads full openad {placement} maxmy getOpenAdLoaded={adpl.isloaded}");
            }

            return re;
        }

        protected override void tryLoadOpenAd(AdPlacementFull adpl)
        {
#if USE_ADSMAX_MY
            string idLoad = "";
            adpl.isAdHigh = false;
            if (adpl.adECPM.idxCurrEcpm >= adpl.adECPM.list.Count)
            {
                adpl.adECPM.idxCurrEcpm = 0;
            }
            idLoad = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
            if (adpl.adECPM.idxHighPriority >= adpl.adECPM.idxCurrEcpm)
            {
                adpl.isAdHigh = true;
            }
            SdkUtil.logd($"ads full openad {adpl.loadPl}-{adpl.placement} maxmy tryLoadOpenAd id={idLoad} idxCurrEcpm={adpl.adECPM.idxCurrEcpm} isHigh={adpl.isAdHigh}");

            int tryload = adpl.countLoad;
            if (tryload >= toTryLoad)
            {
                SdkUtil.logd($"ads full openad {adpl.loadPl}-{adpl.placement} maxmy tryLoadOpenAd over try");
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }

                return;
            }
            if (idLoad != null && idLoad.Length > 2)
            {
                AdsHelper.onAdLoad(adpl.loadPl, "openad", idLoad, "applovin");
                FIRhelper.logAdEvent("ads_openad_max_load");
                AppsFlyerHelperScript.logAdEvent("ads_load", "", "openad", idLoad, "applovin", "");
                adpl.isLoading = true;
                adpl.isloaded = false;
                AdsMaxMyBridge.Instance.loadOpenAd(adpl.placement, idLoad);
            }
            else
            {
                SdkUtil.logd($"ads full openad {adpl.loadPl}-{adpl.placement} maxmy tryLoadOpenAd id not correct");
                adpl.isLoading = false;
                adpl.isloaded = false;
            }
#else
            SdkUtil.logd($"ads full openad {adpl.loadPl}-{adpl.placement} maxmy tryLoadOpenAd not enable");
            var tmpcb = adpl.cbLoad;
            adpl.cbLoad = null;
            tmpcb(AD_State.AD_LOAD_FAIL);
#endif
        }
        public override void loadOpenAd(string placement, AdCallBack cb)
        {
#if USE_ADSMAX_MY
            SdkUtil.logd($"ads full openad {placement} maxmy loadOpenAd type={adsType}");
            AdPlacementFull adpl = getPlOpenAd(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads full openad {placement} maxmy loadOpenAd not placement");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            else
            {
                if (adpl.adECPM.list.Count <= 0)
                {
                    if (cb != null)
                    {
                        cb(AD_State.AD_LOAD_FAIL);
                    }
                    return;
                }
                if (!adpl.isloaded && !adpl.isLoading)
                {
                    SdkUtil.logd($"ads full openad {placement}-{adpl.placement} maxmy loadOpenAd");
                    adpl.cbLoad = cb;
                    openadisnew = false;
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadOpenAd(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads full openad {placement}-{adpl.placement} maxmy loadOpenAd isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                }
            }
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showOpenAd(string placement, float timeDelay, bool isShow2, bool isDelay, AdCallBack cb)
        {
            isOpenAd2 = isShow2;
            float tdelay = 4.02f;
#if UNITY_ANDROID
            tdelay = 0.1f;
#endif
            if (!isDelay)
            {
                tdelay = 0;
            }
            timeDelay += tdelay;
#if USE_ADSMAX_MY
            AdPlacementFull adpl = getPlOpenAd(placement);
            if (adpl != null)
            {
                adpl.cbShow = null;
                int ss = getOpenAdLoaded(adpl.placement);
                if (ss > 0)
                {
                    SdkUtil.logd($"ads full openad {placement}-{adpl.placement} maxmy showOpenAd type={adsType} isDelay={isDelay} tdl={tdelay}");
                    adpl.countLoad = 0;
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    if (timeDelay > 0)
                    {
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            AdsHelper.onAdShowStart(placement, "openad", "applovin", "");
                            AdsMaxMyBridge.Instance.showOpenAd(adpl.placement);
                            SDKManager.Instance.closeWaitShowFull();
                        }, tdelay);
                        return true;
                    }
                    else
                    {
                        AdsHelper.onAdShowStart(placement, "openad", "applovin", "");
                        return AdsMaxMyBridge.Instance.showOpenAd(adpl.placement);
                    }
                }
                else
                {
                    SdkUtil.logd($"ads full openad {placement}-{adpl.placement} maxmy showOpenAd type={adsType} not loaded");
                }
            }
            else
            {
                SdkUtil.logd($"ads full openad {placement} maxmy showOpenAd type={adsType} not pl");
            }
#endif
            return false;
        }
        //
        public override int getNativeFullLoaded(string placement)
        {
#if USE_ADSMAX_MY
            AdPlacementFull adpl = getPlNtFull(placement, true);
            if (adpl == null)
            {
                SdkUtil.logd($"ads fullnt {placement} maxmy getNativeFullLoaded not pl");
                return 0;
            }
            else
            {
                SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} maxmy getNativeFullLoaded={adpl.isloaded}");
                if (adpl.isloaded)
                {
                    return 1;
                }
            }
#endif
            return 0;
        }
        protected override void tryLoadNativeFull(AdPlacementFull adpl)
        {
#if USE_ADSMAX_MY
            string idLoad = "";
            adpl.isAdHigh = false;
            if (adpl.adECPM.idxCurrEcpm >= adpl.adECPM.list.Count)
            {
                adpl.adECPM.idxCurrEcpm = 0;
            }
            idLoad = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
            if (adpl.adECPM.idxHighPriority >= adpl.adECPM.idxCurrEcpm)
            {
                adpl.isAdHigh = true;
            }
            SdkUtil.logd($"ads fullnt {adpl.loadPl}-{adpl.placement} maxmy tryLoadNativeFull id={idLoad} idxCurrEcpmFull={adpl.adECPM.idxCurrEcpm} isFullHigh={adpl.isAdHigh}");

            int tryload = adpl.countLoad;
            if (tryload >= toTryLoad)
            {
                SdkUtil.logd($"ads fullnt {adpl.loadPl}-{adpl.placement} maxmy tryLoadNativeFull over try");
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }

                return;
            }
            if (idLoad != null && idLoad.Length > 2)
            {
                AdsHelper.onAdLoad(adpl.loadPl, "native_full", idLoad, "applovin");
                FIRhelper.logAdEvent("ads_fullnt_max_load");
                AppsFlyerHelperScript.logAdEvent("ads_load", "", "native_full", idLoad, "applovin", "");
                adpl.isLoading = true;
                adpl.isloaded = false;
                AdsMaxMyBridge.Instance.loadNativeFull(adpl.placement, idLoad, (int)advhelper.bnOrien);
            }
            else
            {
                SdkUtil.logd($"ads fullnt {adpl.loadPl}-{adpl.placement} maxmy tryLoadNativeFull id not correct");
                adpl.isLoading = false;
                adpl.isloaded = false;
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads fullnt {adpl.placement} maxmy tryLoadNativeFull not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadNativeFull(string placement, AdCallBack cb)
        {
#if USE_ADSMAX_MY
            AdPlacementFull adpl = getPlNtFull(placement, false);
            if (adpl == null)
            {
                SdkUtil.logd($"ads fullnt {placement} maxmy loadNativeFull not placement");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            else
            {
                if (adpl.adECPM.list.Count <= 0)
                {
                    SdkUtil.logd($"ads fullnt {placement} maxmy loadNativeFull list.Count=0");
                    if (cb != null)
                    {
                        cb(AD_State.AD_LOAD_FAIL);
                    }
                    return;
                }
                if (!adpl.isloaded && !adpl.isLoading)
                {
                    SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} maxmy loadNativeFull type={adsType}");
                    adpl.cbLoad = cb;
                    nativefullisnew = false;
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadNativeFull(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} maxmy loadNativeFull isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                }
            }
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showNativeFull(string placement, float timeDelay, int timeNtDl, bool isHideBtClose, bool isShow2, int timeClose, bool isAutoCloseWhenClick, AdCallBack cb)
        {
            isFullNt2 = isShow2;
#if USE_ADSMAX_MY
            AdPlacementFull adpl = getPlNtFull(placement, true);
            if (adpl != null)
            {
                adpl.cbShow = null;
                int ss = getNativeFullLoaded(adpl.placement);
                if (ss > 0)
                {
                    adpl.countLoad = 0;
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    if (timeDelay > 0)
                    {
                        SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} maxmy showNativeFull net={fullAdNetwork} timeDelay={timeDelay}");
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            AdsHelper.onAdShowStart(placement, "native_full", "applovin", "");
                            bool iss = AdsMaxMyBridge.Instance.showNativeFull(adpl.placement, timeClose, timeNtDl, isAutoCloseWhenClick);
                            if (!iss)
                            {
                                adpl.setShowing(false);
                                if (cb != null)
                                {
                                    cb(AD_State.AD_SHOW_FAIL);
                                }
                            }
                        }, timeDelay);
                        return true;
                    }
                    else
                    {
                        SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} maxmy showNativeFull net={fullAdNetwork} timeDelay=0");
                        AdsHelper.onAdShowStart(placement, "native_full", "applovin", "");
                        return AdsMaxMyBridge.Instance.showNativeFull(adpl.placement, timeClose, timeNtDl, isAutoCloseWhenClick);
                    }
                }
                else
                {
                    SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} maxmy showNativeFull not load");
                }
            }
            else
            {

                SdkUtil.logd($"ads fullnt {placement} maxmy showNativeFull not pl");
            }
#endif
            return false;
        }
        public static void reCountNtFull()
        {
#if UNITY_IOS || UNITY_IPHONE
            AdsMaxMyiOSBridge.reCountCurrShow();
#endif
        }
        //============
        public override void clearCurrFull(string placement)
        {
        }
        public override int getFullLoaded(string placement)
        {
            return 0;
        }
        protected override void tryLoadFull(AdPlacementFull adpl)
        {
            var tmpcb = adpl.cbLoad;
            adpl.cbLoad = null;
            tmpcb(AD_State.AD_LOAD_FAIL);
        }
        public override void loadFull(string placement, AdCallBack cb)
        {
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
            return;
        }
        public override bool showFull(string placement, float timeDelay, bool isShow2, AdCallBack cb)
        {
            isFull2 = isShow2;
            return false;
        }

        //
        public override void clearCurrGift(string placement)
        {
            if (getGiftLoaded(placement) == 1)
            {
            }
        }
        public override int getGiftLoaded(string placement)
        {
            return 0;
        }
        protected override void tryloadGift(AdPlacementFull adpl)
        {
            if (adpl != null && adpl.cbLoad != null)
            {
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
        }
        public override void loadGift(string placement, AdCallBack cb)
        {
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
        }
        public override bool showGift(string placement, float timeDelay, AdCallBack cb)
        {
            return false;
        }
        //-------------------------------------------------------------------------------
#if USE_ADSMAX_MY

        #region OPEN AD EVENTS
        private void OnOpenAdLoadedEvent(string placement, string adsId, string netName)
        {
            FIRhelper.logAdEvent("ads_openad_max_load_1");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "openad", adsId, "applovin", "1");
            if (dicPLOpenAd.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLOpenAd[placement];
                SdkUtil.logd($"ads full openad {adpl.loadPl}-{placement} maxmy OnOpenAdLoadedEvent net={netName}");
                string adsource = FIRhelper.getAdsourceMax(netName);
                AdsHelper.onAdLoadResult(adpl.loadPl, "openad", adsId, "applovin", adsource, true);
                fullAdNetwork = netName;
                adpl.countLoad = 0;
                adpl.isLoading = false;
                adpl.isloaded = true;
                openadisnew = false;
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
            }
            else
            {
                SdkUtil.logd($"ads full openad {placement} maxmy OnOpenAdLoadedEvent not pl");
            }
        }
        private void OnOpenAdFailedEvent(string placement, string adsId, string err)
        {
            FIRhelper.logAdEvent("ads_openad_max_load_0");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "openad", adsId, "applovin", "0");
            if (dicPLOpenAd.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLOpenAd[placement];
                SdkUtil.logd($"ads full openad {adpl.loadPl}-{placement} maxmy OnOpenAdFailedEvent=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "openad", adsId, "applovin", "", false);
                adpl.isLoading = false;
                adpl.isloaded = false;
                if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                {
                    if (openadisnew)
                    {
                        openadisnew = false;
                        adpl.adECPM.idxCurrEcpm = 0;
                    }
                    else
                    {
                        adpl.adECPM.idxCurrEcpm++;
                    }
                    tryLoadOpenAd(adpl);
                }
                else
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        dicPLOpenAd[placement].countLoad++;
                        tryLoadOpenAd(adpl);
                    }, 1.0f);
                }
            }
            else
            {
                SdkUtil.logd($"ads full openad {placement} maxmy OnOpenAdFailedEvent not pl err={err}");
            }
        }
        private void OnOpenAdDisplayedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLOpenAd.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLOpenAd[placement];
                SdkUtil.logd($"ads full openad {adpl.showPl}-{placement} maxmy OnOpenAdDisplayedEvent");
                if (dicPLOpenAd[placement].cbShow != null)
                {
                    AdCallBack tmpcb = dicPLOpenAd[placement].cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads full openad {placement} maxmy OnOpenAdDisplayedEvent not pl");
            }
        }
        private void onOpenAdImpresstionEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads full openad {placement} maxmy onOpenAdImpresstionEvent");
        }
        private void OnOpenAdClickEvent(string placement, string adsId, string adnet)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLOpenAd.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLOpenAd[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            SdkUtil.logd($"ads full openad {spl} maxmy OnOpenAdClickEvent");
            string adsource = FIRhelper.getAdsourceMax(adnet);
            AdsHelper.onAdClick(spl, "openad", "applovin", adsource, adsId);
            if (!openIsClick)
            {
                openIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "openad", adsId, "applovin", "");
            }
        }
        private void onOpenAdFailedToShow(string placement, string adsId, string adnet, string err)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLOpenAd.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLOpenAd[placement];
                SdkUtil.logd($"ads full openad {adpl.showPl}-{placement} maxmy onOpenAdFailedToShow=" + err);
                adpl.isloaded = false;
                adpl.isLoading = false;
                spl = adpl.showPl;
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                }
            }
            else
            {
                SdkUtil.logd($"ads full openad {placement} maxmy onOpenAdFailedToShow dic not contain pl");
            }
            string adsource = FIRhelper.getAdsourceMax(adnet);
            AdsHelper.onAdShowEnd(spl, "openad", "applovin", adsource, adsId, false, err);
            onFullClose(placement);
        }
        private void OnOpenAdDismissedEvent(string placement, string adsId, string adnet)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLOpenAd.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLOpenAd[placement];
                SdkUtil.logd($"ads full openad {adpl.showPl}-{placement} maxmy OnOpenAdDismissedEvent");
                dicPLOpenAd[placement].isloaded = false;
                dicPLOpenAd[placement].isLoading = false;
                spl = dicPLOpenAd[placement].showPl;
                if (dicPLOpenAd[placement].cbShow != null)
                {
                    AdCallBack tmpcb = dicPLOpenAd[placement].cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });
                }
                dicPLOpenAd[placement].cbShow = null;
                dicPLOpenAd[placement].countLoad = 0;
            }
            else
            {
                SdkUtil.logd($"ads full openad {placement} maxmy OnOpenAdDismissedEvent not pl");
            }
            string adsource = FIRhelper.getAdsourceMax(adnet);
            AdsHelper.onAdShowEnd(spl, "openad", "applovin", adsource, adsId, true, "");
            onFullClose(placement);
        }
        private void OnOpenAdAdPaidEvent(string placement, string adsId, string netName, string format, string adPlacement, string netPlacement, double value)
        {
            openIsClick = false;
            if (!isOpenAd2)
            {
                FIRhelper.logEvent("show_ads_total_imp");
                FIRhelper.logEvent("show_ads_full_imp");
                FIRhelper.logEvent("show_ads_full_imp_6_oad");
            }
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLOpenAd.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLOpenAd[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adsource = FIRhelper.getAdsourceMax(netName);
            FIRhelper.logEventAdsPaidMax(spl, "openad", adsource, adsId, value, "", "");
            TiktokBusiness.logAdRevenueMax(value, GameHelper.Instance.countryCode, netName, format, adsId, adPlacement, netPlacement);
            AdsHelper.onAdImpression(spl, adsId, "openad", "applovin", adsource, (float)value);

            FIRhelper.logAdEvent("ads_openad_max_imp");
            AppsFlyerHelperScript.logAdEvent("ads_impression", spl, "openad", adsId, "applovin", "");
        }
        #endregion

        #region Native Full AD EVENTS
        private void OnNativeFullLoadedEvent(string placement, string adsId, string netName)
        {
            FIRhelper.logAdEvent("ads_fullnt_max_load_1");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "native_full", adsId, "applovin", "1");
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.loadPl}-{placement} maxmy OnNativeFullLoadedEvent net={netName}");
                string adsource = FIRhelper.getAdsourceMax(netName);
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_full", adsId, "applovin", adsource, true);
                adpl.countLoad = 0;
                adpl.isLoading = false;
                adpl.isloaded = true;
                nativefullisnew = false;
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
            }
            else
            {
                SdkUtil.logd($"ads fullnt {placement} maxmy OnNativeFullLoadedEvent not pl");
            }
        }
        private void OnNativeFullFailedEvent(string placement, string adsId, string err)
        {
            FIRhelper.logAdEvent("ads_fullnt_max_load_0");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "native_full", adsId, "applovin", "0");
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.loadPl}-{placement} maxmy onload fail=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_full", adsId, "applovin", "", false);
                adpl.isLoading = false;
                adpl.isloaded = false;
                if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                {
                    if (nativefullisnew)
                    {
                        nativefullisnew = false;
                        adpl.adECPM.idxCurrEcpm = 0;
                    }
                    else
                    {
                        adpl.adECPM.idxCurrEcpm++;
                    }
                    tryLoadNativeFull(adpl);
                }
                else
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        adpl.countLoad++;
                        tryLoadNativeFull(adpl);
                    }, 1.0f);
                }
            }
            else
            {
                SdkUtil.logd($"ads fullnt {placement} maxmy onload fail=" + err);
            }
        }
        private void OnNativeFullDisplayedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.showPl}-{placement} maxmy onshow");
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads fullnt {placement} maxmy onshow not pl");
            }
        }
        private void OnNativeFullImpresstionEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads fullnt {placement} maxmy OnNativeFullImpresstionEvent");
        }
        private void onNativeFullFailedToShow(string placement, string adsId, string adnet, string err)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.showPl}-{placement} maxmy onshowfail=" + err);
                adpl.isLoading = false;
                adpl.isloaded = false;
                spl = adpl.showPl;
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                }
            }
            else
            {
                SdkUtil.logd($"ads fullnt {placement} maxmy not pl onshowfail=" + err);
            }
            string adsource = FIRhelper.getAdsourceMax(adnet);
            AdsHelper.onAdShowEnd(spl, "native_full", "applovin", adsource, adsId, false, err);
            onFullClose(placement);
            advhelper.onCloseFullGift(true);
        }
        private void OnNativeFullClickEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads fullnt {placement} maxmy click");
            if (!isFullNt2)
            {
                FIRhelper.logEvent("show_ads_full_nt6_click");
            }
            else
            {
                FIRhelper.logEvent("show_ads_full2_nt6_click");
            }
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adsource = FIRhelper.getAdsourceMax(adnet);
            AdsHelper.onAdClick(spl, "native_full", "applovin", adsource, adsId);
            if (!fullntIsClick)
            {
                fullntIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "native_full", adsId, "applovin", "");
            }
        }
        private void OnNativeFullDismissedEvent(string placement, string adsId, string adnet)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.showPl}-{placement} maxmy onclose");
                adpl.isLoading = false;
                adpl.isloaded = false;
                spl = adpl.showPl;
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });
                }

                adpl.countLoad = 0;
                adpl.cbShow = null;
            }
            else
            {
                SdkUtil.logd($"ads fullnt {placement} maxmy onclose not pl");
            }
            string adsource = FIRhelper.getAdsourceMax(adnet);
            AdsHelper.onAdShowEnd(spl, "native_full", "applovin", adsource, adsId, true, "");
            onFullClose(placement);
            advhelper.onCloseFullGift(true);
        }
        private void OnNativeFullFinishShowEvent(string placement, string adsId, string err)
        {
            //advhelper.onCloseFullGift(true);
        }
        private void OnNativeFullAdPaidEvent(string placement, string adsId, string netName, string format, string adPlacement, string netPlacement, double value)
        {
            fullntIsClick = false;
            if (!isFullNt2)
            {
                FIRhelper.logEvent("show_ads_total_imp");
                FIRhelper.logEvent("show_ads_full_imp");
                FIRhelper.logEvent("show_ads_full_imp_6_nt");
            }
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adsource = FIRhelper.getAdsourceMax(netName);
            FIRhelper.logEventAdsPaidMax(spl, "native_full", adsource, adsId, value, "", "");
            TiktokBusiness.logAdRevenueMax(value, GameHelper.Instance.countryCode, netName, format, adsId, adPlacement, netPlacement);
            AdsHelper.onAdImpression(spl, adsId, "native_full", "applovin", adsource, (float)value);

            FIRhelper.logAdEvent("ads_fullnt_max_imp");
            AppsFlyerHelperScript.logAdEvent("ads_impression", spl, "native_full", adsId, "applovin", "");
        }
        #endregion

#endif

    }
}