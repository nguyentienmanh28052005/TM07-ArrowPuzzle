#if UNITY_ANDROID

using System;
using mygame.sdk;
using UnityEngine;

namespace mygame.plugin.Android
{
    public class GameHelperAndroid
    {
        public static void Share(string body, string subject, string url, string[] filePaths, string mimeType, bool chooser, string chooserText)
        {
            Debug.Log("mysdk ShareAndroid 1");
            using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
            using (AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent"))
            {
                Debug.Log("mysdk ShareAndroid 2");
                using (intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND")))
                { }
                Debug.Log("mysdk ShareAndroid 23");
                using (intentObject.Call<AndroidJavaObject>("setType", mimeType))
                { }
                Debug.Log("mysdk ShareAndroid 24");
                using (intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), subject))
                { }
                Debug.Log("mysdk ShareAndroid 25");
                using (intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), body))
                { }
                Debug.Log("mysdk ShareAndroid 3");
                if (!string.IsNullOrEmpty(url))
                {
                    Debug.Log("mysdk ShareAndroid 31");
                    // attach url
                    using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
                    using (AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", url))
                    using (intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject))
                    { }
                }
                Debug.Log("mysdk ShareAndroid 4");
                // finally start application
                using (AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    if (chooser)
                    {
                        Debug.Log("mysdk ShareAndroid 31");
                        AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, chooserText);
                        currentActivity.Call("startActivity", jChooser);
                    }
                    else
                    {
                        Debug.Log("mysdk ShareAndroid 32");
                        currentActivity.Call("startActivity", intentObject);
                    }
                }
            }
        }

        public static int isVn()
        {
            int re = 0;
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        re = gameUtil.CallStatic<int>("isVn", activity);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid isVn ERROR: " + e);
            }


            return re;
        }

        public static string getCountryCode(bool isRequestPermission)
        {
            string re = "";
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        re = gameUtil.CallStatic<string>("getCountryCode", activity, isRequestPermission);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid getCountryCode ERROR: " + e);
            }
            return re;
        }

        public static string getDetectedCountryCode()
        {
            string re = "";
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        re = gameUtil.CallStatic<string>("getDetectedCountryCode", activity);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid getDetectedCountryCode ERROR: " + e);
            }
            return re;
        }

        public static string getLanguageCode()
        {
            string re = "";
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        re = gameUtil.CallStatic<string>("getLanguageCode", activity);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid getLanguageCode ERROR: " + e);
            }
            return re;
        }

        public static void getAdsIdentify()
        {
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        gameUtil.CallStatic("getAdsIdentify", activity);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid getAdsIdentify ERROR: " + e);
            }
        }

        public static bool isContainDeviceTest(string deviceId)
        {
            bool re = false;
            try
            {
                using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                {
                    re = gameUtil.CallStatic<bool>("isContainDeviceId", deviceId);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid isContainDeviceTest ERROR: " + e);
            }
            return re;
        }

        public static bool checkGameInstalled(string pkgName)
        {
            try
            {
                using (AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager"))
                {
                    AndroidJavaObject launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", pkgName);
                    return launchIntent != null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid checkGameInstalled ERROR: " + e);
                return false;
            }
        }

        public static void Vibrate(int amply, int lenght)
        {
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        gameUtil.CallStatic("vibrate", activity, amply, lenght);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid Vibrate ERROR: " + e);
            }
        }

        public static void configAppOpenAd(int timeBg, int orien)
        {
            try
            {
                using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                {
                    int fff = mygame.sdk.FIRhelper.Instance.isAdSkip;
                    if (fff != 97 && AdsHelper.Instance != null)
                    {
                        if (AdsHelper.Instance.isApplyLogicSkip == 0 || AdsHelper.Instance.isApplyLogicSkip == 1)
                        {
                            fff = 97;
                        }
                    }
                    gameUtil.CallStatic("configAppOpenAd", timeBg, fff);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid configAppOpenAd ERROR: " + e);
            }
        }
        public static void loadAppOpenAd(string adUnitId)
        {
            try
            {
                using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                {
                    gameUtil.CallStatic("loadAppOpenAd", adUnitId);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid loadAppOpenAd ERROR: " + e);
            }
        }

        public static bool isAppOpenAdLoaded()
        {
            try
            {
                using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                {
                    return gameUtil.CallStatic<bool>("isAppOpenAdLoaded");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid isAppOpenAdLoaded ERROR: " + e);
                return false;
            }
        }
        public static bool showAppOpenAd()
        {
            try
            {
                using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                {
                    return gameUtil.CallStatic<bool>("showAppOpenAd");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid showAppOpenAd ERROR: " + e);
                return false;
            }
        }

        public static void appReview()
        {
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        gameUtil.CallStatic("appReview", activity);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid appReview ERROR: " + e);
            }
        }

        public static void showCMP(bool istest = false)
        {
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        gameUtil.CallStatic("ShowconsentCMP", activity, istest);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid showCMP ERROR: " + e);
            }
        }

        public static void fixbugBanner()
        {
            try
            {
                using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                {
                    gameUtil.CallStatic("fixBannerBug");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid fixbugBanner ERROR: " + e);
            }
        }

        public static bool deviceIsRooted()
        {
            try
            {
                using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                {
                    bool isDvZin = gameUtil.CallStatic<bool>("xemMayZin");
                    return !isDvZin;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid deviceIsRooted ERROR: " + e);
                return false;
            }
        }

        public static bool isInstallFromGooglePlay()
        {
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        return gameUtil.CallStatic<bool>("checkCaitugl", activity);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid isInstallFromGooglePlay ERROR: " + e);
                return false;
            }
        }

        public static void checkPiraCheck(int flag)
        {
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        byte[] makey = { 106, 69, 88, 82, 66, 67, 100, 59, 86, 95, 105, 61, 116, 68, 102, 111, 117, 90, 104, 108, 78, 76, 119, 56, 53, 51, 77, 61 };
                        int[] paskey = { -1, -1, 1, 1, 5, 2, 10, 2, 6, 7, 13, 7, 13, 16, 16 };
                        byte[] pasva = { 5, 16, 13, 1, 18, 7, 12, 16, 12, 1, 8, 16, 3, 1, 0 };
                        string pkey = mygame.sdk.SdkUtil.myGiaima(makey, paskey, pasva);
                        gameUtil.CallStatic("piraCheck", activity, pkey, flag);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid checkPiraCheck ERROR: " + e);
            }
        }

        public static void printSigning()
        {
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        gameUtil.CallStatic("printSigngame", activity);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid printSigning ERROR: " + e);
            }
        }

        public static int getFreeMem()
        {
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        return gameUtil.CallStatic<int>("getFreeMem", activity);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid getFreeMem ERROR: " + e);
                return 10000;
            }
        }

        public static void setupEnviromentNotify(string nameAc = "com.unity3d.player.UnityPlayer", string nameIcon = "app_icon")
        {
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        gameUtil.CallStatic("setupEnviromentNotify", activity, nameAc, nameIcon);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid setupEnviromentNotify ERROR: " + e);
            }
        }

        public static void setupLocalNotifyNotify(string dataNoti)
        {
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        gameUtil.CallStatic("setupPushNotify", activity, dataNoti);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid setupLocalNotifyNotify ERROR: " + e);
            }
        }

        public static int pushNotify(int timeFireInseconds, string title, string msg)
        {
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        return gameUtil.CallStatic<int>("pushNoti", activity, title, msg, timeFireInseconds);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid pushNotify ERROR: " + e);
                return -1;
            }
        }

        public static void cancelNoti(string ids)
        {
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        gameUtil.CallStatic("cancelNoti", activity, ids);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid cancelNoti ERROR: " + e);
            }
        }

        public static void switchFlash(bool isOn)
        {
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        gameUtil.CallStatic("switchFlash", activity, isOn);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid switchFlash ERROR: " + e);
            }
        }

        public static void ScreenInfo()
        {
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                    {
                        gameUtil.CallStatic("ScreenInfo", activity);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid ScreenInfo ERROR: " + e);
            }
        }

        public static long CurrentTimeMilisReal()
        {
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.TimeUtil"))
                    {
                        return gameUtil.CallStatic<long>("getTimeLocal", activity);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid CurrentTimeMilisReal ERROR: " + e);
                return SdkUtil.CurrentTimeMilis();
            }
        }

        public static void sendFlagCheckMaxAdsErr()
        {
            try
            {
                using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                {
                    gameUtil.CallStatic("cbCheckAdErr");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid sendFlagCheckMaxAdsErr ERROR: " + e);
            }
        }

        public static void cfCheckAdErr(bool isCheck)
        {
            try
            {
                using (AndroidJavaClass gameUtil = new AndroidJavaClass("mygame.plugin.util.GameUtil"))
                {
                    gameUtil.CallStatic("cfCheckAdErr", isCheck);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("mysdk GameHelperAndroid cfCheckAdErr ERROR: " + e);
            }
        }
    }
}

#endif