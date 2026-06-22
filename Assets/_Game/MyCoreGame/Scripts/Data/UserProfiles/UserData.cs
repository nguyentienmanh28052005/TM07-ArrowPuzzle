using DG.Tweening.CustomPlugins;
using mygame.sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices.WindowsRuntime;
using master;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public class UserData
{
    #region Base Information

    public string player_uuid; // 
    public string device_id;
    public string social_id;
    public string platform;
    public string name; // 
    public string country; // 
    public string password; // 
    public string access_token;
    public string gcm_id;
    public string media_name;
    public int media_type = -1;
    public int ver_app;
    public int day_install;
    public int day_login;
    public int lastWeekLogin;
    public int lastMonthLogin;
    public int lastMonthLoginCount;
    public int lastDayLogin;
    public int section_login;
    #endregion

    #region New

    [DefaultValue(0)] public int verapp;
    [DefaultValue("")] public string playerUUID;
    [DefaultValue("")] public string userID;
    //[DefaultValue("")] public string socialId;
    [DefaultValue("")] public string appleId;
    [DefaultValue("")] public string googleId;
    [DefaultValue("")] public string facebookId;
    public int level;
    public long createdDate;
    [DefaultValue("")] public string guildId;
    public int guildLogoId;
    [DefaultValue("")] public string guildName;
    [DefaultValue("")] public string countryName;
    [DefaultValue("")] public string countryCode;
    [DefaultValue(-1)] public int positionGuild = -1;
    public int firstTryWins;
    public int helpMadeGuilds;
    public int helpMades;
    public int helpReceives;
    public int areasCompleted;
    public int continuousWin;
    public int totalWins;
    public int totalGames;
    public int continuousLogin;
    [FormerlySerializedAs("heartReceived")] public int helpsReceived;

    [DefaultValue(-1)] public int avatarId = -1;
    [DefaultValue(-1)] public int frameId = -1;
    [DefaultValue(0)] public int nameId = 0;
    public string avatarUrl;
    public bool addFriendAble;

    #endregion


    #region Data Game

    public DataGame game_info; // not save

    #endregion


    private void checkCountry()
    {
        if (string.IsNullOrEmpty(country) || country == GameHelper.CountryDefault)
        {
            country = string.IsNullOrEmpty(GameHelper.Instance.countryCode)
                ? GameHelper.CountryDefault
                : GameHelper.Instance.countryCode;
        }
    }

    private string getNewUserName()
    {
        return $"User#{Random.Range(0, 10000000):0000}";
    }

    private string getNewUserId()
    {
        return $"{AppConfig.platformName.ToLower()}_{Guid.NewGuid()}";
    }
}

[Serializable]
public class RefillPackSaveData
{
    public PackType packType;
    public bool isActive;
    public long timeStart;
    public bool hasJoined;
}

[Serializable]
public class PurchaseRecord

{
    public long timestamp;
    public float amount;
}

[Serializable]
public class DataGame
{
    public DataGame()
    {
        playerData = new PlayerData();
    }

    public PlayerData playerData;
    public byte type_player;
    public List<PurchaseRecord> purchaseHistory;
    public List<RefillPackSaveData> refillPackData;


    public int wc_weekId;
    public int wc_team;
    public bool wc_canClaimRewards;
    public bool wc_showTutorial;
    public int wc_currentScore;
    
    public bool isFirstShowChangeName;
    public long LastTimeLogin;
    public long FirstTimeJoinGame;
    public int lastNormalFrameId;
    public int lastNormalNameId;

    public int consecutivePlay;
    public int consecutiveLose;
    public int consecutiveWin;
    public List<DataResource> dataResources;
    public int StateJoinGuild;

    public bool haveLikedFacebook;
    public bool haveLikedInstagram;
    public UserFrameData userFrameData;
    public UserNameData userNameData;
    public bool HasChangeName;

    public int avatar_id
    {
        get
        {
            if (playerData != null)
            {
                return playerData.avatarID;
            }

            return Random.Range(0, 10);
        }
        set
        {
            if (playerData == null)
            {
                playerData = new PlayerData();
            }

            playerData.avatarID = value;
        }
    }

    [JsonIgnore] public bool IsPlayer => type_player == 0;

    //public DataPackage[] packages;
    public int[] typePackages;
    public bool isDailyGiftClaim;
    public int countPlayLastLevel;
    public bool isFreeTicketIfFail;

    public int roundRace2;

    //public DataEventRace dataEventRace2;
    public int prevEventRace2;

    //public DataEventRace dataEventRace;


    public long lastTimeActiveBattlePass;

    //public DataBattlePass dataBattlePass;
    public long ExpiredTimeMaxHeart;
    public string lastTimeData;
    public string InfinityEndTimeData;

    public bool IsTutBreakObject;
    public bool IsTutAddHole;
    public bool IsTutClear;
    public bool IsTutMutilColoxBox;
    public bool IsTutMagnet;
    public bool IsTutStreak;
    public long InfinityUnlimitedBox;
    public long InfinityUnlimitedMagnet;
}

[System.Serializable]
public class PlayerData
{
    public string prefData;
    public int avatarID;
    public long lastTimeLogin;
    public long firstTimeJoinGame;

    public PlayerData()
    {
        prefData = "";
        avatarID = 1;
    }
}

public class UserNameData
{
    public List<(int,long)> data;
}

public class UserFrameData
{
    public List<(int,long)> data;
}