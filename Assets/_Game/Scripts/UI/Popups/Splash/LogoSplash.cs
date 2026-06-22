using System.Collections;
using System.Collections.Generic;
using mygame.sdk;
using UnityEngine;

public class LogoSplash : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;
    [SerializeField] Vector2 iPhonePos;
    [SerializeField] Vector2 normalPos;

    private void Awake()
    {
        //#if (UNITY_IOS || UNITY_IPHONE)
        //        if (SdkUtil.isiPad())
        //        {
        //            rectTransform.anchoredPosition = normalPos;
        //        }else{
        //            rectTransform.anchoredPosition = iPhonePos;
        //        }
        //#endif
        int ratio = Screen.height / Screen.width;
        if (ratio > 1.85f)
        {
            rectTransform.anchoredPosition = iPhonePos;
        }
        else
        {
            rectTransform.anchoredPosition = normalPos;
        }
    }
}
