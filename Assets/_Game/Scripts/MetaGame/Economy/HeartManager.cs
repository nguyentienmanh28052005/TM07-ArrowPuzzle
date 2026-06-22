using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using mygame.sdk;
using time;
using UniRx;
using UnityEngine;

public enum EHeartState
{
    None = 0,
    Normal = 1,
    Unlimited = 2
}

public class DataHeartReceive
{
    public class DataUserDonater
    {
        public string playerId;
        public string name;
        public int avatarId;
        public int frameId;
        public string avatarUrl;
    }

    public int resourceId;
    public int amount;
    public List<DataUserDonater> listDonater;
}

public class HeartManager : master.Singleton<HeartManager>, ISyncData
{
    public static int TimePerHeart => CF_RecoverTimeHeart;


    #region Pref data

    public static int CF_GoldBuyHeart
    {
        get => PlayerPrefs.GetInt("cf_gold_buy_heart", 900);
        set => PlayerPrefs.SetInt("cf_gold_buy_heart", value);
    }

    private static int? heart;
    private static int? enableHeart;
    private static int? recoverTimeHeart;

    public static int CF_RecoverTimeHeart
    {
        get
        {
            if (!recoverTimeHeart.HasValue)
                recoverTimeHeart = PlayerPrefs.GetInt("cf_recover_time_heart", 1500);

            return recoverTimeHeart.Value;
        }
        set
        {
            if (recoverTimeHeart.HasValue && recoverTimeHeart.Value == value)
                return;

            recoverTimeHeart = value;
            PlayerPrefs.SetInt("cf_recover_time_heart", value);
        }
    }
    
    public static int Heart
    {
        get
        {
            if (!heart.HasValue)
                heart = GameRes.getRes(RES_type.Heart, MAX_HEART);

            return heart.Value;
        }
        set
        {
            int clampedValue = Mathf.Clamp(value, 0, MAX_HEART);

            if (!heart.HasValue)
                heart = GameRes.getRes(RES_type.Heart, MAX_HEART);

            int oldValue = heart.Value;
            if (oldValue == clampedValue) return;

            int distance = clampedValue - oldValue;
            GameRes.AddRes(RES_type.Heart, distance);

            heart = clampedValue;
        }
    }

    public static int CF_EnableHeart
    {
        get
        {
            if (!enableHeart.HasValue)
                enableHeart = PlayerPrefs.GetInt("cf_enable_heart", 1);

            return enableHeart.Value;
        }
        set
        {
            if (enableHeart.HasValue && enableHeart.Value == value)
                return;

            enableHeart = value;
            PlayerPrefs.SetInt("cf_enable_heart", value);
        }
    }
    
    public static int HeartInGame => Mathf.Min(Heart + 1, MAX_HEART);

    public static bool IsActiveHeart => CF_EnableHeart > 0;

    public static int MAX_HEART => MGTime.GetUtcTime() < ExpiredTimeMaxHeart ? 8 : 5;

    public static long ExpiredTimeMaxHeart
    {
        get => long.Parse(PlayerPrefs.GetString("expired_time_maxHeart", "0"));
        set => PlayerPrefs.SetString("expired_time_maxHeart", value.ToString());
    }

    private static string lastTimeData
    {
        get { return PlayerPrefs.GetString("last_heart_counter_save", "0"); }
        set { PlayerPrefs.SetString("last_heart_counter_save", value); }
    }

    private static string InfinityEndTimeData
    {
        get { return PlayerPrefs.GetString("InfinityEndTime", "0"); }
        set { PlayerPrefs.SetString("InfinityEndTime", value); }
    }

    public static bool IsFirstOutOfHeart
    {
        get => PlayerPrefs.GetInt("is_first_out_of_hearts", 0) == 1;
        set => PlayerPrefs.SetInt("is_first_out_of_hearts", value ? 1 : 0);
    }

    public static int CountWatchAdsRefillHeartEachDay
    {
        get => PlayerPrefs.GetInt("count_watch_ads_refill_heart_each_day", 5);
        set => PlayerPrefs.SetInt("count_watch_ads_refill_heart_each_day", value);
    }

    #endregion

    [SerializeField] private NotificationData notificationData;


    private EHeartState heartState = EHeartState.Unlimited;

    public EHeartState HeartState
    {
        get
        {
            if (InfinityEndTime > MGTime.GetUtcTime())
            {
                heartState = EHeartState.Unlimited;
            }
            else if (Heart > 0)
            {
                heartState = EHeartState.Normal;
            }
            else
            {
                heartState = EHeartState.None;
            }

            return heartState;
        }
    }

    public bool IsUnlimitedTime => HeartState == EHeartState.Unlimited;
    public bool CannotAddHeart => IsUnlimitedTime || IsFullHeart;

    public void IsNoMoreLives(Action noHeart = null, Action hasHeart = null, bool isShowPopup = true)
    {
        var noMoreLives = CurrentHeart <= 0 && HeartState != EHeartState.Unlimited;

        // Có tim
        if (!noMoreLives)
        {
            hasHeart?.Invoke();
            return;
        }

        // Hết tim, nhưng lần đầu => tặng unlimited và coi như "đã có tim"
        if (!IsFirstOutOfHeart)
        {
            DataResource[] gift =
            {
                new DataResource
                {
                    resType = RES_type.UnlimitedHeart,
                    amount = 60 * 30, // 30 phút (nếu amount là giây)
                }
            };

            if (isShowPopup)
            {
                UIManager.Instance.ShowPopup<UIFirstOutOfHeart>(() =>
                {
                RewardReceivedHub.Instance.ShowRewardGroupImmediate(gift,
                    () =>
                    {
                        GameEvent.OnReceiveResource?.Invoke(LogEvent.ReasonItem.reward, "first_out_of_hearts", gift,
                            DataManager.Level);

                        IsFirstOutOfHeart = true; //dangvq
                    }, hasHeart);
                }).Initialized(gift);
            }
            else
            {
                hasHeart?.Invoke();
            }

            return;
        }

        // no Heart
        noHeart?.Invoke();
    }

    public int CurrentHeart => PlayerPrefsUtil.CF_EnableHeart ? Heart : 5;

    private int heartCounter;
    public int HeartCounter => heartCounter;
    public bool IsFullHeart => Heart >= MAX_HEART;

    public void AddHeart(int value, string where, LogEvent.ReasonItem reason)
    {
        switch (value)
        {
            case < 0 when HeartState == EHeartState.Unlimited:
                return;
            case < 0 when IsFullHeart:
                LastTimeAddHeart = MGTime.GetUtcTime();
                break;
        }

        DataManager.Instance.ReceiveGift(false, 0, where, reason, value > 0, DataManager.Level,
            new ItemInfo(null, RES_type.Heart, value));
    }

    public static Action<bool> OnReceiveHeartDone;

    public static ReactiveProperty<DataHeartReceive> HeartReceive { get; set; } = new(new DataHeartReceive()
        { amount = 0, listDonater = new List<DataHeartReceive.DataUserDonater>() });

    public static long LastTimeAddHeart
    {
        get => lastTimeAddHeart;
        set
        {
            lastTimeAddHeart = value;
            lastTimeData = value.ToString();
        }
    }

    private static long lastTimeAddHeart;

    public static long InfinityEndTime
    {
        get => infinityEndTime;
        set
        {
            infinityEndTime = value;
            InfinityEndTimeData = value.ToString();
        }
    }

    private static long infinityEndTime;

    private void Start()
    {
        CheckOffline();
    }

    protected bool hasRegisterEvent;
    private IDisposable subLogGame;
    public static int cachedReceiveHeart = 1;

    protected override void Awake()
    {
        base.Awake();
        lastTimeAddHeart = long.Parse(lastTimeData);
        infinityEndTime = long.Parse(InfinityEndTimeData);
        if (!hasRegisterEvent)
        {
            hasRegisterEvent = true;
            RegisterListener();
        }
    }

    private void OnDestroy()
    {
        if (hasRegisterEvent)
        {
            RemoveListener();
        }
    }

    public void RegisterListener()
    {
        OnReceiveHeartDone += OnAddHeartDone;
        NotificationHandler.NotificationOpened += NotificationOpened;
        subLogGame = SocketHub.IsLoggedGame.Subscribe(x =>
        {
            if (x)
            {
                ServerHub.GetSendRequestServer<SendRequestUser>().RequestGetResource(2);
            }
        });
    }

    private void NotificationOpened(string identifier)
    {
        if (notificationData.identifier != identifier)
            LogEvent.LogNotificationOpen("event", notificationData.identifier);
    }

    void OnAddHeartDone(bool v)
    {
        if (!v) return;
        if (cachedReceiveHeart != 0) AddHeart(cachedReceiveHeart, "sync_data", LogEvent.ReasonItem.reward);
    }

    public void RemoveListener()
    {
        subLogGame.Dispose();
        OnReceiveHeartDone -= OnAddHeartDone;
    }

    private void Update()
    {
        if (!IsActiveHeart) return;
        long now = MGTime.GetUtcTime();
        if (Heart >= MAX_HEART) return;
        long r = now - LastTimeAddHeart;
        long timeRecover = CF_RecoverTimeHeart * 1000L;
        int lives = (int)(r / timeRecover);
        int maxLives = MAX_HEART - Heart;
        if (lives > maxLives)
        {
            lives = maxLives;
        }

        if (lives > 0)
        {
            if (Heart >= MAX_HEART) return;
            AddHeart(lives, "auto_refill", LogEvent.ReasonItem.reward);

            long timeOffset = r - lives * timeRecover;
            if (lives >= maxLives)
            {
                LastTimeAddHeart = now;
            }
            else
            {
                LastTimeAddHeart = now - timeOffset;
            }
        }
    }

    public void AddHeart(int amount, int typeHeart = 0)
    {
        Debug.Log($"check heart 1: {amount}_{typeHeart}");
        if (typeHeart == 0)
        {
            int current = Heart;
            int newValue = current + amount;
            if (Heart == MAX_HEART && amount < 0)
            {
                LastTimeAddHeart = MGTime.GetUtcTime();
            }

            if (newValue < 0)
            {
                newValue = 0;
            }

            Heart = newValue;
            OnAddHeart(newValue);
        }
        else
        {
            var now = MGTime.GetUtcTime();
            if (InfinityEndTime < now)
            {
                InfinityEndTime = now + amount * 1000;
            }
            else
            {
                InfinityEndTime += amount * 1000;
            }
        }
    }

    private void OnAddHeart(int newValue)
    {
        /*if (newValue < MAX_HEART && AddTime(LastTimeAddHeart, CF_RecoverTimeHeart, 0) <= MGTime.Instance.GetUtcTime())
        {

        }*/
        if (HeartState == EHeartState.None && Heart == 0)
        {
            var data = notificationData;

            long now = MGTime.GetUtcTime();
            long saveTime = LastTimeAddHeart;
            long recoverTime = AddTime(saveTime, CF_RecoverTimeHeart, 0);

            if (data != null && data.isSendNotification)
            {
                var sendTimeInterval = recoverTime - now + CF_RecoverTimeHeart * 4 * 1000;
                var ss = NotificationHandler.SendNotification(data.identifier, data.title, data.subtitle, data.body,
                    sendTimeInterval);
                data.isSendNotification = false;

                if (ss >= 0)
                {
                    LogEvent.LogNotificationSend("event", data.identifier,
                        NotificationHandler.IsNotificationEnabled() && NotificationHandler.enableNotification ? 1 : 0);
                }
            }
        }
        else
        {
            NotificationHandler.RemoveNotification(notificationData.identifier);
            notificationData.ClearCache();
        }
    }

    public string GetTimeRemaningText(int heart = -1)
    {
        if (heart == -1) heart = Heart;
        long now = MGTime.GetUtcTime();
        if (infinityEndTime > now)
        {
            var timeLerp = infinityEndTime - now;
            var timeLeft = new TimeSpan(timeLerp * 10000);
            if (timeLeft.Hours > 0 || timeLeft.Days > 0)
            {
                return $"{(timeLeft.Hours + timeLeft.Days * 24):D2}h {timeLeft.Minutes:D2}m";
            }
            else
            {
                return $"{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
            }
        }

        if (heart < MAX_HEART)
        {
            long lastTime = LastTimeAddHeart;
            long recoverTime = AddTime(lastTime, CF_RecoverTimeHeart, 0);
            long timeDifference = recoverTime - now;
            long hours = timeDifference / 3600000;
            long minutes = (timeDifference % 3600000) / 60000;
            long seconds = (timeDifference % 60000) / 1000;
            string minuteSt = minutes < 10 ? $"0{minutes}" : minutes.ToString();
            string secondSt = seconds < 10 ? $"0{seconds}" : seconds.ToString();
            if (hours > 0)
            {
                string hourSt = hours < 10 ? $"0{hours}" : hours.ToString();
                return $"{hourSt}:{minuteSt}:{secondSt}";
            }
            else
            {
                return $"{minuteSt}:{secondSt}";
            }
        }

        return MutilLanguage.getStringWithKey("full");
    }

    public static long AddTime(long timeCurrent, int secon, int mini, int hour = 0, int day = 0)
    {
        long milliseconds = secon * 1000L;
        long minutesInMillis = mini * 60 * 1000L;
        long hoursInMillis = hour * 3600 * 1000L;
        long daysInMillis = day * 86400 * 1000L;
        long returnTime = timeCurrent + milliseconds + minutesInMillis + hoursInMillis + daysInMillis;
        return returnTime;
    }

    private void CheckOffline()
    {
        return;
        if (lastTimeData.Length == 0) return;
        long now = MGTime.GetUtcTime();
        long timeSave = LastTimeAddHeart;
        long r = now - timeSave;
        long timeRecover = CF_RecoverTimeHeart * 1000L;
        int lives = (int)(r / timeRecover);
        int maxLives = MAX_HEART - Heart;
        Debug.Log($"check heart 2:{maxLives}_{LastTimeAddHeart}_{Heart}");
        if (lives > maxLives)
        {
            lives = maxLives;
        }

        if (lives > 0)
        {
            AddHeart(lives, "offline", LogEvent.ReasonItem.reward);
        }
    }

    public void ActiveMoreHeartMax(long expiredTime)
    {
        ExpiredTimeMaxHeart = expiredTime;
    }

    public void OnSyncData(UserData newData)
    {
        HeartReceive.Value = new DataHeartReceive()
            { amount = 0, listDonater = new List<DataHeartReceive.DataUserDonater>() };
        if (newData.game_info != null)
        {
            ExpiredTimeMaxHeart = newData.game_info.ExpiredTimeMaxHeart;
            lastTimeData = newData.game_info.lastTimeData;
            InfinityEndTimeData = newData.game_info.InfinityEndTimeData;
        }
        else
        {
            ExpiredTimeMaxHeart = 0;
            lastTimeData = "0";
            InfinityEndTimeData = "0";
        }

        notificationData.ClearCache();
    }

    public void SendSyncData(UserData dataUser)
    {
        dataUser.game_info.ExpiredTimeMaxHeart = ExpiredTimeMaxHeart;
        dataUser.game_info.lastTimeData = lastTimeData;
        dataUser.game_info.InfinityEndTimeData = InfinityEndTimeData;
    }
}