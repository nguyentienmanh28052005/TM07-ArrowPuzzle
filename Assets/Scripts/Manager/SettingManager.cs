using UnityEngine;
using Pixelplacement;
using System;

public class SettingManager : Singleton<SettingManager>
{
    public Action<bool> OnMusicStateChanged;
    public Action<bool> OnSfxStateChanged;
    public Action<bool> OnHapticStateChanged;

    public bool IsMusicOn { get; private set; }
    public bool IsSfxOn { get; private set; }
    public bool IsHapticOn { get; private set; }

    protected void Awake()
    {
        LoadSettings();
    }

    private void Start()
    {
        // Phải gọi ở Start để đảm bảo AudioManager đã Awake xong
        ApplyMusicState();
        ApplySfxState();
    }

    private void LoadSettings()
    {
        IsMusicOn = PlayerPrefs.GetInt("SETTING_MUSIC", 1) == 1;
        IsSfxOn = PlayerPrefs.GetInt("SETTING_SFX", 1) == 1;
        IsHapticOn = PlayerPrefs.GetInt("SETTING_HAPTIC", 1) == 1;
    }

    public void ToggleMusic()
    {
        IsMusicOn = !IsMusicOn;
        PlayerPrefs.SetInt("SETTING_MUSIC", IsMusicOn ? 1 : 0);
        PlayerPrefs.Save();
        
        ApplyMusicState();
        OnMusicStateChanged?.Invoke(IsMusicOn);
    }

    public void ToggleSfx()
    {
        IsSfxOn = !IsSfxOn;
        PlayerPrefs.SetInt("SETTING_SFX", IsSfxOn ? 1 : 0);
        PlayerPrefs.Save();
        
        ApplySfxState();
        OnSfxStateChanged?.Invoke(IsSfxOn);
    }

    public void ToggleHaptic()
    {
        IsHapticOn = !IsHapticOn;
        PlayerPrefs.SetInt("SETTING_HAPTIC", IsHapticOn ? 1 : 0);
        PlayerPrefs.Save();
        
        OnHapticStateChanged?.Invoke(IsHapticOn);
        
        // Test rung nhẹ ngay khi bật lại
        if (IsHapticOn) PlayHaptic(Solo.MOST_IN_ONE.MOST_HapticFeedback.HapticTypes.LightImpact);
    }

    private void ApplyMusicState()
    {
        if (AudioManager.Instance != null)
        {
            // Bật Setting (IsMusicOn) thì TẮT Mute (Mute = false) và ngược lại
            AudioManager.Instance.IsMusicMuted = !IsMusicOn; 
        }
    }

    private void ApplySfxState()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.IsSfxMuted = !IsSfxOn;
        }
    }

    /// <summary>
    /// HÀM KIỂM DUYỆT RUNG CẤP CAO (Giao tiếp với MOST_IN_ONE)
    /// </summary>
    public void PlayHaptic(Solo.MOST_IN_ONE.MOST_HapticFeedback.HapticTypes hapticType)
    {
        if (IsHapticOn)
        {
            Solo.MOST_IN_ONE.MOST_HapticFeedback.Generate(hapticType);
        }
    }
}