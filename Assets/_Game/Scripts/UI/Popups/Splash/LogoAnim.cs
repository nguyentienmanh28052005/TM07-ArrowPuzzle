using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoAnim : MonoBehaviour
{
    [SerializeField] SkeletonGraphic skeletonGraphic;
    [SerializeField,SpineAnimation] string startAnim;
    [SerializeField, SpineAnimation] string idleAnim;
    private void OnEnable()
    {
        if (skeletonGraphic != null)
        {
            skeletonGraphic.Initialize(false);
            if (skeletonGraphic.IsValid)
            {
                skeletonGraphic.AnimationState.ClearTracks();
                skeletonGraphic.Skeleton.SetToSetupPose();
                skeletonGraphic.AnimationState.SetAnimation(0, startAnim, false);
                skeletonGraphic.AnimationState.AddAnimation(0, idleAnim, true, 0);
                skeletonGraphic.Update(0);
            }
        }
    }
}
