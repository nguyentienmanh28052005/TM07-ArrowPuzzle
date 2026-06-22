using mygame.sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class LevelChestManager : master.Singleton<LevelChestManager>
{
    public LevelChestConfig defaultConfig;
    public LevelChestRewardData levelChestConfig;
    public static bool CF_ACTIVE
    {
        get { return PlayerPrefs.GetInt("cf_active_level_chest", 0)==1; }
        set { PlayerPrefs.SetInt("cf_active_level_chest", value ? 1 : 0); }
    }

    public int lcIDRewardQueue
    {
        get { return PlayerPrefs.GetInt("lc_idreward_queue", 0); }
        set { PlayerPrefs.SetInt("lc_idreward_queue", value); }
    }

    public static string CF_LevelChest
    {
        get => PlayerPrefs.GetString("cf_data_level_chest", "");
        set => PlayerPrefs.SetString("cf_data_level_chest", value);
    }

    protected override void Awake()
    {
        base.Awake();
        string dataJson = CF_LevelChest;
        if (!string.IsNullOrEmpty(dataJson))
        {
            try
            {
                var data = JsonUtility.FromJson<LevelChestRewardData>(dataJson);
                if (data != null)
                {
                    levelChestConfig = data;
                    if (levelChestConfig.levelsPerChest <= 0)
                        levelChestConfig.levelsPerChest = defaultConfig.levelsPerChest;
                }
                else
                {
                    InitDefaultConfig();
                }
            }
            catch (Exception)
            {
                InitDefaultConfig();
            }       
        }
        else
        {
            InitDefaultConfig();
        }
    }

    private void InitDefaultConfig()
    {
        levelChestConfig = new LevelChestRewardData
        {
            levelsPerChest = defaultConfig.levelsPerChest,
            chestReward = defaultConfig.chestReward
        };
    }

    public int GetChestGiftIdByLevel(int level)
    {
        int interval = levelChestConfig.levelsPerChest;
        if (interval <= 0) interval = defaultConfig.levelsPerChest;

        if (level <= 0 || level % interval != 0)
            return 0;

        var rewards = levelChestConfig.chestReward;
        if (rewards == null || rewards.Length == 0)
            return 0;

        int step = level / interval;
        int total = rewards.Length;
        int id = ((step - 1) % total) + 1;
        Debug.Log($"GetChestGiftIdByLevel: {id}");
        return id;
    }

    public void SetLevelChestQueue(int level)
    {
        lcIDRewardQueue = GetChestGiftIdByLevel(level); 
    }

    public List<DataResource> GetChestRewardsByLevel(int level)
    {
        int id = GetChestGiftIdByLevel(level);
        if (id == 0)
            return null;
        var entry = levelChestConfig.chestReward[id - 1];
        return entry.rewards;
    }

    public List<DataResource> GetRewardsById(int id)
    {
        if (id == 0)
            return null;    
        if (levelChestConfig == null || levelChestConfig.chestReward == null)
            return null;
        var entry = levelChestConfig.chestReward
                                    .FirstOrDefault(x => x.id == id);
        return entry != null ? entry.rewards : null;
    }
}
[System.Serializable]
public class LevelChestRewardData
{
    public int levelsPerChest = 5;
    public LevelChestReward[] chestReward;
}
