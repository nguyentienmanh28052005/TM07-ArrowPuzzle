using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

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

// ==========================================
// BẢN VÁ: KHÔI PHỤC LẠI DỮ LIỆU HỐ ĐEN (PORTAL)
// ==========================================
[System.Serializable]
[MemoryPackable]
public partial class PortalData
{
    public Vector2Int entrance;
    public Vector2Int exit;
    public Color portalColor;
}

[CreateAssetMenu(fileName = "NewLevel", menuName = "ArrowPuzzle/LevelData")]
public class LevelDataSO : ScriptableObject
{
    public GameMode gameMode;
    public LevelDifficulty levelDifficulty;

    public float timeLimit = 60f;
    
    public float rewardCoins;
    public float rewardDiamonds;
    
    public List<SnakeSaveData> snakes = new List<SnakeSaveData>();
    public List<KeycardSaveData> keycards = new List<KeycardSaveData>();
    public List<GateSaveData> gates = new List<GateSaveData>();
    
    // BẢN VÁ: Khôi phục danh sách lưu Hố Đen
    public List<PortalData> portals = new List<PortalData>(); 
}