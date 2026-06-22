
using master;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityWebSocket;
using UniRx;

public class UISyncData : PopupUI
{
    public Button btnYes;
    public Button btnNo;
    public InfoSynchDataContainer[] infos;
    public AvatarGuildSO avatarGuildSO;
    public GameObject loading;
    private int selected;
    private float startTimeRequest;
    private IDisposable subInternetReachability;
    private bool isRequesting;

    public override void Show(Action onClose)
    {
        base.Show(onClose);
        loading.SetActive(false);
    }

    private void Start()
    {
        RegisterListener();
        btnYes.onClick.AddListener(OnClickYes);
        btnYes.onClick.AddListener(OnClickNo);
        infos[0].btnSelected.onClick.AddListener(() => OnClickSelect(0));
        infos[1].btnSelected.onClick.AddListener(() => OnClickSelect(1));
    }

    private void OnDestroy()
    {
        cancelTimeOut();
        RemoveListener();
    }

    private bool hasRegisterEvent;

    private void RegisterListener()
    {
        if (hasRegisterEvent) return;
        hasRegisterEvent = true;
        SocketHub.OnSocketError += OnSocketError;
        var ob1 = master.Observer.GetObservable(master.ObserverName.internet_reachability, false);
        subInternetReachability = ob1.Subscribe(x =>
        {
            var v = (bool)x;
            if (!v)
            {
                cancelTimeOut();
                CheckInternet();
            }
            else if (isRequesting)
            {
                OnClickYes();
            }
        });
    }

    private void RemoveListener()
    {
        if (!hasRegisterEvent) return;
        hasRegisterEvent = false;
        SocketHub.OnSocketError -= OnSocketError;
        SocialManager.OnSynchUserDataDone -= OnSynchUserDataDone;
        subInternetReachability.Dispose();
    }

    private void OnSocketError()
    {
        cancelTimeOut();
        UIManager.Instance.NotifyContent("", "errNtry_again");
    }

    private void setSocialId()
    {
        switch (socialType)
        {
            case ESocialType.Facebook:
                UserDataManager.Instance.SetFacebookId(socialId);
                SocialManager.Instance.LoadAvatarFacebook(accessToken, status =>
                {
                    if (status)
                    {
                        UserDataManager.Instance.ChangeAvatar(9999);
                        ServerHub.GetSendRequestServer<SendRequestUser>().UpdateUserInfo(avatarId: 9999);
                        GameEvent.OnRefreshAvatar?.Invoke();
                    }
                });
                break;
            case ESocialType.Google:
            {
                UserDataManager.Instance.SetGoogleId(socialId);
                break;
            }
            default:
                UserDataManager.Instance.SetAppleId(socialId);
                break;
        }
    }

    private void OnSynchUserDataDone(UserData dataUser)
    {
        isRequesting = false;
        cancelTimeOut();
        if (dataUser != null)
        {
            if (selected == 0)
            {
                Hide();
                SocialManager.Instance.ReceiveUserDataSync(dataUser);
                ServerHub.Instance.Disconnect();
                SceneManager.LoadScene(0);
            }
            else
            {
                setSocialId();
                Hide();
            }

            UIManager.Instance.NotifyContent("", "synch_data_success");
        }
        else
        {
            Hide();
            loading.SetActive(false);
            UIManager.Instance.NotifyContent("", "synch_data_failed");
        }
    }

    private void OnClickNo()
    {
    }

    private void OnClickYes()
    {
        if (!CheckInternet()) return;
        cancelTimeOut();
        loading.SetActive(true);
        corCheckTimeOut = StartCoroutine(checkTimeOut());
        isRequesting = true;
        SocialManager.OnSynchUserDataDone -= OnSynchUserDataDone;
        SocialManager.OnSynchUserDataDone += OnSynchUserDataDone;
        ServerHub.GetSendRequestServer<SendRequestUser>().RequestSynchData(selected + 1);
    }

    private void OnClickSelect(int v)
    {
        selected = v;
        infos[v].objSelected.SetActive(true);
        infos[v == 0 ? 1 : 0].objSelected.SetActive(false);
    }

    private string accessToken;
    public string socialId;
    public ESocialType socialType;

    public void Setup(UserData oldData, UserData newData, string socialId, ESocialType socialType, string accessToken)
    {
        this.socialId = socialId;
        this.socialType = socialType;
        this.accessToken = accessToken;
        newData = UserDataManager.Instance.UserData;

        infos[0].Initialize(oldData, avatarGuildSO);
        infos[1].Initialize(newData, avatarGuildSO);
        OnClickSelect(0);
    }

    private static bool CheckInternet()
    {
        if (!ServerHub.Instance.InternetState)
        {
            UIManager.Instance.NotifyContent("", "please_check_internet");
            return false;
        }
        else if (SocketHub.WebSocketState != WebSocketState.Open)
        {
            UIManager.Instance.NotifyContent("", "errNtry_again");
            return false;
        }

        return true;
    }

    private Coroutine corCheckTimeOut;

    IEnumerator checkTimeOut()
    {
        for (int i = 0; i < 30; i++)
        {
            yield return new WaitForSeconds(1f);
        }

        UIManager.Instance.NotifyContent("", "errNtry_again");
    }

    private void cancelTimeOut()
    {
        if (corCheckTimeOut != null)
        {
            StopCoroutine(corCheckTimeOut);
        }

        loading.SetActive(false);
    }
}