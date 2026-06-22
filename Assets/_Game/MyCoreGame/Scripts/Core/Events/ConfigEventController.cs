using System;
using System.Collections.Generic;
using System.Linq;
using master;
using mygame.sdk;
using Newtonsoft.Json;
using time;
using UnityEngine;

public enum EEventConfig
{
    Event_Sky_Race = 0,
    Event_Space_Mission = 1,
    Event_Music_Tour = 2,
    Event_Train_Journey = 3,
    Event_Hidden_Temple = 4,
    Event_Pinata = 5,
    Event_Weekly_Contest = 6,
    Event_Cube_Blast = 7,
    Event_Balloon_Rise = 8,
    Event_Golden = 9,
    Event_Team_Battle = 10,
    Event_Team_Adventure = 11,
    Event_Battle_Pass = 12,
    Event_Mission_controller = 13,
    Event_Lava_Quest = 14,
    Event_Golden_2 = 15,
    Event_Ocean_Odyssey = 16,
    Event_Daily_Gift = 17,
    Event_Watch_And_Win = 18,
    Event_Piggy_Bank = 19,
    Event_Add_Slot_Refill_Pack = 20,
    Event_Hand_Refill_Pack = 21,
    Event_Clear_Refill_Pack = 22,
    Event_Shuffle_Refill_Pack = 23,
    Event_Weekend_Pack = 24,
    Event_Happy_Pack = 25,
}


public class ConfigAllEventData
{
    public Dictionary<EEventConfig, EventDataConfig> configAllEvent;
}

public static class ConfigEventController
{
    private static bool _isLoadConfig = false;
    private static ConfigAllEventData _configAllEventData;
    public static int BasePointUp = 1;
    public static List<int> MutilPointByDifficult = new List<int>() { 1, 3, 5 };

    public static int GetMutilPointByDifficult(int difficult)
    {
        return MutilPointByDifficult[Mathf.Clamp(difficult, 0, MutilPointByDifficult.Count - 1)];
    }

    public static void SetDataEventConfig(string cf)
    {
        Debug.Log($"Check_cf_all_ev = {cf}");
        try
        {
            if (!string.IsNullOrEmpty(cf))
            {
                var config = JsonConvert.DeserializeObject<ConfigAllEventData>(cf);
                _configAllEventData = config;
                SaveLoadUtil.SaveData<ConfigAllEventData>(_configAllEventData, "config_all_event.cieet", false,
                    TypeSave.None);
            }
            else if (_configAllEventData != null)
            {
                _configAllEventData.configAllEvent = null;
                SaveLoadUtil.SaveData<ConfigAllEventData>(_configAllEventData, "config_all_event.cieet", false,
                    TypeSave.None);
            }
        }
        catch (Exception e)
        {
        }
    }

    public static EventDataConfig GetDataEventConfig(EEventConfig typeEvent)
    {
        loadConfig();
        if (_configAllEventData != null && _configAllEventData.configAllEvent != null &&
            _configAllEventData.configAllEvent.ContainsKey(typeEvent) &&
            _configAllEventData.configAllEvent[typeEvent] != null)
        {
            return _configAllEventData.configAllEvent[typeEvent];
        }

        var textAsset = Resources.Load<TextAsset>($"ConfigEvent/{GetStringKey(typeEvent)}_config");
        return JsonUtility.FromJson<EventDataConfig>(textAsset.text);
    }

    private static void loadConfig()
    {
        if (_isLoadConfig) return;
        _isLoadConfig = true;
        _configAllEventData = SaveLoadUtil.LoadData<ConfigAllEventData>("config_all_event.cieet", false, TypeSave.None);
    }

    private static string GetStringKey(EEventConfig TypeEvent)
    {
        return TypeEvent.ToString().ToLower();
    }

    public static long GetTimeUntilNextDayOfWeek(int week, long fromTime, int dayOfWeek, int hour = 0)
    {
        var time = SdkUtil.timeStamp2DateTime(fromTime);
        var daysUntil = (dayOfWeek - (int)time.DayOfWeek + 7) % 7;
        if (daysUntil == 0 && time.TimeOfDay < new TimeSpan(hour, 0, 0))
        {
            daysUntil = 0;
        }
        else
        {
            daysUntil = daysUntil == 0 ? 7 : daysUntil;
        }

        return SdkUtil.toTimestamp(time.Date.AddDays(daysUntil + 7 * week).AddHours(hour));
    }

    public static long GetTimeToNearestDay(long fromTimeLong, List<DayOfWeek> days)
    {
        DateTime fromTime = SdkUtil.timeStamp2DateTime(fromTimeLong);
        for (int i = (int)fromTime.DayOfWeek + 1; i <= (int)fromTime.DayOfWeek + 7; i++)
        {
            if (days.Contains((DayOfWeek)(i % 7))) return fromTimeLong + (i - (int)fromTime.DayOfWeek) * 86400000;
        }

        return 0;
    }

    public static void GetEventTime(EventDataConfig eventDataConfig, out long activeTime, out long endTime)
    {
        long now = MGTime.GetUtcTime();
        DateTime dateTimeNow = SdkUtil.timeStamp2DateTime(now);
        switch (eventDataConfig.eventType)
        {
            case ConfigEventType.Daily:
                var dayOfWeekEndTime = eventDataConfig.daysOfWeek.Select(x => (DayOfWeek)(((int)x + (int)(eventDataConfig.activeHour + eventDataConfig.duration)/24)%7)).ToList();
                if (dayOfWeekEndTime.Any(s => s == dateTimeNow.DayOfWeek))
                {
                    var eTime = dateTimeNow.Date.AddHours((int)(eventDataConfig.activeHour + eventDataConfig.duration) % 24);
                    endTime = SdkUtil.toTimestamp(eTime);
                    if (endTime > now)
                    {
                        activeTime = endTime - (long)(eventDataConfig.duration * 3600000d);
                        return;
                    }
                }
                endTime = GetTimeToNearestDay(SdkUtil.toTimestamp(dateTimeNow.Date.AddHours((int)(eventDataConfig.activeHour + eventDataConfig.duration)%24)), dayOfWeekEndTime);
                activeTime = endTime - (long)(eventDataConfig.duration * 3600000d);
                return;
            case ConfigEventType.Weekly:
                if (eventDataConfig.weeklyCycle <= 0)
                {
                    activeTime = 0;
                    endTime = 0;
                    return;
                }

                var iEndDay = eventDataConfig.weeklyActiveDay == 0 ? 7 : eventDataConfig.weeklyActiveDay + (int)((eventDataConfig.activeHour + eventDataConfig.duration) / 24);
                var inTwoWeek = iEndDay <= 7 ? 0 : 1;
                var beyondEndTimeOfCurWeek = (iEndDay > ((int)dateTimeNow.DayOfWeek == 0 ? 7 : (int)dateTimeNow.DayOfWeek) || (iEndDay == ((int)dateTimeNow.DayOfWeek == 0 ? 7 : (int)dateTimeNow.DayOfWeek) && (eventDataConfig.activeHour + eventDataConfig.duration) % 24 > dateTimeNow.Hour)) ? 0 : 1;
                var endHour = (int)(eventDataConfig.activeHour + eventDataConfig.duration) % 24;
                var timeToCount = GetTimeUntilNextDayOfWeek(0, now, iEndDay, endHour) - 604800000;
                var weekCount = (timeToCount - 946857600000) / 604800000;
                endTime = GetTimeUntilNextDayOfWeek((eventDataConfig.weeklyCycle + eventDataConfig.weeklyCycleSurplus - (int)(weekCount % eventDataConfig.weeklyCycle) + inTwoWeek - beyondEndTimeOfCurWeek) % eventDataConfig.weeklyCycle, timeToCount, iEndDay, (eventDataConfig.activeHour + (int)eventDataConfig.duration) % 24);
                activeTime = endTime - (long)(eventDataConfig.duration * 3600000d);
                return;
            case ConfigEventType.Monthly:
                if (eventDataConfig.monthlyStartDay == 1 && eventDataConfig.duration == 744)
                {
                    endTime = MGTime.StartOfMonth(now) + eventDataConfig.activeHour * 3600000;
                    if (now < endTime)
                    {
                        activeTime = MGTime.StartOfMonth(MGTime.StartOfMonth(now) - 2) + eventDataConfig.activeHour * 3600000;
                    }
                    else
                    {
                        endTime = MGTime.EndOfMonth(now) + eventDataConfig.activeHour * 3600000;
                        activeTime = MGTime.StartOfMonth(now, eventDataConfig.monthlyStartDay) + eventDataConfig.activeHour * 3600000;
                    }
                    return;
                }
                var dayInCurrentMonth = DateTime.DaysInMonth(dateTimeNow.Year, dateTimeNow.Month);
                var endDay = eventDataConfig.monthlyStartDay + (int)(eventDataConfig.activeHour + eventDataConfig.duration)/24;
                if (endDay > dayInCurrentMonth)
                {
                    var eTime = MGTime.StartOfMonth(now, endDay-DateTime.DaysInMonth(dateTimeNow.Month == 1 ? dateTimeNow.Year-1 : dateTimeNow.Year, dateTimeNow.Month == 1 ? 12 : dateTimeNow.Month-1)) +  ((int)(eventDataConfig.activeHour + eventDataConfig.duration)%24) * 3600000;
                    if (eTime > now)
                    {
                        endTime = eTime;
                    }
                    else
                    {
                        endTime = MGTime.StartOfMonth(MGTime.EndOfMonth(now) + 2, endDay-dayInCurrentMonth) +  ((int)(eventDataConfig.activeHour + eventDataConfig.duration)%24) * 3600000;
                    }
                }
                else
                {
                    endTime = MGTime.StartOfMonth(now, endDay) +
                              ((int)(eventDataConfig.activeHour + eventDataConfig.duration) % 24) * 3600000;
                    if (endTime < now)
                    {
                        endTime = MGTime.StartOfMonth(MGTime.EndOfMonth(now) + 2, endDay) +  ((int)(eventDataConfig.activeHour + eventDataConfig.duration)%24) * 3600000;
                    }
                    
                }
                activeTime = endTime - (long)(eventDataConfig.duration * 3600000d);
                return;
            case ConfigEventType.Special:
                activeTime = eventDataConfig.specialStartTime;
                endTime = eventDataConfig.specialEndTime;
                return;
            default:
                activeTime = 0;
                endTime = 0;
                return;
        }
    }
}