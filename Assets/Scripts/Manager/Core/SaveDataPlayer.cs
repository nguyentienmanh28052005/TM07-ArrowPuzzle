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
    private const float AutoSaveDelaySeconds = 0.35f;

    public PlayerSaveData saveData = new PlayerSaveData();
    private string filePath;
    private bool _savePending;
    private float _saveAtTime;
    private bool _isSaving;

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
        if (GameManager.Instance != null)
        {
            Save(1, GameManager.Instance.level);
        }
        SaveDataSync();
    }

    public void Save(int key, float value)
    {
        if (saveData == null) saveData = new PlayerSaveData();
        if (saveData.Items.ContainsKey(key)) saveData.Items[key] = value;
        else saveData.Items.Add(key, value);
        RequestSave();
    }

    public float Value(int key)
    {
        if (saveData.Items.TryGetValue(key, out float val)) return val;
        return 0;
    }

    public void LoadData()
    {
        if (!File.Exists(filePath))
        {
            ResetData();
            return;
        }

        try
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            saveData = MemoryPackSerializer.Deserialize<PlayerSaveData>(bytes);
            if (saveData == null) saveData = new PlayerSaveData();
            Debug.Log("MemoryPack Loaded: " + filePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"MemoryPack Load failed, resetting save. Error: {ex.Message}");
            ResetData();
        }
    }

    public async UniTaskVoid SaveDataAsync()
    {
        if (_isSaving)
        {
            RequestSave();
            return;
        }

        _isSaving = true;
        try
        {
            if (saveData == null) saveData = new PlayerSaveData();
            byte[] bytes = MemoryPackSerializer.Serialize(saveData);
            await File.WriteAllBytesAsync(filePath, bytes);
            Debug.Log($"MemoryPack Saved ({bytes.Length} bytes)");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MemoryPack Save failed: {ex.Message}");
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void SaveDataSync()
    {
        try
        {
            if (saveData == null) saveData = new PlayerSaveData();
            byte[] bytes = MemoryPackSerializer.Serialize(saveData);
            File.WriteAllBytes(filePath, bytes);
            Debug.Log($"MemoryPack Saved Sync ({bytes.Length} bytes)");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MemoryPack Sync Save failed: {ex.Message}");
        }
    }

    public void ResetData()
    {
        saveData = new PlayerSaveData();
        RequestSave();
    }

    private void Update()
    {
        if (_savePending && Time.unscaledTime >= _saveAtTime)
        {
            _savePending = false;
            SaveDataAsync().Forget();
        }

        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            Save(key, value);
            Debug.Log($"Updated RAM: {key} - {value}");
        }
        if (Input.GetKeyUp(KeyCode.Alpha2)) Debug.Log("Value: " + Value(key));
        if (Input.GetKeyUp(KeyCode.Alpha3)) SaveAllDataAndWriteToDisk();
    }

    private void RequestSave()
    {
        _savePending = true;
        _saveAtTime = Time.unscaledTime + AutoSaveDelaySeconds;
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
        RequestSave();
        Debug.Log("Đã lưu trạng thái bàn cờ dở dang!");
    }

    public void ClearBoardState()
    {
        saveData.SavedLevelState = null;
        RequestSave();
    }
}