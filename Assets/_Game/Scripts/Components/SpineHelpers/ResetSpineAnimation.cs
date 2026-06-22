using System.Collections; // Thêm thư viện này để dùng Coroutine
using UnityEngine;
using Spine.Unity;

public class ResetSpineAnimation : MonoBehaviour
{
    [SerializeField] private SkeletonGraphic spineGraphic;

    [SpineAnimation] public string poseAnimationName;

    [SpineAnimation] public string animationName;

    [SerializeField] private bool isLooping = false;

    [SerializeField] private float delayTime = 1f;

    private void Awake()
    {
        if (spineGraphic == null)
        {
            spineGraphic = GetComponent<SkeletonGraphic>();
        }
    }

    private void OnEnable()
    {
        StartCoroutine(PlayAnimationWithDelay());
    }

    private IEnumerator PlayAnimationWithDelay()
    {
        if (spineGraphic != null && spineGraphic.IsValid)
        {
            spineGraphic.AnimationState.ClearTracks();
            spineGraphic.Skeleton.SetToSetupPose();

            if (!string.IsNullOrEmpty(poseAnimationName))
            {
                spineGraphic.AnimationState.SetAnimation(0, poseAnimationName, true);
            }

            yield return new WaitForSeconds(delayTime);

            if (!string.IsNullOrEmpty(animationName))
            {
                spineGraphic.AnimationState.SetAnimation(0, animationName, isLooping);
            }
        }
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
}