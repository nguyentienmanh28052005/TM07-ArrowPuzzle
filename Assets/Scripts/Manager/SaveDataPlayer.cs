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

    public int key;
    public float value;

    private void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "save_data.bin");
        LoadData();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAllDataAndWriteToDisk();
        }
    }

    private void OnApplicationQuit()
    {
        SaveAllDataAndWriteToDisk();
    }

    public void SaveAllDataAndWriteToDisk()
    {
        Save(1, GameManager.Instance.level);

        SaveDataAsync().Forget();
    }

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

    public float Value(int key)
    {
        if (saveData.Items.TryGetValue(key, out float val))
        {
            return val;
        }
        return 0;
    }

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

    public async UniTaskVoid SaveDataAsync()
    {
        byte[] bytes = MemoryPackSerializer.Serialize(saveData);
        await File.WriteAllBytesAsync(filePath, bytes);
        Debug.Log($"MemoryPack Saved ({bytes.Length} bytes)");
    }

    public void ResetData()
    {
        saveData = new PlayerSaveData();
        SaveDataAsync().Forget();
    }

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