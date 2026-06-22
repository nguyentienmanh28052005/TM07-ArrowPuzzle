#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;

public class AutoSetIconAndSplash : MonoBehaviour
{
    public static void SetIconAndSplash(string iconSplashFolder)
    {
        string androidFolder = $"{iconSplashFolder}/Android";
        string iosFolder = $"{iconSplashFolder}/iOS";
        
        BuildTargetGroup currentGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

        switch (currentGroup)
        {
            case BuildTargetGroup.Android:
                Debug.Log("🟢 Setting up Android...");
                SetPlatformIcon(currentGroup, $"{androidFolder}/");
                SetSplashScreen(currentGroup, $"{androidFolder}/splash_logo.png", $"{androidFolder}/splash_bg.png");
                break;

            case BuildTargetGroup.iOS:
                Debug.Log("🟢 Setting up iOS...");
                SetPlatformIcon(currentGroup, $"{iosFolder}/Icon.png");
                SetSplashScreen(currentGroup, $"{iosFolder}/splash_logo.png", $"{iosFolder}/splash_bg.png");
                SetLaunchScreenStoryboard($"{iosFolder}");
                break;

            default:
                Debug.LogWarning($"⚠️ Current platform '{currentGroup}' is not supported.");
                break;
        }

        Debug.Log("✅ Auto setup completed for current platform.");
    }

    static void SetPlatformIcon(BuildTargetGroup group, string defaultIconPath)
    {
        if (group == BuildTargetGroup.Android)
        {
            // Ưu tiên icon trong mipmap-* nếu có
            string basePath = System.IO.Path.GetDirectoryName(defaultIconPath);
            string[] densities = { "mdpi", "hdpi", "xhdpi", "xxhdpi", "xxxhdpi" };
            bool foundAny = false;
            
            
                        
            string nameAdaptive = "";
            string namebg = "";
            string nameRound = "";
            string nameLegacy = "";
            
            foreach (var abc in densities)
            {
                string pathhh = $"{basePath}/mipmap-{abc}";
                if (Directory.Exists(pathhh))
                {
                    string[] yyyy = Directory.GetFiles(pathhh);
                    foreach (var lll in yyyy)
                    {
                        if (lll.Contains(".webp") && !lll.Contains(".meta"))
                        {
                            string newname = lll.Replace(".webp", ".png");
                            File.Move(lll, newname);
                        }

                        if (!lll.Contains(".meta"))
                        {
                            if (lll.Contains("_foreground"))
                            {
                                nameAdaptive = Path.GetFileName(lll);
                            }
                            else if (lll.Contains("_round"))
                            {
                                nameRound = Path.GetFileName(lll);
                            }
                            else if (lll.Contains("_background"))
                            {
                                namebg = Path.GetFileName(lll);
                            }
                            else
                            {
                                nameLegacy = Path.GetFileName(lll);
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(namebg)) namebg = nameAdaptive;
            PlatformIcon[] Adaptives = PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android, AndroidPlatformIconKind.Adaptive);
            PlatformIcon[] Round = PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android, AndroidPlatformIconKind.Round);
            PlatformIcon[] Legacy = PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android, AndroidPlatformIconKind.Legacy);
            for (int i = 0; i < densities.Length; i++)
            {
                Texture2D[] icons = new Texture2D[2];
                string path = $"{basePath}/mipmap-{Adaptives[i].ToString().Split(' ')[1]}/{namebg}";
                icons[0] = TryLoadAssetAtPath<Texture2D>(path);
                
                path = $"{basePath}/mipmap-{Adaptives[i].ToString().Split(' ')[1]}/{nameAdaptive}";
                icons[1] = TryLoadAssetAtPath<Texture2D>(path);
                Adaptives[i].SetTextures(icons);
               
                
                path = $"{basePath}/mipmap-{Round[i].ToString().Split(' ')[1]}/{nameRound}"; 
                var icon = TryLoadAssetAtPath<Texture2D>(path);
                Round[i].SetTextures(icon);
                
                path = $"{basePath}/mipmap-{Round[i].ToString().Split(' ')[1]}/{nameLegacy}"; 
                icon = TryLoadAssetAtPath<Texture2D>(path);
                Legacy[i].SetTextures(icon);
                
            }
            PlayerSettings.SetPlatformIcons(BuildTargetGroup.Android, AndroidPlatformIconKind.Adaptive, Adaptives);

            Debug.Log("✅ Android icons set.");
        }
        else
        {
            // iOS hoặc nền tảng khác chỉ dùng 1 icon
            Texture2D icon = TryLoadAssetAtPath<Texture2D>(defaultIconPath);
            if (icon == null)
            {
                Debug.LogWarning($"❌ Icon not found at: {defaultIconPath}");
                return;
            }

            PlayerSettings.SetIconsForTargetGroup(group, new[] { icon });
            Debug.Log($"✅ Icon set for {group}");
        }
    }


    static void SetSplashScreen(BuildTargetGroup group, string logoPath, string bgPath)
    {
        Sprite logo = TryLoadAssetAtPath<Sprite>(logoPath);
        Sprite bg = TryLoadAssetAtPath<Sprite>(bgPath);

        if (logo == null || bg == null)
        {
            Debug.LogWarning($"❌ Missing splash assets for {group}");
            return;
        }

        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.SplashScreen.showUnityLogo = false;
        PlayerSettings.SplashScreen.blurBackgroundImage = false;
        PlayerSettings.SplashScreen.background = bg;
        PlayerSettings.SplashScreen.logos = new[] { PlayerSettings.SplashScreenLogo.Create(2, logo) };

        Debug.Log($"✅ Splash screen set for {group}");
    }

    static void SetLaunchScreenStoryboard(string path)
    {
        PlayerSettings.iOS.SetiPadLaunchScreenType(iOSLaunchScreenType.CustomStoryboard);
        PlayerSettings.iOS.SetiPhoneLaunchScreenType(iOSLaunchScreenType.CustomStoryboard);

        var playerSettingsType = typeof(PlayerSettings);
        var method = playerSettingsType.GetMethod("GetSerializedObject", BindingFlags.NonPublic | BindingFlags.Static);
        if (method == null)
        {
            Debug.LogError("❌ Cannot find internal method 'PlayerSettings.GetSerializedObject()'");
            return;
        }

        SerializedObject serializedObject = (SerializedObject)method.Invoke(null, null);

        // Set tên file storyboard
        var storyboardNameProp = serializedObject.FindProperty("iOSLaunchScreenCustomStoryboardPath");
        if (storyboardNameProp != null)
        {
            storyboardNameProp.stringValue = path + "LaunchScreen-iPhone.storyboard"; // không kèm đuôi
            Debug.Log("✅ Set LaunchScreen storyboard name = LaunchScreen");
        }

        // Set tên file storyboard
        storyboardNameProp = serializedObject.FindProperty("iOSLaunchScreeniPadCustomStoryboardPath");
        if (storyboardNameProp != null)
        {
            storyboardNameProp.stringValue = path + "LaunchScreen-iPad.storyboard"; // không kèm đuôi
            Debug.Log("✅ Set LaunchScreen storyboard name = LaunchScreen");
        }

        serializedObject.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        Debug.Log("✅ LaunchScreen settings applied via reflection.");


        Debug.Log("✅ iOS LaunchScreen set to 'LaunchScreen.storyboard'");
    }
    
    static T TryLoadAssetAtPath<T>(string path) where T : UnityEngine.Object
    {
        string pngPath = path.Replace(".jpg", ".png");
        string jpgPath = path.Replace(".png", ".jpg");

        T asset = AssetDatabase.LoadAssetAtPath<T>(pngPath);
        if (asset != null)
            return asset;

        asset = AssetDatabase.LoadAssetAtPath<T>(jpgPath);
        if (asset != null)
            return asset;

        Debug.LogWarning($"⚠️ Asset not found: {pngPath} or {jpgPath}");
        return null;
    }
}
#endif