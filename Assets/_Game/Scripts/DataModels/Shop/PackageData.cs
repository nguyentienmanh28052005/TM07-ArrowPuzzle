using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using mygame.sdk;
using UnityEngine;

public enum BuyType
{
    None,
    Trade,
    Iap,
    Ads
}

[CreateAssetMenu(fileName = "PackageData", menuName = "Data/PackageData")]
public class PackageData : ScriptableObject
{
    public PackageInfo[] ticketPackInfos;
    public PackageInfo[] goldPackInfos;
    public PackageInfo[] packageInfos;
    // public PackageTier[] packageTierInfos;
    public PackageFlashSaleInfo[] packageFlashSaleInfos;

    public enum ResetType
    {
        None,
        Daily,
        Weekly,
        Monthly,
    }

    [Serializable]
    public class PackageInfo
    {
        public string skuId;
        public string tile;
        public string desc;
        public int id;
        public int count;
        public int levelUnlock;

        public bool isX2IfFirst;
        public bool onlyOnce;

        public RES_type priceType;
        public Sprite priceIcon;
        public int price;
        public int saleOff;

        public ResetType resetType;

        public BuyType buyType;

        public ItemBuyInfo[] allItems;

        private int buyCountInStep
        {
            get => PlayerPrefsBase.Instance().getInt("buy_count_in_step_" + skuId, 0);
            set => PlayerPrefsBase.Instance().setInt("buy_count_in_step_" + skuId, value);
        }

        public int buyAmount => count - buyCountInStep;

        public bool isBuy => levelUnlock < GameRes.GetLevel() && (count == 0 || buyCountInStep < count);

        public bool isActive => !onlyOnce || buyCountInStep == 0;

        private long lastBuyTime
        {
            get => long.Parse(PlayerPrefsBase.Instance().getString("last_buy_time" + skuId, "0"));
            set => PlayerPrefsBase.Instance().setString("last_buy_time" + skuId, value.ToString());
        }

        public void CheckReset()
        {
            if (lastBuyTime == 0 || count == 0) return;
            var dateTime = new DateTime(lastBuyTime, DateTimeKind.Local);
            switch (resetType)
            {
                case ResetType.Daily:
                    if (DateTime.Now.Day != dateTime.Day || DateTime.Now.Month != dateTime.Month || DateTime.Now.Year != dateTime.Year)
                    {
                        buyCountInStep = 0;
                        lastBuyTime = 0;
                    }

                    break;
                case ResetType.Weekly:
                    var now = DateTime.Now;
                    var last = dateTime;

                    if (StartOfWeek(now) != StartOfWeek(last))
                    {
                        buyCountInStep = 0;
                        lastBuyTime = 0;
                    }

                    break;
                case ResetType.Monthly:
                    if (DateTime.Now.Month != dateTime.Month || DateTime.Now.Year != dateTime.Year)
                    {
                        buyCountInStep = 0;
                        lastBuyTime = 0;
                    }
                    break;
            }
            
            static DateTime StartOfWeek(DateTime dt, DayOfWeek start = DayOfWeek.Monday)
            {
                int diff = (7 + (dt.DayOfWeek - start)) % 7;
                return dt.Date.AddDays(-diff); // 00:00 của ngày đầu tuần
            }

        }

        public void BuyPackage()
        {
            if (lastBuyTime == 0) lastBuyTime = DateTime.Now.Ticks;
            buyCountInStep++;
        }

        public TimeSpan ResetDuration()
        {
            var now = DateTime.Now;
            switch (resetType)
            {
                case ResetType.Daily:
                    var endOfDay = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
                    return endOfDay - now;
                case ResetType.Weekly:
                    var daysUntilEndOfWeek = DayOfWeek.Saturday - now.DayOfWeek + 1;
                    var endOfWeek = now.AddDays(daysUntilEndOfWeek).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    return endOfWeek - now;
                case ResetType.Monthly:
                    var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                    var endOfMonth = new DateTime(now.Year, now.Month, daysInMonth, 23, 59, 59);
                    return endOfMonth - now;
            }

            return TimeSpan.Zero;
        }
    }

    [Serializable]
    public class ItemBuyInfo : ItemInfo
    {
        public Sprite previewIcon;

        public ItemBuyInfo(Sprite ic, RES_type rwType, int am) : base(ic, rwType, am)
        {
        }
    }

    public PackageInfo FindPackage(int id)
    {
        return packageInfos.SingleOrDefault(x => x.id == id);
    }

    [Serializable]
    public class PackageFlashSaleInfo
    {
        public string skuId;
        public RES_type boosterType;
        public Sprite spriteBooster;
        public ItemInfo[] itemInfos;
    }

    // [System.Serializable]
    // public class PackageTierInfo
    // {
    //     public int idTier;
    //     public PackageInfo packageAll;
    //     public PackageInfo option1;
    //     public PackageInfo option2;
    //     public PackageInfo option3;
    // }
    //
    // [System.Serializable]
    // public class PackageTierData
    // {
    //     public PackageTierInfo tier1;
    //     public PackageTierInfo tier2;
    //     public PackageTierInfo tier3;
    // }
    // [System.Serializable]
    // public class PackageTier
    // {
    //     public int id;
    //     public PackageTierData packageInfo;
    //     public PackageTierInfo[] tier;
    //     public ItemBuyInfo itemBonus;
    // }
}


[Serializable]
public class ItemInfo
{
    public RES_type itemType;
    [SerializeField] private Sprite itemIcon;
    public int itemAmount;

    public string itemName;

    public Sprite Icon
    {
        get
        {
            if (itemIcon == null)
            {
                itemIcon = DataManager.Instance.GetIcon(itemType);
            }

            return itemIcon;
        }
    }


    public ItemInfo(Sprite ic, RES_type rwType, int am, string name = "")
    {
        itemIcon = ic;
        itemType = rwType;
        itemAmount = am;
        itemName = name;
    }

    public ItemInfo(ItemInfo itemInf)
    {
        itemIcon = itemInf.itemIcon;
        itemType = itemInf.itemType;
        itemAmount = itemInf.itemAmount;
        itemName = itemInf.itemName;
    }

    public ItemInfo()
    {
    }

    public DataResource ToDataResource()
    {
        DataResource dataResource = new DataResource();
        dataResource.amount = itemAmount;
        if (itemType == RES_type.UnlimitedHeart)
        {
            dataResource.amount = itemAmount;
        }

        dataResource.resType = itemType;
        dataResource.icon = itemIcon;
        return dataResource;
    }

    public ItemInfo CopyFromDataResource(DataResource dataResource)
    {
        this.itemAmount = dataResource.amount;
        if (itemType == RES_type.UnlimitedHeart)
        {
            itemAmount = dataResource.amount;
        }

        itemType = dataResource.resType;
        itemIcon = dataResource.icon;
        return this;
    }
}