using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningConnector : MonoBehaviour
{
    [Header("Mục Tiêu Kết Nối")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Cấu Hình Tia Sét")]
    [Tooltip("Số lượng khúc gãy của tia sét (Càng cao càng chi tiết)")]
    public int segments = 12;

    [Tooltip("Tự động tính số segment theo độ dài")]
    public bool useAutoSegments = true;

    [Tooltip("Số segment trên mỗi đơn vị chiều dài")]
    public float segmentsPerUnit = 2f;

    [Tooltip("Giới hạn nhỏ nhất cho số segment")]
    public int minSegments = 6;

    [Tooltip("Giới hạn lớn nhất cho số segment")]
    public int maxSegments = 30;
    
    [Tooltip("Độ giật/độ lệch của tia sét so với đường thẳng")]
    public float jaggedness = 0.5f;
    
    [Tooltip("Tốc độ giật chớp (Giây). Ví dụ: 0.05 là chớp liên tục")]
    public float updateRate = 0.05f;

    [Header("Retract Shape")]
    [SerializeField] private int retractMinimumPoints = 5;
    [SerializeField] private float retractTipPull = 0.34f;
    [SerializeField] private float retractTipWave = 0.42f;
    [SerializeField] private float retractTipWaveFrequency = 4.2f;
    [SerializeField] private float retractTipInfluence = 0.52f;
    [SerializeField] private float retractShimmerSpeed = 52f;
    [SerializeField] private float retractJoltStrength = 0.28f;
    [SerializeField] private float retractFoldLength = 0.42f;
    [SerializeField] private float retractWidthPulse = 0.65f;

    [Header("Disappear Audio")]
    [SerializeField] private AudioClip disappearSound;
    [SerializeField, Range(0f, 1f)] private float disappearSoundVolume = 0.8f;
    [SerializeField, Range(0.1f, 3f)] private float disappearSoundPitch = 1f;

    private LineRenderer _lineRenderer;
    private float _timer;
    private bool _useProgress;
    private float _progress;
    private bool _progressFromStart = true;
    private bool _isRetracting;
    private float _progressVisualTime;
    private int _progressShapeSeed;
    private float _baseWidthMultiplier = 1f;
    private bool _hasBaseWidthMultiplier;
    private Coroutine _progressRoutine;

    private void Awake()
    {
        EnsureRenderers();
    }

    public void SetTargets(Transform start, Transform end)
    {
        startPoint = start;
        endPoint = end;
        EnsureRenderers();
        DrawLightning();
    }

    public void SetColor(Color color)
    {
        EnsureRenderers();
        if (_lineRenderer == null) return;
        _lineRenderer.startColor = color;
        _lineRenderer.endColor = color;
    }

    public void SetActive(bool isActive)
    {
        enabled = isActive;
        EnsureRenderers();
        if (_lineRenderer != null) _lineRenderer.enabled = isActive;
    }

    public void PlayAppear(float duration, bool fromStart)
    {
        StartProgressAnimation(0f, 1f, duration, fromStart, false);
    }

    public void PlayDisappear(float duration, bool fromStart)
    {
        PlayDisappearSound();
        StartProgressAnimation(1f, 0f, duration, fromStart, true);
    }

    private void Update()
    {
        EnsureRenderers();
        if (_lineRenderer == null) return;

        // Kiểm tra nếu mất mục tiêu thì tắt tia sét
        if (startPoint == null || endPoint == null)
        {
            _lineRenderer.enabled = false;
            return;
        }

        _lineRenderer.enabled = true;

        if (_useProgress)
        {
            _progressVisualTime += Time.deltaTime;
            DrawLightningProgress(_progress, _progressFromStart);
            return;
        }

        // Bộ đếm thời gian để tia sét thay đổi hình dáng (nhấp nháy)
        _timer += Time.deltaTime;
        if (_timer >= updateRate)
        {
            _timer = 0f;
            DrawLightning();
        }
    }

    private void DrawLightning()
    {
        EnsureRenderers();
        if (_lineRenderer == null || startPoint == null || endPoint == null) return;
        Vector3 startPos = startPoint.position;
        Vector3 endPos = endPoint.position;

        // Tính toán hướng và khoảng cách của đường thẳng nối 2 điểm
        float distance = Vector3.Distance(startPos, endPos);
        Vector3 direction = distance > 0.001f ? (endPos - startPos).normalized : Vector3.right;
        int safeSegments = GetSegmentCount(distance);
        float segmentLength = distance / safeSegments;

        // Tìm Vector vuông góc (Perpendicular) trong không gian 2D để tạo độ lệch
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);

        _lineRenderer.positionCount = safeSegments + 1;
        _lineRenderer.SetPosition(0, startPos);

        for (int i = 1; i < safeSegments; i++)
        {
            Vector3 basePosition = startPos + direction * (segmentLength * i);
            float offset = Random.Range(-jaggedness, jaggedness);
            Vector3 finalPosition = basePosition + (perpendicular * offset);
            _lineRenderer.SetPosition(i, finalPosition);
        }

        _lineRenderer.SetPosition(safeSegments, endPos);
    }

    private void DrawLightningProgress(float progress, bool fromStart)
    {
        if (_lineRenderer == null || startPoint == null || endPoint == null) return;

        Vector3 startPos = startPoint.position;
        Vector3 endPos = endPoint.position;

        if (!fromStart)
        {
            Vector3 temp = startPos;
            startPos = endPos;
            endPos = temp;
        }

        float clamped = Mathf.Clamp01(progress);
        if (clamped <= 0f)
        {
            RestoreWidthMultiplier();
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0, startPos);
            _lineRenderer.SetPosition(1, startPos);
            return;
        }

        if (clamped >= 1f)
        {
            RestoreWidthMultiplier();
            DrawLightning();
            return;
        }

        ApplyProgressWidth(clamped);

        float distance = Vector3.Distance(startPos, endPos);
        Vector3 direction = distance > 0.001f ? (endPos - startPos).normalized : Vector3.right;
        int safeSegments = GetSegmentCount(distance);
        float segmentLength = distance / safeSegments;
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);

        float scaled = clamped * safeSegments;
        int lastFull = Mathf.Clamp(Mathf.FloorToInt(scaled), 0, safeSegments - 1);
        float partial = scaled - lastFull;
        bool hasPartialTip = partial > 0.001f;
        int visiblePointCount = lastFull + (hasPartialTip ? 2 : 1);

        if (_isRetracting)
        {
            int minimumPoints = Mathf.Min(Mathf.Max(2, retractMinimumPoints), safeSegments + 1);
            visiblePointCount = Mathf.Max(visiblePointCount, minimumPoints);
        }

        visiblePointCount = Mathf.Clamp(visiblePointCount, 2, safeSegments + 1);
        _lineRenderer.positionCount = visiblePointCount;

        for (int i = 0; i < visiblePointCount; i++)
        {
            float segmentIndex = Mathf.Min(i, scaled);
            Vector3 point = GetProgressPoint(
                startPos,
                direction,
                perpendicular,
                segmentLength,
                safeSegments,
                segmentIndex,
                clamped);

            if (_isRetracting && i > lastFull + 1)
            {
                float foldedTail = i - (lastFull + 1f);
                float foldedRatio = foldedTail / Mathf.Max(1f, visiblePointCount - lastFull - 2f);
                float retractAmount = 1f - clamped;
                float foldWave = Mathf.Sin((foldedRatio * 2.5f + _progressVisualTime * 12f) * Mathf.PI);
                point += direction * (foldedRatio * segmentLength * retractFoldLength);
                point += perpendicular * (foldWave * retractTipWave * 0.45f * retractAmount);
            }

            _lineRenderer.SetPosition(i, point);
        }
    }

    private Vector3 GetJaggedPoint(Vector3 startPos, Vector3 direction, Vector3 perpendicular, float segmentLength, int index)
    {
        Vector3 basePosition = startPos + direction * (segmentLength * index);
        float offset = Random.Range(-jaggedness, jaggedness);
        return basePosition + (perpendicular * offset);
    }

    private Vector3 GetProgressPoint(
        Vector3 startPos,
        Vector3 direction,
        Vector3 perpendicular,
        float segmentLength,
        int safeSegments,
        float segmentIndex,
        float visibleProgress)
    {
        float clampedIndex = Mathf.Clamp(segmentIndex, 0f, safeSegments);
        float normalized = safeSegments > 0 ? clampedIndex / safeSegments : 0f;
        Vector3 basePosition = startPos + direction * (segmentLength * clampedIndex);

        if (clampedIndex <= 0.001f) return basePosition;

        float seed = _progressShapeSeed * 0.173f;
        float coarseNoise = Mathf.PerlinNoise(seed, normalized * 7.13f) * 2f - 1f;
        float shimmer = Mathf.Sin(seed + normalized * 31.4f + _progressVisualTime * retractShimmerSpeed) * 0.22f;
        float offset = (coarseNoise + shimmer) * jaggedness;

        if (_isRetracting)
        {
            float influenceLength = Mathf.Max(0.001f, retractTipInfluence);
            float tipStart = Mathf.Max(0f, visibleProgress - influenceLength);
            float tipInfluence = Mathf.InverseLerp(tipStart, Mathf.Max(tipStart + 0.001f, visibleProgress), normalized);
            float retractAmount = 1f - visibleProgress;
            float wavePhase = (normalized * retractTipWaveFrequency + _progressVisualTime * 6f + seed) * Mathf.PI * 2f;
            float joltPhase = (normalized * 17.7f + _progressVisualTime * 18f + seed) * Mathf.PI * 2f;
            float snap = Mathf.SmoothStep(0f, 1f, retractAmount);

            offset += Mathf.Sin(wavePhase) * retractTipWave * tipInfluence;
            offset += Mathf.Sign(Mathf.Sin(joltPhase)) * retractJoltStrength * snap * tipInfluence;
            basePosition -= direction * (retractTipPull * snap * tipInfluence);
        }

        return basePosition + perpendicular * offset;
    }

    private void StartProgressAnimation(float from, float to, float duration, bool fromStart, bool keepProgress)
    {
        if (_progressRoutine != null)
        {
            StopCoroutine(_progressRoutine);
            _progressRoutine = null;
        }

        _progressRoutine = StartCoroutine(AnimateProgress(from, to, duration, fromStart, keepProgress));
    }

    private void PlayDisappearSound()
    {
        if (disappearSound == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySfx(disappearSound, disappearSoundVolume, disappearSoundPitch);
    }

    private System.Collections.IEnumerator AnimateProgress(float from, float to, float duration, bool fromStart, bool keepProgress)
    {
        _useProgress = true;
        _progressFromStart = fromStart;
        _isRetracting = to < from;
        _progressVisualTime = 0f;
        _progressShapeSeed = Random.Range(1, 10000);
        _progress = Mathf.Clamp01(from);

        if (duration <= 0f)
        {
            _progress = Mathf.Clamp01(to);
            if (!keepProgress && Mathf.Approximately(to, 1f)) _useProgress = false;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / duration);
            float easedRatio = _isRetracting ? EaseOutCubic(ratio) : ratio;
            _progress = Mathf.Lerp(from, to, easedRatio);
            yield return null;
        }

        _progress = Mathf.Clamp01(to);
        RestoreWidthMultiplier();
        if (!keepProgress && Mathf.Approximately(to, 1f)) _useProgress = false;
    }

    private void ApplyProgressWidth(float visibleProgress)
    {
        if (_lineRenderer == null) return;
        CaptureBaseWidth();

        if (!_isRetracting)
        {
            RestoreWidthMultiplier();
            return;
        }

        float retractAmount = 1f - visibleProgress;
        float pulse = Mathf.Sin(Mathf.Clamp01(retractAmount) * Mathf.PI);
        _lineRenderer.widthMultiplier = _baseWidthMultiplier * (1f + retractWidthPulse * pulse);
    }

    private void CaptureBaseWidth()
    {
        if (_lineRenderer == null || _hasBaseWidthMultiplier) return;
        _baseWidthMultiplier = _lineRenderer.widthMultiplier;
        _hasBaseWidthMultiplier = true;
    }

    private void RestoreWidthMultiplier()
    {
        if (_lineRenderer == null || !_hasBaseWidthMultiplier) return;
        _lineRenderer.widthMultiplier = _baseWidthMultiplier;
    }

    private static float EaseOutCubic(float value)
    {
        float inverse = 1f - Mathf.Clamp01(value);
        return 1f - inverse * inverse * inverse;
    }

    private int GetSegmentCount(float distance)
    {
        if (!useAutoSegments) return Mathf.Max(1, segments);

        int min = Mathf.Max(1, minSegments);
        int max = Mathf.Max(min, maxSegments);
        int computed = Mathf.RoundToInt(distance * segmentsPerUnit);
        return Mathf.Clamp(computed, min, max);
    }

    private void EnsureRenderers()
    {
        if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();
        CaptureBaseWidth();
        ApplyRendererDefaults(_lineRenderer);

        LineRenderer[] renderers = GetComponents<LineRenderer>();
        for (int i = 1; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            renderers[i].enabled = false;
            renderers[i].positionCount = 0;
        }
    }

    private void ApplyRendererDefaults(LineRenderer renderer)
    {
        if (renderer == null) return;
        renderer.useWorldSpace = true;
        renderer.numCapVertices = 2;
        renderer.numCornerVertices = 2;
    }

}
