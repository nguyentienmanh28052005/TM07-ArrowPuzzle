using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mygame.sdk
{
    partial class AdsAdmobMy
    {
        private void initBnNt()
        {
            if (adsType != 0)
            {
                return;
            }
            try
            {
                Debug.Log($"mysdk: ads bnnt admobmy adCfPlacementBnNt=" + advhelper.currConfig.adCfPlacementBnNt);
                if (advhelper.currConfig.adCfPlacementBnNt.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementBnNt.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementNative>(dicPLBnNt, plitem, true);
                    }
                }
                string[] listpldf = AdIdsConfig.AdmobPlBnNt.Split(new char[] { '#' });
                foreach (string plitem in listpldf)
                {
                    addAdPlacement<AdPlacementNative>(dicPLBnNt, plitem, false);
                }
                int flagShowmedia = PlayerPrefs.GetInt("cf_bnnt_showmedia", 0);
                setTypeBnnt(flagShowmedia);
            }
            catch (Exception ex)
            {
                Debug.Log($"mysdk: ads bnnt admobmy initBnNt ex=" + ex.ToString());
            }
        }
        
        // bnnt
        protected override void tryLoadBnNt(AdPlacementNative adpl)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            if (adpl.adECPM.idxCurrEcpm >= adpl.adECPM.list.Count)
            {
                adpl.adECPM.idxCurrEcpm = 0;
            }
            string idload = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
            if (adpl.adECPM.idxHighPriority >= adpl.adECPM.idxCurrEcpm)
            {
                adpl.isAdHigh = true;
            }
            SdkUtil.logd($"ads bnnt {adpl.loadPl}-{adpl.placement} admobmy tryLoadBnNt = " + idload + ", idxCurrEcpm=" + adpl.adECPM.idxCurrEcpm);
            AdsHelper.onAdLoad(adpl.loadPl, "native_banner", idload, "admob");
            adpl.isLoading = true;
            adpl.isloaded = false;
            AdsAdmobMyBridge.Instance.loadBnNt(adpl.placement, idload);
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads bnnt {adpl.loadPl} admobmy tryLoadBnNt not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadBnNt(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementNative adpl = getPlBnNt(placement);
            if (adpl != null)
            {
                adpl.cbLoad = cb;
                if (!adpl.isLoading && !adpl.isloaded)
                {
                    SdkUtil.logd($"ads bnnt {placement}-{adpl.placement} admobmy loadBnNt");
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadBnNt(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads bnnt {placement}-{adpl.placement} admobmy loadBnNt isLoading={adpl.isLoading} isloaded={adpl.isloaded}");
                }
            }
            else
            {

                SdkUtil.logd($"ads bnnt {placement} admobmy loadBnNt not pl");
            }
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showBnNt(string placement, int pos, int width, int maxH, AdCallBack cb, float dxCenter)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementNative adpl = getPlBnNt(placement);
            if (adpl == null)
            {
                if (cb != null)
                {
                    SdkUtil.logd($"ads bnnt {placement} admobmy showBnNt not pl");
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return false;
            }
            SdkUtil.logd($"ads bnnt {placement}-{adpl.placement} admobmy showBnNt pos=" + pos + ", dxCenter=" + dxCenter + ", idxEcpm=" + adpl.adECPM.idxCurrEcpm + ", countecpm=" + adpl.adECPM.list.Count);
            adpl.isShow = true;
            adpl.posBanner = pos;
            adpl.setSetPlacementShow(placement);
            bnWidth = width;
            bnDxCenter = dxCenter;
            int trefresh = PlayerPrefs.GetInt("cf_bnnt_refresh", 20);
            if (!adpl.isLoading)
            {
                bnnmisnew = false;
                int idxshow = -10;
                adpl.adECPM.idxCurrEcpm = 0;
                for (int j = 0; j < adpl.adECPM.list.Count; j++)
                {
                    AdECPMItem bnec = adpl.adECPM.list[j];
                    if (bnec.isLoaded)
                    {
                        string idload = bnec.adsId;
                        SdkUtil.logd($"ads bnnt {placement}-{adpl.placement} admobmy showBnNt show pre loaded adsid=" + bnec.adsId + ", idx=" + j + ", dxCenter=" + dxCenter);
                        AdsAdmobMyBridge.Instance.showBnNt(adpl.placement, adpl.posBanner, width, maxH, (int)advhelper.bnOrien, dxCenter, trefresh);
                        advhelper.hideOtherBanner(20);
                        idxshow = j;
                        break;
                    }
                }

                if (idxshow != -10)
                {
                    if (cb != null)
                    {
                        cb(AD_State.AD_SHOW);
                    }
                    return true;
                }
                else
                {
                    AdsAdmobMyBridge.Instance.showBnNt(adpl.placement, adpl.posBanner, width, maxH, (int)advhelper.bnOrien, dxCenter, trefresh);
                    loadBnNt(placement, cb);
                    return false;
                }
            }
            else
            {
                SdkUtil.logd($"ads bnnt {placement}-{adpl.placement} admobmy showBnNt isprocess show dxCenter=" + dxCenter);
                adpl.cbLoad = cb;
                bool _iss = false;
                for (int j = 0; j < adpl.adECPM.list.Count; j++)
                {
                    AdECPMItem bnec = adpl.adECPM.list[j];
                    if (bnec.isLoaded)
                    {
                        _iss = true;
                        string idload = bnec.adsId;
                        AdsAdmobMyBridge.Instance.showBnNt(adpl.placement, adpl.posBanner, width, maxH, (int)advhelper.bnOrien, dxCenter, trefresh);
                        advhelper.hideOtherBanner(20);
                        break;
                    }
                }
                if (!_iss)
                {
                    AdsAdmobMyBridge.Instance.showBnNt(adpl.placement, adpl.posBanner, width, maxH, (int)advhelper.bnOrien, dxCenter, trefresh);
                }
                return false;
            }
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads bnnt {placement} admobmy showBnNt not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
            return false;
#endif
        }
        public override void hideBnNt()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            SdkUtil.logd($"ads bnnt admobmy hideBnNt");
            foreach (var adi in dicPLBanner)
            {
                adi.Value.isShow = false;
                adi.Value.isRealShow = false;
            }
            AdsAdmobMyBridge.Instance.hideBnNt();
#endif
        }
        public override void destroyBnNt()
        {
            hideBnNt();
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
        #region Banner native
        public void OnBnNtAdLoadedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLBnNt.ContainsKey(placement))
            {
                AdPlacementNative adpl = dicPLBnNt[placement];
                SdkUtil.logd($"ads bnnt {placement} admobmy OnBnNtAdLoadedEvent");
                string adsource = FIRhelper.getAdsourceAdmob(adnet);
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_banner", adsId, "admob", adsource, true);
                if (adpl.isLoading)
                {
                    bnnmisnew = false;
                    adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].isLoaded = true;
                    if (adpl.isShow)
                    {
                        tShowBannerNm = 0;
                        adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].timeShow = SdkUtil.CurrentTimeMilis() / 1000;
                    }
                }
                adpl.isloaded = true;
                adpl.isLoading = false;
                adpl.countLoad = 0;

                if (adpl.isShow)
                {
                    if (advhelper.bnCurrShow == 20)
                    {
                        SdkUtil.logd($"ads bnnt {placement} admobmy OnBnNtAdLoadedEvent hide other");
                        advhelper.hideOtherBanner(20);
                    }
                }

                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
                if (advhelper != null)
                {
                    advhelper.onBannerLoadOk(adsType);
                }
            }
            else
            {

                SdkUtil.logd($"ads bnnt {placement} admobmy OnBnNtAdLoadedEvent not pl");
            }
        }
        private void OnBnNtAdLoadFailedEvent(string placement, string adsId, string err)
        {
            if (dicPLBnNt.ContainsKey(placement))
            {
                AdPlacementNative adpl = dicPLBnNt[placement];
                SdkUtil.logd($"ads bnnt {adpl.loadPl}-{placement} admobmy OnBnNtAdLoadFailedEvent=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_banner", adsId, "admob", "", false);
                if (adpl.isLoading)
                {
                    SdkUtil.logd($"ads bnnt {adpl.loadPl}-{placement} admobmy OnBnNtAdLoadFailedEvent isloading=true");
                    if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                    {
                        if (bnnmisnew)
                        {
                            bnnmisnew = false;
                            adpl.adECPM.idxCurrEcpm = 0;
                        }
                        else
                        {
                            adpl.adECPM.idxCurrEcpm++;
                        }
                        if (isShowHighPriorityBanner)
                        {
                            if (adpl.adECPM.idxHighPriority >= adpl.adECPM.idxCurrEcpm)
                            {
                                tryLoadBnNt(adpl);
                            }
                            else
                            {
                                adpl.adECPM.idxCurrEcpm = 0;
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
                        else
                        {
                            tryLoadBnNt(adpl);
                        }
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
                            if (advhelper != null)
                            {
                                advhelper.onBannerLoadFail(adsType);
                            }
                        });
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads bnnt {placement} admobmy not pl OnBnNtAdLoadFailedEvent=" + err);
            }
        }
        private void OnBnNtClickEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads bnnt {placement} admobmy OnBnNtClick");
            SDKManager.Instance.onClickAd();
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdClick(SDKManager.Instance.currPlacement, "native_banner", "admob", adsource, adsId);
        }
        private void OnBnNtImpression(string placement, string adsId, string adnet)
        {
            if (dicPLBnNt.ContainsKey(placement))
            {
                AdPlacementNative adpl = dicPLBnNt[placement];
                SdkUtil.logd($"ads bnnt {adpl.showPl}-{placement} admobmy OnBnNtImpression");
                if (bnntisnew && !adpl.isCheckNewIds)
                {
                    adpl.isCheckNewIds = true;
                    adpl.isloaded = false;
                }
                if (adpl.adECPM.idxHighPriority >= adpl.adECPM.idxCurrEcpm && AdsHelper.Instance.statusLogicIron > 0 && isBannerHigh)
                {
                    SdkUtil.logd($"ads bnnt {adpl.showPl}-{placement} admobmy OnBnNtImpression {placement} hideOtherBanner");
                    advhelper.hideOtherBanner(20);
                }
            }
            else
            {
                SdkUtil.logd($"ads bnnt {placement} admobmy OnBnNtImpression not pl");
            }
        }
        private void OnBnNtAdPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            SdkUtil.logd($"ads bnnt {placement} admobmy bnnt OnBnNtAdPaidEvent va={valueMicros}");
            FIRhelper.logEvent("show_ads_nt");
            FIRhelper.logEvent("show_ads_nt_bn");
            string spl = SDKManager.Instance.currPlacement;
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adformat = FIRhelper.getAdformatAdmob(10);
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