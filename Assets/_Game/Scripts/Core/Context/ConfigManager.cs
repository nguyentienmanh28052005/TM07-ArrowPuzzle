using System;
using master;
using mygame.sdk;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using time;

public class ConfigManager : master.Singleton<ConfigManager>
{
    [SerializeField] ConfigAdsLose[] allConfigAdsLose;
    
    private void Start()
    {
        GameEvent.OnReceiveFirebaseDataDone += SetConfigShowAdsFull;
        SetConfigShowAdsFull();
    }
    
    private void OnDestroy()
    {
        GameEvent.OnReceiveFirebaseDataDone -= SetConfigShowAdsFull;
    }

    private static void CheckResetActivePopup(long time, out bool newDay, out bool newWeek, out bool newMonth)
    {
        newDay = false;
        newWeek = false;
        newMonth = false;
        var dateTime = new DateTime(time, DateTimeKind.Local);
        if (DateTime.Now.Day != dateTime.Day || DateTime.Now.Month != dateTime.Month || DateTime.Now.Year != dateTime.Year)
        {
            newDay = true;
        }

        var now = DateTime.Now;
        var last = dateTime;

        if (StartOfWeek(now) != StartOfWeek(last))
        {
            newWeek = true;
        }

        if (DateTime.Now.Month != dateTime.Month || DateTime.Now.Year != dateTime.Year)
        {
            newMonth = true;
        }

        static DateTime StartOfWeek(DateTime dt, DayOfWeek start = DayOfWeek.Monday)
        {
            int diff = (7 + (dt.DayOfWeek - start)) % 7;
            return dt.Date.AddDays(-diff); // 00:00 của ngày đầu tuần
        }
    }
    
    public bool IsShowPackage(int level)
    {
        if (IsShowStartPack(level))
        {
            return true;
        }
        else if (IsShowRemoveAdsPack(level))
        {
            return true;
        }
        else if (IsShowHappyPack(level))
        {
            return true;
        }
        else if (IsShowWeekendPack(level))
        {
            return true;
        }
        return false;
    }
    public void SetConfigShowAdsFull()
    {
        if (!string.IsNullOrEmpty(PlayerPrefsUtil.CFShowFullLoseLevel))
        {
            try
            {
                var config = JsonConvert.DeserializeObject<ConfigAdsLose[]>(PlayerPrefsUtil.CFShowFullLoseLevel);
                if(config.Length > 0)
                {
                    allConfigAdsLose = config;
                }
            }catch(Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
    
    public void ShowPackage(int level)
    {
        if (IsShowStartPack(level))
        {
            var popup = UIManager.Instance.ShowPopup<UIStarterPack>(null);
        }
        else if (IsShowRemoveAdsPack(level))
        {
            var popup = UIManager.Instance.ShowPopup<UIRemoveAds>(null);
        }
        else if (IsShowHappyPack(level, true))
        {
            var config = ConfigEventController.GetDataEventConfig(EEventConfig.Event_Happy_Pack);
            ConfigEventController.GetEventTime(config, out var activeTime, out var endTime);

            var popup = UIManager.Instance.ShowPopup<UIHappyPack>(null);
            popup.SetTimeText(activeTime, endTime - activeTime);
        }
    }
    public static bool IsShowStartPack(int levelCurrent)
    {
        var list = GetLevelShowPackage(CF_LevelShowPackageStarterPack);
        var baseItem = DataManager.Instance.packageData.FindPackage(1);
        bool isBuy = !baseItem.isActive;
        if (list != null && list.Count > 0 && !isBuy && PlayerPrefs.GetInt("start_pack_show_at", 0) == 0)
        {
            PlayerPrefs.GetInt("start_pack_show_at", 1);
            return list.Contains(levelCurrent);
        }
        else
        {
            return false;
        }
    }
    public static bool IsShowRemoveAdsPack(int levelCurrent)
    {
        var list = GetLevelShowPackage(CF_LevelShowPackageRemoveAds);
        bool isRemoveAds = AdsHelper.isRemoveAds(0);
        if (list != null && list.Count > 0 && !isRemoveAds && PlayerPrefs.GetInt("remove_ads_show_at", 0) == 0)
        {
            PlayerPrefs.GetInt("remove_ads_show_at", 1);
            return list.Contains(levelCurrent);
        }
        else
        {
            return false;
        }
    }

    public static bool IsShowHappyPack(int levelCurrent, bool isShowPack = false)
    {
        var time = long.Parse(PlayerPrefs.GetString("last_time_show_happy_pack", "0"));
        CheckResetActivePopup(time, out bool newDay, out bool newWeek, out bool newMonth);
        if (newDay)
        {
            PlayerPrefs.SetInt("happy_pack_show_at", 0);
        }
        
        var config = ConfigEventController.GetDataEventConfig(EEventConfig.Event_Happy_Pack);
        if (config == null) return false;

        ConfigEventController.GetEventTime(config, out var activeTime, out var endTime);
        long now = MGTime.GetUtcTime();
        bool isTimeActive = now >= activeTime && now < endTime;

        var baseItem = DataManager.Instance.packageData.FindPackage(30);
        bool isBuy = baseItem != null && !baseItem.isActive;

        if (isTimeActive && !isBuy && DataManager.Level >= config.levelUnlock)
        {
            if (PlayerPrefs.GetInt("happy_pack_show_at", 0) == 0)
            {
                if (isShowPack)
                {
                    PlayerPrefs.SetInt("happy_pack_show_at", 1);
                    PlayerPrefs.SetString("last_time_show_happy_pack", DateTime.Now.Ticks.ToString());
                }
                
                return true;
            }
        }
        return false;
    }

    public static bool IsShowWeekendPack(int levelCurrent, bool isShowPack = false)
    {
        var time = long.Parse(PlayerPrefs.GetString("last_time_show_weekend_pack", "0"));
        CheckResetActivePopup(time, out bool newDay, out bool newWeek, out bool newMonth);
        if (newWeek)
        {
            PlayerPrefs.SetInt("weekend_pack_show_at", 0);
        }
        var config = ConfigEventController.GetDataEventConfig(EEventConfig.Event_Weekend_Pack);
        if (config == null) return false;
        ConfigEventController.GetEventTime(config, out var activeTime, out var endTime);
        long now = MGTime.GetUtcTime();
        bool isTimeActive = now >= activeTime && now < endTime;

        var baseItem = DataManager.Instance.packageData.FindPackage(500);
        bool isBuy = baseItem != null && !baseItem.isActive;

        if (isTimeActive && !isBuy && DataManager.Level >= config.levelUnlock)
        {
            if (PlayerPrefs.GetInt("weekend_pack_show_at", 0) == 0)
            {
                if (isShowPack)
                {
                    PlayerPrefs.SetInt("weekend_pack_show_at", 1);
                    PlayerPrefs.SetString("last_time_show_weekend_pack", DateTime.Now.Ticks.ToString());
                }
                
                return true;
            }
        }
        return false;
    }
    
    public static List<int> GetLevelShowPackage(string cf)
    {
        string value = cf;
        value = value.Trim('[', ']');
        string[] parts = value.Split(',');
        int[] arr = parts.Select(x => int.Parse(x)).ToArray();
        return new List<int>(arr);
    }
    public static string CF_LevelShowPackageStarterPack
    {
        get => PlayerPrefs.GetString("cf_lv_show_package_starter_pack", "[12]");
        set => PlayerPrefs.SetString("cf_lv_show_package_starter_pack", value);
    }
    public static string CF_LevelShowPackageRemoveAds
    {
        get => PlayerPrefs.GetString("cf_lv_show_package_remove_ads", "[15]");
        set => PlayerPrefs.SetString("cf_lv_show_package_remove_ads", value);
    }
    public static string CF_LevelShowPackageHappyPack
    {
        get => PlayerPrefs.GetString("cf_lv_show_package_happy_pack", "[55]");
        set => PlayerPrefs.SetString("cf_lv_show_package_happy_pack", value);
    }
    public static string CF_LevelShowPackageWeekendPack
    {
        get => PlayerPrefs.GetString("cf_lv_show_package_weekend_pack", "[60]");
        set => PlayerPrefs.SetString("cf_lv_show_package_weekend_pack", value);
    }
    public static string CF_X2ValueGoldWin
    {
        get => PlayerPrefs.GetString("cf_x2_value_gold_win", "[2, 2, 2, 2, 2]");
        set => PlayerPrefs.SetString("cf_x2_value_gold_win", value);
    }

    public static bool CF_ShiftBlockPictureMode
    {
        get => PlayerPrefs.GetInt("cf_shift_block_picture_mode", 0) == 1;
        set => PlayerPrefs.SetInt("cf_shift_block_picture_mode", value ? 1 : 0);
    }

    public static int CF_LevelShowFirstTutorial
    {
        get => PlayerPrefs.GetInt("cf_level_show_first_tutorial", 1);
        set => PlayerPrefs.SetInt("cf_level_show_first_tutorial", value);
    }

    [SerializeField] private int[] defaultRewardsWin;
   
    public static string CF_RefillPackTierConfig
    {
        get => PlayerPrefs.GetString("cf_refill_pack_tier_config", "");
        set => PlayerPrefs.SetString("cf_refill_pack_tier_config", value);
    }

    public ConfigAdsLose GetConfigShowAdsFull(int level)
    {
        var configs = allConfigAdsLose.LastOrDefault(x => x.minLevel <= level && x.maxLevel >= level);
        if (configs != null)
        {
            return configs;
        }
        else
        {
            return new ConfigAdsLose();
        }
    }
    
    public int GetGoldCompleteLevel(LevelType levelType)
    {
        var defaultValue = new int[] { 20, 30, 40 };

        try
        {
            var cfg = PlayerPrefsUtil.CFGoldCompleteLevel;

            if (string.IsNullOrEmpty(cfg))
                return defaultValue[(int)levelType];

            var golds = JsonConvert.DeserializeObject<int[]>(cfg);

            if (golds == null || golds.Length == 0)
                return defaultValue[(int)levelType];

            int index = Mathf.Clamp((int)levelType, 0, golds.Length - 1);

            return golds[index];
        }
        catch
        {
            return defaultValue[(int)levelType];
        }
    }
}

public enum EFeature
{
    BattlePass = 1,
}

[Serializable]
public class ConfigAdsLose
{
    public int CFShowFullFirstTimeFail;
    public int CFShowFullFailSpaceByLevel;
    public int CFShowFullFailSpace;
    public int minLevel;
    public int maxLevel;
}