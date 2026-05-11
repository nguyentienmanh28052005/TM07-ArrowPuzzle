using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour, IScreenLifecycle
{
    [SerializeField] private int countArrowInGame;
    private bool _isLevelComplete;

    public void OnScreenShow()
    {
        _isLevelComplete = false;
        if (PlaytestSession.IsPlaytesting)
        {
            countArrowInGame = PlaytestSession.LevelData.snakes != null ? PlaytestSession.LevelData.snakes.Count : 0;
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.levelDataSOs != null)
        {
            countArrowInGame = GameManager.Instance.levelDataSOs[GameManager.Instance.level - 1].snakes.Count;
        }
    }

    public void OnScreenHide()
    {
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
            Debug.Log("Level Complete");

            LevelDataSO currentLevelData = null;
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
    public IEnumerator SequenceWinGame(float waitTime, LevelDataSO completedLevelData, bool isFullCombo)
    {
        yield return new WaitForSeconds(waitTime);

        if (PlaytestSession.IsPlaytesting)
        {
            completedLevelData = PlaytestSession.LevelData;
            if (completedLevelData != null && ComboManager.Instance != null && completedLevelData.snakes != null)
            {
                isFullCombo = (ComboManager.Instance.currentCombo >= completedLevelData.snakes.Count);
            }

            if (completedLevelData != null && MessageManager.Instance != null)
            {
                object[] rewardData = new object[] { completedLevelData, isFullCombo };
                MessageManager.Instance.SendMessage(ManhMessageType.OnComplete, rewardData);
            }

            yield break;
        }

        if (GameManager.Instance != null && GameManager.Instance.levelDataSOs != null)
            {
                completedLevelData = GameManager.Instance.levelDataSOs[GameManager.Instance.level - 1];
                
                // ==========================================
                // KIỂM TRA ĐIỀU KIỆN FULL COMBO
                // ==========================================
                if (ComboManager.Instance != null)
                {
                    // Nếu số combo hiện tại >= tổng số mũi tên ban đầu -> Người chơi đã đánh 1 mạch không lỗi
                    isFullCombo = (ComboManager.Instance.currentCombo >= completedLevelData.snakes.Count);
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

            if (GameManager.Instance != null && GameManager.Instance.level < GameManager.Instance.currentMaxLevel)
            {
                GameManager.Instance.level++;
                SaveDataPlayer.Instance.Save(1, GameManager.Instance.level);
            }

        GameCanvas canvas = FindObjectOfType<GameCanvas>();
        if (canvas != null && completedLevelData != null)
        {
            // Đóng gói Data SO và Cờ Full Combo vào một mảng object để gửi đi chung 1 chuyến xe
            object[] rewardData = new object[] { completedLevelData, isFullCombo };
            MessageManager.Instance.SendMessage(ManhMessageType.OnComplete, rewardData);
        }


    }
}
