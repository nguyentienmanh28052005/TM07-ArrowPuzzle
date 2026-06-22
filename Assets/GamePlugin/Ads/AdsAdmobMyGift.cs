using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mygame.sdk
{
    partial class AdsAdmobMy
    {
        bool giftIsClick = false;
        private void initGift()
        {
            if (adsType != 0)
            {
                return;
            }
            try
            {
                Debug.Log("mysdk: ads gift admobmy init adCfPlacementGift=" + advhelper.currConfig.adCfPlacementGift);
                if (advhelper.currConfig.adCfPlacementGift.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementGift.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementFull>(dicPLGift, plitem, true);
                    }
                }
                string[] listpldf = AdIdsConfig.AdmobPlGift.Split(new char[] { '#' });
                foreach (string plitem in listpldf)
                {
                    addAdPlacement<AdPlacementFull>(dicPLGift, plitem, false);
                }
                foreach (var item in dicPLGift)
                {
                    string listads = item.Value.placement + ";";
                    foreach (var iids in item.Value.adECPM.list)
                    {
                        listads += iids.adsId + ",";
                    }
                    Debug.Log("mysdk: ads gift admobmy init pl=" + listads);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ads gift admobmy initGift ex=" + ex.ToString());
            }
        }

        public override void clearCurrGift(string placement)
        {
            if (getGiftLoaded(placement) == 1)
            {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
                AdsAdmobMyBridge.Instance.clearCurrGift(placement);
                AdPlacementFull adpl = getPlGift(placement);
                if (adpl != null)
                {
                    adpl.isloaded = false;
                    adpl.isAdHigh = false;
                }
#endif
            }
        }
        public override int getGiftLoaded(string placement)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlGift(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads gift {placement} admobmy getGiftLoaded not pl");
                return 0;
            }
            else
            {
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
                        SdkUtil.logd($"ads gift {placement}-{adpl.placement} admobmy getGiftLoaded={adpl.isloaded}");
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
        protected override void tryloadGift(AdPlacementFull adpl)
        {
            isGiftHigh = false;
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
                    SdkUtil.logd($"ads gift {adpl.loadPl}-{adpl.placement} admobmy tryloadGift over try");
                    if (adpl.cbLoad != null)
                    {
                        var tmpcb = adpl.cbLoad;
                        adpl.cbLoad = null;
                        tmpcb(AD_State.AD_LOAD_FAIL);
                    }
                    return;
                }
                SdkUtil.logd($"ads gift {adpl.loadPl}-{adpl.placement} admobmy tryloadGift =" + idLoad + ", idxCurrEcpmGift=" + adpl.adECPM.idxCurrEcpm + ", isGiftHigh=" + isGiftHigh);
                AdsHelper.onAdLoad(adpl.loadPl, "rewarded", idLoad, "admob");
                FIRhelper.logAdEvent("ads_gift_load");
                AppsFlyerHelperScript.logAdEvent("ads_load", "", "rewarded", idLoad, "admob", "");
                adpl.isLoading = true;
                adpl.isloaded = false;
                if (timeDeltaLoad <= 0 || adpl.adECPM.idxCurrEcpm == 0)
                {
                    AdsAdmobMyBridge.Instance.loadGift(adpl.placement, idLoad, adpl.adECPM.idxCurrEcpm);
                }
                else
                {
                    if (timeDeltaLoad > 30)
                    {
                        timeDeltaLoad = 30;
                    }
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        AdsAdmobMyBridge.Instance.loadGift(adpl.placement, idLoad, adpl.adECPM.idxCurrEcpm);
                    }, timeDeltaLoad);
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
                        SdkUtil.logd($"ads gift {adpl.loadPl}-{adpl.placement} admobmy tryloadGift =" + idLoad + ", idxload=" + i + ", isGiftHigh=" + isGiftHigh);
                        AdsHelper.onAdLoad(adpl.loadPl, "rewarded", idLoad, "admob");
                        FIRhelper.logAdEvent("ads_gift_load");
                        AppsFlyerHelperScript.logAdEvent("ads_load", "", "rewarded", idLoad, "admob", "");
                        adpl.adECPM.list[i].isLoading = true;
                        adpl.adECPM.list[i].isLoaded = false;
                        AdsAdmobMyBridge.Instance.loadGift(adpl.placement, idLoad, i);
                    }
                }
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads gift admobmy tryloadGift not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadGift(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlGift(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads gift {placement} admobmy loadGift not placement");
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
                    if ((flagLoadAll & 2) != 0)
                    {
                        if (!adpl.isHighAdLoaded() && adpl.count4LoadAll <= 0)
                        {
                            SdkUtil.logd($"ads gift {placement}-{adpl.placement} admobmy loadGift all");
                            adpl.cbLoad = cb;
                            giftisnew = false;
                            adpl.count4LoadAll = 0;
                            adpl.adECPM.idxCurrEcpm = 0;
                            adpl.setSetPlacementLoad(placement);
                            tryloadGift(adpl);
                        }
                        else
                        {
                            SdkUtil.logd($"ads gift {placement}-{adpl.placement} admobmy loadGift isHighAdLoaded={adpl.isHighAdLoaded()} or count4LoadAll={adpl.count4LoadAll}");
                        }
                    }
                    else
                    {
                        if (!adpl.isloaded && !adpl.isLoading && !adpl.getShowing())
                        {
                            SdkUtil.logd($"ads gift {placement}-{adpl.placement} admobmy loadGift");
                            adpl.cbLoad = cb;
                            giftisnew = false;
                            adpl.countLoad = 0;
                            adpl.adECPM.idxCurrEcpm = 0;
                            adpl.setSetPlacementLoad(placement);
                            tryloadGift(adpl);
                        }
                        else
                        {
                            SdkUtil.logd($"ads gift {placement}-{adpl.placement} admobmy loadGift isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                        }
                    }
                }
                else
                {
                    SdkUtil.logd($"ads gift {placement}-{adpl.placement} admobmy loadGift showing={adpl.getShowing()}");
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
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlGift(placement);
            if (adpl != null)
            {
                //adpl.cbShow = null;
                int ss = getGiftLoaded(adpl.placement);
                if (ss > 0 && !adpl.getShowing())
                {
                    SdkUtil.logd($"ads gift {placement}-{adpl.placement} admobmy showGift timeDelay={timeDelay}");
                    adpl.countLoad = 0;
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    if (timeDelay > 0)
                    {
                        adpl.setShowing(true);
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            AdsHelper.onAdShowStart(placement, "rewarded", "admob", "");
                            bool iss = AdsAdmobMyBridge.Instance.showGift(adpl.placement);
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
                        bool iss = AdsAdmobMyBridge.Instance.showGift(adpl.placement);
                        adpl.setShowing(iss);
                        return iss;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads gift {placement}-{adpl.placement} admobmy showGift not load or showing={adpl.getShowing()}");
                }
            }
            else
            {
                SdkUtil.logd($"ads gift {placement} admobmy showGift not pl");
            }
#endif
            return false;
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
        #region REWARDED VIDEO AD EVENTS
        private void OnRewardedAdLoadedEvent(string placement, string adsId, string adnet)
        {
            FIRhelper.logAdEvent("ads_gift_load_1");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "rewarded", adsId, "admob", "1");
            if (dicPLGift.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLGift[placement];
                SdkUtil.logd($"ads gift {adpl.loadPl}-{placement} admobmy OnRewardedAdLoadedEvent");
                string adsource = FIRhelper.getAdsourceAdmob(adnet);
                AdsHelper.onAdLoadResult(adpl.loadPl, "rewarded", adsId, "admob", adsource, true);
                adpl.countLoad = 0;
                adpl.isLoading = false;
                adpl.isloaded = true;
                giftisnew = false;
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
                SdkUtil.logd($"ads gift {placement} admobmy OnRewardedAdLoadedEvent not pl");
            }
        }
        private void OnRewardedAdFailedEvent(string placement, string adsId, string err)
        {
            FIRhelper.logAdEvent("ads_gift_load_0");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "rewarded", adsId, "admob", "0");
            if (dicPLGift.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLGift[placement];
                SdkUtil.logd($"ads gift {adpl.loadPl}-{placement} admobmy OnRewardedAdFailedEvent=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "rewarded", adsId, "admob", "", false);
                if ((flagLoadAll & 2) != 0)
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
                else
                {
                    adpl.isLoading = false;
                    adpl.isloaded = false;
                    if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                    {
                        if (giftisnew)
                        {
                            giftisnew = false;
                            adpl.adECPM.idxCurrEcpm = 0;
                        }
                        else
                        {
                            adpl.adECPM.idxCurrEcpm++;
                        }
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            tryloadGift(adpl);
                        }, 1);
                    }
                    else
                    {
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            adpl.countLoad++;
                            tryloadGift(adpl);
                        }, 1.0f);
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads gift {placement} admobmy not pl OnRewardedAdFailedEvent=" + err);
            }
        }
        private void OnRewardedAdFailedToDisplayEvent(string placement, string adsId, string adnet, string err)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLGift.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLGift[placement];
                SdkUtil.logd($"ads gift {adpl.showPl}-{placement} admobmy OnRewardedAdFailedToDisplayEvent=" + err);
                adpl.isloaded = false;
                adpl.isLoading = false;
                adpl.setShowing(false);
                adpl.setStatusLoad(adsId, false);
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
                SdkUtil.logd($"ads gift {placement} admobmy OnRewardedAdFailedToDisplayEvent dic not pl");
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "rewarded", "admob", adsource, adsId, false, err);
            onGiftClose(placement);
            advhelper.onCloseFullGift(false);
        }
        private void OnRewardedAdDisplayedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLGift.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLGift[placement];
                SdkUtil.logd($"ads gift {adpl.showPl}-{placement} admobmy OnRewardedAdDisplayedEvent");
                adpl.countLoad = 0;
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads gift {placement} admobmy OnRewardedAdDisplayedEvent not pl");
            }
        }
        private void OnRewardedImpresstionEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads gift {placement} admobmy OnRewardedImpresstionEvent");
        }
        private void OnRewardedAdClickEvent(string placement, string adsId, string adnet)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLGift.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLGift[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdClick(spl, "rewarded", "admob", adsource, adsId);
            if (!giftIsClick)
            {
                giftIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "rewarded", adsId, "admob", "");
            }
        }
        private void OnRewardedAdReceivedRewardEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads gift admobmy OnRewardedAdReceivedRewardEvent");
            isRewardCom = true;
        }
        private void OnRewardedAdDismissedEvent(string placement, string adsId, string adnet)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLGift.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLGift[placement];
                SdkUtil.logd($"ads gift {adpl.showPl}-{placement} admobmy OnRewardedAdDismissedEvent");
                adpl.isloaded = false;
                adpl.isLoading = false;
                adpl.setShowing(false);
                adpl.setStatusLoad(adsId, false);
                spl = adpl.showPl;
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
                }
                adpl.cbShow = null;
                adpl.countLoad = 0;
            }
            else
            {
                SdkUtil.logd($"ads gift {placement} admobmy OnRewardedAdDismissedEvent not pl");
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "rewarded", "admob", adsource, adsId, true, "");
            onGiftClose(placement);
            isRewardCom = false;
            advhelper.onCloseFullGift(false);
        }
        private void OnRewardedAdFinishShowEvent(string placement, string adsId, string err)
        {
            //advhelper.onCloseFullGift(false);
        }
        private void OnRewardedAdPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            FIRhelper.logEvent("show_ads_total_imp");
            FIRhelper.logEvent("show_ads_reward_imp");
            FIRhelper.logEvent("show_ads_reward_imp_0");
            giftIsClick = false;
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLGift.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLGift[placement];
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

            FIRhelper.logAdEvent("ads_gift_imp");
            AppsFlyerHelperScript.logAdEvent("ads_impression", spl, "rewarded", adsId, "admob", "");
        }
        #endregion
#endif
    }
}