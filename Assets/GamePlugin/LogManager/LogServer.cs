//#define ENABLE_LOGDATA_MYSERVER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

#if ENABLE_LOGDATA_MYSERVER
namespace mygame.sdk
{
    public class LogServer : ILogEvent
    {
        private const string url = "";

        public static bool CF_EnableLogServer
        {
            get => LogPrefs.GetBool("cf_enable_log_server", true);
            set => LogPrefs.SetBool("cf_enable_log_server", value);
        }
        public List<ObjectLog> listObjectLog = new();
        private string logCachePref
        {
            get => LogPrefs.GetString("logs_cache_server");
            set => LogPrefs.SetString("logs_cache_server", value);
        }

        private void SendEvent(string data, Action<bool> result)
        {
            Debug.Log($"server: {data}");
            LogEventManager.Instance.StartCoroutine(PostRequest(data, result));
        }

        IEnumerator PostRequest(string data, Action<bool> result)
        {
            using var www = UnityWebRequest.Post(url, data, "application/json");
            www.SetRequestHeader("Content-Type", "application/json");
            //www.SetRequestHeader("X-API-KEY", XAPIKEY);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                //Debug.LogError($"Server Request failed: {www.error}");
                result?.Invoke(false);
            }
            else
            {
                //Debug.Log($"Server Success: {data}");
                result?.Invoke(true);
            }
        }
        private void SaveLogObjsServer()
        {
            logCachePref = JsonConvert.SerializeObject(listObjectLog,LogEventManager.jsonSetting);
        }
        public void AddLogCache(ObjectLog log)
        {
            if(!CF_EnableLogServer) return;
            listObjectLog.Add(log);
            SaveLogObjsServer();
        }
        void AddLogCache(List<ObjectLog> log)
        {
            listObjectLog.AddRange(log);
            SaveLogObjsServer();
        }
        public void LogData()
        {
            if(!CF_EnableLogServer || listObjectLog.Count == 0) return;
            var data = "";
            var list = new List<ObjectLog>(listObjectLog);
            foreach (var t in list.Where(t => !t.sendedServer))
            {
                if (data.Length == 0)
                {
                    data += LogEventManager. ToDataBucket(t);
                }
                else
                {
                    data += "\n" + LogEventManager. ToDataBucket(t);
                }
                t.sendedServer = true;
            }

            listObjectLog.Clear();
            SaveLogObjsServer();
            SendEvent(data, result =>
            {
                if (result)
                {
                    return;
                }

                foreach (var t in list)
                {
                    t.sendedServer = false;
                }

                AddLogCache(list);
            });
        }
        public bool EnableLog => CF_EnableLogServer;

        public void Initialized()
        {
            if (logCachePref.Length > 0)
            {
                listObjectLog = JsonConvert.DeserializeObject<List<ObjectLog>>(logCachePref);
            }
            listObjectLog ??= new List<ObjectLog>();
        }
    }
}
#endif
