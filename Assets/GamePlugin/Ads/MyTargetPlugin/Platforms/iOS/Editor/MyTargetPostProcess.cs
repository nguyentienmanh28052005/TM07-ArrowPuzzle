using UnityEngine;
#if UNITY_EDITOR
using System.IO;
#endif

public static class MyTargetPostProcess
{
#if ENABLE_ADS_MYTARGET
    //    [PostProcessBuild]
    //    public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
    //    {
    //        if (buildTarget == BuildTarget.iOS)
    //        {
    //            var projectPath = buildPath + "/Unity-iPhone.xcodeproj/project.pbxproj";

    //            var project = new PBXProject();
    //            project.ReadFromFile(projectPath);

    //            var unityFrameworkGuid = project.GetUnityFrameworkTargetGuid();
    //            project.SetBuildProperty(unityFrameworkGuid, "SWIFT_OBJC_BRIDGING_HEADER", "Libraries/GamePlugin/Ads/MyTargetPlugin/Platforms/iOS/MyTargetSwift-Bridging-Header.h");
    //            project.WriteToFile(projectPath);
    //        }
    //    }
#endif

    public static void enableOrDisableMytargetios()
    {
#if UNITY_IOS || UNITY_IPHONE
        string path = Application.dataPath + "/GamePlugin/Ads/MyTargetPlugin/Platforms/iOS/MyTargetiOS.m";
        if (File.Exists(path))
        {
            bool isEnable = false;
#if ENABLE_ADS_MYTARGET
            isEnable = true;
#endif
            bool isw = false;
            string[] alline = File.ReadAllLines(path);
            for (int i = 0; i < alline.Length; i++)
            {
                if (alline[i].Contains("#define ENABLE_ADS_MYTARGET"))
                {
                    if (isEnable)
                    {
                        if (alline[i].StartsWith("//"))
                        {
                            alline[i] = "#define ENABLE_ADS_MYTARGET";
                            isw = true;
                        }
                    }
                    else
                    {
                        if (alline[i].StartsWith("#define"))
                        {
                            alline[i] = "//#define ENABLE_ADS_MYTARGET";
                            isw = true;
                        }
                    }
                }
            }

            if (isw)
            {
                File.WriteAllLines(path, alline);
            }
        }
#endif
    }

    public static void AdjustBanner(string pathbuild)
    {
#if UNITY_IOS || UNITY_IPHONE
        if (AppConfig.AdjustBanner > 0)
        {
            string pathsrc = Application.dataPath + "/GamePlugin/Ads/MyTargetPlugin/Platforms/iOS/MyTargetiOS.m";
            string path = pathbuild + "/Libraries/GamePlugin/Ads/MyTargetPlugin/Platforms/iOS/MyTargetiOS.m";
            SettingBuildAndroid.coppyFileNormal(pathsrc, path);
            if (File.Exists(path))
            {
                bool isw = false;
                string[] alline = File.ReadAllLines(path);
                for (int i = 0; i < alline.Length; i++)
                {
                    if (alline[i].Contains("unityView.safeAreaInsets.top / 1;"))
                    {
                        isw = true;
                        alline[i] = alline[i].Replace("unityView.safeAreaInsets.top / 1;", $"unityView.safeAreaInsets.top / {AppConfig.AdjustBanner};");
                    }
                    else if (alline[i].Contains("unityView.safeAreaInsets.bottom / 1;"))
                    {
                        isw = true;
                        alline[i] = alline[i].Replace("unityView.safeAreaInsets.bottom / 1;", $"unityView.safeAreaInsets.bottom / {AppConfig.AdjustBanner};");
                    }
                }

                if (isw)
                {
                    File.WriteAllLines(path, alline);
                }
            }
        }
#endif
    }
}
