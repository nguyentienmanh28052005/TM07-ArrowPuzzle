using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

public class GameManager : Singleton<GameManager>
{
    public List<LevelDataSO> levelDataSOs;
    public int level = 1;
    public int currentMaxLevel = 3;
    public bool isGameOver = false; 

    /// <summary>
    /// Đẩy quá trình khởi tạo các thư viện nặng sang luồng bất đồng bộ để chống đứng hình.
    /// </summary>
    void Awake()
    {
        StartCoroutine(PrewarmAsyncRoutine());
    }

    /// <summary>
    /// Coroutine chịu trách nhiệm mồi (Warm-up) Engine Vật Lý, Burst Compiler và hệ thống Audio.
    /// </summary>
    private IEnumerator PrewarmAsyncRoutine()
    {
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

    /// <summary>
    /// Khôi phục trạng thái Level từ hệ thống lưu trữ.
    /// </summary>
    void Start()
    {
        isGameOver = false; 

        if((int)SaveDataPlayer.Instance.Value(1) != 0)
        {
            level = (int)SaveDataPlayer.Instance.Value(1);
        }
    }

    void Update()
    {
        
    }

    /// <summary>
    /// Trích xuất dữ liệu bản đồ cấu hình cho Level hiện tại.
    /// </summary>
    public LevelDataSO GetCurrentLevelData()
    {
        if (levelDataSOs == null || levelDataSOs.Count == 0) return null;
        if (level < 1 || level >= levelDataSOs.Count + 1) return null;
        return levelDataSOs[level-1];
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