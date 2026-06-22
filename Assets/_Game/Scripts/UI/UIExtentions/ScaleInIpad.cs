using System.Collections;
using System.Collections.Generic;
using mygame.sdk;
using UnityEngine;
using UnityEngine.Serialization;

public class ScaleInIpad : MonoBehaviour
{
    public float scaleIfIpad = 1;
    public float scaleIfFold = 1;

    void Start()
    {
        if (SdkUtil.isFold())
        {
            transform.localScale *= scaleIfFold;
        }
        else if (SdkUtil.isiPad())
        {
            transform.localScale *= scaleIfIpad;
        }
    }
}