using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Facebook.Unity;
using mygame.sdk;
using Newtonsoft.Json;
using time;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_ANDROID
using Unity.Notifications.Android;

#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif
[System.Serializable]
public class CachedNotificationData
{
    public int id;
    public string title;
    public string identifier;
    public string body;
    public string subtitle;
    public long fireTimeMillisecond;
    public bool groupSummary;
}

public class NotificationHandler : MonoBehaviour
{
    public static bool enableNotification
    {
        set => PlayerPrefs.SetInt("is_enable_local_notification", value ? 1 : 0);
        get => PlayerPrefs.GetInt("is_enable_local_notification", 1) == 1;
    }

    public static Action AuthorizationCompleted;
    public static Action<string> NotificationOpened;
    public static Action<string> OnRemoveDeliveredNotification;

    public static bool IsAuthorization;

    private static List<CachedNotificationData> notifications = new();

    private void Start()
    {
        StartCoroutine(RequestAuthorization());
    }

#if UNITY_ANDROID
    private void OnNotificationReceived(AndroidNotificationIntentData data)
    {
        Debug.Log("Notification revive: " + data.Id);
    }
#endif

#if UNITY_IOS
    private void OnNotificationReceived(iOSNotification notification)
    {
        Debug.Log("Notification revive: " + notification.Identifier);
    }
#endif

    public static int SendNotification(string identifier, string title, string subtitle, string body, long timeInterval)
    {
        if (PlayerPrefs.GetInt("cf_local_notification", 1) == 0) return -1;
        var fireTime = DateTime.Now.AddMilliseconds(timeInterval);

        if (!enableNotification)
        {
            AddNotificationCache(-1, identifier, false, title, subtitle, body,
                SdkUtil.toTimestamp(DateTime.UtcNow.AddMilliseconds(timeInterval)));
            return -1;
        }
#if UNITY_ANDROID
        // var notification = new AndroidNotification();
        // notification.Title = title;
        // notification.Text = body;
        // notification.ShowInForeground = false;
        // notification.ShouldAutoCancel = true;
        // notification.Group = Application.productName;
        // notification.SortKey = $"{fireTime.ToUniversalTime().Ticks}_{identifier}";
        // notification.SmallIcon = "small_icon"; // (icon cần nằm trong Plugins/Android/res/drawable)
        // notification.LargeIcon = "large_icon";
        // notification.FireTime = fireTime;
        //
        // notification.GroupSummary = IsGroupSummary();
        // var notificationId = AndroidNotificationCenter.SendNotification(notification, "default_channel");
        // Debug.Log($"Notification send: identifier={identifier},fireTime={fireTime},notificationId={notificationId}" + identifier);
        // AddNotificationCache(notificationId, identifier, notification.GroupSummary, title, subtitle, body, SdkUtil.toTimestamp(DateTime.UtcNow.AddMilliseconds(timeInterval)));
        return 0; //notificationId;
#elif UNITY_IOS
        // Gửi local notification đơn giản
        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(timeInterval * TimeSpan.TicksPerMillisecond), // sau 5 giây
            Repeats = false
        };
        
        var notification = new iOSNotification
        {
            Identifier = identifier,
            Title = title,
            Body = body,
            Subtitle = subtitle,
            ShowInForeground = false,
            ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger
        };
        iOSNotificationCenter.ScheduleNotification(notification);
        AddNotificationCache(0, identifier, false, title, subtitle, body, SdkUtil.toTimestamp(DateTime.UtcNow.AddMilliseconds(timeInterval)));
        Debug.Log($"Notification send: identifier={identifier},fireTime={fireTime}" + identifier);
#endif

        Debug.Log("Notification send");
        return 0;
    }

    private static bool IsGroupSummary()
    {
        var now = GameHelper.CurrentTimeMilisReal();
        var summaryNotification = notifications.FirstOrDefault(x => x.fireTimeMillisecond > now && x.groupSummary);
        return summaryNotification == null;
    }

    private static void ResendGroupSummaryNotification()
    {
#if UNITY_ANDROID
        if (PlayerPrefs.GetInt("cf_local_notification", 1) == 0 || !enableNotification) return;

        if (IsGroupSummary())
        {
            var now = GameHelper.CurrentTimeMilisReal();
            var data = notifications.FirstOrDefault(x => x.fireTimeMillisecond > now);
            if (data != null)
            {
                var sendTime = data.fireTimeMillisecond - now;
                RemoveNotification(data.identifier);
                SendNotification(data.identifier, data.title, data.subtitle, data.body, sendTime);
            }
        }
#endif
    }

    public static void RemoveNotification(string identifier)
    {
        if (PlayerPrefs.GetInt("cf_local_notification", 1) == 0) return;
        var now = GameHelper.CurrentTimeMilisReal();
        var notificationData =
            notifications.FirstOrDefault(x => x.identifier == identifier && x.fireTimeMillisecond > now);
#if UNITY_ANDROID
        // if (notificationData != null)
        // {
        //     AndroidNotificationCenter.CancelNotification(notificationData.id);
        // }
#elif UNITY_IOS
        iOSNotificationCenter.RemoveScheduledNotification(identifier);
#endif
        if (notificationData != null)
        {
            notifications.Remove(notificationData);
            ResendGroupSummaryNotification();
        }
    }

    private static void LoadNotificationCache()
    {
        string json = PlayerPrefs.GetString("notif_cache", "");
        Debug.Log($"Notification cache: {json}");
        notifications = JsonConvert.DeserializeObject<List<CachedNotificationData>>(json) ??
                        new List<CachedNotificationData>();
    }

    private static void RemoveDeliveredNotification()
    {
        var now = GameHelper.CurrentTimeMilisReal();
        for (var i = notifications.Count - 1; i >= 0; i--)
        {
            if (notifications[i].fireTimeMillisecond <= now)
            {
                OnRemoveDeliveredNotification?.Invoke(notifications[i].identifier);
                notifications.RemoveAt(i);
            }
        }
    }

    private static void AddNotificationCache(int id, string identifier, bool groupSummary, string title,
        string subtitle, string body, long fireTime)
    {
        notifications.Add(new CachedNotificationData
        {
            id = id,
            identifier = identifier,
            title = title,
            body = body,
            groupSummary = groupSummary,
            subtitle = subtitle,
            fireTimeMillisecond = fireTime
        });

        PlayerPrefs.SetString("notif_cache", JsonConvert.SerializeObject(notifications));
    }

    private static void ResendCachedNotifications()
    {
        if (PlayerPrefs.GetInt("cf_local_notification", 1) == 0 || !enableNotification) return;
        var array = new List<CachedNotificationData>(notifications);
        notifications.Clear();
        var now = GameHelper.CurrentTimeMilisReal();
        foreach (var data in array)
        {
            if (data.fireTimeMillisecond <= now) continue;
            var sendTime = data.fireTimeMillisecond - now;
            SendNotification(data.identifier, data.title, data.subtitle, data.body, sendTime);
        }

        PlayerPrefs.SetString("notif_cache", JsonConvert.SerializeObject(notifications));
    }

    public static bool IsNotificationEnabled()
    {
#if UNITY_IOS
        var currentSettings = iOSNotificationCenter.GetNotificationSettings();
        switch (currentSettings.AuthorizationStatus)
        {
            case AuthorizationStatus.NotDetermined:
                return false;
            case AuthorizationStatus.Denied:
                return false;
            case AuthorizationStatus.Authorized:
                return true;
            case AuthorizationStatus.Provisional:
                return true;
            case AuthorizationStatus.Ephemeral:
                return false;
            default:
                return false;
        }
#elif UNITY_ANDROID && !UNITY_EDITOR
        // using var version = new AndroidJavaClass("android.os.Build$VERSION");
        // int sdkVersion = version.GetStatic<int>("SDK_INT");
        // using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        // using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        // if (sdkVersion >= 33) // Android 13+
        // {
        //     // Kiểm tra quyền POST_NOTIFICATIONS
        //     using var contextCompat = new AndroidJavaClass("androidx.core.content.ContextCompat");
        //     string permission = "android.permission.POST_NOTIFICATIONS";
        //     int permissionResult = contextCompat.CallStatic<int>("checkSelfPermission", activity, permission);
        //
        //     if (permissionResult == 0) // PackageManager.PERMISSION_GRANTED
        //     {
        //         Debug.Log("Notification enabled");
        //         return true;
        //     }
        //     else
        //     {
        //         Debug.Log("Notification disabled");
        //         return false;
        //     }
        // }
        // else
        // {
        //     using var notificationManagerCompat = new AndroidJavaClass("androidx.core.app.NotificationManagerCompat");
        //     var manager = notificationManagerCompat.CallStatic<AndroidJavaObject>("from", activity);
        //     bool areNotificationsEnabled = manager.Call<bool>("areNotificationsEnabled");
        //
        //     if (areNotificationsEnabled) // PackageManager.PERMISSION_GRANTED
        //     {
        //         Debug.Log("Notification enabled");
        //         return true;
        //     }
        //     else
        //     {
        //         Debug.Log("Notification disabled");
        //         return false;
        //     }
        // }
#endif
        return false;
    }

    private IEnumerator RequestAuthorization()
    {
        LoadNotificationCache();
#if UNITY_IOS
        if (!IsNotificationEnabled())
        {
            var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
            using (var req = new AuthorizationRequest(authorizationOption, true))
            {
                while (!req.IsFinished)
                {
                    yield return null;
                }

                string res = "\n RequestAuthorization:";
                res += "\n finished: " + req.IsFinished;
                res += "\n granted :  " + req.Granted;
                res += "\n error:  " + req.Error;
                res += "\n deviceToken:  " + req.DeviceToken;
                Debug.Log(res);
            }
        }

#else
#if !UNITY_EDITOR
        // using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
        // {
        //     int sdkVersion = version.GetStatic<int>("SDK_INT");
        //     if (sdkVersion >= 33) // Android 13+
        //     {
        //         if (!IsNotificationEnabled())
        //         {
        //             var request = new PermissionRequest();
        //             while (request.Status == PermissionStatus.RequestPending)
        //                 yield return null;
        //             Debug.Log("Notification: permission=" + request.Status);
        //         }
        //     }
        // }

#endif

#endif

        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(.5f);
#if UNITY_ANDROID
        // var channel = new AndroidNotificationChannel()
        // {
        //     Id = "default_channel",
        //     Name = "Default Channel",
        //     Importance = Importance.Default,
        //     Description = "Notification InGame",
        // };
        // AndroidNotificationCenter.RegisterNotificationChannel(channel);
        //
        // AndroidNotificationCenter.OnNotificationReceived += OnNotificationReceived;
        //
        // var notification = AndroidNotificationCenter.GetLastNotificationIntent();
        // if (notification != null)
        // {
        //     var now = GameHelper.CurrentTimeMilisReal();
        //     var notificationData = notifications.FirstOrDefault(x => x.id == notification.Id && x.fireTimeMillisecond <= now);
        //     if (notificationData != null)
        //     {
        //         NotificationOpened?.Invoke(notificationData.identifier);
        //         Debug.Log("User clicked notification with ID: " + notificationData.identifier);
        //     }
        //     else
        //     {
        //         Debug.Log("User clicked notification with ID: -1");
        //     }
        // }

#elif UNITY_IOS
        iOSNotificationCenter.OnNotificationReceived += OnNotificationReceived;

        var notification = iOSNotificationCenter.GetLastRespondedNotification();
        if (notification != null)
        {
            Debug.Log("User clicked notification with ID: " + notification.Identifier);
            NotificationOpened?.Invoke(notification.Identifier);
        }
#endif
        RemoveDeliveredNotification();
        ResendGroupSummaryNotification();

        IsAuthorization = true;
        AuthorizationCompleted?.Invoke();
    }

    public static void SetActiveNotifications(bool active)
    {
        enableNotification = active;
        if (!active)
        {
            RemoveAllNotification();
        }
        else
        {
            ResendCachedNotifications();
        }
    }

    public static void ClearDisplayNotifications()
    {
#if UNITY_ANDROID
        // AndroidNotificationCenter.CancelAllDisplayedNotifications(); // Android
#elif UNITY_IOS
        iOSNotificationCenter.ApplicationBadge = 0;
        iOSNotificationCenter.RemoveAllDeliveredNotifications();     // iOS
#endif
    }

    public static void RemoveAllNotification(bool isRemoveCache = false)
    {
#if UNITY_ANDROID
        // AndroidNotificationCenter.CancelAllScheduledNotifications();
        // AndroidNotificationCenter.CancelAllDisplayedNotifications();
#elif UNITY_IOS
        iOSNotificationCenter.RemoveAllScheduledNotifications();  
        iOSNotificationCenter.RemoveAllDeliveredNotifications();  // Hủy thông báo đã hiển thị
#endif
        if (isRemoveCache)
        {
            notifications.Clear();
            PlayerPrefs.SetString("notif_cache", JsonConvert.SerializeObject(notifications));
        }
    }
}