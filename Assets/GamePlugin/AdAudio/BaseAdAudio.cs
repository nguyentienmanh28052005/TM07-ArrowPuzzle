using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AdAudioType
{
    Odeeo = 0,
    Audiomob
}

public abstract class BaseAdAudio : MonoBehaviour
{
    public AdAudioType typeAdAudio;

    protected Canvas canvas;
    protected RectTransform rect;

    public abstract void onAwake();

    public abstract void onStart();

    public abstract void onShowCmpNative();

    public abstract void onCMPOK(string iABTCv2String);

    public abstract void onUpdate();

    public abstract BaseAdAudio show(int xOffset, int yOffset, int size);

    public abstract BaseAdAudio show(Canvas cv, RectTransform rectTf);

    public abstract BaseAdAudio updatePos(Canvas cv, RectTransform rectTf);

    public abstract void hideAds();

    public abstract void close();

    public virtual bool isShow()
    {
        return true;
    }
}
