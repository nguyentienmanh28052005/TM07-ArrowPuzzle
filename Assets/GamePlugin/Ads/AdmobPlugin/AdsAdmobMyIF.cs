//#define TEST_ADS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace mygame.sdk
{
    public interface AdsAdmobMyIF
    {
        #region IGoogleMobileAdsInterstitialClient implementation
        void Initialize();
        void targetingAdContent(bool isChild, bool isUnderAgeConsent, int rating);
        void setLog(bool isLog);
        void cfBnRefresh(string config);
        void cfBnAreaTouchex(int w, int h);
        void trackingTiktok(int flagTrackingTiktok);

        void loadOpenAd(string placement, string adsId);
        bool showOpenAd(string placement, int timeDelay);

        bool showBanner(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter);
        void loadBanner(string placement, string adsId, bool iPad);
        void hideBanner();
        void destroyBanner();

        bool showBannerCl(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter);
        void loadBannerCl(string placement, string adsId, bool iPad);
        void hideBannerCl();
        void destroyBannerCl();

        bool showBannerRect(string placement, int pos, float width, int maxH, float dxCenter, float dyVertical);
        void loadBannerRect(string placement, string adsId);
        void hideBannerRect();
        void destroyBannerRect();

        bool showBnNt(string placement, int pos, int width, int maxH, int orien, bool iPad, float dxCenter, float dyVertical, int trefresh);
        void loadBnNt(string placement, string adsId, bool iPad);
        void hideBnNt();
        void destroyBnNt();

        void loadNativeCl(string placement, string adsId, int index, int orient);
        bool showNativeCl(string placement, int pos, int width, float dxCenter, bool isHideBtClose, bool isLouWhenick);
        void hideNativeCl();

        bool showRectNt(String placement, int pos, int orient, float width, float height, float dx, float dy);
        void loadRectNt(String placement, String adsId);
        void hideRectNt();

        void setCfNtFull(int v1, int v2, int v3, int v4, int v5, int v6);
        void setCfNtdayClick(string cfdayclick);
        void setCfNtFullFbExcluse(int rows, int columns, string areaExcluse);
        void setCfNtCl(int v1, int v2, int v3, int v4);
        void setCfNtClFbExcluse(int rows, int columns, string areaExcluse, int levelFlick);
        void setTypeBnnt(bool isShowMedia);

        void loadNativeFull(string placement, string adsId, int index, int orient);
        bool showNativeFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick, int layout, bool isAd2);

        void loadNativeIcFull(string placement, string adsId, int index, int orient);
        bool showNativeIcFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick, int layout, bool isAd2);

        void loadNativePgFull(string placement, int adPos, string adsId, int index);
        bool showNativePgFull(string placement, bool isNham, int timeShowBtClose, int timeDelay);

        void clearCurrFull(string placement);
        void loadFull(string placement, string adsId, int index);
        bool showFull(string placement, int timeDelay);

        void loadFullRwInter(string placement, string adsId);
        bool showFullRwInter(string placement, int timeDelay);

        void loadFullRwRw(string placement, string adsId);
        bool showFullRwRw(string placement, int timeDelay);

        void clearCurrGift(string placement);
        void loadGift(string placement, string adsId, int index);
        bool showGift(string placement);

        void loadNativeGift(string placement, int adPos, string adsId, int index);
        bool showNativeGift(string placement, bool isNham, int timeShowBtClose, int timeDelay);

        #endregion
    }
}
