using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using mygame.sdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using time;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using static LevelConfig;
using Object = UnityEngine.Object;

public class LevelRemoteManager : master.Singleton<LevelRemoteManager>
{
    public enum LoadingStatus
    {
        None = 0,
        Downloading,
        DownloadFailed,
        Loading,
        Failed,
        Succeeded
    }

    public static string CFLevelConfig
    {
        get => PlayerPrefs.GetString("cf_level_config_v1", "");
        set => PlayerPrefs.SetString("cf_level_config_v1", value);
    }

    public static bool CFUseProtobufBytes
    {
        get => PlayerPrefs.GetInt("cf_use_protobuf_bytes", 0) == 1;
        set => PlayerPrefs.SetInt("cf_use_protobuf_bytes", value ? 1 : 0);
    }

    public static int CFTryDownloadLevelCount
    {
        get => PlayerPrefs.GetInt("cf_try_download_level_count", 3);
        set => PlayerPrefs.SetInt("cf_try_download_level_count", value);
    }

    public static bool CFUseLocalIfDisableNetwork
    {
        get => PlayerPrefs.GetInt("cf_use_local_if_no_network", 0) == 1;
        set => PlayerPrefs.SetInt("cf_use_local_if_no_network", value ? 1 : 0);
    }

    public static int CFPreloadLevelCount
    {
        get => PlayerPrefs.GetInt("cf_preload_level_count", 25);
        set => PlayerPrefs.SetInt("cf_preload_level_count", value);
    }

    public static int CFSpaceDownloadLevel
    {
        get => PlayerPrefs.GetInt("cf_space_download_level", 5);
        set => PlayerPrefs.SetInt("cf_space_download_level", value);
    }

    public static int CFMaxDownloadLabel
    {
        get => PlayerPrefs.GetInt("cf_max_download_label", 0);
        set => PlayerPrefs.SetInt("cf_max_download_label", value);
    }

    public static bool CFClearOldLevelCache
    {
        get => PlayerPrefs.GetInt("cf_clear_old_level_cache", 1) == 1;
        set => PlayerPrefs.SetInt("cf_clear_old_level_cache", value ? 1 : 0);
    }

    public static string CFRemoteBaseUrl
    {
        get => PlayerPrefs.GetString("cf_remote_base_url", "");
        set => PlayerPrefs.SetString("cf_remote_base_url", value);
    }


    public int tryDownloadLevelCount
    {
        get => PlayerPrefs.GetInt("try_download_level_count", 1);
        set => PlayerPrefs.SetInt("try_download_level_count", value);
    }

    private int lastDownloadFaildedLevel
    {
        get => PlayerPrefs.GetInt("last_download_failed_level", 0);
        set => PlayerPrefs.SetInt("last_download_failed_level", value);
    }

    private int lastDownloadLabel
    {
        get => PlayerPrefs.GetInt("last_download_label", 1);
        set => PlayerPrefs.SetInt("last_download_label", value);
    }

    private int lastLevelClearCacheBundle
    {
        get => PlayerPrefs.GetInt("last_level_clear_cache_bundle", 1);
        set => PlayerPrefs.SetInt("last_level_clear_cache_bundle", value);
    }

    public int CF_TimeRequestDownloadBunble
    {
        get => PlayerPrefs.GetInt("cf_time_request_download_bunble", 30000);
        set => PlayerPrefs.SetInt("cf_time_request_download_bunble", value);
    }


    public LevelConfig levelConfig
    {
        get
        {
#if UNITY_ANDROID
            return levelConfigAndroid;
#endif
            return levelConfigIOS;
        }
    }

    [SerializeField] private LevelConfig levelConfigAndroid;
    [SerializeField] private LevelConfig levelConfigIOS;

    public int localLevelCount;
    public int levelPackSize;
    public int iconPackSize;

    private bool isWaitReleaseHandle;

    private ABOperationHandle<Object> handle { get; set; }

    private List<ABOperationHandle<Sprite>> handleIcon { get; set; } = new List<ABOperationHandle<Sprite>>();


    public LevelInfo levelInfo { get; private set; }

    public LoadingStatus status
    {
        get;
        private set;
    } = LoadingStatus.None;
    public bool initialized { get; private set; }

    private bool isConnection { get; set; } = false;

    private int statusDownloadCatalog;

    private bool isCheckConnectionRunning { get; set; } = false;

    public bool usingIcon = true;
    public bool isStartDownloadBundle, isTimeOutDownloadBundle, isStartDownloadBundleSS = false;
    public long timeCountDownloadBundle = 0;
    private bool loadingCatalog;
    private IResourceLocator cacheLocator;

    public List<ABOperationDownloadHandle> downloadHandles = new List<ABOperationDownloadHandle>();


    private IEnumerator Start()
    {
        if (tryDownloadLevelCount < CFTryDownloadLevelCount) tryDownloadLevelCount = 1;
        StartCoroutine(CheckConnectionCoroutine());
        AssetBundleManager.RemoteCatalogUpdated += OnRemoteCatalogChange;
        while (!AssetBundleManager.Instance.initialized)
        {
            yield return new WaitForEndOfFrame();
        }

        SetConfig();
        yield return new WaitForEndOfFrame();
        initialized = true;

    }

    private IEnumerator CheckConnectionCoroutine()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            isConnection = false;
            yield break;
        }

        var isRunning = isCheckConnectionRunning;
        while (isCheckConnectionRunning)
        {
            yield return new WaitForEndOfFrame();
        }

        if (isRunning) yield break;

        isCheckConnectionRunning = true;
        using (UnityWebRequest req = UnityWebRequest.Head("https://www.google.com"))
        {
            req.timeout = 10;
            yield return req.SendWebRequest();

            bool ok = false;
#if UNITY_2020_1_OR_NEWER
            ok = !req.result.HasFlag(UnityWebRequest.Result.ConnectionError) && !req.result.HasFlag(UnityWebRequest.Result.ProtocolError);
#else
            ok = !(req.isNetworkError || req.isHttpError);
#endif
            isConnection = ok;
            isCheckConnectionRunning = false;
        }
    }

    public void DownloadBundle()
    {
        var lv = GameRes.GetLevel();
        var isDownloading = false;
        if (lv < levelConfig.GetLevelInfos().Length)
        {
            for (int i = 0; i < CFPreloadLevelCount; i++)
            {
                var lvl = lv + i;
                if (lvl < localLevelCount || isDownloading) continue;
                var lf = levelConfig.GetLevelInfo(lvl, false);
                if (lf == null) continue;
                if ((lf.isValidLocal == null || !lf.isValidLocal.Value) && !AddressableContainsInLocal(LevelName(lf.levelID)))
                {
                    DownloadBundle(LevelName(lf.levelID), DownloadSuccess);
                    isDownloading = true;
                }

                for (int k = 0; k <= 5 - (CFPreloadLevelCount - i); k++)
                {
                    lf = levelConfig.GetLevelInfo(lvl + k, false);
                    if (lf != null && (lf.iconValidLocal == null || !lf.iconValidLocal.Value) && !AddressableContainsInLocal(IconName(lf.levelID)))
                    {
                        DownloadBundle(IconName(lf.levelID), DownloadSuccess);
                    }
                }

            }

            void DownloadSuccess(ABOperationDownloadHandle hd)
            {
                if (hd.Status == ABOperationStatus.Succeeded)
                {
                    for (int i = -levelPackSize; i < CFPreloadLevelCount + levelPackSize; i++)
                    {
                        var lf = levelConfig.GetLevelInfo(lv + i, false);
                        lf?.ClearIsValidCache();
                    }
                }
            }

        }
        else
        {
            var maxLabel = int.MaxValue;
            for (int i = lastDownloadLabel; i < CFMaxDownloadLabel; i++)
            {
                if (!AddressableContainsInLocal(GroupName(i)))
                {
                    DownloadBundle(GroupName(i), DownloadSuccess);
                }
                break;

                void DownloadSuccess(ABOperationDownloadHandle hd)
                {
                    if (hd.Status == ABOperationStatus.Succeeded)
                    {
                        if (i + 1 < maxLabel) lastDownloadLabel = i + 1;
                    }
                    else
                    {
                        maxLabel = i + 1;
                    }
                }
            }
        }
    }

    public void ClearOldLevelCache()
    {
        if (!CFClearOldLevelCache) return;
        StartCoroutine(CheckAndClearOldLevelCache());
    }

    private IEnumerator CheckAndClearOldLevelCache(bool force = false)
    {
        var level = GameRes.GetLevel();
        if (level - lastLevelClearCacheBundle < levelPackSize && !force) yield break;

        var startClearIndex = Mathf.Max(localLevelCount + 1, level - levelPackSize * 4);
        for (int i = startClearIndex; i < level - levelPackSize; i += levelPackSize / 4)
        {
            var lf = levelConfig.GetLevelInfo(i, false);
            if (lf == null) continue;
            lf.ClearIsValidCache();
            AssetBundleManager.Instance.ClearBundleCache(LevelName(lf.levelID));
        }

        startClearIndex = Mathf.Max(localLevelCount + 1, level - iconPackSize * 4);
        for (int i = startClearIndex; i < level - iconPackSize; i += iconPackSize / 4)
        {
            var lf = levelConfig.GetLevelInfo(i, false);
            if (lf == null) continue;
            lf.ClearIsValidCache();
            AssetBundleManager.Instance.ClearBundleCache(IconName(lf.levelID));
        }

        lastLevelClearCacheBundle = level;
    }

    private List<ABOperationDownloadHandle> StartDownloadIcon(LevelInfo[] levelInfos, Action<ABOperationDownloadHandle> callback = null)
    {
        var handles = new List<ABOperationDownloadHandle>();
        if (!usingIcon)
        {
            return new List<ABOperationDownloadHandle>();
        }
        for (int i = 0; i < 5; i++)
        {
            var lf = levelInfos[i];
            if ((lf.iconValidLocal == null || !lf.iconValidLocal.Value) && !AddressableContainsInLocal(IconName(lf.levelID)))
            {
                var hd = DownloadBundle(IconName(lf.levelID), DownloadSuccess);
                if (!handles.Contains(hd)) handles.Add(hd);
            }
        }

        return handles;
        void DownloadSuccess(ABOperationDownloadHandle hd)
        {
            for (int i = 0; i < 5; i++)
            {
                var lf = levelInfos[i];
                lf.ClearIsValidCache();
            }

            callback?.Invoke(hd);
        }
    }

    private ABOperationDownloadHandle DownloadBundle(string key, Action<ABOperationDownloadHandle> callback = null)
    {
        var size = AssetBundleManager.GetDownloadSize(key);
        var hash = "";

        if (size > 0)
        {
            StartCoroutine(CheckConnectionCoroutine());
            // yield return CheckConnectionCoroutine();
            // if (!isConnection)
            // {
            //     downloaded[hash].status = 2;
            //     callback?.Invoke(false);
            //     downloaded.Remove(hash);
            //     yield break;
            // }


            Debug.Log($"📦 Need to download {size / 1048576f:F2} MB for {key}");
            var downloadHandle = AssetBundleManager.DownloadDependenciesAsync(key, Callback);

            void Callback(ABOperationDownloadHandle hd)
            {
                if (hd.Status == ABOperationStatus.Succeeded)
                {
                    Debug.Log("✅ Preload complete.");
                    callback?.Invoke(hd);
                    var statusInternet = 0;
                    if (isConnection) statusInternet = 1;
                    if (!isCheckConnectionRunning && !isConnection) statusInternet = 2;
                    LogEvent.DownloadBundle(key, hash, statusInternet, 1);
                }
                else
                {
                    if (hd.Exception is { Status: ABDownloadHandleException.ExceptionStatus.SizeMismatch })
                    {
                        tryDownloadLevelCount = CFTryDownloadLevelCount;
                    }

                    callback?.Invoke(hd);
                    var statusInternet = 0;
                    if (isConnection) statusInternet = 1;
                    if (!isCheckConnectionRunning && !isConnection) statusInternet = 2;
                    LogEvent.DownloadBundle(key, hash, statusInternet, 0);
                    Debug.LogError($"❌ Preload failed: {hd.Exception?.Message}");
                }
            }
            Debug.Log($"✅ Download completed for {key}");
            return downloadHandle;
        }
        else
        {
            Debug.Log($"✅ {key} already cached.");

            var downloadHandle = ABOperationDownloadHandle.Create();
            StartCoroutine(DelayOneFrame(() =>
            {
                downloadHandle.SetProgress(1);
                downloadHandle.SetResult();
                callback?.Invoke(downloadHandle);
            }));
            return downloadHandle;
        }
    }

    IEnumerator DelayOneFrame(Action callback)
    {
        yield return null;   // đợi đúng 1 frame
        callback?.Invoke();
    }

    public void LoadLevelCache(int level, bool forceRelease = true)
    {
        if (status == LoadingStatus.Loading || status == LoadingStatus.Downloading)
        {
            Debug.LogWarning($"[LevelRemoteManager] LoadLevelCache skipped for level {level}, current status: {status}");
            return;
        }
        Debug.Log("---Load Level Cache---");
        status = LoadingStatus.Loading;
        isStartDownloadBundleSS = true;
        levelInfo = levelConfig.GetLevelInfo(level, false);

        var iconLevelInfos = IconLevelInfos(level, false);

        if (levelInfo != null)
        {
            Debug.Log($"---Load Level Cache: Level= {levelInfo.level}, levelID= {levelInfo.levelID}---");
            levelInfo.IsValid();
            var isLevelNotCache = levelInfo.isValid != null && levelInfo.isValid.Value && levelInfo.isValidLocal != null && !levelInfo.isValidLocal.Value;
            if (level - lastDownloadFaildedLevel >= CFSpaceDownloadLevel && (isLevelNotCache || IconNotContainsInLocal(iconLevelInfos)))
            {
                if (!string.IsNullOrEmpty(CacheLevelIndex))
                {
                    LastCacheLevelIndex = CacheLevelIndex;
                    CacheLevelIndex = "";
                }
                tryDownloadLevelCount = 1;
            }
        }

        levelInfo = levelConfig.GetLevelInfo(level);
        Debug.Log($"---Load Level Cache: 2 Level= {levelInfo.level}, levelID= {levelInfo.levelID}---");

        var lvl = levelInfo.levelID;
        iconLevelInfos = IconLevelInfos(level);

        if (forceRelease)
        {
            AssetBundleManager.Instance.Release(handle);
            var index = (level - 1) % 5;
            var ic = LoadCachedIcon(index);
            if (ic == null || ic.name != $"Level_{lvl}")
            {
                ReleaseIconHandle();
                if (handleIcon.All(x => x.Result == null))
                {
                    handleIcon.Clear();
                }
            }

            isWaitReleaseHandle = false;
        }
        else
        {
            isWaitReleaseHandle = true;
            var h = handle;

            var index = (level - 1) % 5;
            var ic = LoadCachedIcon(index);
            var clonedList = new List<ABOperationHandle<Sprite>>();
            if (ic == null || ic.name != $"Level_{lvl}")
            {
                clonedList = new List<ABOperationHandle<Sprite>>(handleIcon);
                handleIcon.Clear();
            }
            DOVirtual.DelayedCall(5, () => ReleaseCacheAddressable(h, clonedList));
        }

        if (!AddressableContainsInLocal(LevelName(lvl)) || IconNotContainsInLocal(iconLevelInfos))
        {
            downloadHandles.Clear();
            status = LoadingStatus.Downloading;
            downloadHandles.AddRange(StartDownloadIcon(iconLevelInfos, DownloadSuccess));
            downloadHandles.Add(DownloadBundle(LevelName(lvl), DownloadSuccess));

            return;

            void DownloadSuccess(ABOperationDownloadHandle hd)
            {
                if (!isStartDownloadBundleSS)
                {
                    return;
                }
                if (status == LoadingStatus.None || status == LoadingStatus.DownloadFailed) return;
                var success = hd.Status == ABOperationStatus.Succeeded && downloadHandles.All(x => x.Status == ABOperationStatus.Succeeded);
                var failed = hd.Status == ABOperationStatus.Failed || downloadHandles.Any(x => x.Status == ABOperationStatus.Failed);

                if (success)
                {
                    status = LoadingStatus.None;
                    CacheLevelIndex = "";
                    LastCacheLevelIndex = "";

                    LoadLevelCache(level, forceRelease);

                    for (int i = level - levelPackSize; i < level + levelPackSize; i++)
                    {
                        var lf = levelConfig.GetLevelInfo(i, false);
                        lf?.ClearIsValidCache();
                    }
                    tryDownloadLevelCount = 1;
                    // downloadHandles.Clear();
                }
                else if (failed)
                {
                    status = LoadingStatus.DownloadFailed;
                    levelInfo.ClearIsValidCache();

                    if (tryDownloadLevelCount > CFTryDownloadLevelCount)
                    {
                        LastCacheLevelIndex = "";
                        CacheLevelIndex = "";
                    }
                    // downloadHandles.Clear();
                }

            }
        }


        LoadAddressable(LevelName(lvl));
        LoadIcon();
        //DOVirtual.DelayedCall(25, () => LoadLevelCache(level + 1));
        void LoadAddressable(string address)
        {
            handle = AssetBundleManager.Instance.LoadAsset<Object>(address, LoadLevelSuccess);
        }

        void LoadIcon()
        {
            if (!usingIcon || handleIcon.Count > 0) return;
            for (int i = 0; i < iconLevelInfos.Length; i++)
            {
                var lf = iconLevelInfos[i];
                if (lf.IsValid() && lf.iconValidLocal != null && lf.iconValidLocal.Value)
                {
                    var loadIconHandle = AssetBundleManager.Instance.LoadAsset<Sprite>(IconName(lf.levelID), LoadIconSuccess);
                    handleIcon.Add(loadIconHandle);
                }
                else
                {
                    handleIcon.Add(default);
                }
            }
        }

        void LoadLevelSuccess(ABOperationHandle<Object> hd)
        {
            if (handle is { IsDone: true } && (handleIcon.All(x => x is { IsDone: true }) || handleIcon.Count == 0))
            {
                status = LoadingStatus.Succeeded;
            }
        }

        void LoadIconSuccess(ABOperationHandle<Sprite> hd)
        {
            if (handle is { IsDone: true } && (handleIcon.All(x => x is { IsDone: true }) || handleIcon.Count == 0))
            {
                status = LoadingStatus.Succeeded;
            }
        }
    }

    private bool IconNotContainsInLocal(LevelInfo[] iconLevelInfos)
    {
        for (int i = 0; i < iconLevelInfos.Length; i++)
        {
            var lf = iconLevelInfos[i];
            if (lf != null && lf.IsValid() && lf.iconValid != null && lf.iconValid.Value && lf.iconValidLocal != null && !lf.iconValidLocal.Value)
            {
                return true;
            }
        }

        return false;
    }

    private LevelInfo[] IconLevelInfos(int level, bool isRandom = true)
    {
        if (usingIcon)
        {
            var iconLevelInfos = new LevelInfo[5];
            int levelStart = (level - 1) / 5 * 5 + 1;
            for (int i = 0; i < 5; i++)
            {
                iconLevelInfos[i] = levelConfig.GetLevelInfo(levelStart + i, isRandom);
            }

            return iconLevelInfos;
        }

        return Array.Empty<LevelInfo>();
    }

    private void OnRemoteCatalogChange()
    {
        StartCoroutine(IEOnRemoteCatalogChange());
    }


    private IEnumerator IEOnRemoteCatalogChange()
    {
        while (status is LoadingStatus.Loading or LoadingStatus.Downloading)
        {
            yield return null;
        }

        Debug.Log("--------------");

        if (GameManager.GameState == GameState.None)
        {
            if (levelInfo != null)
            {
                var lv = GameRes.GetLevel();
                var lf = levelConfig.GetLevelInfo(lv, false);
                if (lf != null)
                {
                    var iconLevelInfos = IconLevelInfos(lv, false);
                    lf.ClearIsValidCache();
                    for (int i = 0; i < iconLevelInfos.Length; i++)
                    {
                        iconLevelInfos[i].ClearIsValidCache();
                    }

                    if ((!AddressableContainsInLocal(LevelName(lf.levelID), out var containKey) || lf.level != levelInfo.level || IconNotContainsInLocal(iconLevelInfos)) && containKey || (!containKey && lf.level == levelInfo.level))
                    {
                        LoadLevelCache(lv);
                    }
                }

            }
        }
        else
        {
            if (levelInfo != null)
            {
                var lv = GameRes.GetLevel();
                var lf = levelConfig.GetLevelInfo(lv, false);
                if (lf != null)
                {
                    var iconLevelInfos = IconLevelInfos(lv, false);
                    lf.ClearIsValidCache();
                    for (int i = 0; i < iconLevelInfos.Length; i++)
                    {
                        iconLevelInfos[i].ClearIsValidCache();
                    }

                    if ((!AddressableContainsInLocal(LevelName(lf.levelID), out var containKey) || lf.level != levelInfo.level || IconNotContainsInLocal(iconLevelInfos)) && containKey || (!containKey && lf.level == levelInfo.level))
                    {
                        status = LoadingStatus.None;
                    }
                }

            }
        }
    }

    public bool IsLoadLocalOnly()
    {
        return tryDownloadLevelCount > CFTryDownloadLevelCount || (Application.internetReachability == NetworkReachability.NotReachable && CFUseLocalIfDisableNetwork);
    }

    public void ReleaseIconHandle()
    {
        for (int i = 0; i < handleIcon.Count; i++)
        {
            AssetBundleManager.Instance.Release(handleIcon[i]);
        }
    }

    public IEnumerator WaitLoadLevel(TransitionUI transitionUI, bool isRandom = false)
    {
        if (status == LoadingStatus.DownloadFailed)
        {
            LoadLevelCache(GameRes.GetLevel());
        }
        isStartDownloadBundle = false;
        isTimeOutDownloadBundle = false;
        var dotTimer = 0f;
        var countDot = 0;
        const float dotInterval = 0.1f;
        var baseText = "_downloading_level_x";
        var size = downloadHandles.Sum(x => x.Size) / 1048576f;
        var txt = $"{0}/{size:F}MB";
        Debug.Log($"[Decor] Remote Loading: {status}");
        if (status == LoadingStatus.Downloading)
        {
            isStartDownloadBundle = true;
            isTimeOutDownloadBundle = false;
            timeCountDownloadBundle = MGTime.GetUtcTime();
            transitionUI.ShowLoading(true, "");
            yield return new WaitForSeconds(0.15f);

            var lastProgress = 0f;
            var startTime = Time.time;
            var loadProgress = LoadProgress();
            while (status == LoadingStatus.Downloading || (status != LoadingStatus.DownloadFailed && loadProgress < 1))
            {
                loadProgress = LoadProgress();
                size = downloadHandles.Sum(x => x.Size) / 1048576f;

                var percentComplete = downloadHandles.Sum(x => x.DownloadedBytes / 1048576f) / size;

                var progress = Mathf.Min(percentComplete, loadProgress);

                if (progress > lastProgress) lastProgress = progress;

                txt = $"{lastProgress * size:F}/{size:F}MB";
                transitionUI.UpdateProgress(lastProgress, txt);

                dotTimer += Time.deltaTime;
                if (dotTimer >= dotInterval)
                {
                    dotTimer = 0f;
                    countDot = (countDot + 1) % 4; // 0,1,2,3 -> lặp lại
                    string dots = new string('.', countDot);
                    transitionUI.UpdateDesc(baseText, dots);
                }

                yield return new WaitForEndOfFrame();
            }

            if (status == LoadingStatus.DownloadFailed)
            {
                isStartDownloadBundle = false;
                isTimeOutDownloadBundle = false;
                UIManager.Instance.ShowPopup<UITryAgainLoadLevel>(() =>
                {
                    tryDownloadLevelCount++;
                    lastDownloadFaildedLevel = GameRes.GetLevel();
                    var loadLocalOnly = IsLoadLocalOnly();

                    if (string.IsNullOrEmpty(CacheLevelIndex) && loadLocalOnly)
                    {
                        if (!string.IsNullOrEmpty(LastCacheLevelIndex))
                        {
                            CacheLevelIndex = LastCacheLevelIndex;
                            LastCacheLevelIndex = "";
                        }
                        else
                        {
                            CacheLevelIndex = "";
                        }
                    }

                    StartCoroutine(WaitLoadLevel(transitionUI, true));
                });

                loadProgress = LoadProgress();
                size = downloadHandles.Sum(x => x.Size) / 1048576f;

                var percentComplete = downloadHandles.Sum(x => x.DownloadedBytes / 1048576f) / size;

                var progress = Mathf.Min(percentComplete, loadProgress);

                if (progress > lastProgress) lastProgress = progress;

                txt = $"{lastProgress * size:F}/{size:F}MB";
                transitionUI.UpdateProgress(lastProgress, txt);

                while (status == LoadingStatus.DownloadFailed)
                {
                    dotTimer += Time.deltaTime;
                    if (dotTimer >= dotInterval)
                    {
                        dotTimer = 0f;
                        countDot = (countDot + 1) % 4; // 0,1,2,3 -> lặp lại
                        string dots = new string('.', countDot);
                        transitionUI.UpdateDesc(baseText, dots);
                    }
                    yield return new WaitForEndOfFrame();
                }

                yield break;
            }

            txt = $"{size:F}/{size:F}MB";
            transitionUI.UpdateProgress(1, txt);
            float LoadProgress() => (Time.time - startTime) / 1f;
        }

        dotTimer = 0f;
        baseText = "Loading level";
        transitionUI.UpdateDesc($"");
        Debug.Log($"[Decor] Remote Loading: {status}");
        if (status == LoadingStatus.Loading)
        {
            isStartDownloadBundle = false;
            isTimeOutDownloadBundle = false;
            transitionUI.ShowLoading(false, "");
            yield return new WaitForSeconds(0.15f);
            var lastProgress = 0f;
            var startTime = Time.time;
            var loadProgress = LoadProgress();
            txt = "";
            while (status != LoadingStatus.Succeeded)
            {
                loadProgress = LoadProgress();
                var progress = handle is { IsDone: false } ? Mathf.Min(handle.Progress, loadProgress) : Mathf.Max(.5f, loadProgress);

                if (progress > lastProgress) lastProgress = progress;

                txt = $"{(int)(lastProgress * 100)}%";
                transitionUI.UpdateProgress(lastProgress, txt);
                yield return new WaitForEndOfFrame();

                dotTimer += Time.deltaTime;
                if (dotTimer >= dotInterval)
                {
                    dotTimer = 0f;
                    countDot = (countDot + 1) % 4; // 0,1,2,3 -> lặp lại
                    string dots = new string('.', countDot);
                    transitionUI.UpdateDesc(baseText, dots);
                }
            }

            while (loadProgress < .8f)
            {
                var progress = loadProgress;
                loadProgress = LoadProgress();

                txt = $"{(int)(progress * 100)}%";
                transitionUI.UpdateProgress(lastProgress, txt);
                if (progress > lastProgress) transitionUI.UpdateProgress(progress, txt);
                yield return new WaitForEndOfFrame();

                dotTimer += Time.deltaTime;
                if (dotTimer >= dotInterval)
                {
                    dotTimer = 0f;
                    countDot = (countDot + 1) % 4; // 0,1,2,3 -> lặp lại
                    string dots = new string('.', countDot);
                    transitionUI.UpdateDesc(baseText, dots);
                }
            }
            txt = "100%";
            transitionUI.UpdateProgress(1, txt);
            yield return new WaitForEndOfFrame();

            float LoadProgress() => (Time.time - startTime) / 1f;
        }
#if FIRBASE_ENABLE
        string stringRandom = isRandom ? "level_random" : "level_bundle";
        mygame.sdk.FIRhelper.logEvent($"cmd_start_load_{stringRandom}");
#endif
        transitionUI.FadeOut(true);
    }

    public void Update()
    {
        if (isStartDownloadBundle && status == LoadingStatus.Downloading)
        {
            if (!isTimeOutDownloadBundle)
            {
                if (MGTime.GetUtcTime() - timeCountDownloadBundle >= CF_TimeRequestDownloadBunble)
                {
                    isTimeOutDownloadBundle = true;
                    isStartDownloadBundle = false;
                    status = LoadingStatus.DownloadFailed;
                    tryDownloadLevelCount = CFTryDownloadLevelCount;
                    isStartDownloadBundleSS = false;
                    Debug.Log($"[TimeOutDownloadBundle] timeout: {CF_TimeRequestDownloadBunble / 1000}");
                }
            }
        }
        else
        {
            if (timeCountDownloadBundle > 0)
            {
                timeCountDownloadBundle = 0;
            }
        }
    }

    public bool AddressableContainsInLocal(string key)
    {
        return AssetBundleManager.ContainKey(key) && !AssetBundleManager.IsBundleModifiedOrEmpty(key);
    }
    public bool AddressableContainsInLocal(string key, out bool containKey)
    {
        containKey = AssetBundleManager.ContainKey(key);
        return containKey && !AssetBundleManager.IsBundleModifiedOrEmpty(key);
    }

    public T LoadCachedObject<T>() where T : Object
    {
        if (handle != null && handle.Status == ABOperationStatus.Succeeded) return (T)handle.Result;
        return null;
    }

    public Sprite LoadCachedIcon(int index)
    {
        if (handleIcon.Count == 0) return null;
        if (handleIcon[index].Status == ABOperationStatus.Succeeded) return handleIcon[index].Result;
        return null;
    }

    public string LevelName(int level)
    {
        if (CFUseProtobufBytes)
        {
            return $"Level_{level}_Bytes";
        }
        return $"Level_{level}";
    }

    private string GroupName(int level)
    {
        return $"Lv_Remote_Group_{level}";
    }

    public string IconName(int level)
    {
        return $"Icon_Level_{level}";
    }

    private void ReleaseCacheAddressable(ABOperationHandle<Object> asyncOperationHandle, List<ABOperationHandle<Sprite>> listHandle)
    {
        if (!isWaitReleaseHandle) return;
        isWaitReleaseHandle = false;

        AssetBundleManager.Instance.Release(asyncOperationHandle);

        for (int i = 0; i < listHandle.Count; i++)
        {
            if ((GameRes.GetLevel() - 1) % 5 == 0)
            {
                AssetBundleManager.Instance.Release(listHandle[i]);
            }
        }
    }

    public void ParserFirConfig()
    {
#if FIRBASE_ENABLE
        var v = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("cf_try_download_level_count");
        if (v.StringValue != null && v.StringValue.Length > 0)
        {
            CFTryDownloadLevelCount = (int)v.LongValue;
        }
            
        v = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("cf_use_local_if_no_network");
        if (v.StringValue != null && v.StringValue.Length > 0)
        {
            CFUseLocalIfDisableNetwork = (int)v.LongValue == 1;
        }
            
        v = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("cf_space_download_level");
        if (v.StringValue != null && v.StringValue.Length > 0)
        {
            CFSpaceDownloadLevel = (int)v.LongValue;
        }
            
        v = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("cf_clear_old_level_cache");
        if (v.StringValue != null && v.StringValue.Length > 0)
        {
            CFClearOldLevelCache = (int)v.LongValue == 1;
        }
            
        v = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("cf_max_download_label");
        if (v.StringValue != null && v.StringValue.Length > 0)
        {
            CFMaxDownloadLabel = (int)v.LongValue;
        }

        v = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("cf_preload_level_count");
        if (v.StringValue != null && v.StringValue.Length > 0)
        {
            CFPreloadLevelCount = (int)v.LongValue;
        }

        v = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("cf_level_config_v1");
        if (v.StringValue != null && v.StringValue.Length > 0)
        {

            CFLevelConfig = v.StringValue;
        }

        v = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("cf_remote_base_url");
        if (v.StringValue != null && v.StringValue.Length > 0)
        {

            CFRemoteBaseUrl = v.StringValue;
        }

        v = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("cf_time_request_download_bunble");
        if (v.StringValue != null && v.StringValue.Length > 0)
        {
            CF_TimeRequestDownloadBunble = (int)v.LongValue;
        }

        SetConfig();
#endif
    }

    public void SetConfig()
    {
        if (!string.IsNullOrEmpty(CFLevelConfig))
        {
            var cf = JsonConvert.DeserializeObject<Dictionary<int, object>>(CFLevelConfig);
            if (cf.TryGetValue(AppConfig.verapp, out var obj) && obj is JObject jObj)
            {
                var data = jObj.ToObject<LevelConfigData>();
                SetData(data);
            }
            else if (cf.TryGetValue(-1, out obj) && obj is JObject jObj2)
            {
                var data = jObj2.ToObject<LevelConfigData>();
                SetData(data);
            }

            void SetData(LevelConfigData config)
            {
                if (config != null && config.data != null)
                {
                    var target = SDKManager.Instance.mediaCampain.ToLower();
                    int underscoreIndex = Mathf.Max(target.IndexOf('_') - 1, 1);
                    var source = config.data
                        .Select(d => new
                        {
                            data = d,
                            score = d.mediaCampain.Zip(target, (a, b) => a == b).TakeWhile(x => x).Count()
                        })
                        // chỉ lấy những cái có score > underscoreIndex
                        .Where(x => x.score > underscoreIndex)
                        .OrderByDescending(x => x.score)
                        .ThenBy(x => x.data.mediaCampain.ToLower().Length)
                        .Select(x => x.data)
                        .FirstOrDefault();
                    if (source == null) source = config.data.FirstOrDefault(x => string.IsNullOrEmpty(x.mediaCampain));
                    Debug.Log("------------camp=" + (source == null ? "null" : source.mediaCampain));
#if USING_LEVELMONET
                    if (source != null) LevelManager.Instance.levelMonetization = LevelMonetization.FromJson(source.levelMonetizations);
#endif
                    if (source != null) SetLevelIndex(source.levelInfos);
                    if (source != null) SetLevelShuffleData(source.dataLevelShuffle); 

                    //LogEvent.LogABTest("cf_level_index", data.version);
                }
            }
        }
        
        SetConfigNew(ParseNewConfig(CFLevelConfigNew));
    }

    private void SetLevelShuffleData(string data)
    {
        levelConfig.SetLevelShuffleData(data);
    }
    public static string CFLevelConfigNew
    {
        get => PlayerPrefs.GetString("cf_level_config_new", "");
        set => PlayerPrefs.SetString("cf_level_config_new", value);
    }

    public List<string> ParseNewConfig(string cf)
    {
        try
        {
            List<string> configs = new List<string>();
            if (string.IsNullOrEmpty(cf)) return configs;
            configs = JsonConvert.DeserializeObject<List<string>>(cf);
            return configs;
        }
        catch (Exception e)
        {
            return new List<string>();
        }
    }

    public void SaveConfigNew(List<string> configs)
    {
        CFLevelConfigNew = JsonConvert.SerializeObject(configs);
    }

    public void GetNewConfig(string json)
    {
        if (string.IsNullOrEmpty(json)) return;

        try
        {
            var data = JObject.Parse(json);

            if (data == null || !data.ContainsKey("message") || data["message"] == null)
            {
                Debug.Log("[LevelRemoteManager] GetNewConfig JSON does not contain message.");
                return;
            }

            var jsonArray = data["message"] as JArray;

            if (jsonArray == null || jsonArray.Count < 2)
            {
                Debug.Log($"[LevelRemoteManager] GetNewConfig Error parsing segment data.");
                return;
            }

            string segmentName = jsonArray[0]?.Value<string>();
            string levelData = jsonArray[1]?.Value<string>();

            Debug.Log($"[LevelRemoteManager] ParseMessages LogSegmentName = {segmentName}");

            if (segmentName != null && segmentName != LogEventManager.LogSegmentName)
            {
                LogEventManager.LogSegmentName = segmentName;
            }

            if (!string.IsNullOrEmpty(levelData))
            {
                var configs = new List<string> { levelData };
                SaveConfigNew(configs);
                SetConfigNew(configs);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LevelRemoteManager] GetNewConfig Exception: {e.Message}");
        }
    }

    public bool SetConfigNew(List<string> configs)
    {
        try
        {
            if (configs == null || configs.Count == 0) return false;

            // 1. Dùng Dictionary để Lookup/Ghi đè với độ phức tạp O(1) thay vì FindIndex O(N)
            Dictionary<int, LevelInfo> levelDict = new();

            if (levelConfig != null && levelConfig.levelInfos != null)
            {
                foreach (var info in levelConfig.levelInfos)
                {
                    levelDict[info.level] = info;
                }
            }

            foreach (var config in configs)
            {
                if (string.IsNullOrEmpty(config)) continue;
                try
                {
                    var cf = JsonConvert.DeserializeObject<LevelConfigDataNew>(config);
                    if (cf == null || cf.levelInfos == null) continue;

                    foreach (var info in cf.levelInfos)
                    {
                        if (info == null || info.level <= 0 || info.levelID <= 0) continue;

                        levelDict[info.level] = info;
                    }
                }
                catch (Newtonsoft.Json.JsonReaderException jre)
                {
                    Debug.LogError($"[LevelRemoteManager] JSON Cú pháp sai tại Dòng: {jre.LineNumber}, Vị trí (Cột): {jre.LinePosition}, Path: {jre.Path}. Chi tiết: {jre.Message}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LevelRemoteManager] Parse config thất bại. Error: {e.Message}");
                }
            }

            // 3. Trả về mảng đã được sort chuẩn xác theo thứ tự Level
            if (levelDict.Count > 0)
            {
                var finalLevelArray = levelDict.Values.OrderBy(x => x.level).ToArray();
                Debug.Log($"[LevelRemoteManager] SetConfigNew merged count = {finalLevelArray.Length}");
                SetLevelIndex(finalLevelArray);
            }
            
            return true;
        }
        catch (Exception e)
        {
            Debug.Log($"[LevelRemoteManager] SetConfigNew Error: {e.Message}");
            return false;
        }
    }

    private void SetLevelIndex(LevelInfo[] data)
    {
        levelConfig.SetLevelInfos(data);

        if (levelInfo == null || levelInfo.level == 0) return;
        var level = DataManager.Level;
        var lf = levelConfig.GetLevelInfo(level);


        if (lf.levelID != levelInfo.levelID)
        {
            Debug.Log("---Reload Level Cache---");
            ReleaseIconHandle();
            handleIcon.Clear();
            LoadLevelCache(level);
        }
    }

}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(LevelRemoteManager))]
public class LevelRemoteManagerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Create Config"))
        {
            var levelRemoteManager = (LevelRemoteManager)target;
            var data = new Dictionary<int, object>();
            var cf = new LevelConfigData();
            cf.data = new[]
            {
                new LevelConfigData.Data()
            {
                mediaCampain = "",
                levelInfos = levelRemoteManager.levelConfig.levelInfos
                }
            };
#if USING_LEVELMONET
            var scene = SceneManager.GetActiveScene();
            if (scene.isLoaded)
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    var levelManager = root.GetComponentInChildren<LevelManager>(true);
                    if (levelManager != null)
                    {
                        var levelMonetization = levelManager.levelMonetization;
                        var dto = new LevelMonetizationDTO
                        {
                            defaultData = levelMonetization.defaultData,
                            configEntry = levelMonetization.configs,
                            configEntryGroup = levelMonetization.configEntryGroup,
                            difficultyGroup = levelMonetization.difficultyGroup,
                            configEasyBoxBoosters = levelMonetization.configEasyBoxBoosters,
                            maxLevelDifficultyReduce = levelMonetization.maxLevelDifficultyReduce,
                            levelStartDifficultyCoin = levelMonetization.levelStartDifficultyCoin,
                            easyModePoint = levelMonetization.easyModePoint,
                            startChangDifficultyCoin = levelMonetization.startChangDifficultyCoin,
                            endChangDifficultyCoin = levelMonetization.endChangDifficultyCoin,
                        };

                        cf.data[0].levelMonetizations = ZipUtils.ToBase64(JsonConvert.SerializeObject(dto));
                    }
                }
            }
#endif 
            data.Add(AppConfig.verapp, cf);
            string path = UnityEditor.EditorUtility.SaveFilePanel("Save json", "", "LevelConfig.json", "json");
            var json = JsonConvert.SerializeObject(data);
            File.WriteAllText(path, json);
        }
        base.OnInspectorGUI();
    }
}
#endif