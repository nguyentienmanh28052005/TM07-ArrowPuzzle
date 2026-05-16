using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Solo.MOST_IN_ONE;

public class SpinManager : MonoBehaviour
{
    public static SpinManager Instance;

    [Header("Spin Settings")]
    [Min(1)] public int arrowsPerSpin = 5;
    public float sparkFlightDuration = 0.2f;
    public Ease sparkFlightEase = Ease.InOutSine;
    public float delayBetweenReleases = 0.05f;

    [Header("Spinning Top Effects")]
    [Tooltip("Seconds per full 360-degree turn. Smaller values spin faster.")]
    public float spinSpeed = 0.15f;
    [Tooltip("Controls how wide the curved flight path is.")]
    public float curvePower = 2.5f;
    public float arrivalPunchScale = 0.4f;

    [Header("Explosion")]
    public GameObject explosionPrefab;
    public float explosionDuration = 0.25f;
    public float explosionScale = 2.2f;
    public int explosionVibrato = 18;

    [Header("Visual Effects")]
    public GameObject dashSparkPrefab;
    public Transform sparkOrigin;

    private Coroutine _spinRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TriggerSpin()
    {
        if (CameraController.IsGameplayBlocking || Time.timeScale == 0f) return;
        if (!HasLaunchableArrow()) return;

        if (CurrencyManager.Instance != null && !CurrencyManager.Instance.SpendDashTool(1))
            return;

        ExecuteSpin();
    }

    public void ExecuteSpin()
    {
        if (CameraController.IsGameplayBlocking || Time.timeScale == 0f) return;
        if (_spinRoutine != null) return;

        _spinRoutine = StartCoroutine(SpinRoutine());
    }

    public void TriggerDash()
    {
        TriggerSpin();
    }

    public void ExecuteDash(ArrowDir targetDir)
    {
        ExecuteSpin();
    }

    public void DashUp() { ExecuteSpin(); }
    public void DashDown() { ExecuteSpin(); }
    public void DashLeft() { ExecuteSpin(); }
    public void DashRight() { ExecuteSpin(); }

    private IEnumerator SpinRoutine()
    {
        CameraController.IsGameplayBlocking = true;

        List<SnakeBlock> targets = PickRandomTargets(arrowsPerSpin);
        if (targets.Count == 0)
        {
            CameraController.IsGameplayBlocking = false;
            _spinRoutine = null;
            yield break;
        }

        if (SettingManager.Instance != null)
            SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.RigidImpact);

        Vector3 startPos = sparkOrigin != null ? sparkOrigin.position : Vector3.zero;
        GameObject spark = null;
        TrailRenderer[] sparkTrails = null;

        if (dashSparkPrefab != null)
        {
            spark = Instantiate(dashSparkPrefab, startPos, Quaternion.identity);
            sparkTrails = spark.GetComponentsInChildren<TrailRenderer>(true);
            SetTrailDrawing(sparkTrails, false, true);

            StartSpinLoop(spark);

            spark.transform.localScale = Vector3.zero;
            Tween spawnTween = spark.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetLink(spark);
            yield return spawnTween.WaitForCompletion();
        }

        bool curveToLeft = true;
        int remainingReleaseTargets = CountLaunchableTargets(targets);
        int releasedTargets = 0;

        foreach (SnakeBlock targetSnake in targets)
        {
            if (!IsLaunchable(targetSnake))
            {
                remainingReleaseTargets--;
                if (releasedTargets >= remainingReleaseTargets)
                    SetTrailDrawing(sparkTrails, false, false);
                continue;
            }

            Vector3 targetPos = targetSnake.HeadPosition;

            if (spark != null)
                yield return MoveSparkToTarget(spark, targetPos, curveToLeft);
            else
                yield return new WaitForSeconds(sparkFlightDuration);

            curveToLeft = !curveToLeft;

            if (!IsLaunchable(targetSnake))
            {
                remainingReleaseTargets--;
                if (releasedTargets >= remainingReleaseTargets)
                    SetTrailDrawing(sparkTrails, false, false);
                continue;
            }

            releasedTargets++;
            if (releasedTargets == 1)
                SetTrailDrawing(sparkTrails, true, true);

            targetSnake.ForceDashExit();

            if (spark != null && arrivalPunchScale > 0f)
                spark.transform.DOPunchScale(Vector3.one * arrivalPunchScale, 0.1f, 1).SetLink(spark);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowTap, 0.8f);

            if (releasedTargets >= remainingReleaseTargets)
                SetTrailDrawing(sparkTrails, false, false);

            if (delayBetweenReleases > 0f)
                yield return new WaitForSeconds(delayBetweenReleases);
        }

        SetTrailDrawing(sparkTrails, false, false);

        if (spark != null)
            yield return ExplodeAndDestroy(spark);

        yield return new WaitForSeconds(0.2f);
        CameraController.IsGameplayBlocking = false;
        _spinRoutine = null;
    }

    private bool HasLaunchableArrow()
    {
        SnakeBlock[] allSnakes = FindObjectsOfType<SnakeBlock>();
        foreach (SnakeBlock snake in allSnakes)
        {
            if (IsLaunchable(snake))
                return true;
        }

        return false;
    }

    private int CountLaunchableTargets(List<SnakeBlock> targets)
    {
        int count = 0;
        foreach (SnakeBlock target in targets)
        {
            if (IsLaunchable(target))
                count++;
        }

        return count;
    }

    private List<SnakeBlock> PickRandomTargets(int maxTargets)
    {
        SnakeBlock[] allSnakes = FindObjectsOfType<SnakeBlock>();
        List<SnakeBlock> targets = new List<SnakeBlock>();

        foreach (SnakeBlock snake in allSnakes)
        {
            if (IsLaunchable(snake))
                targets.Add(snake);
        }

        for (int i = 0; i < targets.Count; i++)
        {
            int randomIndex = Random.Range(i, targets.Count);
            SnakeBlock temp = targets[i];
            targets[i] = targets[randomIndex];
            targets[randomIndex] = temp;
        }

        if (targets.Count > maxTargets)
            targets.RemoveRange(maxTargets, targets.Count - maxTargets);

        return targets;
    }

    private bool IsLaunchable(SnakeBlock snake)
    {
        return snake != null
            && snake.gameObject.activeInHierarchy
            && !snake.IsMoving
            && snake.LogicNodes != null
            && snake.LogicNodes.Count > 0;
    }

    private void StartSpinLoop(GameObject spark)
    {
        spark.transform.DORotate(new Vector3(0f, 0f, -360f), spinSpeed, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetRelative(true)
            .SetEase(Ease.Linear)
            .SetLink(spark);
    }

    private void SetTrailDrawing(TrailRenderer[] trails, bool isDrawing, bool clear)
    {
        if (trails == null) return;

        foreach (TrailRenderer trail in trails)
        {
            if (trail == null) continue;

            trail.emitting = isDrawing;
            if (clear)
                trail.Clear();
        }
    }

    private IEnumerator MoveSparkToTarget(GameObject spark, Vector3 targetPos, bool curveToLeft)
    {
        bool hasReached = false;
        Vector3 currentPos = spark.transform.position;
        Vector3 directionToTarget = targetPos - currentPos;
        Vector3 midPoint = currentPos + directionToTarget * 0.5f;
        Vector3 perpendicularOffset = new Vector3(-directionToTarget.y, directionToTarget.x, 0f).normalized;

        if (!curveToLeft)
            perpendicularOffset *= -1f;

        midPoint += perpendicularOffset * curvePower;

        Vector3[] pathPoints = new Vector3[] { currentPos, midPoint, targetPos };
        spark.transform.DOPath(pathPoints, sparkFlightDuration, PathType.CatmullRom)
            .SetEase(sparkFlightEase)
            .OnComplete(() => hasReached = true)
            .SetLink(spark);

        while (spark != null && !hasReached)
            yield return null;
    }

    private IEnumerator ExplodeAndDestroy(GameObject spark)
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, spark.transform.position, Quaternion.identity);
            Destroy(explosion, Mathf.Max(0.1f, explosionDuration * 2f));
        }

        spark.transform.DOKill();

        bool completed = false;
        Sequence explosionSeq = DOTween.Sequence();
        explosionSeq.Append(
            spark.transform.DOScale(Vector3.one * explosionScale, explosionDuration * 0.5f)
                .SetEase(Ease.OutBack)
        );
        explosionSeq.Join(
            spark.transform.DOShakeRotation(
                explosionDuration,
                new Vector3(0f, 0f, 90f),
                explosionVibrato,
                90f,
                false
            )
        );
        explosionSeq.Append(
            spark.transform.DOScale(Vector3.zero, explosionDuration * 0.5f)
                .SetEase(Ease.InBack)
        );
        explosionSeq.OnComplete(() =>
        {
            completed = true;
            Destroy(spark);
        });

        while (spark != null && !completed)
            yield return null;
    }
}
