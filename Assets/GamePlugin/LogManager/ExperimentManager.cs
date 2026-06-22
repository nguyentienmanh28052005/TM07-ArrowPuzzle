using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using mygame.sdk;
public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ExperimentManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("ExperimentManager");
                    _instance = obj.AddComponent<ExperimentManager>();
                }
            }
            return _instance;
        }
    }
    private static ExperimentManager _instance;
    private const string BaseUrl = "https://hub.databucket.io/eval";
    private void Awake()
    {
        _instance = this;
    }
    public void FetchUserSegments(string ruleName, Action<bool, string> callBack, Dictionary<string, string> parameters = null)
    {
#if ENABLE_LOGDATA_BUCKET
        StartCoroutine(GetSegmentsCoroutine(ruleName, parameters, callBack));
#else
        callBack?.Invoke(false, "Not enable log data bucket");
#endif
    }
#if ENABLE_LOGDATA_BUCKET
    private IEnumerator GetSegmentsCoroutine(string ruleName, Dictionary<string, string> parameters, Action<bool, string> callBack)
    {
        string userID = LogEventCustom.User_ID;
        string url = $"{BaseUrl}?rule={ruleName}&uid={userID}";
        if (parameters != null && parameters.Count > 0)
        {
            foreach (var param in parameters)
            {
                url += $"&{param.Key}={param.Value}";
            }
        }
        Debug.Log($"[ExperimentManager] url: {url}");
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("X-API-KEY", mygame.sdk.LogDataBucket.XAPIKEY);
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log($"[ExperimentManager] Success {jsonResponse}");
                callBack?.Invoke(true, jsonResponse);
            }
            else
            {
                callBack?.Invoke(false, webRequest.error);
                Debug.Log($"[ExperimentManager] API Error: {webRequest.error}");
            }
        }
    }
#endif
#if UNITY_EDITOR
    [UnityEditor.MenuItem("TEST/DATABUCKETS/SEGMENT")]
    public static void TestSendSegment()
    {
        ExperimentManager.Instance.FetchUserSegments("user_android", (suc, json) =>
        {
            if (suc)
            {
                //Debug.Log($"FetchUserSegments {suc}_{json}");
                //var dictmp = (IDictionary<string, object>)MyJson.JsonDecoder.DecodeText(json);
                //LevelRemoteManager.CFLevelConfig = dictmp["message"].ToString();
                //LevelRemoteManager.Instance.SetConfig(true);
            }
        });
    }
#endif
}
