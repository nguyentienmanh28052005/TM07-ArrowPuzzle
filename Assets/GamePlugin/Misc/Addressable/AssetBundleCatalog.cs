using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;

public class AssetBundleCatalog
{
    public static string CachedCatalogPath => Path.Combine(Application.persistentDataPath, "catalog_cache.json");

    public static int CacheVersionCatalog
    {
        get => PlayerPrefs.GetInt("cache_version_catalog", 0);
        set => PlayerPrefs.SetInt("cache_version_catalog", value);
    }

    public static string CFCataloginfo
    {
        get => PlayerPrefs.GetString("cf_catalog_info", "{\"name\":\"11\",\"remoteBaseUrl\":\"\", \"version\":0}");
        set => PlayerPrefs.SetString("cf_catalog_info", value);
    }


    public static IEnumerator IELoadCatalog(string catalogUrlOrPath, Action<int> callback)
    {
        // 1. Load JSON
        string json = null;
        bool done = false;

        yield return LoadJson(catalogUrlOrPath, s =>
        {
            json = s;
            done = true;
        });

        if (!done || string.IsNullOrEmpty(json))
        {
            Debug.LogError("[CustomCatalog] JSON empty!");
            callback?.Invoke(-1);
            yield break;
        }

        // 2. Parse JSON → ContentCatalogData
        var catalogData = JsonUtility.FromJson<ContentCatalogData>(json);
        if (catalogData == null)
        {
            Debug.LogError("[CustomCatalog] JSON parse fail!");
            callback?.Invoke(-1);
            yield break;
        }

        // 3. Create locator
        IResourceLocator locator = catalogData.CreateLocator();

        // 4. Register locator
        if (AssetBundleManager.resourceLocators.Count > 1)
            AssetBundleManager.resourceLocators.RemoveRange(1, AssetBundleManager.resourceLocators.Count - 1);
        AssetBundleManager.resourceLocators.Add(locator);

        Debug.Log("[CustomCatalog] Loaded locator = " + catalogUrlOrPath);
        callback?.Invoke(1);
    }

    //=====================================================
    // 1. LOAD JSON (local hoặc remote)
    //=====================================================
    private static IEnumerator LoadJson(string path, Action<string> callback)
    {
        Debug.Log("[CustomCatalog] Download url: " + path);
        // --- Remote URL ---
        if (path.StartsWith("http") || path.StartsWith("file://") || path.StartsWith("jar:file://"))
        {
            using (UnityWebRequest req = UnityWebRequest.Get(path))
            {
                req.timeout = 15;
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("[CustomCatalog] Download fail: " + req.error + ",url=" + path);
                    callback(null);
                    yield break;
                }

                var json = req.downloadHandler.text;
                if (path.StartsWith("http"))
                {
                    json = json.Replace("\"m_LocatorId\":\"AddressablesMainContentCatalog\"", $"\"m_LocatorId\":\"{path}\"");
                    Debug.Log($"[CustomCatalog] Load Catalog Online: " + path);
                    Debug.Log("[CustomCatalog] Json Data: " + json);
                    File.WriteAllText(CachedCatalogPath, json);
                }
                callback(json);
                yield break;
            }
        }

        if (!File.Exists(path))
        {
            Debug.LogError("[CustomCatalog] Local file NOT FOUND: " + path);
            callback(null);
            yield break;
        }
        Debug.Log("[CustomCatalog] Data Local path: " + path);
        callback(File.ReadAllText(path));

    }
}