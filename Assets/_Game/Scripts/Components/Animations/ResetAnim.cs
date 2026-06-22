using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class ResetAnim : MonoBehaviour
{
    private SkeletonGraphic skeletonGraphic;

    void Start()
    {
        skeletonGraphic = GetComponent<SkeletonGraphic>();
    }

    void OnEnable()
    {
        if (skeletonGraphic == null) return;

        // Force update skeleton
        skeletonGraphic.Initialize(true);

        // Lấy animation hiện tại trong track 0
        var state = skeletonGraphic.AnimationState;
        var current = state.GetCurrent(0);

        if (current != null)
        {
            // Restart animation
            state.SetAnimation(0, current.Animation, current.Loop);
        }
    }
}
