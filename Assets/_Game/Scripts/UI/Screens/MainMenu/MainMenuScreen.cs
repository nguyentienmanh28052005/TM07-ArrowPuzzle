using System;
using mygame.sdk;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Crystal;
using DanielLochner.Assets.SimpleScrollSnap;
using master;
using Observer = master.Observer;
using UniRx;
using Unity.VisualScripting;

public class MainMenuScreen : ScreenUI
{
    [System.Serializable]
    public class Group
    {
        public MainMenuPanel panel;
        public ButtonMainMenu button;
    }
    [SerializeField] private SimpleScrollSnap pageScrollSnap;
    [SerializeField] private RectTransform tabBG;

    public Group[] groups;

    private bool hasRegisterEvent;
    private IDisposable sub;
    
    private int startPanel = 1;
    private int currentPanel = -1;

    [Range(0f, 1f)] [SerializeField] private float selectedSizePercent = 0.37f;
    [Range(0f, 1f)] [SerializeField] private float normalSizePercent = 0.33f;
    private int selectedSize = 400;
    private int pageCount = 0;
    private int normalSize = 360;
    private int [] buttonPosition;
    private string _homeButtonNameClick = "auto";

    
    public override void Initialize(UIManager uiManager)
    {
        base.Initialize(uiManager);
        for (int i = 0; i < groups.Length; i++)
        {
            var idx = i;
            groups[i].button?.SetEventClick(() => ActivePanel(idx));
            groups[i].panel?.Initialize(this);
        }

        _homeButtonNameClick = GameManager.ReasonBackToHome == ReasonBackToHome.Lose
            ? LogEvent.ButtonName.ButtonCancelRetry
            : LogEvent.ButtonName.ButtonHome;
        SetupScroll();
    }

    private void RegisterListener()
    {
        if (hasRegisterEvent) return;
        hasRegisterEvent = true;
        var ob1 = Observer.GetObservable(ObserverName.screen_resize, 0);
        sub = ob1.Subscribe(x => { OnScreenSize(); });
    }
    private void SetupScroll()
    {
        pageScrollSnap.Setup();
        pageScrollSnap.ScrollRect.onValueChanged.AddListener(OnPanelSelecting);
        pageScrollSnap.OnPanelSelected.AddListener(OnPanelSelected);
            
        var rect = pageScrollSnap.Content;
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0);
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, 0);

        pageCount = pageScrollSnap.Content.childCount;
        for (int i = 0; i < pageCount; i++)
        {
            rect = pageScrollSnap.Content.GetChild(i) as RectTransform;
            if (rect == null) continue;
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, 0);
        }

        CalculateSizes();
        tabBG.position = new Vector3(groups[startPanel].button.transform.position.x, tabBG.position.y, tabBG.position.z);
        buttonPosition = new int[pageCount];
        GetButtonPosition(startPanel);
        OnPageChange(startPanel);
        pageScrollSnap.GoToPanel(startPanel);
    }

    private void OnPanelSelecting(Vector2 arg0)
    {
        var delta = pageScrollSnap.ScrollRect.horizontalNormalizedPosition;
        var size = buttonPosition[^1] - buttonPosition[0];
        tabBG.anchoredPosition = new Vector2(buttonPosition[0] + delta * size, tabBG.anchoredPosition.y);

    }
    
    private void OnPanelSelected(int selected)
    {
        OnPageChange(pageScrollSnap.CenteredPanel);
        if (groups[currentPanel].panel is HomePanel)
        {
            LogEvent.ScreenGo(LogEvent.ScreenName.Home, _homeButtonNameClick);
            _homeButtonNameClick = LogEvent.ButtonName.ButtonHome;
        }
        else if (groups[currentPanel].panel is ShopPanel)
        {
            LogEvent.ScreenGo(LogEvent.ScreenName.ShopHome, LogEvent.ButtonName.ButtonHome);
        }
        else if (groups[currentPanel].panel is SettingPanel)
        {
            LogEvent.ScreenGo(LogEvent.ScreenName.SettingHome, LogEvent.ButtonName.ButtonHome);
        }
    }
    
    private void OnPageChange(int pageID)
    {
        tabBG.DOKill();
        GetButtonPosition(pageID);
        for (int i = 0; i < groups.Length; i++)
        {
            var btn = groups[i].button.GetComponent<RectTransform>();
            btn.DOAnchorPosX(buttonPosition[i], .35f).SetId(this);
            if (i == pageID)
            {
                btn.DOSizeDelta(new Vector2(selectedSize, btn.sizeDelta.y), .35f).SetId(this);
            }
            else
            {
                btn.DOSizeDelta(new Vector2(normalSize, btn.sizeDelta.y), .35f).SetId(this);
            }
        }
        
        tabBG.DOAnchorPosX(buttonPosition[pageID], .35f).SetId(this);
        tabBG.DOSizeDelta(new Vector2(selectedSize, tabBG.sizeDelta.y), .35f).SetId(this);
        if (currentPanel != pageID)
        {
            if (currentPanel >= 0)
            {
                groups[currentPanel].panel.Deactive();
                groups[currentPanel].button.OnDeselectButton();
            }

            currentPanel = pageID;
            groups[currentPanel].panel.Active();
            groups[currentPanel].button.OnSelectButton();
        }
    }

    private float GetParentWidth()
    {
        if (tabBG != null && tabBG.parent != null)
        {
            var rectTransform = tabBG.parent as RectTransform;
            if (rectTransform != null)
            {
                return rectTransform.rect.width;
            }
        }
        return 1080f; // fallback to default
    }

    private void CalculateSizes()
    {
        float parentWidth = GetParentWidth();
        selectedSize = Mathf.RoundToInt(parentWidth * selectedSizePercent);
        normalSize = Mathf.RoundToInt(parentWidth * normalSizePercent);
    }

    private void GetButtonPosition(int pageID)
    {
        float parentWidth = GetParentWidth();
        var lastPosition = 0f;
        var lastSizeDelta = 0;
        for (int i = 0; i < pageCount; i++)
        {
            var sizeDelta = i == pageID ? selectedSize : normalSize;
            lastPosition += sizeDelta / 2f + lastSizeDelta / 2f;
            lastSizeDelta = sizeDelta;
            buttonPosition[i] = (int) lastPosition - (int)(parentWidth / 2f);
        }
    }


    private void RemoveListener()
    {
        if (!hasRegisterEvent) return;
        hasRegisterEvent = false;
        sub.Dispose();
    }

    void OnScreenSize()
    {
        if (pageCount <= 0) return;
        CalculateSizes();
        int activePanel = currentPanel >= 0 ? currentPanel : startPanel;
        GetButtonPosition(activePanel);
        
        for (int i = 0; i < groups.Length; i++)
        {
            var btn = groups[i].button.GetComponent<RectTransform>();
            btn.DOKill();
            btn.anchoredPosition = new Vector2(buttonPosition[i], btn.anchoredPosition.y);
            if (i == activePanel)
            {
                btn.sizeDelta = new Vector2(selectedSize, btn.sizeDelta.y);
            }
            else
            {
                btn.sizeDelta = new Vector2(normalSize, btn.sizeDelta.y);
            }
        }
        
        tabBG.DOKill();
        tabBG.anchoredPosition = new Vector2(buttonPosition[activePanel], tabBG.anchoredPosition.y);
        tabBG.sizeDelta = new Vector2(selectedSize, tabBG.sizeDelta.y);
    }

    public override void Active()
    {
        base.Active();

        uiManager.ResetResolution();
        ActivePanel(startPanel);
        if (!SplashLoadingCtr.isLoading)
        {
            PlayMusic();
        }
    }

    public void PlayMusic()
    {
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlayBGMusicMain();
    }

    public void ActivePanel(int index)
    {
        if (index == currentPanel) return;
        if (currentPanel >= 0)
        {
            groups[currentPanel].panel.Deactive();
            groups[currentPanel].button.OnDeselectButton();
        }

        currentPanel = index;
        OnPageChange(currentPanel);
        pageScrollSnap.GoToPanel(currentPanel);
        groups[currentPanel].panel.Active();
        groups[currentPanel].button.OnSelectButton();
    }

    public void ResetCurrentPanel()
    {
        currentPanel = -1;
    }

    private void OnEnable()
    {
        Debug.Log($"Main: {1}");
        RegisterListener();
    }

    private void OnDisable()
    {
        Debug.Log($"Main: {0}");
        RemoveListener();
        DOTween.Kill(this);
    }

    public MainMenuPanel GetCurrentPanel()
    {
        if(currentPanel < 0) return null;
        return groups[currentPanel].panel;
    }

    public override void Deactive()
    {
        base.Deactive();
    }
}