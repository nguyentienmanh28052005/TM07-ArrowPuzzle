using mygame.sdk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionButtonAdsConfig : MonoBehaviour
{
    [SerializeField] Vector3 anchorPosIfConfig; 
    [SerializeField] Vector3 anchorPosIfNotConfig;

    private void OnEnable()
    {
        if (PlayerPrefsUtil.CF_AdsButtonPosition)
        {
            GetComponent<RectTransform>().anchoredPosition = anchorPosIfConfig;
        }
        else
        {
            GetComponent<RectTransform>().anchoredPosition = anchorPosIfNotConfig;
        }
    }
}
