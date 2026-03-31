using DG.Tweening;
using Solo.MOST_IN_ONE;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvas : MonoBehaviour
{
    public enum PopupState { None, Pause, Complete, GameOver }
    private PopupState _currentPopup = PopupState.None;

    #region [ REFERENCES & SETTINGS ]
    [Header("Core UI")]
    [SerializeField] private GameObject gameContainer;
    [SerializeField] private Transform healthContainer;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private CanvasGroup overlayBg;

    [Header("Pause Pop-up")]
    [SerializeField] private CanvasGroup pausePanel;
    [SerializeField] private Transform pauseContent;

    [Header("Complete Pop-up")]
    [SerializeField] private CanvasGroup completePanel;
    [SerializeField] private Transform completeContent;
    [SerializeField] private ParticleSystem completeParticle;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI diamondText;
    [SerializeField] private RectTransform rewardCoinIcon;
    [SerializeField] private RectTransform rewardDiamondIcon;
    [SerializeField] private RectTransform[] starFills;

    [Header("Game Over Pop-up")]
    [SerializeField] private CanvasGroup gameOverPanel;
    [SerializeField] private Transform gameOverContent;

    [Header("Currency Burst Effect")]
    [SerializeField] private GameObject flyingCoinPrefab;     
    [SerializeField] private GameObject flyingDiamondPrefab;  
    [SerializeField] private int maxFlyingItems = 12;         

    [Header("Juice Settings")]
    [SerializeField] private float popupAnimDuration = 0.3f;
    [SerializeField] private float overlayAlpha = 0.75f;
    
    [Header("Currency Top Bar")]
    [SerializeField] TextMeshProUGUI currentCoinText;
    [SerializeField] TextMeshProUGUI currentDiamondText;
    [SerializeField] private TextMeshProUGUI currentCoinTextLose;
    [SerializeField] private RectTransform currentCoinIcon;
    [SerializeField] private RectTransform currentDiamondIcon;

    [Header("Tools")]
    [SerializeField] private TextMeshProUGUI currentEraseToolText;
    [SerializeField] private TextMeshProUGUI currentHintToolText;
    [SerializeField] private TextMeshProUGUI currentDashToolText;
    [SerializeField] private GameObject dashToolPanel;

    private bool _isShowing = false;
    private List<GameObject> hearts;
    private int countHeart;
    private Vector3 _pauseOriginalScale;
    private Vector3 _completeOriginalScale;
    private Vector3 _gameOverOriginalScale; // Thêm biến lưu scale gốc của Game Over
    private bool _isTransitioning = false;
    
    private Vector2[] _starOriginalPositions;
    #endregion

    #region [ INITIALIZATION & LIFECYCLE ]
    private void Awake()
    {
        if (pauseContent != null) _pauseOriginalScale = pauseContent.localScale;
        if (completeContent != null) _completeOriginalScale = completeContent.localScale;
        if (gameOverContent != null) _gameOverOriginalScale = gameOverContent.localScale; 

        if (starFills != null)
        {
            _starOriginalPositions = new Vector2[starFills.Length];
            for (int i = 0; i < starFills.Length; i++)
            {
                if (starFills[i] != null)
                    _starOriginalPositions[i] = starFills[i].anchoredPosition;
            }
        }

        InitializeHearts();
        InitializePopups();
        InitializeTools();
    }

    private void InitializeTools()
    {
        UpdateToolCountText(null);
    }

    private void InitializeHearts()
    {
        if (healthContainer != null)
        {
            // 1. Ép Unity tính toán và xếp thẳng hàng các trái tim NGAY LẬP TỨC
            RectTransform containerRect = healthContainer.GetComponent<RectTransform>();
            if (containerRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
            }

            // 2. QUA CẦU RÚT VÁN: Tắt Component LayoutGroup đi để nó không bao giờ phá DOTween nữa!
            HorizontalLayoutGroup layout = healthContainer.GetComponent<HorizontalLayoutGroup>();
            if (layout != null) layout.enabled = false;

            countHeart = healthContainer.childCount;
            hearts = new List<GameObject>(countHeart);
            foreach (Transform child in healthContainer)
            {
                hearts.Add(child.gameObject);
            }
        }
    }

    public void SetupModeUI(GameMode mode)
    {
        if (healthContainer != null)
        {
            healthContainer.gameObject.SetActive(mode == GameMode.Classic || mode == GameMode.Memory);
        }
    }

    private void UpdateToolCountText(object data)
    {
        if (currentEraseToolText != null)
        {
            if(CurrencyManager.Instance.EraseToolCount > 99)
            {
                currentEraseToolText.text = "99+";
            }
            else
            {
                currentEraseToolText.text = CurrencyManager.Instance.EraseToolCount.ToString();
            }
        }

        if (currentHintToolText != null)
        {
            if(CurrencyManager.Instance.HintToolCount > 99)
            {
                currentHintToolText.text = "99+";
            }
            else
            {
                currentHintToolText.text = CurrencyManager.Instance.HintToolCount.ToString();
            }
        }

        if (currentDashToolText != null)
        {
            if(CurrencyManager.Instance.DashToolCount > 99)
            {
                currentDashToolText.text = "99+";
            }
            else
            {
                currentDashToolText.text = CurrencyManager.Instance.DashToolCount.ToString();
            }
        }
    }

    private void InitializePopups()
    {
        if (feedbackText != null) { feedbackText.alpha = 0f; feedbackText.gameObject.SetActive(false); }
        HidePanelImmediate(overlayBg);
        HidePanelImmediate(pausePanel);
        HidePanelImmediate(completePanel);
        HidePanelImmediate(gameOverPanel); // Đảm bảo tàng hình lúc mới vào game

        if (starFills != null)
        {
            foreach (var star in starFills)
            {
                if (star != null) star.localScale = Vector3.zero;
            }
        }

        _currentPopup = PopupState.None;
    }

    private void OnEnable()
    {
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnTakeDamage, DecreaseHeart);
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnComplete, ShowCompletePopup);
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnHintToolChanged, UpdateToolCountText);
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnEraseToolChanged, UpdateToolCountText);
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnDashToolChanged, UpdateToolCountText);
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnSelectDashDirection, SetDashToolPanelActive);
    }

    private void OnDisable()
    {
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnTakeDamage, DecreaseHeart);
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnComplete, ShowCompletePopup);
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnHintToolChanged, UpdateToolCountText); 
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnEraseToolChanged, UpdateToolCountText);
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnDashToolChanged, UpdateToolCountText);
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnSelectDashDirection, SetDashToolPanelActive);
    }

    public void SetDashToolPanelActive(object data)
    {
        if (data is bool isActive)
        {
            if (dashToolPanel != null) dashToolPanel.SetActive(isActive);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !_isTransitioning)
        {
            if (_currentPopup == PopupState.Pause) ClosePause();
            else if (_currentPopup == PopupState.None) ShowPause();
        }
    }
    #endregion

    #region [ POP-UP MANAGEMENT ]
    public void ShowPause()
    {
        if (_currentPopup != PopupState.None || _isTransitioning) return;
        
        _currentPopup = PopupState.Pause;
        Time.timeScale = 0f; 
        
        ShowOverlay(true);
        OpenPopupTween(pausePanel, pauseContent, _pauseOriginalScale);
    }

    public void ClosePause()
    {
        if (_currentPopup != PopupState.Pause || _isTransitioning) return;
        
        ClosePopupTween(pausePanel, pauseContent, () => 
        {
            _currentPopup = PopupState.None;
            ShowOverlay(false);
            Time.timeScale = 1f; 
        });
    }

    public void ShowCompletePopup(object data)
    {
        if (_currentPopup != PopupState.None || _isTransitioning) return;

        int earnedCoins = 0;
        int earnedDiamonds = 0;

        if (data is object[] rewardData && rewardData.Length >= 2)
        {
            LevelDataSO levelData = rewardData[0] as LevelDataSO;
            bool isFullCombo = (bool)rewardData[1];

            if (levelData != null)
            {
                earnedCoins = (int)levelData.rewardCoins;
                earnedDiamonds = isFullCombo ? (int)levelData.rewardDiamonds : 0;

                if (coinText != null) coinText.text = $"x{earnedCoins}";
                if (diamondText != null) diamondText.text = $"x{earnedDiamonds}";
            }
        }
        else if (data is LevelDataSO levelDataFallback)
        {
            earnedCoins = (int)levelDataFallback.rewardCoins;
            earnedDiamonds = (int)levelDataFallback.rewardDiamonds;

            if (coinText != null) coinText.text = $"x{earnedCoins}";
            if (diamondText != null) diamondText.text = $"x{earnedDiamonds}";
        }

        float oldCoins = CurrencyManager.Instance.Coins - earnedCoins;
        float oldDiamonds = CurrencyManager.Instance.Diamonds - earnedDiamonds;

        if (currentCoinText != null) currentCoinText.text = Mathf.RoundToInt(oldCoins).ToString();
        if (currentDiamondText != null) currentDiamondText.text = Mathf.RoundToInt(oldDiamonds).ToString();

        _currentPopup = PopupState.Complete;
        AudioManager.Instance.PlaySfx(AudioManager.Instance.winSound);
        ShowOverlay(true);
        OpenPopupTween(completePanel, completeContent, _completeOriginalScale);

        if (completeParticle != null)
        {
            completeParticle.gameObject.SetActive(true);
            completeParticle.Play(true);
        }

        PlayWinSequenceEffect(earnedCoins, earnedDiamonds, oldCoins, oldDiamonds);
    }

    // ĐÃ HOÀN THIỆN: Logic Bung Popup Game Over chuẩn form
    public void ShowLosePopup(object data)
    {
        if (_currentPopup != PopupState.None || _isTransitioning) return;

        _currentPopup = PopupState.GameOver;

        if (currentCoinTextLose != null) currentCoinTextLose.text = CurrencyManager.Instance.Coins.ToString();
    
        // Rung nhẹ màn hình một phát để tăng độ cay cú khi thua (Game Feel)
        if (SettingManager.Instance != null) 
        {
            SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.HeavyImpact);
        }
        AudioManager.Instance.PlaySfx(AudioManager.Instance.loseSound);
        ShowOverlay(true);
        OpenPopupTween(gameOverPanel, gameOverContent, _gameOverOriginalScale);
    }
    #endregion

    #region [ WIN SEQUENCE & CURRENCY BURST ]
    private Vector3 GetTrueWorldCenter(RectTransform rect)
    {
        if (rect == null) return Vector3.zero;
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return (corners[0] + corners[1] + corners[2] + corners[3]) / 4f;
    }

    private void PlayWinSequenceEffect(int earnedCoins, int earnedDiamonds, float oldCoins, float oldDiamonds)
    {
        foreach (var star in starFills)
        {
            if (star != null) 
            {
                star.localScale = Vector3.one * 5f; 
                
                Image img = star.GetComponent<Image>();
                if (img != null)
                {
                    Color c = img.color;
                    c.a = 0f; 
                    img.color = c;
                }
            }
        }

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); 
        seq.AppendInterval(0.4f);

        for (int i = 0; i < countHeart; i++)
        {
            if (i >= starFills.Length) break; 
            RectTransform star = starFills[i];
            if (star == null) continue;

            Image img = star.GetComponent<Image>();

            seq.Append(star.DOScale(Vector3.one, 0.2f).SetEase(Ease.InExpo));
            if (img != null)
            {
                seq.Join(img.DOFade(1f, 0.2f).SetEase(Ease.InExpo));
            }

            seq.AppendCallback(() => {
                star.DOKill();
                star.localScale = Vector3.one;
                star.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 1).SetUpdate(true);
                AudioManager.Instance.PlaySfx(AudioManager.Instance.starHit, 1f);

                completeContent.DOKill(false);
                completeContent.DOPunchPosition(new Vector3(0, -15f, 0), 0.2f, 10, 1).SetUpdate(true);
            });

            if (i < countHeart - 1)
                seq.AppendInterval(0.15f);
        }

        seq.OnComplete(() => {
            if (flyingCoinPrefab != null && rewardCoinIcon != null && currentCoinIcon != null && earnedCoins > 0)
            {
                StartCoroutine(SpawnFlyingItems(flyingCoinPrefab, rewardCoinIcon, currentCoinIcon, Mathf.Min(earnedCoins, maxFlyingItems), earnedCoins, oldCoins, currentCoinText));
            }

            if (flyingDiamondPrefab != null && rewardDiamondIcon != null && currentDiamondIcon != null && earnedDiamonds > 0)
            {
                StartCoroutine(SpawnFlyingItems(flyingDiamondPrefab, rewardDiamondIcon, currentDiamondIcon, Mathf.Min(earnedDiamonds, maxFlyingItems), earnedDiamonds, oldDiamonds, currentDiamondText));
            }
        });
    }

    private IEnumerator SpawnFlyingItems(GameObject prefab, RectTransform startIcon, RectTransform targetIcon, int spawnCount, int totalEarned, float oldValue, TextMeshProUGUI textUI)
    {
        Vector3 startPos = GetTrueWorldCenter(startIcon);
        Vector3 targetPos = GetTrueWorldCenter(targetIcon);
        targetPos.z = startPos.z;

        float scatterOffset = Vector3.Distance(startPos, targetPos) * 0.15f; 
        float valuePerHit = (float)totalEarned / spawnCount;
        int itemsHit = 0;

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject item = Instantiate(prefab, transform); 
            item.transform.SetAsLastSibling(); 

            RectTransform rect = item.GetComponent<RectTransform>();
            if (rect != null) rect.pivot = new Vector2(0.5f, 0.5f);

            item.transform.position = startPos;
            item.transform.localScale = Vector3.zero;

            Vector3 jumpPos = startPos + new Vector3(Random.Range(-scatterOffset, scatterOffset), Random.Range(-scatterOffset, scatterOffset), 0f);

            Sequence seq = DOTween.Sequence();
            seq.SetUpdate(true);

            seq.Append(item.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad));
            seq.Join(item.transform.DOMove(jumpPos, 0.3f).SetEase(Ease.OutQuad));
            seq.AppendInterval(Random.Range(0f, 0.15f));
            seq.Append(item.transform.DOMove(targetPos, 0.5f).SetEase(Ease.InOutSine));

            seq.OnComplete(() => {

                AudioManager.Instance.PlaySfx(AudioManager.Instance.coinHit, 0.5f);
                Destroy(item);
                
                itemsHit++;
                if (textUI != null)
                {
                    float displayValue = (itemsHit == spawnCount) ? (oldValue + totalEarned) : (oldValue + valuePerHit * itemsHit);
                    textUI.text = Mathf.RoundToInt(displayValue).ToString();
                }

                targetIcon.DOKill();
                targetIcon.localScale = Vector3.one;
                targetIcon.DOPunchScale(Vector3.one * 0.2f, 0.15f, 10, 1).SetUpdate(true);
            });

            yield return new WaitForSecondsRealtime(0.08f); 
        }
    }
    #endregion

    #region [ ANIMATION ENGINE ]
    private void OpenPopupTween(CanvasGroup panel, Transform content, Vector3 targetScale)
    {
        panel.gameObject.SetActive(true);
        panel.blocksRaycasts = true; 
        panel.alpha = 0f;
        
        content.DOKill();
        content.localScale = targetScale * 0.6f;

        Sequence seq = DOTween.Sequence();
        seq.Join(panel.DOFade(1f, popupAnimDuration * 0.5f));
        seq.Join(content.DOScale(targetScale * 1.05f, popupAnimDuration * 0.7f).SetEase(Ease.OutQuad));
        seq.Append(content.DOScale(targetScale, popupAnimDuration * 0.3f).SetEase(Ease.InOutSine));
        
        seq.SetUpdate(true);
        seq.SetLink(content.gameObject);
    }

    private void ClosePopupTween(CanvasGroup panel, Transform content, System.Action onComplete)
    {
        panel.blocksRaycasts = false; 

        content.DOKill();
        Sequence seq = DOTween.Sequence();
        seq.Join(content.DOScale(content.localScale * 1.02f, popupAnimDuration * 0.3f).SetEase(Ease.OutQuad));
        seq.Append(content.DOScale(Vector3.zero, popupAnimDuration * 0.7f).SetEase(Ease.InBack));
        seq.Join(panel.DOFade(0f, popupAnimDuration * 0.5f).SetDelay(popupAnimDuration * 0.5f));
        
        seq.SetUpdate(true);
        seq.SetLink(content.gameObject);
        seq.OnComplete(() => 
        {
            panel.gameObject.SetActive(false);
            onComplete?.Invoke();   
        });
    }
    #endregion

    #region [ SCENE TRANSITIONS ]
    public void RestartGame()
    {
        if (_isTransitioning) return;
        StartCoroutine(TransitionToScene("GameScene"));
    }

    public void NextLevel()
    {
        if (_isTransitioning) return;
        StartCoroutine(TransitionToScene("GameScene"));
    }

    public void Replay()
    {
        if (_isTransitioning) return;
        GameManager.Instance.level--;
        SaveDataPlayer.Instance.Save(1, GameManager.Instance.level);
        StartCoroutine(TransitionToScene("GameScene"));
    }

    public void OutLevel()
    {
        if (_isTransitioning) return;
        StartCoroutine(TransitionToScene("GameMenu"));
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        _isTransitioning = true;
        Time.timeScale = 1f; 

        if (overlayBg != null)
        {
            overlayBg.gameObject.SetActive(true);
            overlayBg.blocksRaycasts = true;
            yield return overlayBg.DOFade(1f, 0.3f).SetUpdate(true).WaitForCompletion();
        }

        SceneController.Instance.LoadScene(sceneName, false, false);
    }
    #endregion

    #region [ UTILITY LOGIC ]
    private void ShowOverlay(bool isShow)
    {
        if (overlayBg == null) return;
        
        overlayBg.DOKill();
        if (isShow)
        {
            overlayBg.gameObject.SetActive(true);
            overlayBg.blocksRaycasts = true;
            overlayBg.DOFade(overlayAlpha, popupAnimDuration).SetUpdate(true);
        }
        else
        {
            overlayBg.blocksRaycasts = false;
            overlayBg.DOFade(0f, popupAnimDuration).SetUpdate(true).OnComplete(() => overlayBg.gameObject.SetActive(false));
        }
    }

    private void HidePanelImmediate(CanvasGroup panel)
    {
        if (panel == null) return;
        panel.alpha = 0f;
        panel.blocksRaycasts = false;
        panel.gameObject.SetActive(false);
    }
    #endregion

    #region [ HEART & FEEDBACK TEXT ]
    public void ShowWinText()
    {
        string message = countHeart >= 3 ? "Perfect!" : (countHeart == 2 ? "Great!" : "Good!");
        ShowText(message, Color.yellow);
    }

    // Thêm tham số System.Action onComplete = null
    public void ShowText(string content, Color textColor, System.Action onComplete = null)
    {
        if (feedbackText != null)
        {
            feedbackText.DOKill();
            feedbackText.transform.DOKill();

            feedbackText.text = content;
            feedbackText.color = textColor;
            feedbackText.alpha = 0f; 

            feedbackText.gameObject.SetActive(true);
            feedbackText.transform.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            seq.SetUpdate(true); 

            seq.Append(feedbackText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
            seq.Join(feedbackText.DOFade(1f, 0.5f));
            seq.AppendInterval(1.5f); 
            seq.Append(feedbackText.DOFade(0f, 0.3f));
            seq.Join(feedbackText.transform.DOScale(1.2f, 0.3f).SetEase(Ease.InQuad));
            
            seq.OnComplete(() => {
                feedbackText.gameObject.SetActive(false);
                onComplete?.Invoke(); 
            });
        }
        else
        {
            onComplete?.Invoke();
        }
    }


    public void DecreaseHeart(object data)
    {
        if (GameManager.Instance.GetCurrentLevelData().gameMode == GameMode.TimeAttack) return;

        if (countHeart <= 0 || _currentPopup != PopupState.None) return;

        countHeart--;
        GameObject heartObj = hearts[countHeart];
        PlayHeartLossEffect(heartObj);

        if (countHeart <= 0)
        {
            if (data is SnakeBlock sb) sb.ForceResetToOrigin();
            CameraController.IsGameplayBlocking = true;
            StartCoroutine(SequenceGameOver());
        }
    }

    private IEnumerator SequenceGameOver()
    {
        yield return new WaitForSeconds(0.1f);
    
        // ShowOverlay(true); 

        // ShowText("Game Over", Color.yellow);
        
        // yield return new WaitForSeconds(2f);
        ShowLosePopup(null); 
    }

    private void PlayHeartLossEffect(GameObject heart)
    {
        RectTransform rect = heart.GetComponent<RectTransform>();
        Image img = heart.GetComponent<Image>();
        Vector2 originalPos = rect.anchoredPosition;

        rect.DOKill();
        if (img != null) img.DOKill();

        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOShakeAnchorPos(0.4f, 15f, 20, 90, false));
        if (img != null) seq.Join(img.DOColor(Color.gray, 0.2f));
        
        seq.Append(rect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
        if (img != null) seq.Join(img.DOFade(0f, 0.3f));

        seq.OnComplete(() =>
        {
            heart.SetActive(false);
            rect.localScale = Vector3.one;
            rect.anchoredPosition = originalPos;
            if (img != null) img.color = Color.white;
        });
    }

    public void ShowAllPaths()
    {
        _isShowing = !_isShowing;
        MessageManager.Instance.SendMessage(ManhMessageType.OnShowAllPaths, _isShowing);
    }
    #endregion

    #region [ REVIVE SYSTEM ]
    /// <summary>
    /// Gắn hàm này vào sự kiện OnClick của nút "GET FREE" (Xem quảng cáo)
    /// </summary>
    public void Btn_ReviveWithAd()
    {
        if (_isTransitioning) return;

        // Tương lai: Bạn sẽ gọi logic của AdMob / AppLovin ở đây. 
        // Khi SDK trả về sự kiện OnAdRewarded (xem xong), bạn mới gọi OnReviveSuccess(1).
        
        // Tạm thời giả lập xem Ads thành công ngay lập tức:
        OnReviveSuccess(1);
    }

    /// <summary>
    /// Gắn hàm này vào sự kiện OnClick của nút "GET 900" (Trả phí bằng Vàng)
    /// </summary>
    public void Btn_ReviveWithCoins()
    {
        if (_isTransitioning) return;

        int cost = 1000;

        // Kiểm tra ví tiền
        if (CurrencyManager.Instance.Coins >= cost)
        {
            // Trừ tiền (Nếu hệ thống của bạn dùng hàm khác như SubtractCoin thì hãy đổi lại nhé)
            CurrencyManager.Instance.SpendCoins(cost); 

            // Cập nhật lại UI tiền ngay lập tức
            if (currentCoinText != null) currentCoinText.text = Mathf.RoundToInt(CurrencyManager.Instance.Coins).ToString();

            // Mua thành công -> Hồi 3 tim
            OnReviveSuccess(3);
        }
        else
        {
            // KHÔNG ĐỦ TIỀN: Cảnh báo người chơi
            if (SettingManager.Instance != null) 
            {
                SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.Failure);
            }
            
            // Lắc nhẹ chữ Text để báo lỗi (Feedback trực quan)
            if (currentCoinText != null)
            {
                currentCoinText.transform.DOKill(true);
                currentCoinText.transform.DOShakePosition(0.3f, new Vector3(10f, 0, 0), 20, 90);
                currentCoinText.DOColor(Color.red, 0.15f).SetLoops(2, LoopType.Yoyo).OnComplete(() => currentCoinText.color = Color.white);
            }
        }
    }

    /// <summary>
    /// Xử lý logic đưa người chơi quay lại bàn cờ sau khi thỏa mãn điều kiện Hồi sinh
    /// </summary>
    private void OnReviveSuccess(int heartsToAdd)
    {
        // 1. Phục hồi biến đếm (Đảm bảo không vượt quá số tim gốc)
        countHeart = Mathf.Min(countHeart + heartsToAdd, hearts.Count);

        // 2. Bật lại UI trái tim với hiệu ứng nảy (Pop)
        for (int i = 0; i < countHeart; i++)
        {
            GameObject heart = hearts[i];
            heart.SetActive(true);
            
            RectTransform rect = heart.GetComponent<RectTransform>();
            Image img = heart.GetComponent<Image>();
            
            if (rect != null)
            {
                rect.DOKill();
                rect.localScale = Vector3.zero;
                rect.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetDelay(i * 0.5f);
            }
            if (img != null)
            {
                img.DOKill();
                img.color = Color.white; 
                img.DOFade(1f, 0.1f);
            }
        }

        // 3. Đóng màn hình Thua và mở lại bàn cờ
        ClosePopupTween(gameOverPanel, gameOverContent, () => 
        {
            _currentPopup = PopupState.None;
            ShowOverlay(false);
            
            // BẢN VÁ: Không cần bật gameContainer nữa
            // if (gameContainer != null) gameContainer.SetActive(true);
            
            // Trả lại quyền tương tác cho người chơi
            CameraController.IsGameplayBlocking = false;
        });
    }
    #endregion
}