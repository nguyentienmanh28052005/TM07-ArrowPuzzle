#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class AddressableAssetsBuild
{
    private static byte[] cacheCatalog;

    public static string GetPlayModeScriptName()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return "Addressables settings not found";
        var builder = settings.GetDataBuilder(settings.ActivePlayModeDataBuilderIndex);
        return builder != null ? builder.Name : "Play Mode Script not set";
    }

    public static int GetPlayModeScriptIndex()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return -1;
        return settings.ActivePlayModeDataBuilderIndex;
    }

    // ======================== 🔹 Build Local Catalog ========================
    [MenuItem("Tools/Addressables/Build")]
    public static void BuildAddressables()
    {
        BuildAddressables(isRemote: true);
        AssetDatabase.Refresh();
        EditorApplication.delayCall += () =>
        {
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.Refresh();
                CopyCatalogToStreamingAssets("remote_catalog.json");
                AssetDatabase.Refresh();

                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.Refresh();
                    BuildAddressables(isRemote: false);

                    EditorApplication.delayCall += () =>
                    {
                        AssetDatabase.Refresh();
                        CopyCatalogToServerBuild("remote_catalog.json");
                        AssetDatabase.Refresh();
                        CreateS3CatalogCopies();
                        AssetDatabase.Refresh();
                    };
                };
            };
        };
    }

    [MenuItem("Tools/Addressables/Clear Cache")]
    public static void AddressablesClearFolderCache()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        AddressableAssetSettings.CleanPlayerContent(settings.ActivePlayerDataBuilder);
        // 1. Xóa Library/com.unity.addressables
        string addressablesFolder = Path.Combine(Application.dataPath, "../Library/com.unity.addressables");
        addressablesFolder = Path.GetFullPath(addressablesFolder);
        DeleteFolderIfExists(addressablesFolder);
        // 2. Xóa ServerData/[Platform]
        string serverDataRoot = Path.Combine(Application.dataPath, "../ServerData");
        string platformFolder = null;

#if UNITY_ANDROID
        platformFolder = "Android";
#elif UNITY_IOS
        platformFolder = "iOS";
#endif

        if (!string.IsNullOrEmpty(platformFolder))
        {
            string platformPath = Path.Combine(serverDataRoot, platformFolder);
            platformPath = Path.GetFullPath(platformPath);
            DeleteFolderIfExists(platformPath);
        }
        void DeleteFolderIfExists(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                try
                {
                    Directory.Delete(folderPath, true);
                    Debug.Log("Deleted folder: " + folderPath);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to delete {folderPath}: {e}");
                }
            }
            else
            {
                Debug.Log($"Folder not found: {folderPath}");
            }
        }
    }

    // =======================================================================
    private static void BuildAddressables(bool isRemote)
    {
        string platform = EditorUserBuildSettings.activeBuildTarget.ToString();
        string newDir = "AddressableAssetsData";
        string newPath = Path.Combine(newDir, $"{platform}_{(isRemote ? "remote" : "local")}_content_state.bin");

        var settings = AddressableAssetSettingsDefaultObject.Settings;

        // settings.BuildRemoteCatalog = isRemote;
        EditorApplication.delayCall += () =>
        {
            AssetDatabase.Refresh();
            // 🔹 Set IncludeInBuild cho group
            foreach (var g in settings.groups)
            {
                var bundled = g.GetSchema<BundledAssetGroupSchema>();
                if (bundled != null)
                {
                    bool isBuild = g.Name.Contains("Common") || (isRemote ? g.Name.Contains("Remote") : !g.Name.Contains("Remote"));
                    bundled.IncludeInBuild = isBuild;
                }

                var player = g.GetSchema<PlayerDataGroupSchema>();
                if (player != null)
                {
                    player.IncludeResourcesFolders = !isRemote;
                    player.IncludeBuildSettingsScenes = !isRemote;
                }
            }

            // 🔹 Nếu chưa có state file → build mới
            if (!File.Exists(newPath) || !isRemote)
            {
                Debug.Log($"🧱 Start build {(isRemote ? "Remote" : "Local")} catalog...");
                AddressableAssetSettings.BuildPlayerContent();
                Debug.Log($"✅ {(isRemote ? "Remote" : "Local")} catalog built successfully!");
            }
            else
            {
                Debug.Log($"🧱 Start update {(isRemote ? "Remote" : "Local")} content...");
                ContentUpdateScript.BuildContentUpdate(settings, newPath);
                Debug.Log($"✅ {(isRemote ? "Remote" : "Local")} content update complete!");
            }
            // 🔹 Copy addressables_content_state.bin ra vị trí cố định
            string oldPath = settings.ContentStateBuildPath + "/addressables_content_state.bin";
            if (File.Exists(oldPath))
            {
                if (!Directory.Exists(newDir))
                    Directory.CreateDirectory(newDir);

                if (File.Exists(newPath))
                    File.Delete(newPath);

                File.Copy(oldPath, newPath);
                Debug.Log($"📦 Saved content state: {newPath}");
            }
            else
            {
                Debug.LogError("❌ Không tìm thấy addressables_content_state.bin sau build!");
            }
            AssetDatabase.Refresh();
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.Refresh();
                // 🔹 Reset lại
                // settings.BuildRemoteCatalog = true;

                foreach (var g in settings.groups)
                {
                    var bundled = g.GetSchema<BundledAssetGroupSchema>();
                    if (bundled != null) bundled.IncludeInBuild = true;

                    var player = g.GetSchema<PlayerDataGroupSchema>();
                    if (player != null)
                    {
                        player.IncludeResourcesFolders = true;
                        player.IncludeBuildSettingsScenes = true;
                    }
                }
                AssetDatabase.Refresh();
            };
        };
    }

    private const string OldDomain = "https://data4gamev2.backendxgame.xyz";
    private const string S3Domain = "https://xgame-app-data.s3.ap-southeast-2.amazonaws.com";

    private static void CreateS3CatalogCopies()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var buildPath = settings.RemoteCatalogBuildPath.GetValue(settings);
        var ver = settings.profileSettings.EvaluateString(settings.activeProfileId, settings.OverridePlayerVersion);

        var srcJson = Path.Combine(buildPath, $"catalog_{ver}.json");
        var srcHash = Path.Combine(buildPath, $"catalog_{ver}.hash");
        var dstJson = Path.Combine(buildPath, $"catalog_s3_{ver}.json");
        var dstHash = Path.Combine(buildPath, $"catalog_s3_{ver}.hash");

        if (!File.Exists(srcJson) || !File.Exists(srcHash))
        {
            Debug.LogError($"Missing catalog/json or hash: {srcJson} / {srcHash}");
            return;
        }

        // 1) JSON: thay domain + ghi file mới
        var json = File.ReadAllText(srcJson);
        var jsonS3 = json.Replace(OldDomain, S3Domain);
        File.WriteAllText(dstJson, jsonS3);

        // 2) Hash: tính lại từ nội dung JSON S3 để khớp
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonS3);
        var h = UnityEngine.Hash128.Compute(bytes).ToString();
        File.WriteAllText(dstHash, h);

        Debug.Log($"Created S3 catalog: {dstJson}\nHash: {dstHash}");
    }

    private static void CopyCatalogToStreamingAssets(string fileName)
    {
        // 🔹 Platform hiện tại
        string platform = GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget);
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var buildPath = settings.RemoteCatalogBuildPath.GetValue(settings);

        // 🔹 Nguồn catalog (sau khi build)
        var ver = settings.profileSettings.EvaluateString(settings.activeProfileId, settings.OverridePlayerVersion);
        string sourcePath = Path.Combine(buildPath, $"catalog_{ver}.json");
        if (!File.Exists(sourcePath))
        {
            Debug.LogError($"❌ Không tìm thấy catalog.json ở {sourcePath}. Hãy build Addressables trước!");
            return;
        }

        // 🔹 Đích đến trong StreamingAssets
        string destDir = Path.Combine(Application.dataPath, "StreamingAssets/com.unity.addressables/aa", platform);
        string destPath = Path.Combine(destDir, fileName);

        if (!Directory.Exists(destDir))
            Directory.CreateDirectory(destDir);

        File.Copy(sourcePath, destPath, true);
        Debug.Log($"✅ Copied catalog.json → {destPath}");
    }

    private static void CopyCatalogToServerBuild(string fileName)
    {
        // 🔹 Platform hiện tại
        string platform = GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget);
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var buildPath = settings.RemoteCatalogBuildPath.GetValue(settings);

        // 🔹 Nguồn catalog (sau khi build)
        var ver = settings.profileSettings.EvaluateString(settings.activeProfileId, settings.OverridePlayerVersion);
        string sourcePath = Path.Combine(buildPath, $"catalog_{ver}.json");
        if (!File.Exists(sourcePath))
        {
            Debug.LogError($"❌ Không tìm thấy catalog.json ở {sourcePath}. Hãy build Addressables trước!");
            return;
        }

        // 🔹 Đích đến trong StreamingAssets
        string destDir = Path.Combine(Application.dataPath, "StreamingAssets/com.unity.addressables/aa", platform);
        string destPath = Path.Combine(destDir, fileName);

        if (!Directory.Exists(destDir))
            Directory.CreateDirectory(destDir);

        File.Copy(destPath, sourcePath, true);
        Debug.Log($"✅ Copied catalog.json → {destPath}");
    }

    private static string GetPlatformFolder(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android: return "Android";
            case BuildTarget.iOS: return "iOS";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64: return "StandaloneWindows64";
            case BuildTarget.WebGL: return "WebGL";
            default: return target.ToString();
        }
    }
}
#endif
