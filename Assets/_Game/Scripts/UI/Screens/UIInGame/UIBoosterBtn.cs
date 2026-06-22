using System;
using System.Collections;
using System.Collections.Generic;
using mygame.sdk;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class UIBoosterBtn : MonoBehaviour, IResourceTarget
{
    public Button btn;
    public Image icon;
    [SerializeField] private Image addIcon;
    [SerializeField] private GameObject amountObject;
    [SerializeField] private Text amountText;
    [SerializeField] private Text lockText;
    [SerializeField] private BoosterType type;
    [SerializeField] Transform holder;
    [SerializeField] private GameObject freeObj;
    [SerializeField] private GameObject lockObj;
    [SerializeField] private GameObject handObj;
    [SerializeField] private ParticleSystem claimVFX;
    private Tween hintHandTween;
    private bool isFree = false; /*=> BoosterManager.IsTutorialDone(type) == false && BoosterConfig.GetConfigData(type).levelTutorial == DataManager.Level;*/
    bool isLock => DataManager.Level < BoosterConfig.GetConfigData(type).levelUnlock;
    private bool isTut = false;
    private bool hasShowTut = false;
    public BoosterType boosterType => type;
    private void Start()
    {
        btn.onClick.AddListener(OnClickBooster);
        IResourceTarget.OnItemChange += AmountChange;
    }
    public void Initialized()
    {
        if (this == null) return;
        var config = BoosterConfig.GetConfigData(type);
        var amount = BoosterManager.Instance.BoosterAmount(type);
        icon.gameObject.SetActive(!isLock);
        addIcon.gameObject.SetActive(amount <= 0 && !isFree && !isLock);
        amountObject.gameObject.SetActive(amount > 0 && !isFree && !isLock);
        amountText.text = amount.ToString();
        lockText.text = $"Lv. {config.levelUnlock.ToString()}";
        freeObj.SetActive(isFree && !isLock);
        lockObj.SetActive(isLock);
        if (config.levelUnlock == DataManager.Level && !BoosterManager.IsGiftedBooster(type))
        {
            IResourceTarget.OnItemChange -= AmountChange;

            if (config.numGift > 0)
            {
                BoosterManager.Instance.AddBooster(type, config.numGift, "gift_first_time", LogEvent.ReasonItem.reward);
                Debug.Log($"gift_first_time {type} {config.numGift}");
            }
            BoosterManager.SetGiftedBooster(type, true);
            IResourceTarget.OnItemChange += AmountChange;

        }
    }
    public void SetFree(bool flag)
    {
        //isFree = flag;
        Initialized();
    }
    Tween tweenHand;



    public void HideTutorialClick(BoosterType boosterType)
    {
        LogEvent.TutorialAction(type.ToString(), "using_booster", 2);
        HideTutorialClick();
    }
    public void HideTutorialClick()
    {
        if (tweenHand != null)
        {
            tweenHand.Kill();
        }

        handObj.gameObject.SetActive(false);
        hasShowTut = false;
        var uiInGame = UIManager.Instance.GetScreenActive<UIInGame>();
        if (uiInGame != null) uiInGame.isShowHandTutorial = false;

        BoosterManager.CBOnUseBooster -= HideTutorialClick;
        LevelManager.UnLoadLevelAction -= HideTutorialClick;
        
        var popupTut = UIManager.Instance.GetPopupActive<UITutorial>();
        if (popupTut != null) popupTut.Hide();
    }
    public bool CheckTutorial()
    {
        var boosterConfig = BoosterConfig.GetConfigData(type);
        if (BoosterManager.IsTutorialDone(type) == false && BoosterManager.IsShowTutorial(type) == false &&
            boosterConfig.levelUnlock == DataManager.Level && !hasShowTut
           /* &&(type != BoosterType.ClearHole || !UIBoosterTutorial.HasPickThreeOfClearHole)*/)
        {
            BoosterManager.SetShowTutorial(type);
            hasShowTut = true;
            if (boosterConfig.showPopup > 0)
            {
                isTut = true;
                // Canvas canvas = gameObject.AddComponent<Canvas>();
                // GraphicRaycaster graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();
                // canvas.overrideSorting = true;
                // canvas.sortingOrder = 101;
                UIManager.Instance.ShowPopup<UIBoosterTutorial>(null).Initialize(BoosterManager.Instance.GetInfo(type), icon.rectTransform, btn.GetComponent<RectTransform>(),
                    () =>
                    {
                        UIManager.Instance.blockerUI.SetActive(true);
                        if (type == BoosterType.Clear)
                        {
                            isTut = false;
                            OnClaimBooster();
                            var UITutorial = UIManager.Instance.ShowPopup<UITutorial>(null);
                            Tutorial(UITutorial, boosterConfig);
                            UIManager.Instance.blockerUI.SetActive(false);
                        }
                        else if (type == BoosterType.Shuffle || type == BoosterType.Hand)
                        {
                            isTut = false;
                            OnClaimBooster();
                            var UITutorial = UIManager.Instance.ShowPopup<UITutorial>(null);
                            UITutorial.TutorialSelectButtonBooster(btn.GetComponent<RectTransform>(), OnTutorialClick, 0);
                            UIManager.Instance.blockerUI.SetActive(false);
                        }
                        else
                        {
                            isTut = false;
                            OnClaimBooster();
                            var UITutorial = UIManager.Instance.ShowPopup<UITutorial>(null);
                            Tutorial(UITutorial, boosterConfig);
                            UIManager.Instance.blockerUI.SetActive(false);
                        }
                    });
            }
            else
            {
                OnClaimBooster();
                var UITutorial = UIManager.Instance.ShowPopup<UITutorial>(() => hasShowTut = false);
                if (type == BoosterType.Clear || type == BoosterType.Shuffle || type == BoosterType.Hand)
                {
                    UITutorial.TutorialSelectButtonBooster(btn.GetComponent<RectTransform>(), OnTutorialClick, 0);
                }
                else
                {
                    Tutorial(UITutorial, boosterConfig);
                }
            }
            return true;
            void OnTutorialClick()
            {
            }
        }

        return false;
    }
    public void Tutorial(UITutorial UITutorial,BoosterConfigData configData)
    {

        handObj.gameObject.SetActive(true);
        LevelManager.UnLoadLevelAction += HideTutorialClick;
        BoosterManager.CBOnUseBooster += HideTutorialClick;
        GetLogName(out string logName);


        LogEvent.TutorialAction(type.ToString(), $"show_hand_tut_{logName}", 1);

        UITutorial.Hide();
        if (BoosterManager.Instance.BoosterAmount(boosterType) <= 0 || configData.showHand ==0)
        {
            HideTutorialClick();
        }
        return;
    }
    public void GetLogName(out string logname)
    {
        logname = "";
        switch (type)
        {
            case BoosterType.Hand:
                logname = "hand";
                break;
            case BoosterType.Clear:
                logname = "clear";
                break;
            case BoosterType.Shuffle:
                logname = "shuffle";
                break;
            case BoosterType.ExtraSlot:
                logname = "extra_slot";
                break;
        }
    }
    private void AmountChange(RES_type res, float duration = 1)
    {
        if (BoosterManager.Instance.GetResType(type) != res) return;
        Initialized();
    }
    public void OnClaimBooster()
    {
        claimVFX.gameObject.SetActive(false);
        claimVFX.gameObject.SetActive(true);
        claimVFX.Play();
        Initialized();
        AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Item_Jump_End);
    }
    private void OnClickBooster()
    {
        if (isLock)
        {
            var config = BoosterConfig.GetConfigData(type);
            UIManager.Instance.NotifyContent2("", "desc_unlock_at_level_x", config.levelUnlock.ToString());
            return;
        }

        if (isTut || GameManager.GameState != GameState.Playing) return;
        var logTutorial = false;
        var popupTut = UIManager.Instance.GetPopupActive<UITutorial>();
        if (popupTut != null)
        {
            popupTut.Hide();
            logTutorial = true;
        }
        
        if (BoosterManager.Instance.BoosterAmount(type) > 0 || isFree)
        {
            BoosterManager.Instance.ActiveBooster(type, false, isFree);
            Initialized();
        }
        else
        {
            LogEvent.ScreenGo(LogEvent.ScreenName.BoosterPopup, LogEvent.ButtonName.ButtonInGame);
            UIManager.Instance.ShowPopup<UIBuyBooster>(() => LogEvent.ChangeScreenName(LogEvent.ScreenName.LevelPlay)).Initialized(BoosterManager.Instance.GetInfo(type), () =>
            {
                BoosterManager.Instance.ActiveBooster(type, false);
                Initialized();
            });

        }
        GetLogName(out string logName);


        FIRhelper.logEvent($"booster_click_{logName}");
        FIRhelper.logEvent($"level_{GameRes.GetLevel():0000}_click_{logName}");
        if (logTutorial)
        {
            LogEvent.TutorialAction("use_booster", $"use_booster_{logName}", 0);
        }
    }


    public void ShowHintHand()
    {
        if (isLock) return;
        hintHandTween?.Kill();
        transform.localScale = Vector3.one;
        hintHandTween = transform.DOPunchScale(Vector3.one * 0.05f, 1.4f, 4, 1f)
            .SetLoops(-1, LoopType.Restart)
            .SetId(this);
    }

    public void HideHintHand()
    {
        hintHandTween?.Kill();
        hintHandTween = null;
        transform.localScale = Vector3.one;
    }

    private void OnDestroy()
    {
        IResourceTarget.OnItemChange -= AmountChange;
        BoosterManager.CBOnUseBooster -= HideTutorialClick;
        LevelManager.UnLoadLevelAction -= HideTutorialClick;
    }

    public List<RES_type> GetResourceTypes()
    {
        return new List<RES_type>() { BoosterManager.Instance.GetResType(type) };
    }

    public Transform GetTransform()
    {
        return holder;
    }

    public void UpdateVisual()
    {
        if (holder == null) return;
        holder.DOPunchScale(Vector3.one * .1f, .25f);
        Initialized();

    }
    private void OnEnable()
    {
        RewardReceivedHub.RegisterTarget(this);
        Initialized();
    }
    private void OnDisable()
    {
        RewardReceivedHub.RemoveTarget(this);
        DOTween.Kill(this);
    }
}
