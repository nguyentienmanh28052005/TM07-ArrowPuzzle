using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using System.Collections.Generic;
using DG.Tweening;
using Observer = master.Observer;
using master;
using UnityEngine.Serialization;

public class UIUseBoosterInfo : MonoBehaviour
{
    [SerializeField] private Text instructText;
    [SerializeField] private Text nameText;
    [SerializeField] private Image icon;
    [SerializeField] private Button closeBtn;
    [SerializeField] private RectTransform visual;
    [SerializeField] private RectTransform rectTransMask;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private BoosterType boosterType;

    private void Start()
    {
        closeBtn.onClick.AddListener(OnCancelUseBooster);
        RegisterListener();
    }
    
    protected void OnDestroy()
    {
        RemoveListener();
        DOTween.Kill(this);
    }
    private bool hasRegisterEvent;
    private IDisposable sub;

    private void RegisterListener()
    {
        if (hasRegisterEvent) return;
        var obs = Observer.GetObservable(ObserverName.screen_resize, 1);
        sub = obs.Subscribe(OnScreenResize);
        hasRegisterEvent = true;
    }
    private void RemoveListener()
    {
        if (!hasRegisterEvent) return;
        hasRegisterEvent = false;
        sub.Dispose();
    }

    private void OnScreenResize(object v)
    {
        if (Screen.height / (float)Screen.width > 1.85f)
        {
            visual.localScale = Vector3.one * .875f;
        }
        else
        {
            visual.localScale = Vector3.one;
        }
    }
    
    private void OnEnable()
    {
        OnScreenResize(null);
    }
    
    public void Initialized(BoosterInfo info)
    {
        nameText.SetText(info.name);
        instructText.SetText(info.instruct);
        icon.sprite = info.bigIcon;
        boosterType = info.type;
        
        closeBtn.gameObject.SetActive(BoosterManager.IsTutorialDone(info.type));
        
        if (info.type == BoosterType.Hand)
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            var bounds = BoundsHelperUI.GetBoundsFromPoints(new List<Vector3> { min, max }, new Vector3(0, 0, 0));

            ShowMask(bounds);
            float timeAnimation = 0.35f;
            var offset = new Vector3(0, 0, 3f);
            var screenOffset = (Vector2)CameraManager.Instance.WorldToScreenOffset(offset);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, screenOffset, UIManager.Instance.canvasScreen.worldCamera, out var localPoint);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, Vector2.zero, UIManager.Instance.canvasScreen.worldCamera, out var localPoint2);
            DOVirtual.DelayedCall(timeAnimation / 2, () =>
            {
                rectTransMask.DOAnchorPos(rectTransMask.anchoredPosition + (localPoint - localPoint2), timeAnimation).SetId(this);
            });
            CameraManager.Instance.MoveCamera(-offset, timeAnimation, timeAnimation / 2, () => { });
        }
        else if (info.type == BoosterType.Clear)
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            var bounds = BoundsHelperUI.GetBoundsFromPoints(new List<Vector3> { min, max }, new Vector3(0, 0, 0));
            ShowMask(bounds);
        }
    }

    public void ShowMask(Bounds bounds)
    {
        rectTransMask.localScale = Vector3.one;

      
        UIMapRectHelper.FitRectToMap(bounds, rectTransMask, UIManager.Instance.canvas);
        rectTransMask.localScale = Vector3.one * 10;
        canvasGroup.alpha = 0;
        GameManager.GameState = GameState.Pause;
        float timeAnimation = 0.35f;
        canvasGroup.DOFade(1, timeAnimation / 3 * 2).SetId(this);
        rectTransMask.DOScale(Vector3.one, timeAnimation).SetEase(Ease.OutQuart).SetId(this).OnComplete(() =>
        {
            GameManager.GameState = GameState.Playing;
        });
        // var uiInGame = UIManager.Instance.GetScreen<UIInGame>();
        // if (uiInGame != null)
        // {
        //     uiInGame.HideBottom();
        // }

    }
    
    private void OnCancelUseBooster()
    {
        BoosterManager.Instance.CancelUseBooster();
    }

    public void Hide()
    {
        if (boosterType == BoosterType.Hand)
        {
            CameraManager.Instance.MoveCamera(Vector3.zero, 0.35f, 0, null);
        }
        gameObject.SetActive(false);
    }
}
