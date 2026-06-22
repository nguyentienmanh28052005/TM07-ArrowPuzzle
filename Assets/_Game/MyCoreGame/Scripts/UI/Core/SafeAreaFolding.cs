using mygame.sdk;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using Observer = master.Observer;
using master;
using DG.Tweening;

public class SafeAreaFolding : Singleton<SafeAreaFolding>
{
    public const float DefaultIpadWidth = 1400f;
    public const float DefaultIpadHeight = 1920f;
    private Vector2 canvasSize;
    private void Start()
    {
        Initialized();
        RegisterListener();
    }
    protected void OnDestroy()
    {
        RemoveListener();
    }
    protected bool hasRegisterEvent;
    private IDisposable sub;
    private void RegisterListener()
    {
        if (hasRegisterEvent) return;
        var obs = Observer.GetObservable(ObserverName.screen_resize, 1);
        sub = obs.Subscribe(onScreenResize);
        hasRegisterEvent = true;
    }
    private void RemoveListener()
    {
        if (!hasRegisterEvent) return;
        hasRegisterEvent = false;
        sub.Dispose();
    }
    void onScreenResize(object v)
    {
        if (!hasRegisterEvent) return;
        var c = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        var scaler = GetComponentInParent<CanvasScaler>();
        if (!c || !scaler) return;
        if (SdkUtil.isiPad())
        {
            scaler.matchWidthOrHeight = 1;
        }
        else
        {
            scaler.matchWidthOrHeight = 0;
        }
        canvasSize = c.sizeDelta;
        //CheckSizeFoldingScreen();

    }
    private void Initialized()
    {
        if (canvasSize.x == 0)
        {
            var c = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            if (c)
            {
                canvasSize = c.sizeDelta;
                //CheckSizeFoldingScreen();
            }
        }
    }
    public static bool IsFoldingScreen()
    {
        float w = Screen.width;
        float h = Screen.height;
        float per = w / h;
        return per >= 0.85f;
    }
    //public void CheckSizeFoldingScreen()
    //{
    //    return;
    //    if (IsFoldingScreen())
    //    {
    //        var w = canvasSize.y / DefaultIpadHeight * DefaultIpadWidth;
    //        var r = (canvasSize.x - w) / (2f * DefaultIpadWidth);
    //        if (r > 0)
    //        {
    //            var v = (canvasSize.x - w) / 2f;
    //            RectTransform rectTransform = GetComponent<RectTransform>();
    //            rectTransform.offsetMin = new Vector2(v, rectTransform.offsetMin.y); // Set Left
    //            rectTransform.offsetMax = new Vector2(-v, rectTransform.offsetMax.y);
    //        }
    //    }
    //    else
    //    {
    //        RectTransform rectTransform = GetComponent<RectTransform>();
    //        rectTransform.offsetMin = new Vector2(0, rectTransform.offsetMin.y); // Set Left
    //        rectTransform.offsetMax = new Vector2(0, rectTransform.offsetMax.y);
    //    }
    //}
    public Vector2 GetScreenSize()
    {
        Initialized();
        return GetComponent<RectTransform>().rect.size;
    }
}

