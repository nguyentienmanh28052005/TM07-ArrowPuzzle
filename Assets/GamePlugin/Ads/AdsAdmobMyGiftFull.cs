using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mygame.sdk
{
    partial class AdsAdmobMy
    {
        bool giftFullIsClick = false;
        private void initGiftFull()
        {
            try
            {
                Debug.Log("mysdk: ads gift full admobmy initGiftFull adCfPlacementGiftFull=" + advhelper.currConfig.adCfPlacementGiftFull);
                if (advhelper.currConfig.adCfPlacementGiftFull.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementGiftFull.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementFull>(dicPLGiftFull, plitem, true);
                    }
                }
                string[] listpldf = AdIdsConfig.AdmobPlGiftFull.Split(new char[] { '#' });
                foreach (string plitem in listpldf)
                {
                    addAdPlacement<AdPlacementFull>(dicPLGiftFull, plitem, false);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ads gift full admobmy initGiftFull ex=" + ex.ToString());
            }

        }


        public override int getGiftFullLoaded(string placement)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlGiftFull(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads gift full {placement} admobmy getGiftFullLoaded not pl");
                return 0;
            }
            else
            {
                //SdkUtil.logd($"ads gift full {placement}-{adpl.placement} admobmy getGiftFullLoaded={adpl.isloaded}");
                if ((flagLoadAll & 2) == 0)
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
                        SdkUtil.logd($"ads gift full {placement}-{adpl.placement} admobmy getGiftFullLoaded={adpl.isloaded}");
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
        protected override void tryLoadGiftFull(AdPlacementFull adpl)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            if ((flagLoadAll & 2) == 0)
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
                    SdkUtil.logd($"ads gift full {adpl.loadPl}-{adpl.placement} admobmy tryLoadGiftFull over try");
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
                    SdkUtil.logd($"ads gift full {adpl.loadPl}-{adpl.placement} admobmy tryLoadGiftFull id={idLoad} idxCurrEcpm={adpl.adECPM.idxCurrEcpm} isFullHigh={adpl.isAdHigh}");
                    AdsHelper.onAdLoad(adpl.loadPl, "interstitial", idLoad, "admob");
                    FIRhelper.logAdEvent("ads_giftfull_load");
                    AppsFlyerHelperScript.logAdEvent("ads_load", "", "interstitial", idLoad, "admob", "");
                    adpl.isLoading = true;
                    adpl.isloaded = false;
                    if (timeDeltaLoad <= 0 || adpl.adECPM.idxCurrEcpm == 0)
                    {
                        AdsAdmobMyBridge.Instance.loadGiftFull(adpl.placement, idLoad, adpl.adECPM.idxCurrEcpm);
                    }
                    else
                    {
                        if (timeDeltaLoad > 30)
                        {
                            timeDeltaLoad = 30;
                        }
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            AdsAdmobMyBridge.Instance.loadGiftFull(adpl.placement, idLoad, adpl.adECPM.idxCurrEcpm);
                        }, timeDeltaLoad);
                    }
                }
                else
                {
                    SdkUtil.logd($"ads gift full {adpl.loadPl}-{adpl.placement} admobmy tryLoadGiftFull id not correct");
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
                        SdkUtil.logd($"ads gift full {adpl.loadPl}-{adpl.placement} admobmy tryLoadGiftFull={idLoad}, idxload={i}, isFullHigh={isGiftHigh}");
                        AdsHelper.onAdLoad(adpl.loadPl, "interstitial", idLoad, "admob");
                        FIRhelper.logAdEvent("ads_giftfull_load");
                        AppsFlyerHelperScript.logAdEvent("ads_load", "", "interstitial", idLoad, "admob", "");
                        adpl.adECPM.list[i].isLoading = true;
                        adpl.adECPM.list[i].isLoaded = false;
                        AdsAdmobMyBridge.Instance.loadGiftFull(adpl.placement, idLoad, i);
                    }
                }
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads gift full {adpl.placement} admobmy tryLoadGiftFull not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadGiftFull(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlGiftFull(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads gift full {placement} admobmy loadGiftFull not placement");
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
                    if ((flagLoadAll & 2) == 0)
                    {
                        if (!adpl.isloaded && !adpl.isLoading && !adpl.getShowing())
                        {
                            SdkUtil.logd($"ads gift full {placement}-{adpl.placement} admobmy loadGiftFull");
                            adpl.cbLoad = cb;
                            giftfullisnew = false;
                            adpl.countLoad = 0;
                            adpl.adECPM.idxCurrEcpm = 0;
                            adpl.setSetPlacementLoad(placement);
                            tryLoadGiftFull(adpl);
                        }
                        else
                        {
                            SdkUtil.logd($"ads gift full {placement}-{adpl.placement} admobmy loadGiftFull isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                        }
                    }
                    else
                    {
                        if (!adpl.isHighAdLoaded() && adpl.count4LoadAll <= 0)
                        {
                            SdkUtil.logd($"ads gift full {placement}-{adpl.placement} admobmy loadGiftFull all");
                            adpl.cbLoad = cb;
                            giftfullisnew = false;
                            adpl.count4LoadAll = 0;
                            adpl.adECPM.idxCurrEcpm = 0;
                            adpl.setSetPlacementLoad(placement);
                            tryLoadGiftFull(adpl);
                        }
                        else
                        {
                            SdkUtil.logd($"ads gift full {placement}-{adpl.placement} admobmy loadGiftFull isHighAdLoaded={adpl.isHighAdLoaded()} or count4LoadAll={adpl.count4LoadAll}");
                        }
                    }
                }
                else
                {
                    SdkUtil.logd($"ads gift full {placement}-{adpl.placement} admobmy loadGiftFull showing={adpl.getShowing()}");
                }
            }
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showGiftFull(string placement, float timeDelay, bool isShow2, AdCallBack cb)
        {
            isFull2 = isShow2;
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlGiftFull(placement);
            if (adpl != null)
            {
                //adpl.cbShow = null;
                int ss = getGiftFullLoaded(adpl.placement);
                if (ss > 0 && !adpl.getShowing())
                {
                    SdkUtil.logd($"ads gift full {placement} admobmy showGiftFull timeDelay={timeDelay}");
                    adpl.countLoad = 0;
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    if (timeDelay > 0)
                    {
                        adpl.setShowing(true);
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            AdsHelper.onAdShowStart(placement, "interstitial", "admob", "");
                            bool iss = AdsAdmobMyBridge.Instance.showGiftFull(adpl.placement);
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
                        bool iss = AdsAdmobMyBridge.Instance.showGiftFull(adpl.placement);
                        adpl.setShowing(iss);
                        return iss;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads gift full {placement} admobmy showGiftFull not load or showing={adpl.getShowing()}");
                }
            }
            else
            {
                SdkUtil.logd($"ads gift full {placement} admobmy showGiftFull not pl");
            }
#endif
            return false;
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
        #region Full AD EVENTS
        private void OnRwInterstitialLoadedEvent(string placement, string adsId, string adnet)
        {
            FIRhelper.logAdEvent("ads_giftfull_load_1");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "interstitial", adsId, "admob", "1");
            if (dicPLGiftFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLGiftFull[placement];
                SdkUtil.logd($"ads gift full {adpl.loadPl}-{placement} admobmy OnRwInterstitialLoadedEvent");
                string adsource = FIRhelper.getAdsourceAdmob(adnet);
                AdsHelper.onAdLoadResult(adpl.loadPl, "interstitial", adsId, "admob", adsource, true);
                
                adpl.countLoad = 0;
                adpl.isLoading = false;
                adpl.isloaded = true;
                giftfullisnew = false;
                if ((flagLoadAll & 2) != 0)
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
                SdkUtil.logd($"ads gift full {placement} admobmy OnRwInterstitialLoadedEvent not pl");
            }
        }
        private void OnRWInterstitialFailedEvent(string placement, string adsId, string err)
        {
            FIRhelper.logAdEvent("ads_giftfull_load_0");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "interstitial", adsId, "admob", "0");
            if (dicPLGiftFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLGiftFull[placement];
                SdkUtil.logd($"ads gift full {adpl.loadPl}-{placement} admobmy OnRWInterstitialFailedEvent=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "interstitial", adsId, "admob", "", false);

                if ((flagLoadAll & 2) == 0)
                {
                    adpl.isLoading = false;
                    adpl.isloaded = false;
                    if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                    {
                        if (giftfullisnew)
                        {
                            giftfullisnew = false;
                            adpl.adECPM.idxCurrEcpm = 0;
                        }
                        else
                        {
                            adpl.adECPM.idxCurrEcpm++;
                        }
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            tryLoadGiftFull(adpl);
                        }, 1);
                    }
                    else
                    {
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            adpl.countLoad++;
                            tryLoadGiftFull(adpl);
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
                SdkUtil.logd($"ads gift full {placement} admobmy not pl OnRWInterstitialFailedEvent=" + err);
            }
        }
        private void OnRWInterstitialDisplayedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLGiftFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLGiftFull[placement];
                SdkUtil.logd($"ads gift full {adpl.showPl}-{placement} admobmy OnRWInterstitialDisplayedEvent");
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads gift full {placement} admobmy OnRWInterstitialDisplayedEvent not pl");
            }
        }
        private void OnRWInterstitialImpresstionEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads gift full {placement} admobmy OnRWInterstitialImpresstionEvent");
        }
        private void OnRWInterstitialClickEvent(string placement, string adsId, string adnet)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLGiftFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLGiftFull[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdClick(spl, "interstitial", "admob", adsource, adsId);
            if (!giftFullIsClick)
            {
                giftFullIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "interstitial", adsId, "admob", "");
            }
        }
        private void onRWInterstitialFailedToShow(string placement, string adsId, string adnet, string err)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLGiftFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLGiftFull[placement];
                SdkUtil.logd($"ads gift full {adpl.showPl}-{placement} admobmy onRWInterstitialFailedToShow=" + err);
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
                SdkUtil.logd($"ads gift full {placement} admobmy onRWInterstitialFailedToShow not pl");
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "interstitial", "admob", adsource, adsId, false, err);
            onFullClose(placement);
            advhelper.onCloseFullGift(true);
        }
        private void OnRWInterstitialDismissedEvent(string placement, string adsId, string adnet)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLGiftFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLGiftFull[placement];
                SdkUtil.logd($"ads gift full {adpl.showPl}-{placement} admobmy OnRWInterstitialDismissedEvent id={adsId}");
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
                SdkUtil.logd($"ads gift full {placement} admobmy OnRWInterstitialDismissedEvent not pl");
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "interstitial", "admob", adsource, adsId, true, "");
            onFullClose(placement);
            advhelper.onCloseFullGift(true);
        }
        private void OnRWInterstitialFinishShowEvent(string placement, string adsId, string err)
        {
            //advhelper.onCloseFullGift(true);
        }
        private void OnRWInterstitialAdPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            FIRhelper.logEvent("show_ads_total_imp");
            FIRhelper.logEvent("show_ads_reward_imp");
            FIRhelper.logEvent("show_ads_reward_imp_full0");
            giftFullIsClick = false;
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLGiftFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLGiftFull[placement];
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

            FIRhelper.logAdEvent("ads_giftfull_imp");
            AppsFlyerHelperScript.logAdEvent("ads_impression", spl, "interstitial", adsId, "admob", "");
        }
        #endregion
#endif
    }
}