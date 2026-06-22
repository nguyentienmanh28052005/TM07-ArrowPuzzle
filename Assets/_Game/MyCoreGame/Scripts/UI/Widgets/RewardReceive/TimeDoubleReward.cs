using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeDoubleReward : MonoBehaviour
{
    [SerializeField] Text txtTime;
    public Action OnDisableAction;
    void OnEnable()
    {
        StartCoroutine(IECountTime());
    }
    IEnumerator IECountTime()
    {
        while (DoubleRewardManager.isActiveTime)
        {
            txtTime.SetTextTime2((int)(DoubleRewardManager.timeDuration / 1000));
            yield return new WaitForSeconds(1);
        }
        gameObject.SetActive(false);
        OnDisableAction?.Invoke();

    }
}
