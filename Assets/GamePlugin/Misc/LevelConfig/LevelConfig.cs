using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using mygame.sdk;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using static LevelConfig;
using Random = Unity.Mathematics.Random;
using static Unity.Collections.AllocatorManager;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif
[CreateAssetMenu(fileName = "LevelConfig", menuName = "Data/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public const int CountLevelRandom = 10;
    public const int MinLevelRandom = 50;

    public static string CacheLevelIndex
    {
        get => PlayerPrefs.GetString("s_cache_level_index", "");
        set => PlayerPrefs.SetString("s_cache_level_index", value);
    }

    public static string LastCacheLevelIndex
    {
        get => PlayerPrefs.GetString("last_cache_level_index", "");
        set => PlayerPrefs.SetString("last_cache_level_index", value);
    }

    [Serializable]
    public class LevelReward
    {
        public LevelType type;
        public DataResource[] reward;
        public int star;
    }

    public class LevelConfigData
    {
        public Data[] data;

        public class Data
        {
            public string mediaCampain = "";
            public int version;
            public string dataLevelShuffle;
            public LevelInfo[] levelInfos;
            public string levelMonetizations;
        }
    }

    public class LevelConfigDataNew
    {
        public LevelInfo[] levelInfos;
    }

    [Serializable]
    public class LevelInfo
    {
#if UNITY_EDITOR
        [JsonIgnore, HideInInspector] public string name;
#endif
        [JsonProperty("l")] public ushort level;
        [JsonProperty("l2")] public int levelID;
        [JsonProperty("sl2")] public ushort splineId;
        [JsonProperty("b")] public int backgroundID;
        [JsonProperty("t")] public LevelType levelType;
        [JsonProperty("im")] public bool isMission;
        [JsonProperty("bd")] public string base64Data;


        [JsonIgnore] public bool? iconValid;
        [JsonIgnore] public bool? iconValidLocal;
        [JsonIgnore] public bool? isValid;
        [JsonIgnore] public bool? isValidLocal;

        public bool IsValid()
        {
            if (isValid == null)
            {
                var hasLocal = LevelRemoteManager.Instance.AddressableContainsInLocal(LevelRemoteManager.Instance.LevelName(levelID), out var containKey);
                isValidLocal ??= hasLocal;
                isValid ??= containKey;
            }

            if (iconValid == null)
            {
                var iconHasLocal = LevelRemoteManager.Instance.AddressableContainsInLocal(LevelRemoteManager.Instance.IconName(levelID), out var containKey);
                iconValidLocal ??= iconHasLocal;
                iconValid ??= containKey;
            }

            if (LevelRemoteManager.Instance.IsLoadLocalOnly())
                return isValidLocal != null && isValidLocal.Value;
            return isValid.Value;
        }

        public bool IsIconValid()
        {
            if (!LevelRemoteManager.Instance.usingIcon) return true;
            Debug.Log($"iconValidLocal= {iconValidLocal.HasValue && iconValidLocal.Value}, iconValid= {iconValid.HasValue && iconValid.Value}, IsLoadLocalOnly= {LevelRemoteManager.Instance.IsLoadLocalOnly()}");
            if (LevelRemoteManager.Instance.IsLoadLocalOnly())
                return iconValidLocal != null && iconValidLocal.Value;
            return iconValid != null && iconValid.Value;
        }

        public void ClearIsValidCache()
        {
            isValid = null;
            isValidLocal = null;
            iconValid = null;
            iconValidLocal = null;
        }
    }

    public string dataLevelShuffleBase64;
    public LevelInfo[] levelInfos;
#if UNITY_EDITOR
    public LevelInfo[] levelABTests;
#endif
    public LevelReward[] levelRewards;

    public LevelInfo[] levelInfoConfig { get; private set; }

    private readonly List<ushort> randomIgnoreLevel = new List<ushort>();


    public DataResource[] GetLevelReward(LevelType type)
    {
        return levelRewards.Single(x => x.type == type).reward;
    }
    public int GetLevelStar(LevelType type)
    {
        return levelRewards.Single(x => x.type == type).star;
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        for (int i = 0; i < levelInfos.Length; i++)
        {
            var lf = levelInfos[i];
            if (lf != null)
            {
                lf.name = $"Level {lf.level} - {lf.levelType.ToString().ToUpper()}";
            }
        }
        if (levelABTests != null)
        {
            foreach (var lf in levelABTests)
            {
                if (lf != null)
                {
                    lf.name = $"Level {lf.level} - {lf.levelType.ToString().ToUpper()}";
                }
            }
        }
    }
#endif

    public void SetLevelShuffleData(string data)
    {
        dataLevelShuffleBase64 = data;
    }
    public void SetLevelInfos(LevelInfo[] data)
    {
        levelInfoConfig = data;
    }
    
    public LevelInfo[] GetLevelInfos()
    {
        if (levelInfoConfig != null && levelInfoConfig.Length > 0) return levelInfoConfig;
        return levelInfos;
    }

    public void UpdateValidStatus()
    {
        var lf = GetLevelInfos();
        for (int i = 0; i < lf.Length; i++)
        {
            lf[i].ClearIsValidCache();
        }
    }

    public LevelInfo GetLevelInfo(int level, bool isRandom = true)
    {
        var data = GetLevelInfos();
        var lf = data.SingleOrDefault(x => x.level == level);
       
        if (isRandom && (lf == null || !lf.IsValid() || !lf.IsIconValid()))
        {
            Debug.LogWarning($"Not exits level {level}!");
            return RandomLevel(data, level);
        }

        return lf;
    }
    public LevelInfo GetLevelInfoByID(int level, bool isRandom = true)
    {
        var data = GetLevelInfos();
        var lf = data.SingleOrDefault(x => x.levelID == level);
        if (isRandom && (lf == null || !lf.IsValid() || !lf.IsIconValid()))
        {
            Debug.LogWarning($"Not exits level {level}!");
            return RandomLevel(data, level);
        }

        return lf;
    }

    public LevelInfo GetLevelInfo(int level, out bool isLocal, out bool isRandom)
    {
        var data = GetLevelInfos();
        var lf = data.SingleOrDefault(x => x.level == level);
        isRandom = false;
        if (lf == null || !lf.IsValid() || !lf.IsIconValid())
        {
            Debug.LogWarning($"Not exits level {level}!");

            var lvRandom = RandomLevel(data, level);
            lvRandom.IsValid();
            isLocal = lvRandom.isValidLocal != null && lvRandom.isValidLocal.Value;
            isRandom = true;
            return lvRandom;
        }

        isLocal = lf.isValidLocal != null && lf.isValidLocal.Value;
        return lf;
    }

    private LevelInfo RandomLevel(LevelInfo[] data, int level)
    {
        if (string.IsNullOrEmpty(CacheLevelIndex))
        {
            var easy = data.Where(x => x.levelType == LevelType.Easy && x.level > MinLevelRandom && !randomIgnoreLevel.Contains(x.level) && x.IsValid() && x.isValidLocal != null && x.isValidLocal.Value).ToArray();
            var hard = data.Where(x => x.levelType == LevelType.Hard && x.level > MinLevelRandom && !randomIgnoreLevel.Contains(x.level) && x.IsValid() && x.isValidLocal != null && x.isValidLocal.Value).ToArray();
            var crazy = data.Where(x => x.levelType == LevelType.Crazy && x.level > MinLevelRandom && !randomIgnoreLevel.Contains(x.level) && x.IsValid() && x.isValidLocal != null && x.isValidLocal.Value).ToArray();

            var selected = new List<LevelInfo>();
            var usedLevels = new HashSet<int>();

            // ===== CRAZY (1) =====
            foreach (var lf in crazy.OrderBy(_ => UnityEngine.Random.value))
            {
                if (usedLevels.Add(lf.level))
                {
                    selected.Add(lf);
                    break;
                }
            }

            // ===== HARD (2) =====
            foreach (var h in hard.OrderBy(_ => UnityEngine.Random.value))
            {
                if (selected.Count >= 3) break; // 1 crazy + 2 hard
                if (usedLevels.Add(h.level))
                    selected.Add(h);
            }

            // ===== EASY (fill) =====
            foreach (var e in easy.OrderBy(_ => UnityEngine.Random.value))
            {
                if (selected.Count >= CountLevelRandom) break;
                if (usedLevels.Add(e.level))
                    selected.Add(e);
            }

            selected = selected.OrderBy(x => UnityEngine.Random.value).ToList();

            for (int i = 1; i < selected.Count; i++)
            {
                var val0 = selected[i - 1];
                var val1 = selected[i];
                if (val0.levelType >= LevelType.Hard && val1.levelType >= LevelType.Hard)
                {
                    int j = selected.FindIndex(i + 1, x => x.levelType == LevelType.Easy);
                    if (j != -1) (selected[i], selected[j]) = (selected[j], selected[i]);
                }
            }

            var output = selected.Select(x => x.level).ToList();
            randomIgnoreLevel.AddRange(output);
            CacheLevelIndex = string.Join(",", output);

            var c = randomIgnoreLevel.Count - (CountLevelRandom + 5);
            if (c > 0) randomIgnoreLevel.RemoveRange(0, c);
        }

        return GetRandomLevelCache(data, level);
    }

    public LevelInfo GetRandomLevelCache(LevelInfo[] data, int level)
    {
        if (string.IsNullOrEmpty(CacheLevelIndex)) return null;
        var lst = CacheLevelIndex.Split(",");
        var lv = lst[(level - 1) % CountLevelRandom];
        if (string.IsNullOrEmpty(lv)) lv = lst[0];
        return data.SingleOrDefault(x => x.level == int.Parse(lv));
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelConfig))]
public class LevelConfigEditor : Editor
{
    public int lvMin = 50;
    public int lvMax = 100;
    public int clientLength = 1000;
    public int groupSize = 50;
    public int groupIconSize = 200;

    public override void OnInspectorGUI()
    {
        var levelConfig = (LevelConfig)target;
        GUILayout.BeginHorizontal();

        GUILayout.Label("Client Length", GUILayout.Width(125));
        clientLength = EditorGUILayout.IntField(clientLength, GUILayout.Width(45));

        GUILayout.Label("Group Size", GUILayout.Width(75));
        groupSize = EditorGUILayout.IntField(groupSize, GUILayout.Width(45));

        GUILayout.Label("Icon Group Size", GUILayout.Width(100));
        groupIconSize = EditorGUILayout.IntField(groupIconSize, GUILayout.Width(45));

        if (GUILayout.Button("Random Level"))
        {
            var data = levelConfig.levelInfos;
            var easy = data.Where(x => x.levelType == LevelType.Easy && x.level > MinLevelRandom).ToArray();
            var hard = data.Where(x => x.levelType == LevelType.Hard && x.level > MinLevelRandom).ToArray();
            var crazy = data.Where(x => x.levelType == LevelType.Crazy && x.level > MinLevelRandom).ToArray();

            var selected = new List<LevelInfo>();
            var usedLevels = new HashSet<int>();

            for (int k = 0; k < 50000; k++)
            {
                if (selected.Count >= 10000) break;
                int selectCount = 0;
                // ===== CRAZY (1) =====
                foreach (var lf in crazy.OrderBy(_ => UnityEngine.Random.value))
                {
                    if (usedLevels.Add(lf.level))
                    {
                        selectCount++;
                        selected.Add(lf);
                        break;
                    }
                }

                // ===== HARD (2) =====
                foreach (var h in hard.OrderBy(_ => UnityEngine.Random.value))
                {
                    if (selectCount >= 3) break; // 1 crazy + 2 hard
                    if (usedLevels.Add(h.level))
                    {
                        selectCount++;
                        selected.Add(h);
                    }
                }

                // ===== EASY (fill) =====
                foreach (var e in easy.OrderBy(_ => UnityEngine.Random.value))
                {
                    if (selectCount >= CountLevelRandom) break;
                    if (usedLevels.Add(e.level))
                    {
                        selectCount++;
                        selected.Add(e);
                    }
                }



                if (usedLevels.Count > 100) usedLevels.Remove(usedLevels.First());
            }

            selected = selected.OrderBy(x => UnityEngine.Random.value).ToList();

            for (int i = 1; i < selected.Count; i++)
            {
                var val0 = selected[i - 1];
                var val1 = selected[i];
                if (val0.levelType >= LevelType.Hard && val1.levelType >= LevelType.Hard)
                {
                    int j = selected.FindIndex(i + 1, x => x.levelType == LevelType.Easy);
                    if (j != -1) (selected[i], selected[j]) = (selected[j], selected[i]);
                }
            }
            levelConfig.levelInfos.AddRange(selected);
            EditorUtility.SetDirty(levelConfig);
        }

        if (GUILayout.Button("Setup Remote Addressable"))
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.isLoaded)
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    var lvR = root.GetComponentInChildren<LevelRemoteManager>(true);
                    if (lvR != null)
                    {
                        lvR.localLevelCount = clientLength;
                        lvR.levelPackSize = groupSize;
                        lvR.iconPackSize = groupIconSize;
                        EditorUtility.SetDirty(lvR);
                    }
                }
            }

            try
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                ProcessLevel(settings, settings.FindGroup("LevelRemote"));
                ProcessIcon(settings, settings.FindGroup("IconRemote"));

            }
            finally
            {
                Debug.Log("✅ Setup Addressables hoàn tất và refresh 1 lần duy nhất.");
            }
        }

        if (GUILayout.Button("Setup Local Addressable"))
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.isLoaded)
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    var lvR = root.GetComponentInChildren<LevelRemoteManager>(true);
                    if (lvR != null)
                    {
                        lvR.localLevelCount = clientLength;
                        lvR.levelPackSize = groupSize;
                        lvR.iconPackSize = groupIconSize;
                        EditorUtility.SetDirty(lvR);
                    }
                }
            }

            try
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                ProcessLevel(settings, settings.FindGroup("LevelNormal"));
                ProcessIcon(settings, settings.FindGroup("IconNormal"));
            }
            finally
            {
                Debug.Log("✅ Setup Addressables hoàn tất và refresh 1 lần duy nhất.");
            }
        }


        void ProcessLevel(AddressableAssetSettings settings, AddressableAssetGroup group)
        {
            var guids = new List<string>();
            string levelPathNormal = "Assets/BusRushFever/Prefabs/DataLevel/Level_";
            var levelPathRemote = "Assets/ConvertData/DataGD/DataRemote/Level_";
            var levelPathMission = "Assets/ConvertData/DataGD/DataMission/Level_";

            var extension = ".json";
            var ls = new List<LevelInfo>(levelConfig.levelInfos);
            if (levelConfig.levelABTests != null)
            {
                ls.AddRange(levelConfig.levelABTests);

            }
            var levelInfos = ls.Where(x => x != null).GroupBy(x => x.levelID).Select(g => g.First()).ToArray();
            for (int i = 0; i < levelInfos.Length; i++)
            {
                if (levelInfos[i].level <= clientLength && group.Name == "LevelNormal")
                {
                    var fileName = levelInfos[i].levelID + extension;
                    var lv = levelPathNormal + fileName;

                    if (!File.Exists(lv))
                    {
                        lv = levelPathRemote + fileName;
                    }
                    if (!File.Exists(lv))
                    {
                        lv = levelPathMission + fileName;
                    }
                    if (File.Exists(lv))
                    {
                        guids.Add(AssetDatabase.AssetPathToGUID(lv));
                        if (File.Exists(levelPathRemote + fileName))
                        {
                            try
                            {
                                File.Move(levelPathRemote + fileName, levelPathNormal + fileName);
                                File.Move(levelPathRemote + fileName + ".meta", levelPathNormal + fileName + ".meta");
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e);
                                Debug.LogError(levelPathNormal + fileName);
                                throw;
                            }
                        }
                    }
                }
                else if (levelInfos[i].level > clientLength && group.Name == "LevelRemote")
                {
                    var fileName = levelInfos[i].levelID + extension;
                    var lv = levelPathNormal + fileName;

                    if (!File.Exists(lv))
                    {
                        lv = levelPathRemote + fileName;
                    }

                    if (File.Exists(lv))
                    {
                        guids.Add(AssetDatabase.AssetPathToGUID(lv));
                        if (File.Exists(levelPathNormal + fileName))
                        {
                            try
                            {
                                File.Move(levelPathNormal + fileName, levelPathRemote + fileName);
                                File.Move(levelPathNormal + fileName + ".meta", levelPathRemote + fileName + ".meta");
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e);
                                Debug.LogError(levelPathNormal + fileName);
                                throw;
                            }
                        }
                    }
                }
            }

            for (int i = group.entries.Count - 1; i >= 0; i--)
            {
                group.RemoveAssetEntry(group.entries.ElementAt(i), false);
            }

            int groupIndex = 1;
            int count = 0;
            string currentLabel = group.Name == "LevelNormal" ? $"Lv_Group_{groupIndex}" : $"Lv_Remote_Group_{groupIndex}";

            if (!settings.GetLabels().Contains(currentLabel))
                settings.AddLabel(currentLabel);

            foreach (var guid in guids)
            {
                if (count >= groupSize)
                {
                    groupIndex++;
                    count = 0;
                    currentLabel = group.Name == "LevelNormal"
                        ? $"Lv_Group_{groupIndex}"
                        : $"Lv_Remote_Group_{groupIndex}";

                    if (!settings.GetLabels().Contains(currentLabel))
                        settings.AddLabel(currentLabel);
                }

                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(path)) continue;

                var entry = settings.CreateOrMoveEntry(guid, group);
                entry.SetAddress(Path.GetFileNameWithoutExtension(path));
                entry.labels.Clear();
                entry.SetLabel(currentLabel, true);

                count++;
            }

            Debug.Log($"✅ Normal → Shared group: {group.Name}, total labels: {groupIndex}");
        }

        void ProcessIcon(AddressableAssetSettings settings, AddressableAssetGroup group)
        {
            if (group == null) return;
            var guids = new List<string>();
            string levelPathNormal = "Assets/Games/Prefabs/Levels/Icon/Normal/Level_";
            var levelPathRemote = "Assets/Games/Prefabs/Levels/Icon/Remote/Level_";
            var ls = new List<LevelInfo>(levelConfig.levelInfos);
            if (levelConfig.levelABTests != null)
            {
                ls.AddRange(levelConfig.levelABTests);
            }
            var levelInfos = ls.Where(x => x != null).GroupBy(x => x.levelID).Select(g => g.First()).ToArray();

            for (int i = 0; i < levelInfos.Length; i++)
            {
                if (levelInfos[i].level <= clientLength && group.Name == "IconNormal")
                {
                    var fileName = levelInfos[i].levelID + ".png";
                    var lv = levelPathNormal + fileName;

                    if (!File.Exists(lv))
                    {
                        lv = levelPathRemote + fileName;
                    }

                    if (File.Exists(lv))
                    {
                        guids.Add(AssetDatabase.AssetPathToGUID(lv));
                        if (File.Exists(levelPathRemote + fileName))
                        {
                            File.Move(levelPathRemote + fileName, levelPathNormal + fileName);
                            File.Move(levelPathRemote + fileName + ".meta", levelPathNormal + fileName + ".meta");
                        }
                    }
                }
                else if (levelInfos[i].level > clientLength && group.Name == "IconRemote")
                {
                    var fileName = levelInfos[i].levelID + ".png";
                    var lv = levelPathNormal + fileName;

                    if (!File.Exists(lv))
                    {
                        lv = levelPathRemote + fileName;
                    }

                    if (File.Exists(lv))
                    {
                        guids.Add(AssetDatabase.AssetPathToGUID(lv));
                        if (File.Exists(levelPathNormal + fileName))
                        {
                            try
                            {
                                File.Move(levelPathNormal + fileName, levelPathRemote + fileName);
                                File.Move(levelPathNormal + fileName + ".meta", levelPathRemote + fileName + ".meta");
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e);
                                Debug.LogError(levelPathNormal + fileName);
                                throw;
                            }
                        }
                    }
                }
            }

            for (int i = group.entries.Count - 1; i >= 0; i--)
            {
                group.RemoveAssetEntry(group.entries.ElementAt(i), false);
            }

            int groupIndex = 1;
            int count = 0;
            string currentLabel = group.Name == "IconNormal" ? $"Icon_Group_{groupIndex}" : $"Icon_Remote_Group_{groupIndex}";

            if (!settings.GetLabels().Contains(currentLabel))
                settings.AddLabel(currentLabel);

            foreach (var guid in guids)
            {
                if (count >= groupIconSize)
                {
                    groupIndex++;
                    count = 0;
                    currentLabel = group.Name == "IconNormal"
                        ? $"Icon_Group_{groupIndex}"
                        : $"Icon_Remote_Group_{groupIndex}";

                    if (!settings.GetLabels().Contains(currentLabel))
                        settings.AddLabel(currentLabel);
                }

                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(path)) continue;

                var entry = settings.CreateOrMoveEntry(guid, group);
                entry.SetAddress($"Icon_{Path.GetFileNameWithoutExtension(path)}");
                entry.labels.Clear();
                entry.SetLabel(currentLabel, true);

                count++;
            }

            Debug.Log($"✅ Normal → Shared group: {group.Name}, total labels: {groupIndex}");
        }
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();

        GUILayout.Label("LvMin", GUILayout.Width(125));
        lvMin = EditorGUILayout.IntField(lvMin, GUILayout.Width(45));

        GUILayout.Label("LvMax", GUILayout.Width(75));
        lvMax = EditorGUILayout.IntField(lvMax, GUILayout.Width(45));

        if (GUILayout.Button("Setup RemoteConfig"))
        {
            var ls = new List<LevelInfo>(levelConfig.levelInfos);
            if (levelConfig.levelABTests != null)
            {
                ls.AddRange(levelConfig.levelABTests);
            }
            var uniqueLs = ls.Where(x => x != null).GroupBy(x => x.levelID).Select(g => g.First()).ToList();
            for (int i = lvMin; i < lvMax; i++)
            {
                if (i >= uniqueLs.Count) break;
                try
                {
                    var bytes = AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/ConvertData/DataGD/DataNormal/Level_{uniqueLs[i].levelID}.bytes").bytes;
                    uniqueLs[i].base64Data = Convert.ToBase64String(bytes);

                }
                catch (Exception e)
                {
                }

                try
                {
                    var bytes2 = AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/ConvertData/DataGD/DataRemote/Level_{uniqueLs[i].levelID}.bytes").bytes;
                    uniqueLs[i].base64Data = Convert.ToBase64String(bytes2);
                }
                catch (Exception e)
                {
                }
            }
            EditorUtility.SetDirty(levelConfig);
        }

        if (GUILayout.Button("Clear RemoteConfig"))
        {
            var ls = new List<LevelInfo>(levelConfig.levelInfos);
            if (levelConfig.levelABTests != null)
            {
                ls.AddRange(levelConfig.levelABTests);
            }
            var uniqueLs = ls.Where(x => x != null).GroupBy(x => x.levelID).Select(g => g.First()).ToList();
            for (int i = lvMin; i < lvMax; i++)
            {
                if (i >= uniqueLs.Count) break;
                uniqueLs[i].base64Data = "";
            }
            EditorUtility.SetDirty(levelConfig);
        }

        GUILayout.EndHorizontal();


        if (GUILayout.Button("Setup Level"))
        {
            var files = Directory.GetFiles("Assets/BusRushFever/Prefabs/DataLevel", "*.bytes", SearchOption.AllDirectories);
            var sorted = files
                .OrderBy(f =>
                {
                    var name = Path.GetFileNameWithoutExtension(f);
                    var match = Regex.Match(name, @"\d+");
                    return match.Success ? int.Parse(match.Value) : int.MaxValue;
                })
                .ToArray();

            var last = 0;
            for (int i = 0; i < sorted.Length; i++)
            {
                var file = new FileInfo(sorted[i]);
                var id = file.Name.Replace(".bytes", "").Replace(".json", "").Split("_")[1];

                if (int.Parse(id) > 200)
                {
                    levelConfig.levelInfos[last].levelID = int.Parse(id);
                    levelConfig.levelInfos[last].level = (ushort)(last + 1);
                    last++;
                }

            }
            for (int i = 0; i < sorted.Length; i++)
            {
                var file = new FileInfo(sorted[i]);
                var id = file.Name.Replace(".bytes", "").Replace(".json", "").Split("_")[1];
                if (int.Parse(id) < 200)
                {
                    levelConfig.levelInfos[last].levelID = int.Parse(id);
                    levelConfig.levelInfos[last].level = (ushort)(last + 1);
                    last++;
                }
            }
            levelConfig.levelInfos = levelConfig.levelInfos.Take(files.Length).ToArray();
            EditorUtility.SetDirty(levelConfig);
        }
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("To Csv"))
        {
            var ss = $"Level,Level Id,Spline Id, backgroundID,levelType,MechanicUnlock,isMission\n";
            var ls = new List<LevelInfo>(levelConfig.levelInfos);
            var uniqueLs = ls.Where(x => x != null).GroupBy(x => x.levelID).Select(g => g.First()).ToList();
            foreach (var i in uniqueLs)
            {
                ss += $"{i.level},{i.levelID},{i.splineId},{i.backgroundID},{(int)i.levelType},{i.isMission}\n";
            }
            File.WriteAllText("levelConfig.csv", ss);
        }
        if (GUILayout.Button("From Csv"))
        {
            var path = EditorUtility.OpenFilePanel("Select CSV", "", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                var lines = File.ReadAllLines(path);
                var list = new List<LevelInfo>();
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;
                    var cols = line.Split(',');

                    var info = new LevelInfo
                    {
                        level = ushort.Parse(cols[0].Trim()),
                        levelID = int.Parse(cols[1].Trim()),
                        splineId = ushort.Parse(cols[2].Trim()),
                        backgroundID = int.Parse(cols[3].Trim()),
                        levelType = (LevelType)int.Parse(cols[4].Trim()),
                        isMission = bool.Parse(cols[5].Trim())
                    };
                    list.Add(info);
                }

                levelConfig.levelInfos = list.ToArray();
                EditorUtility.SetDirty(levelConfig);
                Debug.Log($"Imported {list.Count} levels from CSV");
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Export Databuckets (Lua)"))
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("local myConfigTable = {");
            sb.AppendLine("    levelInfos = {");
            foreach (var info in levelConfig.levelInfos)
            {
                sb.AppendLine("        {");
                sb.AppendLine($"            l = {info.level},");
                sb.AppendLine($"            l2 = {info.levelID},");
                sb.AppendLine($"            sl2 = {info.splineId},");
                sb.AppendLine($"            b = {info.backgroundID},");
                sb.AppendLine($"            t = {(int)info.levelType},");
                sb.AppendLine($"            im = {(info.isMission ? "true" : "false")},");
                sb.AppendLine($"            bd = \"{info.base64Data}\"");
                sb.AppendLine("        },");
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");
            File.WriteAllText("Databuckets_Config.lua", sb.ToString());
            Debug.Log("Exported to Databuckets_Config.lua in Project root");
        }
        
        if (GUILayout.Button("Export Databuckets (JSON)"))
        {
            var data = new LevelConfigDataNew { levelInfos = levelConfig.levelInfos };
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText("Databuckets_Config.json", json);
            Debug.Log("Exported to Databuckets_Config.json in Project root");
        }
        GUILayout.EndHorizontal();

        base.OnInspectorGUI();
    }

}
#endif
