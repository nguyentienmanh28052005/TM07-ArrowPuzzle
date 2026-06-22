using System;
using System.Collections;
using System.Collections.Generic;
using Crystal;
using DG.Tweening;
using mygame.sdk;
using UnityEngine;
using UnityEngine.UI;

public class ItemDisplay : MonoBehaviour,IResourceTarget
{
    [SerializeField] private Text text;

    [SerializeField] private RES_type resType;

    [SerializeField] private bool isSetupEnable = true;
    [SerializeField] private Image icon;
    [SerializeField] Button button;
    [SerializeField] bool isOnMain;
    private int oldNum;

    private void Awake()
    {
        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                switch (resType)
                {
                    case RES_type.Star:
                        //if (HomeDecor.GameManager.Instance.IsActiveDecorate)
                        //{
                        //    UIManager.Instance.ShowPopup<PopupEarnStar>(null);
                        //}
                        //else if(HomeDecor.GameManager.Instance.GetUnlockLevel()>0 && UIManager.Instance.GetScreenActive<MainMenuScreen>() != null && UIManager.Instance.GetScreen<MainMenuScreen>().gameObject.activeInHierarchy == true)
                        //{
                        //    UIManager.Instance.GetScreen<MainMenuScreen>().ActivePanel(5);

                        //}
                        break;
                    default:
                        if (UIManager.Instance.GetScreenActive<MainMenuScreen>() != null && UIManager.Instance.GetScreen<MainMenuScreen>().gameObject.activeInHierarchy == true)
                        {
                            var mainScreen = UIManager.Instance.GetScreen<MainMenuScreen>();
                            mainScreen.ActivePanel(0);
                            ((ShopPanel)mainScreen.groups[0].panel).uIShopPopup.ScrollToTarget();
                        }
                        else
                        {
                            var shopPopup = UIManager.Instance.ShowPopup<UIShopPopup>(null);
                            shopPopup.GetComponent<SafeAreaPanel>().enabled = true;
                            shopPopup.SetShowPosition(LogEvent.IAP_ShowPosition.shop_popup);
                            shopPopup.ScrollToTarget();
                            shopPopup.PlayAnimation();
                        }
                        break;
                }
                

            });
        }
    }
    private void OnEnable()
    {
        
        IResourceTarget.OnItemChange += SetText;
        if (isSetupEnable)
        {
            int res = GameRes.getRes(resType);
            int cache = RewardReceivedHub.GetCacheValue(resType);
            int endValue = res- cache;

            SetText(endValue);
        }
        RewardReceivedHub.RegisterTarget(this);
    }

    private void OnDisable()
    {
        this.DOKill();
        tweenScale?.Kill();
        IResourceTarget.OnItemChange -= SetText;
        RewardReceivedHub.RemoveTarget(this);
    }

    private void OnDestroy()
    {
        this.DOKill();
        tweenScale?.Kill();
        IResourceTarget.OnItemChange -= SetText;
    }
    Tween tweenSetText;
    private void SetText(RES_type res, float duration = 1)
    {
        if (resType != RES_type.NONE && res != resType) return;
        if (tweenSetText != null)
        {
            tweenSetText.Kill();
        }
        int endValue = GameRes.getRes(resType) - RewardReceivedHub.GetCacheValue(resType);
       
        int num = oldNum;
        tweenSetText = DOTween.To(() => num, x => num = x, endValue, duration).OnUpdate(() => { SetText(num); });
    }

    public void SetText(int endValue)
    {
        oldNum = endValue;
        if (text == null) return;
        text.text = SdkUtil.convertMoneyToString(endValue);
    }
    public List<RES_type> GetResourceTypes()
    {
        return new List<RES_type>() { resType };
    }

    public Transform GetTransform()
    {
        return icon.transform;
    }
    Tween tweenScale = null;

    public void UpdateVisual()
    {
        if (this == null || transform == null) return;
        tweenScale?.Kill();

        tweenScale = transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.05f).SetId(this).OnComplete(() =>
        {
            if (this != null && transform != null)
            {
                transform.DOScale(Vector3.one, 0.03f).SetId(this);
            }
        });
        if(resType == RES_type.Star)
        {
            int res = GameRes.getRes(resType);
            int cache = RewardReceivedHub.GetCacheValue(resType);
            int endValue = res - cache;

            SetText(endValue);
        }
    }
}