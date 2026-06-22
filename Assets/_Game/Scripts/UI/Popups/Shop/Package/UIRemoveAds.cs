using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using mygame.sdk;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class UIRemoveAds : PopupUI
{
    [SerializeField] SkeletonGraphic skeletonGraphic;
    [SerializeField] UIPackage uiPackage;
    [SerializeField] UIPackage uiPackagev1;

    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        uiPackage.onBuySuccess += Hide;
        uiPackagev1.onBuySuccess += Hide;

        // var sizeRate = Screen.height / (float)Screen.width;
        // if (sizeRate < 1.8f)
        // {
        //     GameManager.Instance.HideBanner();
        // }
        
        skeletonGraphic.AnimationState.SetAnimation(0, "action", false).Complete += entry => 
        {
            skeletonGraphic.AnimationState.SetAnimation(0, "idle", true);
        };
    }
    
    public void IAPShowAction(LogEvent.IAP_ShowAction showAction)
    {
        if (PlayerPrefsUtil.CFLevelShowPlayPopup > DataManager.Level)
        {
            uiPackage.gameObject.SetActive(false);
            uiPackagev1.gameObject.SetActive(true);
            uiPackagev1.IAPShowAction(showAction, LogEvent.IAP_ShowPosition.home_popup, true);
        }
        else
        {
            uiPackage.gameObject.SetActive(true);
            uiPackagev1.gameObject.SetActive(false);
            uiPackage.IAPShowAction(showAction, LogEvent.IAP_ShowPosition.home_popup, true);

        }
    }
    
    /*public override void Hide()
    {
        var sizeRate = Screen.height / (float)Screen.width;
        if (sizeRate < 1.8f)
        {
            GameManager.Instance.ShowBanner();
        }
        base.Hide();
    }*/
}
