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
    public void PlayWinEffect()
    {
        if (gameContainer == null)
        {
            Debug.LogError("Chưa gán GameContainer vào WinEffectManager!");
            return;
        }

        GridDot[] allDots = gameContainer.GetComponentsInChildren<GridDot>();

        if (allDots.Length == 0) return;

        Vector3 center = (centerPoint != null) ? centerPoint.position : Vector3.zero;

        foreach (var dot in allDots)
        {
            if (dot == null) continue;

            float distance = Vector3.Distance(dot.transform.position, center);
            float delay = distance * waveSpeed;

            dot.PlayWinAnimation(winColor, delay, scaleMultiplier, animationDuration);
        }
    }
}