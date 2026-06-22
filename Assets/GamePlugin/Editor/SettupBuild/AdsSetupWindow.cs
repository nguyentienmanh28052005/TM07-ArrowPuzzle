using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;
using UnityEngine.Networking;

public class AdsSetupWindow : EditorWindow
{
    private bool useUnityIAP = true;
    private string unityIAPVersion;

    private bool useAdMob = true;
    private string admobVersion;
    private bool admobIncludeNativeAds = false;

    private bool useAppLovin = false;
    private string applovinVersion;

    private bool useIronSource = false;
    private string ironSourceVersion;

    private bool useAppsFlyer = true;
    private string appsFlyerVersion;

    private bool useFirebase = true;
    private string firebaseVersion;
    private bool firebaseAnalytics = true;
    private bool firebaseCrashlytics = true;
    private bool firebaseMessaging = true;
    private bool firebaseRemoteConfig = true;

    private bool useAudioAds = false;
    private bool useAudiomob = false;
    private string audiomobVersion;
    private bool usePlayOne = false;
    private string playoneVersion;

    private bool useAdCanvas = false;
    private bool useAdGasme = false;
    private string adGasmeVersion;
    private bool useAdverty = false;
    private string advertyVersion;

    private bool useFacebook = false;
    private string facebookVersion;

    private bool useGooglePlayGames = false;
    private string googlePlayGamesVersion;

    private SDKVersionConfig sdkVersions;
    
    private List<string> pendingImports = new List<string>();
    private int totalDownloads = 0;
    private int completedDownloads = 0;
    
    [MenuItem("Build Setup/Select SDKs")]
    public static void ShowWindow()
    {
        GetWindow<AdsSetupWindow>("SDK Setup");
    }

    private void OnEnable()
    {
        string path = "Assets/Editor/SDKSetup/sdk_versions.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            sdkVersions = JsonUtility.FromJson<SDKVersionConfig>(json);
        }
        else
        {
            sdkVersions = new SDKVersionConfig();
        }

        unityIAPVersion = sdkVersions.unity_iap;
        admobVersion = sdkVersions.admob;
        applovinVersion = sdkVersions.applovin;
        ironSourceVersion = sdkVersions.ironsource;
        appsFlyerVersion = sdkVersions.appsflyer;
        firebaseVersion = sdkVersions.firebase;
        audiomobVersion = sdkVersions.audiomob;
        playoneVersion = sdkVersions.playone;
        adGasmeVersion = sdkVersions.adgasme;
        advertyVersion = sdkVersions.adverty;
        facebookVersion = sdkVersions.facebook;
        googlePlayGamesVersion = sdkVersions.google_play_games;
    }

    private void OnGUI()
    {
        GUILayout.Label("Chọn các SDK để setup:", EditorStyles.boldLabel);

        useUnityIAP = EditorGUILayout.BeginToggleGroup("Unity IAP (In-App Purchase)", useUnityIAP);
        unityIAPVersion = EditorGUILayout.TextField("Version", unityIAPVersion);
        EditorGUILayout.EndToggleGroup();

        useAdMob = EditorGUILayout.BeginToggleGroup("AdMob SDK (download)", useAdMob);
        admobVersion = EditorGUILayout.TextField("Version", admobVersion);
        admobIncludeNativeAds = EditorGUILayout.Toggle("Include Native Ads", admobIncludeNativeAds);
        EditorGUILayout.EndToggleGroup();

        useAppLovin = EditorGUILayout.BeginToggleGroup("AppLovin SDK (download)", useAppLovin);
        applovinVersion = EditorGUILayout.TextField("Version", applovinVersion);
        EditorGUILayout.EndToggleGroup();

        useIronSource = EditorGUILayout.BeginToggleGroup("IronSource SDK (download)", useIronSource);
        ironSourceVersion = EditorGUILayout.TextField("Version", ironSourceVersion);
        EditorGUILayout.EndToggleGroup();

        useAppsFlyer = EditorGUILayout.BeginToggleGroup("AppsFlyer SDK (download)", useAppsFlyer);
        appsFlyerVersion = EditorGUILayout.TextField("Version", appsFlyerVersion);
        EditorGUILayout.EndToggleGroup();

        useFirebase = EditorGUILayout.BeginToggleGroup("Firebase SDK (download)", useFirebase);
        firebaseVersion = EditorGUILayout.TextField("Version", firebaseVersion);
        firebaseAnalytics = EditorGUILayout.Toggle("Firebase Analytics", firebaseAnalytics);
        firebaseCrashlytics = EditorGUILayout.Toggle("Firebase Crashlytics", firebaseCrashlytics);
        firebaseMessaging = EditorGUILayout.Toggle("Firebase Messaging", firebaseMessaging);
        firebaseRemoteConfig = EditorGUILayout.Toggle("Firebase Remote Config", firebaseRemoteConfig);
        EditorGUILayout.EndToggleGroup();

        useAudioAds = EditorGUILayout.BeginToggleGroup("Audio Ads SDK (download)", useAudioAds);
        useAudiomob = EditorGUILayout.Toggle("Use Audiomob", useAudiomob);
        if (useAudiomob)
            audiomobVersion = EditorGUILayout.TextField("Audiomob Version", audiomobVersion);
        usePlayOne = EditorGUILayout.Toggle("Use PlayOne", usePlayOne);
        if (usePlayOne)
            playoneVersion = EditorGUILayout.TextField("PlayOne Version", playoneVersion);
        EditorGUILayout.EndToggleGroup();

        useAdCanvas = EditorGUILayout.BeginToggleGroup("AdCanvas SDK (download)", useAdCanvas);
        useAdGasme = EditorGUILayout.Toggle("Use AdGame", useAdGasme);
        if (useAdGasme)
            adGasmeVersion = EditorGUILayout.TextField("AdGame Version", adGasmeVersion);
        useAdverty = EditorGUILayout.Toggle("Use Adverty", useAdverty);
        if (useAdverty)
            advertyVersion = EditorGUILayout.TextField("Adverty Version", advertyVersion);
        EditorGUILayout.EndToggleGroup();

        useFacebook = EditorGUILayout.BeginToggleGroup("Facebook SDK", useFacebook);
        facebookVersion = EditorGUILayout.TextField("Version", facebookVersion);
        EditorGUILayout.EndToggleGroup();

        useGooglePlayGames = EditorGUILayout.BeginToggleGroup("Google Play Games SDK", useGooglePlayGames);
        googlePlayGamesVersion = EditorGUILayout.TextField("Version", googlePlayGamesVersion);
        EditorGUILayout.EndToggleGroup();

        GUILayout.Space(15);
        if (GUILayout.Button("❌ Xoá tất cả SDK đã import"))
        {
            if (EditorUtility.DisplayDialog("Xác nhận", "Bạn có chắc muốn xoá tất cả SDK đã import không?", "Xoá", "Huỷ"))
            {
                RemoveImportedSDKs();
            }
        }

        if (GUILayout.Button("✅ Setup Selected SDKs"))
        {
            SetupSelectedSDKs();
        }
    }

    private void RemoveImportedSDKs()
    {
        string[] sdkFolders = new string[]
        {
            "Assets/AppsFlyer",

            "Assets/GoogleMobileAds",
            "Assets/GoogleMobileAdsNative",
            
            "Assets/Firebase",
            "Assets/Editor Default Resources",
            
            "Assets/IronSource",
            "Assets/LevelPlay",
            
            "Assets/MaxSdk",
            
            "Assets/Plugins/Audiomob",
            "Assets/Odeeo",
            
            "Assets/Adverty",
            "Assets/Gadsme",
            
            "Assets/FacebookSDK",
            "Assets/GooglePlayGames",
            "Assets/ExternalDependencyManager",
            "Assets/PlayServicesResolver",
            
            "Assets/Plugins/Android",
            "Assets/Plugins/iOS",
            "Assets/Plugins/tvOS",
        };

        foreach (var folder in sdkFolders)
        {
            if (Directory.Exists(folder))
            {
                FileUtil.DeleteFileOrDirectory(folder);
                FileUtil.DeleteFileOrDirectory(folder + ".meta");
                Debug.Log($"🧹 Đã xoá: {folder}");
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Xoá SDK", "Đã xoá xong tất cả SDK đã import!", "OK");
    }
    
    private void SetupSelectedSDKs()
    {
        pendingImports.Clear();
        completedDownloads = 0;
        totalDownloads = 0;
        EnsureGradleTemplateFiles();
        if (useUnityIAP)
            AddUPMPackage("Unity IAP", "com.unity.purchasing", unityIAPVersion);

        if (useAdMob)
        {
            string fileName = $"GoogleMobileAds_{admobVersion}.unitypackage";
            string url = $"https://github.com/googleads/googleads-mobile-unity/releases/download/v{admobVersion}/GoogleMobileAds-v{admobVersion}.unitypackage";

            DownloadToGlobalCache("AdMob SDK", url, fileName);
          
            if (admobIncludeNativeAds)
            {
                fileName = $"GoogleMobileAds-native.unitypackage";
                url = $"https://dl.google.com/googleadmobadssdk/GoogleMobileAds-native.unitypackage";

                DownloadToGlobalCache("AdMob SDK", url, fileName);
            }
        }

        if (useAppLovin)
        {
            DownloadAppLovinLatest("AppLovin SDK", applovinVersion);
        }

        if (useIronSource)
        {
            string fileName = $"UnityLevelPlay_v{ironSourceVersion}.unitypackage";
            string url = $"https://github.com/ironsource-mobile/Unity-sdk/releases/download/UnityLevelPlay_{ironSourceVersion}/{fileName}";
            DownloadToGlobalCache("IronSource SDK", url, fileName);
        }

        if (useAppsFlyer)
        {
            https://github.com/AppsFlyerSDK/appsflyer-unity-plugin/blob/v6.16.21/appsflyer-unity-plugin-6.16.21.unitypackage
            string filename = $"appsflyer-unity-plugin-{appsFlyerVersion}.unitypackage";
            string url = $"https://github.com/AppsFlyerSDK/appsflyer-unity-plugin/raw/{appsFlyerVersion}/{filename}";
            DownloadToGlobalCache("AppsFlyer SDK", url, filename);
        }

        if (useFirebase)
        {
            string filenamePrefix = $"firebase_unity_sdk_{firebaseVersion}/";

            if (firebaseAnalytics)
            {
                string url = $"https://dl.google.com/firebase/sdk/unity/dotnet4/FirebaseAnalytics_{firebaseVersion}.unitypackage";
                DownloadToGlobalCache("Firebase Analytics", url, filenamePrefix + "FirebaseAnalytics.unitypackage");
            }

            if (firebaseCrashlytics)
            {
                string url = $"https://dl.google.com/firebase/sdk/unity/dotnet4/FirebaseCrashlytics_{firebaseVersion}.unitypackage";
                DownloadToGlobalCache("Firebase Crashlytics", url, filenamePrefix + "FirebaseCrashlytics.unitypackage");
            }

            if (firebaseMessaging)
            {
                string url = $"https://dl.google.com/firebase/sdk/unity/dotnet4/FirebaseMessaging_{firebaseVersion}.unitypackage";
                DownloadToGlobalCache("Firebase Messaging", url, filenamePrefix + "FirebaseMessaging.unitypackage");
            }

            if (firebaseRemoteConfig)
            {
                string url = $"https://dl.google.com/firebase/sdk/unity/dotnet4/FirebaseRemoteConfig_{firebaseVersion}.unitypackage";
                DownloadToGlobalCache("Firebase Remote Config", url, filenamePrefix + "FirebaseRemoteConfig.unitypackage");
            }
        }
        
        if (useAudioAds)
        {
            if (useAudiomob)
            {
                string fileName = $"AudioMob_{audiomobVersion}.unitypackage";
                string url = $"";
                DownloadToGlobalCache("AudioMob SDK", url, fileName);
            }

            if (usePlayOne)
            {
                string fileName = $"playonv{playoneVersion}.unitypackage";
                string url = $"";
                DownloadToGlobalCache("AudioMob SDK", url, fileName);
            }
        }
        
        if (useAdCanvas)
        {
            if (useAdverty)
            {
                string fileName = $"AdvertySDK_{advertyVersion}.unitypackage";
                string url = $"";
                DownloadToGlobalCache("Adverty SDK", url, fileName);
            }

            if (useAdGasme)
            {
                string fileName = $"GadsmeSDK_{adGasmeVersion}.unitypackage";
                string url = $"";
                DownloadToGlobalCache("Gasme SDK", url, fileName);
            }
        }
        
        if (useFacebook)
        {
            string fileName = $"facebook-unity-sdk-{facebookVersion}.unitypackage";
            string url = $"https://github.com/facebook/facebook-sdk-for-unity/releases/download/sdk-version-{facebookVersion}/facebook-unity-sdk-{facebookVersion}.zip";
            DownloadToGlobalCacheZip("Facebook SDK", url, fileName);
        }

        if (useGooglePlayGames)
        {
            https://github.com/playgameservices/play-games-plugin-for-unity/blob/master/current-build/GooglePlayGamesPlugin-2.1.0.unitypackage
            string fileName = $"GooglePlayGamesPlugin-{googlePlayGamesVersion}.unitypackage";
            string url = $"https://github.com/playgameservices/play-games-plugin-for-unity/raw/v{googlePlayGamesVersion}/current-build/{fileName}";
            DownloadToGlobalCache("Google Play Games SDK", url, fileName);
        }
    }

    private void EnsureGradleTemplateFiles()
    {
        string androidPath = "Assets/Plugins/Android";
        if (!Directory.Exists(androidPath))
            Directory.CreateDirectory(androidPath);

        CreateIfMissing("mainTemplate.gradle", @"// Unity default mainTemplate.gradle\napply plugin: 'com.android.application'");
        CreateIfMissing("gradleTemplate.properties", @"org.gradle.jvmargs=-Xmx4096M\nandroid.useAndroidX=true\nandroid.enableJetifier=true");
        CreateIfMissing("AndroidManifest.xml", @"<manifest xmlns:android=""http://schemas.android.com/apk/res/android\"" package=\""com.DefaultCompany.App\""><application><activity android:name=\""com.unity3d.player.UnityPlayerActivity\""></activity></application></manifest>");
        AssetDatabase.Refresh();
    }
    
    private void CreateIfMissing(string fileName, string content)
    {
        string path = Path.Combine("Assets/Plugins/Android", fileName);
        if (!File.Exists(path))
        {
            File.WriteAllText(path, content);
            Debug.Log($"📝 Đã tạo {fileName}");
        }
    }
    
    private void AddUPMPackage(string name, string packageId, string version)
    {
        string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
        if (!File.Exists(manifestPath))
        {
            Debug.LogError("Không tìm thấy manifest.json!");
            return;
        }

        string json = File.ReadAllText(manifestPath);
        if (json.Contains($"\"{packageId}\""))
        {
            Debug.Log($"✅ {name} đã tồn tại.");
            return;
        }

        int index = json.LastIndexOf("}");
        if (index == -1)
        {
            Debug.LogError("manifest.json lỗi format.");
            return;
        }

        string line = $",\n    \"{packageId}\": \"{version}\"";
        json = json.Insert(index - 1, line);
        File.WriteAllText(manifestPath, json);
        Debug.Log($"✅ Đã thêm {name} ({packageId}@{version}) vào manifest.");
        AssetDatabase.Refresh();
    }

    private string GetGlobalCachePath()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string path = Path.Combine(home, ".unity-sdk-cache");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        return path;
    }

    private void DownloadAppLovinLatest(string sdkName, string version)
    {
        string tag = $"release_{version.Replace(".", "_")}";
        string apiUrl = $"https://api.github.com/repos/AppLovin/AppLovin-MAX-Unity-Plugin/releases/tags/{tag}";

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("User-Agent", "UnityEditor"); // Bắt buộc với GitHub API

        var operation = request.SendWebRequest();
        operation.completed += _ =>
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Lỗi khi gọi GitHub API: " + request.error);
                EditorUtility.DisplayDialog("Lỗi", "Không thể lấy thông tin từ GitHub:\n" + request.error, "OK");
                return;
            }

            string json = request.downloadHandler.text;

            // Parse tag_name và asset .unitypackage
            var release = JsonUtility.FromJson<GitHubReleaseWrapper>(json);
            string version = release.tag_name;

            GitHubReleaseAsset unityPackageAsset = null;
            foreach (var asset in release.assets)
            {
                if (asset.name.EndsWith(".unitypackage"))
                {
                    unityPackageAsset = asset;
                    break;
                }
            }

            if (unityPackageAsset == null)
            {
                Debug.LogError("❌ Không tìm thấy file .unitypackage trong bản phát hành.");
                EditorUtility.DisplayDialog("Lỗi", "Không có file .unitypackage trong bản phát hành AppLovin", "OK");
                return;
            }

            string fileName = unityPackageAsset.name;
            string downloadUrl = unityPackageAsset.browser_download_url;
            Debug.Log($"📦 Tải AppLovin v{version}: {fileName}");
            DownloadToGlobalCache(sdkName, downloadUrl, fileName);
        };
    }
    
    private void DownloadToGlobalCache(string sdkName, string url, string fileName, bool autoImport = true)
    {
        string cachePath = GetGlobalCachePath();
        string fullPath = Path.Combine(cachePath, fileName);
        totalDownloads++;

        string localPath = Path.Combine("Assets/GamePlugin/Editor/PackageCache", fileName);
        if (File.Exists(localPath))
        {
            Debug.Log($"📦 Dùng file local cho {sdkName}: {localPath}");
            if (autoImport)
                ImportPackage(sdkName, localPath);
            return;
        }
        
        if (File.Exists(fullPath))
        {
            Debug.Log($"✅ {sdkName} đã có trong cache: {fullPath}");
            if (autoImport && fileName.EndsWith(".unitypackage"))
            {
                ImportPackage(sdkName, fullPath);
            }

            return;
        }

        EditorUtility.DisplayProgressBar($"Đang tải {sdkName}", $"Tải từ: {url}", 0.5f);

        UnityWebRequest www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
        www.downloadHandler = new DownloadHandlerFile(fullPath);
        var operation = www.SendWebRequest();
        operation.completed += _ =>
        {
            EditorUtility.ClearProgressBar();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ Đã tải {sdkName} về: {fullPath}");

                if (autoImport && fileName.EndsWith(".unitypackage"))
                {
                    ImportPackage(sdkName, fullPath);
                }
            }
            else
            {
                Debug.LogError($"❌ Lỗi khi tải {sdkName}: {www.error}");
                EditorUtility.DisplayDialog($"Lỗi tải {sdkName}", $"Không thể tải từ:\n{url}", "OK");
            }
        };
    }

    private void DownloadToGlobalCacheZip(string sdkName, string url, string fileName, bool autoImport = true)
    {
        string localPath = Path.Combine("Assets/GamePlugin/Editor/PackageCache", fileName);
        if (File.Exists(localPath))
        {
            Debug.Log($"📦 Dùng file local cho {sdkName}: {localPath}");
            if (autoImport)
                ImportPackage(sdkName, localPath);
            return;
        }
        
        string cachePath = GetGlobalCachePath();
        string fullPath = Path.Combine(cachePath, fileName);
        string zipPath = Path.Combine(cachePath, fileName.Replace(".unitypackage", ".zip"));
        totalDownloads++;

        if (File.Exists(fullPath))
        {
            Debug.Log($"✅ {sdkName} đã có trong cache: {fullPath}");
            if (autoImport && fileName.EndsWith(".unitypackage"))
            {
                ImportPackage(sdkName, fullPath);
            }

            return;
        }

        EditorUtility.DisplayProgressBar($"Đang tải {sdkName}", $"Tải từ: {url}", 0.5f);

        UnityWebRequest www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
        www.downloadHandler = new DownloadHandlerFile(zipPath);
        var operation = www.SendWebRequest();
        operation.completed += _ =>
        {
            EditorUtility.ClearProgressBar();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ Đã tải {sdkName} về: {fullPath}");

                if (autoImport && fileName.EndsWith(".unitypackage"))
                {
                    ExtractUnityPackage(zipPath, fullPath);
                    File.Delete(zipPath);
                    ImportPackage(sdkName, fullPath);
                }
            }
            else
            {
                Debug.LogError($"❌ Lỗi khi tải {sdkName}: {www.error}");
                EditorUtility.DisplayDialog($"Lỗi tải {sdkName}", $"Không thể tải từ:\n{url}", "OK");
            }
        };
    }

    private static void ExtractUnityPackage(string zipPath, string outputPath)
    {
        using (ZipArchive archive = ZipFile.OpenRead(zipPath))
        {
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.EndsWith(".unitypackage", StringComparison.OrdinalIgnoreCase))
                {
                    entry.ExtractToFile(outputPath, overwrite: true);
                    Debug.Log($"✅ Đã giải nén {entry.FullName} → {outputPath}");
                    return;
                }
            }
        }

        Debug.LogError("❌ Không tìm thấy file .unitypackage trong .zip.");
        EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy file .unitypackage trong file .zip", "OK");
    }

    private void ImportAllPendingPackages()
    {
        foreach (var file in pendingImports)
        {
            Debug.Log($"📦 Importing {file}...");
            AssetDatabase.ImportPackage(file, false);
        }

        AssetDatabase.Refresh();
        pendingImports.Clear();
        
        string[] sdkFolders = new string[]
        {
            // Các thư mục cần xoá và re-import lại
            "Assets/ExternalDependencyManager",
            "Assets/PlayServicesResolver",
        };

        foreach (var folder in sdkFolders)
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder);
                File.Delete(folder + ".meta");
                Debug.Log($"🧹 Đã xoá: {folder}");
            }
        }

        ImportExternalDependencyManager();
        AssetDatabase.Refresh();

        Debug.Log("✅ Tất cả package đã được import xong!");
    }

    private void ImportExternalDependencyManager()
    {
        string highestVersion = null;

        Version v = GetHighestEDMVersion();
        if (v != null) highestVersion = v.ToString();

        string fileName = $"external-dependency-manager-{highestVersion}.unitypackage";
        string localCachePath = Path.Combine(GetGlobalCachePath(), fileName);

        if (File.Exists(localCachePath))
        {
            Debug.Log($"✅ EDM đã có trong cache: {localCachePath}");
            ImportPackage("External Dependency Manager", localCachePath);
            return;
        }

        if (highestVersion == "0.0.0" || string.IsNullOrEmpty(highestVersion))
        {
            string apiUrl = $"https://api.github.com/repos/googlesamples/unity-jar-resolver/releases/latest";

            UnityWebRequest request = UnityWebRequest.Get(apiUrl);
            request.SetRequestHeader("User-Agent", "UnityEditor"); // Bắt buộc với GitHub API

            var operation = request.SendWebRequest();
            operation.completed += _ =>
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("❌ Lỗi khi gọi GitHub API: " + request.error);
                    EditorUtility.DisplayDialog("Lỗi", "Không thể lấy thông tin từ GitHub:\n" + request.error, "OK");
                    return;
                }

                string json = request.downloadHandler.text;

                // Parse tag_name và asset .unitypackage
                var release = JsonUtility.FromJson<GitHubReleaseWrapper>(json);
                string version = release.tag_name.Replace("v", "");

                string fileName = $"external-dependency-manager-{version}.unitypackage";
                string url = $"https://github.com/googlesamples/unity-jar-resolver/raw/v{version}/{fileName}";
                DownloadToGlobalCache("External Dependency Manager", url, fileName);
            };
        }
        else
        {
            https://github.com/googlesamples/unity-jar-resolver/blob/master/external-dependency-manager-1.2.186.unitypackage
            string url = $"https://github.com/googlesamples/unity-jar-resolver/raw/v{highestVersion}/{fileName}";
            DownloadToGlobalCache("External Dependency Manager", url, fileName);
        }
        
    }

    private Version GetHighestEDMVersion()
    {
        string basePath = "Assets/ExternalDependencyManager/Editor";
        if (!Directory.Exists(basePath))
        {
            Debug.LogWarning("❌ Không tìm thấy thư mục ExternalDependencyManager/Editor.");
            return null;
        }

        string[] versionFolders = Directory.GetDirectories(basePath);
        Version highest = new Version("0.0.0");

        foreach (string folder in versionFolders)
        {
            string versionName = Path.GetFileName(folder);
            if (Version.TryParse(versionName, out Version version))
            {
                if (version > highest)
                    highest = version;
            }
        }

        Debug.Log($"📌 EDM version cao nhất: {highest}");
        return highest;
    }

    private void ImportPackage(string sdkName, string fullPath)
    {
        if (sdkName == "External Dependency Manager")
        {
            AssetDatabase.ImportPackage(fullPath, false);
        }
        else
        {
            pendingImports.Add(fullPath);
            completedDownloads++;

            if (completedDownloads == totalDownloads)
            {
                EditorApplication.delayCall += ImportAllPendingPackages;
            }
        }
        
    }
}

[System.Serializable]
public class SDKVersionConfig
{
    public string unity_iap = "4.10.0";
    public string admob = "latest";
    public string appsflyer = "latest";
    public string firebase = "latest";
    public string applovin = "latest";
    public string ironsource = "latest";
    public string audiomob;
    public string playone;
    public string adgasme;
    public string adverty;
    public string facebook;
    public string google_play_games;
}

[Serializable]
public class GitHubReleaseWrapper
{
    public string tag_name;
    public string zipball_url;
    public GitHubReleaseAsset[] assets;
}

[Serializable]
public class GitHubReleaseAsset
{
    public string name;
    public string browser_download_url;
}
