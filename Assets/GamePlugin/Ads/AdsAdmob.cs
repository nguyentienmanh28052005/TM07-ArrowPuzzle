//#define ENABLE_ADS_ADMOB
//#define ENABLE_ADS_ADMOB_NATIVE
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

#if ENABLE_ADS_ADMOB && (ENABLE_ADS_ADMOB_NATIVE || USE_NATIVE_UNITY)
using GoogleMobileAds.Api;
#endif

namespace mygame.sdk
{
    public class AdsAdmob : AdsBase
    {
        long timeShowBanner = 0;
        float bnClDxcenter = 0;

        private bool waitReinitBanner = false;
        public static string stepFloorECPMNative = "";

        [HideInInspector] public bool bnIsFirInit = false;
        [HideInInspector] public bool fullIsFirInit = false;
        [HideInInspector] public bool giftIsFirInit = false;

#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
        private InterstitialAd full = null;
        private RewardedAd gift = null;

        float tShowBannerNm = -1;
        float tShowBannerCl = -1;
        float tShowBannerRect = -1;
        float tChangeCl2Nm = -1;
        int flagChangecl2Nm = 0;
        bool isShowingCollapse = false;

#if ENABLE_ADS_ADMOB_NATIVE
        Dictionary<AdLoader, Object4LoadNative> dicAdsNative = new Dictionary<AdLoader, Object4LoadNative>();
        List<AdsNativeAdmob> listAdNativeUse = new List<AdsNativeAdmob>();
        List<AdsNativeAdmob> listAdNativeFree = new List<AdsNativeAdmob>();
#endif

#endif

        public override void AdsAwake()
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            isEnable = true;
#endif
        }

        public override void InitAds()
        {
            initBanner();
            initNative();
            initFull();
            initGift();
            fullAdNetwork = "admob";
            giftAdNetwork = "admob";

#if ENABLE_ADS_ADMOB && (ENABLE_ADS_ADMOB_NATIVE || USE_NATIVE_UNITY)
            //Debug.Log("mysdk: admob version:" + GoogleMobileAds.Api.MobileAds.GetPlatformVersion().ToString());
#endif

#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            isEnable = true;
            if (adsType == 0)
            {
                MobileAds.RaiseAdEventsOnUnityMainThread = true;
                MobileAds.Initialize((initStatus) =>
                {
                    Dictionary<string, AdapterStatus> map = initStatus.getAdapterStatusMap();
                    foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
                    {
                        string className = keyValuePair.Key;
                        AdapterStatus status = keyValuePair.Value;
                        switch (status.InitializationState)
                        {
                            case AdapterState.NotReady:
                                // The adapter initialization did not complete.
                                SdkUtil.logd($"ads admob{adsType} Adapter: " + className + " not ready.");
                                break;
                            case AdapterState.Ready:
                                // The adapter was successfully initialized.
                                SdkUtil.logd($"ads admob{adsType} Adapter: " + className + " is initialized.");
                                break;
                        }
                    }
                    advhelper.onAdsInitSuc(0);
                    //                    RequestConfiguration requestConfiguration = new RequestConfiguration();
                    //                    if (requestConfiguration.TestDeviceIds == null)
                    //                    {
                    //                        requestConfiguration.TestDeviceIds = new List<string>();
                    //                    }
                    //                    requestConfiguration.TestDeviceIds.Add("9AAAEC2A7671ABDF39F852A213C73A77");
                    //                    MobileAds.SetRequestConfiguration(requestConfiguration);
                });
            }
#endif
        }

#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
        private void Update()
        {
            if (tShowBannerNm >= 0 && bannerInfo.isBNShow && advhelper.currConfig.timeAutoReloadBanner >= 5)
            {
                tShowBannerNm += Time.deltaTime * Time.timeScale;
                if (tShowBannerNm >= advhelper.currConfig.timeAutoReloadBanner)
                {
                    SdkUtil.logd("ads admob banner autoreload");
                    tShowBannerNm = -1;
                    StartCoroutine(waitLoadNextBanner());
                }
            }
            if (bannerClInfo.isBNShow)
            {
                if (flagChangecl2Nm == 2 && tChangeCl2Nm >= 0 && advhelper.currConfig.timeChangeCl2Banner >= 5)
                {
                    tChangeCl2Nm += Time.deltaTime * Time.timeScale;
                    if (tChangeCl2Nm >= advhelper.currConfig.timeChangeCl2Banner)
                    {
                        SdkUtil.logd("ads admob change collapse to banner");
                        tChangeCl2Nm = -1;
                        tShowBannerCl = -1;
                        advhelper.hideBannerCollapse();
                        advhelper.hideBanner(0);
                        if (bannerClInfo.posBanner == 0)
                        {
                            advhelper.showBanner(AD_BANNER_POS.TOP, advhelper.bnOrien, bnClWidth, 0);
                        }
                        else
                        {
                            advhelper.showBanner(AD_BANNER_POS.BOTTOM, advhelper.bnOrien, bnClWidth, 0);
                        }
                    }
                }
                if (tShowBannerCl >= 0 && advhelper.currConfig.timeAutoReloadBanner >= 5)
                {
                    tShowBannerCl += Time.deltaTime * Time.timeScale;
                    if (tShowBannerCl >= advhelper.currConfig.timeAutoReloadBanner)
                    {
                        SdkUtil.logd("ads admob banner collapse autoreload");
                        tShowBannerCl = -1;
                        StartCoroutine(waitLoadNextBannerCl());
                    }
                }
            }
            if (tShowBannerRect >= 0 && bannerRectInfo.isBNShow && advhelper.currConfig.timeAutoReloadBanner >= 5)
            {
                tShowBannerRect += Time.deltaTime * Time.timeScale;
                if (tShowBannerRect >= advhelper.currConfig.timeAutoReloadBanner)
                {
                    SdkUtil.logd("ads admob banner rect autoreload");
                    tShowBannerRect = -1;
                    StartCoroutine(waitLoadNextBannerRect());
                }
            }
        }
#endif
        public void checkFloorInit()
        {

        }

        public void initBanner()
        {
            if (adsType == 0)
            {
                var adpl = getPlBanner(advhelper.memPlacementBn, 0);
                if (adpl != null && adpl.isLoading)
                {
                    Debug.Log($"mysdk: ads admob{adsType} bn initBanner waitReinitBanner isprocessing");
                    waitReinitBanner = true;
                    return;
                }
                Debug.Log($"mysdk: ads admob{adsType} bn adCfPlacementBanner=" + advhelper.currConfig.adCfPlacementBanner);
                dicPLBanner.Clear();
                if (advhelper.currConfig.adCfPlacementBanner.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementBanner.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementBanner>(dicPLBanner, plitem, true);
                    }
                    string[] listpldf = AdIdsConfig.AdmobPlBanner.Split(new char[] { '#' });
                    foreach (string plitem in listpldf)
                    {
                        addAdPlacement<AdPlacementBanner>(dicPLBanner, plitem, false);
                    }
                }

                //collapse banner
                Debug.Log($"mysdk: ads admob{adsType} cl adCfPlacementCollapse=" + advhelper.currConfig.adCfPlacementCollapse);
                dicPLCl.Clear();
                if (advhelper.currConfig.adCfPlacementCollapse.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementCollapse.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementBanner>(dicPLCl, plitem, true);
                    }
                    string[] listpldf = AdIdsConfig.AdmobPlCl.Split(new char[] { '#' });
                    foreach (string plitem in listpldf)
                    {
                        addAdPlacement<AdPlacementBanner>(dicPLCl, plitem, false);
                    }
                }

                //Rect banner
                Debug.Log($"mysdk: ads admob{adsType} rect adCfPlacementRect=" + advhelper.currConfig.adCfPlacementRect);
                dicPLRect.Clear();
                if (advhelper.currConfig.adCfPlacementRect.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementRect.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementBanner>(dicPLRect, plitem, true);
                    }
                    string[] listpldf = AdIdsConfig.AdmobPlCl.Split(new char[] { '#' });
                    foreach (string plitem in listpldf)
                    {
                        addAdPlacement<AdPlacementBanner>(dicPLRect, plitem, false);
                    }
                }
            }
            else
            {
                dicPLBanner.Clear();
                AdPlacementBanner plbn = new AdPlacementBanner();
                dicPLBanner.Add(PLFullDefault, plbn);
                plbn.placement = PLFullDefault;
                plbn.adECPM.idxHighPriority = -1;
                plbn.adECPM.listFromDstring(bannerId);

                dicPLCl.Clear();
                AdPlacementBanner plcl = new AdPlacementBanner();
                dicPLCl.Add(PLFullDefault, plcl);
                plcl.placement = PLFullDefault;
                plcl.adECPM.idxHighPriority = -1;
                plcl.adECPM.listFromDstring(bannerCollapseId);

                dicPLRect.Clear();
            }
        }

        public void initNative()
        {
            if (adsType == 0)
            {
                try
                {
                    //Debug.Log("ads admobmy native init stepFloorECPMNative=" + stepFloorECPMNative);
                    //dicPLNative.Clear();
                    //if (stepFloorECPMNative.Length > 0)
                    //{
                    //    string[] listpl = stepFloorECPMNative.Split(new char[] { '#' });
                    //    foreach (string plitem in listpl)
                    //    {
                    //        addAdPlacement<AdPlacementNative>(dicPLNative, plitem);
                    //    }
                    //}
                    //string[] listpldf = AdIdsConfig.AdmobPlNative.Split(new char[] { '#' });
                    //foreach (string plitem in listpldf)
                    //{
                    //    addAdPlacement<AdPlacementNative>(dicPLNative, plitem);
                    //}
                }
                catch (Exception ex)
                {
                    Debug.Log($"mysdk: ads admob{adsType} initBanner ex=" + ex.ToString());
                }
            }
        }

        public void initFull()
        {
            dicPLFull.Clear();
            try
            {
                Debug.Log("ads admob init stepFloorECPMFull=" + advhelper.currConfig.adCfPlacementFull);
                if (advhelper.currConfig.adCfPlacementFull.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementFull.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacement<AdPlacementFull>(dicPLFull, plitem, true);
                    }
                }
                string[] listpldf = AdIdsConfig.AdmobPlFullAll.Split(new char[] { '#' });
                foreach (string plitem in listpldf)
                {
                    if (plitem.Contains(PLFullDefault))
                    {
                        addAdPlacement<AdPlacementFull>(dicPLFull, plitem, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ads admob initFull ex=" + ex.ToString());
            }
        }

        public void initGift()
        {
            dicPLGift.Clear();
            try
            {
                Debug.Log("ads admob init adCfPlacementGift=" + advhelper.currConfig.adCfPlacementGift);
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
                    if (plitem.Contains(PLGiftDefault))
                    {
                        addAdPlacement<AdPlacementFull>(dicPLGift, plitem, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ads admob initgift ex=" + ex.ToString());
            }
        }

        public override string getname()
        {
            return "adsmob";
        }

        //Banner
        IEnumerator waitLoadNextBanner(string placement)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            bannerInfo.isBNLoading = true;
            ListAdsECPM lecpm = listBannerGroup.list[listBannerGroup.idxList];
            lecpm.idxCurrEcpm = 0;
            tShowBannerNm = -1;

            yield return new WaitForSeconds(0.1f);
            tryLoadBanner(placement);
#else
            yield return null;
#endif
        }
        protected override void tryLoadBanner(AdPlacementBanner adpl)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            ListAdsECPM lecpm = listBannerGroup.list[listBannerGroup.idxList];
            string idload = lecpm.list[lecpm.idxCurrEcpm].adsid;

            SdkUtil.logd($"ads admob{adsType} tryLoadBanner = " + idload + ", idxList=" + listBannerGroup.idxList + ", idxCurrEcpm=" + lecpm.idxCurrEcpm);
            bannerInfo.isBNLoading = true;
            AdPosition adposbn;
            if (bannerInfo.posBanner == 0)
            {
                adposbn = AdPosition.Top;
            }
            else if (bannerInfo.posBanner == 1)
            {
                adposbn = AdPosition.Bottom;
            }
            else
            {
                adposbn = AdPosition.Bottom;
            }

            BannerView bannerView;
            if (bannerInfo.typeSize == 0)
            {

                SdkUtil.logd($"ads admob{adsType} tryLoadBanner banner size");

                if (SdkUtil.isiPad())
                {
                    bannerView = new BannerView(idload.Trim(), AdSize.Leaderboard, adposbn);
                }
                else
                {
                    bannerView = new BannerView(idload.Trim(), AdSize.Banner, adposbn);
                }
            }
            else if (bannerInfo.typeSize == 1)
            {

                SdkUtil.logd($"ads admob{adsType} tryLoadBanner smart");

                bannerView = new BannerView(idload.Trim(), AdSize.Banner, adposbn);
            }
            else
            {

                SdkUtil.logd($"ads admob{adsType} tryLoadBanner banner adaptive ");

                float widthInPixels = Screen.safeArea.width > 0 ? Screen.safeArea.width : Screen.width;
                int width = (int)(widthInPixels / MobileAds.Utils.GetDeviceScale());
                AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(width);

                bannerView = new BannerView(idload.Trim(), adaptiveSize, adposbn);
            }
            bannerView.OnBannerAdLoaded += () =>
            {
                HandleBannerAdLoaded(bannerView, idload);
            };
            bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                HandleBannerAdFailedToLoad(bannerView, idload, error);
            };
            bannerView.OnAdImpressionRecorded += HandleBannerAdImpressionRecorded;
            bannerView.OnAdClicked += HandleBannerAdClicked;
            bannerView.OnAdPaid += (value) =>
            {
                HandleBannerAdPaidEvent(value, 0);
            };
            bannerView.OnAdFullScreenContentOpened += HandleBannerAdFullScreenContentOpened;
            bannerView.OnAdFullScreenContentClosed += HandleBannerAdFullScreenContentClosed;
            var adRequest = new AdRequest();
            if (adRequest != null)
            {
                bannerView.LoadAd(adRequest);
            }
            else
            {
                adRequest = new AdRequest();
                if (adRequest != null)
                {
                    bannerView.LoadAd(adRequest);
                }
            }
            SdkUtil.logd($"ads admob{adsType} tryLoadBanner 3");
#else
            SdkUtil.logd($"ads admob{adsType} bn tryLoadFull not enable");
            var tmpcb = adpl.cbLoad;
            adpl.cbLoad = null;
            tmpcb(AD_State.AD_LOAD_FAIL);
#endif
        }
        public override void loadBanner(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            SdkUtil.logd($"ads admob{adsType} loadBanner");
            cbBanner = cb;
            if (!bannerInfo.isBNLoading)
            {
                BNTryLoad = 0;
                ListAdsECPM lecpm = listBannerGroup.list[listBannerGroup.idxList];
                lecpm.idxCurrEcpm = 0;
                if (lecpm.list.Count > 0)
                {
                    tryLoadBanner(placement);
                }
                else
                {
                    if (cb != null)
                    {
                        SdkUtil.logd($"ads admob{adsType} loadBanner not id");
                        cb(AD_State.AD_LOAD_FAIL);
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads admob{adsType} loadBanner is loading");
            }
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads admob{adsType} loadBanner not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showBanner(string placement, int pos, int width, int maxH, AdCallBack cb, float dxCenter, bool highP = false)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            SdkUtil.logd($"ads admob{adsType} showBanner");
            if (listBannerGroup.list.Count <= 0)
            {
                SdkUtil.logd($"ads admob{adsType} showBanner not id");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return false;
            }
            bannerInfo.isBNShow = true;
            bannerInfo.posBanner = pos;
            bnWidth = width;

            bool re = false;
            int idxrun = listBannerGroup.idxList;
            object bannercurr = null;
            BannerPlacement bnpl = null;
            for (int ii = 0; ii < listBannerGroup.list.Count; ii++)
            {
                ListAdsECPM lecpm = listBannerGroup.list[idxrun];
                for (int j = 0; j < lecpm.list.Count; j++)
                {
                    AdsECPM item = lecpm.list[j];
                    if (bannerInfo.dicBanner.ContainsKey(item.adsid))
                    {
                        bannercurr = bannerInfo.dicBanner[item.adsid].banner;
                        if (idxrun == listBannerGroup.idxList)
                        {
                            bnpl = bannerInfo.dicBanner[item.adsid];
                        }
                        break;
                    }
                    if (bannercurr != null)
                    {
                        break;
                    }
                }

                idxrun++;
                if (idxrun >= listBannerGroup.list.Count)
                {
                    idxrun = 0;
                }
            }

            if (bannercurr != null)
            {
                doshowBanner(bannercurr);
                if (cb != null)
                {
                    cb(AD_State.AD_SHOW);
                }
                re = true;
            }
            if (!bannerInfo.isBNLoading)
            {
                if (bnpl != null)
                {
                    long tcurr = SdkUtil.CurrentTimeMilis() / 1000;
                    int deltimechange = advhelper.currConfig.timeReloadBanner / listBannerGroup.list.Count;
                    if ((tcurr - bnpl.tShow) >= deltimechange)
                    {
                        listBannerGroup.idxList++;
                        if (listBannerGroup.idxList >= listBannerGroup.list.Count)
                        {
                            listBannerGroup.idxList = 0;
                        }
                        SdkUtil.logd($"ads admob{adsType} will reloa loadbanner");
                        loadBanner(cb);
                    }
                }
                else
                {
                    SdkUtil.logd($"ads admob{adsType} will loadbanner");
                    loadBanner(cb);
                }
            }
            else
            {
                SdkUtil.logd($"ads admob{adsType} showBanner isprocess");
                cbBanner = cb;
            }
            return re;
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads admob{adsType} tryLoadBanner not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
            return false;
#endif
        }
        void doshowBanner(object banner)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            SdkUtil.logd($"ads admob{adsType} doshowBanner");
            if (banner != null)
            {
                SdkUtil.logd($"ads admob{adsType} doshowBanner show");
                AdPosition adposbn;
                if (bannerInfo.posBanner == 0)
                {
                    adposbn = AdPosition.Top;
                }
                else if (bannerInfo.posBanner == 1)
                {
                    adposbn = AdPosition.Bottom;
                }
                else
                {
                    adposbn = AdPosition.Bottom;
                }
                if (banner != bannerInfo.banner && bannerInfo.banner != null)
                {
                    ((BannerView)bannerInfo.banner).Hide();
                }
                bannerInfo.isBNRealShow = true;
                bannerInfo.banner = banner;
                ((BannerView)bannerInfo.banner).SetPosition(adposbn);
                ((BannerView)bannerInfo.banner).Show();
            }
#endif
        }
        public override void hideBanner()
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            SdkUtil.logd($"ads admob{adsType} hideBanner");
            foreach (var adi in dicPLBanner)
            {
                adi.Value.isShow = false;
                adi.Value.isRealShow = false;
                
                if (bannerInfo.banner != null)
                {
                    ((BannerView)bannerInfo.banner).Hide();
                    bannerInfo.isBNRealShow = false;
                }
            }
#endif
        }
        public override void destroyBanner()
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            SdkUtil.logd($"ads admob{adsType} destroyBanner");
            foreach (var adi in dicPLBanner)
            {
                adi.Value.isShow = false;
                adi.Value.isRealShow = false;
                adi.Value.isloaded = false;
                if (bannerInfo.banner != null)
                {
                    ((BannerView)bannerInfo.banner).Hide();
                    bannerInfo.isBNRealShow = false;
                }
            }
#endif
        }
        //Collapse
        IEnumerator waitLoadNextBannerCl(string placement)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            bannerClInfo.isBNLoading = true;
            ListAdsECPM lecpm = listBannerClGroup.list[listBannerClGroup.idxList];
            lecpm.idxCurrEcpm = 0;
            tShowBannerCl = -1;

            yield return new WaitForSeconds(0.1f);
            tryLoadCollapseBanner(placement);
#else
            yield return null;
#endif
        }
        protected override void tryLoadCollapseBanner(AdPlacementBanner adpl)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            ListAdsECPM lecpm = listBannerClGroup.list[listBannerClGroup.idxList];
            string idload = lecpm.list[lecpm.idxCurrEcpm].adsid;
            SdkUtil.logd($"ads admob{adsType} tryLoadCollapseBanner = " + idload + ", idxList=" + listBannerClGroup.idxList + ", idxCurrEcpm=" + lecpm.idxCurrEcpm);
            bannerClInfo.isBNLoading = true;
            AdPosition adposbn;
            if (bannerClInfo.posBanner == 0)
            {
                adposbn = AdPosition.Top;
            }
            else if (bannerClInfo.posBanner == 1)
            {
                adposbn = AdPosition.Bottom;
            }
            else
            {
                adposbn = AdPosition.Bottom;
            }

            BannerView bannerView;
            if (SdkUtil.isiPad())
            {
                bannerView = new BannerView(idload.Trim(), AdSize.Leaderboard, adposbn);
            }
            else
            {
                bannerView = new BannerView(idload.Trim(), AdSize.Banner, adposbn);
            }
            bannerView.OnBannerAdLoaded += () =>
            {
                HandleBannerClAdLoaded(bannerView, idload);
            };
            bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                HandleBannerClAdFailedToLoad(bannerView, idload, error);
            };
            bannerView.OnAdImpressionRecorded += HandleBannerAdImpressionRecorded;
            bannerView.OnAdClicked += HandleBannerAdClicked;
            bannerView.OnAdPaid += (value) =>
            {
                HandleBannerAdPaidEvent(value, 1);
            };
            bannerView.OnAdFullScreenContentOpened += HandleBannerAdFullScreenContentOpened;
            bannerView.OnAdFullScreenContentClosed += HandleBannerAdFullScreenContentClosed;
            var adRequest = new AdRequest();
            if (bannerClInfo.posBanner == 0)
            {
                adRequest.Extras.Add("collapsible", "top");
                //GoogleMobileAds.Api.Mediation.MediationExtras mextra = new MyAdmobMediationExtras("collapsible", "top");
                //adRequest.MediationExtras.Add(new GoogleMobileAds.Api.Mediation.MediationExtras());
            }
            else if (bannerClInfo.posBanner == 1)
            {
                adRequest.Extras.Add("collapsible", "bottom");
            }
            else
            {
                adRequest.Extras.Add("collapsible", "bottom");
            }

            bannerView.LoadAd(adRequest);
            SdkUtil.logd($"ads admob{adsType} tryLoadCollapseBanner 3");
#else
            SdkUtil.logd($"ads admob{adsType} bn tryLoadFull not enable");
            var tmpcb = adpl.cbLoad;
            adpl.cbLoad = null;
            tmpcb(AD_State.AD_LOAD_FAIL);
#endif
        }
        public override void loadCollapseBanner(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            SdkUtil.logd($"ads admob{adsType} loadCollapseBanner");
            cbBanner = cb;
            if (!bannerInfo.isBNLoading)
            {
                BNTryLoadCollapse = 0;
                ListAdsECPM lecpm = listBannerGroup.list[listBannerGroup.idxList];
                lecpm.idxCurrEcpm = 0;
                if (lecpm.list.Count > 0)
                {
                    tryLoadCollapseBanner(placement);
                }
                else
                {
                    if (cb != null)
                    {
                        SdkUtil.logd($"ads admob{adsType} loadCollapseBanner not id");
                        cb(AD_State.AD_LOAD_FAIL);
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads admob{adsType} loadCollapseBanner is loading");
            }
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads admob{adsType} loadCollapseBanner not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showCollapseBanner(string placement, int pos, int width, int maxH, float dxCenter, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            SdkUtil.logd($"ads admob{adsType} showCollapseBanner");
            if (listBannerClGroup.list.Count <= 0)
            {
                SdkUtil.logd($"ads admob{adsType} showCollapseBanner not id");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return false;
            }
            bannerClInfo.isBNShow = true;
            bannerClInfo.posBanner = pos;
            bnClWidth = width;
            bnClDxcenter = dxCenter;
            flagChangecl2Nm = advhelper.getCfCollapsePlacement(placement, -1);
            bool re = false;
            int idxrun = listBannerClGroup.idxList;
            object bannercurr = null;
            BannerPlacement bnpl = null;
            for (int ii = 0; ii < listBannerClGroup.list.Count; ii++)
            {
                ListAdsECPM lecpm = listBannerClGroup.list[idxrun];
                for (int j = 0; j < lecpm.list.Count; j++)
                {
                    AdsECPM item = lecpm.list[j];
                    if (bannerClInfo.dicBanner.ContainsKey(item.adsid))
                    {
                        bannercurr = bannerClInfo.dicBanner[item.adsid].banner;
                        if (idxrun == listBannerClGroup.idxList)
                        {
                            bnpl = bannerClInfo.dicBanner[item.adsid];
                        }
                        break;
                    }
                    if (bannercurr != null)
                    {
                        break;
                    }
                }

                idxrun++;
                if (idxrun >= listBannerClGroup.list.Count)
                {
                    idxrun = 0;
                }
            }

            if (bannercurr != null)
            {
                if (tChangeCl2Nm > 0)
                {
                    tChangeCl2Nm = 0;
                }
                doshowBannerCl(bannercurr);
                if (cb != null)
                {
                    cb(AD_State.AD_SHOW);
                }
                re = true;
            }
            if (!bannerClInfo.isBNLoading)
            {
                if (bnpl != null)
                {
                    long tcurr = SdkUtil.CurrentTimeMilis() / 1000;
                    int deltimechange = advhelper.currConfig.timeReloadBanner / listBannerClGroup.list.Count;
                    if ((tcurr - bnpl.tShow) >= deltimechange)
                    {
                        listBannerClGroup.idxList++;
                        if (listBannerClGroup.idxList >= listBannerClGroup.list.Count)
                        {
                            listBannerClGroup.idxList = 0;
                        }
                        SdkUtil.logd($"ads admob{adsType} showBannerCl will reloa loadbanner");
                        loadCollapseBanner(cb);
                    }
                }
                else
                {
                    SdkUtil.logd($"ads admob{adsType} showBannerCl will loadbanner Cl");
                    loadCollapseBanner(cb);
                }
            }
            else
            {
                SdkUtil.logd($"ads admob{adsType} showBannerCl isprocess");
                cbBanner = cb;
            }
            return re;
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads admob{adsType} showBannerCl not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
            return false;
#endif
        }
        void doshowBannerCl(object banner)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            SdkUtil.logd($"ads admob{adsType} doshowBannerCl");
            if (banner != null)
            {
                SdkUtil.logd($"ads admob{adsType} doshowBannerCl show");
                AdPosition adposbn;
                if (bannerClInfo.posBanner == 0)
                {
                    adposbn = AdPosition.Top;
                }
                else if (bannerClInfo.posBanner == 1)
                {
                    adposbn = AdPosition.Bottom;
                }
                else
                {
                    adposbn = AdPosition.Bottom;
                }
                if (banner != bannerClInfo.banner && bannerClInfo.banner != null)
                {
                    ((BannerView)bannerClInfo.banner).Hide();
                }
                bannerClInfo.isBNRealShow = true;
                bannerClInfo.banner = banner;

                isShowingCollapse = true;
                ((BannerView)bannerClInfo.banner).SetPosition(adposbn);
                ((BannerView)bannerClInfo.banner).Show();
            }
#endif
        }
        public override void hideCollapseBanner()
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            if (PlayerPrefs.GetInt("cf_close_bncl", 0) == 0)
            {
                bannerClInfo.isBNShow = false;
                SdkUtil.logd($"ads admob{adsType} hideCollapseBanner call hide");
                isShowingCollapse = false;
                if (bannerClInfo.banner != null)
                {
                    ((BannerView)bannerClInfo.banner).Hide();
                    bannerClInfo.isBNRealShow = false;
                }
            }
            else
            {
                bannerClInfo.isBNShow = false;
                SdkUtil.logd($"ads admob{adsType} hideCollapseBanner call destroy");
                isShowingCollapse = false;
                if (bannerClInfo.banner != null)
                {
                    ((BannerView)bannerClInfo.banner).Hide();
                    ((BannerView)bannerClInfo.banner).Destroy();
                    foreach (KeyValuePair<string, BannerPlacement> entry in bannerClInfo.dicBanner)
                    {
                        if (entry.Value.banner.Equals(bannerClInfo.banner))
                        {
                            bannerClInfo.dicBanner.Remove(entry.Key);
                            break;
                        }
                    }
                    bannerClInfo.banner = null;
                    bannerClInfo.isBNRealShow = false;
                }
            }
#endif
        }
        public override void destroyCollapseBanner()
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            bannerClInfo.isBNShow = false;
            SdkUtil.logd($"ads admob{adsType} destroyCollapseBanner");
            isShowingCollapse = false;
            if (bannerClInfo.banner != null)
            {
                ((BannerView)bannerClInfo.banner).Hide();
                bannerClInfo.isBNRealShow = false;
            }
#endif
        }
        //Rect
        IEnumerator waitLoadNextBannerRect(string placement)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            bannerRectInfo.isBNLoading = true;
            ListAdsECPM lecpm = listBannerRectGroup.list[listBannerRectGroup.idxList];
            lecpm.idxCurrEcpm = 0;
            tShowBannerRect = -1;
            yield return new WaitForSeconds(0.1f);
            tryLoadRectBanner(placement);
#else
            yield return null;
#endif
        }
        protected override void tryLoadRectBanner(AdPlacementBanner adpl)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            ListAdsECPM lecpm = listBannerRectGroup.list[listBannerRectGroup.idxList];
            string idload = lecpm.list[lecpm.idxCurrEcpm].adsid;

            SdkUtil.logd($"ads admob{adsType} tryLoadRectBanner = " + idload + ", idxList=" + listBannerRectGroup.idxList + ", idxCurrEcpm=" + lecpm.idxCurrEcpm);
            bannerRectInfo.isBNLoading = true;
            AdPosition adposbn;
            if (bannerRectInfo.posBanner == 0)
            {
                adposbn = AdPosition.Top;
            }
            else if (bannerRectInfo.posBanner == 1)
            {
                adposbn = AdPosition.Bottom;
            }
            else
            {
                adposbn = AdPosition.Bottom;
            }

            BannerView bannerView = new BannerView(idload.Trim(), AdSize.MediumRectangle, adposbn);
            bannerView.OnBannerAdLoaded += () =>
            {
                HandleBannerRectAdLoaded(bannerView, idload);
            };
            bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                HandleBannerRectAdFailedToLoad(bannerView, idload, error);
            };
            bannerView.OnAdImpressionRecorded += HandleBannerAdImpressionRecorded;
            bannerView.OnAdClicked += HandleBannerAdClicked;
            bannerView.OnAdPaid += (value) =>
            {
                HandleBannerAdPaidEvent(value, 2);
            };
            bannerView.OnAdFullScreenContentOpened += HandleBannerAdFullScreenContentOpened;
            bannerView.OnAdFullScreenContentClosed += HandleBannerAdFullScreenContentClosed;
            var adRequest = new AdRequest();
            if (adRequest != null)
            {
                bannerView.LoadAd(adRequest);
            }
            else
            {
                adRequest = new AdRequest();
                if (adRequest != null)
                {
                    bannerView.LoadAd(adRequest);
                }
            }
            SdkUtil.logd($"ads admob{adsType} tryLoadRectBanner 3");
#else
            SdkUtil.logd($"ads admob{adsType} bn tryLoadFull not enable");
            var tmpcb = adpl.cbLoad;
            adpl.cbLoad = null;
            tmpcb(AD_State.AD_LOAD_FAIL);
#endif
        }
        public override void loadRectBanner(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            SdkUtil.logd($"ads admob{adsType} loadRectBanner");
            cbBanner = cb;
            if (!bannerInfo.isBNLoading)
            {
                BNTryLoadRect = 0;
                ListAdsECPM lecpm = listBannerGroup.list[listBannerGroup.idxList];
                lecpm.idxCurrEcpm = 0;
                if (lecpm.list.Count > 0)
                {
                    tryLoadRectBanner(placement);
                }
                else
                {
                    if (cb != null)
                    {
                        SdkUtil.logd($"ads admob{adsType} loadRectBanner not id");
                        cb(AD_State.AD_LOAD_FAIL);
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads admob{adsType} loadRectBanner is loading");
            }
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads admob{adsType} loadRectBanner not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showRectBanner(string placement, int pos, float width, int maxH, float dxCenter, float dyVertical, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            SdkUtil.logd($"ads admob{adsType} showRectBanner");
            if (listBannerRectGroup.list.Count <= 0)
            {
                SdkUtil.logd($"ads admob{adsType} showRectBanner not id");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return false;
            }
            bannerRectInfo.isBNShow = true;
            bannerRectInfo.posBanner = pos;
            bnRectWidth = width;
            bool re = false;
            int idxrun = listBannerRectGroup.idxList;
            object bannercurr = null;
            BannerPlacement bnpl = null;
            for (int ii = 0; ii < listBannerRectGroup.list.Count; ii++)
            {
                ListAdsECPM lecpm = listBannerRectGroup.list[idxrun];
                for (int j = 0; j < lecpm.list.Count; j++)
                {
                    AdsECPM item = lecpm.list[j];
                    if (bannerRectInfo.dicBanner.ContainsKey(item.adsid))
                    {
                        bannercurr = bannerRectInfo.dicBanner[item.adsid].banner;
                        if (idxrun == listBannerRectGroup.idxList)
                        {
                            bnpl = bannerRectInfo.dicBanner[item.adsid];
                        }
                        break;
                    }
                    if (bannercurr != null)
                    {
                        break;
                    }
                }

                idxrun++;
                if (idxrun >= listBannerRectGroup.list.Count)
                {
                    idxrun = 0;
                }
            }

            if (bannercurr != null)
            {
                doshowBannerRect(bannercurr);
                if (cb != null)
                {
                    cb(AD_State.AD_SHOW);
                }
                re = true;
            }
            if (!bannerRectInfo.isBNLoading)
            {
                if (bnpl != null)
                {
                    long tcurr = SdkUtil.CurrentTimeMilis() / 1000;
                    int deltimechange = advhelper.currConfig.timeReloadBanner / listBannerRectGroup.list.Count;
                    if ((tcurr - bnpl.tShow) >= deltimechange)
                    {
                        listBannerRectGroup.idxList++;
                        if (listBannerRectGroup.idxList >= listBannerRectGroup.list.Count)
                        {
                            listBannerRectGroup.idxList = 0;
                        }
                        SdkUtil.logd($"ads admob{adsType} showRectBanner will reloa loadbanner");
                        loadRectBanner(cb);
                    }
                }
                else
                {
                    SdkUtil.logd($"ads admob{adsType} showRectBanner will loadbanner");
                    loadRectBanner(cb);
                }
            }
            else
            {
                SdkUtil.logd($"ads admob{adsType} showRectBanner isprocess");
                cbBanner = cb;
            }
            return re;
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads admob{adsType} showRectBanner not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
            return false;
#endif
        }
        void doshowBannerRect(object banner)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            SdkUtil.logd($"ads admob{adsType} doshowBannerRect");
            if (banner != null)
            {
                SdkUtil.logd($"ads admob{adsType} doshowBannerRect show");
                AdPosition adposbn;
                if (bannerRectInfo.posBanner == 0)
                {
                    adposbn = AdPosition.Top;
                }
                else if (bannerRectInfo.posBanner == 1)
                {
                    adposbn = AdPosition.Bottom;
                }
                else
                {
                    adposbn = AdPosition.Bottom;
                }
                if (banner != bannerRectInfo.banner && bannerRectInfo.banner != null)
                {
                    ((BannerView)bannerRectInfo.banner).Hide();
                }
                bannerRectInfo.isBNRealShow = true;
                bannerRectInfo.banner = banner;
                ((BannerView)bannerRectInfo.banner).SetPosition(adposbn);
                ((BannerView)bannerRectInfo.banner).Show();
            }
#endif
        }
        public override void hideRectBanner()
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            bannerRectInfo.isBNShow = false;
            SdkUtil.logd($"ads admob{adsType} hideRectBanner");
            if (bannerRectInfo.banner != null)
            {
                ((BannerView)bannerRectInfo.banner).Hide();
                bannerRectInfo.isBNRealShow = false;
            }
#endif
        }
        public override void destroyRectBanner()
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            bannerRectInfo.isBNShow = false;
            SdkUtil.logd($"ads admob{adsType} destroyRectBanner");
            if (bannerRectInfo.banner != null)
            {
                ((BannerView)bannerRectInfo.banner).Hide();
                bannerRectInfo.isBNRealShow = false;
            }
#endif
        }

        //Native
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY && ENABLE_ADS_ADMOB_NATIVE
        private void pushNative2GameObject(Object4LoadNative ob, NativeAd nativeAd)
        {
            if (ob != null && ob.gameObject != null && nativeAd != null)
            {
                Texture2D iconTexture = nativeAd.GetIconTexture();
                if (iconTexture != null)
                {
                    SdkUtil.logd($"ads admob{adsType} pushNative2GameObject load ok and has texture");
                    ob.gameObject.transform.Find("Icon").gameObject.SetActive(true);
                    ob.gameObject.transform.Find("Icon").GetComponent<Image>().sprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), Vector2.zero);
                    ob.gameObject.transform.Find("Headline").GetComponent<Text>().text = nativeAd.GetHeadlineText();
                    ob.gameObject.transform.Find("BodyTxt").GetComponent<Text>().text = nativeAd.GetBodyText();

                    // Register GameObject that will display icon asset of native ad.
                    if (!nativeAd.RegisterIconImageGameObject(ob.gameObject))
                    {
                        SdkUtil.logd($"ads admob{adsType} pushNative2GameObject RegisterIconImageGameObject");
                    }
                    if (ob.cbLoad != null)
                    {
                        var tmpcb = ob.cbLoad;
                        ob.cbLoad = null;
                        tmpcb(AD_State.AD_LOAD_OK);
                    }
                }
                else
                {
                    ob.gameObject.transform.Find("Icon").gameObject.SetActive(false);
                    if (ob.cbLoad != null)
                    {
                        var tmpcb = ob.cbLoad;
                        ob.cbLoad = null;
                        tmpcb(AD_State.AD_LOAD_FAIL);
                    }
                    else
                    {
                        SdkUtil.logd($"ads admob{adsType} pushNative2GameObject AD_LOAD_FAIL and not cb");
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads admob{adsType} pushNative2GameObject ob = null || ob.gameObject = null || nativeAd = null");
            }
        }
#endif
        protected override void tryLoadNative(AdPlacementNative adpl, int idxId, AdsNativeObject ad, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB_NATIVE
#endif
        }
        public override void loadNative(string placement, AdsNativeObject ad, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB_NATIVE
            AdLoader adLoader = new AdLoader.Builder(nativeId)
                .ForNativeAd()
                .Build();
            dicAdsNative.Add(adLoader, new Object4LoadNative(obAds, cb));
            adLoader.OnNativeAdLoaded += this.HandleNativeAdLoaded;
            adLoader.OnAdFailedToLoad += this.HandleAdFailedToLoad;
            adLoader.LoadAd(new AdRequest());
#endif
        }
        public override bool showNative(string placement, AdsNativeObject ad, bool isRefresh, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB_NATIVE
            if (obAds == null)
            {
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return false;
            }
            if (listAdNativeFree.Count > 0)
            {
                AdsNativeAdmob adsnt = listAdNativeFree[0];
                listAdNativeFree.RemoveAt(0);
                listAdNativeUse.Add(adsnt);
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_OK);
                }
            }
            else
            {
                loadNative(obAds, cb);
            }
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
            return false;
        }
        public override void freeNative(AdsNativeObject ads)
        {
#if ENABLE_ADS_ADMOB_NATIVE
            listAdNativeFree.Add(ads);
            listAdNativeUse.Remove(ads);
#endif
        }
        public override void freeAllNative()
        {
#if ENABLE_ADS_ADMOB_NATIVE
            listAdNativeFree.AddRange(listAdNativeUse);
            listAdNativeUse.Clear();
#endif
        }

        //Full
        public override int getFullLoaded(string placement)
        {
            int re = 0;
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            SdkUtil.logd($"ads admob{adsType} full getFullLoaded");
            if (full != null && isFullLoaded && full.CanShowAd())
            {
                re = 1;
            }
#endif
            return re;
        }
        public override void clearCurrFull(string placement)
        {
            if (getFullLoaded(placement) == 1)
            {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
                full.Destroy();
                full = null;
                isFullLoaded = false;
#endif
            }
        }
        protected override void tryLoadFull(AdPlacementFull adpl)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            if (listFullEcpm.idxCurrEcpm >= listFullEcpm.list.Count)
            {
                listFullEcpm.idxCurrEcpm = 0;
            }
            string idLoad = listFullEcpm.list[listFullEcpm.idxCurrEcpm].adsid;
            SdkUtil.logd($"ads admob{adsType} full tryLoadFull load idLoad=" + idLoad + "listFullEcpm = " + listFullEcpm.list.Count + ", listFullEcpm.idxCurrEcpm = " + listFullEcpm.idxCurrEcpm);
            isFullLoading = true;
            isFullLoaded = false;
            if (full != null)
            {
                full.Destroy();
                full = null;
            }

#if ADMOB_VUNG
            VungleInterstitialMediationExtras extras = new VungleInterstitialMediationExtras();
#if UNITY_ANDROID
            extras.SetAllPlacements(new string[] { "ANDROID_PLACEMENT_1", "ANDROID_PLACEMENT_2" });
#elif UNITY_IOS || UNITY_IPHONE
            extras.SetAllPlacements(new string[] { "IOS_PLACEMENT_1", "IOS_PLACEMENT_2" });
#endif

            var adRequest = new AdRequest();
            adRequest.MediationExtras = extras;
#else
            var adRequest = new AdRequest();
#endif
            InterstitialAd.Load(idLoad.Trim(), adRequest, (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    full = null;
                    HandleInterstitialFailedToLoad(error);
                }
                else
                {
                    full = ad;
                    HandleInterstitialLoaded();
                }
            });
#else
            SdkUtil.logd($"ads admob{adsType} full tryLoadFull not enable");
            var tmpcb = adpl.cbLoad;
            adpl.cbLoad = null;
            tmpcb(AD_State.AD_LOAD_FAIL);
#endif
        }
        public override void loadFull(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            SdkUtil.logd($"ads admob{adsType} full loadFull type=" + adsType);
            cbFullLoad = cb;
            if (!isFullLoading && !isFullLoaded)
            {
                FullTryLoad = 0;
                listFullEcpm.idxCurrEcpm = 0;
                if (listFullEcpm.list.Count > 0)
                {
                    tryLoadFull(placement);
                }
                else
                {
                    SdkUtil.logd($"ads admob{adsType} full loadFull not id");
                    if (cbFullLoad != null)
                    {
                        var tmpcb = cbFullLoad;
                        cbFullLoad = null;
                        tmpcb(AD_State.AD_LOAD_FAIL);
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads admob{adsType} full loadFull isFullLoading={isFullLoading} isFullLoaded={isFullLoaded}");
            }
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showFull(string placement, float timeDelay, bool isShow2, AdCallBack cb)
        {
            isFull2 = isShow2;
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            SdkUtil.logd($"ads admob{adsType} full showFull type=" + adsType);
            cbFullShow = null;
            int ss = getFullLoaded(placement);
            if (ss > 0)
            {

                SdkUtil.logd($"ads admob{adsType} full showFull type=" + adsType);

                FullTryLoad = 0;
                cbFullShow = cb;
                full.OnAdImpressionRecorded += HandleInterstitialImpressionRecorded;
                full.OnAdFullScreenContentOpened += HandleInterstitialOpened;
                full.OnAdClicked += HandleInterstitialClicked;
                full.OnAdFullScreenContentFailed += HandleInterstitialFailedToShow;
                full.OnAdFullScreenContentClosed += HandleInterstitialClosed;
                full.OnAdPaid += HandleInterstitialPaidEvent;
                full.Show();
                return true;
            }
#endif
            return false;
        }

        //Gift
        public override void clearCurrGift(string placement)
        {
            if (getGiftLoaded(placement) == 1)
            {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
                gift.Destroy();
                gift = null;
                isGiftLoaded = false;
#endif
            }
        }
        public override int getGiftLoaded(string placement)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            if (isGiftLoaded && gift != null && gift.CanShowAd())
            {
                return 1;
            }
#endif
            return 0;
        }
        protected override void tryloadGift(AdPlacementFull adpl)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            if (listGiftEcpm.idxCurrEcpm >= listGiftEcpm.list.Count)
            {
                listGiftEcpm.idxCurrEcpm = 0;
            }
            string idLoad = listGiftEcpm.list[listGiftEcpm.idxCurrEcpm].adsid;
            SdkUtil.logd($"ads admob{adsType} gift tryloadGift =" + idLoad + ", idxCurrEcpmGift=" + listGiftEcpm.idxCurrEcpm);
            if (gift != null)
            {
                gift.Destroy();
                gift = null;
            }
            isGiftLoading = true;
            isGiftLoaded = false;

#if ADMOB_VUNG
            VungleRewardedVideoMediationExtras extras = new VungleRewardedVideoMediationExtras();
#if UNITY_ANDROID
            extras.SetAllPlacements(new string[] { "ANDROID_PLACEMENT_1", "ANDROID_PLACEMENT_2" });
#elif UNITY_IOS || UNITY_IPHONE
            extras.SetAllPlacements(new string[] { "IOS_PLACEMENT_1", "IOS_PLACEMENT_2" });
#endif

            var adRequest = new AdRequest();
            adRequest.MediationExtras = extras;
#else
            var adRequest = new AdRequest();
#endif
            RewardedAd.Load(idLoad.Trim(), adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    gift = null;
                    HandleRewardBasedVideoFailedToLoad(error);
                }
                else
                {
                    gift = ad;
                    HandleRewardBasedVideoLoaded();
                }
            });
#else
            SdkUtil.logd($"ads admob{adsType} gift tryloadGift not enable");
            var tmpcb = adpl.cbLoad;
            adpl.cbLoad = null;
            tmpcb(AD_State.AD_LOAD_FAIL);
#endif
        }
        public override void loadGift(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            SdkUtil.logd($"ads admob{adsType} gift loadGift");
            cbGiftLoad = cb;
            if (!isGiftLoading && !isGiftLoaded)
            {
                GiftTryLoad = 0;
                listGiftEcpm.idxCurrEcpm = 0;
                if (listGiftEcpm.list.Count > 0)
                {
                    tryloadGift(placement);
                }
                else
                {
                    SdkUtil.logd($"ads admob{adsType} gift loadGift not id");
                    if (cbGiftLoad != null)
                    {
                        var tmpcb = cbGiftLoad;
                        cbGiftLoad = null;
                        tmpcb(AD_State.AD_LOAD_FAIL);
                    }
                }
            }
            else
            {

                SdkUtil.logd($"ads admob{adsType} loadGift loading={isGiftLoading} or loaded={isGiftLoaded}");

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
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            SdkUtil.logd($"ads admob{adsType} gift showGift");
            cbGiftShow = null;
            if (getGiftLoaded() > 0)
            {
                cbGiftShow = cb;
                gift.OnAdImpressionRecorded += HandleRewardBasedVideoImpressionRecorded;
                gift.OnAdFullScreenContentOpened += HandleRewardBasedVideoOpened;
                gift.OnAdClicked += HandleRewardBasedVideoClicked;
                gift.OnAdFullScreenContentFailed += HandleRewardBasedVideoFailToShow;
                gift.OnAdFullScreenContentClosed += HandleRewardBasedVideoClosed;
                gift.OnAdPaid += HandleRewardBasedPaidEvent;
                gift.Show((Reward reward) =>
                {
                    HandleRewardBasedVideoRewarded();
                });
                return true;
        }
#endif
            return false;
        }

        //-------------------------------------------------------------------------------
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY

        #region BANNER AD EVENTS
        private void HandleBannerAdLoaded(BannerView bannerView, string adsId)
        {
            SdkUtil.logd($"ads admob{adsType} HandleBannerAdLoaded ads=" + adsId);
            if (bannerInfo.isBNLoading)
            {
                bannerInfo.isBNLoading = false;
                BannerPlacement bnpl = null;
                if (bannerInfo.dicBanner.ContainsKey(adsId))
                {
                    SdkUtil.logd($"ads admob{adsType} HandleBannerAdLoaded contain in dic");
                    bnpl = bannerInfo.dicBanner[adsId];
                    if (bnpl.banner == bannerView)
                    {
                        SdkUtil.logd($"ads admob{adsType} HandleBannerAdLoaded contain in dic errrrrrr");
                    }
                    else
                    {
                        SdkUtil.logd($"ads admob{adsType} HandleBannerAdLoaded replace new banner in dic");
                        ((BannerView)bnpl.banner).Hide();
                        ((BannerView)bnpl.banner).Destroy();
                        bnpl.banner = null;
                    }
                    bnpl.banner = bannerView;
                }
                else
                {
                    SdkUtil.logd($"ads admob{adsType} HandleBannerAdLoaded not contain in dic");
                    bnpl = new BannerPlacement(adsId);
                    bnpl.banner = bannerView;
                    bnpl.isloaded = true;
                    bnpl.isLoading = false;
                    bannerInfo.dicBanner.Add(adsId, bnpl);
                }
                if (bannerInfo.isBNShow)
                {
                    tShowBannerNm = 0;
                    if (bnpl != null)
                    {
                        bnpl.tShow = SdkUtil.CurrentTimeMilis() / 1000;
                    }
                    doshowBanner(bannerView);
                }
                else
                {
                    bannerView.Hide();
                }

                if (cbBanner != null)
                {
                    var tmpcb = cbBanner;
                    cbBanner = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
            }
            if (!bannerInfo.isBNShow)
            {
                hideBanner();
            }
        }
        private void HandleBannerAdFailedToLoad(BannerView bannerView, string adsId, LoadAdError error)
        {
            SdkUtil.logd($"ads admob{adsType} HandleBannerAdFailedToLoad adsId=" + adsId);
            if (bannerInfo.isBNLoading)
            {
                bannerInfo.isBNLoading = false;
                bool isFinishLoad = false;
                bannerView.Destroy();
                if (listBannerGroup.idxList < listBannerGroup.list.Count)
                {
                    ListAdsECPM lecpm = listBannerGroup.list[listBannerGroup.idxList];
                    if (lecpm.idxCurrEcpm < (lecpm.list.Count - 1))
                    {
                        lecpm.idxCurrEcpm++;
                        tryLoadBanner();
                    }
                    else
                    {
                        isFinishLoad = true;
                    }
                }
                else
                {
                    isFinishLoad = true;
                }
                if (isFinishLoad)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        if (cbBanner != null)
                        {
                            var tmpcb = cbBanner;
                            cbBanner = null;
                            tmpcb(AD_State.AD_LOAD_FAIL);
                        }
                        if (advhelper != null)
                        {
                            advhelper.onBannerLoadFail(adsType);
                        }
                    });
                    if (waitReinitBanner)
                    {
                        waitReinitBanner = false;
                        Debug.Log($"mysdk: ads admob{adsType} HandleBannerAdFailedToLoad re init banner when waitReinitBanner");
                        initBanner();
                    }
                }
            }
        }
        //
        private void HandleBannerClAdLoaded(BannerView bannerView, string adsId)
        {
            SdkUtil.logd($"ads admob{adsType} HandleBannerClAdLoaded ads=" + adsId);
            if (bannerClInfo.isBNLoading)
            {
                bannerClInfo.isBNLoading = false;
                BannerPlacement bnpl = null;
                if (bannerClInfo.dicBanner.ContainsKey(adsId))
                {
                    SdkUtil.logd($"ads admob{adsType} HandleBannerClAdLoaded contain in dic");
                    bnpl = bannerClInfo.dicBanner[adsId];
                    if (bnpl.banner == bannerView)
                    {
                        SdkUtil.logd($"ads admob{adsType} HandleBannerClAdLoaded contain in dic errrrrrr");
                    }
                    else
                    {
                        SdkUtil.logd($"ads admob{adsType} HandleBannerClAdLoaded replace new banner in dic");
                        ((BannerView)bnpl.banner).Hide();
                        ((BannerView)bnpl.banner).Destroy();
                        bnpl.banner = null;
                    }
                    bnpl.banner = bannerView;
                }
                else
                {
                    SdkUtil.logd($"ads admob{adsType} HandleBannerClAdLoaded not contain in dic");
                    bnpl = new BannerPlacement(adsId);
                    bnpl.banner = bannerView;
                    bnpl.isloaded = true;
                    bnpl.isLoading = false;
                    bannerClInfo.dicBanner.Add(adsId, bnpl);
                }
                if (bannerClInfo.isBNShow)
                {
                    tShowBannerCl = 0;
                    if (tChangeCl2Nm < 0)
                    {
                        tChangeCl2Nm = 0;
                    }
                    if (bnpl != null)
                    {
                        bnpl.tShow = SdkUtil.CurrentTimeMilis() / 1000;
                    }
                    Debug.Log("mysdk: admob advhelper.currConfig.timeAutoReloadBanner=" + advhelper.currConfig.timeAutoReloadBanner);
                    doshowBannerCl(bannerView);
                }
                else
                {
                    bannerView.Hide();
                }

                if (cbBanner != null)
                {
                    var tmpcb = cbBanner;
                    cbBanner = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
            }
            if (!bannerClInfo.isBNShow)
            {
                hideCollapseBanner();
            }
        }
        private void HandleBannerClAdFailedToLoad(BannerView bannerView, string adsId, LoadAdError error)
        {
            SdkUtil.logd($"ads admob{adsType} HandleBannerClAdFailedToLoad adsId=" + adsId);
            if (bannerClInfo.isBNLoading)
            {
                bannerClInfo.isBNLoading = false;
                bool isFinishLoad = false;
                bannerView.Destroy();
                if (listBannerClGroup.idxList < listBannerClGroup.list.Count)
                {
                    ListAdsECPM lecpm = listBannerClGroup.list[listBannerClGroup.idxList];
                    if (lecpm.idxCurrEcpm < (lecpm.list.Count - 1))
                    {
                        lecpm.idxCurrEcpm++;
                        tryLoadCollapseBanner();
                    }
                    else
                    {
                        isFinishLoad = true;
                    }
                }
                else
                {
                    isFinishLoad = true;
                }
                if (isFinishLoad)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        if (cbBanner != null)
                        {
                            var tmpcb = cbBanner;
                            cbBanner = null;
                            tmpcb(AD_State.AD_LOAD_FAIL);
                        }
                        if (advhelper != null)
                        {
                            advhelper.onBannerLoadFail(adsType);
                        }
                        if (!isShowingCollapse)
                        {
                            advhelper.hideBannerCollapse();
                            advhelper.showBanner((AD_BANNER_POS)bannerClInfo.posBanner, advhelper.bnOrien, bnClWidth, bnClDxcenter);
                        }
                    });
                    if (waitReinitBanner)
                    {
                        waitReinitBanner = false;
                        Debug.Log($"mysdk: ads admob{adsType} HandleBannerClAdFailedToLoad re init banner when waitReinitBanner");
                        initBanner();
                    }
                }
            }
        }
        //
        private void HandleBannerRectAdLoaded(BannerView bannerView, string adsId)
        {
            SdkUtil.logd($"ads admob{adsType} HandleBannerRectAdLoaded ads=" + adsId);
            if (bannerRectInfo.isBNLoading)
            {
                bannerRectInfo.isBNLoading = false;
                BannerPlacement bnpl = null;
                if (bannerRectInfo.dicBanner.ContainsKey(adsId))
                {
                    SdkUtil.logd($"ads admob{adsType} HandleBannerRectAdLoaded contain in dic");
                    bnpl = bannerRectInfo.dicBanner[adsId];
                    if (bnpl.banner == bannerView)
                    {
                        SdkUtil.logd($"ads admob{adsType} HandleBannerRectAdLoaded contain in dic errrrrrr");
                    }
                    else
                    {
                        SdkUtil.logd($"ads admob{adsType} HandleBannerRectAdLoaded replace new banner in dic");
                        ((BannerView)bnpl.banner).Hide();
                        ((BannerView)bnpl.banner).Destroy();
                        bnpl.banner = null;
                    }
                    bnpl.banner = bannerView;
                }
                else
                {
                    SdkUtil.logd($"ads admob{adsType} HandleBannerRectAdLoaded not contain in dic");
                    bnpl = new BannerPlacement(adsId);
                    bnpl.banner = bannerView;
                    bnpl.isloaded = true;
                    bnpl.isLoading = false;
                    bannerRectInfo.dicBanner.Add(adsId, bnpl);
                }
                if (bannerRectInfo.isBNShow)
                {
                    tShowBannerRect = 0;
                    if (bnpl != null)
                    {
                        bnpl.tShow = SdkUtil.CurrentTimeMilis() / 1000;
                    }
                    doshowBannerRect(bannerView);
                }
                else
                {
                    bannerView.Hide();
                }

                if (cbBanner != null)
                {
                    var tmpcb = cbBanner;
                    cbBanner = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
            }
            if (!bannerRectInfo.isBNShow)
            {
                hideRectBanner();
            }
        }
        private void HandleBannerRectAdFailedToLoad(BannerView bannerView, string adsId, LoadAdError error)
        {
            SdkUtil.logd($"ads admob{adsType} HandleBannerRectAdFailedToLoad adsId=" + adsId);
            if (bannerRectInfo.isBNLoading)
            {
                bannerRectInfo.isBNLoading = false;
                bool isFinishLoad = false;
                bannerView.Destroy();
                if (listBannerRectGroup.idxList < listBannerRectGroup.list.Count)
                {
                    ListAdsECPM lecpm = listBannerRectGroup.list[listBannerRectGroup.idxList];
                    if (lecpm.idxCurrEcpm < (lecpm.list.Count - 1))
                    {
                        lecpm.idxCurrEcpm++;
                        tryLoadRectBanner();
                    }
                    else
                    {
                        isFinishLoad = true;
                    }
                }
                else
                {
                    isFinishLoad = true;
                }
                if (isFinishLoad)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        if (cbBanner != null)
                        {
                            var tmpcb = cbBanner;
                            cbBanner = null;
                            tmpcb(AD_State.AD_LOAD_FAIL);
                        }
                    });
                    if (waitReinitBanner)
                    {
                        waitReinitBanner = false;
                        Debug.Log($"mysdk: ads admob{adsType} HandleBannerRectAdFailedToLoad re init banner when waitReinitBanner");
                        initBanner();
                    }
                }
            }
        }
        //
        private void HandleBannerAdPaidEvent(AdValue advalue, int typeBanner)
        {

            SdkUtil.logd($"ads admob{adsType} HandleBannerAdPaidEvent v={1000 * advalue.Value}-{advalue.CurrencyCode}-{advalue.Precision}");

            FIRhelper.logEventAdsPaidAdmob(0, bannerId, (int)advalue.Precision, advalue.CurrencyCode, 1000 * advalue.Value);
            FIRhelper.logEvent("show_ads_bn");
            if (typeBanner == 0)
            {
                FIRhelper.logEvent("show_ads_bn0_nm");
            }
            else if (typeBanner == 1)
            {
                FIRhelper.logEvent("show_ads_bn0_cl");
            }
            else
            {
                FIRhelper.logEvent("show_ads_bn0_rect");
            }
        }
        private void HandleBannerAdImpressionRecorded()
        {

            SdkUtil.logd($"ads admob{adsType} HandleBannerAdImpressionRecorded");

        }
        private void HandleBannerAdClicked()
        {

            SdkUtil.logd($"ads admob{adsType} HandleBannerAdClicked");

        }
        private void HandleBannerAdFullScreenContentOpened()
        {

            SdkUtil.logd($"ads admob{adsType} HandleBannerAdFullScreenContentOpened");

        }
        private void HandleBannerAdFullScreenContentClosed()
        {

            SdkUtil.logd($"ads admob{adsType} HandleBannerAdFullScreenContentClosed");

        }
        #endregion

        #region native ads
#if ENABLE_ADS_ADMOB_NATIVE
        private void HandleNativeAdLoaded(object sender, NativeAdEventArgs args)
        {

            SdkUtil.logd($"ads admob{adsType} HandleNativeAdLoaded=" + sender);

            AdsNativeAdmob adnt = new AdsNativeAdmob();
            adnt.nativeAd = args.nativeAd;
            adnt.isLoaded = true;
            adnt.isLoading = false;
            listAdNativeUse.Add(adnt);
            adnt.nativeAd.OnPaidEvent += HandleNativeAdPaidEvent;
            if (dicAdsNative.ContainsKey((AdLoader)sender))
            {
                Object4LoadNative obAd = dicAdsNative[(AdLoader)sender];
                dicAdsNative.Remove((AdLoader)sender);
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    pushNative2GameObject(obAd, adnt.nativeAd);
                }, 0.01f);
            }
        }

        private void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {

            SdkUtil.logd($"ads admob{adsType} HandleAdFailedToLoad=" + args.LoadAdError.GetMessage());

#if ENABLE_ADS_ADMOB_NATIVE
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
#endif
        }

        private void HandleNativeAdPaidEvent(object sender, AdValueEventArgs adValue)
        {

            SdkUtil.logd($"ads admob{adsType} HandleNativeAdPaidEvent v={1000 * adValue.AdValue.Value}-{adValue.AdValue.CurrencyCode}-{adValue.AdValue.Precision}");

            FIRhelper.logEventAdsPaidAdmob(11, nativeId, (int)adValue.AdValue.Precision, adValue.AdValue.CurrencyCode, 1000 * adValue.AdValue.Value);
        }
#endif
        #endregion

        #region INTERSTITIAL AD EVENTS
        private void HandleInterstitialLoaded()
        {
            SdkUtil.logd($"ads admob{adsType} full HandleInterstitialLoaded");
            FullTryLoad = 0;
            isFullLoading = false;
            isFullLoaded = true;
            if (cbFullLoad != null)
            {
                var tmpcb = cbFullLoad;
                cbFullLoad = null;
                tmpcb(AD_State.AD_LOAD_OK);
            }

            SdkUtil.logd($"ads admob{adsType} full HandleInterstitialLoaded 1");

        }

        private void HandleInterstitialFailedToLoad(LoadAdError error)
        {
            SdkUtil.logd($"ads admob{adsType} full HandleInterstitialFailedToLoad=" + error.ToString());
            isFullLoading = false;
            isFullLoaded = false;
            if (listFullEcpm.idxCurrEcpm < (listFullEcpm.list.Count - 1))
            {
                SdkUtil.logd($"ads admob{adsType} full HandleInterstitialFailedToLoad load other ecpm idxCurrEcpmFull=" + listFullEcpm.idxCurrEcpm + ", count=" + listFullEcpm.list.Count);
                listFullEcpm.idxCurrEcpm++;
                tryLoadFull();
            }
            else
            {
                if (cbFullLoad != null)
                {
                    var tmpcb = cbFullLoad;
                    cbFullLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
            }
            SdkUtil.logd($"ads admob{adsType} full HandleInterstitialFailedToLoad 1");
        }

        private void HandleInterstitialImpressionRecorded()
        {
            SdkUtil.logd($"ads admob{adsType} full HandleInterstitialImpressionRecorded");
        }

        private void HandleInterstitialOpened()
        {
            SdkUtil.logd($"ads admob{adsType} full HandleInterstitialOpened");
            if (cbFullShow != null)
            {
                AdCallBack tmpcb = cbFullShow;
                AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
            }
        }

        private void HandleInterstitialClicked()
        {
            SdkUtil.logd($"ads admob{adsType} full HandleInterstitialClicked");
        }

        private void HandleInterstitialFailedToShow(AdError error)
        {
            SdkUtil.logd($"ads admob{adsType} full HandleInterstitialFailedToShow=" + error.ToString());
            isFullLoading = false;
            isFullLoaded = false;
            if (cbFullShow != null)
            {
                AdCallBack tmpcb = cbFullShow;
                AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
            }
            onFullClose();

            FullTryLoad = 0;
            cbFullShow = null;
        }

        private void HandleInterstitialClosed()
        {
            SdkUtil.logd($"ads admob{adsType} full HandleInterstitialClosed");
            isFullLoading = false;
            isFullLoaded = false;
            if (cbFullShow != null)
            {
                AdCallBack tmpcb = cbFullShow;
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    tmpcb(AD_State.AD_CLOSE);
                });
            }
            onFullClose();
            FullTryLoad = 0;
            cbFullShow = null;
            SdkUtil.logd($"ads admob{adsType} full HandleInterstitialClosed 1");
        }

        private void HandleInterstitialPaidEvent(AdValue adValue)
        {
            SdkUtil.logd($"ads admob{adsType} full HandleInterstitialPaidEvent v={1000 * adValue.Value}-{adValue.CurrencyCode}-{adValue.Precision}");
            FIRhelper.logEventAdsPaidAdmob(4, fullId, (int)adValue.Precision, adValue.CurrencyCode, 1000 * adValue.Value);
        }

        private void HandleInterstitialLeftApplication(object sender, EventArgs e)
        {
            SdkUtil.logd($"ads admob{adsType} full HandleInterstitialLeftApplication 1");
        }

        #endregion

        #region REWARDED VIDEO AD EVENTS

        private void HandleRewardBasedVideoLoaded()
        {
            SdkUtil.logd($"ads admob{adsType} gift HandleRewardBasedVideoLoaded");
            GiftTryLoad = 0;
            isGiftLoading = false;
            isGiftLoaded = true;
            if (cbGiftLoad != null)
            {
                var tmpcb = cbGiftLoad;
                cbGiftLoad = null;
                tmpcb(AD_State.AD_LOAD_OK);
            }
            SdkUtil.logd($"ads admob{adsType} gift HandleRewardBasedVideoLoaded 1");
        }

        private void HandleRewardBasedVideoFailedToLoad(LoadAdError error)
        {
            SdkUtil.logd($"ads admob{adsType} gift HandleRewardBasedVideoFailedToLoad=" + error.ToString());
            isGiftLoading = false;
            isGiftLoaded = false;
            if (listGiftEcpm.idxCurrEcpm < (listGiftEcpm.list.Count - 1))
            {
                SdkUtil.logd($"ads admob{adsType} gift OnRewardedAdFailedEvent load other ecpm idxCurrEcpmGift=" + listGiftEcpm.idxCurrEcpm + ", count=" + listGiftEcpm.list.Count);
                listGiftEcpm.idxCurrEcpm++;
                tryloadGift();
            }
            else
            {
                if (cbGiftLoad != null)
                {
                    var tmpcb = cbGiftLoad;
                    cbGiftLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
            }
            SdkUtil.logd($"ads admob{adsType} gift HandleRewardBasedVideoFailedToLoad 1");
        }

        private void HandleRewardBasedVideoImpressionRecorded()
        {
            SdkUtil.logd($"ads admob{adsType} gift HandleRewardBasedVideoImpressionRecorded");
        }

        private void HandleRewardBasedVideoOpened()
        {
            SdkUtil.logd($"ads admob{adsType} gift HandleRewardBasedVideoOpened");
            GiftTryLoad = 0;
            if (cbGiftShow != null)
            {
                var tmpcb = cbGiftShow;
                SdkUtil.logd($"ads admob{adsType} gift HandleRewardBasedVideoClosed2");
                AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
            }
            SdkUtil.logd($"ads admob{adsType} gift HandleRewardBasedVideoOpened 1");
        }

        private void HandleRewardBasedVideoClicked()
        {
            SdkUtil.logd($"ads admob{adsType} gift HandleRewardBasedVideoClicked");
        }

        private void HandleRewardBasedVideoFailToShow(AdError error)
        {
            SdkUtil.logd($"ads admob{adsType} gift HandleRewardBasedVideoFailToShow=" + error.ToString());
            GiftTryLoad = 0;
            isGiftLoading = false;
            isGiftLoaded = false;
            if (cbGiftShow != null)
            {
                AdCallBack tmpcb = cbGiftShow;
                AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
            }
            onGiftClose();
            isRewardCom = false;
            cbGiftShow = null;
        }

        private void HandleRewardBasedVideoStarted(object sender, EventArgs e)
        {
            SdkUtil.logd($"ads admob{adsType} gift HandleRewardBasedVideoStarted");
            GiftTryLoad = 0;
        }

        private void HandleRewardBasedVideoRewarded()
        {
            SdkUtil.logd($"ads admob{adsType} gift HandleRewardBasedVideoRewarded");
            isRewardCom = true;
        }

        private void HandleRewardBasedVideoClosed()
        {
            SdkUtil.logd($"ads admob{adsType} gift HandleRewardBasedVideoClosed");
            isGiftLoading = false;
            isGiftLoaded = false;
            if (cbGiftShow != null)
            {
                AdCallBack tmpcb = cbGiftShow;
                SdkUtil.logd($"ads admob{adsType} gift _cbAD != null");

                if (isRewardCom)
                {
                    SdkUtil.logd($"ads admob{adsType} gift _cbAD reward");
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_OK); });
                }
                else
                {
                    SdkUtil.logd($"ads admob{adsType} gift _cbAD fail");
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_FAIL); });
                }
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    tmpcb(AD_State.AD_CLOSE);
                });
            }
            onGiftClose();

            isRewardCom = false;
            GiftTryLoad = 0;
            cbGiftShow = null;

            SdkUtil.logd($"ads admob{adsType} gift HandleRewardBasedVideoClosed 3");

        }

        private void HandleRewardBasedVideoLeftApplication(object sender, EventArgs e)
        {

            SdkUtil.logd($"ads admob{adsType} gift HandleRewardBasedVideoLeftApplication");

        }

        private void HandleRewardBasedPaidEvent(AdValue adValue)
        {

            SdkUtil.logd($"ads admob{adsType} HandleRewardBasedPaidEvent v={1000 * adValue.Value}-{adValue.CurrencyCode}-{adValue.Precision}");

            FIRhelper.logEventAdsPaidAdmob(5, giftId, (int)adValue.Precision, adValue.CurrencyCode, 1000 * adValue.Value);
        }

        #endregion

#endif

    }

    public class AdsNativeMemAdmob
    {
        public List<AdsNativeAdmob> listAdNativeNew = new List<AdsNativeAdmob>();
    }

    public class AdsNativeAdmob
    {
#if ENABLE_ADS_ADMOB && (ENABLE_ADS_ADMOB_NATIVE || USE_NATIVE_UNITY)
        public NativeAd nativeAd;
#endif
        public string adUnitId = "";
        public bool isLoaded;
        public bool isLoading;
        public long tLoaded = 0;
    }

    public class Object4LoadNative
    {
        public AdsNativeObject adNative;
        public AdCallBack cbLoad;

        public Object4LoadNative(AdsNativeObject ad, AdCallBack cb)
        {
            adNative = ad;
            cbLoad = cb;
        }
    }

#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY && ENABLE_ADS_ADMOB_NATIVE
    public class MyAdmobMediationExtras : GoogleMobileAds.Api.Mediation.MediationExtras
    {
        public MyAdmobMediationExtras(string key, string value)
        {
            Extras = new Dictionary<string, string>();
            Extras.Add(key, value);
        }
        public override string AndroidMediationExtraBuilderClassName
        {
            get { return "com.google.ads.mediation.admob.AdMobAdapter"; }
        }

        public override string IOSMediationExtraBuilderClassName
        {
            get { return "GADUAdNetworkExtras"; }
        }
    }
#endif
}