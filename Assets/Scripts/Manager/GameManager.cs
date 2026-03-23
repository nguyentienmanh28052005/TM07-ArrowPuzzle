using DG.Tweening;
using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public List<LevelDataSO> levelDataSOs;
    public int level = 1;
    public int currentMaxLevel = 3;

    public bool isGameOver = false;

    void Awake()
    {
        // ==========================================
        // CHIẾN DỊCH PRE-WARM (CHỐNG GIẬT LẦN ĐẦU)
        // ==========================================

        // 1. HÂM NÓNG DOTWEEN: 
        // Ép DOTween khởi tạo engine và cấp phát sẵn 500 ô nhớ cho các mũi tên, 
        // tránh việc nó phải tự xin thêm RAM lúc mũi tên đang chạy.
        DOTween.Init(true, true, LogBehaviour.ErrorsOnly).SetCapacity(500, 100);

        // 2. HÂM NÓNG ÂM THANH:
        // Đánh thức Audio Engine của Unity bằng cách phát một âm thanh với âm lượng 0.
        // Ngăn chặn việc bị khựng lúc 2 mũi tên đâm nhau lần đầu.
        if (AudioManager.Instance != null && AudioManager.Instance.sfxArrowHit != null)
        {
            AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowHit, 0f);
        }

        // 3. HÂM NÓNG HAPTIC (RUNG):
        // Gọi hàm rung cực nhẹ một lần để Android/iOS nạp API rung vào bộ nhớ.
        // Solo.MOST_IN_ONE.MOST_HapticFeedback.Generate(Solo.MOST_IN_ONE.MOST_HapticFeedback.HapticTypes.LightImpact);
    }

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

    public LevelDataSO GetCurrentLevelData()
    {
        if (levelDataSOs == null || levelDataSOs.Count == 0) return null;
        if (level < 1 || level >= levelDataSOs.Count + 1) return null;
        return levelDataSOs[level-1];
    }
}
