#if UNITY_IOS

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace mygame.sdk
{
    public class AdsMaxMyiOS : AdsMaxMyIF
    {
#region IGoogleMobileAdsInterstitialClient implementation
        public void Initialize(string appId)
        {
            AdsMaxMyiOSBridge.Initialize();
        }
        //----------------------------------------------
        public void loadOpenAd(string placement, string adsId)
        {
            AdsMaxMyiOSBridge.loadOpenAd(placement, adsId);
        }
        public bool showOpenAd(string placement, int timeDelay)
        {
            bool re = AdsMaxMyiOSBridge.showOpenAd(placement);
            return re;
        }
        //----------------------------------------------
        public void setCfNtFull(int v1, int v2, int v3, int v4, int v5, int v6)
        {
            AdsMaxMyiOSBridge.setCfNtFull(v1, v2, v3, v4, v5, v6);
        }
        public void loadNativeFull(string placement, string adsId, int orient)
        {
            AdsMaxMyiOSBridge.loadNtFull(placement, adsId, orient);
        }
        public bool showNativeFull(string placement, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick)
        {
            return AdsMaxMyiOSBridge.showNtFull(placement, timeShowBtClose, timeDelay, isAutoCloseWhenClick);
        }
#endregion
    }
}

#endif
