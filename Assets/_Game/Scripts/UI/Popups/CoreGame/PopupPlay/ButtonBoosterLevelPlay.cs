using mygame.sdk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonBoosterLevelPlay : MonoBehaviour
{
    [SerializeField] GameObject btnLock;
    [SerializeField] GameObject btnBooster;
    [SerializeField] GameObject btnAdd;
    [SerializeField] GameObject btnSelect;
    [SerializeField] GameObject btnNumber;
    [SerializeField] Text txtNumberBooster;
    [SerializeField] Image imgBtnBooster;
    [SerializeField] Sprite spriteBtnSelect;
    [SerializeField] Sprite spriteBtnDeselect;
    [SerializeField] Button btnTrigger;
    [SerializeField] GameObject tutObj;
    [SerializeField] Text txtTime;
    [SerializeField] GameObject unlimitedObj;
    public BoosterType boosterType;
    private bool isSelected;
    private bool isTutorial;
    private PopupUI popup;
    public bool IsTutorial {  get { return isTutorial; } }
    private long timeUnlimited => BoosterManager.GetEndTimeUnlimited(boosterType) - SdkUtil.CurrentTimeMilis();
    public bool IsSelected
    {
        get
        {
            return isSelected;
        }
    }
   
    private void Awake()
    {
        btnTrigger.onClick.AddListener(OnClick);
    }
    private void Update()
    {
        if (timeUnlimited > 0)
        {
            txtTime.SetTextTime((int)(timeUnlimited / 1000));
        }
        else
        {
            if (unlimitedObj.activeSelf)
            {
                unlimitedObj.SetActive(false);
                OnDeselect();
            }
        }
    }
   
    public void Initialize(PopupUI popupUI)
    {
        this.popup = popupUI;
        OnDeselect();
        SetAmount();

        tutObj.SetActive(false);
        isTutorial = false;
        

    }
    void SetAmount()
    {
        int amountBooster = BoosterManager.Instance.BoosterAmount(boosterType);
        txtNumberBooster.text = $"{amountBooster}";

        btnAdd.SetActive(amountBooster <= 0);

        btnNumber.SetActive(amountBooster > 0);

    }
    private void OnClick()
    {
       
    }
    private void OnSelect()
    {
        if (BoosterManager.Instance.BoosterAmount(boosterType) <= 0 && tutObj.activeSelf == false && BoosterManager.GetEndTimeUnlimited(boosterType) < SdkUtil.CurrentTimeMilis())
        {
            popup.gameObject.SetActive(false);
            UIManager.Instance.ShowPopup<UIBuyBooster>(()=> {
                popup.gameObject.SetActive(true);
            }).Initialized(BoosterManager.Instance.GetInfo(boosterType),()=> {
                Initialize(popup);
                OnSelect(); 
            });
            return;
        }
        isSelected = true;
        imgBtnBooster.sprite = spriteBtnSelect;
        btnSelect.gameObject.SetActive(true);
        if (isTutorial)
        {
            tutObj.gameObject.SetActive(false);
        }
    }
    private void OnDeselect()
    {
        if (timeUnlimited > 0)
        {
            return;
        }
        isSelected = false;
        imgBtnBooster.sprite = spriteBtnDeselect;
        btnSelect.gameObject.SetActive(false);
        if (isTutorial)
        {
            tutObj.gameObject.SetActive(true);
        }
    }
    private void ActiveLockObject(bool isLock)
    {
        btnBooster.SetActive(!isLock);
        btnLock.SetActive(isLock);
    }
    public void ActiveBooster()
    {

        
    }
}
