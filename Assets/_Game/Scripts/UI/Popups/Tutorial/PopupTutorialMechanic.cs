using DG.Tweening;
using mygame.sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class PopupTutorialMechanic : PopupUI
{
    [SerializeField] private Text boosterNameText;
    [SerializeField] private Text boosterDesText;

    [SerializeField] private List<MechanicTutorialInfo> tutorialObjects;
    [SerializeField] Image bg;
    [SerializeField] GameObject block;
    [SerializeField] RectTransform popup;

    private MechanicTutorialType mechanicTutorialType;
    public static bool IsShowedMechanic(MechanicTutorialType mechanicTutorial)
    {
        return PlayerPrefs.GetInt($"show_mechanic_{(int)mechanicTutorial}", 0) > 0;
    }
    public static void SetShowedMechanic(MechanicTutorialType mechanicTutorial, bool flag = true)
    {
        PlayerPrefs.SetInt($"show_mechanic_{(int)mechanicTutorial}", flag ? 1 : 0);
    }
    public override void Show(Action onClose)
    {

        base.Show(onClose);
        block.SetActive(false);
    }
    public void Initialize(MechanicTutorialType tutorialType)
    {
        mechanicTutorialType = tutorialType;
        var info = tutorialObjects.FirstOrDefault(x => x.mechanicTutorialType == mechanicTutorialType);
        for (int i = 0; i < tutorialObjects.Count; i++)
        {
            tutorialObjects[i].objectTutorial.SetActive(mechanicTutorialType == tutorialObjects[i].mechanicTutorialType);
        }

        // info.skeletonAnimation.AnimationState.TimeScale = 0;
        // DOVirtual.DelayedCall(1f, () =>
        // {
        //     info.skeletonAnimation.AnimationState.TimeScale = 1;
        // }).SetId(this);

        boosterDesText.SetText(info.descTutorial);
        boosterNameText.SetText(info.nameTutorial);
        SetShowedMechanic(mechanicTutorialType);
    }
    public override void Hide()
    {
        //var uiIngame = uiManager.GetScreenActive<UIInGame>();
        //if (uiIngame != null)
        //{
        //    if (PlayerPrefsUtil.CFTutorialNewMechanicGuide > 0)
        //    {
        //        block.SetActive(true);
        //        bg.DOFade(0, .2f).SetId(this);
        //        popup.transform.DOScale(1f, .15f).SetEase(Ease.OutSine).SetId(this);
        //        popup.transform.DOScale(0f, .425f).SetId(this).SetEase(Ease.InQuad).SetDelay(.25f);
        //        popup.transform.DOMove(uiIngame.PanelSetting.BtnSetting.transform.position, .425f).SetDelay(.25f).SetEase(Ease.InQuad).SetId(this).OnComplete(() =>
        //        {
        //            uiIngame.PanelSetting.AnimatorSettingButton.Play("HideTut");
        //            uiIngame.PanelSetting.EnableNoticeGuide();
        //            base.Hide();
        //        });
        //    }
        //    else
        //    {
        //        uiIngame.PanelSetting.AnimatorSettingButton.Play("HideTut");
        //        uiIngame.PanelSetting.EnableNoticeGuide();
        //        base.Hide();
        //    }
        //}
        //else
        //{
        //    base.Hide();
        //}
        base.Hide();
    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
[Serializable]
public struct MechanicTutorialInfo
{
    public MechanicTutorialType mechanicTutorialType;
    public GameObject objectTutorial;
    public SkeletonGraphic skeletonAnimation;
    public string descTutorial;
    public string nameTutorial;
}

public enum MechanicTutorialType
{
    None = 0,
}