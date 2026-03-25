using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour
{
    private int lastFrameIndex;
    private float[] frameDeltaTimeArray;
    public TextMeshProUGUI _Text;

    /// <summary>
    /// Khởi tạo mảng lưu trữ thời gian của 50 khung hình gần nhất.
    /// </summary>
    private void Awake()
    {
        frameDeltaTimeArray = new float[50];
    }

    /// <summary>
    /// Ghi nhận thời gian khung hình và cập nhật UI liên tục.
    /// </summary>
    private void Update()
    {
        frameDeltaTimeArray[lastFrameIndex] = Time.unscaledDeltaTime;
        lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;
        _Text.text = Mathf.RoundToInt(CalculateFPS()).ToString();
    }

    /// <summary>
    /// Tính toán chỉ số FPS trung bình dựa trên mảng lịch sử khung hình.
    /// </summary>
    private float CalculateFPS()
    {
        float total = 0f;
        foreach (var deltaTime in frameDeltaTimeArray)
        {
            total += deltaTime;
        }

        return frameDeltaTimeArray.Length / total;
    }
}