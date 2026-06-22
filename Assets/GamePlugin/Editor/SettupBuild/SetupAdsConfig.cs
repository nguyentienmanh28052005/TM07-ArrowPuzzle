using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using Newtonsoft.Json;
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using System.IO;
using Newtonsoft.Json;
using System;

public class UnifiedAdapterCrawlerEditor : EditorWindow
{
    private Vector2 scroll;
    private static string log = "";
    private string gmaInput = "";
    private Dictionary<string, AdapterWithSdk> gmaLookupResult;
    
    [MenuItem("Build Setup/Mediation Adapter Crawler")]
    public static void ShowWindow() => GetWindow<UnifiedAdapterCrawlerEditor>("Adapter Crawler");

    private void OnGUI()
    {
        GUILayout.Space(10);
        if (GUILayout.Button("🔄 Crawl AppLovin Adapters"))
        {
            log = "🔍 Start crawling AppLovin...\n";
            EditorCoroutineUtility.StartCoroutineOwnerless(ApplovinAdapterCrawler.CrawlAll(AppendLog));
        }

        if (GUILayout.Button("🔄 Crawl ironSource Adapters"))
        {
            log = "🔍 Start crawling ironSource...\n";
            EditorCoroutineUtility.StartCoroutineOwnerless(IronSourceAdapterCrawler.DoCrawl(AppendLog));
        }

        if (GUILayout.Button("🔄 Crawl AdMob Adapters"))
        {
            log = "🔍 Start crawling AdMob...\n";
            EditorCoroutineUtility.StartCoroutineOwnerless(AdmobMediationScraper.DoCrawl(AppendLog));
        }

        GUILayout.Space(20);
        GUILayout.Label("🔍 Tra cứu adapter theo GMA SDK Version", EditorStyles.boldLabel);
        gmaInput = EditorGUILayout.TextField("GMA SDK Version", gmaInput);
        if (GUILayout.Button("Tra cứu adapter tương ứng"))
        {
            LookupGmaVersion(gmaInput);
        }

        if (gmaLookupResult != null)
        {
            if (GUILayout.Button("Set version Adapter"))
            {
                log = "";
                string basePath = "Assets/MaxSdk/Mediation"; // <- thay đúng path
                foreach (var kvp in gmaLookupResult)
                {
                    string adapterName = kvp.Key; // ví dụ: "unityads"
                    var data = kvp.Value;

                    string folderName = adapterName switch
                    {
                        "Unity" => "UnityAds",
                        "unity" => "UnityAds",
                        "unityads" => "UnityAds",
                        "chartboost" => "Chartboost",
                        "Ironsource" => "IronSource",
                        "mintegral" => "Mintegral",
                        "meta" => "Facebook",
                        "Inmobi" => "InMobi",
                        "inmobi" => "InMobi",
                        "imobile" => "InMobi",
                        "Pangle" => "ByteDance",
                        "pangle" => "ByteDance",
                        _ => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(adapterName)
                    };

                    string folderPath = Path.Combine(basePath, folderName);
                    string xmlPath = Path.Combine(folderPath, "Editor/Dependencies.xml");

                    if (!File.Exists(xmlPath))
                    {
                        Debug.LogWarning($"Không tìm thấy file: {xmlPath}");
                        AppendLog($"Không tìm thấy file: {xmlPath}");
                        continue;
                    }

                    string content = File.ReadAllText(xmlPath);
                    string updated = ReplaceVersionInfoAplovin(content, data, out var logs);

                    if (updated != content)
                    {
                        File.WriteAllText(xmlPath, updated);
                        foreach (var lg in logs)
                        {
                            AppendLog(lg);
                            Debug.Log($"[Adapter Version Updated] {Path.GetFileName(xmlPath)} - {lg}");
                        }
                    }
                }

                basePath = "LevelPlay/Editor"; // <- thay đúng path
                foreach (var kvp in gmaLookupResult)
                {
                    string adapterName = kvp.Key; // ví dụ: "unityads"
                    var data = kvp.Value;

                    string folderName = adapterName switch
                    {
                        "applovin" => "ISAppLovinAdapterDependencies",
                        "chartboost" => "ISChartboostAdapterDependencies",
                        "inmobi" => "ISInMobiAdapterDependencies",
                        "meta" => "ISFacebookAdapterDependencies",
                        "mintegral" => "ISMintegralAdapterDependencies",
                        "mytarget" => "ISMyTargetAdapterDependencies",
                        "pangle" => "ISPangleAdapterDependencies",
                        "unity" => "ISUnityAdsAdapterDependencies",
                        _ => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(adapterName)
                    };

                    string folderPath = Path.Combine(basePath, folderName);
                    string xmlPath = folderPath + ".xml";

                    if (!File.Exists(xmlPath))
                    {
                        Debug.LogWarning($"Không tìm thấy file: {xmlPath}");
                        AppendLog($"Không tìm thấy file: {xmlPath}");
                        continue;
                    }

                    string content = File.ReadAllText(xmlPath);
                    string updated = ReplaceVersionInfoIron(content, data, out var logs);

                    if (updated != content)
                    {
                        File.WriteAllText(xmlPath, updated);
                        foreach (var lg in logs)
                        {
                            AppendLog(lg);
                            Debug.Log($"[Adapter Version Updated] {Path.GetFileName(xmlPath)} - {lg}");
                        }
                    }
                }
                
                AssetDatabase.Refresh();
            }
            
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Kết quả:", EditorStyles.boldLabel);

            // Header
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Network", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Adapter", EditorStyles.boldLabel, GUILayout.Width(70));
            GUILayout.Label("GMA AOS", EditorStyles.boldLabel, GUILayout.Width(85));
            GUILayout.Label("GMA iOS", EditorStyles.boldLabel, GUILayout.Width(85));
            GUILayout.Label("Iron AOS", EditorStyles.boldLabel, GUILayout.Width(85));
            GUILayout.Label("Iron iOS", EditorStyles.boldLabel, GUILayout.Width(85));
            GUILayout.Label("AppLovin AOS", EditorStyles.boldLabel, GUILayout.Width(100));
            GUILayout.Label("AppLovin iOS", EditorStyles.boldLabel, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            foreach (var kvp in gmaLookupResult)
            {
                var v = kvp.Value;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(kvp.Key, GUILayout.Width(80));
                GUILayout.Label(v.adapterVersion, GUILayout.Width(70));
                GUILayout.Label(v.android, GUILayout.Width(85));
                GUILayout.Label(v.ios, GUILayout.Width(85));
                GUILayout.Label(v.ironAndroid, GUILayout.Width(85));
                GUILayout.Label(v.ironIos, GUILayout.Width(85));
                GUILayout.Label(v.applovinAndroid, GUILayout.Width(100));
                GUILayout.Label(v.applovinIos, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
            }
        }


        GUILayout.Space(20);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.TextArea(log, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private string ReplaceVersionInfoAplovin(string content, AdapterWithSdk data, out List<string> logs)
    {
        logs = new List<string>();

        // 1. Tìm adapter key
        content = Regex.Replace(content, @"<!--[\s\S]*?-->", "");
        var androidMatch = Regex.Match(content, @"com\.applovin\.mediation:([a-zA-Z0-9]+)-adapter:([^""]+)");
        if (!androidMatch.Success) return content;

        string adapterKey = androidMatch.Groups[1].Value;        // ví dụ: fyber
        string oldAndroidVer = androidMatch.Groups[2].Value;     // ví dụ: 8.3.5.0
        string newAndroidVer = data.applovinAndroid;

        string adapterCapitalized = char.ToUpper(adapterKey[0]) + adapterKey.Substring(1);

        // 2. Cập nhật androidPackage
        if (oldAndroidVer != newAndroidVer && newAndroidVer != "?")
        {
            logs.Add($"{adapterKey} Android: {oldAndroidVer} → {newAndroidVer}");
            content = Regex.Replace(
                content,
                $@"<androidPackage spec=""com\.applovin\.mediation:{adapterKey}-adapter:[^""]+""\s*/>",
                $@"<androidPackage spec=""com.applovin.mediation:{adapterKey}-adapter:{newAndroidVer}"" />"
            );
        }
        else
        {
            logs.Add($"{adapterKey} Android: {oldAndroidVer} → {newAndroidVer}");
        }

        // 3. Cập nhật iosPod
        var iosMatch = Regex.Match(content, $@"<iosPod name=""AppLovinMediation{adapterCapitalized}Adapter"" version=""([^""]+)""\s*/>");
        if (iosMatch.Success)
        {
            string oldIosVer = iosMatch.Groups[1].Value;
            string newIosVer = data.applovinIos;

            if (oldIosVer != newIosVer && newIosVer != "?")
            {
                logs.Add($"{adapterKey} iOS: {oldIosVer} → {newIosVer}");
                content = Regex.Replace(
                    content,
                    $@"<iosPod name=""AppLovinMediation{adapterCapitalized}Adapter"" version=""[^""]+""\s*/>",
                    $@"<iosPod name=""AppLovinMediation{adapterCapitalized}Adapter"" version=""{newIosVer}"" />"
                );
            }
            else
            {
                logs.Add($"{adapterKey} iOS: {oldIosVer} → {newIosVer}");
            }
        }

        return content;
    }
    
   private string ReplaceVersionInfoIron(string content, AdapterWithSdk data, out List<string> logs)
{
    logs = new List<string>();

    // 1. Loại bỏ comment
    content = Regex.Replace(content, @"<!--[\s\S]*?-->", "");

    // 2. Tìm adapterKey từ androidPackage: com.ironsource.adapters:{adapterKey}adapter:version
    var androidMatch = Regex.Match(content, @"com\.ironsource\.adapters:([a-zA-Z0-9]+)adapter:([^""]+)");
    if (!androidMatch.Success) return content;

    string adapterKey = androidMatch.Groups[1].Value;       // ví dụ: facebook
    string oldAndroidVer = androidMatch.Groups[2].Value;
    string newAndroidVer = data.ironAndroid;

    // 3. Cập nhật android
    if (oldAndroidVer != newAndroidVer && newAndroidVer != "?")
    {
        logs.Add($"{adapterKey} Android: {oldAndroidVer} → {newAndroidVer}");
        content = Regex.Replace(
            content,
            $@"<androidPackage spec=""com\.ironsource\.adapters:{adapterKey}adapter:[^""]+""",
            $@"<androidPackage spec=""com.ironsource.adapters:{adapterKey}adapter:{newAndroidVer}"""
        );
    }
    else
    {
        logs.Add($"{adapterKey} Android: {oldAndroidVer} → {newAndroidVer} (giữ nguyên)");
    }

    // 4. Tìm iOSPod: IronSource{AdapterKey}Adapter
    string adapterPascal = char.ToUpper(adapterKey[0]) + adapterKey.Substring(1);
    var iosMatch = Regex.Match(content, $@"<iosPod name=""IronSource{adapterPascal}Adapter"" version=""([^""]+)""");
    if (iosMatch.Success)
    {
        string oldIosVer = iosMatch.Groups[1].Value;
        string newIosVer = data.ironIos;

        if (oldIosVer != newIosVer && newIosVer != "?")
        {
            logs.Add($"{adapterKey} iOS: {oldIosVer} → {newIosVer}");
            content = Regex.Replace(
                content,
                $@"<iosPod name=""IronSource{adapterPascal}Adapter"" version=""[^""]+""",
                $@"<iosPod name=""IronSource{adapterPascal}Adapter"" version=""{newIosVer}"""
            );
        }
        else
        {
            logs.Add($"{adapterKey} iOS: {oldIosVer} → {newIosVer} (giữ nguyên)");
        }
    }

    return content;
}

private void LookupGmaVersion(string gmaVersion)
{
    string mapPath = Path.Combine("Assets/GamePlugin/Editor/SettupBuild", "adapter_mapping.json");
    string fullPath = Path.Combine("Assets/GamePlugin/Editor/SettupBuild", "adapter_full_list.json");
    string ironPath = Path.Combine("Assets/GamePlugin/Editor/SettupBuild", "iron_adapter_versions.json");
    string applovinPath = Path.Combine("Assets/GamePlugin/Editor/SettupBuild", "adapter_versions.json");

    if (!File.Exists(mapPath) || !File.Exists(fullPath) || !File.Exists(ironPath) || !File.Exists(applovinPath))
    {
        Debug.LogError("❌ Missing one or more files: adapter_mapping.json / adapter_full_list.json / iron_adapter_versions.json / adapter_versions.json");
        return;
    }

    var mapping = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(mapPath));
    var fullList = JsonConvert.DeserializeObject<List<MediationAdapterInfo>>(File.ReadAllText(fullPath));
    
    var ironDict = JsonConvert.DeserializeObject<Dictionary<string, List<IronAdapterEntry>>>(File.ReadAllText(ironPath));
    var applovinRaw = JsonConvert.DeserializeObject<Dictionary<string, List<AppLovinAdapterEntry>>>(File.ReadAllText(applovinPath));

    gmaLookupResult = new Dictionary<string, AdapterWithSdk>();


    
    if (!mapping.TryGetValue(gmaVersion, out var networks))
    {
        log += $"⚠️ Không tìm thấy version {gmaVersion} trong mapping\n";
        return;
    }
    
    var requiredNetworks = new List<string> { "applovin", "chartboost", "inmobi", "ironsource", "maio", "meta", "mintegral", "mytarget", "pangle", "unity" };
    networks = GetVersionWithFallback(mapping, gmaVersion, requiredNetworks, out log);


    var maxAlias = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "unity", "unityads" },
        { "meta", "facebook" },
        { "adcolony", "adcolony" },
        { "amazonadmarketplace", "amazon" },
        { "aps", "aps" },
        { "bidmachine", "bidmachine" },
        { "bigo", "bigo" },
        { "chartboost", "chartboost" },
        { "fyber", "fyber" },
        { "inmobi", "inmobi" },
        { "ironsource", "ironsource" },
        { "maio", "maio" },
        { "mintegral", "mintegral" },
        { "moloco", "moloco" },
        { "mytarget", "mytarget" },
        { "ogury", "ogury" },
        { "pangle", "bytedance" },
        { "smaato", "smaato" },
        { "superawesome", "superawesome" },
        { "verve", "verve" },
        { "vungle", "vungle" },
        { "yandex", "yandex" },
        { "vk", "vk" }
    };


    var alias = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "applovin", "applovin-change-log" },
        { "meta", "meta-audience-network-change-log" },
        { "unity", "unityads-change-log" },
        { "chartboost", "chartboost-change-log" },
        { "inmobi", "inmobi-change-log" },
        { "pangle", "pangle-change-log" },
        { "vungle", "vungle-change-log" },
        { "mintegral", "mintegral-change-log" },
        { "maio", "maio-change-log" },
        { "smaato", "smaato-change-log" },
        { "fyber", "fyber-change-log" },
        { "bigo", "bigo-change-log" },
        { "yandex", "yandex-change-log" },
        { "ogury", "ogury-change-log" },
        { "moloco", "moloco-change-log" },
        { "superawesome", "superawesome-change-log" },
        { "admob", "admob-change-log" },
        { "aps", "aps-change-log" },
        { "hyprmx", "hyprmx-change-log" },
        { "verve", "verve-change-log" },
        { "mobilefuse", "mobilefuse-change-log" },
        { "mytarget", "vk-ad-network-change-log" }
    };
    foreach (var net in networks)
    {
        string network = net.Key.ToLowerInvariant();
        string adapterVer = net.Value;

        var match = fullList.FirstOrDefault(x => x.networkName == network && x.unityAdapterVersion == adapterVer);

        string android = match?.androidAdapterVersion ?? "?";
        string ios = match?.iosAdapterVersion ?? "?";

        // iron mapping
        var ironKey = alias.TryGetValue(network, out var ironAlias) ? ironAlias : network;
        string ironVersion = "?", ironAndroid = "?", ironIos = "?";
        if (ironDict.TryGetValue(ironKey, out var ironList) && ironList != null)
        {
            var found = ironList.FirstOrDefault(i => LooseMatch(i.android, android) || LooseMatch(i.ios, ios));
            var foundiOS = ironList.FirstOrDefault(i => LooseMatch(i.ios, ios));
            if (found != null)
            {
                ironVersion = found.version;
                ironAndroid = found.version;
            }

            if (foundiOS != null)
            {
                ironIos = foundiOS.version;
            }
        }

        // AppLovin mapping
        string applovinVersion = "?", applovinAndroid = "?", applovinIos = "?";
        string applovinKey = maxAlias.TryGetValue(network, out var appAlias) ? appAlias : network;
        if (applovinRaw.TryGetValue(applovinKey, out var applovinList) && applovinList != null)
        {
            var found = applovinList.FirstOrDefault(i => LooseMatch(i.android, android));
            var foundiOS = applovinList.FirstOrDefault(i => LooseMatch(i.ios, ios));
            if (found != null)
            {
                applovinVersion = found.adapterVersion;
                applovinAndroid = found.android;
            }
            
            if (foundiOS != null)
            {
                applovinIos = foundiOS.ios;
            }
        }

        gmaLookupResult[network] = new AdapterWithSdk
        {
            adapterVersion = adapterVer,
            android = android,
            ios = ios,
            ironVersion = ironVersion,
            ironAndroid = ironAndroid,
            ironIos = ironIos,
            applovinVersion = applovinVersion,
            applovinAndroid = applovinAndroid,
            applovinIos = applovinIos
        };
    }

    log += $"✅ Found {gmaLookupResult.Count} adapters for GMA {gmaVersion}\n";
}
Dictionary<string, string> GetVersionWithFallback(Dictionary<string, Dictionary<string, string>> mapping, string gmaVersion, List<string> requiredNetworks, out string log)
{
    log = "";
    if (!mapping.TryGetValue(gmaVersion, out var current))
    {
        log += $"⚠️ Không tìm thấy version {gmaVersion} trong mapping\n";
        return null;
    }

    // Kết quả cuối cùng
    var result = new Dictionary<string, string>(current);

    foreach (var adnet in requiredNetworks)
    {
        if (result.ContainsKey(adnet))
            continue; // Đã có → bỏ qua

        // Tìm version gần nhất khác có adnet này
        var nearest = mapping
            .Where(kv => kv.Value.ContainsKey(adnet))
            .OrderBy(kv => VersionDistance(ParseVer(kv.Key), ParseVer(gmaVersion)))
            .FirstOrDefault();

        if (!string.IsNullOrEmpty(nearest.Key))
        {
            result[adnet] = nearest.Value[adnet];
            log += $"➕ Dùng {adnet} từ {nearest.Key}: {nearest.Value[adnet]}\n";
        }
        else
        {
            log += $"❌ Không tìm thấy {adnet} trong bất kỳ version nào\n";
        }
    }

    return result;
}
Version ParseVer(string v)
{
    var parts = v.Split('.').Select(s => int.TryParse(s, out var n) ? n : 0).ToList();
    while (parts.Count < 3) parts.Add(0);
    return new Version(parts[0], parts[1], parts[2]);
}

int VersionDistance(Version a, Version b)
{
    return Math.Abs(a.Major - b.Major) * 10000 +
           Math.Abs(a.Minor - b.Minor) * 100 +
           Math.Abs(a.Build - b.Build);
}

private static bool LooseMatch(string a, string b)
{
    if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;

    var va = ParseMajorMinor(a);
    var vb = ParseMajorMinor(b);

    return va.Major == vb.Major && va.Minor == vb.Minor;
}

private static Version ParseMajorMinor(string version)
{
    var nums = System.Text.RegularExpressions.Regex.Matches(version, @"\d+")
        .Cast<System.Text.RegularExpressions.Match>()
        .Select(m => int.Parse(m.Value)).ToList();

    int major = nums.Count > 0 ? nums[0] : 0;
    int minor = nums.Count > 1 ? nums[1] : 0;

    return new Version(major, minor);
}



[Serializable]
public class AppLovinAdapterEntry
{
    public string adapterVersion;
    public string android;
    public string ios;
}

[Serializable]
public class AdapterWithSdk
{
    public string adapterVersion;
    public string android;
    public string ios;
    public string ironVersion;
    public string ironAndroid;
    public string ironIos;

    public string applovinVersion;
    public string applovinAndroid;
    public string applovinIos;
}

[Serializable]
public class IronAdapterEntry
{
    public string version;
    public string android;
    public string ios;
}
    public static void AppendLog(string message)
    {
        log += message + "\n";
    }
}
#endif

public static class ApplovinAdapterCrawler
{
    private const string GitHubToken = "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"; // Optional
    private const string BaseApiAndroid = "https://api.github.com/repos/AppLovin/AppLovin-MAX-SDK-Android/contents/";
    private const string BaseApiiOS = "https://api.github.com/repos/AppLovin/AppLovin-MAX-SDK-iOS/contents/";

    public static IEnumerator CrawlAll(Action<string> logCallback)
    {
        logCallback?.Invoke("🔎 Crawling AppLovin adapters...");
        var androidData = new List<AdapterVersionData>();
        var iosData = new List<AdapterVersionData>();

        yield return CrawlPlatform("Android", BaseApiAndroid, androidData, logCallback);
        yield return CrawlPlatform("iOS", BaseApiiOS, iosData, logCallback);

        var merged = new Dictionary<(string, string), MergedAdapterVersion>();

        foreach (var data in androidData)
        {
            var key = (data.adapter, data.adapterVersion);
            if (!merged.TryGetValue(key, out var entry))
                entry = merged[key] = new MergedAdapterVersion { adapter = data.adapter, adapterVersion = data.adapterVersion };
            entry.android = data.sdkVersion;
        }

        foreach (var data in iosData)
        {
            var key = (data.adapter, data.adapterVersion);
            if (!merged.TryGetValue(key, out var entry))
                entry = merged[key] = new MergedAdapterVersion { adapter = data.adapter, adapterVersion = data.adapterVersion };
            entry.ios = data.sdkVersion;
        }

        var grouped = merged.Values
            .GroupBy(e => e.adapter)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(e =>
                {
                    Version.TryParse(e.adapterVersion, out var v);
                    return v;
                }).ToList()
            );

        var json = JsonConvert.SerializeObject(grouped, Formatting.Indented);
        var path = Path.Combine("Assets/GamePlugin/Editor/SettupBuild", "adapter_versions.json");
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();

        logCallback?.Invoke($"✅ Exported to {path}");
    }

    private static IEnumerator CrawlPlatform(string label, string baseApi, List<AdapterVersionData> result, Action<string> logCallback)
    {
        var client = CreateClient();
        Task<List<string>> task = GetAdapterList(client, baseApi);
        yield return new EditorWaitForTask(task);
        var adapters = task.Result;

        foreach (var adapter in adapters)
        {
            logCallback?.Invoke($"[{label}] Adapter: {adapter}");
            string changelog = "";
            Task<string> changelogTask = GetChangelogContent(client, baseApi, adapter);
            yield return new EditorWaitForTask(changelogTask);

            try
            {
                changelog = changelogTask.Result;
                var versions = ParseVersions(changelog);
                foreach (var (adapterVer, sdkVer) in versions)
                {
                    result.Add(new AdapterVersionData
                    {
                        adapter = adapter.ToLower(),
                        platform = label.ToLower(),
                        adapterVersion = adapterVer,
                        sdkVersion = sdkVer
                    });
                    logCallback?.Invoke($"   {adapterVer} → SDK {sdkVer}");
                }
            }
            catch (Exception e)
            {
                logCallback?.Invoke($"   ❌ Failed to parse {adapter}: {e.Message}");
            }
        }
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("UnityCrawlerBot");

        if (!string.IsNullOrEmpty(GitHubToken))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("token", GitHubToken);
        }

        return client;
    }

    private static async Task<List<string>> GetAdapterList(HttpClient client, string apiUrl)
    {
        var json = await client.GetStringAsync(apiUrl);
        var array = JsonConvert.DeserializeObject<List<GitHubContent>>(json);
        return array
            .Where(f => f.type == "dir" && f.name != "src" && !f.name.Contains("Demo"))
            .Select(f => f.name)
            .ToList();
    }

    private static async Task<string> GetChangelogContent(HttpClient client, string baseApi, string adapter)
    {
        string api = $"{baseApi}{adapter}/CHANGELOG.md";
        var rawJson = await client.GetStringAsync(api);
        var content = JsonConvert.DeserializeObject<GitHubContent>(rawJson);
        return await client.GetStringAsync(content.download_url);
    }

    private static List<(string adapterVersion, string sdkVersion)> ParseVersions(string changelog)
    {
        var lines = changelog.Split('\n');
        var result = new List<(string, string)>();
        string currentVersion = "";

        foreach (var line in lines)
        {
            if (Regex.IsMatch(line.Trim().TrimStart('#').Trim(), @"^\d+(\.\d+){2,4}"))
            {
                currentVersion = line.Trim().TrimStart('#').Trim();
                result.Add((currentVersion, currentVersion)); // Use version as SDK version
            }
        }

        return result;
    }

    [Serializable]
    public class GitHubContent
    {
        public string name;
        public string type;
        public string download_url;
    }

    [Serializable]
    public class AdapterVersionData
    {
        public string adapter;
        public string platform;
        public string adapterVersion;
        public string sdkVersion;
    }

    [Serializable]
    public class MergedAdapterVersion
    {
        public string adapter;
        public string adapterVersion;
        public string android;
        public string ios;
    }
}
#endif

#if UNITY_EDITOR
public static class IronSourceAdapterCrawler
{
    private static string rootUrl = "https://developers.is.com/ironsource-mobile/unity/mediation-networks-unity/";

    public static IEnumerator DoCrawl(Action<string> logCallback = null)
    {
        var result = new Dictionary<string, List<AdapterVersionInfo>>();

        using (UnityWebRequest www = UnityWebRequest.Get(rootUrl))
        {
            SetupRequestHeaders(www);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                logCallback?.Invoke("❌ Failed to load root page: " + www.error);
                yield break;
            }

            string html = www.downloadHandler.text;
            var links = ParseNetworkLinks(html);

            foreach (var kvp in links)
            {
                string network = kvp.Key;
                string detailUrl = kvp.Value;

                using (UnityWebRequest detailReq = UnityWebRequest.Get(detailUrl))
                {
                    SetupRequestHeaders(detailReq);
                    yield return detailReq.SendWebRequest();

                    if (detailReq.result != UnityWebRequest.Result.Success)
                    {
                        logCallback?.Invoke($"⚠️ Failed to load {network}: {detailReq.error}");
                        continue;
                    }

                    var versionList = ParseAdapterVersions(detailReq.downloadHandler.text);
                    result[network] = versionList;
                    logCallback?.Invoke($"✅ [{network}] Found {versionList.Count} versions");
                }
            }
        }

        string json = JsonConvert.SerializeObject(result, Formatting.Indented);
        string path = Path.Combine("Assets/GamePlugin/Editor/SettupBuild", "iron_adapter_versions.json");
        File.WriteAllText(path, json);
        logCallback?.Invoke("📦 Exported to: " + path);
        AssetDatabase.Refresh();
    }

    private static void SetupRequestHeaders(UnityWebRequest req)
    {
        req.SetRequestHeader("User-Agent", "Mozilla/5.0 (Unity)");
        req.SetRequestHeader("Accept", "text/html");
        req.SetRequestHeader("Accept-Language", "en-US,en;q=0.9");
        req.SetRequestHeader("Referer", "https://developers.is.com/");
    }

    public static Dictionary<string, string> ParseNetworkLinks(string html)
    {
        var dict = new Dictionary<string, string>();
        var regex = new Regex("<a href=\\\"(.*?)\\\">(.*?)<\\/a>", RegexOptions.IgnoreCase);

        foreach (Match match in regex.Matches(html))
        {
            string url = match.Groups[1].Value.Trim();
            string label = match.Groups[2].Value.Trim();

            if (!url.Contains("-change-log")) continue;

            string key = url
                .Replace("https://developers.is.com/ironsource-mobile/unity/", "")
                .Replace("-change-log", "")
                .Replace("/", "")
                .ToLowerInvariant();

            dict[key] = url;
        }

        return dict;
    }

    private static List<AdapterVersionInfo> ParseAdapterVersions(string html)
    {
        var result = new List<AdapterVersionInfo>();
        var rowRegex = new Regex(@"<tr>\s*<td.*?>(.*?)<\/td>\s*<td.*?>(.*?)<\/td>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        foreach (Match match in rowRegex.Matches(html))
        {
            string adapterVersion = StripHtml(match.Groups[1].Value).Trim();
            string sdkVersions = StripHtml(match.Groups[2].Value).Trim();

            string[] parts = sdkVersions.Split('/');
            string ios = parts.Length > 0 ? parts[0].Trim() : "";
            string android = parts.Length > 1 ? parts[1].Trim() : ios;

            result.Add(new AdapterVersionInfo
            {
                version = adapterVersion,
                ios = ios,
                android = android
            });
        }

        return result;
    }

    private static string StripHtml(string input)
    {
        return DecodeHtmlEntities(Regex.Replace(input, "<.*?>", string.Empty));
    }

    private static string DecodeHtmlEntities(string input)
    {
        return input.Replace("&#8211;", "–")
                    .Replace("&amp;", "&")
                    .Replace("&lt;", "<")
                    .Replace("&gt;", ">")
                    .Replace("&quot;", "\"")
                    .Replace("&nbsp;", " ")
                    .Replace("&#39;", "'");
    }

    [Serializable]
    public class AdapterVersionInfo
    {
        public string version;
        public string android;
        public string ios;
    }
}
#endif

#if UNITY_EDITOR

public static class AdmobMediationScraper
{
    private const string BaseUrl = "https://developers.google.com/admob/unity/mediation";

    public static IEnumerator DoCrawl(Action<string> logCallback = null)
    {
        logCallback?.Invoke("🔍 Crawling AdMob mediation networks...");
        var urls = new Dictionary<string, string>();

        yield return GetAllNetworkUrls(urls, logCallback);

        var adapterList = new List<MediationAdapterInfo>();

        foreach (var kvp in urls)
        {
            string network = kvp.Key;
            string url = kvp.Value;

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    logCallback?.Invoke($"❌ Failed to load {network}: {www.error}");
                    continue;
                }

                string html = www.downloadHandler.text;
                var adapters = ParseAdapterChangelog(network, html);

                if (adapters != null && adapters.Count > 0)
                {
                    adapterList.AddRange(adapters);
                    logCallback?.Invoke($"✅ {network}: found {adapters.Count} version(s)");
                }
                else
                {
                    logCallback?.Invoke($"⚠️ {network}: no adapter version found.");
                }
            }
        }

        var mapping = new Dictionary<string, Dictionary<string, string>>();

        foreach (var adapter in adapterList)
        {
            if (string.IsNullOrEmpty(adapter.gmaSdkVersion) || string.IsNullOrEmpty(adapter.unityAdapterVersion))
                continue;

            if (!mapping.ContainsKey(adapter.gmaSdkVersion))
                mapping[adapter.gmaSdkVersion] = new Dictionary<string, string>();

            mapping[adapter.gmaSdkVersion][adapter.networkName] = adapter.unityAdapterVersion;
        }

        string jsonList = JsonConvert.SerializeObject(adapterList, Formatting.Indented);
        string jsonMap = JsonConvert.SerializeObject(mapping, Formatting.Indented);

        File.WriteAllText(Path.Combine("Assets/GamePlugin/Editor/SettupBuild", "adapter_full_list.json"), jsonList);
        File.WriteAllText(Path.Combine("Assets/GamePlugin/Editor/SettupBuild", "adapter_mapping.json"), jsonMap);
        logCallback?.Invoke("📦 Exported: adapter_full_list.json & adapter_mapping.json");
        AssetDatabase.Refresh();
    }

    private static IEnumerator GetAllNetworkUrls(Dictionary<string, string> resultUrls, Action<string> logCallback = null)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(BaseUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                logCallback?.Invoke("❌ Failed to fetch overview page: " + www.error);
                yield break;
            }

            string html = www.downloadHandler.text;
            var itemRegex = new Regex("<li class=\"devsite-nav-item\">(.*?)</li>", RegexOptions.Singleline);
            var matches = itemRegex.Matches(html);

            foreach (Match match in matches)
            {
                string block = match.Groups[1].Value;
                if (block.Contains("data-icon=\"deprecated\"")) continue;

                var detailMatch = Regex.Match(block, "track-metadata-eventdetail=\"(/admob/unity/mediation/[a-z0-9\\-_]+)\"", RegexOptions.IgnoreCase);
                if (!detailMatch.Success) continue;

                string relativeUrl = detailMatch.Groups[1].Value;
                string name = relativeUrl.Split('/')[^1].ToLowerInvariant();
                string fullUrl = "https://developers.google.com" + relativeUrl;

                if (!resultUrls.ContainsKey(name))
                    resultUrls.Add(name, fullUrl);
            }

            logCallback?.Invoke($"✅ Found {resultUrls.Count} active mediation networks.");
        }
    }

    public static List<MediationAdapterInfo> ParseAdapterChangelog(string networkName, string html)
    {
        var results = new List<MediationAdapterInfo>();

        var blockRegex = new Regex(
            @$"<h4[^>]*?><a\s+href=""(?<url>https:\/\/[^""]*?{networkName}.*?-(?<unityVer>[\d.]+)\.zip)"">.*?</a>\s*<\/h4>\s*<ul>(?<ul>.*?)<\/ul>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        var androidRegex = new Regex(@"Android adapter version (\d+\.\d+\.\d+\.\d+)", RegexOptions.IgnoreCase);
        var iosRegex = new Regex(@"iOS adapter version (\d+\.\d+\.\d+\.\d+)", RegexOptions.IgnoreCase);
        var gmaRegex = new Regex(@"Google Mobile Ads Unity Plugin version (\d+\.\d+\.\d+)", RegexOptions.IgnoreCase);

        foreach (Match match in blockRegex.Matches(html))
        {
            var info = new MediationAdapterInfo
            {
                networkName = networkName.ToLowerInvariant(),
                unityAdapterVersion = match.Groups["unityVer"].Value,
                downloadUrl = match.Groups["url"].Value
            };

            string ulBlock = match.Groups["ul"].Value;

            var androidMatch = androidRegex.Match(ulBlock);
            if (androidMatch.Success)
                info.androidAdapterVersion = androidMatch.Groups[1].Value;

            var iosMatch = iosRegex.Match(ulBlock);
            if (iosMatch.Success)
                info.iosAdapterVersion = iosMatch.Groups[1].Value;

            var gmaMatch = gmaRegex.Match(ulBlock);
            if (gmaMatch.Success)
                info.gmaSdkVersion = gmaMatch.Groups[1].Value;

            results.Add(info);
        }

        return results;
    }

    [Serializable]
    public class MediationAdapterInfo
    {
        public string networkName;
        public string unityAdapterVersion;
        public string androidAdapterVersion;
        public string iosAdapterVersion;
        public string gmaSdkVersion;
        public string downloadUrl;
    }
}
#endif


public class MediationAdapterInfo
{
    public string networkName;
    public string unityAdapterVersion;
    public string androidAdapterVersion;
    public string iosAdapterVersion;
    public string gmaSdkVersion;
    public string downloadUrl;
}

[Serializable]
public class AdapterWithSdk
{
    public string adapterVersion;
    public string android;
    public string ios;
    public string ironVersion;
    public string ironAndroid;
    public string ironIos;

    public string applovinVersion;
    public string applovinAndroid;
    public string applovinIos;
}



#if UNITY_EDITOR
public class EditorWaitForTask<T> : CustomYieldInstruction
{
    private Task<T> task;

    public T Result => task.Result;
    public override bool keepWaiting => !task.IsCompleted;

    public EditorWaitForTask(Task<T> task)
    {
        this.task = task;
    }
}

public class EditorWaitForTask : CustomYieldInstruction
{
    private Task task;

    public override bool keepWaiting => !task.IsCompleted;

    public EditorWaitForTask(Task task)
    {
        this.task = task;
    }
}
#endif