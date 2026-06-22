using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mygame.sdk
{
    partial class AdsAdmobMy
    {
        bool openIsClick = false;
        private void initOpenAd()
        {
            if (adsType != 0)
            {
                return;
            }
            try
            {
                Debug.Log("mysdk: ads full admobmy init adCfPlacementOpen=" + advhelper.currConfig.adCfPlacementOpen);
                if (advhelper.currConfig.adCfPlacementOpen.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementOpen.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementFull>(dicPLOpenAd, plitem, true);
                    }
                }
                string[] listpldf = AdIdsConfig.AdmobPlOpenAd.Split(new char[] { '#' });
                foreach (string plitem in listpldf)
                {
                    addAdPlacement<AdPlacementFull>(dicPLOpenAd, plitem, false);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ads full openad admobmy initopenad ex=" + ex.ToString());
            }
        }
        public override int getOpenAdLoaded(string placement)
        {
            AdPlacementFull adpl = getPlOpenAd(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads full openad {placement} admobmy getOpenAdLoaded not pl");
                return 0;
            }
            int re = 0;

            if (adpl.isloaded)
            {
                re = 1;
            }
            else
            {
                SdkUtil.logd($"ads full openad {placement} admobmy getOpenAdLoaded={adpl.isloaded}");
            }

            return re;
        }

        protected override void tryLoadOpenAd(AdPlacementFull adpl)
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
            SdkUtil.logd($"ads full openad {adpl.loadPl}-{adpl.placement} admobmy tryLoadOpenAd id={idLoad} idxCurrEcpm={adpl.adECPM.idxCurrEcpm} isHigh={adpl.isAdHigh}");

            int tryload = adpl.countLoad;
            if (tryload >= toTryLoad)
            {
                SdkUtil.logd($"ads full openad {adpl.loadPl}-{adpl.placement} admobmy tryLoadOpenAd over try");
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
                adpl.isLoading = true;
                adpl.isloaded = false;
                AdsHelper.onAdLoad(adpl.loadPl, "openad", idLoad, "admob");
                AdsAdmobMyBridge.Instance.loadOpenAd(adpl.placement, idLoad);
            }
            else
            {
                SdkUtil.logd($"ads full openad {adpl.loadPl}-{adpl.placement} admobmy tryLoadOpenAd id not correct");
                adpl.isLoading = false;
                adpl.isloaded = false;
            }
#else
            SdkUtil.logd($"ads full openad {adpl.loadPl}-{adpl.placement} admobmy tryLoadOpenAd not enable");
            var tmpcb = adpl.cbLoad;
            adpl.cbLoad = null;
            tmpcb(AD_State.AD_LOAD_FAIL);
#endif
        }
        public override void loadOpenAd(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            SdkUtil.logd($"ads full openad {placement} admobmy loadOpenAd type={adsType}");
            AdPlacementFull adpl = getPlOpenAd(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads full openad {placement} admobmy loadOpenAd not placement");
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
                    SdkUtil.logd($"ads full openad {placement}-{adpl.placement} admobmy loadOpenAd");
                    adpl.cbLoad = cb;
                    openadisnew = false;
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadOpenAd(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads full openad {placement}-{adpl.placement} admobmy loadOpenAd isloading={adpl.isLoading} or isloaded={adpl.isloaded} showing={adpl.getShowing()}");
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
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlOpenAd(placement);
            if (adpl != null)
            {
                //adpl.cbShow = null;
                int ss = getOpenAdLoaded(adpl.placement);
                if (ss > 0 && !adpl.getShowing())
                {
                    SdkUtil.logd($"ads full openad {placement}-{adpl.placement} admobmy showOpenAd isDelay={isDelay} tdelay={tdelay}");
                    adpl.countLoad = 0;
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    if (timeDelay > 0)
                    {
                        adpl.setShowing(true);
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            AdsHelper.onAdShowStart(placement, "openad", "admob", "");
                            bool iss = AdsAdmobMyBridge.Instance.showOpenAd(adpl.placement);
                            if (!iss)
                            {
                                adpl.setShowing(false);
                                if (cb != null)
                                {
                                    cb(AD_State.AD_SHOW_FAIL);
                                }
                            }
                            SDKManager.Instance.closeWaitShowFull();
                        }, tdelay);
                        return true;
                    }
                    else
                    {
                        AdsHelper.onAdShowStart(placement, "openad", "admob", "");
                        bool iss = AdsAdmobMyBridge.Instance.showOpenAd(adpl.placement);
                        adpl.setShowing(iss);
                        return iss;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads full openad {placement} admobmy showOpenAd type={adsType} not loaded or showing={adpl.getShowing()}");
                }
            }
            else
            {
                SdkUtil.logd($"ads full openad {placement} admobmy showOpenAd type={adsType} not pl");
            }
#endif
            return false;
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
        #region Open AD EVENTS
        private void OnOpenAdLoadedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLOpenAd.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLOpenAd[placement];
                SdkUtil.logd($"ads full openad {adpl.loadPl}-{placement} admobmy OnOpenAdLoadedEvent");
                string adsource = FIRhelper.getAdsourceAdmob(adnet);
                AdsHelper.onAdLoadResult(adpl.loadPl, "openad", adsId, "admob", adsource, true);
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
                SdkUtil.logd($"ads full openad {placement} admobmy OnOpenAdLoadedEvent not pl");
            }
        }
        private void OnOpenAdFailedEvent(string placement, string adsId, string err)
        {
            if (dicPLOpenAd.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLOpenAd[placement];
                SdkUtil.logd($"ads full openad {adpl.loadPl}-{placement} admobmy OnOpenAdFailedEvent=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "openad", adsId, "admob", "", false);
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
                SdkUtil.logd($"ads full openad {placement} admobmy OnOpenAdFailedEvent not pl err={err}");
            }
        }
        private void OnOpenAdDisplayedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLOpenAd.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLOpenAd[placement];
                SdkUtil.logd($"ads full openad {adpl.showPl}-{placement} admobmy OnOpenAdDisplayedEvent");
                if (dicPLOpenAd[placement].cbShow != null)
                {
                    AdCallBack tmpcb = dicPLOpenAd[placement].cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads full openad {placement} admobmy OnOpenAdDisplayedEvent not pl");
            }
        }
        private void onOpenAdImpresstionEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads full openad {placement} admobmy onOpenAdImpresstionEvent");
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
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdClick(spl, "openad", "admob", adsource, adsId);
            if (!openIsClick)
            {
                openIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "openad", adsId, "admob", "");
            }
        }
        private void onOpenAdFailedToShow(string placement, string adsId, string adnet, string err)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLOpenAd.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLOpenAd[placement];
                SdkUtil.logd($"ads full openad {adpl.showPl}-{placement} admobmy onOpenAdFailedToShow=" + err);
                adpl.isloaded = false;
                adpl.isLoading = false;
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
                SdkUtil.logd($"ads full openad {placement} admobmy onOpenAdFailedToShow dic not contain pl");
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "opened", "admob", adsource, adsId, false, err);
            onFullClose(placement);
        }
        private void OnOpenAdDismissedEvent(string placement, string adsId, string adnet)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLOpenAd.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLOpenAd[placement];
                SdkUtil.logd($"ads full openad {adpl.showPl}-{placement} admobmy OnOpenAdDismissedEvent");
                adpl.isloaded = false;
                adpl.isLoading = false;
                adpl.setShowing(false);
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
                SdkUtil.logd($"ads full openad {placement} admobmy OnOpenAdDismissedEvent not pl");
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "opened", "admob", adsource, adsId, true, "");
            onFullClose(placement);
        }
        private void OnOpenAdAdPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (!isOpenAd2)
            {
                FIRhelper.logEvent("show_ads_total_imp");
                FIRhelper.logEvent("show_ads_full_imp");
                FIRhelper.logEvent("show_ads_full_imp_0_oad");
            }
            openIsClick = false;
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
            string adformat = FIRhelper.getAdformatAdmob(7);
            string adsource = FIRhelper.getAdsourceAdmob(adNet);
            var dicpr = TiktokBusiness.getAdmobParam(currencyCode);
            FIRhelper.logEventAdsPaidAdmob(spl, adformat, adsource, adsId, valueMicros, valueMicros, dicpr["currency_code"]);
            TiktokBusiness.logAdRevenueAdmob(adformat, adsource, adsId, precisionType, valueMicros / 1000, dicpr);
            float realValue = ((float)valueMicros) / 1000000000.0f;
            AdsHelper.onAdImpression(spl, adsId, adformat, "admob", adsource, realValue, valueMicros);
        }
        #endregion
#endif
    }
}