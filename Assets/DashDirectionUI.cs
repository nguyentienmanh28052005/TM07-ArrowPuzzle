using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Solo.MOST_IN_ONE; 

public class DashDirectionUI : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup overlayBg; 
    
    [Header("Direction Buttons (RectTransform)")]
    public RectTransform btnUp;
    public RectTransform btnDown;
    public RectTransform btnLeft;
    public RectTransform btnRight;

    [Header("Tension Animation Settings")]
    [Tooltip("Thời gian trượt về tâm ban đầu")]
    public float glideDuration = 0.4f; 
    [Tooltip("Thời gian gồng kéo lùi về sau")]
    public float tensionDuration = 0.6f;
    [Tooltip("Khoảng cách kéo lùi lại")]
    public float pullBackDistance = 45f;
    [Tooltip("Khoảng nín thở (Giữ nguyên độ căng) trước khi nhả tay")]
    public float holdDuration = 0.08f;
    [Tooltip("Thời gian vút đi")]
    public float shootDuration = 0.3f;

    [Header("Shiver (Run Rẩy) Settings")]
    [Tooltip("Thời gian run rẩy ở cuối pha gồng")]
    public float shiverDuration = 0.25f;
    [Tooltip("Góc nghiêng run rẩy (Độ)")]
    public float shiverAngle = 8f;
    [Tooltip("Tần số run (Càng cao càng run nhanh)")]
    public int shiverVibrato = 40;

    private bool _isAnimating = false;
    private Dictionary<ArrowDir, RectTransform> _buttonMap;
    private Dictionary<ArrowDir, Vector2> _originalPositions;
    private Dictionary<ArrowDir, CanvasGroup> _canvasGroupMap; 
    private Quaternion _originalRotation;

    private void Awake()
    {
        _buttonMap = new Dictionary<ArrowDir, RectTransform>()
        {
            { ArrowDir.Up, btnUp }, { ArrowDir.Down, btnDown },
            { ArrowDir.Left, btnLeft }, { ArrowDir.Right, btnRight }
        };

        _originalPositions = new Dictionary<ArrowDir, Vector2>();
        _canvasGroupMap = new Dictionary<ArrowDir, CanvasGroup>();
        _originalRotation = btnUp.localRotation; // Lưu lại góc xoay gốc

        foreach (var kvp in _buttonMap)
        {
            if (kvp.Value != null)
            {
                _originalPositions.Add(kvp.Key, kvp.Value.anchoredPosition);
                CanvasGroup cg = kvp.Value.GetComponent<CanvasGroup>();
                if (cg == null) cg = kvp.Value.gameObject.AddComponent<CanvasGroup>();
                _canvasGroupMap.Add(kvp.Key, cg);
            }
        }
    }

    private void OnEnable()
    {
        _isAnimating = false;
        if (overlayBg != null) overlayBg.alpha = 1f;

        foreach (var kvp in _buttonMap)
        {
            if (kvp.Value != null)
            {
                kvp.Value.DOKill();
                kvp.Value.gameObject.SetActive(true);
                kvp.Value.localScale = Vector3.one;
                kvp.Value.localRotation = _originalRotation;
                kvp.Value.anchoredPosition = _originalPositions[kvp.Key];
                _canvasGroupMap[kvp.Key].alpha = 1f; 
                
                Button btn = kvp.Value.GetComponent<Button>();
                if (btn != null) btn.interactable = true;
            }
        }
    }

    public void SelectDirection(int dirIndex)
    {
        ArrowDir selectedDir = (ArrowDir)dirIndex;
        if (_isAnimating) return; 
        _isAnimating = true;

        foreach (var kvp in _buttonMap)
        {
            if (kvp.Value != null)
            {
                kvp.Value.DOKill(); 
                Button btn = kvp.Value.GetComponent<Button>();
                if (btn != null) btn.interactable = false;
            }
        }

        RectTransform selectedBtn = _buttonMap[selectedDir];
        
        // HARD RESET toàn diện (Cả Scale và Rotation)
        selectedBtn.localScale = Vector3.one; 
        selectedBtn.localRotation = _originalRotation;

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); 
        Canvas.ForceUpdateCanvases();
        
        // ==========================================
        // PHASE 1: TRƯỢT VỀ TÂM 
        // ==========================================
        foreach (var kvp in _buttonMap)
        {
            if (kvp.Key != selectedDir)
            {
                seq.Insert(0f, _canvasGroupMap[kvp.Key].DOFade(0f, glideDuration * 0.7f));
                seq.Insert(0f, kvp.Value.DOScale(0.5f, glideDuration).SetEase(Ease.InQuad));
            }
        }
        seq.Insert(0f, selectedBtn.DOAnchorPos(Vector2.zero, glideDuration).SetEase(Ease.OutQuart));

        // ==========================================
        // PHASE 2: ÉP LÒ XO & GỒNG LỰC (THE ULTIMATE TENSION)
        // ==========================================
        Vector2 pullBackPos = GetPullBackPosition(selectedDir);
        Vector3 compressionScale = GetCompressionScale(selectedDir); // Nén dẹt mũi tên lại
        
        // 1. Trượt lùi chậm dần đều (OutCubic) tạo cảm giác đụng phải giới hạn của dây cung
        seq.Append(selectedBtn.DOAnchorPos(pullBackPos, tensionDuration).SetEase(Ease.OutCubic));
        
        // 2. Ép dẹt mũi tên lại theo hướng kéo (Tạo sức nặng vật lý)
        seq.Join(selectedBtn.DOScale(compressionScale, tensionDuration).SetEase(Ease.OutCubic));
        
        // 3. Rung bần bật theo trục xoay (Shiver) ở nửa cuối pha gồng
        if (shiverDuration > 0f)
        {
            float shakeStartTime = glideDuration + (tensionDuration - shiverDuration);
            seq.Insert(shakeStartTime, selectedBtn.DOShakeRotation(shiverDuration, new Vector3(0, 0, shiverAngle), shiverVibrato, 90, false));
        }

        // ==========================================
        // PHASE 2.5: ĐIỂM CHẾT (THE HOLD)
        // ==========================================
        // Khựng lại một nhịp siêu ngắn, nín thở trước khi buông tay
        seq.AppendInterval(holdDuration);
        seq.AppendCallback(() => {
            if (SettingManager.Instance != null) 
                SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.HeavyImpact);
        });

        // ==========================================
        // PHASE 3: BUNG LỰC (RELEASE)
        // ==========================================
        Vector2 shootTarget = GetShootTarget(selectedDir);
        Vector3 stretchScale = GetStretchScale(selectedDir);
        
        // Vút đi với vận tốc xé gió (InExpo)
        seq.Append(selectedBtn.DOAnchorPos(shootTarget, shootDuration).SetEase(Ease.InExpo));
        
        // Đảo ngược trạng thái: Từ ép dẹt (Squash) sang kéo giãn (Stretch)
        seq.Join(selectedBtn.DOScale(stretchScale, shootDuration).SetEase(Ease.InExpo));
        
        if (overlayBg != null)
            seq.Join(overlayBg.DOFade(0f, shootDuration * 0.8f).SetEase(Ease.InQuad));

        seq.OnComplete(() =>
        {
            DashManager.Instance.ExecuteDash(selectedDir);
            gameObject.SetActive(false);
        });
    }

    private Vector2 GetPullBackPosition(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up:    return new Vector2(0, -pullBackDistance); 
            case ArrowDir.Down:  return new Vector2(0, pullBackDistance);  
            case ArrowDir.Left:  return new Vector2(pullBackDistance, 0);  
            case ArrowDir.Right: return new Vector2(-pullBackDistance, 0); 
            default: return Vector2.zero;
        }
    }

    // HIỆU ỨNG NÉN (Squash): Bị ép ngắn lại và phình to bề ngang
    private Vector3 GetCompressionScale(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up:   case ArrowDir.Down:  return new Vector3(1.3f, 0.75f, 1f); 
            case ArrowDir.Left: case ArrowDir.Right: return new Vector3(0.75f, 1.3f, 1f); 
            default: return Vector3.one;
        }
    }

    // HIỆU ỨNG GIÃN (Stretch): Kéo dài ngoằng ra và ốm lại
    private Vector3 GetStretchScale(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up:   case ArrowDir.Down:  return new Vector3(0.4f, 2.5f, 1f);
            case ArrowDir.Left: case ArrowDir.Right: return new Vector3(2.5f, 0.4f, 1f);
            default: return Vector3.one;
        }
    }

    private Vector2 GetShootTarget(ArrowDir dir)
    {
        float dist = 2000f;
        switch (dir)
        {
            case ArrowDir.Up:    return new Vector2(0, dist);
            case ArrowDir.Down:  return new Vector2(0, -dist);
            case ArrowDir.Left:  return new Vector2(-dist, 0);
            case ArrowDir.Right: return new Vector2(dist, 0);
            default: return Vector2.zero;
        }
    }
}