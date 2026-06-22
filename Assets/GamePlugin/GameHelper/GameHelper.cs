#define Nho_TrTh_Tao
#define Xem_Nga_Tao

using System;
using System.Globalization;
using Myapi;
using UnityEngine;

namespace mygame.sdk
{
    public enum Type_vibreate
    {
        Vib_Max = 0,
        Vib_Error,
        Vib_Success,
        Vib_Waring,
        Vib_Light,
        Vib_Medium,
        Vib_Heavy,
        Vib_Min,
    }

    public class GameHelper : MonoBehaviour
    {
        public static GameHelper Instance { get; private set; }

        public const string CountryDefault = "default";
        public const string KeyConfigVibrate = "key_config_vibrate";

        public string deviceid { get; set; }
        public string countryCode { get; private set; }
        public string languageCode { get; private set; }
        public string AdsIdentify { get; set; }
        public bool isAlowShowAppOpenAd { get; set; }

        private const long Day_len_Luc = 1755933745;
        private const int So_nga_xem = 2;
        static bool isShowCMP = false;
        private bool isOpenAdCheck4Lv = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                deviceid = SystemInfo.deviceUniqueIdentifier;
                countryCode = PlayerPrefs.GetString("mem_countryCode", "");
                languageCode = PlayerPrefs.GetString("mem_languageCode", MutilLanguage.langDefault);
                AdsIdentify = PlayerPrefs.GetString("key_ads_identify", "");
                isAlowShowAppOpenAd = true;
                if (AdsIdentify.StartsWith("0000000"))
                {
                    AdsIdentify = "";
                }
                else
                {
                    Debug.Log("mysdk: GameHelper AdsIdentify=" + AdsIdentify);
                }
                // if (languageCode == null || languageCode.Length <= 0 || string.Compare(languageCode, MutilLanguage.langDefault) == 0 || string.Compare(languageCode, "en") == 0)
                // {
                getLanguageCode();
                // }
                // if (languageCode == null || languageCode.Length <= 0 || string.Compare(languageCode, MutilLanguage.langDefault) == 0 || string.Compare(languageCode, "en") == 0)
                // {
                //     languageCode = CountryCodeUtil.getLanguageCodeFromCountryCode(countryCode);
                // }
                if (languageCode == null || languageCode.Length <= 0)
                {
                    languageCode = MutilLanguage.langDefault;
                }
                FIRhelper.checkGroupTier(countryCode);

                MutilLanguage.Instance().initRes();
            }
            else
            {
                //if (this != Instance) Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (countryCode == null || countryCode.Length <= 0 || string.Compare(countryCode, CountryDefault) == 0)
            {
                getCountryCode();
            }
            else
            {
                if (string.Compare(countryCode, "vn") != 0 && string.Compare(languageCode, "vi") != 0)
                {
                    Debug.Log("mysdk: GameHelper Start get countryCode mem={countryCode}");
                    getCountryCode();
                }
                else
                {
                    Debug.Log("mysdk: GameHelper Start countryCode=" + countryCode);
                    AdsHelper.Instance.FbntFullECPMdefault = AdsFbEcpmDF.getDefault(countryCode);
                    FIRhelper.checkGroupTier(countryCode);
                }
            }
            AdsHelper.Instance.configWithRegion(false);
            if (AdsIdentify.Length < 2)
            {
                getAdsIdentify();
            }
        }

        //===================================================================================================
        public static int getFlagVn()
        {
            int isVn = PlayerPrefs.GetInt("mem_dt_iam", 3);
            return isVn;
        }
        public void getLanguageCode()
        {
#if UNITY_EDITOR
            languageCode = MutilLanguage.langDefault;
#elif UNITY_ANDROID
            languageCode = mygame.plugin.Android.GameHelperAndroid.getLanguageCode();
#elif UNITY_IOS || UNITY_IPHONE
            languageCode = GameHelperIos.getLanguageCode();
#endif
            languageCode = languageCode.ToLower();
            Debug.Log("mysdk: GameHelper getLanguageCode=" + languageCode);
            PlayerPrefs.SetString("mem_languageCode", languageCode);
        }
        public void getCountryCode()
        {
#if UNITY_EDITOR
            countryCode = CountryDefault;
            //countryCode = "vn";//vvv
            checkCountryNoInapp();
#elif UNITY_ANDROID
            //mygame.plugin.Android.GameHelperAndroid.getCountryCode(false);
            countryCode = mygame.plugin.Android.GameHelperAndroid.getDetectedCountryCode();
            countryCode = countryCode.ToLower();
            Debug.Log("mysdk: GameHelper getCountryCode countryCode=" + countryCode);
            PlayerPrefs.SetString("mem_countryCode", countryCode);
            checkCountryNoInapp();
            AdsHelper.Instance.FbntFullECPMdefault = AdsFbEcpmDF.getDefault(countryCode);
            FIRhelper.checkGroupTier(countryCode);

            if (FIRhelper.Instance != null)
            {
                FIRhelper.Instance.checkcounty();
            }
#elif UNITY_IOS || UNITY_IPHONE
            countryCode = GameHelperIos.GetCountryCode();
            countryCode = countryCode.ToLower();
            Debug.Log("mysdk: GameHelper getCountryCode countryCode=" + countryCode);
            PlayerPrefs.SetString("mem_countryCode", countryCode);
            checkCountryNoInapp();
            AdsHelper.Instance.FbntFullECPMdefault = AdsFbEcpmDF.getDefault(countryCode);
            FIRhelper.checkGroupTier(countryCode);
#endif
            int isVn = getFlagVn();
            if (countryCode != null)
            {
                if (countryCode.CompareTo("vn") == 0 || countryCode.CompareTo("VN") == 0)
                {
                    isVn = 1;
                    PlayerPrefs.SetInt("mem_dt_iam", isVn);//
                    FIRhelper.logEvent($"check_isvna1{countryCode}_{isVn}");
                }
            }
            if (isVn != 1)
            {
#if UNITY_EDITOR
                isVn = 4;
#elif UNITY_ANDROID
                isVn = mygame.plugin.Android.GameHelperAndroid.isVn();
#elif UNITY_IOS || UNITY_IPHONE
                isVn = GameHelperIos.isVn();
#endif
                PlayerPrefs.SetInt("mem_dt_iam", isVn);//
                FIRhelper.logEvent($"check_isvna2{countryCode}_{isVn}");
            }
            Debug.Log("mysdk: GameHelper getCountryCode iam=" + isVn);
        }
        public void getAdsIdentify()
        {
            Debug.Log("mysdk: GameHelper call getAdsIdentify");
#if UNITY_EDITOR
#elif UNITY_ANDROID
            mygame.plugin.Android.GameHelperAndroid.getAdsIdentify();
#elif UNITY_IOS || UNITY_IPHONE
            AdsIdentify = GameHelperIos.GetAdsIdentify();
            if (AdsIdentify.StartsWith("0000000"))
            {
                AdsIdentify = "";
            } 
            else 
            {
                PlayerPrefs.SetString("key_ads_identify", AdsIdentify);
                //SingleTonApi<LogInstallApi, LogInstallOb>.Instance.check();
            }
            Debug.Log("mysdk: GameHelper AdsIdentify=" + AdsIdentify);
#endif
        }
        public bool isContainDeviceTest(string deviceId)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            return mygame.plugin.Android.GameHelperAndroid.isContainDeviceTest(deviceId);
#else
            return false;
#endif
        }
        public void rate()
        {
            //isAlowShowOpen = false;
            if (PlayerPrefsUtil.CF_RateAppReview)
            {
                appReview();
            }
            else
            {
                gotoStore(AppConfig.appid);
            }
            // int memverrate = PlayerPrefs.GetInt("mem_ver_rate", -1);
            // #if UNITY_ANDROID
            //             if (memverrate != AppConfig.verapp)
            //             {
            //                 appReview();
            //                 PlayerPrefs.SetInt("mem_ver_rate", AppConfig.verapp);
            //             }
            //             else
            //             {
            //                 gotoStore(Application.identifier);
            //             }
            // #elif UNITY_IOS || UNITY_IPHONE
            //             gotoStore(AppConfig.appid);
            // #endif      
        }

        public static int getFreeDisk()
        {
#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
            return 10000;
#else
            // return mygame.plugin.Android.GameHelperAndroid.getFreeDisk();
#endif
            return 10000;
        }

        public void gotoStore(string idapp)
        {
#if UNITY_ANDROID
            Application.OpenURL(string.Format(AppConfig.urlstore, idapp));
#elif UNITY_IOS || UNITY_IPHONE
            Application.OpenURL(string.Format(AppConfig.urlstore, idapp));
#endif
        }

        public void gotoLink(string url)
        {
            Application.OpenURL(url);
        }

        public string getlinkStore()
        {
#if UNITY_ANDROID
            return string.Format(AppConfig.urlstore, Application.identifier);
#elif UNITY_IOS || UNITY_IPHONE
            return string.Format(AppConfig.urlstore, AppConfig.appid);
#else
            return "";
#endif
        }

        public string getlinkHttpStore()
        {
#if UNITY_ANDROID
            return string.Format(AppConfig.urlstorehttp, Application.identifier);
#elif UNITY_IOS || UNITY_IPHONE
            return string.Format(AppConfig.urlstore, AppConfig.appid);
#else
            return "";
#endif
        }

        public void openFanpage(string pageid)
        {
#if UNITY_EDITOR
            Application.OpenURL("https://www.facebook.com/" + pageid);

#elif UNITY_IOS || UNITY_IPHONE
            Application.OpenURL("fb://profile/" + pageid);

#elif UNITY_ANDROID
            if(checkGameInstalled("com.facebook.katana")) {
                Application.OpenURL("fb://page/" + pageid);
            } else {
                Application.OpenURL("https://www.facebook.com/" + pageid); // no Facebook app - use built-in web browser
            }

#else
            Application.OpenURL("https://www.facebook.com/" + pageid);

#endif
        }

        public bool checkGameInstalled(string package)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return mygame.plugin.Android.GameHelperAndroid.checkGameInstalled(package);
#else
            return false;
#endif
        }

        public static void ChangeSettingVibrate(bool value)
        {
            PlayerPrefs.SetInt(KeyConfigVibrate, value ? 1 : 0);
        }

        /*
         * amply: type of vibrate
         *      = 0: Vibrate
         *      = 1: haptic error
         *      = 2: haptic success
         *      = 3: haptic warning
         *      = 4: haptic impactOccurred light
         *      = 5: haptic impactOccurred medium
         *      = 6: haptic impactOccurred heavy
         *      = 7: haptic impactOccurred selectionChanged
        */
        public void Vibrate(Type_vibreate type)
        {
            if (PlayerPrefs.GetInt(KeyConfigVibrate, 1) == 0)
            {
                return;
            }
            try
            {
#if UNITY_EDITOR
                Handheld.Vibrate();
#elif UNITY_ANDROID
                int amply = 70;
                int lenght = 40;
                if (type == Type_vibreate.Vib_Max) {
                    amply = 100;
                    lenght = 200;
                } else if (type == Type_vibreate.Vib_Error) {
                    amply = -1;
                    lenght = 70;
                } else if (type == Type_vibreate.Vib_Success) {
                    amply = -1;
                    lenght = 60;
                } else if (type == Type_vibreate.Vib_Waring) {
                    amply = -1;
                    lenght = 50;
                } else if (type == Type_vibreate.Vib_Light) {
                    amply = 60;
                    lenght = 35;
                } else if (type == Type_vibreate.Vib_Medium) {
                    amply = 60;
                    lenght = 35;
                } else if (type == Type_vibreate.Vib_Heavy) {
                    amply = 100;
                    lenght = 70;
                } else {
                    amply = 60;
                    lenght = 35;
                }
                mygame.plugin.Android.GameHelperAndroid.Vibrate(amply, lenght);
#elif UNITY_IOS || UNITY_IPHONE
                GameHelperIos.vibrate((int)type);
#endif
            }
            catch (Exception)
            {

            }
        }

        public static int pushNotify(int timeFireInseconds, string title, string msg)
        {
            Debug.Log($"mysdk: GameHelper pushNotify when={timeFireInseconds}, title={title}, msg={msg}");
#if UNITY_ANDROID && ! UNITY_EDITOR
            return mygame.plugin.Android.GameHelperAndroid.pushNotify(timeFireInseconds, title, msg);
#elif UNITY_IOS || UNITY_IPHONE
            return GameHelperIos.pushNotify(timeFireInseconds, title, msg);
#else
            Debug.Log("No sharing set up for this platform.");
            return -1;
#endif
        }

        public static void cancelNoti(string ids)
        {
            Debug.Log($"mysdk: GameHelper cancelNoti idsNoti={ids}");
#if UNITY_ANDROID && !UNITY_EDITOR
            mygame.plugin.Android.GameHelperAndroid.cancelNoti(ids);
#elif UNITY_IOS || UNITY_IPHONE
            GameHelperIos.cancelNoti(ids);
#else
            Debug.Log("No sharing set up for this platform.");
#endif
        }

        public bool showAppOpenAdFirst(int flagShow, int flagShowFormatOpen, bool allow2 = false)
        {
            Debug.Log($"mysdk: ads open GameHelper showAppOpenAdFirst isAlowShowAppOpenAd={isAlowShowAppOpenAd} flagShow={flagShow} low2={allow2}");
            if (isAlowShowAppOpenAd && AdsHelper.Instance != null && AdsHelper.Instance.isShowOpenAds(true) > 0)
            {
#if UNITY_EDITOR
                return false;
#elif UNITY_ANDROID
                bool isshow = false;
                if (flagShow == 1 || flagShow == 2)
                {
                    isshow = AdsHelper.Instance.adsAdmobMyLower.showFull(AdsBase.PLFullSplash, 0, false, (satead) =>
                    {
                        if (satead == AD_State.AD_SHOW)
                        {
                            SDKManager.Instance.PopupShowFirstAds.gameObject.SetActive(false);
                        }
                        if (satead == AD_State.AD_CLOSE || satead == AD_State.AD_SHOW_FAIL)
                        {
                            SDKManager.Instance.PopupShowFirstAds.gameObject.SetActive(false);
                            SDKManager.Instance.flagTimeScale = 0;
                            Time.timeScale = 1;
                            if (SDKManager.Instance.CBPauseGame != null)
                            {
                                SDKManager.Instance.CBPauseGame.Invoke(false);
                            }
                        }
                    });
                }

                if (!isshow && flagShow > 1)
                {
                    int timg = flagShow - 3;
                    isshow = AdsHelper.Instance.showFull(AdsBase.PLFullSplash, GameRes.GetLevel(Level_type.Common), 0, timg, 0, false, false, false, (satead) =>
                    {
                        if (satead == AD_State.AD_SHOW)
                        {
                            SDKManager.Instance.PopupShowFirstAds.gameObject.SetActive(false);
                        }
                        if (satead == AD_State.AD_CLOSE || satead == AD_State.AD_SHOW_FAIL || satead == AD_State.AD_CLOSE2 || satead == AD_State.AD_SHOW_FAIL2)
                        {
                            SDKManager.Instance.PopupShowFirstAds.gameObject.SetActive(false);
                            SDKManager.Instance.flagTimeScale = 0;
                            Time.timeScale = 1;
                            if (SDKManager.Instance.CBPauseGame != null)
                            {
                                SDKManager.Instance.CBPauseGame.Invoke(false);
                            }
                        }
                    }, false, allow2);
                }
                if (!isshow && flagShowFormatOpen == 0)
                {
                    SDKManager.Instance.currPlacement = AdsBase.PLFullSplash;
                    isshow = mygame.plugin.Android.GameHelperAndroid.showAppOpenAd();
                }
                return isshow;
#elif UNITY_IOS || UNITY_IPHONE
                bool isshow = false;
                if (flagShow == 1 || flagShow == 2)
                {
                    isshow = AdsHelper.Instance.adsAdmobMyLower.showFull(AdsBase.PLFullSplash, 0, false, (satead) =>
                    {
                        if (satead == AD_State.AD_SHOW)
                        {
                            SDKManager.Instance.PopupShowFirstAds.gameObject.SetActive(false);
                        }
                        if (satead == AD_State.AD_CLOSE || satead == AD_State.AD_SHOW_FAIL)
                        {
                            SDKManager.Instance.PopupShowFirstAds.gameObject.SetActive(false);
                            SDKManager.Instance.flagTimeScale = 0;
                            Time.timeScale = 1;
                            if (SDKManager.Instance.CBPauseGame != null)
                            {
                                SDKManager.Instance.CBPauseGame.Invoke(false);
                            }
                        }
                    });
                }
                if (!isshow && flagShow > 1)
                {
                    int timg = flagShow - 3;
                    isshow = AdsHelper.Instance.showFull(AdsBase.PLFullSplash, GameRes.GetLevel(Level_type.Common), 0, timg, 0, false, false, false, (satead) =>
                    {
                        if (satead == AD_State.AD_SHOW)
                        {
                            SDKManager.Instance.PopupShowFirstAds.gameObject.SetActive(false);
                        }
                        if (satead == AD_State.AD_CLOSE || satead == AD_State.AD_SHOW_FAIL || satead == AD_State.AD_CLOSE2 || satead == AD_State.AD_SHOW_FAIL2)
                        {
                            SDKManager.Instance.PopupShowFirstAds.gameObject.SetActive(false);
                            SDKManager.Instance.flagTimeScale = 0;
                            Time.timeScale = 1;
                            if (SDKManager.Instance.CBPauseGame != null)
                            {
                                SDKManager.Instance.CBPauseGame.Invoke(false);
                            }
                        }
                    }, false, allow2);
                }
                if (!isshow && flagShowFormatOpen == 0)
                {
                    SDKManager.Instance.currPlacement = AdsBase.PLFullSplash;
                    isshow = GameHelperIos.showAppOpenAd();
                }
                return isshow;
#endif
            }
            else
            {
                isAlowShowAppOpenAd = true;
                return false;
            }
        }

        public void configAppOpenAd(int timegb)
        {
#if UNITY_EDITOR
            return;
#elif UNITY_ANDROID
            mygame.plugin.Android.GameHelperAndroid.configAppOpenAd(timegb, AppConfig.AppOpenAdOrien);
#elif UNITY_IOS || UNITY_IPHONE
            GameHelperIos.configAppOpenAd(timegb, AppConfig.AppOpenAdOrien);
#endif
        }
        public void loadAppOpenAd()
        {
#if UNITY_EDITOR
            return;
#elif UNITY_ANDROID
            string appOpenAdId = AdsHelper.Instance.OpenAdIdAndroid;
            mygame.plugin.Android.GameHelperAndroid.loadAppOpenAd(appOpenAdId);
#elif UNITY_IOS || UNITY_IPHONE
            string appOpenAdId = AdsHelper.Instance.OpenAdIdiOS;
            GameHelperIos.loadAppOpenAd(appOpenAdId);
#endif
        }

        public bool isAppOpenAdLoaded()
        {
#if UNITY_EDITOR
            return false;
#elif UNITY_ANDROID
            bool re = mygame.plugin.Android.GameHelperAndroid.isAppOpenAdLoaded();
            return re;
#elif UNITY_IOS || UNITY_IPHONE
            bool re = GameHelperIos.isAppOpenAdLoaded();
            return re;
#endif
        }

        public bool showAppOpenAd()
        {
#if UNITY_EDITOR
            return false;
#elif UNITY_ANDROID
            bool re = mygame.plugin.Android.GameHelperAndroid.showAppOpenAd();
            return re;
#elif UNITY_IOS || UNITY_IPHONE
            bool re = GameHelperIos.showAppOpenAd();
            return re;
#endif
        }

        public bool appReview()
        {
#if UNITY_EDITOR
            return false;
#endif
            try
            {
#if UNITY_ANDROID
                mygame.plugin.Android.GameHelperAndroid.appReview();
                return true;
#elif UNITY_IOS || UNITY_IPHONE
                bool re = GameHelperIos.appReview();
                return re;
#endif
            }
            catch (Exception)
            {

            }
            return false;
        }

        public static bool isRequestIDFA()
        {
            int dfLvShow = 0;
#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
            dfLvShow = AppConfig.LevelShowAttracking;
#endif
            int lvshowrequest = PlayerPrefs.GetInt("lv_show_request_idfa", dfLvShow);
            Debug.Log("mysdk: GameHelper requestIDFA lvshowrequest=" + lvshowrequest);
            if (lvshowrequest < 0)
            {
                return false;
            }
            if (GameRes.LevelCommon() >= lvshowrequest)
            {
#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
                return true;
#endif
            }
            return false;
        }

        public static void requestIDFA()
        {
            PlayerPrefs.SetInt("lv_show_request_idfa", -1);
#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
            int isallversion = PlayerPrefs.GetInt("cf_ver_os_show_idfa", 1);
            int vergameshowidfa = PlayerPrefs.GetInt("cf_ver_game_show_idfa", 0);
            if (AppConfig.verapp < vergameshowidfa)
            {
                isallversion = 0;
            }
            Debug.Log("mysdk: GameHelper requestIDFA call isallversion=" + isallversion + ", vergameshowidfa=" + vergameshowidfa);
            GameHelperIos.requestIDFA(isallversion);
#endif
        }

        public static bool showCMP()
        {
            if (isShowCMP || PlayerPrefs.GetInt("mem_show_CMP", 0) > 0)
            {
                GameAdsHelperBridge.unityOnNotShowCMP();
                return false;
            }
            isShowCMP = true;
#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
            mygame.sdk.GameHelperIos.showCMP();
#elif UNITY_ANDROID && !UNITY_EDITOR
            mygame.plugin.Android.GameHelperAndroid.showCMP(false);
#endif
            return true;
        }

        public static bool deviceIsRooted()
        {
#if UNITY_EDITOR
            return false;
#elif UNITY_ANDROID
            return mygame.plugin.Android.GameHelperAndroid.deviceIsRooted();
#endif
            return false;
        }

        public static float getScreenWidth()
        {
#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
            return GameHelperIos.getScreenWidth();
#else
            return Screen.width;
#endif
        }

        public static void switchFlash(bool isOn)
        {
#if !UNITY_EDITOR

#if (UNITY_IOS || UNITY_IPHONE)
            mygame.sdk.GameHelperIos.switchFlash(isOn);
#else
            mygame.plugin.Android.GameHelperAndroid.switchFlash(isOn);
#endif

#endif
        }

        public static long CurrentTimeMilisReal()
        {
#if AllowCustomeTime
            return SdkUtil.CurrentTimeMilis();
#endif
#if !UNITY_EDITOR

#if UNITY_IOS || UNITY_IPHONE
            return mygame.sdk.GameHelperIos.CurrentTimeMilisReal();
#else
            long tcur = mygame.plugin.Android.GameHelperAndroid.CurrentTimeMilisReal();
            return tcur;
#endif

#else
            return SdkUtil.CurrentTimeMilis();
#endif
        }

        public static void ScreenInfo()
        {
#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
            //mygame.sdk.GameHelperIos.ScreenInfo(isOn);
#else
            //mygame.plugin.Android.GameHelperAndroid.ScreenInfo();
#endif
        }

        public static bool checkLvXaDu()
        {
#if UNITY_IOS || UNITY_IPHONE
            int memsen = 0;
            int TT_Xadu = PlayerPrefs.GetInt("cf_trathai_xadu", 0);
#if Nho_TrTh_Tao
            if (TT_Xadu != 100)
            {
                memsen = PlayerPrefs.GetInt("nho_tt_ktro", 0);
            }
#endif

            if (AppConfig.khong_check == 5)
            {
                memsen = 1001;
            }
            if (memsen == 1001)
            {
                return true;
            }
            else
            {
                int deban = AppConfig.verapp - 1;
                long dt = -1000;
                int lvban = PlayerPrefs.GetInt("cf_lv_ktro", deban);
                Debug.Log($"mysdk: GameHelper checkLvXaDu ban={lvban}");
#if Xem_Nga_Tao
                if (TT_Xadu != 100 && TT_Xadu != 99)
                {
                    long tcurr = CurrentTimeMilisReal() / 1000;
                    dt = tcurr - Day_len_Luc;
                    Debug.Log($"mysdk: GameHelper checkLvXaDu dt={dt}");
                }
#endif
                if (dt >= So_nga_xem * 24 * 3600 || lvban >= (deban + 1))
                {
                    PlayerPrefs.SetInt("nho_tt_ktro", 1001);
                    return true;
                }
                else
                {
                    return false;
                }
            }
#else
            return true;
#endif
        }

        public static int getFreeMem()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return mygame.plugin.Android.GameHelperAndroid.getFreeMem();
#else
            return 10000;
#endif
        }

        public static void cfCheckAdErr(bool isCheck)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            mygame.plugin.Android.GameHelperAndroid.cfCheckAdErr(isCheck);
#elif !UNITY_EDITOR
            
#endif
        }

        public static bool deleteImagesFromImessage(string[] listNames, string groupName)
        {
#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
            return GameHelperIos.deleteImagesFromImessage(listNames, groupName);
#endif
            return false;
        }
        public static bool deleteImageFromImessage(string listName, string groupName)
        {
#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
            return GameHelperIos.deleteImageFromImessage(listName, groupName);
#endif
            return false;
        }
        public static bool shareImages2Imessage(string[] listNames, int[] lenDatas, byte[] datas, string nameGroup)
        {
#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
            return GameHelperIos.shareImages2Imessage(listNames, lenDatas, datas, nameGroup);
#endif
            return false;
        }
        public static bool shareImage2Imessage(byte[] data, string nameImg, string nameGroup)
        {
#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
            return GameHelperIos.shareImage2Imessage(data, nameImg, nameGroup);
#endif
            return false;
        }
        //================================================================================================
        public void checkCountryNoInapp()
        {
            string listctry = PlayerPrefs.GetString("key_country_noinapp", "");
        }

        //================================================================================================
#if UNITY_ANDROID
        public void HandleGetCountryCode(string description)
        {
            string oldcc = countryCode;
            countryCode = description;
            countryCode = countryCode.ToLower();
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("mysdk: GameHelper HandleGetCountryCode countryCode=" + countryCode);
                PlayerPrefs.SetString("mem_countryCode", countryCode);
                if (FIRhelper.Instance != null)
                {
                    FIRhelper.Instance.checkcounty();
                }
                if (oldcc != null && oldcc.CompareTo(countryCode) != 0)
                {
                    AdsHelper.Instance.configWithRegion(false);
                }
                checkCountryNoInapp();
                AdsHelper.Instance.FbntFullECPMdefault = AdsFbEcpmDF.getDefault(countryCode);
                FIRhelper.checkGroupTier(countryCode);
            });
        }

        public void HandleGetAdsIdentify(string description)
        {
            AdsIdentify = description;
            Debug.Log("mysdk: GameHelper HandleGetAdsIdentify AdsIdentify=" + AdsIdentify);
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                PlayerPrefs.SetString("key_ads_identify", AdsIdentify);
            });

        }
#endif
    }
}
