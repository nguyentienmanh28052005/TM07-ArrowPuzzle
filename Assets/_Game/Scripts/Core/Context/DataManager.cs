using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using mygame.sdk;
using UnityEngine;
using Random = UnityEngine.Random;

public class DataManager : master.Singleton<DataManager>, ISyncData
{
    //public static DataManager Instance;
    public PackageData packageData
    {
        get
        {
            if (PlayerPrefsUtil.CFLevelShowPlayPopup > DataManager.Level)
            {
                return packageData_v1;
            }
            else
            {
                return packageData_BackUp;
            }
        }
    }

    public PackageData packageData_v1;
    public PackageData packageData_BackUp;
    public AvatarConfig avatarConfig;
    public ItemInfo[] allGift;
    private ComIAP_PackWeekend comIAP_PackWeekend = new ComIAP_PackWeekend();
    public ComIAP_PackWeekend ComIAP_PackWeekend => comIAP_PackWeekend;
    private ComIAP_ControlPack comIAP_ControlPack = new ComIAP_ControlPack();
    public ComIAP_ControlPack ComIAP_ControlPack => comIAP_ControlPack;
    public Sprite defaultArt;

    public string userName
    {
        get => UserDataManager.Instance.GetDataUser().name;
        set
        {
            UserDataManager.Instance.ChangeUserName(value);
            userNameChange?.Invoke();
        }
    }

    [Obsolete]
    public int rankIndex
    {
        get => PlayerPrefs.GetInt("rankIndex", -1);
        set => PlayerPrefs.SetInt("rankIndex", value);
    }

    [Obsolete]
    public int avtarID
    {
        get => UserDataManager.Instance.UserData.avatarId;
        set
        {
            UserDataManager.Instance.ChangeAvatar(value, "");
            avatarChange?.Invoke();
        }
    }

    public int ConsecutivePlay
    {
        get => PlayerPrefs.GetInt("consecutive_play", 0);
        set => PlayerPrefs.SetInt("consecutive_play", value);
    }

    public static int LevelCacheStart
    {
        get => PlayerPrefs.GetInt("levelcachestart", -1);
        set => PlayerPrefs.SetInt("levelcachestart", value);
    }

    public int ConsecutiveLose
    {
        get => PlayerPrefs.GetInt("consecutive_lose", 0);
        set => PlayerPrefs.SetInt("consecutive_lose", value);
    }

    public static bool isLose
    {
        get => PlayerPrefs.GetInt("is_lose_last_game", 0) > 0;
        set => PlayerPrefs.SetInt("is_lose_last_game", value ? 1 : 0);
    }

    public int AdsOrDeathInLevel
    {
        get => PlayerPrefs.GetInt("ads_or_death_in_level", 0);
        set => PlayerPrefs.SetInt("ads_or_death_in_level", value);
    }

    public int AdsPerLevelPlay
    {
        get => PlayerPrefs.GetInt("ads_per_level_play", 0);
        set => PlayerPrefs.SetInt("ads_per_level_play", value);
    }

    public int LastDeathProgressInLevel
    {
        get => PlayerPrefs.GetInt("last_death_progress_in_level", 0);
        set => PlayerPrefs.SetInt("last_death_progress_in_level", value);
    }

    public int ConsecutiveWin
    {
        get => PlayerPrefs.GetInt("consecutive_win", 0);
        set => PlayerPrefs.SetInt("consecutive_win", value);
    }

    [Obsolete]
    public bool isFreeTicketIfFail
    {
        get => PlayerPrefs.GetInt("is_free_ticket_if_fail", 1) == 1;
        set => PlayerPrefs.SetInt("is_free_ticket_if_fail", value ? 1 : 0);
    }

    public static int Gold
    {
        get => GameRes.getRes(RES_type.GOLD, 0);
        set => GameRes.AddRes(RES_type.GOLD, value);
    }

    public static int Level => GameRes.GetLevel();
    public static Action avatarChange;
    public static Action userNameChange;

    protected override void Awake()
    {
        base.Awake();
        //Instance = this;
        comIAP_PackWeekend.Initialized();
        comIAP_ControlPack.Initialized();
        GameEvent.OnReceiveResource += OnReceiveResource;
        for (int i = 0; i < packageData.packageInfos.Length; i++)
        {
            packageData.packageInfos[i].CheckReset();
        }
    }

    private void OnDestroy()
    {
        GameEvent.OnReceiveResource -= OnReceiveResource;
    }

    private void OnReceiveResource(LogEvent.ReasonItem reason, string where, DataResource[] dataResources, int level)
    {
        for (int i = 0; i < dataResources.Length; i++)
        {
            if (dataResources[i].resType == RES_type.UnlimitedHeart)
            {
                HeartManager.Instance.AddHeart(dataResources[i].amount, 1);
                continue;
            }

            if (dataResources[i].resType == RES_type.Heart)
            {
                HeartManager.Heart += dataResources[i].amount;
                continue;
            }

            if (dataResources[i].resType == RES_type.UnlimitedMagnet)
            {
                BoosterManager.AddTimeUnlimited(BoosterType.Magnet, dataResources[i].amount);
                continue;
            }

            if (dataResources[i].resType == RES_type.UnlimitedMutilColorBox)
            {
                BoosterManager.AddTimeUnlimited(BoosterType.MutilColorBox, dataResources[i].amount);
                continue;
            }

            if (dataResources[i].resType == RES_type.DoubleReward)
            {
                DoubleRewardManager.AddTimeUnlimited(dataResources[i].amount);
                continue;
            }

            GameRes.AddRes(dataResources[i].resType, dataResources[i].amount, where, true);
            Debug.Log($"Add Ress = {dataResources[i].resType}_{dataResources[i].amount}");
        }
        ItemInfo[] itemInfos = new ItemInfo[dataResources.Length];
        for (int i = 0; i < dataResources.Length; i++)
        {
            itemInfos[i] = new ItemInfo();

            itemInfos[i].CopyFromDataResource(dataResources[i]);
        }
        LogEvent.ResourceEarn(where, itemInfos, reason, level);
    }

    public void OnSinkResource(LogEvent.ReasonItem reason, string where, DataResource[] dataResources, int level)
    {
        for (int i = 0; i < dataResources.Length; i++)
        {
            if (dataResources[i].resType == RES_type.UnlimitedHeart)
            {
                HeartManager.Instance.AddHeart(dataResources[i].amount, 1);
                continue;
            }

            if (dataResources[i].resType == RES_type.UnlimitedMagnet)
            {
                BoosterManager.AddTimeUnlimited(BoosterType.Magnet, dataResources[i].amount);
                continue;
            }

            if (dataResources[i].resType == RES_type.UnlimitedMutilColorBox)
            {
                BoosterManager.AddTimeUnlimited(BoosterType.MutilColorBox, dataResources[i].amount);
                continue;
            }

            if (dataResources[i].resType == RES_type.DoubleReward)
            {
                DoubleRewardManager.AddTimeUnlimited(dataResources[i].amount);
                continue;
            }

            GameRes.AddRes(dataResources[i].resType, dataResources[i].amount, where, true);
            Debug.Log($"Add Ress1 = {dataResources[i].resType}_{dataResources[i].amount}");
        }

        ItemInfo[] itemInfos = new ItemInfo[dataResources.Length];
        for (int i = 0; i < dataResources.Length; i++)
        {
            itemInfos[i] = new ItemInfo();
            itemInfos[i].CopyFromDataResource(dataResources[i]);
        }

        LogEvent.ResourceSink(where, itemInfos, reason, level);
    }


    public Sprite GetIcon(RES_type rewardType)
    {
        var item = allGift.SingleOrDefault(x => x.itemType == rewardType);
        return item?.Icon;
    }

    [Obsolete]
    public void ReceiveGift(bool showPopup, float delay, string where, LogEvent.ReasonItem reason, bool isEarn,
        int level, params ItemInfo[] itemInfo)
    {
        for (int i = 0; i < itemInfo.Length; i++)
        {
            if (itemInfo[i].itemType == RES_type.UnlimitedHeart)
            {
                HeartManager.Instance.AddHeart(itemInfo[i].itemAmount, 1);
                continue;
            }

            if (itemInfo[i].itemType == RES_type.Heart)
            {
                HeartManager.Heart += itemInfo[i].itemAmount;
                continue;
            }

            if (itemInfo[i].itemType == RES_type.UnlimitedMagnet)
            {
                BoosterManager.AddTimeUnlimited(BoosterType.Magnet, itemInfo[i].itemAmount);
                continue;
            }

            if (itemInfo[i].itemType == RES_type.UnlimitedMutilColorBox)
            {
                BoosterManager.AddTimeUnlimited(BoosterType.MutilColorBox, itemInfo[i].itemAmount);
                continue;
            }

            if (itemInfo[i].itemType == RES_type.DoubleReward)
            {
                DoubleRewardManager.AddTimeUnlimited(itemInfo[i].itemAmount);
                continue;
            }

            GameRes.AddRes(itemInfo[i].itemType, itemInfo[i].itemAmount, where, true);
            Debug.Log($"Add Ress2 = {itemInfo[i].itemType}_{itemInfo[i].itemAmount}");
        }

        if (showPopup)
        {
            UIManager.Instance.ShowPopup<UIReceiveGift>(null).Initialized(itemInfo);
        }

        if (isEarn)
        {
            LogEvent.ResourceEarn(where, itemInfo, reason, level);
        }
        else
        {
            LogEvent.ResourceSink(where, itemInfo, reason, level);
        }
    }

    [Obsolete]
    public void ReceiveGift(bool showPopup, float delay, string where, LogEvent.ReasonItem reason, bool isEarn,
        int level, params PackageData.ItemBuyInfo[] itemInfo)
    {
        for (int i = 0; i < itemInfo.Length; i++)
        {
            if (itemInfo[i].itemType == RES_type.UnlimitedHeart)
            {
                HeartManager.Instance.AddHeart(itemInfo[i].itemAmount, 1);
                continue;
            }

            if (itemInfo[i].itemType == RES_type.Heart)
            {
                HeartManager.Heart += itemInfo[i].itemAmount;
                continue;
            }

            if (itemInfo[i].itemType == RES_type.UnlimitedMagnet)
            {
                BoosterManager.AddTimeUnlimited(BoosterType.Magnet, itemInfo[i].itemAmount);
                continue;
            }

            if (itemInfo[i].itemType == RES_type.UnlimitedMutilColorBox)
            {
                BoosterManager.AddTimeUnlimited(BoosterType.MutilColorBox, itemInfo[i].itemAmount);
                continue;
            }

            if (itemInfo[i].itemType == RES_type.DoubleReward)
            {
                DoubleRewardManager.AddTimeUnlimited(itemInfo[i].itemAmount);
                continue;
            }

            GameRes.AddRes(itemInfo[i].itemType, itemInfo[i].itemAmount, where, true);
        }

        if (showPopup && itemInfo.Length > 0)
        {
            UIManager.Instance.ShowPopup<UIReceiveGift>(null).Initialized(itemInfo);
        }

        if (isEarn)
        {
            LogEvent.ResourceEarn(where, itemInfo, reason, level);
        }
        else
        {
            LogEvent.ResourceSink(where, itemInfo, reason, level);
        }
    }

    [Obsolete]
    public bool IsUsedBooster(RES_type boosterType)
    {
        return PlayerPrefs.GetInt($"is_used_booster_{boosterType.ToString()}", 0) == 1;
    }

    [Obsolete]
    public void SetUsedBooster(RES_type boosterType, int value = 1)
    {
        PlayerPrefs.SetInt($"is_used_booster_{boosterType.ToString()}", value);
    }

    public void OnSyncData(UserData newData)
    {
        if (newData.game_info != null)
        {
            ConsecutivePlay = newData.game_info.consecutivePlay;
            ConsecutiveLose = newData.game_info.consecutiveLose;
            ConsecutiveWin = newData.game_info.consecutiveWin;
            isFreeTicketIfFail = newData.game_info.isFreeTicketIfFail;
            GameRes.SetLevel(Level_type.Normal, newData.level);
            foreach (RES_type type in Enum.GetValues(typeof(RES_type)))
            {
                var cur = GameRes.getRes(type);
                GameRes.AddRes(type, -cur, "sync_data");
            }

            if (newData.game_info.dataResources != null)
            {
                foreach (var res in newData.game_info.dataResources)
                {
                    GameRes.AddRes(res.resType, res.amount, "sync_data");
                }
            }
        }
        else
        {
            ConsecutivePlay = 0;
            ConsecutiveLose = 0;
            ConsecutiveWin = 0;
            isFreeTicketIfFail = true;
            foreach (RES_type type in Enum.GetValues(typeof(RES_type)))
            {
                var cur = GameRes.getRes(type);
                GameRes.AddRes(type, -cur, "sync_data");
            }
        }
    }

    public void SendSyncData(UserData dataUser)
    {
        dataUser.game_info.consecutivePlay = ConsecutivePlay;
        dataUser.game_info.consecutiveLose = ConsecutiveLose;
        dataUser.game_info.consecutiveWin = ConsecutiveWin;
        dataUser.game_info.isFreeTicketIfFail = isFreeTicketIfFail;
        List<DataResource> dataRes = new();
        foreach (RES_type type in Enum.GetValues(typeof(RES_type)))
        {
            var amount = GameRes.getRes(type);
            if (amount > 0)
            {
                var res = new DataResource(type, amount);
                dataRes.Add(res);
            }
        }

        dataUser.game_info.dataResources = dataRes;
    }
}