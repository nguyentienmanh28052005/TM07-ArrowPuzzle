//#define ENABLE_LOGDATA_BUCKET
//#define ENABLE_LOG_DATABUCKET_CONSOLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

#if ENABLE_LOGDATA_BUCKET
namespace mygame.sdk
{
    public class LogDataBucket : ILogEvent
    {
        public const string XAPIKEY = "DhnCxgOIU6qZ8TvVOL6efqv9elQNJnLBEptuCjuQXzQZHUIIYGZgXHSFHTtqfFtIPaewoFl5BbVYZDykjAxNOZRyvv5BVHQPIeuL6r0Gbzm2SIZ1PbuTZZeLj8UKSInD";

        public static bool CF_EnableLogDataBucket
        {
            get => LogPrefs.GetBool("cf_enable_log_data_bucket", true);
            set => LogPrefs.SetBool("cf_enable_log_data_bucket", value);
        }

        public List<ObjectLog> listObjectLog = new();
        private readonly string url = "https://ingest.databuckets.com/push";
        private string logCachePref
        {
            get => LogPrefs.GetString("logs_cache");
            set => LogPrefs.SetString("logs_cache", value);
        }

        public void SendEvent(string data, Action<bool> result)
        {
#if ENABLE_LOG_DATABUCKET_CONSOLE
            Debug.Log($"databucket: {data}");
#endif
            LogEventManager.Instance.StartCoroutine(PostRequest(data, result));
        }

        IEnumerator PostRequest(string data, Action<bool> result)
        {
            using var www = UnityWebRequest.Post(url, data, "application/json");
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("X-API-KEY", XAPIKEY);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
#if ENABLE_LOG_DATABUCKET_CONSOLE
                Debug.LogError($"DataBuckets Request failed: {www.error}");
#endif
                result?.Invoke(false);
            }
            else
            {
#if ENABLE_LOG_DATABUCKET_CONSOLE
                Debug.Log($"DataBuckets Success: {data}");
#endif
                result?.Invoke(true);
            }
        }

        public bool EnableLog => CF_EnableLogDataBucket;

        public void Initialized()
        {
            if (logCachePref.Length > 0)
            {
                listObjectLog = JsonConvert.DeserializeObject<List<ObjectLog>>(logCachePref);
            }

            listObjectLog ??= new List<ObjectLog>();
        }

        public void LogData()
        {
            if (!EnableLog || listObjectLog.Count == 0) return;
            var data = "";
            var list = new List<ObjectLog>(listObjectLog);
            foreach (var t in list.Where(t => !t.sended))
            {
                if (data.Length == 0)
                {
                    data += LogEventManager.ToDataBucket(t);
                }
                else
                {
                    data += "\n" + LogEventManager.ToDataBucket(t);
                }

                t.sended = true;
            }

            listObjectLog.Clear();
            SaveLogObjs();
            SendEvent(data, result =>
            {
                if (result)
                {
                    return;
                }

                foreach (var t in list)
                {
                    t.sended = false;
                }
                AddLogCache(list);
            });
        }

        public void AddLogCache(ObjectLog log)
        {
            if (!CF_EnableLogDataBucket) return;
            listObjectLog.Add(log);
            SaveLogObjs();
        }
        private void AddLogCache(List<ObjectLog> log)
        {
            listObjectLog.AddRange(log);
            SaveLogObjs();
        }
        private void SaveLogObjs()
        {
            logCachePref = JsonConvert.SerializeObject(listObjectLog, LogEventManager.jsonSetting);
        }
    }
}
#endif
