using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using DG.Tweening;
using Google.Protobuf;
using UnityEngine;
using master;
using mygame.sdk;
using Newtonsoft.Json;
using UnityEngine.Serialization;


public class LevelManager : Singleton<LevelManager>
{
    public bool initialized;
    public static Dictionary<BoosterType, int> DictBoosterUsed = new Dictionary<BoosterType, int>();

    public float playDuration => Time.time - timeStartLevel;
    public float timeStartLevel { get; private set; }
    private int countCheckLose = 0;

    public static Action UnLoadLevelAction;

    public static int GoldSpendInLevel
    {
        get => PlayerPrefs.GetInt("gold_spend_in_level", 0);
        set => PlayerPrefs.SetInt("gold_spend_in_level", value);
    }

    public static int LastDeath
    {
        get => PlayerPrefs.GetInt("last_death_in_level", 0);
        set => PlayerPrefs.SetInt("last_death_in_level", value);
    }

    public static int Count_Revive
    {
        get => PlayerPrefs.GetInt("count_revive", 0);
        set => PlayerPrefs.SetInt("count_revive", value);
    }

    /*
     -1: disable
     0: enable every level loading
     1: enable Home play game only
     */
    public static int CF_ShowIntroLevel
    {
        get => PlayerPrefs.GetInt("cf_show_intro_level", 1);
        set => PlayerPrefs.SetInt("cf_show_intro_level", value);
    }
    
    public static int CF_TimeHintInLevel
    {
        get => PlayerPrefs.GetInt("cf_time_hint_in_level", 3);
        set => PlayerPrefs.SetInt("cf_time_hint_in_level", value);
    }


    public int levelOder => LevelRemoteManager.Instance.levelInfo.levelID;

    public void LoadLevel(bool isForce = false)
    {
        UIManager.Instance.blockerUI.SetActive(true);
        Debug.Log($"LoadLevel({isForce})");
        UIRevive.Count_Revive_Ads = 0;
        initialized = false;
        Count_Revive = 0;
        GameManager.GameState = GameState.LoadLevel;
        var curLevel = DataManager.Level;
        if (LevelRemoteManager.Instance.status == LevelRemoteManager.LoadingStatus.None)
            LevelRemoteManager.Instance.LoadLevelCache(curLevel);
        UnloadGame();
        var uiInGame = UIManager.Instance.GetScreenActive<UIInGame>();

        var isLoaded = LevelRemoteManager.Instance.LoadCachedObject<UnityEngine.Object>() != null;
        if (!isLoaded || /*uiInGame == null &&*/ !isForce)
        {
            var transitionUI = UIManager.Instance.Transition(false, Load, LoadComplete, null, true, true);

            transitionUI.FadeIn(() =>
            {
                if (uiInGame == null) uiInGame = UIManager.Instance.ShowScreen<UIInGame>();
                else uiInGame.Active();
                uiInGame.Initialized(curLevel, GetLevelType(curLevel));
                StartCoroutine(LevelRemoteManager.Instance.WaitLoadLevel(transitionUI));
            });
        }
        else
        {
            if (uiInGame == null)
                uiInGame = UIManager.Instance.ShowScreen<UIInGame>();
            else
                uiInGame.Active();
            uiInGame.Initialized(curLevel, GetLevelType(curLevel));
            Load();
            LoadComplete();
        }

        void Load()
        {
        }


        void LoadComplete()
        {
            AdsHelperWrapper.SetBannerShowState(true, "enter_game");
            AudioManager.Instance.PlayBGMusicInGame();
            GameManager.GameState = GameState.Playing;
            timeStartLevel = Time.time;
            UserDataManager.Instance.AddTotalGames(1);
            DataManager.Instance.ConsecutivePlay++;
            initialized = true;
            Initialized();

            DOVirtual.DelayedCall(0.1f, () => { UIManager.Instance.blockerUI.SetActive(false); }, false)
                .SetUpdate(false).SetId(this);

            DOVirtual.DelayedCall(.6f, () =>
            {
                if (uiInGame == null) uiInGame = UIManager.Instance.GetScreenActive<UIInGame>();
                if (uiInGame == null) return;
                uiInGame.IntroLevel(0);
                uiInGame.PlayHardLevelFx(GetLevelType(GameManager.CurrentLevel));
            }, false).SetUpdate(false).SetId(this);
        }
    }


    public void Initialized()
    {
        DictBoosterUsed.Clear();
        GameManager.GameState = GameState.Playing;
        LogFireBaseCustomer.LogLevelPlay(GameManager.CurrentLevel);

        GetLevelAnalyticsData(out int totalBus, out _, out int baseSlot, out _);

        LogEvent.LevelPlay(GameRes.GetLevel(), LevelManager.Instance.levelOder, GameManager.CurrentPlayType, DataManager.Instance.ConsecutiveLose, GameMode.Level, totalBus, baseSlot);

        countCheckLose = 0;
    }

    private void GetBoosterStrings(out string qtyString, out string nameString)
    {
        if (DictBoosterUsed == null || DictBoosterUsed.Count == 0)
        {
            qtyString = "0";
            nameString = "default";
            return;
        }

        List<string> qList = new List<string>();
        List<string> nList = new List<string>();
        foreach (var kvp in DictBoosterUsed)
        {
            if (kvp.Value > 0)
            {
                qList.Add(kvp.Value.ToString());
                nList.Add(LogEvent.GetBoosterName(kvp.Key));
            }
        }

        if (qList.Count == 0)
        {
            qtyString = "0";
            nameString = "default";
        }
        else
        {
            qtyString = string.Join(",", qList);
            nameString = string.Join(",", nList);
        }
    }

    public void GetLevelAnalyticsData(out int totalBus, out int completedBus, out int baseSlot, out int finalSlot)
    {
        totalBus = 0;
        completedBus = 0;
        baseSlot = 0;
        finalSlot = 0;

    }

    public void OnReviveSuccess(ReviveType reviveType)
    {
        AdsHelperWrapper.SetBannerShowState(true, "enter_game");
        LogFireBaseCustomer.LogRevive(GameManager.CurrentLevel, isReviveByGold: reviveType == 0,
            isReviveByAds: reviveType == ReviveType.None);
        Count_Revive++;

        var rvType = "space";
        GameManager.GameState = GameState.Playing;
        TimeSpan ts = TimeSpan.FromSeconds(Instance.playDuration);
        long msFromTs = (long)ts.TotalMilliseconds;
        LogEvent.LevelSecondChance
        (
            playIndex: DataManager.Instance.ConsecutiveLose,
            reviveType: rvType,
            levelProgress: GetProgress(),
            durationTotal: msFromTs
        );
    }

    public void OnComplete()
    {
        if (GameManager.GameState == GameState.Playing || GameManager.GameState == GameState.Pause)
        {
            LogFireBaseCustomer.LogLevelEnd(GameManager.CurrentLevel, isLevelWin: true);
            GameManager.GameState = GameState.Complete;

            TimeSpan ts = TimeSpan.FromSeconds(playDuration);
            long msFromTs = (long)ts.TotalMilliseconds;
            
            GetLevelAnalyticsData(out int totalBus, out int completedBus, out int baseSlot, out int finalSlot);
            GetBoosterStrings(out string bQty, out string bName);

            LogEvent.LevelEnd
            (
                lv: GameRes.GetLevel(),
                levelId: levelOder,
                playTime: msFromTs,
                playIndex: DataManager.Instance.ConsecutivePlay - 1,
                gameMode: GameMode.Level,
                levelProgress: GetProgress(),
                result: LogEvent.LevelResult.win,
                totalBus: totalBus,
                completedBus: completedBus,
                reason: "default",
                useBoosterQty: bQty,
                useBoosterName: bName,
                baseSlot: baseSlot,
                finalSlot: finalSlot,
                levelProgressDetail: PlayProgress()
            );

            AudioManager.Instance.StopMusic();
            UIManager.Instance.blockerUI.SetActive(true);

            if (DataManager.Instance.ConsecutivePlay == 1)
            {
                UserDataManager.Instance.AddFirstTryWins(1);
                UserDataManager.Instance.AddWinStreak(1);
            }

            UserDataManager.Instance.AddTotalWins(1);
            DataManager.Instance.AdsOrDeathInLevel = 0;
            DataManager.Instance.ConsecutivePlay = 0;
            DataManager.Instance.ConsecutiveWin++;
            HeartManager.Instance.AddHeart(1, "complete_game", LogEvent.ReasonItem.reward);
            int level = GameRes.GetLevel();
            var levelType = GetLevelType(level);
            AppsFlyerHelperScript.logLevelAchieve(level);
            LevelChestManager.Instance.SetLevelChestQueue(level);
            GameRes.IncreaseLevel();
            GameEvent.OnIncreaseLevel?.Invoke();
            //get gold here
            var reward = new[] { new DataResource(RES_type.GOLD, ConfigManager.Instance.GetGoldCompleteLevel(levelType)) };
            var goldClaim = reward.FirstOrDefault(x => x.resType == RES_type.GOLD)!.amount;
            if (UILevelComplete.cacheCoinGet > 0)
            {
                RewardReceivedHub.AddCacheValue(RES_type.GOLD, -UILevelComplete.cacheCoinGet);
            }

            UILevelComplete.cacheCoinGet = goldClaim;
            RewardReceivedHub.AddCacheValue(RES_type.GOLD, goldClaim);
            GameEvent.OnReceiveResource?.Invoke(LogEvent.ReasonItem.reward, "complete_level", reward, level);
            GameEvent.OnFinishLevel?.Invoke(level, true, (int)levelType, EGameMode.Level);
            if (GameRes.GetLevel() == PlayerPrefsUtil.CF_LevelDisableMusic)
            {
                AudioManager.AudioMusicSetting = false;
            }

            DataManager.Instance.ConsecutiveLose = 0;
            PlayerPrefsUtil.LastIndexFailShowAds = 0;
            DataManager.isLose = false;

            UIManager.Instance.HideAllPopup();
            if (PlayerPrefsUtil.CF_ShowAdsComplete == 0)
            {
                AudioManager.Instance.SetCacheAudio();
            }

            // if (uiInGame != null)
            // {
            //     uiInGame.ShowAnimationOutro();
            // }
            UIManager.Instance.blockerUI.SetActive(false);
            UIManager.Instance.GetScreen<UIInGame>().PlayCompleteFx(() =>
            {
                if (PlayerPrefsUtil.CF_ShowAdsComplete == 0)
                {
                    AudioManager.Instance.SetCacheAudio();
                    AdsHelperWrapper.ShowFull("ui_complete_bf", _ => { },
                        onPopupBreakAdsClose: () =>
                        {
                            UIManager.Instance.GetScreen<UIInGame>().HideCompleteFx();
                            UIManager.Instance.ShowPopup<UILevelComplete>(null).Initialized(reward);
                            AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Finish_Level, 0.9f);

                        });
                }
                else
                {
                    AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Finish_Level, 0.9f);
                    UIManager.Instance.ShowPopup<UILevelComplete>(null).Initialized(reward);
                    UIManager.Instance.GetScreen<UIInGame>().HideCompleteFx();
                }
            });

            UserDataManager.Instance.SetLevel(GameRes.GetLevel());
            LevelRemoteManager.Instance.DownloadBundle();
            LevelRemoteManager.Instance.ClearOldLevelCache();
            LevelRemoteManager.Instance.LoadLevelCache(DataManager.Level);
        }
    }

    public void OnFail()
    {
        if (GameManager.GameState == GameState.Defeat) return;
        LogFireBaseCustomer.LogLevelEnd(GameManager.CurrentLevel, isLevelWin: false);
        GameManager.GameState = GameState.Defeat;
        AudioManager.Instance.StopMusic();
        UserDataManager.Instance.ResetWinStreak();
        DataManager.Instance.ConsecutiveLose++;
        DataManager.isLose = true;
        DataManager.Instance.ConsecutiveWin = 0;

        TimeSpan ts = TimeSpan.FromSeconds(playDuration);
        long msFromTs = (long)ts.TotalMilliseconds;
        
        GetLevelAnalyticsData(out int totalBus, out int completedBus, out int baseSlot, out int finalSlot);
        GetBoosterStrings(out string bQty, out string bName);

        LogEvent.LevelEnd(
            lv: GameRes.GetLevel(),
            levelId: levelOder,
            playTime: msFromTs,
            playIndex: DataManager.Instance.ConsecutivePlay - 1,
            gameMode: GameMode.Level,
            levelProgress: GetProgress(),
            result: LogEvent.LevelResult.lose,
            totalBus: totalBus,
            completedBus: completedBus,
            reason: "lose_by_step",
            useBoosterQty: bQty,
            useBoosterName: bName,
            baseSlot: baseSlot,
            finalSlot: finalSlot,
            levelProgressDetail: PlayProgress()
        );

        if (PlayerPrefsUtil.CF_ResetSpendIfLose) GoldSpendInLevel = 0;
        int level = GameRes.GetLevel();
        UIManager.Instance.HideAllPopup();
        GameEvent.OnFinishLevel?.Invoke(level, false, (int)GetLevelType(level), EGameMode.Level);
        if (level < PlayerPrefsUtil.CFLevelShowPlayPopup)
        {
            // FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_lose_home");
            LogEvent.ScreenGo(LogEvent.ScreenName.LevelLose, LogEvent.ButtonName.ButtonCloseRevive);
            UIManager.Instance.ShowPopup<UITryAgain>(null);
        }
        else
        {
            UIManager.Instance.ShowPopup<PopupReplay>(null);
        }
    }

    public void UnloadGame()
    {
        if (GameManager.GameState == GameState.Playing || GameManager.GameState == GameState.Pause)
        {
            TimeSpan ts = TimeSpan.FromSeconds(playDuration);
            long msFromTs = (long)ts.TotalMilliseconds;

            int totalBus = 0;
            int completedBus = 0;
            GetBoosterStrings(out string bQty, out string bName);

            // LogEvent.LevelExit(
            //     lv: GameRes.GetLevel(),
            //     levelId: levelOder,
            //     playTime: msFromTs,
            //     playIndex: DataManager.Instance.ConsecutivePlay - 1,
            //     gameMode: GameMode.Level,
            //     levelProgress: GetProgress(),
            //     result: LogEvent.LevelResult.exit,
            //     totalBus: totalBus,
            //     completedBus: completedBus,
            //     reason: "back_to_menu",
            //     useBoosterQty: bQty,
            //     useBoosterName: bName,
            //     levelProgressDetail: levelController.GetListIdActiveVehicle()
            // );
        }

        DictBoosterUsed.Clear();
        UnLoadLevelAction?.Invoke();
    }

    public int GetProgress()
    {
        return 0;
    }

    public string PlayProgress()
    {
        return "";
    }

    public static LevelType GetLevelType(int level)
    {
        return LevelRemoteManager.Instance.levelConfig.GetLevelInfo(level).levelType;
    }

    public bool IsHaveMechanic(MechanicTutorialType type)
    {
        switch (type)
        {
            default:
                return false;
        }
    }

    public bool IsHasSaveGameData(int lvl)
    {
        return false;
    }
}