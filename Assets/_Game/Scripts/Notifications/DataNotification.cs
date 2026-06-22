using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum NotificationType
{
    MagicCauldronStarted,
    TeamTreasureStarted,
    BalloonRiseStarted,
    SkyRaceStarted,
    TeamBattleStarted,
    ScrewOfTreasureStarted,
    HiddenTempleStarted,
    LavaQuestStarted,
    SpaceMissionStarted,
    PinataPartyStarted,
    LastOneDayTeamTreasure,
}

public class DataNotification : ScriptableObject
{
    
    [Serializable]
    public class Data
    {
        public NotificationType type;
        public string identifier;
        public string title;
        public string subtitle;
        public string body;
    }
    
    public Data[] data;

    public Data GetNotificationData(NotificationType notificationType)
    {
        return data.SingleOrDefault(x => x.type == notificationType);
    }
}
