#define MAX_USE_CONSTRAINTS

using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor.Callbacks;
using System.Threading;
#if UNITY_IOS || UNITY_IPHONE
using UnityEditor.iOS.Xcode;
#endif

#if UNITY_IOS || UNITY_IPHONE
public class BuildPostProcessor
{

    [PostProcessBuild]
    public static void ChangeXcodePlist(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {

            string plistPath = path + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);


            PlistElementDict rootDict = plist.root;

            // example of changing a value:
            // rootDict.SetString("CFBundleVersion", "6.6.6");

            // example of adding a boolean key...
            // < key > ITSAppUsesNonExemptEncryption </ key > < false />
            rootDict.SetString("GADApplicationIdentifier", "ca-app-pub-2777953690987264~6269905542");
            rootDict.SetString("NSUserTrackingUsageDescription", "This identifier will be used to deliver personalized ads to you.");

            PlistElementArray adNetworksItems = rootDict["SKAdNetworkItems"] == null
                ? rootDict.CreateArray("SKAdNetworkItems")
                : rootDict["SKAdNetworkItems"].AsArray();
            List<string> existingadNetworksItems = new List<string>();
            foreach (PlistElement element in adNetworksItems.values)
            {
                existingadNetworksItems.Add(element.AsDict().values["SKAdNetworkIdentifier"].AsString());
            }
            bool isAddBigo = false;
#if ENABLE_ADS_IRON || ENABLE_ADS_MAX
            if (isAddBigo)
            {
                List<string> dicnet = getDicnetSkadnet(Application.dataPath + "/GamePlugin/Ads/Editor/BigoAds/BigoSKAdNetworkItems.txt");
                foreach (string itemnet in dicnet)
                {
                    if (!existingadNetworksItems.Contains(itemnet))
                    {
                        PlistElementDict adNetworksItem = adNetworksItems.AddDict();
                        adNetworksItem.SetString("SKAdNetworkIdentifier", itemnet);
                        existingadNetworksItems.Add(itemnet);
                    }
                }
            }
#endif
#if ENABLE_ADS_MYTARGET
            List<string> dicnettarget = new List<string>() { "r26jy69rpl.skadnetwork" };
            foreach (string itemnet in dicnettarget)
            {
                if (!existingadNetworksItems.Contains(itemnet))
                {
                    PlistElementDict adNetworksItem = adNetworksItems.AddDict();
                    adNetworksItem.SetString("SKAdNetworkIdentifier", itemnet);
                    existingadNetworksItems.Add(itemnet);
                }
            }
#endif
#if ENABLE_ADS_TOPON && !USE_TOPONMY
            List<string> dicnet = getDicnetSkadnet(Application.dataPath + "/GamePlugin/Ads/Topon/Editor/SKAdNetworkItems.txt");
            foreach (string itemnet in dicnet)
            {
                if (!existingadNetworksItems.Contains(itemnet))
                {
                    PlistElementDict adNetworksItem = adNetworksItems.AddDict();
                    adNetworksItem.SetString("SKAdNetworkIdentifier", itemnet);
                    existingadNetworksItems.Add(itemnet);
                }
            }
            PlistElementArray arrSchecms = rootDict["LSApplicationQueriesSchemes"] == null
                ? rootDict.CreateArray("LSApplicationQueriesSchemes")
                : rootDict["LSApplicationQueriesSchemes"].AsArray();
            List<string> existingSchemes = new List<string>();
            foreach (PlistElement element in arrSchecms.values)
            {
                existingSchemes.Add(element.AsString());
            }
            List<string> dicSchemes = getAppQueriesSchemesTopon();
            foreach (string itemnet in dicSchemes)
            {
                if (!existingSchemes.Contains(itemnet))
                {
                    arrSchecms.AddString(itemnet);
                    existingSchemes.Add(itemnet);
                }
            }
#endif

            string[] arrNetAdd = {
                "v9wttpbfk9.skadnetwork",
                "n38lu8286q.skadnetwork"
            };
            foreach (string itemnet in arrNetAdd)
            {
                if (!existingadNetworksItems.Contains(itemnet))
                {
                    PlistElementDict adNetworksItem = adNetworksItems.AddDict();
                    adNetworksItem.SetString("SKAdNetworkIdentifier", itemnet);
                    existingadNetworksItems.Add(itemnet);
                }
            }
            string dirLangs = "";
            string dirFind = path;
            for (int i = 0; i < 5; i++)
            {
                DirectoryInfo d = new DirectoryInfo(dirFind);
                DirectoryInfo pd = d.Parent;
                if (pd != null)
                {
                    DirectoryInfo[] listd = pd.GetDirectories();
                    foreach (var id in listd)
                    {
                        if (id.Name.CompareTo("Assets") == 0)
                        {
                            dirLangs = id.FullName;
                            break;
                        }
                    }
                }
                if (dirLangs.Length > 3)
                {
                    break;
                }
                else
                {
                    dirFind = pd.FullName;
                }
            }
            string pathLangs = dirLangs + "/GamePlugin/Resources/Langs";
            string[] arrname = Directory.GetFiles(pathLangs);
            List<string> listLangs = new List<string>();
            foreach (var item in arrname)
            {
                if (!item.Contains(".meta"))
                {
                    string langcode = Path.GetFileNameWithoutExtension(item);
                    if (langcode.Contains("default"))
                    {
                        listLangs.Add("en");
                    }
                    else
                    {
                        listLangs.Add(langcode);
                    }
                }
            }

            if (listLangs.Count > 0) 
            {
                // Remove old if exists (tránh duplicate)
                if (rootDict.values.ContainsKey("CFBundleLocalizations"))
                {
                    rootDict.values.Remove("CFBundleLocalizations");
                }

                // Create localization array
                PlistElementArray locales = rootDict.CreateArray("CFBundleLocalizations");
                foreach (var item in listLangs)
                {
                    locales.AddString(item);   // English
                }
            }

            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }

    [PostProcessBuildAttribute(1)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            // We need to construct our own PBX project path that corrently refers to the Bridging header
            // var projPath = PBXProject.GetPBXProjectPath(buildPath);
            var projPath = buildPath + "/Unity-iPhone.xcodeproj/project.pbxproj";
            var proj = new PBXProject();
            proj.ReadFromFile(projPath);

            //var targetGuid = proj.GetUnityFrameworkTargetGuid();
            var targetGuid = proj.GetUnityMainTargetGuid();
            var gameAssemblyTargetGuid = proj.TargetGuidByName("GameAssembly");


            //// Configure build settings
            proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
            //proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_BRIDGING_HEADER", "Libraries/Plugins/iOS/MyUnityPlugin/Source/MyUnityPlugin-Bridging-Header.h");
            //proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_INTERFACE_HEADER_NAME", "MyUnityPlugin-Swift.h");
            //proj.AddBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");

            var frameworkGuid = proj.GetUnityFrameworkTargetGuid();
            proj.SetBuildProperty(frameworkGuid, "ENABLE_BITCODE", "NO");
            proj.SetBuildProperty(frameworkGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");

            proj.WriteToFile(projPath);

            addNativeViewXib(projPath, buildPath);
            RemoveDuplicateRunScriptGUIDsFromProjectFile(projPath, gameAssemblyTargetGuid);
            try
            {
                if (AppConfig.AdjustBanner > 0)
                {
#if ENABLE_ADS_MAX || ENABLE_ADS_IRON
                    fixBannerMax(buildPath, true);
                    fixBannetIron(buildPath);
#endif
                    fixBannerAdmob(buildPath);
                    MyTargetPostProcess.AdjustBanner(buildPath);
                    YandexPostProcess.AdjustBanner(buildPath);

                    fixRenderMetal(buildPath);
                }
            }
            catch (Exception ex) { }
        }
    }

    static void addNativeViewXib(string projPath, string buildPath)
    {
        // Danh sách đầy đủ file bạn cần thêm và đường dẫn tương đối trong Xcode project
        List<(string Key, string Value)> resources = new List<(string, string)>
        {
            ("GGNativeAdCl.xib", "Libraries/GamePlugin/Ads/AdmobPlugin/Platforms/iOS/GGNativeAdCl.xib"),
            ("GGNativeAdBn.xib", "Libraries/GamePlugin/Ads/AdmobPlugin/Platforms/iOS/GGNativeAdBn.xib"),
            ("GGNativeAdRectNt.xib", "Libraries/GamePlugin/Ads/AdmobPlugin/Platforms/iOS/GGNativeAdRectNt.xib"),
            ("GGNativeAdView.xib", "Libraries/GamePlugin/Ads/AdmobPlugin/Platforms/iOS/GGNativeAdView.xib"),
            ("GGNativeAdView1.xib", "Libraries/GamePlugin/Ads/AdmobPlugin/Platforms/iOS/GGNativeAdView1.xib"),
            ("GGNativeAdView12.xib", "Libraries/GamePlugin/Ads/AdmobPlugin/Platforms/iOS/GGNativeAdView12.xib"),
            ("MaxNativeAdView.xib", "Libraries/GamePlugin/Ads/MaxPlugin/Platforms/iOS/MaxNativeAdView.xib"),
            ("FbNativeAdView.xib", "Libraries/GamePlugin/Ads/FbPlugin/Platforms/iOS/FbNativeAdView.xib"),
            ("button_close.png", "Libraries/GamePlugin/Plugins/iOS/button_close.png"),
            ("icon_down.png", "Libraries/GamePlugin/Plugins/iOS/icon_down.png"),
            ("button_close.png", "Libraries/button_close.png"),
            ("icon_down.png", "Libraries/icon_down.png"),
            ("IconAdsiOS.png", "Libraries/IconAdsiOS.png"),
            ("black.png", "Libraries/black.png")
        };

        var proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string projText = File.ReadAllText(projPath); // đọc để kiểm tra thủ công các file đã nằm trong Resources
        string targetGuid = proj.GetUnityMainTargetGuid();
        string frameworkGuid = proj.GetUnityFrameworkTargetGuid();
        string resourcesBuildPhaseGuid = proj.GetResourcesBuildPhaseByTarget(targetGuid);

        // Đảm bảo tắt Bitcode và Swift Embed
        proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
        proj.SetBuildProperty(frameworkGuid, "ENABLE_BITCODE", "NO");
        proj.SetBuildProperty(frameworkGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");

        foreach (var kvp in resources)
        {
            string fileName = kvp.Key;
            string localPath = kvp.Value.Replace("\\", "/");
            string fullPath = Path.Combine(buildPath, localPath);

            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"❌ File not found: {fullPath}");
                continue;
            }

            bool alreadyInProject = proj.ContainsFileByProjectPath(localPath);

            string fileGuid = alreadyInProject
                ? proj.FindFileGuidByProjectPath(localPath)
                : proj.AddFile(localPath, localPath, PBXSourceTree.Source);

            if (!IsFileInMainResourcesPhase(projText, resourcesBuildPhaseGuid, fileName))
            {
                proj.AddFileToBuildSection(targetGuid, resourcesBuildPhaseGuid, fileGuid);
                Debug.Log($"✅ Added to Copy Bundle Resources: {fileName}");
            }
            else
            {
                Debug.Log($"ℹ️ File already in Copy Bundle Resources: {fileName}");
            }
        }

        proj.WriteToFile(projPath);
        Debug.Log("✅ Completed updating Xcode project with resource files.");
    }

    static void RemoveDuplicateRunScriptGUIDsFromProjectFile(string projPath, string targetGuid)
    {
        string[] lines = File.ReadAllLines(projPath);
        List<string> output = new List<string>();

        bool insideTarget = false;
        bool insideBuildPhases = false;
        HashSet<string> scriptGuids = new HashSet<string>();
        HashSet<string> duplicatesToRemove = new HashSet<string>();

        foreach (string line in lines)
        {
            string trimmed = line.Trim();

            if (trimmed.StartsWith($"{targetGuid} ") && trimmed.Contains("/* GameAssembly */"))
            {
                insideTarget = true;
            }

            if (insideTarget && trimmed.StartsWith("buildPhases = ("))
            {
                insideBuildPhases = true;
                output.Add(line);
                continue;
            }

            if (insideBuildPhases)
            {
                if (trimmed == ");")
                {
                    insideBuildPhases = false;
                    insideTarget = false;
                    output.Add(line);
                    continue;
                }

                if (trimmed.Contains("/* Run Script */"))
                {
                    string guid = trimmed.Split(' ')[0].Trim();
                    if (!scriptGuids.Add(guid))
                    {
                        duplicatesToRemove.Add(guid);
                        Debug.Log($"🗑 Found duplicate Run Script GUID: {guid}");
                        continue; // skip adding this duplicate line
                    }
                }
                
                if (trimmed.Contains("/* ShellScript */"))
                {
                    string guid = trimmed.Split(' ')[0].Trim();
                    if (!scriptGuids.Add(guid))
                    {
                        duplicatesToRemove.Add(guid);
                        Debug.Log($"🗑 Found duplicate Run Script GUID: {guid}");
                        continue; // skip adding this duplicate line
                    }
                }
            }

            output.Add(line);
        }

        File.WriteAllLines(projPath, output.ToArray());
        Debug.Log($"✅ Cleaned project.pbxproj. Removed {duplicatesToRemove.Count} duplicate Run Script entries.");
    }

    static bool IsFileInMainResourcesPhase(string pbxText, string resourcesPhaseGuid, string fileName)
    {
        string pattern = $@"{Regex.Escape(resourcesPhaseGuid)}.*?files\s*=\s*\(([\s\S]*?)\);";
        Regex regex = new Regex(pattern, RegexOptions.Singleline);
        Match match = regex.Match(pbxText);

        if (!match.Success)
        {
            Debug.LogWarning($"[XcodePostProcess] Resources Build Phase GUID not found: {resourcesPhaseGuid}");
            return false;
        }

        string content = match.Groups[1].Value;
        string filePattern = $@"\/\*\s*{Regex.Escape(fileName)}\s*in\s*Resources\s*\*\/";
        bool found = Regex.IsMatch(content, filePattern);

        Debug.Log($"[XcodePostProcess] Checking file '{fileName}' in Resources phase: {(found ? "FOUND" : "NOT FOUND")}");
        return found;
    }

    static void fixRenderMetal(string path)
    {
        string pfilem = path + "/Classes/UnityAppController.h";
        if (File.Exists(pfilem))
        {
            bool isw = false;
            string[] linesRead = File.ReadAllLines(pfilem);
            for (int i = 0; i < linesRead.Length; i++)
            {
                if (linesRead[i].Contains("#define UNITY_USES_METAL_DISPLAY_LINK (UNITY_HAS_IOSSDK_17_0 || UNITY_HAS_TVOSSDK_17_0)"))
                {
                    linesRead[i] = "#define UNITY_USES_METAL_DISPLAY_LINK 0";
                    isw = true;
                    break;
                }
            }
            if (isw)
            {
                System.IO.File.WriteAllLines(pfilem, linesRead);
            }
        }
    }
    public static void fixBannerAdmob(string path)
    {
        string pathsrc = Application.dataPath + "/GamePlugin/Ads/AdmobPlugin/Platforms/iOS/AdmobBanner.m";
        string pfilem = path + "/Libraries/GamePlugin/Ads/AdmobPlugin/Platforms/iOS/AdmobBanner.m";
        SettingBuildAndroid.coppyFileNormal(pathsrc, pfilem);
        if (File.Exists(pfilem))
        {
            string[] linesRead = File.ReadAllLines(pfilem);
            bool isw = false;
            for (int i = 0; i < linesRead.Length; i++)
            {
                if (linesRead[i].Contains("unityView.safeAreaInsets.top /") || linesRead[i].Contains("unityView.safeAreaInsets.top/"))
                {
                    isw = true;
                    linesRead[i] = $"            safetop = unityView.safeAreaInsets.top / {AppConfig.AdjustBanner};";
                }
                else if (linesRead[i].Contains("unityView.safeAreaInsets.bottom /") || linesRead[i].Contains("unityView.safeAreaInsets.bottom/"))
                {
                    isw = true;
                    linesRead[i] = $"            safebot = unityView.safeAreaInsets.bottom - unityView.safeAreaInsets.bottom / {AppConfig.AdjustBanner};";
                }
            }
            if (isw)
            {
                System.IO.File.WriteAllLines(pfilem, linesRead);
            }
        }
    }

    static void fixBannetIron(string path)
    {
        string pfilem = path + "/Libraries/LevelPlay/Runtime/Plugins/iOS/iOSBridge.m";
        if (File.Exists(pfilem))
        {
            string[] linesRead = File.ReadAllLines(pfilem);
            List<string> linesWrite = new List<string>();
            int stateCheck = 0;
            bool isw = false;
            for (int i = 0; i < linesRead.Length; i++)
            {
                if (stateCheck == 0)
                {
                    if (linesRead[i].Contains("- (CGPoint)getBannerCenter:(NSInteger)position rootView:(UIView *)rootView {"))
                    {
                        stateCheck = 1;
                    }
                    linesWrite.Add(linesRead[i]);
                }
                else if (stateCheck == 1)
                {
                    if (linesRead[i].Contains("+ rootView.safeAreaInsets.top;"))
                    {
                        string newline = linesRead[i].Replace("+ rootView.safeAreaInsets.top;", $"+ rootView.safeAreaInsets.top / {AppConfig.AdjustBanner};");
                        linesWrite.Add(newline);
                        isw = true;
                    }
                    else if (linesRead[i].Contains("- rootView.safeAreaInsets.bottom;"))
                    {
                        string newline = linesRead[i].Replace("- rootView.safeAreaInsets.bottom;", $"- rootView.safeAreaInsets.bottom / {AppConfig.AdjustBanner};");
                        linesWrite.Add(newline);
                        isw = true;
                    }
                    else if (linesRead[i].Contains("return CGPointMake(rootView.frame.size.width / 2, y);"))
                    {
                        stateCheck = 2;
                        linesWrite.Add(linesRead[i]);
                    }
                    else
                    {
                        linesWrite.Add(linesRead[i]);
                    }
                }
                else
                {
                    linesWrite.Add(linesRead[i]);
                }
            }
            if (isw)
            {
                System.IO.File.WriteAllLines(pfilem, linesWrite);
                linesWrite.Clear();
            }
        }
    }

    static void fixBannerMax(string path, bool isPortrait = false)
    {
        string pfilem = path + "/Libraries/MaxSdk/AppLovin/Plugins/iOS/MAUnityAdManager.m";
        if (File.Exists(pfilem))
        {
            string[] linesRead = File.ReadAllLines(pfilem);
            List<string> linesWrite = new List<string>();
            int stateCheck = 0;
            bool isw = false;

            for (int i = 0; i < linesRead.Length; i++)
            {
                if (linesRead[i].Contains("[constraints addObject: [adView.bottomAnchor constraintEqualToAnchor: layoutGuide.bottomAnchor"))
                {
                    isw = true;
#if !MAX_USE_CONSTRAINTS
                    linesWrite.Add($"//                    [constraints addObject: [adView.bottomAnchor constraintEqualToAnchor: layoutGuide.bottomAnchor constant:superview.safeAreaInsets.bottom / {AppConfig.AdjustBanner}]];");
#else
                    if (AppConfig.AdjustBanner > 0)
                    {
                        linesWrite.Add($"                    [constraints addObject: [adView.bottomAnchor constraintEqualToAnchor: layoutGuide.bottomAnchor constant:superview.safeAreaInsets.bottom / {AppConfig.AdjustBanner}]];");
                    }
                    else
                    {
                        linesWrite.Add($"                    [constraints addObject: [adView.bottomAnchor constraintEqualToAnchor: layoutGuide.bottomAnchor]];");
                    }
#endif
                }
                else
                {
                    linesWrite.Add(linesRead[i]);
                }
            }
            if (isw)
            {
                System.IO.File.WriteAllLines(pfilem, linesWrite);
                linesWrite.Clear();
            }
        }
    }

    public static List<string> getDicnetSkadnet(string path)
    {
        List<string> dicnet = new List<string>();
        if (File.Exists(path))
        {
            var appCf = File.ReadAllLines(path);
            for (int i = 0; i < appCf.Length; i++)
            {
                int idxfind = 0;
                while (idxfind < appCf[i].Length)
                {
                    int idxsub = appCf[i].IndexOf("<string>", idxfind);
                    if (idxsub < 0)
                    {
                        break;
                    }
                    else
                    {
                        int idxff = appCf[i].IndexOf('.', idxsub + 8);
                        if (idxff > idxsub)
                        {
                            idxsub += 8;
                            string SKAdNetworkIdentifier = appCf[i].Substring(idxsub, idxff - idxsub);
                            dicnet.Add(SKAdNetworkIdentifier.ToLower() + ".skadnetwork");
                            idxfind = idxff;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

            }
        }

        return dicnet;
    }

    public static List<string> getAppQueriesSchemesTopon()
    {
        List<string> dicnet = new List<string>();

        string passet = Application.dataPath + "//GamePlugin/Ads/Topon/Editor/ApplicationQueriesSchemes.txt";
        if (File.Exists(passet))
        {
            var appCf = File.ReadAllLines(passet);
            for (int i = 0; i < appCf.Length; i++)
            {
                int idxfind = 0;
                while (idxfind < appCf[i].Length)
                {
                    int idxsub = appCf[i].IndexOf("<string>", idxfind);
                    if (idxsub < 0)
                    {
                        break;
                    }
                    else
                    {
                        int idxff = appCf[i].IndexOf("</", idxsub + 8);
                        if (idxff > idxsub)
                        {
                            idxsub += 8;
                            string SKAdNetworkIdentifier = appCf[i].Substring(idxsub, idxff - idxsub);
                            dicnet.Add(SKAdNetworkIdentifier);
                            idxfind = idxff;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

            }
        }

        return dicnet;
    }
}
#endif
