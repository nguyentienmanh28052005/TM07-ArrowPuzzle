using System.Net.Mime;
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

    [Header("Juice Settings")]
    [SerializeField] private float popupAnimDuration = 0.3f;
    [SerializeField] private float overlayAlpha = 0.75f;

    private bool _isShowing = false;
    private List<GameObject> hearts;
    private int countHeart;
    private Vector3 _pauseOriginalScale;
    private Vector3 _completeOriginalScale;
    private bool _isTransitioning = false;
    #endregion

    #region [ INITIALIZATION & LIFECYCLE ]
    private void Awake()
    {
        if (pauseContent != null) _pauseOriginalScale = pauseContent.localScale;
        if (completeContent != null) _completeOriginalScale = completeContent.localScale;
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
    /// <summary>
    /// Kích hoạt bảng Tạm Dừng và đóng băng trò chơi.
    /// </summary>
    public void ShowPause()
    {
        if (_currentPopup != PopupState.None || _isTransitioning) return;
        
        _currentPopup = PopupState.Pause;
        Time.timeScale = 0f; 
        
        ShowOverlay(true);
        OpenPopupTween(pausePanel, pauseContent, _pauseOriginalScale);
    }

    /// <summary>
    /// Đóng bảng Tạm Dừng và tiếp tục trò chơi.
    /// </summary>
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

    /// <summary>
    /// Kích hoạt bảng Hoàn Thành Màn Chơi kèm hiệu ứng hạt.
    /// </summary>
    public void ShowCompletePopup(object data)
    {
        if (_currentPopup != PopupState.None || _isTransitioning) return;
        
        _currentPopup = PopupState.Complete;
        ShowOverlay(true);
        OpenPopupTween(completePanel, completeContent, _completeOriginalScale);

        if (completeParticle != null)
        {
            completeParticle.gameObject.SetActive(true);
            completeParticle.Play(true);
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
    /// <summary>
    /// Đánh giá và hiển thị thông báo chiến thắng dựa trên số lượng tim còn lại.
    /// </summary>
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

    /// <summary>
    /// Xử lý logic trừ tim, chạy hoạt ảnh vỡ tim và kích hoạt GameOver nếu hết máu.
    /// </summary>
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