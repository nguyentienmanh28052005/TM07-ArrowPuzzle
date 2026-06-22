#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class IAPDataToolsWindow : EditorWindow
{
    // ================== CONFIG ==================
    private string googleSheetLink = "";

    // 2 biến này giờ chỉ dùng nội bộ, không cho sửa trong GUI
    private string exportFolderPath = "";
    private string exportFileName = "data.json";

    private readonly string cacheSheetLinkKey = "IAPTools_SheetLink";

    // Thư mục default trong Assets
    private readonly string defaultPathExport = "Assets/GamePlugin/Resources/Inapp";

#if UNITY_ANDROID
    private readonly string platform = "Android";
#elif UNITY_IOS
    private readonly string platform = "iOS";
#else
    private readonly string platform = "Default";
#endif

    // Kết quả: Assets/GamePlugin/Resources/Inapp/<Platform>
    public string PathFile => Path.Combine(defaultPathExport, platform);

    // Prefix để build pkg nếu cột pkg trống (user có thể override trong UI)
    private string packagePrefix = "";

    // ================== MENU ==================
    [MenuItem("MyTools/IAP Data Exporter", priority = 10)]
    private static void ShowWindow()
    {
        var window = GetWindow<IAPDataToolsWindow>();
        window.titleContent = new GUIContent("IAP Data Export");
        window.minSize = new Vector2(500, 220);
        window.Show();
    }

    private void OnEnable()
    {
        googleSheetLink = EditorPrefs.GetString(cacheSheetLinkKey, "");

        // Luôn dùng đường dẫn & tên file mặc định
        exportFolderPath = PathFile.Replace("\\", "/");
        exportFileName = "data.json";
    }

    private void OnDisable()
    {
        EditorPrefs.SetString(cacheSheetLinkKey, googleSheetLink);
    }

    // ================== GUI ==================
    private void OnGUI()
    {
        GUILayout.Label("Google Sheet Link:", EditorStyles.boldLabel);
        googleSheetLink = EditorGUILayout.TextField("Sheet Link:", googleSheetLink);

        EditorGUILayout.Space(5);

        GUILayout.Label("Đường dẫn export (cố định):", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Export Folder:", exportFolderPath);
        EditorGUILayout.LabelField("Export File:", exportFileName);

        EditorGUILayout.Space(5);
        packagePrefix = EditorGUILayout.TextField("Package Prefix (optional):", packagePrefix);

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Export IAP Data từ Google Sheet", GUILayout.Height(40)))
        {
            ExportIAPSheetToJson();
        }

        EditorGUILayout.Space(15);
        GUILayout.Label("Tips:", EditorStyles.boldLabel);
        GUILayout.Label("1. Sheet phải cho phép mọi người xem (không nhất thiết cho sửa).");
        GUILayout.Label(
            "2. Header ví dụ: type, id, price, pkg (optional), removeads, money-gold, items-time, items-hint, items-eye.");
        GUILayout.Label("3. Các cột bắt đầu bằng 'money-' sẽ gộp vào receiver.money (vd: money-gold, money-gem).");
        GUILayout.Label("4. Các cột bắt đầu bằng 'items-' sẽ gộp vào receiver.items (vd: items-time, items-hint...).");
        GUILayout.Label("5. CSV debug + JSON cache lưu ở Application.temporaryCachePath.");
        GUILayout.Label("6. Nếu Package Prefix để trống, sẽ tự lấy từ PlayerSettings.applicationIdentifier.");
    }

    /// <summary>
    /// Lấy prefix để build pkg:
    /// - Nếu user điền packagePrefix trong tool -> dùng cái đó
    /// - Nếu để trống -> lấy PlayerSettings.applicationIdentifier + "."
    /// - Nếu vẫn rỗng -> trả về "" (khi đó pkg = id)
    /// </summary>
    private string GetAutoPackagePrefix()
    {
        // Nếu user đã gõ vào field -> ưu tiên dùng
        if (!string.IsNullOrEmpty(packagePrefix))
        {
            string p = packagePrefix.Trim();
            p = p.TrimEnd('.');
            return p.Length > 0 ? p + "." : "";
        }

        // Ngược lại lấy từ Unity PlayerSettings
        string appId = PlayerSettings.applicationIdentifier;
        if (string.IsNullOrEmpty(appId))
            return "";

        appId = appId.Trim().TrimEnd('.');
        return appId.Length > 0 ? appId + "." : "";
    }

    // ================== MAIN EXPORT ==================
    private void ExportIAPSheetToJson()
    {
        List<string> errorMessages = new List<string>();

        // Luôn set lại cho chắc
        exportFolderPath = PathFile.Replace("\\", "/");
        exportFileName = "data.json";

        if (string.IsNullOrEmpty(googleSheetLink))
        {
            EditorUtility.DisplayDialog("Thiếu thông tin",
                "Vui lòng nhập link Google Sheet!", "OK");
            return;
        }

        // Chuẩn hoá đường dẫn để check "Assets"
        string unityPath = exportFolderPath.Replace("\\", "/");
        int assetsIndex = unityPath.IndexOf("Assets", StringComparison.Ordinal);
        if (assetsIndex >= 0)
        {
            // Dù PathFile đã là relative nhưng cứ làm cho chắc
            unityPath = unityPath.Substring(assetsIndex);
            exportFolderPath = unityPath;
        }

        if (!unityPath.StartsWith("Assets"))
        {
            EditorUtility.DisplayDialog("Sai thư mục!",
                "Thư mục export phải nằm trong folder 'Assets' để Unity nhận!",
                "OK");
            return;
        }

        try
        {
            string csvUrl = ConvertToCSVUrl(googleSheetLink);
            string tempCsvPath = Path.Combine(Application.temporaryCachePath, "temp_iap_sheet.csv");
            using (var wc = new WebClient())
            {
                wc.DownloadFile(csvUrl, tempCsvPath);
            }

            // Đọc CSV thành các record (multi-line cells vẫn giữ)
            var lines = GetCsvRecords(tempCsvPath);
            if (lines.Count < 2)
            {
                EditorUtility.DisplayDialog("Lỗi",
                    "Sheet không đủ dữ liệu (cần ít nhất 2 dòng: header + data)!",
                    "OK");
                return;
            }

            // Ghi thêm file CSV debug để inspect khi lỗi (temp)
            string csvDebugName = "iap_sheet_debug.csv";
            string csvDebugPath = Path.Combine(Application.temporaryCachePath, csvDebugName);
            try
            {
                File.Copy(tempCsvPath, csvDebugPath, true);
                Log($"CSV debug saved at: {csvDebugPath}");
            }
            catch (Exception e)
            {
                LogWarning($"Không ghi được file CSV debug: {e.Message}");
                errorMessages.Add("Không ghi được file CSV debug: " + e.Message);
            }

            // Header
            string[] headersRaw = SplitCsvLine(lines[0]);
            int nCol = headersRaw.Length;

            // Map header -> index (lowercase)
            Dictionary<string, int> headerIndex = new Dictionary<string, int>();
            for (int i = 0; i < nCol; i++)
            {
                string h = CleanTextHeadAndTailOnly(headersRaw[i]).ToLower();
                if (!string.IsNullOrEmpty(h) && !headerIndex.ContainsKey(h))
                {
                    headerIndex.Add(h, i);
                }
            }

            // Alias cho "pkg (optional)" -> "pkg"
            if (!headerIndex.ContainsKey("pkg"))
            {
                foreach (var kv in headerIndex)
                {
                    if (kv.Key.StartsWith("pkg"))
                    {
                        headerIndex["pkg"] = kv.Value;
                        break;
                    }
                }
            }

            // Danh sách IAP
            var nonConsumList = new List<IAPItemNonConsum>();
            var consumList = new List<IAPItemConsum>();

            // Duyệt từng dòng data
            for (int row = 1; row < lines.Count; row++)
            {
                var cols = SplitCsvLine(lines[row]);
                if (cols.Length == 0) continue;

                string GetCell(string colName)
                {
                    colName = colName.ToLower();
                    if (headerIndex.TryGetValue(colName, out int idx))
                    {
                        if (idx < cols.Length && cols[idx] != null)
                            return CleanTextHeadAndTailOnly(cols[idx]);
                    }

                    return "";
                }

                // ===== Lấy type / id và bỏ qua dòng không phải IAP =====
                string type = GetCell("type");
                string id = GetCell("id");

                // Nếu không có type hoặc id -> không phải dòng IAP -> bỏ qua, không log warning
                if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(id))
                {
                    continue;
                }

                string price = GetCell("price");
                if (string.IsNullOrEmpty(price))
                {
                    string msg = $"⚠️ Thiếu price tại dòng {row + 1} (id: {id}), bỏ qua.";
                    LogWarning(msg);
                    errorMessages.Add(msg);
                    continue;
                }

                string pkg = GetCell("pkg");
                if (string.IsNullOrEmpty(pkg))
                {
                    var prefix = GetAutoPackagePrefix();
                    if (string.IsNullOrEmpty(prefix))
                        pkg = id; // fallback: không có prefix thì dùng id trần
                    else
                        pkg = prefix + id;
                }

                // ===== Non-consumable (vd: remove ads, nhưng vẫn có thể có money/items) =====
                if (!string.IsNullOrEmpty(type) &&
                    type.Trim().ToLower().StartsWith("non"))
                {
                    // mặc định removeAds = 1
                    int removeAds = 1;
                    string removeAdsStr = GetCell("removeads");
                    if (!string.IsNullOrEmpty(removeAdsStr))
                    {
                        if (int.TryParse(removeAdsStr, out var tmp))
                        {
                            removeAds = tmp > 0 ? 1 : 0;
                        }
                    }

                    // Gom money-* và items-* giống consumable
                    List<string> moneyParts = new List<string>();
                    List<string> itemParts = new List<string>();

                    for (int colIndex = 0; colIndex < headersRaw.Length; colIndex++)
                    {
                        string headerRaw = headersRaw[colIndex];
                        if (string.IsNullOrEmpty(headerRaw))
                            continue;

                        string headerClean = CleanTextHeadAndTailOnly(headerRaw);
                        string headerLower = headerClean.ToLower();

                        if (headerLower.StartsWith("money-"))
                        {
                            string key = headerLower.Substring("money-".Length);
                            if (string.IsNullOrEmpty(key))
                                continue;

                            string cellValue = colIndex < cols.Length
                                ? CleanTextHeadAndTailOnly(cols[colIndex])
                                : "";

                            if (string.IsNullOrEmpty(cellValue))
                                continue;

                            moneyParts.Add($"{key}:{cellValue}");
                        }
                        else if (headerLower.StartsWith("items-"))
                        {
                            string key = headerLower.Substring("items-".Length);
                            if (string.IsNullOrEmpty(key))
                                continue;

                            string cellValue = colIndex < cols.Length
                                ? CleanTextHeadAndTailOnly(cols[colIndex])
                                : "";

                            if (string.IsNullOrEmpty(cellValue))
                                continue;

                            itemParts.Add($"{key}:{cellValue}");
                        }
                    }

                    string moneyStr = moneyParts.Count > 0
                        ? string.Join(",", moneyParts)
                        : "";

                    string itemsStr = itemParts.Count > 0
                        ? string.Join(",", itemParts)
                        : "";

                    var item = new IAPItemNonConsum
                    {
                        id = id,
                        pkg = pkg,
                        price = price,
                        receiver = new IAPReceiverNonConsum
                        {
                            removeads = removeAds,
                            money = string.IsNullOrEmpty(moneyStr) ? null : moneyStr,
                            items = string.IsNullOrEmpty(itemsStr) ? null : itemsStr
                        }
                    };
                    nonConsumList.Add(item);
                }
                else // ===== Consumable =====
                {
                    // Gom tất cả cột money-* vào receiver.money
                    List<string> moneyParts = new List<string>();
                    // Gom tất cả cột items-* vào receiver.items
                    List<string> itemParts = new List<string>();

                    for (int colIndex = 0; colIndex < headersRaw.Length; colIndex++)
                    {
                        string headerRaw = headersRaw[colIndex];
                        if (string.IsNullOrEmpty(headerRaw))
                            continue;

                        string headerClean = CleanTextHeadAndTailOnly(headerRaw);
                        string headerLower = headerClean.ToLower();

                        if (headerLower.StartsWith("money-"))
                        {
                            string key = headerLower.Substring("money-".Length);
                            if (string.IsNullOrEmpty(key))
                                continue;

                            string cellValue = colIndex < cols.Length
                                ? CleanTextHeadAndTailOnly(cols[colIndex])
                                : "";

                            if (string.IsNullOrEmpty(cellValue))
                                continue;

                            moneyParts.Add($"{key}:{cellValue}");
                        }
                        else if (headerLower.StartsWith("items-"))
                        {
                            string key = headerLower.Substring("items-".Length);
                            if (string.IsNullOrEmpty(key))
                                continue;

                            string cellValue = colIndex < cols.Length
                                ? CleanTextHeadAndTailOnly(cols[colIndex])
                                : "";

                            if (string.IsNullOrEmpty(cellValue))
                                continue;

                            itemParts.Add($"{key}:{cellValue}");
                        }
                    }

                    string moneyStr = moneyParts.Count > 0
                        ? string.Join(",", moneyParts)
                        : "";

                    string itemsStr = itemParts.Count > 0
                        ? string.Join(",", itemParts)
                        : "";

                    var receiver = new IAPReceiverConsum();
                    if (!string.IsNullOrEmpty(moneyStr))
                        receiver.money = moneyStr;
                    if (!string.IsNullOrEmpty(itemsStr))
                        receiver.items = itemsStr;

                    var item = new IAPItemConsum
                    {
                        id = id,
                        pkg = pkg,
                        price = price,
                        receiver = receiver
                    };
                    consumList.Add(item);
                }
            }

            // Nếu không parse được IAP nào => coi như lỗi, KHÔNG ghi đè file chính
            int totalItems = nonConsumList.Count + consumList.Count;
            if (totalItems == 0)
            {
                string msg =
                    "Không có IAP nào được parse từ sheet (non_consum + consum đều = 0). Kiểm tra lại cột 'type', 'id' và 'price'.";
                LogWarning(msg);
                errorMessages.Add(msg);
            }

            Log($"Parsed IAP: non_consum = {nonConsumList.Count}, consum = {consumList.Count}");

            // ========== BUILD JSON ==========
            string json = BuildJson(nonConsumList, consumList);

            // ========== GHI FILE JSON CACHE (DEBUG / PREVIEW) Ở TEMP ==========
            string cacheFileName = Path.GetFileNameWithoutExtension(exportFileName) + "_cache.json";
            string cachePath = Path.Combine(Application.temporaryCachePath, cacheFileName);

            try
            {
                using (StreamWriter swCache = new StreamWriter(cachePath, false, Encoding.UTF8))
                {
                    swCache.Write(json);
                }

                Log($"JSON cache saved at: {cachePath}");
            }
            catch (Exception e)
            {
                LogError($"Không ghi được file JSON cache: {e.Message}");
                errorMessages.Add("Không ghi được file JSON cache: " + e.Message);
            }

            // ========== CHỈ GHI ĐÈ FILE CHÍNH KHI KHÔNG CÓ LỖI/CẢNH BÁO ==========
            if (errorMessages.Count == 0)
            {
                string outPath = Path.Combine(exportFolderPath, exportFileName);
                using (StreamWriter sw = new StreamWriter(outPath, false, Encoding.UTF8))
                {
                    sw.Write(json);
                }

                EditorUtility.DisplayDialog("Thành công!",
                    "Đã export IAP data vào:\n" + outPath +
                    "\n\nFile CSV debug (tạm) lưu tại:\n" + csvDebugPath +
                    "\nFile JSON cache (tạm) lưu tại:\n" + cachePath,
                    "OK");

                AssetDatabase.Refresh();
                RefreshAssetDatabaseAfterDelay(1f);
            }
            else
            {
                string all = string.Join("\n", errorMessages);
                EditorUtility.DisplayDialog("⚠️ Có lỗi/cảnh báo khi parse",
                    "File CSV debug (tạm) lưu tại:\n" + csvDebugPath +
                    "\nFile JSON cache (tạm) lưu tại:\n" + cachePath +
                    "\n\nFile gốc " + exportFileName + " KHÔNG bị ghi đè." +
                    "\n\nChi tiết lỗi:\n" + all,
                    "OK");
            }
        }
        catch (Exception ex)
        {
            LogError(ex);
            EditorUtility.DisplayDialog("Lỗi nghiêm trọng",
                "Có exception khi export.\nChi tiết xem trong Console.\n\n" + ex.Message,
                "OK");
        }
    }

    // ================== JSON BUILD ==================
    private string BuildJson(List<IAPItemNonConsum> nonConsumList, List<IAPItemConsum> consumList)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine("  \"default\": {");

        // non_consum
        sb.AppendLine("    \"non_consum\": [");
        for (int i = 0; i < nonConsumList.Count; i++)
        {
            var it = nonConsumList[i];
            sb.AppendLine("      {");
            sb.AppendLine($"        \"id\": \"{EscapeJson(it.id)}\",");
            sb.AppendLine($"        \"pkg\": \"{EscapeJson(it.pkg)}\",");
            sb.AppendLine($"        \"price\": \"{EscapeJson(it.price)}\",");
            sb.AppendLine("        \"receiver\": {");

            bool hasRemoveAds = it.receiver.removeads != 0;
            bool hasMoney = !string.IsNullOrEmpty(it.receiver.money);
            bool hasItems = !string.IsNullOrEmpty(it.receiver.items);

            bool wrote = false;

            if (hasRemoveAds)
            {
                sb.AppendLine($"          \"removeads\": {it.receiver.removeads}{(hasMoney || hasItems ? "," : "")}");
                wrote = true;
            }

            if (hasMoney)
            {
                sb.AppendLine($"          \"money\": \"{EscapeJson(it.receiver.money)}\"{(hasItems ? "," : "")}");
                wrote = true;
            }

            if (hasItems)
            {
                sb.AppendLine($"          \"items\": \"{EscapeJson(it.receiver.items)}\"");
                wrote = true;
            }

            if (!wrote)
            {
                // Không có gì thì vẫn là object rỗng
            }

            sb.Append("        }");
            sb.AppendLine();
            sb.Append("      }");
            if (i < nonConsumList.Count - 1 || consumList.Count > 0)
                sb.Append(",");
            sb.AppendLine();
        }

        // consum
        sb.AppendLine("    ],");
        sb.AppendLine("    \"consum\": [");
        for (int i = 0; i < consumList.Count; i++)
        {
            var it = consumList[i];
            sb.AppendLine("      {");
            sb.AppendLine($"        \"id\": \"{EscapeJson(it.id)}\",");
            sb.AppendLine($"        \"pkg\": \"{EscapeJson(it.pkg)}\",");
            sb.AppendLine($"        \"price\": \"{EscapeJson(it.price)}\",");
            sb.AppendLine("        \"receiver\": {");

            bool hasMoney = !string.IsNullOrEmpty(it.receiver.money);
            bool hasItems = !string.IsNullOrEmpty(it.receiver.items);

            if (hasMoney)
            {
                sb.AppendLine(
                    $"          \"money\": \"{EscapeJson(it.receiver.money)}\"{(hasItems ? "," : "")}");
            }

            if (hasItems)
            {
                sb.AppendLine($"          \"items\": \"{EscapeJson(it.receiver.items)}\"");
            }

            sb.Append("        }");
            sb.AppendLine();
            sb.Append("      }");
            if (i < consumList.Count - 1)
                sb.Append(",");
            sb.AppendLine();
        }

        sb.AppendLine("    ]");
        sb.AppendLine("  }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    // ================== DATA STRUCT ==================
    [Serializable]
    private class IAPItemNonConsum
    {
        public string id;
        public string pkg;
        public string price;
        public IAPReceiverNonConsum receiver;
    }

    [Serializable]
    private class IAPReceiverNonConsum
    {
        public int removeads;
        public string money;
        public string items;
    }

    [Serializable]
    private class IAPItemConsum
    {
        public string id;
        public string pkg;
        public string price;
        public IAPReceiverConsum receiver;
    }

    [Serializable]
    private class IAPReceiverConsum
    {
        public string money;
        public string items;
    }

    // ================== CSV HELPERS ==================
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

    // ===== LẤY ĐÚNG SHEET THEO gid =====
    private static string ConvertToCSVUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return url;

        // Lấy spreadsheet id
        var idMatch = Regex.Match(url, @"/d/([a-zA-Z0-9-_]+)");
        if (!idMatch.Success)
            return url; // fallback: dùng nguyên url nếu không match

        string id = idMatch.Groups[1].Value;

        // Tìm gid của sheet hiện tại (nếu có) trong query string
        var gidMatch = Regex.Match(url, @"[?&]gid=(\d+)");
        if (gidMatch.Success)
        {
            string gid = gidMatch.Groups[1].Value;
            // Xuất đúng sheet có gid này
            return $"https://docs.google.com/spreadsheets/d/{id}/export?format=csv&gid={gid}";
        }

        // Không có gid -> export sheet đầu tiên như cũ
        return $"https://docs.google.com/spreadsheets/d/{id}/export?format=csv";
    }

    private static string CleanTextHeadAndTailOnly(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        s = s.Trim(' ', '\t', '\r', '\n');
        if (s.Length > 1 && s.StartsWith("\"") && s.EndsWith("\""))
        {
            s = s.Substring(1, s.Length - 2);
        }

        return s;
    }

    private static string EscapeJson(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\"", "\\\"");
    }

    // ================== ASSET REFRESH ==================
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

    // ================== LOG ==================
#if UNITY_EDITOR
    private static readonly bool ENABLE_LOGGING = true;
#else
    private static readonly bool ENABLE_LOGGING = false;
#endif

    private static string LogRegion = $"{nameof(IAPDataToolsWindow)}";

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
}
#endif