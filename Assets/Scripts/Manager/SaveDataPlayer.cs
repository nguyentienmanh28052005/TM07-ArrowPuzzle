using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MemoryPack;
using Cysharp.Threading.Tasks;
using Pixelplacement;

[MemoryPackable]
public partial class PlayerSaveData
{
    public Dictionary<int, float> Items { get; set; } = new Dictionary<int, float>();

    public PlayerSaveData() { }
}

public class SaveDataPlayer : Singleton<SaveDataPlayer>
{
    public PlayerSaveData saveData = new PlayerSaveData();
    private string filePath;

    [Header("Debug Tools (Hotkeys)")]
    public int key;
    public float value;

    /// <summary>
    /// Khởi tạo đường dẫn an toàn và nạp dữ liệu ngay khi khởi động.
    /// </summary>
    private void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "save_data.bin");
        LoadData();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveAllDataAndWriteToDisk();
    }

    private void OnApplicationQuit()
    {
        SaveAllDataAndWriteToDisk();
    }

    /// <summary>
    /// Thu thập toàn bộ dữ liệu hiện tại và ghi đè xuống bộ nhớ thiết bị.
    /// </summary>
    public void SaveAllDataAndWriteToDisk()
    {
        Save(1, GameManager.Instance.level);
        SaveDataAsync().Forget();
    }

    /// <summary>
    /// Lưu trữ hoặc cập nhật một cặp Key-Value vào bộ đệm RAM.
    /// </summary>
    public void Save(int key, float value)
    {
        if (saveData.Items.ContainsKey(key))
        {
            saveData.Items[key] = value;
        }
        else
        {
            saveData.Items.Add(key, value);
        }
    }

    /// <summary>
    /// Lấy giá trị Float dựa trên Key tương ứng. Trả về 0 nếu không tìm thấy.
    /// </summary>
    public float Value(int key)
    {
        if (saveData.Items.TryGetValue(key, out float val))
        {
            return val;
        }
        return 0;
    }

    /// <summary>
    /// Đọc và giải mã dữ liệu nhị phân từ ổ cứng lên RAM bằng MemoryPack.
    /// </summary>
    public void LoadData()
    {
        if (File.Exists(filePath))
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            saveData = MemoryPackSerializer.Deserialize<PlayerSaveData>(bytes);

            if (saveData == null) saveData = new PlayerSaveData();

            Debug.Log("MemoryPack Loaded: " + filePath);
        }
        else
        {
            ResetData();
        }
    }

    /// <summary>
    /// Tiến trình ghi file bất đồng bộ (Async) giúp game không bị giật lag khi lưu.
    /// </summary>
    public async UniTaskVoid SaveDataAsync()
    {
        byte[] bytes = MemoryPackSerializer.Serialize(saveData);
        await File.WriteAllBytesAsync(filePath, bytes);
        Debug.Log($"MemoryPack Saved ({bytes.Length} bytes)");
    }

    /// <summary>
    /// Khôi phục toàn bộ dữ liệu về trạng thái trống.
    /// </summary>
    public void ResetData()
    {
        saveData = new PlayerSaveData();
        SaveDataAsync().Forget();
    }

    /// <summary>
    /// Khu vực phím tắt (Hotkeys) phục vụ cho việc Debug trên Editor.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            Save(key, value);
            Debug.Log($"Updated RAM: {key} - {value}");
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            Debug.Log("Value: " + Value(key));
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            SaveAllDataAndWriteToDisk();
        }
    }
}