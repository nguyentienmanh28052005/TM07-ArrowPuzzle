//#define ENABLE_AppsFlyer
//#define AppsFlyer_IAPConnector
//#define Test_campain_source
//#define CheckLogAdLoad
#define ENABLE_LOG_AD_EVENT
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyJson;
#if ENABLE_AppsFlyer
using AppsFlyerSDK;
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif
#if AppsFlyer_IAPConnector
using AppsFlyerConnector;
#endif
#endif

namespace mygame.sdk
{
    public enum AFMediationNetwork : ulong
    {
        GoogleAdMob = 1,
        IronSource = 2,
        ApplovinMax = 3,
        Fyber = 4,
        Appodeal = 5,
        Admost = 6,
        Topon = 7,
        Tradplus = 8,
        Yandex = 9,
        ChartBoost = 10,
        Unity = 11,
        ToponPte = 12,
        Custom = 13,
        DirectMonetization = 14
    }

    // This class is intended to be used the the AppsFlyerObject.prefab

#if ENABLE_AppsFlyer
    public class AppsFlyerHelperScript : MonoBehaviour, IAppsFlyerConversionData
#else
    public class AppsFlyerHelperScript : MonoBehaviour
#endif
    {
        public static Action<bool, string> OnConversionDataDone;
        public static AppsFlyerHelperScript Instance = null;

        // These fields are set from the editor so do not modify!
        //******************************//
        public string devKey;
        public string appID;
        public string UWPAppID;
        public bool isDebug;
        public bool getConversionData;
        //******************************//

        int isLogCon = 1;
        private bool isStartAf = false;

        private Action<bool> _callback;
        private string memIapIonInfo = "";

        public bool isGetData { get; private set; }
        private Dictionary<string, string> dicMediaSource = new Dictionary<string, string>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                string[] mediaSourceMap = {
                    "gg", "googleadwords_int",
                    "min", "mintegral_int",
                    "apl", "applovin_int",
                    "fb", "Facebook Ads",
                    "uni", "unityads_int",
                    "cpr", "af_cross_promotion",
                    "ir", "ironsource_int",
                    "tik1", "bytedanceglobal_int",
                    "ret", "restricted",
                    "asa", "Apple Search Ads",
                    "mvis", "mobvista_int",
                    "tik2", "tiktokglobal_int",
                    "tap", "tapjoy_int"
                };
                for (int i = 0; i < mediaSourceMap.Length / 2; i++)
                {
                    dicMediaSource.Add(mediaSourceMap[2 * i], mediaSourceMap[2 * i + 1]);
                }
            }
        }

        void Start()
        {
#if ENABLE_AppsFlyer
            // These fields are set from the editor so do not modify!
            //******************************//
            AppsFlyer.setIsDebug(isDebug);
            AppsFlyer.setHost("", "appsflyersdk.com");
            isGetData = false;
#if UNITY_WSA_10_0 && !UNITY_EDITOR
            AppsFlyer.initSDK(devKey, UWPAppID, getConversionData ? this : null);
#else
            AppsFlyer.initSDK(devKey, appID, getConversionData ? this : null);
#endif
            AppsFlyer.OnDeepLinkReceived += OnDeepLink;

#if UNITY_EDITOR
            StartCoroutine(test4Editor());
#endif

#if AppsFlyer_IAPConnector

#if UNITY_ANDROID
            AppsFlyerPurchaseConnector.init(this, AppsFlyerConnector.Store.GOOGLE);
            AppsFlyerPurchaseConnector.setIsSandbox(isDebug);
            int per = PlayerPrefs.GetInt("cf_per_post_iap", AppConfig.PerValuePostIAP);
            if (per >= 50)
            {
                AppsFlyerPurchaseConnector.setAutoLogPurchaseRevenue(AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsDisabled);
                //AppsFlyerPurchaseConnector.setAutoLogPurchaseRevenue(AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions, AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases);
            }
            AppsFlyerPurchaseConnector.setPurchaseRevenueValidationListeners(true);
            AppsFlyerPurchaseConnector.build();
            AppsFlyerPurchaseConnector.startObservingTransactions();
#endif
#endif
            //******************************/
            isStartAf = false;
            StartCoroutine(waitStartAF());
#endif
        }

#if UNITY_IOS
        IEnumerator RequestAuthorization()
        {
            using (var req = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge, true))
            {
                while (!req.IsFinished)
                {
                    yield return null;
                }
                Debug.Log($"Granted = {req.Granted}");
                Debug.Log($"Error = {req.Error}");
                Debug.Log($"Token = [{req.DeviceToken}]");
                if (req.Granted && !string.IsNullOrEmpty(req.DeviceToken)) 
                {
                    byte[] tokenBytes = ConvertHexStringToByteArray(req.DeviceToken);
                    AppsFlyer.registerUninstall(tokenBytes);
                }
            }
        }

    private byte[] ConvertHexStringToByteArray(string hexString)
    {

        byte[] data = new byte[hexString.Length / 2];
        for (int index = 0; index < data.Length; index++)
        {
            string byteValue = hexString.Substring(index * 2, 2);
            data[index] = System.Convert.ToByte(byteValue, 16);
        }
        return data;
    }
#endif

        IEnumerator waitStartAF()
        {
#if UNITY_ANDROID
            yield return new WaitForSeconds(1);
#else
            yield return new WaitForSeconds(60);
#endif
            checkStartAF();
        }

        public void checkStartAF()
        {
#if ENABLE_AppsFlyer
            if (!isStartAf)
            {
                Debug.Log($"mysdk: AF checkStartAF start");
                isStartAf = true;
                AppsFlyer.startSDK();
#if UNITY_IOS
                StartCoroutine(RequestAuthorization());
#endif
            }
            else
            {
                Debug.Log($"mysdk: AF checkStartAF is started");
            }
#endif
        }

        IEnumerator test4Editor()
        {
            yield return new WaitForSeconds(0.04f);
            string oldcam = PlayerPrefsBase.Instance().getString("mem_af_media_campain", "UnKnown");
            SDKManager.Instance.mediaSource = "applovin_int";
            SDKManager.Instance.mediaCampain = "iap_any_apl_D7_abc_xyzmn";
            PlayerPrefsBase.Instance().setString("mem_af_media_source", SDKManager.Instance.mediaSource);
            PlayerPrefsBase.Instance().setString("mem_af_media_campain", SDKManager.Instance.mediaCampain);
            isGetData = true;
            AdsHelper.Instance.checkCampain(oldcam != SDKManager.Instance.mediaCampain);
            Debug.Log($"mysdk: AF testttttttttttttt eeeeeeeeeeeeeeeeeditorrrrrr");
        }

        public void setIapConnectorCB(Action<bool> cb)
        {
            _callback = cb;
            if (memIapIonInfo != null && memIapIonInfo.Length > 10)
            {
                processIapInfo(memIapIonInfo);
                memIapIonInfo = "";
            }
        }

        public void didReceivePurchaseRevenueValidationInfo(string validationInfo)
        {
            if (_callback != null)
            {
                processIapInfo(validationInfo);
                memIapIonInfo = "";
            }
            else
            {
                memIapIonInfo = validationInfo;
            }
        }

        public void didReceivePurchaseRevenueError(string error)
        {
            Debug.Log($"mysdk: AF didReceivePurchaseRevenueError error={error}");
        }

        private void processIapInfo(string validationInfo)
        {
            if (_callback != null)
            {
#if AppsFlyer_IAPConnector
                SdkUtil.logd("AF didReceivePurchaseRevenueValidationInfo " + validationInfo);
                Dictionary<string, object> dictionary = AFMiniJSON.Json.Deserialize(validationInfo) as Dictionary<string, object>;
                int hjk = 1;
                if (dictionary != null)
                {
                    foreach (var item in dictionary)
                    {
                        if (item.Key.ToLower().Equals("success"))
                        {
                            SdkUtil.logd($"AF didReceivePurchaseRevenueValidationInfo: {item.Key}-{item.Value}");
                            string abc = (string)item.Value;
                            if (abc.ToLower().Equals("true"))
                            {
                                hjk = 0;
                            }
                            break;
                        }
                    }
                }
                if (hjk == 0)
                {
                    SdkUtil.logd("AF didReceivePurchaseRevenueValidationInfo cb1");
                    _callback(true);
                    _callback = null;
                }
                else
                {
                    SdkUtil.logd("AF didReceivePurchaseRevenueValidationInfo cb2");
                    _callback(false);
                    _callback = null;
                }
#endif
            }
            else
            {
                SdkUtil.logd("AF didReceivePurchaseRevenueValidationInfo cb null");
                FIRhelper.logEvent("IAP_af_connector_cbnull");
            }
        }
#if ENABLE_AppsFlyer
        void OnDeepLink(object sender, EventArgs args)
        {
            SdkUtil.logd("AF OnDeepLink");
            var deepLinkEventArgs = args as DeepLinkEventsArgs;

            switch (deepLinkEventArgs.status)
            {
                case DeepLinkStatus.FOUND:

                    if (deepLinkEventArgs.isDeferred())
                    {
                        SdkUtil.logd("AF OnDeepLink This is a deferred deep link");
                    }
                    else
                    {
                        SdkUtil.logd("AF OnDeepLink This is a direct deep link");
                    }
                    string deep_link_value = "";
                    foreach (var item in deepLinkEventArgs.deepLink)
                    {
                        SdkUtil.logd($"AF OnDeepLink {item.Key}={item.Value}");
                        if (item.Key.CompareTo("deep_link_value") == 0)
                        {
                            deep_link_value = item.Value.ToString();
                        }
                    }

                    // deepLinkParamsDictionary contains all the deep link parameters as keys
                    Dictionary<string, object> deepLinkParamsDictionary = null;
#if UNITY_IOS && !UNITY_EDITOR
              if (deepLinkEventArgs.deepLink.ContainsKey("click_event") && deepLinkEventArgs.deepLink["click_event"] != null)
              {
                  deepLinkParamsDictionary = deepLinkEventArgs.deepLink["click_event"] as Dictionary<string, object>;
              }
#elif UNITY_ANDROID && !UNITY_EDITOR
                  deepLinkParamsDictionary = deepLinkEventArgs.deepLink;
#endif
                    if (deep_link_value != null && deep_link_value.Length > 3)
                    {
                        string mes = "";
                        string[] arrPks = deep_link_value.ToLower().Split('_');
                        if (arrPks.Length >= 3)
                        {
                            mes = arrPks[2];
                            if (mes != null && mes.Length > 1 && dicMediaSource.ContainsKey(mes))
                            {
                                SDKManager.Instance.mediaSource = dicMediaSource[mes];
                            }
                        }
                        string oldmediaCampain = PlayerPrefsBase.Instance().getString("mem_af_media_campain", "UnKnown");
                        string oldmediaSource = PlayerPrefsBase.Instance().getString("mem_af_media_source", "UnKnown");
                        SDKManager.Instance.mediaCampain = deep_link_value;
                        PlayerPrefsBase.Instance().setString("mem_af_media_campain", SDKManager.Instance.mediaCampain);
                        PlayerPrefsBase.Instance().setString("mem_af_media_source", SDKManager.Instance.mediaSource);
                        AdsHelper.Instance.checkCampain(oldmediaCampain != SDKManager.Instance.mediaCampain);
                        if (oldmediaCampain.CompareTo(SDKManager.Instance.mediaCampain) != 0)
                        {
                            string ename = $"af_depl_{SDKManager.Instance.mediaCampain}";
                            if (ename.Length > 38)
                            {
                                ename = ename.Substring(0, 38);
                            }
                            FIRhelper.logEvent(ename);
                        }
                        isGetData = true;
                    }

                    break;
                case DeepLinkStatus.NOT_FOUND:
                    SdkUtil.logd("AF OnDeepLink Deep link not found");
                    break;
                default:
                    SdkUtil.logd("AF OnDeepLink Deep link error");
                    break;
            }
        }
#endif

        // Mark AppsFlyer CallBacks
        public void onConversionDataSuccess(string conversionData)
        {
            if (string.IsNullOrEmpty(conversionData))
            {
                return;
            }
#if ENABLE_AppsFlyer
            Dictionary<string, object> conversionDataDictionary = null;
            try
            {
                conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
            }
            catch (Exception ex)
            {

            }
            string jsondata = "{";
            bool isbg = true;
            string oldmediaSource = PlayerPrefsBase.Instance().getString("mem_af_media_source", "UnKnown");
            string oldmediaCampain = PlayerPrefsBase.Instance().getString("mem_af_media_campain", "UnKnown");
            if (conversionDataDictionary != null)
            {
                foreach (var item in conversionDataDictionary)
                {
                    if (item.Key == null)
                    {
                        continue;
                    }
                    if (item.Key.Equals("media_source"))
                    {
                        string tmp = ((string)item.Value).ToLower();
                        if (tmp != null && !tmp.Contains("unknown"))
                        {
                            SDKManager.Instance.mediaSource = tmp;
                            PlayerPrefsBase.Instance().setString("mem_af_media_source", SDKManager.Instance.mediaSource);
                        }
                    }
                    else if (item.Key.Equals("campaign"))
                    {
                        string tmp = ((string)item.Value).ToLower();
                        if (tmp != null && !tmp.Contains("unknown"))
                        {
                            SDKManager.Instance.mediaCampain = tmp;
                            PlayerPrefsBase.Instance().setString("mem_af_media_campain", SDKManager.Instance.mediaCampain);
                        }
                    }
                    else if (item.Key.Equals("af_adset"))
                    {
                        SDKManager.Instance.afAdset = ((string)item.Value).ToLower();
                        PlayerPrefsBase.Instance().setString("mem_af_adset", SDKManager.Instance.afAdset);
                    }
                    else if (item.Key.Equals("af_ad"))
                    {
                        SDKManager.Instance.afAd = ((string)item.Value).ToLower();
                        PlayerPrefsBase.Instance().setString("mem_af_ad", SDKManager.Instance.afAd);
                    }
                    else if (item.Key.Equals("af_ad_id"))
                    {
                        SDKManager.Instance.afAd = ((string)item.Value).ToLower();
                        PlayerPrefsBase.Instance().setString("mem_af_ad_id", SDKManager.Instance.afAdId);
                    }
                    if (isLogCon > 0)
                    {
                        Debug.Log($"mysdk: AF data conversion key={item.Key}, value={item.Value}");
                        if (isbg)
                        {
                            isbg = false;
                            jsondata += "\"" + item.Key + " \":\"" + item.Value + "\"";
                        }
                        else
                        {
                            jsondata += ",\"" + item.Key + " \":\"" + item.Value + "\"";
                        }
                    }
                }
            }
#if Test_campain_source
            SDKManager.Instance.mediaSource = "ironsource_int";
            PlayerPrefsBase.Instance().setString("mem_af_media_source", SDKManager.Instance.mediaSource);
            SDKManager.Instance.mediaCampain = "iap_ir_d7_abc_xyz";
            PlayerPrefsBase.Instance().setString("mem_af_media_campain", SDKManager.Instance.mediaCampain);
#endif
            if (!checkSameSource("ir"))
            {
                if (AdsHelper.Instance != null)
                {
                    AdsHelper.Instance.statusLogicIron = 0;
                }
                else
                {
                    PlayerPrefs.GetInt("mem_cf_login_iron", 0);
                }
            }
            if (isLogCon > 0)
            {
                jsondata += "}";
                Myapi.LogEventApi.Instance().LogEvent(Myapi.MyEventLog.AdsMaxConversionData, jsondata, true);
                isLogCon--;
            }
            bool isCheckCamSource = false;
            if (oldmediaSource.CompareTo(SDKManager.Instance.mediaSource) != 0 && SDKManager.Instance.mediaSource.Length > 3)
            {
                string eventname = SDKManager.Instance.mediaSource;
                for (int i = 0; i < 10; i++)
                {
                    if (eventname.Contains(" "))
                    {
                        eventname = eventname.Replace(" ", "_");
                    }
                    else
                    {
                        break;
                    }
                }
                if (eventname.Length > 25)
                {
                    eventname = eventname.Substring(0, 25);
                }
                eventname = "af_source_" + eventname;
                FIRhelper.logEvent(eventname);
                isCheckCamSource = true;
                Debug.Log($"mysdk: AF Rcv new mediaSource={SDKManager.Instance.mediaSource}");
            }
            if (oldmediaCampain.CompareTo(SDKManager.Instance.mediaCampain) != 0 && SDKManager.Instance.mediaCampain.Length > 3)
            {
                string eventname = SDKManager.Instance.mediaCampain;
                if (eventname.Length > 25)
                {
                    eventname = eventname.Substring(0, 25);
                }
                eventname = "af_cam_" + eventname;
                FIRhelper.logEvent(eventname);
                isCheckCamSource = true;
                Debug.Log($"mysdk: AF Rcv new meiaCampain={SDKManager.Instance.mediaCampain}");
            }
            isGetData = true;
            AdsHelper.Instance.checkCampain(isCheckCamSource);
            Debug.Log($"mysdk: AF mediaSource={SDKManager.Instance.mediaSource}, meiaCampain={SDKManager.Instance.mediaCampain}");
#endif
            OnConversionDataDone?.Invoke(true, conversionData);
        }

        public void onConversionDataFail(string error)
        {
#if ENABLE_AppsFlyer
            AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
            OnConversionDataDone?.Invoke(false, error);
#endif
        }

        public void onAppOpenAttribution(string attributionData)
        {
#if ENABLE_AppsFlyer
            AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
            Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
            // add direct deeplink logic here
#endif
        }

        public void onAppOpenAttributionFailure(string error)
        {
#if ENABLE_AppsFlyer
            AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
#endif
        }

        public static void logPurchase(string sku, string value, string currency)
        {
#if ENABLE_AppsFlyer
            SdkUtil.logd($"AF logPurchase={sku}-va={value}");
            Dictionary<string, string> dicParams = new Dictionary<string, string>();
            dicParams.Add(AFInAppEvents.CONTENT_ID, sku);
            dicParams.Add(AFInAppEvents.CURRENCY, currency);
            dicParams.Add(AFInAppEvents.REVENUE, value);
            dicParams.Add("vercode_install", "" + SDKManager.Instance.verCodeInstall);
            dicParams.Add("vername_install", SDKManager.Instance.verNameInstall);
            logEvent(AFInAppEvents.PURCHASE, dicParams);
#endif
        }

        public static void logAdRevenue(string placement, AFMediationNetwork mediaNet, string adSource, string adformat, string adunitId, string countrycode, double revenue, string currency)
        {
#if ENABLE_AppsFlyer
            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add(AppsFlyerSDK.AdRevenueScheme.PLACEMENT, placement);
            additionalParams.Add(AppsFlyerSDK.AdRevenueScheme.AD_TYPE, adformat);
            additionalParams.Add(AppsFlyerSDK.AdRevenueScheme.AD_UNIT, adunitId);
            additionalParams.Add("vercode_install", "" + SDKManager.Instance.verCodeInstall);
            additionalParams.Add("vername_install", SDKManager.Instance.verNameInstall);
            if (countrycode != null && countrycode.Length > 0)
            {
                additionalParams.Add(AppsFlyerSDK.AdRevenueScheme.COUNTRY, countrycode);
            }
            AppsFlyerSDK.MediationNetwork mediationNetwork = AppsFlyerSDK.MediationNetwork.GoogleAdMob;
            if (mediaNet == AFMediationNetwork.GoogleAdMob)
            {
                mediationNetwork = AppsFlyerSDK.MediationNetwork.GoogleAdMob;
            }
            else if (mediaNet == AFMediationNetwork.IronSource)
            {
                mediationNetwork = AppsFlyerSDK.MediationNetwork.IronSource;
            }
            else if (mediaNet == AFMediationNetwork.ApplovinMax)
            {
                mediationNetwork = AppsFlyerSDK.MediationNetwork.ApplovinMax;
            }
            else if (mediaNet == AFMediationNetwork.Custom)
            {
                mediationNetwork = AppsFlyerSDK.MediationNetwork.Custom;
            }
            else if (mediaNet == AFMediationNetwork.DirectMonetization)
            {
                mediationNetwork = AppsFlyerSDK.MediationNetwork.DirectMonetization;
            }
            var logRevenue = new AppsFlyerSDK.AFAdRevenueData(adSource, mediationNetwork, currency, revenue);
            AppsFlyerSDK.AppsFlyer.logAdRevenue(logRevenue, additionalParams);

#endif
        }

#if CheckLogAdLoad
        static int loadCount = 0;
        static int loadReCount = 0;
#endif
        public static void logAdEvent(string eventName, string pl, string adformat, string adid, string adPatform, string status)
        {
            if (eventName.CompareTo("ads_impression") == 0)
            {
                return;
            }
#if CheckLogAdLoad
            if (eventName.CompareTo("ads_load") == 0)
            {
                loadCount++;
            } 
            else if (eventName.CompareTo("ads_load_result") == 0)
            {
                loadReCount++;
            }
            Debug.Log($"Ads_event_af: {eventName} format={adformat} id={adid} pf={adPatform} pl={pl} ({loadCount}-{loadReCount})");
#endif
#if ENABLE_LOG_AD_EVENT
#if !CheckLogAdLoad
            Debug.Log($"Ads_event_af: {eventName} format={adformat} id={adid} pf={adPatform} pl={pl}");
#endif
            Dictionary<string, string> dicParams = new Dictionary<string, string>();
            dicParams.Add("ad_format", adformat);
            dicParams.Add("ad_id", adid);
            dicParams.Add("ad_platform", adPatform);
            dicParams.Add("vercode_install", "" + SDKManager.Instance.verCodeInstall);
            dicParams.Add("vername_install", SDKManager.Instance.verNameInstall);
            if (pl != null && pl.Length > 0)
            {
                dicParams.Add("ad_placement", pl);
            }
            if (status != null && status.Length > 0)
            {
                dicParams.Add("ad_status", status);
            }
            logEvent(eventName, dicParams);
#endif
        }

        public static void logEvent(string eventName, Dictionary<string, string> dicPrams = null)
        {
#if ENABLE_AppsFlyer
            if (dicPrams == null)
            {
                Dictionary<string, string> dicParams = new Dictionary<string, string>();
            }
            AppsFlyer.sendEvent(eventName, dicPrams);
#endif
        }

        public static void logLevelAchieve(int lv, string rules = "1-100:5,150-600:50,700:100")
        {
#if ENABLE_AppsFlyer
            Dictionary<string, string> dicParams = new Dictionary<string, string>();
            dicParams.Add(AFInAppEvents.LEVEL, "" + lv);
            AppsFlyer.sendEvent(AFInAppEvents.LEVEL_ACHIEVED, dicParams);
            if (isMatchRules(lv, rules))
            {
                string eventName = $"Level_success_{lv:0000}";
                AppsFlyer.sendEvent(eventName, dicParams);
            }
#endif
        }

        public static bool isMatchRules(int lv, string rules)
        {
            if (rules != null)
            {
                string[] items = rules.Split(',');
                foreach (string ir in items)
                {
                    string[] dr = ir.Split(':');
                    if (dr != null && dr.Length == 2)
                    {
                        string[] splv = dr[0].Split('-');
                        int plv1 = -1;
                        int plv2 = -1;
                        int step = -1;
                        if (splv != null)
                        {
                            if (splv.Length > 0)
                            {
                                int.TryParse(splv[0], out plv1);
                            }
                            if (splv.Length > 1)
                            {
                                int.TryParse(splv[1], out plv2);
                            }
                        }
                        if (plv1 > 0 && lv >= plv1 && (plv2 <= 0 || lv <= plv2))
                        {
                            if (lv == plv1 || lv == plv2)
                            {
                                return true;
                            }
                            else
                            {
                                if (int.TryParse(dr[1], out step))
                                {
                                    if (lv % step == 0)
                                    {
                                        return true;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
            return false;
        }

        public void mapNewMediaSource(string data)
        {
            string[] map = data.Split(';');
            if (map != null)
            {
                for (int i = 0; i < map.Length; i++)
                {
                    string[] items = map[i].Split(',');
                    if (items != null && items.Length == 2)
                    {
                        if (items[0].Length > 0 && items[1].Length > 0)
                        {
                            if (dicMediaSource.ContainsKey(items[0]))
                            {
                                dicMediaSource[items[0]] = items[1];
                            }
                            else
                            {
                                dicMediaSource.Add(items[0], items[1]);
                            }
                        }
                    }
                }
            }
        }

        public bool checkSameSource(string sortSource)
        {
            if (sortSource != null && dicMediaSource.ContainsKey(sortSource))
            {
                string ss = dicMediaSource[sortSource];
                return SDKManager.Instance.mediaSource.EndsWith(ss);
            }
            return false;
        }
    }
}
