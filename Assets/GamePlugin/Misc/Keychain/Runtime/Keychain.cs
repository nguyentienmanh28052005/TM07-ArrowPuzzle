using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class Keychain
{
    private const string prefsName = AppConfig.appid;
    private static AndroidJavaObject keychainJavaObject; // Android
    private static IntPtr iosManager; // iOS

    private static bool initialized = false;

    private static void Initialize()
    {
        if (initialized) return;
#if UNITY_EDITOR
#elif UNITY_ANDROID 
        if (keychainJavaObject != null)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            keychainJavaObject = new AndroidJavaObject("com.game.plugin.misc.KeychainHelper", activity, prefsName);
        }
#elif UNITY_IOS
        if (iosManager == System.IntPtr.Zero)
        {
            iosManager = Init(prefsName);
        }
#endif
        initialized = true;
    }

    // Lưu dữ liệu kiểu String
    public static void SaveString(string key, string value)
    {
        Initialize();
#if UNITY_EDITOR
        PlayerPrefs.SetString(key, value);
#elif UNITY_ANDROID 
        if (keychainJavaObject != null)
        {
            keychainJavaObject.Call("saveString", key, value);
        }
#elif UNITY_IOS
        if (iosManager != System.IntPtr.Zero)
        {
            SaveString(iosManager, key, value);
        }
#endif
    }

    // Lấy dữ liệu kiểu String
    public static string GetString(string key, string defaultValue)
    {
        Initialize();
#if UNITY_EDITOR
        return PlayerPrefs.GetString(key, defaultValue);
#elif UNITY_ANDROID 
        if (keychainJavaObject != null)
        {
            return keychainJavaObject.Call<string>("getString", key, defaultValue);
        }
#elif UNITY_IOS
        if (iosManager != System.IntPtr.Zero)
        {
            return Marshal.PtrToStringAnsi(GetString(iosManager, key, defaultValue));
        }
#endif
        return defaultValue;
    }

    // Lưu dữ liệu kiểu int
    public static void SaveInt(string key, int value)
    {
        Initialize();
#if UNITY_EDITOR
        PlayerPrefs.SetInt(key, value);
#elif UNITY_ANDROID 
        if (keychainJavaObject != null)
        {
            keychainJavaObject.Call("saveInt", key, value);
        }
#elif UNITY_IOS
        if (iosManager != System.IntPtr.Zero)
        {
            SaveInt(iosManager, key, value);
        }
#endif
    }

    // Lấy dữ liệu kiểu int
    public static int GetInt(string key, int defaultValue)
    {
        Initialize();

#if UNITY_EDITOR
        PlayerPrefs.GetInt(key, defaultValue);
#elif UNITY_ANDROID 
        if (keychainJavaObject != null)
        {
            return keychainJavaObject.Call<int>("getInt", key, defaultValue);
        }
#elif UNITY_IOS
        if (iosManager != System.IntPtr.Zero)
        {
            return GetInt(iosManager, key, defaultValue);
        }
#endif
        return defaultValue;
    }
    
    // Lưu dữ liệu kiểu float
    public static void SaveFloat(string key, int value)
    {
        Initialize();
#if UNITY_EDITOR
        PlayerPrefs.SetFloat(key, value);
#elif UNITY_ANDROID 
        if (keychainJavaObject != null)
        {
            keychainJavaObject.Call("saveFloat", key, value);
        }
#elif UNITY_IOS
        if (iosManager != System.IntPtr.Zero)
        {
            SaveFloat(iosManager, key, value);
        }
#endif
    }

    // Lấy dữ liệu kiểu float
    public static int GetFloat(string key, int defaultValue)
    {
        Initialize();

#if UNITY_EDITOR
        PlayerPrefs.GetFloat(key, defaultValue);
#elif UNITY_ANDROID 
        if (keychainJavaObject != null)
        {
            return keychainJavaObject.Call<int>("getFloat", key, defaultValue);
        }
#elif UNITY_IOS
        if (iosManager != System.IntPtr.Zero)
        {
            return GetFloat(iosManager, key, defaultValue);
        }
#endif
        return defaultValue;
    }

    // Xóa dữ liệu
    public static void ClearData(string key)
    {
        Initialize();

#if UNITY_EDITOR
        PlayerPrefs.DeleteKey(key);
#elif UNITY_ANDROID 
        if (keychainJavaObject != null)
        {
            keychainJavaObject.Call("clearData", key);
        }
#elif UNITY_IOS
        if (iosManager != System.IntPtr.Zero)
        {
            ClearData(iosManager, key);
        }
#endif
    }

    // Xóa toàn bộ dữ liệu
    public static void ClearAllData()
    {
        Initialize();

#if UNITY_EDITOR
        PlayerPrefs.DeleteAll();
#elif UNITY_ANDROID 
        if (keychainJavaObject != null)
        {
            keychainJavaObject.Call("clearAllData");
        }
#elif UNITY_IOS
        if (iosManager != System.IntPtr.Zero)
        {
            ClearAllData(iosManager);
        }
#endif
    }

    // Định nghĩa các hàm iOS native
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern IntPtr Init(string service);

    [DllImport("__Internal")]
    private static extern bool SaveString(IntPtr manager, string key, string value);

    [DllImport("__Internal")]
    private static extern IntPtr GetString(IntPtr manager, string key, string defaultValue);

    [DllImport("__Internal")]
    private static extern bool SaveInt(IntPtr manager, string key, int value);

    [DllImport("__Internal")]
    private static extern int GetInt(IntPtr manager, string key, int defaultValue);

    [DllImport("__Internal")]
    private static extern bool SaveFloat(IntPtr manager, string key, int value);

    [DllImport("__Internal")]
    private static extern int GetFloat(IntPtr manager, string key, int defaultValue);

    [DllImport("__Internal")]
    private static extern bool ClearData(IntPtr manager, string key);

    [DllImport("__Internal")]
    private static extern bool ClearAllData(IntPtr manager);
#endif
}