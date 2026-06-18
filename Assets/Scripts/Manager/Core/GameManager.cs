using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

public class GameManager : Singleton<GameManager>, IScreenLifecycle
{
    public List<LevelDataV2> levelDataV2s;
    public int level = 1;
    public int currentMaxLevel = 3;
    public bool isGameOver = false; 

    public LevelDataV2 CurrentLevelData => GetCurrentLevelData();

    private bool _hasInitialized = false;

    /// <summary>
    /// Đẩy quá trình khởi tạo các thư viện nặng sang luồng bất đồng bộ để chống đứng hình.
    /// </summary>
    void Awake()
    {
        StartCoroutine(PrewarmAsyncRoutine());

        QualitySettings.vSyncCount = 0; 

        Application.targetFrameRate = 60;
    }


    /// <summary>
    /// Coroutine chịu trách nhiệm mồi (Warm-up) Engine Vật Lý, Burst Compiler và hệ thống Audio.
    /// </summary>
    private IEnumerator PrewarmAsyncRoutine()
    {
        yield return null;

        DOTween.Init(true, true, LogBehaviour.ErrorsOnly).SetCapacity(1000, 200); 
        yield return null; 

        NativeArray<int> dummyData = new NativeArray<int>(1, Allocator.TempJob);
        DummyJob dummyJob = new DummyJob { result = dummyData };
        dummyJob.Schedule(1, 1).Complete(); 
        dummyData.Dispose();
        yield return null; 

        Physics2D.Raycast(Vector2.zero, Vector2.up, 0.1f);
        yield return null; 

        if (AudioManager.Instance != null && AudioManager.Instance.sfxArrowHit != null)
        {
            AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowHit, 0f); 
        }
    }

    private void OnEnable()
    {
        ScreenManager.ScreenShown += HandleScreenShown;
    }

    private void OnDisable()
    {
        ScreenManager.ScreenShown -= HandleScreenShown;
    }

    private void HandleScreenShown(ScreenType type)
    {
        if (type == ScreenType.Gameplay)
        {
            OnScreenShow();
        }
    }

    /// <summary>
    /// Khôi phục trạng thái Level từ hệ thống lưu trữ.
    /// </summary>
    public void OnScreenShow()
    {
        isGameOver = false; 

        if (!_hasInitialized)
        {
            if ((int)SaveDataPlayer.Instance.Value(1) != 0)
            {
                level = (int)SaveDataPlayer.Instance.Value(1);
            }

            _hasInitialized = true;
        }
    }

    public void OnScreenHide()
    {
    }

    public void NextLevel()
    {
        ChangeLevelAndReload(1);
    }

    public void PreviousLevel()
    {
        ChangeLevelAndReload(-1);
    }

    private void ChangeLevelAndReload(int levelOffset)
    {
        if (PlaytestSession.IsActive) return;
        if (TransitionManager.Instance != null && TransitionManager.Instance.IsTransitioning) return;

        int maxLevel = GetMaxPlayableLevel();
        int targetLevel = Mathf.Clamp(level + levelOffset, 1, maxLevel);
        if (targetLevel == level) return;

        level = targetLevel;
        isGameOver = false;

        if (SaveDataPlayer.Instance != null)
        {
            SaveDataPlayer.Instance.Save(1, level);
            SaveDataPlayer.Instance.ClearBoardState();
        }

        ReloadGameplayScreen();
    }

    private int GetMaxPlayableLevel()
    {
        int dataCount = levelDataV2s != null ? levelDataV2s.Count : 0;
        int configuredMaxLevel = currentMaxLevel > 0 ? currentMaxLevel : dataCount;

        if (dataCount > 0 && configuredMaxLevel > 0)
        {
            return Mathf.Max(1, Mathf.Min(configuredMaxLevel, dataCount));
        }

        return Mathf.Max(1, configuredMaxLevel);
    }

    private void ReloadGameplayScreen()
    {
        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllSfx();
        }

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TransitionToScreen(ScreenType.Gameplay, true);
            return;
        }

        if (ScreenManager.Instance != null)
        {
            ScreenManager.Instance.ShowScreen(ScreenType.Gameplay, true);
        }
    }

    void Update()
    {
        
    }

    /// <summary>
    /// Trích xuất dữ liệu bản đồ cấu hình cho Level hiện tại.
    /// </summary>
    public LevelDataV2 GetCurrentLevelData()
    {
        if (levelDataV2s == null || levelDataV2s.Count == 0) return null;
        if (level < 1 || level >= levelDataV2s.Count + 1) return null;
        return levelDataV2s[level-1];
    }

    /// <summary>
    /// Struct Dummy nhằm đánh thức các lõi CPU xử lý đa luồng (Burst/Job) ngay từ đầu game.
    /// </summary>
    [BurstCompile]
    private struct DummyJob : IJobParallelFor
    {
        public NativeArray<int> result;
        public void Execute(int index)
        {
            result[index] = 1 + 1; 
        }
    }
}
