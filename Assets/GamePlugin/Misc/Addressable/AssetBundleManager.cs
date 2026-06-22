using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;
using DG.Tweening;


public class AssetBundleManager : MonoBehaviour
{
    public class CatalogInfo
    {
        public string name;
        public string remoteBaseUrl;
        public int version;
    }

    public static bool ManualUpdateCatalog = false;

    public bool initialized = false;
    public int statusRemoteCatalog;

    [SerializeField] private string remoteBaseUrl;


    //=====================
    // FIELDS
    //=====================
    public static AssetBundleManager Instance;

    public static List<IResourceLocator> resourceLocators = new();
    public static Action RemoteCatalogUpdated;

    // Bundle đã load (cache)
    private Dictionary<string, object> loadedBundles = new();
    private HashSet<string> loadingBundles = new();

    private HashSet<string> unloadingBundles = new();

    private Dictionary<object, Dictionary<string, object>> loadedAsset = new();

    private int lastVersionCatalog
    {
        get =>  PlayerPrefs.GetInt("lastVersionCatalog", 0);
        set => PlayerPrefs.SetInt("lastVersionCatalog", value);
    }
    //=====================
    // INIT
    //=====================
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (!ManualUpdateCatalog)
        {
            StartCoroutine(IELoadCatalog());
        }
        Debug.Log("-----" + PlayerPrefs.GetInt("mem_ver_app_ins", AppConfig.verapp));
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    static void OnUnityStart()
    {
        // Delay để tránh gọi quá sớm khi Addressables chưa init
        UnityEditor.EditorApplication.delayCall += UpdateRemoteUrl;
    }

    // ------------------------------------------------------------
    // 2. Chạy khi Unity reload script (recompile)
    // ------------------------------------------------------------
    [UnityEditor.Callbacks.DidReloadScripts]
    static void OnScriptsReload()
    {
        UnityEditor.EditorApplication.delayCall += UpdateRemoteUrl;
        UnityEditor.AssemblyReloadEvents.afterAssemblyReload += UpdateRemoteUrl;
    }

    private static void UpdateRemoteUrl()
    {
        var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogWarning("⚠ AddressableAssetSettings not found.");
            return;
        }

        string remotePath = settings.RemoteCatalogLoadPath.GetValue(settings);

        if (string.IsNullOrEmpty(remotePath))
        {
            Debug.LogWarning("⚠ RemoteLoadPath not found in current Addressables Profile.");
            return;
        }

        var scene = SceneManager.GetActiveScene();
        if (scene.isLoaded)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var levelRemoteManager = root.GetComponentInChildren<AssetBundleManager>(true);
                if (levelRemoteManager != null)
                {
                    if (levelRemoteManager.remoteBaseUrl != remotePath)
                    {
                        levelRemoteManager.remoteBaseUrl = remotePath;

                        UnityEditor.EditorUtility.SetDirty(levelRemoteManager);
                        UnityEditor.AssetDatabase.SaveAssets();

                        Debug.Log($"✔ Auto updated RemoteBaseUrl:\n{remotePath}");
                    }
                }
            }
        }
    }
#endif

    public IEnumerator IELoadCatalog()
    {
        var catalogPath =
#if UNITY_EDITOR
            Addressables.RuntimePath + "/catalog.json";
#else
            Addressables.RuntimePath + "/catalog.json";
#endif
        string platformFolder = null;
#if UNITY_ANDROID
        platformFolder = "Android";
#elif UNITY_IOS
        platformFolder = "iOS";
#endif
        Debug.Log($"Catalog: verapp:{AppConfig.verapp},lastVersionCatalog:{lastVersionCatalog}");

        if (AppConfig.verapp > lastVersionCatalog)
        {
            lastVersionCatalog = AppConfig.verapp;
            if (File.Exists(AssetBundleCatalog.CachedCatalogPath))
            {
                File.Delete(AssetBundleCatalog.CachedCatalogPath);
                Debug.Log($"Catalog: Clear old version catalog");
            }
        }

        yield return AssetBundleCatalog.IELoadCatalog(catalogPath, _ => { });

        if (File.Exists(AssetBundleCatalog.CachedCatalogPath))
        {
            catalogPath = AssetBundleCatalog.CachedCatalogPath;
        }
        else
        {
            catalogPath = Path.Combine(Application.streamingAssetsPath, $"com.unity.addressables/aa/{platformFolder}/remote_catalog.json");
        }
        yield return AssetBundleCatalog.IELoadCatalog(catalogPath, _ => { });
        initialized = true;

        yield return IELoadRemoteCatalog();
    }

    public IEnumerator IELoadRemoteCatalog()
    {
        while (statusRemoteCatalog == 2)
        {
            yield return null;
        }
        var catalogInfo = JsonConvert.DeserializeObject<CatalogInfo>(AssetBundleCatalog.CFCataloginfo);
        if (catalogInfo.version > AssetBundleCatalog.CacheVersionCatalog)
        {
            statusRemoteCatalog = 2;
            var url = string.IsNullOrEmpty(catalogInfo.remoteBaseUrl) ? remoteBaseUrl : catalogInfo.remoteBaseUrl;
            string path = url + $"/catalog_{catalogInfo.name}.json";
            StartCoroutine(AssetBundleCatalog.IELoadCatalog(path, status =>
            {
                statusRemoteCatalog = status;
                if (status == 1)
                {
                    AssetBundleCatalog.CacheVersionCatalog = catalogInfo.version;
                    RemoteCatalogUpdated?.Invoke();
                }
            }));
        }
    }

    public static long GetDownloadSize(string key)
    {
        var locations = DependenciesLocation(key);
        return AssetBundleDownloader.GetDownloadSize(locations);
    }

    public static ABOperationDownloadHandle DownloadDependenciesAsync(string key, Action<ABOperationDownloadHandle> callback)
    {
        if (AssetBundleDownloader.waitingByKeyHandles.TryGetValue(key, out var handle))
        {
            handle.Completed += callback;
            return handle;
        }

        var locations = DependenciesLocation(key);

        foreach (var location in locations)
        {
            if (location.Data is not AssetBundleRequestOptions data) continue;
            if (!ResourceManagerConfig.IsPathRemote(location.InternalId)) continue;
            if (AssetBundleDownloader.waitingHandles.TryGetValue(data.Hash, out handle))
            {
                handle.Completed += callback;
                return handle;
            }
        }

        handle = ABOperationDownloadHandle.Create();
        handle.Completed += callback;
        Instance.StartCoroutine(AssetBundleDownloader.IEDownLoadBundle(key, handle));
        return handle;
    }

    public static string LocalBundlePath(string bundleName)
    {
        var dir = Path.Combine(Application.persistentDataPath, "ab_cache");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        return Path.Combine(Application.persistentDataPath, "ab_cache", bundleName + ".bundle");
    }

    public void ClearBundleCache(string key)
    {
        var locations = DependenciesLocation(key);

        foreach (var location in locations)
        {
            if (location.Data is not AssetBundleRequestOptions data) continue;
            if (!ResourceManagerConfig.IsPathRemote(location.InternalId)) continue;
            if (!IsBundleModifiedOrEmpty(key))
            {
                string path = LocalBundlePath(data.BundleName);
                string hashPath = path + ".hash";

                File.Delete(path);
                File.Delete(hashPath);
            }
        }
    }

    public void ClearUnUseBundleCache()
    {
        var allFile = Directory.GetFiles("ab_cache", "*.bundle", SearchOption.AllDirectories);
        foreach (var info in allFile)
        {
        }
    }

    public static bool IsBundleModifiedOrEmpty(IResourceLocation loc)
    {
#if UNITY_EDITOR
        var playModeScriptIndex = AddressableAssetsBuild.GetPlayModeScriptIndex();
        if (playModeScriptIndex == 0)
        {
            return false;
        }
#endif

        if (loc.Data is not AssetBundleRequestOptions data)
        {
            Debug.LogWarning($"[IsBundleModified] Location has no AssetBundleRequestOptions: {loc.InternalId}");
            return false;
        }

        if (!ResourceManagerConfig.IsPathRemote(loc.InternalId))
        {
            Debug.Log($"[IsBundleModified] Skip (Local bundle): {data.BundleName}");
            return false;
        }

        string path = LocalBundlePath(data.BundleName);
        string hashPath = path + ".hash";

        Debug.Log($"[IsBundleModified] Checking bundle: {data.BundleName}\n" + $"- LocalPath: {path}\n" + $"- RemoteHash: {data.Hash}\n" + $"- RemoteSize: {data.BundleSize}");

        // 1. Missing file
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[IsBundleModified] Missing bundle file → NEED DOWNLOAD: {path}");
            return true;
        }

        if (!File.Exists(hashPath))
        {
            Debug.LogWarning($"[IsBundleModified] Missing hash file → NEED DOWNLOAD: {hashPath}");
            return true;
        }

        // 2. Size mismatch
        var fileInfo = new FileInfo(path);
        if (fileInfo.Length != data.BundleSize)
        {
            Debug.LogWarning($"[IsBundleModified] Size mismatch → NEED DOWNLOAD\n" + $"- LocalSize: {fileInfo.Length}\n" + $"- RemoteSize: {data.BundleSize}");
            return true;
        }

        // 3. Hash mismatch
        string localHash = File.ReadAllText(hashPath).Trim();

        if (string.IsNullOrEmpty(localHash))
        {
            Debug.LogWarning($"[IsBundleModified] Local hash EMPTY → NEED DOWNLOAD");
            return true;
        }

        if (!localHash.Equals(data.Hash, StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning($"[IsBundleModified] HASH mismatch → NEED DOWNLOAD\n" + $"- LocalHash: {localHash}\n" + $"- RemoteHash: {data.Hash}");
            return true;
        }

        Debug.Log($"[IsBundleModified] Bundle OK → No need download: {data.BundleName}");
        return false;
    }

    public static bool IsBundleModifiedOrEmpty(string key)
    {
        var locations = DependenciesLocation(key);
        foreach (var location in locations)
        {
            if (location.Data is not AssetBundleRequestOptions data) continue;
            if (IsBundleModifiedOrEmpty(location))
            {
                return true;
            }
        }
        return false;
    }

    public static bool ContainKey(string key)
    {
        var locations = DependenciesLocation(key);
        return locations.Count > 0;
    }

    public static List<IResourceLocation> DependenciesLocation(object key)
    {
        var locations = new List<IResourceLocation>();
        foreach (var locator in resourceLocators)
        {
            if (locator == null)
                continue;

            if (locator.Locate(key, typeof(object), out var resourceLocations))
            {
                foreach (var loc in resourceLocations)
                {
                    if (loc == null) continue;
                    if (loc.Data is AssetBundleRequestOptions opt)
                    {
                        locations.Add(loc);
                    }

                    // Kiểm tra các dependencies (bundle khác mà nó cần)
                    if (loc.Dependencies != null)
                    {
                        foreach (var dep in loc.Dependencies)
                        {
                            if (dep?.Data is AssetBundleRequestOptions depOpt)
                            {
                                locations.Add(dep);
                            }
                        }
                    }
                }
            }
        }
        locations = locations.GroupBy(loc => new { GetFileName = ResourceManagerConfig.IsPathRemote(loc.InternalId) ? Path.GetFileName(loc.InternalId) : loc.InternalId, loc.ResourceType, loc.ProviderId }).Select(g => ResourceManagerConfig.IsPathRemote(g.Last().InternalId) ? g.Last() : g.First()).ToList();
        Debug.Log("[Decor] locations: " + locations.Count);
        foreach(var location in locations)
        {
            Debug.Log("[Decor] location_: " + location.Dependencies);
        }
        return locations;
    }

    public static List<IResourceLocation> DependenciesLocation(object key, out string internalId)
    {
        internalId = "";
        var locations = new List<IResourceLocation>();
        foreach (var locator in resourceLocators)
        {
            if (locator == null)
                continue;

            if (locator.Locate(key, typeof(object), out var resourceLocations))
            {
                foreach (var loc in resourceLocations)
                {
                    if (loc == null) continue;
                    internalId = loc.InternalId;
                    if (loc.Data is AssetBundleRequestOptions opt)
                    {
                        locations.Add(loc);
                    }

                    // Kiểm tra các dependencies (bundle khác mà nó cần)
                    if (loc.Dependencies != null)
                    {
                        foreach (var dep in loc.Dependencies)
                        {
                            if (dep?.Data is AssetBundleRequestOptions depOpt)
                            {
                                locations.Add(dep);
                            }
                        }
                    }
                }
            }
        }

        return locations;
    }

    public ABOperationHandle<T> LoadAsset<T>(object key, Action<ABOperationHandle<T>> cb) where T : Object
    {
        var handle = ABOperationHandle<T>.Create(key);
        handle.Completed += cb;
        StartCoroutine(IELoadAsset(key, handle));
        return handle;
    }

    private IEnumerator IELoadAsset<T>(object key, ABOperationHandle<T> handle) where T : Object
    {

        /////////////////////////////////////////////////////////////////////////
        // 1. Resolve all bundle dependencies
        /////////////////////////////////////////////////////////////////////////
        var locations = DependenciesLocation(key, out var internalId);
        if (locations == null || locations.Count == 0)
        {
            handle.SetFail("[LoadAsset] Key not found: " + key);
            yield break;
        }
        locations.RemoveAll(x => x.Data is not AssetBundleRequestOptions);

#if UNITY_EDITOR
        var playModeScriptIndex = AddressableAssetsBuild.GetPlayModeScriptIndex();
        if (playModeScriptIndex == 0)
        {
            yield return new WaitForEndOfFrame();
            var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(internalId);
            if (obj != null)
            {
                handle.SetResult(obj);
                Debug.Log("[LoadAsset] Load success asset: " + internalId);
            }
            else
            {
                Debug.LogError("[LoadAsset] Failed to read asset: " + internalId);
                handle.SetFail("[LoadAsset] Failed to read asset: " + internalId);
            }
            yield break;
        }
#endif

        var requests = new Dictionary<string, object>();
        var size = locations.Sum(x => x.Data is AssetBundleRequestOptions opt ? opt.BundleSize : 0);
        var rateProgress = .9f;
        foreach (var location in locations)
        {
            if (location.Data is not AssetBundleRequestOptions opt) continue;
            if (loadingBundles.Contains(opt.BundleName))
            {
                object bundle = null;
                while (!loadedBundles.TryGetValue(opt.BundleName, out bundle))
                {
                    yield return null;
                }

                loadedBundles.TryGetValue(opt.BundleName, out bundle);

                while (bundle is AssetBundleCreateRequest { isDone: false } or UnityWebRequest { isDone: false })
                {
                    handle.SetProgress(CalculateProgress() * rateProgress);
                    yield return null;
                }

                requests.TryAdd(opt.BundleName, bundle);
            }
            else
            {
                loadingBundles.Add(opt.BundleName);

                while (unloadingBundles.Contains(opt.BundleName))
                {
                    yield return null;
                }

                if (loadedBundles.TryGetValue(opt.BundleName, out var bundle))
                {
                    requests.TryAdd(opt.BundleName, bundle);
                    while (bundle is AssetBundleCreateRequest { isDone: false } or UnityWebRequest { isDone: false })
                    {
                        handle.SetProgress(CalculateProgress() * rateProgress);
                        yield return null;
                    }
                }
                else if (ResourceManagerConfig.IsPathRemote(location.InternalId))
                {
                    var bundlePath = LocalBundlePath(opt.BundleName);

                    if (!File.Exists(bundlePath))
                        continue;

                    var bundleCreateRequest = AssetBundle.LoadFromFileAsync(bundlePath);
                    requests.TryAdd(opt.BundleName, bundleCreateRequest);

                    loadedBundles.Add(opt.BundleName, bundleCreateRequest);
                    while (!bundleCreateRequest.isDone)
                    {
                        handle.SetProgress(CalculateProgress() * rateProgress);
                        yield return null;
                    }
                }
                else
                {
#if UNITY_EDITOR || UNITY_IOS
                    if (File.Exists(location.InternalId))
                    {
                        var bundleCreateRequest = AssetBundle.LoadFromFileAsync(location.InternalId);
                        requests.TryAdd(opt.BundleName, bundleCreateRequest);
                        loadedBundles.Add(opt.BundleName, bundleCreateRequest);
                        while (!bundleCreateRequest.isDone)
                        {
                            handle.SetProgress(CalculateProgress() * rateProgress);
                            yield return null;
                        }
                    }

#else
                    var uwr = UnityWebRequestAssetBundle.GetAssetBundle(location.InternalId);
                    uwr.SendWebRequest();
                    requests.TryAdd(opt.BundleName, uwr);
                    loadedBundles.Add(opt.BundleName, uwr);
                    
                    while (!uwr.isDone)
                    {
                        handle.SetProgress(CalculateProgress() * rateProgress);
                        yield return null;
                    }

                    if (uwr.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError("[LoadAsset] Failed to read StreamingAssets bundle: " + uwr.error);
                        handle.SetFail("[LoadAsset] Failed to read StreamingAssets bundle: " + uwr.error);
                        loadingBundles.Remove(opt.BundleName);
                        yield break;
                    }

                    var uwrBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    requests[opt.BundleName] = uwrBundle;
                    loadedBundles[opt.BundleName] = uwrBundle;
                    // Debug.LogError("[LoadAsset] " + DownloadHandlerAssetBundle.GetContent(uwr).name + uwr.result);
#endif
                }

                loadingBundles.Remove(opt.BundleName);
            }
        }

        loadedAsset.TryAdd(key, requests);

        var assetBundle = requests.First().Value switch
        {
            AssetBundleCreateRequest bundleCreateRequest => bundleCreateRequest.assetBundle,
            UnityWebRequest uwr => DownloadHandlerAssetBundle.GetContent(uwr),
            AssetBundle bundle => bundle,
            _ => null
        };
        if (assetBundle == null)
        {
            Debug.LogError("[LoadAsset] Failed to read file bundle");
            handle.SetFail("[LoadAsset] Failed to read file bundle");
            yield break;
        }
        var loadAssetAsync = assetBundle.LoadAssetAsync<T>(internalId);
        while (!loadAssetAsync.isDone)
        {
            handle.SetProgress(loadAssetAsync.progress * (1 - rateProgress) + CalculateProgress() * rateProgress);
            yield return null;
        }
        handle.SetResult(loadAssetAsync.asset as T);

        float CalculateProgress()
        {
            var progress = 0f;
            foreach (var location in locations)
            {
                if (location.Data is not AssetBundleRequestOptions opt) continue;
                if (loadedBundles.TryGetValue(opt.BundleName, out var bundle))
                {
                    progress += bundle switch
                    {
                        AssetBundleCreateRequest bcr => bcr.progress * ((float)opt.BundleSize / size),
                        UnityWebRequest uwr => uwr.downloadProgress * ((float)opt.BundleSize / size),
                        _ => 1
                    };
                }
            }
            return progress;
        }
    }

    public void Release<T>(ABOperationHandle<T> handle) where T : Object
    {
        if (handle != null)
        {
            handle.Release();
            loadedAsset.Remove(handle.key);
            var unusedBundles = GetUnusedBundles();
            foreach (var bundleCreate in unusedBundles)
            {
                unloadingBundles.Add(bundleCreate.Key);
                loadedBundles.Remove(bundleCreate.Key);

                var assetBundle = bundleCreate.Value switch
                {
                    AssetBundleCreateRequest bundleCreateRequest => bundleCreateRequest.assetBundle,
                    UnityWebRequest uwr => DownloadHandlerAssetBundle.GetContent(uwr),
                    AssetBundle bundle => bundle,
                    _ => null
                };

                if (assetBundle != null)
                {
                    assetBundle.UnloadAsync(true).completed += sync =>
                    {
                        unloadingBundles.Remove(bundleCreate.Key);
                    };
                }
            }
        }
    }

    public Dictionary<string, object> GetUnusedBundles()
    {
        Dictionary<string, object> result = new();

        foreach (var kv in loadedBundles)
        {
            var bundleReq = kv.Value;
            bool used = false;
            if (loadingBundles.Contains(kv.Key)) continue;
            foreach (var assetList in loadedAsset.Values)
            {
                if (assetList.Values.Contains(bundleReq))
                {
                    used = true;
                    break;
                }
            }

            if (!used)
                result.Add(kv.Key, kv.Value);
        }

        return result;
    }


    private void SetConfig()
    {
        if (!ManualUpdateCatalog && initialized)
        {
            StartCoroutine(IELoadRemoteCatalog());
        }
    }

    public void ParserFirConfig()
    {
#if FIRBASE_ENABLE
        var v = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("cf_catalog_info_1");
        if (v.StringValue != null && v.StringValue.Length > 0)
        {
            AssetBundleCatalog.CFCataloginfo = v.StringValue;
        }
        SetConfig();
#endif
    }
}