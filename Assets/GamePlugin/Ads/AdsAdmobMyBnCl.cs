using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mygame.sdk
{
    partial class AdsAdmobMy
    {
        private void initBannerCl()
        {
            if (adsType != 0)
            {
                return;
            }
            try
            {
                Debug.Log($"mysdk: ads bncl admobmy adCfPlacementCollapse=" + advhelper.currConfig.adCfPlacementCollapse);
                if (advhelper.currConfig.adCfPlacementCollapse.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementCollapse.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementBanner>(dicPLCl, plitem, true);
                    }
                }
                string[] listpldf = AdIdsConfig.AdmobPlCl.Split(new char[] { '#' });
                foreach (string plitem in listpldf)
                {
                    addAdPlacement<AdPlacementBanner>(dicPLCl, plitem, false);
                }
                string memcfntclfbex = PlayerPrefs.GetString("cf_ntcl_fb_excluse", "6;5;0;2");
                setCfNtClFbExcluse(memcfntclfbex);
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ads bncl admobmy initBanner collapse ex=" + ex.ToString());
            }
        }
        
        //bn cl
        protected override void tryLoadCollapseBanner(AdPlacementBanner adpl)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            SdkUtil.logd($"ads bncl {adpl.loadPl}-{adpl.placement} admobmy tryLoadCollapseBanner");
            string idload = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
            adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].timeShow = 0;
            adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].time4Count = 0;
            adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].countTimeShow = 0;
            adpl.isLoading = true;
            adpl.isloaded = false;
            AdsHelper.onAdLoad(adpl.loadPl, "banner_collapse", idload, "admob");
            AdsAdmobMyBridge.Instance.loadBannerCl(adpl.placement, idload);
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads bncl {adsType} admobmy tryLoadCollapseBanner not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadCollapseBanner(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementBanner adpl = getPlBanner(placement, 1);
            if (adpl != null)
            {
                adpl.cbLoad = cb;
                if (!adpl.isLoading)
                {
                    SdkUtil.logd($"ads bncl {placement}-{adpl.placement} admobmy loadCollapseBanner");
                    adpl.countLoad = 0;
                    tShowBannerCl = -1;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadCollapseBanner(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads bncl {placement}-{adpl.placement} admobmy loadCollapseBanner isProcessShow");
                }
            }
            else
            {
                SdkUtil.logd($"ads bncl {placement} admobmy loadCollapseBanner not pl");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
            }
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads bncl {placement} admobmy loadCollapseBanner not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showCollapseBanner(string placement, int pos, int width, int maxH, float dxCenter, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementBanner adpl = getPlBanner(placement, 1);
            if (adpl != null)
            {
                adpl.isShow = true;
                adpl.posBanner = pos;
                adpl.setSetPlacementShow(placement);
                bnClWidth = width;
                bnClDxCenter = dxCenter;
                flagChangecl2Nm = -1;
                var cf = advhelper.getCfAdsPlacement(placement, -1);
                if (cf != null)
                {
                    flagChangecl2Nm = cf.flagShow;
                }
                SdkUtil.logd($"ads bncl {placement}-{adpl.placement} admobmy showCollapseBanner pos={pos} flagChangecl2Nm={flagChangecl2Nm}");
                if (!adpl.isLoading)
                {
                    bnclisnew = false;
                    int idxsh = -10;
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
                            SdkUtil.logd($"ads bncl {placement}-{adpl.placement} admobmy showCollapseBanner show pre loaded adsid=" + bnec.adsId + ", idx=" + j + ", dxCenter=" + dxCenter);
                            isShowingCollapse = true;
                            AdsAdmobMyBridge.Instance.showBannerCl(adpl.placement, adpl.posBanner, width, maxH, (int)advhelper.bnOrien, dxCenter);
                            if (ishasnext)
                            {
                                StartCoroutine(waitLoadNextBannerCl(adpl));
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
                        if (tChangeCl2Nm > 0)
                        {
                            tChangeCl2Nm = 0;
                        }
                        return true;
                    }
                    else
                    {
                        SdkUtil.logd($"ads bncl {placement}-{adpl.placement} admobmy show with empty id 1");
                        AdsAdmobMyBridge.Instance.showBannerCl(adpl.placement, adpl.posBanner, width, maxH, (int)advhelper.bnOrien, dxCenter);
                        loadCollapseBanner(placement, cb);
                        return false;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads bncl {placement}-{adpl.placement} admobmy isprocess show dxCenter=" + dxCenter);
                    adpl.cbLoad = cb;
                    bool _iss = false;
                    for (int ii = 0; ii < adpl.adECPM.list.Count; ii++)
                    {
                        AdECPMItem bnec = adpl.adECPM.list[ii];
                        if (bnec.isLoaded)
                        {
                            _iss = true;
                            string idload = bnec.adsId;
                            isShowingCollapse = true;
                            if (tChangeCl2Nm > 0)
                            {
                                tChangeCl2Nm = 0;
                            }
                            AdsAdmobMyBridge.Instance.showBannerCl(adpl.placement, adpl.posBanner, width, maxH, (int)advhelper.bnOrien, dxCenter);
                            break;
                        }
                    }
                    if (!_iss)
                    {
                        SdkUtil.logd($"ads bncl {placement}-{adpl.placement} admobmy show with empty id 2");
                        AdsAdmobMyBridge.Instance.showBannerCl(adpl.placement, adpl.posBanner, width, maxH, (int)advhelper.bnOrien, dxCenter);
                    }
                    return false;
                }
            }
            else
            {
                SdkUtil.logd($"ads bncl {placement} admobmy showCollapseBanner not pl");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return false;
            }
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads bncl {placement} admobmy showCollapseBanner not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
            return false;
#endif
        }
        IEnumerator waitLoadNextBannerCl(AdPlacementBanner adpl)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            adpl.isLoading = true;
            tShowBannerCl = -1;
            yield return new WaitForSeconds(0.1f);
            adpl.adECPM.idxCurrEcpm = 0;
            tryLoadCollapseBanner(adpl);
#else
            yield return null;
#endif
        }
        public override void hideCollapseBanner()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            SdkUtil.logd($"ads bncl admobmy hideCollapseBanner call hide");
            foreach (var adi in dicPLCl)
            {
                adi.Value.isShow = false;
                adi.Value.isRealShow = false;
            }
            isShowingCollapse = false;
            AdsAdmobMyBridge.Instance.hideBannerCl();
#endif
        }
        public override void destroyCollapseBanner()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            SdkUtil.logd($"ads bncl admobmy destroyCollapseBanner");
            foreach (var adi in dicPLCl)
            {
                adi.Value.isShow = false;
                adi.Value.isRealShow = false;
                adi.Value.isloaded = false;
            }
            isShowingCollapse = false;
            AdsAdmobMyBridge.Instance.destroyBannerCl();
#endif
        }
        
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
        #region BANNER Collapse AD EVENTS
        public void OnBannerClAdLoadedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLCl.ContainsKey(placement))
            {
                AdPlacementBanner adpl = dicPLCl[placement];
                SdkUtil.logd($"ads bncl {adpl.loadPl}-{placement} admobmy OnBannerClAdLoadedEvent");
                statusShowCl = 1;
                if (adpl.isLoading)
                {
                    bnclisnew = false;
                    adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].isLoaded = true;
                    if (adpl.isShow)
                    {
                        tShowBannerCl = 0;
                        if (tChangeCl2Nm < 0)
                        {
                            tChangeCl2Nm = 0;
                        }
                        isShowingCollapse = true;
                        adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].timeShow = SdkUtil.CurrentTimeMilis() / 1000;
                        adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].time4Count = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].timeShow;
                        adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].countTimeShow = 0;
                    }
                }
                string adsource = FIRhelper.getAdsourceAdmob(adnet);
                AdsHelper.onAdLoadResult(adpl.loadPl, "banner_collapse", adsId, "admob", adsource, true);
                adpl.isloaded = true;
                adpl.isLoading = false;
                adpl.countLoad = 0;

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
        }
        private void OnBannerClAdLoadFailedEvent(string placement, string adsId, string err)
        {
            if (dicPLCl.ContainsKey(placement))
            {
                AdPlacementBanner adpl = dicPLCl[placement];
                SdkUtil.logd($"ads bncl {adpl.loadPl}-{placement} admobmy OnBannerClAdLoadFailedEvent=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "banner_collapse", adsId, "admob", "", false);
                if (adpl.isLoading)
                {
                    if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                    {
                        if (bnclisnew)
                        {
                            bnclisnew = false;
                            adpl.adECPM.idxCurrEcpm = 0;
                        }
                        else
                        {
                            adpl.adECPM.idxCurrEcpm++;
                        }
                        tryLoadCollapseBanner(adpl);
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
                            if (!isShowingCollapse)
                            {
                                SdkUtil.logd($"ads bncl {adpl.loadPl}-{placement} admobmy OnBannerClAdLoadFailedEvent show bn when cl load fail isShowingCollapse = false isshow={adpl.isShow}");
                                advhelper.hideBannerCollapse();
                                if (adpl.isShow)
                                {
                                    advhelper.showBanner(adpl.loadPl, (AD_BANNER_POS)adpl.posBanner, advhelper.bnOrien, 0, bnClWidth, advhelper.bnMaxH, bnClDxCenter);
                                }
                            }
                            else
                            {
                                SdkUtil.logd($"ads bncl {adpl.loadPl}-{placement} admobmy OnBannerClAdLoadFailedEvent show bn when cl load fail isShowingCollapse = true");
                            }
                        });
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads bncl {placement} admobmy not pl OnBannerClAdLoadFailedEvent=" + err);
            }
        }
        private void OnBannerClImpression(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads bncl {placement} admobmy OnBannerClImpression");
            if (dicPLCl.ContainsKey(placement))
            {
                AdPlacementBanner adpl = dicPLCl[placement];
                if (bnclisnew && !adpl.isCheckNewIds)
                {
                    adpl.isCheckNewIds = true;
                    adpl.isloaded = false;
                }
            }
        }
        private void OnBannerClOpen(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads bncl {placement} admobmy OnBannerClOpen");
            advhelper.isBannerClExpand = true;
            if (statusShowCl == 1)
            {
                StatusClViewShow++;
                if (advhelper.currConfig.typeAutoReloadBannerCl == 0)
                {
                    tShowBannerCl = -1;
                }
                FIRhelper.logEvent("show_ads_banner_cletr");
                SdkUtil.logd($"ads bncl {placement} admobmy OnBannerClOpen isClViewShow");
            }
        }
        private void OnBannerClClickEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads bncl {placement} admobmy OnBannerClClick");
            SDKManager.Instance.onClickAd();
            statusShowCl = 2;
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLCl.ContainsKey(placement))
            {
                AdPlacementBanner adpl = dicPLCl[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdClick(spl, "banner_collapse", "admob", adsource, adsId);
        }
        private void OnBannerClClose(string placement, string adsId, string adnet)
        {
            if (dicPLCl.ContainsKey(placement))
            {
                AdPlacementBanner adpl = dicPLCl[placement];
                SdkUtil.logd($"ads bncl {adpl.showPl}-{placement} admobmy OnBannerClClose");
                advhelper.isBannerClExpand = false;
                StatusClViewShow--;
                if (StatusClViewShow < 0)
                {
                    StatusClViewShow = 0;
                }
                if (flagChangecl2Nm == 2)
                {
                    SdkUtil.logd($"ads bncl {adpl.showPl}-{placement} admobmy OnBannerClClose change collapse to banner");
                    tChangeCl2Nm = -1;
                    tShowBannerCl = -1;
                    advhelper.hideBannerCollapse();
                    if (adpl.posBanner == 0)
                    {
                        advhelper.showBanner(adpl.showPl, AD_BANNER_POS.TOP, advhelper.bnOrien, 0, bnClWidth, advhelper.bnMaxH, bnClDxCenter);
                    }
                    else
                    {
                        advhelper.showBanner(adpl.showPl, AD_BANNER_POS.BOTTOM, advhelper.bnOrien, 0, bnClWidth, advhelper.bnMaxH, bnClDxCenter);
                    }
                }
                else if (flagChangecl2Nm == 1)
                {
                    SdkUtil.logd($"ads bncl {adpl.showPl}-{placement} admobmy OnBannerClClose typerl=" + advhelper.currConfig.typeAutoReloadBannerCl + ", StatusClViewShow=" + StatusClViewShow);
                    if (advhelper.currConfig.typeAutoReloadBannerCl == 0 && StatusClViewShow <= 0)
                    {
                        tShowBannerCl = 0;
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads bncl {placement} admobmy OnBannerClClose not pl");
            }
        }
        private void OnBannerClAdPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            FIRhelper.logEvent("show_ads_bn");
            FIRhelper.logEvent("show_ads_bn_cl_0");
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLCl.ContainsKey(placement))
            {
                AdPlacementBanner adpl = dicPLCl[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adformat = FIRhelper.getAdformatAdmob(1);
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