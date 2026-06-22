using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;

public interface IProgressDownloadAsset
{
    void OnStart(float totalMB);
    void OnProgress(float downloadedMB, float totalMB, float percent01);
    void OnComplete(bool success);
}

public class DownloadAssetHub : master.Singleton<DownloadAssetHub>
{
    public static int CFVersionCatalog
    {
        get => PlayerPrefs.GetInt("cf_version_catalog", 0);
        set => PlayerPrefs.SetInt("cf_version_catalog", value);
    }

    public static int CacheVersionCatalog
    {
        get => PlayerPrefs.GetInt("cache_version_catalog", 0);
        set => PlayerPrefs.SetInt("cache_version_catalog", value);
    }

    public class DownloadCache
    {
        public AsyncOperationHandle handle;
        public Action<bool> onDownloaded;
        public long size;
        public int status;         // 0: downloading, 1: success, 2: fail
        public bool finalized;     // tránh finalize 2 lần (event + coroutine)
    }

    private enum DownloadOutcome { None = 0, Success = 1, Fail = 2 }

    [Serializable]
    private sealed class DownloadLedgerEntry
    {
        public long size;                 // tổng byte cần tải (GetDownloadSizeAsync)
        public DownloadOutcome outcome;   // None/Success/Fail
        public double completedAt;        // Time.realtimeSinceStartupAsDouble khi finalize
    }

    // ======= STATE =======
    private readonly Dictionary<string, DownloadLedgerEntry> ledger = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DownloadCache> downloaded = new(StringComparer.Ordinal);

    private bool catalogUpdateSuccess;
    private bool isCheckConnectionRunning;
    private bool initialized;
    private bool isConnection;

    // ======= LIFECYCLE =======
    private IEnumerator Start()
    {
        StartCoroutine(CheckConnectionCoroutine());
        var init = Addressables.InitializeAsync();
        yield return init;
        yield return new WaitForEndOfFrame();
        initialized = true;
        UpdateCatalog();
    }

    public void UpdateCatalog()
    {
        if (initialized && CFVersionCatalog > CacheVersionCatalog)
            StartCoroutine(CatalogUpdate(OnCatalogUpdatedSuccessfully));
    }

    private IEnumerator CatalogUpdate(Action callback = null)
    {
        while (isCheckConnectionRunning) yield return new WaitForEndOfFrame();

        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;
        catalogUpdateSuccess = true;

        if (!checkHandle.IsValid() || checkHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogWarning("⚠️ Invalid check handle or failed to check updates.");
            if (checkHandle.IsValid()) Addressables.Release(checkHandle);
            callback?.Invoke();
            yield break;
        }

        if (checkHandle.Result != null && checkHandle.Result.Count > 0)
        {
            Debug.Log($"🆕 Found {checkHandle.Result.Count} new catalog(s). Updating...");
            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result, false);
            yield return updateHandle;

            if (updateHandle.Status == AsyncOperationStatus.Succeeded)
            {
                CacheVersionCatalog = CFVersionCatalog;
                OnCatalogUpdatedSuccessfully();       // size/hash có thể đổi → xoá ledger
                StartCoroutine(CleanupUnusedBundles());
                Debug.Log("✅ Catalog updated successfully!");
            }
            else
            {
                Debug.LogError("❌ Catalog update failed!");
            }

            if (updateHandle.IsValid()) Addressables.Release(updateHandle);
        }
        else
        {
            Debug.Log("✅ No catalog update needed.");
        }

        if (checkHandle.IsValid()) Addressables.Release(checkHandle);
        callback?.Invoke();
    }

    private void OnCatalogUpdatedSuccessfully()
    {
        ledger.Clear();
    }

    private IEnumerator CleanupUnusedBundles()
    {
        var cachePath = Caching.defaultCache.path;
        if (!Directory.Exists(cachePath))
        {
            Debug.LogWarning($"⚠️ Cache path not found: {cachePath}");
            yield break;
        }

        var used = new HashSet<string>(StringComparer.Ordinal);
        foreach (var locator in Addressables.ResourceLocators)
        {
            foreach (var key in locator.Keys)
            {
                if (locator.Locate(key, typeof(object), out var locs))
                {
                    for (int i = 0; i < locs.Count; i++)
                    {
                        var loc = locs[i];
                        if (loc != null && loc.Data is AssetBundleRequestOptions opt)
                            used.Add(opt.BundleName);
                    }
                }
            }
        }

        var dir = new DirectoryInfo(cachePath);
        var files = dir.GetFiles("*.bundle", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            var f = files[i];
            if (!used.Contains(f.Name))
            {
                try { f.Delete(); }
                catch (Exception ex) { Debug.LogWarning($"⚠️ Could not delete {f.Name}: {ex.Message}"); }
            }
        }
        yield break;
    }

    private IEnumerator CheckConnectionCoroutine()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            isConnection = false;
            yield break;
        }

        var wasRunning = isCheckConnectionRunning;
        while (isCheckConnectionRunning) yield return new WaitForEndOfFrame();
        if (wasRunning) yield break;

        isCheckConnectionRunning = true;
        using (UnityWebRequest req = UnityWebRequest.Head("https://www.google.com"))
        {
            req.timeout = 10;
            yield return req.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
            bool ok = !req.result.HasFlag(UnityWebRequest.Result.ConnectionError) &&
                      !req.result.HasFlag(UnityWebRequest.Result.ProtocolError);
#else
            bool ok = !(req.isNetworkError || req.isHttpError);
#endif
            isConnection = ok;
            isCheckConnectionRunning = false;
            if (!catalogUpdateSuccess) UpdateCatalog();
        }
    }

    // ======= API =======
    public bool IsDownloading(string key)
    {
        return downloaded.TryGetValue(key, out var dc) &&
               dc.handle.IsValid() &&
               !dc.handle.IsDone &&
               dc.status == 0;
    }

    public void MarkRetry(string key)
    {
        if (ledger.TryGetValue(key, out var e))
        {
            e.outcome = DownloadOutcome.None;
            e.completedAt = 0;
        }
    }

    // trả true nếu đã bắt đầu tải (spawn coroutine), false nếu asset có sẵn hoặc đang tải rồi
    public bool DownloadBundle(string key, Action<bool> callback = null)
    {
        MarkRetry(key);

        // local sẵn → short-circuit
        if (AddressableContainsInLocal(key, out _))
        {
            if (!ledger.TryGetValue(key, out var e)) ledger[key] = e = new DownloadLedgerEntry();
            e.outcome = DownloadOutcome.Success;
            e.completedAt = Time.realtimeSinceStartupAsDouble;
            callback?.Invoke(true);
            return false;
        }

        if (!downloaded.ContainsKey(key))
            downloaded.Add(key, new DownloadCache());

        // mark pending vào ledger để Wait… thấy key ngay
        if (!ledger.TryGetValue(key, out var pending)) ledger[key] = pending = new DownloadLedgerEntry();
        pending.outcome = DownloadOutcome.None;
        pending.completedAt = 0;

        if (!IsDownloading(key))
        {
            StartCoroutine(DownloadBundleIE(key, callback));
            return true;
        }

        return false;
    }

    public IEnumerator DownloadBundleIE(string key, Action<bool> callback = null)
    {
        if (!downloaded.TryGetValue(key, out var dc))
        {
            // Không vào queue → coi là available local
            ledger[key] = new DownloadLedgerEntry
            {
                size = 0,
                outcome = DownloadOutcome.Success,
                completedAt = Time.realtimeSinceStartupAsDouble
            };
            callback?.Invoke(true);
            yield break;
        }

        // 1) Lấy size
        var sizeHandle = Addressables.GetDownloadSizeAsync(key);
        dc.status = 0;
        yield return sizeHandle;

        long size = sizeHandle.Result;
        dc.size = size;

        if (!ledger.TryGetValue(key, out var entry)) ledger[key] = entry = new DownloadLedgerEntry();
        entry.size = size;

        // 2) Nếu cần tải → tải và finalize bằng Completed event
        if (size > 0)
        {
            StartCoroutine(CheckConnectionCoroutine());
            var downloadHandle = Addressables.DownloadDependenciesAsync(key);
            dc.handle = downloadHandle;
            dc.finalized = false;

            downloadHandle.Completed += h =>
            {
                if (!downloaded.TryGetValue(key, out var live)) return; // đã bị dọn bởi nơi khác

                if (live.finalized) return;
                live.finalized = true;

                bool success = (h.Status == AsyncOperationStatus.Succeeded);
                live.status = success ? 1 : 2;

                if (!ledger.TryGetValue(key, out var e2)) ledger[key] = e2 = new DownloadLedgerEntry();
                e2.outcome = success ? DownloadOutcome.Success : DownloadOutcome.Fail;
                Debug.Log($"Download finished {success}_{key}");
                e2.completedAt = Time.realtimeSinceStartupAsDouble;

                try { live.onDownloaded?.Invoke(success); } catch { /* ignore */ }
                try { callback?.Invoke(success); } catch { /* ignore */ }

                // logging
                int statusInternet = isConnection ? 1 : (!isCheckConnectionRunning && !isConnection ? 2 : 0);
                LogEvent.DownloadBundle(key, key, statusInternet, success ? 1 : 0);

                downloaded.Remove(key);
            };

            // Đợi handle xong (event đã finalize state rồi)
            while (!downloadHandle.IsDone)
                yield return null;

            Addressables.Release(downloadHandle);
            PurgeLedger(TimeSpan.FromMinutes(10));
        }
        else
        {
            // 3) Đã cached sẵn
            dc.status = 1;
            entry.outcome = DownloadOutcome.Success;
            entry.completedAt = Time.realtimeSinceStartupAsDouble;

            try { callback?.Invoke(true); } catch { /* ignore */ }
            try { dc.onDownloaded?.Invoke(true); } catch { /* ignore */ }

            int statusInternet = isConnection ? 1 : (!isCheckConnectionRunning && !isConnection ? 2 : 0);
            LogEvent.DownloadBundle(key, key, statusInternet, 1);

            downloaded.Remove(key);
            PurgeLedger(TimeSpan.FromMinutes(10));
        }

        Addressables.Release(sizeHandle);
    }

    /// <summary>
    /// Quan sát tiến trình tải.
    /// stickyTotal=true: Tổng cần tải "đóng băng tuyệt đối" (baseline chụp ngay khi mở UI – có fallback semi-sticky).
    /// stickyTotal=false: Tổng còn lại (remaining) thay đổi theo thời gian.
    /// </summary>
    public IEnumerator WaitDownloadBundle(IProgressDownloadAsset sink, string[] keys = null, bool stickyTotal = true)
    {
        if(sink==null) yield break;
        if (keys == null || keys.Length == 0)
        {
            if (downloaded.Count == 0)
            {
                sink?.OnStart(0);
                sink?.OnComplete(true);
                yield break;
            }
            keys = new string[downloaded.Count];
            downloaded.Keys.CopyTo(keys, 0);
        }

        // Đảm bảo có size sớm (tránh baseline = 0 vì race)
        yield return EnsureSizesAvailable(keys, 2f);

        const float UI_INTERVAL = 0.10f;
        const double INV_MB = 1.0 / 1048576.0;
        static double ToMB(ulong b) => b * INV_MB;

        // ---- Baseline (đóng băng) + fallback ban đầu ----
        ulong baselineBytes = 0;
        if (stickyTotal)
        {
            for (int i = 0; i < keys.Length; i++)
                if (ledger.TryGetValue(keys[i], out var e) && e.size > 0)
                    baselineBytes += (ulong)e.size;

            if (baselineBytes == 0)
            {
                for (int i = 0; i < keys.Length; i++)
                    if (downloaded.TryGetValue(keys[i], out var dc) && dc.size > 0)
                        baselineBytes += (ulong)dc.size;
            }
        }

        var (down0, total0) = GetCurrentBytes(keys, stickyTotal, baselineBytes);
        double totalMB0 = ToMB(total0);
        double downMB0  = ToMB(down0);
        float pct0      = (total0 > 0) ? Mathf.Clamp01((float)(downMB0 / Math.Max(1e-6, totalMB0))) : 0f;

        sink?.OnStart((float)totalMB0);
        sink?.OnProgress((float)downMB0, (float)totalMB0, pct0);

        if (AllFinished(keys))
        {
            // nhả 1 frame để chắc chắn event Completed đã finalize
            yield return null;
            sink?.OnComplete(AllSucceeded(keys));
            yield break;
        }

        float  uiClock      = 0f;
        double lastShownMB  = downMB0;
        float  lastShownPct = pct0;

        while (true)
        {
            if (AllFinished(keys)) break;

            uiClock += Time.deltaTime;
            if (uiClock >= UI_INTERVAL)
            {
                uiClock = 0f;

                var (downBytes, totalBytes) = GetCurrentBytes(keys, stickyTotal, baselineBytes);
                double totalMB = ToMB(totalBytes);
                double downMB  = ToMB(downBytes);
                float  pct     = (totalBytes > 0) ? Mathf.Clamp01((float)(downMB / Math.Max(1e-6, totalMB))) : 0f;

                // throttle UI
                if (Math.Abs(downMB - lastShownMB) >= 0.1 || Math.Abs(pct - lastShownPct) >= 0.01f)
                {
                    sink?.OnProgress((float)downMB, (float)totalMB, pct);
                    lastShownMB  = downMB;
                    lastShownPct = pct;
                }
            }
            yield return null;
        }

        // đệm 1 frame để DownloadBundleIE.Completed finalize xong
        yield return null;

        // chốt
        {
            var (downBytes, totalBytes) = GetCurrentBytes(keys, stickyTotal, baselineBytes);
            double totalMB = ToMB(totalBytes);
            double downMB  = ToMB(downBytes);
            float  pct     = (totalBytes > 0) ? Mathf.Clamp01((float)(downMB / Math.Max(1e-6, totalMB))) : 1f;

            sink?.OnProgress((float)downMB, (float)totalMB, pct);
            sink?.OnComplete(AllSucceeded(keys));
        }
    }

    // ======= HELPERS =======
    private (ulong downloadedBytes, ulong totalBytes) GetCurrentBytes(string[] ks, bool sticky, ulong baseline)
    {
        ulong total = 0;
        ulong downloadedBytes = 0;

        if (sticky)
        {
            // total baseline (đóng băng) nhưng không nhỏ hơn size đã biết hiện tại (semi-sticky nhích lên)
            total = baseline;
            ulong knownNow = 0;

            for (int i = 0; i < ks.Length; i++)
            {
                var k = ks[i];
                if (ledger.TryGetValue(k, out var e) && e.size > 0) knownNow += (ulong)e.size;
                else if (downloaded.TryGetValue(k, out var dc0) && dc0.size > 0) knownNow += (ulong)dc0.size;
            }
            if (knownNow > total) total = knownNow;

            for (int i = 0; i < ks.Length; i++)
            {
                var key = ks[i];

                if (downloaded.TryGetValue(key, out var dc))
                {
                    if (dc.status == 1)
                        downloadedBytes += (ulong)(dc.size > 0 ? dc.size : 0);
                    else if (dc.status == 0 && dc.handle.IsValid() && !dc.handle.IsDone)
                        downloadedBytes += (ulong)Math.Max(0L, (long)dc.handle.GetDownloadStatus().DownloadedBytes);
                }
                else
                {
                    if (ledger.TryGetValue(key, out var e2) && e2.outcome == DownloadOutcome.Success)
                        downloadedBytes += (ulong)(e2.size > 0 ? e2.size : 0);
                }
            }
        }
        else
        {
            // remaining
            for (int i = 0; i < ks.Length; i++)
                if (downloaded.TryGetValue(ks[i], out var dc))
                    total += (ulong)(dc.size > 0 ? dc.size : 0);

            for (int i = 0; i < ks.Length; i++)
            {
                if (!downloaded.TryGetValue(ks[i], out var dc)) continue;

                if (dc.status == 1)
                    downloadedBytes += (ulong)(dc.size > 0 ? dc.size : 0);
                else if (dc.status == 0 && dc.handle.IsValid() && !dc.handle.IsDone)
                    downloadedBytes += (ulong)Math.Max(0L, (long)dc.handle.GetDownloadStatus().DownloadedBytes);
            }
        }

        return (downloadedBytes, total);
    }

    private bool AllFinished(string[] ks)
    {
        // Chỉ coi là xong khi không còn key nào status==0 trong dict
        for (int i = 0; i < ks.Length; i++)
        {
            var key = ks[i];
            if (!downloaded.TryGetValue(key, out var dc))
                continue; // đã remove -> finished
            if (dc.status == 0) return false;
        }
        return true;
    }

    private bool AllSucceeded(string[] ks)
    {
        for (int i = 0; i < ks.Length; i++)
        {
            var key = ks[i];

            if (downloaded.TryGetValue(key, out var dc))
            {
                if (dc.status != 1) return false;
                continue;
            }

            if (ledger.TryGetValue(key, out var e))
            {
                if (e.outcome == DownloadOutcome.Fail) return false; // fail → false
                // Success/None -> ok
            }
            // Không có trong ledger: coi là ok (không cần tải/đã có local trước đó)
        }
        return true;
    }

    private IEnumerator EnsureSizesAvailable(string[] keys, float timeoutSec = 2f)
    {
        // hỏi size cho key chưa có e.size trong ledger
        var pending = new List<(string key, AsyncOperationHandle<long> h)>(keys.Length);
        for (int i = 0; i < keys.Length; i++)
        {
            var k = keys[i];
            if (!ledger.TryGetValue(k, out var e) || e.size <= 0)
            {
                var h = Addressables.GetDownloadSizeAsync(k);
                pending.Add((k, h));
            }
        }

        float t = 0f;
        while (pending.Count > 0 && t < timeoutSec)
        {
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                var (key, h) = pending[i];
                if (!h.IsDone) continue;

                long sz = (h.Status == AsyncOperationStatus.Succeeded) ? h.Result : 0;
                if (!ledger.TryGetValue(key, out var e2)) ledger[key] = e2 = new DownloadLedgerEntry();
                if (e2.size <= 0) e2.size = sz;

                Addressables.Release(h);
                pending.RemoveAt(i);
            }

            t += Time.deltaTime;
            yield return null;
        }

        // release còn lại (nếu timeout)
        for (int i = 0; i < pending.Count; i++)
            Addressables.Release(pending[i].h);
    }

    private void PurgeLedger(TimeSpan ttl)
    {
        if (ttl <= TimeSpan.Zero) return;

        double now = Time.realtimeSinceStartupAsDouble;
        var toRemove = new List<string>();

        foreach (var kv in ledger)
        {
            string key = kv.Key;
            var e = kv.Value;

            // đang tải → giữ
            if (downloaded.ContainsKey(key)) continue;

            // chỉ dọn khi đã có outcome và quá TTL
            if (e.outcome != DownloadOutcome.None && (now - e.completedAt) >= ttl.TotalSeconds)
                toRemove.Add(key);
        }

        for (int i = 0; i < toRemove.Count; i++)
            ledger.Remove(toRemove[i]);
    }

    // ======= LOCAL CHECK =======
    public bool AddressableContainsInLocal(string key, out string hash)
    {
        hash = "";
        var isContains = false;

        foreach (var locator in Addressables.ResourceLocators)
        {
            if (locator == null) continue;

            if (locator.Locate(key, typeof(object), out var resourceLocations))
            {
                isContains = true;

                for (int i = 0; i < resourceLocations.Count; i++)
                {
                    var loc = resourceLocations[i];
                    if (loc == null) continue;

#if UNITY_EDITOR
                    if (loc.Data == null) hash = loc.GetHashCode().ToString();
#endif
                    if (loc.Data is AssetBundleRequestOptions opt)
                    {
                        if (!IsBundleCached(opt, out hash) &&
                            ResourceManagerConfig.IsPathRemote(loc.InternalId))
                            return false;
                    }

                    var deps = loc.Dependencies;
                    if (deps != null)
                    {
                        for (int d = 0; d < deps.Count; d++)
                        {
                            var dep = deps[d];
                            if (dep?.Data is AssetBundleRequestOptions depOpt)
                            {
                                if (!IsBundleCached(depOpt, out hash) &&
                                    ResourceManagerConfig.IsPathRemote(dep.InternalId))
                                    return false;
                            }
                        }
                    }
                }

                // ĐÃ locate ở locator này → ok
                return true;
            }
        }

        return isContains;
    }

    private bool IsBundleCached(AssetBundleRequestOptions opt, out string hash)
    {
        hash = opt.Hash;
        if (string.IsNullOrEmpty(hash)) return true;

        var parsed = Hash128.Parse(hash);
        return Caching.IsVersionCached(new CachedAssetBundle(opt.BundleName, parsed));
    }
}
