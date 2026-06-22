//#define ENABLE_LOG_SERVER_TEST

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DG.Tweening;
using master;
using mygame.sdk;
using MyJson;
using Newtonsoft.Json;
using time;
using UnityEngine;
using UnityWebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

using Newtonsoft.Json.Linq;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;
using System.Xml.Linq;

public class IgnoreEmptyStringResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization serialization)
    {
        var prop = base.CreateProperty(member, serialization);

        if (prop.PropertyType == typeof(string))
        {
            prop.ShouldSerialize = instance =>
            {
                var value = prop.ValueProvider.GetValue(instance) as string;
                return !string.IsNullOrEmpty(value);
            };
        }

        if (prop.PropertyType == typeof(double))
        {
            prop.ShouldSerialize = instance =>
            {
                var value = prop.ValueProvider.GetValue(instance);
                return !value.Equals(-1.0);
            };
        }

        if (prop.PropertyType == typeof(int))
        {
            prop.ShouldSerialize = instance =>
            {
                var value = prop.ValueProvider.GetValue(instance);
                return !value.Equals(-1);
            };
        }

        return prop;
    }
}

public class LogEventCustom : master.Singleton<LogEventCustom>
{
    public static bool CF_EnableLogRequestAssets
    {
        get => PlayerPrefs.GetInt("cf_enable_log_request_assets", 1) == 1;
        set => PlayerPrefs.SetInt("cf_enable_log_request_assets", value ? 1 : 0);
    }

    private static int _logSequenceID
    {
        get => PlayerPrefs.GetInt("logsequenceid", 1);
        set => PlayerPrefs.SetInt("logsequenceid", value);
    }

    private int logSequenceID
    {
        get
        {
            int id = _logSequenceID;
            _logSequenceID++;
            if (_logSequenceID >= int.MaxValue - 1)
            {
                _logSequenceID = 1;
            }

            return id;
        }
    }

    public static string User_ID
    {
        get
        {
            var v = PlayerPrefs.GetString("user_id", "");
            if (!string.IsNullOrEmpty(v)) return v;
            v = _logSequenceID != 1 ? SystemInfo.deviceUniqueIdentifier : Guid.NewGuid().ToString();
            PlayerPrefs.SetString("user_id", v);
            return v;
        }
    }

    public static string GetDeviceModel()
    {
#if UNITY_IOS || UNITY_IPHONE
        return UnityEngine.iOS.Device.generation.ToString();
#else
        return SystemInfo.deviceModel;
#endif
    }

    public static string PlayerName => UserDataManager.Instance.GetDataUser().name;

    public static string AppName => Application.productName;

    public static int SessionNumber
    {
        get
        {
            var v = PlayerPrefs.GetInt("session_number", 0);
            if (v != 0 || UserDataManager.Instance.UserData.section_login == 0) return v;
            v = UserDataManager.Instance.UserData.section_login;
            PlayerPrefs.SetInt("session_number", v);

            return v;
        }
        set => PlayerPrefs.SetInt("session_number", value);
    }

    public static int InstallDay
    {
        get
        {
            var v = UserDataManager.FirstTimeJoinGame;
            if (v == 0)
            {
                v = MGTime.GetUtcTime();
            }

            return int.Parse(MGTime.TimestampToDateTime(v).ToString("yyyyMMdd"));
        }
    }

    public static string SessionID => LogEventManager.SessionID;

    public static int RetentionDay
    {
        get
        {
            var v = UserDataManager.FirstTimeJoinGame;
            if (v == 0)
            {
                return 0;
            }
            else
            {
                return (int)((MGTime.GetUtcTime() - v) / 86400000L);
            }
        }
    }

    public static int ActiveDay
    {
        get
        {
            var v = PlayerPrefs.GetInt("active_day", -1);
            if (v != -1 || UserDataManager.Instance.UserData.day_login == 0) return v;
            v = UserDataManager.Instance.UserData.day_login;
            PlayerPrefs.SetInt("active_day", v);
            return v;
        }
        set => PlayerPrefs.SetInt("active_day", value);
    }

    public static int IsAllowTracking
    {
        get
        {
            var val = PlayerPrefs.GetInt("is_allow_tracking", 0);
            if (val == -1)
            {
                return 0;
            }

            if (val == 0)
            {
                return -1;
            }

            return 1;
        }
    }

    public static int PlayCount
    {
        get => PlayerPrefs.GetInt("play_count", 0);
        set => PlayerPrefs.SetInt("play_count", value);
    }

    public static int LoseCount
    {
        get => PlayerPrefs.GetInt("lose_count", 0);
        set => PlayerPrefs.SetInt("lose_count", value);
    }

    public static int ExitCount
    {
        get => PlayerPrefs.GetInt("exit_count", 0);
        set => PlayerPrefs.SetInt("exit_count", value);
    }
    
    public static string UserSegment
    {
        get => PlayerPrefs.GetString("user_segment", "default");
        set => PlayerPrefs.SetString("user_segment", value);
    }

    //========
    private bool isReady;
    private bool initialized;

    protected override void Awake()
    {
        base.Awake();
        var v = PlayerPrefs.GetInt("log_session_number", 0);
        if (v == 0)
        {
            PlayerPrefs.SetInt("log_session_number", LogEventManager.SessionNumber);
        }

        LogEventManager.OnProvideProperties -= OnProvideProperties;
        LogEventManager.OnProvideProperties += OnProvideProperties;
    }

    private void OnDestroy()
    {
        LogEventManager.OnProvideProperties -= OnProvideProperties;
    }

    private Dictionary<string, object> OnProvideProperties()
    {
        var userData = UserDataManager.Instance.UserData;
        var utcNow = MGTime.GetUtcTime();
        var now = MGTime.TimestampToDateTime(utcNow);
        var guild = ""; // Guild module removed
        var logDict = new Dictionary<string, object>
        {
            { "sequenceId", logSequenceID },
            { "_ts", utcNow },
            { "user_id", User_ID },
            { "player_name", PlayerName },
            { "player_id", userData.playerUUID },
            { "app_name", AppName },
            { "app_version", Application.version },
            { "language", GameHelper.Instance.languageCode },
            { "country", GameHelper.Instance.countryCode },
            { "device_info", $"{GetDeviceModel()},{SystemInfo.deviceName}" },
            { "event_date", int.Parse(now.ToString("yyyyMMdd")) },
            //{ "session_number", LogEventCustom.SessionNumber },
            { "session_id", SessionID },
            { "install_day", InstallDay },
            { "retention_day", RetentionDay },
            { "active_day", ActiveDay },
            { "local_hour", DateTime.Now.Hour },
            { "local_weekday", $"{((int)now.DayOfWeek + 1)} - {now.DayOfWeek}" },
            { "allow_tracking", IsAllowTracking },
            {
                "platform",
#if UNITY_ANDROID
                "Android"
#elif UNITY_IOS || UNITY_IPHONE
        "IOS"
#else
        "Default"
#endif
            },
            { "connection_type", GetConectionType() },
            { "guild", guild },
            { "media_source", SDKManager.Instance.mediaSource },
            { "campaign", SDKManager.Instance.mediaCampain },
            { "af_adset", SDKManager.Instance.afAdset },
            { "af_ad", SDKManager.Instance.afAd },
            { "af_ad_id", SDKManager.Instance.afAdId },
            { "current_level", GameManager.CurrentLevel },
            { "current_mode", (int)GameManager.CurrentMode },
            { "win_streak", DataManager.Instance.ConsecutiveWin },
            { "lose_streak", DataManager.Instance.ConsecutiveLose },
            { "balance_coin", GameRes.getRes(RES_type.GOLD) },
            { "balance_star", GameRes.getRes(RES_type.Star) },
            { "balance_hand", BoosterManager.Instance.BoosterAmount(BoosterType.Hand) },
            { "balance_shuffle", BoosterManager.Instance.BoosterAmount(BoosterType.Shuffle) },
            { "balance_clear", BoosterManager.Instance.BoosterAmount(BoosterType.Clear) },
            { "balance_extra_slot", GameRes.getRes(RES_type.ExtraSlot) },
            // { "balance_mutil_color_box", GameRes.getRes(RES_type.MutilColorBox) },
            { "balance_heart", HeartManager.Instance.CurrentHeart },
            { "timestamp", utcNow },
            { "user_segment", UserSegment},
        };
        if (HeartManager.InfinityEndTime > utcNow)
            logDict["balance_heart_time"] = (int)(HeartManager.InfinityEndTime - utcNow);

        // var t1 = BoosterManager.GetEndTimeUnlimited(BoosterType.MutilColorBox);
        // if (t1 > utcNow)
        //     logDict["balance_unlimited_mutil_color_box"] = (int)(t1 - utcNow);
        //
        // var t2 = BoosterManager.GetEndTimeUnlimited(BoosterType.Magnet);
        // if (t2 > utcNow)
        //     logDict["balance_unlimited_magnet"] = (int)(t2 - utcNow);

        var t3 = DoubleRewardManager.GetEndTimeUnlimited();
        if (t3 > utcNow)
            logDict["balance_double_reward"] = (int)(t3 - utcNow);
        return logDict;
    }

    private void Start()
    {
    }

    public void LoadingStart(string placement)
    {
        LogEventManager.Instance.LogEvent("loading_start", new()
        {
            { "placement", placement }
        });
    }

    public void LoadingFinish(string placement, int duration)
    {
        LogEventManager.Instance.LogEvent("loading_finish", new Dictionary<string, object>
        {
            { "placement", placement },
            { "load_time", duration },
        });
    }

    public void LogLevelStart(int level, int levelId, int mode, string playType = null)
    {
        LogEventManager.Instance.LogEvent("level_start", new Dictionary<string, object>
            {
                { "level", level },
                { "level_id", levelId },
                { "play_type", playType },
                { "play_index", 0 },
                { "lose_index", 0 },
                { "total_duration_start", 0 },
                { "mode", mode },
            });
    }
    public void LogLevel(ELogEventName eventName, int level, int levelId, int mode, int playIndex,
        string reviveType, int currentWinStrike, int currentLoseStrike, int levelProgress = -1, int unpinScrew = -1,
        long durationTotal = 0, string result = null, string playType = null, int totalBus = 0, int baseSlot = 0, int finalSlot = 0, int completedBus = 0,
        string reason = "default", string useBoosterQty = "0", string useBoosterName = "default", string levelProgressDetail = null, string levelDifficulty = "normal")
    {
        if (playIndex >= 0) playIndex += 1;
        if (eventName == ELogEventName.level_play)
        {
            LogEventManager.Instance.LogEvent(eventName.ToString(), new Dictionary<string, object>
            {
                { "level", level },
                { "level_id", levelId },
                { "mode", mode },
                { "play_index", playIndex },
                { "play_type", playType },
                { "total_bus", totalBus },
                { "base_slot", baseSlot },
                { "level_difficulty", levelDifficulty }
            });
        }
        else if (eventName == ELogEventName.level_end)
        {
            LogEventManager.Instance.LogEvent(eventName.ToString(), new Dictionary<string, object>
            {
                { "level", level },
                { "level_id", levelId },
                { "mode", mode },
                { "play_index", playIndex },
                { "play_time", durationTotal },
                { "level_progress", levelProgress },
                { "level_progress_detail", levelProgressDetail },
                { "result", result },
                { "total_bus", totalBus },
                { "completed_bus", completedBus },
                { "base_slot", baseSlot },
                { "final_slot", finalSlot },
                { "reason", reason },
                { "use_booster_qty", useBoosterQty },
                { "use_booster_name", useBoosterName },
                { "level_difficulty", levelDifficulty }
            });
        }
        else
        {
            LogEventManager.Instance.LogEvent(eventName.ToString(), new Dictionary<string, object>
            {
                { "level", level },
                { "level_id", levelId },
                { "mode", mode },
                { "play_index", playIndex },
                { "play_type", playType },
                { "play_time", durationTotal },
                { "level_progress", levelProgress },
                { "resource_type", reviveType },
            });
        }
    }

    public static void LogResource(string reason, string position, params DataResource[] items)
    {
        if (items == null || items.Length == 0) return;

        ExtractLogInfo(items, out var resource_type, out var resource_name, out var resource_amount);
        var eventName = items[0].amount < 0 ? "resource_sink" : "resource_earn";

        var logParams = new Dictionary<string, object>
        {
            { "resource_type", resource_type },
            { "resource_name", resource_name },
            { "resource_amount", resource_amount },
            { "reason", reason },
            { "position", position },
        };
        LogEventManager.Instance.LogEvent(eventName, logParams);
    }

    public static void LogResource(string reason, string position, params PackageData.ItemBuyInfo[] items)
    {
        if (items == null || items.Length == 0) return;

        ExtractLogInfo(items, out var resource_type, out var resource_name, out var resource_amount);
        var eventName = items[0].itemAmount < 0 ? "resource_sink" : "resource_earn";

        var logParams = new Dictionary<string, object>
        {
            { "resource_type", resource_type },
            { "resource_name", resource_name },
            { "resource_amount", resource_amount },
            { "reason", reason },
            { "position", position },
        };
        LogEventManager.Instance.LogEvent(eventName, logParams);
    }

    public static void LogResource(string reason, string position, params ItemInfo[] items)
    {
        if (items == null || items.Length == 0) return;

        ExtractLogInfo(items, out var resource_type, out var resource_name, out var resource_amount);
        var eventName = items[0].itemAmount > 0 ? "resource_earn" : "resource_sink";

        var logParams = new Dictionary<string, object>
        {
            { "resource_type", resource_type },
            { "resource_name", resource_name },
            { "resource_amount", resource_amount },
            { "reason", reason },
            { "position", position },
        };
        LogEventManager.Instance.LogEvent(eventName, logParams);
    }

    private static void ExtractLogInfo(DataResource[] reward, out string resource_type, out string resource_name,
        out string resource_amount)
    {
        resource_type = string.Join(",", reward.Select(r => $"[{GetResourceType(r.resType)}]"));
        resource_name = string.Join(",", reward.Select(r => $"[{r.resType}]"));
        resource_amount = string.Join(",", reward.Select(r => $"[{Mathf.Abs(r.amount)}]"));
    }

    private static void ExtractLogInfo(ItemInfo[] reward, out string resource_type, out string resource_name,
        out string resource_amount)
    {
        resource_type = string.Join(",", reward.Select(r => $"[{GetResourceType(r.itemType)}]"));
        resource_name = string.Join(",", reward.Select(r => $"[{r.itemType}]"));
        resource_amount = string.Join(",", reward.Select(r => $"[{Mathf.Abs(r.itemAmount)}]"));
    }

    private static void ExtractLogInfo(PackageData.ItemBuyInfo[] reward, out string resource_type,
        out string resource_name,
        out string resource_amount)
    {
        resource_type = string.Join(",", reward.Select(r => $"[{GetResourceType(r.itemType)}]"));
        resource_name = string.Join(",", reward.Select(r => $"[{r.itemType}]"));
        resource_amount = string.Join(",", reward.Select(r => $"[{Mathf.Abs(r.itemAmount)}]"));
    }

    public void LogResourceSink(string reason, string where, int level = -1, params PackageData.ItemBuyInfo[] items)
    {
        LogResource(reason, where, items);
    }

    public void LogResourceSink(string reason, string where, int level = -1, params ItemInfo[] items)
    {
        LogResource(reason, where, items);
    }

    public void LogResourceEarn(string reason, string where, int level = -1, params ItemInfo[] items)
    {
        LogResource(reason, where, items);
    }

    public void LogResourceEarn(string reason, string where, int level = -1, params PackageData.ItemBuyInfo[] items)
    {
        LogResource(reason, where, items);
    }

    public void LogBuyInapp(string pkg, string where, string show_type, string show_action, decimal price,
        int level = -1, params DataResource[] items)
    {
        LogEventManager.Instance.LogEvent("iap_purchase", new Dictionary<string, object>
        {
            { "pack_name", pkg },
            { "price", price },
            { "currency", InappHelper.Instance.CurrencyCode },
            { "position", where },
            { "show_type", show_type },
            { "show_action", show_action },
        });
    }

    public void LogShowInapp(string where, string show_type, string show_action, string pack_name, int level = -1)
    {
        LogEventManager.Instance.LogEvent("iap_show", new Dictionary<string, object>
        {
            { "pack_name", pack_name },
            { "position", where },
            { "show_type", show_type },
            { "show_action", show_action },
        });
    }

    public void LogInappClick(string pkg, string show_type, string where, string show_action, int level = -1)
    {
        LogEventManager.Instance.LogEvent("iap_click", new Dictionary<string, object>
        {
            { "pack_name", pkg },
            { "position", where },
            { "show_type", show_type },
            { "show_action", show_action },
        });
    }

    public void LogPlayerAction(string action, int level = -1, params DataResource[] items)
    {
        LogEventManager.Instance.LogEvent("player_action", new Dictionary<string, object>
        {
            { "level", level },
            { "name", action },
        });
    }

    public void LogABTest(string evName, int value)
    {
        return;
    }

    public void LogNotificationSend(string notiName, string notiCate, int status)
    {
        return;
        LogEventManager.Instance.LogEvent("noti_send", new Dictionary<string, object>
        {
            { "noti_name", notiName },
            { "noti_cate", notiCate },
            { "noti_status", status },
        });
    }

    public void LogNotificationOpen(string notiName, string notiCate)
    {
        return;
        LogEventManager.Instance.LogEvent("noti_open", new Dictionary<string, object>
        {
            { "noti_name", notiName },
            { "noti_cate", notiCate },
        });
    }

    public void EventFirstShow(string evName, LogEvent.EventPlacement placement)
    {
        return;
        LogEventManager.Instance.LogEvent("event_first_show", new Dictionary<string, object>
        {
            { "event_name", evName },
            { "placement", placement },
        });
    }

    public void LogEventOpen(string evName, LogEvent.EventPlacement placement, int openIndex)
    {
        return;
        LogEventManager.Instance.LogEvent("event_open", new Dictionary<string, object>
        {
            { "event_name", evName },
            { "placement", placement },
            { "open_index", openIndex },
        });
    }

    public void LogEventDecor(string itemName, int itemTotal, int decorMap, int decorCurrentItem, int decorMapTotal)
    {
        return;
        LogEventManager.Instance.LogEvent("decor_complete_item", new Dictionary<string, object>
        {
            { "decor_item_name", itemName },
            { "decor_item_total", itemTotal },
            { "decor_map", decorMap },
            { "decor_current_item", decorCurrentItem },
            { "decor_map_total", decorMapTotal },
        });
    }

    public void LogEventDecorCompleteMap(int decorMap, int decorMapTotal)
    {
        return;
        LogEventManager.Instance.LogEvent("decor_complete_map", new Dictionary<string, object>
        {
            { "decor_map", decorMap },
            { "decor_map_total", decorMapTotal },
        });
    }

    public void LogEventEventClose(string evName, LogEvent.EventCloseReason reason, LogEvent.EventPlacement placement,
        int duration, int openIndex)
    {
        return;
        LogEventManager.Instance.LogEvent("event_close", new Dictionary<string, object>
        {
            { "event_name", evName },
            { "placement", placement },
            { "open_index", openIndex },
            { "reason", reason },
            { "event_duration", duration },
        });
    }

    public void LogEventEnd(string event_name, int event_duration, int event_stage, int event_ranking)
    {
        return;
        LogEventManager.Instance.LogEvent("event_end", new Dictionary<string, object>
        {
            { "event_name", event_name },
            { "event_duration", event_duration },
            { "event_stage", event_stage },
            { "event_ranking", event_ranking },
        });
    }

    public void TutorialAction(string actionCate, string actionName, int actionIndex)
    {
        return;
        LogEventManager.Instance.LogEvent("tutorial_action", new Dictionary<string, object>
        {
            { "action_cate", actionCate },
            { "action_name", actionName },
            { "action_index", actionIndex },
        });
    }

    public static bool CF_EnableLogDataBucketServer
    {
        get => PlayerPrefs.GetInt("cf_enable_log_data_bucket_server", 1) == 1;
        set => PlayerPrefs.SetInt("cf_enable_log_data_bucket_server", value ? 1 : 0);
    }

    public void LogRequestFailed(string requestName, string requestMessage, float requestTime)
    {
        return;
        if (!CF_EnableLogDataBucketServer) return;
        LogEventManager.Instance.LogEvent("request_server", new Dictionary<string, object>
        {
            { "request_name", requestName },
            { "request_message", requestMessage },
            { "request_time", requestTime },
            { "request_type", "failed" },
        });
    }

    public void LogRequestSuccess(string requestName, float requestTime)
    {
        return;
        if (!CF_EnableLogDataBucketServer) return;
        LogEventManager.Instance.LogEvent("request_server", new Dictionary<string, object>
        {
            { "request_name", requestName },
            { "request_time", requestTime },
            { "request_type", "success" }
        });
    }

    public void LogRequestError()
    {
        return;
        if (!CF_EnableLogDataBucketServer) return;
        LogEventManager.Instance.LogEvent("request_server", new Dictionary<string, object>
        {
            { "request_type", "error" }
        });
    }

    public void LogRequestTimeOut(string requestName)
    {
        return;
        if (!CF_EnableLogDataBucketServer) return;
        LogEventManager.Instance.LogEvent("request_server", new Dictionary<string, object>
        {
            { "request_name", requestName },
            { "request_type", "timeout" }
        });
    }

    public void LogRequestAssets(string pack_name, float requestTime, bool result)
    {
        return;
        if (!CF_EnableLogRequestAssets) return;
        LogEventManager.Instance.LogEvent("request_assets", new Dictionary<string, object>
        {
            { "name", pack_name },
            { "request_time", requestTime },
            { "is_load", result ? 1 : 0 }
        });
    }

    public void LogBot(int level, int level_logic, int total_screw, string selector, string selector2,
        string selector2IfNull, List<int> choke_point,
        List<int> lose_point, List<int> completed_process)
    {
        return;
        var str_choke_point = string.Join("|", choke_point);
        var str_lose_point = string.Join("|", lose_point);
        var str_completed_process = string.Join("|", completed_process);
        LogEventManager.Instance.LogEvent("bot_test_level", new Dictionary<string, object>
        {
            { "level", level },
            { "total_screw", total_screw },
            { "selector", selector },
            { "selector2", selector2 },
            { "selector2_if_null", selector2IfNull },
            { "choke_point", str_choke_point },
            { "lose_point", str_lose_point },
            { "level_logic", level_logic },
            { "completed_process", str_completed_process }
        });
    }

    public static string GetConectionType()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return "offline";
        }
        else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
        {
            return "mobile_data";
        }
        else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            return "wifi";
        }

        return "unknown";
    }

    private static Dictionary<RES_type, LogEvent.ResourceType> mapResourceType = new()
    {
        { RES_type.Ticket, global::LogEvent.ResourceType.item },
        { RES_type.Hand, global::LogEvent.ResourceType.booster },
        { RES_type.Shuffle, global::LogEvent.ResourceType.booster },
        { RES_type.Clear, global::LogEvent.ResourceType.booster },
        { RES_type.ExtraSlot, global::LogEvent.ResourceType.booster },
        { RES_type.MutilColorBox, global::LogEvent.ResourceType.booster },
        { RES_type.UnlimitedHeart, global::LogEvent.ResourceType.unlimited },
        { RES_type.UnlimitedMagnet, global::LogEvent.ResourceType.unlimited },
        { RES_type.UnlimitedMutilColorBox, global::LogEvent.ResourceType.unlimited },
        { RES_type.Heart, global::LogEvent.ResourceType.heart },
    };

    public static LogEvent.ResourceType GetResourceType(RES_type itemType)
    {
        return mapResourceType.GetValueOrDefault(itemType, global::LogEvent.ResourceType.currency);
    }

    public void LogScreenGo(string screenName, string buttonName, string prevScreenName, int durationPrevScreen)
    {
        LogEventManager.Instance.LogEvent("screen_go", new Dictionary<string, object>
        {
            { "screen_name", screenName },
            { "button_name", buttonName },
            { "prev_screen_name", prevScreenName },
            { "duration_prev_screen", durationPrevScreen }
        });
    }
}

public enum ELogEventName
{
    level_play,
    level_end,
    level_success,
    level_fail,
    level_exit,
    level_resume,
    level_show_revive,
    level_retry,
    level_second_chance,
    level_action,
    iap_purchase,
    player_action,
    all_action_click_reward,
    all_action_click_banner,
    all_action_show_inter,
    all_action_click_button,
    resource_sink,
    resource_earn,
    decorate,
    session_start,
    iap_click,
    iap_show,
    first_open,
    ab_test,
    noti_send,
    noti_open,
    event_first_show,
    event_open,
    event_close,
    loading_start,
    loading_finish,
    tutorial_action,
    request_server,
    decor_complete_item,
    decor_complete_map,
    screen_go
}