using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using DG.Tweening;
using UnityEngine.UI;
using System;

public class RewardClaimChestPanel : MonoBehaviour
{
    [SerializeField] SkeletonGraphic[] skeletonGraphicsPrefab;
    [SerializeField] RectTransform[] holderChest;
    SkeletonGraphic skeletonChest;
    [SerializeField] string openAnimation = "Opening";
    [SerializeField] string jumpAnimation = "Appear";
    [SerializeField] string openingAnimation= "Open_idle";
    [SerializeField] VisualShowClaimChest[] visualShowClaimChests;
    [SerializeField] RectTransform holder;
    [SerializeField] RectTransform chestHolder;
    VisualShowClaimChest visualShowClaim;
    public DataResource[] DataResources;
    RectTransform btnClaimCached;
    [SerializeField] Button btnClaim;
    [SerializeField] ParticleSystem fxOpen;
    [SerializeField] ParticleSystem fxReward;
    [SerializeField] string[] listSkinChest = {"hidden_1","hidden_2","hidden_3","hidden_4"};
    public RewardVisualClaim visualClaimPrefab;
    Action cbOnRewarded;
    Action cbOnHide;
    bool isWaiting;
    private void Awake()
    {
        //for(int i = 0; i < skeletonGraphicsPrefab.Length; i++)
        //{
        //    if (skeletonGraphicsPrefab[i] != null)
        //    {
        //        skeletonGraphicsPrefab[i].AnimationState.Event += OnSpineEvent;
        //    }
        //}
        btnClaim.onClick.AddListener(ClaimReward);
    }
    public void Initialize()
    {
        visualClaimPrefab.gameObject.SetActive(false);
        foreach (var visual in visualShowClaimChests)
        {
            visual.Initialize(visualClaimPrefab);
        }

    }
    public void Setup(DataResource[] dataResources, RectTransform rectHolderChest, RectTransform claimBtnFake, Action cbOnRewarded, Action cbOnHide,int typeChest,string skinName,ChestSkeleton chestSkeleton,bool isWaiting)
    {
        this.cbOnRewarded = cbOnRewarded;
        this.btnClaimCached = claimBtnFake;
        this.cbOnHide = cbOnHide;
        DataResources = dataResources;
        btnClaimCached.gameObject.SetActive(false);
        btnClaim.gameObject.SetActive(false);
        this.isWaiting = isWaiting;
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), screenPos, RewardReceivedHub.Instance.Canvas.worldCamera, out Vector2 localPoint);
        holder.anchoredPosition = RewardReceivedHub.PositionInsideContainer(holder, rectHolderChest);
        if (dataResources.Length <= 5)
        {
            holder.anchoredPosition += Vector2.up * 120;
        }
        fxReward.gameObject.SetActive(false);
        //for(int i = 0; i < skeletonGraphicsPrefab.Length; i++)
        //{
        //    skeletonGraphicsPrefab[i].gameObject.SetActive(false);
        //}
        int indexChestSkeleton = Mathf.Clamp((int)chestSkeleton, 0, skeletonGraphicsPrefab.Length - 1);
        skeletonChest = Instantiate(skeletonGraphicsPrefab[indexChestSkeleton], holderChest[indexChestSkeleton]);
        skeletonChest.AnimationState.Event += OnSpineEvent;
        fxOpen = skeletonChest.transform.GetChild(0).GetComponentInChildren<ParticleSystem>();
        skeletonChest.gameObject.SetActive(true);
        var skeleton = skeletonChest.Skeleton;
        if (string.IsNullOrEmpty(skinName))
        {
            skinName = listSkinChest[Mathf.Clamp(typeChest, 0, listSkinChest.Length - 1)];
        }
        skeleton.SetSkin((Skin)null);
        skeleton.SetSkin(skinName);
        skeleton.SetSlotsToSetupPose(); 
    }
    public void ShowRewardGroup(DataResource[] dataResources, RectTransform rectHolderChest, RectTransform claimBtnFake, Action cbOnRewarded, Action cbOnHide,int typeChest, float scaleStart,string skinName, ChestSkeleton chestSkeleton = ChestSkeleton.Default, bool isWaiting= true)
    {
        Setup(dataResources, rectHolderChest, claimBtnFake, cbOnRewarded, cbOnHide,typeChest, skinName,chestSkeleton,isWaiting);
        OpenChest(scaleStart);
    }
    public void ShowRewardGroup(DataResource[] dataResources, RectTransform rectHolderChest, RectTransform claimBtnFake, Action cbOnRewarded, Vector2 startPosJump, Action cbOnHide,int typeChest, float scaleStart, string skinName, ChestSkeleton chestSkeleton = ChestSkeleton.Default,bool isWaiting = true)
    {
        Setup(dataResources, rectHolderChest, claimBtnFake, cbOnRewarded, cbOnHide, typeChest, skinName, chestSkeleton,isWaiting);
        OpenChest(startPosJump, scaleStart);
    }
    private void OnSpineEvent(Spine.TrackEntry trackEntry, Spine.Event e)
    {

        if (e.Data.Name.ToLower() == "open")
        {
            ShowReward(DataResources,isWaiting);
            AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Open_Chest);
            fxOpen.gameObject.SetActive(true);
            fxReward.gameObject.SetActive(true);
            fxOpen.Play();
            fxReward.Play();
        }

    }
    public void OpenChest(float scaleStart)
    {
        skeletonChest.transform.localScale = Vector3.one * scaleStart;
        skeletonChest.transform.DOScale(0.8f, .4f);
        AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Chest_Jump);
        PlayAnimation(jumpAnimation);
        for (int i = 0; i < visualShowClaimChests.Length; i++)
        {
            visualShowClaimChests[i].Hide();
        }
    }
    public void OpenChest(Vector2 startScrPoint, float scaleStart)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(chestHolder, startScrPoint, RewardReceivedHub.Instance.Canvas.worldCamera, out Vector2 localPoint);
        skeletonChest.rectTransform.anchoredPosition = localPoint;
        skeletonChest.rectTransform.DOLocalJump(Vector2.zero, 400, 1, .7f);
        OpenChest(scaleStart);
    }
    public void ShowReward(DataResource[] dataResources,bool isWaiting)
    {
        visualShowClaim = null;
        for (int i = 0; i < visualShowClaimChests.Length; i++)
        {
            if (visualShowClaimChests[i].numReward == dataResources.Length)
            {
                visualShowClaimChests[i].Show();
                visualShowClaimChests[i].isWaiting = isWaiting;
                visualShowClaimChests[i].SetResources(dataResources);
                Sequence sequence = visualShowClaimChests[i].AnimAppear(chestHolder.position);

                sequence.OnComplete(() =>
                {
                    btnClaim.gameObject.SetActive(true);
                    btnClaimCached.gameObject.SetActive(true);
                    btnClaimCached.transform.localScale = Vector3.zero;
                    btnClaimCached.transform.DOScale(1, .3f).SetId(this);
                });
                visualShowClaim = visualShowClaimChests[i];
                break;
            }
        }
    }
    public void ClaimReward()
    {
        btnClaim.gameObject.SetActive(false);
        skeletonChest.transform.DOScale(0, .2f);
        cbOnRewarded?.Invoke();
        visualShowClaim.ReceiveReward(Hide);
    }
    private void PlayAnimation(string animationName,float speed=1,bool isLoop = false)
    {
        var trackEntry = skeletonChest.AnimationState.SetAnimation(0, animationName, isLoop);
        trackEntry.Complete += OnAnimationComplete;
        trackEntry.TimeScale = speed;
    }

    private void OnAnimationComplete(Spine.TrackEntry trackEntry)
    {
        if (trackEntry.Animation.Name == jumpAnimation)
        {
            PlayAnimation(openAnimation,1.5f);
        }
        if (trackEntry.Animation.Name == openAnimation)
        {
            PlayAnimation(openingAnimation, 1,true);
        }
    }
    public void Hide()
    {
        cbOnHide?.Invoke();
        gameObject.SetActive(false);
        Destroy(skeletonChest.gameObject);
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
}
