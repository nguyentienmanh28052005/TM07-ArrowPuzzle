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
    public class AdsAdmobMyLowBridge : MonoBehaviour
    {
        public static event Action<string, string, string> onInterstitialLoaded;
        public static event Action<string, string, string> onInterstitialLoadFail;
        public static event Action<string, string, string, string> onInterstitialFailedToShow;
        public static event Action<string, string, string> onInterstitialShowed;
        public static event Action<string, string, string> onInterstitialClick;
        public static event Action<string, string, string> onInterstitialImpresstion;
        public static event Action<string, string, string> onInterstitialDismissed;
        public static event Action<string, string, string> onInterstitialFinishShow;
        public static event Action<string, string, string, int, string, long> onInterstitialPaid;

        private AdsAdmobMyIF adsAdmobIF;

        public static AdsAdmobMyLowBridge Instance { get; private set; }

#if UNITY_ANDROID
#endif

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                gameObject.name = "AdsAdmobMyLowBridge";
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
#if UNITY_ANDROID
adsAdmobIF = new AdsAdmobMyLowAndroid();
#else
adsAdmobIF = new AdsAdmobMyLowiOS();
#endif
#endif
            }
            else
            {
                //if (this != Instance) Destroy(gameObject);
            }
        }


        #region IGoogleMobileAdsInterstitialClient implementation
        public void Initialize()
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.Initialize();
#endif
        }
        public void setLog(bool isLog)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.setLog(isLog);
#endif
        }
        //
        public void clearCurrFull(string placement)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.clearCurrFull(placement);
#endif
        }
        public void loadFull(string placement, string adsId, int index)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
            adsAdmobIF.loadFull(placement, adsId, index);
#endif
        }
        public bool showFull(string placement, int timeDelay)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && !UNITY_EDITOR
             return adsAdmobIF.showFull(placement, timeDelay); 
#else
            return false;
#endif
        }

        #endregion

#if UNITY_ANDROID

        #region Callbacks from UnityInterstitialAdListener.
        //Full
        public void onFullLoaded(string placement, string adsId, string adnet)
        {
            if (onInterstitialLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialLoaded(placement, adsId, adnet);
                });
            }
        }
        public void onFullLoadFail(string placement, string adsId, string err)
        {
            if (onInterstitialLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialLoadFail(placement, adsId, err);
                });
            }
        }
        public void onFullFailedToShow(string placement, string adsId, string adnet, string err)
        {
            if (onInterstitialFailedToShow != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialFailedToShow(placement, adsId, adnet, err);
                });
            }
        }
        public void onFullShowed(string placement, string adsId, string adnet)
        {
            if (onInterstitialShowed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialShowed(placement, adsId, adnet);
                });
            }
        }
        public void onFullDismissed(string placement, string adsId, string adnet)
        {
            if (onInterstitialDismissed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialDismissed(placement, adsId, adnet);
                });
            }
        }
        public void onFullImpresstion(string placement, string adsId, string adnet)
        {
            if (onInterstitialImpresstion != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialImpresstion(placement, adsId, adnet);
                });
            }
        }
        public void onFullClick(string placement, string adsId, string adnet)
        {
            if (onInterstitialClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onInterstitialClick(placement, adsId, adnet);
                //});
            }
        }
        public void onFullPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onInterstitialPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onInterstitialPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        #endregion

#elif UNITY_IOS || UNITY_IPHONE

        #region Callbacks from ios.
        //Full
        public void iOSCBFullLoaded(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onInterstitialLoaded != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onInterstitialLoaded(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBonFullLoadFail(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onInterstitialLoadFail != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onInterstitialLoadFail(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBFullFailedToShow(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 4)
            {
                if (onInterstitialFailedToShow != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onInterstitialFailedToShow(arr[0], arr[1], arr[2], arr[3]);
                    });
                }
            }
        }
        public void iOSCBFullShowed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onInterstitialShowed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onInterstitialShowed(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBFullDismissed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onInterstitialDismissed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onInterstitialDismissed(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBFullFinishShow(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onInterstitialFinishShow != null)
                {
                    onInterstitialFinishShow(arr[0], arr[1], arr[2]);
                }
            }
        }
        public void iOSCBFullImpresstion(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onInterstitialImpresstion != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onInterstitialImpresstion(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBFullClick(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onInterstitialClick != null)
                {
                    //AdsProcessCB.Instance().Enqueue(() =>
                    //{
                        onInterstitialClick(arr[0], arr[1], arr[2]);
                    //});
                }
            }
        }
        public void iOSCBFullPaid(string param)
        {
            SdkUtil.logd("admobmy full paid=" + param);
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 6)
            {
                int precisionType = int.Parse(arr[3]);
                string currencyCode = arr[4];
                long valueMicros = long.Parse(arr[5]);
                onInterstitialPaid(arr[0], arr[1], arr[2], precisionType, currencyCode, valueMicros);
            }
        }
        #endregion
#endif
    }
}
