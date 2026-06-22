using System;
using System.Collections;
using System.Collections.Generic;
using mygame.sdk;
using UnityEngine;
using UnityEngine.UI;

public class UISettingModule : MonoBehaviour
{
    [SerializeField] private Button musicBtn;
    [SerializeField] private Button soundBtn;
    [SerializeField] private Button vibrateBtn;
    [SerializeField] private Button hintBtn;
    [SerializeField] private Button autoRotateBtn;
    [SerializeField] private Button restoreIAPBtn;
    
    private void Start()
    {
        musicBtn.onClick.AddListener(OnClickMusic);
        soundBtn.onClick.AddListener(OnClickSound);
        vibrateBtn.onClick.AddListener(OnClickVibrate);
        hintBtn.onClick.AddListener(OnClickHint);
        //autoRotateBtn.onClick.AddListener(OnClickAutoRotate);
        if (restoreIAPBtn != null) restoreIAPBtn.onClick.AddListener(OnClickRestoreIAP);
    }
    private void OnEnable()
    {
        Refresh();
    }

    private void OnClickRestoreIAP()
    {
        //InappHelper.Instance.RestorePurchases();
    }

    private void OnClickMusic()
    {
        AudioManager.AudioMusicSetting = !AudioManager.AudioMusicSetting;
        AudioManager.Instance.musicSource.volume = AudioManager.AudioMusicSetting ? AudioManager.volumnMusic : 0;
        LogFireBaseCustomer.EnableMusic(AudioManager.AudioMusicSetting);
        AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_UI_Button_Click_Switch);
        musicBtn.GetComponent<Animation>().Play(AudioManager.AudioMusicSetting ? "SettingOn_New" : "SettingOff_New");
        AudioManager.Instance.FixVolumeMusic();
    }

    private void OnClickSound()
    {
        AudioManager.AudioSoundSetting = !AudioManager.AudioSoundSetting;
        AudioManager.Instance.FixVolumeSFX();
        AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_UI_Button_Click_Switch);
        soundBtn.GetComponent<Animation>().Play(AudioManager.AudioSoundSetting ? "SettingOn_New" : "SettingOff_New");
    }

    private void OnClickVibrate()
    {
        PlayerPrefsUtil.AudioVibrateSetting = !PlayerPrefsUtil.AudioVibrateSetting;
        AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_UI_Button_Click_Switch);
        vibrateBtn.GetComponent<Animation>().Play(PlayerPrefsUtil.AudioVibrateSetting ? "SettingOn_New" : "SettingOff_New");
    }

    private void OnClickHint()
    {
        // PlayerPrefsUtil.HintClickObjectSetting = !PlayerPrefsUtil.HintClickObjectSetting;
        // if (!PlayerPrefsUtil.HintClickObjectSetting)
        // {
        //     UIManager.Instance.GetScreenActive<UIInGame>()?.HideHint();
        // }
        // hintBtn.GetComponent<Animation>().Play(PlayerPrefsUtil.HintClickObjectSetting ? "SettingOn_New" : "SettingOff_New");
    }

    private void OnClickAutoRotate()
    {
        PlayerPrefsUtil.LevelAutoRotate = !PlayerPrefsUtil.LevelAutoRotate;
        AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_UI_Button_Click_Switch);
        //autoRotateBtn.GetComponent<Animation>().Play(PlayerPrefsUtil.LevelAutoRotate ? "SettingOn_New" : "SettingOff_New");
    }

    private void Refresh()
    {
        musicBtn.GetComponent<Animation>().Play(AudioManager.AudioMusicSetting ? "SettingOn_New" : "SettingOff_New");
        soundBtn.GetComponent<Animation>().Play(AudioManager.AudioSoundSetting ? "SettingOn_New" : "SettingOff_New");
        vibrateBtn.GetComponent<Animation>().Play(PlayerPrefsUtil.AudioVibrateSetting ? "SettingOn_New" : "SettingOff_New");
        hintBtn.GetComponent<Animation>().Play(PlayerPrefsUtil.HintClickObjectSetting ? "SettingOn_New" : "SettingOff_New");
        //autoRotateBtn.GetComponent<Animation>().Play(PlayerPrefsUtil.LevelAutoRotate ? "SettingOn_New" : "SettingOff_New");
    }
}