//# define ENABLE_LOG_SOCKET
# define ENABLE_SOCKET
using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityWebSocket;
using System.ComponentModel;
using UniRx;

public class SocketHub : MonoBehaviour
{
    public static Action OnSocketError;
    public static Action OnSocketClose;
    public static Action OnSocketOpen;
    public static WebSocketState WebSocketState => socket.ReadyState;
    public static ReactiveProperty<bool> IsLoggedGame { get; set; } = new ReactiveProperty<bool>(false);
    private static IWebSocket socket;
#if UNITY_EDITOR
    [SerializeField] string address = $"";
#else
#if ENABLE_HACK
    private string address = $"";
#else
    private string address = $"";
#endif
#endif


    public static JsonSerializerSettings jsonSetting = new JsonSerializerSettings
        { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore };

    private IDisposable subIsLoggedGame;
    private FactoryProcessDataServer factory;
#if ENABLE_SOCKET
    private void Awake()
    {
        factory = new FactoryProcessDataServer();
        socket = new WebSocket(address);
        socket.OnOpen += Socket_OnOpen;
        socket.OnMessage += Socket_OnMessage;
        socket.OnClose += Socket_OnClose;
        socket.OnError += Socket_OnError;
        subIsLoggedGame = IsLoggedGame.Subscribe(x =>
        {
            if (x)
            {
                requestOnReconnect();
            }
        });
    }

    private void OnDestroy()
    {
        socket.OnOpen -= Socket_OnOpen;
        socket.OnMessage -= Socket_OnMessage;
        socket.OnClose -= Socket_OnClose;
        socket.OnError -= Socket_OnError;
        subIsLoggedGame.Dispose();
    }
#endif
    private string cacheRequest;

    private void requestOnReconnect()
    {
        if (!string.IsNullOrEmpty(cacheRequest))
        {
            Send(cacheRequest);
            cacheRequest = "";
        }
    }

    public void Connect()
    {
#if ENABLE_SOCKET
        if (socket.ReadyState != WebSocketState.Closed) return;
#if ENABLE_LOG_SOCKET
        Debug.Log($"<color=#FFE309>SOCKET_SERVER: CONNECT</color>");
#endif
        socket.ConnectAsync();
#endif
    }

    public void Disconnect()
    {
#if ENABLE_SOCKET
        if (socket.ReadyState != WebSocketState.Open) return;
#if ENABLE_LOG_SOCKET
        Debug.Log($"<color=#FF3308>SOCKET_SERVER: DISCONNECT</color>");
#endif
        cacheRequest = "";
        socket.CloseAsync();
#endif
    }

    public void Send(string message, bool reLogin = true)
    {
#if ENABLE_SOCKET
        if (socket.ReadyState != WebSocketState.Open)
        {
            if (string.IsNullOrEmpty(cacheRequest))
            {
                cacheRequest = message;
            }

            if (reLogin)
            {
                Connect();
            }

            return;
#endif
        }
#if ENABLE_LOG_SOCKET
        Debug.Log($"<color=#FFE309>SOCKET_SERVER: Send ={message}</color>");
#endif
        if (!IsLoggedGame.Value && reLogin)
        {
            if (string.IsNullOrEmpty(cacheRequest))
            {
                cacheRequest = message;
            }

            Login();
            return;
        }

        Debug.Log($"<color=#FFE309>SOCKET_SERVER: SendAsync ={message}</color>");

        socket.SendAsync(message);
    }

    private void Socket_OnOpen(object sender, OpenEventArgs e)
    {
#if ENABLE_SOCKET
        Login();
        IsLoggedGame.Value = true;
        OnSocketOpen?.Invoke();
#endif
    }

    private void Socket_OnMessage(object sender, MessageEventArgs e)
    {
#if ENABLE_SOCKET
        if (e.IsBinary)
        {
            var x = string.Format("Receive Bytes ({1}): {0}", e.Data, e.RawData.Length);
            Debug.Log($"<color=#34FF00>SOCKET_SERVER: Received ={x}</color>");
        }
        else if (e.IsText)
        {
            if (e.Data != null)
            {
                try
                {
#if ENABLE_LOG_SOCKET
                    Debug.Log($"<color=#34FF00>SOCKET_SERVER: Received ={e.Data}</color>");
#endif
                    var data = JsonConvert.DeserializeObject<DataServerReceived>(e.Data);
                    if (data.auth != null)
                    {
                        if (factory.DictionaryProcess.TryGetValue(data.auth.action, out var process))
                        {
                            process.Process(data);
                        }
                    }
                    else if (data.guild != null)
                    {
                        if (factory.DictionaryProcess.TryGetValue(data.guild.action, out var process))
                        {
                            process.Process(data);
                        }
                    }
                    else if (data.chat != null)
                    {
                        if (factory.DictionaryProcess.TryGetValue(data.chat.action, out var process))
                        {
                            process.Process(data);
                        }
                    }
                    else if (data.user != null)
                    {
                        if (factory.DictionaryProcess.TryGetValue(data.user.action, out var process))
                        {
                            process.Process(data);
                        }
                    }
                    else if (data.battle != null)
                    {
                        if (factory.DictionaryProcess.TryGetValue(data.battle.action, out var process))
                        {
                            process.Process(data);
                        }
                    }
                    else if (data.adventure != null)
                    {
                        if (factory.DictionaryProcess.TryGetValue(data.adventure.action, out var process))
                        {
                            process.Process(data);
                        }
                    }
                    else if (data.leaderboards != null)
                    {
                        if (factory.DictionaryProcess.TryGetValue(data.leaderboards.action, out var process))
                        {
                            process.Process(data);
                        }
                    }
                    else if (data.userfriends != null)
                    {
                        if (factory.DictionaryProcess.TryGetValue(data.userfriends.action, out var process))
                        {
                            process.Process(data);
                        }
                    }
                    else if (data.weeklyContest != null)
                    {
                        if (factory.DictionaryProcess.TryGetValue(data.weeklyContest.action, out var process))
                        {
                            process.Process(data);
                        }
                    }
                    else if (data.guildgoal != null)
                    {
                        if (factory.DictionaryProcess.TryGetValue(data.guildgoal.action, out var process))
                        {
                            process.Process(data);
                        }
                    }
                    else if (data.survey != null)
                    {
                        if (factory.DictionaryProcess.TryGetValue(data.survey.action, out var process))
                        {
                            process.Process(data);
                        }
                    }
                    else if (data.mail != null)
                    {
                        if (factory.DictionaryProcess.TryGetValue(data.mail.action, out var process))
                        {
                            process.Process(data);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
        else
        {
            Debug.Log($"<color=#34FF00>SOCKET_SERVER: Received ={e.RawData}</color>");
        }
#endif
    }

    private void Socket_OnError(object sender, ErrorEventArgs e)
    {
#if ENABLE_SOCKET
#if ENABLE_LOG_SOCKET
        Debug.Log($"<color=#34FF00>SOCKET_SERVER_ERR Received ={e.Message}, {sender}</color>");
#endif
        OnSocketError?.Invoke();
        RequestTracker.OnSocketServerError();
#endif
    }

    private void Socket_OnClose(object sender, CloseEventArgs e)
    {
#if ENABLE_SOCKET
        IsLoggedGame.Value = false;
        OnSocketClose?.Invoke();
#endif
    }

    private void OnApplicationQuit()
    {
#if ENABLE_SOCKET
        if (socket != null && socket.ReadyState != WebSocketState.Closed)
        {
            socket.CloseAsync();
        }
#endif
    }

    public void Login()
    {
#if ENABLE_SOCKET
        var loginData = new LoginData();
        var playerUuid = UserDataManager.Instance.UserData.playerUUID;
        if (!string.IsNullOrEmpty(playerUuid) && playerUuid.Length > 1)
        {
            loginData.auth.playerUUID = playerUuid; /* "39b57599-f4ad-4b8a-88cc-98d87f63a0da";*/
        }
        else
        {
            //first login
            loginData.auth.name = UserDataManager.Instance.UserData.name;
        }

        loginData.auth.appVersion = Application.version;

        var message = JsonConvert.SerializeObject(loginData, jsonSetting);
        socket.SendAsync(message);
        RequestTracker.TrackRequest("authenication");
#endif
    }
}

[Serializable]
public class LoginData
{
    public AuthenticationData auth;

    public LoginData()
    {
        auth = new AuthenticationData();
    }
}

[Serializable]
public struct AuthenticationData
{
    public string deviceId;
    public string playerUUID;
    public string userId;
    public string name;
    public string appVersion;
    public string countryName;
    public string countryCode;
}

public class DataServerReceived
{
    public class RecieveData
    {
        public string action;
        public object data;
    }

    public RecieveData guild;
    public RecieveData auth;
    public RecieveData chat;
    public RecieveData user;
    public RecieveData battle;
    public RecieveData adventure;
    public RecieveData leaderboards;
    public RecieveData userfriends;
    public RecieveData weeklyContest;
    public RecieveData guildgoal;
    public RecieveData survey;
    public RecieveData mail;
}

[Serializable]
public class AuthRecieveData
{
    public string action;
    public object data;
}

public class SendDataServer
{
    public class GuildSendData
    {
        public class Parameters
        {
            public string guildId;
            public string keySearch;
            public int level;
            public string name;
            public long createTime;
            public string socialId;
            public int areasCompleted;
            [DefaultValue(-1)] public int avatarId = -1;
            public string guildName;
            public string description;
            [DefaultValue(-1)] public int logoId = -1;
            [DefaultValue(-1)] public int requiredLevel = -1;
            [DefaultValue(-1)] public int verificationType = -1;
            public string playerUUID;
        }

        public string action;
        public Parameters parameters;
    }

    public class MessengerSendData
    {
        public class Parameters
        {
            public int type;
            public string content;
            public string messageId;
        }

        public string action;
        public Parameters parameters;
    }

    public class UserSendData
    {
        public class Parameters
        {
            [DefaultValue(-1)] public int level = -1;
            public string name;
            public string playerUUID;
            [DefaultValue(-1)] public long createdDate = -1;
            public string socialId;
            [DefaultValue(-1)] public int areasCompleted = -1;
            [DefaultValue(-2)] public int avatarId = -2;
            [DefaultValue(-1)] public int frameId = -1;
            [DefaultValue(-1)] public int nameId = -1;
            public string avatarUrl;
            [DefaultValue(-1)] public int firstTryWins = -1;
            [DefaultValue(-100)] public int resourceId = -100;
            public int amount;
            public string senderId;
            public string facebookId;
            public string googleId;
            public string appleId;
            public string platformToken;
            public UserData data;
            [DefaultValue(-1)] public int type = -1;
            public string token;
            public int count;
        }

        public string action;
        public Parameters parameters;
    }

    public class BattleSendData
    {
        public class Parameters
        {
            [DefaultValue(-1)] public int battleId = -1;
            [DefaultValue(-1)] public int points = -1;
            [DefaultValue(-1)] public int type = -1;
            [DefaultValue(-1)] public int periodId = -1;
            [DefaultValue(-1)] public int milestoneId = -1;
        }

        public string action;
        public Parameters parameters;
    }

    public class LeaderboardData
    {
        public class Parameters
        {
            [DefaultValue(-1)] public int page;
        }

        public string action;
        public Parameters parameters;
    }

    public class UserFriendsData
    {
        public class Parameters
        {
            [DefaultValue(-1)] public int page = -1;
            public string fromPlayerUUID;
            public string toPlayerUUID;
            public string playerUserId;
            public string dismissUUID;
            public string friendUUID;
        }

        public string action;
        public Parameters parameters;
    }

    public class WeeklyContestSendData
    {
        public class Parameters
        {
            public int weekId;
            public int weeklyScore;
            public int team;
        }

        public string action;
        public Parameters parameters;
    }

    public class GuildGoalSendData
    {
        public class Parameters
        {
            [DefaultValue(-1)] public int weekId = -1;
        }

        public string action;
        public Parameters parameters;
    }

    public GuildSendData guild;
    public MessengerSendData chat;
    public UserSendData user;
    public BattleSendData battle;
    public BattleSendData adventure;
    public LeaderboardData leaderboards;
    public UserFriendsData userfriends;
    public WeeklyContestSendData weeklycontest;
    public GuildGoalSendData guildgoal;
}