using System;
using System.Collections;
using System.Collections.Generic;
using mygame.sdk;
using Newtonsoft.Json;
using UnityEngine;

public static class AdsRewardConfig 
{
    public static RewardLevelConfig rewardLevelConfig;
    public static RewardLevelConfig dfRewardLevel = new RewardLevelConfig();
    public static RewardLevelConfig databucketRewardLevelConfig;
    public static string CF_REWARD_LEVEL
    {
        get => PlayerPrefs.GetString("cf_remove_ads_reward_level","");
        set => PlayerPrefs.SetString("cf_remove_ads_reward_level", value);
    }

    public static void SetConfigAll()
    {
        if (!string.IsNullOrEmpty(CF_REWARD_LEVEL) || CF_REWARD_LEVEL.Length > 0)
        {
            try
            {
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<RewardLevelConfig>(CF_REWARD_LEVEL);
                rewardLevelConfig = data;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError(CF_REWARD_LEVEL);
            }

        }
    }
    public static void SetConfigDataBucket(string json)
    {
        if (!string.IsNullOrEmpty(json) || json.Length > 0)
        {
            try
            {
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<RewardLevelConfig>(json);
                databucketRewardLevelConfig = data;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError(json);
            }

        }
    }
    public static RewardLevelConfig GetConfig()
    {
        Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(dfRewardLevel));

        if (databucketRewardLevelConfig != null)
        {
            return databucketRewardLevelConfig;
        }
        if (rewardLevelConfig != null) {
            return rewardLevelConfig;
        }
        return dfRewardLevel;
    }
    
    public static void SetNumAdsRevive(int value)
    {
        PlayerPrefsUtil.CF_NumReviveShowAds = value;
    }
}
public class RewardLevelConfig
{
    [JsonProperty("rmrv")] public int levelRemoveAdsRevive = 1;
    [JsonProperty("rmbb")] public int levelRemoveAdsBuyBooster = 1;
    [JsonProperty("rmw")] public int levelRemoveAdsWinUI = 0;
    [JsonProperty("rmbh")] public int levelRemoveAdsBuyHeart = 1;

    [JsonProperty("arv")] public int levelActiveAdsRevive = 0;
    [JsonProperty("abb")] public int levelActiveAdsBuyBooster = 0;
    [JsonProperty("aw")] public int levelActiveAdsWinUI = 9;
    [JsonProperty("abh")] public int levelActiveAdsBuyHeart = 0;

    [JsonProperty("awm")] public int[] rewardAdsWinMulti = new int[] { 2, 2, 2 };
    public int countWatchAdsRefillHeartEachDay = 0;
}