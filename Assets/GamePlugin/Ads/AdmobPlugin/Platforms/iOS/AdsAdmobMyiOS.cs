#if UNITY_IOS

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace mygame.sdk
{
    public class AdsAdmobMyiOS : AdsAdmobMyIF
    {
#region IGoogleMobileAdsInterstitialClient implementation
        public void Initialize()
        {
            AdsAdmobMyiOSBridge.Initialize();
        }
        public void targetingAdContent(bool isChild, bool isUnderAgeConsent, int rating)
        {
            AdsAdmobMyiOSBridge.targetingAdContent(isChild, isUnderAgeConsent, rating);
        }
        public void setLog(bool isLog)
        {
        }
        public void cfBnRefresh(string config)
        {
        }
        public void cfBnAreaTouchex(int w, int h)
        {
            AdsAdmobMyiOSBridge.setBannerAreaTouchex(w, h);
        }
        public void trackingTiktok(int flagTrackingTiktok)
        {
            AdsAdmobMyiOSBridge.trackingTiktok(flagTrackingTiktok);
        }
        //----------------------------------------------
        public void loadOpenAd(string placement, string adsId)
        {
            AdsAdmobMyiOSBridge.loadOpenAd(placement, adsId);
        }
        public bool showOpenAd(string placement, int timeDelay)
        {
            bool re = AdsAdmobMyiOSBridge.showOpenAd(placement);
            return re;
        }
        //
        public bool showBanner(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter)
        {
            return AdsAdmobMyiOSBridge.showBanner(placement, pos, width, maxH, orien, iPad, dxCenter);
        }
        public void loadBanner(string placement, string adsId, bool iPad)
        {
            AdsAdmobMyiOSBridge.loadBanner(placement, adsId, iPad);
        }
        public void hideBanner()
        {
            AdsAdmobMyiOSBridge.hideBanner();
        }
        public void destroyBanner()
        {
            AdsAdmobMyiOSBridge.destroyBanner();
        }
        //----------------------------------------------
        public bool showBannerCl(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter)
        {
            return AdsAdmobMyiOSBridge.showBannerCl(placement, pos, width, maxH, orien, iPad, dxCenter);
        }
        public void loadBannerCl(string placement, string adsId, bool iPad)
        {
            AdsAdmobMyiOSBridge.loadBannerCl(placement, adsId, iPad);
        }
        public void hideBannerCl()
        {
            AdsAdmobMyiOSBridge.hideBannerCl();
        }
        public void destroyBannerCl()
        {
            AdsAdmobMyiOSBridge.destroyBannerCl();
        }
        //----------------------------------------------
        public bool showBannerRect(string placement, int pos, float width, int maxH, float dxCenter, float dyVertical)
        {
            return AdsAdmobMyiOSBridge.showBannerRect(placement, pos, width, maxH, dxCenter, dyVertical);
        }
        public void loadBannerRect(string placement, string adsId)
        {
            AdsAdmobMyiOSBridge.loadBannerRect(placement, adsId);
        }
        public void hideBannerRect()
        {
            AdsAdmobMyiOSBridge.hideBannerRect();
        }
        public void destroyBannerRect()
        {
            AdsAdmobMyiOSBridge.destroyBannerRect();
        }
        //----------------------------------------------
        public bool showBnNt(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter, float dyVertical, int trefresh)
        {
            return AdsAdmobMyiOSBridge.showBnNt(placement, pos, width, maxH, orien, iPad, dxCenter, dyVertical, trefresh); ;
        }
        public void loadBnNt(string placement, string adsId, bool iPad)
        {
            AdsAdmobMyiOSBridge.loadBnNt(placement, adsId, iPad);
        }
        public void hideBnNt()
        {
            AdsAdmobMyiOSBridge.hideBnNt();
        }
        public void destroyBnNt()
        {
            AdsAdmobMyiOSBridge.destroyBnNt();
        }
        //----------------------------------------------
        public void loadNativeCl(string placement, string adsId, int index, int orient)
        {
            AdsAdmobMyiOSBridge.loadNativeCl(placement, adsId, index, orient);
        }
        public bool showNativeCl(string placement, int pos, int width, float dxCenter, bool isHideBtClose, bool isLouWhenick)
        {
            return AdsAdmobMyiOSBridge.showNativeCl(placement, pos, width, dxCenter, isHideBtClose, isLouWhenick); ;
        }
        public void hideNativeCl()
        {
            AdsAdmobMyiOSBridge.hideNativeCl();
        }
        //----------------------------------------------
        public bool showRectNt(string placement, int pos, int orient, float width, float height, float dx, float dy)
        {
            return AdsAdmobMyiOSBridge.showRectNt(placement, pos, orient, width, height, dx, dy);
        }
        public void loadRectNt(string placement, string adsId)
        {
            AdsAdmobMyiOSBridge.loadRectNt(placement, adsId);
        }
        public void hideRectNt()
        {
            AdsAdmobMyiOSBridge.hideRectNt();
        }
        //----------------------------------------------
        public void setCfNtFull(int v1, int v2, int v3, int v4, int v5, int v6)
        {
            AdsAdmobMyiOSBridge.setCfNtFull(v1, v2, v3, v4, v5, v6);
        }
        public void setCfNtdayClick(string cfdayclick)
        {

        }
        public void setCfNtFullFbExcluse(int rows, int columns, string areaExcluse)
        {
            AdsAdmobMyiOSBridge.setCfNtFullFbExcluse(rows, columns, areaExcluse);
        }
        public void setCfNtCl(int v1, int v2, int v3, int v4)
        {
            AdsAdmobMyiOSBridge.setCfNtCl(v1, v2, v3, v4);
        }
        public void setCfNtClFbExcluse(int rows, int columns, string areaExcluse, int levelFlick)
        {
            AdsAdmobMyiOSBridge.setCfNtClFbExcluse(rows, columns, areaExcluse, levelFlick);
        }
        public void setTypeBnnt(bool isShowMedia)
        {
            AdsAdmobMyiOSBridge.setTypeBnnt(isShowMedia);
        }
        //----------------------------------------------
        public void loadNativeFull(string placement, string adsId, int index, int orient)
        {
            AdsAdmobMyiOSBridge.loadNtFull(placement, adsId, index, orient);
        }
        public bool showNativeFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick, int layput, bool isAd2)
        {
            return AdsAdmobMyiOSBridge.showNtFull(placement, timeShowBtClose, timeDelay, isAutoCloseWhenClick, layput, isAd2);
        }
        //----------------------------------------------
        public void loadNativeIcFull(string placement, string adsId, int index, int orient)
        {
            AdsAdmobMyiOSBridge.loadNtIcFull(placement, adsId, index, orient);
        }
        public bool showNativeIcFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick, int layput, bool isAd2)
        {
            return AdsAdmobMyiOSBridge.showNtIcFull(placement, timeShowBtClose, timeDelay, isAutoCloseWhenClick, layput, isAd2);
        }
        //----------------------------------------------
        public void loadNativePgFull(string placement, int adPos, string adsId, int index)
        {
            if (AdsAdmobMyBridge.Instance != null)
            {
                AdsAdmobMyBridge.Instance.iOSCBNtPgFullLoadFail($"{placement};{adsId};notsuport");
            }
        }
        public bool showNativePgFull(string placement, bool isNham, int timeShowBtClose, int timeDelay)
        {
            return false;
        }
        //----------------------------------------------
        public void clearCurrFull(string placement)
        {
            AdsAdmobMyiOSBridge.clearCurrFull(placement);
        }
        public void loadFull(string placement, string adsId, int index)
        {
            AdsAdmobMyiOSBridge.loadFull(placement, adsId, index);
        }
        public bool showFull(string placement, int timeDelay)
        {
            bool re = AdsAdmobMyiOSBridge.showFull(placement);
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
            AdsAdmobMyiOSBridge.clearCurrGift(placement);
        }
        public void loadGift(string placement, string adsId, int index)
        {
            AdsAdmobMyiOSBridge.loadGift(placement, adsId, index);
        }
        public bool showGift(string placement)
        {
            bool re = AdsAdmobMyiOSBridge.showGift(placement);
            return re;
        }
        //------------------------------------------------
        public void loadNativeGift(string placement, int adPos, string adsId, int index)
        {
            if (AdsAdmobMyBridge.Instance != null)
            {
                AdsAdmobMyBridge.Instance.iOSCBNtGiftLoadFail($"{placement};{adsId};notsuport");
            }
        }
        public bool showNativeGift(string placement, bool isNham, int timeShowBtClose, int timeDelay)
        {
            return false;
        }

#endregion
    }
}

#endif
