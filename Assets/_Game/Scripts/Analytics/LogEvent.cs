using System;
using System.Collections;
using System.Collections.Generic;
using Myapi;
using mygame.sdk;
using Newtonsoft.Json;
using UnityEngine;

public class LogEvent : MonoBehaviour
{
    public enum IAP_ShowType
    {
        shop,
        pack
    }

    public enum IAP_ShowAction
    {
        click_button,
        auto_show,
    }

    public enum IAP_ShowPosition
    {
        home_shop,
        shop_popup,
        booster_popup,
        pack,
        home_popup,
        battle_pass,
        revive,
        refill_heart,
        ingame,
    }

    public enum ReasonItem
    {
        use,
        purchase,
        exchange,
        reward,
        watch_ads,
    }

    public enum ResourceType
    {
        currency,
        item,
        booster,
        heart,
        unlimited,
    }

    public enum LevelResult
    {
        win,
        lose,
        retry,
        exit
    }

    public enum EventPlacement
    {
        home,
        home_icon,
        home_popup,
        end_level_icon,
    }

    public enum EventCloseReason
    {
        complete,
        out_of_time,
        lose,
        close,
    }

    public static int OpenEventPopupDuration;

    public static class ScreenName
    {
        public const string Home = "home";
        public const string ShopHome = "shop_home";
        public const string ShopPlay = "shop_play";
        public const string BoosterPopup = "booster_popup";
        public const string ExtraSlot = "extra_slot";
        public const string Loading = "loading";
        public const string LevelPlay = "level_play";
        public const string LevelWin = "level_win";
        public const string LevelLose = "level_lose";
        public const string SettingHome = "setting_home";
        public const string SettingPlay = "setting_play";
    }
    
    public static class ButtonName
    {
        public const string ButtonInGame = "button_ingame";
        public const string ButtonHome = "button_home";
        public const string ButtonQuit = "button_quit";
        public const string ButtonCancelRetry = "button_cancel_retry";
        public const string ButtonSlotBar = "button_slot_bar";
        public const string ButtonCloseRevive = "button_close_revive";
    }

    private static string currentScreenName = "none";
    private static float timeEnterCurrentScreen = 0f;

    public static void ScreenGo(string screenName, string buttonName = "auto")
    {
        int durationPrevScreen = 0;
        if (timeEnterCurrentScreen > 0f)
        {
            durationPrevScreen = (int)((Time.realtimeSinceStartup - timeEnterCurrentScreen) * 1000);
        }

        string prevScreenName = currentScreenName;

        LogEventCustom.Instance.LogScreenGo(screenName, buttonName, prevScreenName, durationPrevScreen);

        currentScreenName = screenName;
        timeEnterCurrentScreen = Time.realtimeSinceStartup;
    }

    public static void ChangeScreenName(string screenName)
    {
        currentScreenName = screenName;
        timeEnterCurrentScreen = Time.realtimeSinceStartup;
    }

    public static void IAPShow(IAP_ShowType showType, IAP_ShowPosition showPosition, IAP_ShowAction showAction,
        string packName)
    {
        LogEventCustom.Instance.LogShowInapp(showPosition.ToString(), showType.ToString(), showAction.ToString(),
            packName);
    }

    public static void IAPClick(IAP_ShowType showType, IAP_ShowPosition showPosition, IAP_ShowAction showAction,
        string packName)
    {
        LogEventCustom.Instance.LogInappClick(packName, showType.ToString(), showPosition.ToString(),
            showAction.ToString());
    }

    public static void IAPBuy(IAP_ShowType showType, IAP_ShowPosition showPosition, IAP_ShowAction showAction,
        string packName)
    {
        LogEventCustom.Instance.LogBuyInapp(packName, showPosition.ToString(), showType.ToString(),
            showAction.ToString(), InappHelper.Instance.getDecimalPrice(packName));
    }

    public static string GetDifficultyString(int level)
    {
        var levelType = LevelManager.GetLevelType(level);
        switch (levelType)
        {
            case LevelType.Easy: return "easy";
            case LevelType.Hard: return "hard";
            case LevelType.Crazy: return "crazy";
            default: return "easy";
        }
    }

    public static void LevelPlay(int lv, int levelId, string playType, int playIndex, GameMode gameMode, int totalBus = 0, int baseSlot = 0)
    {
        GameManager.CurrentPlayType = playType;
        LogEventCustom.Instance.LogLevel(ELogEventName.level_play, lv, levelId, (int)gameMode, playIndex,
            "", DataManager.Instance.ConsecutiveWin, DataManager.Instance.ConsecutiveLose, -1, -1, -1, null, playType, totalBus, baseSlot, 0, 0, "default", "0", "default", null, GetDifficultyString(lv));
    }

    public static void LevelEnd(int lv, int levelId, long playTime, int playIndex, GameMode gameMode, int levelProgress, LevelResult result, int totalBus = 0, int completedBus = 0, string reason = "default", string useBoosterQty = "0", string useBoosterName = "default", int baseSlot = 0, int finalSlot = 0, string levelProgressDetail = null)
    {
        LogEventCustom.Instance.LogLevel(ELogEventName.level_end, lv, levelId, (int)gameMode, playIndex,
            "", DataManager.Instance.ConsecutiveWin, DataManager.Instance.ConsecutiveLose,
            levelProgress, -1, playTime, result.ToString(), GameManager.CurrentPlayType, totalBus, baseSlot, finalSlot, completedBus, reason, useBoosterQty, useBoosterName, levelProgressDetail, GetDifficultyString(lv));
    }

    public static void LevelExit(int lv, int levelId, long playTime, int playIndex, GameMode gameMode,
        int levelProgress, LevelResult result, int totalBus = 0, int completedBus = 0, string reason = "back_to_menu", string useBoosterQty = "0", string useBoosterName = "default", int baseSlot = 0, int finalSlot = 0, string levelProgressDetail = null)
    {
        LogEventCustom.Instance.LogLevel(ELogEventName.level_end, lv, levelId, (int)gameMode, playIndex,
            "", DataManager.Instance.ConsecutiveWin, DataManager.Instance.ConsecutiveLose,
            levelProgress, -1, playTime, result.ToString(), GameManager.CurrentPlayType, totalBus, baseSlot, finalSlot, completedBus, reason, useBoosterQty, useBoosterName, levelProgressDetail, GetDifficultyString(lv));
    }

    public static void LevelResume(int lv, int levelId, long playTime, int playIndex, GameMode gameMode,
        int levelProgress, LevelResult result, int totalBus = 0, int completedBus = 0, string reason = "default", string useBoosterQty = "0", string useBoosterName = "default", int baseSlot = 0, int finalSlot = 0, string levelProgressDetail = null)
    {
        LogEventCustom.Instance.LogLevel(ELogEventName.level_end, lv, levelId, (int)gameMode, playIndex,
            "", DataManager.Instance.ConsecutiveWin, DataManager.Instance.ConsecutiveLose,
            levelProgress, -1, playTime, result.ToString(), GameManager.CurrentPlayType, totalBus, baseSlot, finalSlot, completedBus, reason, useBoosterQty, useBoosterName, levelProgressDetail, GetDifficultyString(lv));
    }

    public static void LevelShowRevive(int lv, int levelId, int peekTurn, long playTime, int playIndex,
        GameMode gameMode, int levelProgress, LevelResult result)
    {
        LogEventCustom.Instance.LogLevel(ELogEventName.level_show_revive, lv, levelId, (int)gameMode, playIndex,
            "", DataManager.Instance.ConsecutiveWin,
            DataManager.Instance.ConsecutiveLose, levelProgress, -1, playTime, result.ToString(),
            GameManager.CurrentPlayType);
    }

    public static void LevelSecondChance(int playIndex, string reviveType, int levelProgress,
        long durationTotal, string bonusType = "vip_slot", int bonusAmount = 0)
    {
        var playType = GameManager.CurrentPlayType;
        if (playIndex >= 0) playIndex += 1;
        LogEventManager.Instance.LogEvent("level_second_chance", new Dictionary<string, object>
        {
            { "level", GameManager.CurrentLevel },
            { "mode", GameManager.CurrentMode },
            { "play_index", playIndex },
            { "revive_type", reviveType },
            { "level_progress", levelProgress },
            { "duration_total", durationTotal },
            { "play_type", playType },
            { "bonus_type", bonusType },
            { "bonus_amount", bonusAmount }
        });
    }

    public static string GetBoosterName(BoosterType type)
    {
        switch (type)
        {
            case BoosterType.ExtraSlot: return "vip_slot";
            case BoosterType.Shuffle: return "shuffle";
            case BoosterType.None: return "default";
            default: return type.ToString().ToLower();
        }
    }

    public static string GetBoosterLogName(BoosterType type)
    {
        switch (type)
        {
            case BoosterType.Clear: return "clear";
            case BoosterType.Hand: return "hand";
            case BoosterType.ExtraSlot: return "extra_slot";
            case BoosterType.Shuffle: return "shuffle";
            default: return type.ToString().ToLower();
        }
    }


    public static void ResourceEarn(string where, ItemInfo[] reward, ReasonItem reason, int level)
    {
        LogEventCustom.Instance.LogResourceEarn(reason.ToString(), where, level, reward);
    }

    public static void ResourceEarn(string where, PackageData.ItemBuyInfo[] reward, ReasonItem reason, int level)
    {
        LogEventCustom.Instance.LogResourceEarn(reason.ToString(), where, level, reward);
    }

    public static void ResourceSink(string where, ItemInfo[] reward, ReasonItem reason, int level)
    {
        LogEventCustom.Instance.LogResourceSink(reason.ToString(), where, level, reward);
    }

    public static void ResourceSink(string where, PackageData.ItemBuyInfo[] reward, ReasonItem reason, int level)
    {
        LogEventCustom.Instance.LogResourceSink(reason.ToString(), where, level, reward);
    }

    public static void PlayerChange(string change, int id)
    {
        LogEventCustom.Instance.LogPlayerAction($"change_{change}");
    }

    public static void PlayerAction(string action)
    {
        LogEventCustom.Instance.LogPlayerAction(action);
    }

    public static void LogABTest(string evName, int value)
    {
        return;
        LogEventCustom.Instance.LogABTest(evName, value);
    }

    public static void LogNotificationSend(string notiName, string notiCate, int status)
    {
        LogEventCustom.Instance.LogNotificationSend(notiName, notiCate, status);
    }

    public static void LogNotificationOpen(string notiName, string notiCate)
    {
        LogEventCustom.Instance.LogNotificationOpen(notiName, notiCate);
    }

    public static void EventFirstShow(string evName, EventPlacement placement)
    {
        LogEventCustom.Instance.EventFirstShow(evName.ToLower(), placement);
    }

    public static void LogEventDecor(string itemName, int itemTotal, int decorMap, int decorCurrentItem,
        int decorMapTotal)
    {
        LogEventCustom.Instance.LogEventDecor(itemName, itemTotal, decorMap, decorCurrentItem, decorMapTotal);
    }

    public static void LogEventDecorCompleteMap(int decorMap, int decorMapTotal)
    {
        LogEventCustom.Instance.LogEventDecorCompleteMap(decorMap, decorMapTotal);
    }

    public static void EventOpen(string evName, EventPlacement placement, int openIndex)
    {
        LogEventCustom.Instance.LogEventOpen(evName.ToLower(), placement, openIndex);
    }

    public static void TutorialAction(string action_cate, string action_name, int action_index)
    {
        LogEventCustom.Instance.TutorialAction(action_cate, action_name, action_index);
    }

    public static void EventClose(string evName, EventCloseReason reason, EventPlacement placement, int duration,
        int openIndex)
    {
        LogEventCustom.Instance.LogEventEventClose(evName.ToLower(), reason, placement, duration, openIndex);
    }

    public static void LoadingStart(string placement)
    {
        LogEventCustom.Instance.LoadingStart(placement);
    }

    public static void LoadingFinish(string placement, int duration)
    {
        LogEventCustom.Instance.LoadingFinish(placement, duration);
    }

    public static void DownloadBundle(string key, string name, int statusInternet, int isSuc)
    {
    }
}
