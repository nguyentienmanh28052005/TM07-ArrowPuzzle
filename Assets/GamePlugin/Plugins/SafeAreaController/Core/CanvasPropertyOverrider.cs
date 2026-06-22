using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mygame.plugin.Android;
using mygame.sdk;
using master;
using UniRx;
using Observer = master.Observer;

[RequireComponent(typeof(RectTransform))]
public class CanvasPropertyOverrider : MonoBehaviour
{
    public enum Platform
    {
        None = -1,
        All,
        Android,
        iOS
    }
    
    public int safeAreaCanvas = 0;
    public bool isFlipBanner = false;
    public bool isBannerCanvas = true;
    public Platform platform;

    private Rect offset
    {
        get { return new Rect(0, 0, 0, navigationBarHeight); }
    }

    private static int navigationBarHeight = 0;

    private void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // if (addSoftkeyArea)
            // RunOnAndroidUiThread(GetNavigationBarHeight);
#endif
    }
    bool hasRegisterEvent;
    IDisposable sub;
    private void Start()
    {
        UpdateCanvasProperty();
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
            UpdateCanvasProperty();
        });
    }
    private void RemoveListener()
    {
        if (!hasRegisterEvent) return;
        hasRegisterEvent = false;
        sub.Dispose();
    }
    // Update Method
    public void UpdateCanvasProperty()
    {
        RectTransform myTransform = GetComponent<RectTransform>();
        Rect safeArea = Screen.safeArea;
        Vector2 screen = new Vector2(Screen.width, Screen.height);

        // 1. Setup and apply safe area
        var h = (screen.y - (screen.y - safeArea.yMax)) / screen.y;
        var heightArea = h + (1 - h) / 2.25f;
        
        if (platform == Platform.All || (platform == Platform.Android && Application.platform == RuntimePlatform.Android) || platform == Platform.iOS && Application.platform != RuntimePlatform.Android)
        {
            
            if (safeAreaCanvas == 1)
            {
                myTransform.anchorMax = new Vector2(1, heightArea);
            }
            else if (safeAreaCanvas == -1)
            {
                myTransform.anchorMax = new Vector2(1, 1 / heightArea + .00275f);
            }
            else
            {
                myTransform.anchorMax = new Vector2(1, 1);
            }
        }
        
        if (isBannerCanvas)
        {
            if (!AdsHelper.isRemoveAds(0) && PlayerPrefsUtil.CFEnableBanner)
            {
                if (isFlipBanner)
                {
                    h = (screen.y - SdkUtil.GetSizeBanner()) / screen.y;
                    myTransform.anchorMin = new Vector2(0, 1 - (1 / h + .00275f));
                }
                else
                {
                    h = (screen.y - SdkUtil.GetSizeBanner()) / screen.y;
                    myTransform.anchorMin = new Vector2(0, 1 - h);
                }
            }
            else
            {
                myTransform.anchorMin = new Vector2(0, 0);
            }
        }
    }

    private static void RunOnAndroidUiThread(Action target)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
        using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {

            activity.Call("runOnUiThread", new AndroidJavaRunnable(target));
        }}
#elif UNITY_EDITOR
        target();
#endif
    }

    private static void GetNavigationBarHeight()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
        using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
        using (var window = activity.Call<AndroidJavaObject>("getWindow")) {
        using (var resources = activity.Call<AndroidJavaObject>("getResources")) {
            var resourceId = resources.Call<int>("getIdentifier", "navigation_bar_height", "dimen", "android");
            if (resourceId > 0) {
                navigationBarHeight = resources.Call<int>("getDimensionPixelSize", resourceId);
            }
        }}}}
#elif UNITY_EDITOR
        navigationBarHeight = 0;
#endif
    }

    public void DisableBannerArea()
    {
        RectTransform myTransform = GetComponent<RectTransform>();
        myTransform.anchorMin = new Vector2(0, 0);
    }
}