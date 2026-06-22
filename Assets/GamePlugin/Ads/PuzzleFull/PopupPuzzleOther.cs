using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using mygame.sdk;
using MyJson;

public class PopupPuzzleOther : MonoBehaviour
{
    public Image gameIc;
    public Text gameName;
    public Text gameNameBottom;
    public GameObject rewarded;
    public GameObject btClose;


    public bool isRewarded = true;

    private PromoGameOb gameShow;
    private static bool isDestroy = false;

    void Awake()
    {
#if UNITY_IOS || UNITY_IPHONE
        isRewarded = false;
#endif
        if (isRewarded)
        {
            rewarded.SetActive(true);
            gameNameBottom.gameObject.SetActive(false);
            gameName.gameObject.SetActive(true);
        }
        else
        {
            rewarded.SetActive(false);
            gameNameBottom.gameObject.SetActive(true);
            gameName.gameObject.SetActive(false);
        }
        isDestroy = false;
    }

    private void OnDestroy()
    {
        isDestroy = true;
    }

    public void setGameShow(PromoGameOb game)
    {
        gameShow = game;
        gameName.text = gameShow.name;
        gameNameBottom.text = gameShow.name;
        ImageLoader.loadImageTexture(gameShow.getImg(0), 150, 150, (tt) =>
        {
            if (tt != null && gameIc != null && !isDestroy)
            {
                gameIc.sprite = Sprite.Create(tt, new Rect(0, 0, tt.width, tt.height), new Vector2(0.5f, 0.5f));
            }
        });
        btClose.SetActive(false);
        Invoke("showClose", 3f);
    }

    void showClose()
    {
        btClose.SetActive(true);
    }

    private void OnEnable()
    {
        PuzzleControl.getInstance().onPuzzleImpression();
    }

    public void onClickClose()
    {
        PuzzleControl.getInstance().onPuzzleClose();
        gameObject.SetActive(false);
    }

    public void onClickInstall()
    {
        PuzzleControl.getInstance().onPuzzleClick();
        FIRhelper.logEvent("click_puzzle_other");

#if ENABLE_AppsFlyer
        Dictionary<string, string> ParamPromo = new Dictionary<string, string>();
        ParamPromo.Add("af_promo_appid", AppConfig.appid);
        AppsFlyerSDK.AppsFlyer.attributeAndOpenStore(gameShow.appid, "cross_promo_campaign", ParamPromo, mygame.sdk.AppsFlyerHelperScript.Instance);
#endif

        if (gameShow.link != null && gameShow.link.Length > 10)
        {
            GameHelper.Instance.gotoLink(gameShow.link);
        }
        else
        {
#if UNITY_IOS || UNITY_IPHONE
            GameHelper.Instance.gotoStore(gameShow.appid);
#else
            GameHelper.Instance.gotoStore(gameShow.pkg);
#endif
        }
    }

}