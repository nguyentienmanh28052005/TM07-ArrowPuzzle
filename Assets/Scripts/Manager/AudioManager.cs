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
    
    private List<AudioSource> _audioSourcePool;
    private AudioClip _currentMusicClip;

    private float _musicVolume = 1f;
    private float _sfxVolume = 1f;
    private bool _isSfxMuted = false;

    // =====================================
    // TÍNH NĂNG MỚI: BẬT/TẮT MUTE AN TOÀN
    // =====================================
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
            _musicVolume = value;
            _musicSource.volume = value;
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
        if (_musicSource != null) _musicSource.volume = MusicVolume;
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
        source.volume = volume; // Giờ đây thoải mái gán volume, vì nếu Mute = true thì nó vẫn không kêu!
        source.pitch = pitch;
        source.PlayOneShot(clip);
    }
   
    public void PlayMusic(AudioClip clip, bool isLoop = true)
    {
        StartCoroutine(FadeOutAndIn(_musicSource, clip, isLoop));
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
            audioSource.volume = Mathf.Lerp(0, MusicVolume, currentTime / 1f);
            yield return null;
        }

        _currentMusicClip = newClip;
    }

    public AudioClip GetCurrentMusicClip()
    {
        return _currentMusicClip;
    }
}