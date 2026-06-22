using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconEventMainBase : MonoBehaviour
{
    [Space, Header("Pos Notift")]
    [SerializeField] Vector2 vc_Noti_Left = new Vector2(-20, 20);
    [SerializeField] Vector2 vc_Noti_Right = new Vector2(20, 20);
    [SerializeField] RectTransform rect_Notify;
    [SerializeField] RectTransform rect_Notify_1;
    [SerializeField] Text txt_Notify_1;

    [Space, Header("Pos Jump")]
    [SerializeField] Vector2 vectorLeft = new Vector2(-270f, 90f);
    [SerializeField] Vector2 vectorRight = new Vector2(270f, 90f);
    [SerializeField] RectTransform rectTransform;
    [SerializeField] RectTransform rect_Icon;
    [SerializeField] RectTransform rect_ItemFly;
    [SerializeField] ItemFlyCurveUI itemFlyCurveUI;

    [Space, Header("Animation Skeleton")]
    [SerializeField] SkeletonGraphic skeleton_Icon;
    [SerializeField, SpineAnimation] string animation_Idle;
    [SerializeField, SpineAnimation] string animation_JumpDone;

    [Space, Header("Pos Move")]
    [SerializeField] RectTransform rect_VisualShowBG;
    [SerializeField] RectTransform rect_VisualShowRoot;
    [SerializeField] RectTransform rect_Text_Holder;
    [SerializeField] Vector2 vc_text_Left = new Vector2(72, 0);
    [SerializeField] Vector2 vc_text_Right = new Vector2(-72, 0);
    [SerializeField] RectTransform rect_Tag;
    [SerializeField] Vector2 vc_tag_Left = new Vector2(122, 0);
    [SerializeField] Vector2 vc_tag_Right = new Vector2(-122, 0);

    [Space, Header("Progress")]
    [SerializeField] RectTransform rect_Progress;
    [SerializeField] Text txt_Progress;
    [SerializeField] GameObject obj_Progress;

    private bool isLeft;
    private float posEnd;
    private float timeMove = 0.5f;
    // private float posProgressCurrent = 0;

    // private void Awake()
    // {
    //     rect_Progress.anchoredPosition = new Vector2(rect_Progress.anchoredPosition.x, rect_Progress.anchoredPosition.y - rect_Progress.sizeDelta.y);
    // }

    public void OnEnable()
    {
        if (skeleton_Icon != null)
        {
            skeleton_Icon.AnimationState.ClearTracks();
            skeleton_Icon.Skeleton.SetToSetupPose();
            skeleton_Icon.AnimationState.SetAnimation(0, animation_Idle, true);
        }
        this.Wait(Time.deltaTime, () =>
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(UIManager.Instance.canvas.worldCamera, rectTransform.position);
            float screenMiddle = Screen.width / 2f;
            if (rect_ItemFly != null)
            {
                if (screenPos.x < screenMiddle)
                {
                    rect_ItemFly.anchoredPosition = vectorRight;
                    rect_VisualShowBG.anchoredPosition = new Vector2(rect_VisualShowBG.sizeDelta.x / 2, 0);
                    rect_VisualShowRoot.anchoredPosition = new Vector2(-rect_VisualShowBG.sizeDelta.x, 0);
                    rect_Notify.anchorMin = new Vector2(1, 1);
                    rect_Notify_1.anchorMin = new Vector2(1, 1);
                    rect_Notify.anchorMax = new Vector2(1, 1);
                    rect_Notify_1.anchorMax = new Vector2(1, 1);
                    rect_Notify.anchoredPosition = vc_Noti_Right;
                    rect_Notify_1.anchoredPosition = vc_Noti_Right;
                    rect_Text_Holder.anchoredPosition = vc_text_Left;
                    rect_Tag.anchoredPosition = vc_tag_Left;
                    isLeft = true;
                }
                else
                {
                    rect_ItemFly.anchoredPosition = vectorLeft;
                    rect_VisualShowBG.anchoredPosition = new Vector2(-rect_VisualShowBG.sizeDelta.x / 2, 0);
                    rect_VisualShowRoot.anchoredPosition = new Vector2(rect_VisualShowBG.sizeDelta.x, 0);
                    rect_Notify.anchorMin = new Vector2(0, 1);
                    rect_Notify_1.anchorMin = new Vector2(0, 1);
                    rect_Notify.anchorMax = new Vector2(0, 1);
                    rect_Notify_1.anchorMax = new Vector2(0, 1);
                    rect_Notify.anchoredPosition = vc_Noti_Left;
                    rect_Notify_1.anchoredPosition = vc_Noti_Left;
                    rect_Text_Holder.anchoredPosition = vc_text_Right;
                    rect_Tag.anchoredPosition = vc_tag_Right;
                    isLeft = false;
                }
            }
        });
    }

    public void ShowNotify(int amount)
    {
        if (amount <= 0)
        {
            rect_Notify.gameObject.SetActive(true);
            rect_Notify_1.gameObject.SetActive(false);
        }
        else
        {
            rect_Notify.gameObject.SetActive(false);
            rect_Notify_1.gameObject.SetActive(true);
            txt_Notify_1.SetValue(amount.ToString());
        }
    }

    public void HideNotify()
    {
        rect_Notify.gameObject.SetActive(false);
        rect_Notify_1.gameObject.SetActive(false);
    }

    public void JumpItemFlyCurve(int count, bool isX2 = false, Action OnCompleted = null, bool isShowContent = false, Action OnSetUp = null, Action<Action> OnMoveUp = null, Action OnFirstComplete = null)
    {
        itemFlyCurveUI.FlyTo(rect_Icon, count, JumpCompleted, isX2, () => { OnFirstComplete?.Invoke(); });

        void JumpCompleted()
        {
            OnCompleted?.Invoke();
            if (skeleton_Icon != null)
            {
                Animate();
            }
            if (isShowContent)
            {
                OnSetUp?.Invoke();
                MoveUpContent(OnMoveUp);
            }
        }
    }

    private void MoveUpContent(Action<Action> OnMoveUp)
    {
        float posStart = 0;
        posEnd = rect_VisualShowRoot.anchoredPosition.x;
        if (isLeft)
        {
            posStart = -rect_VisualShowRoot.sizeDelta.x;
        }
        else
        {
            posStart = rect_VisualShowRoot.sizeDelta.x;
        }
        rect_VisualShowRoot.anchoredPosition = new Vector2(posStart, rect_VisualShowRoot.anchoredPosition.y);
        rect_VisualShowRoot.DOAnchorPosX(-10, timeMove).SetId(this).OnComplete(() =>
        {
            OnMoveUp?.Invoke(MoveDownContent);
        });
    }

    private void MoveDownContent()
    {
        rect_VisualShowRoot.DOAnchorPosX(0, 0.2f).SetId(this).OnComplete(() =>
        {
            rect_VisualShowRoot.DOAnchorPosX(posEnd, timeMove).SetId(this);
        });
    }

    private void OnDisable()
    {
        DOTween.Kill(this);
    }
    public void CancelJump()
    {
        itemFlyCurveUI.StopJump();
    }
    public void Animate()
    {
        if (skeleton_Icon != null && animation_Idle != null && animation_Idle.Length > 0 && animation_Idle.Length > 0)
        {
            skeleton_Icon.AnimationState.SetAnimation(0, animation_JumpDone, false);
            skeleton_Icon.AnimationState.AddAnimation(0, animation_Idle, true, 0);
        }
    }

    public void ShowProgress(string content, bool isActive)
    {
        txt_Progress.text = content;
        obj_Progress.gameObject.SetActive(isActive);
        // rect_Progress.anchoredPosition = new Vector2(rect_Progress.anchoredPosition.x, posProgressCurrent);
    }

    // public void AnimationProgress(string txt_Conten)
    // {
    //     if (!obj_Progress.activeSelf)
    //     {
    //         rect_Progress.DOAnchorPosY(posProgressCurrent, 0.5f).SetId(this);
    //     }
    //     obj_Progress.SetActive(true);
    //     txt_Progress.text = txt_Conten;
    // }
}