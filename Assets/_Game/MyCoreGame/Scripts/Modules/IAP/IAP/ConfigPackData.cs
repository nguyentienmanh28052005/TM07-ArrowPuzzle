using System;
using UnityEngine;
namespace master
{
    [System.Serializable]
    public class ConfigPackData
    {
        public string name;
        public PackType type;
        [Tooltip("Package display priority")]
        public byte displayPriority;
        [Tooltip("Maximum package purchase. If maxNumOfPurchases<=0, you can purchase permanently")]
        public int maxNumOfPurchases;
        [Tooltip("Time active package. If timeActive <=0 then package does not take time")]
        public long timeActive;
        [Tooltip("Level active")]
        public short lvActive;
        [Tooltip("Active on new day?")]
        public byte activeNewDay;
        [Tooltip("Active on start game?")]
        public byte activeOnStart;
        [Tooltip("Num of show pack in section. If showInSection <=0, not limit show in section")]
        public byte showInSection;
        [Tooltip("Level start sale. If levelStartSale <=0, not sale")]
        public int levelStartSale;
        [Tooltip("Time sale (seconds). If saleTime <=0, Permanent sale")]
        public int saleTime;
        [Tooltip("SkuId, Gift, X-Value, Sale-Value")]
        public GroupPackData[] groupData;
    }
    [System.Serializable]
    public class ConfigPackDataServer
    {
        public struct GroupGift
        {
            public DataResource[] gifts;
        }
        public PackType type;
        public short lvActive;
        public GroupGift[] groupGift;
    }
    [Serializable]
    public class GroupPackData
    {
        public string sku;
        public string saleSku;
        public string xValue;
        public string saleValue;
        public DataResource[] gifts;
    }
    public enum PackType
    {
        None = -1,
        StarterPack = 0,
        SubPack = 1,
        FailOffer = 2,
        BigBundle = 3,
        GreatBundle = 4,
        AddSlotRefillPack = 5,
        HandRefillPack = 6,
        ClearRefillPack = 7,
        ShuffleRefillPack = 8,
    }

}
