using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
using UnityEngine.UI;
using GoogleMobileAds.Api;
#endif

namespace mygame.sdk
{
    partial class AdsAdmobMy
    {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
        Dictionary<AdLoader, Object4LoadNative> dicAdsNative = new Dictionary<AdLoader, Object4LoadNative>();
        List<AdsNativeAdmob> listAdNativeUse = new List<AdsNativeAdmob>();
        List<AdsNativeAdmob> listAdNativeFree = new List<AdsNativeAdmob>();
        Dictionary<string, AdsNativeMemAdmob> dicMemNew = new Dictionary<string, AdsNativeMemAdmob>();
#endif

        private string adnetLoaded = "";

        public void initNative()
        {
            if (adsType == 0)
            {
                try
                {
                    Debug.Log("mysdk: ads native admobmy init stepFloorECPMNative=" + advhelper.currConfig.adCfPlacementNative);
                    if (advhelper.currConfig.adCfPlacementNative.Length > 0)
                    {
                        string[] listpl = advhelper.currConfig.adCfPlacementNative.Split(new char[] { '#' });
                        foreach (string plitem in listpl)
                        {
                            addAdPlacement<AdPlacementNative>(dicPLNative, plitem, true);
                        }
                    }
                    string[] listpldf = AdIdsConfig.AdmobPlNative.Split(new char[] { '#' });
                    foreach (string plitem in listpldf)
                    {
                        addAdPlacement<AdPlacementNative>(dicPLNative, plitem, false);
                    }
                    if (dicPLNative.Count > 0)
                    {
                        List<string> listMore = new List<string>();
                        foreach (var item in dicPLNative)
                        {
                            if (!item.Key.EndsWith("_more"))
                            {
                                listMore.Add(item.Key);
                            }
                        }
                        foreach (var p in listMore)
                        {
                            string pmore = p + "_more";
                            if (!dicPLNative.ContainsKey(pmore))
                            {
                                AdPlacementNative admore = new AdPlacementNative();
                                admore.coppyFrom(dicPLNative[p]);
                                admore.placement = pmore;
                                dicPLNative.Add(pmore, admore);
                            }
                            else
                            {
                                AdPlacementNative admore = dicPLNative[pmore];
                                List<AdECPMItem> tmpl = new List<AdECPMItem>();
                                tmpl.AddRange(admore.adECPM.list);
                                admore.coppyFrom(dicPLNative[p]);
                                admore.placement = pmore;

                                for (int i = 0; i < tmpl.Count; i++)
                                {
                                    for (int j = 0; j < admore.adECPM.list.Count; j++)
                                    {
                                        if (admore.adECPM.list[j].adsId.CompareTo(tmpl[i].adsId) == 0)
                                        {
                                            admore.adECPM.list[j].coppyFrom(tmpl[i]);
                                            tmpl.RemoveAt(i);
                                            i--;
                                            break;
                                        }
                                    }
                                }
                                if (tmpl.Count > 0)
                                {
                                    admore.adECPM.list.AddRange(tmpl);
                                }
                                tmpl.Clear();
                            }
                        }
                    }
                    Debug.Log("mysdk: ads native admobmy init dicPLNative=" + dicPLNative.Count);
                }
                catch (Exception ex)
                {
                    Debug.Log($"mysdk: ads native admobmy initBanner ex=" + ex.ToString());
                }
            }
        }

        protected override void tryLoadNative(AdPlacementNative adpl, int idxId, AdsNativeObject ad, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
            if (adpl.adECPM.list.Count > 0 && idxId < adpl.adECPM.list.Count)
            {
                adpl.isLoading = true;
                int tmpidx = idxId;
                string idload = adpl.adECPM.list[tmpidx].adsId;
                SdkUtil.logd($"ads native {adpl.loadPl}-{adpl.placement} admobmy tryload id=" + idload);
                AdsHelper.onAdLoad(adpl.loadPl, "native_custome", idload, "admob");
                AdLoader adLoader = new AdLoader.Builder(idload)
                    .ForNativeAd()
                    .Build();
                if (ad != null)
                {
                    dicAdsNative.Add(adLoader, new Object4LoadNative(ad, cb));
                }
                adLoader.OnNativeAdLoaded += (sender, nativeArgs) => { this.HandleNativeAdLoaded(adpl.placement, idload, sender, nativeArgs); };
                adLoader.OnAdFailedToLoad += (sender, nativeFailArgs) => { this.HandleAdFailedToLoad(adpl.placement, tmpidx, idload, sender, nativeFailArgs); };
                adLoader.OnNativeAdImpression += (sender, evntArgs) => { this.HandleNativeAdImpression(adpl.placement, idload, sender, evntArgs); };
                adLoader.OnNativeAdOpening += (sender, evntArgs) => { this.HandleNativeAdOpening(adpl.placement, idload, sender, evntArgs); };
                adLoader.OnNativeAdClicked += (sender, evntArgs) => { this.HandleNativeAdClicked(adpl.loadPl, idload, sender, evntArgs); };
                adLoader.OnNativeAdClosed += (sender, evntArgs) => { this.HandleNativeAdClosed(adpl.placement, idload, sender, evntArgs); };
                adLoader.LoadAd(new AdRequest());
            }
            else
            {
                SdkUtil.logd($"ads native {adpl.loadPl}-{adpl.placement} admobmy tryload not pl pr not id idxId={idxId}");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
            }
#endif
        }
        public override void loadNative(string placement, AdsNativeObject ad, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
            AdPlacementNative adpl = getPlNt(placement);
            bool hasinwait = false;
            if (adpl != null && adpl.adECPM.list.Count > 0)
            {
                if (ad != null)
                {
                    foreach (KeyValuePair<AdLoader, Object4LoadNative> item in dicAdsNative)
                    {
                        if (ad.Equals(item.Value.adNative))
                        {
                            hasinwait = true;
                            break;
                        }
                    }
                }
                if (!hasinwait)
                {
                    SdkUtil.logd($"ads native {placement}-{adpl.placement} admobmy loadNative load");
                    adpl.setSetPlacementLoad(placement);
                    tryLoadNative(adpl, 0, ad, cb);
                }
                else
                {
                    SdkUtil.logd($"ads native {placement}-{adpl.placement} admobmy loadNative has in wait load");
                }
            }
            else
            {
                SdkUtil.logd($"ads native {placement} admobmy loadNative not pl pr not id");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
            }
#endif
        }

        public AdsNativeAdmob getNativeMem(string placement)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
            if (dicMemNew.ContainsKey(placement))
            {
                if (dicMemNew[placement].listAdNativeNew.Count > 0)
                {
                    SdkUtil.logd($"ads native {placement} admobmy getNativeMem");
                    AdsNativeAdmob ad = dicMemNew[placement].listAdNativeNew[0];
                    dicMemNew[placement].listAdNativeNew.RemoveAt(0);
                    return ad;
                }
            }
            string pmore = placement + "_more";
            if (dicMemNew.ContainsKey(pmore))
            {
                if (dicMemNew[pmore].listAdNativeNew.Count > 0)
                {
                    SdkUtil.logd($"ads native {placement} admobmy getNativeMem from placement more");
                    AdsNativeAdmob ad = dicMemNew[pmore].listAdNativeNew[0];
                    dicMemNew[pmore].listAdNativeNew.RemoveAt(0);
                    return ad;
                }
            }
            pmore = PLNtDefault + "_more";
            if (dicMemNew.ContainsKey(pmore))
            {
                if (dicMemNew[pmore].listAdNativeNew.Count > 0)
                {
                    SdkUtil.logd($"ads native {placement} admobmy getNativeMem from PLNtDefault more");
                    AdsNativeAdmob ad = dicMemNew[pmore].listAdNativeNew[0];
                    dicMemNew[pmore].listAdNativeNew.RemoveAt(0);
                    return ad;
                }
            }

            if (dicMemNew.ContainsKey(PLNtDefault))
            {
                if (dicMemNew[PLNtDefault].listAdNativeNew.Count > 0)
                {
                    SdkUtil.logd($"ads native {placement} admobmy getNativeMem from PLNtDefault");
                    AdsNativeAdmob ad = dicMemNew[PLNtDefault].listAdNativeNew[0];
                    dicMemNew[PLNtDefault].listAdNativeNew.RemoveAt(0);
                    return ad;
                }
            }
#endif
            return null;
        }
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
        public void save2Mem(string placement, AdsNativeAdmob ad, AdLoader adLoader)
        {
            if (ad != null)
            {
                AdsNativeMemAdmob admem = null;
                if (dicMemNew.ContainsKey(placement))
                {
                    admem = dicMemNew[placement];
                }
                else
                {
                    admem = new AdsNativeMemAdmob();
                    dicMemNew.Add(placement, admem);
                }
                adLoader.OnNativeAdClicked += (sender, evntArgs) => { this.HandleNativeAdClicked(placement, ad.adUnitId, sender, evntArgs); };
                adLoader.OnNativeAdClosed += (sender, evntArgs) => { this.HandleNativeAdClosed(placement, ad.adUnitId, sender, evntArgs); };
                admem.listAdNativeNew.Add(ad);
            }
        }
#endif

        public override bool showNative(string placement, AdsNativeObject ad, bool isRefresh, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
            SdkUtil.logd($"ads native {placement} admobmy showNative isRefresh={isRefresh}");
            if (ad == null)
            {
                SdkUtil.logd($"ads native {placement} admobmy showNative ad ob null");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return false;
            }
            AdsNativeAdmob adsnt = getNativeMem(placement);
            if (adsnt != null)
            {
                SdkUtil.logd($"ads native {placement} admobmy showNative get from memnew");
                listAdNativeUse.Add(adsnt);
                ad.adNative = adsnt;
                SdkUtil.logd($"ads native {placement} admobmy showNative get from memnew-{adsnt}");
                ad.pushNative2GameObject(placement, adsnt.nativeAd, cb);
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_OK);
                }
            }
            else
            {
                if (!isRefresh && (listAdNativeFree.Count > 0 || listAdNativeUse.Count > 0))
                {
                    SdkUtil.logd($"ads native {placement} admobmy showNative get from free and show");
                    if (listAdNativeFree.Count > 0)
                    {
                        adsnt = listAdNativeFree[0];
                        listAdNativeFree.RemoveAt(0);
                        listAdNativeUse.Add(adsnt);
                    }
                    else
                    {
                        adsnt = listAdNativeUse[0];
                    }
                    ad.adNative = adsnt;
                    //ad.pushNative2GameObject(placement, adsnt.nativeAd, cb);
                    ad.gameObject.SetActive(false);
                    if (cb != null)
                    {
                        cb(AD_State.AD_LOAD_FROM_EXIST);
                    }
                    //long t = SdkUtil.CurrentTimeMilis();
                    //if ((t - adsnt.tLoaded) >= 20000)
                    {
                        SdkUtil.logd($"ads native {placement} admobmy showNative get free and load new");
                        loadNative(placement, ad, cb);
                    }
                }
                else
                {
                    SdkUtil.logd($"ads native {placement} admobmy showNative not mem and load new");
                    ad.gameObject.SetActive(isRefresh);
                    loadNative(placement, ad, cb);
                }
            }
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
            return false;
        }
        public override void freeNative(AdsNativeObject ad)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
            if (ad != null)
            {
                if (ad.adNative != null && ad.adNative.nativeAd != null)
                {
                    SdkUtil.logd($"ads native admobmy freeNative use and free");
                    listAdNativeFree.Add(ad.adNative);
                    listAdNativeUse.Remove(ad.adNative);
                    if (listAdNativeFree.Count > 3)
                    {
                        var addf = listAdNativeFree[0];
                        listAdNativeFree.RemoveAt(0);
                        addf.nativeAd.Destroy();
                    }
                }

                foreach (KeyValuePair<AdLoader, Object4LoadNative> item in dicAdsNative)
                {
                    if (ad.Equals(item.Value.adNative))
                    {
                        SdkUtil.logd($"ads native admobmy freeNative ad free in list wait");
                        dicAdsNative.Remove(item.Key);
                        break;
                    }
                }
            }
#endif
        }
        public override void freeAllNative()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
            SdkUtil.logd($"ads native admobmy freeAllNative");
            listAdNativeFree.AddRange(listAdNativeUse);
            listAdNativeUse.Clear();
            dicAdsNative.Clear();
#endif
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
        #region native ads
        private void HandleNativeAdLoaded(string placement, string adsId, object sender, NativeAdEventArgs args)
        {
            AdPlacementNative adpl = getPlNt(placement);
            string gpl = "";
            if (adpl != null)
            {
                SdkUtil.logd($"ads native {adpl.loadPl}-{placement} admobmy HandleNativeAdLoaded has pl");
                string adsource = FIRhelper.getAdsourceAdmob(args.nativeAd.GetResponseInfo().GetMediationAdapterClassName());
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_custome", adsId, "admob", adsource, true);
                gpl = "" + adpl.loadPl + "-";
                adpl.isLoading = false;
            }
            else
            {
                SdkUtil.logd($"ads native {placement} admobmy HandleNativeAdLoaded not pl");
            }

            AdsNativeAdmob adnt = new AdsNativeAdmob();
            adnt.adUnitId = adsId;
            adnt.isLoaded = true;
            adnt.isLoading = false;
            adnt.nativeAd = args.nativeAd;
            adnt.tLoaded = SdkUtil.CurrentTimeMilis();
            adnetLoaded = adnt.nativeAd.GetResponseInfo().GetMediationAdapterClassName();
            adnt.nativeAd.OnPaidEvent += (sender, adValue) => {
                HandleNativeAdPaidEvent(placement, adsId, adnt.nativeAd.GetResponseInfo().GetMediationAdapterClassName(), adValue.AdValue.Value, adValue.AdValue.CurrencyCode, (int)adValue.AdValue.Precision);
            };
            if (dicAdsNative.ContainsKey((AdLoader)sender))
            {
                SdkUtil.logd($"ads native {gpl}{placement} admobmy HandleNativeAdLoaded has dic");
                Object4LoadNative obAd = dicAdsNative[(AdLoader)sender];
                dicAdsNative.Remove((AdLoader)sender);
                listAdNativeUse.Add(adnt);
                obAd.adNative.adNative = adnt;
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    if (obAd != null && obAd.adNative != null && adnt != null)
                    {
                        obAd.adNative.pushNative2GameObject(placement, adnt.nativeAd, obAd.cbLoad);
                    }
                }, 0.001f);
                if (dicMemNew.ContainsKey(PLNtDefault + "_more"))
                {
                    if (dicMemNew[PLNtDefault + "_more"].listAdNativeNew.Count <= 0)
                    {
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            loadNative(PLNtDefault + "_more", null, null);
                        }, 3f);
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads native {gpl}{placement} admobmy HandleNativeAdLoaded add mem new");
                AdsNativeMemAdmob admem = null;
                if (dicMemNew.ContainsKey(placement))
                {
                    admem = dicMemNew[placement];
                }
                else
                {
                    admem = new AdsNativeMemAdmob();
                    dicMemNew.Add(placement, admem);
                }
                admem.listAdNativeNew.Add(adnt);
            }
        }
        private void HandleAdFailedToLoad(string placement, int idxId, string adsId, object sender, AdFailedToLoadEventArgs args)
        {
            AdPlacementNative adpl = getPlNt(placement);
            if (adpl != null)
            {
                SdkUtil.logd($"ads native {adpl.loadPl}-{placement} admobmy HandleAdFailedToLoad=" + args.LoadAdError.GetMessage());
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_custome", adsId, "admob", "", false);
                adpl.isLoading = false;
                if (adpl.adECPM.list.Count > 0)
                {
                    if (dicAdsNative.ContainsKey((AdLoader)sender))
                    {
                        Object4LoadNative obAd = dicAdsNative[(AdLoader)sender];
                        dicAdsNative.Remove((AdLoader)sender);
                        int idx = idxId + 1;
                        if (idx < adpl.adECPM.list.Count)
                        {
                            tryLoadNative(adpl, idx, obAd.adNative, obAd.cbLoad);
                        }
                        else
                        {
                            if (obAd != null && obAd.cbLoad != null)
                            {
                                var tmpcb = obAd.cbLoad;
                                obAd.cbLoad = null;
                                tmpcb(AD_State.AD_LOAD_FAIL);
                            }
                        }
                    }
                    else
                    {
                        SdkUtil.logd($"ads native {adpl.loadPl}-{placement} admobmy HandleAdFailedToLoad dic not contain");
                        int idx = idxId + 1;
                        if (adpl.adECPM.idxCurrEcpm < adpl.adECPM.list.Count)
                        {
                            tryLoadNative(adpl, idx, null, null);
                        }
                    }
                }
                else
                {
                    if (dicAdsNative.ContainsKey((AdLoader)sender))
                    {
                        Object4LoadNative obAd = dicAdsNative[(AdLoader)sender];
                        dicAdsNative.Remove((AdLoader)sender);
                        if (obAd != null && obAd.cbLoad != null)
                        {
                            var tmpcb = obAd.cbLoad;
                            obAd.cbLoad = null;
                            tmpcb(AD_State.AD_LOAD_FAIL);
                        }
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads native {placement} admobmy HandleAdFailedToLoad not pl");
                if (dicAdsNative.ContainsKey((AdLoader)sender))
                {
                    Object4LoadNative obAd = dicAdsNative[(AdLoader)sender];
                    dicAdsNative.Remove((AdLoader)sender);
                    if (obAd != null && obAd.cbLoad != null)
                    {
                        var tmpcb = obAd.cbLoad;
                        obAd.cbLoad = null;
                        tmpcb(AD_State.AD_LOAD_FAIL);
                    }
                }
            }
        }
        private void HandleNativeAdPaidEvent(string placement, string adsId, string adnet, long adValue, string currentCode, int precision)
        {
            SdkUtil.logd($"ads native admobmy HandleNativeAdPaidEvent v={1000 * adValue}-{currentCode}-{precision}");
            FIRhelper.logEvent("show_ads_nt");
            FIRhelper.logEvent("show_ads_nt_0");
            string spl = SDKManager.Instance.currPlacement;
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            string adformat = FIRhelper.getAdformatAdmob(12);
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            var dicpr = TiktokBusiness.getAdmobParam(currencyCode);
            long vapost = 1000 * adValue;
            FIRhelper.logEventAdsPaidAdmob(spl, adformat, adsource, adsId, vapost, vapost, dicpr["currency_code"]);
            TiktokBusiness.logAdRevenueAdmob(adformat, adsource, adsId, precisionType, valueMicros / 1000, dicpr);
            float realValue = ((float)adValue) / 1000000.0f;
            AdsHelper.onAdImpression(spl, adsId, adformat, "admob", adsource, realValue, vapost);
        }
        private void HandleNativeAdImpression(string placement, string adsId, object sender, EventArgs args)
        {
            SdkUtil.logd($"ads native {placement} admobmy HandleNativeAdImpression id={adsId}");
        }
        private void HandleNativeAdOpening(string placement, string adsId, object sender, EventArgs args)
        {
            SdkUtil.logd($"ads native {placement} admobmy HandleNativeAdOpening id={adsId}");
        }
        private void HandleNativeAdClicked(string placement, string adsId, object sender, EventArgs args)
        {
            SdkUtil.logd($"ads native {placement} admobmy HandleNativeAdClicked id={adsId}");
            SDKManager.Instance.onClickAd();
            string adsource = FIRhelper.getAdsourceAdmob(adnetLoaded);
            AdsHelper.onAdClick(SDKManager.Instance.currPlacement, "native_custome", "admob", adsource, adsId);
        }
        private void HandleNativeAdClosed(string placement, string adsId, object sender, EventArgs args)
        {
            SdkUtil.logd($"ads native {placement} admobmy HandleNativeAdClosed id={adsId}");
        }
        #endregion
#endif
    }
}