using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICountdownTimer : MonoBehaviour
{
    [SerializeField] protected Text txtTimer;
    [SerializeField] protected string formatHours = "{0:D2}:{1:D2}:{2:D2}";
    [SerializeField] protected string formatMinutes = "{0:D2}:{1:D2}";
    [SerializeField] protected bool hideOnEnd = false;

    private Func<long> _getRemainingMillis;
    private Action _onTimerEnd;
    private bool _isCompleted = false;

    public void Init(Func<long> getRemainingMillis, Action onTimerEnd = null)
    {
        _getRemainingMillis = getRemainingMillis;
        _onTimerEnd = onTimerEnd;
        _isCompleted = false;
        UpdateDisplay();
    }

    private void Update()
    {
        if (_getRemainingMillis == null || _isCompleted) return;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (txtTimer == null) return;

        long remainingMillis = _getRemainingMillis();
        if (remainingMillis <= 0)
        {
            txtTimer.text = "00:00";
            _isCompleted = true;
            _onTimerEnd?.Invoke();
            if (hideOnEnd) gameObject.SetActive(false);
            return;
        }

        TimeSpan t = TimeSpan.FromMilliseconds(remainingMillis);
        if (t.TotalHours >= 1)
        {
            txtTimer.text = string.Format(formatHours, (int)t.TotalHours, t.Minutes, t.Seconds);
        }
        else
        {
            txtTimer.text = string.Format(formatMinutes, t.Minutes, t.Seconds);
        }
    }
}
