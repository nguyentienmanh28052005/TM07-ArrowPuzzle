using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

/// <summary>
/// Cấu trúc dữ liệu lưu trữ trạng thái của một con rắn đơn lẻ trên bàn cờ.
/// </summary>
/// 
[System.Serializable]
[MemoryPackable]
public partial class SnakeSaveData
{
    public ArrowDir direction;
    public Color arrowColor;
    public List<Vector2Int> segmentPositions = new List<Vector2Int>();
}

/// <summary>
/// Đối tượng ScriptableObject chứa toàn bộ dữ liệu cấu hình để sinh ra một màn chơi hoàn chỉnh.
/// </summary>
[CreateAssetMenu(fileName = "NewLevel", menuName = "ArrowPuzzle/LevelData")]
public class LevelDataSO : ScriptableObject
{
    public GameMode gameMode;
    public LevelDifficulty levelDifficulty;

    [Header("Mode Specific Settings")]
    [Tooltip("Thời gian đếm ngược (Giây). Chỉ dùng cho Time Attack.")]
    public float timeLimit = 60f;
    
    [Header("Rewards")]
    public float rewardCoins;
    public float rewardDiamonds;
    public List<SnakeSaveData> snakes = new List<SnakeSaveData>();
}