//#define FIRBASE_ENABLE
//#define FIR_DATABASE_E
//#define FIRESTORE_ENABLE
//#define CheckLogAdLoad
#define ENABLE_LOG_ADMOB_REVENUE
#define ENABLE_FIRHELPER_LOG_CLIENT

//#define ENABLE_GETCONFIG
//#define ENABLE_GETFCM_TOKEN

//#define ENABLE_CHECKAND

#define ENABLE_LOG_UNITY
//#define ENABLE_LOG_FB

using System;
using System.Collections;
using System.Collections.Generic;
using Myapi;
using MyJson;
using UnityEngine;
using UnityEngine.Analytics;
#if FIRBASE_ENABLE
using Firebase;
using Firebase.Extensions;
using Firebase.Analytics;
using Firebase.RemoteConfig;
using Firebase.Messaging;

#if FIR_DATABASE_E
using Firebase.Database;

#endif

#elif ENABLE_GETCONFIG
using Firebase.RemoteConfig;
#endif

#if UNITY_ANDROID
using UnityEngine.Android;
#endif



namespace mygame.sdk
{
    public partial class FIRhelper : MonoBehaviour
    {
        public static FIRhelper Instance { get; private set; }
        public static event Action CBGetconfig = null;

        public int idxOther { get; private set; }

        public int statusInitFir = 0;
        private static int isFetchConfig = 0;

        //private bool statelogData = false;
        public string playerName { get; set; }
        public string gcm_id { get; set; }
        public int countRewardAd4heart;
        private PromoGameOb gamePromoCurr = null;
        public long tMemPromo4Native = -1;
        private PromoGameOb gameNtPromoCurr = null;
        private static List<QueueLogFir> listWaitLog = new List<QueueLogFir>();

        public static long AdmobRevenewDivide = 1000000000;
        public static int isLogAdmobRevenueAppsFlyer = 1;
        public static int isLogAdmobRevenueAppsFlyerinMe = 0;
        static int perPostAdVa = 100;
        public static int flagLog4CheckErr = 0;

        public static float perPostAdsNtSplash = 0.05f;
        public static float perPostAdsNtFull2 = 0.5f;

        public static float ValueDailyAdRevenew = 0.04f;
        public static float[] TroasTier = { 0.1f, 0.04f, 0.01f };
        public static int IdxTier = 1;
        private static string GroupTier1Code = ",AT,AU,CA,CH,DE,DK,FR,GB,JP,KR,NO,NZ,SE,TW,US,";
        private static string GroupTier2Code = ",AE,BE,CN,CZ,ES,FI,HK,IE,IL,IS,IT,KW,LU,NL,PL,PR,SA,ZA,";

        public static float[] AdTopThreshold = { 0.047f, 0.066f, 0.097f, 0.135f, 0.234f };
        public long t4tLoadrm = 0;
        private long tStartFetch = 0;
        public byte isAdSkip = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                gcm_id = PlayerPrefs.GetString("get_gcm_id", "");
                isLogAdmobRevenueAppsFlyer = PlayerPrefs.GetInt("is_log_admob_va2af", 1);
                isLogAdmobRevenueAppsFlyerinMe = PlayerPrefs.GetInt("is_log_admob_va2af_me", 0);

                perPostAdsNtSplash = PlayerPrefs.GetFloat("cf_post_ad_ntsplash", AppConfig.PerPostNtSplash);
                perPostAdsNtFull2 = PlayerPrefs.GetFloat("cf_post_ad_ntfull2", AppConfig.PerPostNt2);

                flagLog4CheckErr = PlayerPrefs.GetInt("flag_log_4check_err", 0);
                AdmobRevenewDivide = PlayerPrefs.GetInt("ad_va_divide", 1000000000);
                float vatroas = PlayerPrefs.GetInt("cf_threshold_troas", 40);
                ValueDailyAdRevenew = vatroas / 10000.0f;
                string stroasGroup = PlayerPrefs.GetString("cf_threshold_troasgr", "1000,40,10");
                parThresholdTroas(stroasGroup);
                string stroasTop = PlayerPrefs.GetString("cf_threshold_top", "470,660,920,1350,2340");
                parThresholdTop(stroasTop);

#if ENABLE_LOG_UNITY
                Dictionary<string, object> dicparam = new Dictionary<string, object>();
                dicparam.Add("Login", 1);
                Analytics.CustomEvent("Login", dicparam);
#endif
                checkOvertimePromoClick();
                listWaitLog.Clear();
                t4tLoadrm = SdkUtil.CurrentTimeMilis();
            }
        }

        // Use this for initialization 
        private void Start()
        {
            Debug.Log("mysdk: fir Start 0");
            int nranc = UnityEngine.Random.Range(5, 15);
            Invoke("checkand", nranc);
            nranc = UnityEngine.Random.Range(5, 15);
            Invoke("checkCoty", nranc);
            checkUpdate();

#if FIRBASE_ENABLE
            statusInitFir = 0;
            isFetchConfig = 0;
            perPostAdVa = PlayerPrefs.GetInt("mem_va_of_ad_postfir", AppConfig.PerValuePostFir);

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    var app = FirebaseApp.DefaultInstance;
                    //Debug.Log($"mysdk: fir Firebase is ready.");
                    InitializeFirebase();
                }
                else
                {
                    Debug.Log($"mysdk: fir Could not resolve all Firebase dependencies: " + dependencyStatus);
                }
            });
            requestPermissionNoti();
#endif
        }

        private void requestPermissionNoti()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (AndroidVersion() >= 13 && !Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                long treq = PlayerPrefs.GetInt("mem_t_req_noti", 0);
                long tcurr = SdkUtil.CurrentTimeMilis() / 1000;
                int countn = PlayerPrefs.GetInt("mem_count_req_noti", 0);
                int k = 1;
                if (countn > 1 && countn < 4)
                {
                    k = 2;
                }
                else if (countn >= 4)
                {
                    k = 5;
                }
                if ((tcurr - treq) >= k * 86400)
                {
                    PlayerPrefs.SetInt("mem_t_req_noti", (int)tcurr);
                    PlayerPrefs.SetInt("mem_count_req_noti", countn++);
                    Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
                }
            }
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        int AndroidVersion()
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
        }
#endif

        private void checkUpdate()
        {
            int ver = PlayerPrefs.GetInt("update_ver", AppConfig.verapp);
            int ss = PlayerPrefs.GetInt("update_status", 0);
            int cfday = PlayerPrefs.GetInt("update_day_cf", 0);
            string gameid = PlayerPrefs.GetString("update_gameid", AppConfig.appid);
            string link = PlayerPrefs.GetString("update_link", "");
            string title = PlayerPrefs.GetString("update_title", "");
            string des = PlayerPrefs.GetString("update_des", "");
            int memverShow = PlayerPrefs.GetInt("update_ver_show", 0);
            int dayInstallGame = PlayerPrefs.GetInt("update_day_install", -1);
            int curday = DateTime.Now.Year * 365 + DateTime.Now.DayOfYear;
            int countday = 0;
            if (dayInstallGame < 0)
            {
                PlayerPrefs.SetInt("update_day_install", curday);
            }
            else
            {
                countday = curday - dayInstallGame;
            }
            if (ver >= AppConfig.verapp && countday >= cfday)
            {
                if (ss == 1)
                {
                    long tcurr = GameHelper.CurrentTimeMilisReal() / 60000;
                    int tmem = PlayerPrefs.GetInt("update_memtime_show", 0);
                    if (memverShow >= AppConfig.verapp && (tcurr - tmem) <= 1800)
                    {
                        ss = 0;
                    }
                }
                if (ss != 0 && (gameid.Length > 3 || link.Length > 10))
                {
                    SDKManager.Instance.showUpdate(ver, ss, gameid, link, title, des);
                    if (ss == 1)
                    {
                        PlayerPrefs.SetInt("update_status", 0);
                        PlayerPrefs.SetInt("update_ver_show", AppConfig.verapp);
                        long tshow = GameHelper.CurrentTimeMilisReal() / 60000;
                        PlayerPrefs.SetInt("update_memtime_show", (int)tshow);
                    }
                }
            }
        }

        private void checkand()
        {
#if UNITY_ANDROID && ENABLE_CHECKAND && !UNITY_EDITOR
                        /*
                         * for android
                         * fill package name to check my app
                        */
                        string pkgCheckAndroid = "com.money.run.hypercasual3d";
                        string pkg = Application.identifier;
                        if ((pkg == null || pkgCheckAndroid == null) || (string.CompareOrdinal(pkg, pkgCheckAndroid) != 0)) 
                        {
                            AdsHelper.Instance = null;
                            double aaaaa = 0.0f;
                            double aaaaa1 = 1234560.12334564764574f;
                            double aaaaa2 = 1234560.12334564764574f;
                            while (true) {
                                for (int i = 0; i < 1234567890; i++)
                                {
                                    aaaaa = aaaaa1 * aaaaa2;
                                    aaaaa = Math.Sqrt(aaaaa);
                                    if(i >= 1234567) {
                                        i = 0;
                                    }
                                }
                            }
                        }
#endif
        }

        public void checkcounty()
        {
            Invoke("checkCoty", 30);
        }

        public void checkCoty()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (GameHelper.Instance != null && GameHelper.Instance.countryCode != null && GameHelper.Instance.countryCode.CompareTo("cn") == 0)
            {
                AdsHelper.Instance = null;
                double aaaaa = 0.0f;
                double aaaaa1 = 1234560.12334564764574f;
                double aaaaa2 = 1234560.12334564764574f;
                while (true)
                {
                    for (int i = 0; i < 1234567890; i++)
                    {
                        aaaaa = aaaaa1 * aaaaa2;
                        aaaaa = Math.Sqrt(aaaaa);
                        if (i >= 1234567)
                        {
                            i = 0;
                        }
                    }
                }
            }
#endif
        }

        public void OnDestroy()
        {
            if (Instance != null && Instance == this)
            {
#if FIRBASE_ENABLE
                Firebase.Messaging.FirebaseMessaging.MessageReceived -= OnMessageReceived;
                Firebase.Messaging.FirebaseMessaging.TokenReceived -= OnTokenReceived;
#endif
            }
        }

        public static void checkGroupTier(string countrycode)
        {
            string upcase = countrycode.ToUpper();
            IdxTier = 1;
            if (GroupTier1Code.Contains(upcase))
            {
                IdxTier = 0;
            }
            else if (GroupTier2Code.Contains(upcase))
            {
                IdxTier = 1;
            }
            else
            {
                IdxTier = 2;
            }

            int va = PlayerPrefs.GetInt("cf_threshold_troas", 0);
            if (va <= 0)
            {
                ValueDailyAdRevenew = TroasTier[IdxTier];
                Debug.Log($"mysdk: fir check troas get from group IdxTroasTier={IdxTier} countrycode={upcase}");
            }
            else
            {
                Debug.Log($"mysdk: fir check troas has va from remote IdxTroasTier={IdxTier} countrycode={upcase}");
            }
        }

        public static void parThresholdTroas(string data)
        {
            try
            {
                string[] sl = data.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (sl != null && sl.Length >= 3)
                {
                    float va = int.Parse(sl[0]);
                    TroasTier[0] = va / 10000.0f;
                    va = int.Parse(sl[1]);
                    TroasTier[1] = va / 10000.0f;
                    va = int.Parse(sl[2]);
                    TroasTier[2] = va / 10000.0f;
                    Debug.Log($"mysdk: fir parThresholdGroup data={data}");
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void parThresholdTop(string data)
        {
            try
            {
                string[] sl = data.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (sl != null && sl.Length >= 5)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        float va = int.Parse(sl[i]);
                        AdTopThreshold[i] = va / 10000.0f;
                    }
                    Debug.Log($"mysdk: fir parThresholdTop data={data}");
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void SetActivePushNotificationsForUser(bool active)
        {
            PlayerPrefs.SetInt("is_enable_push_notification", active ? 1 : 0);

#if FIRBASE_ENABLE
            if (active)
            {
                // Đăng ký topic nếu cần
                FirebaseMessaging.SubscribeAsync("/topics/all").ContinueWith(task =>
                {
                    Debug.Log("Subscribed to notifications");
                });
            }
            else
            {
                // Hủy nhận thông báo
                FirebaseMessaging.UnsubscribeAsync("/topics/all").ContinueWith(task =>
                {
                    Debug.Log("Unsubscribed from notifications");
                });
            }
#endif
        }

        public void callUpdateGcm()
        {
#if ENABLE_SERVER_NOTIFICATION
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                StartCoroutine(waitUpdateGcm());
            });
#endif
        }
#if ENABLE_SERVER_NOTIFICATION
        IEnumerator waitUpdateGcm()
        {
            yield return new WaitForSeconds(5);
            PushGCMAPI.Instance.isLastPushSuccess = false;
            ServerHub.GetSendRequestServer<SendRequestUser>().RequestSendGCM(gcm_id);
                    
            PushGCMAPI.Instance.Push(gcm_id, ob =>
            {
                PushGCMAPI.Instance.isLastPushSuccess = true;
            });
        }
#endif

        public void InitializeFirebase()
        {
            Debug.Log($"mysdk: fir InitializeFirebase");
#if FIRBASE_ENABLE
            statusInitFir = 1;
#if FIR_DATABASE_E
#if UNITY_ANDROID
            myApp.SetEditorDatabaseUrl("https://run-clash-3d.firebaseio.com/");
#elif UNITY_IOS || UNITY_IPHONE
            myApp.SetEditorDatabaseUrl("https://run-clash-3d.firebaseio.com");
#endif
#endif
            //message
            Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
            Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
            Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;

            SetActivePushNotificationsForUser(PlayerPrefs.GetInt("is_enable_push_notification", 1) == 1);

            int a1 = PlayerPrefs.GetInt("mem_log_ver_ins", 0);
            if (a1 != 1)
            {
                PlayerPrefs.SetInt("mem_log_ver_ins", 1);
                if (SDKManager.Instance.verCodeInstall == AppConfig.verapp)
                {
                    logEvent($"ver_app_ins_{AppConfig.verapp}");
                }
            }

            fetchDataConfig();

            FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLogin);
            foreach (QueueLogFir memlog in listWaitLog)
            {
                if (memlog != null)
                {
                    if (memlog.extral == null || memlog.extral.Length < 2)
                    {
                        if (memlog.memParams != null && memlog.memParams.Count > 0)
                        {
                            Firebase.Analytics.FirebaseAnalytics.LogEvent(memlog.eventName, DicParamsToParams(memlog.memParams));
                            Debug.Log($"mysdk: fir log wait2 event=" + memlog.eventName);
                        }
                        else
                        {
                            FirebaseAnalytics.LogEvent(memlog.eventName);
                            Debug.Log($"mysdk: fir log wait1 event=" + memlog.eventName);
                        }
                    }
                    else
                    {
                        Firebase.Analytics.FirebaseAnalytics.SetUserProperty(memlog.eventName, memlog.extral);
                        Debug.Log($"mysdk: fir set property wait name={memlog.eventName} Property={memlog.extral}");
                    }
                }
            }
            listWaitLog.Clear();
#endif
        }
        private void fetchDataConfig()
        {
#if FIRBASE_ENABLE
            tStartFetch = SdkUtil.CurrentTimeMilis();
            logTime4Firebase("init", tStartFetch - t4tLoadrm);
            Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero).ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompleted)
                {
                    Debug.Log($"mysdk: fir FetchAsync Retrieval hasn't finished");
                    return;
                }
                fetchRemote();
            });
#if FIRESTORE_ENABLE
            Firebase.Firestore.CollectionReference adsIdCf = Firebase.Firestore.FirebaseFirestore.GetInstance("ads-config-ids").Collection("/ids");
            adsIdCf.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                long tcurr = SdkUtil.CurrentTimeMilis();
                logTime4Firebase("init_firstoreok", tcurr - t4tLoadrm);
                logTime4Firebase("store_firstoreok", tcurr - tStartFetch);
            });

#endif
#endif
        }
        private void logTime4Firebase(string key, long dt)
        {
            if (dt <= 1000)
            {
                logEvent($"fir_{key}_1");
            }
            else if (dt <= 2000)
            {
                logEvent($"fir_{key}_2");
            }
            else if (dt <= 3000)
            {
                logEvent($"fir_{key}_3");
            }
            else if (dt <= 4000)
            {
                logEvent($"fir_{key}_4");
            }
            else if (dt <= 5000)
            {
                logEvent($"fir_{key}_5");
            }
            else if (dt <= 6000)
            {
                logEvent($"fir_{key}_6");
            }
            else if (dt <= 7000)
            {
                logEvent($"fir_{key}_7");
            }
            else if (dt <= 8000)
            {
                logEvent($"fir_{key}_8");
            }
            else if (dt <= 9000)
            {
                logEvent($"fir_{key}_9");
            }
            else
            {
                logEvent($"fir_{key}_10");
            }
        }
#if FIRBASE_ENABLE || ENABLE_GETFCM_TOKEN

        private void fetchRemote()
        {
            Debug.Log($"mysdk: fir fetchRemote");
            var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
#if UNITY_ANDROID
            var info = remoteConfig.Info;
            if (info == null || info.LastFetchStatus != LastFetchStatus.Success)
            {
                Debug.Log($"mysdk: fir FetchAsync was unsuccessful {nameof(info.LastFetchStatus)}:{info.LastFetchStatus}");
                return;
            }
#endif
            remoteConfig.ActivateAsync().ContinueWithOnMainThread(taskac =>
            {
                Debug.Log($"mysdk: fir Remote data loaded and ready for use");
                long tcurr = SdkUtil.CurrentTimeMilis();
                logTime4Firebase("init_fetch", tcurr - t4tLoadrm);
                logTime4Firebase("rm_fetch", tcurr - tStartFetch);
                parserConfig();
                if (CBGetconfig != null)
                {
                    CBGetconfig();
                }
                Debug.Log($"mysdk: fir fetchRemote 2=" + remoteConfig.Info.LastFetchStatus);
            });
            Debug.Log($"mysdk: fir fetchRemote1");
        }

        public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
        {
#if UNITY_ANDROID
            AppsFlyerSDK.AppsFlyer.updateServerUninstallToken(token.Token);
#endif
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log($"mysdk: fir Token: " + token.Token);
                if (token != null && token.Token != null && token.Token.Length > 0)
                {
                    gcm_id = token.Token;

#if ENABLE_SERVER_NOTIFICATION
                    if (gcm_id != PlayerPrefs.GetString("get_gcm_id", "") || !PushGCMAPI.Instance.isLastPushSuccess)
                    {
                        callUpdateGcm();
                    }
#endif

                    PlayerPrefs.SetString("get_gcm_id", gcm_id);
                    if (PlayerPrefs.GetInt("mem_send_gcm_ok", 0) != 1)
                    {
                        //callUpdateGcm();
                    }
                }
            });
        }
#endif
#if FIRBASE_ENABLE

        public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                SdkUtil.logd("fir OnMessageReceived a new message");
                if (e.Message.From.Length > 0)
                    SdkUtil.logd("fir OnMessageReceived from: " + e.Message.RawData);
                if (e.Message.Data.Count > 0)
                {
                    SdkUtil.logd("fir OnMessageReceived data:");
                    foreach (System.Collections.Generic.KeyValuePair<string, string> iter in
                        e.Message.Data)
                    {
                        SdkUtil.logd("fir OnMessageReceived " + iter.Key + ": " + iter.Value);
#if UNITY_IOS || UNITY_IPHONE
                        if (iter.Key.Equals("gift_box"))
                        {
                            PlayerPrefs.SetString("mem_gift_box_ios", iter.Value);
                        }
#endif
                    }
                }
            });
        }
#endif

        public static void setUserProperty(string name, string property)
        {
#if FIRBASE_ENABLE
            if (Instance != null && Instance.statusInitFir == 1)
            {
                Firebase.Analytics.FirebaseAnalytics.SetUserProperty(name, property);
            }
            else
            {
                QueueLogFir mem = new QueueLogFir(name);
                mem.extral = property;
                listWaitLog.Add(mem);
            }
#endif
        }

        public static void logEvent(string eventName, Dictionary<string, object> dicParams = null)
        {
#if FIRBASE_ENABLE
            _logEvent(eventName, dicParams);
#endif

#if ENABLE_LOG_FB
            FBHelper.Instance.logEvent(name, parameter, value);
#endif

#if ENABLE_LOG_UNITY
            Analytics.CustomEvent(eventName, dicParams);
#endif
        }
#if FIRBASE_ENABLE
        private static Parameter[] DicParamsToParams(Dictionary<string, object> dicParams)
        {
            Parameter[] prams = null;
            if (dicParams != null && dicParams.Count > 0)
            {
                prams = new Parameter[dicParams.Count];
                int idx = 0;
                foreach (var item in dicParams)
                {
                    if (item.Value is double)
                    {
                        prams[idx] = new Parameter(item.Key, (double)item.Value);
                    }
                    else if (item.Value is float)
                    {
                        prams[idx] = new Parameter(item.Key, (float)item.Value);
                    }
                    else if (item.Value is int)
                    {
                        prams[idx] = new Parameter(item.Key, (int)item.Value);
                    }
                    else if (item.Value is long)
                    {
                        prams[idx] = new Parameter(item.Key, (long)item.Value);
                    }
                    else
                    {
                        prams[idx] = new Parameter(item.Key, item.Value.ToString());
                    }
                    idx++;
                }
            }
            return prams;
        }
        private static void _logEvent(string eventName, Dictionary<string, object> eventParams)
        {
            if (Instance != null && Instance.statusInitFir == 1)
            {
                if (eventParams != null && eventParams.Count > 0)
                {
                    Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, DicParamsToParams(eventParams));
                }
                else
                {
                    Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);
                }
#if ENABLE_FIRHELPER_LOG_CLIENT
                Debug.Log("mysdk: fir log event=" + eventName);
#endif
            }
            else
            {
                QueueLogFir mem = new QueueLogFir(eventName);
                mem.memParams = eventParams;
                listWaitLog.Add(mem);
                Debug.Log("mysdk: fir memlog=" + eventName);
            }
        }
#endif

#if CheckLogAdLoad
        static int loadCount = 0;
        static int loadReCount = 0;
#endif
        public static void logAdEvent(string eventName, Dictionary<string, object> dicParams = null)
        {
#if CheckLogAdLoad
            if (eventName.EndsWith("_load"))
            {
                loadCount++;
            } 
            else if (eventName.Contains("_load_"))
            {
                loadReCount++;
            }
#endif
#if FIRBASE_ENABLE
            _logEvent(eventName, dicParams);
#endif
        }
        public static void logEvent4CheckErr(string eventName)
        {
            if (flagLog4CheckErr != 101)
            {
                return;
            }
#if FIRBASE_ENABLE
            _logEvent(eventName, null);
#endif
        }

        //int typeAds 0-bn, 1-full, 2-gift
        public static void logEventAdsPaidAdmob(string placement, string adformat, string adSource, string adunitId, long vaMicros, long originVaM, string currencyCode)
        {
            long postMicros = vaMicros * perPostAdVa / 100;
            double postdouble = ((double)postMicros) / ((double)AdmobRevenewDivide);
            Debug.Log($"mysdk: fir logEventAdsPaidAdmob placement={placement} adformat={adformat} adsource={adSource} adunitId={adunitId} perpost={perPostAdVa} vaMicros={vaMicros} originMicros={originVaM}, currencyCode={currencyCode}");
            if (isLogAdmobRevenueAppsFlyer == 1)
            {
                AppsFlyerHelperScript.logAdRevenue(placement, AFMediationNetwork.GoogleAdMob, adSource, adformat, adunitId, "", postdouble, currencyCode);
            }
            else
            {
                Debug.Log($"mysdk: fir logEventAdsPaidAdmob not post to AF");
            }

#if FIRBASE_ENABLE && ENABLE_LOG_ADMOB_REVENUE
            double ltv = calculatorDaily4Troas(1, postMicros / ((double)AdmobRevenewDivide));
            float newva = postMicros;
            float div = AdmobRevenewDivide;
            newva = newva / div;

            Dictionary<string, object> paramspaid = getParamAdrevenue("admob", adSource, adunitId, adformat, postdouble, currencyCode);
            logEventAdsPaid(paramspaid, ltv, newva);
#endif
        }
        public static string getAdformatAdmob(int typeAds)
        {
            string adformat;
            if (typeAds == 0)
            {
                adformat = "banner";
            }
            else if (typeAds == 1)
            {
                adformat = "banner_collapse";
            }
            else if (typeAds == 2)
            {
                adformat = "banner_rect";
            }
            else if (typeAds == 3)
            {
                adformat = "native_full";
            }
            else if (typeAds == 4)
            {
                adformat = "interstitial";
            }
            else if (typeAds == 5)
            {
                adformat = "rewarded";
            }
            else if (typeAds == 6)
            {
                adformat = "native_collapse";
            }
            else if (typeAds == 7)
            {
                adformat = "openad";
            }
            else if (typeAds == 8)
            {
                adformat = "native_rect";
            }
            else if (typeAds == 9)
            {
                adformat = "native_full";
            }
            else if (typeAds == 10)
            {
                adformat = "native_banner";
            }
            else if (typeAds == 11)
            {
                adformat = "rewarded_interstitial";
            }
            else if (typeAds == 12)
            {
                adformat = "native_custome";
            }
            else
            {
                adformat = $"unknow_{typeAds}";
            }
            return adformat;
        }
        public static string getAdsourceAdmob(string source)
        {
            string adSource;
            if (source != null)
            {
                adSource = source.ToLower();
                if (adSource.Contains("pangle"))
                {
                    adSource = "pangle";
                }
                else if (adSource.Contains("applovin"))
                {
                    adSource = "applovin";
                }
                else if (adSource.Contains("chartboost"))
                {
                    adSource = "chartboost";
                }
                else if (adSource.Contains("facebook"))
                {
                    adSource = "facebook";
                }
                else if (adSource.Contains("fyber"))
                {
                    adSource = "fyber";
                }
                else if (adSource.Contains("mytarget"))
                {
                    adSource = "mytarget";
                }
                else if (adSource.Contains("unity"))
                {
                    adSource = "unity";
                }
                else if (adSource.Contains("vungle"))
                {
                    adSource = "vungle";
                }
                else if (adSource.Contains("ironsource"))
                {
                    adSource = "ironsource";
                }
                else if (adSource.Contains("mintegral"))
                {
                    adSource = "mintegral";
                }
                else if (adSource.Contains("google ad manager"))
                {
                    adSource = "gam";
                }
                else if (adSource.Contains("admob"))
                {
                    adSource = "admob";
                }
            }
            else
            {
                adSource = "adsource_null";
            }
            return adSource;
        }

        public static void logEventAdsPaidFb(string placement, string adformat, string adunitId, double rrevenue)
        {
            double newrevenue = rrevenue * perPostAdVa / 100.0f;

            adformat = adformat.ToLower();
            Debug.Log($"mysdk: fir logEventAdsPaidFb placement={placement} adformat={adformat} adunitId={adunitId} postValue={newrevenue} adva={rrevenue}");
            AppsFlyerHelperScript.logAdRevenue(placement, AFMediationNetwork.Custom, "facebook", adformat, adunitId, "", newrevenue, "USD");

#if FIRBASE_ENABLE
            double ltv = calculatorDaily4Troas(2, newrevenue);
            Dictionary<string, object> paramspaid = getParamAdrevenue("facebook", "facebook", adunitId, adformat, newrevenue, "USD");
            logEventAdsPaid(paramspaid, ltv, newrevenue);
#endif
        }

        public static void logEventAdsPaidMax(string placement, string adformat, string adSource, string adunitId, double rrevenue, string adUnitIdentifier, string countrycode)
        {
            double newrevenue = rrevenue * perPostAdVa / 100.0f;

            adformat = adformat.ToLower();
            Debug.Log($"mysdk: fir logEventAdsPaidMax placement={placement} adformat={adformat} adsource={adSource} adunitId={adunitId} postValue={newrevenue} adva={rrevenue} adUnitIdentifier={adUnitIdentifier} countrycode={countrycode}");
            AppsFlyerHelperScript.logAdRevenue(placement, AFMediationNetwork.ApplovinMax, adSource, adformat, adunitId, countrycode, newrevenue, "USD");
            if (isLogAdmobRevenueAppsFlyerinMe == 1 && (adSource.Contains("google") || adSource.Contains("admob")))
            {
                AppsFlyerHelperScript.logAdRevenue(placement, AFMediationNetwork.GoogleAdMob, adSource, adformat, adunitId, countrycode, newrevenue, "USD");
            }

#if FIRBASE_ENABLE
            double ltv = calculatorDaily4Troas(3, newrevenue);
            Dictionary<string, object> paramspaid = getParamAdrevenue("AppLovin", adSource, adunitId, adformat, newrevenue, "USD");
            logEventAdsPaid(paramspaid, ltv, newrevenue);
#endif
        }
        public static string getAdsourceMax(string source)
        {
            string adSource;
            if (source != null)
            {
                adSource = source.ToLower();
                if (adSource.Contains("pangle"))
                {
                    adSource = "pangle";
                }
                else if (adSource.Contains("applovin") || adSource.Contains("applovin_exchange"))
                {
                    adSource = "applovin";
                }
                else if (adSource.Contains("chartboost"))
                {
                    adSource = "chartboost";
                }
                else if (adSource.Contains("facebook"))
                {
                    adSource = "facebook";
                }
                else if (adSource.Contains("fyber") || adSource.Contains("dt exchange"))
                {
                    adSource = "fyber";
                }
                else if (adSource.Contains("mytarget") || adSource.Contains("vk ad network"))
                {
                    adSource = "mytarget";
                }
                else if (adSource.Contains("unity"))
                {
                    adSource = "unity";
                }
                else if (adSource.Contains("vungle") || adSource.Contains("liftoff"))
                {
                    adSource = "vungle";
                }
                else if (adSource.Contains("ironsource"))
                {
                    adSource = "ironsource";
                }
                else if (adSource.Contains("mintegral"))
                {
                    adSource = "mintegral";
                }
                else if (adSource.Contains("google ad manager"))
                {
                    adSource = "gam";
                }
                else if (adSource.Contains("admob"))
                {
                    adSource = "admob";
                }
                else if (adSource.Contains("bigo") || adSource.Contains("bıgo"))
                {
                    adSource = "bigo";
                }
            }
            else
            {
                adSource = "adsource_null";
            }
            return adSource;
        }

        public static void logEventAdsPaidIron(string placement, string adformat, string adSource, string adunitId, double rrevenue, string countrycode)
        {
            double newrevenue = rrevenue * perPostAdVa / 100.0f;
            Debug.Log($"mysdk: fir logEventAdsPaidIron placement={placement} adformat={adformat} adsource={adSource} adunitId={adunitId} postValue={newrevenue} adva={rrevenue}, countrycode={countrycode}");
            AppsFlyerHelperScript.logAdRevenue(placement, AFMediationNetwork.IronSource, adSource, adformat, adunitId, countrycode, newrevenue, "USD");
            if (isLogAdmobRevenueAppsFlyerinMe == 1 && (adSource.Contains("google") || adSource.Contains("admob")))
            {
                AppsFlyerHelperScript.logAdRevenue(placement, AFMediationNetwork.GoogleAdMob, adSource, adformat, adunitId, countrycode, newrevenue, "USD");
            }

#if FIRBASE_ENABLE
            double ltv = calculatorDaily4Troas(4, newrevenue);
            Dictionary<string, object> paramspaid = getParamAdrevenue("ironSource", adSource, adunitId, adformat, newrevenue, "USD");
            logEventAdsPaid(paramspaid, ltv, newrevenue);
#endif
        }
        public static string getAdsourceIron(string source)
        {
            string adSource;
            if (source != null)
            {
                adSource = source.ToLower();
                if (adSource.Contains("pangle"))
                {
                    adSource = "pangle";
                }
                else if (adSource.Contains("applovin") || adSource.Contains("applovin_exchange"))
                {
                    adSource = "applovin";
                }
                else if (adSource.Contains("chartboost"))
                {
                    adSource = "chartboost";
                }
                else if (adSource.Contains("facebook"))
                {
                    adSource = "facebook";
                }
                else if (adSource.Contains("fyber") || adSource.Contains("dt exchange"))
                {
                    adSource = "fyber";
                }
                else if (adSource.Contains("mytarget") || adSource.Contains("vk ad network"))
                {
                    adSource = "mytarget";
                }
                else if (adSource.Contains("unity"))
                {
                    adSource = "unity";
                }
                else if (adSource.Contains("vungle") || adSource.Contains("liftoff"))
                {
                    adSource = "vungle";
                }
                else if (adSource.Contains("ironsource"))
                {
                    adSource = "ironsource";
                }
                else if (adSource.Contains("mintegral"))
                {
                    adSource = "mintegral";
                }
                else if (adSource.Contains("google ad manager"))
                {
                    adSource = "gam";
                }
                else if (adSource.Contains("admob"))
                {
                    adSource = "admob";
                }
                else if (adSource.Contains("bigo") || adSource.Contains("bıgo"))
                {
                    adSource = "bigo";
                }
            }
            else
            {
                adSource = "adsource_null";
            }
            return adSource;
        }
#if FIRBASE_ENABLE
        static Dictionary<string, object> getParamAdrevenue(string adPlatform, string adSource, string adunitId, string adformat, double revenue, string currency)
        {
            Dictionary<string, object> paramspaid = new Dictionary<string, object>();
            paramspaid.Add("ad_platform", adPlatform);
            paramspaid.Add("ad_format", adformat);
            paramspaid.Add("ad_source", adSource);
            paramspaid.Add("ad_unit_name", adunitId);
            paramspaid.Add("value", revenue);
            paramspaid.Add("currency", currency);

            return paramspaid;
        }

        static void logEventAdsPaid(Dictionary<string, object> paramspaid, double ltv, double newVa)
        {
            logEvent("ad_impression", paramspaid);
            if (ltv >= ValueDailyAdRevenew)
            {
                logEventTroasAdrevenue(ltv);
            }

            float preva = PlayerPrefs.GetFloat("mem_tcpa_ltv_top", 0);
            float currva = preva + (float)newVa;
            PlayerPrefs.SetFloat("mem_tcpa_ltv_top", currva);
            logCustomTcpa(preva, currva);
        }

        static double calculatorDaily4Troas(int typeAd, double newrevenue)
        {
            double admobLtv = PlayerPrefs.GetFloat("mem_paid_admob_ltv", 0);
            double fbLtv = PlayerPrefs.GetFloat("mem_paid_fb_ltv", 0);
            double maxLtv = PlayerPrefs.GetFloat("mem_paid_max_ltv", 0);
            double ironLtv = PlayerPrefs.GetFloat("mem_paid_iron_ltv", 0);
            // move old type value to new type value
            // old value is nano value
            // new value is real
            SdkUtil.logd($"fir calculatorDaily4Troas typeAd={typeAd} newrevenue={newrevenue} admobLtv={admobLtv}");
            if (admobLtv >= 1)
            {
                admobLtv = admobLtv / ((double)AdmobRevenewDivide);
                SdkUtil.logd($"fir calculatorDaily4Troas typeAd={typeAd} after admobLtv={admobLtv}");
            }
            double ltv = newrevenue + admobLtv + fbLtv + maxLtv + ironLtv;
            if (ltv >= ValueDailyAdRevenew)
            {
                PlayerPrefs.SetFloat("mem_paid_max_ltv", 0);
                PlayerPrefs.SetFloat("mem_paid_admob_ltv", 0);
                PlayerPrefs.SetFloat("mem_paid_iron_ltv", 0);
                PlayerPrefs.SetFloat("mem_paid_fb_ltv", 0);
            }
            else
            {
                string keycur = "";
                double calVa = 0;
                if (typeAd == 1)//admob
                {
                    keycur = "mem_paid_admob_ltv";
                    calVa = admobLtv;
                }
                else if (typeAd == 2)//fb
                {
                    keycur = "mem_paid_fb_ltv";
                    calVa = fbLtv;
                }
                else if (typeAd == 3)//max
                {
                    keycur = "mem_paid_max_ltv";
                    calVa = maxLtv;
                }
                else if (typeAd == 4)//ir
                {
                    keycur = "mem_paid_iron_ltv";
                    calVa = ironLtv;
                }
                PlayerPrefs.SetFloat(keycur, (float)(calVa + newrevenue));
            }
            SdkUtil.logd($"fir calculatorDaily4Troas typeAd={typeAd} newrevenue={newrevenue} ltv={ltv}");
            return ltv;
        }

        static void logEventTroasAdrevenue(double ltv)
        {
            Dictionary<string, object> paramspaid = new Dictionary<string, object>();
            paramspaid.Add("value", ltv);
            paramspaid.Add("currency", "USD");
            logEvent("Daily_Ads_Revenue", paramspaid);
            string evname = $"Daily_Ads_Revenue_{IdxTier + 1}";
            logEvent(evname, paramspaid);
        }

        static void logCustomTcpa(float preva, float currVa)
        {
            for (int i = 0; i < 5; i++)
            {
                if (preva < AdTopThreshold[i] && currVa >= AdTopThreshold[i])
                {
                    Dictionary<string, object> paramspaid = new Dictionary<string, object>();
                    paramspaid.Add("value", AdTopThreshold[i]);
                    paramspaid.Add("currency", "USD");
                    string evname = "";
                    switch (i)
                    {
                        case 0:
                            evname = "AdLTV_OnDay_Top50Peercent";
                            break;
                        case 1:
                            evname = "AdLTV_OnDay_Top40Peercent";
                            break;
                        case 2:
                            evname = "AdLTV_OnDay_Top30Peercent";
                            break;
                        case 3:
                            evname = "AdLTV_OnDay_Top20Peercent";
                            break;
                        default:
                            evname = "AdLTV_OnDay_Top10Peercent";
                            break;
                    }
                    logEvent(evname, paramspaid);

#if ENABLE_AppsFlyer
                    //Dictionary<string, string> appsParams = new Dictionary<string, string>();
                    //appsParams.Add(AFInAppEvents.REVENUE, AdTopThreshold[i].ToString(System.Globalization.CultureInfo.InvariantCulture));
                    //AppsFlyerHelperScript.logEvent(evname, appsParams);
                    AppsFlyerHelperScript.logEvent(evname);
#endif
                }
            }
        }
#endif

        public PromoGameOb getGameHasIcon()
        {
            string memlistgame = PlayerPrefs.GetString("cf_game_promo", "");
            string memclick = PlayerPrefs.GetString("mem_game_promo_click", "");
            if (memlistgame != null && memlistgame.Length > 10)
            {
                var obmemgames = (IDictionary<string, object>)JsonDecoder.DecodeText(memlistgame);
                List<object> listmemgames = null;
                if (obmemgames != null && obmemgames.ContainsKey("games"))
                {
                    listmemgames = (List<object>)obmemgames["games"];
                }
                if (listmemgames != null)
                {
                    for (int i = 0; i < listmemgames.Count; i++)
                    {
                        var gamepromo = (IDictionary<string, object>)listmemgames[i];
                        string pkgcuu = (string)gamepromo["pkg"];
                        if (!memclick.Contains(pkgcuu))
                        {
                            string linkicon = (string)gamepromo["icon"];
                            string nameimg = ImageLoader.url2nameData(linkicon, 1);
                            if (System.IO.File.Exists(DownLoadUtil.pathCache() + "/" + nameimg))
                            {
                                PromoGameOb tmpCurr = PromoGameOb.newFromData(gamepromo);
                                return tmpCurr;
                            }
                        }
                    }
                    for (int i = 0; i < listmemgames.Count; i++)
                    {
                        var gamepromo = (IDictionary<string, object>)listmemgames[i];
                        string pkgcuu = (string)gamepromo["pkg"];
                        string linkicon = (string)gamepromo["icon"];
                        string nameimg = ImageLoader.url2nameData(linkicon, 1);
                        if (System.IO.File.Exists(DownLoadUtil.pathCache() + "/" + nameimg))
                        {
                            PromoGameOb tmpCurr = PromoGameOb.newFromData(gamepromo);
                            return tmpCurr;
                        }
                    }
                }
            }
            return null;
        }
        public PromoGameOb getGamePromo()
        {
            if (gamePromoCurr != null)
            {
                return gamePromoCurr;
            }
            else
            {
                string memlistgame = PlayerPrefs.GetString("cf_game_promo", "");
                string memclick = PlayerPrefs.GetString("mem_game_promo_click", "");
                if (memlistgame != null && memlistgame.Length > 10)
                {
                    var obmemgames = (IDictionary<string, object>)JsonDecoder.DecodeText(memlistgame);
                    List<object> listmemgames = null;
                    if (obmemgames != null && obmemgames.ContainsKey("games"))
                    {
                        listmemgames = (List<object>)obmemgames["games"];
                    }
                    if (listmemgames != null)
                    {
                        string pkggameWillPromo = PlayerPrefs.GetString("mem_game_will_promo", "");
                        PromoGameOb tmpCurr = null;
                        for (int i = 0; i < listmemgames.Count; i++)
                        {
                            var gamepromo = (IDictionary<string, object>)listmemgames[i];
                            string pkgcuu = (string)gamepromo["pkg"];
                            if (!memclick.Contains(pkgcuu))
                            {
                                if (pkggameWillPromo == null || pkggameWillPromo.Length < 5)
                                {
                                    gamePromoCurr = PromoGameOb.newFromData(gamepromo);
                                }
                                else
                                {
                                    if (pkgcuu.Equals(pkggameWillPromo))
                                    {
                                        gamePromoCurr = PromoGameOb.newFromData(gamepromo);
                                    }
                                    else if (tmpCurr == null)
                                    {
                                        tmpCurr = PromoGameOb.newFromData(gamepromo);
                                    }
                                }
                                if (listmemgames.Count > 1 && gamePromoCurr != null)
                                {
                                    int idxnex = i;
                                    for (int pp = 0; pp < listmemgames.Count; pp++)
                                    {
                                        idxnex++;
                                        if (idxnex >= listmemgames.Count)
                                        {
                                            idxnex = 0;
                                        }
                                        gamepromo = (IDictionary<string, object>)listmemgames[idxnex];
                                        pkgcuu = (string)gamepromo["pkg"];
                                        if (!memclick.Contains(pkgcuu))
                                        {
                                            PlayerPrefs.SetString("mem_game_will_promo", pkgcuu);
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        if (gamePromoCurr == null && tmpCurr != null)
                        {
                            gamePromoCurr = tmpCurr;
                        }
                        if (gamePromoCurr != null)
                        {
                            downLoadIconPromoGame(listmemgames, gamePromoCurr.pkg);
                        }
                    }
                }
                else
                {
                    gamePromoCurr = null;
                }
            }

            return gamePromoCurr;
        }
        public PromoGameOb nextGamePromo()
        {
            gamePromoCurr = null;
            return getGamePromo();
        }
        private void downLoadIconPromoGame(List<object> listmemgames, string pkgstart)
        {
            if (listmemgames != null && listmemgames.Count > 0)
            {
                int idxDown = -1;
                if (pkgstart == null || pkgstart.Length == 0)
                {
                    idxDown = 0;
                }
                else
                {
                    for (int i = 0; i < listmemgames.Count; i++)
                    {
                        var gamepromo = (IDictionary<string, object>)listmemgames[i];
                        string pkgcuu = (string)gamepromo["pkg"];
                        if (pkgstart.CompareTo(pkgcuu) == 0)
                        {
                            idxDown = i;
                            break;
                        }
                    }
                }
                for (int i = 0; i < 5; i++)
                {
                    var gamepromo = (IDictionary<string, object>)listmemgames[idxDown];
                    idxDown++;
                    if (idxDown >= listmemgames.Count)
                    {
                        idxDown = 0;
                    }

                    string linkicon = (string)gamepromo["icon"];
                    string nameimg = ImageLoader.url2nameData(linkicon, 1);
                    if (!System.IO.File.Exists(DownLoadUtil.pathCache() + "/" + nameimg))
                    {
                        ImageLoader.loadImageTexture(linkicon, 100, 100, null);
                    }
                }
            }
        }
        public void onClickPromoGame(PromoGameOb game)
        {
            if (game != null)
            {
                string memclick = PlayerPrefs.GetString("mem_game_promo_click", "");
                memclick += (";" + game.pkg + "," + GameHelper.CurrentTimeMilisReal());
                PlayerPrefs.SetString("mem_game_promo_click", memclick);
                logEvent("click_promo");

#if ENABLE_AppsFlyer
                Dictionary<string, string> ParamPromo = new Dictionary<string, string>();
                ParamPromo.Add("af_promo_appid", AppConfig.appid);
                AppsFlyerSDK.AppsFlyer.attributeAndOpenStore(game.appid, "cross_promo_campaign", ParamPromo, mygame.sdk.AppsFlyerHelperScript.Instance);
#endif

                if (gamePromoCurr == null || (gamePromoCurr != null && game.name.CompareTo(gamePromoCurr.name) == 0))
                {
                    gamePromoCurr = null;
                    getGamePromo();
                }
                if (game.link != null && game.link.Length > 10)
                {
                    GameHelper.Instance.gotoLink(game.link);
                }
                else
                {
#if UNITY_IOS || UNITY_IPHONE
                    GameHelper.Instance.gotoStore(game.appid);
#else
                    GameHelper.Instance.gotoStore(game.pkg);
#endif
                }
            }
        }
        private void checkOvertimePromoClick()
        {
            string memclick = PlayerPrefs.GetString("mem_game_promo_click", "");
            string[] listmemclick = memclick.Split(new char[] { ';' });
            memclick = "";
            bool isovettime = false;
            for (int i = 0; i < listmemclick.Length; i++)
            {
                string[] gameclick = listmemclick[i].Split(new char[] { ',' });
                if (gameclick != null && gameclick.Length == 2)
                {
                    long tcurr = GameHelper.CurrentTimeMilisReal();
                    long tmem = long.Parse(gameclick[1]);
                    if ((tcurr - tmem) > (24 * 60 * 60000))
                    {
                        isovettime = true;
                    }
                    else
                    {
                        if (memclick.Length == 0)
                        {
                            memclick = listmemclick[i];
                        }
                        else
                        {
                            memclick += (";" + listmemclick[i]);
                        }
                    }
                }
            }
            if (isovettime)
            {
                PlayerPrefs.SetString("mem_game_promo_click", memclick);
            }
        }
        public List<PromoGameOb> getListGamePromo(int num)
        {
            string memlistgame = PlayerPrefs.GetString("cf_game_promo", "");
            string memclick = PlayerPrefs.GetString("mem_game_promo_click", "");
            if (memlistgame != null && memlistgame.Length > 10)
            {
                var obmemgames = (IDictionary<string, object>)JsonDecoder.DecodeText(memlistgame);
                List<object> listmemgames = null;
                List<PromoGameOb> listTmp = null;
                List<PromoGameOb> re = new List<PromoGameOb>();
                if (obmemgames != null && obmemgames.ContainsKey("games"))
                {
                    listmemgames = (List<object>)obmemgames["games"];
                    listTmp = new List<PromoGameOb>();
                }
                if (listmemgames != null)
                {
                    int idxproed = PlayerPrefs.GetInt("mem_idx_promo_ed", 0);
                    if (idxproed < 0)
                    {
                        idxproed = 0;
                    }
                    else if (idxproed >= listmemgames.Count)
                    {
                        idxproed = 0;
                    }
                    Debug.Log($"mysdk: fir getListGamePromo idxproed={idxproed}");
                    int idx = idxproed;
                    for (int run = 0; run < listmemgames.Count; run++)
                    {
                        var gamepromo = (IDictionary<string, object>)listmemgames[idx];
                        var gametmp = new PromoGameOb();
                        gametmp.pkg = (string)gamepromo["pkg"];
                        gametmp.name = (string)gamepromo["name"];
                        gametmp.appid = (string)gamepromo["appid"];
                        gametmp.setImg((string)gamepromo["icon"]);
                        gametmp.link = (string)gamepromo["link"];
                        gametmp.des = (string)gamepromo["des"];
                        if (!memclick.Contains(gametmp.pkg))
                        {
                            re.Add(gametmp);
                        }
                        else
                        {
                            listTmp.Add(gametmp);
                        }
                        idx++;
                        if (idx >= listmemgames.Count)
                        {
                            idx = 0;
                        }
                        if (re.Count >= num)
                        {
                            break;
                        }
                    }
                    PlayerPrefs.SetInt("mem_idx_promo_ed", idx);
                    if (re.Count < num && listTmp.Count > 0)
                    {
                        for (int i = re.Count; i < num; i++)
                        {
                            re.Add(listTmp[0]);
                            listTmp.RemoveAt(0);
                            if (listTmp.Count <= 0)
                            {
                                break;
                            }
                        }
                        listTmp.Clear();
                    }
                    if (re.Count > 0)
                    {
                        downLoadIconPromoGame(listmemgames, re[re.Count - 1].pkg);
                    }
                    return re;
                }
            }
            return null;
        }
        public void getListSkuApp()
        {
            string linkdownload = PlayerPrefs.GetString("mem_link_inapp", "");
            if (linkdownload.Length < 10 || linkdownload.StartsWith("105"))
            {
                return;
            }

            string url = linkdownload.Substring(3);
            if (!System.IO.Directory.Exists(Application.persistentDataPath + "/files/"))
            {
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/files/");
            }

            string path = Application.persistentDataPath + "/files/cf_ina_ga.txt";
            DownLoadUtil.downloadText(url, path, (statedown) =>
            {
                if (statedown == 1)
                {
                    linkdownload = "105" + url;
                    PlayerPrefs.SetString("mem_link_inapp", linkdownload);
                }
            });
        }

        public PromoGameOb getGame4Native(MyAdNativeOrient orient)
        {
            bool isNext = false;
            long tcurr = SdkUtil.CurrentTimeMilis();
            if (tMemPromo4Native < 0)
            {
                tMemPromo4Native = tcurr;
            }
            if ((tcurr - tMemPromo4Native) >= 10000)
            {
                isNext = true;
            }
            if (gameNtPromoCurr != null && !isNext)
            {
                return gameNtPromoCurr;
            }
            else
            {
                string memlistgame = PlayerPrefs.GetString("cf_game_promo", "");
                string memclick = PlayerPrefs.GetString("mem_game_ntpromo_click", "");
                string keyOrient = "ad_h";
                int countClick = 0;
                if (orient == MyAdNativeOrient.Vertical)
                {
                    keyOrient = "ad_v";
                }
                if (memlistgame != null && memlistgame.Length > 10)
                {
                    var obmemgames = (IDictionary<string, object>)JsonDecoder.DecodeText(memlistgame);
                    List<object> listmemgames = null;
                    if (obmemgames != null && obmemgames.ContainsKey("games"))
                    {
                        listmemgames = (List<object>)obmemgames["games"];
                    }
                    if (listmemgames != null)
                    {
                        string pkggameWillPromo = PlayerPrefs.GetString("mem_game_will_ntpromo", "");
                        PromoGameOb tmpClick = null;
                        PromoGameOb tmpRe = null;
                        int idxget = -1;
                        int idxClickget = -1;
                        int countGame4Nt = 0;
                        gameNtPromoCurr = null;
                        for (int i = 0; i < listmemgames.Count; i++)
                        {
                            var gamepromo = (IDictionary<string, object>)listmemgames[i];
                            string pkgcuu = (string)gamepromo["pkg"];
                            if (gamepromo.ContainsKey(keyOrient))
                            {
                                countGame4Nt++;
                                if (!memclick.Contains(pkgcuu))
                                {
                                    if (pkggameWillPromo == null || pkggameWillPromo.Length < 5)
                                    {
                                        gameNtPromoCurr = PromoGameOb.newFromData(gamepromo);
                                        idxget = i;
                                        break;
                                    }
                                    else
                                    {
                                        if (pkgcuu.Equals(pkggameWillPromo))
                                        {
                                            gameNtPromoCurr = PromoGameOb.newFromData(gamepromo);
                                            idxget = i;
                                            break;
                                        }
                                        else if (tmpRe == null)
                                        {
                                            tmpRe = PromoGameOb.newFromData(gamepromo);
                                            idxget = i;
                                        }
                                    }
                                }
                                else
                                {
                                    if (tmpClick == null)
                                    {
                                        idxClickget = i;
                                        tmpClick = PromoGameOb.newFromData(gamepromo);
                                    }
                                    countClick++;
                                }
                            }
                        }
                        if (countClick >= countGame4Nt)
                        {
                            PlayerPrefs.SetString("mem_game_ntpromo_click", "");
                            PlayerPrefs.SetString("mem_game_will_ntpromo", "");
                        }
                        if (gameNtPromoCurr == null)
                        {
                            if (tmpRe != null)
                            {
                                gameNtPromoCurr = tmpRe;
                            }
                            else
                            {
                                if (tmpClick != null)
                                {
                                    gameNtPromoCurr = tmpClick;
                                    idxget = idxClickget;
                                }
                            }
                        }
                        if (gameNtPromoCurr != null && listmemgames.Count > 0)
                        {
                            idxget++;
                            if (idxget >= listmemgames.Count)
                            {
                                idxget = 0;
                            }
                            var gamepromo = (IDictionary<string, object>)listmemgames[idxget];
                            PlayerPrefs.SetString("mem_game_will_ntpromo", (string)gamepromo["pkg"]);
                        }
                    }
                }
            }
            return gameNtPromoCurr;
        }
        public void onclickNativePromo(PromoGameOb game)
        {
            if (game != null)
            {
                string memclick = PlayerPrefs.GetString("mem_game_ntpromo_click", "");
                memclick += (";" + game.pkg + "," + GameHelper.CurrentTimeMilisReal());
                PlayerPrefs.SetString("mem_game_ntpromo_click", memclick);
                logEvent("click_promont");

#if ENABLE_AppsFlyer
                Dictionary<string, string> ParamPromo = new Dictionary<string, string>();
                ParamPromo.Add("af_promo_appid", AppConfig.appid);
                AppsFlyerSDK.AppsFlyer.attributeAndOpenStore(game.appid, "cross_promo_campaign", ParamPromo, mygame.sdk.AppsFlyerHelperScript.Instance);
#endif

                if (gameNtPromoCurr == null || (gameNtPromoCurr != null && game.name.CompareTo(gameNtPromoCurr.name) == 0))
                {
                    gameNtPromoCurr = null;
                    getGame4Native(MyAdNativeOrient.Horizontal);
                }
                if (game.link != null && game.link.Length > 10)
                {
                    GameHelper.Instance.gotoLink(game.link);
                }
                else
                {
#if UNITY_IOS || UNITY_IPHONE
                    GameHelper.Instance.gotoStore(game.appid);
#else
                    GameHelper.Instance.gotoStore(game.pkg);
#endif
                }
            }
        }
        public PromoGameOb nextPromo4Native()
        {
            gameNtPromoCurr = null;
            return getGame4Native(MyAdNativeOrient.Horizontal);
        }
    }

    public class QueueLogFir
    {
        public string eventName { get; private set; }
        public string extral = "";
#if FIRBASE_ENABLE
        public Dictionary<string, object> memParams = null;
#endif

        public QueueLogFir(string eventName)
        {
            this.eventName = eventName;
            extral = "";
#if FIRBASE_ENABLE
            memParams = null;
#endif
        }
    }
}
