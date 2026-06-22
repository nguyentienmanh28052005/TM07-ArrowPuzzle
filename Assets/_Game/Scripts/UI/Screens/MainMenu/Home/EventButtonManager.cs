using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventButtonManager
{
    public static List<IEventButton> listEventButton = new List<IEventButton>();

    public static void AddEventButton(IEventButton eventButton)
    {
        if (!listEventButton.Contains(eventButton))
        {
            listEventButton.Add(eventButton);
        }
    }

    public static bool RemoveEventButton(IEventButton eventButton)
    {
        return listEventButton.Remove(eventButton);
    }

    public static Sequence sequence;

    public static void PlayVisualFlyAnim(Action onComplete)
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        ClearNullCached();
        sequence = DOTween.Sequence();
        if (listEventButton.Count >= 2)
        {
            listEventButton.Sort((x, y) =>
            {
                Vector2 scrPosX = x.GetScreenPosition();
                Vector2 scrPosY = y.GetScreenPosition();
                int compareX = scrPosX.x.CompareTo(scrPosY.x);
                if (compareX != 0)
                    return compareX;
                return scrPosY.y.CompareTo(scrPosX.y);
            });
        }

        for (int i = 0; i < listEventButton.Count; i++)
        {
            IEventButton eventButton = listEventButton[i];
            if (eventButton.CanStartFlyJump())
            {
                sequence.AppendCallback(() =>
                {
                    if (eventButton != null)
                    {
                        eventButton.StartFlyJump();
                    }
                });
                sequence.AppendInterval(0.6f);
            }
        }
        sequence.AppendCallback(() => { onComplete?.Invoke(); });
        sequence.AppendInterval(2f);
        sequence.AppendCallback(PlayAnimActive);
    }

    static Tween tweenAnimIcon;

    public static void PlayAnimActive()
    {
        if (tweenAnimIcon != null)
        {
            tweenAnimIcon.Kill();
        }
        ClearNullCached();
        var seq = DOTween.Sequence();
        tweenAnimIcon = seq;
        if (listEventButton.Count >= 2)
        {
            listEventButton.Sort((x, y) =>
            {
                Vector2 scrPosX = x.GetScreenPosition();
                Vector2 scrPosY = y.GetScreenPosition();
                int compareX = scrPosX.x.CompareTo(scrPosY.x);
                if (compareX != 0)
                    return compareX;
                return scrPosY.y.CompareTo(scrPosX.y);
            });
        }

        for (int i = 0; i < listEventButton.Count; i++)
        {
            IEventButton eventButton = listEventButton[i];

            seq.AppendCallback(() =>
            {
                if (eventButton != null)
                {
                    eventButton.Animate();
                }
            });
            seq.AppendInterval(4f);
        }

        seq.AppendInterval(7f);
        seq.AppendCallback(PlayAnimActive);
        seq.Play();
    }

    public static void StopAnimActive()
    {
        if (tweenAnimIcon != null)
        {
            tweenAnimIcon.Kill();
        }

        if (sequence != null)
        {
            sequence.Kill();
        }
    }
    public static void ClearNullCached()
    {
        for (int i = listEventButton.Count - 1; i >= 0; i--)
        {
            var x = listEventButton[i];
            if ((UnityEngine.Object)x == null)
            {
                listEventButton.RemoveAt(i);
            }
        }
    }
}