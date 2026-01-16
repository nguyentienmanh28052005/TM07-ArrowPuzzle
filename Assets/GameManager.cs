using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public List<LevelDataSO> levelDataSOs;
    public int level = 1;

    void Awake()
    {
        
    }

    void Start()
    {
        if((int)SaveDataPlayer.Instance.Value(1) != 0)
        {
            level = (int)SaveDataPlayer.Instance.Value(1);
        }
    }

    void Update()
    {
        
    }

    public LevelDataSO GetCurrentLevelData()
    {
        if (levelDataSOs == null || levelDataSOs.Count == 0) return null;
        if (level < 1 || level >= levelDataSOs.Count + 1) return null;
        return levelDataSOs[level-1];
    }
}
