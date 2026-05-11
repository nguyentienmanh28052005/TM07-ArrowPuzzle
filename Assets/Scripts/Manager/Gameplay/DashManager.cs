using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Solo.MOST_IN_ONE; // Đảm bảo namespace này khớp với enum ArrowDir của bạn

public class DashManager : MonoBehaviour
{
    public static DashManager Instance;

    [Header("Dash Settings")]
    [Tooltip("Thời gian chờ giữa mỗi lần phóng rắn để tạo cảm giác liên hoàn và tránh đụng đuôi nhau")]
    public float delayBetweenLaunches = 0.15f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Gắn hàm này vào 4 nút bấm chọn hướng trên UI của bạn
    /// (Hoặc gọi qua script UI)
    /// </summary>
    public void ExecuteDash(ArrowDir targetDir)
    {
        if (CameraController.IsGameplayBlocking || Time.timeScale == 0f) return;

            MessageManager.Instance.SendMessage(ManhMessageType.OnSelectDashDirection, false);
            if (BoosterTutorialManager.Instance != null)
                BoosterTutorialManager.Instance.NotifyDashDirectionSelected(targetDir);
            StartCoroutine(DashRoutine(targetDir));
    }

    public void TriggerDash()
    {
        if (CameraController.IsGameplayBlocking || Time.timeScale == 0f) return;

        if(CurrencyManager.Instance.SpendDashTool(1))
        {
            MessageManager.Instance.SendMessage(ManhMessageType.OnSelectDashDirection, true);
            if (BoosterTutorialManager.Instance != null)
                BoosterTutorialManager.Instance.NotifyDashTriggered();
        }
    }

    public void DashUp() { ExecuteDash(ArrowDir.Up); }
    public void DashDown() { ExecuteDash(ArrowDir.Down); }
    public void DashLeft() { ExecuteDash(ArrowDir.Left); }
    public void DashRight() { ExecuteDash(ArrowDir.Right); }

    private IEnumerator DashRoutine(ArrowDir targetDir)
    {
        CameraController.IsGameplayBlocking = true; // Khóa màn hình không cho chạm lung tung

        // 1. Thu thập tất cả các con rắn hợp lệ
        SnakeBlock[] allSnakes = FindObjectsOfType<SnakeBlock>();
        List<SnakeBlock> matchingSnakes = new List<SnakeBlock>();

        foreach (var snake in allSnakes)
        {
            if (snake != null && snake.direction == targetDir && !snake.IsMoving)
            {
                matchingSnakes.Add(snake);
            }
        }

        if (matchingSnakes.Count == 0)
        {
            Debug.Log($"<color=yellow>DASH: Không có mũi tên nào hướng {targetDir} trên bàn cờ!</color>");
            CameraController.IsGameplayBlocking = false;
            yield break;
        }

        // 2. THUẬT TOÁN SẮP XẾP (SECRET SAUCE)
        // Tránh tình trạng 2 con cùng hướng đâm vào đít nhau. Phải cho con ở gần lối thoát chạy trước.
        SortSnakesForSafeLaunch(matchingSnakes, targetDir);

        // 3. Hiệu ứng làm chậm thời gian nhẹ trước khi phóng (Game Feel)
        if (SettingManager.Instance != null) SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.RigidImpact);
        yield return new WaitForSeconds(0.2f);

        // 4. Lần lượt phóng các con rắn
        foreach (var snake in matchingSnakes)
        {
            if (snake != null)
            {
                snake.ForceDashExit(); 
                
                if (AudioManager.Instance != null) 
                    AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowTap, 0.8f);

                yield return new WaitForSeconds(delayBetweenLaunches); // Delay liên hoàn
            }
        }

        // 5. Trả lại quyền điều khiển
        // Chờ thêm một chút để con rắn cuối cùng đi qua hết mới mở khóa
        yield return new WaitForSeconds(0.5f);
        CameraController.IsGameplayBlocking = false;
    }

    private void SortSnakesForSafeLaunch(List<SnakeBlock> snakes, ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up:
                // Ưu tiên Y lớn nhất (Nằm cao nhất) bay trước
                snakes.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y)); 
                break;
            case ArrowDir.Down:
                // Ưu tiên Y nhỏ nhất (Nằm thấp nhất) bay trước
                snakes.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y)); 
                break;
            case ArrowDir.Right:
                // Ưu tiên X lớn nhất (Nằm sát biên phải nhất) bay trước
                snakes.Sort((a, b) => b.transform.position.x.CompareTo(a.transform.position.x)); 
                break;
            case ArrowDir.Left:
                // Ưu tiên X nhỏ nhất (Nằm sát biên trái nhất) bay trước
                snakes.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x)); 
                break;
        }
    }
}