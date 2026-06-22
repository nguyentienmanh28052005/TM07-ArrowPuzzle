using mygame.sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelSetting : MonoBehaviour
{
    [SerializeField] Animator animatorSettingButton;
    [SerializeField] Animator animatorSettingPanel;
    [SerializeField] Button btnSetting;
    [SerializeField] Button btnBgSetting;
    [SerializeField] Button btnQuit;
    [SerializeField] private Button btnReplay;
    //[SerializeField] Button btnGuide;
    public Button BtnSetting => btnSetting;
    //public Button BtnGuide => btnGuide;
    private void Awake()
    {
        btnSetting.onClick.AddListener(SettingButton);
        btnBgSetting.onClick.AddListener(SettingButton);
        btnQuit.onClick.AddListener(QuitButton);
        //btnGuide.onClick.AddListener(GuideButton);
        animatorSettingButton.gameObject.SetActive(true);
        animatorSettingPanel.gameObject.SetActive(true);
        btnReplay.onClick.AddListener(ReplayButton);
    }

    private void ReplayButton()
    {
        //LogFireBaseCustomer.LogRetryLevel(GameManager.CurrentLevel);
        //TimeSpan ts = TimeSpan.FromSeconds(LevelManager.Instance.playDuration);
        //long msFromTs = (long)ts.TotalMilliseconds;
        //LogEvent.LevelEnd(GameRes.GetLevel(), LevelControllerBase.LevelOrder, msFromTs,
        //    DataManager.Instance.ConsecutivePlay, GameMode.Level, LevelManager.Instance.LevelController.GetProgress(),
        //    LogEvent.LevelResult.lose);
        GameManager.Instance.RestartGame();
    }

    public void SettingButton()
    {
        if (GameManager.GameState == GameState.Complete || GameManager.GameState == GameState.Defeat)
        {
            return;
        }
        var uiTutorial = UIManager.Instance.GetPopupActive<UITutorial>();
        if (uiTutorial != null && uiTutorial.isShowing)
        {
            return;
        }
        bool isOpen = animatorSettingButton.GetBool("open");
        animatorSettingButton.SetBool("open", !isOpen);
        animatorSettingPanel.SetBool("open", !isOpen);
        if (!isOpen)
        {
            LogEvent.ScreenGo(LogEvent.ScreenName.SettingPlay, LogEvent.ButtonName.ButtonInGame);
            GameManager.Instance.PauseGame();
            // FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_pause");
        }
        else
        {
            GameManager.Instance.ResumeGame();
            LogEvent.ChangeScreenName(LogEvent.ScreenName.LevelPlay);
            // FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_resume");
        }

    }

    public void SettingButton(bool open)
    {
        if (GameManager.GameState == GameState.Complete || GameManager.GameState == GameState.Defeat)
        {
            return;
        }
        bool isOpen = animatorSettingButton.GetBool("open");
        if (isOpen == open) return;
        animatorSettingButton.SetBool("open", !isOpen);
        animatorSettingPanel.SetBool("open", !isOpen);
        if (!isOpen)
        {
            LogEvent.ScreenGo(LogEvent.ScreenName.SettingPlay);
            GameManager.Instance.PauseGame();
            // FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_pause");
        }
        else
        {
            GameManager.Instance.ResumeGame();
            // FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_resume");
        }
    }
    public void QuitButton()
    {
        bool isOpen = animatorSettingButton.GetBool("open");
        animatorSettingButton.SetBool("open", !isOpen);
        animatorSettingPanel.SetBool("open", !isOpen);
        UIManager.Instance.ShowPopup<UIQuitInGame>(() =>
        {

        });

    }
    public void GuideButton()
    {
        bool isOpen = animatorSettingButton.GetBool("open");
        animatorSettingButton.SetBool("open", !isOpen);
        animatorSettingPanel.SetBool("open", !isOpen);
        UIManager.Instance.ShowPopup<PopupMechanicGuide>(() =>
        {
            GameManager.Instance.ResumeGame();
        });

    }
    private void OnDisable()
    {
        animatorSettingButton.SetBool("open", false);
        animatorSettingPanel.SetBool("open", false);
    }
}
