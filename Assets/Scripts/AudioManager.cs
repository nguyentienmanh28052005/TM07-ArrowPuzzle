// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Pixelplacement;
// using UnityEngine;
// using UnityEngine.Serialization;
//
// public class AudioManager : Singleton<AudioManager>
// {
//     [SerializeField] private int _poolSize = 10;
//     [SerializeField] private AudioSource _musicSource;
//
//     [Header("Music Clips")] 
//     // public AudioClip _musicMainMenu;
//     // public AudioClip _musicGamePlay;
//     // public AudioClip _musicRound1;
//     // public AudioClip _musicRainRound1;
//
//
//     [Header("SFX Clips")] 
//     // public AudioClip _sfxGameOver;
//     public AudioClip _sfxButtonClick;
//     public AudioClip _sfxTakeItem;
//     public AudioClip _sfxDropItem;
//     public AudioClip _sfxPlasmaGunReload;
//     public AudioClip _sfxPlasmaGunShoot;
//     public AudioClip _sfxPlasmaGunExplosion;
//
//     public AudioClip _sfxShoot;
//     // public AudioClip _sfxEnemyDie;
//     // public AudioClip _sfxCollectCoin;
//     // public AudioClip _sfxHitEnemy;
//     // public AudioClip _sfxPopupShow;
//     
//     
//     
//     private List<AudioSource> _audioSourcePool;
//     private AudioClip _currentMusicClip;
//
//     private float _musicVolume = 1f;
//     private float _sfxVolume = 1f;
//
//     public float MusicVolume
//     {
//         get => _musicVolume;
//         set
//         {
//             _musicVolume = value;
//             _musicSource.volume = value;
//         }
//     }
//
//     public float SFXVolume
//     {
//         get => _sfxVolume;
//         set
//         {
//             _sfxVolume = value;
//             foreach (var source in _audioSourcePool)
//             {
//                 source.volume = value;
//             }
//         }
//     }
//
//     private void Start()
//     {
//         _musicSource.volume = MusicVolume;
//         InitializeAudioSourcePool();
//         
//         // MessageManager.Instance.AddSubcriber(ManhMessageType.OnGameLose, this);
//         // MessageManager.Instance.AddSubcriber(ManhMessageType.OnGameStart, this);
//         // MessageManager.Instance.AddSubcriber(ManhMessageType.OnCollectCoin, this);
//         // MessageManager.Instance.AddSubcriber(ManhMessageType.OnEnemyDie, this);
//         // MessageManager.Instance.AddSubcriber(ManhMessageType.OnHitEnemy, this);
//         MessageManager.Instance.AddSubcriber(ManhMessageType.OnButtonClick, this);
//         MessageManager.Instance.AddSubcriber(ManhMessageType.OnTakeItem, this);
//         MessageManager.Instance.AddSubcriber(ManhMessageType.OnDropItem, this);
//         MessageManager.Instance.AddSubcriber(ManhMessageType.OnShoot, this);
//         MessageManager.Instance.AddSubcriber(ManhMessageType.OnPlasmaGunReloadBullet, this);
//         MessageManager.Instance.AddSubcriber(ManhMessageType.OnShootPlasmaGun, this);
//         MessageManager.Instance.AddSubcriber(ManhMessageType.OnExplosionPlasmaGun, this);
//
//
//     }
//     
//     
//     
//     private void OnDisable()
//     {
//         // MessageManager.Instance.RemoveSubcriber(ManhMessageType.OnGameLose, this);
//         // MessageManager.Instance.RemoveSubcriber(ManhMessageType.OnGameStart, this);
//         // MessageManager.Instance.RemoveSubcriber(ManhMessageType.OnCollectCoin, this);
//         // MessageManager.Instance.RemoveSubcriber(ManhMessageType.OnEnemyDie, this);
//         // MessageManager.Instance.RemoveSubcriber(ManhMessageType.OnHitEnemy, this);
//         MessageManager.Instance.RemoveSubcriber(ManhMessageType.OnButtonClick, this);
//         MessageManager.Instance.RemoveSubcriber(ManhMessageType.OnTakeItem, this);
//         MessageManager.Instance.RemoveSubcriber(ManhMessageType.OnDropItem, this);
//         MessageManager.Instance.RemoveSubcriber(ManhMessageType.OnShoot, this);
//         MessageManager.Instance.RemoveSubcriber(ManhMessageType.OnPlasmaGunReloadBullet, this);
//         MessageManager.Instance.RemoveSubcriber(ManhMessageType.OnShootPlasmaGun, this);
//         MessageManager.Instance.RemoveSubcriber(ManhMessageType.OnExplosionPlasmaGun, this);
//
//         
//     }
//
//     private void InitializeAudioSourcePool()
//     {
//         _audioSourcePool = new List<AudioSource>();
//         for (int i = 0; i < _poolSize; i++)
//         {
//             AudioSource source = gameObject.AddComponent<AudioSource>();
//             source.playOnAwake = false;
//             source.volume = SFXVolume;
//             _audioSourcePool.Add(source);
//         }
//     }
//
//     public void PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f)
//     {
//         AudioSource source = GetAvailableAudioSource();
//         source.volume = volume;
//         source.pitch = pitch;
//         source.PlayOneShot(clip);
//     }
//    
//
//     public void PlayMusic(AudioClip clip, bool isLoop = true)
//     {
//         StartCoroutine(FadeOutAndIn(_musicSource, clip, isLoop));
//     }
//
//     private AudioSource GetAvailableAudioSource()
//     {
//         foreach (var source in _audioSourcePool)
//         {
//             if (!source.isPlaying)
//             {
//                 return source;
//             }
//         }
//         // If no available source, create a new one
//         AudioSource newSource = gameObject.AddComponent<AudioSource>();
//         newSource.playOnAwake = false;
//         newSource.volume = SFXVolume;
//         _audioSourcePool.Add(newSource);
//         return newSource;
//     }
//
//     private IEnumerator FadeOutAndIn(AudioSource audioSource, AudioClip newClip, bool isLoop)
//     {
//         float currentTime = 0;
//         float startVolume = audioSource.volume;
//
//         while (currentTime < 1f)
//         {
//             currentTime += Time.deltaTime;
//             audioSource.volume = Mathf.Lerp(startVolume, 0, currentTime / 1f);
//             yield return null;
//         }
//
//         audioSource.clip = newClip;
//         audioSource.loop = isLoop;
//         audioSource.Play();
//
//         currentTime = 0;
//         while (currentTime < 1f)
//         {
//             currentTime += Time.deltaTime;
//             audioSource.volume = Mathf.Lerp(0, MusicVolume, currentTime / 1f);
//             yield return null;
//         }
//
//         _currentMusicClip = newClip;
//     }
//
//     public AudioClip GetCurrentMusicClip()
//     {
//         return _currentMusicClip;
//     }
//
//     public void Handle(Message message)
//     {
//         // Debug.Log($"AudioManager: Handle message {message.type.ToString()}");
//         switch (message.type)
//         {
//             //MUSIC
//             // case ManhMessageType.OnGameStart:
//             //     Debug.Log("hi");
//             //     PlayMusic(_musicGamePlay, true);
//             //     break;
//             // case ManhMessageType.OnRound1:
//             //     PlayMusic(_musicRainRound1, true);
//             //     break;
//             // //SFX
//             // case ManhMessageType.OnGameLose:
//             //     PlaySfx(_sfxGameOver);
//             //     break;
//             // case ManhMessageType.OnCollectCoin:
//             //     PlaySfx(_sfxCollectCoin);
//             //     break;
//             // case ManhMessageType.OnHitEnemy:
//             //     PlaySfx(_sfxHitEnemy);
//             //     break;
//             // case ManhMessageType.OnEnemyDie:
//             //     PlaySfx(_sfxEnemyDie, 0.4f);
//             //     break;
//             case ManhMessageType.OnButtonClick:
//                 PlaySfx(_sfxButtonClick);
//                 break;
//             case ManhMessageType.OnTakeItem:
//                 PlaySfx(_sfxTakeItem, 0.8f);
//                 break;
//             case ManhMessageType.OnDropItem:
//                 PlaySfx(_sfxDropItem, 1f);
//                 break;
//             case ManhMessageType.OnShoot:
//                 PlaySfx(_sfxShoot, 0.3f, 0.3f);
//                 break;
//             case ManhMessageType.OnPlasmaGunReloadBullet:
//                 PlaySfx(_sfxPlasmaGunReload);
//                 break;
//             case ManhMessageType.OnShootPlasmaGun:
//                 PlaySfx(_sfxPlasmaGunShoot);
//                 break;
//             case ManhMessageType.OnExplosionPlasmaGun:
//                 PlaySfx(_sfxPlasmaGunExplosion);
//                 break;
//         }
//     }
// }