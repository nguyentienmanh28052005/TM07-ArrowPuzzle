using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class LogBundleDownload : master.Singleton<LogBundleDownload>
{
    public enum NetType
    {
        NoInternet = 0,
        Wifi = 1,
        Cellular = 2,
        Unknown = 3
    }

    public static NetType GetNetType()
    {
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable:
                return NetType.NoInternet;
            case NetworkReachability.ReachableViaLocalAreaNetwork:
                return NetType.Wifi;
            case NetworkReachability.ReachableViaCarrierDataNetwork:
                return NetType.Cellular;
            default:
                return NetType.Unknown;
        }
    }

    public static IEnumerator CheckInternetReachable(Action<bool> onDone, int timeoutSec = 5)
    {
        using var req = UnityWebRequest.Head("https://www.google.com/generate_204");
        req.timeout = timeoutSec;
        yield return req.SendWebRequest();
        bool ok = req.result == UnityWebRequest.Result.Success && req.responseCode >= 200 && req.responseCode < 400;
        onDone?.Invoke(ok);
    }

    public void LogDownloadStart(string bundleName, string pathDownload)
    {
        var netType = GetNetType();
        Debug.Log($"[LogDownloadBundle] Download_Start\n Name: {bundleName},\n Path: {pathDownload},\n NetType: {netType}");
        LogFirebase($"cmd_download_start_{bundleName}_{(int)netType}_{pathDownload}");
    }

    public void LogDownLoadEnd(string bundleName, long timeDownload, bool status, string loseby = "")
    {
        string logLoseBy = "";
        if (!status && !string.IsNullOrEmpty(loseby) && loseby.Length > 0)
        {
            loseby = $",\n Lose by: {loseby}";
            logLoseBy = $"_{loseby}";
        }
        else
        {
            loseby = "";
        }
        Debug.Log($"[LogDownloadBundle] Download_End\n Name: {bundleName},\n Time_Download: {timeDownload},\n Status: {status}{loseby}");
        LogFirebase($"cmd_download_end_{bundleName}_{status}{logLoseBy}_{timeDownload}");
    }

    public void LogNoInternet(string bundleName, string pathDownload)
    {
        Debug.Log($"[LogDownloadBundle] No Internet\n Name: {bundleName},\n Path: {pathDownload}");
        LogFirebase($"cmd_network_lost_{bundleName}");
    }

    public void LogDiskFull(string bundleName, long sizeBundle)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        long sizeFree = GetAvailableStorageAndroid();
#else
        long sizeFree = GetFreeDiskBytes();
#endif
        var sizeBundleFM = FormatBytes(sizeBundle);
        var sizeFreeFM = FormatBytes(sizeFree);
        Debug.Log($"[LogDownloadBundle] Full Disk\n Name: {bundleName},\n Size Bundle: {sizeBundleFM},\n Free Size: {sizeFreeFM}");
        LogFirebase($"cmd_not_enough_storage_{bundleName}_{sizeBundleFM}_{sizeFreeFM}");
    }

    public void LogFirebase(string content)
    {
#if FIRBASE_ENABLE
        mygame.sdk.FIRhelper.logEvent(content);
#endif
    }

    public bool IsFreeDiskBytes(long bundleSizeBytes)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        long freeBytes = GetAvailableStorageAndroid();
        bool enough = freeBytes >= 0 && freeBytes > bundleSizeBytes;
        return enough;
#elif UNITY_EDITOR
        long freeBytes = GetFreeDiskBytes();
        bool enough = freeBytes >= 0 && freeBytes >= bundleSizeBytes;
        return enough;
#else
        return true;
#endif
    }

    private static long GetFreeDiskBytes()
    {
        try
        {
            var root = Path.GetPathRoot(Application.persistentDataPath);
            if (string.IsNullOrEmpty(root)) return -1;
            var drive = new DriveInfo(root);
            return drive.AvailableFreeSpace;
        }
        catch
        {
            return -1;
        }
    }
    public static long GetAvailableStorageAndroid()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject filesDir = activity.Call<AndroidJavaObject>("getFilesDir");
            string path = filesDir.Call<string>("getAbsolutePath");

            AndroidJavaObject statFs = new AndroidJavaObject("android.os.StatFs", path);
            long availableBlocks = statFs.Call<long>("getAvailableBlocksLong");
            long blockSize = statFs.Call<long>("getBlockSizeLong");
            long freeSpace = availableBlocks * blockSize;
            return freeSpace;
        }
#elif UNITY_EDITOR
        System.IO.DriveInfo drive = new System.IO.DriveInfo(System.IO.Path.GetPathRoot(Application.persistentDataPath));
        long totalSpace = drive.TotalSize;
        long freeSpace = drive.AvailableFreeSpace;
        return freeSpace;
#else
        return long.MaxValue;
#endif
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 0) return "Unknown";
        const double kb = 1024d, mb = kb * 1024d, gb = mb * 1024d;
        if (bytes >= gb) return $"{bytes / gb:F2} GB";
        if (bytes >= mb) return $"{bytes / mb:F2} MB";
        if (bytes >= kb) return $"{bytes / kb:F2} KB";
        return $"{bytes} B";
    }
}