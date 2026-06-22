#if UNITY_IOS

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace mygame.sdk
{
	public class AdsAdmobMyiOSBridge
	{
		[DllImport("__Internal")] private static extern void InitializeNative();
		public static void Initialize()
		{
			InitializeNative();
		}
		[DllImport("__Internal")] private static extern void targetingAdContentNative(bool isChild, bool isUnderAgeConsent, int rating);
		public static void targetingAdContent(bool isChild, bool isUnderAgeConsent, int rating)
		{
		    targetingAdContentNative(isChild, isUnderAgeConsent, rating);
		}
		[DllImport("__Internal")] private static extern void setBannerAreaTouchexNative(int w, int h);
		public static void setBannerAreaTouchex(int w, int h)
		{
			setBannerAreaTouchexNative(w, h);
		}
		[DllImport("__Internal")] private static extern void trackingTiktokNative(int flag);
		public static void trackingTiktok(int flag)
		{
			trackingTiktokNative(flag);
		}
		//----------------------------------------------
		[DllImport("__Internal")] private static extern void loadOpenAdNative(string placement, string adsId);
		public static void loadOpenAd(string placement, string adsId)
		{
			loadOpenAdNative(placement, adsId);
		}
		[DllImport("__Internal")] private static extern bool showOpenAdNative(string placement);
		public static bool showOpenAd(string placement)
		{
			return showOpenAdNative(placement);
		}
		//----------------------------------------------
		[DllImport("__Internal")] private static extern bool showBannerNative(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter);
		public static bool showBanner(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter)
		{
			return showBannerNative(placement, pos, width, maxH, orien, iPad, dxCenter);
		}
		[DllImport("__Internal")] private static extern void loadBannerNative(string placement, string adsId, bool iPad);
		public static void loadBanner(string placement, string adsId, bool iPad)
		{
			loadBannerNative(placement, adsId, iPad);
		}
		[DllImport("__Internal")] private static extern void hideBannerNative();
		public static void hideBanner()
		{
			hideBannerNative();
		}
		[DllImport("__Internal")] private static extern void destroyBannerNative();
		public static void destroyBanner()
		{
			destroyBannerNative();
		}
		//----------------------------------------------
		[DllImport("__Internal")] private static extern bool showBannerClNative(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter);
		public static bool showBannerCl(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter)
		{
			return showBannerClNative(placement, pos, width, maxH, orien, iPad, dxCenter);
		}
		[DllImport("__Internal")] private static extern void loadBannerClNative(string placement, string adsId, bool iPad);
		public static void loadBannerCl(string placement, string adsId, bool iPad)
		{
			loadBannerClNative(placement, adsId, iPad);
		}
		[DllImport("__Internal")] private static extern void hideBannerClNative();
		public static void hideBannerCl()
		{
			hideBannerClNative();
		}
		[DllImport("__Internal")] private static extern void destroyBannerClNative();
		public static void destroyBannerCl()
		{
			destroyBannerClNative();
		}
		//----------------------------------------------
		[DllImport("__Internal")] private static extern bool showBannerRectNative(string placement, int pos, float width, int maxH, float dxCenter, float dyVertical);
		public static bool showBannerRect(string placement, int pos, float width, int maxH, float dxCenter, float dyVertical)
		{
			return showBannerRectNative(placement, pos, width, maxH, dxCenter, dyVertical);
		}
		[DllImport("__Internal")] private static extern void loadBannerRectNative(string placement, string adsId);
		public static void loadBannerRect(string placement, string adsId)
		{
			loadBannerRectNative(placement, adsId);
		}
		[DllImport("__Internal")] private static extern void hideBannerRectNative();
		public static void hideBannerRect()
		{
			hideBannerRectNative();
		}
		[DllImport("__Internal")] private static extern void destroyBannerRectNative();
		public static void destroyBannerRect()
		{
			destroyBannerRectNative();
		}
		//----------------------------------------------
		[DllImport("__Internal")] private static extern bool showBnNtNative(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter, float dyVertical, int trefresh);
		public static bool showBnNt(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter, float dyVertical, int trefresh)
		{
			return showBnNtNative(placement, pos, width, maxH, orien, iPad, dxCenter, dyVertical, trefresh);
		}
		[DllImport("__Internal")] private static extern void loadBnNtNative(string placement, string adsId, bool iPad);
		public static void loadBnNt(string placement, string adsId, bool iPad)
		{
			loadBnNtNative(placement, adsId, iPad);
		}
		[DllImport("__Internal")] private static extern void hideBnNtNative();
		public static void hideBnNt()
		{
			hideBnNtNative();
		}
		[DllImport("__Internal")] private static extern void destroyBnNtNative();
		public static void destroyBnNt()
		{
			destroyBnNtNative();
		}
		//----------------------------------------------
		[DllImport("__Internal")] private static extern void loadNativeClNative(string placement, string adsId, int index, int orient);
		public static void loadNativeCl(string placement, string adsId, int index, int orient)
		{
			loadNativeClNative(placement, adsId, index, orient);
		}
		[DllImport("__Internal")] private static extern bool showNativeClNative(string placement, int pos, int width, float dxCenter, bool isHideBtClose, bool isLouWhenick);
		public static bool showNativeCl(string placement, int pos, int width, float dxCenter, bool isHideBtClose, bool isLouWhenick)
		{
			return showNativeClNative(placement, pos, width, dxCenter, isHideBtClose, isLouWhenick); ;
		}
		[DllImport("__Internal")] private static extern void hideNativeClNative();
		public static void hideNativeCl()
		{
			hideNativeClNative();
		}
		//----------------------------------------------
		[DllImport("__Internal")] private static extern bool showRectNtNative(string placement, int pos, int orient, float width, float height, float dx, float dy);
		public static bool showRectNt(string placement, int pos, int orient, float width, float height, float dx, float dy)
		{
			return showRectNtNative(placement, pos, orient, width, height, dx, dy);
		}
		[DllImport("__Internal")] private static extern void loadRectNtNative(string placement, string adsId);
		public static void loadRectNt(string placement, string adsId)
		{
			loadRectNtNative(placement, adsId);
		}
		[DllImport("__Internal")] private static extern void hideRectNtNative();
		public static void hideRectNt()
		{
			hideRectNtNative();
		}
		//----------------------------------------------
		[DllImport("__Internal")] private static extern void setCfNtFullNative(int v1, int v2, int v3, int v4, int v5, bool v6);
		public static void setCfNtFull(int v1, int v2, int v3, int v4, int v5, int v6)
		{
			setCfNtFullNative(v1, v2, v3, v4, v5, v6 > 0);
		}
		[DllImport("__Internal")] private static extern void setCfNtFullFbExcluseNative(int rows, int columns, string areaExcluse);
		public static void setCfNtFullFbExcluse(int rows, int columns, string areaExcluse)
		{
			setCfNtFullFbExcluseNative(rows, columns, areaExcluse);
		}
		[DllImport("__Internal")] private static extern void setCfNtClNative(int v1, int v2, int v3, int v4);
		public static void setCfNtCl(int v1, int v2, int v3, int v4)
		{
			setCfNtClNative(v1, v2, v3, v4);
		}
		[DllImport("__Internal")] private static extern void setCfNtClFbExcluseNative(int rows, int columns, string areaExcluse, int levelFlick);
		public static void setCfNtClFbExcluse(int rows, int columns, string areaExcluse, int levelFlick)
		{
			setCfNtClFbExcluseNative(rows, columns, areaExcluse, levelFlick);
		}
		[DllImport("__Internal")] private static extern void setTypeBnntNative(bool isShowMedia);
		public static void setTypeBnnt(bool isShowMedia)
        {
			setTypeBnntNative(isShowMedia);
		}
		//----------------------------------------------
		[DllImport("__Internal")] private static extern void loadNtFullNative(string placement, string adsId, int index, int orient);
		public static void loadNtFull(string placement, string adsId, int index, int orient)
		{
			loadNtFullNative(placement, adsId, index, orient);
		}
		[DllImport("__Internal")] private static extern bool showNtFullNative(string placement, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick, int layout, bool isAd2);
		public static bool showNtFull(string placement, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick, int layout, bool isAd2)
		{
			return showNtFullNative(placement, timeShowBtClose, timeDelay, isAutoCloseWhenClick, layout, isAd2);
		}
		[DllImport("__Internal")] private static extern bool reCountCurrShowNative();
		public static bool reCountCurrShow()
		{
			return reCountCurrShowNative();
		}
		//----------------------------------------------
		[DllImport("__Internal")] private static extern void loadNtIcFullNative(string placement, string adsId, int index, int orient);
		public static void loadNtIcFull(string placement, string adsId, int index, int orient)
		{
			loadNtIcFullNative(placement, adsId, index, orient);
		}
		[DllImport("__Internal")] private static extern bool showNtIcFullNative(string placement, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick, int layout, bool isAd2);
		public static bool showNtIcFull(string placement, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick, int layout, bool isAd2)
		{
			return showNtIcFullNative(placement, timeShowBtClose, timeDelay, isAutoCloseWhenClick, layout, isAd2);
		}
		//----------------------------------------------
		[DllImport("__Internal")] private static extern void clearCurrFullNative(string placement);
		public static void clearCurrFull(string placement)
		{
			clearCurrFullNative(placement);
		}
		[DllImport("__Internal")] private static extern void loadFullNative(string placement, string adsId, int index);
		public static void loadFull(string placement, string adsId, int index)
		{
			loadFullNative(placement, adsId, index);
		}
		[DllImport("__Internal")] private static extern bool showFullNative(string placement);
		public static bool showFull(string placement)
		{
			return showFullNative(placement);
		}
		//----------------------------------------------
		[DllImport("__Internal")] private static extern void clearCurrGiftNative(string placement);
		public static void clearCurrGift(string placement)
		{
			clearCurrGiftNative(placement);
		}
		[DllImport("__Internal")] private static extern void loadGiftNative(string placement, string adsId, int index);
		public static void loadGift(string placement, string adsId, int index)
		{
			loadGiftNative(placement, adsId, index);
		}
		[DllImport("__Internal")] private static extern bool showGiftNative(string placement);
		public static bool showGift(string placement)
		{
			return showGiftNative(placement);
		}
	}
}

#endif
