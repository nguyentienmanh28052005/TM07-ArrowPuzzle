#if UNITY_IOS

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace mygame.sdk
{
    public class MyYandexiOSBridge
    {
		[DllImport ("__Internal")] private static extern void myYandexInitializeNative();
		public static void Initialize()
		{
			myYandexInitializeNative();
		}
        [DllImport("__Internal")] private static extern void myYandexSetTestModeNative(bool isTestMode);
        public static void setTestMode(bool isTestMode)
        {
            myYandexSetTestModeNative(isTestMode);
        }
        [DllImport("__Internal")] private static extern void myYandexAddTestDeviceNative(string deviceId);
		public static void addTestDevice(string deviceId)
		{
			myYandexAddTestDeviceNative(deviceId);
		}
		[DllImport ("__Internal")] private static extern void myYandexSetBannerPosNative(int pos, int width, float dxCenter);
		public static void setBannerPos(int pos, int width, float dxCenter)
		{
			myYandexSetBannerPosNative(pos, width, dxCenter);
		}
		[DllImport ("__Internal")] private static extern void myYandexShowBannerNative(string adsId, int pos, int width, int orien, bool iPad, float dxCenter);
		public static void showBanner(string adsId, int pos, int width, int orien, bool iPad, float dxCenter)
		{
			myYandexShowBannerNative(adsId, pos, width, orien, iPad, dxCenter);
		}

		[DllImport ("__Internal")] private static extern void myYandexHideBannerNative();
		public static void hideBanner()
		{
			myYandexHideBannerNative();
		}

		[DllImport("__Internal")] private static extern void myYandexClearCurrFullNative();
		public static void clearCurrFull()
        {
			myYandexClearCurrFullNative();
		}

		[DllImport ("__Internal")] private static extern void myYandexLoadFullNative(string adsId);
		public static void loadFull(string adsId)
		{
			myYandexLoadFullNative(adsId);
		}

		[DllImport("__Internal")] private static extern bool myYandexShowFullNative();
		public static bool showFull()
		{
			return myYandexShowFullNative();
		}

		[DllImport("__Internal")] private static extern void myYandexClearCurrGiftNative();
		public static void clearCurrGift()
		{
			myYandexClearCurrGiftNative();
		}

		[DllImport("__Internal")] private static extern void myYandexLoadGiftNative(string adsId);
		public static void loadGift(string adsId)
		{
			myYandexLoadGiftNative(adsId);
		}

		[DllImport("__Internal")] private static extern bool myYandexShowGiftNative();
		public static bool showGift()
		{
			return myYandexShowGiftNative();
		}
    }
}

#endif