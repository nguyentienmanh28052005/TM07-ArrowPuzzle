using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResUIImp : ResUIBase
{
    [SerializeField] Text textValue;
    [SerializeField] Image icon;
    private DataResource dataResource;

    public override Text TextValue
    {
        get => textValue;
    }

    public override Image Icon
    {
        get => icon;
    }

    public override bool CanVisual(DataResource data)
    {
        return true;
    }

    public override DataResource GetData()
    {
        return dataResource;
    }

    public override void Init(DataResource data, Action OnClickRes)
    {
        if (textValue)
        {
            if (data.resType == mygame.sdk.RES_type.UnlimitedHeart ||
                data.resType == mygame.sdk.RES_type.UnlimitedMutilColorBox ||
                data.resType == mygame.sdk.RES_type.UnlimitedMagnet || data.resType == mygame.sdk.RES_type.DoubleReward)
            {
                textValue.text = VisualTextForUnlimitedHeart(data.amount);
            }
            else if (data.resType != mygame.sdk.RES_type.GOLD)
            {
                textValue.text = "x" + data.amount.ToString();
            }
            else
            {
                textValue.text = (data.amount < 10000)
                    ? data.amount.ToString()
                    : ((float)data.amount / 1000).ToString() + "k";
            }
        }

        if (data.GetIdIcon() != 0)
        {
            icon.sprite = SpriteResourceManager.Instance.GetIcon(data.GetIdIcon());
        }
        else icon.sprite = data.icon;

        dataResource = data;
        OnClickRes?.Invoke();
    }

    public override void VisualTextAmount(int value)
    {
        textValue.text = value.ToString();
    }

    protected override void SetUp()
    {
    }

    private string VisualTextForUnlimitedHeart(int seconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        string timeString = "";

        if (timeSpan.Days > 0)
        {
            timeString = timeSpan.Days + "d";
        }

        if (timeSpan.Hours > 0)
        {
            string tmp = timeSpan.Hours + "h";
            if (timeString != "") timeString = timeString + ":" + tmp;
            else timeString = tmp;
        }

        if (timeSpan.Minutes > 0)
        {
            string tmp = timeSpan.Minutes + "m";
            if (timeString != "") timeString = timeString + ":" + tmp;
            else timeString = tmp;
        }

        if (timeSpan.Seconds > 0)
        {
            string tmp = timeSpan.Seconds + "s";
            if (timeString != "") timeString = timeString + ":" + tmp;
            else timeString = tmp;
        }

        return timeString;
    }
}