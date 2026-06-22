using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;
using UnityEngine.Events;

public class SpineNormalEventTrigger : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation skeletonGraphic;
    Spine.AnimationState animationState;
    public SpineNormalEventHolder[] SpineNormalEventHolder;

    private void Awake()
    {
        skeletonGraphic = GetComponent<SkeletonAnimation>();
        animationState = skeletonGraphic.AnimationState;
        animationState.Event += OnUserDefinedEvent;
    }

    public void OnUserDefinedEvent(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        for (int i = 0; i < SpineNormalEventHolder.Length; i++)
        {
            if (e.Data.Name == SpineNormalEventHolder[i].eventName)
            {
                SpineNormalEventHolder[i].unityAction.Invoke();
            }
        }
    }
}

[System.Serializable]
public class SpineNormalEventHolder 
{
    [SpineEvent] public string eventName;
    public UnityEvent unityAction;
}
