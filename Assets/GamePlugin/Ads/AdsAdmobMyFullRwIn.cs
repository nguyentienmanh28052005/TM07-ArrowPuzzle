using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mygame.sdk
{
    partial class AdsAdmobMy
    {
        private bool isFullRewardCom = false;

        private void initFullRwInter()
        {
            if (adsType != 0)
            {
                return;
            }
            try
            {
                Debug.Log("mysdk: ads fullrwin admobmy init adCfPlacementFullRwInter=" + advhelper.currConfig.adCfPlacementFullRwInter);
                if (advhelper.currConfig.adCfPlacementFullRwInter.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementFullRwInter.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementFull>(dicPLFullRwInter, plitem, true);
                    }
                }
                string[] listpldf = AdIdsConfig.AdmobPlFullRwInter.Split(new char[] { '#' });
                foreach (string plitem in listpldf)
                {
                    addAdPlacement<AdPlacementFull>(dicPLFullRwInter, plitem, false);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ads fullrwin admobmy initFullRwInter ex=" + ex.ToString());
            }
        }
        
        //full Rw Inter
        protected override void tryLoadFullRwInter(AdPlacementFull adpl)
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
                SdkUtil.logd($"ads fullrwin {adpl.loadPl}-{adpl.placement} admobmy tryLoadFullRwInter over try");
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
                SdkUtil.logd($"ads fullrwin {adpl.loadPl}-{adpl.placement} admobmy tryLoadFullRwInter id={idLoad} idxCurrEcpmFull={adpl.adECPM.idxCurrEcpm} isFullHigh={adpl.isAdHigh}");
                AdsHelper.onAdLoad(adpl.loadPl, "rewarded_interstitial", idLoad, "admob");
                adpl.isLoading = true;
                adpl.isloaded = false;
                AdsAdmobMyBridge.Instance.loadFullRwInter(adpl.placement, idLoad);
            }
            else
            {
                SdkUtil.logd($"ads fullrwin {adpl.loadPl}-{adpl.placement} admobmy tryLoadFullRwInter id not correct");
                adpl.isLoading = false;
                adpl.isloaded = false;
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads fullrwin {adpl.placement} admobmy tryLoadFullRwInter not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadFullRwInter(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlFullRwInter(placement, false);
            if (adpl == null)
            {
                SdkUtil.logd($"ads fullrwin {placement} admobmy loadFullRwInter not placement");
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
                    SdkUtil.logd($"ads fullrwin {placement}-{adpl.placement} admobmy loadFullRwInter");
                    adpl.cbLoad = cb;
                    fullRwInterisnew = false;
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadFullRwInter(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads fullrwin {placement}-{adpl.placement} admobmy loadFullRwInter isloading={adpl.isLoading} or isloaded={adpl.isloaded} showing={adpl.getShowing()}");
                }
            }
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showFullRwInter(string placement, float timeDelay, bool isShow2, AdCallBack cb)
        {
            isFull2 = isShow2;
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlFullRwInter(placement, true);
            if (adpl != null)
            {
                //adpl.cbShow = null;
                int ss = getFullRwInterLoaded(adpl.placement);
                if (ss > 0 && !adpl.getShowing())
                {
                    SdkUtil.logd($"ads fullrwin {placement} admobmy showFullRwInter timeDelay={timeDelay}");
                    adpl.countLoad = 0;
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    if (timeDelay > 0)
                    {
                        adpl.setShowing(true);
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            AdsHelper.onAdShowStart(placement, "rewarded_interstitial", "admob", "");
                            bool iss = AdsAdmobMyBridge.Instance.showFullRwInter(adpl.placement);
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
                        AdsHelper.onAdShowStart(placement, "rewarded_interstitial", "admob", "");
                        bool iss = AdsAdmobMyBridge.Instance.showFullRwInter(adpl.placement);
                        adpl.setShowing(iss);
                        return iss;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads fullrwin {placement} admobmy showFullRwInter not load or showing={adpl.getShowing()}");
                }
            }
            else
            {
                SdkUtil.logd($"ads fullrwin {placement} admobmy showFullRwInter not pl");
            }
#endif
            return false;
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
        #region Full REWARDED Inter AD EVENTS
        private void OnInterRwInterLoadedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLFullRwInter.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFullRwInter[placement];
                SdkUtil.logd($"ads fullrwin {adpl.loadPl}-{placement} admobmy OnInterRwInterLoadedEvent");
                string adsource = FIRhelper.getAdsourceAdmob(adnet);
                AdsHelper.onAdLoadResult(adpl.loadPl, "rewarded_interstitial", adsId, "admob", adsource, true);
                adpl.countLoad = 0;
                adpl.isLoading = false;
                adpl.isloaded = true;
                fullRwInterisnew = false;
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
            }
            else
            {
                SdkUtil.logd($"ads fullrwin {placement} admobmy OnInterRwInterLoadedEvent not pl");
            }
        }
        private void OnInterRwInterLoadFailEvent(string placement, string adsId, string err)
        {
            if (dicPLFullRwInter.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFullRwInter[placement];
                SdkUtil.logd($"ads fullrwin {adpl.loadPl}-{placement} admobmy OnInterRwInterLoadFailEvent=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "rewarded_interstitial", adsId, "admob", "", false);
                adpl.isLoading = false;
                adpl.isloaded = false;
                if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                {
                    if (fullRwInterisnew)
                    {
                        fullRwInterisnew = false;
                        adpl.adECPM.idxCurrEcpm = 0;
                    }
                    else
                    {
                        adpl.adECPM.idxCurrEcpm++;
                    }
                    tryLoadFullRwInter(adpl);
                }
                else
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        adpl.countLoad++;
                        tryLoadFullRwInter(adpl);
                    }, 1.0f);
                }
            }
            else
            {
                SdkUtil.logd($"ads fullrwin {placement} admobmy not pl OnInterRwInterLoadFailEvent=" + err);
            }
        }
        private void OnInterRwInterFailedToShowEvent(string placement, string adsId, string adnet, string err)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFullRwInter.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFullRwInter[placement];
                SdkUtil.logd($"ads fullrwin {adpl.showPl}-{placement} admobmy OnInterRwInterFailedToShowEvent=" + err);
                adpl.isloaded = false;
                adpl.isLoading = false;
                adpl.setShowing(false);
                spl = adpl.showPl;
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_FAIL); });
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                }
            }
            else
            {
                SdkUtil.logd($"ads fullrwin {placement} admobmy OnInterRwInterFailedToShowEvent dic not pl");
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "rewarded_interstitial", "admob", adsource, adsId, false, err);
            onFullClose(placement);
            advhelper.onCloseFullGift(true);
        }
        private void OnInterRwInterShowedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLFullRwInter.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFullRwInter[placement];
                SdkUtil.logd($"ads fullrwin {adpl.showPl}-{placement} admobmy OnInterRwInterShowedEvent");
                adpl.countLoad = 0;
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads fullrwin {placement} admobmy OnInterRwInterShowedEvent not pl");
            }
        }
        private void OnInterRwInterImpresstionEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads fullrwin {placement} admobmy OnInterRwInterImpresstionEvent");
        }
        private void OnInterRwInterClickEvent(string placement, string adsId, string adnet)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFullRwInter.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFullRwInter[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdClick(spl, "rewarded_interstitial", "admob", adsource, adsId);
        }
        private void OnInterRwInterRewardEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads fullrwin admobmy OnInterRwInterRewardEvent");
            isFullRewardCom = true;
        }
        private void OnInterRwInterDismissedEvent(string placement, string adsId, string adnet)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFullRwInter.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFullRwInter[placement];
                SdkUtil.logd($"ads fullrwin {adpl.showPl}-{placement} admobmy OnInterRwInterDismissedEvent");
                adpl.isloaded = false;
                adpl.isLoading = false;
                adpl.setShowing(false);
                spl = adpl.showPl;
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    if (isFullRewardCom)
                    {
                        AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_OK); });
                    }
                    else
                    {
                        AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_FAIL); });
                    }
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });
                }
                adpl.cbShow = null;
                adpl.countLoad = 0;
            }
            else
            {
                SdkUtil.logd($"ads fullrwin {placement} admobmy OnInterRwInterDismissedEvent not pl");
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "rewarded_interstitial", "admob", adsource, adsId, true, "");
            onFullClose(placement);
            isFullRewardCom = false;
            advhelper.onCloseFullGift(true);
        }
        private void OnInterRwInterFinishShowEvent(string placement, string adsId, string err)
        {
            //advhelper.onCloseFullGift(true);
        }
        private void OnInterRwInterPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            FIRhelper.logEvent("show_ads_total_imp");
            FIRhelper.logEvent("show_ads_full_rwin_imp_0");
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFullRwInter.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFullRwInter[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adformat = FIRhelper.getAdformatAdmob(11);
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