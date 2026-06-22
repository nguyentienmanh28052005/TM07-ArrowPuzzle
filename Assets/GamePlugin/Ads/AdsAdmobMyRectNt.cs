using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mygame.sdk
{
    partial class AdsAdmobMy
    {
        private void initRectNt()
        {
            if (adsType != 0)
            {
                return;
            }
            try
            {
                Debug.Log($"mysdk: ads rectnt admobmy initRectNt adCfPlacementRectNt=" + advhelper.currConfig.adCfPlacementRectNt);
                if (advhelper.currConfig.adCfPlacementRectNt.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementRectNt.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementNative>(dicPLRectNt, plitem, true);
                    }
                }
                string[] listpldf = AdIdsConfig.AdmobPlRectNt.Split(new char[] { '#' });
                foreach (string plitem in listpldf)
                {
                    addAdPlacement<AdPlacementNative>(dicPLRectNt, plitem, false);
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"mysdk: ads rectnt admobmy initRectNt ex=" + ex.ToString());
            }
        }
        //rect native
        protected override void tryLoadRectNt(AdPlacementNative adpl)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            SdkUtil.logd($"ads rectnt {adpl.loadPl}-{adpl.placement} admobmy tryLoadRectNt");
            string idload = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
            adpl.isLoading = true;
            adpl.isloaded = false;
            AdsHelper.onAdLoad(adpl.loadPl, "native_custome", idload, "admob");
            AdsAdmobMyBridge.Instance.loadRectNt(adpl.placement, idload);
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads rectnt admobmy tryLoadRectNt not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadRectNt(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            SdkUtil.logd($"ads rectnt {placement} admobmy loadRectNt");
            AdPlacementNative adpl = getPlRectNt(placement);
            if (adpl != null)
            {
                adpl.cbLoad = cb;
                if (!adpl.isLoading)
                {
                    adpl.countLoad = 0;
                    tShowBannerRect = -1;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadRectNt(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads rectnt {placement}-{adpl.placement} admobmy loadRectNt isProcessShow");
                }
            }
            else
            {
                SdkUtil.logd($"ads rectnt {placement} admobmy loadRectNt not pl");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
            }
#else
            SdkUtil.logd($"ads rectnt {placement} admobmy loadRectNt not enable");
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showRectNt(string placement, int pos, int orien, float width, float height, float dx, float dy, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            SdkUtil.logd($"ads rectnt {placement} admobmy showRectNt");
            AdPlacementNative adpl = getPlRectNt(placement);
            if (adpl != null)
            {
                adpl.isShow = true;
                adpl.posBanner = pos;
                adpl.setSetPlacementShow(placement);
                rectntWith = width;
                rectntHeight = height;
                rectntdx = dx;
                rectntdy = dy;
                if (!adpl.isLoading)
                {
                    rectntisnew = false;
                    int idxsh = -10;
                    long tcurr = SdkUtil.CurrentTimeMilis() / 1000;
                    for (int j = 0; j < adpl.adECPM.list.Count; j++)
                    {
                        AdECPMItem rectntec = adpl.adECPM.list[j];
                        if (rectntec.isLoaded)
                        {
                            string idload = rectntec.adsId;
                            SdkUtil.logd($"ads rectnt {placement}-{adpl.placement} admobmy showRectNt show pre loaded adsid=" + rectntec.adsId + ", idx=" + j + ", dx=" + dx);
                            AdsAdmobMyBridge.Instance.showRectNt(adpl.placement, pos, orien, width, height, dx, dy);
                            idxsh = j;
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
                        AdsAdmobMyBridge.Instance.showRectNt(adpl.placement, pos, orien, width, height, dx, dy);
                        loadRectNt(placement, cb);
                        return false;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads rectnt {placement}-{adpl.placement} admobmy showRectNt isprocess show dx=" + dx);
                    adpl.cbLoad = cb;
                    bool _iss = false;
                    for (int j = 0; j < adpl.adECPM.list.Count; j++)
                    {
                        AdECPMItem rectntec = adpl.adECPM.list[j];
                        if (rectntec.isLoaded)
                        {
                            _iss = true;
                            string idload = rectntec.adsId;
                            AdsAdmobMyBridge.Instance.showRectNt(adpl.placement, pos, orien, width, height, dx, dy);
                            break;
                        }
                    }
                    if (!_iss)
                    {
                        AdsAdmobMyBridge.Instance.showRectNt(adpl.placement, pos, orien, width, height, dx, dy);
                    }
                    return false;
                }
            }
            else
            {
                SdkUtil.logd($"ads rectnt {placement} admobmy showRectNt not pl");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return false;
            }
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads rectnt {placement} admobmy showRectNt not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
            return false;
#endif
        }
        public override void hideRectNt()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            SdkUtil.logd($"ads rectnt admobmy hideRectNt");
            foreach (var adi in dicPLRectNt)
            {
                adi.Value.isShow = false;
            }
            AdsAdmobMyBridge.Instance.hideRectNt();
#endif
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY


        #region rect native
        public void OnRectNtAdLoadedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLRectNt.ContainsKey(placement))
            {
                AdPlacementNative adpl = dicPLRectNt[placement];
                SdkUtil.logd($"ads rectnt {placement} admobmy OnRectNtAdLoadedEvent");
                string adsource = FIRhelper.getAdsourceAdmob(adnet);
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_custome", adsId, "admob", adsource, true);
                if (adpl.isLoading)
                {
                    rectntisnew = false;
                    adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].isLoaded = true;
                }
                adpl.isloaded = true;
                adpl.isLoading = false;
                adpl.countLoad = 0;
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
            }
            else
            {
                SdkUtil.logd($"ads rectnt {placement} admobmy OnRectNtAdLoadedEvent not pl");
            }
        }
        private void OnRectNtAdLoadFailedEvent(string placement, string adsId, string err)
        {
            if (dicPLRectNt.ContainsKey(placement))
            {
                AdPlacementNative adpl = dicPLRectNt[placement];
                SdkUtil.logd($"ads rectnt {adpl.loadPl}-{placement} admobmy OnRectNtAdLoadFailedEvent=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_custome", adsId, "admob", "", false);
                if (adpl.isLoading)
                {
                    SdkUtil.logd($"ads rectnt {adpl.loadPl}-{placement} admobmy OnRectNtAdLoadFailedEvent isloading=true");
                    if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                    {
                        if (rectntisnew)
                        {
                            rectntisnew = false;
                            adpl.adECPM.idxCurrEcpm = 0;
                        }
                        else
                        {
                            adpl.adECPM.idxCurrEcpm++;
                        }
                        tryLoadRectNt(adpl);
                    }
                    else
                    {
                        adpl.isLoading = false;
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            if (adpl.cbLoad != null)
                            {
                                var tmpcb = adpl.cbLoad;
                                adpl.cbLoad = null;
                                tmpcb(AD_State.AD_LOAD_FAIL);
                            }
                        });
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads rectnt {placement} admobmy not pl OnRectNtAdLoadFailedEvent=" + err);
            }
        }
        private void OnRectNtClickEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads rectnt {placement} admobmy OnRectNtClickEvent");
            SDKManager.Instance.onClickAd();
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdClick(SDKManager.Instance.currPlacement, "native_custome", "admob", adsource, adsId);
        }
        private void OnRectNtImpression(string placement, string adsId, string adnet)
        {
            if (dicPLRectNt.ContainsKey(placement))
            {
                AdPlacementNative adpl = dicPLRectNt[placement];
                SdkUtil.logd($"ads rectnt {adpl.showPl}-{placement} admobmy OnRectNtImpression");
                if (bnntisnew && !adpl.isCheckNewIds)
                {
                    adpl.isCheckNewIds = true;
                    adpl.isloaded = false;
                }
                if (adpl.adECPM.idxHighPriority >= adpl.adECPM.idxCurrEcpm && AdsHelper.Instance.statusLogicIron > 0 && isBannerHigh)
                {
                    SdkUtil.logd($"ads rectnt {adpl.showPl}-{placement} admobmy OnRectNtImpression {placement} hideOtherBanner");
                    advhelper.hideOtherBanner(20);
                }
            }
            else
            {
                SdkUtil.logd($"ads rectnt {placement} admobmy OnRectNtImpression not pl");
            }
        }
        private void OnRectNtAdPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            SdkUtil.logd($"ads rectnt {placement} admobmy bnnt OnRectNtAdPaidEvent va={valueMicros}");
            FIRhelper.logEvent("show_ads_nt");
            FIRhelper.logEvent("show_ads_nt_bn");
            string spl = SDKManager.Instance.currPlacement;
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adsource = FIRhelper.getAdsourceAdmob(adNet);
            var dicpr = TiktokBusiness.getAdmobParam(currencyCode);
            FIRhelper.logEventAdsPaidAdmob(spl, "native_custome", adsource, adsId, valueMicros, valueMicros, dicpr["currency_code"]);
            TiktokBusiness.logAdRevenueAdmob("native", adsource, adsId, precisionType, valueMicros / 1000, dicpr);
            float realValue = ((float)valueMicros) / 1000000000.0f;
            AdsHelper.onAdImpression(spl, adsId, "native_custome", "admob", adsource, realValue, valueMicros);
        }
        #endregion
#endif
    }
}