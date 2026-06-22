using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using mygame.sdk;

public class PopupMyOpenAds : MonoBehaviour
{
    MyOpenAdsOb myads;
    public Button btClose;
    public Image imgCircle;
    public Image imgTick;
    public Image imgContent;

    Action<int> _cbShow;

    public void initAds(MyOpenAdsOb ad, Action<int> cbShow)
    {
        myads = ad;
        _cbShow = cbShow;
        if (myads != null && myads.texture != null)
        {
            imgContent.sprite = Sprite.Create(myads.texture, new Rect(0, 0, myads.texture.width, myads.texture.height), Vector2.zero);
        }
    }

    private void OnEnable()
    {
        btClose.enabled = false;
        StartCoroutine(waitEnableBtClose());
        if (_cbShow != null)
        {
            _cbShow(1);
        }
    }

    private void OnDisable()
    {
        if (_cbShow != null)
        {
            _cbShow(2);
            _cbShow = null;
        }
    }

    private void OnDestroy()
    {
        if (_cbShow != null)
        {
            _cbShow(2);
            _cbShow = null;
        }
    }

    IEnumerator waitEnableBtClose()
    {
        yield return new WaitForSeconds(3);
        btClose.enabled = true;
    }

    public void onclickAds()
    {
        gameObject.SetActive(false);
        if (myads != null && myads.gameId != null)
        {
            if (myads.gameId.StartsWith("http"))
            {
                GameHelper.Instance.gotoLink(myads.gameId);
            }
            else
            {
                GameHelper.Instance.gotoStore(myads.gameId);
            }
        }
    }

    public void onClickClose()
    {
        gameObject.SetActive(false);
    }
}
