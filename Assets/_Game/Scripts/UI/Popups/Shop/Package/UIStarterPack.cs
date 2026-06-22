using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using mygame.sdk;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class UIStarterPack : PopupUI
{
    [SerializeField] private CanvasPropertyOverrider canvasPropertyOverrider;
    [SerializeField] UIPackage uiPackage;
    [SerializeField] UIPackage uiPackagev1;
    [SerializeField] SkeletonGraphic skeleton;

    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        uiPackage.onBuySuccess += Hide;
        uiPackagev1.onBuySuccess += Hide;
        PlayOpen();
        skeleton.AnimationState.Complete += OnAnimationComplete;
    }

    public void PlayOpen()
    {
        skeleton.AnimationState.SetAnimation(0, "open", false);
    }

    private void OnAnimationComplete(TrackEntry entry)
    {
        // Khi anim "Open" chạy xong → tự về "Idle"
        if (entry.Animation.Name == "open")
        {
            skeleton.AnimationState.SetAnimation(0, "idle", true);
        }
    }

    public void IAPShowAction(LogEvent.IAP_ShowAction showAction)
    {
        if (PlayerPrefsUtil.CFLevelShowPlayPopup > DataManager.Level)
        {
            uiPackage.gameObject.SetActive(false);
            uiPackagev1.gameObject.SetActive(true);
            uiPackagev1.IAPShowAction(showAction, LogEvent.IAP_ShowPosition.home_popup);
        }
        else
        {
            uiPackage.gameObject.SetActive(true);
            uiPackagev1.gameObject.SetActive(false);
            uiPackage.IAPShowAction(showAction, LogEvent.IAP_ShowPosition.home_popup);

        }
    }
}