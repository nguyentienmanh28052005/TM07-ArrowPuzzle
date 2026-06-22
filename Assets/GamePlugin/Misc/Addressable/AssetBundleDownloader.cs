using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using time;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;

public class AssetBundleDownloader : MonoBehaviour
{
    public static Dictionary<string, ABOperationDownloadHandle> waitingHandles = new();
    public static Dictionary<object, ABOperationDownloadHandle> waitingByKeyHandles = new();

    public static IEnumerator IEDownLoadBundle(object key, ABOperationDownloadHandle downloadHandle)
    {
        Debug.Log($"[DownLoadBundle] Start. Key = {key}");

        /////////////////////////////////////////////
        // 0. Resolve ResourceLocation
        /////////////////////////////////////////////
        var locations = AssetBundleManager.DependenciesLocation(key);
        downloadHandle.Size = GetDownloadSize(locations);

        if (downloadHandle.Status == ABOperationStatus.InProgress)
        {
            Debug.Log($"[DownLoadBundle] Handle.Status = InProgress");
            yield break;
        }

        if (locations.Count == 0)
        {
            string msg = $"[DownLoadBundle] Key not found: {key}";
            Debug.LogError(msg);
            downloadHandle.SetFail(msg, ABDownloadHandleException.ExceptionStatus.KeyNotFound);
            yield break;
        }

        waitingByKeyHandles[key] = downloadHandle;

        var downloadBytes = new long[locations.Count];
        int index = -1;

        /////////////////////////////////////////////
        // 1. PREPARE: Mark modified bundles to download
        /////////////////////////////////////////////
        foreach (var loc in locations)
        {
            if (loc.Data is not AssetBundleRequestOptions data) continue;
            if (!ResourceManagerConfig.IsPathRemote(loc.InternalId)) continue;

            if (AssetBundleManager.IsBundleModifiedOrEmpty(loc))
            {
                Debug.LogWarning($"[DownLoadBundle] Mark bundle for download: {data.BundleName}");
                waitingHandles[data.Hash] = downloadHandle;
            }
            else
            {
                Debug.Log($"[DownLoadBundle] Bundle OK (no need download): {data.BundleName}");
            }
        }

        yield return new WaitForEndOfFrame();

        /////////////////////////////////////////////
        // 2. DOWNLOAD BUNDLES
        /////////////////////////////////////////////
        if (downloadHandle.Status == ABOperationStatus.InProgress)
        {
            Debug.Log($"[DownLoadBundle] Handle.Status = InProgress");
            yield break;
        }
        downloadHandle.Status = ABOperationStatus.InProgress;

        foreach (var loc in locations)
        {
            if (loc.Data is not AssetBundleRequestOptions data) continue;
            if (!ResourceManagerConfig.IsPathRemote(loc.InternalId)) continue;

            string savePath = AssetBundleManager.LocalBundlePath(data.BundleName);
            string fullUrl = loc.InternalId;

            // Skip bundle that is already valid
            if (!AssetBundleManager.IsBundleModifiedOrEmpty(loc))
            {
                Debug.Log($"[DownLoadBundle] Skip valid bundle: {data.BundleName}");
                continue;
            }

            Debug.Log($"[DownLoadBundle] ========================================");
            Debug.Log($"[DownLoadBundle] Downloading bundle ({++index + 1}/{locations.Count}): {data.BundleName}");
            Debug.Log($"[DownLoadBundle] URL: {fullUrl}");
            Debug.Log($"[DownLoadBundle] Path Save: {savePath}");

            LogBundleDownload.Instance.LogDownloadStart(data.BundleName, fullUrl);

#if !UNITY_EDITOR && UNITY_ANDROID
            if (!LogBundleDownload.Instance.IsFreeDiskBytes(data.BundleSize))
            {
                LogBundleDownload.Instance.LogDiskFull(data.BundleName, data.BundleSize);
            }
#endif

            // Remove old cache file
            if (File.Exists(savePath))
            {
                Debug.LogWarning($"[DownLoadBundle] Delete old cache file: {savePath}");
                File.Delete(savePath);
            }

            // Request
            var www = new UnityWebRequest(fullUrl, UnityWebRequest.kHttpVerbGET, new DownloadHandlerFile(savePath), null);
            www.SendWebRequest();

            long timeCheckInternetCache = MGTime.GetUtcTime();
            long timeStartDownload = timeCheckInternetCache;
            // Progress update
            while (!www.isDone)
            {
                long timeNow = MGTime.GetUtcTime();
                if (timeNow - timeCheckInternetCache >= 5 * 1000)
                {
                    timeCheckInternetCache = timeNow;
                    if (!NetworkHeartbeat.Instance.IsInternetOk)
                    {
                        LogBundleDownload.Instance.LogNoInternet(data.BundleName, fullUrl);
                    }
                }
                if ((long)www.downloadedBytes > data.BundleSize)
                {
                    long timeDownload = timeNow - timeStartDownload;
                    string msg = $"[DownLoadBundle] VERIFY FAIL after download (Bundle corrupted)\n" + $"Bundle: {data.BundleName}\n" + $"URL: {fullUrl}\n";
                    LogBundleDownload.Instance.LogDownLoadEnd(data.BundleName, timeDownload, false, "Verify download (Bundle corrupted)");
                    Debug.LogError(msg);
                    waitingHandles.Remove(data.Hash);
                    waitingByKeyHandles.Remove(key);
                    downloadHandle.SetFail(msg, ABDownloadHandleException.ExceptionStatus.SizeMismatch);
                    yield break;
                }
                downloadBytes[index] = (long)www.downloadedBytes;
                downloadHandle.DownloadedBytes = downloadBytes.Sum();
                downloadHandle.SetProgress((float)downloadHandle.DownloadedBytes / downloadHandle.Size);
                yield return null;
            }

            // Final update
            downloadBytes[index] = (long)www.downloadedBytes;
            downloadHandle.DownloadedBytes = downloadBytes.Sum();

            /////////////////////////////////////////////
            // Check request result
            /////////////////////////////////////////////
            if (www.result != UnityWebRequest.Result.Success)
            {
                long timeNow = MGTime.GetUtcTime();
                long timeDownload = timeNow - timeStartDownload;
                string msg = $"[DownLoadBundle] DOWNLOAD ERROR\n" + $"Bundle: {data.BundleName}\n" + $"URL: {fullUrl}\n" + $"Error: {www.error}";
                LogBundleDownload.Instance.LogDownLoadEnd(data.BundleName, timeDownload, false, www.error);
                Debug.LogError(msg);
                waitingHandles.Remove(data.Hash);
                waitingByKeyHandles.Remove(key);
                downloadHandle.SetFail(msg, ABDownloadHandleException.ExceptionStatus.NetworkError);
                yield break;
            }

            // Save hash
            File.WriteAllText(savePath + ".hash", data.Hash);
            waitingHandles.Remove(data.Hash);

            /////////////////////////////////////////////
            // Verify after download
            /////////////////////////////////////////////
            if (AssetBundleManager.IsBundleModifiedOrEmpty(loc))
            {
                long timeNow = MGTime.GetUtcTime();
                long timeDownload = timeNow - timeStartDownload;
                string msg = $"[DownLoadBundle] VERIFY FAIL after download (Bundle corrupted)\n" + $"Bundle: {data.BundleName}\n" + $"URL: {fullUrl}\n";
                LogBundleDownload.Instance.LogDownLoadEnd(data.BundleName, timeDownload, false, "Verify after download (Bundle corrupted)");
                Debug.LogError(msg);
                waitingByKeyHandles.Remove(key);
                downloadHandle.SetFail(msg, ABDownloadHandleException.ExceptionStatus.SizeMismatch);
                yield break;
            }
            long timeNowEnd = MGTime.GetUtcTime();
            long timeDownloadEnd = timeNowEnd - timeStartDownload;
            LogBundleDownload.Instance.LogDownLoadEnd(data.BundleName, timeDownloadEnd, true);
            Debug.Log($"✓ [DownLoadBundle] Download success: {data.BundleName}");
        }

        /////////////////////////////////////////////
        // 3. SUCCESS
        /////////////////////////////////////////////
        waitingByKeyHandles.Remove(key);
        downloadHandle.SetProgress(1f);
        downloadHandle.SetResult();

        Debug.Log($"✓ [DownLoadBundle] Complete. All bundles OK for key={key}");
    }

    public static long GetDownloadSize(List<IResourceLocation> locations)
    {
        var size = 0L;
        foreach (var location in locations)
        {
            if (location.Data is not AssetBundleRequestOptions data) continue;
            if (!ResourceManagerConfig.IsPathRemote(location.InternalId)) continue;
            if (!AssetBundleManager.IsBundleModifiedOrEmpty(location)) continue;
            size += data.BundleSize;
        }
        return size;
    }
}
