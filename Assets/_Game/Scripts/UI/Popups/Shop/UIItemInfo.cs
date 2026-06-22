using System;
using System.Collections;
using System.Collections.Generic;
using mygame.sdk;
using UnityEngine;
using UnityEngine.UI;

public class UIItemInfo : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text amount;
    public GameObject infinitySymbol;

    protected ItemInfo dataInfo;

    public void Initialized(Sprite ic, int am)
    {
        if (icon)
        {
            icon.sprite = ic;
            icon.rectTransform.offsetMin = new Vector2(0f, icon.rectTransform.offsetMin.y);
            icon.rectTransform.offsetMax = new Vector2(0f, icon.rectTransform.offsetMax.y);
        }

        amount.text = $"x{am}";
    }

    public void SetUpData(ItemInfo data)
    {
        dataInfo = data;
    }

    public void Initialized(RES_type resType, Sprite ic, int am)
    {
        if (icon)
        {
            icon.sprite = ic;
            if (resType == RES_type.Clear)
            {
                icon.rectTransform.offsetMin = new Vector2(-10f, icon.rectTransform.offsetMin.y);
                icon.rectTransform.offsetMax = new Vector2(10f, icon.rectTransform.offsetMax.y);
            }
            else
            {
                icon.rectTransform.offsetMin = new Vector2(0f, icon.rectTransform.offsetMin.y);
                icon.rectTransform.offsetMax = new Vector2(0f, icon.rectTransform.offsetMax.y);
            }
        }

        if (resType == RES_type.UnlimitedHeart)
        {
            if (am >= 3600)
            {
                if (am % 3600 == 0)
                {
                    amount.text = $"{am / 3600}H";
                }
                else
                {
                    amount.text = $"{am / 3600}H{am % 3600 / 60}M";
                }
            }
            else
            {
                amount.text = $"{am / 60}M";
            }

            amount.gameObject.SetActive(false);
        }
        else
        {
            amount.text = $"x{am}";
            amount.gameObject.SetActive(true);
        }
    }
}