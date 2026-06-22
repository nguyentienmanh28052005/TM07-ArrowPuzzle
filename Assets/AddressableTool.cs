#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

public class AddressableTool : EditorWindow
{
    string groupA = "PopupAndScreenGroup";
    string groupB = "Default Local Group";
    string labelInput = "common"; // Có thể nhập nhiều label, ngăn cách dấu phẩy

    // Thêm input cho hàm mới
    string assetPaths = ""; // Assets/... ngăn cách dấu phẩy
    string groupTarget = "PopupAndScreenGroup";
    string labelTarget = "common_custom";

    [MenuItem("Tools/Addressable/Move Assets By Label")]
    static void ShowWindow() => GetWindow<AddressableTool>("Move Assets By Label");

    void OnGUI()
    {
        GUILayout.Label("Chuyển asset theo label", EditorStyles.boldLabel);
        groupA = EditorGUILayout.TextField("Group nguồn (A):", groupA);
        groupB = EditorGUILayout.TextField("Group đích (B):", groupB);
        labelInput = EditorGUILayout.TextField("Label (ngăn cách dấu phẩy):", labelInput);

        GUILayout.Space(10);
        if (GUILayout.Button("Move Now"))
            MoveAssets();
        if (GUILayout.Button("Remove Label(s)"))
            RemoveLabels();

        GUILayout.Space(20);
        GUILayout.Label("Gán Nhiều Asset vào 1 Group & Label", EditorStyles.boldLabel);
        assetPaths = EditorGUILayout.TextField("Asset Paths (dấu phẩy):", assetPaths);
        groupTarget = EditorGUILayout.TextField("Group đích:", groupTarget);
        labelTarget = EditorGUILayout.TextField("Label:", labelTarget);
        if (GUILayout.Button("Add Assets To Group & Label"))
            AddAssetsToGroupAndLabel();
    }

    void MoveAssets()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressables not initialized!");
            return;
        }

        var srcGroup = settings.FindGroup(groupA);
        var dstGroup = settings.FindGroup(groupB);

        if (srcGroup == null || dstGroup == null)
        {
            Debug.LogError("Không tìm thấy group nguồn hoặc group đích!");
            return;
        }

        var labels = labelInput.Split(',').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToList();
        int moveCount = 0;

        foreach (var entry in srcGroup.entries.ToList())
        {
            if (labels.Any(label => entry.labels.Contains(label)))
            {
                settings.MoveEntry(entry, dstGroup);
                moveCount++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Đã chuyển {moveCount} asset từ '{groupA}' sang '{groupB}' theo label bạn nhập.");
    }

    void RemoveLabels()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressables not initialized!");
            return;
        }

        var srcGroup = settings.FindGroup(groupA);
        if (srcGroup == null)
        {
            Debug.LogError("Không tìm thấy group nguồn!");
            return;
        }

        var labels = labelInput.Split(',').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToList();
        int removeCount = 0;

        foreach (var entry in srcGroup.entries)
        {
            foreach (var label in labels)
            {
                if (entry.labels.Contains(label))
                {
                    entry.SetLabel(label, false); // Xóa label
                    removeCount++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Đã xóa {removeCount} lần label (có thể trùng nếu asset có nhiều label) trong group '{groupA}'.");
    }

    // HÀM MỚI: Thêm nhiều asset vào group và label chỉ định
    void AddAssetsToGroupAndLabel()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressables not initialized!");
            return;
        }

        var group = settings.FindGroup(groupTarget);
        if (group == null)
        {
            group = settings.CreateGroup(groupTarget, false, false, false, null,
                typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema));
            Debug.Log("Created group: " + groupTarget);
        }

        var assets = assetPaths.Split(',').Select(a => a.Trim()).Where(a => !string.IsNullOrEmpty(a)).ToList();
        int addCount = 0;

        foreach (var assetPath in assets)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogWarning("Không tìm thấy asset: " + assetPath);
                continue;
            }

            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.SetLabel(labelTarget, true);
            addCount++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Đã add {addCount} asset vào group '{groupTarget}' với label '{labelTarget}'.");
    }
}

public class AddressablesAnalyzeJsonExport : EditorWindow
{
    string analyzePath = "";
    string exportPath = "Assets/addressables_analyze_export.csv";

    [MenuItem("Tools/Addressable/Export AnalyzeRuleData JSON")]
    static void ShowWindow() => GetWindow<AddressablesAnalyzeJsonExport>("Export AnalyzeRuleData");

    void OnGUI()
    {
        GUILayout.Label("Export AnalyzeRuleData.json", EditorStyles.boldLabel);

        // Browse for input file
        GUILayout.BeginHorizontal();
        analyzePath = EditorGUILayout.TextField("AnalyzeRuleData.json Path", analyzePath);
        if (GUILayout.Button("Browse...", GUILayout.Width(80)))
        {
            string chosen = EditorUtility.OpenFilePanel("Chọn file AnalyzeRuleData.json", "", "json");
            if (!string.IsNullOrEmpty(chosen))
                analyzePath = chosen;
        }

        GUILayout.EndHorizontal();

        // Browse for output file
        GUILayout.BeginHorizontal();
        exportPath = EditorGUILayout.TextField("Export CSV Path", exportPath);
        if (GUILayout.Button("Browse...", GUILayout.Width(80)))
        {
            string chosen =
                EditorUtility.SaveFilePanel("Chọn nơi lưu file CSV", "", "addressables_analyze_export.csv", "csv");
            if (!string.IsNullOrEmpty(chosen))
                exportPath = chosen;
        }

        GUILayout.EndHorizontal();

        if (GUILayout.Button("Export"))
        {
            Export();
        }
    }

    void Export()
    {
        if (!File.Exists(analyzePath))
        {
            Debug.LogError("Không tìm thấy AnalyzeRuleData.json ở: " + analyzePath);
            return;
        }

        string json = File.ReadAllText(analyzePath);

        // Parse file: lấy đúng trường "m_RuleToResults"
        var data = JsonUtility.FromJson<AnalyzeRuleRoot>("{\"m_RuleToResults\":" +
                                                         JsonExtractArray(json, "m_RuleToResults") + "}");

        // Parse asset-label map
        var assetToLabels = new Dictionary<string, HashSet<string>>();
        foreach (var rule in data.m_RuleToResults)
        {
            if (rule.Results == null) continue;
            foreach (var res in rule.Results)
            {
                var parts = res.m_ResultName.Split(new char[] { ':' }, 3);
                if (parts.Length == 3)
                {
                    var bundle = parts[1].Replace(".bundle", "").Trim();
                    var asset = parts[2].Trim();
                    if (!assetToLabels.ContainsKey(asset)) assetToLabels[asset] = new HashSet<string>();
                    assetToLabels[asset].Add(bundle);
                }
            }
        }

        // Gom nhóm theo cùng label set
        var labelGroup = new Dictionary<string, List<string>>();
        foreach (var kv in assetToLabels)
        {
            var labelKey = string.Join(",", kv.Value.OrderBy(x => x));
            if (!labelGroup.ContainsKey(labelKey)) labelGroup[labelKey] = new List<string>();
            labelGroup[labelKey].Add(kv.Key);
        }

        // Xuất file CSV
        using (var sw = new StreamWriter(exportPath, false, System.Text.Encoding.UTF8))
        {
            sw.WriteLine("Assets,Labels,Count Label,Count Assets");
            foreach (var kv in labelGroup.OrderByDescending(x => x.Key.Split(',').Length))
            {
                var assets = string.Join(", ", kv.Value.OrderBy(a => a));
                var labels = kv.Key;
                var countLabel = string.IsNullOrEmpty(labels) ? 0 : labels.Split(',').Length;
                var countAssets = kv.Value.Count;
                sw.WriteLine($"\"{assets}\",\"{labels}\",{countLabel},{countAssets}");
            }
        }

        Debug.Log("Exported grouped data to: " + exportPath);
        AssetDatabase.Refresh();
    }

    // Helper: parse array field from JSON string (thủ công, đơn giản)
    string JsonExtractArray(string json, string key)
    {
        int idx = json.IndexOf($"\"{key}\":");
        if (idx == -1) return "[]";
        int start = json.IndexOf('[', idx);
        int end = json.LastIndexOf(']');
        return json.Substring(start, end - start + 1);
    }

    [System.Serializable]
    public class AnalyzeRuleRoot
    {
        public List<RuleResult> m_RuleToResults;
    }

    [System.Serializable]
    public class RuleResult
    {
        public string RuleName;
        public List<RuleResultEntry> Results;
    }

    [System.Serializable]
    public class RuleResultEntry
    {
        public string m_ResultName;
        public int m_Severity;
    }
}

public class PrefabLabelAssetChecker : EditorWindow
{
    private string labelName = "";
    private string assetsList = ""; // nhập list asset path, mỗi dòng 1 cái

    private Vector2 scroll;
    private List<string> results = new List<string>();

    [MenuItem("Tools/Addressable/Check Prefabs in Label using Assets")]
    public static void ShowWindow()
    {
        GetWindow<PrefabLabelAssetChecker>("Prefab Label Asset Checker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Tìm prefab trong Label có dùng Asset(s)", EditorStyles.boldLabel);

        labelName = EditorGUILayout.TextField("Label", labelName);

        GUILayout.Label("Danh sách Asset Paths (mỗi dòng 1 path):");
        assetsList = EditorGUILayout.TextArea(assetsList, GUILayout.Height(100));

        if (GUILayout.Button("Check"))
        {
            if (string.IsNullOrEmpty(labelName))
            {
                Debug.LogWarning("Hãy nhập tên Label!");
                return;
            }

            if (string.IsNullOrEmpty(assetsList))
            {
                Debug.LogWarning("Hãy nhập ít nhất 1 asset path!");
                return;
            }

            results.Clear();

            string[] assetPaths = assetsList.Split(new[] { '\n', '\r', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string rawPath in assetPaths)
            {
                string assetPath = rawPath.Trim();
                if (!string.IsNullOrEmpty(assetPath))
                {
                    results.AddRange(CheckPrefabsInLabel(labelName, assetPath));
                }
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("Kết quả:", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(300));
        if (results.Count == 0)
        {
            GUILayout.Label("Chưa có kết quả hoặc không tìm thấy Prefab nào.");
        }
        else
        {
            foreach (string line in results)
            {
                EditorGUILayout.TextField(line);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private List<string> CheckPrefabsInLabel(string label, string assetPath)
    {
        var found = new List<string>();
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            found.Add("Không tìm thấy AddressableAssetSettings!");
            return found;
        }

        foreach (var group in settings.groups)
        {
            foreach (var entry in group.entries)
            {
                if (entry == null) continue;
                if (!entry.labels.Contains(label)) continue;

                string prefabPath = AssetDatabase.GUIDToAssetPath(entry.guid);
                if (string.IsNullOrEmpty(prefabPath)) continue;

                if (!prefabPath.EndsWith(".prefab")) continue;

                string[] dependencies = AssetDatabase.GetDependencies(prefabPath, true);
                foreach (string dep in dependencies)
                {
                    if (dep == assetPath)
                    {
                        found.Add(
                            $"Asset: {Path.GetFileName(assetPath)} | Prefab: {prefabPath} | Group: {group.Name} | Labels: {string.Join(",", entry.labels)}"
                        );
                    }
                }
            }
        }
        return found;
    }
}
public class AddressableBatchRenamer : EditorWindow
{
    [MenuItem("Tools/Addressables/Batch Rename Levels")]
    public static void ShowWindow()
    {
        GetWindow<AddressableBatchRenamer>("Batch Rename Levels");
    }

    private string folderPath = "Assets/_Game/Resources_moved/DataLevel";
    private string prefix = "Level_";
    private string extension = ".bytes";

    private void OnGUI()
    {
        GUILayout.Label("Batch Rename Addressables", EditorStyles.boldLabel);
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);
        prefix = EditorGUILayout.TextField("Prefix", prefix);
        extension = EditorGUILayout.TextField("Extension", extension);

        if (GUILayout.Button("Rename Now"))
        {
            RenameAll();
        }
    }

    private void RenameAll()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Cannot find AddressableAssetSettings!");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { folderPath });
        int count = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            var entry = settings.FindAssetEntry(guid);

            if (entry != null)
            {
                // Nếu tên file là Level_x.bytes → chỉ giữ phần số
                string newAddress = fileName;
                if (fileName.StartsWith(prefix))
                    newAddress = fileName; // hoặc tùy ý: fileName.Replace(prefix, "")
                else
                    newAddress = prefix + fileName;

                entry.SetAddress(newAddress);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, null, true);
        Debug.Log($"✅ Renamed {count} addressable entries in folder: {folderPath}");
    }
    public class AddressableLevelGrouper : EditorWindow
{
    private int levelsPerGroup = 50;
    private string labelPrefix = "level_group_";

    [MenuItem("Tools/Addressables/Group Levels by Range")]
    public static void ShowWindow()
    {
        GetWindow<AddressableLevelGrouper>("Group Levels");
    }

    private void OnGUI()
    {
        GUILayout.Label("Group Levels into Addressable Labels", EditorStyles.boldLabel);
        levelsPerGroup = EditorGUILayout.IntField("Levels per Group", levelsPerGroup);
        labelPrefix = EditorGUILayout.TextField("Label Prefix", labelPrefix);

        if (GUILayout.Button("Assign Labels"))
        {
            AssignLabels();
        }
    }

    private void AssignLabels()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("⚠️ Addressable settings not found!");
            return;
        }

        var entries = settings.groups
            .SelectMany(g => g.entries)
            .Where(e => e.address.StartsWith("Level_"))
            .OrderBy(e => ExtractLevelNumber(e.address))
            .ToList();

        if (entries.Count == 0)
        {
            Debug.LogWarning("No entries found with prefix 'Level_'!");
            return;
        }

        int groupIndex = 1;
        int count = 0;

        foreach (var entry in entries)
        {
            int levelNum = ExtractLevelNumber(entry.address);
            if (levelNum == -1) continue;

            int currentGroup = Mathf.CeilToInt(levelNum / (float)levelsPerGroup);
            string label = $"{labelPrefix}{currentGroup}";
            

            // Xóa label cũ dạng level_group_*
            foreach (var oldLabel in entry.labels.ToList())
            {
                if (oldLabel.StartsWith(labelPrefix))
                    entry.SetLabel(oldLabel, false);
            }

            // Gán label mới
            entry.SetLabel(label, true);
            count++;
        }

        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, null, true);
        AssetDatabase.SaveAssets();
        Debug.Log($"✅ Assigned {count} entries into groups of {levelsPerGroup}.");
    }

    private int ExtractLevelNumber(string address)
    {
        string digits = new string(address.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out int num) ? num : -1;
    }
}
}
#endif