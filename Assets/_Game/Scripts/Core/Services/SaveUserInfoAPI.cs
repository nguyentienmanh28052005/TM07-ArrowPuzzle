using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using Myapi;
using mygame.sdk;
using UnityEngine;

public class SaveUserInfoAPI : SingleTonApi<SaveUserInfoAPI, UserInfo>
{
    private float lastSaveTime = -200;
    
    public static int DelaySaveUserInfo
    {
        get => PlayerPrefs.GetInt("cf_delay_save_user_info", 200);
        set => PlayerPrefs.SetInt("cf_delay_save_user_info", value);
    }
    
    public void SaveData(Action<UserInfo> cb)
    {
        if (Time.time - lastSaveTime > DelaySaveUserInfo)
        {
            long idrq = GameHelper.CurrentTimeMilisReal();
            idrq = addQueueCallback(idrq, cb);
            string re = $"?";
            re += $"userid={HttpUtility.UrlEncode(UserDataManager.Instance.GetDataUser().player_uuid)}";
            re += $"&name={HttpUtility.UrlEncode(DataManager.Instance.userName)}";
            //re += $"&avatar={DataManager.Instance.avtarID}";
            re += $"&platform={AppConfig.platformName.ToUpper()}";
            re += $"&country={GameHelper.Instance.countryCode}";
            re += $"&level={GameRes.GetLevel()}";
            PostRequest(idrq, $"{AppConfig.urlLogEvent}/api/savedata{re}", 15);
        }
    }

    protected override void OnError(long idRequest, string error)
    {
        base.OnError(idRequest, error);
        if (callBacks.ContainsKey(idRequest) && callBacks[idRequest] != null)
        {
            callBacks[idRequest](null);
            callBacks.Remove(idRequest);
        }
    }

    protected override void Process(long idRequest, UserInfo data)
    {
        if (callBacks.ContainsKey(idRequest) && callBacks[idRequest] != null)
        {
            callBacks[idRequest](data);
            callBacks.Remove(idRequest);
        }
    }
}

public class UserInfo
{
    public int status;
}