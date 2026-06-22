using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

[RequireComponent(typeof(SkeletonGraphic))]
public class home_anim_ctrl : MonoBehaviour
{
    [Header("Spine")]
    [SerializeField] private SkeletonGraphic skeletonGraphic;

    [SpineAnimation] public string idleAnim = "idle";
    [SpineAnimation] public string actionAnim = "action";

    [Header("Idle Sub Animations")]
    [SpineAnimation] public string[] idleSubAnims = { "idle_blue", "idle_green", "idle_Yellow" };

    [Header("Action Sub Animations")]
    [SpineAnimation] public string[] actionSubAnims = { "action_blue", "action_green", "action_Yellow" };

    [Header("Settings")]
    [SerializeField] private float idleDuration = 5f;
    [SerializeField] private Vector2 idleSubDelayRange = new Vector2(0.8f, 1.8f);
    [SerializeField] private Vector2 actionSubDelayRange = new Vector2(0.2f, 0.6f);

    [Tooltip("Thời gian chuyển mượt giữa các sub-anim và khi kết thúc để tránh bị giật")]
    [SerializeField] private float mixDuration = 0.2f;

    private Coroutine mainLoopCoroutine;
    private Coroutine subAnimCoroutine; // Dùng chung 1 biến quản lý cho gọn

    private const int BASE_TRACK = 0;
    private const int SUB_TRACK = 1;

    private void Reset()
    {
        skeletonGraphic = GetComponent<SkeletonGraphic>();
    }

    private void Awake()
    {
        if (skeletonGraphic == null)
            skeletonGraphic = GetComponent<SkeletonGraphic>();
    }

    private void OnEnable()
    {
        mainLoopCoroutine = StartCoroutine(MainLoop());
    }

    private void OnDisable()
    {
        if (mainLoopCoroutine != null) StopCoroutine(mainLoopCoroutine);
        if (subAnimCoroutine != null) StopCoroutine(subAnimCoroutine);

        if (skeletonGraphic != null && skeletonGraphic.AnimationState != null)
        {
            skeletonGraphic.AnimationState.ClearTrack(SUB_TRACK);
        }
    }

    private IEnumerator MainLoop()
    {
        while (true)
        {
            //--------------------------------
            // 1. IDLE PHASE
            //--------------------------------
            skeletonGraphic.AnimationState.SetAnimation(BASE_TRACK, idleAnim, true);

            if (subAnimCoroutine != null) StopCoroutine(subAnimCoroutine);
            subAnimCoroutine = StartCoroutine(PlaySubAnimationsLoop(idleSubAnims, idleSubDelayRange));

            yield return new WaitForSeconds(idleDuration);

            //--------------------------------
            // 2. CHUYỂN SANG ACTION PHASE
            //--------------------------------
            if (subAnimCoroutine != null) StopCoroutine(subAnimCoroutine);
            // Thay vì ClearTrack làm giật cơ, ta cho Track 1 empty để nó tự fade out mượt mà
            var emptyTrack = skeletonGraphic.AnimationState.SetEmptyAnimation(SUB_TRACK, mixDuration);

            TrackEntry actionTrack = skeletonGraphic.AnimationState.SetAnimation(BASE_TRACK, actionAnim, false);

            subAnimCoroutine = StartCoroutine(PlaySubAnimationsLoop(actionSubAnims, actionSubDelayRange));

            // Đợi đúng bằng thời gian của Action chính
            yield return new WaitForSeconds(actionTrack.Animation.Duration);

            // Kết thúc Action Phase, dọn dẹp mượt mà Track 1 trước khi quay lại Idle
            if (subAnimCoroutine != null) StopCoroutine(subAnimCoroutine);
            skeletonGraphic.AnimationState.SetEmptyAnimation(SUB_TRACK, mixDuration);
        }
    }

    // Gộp 2 hàm chạy sub-anim cũ thành 1 hàm loop thông minh hơn
    private IEnumerator PlaySubAnimationsLoop(string[] subAnims, Vector2 delayRange)
    {
        if (subAnims == null || subAnims.Length == 0) yield break;

        while (true)
        {
            // Đợi một khoảng delay ngẫu nhiên trước khi chơi sub-anim
            yield return new WaitForSeconds(Random.Range(delayRange.x, delayRange.y));

            string randomAnim = subAnims[Random.Range(0, subAnims.Length)];

            TrackEntry entry = skeletonGraphic.AnimationState.SetAnimation(SUB_TRACK, randomAnim, false);
            entry.Alpha = 1f;
            // Đặt thời gian mix để lúc bắt đầu và kết thúc sub-anim nó hòa trộn mượt vào anim chính
            entry.MixDuration = mixDuration;

            // Đợi cho sub-anim này chạy gần xong (trừ đi thời gian mix) thì mới cho phép loop tiếp theo
            float waitTime = Mathf.Max(0, entry.Animation.Duration - mixDuration);
            yield return new WaitForSeconds(waitTime);

            // Cho Track 1 tự rút lui mượt mà thay vì biến mất đột ngột
            skeletonGraphic.AnimationState.SetEmptyAnimation(SUB_TRACK, mixDuration);
            yield return new WaitForSeconds(mixDuration);
        }
    }
}