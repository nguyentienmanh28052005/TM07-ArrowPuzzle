#if UNITY_IOS

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace mygame.sdk
{
    public class AdsAdmobMyLowiOS : AdsAdmobMyIF
    {
#region IGoogleMobileAdsInterstitialClient implementation
        public void Initialize()
        {
            AdsAdmobMyLowiOSBridge.Initialize();
        }
        public void targetingAdContent(bool isChild, bool isUnderAgeConsent, int rating)
        {
        }
        public void setLog(bool isLog)
        {
        }
        public void cfBnRefresh(string config)
        {
        }
        public void cfBnAreaTouchex(int w, int h)
        {
        }
        public void trackingTiktok(int flagTrackingTiktok)
        {}
        //----------------------------------------------
        public void loadOpenAd(string placement, string adsId)
        {
            AdsAdmobMyLowiOSBridge.loadOpenAd(placement, adsId);
        }
        public bool showOpenAd(string placement, int timeDelay)
        {
            bool re = AdsAdmobMyLowiOSBridge.showOpenAd(placement);
            return re;
        }
        public bool showBanner(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter)
        {
            return AdsAdmobMyLowiOSBridge.showBanner(placement, pos, width, maxH, orien, iPad, dxCenter);
        }
        public void loadBanner(string placement, string adsId, bool iPad)
        {
            AdsAdmobMyLowiOSBridge.loadBanner(placement, adsId, iPad);
        }
        public void hideBanner()
        {
            AdsAdmobMyLowiOSBridge.hideBanner();
        }
        public void destroyBanner()
        {
            AdsAdmobMyLowiOSBridge.destroyBanner();
        }
        //----------------------------------------------
        public bool showBannerCl(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter)
        {
            return AdsAdmobMyLowiOSBridge.showBannerCl(placement, pos, width, maxH, orien, iPad, dxCenter);
        }
        public void loadBannerCl(string placement, string adsId, bool iPad)
        {
            AdsAdmobMyLowiOSBridge.loadBannerCl(placement, adsId, iPad);
        }
        public void hideBannerCl()
        {
            AdsAdmobMyLowiOSBridge.hideBannerCl();
        }
        public void destroyBannerCl()
        {
            AdsAdmobMyLowiOSBridge.destroyBannerCl();
        }
        //----------------------------------------------
        public bool showBannerRect(string placement, int pos, float width, int maxH, float dxCenter, float dyVertical)
        {
            return AdsAdmobMyLowiOSBridge.showBannerRect(placement, pos, width, maxH, dxCenter, dyVertical);
        }
        public void loadBannerRect(string placement, string adsId)
        {
            AdsAdmobMyLowiOSBridge.loadBannerRect(placement, adsId);
        }
        public void hideBannerRect()
        {
            AdsAdmobMyLowiOSBridge.hideBannerRect();
        }
        public void destroyBannerRect()
        {
            AdsAdmobMyLowiOSBridge.destroyBannerRect();
        }
        //----------------------------------------------
        public bool showBnNt(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter, float dyVertical, int trefresh) { return false; }
        public void loadBnNt(string placement, string adsId, bool iPad) {}
        public void hideBnNt() {}
        public void destroyBnNt() {}
        //----------------------------------------------
        public void loadNativeCl(string placement, string adsId, int index, int orient) {}
        public bool showNativeCl(string placement, int pos, int width, float dxCenter, bool isHideBtClose, bool isLouWhenick) {return false;}
        public void hideNativeCl() {}
        //----------------------------------------------
        public bool showRectNt(string placement, int pos, int orient, float width, float height, float dx, float dy) { return false; }
        public void loadRectNt(string placement, string adsId) {}
        public void hideRectNt() { }
        //----------------------------------------------
        public void setCfNtFull(int v1, int v2, int v3, int v4, int v5, int v6) { }
        public void setCfNtdayClick(string cfdayclick) { }
        public void setCfNtFullFbExcluse(int rows, int columns, string areaExcluse) { }
        public void setCfNtCl(int v1, int v2, int v3, int v4) { }
        public void setCfNtClFbExcluse(int rows, int columns, string areaExcluse, int levelFlick) { }
        public void setTypeBnnt(bool isShowMedia) { }
        //----------------------------------------------
        public void loadNativeFull(string placement, string adsId, int index, int orient)
        {
            AdsAdmobMyLowiOSBridge.loadNtFull(placement, adsId, index, orient);
        }
        public bool showNativeFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick, int layput, bool isAd2)
        {
            return AdsAdmobMyLowiOSBridge.showNtFull(placement, timeShowBtClose, isAutoCloseWhenClick);
        }
        //----------------------------------------------
        public void loadNativeIcFull(string placement, string adsId, int index, int orient)
        {
            AdsAdmobMyLowiOSBridge.loadNtIcFull(placement, adsId, index, orient);
        }
        public bool showNativeIcFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick, int layput, bool isAd2)
        {
            return AdsAdmobMyLowiOSBridge.showNtIcFull(placement, timeShowBtClose, isAutoCloseWhenClick);
        }
        //------------------------------------------------
        public void loadNativePgFull(string placement, int adPos, string adsId, int index)
        {
        }
        public bool showNativePgFull(string placement, bool isNham, int timeShowBtClose, int timeDelay)
        {
            return false;
        }
        //----------------------------------------------
        public void clearCurrFull(string placement)
        {
            AdsAdmobMyLowiOSBridge.clearCurrFull(placement);
        }
        public void loadFull(string placement, string adsId, int index)
        {
            AdsAdmobMyLowiOSBridge.loadFull(placement, adsId, index);
        }
        public bool showFull(string placement, int timeDelay)
        {
            bool re = AdsAdmobMyLowiOSBridge.showFull(placement, timeDelay);
            return re;
        }
        //----------------------------------------------
        public void loadFullRwInter(string placement, string adsId) { }
        public bool showFullRwInter(string placement, int timeDelay) { return false; }

        public void loadFullRwRw(string placement, string adsId) { }
        public bool showFullRwRw(string placement, int timeDelay) { return false; }
        //----------------------------------------------
        public void clearCurrGift(string placement)
        {
            AdsAdmobMyLowiOSBridge.clearCurrGift(placement);
        }
        public void loadGift(string placement, string adsId, int index)
        {
            AdsAdmobMyLowiOSBridge.loadGift(placement, adsId, index);
        }
        public bool showGift(string placement)
        {
            bool re = AdsAdmobMyLowiOSBridge.showGift(placement);
            return re;
        }
        //------------------------------------------------
        public void loadNativeGift(string placement, int adPos, string adsId, int index)
        {
        }
        public bool showNativeGift(string placement, bool isNham, int timeShowBtClose, int timeDelay)
        {
            return false;
        }
#endregion
    }
}

#endif
