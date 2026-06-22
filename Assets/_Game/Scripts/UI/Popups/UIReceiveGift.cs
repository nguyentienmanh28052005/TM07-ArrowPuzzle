using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class UIReceiveGift : PopupUI
{
    private ItemInfo[] reward;
    [SerializeField] RectTransform rectHolderChest;
    [SerializeField] RectTransform claimTxt;
    [SerializeField] SkeletonGraphic skeletonGraphic;
    public override void Show(Action onClose)
    {
        base.Show(onClose);
        skeletonGraphic.Skeleton.SetSlotsToSetupPose();
        skeletonGraphic.AnimationState.SetAnimation(0, "action", false);
        skeletonGraphic.AnimationState.AddAnimation(0, "idle", true,0);
        //AnimCtrl();
    }

    public void Initialized(ItemInfo[] itemInfos)
    {
        reward = itemInfos;
        DataResource[] dataResources = new DataResource[reward.Length];
        for (int i = 0; i < itemInfos.Length; i++)
        {
            dataResources[i] = itemInfos[i].ToDataResource();
        }

        RewardReceivedHub.Instance.ShowRewardGroupChest(dataResources, rectHolderChest, claimTxt, () =>
        {
            gameObject.SetActive(false);
        }, () => {
            Hide();
        }, startScale: .4f, typeChest: (int)ChestType.Purple);
    }
    public void Initialized(DataResource[] dataResources)
    {
        RewardReceivedHub.Instance.ShowRewardGroupChest(dataResources, rectHolderChest, claimTxt, () =>
        {
            gameObject.SetActive(false);
        }, () => {
            Hide();
        }, startScale: .4f, typeChest: (int)ChestType.Purple);
    }
}