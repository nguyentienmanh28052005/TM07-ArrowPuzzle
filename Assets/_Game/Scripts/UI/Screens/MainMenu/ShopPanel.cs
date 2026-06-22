using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPanel : MainMenuPanel
{
    public UIShopPopup uIShopPopup;
    public CanvasGroup canvasGroup;
    private bool isCache = true;

    public override void Initialize(MainMenuScreen screenUI)
    {
        base.Initialize(screenUI);
        if (uIShopPopup != null)
        {
            uIShopPopup.Show(null);
            uIShopPopup.Setup();
        }
    }

    public override void Active()
    {
        base.Active();
        canvasGroup.blocksRaycasts = true;
        if (uIShopPopup == null)
        {
            isCache = false;
            uIShopPopup = UIManager.Instance.ShowPopup<UIShopPopup>(null);
        }

        uIShopPopup.SetShowPosition(LogEvent.IAP_ShowPosition.home_shop);
        uIShopPopup.transform.SetParent(transform, false);
        uIShopPopup.SetUpMainShop();
        uIShopPopup.ScrollTo(0);
    }

    public override void Deactive()
    {
        if (uIShopPopup == null) return;
        if (!isCache) uIShopPopup.Hide();
        canvasGroup.blocksRaycasts = false;
    }
}