using mygame.sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DoubleRewardManager
{
    public static Action OnAddTimeUnlimited;
    public static long timeDuration => GetEndTimeUnlimited() - SdkUtil.CurrentTimeMilis();
    public static bool IsActive;
    public static bool isActiveTime=>GetEndTimeUnlimited() > SdkUtil.CurrentTimeMilis();
    public static long GetEndTimeUnlimited()
    {
        long.TryParse(PlayerPrefs.GetString("double_reward_end_time", "0"), out long res);
        return res;
    }
    public static void AddTimeUnlimited( int timeSecond)
    {
        long endTime = GetEndTimeUnlimited();
        long curTime = SdkUtil.CurrentTimeMilis();
        if (endTime < curTime)
        {
            endTime = curTime;
        }
        endTime += (long)timeSecond * 1000;
        PlayerPrefs.SetString($"double_reward_end_time", endTime.ToString());
        OnAddTimeUnlimited?.Invoke();
    }
}
