using mygame.sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupReplay : PopupUI
{
    [SerializeField] ButtonBoosterLevelPlay[] buttonBoosters;
    [SerializeField] Button btnPlay;
    [SerializeField] Text txtLevel;
    [SerializeField] Image buttonBGImg;
    [SerializeField] GameObject tagObj;

    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        btnPlay.onClick.AddListener(OnClickPlayLevel);
    }

    private void OnClickPlayLevel()
    {
        HeartManager.Instance.IsNoMoreLives(() =>
        {
            gameObject.SetActive(false);
            UIManager.Instance.ShowPopup<UIGuildNoMoreLives>(() => { gameObject.SetActive(true); });
        });
    }

    public override void OnClickClose()
    {
        // FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_lose_home");
        AudioManager.Instance.SetCacheAudio();
        var ss = AdsHelper.Instance.showFull("ui_fail", GameRes.GetLevel(), DataManager.Instance.ConsecutiveLose, 0, 0,
            false, false, cb: (state) =>
            {
                if (state == AD_State.AD_REWARD_OK || state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 ||
                    state == AD_State.AD_SHOW_MISS_CB || state == AD_State.AD_SHOW_FAIL ||
                    state == AD_State.AD_SHOW_FAIL2)
                {
                    AudioManager.Instance.ResetAudio();
                }

                if (state == AD_State.AD_CLOSE || state == AD_State.AD_CLOSE2 || state == AD_State.AD_SHOW_FAIL ||
                    state == AD_State.AD_SHOW_FAIL2)
                {
                    GameManager.Instance.TransitionBackToHome(ReasonBackToHome.Lose, base.OnClickClose);
                }
            });
        if (ss == false)
        {
            GameManager.Instance.TransitionBackToHome(ReasonBackToHome.Lose, base.OnClickClose);
        }
    }
}