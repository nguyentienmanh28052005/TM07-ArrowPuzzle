#if UNITY_ANDROID
using System;
using UnityEngine;
using mygame.sdk;

namespace mygame.plugin.Android
{
    public class AdsFbMyAndroid : AndroidJavaProxy, AdsFbMyIF
    {
        private const string adsFbMyName = "mygame.plugin.myads.adsfb.AdsFbMy";
        private const string IFadsFbMyName = "mygame.plugin.myads.adsfb.IFAdsFbMy";
        private AndroidJavaObject adsFbMy;

        public AdsFbMyAndroid() : base(IFadsFbMyName)
        {
            using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    this.adsFbMy = new AndroidJavaObject(adsFbMyName, activity, this);
                }
            }
        }

#region IGoogleMobileAdsInterstitialClient implementation
        public void Initialize()
        {
            this.adsFbMy.Call("Initialize");
        }
        public void setLog(bool isLog)
        {
            this.adsFbMy.Call("setLog", isLog);
        }
        public void setTestDevices(string testDevices)
        {
            this.adsFbMy.Call("setTestDevices", testDevices);
        }
        //
        public void setCfNtFull(int v1, int v2, int v3, int v4, int v5, int v6)
        {
            this.adsFbMy.Call("setCfNtFull", v1, v2, v3, v4, v5, v6 > 0);
        }

        public void setCfNtFullFbExcluse(int rows, int columns, string areaExcluse)
        {
            this.adsFbMy.Call("setCfAreaExcluseFlicAsFull", rows, columns, areaExcluse);
        }
        //
        public void loadNativeFull(string placement, string adsId, int orient)
        {
            this.adsFbMy.Call("loadNativeFull", placement, adsId);
        }
        public bool showNativeFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick)
        {
            bool re = this.adsFbMy.Call<bool>("showNativeFull", placement, isNham, timeShowBtClose, timeDelay);
            return re;
        }
        //
        public void loadNativeIcFull(string placement, string adsId, int orient)
        {
            this.adsFbMy.Call("loadNativeIcFull", placement, adsId);
        }
        public bool showNativeIcFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick)
        {
            bool re = this.adsFbMy.Call<bool>("showNativeIcFull", placement, isNham, timeShowBtClose, timeDelay);
            return re;
        }
#endregion

#region Callbacks from UnityInterstitialAdListener.
        public void onLoaded(int typeAds, string placement, string adId)
        {
            if (AdsFbMyBridge.Instance != null)
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
                    AdsFbMyBridge.Instance.onNtFullLoaded(placement, adId);
                }
                else if (typeAds == 30)
                {
                    AdsFbMyBridge.Instance.onNtIcFullLoaded(placement, adId);
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
        public void onLoadFail(int typeAds, string placement, string adId, string err)
        {
            if (AdsFbMyBridge.Instance != null)
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
                    AdsFbMyBridge.Instance.onNtFullLoadFail(placement, adId, err);
                }
                else if (typeAds == 30)
                {
                    AdsFbMyBridge.Instance.onNtIcFullLoadFail(placement, adId, err);
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
        public void onShowFail(int typeAds, string placement, string adId, string err)
        {
            if (AdsFbMyBridge.Instance != null)
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
                    AdsFbMyBridge.Instance.onNtFullFailedToShow(placement, adId, err);
                }
                else if (typeAds == 30)
                {
                    AdsFbMyBridge.Instance.onNtIcFullFailedToShow(placement, adId, err);
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
        public void onShowed(int typeAds, string placement, string adId)
        {
            if (AdsFbMyBridge.Instance != null)
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
                    AdsFbMyBridge.Instance.onNtFullShowed(placement, adId);
                }
                else if (typeAds == 30)
                {
                    AdsFbMyBridge.Instance.onNtIcFullShowed(placement, adId);
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
        public void onImpresstion(int typeAds, string placement, string adId)
        {
            if (AdsFbMyBridge.Instance != null)
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
                    AdsFbMyBridge.Instance.onNtFullImpresstion(placement, adId);
                }
                else if (typeAds == 30)
                {
                    AdsFbMyBridge.Instance.onNtIcFullImpresstion(placement, adId);
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
        public void onClick(int typeAds, string placement, string adId)
        {
            if (AdsFbMyBridge.Instance != null)
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
                    AdsFbMyBridge.Instance.onNtFullClick(placement, adId);
                }
                else if (typeAds == 30)
                {
                    AdsFbMyBridge.Instance.onNtIcFullClick(placement, adId);
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
        public void onDismissed(int typeAds, string placement, string adId)
        {
            if (AdsFbMyBridge.Instance != null)
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
                    AdsFbMyBridge.Instance.onNtFullDismissed(placement, adId);
                }
                else if (typeAds == 30)
                {
                    AdsFbMyBridge.Instance.onNtIcFullDismissed(placement, adId);
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
        public void onRewarded(int typeAds, string placement, string adId)
        {
            if (AdsFbMyBridge.Instance != null)
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
        public void onPaidEvent(int typeAds, string placement, string adId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (AdsFbMyBridge.Instance != null)
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
                    AdsFbMyBridge.Instance.onNtFullPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
                }
                else if (typeAds == 30)
                {
                    AdsFbMyBridge.Instance.onNtIcFullPaid(placement, adId, adNet, precisionType, currencyCode, valueMicros);
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
        public void onDesTroyAd(int typeAds, string placement, string adId)
        {
            if (AdsFbMyBridge.Instance != null)
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
