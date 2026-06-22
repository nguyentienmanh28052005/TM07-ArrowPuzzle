using System.Collections;
using System.Collections.Generic;
using mygame.sdk;
using UnityEngine;

public class UIAvatarPage : MonoBehaviour
{
    [SerializeField] private UIAvatarInfo uiItem;

    private List<UIAvatarInfo> allItems = new List<UIAvatarInfo>();

    private UIAvatarInfo selectItem;
    private UIAvatarInfo equipItemItem;
    
    private void Start()
    {
        Initialized(DataManager.Instance.avatarConfig.avatarInfos);
        
    }

    private void Initialized(IList<AvatarConfig.AvatarInfo> avatarInfos)
    {
        uiItem.gameObject.SetActive(false);
        var count = Mathf.Max(avatarInfos.Count, allItems.Count);
        for (int i = 0; i < count; i++)
        {
            if (i < avatarInfos.Count)
            {
                UIAvatarInfo item; 
                if (i < allItems.Count)
                {
                    item = allItems[i];
                }
                else
                {
                    item = Instantiate(uiItem, uiItem.transform.parent);
                    allItems.Add(item);
                }

                item.Initialized(this, avatarInfos[i]);
                if (DataManager.Instance.avtarID == avatarInfos[i].id)
                {
                    item.SetEquipStatus(true);
                    selectItem = item;
                    equipItemItem = item;
                }
                item.gameObject.SetActive(true);
            }
            else
            {
                allItems[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnSelectAvatar(UIAvatarInfo avatar)
    {
        if (selectItem != null)
        {
            selectItem.SetSelectStatus(false);
        }

        avatar.SetSelectStatus(true);
        selectItem = avatar;
    }
    
    public void OnEquipAvatar(UIAvatarInfo avatar)
    {
        if (equipItemItem != null)
        {
            equipItemItem.SetEquipStatus(false);
        }

        DataManager.Instance.avtarID = avatar.avtarID;
        LogEvent.PlayerChange("avatar", avatar.avtarID);
        FIRhelper.logEvent("click_avatar_change"); 
        avatar.SetEquipStatus(true);
        equipItemItem = avatar;
    }
}
