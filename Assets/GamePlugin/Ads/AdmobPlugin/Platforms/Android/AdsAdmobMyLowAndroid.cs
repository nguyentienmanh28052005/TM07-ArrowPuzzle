#if UNITY_ANDROID
using System;
using UnityEngine;
using mygame.sdk;

namespace mygame.plugin.Android
{
    public class AdsAdmobMyLowAndroid : AndroidJavaProxy, AdsAdmobMyIF
    {
        private const string AdsAdmobMyName = "mygame.plugin.myads.adsmob.AdsAdmobMy";
        private const string IFAdsAdmobMyName = "mygame.plugin.myads.adsmob.IFAdsAdmobMy";
        private AndroidJavaObject adsAdmobMy;

        public AdsAdmobMyLowAndroid() : base(IFAdsAdmobMyName)
        {
            using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    this.adsAdmobMy = new AndroidJavaObject(AdsAdmobMyName, activity, this);
                }
            }
        }

        #region IGoogleMobileAdsInterstitialClient implementation
        public void Initialize()
        {
            this.adsAdmobMy.Call("Initialize");
        }
        public void targetingAdContent(bool isChild, bool isUnderAgeConsent, int rating)
        {
        }
        public void setLog(bool isLog)
        {
            this.adsAdmobMy.Call("setLog", isLog);
        }
        public void cfBnRefresh(string config)
        {
        }
        public void cfBnAreaTouchex(int w, int h)
        {
        }
        public void trackingTiktok(int flagTrackingTiktok)
        {
        }
        //
        public void loadOpenAd(string placement, string adsId)
        {
            this.adsAdmobMy.Call("loadOpenAd", placement, adsId);
        }
        public bool showOpenAd(string placement, int timeDelay)
        {
            bool re = this.adsAdmobMy.Call<bool>("showOpenAd", placement);
            return re;
        }
        //
        public bool showBanner(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter)
        {
            return this.adsAdmobMy.Call<bool>("showBanner", placement, pos, width, maxH, orien, iPad, dxCenter);
        }
        public void loadBanner(string placement, string adsId, bool iPad)
        {
            this.adsAdmobMy.Call("loadBanner", placement, adsId, iPad);
        }
        public void hideBanner()
        {
            this.adsAdmobMy.Call("hideBanner");
        }
        public void destroyBanner()
        {
            this.adsAdmobMy.Call("destroyBanner");
        }
        //
        public bool showBannerCl(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter)
        {
            return this.adsAdmobMy.Call<bool>("showBannerCl", placement, pos, width, maxH, orien, iPad, dxCenter);
        }
        public void loadBannerCl(string placement, string adsId, bool iPad)
        {
            this.adsAdmobMy.Call("loadBannerCl", placement, adsId, iPad);
        }
        public void hideBannerCl()
        {
            this.adsAdmobMy.Call("hideBannerCl");
        }
        public void destroyBannerCl()
        {
            this.adsAdmobMy.Call("destroyBannerCl");
        }
        //
        public bool showBannerRect(string placement, int pos, float width, int maxH, float dxCenter, float dyVertical)
        {
            return this.adsAdmobMy.Call<bool>("showBannerRect", placement, pos, width, maxH, dxCenter, dyVertical);
        }
        public void loadBannerRect(string placement, string adsId)
        {
            this.adsAdmobMy.Call("loadBannerRect", placement, adsId);
        }
        public void hideBannerRect()
        {
            this.adsAdmobMy.Call("hideBannerRect");
        }
        public void destroyBannerRect()
        {
            this.adsAdmobMy.Call("destroyBannerRect");
        }
        //
        public bool showBnNt(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter, float dyVertical, int trefresh) { return false; }
        public void loadBnNt(string placement, string adsId, bool iPad) { }
        public void hideBnNt() { }
        public void destroyBnNt() { }
        //
        public void loadNativeCl(string placement, string adsId, int index, int orient)
        {
            this.adsAdmobMy.Call("loadNativeCl", placement, adsId, index, orient);
        }
        public bool showNativeCl(string placement, int pos, int width, float dxCenter, bool isHideBtClose, bool isLouWhenick)
        {
            return this.adsAdmobMy.Call<bool>("showNativeCl", placement, pos, width, dxCenter, isHideBtClose, isLouWhenick);
        }
        public void hideNativeCl()
        {
            this.adsAdmobMy.Call("hideNativeCl");
        }
        //
        public bool showRectNt(String placement, int pos, int orient, float width, float height, float dx, float dy)
        {
            return false;
        }
        public void loadRectNt(String placement, String adsId){}
        public void hideRectNt(){}
        //
        public void setCfNtFull(int v1, int v2, int v3, int v4, int v5, int v6) { }
        public void setCfNtdayClick(string cfdayclick) { }
        public void setCfNtFullFbExcluse(int rows, int columns, string areaExcluse) { }
        public void setCfNtCl(int v1, int v2, int v3, int v4) { }
        public void setCfNtClFbExcluse(int rows, int columns, string areaExcluse, int levelFlick) { }
        public void setTypeBnnt(bool isShowMedia) { }
        public void loadNativeFull(string placement, string adsId, int index, int orient)
        {
            this.adsAdmobMy.Call("loadNativeFull", placement, adsId, index);
        }
        public bool showNativeFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick, int layput, bool isAd2)
        {
            bool re = this.adsAdmobMy.Call<bool>("showNativeFull", placement, isNham, timeShowBtClose, timeDelay, layput);
            return re;
        }//
        public void loadNativeIcFull(string placement, string adsId, int index, int orient)
        {
            this.adsAdmobMy.Call("loadNativeIcFull", placement, adsId, index);
        }
        public bool showNativeIcFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick, int layput, bool isAd2)
        {
            bool re = this.adsAdmobMy.Call<bool>("showNativeIcFull", placement, isNham, timeShowBtClose, timeDelay, layput);
            return re;
        }
        //
        public void loadNativePgFull(string placement, int adPos, string adsId, int index)
        {
        }
        public bool showNativePgFull(string placement, bool isNham, int timeShowBtClose, int timeDelay)
        {
            return false;
        }
        //
        public void clearCurrFull(string placement)
        {
            this.adsAdmobMy.Call("clearCurrFull", placement);
        }
        public void loadFull(string placement, string adsId, int index)
        {
            this.adsAdmobMy.Call("loadFull", placement, adsId, index);
        }
        public bool showFull(string placement, int timeDelay)
        {
            bool re = this.adsAdmobMy.Call<bool>("showFull", placement, timeDelay);
            return re;
        }
        //
        public void loadFullRwInter(string placement, string adsId) { }
        public bool showFullRwInter(string placement, int timeDelay) { return false; }

        public void loadFullRwRw(string placement, string adsId) { }
        public bool showFullRwRw(string placement, int timeDelay) { return false; }
        //
        public void clearCurrGift(string placement)
        {
            this.adsAdmobMy.Call("clearCurrGift", placement);
        }
        public void loadGift(string placement, string adsId, int index)
        {
            this.adsAdmobMy.Call("loadGift", placement, adsId, index);
        }
        public bool showGift(string placement)
        {
            bool re = this.adsAdmobMy.Call<bool>("showGift", placement);
            return re;
        }
        //
        public void loadNativeGift(string placement, int adPos, string adsId, int index) { }
        public bool showNativeGift(string placement, bool isNham, int timeShowBtClose, int timeDelay) { return false; }

        #endregion

        #region Callbacks from UnityInterstitialAdListener.
        public void onLoaded(int typeAds, string placement, string adId, string adnet)
        {
            if (AdsAdmobMyLowBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                }
                else if (typeAds == 1)
                {
                }
                else if (typeAds == 2)
                {
                }
                else if (typeAds == 3)
                {
                }
                else if (typeAds == 4)
                {
                    AdsAdmobMyLowBridge.Instance.onFullLoaded(placement, adId, adnet);
                }
                else if (typeAds == 5)
                {
                }
            }
        }
        public void onLoadFail(int typeAds, string placement, string adId, string err)
        {
            if (AdsAdmobMyLowBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                }
                else if (typeAds == 1)
                {
                }
                else if (typeAds == 2)
                {
                }
                else if (typeAds == 3)
                {
                }
                else if (typeAds == 4)
                {
                    AdsAdmobMyLowBridge.Instance.onFullLoadFail(placement, adId, err);
                }
                else if (typeAds == 5)
                {
                }
            }
        }
        public void onShowFail(int typeAds, string placement, string adId, string adnet, string err)
        {
            if (AdsAdmobMyLowBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                }
                else if (typeAds == 1)
                {
                }
                else if (typeAds == 2)
                {
                }
                else if (typeAds == 3)
                {
                }
                else if (typeAds == 4)
                {
                    AdsAdmobMyLowBridge.Instance.onFullFailedToShow(placement, adId, adnet, err);
                }
                else if (typeAds == 5)
                {
                }
            }
        }
        public void onShowed(int typeAds, string placement, string adId, string adnet)
        {
            if (AdsAdmobMyLowBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                }
                else if (typeAds == 1)
                {
                }
                else if (typeAds == 2)
                {
                }
                else if (typeAds == 3)
                {
                }
                else if (typeAds == 4)
                {
                    AdsAdmobMyLowBridge.Instance.onFullShowed(placement, adId, adnet);
                }
                else if (typeAds == 5)
                {
                }
            }
        }
        public void onImpresstion(int typeAds, string placement, string adId, string adnet)
        {
            if (AdsAdmobMyLowBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                }
                else if (typeAds == 1)
                {
                }
                else if (typeAds == 2)
                {
                }
                else if (typeAds == 3)
                {
                }
                else if (typeAds == 4)
                {
                    AdsAdmobMyLowBridge.Instance.onFullImpresstion(placement, adId, adnet);
                }
                else if (typeAds == 5)
                {
                }
            }
        }
        public void onClick(int typeAds, string placement, string adId, string adnet)
        {
            if (AdsAdmobMyLowBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                }
                else if (typeAds == 1)
                {
                }
                else if (typeAds == 2)
                {
                }
                else if (typeAds == 3)
                {
                }
                else if (typeAds == 4)
                {
                    AdsAdmobMyLowBridge.Instance.onFullClick(placement, adId, adnet);
                }
                else if (typeAds == 5)
                {
                }
            }
        }
        public void onDismissed(int typeAds, string placement, string adId, string adnet)
        {
            if (AdsAdmobMyLowBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                }
                else if (typeAds == 1)
                {
                }
                else if (typeAds == 2)
                {
                }
                else if (typeAds == 3)
                {
                }
                else if (typeAds == 4)
                {
                    AdsAdmobMyLowBridge.Instance.onFullDismissed(placement, adId, adnet);
                }
                else if (typeAds == 5)
                {
                }
            }
        }
        public void onRewarded(int typeAds, string placement, string adId, string adnet)
        {
            if (AdsAdmobMyLowBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                }
                else if (typeAds == 1)
                {
                }
                else if (typeAds == 2)
                {
                }
                else if (typeAds == 3)
                {
                }
                else if (typeAds == 4)
                {
                }
                else if (typeAds == 5)
                {
                }
            }
        }
        public void onPaidEvent(int typeAds, string placement, string adId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (AdsAdmobMyLowBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                }
                else if (typeAds == 1)
                {
                }
                else if (typeAds == 2)
                {
                }
                else if (typeAds == 3)
                {
                }
                else if (typeAds == 4)
                {
                    AdsAdmobMyLowBridge.Instance.onFullPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
                }
                else if (typeAds == 5)
                {
                }
            }
        }
        public void onDesTroyAd(int typeAds, string placement, string adId)
        {
            if (AdsAdmobMyBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                }
                else if (typeAds == 1)
                {
                }
                else if (typeAds == 2)
                {
                }
                else if (typeAds == 3)
                {
                }
                else if (typeAds == 4)
                {
                }
                else if (typeAds == 5)
                {
                }
                else if (typeAds == 6)
                {
                }
            }
        }
        #endregion
    }
}

#endif
