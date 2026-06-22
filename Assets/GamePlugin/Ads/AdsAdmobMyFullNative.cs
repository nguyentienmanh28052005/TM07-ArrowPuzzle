using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mygame.sdk
{
    partial class AdsAdmobMy
    {
        bool fullntIsClick = false;
        private void initNativeFull()
        {
            if (adsType != 0)
            {
                return;
            }
            try
            {
                Debug.Log("mysdk: ads fullnt admobmy init adCfPlacementNtFull=" + advhelper.currConfig.adCfPlacementNtFull);
                bool isFull2 = false;
                if (advhelper.currConfig.adCfPlacementNtFull.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementNtFull.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementFull>(dicPLNtFull, plitem, true);
                        if (!isFull2 && plitem.Contains(PLFull2Default))
                        {
                            isFull2 = true;
                        }
                    }
                }
                string[] listpldf = AdIdsConfig.AdmobPlNtFull.Split(new char[] { '#' });
                foreach (string plitem in listpldf)
                {
                    addAdPlacement<AdPlacementFull>(dicPLNtFull, plitem, false);
                    if (!isFull2 && plitem.Contains(PLFull2Default))
                    {
                        isFull2 = true;
                    }
                }
                if (dicPLNtFull.ContainsKey(PLFullDefault))
                {
                    if (!dicPLNtFull.ContainsKey(PLFull2Default))
                    {
                        AdPlacementFull adplfull2 = new AdPlacementFull();
                        adplfull2.coppyFrom(dicPLNtFull[PLFullDefault]);
                        adplfull2.placement = PLFull2Default;
                        dicPLNtFull.Add(PLFull2Default, adplfull2);
                    }
                    else
                    {
                        if (!isFull2)
                        {
                            AdPlacementFull adplfull2 = dicPLNtFull[PLFull2Default];
                            List<AdECPMItem> tmpl = new List<AdECPMItem>();
                            tmpl.AddRange(adplfull2.adECPM.list);
                            adplfull2.coppyFrom(dicPLNtFull[PLFullDefault]);
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
                string memcfdayclisk = PlayerPrefs.GetString("mem_cf_nt_dayflic", "2,10,4;3,25,4;4,40,3;5,50,2");
                setCfNtdayClick(memcfdayclisk);
                string memcfntfullfbex = PlayerPrefs.GetString("cf_ntfull_fb_excluse", "8;8;7");
                setCfNtFullFbExcluse(memcfntfullfbex);
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ads fullnt admobmy initNativeFull ex=" + ex.ToString());
            }
        }
        //full nt
        public override int getNativeFullLoaded(string placement)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlNtFull(placement, true);
            if (adpl == null)
            {
                SdkUtil.logd($"ads fullnt {placement} admobmy getNativeFullLoaded not pl");
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
                        SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} admobmy getNativeFullLoaded={adpl.isloaded}");
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
        protected override void tryLoadNativeFull(AdPlacementFull adpl)
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
                    SdkUtil.logd($"ads fullnt {adpl.loadPl}-{adpl.placement} admobmy tryLoadNativeFull over try");
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
                    SdkUtil.logd($"ads fullnt {adpl.loadPl}-{adpl.placement} admobmy tryLoadNativeFull id={idLoad} idxCurrEcpmFull={adpl.adECPM.idxCurrEcpm} isFullHigh={adpl.isAdHigh} plid={adpl.idPl}");
                    AdsHelper.onAdLoad(adpl.loadPl, "native_full", idLoad, "admob");
                    FIRhelper.logAdEvent("ads_fullnt_load");
                    AppsFlyerHelperScript.logAdEvent("ads_load", "", "native_full", idLoad, "admob", "");
                    adpl.isLoading = true;
                    adpl.isloaded = false;
                    if (timeDeltaLoad <= 0 || adpl.adECPM.idxCurrEcpm == 0)
                    {
                        AdsAdmobMyBridge.Instance.loadNativeFull(adpl.placement, idLoad, adpl.adECPM.idxCurrEcpm, (int)advhelper.bnOrien);
                    }
                    else
                    {
                        if (timeDeltaLoad > 30)
                        {
                            timeDeltaLoad = 30;
                        }
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            AdsAdmobMyBridge.Instance.loadNativeFull(adpl.placement, idLoad, adpl.adECPM.idxCurrEcpm, (int)advhelper.bnOrien);
                        }, timeDeltaLoad);
                    }
                }
                else
                {
                    SdkUtil.logd($"ads fullnt {adpl.loadPl}-{adpl.placement} admobmy tryLoadNativeFull id not correct");
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
                        SdkUtil.logd($"ads fullnt {adpl.loadPl}-{adpl.placement} admobmy tryLoadNativeFull={idLoad}, idxload={i}");
                        AdsHelper.onAdLoad(adpl.loadPl, "native_full", idLoad, "admob");
                        FIRhelper.logAdEvent("ads_fullnt_load");
                        AppsFlyerHelperScript.logAdEvent("ads_load", "", "native_full", idLoad, "admob", "");
                        adpl.adECPM.list[i].isLoading = true;
                        adpl.adECPM.list[i].isLoaded = false;
                        AdsAdmobMyBridge.Instance.loadNativeFull(adpl.placement, idLoad, i, (int)advhelper.bnOrien);
                    }
                }
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads fullnt {adpl.placement} admobmy tryLoadNativeFull not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadNativeFull(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlNtFull(placement, false);
            if (adpl == null)
            {
                SdkUtil.logd($"ads fullnt {placement} admobmy loadNativeFull not placement");
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
                            SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} admobmy loadNativeFull type={adsType}");
                            adpl.cbLoad = cb;
                            nativefullisnew = false;
                            adpl.countLoad = 0;
                            adpl.adECPM.idxCurrEcpm = 0;
                            adpl.setSetPlacementLoad(placement);
                            tryLoadNativeFull(adpl);
                        }
                        else
                        {
                            SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} admobmy loadNativeFull isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                        }
                    }
                    else
                    {
                        if (!adpl.isHighAdLoaded() && adpl.count4LoadAll <= 0)
                        {
                            SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} admobmy loadNativeFull all");
                            adpl.cbLoad = cb;
                            nativefullisnew = false;
                            adpl.count4LoadAll = 0;
                            adpl.adECPM.idxCurrEcpm = 0;
                            adpl.setSetPlacementLoad(placement);
                            tryLoadNativeFull(adpl);
                        }
                        else
                        {
                            SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} admobmy loadNativeFull isHighAdLoaded={adpl.isHighAdLoaded()} or count4LoadAll={adpl.count4LoadAll}");
                        }
                    }
                }
                else
                {
                    SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} admobmy loadNativeFull showing={adpl.getShowing()}");
                }
            }
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showNativeFull(string placement, float timeDelay, int timeNtDl, bool isHideBtClose, bool isShow2, int timeClose, bool isAutoCloseWhenClick, AdCallBack cb)
        {
            isFullNt2 = isShow2;
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlNtFull(placement, true);
            if (adpl != null)
            {
                //adpl.cbShow = null;
                int ss = getNativeFullLoaded(adpl.placement);
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
                        SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} admobmy showfull native delay:{tdelaycom} call show delay={timeDelay} ntdelay={timeNtDl}");
                        adpl.setShowing(true);
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} admobmy showfull native show!!!!!!");
                            AdsHelper.onAdShowStart(placement, "native_full", "admob", "");
                            bool iss = AdsAdmobMyBridge.Instance.showNativeFull(adpl.placement, !isHideBtClose, timeClose, timeNtDl, isAutoCloseWhenClick, layout, isShow2);
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
                        SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} admobmy showfull native call show delay={timeDelay} ntdelay={timeNtDl}");
                        AdsHelper.onAdShowStart(placement, "native_full", "admob", "");
                        bool iss = AdsAdmobMyBridge.Instance.showNativeFull(adpl.placement, !isHideBtClose, timeClose, timeNtDl, isAutoCloseWhenClick, layout, isShow2);
                        adpl.setShowing(iss);
                        return iss;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads fullnt {placement}-{adpl.placement} admobmy showfull native not load or showing={adpl.getShowing()}");
                }
            }
            else
            {
                SdkUtil.logd($"ads fullnt {placement} admobmy showfull native not pl");
            }
#endif
            return false;
        }
        public static void reCountNtFull()
        {
#if UNITY_IOS || UNITY_IPHONE
            AdsAdmobMyiOSBridge.reCountCurrShow();
#endif
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
        #region Full Native AD EVENTS
        private void OnNativeFullLoadedEvent(string placement, string adsId, string adnet)
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
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.loadPl}-{placement} admobmy OnNativeFullLoadedEvent plid={adpl.idPl}");
                string adsource = FIRhelper.getAdsourceAdmob(adnet);
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_full", adsId, "admob", adsource, true);
                adpl.countLoad = 0;
                adpl.isLoading = false;
                adpl.isloaded = true;
                nativefullisnew = false;
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
                SdkUtil.logd($"ads fullnt {placement} admobmy OnNativeFullLoadedEvent not pl");
            }
        }
        private void OnNativeFullFailedEvent(string placement, string adsId, string err)
        {
            FIRhelper.logAdEvent("ads_fullnt_load_0");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "native_full", adsId, "admob", "0");
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.loadPl}-{placement} admobmy onload fail=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_full", adsId, "admob", "", false);
                if ((flagLoadAll & 4) == 0)
                {
                    adpl.isLoading = false;
                    adpl.isloaded = false;
                    if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                    {
                        if (nativefullisnew)
                        {
                            nativefullisnew = false;
                            adpl.adECPM.idxCurrEcpm = 0;
                        }
                        else
                        {
                            adpl.adECPM.idxCurrEcpm++;
                        }
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            tryLoadNativeFull(adpl);
                        }, 1);
                    }
                    else
                    {
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            adpl.countLoad++;
                            tryLoadNativeFull(adpl);
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
                SdkUtil.logd($"ads fullnt {placement} admobmy onload fail=" + err);
            }
        }
        private void OnNativeFullDisplayedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.showPl}-{placement} admobmy onshow");
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads fullnt {placement} admobmy onshow not pl");
            }
        }
        private void OnNativeFullImpresstionEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads fullnt {placement} admobmy OnNativeFullImpresstionEvent");
        }
        private void onNativeFullFailedToShow(string placement, string adsId, string adnet, string err)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.showPl}-{placement} admobmy onNativeFullFailedToShow=" + err);
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
                SdkUtil.logd($"ads fullnt {placement} admobmy not pl onNativeFullFailedToShow=" + err);
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "native_full", "admob", adsource, adsId, false, err);
            onFullClose(placement);
            advhelper.onCloseFullGift(true);
        }
        private void OnNativeFullClickEvent(string placement, string adsId, string adnet)
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
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdClick(spl, "native_full", "admob", adsource, adsId);
            if (!fullntIsClick)
            {
                fullntIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "native_full", adsId, "admob", "");
            }
        }
        private void OnNativeFullDismissedEvent(string placement, string adsId, string adnet)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
                SdkUtil.logd($"ads fullnt {adpl.showPl}-{placement} admobmy OnNativeFullDismissedEvent id={adsId}");
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
                SdkUtil.logd($"ads fullnt {placement} admobmy onclose not pl");
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "native_full", "admob", adsource, adsId, true, "");
            onFullClose(placement);
            advhelper.onCloseFullGift(true);
        }
        private void OnNativeFullFinishShowEvent(string placement, string adsId, string err)
        {
            //advhelper.onCloseFullGift(true);
        }
        private void OnNativeFullAdPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            fullntIsClick = false;
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
                Debug.Log($"mysdk: ads fullnt {placement} admobmy onpaid v={valueMicros} perpost={FIRhelper.perPostAdsNtFull2}");
            }
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtFull[placement];
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
                Debug.Log($"mysdk: ads fullnt {spl} admobmy onpaid v={valueMicros} perpost={FIRhelper.perPostAdsNtSplash}");
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