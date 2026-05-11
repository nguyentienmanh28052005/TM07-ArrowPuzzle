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