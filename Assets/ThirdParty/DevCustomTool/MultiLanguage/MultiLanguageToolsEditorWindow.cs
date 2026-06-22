#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class MultiLanguageToolsWindow : EditorWindow
{
    private string googleSheetLink = "";
    private string exportPath = "";
    private string cacheExportPathKey = "MLangTools_ExportPath";
    private string cacheSheetLinkKey = "MLangTools_SheetLink";

    private readonly string defaultPathExport = $"Assets/GamePlugin/Resources/Langs";

    [MenuItem("MyTools/Multi Language Key Exporter", priority = 0)]
    private static void ShowWindow()
    {
        var window = GetWindow<MultiLanguageToolsWindow>();
        window.titleContent = new GUIContent("Lang Key Export");
        window.minSize = new Vector2(500, 200);
        window.Show();
    }

    private void OnEnable()
    {
        googleSheetLink = EditorPrefs.GetString(cacheSheetLinkKey, "");
        exportPath = EditorPrefs.GetString(cacheExportPathKey, defaultPathExport);
    }

    private void OnDisable()
    {
        EditorPrefs.SetString(cacheSheetLinkKey, googleSheetLink);
        EditorPrefs.SetString(cacheExportPathKey, exportPath);
    }

    private void OnGUI()
    {
        GUILayout.Label("Google Sheet Link:", EditorStyles.boldLabel);
        googleSheetLink = EditorGUILayout.TextField("Sheet Link:", googleSheetLink);

        EditorGUILayout.Space(5);

        GUILayout.Label("Folder lưu file txt:", EditorStyles.boldLabel);
        exportPath = EditorGUILayout.TextField("Export Folder:", exportPath);

        EditorGUILayout.Space(5);
        if (GUILayout.Button("Chọn thư mục lưu...", GUILayout.Height(24)))
        {
            string selPath = EditorUtility.OpenFolderPanel("Chọn thư mục lưu file txt", exportPath, "");
            if (!string.IsNullOrEmpty(selPath))
                exportPath = selPath;
        }

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Export Multi Lang TXT từ Google Sheet", GUILayout.Height(40)))
        {
            ExportLangSheetToTxt();
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Generate CSV từ Local Lang Files (để paste vào Sheet)", GUILayout.Height(40)))
        {
            GenerateCsvFromLocalLangs();
        }

        EditorGUILayout.Space(20);
        GUILayout.Label("Tips:", EditorStyles.boldLabel);
        GUILayout.Label(
            "1. Muốn dùng được thì bắt buộc phải để link google sheet cho mọi người xem,\r\n không bắt buộc quyền chỉnh sửa sheet");
        GUILayout.Label("2. Dòng 1 trong sheet là header, có các giá trị tối thiểu là các cột key, default");
        GUILayout.Label(
            "3. Dòng 2 trong sheet là checkbox, dùng để tích chọn các ngôn ngữ cần export,\r\n nếu không tích thì sẽ xoá file ngôn ngữ đó nếu check là tồn tại");
    }

    private void ExportLangSheetToTxt()
    {
        List<string> errorMessages = new List<string>();

        if (string.IsNullOrEmpty(googleSheetLink) || string.IsNullOrEmpty(exportPath))
        {
            EditorUtility.DisplayDialog("Thiếu thông tin", "Vui lòng nhập đủ link Google Sheet và thư mục lưu!", "OK");
            return;
        }

        if (!exportPath.Replace("\\", "/").StartsWith("Assets"))
        {
            EditorUtility.DisplayDialog("Sai thư mục!", "Thư mục export phải nằm trong folder 'Assets' để Unity nhận!",
                "OK");
            return;
        }

        EditorPrefs.SetString(cacheSheetLinkKey, googleSheetLink);
        EditorPrefs.SetString(cacheExportPathKey, exportPath);

        try
        {
            string csvUrl = ConvertToCSVUrl(googleSheetLink);
            string tempCsvPath = Path.Combine(Application.temporaryCachePath, "temp_multilang_sheet.csv");
            using (var wc = new WebClient())
                wc.DownloadFile(csvUrl, tempCsvPath);

            var lines = GetCsvRecords(tempCsvPath);
            if (lines.Count < 3)
            {
                EditorUtility.DisplayDialog("Lỗi",
                    "Sheet không đủ dữ liệu (cần ít nhất 3 dòng: header, checkbox, data)!", "OK");
                return;
            }

            string[] headers = SplitCsvLine(lines[0]);
            string[] checkFlags = SplitCsvLine(lines[1]);
            int nCol = headers.Length;

            Dictionary<string, List<string>> langDict = new Dictionary<string, List<string>>();

            for (int col = 0; col < nCol; col++) // Bắt đầu từ 0 để lấy cả key
            {
                string lang = CleanTextHeadAndTailOnly(headers[col].Trim());
                bool isKeyOrDefault = lang == "key" || lang == "default";
                bool export = isKeyOrDefault || (checkFlags.Length > col && checkFlags[col].Trim().ToLower() == "true");

                if (!string.IsNullOrEmpty(lang))
                {
                    if (export)
                    {
                        langDict[lang] = new List<string>();
                    }
                    else
                    {
                        string outPath = Path.Combine(exportPath, $"{lang}.txt");
                        if (File.Exists(outPath))
                            File.Delete(outPath);
                    }
                }
            }

            Dictionary<string, int> keyLineMap = new Dictionary<string, int>();

            for (int row = 2; row < lines.Count; row++)
            {
                var cols = SplitCsvLine(lines[row]);
                if (cols.Length == 0) continue;

                string key = (cols.Length > 0) ? CleanTextHeadAndTailOnly(cols[0].Trim()) : "";
                string defaultVal = (cols.Length > 1) ? CleanTextHeadAndTailOnly(cols[1].Trim()) : "";

                // ❌ Key rỗng
                if (string.IsNullOrEmpty(key))
                {
                    string msg = $"⚠️ Key bị thiếu tại dòng {row + 1} trong sheet.";
                    LogWarning(msg);
                    errorMessages.Add(msg);
                    continue;
                }

                // ❌ Default trống
                if (langDict.ContainsKey("default") && string.IsNullOrEmpty(defaultVal))
                {
                    string msg = $"⚠️ Default value bị thiếu tại dòng {row + 1} (key: {key})";
                    LogWarning(msg);
                    errorMessages.Add(msg);
                }

                // ❌ Key trùng
                if (keyLineMap.TryGetValue(key, out var firstLine))
                {
                    string msg =
                        $"⚠️ Trùng key \"{key}\" tại dòng {row + 1} (đã khai báo ở dòng {firstLine}) — dòng này sẽ bị bỏ qua.";
                    LogWarning(msg);
                    errorMessages.Add(msg);
                    continue; // 🚫 Bỏ qua dòng trùng
                }
                else
                {
                    keyLineMap[key] = row + 1;
                }

                // Tiếp tục xử lý export
                for (int col = 0; col < nCol; col++)
                {
                    string lang = CleanTextHeadAndTailOnly(headers[col].Trim());
                    if (!langDict.ContainsKey(lang)) continue;
                    string value = (cols.Length > col && cols[col] != null)
                        ? CleanTextHeadAndTailOnly(cols[col].Trim())
                        : "";
                    value = EscapeJson(value);
                    langDict[lang].Add($"  \"{EscapeJson(key)}\": \"{value}\"");
                }
            }

            foreach (var pair in langDict)
            {
                if (pair.Key.ToLower() == "key") continue; // 🚫 Bỏ qua file key.txt

                string outPath = Path.Combine(exportPath, $"{pair.Key}.txt");
                using StreamWriter sw = new StreamWriter(outPath, false, System.Text.Encoding.UTF8);
                sw.WriteLine("{");
                sw.WriteLine(string.Join(",\n", pair.Value));
                sw.WriteLine("}");
            }

            EditorUtility.DisplayDialog("Thành công!", "Đã xuất các file txt vào: " + exportPath, "OK");
            AssetDatabase.Refresh();
            RefreshAssetDatabaseAfterDelay(1f);
        }
        catch (Exception ex)
        {
            errorMessages.Add(ex.Message);
        }

        if (errorMessages.Count > 0)
        {
            string all = string.Join("\n", errorMessages);
            EditorUtility.DisplayDialog("⚠️ Có lỗi trong quá trình export", all, "OK");
        }
    }

    private void GenerateCsvFromLocalLangs()
    {
        if (string.IsNullOrEmpty(exportPath))
        {
            EditorUtility.DisplayDialog("Thiếu thông tin", "Vui lòng nhập thư mục chứa các file lang!", "OK");
            return;
        }

        string fullPath = exportPath.Replace("\\", "/");
        if (!fullPath.StartsWith("Assets"))
        {
            EditorUtility.DisplayDialog("Sai thư mục!", "Thư mục phải nằm trong folder 'Assets'!", "OK");
            return;
        }

        string absFolder = Path.Combine(Application.dataPath, fullPath.Substring("Assets".Length).TrimStart('/', '\\'));
        if (!Directory.Exists(absFolder))
        {
            EditorUtility.DisplayDialog("Lỗi", $"Thư mục không tồn tại: {absFolder}", "OK");
            return;
        }

        var txtFiles = Directory.GetFiles(absFolder, "*.txt");
        if (txtFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy file .txt nào trong thư mục!", "OK");
            return;
        }

        // Parse all lang files into dictionaries
        var langData = new Dictionary<string, Dictionary<string, string>>();
        var allKeys = new List<string>();
        var keySet = new HashSet<string>();

        // Ensure "default" is processed first to establish key order
        var sortedFiles = new List<string>(txtFiles);
        sortedFiles.Sort((a, b) =>
        {
            string nameA = Path.GetFileNameWithoutExtension(a);
            string nameB = Path.GetFileNameWithoutExtension(b);
            if (nameA == "default") return -1;
            if (nameB == "default") return 1;
            return string.Compare(nameA, nameB, StringComparison.Ordinal);
        });

        foreach (var filePath in sortedFiles)
        {
            string langName = Path.GetFileNameWithoutExtension(filePath);
            var dict = ParseLangFile(filePath);
            if (dict == null)
            {
                LogWarning($"Không parse được file: {filePath}");
                continue;
            }

            langData[langName] = dict;

            // Collect keys in order of first appearance (default file first)
            foreach (var key in dict.Keys)
            {
                if (keySet.Add(key))
                    allKeys.Add(key);
            }
        }

        if (allKeys.Count == 0)
        {
            EditorUtility.DisplayDialog("Lỗi", "Không có dữ liệu nào trong các file lang!", "OK");
            return;
        }

        // Build column order matching Google Sheet: key, default, vi, ar, it, de, fr, he, ru, pt, ja, ko
        var columnOrder = new[] { "default", "vi", "ar", "it", "de", "fr", "he", "ru", "pt", "ja", "ko" };
        var langNames = new List<string>();
        foreach (var lang in columnOrder)
        {
            if (langData.ContainsKey(lang)) langNames.Add(lang);
        }
        // Append any extra langs not in the predefined order
        foreach (var lang in langData.Keys)
        {
            if (!langNames.Contains(lang)) langNames.Add(lang);
        }

        // Build CSV
        var sb = new System.Text.StringBuilder();

        // Row 1: headers
        sb.Append("key");
        foreach (var lang in langNames)
        {
            sb.Append(',');
            sb.Append(lang);
        }
        sb.AppendLine();

        // Row 2: checkboxes (all TRUE)
        sb.Append("TRUE"); // for key column
        foreach (var lang in langNames)
        {
            sb.Append(",TRUE");
        }
        sb.AppendLine();

        // Row 3+: data
        foreach (var key in allKeys)
        {
            sb.Append(EscapeCsvCell(key));
            foreach (var lang in langNames)
            {
                sb.Append(',');
                if (langData.TryGetValue(lang, out var dict) && dict.TryGetValue(key, out var val))
                    sb.Append(EscapeCsvCell(val));
            }
            sb.AppendLine();
        }

        // Save CSV
        string csvOutPath = Path.Combine(absFolder, "_langs_export.csv");
        File.WriteAllText(csvOutPath, sb.ToString(), System.Text.Encoding.UTF8);

        EditorUtility.DisplayDialog("Thành công!",
            $"Đã tạo CSV với {allKeys.Count} keys, {langNames.Count} ngôn ngữ.\n" +
            $"File: {csvOutPath}\n\n" +
            "Mở file CSV → Copy tất cả → Paste vào Google Sheet.", "OK");

        // Reveal in explorer
        EditorUtility.RevealInFinder(csvOutPath);
    }

    private static Dictionary<string, string> ParseLangFile(string filePath)
    {
        var dict = new Dictionary<string, string>();
        var content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);

        // Match JSON key-value pairs: "key": "value"
        var matches = Regex.Matches(content, @"""([^""\\]*(?:\\.[^""\\]*)*)""\s*:\s*""([^""\\]*(?:\\.[^""\\]*)*)""");
        foreach (Match m in matches)
        {
            string key = UnescapeJsonValue(m.Groups[1].Value);
            string val = UnescapeJsonValue(m.Groups[2].Value);
            if (!dict.ContainsKey(key))
                dict[key] = val;
        }

        return dict;
    }

    private static string UnescapeJsonValue(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\\"", "\"").Replace("\\\\", "\\");
    }

    private static string EscapeCsvCell(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        // If cell contains comma, quote, or newline, wrap in quotes and escape internal quotes
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return '"' + value.Replace("\"", "\"\"") + '"';
        }
        return value;
    }

    private static void RefreshAssetDatabaseAfterDelay(float delaySeconds = 1f)
    {
        double startTime = EditorApplication.timeSinceStartup;
        EditorApplication.update += Update;

        void Update()
        {
            if (EditorApplication.timeSinceStartup - startTime >= delaySeconds)
            {
                EditorApplication.update -= Update;
                AssetDatabase.Refresh();
                Log($"AssetDatabase refreshed after {delaySeconds} seconds.");
            }
        }
    }

    static string CleanTextHeadAndTailOnly(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";

        // Xoá khoảng trắng thừa đầu/cuối
        s = s.Trim(' ', '\t', '\r', '\n');

        // Nếu có cặp " bao quanh thì bỏ
        if (s.Length > 1 && s.StartsWith("\"") && s.EndsWith("\""))
        {
            s = s.Substring(1, s.Length - 2);
        }

        return s;
    }


    private static List<string> GetCsvRecords(string csvPath)
    {
        var lines = File.ReadAllLines(csvPath);
        var result = new List<string>();
        string current = "";
        bool inQuotes = false;
        foreach (var line in lines)
        {
            if (current.Length > 0) current += "\n";
            current += line;
            int quoteCount = 0;
            for (int i = 0; i < line.Length; i++)
                if (line[i] == '"')
                    quoteCount++;
            inQuotes ^= (quoteCount % 2) != 0;
            if (!inQuotes)
            {
                result.Add(current);
                current = "";
            }
        }

        if (!string.IsNullOrEmpty(current))
            result.Add(current);
        return result;
    }

    private static string[] SplitCsvLine(string line)
    {
        var list = new List<string>();
        bool inQuotes = false;
        int start = 0;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    i++;
                else
                    inQuotes = !inQuotes;
            }
            else if (line[i] == ',' && !inQuotes)
            {
                list.Add(UnescapeCsvCell(line.Substring(start, i - start)));
                start = i + 1;
            }
        }

        if (start <= line.Length)
            list.Add(UnescapeCsvCell(line.Substring(start)));
        return list.ToArray();
    }

    private static string UnescapeCsvCell(string input)
    {
        input = input.Trim();
        if (input.StartsWith("\"") && input.EndsWith("\""))
            input = input.Substring(1, input.Length - 2).Replace("\"\"", "\"");
        return input;
    }

    private static string ConvertToCSVUrl(string url)
    {
        var match = Regex.Match(url, @"/d/([a-zA-Z0-9-_]+)");
        if (match.Success)
            return $"https://docs.google.com/spreadsheets/d/{match.Groups[1].Value}/export?format=csv";
        return url;
    }

    private static string EscapeJson(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\"", "\\\"");
    }

    #region Log API

#if UNITY_EDITOR
    private static readonly bool ENABLE_LOGGING = true;
#else
    private static readonly bool ENABLE_LOGGING = false;
#endif

    private static string LogRegion = $"{nameof(MultiLanguageToolsWindow)}";

    private static void Log(object message)
    {
        if (ENABLE_LOGGING)
        {
            Debug.Log($"[{LogRegion}] Log: {message}");
        }
    }

    private static void LogError(object message)
    {
        if (ENABLE_LOGGING)
        {
            Debug.LogError($"[{LogRegion}] LogError: {message}");
        }
    }

    private static void LogWarning(object message)
    {
        if (ENABLE_LOGGING)
        {
            Debug.LogWarning($"[{LogRegion}] LogWarning: {message}");
        }
    }

    #endregion
}
#endif