//#define TEST_ADS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace mygame.sdk
{
    public interface AdsFbMyIF
    {
        #region IGoogleMobileAdsInterstitialClient implementation
        void Initialize();
        void setLog(bool isLog);

        void setCfNtFull(int v1, int v2, int v3, int v4, int v5, int v6);
        void setCfNtFullFbExcluse(int rows, int columns, string areaExcluse);

        void loadNativeFull(string placement, string adsId, int orient);
        bool showNativeFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick);

        void loadNativeIcFull(string placement, string adsId, int orient);
        bool showNativeIcFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick);
        #endregion
    }
}
