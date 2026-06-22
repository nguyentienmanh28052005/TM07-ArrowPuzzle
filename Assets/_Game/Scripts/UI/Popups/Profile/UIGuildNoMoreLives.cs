using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using mygame.sdk;
using time;


public class UIGuildNoMoreLives : PopupUI
{
    [Space, Header("UI")]
    [SerializeField] Button btn_Close;
    [SerializeField] Button btn_FreeLives;
    [SerializeField] Button btn_DeactiveFreeLives;
    [SerializeField] Button btn_Refill;
    [SerializeField] Text txt_HeartCurrent;
    [SerializeField] Text txt_des;
    [SerializeField] Text txt_TimeRecoverHeart;
    [SerializeField] Text txt_NotifyHeart;
    [SerializeField] Text txt_gold;
    [SerializeField] GameObject obj_Infinity;
    private IDisposable subHeartReceive;
    [SerializeField] RectTransform rectMoreLive;
    [SerializeField] GameObject offerObject;
    
    [SerializeField] GameObject loading;
    
    private void OnEnable()
    {
        HeartManager.OnReceiveHeartDone += OnReceiveHeartDone;
        subHeartReceive = HeartManager.HeartReceive.Subscribe(x =>
        {
            txt_NotifyHeart.SetValue(Mathf.Min(x.listDonater.Count, HeartManager.MAX_HEART - HeartManager.Heart));
            btn_DeactiveFreeLives.gameObject.SetActive(x.listDonater.Count <= 0);
            btn_FreeLives.gameObject.SetActive(x.listDonater.Count > 0);

        });
    }
    private void OnDisable()
    {
        HeartManager.OnReceiveHeartDone -= OnReceiveHeartDone;
        subHeartReceive.Dispose();
    }

    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        btn_Close.onClick.AddListener(Hide);
        btn_FreeLives.onClick.AddListener(FreeLives);
        btn_Refill.onClick.AddListener(Refill);
        
        var sizeRate = Screen.height / (float)Screen.width;
        if (SdkUtil.isiPad())
        {
            offerObject.SetActive(true);
            rectMoreLive.anchoredPosition = new Vector2(0, 175);
        }
        else if (sizeRate > 2f)
        {
            offerObject.SetActive(true);
            rectMoreLive.anchoredPosition = new Vector2(0, 140);
        }
        else if (sizeRate > 1.8f)
        {
            offerObject.SetActive(true);
            rectMoreLive.anchoredPosition = new Vector2(0, 170);
        }
        else
        {
            offerObject.SetActive(false);
            rectMoreLive.anchoredPosition = Vector2.zero;
        }
    }
    public override void Show(Action onClose)
    {
        base.Show(onClose);
        txt_gold.SetValue(HeartManager.CF_GoldBuyHeart);
        AdsHelperWrapper.SetBannerShowState(false, "more_live");
    }
    private void Update()
    {
        if (HeartManager.InfinityEndTime > MGTime.GetUtcTime())
        {
            obj_Infinity.SetActive(true);
            txt_HeartCurrent.gameObject.SetActive(false);
            txt_des.SetText("unlimited_lives_in");
        }
        else
        {
            txt_des.SetText("time_to_next_live");
            obj_Infinity.SetActive(false);
            txt_HeartCurrent.gameObject.SetActive(true);
            txt_HeartCurrent.SetValue(HeartManager.Heart);

        }
        txt_TimeRecoverHeart.SetValue(HeartManager.Instance.GetTimeRemaningText());
    }

    public void FreeLives()
    {
        loading.SetActive(true);
        int value = Mathf.Min(HeartManager.MAX_HEART - HeartManager.Heart, HeartManager.HeartReceive.Value.listDonater.Count);
        HeartManager.cachedReceiveHeart = value;
        ServerHub.GetSendRequestServer<SendRequestUser>().RequestUseResource(2, value, value == 1? HeartManager.HeartReceive.Value.listDonater[0].playerId : "");
    }
    public void Refill()
    {
        if (HeartManager.InfinityEndTime > MGTime.GetUtcTime())
        {
            UIManager.Instance.NotifyContent("", "noti_infinity_heart");
            return;
        }
        int cf_Gold = HeartManager.CF_GoldBuyHeart;
        if (HeartManager.Heart < HeartManager.MAX_HEART)
        {
            if (DataManager.Gold >= cf_Gold)
            {
                DataManager.Instance.OnSinkResource( LogEvent.ReasonItem.use, "refill_heart",new DataResource[]{new (){resType = RES_type.GOLD,amount = -cf_Gold}} , DataManager.Level);
                IResourceTarget.OnItemChange?.Invoke(RES_type.GOLD, .5f);
                HeartManager.Instance.AddHeart(HeartManager.MAX_HEART - HeartManager.Heart, "ui_more_lives", LogEvent.ReasonItem.exchange);
                Hide();
            }
            else
            {
                // show shop (or mini shop)
                gameObject.SetActive(false);
                var UIShopPopup = UIManager.Instance.ShowPopup<UIShopPopup>(() =>
                {
                    gameObject.SetActive(true);

                });
                UIShopPopup.SetShowPosition(LogEvent.IAP_ShowPosition.shop_popup);
                UIShopPopup.ScrollToTarget();
            }
        }
        else
        {
            UIManager.Instance.NotifyContent("", "noti_full_receive_heart");
        }
    }

    private void OnReceiveHeartDone(bool obj)
    {
        loading.SetActive(false);
        UIManager.Instance.NotifyContent("", obj ? "receive_heart_success" : "receive_heart_failed");
        if (obj == true)
        {
            Hide();
        }
    }

    private void OnClickAddHeart()
    {
       
    }

    public override void Hide()
    {
        AdsHelperWrapper.SetBannerShowState(true, "more_live");
        base.Hide();
    }
}
