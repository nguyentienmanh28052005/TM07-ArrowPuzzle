using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_ANDROID
using mygame.plugin.Android;
#endif
namespace mygame.sdk
{
    public class AdsMyYandexBridge : MonoBehaviour
    {
        public static event Action onBNLoaded;
        public static event Action<string> onBNLoadFail;
        public static event Action onBNClose;
        public static event Action onBNOpen;
        public static event Action onBNClick;
        public static event Action onBNImpression;
        public static event Action<int, string, long> onBNPaid;

        public static event Action onInterstitialLoaded;
        public static event Action<string> onInterstitialLoadFail;
        public static event Action<string> onInterstitialFailedToShow;
        public static event Action onInterstitialShowed;
        public static event Action onInterstitialClick;
        public static event Action onInterstitialDismissed;
        public static event Action onInterstitialImpresstion;
        public static event Action<int, string, long> onInterstitialPaid;

        public static event Action onRewardLoaded;
        public static event Action<string> onRewardLoadFail;
        public static event Action<string> onRewardFailedToShow;
        public static event Action onRewardShowed;
        public static event Action onRewardClick;
        public static event Action onRewardDismissed;
        public static event Action onRewardImpresstion;
        public static event Action onRewardReward;
        public static event Action<int, string, long> onRewardPaid;

        private AdsMyYandexIF adsMyYandexIF;

        public static AdsMyYandexBridge Instance { get; private set; }

#if UNITY_ANDROID
#endif

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                gameObject.name = "AdsMyYandexBridge";
#if ENABLE_ADS_YANDEX && !UNITY_EDITOR
#if UNITY_ANDROID
                adsMyYandexIF = new AdsMyYandexAndroid();
#else
                adsMyYandexIF = new AdsMyYandexiOS();
#endif
#endif
            }
            else
            {
                //if (this != Instance) Destroy(gameObject);
            }
        }


#region IAdsMyYandexIFClient implementation
        public void Initialize()
        {
#if ENABLE_ADS_YANDEX && !UNITY_EDITOR
            adsMyYandexIF.Initialize();
#endif
        }
        public void addTestDevice(string deviceId)
        {
#if ENABLE_ADS_YANDEX && !UNITY_EDITOR
            adsMyYandexIF.addTestDevice(deviceId);
#endif
        }
        public void setTestMode(bool isTestMode)
        {
#if ENABLE_ADS_YANDEX && !UNITY_EDITOR
            adsMyYandexIF.setTestMode(isTestMode);
#endif
        }
        public void setBannerPos(int pos, int width, float dxCenter)
        {
#if ENABLE_ADS_YANDEX && !UNITY_EDITOR
            adsMyYandexIF.setBannerPos(pos, width, dxCenter);
#endif
        }
        public void showBanner(string adsId, int pos, int width, int orien, bool iPad, float dxCenter)
        {
#if ENABLE_ADS_YANDEX && !UNITY_EDITOR
            adsMyYandexIF.showBanner(adsId, pos, width, orien, iPad, dxCenter);
#endif
        }
        public void hideBanner()
        {
#if ENABLE_ADS_YANDEX && !UNITY_EDITOR
            adsMyYandexIF.hideBanner();
#endif
        }

        public void clearCurrFull()
        {
#if ENABLE_ADS_YANDEX && !UNITY_EDITOR
            adsMyYandexIF.clearCurrFull();
#endif
        }

        public void loadFull(string adsId)
        {
#if ENABLE_ADS_YANDEX && !UNITY_EDITOR
            adsMyYandexIF.loadFull(adsId);
#endif
        }
        public bool showFull()
        {
#if ENABLE_ADS_YANDEX && !UNITY_EDITOR
             return adsMyYandexIF.showFull(); 
#else
            return false;
#endif
        }

        public void clearCurrGift()
        {
#if ENABLE_ADS_YANDEX && !UNITY_EDITOR
            adsMyYandexIF.clearCurrGift();
#endif
        }

        public void loadGift(string adsId)
        {
#if ENABLE_ADS_YANDEX && !UNITY_EDITOR
            adsMyYandexIF.loadGift(adsId); 
#endif
        }
        public bool showGift()
        {
#if ENABLE_ADS_YANDEX && !UNITY_EDITOR
            return adsMyYandexIF.showGift(); 
#else
            return false;
#endif
        }

#endregion

#region Callbacks from UnityInterstitialAdListener.
        public void onBannerLoaded()
        {
            if (onBNLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNLoaded();
                });
            }
        }
        public void onBannerLoadFail(string err)
        {
            if (onBNLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNLoadFail(err);
                });
            }
        }
        public void onBannerClose()
        {
            if (onBNClose != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNClose();
                });
            }
        }
        public void onBannerOpen()
        {
            if (onBNOpen != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNOpen();
                });
            }
        }
        public void onBannerClick()
        {
            if (onBNClick != null)
            {
                onBNClick();
            }
        }
        public void onBannerImpression()
        {
            if (onBNImpression != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNImpression();
                });
            }
        }
        public void onBannerPaid(int precisionType, string currencyCode, long valueMicros)
        {
            if (onBNPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onBNPaid(precisionType, currencyCode, valueMicros);
                });
            }
        }

        //Full
        public void onFullLoaded()
        {
            if (onInterstitialLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialLoaded();
                });
            }
        }
        public void onFullLoadFail(string err)
        {
            if (onInterstitialLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialLoadFail(err);
                });
            }
        }
        public void onFullFailedToShow(string err)
        {
            if (onInterstitialFailedToShow != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialFailedToShow(err);
                });
            }
        }
        public void onFullShowed()
        {
            if (onInterstitialShowed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialShowed();
                });
            }
        }
        public void onFullClick()
        {
            if (onInterstitialClick != null)
            {
                onInterstitialClick();
            }
        }
        public void onFullDismissed()
        {
            if (onInterstitialDismissed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialDismissed();
                });
            }
        }
        public void onFullImpresstion()
        {
            if (onInterstitialImpresstion != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialImpresstion();
                });
            }
        }
        public void onFullPaid(int precisionType, string currencyCode, long valueMicros)
        {
            if (onInterstitialPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialPaid(precisionType, currencyCode, valueMicros);
                });
            }
        }

        //gift
        public void onGiftLoaded()
        {
            if (onRewardLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRewardLoaded();
                });
            }
        }
        public void onGiftLoadFail(string err)
        {
            if (onRewardLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRewardLoadFail(err);
                });
            }
        }
        public void onGiftFailedToShow(string err)
        {
            if (onRewardFailedToShow != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRewardFailedToShow(err);
                });
            }
        }
        public void onGiftShowed()
        {
            if (onRewardShowed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRewardShowed();
                });
            }
        }
        public void onGiftClick()
        {
            if (onRewardClick != null)
            {
                onRewardClick();
            }
        }
        public void onGiftDismissed()
        {
            if (onRewardDismissed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRewardDismissed();
                });
            }
        }
        public void onGiftImpresstion()
        {
            if (onRewardImpresstion != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRewardImpresstion();
                });
            }
        }
        public void onGiftReward()
        {
            if (onRewardReward != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRewardReward();
                });
            }
        }
        public void onGiftPaid(int precisionType, string currencyCode, long valueMicros)
        {
            if (onRewardPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onRewardPaid(precisionType, currencyCode, valueMicros);
                });
            }
        }
#endregion

#region Callbacks from ios.
        public void iOSCBBannerLoaded()
        {
            onBannerLoaded();
        }
        public void iOSCBBannerLoadFail(string err)
        {
            onBannerLoadFail(err);
        }
        public void iOSCBBannerClose()
        {
            onBannerClose();
        }
        public void iOSCBBannerOpen()
        {
            onBannerOpen();
        }
        public void iOSCBBannerClick()
        {
            onBannerClick();
        }
        public void iOSCBBannerImpression()
        {
            onBannerImpression();
        }
        public void iOSCBBannerPaid(string param)
        {
            //SdkUtil.logd("admobmy banner paid=" + param);
            //string[] arr = param.Split(';');
            //if (arr != null && arr.Length == 3)
            //{
            //    int precisionType = int.Parse(arr[0]);
            //    string currencyCode = arr[1];
            //    long valueMicros = long.Parse(arr[2]);
            //    onBannerPaid(precisionType, currencyCode, valueMicros);
            //}
        }

        //Full
        public void iOSCBFullLoaded()
        {
            onFullLoaded();
        }
        public void iOSCBonFullLoadFail(string err)
        {
            onFullLoadFail(err);
        }
        public void iOSCBFullFailedToShow(string err)
        {
            onFullFailedToShow(err);
        }
        public void iOSCBFullShowed()
        {
            onFullShowed();
        }
        public void iOSCBFullDismissed()
        {
            onFullDismissed();
        }
        public void iOSCBFullImpresstion()
        {
            onFullImpresstion();
        }
        public void iOSCBFullPaid(string param)
        {
            //SdkUtil.logd("admobmy full paid=" + param);
            //string[] arr = param.Split(';');
            //if (arr != null && arr.Length == 3)
            //{
            //    int precisionType = int.Parse(arr[0]);
            //    string currencyCode = arr[1];
            //    long valueMicros = long.Parse(arr[2]);
            //    onInterstitialPaid(precisionType, currencyCode, valueMicros);
            //}
        }

        //gift
        public void iOSCBGiftLoaded()
        {
            onGiftLoaded();
        }
        public void iOSCBGiftLoadFail(string err)
        {
            onGiftLoadFail(err);
        }
        public void iOSCBGiftFailedToShow(string err)
        {
            onGiftFailedToShow(err);
        }
        public void iOSCBGiftShowed()
        {
            onGiftShowed();
        }
        public void iOSCBGiftDismissed()
        {
            onGiftDismissed();
        }
        public void iOSCBGiftImpresstion()
        {
            onGiftImpresstion();
        }
        public void iOSCBGiftReward()
        {
            onGiftReward();
        }
        public void iOSCBGiftPaid(string param)
        {
            //SdkUtil.logd("admobmy gift paid=" + param);
            //string[] arr = param.Split(';');
            //if (arr != null && arr.Length == 3)
            //{
            //    int precisionType = int.Parse(arr[0]);
            //    string currencyCode = arr[1];
            //    long valueMicros = long.Parse(arr[2]);
            //    onRewardPaid(precisionType, currencyCode, valueMicros);
            //}
        }
#endregion
    }
}
