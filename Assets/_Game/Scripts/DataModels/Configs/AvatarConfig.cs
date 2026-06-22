using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "AvatarConfig", menuName = "Data/AvatarConfig")]
public class AvatarConfig : ScriptableObject
{
    [Serializable]
    public class AvatarInfo
    {
        public int id;
        public Sprite icon;
        public bool isLocked;
    }
    
    public AvatarInfo[] avatarInfos;

    public Sprite GetAvatar(int id)
    {
        var av = avatarInfos.SingleOrDefault(x => x.id == id);
        if(av == null) return avatarInfos[Random.Range(0, avatarInfos.Length)].icon;
        return av.icon;
    }

    public Sprite GetPlayerAvatar()
    {
        return GetAvatar(DataManager.Instance.avtarID);
    }
}
