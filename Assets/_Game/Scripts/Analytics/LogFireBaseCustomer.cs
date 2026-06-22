using System.Collections;
using System.Collections.Generic;
using mygame.sdk;
using UnityEngine;

public class LogFireBaseCustomer
{
    public static void LogLevelPlay(int levelCurrent)
    {
        LogFire($"level_play_{levelCurrent:00000}");
    }
    public static void LogRevive(int levelCurrent, bool isReviveByAds = false, bool isReviveByGold = false, bool isShowRevive = false)
    {
        if (isReviveByAds)
        {
            LogFire($"level_{levelCurrent}_revive_ads");
        }
        else if (isReviveByGold)
        {
            LogFire($"level_{levelCurrent}_revive_gold");
        }
        else if (isShowRevive)
        {
            LogFire($"level_{levelCurrent}_show_revive");
        }
        else
        {
            LogFire($"level_{levelCurrent}_revive");
        }
    }
    public static void LogLevelEnd(int levelCurrent, bool isLevelWin = false)
    {
        if (isLevelWin)
        {
            LogFire($"level_{levelCurrent}_complete");
        }
        else
        {
            LogFire($"level_{levelCurrent}_lose");
        }
    }
    public static void LogRetryLevel(int levelCurrent)
    {
        LogFire($"level_{levelCurrent}_lose_retry");
    }
    public static void LogGetBooster(int levelCurrent, BoosterType boosterType, bool isTutorial = false, bool isBuyBooster = false, bool isBuyByAds = false, bool isBuyByGold = false)
    {
        string booster = GetStringBooster(boosterType);
        if (isTutorial)
        {
            LogFire($"level_{levelCurrent}_get_{booster}_tutorial");
        }
        else if (isBuyBooster)
        {
            if (isBuyByAds)
            {
                LogFire($"level_{levelCurrent}_get_booster_{booster}_in_buy_booster_by_ads");
            }
            else if (isBuyByGold)
            {
                LogFire($"level_{levelCurrent}_get_booster_{booster}_in_buy_booster_by_gold");
            }
        }
    }
    private static string GetStringBooster(BoosterType boosterType)
    {
        string booster = "hand";
        switch (boosterType)
        {
            case BoosterType.Hand:
                booster = "hand";
                break;
            case BoosterType.Shuffle:
                booster = "shuffle";
                break;
            case BoosterType.Clear:
                booster = "clear";
                break;
            case BoosterType.ExtraSlot:
                booster = "extra_slot";
                break;
        }
        return booster;
    }
    public static void ClickBooster(BoosterType boosterType)
    {
        string booster = GetStringBooster(boosterType);
        LogFire($"booster_click_{booster}");
    }
    public static void UseBooster(BoosterType boosterType)
    {
        string booster = GetStringBooster(boosterType);
        LogFire($"booster_use_{booster}");
    }
    public static void ClickCancleBooster(BoosterType boosterType)
    {
        string booster = GetStringBooster(boosterType);
        LogFire($"booster_cancel_{booster}");
    }
    public static void EnableMusic(bool isActive)
    {
        if (isActive)
        {
            LogFire($"enable_music_background");
        }
        else
        {
            LogFire($"disable_music_background");
        }
    }
    public static void ClickButtonCompleted(bool isx2Reward = false)
    {
        if (isx2Reward)
        {
            LogFire($"click_button_x2");
        }
        else
        {
            LogFire($"click_button_continue");
        }
    }
    private static void LogFire(string value)
    {
        FIRhelper.logEvent(value);
        Debug.Log("Log Firebase: " + value);
    }
}