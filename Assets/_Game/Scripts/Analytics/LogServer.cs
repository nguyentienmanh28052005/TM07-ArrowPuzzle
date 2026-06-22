using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LogServer : MonoBehaviour
{
    private readonly string url = "https://logapi.backendxgame.xyz/api/log/com.screw.puzzle.story";
    public void SendEvent(string data, Action<bool> result)
    {
        StartCoroutine(PostRequest(data, result));
    }

    IEnumerator PostRequest(string data, Action<bool> result)
    {
        using UnityWebRequest www = UnityWebRequest.Post(url, data, "application/json");
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
}
