using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public enum ManhMessageType
{
    OnGameStart,
    OnRound1,
    OnGameLose,
    OnGameWin,
    OnButtonClick,
    OnComplete,
    OnUnScrewTool,
}


[RequireComponent(typeof(Initialization))]
public class MessageManager : Singleton<MessageManager>
{
    private Dictionary<ManhMessageType, Action<object>> _eventDictionary = new Dictionary<ManhMessageType, Action<object>>();

    public void AddSubscriber(ManhMessageType type, Action<object> listener)
    {
        if (!_eventDictionary.ContainsKey(type))
        {
            _eventDictionary[type] = null;
        }

        _eventDictionary[type] += listener;
    }

    public void RemoveSubscriber(ManhMessageType type, Action<object> listener)
    {
        if (_eventDictionary.ContainsKey(type))
        {
            _eventDictionary[type] -= listener;

            if (_eventDictionary[type] == null)
            {
                _eventDictionary.Remove(type);
            }
        }
    }

    public void SendMessage(ManhMessageType type, object data = null)
    {
        if (_eventDictionary.TryGetValue(type, out var thisEvent))
        {
            try
            {
                thisEvent?.Invoke(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error handling event {type}: {e.Message}\nStack Trace: {e.StackTrace}");
            }
        }
    }

    public void SendMessageWithDelay(ManhMessageType type, float delay, object data = null)
    {
        StartCoroutine(_DelaySendMessage(type, delay, data));
    }

    private IEnumerator _DelaySendMessage(ManhMessageType type, float delay, object data)
    {
        yield return new WaitForSeconds(delay);
        SendMessage(type, data);
    }
}