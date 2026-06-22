using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using mygame.sdk;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Android;
using System.Threading;
#if UNITY_IOS || UNITY_IPHONE
using UnityEditor.iOS;
#endif
using System.IO;
#endif

[CustomEditor(typeof(mygame.sdk.SDKManager)), CanEditMultipleObjects]
public class SettingBuildAndroid : Editor
{
    public static mygame.sdk.SDKManager groupControl = null;
    public static string unityver = "20";
    public static string defaultOrientation = "0";
    bool isTestInApp = false;
    bool forceGenBaseKey = false;
    string packageNameTest = "";
    string mempackageNameTest = "";
    string DefineSymbolAdd = "";//
    string memDefineSymbolAdd = "";
    string GameName = "";
    string MemGameName = "";
    public static bool isIronEnable = false;

    //
    private string verAdmob = "23.4.0";

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(GameName) || string.IsNullOrEmpty(MemGameName))
        {
            GameName = getMemSetting("key_mem_gamename", "");
            MemGameName = GameName;
        }
        if (string.IsNullOrEmpty(packageNameTest) || string.IsNullOrEmpty(mempackageNameTest))
        {
            packageNameTest = getMemSetting("key_mem_pkgtest", "");
            mempackageNameTest = packageNameTest;
        }
        if (string.IsNullOrEmpty(DefineSymbolAdd) || string.IsNullOrEmpty(memDefineSymbolAdd))
        {
            DefineSymbolAdd = getMemSetting("key_mem_define_sb_a", "");
            memDefineSymbolAdd = DefineSymbolAdd;
        }
        addSDKUpdate();
    }

    public override void OnInspectorGUI()
    {
        GUILayout.BeginVertical();
        GameName = EditorGUILayout.TextField("GameName", GameName);
        if (GameName.CompareTo(MemGameName) != 0)
        {
            setMemSetting("key_mem_gamename", GameName);
            MemGameName = GameName;
        }
        if (GUILayout.Button("SetupSdk"))
        {
            getGroupControl(target);
            doSetting4Game();
            AssetDatabase.Refresh();
        }
        if (GUILayout.Button("InitLib"))
        {
            getGroupControl(target);
            initLibs();
            AssetDatabase.Refresh();
        }

        packageNameTest = EditorGUILayout.TextField("PackageNameTest", packageNameTest);
        if (packageNameTest.CompareTo(mempackageNameTest) != 0)
        {
            setMemSetting("key_mem_pkgtest", packageNameTest);
            mempackageNameTest = packageNameTest;
        }

        DefineSymbolAdd = EditorGUILayout.TextField("Define Symbols Add", DefineSymbolAdd);
        if (DefineSymbolAdd.CompareTo(memDefineSymbolAdd) != 0)
        {
            setMemSetting("key_mem_define_sb_a", DefineSymbolAdd);
            memDefineSymbolAdd = DefineSymbolAdd;
        }
        EditorGUILayout.BeginHorizontal();
        forceGenBaseKey = EditorGUILayout.Toggle("ForceGenBaseKey", forceGenBaseKey);
        bool ismem = (PlayerPrefs.GetInt("key_mem_is_inapp", 0) == 1);
        isTestInApp = EditorGUILayout.Toggle("isTestInapp", ismem);
        if (isTestInApp)
        {
            PlayerPrefs.SetInt("key_mem_is_inapp", 1);
        }
        else
        {
            PlayerPrefs.SetInt("key_mem_is_inapp", 0);
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Set Icon & Splash"))
        {
            getGroupControl(target);

#if UNITY_ANDROID
            setSpslashLoading("Android");
            setMyopenAds("Android/mipmap-hdpi");
#elif UNITY_IOS || UNITY_IPHONE
            setSpslashLoading("iOS");
            setMyopenAds("iOS");
#endif

            setIcon();
            AssetDatabase.Refresh();
        }
        // if (GUILayout.Button("GetSupprtFlag"))
        // {
        //     getGroupControl(target);
        //     GetSupprtFlag();
        //     AssetDatabase.Refresh();
        // }
        // if (GUILayout.Button("WiteStringKey"))
        // {
        //     getGroupControl(target);
        //     WiteStringKey();
        //     AssetDatabase.Refresh();
        // }
        GUILayout.EndVertical();

        try
        {
            base.OnInspectorGUI();
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: OnInspectorGUI ex=" + ex.ToString());
        }
    }

    public static string getMemSetting(string key, string defaultvalue)
    {
        string re = defaultvalue;
        if (re == null)
        {
            re = "";
        }
        string levelDirectoryPathSrc = Application.dataPath + "/GamePlugin/Editor/.memsetting.txt";
        try
        {
            string[] alline = File.ReadAllLines(levelDirectoryPathSrc);
            for (int i = 0; i < alline.Length; i++)
            {
                if (alline[i].StartsWith(key + ":"))
                {
                    re = alline[i].Replace(key + ":", "");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log("mysdk: ex=" + ex.ToString());
        }

        return re;
    }

    private static void setMemSetting(string key, string value)
    {
        string levelDirectoryPathSrc = Application.dataPath + "/GamePlugin/Editor/.memsetting.txt";
        try
        {
            List<string> lww = new List<string>();
            bool isWwww = false;
            bool hasInMem = false;
            if (File.Exists(levelDirectoryPathSrc))
            {
                string[] alline = File.ReadAllLines(levelDirectoryPathSrc);
                for (int i = 0; i < alline.Length; i++)
                {
                    if (alline[i].StartsWith(key + ":"))
                    {
                        hasInMem = true;
                        if (value == null || value.Length == 0)
                        {
                            isWwww = true;
                        }
                        else
                        {
                            string ladd = key + ":" + value;
                            if (alline[i].CompareTo(ladd) != 0)
                            {
                                isWwww = true;
                                lww.Add(ladd);
                            }
                        }
                    }
                    else
                    {
                        lww.Add(alline[i]);
                    }
                }
            }
            if (!hasInMem && value != null && value.Length > 0)
            {
                isWwww = true;
                lww.Add(key + ":" + value);
            }
            if (isWwww)
            {
                File.WriteAllLines(levelDirectoryPathSrc, lww);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: setMemSetting ex=" + ex.ToString());
        }
    }

    void addSDKUpdate()
    {
        getGroupControl(target);
        if (groupControl.transform.GetComponent<SDKUpdate>() == null)
        {
            groupControl.gameObject.AddComponent<SDKUpdate>();
        }
    }

    private static void getGroupControl(UnityEngine.Object ob)
    {
        if (groupControl == null)
        {
            groupControl = (mygame.sdk.SDKManager)ob;
            getVerUnity();
        }
        getOrientation();
    }

    static void getVerUnity()
    {
        try
        {
            string passet = Application.dataPath;
            string ppro = passet.Replace("/Assets", "");
            string psetting = ppro + "/ProjectSettings/ProjectVersion.txt";
            string[] allLinesMeta = File.ReadAllLines(psetting);
            foreach (var line in allLinesMeta)
            {
                if (line.Contains("m_EditorVersion"))
                {
                    if (line.Contains("2021"))
                    {
                        unityver = "21";
                    }
                    else if (line.Contains("2022"))
                    {
                        unityver = "22";
                    }
                    else if (line.Contains("2020"))
                    {
                        unityver = "20";
                    }
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log("mysdk: ex=" + ex.ToString());
        }
    }

    static void getOrientation()
    {
        try
        {
            string passet = Application.dataPath;
            string ppro = passet.Replace("/Assets", "");
            string psetting = ppro + "/ProjectSettings/ProjectSettings.asset";
            string[] allLinesMeta = File.ReadAllLines(psetting);
            foreach (var line in allLinesMeta)
            {
                if (line.Contains("defaultScreenOrientation"))
                {
                    string[] llll = line.Split(':');
                    if (llll.Length == 2)
                    {
                        defaultOrientation = llll[1];
                        defaultOrientation = defaultOrientation.Replace(" ", "");
                        setMemSetting("key_mem_df_orien", defaultOrientation);
                    }
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log("mysdk: ex=" + ex.ToString());
        }
    }

    private void doSetting4Game(bool istest = false)
    {
        string mydvid = SystemInfo.deviceUniqueIdentifier;
        Debug.Log("abc=" + mydvid);
        string[] whileListDevices = {
            "69ff05525046b150d7f28d45052705772c94001a",
            "ed0cce092ba3c88fa8f7a01d9a715e6873fd80ff",
            "8b5e79c73f144af53538b0abc53bb15f23bec748",
            "174D9302-3683-5F19-B27C-0B4AEBD08F8D",
            "3826a02f015a27caa46cb0e09a9fe748a335ca53",
            "C2C39E20-8B14-5CE2-95C9-3206C611AE67",
            "9C1D7D4D-4727-5CA8-82E9-18D67A4D2317",
            "868E6C2A-FF96-5913-97B2-3342DDE97787",
            "DA664010-0E4B-530A-8C5C-69B44662FB20",
            "361640e93b2c09180d6bf25e858798a5cde9f9c4",
            "43E8B34B-44AA-5C0C-A64B-B48B29EF79FF",
            "DA390F43-496B-5856-B092-AE514135C1DC",
            "72D2DC66-25AB-5854-BF12-85592961B1D3",
            "169427E2-9F54-5840-A7FB-C1D8D2A6BD32",
            "D6AF302B-71C0-5EDB-B196-59F4AEDE735B",
            "1448ED9D-C8E8-58F3-9504-842AD9B5E848",
            "9D27098C-6D03-5CC8-A610-B95F25E6C5C8",
            "E5492D1D-AB60-5D21-817B-E8C770D62B2F",
            "84A7D537-893C-5845-870B-D2C5CBB54262",
            "6F04DDF5-503A-5BEE-9BED-ED05726F3EC8",
            "906A0959-82D7-5224-94F1-D4FA8BE30AE0",
            "0F75A594-12A3-5B3A-8C4D-FA84591878B1",
            "22B33593-19EE-51F0-8B9D-69DC32FBB36B",
            "740B8BEE-71B7-52C2-A484-CEE6CF1A673F",
            "a92e36ed583c5cd1f77fc9a9875e1c94e28ae05c",
            "41643fa7dee6ab26a8d8eaf5b9e298ffe51dd0b3",
            "EF32EB3C-1F29-5FAE-A5C5-83D4ED7ECD9B",
            "ad33161c186732df9de34ec98b3552961e6e4073",
            "e9ad407031753fe49c850a8bdd46820fe9a8657c",
            "b5a70fabdf2c194e9bf2201d1156bc7a70f96780",
            "f0515de267e952d3701c57dc7d5adce13ca94e57",
            "a004cf84a2e0d5c5eced1e3a2484a5343e92e84b",
            "defecbffff0bbb3e3acd5d48796b8542bcf90acb",
            "e9cbc59144c8473782c2d365b2ac93804f53dc56",
            "C63A610E-732E-529F-9B2F-C22CCD336DFD"
        };

        if (!istest)
        {
            istest = true;
            foreach (var item in whileListDevices)
            {
                if (item.CompareTo(mydvid) == 0)
                {
                    istest = false;
                    break;
                }
            }
            if (istest)
            {
                Debug.Log("abc=change is test");
            }
            else
            {
                Debug.Log("abc=set sdk main");
            }
        }

        try
        {
            parConfigGame(istest);
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: doSetting4Game ex=" + ex.ToString());
        }
        mygame.sdk.AdsHelper adshelper = groupControl.transform.Find("AdvHelper").GetComponent<mygame.sdk.AdsHelper>();
        SdkSetup.SetupSdk(groupControl, adshelper, istest);

#if UNITY_ANDROID
        if (groupControl.isLicenses)
        {
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;
        }
#elif UNITY_IOS || UNITY_IPHONE
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.SplashScreen.showUnityLogo = false;
#endif
        coppyRes2Assets(GameName);

        synchAdmoveVer(verAdmob);
    }

    private void setIcon()
    {
        try
        {
#if UNITY_ANDROID
            setIconAndroid();
#elif UNITY_IOS || UNITY_IPHONE
            setIconIOS();
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: setIcon ex=" + ex.ToString());
        }
    }

    private void setSpslashLoading(string platformName)
    {
        try
        {
            string pathGetNameSplash = Application.dataPath + $"/IconSplash{GameName}/{platformName}";
            string nameFolderICSplash;
            if (!Directory.Exists(pathGetNameSplash))
            {
                nameFolderICSplash = "IconSplash";
                pathGetNameSplash = Application.dataPath + $"/IconSplash/{platformName}";
            }
            else
            {
                nameFolderICSplash = $"IconSplash{GameName}";
            }
            string[] arrppp = {
                "/Android/mipmap-hdpi",
                "/Android/mipmap-mdpi",
                "/Android/mipmap-xhdpi",
                "/Android/mipmap-xxhdpi",
                "/Android/mipmap-xxxhdpi",
            };
            foreach (var abc in arrppp)
            {
                string pathhh = Application.dataPath + "/" + nameFolderICSplash + abc;
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
                    }
                }
            }
            string[] listFileinSplash = Directory.GetFiles(pathGetNameSplash);
            string[] splarr = { "", "" };
            int count = 0;
            foreach (var item in listFileinSplash)
            {
                if (!item.Contains(".meta") && item.ToLower().Contains("splash_"))
                {
                    if (item.ToLower().Contains(".png") || item.ToLower().Contains(".jpg"))
                    {
                        string splashName = Path.GetFileName(item);
                        if (!splashName.StartsWith("."))
                        {
                            if (item.ToLower().Contains("_bg."))
                            {
                                splarr[0] = splashName;
                                count++;
                            }
                            else if (item.ToLower().Contains("_logo."))
                            {
                                splarr[1] = splashName;
                                count++;
                            }
                            string pasd = "Assets" + $"/{nameFolderICSplash}/{platformName}/{splashName}";
                            TextureImporter importer = AssetImporter.GetAtPath(pasd) as TextureImporter;
                            importer.textureType = TextureImporterType.Sprite;
                            importer.isReadable = true;
                            AssetDatabase.ImportAsset(pasd, ImportAssetOptions.ForceUpdate);
                            if (count >= 2)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            string[] splashGuiid = { "", "" };
            for (int klp = 0; klp < 2; klp++)
            {
                string pathMetaSplash = Application.dataPath + $"/{nameFolderICSplash}/{platformName}/{splarr[klp]}.meta";
                string[] allLinesMeta = File.ReadAllLines(pathMetaSplash);
                foreach (var line in allLinesMeta)
                {
                    if (line.Contains("guid:"))
                    {
                        splashGuiid[klp] = line.Substring(6);
                        break;
                    }
                }
            }

            string pathPrefab = Application.dataPath + "/GamePlugin/Resources/Popup/PopupSplashLoading.prefab";
            string[] allLines = File.ReadAllLines(pathPrefab);
            bool flaggetdata = false;
            GameOb4Addrf obdata = new GameOb4Addrf();
            int countrf = 0;
            int flagAddRf = 0;
            int subflagAddRf = 0;
            for (int p = 0; p < allLines.Length; p++)
            {
                string line = allLines[p];
                if (flaggetdata)
                {
                    if (line.StartsWith("  m_Name:"))
                    {
                        flaggetdata = false;
                        subflagAddRf = 0;
                        string fileid = getValueMeta(line, "m_Name", "");
                        if (fileid.Equals("Bg"))
                        {
                            countrf++;
                            flagAddRf = 1;
                        }
                        else if (fileid.Equals("Logo"))
                        {
                            countrf++;
                            flagAddRf = 2;
                        }
                    }
                    else if (line.Contains("component"))
                    {
                        string fileid = getValueMeta(line, "component", "fileID");
                        obdata.listComponent.Add(fileid);
                    }
                }
                else
                {
                    if (line.StartsWith("  m_Component:"))
                    {
                        flaggetdata = true;
                        obdata.listComponent.Clear();
                    }
                    else
                    {
                        if (flagAddRf == 1)
                        {
                            if (line.StartsWith("--- !u!114 &"))
                            {
                                for (int i = 0; i < obdata.listComponent.Count; i++)
                                {
                                    if (line.Contains(obdata.listComponent[i]))
                                    {
                                        subflagAddRf = 1;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (subflagAddRf == 1)
                                {
                                    if (line.Contains("m_Sprite:"))
                                    {
                                        string grid = getValueMeta(line, "m_Sprite", "guid");
                                        allLines[p] = allLines[p].Replace(grid, splashGuiid[0]);
                                        Debug.Log("aaab=" + allLines[p]);
                                        subflagAddRf = 0;
                                        flagAddRf = 0;
                                        if (countrf >= 2)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else if (flagAddRf == 2)
                        {
                            if (line.StartsWith("--- !u!114 &"))
                            {
                                for (int i = 0; i < obdata.listComponent.Count; i++)
                                {
                                    if (line.Contains(obdata.listComponent[i]))
                                    {
                                        subflagAddRf = 1;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (subflagAddRf == 1)
                                {
                                    if (line.Contains("m_Sprite:"))
                                    {
                                        string grid = getValueMeta(line, "m_Sprite", "guid");
                                        allLines[p] = allLines[p].Replace(grid, splashGuiid[1]);
                                        Debug.Log("aaab=" + allLines[p]);
                                        subflagAddRf = 0;
                                        flagAddRf = 0;
                                        if (countrf >= 2)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            File.WriteAllLines(pathPrefab, allLines);
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: setSpslashLoading ex=" + ex.ToString());
        }
    }

    private void setMyopenAds(string platformName)
    {
        try
        {
            string pathGetNameSplash = Application.dataPath + $"/IconSplash{GameName}/{platformName}";
            string nameFolderICSplash;
            if (!Directory.Exists(pathGetNameSplash))
            {
                nameFolderICSplash = "IconSplash";
                pathGetNameSplash = Application.dataPath + $"/IconSplash/{platformName}";
            }
            else
            {
                nameFolderICSplash = $"IconSplash{GameName}";
            }
            string[] listFileinSplash = Directory.GetFiles(pathGetNameSplash);
            string iconName = "";
            int count = 0;
            foreach (var item in listFileinSplash)
            {
                if (!item.Contains(".meta") && (item.ToLower().Contains("app_icon") || item.ToLower().Contains("icon_ios")))
                {
                    if (item.ToLower().Contains(".png") || item.ToLower().Contains(".jpg"))
                    {
                        iconName = Path.GetFileName(item);
                        if (!iconName.StartsWith("."))
                        {
                            string pasd = "Assets" + $"/{nameFolderICSplash}/{platformName}/{iconName}";
                            TextureImporter importer = AssetImporter.GetAtPath(pasd) as TextureImporter;
                            importer.textureType = TextureImporterType.Sprite;
                            importer.isReadable = true;
                            AssetDatabase.ImportAsset(pasd, ImportAssetOptions.ForceUpdate);
                            break;
                        }
                    }
                }
            }

            string iconNameGuiid = "";
            string pathMetaSplash = Application.dataPath + $"/{nameFolderICSplash}/{platformName}/{iconName}.meta";
            string[] allLinesMeta = File.ReadAllLines(pathMetaSplash);
            foreach (var line in allLinesMeta)
            {
                if (line.Contains("guid:"))
                {
                    iconNameGuiid = line.Substring(6);
                    break;
                }
            }

            string pathPrefab = Application.dataPath + "/GamePlugin/Resources/Popup/PopupMyopenAds.prefab";
            string[] allLines = File.ReadAllLines(pathPrefab);
            bool flaggetdata = false;
            GameOb4Addrf obdata = new GameOb4Addrf();
            int countrf = 0;
            int flagAddRf = 0;
            int subflagAddRf = 0;
            for (int p = 0; p < allLines.Length; p++)
            {
                string line = allLines[p];
                if (flaggetdata)
                {
                    if (line.StartsWith("  m_Name:"))
                    {
                        flaggetdata = false;
                        subflagAddRf = 0;
                        string fileid = getValueMeta(line, "m_Name", "");
                        if (fileid.Equals("IconGame"))
                        {
                            countrf++;
                            flagAddRf = 1;
                        }
                        else if (fileid.Equals("TxtNameGame"))
                        {
                            countrf++;
                            flagAddRf = 2;
                        }
                    }
                    else if (line.Contains("component"))
                    {
                        string fileid = getValueMeta(line, "component", "fileID");
                        obdata.listComponent.Add(fileid);
                    }
                }
                else
                {
                    if (line.StartsWith("  m_Component:"))
                    {
                        flaggetdata = true;
                        obdata.listComponent.Clear();
                    }
                    else
                    {
                        if (flagAddRf == 1)
                        {
                            if (line.StartsWith("--- !u!114 &"))
                            {
                                for (int i = 0; i < obdata.listComponent.Count; i++)
                                {
                                    if (line.Contains(obdata.listComponent[i]))
                                    {
                                        subflagAddRf = 1;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (subflagAddRf == 1)
                                {
                                    if (line.Contains("m_Sprite:"))
                                    {
                                        string grid = getValueMeta(line, "m_Sprite", "guid");
                                        allLines[p] = allLines[p].Replace(grid, iconNameGuiid);
                                        Debug.Log("aaab=" + allLines[p]);
                                        subflagAddRf = 0;
                                        flagAddRf = 0;
                                        if (countrf >= 2)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else if (flagAddRf == 2)
                        {
                            if (line.StartsWith("--- !u!114 &"))
                            {
                                for (int i = 0; i < obdata.listComponent.Count; i++)
                                {
                                    if (line.Contains(obdata.listComponent[i]))
                                    {
                                        subflagAddRf = 1;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (subflagAddRf == 1)
                                {
                                    if (line.Contains("m_Text:"))
                                    {
                                        string grid = getValueMeta(line, "m_Text", "");
                                        allLines[p] = allLines[p].Replace(grid, $"'{PlayerSettings.productName}'");
                                        Debug.Log("aaab=" + PlayerSettings.productName);
                                        subflagAddRf = 0;
                                        flagAddRf = 0;
                                        if (countrf >= 2)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            File.WriteAllLines(pathPrefab, allLines);
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: setMyopenAds ex=" + ex.ToString());
        }
    }

    private string getValueMeta(string data, string key, string child)
    {
        int n = data.IndexOf(key);
        if (n > 0)
        {
            if (child == null || child.Length <= 0)
            {
                int nv = n + key.Length + 2;
                string va = data.Substring(nv, data.Length - nv);
                // Debug.Log("aaa=" + va);
                return va;
            }
            else
            {
                int nv = n + key.Length + 3;
                string va = data.Substring(nv, data.Length - nv - 1);
                // Debug.Log("aaa1=" + va);
                string[] llll = va.Split(',');
                for (int i = 0; i < llll.Length; i++)
                {
                    if (llll[i].Contains(child))
                    {
                        n = llll[i].IndexOf(child);
                        nv = n + child.Length + 2;
                        va = llll[i].Substring(nv, llll[i].Length - nv);
                        // Debug.Log("aaa1=" + va);
                        return va;
                    }
                }
            }
        }

        return "";
    }

    private void setIconIOS()
    {
#if UNITY_IOS || UNITY_IPHONE
        string pathgetname;
        string nameFolderICSplash;
        // if (Application.platform == RuntimePlatform.OSXEditor)
        {
            pathgetname = Application.dataPath + $"/IconSplash{GameName}/iOS";
            if (!Directory.Exists(pathgetname))
            {
                nameFolderICSplash = "IconSplash";
                pathgetname = Application.dataPath + $"/IconSplash/iOS";
            }
            else
            {
                nameFolderICSplash = $"IconSplash{GameName}";
            }
        }
        // else
        // {
        //     pathgetname = Application.dataPath + "\\IconSplash\\iOS";
        // }
        string[] arrname = Directory.GetFiles(pathgetname);
        string nameIcon = "";
        foreach (var item in arrname)
        {
            if (!item.Contains(".meta"))
            {
                String namefile = Path.GetFileName(item);
                if (!namefile.StartsWith(".") && namefile.ToLower().Contains("icon") && (namefile.ToLower().Contains("png") || namefile.ToLower().Contains("jpg")))
                {
                    nameIcon = Path.GetFileName(item);
                    break;
                }
            }
        }
        if (nameIcon.Length <= 1)
        {
            return;
        }
        string ppp = $"Assets/{nameFolderICSplash}/iOS/{nameIcon}";
        var tt = AssetDatabase.LoadAssetAtPath<Texture2D>(ppp);

        PlatformIcon[] Applications = PlayerSettings.GetPlatformIcons(BuildTargetGroup.iOS, iOSPlatformIconKind.Application);
        for (int i = 0; i < Applications.Length; i++)
        {
            if (Applications[i].width == 180 || Applications[i].width == 167)
            {
                Applications[i].SetTextures(new Texture2D[] { tt });
            }
        }
        PlayerSettings.SetPlatformIcons(BuildTargetGroup.iOS, iOSPlatformIconKind.Application, Applications);
        //
        PlatformIcon[] Spotlights = PlayerSettings.GetPlatformIcons(BuildTargetGroup.iOS, iOSPlatformIconKind.Spotlight);
        for (int i = 0; i < Spotlights.Length; i++)
        {
            if (Spotlights[i].width == 120 || (Spotlights[i].width == 80 && Spotlights[i].ToString().ToLower().Contains("ipad")))
            {
                Spotlights[i].SetTextures(new Texture2D[] { tt });
            }
        }
        PlayerSettings.SetPlatformIcons(BuildTargetGroup.iOS, iOSPlatformIconKind.Spotlight, Spotlights);
        //
        PlatformIcon[] Settingss = PlayerSettings.GetPlatformIcons(BuildTargetGroup.iOS, iOSPlatformIconKind.Settings);
        for (int i = 0; i < Settingss.Length; i++)
        {
            if (Settingss[i].width == 87 || (Settingss[i].width == 58 && Settingss[i].ToString().ToLower().Contains("ipad")))
            {
                Settingss[i].SetTextures(new Texture2D[] { tt });
            }
        }
        PlayerSettings.SetPlatformIcons(BuildTargetGroup.iOS, iOSPlatformIconKind.Settings, Settingss);
        //
        PlatformIcon[] Notifications = PlayerSettings.GetPlatformIcons(BuildTargetGroup.iOS, iOSPlatformIconKind.Notification);
        for (int i = 0; i < Notifications.Length; i++)
        {
            if (Notifications[i].width == 60 || (Notifications[i].width == 40 && Notifications[i].ToString().ToLower().Contains("ipad")))
            {
                Notifications[i].SetTextures(new Texture2D[] { tt });
            }
        }
        PlayerSettings.SetPlatformIcons(BuildTargetGroup.iOS, iOSPlatformIconKind.Notification, Notifications);
        //
        PlatformIcon[] Marketings = PlayerSettings.GetPlatformIcons(BuildTargetGroup.iOS, iOSPlatformIconKind.Marketing);
        for (int i = 0; i < Marketings.Length; i++)
        {
            if (Marketings[i].width == 1024)
            {
                Marketings[i].SetTextures(new Texture2D[] { tt });
            }
        }
        PlayerSettings.SetPlatformIcons(BuildTargetGroup.iOS, iOSPlatformIconKind.Marketing, Marketings);
        //
        arrname = Directory.GetFiles(pathgetname);
        string nameSplash = "";
        string nameStoryboardiPhone = "";
        string nameStoryboardiPad = "";
        foreach (var item in arrname)
        {
            if (!item.Contains(".meta"))
            {
                if (item.ToLower().Contains("splash") && (item.ToLower().Contains(".png") || item.ToLower().Contains(".jpg")))
                {
                    nameSplash = Path.GetFileName(item);
                }
                else if (item.Contains(".storyboard"))
                {
                    if (item.Contains("iPad"))
                    {
                        nameStoryboardiPad = Path.GetFileName(item);
                    }
                    else
                    {
                        nameStoryboardiPhone = Path.GetFileName(item);
                    }
                }
            }
        }
        if (nameStoryboardiPad.Length == 0)
        {
            nameStoryboardiPad = nameStoryboardiPhone;
        }
        else if (nameStoryboardiPhone.Length == 0)
        {
            nameStoryboardiPhone = nameStoryboardiPad;
        }

        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.SplashScreen.showUnityLogo = false;

        const string projectSettings = "ProjectSettings/ProjectSettings.asset";
        UnityEngine.Object obj = AssetDatabase.LoadAllAssetsAtPath(projectSettings)[0];
        SerializedObject psObj = new SerializedObject(obj);
        SerializedProperty iosUseStoryboard = psObj.FindProperty("iOSUseLaunchScreenStoryboard");
        if (iosUseStoryboard != null)
        {
            iosUseStoryboard.boolValue = true;
        }
        SerializedProperty iosPathStoryboard = psObj.FindProperty("iOSLaunchScreenCustomStoryboardPath");
        if (iosPathStoryboard != null)
        {
            iosPathStoryboard.stringValue = $"Assets/{nameFolderICSplash}/iOS/{nameStoryboardiPhone}";
        }
        SerializedProperty iosipadPathStoryboard = psObj.FindProperty("iOSLaunchScreeniPadCustomStoryboardPath");
        if (iosipadPathStoryboard != null)
        {
            iosipadPathStoryboard.stringValue = $"Assets/{nameFolderICSplash}/iOS/{nameStoryboardiPad}";
        }

        psObj.ApplyModifiedProperties();

        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.SplashScreen.showUnityLogo = false;
#endif
    }

    private void setIconAndroid()
    {
#if UNITY_ANDROID
        string pathgetname;
        string nameFolderICSplash;
        // if (Application.platform == RuntimePlatform.OSXEditor)
        {
            pathgetname = Application.dataPath + $"/IconSplash{GameName}/Android/mipmap-hdpi";
            if (!Directory.Exists(pathgetname))
            {
                nameFolderICSplash = "IconSplash";
                pathgetname = Application.dataPath + $"/IconSplash/Android/mipmap-hdpi";
            }
            else
            {
                nameFolderICSplash = $"IconSplash{GameName}";
            }
        }
        // else
        // {
        //     pathgetname = Application.dataPath + "\\IconSplash\\Android\\mipmap-hdpi";
        // }
        string[] arrname = Directory.GetFiles(pathgetname);
        string nameAdaptive = "";
        string namebg = "";
        string nameRound = "";
        string nameLegacy = "";
        foreach (var item in arrname)
        {
            if (!item.Contains(".meta"))
            {
                if (item.Contains("_foreground"))
                {
                    nameAdaptive = Path.GetFileName(item);
                }
                else if (item.Contains("_round"))
                {
                    nameRound = Path.GetFileName(item);
                }
                else if (item.Contains("_background"))
                {
                    namebg = Path.GetFileName(item);
                }
                else
                {
                    nameLegacy = Path.GetFileName(item);
                }
            }
        }
        if (nameRound.Length <= 1 || nameAdaptive.Length <= 1 || nameLegacy.Length <= 1)
        {
            return;
        }
        if (namebg.Length == 0)
        {
            namebg = nameAdaptive;
        }
        PlatformIcon[] Adaptives = PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android, AndroidPlatformIconKind.Adaptive);
        for (int i = 0; i < Adaptives.Length; i++)
        {
            string ppp = "";
            string pbg = "";
            if (Adaptives[i].width == 432)
            {
                pbg = $"Assets/{nameFolderICSplash}/Android/mipmap-xxxhdpi/{namebg}";
                ppp = $"Assets/{nameFolderICSplash}/Android/mipmap-xxxhdpi/{nameAdaptive}";
            }
            else if (Adaptives[i].width == 324)
            {
                pbg = $"Assets/{nameFolderICSplash}/Android/mipmap-xxhdpi/{namebg}";
                ppp = $"Assets/{nameFolderICSplash}/Android/mipmap-xxhdpi/{nameAdaptive}";
            }
            else if (Adaptives[i].width == 216)
            {
                pbg = $"Assets/{nameFolderICSplash}/Android/mipmap-xhdpi/{namebg}";
                ppp = $"Assets/{nameFolderICSplash}/Android/mipmap-xhdpi/{nameAdaptive}";
            }
            else if (Adaptives[i].width == 162)
            {
                pbg = $"Assets/{nameFolderICSplash}/Android/mipmap-hdpi/{namebg}";
                ppp = $"Assets/{nameFolderICSplash}/Android/mipmap-hdpi/{nameAdaptive}";
            }
            else if (Adaptives[i].width == 108)
            {
                pbg = $"Assets/{nameFolderICSplash}/Android/mipmap-mdpi/{namebg}";
                ppp = $"Assets/{nameFolderICSplash}/Android/mipmap-mdpi/{nameAdaptive}";
            }
            if (ppp.Length > 0)
            {
                var tbg = AssetDatabase.LoadAssetAtPath<Texture2D>(pbg);
                var tt = AssetDatabase.LoadAssetAtPath<Texture2D>(ppp);
                Adaptives[i].SetTextures(new Texture2D[] { tbg, tt });
            }
            else
            {
                Adaptives[i].SetTexture(null);
            }
        }
        PlayerSettings.SetPlatformIcons(BuildTargetGroup.Android, AndroidPlatformIconKind.Adaptive, Adaptives);
        //
        PlatformIcon[] Rounds = PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android, AndroidPlatformIconKind.Round);
        for (int i = 0; i < Rounds.Length; i++)
        {
            string ppp = "";
            if (Rounds[i].width == 192)
            {
                ppp = $"Assets/{nameFolderICSplash}/Android/mipmap-xxxhdpi/{nameRound}";
            }
            else if (Rounds[i].width == 144)
            {
                ppp = $"Assets/{nameFolderICSplash}/Android/mipmap-xxhdpi/{nameRound}";
            }
            else if (Rounds[i].width == 96)
            {
                ppp = $"Assets/{nameFolderICSplash}/Android/mipmap-xhdpi/{nameRound}";
            }
            else if (Rounds[i].width == 72)
            {
                ppp = $"Assets/{nameFolderICSplash}/Android/mipmap-hdpi/{nameRound}";
            }
            else if (Rounds[i].width == 48)
            {
                ppp = $"Assets/{nameFolderICSplash}/Android/mipmap-mdpi/{nameRound}";
            }
            if (ppp.Length > 0)
            {
                var tt = AssetDatabase.LoadAssetAtPath<Texture2D>(ppp);
                Rounds[i].SetTexture(tt);
            }
            else
            {
                Rounds[i].SetTexture(null);
            }
        }
        PlayerSettings.SetPlatformIcons(BuildTargetGroup.Android, AndroidPlatformIconKind.Round, Rounds);
        //
        PlatformIcon[] Legacys = PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android, AndroidPlatformIconKind.Legacy);
        for (int i = 0; i < Legacys.Length; i++)
        {
            string ppp = "";
            if (Legacys[i].width == 192)
            {
                ppp = $"Assets/{nameFolderICSplash}/Android/mipmap-xxxhdpi/{nameLegacy}";
            }
            else if (Legacys[i].width == 144)
            {
                ppp = $"Assets/{nameFolderICSplash}/Android/mipmap-xxhdpi/{nameLegacy}";
            }
            else if (Legacys[i].width == 96)
            {
                ppp = $"Assets/{nameFolderICSplash}/Android/mipmap-xhdpi/{nameLegacy}";
            }
            else if (Legacys[i].width == 72)
            {
                ppp = $"Assets/{nameFolderICSplash}/Android/mipmap-hdpi/{nameLegacy}";
            }
            else if (Legacys[i].width == 48)
            {
                ppp = $"Assets/{nameFolderICSplash}/Android/mipmap-mdpi/{nameLegacy}";
            }
            if (ppp.Length > 0)
            {
                var tt = AssetDatabase.LoadAssetAtPath<Texture2D>(ppp);
                Legacys[i].SetTexture(tt);
            }
            else
            {
                Legacys[i].SetTexture(null);
            }
        }
        PlayerSettings.SetPlatformIcons(BuildTargetGroup.Android, AndroidPlatformIconKind.Legacy, Legacys);


        // if (Application.platform == RuntimePlatform.OSXEditor)
        {
            pathgetname = Application.dataPath + $"/{nameFolderICSplash}/Android";
        }
        // else
        // {
        //     pathgetname = Application.dataPath + "\\IconSplash\\Android";
        // }
        arrname = Directory.GetFiles(pathgetname);
        string nameSplashNative = "";
        foreach (var item in arrname)
        {
            if (!item.Contains(".meta"))
            {
                if (item.ToLower().Contains("splash_native"))
                {
                    nameSplashNative = Path.GetFileName(item);
                    break;
                }
            }
        }
        if (nameSplashNative.Length > 5)
        {
            //PlayerSettings.SplashScreen.show = false;
            //PlayerSettings.SplashScreen.showUnityLogo = false;

            var companyLogo = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/{nameFolderICSplash}/Android/{nameSplashNative}");
            PlayerSettings.virtualRealitySplashScreen = companyLogo;
            PlayerSettings.Android.splashScreenScale = AndroidSplashScreenScale.ScaleToFill;

            const string projectSettings = "ProjectSettings/ProjectSettings.asset";
            UnityEngine.Object obj = AssetDatabase.LoadAllAssetsAtPath(projectSettings)[0];
            SerializedObject psObj = new SerializedObject(obj);
            SerializedProperty androidSplashFileId = psObj.FindProperty("androidSplashScreen.m_FileID");
            if (androidSplashFileId != null)
            {
                androidSplashFileId.intValue = companyLogo.GetInstanceID();
            }
            psObj.ApplyModifiedProperties();
        }
#endif
    }

    void parConfigGame(bool isTest)
    {
        PlayerSettings.stripEngineCode = false;
        PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
        PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.Full);
        PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);
        if (isTest)
        {
#if UNITY_ANDROID
            string pathFrom, pto;
            string passet = Application.dataPath;
            string ppro;
            // if (Application.platform == RuntimePlatform.OSXEditor)
            {
                ppro = passet.Replace("/Assets", "");
                pathFrom = passet + "/GamePlugin/Editor/FireBase4Test/fir_config.txt";
                pto = passet + "/GamePlugin/FirConfig/google-services.json";
            }
            // else
            // {
            //     ppro = passet.Replace("\\Assets", "");
            //     ppro = passet.Replace("/Assets", "");
            //     pathFrom = ppro + "\\Mem\\Android\\FirConfigTest\\google-services.json";
            //     pto = Application.dataPath + "\\GamePlugin\\FirConfig";
            // }

            mygame.sdk.AdsHelper adshelper = groupControl.transform.Find("AdvHelper").GetComponent<mygame.sdk.AdsHelper>();

            string pkgtest = packageNameTest;
            if (pkgtest.Length < 3)
            {
                pkgtest = "com.viet.game.app.dev";
            }
            coppyFileWithNewName(pathFrom, pto, pkgtest, true);

            pathFrom = passet + $"/GamePlugin/Editor/ForBuildAndroid_{unityver}/gradleTemplate.properties";
            pto = passet + "/Plugins/Android";
            coppyFile(pathFrom, pto, "", false);
            editGradleTemplateProperties();

            pathFrom = passet + $"/GamePlugin/Editor/ForBuildAndroid_{unityver}/launcherTemplate.gradle";
            pto = passet + "/Plugins/Android";
            coppyFile(pathFrom, pto, "", false);
            editGradleTemplateProperties();

            pathFrom = passet + $"/GamePlugin/Editor/ForBuildAndroid_{unityver}/MyGameRes.androidlib";
            pto = passet + "/Plugins/Android/MyGameRes.androidlib";
            Directory.CreateDirectory(pto);
            if (System.IO.Directory.Exists(pathFrom))
            {
                string[] files = System.IO.Directory.GetFiles(pathFrom);

                // Copy the files and overwrite destination files if they already exist.
                foreach (string s in files)
                {
                    // Use static Path methods to extract only the file name from the path.
                    var fileName = System.IO.Path.GetFileName(s);
                    var destFile = System.IO.Path.Combine(pto, fileName);
                    System.IO.File.Copy(s, destFile, true);
                }
            }

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, pkgtest);
            PlayerSettings.Android.useCustomKeystore = false;

            adshelper.AdmobAppID4Android = "ca-app-pub-3940256099942544~3347511713";
            adshelper.OpenAdIdAndroid = "ca-app-pub-3940256099942544/9257395921";

            adshelper.adsAdmobMy.android_app_id = "ca-app-pub-3940256099942544~3347511713";
            adshelper.adsAdmobMy.android_bn_id = "ca-app-pub-3940256099942544/6300978111";
            adshelper.adsAdmobMy.android_full_id = "ca-app-pub-3940256099942544/1033173712";
            adshelper.adsAdmobMy.android_gift_id = "ca-app-pub-3940256099942544/5224354917";

            adshelper.adsAdmobLower.android_app_id = "ca-app-pub-3940256099942544~3347511713";
            adshelper.adsAdmobLower.android_full_id = "ca-app-pub-3940256099942544/1033173712";

            SetAppCf("public const string AdmobPlOpenAd", "\"openad_default,-1,ca-app-pub-3940256099942544/3347511713\"", true);
            SetAppCf("public const string AdmobPlBanner", "\"bn_default,-1,ca-app-pub-3940256099942544/6300978111\"", true);
            SetAppCf("public const string AdmobPlCl", "\"cl_default,-1,ca-app-pub-3940256099942544/6300978111\"", true);
            SetAppCf("public const string AdmobPlNative", "\"nt_default,-1,ca-app-pub-3940256099942544/2247696110\"", true);
            SetAppCf("public const string AdmobPlNtFull", "\"full_default,-1,ca-app-pub-3940256099942544/2247696110\"", true);
            SetAppCf("public const string AdmobPlFullImg", "\"full_default,-1,ca-app-pub-3940256099942544/1033173712\"", true);
            SetAppCf("public const string AdmobPlFullAll", "\"full_default,-1,ca-app-pub-3940256099942544/1033173712\"", true);
            SetAppCf("public const string AdmobPlGift", "\"gift_default,-1,ca-app-pub-3940256099942544/5224354917\"", true);

            setPropertyAsset("/MaxSdk/Resources/AppLovinSettings.asset", "adMobAndroidAppId:", adshelper.AdmobAppID4Android);
            addIronConfig(adshelper.AdmobAppID4Android, true);
            setPropertyAsset("/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset", "adMobAndroidAppId:", adshelper.AdmobAppID4Android);

            SetAppCf("public const string appid", $"\"{pkgtest}\"");

            EditorUtility.SetDirty(adshelper.adsAdmobMy);
            EditorUtility.SetDirty(adshelper.adsAdmobLower);
            EditorUtility.SetDirty(adshelper);

            // PlayerSettings.Android.useCustomKeystore = true;
            // PlayerSettings.Android.keyaliasName = "stic_dra_figh_w_al";
            // PlayerSettings.Android.keystorePass = "xgamest@123";
            // PlayerSettings.Android.keyaliasPass = "xgamest@123";

            string dft = "";
            if (isTestInApp)
            {
                dft = "ENABLE_ADS_ADMOB;USE_ADSMOB_MY;ENABLE_INAPP;ENABLE_TEST_INAPP;ENABLE_MYLOG";
            }
            else
            {
                dft = "ENABLE_ADS_ADMOB;USE_ADSMOB_MY;ENABLE_MYLOG";
            }
            if (DefineSymbolAdd.Length > 2)
            {
                dft = DefineSymbolAdd + ";" + dft;
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, dft);
#endif      
        }
        else
        {
            string pathFileConfig;
            string passet = Application.dataPath;
            string ppro;
            string pathFrom, pto;
            string iconsplashPlatform, icsplFrom;
            //if (Application.platform == RuntimePlatform.OSXEditor)
            {
                ppro = passet.Replace("/Assets", "");
#if UNITY_IOS || UNITY_IPHONE
                pathFileConfig = "/iOS/GameConfigiOS.json";
                pathFrom = "/iOS/GoogleService-Info.plist";
                iconsplashPlatform = "iOS";
#else
                pathFileConfig = "/Android/GameConfigAndroid.json";
                pathFrom = "/Android/google-services.json";
                iconsplashPlatform = "Android";
#endif
                string pptt = ppro + $"/Mem{GameName}" + pathFileConfig;
                if (!File.Exists(pptt))
                {
                    pathFileConfig = ppro + $"/Mem" + pathFileConfig;
                    pathFrom = ppro + $"/Mem" + pathFrom;
                    icsplFrom = ppro + $"/Mem/IconSplash/{iconsplashPlatform}";
                }
                else
                {
                    pathFileConfig = pptt;
                    pathFrom = ppro + $"/Mem{GameName}" + pathFrom;
                    icsplFrom = ppro + $"/Mem{GameName}/IconSplash/{iconsplashPlatform}";
                }
                pto = Application.dataPath + "/GamePlugin/FirConfig";
            }
            //             else
            //             {
            //                 ppro = passet.Replace("\\Assets", "");
            //                 ppro = passet.Replace("/Assets", "");
            // #if UNITY_IOS || UNITY_IPHONE
            //                 pathFileConfig = ppro + "\\Mem\\iOS\\GameConfigiOS.json";
            //                 pathFrom = ppro + "\\Mem\\iOS\\GoogleService-Info.plist";
            // #else
            //                 pathFileConfig = ppro + "\\Mem\\Android\\GameConfigAndroid.json";
            //                 pathFrom = ppro + "\\Mem\\Android\\google-services.json";
            // #endif
            //                 pto = Application.dataPath + "\\GamePlugin\\FirConfig";
            //             }
            coppyFile(pathFrom, pto, "", false);
            coppyFolderWithChangeExt(icsplFrom, Application.dataPath + $"/IconSplash/{iconsplashPlatform}", 4, ".webp", ".png");

            pathFrom = passet + $"/GamePlugin/Editor/ForBuildAndroid_{unityver}/gradleTemplate.properties";
            pto = passet + "/Plugins/Android";
            coppyFile(pathFrom, pto, "", false);
            editGradleTemplateProperties();

            pathFrom = passet + $"/GamePlugin/Editor/ForBuildAndroid_{unityver}/launcherTemplate.gradle";
            pto = passet + "/Plugins/Android";
            coppyFile(pathFrom, pto, "", false);
            editGradleTemplateProperties();

            pathFrom = passet + $"/GamePlugin/Editor/ForBuildAndroid_{unityver}/MyGameRes.androidlib";
            pto = passet + "/Plugins/Android/MyGameRes.androidlib";
            Directory.CreateDirectory(pto);
            if (System.IO.Directory.Exists(pathFrom))
            {
                string[] files = System.IO.Directory.GetFiles(pathFrom);

                // Copy the files and overwrite destination files if they already exist.
                foreach (string s in files)
                {
                    // Use static Path methods to extract only the file name from the path.
                    var fileName = System.IO.Path.GetFileName(s);
                    var destFile = System.IO.Path.Combine(pto, fileName);
                    System.IO.File.Copy(s, destFile, true);
                }
            }

            mygame.sdk.AdsHelper adshelper = groupControl.transform.Find("AdvHelper").GetComponent<mygame.sdk.AdsHelper>();
            mygame.sdk.AdjustHelper adjustHelper = groupControl.transform.Find("AdjustHelper").GetComponent<mygame.sdk.AdjustHelper>();
            mygame.sdk.AppsFlyerHelperScript FlyerHelper = groupControl.transform.Find("AppFlyerHelper").GetComponent<mygame.sdk.AppsFlyerHelperScript>();
            adjustHelper.gameObject.SetActive(false);
            EditorUtility.SetDirty(adjustHelper);
            FlyerHelper.gameObject.SetActive(false);
            EditorUtility.SetDirty(FlyerHelper);
            // ObjectCfGame ob = new ObjectCfGame();
            string[] allLines = File.ReadAllLines(pathFileConfig);
            string datacf = "";
            foreach (var line in allLines)
            {
                datacf += line;
            }
            var dictmp = (IDictionary<string, object>)MyJson.JsonDecoder.DecodeText(datacf);
            foreach (KeyValuePair<string, object> item in dictmp)
            {
                if (item.Key.Equals("name"))
                {
                    PlayerSettings.productName = (string)item.Value;
                }
                else if (item.Key.Equals("pkg"))
                {
#if UNITY_IOS || UNITY_IPHONE
                    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, (string)item.Value);
#else
                    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, (string)item.Value);
                    SetAppCf("public const string appid", $"\"{(string)item.Value}\"");
                    string newva = $"                        string pkgCheckAndroid = \"{(string)item.Value}\";";
                    SetPropertyLine("Assets/GamePlugin/FirHelper/FIRhelper.cs", "string pkgCheckAndroid = \"", newva);
#endif
                }
                else if (item.Key.Equals("signcer"))
                {
#if UNITY_ANDROID
                    addValidSigngame((string)item.Value);
#endif
                }
                else if (item.Key.Equals("build_name"))
                {
                    PlayerSettings.bundleVersion = (string)item.Value;
                }
                else if (item.Key.Equals("build_code"))
                {
#if UNITY_IOS || UNITY_IPHONE
                    PlayerSettings.iOS.buildNumber = (string)item.Value;
#else
                    PlayerSettings.Android.bundleVersionCode = int.Parse((string)item.Value);
#endif
                }
                else if (item.Key.Equals("keystore"))
                {
                    IDictionary<string, object> keystorecf = (IDictionary<string, object>)item.Value;
                    PlayerSettings.Android.useCustomKeystore = true;
                    foreach (KeyValuePair<string, object> kcf in keystorecf)
                    {
                        if (kcf.Key.Equals("atlas"))
                        {
                            PlayerSettings.Android.keyaliasName = (string)kcf.Value;
                        }
                        else if (kcf.Key.Equals("pass"))
                        {
                            PlayerSettings.Android.keystorePass = (string)kcf.Value;
                            PlayerSettings.Android.keyaliasPass = (string)kcf.Value;
                        }
                    }
                }
                else if (item.Key.Equals("game_config"))
                {
                    IDictionary<string, object> gameCf = (IDictionary<string, object>)item.Value;
                    foreach (KeyValuePair<string, object> kcf in gameCf)
                    {
                        if (kcf.Key.Equals("GameId"))
                        {
                            SetAppCf("public const int gameID", (string)kcf.Value);
                        }
                        else if (kcf.Key.Equals("appId"))
                        {
#if UNITY_IOS || UNITY_IPHONE
                        SetAppCf("public const string appid", $"\"{(string)kcf.Value}\"");
                        FlyerHelper.appID = (string)kcf.Value;
                        if (FlyerHelper.appID == null || FlyerHelper.appID.Length < 3)
                        {
                            Debug.LogError("mysdk: config FlyerHelper errrrrrrrrrr Gameid not match");
                        }
                        EditorUtility.SetDirty(FlyerHelper);
#endif
                        }
                        else if (kcf.Key.Equals("ApplicationType"))
                        {
                            SetAppCf("public const int ApplicationType", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("DevicesTest"))
                        {
                            SetAppCf("public const string Device_test_id", $"\"{(string)kcf.Value}\"");
                        }
                        else if (kcf.Key.Equals("PerValuePostFir"))
                        {
                            SetAppCf("public const int PerValuePostFir", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("PerValuePostIAP"))
                        {
                            SetAppCf("public const int PerValuePostIAP", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("perPostNtSplash"))
                        {
                            SetAppCf("public const float PerPostNtSplash", (string)kcf.Value);
                        }
                        else if (kcf.Key.Equals("perPostNt2"))
                        {
                            SetAppCf("public const float PerPostNt2", (string)kcf.Value);
                        }
                        else if (kcf.Key.Equals("perPostFbAdRev"))
                        {
                            SetAppCf("public const int PerPostFbAdRev", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("khong_check"))
                        {
                            SetAppCf("public const int khong_check", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("urlLogEvent"))
                        {
                            SetAppCf("public const string urlLogEvent", $"\"{(string)kcf.Value}\"");
                        }
                        else if (kcf.Key.Equals("isDataEncode"))
                        {
                            SetAppCf("public const bool isDataEncode", kcf.Value.ToString().ToLower());
                        }
                        else if (kcf.Key.Equals("urlPolicy"))
                        {
                            SetAppCf("public const string urlPolicy", $"\"{(string)kcf.Value}\"");
                        }
                        else if (kcf.Key.Equals("FlagShowSplashLoading"))
                        {
                            SetAppCf("public const int FlagShowSplashLoading", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("Flag_show_langeAge"))
                        {
                            SetAppCf("public const int Flag_show_langeAge", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("isHardwareAccelerated"))
                        {
                            SetAppCf("public const bool isHardwareAccelerated", kcf.Value.ToString().ToLower());
                        }
                        else if (kcf.Key.Equals("Flag_Admob_Optimize"))
                        {
                            SetAppCf("public const int Flag_Admob_Optimize", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("Level_Show_ATT"))
                        {
                            SetAppCf("public const int LevelShowAttracking", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("WaitSplash"))
                        {
                            SetAppCf("public const int WaitSplash", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("FirstWaitSplash"))
                        {
                            SetAppCf("public const int FirstWaitSplash", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("FlagCheckLowEndDevice"))
                        {
                            SetAppCf("public const int FlagCheckLowEndDevice", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("AdmobDeltaTimeLoadNext"))
                        {
                            SetAppCf("public const int AdmobDeltaTimeLoadNext", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("AdmobLoadAll"))
                        {
                            SetAppCf("public const int AdmobLoadAll", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("AdmobTrckingTiktok"))
                        {
                            SetAppCf("public const int AdmobTrckingTiktok", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("ShowSplashFirst"))
                        {
                            SetAppCf("public const int ShowSplashFirst", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("databucket_default_log"))
                        {
                            string newva = $"            get => LogPrefs.GetBool(\"cf_enable_log_data_bucket\", {kcf.Value.ToString().ToLower()});";
                            SetPropertyLine("Assets/GamePlugin/LogManager/LogDataBucket.cs", "get => LogPrefs.GetBool(\"cf_enable_log_data_bucket\",", newva);
                            newva = $"            get => LogPrefs.GetBool(\"cf_enable_log_server\", {kcf.Value.ToString().ToLower()});";
                            SetPropertyLine("Assets/GamePlugin/LogManager/LogServer.cs", "get => LogPrefs.GetBool(\"cf_enable_log_server\",", newva);
                        }
                        else if (kcf.Key.Equals("databucket_apikey"))
                        {
                            setcfVaFor("Assets/GamePlugin/LogManager/LogDataBucket.cs", "public const string XAPIKEY", $"\"{(string)kcf.Value}\"");
                        }
                        else if (kcf.Key.Equals("MaxFlagCustomeBanner"))
                        {
                            string sva = kcf.Value.ToString();
                            string pfilebui = "Assets/GamePlugin/Editor/BuildPostProcessor.cs";
                            if (sva.CompareTo("0") == 0)
                            {
                                replaceLine(pfilebui, "#define MAX_USE_MYVIEW", "//#define MAX_USE_MYVIEW");
                                replaceLine(pfilebui, "#define MAX_USE_CONSTRAINTS", "//#define MAX_USE_CONSTRAINTS");
                            }
                            else if (sva.CompareTo("1") == 0)
                            {
                                replaceLine(pfilebui, "//#define MAX_USE_MYVIEW", "#define MAX_USE_MYVIEW");
                                replaceLine(pfilebui, "#define MAX_USE_CONSTRAINTS", "//#define MAX_USE_CONSTRAINTS");
                            }
                            else if (sva.CompareTo("2") == 0)
                            {
                                replaceLine(pfilebui, "#define MAX_USE_MYVIEW", "//#define MAX_USE_MYVIEW");
                                replaceLine(pfilebui, "//#define MAX_USE_CONSTRAINTS", "#define MAX_USE_CONSTRAINTS");
                            }
                            else if (sva.CompareTo("3") == 0)
                            {
                                replaceLine(pfilebui, "//#define MAX_USE_MYVIEW", "#define MAX_USE_MYVIEW");
                                replaceLine(pfilebui, "//#define MAX_USE_CONSTRAINTS", "#define MAX_USE_CONSTRAINTS");
                            }
                        }
                    }
                }
                else if (item.Key.Equals("iron_mediation"))
                {
                    IDictionary<string, object> IronMe = (IDictionary<string, object>)item.Value;
                    IrMediationTool.SetupMediation(IronMe);
                }
                else if (item.Key.Equals("max_mediation"))
                {
                    IDictionary<string, object> MaxMe = (IDictionary<string, object>)item.Value;
                    MaxMediationTool.SetupUnityMediation(MaxMe);
                }
                else if (item.Key.Equals("admob_mediation"))
                {
                    IDictionary<string, object> AdmobMe = (IDictionary<string, object>)item.Value;
                    if (AdmobMe.ContainsKey("VerAdmob"))
                    {
                        string vvv = AdmobMe["VerAdmob"].ToString();
                        string[] arv = vvv.Split(';');
                        if (arv.Length >= 2 && arv[1].Length > 2)
                        {
                            verAdmob = arv[1];
                        }
                    }
                    mygame.sdk.AdmobMediationTool.setupMediation(AdmobMe);
                }
                else if (item.Key.Equals("maxmy_mediation"))
                {
                    IDictionary<string, object> MaxMyMe = (IDictionary<string, object>)item.Value;
                    MaxMediationTool.SetupMediation(MaxMyMe);
                }
                else if (item.Key.Equals("default_cfads"))
                {
                    IDictionary<string, object> CfAds = (IDictionary<string, object>)item.Value;
                    foreach (KeyValuePair<string, object> kcf in CfAds)
                    {
                        if (kcf.Key.Equals("isOnlyDefault"))
                        {
                            SetAppCf("public const bool isOnlyDefault", kcf.Value.ToString().ToLower());
                        }
                        else if (kcf.Key.Equals("isBannerIpad"))
                        {
                            SetAppCf("public const bool isBannerIpad", kcf.Value.ToString().ToLower());
                        }
                        else if (kcf.Key.Equals("bn_step"))
                        {
                            SetAppCf("public const string defaultStepBanner", $"\"{(string)kcf.Value}\"");
                        }
                        else if (kcf.Key.Equals("full_step"))
                        {
                            SetAppCf("public const string defaultStepFull", $"\"{(string)kcf.Value}\"");
                        }
                        else if (kcf.Key.Equals("gift_step"))
                        {
                            SetAppCf("public const string defaultStepGift", $"\"{(string)kcf.Value}\"");
                        }
                        else if (kcf.Key.Equals("full_lv_start"))
                        {
                            SetAppCf("public const int full_lv_start", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("full_deltime"))
                        {
                            SetAppCf("public const int full_deltime", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("full2_type"))
                        {
                            SetAppCf("public const int full2_type", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("full2_lv_start"))
                        {
                            SetAppCf("public const int full2_lv_start", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("full2_deltime"))
                        {
                            SetAppCf("public const int full2_deltime", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("FlagShowRectSplash"))
                        {
                            SetAppCf("public const int FlagShowRectSplash", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("open_type"))
                        {
                            SetAppCf("public const int open_type", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("open_lv"))
                        {
                            SetAppCf("public const int open_lv", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("open_newlogic"))
                        {
                            SetAppCf("public const int open_newlogic", kcf.Value.ToString());
                        }
                        else if (kcf.Key.Equals("native_full_layout"))
                        {
                            SetAppCf("public const int native_full_layout", kcf.Value.ToString());
                        }
                    }
                }
                else if (item.Key.Equals("ads"))
                {
                    IDictionary<string, object> adscf = (IDictionary<string, object>)item.Value;
                    foreach (KeyValuePair<string, object> acf in adscf)
                    {
                        if (acf.Key.Equals("ads_max"))
                        {
#if UNITY_IOS || UNITY_IPHONE
                            adshelper.adsApplovinMaxLow.ios_bn_id = "";
                            adshelper.adsApplovinMaxLow.ios_full_id = "";
                            adshelper.adsApplovinMaxLow.ios_gift_id = "";
                            adshelper.adsApplovinMax.ios_bn_cn_id = "";
                            adshelper.adsApplovinMax.ios_full_cn_id = "";
                            adshelper.adsApplovinMax.ios_gift_cn_id = "";

                            adshelper.adsApplovinMaxMy.ios_native_id = "";
                            adshelper.adsApplovinMaxMy.ios_native_full_id = "";
#else
                            adshelper.adsApplovinMaxLow.android_bn_id = "";
                            adshelper.adsApplovinMaxLow.android_full_id = "";
                            adshelper.adsApplovinMaxLow.android_gift_id = "";

                            adshelper.adsApplovinMaxMy.android_native_id = "";
                            adshelper.adsApplovinMaxMy.android_native_full_id = "";
#endif
                            IDictionary<string, object> netadscf = (IDictionary<string, object>)acf.Value;
                            foreach (KeyValuePair<string, object> net in netadscf)
                            {
                                if (net.Key.Equals("devKey"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsApplovinMax.ios_app_id = (string)net.Value;
                                    if (adshelper.adsApplovinMaxLow != null)
                                    {
                                        adshelper.adsApplovinMaxLow.ios_app_id = (string)net.Value;
                                    }
                                    if (adshelper.adsApplovinMaxMy != null)
                                    {
                                        adshelper.adsApplovinMaxMy.ios_app_id = (string)net.Value;
                                    }
                                    setPropertyAsset("/MaxSdk/Resources/AppLovinSettings.asset", "sdkKey:", (string)net.Value);
#else
                                    adshelper.adsApplovinMax.android_app_id = (string)net.Value;
                                    if (adshelper.adsApplovinMaxLow != null)
                                    {
                                        adshelper.adsApplovinMaxLow.android_app_id = (string)net.Value;
                                    }
                                    if (adshelper.adsApplovinMaxMy != null)
                                    {
                                        adshelper.adsApplovinMaxMy.android_app_id = (string)net.Value;
                                    }
                                    setPropertyAsset("/MaxSdk/Resources/AppLovinSettings.asset", "sdkKey:", (string)net.Value);
#endif
                                }
                                else if (net.Key.Equals("banner"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsApplovinMax.ios_bn_id = (string)net.Value;
#else
                                    adshelper.adsApplovinMax.android_bn_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("full"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsApplovinMax.ios_full_id = (string)net.Value;
#else
                                    adshelper.adsApplovinMax.android_full_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("full2"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsApplovinMaxLow.ios_full_id = (string)net.Value;
#else
                                    adshelper.adsApplovinMaxLow.android_full_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("gift"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsApplovinMax.ios_gift_id = (string)net.Value;
#else
                                    adshelper.adsApplovinMax.android_gift_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("banner_low"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsApplovinMaxLow.ios_bn_id = (string)net.Value;
#else
                                    adshelper.adsApplovinMaxLow.android_bn_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("full_low"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsApplovinMaxLow.ios_full_id = (string)net.Value;
#else
                                    adshelper.adsApplovinMaxLow.android_full_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("gift_low"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsApplovinMaxLow.ios_gift_id = (string)net.Value;
#else
                                    adshelper.adsApplovinMaxLow.android_gift_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("banner_china"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsApplovinMax.ios_bn_cn_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("full_china"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsApplovinMax.ios_full_cn_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("gift_china"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsApplovinMax.ios_gift_cn_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("open"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    if (adshelper.adsApplovinMaxMy != null)
                                    {
                                        adshelper.adsApplovinMaxMy.ios_native_id = (string)net.Value;
                                    }
#else
                                    if (adshelper.adsApplovinMaxMy != null)
                                    {
                                        adshelper.adsApplovinMaxMy.android_native_id = (string)net.Value;
                                    }
#endif
                                }
                                else if (net.Key.Equals("full_nt"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    if (adshelper.adsApplovinMaxMy != null)
                                    {
                                        adshelper.adsApplovinMaxMy.ios_native_full_id = (string)net.Value;
                                    }
#else
                                    if (adshelper.adsApplovinMaxMy != null)
                                    {
                                        adshelper.adsApplovinMaxMy.android_native_full_id = (string)net.Value;
                                    }
#endif
                                }
                                else if (net.Key.Equals("amazon"))
                                {
                                    IDictionary<string, object> amazonDic = (IDictionary<string, object>)net.Value;
                                    mygame.sdk.AdsMax maxama = (mygame.sdk.AdsMax)adshelper.adsApplovinMax;
                                    foreach (KeyValuePair<string, object> amazon in amazonDic)
                                    {
                                        if (amazon.Key.Equals("appid"))
                                        {
                                            maxama.amazonAppId = (string)amazon.Value;
                                        }
                                        else if (amazon.Key.Equals("banner"))
                                        {
                                            maxama.amazonBnSlotId = (string)amazon.Value;
                                        }
                                        else if (amazon.Key.Equals("leader"))
                                        {
                                            maxama.amazonBnLeaderSlotId = (string)amazon.Value;
                                        }
                                        else if (amazon.Key.Equals("full"))
                                        {
                                            maxama.amazonInterSlotId = (string)amazon.Value;
                                        }
                                        else if (amazon.Key.Equals("gift"))
                                        {
                                            maxama.amazonVideoRewardedSlotId = (string)amazon.Value;
                                        }
                                    }
                                }
                            }
                            EditorUtility.SetDirty(adshelper.adsApplovinMax);
                            EditorUtility.SetDirty(adshelper.adsApplovinMaxLow);
                            if (adshelper.adsApplovinMaxMy != null)
                            {
                                EditorUtility.SetDirty(adshelper.adsApplovinMaxMy);
                            }
                        }
                        else if (acf.Key.Equals("ads_iron"))
                        {
                            IDictionary<string, object> netadscf = (IDictionary<string, object>)acf.Value;
                            foreach (KeyValuePair<string, object> net in netadscf)
                            {
                                if (net.Key.Equals("app_id"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsIron.ios_app_id = (string)net.Value;
                                    addIronConfig((string)net.Value, false);
#else
                                    adshelper.adsIron.android_app_id = (string)net.Value;
                                    addIronConfig((string)net.Value, false);
#endif
                                }
                                else if (net.Key.Equals("banner"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsIron.ios_bn_id = (string)net.Value;
#else
                                    adshelper.adsIron.android_bn_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("full"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsIron.ios_full_id = (string)net.Value;
#else
                                    adshelper.adsIron.android_full_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("gift"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsIron.ios_gift_id = (string)net.Value;
#else
                                    adshelper.adsIron.android_gift_id = (string)net.Value;
#endif
                                }
                            }
                            EditorUtility.SetDirty(adshelper.adsIron);
                        }
                        else if (acf.Key.Equals("ads_admob"))
                        {
                            IDictionary<string, object> netadscf = (IDictionary<string, object>)acf.Value;
                            foreach (KeyValuePair<string, object> net in netadscf)
                            {
                                if (net.Key.Equals("appid"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.AdmobAppID4iOS = (string)net.Value;
                                    adshelper.adsAdmob.ios_app_id = (string)net.Value;
                                    adshelper.adsAdmobLower.ios_app_id = (string)net.Value;
                                    adshelper.adsAdmobMy.ios_app_id = (string)net.Value;
                                    adshelper.adsAdmobMyLower.ios_app_id = (string)net.Value;
                                    adshelper.OpenAdIdiOS = "";
                                    setPropertyAsset("/MaxSdk/Resources/AppLovinSettings.asset", "adMobIosAppId:", (string)net.Value);
                                    addIronConfig(adshelper.AdmobAppID4iOS, true);
                                    setPropertyAsset("/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset", "adMobIOSAppId:", (string)net.Value);
#else
                                    adshelper.AdmobAppID4Android = (string)net.Value;
                                    adshelper.adsAdmob.android_app_id = (string)net.Value;
                                    adshelper.adsAdmobLower.android_app_id = (string)net.Value;
                                    adshelper.adsAdmobMy.android_app_id = (string)net.Value;
                                    adshelper.adsAdmobMyLower.android_app_id = (string)net.Value;
                                    adshelper.OpenAdIdAndroid = "";
                                    setPropertyAsset("/MaxSdk/Resources/AppLovinSettings.asset", "adMobAndroidAppId:", (string)net.Value);
                                    addIronConfig(adshelper.AdmobAppID4Android, true);
                                    setPropertyAsset("/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset", "adMobAndroidAppId:", (string)net.Value);
#endif
                                }
                                else if (net.Key.Equals("open"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.OpenAdIdiOS = (string)net.Value;
#else
                                    adshelper.OpenAdIdAndroid = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("immirsive"))
                                {
                                    SetAppCf("public const string AdmobImmirsive", "\"" + (string)net.Value + "\"", true);
                                }
                                else if (net.Key.Equals("placement_openad")
                                    || net.Key.Equals("placement_banner")
                                    || net.Key.Equals("placement_bnnt")
                                    || net.Key.Equals("placement_ntcl")
                                    || net.Key.Equals("placement_cl")
                                    || net.Key.Equals("placement_rect")
                                    || net.Key.Equals("placement_native")
                                    || net.Key.Equals("placement_rectnt")
                                    || net.Key.Equals("placement_ntfull")
                                    || net.Key.Equals("placement_nticfull")
                                    || net.Key.Equals("placement_ntpgfull")
                                    || net.Key.Equals("placement_mntfull")
                                    || net.Key.Equals("placement_full_img")
                                    || net.Key.Equals("placement_full")
                                    || net.Key.Equals("placement_full_rwinter")
                                    || net.Key.Equals("placement_full_rwrw")
                                    || net.Key.Equals("placement_gift")
                                    || net.Key.Equals("placement_ntgift")
                                    )
                                {
                                    string scf = "";
                                    IDictionary<string, object> cfPl = (IDictionary<string, object>)net.Value;
                                    foreach (KeyValuePair<string, object> itemcfPl in cfPl)
                                    {
                                        if (scf.Length > 0)
                                        {
                                            scf += "#" + itemcfPl.Key;
                                        }
                                        else
                                        {
                                            scf = itemcfPl.Key;
                                        }
                                        IDictionary<string, object> plcf = (IDictionary<string, object>)itemcfPl.Value;
                                        int prio = -1;
                                        if (plcf.ContainsKey("idx_high_priority"))
                                        {
                                            prio = Convert.ToInt32(plcf["idx_high_priority"]);
                                        }
                                        scf += "," + prio;
                                        scf += "," + (string)plcf["list"];
                                    }
                                    if (net.Key.Equals("placement_openad"))
                                    {
                                        SetAppCf("public const string AdmobPlOpenAd", $"\"{scf}\"", true);
                                    }
                                    else if (net.Key.Equals("placement_banner"))
                                    {
                                        SetAppCf("public const string AdmobPlBanner", $"\"{scf}\"", true);
                                    }
                                    else if (net.Key.Equals("placement_bnnt"))
                                    {
                                        SetAppCf("public const string AdmobPlBnNt", $"\"{scf}\"", true);
                                    }
                                    else if (net.Key.Equals("placement_ntcl"))
                                    {
                                        SetAppCf("public const string AdmobPlBnNativeCl", $"\"{scf}\"", true);
                                    }
                                    else if (net.Key.Equals("placement_cl"))
                                    {
                                        SetAppCf("public const string AdmobPlCl", $"\"{scf}\"", true);
                                    }
                                    else if (net.Key.Equals("placement_rect"))
                                    {
                                        SetAppCf("public const string AdmobPlRect", $"\"{scf}\"", true);
                                    }
                                    else if (net.Key.Equals("placement_native"))
                                    {
                                        SetAppCf("public const string AdmobPlNative", $"\"{scf}\"", true);
                                    }
                                    else if (net.Key.Equals("placement_rectnt"))
                                    {
                                        SetAppCf("public const string AdmobPlRectNt", $"\"{scf}\"", true);
                                    }
                                    else if (net.Key.Equals("placement_ntfull"))
                                    {
                                        SetAppCf("public const string AdmobPlNtFull", $"\"{scf}\"", true);
                                    }
                                    else if (net.Key.Equals("placement_nticfull"))
                                    {
                                        SetAppCf("public const string AdmobPlNtIcFull", $"\"{scf}\"", true);
                                    }
                                    else if (net.Key.Equals("placement_ntpgfull"))
                                    {
                                        SetAppCf("public const string AdmobPlNtPgFull", $"\"{scf}\"", true);
                                    }
                                    else if (net.Key.Equals("placement_full_img"))
                                    {
                                        SetAppCf("public const string AdmobPlFullImg", $"\"{scf}\"", true);
                                    }
                                    else if (net.Key.Equals("placement_full"))
                                    {
                                        SetAppCf("public const string AdmobPlFullAll", $"\"{scf}\"", true);
                                    }
                                    else if (net.Key.Equals("placement_full_rwinter"))
                                    {
                                        SetAppCf("public const string AdmobPlFullRwInter", $"\"{scf}\"", true);
                                    }
                                    else if (net.Key.Equals("placement_full_rwrw"))
                                    {
                                        SetAppCf("public const string AdmobPlFullRwRw", $"\"{scf}\"", true);
                                    }
                                    else if (net.Key.Equals("placement_gift"))
                                    {
                                        SetAppCf("public const string AdmobPlGift", $"\"{scf}\"", true);
                                    }
                                    else if (net.Key.Equals("placement_ntgift"))
                                    {
                                        SetAppCf("public const string AdmobPlNtGift", $"\"{scf}\"", true);
                                    }
                                }
                                EditorUtility.SetDirty(adshelper.adsAdmob);
                                EditorUtility.SetDirty(adshelper.adsAdmobLower);
                                EditorUtility.SetDirty(adshelper.adsAdmobMy);
                                EditorUtility.SetDirty(adshelper.adsAdmobMyLower);
                                EditorUtility.SetDirty(adshelper);
                            }
                        }
                        else if (acf.Key.Equals("ads_fb"))
                        {
                            IDictionary<string, object> netadscf = (IDictionary<string, object>)acf.Value;
                            foreach (KeyValuePair<string, object> net in netadscf)
                            {
                                if (net.Key.Equals("appid"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsFb.ios_app_id = (string)net.Value;
#else
                                    adshelper.adsFb.android_app_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("banner"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsFb.ios_bn_id = (string)net.Value;
#else
                                    adshelper.adsFb.android_bn_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("native"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsFb.ios_native_id = (string)net.Value;
#else
                                    adshelper.adsFb.android_native_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("native_full"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsFb.ios_native_full_id = (string)net.Value;
#else
                                    adshelper.adsFb.android_native_full_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("full"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsFb.ios_full_id = (string)net.Value;
#else
                                    adshelper.adsFb.android_full_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("gift"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsFb.ios_gift_id = (string)net.Value;
#else
                                    adshelper.adsFb.android_gift_id = (string)net.Value;
#endif
                                }
                            }
                            EditorUtility.SetDirty(adshelper.adsFb);
                        }
                        else if (acf.Key.Equals("ads_mytarget"))
                        {
                            IDictionary<string, object> netadscf = (IDictionary<string, object>)acf.Value;
                            foreach (KeyValuePair<string, object> net in netadscf)
                            {
                                if (net.Key.Equals("banner"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsMyTarget.ios_bn_id = (string)net.Value;
#else
                                    adshelper.adsMyTarget.android_bn_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("full"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsMyTarget.ios_full_id = (string)net.Value;
#else
                                    adshelper.adsMyTarget.android_full_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("rewarded"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsMyTarget.ios_gift_id = (string)net.Value;
#else
                                    adshelper.adsMyTarget.android_gift_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("bn_floor"))
                                {
                                    SetAppCf("public const string TargetBnFloor", $"\"{(string)net.Value}\"", true);
                                }
                                else if (net.Key.Equals("full_floor"))
                                {
                                    SetAppCf("public const string TargetFullFloor", $"\"{(string)net.Value}\"", true);
                                }
                                else if (net.Key.Equals("gift_floor"))
                                {
                                    SetAppCf("public const string TargetGiftFloor", $"\"{(string)net.Value}\"", true);
                                }
                            }
                            EditorUtility.SetDirty(adshelper.adsMyTarget);
                        }
                        else if (acf.Key.Equals("ads_yandex"))
                        {
                            IDictionary<string, object> netadscf = (IDictionary<string, object>)acf.Value;
                            foreach (KeyValuePair<string, object> net in netadscf)
                            {
                                if (net.Key.Equals("banner"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsMyYandex.ios_bn_id = (string)net.Value;
#else
                                    adshelper.adsMyYandex.android_bn_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("full"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsMyYandex.ios_full_id = (string)net.Value;
#else
                                    adshelper.adsMyYandex.android_full_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("rewarded"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsMyYandex.ios_gift_id = (string)net.Value;
#else
                                    adshelper.adsMyYandex.android_gift_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("bn_floor"))
                                {
                                    SetAppCf("public const string YandexBnFloor", $"\"{(string)net.Value}\"", true);
                                }
                                else if (net.Key.Equals("full_floor"))
                                {
                                    SetAppCf("public const string YandexFullFloor", $"\"{(string)net.Value}\"", true);
                                }
                                else if (net.Key.Equals("gift_floor"))
                                {
                                    SetAppCf("public const string YandexGiftFloor", $"\"{(string)net.Value}\"", true);
                                }
                            }
                            EditorUtility.SetDirty(adshelper.adsMyYandex);
                        }
                        else if (acf.Key.Equals("ads_topon"))
                        {
#if ENABLE_ADS_TOPON
                            IDictionary<string, object> netadscf = (IDictionary<string, object>)acf.Value;
                            foreach (KeyValuePair<string, object> net in netadscf)
                            {
                                if (net.Key.Equals("app_key"))
                                {

#if UNITY_IOS || UNITY_IPHONE
                                    ((mygame.sdk.AdsToponMe)adshelper.adsToponMe).appkey = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("app_id"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsMyTarget.ios_bn_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("splash"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    ((mygame.sdk.AdsToponMe)adshelper.adsToponMe).splashAdId = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("banner"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsMyTarget.ios_bn_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("full"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsMyTarget.ios_full_id = (string)net.Value;
#endif
                                }
                                else if (net.Key.Equals("rewarded"))
                                {
#if UNITY_IOS || UNITY_IPHONE
                                    adshelper.adsMyTarget.ios_gift_id = (string)net.Value;
#endif
                                }
                            }
                            EditorUtility.SetDirty(adshelper.adsMyTarget);
#endif
                        }
                    }
                }
                else if (item.Key.Equals("gadsme"))
                {
                    IDictionary<string, object> adcanvas = (IDictionary<string, object>)item.Value;
                    string apikey = "";
                    foreach (KeyValuePair<string, object> acf in adcanvas)
                    {
                        if (acf.Key.Equals("apikey"))
                        {
                            apikey = (string)acf.Value;
                        }
                    }
                    setupGadsme(apikey);
                }
                else if (item.Key.Equals("adverty"))
                {
                    IDictionary<string, object> adcanvas = (IDictionary<string, object>)item.Value;
                    foreach (KeyValuePair<string, object> acf in adcanvas)
                    {
                        if (acf.Key.Equals("apikey"))
                        {
                            setupAdverty((string)acf.Value);
                        }
                    }
                }
                else if (item.Key.Equals("adinmo"))
                {
                    IDictionary<string, object> adcanvas = (IDictionary<string, object>)item.Value;
#if UNITY_IOS || UNITY_IPHONE
                    string adobappid = adshelper.AdmobAppID4iOS;
#else
                    string adobappid = adshelper.AdmobAppID4Android;
#endif
                    setupAdinmo(adcanvas, adobappid);
                }
                else if (item.Key.Equals("bidstack"))
                {
                    IDictionary<string, object> adcanvas = (IDictionary<string, object>)item.Value;
                    string apikey = "";
                    IDictionary<string, object> listplm = null;
                    foreach (KeyValuePair<string, object> acf in adcanvas)
                    {
                        if (acf.Key.Equals("apikey"))
                        {
                            apikey = (string)acf.Value;
                        }
                        else if (acf.Key.Equals("placements"))
                        {
                            listplm = (IDictionary<string, object>)acf.Value;
                        }
                    }
                    setupBidstack(apikey, listplm);
                }
                else if (item.Key.Equals("odeeo"))
                {
                    IDictionary<string, object> adcanvas = (IDictionary<string, object>)item.Value;
                    foreach (KeyValuePair<string, object> acf in adcanvas)
                    {
                        if (acf.Key.Equals("appkey"))
                        {
                            SetAppCf("public const string OdeeoAppkey", $"\"{(string)acf.Value}\"", true);
                        }
                        else if (acf.Key.Equals("placementid"))
                        {
                            SetAppCf("public const string OdeeoPlacementId", $"\"{(string)acf.Value}\"", true);
                        }
                    }
                }
                else if (item.Key.Equals("audiomob"))
                {
                    IDictionary<string, object> adcanvas = (IDictionary<string, object>)item.Value;
                    foreach (KeyValuePair<string, object> acf in adcanvas)
                    {
                        if (acf.Key.Equals("apikey"))
                        {
                            setPropertyAsset("/Plugins/AudioMob/Resources/Settings/audiomob-settings.asset", "apiKey:", (string)acf.Value);
                        }
                    }
                }
                else if (item.Key.Equals("fb"))
                {
                    IDictionary<string, object> adjcf = (IDictionary<string, object>)item.Value;
                    foreach (KeyValuePair<string, object> ajcf in adjcf)
                    {
                        if (ajcf.Key.Equals("appid"))
                        {
                            if (((string)ajcf.Value).Length > 5)
                            {
                                addFacebookConfig("appIds:", (string)ajcf.Value);
                            }
                        }
                    }
                    EditorUtility.SetDirty(FlyerHelper);
                }
                else if (item.Key.Equals("adjust"))
                {
                    IDictionary<string, object> adjcf = (IDictionary<string, object>)item.Value;
                    foreach (KeyValuePair<string, object> ajcf in adjcf)
                    {
                        if (ajcf.Key.Equals("app_id"))
                        {
                            adjustHelper.gameObject.SetActive(((string)ajcf.Value).Length > 3);
#if UNITY_IOS || UNITY_IPHONE
                            adjustHelper.appTokeniOS = (string)ajcf.Value;
#else
                            adjustHelper.appTokenAndroid = (string)ajcf.Value;
#endif
                        }
                        else if (ajcf.Key.Equals("events"))
                        {
                            var evItems = (Dictionary<string, object>)ajcf.Value;
                            List<mygame.sdk.MyAdjustEvent> myAdjustEvents = adjustHelper.listEvent;
                            foreach (var newItem in evItems)
                            {
#if UNITY_IOS || UNITY_IPHONE
                                bool ishas = false;
                                for (int l = 0; l < myAdjustEvents.Count; l++)
                                {
                                    if (myAdjustEvents[l].name.CompareTo(newItem.Key) == 0)
                                    {
                                        ishas = true;
                                        myAdjustEvents[l].TokenIOS = (string)newItem.Value;
                                    }
                                }
                                if (!ishas)
                                {
                                    myAdjustEvents.Add(new mygame.sdk.MyAdjustEvent(newItem.Key, "", (string)newItem.Value));
                                }
#else
                                bool ishas = false;
                                for (int l = 0; l < myAdjustEvents.Count; l++)
                                {
                                    if (myAdjustEvents[l].name.CompareTo(newItem.Key) == 0)
                                    {
                                        ishas = true;
                                        myAdjustEvents[l].TokenAndroid = (string)newItem.Value;
                                        break;
                                    }
                                }
                                if (!ishas)
                                {
                                    myAdjustEvents.Add(new mygame.sdk.MyAdjustEvent(newItem.Key, (string)newItem.Value, ""));
                                }
#endif
                            }
                            adjustHelper.listEvent = myAdjustEvents;
                        }
                    }
                    EditorUtility.SetDirty(adjustHelper);
                }
                else if (item.Key.Equals("appflyer"))
                {
                    IDictionary<string, object> adjcf = (IDictionary<string, object>)item.Value;
                    foreach (KeyValuePair<string, object> ajcf in adjcf)
                    {
                        if (ajcf.Key.Equals("Dev_key"))
                        {
                            FlyerHelper.gameObject.SetActive(true);
                            FlyerHelper.getConversionData = true;
#if UNITY_IOS || UNITY_IPHONE
                            FlyerHelper.devKey = (string)ajcf.Value;
#else
                            FlyerHelper.devKey = (string)ajcf.Value;
#endif
                            customePodAppsflyeriOS();
                        }
                        else if (ajcf.Key.Equals("uwp_app_id"))
                        {
#if UNITY_IOS || UNITY_IPHONE
                            FlyerHelper.UWPAppID = (string)ajcf.Value;
#else
                            FlyerHelper.UWPAppID = (string)ajcf.Value;
#endif
                        }
                    }
                    EditorUtility.SetDirty(FlyerHelper);
                }
                else if (item.Key.Equals("define"))
                {
                    string dft = (string)item.Value;
                    if (DefineSymbolAdd.Length > 2)
                    {
                        dft = dft + ";" + DefineSymbolAdd;
                    }
                    isIronEnable = false;
                    if (dft.Contains("ENABLE_ADS_IRON"))
                    {
                        isIronEnable = true;
                    }
#if UNITY_IOS || UNITY_IPHONE
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, dft);
#else
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, dft);
#endif
                }
                else if (item.Key.Equals("base_64"))
                {
#if UNITY_ANDROID
                    addValidIapAndroid((string)item.Value);
#elif UNITY_IOS || UNITY_IPHONE
#endif
                }
            }
            SetAppCf("public const string verName", $"\"{Application.version}\"");
#if UNITY_IOS || UNITY_IPHONE
            SetAppCf("public const int verapp", PlayerSettings.iOS.buildNumber);
            SetPropertyFile("Assets/GamePlugin/GameHelper/GameHelper.cs", "private const long Day_len_Luc", "" + (mygame.sdk.SdkUtil.CurrentTimeMilis() / 1000));
#else
            SetAppCf("public const int verapp", PlayerSettings.Android.bundleVersionCode.ToString());
#endif
        }
        // return ob;
    }

    void editGradleTemplateProperties()
    {
#if UNITY_ANDROID

#if ENABLE_ADAUDIO && ENABLE_ODEEO
        string passet = Application.dataPath;
        string pto = passet + "/Plugins/Android/gradleTemplate.properties";
        string[] lines = File.ReadAllLines(pto);
        bool isw = false;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("android.enableDexingArtifactTransform=false"))
            {
                if (lines[i].StartsWith("#"))
                {
                    lines[i] = "android.enableDexingArtifactTransform=false";
                    isw = true;
                }
                break;
            }
        }
        if (isw)
        {
            File.WriteAllLines(pto, lines);
        }
#elif ENABLE_ADS_ADMOB && !USE_ADSMOB_MY && !ENABLE_ADS_IRON && !ENABLE_ADS_MAX
        string passet = Application.dataPath;
        string pto = passet + "/Plugins/Android/gradleTemplate.properties";
        string[] lines = File.ReadAllLines(pto);
        bool isw = false;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("android.enableDexingArtifactTransform=false"))
            {
                if (!lines[i].StartsWith("#"))
                {
                    lines[i] = "#android.enableDexingArtifactTransform=false";
                    isw = true;
                }
                break;
            }
        }
        if (isw)
        {
            File.WriteAllLines(pto, lines);
        }
#endif

#endif
    }

    void customePodAppsflyeriOS()
    {
        try
        {
            string passet = Application.dataPath + "/AppsFlyer/Editor/AppsFlyerAdrevenueDependencies.xml";
            if (File.Exists(passet))
            {
                var appCf = File.ReadAllLines(passet);
                bool isw = false;
                for (int i = 0; i < appCf.Length; i++)
                {
                    if (appCf[i].Contains("<iosPod name=\"AppsFlyer-AdRevenue-MoPub\""))
                    {
                        isw = true;
                        appCf[i] = "    <iosPod name=\"AppsFlyer-AdRevenue\" minTargetSdk=\"10.0\">";
                    }
                }
                if (isw)
                {
                    File.WriteAllLines(passet, appCf);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: customePodAppsflyeriOS ex=" + ex.ToString());
        }
    }

    void addValidIapAndroid(string bkey)
    {
        try
        {
            if (forceGenBaseKey)
            {
                forceGenBaseKey = false;
                string pathfile = "Assets/GamePlugin/Inapp/InappHelper.cs";
                var appCf = File.ReadAllLines(pathfile);
                string linemakey = "                byte[] makey = { 0 };";
                string linepaskey = "                int[] paskey = { 0 };";
                string linepasva = "                byte[] pasva = { 0 };";

                if (bkey != null && bkey.Length > 5)
                {
                    byte[] makey;
                    int[] paskey;
                    byte[] pasva;
                    mygame.sdk.SdkUtil.myMaHoa(bkey, out makey, out paskey, out pasva);
                    //==========================
                    linemakey = "                byte[] makey = { " + makey[0];
                    for (int j = 1; j < makey.Length; j++)
                    {
                        linemakey += ", " + makey[j];
                    }
                    linemakey += " };";
                    //==========================
                    linepaskey = "                int[] paskey = { " + paskey[0];
                    for (int j = 1; j < paskey.Length; j++)
                    {
                        linepaskey += ", " + paskey[j];
                    }
                    linepaskey += " };";
                    //==========================
                    linepasva = "                byte[] pasva = { " + pasva[0];
                    for (int j = 1; j < pasva.Length; j++)
                    {
                        linepasva += ", " + pasva[j];
                    }
                    linepasva += " };";
                }
                for (int i = 0; i < appCf.Length; i++)
                {
                    if (appCf[i].Contains("byte[] makey = {"))
                    {
                        appCf[i] = linemakey;
                    }
                    else if (appCf[i].Contains("int[] paskey = {"))
                    {
                        appCf[i] = linepaskey;
                    }
                    else if (appCf[i].Contains("byte[] pasva = {"))
                    {
                        appCf[i] = linepasva;
                    }
                }
                File.WriteAllLines(pathfile, appCf);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: addValidIapAndroid=" + ex.ToString());
        }
    }

    void addValidSigngame(string bkey)
    {
        try
        {
            if (forceGenBaseKey)
            {
                string pathfile = "Assets/GamePlugin/Platforms/Android/GameHelperAndroid.cs";
                var appCf = File.ReadAllLines(pathfile);
                string linemakey = "                    byte[] makey = { 0 };";
                string linepaskey = "                    int[] paskey = { 0 };";
                string linepasva = "                    byte[] pasva = { 0 };";

                if (bkey != null && bkey.Length > 5)
                {
                    byte[] makey;
                    int[] paskey;
                    byte[] pasva;
                    mygame.sdk.SdkUtil.myMaHoa(bkey, out makey, out paskey, out pasva);
                    //==========================
                    linemakey = "                    byte[] makey = { " + makey[0];
                    for (int j = 1; j < makey.Length; j++)
                    {
                        linemakey += ", " + makey[j];
                    }
                    linemakey += " };";
                    //==========================
                    linepaskey = "                    int[] paskey = { " + paskey[0];
                    for (int j = 1; j < paskey.Length; j++)
                    {
                        linepaskey += ", " + paskey[j];
                    }
                    linepaskey += " };";
                    //==========================
                    linepasva = "                    byte[] pasva = { " + pasva[0];
                    for (int j = 1; j < pasva.Length; j++)
                    {
                        linepasva += ", " + pasva[j];
                    }
                    linepasva += " };";
                }
                for (int i = 0; i < appCf.Length; i++)
                {
                    if (appCf[i].Contains("byte[] makey = {"))
                    {
                        appCf[i] = linemakey;
                    }
                    else if (appCf[i].Contains("int[] paskey = {"))
                    {
                        appCf[i] = linepaskey;
                    }
                    else if (appCf[i].Contains("byte[] pasva = {"))
                    {
                        appCf[i] = linepasva;
                    }
                }
                File.WriteAllLines(pathfile, appCf);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: addValidSigngame=" + ex.ToString());
        }
    }

    public static void setPropertyAsset(string path, string key, string value, string keyParent = "")
    {
        string passet = Application.dataPath + path;
        if (File.Exists(passet))
        {
            bool isw = false;
            var appCf = File.ReadAllLines(passet);
            bool isSearch = true;
            if (keyParent != null && keyParent.Length > 1)
            {
                isSearch = false;
            }
            for (int i = 0; i < appCf.Length; i++)
            {
                if (!isSearch && appCf[i].Contains(keyParent))
                {
                    isSearch = true;
                }
                if (isSearch && appCf[i].Contains(key))
                {
                    int idx = appCf[i].IndexOf(":");
                    string orkey = appCf[i].Substring(0, idx + 1);
                    isw = true;
                    appCf[i] = $"{orkey} {value}";
                    break;
                }
            }
            if (isw)
            {
                File.WriteAllLines(passet, appCf);
            }
        }
    }

    public static void setDefaultCfAd(string key, string value)
    {
        string passet = Application.dataPath + "";
        if (File.Exists(passet))
        {
            bool isw = false;
            var appCf = File.ReadAllLines(passet);
            string keyParent = "public void loadFromPlayerPrefs()";
            bool isSearch = false;
            for (int i = 0; i < appCf.Length; i++)
            {
                if (!isSearch && appCf[i].Contains(keyParent))
                {
                    isSearch = true;
                }
                if (isSearch && appCf[i].Contains(key))
                {
                    int idx = appCf[i].IndexOf(",");
                    string orkey = appCf[i].Substring(0, idx + 1);
                    isw = true;
                    appCf[i] = $"{orkey} {value});";
                    break;
                }
            }
            if (isw)
            {
                File.WriteAllLines(passet, appCf);
            }
        }
    }

    private void addIronConfig(string appKey, bool isNetSetting)
    {
        string pAssetMedia = "/IronSource/Resources/IronSourceMediationSettings.asset";
        string pAssetNet = "/IronSource/Resources/IronSourceMediatedNetworkSettings.asset";
        if (!File.Exists(Application.dataPath + pAssetMedia))
        {
            pAssetMedia = "/LevelPlay/Resources/IronSourceMediationSettings.asset";
            pAssetNet = "/LevelPlay/Resources/IronSourceMediatedNetworkSettings.asset";
        }
        if (!File.Exists(Application.dataPath + pAssetMedia))
        {
            pAssetMedia = "/LevelPlay/Resources/LevelPlayMediationSettings.asset";
            pAssetNet = "/LevelPlay/Resources/LevelPlayMediatedNetworkSettings.asset";
        }
        if (File.Exists(Application.dataPath + pAssetMedia))
        {
            if (isNetSetting)
            {
                setPropertyAsset(pAssetNet, "EnableAdmob:", "1");
#if UNITY_ANDROID
                setPropertyAsset(pAssetNet, "AdmobAndroidAppId:", appKey);
#elif UNITY_IOS || UINITY_IPHONE
                setPropertyAsset(pAssetNet, "AdmobIOSAppId:", appKey);
#endif
            }
            else
            {
#if UNITY_ANDROID
                setPropertyAsset(pAssetMedia, "AndroidAppKey:", appKey);
#elif UNITY_IOS || UINITY_IPHONE
                setPropertyAsset(pAssetMedia, "IOSAppKey:", appKey);
#endif
                setPropertyAsset(pAssetMedia, "EnableIronsourceSDKInitAPI:", "0");
                setPropertyAsset(pAssetMedia, "AddIronsourceSkadnetworkID:", "1");
                setPropertyAsset(pAssetMedia, "DeclareAD_IDPermission:", "1");
            }
        }
    }

    private void addFacebookConfig(string key, string value)
    {
        string passet = Application.dataPath + "/FacebookSDK/SDK/Resources/FacebookSettings.asset";
        if (File.Exists(passet))
        {
            bool isw = false;
            var appCf = File.ReadAllLines(passet);
            for (int i = 0; i < appCf.Length; i++)
            {
                if (appCf[i].Contains(key))
                {
                    i++;
                    isw = true;
                    appCf[i] = $"  - {value}";
                }
                else if (appCf[i].Contains("appLabels:") && isw)
                {
                    i++;
                    if (!PlayerSettings.productName.Contains(":"))
                    {
                        appCf[i] = $"  - {PlayerSettings.productName}";
                    }
                    else
                    {
                        appCf[i] = $"  - '{PlayerSettings.productName}'";
                    }
                }
            }
            if (isw)
            {
                File.WriteAllLines(passet, appCf);
            }
        }
    }

    private void editManifest4MySplash()
    {
        bool isw = false;
        string pmani = Application.dataPath + "/Plugins/Android/AndroidManifest.xml";
        string[] lines = System.IO.File.ReadAllLines(pmani);
        for (int i = 0; i < lines.Length; i++)
        {

        }
        if (isw)
        {
            File.WriteAllLines(pmani, lines);
        }
    }

    private void initLibs()
    {
        string pathSDK = groupControl.transform.GetComponent<SDKUpdate>().pathSDK;
        string verir = IrMediationTool.getVerMediation(GameName);
        DirectoryInfo d = new DirectoryInfo(pathSDK);
        DirectoryInfo pd = d.Parent;
        string pathPSdk = pd.FullName;
        if (verir != null && verir.Contains("."))
        {
            coppyFolder(pathPSdk + $"/__Libs/LevelPlay_{verir}", Application.dataPath + "/LevelPlay", 7);
        }
        string vermax = MaxMediationTool.getVerMediation(GameName);
        if (vermax != null && vermax.Contains("."))
        {
            SettingBuildAndroid.coppyFolder(pathPSdk + $"/__Libs/MaxSdk_{vermax}", Application.dataPath + "/MaxSdk", 7);
        }
    }

    private void synchAdmoveVer(string ver)
    {
#if UNITY_ANDROID
        string[] paths = {
            Application.dataPath + "/LevelPlay/Editor/ISAdMobAdapterDependencies.xml",
            Application.dataPath + "/GoogleMobileAds/Editor/GoogleMobileAdsDependencies.xml",
            Application.dataPath + "/Plugins/Android/launcherTemplate.gradle"
        };
        for (int aa = 0; aa < paths.Length; aa++)
        {
            if (File.Exists(paths[aa]))
            {
                var appCf = File.ReadAllLines(paths[aa]);
                bool iswrite = false;
                for (int i = 0; i < appCf.Length; i++)
                {
                    if (appCf[i].Contains("com.google.android.gms:play-services-ads:") && !appCf[i].Contains(ver))
                    {
                        string verc = SdkSetup.getVerFromXml(appCf[i]);
                        if (ver.Length > 0)
                        {
                            iswrite = true;
                            appCf[i] = appCf[i].Replace(verc, ver);
                        }
                    }
                }
                if (iswrite)
                {
                    File.WriteAllLines(paths[aa], appCf);
                }
            }
        }
#endif
    }

    private void setupGadsme(string apikey)
    {

    }

    private void setupAdverty(string apikey)
    {
        SetAppCf("public const string AdvertyApiKey", $"\"{apikey}\"", true);
        //setPropertyAsset("/Adverty5/Resources/Data/AdvertySettings.asset", "platform:", "0", "platformSettings:");
        setPropertyAsset("/Adverty5/Resources/Data/AdvertySettings.asset", "apiKey:", apikey, "platformSettings:");
    }

    private void setupAdinmo(IDictionary<string, object> dicAd, string admobAppId)
    {
#if ENABLE_AdInMo
        if (dicAd != null)
        {
            if (dicAd.ContainsKey("apikey"))
            {
                string gameKey = (string)dicAd["apikey"];
                if (gameKey != null && gameKey.Length > 5)
                {
                    setPropertyAsset("/Adinmo/Prefabs/AdinmoManager.prefab", "m_gameKey:", gameKey);
                    setPropertyAsset("/Adinmo/Prefabs/AdinmoManager.prefab", "applicationVersion:", Application.version);
#if UNITY_ANDROID
                    setPropertyAsset("/Adinmo/Prefabs/AdinmoManager.prefab", "m_AndroidAdMobAppId:", admobAppId);
#else
                    setPropertyAsset("/Adinmo/Prefabs/AdinmoManager.prefab", "m_iOSAdMobAppId:", admobAppId);
#endif
                }
            }
            GameObject AdInMo = GameObject.Find("AdInMo");
            if (AdInMo != null)
            {
                if (dicAd.ContainsKey("aquare"))
                {
                    AdInMo.transform.Find("BillBoard1Face").Find("Face").Find("AdinmoQuadSquare").GetComponent<Adinmo.AdinmoTexture>().m_placementKey = (string)dicAd["aquare"];
                }
                if (dicAd.ContainsKey("tv"))
                {
                    AdInMo.transform.Find("BillBoard1Video").Find("Face").Find("AdinmoQuadTV").GetComponent<Adinmo.AdinmoTexture>().m_placementKey = (string)dicAd["tv"];
                }
                if (dicAd.ContainsKey("portrait"))
                {
                    AdInMo.transform.Find("BillBoard300x600").Find("Face").Find("AdinmoQuadPortrait").GetComponent<Adinmo.AdinmoTexture>().m_placementKey = (string)dicAd["portrait"];
                }
                if (dicAd.ContainsKey("landscape"))
                {
                    AdInMo.transform.Find("BillBoard960x540").Find("Face").Find("AdinmoQuadLandscape").GetComponent<Adinmo.AdinmoTexture>().m_placementKey = (string)dicAd["landscape"];
                }
                EditorUtility.SetDirty(AdInMo);
            }
        }
#endif
    }

    private void setupBidstack(string apikey, IDictionary<string, object> placemants)
    {
        setPropertyAsset("/GamePlugin/AdCanvasHelper/AdBidstack/BidstackInGameAdSystemSettings.asset", "authKey:", apikey);
        if (placemants != null && placemants.Count > 0)
        {
            AdCanvasHelper adCanvasHelper = GadsmeObject.FindObjectOfType<AdCanvasHelper>();
            if (adCanvasHelper != null)
            {
                Transform tfgads = adCanvasHelper.transform.Find("BidstackGroup");
                if (tfgads != null)
                {
                    BidstackHelper gads = tfgads.GetComponent<BidstackHelper>();
                    gads.listPlacementsWithType.Clear();
                    foreach (KeyValuePair<string, object> arrpm in placemants)
                    {
                        List<object> listplm = (List<object>)arrpm.Value;
                        BidstackPlacementType btype = (BidstackPlacementType)Enum.Parse(typeof(BidstackPlacementType), arrpm.Key);
                        BidstackPlacementIdWithType item = new BidstackPlacementIdWithType();
                        for (int i = 0; i < listplm.Count; i++)
                        {
                            item.listPlacements.Add((string)listplm[i]);
                        }
                        item.type = btype;

                        gads.listPlacementsWithType.Add(item);
                    }
                    EditorUtility.SetDirty(tfgads);
                    EditorUtility.SetDirty(adCanvasHelper);
                }
            }
        }
    }

    public void replaceLine(string file, string oldLine, string newLine)
    {
        bool isWrite = false;
        string[] appCf = File.ReadAllLines(file);
        for (int i = 0; i < appCf.Length; i++)
        {
            if (appCf[i].CompareTo(oldLine) == 0)
            {
                isWrite = true;
                appCf[i] = newLine;
                break;
            }
        }
        if (isWrite)
        {
            File.WriteAllLines(file, appCf);
        }
    }

    public void SetAppCf(string key, string newValue, bool isAdId = false)
    {
        string platf;
#if UNITY_IOS || UNITY_IPHONE
        platf = "UNITY_IOS";
#else
        platf = "UNITY_ANDROID";
#endif
        if (!isAdId)
        {
            setcfVaFor("Assets/GamePlugin/GameManager/AppConfig.cs", key, newValue, platf);
        }
        else
        {
            setcfVaFor("Assets/GamePlugin/Ads/AdIdsConfig.cs", key, newValue, platf);
        }
    }

    public void setcfVaFor(string file, string key, string va, string keyStart = "")
    {
        bool flag = false;
        bool isWrite = false;
        string[] appCf = File.ReadAllLines(file);
        if (keyStart == null || keyStart.Length < 2)
        {
            flag = true;
        }
        for (int i = 0; i < appCf.Length; i++)
        {
            if (keyStart != null && keyStart.Length > 3 && !flag && appCf[i].Contains(keyStart))
            {
                flag = true;
            }
            else if (flag)
            {
                if (appCf[i].Contains(key))
                {
                    isWrite = true;
                    int idxc = appCf[i].IndexOf(key);
                    string nspace = new string(' ', idxc);
                    appCf[i] = $"{nspace}{key} = {va};";
                    break;
                }
            }
        }
        if (isWrite)
        {
            File.WriteAllLines(file, appCf);
        }
    }

    public void SetPropertyLine(string pathfile, string key, string newValue)
    {
        try
        {
            var appCf = File.ReadAllLines(pathfile);
            bool isWrite = false;
            for (int i = 0; i < appCf.Length; i++)
            {
                if (appCf[i].Contains(key))
                {
                    appCf[i] = newValue;
                    isWrite = true;
                    break;
                }
            }
            if (isWrite)
            {
                File.WriteAllLines(pathfile, appCf);
            }
        }
        catch (Exception ex)
        {

        }
    }

    public void SetPropertyFile(string pathfile, string key, string newValue)
    {
        try
        {
            var appCf = File.ReadAllLines(pathfile);
            bool isWrite = false;
            for (int i = 0; i < appCf.Length; i++)
            {
                if (appCf[i].Contains(key))
                {
                    bool isallow = true;
                    if (key.Contains("Day_len_Luc"))
                    {
                        int n1 = appCf[i].IndexOf('=', 10);
                        int n2 = appCf[i].IndexOf(';', n1);
                        if (n1 > 0 && n2 > 0 && n2 > n1)
                        {
                            string stm = appCf[i].Substring(n1 + 2, n2 - n1 - 2);
                            long ltm;
                            if (long.TryParse(stm, out ltm))
                            {
                                Debug.Log("mysdk: setup ios TimeSubmitReview=" + ltm);
                                long tcu = mygame.sdk.SdkUtil.CurrentTimeMilis() / 1000;
                                if ((tcu - ltm) <= 4 * 60 * 60)
                                {
                                    isallow = false;
                                }
                            }
                        }
                    }
                    if (isallow)
                    {
                        isWrite = true;
                        appCf[i] = $"        {key} = {newValue};";
                    }
                    break;
                }
            }
            if (isWrite)
            {
                File.WriteAllLines(pathfile, appCf);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: SetPropertyFile=" + ex.ToString());
        }
    }

    private void GetSupprtFlag()
    {
        try
        {
            string levelDirectoryPathResFlag;
            string levelDirectoryPathFlagmanager;
            // if (Application.platform == RuntimePlatform.OSXEditor)
            {
                levelDirectoryPathResFlag = Application.dataPath + "/GamePlugin/Resources/Flags/";
                levelDirectoryPathFlagmanager = Application.dataPath + "/GamePlugin/FlagManager/FlagManger.cs";
            }
            // else
            // {
            //     levelDirectoryPathResFlag = Application.dataPath + "\\GamePlugin\\Resources\\Flags\\";
            //     levelDirectoryPathFlagmanager = Application.dataPath + "\\GamePlugin\\FlagManager\\FlagManger.cs";
            // }
            DirectoryInfo d = new DirectoryInfo(levelDirectoryPathResFlag);//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles(); //Getting Text files
            string str = "                    string[] arrflags = {";
            bool isbegin = true;
            foreach (FileInfo file in Files)
            {
                if (!file.Name.Contains(".meta"))
                {
                    int idx = file.Name.IndexOf(".");
                    if (idx > 0)
                    {
                        string code = file.Name.Substring(0, idx);
                        if (code != null && code.Length >= 2 && code.Length <= 3)
                        {
                            if (isbegin)
                            {
                                isbegin = false;
                                str += ("\"" + code + "\"");
                            }
                            else
                            {
                                str += (", \"" + code + "\"");
                            }
                        }
                    }
                }
            }
            str += "};";

            string[] lines = System.IO.File.ReadAllLines(levelDirectoryPathFlagmanager);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("string[] arrflags"))
                {
                    lines[i] = str;
                }
            }
            System.IO.File.WriteAllLines(levelDirectoryPathFlagmanager, lines);
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: GetSupprtFlag ex=" + ex.ToString());
        }
    }

    private void WiteStringKey()
    {
        try
        {
            string levelDirectoryPath;
            // if (Application.platform == RuntimePlatform.OSXEditor)
            {
                levelDirectoryPath = Application.dataPath + "/Plugins/Android/MyGameRes.androidlib/Assets/";
            }
            // else
            // {
            //     levelDirectoryPath = Application.dataPath + "\\Plugins\\Android\\Assets\\";
            // }
            Directory.CreateDirectory(levelDirectoryPath);
            string path = levelDirectoryPath + "sowe.dat";
            string[] arrstr = {
                "http",
                "fb:",
                "pkg:",
                "https://play.google.com/store/apps/details?id=",
                "play.google.com/store/apps/details?id=",
                "market://details?id=",
                "https://www.youtube.com/watch?v=",
                "vnd.youtube:"
            };
            int sizrbyte = 1;
            for (int i = 0; i < arrstr.Length; i++)
            {
                sizrbyte += (2 * arrstr[i].Length + 1);
            }
            byte[] bytesadid = new byte[sizrbyte];
            bytesadid[0] = (byte)arrstr.Length;
            int idx = 1;
            for (int i = 0; i < arrstr.Length; i++)
            {
                bytesadid[idx] = (byte)(2 * arrstr[i].Length);
                idx++;
                writestringtobytes(arrstr[i], bytesadid, idx);
                idx += 2 * arrstr[i].Length;
            }
            System.IO.File.WriteAllBytes(path, bytesadid);
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: WiteStringKey ex=" + ex.ToString());
        }
    }

    private void writestringtobytes(string txt, byte[] arr, int idxofarr)
    {
        for (int j = 0; j < txt.Length; j++)
        {
            int va = (byte)txt[j] - 15;
            int va1 = UnityEngine.Random.Range(0, 200);
            arr[idxofarr + 2 * j] = (byte)va1;
            arr[idxofarr + 2 * j + 1] = (byte)va;
        }
    }

    public static void coppyFileNormal(string from, string to)
    {
        try
        {
            byte[] alldata = File.ReadAllBytes(from);
            File.WriteAllBytes(to, alldata);
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: coppyFileNormal ex=" + ex.ToString());
        }
    }

    public static void coppyFolderWithChangeExt(string from, string to, int deep, string oldExt, string newExt)
    {
        if (Directory.Exists(from))
        {
            if (!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }
            string[] listfiles = Directory.GetFiles(from);
            if (listfiles != null && listfiles.Length > 0)
            {
                if (listfiles != null && listfiles.Length > 0)
                {
                    for (int i = 0; i < listfiles.Length; i++)
                    {
                        string namefile = Path.GetFileName(listfiles[i]);
                        string pto;
                        if (oldExt != null && oldExt.Length > 0 && newExt != null && newExt.Length > 0 && namefile.EndsWith(oldExt))
                        {
                            namefile = namefile.Replace(oldExt, newExt);
                            pto = to + "/" + namefile;
                        }
                        else
                        {
                            pto = to + "/" + namefile;
                        }
                        coppyFileNormal(listfiles[i], pto);
                    }
                }
            }
            if (deep <= 100)
            {
                string[] listd = Directory.GetDirectories(from);
                if (listd != null && listd.Length > 0)
                {
                    for (int i = 0; i < listd.Length; i++)
                    {
                        string namedir = Path.GetFileName(listd[i]);
                        string dto = to + "/" + namedir;
                        coppyFolderWithChangeExt(listd[i], dto, deep + 1, oldExt, newExt);
                    }
                }
            }
        }
    }

    public static void coppyFolder(string from, string to, int deep, string excluseExt = "", string changeExcluseExt = "")
    {
        if (Directory.Exists(from))
        {
            if (!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }
            string[] listfiles = Directory.GetFiles(from);
            if (listfiles != null && listfiles.Length > 0)
            {
                if (listfiles != null && listfiles.Length > 0)
                {
                    for (int i = 0; i < listfiles.Length; i++)
                    {
                        string namefile = Path.GetFileName(listfiles[i]);
                        if (!namefile.StartsWith(".") && (excluseExt == null || excluseExt.Length <= 0 || !namefile.EndsWith(excluseExt)))
                        {
                            string pto = to + "/" + namefile;
                            if (changeExcluseExt != null && changeExcluseExt.Length > 1)
                            {
                                string extf = Path.GetExtension(listfiles[i]);
                                string newextf = extf.Replace(changeExcluseExt, "");
                                namefile = namefile.Replace(extf, newextf);
                                pto = to + "/" + namefile;
                            }
                            else
                            {
                                pto = to + "/" + namefile;
                            }
                            SettingBuildAndroid.coppyFileNormal(listfiles[i], pto);
                        }
                    }
                }
            }
            if (deep <= 100)
            {
                string[] listd = Directory.GetDirectories(from);
                if (listd != null && listd.Length > 0)
                {
                    for (int i = 0; i < listd.Length; i++)
                    {
                        string namedir = Path.GetFileName(listd[i]);
                        string dto = to + "/" + namedir;
                        coppyFolder(listd[i], dto, deep + 1, excluseExt, changeExcluseExt);
                    }
                }
            }
        }
    }

    public static void coppyRes2Assets(string GameName)
    {
        string passet = Application.dataPath;
        string ppro = passet.Replace("/Assets", "");
        string pathRes = ppro + $"/Mem{GameName}/Res";
        if (!Directory.Exists(pathRes))
        {
            return;
        }
        string[] listfiles = Directory.GetFiles(pathRes);
        if (listfiles != null)
        {
            for (int i = 0; i < listfiles.Length; i++)
            {
                string namefile = Path.GetFileName(listfiles[i]);
                string pto = passet + "/" + namefile;
                coppyFileNormal(listfiles[i], pto);
            }
        }
        string[] listd = Directory.GetDirectories(pathRes);
        if (listd != null && listd.Length > 0)
        {
            for (int i = 0; i < listd.Length; i++)
            {
                string namedir = Path.GetFileName(listd[i]);
                if (namedir.CompareTo("Android") != 0 && namedir.CompareTo("iOS") != 0)
                {
                    string dto = passet + "/" + namedir;
                    coppyFolder(listd[i], dto, 1);
                }
            }
        }
        string cplatform;
#if UNITY_ANDROID
        cplatform = pathRes + "/Android";
#else
        cplatform = pathRes + "/iOS";
#endif
        if (Directory.Exists(cplatform))
        {
            string[] listfilespl = Directory.GetFiles(cplatform);
            if (listfilespl != null)
            {
                for (int i = 0; i < listfilespl.Length; i++)
                {
                    string namefile = Path.GetFileName(listfilespl[i]);
                    string pto = passet + "/" + namefile;
                    coppyFileNormal(listfilespl[i], pto);
                }
            }
            string[] listdpl = Directory.GetDirectories(cplatform);
            if (listdpl != null && listdpl.Length > 0)
            {
                for (int i = 0; i < listdpl.Length; i++)
                {
                    string namedir = Path.GetFileName(listdpl[i]);
                    string dto = passet + "/" + namedir;
                    coppyFolder(listdpl[i], dto, 1);
                }
            }
        }
    }

    public static void backupGamePlugin(string GameName)
    {
        string passet = Application.dataPath;
        string ppro = passet.Replace("/Assets", "");
        string pathRes = ppro + $"/Mem{GameName}/Res";
        coppyFolder(passet + "/GamePlugin/Resources/Inapp", pathRes + "/GamePlugin/Resources/Inapp", 1);
        coppyFolder(passet + "/GamePlugin/Resources/Langs", pathRes + "/GamePlugin/Resources/Langs", 1);

        if (File.Exists(passet + "/GamePlugin/AdCanvasHelper/AdCanvasHelper.prefab"))
        {
            if (!Directory.Exists(pathRes + "/GamePlugin/AdCanvasHelper"))
            {
                Directory.CreateDirectory(pathRes + "/GamePlugin/AdCanvasHelper");
            }
            coppyFileNormal(passet + "/GamePlugin/AdCanvasHelper/AdCanvasHelper.prefab", pathRes + "/GamePlugin/AdCanvasHelper/AdCanvasHelper.prefab");
        }
        if (File.Exists(passet + "/GamePlugin/AdCanvasHelper/Res/ttDefault.jpg"))
        {
            if (!Directory.Exists(pathRes + "/GamePlugin/AdCanvasHelper/Res"))
            {
                Directory.CreateDirectory(pathRes + "/GamePlugin/AdCanvasHelper/Res");
            }
            coppyFileNormal(passet + "/GamePlugin/AdCanvasHelper/Res/ttDefault.jpg", pathRes + "/GamePlugin/AdCanvasHelper/Res/ttDefault.jpg");
        }
    }

    public static void coppyFile(string from, string to, string pkg, bool isTest)
    {
        try
        {
            string fname = Path.GetFileName(from);
            byte[] alldata = File.ReadAllBytes(from);
            string pto = to + "/" + fname;
            File.WriteAllBytes(pto, alldata);
            if (isTest)
            {
                string[] lines = File.ReadAllLines(pto);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("\"package_name\":"))
                    {
                        lines[i] = $"          \"package_name\": \"{pkg}\"";
                        break;
                    }
                }
                File.WriteAllLines(pto, lines);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: coppyFile ex=" + ex.ToString());
        }
    }

    public static void coppyFileWithNewName(string from, string to, string pkg, bool isTest)
    {
        try
        {
            byte[] alldata = File.ReadAllBytes(from);
            File.WriteAllBytes(to, alldata);
            if (isTest)
            {
                string[] lines = File.ReadAllLines(to);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("\"package_name\":"))
                    {
                        lines[i] = $"          \"package_name\": \"{pkg}\"";
                    }
                    else if (lines[i].Contains("\"bundle_id\":"))
                    {
                        lines[i] = $"                \"bundle_id\": \"{pkg}\"";
                        break;
                    }
                }
                File.WriteAllLines(to, lines);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: coppyFileWithNewName ex=" + ex.ToString());
        }
    }
}

public class SdkSetup
{
    public static void SetupSdk(mygame.sdk.SDKManager sdkManager, mygame.sdk.AdsHelper adsHelper, bool istest)
    {
        if (SettingBuildAndroid.defaultOrientation.Contains("0") || SettingBuildAndroid.defaultOrientation.Contains("1") || SettingBuildAndroid.defaultOrientation.Contains("4"))
        {
            sdkManager.OpenAdOrientation = mygame.sdk.App_Open_ad_Orien.Orien_Portraid;
        }
        else
        {
            sdkManager.OpenAdOrientation = mygame.sdk.App_Open_ad_Orien.Orien_Landscape;
        }
        EditorUtility.SetDirty(sdkManager);
#if ENABLE_AdInMo
        addAdinmo(sdkManager);
#endif
        setAdCanvasSandbox(sdkManager.isAdCanvasSandbox);
#if UNITY_ANDROID
        doSettingAppOpenAdAndroid(sdkManager, adsHelper);
#if ENABLE_ADS_IRON
        ads.myeditor.IronYandexAdsUtil.addRemoveAds("");
#endif
        doAddGradle((mygame.sdk.AppType)AppConfig.ApplicationType, adsHelper);
#endif
#if UNITY_IOS || UNITY_IPHONE
        doSettingiOS(sdkManager, adsHelper);
        MyTargetPostProcess.enableOrDisableMytargetios();
#if ENABLE_ADS_IRON
        ads.myeditor.IronYandexAdsUtil.addRemoveAds("");
#endif
#endif

#if !USE_ADMOB_MEDIATON
        mygame.sdk.AdmobMediationTool.clear();
#endif

#if !USE_MYMAX_MEDIATON
        MaxMediationTool.clear();
#endif
    }

    private static void addAdinmo(mygame.sdk.SDKManager sdkManager)
    {
#if ENABLE_AdInMo
        GameObject AdinmoManager = GameObject.Find("AdinmoManager");
        if (AdinmoManager == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Adinmo/Prefabs/AdinmoManager.prefab");
            AdinmoManager = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            AdinmoManager.transform.parent = null;
        }
#endif
    }

    private static void setAdCanvasSandbox(bool isSanbox)
    {
        string value;
        if (isSanbox)
        {
            value = "1";
        }
        else
        {
            value = "0";
        }
        SettingBuildAndroid.setPropertyAsset("/Resources/GadsmePreferences.asset", "forceSandbox:", value);
        SettingBuildAndroid.setPropertyAsset("/Resources/GadsmePreferences.asset", "showLoadingDistance:", value);
        SettingBuildAndroid.setPropertyAsset("/Resources/GadsmePreferences.asset", "showVisibilityRays:", value);
        SettingBuildAndroid.setPropertyAsset("/Resources/GadsmePreferences.asset", "debugRenderingEngine:", value);
        //adverty
        SettingBuildAndroid.setPropertyAsset("/Adverty5/Resources/Data/AdvertySettings.asset", "sandboxMode:", value);
        //audiomob
        SettingBuildAndroid.setPropertyAsset("/Plugins/AudioMob/Resources/Settings/audiomob-settings.asset", "forceTestAds:", value);
    }
    private static void doSettingAppOpenAdAndroid(mygame.sdk.SDKManager sdkManager, mygame.sdk.AdsHelper adsHelper, bool istest = false)
    {
#if UNITY_EDITOR && UNITY_ANDROID
        try
        {
            string levelDirectoryPath;
            // if (Application.platform == RuntimePlatform.OSXEditor)
            {
                levelDirectoryPath = Application.dataPath + "/Plugins/Android/MyGameRes.androidlib/Assets/";
            }
            // else
            // {
            //     levelDirectoryPath = Application.dataPath + "\\Plugins\\Android\\Assets\\";
            // }
            Directory.CreateDirectory(levelDirectoryPath);
            string path = levelDirectoryPath + "appopenad.dat";

            char[] charid = adsHelper.OpenAdIdAndroid.Trim().ToCharArray();
            byte[] bytesadid = new byte[5 + charid.Length];

            bytesadid[0] = (byte)sdkManager.OpenAdOrientation;
            bytesadid[1] = 2;
            bytesadid[2] = 3;
            bytesadid[3] = 0;
            bytesadid[4] = 30;

            for (int i = 0; i < charid.Length; i++)
            {
                bytesadid[5 + i] = (byte)charid[i];
            }
            System.IO.File.WriteAllBytes(path, bytesadid);

            //---------com.unity3d.player.UnityPlayerActivity
            path = levelDirectoryPath + "unityname.dat";
            string nameuac = "com.google.firebase.MessagingUnityPlayerActivity";
            char[] arrnamuac = nameuac.ToCharArray();
            byte[] banamuc = new byte[arrnamuac.Length];
            for (int i = 0; i < arrnamuac.Length; i++)
            {
                banamuc[i] = (byte)arrnamuac[i];
            }
            System.IO.File.WriteAllBytes(path, banamuc);
            //==================
            SettingBuildAndroid.coppyFileNormal(Application.dataPath + "/IconSplash/Android/splash_native.jpg", levelDirectoryPath + "splash_native.jpg");


            // if (Application.platform == RuntimePlatform.OSXEditor)
            {
                levelDirectoryPath = Application.dataPath + "/Plugins/Android/";
            }
            // else
            // {
            //     levelDirectoryPath = Application.dataPath + "\\Plugins\\Android\\";
            // }

            if (!adsHelper.AdmobAppID4Android.StartsWith("ca-app-pub-"))
            {
                Debug.LogError("mysdk: -------Error Android Adnob AppId");
            }
            path = levelDirectoryPath + "AndroidManifest.xml";
            string[] lines = System.IO.File.ReadAllLines(path);
            int statusAddAdmobAppId = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("ca-app-pub-"))
                {
                    string admobappid = "\"" + adsHelper.AdmobAppID4Android.Trim();
                    if (lines[i].Contains(admobappid))
                    {
                        statusAddAdmobAppId = 1;
                    }
                    else
                    {
                        statusAddAdmobAppId = 2;
                    }
                    break;
                }
            }
            List<string> allline = new List<string>();
            int idxaddAD_ID = -1;
            int idxAdd4Amazon = -1;
            int has4amazon = 0;
            int flagExport = 0;
            int flagAddPFLASHLIGHT = 1;
            mygame.sdk.AppType appType = (mygame.sdk.AppType)AppConfig.ApplicationType;
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].Contains("</application>"))
                {
                    if (lines[i].Contains("<application"))
                    {
                        string lapp = addApplication(appType, lines[i]);
                        allline.Add(lapp);
                    }
                    else if (lines[i].Contains("ca-app-pub-") && statusAddAdmobAppId == 2)
                    {
                        allline.Add($"    <meta-data android:name=\"com.google.android.gms.ads.APPLICATION_ID\" android:value=\"{adsHelper.AdmobAppID4Android.Trim()}\" />");
                    }
                    else
                    {
                        if (lines[i].Contains("uses-permission"))
                        {
                            if (lines[i].Contains("com.google.android.gms.ads.permission.AD_ID"))
                            {
                                idxaddAD_ID = -2;
                                idxAdd4Amazon = allline.Count + 1;
                            }
                            else
                            {
                                if (idxaddAD_ID != -2)
                                {
                                    idxaddAD_ID = allline.Count;
                                }
                            }
                            if (lines[i].Contains("android.permission.ACCESS_FINE_LOCATION"))
                            {
                                has4amazon |= 2;
                            }
                            if (lines[i].Contains("android.permission.ACCESS_COARSE_LOCATION"))
                            {
                                has4amazon |= 1;
                            }
                        }
                        else if (lines[i].Contains("<permission"))
                        {
                            if (lines[i].Contains("android.permission.FLASHLIGHT"))
                            {
                                flagAddPFLASHLIGHT = 0;
                            }
                        }
                        else if (lines[i].Contains("<activity")
                            || lines[i].Contains("<receiver")
                            || lines[i].Contains("<service")
                            )
                        {
                            if (lines[i].EndsWith(">"))
                            {
                                if (!lines[i].Contains("android:exported="))
                                {
                                    if (lines[i].EndsWith(" />"))
                                    {
                                        lines[i] = lines[i].Replace("/>", "android:exported=\"true\" />");
                                    }
                                    else if (lines[i].EndsWith("/>"))
                                    {
                                        lines[i] = lines[i].Replace("/>", " android:exported=\"true\" />");
                                    }
                                    else
                                    {
                                        lines[i] = lines[i].Replace(">", " android:exported=\"true\">");
                                    }
                                }
                            }
                            else
                            {
                                flagExport = 1;
                            }
                        }
                        else if (lines[i].Contains("</manifest>"))
                        {
#if ENABLE_Permission_flash
                            if (flagAddPFLASHLIGHT == 1)
                            {
                                allline.Insert(idxaddAD_ID, $"    <permission android:name=\"android.permission.FLASHLIGHT\" android:protectionLevel=\"normal\" />");
                            }
#endif
                        }
                        if (flagExport == 1)
                        {
                            if (lines[i].Contains("android:exported="))
                            {
                                flagExport = 0;
                            }
                            if (lines[i].EndsWith(">") && flagExport == 1)
                            {
                                if (lines[i].EndsWith(" />"))
                                {
                                    lines[i] = lines[i].Replace("/>", "android:exported=\"true\" />");
                                }
                                else if (lines[i].EndsWith("/>"))
                                {
                                    lines[i] = lines[i].Replace("/>", " android:exported=\"true\" />");
                                }
                                else
                                {
                                    lines[i] = lines[i].Replace(">", " android:exported=\"true\">");
                                }
                                flagExport = 0;
                            }
                        }
                        allline.Add(lines[i]);
                    }
                }
                else
                {
                    if (statusAddAdmobAppId == 0)
                    {
                        allline.Add($"    <meta-data android:name=\"com.google.android.gms.ads.APPLICATION_ID\" android:value=\"{adsHelper.AdmobAppID4Android.Trim()}\" />");
                    }
                    allline.Add(lines[i]);
                    if (idxaddAD_ID == -1)
                    {
                        idxaddAD_ID = allline.Count;
                        idxAdd4Amazon = idxaddAD_ID;
                    }
                }
            }

            if (idxaddAD_ID >= 0)
            {
                allline.Insert(idxaddAD_ID, $"    <uses-permission android:name=\"com.google.android.gms.ads.permission.AD_ID\" />");
                idxAdd4Amazon = idxaddAD_ID + 1;
            }
#if ENABLE_ADS_AMAZON
            if (has4amazon == 3)
            {
                idxAdd4Amazon = -1;
            }
            if (idxAdd4Amazon >= 0)
            {
                if ((has4amazon & 2) == 0)
                {
                    //allline.Insert(idxAdd4Amazon, $"  <uses-permission android:name=\"android.permission.ACCESS_FINE_LOCATION\" />");
                }
                if ((has4amazon & 1) == 0)
                {
                    //allline.Insert(idxAdd4Amazon, $"  <uses-permission android:name=\"android.permission.ACCESS_COARSE_LOCATION\" />");
                }
            }
#endif
            int idxinsertsplashactivity = -1;
            bool isMainSplash = false;
            for (int i = 0; i < allline.Count; i++)
            {
                if (allline[i].Contains("<activity"))
                {
                    if (allline[i].Contains("UnityPlayerActivity"))
                    {
                        idxinsertsplashactivity = i;
                        int idxinsert = -1;
                        bool isMain = false;
                        for (int j = (i + 1); j < allline.Count; j++)
                        {
                            if (sdkManager.IndexSceneLoading == -10)
                            {
                                if (allline[j].Contains("<intent-filter>"))
                                {
                                    for (int k = j; k < allline.Count; k++)
                                    {
                                        string lll = allline[k];
                                        allline.RemoveAt(k);
                                        k--;
                                        if (lll.Contains("</intent-filter>"))
                                        {
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                if (allline[j].Contains("<intent-filter>"))
                                {
                                    isMain = true;
                                }
                                else if (allline[j].Contains("<meta-data"))
                                {
                                    idxinsert = j;
                                }
                            }
                            if (allline[j].Contains("</activity>"))
                            {
                                if (sdkManager.IndexSceneLoading != -10)
                                {
                                    if (!isMain && idxinsert >= 0)
                                    {
                                        allline.Insert(idxinsert, "      <intent-filter>");
                                        allline.Insert(idxinsert + 1, "        <action android:name=\"android.intent.action.MAIN\" />");
                                        allline.Insert(idxinsert + 2, "        <category android:name=\"android.intent.category.LAUNCHER\" />");
                                        allline.Insert(idxinsert + 3, "      </intent-filter>");
                                    }
                                }
                                break;
                            }
                        }
                    }
                    else if (allline[i].Contains("SplashActivity"))
                    {
                        isMainSplash = true;
                        if (sdkManager.IndexSceneLoading == -10)
                        {
                            break;
                        }
                        else
                        {
                            for (int j = i; j < allline.Count; j++)
                            {
                                string lll = allline[j];
                                allline.RemoveAt(j);
                                j--;
                                if (lll.Contains("</activity>"))
                                {
                                    break;
                                }
                            }
                            i--;
                        }
                    }
                }
            }
            if (sdkManager.IndexSceneLoading == -10)
            {
                if (!isMainSplash && idxinsertsplashactivity >= 0)
                {
                    allline.Insert(idxinsertsplashactivity + 0, "    <activity android:name=\"mygame.plugin.activity.SplashActivity\" android:configChanges=\"fontScale | keyboard | keyboardHidden | locale | mnc | mcc | navigation | orientation | screenLayout | screenSize | smallestScreenSize | uiMode | touchscreen\" android:exported=\"true\">");
                    allline.Insert(idxinsertsplashactivity + 1, "      <intent-filter>");
                    allline.Insert(idxinsertsplashactivity + 2, "        <action android:name=\"android.intent.action.MAIN\" />");
                    allline.Insert(idxinsertsplashactivity + 3, "        <category android:name=\"android.intent.category.LAUNCHER\" />");
                    allline.Insert(idxinsertsplashactivity + 4, "      </intent-filter>");
                    allline.Insert(idxinsertsplashactivity + 5, "    </activity>");
                }
            }

            System.IO.File.WriteAllLines(path, allline);
            allline.Clear();

            ModifyUnityAndroidAppManifestSample.modifyManifest(path);
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: doSettingAppOpenAdAndroid ex=" + ex.ToString());
        }
#endif
    }

    private static void addOrRemvoeAdFly(string stateAds)
    {
        try
        {
            string[] arrsss = stateAds.Split(';');
            int isAdd = 0;
            string ver = "";
            if (arrsss.Length == 2)
            {
                isAdd = int.Parse(arrsss[0]);
                ver = arrsss[1];
            }
            string pathdir = Application.dataPath + "/MaxSdk/Mediation/AdFly/Editor";
            if (isAdd == 1)
            {
                string pathmax = Application.dataPath + "/MaxSdk/AppLovin.meta";
                if (File.Exists(pathmax))
                {
                    Directory.CreateDirectory(pathdir);
                    string pathfile = pathdir + "/Dependencies.xml";
                    List<string> allline = new List<string>();
                    allline.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    allline.Add("<dependencies>");
                    allline.Add("    <androidPackages>");
                    allline.Add($"        <androidPackage spec=\"pub.adfly:adapter-max:{ver}\">");
                    allline.Add("            <repositories>");
                    allline.Add("                <repository>https://repo1.maven.org/maven2/</repository>");
                    allline.Add("            </repositories>");
                    allline.Add("        </androidPackage>");
                    allline.Add("    </androidPackages>");
                    allline.Add("</dependencies>");

                    System.IO.File.WriteAllLines(pathfile, allline);
                }
            }
            else
            {
                string pathfile = pathdir + "/Dependencies.xml";
                if (File.Exists(pathfile))
                {
                    File.Delete(pathfile);
                    pathfile = pathdir + "/Dependencies.xml.meta";
                    File.Delete(pathfile);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: addOrRemvoeAdFly ex=" + ex.ToString());
        }
    }

    //flag = 0-single, 1-mutil, 2-extraMutil
    private static string addApplication(mygame.sdk.AppType flag, string line)
    {
        string re = "";

        string nameapp = "";
        bool isExral = false;
        if (flag == mygame.sdk.AppType.Application)
        {
            nameapp = "\"mygame.plugin.app.PluginApp\"";
        }
        else if (flag == mygame.sdk.AppType.MultiApplication)
        {
            nameapp = "\"mygame.plugin.app.PluginAppMulti\"";
        }
        else if (flag == mygame.sdk.AppType.MultiApplicationStrictMode)
        {
            nameapp = "\"mygame.plugin.app.PluginStrictModeAppMulti\"";
        }
        else
        {
            isExral = true;
            nameapp = "\"mygame.plugin.app.PluginAppExtraMulti\"";
        }
        int idx = line.IndexOf("android:name=");
        if (idx < 0)
        {
            string appre = "<application android:name=" + nameapp + " android:usesCleartextTraffic=\"true\"";
            re = line.Replace("<application", appre);
        }
        else
        {
            int idx1 = line.IndexOf("\"", (idx + 16));
            string sreed = line.Substring(idx, (idx1 - idx + 1));
            if (!line.Contains("android:usesCleartextTraffic"))
            {
                nameapp += " android:usesCleartextTraffic=\"true\"";
            }
            string appre = "android:name=" + nameapp;
            re = line.Replace(sreed, appre);
        }
        string levelDirectoryPath;
        // if (Application.platform == RuntimePlatform.OSXEditor)
        {
            levelDirectoryPath = Application.dataPath + "/GamePlugin/Plugins/Android/";
        }
        // else
        // {
        //     levelDirectoryPath = Application.dataPath + "\\GamePlugin/Plugins/Android\\";
        // }
        string pathto = levelDirectoryPath + "myutilsmall.aar";
        if (isExral)
        {
            string passet = Application.dataPath;
            string ppr = passet.Replace("/Assets", "");
            string pathFrom = ppr + "/Mem/Android/For Build/MyApp/myutilsmall.aar";
            File.Copy(pathFrom, pathto, true);
        }
        else
        {
            File.Delete(pathto);
            File.Delete(pathto + ".meta");
        }
        return re;
    }

    static void doSettingiOS(mygame.sdk.SDKManager sdkManager, mygame.sdk.AdsHelper adsHelper)
    {
#if UNITY_EDITOR
        try
        {
            string levelDirectoryPath;
            // if (Application.platform == RuntimePlatform.OSXEditor)
            {
                levelDirectoryPath = Application.dataPath + "/GamePlugin/Plugins/iOS/";
            }
            // else
            // {
            //     levelDirectoryPath = Application.dataPath + "\\GamePlugin\\Plugins\\iOS\\";
            // }
            string admonappidori = adsHelper.AdmobAppID4iOS;

            //path = levelDirectoryPath + "MyGameAppController.h";
            //lines = System.IO.File.ReadAllLines(path);
            //for (int i = 0; i < lines.Length; i++)
            //{
            //    if (lines[i].Contains("#import <GoogleMobileAds/GoogleMobileAds.h>"))
            //    {
            //        allline.Add(lines[i]);
            //        int nl = i + 1;
            //        if (SettingBuildAndroid.isAdcanvas)
            //        {
            //            if (!lines[nl].Contains("#import \"VXWebViewAppController.h\""))
            //            {
            //                allline.Add("#import \"VXWebViewAppController.h\"");
            //            }
            //        }
            //        else
            //        {
            //            if (lines[nl].Contains("#import \"VXWebViewAppController.h\""))
            //            {
            //                i++;
            //            }
            //        }
            //    }
            //    else if (lines[i].Contains("@interface MyGameAppController"))
            //    {
            //        if (SettingBuildAndroid.isAdcanvas)
            //        {
            //            allline.Add("@interface MyGameAppController : VXWebViewAppController <GADFullScreenContentDelegate>");
            //        }
            //        else
            //        {
            //            allline.Add("@interface MyGameAppController : UnityAppController <GADFullScreenContentDelegate>");
            //        }
            //    }
            //    else
            //    {
            //        allline.Add(lines[i]);
            //    }
            //}
            //System.IO.File.WriteAllLines(path, allline);
            //allline.Clear();

            // if (Application.platform == RuntimePlatform.OSXEditor)
            {
                levelDirectoryPath = Application.dataPath + "/GamePlugin/Editor/";
            }
            // else
            // {
            //     levelDirectoryPath = Application.dataPath + "\\GamePlugin\\Editor\\";
            // }
            string path = levelDirectoryPath + "BuildPostProcessor.cs";
            string[] linesBuil = System.IO.File.ReadAllLines(path);
            List<string> allline = new List<string>();
            for (int i = 0; i < linesBuil.Length; i++)
            {
                if (linesBuil[i].Contains("rootDict.SetString(\"GADApplicationIdentifier\""))
                {
                    if (!admonappidori.StartsWith("ca-app-pub-"))
                    {
                        Debug.LogError("mysdk: -------Error iOS Adnob AppId");
                    }
                    allline.Add($"            rootDict.SetString(\"GADApplicationIdentifier\", \"{admonappidori.Trim()}\");");
                }
                else
                {
                    allline.Add(linesBuil[i]);
                }
            }
            System.IO.File.WriteAllLines(path, allline);
            allline.Clear();

            //coppy button
            string pathimg = Application.dataPath + "/GamePlugin/Plugins/iOS/button_close.png";
            string patdest = Application.dataPath + "/Plugins/iOS/button_close.png";
            try
            { // get file length
                var buffer = File.ReadAllBytes(pathimg);
                File.WriteAllBytes(patdest, buffer);
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ex=" + ex.ToString());
#if UNITY_IOS || UNITY_IPHONE
                Debug.LogError("mysdk: doSettingAppOpenAdiOS button_close ex=" + ex.ToString());
#endif
            }
            finally
            {

            }

            //coppy icon down
            pathimg = Application.dataPath + "/GamePlugin/Plugins/iOS/icon_down.png";
            patdest = Application.dataPath + "/Plugins/iOS/icon_down.png";

            string picad = Application.dataPath + "/GamePlugin/Plugins/iOS/IconAdsiOS.png";
            string pdesicad = Application.dataPath + "/Plugins/iOS/IconAdsiOS.png";

            string pbgblack = Application.dataPath + "/GamePlugin/Plugins/iOS/black.png";
            string pdesbgblack = Application.dataPath + "/Plugins/iOS/black.png";
            try
            { // get file length
                var buffer = File.ReadAllBytes(pathimg);
                File.WriteAllBytes(patdest, buffer);

                var buffericad = File.ReadAllBytes(picad);
                File.WriteAllBytes(pdesicad, buffericad);

                var bufferbgblack = File.ReadAllBytes(pbgblack);
                File.WriteAllBytes(pdesbgblack, bufferbgblack);
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ex=" + ex.ToString());
#if UNITY_IOS || UNITY_IPHONE
                Debug.LogError("mysdk: doSettingAppOpenAdiOS icon_down ex=" + ex.ToString());
#endif
            }
        }
        catch (Exception ex)
        {
            Debug.Log("mysdk: ex=" + ex.ToString());
#if UNITY_IOS || UNITY_IPHONE
            Debug.LogError("mysdk: doSettingAppOpenAdiOS all ex=" + ex.ToString());
#endif
        }
#endif
    }

    private static void doAddGradle(mygame.sdk.AppType typeapp, mygame.sdk.AdsHelper groupControl)
    {
#if UNITY_EDITOR
        try
        {
            string levelDirectoryPath;
            // if (Application.platform == RuntimePlatform.OSXEditor)
            {
                levelDirectoryPath = Application.dataPath + "/Plugins/Android/";
            }
            // else
            // {
            //     levelDirectoryPath = Application.dataPath + "\\Plugins\\Android\\";
            // }
            string path = levelDirectoryPath + "mainTemplate.gradle";
            string[] lines = System.IO.File.ReadAllLines(path);
            bool ishaslifecycle = false;
            int isCheck = 0;
            int statusAddPlaycore = 0;
            int statusAddNativeCmp = 0;
            int statusAddAppset4Iron = 0;
            int statusAddPoraCheck1 = 0;
            int statusAddPoraCheck2 = 0;
            int statusFixDupplicateIRon = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("def unityProjectPath"))
                {
#if UNITY_ANDROID && CHECK_4INAPP
                    statusAddPoraCheck1 = 1;
                    statusAddPoraCheck2 = 1;
                    isCheck = 2;
#endif
                }
                else if (lines[i].Contains("dependencies {"))
                {
                    isCheck = 1;
                }
                if (isCheck == 2)
                {
                    if (lines[i].Contains("url \"https://jitpack.io\""))
                    {
                        statusAddPoraCheck1 = 0;
                        isCheck = 0;
                    }
                }
                else if (isCheck == 1)
                {
                    if (lines[i].Contains("androidx.lifecycle:lifecycle-process"))
                    {
                        ishaslifecycle = true;
                    }
                    else if (lines[i].Contains("implementation 'com.google.android.play:review:"))
                    {
                        statusAddPlaycore = 1;
                    }
                    else if (lines[i].Contains("com.google.android.ump:user-messaging-platform"))
                    {
                        string vergradle = getVerFromGradle(lines[i]);
                        if (vergradle != null && vergradle.Length > 1)
                        {
                            if (compareVersion("2.2.0", vergradle) > 0)
                            {
                                statusAddNativeCmp = 2;
                            }
                            else
                            {
                                statusAddNativeCmp = 1;
                            }
                        }
                    }
                    else if (lines[i].Contains("com.google.ads.mediation:ironsource"))
                    {
                        statusFixDupplicateIRon = 1;
                        if (i < (lines.Length - 3))
                        {
                            if (lines[i + 1].Contains("{") && lines[i + 2].Contains("exclude group"))
                            {
                                statusFixDupplicateIRon = 0;
                            }
                        }
                    }
                    else if (lines[i].Contains("com.google.android.gms:play-services-appset"))
                    {
                        statusAddAppset4Iron = 1;
                    }
                    else if (lines[i].Contains("com.github.javiersantos:PiracyChecker"))
                    {
#if UNITY_ANDROID && CHECK_4INAPP
                        statusAddPoraCheck2 = 0;
#endif
                    }
                    if (lines[i].StartsWith("**DEPS**}") || lines[i].StartsWith("}") || lines[i].Contains("// Android Resolver Dependencies End"))
                    {
                        isCheck = 10;
                        break;
                    }
                }
            }

            List<string> allline = new List<string>();
            if ((statusAddPoraCheck1 == 1 || statusAddPoraCheck2 == 1)
                || !ishaslifecycle
                || statusAddPlaycore == 0
                || (statusAddNativeCmp == 0 || statusAddNativeCmp == 2)
                || (statusAddAppset4Iron == 0 && SettingBuildAndroid.isIronEnable)
                || statusFixDupplicateIRon == 1
                )
            {
                int isAdd = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("def unityProjectPath"))
                    {
                        if (statusAddPoraCheck1 == 1)
                        {
                            isAdd = 10;
                        }
                    }
                    else if (lines[i].Contains("dependencies {"))
                    {
                        isAdd = 1;
                    }

                    if (isAdd == 10)
                    {
                        allline.Add(lines[i]);
                        allline.Add("        maven {");
                        allline.Add("            url \"https://jitpack.io\"");
                        allline.Add("        }");
                        isAdd = 0;
                    }
                    else if (isAdd == 1)
                    {
                        if (statusFixDupplicateIRon == 1 && lines[i].Contains("com.google.ads.mediation:ironsource"))
                        {
                            string nlinei = lines[i].Replace("'com.google", "('com.google");
                            int nnn = nlinei.IndexOf(":ironsource");
                            nnn = nlinei.IndexOf("'", nnn);
                            nlinei = nlinei.Insert(nnn + 1, ")");

                            allline.Add(nlinei);
                            allline.Add("    {");
                            allline.Add("        exclude group: 'com.ironsource.sdk', module: 'mediationsdk'");
                            allline.Add("    }");
                        }
                        else if (lines[i].StartsWith("**DEPS**}") || lines[i].StartsWith("}") || lines[i].Contains("// Android Resolver Dependencies End"))
                        {
                            isAdd = 2;

                            if (statusAddPlaycore == 0)
                            {
                                allline.Add(" ");
                                allline.Add($"    implementation 'com.android.support.constraint:constraint-layout:2.1.4'");
                                allline.Add($"    implementation 'com.google.android.play:review:2.0.1'");
                            }

                            if (statusAddNativeCmp == 0)
                            {
                                allline.Add($"    implementation 'com.google.android.ump:user-messaging-platform:2.2.0'");
                            }
                            if (statusAddPoraCheck2 == 1)
                            {
                                allline.Add("    implementation 'com.github.javiersantos:PiracyChecker:1.2.8'");
                            }
                            if (!ishaslifecycle)
                            {
                                allline.Add(" ");
                                allline.Add("    implementation 'androidx.lifecycle:lifecycle-process:2.7.0'");
                            }
                            allline.Add(lines[i]);
                        }
                        else
                        {
                            if (lines[i].Contains("com.google.android.ump:user-messaging-platform:"))
                            {
                                string vergradle = getVerFromGradle(lines[i]);
                                if (vergradle != null && vergradle.Length > 1 && compareVersion("2.2.0", vergradle) > 0)
                                {
                                    allline.Add($"    implementation 'com.google.android.ump:user-messaging-platform:2.2.0'");
                                }
                                else
                                {
                                    allline.Add(lines[i]);
                                }
                            }
                            else
                            {
                                allline.Add(lines[i]);
                            }
                        }
                    }
                    else
                    {
                        allline.Add(lines[i]);
                    }
                }
                System.IO.File.WriteAllLines(path, allline);
                allline.Clear();
            }

            //launcherTemplate.gradle
            allline.Clear();
            path = levelDirectoryPath + "launcherTemplate.gradle";
            string[] lines2 = System.IO.File.ReadAllLines(path);
            int statusmultidex = 0;
            int statussplit = 0;
            int statusFlagMulti = 0;
            for (int i = 0; i < lines2.Length; i++)
            {
                if (lines2[i].Contains("androidx.multidex:multidex:"))
                {
                    statusmultidex = 1;
                }
                else if (lines2[i].Contains("multiDexEnabled true"))
                {
                    statusFlagMulti = 1;
                }
                else if (lines2[i].Contains("import com.android.build.OutputFile"))
                {
                    statussplit = 1;
                }
            }
            if (statussplit == 0 || ((statusmultidex == 0 || statusFlagMulti == 0) && typeapp != mygame.sdk.AppType.Application) || ((statusmultidex == 1 || statusFlagMulti == 1) && typeapp == mygame.sdk.AppType.Application))
            {
                for (int i = 0; i < lines2.Length; i++)
                {
                    if (lines2[i].Contains("dependencies"))
                    {
                        allline.Add(lines2[i]);
                        if (statusmultidex == 0 && typeapp != mygame.sdk.AppType.Application)
                        {
                            allline.Add("   implementation 'androidx.multidex:multidex:2.0.1'");
                        }
                    }
                    else if (lines2[i].Contains("androidx.multidex:multidex:"))
                    {
                        if (statusmultidex == 0 || typeapp != mygame.sdk.AppType.Application)
                        {
                            allline.Add(lines2[i]);
                        }
                    }
                    else if (lines2[i].Contains("versionName"))
                    {
                        allline.Add(lines2[i]);
                        if (statusFlagMulti == 0 && typeapp != mygame.sdk.AppType.Application)
                        {
                            allline.Add("       multiDexEnabled true");
                        }
                    }
                    else if (lines2[i].Contains("multiDexEnabled true"))
                    {
                        if (statusFlagMulti == 0 || typeapp != mygame.sdk.AppType.Application)
                        {
                            allline.Add(lines2[i]);
                        }
                    }
                    else if (lines2[i].Contains("}**SPLITS_VERSION_CODE****"))
                    {
                        allline.Add(lines2[i]);
                        if (statussplit == 0)
                        {
                            allline.Add("ext.abiCodes = ['armeabi-v7a':0, 'arm64-v8a':1]");
                            allline.Add("");
                            allline.Add("import com.android.build.OutputFile");
                            allline.Add("");
                            allline.Add("android.applicationVariants.all { variant ->");
                            allline.Add("  variant.outputs.each { output ->");
                            allline.Add("    def baseAbiVersionCode =");
                            allline.Add("            // Determines the ABI for this variant and returns the mapped value.");
                            allline.Add("            project.ext.abiCodes.get(output.getFilter(OutputFile.ABI))");
                            allline.Add("    if (baseAbiVersionCode != null) {");
                            allline.Add("      output.versionCodeOverride =");
                            allline.Add("              baseAbiVersionCode + variant.versionCode");
                            allline.Add("    }");
                            allline.Add("  }");
                            allline.Add("}");
                        }
                    }
                    else
                    {
                        allline.Add(lines2[i]);
                    }
                }
                System.IO.File.WriteAllLines(path, allline);
            }

            //settingsTemplate.gradle
            allline.Clear();
            path = levelDirectoryPath + "settingsTemplate.gradle";
            string[] lines3 = System.IO.File.ReadAllLines(path);
            int statussetting = 0;
            int countdau = 0;
            int idxAddsetting = -1;
            for (int i = 0; i < lines3.Length; i++)
            {
                if (statussetting == 0)
                {
                    if (lines3[i].Contains("pluginManagement"))
                    {
                        statussetting = 1;
                    }
                }
                if (statussetting == 1)
                {
                    if (lines3[i].Contains("{"))
                    {
                        countdau++;
                    }
                    if (lines3[i].Contains("}"))
                    {
                        countdau--;
                    }
                    if (lines3[i].Contains("buildscript"))
                    {
                        statussetting = 0;
                        break;
                    }
                    if (i > 3 && countdau == 0)
                    {
                        idxAddsetting = i;
                        statussetting = 2;
                        break;
                    }
                }
            }
            if (statussetting == 2)
            {
                for (int i = 0; i < lines3.Length; i++)
                {
                    if (i == idxAddsetting)
                    {
                        allline.Add("    buildscript {");
                        allline.Add("        repositories {");
                        allline.Add("            mavenCentral()");
                        allline.Add("            maven {");
                        allline.Add("                url=uri(\"https://storage.googleapis.com/r8-releases/raw\")");
                        allline.Add("            }");
                        allline.Add("        }");
                        allline.Add("        dependencies {");
                        allline.Add("            classpath(\"com.android.tools:r8:8.2.26\")");
                        allline.Add("        }");
                        allline.Add("    }");
                        allline.Add(lines3[i]);
                    }
                    else
                    {
                        allline.Add(lines3[i]);
                    }
                }
                System.IO.File.WriteAllLines(path, allline);
                allline.Clear();
            }
        }
        catch (Exception ex)
        {
#if UNITY_ANDROID
            Debug.LogError("mysdk: doAddGradle ex=" + ex.ToString());
#endif
        }
#endif
    }

    public static string getVerFromGradle(string data)
    {
        string re = "";

        int idx1 = data.IndexOf("//");

        if (idx1 > 17 || idx1 < 0)
        {
            idx1 = data.IndexOf("\'");
            string charkey = "\'";
            if (idx1 < 0)
            {
                idx1 = data.IndexOf("\"");
                charkey = "\"";
            }
            if (idx1 > 0)
            {
                int idx2 = data.IndexOf(charkey, idx1 + 5);
                if (idx2 > 0)
                {
                    string rdata = data.Substring(idx1 + 1, idx2 - idx1 - 1);
                    idx1 = rdata.LastIndexOf(":");
                    if (idx1 > 0)
                    {
                        re = rdata.Substring(idx1 + 1);
                    }
                }
            }
        }

        return re;
    }

    public static string getVerFromXml(string data)
    {
        string re = "";

        int idx1 = data.LastIndexOf(":");

        if (idx1 > 5)
        {
            char chcheck = '"';
            if (data.Length > (idx1 + 1))
            {
                if (data[idx1 + 1] == '[')
                {
                    idx1++;
                    chcheck = ']';
                }
            }
            int idx2 = data.IndexOf(chcheck, idx1 + 1);
            if (idx2 < idx1 && chcheck == '"')
            {
                chcheck = '\'';
                idx2 = data.IndexOf(chcheck, idx1 + 1);
            }
            if (idx2 > 0)
            {
                re = data.Substring(idx1 + 1, idx2 - idx1 - 1);
            }
        }

        return re;
    }

    private static int compareVersion(string v1, string v2)
    {
        try
        {
            string[] arrv1 = v1.Split(new char[] { '.' });
            string[] arrv2 = v2.Split(new char[] { '.' });
            int[] numv1 = new int[arrv1.Length];
            int[] numv2 = new int[arrv2.Length];
            for (int i = 0; i < arrv1.Length; i++)
            {
                numv1[i] = int.Parse(arrv1[i]);
            }
            for (int i = 0; i < arrv2.Length; i++)
            {
                numv2[i] = int.Parse(arrv2[i]);
            }
            int n = numv1.Length;
            if (n > numv2.Length)
            {
                n = numv2.Length;
            }
            for (int i = 0; i < n; i++)
            {
                if (numv1[i] > numv2[i])
                {
                    return 1;
                }
                else if (numv1[i] < numv2[i])
                {
                    return -1;
                }
            }
            if (numv1.Length > numv2.Length)
            {
                return 1;
            }
            else if (numv1.Length < numv2.Length)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
        catch (Exception ex)
        {
            Debug.Log("mysdk: compareVersion!!!!!!!!! ex=" + ex.ToString());
            return 0;
        }
    }

}

public class GameOb4Addrf
{
    public List<string> listComponent = new List<string>();
}

[CustomEditor(typeof(SDKUpdate)), CanEditMultipleObjects]
public class UpdateSdk : Editor
{
    public static SDKUpdate groupControl = null;

    public bool isBuild = false;
    private bool membuild = false;
    private Thread _t1 = null;

    public override void OnInspectorGUI()
    {
        if (groupControl == null)
        {
            groupControl = (SDKUpdate)target;
        }
        GUILayout.BeginVertical();

        if (GUILayout.Button("UpdateSdk"))
        {
            doUpdate();
            AssetDatabase.Refresh();
        }
        GUILayout.EndVertical();

        try
        {
            base.OnInspectorGUI();
        }
        catch (Exception ex)
        {
            Debug.LogError("mysdk: OnInspectorGUI ex=" + ex.ToString());
        }
    }

    private void _func1()
    {
        //while (isBuild)
        //{
        //    string passet = Application.dataPath;
        //    string ppro = passet.Replace("/Assets", "");
        //    string psetting = ppro + "/ProjectSettings/ProjectSettings.asset";
        //    string[] allLinesMeta = File.ReadAllLines(psetting);
        //    foreach (var line in allLinesMeta)
        //    {
        //        if (line.Contains("m_EditorVersion"))
        //        {
        //            if (line.Contains("2021"))
        //            {
        //                unityver = "21";
        //            }
        //            else if (line.Contains("2022"))
        //            {
        //                unityver = "22";
        //            }
        //            else if (line.Contains("2020"))
        //            {
        //                unityver = "20";
        //            }
        //            break;
        //        }
        //    }
        //}
    }

    private void stopclearlogo()
    {
        Debug.Log("stopclearlogo=" + isBuild);
        if (_t1 != null)
        {
            _t1.Abort();
            _t1 = null;
        }
    }

    public void doUpdate()
    {
        groupControl = (SDKUpdate)target;

        if (groupControl.pathSDK != null && groupControl.pathSDK.Length > 5 && Directory.Exists(groupControl.pathSDK))
        {
            string GamePlugin = Application.dataPath + "/GamePlugin";
            Directory.Delete(GamePlugin + "/Ads/AdmobPlugin/Editor", true);
            Directory.Delete(GamePlugin + "/Ads/IronPlugin/Editor", true);
            Directory.Delete(GamePlugin + "/Ads/MaxPlugin/Editor", true);

            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/AdAudio", GamePlugin + "/AdAudio", 1);
            if (groupControl.AdCanvasFolder)
            {
                SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/AdCanvasHelper", GamePlugin + "/AdCanvasHelper", 1);
            }
            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/Ads", GamePlugin + "/Ads", 1);
            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/Analytic", GamePlugin + "/Analytic", 1);
            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/AppFlyerHelper", GamePlugin + "/AppFlyerHelper", 1);
            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/Editor", GamePlugin + "/Editor", 1);
            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/Fonts", GamePlugin + "/Fonts", 1);
            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/FirHelper", GamePlugin + "/FirHelper", 1);
            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/GameHelper", GamePlugin + "/GameHelper", 1);
            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/GameManager", GamePlugin + "/GameManager", 1);
            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/ImageLoader", GamePlugin + "/ImageLoader", 1);
            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/Inapp", GamePlugin + "/Inapp", 1);
            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/OtherGame", GamePlugin + "/OtherGame", 1);
            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/Platforms", GamePlugin + "/Platforms", 1);
            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/Plugins", GamePlugin + "/Plugins", 1);
            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/Script", GamePlugin + "/Script", 1);
            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/Sptires", GamePlugin + "/Sptires", 1);
            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/Utils", GamePlugin + "/Utils", 1);
            SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/LogManager", GamePlugin + "/LogManager", 1);
            if (!File.Exists(GamePlugin + "/Scene/SplashScene.unity"))
            {
                SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/Scene", GamePlugin + "/Scene", 1);
            }
            SettingBuildAndroid.coppyFileNormal(groupControl.pathSDK + "/Api/LogEventApi.cs", GamePlugin + "/Api/LogEventApi.cs");
            if (groupControl.AdCanvasTTDefault)
            {
                SettingBuildAndroid.coppyFileNormal(groupControl.pathSDK + "/ttDefault.jpg", GamePlugin + "/AdCanvasHelper/Res/ttDefault.jpg");
                SettingBuildAndroid.coppyFileNormal(groupControl.pathSDK + "/ttDefault.jpg.meta", GamePlugin + "/AdCanvasHelper/Res/ttDefault.jpg.meta");
            }
            if (groupControl.AdCanvasPrefab)
            {
                SettingBuildAndroid.coppyFileNormal(groupControl.pathSDK + "/AdCanvasHelper.prefab", GamePlugin + "/AdCanvasHelper/AdCanvasHelper.prefab");
            }
            if (groupControl.ApiAdd)
            {
                SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/Api", GamePlugin + "/Api", 1);
            }
            if (groupControl.GamepluginPreb)
            {
                SettingBuildAndroid.coppyFileNormal(groupControl.pathSDK + "/GamePlugin.prefab", GamePlugin + "/GameManager/GamePlugin.prefab");
            }
            if (groupControl.GameResAdd)
            {
                SettingBuildAndroid.coppyFolder(groupControl.pathSDK + "/GameRes", GamePlugin + "/GameRes", 1);
            }
        }
    }
}