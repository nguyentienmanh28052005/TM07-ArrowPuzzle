#if UNITY_ANDROID
using System;
using UnityEngine;
using mygame.sdk;

namespace mygame.plugin.Android
{
    public class AdsMyYandexAndroid : AndroidJavaProxy, AdsMyYandexIF
    {
        private const string AdsMyYandexName = "mygame.plugin.myads.myyandex.AdsMyYandex";
        private const string IFAdsMyYandexName = "mygame.plugin.myads.myyandex.IFAdsMyYandex";
        private AndroidJavaObject adsMyYandex;

        public AdsMyYandexAndroid() : base(IFAdsMyYandexName)
        {
            using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    this.adsMyYandex = new AndroidJavaObject(AdsMyYandexName, activity, this);
                }
            }
        }

#region IMyYandexAdsInterstitialClient implementation
        public void Initialize()
        {
            this.adsMyYandex.Call("Initialize");
        }

        public void setTestMode(bool isTestMode)
        {
            this.adsMyYandex.Call("setTestMode", isTestMode);
        }

        public void addTestDevice(string deviceId)
        {
            this.adsMyYandex.Call("addTestDevice", deviceId);
        }

        public void setLog(bool isLog)
        {
            this.adsMyYandex.Call("Initialize", isLog);
        }

        public void setBannerPos(int pos, int width, float dxCenter)
        {
            this.adsMyYandex.Call("setBannerPos", pos, width, dxCenter);
        }

        public void showBanner(string adsId, int pos, int width, int orien, bool iPad, float dxCenter)
        {
            this.adsMyYandex.Call("showBanner", adsId, pos, width, orien, iPad, dxCenter);
        }
        public void hideBanner()
        {
            this.adsMyYandex.Call("hideBanner");
        }

        public void clearCurrFull()
        {
            this.adsMyYandex.Call("clearCurrFull");
        }

        public void loadFull(string adsId)
        {
            this.adsMyYandex.Call("loadFull", adsId);
        }
        public bool showFull()
        {
            bool re = this.adsMyYandex.Call<bool>("showFull");
            return re;
        }

        public void clearCurrGift()
        {
            this.adsMyYandex.Call("clearCurrGift");
        }

        public void loadGift(string adsId)
        {
            this.adsMyYandex.Call("loadGift", adsId);
        }
        public bool showGift()
        {
            bool re = this.adsMyYandex.Call<bool>("showGift");
            return re;
        }

#endregion

#region Callbacks from UnityInterstitialAdListener.
        public void onBannerLoaded()
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onBannerLoaded();
        }
        public void onBannerLoadFail(string err)
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onBannerLoadFail(err);
        }
        public void onBannerClose()
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onBannerClose();
        }
        public void onBannerOpen()
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onBannerOpen();
        }
        public void onBannerClick()
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onBannerClick();
        }
        public void onBannerImpression()
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onBannerImpression();
        }
        public void onBannerPaidEvent(int precisionType, string currencyCode, long valueMicros)
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onBannerPaid(precisionType, currencyCode, valueMicros);
        }

        //Full
        public void onFullLoaded()
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onFullLoaded();
        }
        public void onFullLoadFail(string err)
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onFullLoadFail(err);
        }
        public void onFullFailedToShow(string err)
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onFullFailedToShow(err);
        }
        public void onFullShowed()
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onFullShowed();
        }
        public void onFullClick()
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onFullClick();
        }
        public void onFullDismissed()
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onFullDismissed();
        }
        public void onFullImpresstion()
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onFullImpresstion();
        }
        public void onFullPaidEvent(int precisionType, string currencyCode, long valueMicros)
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onFullPaid(precisionType, currencyCode, valueMicros);
        }

        //gift
        public void onGiftLoaded()
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onGiftLoaded();
        }
        public void onGiftLoadFail(string err)
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onGiftLoadFail(err);
        }
        public void onGiftFailedToShow(string err)
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onGiftFailedToShow(err);
        }
        public void onGiftShowed()
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onGiftShowed();
        }
        public void onGiftClick()
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onGiftClick();
        }
        public void onGiftDismissed()
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onGiftDismissed();
        }
        public void onGiftImpresstion()
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onGiftImpresstion();
        }
        public void onGiftReward()
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onGiftReward();
        }
        public void onGiftPaidEvent(int precisionType, string currencyCode, long valueMicros)
        {
            if (AdsMyYandexBridge.Instance != null) AdsMyYandexBridge.Instance.onGiftPaid(precisionType, currencyCode, valueMicros);
        }
#endregion
    }
}

#endif
