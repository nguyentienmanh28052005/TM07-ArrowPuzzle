using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using master;
using mygame.sdk;
using Newtonsoft.Json;
using time;
using Unity.Notifications;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.String;

public enum GameState
{
    None,
    LoadLevel,
    Playing,
    Pause,
    WaitRevive,
    Complete,
    Defeat,
}

public enum GameMode
{
    Level
}

public enum ReasonBackToHome
{
    NewDay,
    OpenGame,
    Win,
    Lose,
    Exit
}

public enum LevelType
{
    Easy = 0,
    Hard = 1,
    Crazy = 2
}

public enum LevelDifficulty
{
    Beginner = 0,
    Easy = 1,
    Casual = 2,
    Moderate = 3,
    Standard = 4,
    Hard = 5,
    Expert = 6
}

public class GameManager : master.Singleton<GameManager>
{
    public static GameState GameState { get; set; }
    public static EGameMode CurrentMode { get; set; }
    public static string CurrentPlayType { get; set; }
    public static int CurrentLevel { get; set; }
    
    public static ReasonBackToHome ReasonBackToHome { get; set; }

    private float startTime;
    private bool isForceShowAllowTracking = false;
    public static int IsAllowTracking => PlayerPrefs.GetInt("user_allow_track_ads", 1);
    public bool isLoading { get; set; }

    public static event Action OnNewDayEvent;
    public SpriteResourceManager SpriteResourceSO;

    private static long lastDayLogin
    {
        get => long.Parse(PlayerPrefs.GetString("game_last_day_login_cache", "0"));
        set => PlayerPrefs.SetString("game_last_day_login_cache", value.ToString());
    }

    protected override void Awake()
    {
        base.Awake();
        SaveLoadUtil.Clear();
        Application.targetFrameRate = 60;
        AppsFlyerHelperScript.OnConversionDataDone += OnConversionDataSuccess;
    }

    protected override void OnDestroy()
    {
        AppsFlyerHelperScript.OnConversionDataDone -= OnConversionDataSuccess;
        SDKManager.CBFinishShowLanAge -= Initialized;
        DOTween.Kill(this);
        RequestTracker.StopChecking();
    }


    void OnConversionDataSuccess(bool isSuc, string conversionData)
    {
        if (isSuc)
        {
        }
    }

    private void checkVersionData()
    {
        return;
        var versionData = PlayerPrefs.GetInt("version_data", 0);
        if (versionData == 0)
        {
            try
            {
                var dataGame = Keychain.GetString("data_game", "");
                if (!IsNullOrEmpty(dataGame))
                {
                    Caching.ClearCache();
                    var newData = JsonConvert.DeserializeObject<UserData>(dataGame);
                    SocialManager.Instance.ReceiveUserDataSync(newData);
                    isForceShowAllowTracking = true;
                }
            }
            catch (Exception e)
            {
                FIRhelper.logEvent("key_chain_err");
            }
        }

        PlayerPrefs.SetInt("version_data", 1);
    }

    public static int CountLoad = 0;
    private static bool isSyncData = false;

    private void Start()
    {
        // AutoQuality.Instance.Setup(true);
        CurrentLevel = DataManager.Level;
        AdsHelper.Instance.getCfAdsPlacement("ui_fail", 0);
        checkVersionData();
        CheckNewDayRunTime();
        if (CountLoad > 0)
        {
            isSyncData = true;
        }

        IAPManager.Instance.ActiveAllPack();
        SocialManager.Instance.LoginSocial(null);
        ServerHub.Instance.ConnectServer();
        LogEvent.LoadingStart("open_game");
        NotificationHandler.ClearDisplayNotifications();
        startTime = MGTime.GetUtcTime();
        isLoading = true;
        StartCoroutine(LoadGame());
        ResgisterIAPCallback();
        if (UserDataManager.FirstTimeJoinGame == 0)
        {
            FreeGiftFirstTime();
            PlayerPrefsUtil.AudioVibrateSetting = PlayerPrefsUtil.CF_DefaultVibrate;
            PlayerPrefsUtil.HintClickObjectSetting = PlayerPrefsUtil.CF_DefaultHintClickObject;
        }

        if (AppConfig.FlagShowSplashLoading == 1)
        {
            SDKManager.CBFinishShowLanAge += Initialized;
        }
        else
        {
            //UIManager.Instance.ShowScreen<MainMenuScreen>();
            Initialized(0);
        }
        /*var a = new List<(string, string)>();
    a.Add(("1", "1"));
    a.Add(("2", "2"));
    Debug.Log($"Check aaaa = {JsonConvert.SerializeObject(a)}");
    var txt = Resources.Load<TextAsset>("DataLevel/ConfigLevelOrder");
    var b = new DataLevelConfig();
    b.Version = 1;
    b.LevelOrderConfig = txt.text;
    Debug.Log($"Check aaaa1 = {JsonConvert.SerializeObject(b)}");*/
    }


#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeLanguage(true);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeLanguage(false);
        }
    }

    private void ChangeLanguage(bool isNext)
    {
        // Lấy ngôn ngữ đang set
        string lset = PlayerPrefs.GetString("mem_set_lang", "");
        if (string.IsNullOrEmpty(lset))
        {
            lset = MutilLanguage.Instance().languageCode4mutil;
        }

        // Tìm index hiện tại
        int index = PopupLanguage.allLangs.FindIndex(x => x.langCode == lset);
        if (index < 0)
        {
            // fallback an toàn: quay về 0
            index = 0;
        }

        int valueAdd = isNext ? 1 : -1;
        int nextIndex = index + valueAdd;

        if (nextIndex >= PopupLanguage.allLangs.Count)
        {
            nextIndex = 0;
        }
        else if (nextIndex < 0)
        {
            nextIndex = PopupLanguage.allLangs.Count - 1;
        }

        MutilLanguage.Instance().setLang(PopupLanguage.allLangs[nextIndex].langCode);
    }
#endif

    private IEnumerator LoadGame()
    {
        yield return new WaitForEndOfFrame();
        var spl = SDKManager.Instance.splashLoadingCtr;
        while (!LevelRemoteManager.Instance.initialized)
        {
            if (spl.timeRun > spl.toTimeWait / 2.25f)
            {
                spl.isPause = true;
            }
            yield return new WaitForEndOfFrame();
        }
        spl.isPause = false;
        LevelRemoteManager.Instance.LoadLevelCache(GameRes.GetLevel());
        
        if (!IsPlayingFirst())
        {
            GameController.Preload();
            if (spl.timeRun > spl.toTimeWait / 2.5f)
            {
                UIManager.Instance.ShowScreen<MainMenuScreen>();
            }
            else
            {
                DOVirtual.DelayedCall(Mathf.Max(spl.toTimeWait / 2.35f - spl.timeRun, 0.1f), () =>
                {
                    UIManager.Instance.ShowScreen<MainMenuScreen>();
                }, false);
            }
        }
    }

    private bool IsPlayingFirst()
    {
        if (UserDataManager.FirstTimeJoinGame == 0)
        {
            return true;
        }
        else
        {
            if (isSyncData) return false;
            if ((GameRes.GetLevel() < PlayerPrefsUtil.CF_FirstLevelShowMain || UserDataManager.Instance.UserData.section_login < PlayerPrefsUtil.CF_SectionShowMain) && PlayerPrefsUtil.CF_PlayGameIfNotFirstOpen)
            {
                return true;
            }
            if (!PlayerPrefsUtil.CF_PlayGameIfHasSave) return false;
            var lvl = GameRes.GetLevel();
            if (LevelManager.Instance.IsHasSaveGameData(lvl))
            {
                return true;
            }
        }
        return false;
    }

    private void Initialized(int flag)
    {
        ReasonBackToHome = ReasonBackToHome.OpenGame;
        GameState = GameState.None;
        UserDataManager.Instance.AddSectionLogin(1);

        if (UserDataManager.FirstTimeJoinGame == 0)
        {
            UserDataManager.FirstTimeJoinGame = MGTime.GetUtcTime();
            //FreeGiftFirstTime();
#if UNITY_IOS
            if (IsAllowTracking == 0)
            {
                isForceShowAllowTracking = false;
                UIManager.Instance.ShowPopup<UIAllowTracking>(FirstJoinGame);
            }
            else
            {
                FirstJoinGame();
            }

#else
            FirstJoinGame();
#endif
            //LogEventManager.Instance.FirstOpen();
            return;

            void FirstJoinGame()
            {
                string playType = "play";
                GameManager.CurrentPlayType = playType;
                PlayGame(true);
                int levelCurrent = GameRes.GetLevel();
                int levelUse = LevelManager.Instance.levelOder;
                if (DataManager.LevelCacheStart <= 0 || DataManager.LevelCacheStart < levelCurrent)
                {
                    DataManager.LevelCacheStart = levelCurrent;
                    LogEventCustom.Instance.LogLevelStart(levelCurrent, levelUse, (int)GameMode.Level, playType);
                }

                //FIRhelper.logEvent("play_first_open");
            }
        }

        if (IsNewDay(UserDataManager.LastTimeLogin))
        {
            UserDataManager.Instance.AddDayLogin(1);
            var today = SdkUtil.timeStamp2DateTime(MGTime.GetUtcTime());
            var lastDay = SdkUtil.timeStamp2DateTime(UserDataManager.LastTimeLogin);
            if (IsNextDay(lastDay, today))
            {
                UserDataManager.Instance.AddLoginStreak(1);
            }
            else
            {
                UserDataManager.Instance.ResetLoginStreak();
            }


            bool IsNextDay(DateTime lastDay, DateTime today)
            {
                // Remove time component
                DateTime lastDate = lastDay.Date;
                DateTime todayDate = today.Date;

                // Check if today is exactly 1 day after lastDay
                return todayDate == lastDate.AddDays(1);
            }
        }

        UserDataManager.LastTimeLogin = MGTime.GetUtcTime();


        string playType = "play";
        GameManager.CurrentPlayType = playType;
        PlayGameOrShowMain();
        int levelCurrent = GameRes.GetLevel();
        int levelUse = LevelManager.Instance.levelOder;
        if (DataManager.LevelCacheStart <= 0 || DataManager.LevelCacheStart < levelCurrent)
        {
            DataManager.LevelCacheStart = levelCurrent;
            LogEventCustom.Instance.LogLevelStart(levelCurrent, levelUse, (int)GameMode.Level, playType);
        }


        void PlayGameOrShowMain()
        {
            if ((GameRes.GetLevel() < PlayerPrefsUtil.CF_FirstLevelShowMain || UserDataManager.Instance.UserData.section_login < PlayerPrefsUtil.CF_SectionShowMain) && PlayerPrefsUtil.CF_PlayGameIfNotFirstOpen)
            {
                if (GameRes.GetLevel() == 1)
                {
                    DataManager.Instance.isFreeTicketIfFail = true;
                }

                PlayGame(true);
                // if (CountLoad == 0)
                // {
                //     LogEvent.LevelPlay(GameRes.GetLevel(), DataManager.Level, "first_open",
                //         DataManager.Instance.ConsecutiveLose, GameMode.Level);
                //     FIRhelper.logEvent("play_first_open");
                // }

                //ShowBanner();
            }
            else
            {
                if (!isSyncData)
                {
                    if (PlayerPrefsUtil.CF_PlayGameIfHasSave)
                    {
                        var lvl = GameRes.GetLevel();


                        if (UIManager.Instance.CurrentScreen == null)
                        {
                            UIManager.Instance.ShowScreen<MainMenuScreen>();
                            GameController.OnBackToMenu(0);
                        }
                        else
                        {
                            GameController.OnBackToMenu(0);
                        }

                        var mainMenuScreen = UIManager.Instance.GetScreenActive<MainMenuScreen>();
                        mainMenuScreen.PlayMusic();
                    }
                    else
                    {
                        GameController.OnBackToMenu(0);
                        var mainMenuScreen = UIManager.Instance.GetScreenActive<MainMenuScreen>();
                        mainMenuScreen.PlayMusic();
                    }
                }
            }
        }
    }

    public void ResgisterIAPCallback()
    {
        InappHelper.OnHandlePurchasePending += OnPurchaseSuccess;
    }

    private void OnPurchaseSuccess(string skuID)
    {
        Debug.Log($"IAP: Auto Process sku:{skuID}");

        var packageData = DataManager.Instance.packageData;
        var package =
            (packageData.packageInfos.SingleOrDefault(x => x.skuId == skuID) ??
             packageData.goldPackInfos.SingleOrDefault(x => x.skuId == skuID)) ??
            packageData.ticketPackInfos.SingleOrDefault(x => x.skuId == skuID);
        if (package != null)
        {
            DataManager.Instance.ReceiveGift(false, .75f, "auto_process", LogEvent.ReasonItem.purchase, true, -1,
                package.allItems);
        }
        else
        {
            var salePack = packageData.packageFlashSaleInfos.SingleOrDefault(x => x.skuId == skuID);

            if (salePack != null)
            {
                DataManager.Instance.ReceiveGift(false, .75f, "auto_process", LogEvent.ReasonItem.purchase, true,
                    -1,
                    salePack.itemInfos);
            }
        }
    }

    public void HideBanner()
    {
        AdsHelper.Instance.hideBanner(0);
    }


    public void PlayGame(bool isForce = false)
    {
        AudioManager.Instance.StopMusic();
        var level = DataManager.Level;
        CurrentLevel = level;
        CurrentMode = EGameMode.Level;
        GameState = GameState.Playing;
        GameController.OnStartLevel();
        GameEvent.OnStartLevel?.Invoke(CurrentLevel, 0, CurrentMode);
        if (isForce)
        {
            AdsHelperWrapper.SetBannerShowState(true, "enter_game");
        }

        LevelManager.Instance.LoadLevel(isForce);

        var lvUp = level + 1;
        if (SDKManager.Instance.counSessionGame >= AdsHelper.Instance.currConfig.fullSessionStart && lvUp >= AdsHelper.Instance.currConfig.fullLevelStart)
        {
            AdsHelper.Instance.loadFull4ThisTurn("open_game", false, level, 0);
        }

        var config = AdsRewardConfig.GetConfig();
        var isShowAdsWin = (config.levelRemoveAdsWinUI == 0 || config.levelRemoveAdsWinUI > lvUp) && lvUp > config.levelActiveAdsWinUI;
        var isShowAdsRevive = (config.levelRemoveAdsRevive == 0 || config.levelRemoveAdsRevive > lvUp) && lvUp > config.levelActiveAdsRevive;
        var isShowAdsBuyBooster = (config.levelRemoveAdsBuyBooster == 0 || config.levelRemoveAdsBuyBooster > lvUp) && lvUp > config.levelActiveAdsBuyBooster;
        var isShowAdsBuyHeart = (config.levelRemoveAdsBuyHeart == 0 || config.levelRemoveAdsBuyHeart > lvUp) && lvUp > config.levelActiveAdsBuyHeart;

        if (isShowAdsWin || isShowAdsRevive || isShowAdsBuyBooster || isShowAdsBuyHeart)
        {
            AdsHelper.Instance.loadGift4ThisTurn("open_game", level, state => { });
        }
        DoubleRewardManager.IsActive = DoubleRewardManager.isActiveTime;
    }

    public void FreeGiftFirstTime()
    {
        if (GameRes.getRes(RES_type.GOLD) < PlayerPrefsUtil.CFGoldDefault)
        {
            //BoosterManager.Instance.AddBooster(BoosterType.AddHole, 1, "gift_first_time", LogEvent.ReasonItem.reward);
            //BoosterManager.Instance.AddBooster(BoosterType.ClearHole, 1, "gift_first_time", LogEvent.ReasonItem.reward);
            //BoosterManager.Instance.AddBooster(BoosterType.BreakObject, 1, "gift_first_time", LogEvent.ReasonItem.reward);
            //BoosterManager.Instance.AddBooster(BoosterType.MutilColorBox, 1, "gift_first_time", LogEvent.ReasonItem.reward);
            //BoosterManager.Instance.AddBooster(BoosterType.Magnet, 1, "gift_first_time", LogEvent.ReasonItem.reward);
            DataManager.Instance.ReceiveGift(false, 0, "gift_first_time", LogEvent.ReasonItem.reward, true,
                DataManager.Level, new ItemInfo(null, RES_type.GOLD, PlayerPrefsUtil.CFGoldDefault));
            //GameEvent.OnReceiveResource?.Invoke(LogEvent.ReasonItem.reward, "gift_first_time", new []{ new DataResource(RES_type.GOLD, 1000)}, DataManager.Level);
        }
    }

    public void SwipePanelGuild()
    {
        MainMenuScreen mainMenu = UIManager.Instance.GetScreenActive<MainMenuScreen>();
        if (mainMenu != null)
        {
            mainMenu.ActivePanel(3);
        }
    }

    public void NextLevel()
    {
        GameManager.CurrentPlayType = "next_level";
        PlayGame();
    }

    public void PauseGame()
    {
        GameState = GameState.Pause;
    }

    public void RestartGame()
    {
        var level = GameRes.GetLevel();
        CurrentLevel = level;
        CurrentMode = EGameMode.Level;
        PlayGame();
        // LogEvent.LevelPlay(GameRes.GetLevel(), DataLevelExtractor.CurrentDataLevelOrder.LevelUse, "retry",
        //     DataManager.Instance.ConsecutiveLose, GameMode.Level);
    }

    public void ResumeGame()
    {
        GameState = GameState.Playing;
    }

    public void BackToHome(ReasonBackToHome reasonBackToHome)
    {
        AdsHelperWrapper.SetBannerShowState(false, "back_home");
        ActionOnBackHome(reasonBackToHome);
        GameController.OnBackToMenu(1);
    }

    public void ActionOnBackHome(ReasonBackToHome reasonBackToHome)
    {
        ReasonBackToHome = reasonBackToHome;
        LevelManager.Instance.UnloadGame();
        GameState = GameState.None;
        UIManager.Instance.ShowScreen<MainMenuScreen>();
        if (UILevelComplete.cacheCoinGet <= 0) return;
        var resourceTarget = RewardReceivedHub.GetResourceTarget(RES_type.GOLD);
        if (resourceTarget is ItemDisplay display)
        {
            display.SetText(GameRes.getRes(RES_type.GOLD) - UILevelComplete.cacheCoinGet);
        }
    }

    public void TransitionBackToHome(ReasonBackToHome reasonBackToHome, Action cbOnLoad)
    {
        AdsHelperWrapper.SetBannerShowState(false, "back_home");
        GameController.Preload();
        UIManager.Instance.Transition(true, () =>
        {
            cbOnLoad?.Invoke();
            ActionOnBackHome(reasonBackToHome);
        }, null, () => { GameController.OnBackToMenu(1); }, true, false);
    }

    public static string GetUserSegment(IList<string> listKey)
    {
        foreach (var item in listKey)
        {
            if (SDKManager.Instance.mediaCampain == null) continue;
            var arrik = item.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in arrik)
            {
                var arrPks = t.ToLower().Split('_');
                if (arrPks is not { Length: > 0 }) continue;
                var camName = arrPks[0];
                if (arrPks.Length > 1 && Compare(arrPks[1], "any", StringComparison.Ordinal) != 0)
                {
                    camName += $"_{arrPks[1]}";
                }

                if (!SDKManager.Instance.mediaCampain.StartsWith(camName + "_") &&
                    Compare(camName, "cany", StringComparison.Ordinal) != 0) continue;
                if (arrPks.Length > 2)
                {
                    if (AppsFlyerHelperScript.Instance.checkSameSource(arrPks[2]))
                    {
                        return item;
                    }
                }
                else
                {
                    return item;
                }
            }
        }

        return "default";
    }

    public static bool IsNewDay(long timeLogin)
    {
        var d1 = TicksToDateTime(TimestampToTicks(timeLogin));
        var d2 = TicksToDateTime(TimestampToTicks(GameHelper.CurrentTimeMilisReal()));
        return d2.Date > d1.Date;

        static long TimestampToTicks(long timstamp)
        {
            return (timstamp * 10000 + 621355968000000000);
        }

        static DateTime TicksToDateTime(long ticks)
        {
            return new DateTime(ticks);
        }
    }

    private void CheckNewDay()
    {
        if (!IsNewDay(lastDayLogin)) return;
        lastDayLogin = GameHelper.CurrentTimeMilisReal();
        OnNewDayEvent?.Invoke();
        OnNewDay();
    }

    private Coroutine checkDayRunTime;

    private void CheckNewDayRunTime()
    {
        CheckNewDay();
        StopCheckNewDayRunTime();
        StartCheckNewDayRunTime();
    }

    private void StopCheckNewDayRunTime()
    {
        if (checkDayRunTime != null) StopCoroutine(checkDayRunTime);
    }

    private void StartCheckNewDayRunTime()
    {
        checkDayRunTime = StartCoroutine(IECheckNewDayRunTime());
    }

    private IEnumerator IECheckNewDayRunTime()
    {
        // while (true)
        // {
        yield return new WaitForSeconds(10);
        CheckNewDay();
        // }
    }

    private void OnNewDay()
    {
        HeartManager.CountWatchAdsRefillHeartEachDay = 0;
    }
}