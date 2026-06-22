using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;
using Spine.Unity;

public class ButtonMainMenu : MonoBehaviour
{
    [SerializeField] protected RectTransform IconImage;
    [SerializeField] SkeletonGraphic iconAnim;
    [SerializeField, SpineAnimation] string[] anims;
    [SerializeField] protected Image btnImg;
    [SerializeField] protected GameObject textObj;
    [SerializeField] Sprite[] btnSprites;
    [SerializeField] float[] position;
    Button btn;
    Action clickAction;
    public RectTransform Icon => IconImage;
    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(Select);
    }
    private void Select()
    {
        AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_UI_Button_Click);
        AudioManager.Instance.PlayVibrate();
        clickAction?.Invoke();
    }
    public void OnSelectButton()
    {
        this.DOKill();
        btnImg.sprite = btnSprites[1];
        //btnImg.rectTransform.sizeDelta = new Vector2(btnImg.rectTransform.sizeDelta.x, 230);
        IconImage.transform.localScale = Vector3.one;
        IconImage.GetComponent<RectTransform>().DOAnchorPosY(position[1], 0.2f).SetId(this);
        IconImage.transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack).SetId(this);
        textObj.GetComponent<RectTransform>().DOAnchorPosY(-120, .2f).SetId(this);
        textObj.transform.DOScale(1, .2f).SetId(this);
        textObj.SetActive(true);
        iconAnim.Initialize(false);
        iconAnim.AnimationState.SetAnimation(0, anims[1], false);
        //SetNoti(false);
    }
    public void OnDeselectButton()
    {
        this.DOKill();
        btnImg.sprite = btnSprites[0];
        //btnImg.rectTransform.sizeDelta = new Vector2(btnImg.rectTransform.sizeDelta.x, 190);

        IconImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, position[0]);
        IconImage.transform.localScale = Vector3.one;
        textObj.SetActive(false);
        textObj.transform.DOScale(.6f, .2f).SetId(this);
        textObj.GetComponent<RectTransform>().DOAnchorPosY(-80, .2f).SetId(this);
        iconAnim.AnimationState.SetAnimation(0, anims[0], false);

    }
    public void SetEventClick(Action action)
    {
        clickAction = action;
    }
    protected virtual void OnDisable()
    {
        this.DOKill();
    }

}
