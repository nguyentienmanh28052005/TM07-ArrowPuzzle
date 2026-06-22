using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class UserNameUIManager
{
    public static Dictionary<string, List<UserNameUI>> userNameUIDict = new();
    
    public static void AddUserName(string playerUUID, UserNameUI userAvatarUI)
    {
        if (!userNameUIDict.ContainsKey(playerUUID))
        {
            userNameUIDict.Add(playerUUID,new List<UserNameUI>());
        }
        userNameUIDict[playerUUID].Add(userAvatarUI);
    }
    public static void RemoveUserName(string playerUUID, UserNameUI userAvatarUI)
    {
        if (userNameUIDict.ContainsKey(playerUUID))
        {
            userNameUIDict[playerUUID].Remove(userAvatarUI);
            if (userNameUIDict[playerUUID].Count <= 0)
            {
                userNameUIDict.Remove(playerUUID);
            }
        }
    }
    public static List<UserNameUI> GetUserNameList(string playerUUID)
    {
        return userNameUIDict.GetValueOrDefault(playerUUID);
    }
    public static void ClearAllUserName()
    {
        userNameUIDict.Clear();
    }
}
public class UserNameUI : MonoBehaviour
{
    [SerializeField] private Text defaultNameText;
    [SerializeField] private int maxCharLimit = 16;
    private int currentNameId = 0;
    private Text currentText;
    public Text CurrentText
    {
        get
        {
            if(currentText == null)
            {
                return defaultNameText;
            }
            return currentText;
        }
    }
    
    private bool hasRegister = false;
    private string playerUUID = "";
    private Coroutine waitRegisterCoroutine;
    public void Initialize(UserData userData, bool isWaitRegisterNameUI = false, bool isUpdateNameUI = false)
    {
        var tmpNameId = userData.nameId;
        var tmpName = userData.name;
        if (isUpdateNameUI)
        {
            UpdateOtherName(userData.playerUUID, userData.nameId, userData.name);
        }
        else if(!string.IsNullOrEmpty(userData.playerUUID))
        {
            var groupUserData = UserDataManager.loadedUserAvatarByUUID.GetValueOrDefault(userData.playerUUID);
            if(groupUserData!=null)
            {
                tmpNameId = groupUserData.nameId;
                tmpName = groupUserData.name;
            }

            if(!isWaitRegisterNameUI) Register(userData.playerUUID);
            else
            {
                if(waitRegisterCoroutine != null) GameManager.Instance.StopCoroutine(waitRegisterCoroutine);
                waitRegisterCoroutine = GameManager.Instance.StartCoroutine(WaitRegisterNameUI(userData.playerUUID));
            }
        }
        InitName(tmpNameId, tmpName);
    }
    public void Initialize(int nameId, string name, string playerUUID = "", bool isUpdateNameUI = false)
    {
        if (isUpdateNameUI)
        {
            UpdateOtherName(playerUUID, nameId, name);
        }
        else if(!string.IsNullOrEmpty(playerUUID))
        {
            var groupUserData = UserDataManager.loadedUserAvatarByUUID.GetValueOrDefault(playerUUID);
            if(groupUserData!=null)
            {
                nameId = groupUserData.nameId;
                name = groupUserData.name;
            }
            Register(playerUUID);
        }
        InitName(nameId, name);
    }
    
    private void Register(string playerUUID)
    {
        if(!hasRegister || this.playerUUID != playerUUID)
        {
            hasRegister = true;
            if (!string.IsNullOrEmpty(this.playerUUID))
            {
                UserNameUIManager.RemoveUserName(this.playerUUID, this);
            }
            if (!string.IsNullOrEmpty(playerUUID))
            {
                UserNameUIManager.AddUserName(playerUUID, this);
            }
            this.playerUUID = playerUUID;
        }
    }

    private IEnumerator WaitRegisterNameUI(string playerUUID)
    {
        yield return new WaitForSeconds(0.5f);
        Register(playerUUID);
    }

    private void Remove()
    {
        if (hasRegister)
        {
            hasRegister = false;
            if (!string.IsNullOrEmpty(playerUUID))
            {
                UserNameUIManager.RemoveUserName(playerUUID,this);
            }
        }
    }
    private void UpdateOtherName(string playerUUID ,int nameId, string name)
    {
        var listUserAvatarUI = UserNameUIManager.GetUserNameList(playerUUID);
        if(listUserAvatarUI?.Count>0)
        {
            foreach (var item in listUserAvatarUI)
            {
                item.Initialize(nameId, name, playerUUID);
            }
        }
    }
    public void AdjustTextInLine(float maxWidth)
    {
        CurrentText.AdjustContentTextInLine(maxWidth, CurrentText.text, false);
    }
    private string GetTruncatedName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "";
        if (maxCharLimit > 0 && name.Length > maxCharLimit)
        {
            return name.Substring(0, maxCharLimit) + "...";
        }
        return name;
    }
    private void InitName(int nameId, string name)
    {
        name = GetTruncatedName(name);
        if (nameId <= 0)
        {
            defaultNameText.gameObject.SetActive(true);
            defaultNameText.SetValue(name);
            if(currentText != null) Destroy(currentText.gameObject);
            currentText = null;
            currentNameId = 0;
        }
        else
        {
            if (currentNameId == nameId)
            {
                currentText.SetValue(name);
            }
            else
            {
                if (currentNameId != 0)
                {
                    if(currentText != null) Destroy(currentText.gameObject);
                    currentNameId = 0;
                    InitName(nameId, name);
                }
                else
                {
                    currentText = Instantiate(UserDataManager.Instance.GetNameText(nameId), transform);
                    currentNameId = nameId;
                    CopyRectTransform(defaultNameText.GetComponent<RectTransform>(), currentText.GetComponent<RectTransform>());
                    CopyTextComponent(defaultNameText, currentText);
                    defaultNameText.gameObject.SetActive(false);
                    currentText.SetValue(name);
                }
            }
        }
    }
    void CopyRectTransform(RectTransform source, RectTransform target)
    {
        if (source == null || target == null) return;

        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.anchoredPosition = source.anchoredPosition;
        target.sizeDelta = source.sizeDelta;
        target.pivot = source.pivot;
        target.localScale = source.localScale;
        target.localRotation = source.localRotation;
        target.localPosition = source.localPosition;
    }
    void CopyTextComponent(Text source, Text target)
    {
        if (source == null || target == null) return;
        
        target.fontSize = source.fontSize;
        target.fontStyle = source.fontStyle;
        target.alignment = source.alignment;
        target.lineSpacing = source.lineSpacing;
        target.supportRichText = source.supportRichText;
        target.resizeTextForBestFit = source.resizeTextForBestFit;
        target.resizeTextMinSize = source.resizeTextMinSize;
        target.resizeTextMaxSize = source.resizeTextMaxSize;
        target.horizontalOverflow = source.horizontalOverflow;
        target.verticalOverflow = source.verticalOverflow;
        target.raycastTarget = source.raycastTarget;
    }

    private void OnDestroy()
    {
        Remove();
    }
}
