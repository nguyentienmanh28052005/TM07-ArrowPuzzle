using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SnakeSaveData
{
    public ArrowDir direction;
    public List<Vector2Int> segmentPositions = new List<Vector2Int>();
}

[CreateAssetMenu(fileName = "NewLevel", menuName = "ArrowPuzzle/LevelData")]
public class LevelDataSO : ScriptableObject
{
    public List<SnakeSaveData> snakes = new List<SnakeSaveData>();
}