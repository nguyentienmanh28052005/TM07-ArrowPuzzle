#if UNITY_IOS

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace mygame.sdk
{
    public class AdsMyTargetiOS : AdsMyTargetIF
    {
#region IGoogleMobileAdsInterstitialClient implementation
        public void Initialize()
        {
            MyTargetiOSBridge.Initialize();
        }
        public void setTestMode(bool isTestMode)
        {
            MyTargetiOSBridge.setTestMode(isTestMode);
        }
        public void addTestDevice(string deviceId)
        {
            MyTargetiOSBridge.addTestDevice(deviceId);
        }
        public void setBannerPos(int pos, int width, float dxCenter)
        {
            MyTargetiOSBridge.setBannerPos(pos, width, dxCenter);
        }
        public void showBanner(string adsId, int pos, int width, int orien, bool iPad, float dxCenter)
        {
            MyTargetiOSBridge.showBanner(adsId, pos, width, orien, iPad, dxCenter);
        }
        public void hideBanner()
        {
            MyTargetiOSBridge.hideBanner();
        }

        public void clearCurrFull()
        {
            MyTargetiOSBridge.clearCurrFull();
        }
        public void loadFull(string adsId)
        {
            MyTargetiOSBridge.loadFull(adsId);
        }
        public bool showFull()
        {
            bool re = MyTargetiOSBridge.showFull();
            return re;
        }

        public void clearCurrGift()
        {
            MyTargetiOSBridge.clearCurrGift();
        }
        public void loadGift(string adsId)
        {
            MyTargetiOSBridge.loadGift(adsId);
        }
        public bool showGift()
        {
            bool re = MyTargetiOSBridge.showGift();
            return re;
        }

#endregion
    }
}

#endif
