using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Crystal;
using DG.Tweening;
using master;
using mygame.sdk;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIShopPopup : PopupUI
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] Button closeButton;
    [SerializeField] Animation animation;
    [Header("Bundle")] [SerializeField] RectTransform bundlePackHolder;
    [SerializeField] UIPackage prefabBundlePack;
    [SerializeField] UIPackage prefabBundlePack_v1;
    [SerializeField] UIPackage prefabBundlePack_1;
    [SerializeField] UIPackage prefabBundlePack_1_v1;
    [SerializeField] UIPackage prefabBundlePack_2;
    [SerializeField] BundlePackInfo[] bundlePacks;
    [Header("GoldPack")] [SerializeField] RectTransform goldPackHolder;
    [SerializeField] UIBuyMoney prefabPack;
    [SerializeField] Transform[] holderPack;
    [SerializeField] RectTransform contentPanel;
    [SerializeField] GameObject adsPack;
    [SerializeField] private Button restoreButton;
    [SerializeField] private RectTransform LoadingPanel;

    public GameObject bpPack;
    public GameObject specialPack;
    private GameObject smallPack;

    public RectTransform targetRect;

    private LogEvent.IAP_ShowPosition showPosition;

    private void Awake()
    {
        restoreButton.onClick.AddListener(RestoreButtonClicked);
    }

    public override void Show(Action onClose)
    {
        base.Show(onClose);
        if (GameManager.GameState == GameState.Playing)
        {
            LogEvent.ScreenGo(LogEvent.ScreenName.ShopPlay, LogEvent.ButtonName.ButtonInGame);
        }
#if UNITY_ANDROID
        restoreButton.transform.parent.gameObject.SetActive(false);
#elif UNITY_IOS
        restoreButton.transform.parent.gameObject.SetActive(true);
#endif
        if (UIManager.Instance.GetScreenActive<MainMenuScreen>() == null)
        {
        }
        //topBar.SetActiveAvatar(UIManager.Instance.GetScreenActive<MainMenuScreen>()!= null);
        // topBar.SetActiveSetting(ScreenManager.Instance.currentScreen.GetType() == typeof(UIMainMenu));
        // DOVirtual.DelayedCall(.1f, () =>
        // {
        //     foreach (var package in GetComponentsInChildren<UIPackage>())
        //     {
        //         package.WaitLogIfVisible();
        //     }
        //
        //     foreach (var package in GetComponentsInChildren<UIBuyMoney>())
        //     {
        //         package.WaitLogIfVisible();
        //
        //     }
        // });
    }

    public void PlayAnimation()
    {
        animation.Play();
    }


    public void ScrollToHolder(int index)
    {
        Canvas.ForceUpdateCanvases();
        RectTransform target = holderPack[index].parent as RectTransform;
        Bounds contentBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scrollRect.content, target);
        Bounds viewportBounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(scrollRect.viewport, scrollRect.content);
        float contentHeight = scrollRect.content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;
        float itemCenterY = contentBounds.center.y;
        float scrollOffset = itemCenterY - viewportHeight / 2f;
        float normalizedPos = 1f - (scrollOffset / (contentHeight - viewportHeight));
        normalizedPos = Mathf.Clamp01(normalizedPos);
        scrollRect.verticalNormalizedPosition = normalizedPos;
    }

    public void ScrollToGoldPack()
    {
        Canvas.ForceUpdateCanvases();
        RectTransform target = goldPackHolder as RectTransform;
        Bounds contentBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scrollRect.content, target);
        Bounds viewportBounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(scrollRect.viewport, scrollRect.content);
        float contentHeight = scrollRect.content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;
        float itemCenterY = contentBounds.center.y;
        float scrollOffset = itemCenterY - viewportHeight / 2f;
        float normalizedPos = 1f - (scrollOffset / (contentHeight - viewportHeight));
        normalizedPos = Mathf.Clamp01(normalizedPos);
        scrollRect.verticalNormalizedPosition = normalizedPos;
    }

    public void ScrollToTarget()
    {
        ScrollTo(4800);
        return;
        Canvas.ForceUpdateCanvases();
        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;
        Vector3 targetLocalPosition = content.InverseTransformPoint(targetRect.position);
        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;
        float scrollableHeight = contentHeight - viewportHeight;
        float targetY = -targetLocalPosition.y;
        float normalizedY = Mathf.Clamp01(targetY / scrollableHeight);
        ;
        scrollRect.verticalNormalizedPosition = 1 - normalizedY;
    }

    Tween tweenRestore;

    private void RestoreButtonClicked()
    {
        LoadingPanel.gameObject.SetActive(true);
        var isShowNotify = true;
        tweenRestore = DOVirtual.DelayedCall(10, () =>
        {
            if (!isShowNotify) return;
            isShowNotify = false;
            LoadingPanel.gameObject.SetActive(false);
            UIManager.Instance.NotifyContent(content: "Restore Timed Out", key: "_restore_time_out");
        }, false);
        // InappHelper.OnPurchasesFetchedCB += OnPurchasesFetched;
        //
        // void OnPurchasesFetched(int status)
        // {
        //     if (!isShowNotify) return;
        //     Debug.Log("Restore Success: " + status);
        //     tweenRestore?.Kill();
        //     LoadingPanel.gameObject.SetActive(false);
        //     
        //     if (status == 1)
        //     {
        //         var contentValue = "Restore Purchase Success";
        //         var keyValue = "_restore_success";
        //         UIManager.Instance.NotifyContent(content: contentValue, key:  keyValue);
        //     }
        //     else if (status == 2)
        //     {
        //         var contentValue = "No Purchases To Restore";
        //         var keyValue = "_no_restore_available";
        //         UIManager.Instance.NotifyContent(content: contentValue, key:  keyValue);
        //     }
        //     else
        //     {
        //         UIManager.Instance.NotifyContent(content: "Restore Purchase Failed", key: "_restore_failed");
        //     }
        //     isShowNotify = false;
        //     InappHelper.OnPurchasesFetchedCB -= OnPurchasesFetched;
        // }

        void RestoreSuccess(bool status, string err)
        {
            if (!isShowNotify) return;
            Debug.Log("Restore Success: " + status);
            tweenRestore?.Kill();
            LoadingPanel.gameObject.SetActive(false);
            if (status)
            {
                var contentValue = "Restore Purchase Success";
                var keyValue = "_restore_success";
                UIManager.Instance.NotifyContent(contentValue, keyValue);
            }
            else
            {
                if (string.IsNullOrEmpty(err))
                    UIManager.Instance.NotifyContent(content: "Restore Purchase Failed", key: "_restore_failed");
                else
                    UIManager.Instance.NotifyContent(err);
            }

            isShowNotify = false;
            // InappHelper.OnPurchasesFetchedCB -= OnPurchasesFetched;
        }

        InappHelper.Instance.RestorePurchases(RestoreSuccess);
    }

    public void DisableAdsPack()
    {
        adsPack.SetActive(false);
    }

    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        if (AdsHelper.isRemoveAds(0))
        {
            DisableAdsPack();
        }

        for (int i = 0; i < DataManager.Instance.packageData.goldPackInfos.Length; i++)
        {
            UIBuyMoney packGold = null;
            if (goldPackHolder.childCount > i)
            {
                packGold = goldPackHolder.GetChild(i).GetComponent<UIBuyMoney>();
            }

            if (packGold == null)
            {
                packGold = Instantiate(prefabPack, goldPackHolder);
            }

            packGold.Initialized(DataManager.Instance.packageData.goldPackInfos[i], true);
        }

        for (int i = 0; i < bundlePacks.Length; i++)
        {
            UIPackage uIPackage;
            if (bundlePacks[i].packType == 1)
            {
                if (PlayerPrefsUtil.CFLevelShowPlayPopup > DataManager.Level)
                {
                    uIPackage = prefabBundlePack_1_v1;
                }
                else
                {
                    uIPackage = prefabBundlePack_1;
                }
            }
            else if (bundlePacks[i].packType == 2)
            {
                uIPackage = prefabBundlePack_2;
            }
            else
            {
                if (PlayerPrefsUtil.CFLevelShowPlayPopup > DataManager.Level)
                {
                    uIPackage = prefabBundlePack_v1;
                }
                else
                {
                    uIPackage = prefabBundlePack;
                }
            }

            UIPackage package = Instantiate(uIPackage, holderPack[bundlePacks[i].holderID]);
            if (bundlePacks[i].backGround != null && bundlePacks[i].btnBackGround != null)
            {
                package.SetBG(bundlePacks[i].backGround, bundlePacks[i].btnBackGround, bundlePacks[i].colorText,
                    bundlePacks[i].itemHolderBackground, bundlePacks[i].itemloopHolder);
            }

            if (bundlePacks[i].hasTagBanner)
            {
                package.SetTagBanner(bundlePacks[i].tagText);
            }
            else
            {
                package.tagBanner.SetActive(false);
            }

            package.shopPopup = this;
            package.SetUp(bundlePacks[i].id);
            if (bundlePacks[i].id == 2)
            {
                smallPack = package.gameObject;
                if (PlayerPrefsUtil.CF_SmallBundleAcive)
                {
                    smallPack.SetActive(!DataManager.Instance.packageData.FindPackage(1).isActive);
                }
                else
                {
                    smallPack.SetActive(true);
                }
            }
        }

        bpPack.SetActive(false);

        CheckRefresShop();
        RefreshLayout();
    }

    public void CheckRefresShop()
    {
        if (smallPack)
        {
            if (PlayerPrefsUtil.CF_SmallBundleAcive)
            {
                smallPack.SetActive(!DataManager.Instance.packageData.FindPackage(1).isActive);
            }
            else
            {
                smallPack.SetActive(true);
            }
        }

        specialPack.SetActive(DataManager.Instance.packageData.FindPackage(1).isActive);
        RefreshLayout();
    }

    public void RefreshLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel);
    }

    private void OnEnable()
    {
        RefreshLayout();
    }

    public void SetShowPosition(LogEvent.IAP_ShowPosition showPosition)
    {
        this.showPosition = showPosition;
        foreach (var package in GetComponentsInChildren<UIPackage>())
        {
            package.IAPShowAction(LogEvent.IAP_ShowAction.click_button, showPosition);
        }

        foreach (var package in GetComponentsInChildren<UIBuyMoney>())
        {
            package.IAPShowAction(LogEvent.IAP_ShowAction.click_button, showPosition);
        }
    }

    public void SetUpMainShop()
    {
        closeButton.gameObject.SetActive(false);
    }

    public void ScrollTo(float value)
    {
        foreach (var layoutGroup in GetComponentsInChildren<VerticalLayoutGroup>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }

        Canvas.ForceUpdateCanvases();
        scrollRect.velocity = Vector2.zero;
        scrollRect.content.localPosition = new Vector3(scrollRect.content.localPosition.x, value, 0);
    }

    public void IAPShowAction(LogEvent.IAP_ShowAction showAction)
    {
        foreach (var package in GetComponentsInChildren<UIPackage>())
        {
            package.IAPShowAction(showAction, showPosition);
        }

        foreach (var package in GetComponentsInChildren<UIBuyMoney>())
        {
            package.IAPShowAction(showAction, showPosition);
        }
    }

    public override void OnClickClose()
    {
        base.OnClickClose();
        
        LogEvent.ChangeScreenName(LogEvent.ScreenName.LevelPlay);
    }

    private void OnDestroy()
    {
        if (tweenRestore != null)
        {
            tweenRestore.Kill();
        }
    }

    [Serializable]
    private struct BundlePackInfo
    {
        public int id;
        public int packType;
        public int holderID;
        public Sprite backGround;
        public Sprite btnBackGround;
        public Sprite itemHolderBackground;
        public Sprite itemloopHolder;
        public Color colorText;
        public bool hasTagBanner;
        public string tagText;
    }
}
