using System;
using mygame.sdk;
using time;
using UnityEngine;
using UnityEngine.UI;

namespace master
{
    public abstract class BasePack : MonoBehaviour
    {
        [Serializable]
        public class GroupPack
        {
            [Space, Header("___PRICE___")] public Text txtPrice;
            public Text txtOldPrice;
            public Button buyButton;
            [Space, Header("___X VALUE___")] public Text txtXValue;
            public GameObject objXValue;
            [Space, Header("___SALE___")] public Text txtSaleValue;
            public GameObject objSaleValue;
            [Space, Header("___VIP___")] public Text txtVipPoint;
            public GameObject objVip;
            [Space, Header("___ADS___")] public GameObject objRemoveAds;
            public GameObject objRemoveAdsFull;
        }

        [SerializeField] protected Button btnClose;
        [SerializeField] protected Text txtTitle;
        [SerializeField] protected Text txtTime;
        [SerializeField] protected GameObject holdTime;
        [SerializeField] protected GroupPack[] packs;
        [SerializeField] protected string prefixVip;
        [SerializeField] protected string suffixesVip;

        protected ConfigPackData ConfigPackData;
        protected ConfigPackDataServer ConfigPackDataServer;
        protected string where;
        protected bool hasSale;
        protected IAPManager iapManager;
        protected bool isDispose = false;

        private void Start()
        {
            for (int i = 0; i < packs.Length; i++)
            {
                var i1 = i;
                packs[i1].buyButton.onClick.AddListener(() => OnClickBuy(i1));
            }

            if (btnClose != null)
            {
                btnClose.onClick.AddListener(() => { Hide(); });
            }
        }

        public virtual void Show()
        {
            isDispose = false;
            gameObject.SetActive(true);
        }

        public virtual void ReInitialize()
        {
            if (ConfigPackData == null || !iapManager.IsActive(ConfigPackData.type))
            {
                Hide();
                return;
            }

            hasSale = iapManager.IsSale(ConfigPackData.type);
            for (int i = 0; i < packs.Length; i++)
            {
                InitGroupPack(packs[i], ConfigPackData.groupData[i], i);
            }

            if (txtTitle != null)
            {
                txtTitle.text = ConfigPackData.name;
            }

            if (holdTime != null)
            {
                holdTime.SetActive(ConfigPackData.timeActive > 0);
            }
        }

        public virtual void Initialize(IAPManager absIAP, ConfigPackData configPackData,
            ConfigPackDataServer configPackDataServer, string where)
        {
            iapManager = absIAP;
            this.where = where;
            ConfigPackData = configPackData;
            ConfigPackDataServer = configPackDataServer;
            ReInitialize();
        }

        protected virtual void InitGroupPack(GroupPack group, GroupPackData data, int idx)
        {
            var salePrice = InappHelper.Instance.getPrice(data.saleSku);
            var price = InappHelper.Instance.getPrice(data.sku);
            var hasSalePrice = hasSale && !string.IsNullOrEmpty(salePrice);
            group.txtPrice.text = hasSalePrice ? salePrice : price;
            if (group.txtOldPrice != null)
            {
                group.txtOldPrice.gameObject.SetActive(hasSalePrice);
                group.txtOldPrice.text = price;
            }

            if (group.txtVipPoint != null)
            {
                //group.txtVipPoint.text = $"{prefixVip}{VipManager.Instance.GetPointVip(InappHelper.Instance.getPointVip(data.sku))}{suffixesVip}";
            }

            if (group.txtXValue != null)
            {
                group.txtXValue.text = data.xValue;
            }

            if (group.txtSaleValue != null)
            {
                group.txtSaleValue.text = data.saleValue;
            }

            if (group.objSaleValue != null)
            {
                group.objSaleValue.SetActive(!string.IsNullOrEmpty(data.saleValue));
            }

            if (group.objXValue != null)
            {
                group.objXValue.SetActive(!string.IsNullOrEmpty(data.xValue));
            }

            if (group.objRemoveAds != null)
            {
                group.objRemoveAds.SetActive(InappHelper.Instance.getRemoveAllAds(data.sku));
            }

            if (group.objRemoveAdsFull != null)
            {
                group.objRemoveAdsFull.SetActive(InappHelper.Instance.getRemoveAdsFull(data.sku));
            }

            if (group.objVip != null)
            {
                group.objVip.SetActive(InappHelper.Instance.getPointVip(data.sku) > 0);
            }
        }

        protected virtual void OnClickBuy(int index)
        {
            var sku = ConfigPackData.groupData[index].sku;
            if (hasSale && !string.IsNullOrEmpty(ConfigPackData.groupData[index].saleSku))
            {
                sku = ConfigPackData.groupData[index].saleSku;
            }
            InappHelper.Instance.BuyPackage(sku, where, callback =>
            {
                if (callback.status == 1)
                {
                    //var g = new List<DataResource>();
                    //var pointVip = InappHelper.Instance.getPointVip(data.groupData[index].sku);
                    //if (pointVip > 0)
                    //{
                    //    //pointVip = VipManager.Instance.GetPointVip(pointVip);
                    //    var l = data.groupData[index].gifts.ToList();
                    //    var n = new DataResource();
                    //    //n.Init(RES_type.VipPoint, 0, pointVip);
                    //    l.Add(n);
                    //    OnBuySuccess(l.ToArray(), index, data.groupData[index].sku);
                    //}
                    //else
                    {
                        OnBuySuccess(GetGift(index), index, sku);
                    }
                }
            });
        }

        protected virtual void OnBuySuccess(DataResource[] gift, int idx, string sku)
        {
            if (gift != null && gift.Length > 0)
            {
                GameEvent.OnReceiveResource?.Invoke(LogEvent.ReasonItem.purchase, where, gift, DataManager.Level);
                UIManager.Instance.ShowPopup<UIReceiveGift>(null).Initialized(gift);
            }

            iapManager.AddPurchase(ConfigPackData.type, idx, sku);
            if (iapManager.IsMaxPurchase(ConfigPackData))
            {
                iapManager.RemovePack(ConfigPackData.type);
                Hide();
            }

            Observer.Notify(ObserverName.purchase_success, (int)ConfigPackData.type);
        }

        protected DataResource[] GetGift(int index)
        {
            if (ConfigPackDataServer == null) return ConfigPackData.groupData[index].gifts;
            return ConfigPackDataServer.groupGift[index].gifts;
        }

        public virtual void Hide(bool hasShowOutAnim = false)
        {
            if (isDispose) return;
            isDispose = true;
            iapManager.ReleaseCurrentPopup();
        }

        protected virtual void FixedUpdate()
        {
            if (ConfigPackData == null) return;
            if (txtTime != null && ConfigPackData.timeActive > 0)
            {
                var time = iapManager.GetTimeActive(ConfigPackData.type);
                var timeLerp = ConfigPackData.timeActive * 1000 - (MGTime.GetUtcTime() - time);
                var timeSpan = new TimeSpan(timeLerp * 10000);
                if (timeSpan.Days > 0)
                {
                    txtTime.text = $"{timeSpan.Days:00}d:{timeSpan.Hours:00}h";
                }
                else if (timeSpan.Hours > 0)
                {
                    txtTime.text = $"{timeSpan.Hours:00}h:{timeSpan.Minutes:00}m";
                }
                else
                {
                    txtTime.text = $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
                }

                if (timeSpan.TotalSeconds <= 0)
                {
                    Hide();
                }
            }
        }
    }
}