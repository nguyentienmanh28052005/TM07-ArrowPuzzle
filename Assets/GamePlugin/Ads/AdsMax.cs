//#define ENABLE_ADS_MAX
//#define ENABLE_ADS_AMAZON
//#define use_load_all

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if ENABLE_ADS_MAX

#if ENABLE_ADS_AMAZON
using AmazonAds;
#endif

#endif

namespace mygame.sdk
{
    public class AdsMax : AdsBase
    {
#if ENABLE_ADS_MAX
        MaxSdkBase.BannerPosition bnCurrPos;
        bool isBnCreate = false;
        bool isFullFirstLoad = true;
        bool isGiftFirstLoad = true;
        private bool isAdsInited = false;
        private int giftReloadWithApplovin = 1;
        private string plWaitFull = "aabc";
        private string plWaitGift = "aabc";

        private string bnIdLoaded = "";
        private string fullIdLoaded = "";
        private string giftIdLoaded = "";

        int timewaitload = 60000;

        bool adsIsClick = false;
#endif

        [Header("Amazon")]
        public string amazonAppId = "";
        public string amazonBnSlotId = "";
        public string amazonBnLeaderSlotId = "";
        public string amazonInterSlotId = "";
        public string amazonVideoRewardedSlotId = "";//AMAZON_VIDEO_REWARDED_SLOT_ID

        public override string getname()
        {
            return "max";
        }

        public override void InitAds()
        {
#if ENABLE_ADS_MAX
            isEnable = true;
            isAdsInited = false;
#if ENABLE_ADS_AMAZON
            if (adsType == 6)
            {
                Amazon.Initialize(amazonAppId);
                Amazon.SetAdNetworkInfo(new AdNetworkInfo(DTBAdNetwork.MAX));
                Amazon.SetMRAIDSupportedVersions(new string[] { "1.0", "2.0", "3.0" });
                Amazon.SetMRAIDPolicy(Amazon.MRAIDPolicy.CUSTOM);
                if (SDKManager.Instance.isAdCanvasSandbox)
                {
                    Amazon.EnableLogging(true);
                    Amazon.EnableTesting(true);
                }
            }
#endif

            if (adsType == 6)
            {
                SdkUtil.logd($"ads max{adsType} init begin");

                // string[] testDeviceIds = new string[] {
                //     "BD441DE9-842E-4D00-8861-E485939A9BAA",
                //     "E17F4088-41EE-484D-BD9C-CD1B763CF3A3"
                // };
                // MaxSdk.SetTestDeviceAdvertisingIdentifiers(testDeviceIds);

                MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
                {
                    SdkUtil.logd($"ads max{adsType} init suc");
                    isAdsInited = true;
                    startAds();

                    if (adsType == 6)
                    {
                        disableAutoRetries();
                        advhelper.onAdsInitSuc(6);
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            if ("aabc".CompareTo(plWaitFull) != 0)
                            {
                                loadFull(plWaitFull, null);
                            }
                            if ("aabc".CompareTo(plWaitGift) != 0)
                            {
                                loadGift(plWaitGift, null);
                            }
                        }, 0.1f);
                    }
                };

                string adUnitIdDisB2B = getAdUnitIdDisableB2B();
                MaxSdk.SetExtraParameter("disable_b2b_ad_unit_ids", adUnitIdDisB2B);
                MaxSdk.SetSdkKey(appId);
                string[] allIds = getAllAdUnitIds();
                MaxSdk.InitializeSdk(allIds);
            }
#endif
        }

        public override void AdsAwake()
        {
#if ENABLE_ADS_MAX
            isEnable = true;
#endif
            initData();
        }

        private void initData()
        {
#if ENABLE_ADS_MAX
            //banner
            dicPLBanner.Clear();
            AdPlacementBanner plbn = new AdPlacementBanner();
            dicPLBanner.Add(PLBnDefault, plbn);
            plbn.placement = PLBnDefault;
            plbn.adECPM.idxHighPriority = -1;
            plbn.adECPM.listFromDstring(bannerId);
            //full
            dicPLFull.Clear();
            AdPlacementFull plfull = new AdPlacementFull();
            dicPLFull.Add(PLFullDefault, plfull);
            plfull.placement = PLFullDefault;
            plfull.adECPM.idxHighPriority = -1;
            plfull.adECPM.listFromDstring(fullId);
            //gift
            dicPLGift.Clear();
            AdPlacementFull plgift = new AdPlacementFull();
            dicPLGift.Add(PLGiftDefault, plgift);
            plgift.placement = PLGiftDefault;
            plgift.adECPM.idxHighPriority = -1;
            plgift.adECPM.listFromDstring(giftId);
            SdkUtil.logd($"ads max{adsType} init dicAds");
#endif
        }

        private void startAds()
        {
#if ENABLE_ADS_MAX
#if UNITY_IOS || UNITY_IPHONE
            if (GameHelper.Instance.countryCode.CompareTo("cn") == 0 || GameHelper.Instance.countryCode.CompareTo("chn") == 0
                    || GameHelper.Instance.countryCode.CompareTo("zh") == 0 || GameHelper.Instance.countryCode.CompareTo("zh0") == 0)
            {
                if (ios_bn_cn_id.Length > 3)
                {
                    Debug.Log($"mysdk: ads max{adsType} change bn to china");
                    bannerId = ios_bn_cn_id;
                }
                if (ios_full_cn_id.Length > 3)
                {
                    Debug.Log($"mysdk: ads max{adsType} change full to china");
                    fullId = ios_full_cn_id;
                }
                if (ios_gift_cn_id.Length > 3)
                {
                    Debug.Log($"mysdk: ads max{adsType} change gift to china");
                    giftId = ios_gift_cn_id;
                }
            }
#endif

            if (adsType == 6)
            {
                MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
                MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
                MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickEvent;
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;

                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialFailedToDisplayEvent;
                MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
                MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
                MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialAdRevenuePaidEvent;

                MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
                MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            }
#endif
        }

        private void disableAutoRetries()
        {
#if ENABLE_ADS_MAX
            foreach (var dicpl in dicPLFull)
            {
                foreach (var itemecmp in dicpl.Value.adECPM.list)
                {
                    MaxSdk.SetInterstitialExtraParameter(itemecmp.adsId, "disable_auto_retries", "true");
                }
            }
            foreach (var dicpl in dicPLGift)
            {
                foreach (var itemecmp in dicpl.Value.adECPM.list)
                {
                    MaxSdk.SetRewardedAdExtraParameter(itemecmp.adsId, "disable_auto_retries", "true");
                }
            }
#endif
        }

        private string[] getAllAdUnitIds()
        {
            List<string> list = new List<string>();
            foreach (var dicpl in dicPLBanner)
            {
                foreach (var itemecmp in dicpl.Value.adECPM.list)
                {
                    list.Add(itemecmp.adsId);
                }
            }
            foreach (var dicpl in dicPLFull)
            {
                foreach (var itemecmp in dicpl.Value.adECPM.list)
                {
                    list.Add(itemecmp.adsId);
                }
            }
            foreach (var dicpl in dicPLGift)
            {
                foreach (var itemecmp in dicpl.Value.adECPM.list)
                {
                    list.Add(itemecmp.adsId);
                }
            }
            if (nativeFullId != null && nativeFullId.Length > 5)
            {
                list.Add(nativeFullId);
            }
            string[] re = new string[list.Count];
            int i = 0;
            foreach (var item in list)
            {
                re[i] = item.ToString();
                i++;
            }
            list.Clear();
            return re;
        }

        private string getAdUnitIdDisableB2B()
        {
            string re = "";
            foreach (var dicpl in dicPLFull)
            {
                foreach (var itemecmp in dicpl.Value.adECPM.list)
                {
                    if (re.Length <= 0)
                    {
                        re = itemecmp.adsId;
                    }
                    else
                    {
                        re += $",{itemecmp.adsId}";
                    }
                }
            }
            foreach (var dicpl in dicPLGift)
            {
                foreach (var itemecmp in dicpl.Value.adECPM.list)
                {
                    if (re.Length <= 0)
                    {
                        re = itemecmp.adsId;
                    }
                    else
                    {
                        re += $",{itemecmp.adsId}";
                    }
                }
            }
            return re;
        }

        protected override void tryLoadBanner(AdPlacementBanner adpl)
        {
#if ENABLE_ADS_MAX
            SdkUtil.logd($"ads bn {adpl.loadPl}-{adpl.placement} max{adsType} tryLoadBanner = " + bannerId);
            if (!isAdsInited)
            {
                SdkUtil.logd($"ads bn {adpl.loadPl}-{adpl.placement} max{adsType} tryLoadBanner not init");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
                return;
            }
            if (adpl.countLoad >= toTryLoad)
            {
                SdkUtil.logd($"ads bn {adpl.loadPl}-{adpl.placement} max{adsType} tryLoadBanner over trycount");
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            if (adpl.adECPM.list.Count <= adpl.adECPM.idxCurrEcpm)
            {
                SdkUtil.logd($"ads bn {adpl.loadPl}-{adpl.placement} max{adsType} tryLoadBanner index out of range idx={adpl.adECPM.idxCurrEcpm} l={adpl.adECPM.list.Count}");
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            string idLoad = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
            if (idLoad == null || idLoad.Length < 3)
            {
                SdkUtil.logd($"ads bn {adpl.loadPl}-{adpl.placement} max{adsType} tryLoadBanner idLoad is empty");
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
#if ENABLE_ADS_AMAZON
            if (isBnCreate)
            {

            }
#else
            if (isBnCreate)
            {
                SdkUtil.logd($"ads bn {adpl.loadPl}-{adpl.placement} max{adsType} tryLoadBanner DestroyBanner");
                MaxSdk.DestroyBanner(idLoad);
            }
#endif

            adpl.isLoading = false;
            adpl.isloaded = true;

            if (adpl.posBanner == 0)
            {
                bnCurrPos = MaxSdkBase.BannerPosition.TopCenter;
            }
            else if (adpl.posBanner == 1)
            {
                bnCurrPos = MaxSdkBase.BannerPosition.BottomCenter;
            }
            else
            {
                bnCurrPos = MaxSdkBase.BannerPosition.BottomCenter;
            }
            if (bnWidth > 0)
            {
                MaxSdk.SetBannerWidth(idLoad, bnWidth);
            }
            string adaptivebn = "true";
            if (bnWidth < -1)
            {
                adaptivebn = "true";
            }
            else
            {
                adaptivebn = "false";
            }
#if ENABLE_ADS_AMAZON
                if (amazonBnSlotId != null && amazonBnSlotId.Length > 3)
                {
                    SdkUtil.logd($"ads bn {placement} max{adsType} tryLoadBanner amazon load");
                    int widtham;
                    int heightam;
                    string slotId;
                    if (MaxSdkUtils.IsTablet() && bnWidth <= 0)
                    {
                        widtham = 728;
                        heightam = 90;
                        slotId = amazonBnLeaderSlotId;
                    }
                    else
                    {
                        widtham = 320;
                        heightam = 50;
                        slotId = amazonBnSlotId;
                    }
                    var apsBanner = new APSBannerAdRequest(widtham, heightam, slotId);
                    apsBanner.onSuccess += (adResponse) =>
                    {
                        MaxSdk.SetBannerLocalExtraParameter(idLoad, "amazon_ad_response", adResponse.GetResponse());
                        createBannerAndShow(adaptivebn);
                    };
                    apsBanner.onFailedWithError += (adError) =>
                    {
                        MaxSdk.SetBannerLocalExtraParameter(idLoad, "amazon_ad_error", adError.GetAdError());
                        createBannerAndShow(adaptivebn);
                    };

                    apsBanner.LoadAd();
                }
                else
                {
                    createBannerAndShow(adaptivebn);
                }
#else
            createBannerAndShow(adpl, adaptivebn);
#endif
#else
            SdkUtil.logd($"ads bn {adpl.loadPl} max{adsType} tryLoadBanner not enable");
            var tmpcb = adpl.cbLoad;
            adpl.cbLoad = null;
            tmpcb(AD_State.AD_LOAD_FAIL);
#endif
        }

#if ENABLE_ADS_MAX
        void createBannerAndShow(AdPlacementBanner adpl, string adaptivebn)
        {
            string idLoad = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
            MaxSdk.SetBannerExtraParameter(idLoad, "adaptive_banner", adaptivebn);
            MaxSdk.CreateBanner(idLoad, bnCurrPos);
            AdsHelper.onAdLoad(adpl.loadPl, "banner", idLoad, "applovin");
            isBnCreate = true;
            adpl.countLoad = 0;
            if (adpl.isShow)
            {
                advhelper.hideOtherBanner(6);
                if (bnWidth > 0)
                {
                    MaxSdk.SetBannerWidth(idLoad, bnWidth);
                }
                //MaxSdk.ShowBanner(idLoad);
                if (!adpl.isRealShow)
                {
                    adpl.isRealShow = true;
                    MaxSdk.ShowBanner(idLoad);
                }
                else
                {
                    MaxSdk.UpdateBannerPosition(idLoad, bnCurrPos);
                    MaxSdk.ShowBanner(idLoad);
                }
            }
            else
            {
                SdkUtil.logd($"ads bn {adpl.placement} max{adsType} tryLoadBanner CreateBanner hide");
                adpl.isShow = false;
                adpl.isRealShow = false;
                MaxSdk.HideBanner(idLoad);
            }
        }
#endif

        public override void loadBanner(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_MAX
            AdPlacementBanner adpl = getPlBanner(placement, 0);
            if (adpl != null)
            {
                SdkUtil.logd($"ads bn {placement} max{adsType} loadBanner");
                adpl.cbLoad = cb;
                if (!adpl.isLoading)
                {
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadBanner(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads bn {placement} max{adsType} loadBanner isloading");
                }
            }
            else
            {
                SdkUtil.logd($"ads bn {placement} max{adsType} loadBanner not pl");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
            }
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads max{adsType} bn loadBanner not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showBanner(string placement, int pos, int width, int maxH, AdCallBack cb, float dxCenter, bool highP = false)
        {
#if ENABLE_ADS_MAX
            AdPlacementBanner adpl = getPlBanner(placement, 0);
            if (adpl != null)
            {
                SdkUtil.logd($"ads bn {placement} max{adsType} showBanner");
                adpl.isShow = true;
                adpl.posBanner = pos;
                adpl.setSetPlacementShow(placement);
                bnWidth = width;
                if (adpl.isloaded)
                {
                    string idLoad = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
                    SdkUtil.logd($"ads bn {placement} max{adsType} showBanner loaded and show id={idLoad}");
                    if (adpl.posBanner == 0)
                    {
                        bnCurrPos = MaxSdkBase.BannerPosition.TopCenter;
                    }
                    else if (adpl.posBanner == 1)
                    {
                        bnCurrPos = MaxSdkBase.BannerPosition.BottomCenter;
                    }
                    else
                    {
                        bnCurrPos = MaxSdkBase.BannerPosition.BottomCenter;
                    }
                    if (bnWidth < -1)
                    {
                        float w = Screen.width * 160 / Screen.dpi;
                        SdkUtil.logd($"ads bn {placement} max{adsType} showBanner w={w}");
                        MaxSdk.SetBannerWidth(idLoad, w);
                        MaxSdk.SetBannerExtraParameter(idLoad, "adaptive_banner", "true");
                    }
                    else if (bnWidth < 10)
                    {
                        MaxSdk.SetBannerExtraParameter(idLoad, "adaptive_banner", "false");
                        if (MaxSdkUtils.IsTablet())
                        {
                            MaxSdk.SetBannerWidth(idLoad, 728);
                        }
                        else
                        {
                            MaxSdk.SetBannerWidth(idLoad, 320);
                        }
                    }
                    else
                    {
                        MaxSdk.SetBannerExtraParameter(idLoad, "adaptive_banner", "false");
                        MaxSdk.SetBannerWidth(idLoad, bnWidth);
                    }
                    MaxSdk.UpdateBannerPosition(idLoad, bnCurrPos);
                    MaxSdk.ShowBanner(idLoad);
                    return true;
                }
                else
                {
                    SdkUtil.logd($"ads bn {placement} max{adsType} showBanner not load and load");
                    loadBanner(placement, cb);
                    return false;
                }
            }
            else
            {
                SdkUtil.logd($"ads bn {placement} max{adsType} showBanner not pl");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return false;
            }
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads bn {placement} max{adsType} tryLoadBanner not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
            return false;
#endif
        }
        public override void hideBanner()
        {
#if ENABLE_ADS_MAX
            SdkUtil.logd($"ads bn max{adsType} hideBanner");
            AdPlacementBanner adpl = getPlBanner(PLBnDefault, 0);
            if (adpl != null && adpl.cbLoad != null)
            {
                adpl.isShow = false;
            }
            if (bannerId != null && bannerId.Length > 3)
            {
                MaxSdk.HideBanner(bannerId);
            }
#endif
        }
        public override void destroyBanner()
        {
#if ENABLE_ADS_MAX
            hideBanner();
            AdPlacementBanner adpl = getPlBanner(PLBnDefault, 0);
            if (adpl != null && adpl.cbLoad != null)
            {
                adpl.isloaded = false;
                adpl.isLoading = false;
            }
            MaxSdk.DestroyBanner(bannerId);
#endif
        }

        // Native

        //
        public override void clearCurrFull(string placement)
        {
#if ENABLE_ADS_MAX
            if (getFullLoaded(placement) == 1)
            {
                AdPlacementFull adpl = getPlFull(placement, false);
                if (adpl != null)
                {
                    adpl.isloaded = false;
                }
            }
#endif
        }
        public override int getFullLoaded(string placement)
        {
#if ENABLE_ADS_MAX
            AdPlacementFull adpl = getPlFull(placement, true);
            if (adpl == null)
            {
                SdkUtil.logd($"ads full {placement} max{adsType} getFullLoaded not pl");
                return 0;
            }
            else
            {
#if use_load_all
                for (int i = 0; i < adpl.adECPM.list.Count; i++)
                {
                    if (adpl.adECPM.list[i].isLoaded)
                    {
                        fullAdNetwork = adpl.adECPM.list[i].adnetname;
                        return 1;
                    }
                }
                return 0;
#else
                if (adpl.isloaded)
                {
                    return 1;
                }
                else
                {
                    SdkUtil.logd($"ads full {placement} max{adsType} getFullLoaded not loaded");
                    return 0;
                }
#endif
            }
#else
            return 0;
#endif
        }
        protected override void tryLoadFull(AdPlacementFull adpl)
        {
#if ENABLE_ADS_MAX
            if (!isAdsInited)
            {
                SdkUtil.logd($"ads full {adpl.loadPl} max{adsType} tryLoadFull not init");
                plWaitFull = adpl.loadPl;
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            if (adpl.countLoad >= toTryLoad)
            {
                SdkUtil.logd($"ads max{adsType} full tryLoadFull over try");
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            if (adpl.adECPM.list.Count <= adpl.adECPM.idxCurrEcpm)
            {
                SdkUtil.logd($"ads full {adpl.loadPl} max{adsType} tryLoadBanner index out of range idx={adpl.adECPM.idxCurrEcpm} l={adpl.adECPM.list.Count}");
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            string idLoad = "";
            adpl.isLoading = true;
            adpl.isloaded = false;
#if use_load_all
            adpl.countLoad = 0;
            for (int i = 0; i < adpl.adECPM.list.Count; i++)
            {
                if (!adpl.adECPM.list[i].isLoading && !adpl.adECPM.list[i].isLoaded)
                {
                    idLoad = adpl.adECPM.list[i].adsId;
                    SdkUtil.logd($"ads full {adpl.loadPl} max{adsType} tryLoadFull load={idLoad}");
                    adpl.adECPM.list[i].isLoading = true;
                    adpl.countLoad++;
                    MaxSdk.LoadInterstitial(idLoad);
                    AdsHelper.onAdLoad(adpl.loadPl, "interstitial", idLoad, "applovin");
                    FIRhelper.logAdEvent("ads_full_max_load");
                    AppsFlyerHelperScript.logAdEvent("ads_load", "", "interstitial", idLoad, "applovin", "");
                }
                else
                {
                    if (adpl.adECPM.list[i].isLoading)
                    {
                        adpl.countLoad++;
                    }
                    if (adpl.adECPM.list[i].isLoaded)
                    {
                        adpl.isloaded = true;
                    }
                    SdkUtil.logd($"ads full {adpl.loadPl} max{adsType} tryLoadFull id={adpl.adECPM.list[i].adsId} loading={adpl.adECPM.list[i].isLoading} loaded={adpl.adECPM.list[i].isLoaded}");
                }
            }
            if (adpl.countLoad == 0)
            {
                adpl.isLoading = false;
            }
#else
            idLoad = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
            adsFullSubType = -1;
            SdkUtil.logd($"ads full {adpl.loadPl} max{adsType} tryLoadFull load={idLoad}");
#if ENABLE_ADS_AMAZON
            if (isFullFirstLoad && amazonInterSlotId != null && amazonInterSlotId.Length > 3)
            {
                isFullFirstLoad = false;
                var interstitialAd = new APSInterstitialAdRequest(amazonInterSlotId);
                interstitialAd.onSuccess += (adResponse) =>
                {
                    MaxSdk.SetInterstitialLocalExtraParameter(idLoad, "amazon_ad_response", adResponse.GetResponse());
                    MaxSdk.LoadInterstitial(idLoad);
                };
                interstitialAd.onFailedWithError += (adError) =>
                {
                    MaxSdk.SetInterstitialLocalExtraParameter(idLoad, "amazon_ad_error", adError.GetAdError());
                    MaxSdk.LoadInterstitial(idLoad);
                };
                AdsHelper.onAdLoad(adpl.loadPl, "interstitial", idLoad, "applovin");
                FIRhelper.logAdEvent("ads_full_max_load");
                AppsFlyerHelperScript.logAdEvent("ads_load", "", "interstitial", idLoad, "applovin", "");
                interstitialAd.LoadAd();
            }
            else
            {
                AdsHelper.onAdLoad(adpl.loadPl, "interstitial", idLoad, "applovin");
                FIRhelper.logAdEvent("ads_full_max_load");
                AppsFlyerHelperScript.logAdEvent("ads_load", "", "interstitial", idLoad, "applovin", "");
                MaxSdk.LoadInterstitial(idLoad);
            }
#else
            MaxSdk.LoadInterstitial(idLoad);
            AdsHelper.onAdLoad(adpl.loadPl, "interstitial", idLoad, "applovin");
            FIRhelper.logAdEvent("ads_full_max_load");
            AppsFlyerHelperScript.logAdEvent("ads_load", "", "interstitial", idLoad, "applovin", "");
#endif

#endif

#else
            SdkUtil.logd($"ads max{adsType} full tryLoadFull not enable");
            var tmpcb = adpl.cbLoad;
            adpl.cbLoad = null;
            tmpcb(AD_State.AD_LOAD_FAIL);
#endif
        }
        public override void loadFull(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_MAX
            AdPlacementFull adpl = getPlFull(placement, false);
            if (adpl == null)
            {
                SdkUtil.logd($"ads full {placement} max{adsType} loadFull not placement");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            else
            {
                SdkUtil.logd($"ads full {placement} max{adsType} loadFull");
#if use_load_all
                if (true)//(!adpl.isLoading)
#else
                if (!adpl.isloaded && !adpl.isLoading)
#endif
                {
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.cbLoad = cb;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadFull(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads full {placement} max{adsType} loadFull isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                }
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
#if ENABLE_ADS_MAX
            AdPlacementFull adpl = getPlFull(placement, true);
            if (adpl != null)
            {
                adpl.cbShow = null;
                int ss = getFullLoaded(placement);
                if (ss > 0)
                {
                    adpl.countLoad = 0;
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    string idShow = "";
                    string netShow = "";
#if use_load_all
                    for (int i = 0; i < adpl.adECPM.list.Count; i++)
                    {
                        if (adpl.adECPM.list[i].isLoaded)
                        {
                            idShow = adpl.adECPM.list[i].adsId;
                            netShow = adpl.adECPM.list[i].adnetname;
                            fullAdNetwork = adpl.adECPM.list[i].adnetname;
                            break;
                        }
                    }
#else
                    idShow = fullIdLoaded;
                    netShow = fullAdNetwork;
#endif
                    SdkUtil.logd($"ads full {placement} max{adsType} showFull isspalsh net={netShow} id={idShow}");
                    if (timeDelay > 0)
                    {
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            AdsHelper.onAdShowStart(placement, "interstitial", "applovin", "");
                            MaxSdk.ShowInterstitial(idShow);
                        }, timeDelay);
                        return true;
                    }
                    else
                    {
                        AdsHelper.onAdShowStart(placement, "interstitial", "applovin", "");
                        MaxSdk.ShowInterstitial(idShow);
                        return true;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads full {placement} max{adsType} showFull not loaded");
                }
            }
            else
            {
                SdkUtil.logd($"ads full {placement} max{adsType} showFull not pl");
            }
#endif
            return false;
        }

        //------------------------------------------------
        public override void clearCurrGift(string placement)
        {
#if ENABLE_ADS_MAX
            if (getGiftLoaded(placement) == 1)
            {
                AdPlacementFull adpl = getPlGift(placement);
                if (adpl != null)
                {
                    adpl.isloaded = false;
                }
            }
#endif
        }
        public override int getGiftLoaded(string placement)
        {
#if ENABLE_ADS_MAX
            AdPlacementFull adpl = getPlGift(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads gift {placement} max{adsType} getGiftLoaded not pl");
                return 0;
            }
            else
            {
#if use_load_all
                for (int i = 0; i < adpl.adECPM.list.Count; i++)
                {
                    if (adpl.adECPM.list[i].isLoaded)
                    {
                        giftAdNetwork = adpl.adECPM.list[i].adnetname;
                        return 1;
                    }
                }
                return 0;
#else
                //SdkUtil.logd($"ads gift {placement} max{adsType} getGiftLoaded getGiftLoaded=" + adpl.isloaded);
                if (adpl.isloaded)
                {
                    return 1;
                }
#endif
            }
#endif
            return 0;
        }
        protected override void tryloadGift(AdPlacementFull adpl)
        {
#if ENABLE_ADS_MAX
            if (!isAdsInited)
            {
                SdkUtil.logd($"ads gift {adpl.loadPl} max{adsType} tryloadGift not init");
                plWaitGift = adpl.loadPl;
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            if (adpl.countLoad >= toTryLoad)
            {
                SdkUtil.logd($"ads gift {adpl.loadPl} max{adsType} tryLoadgift over try");
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            if (adpl.adECPM.list.Count <= adpl.adECPM.idxCurrEcpm)
            {
                SdkUtil.logd($"ads gift {adpl.loadPl} max{adsType} tryLoadBanner index out of range idx={adpl.adECPM.idxCurrEcpm} l={adpl.adECPM.list.Count}");
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            string idLoad = "";
            adpl.isLoading = true;
            adpl.isloaded = false;
#if use_load_all
            adpl.countLoad = 0;
            for (int i = 0; i < adpl.adECPM.list.Count; i++)
            {
                if (!adpl.adECPM.list[i].isLoading && !adpl.adECPM.list[i].isLoaded)
                {
                    idLoad = adpl.adECPM.list[i].adsId;
                    SdkUtil.logd($"ads gift {adpl.loadPl} max{adsType} tryloadGift load={idLoad}");
                    adpl.adECPM.list[i].isLoading = true;
                    adpl.countLoad++;
                    MaxSdk.LoadRewardedAd(idLoad);
                    AdsHelper.onAdLoad(adpl.loadPl, "rewarded", idLoad, "applovin");
                    FIRhelper.logAdEvent("ads_gift_max_load");
                    AppsFlyerHelperScript.logAdEvent("ads_load", "", "rewarded", idLoad, "applovin", "");
                }
                else
                {
                    if (adpl.adECPM.list[i].isLoading)
                    {
                        adpl.countLoad++;
                    }
                    if (adpl.adECPM.list[i].isLoaded)
                    {
                        adpl.isloaded = true;
                    }
                    SdkUtil.logd($"ads gift {adpl.loadPl} max{adsType} tryloadGift id={adpl.adECPM.list[i].adsId} loading={adpl.adECPM.list[i].isLoading} loaded={adpl.adECPM.list[i].isLoaded}");
                }
            }
            if (adpl.countLoad == 0)
            {
                adpl.isLoading = false;
            }
#else
            idLoad = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
            SdkUtil.logd($"ads gift {adpl.loadPl} max{adsType} tryloadGift load={idLoad}");
#if ENABLE_ADS_AMAZON
            if (isGiftFirstLoad && amazonVideoRewardedSlotId != null && amazonVideoRewardedSlotId.Length > 3)
            {
                isGiftFirstLoad = false;

                var rewardedVideoAd = new APSVideoAdRequest(320, 480, amazonVideoRewardedSlotId);
                rewardedVideoAd.onSuccess += (adResponse) =>
                {
                    MaxSdk.SetRewardedAdLocalExtraParameter(idLoad, "amazon_ad_response", adResponse.GetResponse());
                    MaxSdk.LoadRewardedAd(idLoad);
                    timeLoadGift = SdkUtil.CurrentTimeMilis();
                };
                rewardedVideoAd.onFailedWithError += (adError) =>
                {
                    MaxSdk.SetRewardedAdLocalExtraParameter(idLoad, "amazon_ad_error", adError.GetAdError());
                    MaxSdk.LoadRewardedAd(idLoad);
                    timeLoadGift = SdkUtil.CurrentTimeMilis();
                };
                AdsHelper.onAdLoad(adpl.loadPl, "rewarded", idLoad, "applovin");
                FIRhelper.logAdEvent("ads_gift_max_load");
                AppsFlyerHelperScript.logAdEvent("ads_load", "", "rewarded", idLoad, "applovin", "");
                rewardedVideoAd.LoadAd();
            }
            else
            {
                AdsHelper.onAdLoad(adpl.loadPl, "rewarded", idLoad, "applovin");
                FIRhelper.logAdEvent("ads_gift_max_load");
                AppsFlyerHelperScript.logAdEvent("ads_load", "", "rewarded", idLoad, "applovin", "");
                MaxSdk.LoadRewardedAd(idLoad);
                timeLoadGift = SdkUtil.CurrentTimeMilis();
            }
#else
            MaxSdk.LoadRewardedAd(adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId);
            timeLoadGift = SdkUtil.CurrentTimeMilis();
            AdsHelper.onAdLoad(adpl.loadPl, "rewarded", idLoad, "applovin");
            FIRhelper.logAdEvent("ads_gift_max_load");
            AppsFlyerHelperScript.logAdEvent("ads_load", "", "rewarded", idLoad, "applovin", "");
#endif

#endif

#else
            SdkUtil.logd($"ads max{adsType} gift tryLoadgift not enable");
            var tmpcb = adpl.cbLoad;
            adpl.cbLoad = null;
            tmpcb(AD_State.AD_LOAD_FAIL);
#endif
        }
        public override void loadGift(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_MAX
            AdPlacementFull adpl = getPlGift(placement);
            if (adpl != null)
            {
                SdkUtil.logd($"ads gift {placement} max{adsType} loadGift");
#if use_load_all
                if (true) //(!adpl.isLoading)
#else
                if (!adpl.isloaded && !adpl.isLoading)
#endif
                {
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.cbLoad = cb;
                    adpl.setSetPlacementLoad(placement);
                    tryloadGift(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads gift {placement} max{adsType} loadGift isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                }
            }
            else
            {
                SdkUtil.logd($"ads gift {placement} max{adsType} loadGift not pl");
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
#if ENABLE_ADS_MAX
            AdPlacementFull adpl = getPlGift(placement);
            if (adpl != null)
            {
                SdkUtil.logd($"ads gift {placement} max{adsType} showGift timeDelay={timeDelay}");
                if (getGiftLoaded(placement) > 0)
                {
                    string idShow = "";
                    string netShow = "";
#if use_load_all
                    for (int i = 0; i < adpl.adECPM.list.Count; i++)
                    {
                        if (adpl.adECPM.list[i].isLoaded)
                        {
                            idShow = adpl.adECPM.list[i].adsId;
                            giftAdNetwork = adpl.adECPM.list[i].adnetname;
                            netShow = adpl.adECPM.list[i].adnetname;
                            break;
                        }
                    }
#else
                    idShow = giftIdLoaded;
                    netShow = giftAdNetwork;
#endif
                    SdkUtil.logd($"ads gift {placement} max{adsType} showGift show net={netShow} idShow={idShow}");
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    if (timeDelay > 0)
                    {
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            AdsHelper.onAdShowStart(placement, "rewarded", "applovin", "");
                            MaxSdk.ShowRewardedAd(idShow);
                        }, timeDelay);
                        return true;
                    }
                    else
                    {
                        AdsHelper.onAdShowStart(placement, "rewarded", "applovin", "");
                        MaxSdk.ShowRewardedAd(idShow);
                        return true;
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads gift {placement} max{adsType} showGift");
                return false;
            }
#endif
            return false;
        }

        private int getAdsType(string adsname)
        {
            if (adsname != null)
            {
                if (adsname.Contains("Google AdMob"))
                {
                    return 0;
                }
                else if (adsname.Contains("Facebook"))
                {
                    return 1;
                }
                else if (adsname.Contains("Unity Ads"))
                {
                    return 2;
                }
                else if (adsname.Contains("AdColony"))
                {
                    return 40;
                }
                else if (adsname.Contains("Chartboost"))
                {
                    return 41;
                }
                else if (adsname.Contains("Fyber"))
                {
                    return 42;
                }
                else if (adsname.Contains("Google Ad Manager"))
                {
                    return 43;
                }
                else if (adsname.Contains("InMobis"))
                {
                    return 44;
                }
                else if (adsname.Contains("IronSource"))
                {
                    return 3;
                }
                else if (adsname.Contains("Mintegral"))
                {
                    return 45;
                }
                else if (adsname.Contains("Tapjoy"))
                {
                    return 46;
                }
                else if (adsname.Contains("Pangle"))
                {
                    return 47;
                }
            }

            return 6;
        }

        //-------------------------------------------------------------------------------
#if ENABLE_ADS_MAX
        #region BANNER AD EVENT
        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adinfo)
        {
            if (dicPLBanner.ContainsKey(PLBnDefault) && bannerId.Contains(adUnitId))
            {
                AdPlacementBanner adpl = dicPLBanner[PLBnDefault];
                SdkUtil.logd($"ads bn {adpl.loadPl} max{adsType} OnBannerAdLoadedEvent adUnitId=" + adUnitId);
                string adsource = FIRhelper.getAdsourceMax(adinfo.NetworkName);
                AdsHelper.onAdLoadResult(adpl.loadPl, "banner", adUnitId, "applovin", adsource, true);
                Rect rbn = MaxSdk.GetBannerLayout(adUnitId);
#if UNITY_IOS || UNITY_IPHONE
                if (adinfo != null && adinfo.NetworkName != null && adinfo.NetworkName.Contains("Google AdMob"))
                {

                }
                else
                {
                    if (!SdkUtil.isiPad())
                    {
                        rbn.height -= 15;
                    }
                }
#endif
                advhelper.onGetBanner(adsType, rbn);
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
                if (adpl.isShow)
                {
                    SdkUtil.logd($"ads bn {adpl.loadPl} max{adsType} OnBannerAdLoadedEvent show");
                    if (bnWidth > 0)
                    {
                        MaxSdk.SetBannerWidth(adUnitId, bnWidth);
                    }
                    if (!adpl.isRealShow)
                    {
                        adpl.isRealShow = true;
                        MaxSdk.ShowBanner(adUnitId);
                    }
                    else
                    {
                        MaxSdk.UpdateBannerPosition(adUnitId, bnCurrPos);
                        //MaxSdk.ShowBanner(bannerId);
                    }

                    if (advhelper.bnCurrShow == adsType)
                    {
                        SdkUtil.logd($"ads bn {adpl.loadPl} max{adsType} OnBannerAdLoadedEvent hide other");
                        advhelper.hideOtherBanner(adsType);
                    }
                }
                else
                {
                    SdkUtil.logd($"ads bn {adpl.loadPl} max{adsType} OnBannerAdLoadedEvent hide");
                    adpl.isRealShow = false;
                    MaxSdk.HideBanner(bannerId);
                }
            }
            else
            {
                if (!dicPLBanner.ContainsKey(PLBnDefault))
                {
                    SdkUtil.logd($"ads bn max{adsType} OnBannerAdLoadedEvent not pl");
                }
            }
        }
        private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo err)
        {
            if (dicPLBanner.ContainsKey(PLBnDefault) && bannerId.Contains(adUnitId))
            {
                AdPlacementBanner adpl = dicPLBanner[PLBnDefault];
                SdkUtil.logd($"ads bn {adpl.loadPl} max{adsType} OnBannerAdLoadFailedEvent adUnitId={adUnitId}");
                AdsHelper.onAdLoadResult(adpl.loadPl, "banner", adUnitId, "applovin", "", false);
                if (adpl.isLoading)
                {
                    adpl.adECPM.idxCurrEcpm++;
                    if (adpl.adECPM.idxCurrEcpm < adpl.adECPM.list.Count)
                    {
                        tryLoadBanner(adpl);
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
                if (!dicPLBanner.ContainsKey(PLBnDefault))
                {
                    SdkUtil.logd($"ads bn max{adsType} OnBannerAdLoadFailedEvent not pl");
                }
            }
        }
        private void OnBannerAdClickEvent(string adUnitId, MaxSdkBase.AdInfo adinfo)
        {
            if (dicPLBanner.ContainsKey(PLBnDefault) && bannerId.Contains(adUnitId))
            {
                AdPlacementBanner adpl = dicPLBanner[PLBnDefault];
                SdkUtil.logd($"ads bn {adpl.showPl} max{adsType} OnBannerAdClickEvent adUnitId= {adUnitId}");
                SDKManager.Instance.onClickAd();
                string adsource = FIRhelper.getAdsourceMax(adinfo.NetworkName);
                AdsHelper.onAdClick(SDKManager.Instance.currPlacement, "banner", "applovin", adsource, adUnitId);
            }
            else
            {
                if (!dicPLBanner.ContainsKey(PLBnDefault))
                {
                    SdkUtil.logd($"ads bn max{adsType} OnBannerAdClickEvent not pl");
                }
            }
        }
        private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adinfo)
        {
            if (bannerId.Contains(adUnitId))
            {
                string adsource = FIRhelper.getAdsourceMax(adinfo.NetworkName);
                FIRhelper.logEventAdsPaidMax(SDKManager.Instance.currPlacement, "banner", adsource, adUnitId, adinfo.Revenue, adinfo.AdUnitIdentifier, MaxSdk.GetSdkConfiguration().CountryCode);
                TiktokBusiness.logAdRevenueMax(adinfo.Revenue, MaxSdk.GetSdkConfiguration().CountryCode, adinfo.NetworkName, adinfo.AdFormat, adinfo.AdUnitIdentifier, adinfo.Placement, adinfo.NetworkPlacement);
                AdsHelper.onAdImpression(SDKManager.Instance.currPlacement, adUnitId, "banner", "applovin", adsource, (float)adinfo.Revenue);

                FIRhelper.logEvent("show_ads_bn");
                FIRhelper.logEvent("show_ads_bn_nm_6");
            }
        }
        #endregion

        #region INTERSTITIAL AD EVENTS
        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adinfo)
        {
            if (dicPLFull.ContainsKey(PLFullDefault) && fullId.Contains(adUnitId))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                SdkUtil.logd($"ads full {adpl.loadPl} max{adsType} OnInterstitialLoadedEvent adUnitId={adUnitId} v={adinfo.Revenue}");
                string adsource = FIRhelper.getAdsourceMax(adinfo.NetworkName);
                AdsHelper.onAdLoadResult(adpl.loadPl, "interstitial", adUnitId, "applovin", adsource, true);
                FIRhelper.logAdEvent("ads_full_max_load_1");
                AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "interstitial", adUnitId, "applovin", "1");

                fullAdNetwork = adinfo.NetworkName;
                adsFullSubType = getAdsType(fullAdNetwork);
                fullIdLoaded = adUnitId;
#if use_load_all
                adpl.countLoad--;
                adpl.isloaded = true;
                adpl.setStateAd4Id(adUnitId, false, true, fullAdNetwork, adinfo.Revenue);
                if (adpl.isLoading)
                {
                    adpl.isLoading = false;
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        if (adpl.cbLoad != null)
                        {
                            var tmpcb = adpl.cbLoad;
                            adpl.cbLoad = null;

                            SdkUtil.logd($"ads full {adpl.loadPl} max{adsType} HandleInterstitialAdDidLoad = " + adUnitId + " -> cb ok");
                            tmpcb(AD_State.AD_LOAD_OK);
                        }
                    });
                }
#else
                adpl.countLoad = 0;
                adpl.isLoading = false;
                adpl.isloaded = true;
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    if (adpl.cbLoad != null)
                    {
                        var tmpcb = adpl.cbLoad;
                        adpl.cbLoad = null;

                        SdkUtil.logd($"ads full {adpl.loadPl} max{adsType} HandleInterstitialAdDidLoad = " + adUnitId + " -> cb ok");
                        tmpcb(AD_State.AD_LOAD_OK);
                    }
                });
#endif
            }
            else
            {
                if (!dicPLFull.ContainsKey(PLFullDefault))
                {
                    SdkUtil.logd($"ads full max{adsType} HandleInterstitialAdDidLoad not pl adid=" + adUnitId + ", net=" + adinfo.NetworkName);
                }
            }
        }
        private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo err)
        {
            if (dicPLFull.ContainsKey(PLFullDefault) && fullId.Contains(adUnitId))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                SdkUtil.logd($"ads full {adpl.loadPl} max{adsType} OnInterstitialFailedEvent adUnitId={adUnitId}");
                AdsHelper.onAdLoadResult(adpl.loadPl, "interstitial", adUnitId, "applovin", "", false);
                FIRhelper.logAdEvent("ads_full_max_load_0");
                AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "interstitial", adUnitId, "applovin", "0");

#if use_load_all
                adpl.countLoad--;
                adpl.setStateAd4Id(adUnitId, false, false, "", null);
                if (adpl.isLoading)
                {
                    adpl.isLoading = false;
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        if (adpl.cbLoad != null)
                        {
                            var tmpcb = adpl.cbLoad;
                            adpl.cbLoad = null;

                            SdkUtil.logd($"ads full {adpl.loadPl} max{adsType} OnInterstitialFailedEvent {adUnitId} -> {adpl.isloaded}");
                            if (adpl.isloaded)
                            {
                                tmpcb(AD_State.AD_LOAD_OK);
                            }
                            else
                            {
                                tmpcb(AD_State.AD_LOAD_FAIL);
                            }
                        }
                    });
                }
#else
                adpl.isloaded = false;
                adpl.isLoading = false;
                adpl.adECPM.idxCurrEcpm++;
                if (adpl.adECPM.idxCurrEcpm < adpl.adECPM.list.Count)
                {
                    tryLoadFull(adpl);
                }
                else
                {
                    if (adpl.cbLoad != null)
                    {
                        var tmpcb = adpl.cbLoad;
                        adpl.cbLoad = null;
                        tmpcb(AD_State.AD_LOAD_FAIL);
                    }
                }
#endif
            }
            else
            {
                if (!dicPLFull.ContainsKey(PLFullDefault))
                {
                    SdkUtil.logd($"ads full max{adsType} HandleInterstitialAdDidFailWithError not pl adid=" + adUnitId + ", err=" + err.ToString());
                }
            }
        }
        private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adinfo)
        {
            if (dicPLFull.ContainsKey(PLFullDefault) && fullId.Contains(adUnitId))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                SdkUtil.logd($"ads full {adpl.showPl} max{adsType} OnInterstitialDisplayedEvent adUnitId= {adUnitId}");
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        tmpcb(AD_State.AD_SHOW);
                    });
                }
            }
            else
            {
                if (!dicPLFull.ContainsKey(PLFullDefault))
                {
                    SdkUtil.logd($"ads full max{adsType} OnInterstitialDisplayedEvent not pl adid=" + adUnitId);
                }
            }
        }
        private void OnInterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo err, MaxSdkBase.AdInfo adinfo)
        {
            if (dicPLFull.ContainsKey(PLFullDefault) && fullId.Contains(adUnitId))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                SdkUtil.logd($"ads full {adpl.showPl} max{adsType} OnInterstitialFailedToDisplayEvent adUnitId= {adUnitId}");
#if !use_load_all
                adpl.isLoading = false;
                adpl.isloaded = false;
#endif
                adpl.setStateAd4Id(adUnitId, false, false, "", null);
                if (adpl.cbShow != null)
                {
                    advhelper.onCloseFullGift(true);
                    AdCallBack tmpcb = adpl.cbShow;
                    SdkUtil.logd($"ads max{adsType} full InterstitialAdShowFailedEvent 1");
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                }
                string adsource = FIRhelper.getAdsourceMax(adinfo.NetworkName);
                AdsHelper.onAdShowEnd(adpl.showPl, "interstitial", "applovin", adsource, adUnitId, false, err.Message);
                onFullClose(PLFullDefault);
            }
            else
            {
                if (!dicPLFull.ContainsKey(PLFullDefault))
                {
                    SdkUtil.logd($"ads full max{adsType} OnInterstitialFailedToDisplayEvent notpl adid=" + adUnitId + ", err=" + err.ToString());
                }
            }
        }
        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adinfo)
        {
            if (dicPLFull.ContainsKey(PLFullDefault) && fullId.Contains(adUnitId))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                string adsource = FIRhelper.getAdsourceMax(adinfo.NetworkName);
                AdsHelper.onAdClick(adpl.showPl, "interstitial", "applovin", adsource, adUnitId);
                if (!adsIsClick)
                {
                    adsIsClick = true;
                    AppsFlyerHelperScript.logAdEvent("ads_click", adpl.showPl, "interstitial", adUnitId, "applovin", "");
                }
            }
            else
            {
                if (!dicPLFull.ContainsKey(PLFullDefault))
                {
                    SdkUtil.logd($"ads full max{adsType} OnInterstitialClickedEvent not pl adid=" + adUnitId);
                }
            }
        }
        private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adinfo)
        {
            if (dicPLFull.ContainsKey(PLFullDefault) && fullId.Contains(adUnitId))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                SdkUtil.logd($"ads full {adpl.showPl} max{adsType} OnInterstitialDismissedEvent1 adUnitId={adUnitId}");
#if !use_load_all
                adpl.isLoading = false;
                adpl.isloaded = false;
#endif
                adpl.setStateAd4Id(adUnitId, false, false, "", null);
                if (adpl.cbShow != null)
                {
                    advhelper.onCloseFullGift(true);
                    AdCallBack tmpcb = adpl.cbShow;
                    SdkUtil.logd($"ads full {adpl.showPl} max{adsType} HandleInterstitialClosed 1");
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });
                }
                string adsource = FIRhelper.getAdsourceMax(adinfo.NetworkName);
                AdsHelper.onAdShowEnd(adpl.showPl, "interstitial", "applovin", adsource, adUnitId, true, "");
                onFullClose(PLFullDefault);
                adpl.cbShow = null;
            }
            else
            {
                if (!dicPLFull.ContainsKey(PLFullDefault))
                {
                    SdkUtil.logd($"ads full max{adsType} OnInterstitialDismissedEvent not pl adid=" + adUnitId);
                }
            }
        }
        private void OnInterstitialAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adinfo)
        {
            if (fullId.Contains(adUnitId))
            {
                adsIsClick = false;
                string spl = SDKManager.Instance.currPlacement;
                if (dicPLFull.ContainsKey(PLFullDefault))
                {
                    AdPlacementFull adpl = dicPLFull[PLFullDefault];
                    spl = adpl.showPl;
                }
                if (!isFull2)
                {
                    if (dicPLFull.ContainsKey(PLFullDefault))
                    {
                        AdPlacementFull adpl = dicPLFull[PLFullDefault];
                        FIRhelper.logEvent("show_ads_total_imp");
                        FIRhelper.logEvent("show_ads_full_imp");
                        FIRhelper.logEvent("show_ads_full_imp_6");
                    }
                    else
                    {
                        SdkUtil.logd($"ads full max{adsType} OnInterstitialAdRevenuePaidEvent not pl adid=" + adUnitId);
                    }
                }
                string adsource = FIRhelper.getAdsourceMax(adinfo.NetworkName);
                AdsHelper.onAdImpression(spl, adUnitId, "interstitial", "applovin", adsource, (float)adinfo.Revenue);
                FIRhelper.logEventAdsPaidMax(spl, "interstitial", adsource, adUnitId, adinfo.Revenue, adinfo.AdUnitIdentifier, MaxSdk.GetSdkConfiguration().CountryCode);
                TiktokBusiness.logAdRevenueMax(adinfo.Revenue, MaxSdk.GetSdkConfiguration().CountryCode, adinfo.NetworkName, adinfo.AdFormat, adinfo.AdUnitIdentifier, adinfo.Placement, adinfo.NetworkPlacement);

                FIRhelper.logAdEvent("ads_full_max_imp");
                AppsFlyerHelperScript.logAdEvent("ads_impression", spl, "interstitial", adUnitId, "applovin", "");
            }
        }
        #endregion

        #region REWARDED VIDEO AD EVENTS
        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adinfo)
        {
            if (dicPLGift.ContainsKey(PLGiftDefault) && giftId.Contains(adUnitId))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                SdkUtil.logd($"ads gift {adpl.loadPl} max{adsType} OnRewardedAdLoadedEvent adUnitId={adUnitId} v={adinfo.Revenue}");
                string adsource = FIRhelper.getAdsourceMax(adinfo.NetworkName);
                AdsHelper.onAdLoadResult(adpl.loadPl, "rewarded", adUnitId, "applovin", adsource, true);
                FIRhelper.logAdEvent("ads_gift_max_load_1");
                AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "rewarded", adUnitId, "applovin", "1");

                giftAdNetwork = adinfo.NetworkName;
                giftIdLoaded = adUnitId;
#if use_load_all
                adpl.countLoad--;
                adpl.isloaded = true;
                adpl.setStateAd4Id(adUnitId, false, true, giftAdNetwork, adinfo.Revenue);
                if (adpl.isLoading)
                {
                    adpl.isLoading = false;
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        if (adpl.cbLoad != null)
                        {
                            var tmpcb = adpl.cbLoad;
                            adpl.cbLoad = null;
                            SdkUtil.logd($"ads gift {adpl.loadPl} max{adsType} OnRewardedAdLoadedEvent = " + adUnitId + " -> cb ok");
                            tmpcb(AD_State.AD_LOAD_OK);
                        }
                    });
                }
#else
                adpl.countLoad = 0;
                adpl.isLoading = false;
                adpl.isloaded = true;
                giftIdLoaded = adUnitId;
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    if (adpl.cbLoad != null)
                    {
                        var tmpcb = adpl.cbLoad;
                        adpl.cbLoad = null;
                        SdkUtil.logd($"ads gift {adpl.loadPl} max{adsType} OnRewardedAdLoadedEvent = " + adUnitId + " -> cb ok");
                        tmpcb(AD_State.AD_LOAD_OK);
                    }
                });
#endif
            }
            else
            {
                if (!dicPLGift.ContainsKey(PLGiftDefault))
                {
                    SdkUtil.logd($"ads gift max{adsType} OnRewardedAdLoadedEvent not pl adid=" + adUnitId + ", net=" + adinfo.NetworkName);
                }
            }
        }
        private void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo err)
        {
            if (dicPLGift.ContainsKey(PLGiftDefault) && giftId.Contains(adUnitId))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                SdkUtil.logd($"ads gift {adpl.loadPl} max{adsType} OnRewardedAdFailedEvent adUnitId= {adUnitId}");
                AdsHelper.onAdLoadResult(adpl.loadPl, "rewarded", adUnitId, "applovin", "", false);
                FIRhelper.logAdEvent("ads_gift_max_load_0");
                AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "rewarded", adUnitId, "applovin", "0");

#if use_load_all
                adpl.countLoad--;
                adpl.setStateAd4Id(adUnitId, false, false, "", null);
                if (adpl.isLoading)
                {
                    adpl.isLoading = false;
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        if (adpl.cbLoad != null)
                        {
                            var tmpcb = adpl.cbLoad;
                            adpl.cbLoad = null;

                            SdkUtil.logd($"ads gift {adpl.loadPl} max{adsType} OnRewardedAdFailedEvent {adUnitId} -> {adpl.isloaded}");
                            if (adpl.isloaded)
                            {
                                tmpcb(AD_State.AD_LOAD_OK);
                            }
                            else
                            {
                                tmpcb(AD_State.AD_LOAD_FAIL);
                            }
                        }
                    });
                }
#else
                adpl.isLoading = false;
                adpl.isloaded = false;
                adpl.adECPM.idxCurrEcpm++;
                if (adpl.adECPM.idxCurrEcpm < adpl.adECPM.list.Count)
                {
                    tryloadGift(adpl);
                }
                else
                {
                    if (adpl.cbLoad != null)
                    {
                        var tmpcb = adpl.cbLoad;
                        adpl.cbLoad = null;
                        tmpcb(AD_State.AD_LOAD_FAIL);
                    }
                }
#endif
            }
            else
            {
                if (!dicPLGift.ContainsKey(PLGiftDefault))
                {
                    SdkUtil.logd($"ads gift max{adsType} OnRewardedAdFailedEvent not pl adid=" + adUnitId + ", err=" + err.ToString());
                }
            }
        }
        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adinfo)
        {
            if (dicPLGift.ContainsKey(PLGiftDefault) && giftId.Contains(adUnitId))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                if (!dicPLGift.ContainsKey(PLGiftDefault))
                {
                    SdkUtil.logd($"ads gift max{adsType} OnRewardedAdDisplayedEvent not pl adid=" + adUnitId);
                }
            }
        }
        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adinfo)
        {
            if (dicPLGift.ContainsKey(PLGiftDefault) && giftId.Contains(adUnitId))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                SdkUtil.logd($"ads gift {adpl.showPl} max{adsType} OnRewardedAdClickedEvent adUnitId= {adUnitId}");
                string adsource = FIRhelper.getAdsourceMax(adinfo.NetworkName);
                AdsHelper.onAdClick(adpl.showPl, "rewarded", "applovin", adsource, adUnitId);
                if (!adsIsClick)
                {
                    adsIsClick = true;
                    AppsFlyerHelperScript.logAdEvent("ads_click", adpl.showPl, "rewarded", adUnitId, "applovin", "");
                }
            }
            else
            {
                if (!dicPLGift.ContainsKey(PLGiftDefault))
                {
                    SdkUtil.logd($"ads gift max{adsType} OnRewardedAdClickedEvent not pl adid=" + adUnitId);
                }
            }
        }
        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo info)
        {
            if (dicPLGift.ContainsKey(PLGiftDefault) && giftId.Contains(adUnitId))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                SdkUtil.logd($"ads gift {adpl.showPl} max{adsType} OnRewardedAdReceivedRewardEvent adUnitId= {adUnitId}");
                isRewardCom = true;
            }
            else
            {
                if (!dicPLGift.ContainsKey(PLGiftDefault))
                {
                    SdkUtil.logd($"ads gift max{adsType} OnRewardedAdReceivedRewardEvent not pl adid=" + adUnitId);
                }
            }
        }
        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo err, MaxSdkBase.AdInfo adinfo)
        {
            if (dicPLGift.ContainsKey(PLGiftDefault) && giftId.Contains(adUnitId))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                SdkUtil.logd($"ads gift {adpl.showPl} max{adsType} OnRewardedAdFailedToDisplayEvent adUnitId= {adUnitId}");
#if !use_load_all
                adpl.isloaded = false;
                adpl.isLoading = false;
#endif
                adpl.setStateAd4Id(adUnitId, false, false, "", null);
                if (adpl.cbShow != null)
                {
                    advhelper.onCloseFullGift(false);
                    AdCallBack tmpcb = adpl.cbShow;
                    SdkUtil.logd($"ads gift {adpl.showPl} max{adsType} _cbAD fail");
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_FAIL); });
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                }
                string adsource = FIRhelper.getAdsourceMax(adinfo.NetworkName);
                AdsHelper.onAdShowEnd(adpl.showPl, "rewarded", "applovin", adsource, adUnitId, false, err.Message);
                onGiftClose(PLGiftDefault);
            }
            else
            {
                if (!dicPLGift.ContainsKey(PLGiftDefault))
                {
                    SdkUtil.logd($"ads gift max{adsType} OnRewardedAdFailedToDisplayEvent not pl adid=" + adUnitId + ", err=" + err.ToString());
                }
            }

        }
        private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adinfo)
        {
            if (dicPLGift.ContainsKey(PLGiftDefault) && giftId.Contains(adUnitId))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                SdkUtil.logd($"ads gift {adpl.showPl} max{adsType} OnRewardedAdDismissedEvent adUnitId= {adUnitId}");
#if !use_load_all
                adpl.isloaded = false;
                adpl.isLoading = false;
#endif
                adpl.setStateAd4Id(adUnitId, false, false, "", null);
                if (adpl.cbShow != null)
                {
                    advhelper.onCloseFullGift(false);
                    AdCallBack tmpcb = adpl.cbShow;
                    SdkUtil.logd($"ads gift {adpl.showPl} max{adsType} _cbAD != null");
                    if (isRewardCom)
                    {
                        SdkUtil.logd($"ads gift {adpl.showPl} max{adsType} _cbAD reward");
                        AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_OK); });
                    }
                    else
                    {
                        SdkUtil.logd($"ads gift {adpl.showPl} max{adsType} _cbAD fail");
                        AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_FAIL); });
                    }
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });
                }
                string adsource = FIRhelper.getAdsourceMax(adinfo.NetworkName);
                AdsHelper.onAdShowEnd(adpl.showPl, "rewarded", "applovin", adsource, adUnitId, true, "");
                onGiftClose(PLGiftDefault);
                isRewardCom = false;
                adpl.cbShow = null;
            }
            else
            {
                if (!dicPLGift.ContainsKey(PLGiftDefault))
                {
                    SdkUtil.logd($"ads gift max{adsType} OnRewardedAdDismissedEvent not pl adid=" + adUnitId);
                }
            }

        }
        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adinfo)
        {
            if (giftId.Contains(adUnitId))
            {
                adsIsClick = false;
                string spl = SDKManager.Instance.currPlacement;
                FIRhelper.logEvent("show_ads_total_imp");
                FIRhelper.logEvent("show_ads_reward_imp");
                FIRhelper.logEvent("show_ads_reward_imp_6");
                if (dicPLGift.ContainsKey(PLGiftDefault))
                {
                    AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                    spl = adpl.showPl;
                }
                string adsource = FIRhelper.getAdsourceMax(adinfo.NetworkName);
                AdsHelper.onAdImpression(spl, adUnitId, "rewarded", "applovin", adsource, (float)adinfo.Revenue);
                FIRhelper.logEventAdsPaidMax(spl, "rewarded", adsource, adUnitId, adinfo.Revenue, adinfo.AdUnitIdentifier, MaxSdk.GetSdkConfiguration().CountryCode);
                TiktokBusiness.logAdRevenueMax(adinfo.Revenue, MaxSdk.GetSdkConfiguration().CountryCode, adinfo.NetworkName, adinfo.AdFormat, adinfo.AdUnitIdentifier, adinfo.Placement, adinfo.NetworkPlacement);

                FIRhelper.logAdEvent("ads_gift_max_imp");
                AppsFlyerHelperScript.logAdEvent("ads_impression", spl, "rewarded", adUnitId, "applovin", "");
            }
            else
            {
                if (!dicPLGift.ContainsKey(PLGiftDefault))
                {
                    SdkUtil.logd($"ads gift max{adsType} OnRewardedAdRevenuePaidEvent not pl adid=" + adUnitId);
                }
            }
        }
        #endregion

#endif

    }
}