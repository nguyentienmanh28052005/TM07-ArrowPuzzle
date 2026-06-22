//#define ENABLE_ADS_FB
//#define Enable_Test_Ad

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_ADS_FB
#endif

namespace mygame.sdk
{
    public class AdsAFbMy : AdsBase
    {

#if ENABLE_ADS_FB

#endif

        static bool isdoinit = true;
        public static float FbPerPost = AppConfig.PerPostFbAdRev;

        bool fullntIsClick = false;
        bool fullnticIsClick = false;

        public override void InitAds()
        {
#if ENABLE_ADS_FB
            isEnable = true;
#endif
            if (isdoinit)
            {
                isdoinit = false;
#if Enable_Test_Ad
                AdsFbMyBridge.Instance.setLog(SdkUtil.isLog());
                AdsFbMyBridge.Instance.setTestDevices("edfe5fff-058d-4ac4-97e0-9a92bd66d0c4");//0338be0c-75a3-43ed-bb02-37e06e62c703
#endif
                AdsFbMyBridge.Instance.Initialize();

                dicPLNtFull.Clear();
                dicPLNtIcFull.Clear();

                initNativeFull();
                initNativeIcFull();
                fullAdNetwork = "fb";
                FbPerPost = PlayerPrefs.GetInt("fb_mem_per_post", AppConfig.PerPostFbAdRev);
            }
        }

        public override void AdsAwake()
        {
#if ENABLE_ADS_FB
            isEnable = true;
#endif
        }

        private void Start()
        {
#if ENABLE_ADS_FB
            AdsFbMyBridge.onNativeFullLoaded += OnNativeFullLoadedEvent;
            AdsFbMyBridge.onNativeFullLoadFail += OnNativeFullFailedEvent;
            AdsFbMyBridge.onNativeFullShowed += OnNativeFullDisplayedEvent;
            AdsFbMyBridge.onNativeFullImpresstion += OnNativeFullImpresstionEvent;
            AdsFbMyBridge.onNativeFullClick += OnNativeFullClickEvent;
            AdsFbMyBridge.onNativeFullFailedToShow += onNativeFullFailedToShow;
            AdsFbMyBridge.onNativeFullDismissed += OnNativeFullDismissedEvent;
            AdsFbMyBridge.onNativeFullFinishShow += OnNativeFullFinishShowEvent;
            AdsFbMyBridge.onNativeFullPaid += OnNativeFullAdPaidEvent;

            AdsFbMyBridge.onNativeIcFullLoaded += OnNativeIcFullLoadedEvent;
            AdsFbMyBridge.onNativeIcFullLoadFail += OnNativeIcFullFailedEvent;
            AdsFbMyBridge.onNativeIcFullShowed += OnNativeIcFullDisplayedEvent;
            AdsFbMyBridge.onNativeIcFullImpresstion += OnNativeIcFullImpresstionEvent;
            AdsFbMyBridge.onNativeIcFullClick += OnNativeIcFullClickEvent;
            AdsFbMyBridge.onNativeIcFullFailedToShow += onNativeIcFullFailedToShow;
            AdsFbMyBridge.onNativeIcFullDismissed += OnNativeIcFullDismissedEvent;
            AdsFbMyBridge.onNativeIcFullFinishShow += OnNativeIcFullFinishShowEvent;
            AdsFbMyBridge.onNativeIcFullPaid += OnNativeIcFullAdPaidEvent;

            InitAds();
#endif
        }

        private void OnDestroy()
        {
#if ENABLE_ADS_FB
            AdsFbMyBridge.onNativeFullLoaded -= OnNativeFullLoadedEvent;
            AdsFbMyBridge.onNativeFullLoadFail -= OnNativeFullFailedEvent;
            AdsFbMyBridge.onNativeFullShowed -= OnNativeFullDisplayedEvent;
            AdsFbMyBridge.onNativeFullImpresstion -= OnNativeFullImpresstionEvent;
            AdsFbMyBridge.onNativeFullClick -= OnNativeFullClickEvent;
            AdsFbMyBridge.onNativeFullFailedToShow -= onNativeFullFailedToShow;
            AdsFbMyBridge.onNativeFullDismissed -= OnNativeFullDismissedEvent;
            AdsFbMyBridge.onNativeFullFinishShow -= OnNativeFullFinishShowEvent;
            AdsFbMyBridge.onNativeFullPaid -= OnNativeFullAdPaidEvent;

            AdsFbMyBridge.onNativeIcFullLoaded -= OnNativeIcFullLoadedEvent;
            AdsFbMyBridge.onNativeIcFullLoadFail -= OnNativeIcFullFailedEvent;
            AdsFbMyBridge.onNativeIcFullShowed -= OnNativeIcFullDisplayedEvent;
            AdsFbMyBridge.onNativeIcFullImpresstion -= OnNativeIcFullImpresstionEvent;
            AdsFbMyBridge.onNativeIcFullClick -= OnNativeIcFullClickEvent;
            AdsFbMyBridge.onNativeIcFullFailedToShow -= onNativeIcFullFailedToShow;
            AdsFbMyBridge.onNativeIcFullDismissed -= OnNativeIcFullDismissedEvent;
            AdsFbMyBridge.onNativeIcFullFinishShow -= OnNativeIcFullFinishShowEvent;
            AdsFbMyBridge.onNativeIcFullPaid -= OnNativeIcFullAdPaidEvent;
#endif
        }

        private void initNativeFull()
        {
            try
            {
                Debug.Log("mysdk: ads fullnt fbmy init adCfPlacementNtFull=" + nativeFullId);
                dicPLNtFull.Clear();
                AdPlacementFull plNtFull = new AdPlacementFull();
                dicPLNtFull.Add(PLFullDefault, plNtFull);
                plNtFull.placement = PLFullDefault;
                plNtFull.adECPM.idxHighPriority = -1;
                plNtFull.adECPM.listFromDstring(nativeFullId);

                string memcfntfull = PlayerPrefs.GetString("mem_cf_ntfbfull_lic", "30,60,70,2,50");
                setCfNtFull(memcfntfull);
                string memcfntfullfbex = PlayerPrefs.GetString("cf_ntfull_fb_excluse", "8;8;7");
                setCfNtFullFbExcluse(memcfntfullfbex);
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ads fullnt fbmy initNativeFull ex=" + ex.ToString());
            }
        }
        private void initNativeIcFull()
        {
            try
            {
                Debug.Log("mysdk: ads fullnt fbmy init adCfPlacementNtIcFull=" + nativeFullId);
                dicPLNtIcFull.Clear();
                AdPlacementFull plNtFull = new AdPlacementFull();
                dicPLNtIcFull.Add(PLFullDefault, plNtFull);
                plNtFull.placement = PLFullDefault;
                plNtFull.adECPM.idxHighPriority = -1;
                plNtFull.adECPM.listFromDstring(nativeFullId);

                string memcfntfull = PlayerPrefs.GetString("mem_cf_ntfbfull_lic", "30,60,70,2,50");
                setCfNtFull(memcfntfull);
                string memcfntfullfbex = PlayerPrefs.GetString("cf_ntfull_fb_excluse", "8;8;7");
                setCfNtFullFbExcluse(memcfntfullfbex);
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ads fullnt fbmy initNativeFull ex=" + ex.ToString());
            }
        }

        public void checkFloorInitRemote()
        {
        }
        public void setCfNtFull(string scfntfullClick)
        {
            if (scfntfullClick != null && scfntfullClick.Length > 0)
            {
#if ENABLE_ADS_FB
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
                    AdsFbMyBridge.Instance.setCfNtFull(v1, v2, v3, v4, v5, v6);
                }
#endif
            }
        }
        public void setCfNtFullFbExcluse(string scf)
        {
            if (scf != null && scf.Length > 0)
            {
#if ENABLE_ADS_FB
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
                    AdsFbMyBridge.Instance.setCfNtFullFbExcluse(v1, v2, arrCf[2]);
                }
#endif
            }
        }

        public override string getname()
        {
            return "adsFbMy";
        }
        //
        protected override void tryLoadBanner(AdPlacementBanner adpl) { }
        public override void loadBanner(string placement, AdCallBack cb) { }
        public override bool showBanner(string placement, int pos, int width, int maxH, AdCallBack cb, float dxCenter, bool highP = false) { return false; }
        public override void hideBanner() { }
        public override void destroyBanner() { }

        //full nt
        public override int getNativeFullLoaded(string placement)
        {
#if ENABLE_ADS_FB
            AdPlacementFull adpl = getPlNtFull(placement, true);
            if (adpl == null)
            {
                SdkUtil.logd($"ads fullnt {placement} fbmy getNativeFullLoaded not pl");
                return 0;
            }
            else
            {
                if (adpl.isloaded)
                {
                    return 1;
                }
                else
                {
                    SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} fbmy getNativeFullLoaded={adpl.isloaded}");
                }
            }
#endif
            return 0;
        }
        protected override void tryLoadNativeFull(AdPlacementFull adpl)
        {
#if ENABLE_ADS_FB
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

            int tryload = adpl.countLoad;
            if (tryload >= toTryLoad)
            {
                SdkUtil.logd($"ads fullnt {adpl.loadPl}-{adpl.placement} fbmy tryLoadNativeFull over try");
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
#if Enable_Test_Ad
                if (Screen.height > Screen.width)
                {
                    idLoad = "VID_HD_9_16_39S_APP_INSTALL#" + idLoad;//VID_HD_9_16_39S_APP_INSTALL
                }
                else
                {
                    idLoad = "VID_HD_16_9_15S_APP_INSTALL#" + idLoad;//VID_HD_9_16_39S_APP_INSTALL
                }
#endif
                SdkUtil.logd($"ads fullnt {adpl.loadPl}-{adpl.placement} fbmy tryLoadNativeFull id={idLoad} idxCurrEcpmFull={adpl.adECPM.idxCurrEcpm} isFullHigh={adpl.isAdHigh} plid={adpl.idPl}");
                AdsHelper.onAdLoad(adpl.loadPl, "native_full", idLoad, "facebook");
                FIRhelper.logAdEvent("ads_fullnt_fb_load");
                AppsFlyerHelperScript.logAdEvent("ads_load", "", "native_full", idLoad, "facebook", "");
                adpl.isLoading = true;
                adpl.isloaded = false;
                AdsFbMyBridge.Instance.loadNativeFull(adpl.placement, idLoad, (int)advhelper.bnOrien);
            }
            else
            {
                SdkUtil.logd($"ads fullnt {adpl.loadPl}-{adpl.placement} fbmy tryLoadNativeFull id not correct");
                adpl.isLoading = false;
                adpl.isloaded = false;
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads fullnt {adpl.placement} fbmy tryLoadNativeFull not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadNativeFull(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_FB
            AdPlacementFull adpl = getPlNtFull(placement, false);
            if (adpl == null)
            {
                SdkUtil.logd($"ads fullnt {placement} fbmy loadNativeFull not placement");
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
                if (!adpl.isloaded && !adpl.isLoading && !adpl.getShowing())
                {
                    SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} fbmy loadNativeFull type={adsType}");
                    adpl.cbLoad = cb;
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadNativeFull(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} fbmy loadNativeFull isloading={adpl.isLoading} or isloaded={adpl.isloaded} showing={adpl.getShowing()}");
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
#if ENABLE_ADS_FB
            AdPlacementFull adpl = getPlNtFull(placement, true);
            if (adpl != null)
            {
                //adpl.cbShow = null;
                int ss = getNativeFullLoaded(adpl.placement);
                if (ss > 0 && !adpl.getShowing())
                {
                    SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} fbmy showNativeFull call show");
                    adpl.countLoad = 0;
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    AdsHelper.onAdShowStart(placement, "native_full", "facebook", "");
                    bool iss = AdsFbMyBridge.Instance.showNativeFull(adpl.placement, !isHideBtClose, timeClose, timeNtDl, isAutoCloseWhenClick);
                    adpl.setShowing(iss);
                    return iss;
                }
                else
                {
                    SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} fbmy showNativeFull not load or showing={adpl.getShowing()}");
                }
            }
            else
            {
                SdkUtil.logd($"ads fullnt {placement} fbmy showNativeFull not pl");
            }
#endif
            return false;
        }
        public static void reCountNtFull()
        {
#if UNITY_IOS || UNITY_IPHONE
            FbMyiOSBridge.reCountCurrShow();
#endif
        }

        //full nt Ic
        public override int getNativeIcFullLoaded(string placement)
        {
#if ENABLE_ADS_FB
            AdPlacementFull adpl = getPlNtIcFull(placement, true);
            if (adpl == null)
            {
                SdkUtil.logd($"ads fullnt {placement} fbmy getNativeIcFullLoaded not pl");
                return 0;
            }
            else
            {
                if (adpl.isloaded)
                {
                    return 1;
                }
                else
                {
                    SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} fbmy getNativeIcFullLoaded={adpl.isloaded}");
                }
            }
#endif
            return 0;
        }
        protected override void tryLoadNativeIcFull(AdPlacementFull adpl)
        {
#if ENABLE_ADS_FB
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

            int tryload = adpl.countLoad;
            if (tryload >= toTryLoad)
            {
                SdkUtil.logd($"ads fullnt {adpl.loadPl}-{adpl.placement} fbmy tryLoadNativeIcFull over try");
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
#if Enable_Test_Ad
                if (Screen.height > Screen.width)
                {
                    idLoad = "VID_HD_9_16_39S_APP_INSTALL#" + idLoad;//VID_HD_9_16_39S_APP_INSTALL
                }
                else
                {
                    idLoad = "VID_HD_16_9_15S_APP_INSTALL#" + idLoad;//VID_HD_9_16_39S_APP_INSTALL
                }
#endif
                SdkUtil.logd($"ads fullnt {adpl.loadPl}-{adpl.placement} fbmy tryLoadNativeIcFull id={idLoad} idxCurrEcpmFull={adpl.adECPM.idxCurrEcpm} isFullHigh={adpl.isAdHigh} plid={adpl.idPl}");
                AdsHelper.onAdLoad(adpl.loadPl, "native_full", idLoad, "facebook");
                FIRhelper.logAdEvent("ads_fullnt_fb_load");
                AppsFlyerHelperScript.logAdEvent("ads_load", "", "native_full", idLoad, "facebook", "");
                adpl.isLoading = true;
                adpl.isloaded = false;
                AdsFbMyBridge.Instance.loadNativeIcFull(adpl.placement, idLoad, (int)advhelper.bnOrien);
            }
            else
            {
                SdkUtil.logd($"ads fullnt {adpl.loadPl}-{adpl.placement} fbmy tryLoadNativeIcFull id not correct");
                adpl.isLoading = false;
                adpl.isloaded = false;
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads fullnt {adpl.placement} fbmy tryLoadNativeIcFull not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadNativeIcFull(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_FB

#if UNITY_IOS || UNITY_IPHONE
            SdkUtil.logd($"ads fullnt {placement} fbmy loadNativeIcFull not in ios");
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
            return;
#endif

            AdPlacementFull adpl = getPlNtIcFull(placement, false);
            if (adpl == null)
            {
                SdkUtil.logd($"ads fullnt {placement} fbmy loadNativeIcFull not placement");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            else
            {
                if (!adpl.isloaded && !adpl.isLoading && !adpl.getShowing())
                {
                    SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} fbmy loadNativeIcFull type={adsType}");
                    adpl.cbLoad = cb;
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadNativeIcFull(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} fbmy loadNativeIcFull isloading={adpl.isLoading} or isloaded={adpl.isloaded} showing={adpl.getShowing()}");
                }
            }
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showNativeIcFull(string placement, float timeDelay, int timeNtDl, bool isHideBtClose, bool isShow2, int timeClose, bool isAutoCloseWhenClick, AdCallBack cb)
        {
            isFullNt2 = isShow2;
#if ENABLE_ADS_FB
            AdPlacementFull adpl = getPlNtIcFull(placement, true);
            if (adpl != null)
            {
                //adpl.cbShow = null;
                int ss = getNativeIcFullLoaded(adpl.placement);
                if (ss > 0 && !adpl.getShowing())
                {
                    SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} fbmy showNativeIcFull call show");
                    adpl.countLoad = 0;
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    AdsHelper.onAdShowStart(placement, "native_full", "facebook", "");
                    bool iss = AdsFbMyBridge.Instance.showNativeIcFull(adpl.placement, !isHideBtClose, timeClose, timeNtDl, isAutoCloseWhenClick);
                    adpl.setShowing(iss);
                    return iss;
                }
                else
                {
                    SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} fbmy showNativeIcFull not load or showing={adpl.getShowing()}");
                }
            }
            else
            {
                SdkUtil.logd($"ads fullnt {placement} fbmy showNativeIcFull not pl");
            }
#endif
            return false;
        }

        //full
        public override void clearCurrFull(string placement) { }
        protected override void tryLoadFull(AdPlacementFull adpl) { }
        public override void loadFull(string placement, AdCallBack cb) { }
        public override bool showFull(string placement, float timeDelay, bool isShow2, AdCallBack cb) { return false; }

        //gift
        public override void clearCurrGift(string placement) { }
        protected override void tryloadGift(AdPlacementFull adpl) { }
        public override void loadGift(string placement, AdCallBack cb) { }
        public override bool showGift(string placement, float timeDelay, AdCallBack cb) { return false; }

        #region Full Native AD EVENTS
        private void OnNativeFullLoadedEvent(string placement, string adsId)
        {
            FIRhelper.logAdEvent("ads_fullnt_fb_load_1");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "native_full", adsId, "facebook", "1");
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.loadPl}-{placement} fbmy OnNativeFullLoadedEvent plid={adpl.idPl}");
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_full", adsId, "facebook", "facebook", true);
                adpl.countLoad = 0;
                adpl.isLoading = false;
                adpl.isloaded = true;
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
            }
            else
            {
                SdkUtil.logd($"ads fullnt {placement} fbmy OnNativeFullLoadedEvent not pl");
            }
        }
        private void OnNativeFullFailedEvent(string placement, string adsId, string err)
        {
            FIRhelper.logAdEvent("ads_fullnt_fb_load_0");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "native_full", adsId, "facebook", "0");
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.loadPl}-{placement} fbmy onload fail=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_full", adsId, "facebook", "facebook", false);
                adpl.isLoading = false;
                adpl.isloaded = false;
                if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                {
                    adpl.adECPM.idxCurrEcpm++;
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
                SdkUtil.logd($"ads fullnt {placement} fbmy onload fail=" + err);
            }
        }
        private void OnNativeFullDisplayedEvent(string placement, string adsId)
        {
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.showPl}-{placement} fbmy onshow");
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads fullnt {placement} fbmy onshow not pl");
            }
        }
        private void OnNativeFullImpresstionEvent(string placement, string adsId)
        {
            SdkUtil.logd($"ads fullnt {placement} fbmy OnNativeFullImpresstionEvent");
            fullntIsClick = false;
            float vacalcu = AdsHelper.Instance.FbntFullECPMCurr / 1000000.0f;
            if (vacalcu <= 0)
            {
                vacalcu = AdsHelper.Instance.FbntFullECPMdefault / 1000.0f;
            }
            float perpost = PlayerPrefs.GetInt("fb_mem_per_post", AppConfig.PerPostFbAdRev);
            if (perpost < 0)
            {
                perpost = 0;
            }
            else if (perpost > 200)
            {
                perpost = 200;
            }
            float vapost = vacalcu * perpost / 100.0f;
            if (!isFullNt2)
            {
                FIRhelper.logEvent("show_ads_total_imp");
                FIRhelper.logEvent("show_ads_full_imp");
                FIRhelper.logEvent("show_ads_full_imp_1_nt");
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
            if (vapost > 0)
            {
                SdkUtil.logd($"ads fullnt {placement} fbmy paid perpost={perpost} vapost={vapost}");
                FIRhelper.logEventAdsPaidFb(spl, "native_full", adsId, vapost);
                AdsHelper.onAdImpression(spl, adsId, "native_full", "facebook", "facebook", vapost);
            }
            FIRhelper.logAdEvent("ads_fullnt_fb_imp");
            AppsFlyerHelperScript.logAdEvent("ads_impression", spl, "native_full", adsId, "facebook", "");
        }
        private void onNativeFullFailedToShow(string placement, string adsId, string err)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.showPl}-{placement} fbmy onshowfail=" + err);
                adpl.isLoading = false;
                adpl.isloaded = false;
                adpl.setShowing(false);
                spl = adpl.showPl;
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                }
            }
            else
            {
                SdkUtil.logd($"ads fullnt {placement} fbmy not pl onshowfail=" + err);
            }
            AdsHelper.onAdShowEnd(spl, "native_full", "facebook", "facebook", adsId, false, err);
            onFullClose(placement);
            advhelper.onCloseFullGift(true);
        }
        private void OnNativeFullClickEvent(string placement, string adsId)
        {
            if (!isFullNt2)
            {
                FIRhelper.logEvent("show_ads_full_nt1_click");
            }
            else
            {
                FIRhelper.logEvent("show_ads_full2_nt1_click");
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
            AdsHelper.onAdClick(spl, "native_full", "facebook", "facebook", adsId);
            if (!fullntIsClick)
            {
                fullntIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "native_full", adsId, "facebook", "");
            }
        }
        private void OnNativeFullDismissedEvent(string placement, string adsId)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.showPl}-{placement} fbmy onclose");
                adpl.isLoading = false;
                adpl.isloaded = false;
                adpl.setShowing(false);
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
                SdkUtil.logd($"ads fullnt {placement} fbmy onclose not pl");
            }
            AdsHelper.onAdShowEnd(spl, "native_full", "facebook", "facebook", adsId, true, "");
            onFullClose(placement);
            advhelper.onCloseFullGift(true);
        }
        private void OnNativeFullFinishShowEvent(string placement, string adsId, string err)
        {
            //advhelper.onCloseFullGift(true);
        }
        private void OnNativeFullAdPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            //FIRhelper.logEventAdsPaidAdmob(3, adsId, precisionType, currencyCode, valueMicros);
        }
        #endregion
        #region Full Native Icon AD EVENTS
        private void OnNativeIcFullLoadedEvent(string placement, string adsId)
        {
            FIRhelper.logAdEvent("ads_fullnt_fb_load_1");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "native_full", adsId, "facebook", "1");
            if (dicPLNtIcFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtIcFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.loadPl}-{placement} fbmy OnNativeIcFullLoadedEvent plid={adpl.idPl}");
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_full", adsId, "facebook", "facebook", true);
                adpl.countLoad = 0;
                adpl.isLoading = false;
                adpl.isloaded = true;
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
            }
            else
            {
                SdkUtil.logd($"ads fullnt {placement} fbmy OnNativeIcFullLoadedEvent not pl");
            }
        }
        private void OnNativeIcFullFailedEvent(string placement, string adsId, string err)
        {
            FIRhelper.logAdEvent("ads_fullnt_fb_load_0");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "native_full", adsId, "facebook", "0");
            if (dicPLNtIcFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtIcFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.loadPl}-{placement} fbmy OnNativeIcFullFailedEvent=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_full", adsId, "facebook", "facebook", false);
                adpl.isLoading = false;
                adpl.isloaded = false;
                if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                {
                    adpl.adECPM.idxCurrEcpm++;
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
                SdkUtil.logd($"ads fullnt {placement} fbmy OnNativeIcFullFailedEvent=" + err);
            }
        }
        private void OnNativeIcFullDisplayedEvent(string placement, string adsId)
        {
            if (dicPLNtIcFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtIcFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.showPl}-{placement} fbmy OnNativeIcFullDisplayedEvent");
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads fullnt {placement} fbmy OnNativeIcFullDisplayedEvent not pl");
            }
        }
        private void OnNativeIcFullImpresstionEvent(string placement, string adsId)
        {
            SdkUtil.logd($"ads fullnt {placement} fbmy OnNativeIcFullImpresstionEvent");
            fullnticIsClick = false;
            float vacalcu = AdsHelper.Instance.FbntFullECPMCurr / 1000000.0f;
            if (vacalcu <= 0)
            {
                vacalcu = AdsHelper.Instance.FbntFullECPMdefault / 1000.0f;
            }
            float perpost = PlayerPrefs.GetInt("fb_mem_per_post", AppConfig.PerPostFbAdRev);
            if (perpost < 0)
            {
                perpost = 0;
            }
            else if (perpost > 200)
            {
                perpost = 200;
            }
            float vapost = vacalcu * perpost / 100.0f;
            if (!isFullNt2)
            {
                FIRhelper.logEvent("show_ads_total_imp");
                FIRhelper.logEvent("show_ads_full_imp");
                FIRhelper.logEvent("show_ads_full_imp_1_nt");
            }
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtIcFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtIcFull[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            if (vapost > 0)
            {
                SdkUtil.logd($"ads fullnt {placement} fbmy paid perpost={perpost} vapost={vapost}");
                FIRhelper.logEventAdsPaidFb(spl, "native_full", adsId, vapost);
                AdsHelper.onAdImpression(spl, adsId, "native_full", "facebook", "facebook", vapost);
            }
            FIRhelper.logAdEvent("ads_fullnt_fb_imp");
            AppsFlyerHelperScript.logAdEvent("ads_impression", spl, "native_full", adsId, "facebook", "");
        }
        private void onNativeIcFullFailedToShow(string placement, string adsId, string err)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtIcFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtIcFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.showPl}-{placement} fbmy OnNativeIcFullImpresstionEvent=" + err);
                adpl.isLoading = false;
                adpl.isloaded = false;
                adpl.setShowing(false);
                spl = adpl.showPl;
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                }
            }
            else
            {
                SdkUtil.logd($"ads fullnt {placement} fbmy not pl OnNativeIcFullImpresstionEvent=" + err);
            }
            AdsHelper.onAdShowEnd(spl, "native_full", "facebook", "facebook", adsId, false, err);
            onFullClose(placement);
            advhelper.onCloseFullGift(true);
        }
        private void OnNativeIcFullClickEvent(string placement, string adsId)
        {
            if (!isFullNt2)
            {
                FIRhelper.logEvent("show_ads_full_nt1_click");
            }
            else
            {
                FIRhelper.logEvent("show_ads_full2_nt1_click");
            }
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtIcFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtIcFull[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            AdsHelper.onAdClick(spl, "native_full", "facebook", "facebook", adsId);
            if (!fullnticIsClick)
            {
                fullnticIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "native_full", adsId, "facebook", "");
            }
        }
        private void OnNativeIcFullDismissedEvent(string placement, string adsId)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtIcFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtIcFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.showPl}-{placement} fbmy OnNativeIcFullDismissedEvent");
                adpl.isLoading = false;
                adpl.isloaded = false;
                adpl.setShowing(false);
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
                SdkUtil.logd($"ads fullnt {placement} fbmy OnNativeIcFullDismissedEvent not pl");
            }
            AdsHelper.onAdShowEnd(spl, "native_full", "facebook", "facebook", adsId, true, "");
            onFullClose(placement);
            advhelper.onCloseFullGift(true);
        }
        private void OnNativeIcFullFinishShowEvent(string placement, string adsId, string err)
        {
            //advhelper.onCloseFullGift(true);
        }
        private void OnNativeIcFullAdPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            //FIRhelper.logEventAdsPaidAdmob(3, adsId, precisionType, currencyCode, valueMicros);
        }
        #endregion
    }
}