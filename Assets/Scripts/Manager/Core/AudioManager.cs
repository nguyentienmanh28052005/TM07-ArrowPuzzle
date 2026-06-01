using System.Collections;
using System.Collections.Generic;
using Pixelplacement;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private int _poolSize = 10;
    [SerializeField] private AudioSource _musicSource;

    [Header("SFX Clips")] 
    public AudioClip sfxArrowTap;
    public AudioClip sfxArrowHit;
    public AudioClip btnClick;
    public AudioClip loseSound;
    public AudioClip winSound;
    public AudioClip coinHit;
    
    public AudioClip starHit;
    
    private List<AudioSource> _audioSourcePool;
    private AudioClip _currentMusicClip;
    private Coroutine _musicFadeRoutine;

    private float _musicVolume = 1f;
    private float _musicVolumeScale = 1f;
    private float _sfxVolume = 1f;
    private bool _isSfxMuted = false;

    private float EffectiveMusicVolume => Mathf.Clamp01(_musicVolume * _musicVolumeScale);

    public bool IsMusicMuted
    {
        get => _musicSource.mute;
        set => _musicSource.mute = value;
    }

    public bool IsSfxMuted
    {
        get => _isSfxMuted;
        set
        {
            _isSfxMuted = value;
            foreach (var source in _audioSourcePool)
            {
                source.mute = value;
            }
        }
    }

    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            _musicVolume = Mathf.Max(0f, value);
            if (_musicSource != null) _musicSource.volume = EffectiveMusicVolume;
        }
    }

    public float SFXVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = value;
            foreach (var source in _audioSourcePool)
            {
                source.volume = value;
            }
        }
    }

    protected void Awake()
    { // Gọi base.Awake() vì chúng ta đang kế thừa Singleton
        InitializeAudioSourcePool();
    }

    // 2. Hàm Start giờ chỉ còn dùng để gán Volume
    private void Start()
    {
        if (_musicSource != null) _musicSource.volume = EffectiveMusicVolume;
    }

    private void InitializeAudioSourcePool()
    {
        _audioSourcePool = new List<AudioSource>();
        for (int i = 0; i < _poolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = SFXVolume;
            source.mute = _isSfxMuted; 
            _audioSourcePool.Add(source);
        }
    }

    public void PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        AudioSource source = GetAvailableAudioSource();
        source.volume = volume;
        source.pitch = pitch;
        source.PlayOneShot(clip);
    }
   
    public void PlayMusic(AudioClip clip, bool isLoop = true, float volumeScale = 1f)
    {
        if (_musicSource == null || clip == null) return;

        float targetVolumeScale = Mathf.Clamp01(volumeScale);
        if (_currentMusicClip == clip && _musicSource.isPlaying)
        {
            _musicVolumeScale = targetVolumeScale;
            if (_musicFadeRoutine == null) _musicSource.volume = EffectiveMusicVolume;
            return;
        }

        if (_musicFadeRoutine != null) StopCoroutine(_musicFadeRoutine);
        _currentMusicClip = clip;
        _musicVolumeScale = targetVolumeScale;
        _musicFadeRoutine = StartCoroutine(FadeOutAndIn(_musicSource, clip, isLoop));
    }

    public void SetCurrentMusicVolumeScale(float volumeScale)
    {
        _musicVolumeScale = Mathf.Clamp01(volumeScale);
        if (_musicSource != null) _musicSource.volume = EffectiveMusicVolume;
    }

    public void StopMusic(bool fadeOut = true)
    {
        if (_musicSource == null) return;

        if (_musicFadeRoutine != null)
        {
            StopCoroutine(_musicFadeRoutine);
            _musicFadeRoutine = null;
        }

        if (fadeOut && _musicSource.isPlaying)
        {
            _musicFadeRoutine = StartCoroutine(FadeOutMusic(_musicSource));
            return;
        }

        _musicSource.Stop();
        _musicSource.clip = null;
        _currentMusicClip = null;
        _musicVolumeScale = 1f;
        _musicSource.volume = EffectiveMusicVolume;
    }

    private AudioSource GetAvailableAudioSource()
    {
        foreach (var source in _audioSourcePool)
        {
            if (!source.isPlaying) return source;
        }
        
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        newSource.volume = SFXVolume;
        newSource.mute = _isSfxMuted; // An toàn tuyệt đối
        _audioSourcePool.Add(newSource);
        return newSource;
    }

    public void StopAllSfx()
    {
        if (_audioSourcePool == null) return;
        foreach (var source in _audioSourcePool)
        {
            if (source != null)
            {
                source.Stop();
            }
        }
    }

    private IEnumerator FadeOutAndIn(AudioSource audioSource, AudioClip newClip, bool isLoop)
    {
        float currentTime = 0;
        float startVolume = audioSource.volume;

        while (currentTime < 1f)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, currentTime / 1f);
            yield return null;
        }

        audioSource.clip = newClip;
        audioSource.loop = isLoop;
        audioSource.Play();

        currentTime = 0;
        while (currentTime < 1f)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0, EffectiveMusicVolume, currentTime / 1f);
            yield return null;
        }

        _currentMusicClip = newClip;
        _musicFadeRoutine = null;
    }

    private IEnumerator FadeOutMusic(AudioSource audioSource)
    {
        float currentTime = 0f;
        float startVolume = audioSource.volume;

        while (currentTime < 1f)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / 1f);
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = null;
        _currentMusicClip = null;
        _musicVolumeScale = 1f;
        audioSource.volume = EffectiveMusicVolume;
        _musicFadeRoutine = null;
    }

    public AudioClip GetCurrentMusicClip()
    {
        return _currentMusicClip;
    }
}
