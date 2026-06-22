using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_ANDROID
using mygame.plugin.Android;
#endif
namespace mygame.sdk
{
    public class AdsMaxMyBridge : MonoBehaviour
    {
        public static event Action<string, string, string> onOpenAdLoaded;
        public static event Action<string, string, string> onOpenAdLoadFail;
        public static event Action<string, string, string, string> onOpenAdFailedToShow;
        public static event Action<string, string, string> onOpenAdShowed;
        public static event Action<string, string, string> onOpenAdClick;
        public static event Action<string, string, string> onOpenAdImpresstion;
        public static event Action<string, string, string> onOpenAdDismissed;
        public static event Action<string, string, string, string, string, string, double> onOpenAdPaid;

        public static event Action<string, string, string> onNativeFullLoaded;
        public static event Action<string, string, string> onNativeFullLoadFail;
        public static event Action<string, string, string, string> onNativeFullFailedToShow;
        public static event Action<string, string, string> onNativeFullShowed;
        public static event Action<string, string, string> onNativeFullClick;
        public static event Action<string, string, string> onNativeFullImpresstion;
        public static event Action<string, string, string> onNativeFullDismissed;
        public static event Action<string, string, string> onNativeFullFinishShow;
        public static event Action<string, string, string, string, string, string, double> onNativeFullPaid;

        private AdsMaxMyIF adsMaxIF;

        public static AdsMaxMyBridge Instance { get; private set; }

#if UNITY_ANDROID
#endif

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                gameObject.name = "AdsMaxMyBridge";
#if USE_ADSMAX_MY && !UNITY_EDITOR
#if UNITY_ANDROID
                adsMaxIF = new AdsMaxMyAndroid();
#else
                adsMaxIF = new AdsMaxMyiOS();
#endif
#endif
            }
            else
            {
                //if (this != Instance) Destroy(gameObject);
            }
        }


        #region MaxClient implementation
        public void Initialize(string appId)
        {
#if USE_ADSMAX_MY && !UNITY_EDITOR
            adsMaxIF.Initialize(appId);
#endif
        }
        public void setCfNtFull(int v1, int v2, int v3, int v4, int v5, int v6)
        {
#if USE_ADSMAX_MY && !UNITY_EDITOR
            adsMaxIF.setCfNtFull(v1, v2, v3, v4, v5, v6);
#endif
        }
        public void setTestDevices(string testDevices)
        {
#if USE_ADSMAX_MY && !UNITY_EDITOR
#if UNITY_ANDROID
            ((AdsMaxMyAndroid)adsMaxIF).setTestDevices(testDevices);
#endif
#endif
        }
        //
        public void loadOpenAd(string placement, string adsId)
        {
#if USE_ADSMAX_MY && !UNITY_EDITOR
            adsMaxIF.loadOpenAd(placement, adsId);
#endif
        }
        public bool showOpenAd(string placement)
        {
#if USE_ADSMAX_MY && !UNITY_EDITOR
             return adsMaxIF.showOpenAd(placement, 0); 
#else
            return false;
#endif
        }
        //
        public void loadNativeFull(string placement, string adsId, int orient)
        {
#if USE_ADSMAX_MY && !UNITY_EDITOR
            adsMaxIF.loadNativeFull(placement, adsId, orient);
#endif
        }
        public bool showNativeFull(string placement, int timeClose, int timeDelay, bool isAutoCloseWhenClick)
        {
#if USE_ADSMAX_MY && !UNITY_EDITOR
             return adsMaxIF.showNativeFull(placement, timeClose, timeDelay, isAutoCloseWhenClick); 
#else
            return false;
#endif
        }

        #endregion
#if UNITY_ANDROID

        #region Callbacks from UnityInterstitialAdListener.
        //Openad
        public void onOpenLoaded(string placement, string adsId, string netName)
        {
            if (onOpenAdLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onOpenAdLoaded(placement, adsId, netName);
                });
            }
        }
        public void onOpenLoadFail(string placement, string adsId, string err)
        {
            if (onOpenAdLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onOpenAdLoadFail(placement, adsId, err);
                });
            }
        }
        public void onOpenFailedToShow(string placement, string adsId, string netName, string err)
        {
            if (onOpenAdFailedToShow != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onOpenAdFailedToShow(placement, adsId, netName, err);
                });
            }
        }
        public void onOpenShowed(string placement, string adsId, string netName)
        {
            if (onOpenAdShowed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onOpenAdShowed(placement, adsId, netName);
                });
            }
        }
        public void onOpenDismissed(string placement, string adsId, string netName)
        {
            if (onOpenAdDismissed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onOpenAdDismissed(placement, adsId, netName);
                });
            }
        }
        public void onOpenImpresstion(string placement, string adsId, string netName)
        {
            if (onOpenAdImpresstion != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onOpenAdImpresstion(placement, adsId, netName);
                });
            }
        }
        public void onOpenClick(string placement, string adsId, string netName)
        {
            if (onOpenAdClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                    onOpenAdClick(placement, adsId, netName);
                //});
            }
        }
        public void onOpenPaid(string placement, string adsId, string netName, string format, string adPlacement, string netPlacement, double value)
        {
            if (onOpenAdPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onOpenAdPaid(placement, adsId, netName, format, adPlacement, netPlacement, value);
                });
            }
        }
        //Native Full
        public void onNtFullLoaded(string placement, string adsId, string netName)
        {
            if (onNativeFullLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullLoaded(placement, adsId, netName);
                });
            }
        }
        public void onNtFullLoadFail(string placement, string adsId, string err)
        {
            if (onNativeFullLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullLoadFail(placement, adsId, err);
                });
            }
        }
        public void onNtFullFailedToShow(string placement, string adsId, string netName, string err)
        {
            if (onNativeFullFailedToShow != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullFailedToShow(placement, adsId, netName, err);
                });
            }
        }
        public void onNtFullShowed(string placement, string adsId, string netName)
        {
            if (onNativeFullShowed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullShowed(placement, adsId, netName);
                });
            }
        }
        public void onNtFullDismissed(string placement, string adsId, string netName)
        {
            if (onNativeFullDismissed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullDismissed(placement, adsId, netName);
                });
            }
        }
        public void onNtFullImpresstion(string placement, string adsId, string netName)
        {
            if (onNativeFullImpresstion != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullImpresstion(placement, adsId, netName);
                });
            }
        }
        public void onNtFullClick(string placement, string adsId, string netName)
        {
            if (onNativeFullClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onNativeFullClick(placement, adsId, netName);
                //});
            }
        }
        public void onNtFullPaid(string placement, string adsId, string netName, string format, string adPlacement, string netPlacement, double value)
        {
            if (onNativeFullPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullPaid(placement, adsId, netName, format, adPlacement, netPlacement, value);
                });
            }
        }
        #endregion

#elif UNITY_IOS || UNITY_IPHONE

        #region Callbacks from ios.
        //OpenAd
        public void iOSCBOpenAdLoaded(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onOpenAdLoaded != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onOpenAdLoaded(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBonOpenAdLoadFail(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onOpenAdLoadFail != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onOpenAdLoadFail(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBOpenAdFailedToShow(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 4)
            {
                if (onOpenAdFailedToShow != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onOpenAdFailedToShow(arr[0], arr[1], arr[2], arr[3]);
                    });
                }
            }
        }
        public void iOSCBOpenAdShowed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onOpenAdShowed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onOpenAdShowed(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBOpenAdDismissed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onOpenAdDismissed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onOpenAdDismissed(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBOpenAdImpresstion(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onOpenAdImpresstion != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onOpenAdImpresstion(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBOpenAdClick(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onOpenAdClick != null)
                {
                    //AdsProcessCB.Instance().Enqueue(() =>
                    //{
                        onOpenAdClick(arr[0], arr[1], arr[2]);
                    //});
                }
            }
        }
        public void iOSCBOpenAdPaid(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 7)
            {
                if (onOpenAdPaid != null)
                {
                    if (!double.TryParse(arr[6], NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
	                {
	                    result = 0;
	                }
                    onOpenAdPaid(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], result);
                }
            }
        }
        //Native Full
        public void iOSCBNtFullLoaded(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeFullLoaded != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullLoaded(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtFullLoadFail(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeFullLoadFail != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullLoadFail(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtFullFailedToShow(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 4)
            {
                if (onNativeFullFailedToShow != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullFailedToShow(arr[0], arr[1], arr[2], arr[3]);
                    });
                }
            }
        }
        public void iOSCBNtFullShowed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeFullShowed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullShowed(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtFullDismissed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeFullDismissed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullDismissed(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtFullFinishShow(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeFullFinishShow != null)
                {
                    onNativeFullFinishShow(arr[0], arr[1], arr[2]);
                }
            }
        }
        public void iOSCBNtFullImpresstion(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeFullImpresstion != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullImpresstion(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtFullClick(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 3)
            {
                if (onNativeFullClick != null)
                {
                    //AdsProcessCB.Instance().Enqueue(() =>
                    //{
                        onNativeFullClick(arr[0], arr[1], arr[2]);
                    //});
                }
            }
        }
        public void iOSCBNtFullPaid(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 7)
            {
                if (!double.TryParse(arr[6], NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                {
                    result = 0;
                }
                if (onNativeFullPaid != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullPaid(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], result);
                    });
                }
            }
        }
        #endregion
#endif
    }
}
