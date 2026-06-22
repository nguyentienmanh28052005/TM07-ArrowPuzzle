using System;
using mygame.sdk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HomePanel : MainMenuPanel 
{
    [SerializeField] Button btnPlay;
    [SerializeField] Button btnProfile;
    [SerializeField] UserAvatarUI userAvatarUI;

    private bool enableClickPlay = true;

    //[Space, Header("Hack")]
    //[SerializeField] Button btn_HackGold;
    //[SerializeField] Button btn_ReGold;
    //[SerializeField] Button btn_ResetGold;

    public static bool IsHomePlay = false;
    private void OnEnable()
    {
        Debug.Log($"HomePanel: {1}");
    }

    private void Awake()
    {
        //if (SdkUtil.isFold())
        //{
        //    BGTransform.localScale = Vector3.one * 1.4f;
        //}
        //else if (SdkUtil.isiPad())
        //{
        //    BGTransform.localScale = Vector3.one * 1.4f;
        //}
        //else
        //{
        //    BGTransform.localScale = Vector3.one;
        //}

        btnPlay.onClick.AddListener(OnClickPlayLevel);
        btnProfile.onClick.AddListener(ShowProfile);
        var activeProfileVersion = 5;
        btnProfile.enabled = SDKManager.Instance.verCodeInstall > activeProfileVersion || PlayerPrefsUtil.CFEnableProfile;
        btnProfile.GetComponent<ButtonScaler>().enabled = SDKManager.Instance.verCodeInstall > activeProfileVersion || PlayerPrefsUtil.CFEnableProfile;

        // var sizeRate = Screen.height / (float)Screen.width;
        // if (sizeRate > 1.8f)
        // {
        //     btnPlay.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 65f);
        // }
        
#if ENABLE_HACK
        //if(btn_HackGold != null && btn_ReGold != null)
        //{
        //    btn_HackGold.gameObject.SetActive(true);
        //    btn_ReGold.gameObject.SetActive(true);
        //    btn_ResetGold.gameObject.SetActive(true);
        //    btn_HackGold.onClick.AddListener(() =>
        //    {
        //        //BoosterManager.Instance.AddBooster(BoosterType.AddHole, 100, "hack", LogEvent.ReasonItem.reward);
        //        //BoosterManager.Instance.AddBooster(BoosterType.ClearHole, 100, "hack", LogEvent.ReasonItem.reward);
        //        //BoosterManager.Instance.AddBooster(BoosterType.BreakObject, 100, "hack", LogEvent.ReasonItem.reward);
        //        GameRes.AddRes(RES_type.GOLD, 1000, "");
        //        //BoosterManager.AddTimeUnlimited(BoosterType.Magnet, 1800);
        //        //BoosterManager.AddTimeUnlimited(BoosterType.MutilColorBox, 1800);
        //    });
        //    btn_ReGold.onClick.AddListener(() =>
        //    {
        //        if (DataManager.Gold >= 1000)
        //        {
        //            GameRes.AddRes(RES_type.GOLD, -1000, "hack");
        //        }
        //    });
        //    btn_ResetGold.onClick.AddListener(() =>
        //    {
        //        GameRes.AddRes(RES_type.GOLD, -DataManager.Gold, "hack");
        //    });
        //}
#else
        //if(btn_HackGold != null && btn_ReGold != null)
        //{
        //    btn_HackGold.gameObject.SetActive(false);
        //    btn_ReGold.gameObject.SetActive(false);
        //    btn_ResetGold.gameObject.SetActive(false);
        //}
#endif
    }

    public override void Active()
    {
        base.Active();
        SetupAvatar();
        RegisterEvent();
        enableClickPlay = true;
    }

    private bool hasRegistered;

    private void RegisterEvent()
    {
        if (!hasRegistered)
        {
            hasRegistered = true;
            GameEvent.OnRefreshAvatar += SetupAvatar;
        }
    }

    private void RemoveEvent()
    {
        if (hasRegistered)
        {
            hasRegistered = false;
            GameEvent.OnRefreshAvatar -= SetupAvatar;
        }
    }

    public void OnDisable()
    {
        RemoveEvent();
        Debug.Log($"HomePanel: {0}");
    }

    private void SetupAvatar()
    {
        var userData = UserDataManager.Instance.UserData;
        userAvatarUI.Initialize(userData.avatarId, userData.frameId, userData.avatarUrl);
    }

    private void ShowProfile()
    {
        if (UserDataManager.IsFirstShowChangeName)
        {
            var UIUserInfor = UIManager.Instance.ShowPopup<UIUserProfile>(SetupAvatar);
        }
        else
        {
            UserDataManager.IsFirstShowChangeName = true;
            UIManager.Instance.ShowPopup<UIEditName>(ShowProfile);
        }
    }

    private void OnClickPlayLevel()
    {
        //if (!SDKManager.Instance.checkConnection()) return;

        //SupperCat
        //EventSupperCatManager eventSupperCatManager = EventSupperCatManager.Instance;
        //if (eventSupperCatManager && eventSupperCatManager.CheckUnlockEvent())
        //{
        //    UIManager.Instance.ShowPopup<PopupSupperCat>(null);
        //    return;
        //}

        var lvl = GameRes.GetLevel();
        HeartManager.Instance.IsNoMoreLives(() =>
        {
            UIManager.Instance.ShowPopup<UIGuildMoreLives>(() =>
            {
                if (HeartManager.Instance.CurrentHeart > 0)
                {
                    PlayGame();
                }
            });
        }, () =>
        {
            if (!enableClickPlay) return;
            if (DataManager.Level >= PlayerPrefsUtil.CFLevelShowPlayPopup)
            {
                UIManager.Instance.ShowPopup<PopupLevelPlay>(null);
            }
            else
            {
                PlayGame();
            }
        });

        return;

        void PlayGame()
        {
            enableClickPlay = false;
            IsHomePlay = true;
            HeartManager.Instance.AddHeart(-1, "play_game", LogEvent.ReasonItem.use);

            GameManager.CurrentPlayType = "play";
            GameManager.Instance.PlayGame();
        }
    }
}