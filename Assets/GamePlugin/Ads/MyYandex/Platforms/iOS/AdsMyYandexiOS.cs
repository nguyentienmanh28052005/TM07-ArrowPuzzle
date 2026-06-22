#if UNITY_IOS

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace mygame.sdk
{
    public class AdsMyYandexiOS : AdsMyYandexIF
    {
#region IGoogleMobileAdsInterstitialClient implementation
        public void Initialize()
        {
            MyYandexiOSBridge.Initialize();
        }
        public void setTestMode(bool isTestMode)
        {
            MyYandexiOSBridge.setTestMode(isTestMode);
        }
        public void addTestDevice(string deviceId)
        {
            MyYandexiOSBridge.addTestDevice(deviceId);
        }
        public void setBannerPos(int pos, int width, float dxCenter)
        {
            MyYandexiOSBridge.setBannerPos(pos, width, dxCenter);
        }
        public void showBanner(string adsId, int pos, int width, int orien, bool iPad, float dxCenter)
        {
            MyYandexiOSBridge.showBanner(adsId, pos, width, orien, iPad, dxCenter);
        }
        public void hideBanner()
        {
            MyYandexiOSBridge.hideBanner();
        }

        public void clearCurrFull()
        {
            MyYandexiOSBridge.clearCurrFull();
        }
        public void loadFull(string adsId)
        {
            MyYandexiOSBridge.loadFull(adsId);
        }
        public bool showFull()
        {
            bool re = MyYandexiOSBridge.showFull();
            return re;
        }

        public void clearCurrGift()
        {
            MyYandexiOSBridge.clearCurrGift();
        }
        public void loadGift(string adsId)
        {
            MyYandexiOSBridge.loadGift(adsId);
        }
        public bool showGift()
        {
            bool re = MyYandexiOSBridge.showGift();
            return re;
        }

#endregion
    }
}

#endif
