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
    [Tooltip("Flight time from the spawn/origin position to the first arrow.")]
    public float firstArrowFlightDuration = 0.35f;
    [Tooltip("Flight time from one released arrow to the next arrow.")]
    public float sparkFlightDuration = 0.2f;
    public Ease sparkFlightEase = Ease.InOutSine;
    public float delayBetweenReleases = 0.05f;

    [Header("Spinning Top Effects")]
    [Tooltip("Seconds per full 360-degree turn. Smaller values spin faster.")]
    public float spinSpeed = 0.15f;
    [Tooltip("Controls how wide the curved flight path is.")]
    public float curvePower = 2.5f;
    public float arrivalPunchScale = 0.4f;

    [Header("Final Directional Flight")]
    public float finalFlightDuration = 0.45f;
    public Ease finalFlightEase = Ease.InOutSine;
    [Tooltip("Runtime direction calculated from the penultimate released arrow to the final released arrow.")]
    public Vector2 finalFlightDirection = Vector2.right;
    public float finalFlightDistance = 3f;
    public float finalFlightJitter = 0.45f;
    [Range(3, 12)] public int finalFlightPointCount = 5;

    [Header("Explosion")]
    public GameObject explosionPrefab;
    public float explosionParticleLifetime = 1.5f;

    [Header("Explosion Camera Shake")]
    public bool shakeCameraOnExplosion = true;
    public float cameraShakeDuration = 0.28f;
    public float cameraShakeStrength = 0.55f;
    public float cameraShakeHitStop = 0.03f;
    public Color cameraShakeFlashColor = new Color(1f, 1f, 1f, 0.22f);

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
        bool shouldCompleteSpinTutorialOnPress = BoosterTutorialManager.Instance != null
            && BoosterTutorialManager.Instance.IsWaitingForSpinButtonPress;

        if (shouldCompleteSpinTutorialOnPress)
            BoosterTutorialManager.Instance.NotifySpinTriggered();

        if (CameraController.IsGameplayBlocking || Time.timeScale == 0f) return;
        if (!HasLaunchableArrow()) return;

        if (CurrencyManager.Instance != null && !CurrencyManager.Instance.SpendSpinTool(1))
            return;

        if (!shouldCompleteSpinTutorialOnPress && BoosterTutorialManager.Instance != null)
            BoosterTutorialManager.Instance.NotifySpinTriggered();

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
        int releasedTargets = 0;
        Vector3 finalDirectionFromPos = startPos;
        Vector3 finalDirectionToPos = startPos;

        foreach (SnakeBlock targetSnake in targets)
        {
            if (!IsLaunchable(targetSnake))
                continue;

            Vector3 targetPos = targetSnake.HeadPosition;

            if (spark != null)
                yield return MoveSparkToTarget(spark, targetPos, curveToLeft, releasedTargets == 0 ? firstArrowFlightDuration : sparkFlightDuration);
            else
                yield return new WaitForSeconds(releasedTargets == 0 ? firstArrowFlightDuration : sparkFlightDuration);

            curveToLeft = !curveToLeft;

            if (!IsLaunchable(targetSnake))
                continue;
            
            bool isFinalRelease = CountLaunchableTargets(targets) <= 1;

            finalDirectionFromPos = releasedTargets > 0 ? finalDirectionToPos : startPos;
            finalDirectionToPos = targetPos;
            releasedTargets++;
            if (releasedTargets == 1)
                SetTrailDrawing(sparkTrails, true, true);

            targetSnake.ForceDashExit();

            if (spark != null && arrivalPunchScale > 0f)
                spark.transform.DOPunchScale(Vector3.one * arrivalPunchScale, 0.1f, 1).SetLink(spark);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowTap, 0.8f);

            if (isFinalRelease)
                SetTrailDrawing(sparkTrails, false, false);

            if (delayBetweenReleases > 0f)
                yield return new WaitForSeconds(delayBetweenReleases);
        }

        SetTrailDrawing(sparkTrails, false, false);

        if (spark != null)
        {
            if (releasedTargets > 0)
                yield return MoveSparkInFinalDirection(spark, GetFinalFlightDirection(finalDirectionFromPos, finalDirectionToPos));

            yield return ExplodeAndDestroy(spark);
        }

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

    private IEnumerator MoveSparkToTarget(GameObject spark, Vector3 targetPos, bool curveToLeft, float duration)
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
        spark.transform.DOPath(pathPoints, Mathf.Max(0.01f, duration), PathType.CatmullRom)
            .SetEase(sparkFlightEase)
            .OnComplete(() => hasReached = true)
            .SetLink(spark);

        while (spark != null && !hasReached)
            yield return null;
    }

    private IEnumerator MoveSparkInFinalDirection(GameObject spark, Vector2 flightDirection)
    {
        if (spark == null || finalFlightDuration <= 0f || finalFlightDistance <= 0f)
            yield break;

        bool hasFinished = false;
        Vector3 startPos = spark.transform.position;
        int pointCount = Mathf.Max(3, finalFlightPointCount);
        Vector3[] pathPoints = new Vector3[pointCount];
        finalFlightDirection = flightDirection.sqrMagnitude > 0.001f ? flightDirection.normalized : Vector2.right;
        Vector2 forward = finalFlightDirection;
        Vector2 perpendicular = new Vector2(-forward.y, forward.x);

        for (int i = 0; i < pointCount; i++)
        {
            float progress = (i + 1f) / pointCount;
            float sideOffset = Random.Range(-finalFlightJitter, finalFlightJitter);
            Vector2 offset = forward * (finalFlightDistance * progress) + perpendicular * sideOffset;
            pathPoints[i] = startPos + new Vector3(offset.x, offset.y, 0f);
        }

        spark.transform.DOPath(pathPoints, finalFlightDuration, PathType.CatmullRom)
            .SetEase(finalFlightEase)
            .OnComplete(() => hasFinished = true)
            .SetLink(spark);

        while (spark != null && !hasFinished)
            yield return null;
    }

    private Vector2 GetFinalFlightDirection(Vector3 fromPos, Vector3 toPos)
    {
        Vector2 direction = new Vector2(toPos.x - fromPos.x, toPos.y - fromPos.y);
        return direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;
    }

    private IEnumerator ExplodeAndDestroy(GameObject spark)
    {
        spark.transform.DOKill();
        PlayExplosionCameraShake();
        HideSparkVisuals(spark);
        PlayExplosionParticles(spark.transform.position);
        Destroy(spark, 0.05f);
        yield break;
    }

    private void HideSparkVisuals(GameObject spark)
    {
        Renderer[] renderers = spark.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
                renderer.enabled = false;
        }
    }

    private void PlayExplosionParticles(Vector3 position)
    {
        if (explosionPrefab == null) return;

        GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.transform.localScale = Vector3.one;

        ParticleSystem[] particleSystems = explosion.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem particleSystem in particleSystems)
        {
            if (particleSystem == null) continue;

            particleSystem.gameObject.SetActive(true);
            particleSystem.Clear(true);
            particleSystem.Play(true);
        }

        Destroy(explosion, Mathf.Max(0.1f, explosionParticleLifetime));
    }

    private void PlayExplosionCameraShake()
    {
        if (!shakeCameraOnExplosion) return;

        ScreenJuiceManager juiceManager = ScreenJuiceManager.Instance;
        if (juiceManager == null) juiceManager = FindObjectOfType<ScreenJuiceManager>();

        if (juiceManager != null)
        {
            juiceManager.PlayCustomJuice(cameraShakeDuration, cameraShakeStrength, cameraShakeHitStop, cameraShakeFlashColor);
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Transform cameraTransform = mainCamera.transform;
        cameraTransform.DOKill();
        Vector3 originalLocalPosition = cameraTransform.localPosition;
        cameraTransform.DOShakePosition(cameraShakeDuration, cameraShakeStrength, 20, 90f, false, true)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (cameraTransform != null)
                    cameraTransform.localPosition = originalLocalPosition;
            });
    }
}
