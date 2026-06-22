#if UNITY_IOS

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace mygame.sdk
{
    public class AdsAdmobMyLowiOSBridge
    {
		[DllImport ("__Internal")] private static extern void LowInitializeNative();
		public static void Initialize()
		{
			LowInitializeNative();
		}
		//----------------------------------------------
		[DllImport("__Internal")] private static extern void LowLoadOpenAdNative(string placement, string adsId);
		public static void loadOpenAd(string placement, string adsId)
		{
			LowLoadOpenAdNative(placement, adsId);
		}
		[DllImport("__Internal")] private static extern bool LowShowOpenAdNative(string placement);
		public static bool showOpenAd(string placement)
		{
			return LowShowOpenAdNative(placement);
		}
		//----------------------------------------------
		[DllImport ("__Internal")] private static extern bool LowShowBannerNative(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter);
		public static bool showBanner(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter)
		{
			return LowShowBannerNative(placement, pos, width, maxH, orien, iPad, dxCenter);
		}
		[DllImport ("__Internal")] private static extern void LowLoadBannerNative(string placement, string adsId, bool iPad);
		public static void loadBanner(string placement, string adsId, bool iPad)
		{
			LowLoadBannerNative(placement, adsId, iPad);
		}
		[DllImport ("__Internal")] private static extern void LowHideBannerNative();
		public static void hideBanner()
		{
			LowHideBannerNative();
		}
		[DllImport ("__Internal")] private static extern void LowDestroyBannerNative();
		public static void destroyBanner()
		{
			LowDestroyBannerNative();
		}
		//----------------------------------------------
		[DllImport ("__Internal")] private static extern bool LowShowBannerClNative(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter);
		public static bool showBannerCl(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter)
		{
			return LowShowBannerClNative(placement, pos, width, maxH, orien, iPad, dxCenter);
		}
		[DllImport ("__Internal")] private static extern void LowLoadBannerClNative(string placement, string adsId, bool iPad);
		public static void loadBannerCl(string placement, string adsId, bool iPad)
		{
			LowLoadBannerClNative(placement, adsId, iPad);
		}
		[DllImport ("__Internal")] private static extern void LowHideBannerClNative();
		public static void hideBannerCl()
		{
			LowHideBannerClNative();
		}
		[DllImport ("__Internal")] private static extern void LowDestroyBannerClNative();
		public static void destroyBannerCl()
		{
			LowDestroyBannerClNative();
		}
		//----------------------------------------------
		[DllImport ("__Internal")] private static extern bool LowShowBannerRectNative(string placement, int pos, float width, int maxH, float dxCenter, float dyVertical);
		public static bool showBannerRect(string placement, int pos, float width, int maxH, float dxCenter, float dyVertical)
		{
			return LowShowBannerRectNative(placement, pos, width, maxH, dxCenter, dyVertical);
		}
		[DllImport ("__Internal")] private static extern void LowLoadBannerRectNative(string placement, string adsId);
		public static void loadBannerRect(string placement, string adsId)
		{
			LowLoadBannerRectNative(placement, adsId);
		}
		[DllImport ("__Internal")] private static extern void LowHideBannerRectNative();
		public static void hideBannerRect()
		{
			LowHideBannerRectNative();
		}
		[DllImport ("__Internal")] private static extern void LowDestroyBannerRectNative();
		public static void destroyBannerRect()
		{
			LowDestroyBannerRectNative();
		}
		//-----------------------------------------------
		[DllImport ("__Internal")] private static extern void LowLoadNtFullNative(string placement, string adsId, int index, int orient);
		public static void loadNtFull(string placement, string adsId, int index, int orient)
		{
			LowLoadNtFullNative(placement, adsId, index, orient);
		}
		[DllImport ("__Internal")] private static extern bool LowShowNtFullNative(string placement, int timeShowBtClose, bool isAutoCloseWhenClick);
		public static bool showNtFull(string placement, int timeShowBtClose, bool isAutoCloseWhenClick)
		{
			return LowShowNtFullNative(placement, timeShowBtClose, isAutoCloseWhenClick);
		}
		//-----------------------------------------------
		[DllImport ("__Internal")] private static extern void LowLoadNtIcFullNative(string placement, string adsId, int index, int orient);
		public static void loadNtIcFull(string placement, string adsId, int index, int orient)
		{
			LowLoadNtIcFullNative(placement, adsId, index, orient);
		}
		[DllImport ("__Internal")] private static extern bool LowShowNtIcFullNative(string placement, int timeShowBtClose, bool isAutoCloseWhenClick);
		public static bool showNtIcFull(string placement, int timeShowBtClose, bool isAutoCloseWhenClick)
		{
			return LowShowNtIcFullNative(placement, timeShowBtClose, isAutoCloseWhenClick);
		}
		//----------------------------------------------
		[DllImport ("__Internal")] private static extern void LowClearCurrFullNative(string placement);
		public static void clearCurrFull(string placement)
		{
			LowClearCurrFullNative(placement);
		}
		[DllImport ("__Internal")] private static extern void LowLoadFullNative(string placement, string adsId, int index);
		public static void loadFull(string placement, string adsId, int index)
		{
			LowLoadFullNative(placement, adsId, index);
		}
		[DllImport ("__Internal")] private static extern bool LowShowFullNative(string placement, int timeDelay);
		public static bool showFull(string placement, int timeDelay)
		{
			return LowShowFullNative(placement, timeDelay);
		}
		//----------------------------------------------
		[DllImport ("__Internal")] private static extern void LowClearCurrGiftNative(string placement);
		public static void clearCurrGift(string placement)
		{
			LowClearCurrGiftNative(placement);
		}
		[DllImport ("__Internal")] private static extern void LowLoadGiftNative(string placement, string adsId, int index);
		public static void loadGift(string placement, string adsId, int index)
		{
			LowLoadGiftNative(placement, adsId, index);
		}
		[DllImport ("__Internal")] private static extern bool LowShowGiftNative(string placement);
		public static bool showGift(string placement)
		{
			return LowShowGiftNative(placement);
		}
	}
}

#endif
