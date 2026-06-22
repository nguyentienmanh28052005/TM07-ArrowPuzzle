//#define TEST_ADS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace mygame.sdk
{
    public interface AdsMaxMyIF
    {
        #region Max implementation
        void Initialize(string appId);

        void loadOpenAd(string placement, string adsId);
        bool showOpenAd(string placement, int timeDelay);

        void setCfNtFull(int v1, int v2, int v3, int v4, int v5, int v6);
        void loadNativeFull(string placement, string adsId, int orient);
        bool showNativeFull(string placement, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick);

        #endregion
    }
}
