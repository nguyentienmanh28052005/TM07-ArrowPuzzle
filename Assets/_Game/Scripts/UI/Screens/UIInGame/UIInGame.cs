using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using DG.Tweening;
using master;
using mygame.sdk;
using UniRx;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Observer = master.Observer;
using Random = UnityEngine.Random;

public class UIInGame : ScreenUI
{
    public enum TipType
    {
        None,
        AddHole,
        BreakObject,
        ClickScrew,
        Rotate,
        Zoom,
        ClearHole
    }

    [Serializable]
    public class TipInfo
    {
        public TipType type;
        public string desc;
        public Animation suggest;
    }

    [SerializeField] private TipInfo[] tipInfos;

    [SerializeField] private Text tipText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text levelTextSize;
    [SerializeField] private Text txtHack;
    [SerializeField] private Button skipCompleteBtn;

    [Space(20)] [SerializeField] public UIUseBoosterInfo useBoosterInfo;
    public UIBoosterBtn[] boosterBtns;

    [SerializeField] private GameObject groupHack;
    [SerializeField] private GameObject warningDefeat;
    [SerializeField] private GameObject completeFx;
    [SerializeField] private GameObject hardLevelFx;
    [SerializeField] private GameObject crazyLevelFx;
    [SerializeField] private RectTransform bottomBG;
    [SerializeField] private RectTransform bgLevel;
    [SerializeField] private RectTransform topBar;
    [SerializeField] private Animation screenanimation;
    [SerializeField] private TipTutorial tipTutorial;
    [SerializeField] private PanelSetting panelSetting;
    [SerializeField] private RectTransform itemDisplay;
    [SerializeField] private RectTransform heartDisplay;
    [SerializeField] private RectTransform goldDisplay;
    [SerializeField] private GameObject[] bgLevels;
    [SerializeField] private CanvasGroup coreGroup;

    [Header("Tutorial Settings")]
    [SerializeField] private bool enableMechanicTutorial = true;

    private bool stageOld;
    private TipType currentTip;
    private Action onComplete;
    private Tween delayTween2;
    private Tween delayTween;
    private bool isShowTutorial = false;

    public bool isShowHandTutorial { get; set; }
    public static float timeDelayFXHard = 1.75f;
    private const int NumberWinClip = 5;

    bool hasRegisterEvent;
    IDisposable sub;

    private void Awake()
    {
        RegisterListener();
    }

    private void OnDestroy()
    {
        RemoveListener();
        delayTween?.Kill();
        delayTween2?.Kill();
        topBar?.DOKill();
        coreGroup?.DOKill();
    }

    public override void Initialize(UIManager uiManager)
    {
        base.Initialize(uiManager);
        skipCompleteBtn.onClick.AddListener(OnClickSkipCompleteFx);

#if ENABLE_HACK
        DebugMenuManager.allowedDeviceIds = new[] { SystemInfo.deviceName, SystemInfo.deviceUniqueIdentifier };
#endif
        SetActiveGroupHack(false);
        string deviceName = SystemInfo.deviceName;
        string deviceId = SystemInfo.deviceUniqueIdentifier;
        Debug.Log("Device ID: " + SystemInfo.deviceUniqueIdentifier);

        bool isAllowed = DebugMenuManager.allowedDeviceIds.Contains(deviceId) ||
                         DebugMenuManager.allowedDeviceIds.Contains(UserDataManager.Instance.UserData.playerUUID) ||
                         DebugMenuManager.allowedDeviceIds.Contains(UserDataManager.Instance.UserData.name);

        if (isAllowed)
        {
            var btn = levelText.gameObject.AddComponent<Button>();
            if (btn == null) btn = levelText.GetComponent<Button>();
            btn.targetGraphic = levelText;
            btn.onClick.AddListener(() => SetActiveGroupHack(!groupHack.activeSelf));

            btn = txtHack.gameObject.AddComponent<Button>();
            if (btn == null) btn = txtHack.GetComponent<Button>();
            btn.targetGraphic = txtHack;
            btn.onClick.AddListener(() => SetActiveGroupHack(!groupHack.activeSelf));

            btn = levelTextSize.gameObject.AddComponent<Button>();
            if (btn == null) btn = levelTextSize.GetComponent<Button>();
            btn.targetGraphic = levelTextSize;
            btn.onClick.AddListener(() => SetActiveGroupHack(!groupHack.activeSelf));
        }
    }

    public void SetActiveGoldDisplay(bool active)
    {
        goldDisplay.GetComponent<CanvasGroup>()?.DOFade(active ? 1 : 0, .25f);
        heartDisplay.GetComponent<CanvasGroup>()?.DOFade(active ? 1 : 0, .25f);
        goldDisplay.GetComponent<CanvasGroup>().blocksRaycasts = active;
        heartDisplay.GetComponent<CanvasGroup>().blocksRaycasts = active;
    }

    private void SetActiveGroupHack(bool active)
    {
        groupHack.SetActive(active);
    }

    private void RegisterListener()
    {
        if (hasRegisterEvent) return;
        hasRegisterEvent = true;
        var ob1 = Observer.GetObservable(ObserverName.screen_resize, 0);
        sub = ob1.Subscribe(x => { OnScreenSize(); });
    }


    private void RemoveListener()
    {
        if (!hasRegisterEvent) return;
        hasRegisterEvent = false;
        sub.Dispose();
    }

    void OnScreenSize()
    {
    }

    public override void Active()
    {
        base.Active();
        LogEvent.ScreenGo(LogEvent.ScreenName.LevelPlay);
        for (int i = 0; i < boosterBtns.Length; i++)
        {
            boosterBtns[i].Initialized();
        }

        OnScreenSize();
        for (int i = 0; i < bgLevels.Length; i++)
        {
            bgLevels[i].SetActive((int)LevelManager.GetLevelType(DataManager.Level) == i);
        }

        switch (LevelManager.GetLevelType(DataManager.Level))
        {
            //case LevelType.Crazy:
            //    if (ColorUtility.TryParseHtmlString("#D94234", out Color color))
            //    {
            //        levelText.color = color;
            //    }

            //    Shadow[] shadows = levelText.GetComponents<Shadow>();
            //    for (int i = 0; i < shadows.Length; i++)
            //    {
            //        shadows[i].effectColor = new Color32(137, 25, 20, 255);
            //    }

            //    break;
            //case LevelType.Hard:
            //    if (ColorUtility.TryParseHtmlString("#A134E2", out Color color2))
            //    {
            //        levelText.color = color2;
            //    }

            //    Shadow[] shadows2 = levelText.GetComponents<Shadow>();
            //    for (int i = 0; i < shadows2.Length; i++)
            //    {
            //        shadows2[i].effectColor = new Color32(128, 23, 147, 255);
            //    }
            //    break;
            default:
                levelText.color = Color.white;
                Shadow[] shadows3 = levelText.GetComponents<Shadow>();
                for (int i = 0; i < shadows3.Length; i++)
                {
                    shadows3[i].effectColor = Color.black;
                }

                break;
        }

        AudioManager.Instance.PlayBGMusicInGame();
        isShowHandTutorial = false;
        // previewLevel.gameObject.SetActive(false);
        // topBar.gameObject.SetActive(false);
        // bottomBG.gameObject.SetActive(false);
        // previewLevel.gameObject.SetActive(false);
        HideUseBoosterUI();
        HideTipTutorial();
        coreGroup.DOKill();
        coreGroup.alpha = 1;
        screenanimation.Play("PreIntro");
        isShowTutorial = false;
    }

    public void Initialized(int level, LevelType levelType)
    {
        //textSkeleton.AnimationState.ClearTracks();
        useBoosterInfo.gameObject.SetActive(false);
        completeFx.gameObject.SetActive(false);
        hardLevelFx.gameObject.SetActive(false);
        crazyLevelFx.gameObject.SetActive(false);

        topBar.gameObject.SetActive(true);
        bottomBG.gameObject.SetActive(DataManager.Level >= PlayerPrefsUtil.CF_LevelShowBooster);
        SetActiveGoldDisplay(true);
        // if (levelType != LevelType.Easy)
        {
            levelText.SetText("_level_x", StateCapText.FirstCap, FormatText.F_String, level);
            levelText.gameObject.SetActive(true);
            levelTextSize.gameObject.SetActive(false);
        }
        // else
        // {
        //     levelTextSize.SetText("_level_x", StateCapText.FirstCap, FormatText.F_String, level);
        //     levelText.gameObject.SetActive(false);
        //     levelTextSize.gameObject.SetActive(true);
        // }

        if (level == 1)
        {
            panelSetting.gameObject.SetActive(false);
            var startPoint = -255;
            for (int i = 0; i < boosterBtns.Length; i++)
            {
                boosterBtns[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(startPoint + i * 255f, 132.5f);
            }
        }
        else
        {
            var startPoint = -375f;
            for (int i = 0; i < boosterBtns.Length; i++)
            {
                boosterBtns[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(startPoint + i * 250f, 132.5f);
            }
            panelSetting.gameObject.SetActive(true);
        }
        //UpdatePainBarProgress(0, 0, 0);
    }

    public void HideOrShowTopBar()
    {
        if (UIManager.Instance.HasPopupShowing())
        {
            topBar?.DOKill();
            topBar.DOAnchorPosY(300, .35f).SetLink(gameObject);
        }
        else
        {
            topBar?.DOKill();
            topBar.DOAnchorPosY(0, .2f).SetLink(gameObject);
        }
    }
    
    public void PlayCompleteFx(Action complete)
    {
        int clipIndex = Random.Range(0, NumberWinClip);
        AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Win_Level + $"_{clipIndex}", 0.9f);
        completeFx.SetActive(true);
        skipCompleteBtn.enabled = false;
        onComplete = complete;
        delayTween = DOVirtual.DelayedCall(3.4f, () => { onComplete?.Invoke(); }, false).SetLink(gameObject);
        delayTween2 = DOVirtual.DelayedCall(1f, () => { skipCompleteBtn.enabled = true; }, false).SetLink(gameObject);
        coreGroup.DOFade(0, .35f).SetLink(gameObject);
    }
    public void HideCompleteFx()
    {
        completeFx.gameObject.SetActive(false);
    }
    private void OnClickSkipCompleteFx()
    {
        delayTween?.Kill();
        delayTween2?.Kill();
        skipCompleteBtn.enabled = false;
        //var skeletonGraphics = completeFx.GetComponentsInChildren<SkeletonGraphic>();
        //for (int i = 0; i < skeletonGraphics.Length; i++)
        //{
        //    skeletonGraphics[i].gameObject.SetActive(false);
        //}
        onComplete?.Invoke();
    }

    public void PlayHardLevelFx(LevelType levelType)
    {
        float timeDelay = timeDelayFXHard;
        if (levelType == LevelType.Hard)
        {
            hardLevelFx.SetActive(true);
        }
        else if (levelType == LevelType.Crazy)
        {
            crazyLevelFx.SetActive(true);
        }
        else if (levelType == LevelType.Easy)
        {
            timeDelay = 0;
        }

        if (timeDelay == 0)
        {
            hardLevelFx.gameObject.SetActive(false);
            crazyLevelFx.gameObject.SetActive(false);
            CheckTutorial();
        }
        else
        {
            DOVirtual.DelayedCall(timeDelay - .1f, () =>
            {
                if (this != null)
                {
                    DOVirtual.DelayedCall(.85f, () =>
                    {
                        if (this != null)
                        {
                            hardLevelFx.gameObject.SetActive(false);
                            crazyLevelFx.gameObject.SetActive(false);
                        }
                    }, false).SetUpdate(false);

                    CheckTutorial();
                }
            }, false).SetUpdate(false);
        }
    }

    public void CheckTutorial()
    {
        if (isShowTutorial)
        {
            return;
        }

        isShowTutorial = true;

        if (enableMechanicTutorial)
        {
            foreach (MechanicTutorialType type in Enum.GetValues(typeof(MechanicTutorialType)))
            {
                if (type == MechanicTutorialType.None)
                    continue;
                if (LevelManager.Instance.IsHaveMechanic(type) && PopupTutorialMechanic.IsShowedMechanic(type) == false)
                {
                    var popup = UIManager.Instance.ShowPopup<PopupTutorialMechanic>(CheckBoosterUsed);
                    popup.Initialize(type);
                    return;
                }
            }
        }

        CheckBoosterUsed();
    }

    public bool CheckTutorialBooster()
    {
        for (int i = 0; i < boosterBtns.Length; i++)
        {
            bool isTut = boosterBtns[i].CheckTutorial();
            if (isTut)
            {
                isShowHandTutorial = true;
                return true;
            }
        }

        return false;
    }

    public void WarningDefeat()
    {
        warningDefeat.SetActive(true);
        var im = warningDefeat.GetComponentsInChildren<Image>();
        for (int i = 0; i < im.Length; i++)
        {
            im[i]?.DOKill();
            im[i].color = Color.white;
        }

        DOVirtual.DelayedCall(4f, () =>
        {
            for (int i = 0; i < im.Length; i++)
            {
                if (im[i] != null)
                {
                    im[i].DOFade(.5f, .5f);
                }
            }
        });
    }

    public void HideWarningDefeat()
    {
        warningDefeat.SetActive(false);
    }

    public UIBoosterBtn GetUIBoosterBtn(BoosterType type)
    {
        foreach (var btn in boosterBtns)
        {
            if (btn.boosterType == type)
            {
                return btn;
            }
        }

        return null;
    }

    public void IntroLevel(float startTime)
    {
        screenanimation.Play("IntroLevel");
        screenanimation["IntroLevel"].time = startTime;
    }

    public void EnableAppearBoosterAnim(bool enable)
    {
        screenanimation.Play(enable ? "AppearBooster" : "DisappearBooster");
    }


    public void ShowUserBoosterUI(BoosterInfo boosterInfo)
    {
        useBoosterInfo.gameObject.SetActive(true);
        useBoosterInfo.Initialized(boosterInfo);
        screenanimation.Play("ShowUseBooster");
        tipText.enabled = false;
    }

    public void HideUseBoosterUI()
    {
        useBoosterInfo.Hide();
        screenanimation.Play("HideUseBooster");
        tipText.enabled = true;
        for (int i = 0; i < boosterBtns.Length; i++)
        {
            boosterBtns[i].Initialized();
        }
    }

    public void CheckBoosterUsed()
    {
        if (LevelManager.Instance.GetProgress() > 0)
        {
            return;
        }

        if (DataManager.Level <= ConfigManager.CF_LevelShowFirstTutorial)
        {
            return;
        }

        CheckTutorialBooster();
    }

    public void ShowTipTutorial(string key)
    {
        RectTransform rectTransform = tipTutorial.GetComponent<RectTransform>();
        tipTutorial.Hide();
        if (DataManager.Level == 1)
        {
            rectTransform.anchorMax = new Vector2(1, 0.5f);
            rectTransform.anchorMin = new Vector2(0, .25f);
            rectTransform.anchoredPosition = new Vector2(0, 50);
        }
        else
        {
            rectTransform.anchorMax = new Vector2(.5f, 0);
            rectTransform.anchorMin = new Vector2(.5f, 0);

            rectTransform.anchoredPosition = new Vector2(0, 320);
        }

        tipTutorial.Show();
        tipTutorial.SetTipText(key);
    }

    public void HideTipTutorial()
    {
        tipTutorial.Hide();
    }
    [SerializeField] private CheatUI cheatUI;
#if UNITY_EDITOR && ENABLE_HACK
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            cheatUI.OnClickNextLevel(1);
        }else if (Input.GetKeyDown(KeyCode.B))
        {
            cheatUI.OnClickNextLevel(-1);
        }
    }
#endif
}