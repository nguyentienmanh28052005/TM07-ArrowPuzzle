using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using mygame.sdk;

public class UIBoosterTutorial : PopupUI
{
    [SerializeField] private Text boosterNameText;
    [SerializeField] private Text[] boosterDesTexts;
    [SerializeField] private Text txtButton;
    [SerializeField] private Text[] txtNumGifts;
    [SerializeField] private Image boosterImg;
    [SerializeField] private BoosterInfo boosterInfo;
    [SerializeField] private ParticleSystem claimFX;

    [SerializeField] private List<GameObject> iconImages;
    [SerializeField] private List<GameObject> AnimBooster;

    private RectTransform buttonIconTransform;
    private RectTransform buttonRectTransform;
    private Action onDone;
    [SerializeField] Image bg;
    [SerializeField] GameObject block;
    [SerializeField] RectTransform popup;

    public static bool HasPickThreeOfClearHole
    {
        get => PlayerPrefs.GetInt("has_pick_three_of_clear_hole", 0) == 1;
        set => PlayerPrefs.SetInt("has_pick_three_of_clear_hole", value ? 1 : 0);
    }

    public void Initialize(BoosterInfo info, RectTransform buttonIconTransform, RectTransform buttonRectTransform,
        Action onDone = null)
    {
        boosterImg.sprite = info.icon;
        boosterNameText.SetText(info.name);
        foreach (var boosterDesText in boosterDesTexts)
        {
            boosterDesText.SetText(info.desc);
        }

        this.buttonIconTransform = buttonIconTransform;
        boosterInfo = info;
        this.buttonRectTransform = buttonRectTransform;
        this.onDone = onDone;
        foreach (var icon in iconImages)
        {
            icon.gameObject.SetActive(false);
        }

        switch (boosterInfo.type)
        {
            case BoosterType.Hand:
                iconImages[0].SetActive(true);
                AnimBooster[0].SetActive(true);
                break;
            case BoosterType.Clear:
                iconImages[1].SetActive(true);
                AnimBooster[1].SetActive(true);
                break;
            case BoosterType.Shuffle:
                iconImages[2].SetActive(true);
                AnimBooster[2].SetActive(true);
                break;
            case BoosterType.ExtraSlot:
                iconImages[3].SetActive(true);
                AnimBooster[3].SetActive(true);
                break;
        }

        boosterImg.gameObject.SetActive(false);
        AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_UI_Button_Click);
    }

    public override void Show(Action onClose)
    {
        LogEvent.TutorialAction(boosterInfo.type.ToString(), "show_tut", 0);
        int numGift = BoosterConfig.GetConfigData(boosterInfo.type).numGift;
        if (numGift <= 0)
        {
            txtButton.SetText("_continue");
        }
        else
        {
            txtButton.SetText("_claim_1");
        }

        for (int i = 0; i < txtNumGifts.Length; i++)
        {
            txtNumGifts[i].SetValue($"x{numGift}");
            if (numGift <= 0)
            {
                txtNumGifts[i].gameObject.SetActive(false);
            }
            else
            {
                txtNumGifts[i].gameObject.SetActive(true);
            }
        }

        block.SetActive(false);
        base.Show(onClose);
    }

  public override void Hide()
    {
        // Canvas canvas = buttonRectTransform.GetComponent<Canvas>();
        // canvas.sortingOrder = 160;
        boosterImg.gameObject.SetActive(true);
        var uiIngame = uiManager.GetScreenActive<UIInGame>();

        if (BoosterConfig.GetConfigData(boosterInfo.type).numGift <= 0)
        {
            if (uiIngame != null)
            {
                AnimGuideSetting(uiIngame, () =>
                {
                    // uiIngame.PanelSetting.EnableNoticeGuide();
                    base.Hide();
                    onDone?.Invoke();
                });
            }
            else
            {
                base.Hide();
                onDone?.Invoke();
            }

            return;
        }

        boosterImg.transform.SetParent(buttonIconTransform, true);
        boosterImg.gameObject.SetActive(false);
        //mainPopUp.gameObject.SetActive(false);

        if (uiIngame != null)
        {
            // canvas.sortingOrder = 120;
            AnimGuideSetting(uiIngame, () =>
            {
                mainPopUp.gameObject.SetActive(false);
                // canvas.sortingOrder = 160;
                // uiIngame.PanelSetting.EnableNoticeGuide();
                boosterImg.gameObject.SetActive(true);
                var sq = DOTween.Sequence().SetId(this);
                sq.Append(boosterImg.rectTransform.DOScale(Vector3.one * 1.2f, 0.3f)
                        .OnComplete(() => AudioManager.Instance.PlayOneShot("SFX_Claim_Tick")))
                    .Append(boosterImg.rectTransform.DOScale(
                        Vector3.one * buttonIconTransform.rect.width / boosterImg.rectTransform.rect.width, 0.5f))
                    .Join(boosterImg.rectTransform.DOJump(buttonIconTransform.transform.position, 12, 1, 0.5f)
                        .OnComplete(() =>
                        {
                            claimFX.transform.SetParent(buttonIconTransform.transform);
                            claimFX.transform.position = buttonIconTransform.transform.position;
                            claimFX.gameObject.SetActive(true);
                            claimFX.Play();
                            base.Hide();
                            onDone?.Invoke();
                            Destroy(boosterImg);
                        }));
            });
        }
        else
        {
            mainPopUp.gameObject.SetActive(false);
            boosterImg.gameObject.SetActive(true);
            var sq = DOTween.Sequence().SetId(this);
            sq.Append(boosterImg.rectTransform.DOScale(Vector3.one * 1.2f, 0.3f)
                    .OnComplete(() => AudioManager.Instance.PlayOneShot("SFX_Claim_Tick")))
                .Append(boosterImg.rectTransform.DOScale(
                    Vector3.one * buttonIconTransform.rect.width / boosterImg.rectTransform.rect.width, 0.5f))
                .Join(boosterImg.rectTransform.DOJump(buttonIconTransform.transform.position, 12, 1, 0.5f)
                    .OnComplete(() =>
                    {
                        claimFX.transform.SetParent(buttonIconTransform.transform);
                        claimFX.transform.position = buttonIconTransform.transform.position;
                        claimFX.gameObject.SetActive(true);
                        claimFX.Play();
                        base.Hide();
                        onDone?.Invoke();
                        Destroy(boosterImg);
                    }));
        }
    }

    void AnimGuideSetting(UIInGame uiIngame, Action cb)
    {
        if (PlayerPrefsUtil.CFTutorialNewMechanicGuide > 0)
        {
            block.SetActive(true);
            bg.DOFade(0, .2f).SetId(this);
            popup.DOScale(1.1f, 0.3f).OnComplete(() =>
            {
                popup.DOScale(1, 0.1f).SetId(this).OnComplete(() => { cb?.Invoke(); });
            }).SetId(this);
        }
        else
        {
            cb?.Invoke();
        }
    }
}