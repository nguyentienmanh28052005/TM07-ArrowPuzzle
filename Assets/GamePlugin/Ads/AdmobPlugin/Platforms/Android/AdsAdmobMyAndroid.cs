#if UNITY_ANDROID
using System;
using UnityEngine;
using mygame.sdk;

namespace mygame.plugin.Android
{
    public class AdsAdmobMyAndroid : AndroidJavaProxy, AdsAdmobMyIF
    {
        private const string AdsAdmobMyName = "mygame.plugin.myads.adsmob.AdsAdmobMy";
        private const string IFAdsAdmobMyName = "mygame.plugin.myads.adsmob.IFAdsAdmobMy";
        private AndroidJavaObject adsAdmobMy;

        public AdsAdmobMyAndroid() : base(IFAdsAdmobMyName)
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
            this.adsAdmobMy.Call("targetingAdContent", isChild, isUnderAgeConsent, rating);
        }
        public void setLog(bool isLog)
        {
            this.adsAdmobMy.Call("setLog", isLog);
        }
        public void setTestDevices(string testDevices)
        {
            this.adsAdmobMy.Call("setTestDevices", testDevices);
        }
        public void cfBnRefresh(string config)
        {
            this.adsAdmobMy.Call("setBannerConfig", config);
        }
        public void cfBnAreaTouchex(int w, int h)
        {
            this.adsAdmobMy.Call("setBannerAreaTouchex", w, h);
        }
        public void trackingTiktok(int flagTrackingTiktok)
        {
            this.adsAdmobMy.Call("trackingTiktok", flagTrackingTiktok);
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
        public bool showBnNt(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter, float dyVertical, int trefresh)
        {
            return this.adsAdmobMy.Call<bool>("showBnNt", placement, pos, width, maxH, orien, iPad, dxCenter, dyVertical, trefresh);
        }
        public void loadBnNt(string placement, string adsId, bool iPad)
        {
            this.adsAdmobMy.Call("loadBnNt", placement, adsId, iPad);
        }
        public void hideBnNt()
        {
            this.adsAdmobMy.Call("hideBnNt");
        }
        public void destroyBnNt()
        {
            this.adsAdmobMy.Call("destroyBnNt");
        }
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
        public bool showRectNt(string placement, int pos, int orient, float width, float height, float dx, float dy)
        {
            return this.adsAdmobMy.Call<bool>("showRectNt", placement, pos, orient, width, height, dx, dy);
        }
        public void loadRectNt(string placement, string adsId)
        {
            this.adsAdmobMy.Call("loadRectNt", placement, adsId);
        }
        public void hideRectNt()
        {
            this.adsAdmobMy.Call("hideRectNt");
        }
        //
        public void setCfNtdayClick(string cfdayclick)
        {
            this.adsAdmobMy.Call("setCfNtday", cfdayclick);
        }
        public void setCfNtFull(int v1, int v2, int v3, int v4, int v5, int v6)
        {
            this.adsAdmobMy.Call("setCfNtFull", v1, v2, v3, v4, v5, v6 > 0);
        }

        public void setCfNtFullFbExcluse(int rows, int columns, string areaExcluse)
        {
            this.adsAdmobMy.Call("setCfAreaExcluseFlicAsFull", rows, columns, areaExcluse);
        }
        public void setCfNtCl(int v1, int v2, int v3, int v4)
        {
            this.adsAdmobMy.Call("setCfNtCl", v1, v2, v3, v4);
        }
        public void setCfNtClFbExcluse(int rows, int columns, string areaExcluse, int levelFlick)
        {
            this.adsAdmobMy.Call("setCfAreaExcluseFlicAscl", rows, columns, areaExcluse, levelFlick);
        }
        public void setTypeBnnt(bool isShowMedia)
        {
            this.adsAdmobMy.Call("setTypeBnnt", isShowMedia);
        }
        //
        public void loadNativeFull(string placement, string adsId, int index, int orient)
        {
            this.adsAdmobMy.Call("loadNativeFull", placement, adsId, index);
        }
        public bool showNativeFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick, int layput, bool isAd2)
        {
            bool re = this.adsAdmobMy.Call<bool>("showNativeFull", placement, isNham, timeShowBtClose, timeDelay, layput);
            return re;
        }
        //
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
            this.adsAdmobMy.Call("loadNativePgFull", placement, adPos, adsId, index);
        }
        public bool showNativePgFull(string placement, bool isNham, int timeShowBtClose, int timeDelay)
        {
            bool re = this.adsAdmobMy.Call<bool>("showNativePgFull", placement, isNham, timeShowBtClose, timeDelay);
            return re;
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
        public void loadFullRwInter(string placement, string adsId)
        {
            this.adsAdmobMy.Call("loadFullRwInter", placement, adsId);
        }
        public bool showFullRwInter(string placement, int timeDelay)
        {
            bool re = this.adsAdmobMy.Call<bool>("showFullRwInter", placement, timeDelay);
            return re;
        }

        public void loadFullRwRw(string placement, string adsId)
        {
            this.adsAdmobMy.Call("loadFullRwRw", placement, adsId);
        }
        public bool showFullRwRw(string placement, int timeDelay)
        {
            bool re = this.adsAdmobMy.Call<bool>("showFullRwRw", placement, timeDelay);
            return re;
        }
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
        public void loadNativeGift(string placement, int adPos, string adsId, int index)
        {
            this.adsAdmobMy.Call("loadGiftNt", placement, adPos, adsId, index);
        }
        public bool showNativeGift(string placement, bool isNham, int timeShowBtClose, int timeDelay)
        {
            bool re = this.adsAdmobMy.Call<bool>("showGiftNt", placement, isNham, timeShowBtClose, timeDelay);
            return re;
        }

#endregion

#region Callbacks from UnityInterstitialAdListener.
        public void onLoaded(int typeAds, string placement, string adId, string adnet)
        {
            if (AdsAdmobMyBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                    AdsAdmobMyBridge.Instance.onBannerLoaded(placement, adId, adnet);
                }
                else if (typeAds == 10)
                {
                    AdsAdmobMyBridge.Instance.onBNNTLoaded(placement, adId, adnet);
                }
                else if (typeAds == 1)
                {
                    AdsAdmobMyBridge.Instance.onBannerClLoaded(placement, adId, adnet);
                }
                else if (typeAds == 2)
                {
                    AdsAdmobMyBridge.Instance.onBannerRectLoaded(placement, adId, adnet);
                }
                else if (typeAds == 3)
                {
                    AdsAdmobMyBridge.Instance.onNtFullLoaded(placement, adId, adnet);
                }
                else if (typeAds == 30)
                {
                    AdsAdmobMyBridge.Instance.onNtIcFullLoaded(placement, adId, adnet);
                }
                else if (typeAds == 4)
                {
                    AdsAdmobMyBridge.Instance.onFullLoaded(placement, adId, adnet);
                }
                else if (typeAds == 5)
                {
                    AdsAdmobMyBridge.Instance.onGiftLoaded(placement, adId, adnet);
                }
                else if (typeAds == 6)
                {
                    AdsAdmobMyBridge.Instance.onNtClLoaded(placement, adId, adnet);
                }
                else if (typeAds == 7)
                {
                    AdsAdmobMyBridge.Instance.onOpenLoaded(placement, adId, adnet);
                }
                else if (typeAds == 8)
                {
                }
                else if (typeAds == 9)
                {
                }
                else if (typeAds == 11)
                {
                    AdsAdmobMyBridge.Instance.onFullRwInterLoaded(placement, adId, adnet);
                }
                else if (typeAds == 12)
                {
                    AdsAdmobMyBridge.Instance.onFullRwRwLoaded(placement, adId, adnet);
                }
                else if (typeAds == 13)
                {
                    AdsAdmobMyBridge.Instance.onRectNTLoaded(placement, adId, adnet);
                }
                else if (typeAds == 14)
                {
                    AdsAdmobMyBridge.Instance.onNtGiftLoaded(placement, adId, adnet);
                }
                else if (typeAds == 15)
                {
                    AdsAdmobMyBridge.Instance.onNtPgFullLoaded(placement, adId, adnet);
                }
            }
        }
        public void onLoadFail(int typeAds, string placement, string adId, string err)
        {
            if (AdsAdmobMyBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                    AdsAdmobMyBridge.Instance.onBannerLoadFail(placement, adId, err);
                }
                else if (typeAds == 10)
                {
                    AdsAdmobMyBridge.Instance.onBNNTLoadFail(placement, adId, err);
                }
                else if (typeAds == 1)
                {
                    AdsAdmobMyBridge.Instance.onBannerClLoadFail(placement, adId, err);
                }
                else if (typeAds == 2)
                {
                    AdsAdmobMyBridge.Instance.onBannerRectLoadFail(placement, adId, err);
                }
                else if (typeAds == 3)
                {
                    AdsAdmobMyBridge.Instance.onNtFullLoadFail(placement, adId, err);
                }
                else if (typeAds == 30)
                {
                    AdsAdmobMyBridge.Instance.onNtIcFullLoadFail(placement, adId, err);
                }
                else if (typeAds == 4)
                {
                    AdsAdmobMyBridge.Instance.onFullLoadFail(placement, adId, err);
                }
                else if (typeAds == 5)
                {
                    AdsAdmobMyBridge.Instance.onGiftLoadFail(placement, adId, err);
                }
                else if (typeAds == 6)
                {
                    AdsAdmobMyBridge.Instance.onNtClLoadFail(placement, adId, err);
                }
                else if (typeAds == 7)
                {
                    AdsAdmobMyBridge.Instance.onOpenLoadFail(placement, adId, err);
                }
                else if (typeAds == 8)
                {
                }
                else if (typeAds == 9)
                {
                }
                else if (typeAds == 11)
                {
                    AdsAdmobMyBridge.Instance.onFullRwInterLoadFail(placement, adId, err);
                }
                else if (typeAds == 12)
                {
                    AdsAdmobMyBridge.Instance.onFullRwRwLoadFail(placement, adId, err);
                }
                else if (typeAds == 13)
                {
                    AdsAdmobMyBridge.Instance.onRectNTLoadFail(placement, adId, err);
                }
                else if (typeAds == 14)
                {
                    AdsAdmobMyBridge.Instance.onNtGiftLoadFail(placement, adId, err);
                }
                else if (typeAds == 15)
                {
                    AdsAdmobMyBridge.Instance.onNtPgFullLoadFail(placement, adId, err);
                }
            }
        }
        public void onShowFail(int typeAds, string placement, string adId, string adnet, string err)
        {
            if (AdsAdmobMyBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                }
                else if (typeAds == 10)
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
                    AdsAdmobMyBridge.Instance.onNtFullFailedToShow(placement, adId, adId, err);
                }
                else if (typeAds == 30)
                {
                    AdsAdmobMyBridge.Instance.onNtIcFullFailedToShow(placement, adId, adId, err);
                }
                else if (typeAds == 4)
                {
                    AdsAdmobMyBridge.Instance.onFullFailedToShow(placement, adId, adId, err);
                }
                else if (typeAds == 5)
                {
                    AdsAdmobMyBridge.Instance.onGiftFailedToShow(placement, adId, adId, err);
                }
                else if (typeAds == 6)
                {
                }
                else if (typeAds == 7)
                {
                    AdsAdmobMyBridge.Instance.onOpenFailedToShow(placement, adId, adId, err);
                }
                else if (typeAds == 8)
                {
                }
                else if (typeAds == 9)
                {
                }
                else if (typeAds == 11)
                {
                    AdsAdmobMyBridge.Instance.onFullRwInterFailedToShow(placement, adId, adId, err);
                }
                else if (typeAds == 12)
                {
                    AdsAdmobMyBridge.Instance.onFullRwRwFailedToShow(placement, adId, adId, err);
                }
                else if (typeAds == 13)
                {
                }
                else if (typeAds == 14)
                {
                    AdsAdmobMyBridge.Instance.onNtGiftFailedToShow(placement, adId, adId, err);
                }
                else if (typeAds == 15)
                {
                    AdsAdmobMyBridge.Instance.onNtPgFullFailedToShow(placement, adId, adId, err);
                }
            }
        }
        public void onShowed(int typeAds, string placement, string adId, string adnet)
        {
            if (AdsAdmobMyBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                    AdsAdmobMyBridge.Instance.onBannerOpen(placement, adId, adnet);
                }
                else if (typeAds == 10)
                {
                    AdsAdmobMyBridge.Instance.onBNNTOpen(placement, adId, adnet);
                }
                else if (typeAds == 1)
                {
                    AdsAdmobMyBridge.Instance.onBannerClOpen(placement, adId, adnet);
                }
                else if (typeAds == 2)
                {
                    AdsAdmobMyBridge.Instance.onBannerRectOpen(placement, adId, adnet);
                }
                else if (typeAds == 3)
                {
                    AdsAdmobMyBridge.Instance.onNtFullShowed(placement, adId, adnet);
                }
                else if (typeAds == 30)
                {
                    AdsAdmobMyBridge.Instance.onNtIcFullShowed(placement, adId, adnet);
                }
                else if (typeAds == 4)
                {
                    AdsAdmobMyBridge.Instance.onFullShowed(placement, adId, adnet);
                }
                else if (typeAds == 5)
                {
                    AdsAdmobMyBridge.Instance.onGiftShowed(placement, adId, adnet);
                }
                else if (typeAds == 6)
                {
                    AdsAdmobMyBridge.Instance.onNtClShowed(placement, adId, adnet);
                }
                else if (typeAds == 7)
                {
                    AdsAdmobMyBridge.Instance.onOpenShowed(placement, adId, adnet);
                }
                else if (typeAds == 8)
                {
                }
                else if (typeAds == 9)
                {
                }
                else if (typeAds == 11)
                {
                    AdsAdmobMyBridge.Instance.onFullRwInterShowed(placement, adId, adnet);
                }
                else if (typeAds == 12)
                {
                    AdsAdmobMyBridge.Instance.onFullRwRwShowed(placement, adId, adnet);
                }
                else if (typeAds == 13)
                {
                }
                else if (typeAds == 14)
                {
                    AdsAdmobMyBridge.Instance.onNtGiftShowed(placement, adId, adnet);
                }
                else if (typeAds == 15)
                {
                    AdsAdmobMyBridge.Instance.onNtPgFullShowed(placement, adId, adnet);
                }
            }
        }
        public void onImpresstion(int typeAds, string placement, string adId, string adnet)
        {
            if (AdsAdmobMyBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                    AdsAdmobMyBridge.Instance.onBannerImpression(placement, adId, adnet);
                }
                else if (typeAds == 10)
                {
                    AdsAdmobMyBridge.Instance.onBNNTImpression(placement, adId, adnet);
                }
                else if (typeAds == 1)
                {
                    AdsAdmobMyBridge.Instance.onBannerClImpression(placement, adId, adnet);
                }
                else if (typeAds == 2)
                {
                    AdsAdmobMyBridge.Instance.onBannerRectImpression(placement, adId, adnet);
                }
                else if (typeAds == 3)
                {
                    AdsAdmobMyBridge.Instance.onNtFullImpresstion(placement, adId, adnet);
                }
                else if (typeAds == 30)
                {
                    AdsAdmobMyBridge.Instance.onNtIcFullImpresstion(placement, adId, adnet);
                }
                else if (typeAds == 4)
                {
                    AdsAdmobMyBridge.Instance.onFullImpresstion(placement, adId, adnet);
                }
                else if (typeAds == 5)
                {
                    AdsAdmobMyBridge.Instance.onGiftImpresstion(placement, adId, adnet);
                }
                else if (typeAds == 6)
                {
                    AdsAdmobMyBridge.Instance.onNtClImpresstion(placement, adId, adnet);
                }
                else if (typeAds == 7)
                {
                    AdsAdmobMyBridge.Instance.onOpenImpresstion(placement, adId, adnet);
                }
                else if (typeAds == 8)
                {
                }
                else if (typeAds == 9)
                {
                }
                else if (typeAds == 11)
                {
                    AdsAdmobMyBridge.Instance.onFullRwInterImpresstion(placement, adId, adnet);
                }
                else if (typeAds == 12)
                {
                    AdsAdmobMyBridge.Instance.onFullRwRwImpresstion(placement, adId, adnet);
                }
                else if (typeAds == 13)
                {
                    AdsAdmobMyBridge.Instance.onRectNTImpression(placement, adId, adnet);
                }
                else if (typeAds == 14)
                {
                    AdsAdmobMyBridge.Instance.onNtGiftImpresstion(placement, adId, adnet);
                }
                else if (typeAds == 15)
                {
                    AdsAdmobMyBridge.Instance.onNtPgFullImpresstion(placement, adId, adnet);
                }
            }
        }
        public void onClick(int typeAds, string placement, string adId, string adnet)
        {
            if (AdsAdmobMyBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                    AdsAdmobMyBridge.Instance.onBannerClick(placement, adId, adnet);
                }
                else if (typeAds == 10)
                {
                    AdsAdmobMyBridge.Instance.onBNNTClick(placement, adId, adnet);
                }
                else if (typeAds == 1)
                {
                    AdsAdmobMyBridge.Instance.onBannerClClick(placement, adId, adnet);
                }
                else if (typeAds == 2)
                {
                    AdsAdmobMyBridge.Instance.onBannerRectClick(placement, adId, adnet);
                }
                else if (typeAds == 3)
                {
                    AdsAdmobMyBridge.Instance.onNtFullClick(placement, adId, adnet);
                }
                else if (typeAds == 30)
                {
                    AdsAdmobMyBridge.Instance.onNtIcFullClick(placement, adId, adnet);
                }
                else if (typeAds == 4)
                {
                    AdsAdmobMyBridge.Instance.onFullClick(placement, adId, adnet);
                }
                else if (typeAds == 5)
                {
                    AdsAdmobMyBridge.Instance.onGiftClick(placement, adId, adnet);
                }
                else if (typeAds == 6)
                {
                    AdsAdmobMyBridge.Instance.onNtClClick(placement, adId, adnet);
                }
                else if (typeAds == 7)
                {
                    AdsAdmobMyBridge.Instance.onOpenClick(placement, adId, adnet);
                }
                else if (typeAds == 8)
                {
                }
                else if (typeAds == 9)
                {
                }
                else if (typeAds == 11)
                {
                    AdsAdmobMyBridge.Instance.onFullRwInterClick(placement, adId, adnet);
                }
                else if (typeAds == 12)
                {
                    AdsAdmobMyBridge.Instance.onFullRwRwClick(placement, adId, adnet);
                }
                else if (typeAds == 13)
                {
                    AdsAdmobMyBridge.Instance.onRectNTClick(placement, adId, adnet);
                }
                else if (typeAds == 14)
                {
                    AdsAdmobMyBridge.Instance.onNtGiftClick(placement, adId, adnet);
                }
                else if (typeAds == 15)
                {
                    AdsAdmobMyBridge.Instance.onNtPgFullClick(placement, adId, adnet);
                }
            }
        }
        public void onDismissed(int typeAds, string placement, string adId, string adnet)
        {
            if (AdsAdmobMyBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                    AdsAdmobMyBridge.Instance.onBannerClose(placement, adId, adnet);
                }
                else if (typeAds == 10)
                {
                    AdsAdmobMyBridge.Instance.onBNNTClose(placement, adId, adnet);
                }
                else if (typeAds == 1)
                {
                    AdsAdmobMyBridge.Instance.onBannerClClose(placement, adId, adnet);
                }
                else if (typeAds == 2)
                {
                    AdsAdmobMyBridge.Instance.onBannerRectClose(placement, adId, adnet);
                }
                else if (typeAds == 3)
                {
                    AdsAdmobMyBridge.Instance.onNtFullDismissed(placement, adId, adnet);
                }
                else if (typeAds == 30)
                {
                    AdsAdmobMyBridge.Instance.onNtIcFullDismissed(placement, adId, adnet);
                }
                else if (typeAds == 4)
                {
                    AdsAdmobMyBridge.Instance.onFullDismissed(placement, adId, adnet);
                }
                else if (typeAds == 5)
                {
                    AdsAdmobMyBridge.Instance.onGiftDismissed(placement, adId, adnet);
                }
                else if (typeAds == 6)
                {
                    AdsAdmobMyBridge.Instance.onNtClDismissed(placement, adId, adnet);
                }
                else if (typeAds == 7)
                {
                    AdsAdmobMyBridge.Instance.onOpenDismissed(placement, adId, adnet);
                }
                else if (typeAds == 8)
                {
                }
                else if (typeAds == 9)
                {
                }
                else if (typeAds == 11)
                {
                    AdsAdmobMyBridge.Instance.onFullRwInterDismissed(placement, adId, adnet);
                }
                else if (typeAds == 12)
                {
                    AdsAdmobMyBridge.Instance.onFullRwRwDismissed(placement, adId, adnet);
                }
                else if (typeAds == 14)
                {
                    AdsAdmobMyBridge.Instance.onNtGiftDismissed(placement, adId, adnet);
                }
                else if (typeAds == 15)
                {
                    AdsAdmobMyBridge.Instance.onNtPgFullDismissed(placement, adId, adnet);
                }
            }
        }
        public void onRewarded(int typeAds, string placement, string adId, string adnet)
        {
            if (AdsAdmobMyBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                }
                else if (typeAds == 10)
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
                else if (typeAds == 30)
                {
                }
                else if (typeAds == 4)
                {
                }
                else if (typeAds == 5)
                {
                    AdsAdmobMyBridge.Instance.onGiftReward(placement, adId, adnet);
                }
                else if (typeAds == 6)
                {
                }
                else if (typeAds == 8)
                {
                }
                else if (typeAds == 9)
                {
                }
                else if (typeAds == 11)
                {
                    AdsAdmobMyBridge.Instance.onFullRwInterReward(placement, adId, adnet);
                }
                else if (typeAds == 12)
                {
                    AdsAdmobMyBridge.Instance.onFullRwRwReward(placement, adId, adnet);
                }
            }
        }
        public void onPaidEvent(int typeAds, string placement, string adId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (AdsAdmobMyBridge.Instance != null)
            {
                if (typeAds == 0)
                {
                    AdsAdmobMyBridge.Instance.onBannerPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
                }
                else if (typeAds == 10)
                {
                    AdsAdmobMyBridge.Instance.onBNNTPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
                }
                else if (typeAds == 1)
                {
                    AdsAdmobMyBridge.Instance.onBannerClPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
                }
                else if (typeAds == 2)
                {
                    AdsAdmobMyBridge.Instance.onBannerRectPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
                }
                else if (typeAds == 3)
                {
                    AdsAdmobMyBridge.Instance.onNtFullPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
                }
                else if (typeAds == 30)
                {
                    AdsAdmobMyBridge.Instance.onNtIcFullPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
                }
                else if (typeAds == 4)
                {
                    AdsAdmobMyBridge.Instance.onFullPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
                }
                else if (typeAds == 5)
                {
                    AdsAdmobMyBridge.Instance.onGiftPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
                }
                else if (typeAds == 6)
                {
                    AdsAdmobMyBridge.Instance.onNtClPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
                }
                else if (typeAds == 7)
                {
                    AdsAdmobMyBridge.Instance.onOpenPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
                }
                else if (typeAds == 8)
                {
                }
                else if (typeAds == 9)
                {
                }
                else if (typeAds == 11)
                {
                    AdsAdmobMyBridge.Instance.onFullRwInterPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
                }
                else if (typeAds == 12)
                {
                    AdsAdmobMyBridge.Instance.onFullRwRwPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
                }
                else if (typeAds == 13)
                {
                    AdsAdmobMyBridge.Instance.onRectNTPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
                }
                else if (typeAds == 14)
                {
                    AdsAdmobMyBridge.Instance.onNtGiftPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
                }
                else if (typeAds == 15)
                {
                    AdsAdmobMyBridge.Instance.onNtPgFullPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
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
                else if (typeAds == 10)
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
                else if (typeAds == 30)
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
                else if (typeAds == 7)
                {
                }
                else if (typeAds == 8)
                {
                }
                else if (typeAds == 9)
                {
                }
                else if (typeAds == 11)
                {
                }
                else if (typeAds == 12)
                {
                }
            }
        }
#endregion
    }
}

#endif
