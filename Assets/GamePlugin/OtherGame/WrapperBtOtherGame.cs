using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mygame.sdk;

public class WrapperBtOtherGame : MonoBehaviour
{
    private long timeShoPromogame = 0;
    [SerializeField] private PromoGame gameOther;
    private PromoGameOb gameProCurr = null;
    bool isfirst = true;

    private void OnEnable()
    {
        if (!isfirst)
        {
            showPromoGame();
        }
    }

    void Start()
    {
        isfirst = false;
        showPromoGame();
    }

    #region gamePromo
    public void showPromoGame()
    {
        Debug.Log("mysdk: promo showPromoGame 0");
        gameOther.gameObject.SetActive(false);
        PromoGameOb gamePro = null;
        if (timeShoPromogame <= 0)
        {
            Debug.Log("mysdk: promo showPromoGame 10");
            gamePro = FIRhelper.Instance.getGamePromo();
            if (gamePro != null)
            {
                Debug.Log("mysdk: promo showPromoGame 100");
                timeShoPromogame = GameHelper.CurrentTimeMilisReal();
            }
        }
        else
        {
            Debug.Log("mysdk: promo showPromoGame 11");
            long t = GameHelper.CurrentTimeMilisReal();
            if ((t - timeShoPromogame) >= 90 * 1000)
            {
                Debug.Log("mysdk: promo showPromoGame 110");
                gamePro = FIRhelper.Instance.nextGamePromo();
                if (gamePro != null)
                {
                    Debug.Log("mysdk: promo showPromoGame 1100");
                    timeShoPromogame = t;
                }
                else
                {
                    Debug.Log("mysdk: promo showPromoGame 1101");
                    gamePro = gameProCurr;
                }
            }
            else
            {
                Debug.Log("mysdk: promo showPromoGame 1103");
                gamePro = gameProCurr;
            }
        }

        if (gamePro != null)
        {
            Debug.Log("mysdk: promo showPromoGame 2");
            string nameimg = ImageLoader.url2nameData(gamePro.getImg(0), 1);
            if (System.IO.File.Exists(DownLoadUtil.pathCache() + "/" + nameimg))
            {
                ImageLoader.Instance.loadTexttureFromCache(nameimg, 100, 100, (tt) =>
                {
                    if (tt != null)
                    {
                        gameProCurr = gamePro;
                        gameOther.intitGame(gameProCurr);
                        gameOther.show(ImageLoader.CreateSprite(tt));
                    }
                    else
                    {
                        PromoGameOb gamehasIcon = FIRhelper.Instance.getGameHasIcon();
                        if (gamehasIcon != null)
                        {
                            nameimg = ImageLoader.url2nameData(gamehasIcon.getImg(0), 1);
                            ImageLoader.Instance.loadTexttureFromCache(nameimg, 100, 100, (tt) =>
                            {
                                gameProCurr = gamehasIcon;
                                gameOther.intitGame(gameProCurr);
                                gameOther.show(ImageLoader.CreateSprite(tt));
                            });
                        }
                        else
                        {
                            gameProCurr = gamePro;
                            gameOther.intitGame(gameProCurr);
                            gameOther.show(null);
                        }
                    }
                });
            }
        }
    }
    #endregion
}
