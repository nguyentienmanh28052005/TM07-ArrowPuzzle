using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EventNotificationType
{
    StartEvent,
    EndEvent,
    Custom,
}


public class DataEventNotification : ScriptableObject
{
    [Serializable]
    public class Data
    {
        public EEventConfig eventConfig;
        public EventNotificationType notificationType;
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
    
    public Data[] data;

    public Data GetNotificationData(EEventConfig eventConfig, EventNotificationType notificationType)
    {
        return data.SingleOrDefault(x => x.eventConfig == eventConfig && x.notificationType == notificationType);
    }
    
    public Data GetNotificationData(string identifier)
    {
        return data.SingleOrDefault(x => x.identifier == identifier);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(DataEventNotification))]
public class DataNotificationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var dataNotification = (DataEventNotification)target;
        
        
        if (GUILayout.Button("To Json"))
        {
#if UNITY_ANDROID
            File.WriteAllText("Assets/Games/Resources/NotificationData/Android/data.txt", JsonConvert.SerializeObject(dataNotification.data));        
#elif UNITY_IOS
            File.WriteAllText("Assets/Games/Resources/NotificationData/iOS/data.txt", JsonConvert.SerializeObject(dataNotification.data));        
#endif            
            AssetDatabase.Refresh();
        }

        base.OnInspectorGUI();
    }
}
#endif