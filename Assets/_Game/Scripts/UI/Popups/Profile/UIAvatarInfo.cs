using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAvatarInfo : MonoBehaviour
{
   private UIAvatarPage avatarPage;

   [SerializeField] private Button clickBtn;
   [SerializeField] private Image avtarIcon;
   [SerializeField] private GameObject equipObject;
   [SerializeField] private GameObject selectObject;

   private AvatarConfig.AvatarInfo avatarInfo;
   public int avtarID => avatarInfo.id;

   private void Start()
   {
      clickBtn.onClick.AddListener(OnClickAvatar);
   }

   public void Initialized(UIAvatarPage av, AvatarConfig.AvatarInfo avatar)
   {
      avatarPage = av;
      avatarInfo = avatar;
      avtarIcon.sprite = avatar.icon;
      equipObject.SetActive(false);
      selectObject.SetActive(false);
   }
   
   private void OnClickAvatar()
   {
      avatarPage.OnSelectAvatar(this);
   }
   
   public void SetEquipStatus(bool status)
   {
      if (status)
      {
         selectObject.SetActive(false);
         equipObject.SetActive(true);
      }
      else
      {
         equipObject.SetActive(false);
      }
   }

   public void SetSelectStatus(bool status)
   {
      if (status)
      {
         if (avatarInfo.isLocked)
         {
            selectObject.SetActive(true);
         }
         else
         {
            avatarPage.OnEquipAvatar(this);
         }
      }
      else
      {
         selectObject.SetActive(false);
      }
   }
}
