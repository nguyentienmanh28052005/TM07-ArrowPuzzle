#if UNITY_IOS

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace mygame.sdk
{
    public class MyTargetiOSBridge
    {
		[DllImport ("__Internal")] private static extern void myTargetInitializeNative();
		public static void Initialize()
		{
			myTargetInitializeNative();
		}
        [DllImport("__Internal")] private static extern void myTargetSetTestModeNative(bool isTestMode);
        public static void setTestMode(bool isTestMode)
        {
            myTargetSetTestModeNative(isTestMode);
        }
        [DllImport("__Internal")] private static extern void myTargetAddTestDeviceNative(string deviceId);
		public static void addTestDevice(string deviceId)
		{
			myTargetAddTestDeviceNative(deviceId);
		}
		[DllImport ("__Internal")] private static extern void myTargetSetBannerPosNative(int pos, int width, float dxCenter);
		public static void setBannerPos(int pos, int width, float dxCenter)
		{
			myTargetSetBannerPosNative(pos, width, dxCenter);
		}
		[DllImport ("__Internal")] private static extern void myTargetShowBannerNative(string adsId, int pos, int width, int orien, bool iPad, float dxCenter);
		public static void showBanner(string adsId, int pos, int width, int orien, bool iPad, float dxCenter)
		{
			myTargetShowBannerNative(adsId, pos, width, orien, iPad, dxCenter);
		}

		[DllImport ("__Internal")] private static extern void myTargetHideBannerNative();
		public static void hideBanner()
		{
			myTargetHideBannerNative();
		}

		[DllImport("__Internal")] private static extern void myTargetClearCurrFullNative();
		public static void clearCurrFull()
        {
			myTargetClearCurrFullNative();
		}

		[DllImport ("__Internal")] private static extern void myTargetLoadFullNative(string adsId);
		public static void loadFull(string adsId)
		{
			myTargetLoadFullNative(adsId);
		}

		[DllImport("__Internal")] private static extern bool myTargetShowFullNative();
		public static bool showFull()
		{
			return myTargetShowFullNative();
		}

		[DllImport("__Internal")] private static extern void myTargetClearCurrGiftNative();
		public static void clearCurrGift()
		{
			myTargetClearCurrGiftNative();
		}

		[DllImport("__Internal")] private static extern void myTargetLoadGiftNative(string adsId);
		public static void loadGift(string adsId)
		{
			myTargetLoadGiftNative(adsId);
		}

		[DllImport("__Internal")] private static extern bool myTargetShowGiftNative();
		public static bool showGift()
		{
			return myTargetShowGiftNative();
		}
    }
}

#endif