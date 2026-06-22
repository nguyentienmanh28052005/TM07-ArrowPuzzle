using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using Newtonsoft.Json;

namespace master
{
    public static class MasterExtention
    {
        #region EventTrigger
        public static void AddEventTriggerListener(this EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> callback)
        {
            var _trigger = trigger.triggers.Find(e => e.eventID == eventType);
            if (_trigger == null)
            {
                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = eventType,
                    callback = new EventTrigger.TriggerEvent()
                };
                entry.callback.AddListener(callback);
                trigger.triggers.Add(entry);
            }
            else
            {
                _trigger.callback.AddListener(callback);
            }
        }
        public static void AddEventTriggerListener(this Transform transform, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> callback)
        {
            EventTrigger trigger = default;
            if (transform.GetComponent<EventTrigger>() == null)
            {
                transform.gameObject.AddComponent<EventTrigger>();
            }
            trigger = transform.GetComponent<EventTrigger>();
            var _trigger = trigger.triggers.Find(e => e.eventID == eventType);
            if (_trigger == null)
            {
                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = eventType,
                    callback = new EventTrigger.TriggerEvent()
                };
                entry.callback.AddListener(callback);
                trigger.triggers.Add(entry);
            }
            else
            {
                _trigger.callback.AddListener(callback);
            }
        }
        public static void RemoveEventTriggerListener(this EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> callback)
        {
            var _trigger = trigger.triggers.Find(e => e.eventID == eventType);
            if (_trigger == null)
                return;
            _trigger.callback.RemoveListener(callback);
        }
        public static void RemoveAllEventTriggerListener(this EventTrigger trigger, EventTriggerType eventType)
        {
            _ = trigger.triggers.RemoveAll(e => e.eventID == eventType);
        }
        #endregion

        #region Clone
        public static T CloneDeep<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null)) return default;

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }

        #endregion

        #region IList
        public static void Shuffle<T>(this IList<T> iList, int startIndex = 0, int endIndex = -1)
        {
            if (iList == null) return;
            if (iList.Count == 0) return;
            if (endIndex < startIndex || endIndex >= iList.Count)
            {
                endIndex = iList.Count - 1;
            }
            for (int i = startIndex; i < endIndex; i++)
            {
                int rnd = UnityEngine.Random.Range(startIndex, endIndex + 1);
                iList.Swap(i, rnd);
            }
        }
        public static void Swap<T>(this IList<T> _swapData, int _firstIndex, int _secondIndex)
        {
            T _t = _swapData[_firstIndex];
            _swapData[_firstIndex] = _swapData[_secondIndex];
            _swapData[_secondIndex] = _t;
        }

        #endregion

        #region Rectranform
        public static void CopyRectTransform(this RectTransform rect, RectTransform from)
        {
            rect.anchorMin = from.anchorMin;
            rect.anchorMax = from.anchorMax;
            rect.anchoredPosition = from.anchoredPosition;
            rect.sizeDelta = from.sizeDelta;
            rect.pivot = from.pivot;
        }
        #endregion
        
        public static void SetPropertyValue<T>(T obj, string propertyName, object value)
        {
            Type type = typeof(T);

            // Ưu tiên tìm property
            PropertyInfo prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                object convertedValue = Convert.ChangeType(value, prop.PropertyType);
                prop.SetValue(obj, convertedValue);
                return;
            }

            // Nếu không có property, thử tìm field
            FieldInfo field = type.GetField(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                object convertedValue = Convert.ChangeType(value, field.FieldType);
                field.SetValue(obj, convertedValue);
            }
        }
    }
}

