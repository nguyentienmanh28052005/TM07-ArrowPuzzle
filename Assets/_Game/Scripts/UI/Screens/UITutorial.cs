using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Animation = UnityEngine.Animation;

public class UITutorial : PopupUI
{
    [SerializeField] private RectTransform hand;
    [SerializeField] private RectTransform arrowObj;
    [SerializeField] private Image darkBg;
    [SerializeField] private SkeletonGraphic handImage;
    [SerializeField] private SkeletonAnimation _objHand3D;
    [SerializeField] Button btnHide;
    [SerializeField] Canvas canvas;
    [SerializeField] private GameObject tutArrow;
    [SerializeField] private Unmask unmask;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] RectTransform _rectBoosterHand;
    [SerializeField] RectTransform _rectBoosterMoveObject;
    [SerializeField] RectTransform _rectBoosterMoveBlock;
    [SerializeField] GameObject _objTutorialLevel1;
    [SerializeField] GameObject _objTutorialMission;
    [SerializeField] RectTransform _missionArrow;
    private Transform tar;
    private Transform tarParent;
    public SkeletonGraphic spineHand => handImage;

    private void Awake()
    {
        btnHide.onClick.AddListener(Hide);
    }

    private void OnEnable()
    {
        LevelManager.UnLoadLevelAction += Hide;
    }

    private void OnDisable()
    {
        LevelManager.UnLoadLevelAction -= Hide;
    }

    public void EnableHideOnClick(bool Active)
    {
        btnHide.gameObject.SetActive(Active);
    }

    public void Initialized(Transform target, Vector2 padding, bool isCanvasTarget, bool isFlip = false,
        bool showDark = false, bool isShowHand = true, bool isHideOnClick = false)
    {
        if (tarParent != null && tar != null) tar.parent = tarParent;
        EnableHideOnClick(isHideOnClick);
        hand.gameObject.SetActive(isShowHand);
        tutArrow.SetActive(false);
        //arrowObj.gameObject.SetActive(!isShowHand);
        if (isCanvasTarget)
        {
            hand.position = target.position;
            //arrowObj.position = target.position;
            //arrowObj.anchoredPosition += padding;
            canvas.sortingOrder = 160;
        }
        else
        {
            var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), screenPoint,
                UIManager.Instance.canvas.worldCamera, out var point);

            hand.anchoredPosition = point + padding;
            arrowObj.anchoredPosition = point + padding;
            canvas.sortingOrder = 110;
        }

        hand.transform.localScale = new Vector3(isFlip ? -1 : 1, 1, 1);
        handImage.color = new Color(1f, 1f, 1f, 0f);
        handImage.DOFade(1, .25f);
        darkBg.gameObject.SetActive(showDark);
        if (showDark)
        {
            tar = target;
            tarParent = target.parent;
            target.SetParent(transform);
            target.SetSiblingIndex(1);
        }
        else
        {
            tarParent = null;
        }
    }

    public void Initialized(Vector3 target, Vector2 padding, bool isCanvasTarget, bool isFlip = false,
        bool showDark = false, bool isShowHand = true, bool isHideOnClick = false)
    {
        if (tarParent != null && tar != null) tar.parent = tarParent;
        EnableHideOnClick(isHideOnClick);
        hand.gameObject.SetActive(isShowHand);
        tutArrow.SetActive(false);
        //arrowObj.gameObject.SetActive(!isShowHand);
        if (isCanvasTarget)
        {
            hand.position = target;
            //arrowObj.position = target.position;
            //arrowObj.anchoredPosition += padding;
            canvas.sortingOrder = 160;
        }
        else
        {
            var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, target);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), screenPoint,
                UIManager.Instance.canvas.worldCamera, out var point);

            hand.anchoredPosition = point + padding;
            arrowObj.anchoredPosition = point + padding;
            canvas.sortingOrder = 110;
        }

        hand.transform.localScale = new Vector3(isFlip ? -1 : 1, 1, 1);
        handImage.color = new Color(1f, 1f, 1f, 0f);
        handImage.DOFade(1, .25f);
        darkBg.gameObject.SetActive(showDark);
    }

    public void Initialized(Vector2 point)
    {
        btnHide.gameObject.SetActive(true);
        tutArrow.SetActive(false);
        hand.gameObject.SetActive(true);
        hand.GetComponent<RectTransform>().anchoredPosition = point;
        EnableHideOnClick(false);
    }

    public bool IsEnableDarkBG()
    {
        return darkBg.gameObject.activeSelf;
    }

    public void TutorialSelectButtonBooster(RectTransform target, Action OnClick, int arrowPos)
    {
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, 1f).SetEase(Ease.OutQuad).SetId(this);
        canvas.sortingOrder = 120;
        darkBg.gameObject.SetActive(true);
        hand.gameObject.SetActive(false);
        btnHide.gameObject.SetActive(false);
        tutArrow.SetActive(true);
        arrowObj.gameObject.SetActive(true);
        unmask.gameObject.SetActive(false);
        arrowObj.transform.localRotation = Quaternion.Euler(0, 0, 150 * arrowPos + (arrowPos == 0 ? 180 : 0));
        tar = target;
        tarParent = target.parent;
        target.SetParent(tutArrow.transform);
        var rect = arrowObj.GetComponent<RectTransform>();
        rect.anchorMin = target.anchorMin;
        rect.anchorMax = target.anchorMax;
        rect.anchoredPosition = target.anchoredPosition + new Vector2(70 * arrowPos, 135 + (arrowPos == 0 ? 40 : 0));
        hand.anchorMax = target.anchorMax;
        hand.anchorMin = target.anchorMin;
        hand.anchoredPosition = target.anchoredPosition + new Vector2(60, 60);
        var button = tar.gameObject.GetComponent<Button>();
        button.onClick.AddListener(OnClickBtn);

        void OnClickBtn()
        {
            Hide();
            OnClick?.Invoke();
            if (button != null)
            {
                button.onClick.RemoveListener(OnClickBtn);
            }
        }
    }
    
    public override void Hide()
    {
        DOTween.Kill(_objHand3D.transform);
        
        if (tarParent != null && tar != null)
        {
            tar.SetParent(tarParent);
        }

        base.Hide();
    }

    public void FadeOut()
    {
        canvasGroup.DOKill();
        _objHand3D.DOKill();
        
        canvasGroup.alpha = 1;
        canvasGroup.DOFade(0, 0.35f).SetEase(Ease.OutQuad).SetId(this);
        Color c = _objHand3D.Skeleton.GetColor();
        c.a = 1f;
        _objHand3D.Skeleton.SetColor(c);
        DOTween.To(() => c.a, x => { c.a = x; _objHand3D.Skeleton.SetColor(c); }, 0f, 0.35f).SetEase(Ease.OutQuad).SetId(this);
    }

    public void FadeIn()
    {
        canvasGroup.DOKill();
        _objHand3D.DOKill();

        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, 0.35f).SetEase(Ease.OutQuad).SetId(this);
        Color c = _objHand3D.Skeleton.GetColor();
        c.a = 0f;
        _objHand3D.Skeleton.SetColor(c);
        DOTween.To(() => c.a, x => { c.a = x; _objHand3D.Skeleton.SetColor(c); }, 1f, 0.35f).SetEase(Ease.OutQuad).SetId(this);
    }
}