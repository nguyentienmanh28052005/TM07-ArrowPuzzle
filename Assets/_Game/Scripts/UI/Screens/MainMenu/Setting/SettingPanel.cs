using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

using mygame.sdk;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingPanel : MainMenuPanel, ISyncData
{
    [SerializeField] private Button musicButton;
    [SerializeField] private Button soundButton;
    [SerializeField] private Button vibrationButton;
    [SerializeField] private Button notifyButton;
    [SerializeField] private Button searchTeamButton;
    [SerializeField] private Button chatButton;
    [SerializeField] private Button saveProgressButton;
    [SerializeField] private Button supportButton;
    [SerializeField] private Button privacyButton;
    [SerializeField] private Button likeFacebookButton;
    [SerializeField] private Button joinGroupFacebookButton;
    [SerializeField] private Button likeInstagramButton;
    [SerializeField] private Button restoreButton;

    [SerializeField] private GameObject disableMusic;
    [SerializeField] private GameObject disableSound;
    [SerializeField] private GameObject disableVibration;
    [SerializeField] private GameObject disableNotify;

    [SerializeField] private GameObject chatOff;
    [SerializeField] private RectTransform horizontal;
    [SerializeField] private RectTransform vertical;

    [SerializeField] private RectTransform LoadingPanel;

    [SerializeField] private Text addGoldFacebookText;
    [SerializeField] private Text addGoldInstagramText;

    public Button closeBtn;

    public static int CFGoldLikeSocial
    {
        get => PlayerPrefs.GetInt("cf_gold_like_social", 200);
        set => PlayerPrefs.SetInt("cf_gold_like_social", value);
    }

    private static bool HaveLikedFacebook
    {
        get => PlayerPrefs.GetInt("have_liked_facebook", 0) == 1 ? true : false;
        set => PlayerPrefs.SetInt("have_liked_facebook", value ? 1 : 0);
    }

    private static bool HaveLikedInstagram
    {
        get => PlayerPrefs.GetInt("have_liked_instagram", 0) == 1 ? true : false;
        set => PlayerPrefs.SetInt("have_liked_instagram", value ? 1 : 0);
    }

    private static bool IsEnableNotification
    {
        get => PlayerPrefs.GetInt("setting_enable_notification", 1) == 1 ? true : false;
        set => PlayerPrefs.SetInt("setting_enable_notification", value ? 1 : 0);
    }

    public static string FanPageFacebookId
    {
        get => PlayerPrefs.GetString("fanPage_facebook_id", "61590360545711");
    }
    
    public static string GroupFacebookId
    {
        get => PlayerPrefs.GetString("group_facebook_id", "2751064381931022");
    }

    public static string FanPageInstagramUrl
    {
        get => PlayerPrefs.GetString("fanpage_instagram_url", "screwpuzzlestory");
    }

    private void OnEnable()
    {
        disableMusic.SetActive(!AudioManager.AudioMusicSetting);
        disableSound.SetActive(!AudioManager.AudioSoundSetting);
        disableVibration.SetActive(PlayerPrefs.GetInt(GameHelper.KeyConfigVibrate, 1) == 0);
        disableNotify.SetActive(!IsEnableNotification);
        chatOff.SetActive(false); // Guild module removed
        // likeFacebookButton.gameObject.SetActive(!HaveLikedFacebook && CFGoldLikeSocial > 0);
        // likeInstagramButton.gameObject.SetActive(!HaveLikedInstagram && CFGoldLikeSocial > 0);

        musicButton.onClick.AddListener(OnMusicButtonClicked);
        soundButton.onClick.AddListener(OnSoundButtonClicked);
        vibrationButton.onClick.AddListener(OnVibrationButtonClicked);
        notifyButton.onClick.AddListener(OnNotifyButtonClicked);
        searchTeamButton.onClick.AddListener(OnSearchTeamButtonClicked);
        chatButton.onClick.AddListener(OnChatButtonClicked);
        saveProgressButton.onClick.AddListener(OnSaveProgressButtonClicked);
        supportButton.onClick.AddListener(OnSupportButtonClicked);
        privacyButton.onClick.AddListener(OnPrivacyButtonClicked);
        likeFacebookButton.onClick.AddListener(LikeFacebookButtonClicked);
        joinGroupFacebookButton.onClick.AddListener(JoinGroupFacebookButtonClicked);
        likeInstagramButton.onClick.AddListener(LikeInstagramButtonClicked);
        restoreButton.onClick.AddListener(RestoreButtonClicked);
        closeBtn.onClick.AddListener(Deactive);

        horizontal.gameObject.SetActive(likeFacebookButton.gameObject.activeSelf ||
                                        likeInstagramButton.gameObject.activeSelf);
        addGoldFacebookText.text = "+" + CFGoldLikeSocial;
        addGoldInstagramText.text = "+" + CFGoldLikeSocial;

        addGoldFacebookText.transform.parent.gameObject.SetActive(!HaveLikedFacebook);
#if UNITY_ANDROID
        restoreButton.gameObject.SetActive(false);
#elif UNITY_IOS
        restoreButton.gameObject.SetActive(true);
#endif
    }

    Tween tweenRestore;

    private void RestoreButtonClicked()
    {
        LoadingPanel.gameObject.SetActive(true);
        var isShowNotify = true;
        tweenRestore = DOVirtual.DelayedCall(10, () =>
        {
            if (!isShowNotify) return;
            isShowNotify = false;
            LoadingPanel.gameObject.SetActive(false);
            UIManager.Instance.NotifyContent(content: "Restore Timed Out", key: "_restore_time_out");
        }, false);
        // InappHelper.OnPurchasesFetchedCB += OnPurchasesFetched;
        //
        // void OnPurchasesFetched(int status)
        // {
        //     if (!isShowNotify) return;
        //     Debug.Log("Restore Success: " + status);
        //     tweenRestore?.Kill();
        //     LoadingPanel.gameObject.SetActive(false);
        //     
        //     if (status == 1)
        //     {
        //         var contentValue = "Restore Purchase Success";
        //         var keyValue = "_restore_success";
        //         UIManager.Instance.NotifyContent(content: contentValue, key:  keyValue);
        //     }
        //     else if (status == 2)
        //     {
        //         var contentValue = "No Purchases To Restore";
        //         var keyValue = "_no_restore_available";
        //         UIManager.Instance.NotifyContent(content: contentValue, key:  keyValue);
        //     }
        //     else
        //     {
        //         UIManager.Instance.NotifyContent(content: "Restore Purchase Failed", key: "_restore_failed");
        //     }
        //     isShowNotify = false;
        //     InappHelper.OnPurchasesFetchedCB -= OnPurchasesFetched;
        // }

        void RestoreSuccess(bool status, string err)
        {
            if (!isShowNotify) return;
            Debug.Log("Restore Success: " + status);
            tweenRestore?.Kill();
            LoadingPanel.gameObject.SetActive(false);
            if (status)
            {
                var contentValue = "Restore Purchase Success";
                var keyValue = "_restore_success";
                UIManager.Instance.NotifyContent(contentValue, keyValue);
            }
            else
            {
                if (string.IsNullOrEmpty(err))
                    UIManager.Instance.NotifyContent(content: "Restore Purchase Failed", key: "_restore_failed");
                else
                    UIManager.Instance.NotifyContent(err);
            }

            isShowNotify = false;
            // InappHelper.OnPurchasesFetchedCB -= OnPurchasesFetched;
        }

        InappHelper.Instance.RestorePurchases(RestoreSuccess);
    }

    private void OnDisable()
    {
        base.Deactive();
        musicButton.onClick.RemoveListener(OnMusicButtonClicked);
        soundButton.onClick.RemoveListener(OnSoundButtonClicked);
        vibrationButton.onClick.RemoveListener(OnVibrationButtonClicked);
        notifyButton.onClick.RemoveListener(OnNotifyButtonClicked);
        searchTeamButton.onClick.RemoveListener(OnSearchTeamButtonClicked);
        chatButton.onClick.RemoveListener(OnChatButtonClicked);
        saveProgressButton.onClick.RemoveListener(OnSaveProgressButtonClicked);
        supportButton.onClick.RemoveListener(OnSupportButtonClicked);
        privacyButton.onClick.RemoveListener(OnPrivacyButtonClicked);
        likeFacebookButton.onClick.RemoveListener(LikeFacebookButtonClicked);
        joinGroupFacebookButton.onClick.RemoveListener(JoinGroupFacebookButtonClicked);
        likeInstagramButton.onClick.RemoveListener(LikeInstagramButtonClicked);
        restoreButton.onClick.RemoveListener(RestoreButtonClicked);
        closeBtn.onClick.RemoveListener(Deactive);
    }

    public void OnMusicButtonClicked()
    {
        var isEnable = AudioManager.AudioMusicSetting;
        if (isEnable)
        {
            AudioManager.AudioMusicSetting = false;
            disableMusic.SetActive(true);
        }
        else
        {
            AudioManager.AudioMusicSetting = true;
            disableMusic.SetActive(false);
        }

        AudioManager.Instance.ChangeStateAudio();
        AudioManager.Instance.FixVolumeMusic();
    }

    public void OnSoundButtonClicked()
    {
        var isEnable = AudioManager.AudioSoundSetting;
        if (isEnable)
        {
            AudioManager.AudioSoundSetting = false;
            disableSound.SetActive(true);
        }
        else
        {
            AudioManager.AudioSoundSetting = true;
            disableSound.SetActive(false);
        }

        AudioManager.Instance.FixVolumeSFX();
    }

    public void OnVibrationButtonClicked()
    {
        var isEnable = PlayerPrefs.GetInt(GameHelper.KeyConfigVibrate, 1) == 1;
        if (isEnable)
        {
            GameHelper.ChangeSettingVibrate(false);
            disableVibration.SetActive(true);
        }
        else
        {
            GameHelper.ChangeSettingVibrate(true);
            disableVibration.SetActive(false);
        }
    }

    public void OnNotifyButtonClicked()
    {
        var isEnable = IsEnableNotification;
        if (isEnable)
        {
            IsEnableNotification = false;
            FIRhelper.Instance.SetActivePushNotificationsForUser(false);
            NotificationHandler.SetActiveNotifications(false);
            disableNotify.SetActive(true);
        }
        else
        {
            IsEnableNotification = true;
            FIRhelper.Instance.SetActivePushNotificationsForUser(true);
            NotificationHandler.SetActiveNotifications(true);
            disableNotify.SetActive(false);
        }
    }

    public void OnSearchTeamButtonClicked()
    {
        // Guild module removed - SearchGuildPopupUI not available
    }

    public void OnChatButtonClicked()
    {
        // Guild module removed - GuildManager not available
    }

    public void OnSaveProgressButtonClicked()
    {
        UIManager.Instance.ShowPopup<UISignSocial>(null);
    }

    public void OnSupportButtonClicked()
    {
        Application.OpenURL("https://thomnguyenstudio.online/policy");
    }

    public void OnPrivacyButtonClicked()
    {
        Application.OpenURL("https://thomnguyenstudio.online/policy");
    }

    public void LikeFacebookButtonClicked()
    {
        OpenFacebookPage();
        if (HaveLikedFacebook) return;
        HaveLikedFacebook = true;
        GameEvent.OnReceiveResource?.Invoke(LogEvent.ReasonItem.reward, "like_facebook",
            new[] { new DataResource(RES_type.GOLD, CFGoldLikeSocial) }, DataManager.Level);
        RewardReceivedHub.AddCacheValue(RES_type.GOLD, CFGoldLikeSocial);
        RewardReceivedHub.Instance.CoinFly(new Vector2(Screen.width / 2, Screen.height / 2), null, CFGoldLikeSocial, null);

        MainMenuScreen mainMenu = UIManager.Instance.GetScreenActive<MainMenuScreen>();
        if (mainMenu != null)
        {
            mainMenu.ResetCurrentPanel();
        }

        Deactive();
        addGoldFacebookText.transform.parent.gameObject.SetActive(!HaveLikedFacebook);
        mainScreenUI.Active();
        DOVirtual.DelayedCall(.1f, () => { RewardReceivedHub.AddCacheValue(RES_type.GOLD, -CFGoldLikeSocial); });
        horizontal.gameObject.SetActive(likeFacebookButton.gameObject.activeSelf || likeInstagramButton.gameObject.activeSelf);
        LayoutRebuilder.ForceRebuildLayoutImmediate(vertical);
    }
    
    public void JoinGroupFacebookButtonClicked()
    {
        string webUrl = "https://www.facebook.com/groups/" + GroupFacebookId; // URL group trên web
        Application.OpenURL(webUrl);
    }


    public void OpenFacebookPage()
    {
        string webUrl = "https://www.facebook.com/" + FanPageFacebookId; // fallback URL
        Application.OpenURL(webUrl);
    }


#if UNITY_ANDROID
    private bool IsFacebookAppInstalled()
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject packageManager = activity.Call<AndroidJavaObject>("getPackageManager"))
            {
                AndroidJavaObject packageInfo =
                    packageManager.Call<AndroidJavaObject>("getPackageInfo", "com.facebook.katana", 0);
                return packageInfo != null;
            }
        }
        catch
        {
            return false;
        }
    }
#endif


    public void OpenInstagram(string username)
    {
        string appUrl = $"instagram://user?username={username}";
        string webUrl = $"https://www.instagram.com/{username}/";
#if UNITY_IOS && !UNITY_EDITOR
        if (URLSchemeChecker.CanOpen(appUrl))
        {
            Application.OpenURL(appUrl);
        }
        else
        {
            Application.OpenURL(webUrl);
        }

#elif UNITY_ANDROID && !UNITY_EDITOR
        if (IsInstagramAppInstalled())
        {
            Application.OpenURL(appUrl);
        }
        else
        {
            Application.OpenURL(webUrl);
        }

#else
        Application.OpenURL(webUrl);
#endif
    }

#if UNITY_ANDROID
    private bool IsInstagramAppInstalled()
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject packageManager = activity.Call<AndroidJavaObject>("getPackageManager"))
            {
                AndroidJavaObject packageInfo =
                    packageManager.Call<AndroidJavaObject>("getPackageInfo", "com.instagram.android", 0);
                return packageInfo != null;
            }
        }
        catch
        {
            return false;
        }
    }
#endif

    public void LikeInstagramButtonClicked()
    {
        if (HaveLikedInstagram) return;
        HaveLikedInstagram = true;
        OpenInstagram(FanPageInstagramUrl);
        GameEvent.OnReceiveResource?.Invoke(LogEvent.ReasonItem.reward, "like_instagram",
            new[] { new DataResource(RES_type.GOLD, CFGoldLikeSocial) }, DataManager.Level);
        RewardReceivedHub.AddCacheValue(RES_type.GOLD, CFGoldLikeSocial);

        MainMenuScreen mainMenu = UIManager.Instance.GetScreenActive<MainMenuScreen>();
        if (mainMenu != null)
        {
            mainMenu.ResetCurrentPanel();
        }

        Deactive();

        mainScreenUI.Active();
        DOVirtual.DelayedCall(.1f, () => { RewardReceivedHub.AddCacheValue(RES_type.GOLD, -CFGoldLikeSocial); });
        RewardReceivedHub.Instance.CoinFly(new Vector2(Screen.width / 2, Screen.height / 2), null, CFGoldLikeSocial,
            null);
        horizontal.gameObject.SetActive(likeFacebookButton.gameObject.activeSelf ||
                                        likeInstagramButton.gameObject.activeSelf);
        LayoutRebuilder.ForceRebuildLayoutImmediate(vertical);
    }

    public void OnSyncData(UserData newData)
    {
        if (newData.game_info != null)
        {
            HaveLikedFacebook = newData.game_info.haveLikedFacebook;
            HaveLikedInstagram = newData.game_info.haveLikedInstagram;
        }
    }

    public void SendSyncData(UserData dataUser)
    {
        dataUser.game_info.haveLikedFacebook = HaveLikedFacebook;
        dataUser.game_info.haveLikedInstagram = HaveLikedInstagram;
    }

    private void OnDestroy()
    {
        if (tweenRestore != null)
        {
            tweenRestore.Kill();
        }
    }
}