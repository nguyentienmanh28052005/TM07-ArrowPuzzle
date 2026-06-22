using System;
using System.Collections;
using System.Collections.Generic;
using mygame.sdk;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class UIQuitInGame : PopupUI
{
    [SerializeField] private Button quitButton;
    [SerializeField] Transform heartVisual;
    [SerializeField] Text txtMode;

    [SerializeField] private AutoChangeTheme levelTypeModifier;

    private void Awake()
    {
        quitButton.onClick.AddListener(OnClickQuit);
    }

    public override void Show(Action onClose)
    {
        base.Show(onClose);
        heartVisual.gameObject.SetActive(true);
        
        var curLevel = GameRes.GetLevel();
        var currentLevelType = LevelManager.GetLevelType(curLevel);
        if (levelTypeModifier != null)
        {
            levelTypeModifier.ApplyTheme(currentLevelType);
        }

        var arr = new string[3]
        {
            "",
            "HARD",
            "CRAZY",
        };

        txtMode.text = arr[(int) currentLevelType];
    }

    private void OnClickQuit()
    {
        onQuit();

        void onQuit()
        {
            int lv = GameRes.GetLevel();
            GameEvent.OnFinishLevel?.Invoke(lv, false, (int)LevelManager.GetLevelType(lv), EGameMode.Level);
            TimeSpan ts = TimeSpan.FromSeconds(LevelManager.Instance.playDuration);
            long msFromTs = (long)ts.TotalMilliseconds;
            LevelManager.Instance.GetLevelAnalyticsData(out int totalBus, out int completedBus, out int baseSlot, out int finalSlot);
            
            LogEvent.LevelEnd(
                lv: GameRes.GetLevel(),
                levelId: LevelManager.Instance.levelOder,
                playTime: msFromTs,
                playIndex: DataManager.Instance.ConsecutivePlay - 1,
                gameMode: GameMode.Level,
                levelProgress: LevelManager.Instance.GetProgress(),
                result: LogEvent.LevelResult.exit,
                totalBus: totalBus,
                completedBus: completedBus,
                baseSlot: baseSlot,
                finalSlot: finalSlot,
                levelProgressDetail: LevelManager.Instance.PlayProgress()
            );
            // FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_pause_home");
            DataManager.Instance.ConsecutiveLose++;
            DataManager.isLose = true;
            DataManager.Instance.ConsecutiveWin = 0;
            var CFLevelShowPlayPopup = PlayerPrefsUtil.CFLevelShowPlayPopup;
            if (lv < CFLevelShowPlayPopup && CFLevelShowPlayPopup > 0)
            {
                LogEvent.ScreenGo(LogEvent.ScreenName.LevelLose, LogEvent.ButtonName.ButtonQuit);
                UIManager.Instance.ShowPopup<UITryAgain>(null);
            }
            else
            {
                UIManager.Instance.ShowPopup<PopupReplay>(null);
            }

            Hide();
        }
    }

    public override void OnClickClose()
    {
        GameManager.Instance.ResumeGame();
        // FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_resume");
        base.OnClickClose();
    }

    public override void Hide()
    {
        if (GameManager.GameState == GameState.Pause)
        {
            GameManager.Instance.ResumeGame();
            // FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_resume");
        }

        base.Hide();
    }
}