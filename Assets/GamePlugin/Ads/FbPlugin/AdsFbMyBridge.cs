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
    public class AdsFbMyBridge : MonoBehaviour
    {
        public static event Action<string, string> onNativeFullLoaded;
        public static event Action<string, string, string> onNativeFullLoadFail;
        public static event Action<string, string, string> onNativeFullFailedToShow;
        public static event Action<string, string> onNativeFullShowed;
        public static event Action<string, string> onNativeFullClick;
        public static event Action<string, string> onNativeFullImpresstion;
        public static event Action<string, string> onNativeFullDismissed;
        public static event Action<string, string, string> onNativeFullFinishShow;
        public static event Action<string, string, string, int, string, long> onNativeFullPaid;

        public static event Action<string, string> onNativeIcFullLoaded;
        public static event Action<string, string, string> onNativeIcFullLoadFail;
        public static event Action<string, string, string> onNativeIcFullFailedToShow;
        public static event Action<string, string> onNativeIcFullShowed;
        public static event Action<string, string> onNativeIcFullClick;
        public static event Action<string, string> onNativeIcFullImpresstion;
        public static event Action<string, string> onNativeIcFullDismissed;
        public static event Action<string, string, string> onNativeIcFullFinishShow;
        public static event Action<string, string, string, int, string, long> onNativeIcFullPaid;

        private AdsFbMyIF adsFbIF;

        public static AdsFbMyBridge Instance { get; private set; }

#if UNITY_ANDROID
#endif

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                gameObject.name = "AdsFbMyBridge";
#if ENABLE_ADS_FB && !UNITY_EDITOR
#if UNITY_ANDROID
                adsFbIF = new AdsFbMyAndroid();
#else
                adsFbIF = new FbMyiOS();
#endif
#endif
            }
            else
            {
                //if (this != Instance) Destroy(gameObject);
            }
        }

        #region IFBAdsInterstitialClient implementation
        public void Initialize()
        {
#if ENABLE_ADS_FB && !UNITY_EDITOR
            adsFbIF.Initialize();
#endif
        }
        public void setLog(bool isLog)
        {
#if ENABLE_ADS_FB && !UNITY_EDITOR
            adsFbIF.setLog(isLog);
#endif
        }
        public void setCfNtFull(int v1, int v2, int v3, int v4, int v5, int v6)
        {
#if ENABLE_ADS_FB && !UNITY_EDITOR
            adsFbIF.setCfNtFull(v1, v2, v3, v4, v5, v6);
#endif
        }
        public void setCfNtFullFbExcluse(int rows, int columns, string areaExcluse)
        {
#if ENABLE_ADS_FB && !UNITY_EDITOR
            adsFbIF.setCfNtFullFbExcluse(rows, columns, areaExcluse);
#endif
        }
        public void setTestDevices(string testDevices)
        {
#if ENABLE_ADS_FB && !UNITY_EDITOR
#if UNITY_ANDROID
            ((AdsFbMyAndroid)adsFbIF).setTestDevices(testDevices);
#endif
#endif
        }
        //
        public void loadNativeFull(string placement, string adsId, int orient)
        {
#if ENABLE_ADS_FB && !UNITY_EDITOR
            adsFbIF.loadNativeFull(placement, adsId, orient);
#endif
        }
        public bool showNativeFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick)
        {
#if ENABLE_ADS_FB && !UNITY_EDITOR
             return adsFbIF.showNativeFull(placement, isNham, timeShowBtClose, timeDelay, isAutoCloseWhenClick);
#else
            return false;
#endif
        }
        //
        public void loadNativeIcFull(string placement, string adsId, int orient)
        {
#if ENABLE_ADS_FB && !UNITY_EDITOR
            adsFbIF.loadNativeIcFull(placement, adsId, orient);
#endif
        }
        public bool showNativeIcFull(string placement, bool isNham, int timeShowBtClose, int timeDelay, bool isAutoCloseWhenClick)
        {
#if ENABLE_ADS_FB && !UNITY_EDITOR
             return adsFbIF.showNativeIcFull(placement, isNham, timeShowBtClose, timeDelay, isAutoCloseWhenClick);
#else
            return false;
#endif
        }

        #endregion
#if UNITY_ANDROID

        #region Callbacks from UnityInterstitialAdListener.
        
        //Native Full
        public void onNtFullLoaded(string placement, string adsId)
        {
            if (onNativeFullLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullLoaded(placement, adsId);
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
        public void onNtFullFailedToShow(string placement, string adsId, string err)
        {
            if (onNativeFullFailedToShow != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullFailedToShow(placement, adsId, err);
                });
            }
        }
        public void onNtFullShowed(string placement, string adsId)
        {
            if (onNativeFullShowed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullShowed(placement, adsId);
                });
            }
        }
        public void onNtFullDismissed(string placement, string adsId)
        {
            if (onNativeFullDismissed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullDismissed(placement, adsId);
                });
            }
        }
        public void onNtFullImpresstion(string placement, string adsId)
        {
            if (onNativeFullImpresstion != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullImpresstion(placement, adsId);
                });
            }
        }
        public void onNtFullClick(string placement, string adsId)
        {
            if (onNativeFullClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onNativeFullClick(placement, adsId);
                //});
            }
        }
        public void onNtFullPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onNativeFullPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeFullPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        //Native IC Full
        public void onNtIcFullLoaded(string placement, string adsId)
        {
            if (onNativeIcFullLoaded != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeIcFullLoaded(placement, adsId);
                });
            }
        }
        public void onNtIcFullLoadFail(string placement, string adsId, string err)
        {
            if (onNativeIcFullLoadFail != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeIcFullLoadFail(placement, adsId, err);
                });
            }
        }
        public void onNtIcFullFailedToShow(string placement, string adsId, string err)
        {
            if (onNativeIcFullFailedToShow != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeIcFullFailedToShow(placement, adsId, err);
                });
            }
        }
        public void onNtIcFullShowed(string placement, string adsId)
        {
            if (onNativeIcFullShowed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeIcFullShowed(placement, adsId);
                });
            }
        }
        public void onNtIcFullDismissed(string placement, string adsId)
        {
            if (onNativeIcFullDismissed != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeIcFullDismissed(placement, adsId);
                });
            }
        }
        public void onNtIcFullImpresstion(string placement, string adsId)
        {
            if (onNativeIcFullImpresstion != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeIcFullImpresstion(placement, adsId);
                });
            }
        }
        public void onNtIcFullClick(string placement, string adsId)
        {
            if (onNativeIcFullClick != null)
            {
                //AdsProcessCB.Instance().Enqueue(() =>
                //{
                onNativeIcFullClick(placement, adsId);
                //});
            }
        }
        public void onNtIcFullPaid(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            if (onNativeIcFullPaid != null)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    onNativeIcFullPaid(placement, adsId, adNet, precisionType, currencyCode, valueMicros);
                });
            }
        }
        #endregion

#elif UNITY_IOS || UNITY_IPHONE

        #region Callbacks from ios.
        //Native Full
        public void iOSCBNtFullLoaded(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 2)
            {
                if (onNativeFullLoaded != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullLoaded(arr[0], arr[1]);
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
            if (arr != null && arr.Length == 3)
            {
                if (onNativeFullFailedToShow != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullFailedToShow(arr[0], arr[1], arr[2]);
                    });
                }
            }
        }
        public void iOSCBNtFullShowed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 2)
            {
                if (onNativeFullShowed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullShowed(arr[0], arr[1]);
                    });
                }
            }
        }
        public void iOSCBNtFullDismissed(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 2)
            {
                if (onNativeFullDismissed != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullDismissed(arr[0], arr[1]);
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
            if (arr != null && arr.Length == 2)
            {
                if (onNativeFullImpresstion != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullImpresstion(arr[0], arr[1]);
                    });
                }
            }
        }
        public void iOSCBNtFullClick(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 2)
            {
                if (onNativeFullClick != null)
                {
                    //AdsProcessCB.Instance().Enqueue(() =>
                    //{
                        onNativeFullClick(arr[0], arr[1]);
                    //});
                }
            }
        }
        public void iOSCBNtFullPaid(string param)
        {
            string[] arr = param.Split(';');
            if (arr != null && arr.Length == 6)
            {
                int precisionType = int.Parse(arr[3]);
                string currencyCode = arr[4];
                long valueMicros = long.Parse(arr[5]);
                if (onNativeFullPaid != null)
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        onNativeFullPaid(arr[0], arr[1], arr[2], precisionType, currencyCode, valueMicros);
                    });
                }
            }
        }
        #endregion
#endif
    }
}
