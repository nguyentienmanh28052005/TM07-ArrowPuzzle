#if UNITY_IOS

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace mygame.sdk
{
    public class AdsMaxMyiOSBridge
    {
		[DllImport ("__Internal")] private static extern void MaxInitializeNative();
		public static void Initialize()
		{
			MaxInitializeNative();
		}
		//----------------------------------------------
		[DllImport("__Internal")] private static extern void maxLoadOpenAdNative(string placement, string adsId);
		public static void loadOpenAd(string placement, string adsId)
		{
			maxLoadOpenAdNative(placement, adsId);
		}
		[DllImport("__Internal")] private static extern bool maxShowOpenAdNative(string placement);
		public static bool showOpenAd(string placement)
		{
			return maxShowOpenAdNative(placement);
		}
		//----------------------------------------------
		[DllImport ("__Internal")] private static extern void maxSetCfNtFullNative(int v1, int v2, int v3, int v4, int v5, bool v6);
		public static void setCfNtFull(int v1, int v2, int v3, int v4, int v5, int v6)
		{
			maxSetCfNtFullNative(v1, v2, v3, v4, v5, v6 > 0);
		}
		[DllImport ("__Internal")] private static extern void maxLoadNtFullNative(string placement, string adsId, int orient);
		public static void loadNtFull(string placement, string adsId, int orient)
		{
			maxLoadNtFullNative(placement, adsId, orient);
		}
		[DllImport ("__Internal")] private static extern bool maxShowNtFullNative(string placement, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick);
		public static bool showNtFull(string placement, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick)
		{
			return maxShowNtFullNative(placement, timeShowBtClose, timeDelay, isAutoCloseWhenClick);
		}
		[DllImport("__Internal")] private static extern bool maxReCountCurrShowNative();
		public static bool reCountCurrShow()
		{
			return maxReCountCurrShowNative();
		}
    }
}

#endif
