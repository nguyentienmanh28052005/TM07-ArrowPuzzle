#if UNITY_ANDROID
using System;
using UnityEngine;
using mygame.sdk;

namespace mygame.plugin.Android
{
    public class AdsMyTargetAndroid : AndroidJavaProxy, AdsMyTargetIF
    {
        private const string AdsMyTargetName = "mygame.plugin.myads.mytarget.AdsMyTarget";
        private const string IFAdsMyTargetName = "mygame.plugin.myads.mytarget.IFAdsMyTarget";
        private AndroidJavaObject adsMyTarget;

        public AdsMyTargetAndroid() : base(IFAdsMyTargetName)
        {
            using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    this.adsMyTarget = new AndroidJavaObject(AdsMyTargetName, activity, this);
                }
            }
        }

#region IMyTargetAdsInterstitialClient implementation
        public void Initialize()
        {
            this.adsMyTarget.Call("Initialize");
        }

        public void setTestMode(bool isTestMode)
        {
            this.adsMyTarget.Call("setTestMode", isTestMode);
        }

        public void addTestDevice(string deviceId)
        {
            this.adsMyTarget.Call("addTestDevice", deviceId);
        }

        public void setLog(bool isLog)
        {
            this.adsMyTarget.Call("Initialize", isLog);
        }

        public void setBannerPos(int pos, int width, float dxCenter)
        {
            this.adsMyTarget.Call("setBannerPos", pos, width, dxCenter);
        }

        public void showBanner(string adsId, int pos, int width, int orien, bool iPad, float dxCenter)
        {
            this.adsMyTarget.Call("showBanner", adsId, pos, width, orien, iPad, dxCenter);
        }
        public void hideBanner()
        {
            this.adsMyTarget.Call("hideBanner");
        }

        public void clearCurrFull()
        {
            this.adsMyTarget.Call("clearCurrFull");
        }

        public void loadFull(string adsId)
        {
            this.adsMyTarget.Call("loadFull", adsId);
        }
        public bool showFull()
        {
            bool re = this.adsMyTarget.Call<bool>("showFull");
            return re;
        }

        public void clearCurrGift()
        {
            this.adsMyTarget.Call("clearCurrGift");
        }

        public void loadGift(string adsId)
        {
            this.adsMyTarget.Call("loadGift", adsId);
        }
        public bool showGift()
        {
            bool re = this.adsMyTarget.Call<bool>("showGift");
            return re;
        }

#endregion

#region Callbacks from UnityInterstitialAdListener.
        public void onBannerLoaded()
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onBannerLoaded();
        }
        public void onBannerLoadFail(string err)
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onBannerLoadFail(err);
        }
        public void onBannerClose()
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onBannerClose();
        }
        public void onBannerOpen()
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onBannerOpen();
        }
        public void onBannerClick()
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onBannerClick();
        }
        public void onBannerImpression()
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onBannerImpression();
        }
        public void onBannerPaidEvent(int precisionType, string currencyCode, long valueMicros)
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onBannerPaid(precisionType, currencyCode, valueMicros);
        }

        //Full
        public void onFullLoaded()
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onFullLoaded();
        }
        public void onFullLoadFail(string err)
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onFullLoadFail(err);
        }
        public void onFullFailedToShow(string err)
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onFullFailedToShow(err);
        }
        public void onFullShowed()
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onFullShowed();
        }
        public void onFullClick()
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onFullClick();
        }
        public void onFullDismissed()
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onFullDismissed();
        }
        public void onFullImpresstion()
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onFullImpresstion();
        }
        public void onFullPaidEvent(int precisionType, string currencyCode, long valueMicros)
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onFullPaid(precisionType, currencyCode, valueMicros);
        }

        //gift
        public void onGiftLoaded()
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onGiftLoaded();
        }
        public void onGiftLoadFail(string err)
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onGiftLoadFail(err);
        }
        public void onGiftFailedToShow(string err)
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onGiftFailedToShow(err);
        }
        public void onGiftShowed()
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onGiftShowed();
        }
        public void onGiftClick()
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onGiftClick();
        }
        public void onGiftDismissed()
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onGiftDismissed();
        }
        public void onGiftImpresstion()
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onGiftImpresstion();
        }
        public void onGiftReward()
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onGiftReward();
        }
        public void onGiftPaidEvent(int precisionType, string currencyCode, long valueMicros)
        {
            if (AdsMyTargetBridge.Instance != null) AdsMyTargetBridge.Instance.onGiftPaid(precisionType, currencyCode, valueMicros);
        }
#endregion
    }
}

#endif
