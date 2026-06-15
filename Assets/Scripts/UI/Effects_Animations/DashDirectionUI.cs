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

    [Tooltip("Khoảng nín thở trước khi phóng đi")]
    public float holdDuration = 0.08f;

    [Tooltip("Thời gian vút đi")]
    public float shootDuration = 0.3f;

    [Header("Pull Back Grow Settings")]
    [Tooltip("Độ phình to khi nút đang lùi lại")]
    public float pullBackGrowScale = 1.35f;

    [Tooltip("Có dùng hiệu ứng bật nhẹ khi phình to không")]
    public bool useBackEaseWhenGrow = true;

    [Header("Shiver Settings")]
    [Tooltip("Thời gian run rẩy ở cuối pha gồng")]
    public float shiverDuration = 0.25f;

    [Tooltip("Góc nghiêng run rẩy")]
    public float shiverAngle = 8f;

    [Tooltip("Tần số run")]
    public int shiverVibrato = 40;

    private bool _isAnimating = false;

    private Dictionary<ArrowDir, RectTransform> _buttonMap;
    private Dictionary<ArrowDir, Vector2> _originalPositions;
    private Dictionary<ArrowDir, CanvasGroup> _canvasGroupMap;

    private Quaternion _originalRotation = Quaternion.identity;

    private void Awake()
    {
        _buttonMap = new Dictionary<ArrowDir, RectTransform>()
        {
            { ArrowDir.Up, btnUp },
            { ArrowDir.Down, btnDown },
            { ArrowDir.Left, btnLeft },
            { ArrowDir.Right, btnRight }
        };

        _originalPositions = new Dictionary<ArrowDir, Vector2>();
        _canvasGroupMap = new Dictionary<ArrowDir, CanvasGroup>();

        foreach (var kvp in _buttonMap)
        {
            RectTransform rect = kvp.Value;

            if (rect == null)
                continue;

            _originalPositions[kvp.Key] = rect.anchoredPosition;
            _originalRotation = rect.localRotation;

            CanvasGroup canvasGroup = rect.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = rect.gameObject.AddComponent<CanvasGroup>();

            _canvasGroupMap[kvp.Key] = canvasGroup;
        }
    }

    private void OnEnable()
    {
        _isAnimating = false;

        if (overlayBg != null)
            overlayBg.alpha = 1f;

        foreach (var kvp in _buttonMap)
        {
            RectTransform rect = kvp.Value;

            if (rect == null)
                continue;

            rect.DOKill();
            rect.gameObject.SetActive(true);

            rect.localScale = Vector3.one;
            rect.localRotation = _originalRotation;

            if (_originalPositions.ContainsKey(kvp.Key))
                rect.anchoredPosition = _originalPositions[kvp.Key];

            if (_canvasGroupMap.ContainsKey(kvp.Key))
                _canvasGroupMap[kvp.Key].alpha = 1f;

            Button button = rect.GetComponent<Button>();

            if (button != null)
                button.interactable = true;
        }
    }

    public void SelectDirection(int dirIndex)
    {
        if (_isAnimating)
            return;

        ArrowDir selectedDir = (ArrowDir)dirIndex;

        if (!_buttonMap.ContainsKey(selectedDir) || _buttonMap[selectedDir] == null)
            return;

        _isAnimating = true;

        foreach (var kvp in _buttonMap)
        {
            RectTransform rect = kvp.Value;

            if (rect == null)
                continue;

            rect.DOKill();

            Button button = rect.GetComponent<Button>();

            if (button != null)
                button.interactable = false;
        }

        RectTransform selectedBtn = _buttonMap[selectedDir];

        selectedBtn.localScale = Vector3.one;
        selectedBtn.localRotation = _originalRotation;

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);
        bool dashReleaseStarted = false;

        Canvas.ForceUpdateCanvases();

        // ==================================================
        // PHASE 1: CÁC NÚT KHÁC MỜ ĐI, NÚT ĐƯỢC CHỌN VỀ TÂM
        // ==================================================

        foreach (var kvp in _buttonMap)
        {
            if (kvp.Value == null)
                continue;

            if (kvp.Key != selectedDir)
            {
                if (_canvasGroupMap.ContainsKey(kvp.Key))
                {
                    seq.Insert(
                        0f,
                        _canvasGroupMap[kvp.Key]
                            .DOFade(0f, glideDuration * 0.7f)
                            .SetEase(Ease.InQuad)
                    );
                }

                seq.Insert(
                    0f,
                    kvp.Value
                        .DOScale(0.5f, glideDuration)
                        .SetEase(Ease.InQuad)
                );
            }
        }

        seq.Insert(
            0f,
            selectedBtn
                .DOAnchorPos(Vector2.zero, glideDuration)
                .SetEase(Ease.OutQuart)
        );

        // ==================================================
        // PHASE 2: LÙI LẠI + PHÌNH TO DẦN
        // ==================================================

        Vector2 pullBackPos = GetPullBackPosition(selectedDir);
        Vector3 growScale = Vector3.one * pullBackGrowScale;

        seq.Append(
            selectedBtn
                .DOAnchorPos(pullBackPos, tensionDuration)
                .SetEase(Ease.OutCubic)
        );

        Tween growTween = selectedBtn
            .DOScale(growScale, tensionDuration);

        if (useBackEaseWhenGrow)
            growTween.SetEase(Ease.OutBack);
        else
            growTween.SetEase(Ease.OutCubic);

        seq.Join(growTween);

        // ==================================================
        // PHASE 2.5: RUN RẨY Ở CUỐI PHA GỒNG
        // ==================================================

        if (shiverDuration > 0f)
        {
            float shakeStartTime = glideDuration + Mathf.Max(0f, tensionDuration - shiverDuration);

            seq.Insert(
                shakeStartTime,
                selectedBtn.DOShakeRotation(
                    shiverDuration,
                    new Vector3(0f, 0f, shiverAngle),
                    shiverVibrato,
                    90f,
                    false
                )
            );
        }

        // ==================================================
        // PHASE 2.6: KHỰNG LẠI TRƯỚC KHI PHÓNG
        // ==================================================

        seq.AppendInterval(holdDuration);

        seq.AppendCallback(() =>
        {
            if (dashReleaseStarted)
                return;

            dashReleaseStarted = true;

            if (DashManager.Instance != null)
                DashManager.Instance.ExecuteDashFromDirectionUI(selectedDir);
        });

        seq.AppendCallback(() =>
        {
            // Nếu muốn bật haptic thì mở lại đoạn này:
            // if (SettingManager.Instance != null)
            //     SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.HeavyImpact);
        });

        // ==================================================
        // PHASE 3: PHÓNG ĐI
        // ==================================================

        Vector2 shootTarget = GetShootTarget(selectedDir);
        Vector3 stretchScale = GetStretchScale(selectedDir);

        seq.Append(
            selectedBtn
                .DOAnchorPos(shootTarget, shootDuration)
                .SetEase(Ease.InExpo)
        );

        seq.Join(
            selectedBtn
                .DOScale(stretchScale, shootDuration)
                .SetEase(Ease.InExpo)
        );

        if (overlayBg != null)
        {
            seq.Join(
                overlayBg
                    .DOFade(0f, shootDuration * 0.8f)
                    .SetEase(Ease.InQuad)
            );
        }

        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    private Vector2 GetPullBackPosition(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up:
                return new Vector2(0f, -pullBackDistance);

            case ArrowDir.Down:
                return new Vector2(0f, pullBackDistance);

            case ArrowDir.Left:
                return new Vector2(pullBackDistance, 0f);

            case ArrowDir.Right:
                return new Vector2(-pullBackDistance, 0f);

            default:
                return Vector2.zero;
        }
    }

    private Vector3 GetStretchScale(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up:
            case ArrowDir.Down:
                return new Vector3(0.45f, 2.4f, 1f);

            case ArrowDir.Left:
            case ArrowDir.Right:
                return new Vector3(2.4f, 0.45f, 1f);

            default:
                return Vector3.one;
        }
    }

    private Vector2 GetShootTarget(ArrowDir dir)
    {
        float distance = 2000f;

        switch (dir)
        {
            case ArrowDir.Up:
                return new Vector2(0f, distance);

            case ArrowDir.Down:
                return new Vector2(0f, -distance);

            case ArrowDir.Left:
                return new Vector2(-distance, 0f);

            case ArrowDir.Right:
                return new Vector2(distance, 0f);

            default:
                return Vector2.zero;
        }
    }
}
