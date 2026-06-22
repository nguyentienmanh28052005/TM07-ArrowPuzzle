using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
using GoogleMobileAds.Api;
#endif

namespace mygame.sdk
{
    public enum MyAdNativeOrient
    {
        Vertical = 0,
        Horizontal
    }
    public enum E_PlacementNative
    {
        nt_default = 0,
        nt_choise_langage,
        nt_splash,
        nt_main,
        nt_shop,
        nt_loading,
        nt_win,
        nt_lose,
        nt_setting,
        nt_profile,
        nt_other1,
        nt_other2,
        nt_other3
    }
    public class AdsNativeWrapper : MonoBehaviour
    {
        public E_PlacementNative placement = E_PlacementNative.nt_default;
        public int defaultShow = 1;
        public MyAdNativeOrient AdOrientation = MyAdNativeOrient.Horizontal;
        public int layerObject = -1;
        public int timeRefresh = 45;
        public int timeRefreshWhenEnable = 60;
        public int timeDelayShow = 0;
        public bool stillShowWhenRmAd = true;
        public AdsNativeObject adNativeObject;

        long tLoadAd = 0;
        AdsAdmobMy adManager;
        bool isLoading = false;
        bool isLoaed = false;
        AdECPMs adEcmpLoad = null;
        float tCount4CheckRefresh;
        PromoGameOb myGamePromo = null;

        bool isStart = false;
        bool isShowMyGame = false;
        int currTypeAd = 0;
        bool isDestroyed = false;
        int statusAllowPushMoromo = 0;
        string adsourceLoaded = "";

        Texture2D ttTmpContent = null;

        private static List<LayerNativeOb> listLayerNative = new List<LayerNativeOb>();
        private static Dictionary<int, LayerNativeOb> dicLayerNative = new Dictionary<int, LayerNativeOb>();

        private void Awake()
        {
            adNativeObject.initAd();
        }

        private void Start()
        {
            tCount4CheckRefresh = 0;
            isLoading = false;
            isLoaed = false;
            isDestroyed = false;
            adManager = (AdsAdmobMy)AdsHelper.Instance.adsAdmobMy;
            if (AdsHelper.Instance != null)
            {
                isStart = true;
                checkShowStart();
            }
            else
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    isStart = true;
                    checkShowStart();
                }, 0.2f);
            }
        }

        private void OnEnable()
        {
            isDestroyed = false;
            checkAddLayer();
            if (timeRefresh < 20)
            {
                timeRefresh = 20;
            }
            if (timeRefreshWhenEnable < 15)
            {
                timeRefreshWhenEnable = 15;
            }
            if (isStart)
            {
                if (isLoaed)
                {
                    long t = SdkUtil.CurrentTimeMilis();
                    if ((t - tLoadAd) >= timeRefreshWhenEnable * 1000)
                    {
                        tLoadAd = t;
                        getAd();
                    }
                }
                else
                {
                    getAd();
                }
            }
            showAds();
        }

        private void OnDisable()
        {
            checkRemoveLayer(false);
        }

        private void OnDestroy()
        {
            isDestroyed = true;
            AdsHelper.Instance.freeNative(adNativeObject);
            checkRemoveLayer(true);
            if (ttTmpContent != null)
            {
                Destroy(ttTmpContent);
                ttTmpContent = null;
            }
        }

        void checkShowStart()
        {
            if (timeDelayShow >= 1)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    getAd();
                }, timeDelayShow);
            }
            else
            {
                getAd();
            }
        }

        private void Update()
        {
            if (isLoaed)
            {
                tCount4CheckRefresh += Time.deltaTime * Time.timeScale;
                if (tCount4CheckRefresh >= 1)
                {
                    tCount4CheckRefresh = 0;
                    long t = SdkUtil.CurrentTimeMilis();
                    if ((t - tLoadAd) >= timeRefresh * 1000)
                    {
                        tCount4CheckRefresh = -1f;
                        tLoadAd = t;
                        getAd();
                    }
                }
            }
            else if (isShowMyGame)
            {
                tCount4CheckRefresh += Time.deltaTime * Time.timeScale;
                if (tCount4CheckRefresh >= 1)
                {
                    tCount4CheckRefresh = 0;
                    long t = SdkUtil.CurrentTimeMilis();
                    if ((t - tLoadAd) >= timeRefresh * 1000)
                    {
                        tCount4CheckRefresh = -1f;
                        tLoadAd = t;
                        getPromo();
                    }
                }
            }
        }

        //
        public void checkAddLayer()
        {
            SdkUtil.logd($"ads native {placement} adntWrapper getPromo checkAddLayer");
            LayerNativeOb layer;
            if (layerObject > 0)
            {
                if (dicLayerNative.ContainsKey(layerObject))
                {
                    layer = dicLayerNative[layerObject];
                }
                else
                {
                    layer = new LayerNativeOb();
                    dicLayerNative.Add(layerObject, layer);
                    listLayerNative.Add(layer);
                }
            }
            else
            {
                layer = new LayerNativeOb();
                listLayerNative.Add(layer);
            }
            layer.listNativeOb.Add(this);
            if (listLayerNative.Count > 0)
            {
                for (int i = (listLayerNative.Count - 1); i >= 0; i--)
                {
                    bool isClick = (i == (listLayerNative.Count - 1));
                    foreach (var item in listLayerNative[i].listNativeOb)
                    {
                        item.adNativeObject.setEnableClick(isClick);
                        SdkUtil.logd($"ads native {item.placement} adntWrapper getPromo enable Click1={isClick}-type={item.adNativeObject.adType}");
                    }
                }
            }
        }
        public void checkRemoveLayer(bool isDestroy)
        {
            SdkUtil.logd($"ads native {placement} adntWrapper getPromo checkRemoveLayer isdes={isDestroy}");
            LayerNativeOb layer = null;
            if (layerObject > 0)
            {
                if (dicLayerNative.ContainsKey(layerObject))
                {
                    layer = dicLayerNative[layerObject];
                    if (layer.listNativeOb.Remove(this))
                    {
                        SdkUtil.logd($"ads native {placement} adntWrapper remove this from diclayer");
                        if (layer.listNativeOb.Count <= 0)
                        {
                            dicLayerNative.Remove(layerObject);
                            listLayerNative.Remove(layer);
                            SdkUtil.logd($"ads native {placement} adntWrapper remove layer from listlayer1");
                        }
                    }
                }
            }
            if (layer == null)
            {
                for (int i = (listLayerNative.Count - 1); i >= 0; i--)
                {
                    if (listLayerNative[i].listNativeOb.Contains(this))
                    {
                        listLayerNative[i].listNativeOb.Remove(this);
                        SdkUtil.logd($"ads native {placement} adntWrapper remove this from listlayer");
                        if (listLayerNative[i].listNativeOb.Count <= 0)
                        {
                            listLayerNative.RemoveAt(i);
                            SdkUtil.logd($"ads native {placement} adntWrapper remove layer from listlayer2");
                        }
                        break;
                    }
                }
            }
            if (listLayerNative.Count > 0)
            {
                for (int i = (listLayerNative.Count - 1); i >= 0; i--)
                {
                    bool isClick = (i == (listLayerNative.Count - 1));
                    foreach (var item in listLayerNative[i].listNativeOb)
                    {
                        item.adNativeObject.setEnableClick(isClick);
                        SdkUtil.logd($"ads native {item.placement} adntWrapper getPromo enable Click2={isClick}-type={item.adNativeObject.adType}");
                    }
                }
            }
        }
        void getPromo()
        {
            var cf = AdsHelper.Instance.getCfAdsPlacement(placement.ToString(), defaultShow);
            if (cf != null && cf.flagShow == 0)
            {
                SdkUtil.logd($"ads native {placement} adntWrapper getPromo cf not show");
                adNativeObject.gameObject.SetActive(false);
                return;
            }
            SdkUtil.logd($"ads native {placement} adntWrapper getPromo");
            myGamePromo = FIRhelper.Instance.getGame4Native(AdOrientation);
            loadAndShowPromo();
        }
        void loadAndShowPromo()
        {
            bool isrm = AdsHelper.isRemoveAds(0);
            SdkUtil.logd($"ads native {placement} adntWrapper loadAndShowPromo isrm={isrm} - stillShowWhenRmAd={stillShowWhenRmAd}");
            if (!stillShowWhenRmAd && isrm) //vvv
            {
                gameObject.SetActive(false);
                return;
            }
            if (myGamePromo != null)
            {
                SdkUtil.logd($"ads native {placement} adntWrapper loadAndShowPromo=" + myGamePromo.name);
                statusAllowPushMoromo = 0;
                int iw = 556;
                int ih = 256;
                string linkContent = myGamePromo.imgH;
                if (AdOrientation == MyAdNativeOrient.Vertical)
                {
                    iw = 256;
                    ih = 292;
                    linkContent = myGamePromo.imgV;
                }
                ImageLoader.loadImageTexture(linkContent, iw, ih, (tt) =>
                {
                    SdkUtil.logd($"ads native {placement} adntWrapper loadAndShowPromo on load content");
                    if (!isDestroyed && tt != null)
                    {
                        if (adNativeObject.isActiveIcon())
                        {
                            downicGame(tt);
                        }
                        else
                        {
                            pushPromoGame(myGamePromo, null, tt);
                        }
                    }
                });
            }
        }
        void downicGame(Texture2D ttContent)
        {
            ttTmpContent = ttContent;
            ImageLoader.loadImageTexture(myGamePromo.getImg(0), 100, 100, (tt) =>
            {
                SdkUtil.logd($"ads native {placement} adntWrapper downicGame on load icon");
                if (!isDestroyed && (tt != null || ttContent != null))
                {
                    ttTmpContent = null;
                    pushPromoGame(myGamePromo, tt, ttContent);
                }
            });
        }
        void pushPromoGame(PromoGameOb game, Texture2D ttIcon, Texture2D ttContent)
        {
            if (!isDestroyed && statusAllowPushMoromo >= 0)
            {
                SdkUtil.logd($"ads native {placement} adntWrapper pushPromoGame={game.name}");
                tLoadAd = SdkUtil.CurrentTimeMilis();
                isShowMyGame = true;
                currTypeAd = 2;
                adNativeObject.pushPromoToNative(game, ttIcon, ttContent);
                adNativeObject.switchAdType(1);
            }
        }
        void getAd()
        {
            var cf = AdsHelper.Instance.getCfAdsPlacement(placement.ToString(), defaultShow);
            if (cf != null && cf.flagShow == 0)
            {
                SdkUtil.logd($"ads native {placement} adntWrapper getAd cf not show");
                adNativeObject.gameObject.SetActive(false);
                return;
            }
#if !UNITY_EDITOR
            if (!isLoaed)
            {
                if (myGamePromo != null)
                {
                    if (!isShowMyGame)
                    {
                        loadAndShowPromo();
                        statusAllowPushMoromo = 1;
                    }
                }
                else
                {
                    getPromo();
                    statusAllowPushMoromo = 1;
                }
            }
            AdsHelper.Instance.showNative(placement.ToString(), adNativeObject, isLoaed, stillShowWhenRmAd, (state) =>
            {
                if (state == AD_State.AD_LOAD_OK)
                {
                    if (statusAllowPushMoromo == 1)
                    {
                        statusAllowPushMoromo = -1;
                    }
                    currTypeAd = 1;
                    isShowMyGame = false;
                    isLoaed = true;
                    tLoadAd = SdkUtil.CurrentTimeMilis();
                    adNativeObject.switchAdType(0);
                }
                else if (state == AD_State.AD_LOAD_FAIL)
                {
                    if (!isLoaed)
                    {
                        getPromo();
                    }
                }
                else if (state == AD_State.AD_LOAD_FROM_EXIST)
                {
                }
            }, defaultShow);
#else
            getPromo();
#endif
        }
        void showAds()
        {
            bool isrm = AdsHelper.isRemoveAds(0);
            SdkUtil.logd($"ads native {placement} adntWrapper showAds not show isrm={isrm} - stillShowWhenRmAd={stillShowWhenRmAd}");
            if (!stillShowWhenRmAd && isrm) //vvv
            {
                SdkUtil.logd($"ads native {placement} adntWrapper showAds not show isrm");
                gameObject.SetActive(false);
                return;
            }
            if (AdsHelper.Instance != null)
            {
                var cf = AdsHelper.Instance.getCfAdsPlacement(placement.ToString(), defaultShow);
                if (cf != null && cf.flagShow == 0)
                {
                    SdkUtil.logd($"ads native {placement} adntWrapper showAds cf not show");
                    adNativeObject.gameObject.SetActive(false);
                    return;
                }
            }
#if UNITY_EDITOR
            adNativeObject.gameObject.SetActive(true);
#else
            adNativeObject.gameObject.SetActive(isLoaed || isShowMyGame);
#endif
        }
        void loadAd()
        {
            if (!isLoading)
            {
                AdPlacementNative adpl = adManager.getPlNt(placement.ToString());
                if (adpl != null)
                {
                    isLoading = true;
                    adEcmpLoad = adpl.adECPM;
                    adpl.setSetPlacementLoad(SDKManager.Instance.currPlacement);
                    tryLoadNative(0);
                }
            }
            else
            {
                SdkUtil.logd($"ads native {placement} adntWrapper loadAd isloading");
            }
        }
        void tryLoadNative(int idxId)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY

            if (adEcmpLoad != null && adEcmpLoad.list.Count > 0 && idxId < adEcmpLoad.list.Count)
            {
                int tmpidx = idxId;
                string idload = adEcmpLoad.list[tmpidx].adsId;
                SdkUtil.logd($"ads native {placement} adntWrapper tryload id=" + idload);
                AdLoader adLoader = new AdLoader.Builder(idload)
                    .ForNativeAd()
                    .Build();
                adLoader.OnNativeAdLoaded += (sender, nativeArgs) => { this.HandleNativeAdLoaded(placement.ToString(), idload, sender, nativeArgs); };
                adLoader.OnAdFailedToLoad += (sender, nativeFailArgs) => { this.HandleAdFailedToLoad(placement.ToString(), tmpidx, idload, sender, nativeFailArgs); };
                adLoader.OnNativeAdImpression += (sender, evntArgs) => { this.HandleNativeAdImpression(placement.ToString(), idload, sender, evntArgs); };
                adLoader.OnNativeAdOpening += (sender, evntArgs) => { this.HandleNativeAdOpening(placement.ToString(), idload, sender, evntArgs); };
                adLoader.OnNativeAdClicked += (sender, evntArgs) => { this.HandleNativeAdClicked(placement.ToString(), idload, sender, evntArgs); };
                adLoader.OnNativeAdClosed += (sender, evntArgs) => { this.HandleNativeAdClosed(placement.ToString(), idload, sender, evntArgs); };
                adLoader.LoadAd(new AdRequest());
            }
            else
            {
                SdkUtil.logd($"ads native {placement} adntWrapper tryload not id idxId={idxId}");
                isLoading = false;
            }
#else
            isLoading = false;
#endif
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
        #region native ads
        private void HandleNativeAdLoaded(string placement, string adsId, object sender, NativeAdEventArgs args)
        {
            SdkUtil.logd($"ads native {placement} adntWrapper HandleNativeAdLoaded");

            AdsNativeAdmob adnt = new AdsNativeAdmob();
            isLoading = false;
            tLoadAd = SdkUtil.CurrentTimeMilis();
            adnt.adUnitId = adsId;
            adnt.isLoaded = true;
            adnt.isLoading = false;
            adnt.nativeAd = args.nativeAd;
            adnt.tLoaded = tLoadAd;
            adsourceLoaded = args.nativeAd.GetResponseInfo().GetMediationAdapterClassName();
            adnt.nativeAd.OnPaidEvent += (sender, adValue) => { HandleNativeAdPaidEvent(sender, adsId, adsourceLoaded, adValue); };
            adNativeObject.adNative = adnt;
            AdsProcessCB.Instance().Enqueue(() =>
            {
                adNativeObject.pushNative2GameObject(placement, adnt.nativeAd, null);
            }, 0.001f);
        }
        private void HandleAdFailedToLoad(string placement, int idxId, string adsId, object sender, AdFailedToLoadEventArgs args)
        {
            SdkUtil.logd($"ads native {placement} adntWrapper HandleAdFailedToLoad=" + args.LoadAdError.GetMessage());
            int idx = idxId + 1;
            if (idx < adEcmpLoad.list.Count)
            {
                tryLoadNative(idx);
            }
        }
        private void HandleNativeAdPaidEvent(object sender, string adsId, string adnet, AdValueEventArgs adValue)
        {
            SdkUtil.logd($"ads native adntWrapper HandleNativeAdPaidEvent v={1000 * adValue.AdValue.Value}-{adValue.AdValue.CurrencyCode}-{adValue.AdValue.Precision}");
            string spl = SDKManager.Instance.currPlacement;
            string adformat = FIRhelper.getAdformatAdmob(12);
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            long vapost = 1000 * adValue.AdValue.Value;
            FIRhelper.logEventAdsPaidAdmob(spl, adformat, "admob", adsId, vapost, vapost, adValue.AdValue.CurrencyCode);
            AdsHelper.onAdImpression(spl, adsId, adformat, "admob", adsource, (float)((float)adValue.AdValue.Value/1000000.0f), vapost);
            FIRhelper.logEvent("show_ads_nt");
            FIRhelper.logEvent("show_ads_nt_0");
        }
        private void HandleNativeAdImpression(string placement, string adsId, object sender, EventArgs args)
        {
            SdkUtil.logd($"ads native {placement} adntWrapper HandleNativeAdImpression id={adsId}");
        }
        private void HandleNativeAdOpening(string placement, string adsId, object sender, EventArgs args)
        {
            SdkUtil.logd($"ads native {placement} adntWrapper HandleNativeAdOpening id={adsId}");
        }
        private void HandleNativeAdClicked(string placement, string adsId, object sender, EventArgs args)
        {
            SdkUtil.logd($"ads native {placement} adntWrapper HandleNativeAdClicked id={adsId}");
            SDKManager.Instance.onClickAd();
        }
        private void HandleNativeAdClosed(string placement, string adsId, object sender, EventArgs args)
        {
            SdkUtil.logd($"ads native {placement} adntWrapper HandleNativeAdClosed id={adsId}");
        }
        #endregion
#endif
    }

    public class LayerNativeOb
    {
        public List<AdsNativeWrapper> listNativeOb = new List<AdsNativeWrapper>();
    }
}