#if UNITY_IOS

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace mygame.sdk
{
    public class FbMyiOS : AdsFbMyIF
    {
#region IFbAdsInterstitialClient implementation
        public void Initialize()
        {
            FbMyiOSBridge.Initialize();
        }
        public void setLog(bool isLog)
        {

        }
        //----------------------------------------------
        public void setCfNtFull(int v1, int v2, int v3, int v4, int v5, int v6)
        {
            FbMyiOSBridge.setCfNtFull(v1, v2, v3, v4, v5, v6);
        }
        public void setCfNtFullFbExcluse(int rows, int columns, string areaExcluse)
        {
            FbMyiOSBridge.setCfNtFullFbExcluse(rows, columns, areaExcluse);
        }
        //----------------------------------------------
        public void loadNativeFull(string placement, string adsId, int orient)
        {
            FbMyiOSBridge.loadNtFull(placement, adsId, orient);
        }
        public bool showNativeFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick)
        {
            return FbMyiOSBridge.showNtFull(placement, timeShowBtClose, timeDelay, isAutoCloseWhenClick);
        }
        //----------------------------------------------
        public void loadNativeIcFull(string placement, string adsId, int orient)
        {
            FbMyiOSBridge.loadNtIcFull(placement, adsId, orient);
        }
        public bool showNativeIcFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick)
        {
            return FbMyiOSBridge.showNtIcFull(placement, timeShowBtClose, timeDelay, isAutoCloseWhenClick);
        }

#endregion
    }
}

#endif
