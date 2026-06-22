using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mygame.sdk
{
    partial class AdsAdmobMy
    {
        bool fullnticIsClick = false;
        private void initNativeIcFull()
        {
            if (adsType != 0)
            {
                return;
            }

            try
            {
                Debug.Log("mysdk: ads fullntic admobmy init adCfPlacementNtIcFull=" + advhelper.currConfig.adCfPlacementNtIcFull);
                bool isFull2 = false;
                if (advhelper.currConfig.adCfPlacementNtIcFull.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementNtIcFull.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementFull>(dicPLNtIcFull, plitem, true);
                        if (!isFull2 && plitem.Contains(PLFull2Default))
                        {
                            isFull2 = true;
                        }
                    }
                }
                string[] listpldf = AdIdsConfig.AdmobPlNtIcFull.Split(new char[] { '#' });
                foreach (string plitem in listpldf)
                {
                    addAdPlacement<AdPlacementFull>(dicPLNtIcFull, plitem, false);
                    if (!isFull2 && plitem.Contains(PLFull2Default))
                    {
                        isFull2 = true;
                    }
                }
                if (dicPLNtIcFull.ContainsKey(PLFullDefault))
                {
                    if (!dicPLNtIcFull.ContainsKey(PLFull2Default))
                    {
                        AdPlacementFull adplfull2 = new AdPlacementFull();
                        adplfull2.coppyFrom(dicPLNtIcFull[PLFullDefault]);
                        adplfull2.placement = PLFull2Default;
                        dicPLNtIcFull.Add(PLFull2Default, adplfull2);
                    }
                    else
                    {
                        if (!isFull2)
                        {
                            AdPlacementFull adplfull2 = dicPLNtIcFull[PLFull2Default];
                            List<AdECPMItem> tmpl = new List<AdECPMItem>();
                            tmpl.AddRange(adplfull2.adECPM.list);
                            adplfull2.coppyFrom(dicPLNtIcFull[PLFullDefault]);
                            adplfull2.placement = PLFull2Default;

                            for (int i = 0; i < tmpl.Count; i++)
                            {
                                for (int j = 0; j < adplfull2.adECPM.list.Count; j++)
                                {
                                    if (adplfull2.adECPM.list[j].adsId.CompareTo(tmpl[i].adsId) == 0)
                                    {
                                        adplfull2.adECPM.list[j].coppyFrom(tmpl[i]);
                                        tmpl.RemoveAt(i);
                                        i--;
                                        break;
                                    }
                                }
                            }
                            if (tmpl.Count > 0)
                            {
                                adplfull2.adECPM.list.AddRange(tmpl);
                            }
                            tmpl.Clear();
                        }
                    }
                }

                string memcfntfull = PlayerPrefs.GetString("mem_cf_ntfull_lic", "30,105,70,2,10");
                setCfNtFull(memcfntfull);
                string memcfntfullfbex = PlayerPrefs.GetString("cf_ntfull_fb_excluse", "8;8;7");
                setCfNtFullFbExcluse(memcfntfullfbex);
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ads fullntic admobmy initNativeFull ex=" + ex.ToString());
            }
        }

        //full nt Ic
        public override int getNativeIcFullLoaded(string placement)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlNtIcFull(placement, true);
            if (adpl == null)
            {
                SdkUtil.logd($"ads fullntic {placement} admobmy getNativeIcFullLoaded not pl");
                return 0;
            }
            else
            {
                if ((flagLoadAll & 4) == 0)
                {
                    if (adpl.isloaded)
                    {
                        return 1;
                    }
                    else
                    {
                        SdkUtil.logd($"ads fullntic {placement}-{adpl.placement} admobmy getNativeIcFullLoaded={adpl.isloaded}");
                    }
                }
                else
                {
                    for (int i = 0; i < adpl.adECPM.list.Count; i++)
                    {
                        if (adpl.adECPM.list[i].isLoaded)
                        {
                            return 1;
                        }
                    }
                }
            }
#endif
            return 0;
        }
        protected override void tryLoadNativeIcFull(AdPlacementFull adpl)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            if ((flagLoadAll & 4) == 0)
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
                    SdkUtil.logd($"ads fullntic {adpl.loadPl}-{adpl.placement} admobmy tryLoadNativeIcFull over try");
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
                    SdkUtil.logd($"ads fullntic {adpl.loadPl}-{adpl.placement} admobmy tryLoadNativeIcFull id={idLoad} idxCurrEcpmFull={adpl.adECPM.idxCurrEcpm} isFullHigh={adpl.isAdHigh} plid={adpl.idPl}");
                    AdsHelper.onAdLoad(adpl.loadPl, "native_full", idLoad, "admob");
                    FIRhelper.logAdEvent("ads_fullnt_load");
                    AppsFlyerHelperScript.logAdEvent("ads_load", "", "native_full", idLoad, "admob", "");
                    adpl.isLoading = true;
                    adpl.isloaded = false;
                    if (timeDeltaLoad <= 0 || adpl.adECPM.idxCurrEcpm == 0)
                    {
                        AdsAdmobMyBridge.Instance.loadNativeIcFull(adpl.placement, idLoad, adpl.adECPM.idxCurrEcpm, (int)advhelper.bnOrien);
                    }
                    else
                    {
                        if (timeDeltaLoad > 30)
                        {
                            timeDeltaLoad = 30;
                        }
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            AdsAdmobMyBridge.Instance.loadNativeIcFull(adpl.placement, idLoad, adpl.adECPM.idxCurrEcpm, (int)advhelper.bnOrien);
                        }, timeDeltaLoad);
                    }
                }
                else
                {
                    SdkUtil.logd($"ads fullntic {adpl.loadPl}-{adpl.placement} admobmy tryLoadNativeIcFull id not correct");
                    adpl.isLoading = false;
                    adpl.isloaded = false;
                }
            }
            else
            {
                for (int i = 0; i < adpl.adECPM.list.Count; i++)
                {
                    if (!adpl.adECPM.list[i].isLoaded && !adpl.adECPM.list[i].isLoading)
                    {
                        adpl.count4LoadAll++;
                        string idLoad = adpl.adECPM.list[i].adsId;
                        SdkUtil.logd($"ads fullntic {adpl.loadPl}-{adpl.placement} admobmy tryLoadNativeIcFull={idLoad}, idxload={i}");
                        AdsHelper.onAdLoad(adpl.loadPl, "native_full", idLoad, "admob");
                        FIRhelper.logAdEvent("ads_fullnt_load");
                        AppsFlyerHelperScript.logAdEvent("ads_load", "", "native_full", idLoad, "admob", "");
                        adpl.adECPM.list[i].isLoading = true;
                        adpl.adECPM.list[i].isLoaded = false;
                        AdsAdmobMyBridge.Instance.loadNativeIcFull(adpl.placement, idLoad, i, (int)advhelper.bnOrien);
                    }
                }
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads fullntic {adpl.placement} admobmy tryLoadNativeIcFull not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadNativeIcFull(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY

#if UNITY_IOS || UNITY_IPHONE
            SdkUtil.logd($"ads fullntic {placement} admobmy loadNativeIcFull not in ios");
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
            return;
#endif

            AdPlacementFull adpl = getPlNtIcFull(placement, false);
            if (adpl == null)
            {
                SdkUtil.logd($"ads fullntic {placement} admobmy loadNativeIcFull not placement");
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
                    if ((flagLoadAll & 4) == 0)
                    {
                        if (!adpl.isloaded && !adpl.isLoading)
                        {
                            SdkUtil.logd($"ads fullntic {placement}-{adpl.placement} admobmy loadNativeIcFull type={adsType}");
                            adpl.cbLoad = cb;
                            nativefullisnew = false;
                            adpl.countLoad = 0;
                            adpl.adECPM.idxCurrEcpm = 0;
                            adpl.setSetPlacementLoad(placement);
                            tryLoadNativeIcFull(adpl);
                        }
                        else
                        {
                            SdkUtil.logd($"ads fullntic {placement}-{adpl.placement} admobmy loadNativeIcFull isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                        }
                    }
                    else
                    {
                        if (!adpl.isHighAdLoaded() && adpl.count4LoadAll <= 0)
                        {
                            SdkUtil.logd($"ads fullntic {placement}-{adpl.placement} admobmy loadNativeIcFull all");
                            adpl.cbLoad = cb;
                            nativefullisnew = false;
                            adpl.count4LoadAll = 0;
                            adpl.adECPM.idxCurrEcpm = 0;
                            adpl.setSetPlacementLoad(placement);
                            tryLoadNativeIcFull(adpl);
                        }
                        else
                        {
                            SdkUtil.logd($"ads fullntic {placement}-{adpl.placement} admobmy loadNativeIcFull isHighAdLoaded={adpl.isHighAdLoaded()} or count4LoadAll={adpl.count4LoadAll}");
                        }
                    }
                }
                else
                {
                    SdkUtil.logd($"ads fullntic {placement}-{adpl.placement} admobmy loadNativeIcFull showing={adpl.getShowing()}");
                }
            }
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showNativeIcFull(string placement, float timeDelay, int timeNtDl, bool isHideBtClose, bool isShow2, int timeClose, bool isAutoCloseWhenClick, AdCallBack cb)
        {
            isFullNt2 = isShow2;
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlNtIcFull(placement, true);
            if (adpl != null)
            {
                //adpl.cbShow = null;
                int ss = getNativeIcFullLoaded(adpl.placement);
                if (ss > 0 && !adpl.getShowing())
                {
                    adpl.countLoad = 0;
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
#if UNITY_IOS || UNITY_IPHONE
                    float tdelaycom = timeDelay;
#else
                    float tdelaycom = timeDelay;
#endif
                    int layout = PlayerPrefs.GetInt("cf_layout_ntfull", AppConfig.native_full_layout);
                    if (tdelaycom > 0)
                    {
                        SdkUtil.logd($"ads fullntic {placement}-{adpl.placement} admobmy showFull nativeic delay:{tdelaycom} call show delay={timeDelay} ntdelay={timeNtDl}");
                        adpl.setShowing(true);
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            SdkUtil.logd($"ads fullntic {placement}-{adpl.placement} admobmy showFull nativeic show!!!!!!");
                            AdsHelper.onAdShowStart(placement, "native_full", "admob", "");
                            bool iss = AdsAdmobMyBridge.Instance.showNativeIcFull(adpl.placement, !isHideBtClose, timeClose, timeNtDl, isAutoCloseWhenClick, layout, isShow2);
                            if (!iss)
                            {
                                adpl.setShowing(false);
                                if (cb != null)
                                {
                                    cb(AD_State.AD_SHOW_FAIL);
                                }
                            }
                        }, tdelaycom);
                        return true;
                    }
                    else
                    {
                        SdkUtil.logd($"ads fullntic {placement}-{adpl.placement} admobmy showFull nativeic call show delay={timeDelay} ntdelay={timeNtDl}");
                        AdsHelper.onAdShowStart(placement, "native_full", "admob", "");
                        bool iss = AdsAdmobMyBridge.Instance.showNativeIcFull(adpl.placement, !isHideBtClose, timeClose, timeNtDl, isAutoCloseWhenClick, layout, isShow2);
                        adpl.setShowing(iss);
                        return iss;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads fullntic {placement}-{adpl.placement} admobmy showFull nativeic not load or showing={adpl.getShowing()}");
                }
            }
            else
            {
                SdkUtil.logd($"ads fullntic {placement} admobmy showFull nativeic not pl");
            }
#endif
            return false;
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
        #region Full Native Icon AD EVENTS
        private void OnNativeIcFullLoadedEvent(string placement, string adsId, string adnet)
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
            FIRhelper.logAdEvent("ads_fullnt_load_1");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "native_full", adsId, "admob", "1");
            if (dicPLNtIcFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtIcFull[placement];
                SdkUtil.logd($"ads fullntic {adpl.loadPl}-{placement} admobmy OnNativeIcFullLoadedEvent plid={adpl.idPl}");
                string adsource = FIRhelper.getAdsourceAdmob(adnet);
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_full", adsId, "admob", adsource, true);
                adpl.countLoad = 0;
                adpl.isLoading = false;
                adpl.isloaded = true;
                nativeicfullisnew = false;
                if ((flagLoadAll & 4) != 0)
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
                SdkUtil.logd($"ads fullntic {placement} admobmy OnNativeIcFullLoadedEvent not pl");
            }
        }
        private void OnNativeIcFullFailedEvent(string placement, string adsId, string err)
        {
            FIRhelper.logAdEvent("ads_fullnt_load_0");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "native_full", adsId, "admob", "0");
            if (dicPLNtIcFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtIcFull[placement];
                SdkUtil.logd($"ads fullntic {adpl.loadPl}-{placement} admobmy OnNativeIcFullFailedEvent=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_full", adsId, "admob", "", false);
                if ((flagLoadAll & 4) == 0)
                {
                    adpl.isLoading = false;
                    adpl.isloaded = false;
                    if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                    {
                        if (nativeicfullisnew)
                        {
                            nativeicfullisnew = false;
                            adpl.adECPM.idxCurrEcpm = 0;
                        }
                        else
                        {
                            adpl.adECPM.idxCurrEcpm++;
                        }
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            tryLoadNativeIcFull(adpl);
                        }, 1);
                    }
                    else
                    {
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            adpl.countLoad++;
                            tryLoadNativeIcFull(adpl);
                        }, 1.0f);
                    }
                }
                else
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
            }
            else
            {
                SdkUtil.logd($"ads fullntic {placement} admobmy OnNativeIcFullFailedEvent=" + err);
            }
        }
        private void OnNativeIcFullDisplayedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLNtIcFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtIcFull[placement];
                SdkUtil.logd($"ads fullntic {adpl.showPl}-{placement} admobmy OnNativeIcFullDisplayedEvent");
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads fullntic {placement} admobmy OnNativeIcFullDisplayedEvent not pl");
            }
        }
        private void OnNativeIcFullImpresstionEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads fullntic {placement} admobmy OnNativeIcFullImpresstionEvent");
        }
        private void onNativeIcFullFailedToShow(string placement, string adsId, string adnet, string err)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtIcFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtIcFull[placement];
                SdkUtil.logd($"ads fullntic {adpl.showPl}-{placement} admobmy onNativeIcFullFailedToShow=" + err);
                adpl.isLoading = false;
                adpl.isloaded = false;
                spl = adpl.showPl;
                adpl.setShowing(false);
                adpl.setStatusLoad(adsId, false);
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                }
            }
            else
            {
                SdkUtil.logd($"ads fullntic {placement} admobmy not pl onNativeIcFullFailedToShow=" + err);
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "native_full", "admob", adsource, adsId, false, err);
            onFullClose(placement);
            advhelper.onCloseFullGift(true);
        }
        private void OnNativeIcFullClickEvent(string placement, string adsId, string adnet)
        {
            if (!isFullNt2)
            {
                FIRhelper.logEvent("show_ads_full_nt_click");
            }
            else
            {
                FIRhelper.logEvent("show_ads_full2_nt_click");
            }
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtIcFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtIcFull[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdClick(spl, "native_full", "admob", adsource, adsId);
            if (!fullnticIsClick)
            {
                fullnticIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "native_full", adsId, "admob", "");
            }
        }
        private void OnNativeIcFullDismissedEvent(string placement, string adsId, string adnet)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtIcFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtIcFull[placement];
                SdkUtil.logd($"ads fullntic {adpl.showPl}-{placement} admobmy OnNativeIcFullDismissedEvent id={adsId}");
                adpl.isLoading = false;
                adpl.isloaded = false;
                spl = adpl.showPl;
                adpl.setShowing(false);
                adpl.setStatusLoad(adsId, false);
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });
                }

                adpl.countLoad = 0;
                adpl.cbShow = null;
            }
            else
            {
                SdkUtil.logd($"ads fullntic {placement} admobmy OnNativeIcFullDismissedEvent not pl");
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "native_full", "admob", adsource, adsId, true, "");
            onFullClose(placement);
            advhelper.onCloseFullGift(true);
        }
        private void OnNativeIcFullFinishShowEvent(string placement, string adsId, string err)
        {
            //advhelper.onCloseFullGift(true);
        }
        private void OnNativeIcFullAdPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            fullnticIsClick = false;
            long originva = valueMicros;
            AdsHelper.Instance.setEcpmNtFull4Fb(originva / 1000);
            if (!isFullNt2)
            {
                FIRhelper.logEvent("show_ads_total_imp");
                FIRhelper.logEvent("show_ads_full_imp");
                FIRhelper.logEvent("show_ads_full_imp_0_nt");
            }
            else
            {
                valueMicros = (long)(valueMicros * FIRhelper.perPostAdsNtFull2);
                Debug.Log($"mysdk: ads fullntic {placement} admobmy onpaid v={valueMicros} perpost={FIRhelper.perPostAdsNtFull2}");
            }
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtIcFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtIcFull[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adformat = FIRhelper.getAdformatAdmob(9);
            string adsource = FIRhelper.getAdsourceAdmob(adNet);
            var dicpr = TiktokBusiness.getAdmobParam(currencyCode);
            if (!isFullNt2 && AdsBase.PLFullSplash.CompareTo(spl) == 0)
            {
                valueMicros = (long)(valueMicros * FIRhelper.perPostAdsNtSplash);
                Debug.Log($"mysdk: ads fullntic {spl} admobmy onpaid v={valueMicros} perpost={FIRhelper.perPostAdsNtSplash}");
            }
            FIRhelper.logEventAdsPaidAdmob(spl, adformat, adsource, adsId, valueMicros, originva, dicpr["currency_code"]);
            TiktokBusiness.logAdRevenueAdmob(adformat, adsource, adsId, precisionType, valueMicros / 1000, dicpr);
            float realValue = ((float)valueMicros) / 1000000000.0f;
            AdsHelper.onAdImpression(spl, adsId, adformat, "admob", adsource, realValue, originva);

            FIRhelper.logAdEvent("ads_fullnt_imp");
            AppsFlyerHelperScript.logAdEvent("ads_impression", spl, "native_full", adsId, "admob", "");
        }
        #endregion
#endif
    }
}