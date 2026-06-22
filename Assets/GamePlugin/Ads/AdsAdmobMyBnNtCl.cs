using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mygame.sdk
{
    partial class AdsAdmobMy
    {
        bool bnntclIsClick = false;
        private void initNativeCl()
        {
            if (adsType == 0)
            {
                try
                {
                    Debug.Log("mysdk: ads bnclnt admobmy init stepFloorECPMNativeCl=" + advhelper.currConfig.adCfPlacementNattiveCl);
                    if (advhelper.currConfig.adCfPlacementNative.Length > 0)
                    {
                        string[] listpl = advhelper.currConfig.adCfPlacementNattiveCl.Split(new char[] { '#' });
                        foreach (string plitem in listpl)
                        {
                            addAdPlacement<AdPlacementNative>(dicPLNativeCl, plitem, true);
                        }
                    }
                    string[] listpldf = AdIdsConfig.AdmobPlBnNativeCl.Split(new char[] { '#' });
                    foreach (string plitem in listpldf)
                    {
                        addAdPlacement<AdPlacementNative>(dicPLNativeCl, plitem, false);
                    }
                    Debug.Log("mysdk: ads bnclnt admobmy init dicPLNativeCl=" + dicPLNativeCl.Count);
                }
                catch (Exception ex)
                {
                    Debug.Log($"mysdk: ads bnclnt admobmy initNativeCl ex=" + ex.ToString());
                }
                string memcfntcl = PlayerPrefs.GetString("mem_cf_ntcl_flic", "20,85,2,50");
                setCfNtCl(memcfntcl);
            }
        }

        //bn nt cl
        public override void loadNtCl(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementNative adpl = getPlNtCl(placement);
            if (adpl != null)
            {
                if ((flagLoadAll & 16) == 0)
                {
                    if (!adpl.isLoading && !adpl.isloaded)
                    {
                        SdkUtil.logd($"ads bnclnt {placement}-{adpl.placement} admobmy loadNativeCl");
                        adpl.cbLoad = cb;
                        bnntclisnew = false;
                        adpl.countLoad = 0;
                        adpl.adECPM.idxCurrEcpm = 0;
                        adpl.setSetPlacementLoad(placement);
                        tryLoadNtCl(adpl, adpl.cbLoad);
                    }
                    else
                    {
                        SdkUtil.logd($"ads bnclnt {placement}-{adpl.placement} admobmy loadNativeCl loading={adpl.isLoading}, loaded={adpl.isloaded}");
                    }
                }
                else
                {
                    if (!adpl.isHighAdLoaded() && adpl.count4LoadAll <= 0)
                    {
                        SdkUtil.logd($"ads bnclnt {placement}-{adpl.placement} admobmy loadNativeCl all");
                        adpl.cbLoad = cb;
                        bnntclisnew = false;
                        adpl.count4LoadAll = 0;
                        adpl.adECPM.idxCurrEcpm = 0;
                        adpl.setSetPlacementLoad(placement);
                        tryLoadNtCl(adpl, adpl.cbLoad);
                    }
                    else
                    {
                        SdkUtil.logd($"ads bnclnt {placement}-{adpl.placement} admobmy loadNativeCl isHighAdLoaded={adpl.isHighAdLoaded()} or count4LoadAll={adpl.count4LoadAll}");
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads bnclnt {placement} admobmy loadNativeCl not pl");
            }
#endif
        }
        protected override void tryLoadNtCl(AdPlacementNative adpl, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            if ((flagLoadAll & 16) == 0)
            {
                if (adpl.adECPM.idxCurrEcpm >= adpl.adECPM.list.Count)
                {
                    adpl.adECPM.idxCurrEcpm = 0;
                }
                string idload = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
                SdkUtil.logd($"ads bnclnt {adpl.loadPl}-{adpl.placement} admobmy tryLoadNtCl=" + idload + ", idxCurrEcpm=" + adpl.adECPM.idxCurrEcpm);
                AdsHelper.onAdLoad(adpl.loadPl, "native_collapse", idload, "admob");
                FIRhelper.logAdEvent("ads_ntcl_load");
                AppsFlyerHelperScript.logAdEvent("ads_load", "", "native_collapse", idload, "admob", "");
                adpl.isLoading = true;
                adpl.isloaded = false;
                AdsAdmobMyBridge.Instance.loadNativeCl(adpl.placement, idload, adpl.adECPM.idxCurrEcpm, (int)advhelper.bnOrien);
            }
            else
            {
                for (int i = 0; i < adpl.adECPM.list.Count; i++)
                {
                    if (!adpl.adECPM.list[i].isLoaded && !adpl.adECPM.list[i].isLoading)
                    {
                        adpl.count4LoadAll++;
                        string idLoad = adpl.adECPM.list[i].adsId;
                        SdkUtil.logd($"ads bnclnt {adpl.loadPl}-{adpl.placement} admobmy tryLoadNtCl={idLoad}, idxload={i}");
                        AdsHelper.onAdLoad(adpl.loadPl, "native_collapse", idLoad, "admob");
                        FIRhelper.logAdEvent("ads_ntcl_load");
                        AppsFlyerHelperScript.logAdEvent("ads_load", "", "native_collapse", idLoad, "admob", "");
                        adpl.adECPM.list[i].isLoading = true;
                        adpl.adECPM.list[i].isLoaded = false;
                        AdsAdmobMyBridge.Instance.loadNativeCl(adpl.placement, idLoad, i, (int)advhelper.bnOrien);
                    }
                }
            }
#endif
        }
        private bool getAdsLoaded(string placement, AdPlacementNative adpl)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            if ((flagLoadAll & 16) == 0)
            {
                if (adpl.isloaded)
                {
                    return true;
                }
            }
            else
            {
                for (int i = 0; i < adpl.adECPM.list.Count; i++)
                {
                    if (adpl.adECPM.list[i].isLoaded)
                    {
                        return true;
                    }
                }
            }
#endif
            return false;
        }
        public override bool showNtCl(string placement, int pos, int width, float dxCenter, bool isHideBtClose, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementNative adpl = getPlNtCl(placement);
            if (adpl == null)
            {
                if (cb != null)
                {
                    SdkUtil.logd($"ads bnclnt {placement} admobmy showNtCl not pl");
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return false;
            }
            else
            {
                bool re = false;
                adpl.isShow = true;
                adpl.setSetPlacementShow(placement);
                if (getAdsLoaded(placement, adpl))
                {
                    SdkUtil.logd($"ads bnclnt {adpl.placement} admobmy showNtCl loaded and show");
                    adpl.isloaded = false;
                    re = AdsAdmobMyBridge.Instance.showNativeCl(adpl.placement, pos, width, dxCenter, isHideBtClose, advhelper.isNtclCloseWhenClick > 0);
                }
                else if (adpl.hasLoaded)
                {
                    SdkUtil.logd($"ads bnclnt {adpl.placement} admobmy showNtCl has loaded, load new and show pre");
                    re = AdsAdmobMyBridge.Instance.showNativeCl(adpl.placement, pos, width, dxCenter, isHideBtClose, advhelper.isNtclCloseWhenClick > 0);
                }
                else
                {
                    SdkUtil.logd($"ads bnclnt {adpl.placement} admobmy showNtCl not loaded");
                }
                if (!re)
                {
                    advhelper.loadNativeCl4NextShow(placement);
                }
                return re;
            }
#endif
            return false;
        }
        public override void hideNtCl()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            SdkUtil.logd($"ads bnclnt admobmy hideNtCl");
            foreach (var adi in dicPLNativeCl)
            {
                adi.Value.isShow = false;
            }
            AdsAdmobMyBridge.Instance.hideNativeCl();
#endif
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
        #region Banner Native Collapse AD EVENTS
        private void OnNativeClLoadedEvent(string placement, string adsId, string adnet)
        {
            if (adnet != null && adnet.EndsWith("@@@"))
            {
                adnet = adnet.Replace("@@@", "");
                advhelper.isQcThu = true;
                if (advhelper.islogttttttt == 0)
                {
                    advhelper.islogttttttt = 1;
                    PlayerPrefs.SetInt("mem_is_log_ttt", advhelper.islogttttttt);
                    FIRhelper.logEvent("ads_test");
                    string dv = $"atn{GameHelper.Instance.AdsIdentify}";
                    dv = dv.Replace("-", "");
                    FIRhelper.logEvent(dv);
                }
            }
            FIRhelper.logAdEvent("ads_ntcl_load_1");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "native_collapse", adsId, "admob", "1");
            if (dicPLNativeCl.ContainsKey(placement))
            {
                AdPlacementNative adpl = dicPLNativeCl[placement];
                SdkUtil.logd($"ads bnclnt {adpl.loadPl}-{placement} admobmy OnNativeClLoadedEvent {adsId} {adnet}");
                string adsource = FIRhelper.getAdsourceAdmob(adnet);
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_collapse", adsId, "admob", adsource, true);
                adpl.countLoad = 0;
                adpl.isLoading = false;
                adpl.isloaded = true;
                adpl.hasLoaded = true;
                bnntclisnew = false;
                if ((flagLoadAll & 16) != 0)
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
                SdkUtil.logd($"ads bnclnt {placement} admobmy OnNativeClLoadedEvent not pl");
            }
        }
        private void OnNativeClFailedEvent(string placement, string adsId, string err)
        {
            FIRhelper.logAdEvent("ads_ntcl_load_0");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "native_collapse", adsId, "admob", "0");
            if (dicPLNativeCl.ContainsKey(placement))
            {
                AdPlacementNative adpl = dicPLNativeCl[placement];
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_collapse", adsId, "admob", "", false);
                if ((flagLoadAll & 16) == 0)
                {
                    if (adpl.isLoading)
                    {
                        SdkUtil.logd($"ads bnclnt {adpl.loadPl}-{placement} admobmy onloadFail {adsId} err=" + err);
                        if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                        {
                            if (bnntclisnew)
                            {
                                bnntclisnew = false;
                                adpl.adECPM.idxCurrEcpm = 0;
                            }
                            else
                            {
                                adpl.adECPM.idxCurrEcpm++;
                            }
                            tryLoadNtCl(adpl, adpl.cbLoad);
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
                    else
                    {
                        SdkUtil.logd($"ads bnclnt {adpl.loadPl}-{placement} admobmy onloadFail {adsId} err=" + err);
                    }
                }
                else
                {
                    SdkUtil.logd($"ads bnclnt {adpl.loadPl}-{placement} admobmy onloadFail {adsId} count={adpl.count4LoadAll} err=" + err);
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
                SdkUtil.logd($"ads bnclnt {placement} admobmy onloadFail not pl err=" + err);
            }
        }
        private void OnNativeClDisplayedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLNativeCl.ContainsKey(placement))
            {
                AdPlacementNative adpl = dicPLNativeCl[placement];
                SdkUtil.logd($"ads bnclnt {adpl.showPl}-{placement} admobmy onshow flag={advhelper.ntclCountShowing}");
                if (adpl.flagCountShow)
                {
                    adpl.flagCountShow = false;
                    advhelper.ntclCountShowing++;
                }
                if (advhelper.isShowBanner)
                {
                    advhelper.statusShowBannerAfterCloseNtCl = 1;
                    advhelper.hideBanner(0, true);
                }
            }
            else
            {
                SdkUtil.logd($"ads bnclnt {placement} admobmy onshow not pl");
            }
            advhelper.onBnClCb(placement, AD_State.AD_SHOW);
        }
        private void OnNativeClImpresstionEvent(string placement, string adsId, string adnet)
        {
            if (dicPLNativeCl.ContainsKey(placement))
            {
                AdPlacementNative adpl = dicPLNativeCl[placement];
                SdkUtil.logd($"ads bnclnt {adpl.showPl}-{placement} admobmy OnNativeClImpresstionEvent");
                adpl.isloaded = false;
                adpl.setStatusLoad(adsId, false);
            }
            else
            {
                SdkUtil.logd($"ads bnclnt {placement} admobmy OnNativeClImpresstionEvent not pl");
            }
        }
        private void OnNativeClFailedToShow(string placement, string adsId, string adnet, string err)
        {
            if (dicPLNativeCl.ContainsKey(placement))
            {
                AdPlacementNative adpl = dicPLNativeCl[placement];
                SdkUtil.logd($"ads bnclnt {adpl.showPl}-{placement} admobmy onshowfail=" + err);
                adpl.isLoading = false;
                adpl.isloaded = false;
                //if (adpl.cbShow != null)
                //{
                //    AdCallBack tmpcb = adpl.cbShow;
                //    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                //}
            }
            else
            {
                SdkUtil.logd($"ads bnclnt {placement} admobmy not pl onshowfail=" + err);
            }
            advhelper.onBnClCb(placement, AD_State.AD_SHOW_FAIL);
        }
        private void OnNativeClClickEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads bnclnt {placement} admobmy OnNativeClClickEvent");
            SDKManager.Instance.onClickAd();
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNativeCl.ContainsKey(placement))
            {
                AdPlacementNative adpl = dicPLNativeCl[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdClick(spl, "native_collapse", "admob", adsource, adsId);
            if (!bnntclIsClick)
            {
                bnntclIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "native_collapse", adsId, "admob", "");
            }
        }
        private void OnNativeClDismissedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLNativeCl.ContainsKey(placement))
            {
                AdPlacementNative adpl = dicPLNativeCl[placement];
                SdkUtil.logd($"ads bnclnt {adpl.showPl}-{placement} admobmy onclose flag={advhelper.statusShowBannerAfterCloseNtCl}");
                advhelper.ntclCountShowing--;
                adpl.flagCountShow = true;
                if (advhelper.statusShowBannerAfterCloseNtCl == 1)
                {
                    if (advhelper.ntclCountShowing <= 0)
                    {
                        advhelper.ntclCountShowing = 0;
                        advhelper.statusShowBannerAfterCloseNtCl = 0;
                        advhelper.prebannerwhenCloseNtCl();
                        advhelper.showBanner(advhelper.memPlacementBn, advhelper.bnPos, advhelper.bnOrien, 0, bnClWidth, advhelper.bnMaxH, bnClDxCenter);
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads bnclnt {placement} admobmy onclose not pl");
            }
            advhelper.onBnClCb(placement, AD_State.AD_CLOSE);

            AdsProcessCB.Instance().Enqueue(() =>
            {
                advhelper.loadNativeCl4NextShow(placement);
            }, 1.0f);
        }
        private void OnNativeClAdPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            SdkUtil.logd($"ads bnclnt {placement} admobmy OnNativeClAdPaidEvent va={valueMicros}");
            FIRhelper.logEvent("show_ads_nt");
            FIRhelper.logEvent("show_ads_nt_cl");
            bnntclIsClick = false;
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNativeCl.ContainsKey(placement))
            {
                AdPlacementNative adpl = dicPLNativeCl[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adformat = FIRhelper.getAdformatAdmob(6);
            string adsource = FIRhelper.getAdsourceAdmob(adNet);
            var dicpr = TiktokBusiness.getAdmobParam(currencyCode);
            FIRhelper.logEventAdsPaidAdmob(spl, adformat, adsource, adsId, valueMicros, valueMicros, dicpr["currency_code"]);
            TiktokBusiness.logAdRevenueAdmob(adformat, adsource, adsId, precisionType, valueMicros / 1000, dicpr);
            float realValue = ((float)valueMicros) / 1000000000.0f;
            AdsHelper.onAdImpression(spl, adsId, adformat, "admob", adsource, realValue, valueMicros);

            FIRhelper.logAdEvent("ads_ntcl_imp");
            AppsFlyerHelperScript.logAdEvent("ads_impression", spl, "native_collapse", adsId, "admob", "");
        }
        #endregion
#endif
    }
}