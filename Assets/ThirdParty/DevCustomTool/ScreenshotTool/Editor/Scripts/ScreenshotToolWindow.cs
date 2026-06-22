#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ScreenshotToolWindow : EditorWindow
{
    private static string NameFolderScreenshots = "Screenshots";
    private static string PathProject = Application.dataPath.Replace("/Assets", "");

    private List<ResolutionItem> resolutions = new List<ResolutionItem>();
    private string saveFolder = "";
    private Vector2 scrollPosition;

    // Fields for adding new resolution
    private string newName = "New Screen";
    private int newWidth = 1080;
    private int newHeight = 1920;
    private ScreenshotOrientation newOrientation = ScreenshotOrientation.Portrait;
    private TargetPlatform newPlatform = TargetPlatform.iOS;
    private TargetPlatform selectedTab = TargetPlatform.iOS;

    private CapturePlatformFilter capturePlatformFilter = CapturePlatformFilter.All;
    private CaptureOrientationFilter captureOrientationFilter = CaptureOrientationFilter.Both;

    private const string PREFS_PAUSE_KEY = "ScreenshotTool_ForcePause_v1";
    private const string PREFS_HIGHEST_QUALITY_KEY = "ScreenshotTool_HighestQuality_v1";
    private const string PREFS_KEY = "ScreenshotTool_Resolutions_v3";
    private const string PREFS_FOLDER_KEY = "ScreenshotTool_SaveFolder_v2";

    [MenuItem("ScreenshotTool/Open Screenshot Window", priority = 0)]
    public static void ShowWindow()
    {
        GetWindow<ScreenshotToolWindow>("Screenshot Settings");
    }

    [MenuItem("ScreenshotTool/Open Screenshot Storage", priority = 1)]
    public static void OpenScreenshotStorage()
    {
        string folderPath = EditorPrefs.GetString(PREFS_FOLDER_KEY, Path.Combine(PathProject, NameFolderScreenshots));
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        EditorUtility.RevealInFinder(folderPath);
    }

    [MenuItem("ScreenshotTool/Screen Shot Without Canvas &x")]
    private static void GetScrShot()
    {
        string folderPath = Path.Combine(PathProject, NameFolderScreenshots);
        ScreenshotCaptureManager.forcePauseGame = EditorPrefs.GetBool(PREFS_PAUSE_KEY, true);
        ScreenshotCaptureManager.useHighestQuality = EditorPrefs.GetBool(PREFS_HIGHEST_QUALITY_KEY, false);
        ScreenshotCaptureManager.StartSingleCapture(folderPath, true);
    }

    [MenuItem("ScreenshotTool/Screen Shot With Canvas &z")]
    private static void GetScrShotCanvas()
    {
        string folderPath = Path.Combine(PathProject, NameFolderScreenshots);
        ScreenshotCaptureManager.forcePauseGame = EditorPrefs.GetBool(PREFS_PAUSE_KEY, true);
        ScreenshotCaptureManager.useHighestQuality = EditorPrefs.GetBool(PREFS_HIGHEST_QUALITY_KEY, false);
        ScreenshotCaptureManager.StartSingleCapture(folderPath, false);
    }

    private void OnEnable()
    {
        LoadSettings();
    }

    private void OnDisable()
    {
        SaveSettings();
    }

    private void LoadSettings()
    {
        if (EditorPrefs.HasKey(PREFS_PAUSE_KEY))
        {
            ScreenshotCaptureManager.forcePauseGame = EditorPrefs.GetBool(PREFS_PAUSE_KEY);
        }

        if (EditorPrefs.HasKey(PREFS_HIGHEST_QUALITY_KEY))
        {
            ScreenshotCaptureManager.useHighestQuality = EditorPrefs.GetBool(PREFS_HIGHEST_QUALITY_KEY);
        }

        if (string.IsNullOrEmpty(saveFolder))
        {
            saveFolder = Path.Combine(PathProject, NameFolderScreenshots).Replace("\\", "/");
        }

        if (EditorPrefs.HasKey(PREFS_FOLDER_KEY))
        {
            saveFolder = EditorPrefs.GetString(PREFS_FOLDER_KEY);
        }

        if (EditorPrefs.HasKey(PREFS_KEY))
        {
            string json = EditorPrefs.GetString(PREFS_KEY);
            if (!string.IsNullOrEmpty(json))
            {
                var wrapper = JsonUtility.FromJson<SerializationWrapper<ResolutionItem>>(json);
                if (wrapper != null && wrapper.items != null && wrapper.items.Count > 0)
                {
                    resolutions = wrapper.items;
                    return;
                }
            }
        }

        ResetToDefaults();
    }

    private void ResetToDefaults()
    {
        resolutions = new List<ResolutionItem>()
        {
            new ResolutionItem("iOS 5.5 Inch (iPhone 8 Plus)", 1242, 2208, ScreenshotOrientation.Portrait, TargetPlatform.iOS),
            new ResolutionItem("iOS 5.5 Inch (iPhone 8 Plus)", 2208, 1242, ScreenshotOrientation.Landscape, TargetPlatform.iOS),
            new ResolutionItem("iOS 6.5 Inch (XS Max / 11 Pro Max)", 1242, 2688, ScreenshotOrientation.Portrait, TargetPlatform.iOS),
            new ResolutionItem("iOS 6.5 Inch (XS Max / 11 Pro Max)", 2688, 1242, ScreenshotOrientation.Landscape, TargetPlatform.iOS),
            new ResolutionItem("iOS 12.9 Inch (iPad Pro)", 2048, 2732, ScreenshotOrientation.Portrait, TargetPlatform.iOS),
            new ResolutionItem("iOS 12.9 Inch (iPad Pro)", 2732, 2048, ScreenshotOrientation.Landscape, TargetPlatform.iOS),
            new ResolutionItem("Android FHD (16:9)", 1080, 1920, ScreenshotOrientation.Portrait, TargetPlatform.Android),
            new ResolutionItem("Android FHD (16:9)", 1920, 1080, ScreenshotOrientation.Landscape, TargetPlatform.Android),
            new ResolutionItem("Android QHD (18:9)", 1440, 2880, ScreenshotOrientation.Portrait, TargetPlatform.Android),
            new ResolutionItem("Android QHD (18:9)", 2880, 1440, ScreenshotOrientation.Landscape, TargetPlatform.Android),
        };
        SaveSettings();
    }

    private void SaveSettings()
    {
        var wrapper = new SerializationWrapper<ResolutionItem>(resolutions);
        string json = JsonUtility.ToJson(wrapper);
        EditorPrefs.SetString(PREFS_KEY, json);
        EditorPrefs.SetString(PREFS_FOLDER_KEY, saveFolder);
        EditorPrefs.SetBool(PREFS_PAUSE_KEY, ScreenshotCaptureManager.forcePauseGame);
        EditorPrefs.SetBool(PREFS_HIGHEST_QUALITY_KEY, ScreenshotCaptureManager.useHighestQuality);
    }

    private void OnGUI()
    {
        GUILayout.Label("Settings", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        ScreenshotCaptureManager.forcePauseGame = EditorGUILayout.Toggle("Force Pause Game On Capture", ScreenshotCaptureManager.forcePauseGame);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        ScreenshotCaptureManager.useHighestQuality = EditorGUILayout.Toggle("Use Highest Quality", ScreenshotCaptureManager.useHighestQuality);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        ScreenshotCaptureManager.saveFolderFormat = (SaveFolderFormat)EditorGUILayout.EnumPopup("Save Folder Format", ScreenshotCaptureManager.saveFolderFormat);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Save Folder:", GUILayout.Width(80));
        saveFolder = EditorGUILayout.TextField(saveFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(70)))
        {
            string newPath = EditorUtility.OpenFolderPanel("Select Screenshot Folder", saveFolder, "");
            if (!string.IsNullOrEmpty(newPath))
            {
                saveFolder = newPath;
                SaveSettings();
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        if (GUILayout.Toggle(selectedTab == TargetPlatform.iOS, "iOS Devices", "Button")) selectedTab = TargetPlatform.iOS;
        if (GUILayout.Toggle(selectedTab == TargetPlatform.Android, "Android Devices", "Button")) selectedTab = TargetPlatform.Android;
        if (GUILayout.Toggle(selectedTab == TargetPlatform.Custom, "Custom", "Button")) selectedTab = TargetPlatform.Custom;
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUILayout.Label("Resolutions (" + selectedTab.ToString() + ")", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Use", GUILayout.Width(40));
        GUILayout.Label("Name", GUILayout.Width(200));
        GUILayout.Label("Size (W x H)", GUILayout.Width(130));
        GUILayout.Label("Orientation", GUILayout.Width(80));
        GUILayout.EndHorizontal();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

        for (int i = 0; i < resolutions.Count; i++)
        {
            var item = resolutions[i];
            if (item.platform != selectedTab) continue;

            GUILayout.BeginHorizontal();
            item.enabled = EditorGUILayout.Toggle(item.enabled, GUILayout.Width(70));
            item.name = EditorGUILayout.TextField(item.name, GUILayout.Width(200));

            GUILayout.BeginHorizontal(GUILayout.Width(100));
            item.width = EditorGUILayout.IntField(item.width, GUILayout.Width(45));
            GUILayout.Label("x", GUILayout.Width(8));
            item.height = EditorGUILayout.IntField(item.height, GUILayout.Width(45));
            GUILayout.EndHorizontal();

            ScreenshotOrientation prevPortrait = item.orientation;
            item.orientation = (ScreenshotOrientation)EditorGUILayout.EnumPopup(item.orientation, GUILayout.Width(80));

            if (prevPortrait != item.orientation)
            {
                (item.width, item.height) = (item.height, item.width);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("X", GUILayout.Width(30)))
            {
                resolutions.RemoveAt(i);
                i--;
                SaveSettings();
            }

            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space();

        GUILayout.BeginVertical("box");
        GUILayout.Label("Add New Resolution", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        GUILayout.Label("Name:", GUILayout.Width(40));
        newName = EditorGUILayout.TextField(newName, GUILayout.Width(100));
        GUILayout.Space(10);
        GUILayout.Label("Size:", GUILayout.Width(30));
        newWidth = EditorGUILayout.IntField(newWidth, GUILayout.Width(40));
        GUILayout.Label("x", GUILayout.Width(10));
        newHeight = EditorGUILayout.IntField(newHeight, GUILayout.Width(40));
        GUILayout.Space(10);
        GUILayout.Label("Orientation:", GUILayout.Width(40));
        
        ScreenshotOrientation prevNewOrientation = newOrientation;
        newOrientation = (ScreenshotOrientation)EditorGUILayout.EnumPopup(newOrientation, GUILayout.Width(80));

        if (prevNewOrientation != newOrientation)
        {
            (newWidth, newHeight) = (newHeight, newWidth);
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Add", GUILayout.Width(60)))
        {
            resolutions.Add(new ResolutionItem(newName, newWidth, newHeight, newOrientation, selectedTab));
            SaveSettings();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        EditorGUILayout.Space();

        GUILayout.BeginVertical("box");
        GUILayout.Label("Capture Queue Settings", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        capturePlatformFilter = (CapturePlatformFilter)EditorGUILayout.EnumPopup("Platform:", capturePlatformFilter, GUILayout.Width(250));
        GUILayout.Space(20);
        captureOrientationFilter = (CaptureOrientationFilter)EditorGUILayout.EnumPopup("Orientation:", captureOrientationFilter, GUILayout.Width(250));
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        GUI.enabled = !ScreenshotCaptureManager.isCapturing;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset to Defaults", GUILayout.Height(35)))
        {
            if (EditorUtility.DisplayDialog("Confirm Reset", "Are you sure you want to reset all resolutions to defaults?", "Yes", "No"))
            {
                ResetToDefaults();
            }
        }

        if (GUILayout.Button("Batch Capture Selected", GUILayout.Height(35)))
        {
            StartCaptureQueue();
        }
        GUILayout.EndHorizontal();
        GUI.enabled = true;
        GUILayout.EndVertical();

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Capture Current Screen", GUILayout.Height(40)))
        {
            GetScrShotCanvas();
        }

        if (GUILayout.Button("Open Folder", GUILayout.Height(40)))
        {
            OpenScreenshotStorage();
        }
        GUILayout.EndHorizontal();

        if (ScreenshotCaptureManager.isCapturing)
        {
            EditorGUILayout.HelpBox("Capturing screenshots... Please wait.", MessageType.Info);
        }
    }

    private void StartCaptureQueue()
    {
        if (resolutions.Count == 0) return;
        SaveSettings();

        List<ResolutionItem> activeResolutions = new List<ResolutionItem>();
        foreach (var r in resolutions)
        {
            if (!r.enabled) continue;
            if (capturePlatformFilter != CapturePlatformFilter.All && r.platform.ToString() != capturePlatformFilter.ToString()) continue;
            if (captureOrientationFilter == CaptureOrientationFilter.PortraitOnly && r.orientation != ScreenshotOrientation.Portrait) continue;
            if (captureOrientationFilter == CaptureOrientationFilter.LandscapeOnly && r.orientation != ScreenshotOrientation.Landscape) continue;

            activeResolutions.Add(r);
        }

        if (activeResolutions.Count == 0)
        {
            Debug.LogWarning("[ScreenshotTool] No resolutions selected for capture.");
            return;
        }

        ScreenshotCaptureManager.StartCaptureQueue(activeResolutions, saveFolder);
    }
}
#endif
