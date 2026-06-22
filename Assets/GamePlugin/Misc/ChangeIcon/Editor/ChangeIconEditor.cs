#if UNITY_IOS
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class ChangeIconEditor
{
    [PostProcessBuild(99)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget != BuildTarget.iOS)
            return;

        string projPath = PBXProject.GetPBXProjectPath(path);
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));

        string mainTargetGuid;
        var unityMainTargetGuidMethod = proj.GetType().GetMethod("GetUnityMainTargetGuid");
        if (unityMainTargetGuidMethod != null)
        {
            mainTargetGuid = (string)unityMainTargetGuidMethod.Invoke(proj, null);
        }
        else
        {
            mainTargetGuid = proj.TargetGuidByName("Unity-iPhone");
        }

        // Enable compiling all app icon sets in the asset catalog
        proj.SetBuildProperty(mainTargetGuid, "ASSETCATALOG_COMPILER_INCLUDE_ALL_APP_ICON_ASSETS", "YES");

        string iconSourceDir = Path.Combine(Application.dataPath, "IconSplash/iOS/Icons");
        if (!Directory.Exists(iconSourceDir))
        {
            Directory.CreateDirectory(iconSourceDir);
            Debug.Log("[ChangeIconEditor] Created Icons directory at: " + iconSourceDir);
            return;
        }

        string[] files = Directory.GetFiles(iconSourceDir, "*.png");
        if (files.Length == 0)
        {
            Debug.Log("[ChangeIconEditor] No alternate icons (.png) found in: " + iconSourceDir);
            return;
        }

        // Find the xcassets folder path
        string xcassetsPath = Path.Combine(path, "Unity-iPhone/Images.xcassets");
        if (!Directory.Exists(xcassetsPath))
        {
            xcassetsPath = Path.Combine(path, "Images.xcassets");
        }

        if (!Directory.Exists(xcassetsPath))
        {
            Debug.LogError("[ChangeIconEditor] Images.xcassets directory not found at: " + xcassetsPath);
            return;
        }

        List<string> alternateIconNames = new List<string>();

        foreach (string file in files)
        {
            if (file.Contains(".meta")) continue;

            string fileName = Path.GetFileName(file);
            
            // Skip dark/tinted files as they are secondary to standard icons
            if (fileName.EndsWith("_dark.png") || fileName.EndsWith("-dark.png") || 
                fileName.EndsWith("_tinted.png") || fileName.EndsWith("-tinted.png"))
            {
                continue;
            }

            string iconName = Path.GetFileNameWithoutExtension(file);
            
            // Create the appiconset directory inside Images.xcassets
            string appiconsetFolder = Path.Combine(xcassetsPath, iconName + ".appiconset");
            if (!Directory.Exists(appiconsetFolder))
            {
                Directory.CreateDirectory(appiconsetFolder);
            }

            // Copy the icon file to the appiconset folder
            string destIconPath = Path.Combine(appiconsetFolder, fileName);
            File.Copy(file, destIconPath, true);

            // Check and copy dark icon if exists
            string darkFileName = iconName + "_dark.png";
            string darkSrcPath = Path.Combine(iconSourceDir, darkFileName);
            if (!File.Exists(darkSrcPath))
            {
                darkFileName = iconName + "-dark.png";
                darkSrcPath = Path.Combine(iconSourceDir, darkFileName);
            }
            
            bool hasDark = File.Exists(darkSrcPath);
            if (hasDark)
            {
                File.Copy(darkSrcPath, Path.Combine(appiconsetFolder, darkFileName), true);
            }
            else
            {
                darkFileName = fileName; // fallback to standard icon
            }

            // Check and copy tinted icon if exists
            string tintedFileName = iconName + "_tinted.png";
            string tintedSrcPath = Path.Combine(iconSourceDir, tintedFileName);
            if (!File.Exists(tintedSrcPath))
            {
                tintedFileName = iconName + "-tinted.png";
                tintedSrcPath = Path.Combine(iconSourceDir, tintedFileName);
            }

            bool hasTinted = File.Exists(tintedSrcPath);
            if (hasTinted)
            {
                File.Copy(tintedSrcPath, Path.Combine(appiconsetFolder, tintedFileName), true);
            }
            else
            {
                tintedFileName = fileName; // fallback to standard icon
            }

            // Create resized standard icons in appiconset
            ResizeAndSavePNG(file, Path.Combine(appiconsetFolder, iconName + "-120.png"), 120, 120);
            ResizeAndSavePNG(file, Path.Combine(appiconsetFolder, iconName + "-152.png"), 152, 152);
            ResizeAndSavePNG(file, Path.Combine(appiconsetFolder, iconName + "-167.png"), 167, 167);
            ResizeAndSavePNG(file, Path.Combine(appiconsetFolder, iconName + "-180.png"), 180, 180);

            // Create resized dark icons in appiconset
            string dark120 = iconName + "_dark-120.png";
            string dark152 = iconName + "_dark-152.png";
            string dark167 = iconName + "_dark-167.png";
            string dark180 = iconName + "_dark-180.png";
            if (hasDark)
            {
                ResizeAndSavePNG(darkSrcPath, Path.Combine(appiconsetFolder, dark120), 120, 120);
                ResizeAndSavePNG(darkSrcPath, Path.Combine(appiconsetFolder, dark152), 152, 152);
                ResizeAndSavePNG(darkSrcPath, Path.Combine(appiconsetFolder, dark167), 167, 167);
                ResizeAndSavePNG(darkSrcPath, Path.Combine(appiconsetFolder, dark180), 180, 180);
            }
            else
            {
                dark120 = iconName + "-120.png";
                dark152 = iconName + "-152.png";
                dark167 = iconName + "-167.png";
                dark180 = iconName + "-180.png";
            }

            // Create resized tinted icons in appiconset
            string tinted120 = iconName + "_tinted-120.png";
            string tinted152 = iconName + "_tinted-152.png";
            string tinted167 = iconName + "_tinted-167.png";
            string tinted180 = iconName + "_tinted-180.png";
            if (hasTinted)
            {
                ResizeAndSavePNG(tintedSrcPath, Path.Combine(appiconsetFolder, tinted120), 120, 120);
                ResizeAndSavePNG(tintedSrcPath, Path.Combine(appiconsetFolder, tinted152), 152, 152);
                ResizeAndSavePNG(tintedSrcPath, Path.Combine(appiconsetFolder, tinted167), 167, 167);
                ResizeAndSavePNG(tintedSrcPath, Path.Combine(appiconsetFolder, tinted180), 180, 180);
            }
            else
            {
                tinted120 = iconName + "-120.png";
                tinted152 = iconName + "-152.png";
                tinted167 = iconName + "-167.png";
                tinted180 = iconName + "-180.png";
            }

            // Generate Contents.json for this appiconset supporting iOS 18+ standard, dark, and tinted slots with multiple sizes
            string contentsJsonPath = Path.Combine(appiconsetFolder, "Contents.json");
            string contentsJsonContent = "{\n" +
                                         "  \"images\" : [\n" +
                                         "    {\n" +
                                         "      \"idiom\" : \"universal\",\n" +
                                         "      \"platform\" : \"ios\",\n" +
                                         "      \"size\" : \"1024x1024\",\n" +
                                         "      \"scale\" : \"1x\",\n" +
                                         "      \"filename\" : \"" + fileName + "\"\n" +
                                         "    },\n" +
                                         "    {\n" +
                                         "      \"appearances\" : [\n" +
                                         "        {\n" +
                                         "          \"appearance\" : \"luminosity\",\n" +
                                         "          \"value\" : \"dark\"\n" +
                                         "        }\n" +
                                         "      ],\n" +
                                         "      \"idiom\" : \"universal\",\n" +
                                         "      \"platform\" : \"ios\",\n" +
                                         "      \"size\" : \"1024x1024\",\n" +
                                         "      \"scale\" : \"1x\",\n" +
                                         "      \"filename\" : \"" + darkFileName + "\"\n" +
                                         "    },\n" +
                                         "    {\n" +
                                         "      \"appearances\" : [\n" +
                                         "        {\n" +
                                         "          \"appearance\" : \"luminosity\",\n" +
                                         "          \"value\" : \"tinted\"\n" +
                                         "        }\n" +
                                         "      ],\n" +
                                         "      \"idiom\" : \"universal\",\n" +
                                         "      \"platform\" : \"ios\",\n" +
                                         "      \"size\" : \"1024x1024\",\n" +
                                         "      \"scale\" : \"1x\",\n" +
                                         "      \"filename\" : \"" + tintedFileName + "\"\n" +
                                         "    },\n" +
                                         "    {\n" +
                                         "      \"idiom\" : \"iphone\",\n" +
                                         "      \"size\" : \"60x60\",\n" +
                                         "      \"scale\" : \"2x\",\n" +
                                         "      \"filename\" : \"" + iconName + "-120.png\"\n" +
                                         "    },\n" +
                                         "    {\n" +
                                         "      \"idiom\" : \"iphone\",\n" +
                                         "      \"size\" : \"60x60\",\n" +
                                         "      \"scale\" : \"3x\",\n" +
                                         "      \"filename\" : \"" + iconName + "-180.png\"\n" +
                                         "    },\n" +
                                         "    {\n" +
                                         "      \"appearances\" : [\n" +
                                         "        {\n" +
                                         "          \"appearance\" : \"luminosity\",\n" +
                                         "          \"value\" : \"dark\"\n" +
                                         "        }\n" +
                                         "      ],\n" +
                                         "      \"idiom\" : \"iphone\",\n" +
                                         "      \"size\" : \"60x60\",\n" +
                                         "      \"scale\" : \"2x\",\n" +
                                         "      \"filename\" : \"" + dark120 + "\"\n" +
                                         "    },\n" +
                                         "    {\n" +
                                         "      \"appearances\" : [\n" +
                                         "        {\n" +
                                         "          \"appearance\" : \"luminosity\",\n" +
                                         "          \"value\" : \"dark\"\n" +
                                         "        }\n" +
                                         "      ],\n" +
                                         "      \"idiom\" : \"iphone\",\n" +
                                         "      \"size\" : \"60x60\",\n" +
                                         "      \"scale\" : \"3x\",\n" +
                                         "      \"filename\" : \"" + dark180 + "\"\n" +
                                         "    },\n" +
                                         "    {\n" +
                                         "      \"appearances\" : [\n" +
                                         "        {\n" +
                                         "          \"appearance\" : \"luminosity\",\n" +
                                         "          \"value\" : \"tinted\"\n" +
                                         "        }\n" +
                                         "      ],\n" +
                                         "      \"idiom\" : \"iphone\",\n" +
                                         "      \"size\" : \"60x60\",\n" +
                                         "      \"scale\" : \"2x\",\n" +
                                         "      \"filename\" : \"" + tinted120 + "\"\n" +
                                         "    },\n" +
                                         "    {\n" +
                                         "      \"appearances\" : [\n" +
                                         "        {\n" +
                                         "          \"appearance\" : \"luminosity\",\n" +
                                         "          \"value\" : \"tinted\"\n" +
                                         "        }\n" +
                                         "      ],\n" +
                                         "      \"idiom\" : \"iphone\",\n" +
                                         "      \"size\" : \"60x60\",\n" +
                                         "      \"scale\" : \"3x\",\n" +
                                         "      \"filename\" : \"" + tinted180 + "\"\n" +
                                         "    },\n" +
                                         "    {\n" +
                                         "      \"idiom\" : \"ipad\",\n" +
                                         "      \"size\" : \"76x76\",\n" +
                                         "      \"scale\" : \"2x\",\n" +
                                         "      \"filename\" : \"" + iconName + "-152.png\"\n" +
                                         "    },\n" +
                                         "    {\n" +
                                         "      \"appearances\" : [\n" +
                                         "        {\n" +
                                         "          \"appearance\" : \"luminosity\",\n" +
                                         "          \"value\" : \"dark\"\n" +
                                         "        }\n" +
                                         "      ],\n" +
                                         "      \"idiom\" : \"ipad\",\n" +
                                         "      \"size\" : \"76x76\",\n" +
                                         "      \"scale\" : \"2x\",\n" +
                                         "      \"filename\" : \"" + dark152 + "\"\n" +
                                         "    },\n" +
                                         "    {\n" +
                                         "      \"appearances\" : [\n" +
                                         "        {\n" +
                                         "          \"appearance\" : \"luminosity\",\n" +
                                         "          \"value\" : \"tinted\"\n" +
                                         "        }\n" +
                                         "      ],\n" +
                                         "      \"idiom\" : \"ipad\",\n" +
                                         "      \"size\" : \"76x76\",\n" +
                                         "      \"scale\" : \"2x\",\n" +
                                         "      \"filename\" : \"" + tinted152 + "\"\n" +
                                         "    },\n" +
                                         "    {\n" +
                                         "      \"idiom\" : \"ipad\",\n" +
                                         "      \"size\" : \"83.5x83.5\",\n" +
                                         "      \"scale\" : \"2x\",\n" +
                                         "      \"filename\" : \"" + iconName + "-167.png\"\n" +
                                         "    },\n" +
                                         "    {\n" +
                                         "      \"appearances\" : [\n" +
                                         "        {\n" +
                                         "          \"appearance\" : \"luminosity\",\n" +
                                         "          \"value\" : \"dark\"\n" +
                                         "        }\n" +
                                         "      ],\n" +
                                         "      \"idiom\" : \"ipad\",\n" +
                                         "      \"size\" : \"83.5x83.5\",\n" +
                                         "      \"scale\" : \"2x\",\n" +
                                         "      \"filename\" : \"" + dark167 + "\"\n" +
                                         "    },\n" +
                                         "    {\n" +
                                         "      \"appearances\" : [\n" +
                                         "        {\n" +
                                         "          \"appearance\" : \"luminosity\",\n" +
                                         "          \"value\" : \"tinted\"\n" +
                                         "        }\n" +
                                         "      ],\n" +
                                         "      \"idiom\" : \"ipad\",\n" +
                                         "      \"size\" : \"83.5x83.5\",\n" +
                                         "      \"scale\" : \"2x\",\n" +
                                         "      \"filename\" : \"" + tinted167 + "\"\n" +
                                         "    }\n" +
                                         "  ],\n" +
                                         "  \"info\" : {\n" +
                                         "    \"version\" : 1,\n" +
                                         "    \"author\" : \"xcode\"\n" +
                                         "  }\n" +
                                         "}";
            File.WriteAllText(contentsJsonPath, contentsJsonContent);
            
            alternateIconNames.Add(iconName);
            Debug.Log($"[ChangeIconEditor] Created appiconset for alternate icon: {iconName} (dark: {hasDark}, tinted: {hasTinted})");
        }

        // Write PBXProject updates (includes ASSETCATALOG_COMPILER_INCLUDE_ALL_APP_ICON_ASSETS setting)
        File.WriteAllText(projPath, proj.WriteToString());
        Debug.Log("[ChangeIconEditor] Successfully updated PBXProject.");
    }

    private static void ResizeAndSavePNG(string sourcePath, string destPath, int width, int height)
    {
        byte[] fileData = File.ReadAllBytes(sourcePath);
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(fileData))
        {
            RenderTexture rt = RenderTexture.GetTemporary(width, height);
            RenderTexture.active = rt;
            Graphics.Blit(tex, rt);
            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            
            byte[] bytes = result.EncodeToPNG();
            File.WriteAllBytes(destPath, bytes);
            
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(result);
        }
        else
        {
            Debug.LogError("[ChangeIconEditor] Failed to load image: " + sourcePath);
        }
    }
}
#endif
