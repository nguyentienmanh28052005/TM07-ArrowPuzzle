using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public enum ConfigEventType{
    Daily = 0,
    Weekly = 1,
    Monthly = 2,
    Special = 3
}
[Serializable]
public class EventDataConfig
{
    [Space,Header("Base")]
    public ConfigEventType eventType;
    /// <summary>
    /// Level required to unlock event
    /// Level <=0 disable event
    /// Example: 39 (event will unlock when game level equal to 39)
    /// </summary>
    public int levelUnlock;
    /// <summary>
    /// Level to show unlock button outside menu
    /// If the value is 0 then display at levelUnlock
    /// Example: 35 (The button outside the main will be displayed but in gray and will say unlock at level 39)
    /// </summary>
    public int levelShowUnlock;
    /// <summary>
    /// Session count required to unlock event
    /// </summary>
    public int sessionUnlock;


    /// <summary>
    /// Level to download resources from server
    /// </summary>
    public int levelDownloadAssets;
    /// <summary>
    /// UTC hour when event can be active
    /// For each type of event the config active day method can be different but active hour can be use in common
    /// Example: 8 (event can be activated when time beyond 8 o'clock UTC of the active day)
    /// </summary>
    public int activeHour;
    /// <summary>
    /// Duration of event active state
    /// </summary>
    public float duration;
    /// <summary>
    /// Duration of a play session of some event that has particular session different to active time
    /// </summary>
    [DefaultValue(-1f)] public float playTime = -1;
    
    /// <summary>
    /// Day of week when event will be taken place. For event that will be taken place daily, this list must contain all day of week
    /// </summary>
    [Space,Header("Daily")]
    public List<DayOfWeek> daysOfWeek;
    
    /// <summary>
    /// For event that take place weekly, it uses a weekly day interval to define the day start
    /// </summary>
    [Space,Header("Weekly")]
    [DefaultValue(-1)]public int weeklyActiveDay = -1;
    /// <summary>
    ///  1: weekly,
    ///  n: once every n weeks
    /// </summary>
    public int weeklyCycle;
    /// <summary>
    /// Define which week in the cycle that event will be taken place.
    /// </summary>
    public int weeklyCycleSurplus;
    
    /// <summary>
    /// Define the day of month that event active
    /// </summary>
    [Space, Header("Monthly")]
    [DefaultValue(0)] public int monthlyStartDay = 0;
    
    /// <summary>
    /// MGTime when the special event will be taken place
    /// </summary>
    [Space, Header("Special Events")]
    [DefaultValue(0)] public long specialStartTime = 0;
    /// <summary>
    /// MGTime end time of special event
    /// </summary>
    [DefaultValue(0)] public long specialEndTime = 0;
}