using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
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
    [SerializeField] private float sliderMoveDuration = 0.01f;
    [SerializeField] private Ease sliderEaseType = Ease.OutQuad;

    [Header("Level Generation")]
    [SerializeField] private List<GameObject> listLevelButtons;


    [SerializeField] private TextMeshProUGUI textCoin;
    [SerializeField] private TextMeshProUGUI textDiamond;

    private bool _hasStarted = false;
    private CanvasGroup _currentPanel; 
    #endregion

    #region [ LIFECYCLE ]
    private void Start()
    {
        _hasStarted = true;
        UpdateLevelUI(); 
        UpdateCurrencyUI((int)CurrencyManager.Instance.Coins, (int)CurrencyManager.Instance.Diamonds);

        HidePanelImmediate(panelShop);
        HidePanelImmediate(panelSetting);
        ShowPanelImmediate(panelHome);
        _currentPanel = panelHome;
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

    /// <summary>
    /// Xử lý logic chuyển đổi UI Panel với hiệu ứng Fade và ngăn chặn click spam.
    /// </summary>
    private void SwitchTabLogic(GameObject targetButton, CanvasGroup targetPanel)
    {
        if (_currentPanel == targetPanel) return;

        MoveSliderToButton(targetButton);

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

    /// <summary>
    /// Dịch chuyển thanh trượt (Slider) đến vị trí của Tab tương ứng.
    /// </summary>
    public void MoveSliderToButton(GameObject targetButton)
    {
        if (slider == null || targetButton == null) return;

        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        RectTransform targetRect = targetButton.GetComponent<RectTransform>();

        if (sliderRect != null && targetRect != null)
        {
            sliderRect.DOKill(true); 
            
            float targetX = targetRect.anchoredPosition.x;
            sliderRect.DOAnchorPosX(targetX, sliderMoveDuration).SetEase(sliderEaseType);

            targetButton.GetComponent<MenuTabButton>()?.PlaySelectAnimation();
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
        GameManager.Instance.level = 1;
        SceneController.Instance.LoadScene("GameMenu", false, false);
    }

    public void EnterGame()
    {
        SceneController.Instance.LoadScene("GameScene", false, false);
    }

    /// <summary>
    /// Làm mới giao diện chỉ số Level và trạng thái khóa/mở khóa của các nút Level.
    /// </summary>
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
}