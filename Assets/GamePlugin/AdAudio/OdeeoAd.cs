using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_ODEEO
using Odeeo;
using Odeeo.Data;
#endif


public class OdeeoAd : BaseAdAudio
{
#if ENABLE_ODEEO
#endif
    bool isCreateAd = false;
    int stateShowAd = 0;
    bool isAdOk = false;
    long tShowAds = 0;
    bool isInit = false;
    string memiABTCv2String = "";

    public override void onAwake()
    {
        typeAdAudio = AdAudioType.Odeeo;
    }

    public override void onStart()
    {
        initAd();
    }

    private void OnDestroy()
    {

    }
    //================
    void initAd()
    {
#if ENABLE_ODEEO
        isInit = false;
        isCreateAd = false;
        OdeeoSdk.OnInitializationSucceed += OnInitializationSuccess;
        OdeeoSdk.OnInitializationFailed += OnInitializationFailed;
        OdeeoSdk.Initialize(AdIdsConfig.OdeeoAppkey);
        Debug.Log("mysdk: adau Odeeo init");
#endif
    }

    void OnInitializationSuccess()
    {
        Debug.Log("mysdk: adau Odeeo OnInitializationSuccess");
        isInit = true;
        if (memiABTCv2String != null && memiABTCv2String.Length > 0)
        {
#if ENABLE_ODEEO
            OdeeoSdk.SetDoNotSellPrivacyString(memiABTCv2String);
#endif
        }
    }
    void OnInitializationFailed(int errorParam, string error)
    {
        Debug.Log("mysdk: adau Odeeo OnInitializationFailed=" + errorParam + ", msg=" + error);
        isInit = false;
    }
    private void SubscribePlacement(string placementId)
    {
#if ENABLE_ODEEO
        //Common callbacks
        if (OdeeoAdManager.AudioCallbacks(placementId) != null)
        {
            OdeeoAdManager.AudioCallbacks(placementId).OnAvailabilityChanged += AdOnAvailabilityChanged;
            OdeeoAdManager.AudioCallbacks(placementId).OnShow += AdOnShow;
            OdeeoAdManager.AudioCallbacks(placementId).OnShowFailed += AdOnShowFailed;
            OdeeoAdManager.AudioCallbacks(placementId).OnClose += AdOnClose;
            OdeeoAdManager.AudioCallbacks(placementId).OnClick += AdOnClick;
            OdeeoAdManager.AudioCallbacks(placementId).OnPause += AdOnPause;
            OdeeoAdManager.AudioCallbacks(placementId).OnResume += AdOnResume;

            //If rewarded ad type, rewarded callbacks
            //OdeeoAdManager.AdUnitCallbacks(placementId).OnReward += AdOnReward;
            //OdeeoAdManager.AdUnitCallbacks(placementId).OnRewardedPopupAppear += AdOnRewardedPopupAppear;
            //OdeeoAdManager.AdUnitCallbacks(placementId).OnRewardedPopupClosed += AdOnRewardedPopupClosed;

            //If Impression turned on
            OdeeoAdManager.AudioCallbacks(placementId).OnImpression += AdOnImpression;
        }
#endif
    }

    //================

    public override void onShowCmpNative()
    {

    }

    public override void onCMPOK(string iABTCv2String)
    {
#if ENABLE_ODEEO
        if (isInit)
        {
            OdeeoSdk.SetDoNotSellPrivacyString(iABTCv2String);
            memiABTCv2String = "";
        }
        else
        {
            memiABTCv2String = iABTCv2String;
        }
#endif
    }

    private void OnApplicationPause(bool pauseStatus)
    {
#if ENABLE_ODEEO
        if (OdeeoSdk.onApplicationPause != null)
        {
            OdeeoSdk.onApplicationPause(pauseStatus);
        }
#endif
    }

    public override void onUpdate()
    {
#if ENABLE_ODEEO
        if (stateShowAd == 2)
        {

        }
#endif
    }

    public override BaseAdAudio show(Canvas cv, RectTransform rectTf)
    {
        Debug.Log("mysdk: adau Odeeo show1");
#if ENABLE_ODEEO
        this.canvas = cv;
        this.rect = rectTf;
        if (isInit)
        {
            if (canvas != null && rect != null)
            {
                if (!isCreateAd)
                {
                    isCreateAd = true;
                    OdeeoAdManager.CreateAudioIconAd(AdIdsConfig.OdeeoPlacementId);
                    OdeeoAdManager.LinkIconToRectTransform(AdIdsConfig.OdeeoPlacementId, OdeeoSdk.IconPosition.Centered, rectTf, canvas);
                    OdeeoAdManager.SetProgressBar(AdIdsConfig.OdeeoPlacementId, Color.white);
                    OdeeoAdManager.SetIconActionButtonPosition(AdIdsConfig.OdeeoPlacementId, OdeeoAdUnit.ActionButtonPosition.TopLeft);
                    stateShowAd = 1;
                    SubscribePlacement(AdIdsConfig.OdeeoPlacementId);
                    Debug.Log("mysdk: adau Odeeo show create1");
                }
                else
                {
                    OdeeoAdManager.LinkIconToRectTransform(AdIdsConfig.OdeeoPlacementId, OdeeoSdk.IconPosition.Centered, rectTf, canvas);
                    if (isAdOk)
                    {
                        Debug.Log("mysdk: adau Odeeo show show");
                        stateShowAd = 2;
                        OdeeoAdManager.ShowAd(AdIdsConfig.OdeeoPlacementId);
                        tShowAds = mygame.sdk.SdkUtil.CurrentTimeMilis();
                    }
                    else
                    {
                        Debug.Log("mysdk: adau Odeeo show wait");
                        stateShowAd = 1;
                    }
                }
                gameObject.SetActive(true);
            }
            return this;
        }
        else
        {
            initAd();
            return null;
        }
#else
        return null;
#endif
    }

    public override BaseAdAudio show(int xOffset, int yOffset, int size)
    {
        Debug.Log("mysdk: adau Odeeo show2");
#if ENABLE_ODEEO
        if (isInit)
        {
            if (!isCreateAd)
            {
                isCreateAd = true;
                OdeeoAdManager.CreateAudioIconAd(AdIdsConfig.OdeeoPlacementId);
                OdeeoAdManager.SetIconPosition(AdIdsConfig.OdeeoPlacementId, OdeeoSdk.IconPosition.Centered, xOffset, yOffset);
                OdeeoAdManager.SetProgressBar(AdIdsConfig.OdeeoPlacementId, Color.white);
                OdeeoAdManager.SetIconActionButtonPosition(AdIdsConfig.OdeeoPlacementId, OdeeoAdUnit.ActionButtonPosition.TopLeft);
                stateShowAd = 1;
                SubscribePlacement(AdIdsConfig.OdeeoPlacementId);
                Debug.Log("mysdk: adau Odeeo show create2");
            }
            else
            {
                OdeeoAdManager.SetIconPosition(AdIdsConfig.OdeeoPlacementId, OdeeoSdk.IconPosition.Centered, xOffset, yOffset);
                if (isAdOk)
                {
                    Debug.Log("mysdk: adau Odeeo show show");
                    stateShowAd = 2;
                    OdeeoAdManager.ShowAd(AdIdsConfig.OdeeoPlacementId);
                    tShowAds = mygame.sdk.SdkUtil.CurrentTimeMilis();
                }
                else
                {
                    Debug.Log("mysdk: adau Odeeo show wait");
                    stateShowAd = 1;
                }
            }
            gameObject.SetActive(true);
            return this;
        }
        else
        {
            initAd();
            return null;
        }
#else
        return null;
#endif
    }

    public override BaseAdAudio updatePos(Canvas cv, RectTransform rectTf)
    {
#if ENABLE_ODEEO
        if (canvas != null && rect != null && isInit)
        {
            if (isCreateAd)
            {
                this.canvas = cv;
                this.rect = rectTf;
                OdeeoAdManager.LinkIconToRectTransform(AdIdsConfig.OdeeoPlacementId, OdeeoSdk.IconPosition.Centered, rectTf, canvas);
            }
        }
#endif
        return this;
    }

    private void _show()
    {

    }

    public override void hideAds()
    {
        gameObject.SetActive(false);
        stateShowAd = 0;
#if ENABLE_ODEEO
        if (isCreateAd)
        {
        }
#endif
    }

    public override void close()
    {
#if ENABLE_ODEEO
        if (isInit)
        {
            OdeeoAdManager.RemoveAd(AdIdsConfig.OdeeoPlacementId);
            isCreateAd = false;
        }
#endif
    }

    //========================================================
#if ENABLE_ODEEO
    private void AdOnAvailabilityChanged(bool flag, OdeeoAdData data)
    {
        Debug.Log("mysdk: adau Odeeo AdOnAvailabilityChanged isAvailable=" + flag);
        if (flag)
        {
            if (stateShowAd == 1)
            {
                Debug.Log("mysdk: adau Odeeo AdOnAvailabilityChanged show");
                stateShowAd = 2;
                OdeeoAdManager.ShowAd(AdIdsConfig.OdeeoPlacementId);
                tShowAds = mygame.sdk.SdkUtil.CurrentTimeMilis();
                isAdOk = false;
            }
            else
            {
                Debug.Log("mysdk: adau Odeeo AdOnAvailabilityChanged change flag isok stateAdAvaiable=" + stateShowAd);
                isAdOk = true;
            }
        }
        else
        {
            isAdOk = false;
        }
    }
    private void AdOnShow()
    {
        Debug.Log("mysdk: adau Odeeo AdOnShow");
        AdAudioHelper.Instance.onShowAd(this);
    }
    private void AdOnShowFailed(string placementId, OdeeoAdUnit.ErrorShowReason reason, string description)
    {
        Debug.Log("mysdk: adau Odeeo AdOnShowFailed=" + reason.ToString() + ", des=" + description);
    }
    private void AdOnClick()
    {
        Debug.Log("mysdk: adau Odeeo AdOnClick");
    }
    private void AdOnReward(float amount)
    {
        Debug.Log("mysdk: adau Odeeo AdOnReward= " + amount);
    }
    private void AdOnRewardedPopupAppear()
    {

    }
    private void AdOnRewardedPopupClosed(OdeeoAdUnit.CloseReason reason)
    {

    }
    public void AdOnImpression(OdeeoImpressionData data)
    {
        Debug.Log("mysdk: adau Odeeo AdOnImpression AdOnImpression callback ->\n"
            + " SessionID: " + data.GetSessionID() + "\n"
            + " PlacementType: " + data.GetPlacementType() + "\n"
            + " PlacementID: " + data.GetPlacementID() + "\n"
            + " Country: " + data.GetCountry() + "\n"
            + " PayableAmount: " + data.GetPayableAmount() + "\n"
            + " TransactionID: " + data.GetTransactionID() + "\n"
            + " CustomTag: " + data.GetCustomTag()
        );

    }
    private void AdOnPause(OdeeoAdUnit.StateChangeReason reason)
    {

    }
    private void AdOnResume(OdeeoAdUnit.StateChangeReason reason)
    {

    }
    private void AdOnMute()
    {

    }
    private void AdOnClose(OdeeoAdUnit.CloseReason reason)
    {
        Debug.Log("mysdk: adau Odeeo AdOnClose=" + reason.ToString());
        if (stateShowAd == 2)
        {
            stateShowAd = 0;
        }
        AdAudioHelper.Instance.onCloseAd(this);
    }
#endif
}
