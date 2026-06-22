
using System.Collections;
using System.Collections.Generic;
using time;
using UnityEngine;
using UnityEngine.UI;


public class InfoSynchDataContainer : MonoBehaviour
{
    public UserNameUI nameUI;
    public Text txtLevel;
    public Text txtGuildName;
    public Text txtTimeCreate;
    public UserAvatarUI userAvatar;
    public Image imgLogoGuild;
    public Button btnSelected;
    public GameObject objGuild;
    public GameObject objSelected;
    public UserData UserData { get; private set; }

    public void Initialize(UserData dataUser, AvatarGuildSO avatarGuild)
    {
        UserData = dataUser;
        var lv = Mathf.Max(1, dataUser.level);
        var dt = MGTime.TimestampToDateTime(dataUser.createdDate);
        txtTimeCreate.SetValue($"{dt:MM/dd/yyyy}");
        nameUI.Initialize(dataUser.nameId, dataUser.name);
        userAvatar.Initialize(dataUser.avatarId, dataUser.frameId, dataUser.avatarUrl);
        txtLevel.SetValue("Lv " + lv);
        if (string.IsNullOrEmpty(dataUser.guildId))
        {
            objGuild.SetActive(false);
        }
        else
        {
            txtGuildName.SetValue(dataUser.guildName);
            setLogoGuild(dataUser.guildLogoId, avatarGuild);
            objGuild.SetActive(true);
        }
    }

    void setLogoGuild(int index, AvatarGuildSO avatarGuild)
    {
        imgLogoGuild.sprite = avatarGuild.LoadSprite(index);
    }
}