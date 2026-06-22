using master;
using mygame.sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using time;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UserDataManager : master.Singleton<UserDataManager>, ISyncData
{
    private const string FILE_AVATAR_FACEBOOK = "avatar_facebook.txt";
    private const string FILE_DATA_USER = "data_user.ssv";
    private const string PASSWORD = "TwistedTangleLog@2024";
    [SerializeField] private UserFrameSO frameSO;
    [SerializeField] private UserAvatarSO avatarSO;
    [SerializeField] private UserNameSO nameSO;

    #region PlayerPref

    public static long FirstTimeJoinGame
    {
        get => long.Parse(PlayerPrefs.GetString("first_time_join_game", "0"));
        set => PlayerPrefs.SetString("first_time_join_game", value.ToString());
    }

    public static long LastTimeLogin
    {
        get => long.Parse(PlayerPrefs.GetString("last_time_login", "0"));
        set => PlayerPrefs.SetString("last_time_login", value.ToString());
    }

    public static bool IsFirstShowChangeName
    {
        get => PlayerPrefs.GetInt("is_first_show_change_name", 0) == 1;
        set => PlayerPrefs.SetInt("is_first_show_change_name", value ? 1 : 0);
    }

    public static int LastNormalFrameId
    {
        get => PlayerPrefs.GetInt("last_normal_frame_id", -1);
        set => PlayerPrefs.SetInt("last_normal_frame_id", value);
    }

    public static int LastNormalNameId
    {
        get => PlayerPrefs.GetInt("last_normal_name_id", -1);
        set => PlayerPrefs.SetInt("last_normal_name_id", value);
    }

    #endregion

    private UserData userData;
    private UserNameData userNameData;
    private UserFrameData userFrameData;
    public int DayInstall => userData.day_install;
    public int DayLogin => userData.day_login;
    public int SectionLogin => userData.section_login;

    private static string PlayerUUID
    {
        get { return PlayerPrefs.GetString("p_uuid_x", ""); }
        set { PlayerPrefs.SetString("p_uuid_x", value); }
    }

    public UserData UserData => userData;
    private string countryName;
    private Sprite avatarSocialSprite;
    public bool GetUserInfor;
    public int numAvatar => avatarSO.Data.Length;
    public int numFrame => frameSO.Data.Length;
    public string CountryName
    {
        get
        {
            if (!string.IsNullOrEmpty(countryName)) return countryName;
            var text = Resources.Load<TextAsset>("country_name");
            var countryCode = MutilLanguage.Instance().languageCode4mutil;
            if (!string.IsNullOrEmpty(UserData.countryCode))
            {
                countryCode = UserData.countryCode.ToLower();
            }

            if (string.IsNullOrEmpty(countryCode))
            {
                return countryName;
            }

            return countryName;
        }
    }

    private IDisposable subLog;

    protected override void Awake()
    {
        base.Awake();
        loadFrameData();
        loadNameData();
        avatarSocialSprite = LoadSpriteFromLocal();
        GameEvent.OnFinishLevel += OnFinishLevel;
        GameEvent.OnStartLevel += OnStartLevel;
        Initialize();
        subLog = SocketHub.IsLoggedGame.Subscribe(x =>
        {
            if (x)
            {
                ServerHub.GetSendRequestServer<SendRequestUser>()
                    .UpdateUserInfo(level: DataManager.Level, firstTryWins: userData.firstTryWins, name: userData.name,
                        nameId: userData.nameId, frameId: userData.frameId, avatarId: userData.avatarId,
                        avatarUrl: userData.avatarUrl);
            }
        });
    }

    private void OnDestroy()
    {
        GameEvent.OnFinishLevel -= OnFinishLevel;
        GameEvent.OnStartLevel -= OnStartLevel;
    }

    private void OnStartLevel(int arg1, int arg2, EGameMode arg3)
    {
        cacheSpriteUserByUID.Clear();
        loadedUserAvatarByUUID.Clear();
        UserAvatarUIManager.ClearAllUserAvatar();
        UserNameUIManager.ClearAllUserName();
    }

    private void OnFinishLevel(int level, bool isWin, int dif, EGameMode mode)
    {
        if (isWin)
        {
            ServerHub.GetSendRequestServer<SendRequestUser>()
                .UpdateUserInfo(level: DataManager.Level, firstTryWins: userData.firstTryWins);
        }
    }

    public void Initialize()
    {
        userData = SaveLoadUtil.LoadData<UserData>(FILE_DATA_USER);
        if (userData == null)
        {
            userData = new UserData();
        }

        if (userData.game_info != null)
        {
            if (userData.game_info.playerData != null)
            {
                FirstTimeJoinGame = userData.game_info.playerData.firstTimeJoinGame;
                LastTimeLogin = userData.game_info.playerData.lastTimeLogin;
            }

            userData.avatarId = userData.game_info.avatar_id;
        }

        bool isSave = false;
        if (string.IsNullOrEmpty(userData.name))
        {
            userData.name = GetNewUserName();
            isSave = true;
        }

        if (userData.createdDate <= 0)
        {
            userData.createdDate = MGTime.GetUtcTime();
            isSave = true;
        }

        if (string.IsNullOrEmpty(userData.playerUUID) && !string.IsNullOrEmpty(PlayerUUID))
        {
            userData.playerUUID = PlayerUUID;
            isSave = true;
        }

        if (userData.avatarId < 0 && string.IsNullOrEmpty(userData.avatarUrl))
        {
            userData.avatarId = UnityEngine.Random.Range(0, numAvatar);
            isSave = true;
        }

        if (userData.frameId < 0 || userData.frameId >= frameSO.Data.Length)
        {
            if (LastNormalFrameId >= 0)
            {
                userData.frameId = LastNormalFrameId;
                LastNormalFrameId = 0;
            }
            else
            {
                userData.frameId = UnityEngine.Random.Range(0, 3);
                LastNormalFrameId = userData.frameId;
            }

            isSave = true;
        }

        if (userData.nameId < 0 || userData.nameId >= nameSO.Data.Length)
        {
            if (LastNormalNameId >= 0)
            {
                userData.nameId = LastNormalNameId;
                LastNormalNameId = 0;
            }
            else
            {
                userData.nameId = 0;
                LastNormalNameId = userData.nameId;
            }

            isSave = true;
        }

        if (userData.level <= 0)
        {
            userData.level = GameRes.GetLevel();
            isSave = true;
        }

        CheckActiveFrame();
        CheckActiveName();
        if (isSave)
        {
            SaveUserData();
        }
    }


    public bool CheckActiveFrame(int id = -99)
    {
        if (id == -99)
        {
            id = userData.frameId;
        }

        if (id >= frameSO.Data.Length || id < 0) return false;
        var frame = frameSO.Data[id];
        if (frame.type == FrameType.Infinity) return true;
        var data = userFrameData.data.Find(x => x.Item1 == id);
        if (data == default || data.Item2 <= MGTime.GetUtcTime())
        {
            ChangeFrame(LastNormalFrameId);
            return false;
        }

        return true;
    }

    /// <summary>
    /// expiredTime == 0 -> infinity
    /// </summary>
    /// <param name="id"></param>
    /// <param name="expiredTime"></param>
    public void ActiveFrame(int id = 0, long expiredTime = 0)
    {
        if (id >= frameSO.Data.Length || id < 0) return;
        var frame = frameSO.Data[id];
        if (frame.type != FrameType.TimeLimit) return;
        var idx = userFrameData.data.FindIndex(x => x.Item1 == id);
        if (idx < 0)
        {
            userFrameData.data.Add(new(id, expiredTime));
        }
        else
        {
            userFrameData.data[idx] = new(id, expiredTime);
        }

        if (frameSO.Data[userData.frameId].type == FrameType.Infinity) LastNormalFrameId = userData.frameId;
        userData.frameId = id;
        saveFrameData();
    }

    public int[] GetFrameIdOfType(int type = -1)
    {
        if (type == -1)
        {
            return (int[])Enumerable.Range(0, frameSO.Data.Length);
        }

        return frameSO.Data
            .Select((item, index) => new { item, index }) // gắn index
            .Where(x => x.item.type == (FrameType)type) // lọc theo enum
            .Select(x => x.index) // lấy index
            .ToArray();
    }

    public bool CheckActiveName(int id = -99)
    {
        if (id == -99)
        {
            id = userData.nameId;
        }

        if (id >= nameSO.Data.Length || id < 0) return false;
        var name = nameSO.Data[id];
        if (name.type == NameType.Infinity) return true;
        var data = userNameData.data.Find(x => x.Item1 == id);
        if (data == default || data.Item2 <= MGTime.GetUtcTime())
        {
            ChangeNameType(LastNormalNameId);
            return false;
        }

        return true;
    }

    public void ActiveName(int id = 0, long expiredTime = 0)
    {
        if (id >= nameSO.Data.Length || id < 0) return;
        var name = nameSO.Data[id];
        if (name.type != NameType.TimeLimit) return;
        var idx = userNameData.data.FindIndex(x => x.Item1 == id);
        if (idx < 0)
        {
            userNameData.data.Add(new ValueTuple<int, long>(id, expiredTime));
        }
        else
        {
            userNameData.data[idx] = new ValueTuple<int, long>(id, expiredTime);
        }

        if (nameSO.Data[userData.nameId].type == NameType.Infinity) LastNormalNameId = userData.nameId;
        userData.nameId = id;
        saveNameData();
    }

    public int[] GetNameIdOfType(int type = -1)
    {
        if (type == -1)
        {
            return (int[])Enumerable.Range(0, frameSO.Data.Length);
        }

        return nameSO.Data
            .Select((item, index) => new { item, index }) // gắn index
            .Where(x => x.item.type == (NameType)type) // lọc theo enum
            .Select(x => x.index) // lấy index
            .ToArray();
    }

    public Sprite GetAvatar(int id, string playerUUID = "")
    {
        if (id == 9999)
        {
            return avatarSO.defaultAvatar;
        }

        var avatarSprite = avatarSO.Data[Mathf.Clamp(id, 0, avatarSO.Data.Length - 1)];
        return avatarSprite;
    }

    private Dictionary<string, Sprite> cacheSpriteUserByUID = new();

    public class GroupAvatarUI
    {
        public int avatarId;
        public string avatarUrl;
        public int frameId;
        public int nameId;
        public string name;
    }

    public static Dictionary<string, GroupAvatarUI> loadedUserAvatarByUUID = new();
    
    public Sprite GetCacheAvatar(string playerUUID)
    {
        if (string.IsNullOrEmpty(playerUUID))
        {
            return null;
        }
        cacheSpriteUserByUID.TryGetValue(playerUUID, out var cachedSprite);
        return cachedSprite;
    }

    public IEnumerator GetAvatarUrl(string url, string playerUUID, Action<Sprite> action)
    {
        if (!string.IsNullOrEmpty(playerUUID) && cacheSpriteUserByUID.ContainsKey(playerUUID) && cacheSpriteUserByUID[playerUUID] != null)
        {
            action?.Invoke(cacheSpriteUserByUID[playerUUID]);
            yield break;
        }

        if (string.IsNullOrEmpty(url))
        {
            action?.Invoke(avatarSO.defaultAvatar);
            yield break;
        }

        if (url == userData.avatarUrl)
        {
            action?.Invoke(avatarSocialSprite);
            yield break;
        }

        Debug.Log($"requesting avatar {url}");
        using UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(url);
        yield return imageRequest.SendWebRequest();

        if (imageRequest.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(imageRequest);
            Sprite avatarSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            if(!string.IsNullOrEmpty(playerUUID)) cacheSpriteUserByUID[playerUUID] = avatarSprite;
            action?.Invoke(avatarSprite);
        }
        else
        {
            Debug.LogError("Error loading image: " + imageRequest.error);
            if (userData.avatarId == 9999)
            {
                action?.Invoke(avatarSO.defaultAvatar);
            }
            else
            {
                action?.Invoke(null);
            }
        }
    }

    public void SaveSpriteToLocal(Sprite sprite)
    {
        avatarSocialSprite = sprite;
        var fileName = FILE_AVATAR_FACEBOOK;
        Texture2D texture = sprite.texture;
        byte[] bytes = texture.EncodeToPNG();
        string path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(path, bytes);
    }

    public Sprite LoadSpriteFromLocal()
    {
        var fileName = FILE_AVATAR_FACEBOOK;
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        return null;
    }

    public GameObject GetFrame(int id)
    {
        return frameSO.Data[Mathf.Clamp(id, 0, frameSO.Data.Length - 1)].frame;
    }

    public Text GetNameText(int id)
    {
        return nameSO.Data[Mathf.Clamp(id, 0, nameSO.Data.Length - 1)].text;
    }

    public UserData GetDataUser()
    {
        //dataUser.CheckBaseInfor();
        return userData;
    }

    public void SavePlayerUUID(string playerUUID)
    {
        userData.playerUUID = playerUUID;
        SaveUserData();
    }

    public void SetGoogleId(string id)
    {
        userData.googleId = id;
        SaveUserData();
    }

    public void SetAppleId(string id)
    {
        userData.appleId = id;
        SaveUserData();
    }

    public void SetFacebookId(string id)
    {
        userData.facebookId = id;
        SaveUserData();
    }

    public void SetAccessToken(string access_token, bool isSave = true)
    {
        userData.access_token = access_token;
        if (isSave)
        {
            SaveUserData();
        }
    }

    public void SaveUserData()
    {
        userData.game_info = null;
        SaveLoadUtil.SaveData(userData, FILE_DATA_USER);
    }

    private void loadFrameData()
    {
        userFrameData = SaveLoadUtil.LoadData<UserFrameData>("user_frame_data.ddd", false, TypeSave.None);
        if (userFrameData == null)
        {
            userFrameData = new();
            userFrameData.data = new List<(int, long)>();
            saveFrameData();
        }
    }

    private void saveFrameData()
    {
        SaveLoadUtil.SaveData(userFrameData, "user_frame_data.ddd", false, TypeSave.None);
    }

    private void loadNameData()
    {
        userNameData = SaveLoadUtil.LoadData<UserNameData>("user_name_data.ddd", false, TypeSave.None);
        if (userNameData == null)
        {
            userNameData = new();
            userNameData.data = new();
            saveNameData();
        }
    }

    private void saveNameData()
    {
        SaveLoadUtil.SaveData(userNameData, "user_name_data.ddd", false, TypeSave.None);
    }

    public void SetAvatarUrl(string avatarUrl)
    {
        if (!string.IsNullOrEmpty(userData.facebookId)) return;
    }

    public void SetLevel(int level)
    {
        userData.level = level;
        //userData.game_info.level = level;
        SaveUserData();
    }

    public void ChangeUserName(string newName)
    {
        userData.name = newName;
        SaveUserData();
    }

    /// <summary>
    /// when change avatar to url, set id to 9999,
    /// if not it will set avatar according to your avatarId
    /// </summary>
    /// <param name="id"></param>
    /// <param name="url"></param>
    public void ChangeAvatar(int id, string url = "")
    {
        userData.avatarId = id;
        if (!string.IsNullOrEmpty(url)) userData.avatarUrl = url;
        SaveUserData();
    }

    public void ChangeFrame(int id)
    {
        if (id >= frameSO.Data.Length || id < 0) return;
        userData.frameId = id;
        if (frameSO.Data[id].type == FrameType.Infinity) LastNormalFrameId = id;
        SaveUserData();
    }

    public void ChangeNameType(int id)
    {
        if (id >= nameSO.Data.Length || id < 0) return;
        userData.nameId = id;
        if (nameSO.Data[id].type == NameType.Infinity) LastNormalNameId = id;
        SaveUserData();
    }

    public void SetDayInstall(int v)
    {
        userData.day_install = v;
    }

    public void AddDayLogin(int v)
    {
        userData.day_login += v;
        SaveUserData();
    }

    public void AddSectionLogin(int v)
    {
        userData.section_login += v;
        SaveUserData();
    }

    public void AddFirstTryWins(int v)
    {
        userData.firstTryWins += v;
        SaveUserData();
    }

    public void AddWinStreak(int v)
    {
        userData.continuousWin += v;
        SaveUserData();
    }

    public void ResetWinStreak()
    {
        userData.continuousWin = 0;
        SaveUserData();
    }

    public void AddTotalWins(int v)
    {
        userData.totalWins += v;
        SaveUserData();
    }

    public void AddTotalGames(int v)
    {
        userData.totalGames += v;
        SaveUserData();
    }

    public void AddLoginStreak(int v)
    {
        userData.continuousLogin += v;
        SaveUserData();
    }

    public void ResetLoginStreak()
    {
        userData.continuousLogin = 1;
        SaveUserData();
    }

    /*public void SetLastTimeLogin()
    {
        userData.game_info.playerData.lastTimeLogin = SdkUtil.CurrentTimeMilis();
        SaveUserData();
    }*/

    /*public void SetFirstTimeLogin()
    {
        userData.game_info.playerData.firstTimeJoinGame = SdkUtil.CurrentTimeMilis();
        SaveUserData();
    }*/
    public void SetUserId(string userId)
    {
        userData.userID = userId;
    }

    public string GetNewUserName()
    {
        var r = UnityEngine.Random.Range(0, 1f);
        if (r < 0.4f)
        {
            return $"User#{UnityEngine.Random.Range(0, 100000):0000}";
        }
        else
        {
            r = UnityEngine.Random.Range(0, 1f);
            return r < 0.5f
                ? $"{Name.namesUser[UnityEngine.Random.Range(0, Name.namesUser.Length)]}{UnityEngine.Random.Range(0, 100)}"
                : Name.namesUser[UnityEngine.Random.Range(0, Name.namesUser.Length)];
        }
    }

    public int GetMaxFrameCount()
    {
        return frameSO.Data.Count();
    }

    public int GetFrameID()
    {
        int ratio = UnityEngine.Random.Range(0, 100);
        if (ratio < 5)
        {
            int[] listID = GetFrameIdOfType((int)FrameType.TimeLimit);
            if (listID != null && listID.Length > 0)
            {
                return listID[UnityEngine.Random.Range(0, listID.Length)];
            }

            return 0;
        }
        else
        {
            int[] listID = GetFrameIdOfType((int)FrameType.Infinity);
            if (listID != null && listID.Length > 0)
            {
                return listID[UnityEngine.Random.Range(0, listID.Length)];
            }

            return 0;
        }
    }

    public void OnSyncData(UserData newData)
    {
        if (newData == null)
        {
            Debug.LogWarning("OnSyncData: newData is null");
            return;
        }

        var newUserData = new UserData();

        newUserData.playerUUID = newData.playerUUID;
        newUserData.userID = newData.userID;
        //newUserData.socialId = newData.socialId;
        newUserData.appleId = newData.appleId;
        newUserData.googleId = newData.googleId;
        newUserData.facebookId = newData.facebookId;
        newUserData.level = newData.level;
        newUserData.createdDate = newData.createdDate;
        //newUserData.guildId = newData.guildId;
        //newUserData.guildLogoId = newData.guildLogoId;
        //newUserData.guildName = newData.guildName;
        newUserData.countryName = newData.countryName;
        newUserData.countryCode = newData.countryCode;
        newUserData.positionGuild = newData.positionGuild;
        newUserData.firstTryWins = newData.firstTryWins;
        newUserData.helpMadeGuilds = newData.helpMadeGuilds;
        newUserData.helpMades = newData.helpMades;
        newUserData.helpReceives = newData.helpReceives;
        newUserData.areasCompleted = newData.areasCompleted;
        newUserData.helpsReceived = newData.helpsReceived;
        newUserData.continuousWin = newData.continuousWin;
        newUserData.totalWins = newData.totalWins;
        newUserData.totalGames = newData.totalGames;
        newUserData.continuousLogin = newData.continuousLogin;
        newUserData.avatarId = newData.avatarId;
        //newUserData.avatarBG = newData.avatarBG;
        newUserData.frameId = newData.frameId;
        newUserData.avatarUrl = newData.avatarUrl;
        newUserData.nameId = newData.nameId;
        newUserData.name = newData.name;
        //newUserData.addFriendAble = newData.addFriendAble;

        //newUserData.game_info.playerData.firstTimeJoinGame = newData.game_info.playerData.firstTimeJoinGame;
        //newUserData.game_info.playerData.lastTimeLogin = newData.game_info.playerData.lastTimeLogin;
        //newUserData.game_info.isFirstShowChangeName = newData.game_info.isFirstShowChangeName;
        if (newData.game_info != null)
        {
            FirstTimeJoinGame = newData.game_info.FirstTimeJoinGame;
            LastTimeLogin = newData.game_info.LastTimeLogin;
            IsFirstShowChangeName = newData.game_info.isFirstShowChangeName;
            LastNormalFrameId = newData.game_info.lastNormalFrameId;
            LastNormalNameId = newData.game_info.lastNormalNameId;
            userFrameData = newData.game_info.userFrameData;
            userNameData = newData.game_info.userNameData;
            saveFrameData();
            saveNameData();
        }
        else
        {
            IsFirstShowChangeName = true;
            FirstTimeJoinGame = 0;
            LastTimeLogin = 0;
            LastNormalFrameId = -1;
            LastNormalNameId = -1;
            userFrameData = new();
            userFrameData.data = new();
            userNameData = new();
            userNameData.data = new();
        }


        newUserData.day_install = newData.day_login;
        newUserData.day_login = newData.day_login;
        newUserData.section_login = newData.section_login;

        userData = newUserData;
        SaveUserData();
    }

    public void SendSyncData(UserData dataUser)
    {
        dataUser.name = userData.name;
        dataUser.playerUUID = userData.playerUUID;
        dataUser.userID = userData.userID;
        //dataUser.socialId = userData.socialId;
        //dataUser.appleId = userData.appleId;
        //dataUser.googleId = userData.googleId;
        //dataUser.facebookId = userData.facebookId;
        dataUser.level = userData.level;
        dataUser.createdDate = userData.createdDate;
        dataUser.guildId = userData.guildId;
        dataUser.guildLogoId = userData.guildLogoId;
        dataUser.guildName = userData.guildName;
        dataUser.countryName = userData.countryName;
        dataUser.countryCode = userData.countryCode;
        //dataUser.positionGuild = userData.positionGuild;
        dataUser.firstTryWins = userData.firstTryWins;
        dataUser.helpMadeGuilds = userData.helpMadeGuilds;
        dataUser.helpMades = userData.helpMades;
        dataUser.helpReceives = userData.helpReceives;
        dataUser.areasCompleted = userData.areasCompleted;
        dataUser.helpsReceived = userData.helpsReceived;
        dataUser.avatarId = userData.avatarId;
        //dataUser.avatarBG = userData.avatarBG;
        dataUser.frameId = userData.frameId;
        dataUser.avatarUrl = userData.avatarUrl;
        dataUser.nameId = userData.nameId;
        //dataUser.addFriendAble = userData.addFriendAble;
        //dataUser.game_info.playerData = new PlayerData();
        dataUser.game_info.FirstTimeJoinGame = FirstTimeJoinGame;
        dataUser.game_info.LastTimeLogin = LastTimeLogin;
        dataUser.game_info.isFirstShowChangeName = IsFirstShowChangeName;
        dataUser.game_info.lastNormalFrameId = LastNormalFrameId;
        dataUser.game_info.lastNormalNameId = LastNormalNameId;
        dataUser.game_info.userFrameData = userFrameData;
        dataUser.game_info.userNameData = userNameData;
    }
}
