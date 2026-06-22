using master;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UniRx;
using Observer = master.Observer;
using UnityEngine.UI;
public class UICropSize : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;
    [SerializeField] RectTransform rectTransformParent;
    public bool height = true;
    public bool width = true;
    public bool checkCropSizeIfMore = true;
    public bool checkCropSizeIfLess = true;

    private void OnEnable()
    {
        Setup();
    }
    bool hasRegisterEvent;
    IDisposable sub;
    private void Start()
    {
        RegisterListener();
    }
    private void OnDestroy()
    {
        RemoveListener();
    }
    private void RegisterListener()
    {
        if (hasRegisterEvent) return;
        hasRegisterEvent = true;
        var ob1 = Observer.GetObservable(ObserverName.screen_resize, 0);
        sub = ob1.Subscribe(x =>
        {
            Setup();
        });
    }
    private void RemoveListener()
    {
        if (!hasRegisterEvent) return;
        hasRegisterEvent = false;
        sub.Dispose();
    }

    public void Setup()
    {
        float scale1 = 0;
        float scale2 = 0;
        bool moreHeight = false;
        bool moreWidth = false;
        var w = Screen.width;
        var h = Screen.height;
        if (rectTransformParent != null)
        {
            w = (int) rectTransformParent.rect.width;
            h = (int) rectTransformParent.rect.height;
        }
        
        if (width == true)
        {
            scale1 = w / rectTransform.rect.width;
            if (rectTransform.rect.width > w)
            {
                moreWidth = true;
            }
        }

        if (height == true)
        {
            scale2 = h / rectTransform.rect.height;
            if (rectTransform.rect.height > h)
            {
                moreHeight = true;
            }
        }

        if (!checkCropSizeIfMore && moreHeight && moreWidth)
        {
            return;
        }
        if (!checkCropSizeIfLess && moreHeight == false && moreWidth == false)
        {
            rectTransform.localScale = Vector3.one;
            return;
        }

        float scale = Mathf.Max(scale1, scale2);
        if (scale <= 0) return;
        rectTransform.localScale = Vector3.one * scale;
    }
}