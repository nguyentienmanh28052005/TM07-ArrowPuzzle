using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class SettingToggleUI : MonoBehaviour, IPointerClickHandler
{
    public enum SettingType { Music, SFX, Haptic }
    
    [Header("Cấu Hình Nút")]
    [Tooltip("Nút này dùng để chỉnh cái gì?")]
    public SettingType targetSetting;
    
    [Header("Giao Diện")]
    [SerializeField] private Image toggleImage; // Kéo ảnh nền của nút vào đây
    [SerializeField] private Sprite spriteOn;   // Ảnh khi Bật
    [SerializeField] private Sprite spriteOff; 
    
    [SerializeField] private GameObject offBar; // Ảnh khi Tắt

    [Header("Hiệu Ứng Nảy (Juice)")]
    [SerializeField] private float punchScale = 0.2f;

    [SerializeField, Range(0f, 1f)] private float offColorMultiplier = 0.7f;

    private Color _baseToggleColor = Color.white;
    private Color _lastAppliedColor = Color.clear;
    private bool _hasBaseToggleColor;
    private bool _hasAppliedColor;

    /// <summary>
    /// Bật cái đài lên và dò đúng tần số mà nút này đang quan tâm
    /// </summary>
    private void OnEnable()
    {
        if (SettingManager.Instance == null) return;

        switch (targetSetting)
        {
            case SettingType.Music:
                SettingManager.Instance.OnMusicStateChanged += UpdateVisual;
                UpdateVisual(SettingManager.Instance.IsMusicOn); // Cập nhật ảnh ngay lúc mở bảng
                break;
            case SettingType.SFX:
                SettingManager.Instance.OnSfxStateChanged += UpdateVisual;
                UpdateVisual(SettingManager.Instance.IsSfxOn);
                break;
            case SettingType.Haptic:
                SettingManager.Instance.OnHapticStateChanged += UpdateVisual;
                UpdateVisual(SettingManager.Instance.IsHapticOn);
                break;
        }
    }

    /// <summary>
    /// Tắt đài đi khi bảng UI bị ẩn, chống lỗi tràn bộ nhớ
    /// </summary>
    private void OnDisable()
    {
        if (SettingManager.Instance == null) return;

        switch (targetSetting)
        {
            case SettingType.Music:
                SettingManager.Instance.OnMusicStateChanged -= UpdateVisual;
                break;
            case SettingType.SFX:
                SettingManager.Instance.OnSfxStateChanged -= UpdateVisual;
                break;
            case SettingType.Haptic:
                SettingManager.Instance.OnHapticStateChanged -= UpdateVisual;
                break;
        }
    }

    /// <summary>
    /// Khi ngón tay người chơi chọt vào nút
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 1. Phình to nảy nút (Juice)
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * punchScale, 0.2f, 5, 1).SetUpdate(true);

        // 2. Gửi lệnh yêu cầu lật trạng thái về cho Tổng đài
        switch (targetSetting)
        {
            case SettingType.Music: SettingManager.Instance.ToggleMusic(); break;
            case SettingType.SFX: SettingManager.Instance.ToggleSfx(); break;
            case SettingType.Haptic: SettingManager.Instance.ToggleHaptic(); break;
        }

        // 3. Phát tiếng "Click" (Nếu SFX đang tắt thì AudioManager sẽ tự động chặn)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(AudioManager.Instance.btnClick);
        }
    }

    /// <summary>
    /// Hàm này sẽ tự động chạy mỗi khi Tổng đài hét lên loa
    /// </summary>
    private void UpdateVisual(bool isOn)
    {
        if (toggleImage != null)
        {
            CaptureBaseToggleColor();

            // Tráo ảnh Bật/Tắt
            toggleImage.sprite = isOn ? spriteOn : spriteOff;
            if (isOn && offBar != null) offBar.SetActive(false);
            else if (offBar != null) offBar.SetActive(true);
            
            // Ép màu tối đi một chút nếu đang Tắt cho ngầu
            Color visualColor = GetToggleVisualColor(isOn);
            toggleImage.color = visualColor;
            _lastAppliedColor = visualColor;
            _hasAppliedColor = true;
        }
    }

    private void CaptureBaseToggleColor()
    {
        if (toggleImage == null) return;

        if (!_hasBaseToggleColor || !_hasAppliedColor || toggleImage.color != _lastAppliedColor)
        {
            _baseToggleColor = toggleImage.color;
            _hasBaseToggleColor = true;
        }
    }

    private Color GetToggleVisualColor(bool isOn)
    {
        Color color = _baseToggleColor;
        if (!isOn)
        {
            color.r *= offColorMultiplier;
            color.g *= offColorMultiplier;
            color.b *= offColorMultiplier;
        }

        return color;
    }
}
