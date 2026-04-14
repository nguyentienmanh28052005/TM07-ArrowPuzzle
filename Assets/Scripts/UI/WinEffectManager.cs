using UnityEngine;
using System.Collections.Generic;

public class WinEffectManager : MonoBehaviour
{
    [Header("Configuration")]
    public Transform gameContainer;

    [Header("Animation Settings")]
    public Color winColor = Color.green;
    public float waveSpeed = 0.1f;
    public float animationDuration = 0.4f;
    public float scaleMultiplier = 1.2f;

    /// <summary>
    /// Kích hoạt chuỗi hiệu ứng gợn sóng chiến thắng quét qua toàn bộ các điểm trên lưới.
    /// Trả về tổng thời gian dài nhất của chuỗi hoạt ảnh để hệ thống chờ đợi.
    /// </summary>
    [ContextMenu("Test Win Effect")]
    public float PlayWinEffect()
    {
        if (gameContainer == null) return 0f;

        GridDot[] allDots = gameContainer.GetComponentsInChildren<GridDot>();

        if (allDots.Length == 0) return 0f;

        Vector3 center = CalculateLevelCenter(allDots);
        float maxDuration = 0f;

        foreach (var dot in allDots)
        {
            if (dot == null) continue;

            float distance = Vector3.Distance(dot.transform.position, center);
            float delay = distance * waveSpeed;

            float totalTimeForThisDot = delay + animationDuration;
            if (totalTimeForThisDot > maxDuration)
            {
                maxDuration = totalTimeForThisDot;
            }

            dot.PlayWinAnimation(winColor, delay, scaleMultiplier, animationDuration);
        }

        return maxDuration;
    }

    private Vector3 CalculateLevelCenter(GridDot[] allDots)
    {
        Bounds bounds = new Bounds(allDots[0].transform.position, Vector3.zero);

        for (int i = 1; i < allDots.Length; i++)
        {
            GridDot dot = allDots[i];
            if (dot == null) continue;
            bounds.Encapsulate(dot.transform.position);
        }

        return bounds.center;
    }
}