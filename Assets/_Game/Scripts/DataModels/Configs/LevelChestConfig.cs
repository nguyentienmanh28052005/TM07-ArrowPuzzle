using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelChestSO", menuName = "SO/LevelChestSO")]
public class LevelChestConfig : ScriptableObject
{
    public int levelsPerChest = 5;
    public LevelChestReward[] chestReward;
}

[System.Serializable]
public class LevelChestReward
{
    public int id;   
    public List<DataResource> rewards;
}