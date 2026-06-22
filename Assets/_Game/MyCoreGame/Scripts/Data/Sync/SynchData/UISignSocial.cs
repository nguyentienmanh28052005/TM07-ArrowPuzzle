using master;
using System;
using System.Collections;
using mygame.sdk;
using UnityEngine;
using UnityEngine.UI;
using UnityWebSocket;
using UniRx;

public enum ESocialType
{
    Facebook = 1,
    Google = 2,
    Apple = 3,
}

public class UISignSocial : PopupUI
{
    public Button btnFb;
    public Button btnGoogle;
    public Button btnApple;
    public Text txtFb;
    public Text txtGoogle;
    public Text txtApple;
    public GameObject loading;
    private bool hasRegisterEvent;

    /// <summary>
    /// 1: FB, 2: GG, 3: Apple
    /// </summary>
    public ESocialType socialType;

    public string socialId;
    private string avatarUrl;
    private string accessToken;

    private void Start()
    {
        RegisterListener();
        btnFb.onClick.AddListener(SignFacebook);
        btnGoogle.onClick.AddListener(SignGoogle);
        btnApple.onClick.AddListener(SignApple);
#if UNITY_ANDROID
        btnApple.gameObject.SetActive(false);
#else
        btnGoogle.gameObject.SetActive(false);
#endif
    }

    private void OnDestroy()
    {
        CancelTimeOut();
        RemoveListener();
    }

    private IDisposable subInternetReachability;

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
                CancelTimeOut();
                CheckInternet();
            }
            else if (isRequestCompareData)
            {
                requestGetCompareData();
            }
        });
    }

    private void RemoveListener()
    {
        if (!hasRegisterEvent) return;
        hasRegisterEvent = false;
        SocketHub.OnSocketError -= OnSocketError;
        SocialManager.OnLinkSocialAccountDone -= OnLinkSocialAccountDone;
        SocialManager.OnGetCompareDataUserDone -= OnGetCompareDataUserDone;
        SocialManager.OnLogOutSocialAccountDone -= OnLogOutSocialAccountDone;
        subInternetReachability.Dispose();
    }

    private void OnLogOutSocialAccountDone(bool obj)
    {
        SocialManager.OnLogOutSocialAccountDone -= OnLogOutSocialAccountDone;
        CancelTimeOut();
        if (obj)
        {
            setSocialId();
            Hide();
            UIManager.Instance.NotifyContent("", "sign_out_success");
        }
        else
        {
            UIManager.Instance.NotifyContent("", "sign_out_failed");
        }
    }

    private void OnSocketError()
    {
        CancelTimeOut();
        UIManager.Instance.NotifyContent("", "errNtry_again");
    }

    private void requestGetCompareData()
    {
        if (!CheckTimeOut(() => { SocialManager.OnGetCompareDataUserDone -= OnGetCompareDataUserDone; })) return;
        isRequestCompareData = true;
        SocialManager.OnGetCompareDataUserDone -= OnGetCompareDataUserDone;
        SocialManager.OnGetCompareDataUserDone += OnGetCompareDataUserDone;
        ServerHub.GetSendRequestServer<SendRequestUser>().RequestGetCompareData();
    }

    private void OnLinkSocialAccountDone(bool obj)
    {
        SocialManager.OnLinkSocialAccountDone -= OnLinkSocialAccountDone;
        CancelTimeOut();
        if (!obj)
        {
            // has old data in server, ask synch data
            requestGetCompareData();
        }
        else
        {
            // no data in server
            setSocialId();
            SetStatusButton();
            Hide();
            SocialManager.Instance.PushData(true);
            UIManager.Instance.NotifyContent("", "sign_in_success");
        }
    }

    private void OnGetCompareDataUserDone(bool status, UserData user1, UserData user2)
    {
        SocialManager.OnGetCompareDataUserDone -= OnGetCompareDataUserDone;
        isRequestCompareData = false;
        CancelTimeOut();
        if (status)
        {
            mainPopUp.gameObject.SetActive(false);
            var UISyncData = UIManager.Instance.ShowPopup<UISyncData>(() =>
            {
                SetStatusButton();
                mainPopUp.gameObject.SetActive(true);
            });
            UISyncData.Setup(user1, user2, socialId, socialType, accessToken);
        }
        else
        {
            /// faild
            UIManager.Instance.NotifyContent("", "sign_in_failed");
        }
    }

    private void setSocialId()
    {
        switch (socialType)
        {
            case ESocialType.Facebook:
                UserDataManager.Instance.SetFacebookId(socialId);
                if (!string.IsNullOrEmpty(socialId))
                {
                    SocialManager.Instance.LoadAvatarFacebook(accessToken, status =>
                    {
                        if (status)
                        {
                            UserDataManager.Instance.ChangeAvatar(9999);
                            ServerHub.GetSendRequestServer<SendRequestUser>().UpdateUserInfo(avatarId: 9999);
                            UserDataManager.IsFirstShowChangeName = true;
                            GameEvent.OnRefreshAvatar?.Invoke();
                        }
                    });
                }
                else
                {
                    UserDataManager.Instance.UserData.avatarUrl = "";
                    UserDataManager.Instance.ChangeAvatar(0);
                    ServerHub.GetSendRequestServer<SendRequestUser>().UpdateUserInfo(avatarUrl: "", avatarId: 0);
                }

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

    public override void Show(Action onClose)
    {
        base.Show(onClose);
        socialId = "";
        SetStatusButton();
        CancelTimeOut();
    }

    private void SignApple()
    {
        if (!CheckInternet()) return;
        var dataUser = UserDataManager.Instance.UserData;
        if (!string.IsNullOrEmpty(dataUser.appleId))
        {
            var UIWarningPopup = UIManager.Instance.ShowPopup<UIWarningPopup>(null);
            var title = MutilLanguage.getStringWithKey("are_you_sure");
            var content = MutilLanguage.getStringWithKey("ask_sign_out_x", stateFormat: mygame.sdk.FormatText.F_String,
                obFormat: "Apple");
            UIWarningPopup.Setup1(title, content,
                onYes: () =>
                {
                    socialType = ESocialType.Apple;
                    socialId = "";
                    if (!CheckTimeOut(() =>
                        {
                            SocialManager.OnLogOutSocialAccountDone -= OnLogOutSocialAccountDone;
                        })) return;
                    RequestLogOutSocialAccount(3);
                }, onNo: () => { });
            return;
        }

        if (!CheckTimeOut(() => { })) return;
        SocialManager.Instance.LoginSocial((status, id, avatarUrl) =>
        {
            if (status == 1)
            {
                this.avatarUrl = avatarUrl;
                socialType = ESocialType.Apple;
                socialId = id;
                if (!CheckTimeOut(() => { SocialManager.OnLinkSocialAccountDone -= OnLinkSocialAccountDone; })) return;
                RequestLinkSocialAccount(appleId: id);
            }
            else
            {
                CancelTimeOut();
                UIManager.Instance.NotifyContent("", "sign_in_failed");
            }
        });
    }

    private void SignGoogle()
    {
        if (!CheckInternet()) return;
        var dataUser = UserDataManager.Instance.UserData;
        if (!string.IsNullOrEmpty(dataUser.googleId))
        {
            var UIWarningPopup = UIManager.Instance.ShowPopup<UIWarningPopup>(null);
            var title = MutilLanguage.getStringWithKey("are_you_sure");
            var content = MutilLanguage.getStringWithKey("ask_sign_out_x", stateFormat: mygame.sdk.FormatText.F_String,
                obFormat: "Google");
            UIWarningPopup.Setup1(title, content,
                onYes: () =>
                {
                    socialType = ESocialType.Google;
                    socialId = "";
                    if (!CheckTimeOut(() =>
                        {
                            SocialManager.OnLogOutSocialAccountDone -= OnLogOutSocialAccountDone;
                        })) return;
                    RequestLogOutSocialAccount(2);
                }, onNo: () => { });
            return;
        }

        if (!CheckTimeOut(() => { })) return;
        SocialManager.Instance.LoginSocial((status, id, avatarUrl) =>
        {
            if (status == 1)
            {
                this.avatarUrl = avatarUrl;
                socialType = ESocialType.Google;
                socialId = id;
                if (!CheckTimeOut(() => { SocialManager.OnLinkSocialAccountDone -= OnLinkSocialAccountDone; })) return;
                RequestLinkSocialAccount(googleId: id);
            }
            else
            {
                CancelTimeOut();
                UIManager.Instance.NotifyContent("", "sign_in_failed");
            }
        });
    }

    private void SignFacebook()
    {
        if (!CheckInternet()) return;
        var dataUser = UserDataManager.Instance.UserData;
        if (!string.IsNullOrEmpty(dataUser.facebookId))
        {
            var UIWarningPopup = UIManager.Instance.ShowPopup<UIWarningPopup>(null);
            var title = MutilLanguage.getStringWithKey("are_you_sure");
            var content = MutilLanguage.getStringWithKey("ask_sign_out_x", stateFormat: FormatText.F_String,
                obFormat: "Facebook");
            UIWarningPopup.Setup1(title, content,
                onYes: () =>
                {
                    socialType = ESocialType.Facebook;
                    socialId = "";
                    if (!CheckTimeOut(() =>
                        {
                            SocialManager.OnLogOutSocialAccountDone -= OnLogOutSocialAccountDone;
                        })) return;
                    FBHelper.Instance.LogOut();
                    RequestLogOutSocialAccount(1);
                }, onNo: () => { });
            return;
        }

        if (!CheckTimeOut(() => { })) return;
        SocialManager.Instance.LoginFacebook((status, id, token) =>
        {
            if (status == 1)
            {
                socialType = ESocialType.Facebook;
                socialId = id;
                accessToken = token;
                if (!CheckTimeOut(() => { SocialManager.OnLinkSocialAccountDone -= OnLinkSocialAccountDone; })) return;
                RequestLinkSocialAccount(facebookId: id, platformToken: token);
            }
            else
            {
                CancelTimeOut();
                UIManager.Instance.NotifyContent("", "sign_in_failed");
            }
        });
    }

    private void SetStatusButton()
    {
        var dataUser = UserDataManager.Instance.UserData;
        txtFb.SetText(string.IsNullOrEmpty(dataUser.facebookId) ? "sign_in_fb" : "sign_out_fb");
        txtGoogle.SetText(string.IsNullOrEmpty(dataUser.googleId) ? "sign_in_gg" : "sign_out_gg");
        txtApple.SetText(string.IsNullOrEmpty(dataUser.appleId) ? "sign_in_apple" : "sign_out_apple");
    }

    private static bool CheckInternet()
    {
        if (!ServerHub.Instance.InternetState)
        {
            UIManager.Instance.NotifyContent("", "please_check_internet");
            return false;
        }

        if (SocketHub.WebSocketState == WebSocketState.Open && SocketHub.IsLoggedGame.Value) return true;
        UIManager.Instance.NotifyContent("", "errNtry_again");
        return false;
    }

    private Coroutine corCheckTimeOut;
    private bool isRequestCompareData;

    private bool CheckTimeOut(Action callback)
    {
        CancelTimeOut();
        if (!CheckInternet()) return false;
        corCheckTimeOut = StartCoroutine(_checkTimeOut(callback));
        loading.SetActive(true);
        return true;
    }

    private IEnumerator _checkTimeOut(Action callback)
    {
        for (var i = 0; i < 30; i++)
        {
            yield return new WaitForSeconds(1f);
        }

        loading.SetActive(false);
        callback?.Invoke();
        UIManager.Instance.NotifyContent("", "errNtry_again");
    }

    private void CancelTimeOut()
    {
        if (corCheckTimeOut != null)
        {
            StopCoroutine(corCheckTimeOut);
        }

        loading.SetActive(false);
    }

    private void RequestLogOutSocialAccount(int option)
    {
        SocialManager.OnLogOutSocialAccountDone -= OnLogOutSocialAccountDone;
        SocialManager.OnLogOutSocialAccountDone += OnLogOutSocialAccountDone;
        ServerHub.GetSendRequestServer<SendRequestUser>().RequestLogOutSocialAccount(option);
    }

    public void RequestLinkSocialAccount(string facebookId = null, string googleId = null, string appleId = null,
        string platformToken = null)
    {
        SocialManager.OnLinkSocialAccountDone -= OnLinkSocialAccountDone;
        SocialManager.OnLinkSocialAccountDone += OnLinkSocialAccountDone;
        ServerHub.GetSendRequestServer<SendRequestUser>()
            .RequestLinkSocialAccount(facebookId, googleId, appleId, platformToken);
    }
}