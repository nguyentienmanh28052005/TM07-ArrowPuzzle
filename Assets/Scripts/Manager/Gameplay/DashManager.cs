using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Solo.MOST_IN_ONE;

public class DashManager : MonoBehaviour
{
    public static DashManager Instance;

    [Header("Dash Settings")]
    public float sparkFlightDuration = 0.2f;
    [Tooltip("Kiểu gia tốc: InOutSine giúp con quay bay chậm lúc đầu, vút nhanh ở giữa và phanh lại lúc chạm đích")]
    public Ease sparkFlightEase = Ease.InOutSine; 
    
    [Header("Spinning Top Effects")]
    [Tooltip("Tốc độ xoay (giây/1 vòng 360 độ). Càng nhỏ xoay càng nhanh.")]
    public float spinSpeed = 0.15f;
    [Tooltip("Độ bẻ cong của đường bay. Số càng lớn vòng cung càng rộng.")]
    public float curvePower = 2.5f;

    [Header("Visual Effects")]
    public GameObject dashSparkPrefab;
    public Transform sparkOrigin; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ExecuteDash(ArrowDir targetDir)
    {
        if (CameraController.IsGameplayBlocking || Time.timeScale == 0f) return;

        MessageManager.Instance.SendMessage(ManhMessageType.OnSelectDashDirection, false);
        StartCoroutine(DashRoutine(targetDir));
    }

    public void TriggerDash()
    {
        if (CameraController.IsGameplayBlocking || Time.timeScale == 0f) return;

        if (CurrencyManager.Instance.SpendDashTool(1))
        {
            MessageManager.Instance.SendMessage(ManhMessageType.OnSelectDashDirection, true);
        }
    }

    public void DashUp() { ExecuteDash(ArrowDir.Up); }
    public void DashDown() { ExecuteDash(ArrowDir.Down); }
    public void DashLeft() { ExecuteDash(ArrowDir.Left); }
    public void DashRight() { ExecuteDash(ArrowDir.Right); }

    private IEnumerator DashRoutine(ArrowDir targetDir)
    {
        CameraController.IsGameplayBlocking = true;

        SnakeBlock[] allSnakes = FindObjectsOfType<SnakeBlock>();
        List<SnakeBlock> matchingSnakes = new List<SnakeBlock>();

        foreach (var snake in allSnakes)
        {
            if (snake != null && snake.direction == targetDir && !snake.IsMoving)
                matchingSnakes.Add(snake);
        }

        if (matchingSnakes.Count == 0)
        {
            CameraController.IsGameplayBlocking = false;
            yield break;
        }

        SortSnakesForSafeLaunch(matchingSnakes, targetDir);

        if (SettingManager.Instance != null) SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.RigidImpact);
        
        Vector3 startPos = sparkOrigin != null ? sparkOrigin.position : Vector3.zero;
        GameObject spark = null;
        
        if (dashSparkPrefab != null)
        {
            spark = Instantiate(dashSparkPrefab, startPos, Quaternion.identity);
            
            // 1. TẠO HIỆU ỨNG XOAY VÔ HẠN (SPIN)
            // Xoay liên tục 360 độ, dùng Linear để vòng quay không bị khựng
            spark.transform.DORotate(new Vector3(0, 0, -360f), spinSpeed, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetRelative(true)
                .SetEase(Ease.Linear)
                .SetLink(spark);

            spark.transform.localScale = Vector3.zero;
            spark.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.2f);
        }

        // Biến này giúp con quay lạng lách: Lần 1 lượn trái, lần 2 lượn phải
        bool toggleCurveSide = true;

        for (int i = 0; i < matchingSnakes.Count; i++)
        {
            SnakeBlock targetSnake = matchingSnakes[i];
            if (targetSnake == null) continue; 

            Vector3 targetPos = targetSnake.HeadPosition;

            if (spark != null)
            {
                bool hasReached = false;
                Vector3 currentPos = spark.transform.position;

                // 2. TOÁN HỌC TẠO ĐƯỜNG CONG (CATMULL-ROM)
                Vector3 directionToTarget = targetPos - currentPos;
                Vector3 midPoint = currentPos + directionToTarget / 2f;
                
                // Tìm vector vuông góc 2D để bẻ cong điểm giữa
                Vector3 perpendicularOffset = new Vector3(-directionToTarget.y, directionToTarget.x, 0).normalized;
                
                // Đảo chiều bẻ cong lượn qua lượn lại
                if (toggleCurveSide) perpendicularOffset *= -1f;
                toggleCurveSide = !toggleCurveSide;

                // Đẩy điểm giữa ra xa để tạo vòng cung
                midPoint += perpendicularOffset * curvePower;

                // Tạo mảng 3 điểm cho DOTween vẽ đường cong
                Vector3[] pathPoints = new Vector3[] { currentPos, midPoint, targetPos };
                
                // 3. DI CHUYỂN PHI TUYẾN TÍNH
                spark.transform.DOPath(pathPoints, sparkFlightDuration, PathType.CatmullRom)
                    .SetEase(sparkFlightEase)
                    .OnComplete(() => hasReached = true)
                    .SetLink(spark);

                yield return new WaitUntil(() => hasReached);
                
                spark.transform.DOPunchScale(Vector3.one * 0.4f, 0.1f, 1);
            }
            else
            {
                yield return new WaitForSeconds(sparkFlightDuration);
            }

            targetSnake.ForceDashExit();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowTap, 0.8f);
        }

        if (spark != null)
        {
            spark.transform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() => Destroy(spark));
        }

        yield return new WaitForSeconds(0.5f);
        CameraController.IsGameplayBlocking = false;
    }

    private void SortSnakesForSafeLaunch(List<SnakeBlock> snakes, ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: snakes.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y)); break;
            case ArrowDir.Down: snakes.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y)); break;
            case ArrowDir.Right: snakes.Sort((a, b) => b.transform.position.x.CompareTo(a.transform.position.x)); break;
            case ArrowDir.Left: snakes.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x)); break;
        }
    }
}