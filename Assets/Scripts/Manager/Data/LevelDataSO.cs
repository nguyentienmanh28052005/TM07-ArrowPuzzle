using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

[System.Serializable]
[MemoryPackable]
public partial class ElectricButtonSaveData
{
    public Vector2Int position;
    public Color color;
}

[System.Serializable]
[MemoryPackable]
public partial class ElectricWallSaveData
{
    public Vector2Int start;
    public Vector2Int end;
    public Color color;
}

[System.Serializable]
[MemoryPackable]
public partial class SnakeSaveData
{
    public ArrowDir direction;
    public Color arrowColor;
    public List<Vector2Int> segmentPositions = new List<Vector2Int>();
}

[System.Serializable]
[MemoryPackable]
public partial class KeycardSaveData
{
    public Vector2Int position;
    public Color color;
}

[System.Serializable]
[MemoryPackable]
public partial class GateSaveData
{
    public Vector2Int position;
    public Color color;
}

[System.Serializable]
[MemoryPackable]
public partial class DeflectorSaveData
{
    public Vector2Int position;
    public ArrowDir direction;
}

[System.Serializable]
[MemoryPackable]
public partial class CountdownBlockSaveData
{
    public Vector2Int position;
    public int count;
}

[System.Serializable]
[MemoryPackable]
public partial class StopBlockSaveData
{
    public Vector2Int position;
    public int count;
}

// BẢN VÁ: THÊM HƯỚNG VÀO HỐ ĐEN
[System.Serializable]
[MemoryPackable]
public partial class PortalData
{
    public Vector2Int entrance;
    // Hướng mũi tên sẽ đi tiếp sau khi chui ra ở đầu 'entrance'
    public ArrowDir entranceDir; 
    public Vector2Int exit;
    // Hướng mũi tên sẽ đi tiếp sau khi chui ra ở đầu 'exit'
    public ArrowDir exitDir;
    public Color portalColor;
}

[CreateAssetMenu(fileName = "NewLevel", menuName = "ArrowPuzzle/LevelData")]
public class LevelDataSO : ScriptableObject
{
    public int levelIndex;
    public GameMode gameMode;
    public LevelDifficulty levelDifficulty;

    [Header("Camera Intro")]
    public bool returnToDefaultZoomAfterIntro = true;

    public float timeLimit = 60f;
    
    public float rewardCoins;
    public float rewardDiamonds;
    
    public List<SnakeSaveData> snakes = new List<SnakeSaveData>();
    public List<KeycardSaveData> keycards = new List<KeycardSaveData>();
    public List<GateSaveData> gates = new List<GateSaveData>();
    public List<ElectricButtonSaveData> electricButtons = new List<ElectricButtonSaveData>();
    public List<ElectricWallSaveData> electricWalls = new List<ElectricWallSaveData>();
    public List<PortalData> portals = new List<PortalData>(); 
    public List<DeflectorSaveData> deflectors = new List<DeflectorSaveData>();
    public List<CountdownBlockSaveData> countdownBlocks = new List<CountdownBlockSaveData>();
    public List<StopBlockSaveData> stopBlocks = new List<StopBlockSaveData>();
}
