using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using mygame.sdk;
using UnityEngine;
using UnityEngine.UI;

public class UIAllowTracking : PopupUI
{
    [SerializeField] private Button confirmButton;
    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        confirmButton.onClick.AddListener(OnClickConfirm);
    }
    public override void Show(Action onClose)
    {
        base.Show(onClose);
    }

    private void OnClickConfirm()
    {
        FIRhelper.logEvent("click_allow_tracking");
        LogEvent.PlayerAction("click_allow_tracking");
        Hide();
    }

    public override void Hide()
    {
        GameHelper.requestIDFA();
        DOVirtual.DelayedCall(1f, () =>
        {
            AdsHelper.Instance.loadGift4ThisTurn("open_game", DataManager.Level, state => { });
            AdsHelper.Instance.loadFull4ThisTurn("open_game", false, DataManager.Level, 0);
        }, false);
        base.Hide();
    }
}
