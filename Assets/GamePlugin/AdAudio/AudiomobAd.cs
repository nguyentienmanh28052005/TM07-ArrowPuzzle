using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_Audiomob
using Audiomob;
#endif


public class AudiomobAd : BaseAdAudio
{
    static int CountShow = 0;
    int stateShowAd = 0;
    bool isAdOk = false;
    long tShowAds = 0;
    private bool adRequested;
    private bool adPauseState;
    public Canvas cvCurrAd;
    public RectTransform mediumRectangleBanner;
    [SerializeField]private EventSystem eventSys;

    public override void onAwake()
    {
        typeAdAudio = AdAudioType.Odeeo;
    }

    public override void onStart()
    {
#if ENABLE_Audiomob
        // Assign callbacks to the AudioMob events.
        if (AudiomobPlugin.Instance != null)
        {
            AudiomobPlugin.Instance.OnAdPlaybackStatusChanged += OnAdPlaybackStatusChanged;
            AudiomobPlugin.Instance.OnAdFailed += OnAdFailed;
        }

        CountShow = 0;
#endif
    }
    private void OnDestroy()
    {
#if ENABLE_Audiomob
        if (AudiomobPlugin.Instance != null)
        {
            AudiomobPlugin.Instance.OnAdPlaybackStatusChanged -= OnAdPlaybackStatusChanged;
            AudiomobPlugin.Instance.OnAdFailed -= OnAdFailed;
        }
#endif
    }
    //================
    void OnInitialized()
    {
        //PlayOnSDK Initialized
    }
    void OnInitializationFailed(int errorParam, string error)
    {
        //PlayOnSDK initialization Failed
    }
    //================

    public override void onShowCmpNative()
    {

    }

    public override void onCMPOK(string iABTCv2String)
    {
#if ENABLE_Audiomob

#endif
    }

    private void OnApplicationPause(bool pauseStatus)
    {
#if ENABLE_Audiomob

#endif
    }

    public override void onUpdate()
    {
#if ENABLE_Audiomob
        if (stateShowAd == 2)
        {

        }
#endif
    }

    public override BaseAdAudio show(Canvas cv, RectTransform rectTf)
    {
        Debug.Log("mysdk: adau AudiomobAd show1");
#if ENABLE_Audiomob
        this.canvas = cv;
        this.rect = rectTf;
        if (canvas != null && rect != null)
        {
            gameObject.SetActive(true);
            _show();
        }
        return this;
#else
        return null;
#endif

    }

    public override BaseAdAudio show(int xOffset, int yOffset, int size)
    {
        Debug.Log("mysdk: adau AudiomobAd show2");
#if ENABLE_Audiomob
        gameObject.SetActive(true);
        _showWithPos(xOffset, yOffset, size);
        return this;
#else
        return null;
#endif
    }

    public override BaseAdAudio updatePos(Canvas cv, RectTransform rectTf)
    {
#if ENABLE_Audiomob
        if (cv != null && rectTf != null)
        {
            this.canvas = cv;
            mediumRectangleBanner.anchoredPosition = rectTf.anchoredPosition;
            mediumRectangleBanner.anchoredPosition3D = rectTf.anchoredPosition3D;
            mediumRectangleBanner.anchorMax = rectTf.anchorMax;
            mediumRectangleBanner.anchorMin = rectTf.anchorMin;
            mediumRectangleBanner.offsetMax = rectTf.offsetMax;
            mediumRectangleBanner.offsetMin = rectTf.offsetMin;
            mediumRectangleBanner.sizeDelta = rectTf.sizeDelta;
            if (canvas != null)
            {
                cvCurrAd.GetComponent<CanvasScaler>().referenceResolution = canvas.GetComponent<CanvasScaler>().referenceResolution;
                cvCurrAd.GetComponent<CanvasScaler>().screenMatchMode = canvas.GetComponent<CanvasScaler>().screenMatchMode;
                cvCurrAd.GetComponent<CanvasScaler>().matchWidthOrHeight = canvas.GetComponent<CanvasScaler>().matchWidthOrHeight;
            }
        }
#endif
        return this;
    }

    private void _show()
    {
#if ENABLE_Audiomob

        mediumRectangleBanner.anchoredPosition = rect.anchoredPosition;
        mediumRectangleBanner.anchoredPosition3D = rect.anchoredPosition3D;
        mediumRectangleBanner.anchorMax = rect.anchorMax;
        mediumRectangleBanner.anchorMin = rect.anchorMin;
        mediumRectangleBanner.offsetMax = rect.offsetMax;
        mediumRectangleBanner.offsetMin = rect.offsetMin;
        mediumRectangleBanner.sizeDelta = rect.sizeDelta;
        if (canvas != null)
        {
            cvCurrAd.GetComponent<CanvasScaler>().referenceResolution = canvas.GetComponent<CanvasScaler>().referenceResolution;
            cvCurrAd.GetComponent<CanvasScaler>().screenMatchMode = canvas.GetComponent<CanvasScaler>().screenMatchMode;
            cvCurrAd.GetComponent<CanvasScaler>().matchWidthOrHeight = canvas.GetComponent<CanvasScaler>().matchWidthOrHeight;
        }
        if (!adRequested)
        {
            if (AudiomobPlugin.Instance != null)
            {
                adRequested = true;
                AudiomobPlugin.Instance.PlayAd(AudiomobPlugin.AdUnits.SkippableRectangle);
            }
        }
#endif
    }

    private void _showWithPos(int xOffset, int yOffset, int size)
    {
#if ENABLE_Audiomob
        mediumRectangleBanner.anchoredPosition = rect.anchoredPosition;
        mediumRectangleBanner.anchoredPosition3D = rect.anchoredPosition3D;
        mediumRectangleBanner.anchorMax = rect.anchorMax;
        mediumRectangleBanner.anchorMin = rect.anchorMin;
        mediumRectangleBanner.offsetMax = rect.offsetMax;
        mediumRectangleBanner.offsetMin = rect.offsetMin;
        mediumRectangleBanner.sizeDelta = rect.sizeDelta;
        if (!adRequested)
        {
            if (AudiomobPlugin.Instance != null)
            {
                adRequested = true;
                AudiomobPlugin.Instance.PlayAd(AudiomobPlugin.AdUnits.SkippableRectangle);
            }
        }
#endif
    }

    public override bool isShow()
    {
        if (CountShow >= AdAudioHelper.Instance.CfCountShowAudioMob)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public override void hideAds()
    {
        //gameObject.SetActive(false);
        stateShowAd = 0;
        eventSys.gameObject.SetActive(false);
#if ENABLE_Audiomob
#endif
    }

    public override void close()
    {
#if ENABLE_Audiomob

#endif
    }

    //========================================================
#if ENABLE_Audiomob
    private void OnAdPlaybackStatusChanged(AdSequence adSequence, AdPlaybackStatus adPlaybackStatus)
    {
        switch (adPlaybackStatus)
        {
            case AdPlaybackStatus.Started:
                /* Write code here to:
             - Turn down your game volume.
             - Turn off your game music.
             - Give your players an instant reward? */
                adRequested = false;
                CountShow++;
                AdAudioHelper.Instance.onShowAd(this);
                pauseAudioGame();
                Debug.Log("mysdk: adau AudiomobAd OnAdPlaybackStatusChanged=Started");
                break;
            case AdPlaybackStatus.Finished:
                /* Write code here to:
               - Give your player a reward for listening to the ad? */
                adRequested = false;
                resumeAudioGame();
                Debug.Log("mysdk: adau AudiomobAd OnAdPlaybackStatusChanged=Finished");
                break;
            case AdPlaybackStatus.Canceled:
                adRequested = false;
                resumeAudioGame();
                AdAudioHelper.Instance.onCloseAd(this);
                Debug.Log("mysdk: adau AudiomobAd OnAdPlaybackStatusChanged=Canceled");
                break;
            case AdPlaybackStatus.Skipped:
                adRequested = false;
                resumeAudioGame();
                AdAudioHelper.Instance.onCloseAd(this);
                Debug.Log("mysdk: adau AudiomobAd OnAdPlaybackStatusChanged=Skipped");
                break;
            case AdPlaybackStatus.Stopped:
                adRequested = false;
                resumeAudioGame();
                AdAudioHelper.Instance.onCloseAd(this);
                Debug.Log("mysdk: adau AudiomobAd OnAdPlaybackStatusChanged=Stopped");
                break;
            default:
                Debug.Log("mysdk: adau AudiomobAd OnAdPlaybackStatusChanged=" + adPlaybackStatus);
                break;
        }
    }

    private void OnAdFailed(string adUnitId, AdFailureReason adFailureReason)
    {
        adRequested = false;
        switch (adFailureReason)
        {
            case AdFailureReason.RequestFailed:
                Debug.Log("mysdk: adau AudiomobAd OnAdFailed=RequestFailed");
                break;
            case AdFailureReason.PlaybackFailed:
                Debug.Log("mysdk: adau AudiomobAd OnAdFailed=PlaybackFailed");
                break;
            default:
                Debug.Log("mysdk: adau AudiomobAd OnAdFailed=" + adFailureReason);
                break;
        }
    }

    private void pauseAudioGame()
    {
        //AudioListener.volume = 0;
        //AdAudioHelper.Instance.onShowAd(this);
        //if (EventSystem.current == null)
        //{
        //    eventSys.gameObject.SetActive(true);
        //}
        //else
        //{
        //    Debug.Log("mysdk: adau AudiomobAd pauseAudioGame=" + EventSystem.current.name);
        //    eventSys.gameObject.SetActive(false);
        //}
        Debug.Log("mysdk: adau AudiomobAd pauseAudioGame");
    }

    private void resumeAudioGame()
    {
        //AudioListener.volume = 1;
        //AdAudioHelper.Instance.onCloseAd(this);
        //eventSys.gameObject.SetActive(false);
        //mygame.sdk.AdsProcessCB.Instance().Enqueue(() => {
        //    AudioListener.volume = 1;
        //}, 0.05f);
        Debug.Log("mysdk: adau AudiomobAd resumeAudioGame");
    }

#endif
}
