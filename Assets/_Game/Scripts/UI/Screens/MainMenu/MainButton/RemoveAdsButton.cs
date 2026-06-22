using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using mygame.sdk;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class RemoveAdsButton : MonoBehaviour, IEventButton
{
    [Space, Header("UI")]
    [SerializeField] Button btnRemoveAds;
    [SerializeField] SkeletonGraphic skeletonGraphic;
    [SerializeField] GameObject obj_Notify;
    [SerializeField] private Text txtBtn;

    private void Start()
    {
        btnRemoveAds.onClick.AddListener(ShowRemoveAds);
        gameObject.SetActive(!AdsHelper.isRemoveAds(0));
        CheckActive();
        obj_Notify.gameObject.SetActive(false);
        InappHelper.Instance.onPurchaseSuccess += OnPurchaseSuccess;
        GameEvent.OnReceiveFirebaseDataDone += CheckActive;
        GameEvent.OnBackToMenu += CheckActive;
        // txtBtn.SetText("_no_ads");
    }

    private void OnDestroy()
    {
        InappHelper.Instance.onPurchaseSuccess -= OnPurchaseSuccess;
        GameEvent.OnReceiveFirebaseDataDone -= CheckActive;
        GameEvent.OnBackToMenu -= CheckActive;
    }
    void CheckActive()
    {
        gameObject.SetActive(DataManager.Level >= PlayerPrefsUtil.CF_FirstLevelShowMain && !AdsHelper.isRemoveAds(0));
    }
    private void OnPurchaseSuccess(string purchasedItem)
    {
        gameObject.SetActive(!AdsHelper.isRemoveAds(0));
    }

    private void OnEnable()
    {
        StartCoroutine(IUpdateVisual());
        skeletonGraphic.Initialize(false);
        skeletonGraphic.AnimationState.ClearTracks();
        skeletonGraphic.Skeleton.SetSlotsToSetupPose();
        skeletonGraphic.AnimationState.SetAnimation(0, "idle", true);
        Register();
        //Appear();
        //CancelInvoke("Appear");
    }
    private void OnDisable()
    {
        UnRegister();
    }
    private void Appear()
    {
        skeletonGraphic.AnimationState.SetAnimation(0, "action", false).Complete += entry =>
        {
            skeletonGraphic.AnimationState.SetAnimation(0, "idle", true);
            Invoke("Appear", 3);
        };
    }

    IEnumerator IUpdateVisual()
    {
        while (gameObject.activeSelf)
        {
            if (CheckShowNotify())
            {
                obj_Notify.SetActive(true);
            }
            else
            {
                obj_Notify.SetActive(false);
            }
            yield return new WaitForSeconds(1);
        }
    }

    private bool CheckShowNotify()
    {
        return false;
    }

    private void ShowRemoveAds()
    {
        var uiRemoveAds = UIManager.Instance.ShowPopup<UIRemoveAds>(null);
        //skeletonGraphic.AnimationState.SetAnimation(0, "action", false).Complete += entry =>
        //{
        //    skeletonGraphic.AnimationState.SetAnimation(0, "idle", true);
        //};
        uiRemoveAds.IAPShowAction(LogEvent.IAP_ShowAction.click_button);
    }

    public Vector2 GetScreenPosition()
    {
        return UIManager.Instance.canvas.worldCamera.WorldToScreenPoint(transform.position);
    }

    public bool CanStartFlyJump()
    {
        return false;
    }

    public void StartFlyJump()
    {
       
    }

    public void CancelFlyJump()
    {
        
    }

    public void Animate()
    {
        if (skeletonGraphic != null && skeletonGraphic.AnimationState != null)
        {
            skeletonGraphic.AnimationState.SetAnimation(0, "action", false);
            skeletonGraphic.AnimationState.AddAnimation(0, "idle", true,0);
        }
    }

    public void Register()
    {
        EventButtonManager.AddEventButton(this);
    }

    public void UnRegister()
    {
        EventButtonManager.RemoveEventButton(this);
    }
}
