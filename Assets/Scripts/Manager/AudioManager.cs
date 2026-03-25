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

    /// <summary>
    /// Khởi tạo âm lượng mặc định và xây dựng Object Pool cho các luồng âm thanh.
    /// </summary>
    private void Start()
    {
        _musicSource.volume = MusicVolume;
        InitializeAudioSourcePool();
    }

    /// <summary>
    /// Tạo danh sách các AudioSource ẩn để tái sử dụng, tối ưu hóa bộ nhớ.
    /// </summary>
    private void InitializeAudioSourcePool()
    {
        _audioSourcePool = new List<AudioSource>();
        for (int i = 0; i < _poolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = SFXVolume;
            _audioSourcePool.Add(source);
        }
    }

    /// <summary>
    /// Phát một hiệu ứng âm thanh (SFX) từ Pool với mức âm lượng và độ cao tùy chỉnh.
    /// </summary>
    public void PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        AudioSource source = GetAvailableAudioSource();
        source.volume = volume;
        source.pitch = pitch;
        source.PlayOneShot(clip);
    }
   
    /// <summary>
    /// Phát nhạc nền với hiệu ứng chuyển tiếp mờ dần (Crossfade).
    /// </summary>
    public void PlayMusic(AudioClip clip, bool isLoop = true)
    {
        StartCoroutine(FadeOutAndIn(_musicSource, clip, isLoop));
    }

    /// <summary>
    /// Tìm một AudioSource đang rảnh trong Pool, hoặc tạo mới nếu Pool đã đầy.
    /// </summary>
    private AudioSource GetAvailableAudioSource()
    {
        foreach (var source in _audioSourcePool)
        {
            if (!source.isPlaying) return source;
        }
        
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        newSource.volume = SFXVolume;
        _audioSourcePool.Add(newSource);
        return newSource;
    }

    /// <summary>
    /// Xử lý logic hạ nhỏ nhạc cũ và tăng dần nhạc mới.
    /// </summary>
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

    /// <summary>
    /// Lấy ra bản nhạc nền đang phát hiện tại.
    /// </summary>
    public AudioClip GetCurrentMusicClip()
    {
        return _currentMusicClip;
    }
}