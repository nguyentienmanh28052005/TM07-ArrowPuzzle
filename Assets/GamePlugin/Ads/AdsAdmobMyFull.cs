using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mygame.sdk
{
    partial class AdsAdmobMy
    {
        bool fullIsClick = false;
        private void initFull()
        {
            if (adsType != 0)
            {
                return;
            }
            try
            {
                Debug.Log("mysdk: ads full admobmy init adCfPlacementFull=" + advhelper.currConfig.adCfPlacementFull);
                if (advhelper.currConfig.adCfPlacementFull.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementFull.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementFull>(dicPLFull, plitem, true);
                    }
                }
                string[] listpldf = AdIdsConfig.AdmobPlFullAll.Split(new char[] { '#' });
                foreach (string plitem in listpldf)
                {
                    addAdPlacement<AdPlacementFull>(dicPLFull, plitem, false);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ads full admobmy initFull ex=" + ex.ToString());
            }
        }

        //full
        public override void clearCurrFull(string placement)
        {
            if (getFullLoaded(placement) == 1)
            {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
                AdsAdmobMyBridge.Instance.clearCurrFull(placement);
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
                SdkUtil.logd($"ads full {placement} admobmy getFullLoaded not pl");
                return 0;
            }
            else
            {
                //SdkUtil.logd($"ads full {placement}-{adpl.placement} admobmy getFullLoaded={adpl.isloaded}");
                if ((flagLoadAll & 1) == 0)
                {
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
                    else
                    {
                        SdkUtil.logd($"ads full {placement}-{adpl.placement} admobmy getFullLoaded={adpl.isloaded}");
                    }
                }
                else
                {
                    for (int i = 0; i < adpl.adECPM.list.Count; i++)
                    {
                        if (adpl.adECPM.list[i].isLoaded)
                        {
                            if (i <= adpl.adECPM.idxHighPriority)
                            {
                                return 2;
                            }
                            else
                            {
                                return 1;
                            }
                        }
                    }
                }
            }
#endif
            return 0;
        }
        protected override void tryLoadFull(AdPlacementFull adpl)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            if ((flagLoadAll & 1) == 0)
            {
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
                    SdkUtil.logd($"ads full {adpl.loadPl}-{adpl.placement} admobmy tryLoadFull over try");
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
                    SdkUtil.logd($"ads full {adpl.loadPl}-{adpl.placement} admobmy tryLoadFull id={idLoad} idxCurrEcpmFull={adpl.adECPM.idxCurrEcpm} isFullHigh={adpl.isAdHigh}");
                    AdsHelper.onAdLoad(adpl.loadPl, "interstitial", idLoad, "admob");
                    FIRhelper.logAdEvent("ads_full_load");
                    AppsFlyerHelperScript.logAdEvent("ads_load", "", "interstitial", idLoad, "admob", "");
                    adpl.isLoading = true;
                    adpl.isloaded = false;
                    if (timeDeltaLoad <= 0 || adpl.adECPM.idxCurrEcpm == 0)
                    {
                        AdsAdmobMyBridge.Instance.loadFull(adpl.placement, idLoad, adpl.adECPM.idxCurrEcpm);
                    }
                    else
                    {
                        if (timeDeltaLoad > 30)
                        {
                            timeDeltaLoad = 30;
                        }
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            AdsAdmobMyBridge.Instance.loadFull(adpl.placement, idLoad, adpl.adECPM.idxCurrEcpm);
                        }, timeDeltaLoad);
                    }
                }
                else
                {
                    SdkUtil.logd($"ads full {adpl.loadPl}-{adpl.placement} admobmy tryLoadFull id not correct");
                    adpl.isLoading = false;
                    adpl.isloaded = false;
                }
            }
            else
            {
                for (int i = 0; i < adpl.adECPM.list.Count; i++)
                {
                    if (!adpl.adECPM.list[i].isLoaded && !adpl.adECPM.list[i].isLoading)
                    {
                        adpl.count4LoadAll++;
                        adpl.isAdHigh = false;
                        string idLoad = adpl.adECPM.list[i].adsId;
                        if (adpl.adECPM.idxHighPriority >= i)
                        {
                            adpl.isAdHigh = true;
                        }
                        SdkUtil.logd($"ads full {adpl.loadPl}-{adpl.placement} admobmy tryLoadFull={idLoad}, idxload={i}, isFullHigh={isGiftHigh}");
                        AdsHelper.onAdLoad(adpl.loadPl, "interstitial", idLoad, "admob");
                        FIRhelper.logAdEvent("ads_full_load");
                        AppsFlyerHelperScript.logAdEvent("ads_load", "", "interstitial", idLoad, "admob", "");
                        adpl.adECPM.list[i].isLoading = true;
                        adpl.adECPM.list[i].isLoaded = false;
                        AdsAdmobMyBridge.Instance.loadFull(adpl.placement, idLoad, i);
                    }
                }
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads full {adpl.placement} admobmy tryLoadFull not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadFull(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlFull(placement, false);
            if (adpl == null)
            {
                SdkUtil.logd($"ads full {placement} admobmy loadFull not placement");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            else
            {
                if (!adpl.getShowing())
                {
                    if ((flagLoadAll & 1) == 0)
                    {
                        if (!adpl.isloaded && !adpl.isLoading && !adpl.getShowing())
                        {
                            SdkUtil.logd($"ads full {placement}-{adpl.placement} admobmy loadFull");
                            adpl.cbLoad = cb;
                            fullisnew = false;
                            adpl.countLoad = 0;
                            adpl.adECPM.idxCurrEcpm = 0;
                            adpl.setSetPlacementLoad(placement);
                            tryLoadFull(adpl);
                        }
                        else
                        {
                            SdkUtil.logd($"ads full {placement}-{adpl.placement} admobmy loadFull isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                        }
                    }
                    else
                    {
                        if (!adpl.isHighAdLoaded() && adpl.count4LoadAll <= 0)
                        {
                            SdkUtil.logd($"ads full {placement}-{adpl.placement} admobmy loadFull all");
                            adpl.cbLoad = cb;
                            fullisnew = false;
                            adpl.count4LoadAll = 0;
                            adpl.adECPM.idxCurrEcpm = 0;
                            adpl.setSetPlacementLoad(placement);
                            tryLoadFull(adpl);
                        }
                        else
                        {
                            SdkUtil.logd($"ads full {placement}-{adpl.placement} admobmy loadFull isHighAdLoaded={adpl.isHighAdLoaded()} or count4LoadAll={adpl.count4LoadAll}");
                        }
                    }
                }
                else
                {
                    SdkUtil.logd($"ads full {placement}-{adpl.placement} admobmy loadFull showing={adpl.getShowing()}");
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
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlFull(placement, true);
            if (adpl != null)
            {
                //adpl.cbShow = null;
                int ss = getFullLoaded(adpl.placement);
                if (ss > 0 && !adpl.getShowing())
                {
                    SdkUtil.logd($"ads full {placement} admobmy showFull timeDelay={timeDelay}");
                    adpl.countLoad = 0;
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    if (timeDelay > 0)
                    {
                        adpl.setShowing(true);
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            AdsHelper.onAdShowStart(placement, "interstitial", "admob", "");
                            bool iss = AdsAdmobMyBridge.Instance.showFull(adpl.placement);
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
                        AdsHelper.onAdShowStart(placement, "interstitial", "admob", "");
                        bool iss = AdsAdmobMyBridge.Instance.showFull(adpl.placement);
                        adpl.setShowing(iss);
                        return iss;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads full {placement} admobmy showFull not load or showing={adpl.getShowing()}");
                }
            }
            else
            {
                SdkUtil.logd($"ads full {placement} admobmy showFull not pl");
            }
#endif
            return false;
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
        #region Full AD EVENTS
        private void OnInterstitialLoadedEvent(string placement, string adsId, string adnet)
        {
            FIRhelper.logAdEvent("ads_full_load_1");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "interstitial", adsId, "admob", "1");
            if (dicPLFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFull[placement];
                SdkUtil.logd($"ads full {adpl.loadPl}-{placement} admobmy OnInterstitialLoadedEvent");
                string adsource = FIRhelper.getAdsourceAdmob(adnet);
                AdsHelper.onAdLoadResult(adpl.loadPl, "interstitial", adsId, "admob", adsource, true);
                
                adpl.countLoad = 0;
                adpl.isLoading = false;
                adpl.isloaded = true;
                fullisnew = false;
                if ((flagLoadAll & 1) != 0)
                {
                    adpl.count4LoadAll--;
                    adpl.setStatusLoad(adsId, true);
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
                SdkUtil.logd($"ads full {placement} admobmy OnInterstitialLoadedEvent not pl");
            }
        }
        private void OnInterstitialFailedEvent(string placement, string adsId, string err)
        {
            FIRhelper.logAdEvent("ads_full_load_0");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "interstitial", adsId, "admob", "0");
            if (dicPLFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFull[placement];
                SdkUtil.logd($"ads full {adpl.loadPl}-{placement} admobmy OnInterstitialFailedEvent=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "interstitial", adsId, "admob", "", false);

                if ((flagLoadAll & 1) == 0)
                {
                    adpl.isLoading = false;
                    adpl.isloaded = false;
                    if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                    {
                        if (fullisnew)
                        {
                            fullisnew = false;
                            adpl.adECPM.idxCurrEcpm = 0;
                        }
                        else
                        {
                            adpl.adECPM.idxCurrEcpm++;
                        }
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            tryLoadFull(adpl);
                        }, 1);
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
                    adpl.count4LoadAll--;
                    adpl.setStatusLoad(adsId, false);

                    if (adpl.count4LoadAll <= 0)
                    {
                        if (adpl.cbLoad != null)
                        {
                            var tmpcb = adpl.cbLoad;
                            adpl.cbLoad = null;
                            tmpcb(AD_State.AD_LOAD_FAIL);
                        }
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads full {placement} admobmy not pl OnInterstitialFailedEvent=" + err);
            }
        }
        private void OnInterstitialDisplayedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFull[placement];
                SdkUtil.logd($"ads full {adpl.showPl}-{placement} admobmy OnInterstitialDisplayedEvent");
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads full {placement} admobmy OnInterstitialDisplayedEvent not pl");
            }
        }
        private void OnInterstitialImpresstionEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads full {placement} admobmy OnInterstitialImpresstionEvent");
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
            if (!fullIsClick)
            {
                fullIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "interstitial", adsId, "admob", "");
            }
        }
        private void onInterstitialFailedToShow(string placement, string adsId, string adnet, string err)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFull[placement];
                SdkUtil.logd($"ads full {adpl.showPl}-{placement} admobmy onInterstitialFailedToShow=" + err);
                adpl.isloaded = false;
                adpl.isLoading = false;
                spl = adpl.showPl;
                adpl.setShowing(false);
                adpl.setStatusLoad(adsId, false);
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                }
            }
            else
            {
                SdkUtil.logd($"ads full {placement} admobmy onInterstitialFailedToShow not pl");
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
                SdkUtil.logd($"ads full {adpl.showPl}-{placement} admobmy OnInterstitialDismissedEvent id={adsId}");
                adpl.isloaded = false;
                adpl.isLoading = false;
                adpl.setShowing(false);
                adpl.setStatusLoad(adsId, false);
                spl = adpl.showPl;
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });
                }
                adpl.cbShow = null;
                adpl.countLoad = 0;
            }
            else
            {
                SdkUtil.logd($"ads full {placement} admobmy OnInterstitialDismissedEvent not pl");
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "interstitial", "admob", adsource, adsId, true, "");
            onFullClose(placement);
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
                FIRhelper.logEvent("show_ads_full_imp_0");
            }
            fullIsClick = false;
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

            FIRhelper.logAdEvent("ads_full_imp");
            AppsFlyerHelperScript.logAdEvent("ads_impression", spl, "interstitial", adsId, "admob", "");
        }
        #endregion
#endif
    }
}