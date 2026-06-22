//#define ENABLE_ADS_ADMOB

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
#endif

namespace mygame.sdk
{
    public class AdsAdmobMyLow : AdsBase
    {
        static bool isdoinit = true;
        int idxLoadFull = 0;
        bool fullowIsClick = false;

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY

#endif
        float _membnDxCenter;

        public override void InitAds()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            isEnable = true;
#endif
        }

        public override void AdsAwake()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            isEnable = true;
#endif  
        }

        public List<string> listIds = new List<string>();

        private void Start()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdsAdmobMyLowBridge.onInterstitialLoaded += OnInterstitialLoadedEvent;
            AdsAdmobMyLowBridge.onInterstitialLoadFail += OnInterstitialFailedEvent;
            AdsAdmobMyLowBridge.onInterstitialShowed += OnInterstitialDisplayedEvent;
            AdsAdmobMyLowBridge.onInterstitialImpresstion += OnInterstitialImpresstionEvent;
            AdsAdmobMyLowBridge.onInterstitialClick += OnInterstitialClickEvent;
            AdsAdmobMyLowBridge.onInterstitialFailedToShow += onInterstitialFailedToShow;
            AdsAdmobMyLowBridge.onInterstitialDismissed += OnInterstitialDismissedEvent;
            AdsAdmobMyLowBridge.onInterstitialFinishShow += OnInterstitialFinishShowEvent;
            AdsAdmobMyLowBridge.onInterstitialPaid += OnInterstitialAdPaidEvent;

            if (isdoinit)
            {
                isdoinit = false;
                //AdsAdmobMyLowBridge.Instance.Initialize();
                string slid = PlayerPrefs.GetString($"mem_ad_placement_low", AdIdsConfig.AdmobPlFullImg);
                initIdLow(slid);
            }
            fullAdNetwork = "admob";
            giftAdNetwork = "admob";
#endif      
        }

        public void initIdLow(string slid)
        {
            try
            {
                Debug.Log("mysdk: ads full admobmyLow init ids=" + slid);
                bool isFull2 = false;
                if (slid.Length > 0)
                {
                    string[] listpl = slid.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementFull>(dicPLFull, plitem, true);
                        if (!isFull2 && plitem.Contains(PLFull2Default))
                        {
                            isFull2 = true;
                        }
                    }
                }
                string[] listpldf = AdIdsConfig.AdmobPlFullImg.Split(new char[] { '#' });
                foreach (string plitem in listpldf)
                {
                    addAdPlacement<AdPlacementFull>(dicPLFull, plitem, false);
                    if (!isFull2 && plitem.Contains(PLFull2Default))
                    {
                        isFull2 = true;
                    }
                }
                if (dicPLFull.ContainsKey(PLFullDefault))
                {
                    if (!dicPLFull.ContainsKey(PLFull2Default))
                    {
                        AdPlacementFull adplfull2 = new AdPlacementFull();
                        adplfull2.coppyFrom(dicPLFull[PLFullDefault]);
                        adplfull2.placement = PLFull2Default;
                        dicPLFull.Add(PLFull2Default, adplfull2);
                    }
                    else
                    {
                        if (!isFull2)
                        {
                            AdPlacementFull adplfull2 = dicPLFull[PLFull2Default];
                            List<AdECPMItem> tmpl = new List<AdECPMItem>();
                            tmpl.AddRange(adplfull2.adECPM.list);
                            adplfull2.coppyFrom(dicPLFull[PLFullDefault]);
                            adplfull2.placement = PLFull2Default;

                            for (int i = 0; i < tmpl.Count; i++)
                            {
                                for (int j = 0; j < adplfull2.adECPM.list.Count; j++)
                                {
                                    if (adplfull2.adECPM.list[j].adsId.CompareTo(tmpl[i].adsId) == 0)
                                    {
                                        adplfull2.adECPM.list[j].coppyFrom(tmpl[i]);
                                        tmpl.RemoveAt(i);
                                        i--;
                                        break;
                                    }
                                }
                            }
                            if (tmpl.Count > 0)
                            {
                                Debug.Log("ads full admobmy init pl=2.count=" + tmpl.Count);
                                adplfull2.adECPM.list.AddRange(tmpl);
                            }
                            tmpl.Clear();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ads full admobmyLow initFull ex=" + ex.ToString());
            }
        }

        private void OnDestroy()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdsAdmobMyLowBridge.onInterstitialLoaded -= OnInterstitialLoadedEvent;
            AdsAdmobMyLowBridge.onInterstitialLoadFail -= OnInterstitialFailedEvent;
            AdsAdmobMyLowBridge.onInterstitialShowed -= OnInterstitialDisplayedEvent;
            AdsAdmobMyLowBridge.onInterstitialImpresstion -= OnInterstitialImpresstionEvent;
            AdsAdmobMyLowBridge.onInterstitialClick -= OnInterstitialClickEvent;
            AdsAdmobMyLowBridge.onInterstitialFailedToShow -= onInterstitialFailedToShow;
            AdsAdmobMyLowBridge.onInterstitialDismissed -= OnInterstitialDismissedEvent;
            AdsAdmobMyLowBridge.onInterstitialFinishShow -= OnInterstitialFinishShowEvent;
            AdsAdmobMyLowBridge.onInterstitialPaid -= OnInterstitialAdPaidEvent;
#endif
        }

        public override string getname()
        {
            return "adsmobMyLow";
        }

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
        public override void clearCurrFull(string placement)
        {
            if (getFullLoaded(placement) == 1)
            {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
                AdsAdmobMyLowBridge.Instance.clearCurrFull(placement);
                AdPlacementFull adpl = getPlFull(placement, true);
                if (adpl != null)
                {
                    adpl.isloaded = false;
                    adpl.isAdHigh = false;
                }
#endif
            }
        }
        public override int getFullLoaded(string placement)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlFull(placement, true);
            if (adpl == null)
            {
                SdkUtil.logd($"ads full {placement} admobmyLow getFullLoaded not pl");
                return 0;
            }
            else
            {
                //SdkUtil.logd($"ads full {placement}-{adpl.placement} admobmyLow getFullLoaded={adpl.isloaded}");
                if (adpl.isloaded)
                {
                    if (adpl.adECPM.idxHighPriority >= adpl.adECPM.idxCurrEcpm && adpl.isAdHigh)
                    {
                        return 2;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }
#endif
            return 0;
        }
        protected override void tryLoadFull(AdPlacementFull adpl)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
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
                SdkUtil.logd($"ads full {adpl.loadPl}-{adpl.placement} admobmyLow tryLoadFull over try");
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }

                return;
            }
            if (idLoad != null && idLoad.Contains("ca-app-pub"))
            {
                SdkUtil.logd($"ads full {adpl.loadPl}-{adpl.placement} admobmyLow tryLoadFull id={idLoad} idxCurrEcpmFull={adpl.adECPM.idxCurrEcpm} isFullHigh={adpl.isAdHigh}");
                AdsHelper.onAdLoad(adpl.loadPl, "interstitial", idLoad, "admob");
                FIRhelper.logAdEvent("ads_fulllow_load");
                AppsFlyerHelperScript.logAdEvent("ads_load", "", "interstitial", idLoad, "admob", "");
                adpl.isLoading = true;
                adpl.isloaded = false;
                AdsAdmobMyLowBridge.Instance.loadFull(adpl.placement, idLoad, adpl.adECPM.idxCurrEcpm);
            }
            else
            {
                SdkUtil.logd($"ads full {adpl.loadPl}-{adpl.placement} admobmyLow tryLoadFull id not correct");
                adpl.isLoading = false;
                adpl.isloaded = false;
            }
#else
            SdkUtil.logd($"ads full admobmyLow tryLoadFull not enable");
            var tmpcb = adpl.cbLoad;
            adpl.cbLoad = null;
            tmpcb(AD_State.AD_LOAD_FAIL);
#endif
        }
        public override void loadFull(string placement, AdCallBack cb)
        {
            AdPlacementFull adpl = getPlFull(placement, false);
            if (adpl == null)
            {
                SdkUtil.logd($"ads full {placement} admobmyLow loadFull type={adsType} not pl");
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
                    adpl.cbLoad = cb;
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadFull(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads full {placement}-{adpl.placement} admobmyLow loadFull isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                }
            }
        }
        public override bool showFull(string placement, float timeDelay, bool isShow2, AdCallBack cb)
        {
            isFull2 = isShow2;
            AdPlacementFull adpl = getPlFull(placement, true);
            if (adpl != null)
            {
                adpl.cbShow = null;
                int ss = getFullLoaded(adpl.placement);
                if (ss > 0)
                {
                    SdkUtil.logd($"ads full {placement}-{adpl.placement} admobmyLow showFull type={adsType} timeDelay={timeDelay}");
                    adpl.countLoad = 0;
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    if (timeDelay > 0)
                    {
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            AdsHelper.onAdShowStart(placement, "interstitial", "admob", "");
                            AdsAdmobMyLowBridge.Instance.showFull(adpl.placement, 0);
                        }, timeDelay);
                        return true;
                    }
                    else
                    {
                        AdsHelper.onAdShowStart(placement, "interstitial", "admob", "");
                        return AdsAdmobMyLowBridge.Instance.showFull(adpl.placement, 0);
                    }
#endif
                }
                else
                {
                    SdkUtil.logd($"ads full {placement}-{adpl.placement} admobmyLow showFull type={adsType} not loaded");
                }
            }
            else
            {

                SdkUtil.logd($"ads full {placement} admobmyLow showFull type={adsType} not pl");
            }
            return false;
        }

        //
        public override void clearCurrGift(string placement)
        {
            if (getGiftLoaded(placement) == 1)
            {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
#endif
            }
        }
        public override int getGiftLoaded(string placement)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY

#endif
            return 0;
        }
        protected override void tryloadGift(AdPlacementFull adpl)
        {
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads gift {adpl.loadPl}-{adpl.placement} admobmy tryloadGift not enable");
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
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY

        #region INTERSTITIAL AD EVENTS
        private void OnInterstitialLoadedEvent(string placement, string adsId, string adnet)
        {
            FIRhelper.logAdEvent("ads_fulllow_load_1");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "interstitial", adsId, "admob", "1");
            if (dicPLFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFull[placement];
                SdkUtil.logd($"ads full {adpl.loadPl}-{placement} admobmyLow HandleInterstitialLoaded");
                string adsource = FIRhelper.getAdsourceAdmob(adnet);
                AdsHelper.onAdLoadResult(adpl.loadPl, "interstitial", adsId, "admob", adsource, true);
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
                SdkUtil.logd($"ads full {placement} admobmyLow HandleInterstitialLoaded not pl");
            }
        }
        private void OnInterstitialFailedEvent(string placement, string adsId, string err)
        {
            FIRhelper.logAdEvent("ads_fulllow_load_0");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "interstitial", adsId, "admob", "0");
            if (dicPLFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFull[placement];
                SdkUtil.logd($"ads full {adpl.loadPl}-{placement} admobmyLow HandleInterstitialFailedToLoad=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "interstitial", adsId, "admob", "", false);
                adpl.isLoading = false;
                adpl.isloaded = false;
                if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                {
                    adpl.adECPM.idxCurrEcpm++;
                    tryLoadFull(adpl);
                }
                else
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        adpl.countLoad++;
                        tryLoadFull(adpl);
                    }, 1.0f);
                }
            }
            else
            {
                SdkUtil.logd($"ads full {placement} admobmyLow not pl HandleInterstitialFailedToLoad=" + err);
            }
        }
        private void OnInterstitialDisplayedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFull[placement];
                SdkUtil.logd($"ads full {adpl.showPl}-{placement} admobmyLow HandleInterstitialOpened");
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads full {placement} admobmyLow HandleInterstitialOpened not pl");
            }
        }
        private void OnInterstitialImpresstionEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads full {placement} admobmyLow OnInterstitialImpresstionEvent");
        }
        private void OnInterstitialClickEvent(string placement, string adsId, string adnet)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFull[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdClick(spl, "interstitial", "admob", adsource, adsId);
            if (!fullowIsClick)
            {
                fullowIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "interstitial", adsId, "admob", "");
            }
        }
        private void onInterstitialFailedToShow(string placement, string adsId, string adnet, string err)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFull[placement];
                SdkUtil.logd($"ads full {adpl.showPl}-{placement} admobmyLow onInterstitialFailedToShow=" + err);
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
                SdkUtil.logd($"ads full {placement} admobmyLow onInterstitialFailedToShow dic not contain pl");
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "interstitial", "admob", adsource, adsId, false, err);
            onFullClose(placement);
            advhelper.onCloseFullGift(true);
        }
        private void OnInterstitialDismissedEvent(string placement, string adsId, string adnet)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFull[placement];
                SdkUtil.logd($"ads full {adpl.showPl}-{placement} admobmyLow OnInterstitialDismissedEvent");
                adpl.isloaded = false;
                adpl.isLoading = false;
                spl = adpl.showPl;
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });
                }
                adpl.cbShow = null;
            }
            else
            {
                SdkUtil.logd($"ads full {placement} admobmyLow OnInterstitialDismissedEvent not pl");
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "interstitial", "admob", adsource, adsId, true, "");
            onFullClose(placement);
            dicPLFull[placement].countLoad = 0;
            advhelper.onCloseFullGift(true);
        }
        private void OnInterstitialFinishShowEvent(string placement, string adsId, string err)
        {
            //advhelper.onCloseFullGift(true);
        }
        private void OnInterstitialAdPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (!isFull2)
            {
                FIRhelper.logEvent("show_ads_total_imp");
                FIRhelper.logEvent("show_ads_full_imp");
                FIRhelper.logEvent("show_ads_full_imp_10");
            }
            fullowIsClick = false;
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFull[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adformat = FIRhelper.getAdformatAdmob(4);
            string adsource = FIRhelper.getAdsourceAdmob(adNet);
            var dicpr = TiktokBusiness.getAdmobParam(currencyCode);

            FIRhelper.logEventAdsPaidAdmob(spl, adformat, adsource, adsId, valueMicros, valueMicros, dicpr["currency_code"]);
            TiktokBusiness.logAdRevenueAdmob(adformat, adsource, adsId, precisionType, valueMicros / 1000, dicpr);
            float realValue = ((float)valueMicros) / 1000000000.0f;
            AdsHelper.onAdImpression(spl, adsId, adformat, "admob", adsource, realValue, valueMicros);

            FIRhelper.logAdEvent("ads_fulllow_imp");
            AppsFlyerHelperScript.logAdEvent("ads_impression", spl, "interstitial", adsId, "admob", "");
        }
        #endregion

#endif

    }
}