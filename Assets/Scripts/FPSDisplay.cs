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

    private void Awake()
    {
        frameDeltaTimeArray = new float[50];
    }

    private void Update()
    {
        frameDeltaTimeArray[lastFrameIndex] = Time.unscaledDeltaTime;
        lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;
        _Text.text = Mathf.RoundToInt(CalculateFPS()).ToString();
    }

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
