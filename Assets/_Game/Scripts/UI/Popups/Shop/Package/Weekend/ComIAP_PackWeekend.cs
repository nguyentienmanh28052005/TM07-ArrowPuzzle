using master;
using System.Collections;
using System.Collections.Generic;
using time;
using UnityEngine;
using System;
using mygame.sdk;


public class ComIAP_PackWeekend
{
    private const string FILE_PACK_WEEKEND = "pack_weekend.aic";
    private DataPackWeekend data;

    private bool IsActivePackWeekend
    {
        get => GameRes.GetLevel() >= PlayerPrefsUtil.CF_LevelUnlockPackWeekend;
    }

    public void Initialized()
    {
        loadData();
    }

    public long TimeStart => data.timeStart;
    public long TimeActive => 172800000;

    public bool IsPackActive()
    {
        if (PlayerPrefsUtil.CF_ActivePackToReview)
        {
            return true;
        }

        if (!IsActivePackWeekend)
        {
            return false;
        }

        var curTime = MGTime.GetUtcTime();
        return curTime >= data.timeStart && curTime <= (data.timeStart + TimeActive);
    }

    public bool IsPurchase()
    {
        return data.timeEventPurchase.Contains(data.timeStart);
    }

    public void ActivePack()
    {
        var curTime = MGTime.GetUtcTime();
        if (PlayerPrefsUtil.CF_ActivePackToReview && GameManager.ReasonBackToHome != ReasonBackToHome.OpenGame)
        {
            data.timeStart = curTime;
        }
        else
        {
            data.timeStart = MGTime.StartOfWeek(curTime, System.DayOfWeek.Saturday);
        }

        Observer.Notify(ObserverName.active_pack_weekend, true);
        saveData();
    }

    public void AddPurchase()
    {
        data.timeEventPurchase.Add(data.timeStart);
        saveData();
    }

    private void loadData()
    {
        data = SaveLoadUtil.LoadData<DataPackWeekend>(FILE_PACK_WEEKEND, false, TypeSave.Encryption);
        if (data == null)
        {
            data = new DataPackWeekend();
        }
    }

    private void saveData()
    {
        SaveLoadUtil.SaveData<DataPackWeekend>(data, FILE_PACK_WEEKEND, false, TypeSave.Encryption);
    }
}

public class DataPackWeekend
{
    public long timeStart;
    public List<long> timeEventPurchase = new List<long>();
}