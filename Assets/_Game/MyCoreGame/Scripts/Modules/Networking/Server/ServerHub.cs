using System;
using time;
using UnityEngine;
using master;
using mygame.sdk;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

public class ServerHub : master.Singleton<ServerHub>
{
    private bool lastInternetState;
    public bool InternetState => lastInternetState;
    private void Start()
    {
        socket.Connect();
    }
    private void Update()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            if (lastInternetState)
            {
                lastInternetState = false;
                Disconnect();
                master.Observer.Notify(ObserverName.internet_reachability, false);
                return;
            }
        }
        else if (!lastInternetState)
        {
            lastInternetState = true;
            ConnectServer();
            master.Observer.Notify(ObserverName.internet_reachability, true);
        }
    }
    public static async UniTask<bool> CheckInternetAvailable(int timeoutSeconds = 10)
        {
            const string testUrl = "https://www.apple.com/library/test/success.html"; 
            using var request = UnityWebRequest.Head(testUrl);
            request.timeout = timeoutSeconds;
            try
            {
                await request.SendWebRequest().ToUniTask();
                return request.result == UnityWebRequest.Result.Success;
            }
            catch
            {
                return false;
            }
        }
    public static string DomainServer
    {
#if UNITY_ANDROID
        get => PlayerPrefsBase.Instance().getString("domain_server", "https://android.iron.twistedtangle.store/api");
#else
        get => PlayerPrefsBase.Instance().getString("domain_server", "https://iron.twistedtangle.store/api");
#endif
        set => PlayerPrefsBase.Instance().setString("domain_server", value);
    }
    public static Action<UserData> OnReceiveUserInfo;

    [SerializeField] private SocketHub socket;
    public SocketHub Socket => socket;
    public static FactorySendRequestServer FactorySendRequestServer { get; private set; }
    public static T GetSendRequestServer<T>() where T : ISendRequestServer
    {
        if (FactorySendRequestServer == null)
        {
            FactorySendRequestServer = new FactorySendRequestServer();
        }
        return (T)FactorySendRequestServer.GetObj(typeof(T).Name);
    }
    public void ConnectServer()
    {
        socket.Connect();
    }

    public void Disconnect()
    {
        socket.Disconnect();
        
    }
}
