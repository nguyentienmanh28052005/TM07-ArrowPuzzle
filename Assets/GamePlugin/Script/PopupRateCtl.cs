using UnityEngine;
using mygame.sdk;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class PopupRateCtl : MonoBehaviour
{
    [SerializeField] GameObject[] stars;
    [SerializeField] RectTransform popup;
    [SerializeField] Image bgImg;
    int countStar = 0;
    public Button star1;
    public Button star2;
    public Button star3;
    public Button star4;
    public Button star5;
    public Button Rate;
    public Button Close;
    public event Action OnClose;

    private void Awake()
    {
        star1.onClick.AddListener(() => onclickStar(1));
        star2.onClick.AddListener(() => onclickStar(2));
        star3.onClick.AddListener(() => onclickStar(3));
        star4.onClick.AddListener(() => onclickStar(4));
        star5.onClick.AddListener(() => onclickStar(5));
        Rate.onClick.AddListener(onClickRate);
        Close.onClick.AddListener(onClose);
    }
    public void SetActionBackClose(Action onClose)
    {
        gameObject.SetActive(true);
        bgImg.color = new Color(0, 0, 0, 0);
        bgImg.DOColor(new Color(0, 0, 0, 0.8f), 0.3f).SetEase(Ease.OutQuart).SetId(this);
        popup.localScale = Vector3.zero;
        popup.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuart).SetId(this);
        OnClose = onClose;
        countStar = 0;
        for (int i = 0; i < 5; i++)
        {
            stars[i].SetActive(i < countStar);
        }
        Rate.gameObject.SetActive(false);
        Close.gameObject.SetActive(false);
        DOVirtual.DelayedCall(2f, () => Close.gameObject.SetActive(true)).SetId(this);
        onclickStar(5);
    }
    public void onclickStar(int count)
    {
        Rate.gameObject.SetActive(true);

        countStar = count;
        for (int i = 0; i < 5; i++)
        {
            stars[i].SetActive(i < count);
        }
    }
    public void onClose()
    {
        gameObject.SetActive(false);
        OnClose?.Invoke();
    }
    public void onClickRate()
    {
        if (countStar > 0)
        {
            FIRhelper.logEvent("game_rate_" + countStar);
        }
        if (countStar >= 4)
        {
            GameHelper.Instance.rate();
        }
        PlayerPrefs.SetInt("is_show_rate", 1);
        onClose();
    }
    void OnDisable()
    {
        this.DOKill();
    }
}
