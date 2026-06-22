//#define ENABLE_LOG_DATABUCKET_CONSOLE
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LogDataBucket : MonoBehaviour
{
    //Real
    //y8tkZ7y8XHy7X5RU91Ptoyuvf2loDeNMxDJU69rtPB4UpI8LY682QMF2cT4LsepdBICNWOQ3XWEdIiXFgx9JNR1yOGQd672TuZvM11bsL3rIA6Gzn0C5pc6s1AW0445z
    private readonly string url = "https://ingest.databuckets.com/push";
    public static string XAPIKEY
    {
        get => PlayerPrefs.GetString("dlxdi_xapi", "qz5R31np6jdSshT0fF421bHjohO5Lp4yexp499mrDysVfaGbPNxqOULgqSgmy7kwxKp7l7SrxiZIpUkGYfRyVYhbG5fwgPk9tpw2VTz08kbCxnuP3bb47cqm566Cjshi");
        set => PlayerPrefs.SetString("dlxdi_xapi", value);
    }
    public void SendEvent(string data, Action<bool> result)
    {
        StartCoroutine(PostRequest(data, result));
    }

    IEnumerator PostRequest(string data, Action<bool> result)
    {
        using UnityWebRequest www = UnityWebRequest.Post(url, data, "application/json");
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
}
