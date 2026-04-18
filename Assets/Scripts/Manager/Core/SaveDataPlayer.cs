using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MemoryPack;
using Cysharp.Threading.Tasks;
using Pixelplacement;

[MemoryPackable]
public partial class InProgressLevelData
{
    public int LevelIndex { get; set; }
    public List<SnakeSaveData> RemainingSnakes { get; set; } = new List<SnakeSaveData>();
}

[MemoryPackable]
public partial class PlayerSaveData
{
    public Dictionary<int, float> Items { get; set; } = new Dictionary<int, float>();
    public InProgressLevelData SavedLevelState { get; set; } = null;

    public PlayerSaveData() { }
}

public class SaveDataPlayer : Singleton<SaveDataPlayer>
{
    public PlayerSaveData saveData = new PlayerSaveData();
    private string filePath;

    [Header("Debug Tools (Hotkeys)")]
    public int key;
    public float value;

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

    public void SaveAllDataAndWriteToDisk()
    {
        Save(1, GameManager.Instance.level);
        SaveDataAsync().Forget();
    }

    public void Save(int key, float value)
    {
        if (saveData.Items.ContainsKey(key)) saveData.Items[key] = value;
        else saveData.Items.Add(key, value);
    }

    public float Value(int key)
    {
        if (saveData.Items.TryGetValue(key, out float val)) return val;
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
        else ResetData();
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
        if (Input.GetKeyUp(KeyCode.Alpha2)) Debug.Log("Value: " + Value(key));
        if (Input.GetKeyUp(KeyCode.Alpha3)) SaveAllDataAndWriteToDisk();
    }

    public void SaveCurrentBoardState()
    {
        InProgressLevelData progressData = new InProgressLevelData();
        progressData.LevelIndex = GameManager.Instance.level;

        SnakeBlock[] allSnakes = FindObjectsOfType<SnakeBlock>();
        foreach (SnakeBlock sb in allSnakes)
        {
            // ĐÃ SỬA: Check số lượng điểm LogicNodes
            if (sb.IsMoving || sb.LogicNodes == null || sb.LogicNodes.Count == 0) continue; 

            SnakeSaveData data = new SnakeSaveData();
            data.direction = sb.direction;
            data.arrowColor = sb.snakeColor;

            // ĐÃ SỬA: Lưu từ mảng Toán học
            foreach (Vector3 node in sb.LogicNodes)
            {
                data.segmentPositions.Add(new Vector2Int(Mathf.RoundToInt(node.x), Mathf.RoundToInt(node.y)));
            }
            progressData.RemainingSnakes.Add(data);
        }

        saveData.SavedLevelState = progressData;
        SaveDataAsync().Forget(); 
        Debug.Log("Đã lưu trạng thái bàn cờ dở dang!");
    }

    public void ClearBoardState()
    {
        saveData.SavedLevelState = null;
        SaveDataAsync().Forget();
    }
}