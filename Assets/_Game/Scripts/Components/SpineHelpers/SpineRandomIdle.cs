using System.Collections;
using UnityEngine;
using Spine;
using Spine.Unity;

public class SpineRandomIdle : MonoBehaviour
{
    [Header("Spine")]
    [SerializeField] private SkeletonGraphic skeleton;

    [Header("Animation Names")]
    [SpineAnimation] public string idleAnim;
    [SpineAnimation] public string actionAnim;

    [Header("Idle Time")]
    public float idleMin = 3f;
    public float idleMax = 5f;

    public bool playActionFirst = true;
    private bool actionFinished;

    void OnEnable()
    {
        if (skeleton == null)
            skeleton = GetComponent<SkeletonGraphic>();

        skeleton.AnimationState.Complete += OnAnimComplete;

        StartCoroutine(Loop());
    }

    void OnDisable()
    {
        if (skeleton != null)
            skeleton.AnimationState.Complete -= OnAnimComplete;

        StopAllCoroutines();
    }

    IEnumerator Loop()
    {
        // Kiểm tra nếu người dùng muốn chạy Action trước
        if (playActionFirst)
        {
            actionFinished = false;
            skeleton.AnimationState.SetAnimation(0, actionAnim, false);
            yield return new WaitUntil(() => actionFinished);
        }

        // Bắt đầu vòng lặp Idle -> Action
        while (true)
        {
            // 1. Idle
            skeleton.AnimationState.SetAnimation(0, idleAnim, true);

            float idleTime = Random.Range(idleMin, idleMax);
            yield return new WaitForSeconds(idleTime);

            // 2. Action
            actionFinished = false;
            skeleton.AnimationState.SetAnimation(0, actionAnim, false);

            // 3. Chờ action chạy xong
            yield return new WaitUntil(() => actionFinished);
        }
    }

    void OnAnimComplete(TrackEntry entry)
    {
        if (entry.Animation.Name == actionAnim)
        {
            actionFinished = true;
        }
    }
}
