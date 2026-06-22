using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class UserAvatarUIManager
{
    public static Dictionary<string, List<UserAvatarUI>> userAvatarDict = new();
    
    public static void AddUserAvatar(string playerUUID, UserAvatarUI userAvatarUI)
    {
        if (!userAvatarDict.ContainsKey(playerUUID))
        {
            userAvatarDict.Add(playerUUID,new List<UserAvatarUI>());
        }
        userAvatarDict[playerUUID].Add(userAvatarUI);
    }
    public static void RemoveUserAvatar(string playerUUID, UserAvatarUI userAvatarUI)
    {
        if (userAvatarDict.ContainsKey(playerUUID))
        {
            userAvatarDict[playerUUID].Remove(userAvatarUI);
            if (userAvatarDict[playerUUID].Count <= 0)
            {
                userAvatarDict.Remove(playerUUID);
            }
        }
    }
    public static List<UserAvatarUI> GetUserAvatarList(string playerUUID)
    {
        return userAvatarDict.GetValueOrDefault(playerUUID);
    }
    public static void ClearAllUserAvatar()
    {
        userAvatarDict.Clear();
    }
}
public class UserAvatarUI : MonoBehaviour
{
    public Image imgAvatar;
    public Transform frameHolder;

    private GameObject currentFrame;
    private int frameId = -1;
    private string playerUUID;
    
    private bool hasRegister = false;
    public void Initialize(UserData userData, bool isWaitGetAvatar = false, bool isUpdateAvatar = false)
    {
        var tmpAvatarId = userData.avatarId;
        var tmpFrameId = userData.frameId;
        var tmpAvatarUrl = userData.avatarUrl;
        if (isUpdateAvatar)
        {
            UpdateOthersAvatar(userData.playerUUID, userData.avatarId, userData.avatarUrl, userData.frameId);
            UserDataManager.loadedUserAvatarByUUID[userData.playerUUID] = new UserDataManager.GroupAvatarUI() {avatarId = userData.avatarId,frameId=userData.frameId, avatarUrl = userData.avatarUrl, nameId = userData.nameId, name = userData.name};
        }
        else if(!string.IsNullOrEmpty(userData.playerUUID))
        {
            var groupUserData = UserDataManager.loadedUserAvatarByUUID.GetValueOrDefault(userData.playerUUID);
            if(groupUserData!=null)
            {
                tmpAvatarId = groupUserData.avatarId;
                tmpAvatarUrl = groupUserData.avatarUrl;
                tmpFrameId = groupUserData.frameId;
            }
            if(!isWaitGetAvatar) Register(userData.playerUUID);
        }
        InitFrame(tmpFrameId);
        InitAvatar(tmpAvatarId, tmpAvatarUrl, isWaitGetAvatar, userData.playerUUID);
    }

    public void Initialize(int avatarId, int frameId, string avatarUrl, string playerUUID = "", bool isUpdateAvatar = false)
    {
        if (isUpdateAvatar)
        {
            UpdateOthersAvatar(playerUUID, avatarId, avatarUrl, frameId);
        }
        else if(!string.IsNullOrEmpty(playerUUID))
        {
            var groupUserData = UserDataManager.loadedUserAvatarByUUID.GetValueOrDefault(playerUUID);
            if(groupUserData!=null)
            {
                avatarId = groupUserData.avatarId;
                avatarUrl = groupUserData.avatarUrl;
                frameId = groupUserData.frameId;
            }
            Register(playerUUID);
        }
        InitFrame(frameId);
        InitAvatar(avatarId, avatarUrl, false, playerUUID);
    }

    private void UpdateOthersAvatar(string playerUUID,int avatarId, string avatarUrl, int frameId)
    {
        var listUserAvatarUI = UserAvatarUIManager.GetUserAvatarList(playerUUID);
        if(listUserAvatarUI?.Count>0)
        {
            foreach (var item in listUserAvatarUI)
            {
                item.Initialize(avatarId, frameId, avatarUrl, playerUUID);
            }
        }
    }

    private void Register(string playerUUID)
    {
        if(!hasRegister || this.playerUUID != playerUUID)
        {
            hasRegister = true;
            if (!string.IsNullOrEmpty(this.playerUUID))
            {
                UserAvatarUIManager.RemoveUserAvatar(this.playerUUID,this);
            }
            if (!string.IsNullOrEmpty(playerUUID))
            {
                UserAvatarUIManager.AddUserAvatar(playerUUID, this);
            }
            this.playerUUID = playerUUID;
                
        }
    }

    private void Remove()
    {
        if (hasRegister)
        {
            hasRegister = false;
            if(!string.IsNullOrEmpty(playerUUID)) UserAvatarUIManager.RemoveUserAvatar(playerUUID,this);
        }
    }

    public void InitFrame(int frameId)
    {
        if (currentFrame == null || this.frameId != frameId)
        {
            this.frameId = frameId;
            if (currentFrame != null) Destroy(currentFrame);
            currentFrame = Instantiate(UserDataManager.Instance.GetFrame(frameId), frameHolder);
            currentFrame.gameObject.SetActive(true);
        }
    }

    private Coroutine coroutine, waitCacheAvatarUICoroutine;

    public void InitAvatar(int avatarId, string avatarUrl, bool isWaitGetAvatar = false, string playerUUID = "")
    {
        if (coroutine != null)
        {
            GameManager.Instance.StopCoroutine(coroutine);
        }

        if (waitCacheAvatarUICoroutine != null)
        {
            GameManager.Instance.StopCoroutine(waitCacheAvatarUICoroutine);
        }

        if (isWaitGetAvatar)
        {
            waitCacheAvatarUICoroutine = GameManager.Instance.StartCoroutine(WaitCacheAvatarUI(playerUUID));
        }
        
        if (avatarId != 9999)
        {
            imgAvatar.sprite = UserDataManager.Instance.GetAvatar(avatarId, playerUUID);
        }
        else
        {
            coroutine = GameManager.Instance.StartCoroutine(GetAvatarUrl(avatarId, avatarUrl, playerUUID, isWaitGetAvatar));
        }
    }

    private IEnumerator WaitCacheAvatarUI(string playerUUID)
    {
        yield return new WaitForSeconds(0.5f);
        Register(playerUUID);
    }
    private IEnumerator GetAvatarUrl(int avatarId ,string avatarUrl,string playerUUID, bool isWaitGetAvatar)
    {
        var cacheSpriteUser = UserDataManager.Instance.GetCacheAvatar(playerUUID);
        if (cacheSpriteUser != null)
        {
            imgAvatar.sprite = cacheSpriteUser;
            yield break;
        }
        if (isWaitGetAvatar)
        {
            yield return new WaitForSeconds(0.5f);
        }
        yield return GameManager.Instance.StartCoroutine(UserDataManager.Instance.GetAvatarUrl(avatarUrl, playerUUID, sp =>
        { 
            if(imgAvatar==null) return;
            if (sp == null)
            {
               imgAvatar.sprite = UserDataManager.Instance.GetAvatar(avatarId);
            }
            else
            {
               imgAvatar.sprite = sp;
            }
        }));
    }
    
    public Sprite GetAvatarSprite()
    {
        return imgAvatar.sprite;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            if (coroutine != null)
            {
                GameManager.Instance.StopCoroutine(coroutine);
            }
        }
        Remove();
    }
}