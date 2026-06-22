using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using DG.Tweening;

public class UIEditName : PopupUI
{
    [SerializeField] private InputField inputField;
    [SerializeField] private Button saveButton;

    private string userName;
    public override void Show(Action onClose = null)
    {
        base.Show(onClose);
        EventSystem.current.SetSelectedGameObject(null);
        userName = UserDataManager.Instance.UserData.name;
        inputField.text = userName;
        inputField.Select();
        inputField.ActivateInputField();
        inputField.onValueChanged.AddListener(OnValueChange);
        saveButton.onClick.AddListener(SaveName);
    }
    
    private void OnValueChange(string name)
    {
        string asciiOnly = Regex.Replace(name, @"[^a-zA-Z0-9 ]", "");
        asciiOnly = asciiOnly.TrimStart();
        userName = asciiOnly;
        DOVirtual.DelayedCall(0.1f, () =>
        {
            if (name.Length > asciiOnly.Length)
            {
                inputField.text = asciiOnly;
                inputField.DeactivateInputField();
                inputField.ActivateInputField();
                UIManager.Instance.NotifyContent("", "character_not_supported");
            }
        });
    }
    public bool CheckName(string name)
    {
        if (name.Trim().Length < 3)
        {
            UIManager.Instance.NotifyContent("","name_too_short");
            return false;
        }

        return true;

    }
    public void SaveName()
    {
        userName = userName.TrimEnd();
        if(!CheckName(userName)) return;
        UserDataManager.Instance.ChangeUserName(userName);
        ServerHub.GetSendRequestServer<SendRequestUser>().UpdateUserInfo(name: userName);
        LogEvent.PlayerChange("name", 0);
        base.Hide();
    }

    public void OnDisable()
    {
        inputField.onValueChanged.RemoveListener(OnValueChange);
        
    }
}
