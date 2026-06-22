//#define ENABLE_INAPP
//#define ENABLE_ADJUST
//#define ENABLE_QONVERSION

using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Myapi;
#if ENABLE_ADJUST
using com.adjust.sdk;
#endif
using System.Linq;
#if ENABLE_INAPP
using System.Collections.ObjectModel;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
#endif

namespace mygame.sdk 
{
#if ENABLE_INAPP
    public class InappHelper : MonoBehaviour
    {
#else
    public class InappHelper : MonoBehaviour
    {
#endif
        public static InappHelper Instance { get; private set; }

        public static event Action<int> OnProductsFetchedCB = null;
        public static event Action<int> OnPurchasesFetchedCB = null;

        public static event Action<string> OnRestoredPurchased = null;

        public static event Action<string> OnHandlePurchasePending = null;

        public static event Action<string, int> SubCallback = null;

        public static event Action<string, int> FakeSubDailyCallback = null;

        public static event Action<string> OnPackagePurchased = null;

        public Action<string> onPurchaseSuccess;

#if ENABLE_INAPP
#if ENABLE_AdInMo && ADINMO_UNITY_STORE_V5
        Adinmo.AdinmoStoreController m_StoreController;
#else
        StoreController m_StoreController;
#endif
        private CrossPlatformValidator m_CrossPlatformValidator;
        private Product productValidbyAppsflyer = null;
        private Dictionary<string, PurchasePendingConfirmOb> mDicPendingOrderCheckConfirmed = new Dictionary<string, PurchasePendingConfirmOb>();

#endif
        public HandleSub _handleSub;
        public InappCountryOb listCurr { get; private set; }
        public Dictionary<string, InappCountryOb> listAll = new Dictionary<string, InappCountryOb>();
        private string buyWhere = "";
        private DateTime mPurchaseDate;
        int countTryCheckFakeSub = 0;
        private bool isSan4Va = false;
        bool isSanbox4Sub = false;
        public string CurrencyCode { get; private set; }
        public static int isPurchase { get { return PlayerPrefs.GetInt("iap_isPurchase", 0); } }
        private Action<PurchaseCallback> _callback;
        short CurrStatePurchase = 0;//0-none, 1-purchasing
        short flagConfirmPending = 0;
        long tCurrPurchase = 0;
        public short isGetProductsInfo { get; private set; }
        private string memSkuFailAndCheckFetched = "";
        private byte statusInit = 0;
        private string memSkubuyWaitInit = "";
        private string transactionIDBuy = "";

        public static int CountPurchase
        {
            get => PlayerPrefs.GetInt("inapp_count_purchase", 0);
            set => PlayerPrefs.SetInt("inapp_count_purchase", value);
        }
        public static float TotalPurchase
        {
            get => PlayerPrefs.GetFloat("inapp_total_purchase", 0f);
            set => PlayerPrefs.SetFloat("inapp_total_purchase", value);
        }

        public static long LastTimePurchase
        {
            get => long.Parse(PlayerPrefs.GetString("last_time_purchase", "0"));
            set => PlayerPrefs.SetString("last_time_purchase", value.ToString());
        }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                CurrStatePurchase = 0;
                tCurrPurchase = 0;
                isGetProductsInfo = 0;
                statusInit = 0;
                CreateServices();
            }
            else
            {
                // if (this != Instance) Destroy(gameObject);
            }
        }

        void Start()
        {
            initInApp();
        }

        #region IAP for init
        void CreateServices()
        {
#if ENABLE_INAPP
#if ENABLE_AdInMo && ADINMO_UNITY_STORE_V5
            m_StoreController = new Adinmo.AdinmoStoreController();
#else
            m_StoreController = UnityIAPServices.StoreController();
#endif

            m_StoreController.OnStoreConnected += OnStoreConnected;
            m_StoreController.OnStoreDisconnected += OnStoreDisconnected;

            m_StoreController.OnProductsFetched += OnProductsFetched;
            m_StoreController.OnProductsFetchFailed += OnProductsFetchFailed;

            m_StoreController.OnPurchasesFetched += OnPurchasesFetched;
            m_StoreController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
            m_StoreController.OnPurchasePending += OnPurchasePending;
            m_StoreController.OnPurchaseConfirmed += OnPurchaseConfirmed;
            m_StoreController.OnCheckEntitlement += OnCheckEntitlement;
            m_StoreController.OnPurchaseFailed += OnPurchaseFailed;
            m_StoreController.OnPurchaseDeferred += OnPurchaseDeferred;

#if ENABLE_QONVERSION
            QonversionUnity.QonversionConfig config = new QonversionUnity.QonversionConfigBuilder(
                AppConfig.QONConversionPKey, 
                QonversionUnity.LaunchMode.Analytics)
                .SetEnvironment(QonversionUnity.Environment.Production)//
                .Build();
            QonversionUnity.Qonversion.Initialize(config);
#endif

#endif
        }

#if ENABLE_INAPP
        async void InitializeIapService()
        {
            try
            {
                var options = new InitializationOptions()
                    .SetEnvironmentName("production");

                await UnityServices.InitializeAsync(options);
            }
            catch (Exception exception)
            {
                Debug.Log($"mysdk: IAP InitializeIapService ex={exception}");
            }
        }

        void CreateCrossPlatformValidator()
        {
#if !UNITY_EDITOR
            try
            {
                m_CrossPlatformValidator = new CrossPlatformValidator(GooglePlayTangle.Data(), Application.identifier);
            }
            catch (NotImplementedException exception)
            {
                Debug.Log($"mysdk: IAP Cross Platform Validator Not Implemented: {exception}");
            }
#endif
        }

        async void ConnectToStore()
        {
            Debug.Log("mysdk: IAP Store Connecting to store.");
            statusInit = 0;
            await m_StoreController.Connect();
            statusInit = 1;
            Debug.Log($"mysdk: IAP Store Connected.");
            initData();
            fetchInitialProducts();
        }
#endif

        void savePrice()
        {
            try
            {
                string dataPrice = "{";
                string pathPrice = Application.persistentDataPath + "/files/mem_iap_price.txt";
                bool isbegin = true;
                foreach (var item in listCurr.listWithSku)
                {
                    if (isbegin)
                    {
                        isbegin = false;
                        dataPrice += ",";
                    }
                    dataPrice += $"\"{item.Value.sku}\":\"{item.Value.price}\"";
                }
                dataPrice += "}";
                File.WriteAllText(pathPrice, dataPrice);
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: IAP ex=" + ex.ToString());
            }
        }

        void getMemPrice()
        {
            string dataPrice = "";
            string pathPrice = Application.persistentDataPath + "/files/mem_iap_price.txt";
            if (File.Exists(pathPrice))
            {
                dataPrice = File.ReadAllText(pathPrice);
                var listmemPrice = (IDictionary<string, object>)MyJson.JsonDecoder.DecodeText(dataPrice);
                if (listmemPrice != null || listmemPrice.Count > 0)
                {
                    foreach (KeyValuePair<string, object> itemdata in listmemPrice)
                    {
                        if (listCurr.listWithSku.ContainsKey(itemdata.Key))
                        {
                            listCurr.listWithSku[itemdata.Key].price = (string)itemdata.Value;
                        }
                    }
                }
            }
        }

        void handleFakeSub(string skuid, int dayRcv)
        {
            if (FakeSubDailyCallback != null)
            {
                FakeSubDailyCallback(skuid, dayRcv);
            }
        }

        void checkTime4FakeSub()
        {
            Debug.Log("mysdk: IAP checkTime4FakeSub");
            countTryCheckFakeSub++;
            foreach (var skuitem in listCurr.listWithId)
            {
                if (skuitem.Value.typeInapp == 0 && skuitem.Value.periodSub > 0 && skuitem.Value.dayPurchased > 0)
                {
                    if (SDKManager.Instance.timeOnline <= 0)
                    {
                        Myapi.ApiManager.Instance.getTimeOnline((status, time) =>
                        {
                            if (status)
                            {
                                SDKManager.Instance.timeOnline = (int)(time / 60000);
                                SDKManager.Instance.timeWhenGetOnline = (int)(GameHelper.CurrentTimeMilisReal() / 60000);
                                checkFakeSub();
                            }
                            else
                            {
                                if (countTryCheckFakeSub <= 3)
                                {
                                    Invoke("checkTime4FakeSub", 60);
                                }
                            }
                        });
                    }
                    else
                    {
                        checkFakeSub();
                    }
                    break;
                }
            }
        }

        void checkFakeSub()
        {
            DateTime nd = SdkUtil.timeStamp2DateTime((long)SDKManager.Instance.timeOnline * 60);
            int ncday = nd.Year * 365 + nd.DayOfYear;
            Debug.Log($"mysdk: IAP checkFakeSub curr day={ncday}:{nd}");
            foreach (var skuitem in listCurr.listWithId)
            {
                if (skuitem.Value.typeInapp == 0 && skuitem.Value.periodSub > 0 && skuitem.Value.dayPurchased > 0)
                {
                    int dd = ncday - skuitem.Value.dayPurchased;
                    if (dd >= skuitem.Value.periodSub)
                    {
                        Debug.Log($"mysdk: IAP checkFakeSub pack:{skuitem.Value.id} daybuy={skuitem.Value.dayPurchased} out of date");
                        skuitem.Value.dayPurchased = 0;
                        PlayerPrefsBase.Instance().setInt($"iap_{skuitem.Value.id}_daypur", 0);
                    }
                    else
                    {
                        int drcvmem = PlayerPrefsBase.Instance().getInt($"iap_{skuitem.Value.id}_dayrcvpur", 0);
                        if (drcvmem < ncday)
                        {
                            Debug.Log($"mysdk: IAP checkFakeSub pack:{skuitem.Value.id} daybuy={skuitem.Value.dayPurchased} rcv day={dd}");
                            skuitem.Value.countRcvDaily++;
                            skuitem.Value.dayRcvFakeSub = ncday;
                            PlayerPrefsBase.Instance().setInt($"iap_{skuitem.Value.id}_dayrcv", skuitem.Value.countRcvDaily);
                            PlayerPrefsBase.Instance().setInt($"iap_{skuitem.Value.id}_dayrcvpur", ncday);

                            handleFakeSub(skuitem.Value.id, dd + 1);
                        }
                        else
                        {
                            Debug.Log($"mysdk: IAP checkFakeSub pack:{skuitem.Value.id} daybuy={skuitem.Value.dayPurchased} dayrcv={drcvmem} has rcv");
                        }
                    }
                }
            }
        }
        private PurchasePendingConfirmOb isHasTransIdWithSku(string sku)
        {
#if ENABLE_INAPP
            foreach (var ietm in mDicPendingOrderCheckConfirmed)
            {
                if (ietm.Value.sku.CompareTo(sku) == 0)
                {
                    return ietm.Value;
                }
            }
#endif
            return null;
        }
        #endregion

        #region PUBLIC METHODS
        public void initInApp()
        {
#if ENABLE_INAPP
            InitializeIapService();
            CreateCrossPlatformValidator();

            ConnectToStore();
#endif
        }

        public bool BuyPackage(string skuid, string where, Action<PurchaseCallback> cb)
        {
#if CHECK_4INAPP
            if (!SDKManager.Instance.isDeviceTest())
            {
                int ss2 = PlayerPrefsBase.Instance().getInt("mem_kt_jvpirakt", 0);
                int ss1 = PlayerPrefsBase.Instance().getInt("mem_kt_cdtgpl", 0);
                int rsesss = PlayerPrefsBase.Instance().getInt("mem_procinva_gema", 3);
                if (rsesss != 1 && rsesss != 2 && rsesss != 3 && rsesss != 101 && rsesss != 102 && rsesss != 103 && rsesss != 1985)
                {
                    rsesss = 103;
                }
                if ((rsesss == 3 && (ss1 == 1 || ss2 == 1)) || (rsesss == 1 && ss1 == 1) || (rsesss == 2 && ss2 == 1))
                {
                    SDKManager.Instance.showNotSupportIAP();
                    FIRhelper.logEvent($"game_invalid_iap1_{ss1}_{ss2}");
                    return false;
                }
            }
#endif
            string realSku = "";
            buyWhere = where;
            if (listCurr.listWithId.ContainsKey(skuid))
            {
                realSku = listCurr.listWithId[skuid].sku;
            }
            else
            {
                if (skuid != null)
                {
                    Debug.LogError("mysdk: IAP BuyPackage err " + skuid);
                }

                return false;
            }

            long t = GameHelper.CurrentTimeMilisReal() / 1000;
            if (CurrStatePurchase != 0)
            {
                if ((t - tCurrPurchase) < 1200)
                {
                    Debug.LogError("mysdk: IAP BuyPackage err in pre processing");
                    return false;
                }
            }
            _callback = cb;

#if ENABLE_TEST_INAPP
        if (realSku.Length > 0) {
            Debug.Log("mysdk: IAP test inapp" + realSku);
            PlayerPrefs.SetInt("iap_isPurchase", 1);
            getRewardInapp(realSku);
            if (_callback != null)
            {
                PurchaseCallback pcb = new PurchaseCallback(1, realSku);
                _callback(pcb);
                _callback = null;
            }
            return true;
        } else {
            return false;
        }
        
#endif
#if ENABLE_INAPP
            string skulog = realSku.Replace('.', '_');
            if (skulog.Length > 25)
            {
                skulog = skulog.Substring(skulog.Length - 25);
            }
            FIRhelper.logEvent($"IAP_click_{skulog}");
            Product product = FindProduct(realSku);
            if (product != null && product.availableToPurchase)
            {
                tCurrPurchase = t;
                CurrStatePurchase = 1;
                transactionIDBuy = "";
                SDKManager.Instance.showWaitCommon();
                var obpen = isHasTransIdWithSku(realSku);
                if (obpen != null)
                {
                    Debug.Log($"mysdk: IAP BuyPackage product:{product.definition.id} but has transectionID -> ConfirmPurchase and buy");
                    flagConfirmPending = 1;
                    memSkuFailAndCheckFetched = realSku;
                    m_StoreController.ConfirmPurchase(obpen.order);
                }
                else
                {
                    Debug.Log($"mysdk: IAP BuyPackage PurchaseProduct: {product.definition.id}");
                    flagConfirmPending = 0;
                    m_StoreController.PurchaseProduct(product);
                }
                return true;
            }
            else
            {
                if (isGetProductsInfo != 1 && Application.internetReachability != NetworkReachability.NotReachable)
                {
                    memSkubuyWaitInit = realSku;
                    SDKManager.Instance.showWaitCommon();
                    if (statusInit == 0)
                    {
                        Debug.LogError($"mysdk: IAP BuyPackage: FAIL, not get info. ConnectToStore and try buy {memSkubuyWaitInit}");
                        ConnectToStore();
                    }
                    else
                    {
                        Debug.LogError($"mysdk: IAP BuyPackage: FAIL, not get info. fetchInitialProducts and try buy {memSkubuyWaitInit}");
                        fetchInitialProducts();
                    }
                }
                else
                {
                    Debug.LogError($"mysdk: IAP BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
                return false;
            }
#else
            return false;
#endif
        }

        public bool BuySubscription(string skuid, string where, Action<PurchaseCallback> cb)
        {
            if (!_handleSub.hasSub(skuid))
            {
                return BuyPackage(skuid, where, cb);
            }
            else
            {
                return false;
            }
        }

        public void RestorePurchases(Action<bool, string> callback)
        {
#if ENABLE_INAPP
            Debug.Log("mysdk: IAP RestorePurchases Click");
            callback += OnTransactionsRestored;
            m_StoreController.RestoreTransactions(callback);
#endif

        }

        public int GetPurchaseDate(string sku)
        {
            return listCurr.listWithId[sku].dayPurchased;
        }

        public void handleSub(string skuid, int dayRcv)
        {
            Debug.Log($"mysdk: IAP handleSub skuid={skuid} dayRcv={dayRcv}");
            if (listCurr.listWithId.ContainsKey(skuid))
            {
                PkgObject obPur = listCurr.listWithId[skuid];
                InappRcvObject rcv = obPur.rcv;
                if (rcv.removeads >= 1)
                {
                    AdsHelper.setRemoveAds(rcv.removeads);
                }
            }
            if (SubCallback != null)
            {
                SubCallback(skuid, dayRcv);
            }
        }

        public bool isExpireSub(string skuid)
        {
#if ENABLE_INAPP
            if (listCurr.listWithId.ContainsKey(skuid))
            {
                if (listCurr.listWithId[skuid].typeInapp == 2)
                {
                    return _handleSub.isExpired(skuid);
                }
            }
            return true;
#else
            return true;
#endif
        }

        public string getSkuIdBySku(string sku)
        {
            if (listCurr.listWithSku.ContainsKey(sku))
            {
                return listCurr.listWithSku[sku].id;
            }
            else
            {
                return sku;
            }
        }
        public decimal getDecimalPrice(string skuid)
        {
#if ENABLE_INAPP
            if (m_StoreController != null)
            {
                if (listCurr.listWithId.ContainsKey(skuid))
                {
                    string realSku = listCurr.listWithId[skuid].sku;
                    Product product = FindProduct(realSku);
                    if (product != null)
                    {
                        return product.metadata.localizedPrice;
                    }
                }
            }

            return 0;
#endif
            return 0;
        }

        public string getPrice(string skuid)
        {
#if UNITY_EDITOR
            if (listCurr == null || listCurr.listWithId == null) return "";
            if (listCurr.listWithId.ContainsKey(skuid))
            {
                return listCurr.listWithId[skuid].price;
            }
#elif ENABLE_INAPP
            if (m_StoreController != null)
            {
                if (listCurr.listWithId.ContainsKey(skuid))
                {
                    string realSku = listCurr.listWithId[skuid].sku;
                    Product product = FindProduct(realSku);
                    if (product != null)
                    {
                        return product.metadata.localizedPriceString;
                    } 
                    else 
                    {
                        if (listCurr.listWithId.ContainsKey(skuid))
                        {
                            return listCurr.listWithId[skuid].price;
                        }
                    }
                }
            }
            else 
            {
                if (listCurr.listWithId.ContainsKey(skuid))
                {
                    return listCurr.listWithId[skuid].price;
                }
            }
#endif
            return "";
        }

        public string getCurrency(string skuid)
        {
#if UNITY_EDITOR
            if (listCurr.listWithId.ContainsKey(skuid))
            {
                return listCurr.listWithId[skuid].currency;
            }
#elif ENABLE_INAPP
            if (m_StoreController != null)
            {
                if (listCurr.listWithId.ContainsKey(skuid))
                {
                    string realSku = listCurr.listWithId[skuid].sku;
                    Product product = FindProduct(realSku);
                    if (product != null)
                    {
                        return product.metadata.isoCurrencyCode;
                    } 
                    else 
                    {
                        if (listCurr.listWithId.ContainsKey(skuid))
                        {
                            return listCurr.listWithId[skuid].currency;
                        }
                    }
                }
            }
            else 
            {
                if (listCurr.listWithId.ContainsKey(skuid))
                {
                    return listCurr.listWithId[skuid].price;
                }
            }
#endif
            return "";
        }

        public decimal getPriceDecimal(string skuid)
        {
#if UNITY_EDITOR
            if (listCurr.listWithId.ContainsKey(skuid))
            {
                return listCurr.listWithId[skuid].vip;
            }
#elif ENABLE_INAPP
            if (m_StoreController != null)
            {
                if (listCurr.listWithId.ContainsKey(skuid))
                {
                    string realSku = listCurr.listWithId[skuid].sku;
                    Product product = FindProduct(realSku);
                    if (product != null)
                    {
                        return product.metadata.localizedPrice;
                    }
                    else
                    {
                        if (listCurr.listWithId.ContainsKey(skuid))
                        {
                            return listCurr.listWithId[skuid].vip;
                        }
                    }
                }
            }
            else
            {
                if (listCurr.listWithId.ContainsKey(skuid))
                {
                    return listCurr.listWithId[skuid].vip;
                }
            }
#endif
            return 0;
        }

        public int getDayReward(string skuid)
        {
            if (listCurr.listWithId.ContainsKey(skuid))
            {
                return listCurr.listWithId[skuid].periodSub;
            }
            return 0;
        }

        public InappRcvObject getReceiver(string skuId)
        {
            if (listCurr.listWithId.ContainsKey(skuId))
            {
                InappRcvObject rcv = listCurr.listWithId[skuId].rcv;
                return rcv;
            }
            return null;
        }

        public int getMoneyRcv(string skuId, string key)
        {
            if (listCurr.listWithId.ContainsKey(skuId))
            {
                InappRcvObject rcv = listCurr.listWithId[skuId].rcv;
                return rcv.getMoney(key);
            }
            return 0;
        }

        public int getItemRcv(string skuId, string key)
        {
            if (listCurr.listWithId.ContainsKey(skuId))
            {
                InappRcvObject rcv = listCurr.listWithId[skuId].rcv;
                return rcv.getItem(key);
            }
            return 0;
        }

        public InappRcvItemsObject getEquipmentRcv(string skuId, string key)
        {
            if (listCurr.listWithId.ContainsKey(skuId))
            {
                InappRcvObject rcv = listCurr.listWithId[skuId].rcv;
                return rcv.getEquipment(key);
            }
            return null;
        }

        public bool getRemoveAllAds(string skuId)
        {
            if (listCurr.listWithId.ContainsKey(skuId))
            {
                InappRcvObject rcv = listCurr.listWithId[skuId].rcv;
                if ((rcv.removeads & 6) > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool getRemoveAdsFull(string skuId)
        {
            if (listCurr.listWithId.ContainsKey(skuId))
            {
                InappRcvObject rcv = listCurr.listWithId[skuId].rcv;

                return (rcv.removeads > 0);
            }
            return false;
        }

        public int getPointVip(string skuId)
        {
            if (listCurr.listWithId.ContainsKey(skuId))
            {
                return listCurr.listWithId[skuId].vip;
            }
            return 0;
        }

        public void onclickPolicy()
        {
            Application.OpenURL(AppConfig.urlPolicy);
        }

        public void onclickTermsofuse()
        {
            Application.OpenURL(AppConfig.urlPolicy);
        }
        #endregion

#if ENABLE_INAPP
        #region Private METHODS
        private void initData()
        {
            string data = "";
            string path = Application.persistentDataPath + "/files/cf_ina_ga.txt";
            Debug.Log($"mysdk: IAP fetchInitialProducts path={path}");
            if (File.Exists(path))
            {
                data = File.ReadAllText(path);
            }

            if (data == null || data.Length < 10)
            {
                Debug.Log($"mysdk: IAP fetchInitialProducts read default");
#if UNITY_ANDROID
                TextAsset txt = (TextAsset)Resources.Load("Inapp/Android/data", typeof(TextAsset));
#else
                TextAsset txt = (TextAsset)Resources.Load("Inapp/iOS/data", typeof(TextAsset));
#endif
                data = txt.text;
            }

            InappUtil.parserDataSkus(data, listAll);
            string countrycode = PlayerPrefs.GetString("mem_countryCode", "");
            countrycode = CountryCodeUtil.convertToCountryCode(countrycode);
            if (listAll.ContainsKey(countrycode))
            {
                listCurr = listAll[countrycode];
            }
            else
            {
                listCurr = listAll[GameHelper.CountryDefault];
            }
        }
        private void reBuyAffterFetchedProducts(string realSku)
        {
            Product product = FindProduct(realSku);
            if (product != null && product.availableToPurchase)
            {
                tCurrPurchase = GameHelper.CurrentTimeMilisReal() / 1000; ;
                CurrStatePurchase = 1;
                SDKManager.Instance.showWaitCommon();
                var obpen = isHasTransIdWithSku(realSku);
                if (obpen != null)
                {
                    Debug.Log($"mysdk: IAP reBuyAffterFetchedProducts ConfirmPurchase: {product.definition.id}");
                    flagConfirmPending = 1;
                    memSkuFailAndCheckFetched = realSku;
                    m_StoreController.ConfirmPurchase(obpen.order);
                }
                else
                {
                    Debug.Log($"mysdk: IAP reBuyAffterFetchedProducts PurchaseProduct: {product.definition.id}");
                    flagConfirmPending = 0;
                    m_StoreController.PurchaseProduct(product);
                }
            }
            else
            {
                Debug.Log($"mysdk: IAP reBuyAffterFetchedProducts product not availableToPurchase");
                SDKManager.Instance.PopupShowFirstAds.gameObject.SetActive(false);
                if (_callback != null)
                {
                    PurchaseCallback pcb = new PurchaseCallback(0, realSku);
                    _callback(pcb);
                    _callback = null;
                }
            }
        }
        private void fetchInitialProducts()
        {
            //getMemPrice();

            var initialProductsToFetch = new List<ProductDefinition>();
            foreach (var skuitem in listCurr.listWithId)
            {
                ProductType tpr = ProductType.Consumable;
                if (skuitem.Value.typeInapp == 0)
                {
                    tpr = ProductType.Consumable;
                }
                else if (skuitem.Value.typeInapp == 1)
                {
                    tpr = ProductType.NonConsumable;
                }
                else if (skuitem.Value.typeInapp == 2)
                {
                    tpr = ProductType.Subscription;
                }
                Debug.Log($"mysdk: IAP fetchInitialProducts sku={skuitem.Value.sku} -type={tpr}");
                ProductDefinition product = new ProductDefinition(skuitem.Value.sku, tpr);
                initialProductsToFetch.Add(product);
            }
            m_StoreController.FetchProducts(initialProductsToFetch);
        }
        private void FetchExistingPurchases()
        {
            m_StoreController.FetchPurchases();
        }
        Product GetFirstProductInOrder(Order order)
        {
            return order.CartOrdered.Items().FirstOrDefault()?.Product;
        }
        bool ValidatePurchase(IOrderInfo orderInfo)
        {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX)
            try
            {
                var result = m_CrossPlatformValidator.Validate(orderInfo.Receipt);
                foreach (IPurchaseReceipt productReceipt in result)
                {
                    mPurchaseDate = productReceipt.purchaseDate;
                }
                return true;
            }
            catch (IAPSecurityException ex)
            {
                return false;
            }
#else
            mPurchaseDate = SdkUtil.timeStamp2DateTime(GameHelper.CurrentTimeMilisReal() / 1000);
            return true;
#endif
        }
        bool IsReceiptAvailable(Orders existingOrders)
        {
            return existingOrders != null &&
                   (existingOrders.ConfirmedOrders.Any(order => !string.IsNullOrEmpty(order.Info.Receipt)) ||
                    existingOrders.PendingOrders.Any(order => !string.IsNullOrEmpty(order.Info.Receipt)));
        }
        Product FindProduct(string productId)
        {
            return GetFetchedProducts()?.FirstOrDefault(product => product.definition.id == productId);
        }
        ReadOnlyObservableCollection<Product> GetFetchedProducts()
        {
            return m_StoreController.GetProducts();
        }
        #endregion

        #region Process Purcahse
        void handleFetchedPurchsed(Orders existingOrders)
        {
            Debug.Log($"mysdk: IAP handleFetchedPurchsed");
            countTryCheckFakeSub = 0;
            _handleSub.checkStartSub();
            checkTime4FakeSub();
            bool isHanSub = false;
            foreach (ConfirmedOrder confirmedOder in existingOrders.ConfirmedOrders)
            {
                Debug.Log($"mysdk: IAP handleFetchedPurchsed ConfirmedOrders");
                if (confirmedOder.Info != null && confirmedOder.Info.Receipt != null)
                {
                    var isPurchaseValid = ValidatePurchase(confirmedOder.Info);
                    if (isPurchaseValid)
                    {
                        bool issan = isIAPTest(confirmedOder.Info.Receipt, false);
                        bool isValideAf = AFValidatePurchase(issan);
                        if (!isValideAf)
                        {
                            Debug.Log($"mysdk: IAP handleFetchedPurchsed is check ok");
                            foreach (IPurchasedProductInfo purchaseInfo in confirmedOder.Info.PurchasedProductInfo)
                            {
                                if (purchaseInfo.subscriptionInfo != null)
                                {
                                    Debug.Log($"mysdk: IAP handleFetchedPurchsed sub productId={purchaseInfo.productId} pdate={purchaseInfo.subscriptionInfo.GetPurchaseDate()}, pe={purchaseInfo.subscriptionInfo.GetExpireDate()}, pr={purchaseInfo.subscriptionInfo.GetRemainingTime()}");
                                    isHanSub = true;
                                    _handleSub.ReceiveSubscriptionProduct(purchaseInfo.subscriptionInfo, listCurr.listWithSku[purchaseInfo.productId]);
                                }
                                else
                                {
                                    Debug.Log($"mysdk: IAP handleFetchedPurchsed restore productId={purchaseInfo.productId} ");
                                    var product = GetFirstProductInOrder(confirmedOder);
                                    if (product != null)
                                    {
                                        if (listCurr.listWithSku.ContainsKey(product.definition.id))
                                        {
                                            PkgObject obPur = listCurr.listWithSku[product.definition.id];
                                            int status = PlayerPrefsBase.Instance().getInt($"iap_{obPur.id}_memp", 0);
                                            if (status == 0)
                                            {
                                                PlayerPrefs.SetInt("iap_isPurchase", 1);
                                                getRewardInapp(product);
                                                if (OnRestoredPurchased != null)
                                                {
                                                    OnRestoredPurchased(obPur.id);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (PendingOrder pendingOrder in existingOrders.PendingOrders)
            {
                Debug.Log($"mysdk: IAP handleFetchedPurchsed PendingOrders");
                if (pendingOrder.Info != null && pendingOrder.Info.Receipt != null)
                {
                    //OnPurchasePending(pendingOrder);
                }
            }
            foreach (DeferredOrder deferredOrder in existingOrders.DeferredOrders)
            {
                Debug.Log($"mysdk: IAP handleFetchedPurchsed DeferredOrders");
                OnPurchaseDeferred(deferredOrder);
            }
            if (CurrStatePurchase == 1)
            {
                Debug.Log($"mysdk: IAP handleFetchedPurchsed err follow buy!!!!!!!!!!!!");
                SDKManager.Instance.PopupShowFirstAds.gameObject.SetActive(false);
                CurrStatePurchase = 0;
                _callback = null;
            }
            if (!isHanSub)
            {
                _handleSub.checkHandSub();
            }
            _handleSub.checkHasSubContainRemoveAds();
        }
        private void handlePurchaseSuc(Product p, bool issss = false)
        {
            Debug.Log($"mysdk: IAP handlePurchaseSuc: {p.definition.id}");
            SDKManager.Instance.PopupShowFirstAds.gameObject.SetActive(false);
#if ENABLE_QONVERSION
            QonversionUnity.Qonversion.GetSharedInstance().SyncStoreKit2Purchases();
#endif
            if (!SDKManager.Instance.isDeviceTest() && !issss)
            {
                if (CurrStatePurchase == 1)
                {
                    float per = PlayerPrefs.GetInt("cf_per_post_iap", AppConfig.PerValuePostIAP);
                    float vapost = per * ((float)p.metadata.localizedPrice) / 100.0f;
                    Dictionary<string, object> iapParam = new Dictionary<string, object>();
                    iapParam.Add("item_id", p.definition.id);
#if UNITY_IOS || UNITY_IPHONE
                    float firPer = PlayerPrefs.GetInt("cf_per_postfir_iap", AppConfig.PerValuePostFirIAP);
                    float firVapost = firPer * ((float)p.metadata.localizedPrice) / 100.0f;
                    iapParam.Add("value", firVapost);
                    iapParam.Add("price", firVapost);
#else
                    iapParam.Add("value", vapost);
                    iapParam.Add("price", vapost);
#endif
                    iapParam.Add("currency", p.metadata.isoCurrencyCode);
                    //iapParam.Add("transaction_id", p.transactionID);

                    FIRhelper.logEvent($"IAP_suc_purchase");
#if UNITY_IOS || UNITY_IPHONE
                    FIRhelper.logEvent("purchase", iapParam);
#endif

                    string skulog = p.definition.id.Replace('.', '_');
                    if (skulog.Length > 25)
                    {
                        skulog = skulog.Substring(skulog.Length - 25);
                    }
                    FIRhelper.logEvent($"IAP_suc_{skulog}");
                    if (AppsFlyerHelperScript.Instance.checkSameSource("fb") && SDKManager.Instance.mediaCampain.StartsWith("iap_"))
                    {
                        FIRhelper.logEvent($"IAP_suc_fb_{skulog}");
                    }
#if (UNITY_ANDROID) || UNITY_IOS || UNITY_IPHONE
                    if (per >= 50)
                    {
                        string svapost = vapost.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
                        if (svapost.Contains(","))
                        {
                            svapost = svapost.Replace(",", ".");
                        }
                        AppsFlyerHelperScript.logPurchase(p.definition.id, svapost, p.metadata.isoCurrencyCode);
                    }
#endif
                    Myapi.LogEventApi.Instance().logInApp(p.definition.id, p.transactionID, "", p.metadata.isoCurrencyCode, (float)vapost, buyWhere);
                    Dictionary<string, string> dicparam = new Dictionary<string, string>();
                    dicparam.Add("producId", p.definition.id);
                    AdjustHelper.LogEvent(AdjustEventName.Inapp, dicparam);
                }
            }
            bool isHandlePending = false;
            if (CurrStatePurchase != 1)
            {
                isHandlePending = true;
            }
            CurrStatePurchase = 0;
            PlayerPrefs.SetInt("iap_isPurchase", 1);
            getRewardInapp(p);
            
            CountPurchase++;
            float usdPrice = 0f;
            if (listCurr != null && listCurr.listWithSku != null && listCurr.listWithSku.TryGetValue(p.definition.id, out PkgObject pkgOb))
            {
                usdPrice = InappUtil.ParsePriceUSD(pkgOb.price);
            }
            TotalPurchase += usdPrice;

            if (_callback != null)
            {
                PurchaseCallback pcb = new PurchaseCallback(1, p.definition.id);
                pcb.transactionID = transactionIDBuy;
                pcb.isSandbox = issss;
                _callback(pcb);
                _callback = null;
            }
            OnPackagePurchased?.Invoke(p.definition.id);
            onPurchaseSuccess?.Invoke(p.definition.id);
            LastTimePurchase = time.MGTime.GetUtcTime();
            if (isHandlePending)
            {
                Debug.Log($"mysdk: IAP handlePurchaseSuc product pending: {p.definition.id}");
                if (OnHandlePurchasePending != null)
                {
                    if (listCurr.listWithSku.ContainsKey(p.definition.id))
                    {
                        PkgObject obPur = listCurr.listWithSku[p.definition.id];
                        OnHandlePurchasePending(obPur.id);
                    }
                }
            }
        }
        private void handlePurchaseFaild(string skuId, string err)
        {
            Debug.Log($"mysdk: IAP handlePurchaseFaild: ProductId:={skuId}', PurchaseFailureReason:={err}");
            SDKManager.Instance.PopupShowFirstAds.gameObject.SetActive(false);
            if (CurrStatePurchase == 1)
            {
                string skulog = skuId.Replace('.', '_');
                if (skulog.Length > 25)
                {
                    skulog = skulog.Substring(skulog.Length - 25);
                }
                FIRhelper.logEvent($"IAP_fail_{skulog}");
            }
            CurrStatePurchase = 0;

            if (_callback != null)
            {
                PurchaseCallback pcb = new PurchaseCallback(0, skuId);
                _callback(pcb);
                _callback = null;
            }
        }
        private void getRewardInapp(Product pro)
        {
            if (listCurr.listWithSku.ContainsKey(pro.definition.id))
            {
                PkgObject obPur = listCurr.listWithSku[pro.definition.id];
                if (obPur.typeInapp == 2)
                {
                    InappRcvObject rcv = obPur.rcv;
                    if (rcv.removeads >= 1)
                    {
                        AdsHelper.setRemoveAds(rcv.removeads);
                    }
                    _handleSub.onBuySubSuccess(obPur, pro);
                }
                else
                {
                    if (obPur.typeInapp == 1)
                    {
                        PlayerPrefsBase.Instance().setInt($"iap_{obPur.id}_memp", 1);
                    }
                    if (obPur.periodSub > 0)
                    {
                        obPur.countRcvDaily = 1;
                        obPur.dayPurchased = mPurchaseDate.Year * 365 + mPurchaseDate.DayOfYear;
                        PlayerPrefsBase.Instance().setInt($"iap_{obPur.id}_dayrcv", obPur.countRcvDaily);
                        PlayerPrefsBase.Instance().setInt($"iap_{obPur.id}_daypur", obPur.dayPurchased);
                        PlayerPrefsBase.Instance().setInt($"iap_{obPur.id}_dayrcvpur", obPur.dayPurchased);
                        Debug.Log($"mysdk: IAP getRewardInapp day buy={mPurchaseDate} daycount={obPur.dayPurchased}");
                        handleFakeSub(obPur.id, 1);
                    }
                    InappRcvObject rcv = obPur.rcv;
                    if (rcv.removeads >= 1)
                    {
                        AdsHelper.setRemoveAds(rcv.removeads);
                    }

                    int va = rcv.getMoney("gold");
                    if (va > 0)
                    {
                        //add gold
                    }

                    var items = rcv.getItems();
                    if (items != null)
                    {
                        //for (int i = 0; i < items.Count; i++)
                        //{
                        //    KeyValuePair<string, int> key = items.ElementAt(i);
                        //    ItemData.ItemName itemName = ItemData.ItemName.ItemIronHammer;
                        //    if (Enum.TryParse(key.Key, out itemName))
                        //    {
                        //        //success
                        //        PlayerInfo.SetItemAmount(itemName, key.Value);
                        //    }
                        //    else
                        //    {
                        //        Debug.Log($"Parse enum fail: {key.Key} : {key.Value}");
                        //        continue;  //fail
                        //    }
                        //}
                    }
                }
            }
            else
            {
                Debug.Log("mysdk: IAP buy success but list not contain sku = " + pro.definition.id);
            }
        }
        private bool isIAPTest(string receipt, bool isFromPending)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            var wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
            var payload = (string)wrapper["Payload"];
            var payloadWrapper = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
            var json = (string)payloadWrapper["json"];
            var googlePurchase = (Dictionary<string, object>)MiniJson.JsonDecode(json);
            string tranId = (string)googlePurchase["orderId"];
            Debug.Log($"mysdk: IAP isIAPTest orderId={tranId}");
            if (isFromPending)
            {
                transactionIDBuy = tranId;
            }
            if (googlePurchase.ContainsKey("purchaseType"))
            {
                var purchaseType = Convert.ToInt32(googlePurchase["purchaseType"]);
                if (purchaseType == 1)
                {
                    return true;
                }
            }
#endif
            return false;
        }
        #endregion

        #region AfValide
        bool AFValidatePurchase(bool issan)
        {
#if !UNITY_EDITOR && ENABLE_AppsFlyer && AppsFlyer_IAPConnector

#if UNITY_ANDROID
                if (PlayerPrefsBase.Instance().getInt("is_vali_appsf", 1) == 1)
                {
                    Debug.Log("mysdk: IAP AFValidatePurchase android do validate");
                    AppsFlyerHelperScript.Instance.setIapConnectorCB((statusAppsfiap) => {
                        if (statusAppsfiap)
                        {
                            handlePurchaseSuc(args.purchasedProduct, issan);
                        }
                        else
                        {
                            handlePurchaseFaild(args.purchasedProduct, "AppsFlyer validPurchase faild");
                        }
                    });
                    return true;
                }
                else
                {
                    Debug.Log("mysdk: IAP AFValidatePurchase android not validate");
                    return false;
                }
#elif UNITY_IOS || UNITY_IPHONE
                Debug.Log("mysdk: IAP AFValidatePurchase iOS not validate");
                return false;
#endif 

#else
            return false;
#endif
        }
        #endregion

        #region IMPLEMENTION IStoreListener
        private void OnStoreConnected()
        {
            Debug.Log($"mysdk: IAP OnStoreConnected");
        }
        private void OnStoreDisconnected(StoreConnectionFailureDescription description)
        {
            Debug.Log($"mysdk: IAP OnStoreDisconnected details: {description.message}");
        }
        private void OnProductsFetched(List<Product> products)
        {
            Debug.Log($"mysdk: IAP OnProductsFetched:");
            isGetProductsInfo = 1;
            if (products.Count > 0)
            {
                foreach (var product in products)
                {
                    Debug.Log($"mysdk: IAP Fetched sku={product.definition.id} price={product.metadata.localizedPriceString}");
                    CurrencyCode = product.metadata.isoCurrencyCode;

                    if (listCurr.listWithSku.ContainsKey(product.definition.id))
                    {
                        if (listCurr.listWithSku[product.definition.id].memPrice == 1)
                        {
                            PlayerPrefs.SetString(product.definition.id, product.metadata.localizedPriceString);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("mysdk: IAP No Products Fetched.");
            }
            if (OnProductsFetchedCB != null)
            {
                OnProductsFetchedCB(1);
            }
            FetchExistingPurchases();
            if (memSkubuyWaitInit != null && memSkubuyWaitInit.Length > 5)
            {
                reBuyAffterFetchedProducts(memSkubuyWaitInit);
                memSkubuyWaitInit = "";
            }
        }
        private void OnProductsFetchFailed(ProductFetchFailed failure)
        {
            string reson = "";
            if (failure != null)
            {
                reson = failure.FailureReason;
            }
            Debug.Log($"mysdk: IAP OnProductsFetchFailed: {reson}");
            if (isGetProductsInfo != 1)
            {
                isGetProductsInfo = -1;
            }
            if (failure.FailedFetchProducts != null && failure.FailedFetchProducts.Count > 0)
            {
                foreach (var product in failure.FailedFetchProducts)
                {
                    Debug.Log($"mysdk: IAP FailedFetchProducts sku={product.id}");
                }
            }
            if (OnProductsFetchedCB != null)
            {
                OnProductsFetchedCB(0);
                OnProductsFetchedCB = null;
            }
            if (memSkubuyWaitInit != null && memSkubuyWaitInit.Length > 5)
            {
                SDKManager.Instance.PopupShowFirstAds.gameObject.SetActive(false);
                if (_callback != null)
                {
                    PurchaseCallback pcb = new PurchaseCallback(0, memSkubuyWaitInit);
                    _callback(pcb);
                    _callback = null;
                }
                memSkubuyWaitInit = "";
            }
        }
        private void OnPurchasesFetched(Orders existingOrders)
        {
            Debug.Log($"mysdk: IAP OnPurchasesFetched:");
            if (IsReceiptAvailable(existingOrders))
            {
                handleFetchedPurchsed(existingOrders);
            }
            else
            {
                if (CurrStatePurchase == 1)
                {
                    Debug.Log($"mysdk: IAP OnPurchasesFetched and handlePurchaseFaild");
                    handlePurchaseFaild(memSkuFailAndCheckFetched, "");
                }
            }
            if (OnPurchasesFetchedCB != null)
            {
                OnPurchasesFetchedCB(1);
                OnPurchasesFetchedCB = null;
            }
        }
        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            Debug.Log($"mysdk: IAP OnPurchasesFetchFailed:");
            if (OnPurchasesFetchedCB != null)
            {
                OnPurchasesFetchedCB(0);
                OnPurchasesFetchedCB = null;
            }
            if (CurrStatePurchase == 1)
            {
                Debug.Log($"mysdk: IAP OnPurchasesFetchFailed -> handlePurchaseFaild");
                handlePurchaseFaild(memSkuFailAndCheckFetched, "DuplicateTransecssion");
            }
        }
        private void OnTransactionsRestored(bool success, string error)
        {
            Debug.Log($"mysdk: IAP OnTransactionsRestored={success} error={error}");
        }
        private void OnPurchasePending(PendingOrder order)
        {
            var product = GetFirstProductInOrder(order);
            if (product is null)
            {
                Debug.Log($"mysdk: IAP OnPurchasePending Could not find product in order. Unable to validate receipt or grant purchase.");
                m_StoreController.ConfirmPurchase(order);
                return;
            }
            var isPurchaseValid = ValidatePurchase(order.Info);
            if (isPurchaseValid)
            {
                bool issan = isIAPTest(order.Info.Receipt, true);
                bool isValideAf = AFValidatePurchase(issan);
                if (!isValideAf)
                {
                    Debug.Log($"mysdk: IAP OnPurchasePending purchase ok productid={product.definition.id}");
                    if (listCurr.listWithSku[product.definition.id] == null && listCurr.listWithSku[product.definition.id].typeInapp == 2)
                    {
                        Debug.Log($"mysdk: IAP OnPurchasePending sub CheckEntitlement");
                        isSanbox4Sub = issan;
                    }
                    else
                    {
                        string mmemTanbuy = PlayerPrefsBase.Instance().getString("mem_tran_buy_pending", "");
                        string[] arrtanbuy = mmemTanbuy.Split("@@");
                        string newMem = "";
                        bool isRev = false;
                        foreach (var item in arrtanbuy)
                        {
                            if (!item.Contains(order.Info.TransactionID))
                            {
                                if (newMem.Length < 1)
                                {
                                    newMem = item;
                                }
                                else
                                {
                                    newMem += "@@" + item;
                                }
                            }
                            else
                            {
                                isRev = true;
                            }
                        }
                        PlayerPrefsBase.Instance().setString("mem_tran_buy_pending", newMem);
                        if (!isRev || CurrStatePurchase == 1)
                        {
                            handlePurchaseSuc(product, issan);
                        }
                        else
                        {
                            Debug.Log($"mysdk: IAP OnPurchasePending {order.Info.TransactionID} has receiver reward");
                        }
                    }
                }
                else
                {
                    Debug.Log($"mysdk: IAP OnPurchasePending AFValidatePurchase wait");
                }
            }
            else
            {
                Debug.Log($"mysdk: IAP OnPurchasePending Invalid receipt");
                handlePurchaseFaild(product.definition.id, "not valid");
            }
            Debug.Log($"mysdk: IAP OnPurchasePending add {order.Info.TransactionID} to dic check pending confirm count={mDicPendingOrderCheckConfirmed.Count + 1}");
            PurchasePendingConfirmOb obCheck = new PurchasePendingConfirmOb();
            obCheck.order = order;
            obCheck.sku = product.definition.id;
            mDicPendingOrderCheckConfirmed.Add(order.Info.TransactionID, obCheck);
            string tranBuy = $"{product.definition.id};;{order.Info.TransactionID}";
            string memTanbuy = PlayerPrefsBase.Instance().getString("mem_tran_buy_pending", "");
            if (memTanbuy == null || memTanbuy.Length < 2)
            {
                memTanbuy = tranBuy;
            }
            else
            {
                memTanbuy += "@@" + tranBuy;
            }
            PlayerPrefsBase.Instance().setString("mem_tran_buy_pending", memTanbuy);
            m_StoreController.ConfirmPurchase(order);
        }
        private void OnPurchaseConfirmed(Order order)
        {
            bool iscfok = false;
            switch (order)
            {
                case FailedOrder failedOrder:
                    Debug.Log($"mysdk: IAP OnPurchaseConfirmed={failedOrder}");
                    OnConfirmationFailed(failedOrder);
                    break;
                case ConfirmedOrder confirmedOrder:
                    Debug.Log($"mysdk: IAP OnPurchaseConfirmed={confirmedOrder}");
                    iscfok = true;
                    OnConfirmationOk(confirmedOrder);
                    break;
                default:
                    Debug.Log($"mysdk: IAP OnPurchaseConfirmed Unknown result.");
                    break;
            }
            var product = GetFirstProductInOrder(order);
            if (product != null)
            {
                m_StoreController.CheckEntitlement(product);
            }
            if (flagConfirmPending == 1 && product != null)
            {
                Debug.Log($"mysdk: IAP OnPurchaseConfirmed -> PurchaseProduct");
                m_StoreController.PurchaseProduct(product);
            }
            else if (flagConfirmPending == 1)
            {
                SDKManager.Instance.PopupShowFirstAds.gameObject.SetActive(false);
            }
            flagConfirmPending = 0;
        }
        void OnCheckEntitlement(Entitlement entitlement)
        {
            Debug.Log($"mysdk: IAP OnCheckEntitlement: status={entitlement.Status} p={entitlement.Product}");
            if (entitlement.Product != null && entitlement.Product.definition != null && listCurr.listWithSku[entitlement.Product.definition.id].typeInapp == 2)
            {
                switch (entitlement.Status)
                {
                    case EntitlementStatus.FullyEntitled:
                        // isSub
                        handlePurchaseSuc(entitlement.Product, isSanbox4Sub);
                        break;
                    default:
                        // not sub
                        handlePurchaseFaild(entitlement.Product.definition.id, entitlement.Status.ToString());
                        break;
                }
            }
        }
        private void OnPurchaseFailed(FailedOrder failedOrder)
        {
            var product = GetFirstProductInOrder(failedOrder);
            string skuId = "";
            if (product == null)
            {
                Debug.Log($"mysdk: IAP OnPurchaseFailed Could not find product in failed order, reason={failedOrder.FailureReason}");
            }
            else
            {
                Debug.Log($"mysdk: IAP OnPurchaseFailed product={product.definition.id} reason={failedOrder.FailureReason}");
                skuId = product.definition.id;
            }
            if (failedOrder.FailureReason == PurchaseFailureReason.DuplicateTransaction && product != null)
            {
                if (CurrStatePurchase == 1)
                {
                    Debug.Log($"mysdk: IAP OnPurchaseFailed {failedOrder.FailureReason} and check product");
                    FetchExistingPurchases();
                }
                else
                {
                    Debug.Log($"mysdk: IAP OnPurchaseFailed {failedOrder.FailureReason} and not check product finish buy");
                    handlePurchaseFaild(skuId, failedOrder.FailureReason.ToString());
                }
            }
            else
            {
                Debug.Log($"mysdk: IAP OnPurchaseFailed finish buy");
                handlePurchaseFaild(skuId, failedOrder.FailureReason.ToString());
            }
        }
        private void OnPurchaseDeferred(DeferredOrder deferredOrder)
        {
            foreach (var cartItem in deferredOrder.CartOrdered.Items())
            {
                Debug.Log($"mysdk: IAP OnPurchaseDeferred: cartItem.Product={cartItem.Product}");
            }
        }
        private void OnConfirmationFailed(FailedOrder failedOrder)
        {
            var reason = failedOrder.FailureReason;
            foreach (var cartItem in failedOrder.CartOrdered.Items())
            {
                Debug.Log($"mysdk: IAP OnConfirmationFailed: Product={cartItem.Product} {reason}");
            }
        }
        private void OnConfirmationOk(ConfirmedOrder order)
        {
            if (mDicPendingOrderCheckConfirmed.ContainsKey(order.Info.TransactionID))
            {
                Debug.Log($"mysdk: IAP OnConfirmationOk: remove {order.Info.TransactionID} from dic check pending confirm {mDicPendingOrderCheckConfirmed.Count - 1}");
                mDicPendingOrderCheckConfirmed.Remove(order.Info.TransactionID);
            }
            else
            {
                Debug.Log($"mysdk: IAP OnConfirmationOk: not contain {order.Info.TransactionID} from dic check pending confirm {mDicPendingOrderCheckConfirmed.Count}");
            }
            string memTanbuy = PlayerPrefsBase.Instance().getString("mem_tran_buy_pending", "");
            string[] arrtanbuy = memTanbuy.Split("@@");
            string newMem = "";
            foreach (var item in arrtanbuy)
            {
                if (!item.Contains(order.Info.TransactionID))
                {
                    if (newMem.Length < 1)
                    {
                        newMem = item;
                    }
                    else
                    {
                        newMem += "@@" + item;
                    }
                }
            }
            PlayerPrefsBase.Instance().setString("mem_tran_buy_pending", newMem);
            foreach (var cartItem in order.CartOrdered.Items())
            {
                var product = cartItem.Product;
                Debug.Log($"mysdk: IAP OnConfirmationOk: product={product} {order.Info}");
            }
        }
        #endregion

#endif
    }
}
