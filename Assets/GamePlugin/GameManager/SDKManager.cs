//#define CHECK_4INAPP
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System.Globalization;

namespace mygame.sdk
{
    public delegate void TakeScreenShootCallback(Texture2D texture);

    public enum AppType
    {
        Application = 0,
        MultiApplication,
        MultiApplicationExtral,
        MultiApplicationStrictMode,
        MultiApplicationStrictModeAfter
    }

    public enum ActivityCustom
    {
        AC_None = 0,
        AC_Unity,
        AC_Firebase
    }

    public enum App_Open_ad_Orien
    {
        Orien_Portraid = 0,
        Orien_Landscape
    }

    public class SDKManager : MonoBehaviour
    {
        public static event Action<int> CBFinishShowLanAge = null;
        public Action<bool> CBPauseGame { get; set; }
#if UNITY_EDITOR
        [HideInInspector] public App_Open_ad_Orien OpenAdOrientation;
#if UNITY_ANDROID
        public bool isLicenses = false;
#endif

#endif

        public int IndexSceneLoading = 1;
        public bool isAdCanvasSandbox = false;
        public bool isDisableLog = false;

        public Transform canvasUpdate;
        public Transform CanvasLoading;
        public Transform PopupShowFirstAds;

        public SplashLoadingCtr splashLoadingCtr;
        private PopupNoConnectionCtr PopupLoseConnection;
        private PopupRateCtl ratePopControl;

        private PopupPuzzleOther otherPuzzlePop;

        private GameObject PopupNotAllowGame;
        private GameObject PopupNotSupportIAP;

        private TakeScreenShootCallback _cb;

        public UpdateGameController updateGameController { get; set; }

        public static SDKManager Instance { get; private set; }

        public List<MoreGameOb> listMoreGame = new List<MoreGameOb>();
        float tcheckFirstShow = 0;
        bool isAdsShowFirsted = false;
        [HideInInspector] public bool isCallShowSplash = false;
        float tpreCheck = -1;
        public int flagTimeScale { get; set; }
        public bool flagNewDay { get; private set; }
        public bool isAllowShowFirstOpen { get; set; }
        static bool isLogRetenrion = false;
        float tShowWaitShowfull = 0;
        int stateShowFirstAds = 0;
        float timeCount4ShowFirstAds = 0;
        public bool deviceIsRooted { get; set; }
        [HideInInspector] public int totalTimePlayGame = 0;
        public int counSessionGame { get; private set; }
        private int time4SessionGame = 0;
        public int activeDay { get; private set; }

        [HideInInspector] public string mediaSource = "unknow";
        [HideInInspector] public string mediaCampain = "unknow";
        public string afAdset { get; set; } = "unknow";
        public string afAd { get; set; } = "unknow";
        public string afAdId { get; set; } = "unknow";
        public string currPlacement { get; set; }

        public Font fontReplace;
        public float fontSize = 1;
        public float lineSpacing = 1;

        public int timeWhenGetOnline { get; set; }
        public int timeOnline { get; set; }//minus

        private int statusCheckFollowPlay = 0;
        private float duarationPlayLv = 0;

        [HideInInspector] public bool isClickAd = false;
        [HideInInspector] public int statusClickAd = 0;

        private float tforCheckSession = -1;
        public float timeEnterGame { get; private set; }
        public float timeEnterSession { get; private set; }

        public int verCodeInstall { get; private set; }
        public string verNameInstall { get; private set; }

        private void Awake()
        {
#if !UNITY_EDITOR
            if (isDisableLog)
            {
                Debug.unityLogger.logEnabled = false;
                Debug.unityLogger.filterLogType = LogType.Exception;
                Debug.developerConsoleVisible = false;
            }
            else
            {
                Debug.unityLogger.logEnabled = true;
                Debug.unityLogger.filterLogType = LogType.Log;
                Debug.developerConsoleVisible = true;
            }
#else
            Debug.unityLogger.logEnabled = true;
            Debug.unityLogger.filterLogType = LogType.Log;
            Debug.developerConsoleVisible = true;
#endif
            if (Instance == null)
            {
                Instance = this;
                counSessionGame = PlayerPrefs.GetInt("mem_gl_count_play", 0);
                time4SessionGame = PlayerPrefs.GetInt("mem_gl_time_session_play", 0);
                timeEnterGame = PlayerPrefs.GetFloat("mem_tenter_game", 0);
                timeEnterSession = PlayerPrefs.GetFloat("mem_tenter_session", 0);
                checkNewSession("Awake", 0);

                activeDay = PlayerPrefs.GetInt("mem_gl_count_acdy", 0);
                verCodeInstall = PlayerPrefs.GetInt("mem_ver_app_ins", -1);

                if (verCodeInstall <= 0)
                {
                    verCodeInstall = AppConfig.verapp;
                    verNameInstall = AppConfig.verName;
                    PlayerPrefs.SetInt("mem_ver_app_ins", AppConfig.verapp);
                    PlayerPrefs.SetString("mem_ver_name_ins", AppConfig.verName);
                    Debug.Log($"mysdk: sdkmanager set ver app install={verCodeInstall}-{verNameInstall}");
                }
                else
                {
                    verNameInstall = PlayerPrefs.GetString("mem_ver_name_ins", "0.0.0");
                    Debug.Log($"mysdk: sdkmanager ver app install={verCodeInstall}-{verNameInstall}");
                }
                Debug.Log($"mysdk: sdkmanager timeEnterGame={timeEnterGame} session={counSessionGame} timeEnterSession={timeEnterSession}");
                mediaSource = PlayerPrefsBase.Instance().getString("mem_af_media_source", "unknow");
                mediaCampain = PlayerPrefsBase.Instance().getString("mem_af_media_campain", "unknow");
                afAd = PlayerPrefsBase.Instance().getString("mem_af_ad", "unknow");
                afAdId = PlayerPrefsBase.Instance().getString("mem_af_ad_id", "unknow");
                afAdset = PlayerPrefsBase.Instance().getString("mem_af_adset", "unknow");
                totalTimePlayGame = PlayerPrefsBase.Instance().getInt("mem_tot_tim_gpla", 0);
                GameRes.transLevelOld2New();
                updateGameController = null;
                flagTimeScale = 0;
                CBPauseGame = null;
                tShowWaitShowfull = 0;
                stateShowFirstAds = 0;
                isAllowShowFirstOpen = true;
                deviceIsRooted = false;
                isAdsShowFirsted = true;
                isCallShowSplash = false;
                timeWhenGetOnline = -1;
                timeOnline = -1;
                flagNewDay = isNewDay();
                statusCheckFollowPlay = PlayerPrefs.GetInt("mem_status_play", 0);
                if (flagNewDay)
                {
                    activeDay++;
                    PlayerPrefs.SetInt("mem_gl_count_acdy", activeDay);
                }

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (this != Instance) Destroy(gameObject);
            }
        }

        private void Start()
        {
            FIRhelper.logEvent("game_0_start");
            AppsFlyerHelperScript.logEvent("game_0_start");
            getDeviceId();
            //GameHelper.ScreenInfo();
            if (AdsHelper.isRemoveAds(0))
            {
                Debug.Log("mysdk: sdkmanager removeads not show first");
                tcheckFirstShow = 10000;
            }
            else
            {
                int ss = AdsHelper.Instance.isShowOpenAds(true);
                if (ss == 0 || ss == 1)
                {
                    Debug.Log($"mysdk: sdkmanager ads open isshowopen={ss}");
                    tcheckFirstShow = 10000;
                }
            }
            if (AppConfig.FlagShowSplashLoading == 1)
            {
                // if (isShowSplash())
                {
                    float tsplash = getTimeSplash();
                    if (tsplash > 0 || (tsplash < 0 && IndexSceneLoading > 0))
                    {
                        if (tsplash < 0)
                        {
                            tsplash = 1;
                        }
                        var prefab = Resources.Load<GameObject>("Popup/PopupSplashLoading");
                        var ob = Instantiate(prefab, Vector3.zero, UIManager.Instance.popupHolder.rotation, UIManager.Instance.popupHolder);
                        splashLoadingCtr = ob.GetComponent<SplashLoadingCtr>();
                        SdkUtil.logd($"sdkmanager showLoading={tsplash}");
                        FIRhelper.logEvent("game_1_showloading");
                        AppsFlyerHelperScript.logEvent("game_1_showloading");
                        splashLoadingCtr.showLoading(tsplash);
                    }
                    else
                    {
                        SdkUtil.logd($"sdkmanager showLoading not show first");
                        onSplashFinishLoding();
                    }
                }
                GameHelper.Instance.isAlowShowAppOpenAd = false;
            }
            else if (AppConfig.FlagShowSplashLoading == 0)
            {
                SdkUtil.logd($"sdkmanager showLoading not show");
                onSplashFinishLoding();
            }
            else
            {
                Debug.Log($"mysdk: sdkmanager let call when finish loading onSplashFinishLoading !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                isAllowShowFirstOpen = false;
                GameHelper.Instance.isAlowShowAppOpenAd = false;
            }
            StartCoroutine(wait4loginLog());
            loadMoreGame();
            downMoreGame();
            PuzzleControl.getInstance();

            if (flagNewDay && !isLogRetenrion)
            {
                Debug.Log("mysdk: sdkmanager Adjust Helper log retention");
                isLogRetenrion = true;
                AdjustHelper.LogEvent(AdjustEventName.Retention, null);
            }

            StartCoroutine(initNotify());

#if UNITY_EDITOR
            Debug.Log("mysdk: sdkmanager path game=" + Application.persistentDataPath);
#endif
#if UNITY_ANDROID && !UNITY_EDITOR && CHECK_4INAPP
            if (isAdCanvasSandbox)
            {
                mygame.plugin.Android.GameHelperAndroid.printSigning();
            }
            bool isInstall = mygame.plugin.Android.GameHelperAndroid.isInstallFromGooglePlay();
            int rsesss = PlayerPrefsBase.Instance().getInt("mem_procinva_gema", 3);
            if (rsesss != 1 && rsesss != 2 && rsesss != 3 && rsesss != 101 && rsesss != 102 && rsesss != 103 && rsesss != 1985)
            {
                rsesss = 103;
            }
            if (!isInstall)
            {
                PlayerPrefsBase.Instance().setInt("mem_kt_cdtgpl", 1);
                if (rsesss == 101 || rsesss == 103)
                {
                    showNotAllowGame();
                    FIRhelper.logEvent($"game_invalid1_notallow");
                }
                FIRhelper.logEvent($"game_invalid1");
            }
            else
            {
                PlayerPrefsBase.Instance().setInt("mem_kt_cdtgpl", 0);
            }
            if (isInstall || (!isInstall && rsesss != 101 && rsesss != 103))
            {
                Debug.Log($"mysdk: sdkmanager bira 1");
                int flagccc = PlayerPrefsBase.Instance().getInt("mem_flag_check_bira", 1);
                mygame.plugin.Android.GameHelperAndroid.checkPiraCheck(flagccc);
            }
#endif
            if (statusCheckFollowPlay == 1)
            {
                onEndGame(Myapi.LevelPassStatus.EndOther, "terminate");
            }
            int flagShow = PlayerPrefs.GetInt("cf_show_langage", AppConfig.Flag_show_langeAge);
            if (flagShow > 0)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    AdsHelper.Instance.loadNative("nt_chose_langage", true);
                }, 3);
            }
        }

        public static float getTimeSplash()
        {
            float tsplash = PlayerPrefs.GetInt("time_splash_scr", AppConfig.WaitSplash);
#if UNITY_IOS || UNITY_IPHONE
            if (Instance != null && Instance.counSessionGame == 1 && tsplash < 3)
            {
                tsplash = 3;
            }
#endif
            if (PlayerPrefsUtil.CF_TimeSplashFirstSection > 1 && Instance != null && Instance.counSessionGame <= 1 && Instance.timeEnterGame < 10)
            {
                tsplash = PlayerPrefsUtil.CF_TimeSplashFirstSection;
            }

            return tsplash;
        }

        public void onClickAd()
        {
            isClickAd = true;
            statusClickAd = 1;
            GameHelper.Instance.isAlowShowAppOpenAd = false;
            StartCoroutine(waitResetFlagShowOpenAds());
        }

        public void onChangePlacement(string pl)
        {
            Debug.Log($"mysdk: sdkmanager onChangePlacement={pl}");
            currPlacement = pl;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            Debug.Log($"mysdk: sdkmanager OnApplicationPause={pauseStatus}");
            closeWaitShowFull();
            if (!pauseStatus)
            {
                if (isClickAd)
                {
                    Debug.Log($"mysdk: sdkmanager OnApplicationPause after openlink when click ad");
                    isClickAd = false;
                    statusClickAd = 0;
                    StartCoroutine(waitResetFlagShowOpenAdsAfterOpenLink());
                }
            }
            else
            {
                checkNewSession("OnApplicationPause", 0);

                if (statusClickAd == 1 && isClickAd)
                {
                    Debug.Log($"mysdk: sdkmanager OnApplicationPause clear flag check click ad wait resume app will allow show openad");
                    statusClickAd = 0;
                }
            }
        }

        private void checkNewSession(string from, float dt)
        {
            long tcur = SdkUtil.CurrentTimeMilis() / 1000;
            if ((tcur - time4SessionGame) > 1800)
            {
                counSessionGame++;
                PlayerPrefs.SetInt("mem_gl_count_play", counSessionGame);
                timeEnterSession = 0;
                PlayerPrefs.SetFloat("mem_tenter_session", timeEnterSession);
                Debug.Log($"mysdk: sdkmanager {from} new session={counSessionGame}");
                if (from.CompareTo("Awake") == 0)
                {
                    Invoke("waitSave4SS", 0.2f);
                }
            }
            else
            {
                timeEnterSession += dt;
                PlayerPrefs.SetFloat("mem_tenter_session", timeEnterSession);
            }
            timeEnterGame += dt;
            PlayerPrefs.SetFloat("mem_tenter_game", timeEnterGame);
            time4SessionGame = (int)tcur;
            PlayerPrefs.SetInt("mem_gl_time_session_play", (int)tcur);

            Debug.Log($"{from} timeEnterGame={timeEnterGame} timeEnterSession={timeEnterSession}");
        }
        private void waitSave4SS()
        {
            PlayerPrefs.SetInt("mem_gl_count_play", counSessionGame);
        }
        private void onUpdate4Session()
        {
            tforCheckSession += Time.deltaTime * Time.timeScale;
            if (tforCheckSession >= 5)
            {
                checkNewSession("update", tforCheckSession);
                tforCheckSession = 0;
            }
        }
        IEnumerator waitResetFlagShowOpenAds()
        {
            yield return new WaitForSeconds(2.5f);
            if (statusClickAd == 1)
            {
                statusClickAd = 0;
                isClickAd = false;
                GameHelper.Instance.isAlowShowAppOpenAd = true;
                Debug.Log($"mysdk: sdkmanager ad click but not openLink");
            }
        }
        IEnumerator waitResetFlagShowOpenAdsAfterOpenLink()
        {
            yield return new WaitForSeconds(0.35f);
            GameHelper.Instance.isAlowShowAppOpenAd = true;
            Debug.Log($"mysdk: sdkmanager set allow openad after open link");
        }

        private IEnumerator initNotify()
        {
            yield return new WaitForSeconds(10);
            int memvernoti = PlayerPrefs.GetInt("mem_ver_data_noti", 0);
#if UNITY_ANDROID && !UNITY_EDITOR
            mygame.plugin.Android.GameHelperAndroid.setupEnviromentNotify();
            TextAsset txtnoti = Resources.Load<TextAsset>("LocalNotify/Android/dataNotify");
            if (txtnoti != null && txtnoti.text != null && txtnoti.text.Length > 0)
            {
                NotifyObject dataNoti = JsonUtility.FromJson<NotifyObject>(txtnoti.text);
                if (dataNoti != null && dataNoti.ver > memvernoti)
                {
                    PlayerPrefs.SetInt("mem_ver_data_noti", dataNoti.ver);
                    if (dataNoti.data.Count > 0)
                    {
                        mygame.plugin.Android.GameHelperAndroid.setupLocalNotifyNotify(dataNoti.data2String());
                    }
                }
            }
#elif (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
            TextAsset txtnoti = Resources.Load<TextAsset>("LocalNotify/iOS/dataNotify");
            if (txtnoti != null && txtnoti.text != null && txtnoti.text.Length > 0)
            {
                NotifyObject dataNoti = JsonUtility.FromJson<NotifyObject>(txtnoti.text);
                if (dataNoti != null && dataNoti.ver > memvernoti)
                {
                    PlayerPrefs.SetInt("mem_ver_data_noti", dataNoti.ver);
                    GameHelperIos.clearAllNoti();
                    if (dataNoti.data.Count > 0)
                    {
                        foreach (NotiData item in dataNoti.data)
                        {
                            bool isrepeat = false;
                            for (int j = 0; j < item.repeat.Length; j++)
                            {
                                if (item.repeat[j] == '1')
                                {
                                    isrepeat = true;
                                    GameHelperIos.localNotify(item.titleNoti, item.msgNoti, item.hour, item.minus, j + 1);
                                }
                            }
                            if (!isrepeat)
                            {
                                GameHelperIos.localNotify(item.titleNoti, item.msgNoti, item.hour, item.minus, -1);
                            }
                        }
                    }
                }
            }
#endif
        }

        private void OnLowMemory()
        {
            FIRhelper.logEvent("app_low_mem");
            if (PlayerPrefs.GetInt("is_unload_when_lowmem", 0) == 1)
            {
                Resources.UnloadUnusedAssets();
            }
        }
        private string getDeviceId()
        {
            string deviceUniqueIdentifier = "0";
#if UNITY_EDITOR
            deviceUniqueIdentifier = PlayerPrefs.GetString("key_ads_identify", "");
            if (deviceUniqueIdentifier == null || deviceUniqueIdentifier.Length < 3)
            {
                deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
                PlayerPrefs.SetString("key_ads_identify", deviceUniqueIdentifier);
            }
#else

#if UNITY_ANDROID
            deviceUniqueIdentifier = PlayerPrefs.GetString("key_ads_identify", "");
#elif UNITY_IOS || UNITY_IPHONE
            deviceUniqueIdentifier = GameHelperIos.GetKeychainValue("device_ios_identify");
            if (deviceUniqueIdentifier == null || deviceUniqueIdentifier.Length < 3)
            {
                deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
                GameHelperIos.SetKeychainValue("device_ios_identify", deviceUniqueIdentifier);
            }
#endif

#endif
            Debug.Log($"mysdk: sdkmanager deviceUniqueIdentifier={deviceUniqueIdentifier}");
            return deviceUniqueIdentifier;
        }
        public bool isDeviceTest()
        {
            string deviceUniqueIdentifier = getDeviceId();
            if (AppConfig.Device_test_id.Contains(deviceUniqueIdentifier))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void updateTimeSplash(int timeSplash)
        {
            if (splashLoadingCtr != null)
            {
                splashLoadingCtr.updateTimeSplash(timeSplash);
            }
        }

        public void onSplashFinishLoding(bool isSetAllowShowFirstOpen = false)
        {
            Debug.Log("mysdk: sdkmanager onSplashFinishLoding");
            
            // Apply alternate app icon if set by remote config
            string alternateIcon = PlayerPrefsUtil.CF_AppIconName;
            if (!string.IsNullOrEmpty(alternateIcon))
            {
                ChangeIconPlugin.SetAlternateIcon(alternateIcon == "default" ? null : alternateIcon);
            }

            FIRhelper.logEvent("game_2_finishloading");
            AppsFlyerHelperScript.logEvent("game_2_finishloading");
            AdsHelper.Instance.hideBannerRect();
            AdsHelper.Instance.hideRectNt();
            splashLoadingCtr = null;
            isCallShowSplash = true;
            if (isSetAllowShowFirstOpen)
            {
                isAllowShowFirstOpen = true;
            }
            GameHelper.Instance.isAlowShowAppOpenAd = true;
            checShowFirst();

            int flagShow = PlayerPrefs.GetInt("cf_show_langage", AppConfig.Flag_show_langeAge);
            int flagMemshowLangage = PlayerPrefs.GetInt("mem_show_langage", 0);
            if (flagShow > 0)
            {
                RectTransform poplangage = null;
                if (flagMemshowLangage == 0 && (flagShow & 1) > 0)
                {
                    SDKManager.Instance.onChangePlacement("choose_lang");
                    var prefab = Resources.Load<RectTransform>("Popup/UIChooseLanguage");
                    poplangage = Instantiate(prefab, Vector3.zero, PopupShowFirstAds.transform.parent.rotation, PopupShowFirstAds.transform.parent);
                    PlayerPrefs.SetInt("mem_show_langage", 1);
                }
                else if (flagMemshowLangage < 2 && (flagShow & 2) > 0)
                {
                    SDKManager.Instance.onChangePlacement("choose_age");
                    var prefab = Resources.Load<RectTransform>("Popup/UIChooseAge");
                    poplangage = Instantiate(prefab, Vector3.zero, PopupShowFirstAds.transform.parent.rotation, PopupShowFirstAds.transform.parent);
                    PlayerPrefs.SetInt("mem_show_langage", 2);
                }
                if (poplangage != null)
                {
                    poplangage.anchoredPosition = Vector2.zero;
                    poplangage.anchoredPosition3D = Vector3.zero;
                    poplangage.anchorMin = Vector2.zero;
                    poplangage.anchorMax = Vector2.one;
                    poplangage.sizeDelta = Vector2.zero;
                    poplangage.localScale = Vector3.one;
                    var flagfull = AdsHelper.Instance.getCfAdsPlacement("full_chooseage", 3);
                    int flagshow = 3;
                    if (flagfull != null)
                    {
                        flagshow = flagfull.flagShow;
                    }
                    if (flagshow > 0)
                    {
                        AdsHelper.Instance.loadFull4ThisTurn("full_chooseage", false, GameRes.GetLevel(Level_type.Common), flagshow - 1);
                    }
                }
                else
                {
                    onCloseLangAge(0);
                }
            }
            else
            {
                PlayerPrefs.SetInt("mem_show_langage", 2);
                onCloseLangAge(-1);
            }
        }

        void checShowFirst()
        {
            int cfSessionShowFirst = PlayerPrefs.GetInt("cf_show_first_open", AppConfig.ShowSplashFirst);
            Debug.Log("mysdk: sdkmanager checShowFirst onfn loading isfirst=" + cfSessionShowFirst + ", counSessionGame=" + counSessionGame);
            bool isCheck = true;
#if UNITY_IOS || UNITY_IPHONE
            AdsHelper.Instance.initAds();
            if (!GameHelper.checkLvXaDu())
            {
                isCheck = false;
            }
#endif
            if (isCheck)
            {
                if (counSessionGame >= cfSessionShowFirst)
                {
                    doShowFirst();
                }
            }
            if (AdsHelper.Instance != null)
            {
                if (AdsHelper.Instance.currConfig.OpenAdIsShowFirstOpen != 2)
                {
                    isAllowShowFirstOpen = false;
                }
            }
        }

        public void onCloseLangAge(int flag)
        {
            if (CBFinishShowLanAge != null)
            {
                CBFinishShowLanAge(0);
            }
        }

        public void onPlayGame(int lv, Myapi.LevelPassStatus statusplay, string where = "", string mode = "", Myapi.TypeDif type = Myapi.TypeDif.Normal)
        {
            duarationPlayLv = 0;
            if (statusCheckFollowPlay != 0)
            {
                Debug.LogError("mysdk: sdkmanager Follow Play game is wrong, let double check end game!!!!!!!!!!!!");
            }
            statusCheckFollowPlay = 1;
            PlayerPrefs.SetInt("mem_status_play", 1);
            PlayerPrefs.SetInt("mem_lv_play_4_log", lv);
            PlayerPrefs.SetString("mem_mode_play_4_log", mode);
            PlayerPrefs.SetInt("mem_dif_play_4_log", ((int)type));
            string sm = "";
            if (mode != null && mode.Length > 0)
            {
                sm = mode + "_";
            }
            FIRhelper.logEvent($"level_{sm}{lv:000}_play");
            if (statusplay == Myapi.LevelPassStatus.PlayAgain)
            {
                FIRhelper.logEvent($"level_{sm}{lv:000}_playagain");
            }
            else if (statusplay == Myapi.LevelPassStatus.PlayRe)
            {
                FIRhelper.logEvent($"level_{sm}{lv:000}_playre");
            }
            Myapi.LogEventApi.Instance().logLevel(lv, mode, type, statusplay, 0, where);
        }

        public void onEndGame(Myapi.LevelPassStatus stateEnd, string where = "")
        {
            if (statusCheckFollowPlay != 1)
            {
                Debug.LogError("mysdk: sdkmanager Follow Play game is wrong, let double check play game!!!!!!!!!!!!");
            }
            int lv = PlayerPrefs.GetInt("mem_lv_play_4_log", 1);
            string mode = PlayerPrefs.GetString("mem_mode_play_4_log", "");
            int idif = PlayerPrefs.GetInt("mem_dif_play_4_log", 1);
            Myapi.TypeDif type = (Myapi.TypeDif)idif;
            PlayerPrefs.SetInt("mem_status_play", 0);
            statusCheckFollowPlay = 0;
            totalTimePlayGame += (int)duarationPlayLv;
            PlayerPrefsBase.Instance().setInt("mem_tot_tim_gpla", totalTimePlayGame);
            string send = "";
            string subSen = "";
            if (stateEnd == Myapi.LevelPassStatus.Win || stateEnd == Myapi.LevelPassStatus.WinAgain)
            {
                send = "win";
                if (stateEnd == Myapi.LevelPassStatus.WinAgain)
                {
                    subSen = "winagain";
                }
            }
            else if (stateEnd == Myapi.LevelPassStatus.Lose || stateEnd == Myapi.LevelPassStatus.LoseAgain)
            {
                send = "lose";
                if (stateEnd == Myapi.LevelPassStatus.LoseAgain)
                {
                    subSen = "loseagain";
                }
            }
            else if (stateEnd == Myapi.LevelPassStatus.Skip)
            {
                send = "skip";
            }
            else
            {
                send = "endother";
            }
            string sm = "";
            if (mode != null && mode.Length > 0)
            {
                sm = mode + "_";
            }
            FIRhelper.logEvent($"level_{sm}{lv:000}_{send}");
            if (subSen.Length > 3)
            {
                FIRhelper.logEvent($"level_{sm}{lv:000}_{subSen}");
            }
            Myapi.LogEventApi.Instance().logLevel(lv, mode, type, stateEnd, (int)duarationPlayLv, where);
            if (duarationPlayLv > 10)
            {
                AdsHelper.Instance.checkChangeSpecialCon();
            }
        }

        public void changeFrameRate()
        {
            // #if UNITY_IOS || UNITY_IPHONE
            //             if (UnityEngine.iOS.Device.generation.ToString().Contains("iPhone"))
            //             {
            //                 int something = (int)UnityEngine.iOS.Device.generation;
            //                 if (something > 1 && something <= 32)
            //                 {
            //                     QualitySettings.vSyncCount = 0;
            //                     Application.targetFrameRate = 60;
            //                     Debug.Log("mysdk: sdkmanager ios low set pfs=" + Application.targetFrameRate);
            //                 }
            //                 else
            //                 {
            //                     Application.targetFrameRate = PlayerPrefs.GetInt("", 60);
            //                     QualitySettings.vSyncCount = 0;
            //                     Debug.Log("mysdk: sdkmanager ios high set pfs=" + Application.targetFrameRate);
            //                 }
            //             }
            //             else
            //             {
            //                 Application.targetFrameRate = PlayerPrefs.GetInt("", 60);
            //                 QualitySettings.vSyncCount = 0;
            //                 Debug.Log("mysdk: sdkmanager ios not iphone set pfs=" + Application.targetFrameRate);
            //             }
            // #else
            //             Application.targetFrameRate = PlayerPrefs.GetInt("", 60);
            //             QualitySettings.vSyncCount = 0;
            //             Debug.Log("mysdk: sdkmanager Android set pfs=" + Application.targetFrameRate);
            // #endif
        }

        IEnumerator wait4loginLog()
        {
            yield return new WaitForSeconds(0.05f);
            changeFrameRate();
            yield return new WaitForSeconds(2);
            Myapi.LogEventApi.Instance().logLogin();
            yield return new WaitForSeconds(5);
            Myapi.LogEventApi.Instance().pushMemLog();
        }

        public bool checkConnection(string title = "", string msg = "")
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                int countopenapp = PlayerPrefs.GetInt("mem_gl_count_play", 0);
                int cfCounOpenApp4CheckCon = PlayerPrefs.GetInt("cf_count_open_checkcon", 10);
                int cfLvcheckCon = PlayerPrefs.GetInt("cf_lv_checkcon", 50);
                if (countopenapp >= cfCounOpenApp4CheckCon || GameRes.LevelCommon() >= cfLvcheckCon)
                {
                    showPopupNotConnection(title, msg);
                    return false;
                }
            }
            return true;
        }

        public void showPopupNotConnection(string title = "", string msg = "")
        {
            if (PopupLoseConnection == null)
            {
                var prefab = Resources.Load<PopupNoConnectionCtr>("Popup/PopupNotConnection");
                PopupLoseConnection = Instantiate(prefab, Vector3.zero, UIManager.Instance.popupHolder.rotation, UIManager.Instance.popupHolder);
            }
            if (SdkUtil.isiPad())
            {
                PopupLoseConnection.transform.GetChild(0).localScale = Vector3.one * .9f;
            }
            PopupLoseConnection.gameObject.SetActive(true);
            PopupLoseConnection.show(title, msg);
        }

        public bool showRate(Action onComplete)
        {
            Debug.Log("aaaaa showRate0");
            if (PlayerPrefs.GetInt("is_show_rate", 0) == 1)
            {
                return false;
            }
            Debug.Log("aaaaa showRate1");
#if UNITY_IOS || UNITY_IPHONE
            GameHelper.Instance.rate();
            onComplete?.Invoke();
#else
            if (ratePopControl == null)
            {
                var prefab = Resources.Load<PopupRateCtl>("Popup/PopupRate");
                ratePopControl = Instantiate(prefab, Vector3.zero, UIManager.Instance.popupHolder.rotation, UIManager.Instance.popupHolder);
            }
            //if (SdkUtil.isiPad())
            //{
            //    ratePopControl.transform.GetChild(0).localScale = Vector3.one * .9f;
            //}
            ratePopControl.gameObject.SetActive(true);
            RectTransform rc = ratePopControl.GetComponent<RectTransform>();
            rc.anchorMin = new Vector2(0, 0);
            rc.anchorMax = new Vector2(1, 1);
            rc.sizeDelta = new Vector2(0, 0);
            rc.anchoredPosition = Vector2.zero;
            rc.anchoredPosition3D = Vector3.zero;
#endif
            return true;
        }

        public void showOtherPuzzle(PromoGameOb game)
        {
            if (otherPuzzlePop == null)
            {
                var prefab = Resources.Load<PopupPuzzleOther>("Puzzle/PopupPuzzle");
                otherPuzzlePop = Instantiate(prefab, Vector3.zero, Quaternion.identity, PopupShowFirstAds.transform.parent);
            }
            otherPuzzlePop.setGameShow(game);
            // if (SdkUtil.isiPad())
            // {
            //     otherPuzzlePop.transform.GetChild(0).localScale = Vector3.one * .9f;
            // }
            otherPuzzlePop.gameObject.SetActive(true);
            RectTransform rc = otherPuzzlePop.GetComponent<RectTransform>();
            rc.anchorMin = new Vector2(0, 0);
            rc.anchorMax = new Vector2(1, 1);
            rc.sizeDelta = new Vector2(0, 0);
            rc.anchoredPosition = Vector2.zero;
            rc.anchoredPosition3D = Vector3.zero;
        }

        public bool isShowRate(int level)
        {
            string memlv = PlayerPrefs.GetString("mem_lv_showrate", ",4,22,41,");
            if (memlv.Length > 0)
            {
                if (!memlv.StartsWith(","))
                {
                    memlv = "," + memlv;
                }
                if (!memlv.EndsWith(","))
                {
                    memlv += ",";
                }
                if (memlv.Contains($",{level},"))
                {
                    return true;
                }
            }
            return false;
        }

        private bool isNewDay()
        {
            var today = DateTime.Today;
            var md = PlayerPrefs.GetInt("mem_day_4new", -1);
            var mm = PlayerPrefs.GetInt("mem_month_4new", -1);
            var my = PlayerPrefs.GetInt("mem_year_4new", -1);

            bool isnew = false;
            if (my < today.Year)
            {
                isnew = true;
            }
            else if (my == today.Year)
            {
                if (mm < today.Month)
                {
                    isnew = true;
                }
                else if (mm == today.Month)
                {
                    if (md < today.Day)
                    {
                        isnew = true;
                    }
                }
            }
            if (isnew)
            {
                Debug.Log("mysdk: sdkmanager new day");
                PlayerPrefs.SetInt("mem_day_4new", today.Day);
                PlayerPrefs.SetInt("mem_month_4new", today.Month);
                PlayerPrefs.SetInt("mem_year_4new", today.Year);
            }
            return isnew;
        }

        private void Update()
        {
            //calculatorFps();
            onUpdate4Session();
            if (statusCheckFollowPlay == 1)
            {
                if (Time.timeScale >= 0.00001f)
                {
                    duarationPlayLv += Time.unscaledDeltaTime;
                }
            }
            if (tcheckFirstShow < AdsHelper.Instance.currConfig.OpenAdTimeWaitShowFirst)
            {
                if (Time.timeScale <= 0.00001f)
                {
                    tcheckFirstShow += Time.unscaledDeltaTime;
                }
                else
                {
                    tcheckFirstShow += Time.deltaTime;
                }

                if (AdsHelper.Instance != null && !AdsHelper.Instance.isShowFulled)
                {
                    if (tcheckFirstShow - tpreCheck >= 0.5f || tcheckFirstShow >= AdsHelper.Instance.currConfig.OpenAdTimeWaitShowFirst)
                    {
                        tpreCheck = tcheckFirstShow;
                        if (isAllowShowFirstOpen)
                        {
                            if (AdsHelper.Instance.currConfig.OpenAdShowtype == 0
                                || AdsHelper.Instance.currConfig.OpenAdShowtype == 2
                                || AdsHelper.Instance.currConfig.OpenAdShowtype == 4
                            )
                            {
                                if (GameHelper.Instance.isAppOpenAdLoaded())
                                {
                                    tcheckFirstShow = 10000;
                                    Debug.Log("mysdk: sdkmanager doShowFirst Update1");
                                    doShowFirst();
                                }
                                else
                                {
                                    if (tcheckFirstShow >= AdsHelper.Instance.currConfig.OpenAdTimeWaitShowFirst)
                                    {
                                        if (AdsHelper.Instance.currConfig.OpenAdShowtype == 2)
                                        {
                                            if (!AdsHelper.Instance.isShowFulled)
                                            {
                                                stateShowFirstAds = 0;
                                                Debug.Log("mysdk: sdkmanager doShowFirst Update2");
                                                doShowFirst();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (AdsHelper.Instance.currConfig.OpenAdShowtype == 2 && tcheckFirstShow >= 12)
                                        {
                                            if (AdsHelper.Instance.isFull4Show(AdsBase.PLFullSplash, true, true))
                                            {
                                                tcheckFirstShow = 10000;
                                                Debug.Log("mysdk: sdkmanager doShowFirst Update3");
                                                doShowFirst();
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (AdsHelper.Instance.isFull4Show(AdsBase.PLFullSplash, true, true))
                                {
                                    tcheckFirstShow = 10000;
                                    stateShowFirstAds = 1;
                                    PopupShowFirstAds.gameObject.SetActive(true);
                                    Time.timeScale = 0;
                                    timeCount4ShowFirstAds = 0;
                                    Debug.Log("mysdk: sdkmanager wait show first ok");
                                }
                                else
                                {
                                    if (tcheckFirstShow >= AdsHelper.Instance.currConfig.OpenAdTimeWaitShowFirst)
                                    {
                                        if (AdsHelper.Instance.currConfig.OpenAdShowtype == 3)
                                        {
                                            tcheckFirstShow = 10000;
                                            Debug.Log("mysdk: sdkmanager doShowFirst Update4");
                                            doShowFirst();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (AdsHelper.Instance != null && AdsHelper.Instance.isShowFulled)
                {
                    tcheckFirstShow = 10000;
                }
            }
            if (stateShowFirstAds > 0)
            {
                if (stateShowFirstAds == 1)
                {
                    timeCount4ShowFirstAds += Time.unscaledDeltaTime;
                    if (timeCount4ShowFirstAds >= 1.0f)
                    {
                        stateShowFirstAds = 0;
                        Debug.Log("mysdk: sdkmanager doShowFirst Update5");
                        doShowFirst();
                    }
                }
            }
            if (flagTimeScale > 0 && Time.timeScale == 0)
            {
                flagTimeScale++;
                if (Input.GetMouseButton(0) && flagTimeScale > 200)
                {
                    SdkUtil.logd($"sdkmanager ads flagTimeScale resumegame");
                    flagTimeScale = 0;
                    Time.timeScale = 1;
                    PopupShowFirstAds.gameObject.SetActive(false);
                }
            }

            if (tShowWaitShowfull > 0)
            {
                tShowWaitShowfull -= Time.deltaTime * Time.timeScale;
                if (tShowWaitShowfull <= 0)
                {
                    tShowWaitShowfull = 0;
                    //PopupShowFirstAds.gameObject.SetActive(false);
                    AdsHelper.Instance.obWaitAd.SetActive(false);
                }
            }
        }

        public bool doShowFirst(bool isSetAllowShowFirst = false, bool isAllow = false)
        {
            Debug.Log($"mysdk: sdkmanager doShowFirst isSetAllowShowFirst={isSetAllowShowFirst}, isAllow={isAllow}, isAdsShowFirsted={isAdsShowFirsted}");
            if (!isAdsShowFirsted || AdsHelper.isRemoveAds(0))
            {
                if (AdsHelper.isRemoveAds(0))
                {
                    isAllowShowFirstOpen = false;
                }
                return false;
            }
            if (AdsHelper.Instance != null && !AdsHelper.Instance.isShowFulled)
            {
                if (isSetAllowShowFirst)
                {
                    isAllowShowFirstOpen = isAllow;
                }
                int cfSession4Show2f = PlayerPrefs.GetInt("cf_ss_show2_first", 1000);
                bool re = false;
                if (AdsHelper.Instance.isNewlogFirtOpen >= 0)
                {
                    int flagShowfo = AdsHelper.Instance.isNewlogFirtOpen;
                    if (flagShowfo >= 20)
                    {
                        flagShowfo = flagShowfo - 20;
                    }
                    re = GameHelper.Instance.showAppOpenAdFirst(flagShowfo, AdsHelper.Instance.currConfig.OpenAdFlagOpenFormat, AdsHelper.Instance.isNewlogFirtOpen < 20 && counSessionGame >= cfSession4Show2f);
                }
                if (re)
                {
                    Debug.Log($"mysdk: sdkmanager doShowFirst show newlogic={AdsHelper.Instance.isNewlogFirtOpen}");
                    tcheckFirstShow = 10000;
                    flagTimeScale = 1;
                    Time.timeScale = 0;
                    if (CBPauseGame != null)
                    {
                        CBPauseGame.Invoke(true);
                    }
                    return true;
                }
                else
                {
                    Debug.Log($"mysdk: sdkmanager doShowFirst not show newlogic={AdsHelper.Instance.isNewlogFirtOpen}");
                    PopupShowFirstAds.gameObject.SetActive(false);
                    flagTimeScale = 0;
                    Time.timeScale = 1;
                    return false;
                }
            }
            else
            {
                PopupShowFirstAds.gameObject.SetActive(false);
                Time.timeScale = 1;
                return false;
            }
        }

        public void onFirstAdShow()
        {
            tcheckFirstShow = 10000;
            isAdsShowFirsted = false;
        }

        public void showWait4ShowFull()
        {
            tShowWaitShowfull = 1.05f;
            //PopupShowFirstAds.gameObject.SetActive(true);
            //if (PopupShowFirstAds.transform.GetChild(1) != null)
            //{
            //    PopupShowFirstAds.transform.GetChild(1).gameObject.SetActive(true);
            //}
            AdsHelper.Instance.obWaitAd.SetActive(true);
        }

        public void showWaitCommon()
        {
            PopupShowFirstAds.gameObject.SetActive(true);
            if (PopupShowFirstAds.transform.GetChild(1) != null)
            {
                PopupShowFirstAds.transform.GetChild(1).gameObject.SetActive(false);
            }
        }

        public void closeWaitShowFull()
        {
            if (tShowWaitShowfull > 0)
            {
                tShowWaitShowfull = 0;
                //PopupShowFirstAds.gameObject.SetActive(false);
                AdsHelper.Instance.obWaitAd.SetActive(false);
            }
        }

        public void Screenshot(TakeScreenShootCallback cb)
        {
            Debug.Log("mysdk: sdkmanager Screenshot");
            _cb = cb;
            StartCoroutine(Take_a_screenshot());
        }

        private IEnumerator Take_a_screenshot()
        {
            yield return new WaitForEndOfFrame();
            var snap = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            snap.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            snap.Apply();
            _cb?.Invoke(snap);
        }

        public void showUpdate(int verup, int status, string gameid, string link = "", string title = "", string des = "")
        {
            if (updateGameController == null)
            {
                var prefab = Resources.Load<UpdateGameController>("Popup/PopupUpdate");
                updateGameController = Instantiate(prefab, Vector3.zero, canvasUpdate.rotation, canvasUpdate);
            }
            updateGameController.showUpdate(verup, status, gameid, link, title, des);
        }

        public void saveMoreGame()
        {
            Debug.Log("mysdk: sdkmanager saveMoreGame");
            string datamoregame = "";
            for (int i = 0; i < listMoreGame.Count; i++)
            {
                datamoregame += listMoreGame[i].ToString();
                if (i < (listMoreGame.Count - 1))
                {
                    datamoregame += ";";
                }
            }
            PlayerPrefs.SetString("my_more_game_data_mem", datamoregame);
        }

        public void loadMoreGame()
        {
            Debug.Log("mysdk: sdkmanager loadMoreGame");
            string datamoregame = PlayerPrefs.GetString("my_more_game_data_mem", "");
            string[] slist = datamoregame.Split(new char[] { ';' });
            foreach (string sgame in slist)
            {
                MoreGameOb more = new MoreGameOb();
                if (more.fromString(sgame))
                {
                    listMoreGame.Add(more);
                }
            }
        }

        public void downMoreGame()
        {
            Debug.Log("mysdk: sdkmanager downMoreGame");
            string btmore = PlayerPrefs.GetString("my_more_game_btmore_mem", "");
            if (btmore.Length > 10)
            {
                ImageLoader.loadImageTexture(btmore);
            }
            foreach (var game in listMoreGame)
            {
                if (game.isStatic == 1)
                {
                    DownLoadUtil.downloadImg(game.playAbleAds, "");
                }
                else
                {
                    if (game.playOnClient == 1)
                    {
                        DownLoadUtil.downloadText(game.playAbleAds, "");
                    }
                }
                ImageLoader.loadImageTexture(game.icon);
            }
        }

        private float deltaTime;
        int countLowFps = 0;
        int countLowFpsCon = 0;
        int countLowFpsCon5 = 0;
        int countVerylow = 0;
        int countVerylowCon = 0;
        int countHighFps = 0;
        float timeTurnEnter = 0;
        float time4countCon5 = 0;


        private void calculatorFps()
        {
            timeTurnEnter += (Time.deltaTime * Time.timeScale);
            if (timeTurnEnter >= 5)
            {
                if (AutoPerformanceOptimizer.Instance != null && !AutoPerformanceOptimizer.Instance.devicesIsLowEnd)
                {
                    deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
                    var fps = 1.0f / deltaTime;
                    if (fps < AutoPerformanceOptimizer.Instance.FPSDetectLowEnd)
                    {
                        countLowFps++;
                        countLowFpsCon++;
                        countLowFpsCon5++;
                        time4countCon5 = timeTurnEnter;
                        if (fps <= 15)
                        {
                            countVerylow++;
                            countVerylowCon++;
                        }
                        Debug.Log($"low={countLowFps} lowcon={countLowFpsCon} lowcon5={countLowFpsCon5} vlow={countVerylow} vlowcon={countVerylowCon} hfps={countHighFps}");
                        if (countVerylowCon >= 300 || countLowFpsCon >= 500 || countLowFpsCon5 >= 600 || (countLowFps > 1000 && countHighFps <= 200))
                        {
                            Debug.Log($"low=detect1");
                            AutoPerformanceOptimizer.Instance.ApplyLowEndSettings(4);
                        }
                        else
                        {
                            float tl = countLowFps * 100 / countHighFps;
                            float tlv = countVerylow * 100 / countHighFps;
                            if ((tl >= 20 || tlv >= 15) && (countLowFps + countHighFps) > 1000)
                            {
                                Debug.Log($"low=detect2");
                                AutoPerformanceOptimizer.Instance.ApplyLowEndSettings(5);
                            }
                        }
                    }
                    else
                    {
                        countHighFps++;
                        countLowFpsCon = 0;
                        countVerylowCon = 0;
                        if ((timeTurnEnter - time4countCon5) > 5)
                        {
                            countLowFpsCon5 = 0;
                        }
                    }
                }
            }
        }

        //==========================================================================================
        public void showNotAllowGame()
        {
            if (PopupNotAllowGame == null)
            {
                var prefab = Resources.Load<GameObject>("Popup/PopupNotAllowGame");
                PopupNotAllowGame = Instantiate(prefab, Vector3.zero, PopupShowFirstAds.transform.parent.rotation, PopupShowFirstAds.transform.parent);
            }
            PopupNotAllowGame.SetActive(true);
            RectTransform rc = PopupNotAllowGame.GetComponent<RectTransform>();
            rc.anchorMin = new Vector2(0, 0);
            rc.anchorMax = new Vector2(1, 1);
            rc.sizeDelta = new Vector2(0, 0);
            rc.anchoredPosition = Vector2.zero;
            rc.anchoredPosition3D = Vector3.zero;
        }

        public void showNotSupportIAP()
        {
            if (PopupNotSupportIAP == null)
            {
                var prefab = Resources.Load<GameObject>("Popup/PopupNotSupportIAP");
                PopupNotSupportIAP = Instantiate(prefab, Vector3.zero, PopupShowFirstAds.transform.parent.rotation, PopupShowFirstAds.transform.parent);
            }
            PopupNotSupportIAP.SetActive(true);
            RectTransform rc = PopupNotSupportIAP.GetComponent<RectTransform>();
            rc.anchorMin = new Vector2(0, 0);
            rc.anchorMax = new Vector2(1, 1);
            rc.sizeDelta = new Vector2(0, 0);
            rc.anchoredPosition = Vector2.zero;
            rc.anchoredPosition3D = Vector3.zero;
        }
        //==========================================================================================
    }
}