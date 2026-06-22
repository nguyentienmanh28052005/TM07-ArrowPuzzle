using master;
using mygame.sdk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using time;

public class ComIAP_ControlPack
{
    public readonly RES_type[] BoosterPacks = new RES_type[]
    {
        RES_type.Hand,
        RES_type.Shuffle,
        RES_type.Clear
    };
    private readonly Dictionary<RES_type, int> LevelActivePackFlashSale = new Dictionary<RES_type, int>()
    {
        { RES_type.Hand,3},
        { RES_type.Shuffle,4},
        { RES_type.Clear,5},
    };

    private static long lastTimeActivePackBooster
    {
        get => long.Parse(PlayerPrefs.GetString("last_time_active_pack_booster", "0"));
        set => PlayerPrefs.SetString("last_time_active_pack_booster", value.ToString());
    }
    private static RES_type lastPackBoosterActive
    {
        get => (RES_type)PlayerPrefs.GetInt("last_pack_booster_active", (int)RES_type.Hand);
        set => PlayerPrefs.SetInt("last_pack_booster_active", (int)value);
    }
    public static int CountShowPackFlashSale
    {
        get => PlayerPrefs.GetInt("count_show_pack_flash_sale", 100);
        set => PlayerPrefs.SetInt("count_show_pack_flash_sale", value);
    }
    public static int CountShowPackWeekend
    {
        get => PlayerPrefs.GetInt("count_show_pack_weekend", 100);
        set => PlayerPrefs.SetInt("count_show_pack_weekend", value);
    }

    public void Initialized()
    {

    }
    public void ActiveBoosterPack()
    {
        var boosterAmount = GameRes.getRes(RES_type.Hand);
        if (boosterAmount <= 0)
        {
            ActivePack(RES_type.Hand);
        }
        boosterAmount = GameRes.getRes(RES_type.Shuffle);
        if (boosterAmount <= 0)
        {
            ActivePack(RES_type.Shuffle);
        }
        boosterAmount = GameRes.getRes(RES_type.Clear);
        if (boosterAmount <= 0)
        {
            ActivePack(RES_type.Clear);
        }
    }
    public bool IsActivePackBooster(RES_type boosterType)
    {
        if (PlayerPrefsUtil.CF_ActivePackToReview)
        {
            if (IsBuyPack(boosterType)) return false;
            ActivePack(boosterType);
            return true;
        }
        var curTime = MGTime.GetUtcTime();
        if (curTime - lastTimeActivePackBooster >= 1800000 || !IsActivePack(lastPackBoosterActive))
        {
            var l = new List<RES_type>();
            if (IsActivePack(RES_type.Hand))
            {
                l.Add(RES_type.Hand);
            }
            if (IsActivePack(RES_type.Shuffle))
            {
                l.Add(RES_type.Shuffle);
            }
            if (IsActivePack(RES_type.Clear))
            {
                l.Add(RES_type.Clear);
            }
            if (l.Count == 0) return false;
            var idx = l.FindIndex(x => x != lastPackBoosterActive);
            if (idx >= 0)
            {
                lastPackBoosterActive = l[idx];
                lastTimeActivePackBooster = curTime;
                CountShowPackFlashSale = 100;
                master.Observer.Notify(ObserverName.flash_sale_active, 1);
            }
            else
            {
                lastPackBoosterActive = l[0];
                lastTimeActivePackBooster = curTime;
                master.Observer.Notify(ObserverName.flash_sale_active, 1);
            }
        }
        return boosterType == lastPackBoosterActive && IsActivePack(boosterType);
    }
    private bool IsActivePack(RES_type boosterType)
    {
        if (PlayerPrefsUtil.CF_ActivePackToReview)
        {
            return !IsBuyPack(boosterType);
        }
        if (!DataManager.Instance.IsUsedBooster(boosterType) && GameRes.GetLevel() < LevelActivePackFlashSale[boosterType]) return false;
        if (IsBuyPack(boosterType)) return false;
        return PlayerPrefs.GetInt("is_active_pack_flash_sale_" + boosterType, 0) > 0;
    }
    private void ActivePack(RES_type boosterType)
    {
        if (IsBuyPack(boosterType)) return;
        PlayerPrefs.SetInt("is_active_pack_flash_sale_" + boosterType, 1);
    }
    public bool IsBuyPack(RES_type boosterType)
    {
        return PlayerPrefs.GetInt("count_buy_pack_flash_sale_" + boosterType, 0) > 0;
    }
    public void BuyPack(RES_type boosterType)
    {
        PlayerPrefs.SetInt("count_buy_pack_flash_sale_" + boosterType, 1);
    }
}
