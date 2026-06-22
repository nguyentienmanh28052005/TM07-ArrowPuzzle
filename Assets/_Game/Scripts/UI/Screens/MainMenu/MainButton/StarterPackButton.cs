using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using mygame.sdk;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class StarterPackButton : MonoBehaviour, IEventButton
{
    [Space, Header("UI")]
    [SerializeField] Button btnStarerPack;
    [SerializeField] GameObject obj_Notify;
    [SerializeField] IconEventMainBase iconEventBase;
    [SerializeField] private Text txtBtn;

    [SerializeField] int id;
    private PackageData.PackageInfo baseItem;

    private void Start()
    {
        baseItem = DataManager.Instance.packageData.FindPackage(id);
        btnStarerPack.onClick.AddListener(ShowStarterPack);
        gameObject.SetActive(baseItem.isActive);
        obj_Notify.gameObject.SetActive(false);
        CheckActive();
        InappHelper.Instance.onPurchaseSuccess += OnPurchaseSuccess;
        GameEvent.OnReceiveFirebaseDataDone += CheckActive;
        GameEvent.OnBackToMenu += CheckActive;
        // txtBtn.SetText("special");
    }
    void CheckActive()
    {
        gameObject.SetActive(DataManager.Level >= PlayerPrefsUtil.CF_FirstLevelShowMain && baseItem.isActive);
    }
    private void OnDestroy()
    {
        InappHelper.Instance.onPurchaseSuccess -= OnPurchaseSuccess;
        GameEvent.OnReceiveFirebaseDataDone -= CheckActive;
        GameEvent.OnBackToMenu -= CheckActive;
    }

    private void OnPurchaseSuccess(string purchasedItem)
    {
        gameObject.SetActive(baseItem.isActive);
    }

    private void OnEnable()
    {
        StartCoroutine(IUpdateVisual());

        //Appear();
        //CancelInvoke("Appear");
        Register();
    }
    private void OnDisable()
    {
        UnRegister();
    }

    private void Appear()
    {
        // skeletonGraphic.AnimationState.SetAnimation(0, "action", false).Complete += entry =>
        // {
        //     skeletonGraphic.AnimationState.SetAnimation(0, "idle", true);
        //     Invoke("Appear", 3);
        // };
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

    private void ShowStarterPack()
    {
        //skeletonGraphic.AnimationState.SetAnimation(0, "action", false).Complete += entry =>
        //{
        //    skeletonGraphic.AnimationState.SetAnimation(0, "idle", true);
        //};
        var uiStarterPack = UIManager.Instance.ShowPopup<UIStarterPack>(null);
        uiStarterPack.IAPShowAction(LogEvent.IAP_ShowAction.click_button);
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
        // skeletonGraphic.AnimationState.SetAnimation(0, "action", false);
        // skeletonGraphic.AnimationState.AddAnimation(0, "idle", true, 0);
        iconEventBase.Animate();
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
