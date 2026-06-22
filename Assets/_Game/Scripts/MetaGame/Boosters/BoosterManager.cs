using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using mygame.sdk;
using time;
using UnityEngine;

public enum BoosterType
{
    None = 0,
    Hand = 1,
    Shuffle = 2,
    Clear = 3,
    ExtraSlot = 4,
    Magnet = 5,
    MutilColorBox = 6,
}

[Serializable]
public class BoosterInfo
{
    public BoosterType type;
    public string name;
    public string desc;
    public string instruct;
    public Sprite bigIcon;
    public Sprite icon;
    public bool inGameOnly;
}

[Serializable]
public class BoosterPrice
{
    public BoosterType type;
    public int price;
    public int typeBuy;
    public int amount = 1;
}

public class BoosterManager : MonoBehaviour, ISyncData
{
    public static Action OnResetVisual;
    public static BoosterManager Instance;
    [SerializeField] private BoosterInfo[] boosterInfos;
    [SerializeField] private BoosterPrice[] boosterPrices;

    public BoosterType activeBooster = BoosterType.None;
    public bool isFreeBooster;
    private const string ValueDefaultLevelConfigBooster = "1:8,2:18,3:14";

    public static string ValueLevelUnlock
    {
        get => PlayerPrefs.GetString($"cf_level_unlock_booster", ValueDefaultLevelConfigBooster);
        set => PlayerPrefs.SetString($"cf_level_unlock_booster", value);
    }

    public static int NumWatchAdsDaily
    {
        get
        {
            long now = MGTime.GetUtcTime();
            DateTime dateTimeNow = SdkUtil.timeStamp2DateTime(now);
            DateTime dateOnly = dateTimeNow.Date;
            var date = SdkUtil.toTimestamp(dateOnly).ToString();
            if (date != LastDateWatchAds)
            {
                PlayerPrefs.SetInt($"num_watch_ads_daily", 0);
                LastDateWatchAds = date;
            }

            return PlayerPrefs.GetInt($"num_watch_ads_daily", 0);
        }
        set
        {
            long now = MGTime.GetUtcTime();
            DateTime dateTimeNow = SdkUtil.timeStamp2DateTime(now);
            DateTime dateOnly = dateTimeNow.Date;
            var date = SdkUtil.toTimestamp(dateOnly).ToString();
            if (date != LastDateWatchAds)
            {
                PlayerPrefs.SetInt($"num_watch_ads_daily", 0);
                LastDateWatchAds = date;
            }

            PlayerPrefs.SetInt($"num_watch_ads_daily", value);
        }
    }

    public static string LastDateWatchAds
    {
        get => PlayerPrefs.GetString($"last_watch_ads_date", "");
        set => PlayerPrefs.SetString($"last_watch_ads_date", value);
    }
    static Dictionary<BoosterType, int> DicBoosterTypeConfig = new Dictionary<BoosterType, int>();

    public static int GetLevelUnlockBooster(BoosterType boosterType)
    {
        return BoosterConfig.GetConfigData(boosterType).levelUnlock;
    }

    public static int NumWatchAds
    {
        get { return PlayerPrefs.GetInt($"num_watch_ads", 0); }
        set { PlayerPrefs.SetInt($"num_watch_ads", value); }
    }

    public static int NumWatchAdsConfig
    {
        get { return PlayerPrefs.GetInt($"cf_num_watch_ads", 5); }
        set { PlayerPrefs.SetInt($"cf_num_watch_ads", value); }
    }

    public static int NumWatchAdsDailyConfig
    {
        get => PlayerPrefs.GetInt($"cf_num_watch_ads_daily", 5);
    }
    public static Action<BoosterType> CBOnUseBooster;
    public static Action<bool> CBOnUseBoosterFirstTime;
    public static bool IsOverAds => NumWatchAds >= NumWatchAdsConfig && NumWatchAdsConfig >= 0;
    public static bool IsOverAdsDaily => NumWatchAdsDaily >= NumWatchAdsDailyConfig && NumWatchAdsDailyConfig >= 0;
    public static void SetNumWatchAdsDailyConfig(int value)
    {
        PlayerPrefs.SetInt($"cf_num_watch_ads_daily", value);
    }
    public static bool IsTutorialDone(BoosterType boosterType)
    {
        return GetNumBoosterUsed(boosterType) > 0;
        //return PlayerPrefs.GetInt($"tutorial_booster_{(int)boosterType}", 0) != 0;
    }

    public static void SetTutorialDone(BoosterType boosterType, int value = 1)
    {
        PlayerPrefs.SetInt($"tutorial_booster_{(int)boosterType}", value);
    }

    public static bool IsShowTutorial(BoosterType boosterType)
    {
        return PlayerPrefs.GetInt($"show_tutorial_booster_{(int)boosterType}", 0) != 0;
    }

    public static void SetShowTutorial(BoosterType boosterType, int value = 1)
    {
        PlayerPrefs.SetInt($"show_tutorial_booster_{(int)boosterType}", value);
    }

    public static long GetEndTimeUnlimited(BoosterType boosterType)
    {
        long.TryParse(PlayerPrefs.GetString($"unlimited_booster_{(int)boosterType}", "0"), out long res);
        return res;
    }

    public static void SetEndTimeUnlimited(BoosterType boosterType, long value)
    {
        PlayerPrefs.SetString($"unlimited_booster_{(int)boosterType}", value.ToString());
    }

    public static int GetNumBoosterUsed(BoosterType boosterType)
    {
        return PlayerPrefs.GetInt($"num_booster_used_{(int)boosterType}", 0);
    }

    public static void SetNumBoosterUsed(BoosterType boosterType, int val)
    {
        PlayerPrefs.SetInt($"num_booster_used_{(int)boosterType}", val);
    }

    public int GetNumBooster(BoosterType boosterType)
    {
        var boosterPrice = boosterPrices.SingleOrDefault(x => x.type == boosterType);
        if (boosterPrice == null) return 0;
        int basePrice = boosterPrice.amount;
        return basePrice;
    }

    public void UseBooster(BoosterType boosterType, int val)
    {
        if (GetInfo(boosterType).inGameOnly) return;
        //Debug.Log("BuyBooster");
        GameRes.AddRes(resType[boosterType], -val);
        List<DataResource> dataResources = new List<DataResource>();
        DataResource data = new DataResource();
        data.amount = -1;
        data.resType = resType[boosterType];
        dataResources.Add(data);
        SetNumBoosterUsed(boosterType, GetNumBoosterUsed(boosterType) + val);
        CBOnUseBooster?.Invoke(boosterType);
        LogEventCustom.LogResource(LogEvent.ReasonItem.use.ToString(), "in_game", new DataResource
        {
            amount = -1,
            resType = resType[boosterType]
        });

        if (LevelManager.DictBoosterUsed.ContainsKey(boosterType))
        {
            LevelManager.DictBoosterUsed[boosterType] += val;
        }
        else
        {
            LevelManager.DictBoosterUsed[boosterType] = val;
        }
    }

    public static bool IsGiftedBooster(BoosterType boosterType)
    {
        return PlayerPrefsUtility.GetBool($"booster_gifted_{(int)boosterType}", false);
    }

    public static void SetGiftedBooster(BoosterType boosterType, bool val)
    {
        PlayerPrefsUtility.SetBool($"booster_gifted_{(int)boosterType}", val);
    }

    public static string CF_Price_Booster
    {
        get { return PlayerPrefs.GetString("cf_price_booster", ""); }
    }

    public static int CF_ExtraSlot_Price_Increase
    {
        get => PlayerPrefs.GetInt("cf_extraslot_price_increase", 150);
        set => PlayerPrefs.SetInt("cf_extraslot_price_increase", value);
    }


    public static void AddTimeUnlimited(BoosterType boosterType, int timeSecond)
    {
        long endTime = GetEndTimeUnlimited(boosterType);
        long curTime = MGTime.GetUtcTime();
        if (endTime < curTime)
        {
            endTime = curTime;
        }

        endTime += (long)timeSecond * 1000;
        SetEndTimeUnlimited(boosterType, endTime);
    }

    private Dictionary<BoosterType, RES_type> resType = new Dictionary<BoosterType, RES_type>()
    {
        { BoosterType.Hand, RES_type.Hand },
        { BoosterType.Shuffle, RES_type.Shuffle },
        { BoosterType.Clear, RES_type.Clear },
        { BoosterType.ExtraSlot, RES_type.ExtraSlot },
    };

    public void LoadConfig()
    {
        if (!string.IsNullOrEmpty(CF_Price_Booster))
        {
            try
            {
                
                var boosterPricesConfig =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<BoosterPrice[]>(CF_Price_Booster);
                if (boosterPricesConfig.Length == boosterInfos.Length)
                {
                    boosterPrices = boosterPricesConfig;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    private void Awake()
    {
        Instance = this;
        GameEvent.LoadNewLevel += ResetNumWatchAds;
        GameEvent.OnReceiveFirebaseDataDone += LoadConfig;
        LoadConfig();
    }

    private void OnDestroy()
    {
        GameEvent.LoadNewLevel -= ResetNumWatchAds;
        GameEvent.OnReceiveFirebaseDataDone -= LoadConfig;
    }

    public static void ResetNumWatchAds()
    {
        NumWatchAds = 0;
    }

    public int GetPrice(BoosterType type)
    {
        var boosterPrice = boosterPrices.SingleOrDefault(x => x.type == type);
        if (boosterPrice == null) return 0;
        int basePrice = boosterPrice.price;
        return basePrice;
    }

    public BoosterPrice GetBoosterPriceConfig(BoosterType type)
    {
        var res = boosterPrices.SingleOrDefault(x => x.type == type);
        res.typeBuy = Mathf.Clamp(res.typeBuy, 0, 2);
        return res;
    }

    public BoosterInfo GetInfo(BoosterType type)
    {
        return boosterInfos.SingleOrDefault(x => x.type == type);
    }

    public RES_type GetResType(BoosterType type)
    {
        return resType[type];
    }

    public bool TryGetResType(BoosterType type, out RES_type res)
    {
        return resType.TryGetValue(type, out res);
    }

    public int BoosterAmount(BoosterType type)
    {
        if (GetInfo(type).inGameOnly) return 0;
        var typeRes = resType[type];
        if (IsLogAddTutorial(typeRes))
        { 
            LogAddTutorial(typeRes);
            LogEventCustom.LogResource("start_game", "in_game", new DataResource
            {
                amount = 0,
                resType = typeRes
            });
        }

        return GameRes.getRes(typeRes, 0);
    }

    private bool IsLogAddTutorial(RES_type type)
    {
        return PlayerPrefs.GetInt($"log_add_tutorial_{type.ToString()}", 0) == 0;
    }

    private void LogAddTutorial(RES_type type)
    {
        PlayerPrefs.SetInt($"log_add_tutorial_{type.ToString()}", 1);
    }

    public void ActiveBooster(BoosterType type, bool inRevive, bool isFree = false)
    {
        isFreeBooster = isFree;
        if (type == BoosterType.Shuffle)
        {
            OnUseBooster(type, inRevive);
        }
        else if (type == BoosterType.Hand)
        {
            OnUseBooster(type, inRevive);
        }
        else if (type == BoosterType.Clear)
        {
            OnUseBooster(type, inRevive);
        }
    }

    public void OnUseBooster(BoosterType type, bool inRevive)
    {
        Instance.UseBooster(type, 1);
        LogFireBaseCustomer.UseBooster(type);
        activeBooster = BoosterType.None;

        GameManager.Instance.ResumeGame();

        var uiTutorial = UIManager.Instance.GetPopupActive<UITutorial>();
        if (uiTutorial != null)
        {
            uiTutorial.Hide();
        }

        var uiInGame = UIManager.Instance.GetScreenActive<UIInGame>();
        uiInGame.HideUseBoosterUI();
    }

    public void CancelUseBooster()
    {
        if (activeBooster == BoosterType.None) return;
        var uiTutorial = UIManager.Instance.GetPopupActive<UITutorial>();
        if (uiTutorial != null)
        {
            uiTutorial.Hide();
        }

        var uiInGame = UIManager.Instance.GetScreenActive<UIInGame>();
        uiInGame.HideUseBoosterUI();
        // FIRhelper.logEvent("booster_cancel_break_object");
        // FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_cancel_break_object");
    }
    
    public void AddBooster(BoosterType type, int value, string where, LogEvent.ReasonItem reason)
    {
        if (GetInfo(type).inGameOnly) return;
        DataManager.Instance.ReceiveGift(false, 0, where, reason, value > 0, DataManager.Level,
            new ItemInfo(null, resType[type], value));
    }

    public void LogBooster(BoosterType type, int value, string where, LogEvent.ReasonItem reason)
    {
        if (GetInfo(type).inGameOnly) return;
        var itemInfo = new ItemInfo(null, resType[type], value);
        LogEvent.ResourceSink(where, new[] { itemInfo }, reason, DataManager.Level);
    }

    public void OnSyncData(UserData newData)
    {
        if (newData.game_info != null)
        {
            if (newData.game_info.IsTutBreakObject)
            {
                SetTutorialDone(BoosterType.Shuffle);
            }

            if (newData.game_info.IsTutAddHole)
            {
                SetTutorialDone(BoosterType.Hand);
            }

            if (newData.game_info.IsTutClear)
            {
                SetTutorialDone(BoosterType.Clear);
            }

            if (newData.game_info.IsTutMutilColoxBox)
            {
                SetTutorialDone(BoosterType.MutilColorBox);
            }

            if (newData.game_info.IsTutMagnet)
            {
                SetTutorialDone(BoosterType.Magnet);
            }

            SetEndTimeUnlimited(BoosterType.Magnet, newData.game_info.InfinityUnlimitedMagnet);
            SetEndTimeUnlimited(BoosterType.MutilColorBox, newData.game_info.InfinityUnlimitedBox);
        }
        else
        {
            SetTutorialDone(BoosterType.Shuffle, 0);
            SetTutorialDone(BoosterType.Hand, 0);
            SetTutorialDone(BoosterType.Clear, 0);
            SetTutorialDone(BoosterType.MutilColorBox, 0);
            SetTutorialDone(BoosterType.Magnet, 0);
            SetEndTimeUnlimited(BoosterType.Magnet, 0);
            SetEndTimeUnlimited(BoosterType.MutilColorBox, 0);
        }
    }

    public void SendSyncData(UserData dataUser)
    {
        if (dataUser != null && dataUser.game_info != null)
        {
            dataUser.game_info.IsTutBreakObject = IsTutorialDone(BoosterType.Shuffle);
            dataUser.game_info.IsTutAddHole = IsTutorialDone(BoosterType.Hand);
            dataUser.game_info.IsTutClear = IsTutorialDone(BoosterType.Clear);
            dataUser.game_info.IsTutMagnet = IsTutorialDone(BoosterType.Magnet);
            dataUser.game_info.IsTutMutilColoxBox = IsTutorialDone(BoosterType.MutilColorBox);
            dataUser.game_info.InfinityUnlimitedBox = GetEndTimeUnlimited(BoosterType.MutilColorBox);
            dataUser.game_info.InfinityUnlimitedMagnet = GetEndTimeUnlimited(BoosterType.Magnet);
        }
    }
}