using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mygame.sdk
{
    partial class AdsAdmobMy
    {
        private void initBannerRect()
        {
            if (adsType != 0)
            {
                return;
            }
            try
            {
                Debug.Log($"mysdk: ads rect admobmy adCfPlacementRect=" + advhelper.currConfig.adCfPlacementRect);
                if (advhelper.currConfig.adCfPlacementRect.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementRect.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementBanner>(dicPLRect, plitem, true);
                    }
                }
                string[] listpldf = AdIdsConfig.AdmobPlRect.Split(new char[] { '#' });
                foreach (string plitem in listpldf)
                {
                    addAdPlacement<AdPlacementBanner>(dicPLRect, plitem, false);
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"mysdk: ads rect admobmy initBanner Rect ex=" + ex.ToString());
            }
        }

        //bn rect
        protected override void tryLoadRectBanner(AdPlacementBanner adpl)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            SdkUtil.logd($"ads rect {adpl.loadPl}-{adpl.placement} admobmy tryLoadRectBanner");
            string idload = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
            adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].timeShow = 0;
            adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].time4Count = 0;
            adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].countTimeShow = 0;
            adpl.isLoading = true;
            adpl.isloaded = false;
            AdsHelper.onAdLoad(adpl.loadPl, "banner_rect", idload, "admob");
            AdsAdmobMyBridge.Instance.loadBannerRect(adpl.placement, idload);
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads rect admobmy tryLoadRectBanner not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadRectBanner(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            SdkUtil.logd($"ads rect {placement} admobmy loadRectBanner");
            AdPlacementBanner adpl = getPlBanner(placement, 2);
            if (adpl != null)
            {
                adpl.cbLoad = cb;
                if (!adpl.isLoading)
                {
                    adpl.countLoad = 0;
                    tShowBannerRect = -1;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadRectBanner(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads rect {placement}-{adpl.placement} admobmy loadRectBanner isProcessShow");
                }
            }
            else
            {
                SdkUtil.logd($"ads rect {placement} admobmy loadRectBanner not pl");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
            }
#else
            SdkUtil.logd($"ads rect {placement} admobmy loadRectBanner not enable");
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showRectBanner(string placement, int pos, float width, int maxH, float dxCenter, float dyVertical, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            SdkUtil.logd($"ads rect {placement} admobmy showRectBanner");
            AdPlacementBanner adpl = getPlBanner(placement, 2);
            if (adpl != null)
            {
                adpl.isShow = true;
                adpl.posBanner = pos;
                adpl.setSetPlacementShow(placement);
                bnRectWidth = width;
                bnRectDxCenter = dxCenter;
                bnRectDyVertical = dyVertical;
                if (!adpl.isLoading)
                {
                    bnrectisnew = false;
                    int idxsh = -10;
                    long tcurr = SdkUtil.CurrentTimeMilis() / 1000;
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
                            SdkUtil.logd($"ads rect {placement}-{adpl.placement} admobmy showRectBanner show pre loaded adsid=" + bnec.adsId + ", idx=" + j + ", dxCenter=" + dxCenter);
                            AdsAdmobMyBridge.Instance.showBannerRect(adpl.placement, adpl.posBanner, width, maxH, dxCenter, dyVertical);
                            if (ishasnext)
                            {
                                StartCoroutine(waitLoadNextBannerRect(adpl));
                            }
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
                        AdsAdmobMyBridge.Instance.showBannerRect(adpl.placement, adpl.posBanner, width, maxH, dxCenter, dyVertical);
                        loadRectBanner(placement, cb);
                        return false;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads rect {placement}-{adpl.placement} admobmy showRectBanner isprocess show dxCenter=" + dxCenter);
                    adpl.cbLoad = cb;
                    bool _iss = false;
                    for (int j = 0; j < adpl.adECPM.list.Count; j++)
                    {
                        AdECPMItem bnec = adpl.adECPM.list[j];
                        if (bnec.isLoaded)
                        {
                            _iss = true;
                            string idload = bnec.adsId;
                            AdsAdmobMyBridge.Instance.showBannerRect(adpl.placement, adpl.posBanner, width, maxH, dxCenter, dyVertical);
                            break;
                        }
                    }
                    if (!_iss)
                    {
                        AdsAdmobMyBridge.Instance.showBannerRect(adpl.placement, adpl.posBanner, width, maxH, dxCenter, dyVertical);
                    }
                    return false;
                }
            }
            else
            {
                SdkUtil.logd($"ads rect {placement} admobmy showRectBanner not pl");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return false;
            }
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads rect {placement} admobmy showCollapseBanner not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
            return false;
#endif
        }
        IEnumerator waitLoadNextBannerRect(AdPlacementBanner adpl)
        {
            adpl.isLoading = true;
            tShowBannerRect = -1;
            yield return new WaitForSeconds(0.1f);
            adpl.adECPM.idxCurrEcpm = 0;
            tryLoadRectBanner(adpl);
        }
        public override void hideRectBanner()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            SdkUtil.logd($"ads rect admobmy hideRectBanner");
            foreach (var adi in dicPLRect)
            {
                adi.Value.isShow = false;
                adi.Value.isRealShow = false;
            }
            AdsAdmobMyBridge.Instance.hideBannerRect();
#endif
        }
        public override void destroyRectBanner()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            SdkUtil.logd($"ads rect admobmy destroyRectBanner");
            foreach (var adi in dicPLRect)
            {
                adi.Value.isShow = false;
                adi.Value.isRealShow = false;
                adi.Value.isloaded = false;
            }
            AdsAdmobMyBridge.Instance.hideBannerRect();
#endif
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
        #region BANNER Rect AD EVENTS
        public void OnBannerRectAdLoadedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLRect.ContainsKey(placement))
            {
                AdPlacementBanner adpl = dicPLRect[placement];
                SdkUtil.logd($"ads rect {adpl.loadPl}-{placement} admobmy OnBannerRectAdLoadedEvent");
                if (adpl.isLoading)
                {
                    bnrectisnew = false;
                    adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].isLoaded = true;
                    if (adpl.isShow)
                    {
                        tShowBannerRect = 0;
                        adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].timeShow = SdkUtil.CurrentTimeMilis() / 1000;
                        adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].time4Count = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].timeShow;
                        adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].countTimeShow = 0;
                    }
                }
                string adsource = FIRhelper.getAdsourceAdmob(adnet);
                AdsHelper.onAdLoadResult(adpl.loadPl, "banner_rect", adsId, "admob", adsource, true);
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
                SdkUtil.logd($"ads rect {placement} admobmy OnBannerRectAdLoadedEvent not pl");
            }
        }
        private void OnBannerRectAdLoadFailedEvent(string placement, string adsId, string err)
        {
            if (dicPLRect.ContainsKey(placement))
            {
                AdPlacementBanner adpl = dicPLRect[placement];
                SdkUtil.logd($"ads rect {adpl.loadPl}-{placement} admobmy OnBannerRectAdLoadFailedEvent err={err}");
                AdsHelper.onAdLoadResult(adpl.loadPl, "banner_rect", adsId, "admob", "", false);
                if (adpl.isLoading)
                {
                    if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                    {
                        if (bnrectisnew)
                        {
                            bnrectisnew = false;
                            adpl.adECPM.idxCurrEcpm = 0;
                        }
                        else
                        {
                            adpl.adECPM.idxCurrEcpm++;
                        }
                        tryLoadRectBanner(adpl);
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
                SdkUtil.logd($"ads rect {placement} admobmy OnBannerRectAdLoadFailedEvent not pl err={err}");
            }
        }
        private void OnBannerRectImpression(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads rect {placement} admobmy OnBannerRectImpression");
            if (dicPLRect.ContainsKey(placement))
            {
                AdPlacementBanner adpl = dicPLRect[placement];
                if (bnrectisnew && !adpl.isCheckNewIds)
                {
                    adpl.isCheckNewIds = true;
                    adpl.isloaded = false;
                }
            }
        }
        private void OnBannerRectClickEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads rect {placement} admobmy OnBannerRectClick");
            SDKManager.Instance.onClickAd();
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLRect.ContainsKey(placement))
            {
                AdPlacementBanner adpl = dicPLRect[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdClick(spl, "banner_rect", "admob", adsource, adsId);
        }
        private void OnBannerRectAdPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            FIRhelper.logEvent("show_ads_bn");
            FIRhelper.logEvent("show_ads_bn_rect_0");
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLRect.ContainsKey(placement))
            {
                AdPlacementBanner adpl = dicPLRect[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adformat = FIRhelper.getAdformatAdmob(2);
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