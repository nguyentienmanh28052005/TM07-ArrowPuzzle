using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SnakeSaveData
{
    public ArrowDir direction; // Hướng của đầu
    public List<Vector2Int> segmentPositions = new List<Vector2Int>(); // List tọa độ: [0] là đầu, [1] là thân...
}

[CreateAssetMenu(fileName = "NewLevel", menuName = "ArrowPuzzle/LevelData")]
public class LevelDataSO : ScriptableObject
{
    public List<SnakeSaveData> snakes = new List<SnakeSaveData>();
}