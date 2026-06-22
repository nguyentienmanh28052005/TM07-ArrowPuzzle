using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using mygame.sdk;
using time;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIUserProfile : PopupUI
{
    [SerializeField] private UserFrameSO frameSO;
    [SerializeField] private UserAvatarSO avatarSO;
    [SerializeField] private UserNameSO nameSO;

    [SerializeField] private UserAvatarUI userAvatarUI;
    [SerializeField] private UserNameUI userNameUI;
    [SerializeField] private Text levelText;
    [SerializeField] private Text countryText;
    [SerializeField] private Text firstLoginText;
    
    [SerializeField] private Button changeNameButton;
    [SerializeField] private Text winStreakText;
    [SerializeField] private Text firstTryWinText;
    [SerializeField] private Text winRateText;
    [SerializeField] private Text totalLoginText;
    [SerializeField] private Text loginStreakText;
    
    
    public override void Show(Action onClose = null)
    {
        base.Show(onClose);
        var userData = UserDataManager.Instance.UserData;
        DateTime firstLogin = SdkUtil.timeStamp2DateTime(UserDataManager.FirstTimeJoinGame);
        
        userNameUI.Initialize(userData.nameId, userData.name, userData.playerUUID);
        winStreakText.text = $"{userData.continuousWin}";
        levelText.text = $"{DataManager.Level}";
        countryText.text = GetCountryName(GameHelper.CountryDefault);
        firstTryWinText.text = $"{userData.firstTryWins}";
        firstLoginText.text = $"{firstLogin.Month:D2}/{firstLogin.Year:D4}";
        totalLoginText.text = $"{userData.day_login}";
        winRateText.text = $"{(userData.totalGames == 0 ? 100 : (int)(100f * userData.totalWins / userData.totalGames))}%";
        loginStreakText.text = $"{userData.continuousLogin}";
        
        changeNameButton.onClick.AddListener(() => ChangeName(onClose));
    }

    public string GetCountryName(string countryCode)
    {
        if (countryCode == "default") countryCode = "VN";
        try
        {
            RegionInfo region = new RegionInfo(countryCode.ToUpper());
            return region.NativeName; // hoặc region.NativeName
        }
        catch
        {
            return countryCode; // fallback nếu code không hợp lệ
        }
    }
    
    public void ChangeName(Action onClose)
    {
        UIManager.Instance.ShowPopup<UIEditName>(onClose);
        Hide();
    }

    public override void Hide()
    {
        base.Hide();
        changeNameButton.onClick.RemoveAllListeners();
    }
}