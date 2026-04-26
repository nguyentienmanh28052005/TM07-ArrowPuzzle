using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameMenuCanvas : MonoBehaviour
{
    #region [ REFERENCES & SETTINGS ]
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI textLevel;

    [Header("UI Panels (CanvasGroup)")]
    [SerializeField] private CanvasGroup panelHome;     
    [SerializeField] private CanvasGroup panelShop;
    [SerializeField] private CanvasGroup panelSetting;  
    [SerializeField] private float panelFadeDuration = 0.2f;

    [Header("Slider Tab Navigation")]
    [SerializeField] private GameObject slider;
    [SerializeField] private GameObject btnHome;
    [SerializeField] private GameObject btnShop;
    [SerializeField] private GameObject btnSetting;
    [SerializeField] private float sliderMoveDuration = 0.2f; 
    [SerializeField] private Ease sliderEaseType = Ease.OutQuad;

    [Header("Level Generation")]
    [SerializeField] private List<GameObject> listLevelButtons;

    [SerializeField] private TextMeshProUGUI textCoin;
    [SerializeField] private TextMeshProUGUI textDiamond;

    private bool _hasStarted = false;
    private CanvasGroup _currentPanel; 
    private Coroutine _postTabRefreshRoutine;

    [SerializeField] private TMP_InputField levelReset;
    #endregion

    #region [ LIFECYCLE ]
    private IEnumerator Start()
    {
        _hasStarted = true;
        UpdateLevelUI(); 
        UpdateCurrencyUI((int)CurrencyManager.Instance.Coins, (int)CurrencyManager.Instance.Diamonds);

        HidePanelImmediate(panelShop);
        HidePanelImmediate(panelSetting);
        ShowPanelImmediate(panelHome);
        _currentPanel = panelHome;
        
        // BÍ QUYẾT TỐI THƯỢNG: Ép luồng code đợi đến cuối khung hình
        // Lúc này Horizontal Layout Group chắc chắn đã xếp xong 3 nút
        yield return new WaitForEndOfFrame();
        
        MoveSliderToButton(btnHome, true);

        
    }

    public void OnEnable()
    {
        if (_hasStarted)
        {
            UpdateLevelUI();
            UpdateCurrencyUI((int)CurrencyManager.Instance.Coins, (int)CurrencyManager.Instance.Diamonds);
        }
    }
    #endregion

    #region [ LOGIC TAB NAVIGATION & PANEL SWITCHER ]
    public void OnClickTabHome() => SwitchTabLogic(btnHome, panelHome);
    public void OnClickTabShop() => SwitchTabLogic(btnShop, panelShop);
    public void OnClickTabSetting() => SwitchTabLogic(btnSetting, panelSetting);

    private void SwitchTabLogic(GameObject targetButton, CanvasGroup targetPanel)
    {
        EventSystem.current?.SetSelectedGameObject(null);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(AudioManager.Instance.btnClick, 1f);
        if (_currentPanel == targetPanel)
        {
            RefreshTabButtonVisuals(targetButton);
            return;
        }

        MoveSliderToButton(targetButton);

        // Some UI scripts (pressed/hover transitions) can overwrite colors later in the same frame.
        // Re-apply selected visuals on the next frame so the tab state is always correct.
        if (_postTabRefreshRoutine != null) StopCoroutine(_postTabRefreshRoutine);
        _postTabRefreshRoutine = StartCoroutine(PostTabRefresh(targetButton));

        if (_currentPanel != null)
        {
            CanvasGroup oldPanel = _currentPanel; 
            
            oldPanel.interactable = false; 
            oldPanel.blocksRaycasts = false;
            
            oldPanel.DOKill();
            oldPanel.DOFade(0f, panelFadeDuration).OnComplete(() => {
                oldPanel.gameObject.SetActive(false); 
            });
        }

        _currentPanel = targetPanel;
        _currentPanel.gameObject.SetActive(true);
        _currentPanel.DOKill();
        _currentPanel.DOFade(1f, panelFadeDuration).OnComplete(() => {
            _currentPanel.interactable = true;
            _currentPanel.blocksRaycasts = true;
        });
    }

    private IEnumerator PostTabRefresh(GameObject selectedButton)
    {
        yield return null;
        RefreshTabButtonVisuals(selectedButton, true);
    }

    public void MoveSliderToButton(GameObject targetButton, bool instant = false)
    {
        if (targetButton == null) return;

        RefreshTabButtonVisuals(targetButton, instant);

        if (slider == null) return;

        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        RectTransform targetRect = targetButton.GetComponent<RectTransform>();

        if (sliderRect == null || targetRect == null) return;

        // DỪNG TẤT CẢ TWEEN CŨ TRƯỚC KHI BẮT ĐẦU ĐỂ TRÁNH XUNG ĐỘT
        sliderRect.DOKill(); 

        RectTransform sliderParent = sliderRect.parent as RectTransform;
        if (sliderParent == null) return;

        Canvas canvas = sliderRect.GetComponentInParent<Canvas>();
        Camera uiCamera = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, targetRect.position);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(sliderParent, screenPoint, uiCamera, out Vector2 localPoint))
        {
            return;
        }

        float targetX = localPoint.x;

        if (instant)
        {
            Vector2 anchored = sliderRect.anchoredPosition;
            anchored.x = targetX;
            sliderRect.anchoredPosition = anchored;
        }
        else
        {
            float distance = Mathf.Abs(targetX - sliderRect.anchoredPosition.x);
            float originalWidth = sliderRect.sizeDelta.x; // Chiều rộng gốc của slider
            float stretchedWidth = originalWidth + distance * 0.7f; // Kéo giãn thêm 70% khoảng cách

            Sequence slideSeq = DOTween.Sequence();
            
            // 1. Nửa thời gian đầu: Kéo giãn chiều ngang (Stretch) ra như kẹo kéo
            slideSeq.Append(sliderRect.DOSizeDelta(new Vector2(stretchedWidth, sliderRect.sizeDelta.y), sliderMoveDuration * 0.5f).SetEase(Ease.OutQuad));
            
            // Cùng lúc đó: Di chuyển tọa độ X tới đích
            slideSeq.Join(sliderRect.DOAnchorPosX(targetX, sliderMoveDuration).SetEase(Ease.InOutSine));
            
            // 2. Nửa thời gian sau: Co rút chiều ngang lại bằng kích thước cũ (Snap back)
            slideSeq.Append(sliderRect.DOSizeDelta(new Vector2(originalWidth, sliderRect.sizeDelta.y), sliderMoveDuration * 0.5f).SetEase(Ease.OutBack));
        }
    }

    private void RefreshTabButtonVisuals(GameObject selectedButton, bool instant = false)
    {
        SetTabButtonVisual(btnHome, btnHome == selectedButton, instant);
        SetTabButtonVisual(btnShop, btnShop == selectedButton, instant);
        SetTabButtonVisual(btnSetting, btnSetting == selectedButton, instant);
    }

    private void SetTabButtonVisual(GameObject buttonObj, bool isSelected, bool instant)
    {
        if (buttonObj == null) return;

        MenuTabButton tabButton = buttonObj.GetComponent<MenuTabButton>();
        if (tabButton == null)
        {
            tabButton = buttonObj.GetComponentInChildren<MenuTabButton>(true);
        }
        if (tabButton == null) return;

        tabButton.SetSelected(isSelected, instant);
        if (isSelected && !instant)
        {
            tabButton.PlaySelectAnimation();
        }
    }

    private void HidePanelImmediate(CanvasGroup panel)
    {
        if (panel == null) return;
        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;
        panel.gameObject.SetActive(false);
    }

    private void ShowPanelImmediate(CanvasGroup panel)
    {
        if (panel == null) return;
        panel.gameObject.SetActive(true);
        panel.alpha = 1f;
        panel.interactable = true;
        panel.blocksRaycasts = true;
    }
    #endregion

    #region [ LEVEL GENERATION LOGIC ]
    public void ResetGame()
    {
        GameManager.Instance.level = levelReset.text != "" ? int.Parse(levelReset.text) : 1;
        SceneController.Instance.LoadScene("GameMenu", false, false);
    }

    public void EnterGame()
    {
        SceneController.Instance.LoadScene("GameScene", false, false);
    }

    public void UpdateLevelUI()
    {
        try
        {
            if (textLevel != null) textLevel.text = "Level " + GameManager.Instance.level;
            UpdateLevelButtons();
        }
        catch (System.Exception e)
        {
            if (textLevel != null) textLevel.text = "Lỗi UI Tổng: " + e.Message;
        }
    }

    public void UpdateCurrencyUI(int coins, int diamonds)
    {
        if (textCoin != null) textCoin.text = coins.ToString();
        if (textDiamond != null) textDiamond.text = diamonds.ToString();
    }

    private void UpdateLevelButtons()
    {
        int currentLevel = GameManager.Instance.level;
        int maxUnlockedLevel = GameManager.Instance.currentMaxLevel;
        int totalLevelsInGame = GameManager.Instance.levelDataSOs != null ? GameManager.Instance.levelDataSOs.Count : 999;

        for (int i = 0; i < listLevelButtons.Count; i++)
        {
            try
            {
                GameObject btnObj = listLevelButtons[i];
                if (btnObj == null) continue; 

                int assignedLevel = currentLevel + i; 

                if (!btnObj.activeSelf) btnObj.SetActive(true);

                TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = assignedLevel.ToString();
                }
                else
                {
                    throw new System.Exception("Mất Component TMP ở Object con!"); 
                }

                bool isPlayable = (assignedLevel <= currentLevel) && (assignedLevel <= totalLevelsInGame);

                ButtonClicky customBtn = btnObj.GetComponent<ButtonClicky>();
                if (customBtn != null)
                {
                    customBtn.SetInteractable(isPlayable);
                }
                else 
                {
                    Button unityBtn = btnObj.GetComponent<Button>();
                    if (unityBtn != null) unityBtn.interactable = isPlayable;
                }
            }
            catch (System.Exception ex)
            {
                if (textLevel != null)
                {
                    textLevel.fontSize = 25; 
                    textLevel.color = Color.red; 
                    textLevel.text = $"Lỗi Nút {i}: {ex.GetType().Name} \n {ex.Message}";
                }
                break; 
            }
        }
    }

    public void NextLevel()
    {
        if(GameManager.Instance.level < GameManager.Instance.currentMaxLevel)
        {
            GameManager.Instance.level++;
            UpdateLevelUI(); 
        }
    }

    public void PreviousLevel()
    {
        if(GameManager.Instance.level > 1)
        {
            GameManager.Instance.level--;
            UpdateLevelUI(); 
        }
    }
    #endregion

    public void Gift()
    {
        CurrencyManager.Instance.AddHintTool(5);
        CurrencyManager.Instance.AddEraseTool(5);
        CurrencyManager.Instance.AddDashTool(5);
        CurrencyManager.Instance.AddCoins(10000);
        CurrencyManager.Instance.AddDiamonds(2000);
    }
    
}