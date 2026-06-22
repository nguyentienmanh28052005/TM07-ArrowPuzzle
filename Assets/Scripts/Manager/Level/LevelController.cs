using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour, IScreenLifecycle
{
    [Header("Music")]
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField, Range(0f, 1f)] private float gameplayMusicVolume = 1f;
    [SerializeField] private bool loopGameplayMusic = true;
    [SerializeField] private bool fadeOutMusicOnEnd = true;
    [SerializeField] private bool waitForTransitionBeforeMusic = true;

    [SerializeField] private int countArrowInGame;
    private bool _isLevelComplete;
    private Coroutine _gameplayMusicRoutine;

    public void OnScreenShow()
    {
        _isLevelComplete = false;
        ScheduleGameplayMusic();

        if (PlaytestSession.IsPlaytesting)
        {
            countArrowInGame = LevelDataV2Queries.GetArrowCount(PlaytestSession.LevelData);
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.levelDataV2s != null)
        {
            countArrowInGame = LevelDataV2Queries.GetArrowCount(GameManager.Instance.levelDataV2s[GameManager.Instance.level - 1]);
        }
    }

    public void OnScreenHide()
    {
        CancelGameplayMusicRoutine();
        StopGameplayMusic(fadeOutMusicOnEnd);
        countArrowInGame = 0;
        _isLevelComplete = false;
    }

    public void SetCountArrowInGame()
    {
        if (_isLevelComplete) return;
        countArrowInGame--;
        if (GridManager.Instance != null) GridManager.Instance.RaiseArrowExited();
        if (countArrowInGame <= 0)
        {
            _isLevelComplete = true;
            GameplayInputLock.SetLock(GameplayLockReason.GameOverSequence, true);
            StopGameplayMusic(fadeOutMusicOnEnd);

            if (TimeAttackManager.Instance != null)
            {
                TimeAttackManager.Instance.StopTimer();
            }

            Debug.Log("Level Complete");

            LevelDataV2 currentLevelData = null;
            bool isFullCombo = false;

            

            CameraController cam = FindObjectOfType<CameraController>();
            if (cam != null)
            {
                cam.ZoomToEndGame();
            }

            float effectDuration = 0f;
            WinEffectManager winEffect = FindObjectOfType<WinEffectManager>();
            if (winEffect != null)
            {
                effectDuration = winEffect.PlayWinEffect();
            }

            float portalVanishDuration = GridPortalVisual.PlayEndGameVanishAll();
            float deflectorVanishDuration = GridDeflectorVisual.PlayEndGameVanishAll();
            effectDuration = Mathf.Max(effectDuration, portalVanishDuration, deflectorVanishDuration);

            StartCoroutine(SequenceWinGame(effectDuration, currentLevelData, isFullCombo));
        }
    }

    // Đã thêm cờ isFullCombo vào Coroutine
    public IEnumerator SequenceWinGame(float waitTime, LevelDataV2 completedLevelData, bool isFullCombo)
    {
        yield return new WaitForSeconds(waitTime);

        if (PlaytestSession.IsPlaytesting)
        {
            completedLevelData = PlaytestSession.LevelData;
            if (completedLevelData != null && ComboManager.Instance != null)
            {
                isFullCombo = (ComboManager.Instance.currentCombo == LevelDataV2Queries.GetArrowCount(completedLevelData));
            }

            if (completedLevelData != null && MessageManager.Instance != null)
            {
                object[] rewardData = new object[] { completedLevelData, isFullCombo };
                MessageManager.Instance.SendMessage(ManhMessageType.OnComplete, rewardData);
            }

            yield break;
        }

        if (GameManager.Instance != null && GameManager.Instance.levelDataV2s != null)
            {
                completedLevelData = GameManager.Instance.levelDataV2s[GameManager.Instance.level - 1];
                
                // ==========================================
                // KIỂM TRA ĐIỀU KIỆN FULL COMBO
                // ==========================================
                if (ComboManager.Instance != null)
                {
                    // Nếu số combo hiện tại >= tổng số mũi tên ban đầu -> Người chơi đã đánh 1 mạch không lỗi
                    isFullCombo = (ComboManager.Instance.currentCombo == LevelDataV2Queries.GetArrowCount(completedLevelData));
                }

                if (CurrencyManager.Instance != null)
                {
                    // Tiền xu luôn được nhận
                    CurrencyManager.Instance.AddCoins(completedLevelData.rewardCoins);
                    
                    // LỆNH GIỚI NGHIÊM: Kim cương chỉ được nhận khi Full Combo
                    if (isFullCombo)
                    {
                        CurrencyManager.Instance.AddDiamonds(completedLevelData.rewardDiamonds);
                    }
                }

            }

            if (GameManager.Instance != null)
            {
                int maxLevelCount = GameManager.Instance.levelDataV2s != null ? GameManager.Instance.levelDataV2s.Count : 0;
                int limit = Mathf.Min(GameManager.Instance.currentMaxLevel, maxLevelCount);
                if (GameManager.Instance.level < limit)
                {
                    GameManager.Instance.level++;
                    SaveDataPlayer.Instance.Save(1, GameManager.Instance.level);
                }
            }

        GameCanvas canvas = FindObjectOfType<GameCanvas>();
        if (canvas != null && completedLevelData != null)
        {
            // Đóng gói Data SO và Cờ Full Combo vào một mảng object để gửi đi chung 1 chuyến xe
            object[] rewardData = new object[] { completedLevelData, isFullCombo };
            MessageManager.Instance.SendMessage(ManhMessageType.OnComplete, rewardData);
        }


    }

    private void PlayGameplayMusic()
    {
        if (gameplayMusic == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlayMusic(gameplayMusic, loopGameplayMusic, gameplayMusicVolume);
    }

    private void StopGameplayMusic(bool fadeOut)
    {
        if (AudioManager.Instance == null) return;
        if (gameplayMusic != null && AudioManager.Instance.GetCurrentMusicClip() != gameplayMusic) return;
        AudioManager.Instance.StopMusic(fadeOut);
    }

    private void ScheduleGameplayMusic()
    {
        CancelGameplayMusicRoutine();

        if (gameplayMusic == null || AudioManager.Instance == null) return;
        _gameplayMusicRoutine = StartCoroutine(PlayGameplayMusicWhenReady());
    }

    private IEnumerator PlayGameplayMusicWhenReady()
    {
        if (gameplayMusic != null && gameplayMusic.loadState == AudioDataLoadState.Unloaded)
            gameplayMusic.LoadAudioData();

        yield return null;

        if (waitForTransitionBeforeMusic)
        {
            while (TransitionManager.Instance != null &&
                   (TransitionManager.Instance.IsTransitioning || TransitionManager.Instance.IsHeld))
            {
                yield return null;
            }
        }

        while (gameplayMusic != null && gameplayMusic.loadState == AudioDataLoadState.Loading)
            yield return null;

        if (gameplayMusic == null || gameplayMusic.loadState == AudioDataLoadState.Failed)
        {
            _gameplayMusicRoutine = null;
            yield break;
        }

        PlayGameplayMusic();
        _gameplayMusicRoutine = null;
    }

    private void CancelGameplayMusicRoutine()
    {
        if (_gameplayMusicRoutine == null) return;
        StopCoroutine(_gameplayMusicRoutine);
        _gameplayMusicRoutine = null;
    }
}
