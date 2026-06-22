using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class NotificationData
{
    public string identifier;
    public string title;
    public string subtitle;
    public string body;

    [JsonIgnore]
    public bool isSendNotification
    {
        get => PlayerPrefs.GetInt($"send_notification_{identifier}", 1) == 1;
        set => PlayerPrefs.SetInt($"send_notification_{identifier}", value ? 1 : 0);
    }

    public void ClearCache()
    {
        isSendNotification = true;
    }
}