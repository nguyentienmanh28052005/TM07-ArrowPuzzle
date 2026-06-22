using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mygame.sdk
{
    partial class AdsAdmobMy
    {
        private void initBannerNm()
        {
            if (adsType != 0)
            {
                return;
            }
            try
            {
                Debug.Log($"mysdk: ads bn admobmy adCfPlacementBanner=" + advhelper.currConfig.adCfPlacementBanner);
                if (advhelper.currConfig.adCfPlacementBanner.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementBanner.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementBanner>(dicPLBanner, plitem, true);
                    }
                }
                string[] listpldf = AdIdsConfig.AdmobPlBanner.Split(new char[] { '#' });
                foreach (string plitem in listpldf)
                {
                    addAdPlacement<AdPlacementBanner>(dicPLBanner, plitem, false);
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"mysdk: ads bn admobmy initBanner ex=" + ex.ToString());
            }
        }

        // bn nm
        protected override void tryLoadBanner(AdPlacementBanner adpl)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            adpl.isAdHigh = false;
            if (adpl.adECPM.idxCurrEcpm >= adpl.adECPM.list.Count)
            {
                adpl.adECPM.idxCurrEcpm = 0;
            }
            string idload = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
            if (adpl.adECPM.idxHighPriority >= adpl.adECPM.idxCurrEcpm)
            {
                adpl.isAdHigh = true;
            }
            SdkUtil.logd($"ads bn {adpl.loadPl}-{adpl.placement} admobmy tryLoadBanner = " + idload + ", idxCurrEcpm=" + adpl.adECPM.idxCurrEcpm);
            AdsHelper.onAdLoad(adpl.loadPl, "banner", idload, "admob");
            adpl.isLoading = true;
            adpl.isloaded = false;
            AdsAdmobMyBridge.Instance.loadBanner(adpl.placement, idload);
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads bn {adpl.loadPl} admobmy tryLoadBanner not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadBanner(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementBanner adpl = getPlBanner(placement, 0);
            if (adpl != null)
            {
                adpl.cbLoad = cb;
                if (!adpl.isLoading && !adpl.isloaded)
                {
                    SdkUtil.logd($"ads bn {placement}-{adpl.placement} admobmy loadBanner");
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadBanner(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads bn {placement}-{adpl.placement} admobmy loadBanner isLoading={adpl.isLoading} isloaded={adpl.isloaded}");
                }
            }
            else
            {

                SdkUtil.logd($"ads bn {placement} admobmy loadBanner not pl");
            }
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showBanner(string placement, int pos, int width, int maxH, AdCallBack cb, float dxCenter, bool highP = false)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementBanner adpl = getPlBanner(placement, 0);
            if (adpl == null)
            {
                if (cb != null)
                {
                    SdkUtil.logd($"ads bn {placement} admobmy showBanner not pl");
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return false;
            }
            SdkUtil.logd($"ads bn {placement}-{adpl.placement} admobmy showBanner pos=" + pos + ", dxCenter=" + dxCenter + ", idxEcpm=" + adpl.adECPM.idxCurrEcpm + ", countecpm=" + adpl.adECPM.list.Count + ", highP=" + highP);
            isShowHighPriorityBanner = highP;
            adpl.isShow = true;
            adpl.posBanner = pos;
            adpl.setSetPlacementShow(placement);
            bnWidth = width;
            bnDxCenter = dxCenter;
            if (!adpl.isLoading)
            {
                bnnmisnew = false;
                int idxshow = -10;
                long tcurr = SdkUtil.CurrentTimeMilis() / 1000;
                adpl.adECPM.idxCurrEcpm = 0;
                for (int j = 0; j < adpl.adECPM.list.Count; j++)
                {
                    AdECPMItem bnec = adpl.adECPM.list[j];
                    if (bnec.isLoaded)
                    {
                        string idload = bnec.adsId;
                        bool ishasnext = false;
                        if ((tcurr - bnec.timeShow) >= advhelper.currConfig.timeReloadBanner)
                        {
                            ishasnext = true;
                        }
                        SdkUtil.logd($"ads bn {placement}-{adpl.placement} admobmy showBanner show pre loaded adsid=" + bnec.adsId + ", idx=" + j + ", dxCenter=" + dxCenter);
                        AdsAdmobMyBridge.Instance.showBanner(adpl.placement, adpl.posBanner, width, maxH, (int)advhelper.bnOrien, dxCenter);
                        advhelper.hideOtherBanner(adsType);
                        if (ishasnext)
                        {
                            StartCoroutine(waitLoadNextBanner(adpl));
                        }
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
                    AdsAdmobMyBridge.Instance.showBanner(adpl.placement, adpl.posBanner, width, maxH, (int)advhelper.bnOrien, dxCenter);
                    loadBanner(placement, cb);
                    return false;
                }
            }
            else
            {
                SdkUtil.logd($"ads bn {placement}-{adpl.placement} admobmy showBanner isprocess show dxCenter=" + dxCenter);
                adpl.cbLoad = cb;
                bool _iss = false;
                for (int j = 0; j < adpl.adECPM.list.Count; j++)
                {
                    AdECPMItem bnec = adpl.adECPM.list[j];
                    if (bnec.isLoaded)
                    {
                        _iss = true;
                        string idload = bnec.adsId;
                        AdsAdmobMyBridge.Instance.showBanner(adpl.placement, adpl.posBanner, width, maxH, (int)advhelper.bnOrien, dxCenter);
                        advhelper.hideOtherBanner(adsType);
                        break;
                    }
                }
                if (!_iss)
                {
                    AdsAdmobMyBridge.Instance.showBanner(adpl.placement, adpl.posBanner, width, maxH, (int)advhelper.bnOrien, dxCenter);
                }
                return false;
            }
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads bn {placement} admobmy showBanner not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
            return false;
#endif
        }
        IEnumerator waitLoadNextBanner(AdPlacementBanner adpl)
        {
            adpl.isLoading = true;
            tShowBannerNm = -1;
            yield return new WaitForSeconds(0.1f);
            adpl.countLoad = 0;
            adpl.adECPM.idxCurrEcpm = 0;
            tryLoadBanner(adpl);
        }
        public override void hideBanner()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            SdkUtil.logd($"ads bn admobmy hideBanner");
            foreach (var adi in dicPLBanner)
            {
                adi.Value.isShow = false;
                adi.Value.isRealShow = false;
            }
            AdsAdmobMyBridge.Instance.hideBanner();
#endif
        }
        public override void destroyBanner()
        {
            SdkUtil.logd($"ads bn admobmy destroyBanner");
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            foreach (var adi in dicPLBanner)
            {
                adi.Value.isShow = false;
                adi.Value.isRealShow = false;
                adi.Value.isloaded = false;
            }
            AdsAdmobMyBridge.Instance.hideBanner();
#endif
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
        #region BANNER AD EVENTS
        public void OnBannerAdLoadedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLBanner.ContainsKey(placement))
            {
                AdPlacementBanner adpl = dicPLBanner[placement];
                SdkUtil.logd($"ads bn {placement} admobmy OnBannerAdLoadedEvent");
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
                string adsource = FIRhelper.getAdsourceAdmob(adnet);
                AdsHelper.onAdLoadResult(adpl.loadPl, "banner", adsId, "admob", adsource, true);
                adpl.isloaded = true;
                adpl.isLoading = false;
                adpl.countLoad = 0;

                if (adpl.isShow)
                {
                    if (advhelper.bnCurrShow == adsType)
                    {
                        SdkUtil.logd($"ads bn {placement} admobmy OnBannerAdLoadedEvent hide other");
                        advhelper.hideOtherBanner(adsType);
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
                SdkUtil.logd($"ads bn {placement} admobmy OnBannerAdLoadedEvent not pl");
            }
        }
        private void OnBannerAdLoadFailedEvent(string placement, string adsId, string err)
        {
            if (dicPLBanner.ContainsKey(placement))
            {
                AdPlacementBanner adpl = dicPLBanner[placement];
                SdkUtil.logd($"ads bn {adpl.loadPl}-{placement} admobmy OnBannerAdLoadFailedEvent=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "banner", adsId, "admob", "", false);
                if (adpl.isLoading)
                {
                    SdkUtil.logd($"ads bn {adpl.loadPl}-{placement} admobmy OnBannerAdLoadFailedEvent isloading=true");
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
                                tryLoadBanner(adpl);
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
                            tryLoadBanner(adpl);
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
                SdkUtil.logd($"ads bn {placement} admobmy not pl OnBannerAdLoadFailedEvent=" + err);
            }
        }
        private void OnBannerClickEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads bn {placement} admobmy OnBannerClick");
            SDKManager.Instance.onClickAd();
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdClick(SDKManager.Instance.currPlacement, "banner", "admob", adsource, adsId);
        }
        private void OnBannerImpression(string placement, string adsId, string adnet)
        {
            if (dicPLBanner.ContainsKey(placement))
            {
                AdPlacementBanner adpl = dicPLBanner[placement];
                SdkUtil.logd($"ads bn {adpl.showPl}-{placement} admobmy OnBannerImpression");
                if (bnnmisnew && !adpl.isCheckNewIds)
                {
                    adpl.isCheckNewIds = true;
                    adpl.isloaded = false;
                }
                if (adpl.adECPM.idxHighPriority >= adpl.adECPM.idxCurrEcpm && AdsHelper.Instance.statusLogicIron > 0 && isBannerHigh)
                {
                    SdkUtil.logd($"ads bn {adpl.showPl}-{placement} admobmy OnBannerImpression {placement} hideOtherBanner");
                    advhelper.hideOtherBanner(adsType);
                }
            }
            else
            {
                SdkUtil.logd($"ads bn {placement} admobmy OnBannerImpression not pl");
            }
        }
        private void OnBannerAdPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            FIRhelper.logEvent("show_ads_bn");
            FIRhelper.logEvent("show_ads_bn_nm_0");
            string spl = SDKManager.Instance.currPlacement;
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adformat = FIRhelper.getAdformatAdmob(0);
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