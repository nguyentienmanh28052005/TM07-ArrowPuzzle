using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mygame.sdk
{
    partial class AdsAdmobMy
    {
        private void initFullRwRw()
        {
            if (adsType != 0)
            {
                return;
            }
            try
            {
                Debug.Log("mysdk: ads fullrwrw admobmy init adCfPlacementFullRwRw=" + advhelper.currConfig.adCfPlacementFullRwRw);
                if (advhelper.currConfig.adCfPlacementFullRwRw.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementFullRwRw.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementFull>(dicPLFullRwRw, plitem, true);
                    }
                }
                string[] listpldf = AdIdsConfig.AdmobPlFullRwRw.Split(new char[] { '#' });
                foreach (string plitem in listpldf)
                {
                    addAdPlacement<AdPlacementFull>(dicPLFullRwRw, plitem, false);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: fullrwrwadmobmy initFullRwRw ex=" + ex.ToString());
            }
        }

        //full Rw Rw
        protected override void tryLoadFullRwRw(AdPlacementFull adpl)
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
                SdkUtil.logd($"ads fullrwrw {adpl.loadPl}-{adpl.placement} admobmy tryLoadFullRwRw over try");
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
                SdkUtil.logd($"ads fullrwrw {adpl.loadPl}-{adpl.placement} admobmy tryLoadFullRwRw id={idLoad} idxCurrEcpmFull={adpl.adECPM.idxCurrEcpm} isFullHigh={adpl.isAdHigh}");
                AdsHelper.onAdLoad(adpl.loadPl, "rewarded", idLoad, "admob");
                adpl.isLoading = true;
                adpl.isloaded = false;
                AdsAdmobMyBridge.Instance.loadFullRwRw(adpl.placement, idLoad);
            }
            else
            {
                SdkUtil.logd($"rwrw {adpl.loadPl}-{adpl.placement} admobmy tryLoadFullRwRw id not correct");
                adpl.isLoading = false;
                adpl.isloaded = false;
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads fullrwrw {adpl.placement} admobmy tryLoadFullRwRw not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadFullRwRw(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlFullRwRw(placement, false);
            if (adpl == null)
            {
                SdkUtil.logd($"ads fullrwrw {placement} admobmy loadFullRwRw not placement");
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
                    SdkUtil.logd($"ads fullrwrw {placement}-{adpl.placement} admobmy loadFullRwRw");
                    adpl.cbLoad = cb;
                    fullRwRwisnew = false;
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadFullRwRw(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads fullrwrw {placement}-{adpl.placement} admobmy loadFullRwRw isloading={adpl.isLoading} or isloaded={adpl.isloaded} showing={adpl.getShowing()}");
                }
            }
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showFullRwRw(string placement, float timeDelay, bool isShow2, AdCallBack cb)
        {
            isFull2 = isShow2;
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlFullRwRw(placement, true);
            if (adpl != null)
            {
                //adpl.cbShow = null;
                int ss = getFullRwRwLoaded(adpl.placement);
                if (ss > 0 && !adpl.getShowing())
                {
                    SdkUtil.logd($"ads fullrwrw {placement} admobmy showFullRwRw timeDelay={timeDelay}");
                    adpl.countLoad = 0;
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    if (timeDelay > 0)
                    {
                        adpl.setShowing(true);
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            AdsHelper.onAdShowStart(placement, "rewarded", "admob", "");
                            bool iss = AdsAdmobMyBridge.Instance.showFullRwRw(adpl.placement);
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
                        AdsHelper.onAdShowStart(placement, "rewarded", "admob", "");
                        bool iss = AdsAdmobMyBridge.Instance.showFullRwRw(adpl.placement);
                        adpl.setShowing(iss);
                        return iss;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads fullrwrw {placement} admobmy showFullRwRw not load or showing={adpl.getShowing()}");
                }
            }
            else
            {
                SdkUtil.logd($"ads fullrwrw {placement} admobmy showFullRwRw not pl");
            }
#endif
            return false;
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
        #region Full REWARDED VIDEO AD EVENTS
        private void OnInterRwRwLoadedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLFullRwRw.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFullRwRw[placement];
                SdkUtil.logd($"ads fullrwrw {adpl.loadPl}-{placement} admobmy OnInterRwRwLoadedEvent");
                string adsource = FIRhelper.getAdsourceAdmob(adnet);
                AdsHelper.onAdLoadResult(adpl.loadPl, "rewarded", adsId, "admob", adsource, true);
                adpl.countLoad = 0;
                adpl.isLoading = false;
                adpl.isloaded = true;
                fullRwRwisnew = false;
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
            }
            else
            {
                SdkUtil.logd($"ads fullrwrw {placement} admobmy OnInterRwRwLoadedEvent not pl");
            }
        }
        private void OnInterRwRwLoadFailEvent(string placement, string adsId, string err)
        {
            if (dicPLFullRwRw.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFullRwRw[placement];
                SdkUtil.logd($"ads fullrwrw {adpl.loadPl}-{placement} admobmy OnInterRwRwLoadFailEvent=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "rewarded", adsId, "admob", "", false);
                adpl.isLoading = false;
                adpl.isloaded = false;
                if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                {
                    if (fullRwRwisnew)
                    {
                        fullRwRwisnew = false;
                        adpl.adECPM.idxCurrEcpm = 0;
                    }
                    else
                    {
                        adpl.adECPM.idxCurrEcpm++;
                    }
                    tryLoadFullRwRw(adpl);
                }
                else
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        adpl.countLoad++;
                        tryLoadFullRwRw(adpl);
                    }, 1.0f);
                }
            }
            else
            {
                SdkUtil.logd($"ads fullrwrw {placement} admobmy not pl OnInterRwRwLoadFailEvent=" + err);
            }
        }
        private void OnInterRwRwFailedToShowEvent(string placement, string adsId, string adnet, string err)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFullRwRw.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFullRwRw[placement];
                SdkUtil.logd($"ads fullrwrw {adpl.showPl}-{placement} admobmy OnInterRwRwFailedToShowEvent=" + err);
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
                SdkUtil.logd($"ads fullrwrw {placement} admobmy OnInterRwRwFailedToShowEvent dic not pl");
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "rewarded", "admob", adsource, adsId, false, err);
            onFullClose(placement);
            advhelper.onCloseFullGift(true);
        }
        private void OnInterRwRwShowedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLFullRwRw.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFullRwRw[placement];
                SdkUtil.logd($"ads fullrwrw {adpl.showPl}-{placement} admobmy OnInterRwRwShowedEvent");
                adpl.countLoad = 0;
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads fullrwrw {placement} admobmy OnInterRwRwShowedEvent not pl");
            }
        }
        private void OnInterRwRwImpresstionEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads fullrwrw {placement} admobmy OnInterRwRwImpresstionEvent");
        }
        private void OnInterRwRwClickEvent(string placement, string adsId, string adnet)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFullRwRw.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFullRwRw[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdClick(spl, "rewarded", "admob", adsource, adsId);
        }
        private void OnInterRwRwRewardEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads fullrwrw admobmy OnInterRwRwRewardEvent");
            isFullRewardCom = true;
        }
        private void OnInterRwRwDismissedEvent(string placement, string adsId, string adnet)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFullRwRw.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFullRwRw[placement];
                SdkUtil.logd($"ads fullrwrw {adpl.showPl}-{placement} admobmy OnInterRwRwDismissedEvent");
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
                SdkUtil.logd($"ads fullrwrw {placement} admobmy OnInterRwRwDismissedEvent not pl");
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "rewarded", "admob", adsource, adsId, true, "");
            onFullClose(placement);
            isFullRewardCom = false;
            advhelper.onCloseFullGift(true);
        }
        private void OnInterRwRwFinishShowEvent(string placement, string adsId, string err)
        {
            //advhelper.onCloseFullGift(true);
        }
        private void OnInterRwRwPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            FIRhelper.logEvent("show_ads_total_imp");
            FIRhelper.logEvent("show_ads_full_rwrw_imp_0");
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFullRwRw.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLFullRwRw[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adformat = FIRhelper.getAdformatAdmob(5);
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