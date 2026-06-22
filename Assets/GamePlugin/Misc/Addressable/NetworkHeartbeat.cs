using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkHeartbeat : master.Singleton<NetworkHeartbeat>
{
    public static string CF_LinkCheckWebOnline
    {
        get => PlayerPrefs.GetString("cf_link_check_web_gg_online", ProbeUrl);
        set => PlayerPrefs.SetString("cf_link_check_web_gg_online", value);
    }

    public bool IsInternetOk { get; private set; }
    public long LastOkUnixMs { get; private set; }
    public string LastError { get; private set; } = "";

    [SerializeField] private float intervalSec = 5f;
    [SerializeField] private int timeoutSec = 3;

    private const string ProbeUrl = "https://www.google.com/generate_204";

    private void OnEnable()
    {
        StartCoroutine(CoHeartbeat());
    }

    private IEnumerator CoHeartbeat()
    {
        while (true)
        {
            yield return CheckNow();
            yield return new WaitForSecondsRealtime(intervalSec);
        }
    }

    public IEnumerator CheckNow()
    {
        string linkWebCheck = CF_LinkCheckWebOnline;
        if (string.IsNullOrEmpty(linkWebCheck))
        {
            linkWebCheck = ProbeUrl;
        }
        using var req = UnityWebRequest.Head(linkWebCheck);
        req.timeout = timeoutSec;
        yield return req.SendWebRequest();

        bool ok = req.result == UnityWebRequest.Result.Success &&
                  req.responseCode >= 200 && req.responseCode < 400;

        IsInternetOk = ok;
        if (ok)
        {
            LastOkUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            LastError = "";
        }
        else
        {
            LastError = req.error;
        }
    }
}