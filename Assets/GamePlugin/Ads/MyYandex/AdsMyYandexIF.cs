//#define TEST_ADS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace mygame.sdk
{
    public interface AdsMyYandexIF
    {
        #region IAdsYandexIFClient implementation
        void Initialize();
        void addTestDevice(string deviceId);
        void setTestMode(bool isTestMode);
        void setBannerPos(int pos, int width, float dxCenter);
        void showBanner(string adsId, int pos, int width, int orien, bool iPad, float dxCenter);
        void hideBanner();

        void clearCurrFull();
        void loadFull(string adsId);
        bool showFull();

        void clearCurrGift();
        void loadGift(string adsId);
        bool showGift();

        #endregion
    }
}
