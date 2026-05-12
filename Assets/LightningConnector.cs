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

    private LineRenderer _lineRenderer;
    private float _timer;
    private bool _useProgress;
    private float _progress;
    private bool _progressFromStart = true;
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

        float clamped = Mathf.Clamp01(progress);
        if (clamped <= 0f)
        {
            _lineRenderer.positionCount = 2;
            Vector3 pos = startPoint.position;
            _lineRenderer.SetPosition(0, pos);
            _lineRenderer.SetPosition(1, pos);
            return;
        }

        if (clamped >= 1f)
        {
            DrawLightning();
            return;
        }

        Vector3 startPos = startPoint.position;
        Vector3 endPos = endPoint.position;

        if (!fromStart)
        {
            Vector3 temp = startPos;
            startPos = endPos;
            endPos = temp;
        }

        float distance = Vector3.Distance(startPos, endPos);
        Vector3 direction = distance > 0.001f ? (endPos - startPos).normalized : Vector3.right;
        int safeSegments = GetSegmentCount(distance);
        float segmentLength = distance / safeSegments;
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);

        _lineRenderer.positionCount = safeSegments + 1;
        _lineRenderer.SetPosition(0, startPos);

        float scaled = clamped * safeSegments;
        int lastFull = Mathf.FloorToInt(scaled);
        float partial = scaled - lastFull;

        Vector3 lastPos = startPos;
        for (int i = 1; i <= safeSegments; i++)
        {
            if (i < lastFull)
            {
                lastPos = GetJaggedPoint(startPos, direction, perpendicular, segmentLength, i);
                _lineRenderer.SetPosition(i, lastPos);
                continue;
            }

            if (i == lastFull)
            {
                lastPos = GetJaggedPoint(startPos, direction, perpendicular, segmentLength, i);
                _lineRenderer.SetPosition(i, lastPos);
                if (partial <= 0f)
                {
                    for (int j = i + 1; j <= safeSegments; j++) _lineRenderer.SetPosition(j, lastPos);
                    break;
                }

                Vector3 nextPos = GetJaggedPoint(startPos, direction, perpendicular, segmentLength, i + 1);
                Vector3 lerped = Vector3.Lerp(lastPos, nextPos, partial);
                _lineRenderer.SetPosition(i + 1, lerped);
                for (int j = i + 2; j <= safeSegments; j++) _lineRenderer.SetPosition(j, lerped);
                break;
            }

            _lineRenderer.SetPosition(i, lastPos);
        }
    }

    private Vector3 GetJaggedPoint(Vector3 startPos, Vector3 direction, Vector3 perpendicular, float segmentLength, int index)
    {
        Vector3 basePosition = startPos + direction * (segmentLength * index);
        float offset = Random.Range(-jaggedness, jaggedness);
        return basePosition + (perpendicular * offset);
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

    private System.Collections.IEnumerator AnimateProgress(float from, float to, float duration, bool fromStart, bool keepProgress)
    {
        _useProgress = true;
        _progressFromStart = fromStart;
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
            _progress = Mathf.Lerp(from, to, ratio);
            yield return null;
        }

        _progress = Mathf.Clamp01(to);
        if (!keepProgress && Mathf.Approximately(to, 1f)) _useProgress = false;
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