using master;
using Myapi;
using mygame.sdk;
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class GameController
{
    public static void OnStartGame()
    {
        /*if (!GameManager.IsPlayingFirst)
        {
            AddressableManager.Instance.CheckDownloadAssetsEvent();
            AddressableManager.Instance.PreloadAssetsPopup();
        }
        EventController.Instance.CheckActiveEvent();
        EventController.Instance.ShowPopupSuggestedEvent();*/
        //EventButtonManager.StopAnimActive();
        if (tweenStar != null)
        {
            tweenStar.Kill();
        }
    }

    public static void OnStartLevel()
    {
        if (tweenStar != null)
        {
            tweenStar.Kill();
        }
    }

    /// <summary>
    /// 0: start game, 1: Level -> Main
    /// </summary>
    public static void OnBackToMenu(int state)
    {
        GameEvent.OnBackToMenu?.Invoke();
        var blockUI = UIManager.Instance.blockerUI;
        blockUI.gameObject.SetActive(false);
        AdsHelperWrapper.HideRectBanner();
        Observer.Notify(ObserverName.screen_resize, null, false);
        void CheckAction()
        {
            var checkBlockUI = ConfigManager.Instance.IsShowPackage(DataManager.Level);

            if (checkBlockUI)
            {
                blockUI.gameObject.SetActive(true);
            }

            OnVisualClaimStar(() =>
                GoldFly(() =>
                {
                    ConfigManager.Instance.ShowPackage(DataManager.Level);
                    blockUI.gameObject.SetActive(false);
                }));
        }
    }

    static Tween tweenStar;

    public static void OnVisualClaimStar(Action onDone)
    {
        GameEvent.OnClaimStar?.Invoke();

        if (RewardReceivedHub.GetCacheValue(RES_type.Star) > 0)
        {
            tweenStar = DOVirtual.DelayedCall(2.5f, () => { onDone?.Invoke(); });
            RewardReceivedHub.RemoveCacheValue(RES_type.Star);
        }
        else
        {
            onDone?.Invoke();
        }
    }

    public static void GoldFly(Action onDone)
    {
        if (UILevelComplete.cacheCoinGet <= 0)
        {
            onDone?.Invoke();
            return;
        }

        if (UIManager.Instance.GetScreenActive<MainMenuScreen>() != null)
        {
            RewardReceivedHub.Instance.CoinFly(new Vector2(Screen.width / 2, Screen.height / 2), null, 8,
                (index, total) =>
                {
                    if (index == total - 1)
                    {
                        onDone?.Invoke();
                    }
                }, UILevelComplete.cacheCoinGet);
        }
        else
        {
            onDone?.Invoke();
        }

        RewardReceivedHub.AddCacheValue(RES_type.GOLD, -UILevelComplete.cacheCoinGet);
        UILevelComplete.cacheCoinGet = 0;
    }

    public static void Preload()
    {
    }
}