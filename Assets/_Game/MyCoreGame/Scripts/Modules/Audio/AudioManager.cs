using System;
using mygame.sdk;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class AudioManager : master.Singleton<AudioManager>
{
    public static bool CF_DefaultMusic
    {
        get => PlayerPrefs.GetInt("cf_default_music", 0) == 1;
        set => PlayerPrefs.SetInt("cf_default_music", value ? 1 : 0);
    }

    public static bool AudioMusicSetting
    {
        get => PlayerPrefs.GetInt("audio_music_setting", CF_DefaultMusic ? 1 : 0) == 1;
        set => PlayerPrefs.SetInt("audio_music_setting", value ? 1 : 0);
    }

    public static bool AudioSoundSetting
    {
        get => PlayerPrefs.GetInt("audio_sound_setting", 1) == 1;
        set => PlayerPrefs.SetInt("audio_sound_setting", value ? 1 : 0);
    }

    public static bool AudioVibrateSetting
    {
        get => PlayerPrefs.GetInt(GameHelper.KeyConfigVibrate, 1) == 1;
        set => PlayerPrefs.SetInt(GameHelper.KeyConfigVibrate, value ? 1 : 0);
    }

    // Instance is provided by Singleton<AudioManager> base class
    int playing;
    bool canPlay = true;

    [Header("SoundFX")] public AudioClip[] soundsFX;
    [Header("Music")] public AudioClip[] musics;

    private Dictionary<string, AudioClip> sfxLookup;
    private Dictionary<string, AudioClip> musicLookup;

    public AudioSource musicSource;
    public AudioSource soundSource;
    public AudioSource soundFXLoopSource;

    private Coroutine _loopCoroutine;
    private string _currentLoopName; // clip name đang pending hoặc đang play

    private float musicPlayTime;
    private bool musicIsPlaying;

    [SerializeField] AudioMixer audioMixer;
    private AudioConfiguration audioConfiguration;
    [field: SerializeField] private List<AudioSource> listAudioSources = new List<AudioSource>();

    public float Ratio_Sound
    {
        get
        {
#if UNITY_ANDROID
            return PlayerPrefs.GetFloat("cf_ratio_sound", 0.7f);
#endif
            return PlayerPrefs.GetFloat("cf_ratio_sound", 0.475f);
        }
        set { PlayerPrefs.SetFloat("cf_ratio_sound", Mathf.Clamp(value, 0, 0.7f)); }
    }

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        BuildLookupTables();

        listAudioSources.Clear();
        listAudioSources.Add(soundSource);
        listAudioSources.Add(soundFXLoopSource);
        for (int i = 0; i < 6; i++)
        {
            AudioSource sourceFX = Instantiate(soundSource, transform);
            listAudioSources.Add(sourceFX);
        }

        musicSource.volume = AudioMusicSetting ? 1 * Ratio_Sound : 0;
        soundSource.volume = AudioSoundSetting ? 1 * Ratio_Sound : 0;
        this.WaitUntil(() => audioMixer != null, () =>
        {
            FixVolumeSFX();
            FixVolumeMusic();
        });
    }

    private void BuildLookupTables()
    {
        sfxLookup = new Dictionary<string, AudioClip>(soundsFX != null ? soundsFX.Length : 0);
        if (soundsFX != null)
        {
            foreach (var clip in soundsFX)
                if (clip != null) sfxLookup[clip.name] = clip;
        }

        musicLookup = new Dictionary<string, AudioClip>(musics != null ? musics.Length : 0);
        if (musics != null)
        {
            foreach (var clip in musics)
                if (clip != null) musicLookup[clip.name] = clip;
        }
    }

    private void Start()
    {
        audioConfiguration = AudioSettings.GetConfiguration();
    }

    private AudioSource GetAudioSource()
    {
        AudioSource audioSource = null;
        for (int i = 0; i < listAudioSources.Count; i++)
        {
            if (listAudioSources[i].isPlaying)
            {
                continue;
            }
            else
            {
                audioSource = listAudioSources[i];
                break;
            }
        }

        if (audioSource == null)
        {
            audioSource = listAudioSources[0];
        }

        return audioSource;
    }

    public void ChangeStateAudio()
    {
        if (AudioMusicSetting)
        {
            if (GameManager.GameState == GameState.None)
            {
                PlayBGMusicMain();
            }
            else
            {
                PlayBGMusicInGame();
            }
        }
        else
        {
            StopMusic();
        }
    }

    public void PlayOneShotByClip(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            soundSource.clip = clip;
            soundSource.PlayOneShot(clip, volume);
        }

        soundSource.volume = AudioSoundSetting ? 1 : 0;
    }

    public const float volumnMusic = 0.2f;

    public void Play(string name, float volume, bool isloop = false)
    {
        if (musicLookup != null && musicLookup.TryGetValue(name, out var s))
        {
            musicSource.clip = s;
            musicSource.volume = volumnMusic;
            musicSource.loop = isloop;
            musicSource.Play();
        }

        musicSource.volume = AudioMusicSetting ? volumnMusic : 0;
    }

    public void PlayBGMusicMain()
    {
        Play(AUDIO_CLIP_NAME.BG_Music, 1f, true);
    }

    public void PlayBGMusicInGame()
    {
        Play(AUDIO_CLIP_NAME.BG_Music, 1f, true);
    }

    public void PlayClip(AudioClip s, float volume, bool isloop = false)
    {
        // if (isTurnOnSound == false) return;
        if (s != null)
        {
            musicSource.clip = s;
            musicSource.volume = volume * Ratio_Sound;
            musicSource.loop = isloop;
            musicSource.Play();
        }

        musicSource.volume = AudioMusicSetting ? volume * Ratio_Sound : 0;
    }

    public void StopMusic() => musicSource.Stop();
    public void StopSFX() => soundSource.Stop();

    private Coroutine _loopFadeCoroutine;
    private float _loopTargetVolume;

    /// <summary>
    /// Phát SFX loop liên tục cho đến khi gọi StopLoop().
    /// Nếu đang phát cùng clip → không restart. Hỗ trợ delayPlay và fadeIn.
    /// </summary>
    public void PlayLoop(string clipName, float delayPlay = 0f, float volume = 1f, float timeFadeIn = 0.15f)
    {
        if (!AudioSoundSetting) return;

        // Nếu đúng clip này đang chạy rồi (không phải pending khác) → không restart
        if (_currentLoopName == clipName && soundFXLoopSource.isPlaying) return;

        // Cancel coroutine cũ (dù đang delay hay đang stop-delay)
        if (_loopCoroutine != null) { StopCoroutine(_loopCoroutine); _loopCoroutine = null; }
        if (_loopFadeCoroutine != null) { StopCoroutine(_loopFadeCoroutine); _loopFadeCoroutine = null; }

        _currentLoopName = clipName;
        _loopTargetVolume = volume * Ratio_Sound;
        _loopCoroutine = StartCoroutine(PlayLoopCoroutine(clipName, delayPlay, timeFadeIn));
    }

    private IEnumerator PlayLoopCoroutine(string clipName, float delayPlay, float timeFadeIn)
    {
        if (delayPlay > 0f) yield return new WaitForSeconds(delayPlay);

        if (sfxLookup == null || !sfxLookup.TryGetValue(clipName, out var clip))
        { _loopCoroutine = null; yield break; }

        // Stop clip cũ nếu khác loại
        if (soundFXLoopSource.isPlaying && soundFXLoopSource.clip != clip)
            soundFXLoopSource.Stop();

        soundFXLoopSource.clip = clip;
        soundFXLoopSource.volume = 0f;
        soundFXLoopSource.loop = true;
        soundFXLoopSource.Play();
        _loopCoroutine = null;

        // FadeIn
        _loopFadeCoroutine = StartCoroutine(FadeLoopVolume(0f, _loopTargetVolume, timeFadeIn));
    }

    /// <summary>Dừng loop ngay lập tức — bất kể đang phát clip nào.</summary>
    public void StopLoop(float timeFadeOut = 0.15f) => StopLoopInternal(null, 0f, timeFadeOut);

    /// <summary>Dừng loop sau delay — bất kể đang phát clip nào.</summary>
    public void StopLoop(float delayStop, float timeFadeOut = 0.15f) => StopLoopInternal(null, delayStop, timeFadeOut);

    /// <summary>Chỉ dừng nếu đúng clip đang phát, hỗ trợ delay và fadeOut.</summary>
    public void StopLoop(string clipName, float delayStop = 0f, float timeFadeOut = 0.15f) => StopLoopInternal(clipName, delayStop, timeFadeOut);

    private void StopLoopInternal(string clipName, float delayStop, float timeFadeOut)
    {
        // Nếu chỉ định clip nhưng không phải clip hiện tại → bỏ qua
        if (clipName != null && _currentLoopName != clipName) return;

        // Cancel mọi coroutine đang chạy
        if (_loopCoroutine != null) { StopCoroutine(_loopCoroutine); _loopCoroutine = null; }
        if (_loopFadeCoroutine != null) { StopCoroutine(_loopFadeCoroutine); _loopFadeCoroutine = null; }

        _currentLoopName = null;

        if (soundFXLoopSource == null || !soundFXLoopSource.isPlaying) return;

        _loopCoroutine = StartCoroutine(StopLoopCoroutine(delayStop, timeFadeOut));
    }

    private IEnumerator StopLoopCoroutine(float delayStop, float timeFadeOut)
    {
        if (delayStop > 0f) yield return new WaitForSeconds(delayStop);

        if (soundFXLoopSource != null && soundFXLoopSource.isPlaying)
        {
            // FadeOut rồi mới Stop
            yield return FadeLoopVolume(soundFXLoopSource.volume, 0f, timeFadeOut);
            soundFXLoopSource.Stop();
        }

        _loopCoroutine = null;
    }

    private IEnumerator FadeLoopVolume(float from, float to, float duration)
    {
        if (duration <= 0f) { soundFXLoopSource.volume = to; yield break; }
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            soundFXLoopSource.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        soundFXLoopSource.volume = to;
        _loopFadeCoroutine = null;
    }


    public void PlayOneShot(string name, float volume = 1, float delayPlay = 0)
    {
        if (!AudioSoundSetting) return;
        if (playing > 10 && canPlay) return;

        // Fast path: no delay, no coroutine → zero GC alloc
        if (delayPlay <= 0f)
        {
            if (sfxLookup == null || !sfxLookup.TryGetValue(name, out var clip)) return;
            AudioSource audioSource = GetAudioSource();
            audioSource.volume = volume * Ratio_Sound;
            audioSource.PlayOneShot(clip, volume * Ratio_Sound);
            return;
        }

        // Slow path: has delay, use coroutine
        AudioSource audioSrc = GetAudioSource();
        StartCoroutine(PlayByName(audioSrc, name, volume, delayPlay));
        canPlay = false;
        audioSrc.volume = volume * Ratio_Sound;
    }

    public void PlayOneShot(AudioClip clip, float volume = 1, float delayPlay = 0)
    {
        // if (isTurnOnSound == false || !gameObject.activeSelf) return;
        if (playing > 10 && canPlay) return;
        AudioSource audioSource = GetAudioSource();
        StartCoroutine(PlayByClip(audioSource, clip, volume, delayPlay));
        canPlay = false;
        audioSource.volume = AudioSoundSetting ? volume * Ratio_Sound : 0;
    }

    IEnumerator PlayByName(AudioSource _audioSource, string _name, float _volume, float _delayPlay = 0)
    {
        if (sfxLookup == null || !sfxLookup.TryGetValue(_name, out var s))
            yield break;

        if (_delayPlay > 0f)
            yield return new WaitForSeconds(_delayPlay);

        _volume = _volume * Ratio_Sound;
        if (s != null)
        {
            playing++;
            canPlay = true;
            _audioSource.clip = s;
            _audioSource.PlayOneShot(s, _volume);
            yield return new WaitForSeconds(0.2f);
            playing--;
        }

        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator PlayByClip(AudioSource _audioSource, AudioClip clip, float _volume, float _delayPlay = 0)
    {
        yield return new WaitForSeconds(_delayPlay);
        _volume = _volume * Ratio_Sound;
        if (clip != null)
        {
            playing++;
            canPlay = true;
            _audioSource.clip = clip;
            _audioSource.PlayOneShot(clip, _volume);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.2f));
            playing--;
        }

        yield return new WaitForSeconds(0.2f);
    }

    public void SetVolume(float volume)
    {
        musicSource.volume = 0.3f;
    }

    public void PauseAudio()
    {
        AudioListener.pause = true;
    }

    public void ResumeAudio()
    {
        AudioListener.pause = false;
    }

    public void ResetAudio()
    {
#if UNITY_IOS
        AudioSettings.Reset(audioConfiguration);
        FixVolumeMusic();
        if (musicIsPlaying)
        {
            musicSource.time = musicPlayTime;
            musicSource.Play();
        }
#endif
    }

    public void SetCacheAudio()
    {
#if UNITY_IOS
        musicPlayTime = musicSource.time;
        musicIsPlaying = musicSource.isPlaying;
#endif
    }

    public void settingMusic(int volume)
    {
        musicSource.volume = 0.3f;
    }

    public void settingSound(int volume)
    {
        soundSource.volume = volume;
    }

    public void FixVolumeSFX()
    {
        float vol = AudioSoundSetting ? 1 * Ratio_Sound : 0;
        float dB = Mathf.Log10(Mathf.Clamp(vol, 0.0001f, 1)) * 20;
        audioMixer.SetFloat("SFXVolume", dB);
    }

    public void FixVolumeMusic()
    {
        float vol = AudioMusicSetting ? 1 * Ratio_Sound : 0;
        float dB = Mathf.Log10(Mathf.Clamp(vol, 0.0001f, 1)) * 20;
        audioMixer.SetFloat("SFXMusic", dB);
    }

    public void PlayVibrate(Type_vibreate type_Vibreate = Type_vibreate.Vib_Medium)
    {
        GameHelper.Instance.Vibrate(type_Vibreate);
    }
}