using DG.Tweening;
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

    [Header("Win Stars (Ruột Sao)")]
    [Tooltip("Kéo 3 object Hình ảnh Ruột Sao vào đây theo thứ tự trái sang phải")]
    [SerializeField] private RectTransform[] starFills;

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
    [SerializeField] private RectTransform currentCoinIcon;
    [SerializeField] private RectTransform currentDiamondIcon;

    private bool _isShowing = false;
    private List<GameObject> hearts;
    private int countHeart;
    private Vector3 _pauseOriginalScale;
    private Vector3 _completeOriginalScale;
    private bool _isTransitioning = false;
    
    // BIẾN MỚI: Cần lưu lại tọa độ gốc của các ngôi sao để đập xuống cho trúng đích
    private Vector2[] _starOriginalPositions;
    #endregion

    #region [ INITIALIZATION & LIFECYCLE ]
    private void Awake()
    {
        if (pauseContent != null) _pauseOriginalScale = pauseContent.localScale;
        if (completeContent != null) _completeOriginalScale = completeContent.localScale;

        // Lưu trữ tọa độ chuẩn của Ruột Sao ngay lúc đầu
        if (starFills != null)
        {
            _starOriginalPositions = new Vector2[starFills.Length];
            for (int i = 0; i < starFills.Length; i++)
            {
                if (starFills[i] != null)
                    _starOriginalPositions[i] = starFills[i].anchoredPosition;
            }
        }
    }

    void Start()
    {
        InitializeHearts();
        InitializePopups();
    }

    private void InitializeHearts()
    {
        if (healthContainer != null)
        {
            countHeart = healthContainer.childCount;
            hearts = new List<GameObject>(countHeart);
            foreach (Transform child in healthContainer)
            {
                hearts.Add(child.gameObject);
            }
        }
    }

    private void InitializePopups()
    {
        if (feedbackText != null) { feedbackText.alpha = 0f; feedbackText.gameObject.SetActive(false); }
        HidePanelImmediate(overlayBg);
        HidePanelImmediate(pausePanel);
        HidePanelImmediate(completePanel);

        // Đảm bảo 3 ngôi sao luôn tàng hình lúc mới vào game
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
    }

    private void OnDisable()
    {
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnTakeDamage, DecreaseHeart);
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnComplete, ShowCompletePopup);
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

        // 1. GIẢI NÉN GÓI HÀNG (Bao gồm Data và trạng thái Full Combo)
        if (data is object[] rewardData && rewardData.Length >= 2)
        {
            LevelDataSO levelData = rewardData[0] as LevelDataSO;
            bool isFullCombo = (bool)rewardData[1];

            if (levelData != null)
            {
                earnedCoins = (int)levelData.rewardCoins;
                
                // NẾU KHÔNG FULL COMBO -> THƯỞNG 0 KIM CƯƠNG
                earnedDiamonds = isFullCombo ? (int)levelData.rewardDiamonds : 0;

                if (coinText != null) coinText.text = $"x{earnedCoins}";
                if (diamondText != null) diamondText.text = $"x{earnedDiamonds}";
            }
        }
        else if (data is LevelDataSO levelDataFallback) // Mã dự phòng an toàn (Fallback)
        {
            earnedCoins = (int)levelDataFallback.rewardCoins;
            earnedDiamonds = (int)levelDataFallback.rewardDiamonds;

            if (coinText != null) coinText.text = $"x{earnedCoins}";
            if (diamondText != null) diamondText.text = $"x{earnedDiamonds}";
        }

        // 2. Tính toán tiền gốc (Dựa trên ví thật trừ đi số tiền VỪA NHẬN)
        float oldCoins = CurrencyManager.Instance.Coins - earnedCoins;
        float oldDiamonds = CurrencyManager.Instance.Diamonds - earnedDiamonds;

        if (currentCoinText != null) currentCoinText.text = Mathf.RoundToInt(oldCoins).ToString();
        if (currentDiamondText != null) currentDiamondText.text = Mathf.RoundToInt(oldDiamonds).ToString();

        _currentPopup = PopupState.Complete;
        ShowOverlay(true);
        OpenPopupTween(completePanel, completeContent, _completeOriginalScale);

        if (completeParticle != null)
        {
            completeParticle.gameObject.SetActive(true);
            completeParticle.Play(true);
        }

        // 3. Khởi chạy chuỗi kịch bản đạo diễn
        PlayWinSequenceEffect(earnedCoins, earnedDiamonds, oldCoins, oldDiamonds);
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
        // 1. SETUP TƯ THẾ BAN ĐẦU: To gấp 5 lần và mờ tịt
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

        // Chờ 0.4s cho Popup bung ra hoàn toàn
        seq.AppendInterval(0.4f);

        // 2. KỊCH BẢN ĐẠO DIỄN: SCREEN SPACE SLAM (Impact Cứng Cáp)
        for (int i = 0; i < countHeart; i++)
        {
            if (i >= starFills.Length) break; 
            RectTransform star = starFills[i];
            if (star == null) continue;

            Image img = star.GetComponent<Image>();

            // A. Lao tới: Thu nhỏ về 1 và hiện rõ
            seq.Append(star.DOScale(Vector3.one, 0.2f).SetEase(Ease.InExpo));
            if (img != null)
            {
                seq.Join(img.DOFade(1f, 0.2f).SetEase(Ease.InExpo));
            }

            // B. Va chạm vật lý: KHÔNG BÓP MÉO NỮA
            seq.AppendCallback(() => {
                // ĐÃ SỬA: Thay vì nảy méo mó (Squash), ta cho nảy đều 4 góc (Uniform) cực nhẹ để tạo phản lực
                star.DOKill();
                star.localScale = Vector3.one;
                star.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 1).SetUpdate(true);
                
                // Giữ nguyên độ rung của Bảng Gỗ để cảm nhận được "Sức nặng" của ngôi sao
                completeContent.DOKill(false);
                completeContent.DOPunchPosition(new Vector3(0, -15f, 0), 0.2f, 10, 1).SetUpdate(true);
            });

            // C. Nhịp nghỉ ngắn trước khi thả ngôi sao tiếp theo
            if (i < countHeart - 1)
                seq.AppendInterval(0.15f);
        }

        // 3. Khi Sao đập xong hết -> Bắn tiền văng tung tóe
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

    private void ShowText(string content, Color textColor)
    {
        if (feedbackText != null)
        {
            feedbackText.text = content;
            feedbackText.color = textColor;
            feedbackText.gameObject.SetActive(true);
            feedbackText.transform.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            seq.Append(feedbackText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
            seq.Join(feedbackText.DOFade(1f, 0.5f));
            seq.AppendInterval(1f); 
            seq.Append(feedbackText.DOFade(0f, 0.3f));
            seq.Join(feedbackText.transform.DOScale(1.2f, 0.3f).SetEase(Ease.InQuad));
        }
    }

    public void DecreaseHeart(object data)
    {
        if (countHeart <= 0 || _currentPopup != PopupState.None) return;

        countHeart--;
        GameObject heartObj = hearts[countHeart];
        PlayHeartLossEffect(heartObj);

        if (countHeart <= 0)
        {
            _currentPopup = PopupState.GameOver; 
            if (gameContainer != null) gameContainer.SetActive(false);             
            StartCoroutine(SequenceGameOver());
        }
    }

    private IEnumerator SequenceGameOver()
    {
        yield return new WaitForSeconds(0.5f);
        ShowText("Game Over", Color.red);
        yield return new WaitForSeconds(2f); 
        OutLevel(); 
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
}