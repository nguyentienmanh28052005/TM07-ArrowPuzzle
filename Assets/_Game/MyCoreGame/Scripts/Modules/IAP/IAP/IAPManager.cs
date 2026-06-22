using System;
using System.Collections.Generic;
using System.Linq;
using mygame.sdk;
using UnityEngine;
using time;
using UnityEngine.AddressableAssets;
using Newtonsoft.Json;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

namespace master
{
    [Serializable]
    public struct VisualPackReference
    {
        public PackType type;
        public AssetReferenceGameObject reference;
    }
    public class IAPManager : master.Singleton<IAPManager>  
    {
        public static string CF_PackData
        {
            get => PlayerPrefsBase.Instance().getString("cf_pack_data", "");
            set
            {
                try
                {
                    configPackDataServer = JsonConvert.DeserializeObject<ConfigPackDataServer[]>(value);
                    PlayerPrefsBase.Instance().setString("cf_pack_data", value);
                }
                catch
                {
                    PlayerPrefsBase.Instance().setString("cf_pack_data", "");
                    configPackDataServer = null;
                }
            }
        }

        [SerializeField] VisualPackReference[] visualReference;
        [SerializeField] ConfigPackDataSO configPackData;
        private static ConfigPackDataServer[] configPackDataServer;

        public Transform holdPopupPack;
        private BasePack CurrentPopup;
        private FactoryGetIAPComposition fac_getIAPComposition;
        private Dictionary<PackType, DataPackage> dataPackage;
        private int LevelGame = 0;
        protected void Awake()
        {
            base.Awake();
            try
            {
                configPackDataServer = JsonConvert.DeserializeObject<ConfigPackDataServer[]>(CF_PackData);
            }
            catch
            {
                PlayerPrefsBase.Instance().setString("cf_pack_data", "");
                configPackDataServer = null;
            }
            fac_getIAPComposition = new FactoryGetIAPComposition(this);
            loadDataPackages();
            RegisterListener();
        }
        private void OnDestroy()
        {
            RemoveListener();
        }
        public void RegisterListener()
        {

            InappHelper.SubCallback += OnReceiveSub;
            InappHelper.FakeSubDailyCallback += OnReceiveSub;
        }
        public void RemoveListener()
        {
            InappHelper.SubCallback -= OnReceiveSub;
            InappHelper.FakeSubDailyCallback -= OnReceiveSub;
        }

        public void ActiveAllPack()
        {
            foreach (var pack in configPackData.data)
            {
                ActivePack(pack.type);
            }
        }
        public void ActivePack(PackType packType)
        {
            if (GetTimeActive(packType) <= 0 && !IsMaxPurchase(packType))
            {
                SetTimeActive(packType, MGTime.GetUtcTime());
            }
        }
        public ConfigPackDataServer GetConfigServer(PackType packType)
        {
            if (configPackDataServer == null) return null;
            return configPackDataServer.SingleOrDefault(x => x.type == packType);
        }
        #region Base
        public void OnReceiveSub(string skuId, int daily)
        {
            var subData = GetConfigPackData(PackType.SubPack);
            if (subData != null)
            {
                var g = subData.groupData.SingleOrDefault(x => x.sku == skuId);
                if (g != null)
                {
                    GameEvent.OnReceiveSubcription?.Invoke(skuId);
                }
            }
        }
        private AsyncOperationHandle<GameObject> asyncOperationHandleGameObject;
        public async Task<T> ShowPopup<T>(PackType packType, string where) where T : BasePack
        {

            var config = configPackData.data.Single(x => x.type == packType);
            var configServer = GetConfigServer(packType);
            var reference = visualReference.Single(x => x.type == packType).reference;
            asyncOperationHandleGameObject = reference.InstantiateAsync(holdPopupPack);
            await asyncOperationHandleGameObject.Task;
            if (asyncOperationHandleGameObject.Status == AsyncOperationStatus.Succeeded)
            {
                var popup = asyncOperationHandleGameObject.Result.GetComponent<T>();
                popup.Initialize(this, config, configServer, where);
                popup.Show();
                CurrentPopup = popup;
                return popup;
            }
            else
            {
                return null;
            }
        }
        public void ReleaseCurrentPopup()
        {
            if (CurrentPopup != null)
            {
                Addressables.ReleaseInstance(CurrentPopup.gameObject);
            }
            if (asyncOperationHandleGameObject.IsValid())
            {
                Addressables.Release(asyncOperationHandleGameObject);
            }
        }
        public ConfigPackData GetConfigPackData(PackType type)
        {
            return configPackData.data.Single(x => x.type == type);
        }

        public static bool ComparePriorityPackData(ConfigPackData d1, ConfigPackData d2)
        {
            if (d1.timeActive <= 0 && d2.timeActive <= 0)
            {
                return d1.displayPriority > d2.displayPriority;
            }
            return d1.timeActive > d2.timeActive;
        }

        #region Data

        private void loadDataPackages()
        {
            dataPackage = new Dictionary<PackType, DataPackage>();
            foreach (PackType packType in System.Enum.GetValues(typeof(PackType)))
            {
                var d = SaveLoadUtil.LoadData<DataPackage>(getDataPackFileName(packType), false);
                if (d != null)
                {
                    dataPackage.Add(packType, d);
                }
            }
        }
        private void saveDataPackage(PackType type)
        {
            SaveLoadUtil.SaveData(dataPackage[type], getDataPackFileName(type), false);
        }
        private void setDefaultDataPackage(PackType type)
        {
            if (!dataPackage.ContainsKey(type))
            {
                var n = new DataPackage();
                dataPackage.Add(type, n);
            }
        }
        private string getDataPackFileName(PackType type)
        {
            return $"data_package_{(int)type}.pa";
        }
        public void AddPurchase(PackType type, int id, string sku)
        {
            SetNumOfPuschase(type, sku, 1, true);
        }
        private void salePack(ConfigPackData promoData, int level)
        {
            if (promoData.levelStartSale <= 0) return;
            if (promoData.levelStartSale <= level && !IsSale(promoData, level))
            {
                setTimeStartSale(promoData.type, MGTime.GetUtcTime());
            }
        }
        public bool IsActive(PackType packType)
        {
            var d = GetConfigPackData(packType);
            if (d == null) return false;
            return IsActive(d);
        }
        public bool IsActive(ConfigPackData promoData)
        {
            if (promoData.type == PackType.SubPack) return true;
            if (promoData.timeActive <= 0)
            {
                bool a = GetTimeActive(promoData.type) > 0;
                bool b = !IsMaxPurchase(promoData);
                return a && b;
            }
            if (IsMaxPurchase(promoData))
            {
                return false;
            }
            return (promoData.timeActive * 1000 + GetTimeActive(promoData.type)) > MGTime.GetUtcTime() && MGTime.GetUtcTime() >= GetTimeActive(promoData.type);
        }
        public bool IsSale(PackType packType)
        {
            var promoData = GetConfigPackData(packType);
            if (promoData == null) return false;
            return IsSale(promoData, LevelGame);
        }
        private bool IsSale(ConfigPackData promoData, int level)
        {
            if (promoData.levelStartSale <= 0) return false;
            if (promoData.levelStartSale <= level)
            {
                if (promoData.saleTime <= 0) return true;
                var timeStartSale = GetTimeStartSale(promoData.type);
                var curTime = MGTime.GetUtcTime();
                return (promoData.saleTime * 1000 + timeStartSale > curTime && curTime >= timeStartSale);
            }
            return false;
        }
        public bool IsMaxPurchase(PackType packType)
        {
            var d = GetConfigPackData(packType);
            if (d == null) return true;
            return IsMaxPurchase(d);
        }
        public bool IsMaxPurchase(ConfigPackData promoData)
        {
            if (promoData.maxNumOfPurchases <= 0) return false;
            return GetNumOfPurchase(promoData.type) >= promoData.maxNumOfPurchases;
        }
        public long GetTimeActive(PackType promoType)
        {
            if (!dataPackage.ContainsKey(promoType)) return 0;
            return dataPackage[promoType].time_active;
        }
        public void SetTimeActive(PackType promoType, long v)
        {
            setDefaultDataPackage(promoType);
            dataPackage[promoType].time_active = v;
            saveDataPackage(promoType);
        }
        public void RemovePack(PackType packType)
        {
            SetTimeActive(packType, 0);
        }
        public int GetNumOfPurchase(PackType promoType)
        {
            if (!dataPackage.ContainsKey(promoType)) return 0;
            return dataPackage[promoType].GetTotalPurchase();
        }
        public void SetNumOfPuschase(PackType promoType, string sku, int v = 1, bool isAdd = true)
        {
            setDefaultDataPackage(promoType);
            dataPackage[promoType].SetPurchase(sku, v, isAdd);
            saveDataPackage(promoType);
        }
        public long GetTimeStartSale(PackType promoType)
        {
            if (!dataPackage.ContainsKey(promoType)) return 0;
            return dataPackage[promoType].start_time_sale;
        }
        private void setTimeStartSale(PackType promoType, long v)
        {
            setDefaultDataPackage(promoType);
            dataPackage[promoType].start_time_sale = v;
            saveDataPackage(promoType);
        }
        #endregion
        #endregion
        #region Count Show Pack
        private class DataCountShowPack
        {
            public PackType type;
            public Dictionary<string, int> dicLevelShow;
        }
        private List<DataCountShowPack> countShowPack = new List<DataCountShowPack>();
        private string getKeyLevelShow(int pos, int level)
        {
            return $"{pos}_{level}";
        }
        public int GetShowAmountPackInSection(PackType pack, int pos, int level)
        {
            var idx = countShowPack.FindIndex(x => x.type == pack);
            if (idx < 0) return 0;
            var k = getKeyLevelShow(pos, level);
            if (countShowPack[idx].dicLevelShow.ContainsKey(k))
            {
                return countShowPack[idx].dicLevelShow[k];
            }
            return 0;
        }
        public void AddShowAmountPackInSection(PackType pack, int pos, int level, int value)
        {
            var idx = countShowPack.FindIndex(x => x.type == pack);
            if (idx < 0)
            {
                var data = new DataCountShowPack();
                data.type = pack;
                data.dicLevelShow = new Dictionary<string, int>();
                var k = getKeyLevelShow(pos, level);
                data.dicLevelShow.Add(k, value);
                countShowPack.Add(data);
            }
            else
            {
                var k = getKeyLevelShow(pos, level);
                if (countShowPack[idx].dicLevelShow.ContainsKey(k))
                {
                    countShowPack[idx].dicLevelShow[k] += value;
                }
                else
                {
                    countShowPack[idx].dicLevelShow.Add(k, value);
                }

            }
        }
        #endregion
        public T GetComponentIAP<T>()
        {
            return (T)fac_getIAPComposition.GetObj(typeof(T).Name);
        }
    }

}
public class DataPackage
{
    public long time_active = 0;
    public long start_time_sale = 0;
    [JsonProperty] private List<DataSku> data_sku;
    public int GetTotalPurchase()
    {
        if (data_sku == null) return 0;
        return data_sku.Sum(x => x.total_purchase);
    }
    public int GetTotalPurchase(string sku)
    {
        if (data_sku == null) return 0;
        var dataSkuIdx = data_sku.FindIndex(x => x.sku == sku);
        if (dataSkuIdx < 0) return 0;
        return data_sku[dataSkuIdx].total_purchase;
    }
    public void SetPurchase(string sku, int v, bool isAdd)
    {
        if (data_sku == null)
        {
            data_sku = new List<DataSku>();
        }
        var dataSkuIdx = data_sku.FindIndex(x => x.sku == sku);
        if (dataSkuIdx < 0)
        {
            var n = new DataSku()
            {
                sku = sku,
                total_purchase = v,
            };
            data_sku.Add(n);
        }
        else if (isAdd)
        {
            data_sku[dataSkuIdx].total_purchase += v;
        }
        else
        {
            data_sku[dataSkuIdx].total_purchase = v;
        }
    }
}
public class DataSku
{
    public string sku;
    public int total_purchase = 0;
}
