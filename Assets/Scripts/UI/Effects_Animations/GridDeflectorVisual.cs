using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(GridDeflector))]
public class GridDeflectorVisual : MonoBehaviour, IPreviewDisableable
{
    [Header("Spawn Effect")]
    [SerializeField] private float spawnDuration = 1.5f;
    [SerializeField] private Ease spawnScaleEase = Ease.OutBack;
    [SerializeField, Min(0f)] private float spawnSpinTurns = 3f;
    
    // Đã xóa bớt các biến StartTilt, OvershootAngle, ReturnAngle rườm rà
    // Vì DOTween OutBack sẽ tự động nội suy quán tính cực kỳ chuẩn!
    [Tooltip("Độ nảy (quá đà) khi dừng lại. Càng cao giật lại càng mạnh.")]
    [SerializeField] private float overshootPower = 1f; 

    [Header("End Game Vanish")]
    [SerializeField] private float endVanishDuration = 0.28f;
    [SerializeField] private Ease endVanishEase = Ease.InBack;

    [Header("Interaction Redirect")]
    [SerializeField] private float interactionDuration = 0.34f;
    [SerializeField] private float interactionNudgeDistance = 0.24f;
    [SerializeField] private float redirectEchoDistance = 0.4f;
    [SerializeField, Min(0.01f)] private float redirectEchoDuration = 0.5f;
    [SerializeField] private float redirectEchoStartScale = 1.3f;
    [SerializeField] private float redirectEchoEndScale = 1f;
    [SerializeField, Range(0f, 1f)] private float redirectEchoAlpha = 0.65f;
    [SerializeField] private Color redirectEchoColor = new Color(0.1f, 1f, 1f, 1f);
    [SerializeField] private Material redirectEchoMaterial;
    [SerializeField] private Material redirectLineMaterial;
    [SerializeField] private Vector2 interactionEffectCenterOffset;
    [SerializeField] private float shockwaveDuration = 0.32f;
    [SerializeField] private float shockwaveRadius = 1.75f;
    [SerializeField] private float shockwaveLineWidth = 0.09f;
    [SerializeField, Range(0f, 1f)] private float shockwaveAlpha = 0.85f;
    [SerializeField] private int redirectStreakCount = 5;
    [SerializeField] private float redirectStreakDuration = 0.3f;
    [SerializeField] private float redirectStreakLength = 0.75f;
    [SerializeField] private float redirectStreakTravel = 1.15f;
    [SerializeField] private float redirectStreakSpacing = 0.2f;
    [SerializeField] private float redirectStreakLineWidth = 0.09f;
    [SerializeField, Range(0f, 1f)] private float redirectStreakAlpha = 0.95f;

    private static readonly HashSet<GridDeflectorVisual> ActiveDeflectors = new HashSet<GridDeflectorVisual>();
    private static readonly Dictionary<Vector2Int, GridDeflectorVisual> DeflectorByCell = new Dictionary<Vector2Int, GridDeflectorVisual>();
    private static Material _defaultEffectMaterial;

    [SerializeField] private SpriteRenderer targetRenderer;

    private Vector3 _baseScale;
    private Vector3 _baseLocalPosition;
    private Color _baseColor = Color.white;
    private float _baseAlpha = 1f;
    private float _baseRotationZ;
    private Vector2Int _cell;
    private bool _isVanishing;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        _baseScale = transform.localScale;
        _baseLocalPosition = transform.localPosition;
        _baseRotationZ = NormalizeAngle(transform.eulerAngles.z);
        
        if (targetRenderer != null)
        {
            _baseColor = targetRenderer.color;
            _baseAlpha = targetRenderer.color.a;
        }
    }

    private void OnEnable()
    {
        ActiveDeflectors.Add(this);
        _baseLocalPosition = transform.localPosition;
        RegisterCell();
        PlaySpawnEffect();
    }

    private void OnDisable()
    {
        ActiveDeflectors.Remove(this);
        if (DeflectorByCell.TryGetValue(_cell, out GridDeflectorVisual current) && current == this)
        {
            DeflectorByCell.Remove(_cell);
        }
        transform.DOKill();
        if (targetRenderer != null)
        {
            targetRenderer.DOKill();
        }
    }

    private void RegisterCell()
    {
        _cell = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        DeflectorByCell[_cell] = this;
    }

    public void RefreshDirectionState()
    {
        _baseRotationZ = NormalizeAngle(transform.eulerAngles.z);
        RegisterCell();
    }

    private void PlaySpawnEffect()
    {
        if (targetRenderer == null) return;

        _isVanishing = false;
        _baseRotationZ = NormalizeAngle(transform.eulerAngles.z);

        transform.DOKill();
        targetRenderer.DOKill();

        // 1. SETUP TRẠNG THÁI CỐ ĐỊNH
        transform.localScale = Vector3.zero;
        
        // Ép nó nằm đúng góc Đích ngay từ đầu
        transform.rotation = Quaternion.Euler(0f, 0f, _baseRotationZ);

        Color color = targetRenderer.color;
        color.a = 0f;
        targetRenderer.color = color;

        Sequence spawnSequence = DOTween.Sequence().SetLink(gameObject);

        spawnSequence.Insert(0f, transform.DOScale(_baseScale, spawnDuration)
            .SetEase(spawnScaleEase));

        spawnSequence.Insert(0f, targetRenderer.DOFade(_baseAlpha, spawnDuration * 0.5f)
            .SetEase(Ease.OutQuad));

        // 2. ÉP XOAY TƯƠNG ĐỐI (RELATIVE)
        // Ép spawnSpinTurns thành số nguyên (ví dụ 2, 3, 5) để đảm bảo nó quay đủ vòng
        // và KHÔNG BAO GIỜ bị lệch góc khi dừng lại
        float totalDegrees = 360f * Mathf.Round(spawnSpinTurns); 

        // Thêm hàm SetRelative(true) để lách luật của Unity
        spawnSequence.Insert(0f, transform.DORotate(new Vector3(0f, 0f, totalDegrees), spawnDuration, RotateMode.FastBeyond360)
            .SetRelative(true) 
            .SetEase(Ease.OutBack, overshootPower)); 
    }

    public void PlayInteractionPulse()
    {
        if (targetRenderer == null || !isActiveAndEnabled || _isVanishing) return;

        transform.DOKill();
        targetRenderer.DOKill();

        transform.localScale = _baseScale;
        transform.localPosition = _baseLocalPosition;
        transform.rotation = Quaternion.Euler(0f, 0f, _baseRotationZ);

        Color baseColor = _baseColor;
        baseColor.a = _baseAlpha;
        targetRenderer.color = baseColor;

        Vector3 redirectDir = transform.up.normalized;
        Vector3 nudgeLocal = transform.parent != null
            ? transform.parent.InverseTransformVector(redirectDir * interactionNudgeDistance)
            : redirectDir * interactionNudgeDistance;

        Sequence interactionSequence = DOTween.Sequence().SetLink(gameObject);
        interactionSequence.Join(transform.DOLocalMove(_baseLocalPosition + nudgeLocal, interactionDuration * 0.45f)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo));
        interactionSequence.Join(transform.DOScale(_baseScale * 0.9f, interactionDuration * 0.35f)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo));

        Vector3 effectCenter = GetEffectCenter();
        SpawnRedirectEcho(redirectDir, effectCenter);
        SpawnShockwave(effectCenter);
        SpawnRedirectStreaks(redirectDir, effectCenter);
    }

    private Vector3 GetEffectCenter()
    {
        Vector3 center = targetRenderer != null ? targetRenderer.bounds.center : transform.position;
        if (interactionEffectCenterOffset != Vector2.zero && targetRenderer != null)
        {
            center += targetRenderer.transform.TransformVector(new Vector3(
                interactionEffectCenterOffset.x,
                interactionEffectCenterOffset.y,
                0f));
        }

        return center;
    }

    private void SpawnRedirectEcho(Vector3 redirectDir, Vector3 effectCenter)
    {
        if (targetRenderer == null || targetRenderer.sprite == null) return;

        Vector3 pivotToCenter = targetRenderer.bounds.center - targetRenderer.transform.position;
        Vector3 startPosition = effectCenter - (pivotToCenter * redirectEchoStartScale);

        GameObject echoObject = new GameObject("DeflectorRedirectEcho");
        echoObject.hideFlags = HideFlags.DontSave;
        echoObject.transform.position = startPosition;
        echoObject.transform.rotation = targetRenderer.transform.rotation;
        echoObject.transform.localScale = targetRenderer.transform.lossyScale * redirectEchoStartScale;

        SpriteRenderer echoRenderer = echoObject.AddComponent<SpriteRenderer>();
        echoRenderer.sprite = targetRenderer.sprite;
        echoRenderer.sharedMaterial = redirectEchoMaterial != null ? redirectEchoMaterial : targetRenderer.sharedMaterial;
        echoRenderer.sortingLayerID = targetRenderer.sortingLayerID;
        echoRenderer.sortingOrder = targetRenderer.sortingOrder + 2;

        Color echoColor = redirectEchoColor;
        echoColor.a = Mathf.Clamp01(redirectEchoAlpha);
        echoRenderer.color = echoColor;

        Vector3 endCenter = effectCenter + redirectDir.normalized * redirectEchoDistance;
        Vector3 endPosition = endCenter - (pivotToCenter * redirectEchoEndScale);
        Vector3 endScale = targetRenderer.transform.lossyScale * redirectEchoEndScale;
        float echoDuration = Mathf.Max(0.01f, redirectEchoDuration);

        Sequence echoSequence = DOTween.Sequence().SetLink(echoObject);
        echoSequence.Join(echoObject.transform.DOMove(endPosition, echoDuration).SetEase(Ease.OutCubic));
        echoSequence.Join(echoObject.transform.DOScale(endScale, echoDuration).SetEase(Ease.OutCubic));
        echoSequence.Join(echoRenderer.DOFade(0f, echoDuration).SetEase(Ease.OutQuad));
        echoSequence.OnComplete(() =>
        {
            if (echoObject != null) Destroy(echoObject);
        });
    }

    private void SpawnShockwave(Vector3 effectCenter)
    {
        Material material = GetEffectMaterial();
        if (material == null) return;

        GameObject ringObject = new GameObject("DeflectorShockwave");
        ringObject.hideFlags = HideFlags.DontSave;
        ringObject.transform.position = effectCenter;
        ringObject.transform.localScale = Vector3.one * 0.25f;

        LineRenderer ringRenderer = ringObject.AddComponent<LineRenderer>();
        ringRenderer.sharedMaterial = material;
        ringRenderer.useWorldSpace = false;
        ringRenderer.loop = true;
        ringRenderer.positionCount = 40;
        ringRenderer.widthMultiplier = shockwaveLineWidth;
        ringRenderer.sortingLayerID = targetRenderer.sortingLayerID;
        ringRenderer.sortingOrder = targetRenderer.sortingOrder + 3;

        Color ringColor = redirectEchoColor;
        ringColor.a = shockwaveAlpha;
        ringRenderer.startColor = ringColor;
        ringRenderer.endColor = ringColor;

        for (int i = 0; i < ringRenderer.positionCount; i++)
        {
            float angle = (Mathf.PI * 2f * i) / ringRenderer.positionCount;
            ringRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.5f, Mathf.Sin(angle) * 0.5f, 0f));
        }

        Sequence ringSequence = DOTween.Sequence().SetLink(ringObject);
        ringSequence.Join(ringObject.transform.DOScale(Vector3.one * shockwaveRadius, shockwaveDuration).SetEase(Ease.OutCubic));
        ringSequence.Join(DOTween.To(() => shockwaveAlpha, alpha =>
        {
            if (ringRenderer == null) return;
            Color fadeColor = redirectEchoColor;
            fadeColor.a = alpha;
            ringRenderer.startColor = fadeColor;
            ringRenderer.endColor = fadeColor;
        }, 0f, shockwaveDuration).SetEase(Ease.OutQuad));
        ringSequence.OnComplete(() =>
        {
            if (ringObject != null) Destroy(ringObject);
        });
    }

    private void SpawnRedirectStreaks(Vector3 redirectDir, Vector3 effectCenter)
    {
        Material material = GetEffectMaterial();
        if (material == null) return;

        Vector3 normalizedDir = redirectDir.normalized;
        Vector3 perpendicular = new Vector3(-normalizedDir.y, normalizedDir.x, 0f);
        int streakCount = Mathf.Max(1, redirectStreakCount);
        float centerOffset = (streakCount - 1) * 0.5f;

        for (int i = 0; i < streakCount; i++)
        {
            float offset = (i - centerOffset) * redirectStreakSpacing;
            Vector3 sideOffset = perpendicular * offset;
            Vector3 startPosition = effectCenter + sideOffset;

            GameObject streakObject = new GameObject("DeflectorRedirectStreak");
            streakObject.hideFlags = HideFlags.DontSave;
            streakObject.transform.position = startPosition;

            LineRenderer streakRenderer = streakObject.AddComponent<LineRenderer>();
            streakRenderer.sharedMaterial = material;
            streakRenderer.useWorldSpace = false;
            streakRenderer.positionCount = 2;
            streakRenderer.widthMultiplier = redirectStreakLineWidth;
            streakRenderer.sortingLayerID = targetRenderer.sortingLayerID;
            streakRenderer.sortingOrder = targetRenderer.sortingOrder + 4;
            streakRenderer.SetPosition(0, Vector3.zero);
            streakRenderer.SetPosition(1, normalizedDir * redirectStreakLength);

            Color startColor = redirectEchoColor;
            startColor.a = redirectStreakAlpha;
            Color endColor = redirectEchoColor;
            endColor.a = 0f;
            streakRenderer.startColor = startColor;
            streakRenderer.endColor = endColor;

            float delay = i * 0.025f;
            Vector3 endPosition = startPosition + normalizedDir * redirectStreakTravel;
            Sequence streakSequence = DOTween.Sequence().SetLink(streakObject);
            streakSequence.SetDelay(delay);
            streakSequence.Join(streakObject.transform.DOMove(endPosition, redirectStreakDuration).SetEase(Ease.OutCubic));
            streakSequence.Join(DOTween.To(() => redirectStreakAlpha, alpha =>
            {
                if (streakRenderer == null) return;
                Color fadingStart = redirectEchoColor;
                fadingStart.a = alpha;
                Color fadingEnd = redirectEchoColor;
                fadingEnd.a = 0f;
                streakRenderer.startColor = fadingStart;
                streakRenderer.endColor = fadingEnd;
            }, 0f, redirectStreakDuration).SetEase(Ease.OutQuad));
            streakSequence.OnComplete(() =>
            {
                if (streakObject != null) Destroy(streakObject);
            });
        }
    }

    private Material GetEffectMaterial()
    {
        if (redirectLineMaterial != null) return redirectLineMaterial;
        if (_defaultEffectMaterial != null) return _defaultEffectMaterial;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) return null;

        _defaultEffectMaterial = new Material(shader)
        {
            hideFlags = HideFlags.DontSave
        };
        return _defaultEffectMaterial;
    }

    public static void PlayInteractionAtCell(Vector2Int cell)
    {
        if (DeflectorByCell.TryGetValue(cell, out GridDeflectorVisual deflector) && deflector != null)
        {
            deflector.PlayInteractionPulse();
        }
    }

    private float PlayEndGameVanish(float delay)
    {
        if (targetRenderer == null || !isActiveAndEnabled) return 0f;
        if (_isVanishing) return delay + endVanishDuration;

        _isVanishing = true;

        transform.DOKill();
        targetRenderer.DOKill();

        transform.localScale = _baseScale;
        Color color = targetRenderer.color;
        color.a = _baseAlpha;
        targetRenderer.color = color;

        transform.DOScale(0f, endVanishDuration)
            .SetEase(endVanishEase)
            .SetDelay(delay)
            .SetLink(gameObject);

        targetRenderer.DOFade(0f, endVanishDuration * 0.9f)
            .SetEase(Ease.InQuad)
            .SetDelay(delay)
            .SetLink(gameObject);

        return delay + endVanishDuration;
    }

    public static float PlayEndGameVanishAll(float stepDelay = 0.03f)
    {
        if (ActiveDeflectors.Count == 0) return 0f;

        GridDeflectorVisual[] visuals = new GridDeflectorVisual[ActiveDeflectors.Count];
        ActiveDeflectors.CopyTo(visuals);

        float maxDuration = 0f;
        int index = 0;
        for (int i = 0; i < visuals.Length; i++)
        {
            GridDeflectorVisual visual = visuals[i];
            if (visual == null) continue;

            float doneAt = visual.PlayEndGameVanish(index * stepDelay);
            if (doneAt > maxDuration) maxDuration = doneAt;
            index++;
        }

        return maxDuration;
    }

    public static void ClearAll()
    {
        ActiveDeflectors.Clear();
        DeflectorByCell.Clear();
    }

    private static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
