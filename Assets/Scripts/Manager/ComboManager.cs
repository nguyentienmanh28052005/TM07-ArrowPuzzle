using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic; 
using Pixelplacement; 

public class ComboManager : Singleton<ComboManager>
{
    [System.Serializable]
    public struct ComboSettings
    {
        public int minComboThreshold; 
        public Color comboColor;     
        public float fontSizeMultiplier; 
    }

    [Header("Combo State")]
    public int currentCombo = 0;
    [SerializeField] private float comboTimeout = 2f; 
    [SerializeField] private int minComboToShow = 3; 
    private float _lastHitTime;
    
    // CỜ BỌC THÉP: Theo dõi trạng thái đã đạt Full Combo chưa
    private bool _isFullComboActive = false; 

    [Header("Visual References")]
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private Transform cameraContainer; 

    [Header("Visual Juice Settings")]
    [SerializeField] private float maxRotationTilt = 0f; 
    [SerializeField] private List<ComboSettings> colorTierSettings; 

    [Header("Kinetic Juice Settings")]
    [SerializeField] private float baseShakeDuration = 0.15f;
    [SerializeField] private float maxHitStopDuration = 0.1f;

    [Header("Rainbow Settings (Only for Full Combo)")]
    [SerializeField] private float rainbowSpeed = 2f;

    /// <summary>
    /// Đưa Text UI về trạng thái ẩn ban đầu.
    /// </summary>
    private void Start()
    {
        if (comboText != null)
        {
            comboText.alpha = 0f;
            comboText.transform.localScale = Vector3.zero;
        }
    }

    /// <summary>
    /// Quản lý hiệu ứng chuyển màu cầu vồng liên tục nếu đạt Full Combo.
    /// </summary>
    void Update()
    {
        // CHỈ chạy cầu vồng khi cờ Full Combo được bật và Text đang hiển thị
        if (_isFullComboActive && comboText.alpha > 0)
        {
            float hue = Mathf.Repeat(Time.time * rainbowSpeed, 1f);
            Color rainbowColor = Color.HSVToRGB(hue, 0.8f, 1f); 
            
            comboText.color = rainbowColor;
        }
    }

    /// <summary>
    /// Tăng bộ đếm Combo và kiểm tra thời gian hết hạn (Timeout).
    /// </summary>
    public void AddCombo()
    {
        if (currentCombo > 0 && Time.time - _lastHitTime > comboTimeout)
        {
            StopCombo(); 
        }

        currentCombo++;
        _lastHitTime = Time.time;

        if (currentCombo >= minComboToShow)
        {
            PlayJuicyFeedback();
        }
    }

    /// <summary>
    /// Reset bộ đếm và tắt hiển thị UI Combo.
    /// </summary>
    public void StopCombo()
    {
        if (currentCombo == 0) return;

        currentCombo = 0;
        _isFullComboActive = false; // TẮT CỜ CẦU VỒNG

        if (comboText != null)
        {
            comboText.DOKill(); 
            comboText.transform.DORotate(Vector3.zero, 0.2f);
            comboText.DOFade(0f, 0.2f); 
            comboText.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack); 
        }
    }

    /// <summary>
    /// Tính toán tổng số mũi tên có trong Level hiện tại để làm mốc Full Combo.
    /// </summary>
    private int GetMaxComboForCurrentLevel()
    {
        if (GameManager.Instance != null && GameManager.Instance.GetCurrentLevelData() != null)
        {
            return GameManager.Instance.GetCurrentLevelData().snakes.Count;
        }
        return 999; 
    }

    /// <summary>
    /// Tổng hợp và thực thi tất cả các hiệu ứng "Juicy" để thưởng thị giác cho người chơi.
    /// </summary>
    private void PlayJuicyFeedback()
    {
        if (comboText == null) return;

        // 1. Kiểm tra trạng thái
        int maxCombo = GetMaxComboForCurrentLevel();
        _isFullComboActive = (currentCombo >= maxCombo); // Bật/Tắt cờ Full Combo

        float sizeMult = 1f;
        Color targetColor = Color.white;

        if (_isFullComboActive)
        {
            comboText.text = "FULL COMBO!";
            sizeMult += 0.5f; 
            // KHÔNG CẦN SET COLOR Ở ĐÂY VÌ HÀM UPDATE SẼ CHIẾM QUYỀN VÀ PHỦ CẦU VỒNG LÊN
        }
        else
        {
            comboText.text = $"Combo x{currentCombo}!";
            
            // 2. Chỉ tính màu Tier bình thường nếu CHƯA Full Combo
            if (colorTierSettings != null && colorTierSettings.Count > 0)
            {
                foreach (var setting in colorTierSettings)
                {
                    if (currentCombo >= setting.minComboThreshold)
                    {
                        targetColor = setting.comboColor;
                        sizeMult = setting.fontSizeMultiplier;
                    }
                    else
                    {
                        break; 
                    }
                }
            }
            comboText.color = targetColor;
        }

        // 3. Thực thi hiệu ứng Animation
        float randomTilt = Random.Range(-maxRotationTilt, maxRotationTilt);
        float tiltFactor = Mathf.Clamp(1f + (currentCombo * 0.05f), 1f, 2f);
        
        comboText.DOKill(); 
        comboText.transform.localRotation = Quaternion.Euler(0f, 0f, randomTilt * tiltFactor);

        comboText.alpha = 1f;
        comboText.transform.localScale = Vector3.one * sizeMult;
        
        float punchForce = Mathf.Clamp(0.2f + (currentCombo * 0.1f), 0.2f, 0.8f);
        
        if (_isFullComboActive)
        {
            comboText.transform.DOPunchScale(Vector3.one * punchForce * 1.5f, 0.5f, 8, 1);
            comboText.DOFade(0f, 0.5f).SetDelay(2f);
        }
        else
        {
            comboText.transform.DOPunchScale(Vector3.one * punchForce, 0.3f, 5, 1);
            comboText.DOFade(0f, 0.5f).SetDelay(1f);
        }
    }

    /// <summary>
    /// Kích hoạt trạng thái ngừng đọng thời gian (Slow motion) tạo lực nhấn.
    /// </summary>
    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.05f; 
        yield return new WaitForSecondsRealtime(duration); 
        Time.timeScale = 1f; 
    }
}