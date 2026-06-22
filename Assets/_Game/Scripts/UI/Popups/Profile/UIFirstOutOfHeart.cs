using UnityEngine;
using UnityEngine.UI;
using System;

public class UIFirstOutOfHeart : PopupUI
{
    [Space, Header("UI")] [SerializeField] Button btn_Close;
    [SerializeField] Button btn_Continue;
    [SerializeField] Text txt_HeartCurrent;
    [SerializeField] Text txt_des;
    [SerializeField] Text txt_TimeRecoverHeart;
    [SerializeField] GameObject obj_Infinity;

    private DataResource[] gift;

    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        btn_Close.onClick.AddListener(Hide);
        btn_Continue.onClick.AddListener(ContinueEvent);
    }

    public void Initialized(DataResource[] dataResources)
    {
        gift = dataResources;
        txt_TimeRecoverHeart.SetTextTime(gift[0].amount);
    }

    public override void Show(Action onClose)
    {
        base.Show(onClose);

        txt_HeartCurrent.gameObject.SetActive(false);
        btn_Continue.gameObject.SetActive(true);
        btn_Close.gameObject.SetActive(false);

        txt_des.SetText("_desc_first_time_bonus");
    }


    public void ContinueEvent()
    {
        Hide();
    }
}