#if UNITY_IOS || UNITY_IPHONE

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace mygame.sdk
{
    public class GameHelperIos
    {
        [DllImport("__Internal")] private static extern int isVnNative();
        public static int isVn()
        {
            return isVnNative();
        }

        [DllImport("__Internal")] private static extern string getLanguageCodeNative();
        public static string getLanguageCode()
        {
            return getLanguageCodeNative();
        }

        [DllImport("__Internal")] private static extern string getCountryCodeNative();
        public static string GetCountryCode()
        {
            return getCountryCodeNative();
        }

        [DllImport("__Internal")] private static extern string getAdsIdentifyNative();
        public static string GetAdsIdentify()
        {
            return getAdsIdentifyNative();
        }

        [DllImport("__Internal")] private static extern long CurrentTimeMilisRealNative();
        public static long CurrentTimeMilisReal()
        {
            return (CurrentTimeMilisRealNative() * 1000);
        }

        [DllImport("__Internal")] private static extern string getGiftBoxNative();
        public static string GetGiftBox()
        {
            return getGiftBoxNative();
        }

        [DllImport("__Internal")] private static extern void vibrateNative(int type);
        public static void vibrate(int type)
        {
            vibrateNative(type);
        }

        [DllImport("__Internal")] private static extern void switchFlashNative(bool isOn);
        public static void switchFlash(bool isOn)
        {
            switchFlashNative(isOn);
        }

        [DllImport("__Internal")] private static extern long getMemoryLimit();
        public static long GetMemoryLimit()
        {
            return getMemoryLimit();
        }

        [DllImport("__Internal")] private static extern long getPhysicMemoryInfo();
        public static long GetPhysicMemoryInfo()
        {
            return getPhysicMemoryInfo();
        }

        [DllImport("__Internal")] private static extern float getScreenWidthNative();
        public static float getScreenWidth()
        {
            return getScreenWidthNative();
        }

        [DllImport("__Internal")] private static extern void configAppOpenAdNative(int timebg, int orien);
        public static void configAppOpenAd(int timebg, int orien)
        {
            configAppOpenAdNative(timebg, orien);
        }
        [DllImport("__Internal")] private static extern void loadAppOpenAdNative(string iappOpenAdId);
        public static void loadAppOpenAd(string appOpenAdId)
        {
            loadAppOpenAdNative(appOpenAdId);
        }
        [DllImport("__Internal")] private static extern bool isAppOpenAdLoadedNative();
        public static bool isAppOpenAdLoaded()
        {
            return isAppOpenAdLoadedNative();
        }
        [DllImport("__Internal")] private static extern bool showAppOpenAdNative();
        public static bool showAppOpenAd()
        {
            return showAppOpenAdNative();
        }

        [DllImport("__Internal")] private static extern bool appReviewNative();
        public static bool appReview()
        {
            return appReviewNative();
        }

        [DllImport("__Internal")] private static extern bool requestIDFANative(int isallversion);
        public static void requestIDFA(int isallversion)
        {
            requestIDFANative(isallversion);
        }

        [DllImport("__Internal")] private static extern void showCMPNative();
        public static void showCMP()
        {
            showCMPNative();
        }

        [DllImport("__Internal")] private static extern void localNotifyNative(string title, string msg, int hour, int minus, int dayrepeat);
        public static void localNotify(string title, string msg, int hour, int minus, int dayrepeat)
        {
            localNotifyNative(title, msg, hour, minus, dayrepeat);
        }

        [DllImport("__Internal")]
        private static extern void clearAllNotiNative();
        public static void clearAllNoti()
        {
            clearAllNotiNative();
        }

        [DllImport("__Internal")]
        private static extern bool deleteImagesFromImessageNative(int countItem, string[] listNames, string groupName);
        public static bool deleteImagesFromImessage(string[] listNames, string groupName)
        {
            return deleteImagesFromImessageNative(listNames.Length, listNames, groupName);
        }

        [DllImport("__Internal")]
        private static extern bool deleteImageFromImessageNative(string listName, string groupName);
        public static bool deleteImageFromImessage(string listName, string groupName)
        {
            return deleteImageFromImessageNative(listName, groupName);
        }

        [DllImport("__Internal")]
        private static extern bool shareImages2ImessageNative(int countItem, string[] listNames, int[] lenItemData, byte[] data, int lenData, string nameGroup);
        public static bool shareImages2Imessage(string[] listNames, int[] lenItemData, byte[] data, string nameGroup)
        {
            return shareImages2ImessageNative(listNames.Length, listNames, lenItemData, data, data.Length, nameGroup);
        }

        [DllImport("__Internal")]
        private static extern bool shareImage2ImessageNative(byte[] data, int lenData, string nameImg, string nameGroup);
        public static bool shareImage2Imessage(byte[] data, string nameImg, string nameGroup)
        {
            return shareImage2ImessageNative(data, data.Length, nameImg, nameGroup);
        }

        [DllImport("__Internal")]
        private static extern int pushNotifyNative(int timeFireInseconds, string title, string msg);
        public static int pushNotify(int timeFireInseconds, string title, string msg)
        {
            return pushNotifyNative(timeFireInseconds, title, msg);
        }
        
        [DllImport("__Internal")]
        private static extern void cancelNotiNative(string ids);
        public static void cancelNoti(string ids)
        {
            cancelNotiNative(ids);
        }

        [DllImport("__Internal")]
        private static extern void _SetKeychainValue(string key, string va);
        public static void SetKeychainValue(string key, string va)
        {
            _SetKeychainValue(key, va);
        }

        [DllImport("__Internal")]
        private static extern string _GetKeychainValue(string key);
        public static string GetKeychainValue(string key)
        {
            return _GetKeychainValue(key);
        }
        
    }
}

#endif
