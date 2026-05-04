using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenuCanvas : MonoBehaviour, IScreenLifecycle
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
    [SerializeField] private float sliderStretchFactor = 0.45f;
    [SerializeField] private float sliderMaxStretchMultiplier = 1.7f;

    [Header("Level Generation")]
    [SerializeField] private List<GameObject> listLevelButtons;

    [SerializeField] private TextMeshProUGUI textCoin;
    [SerializeField] private TextMeshProUGUI textDiamond;

    private bool _hasStarted = false;
    private CanvasGroup _currentPanel; 

    [SerializeField] private TMP_InputField levelReset;

    private float _defaultSliderWidth = -1f; // Biến lưu kích thước gốc của Slider
    #endregion

    #region [ LIFECYCLE ]
    private IEnumerator Start()
    {
        _hasStarted = true;
        SyncLevelFromSave();
        UpdateLevelUI(); 
        UpdateCurrencyUI((int)CurrencyManager.Instance.Coins, (int)CurrencyManager.Instance.Diamonds);

        HidePanelImmediate(panelShop);
        HidePanelImmediate(panelSetting);
        ShowPanelImmediate(panelHome);
        _currentPanel = panelHome;
        
        // BÍ QUYẾT TỐI THƯỢNG: Ép luồng code đợi đến cuối khung hình
        yield return new WaitForEndOfFrame();

        if (slider != null)
        {
            RectTransform sliderRect = slider.GetComponent<RectTransform>();
            if (sliderRect != null)
            {
                _defaultSliderWidth = sliderRect.sizeDelta.x;
            }
        }
        
        // Gọi lần đầu tiên để set vị trí (instant = true)
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

    public void OnScreenShow()
    {
        if (CurrencyManager.Instance != null)
        {
            UpdateCurrencyUI((int)CurrencyManager.Instance.Coins, (int)CurrencyManager.Instance.Diamonds);
        }

        UpdateLevelUI();
    }

    public void OnScreenHide()
    {
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
        
        // FIX LỖI 1: Nếu click lại tab đang mở -> Bỏ qua không làm gì cả, tránh 3 nút cùng nhảy múa
        if (_currentPanel == targetPanel)
        {
            return;
        }

        // Kích hoạt di chuyển slider và chạy Animation của MenuTabButton
        MoveSliderToButton(targetButton);

        // FIX LỖI 2: Đã loại bỏ hoàn toàn Coroutine PostTabRefresh ở đây! 
        // Nó chính là thủ phạm giết chết Animation khi chuyển tab.

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

    public void MoveSliderToButton(GameObject targetButton, bool instant = false)
    {
        if (targetButton == null || slider == null) return;

        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        RectTransform targetRect = targetButton.GetComponent<RectTransform>();

        if (sliderRect == null || targetRect == null) return;

        // BƯỚC 1: LƯU KÍCH THƯỚC GỐC
        if (_defaultSliderWidth <= 0f) _defaultSliderWidth = sliderRect.sizeDelta.x;

        // BƯỚC 2: RESET TRẠNG THÁI SẠCH (Cực kỳ quan trọng)
        sliderRect.DOKill(); 
        sliderRect.sizeDelta = new Vector2(_defaultSliderWidth, sliderRect.sizeDelta.y);

        // BƯỚC 3: ÉP UNITY CẬP NHẬT TỌA ĐỘ NGAY LẬP TỨC
        // Tránh việc Layout Group chưa kịp xếp xong vị trí các nút
        Canvas.ForceUpdateCanvases();

        RefreshTabButtonVisuals(targetButton, instant);

        RectTransform sliderParent = sliderRect.parent as RectTransform;
        if (sliderParent == null) return;

        // BƯỚC 4: TÍNH TỌA ĐỘ ĐÍCH DỰA TRÊN VỊ TRÍ THẾ GIỚI
        // Cách này an toàn hơn WorldToScreenPoint khi spam click
        float targetX = sliderParent.InverseTransformPoint(targetRect.position).x;

        if (instant)
        {
            sliderRect.anchoredPosition = new Vector2(targetX, sliderRect.anchoredPosition.y);
        }
        else
        {
            // BƯỚC 5: TÍNH KHOẢNG CÁCH DỰA TRÊN TỌA ĐỘ ĐÃ RESET
            float currentX = sliderRect.anchoredPosition.x;
            float distance = Mathf.Abs(targetX - currentX);
            
            // Nếu khoảng cách quá nhỏ (đang ở chính nút đó) thì không cần chạy animation giãn
            if (distance < 1f) 
            {
                sliderRect.DOAnchorPosX(targetX, sliderMoveDuration).SetEase(sliderEaseType);
                return;
            }

            float stretchedWidth = _defaultSliderWidth + (distance * sliderStretchFactor);
            stretchedWidth = Mathf.Min(stretchedWidth, _defaultSliderWidth * sliderMaxStretchMultiplier);

            Sequence slideSeq = DOTween.Sequence();
            
            // Kéo giãn và Di chuyển cùng lúc
            slideSeq.Append(sliderRect.DOSizeDelta(new Vector2(stretchedWidth, sliderRect.sizeDelta.y), sliderMoveDuration * 0.4f).SetEase(Ease.OutQuad));
            slideSeq.Join(sliderRect.DOAnchorPosX(targetX, sliderMoveDuration).SetEase(sliderEaseType));
            
            // Co lại khi gần tới đích
            slideSeq.Append(sliderRect.DOSizeDelta(new Vector2(_defaultSliderWidth, sliderRect.sizeDelta.y), sliderMoveDuration * 0.6f).SetEase(Ease.OutBack));

            slideSeq.SetLink(slider);
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
        int targetLevel = 1;
        if (levelReset != null && !string.IsNullOrWhiteSpace(levelReset.text))
        {
            if (!int.TryParse(levelReset.text, out targetLevel))
            {
                targetLevel = 1;
            }
        }

        targetLevel = Mathf.Max(1, targetLevel);

        // Tìm GameManager thực tế trong scene để tránh bug Singleton static reference
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.level = targetLevel;
        }
        // Đồng bộ cả static Instance (phòng trường hợp Instance trỏ đúng)
        if (GameManager.Instance != null && GameManager.Instance != gm)
        {
            GameManager.Instance.level = targetLevel;
        }

        SaveDataPlayer save = FindObjectOfType<SaveDataPlayer>();
        if (save == null) save = SaveDataPlayer.Instance;
        if (save != null)
        {
            save.Save(1, targetLevel);
            save.ClearBoardState();
            save.SaveAllDataAndWriteToDisk();
        }

        // Cập nhật UI ngay tại chỗ, KHÔNG reload scene
        UpdateLevelUI();
    }

    public void EnterGame()
    {
        RequestScreen(ScreenType.Gameplay);
    }

    private void RequestScreen(ScreenType type, bool force = false)
    {
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TransitionToScreen(type, force);
            return;
        }

        if (ScreenManager.Instance != null)
        {
            ScreenManager.Instance.ShowScreen(type, force);
        }
    }

    public void UpdateLevelUI()
    {
        try
        {
            SyncLevelFromSave();
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm == null) gm = GameManager.Instance;
            if (gm == null) return;

            if (levelReset != null && !levelReset.isFocused)
            {
                levelReset.SetTextWithoutNotify(gm.level.ToString());
            }
            if (textLevel != null) textLevel.text = "Level " + gm.level;
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
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm == null) gm = GameManager.Instance;
        if (gm == null) return;

        int currentLevel = gm.level;
        int maxUnlockedLevel = gm.currentMaxLevel;
        int totalLevelsInGame = gm.levelDataSOs != null ? gm.levelDataSOs.Count : 999;

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

    private void SyncLevelFromSave()
    {
        if (SaveDataPlayer.Instance == null) return;

        int savedLevel = Mathf.RoundToInt(SaveDataPlayer.Instance.Value(1));
        if (savedLevel > 0)
        {
            // Tìm GameManager thực tế để tránh bug static reference trỏ sai object
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                gm.level = savedLevel;
            }
            // Đồng bộ cả static Instance
            if (GameManager.Instance != null && GameManager.Instance != gm)
            {
                GameManager.Instance.level = savedLevel;
            }
        }
    }

    public void NextLevel()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm == null) gm = GameManager.Instance;
        if (gm == null) return;

        if (gm.level < gm.currentMaxLevel)
        {
            gm.level++;
            // Đồng bộ static Instance
            if (GameManager.Instance != null && GameManager.Instance != gm)
                GameManager.Instance.level = gm.level;

            if (SaveDataPlayer.Instance != null)
            {
                SaveDataPlayer.Instance.Save(1, gm.level);
            }
            UpdateLevelUI(); 
        }
    }

    public void PreviousLevel()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm == null) gm = GameManager.Instance;
        if (gm == null) return;

        if (gm.level > 1)
        {
            gm.level--;
            // Đồng bộ static Instance
            if (GameManager.Instance != null && GameManager.Instance != gm)
                GameManager.Instance.level = gm.level;

            if (SaveDataPlayer.Instance != null)
            {
                SaveDataPlayer.Instance.Save(1, gm.level);
            }
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