using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuCanvas : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI textLevel;

    [Header("UI Panels (CanvasGroup)")]
    [SerializeField] private CanvasGroup panelHome;     // Thay thế cho panelMainMenu cũ
    [SerializeField] private CanvasGroup panelShop;
    [SerializeField] private CanvasGroup panelSetting;  // Thay thế cho panelSetting cũ
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

    private bool _hasStarted = false;
    private CanvasGroup _currentPanel; // Lưu trữ bảng đang hiển thị

    private void Start()
    {
        _hasStarted = true;
        UpdateLevelUI(); 

        // Khởi tạo: Giấu các bảng phụ, chỉ hiện bảng chính (Home)
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
        }
    }

    // ==========================================
    // LOGIC TAB NAVIGATION & PANEL SWITCHER
    // ==========================================
    
    // Gắn 3 hàm này vào sự kiện OnClick() của 3 nút tương ứng trên Inspector
    public void OnClickTabHome() => SwitchTabLogic(btnHome, panelHome);
    public void OnClickTabShop() => SwitchTabLogic(btnShop, panelShop);
    public void OnClickTabSetting() => SwitchTabLogic(btnSetting, panelSetting);

    private void SwitchTabLogic(GameObject targetButton, CanvasGroup targetPanel)
    {
        // 1. Nếu người chơi bấm lại vào Tab đang mở thì bỏ qua
        if (_currentPanel == targetPanel) return;

        // 2. Di chuyển Slider và kích hoạt nhún nhảy Icon
        MoveSliderToButton(targetButton);

        // 3. Hiệu ứng mờ dần (Fade Out) cho bảng cũ
        if (_currentPanel != null)
        {
            CanvasGroup oldPanel = _currentPanel; 
            
            // Khóa tương tác ngay lập tức để chống spam click lúc đang mờ
            oldPanel.interactable = false; 
            oldPanel.blocksRaycasts = false;
            
            oldPanel.DOKill();
            oldPanel.DOFade(0f, panelFadeDuration).OnComplete(() => {
                oldPanel.gameObject.SetActive(false); // Tắt hẳn sau khi mờ xong
            });
        }

        // 4. Hiệu ứng rõ dần (Fade In) cho bảng mới
        _currentPanel = targetPanel;
        _currentPanel.gameObject.SetActive(true);
        _currentPanel.DOKill();
        _currentPanel.DOFade(1f, panelFadeDuration).OnComplete(() => {
            // Sáng rõ 100% rồi mới mở khóa tương tác
            _currentPanel.interactable = true;
            _currentPanel.blocksRaycasts = true;
        });
    }

    public void MoveSliderToButton(GameObject targetButton)
    {
        if (slider == null || targetButton == null) return;

        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        RectTransform targetRect = targetButton.GetComponent<RectTransform>();

        if (sliderRect != null && targetRect != null)
        {
            sliderRect.DOKill(true); // Dịch chuyển tức thời đến đích cũ nếu bị chuyển hướng đột ngột
            
            float targetX = targetRect.anchoredPosition.x;
            sliderRect.DOAnchorPosX(targetX, sliderMoveDuration).SetEase(sliderEaseType);

            targetButton.GetComponent<MenuTabButton>()?.PlaySelectAnimation();
        }
    }

    // Hàm phụ trợ ẩn hiện nhanh không qua animation (Dùng cho lúc mới khởi động game)
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

    // ==========================================
    // CÁC LOGIC KHÁC CỦA BẠN (GIỮ NGUYÊN)
    // ==========================================
    public void ResetGame()
    {
        GameManager.Instance.level = 1;
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
}