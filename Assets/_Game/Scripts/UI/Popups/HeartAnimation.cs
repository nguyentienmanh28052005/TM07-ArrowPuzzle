using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartAnimation : MonoBehaviour
{
    [SerializeField] private SkeletonGraphic heartAnim;
    [SerializeField, SpineAnimation] private string heartAnimIdle;
    [SerializeField, SpineAnimation] private string heartAnimBreak;
    [SerializeField, SpineAnimation] private string heartAnimBreakIdle;

    void OnEnable()
    {
        heartAnim.AnimationState.SetAnimation(0, heartAnimBreak, false);
        heartAnim.AnimationState.AddAnimation(0, heartAnimBreakIdle, true, 0);
    }
}
