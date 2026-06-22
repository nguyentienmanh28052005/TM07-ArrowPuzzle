using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using mygame.sdk;
using UnityEngine;
using UnityEngine.UI;
using time;
public static class UIExtention
{
    public static Vector2 OffsetNewFont = new Vector2(0, 3);
    public static void SetInteractable(this Button bt, bool interactable)
    {
        var graphics = bt.GetComponentsInChildren<MaskableGraphic>();
        bt.interactable = interactable;
        for (int i = 0; i < graphics.Length; i++)
        {
            graphics[i].color = interactable ? bt.colors.normalColor : bt.colors.disabledColor;
        }
    }

    public static int SumRange(this IList<int> collection, int min, int max)
    {
        int num = 0;
        for (var i = min; i <= max && i < collection.Count; i++)
        {
            int obj = collection[i];
            num += obj;
        }

        return num;
    }

    public static int IndexOf<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
    {
        int num = 0;
        foreach (T obj in collection)
        {
            if (predicate(obj))
                return num;
            ++num;
        }
        return -1;
    }
    public static void SetText(this Text text, string key, StateCapText stateCap = StateCapText.None, FormatText stateFormat = FormatText.None, object obFormat = null, bool changeOffset = true)
    {
        if (key == null || key.Length <= 0)
        {
            return;
        }

        try
        {
            var txMulti = text.GetComponent<TextMutil>();
            if (txMulti == null)
            {
                txMulti = text.gameObject.AddComponent<TextMutil>();
            }
            txMulti.key = key;
            txMulti.stateCap = stateCap;
            txMulti.stateFormat = stateFormat;
            if (obFormat != null) txMulti.objectFormat = obFormat.ToString();
            txMulti.Initialized(false, false);
            if (changeOffset) txMulti.setNewPosition(OffsetNewFont);
            string textValue = MutilLanguage.getStringWithKey(key, stateCap, stateFormat, obFormat);
            text.text = ConvertRTL(textValue);
        }
        catch (Exception ex)
        {
            Debug.Log("mysdk: ex=extentions 1:" + ex.ToString());
        }
    }

    public static Tween DOFadeAllShadow(this Text text, float alpha, float duration, float timeDelay = 0, Ease type = Ease.OutQuad, object obj_ID = null, Action onComplete = null)
    {
        Shadow[] betterOutlines = text.GetComponents<Shadow>();
        var shadowAlpha = betterOutlines.Select(t => t.effectColor).ToList();
        Tween tween = text.DOFade(alpha, duration)
            .OnUpdate(() =>
            {
                if (betterOutlines != null && betterOutlines.Length > 0)
                {
                    for (var index = 0; index < betterOutlines.Length; index++)
                    {
                        var shadowColor = betterOutlines[index].effectColor;
                        var value = (text.color.a) * 0.45f;
                        shadowColor.a = Mathf.Clamp(value, 0, 1);
                        betterOutlines[index].effectColor = shadowColor;
                    }
                }
            })
            .SetDelay(timeDelay)
            .SetId(obj_ID)
            .SetEase(type)
            .OnComplete(() =>
            {
                for (var index = 0; index < betterOutlines.Length; index++)
                {
                    betterOutlines[index].effectColor = shadowAlpha[index];
                }
                onComplete?.Invoke();
            });
        return tween;

        float ColorToFloat(Color color)
        {
            return (color.r + color.g + color.b) / 3;
        }
    }


    public static void SetValue(this Text text, string value)
    {
        text.text = value;
        try
        {
            var txMulti = text.GetComponent<TextMutil>();
            if (txMulti == null) txMulti = text.gameObject.AddComponent<TextMutil>();
            txMulti.Initialized(false, false);
            txMulti.setNewPosition(OffsetNewFont);
            
            text.text = ConvertRTL(value);
        }
        catch (Exception ex)
        {
            Debug.Log("mysdk: ex=extentions 2:" + ex.ToString());
        }
    }
    
    public static void SetValue(this Text text, string value, bool changeOffset)
    {
        text.text = value;
        try
        {
            var txMulti = text.GetComponent<TextMutil>();
            if (txMulti == null) txMulti = text.gameObject.AddComponent<TextMutil>();
            txMulti.Initialized(false, false);
            if (changeOffset) txMulti.setNewPosition(OffsetNewFont);
            
            text.text = ConvertRTL(value);
        }
        catch (Exception ex)
        {
            Debug.Log("mysdk: ex=extentions 3:" + ex.ToString());
        }
    }
    
    public static void SetValue(this Text text, object value, bool changeOffset = true)
    {
        try
        {
            if (text == null)
            {
                Debug.Log("mysdk: null text, val=" + value.ToString());
                return;
            }
            
            if (value == null)
            {
                Debug.Log("mysdk: null value");
                return;
            }
            var txMulti = text.GetComponent<TextMutil>();
            if (txMulti == null) txMulti = text.gameObject.AddComponent<TextMutil>();
            txMulti.Initialized(false, false);
            if (changeOffset) txMulti.setNewPosition(OffsetNewFont);
            text.text = ConvertRTL(value.ToString());
        }
        catch (Exception ex)
        {
            Debug.Log("mysdk: ex=extentions 4:" + ex.ToString());
        }
    }
    public static string ConvertRTL(string text)
    {
        string textValue = text;
        if (RTLTextProcessor.IsRTL(textValue))
        {
            textValue = RTLTextProcessor.FixRTLText(textValue);
        }
        return textValue;
    }
    public static void SetText(this Text text, string key, float sizeRate, StateCapText stateCap = StateCapText.None, FormatText stateFormat = FormatText.None, object obFormat = null)
    {
        if (key == null || key.Length <= 0)
        {
            return;
        }
        try
        {
            var txMulti = text.GetComponent<TextMutil>();
            if (txMulti == null)
            {
                txMulti = text.gameObject.AddComponent<TextMutil>();

            }
            txMulti.key = key;
            txMulti.stateCap = stateCap;
            txMulti.stateFormat = stateFormat;
            if (obFormat != null) txMulti.objectFormat = obFormat.ToString();
            txMulti.Initialized(false, false);
            txMulti.Resize(sizeRate);
            txMulti.setNewPosition(OffsetNewFont);

            text.text = ConvertRTL(MutilLanguage.getStringWithKey(key, stateCap, stateFormat, obFormat));
        }
        catch (Exception ex)
        {
            Debug.Log("mysdk: ex=extentions 5:" + ex.ToString());
        }
    }
    public static void SetText(this Text text, string key, float sizeRate, StateCapText stateCap = StateCapText.None, FormatText stateFormat = FormatText.None, params object[] obFormat)
    {
        if (key == null || key.Length <= 0)
        {
            return;
        }

        try
        {
            var txMulti = text.GetComponent<TextMutil>();
            if (txMulti == null)
            {
                txMulti = text.gameObject.AddComponent<TextMutil>();

            }
            txMulti.key = key;
            txMulti.stateCap = stateCap;
            txMulti.stateFormat = stateFormat;
            txMulti.Initialized(false, false);
            txMulti.Resize(sizeRate);
;
            text.text = ConvertRTL(MutilLanguage.getStringWithKey2(key, stateCap, stateFormat, obFormat));
        }
        catch (Exception ex)
        {
            Debug.Log("mysdk: ex=extentions 6:" + ex.ToString());
        }
    }
    public static void SetValue(this Text text, string value, float sizeRate)
    {
        try
        {
            var txMulti = text.GetComponent<TextMutil>();
            if (txMulti == null) txMulti = text.gameObject.AddComponent<TextMutil>();
            txMulti.Initialized(false, false);
            txMulti.Resize(sizeRate);
            txMulti.setNewPosition(OffsetNewFont);
            text.text = ConvertRTL(value);
        }
        catch (Exception ex)
        {
            Debug.Log("mysdk: ex=extentions 7:" + ex.ToString());
        }
    }
    public static void SetValue(this Text text, object value, float sizeRate)
    {
        try
        {
            var txMulti = text.GetComponent<TextMutil>();
            if (txMulti == null) txMulti = text.gameObject.AddComponent<TextMutil>();
            txMulti.Initialized(false, false);
            txMulti.Resize(sizeRate);
            txMulti.setNewPosition(OffsetNewFont);
            text.text = ConvertRTL(value.ToString());
        }
        catch (Exception ex)
        {
            Debug.Log("mysdk: ex=extentions 8:" + ex.ToString());
        }
    }
    public static void SetSize(this Text text, float sizeRate = 1)
    {
        try
        {
            var txMulti = text.GetComponent<TextMutil>();
            if (txMulti == null) txMulti = text.gameObject.AddComponent<TextMutil>();
            txMulti.Initialized(false, false);
            txMulti.Resize(sizeRate);
            txMulti.setNewPosition(OffsetNewFont);
        }
        catch (Exception ex)
        {
            Debug.Log("mysdk: ex=extentions 9:" + ex.ToString());
        }
    }
    /// <summary>
    /// 06D 10H, 10H 15M, 15M 60S
    /// </summary>
    public static void TimeRemain(this Text txtTime, long startTime, long timspan, Action onTimeUp)
    {
        var timeLerp = timspan - (MGTime.GetUtcTime() - startTime);
        var timeLeft = new TimeSpan(timeLerp * 10000);
        if (timeLeft.TotalSeconds > 0)
        {
            if (timeLeft.Days > 0)
            {
                txtTime.SetText("time_remain_x", stateFormat: FormatText.F_String, obFormat: $"{timeLeft.Days:00}D {timeLeft.Hours:00}H");
            }
            else
            {
                txtTime.SetText("time_remain_x", stateFormat: FormatText.F_String, obFormat: $"{timeLeft.Hours:00}H {timeLeft.Minutes:00}M");
            }
        }
        else
        {
            onTimeUp?.Invoke();
        }
    }

    /// <summary>
    /// 06D 10H, 10H 15M, 15M 60S
    /// </summary>
    public static void SetTextTime(this Text txtTime, int secondTime)
    {
        int hour = secondTime / 3600;
        int min = (secondTime % 3600) / 60;
        int sec = secondTime % 60;

        string timeText = (hour > 0)
            ? $"{hour:D2}:{min:D2}:{sec:D2}"
            : $"{min:D2}:{sec:D2}";

        txtTime.text = timeText;
    }
    public static void SetTextTime2(this Text txtTime, int secondTime)
    {
        int hour = secondTime / 3600;
        int min = (secondTime % 3600) / 60;
        int sec = secondTime % 60;

        string timeText = $"{hour:D2}:{min:D2}:{sec:D2}";

        txtTime.text = timeText;
    }
    public static void CountTime(this Text txtTime, long startTime, long timspan, Action onTimeUp)
    {
        var timeLerp = timspan - (MGTime.GetUtcTime() - startTime);
        var timeLeft = new TimeSpan(timeLerp * 10000);
        if (timeLeft.TotalSeconds > 0)
        {
            if (timeLeft.Days > 0)
            {
                if (timeLeft.Hours == 0)
                {
                    txtTime.text = $"{timeLeft.Days:0} day" + (timeLeft.Days > 1 ? "s" : "");
                }
                else
                {
                    txtTime.SetValue($"{timeLeft.Days:0}d {timeLeft.Hours:#00}h");
                }
            }
            else if (timeLeft.Hours > 0)
            {
                txtTime.text = $"{timeLeft.Hours:0}h {timeLeft.Minutes:00}m";
            }
            else
            {
                txtTime.text = $"{timeLeft.Minutes:0}m {timeLeft.Seconds:00}s";
            }
        }
        else
        {
            onTimeUp?.Invoke();
        }
    }
    public static string CountTime(long startTime, long timspan)
    {
        string res = "";
        var timeLerp = timspan - (MGTime.GetUtcTime() - startTime);
        var timeLeft = new TimeSpan(timeLerp * 10000);
        if (timeLeft.TotalSeconds > 0)
        {
            if (timeLeft.Days > 0)
            {
                if (timeLeft.Hours == 0)
                {
                    res = $"{timeLeft.Days:0} day" + (timeLeft.Days > 1 ? "s" : "");
                }
                else
                {
                    res= $"{timeLeft.Days:0}d {timeLeft.Hours:#00}h";
                }
            }
            else if (timeLeft.Hours > 0)
            {
                res = $"{timeLeft.Hours:0}h {timeLeft.Minutes:00}m";
            }
            else
            {
                res = $"{timeLeft.Minutes:0}m {timeLeft.Seconds:00}s";
            }
        }

        return res;
    }
    public static void CountTime(this Text txtTime, DateTime startTime, DateTime endTime, Action onTimeUp)
    {

        TimeSpan timeLeft = endTime - startTime;

        if (timeLeft.TotalSeconds > 0)
        {
            if (timeLeft.Days > 0)
            {
                if (timeLeft.Hours == 0)
                {
                    txtTime.text = $"{timeLeft.Days:0} day" + (timeLeft.Days > 1 ? "s" : "");
                }
                else
                {
                    txtTime.text = $"{timeLeft.Days:0}d {timeLeft.Hours:00}h";
                }
            }
            else if (timeLeft.Hours > 0)
            {
                txtTime.text = $"{timeLeft.Hours:0}h {timeLeft.Minutes:00}m";
            }
            else
            {
                txtTime.text = $"{timeLeft.Minutes:0}m {timeLeft.Seconds:00}s";
            }
        }
        else
        {
            onTimeUp?.Invoke();
        }

    }
    /// <summary>
    /// DD HH MM SS
    /// </summary>
    public static void CountTime2(this Text txtTime, long startTime, long timspan, Action onTimeUp)
    {
        var timeLerp = timspan - (MGTime.GetUtcTime() - startTime);
        var timeLeft = new TimeSpan(timeLerp * 10000);
        txtTime.text = $"{timeLeft.Days:#0}d {timeLeft.Hours:00}h {timeLeft.Minutes:00}m {timeLeft.Seconds:00}s";
        if (timeLeft.TotalSeconds <= 0)
        {
            onTimeUp?.Invoke();
        }
    }
    public static void CountTime3(this Text txtTime, long startTime, long timspan, Action onTimeUp)
    {
        var timeLerp = timspan - (MGTime.GetUtcTime() - startTime);
        var timeLeft = new TimeSpan(timeLerp * 10000);
        if (timeLeft.TotalSeconds > 0)
        {
            txtTime.text = timeLeft.ToString("hh\\:mm\\:ss");
        }
        else
        {
            onTimeUp?.Invoke();
        }
    }
    public static void CountTimeLeft(this Text txtTime, long startTime, long timspan, Action onTimeUp)
    {
        var timeLerp = timspan - (MGTime.GetUtcTime() - startTime);
        var timeLeft = new TimeSpan(timeLerp * 10000);
        txtTime.SetText("time_left_x", 1f, StateCapText.None, FormatText.F_String, timeLeft.ToString(@"dd\:hh\:mm\:ss"));
        if (timeLeft.TotalSeconds <= 0)
        {
            onTimeUp?.Invoke();
        }

    }
    public static float GetTextWidth(Text text, string content)
    {
        TextGenerator textGen = new TextGenerator();
        TextGenerationSettings generationSettings = text.GetGenerationSettings(text.rectTransform.rect.size);
        float width = textGen.GetPreferredWidth(content, generationSettings) / text.pixelsPerUnit;
        return width;
    }
    private static readonly TextGenerator SharedTextGen = new();

    public static void AdjustContentTextInLine(this Text text, float maxWidth, string content, bool useTextMutil = true)
    {
        if (useTextMutil)
        {
            var txMulti = text.GetComponent<TextMutil>() ?? text.gameObject.AddComponent<TextMutil>();
            txMulti.Initialized(false, false);
        }

        if (string.IsNullOrEmpty(content))
        {
            text.text = "";
            return;
        }

        var settings = text.GetGenerationSettings(new Vector2(maxWidth, float.MaxValue));
        float ellipsisWidth = SharedTextGen.GetPreferredWidth("...", settings) / text.pixelsPerUnit;
        float contentWidth = SharedTextGen.GetPreferredWidth(content, settings) / text.pixelsPerUnit;

        if (contentWidth <= maxWidth)
        {
            text.text = content;
            return;
        }

        // Binary search để tìm vị trí cắt
        int left = 0;
        int right = content.Length - 1;
        int fitIndex = 0;

        while (left <= right)
        {
            int mid = (left + right) / 2;
            string substr = content.Substring(0, mid + 1);
            float width = SharedTextGen.GetPreferredWidth(substr, settings) / text.pixelsPerUnit;

            if (width + ellipsisWidth <= maxWidth)
            {
                fitIndex = mid + 1;
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        text.text = content.Substring(0, fitIndex) + "...";
    }
}
