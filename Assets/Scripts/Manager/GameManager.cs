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
    
    // Cờ trạng thái chống xung đột animation
    public bool isGameOver = false; 

    void Awake()
    {
        // Chuyển toàn bộ gánh nặng khởi tạo sang một luồng chạy ngầm (Coroutine)
        // Việc này giúp Main Thread không bị nghẽn, màn hình game không bị giật (freeze)
        StartCoroutine(PrewarmAsyncRoutine());
    }

    private IEnumerator PrewarmAsyncRoutine()
    {
        // ==========================================
        // KHUNG HÌNH 1: Hâm nóng UI & Animation (DOTween)
        // ==========================================
        DOTween.Init(true, true, LogBehaviour.ErrorsOnly).SetCapacity(1000, 200); 
        yield return null; // Nghỉ 1 frame, trả quyền cho CPU vẽ màn hình

        // ==========================================
        // KHUNG HÌNH 2: Thông nòng Job System & Burst Compiler
        // ==========================================
        // Ép Burst Compiler dịch mã C# sang Assembly ngay lúc này thay vì chờ lúc đâm nhau
        NativeArray<int> dummyData = new NativeArray<int>(1, Allocator.TempJob);
        DummyJob dummyJob = new DummyJob { result = dummyData };
        dummyJob.Schedule(1, 1).Complete(); 
        dummyData.Dispose();
        yield return null; // Nghỉ 1 frame

        // ==========================================
        // KHUNG HÌNH 3: Thông nòng cỗ máy Vật lý (Physics2D)
        // ==========================================
        // Bắn một tia Raycast tàng hình cực ngắn để Engine Vật lý xây dựng lưới không gian (Spatial Tree)
        Physics2D.Raycast(Vector2.zero, Vector2.up, 0.1f);
        yield return null; // Nghỉ 1 frame

        // ==========================================
        // KHUNG HÌNH 4: Hâm nóng Audio Engine
        // ==========================================
        // Kích hoạt loa điện thoại với âm lượng 0
        if (AudioManager.Instance != null && AudioManager.Instance.sfxArrowHit != null)
        {
            AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowHit, 0f); 
        }
    }

    void Start()
    {
        // RESET CỜ LẠI TỪ ĐẦU KHI VÀO MÀN MỚI
        isGameOver = false; 

        if((int)SaveDataPlayer.Instance.Value(1) != 0)
        {
            level = (int)SaveDataPlayer.Instance.Value(1);
        }
    }

    void Update()
    {
        
    }

    public LevelDataSO GetCurrentLevelData()
    {
        if (levelDataSOs == null || levelDataSOs.Count == 0) return null;
        if (level < 1 || level >= levelDataSOs.Count + 1) return null;
        return levelDataSOs[level-1];
    }

    // ==========================================
    // JOB GIẢ ĐỂ ĐÁNH THỨC CÁC LÕI CPU PHỤ
    // ==========================================
    [BurstCompile]
    private struct DummyJob : IJobParallelFor
    {
        public NativeArray<int> result;
        public void Execute(int index)
        {
            result[index] = 1 + 1; // Một phép toán siêu nhẹ để mồi hệ thống
        }
    }
}