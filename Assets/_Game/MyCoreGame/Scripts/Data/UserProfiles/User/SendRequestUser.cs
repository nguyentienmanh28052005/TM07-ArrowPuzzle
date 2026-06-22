using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using master;

public class SendRequestUser : ISendRequestServer
{
    private SocketHub socketHub => ServerHub.Instance.Socket;

    public void UpdateUserInfo(int avatarId = -2, int frameId = -1, int level = -1, string name = null, long createTime = -1, string socialId = null, int areasCompleted = -1,
         int firstTryWins = -1, string avatarUrl = null, int nameId = -1)
    {
        var data = new SendDataServer
        {
            user = new SendDataServer.UserSendData
            {
                parameters = new SendDataServer.UserSendData.Parameters(),
                action = "user_update_info"
            }
        };
        data.user.parameters.avatarId = avatarId;
        data.user.parameters.level = level;
        data.user.parameters.name = name;
        data.user.parameters.createdDate = createTime;
        data.user.parameters.socialId = socialId;
        data.user.parameters.areasCompleted = areasCompleted;
        data.user.parameters.firstTryWins = firstTryWins;
        data.user.parameters.frameId = frameId;
        data.user.parameters.avatarUrl = avatarUrl;
        data.user.parameters.nameId = nameId;
        
        var message = JsonConvert.SerializeObject(data, SaveLoadUtil.JsonSetting);
        socketHub.Send(message);
        RequestTracker.TrackRequest("user_update_info");
    }
    public void RequestGetUserInfor(string playerUUID)
    {
        var data = new SendDataServer();
        data.user = new SendDataServer.UserSendData();
        data.user.parameters = new SendDataServer.UserSendData.Parameters();
        data.user.action = "get_user_info";
        data.user.parameters.playerUUID = playerUUID;
        var message = JsonConvert.SerializeObject(data, SaveLoadUtil.JsonSetting);
        socketHub.Send(message);
        RequestTracker.TrackRequest("get_user_info");
    }
    public void RequestGetResource(int id)
    {
        var data = new SendDataServer();
        data.user = new SendDataServer.UserSendData();
        data.user.parameters = new SendDataServer.UserSendData.Parameters();
        data.user.action = "get_resource";
        data.user.parameters.resourceId = id;
        var message = JsonConvert.SerializeObject(data, SaveLoadUtil.JsonSetting);
        socketHub.Send(message);
        RequestTracker.TrackRequest("get_resource");
    }
    public void RequestUseResource(int id, int amount, string senderId)
    {
        var data = new SendDataServer();
        data.user = new SendDataServer.UserSendData();
        data.user.parameters = new SendDataServer.UserSendData.Parameters();
        data.user.action = "use_resource";
        data.user.parameters.resourceId = id;
        data.user.parameters.amount = amount;
        data.user.parameters.senderId = senderId;
        var message = JsonConvert.SerializeObject(data, SaveLoadUtil.JsonSetting);
        socketHub.Send(message);
        RequestTracker.TrackRequest("use_resource");
    }
    public void RequestGetTimeRemainRequestHeart()
    {
        var data = new SendDataServer();
        data.user = new SendDataServer.UserSendData();
        data.user.action = "get_request_heart_remain_time";
        var message = JsonConvert.SerializeObject(data, SaveLoadUtil.JsonSetting);
        socketHub.Send(message);
        RequestTracker.TrackRequest("get_request_heart_remain_time");
    }

    public void RequestSendGCM(string gcm)
    {
        var data = new SendDataServer();
        data.user = new SendDataServer.UserSendData();
        data.user.action = "update_user_fcm";
        data.user.parameters = new SendDataServer.UserSendData.Parameters();
        data.user.parameters.token = gcm;
        var message = JsonConvert.SerializeObject(data, SaveLoadUtil.JsonSetting);
        socketHub.Send(message);
        RequestTracker.TrackRequest("update_user_fcm");
    }
    
    public void RequestLinkSocialAccount(string facebookId = null, string googleId = null, string appleId = null,string platformToken = null)
    {
        var data = new SendDataServer();
        data.user = new SendDataServer.UserSendData();
        data.user.parameters = new SendDataServer.UserSendData.Parameters();
        data.user.action = "link_social_account";
        data.user.parameters.facebookId = facebookId;
        data.user.parameters.googleId = googleId;
        data.user.parameters.appleId = appleId;
        data.user.parameters.platformToken = platformToken;
        var message = JsonConvert.SerializeObject(data, SaveLoadUtil.JsonSetting);
        socketHub.Send(message);
        RequestTracker.TrackRequest("link_social_account");
    }
    public void RequestSaveUserData(UserData dataUser)
    {
        var data = new SendDataServer();
        data.user = new SendDataServer.UserSendData();
        data.user.parameters = new SendDataServer.UserSendData.Parameters();
        data.user.action = "save_user_data";
        data.user.parameters.data = dataUser;
        var message = JsonConvert.SerializeObject(data, SaveLoadUtil.JsonSetting);
        socketHub.Send(message);
        RequestTracker.TrackRequest("save_user_data");
    }

    public void RequestGetCompareData()
    {
        var data = new SendDataServer();
        data.user = new SendDataServer.UserSendData();
        data.user.action = "get_compare_data";
        var message = JsonConvert.SerializeObject(data, SaveLoadUtil.JsonSetting);
        socketHub.Send(message);
        RequestTracker.TrackRequest("get_compare_data");
    }
    public void RequestSynchData(int option)
    {
        var data = new SendDataServer();
        data.user = new SendDataServer.UserSendData();
        data.user.action = "synch_user_data";
        data.user.parameters = new SendDataServer.UserSendData.Parameters();
        data.user.parameters.type = option;
        var message = JsonConvert.SerializeObject(data, SaveLoadUtil.JsonSetting);
        socketHub.Send(message);
        RequestTracker.TrackRequest("synch_user_data");
    }
    public void RequestLogOutSocialAccount(int option)
    {
        var data = new SendDataServer();
        data.user = new SendDataServer.UserSendData();
        data.user.action = "log_out_social_account";
        data.user.parameters = new SendDataServer.UserSendData.Parameters();
        data.user.parameters.type = option;
        var message = JsonConvert.SerializeObject(data, SaveLoadUtil.JsonSetting);
        socketHub.Send(message);
        RequestTracker.TrackRequest("log_out_social_account");
    }
    public void RequestGetRandomUserWithAvatarUrl(int num)
    {
        var data = new SendDataServer();
        data.user = new SendDataServer.UserSendData();
        data.user.action = "get_random_user_with_avatar_url";
        data.user.parameters = new SendDataServer.UserSendData.Parameters();
        data.user.parameters.count = num;
        var message = JsonConvert.SerializeObject(data, SaveLoadUtil.JsonSetting);
        socketHub.Send(message);
        RequestTracker.TrackRequest("get_random_user_with_avatar_url");

    }
}