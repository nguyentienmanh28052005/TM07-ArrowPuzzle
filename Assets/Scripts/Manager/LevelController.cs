using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField] private int countArrowInGame;

    void Awake()
    {
        if (GameManager.Instance != null && GameManager.Instance.levelDataSOs != null)
        {
            countArrowInGame = GameManager.Instance.levelDataSOs[GameManager.Instance.level - 1].snakes.Count;
        }
    }

    public void SetCountArrowInGame()
    {
        countArrowInGame--;
        if (countArrowInGame <= 0)
        {
            Debug.Log("Level Complete");

            LevelDataSO currentLevelData = null;
            bool isFullCombo = false;

            if (GameManager.Instance != null && GameManager.Instance.levelDataSOs != null)
            {
                currentLevelData = GameManager.Instance.levelDataSOs[GameManager.Instance.level - 1];
                
                // ==========================================
                // KIỂM TRA ĐIỀU KIỆN FULL COMBO
                // ==========================================
                if (ComboManager.Instance != null)
                {
                    // Nếu số combo hiện tại >= tổng số mũi tên ban đầu -> Người chơi đã đánh 1 mạch không lỗi
                    isFullCombo = (ComboManager.Instance.currentCombo >= currentLevelData.snakes.Count);
                }

                if (CurrencyManager.Instance != null)
                {
                    // Tiền xu luôn được nhận
                    CurrencyManager.Instance.AddCoins(currentLevelData.rewardCoins);
                    
                    // LỆNH GIỚI NGHIÊM: Kim cương chỉ được nhận khi Full Combo
                    if (isFullCombo)
                    {
                        CurrencyManager.Instance.AddDiamonds(currentLevelData.rewardDiamonds);
                    }
                }
            }

            if (GameManager.Instance != null && GameManager.Instance.level < GameManager.Instance.currentMaxLevel)
            {
                GameManager.Instance.level++;
                SaveDataPlayer.Instance.Save(1, GameManager.Instance.level);
            }

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

            StartCoroutine(SequenceWinGame(effectDuration, currentLevelData, isFullCombo));
        }
    }

    // Đã thêm cờ isFullCombo vào Coroutine
    public IEnumerator SequenceWinGame(float waitTime, LevelDataSO completedLevelData, bool isFullCombo)
    {
        yield return new WaitForSeconds(waitTime);

        GameCanvas canvas = FindObjectOfType<GameCanvas>();
        if (canvas != null && completedLevelData != null)
        {
            // Đóng gói Data SO và Cờ Full Combo vào một mảng object để gửi đi chung 1 chuyến xe
            object[] rewardData = new object[] { completedLevelData, isFullCombo };
            MessageManager.Instance.SendMessage(ManhMessageType.OnComplete, rewardData);
        }
    }
}