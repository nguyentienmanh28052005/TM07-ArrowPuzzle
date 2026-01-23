using UnityEngine;
using System.Collections.Generic;

public class WinEffectManager : MonoBehaviour
{
    [Header("Configuration")]
    public Transform centerPoint;
    public Transform gameContainer;

    [Header("Animation Settings")]
    public Color winColor = Color.green;
    public float waveSpeed = 0.1f;
    public float animationDuration = 0.4f;
    public float scaleMultiplier = 1.5f;

    [ContextMenu("Test Win Effect")]
    public float PlayWinEffect()
    {
        if (gameContainer == null)
        {
            return 0f;
        }

        GridDot[] allDots = gameContainer.GetComponentsInChildren<GridDot>();

        if (allDots.Length == 0) return 0f;

        Vector3 center = (centerPoint != null) ? centerPoint.position : Vector3.zero;
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
}