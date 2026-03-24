using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic; // BẮT BUỘC để dùng List
using Pixelplacement; // Dùng chung Singleton của bạn

public class ComboManager : Singleton<ComboManager>
{
    // Cấu trúc dữ liệu để định nghĩa màu sắc cho từng cấp độ Combo trên Inspector
    [System.Serializable]
    public struct ComboSettings
    {
        public int minComboThreshold; // Cấp độ tối thiểu (Ví dụ: 3, 6, 10)
        public Color comboColor;     // Màu sắc của chữ ở cấp độ này
        public float fontSizeMultiplier; // Độ phóng to thêm của chữ (Ví dụ: 1.1f, 1.2f)
    }

    [Header("Combo State")]
    public int currentCombo = 0;
    [SerializeField] private float comboTimeout = 2f; 
    [SerializeField] private int minComboToShow = 3; 
    private float _lastHitTime;

    [Header("Visual References")]
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private Transform cameraContainer; 

    [Header("Visual Juice Settings")]
    [SerializeField] private float maxRotationTilt = 0f; // Góc nghiêng tối đa (Trái/Phải)
    [SerializeField] private List<ComboSettings> colorTierSettings; // KÉO THẢ VÀ PHỐI MÀU TRÊN INSPECTOR

    [Header("Kinetic Juice Settings")]
    [SerializeField] private float baseShakeDuration = 0.15f;
    [SerializeField] private float maxHitStopDuration = 0.1f;

    [Header("Rainbow Settings")]
    [SerializeField] private bool useRainbowAtMaxTier = true;
    [SerializeField] private float rainbowSpeed = 2f;

    private void Start()
    {
        if (comboText != null)
        {
            comboText.alpha = 0f;
            comboText.transform.localScale = Vector3.zero;
        }
    }

    void Update()
    {
        // Nếu đang có combo cao (ví dụ tier cuối cùng) thì chạy hiệu ứng cầu vồng
        if (useRainbowAtMaxTier && currentCombo >= 25 && comboText.alpha > 0)
        {
            // Tính toán màu dựa trên thời gian thực
            // Mathf.Repeat(Time.time * speed, 1f) sẽ tạo ra vòng lặp từ 0 -> 1
            float hue = Mathf.Repeat(Time.time * rainbowSpeed, 1f);
            Color rainbowColor = Color.HSVToRGB(hue, 0.8f, 1f); // Saturation 0.8, Value 1 cho màu tươi
            
            comboText.color = rainbowColor;
        }
    }

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

    public void StopCombo()
    {
        if (currentCombo == 0) return;

        currentCombo = 0;

        if (comboText != null)
        {
            comboText.DOKill(); 
            // Khi mất combo, chữ xoay về thẳng đứng và mờ đi
            comboText.transform.DORotate(Vector3.zero, 0.2f);
            comboText.DOFade(0f, 0.2f); 
            comboText.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack); 
        }
    }

    private void PlayJuicyFeedback()
    {
        if (comboText == null) return;

        // -----------------------------------------
        // BƯỚC 1: XỬ LÝ MÀU SẮC VÀ KÍCH THƯỚC DỰA TRÊN CẤP ĐỘ
        // -----------------------------------------
        comboText.text = $"Combo x{currentCombo}!";
        
        Color targetColor = Color.white; // Màu mặc định
        float sizeMult = 1f;

        // Duyệt qua danh sách cấu trúc dữ liệu để tìm màu phù hợp nhất
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
                    break; // Dừng lại vì danh sách phải được xếp tăng dần
                }
            }
        }

        // Áp dụng màu sắc ngay lập tức
        comboText.color = targetColor;

        // -----------------------------------------
        // BƯỚC 2: XỬ LÝ GÓC NGHIÊNG (ROTATION JUICE)
        // -----------------------------------------
        // Tạo góc nghiêng ngẫu nhiên quanh trục Z
        float randomTilt = Random.Range(-maxRotationTilt, maxRotationTilt);
        // Combo càng cao, biên độ nghiêng càng lớn
        float tiltFactor = Mathf.Clamp(1f + (currentCombo * 0.05f), 1f, 2f);
        
        comboText.DOKill(); // Dọn dẹp tween cũ
        
        // Nghiêng chữ cái "bụp" và giữ nguyên
        comboText.transform.localRotation = Quaternion.Euler(0f, 0f, randomTilt * tiltFactor);

        // -----------------------------------------
        // BƯỚC 3: VISUAL FEEDBACK (SCALING & FADE)
        // -----------------------------------------
        comboText.alpha = 1f;
        // Phóng to chữ dựa trên cấp độ màu
        comboText.transform.localScale = Vector3.one * sizeMult;
        
        // Hiệu ứng giật PunchScale
        float punchForce = Mathf.Clamp(0.2f + (currentCombo * 0.1f), 0.2f, 0.8f);
        comboText.transform.DOPunchScale(Vector3.one * punchForce, 0.3f, 5, 1);
        
        // Mờ dần đi
        comboText.DOFade(0f, 0.5f).SetDelay(1f);

        // --- CÁC HIỆU ỨNG KHÁC CỦA BẠN (GIỮ NGUYÊN) ---
        // 4. AUDIO: TĂNG CAO ĐỘ
        if (AudioManager.Instance != null && AudioManager.Instance.sfxArrowHit != null)
        {
            float pitch = Mathf.Clamp(1f + (currentCombo * 0.05f), 1f, 2f);
            //AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowHit, 1f); 
        }

        // 5. KINETIC: RUNG MÀN HÌNH
        if (cameraContainer != null)
        {
            cameraContainer.DOKill(true); 
            float shakeStrength = Mathf.Clamp(0.1f + (currentCombo * 0.05f), 0.1f, 0.5f);
            //cameraContainer.DOShakePosition(baseShakeDuration, shakeStrength, 20, 90f, false, true);
        }

        // 6. TEMPORAL: KHỰNG THỜI GIAN
        if (currentCombo > minComboToShow) 
        {
            float stopDuration = Mathf.Clamp(currentCombo * 0.01f, 0.02f, maxHitStopDuration);
            //StartCoroutine(HitStopRoutine(stopDuration));
        }
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.05f; 
        yield return new WaitForSecondsRealtime(duration); 
        Time.timeScale = 1f; 
    }
}