#if UNITY_ANDROID
using System;
using UnityEngine;
using mygame.sdk;

namespace mygame.plugin.Android
{
    public class AdsMaxMyAndroid : AndroidJavaProxy, AdsMaxMyIF
    {
        private const string AdsMaxMyName = "mygame.plugin.myads.adsmax.AdsMaxMy";
        private const string IFAdsMaxMyName = "mygame.plugin.myads.adsmax.IFAdsMaxMy";
        private AndroidJavaObject adsMaxMy;

        public AdsMaxMyAndroid() : base(IFAdsMaxMyName)
        {
            using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    this.adsMaxMy = new AndroidJavaObject(AdsMaxMyName, activity, this);
                }
            }
        }

#region MaxClientax  implementation
        public void Initialize(string appId)
        {
            this.adsMaxMy.Call("Initialize", appId);
        }
        public void setLog(bool isLog)
        {
            this.adsMaxMy.Call("Initialize", isLog);
        }
        public void setTestDevices(string testDevices)
        {
            this.adsMaxMy.Call("setTestDevices", testDevices);
        }
        //
        public void loadOpenAd(string placement, string adsId)
        {
            this.adsMaxMy.Call("loadOpenAd", placement, adsId);
        }
        public bool showOpenAd(string placement, int timeDelay)
        {
            bool re = this.adsMaxMy.Call<bool>("showOpenAd", placement);
            return re;
        }
        //
        public void setCfNtFull(int v1, int v2, int v3, int v4, int v5, int v6)
        {
            this.adsMaxMy.Call("setCfNtFull", v1, v2, v3, v4, v5, v6 > 0);
        }
        public void loadNativeFull(string placement, string adsId, int orient)
        {
            this.adsMaxMy.Call("loadNativeFull", placement, adsId);
        }
        public bool showNativeFull(string placement, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick)
        {
            bool re = this.adsMaxMy.Call<bool>("showNativeFull", placement, timeShowBtClose, timeDelay);
            return re;
        }

#endregion

#region Callbacks from UnityInterstitialAdListener.
        public void onLoaded(int typeAds, string placement, string adId, string netName)
        {
            if (AdsMaxMyBridge.Instance != null)
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
                    AdsMaxMyBridge.Instance.onNtFullLoaded(placement, adId, netName);
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
                    AdsMaxMyBridge.Instance.onOpenLoaded(placement, adId, netName);
                }
            }
        }
        public void onLoadFail(int typeAds, string placement, string adId, string err)
        {
            if (AdsMaxMyBridge.Instance != null)
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
                    AdsMaxMyBridge.Instance.onNtFullLoadFail(placement, adId, err);
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
                    AdsMaxMyBridge.Instance.onOpenLoadFail(placement, adId, err);
                }
            }
        }
        public void onShowFail(int typeAds, string placement, string adId, string adnet, string err)
        {
            if (AdsMaxMyBridge.Instance != null)
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
                    AdsMaxMyBridge.Instance.onNtFullFailedToShow(placement, adId, adnet, err);
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
                    AdsMaxMyBridge.Instance.onOpenFailedToShow(placement, adId, adnet, err);
                }
            }
        }
        public void onShowed(int typeAds, string placement, string adnet, string adId)
        {
            if (AdsMaxMyBridge.Instance != null)
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
                    AdsMaxMyBridge.Instance.onNtFullShowed(placement, adId, adnet);
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
                    AdsMaxMyBridge.Instance.onOpenShowed(placement, adId, adnet);
                }
            }
        }
        public void onImpresstion(int typeAds, string placement, string adId, string adnet)
        {
            if (AdsMaxMyBridge.Instance != null)
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
                    AdsMaxMyBridge.Instance.onNtFullImpresstion(placement, adId, adnet);
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
                    AdsMaxMyBridge.Instance.onOpenImpresstion(placement, adId, adnet);
                }
            }
        }
        public void onClick(int typeAds, string placement, string adId, string adnet)
        {
            if (AdsMaxMyBridge.Instance != null)
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
                    AdsMaxMyBridge.Instance.onNtFullClick(placement, adId, adnet);
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
                    AdsMaxMyBridge.Instance.onOpenClick(placement, adId, adnet);
                }
            }
        }
        public void onDismissed(int typeAds, string placement, string adId, string adnet)
        {
            if (AdsMaxMyBridge.Instance != null)
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
                    AdsMaxMyBridge.Instance.onNtFullDismissed(placement, adId, adnet);
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
                    AdsMaxMyBridge.Instance.onOpenDismissed(placement, adId, adnet);
                }
            }
        }
        public void onRewarded(int typeAds, string placement, string adId, string adnet)
        {
            if (AdsMaxMyBridge.Instance != null)
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
        public void onPaidEvent(int typeAds, string placement, string adsId, string netName, string format, string adPlacement, string netPlacement, double value)
        {
            if (AdsMaxMyBridge.Instance != null)
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
                    AdsMaxMyBridge.Instance.onNtFullPaid(placement, adsId, netName, format, adPlacement, netPlacement, value);
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
                    AdsMaxMyBridge.Instance.onOpenPaid(placement, adsId, netName, format, adPlacement, netPlacement, value);
                }
            }
        }
        public void onDesTroyAd(int typeAds, string placement, string adId)
        {
            if (AdsMaxMyBridge.Instance != null)
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
