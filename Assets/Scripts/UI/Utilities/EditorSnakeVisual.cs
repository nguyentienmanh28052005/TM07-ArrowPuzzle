using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script siêu nhẹ chuyên dùng cho Editor để hiển thị hình ảnh con rắn.
/// Chứa thêm data để LevelEditor có thể đọc và Save file.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class EditorSnakeVisual : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float cornerRadius = 1f;
    [SerializeField] private int cornerSmoothSteps = 10;
    [SerializeField] private float lineWidth = 0.35f;

    [Header("Visual References")]
    [SerializeField] private Transform arrowVisual;
    [SerializeField] private LineRenderer lineRenderer;

    // --- BỔ SUNG DATA CHO LEVEL EDITOR ---
    public ArrowDir direction;
    public Color snakeColor;
    public List<Vector2Int> LogicNodes { get; private set; } = new List<Vector2Int>();

    private List<Vector3> _renderPointsCache = new List<Vector3>();
    private List<Vector3> _smoothedPointsCache = new List<Vector3>();

    private void Awake()
    {
        SetupLineRenderer();
    }

    

    private void SetupLineRenderer()
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;
        //lineRenderer.sortingOrder = 5; 
        lineRenderer.numCapVertices = 10; 
    }

    public void Initialize(ArrowDir dir, List<Vector2Int> positions, Color color)
    {
        if (positions == null || positions.Count == 0) return;

        // Lưu trữ Data cho việc Save Level
        direction = dir;
        LogicNodes = new List<Vector2Int>(positions);

        SetupLineRenderer(); 
        SetColorImmediatePublic(color);
        UpdateVisualRotation();

        if (arrowVisual != null)
        {
            arrowVisual.position = new Vector3(positions[0].x, positions[0].y, 0f);
        }

        _renderPointsCache.Clear();
        foreach (var pos in positions)
        {
            _renderPointsCache.Add(new Vector3(pos.x, pos.y, 0f));
        }

        UpdateVisuals();
    }

    public void SetColorImmediatePublic(Color color)
    {
        snakeColor = color;
        if (lineRenderer != null)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
        if (arrowVisual != null)
        {
            var sr = arrowVisual.GetComponentInChildren<SpriteRenderer>();
            if (sr) sr.color = color;
        }
    }

    public void UpdateVisualRotation()
    {
        if (arrowVisual == null) return;
        float angle = 0f;
        switch (direction)
        {
            case ArrowDir.Up: angle = 0f; break;
            case ArrowDir.Down: angle = 180f; break;
            case ArrowDir.Left: angle = 90f; break;
            case ArrowDir.Right: angle = -90f; break;
        }
        arrowVisual.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void SetArrowWorldPosition(Vector2Int headGridPos)
    {
        if (arrowVisual == null) return;
        arrowVisual.position = new Vector3(headGridPos.x, headGridPos.y, 0f);
    }

    private void UpdateVisuals()
    {
        if (lineRenderer == null || _renderPointsCache.Count == 0) return;

        if (_renderPointsCache.Count > 2 && cornerRadius > 0f)
        {
            BuildSmoothedPositionsForRenderCached(_renderPointsCache, _smoothedPointsCache);
            lineRenderer.positionCount = _smoothedPointsCache.Count;
            for (int i = 0; i < _smoothedPointsCache.Count; i++)
            {
                lineRenderer.SetPosition(i, _smoothedPointsCache[i]);
            }
        }
        else
        {
            lineRenderer.positionCount = _renderPointsCache.Count;
            for (int i = 0; i < _renderPointsCache.Count; i++)
            {
                lineRenderer.SetPosition(i, _renderPointsCache[i]);
            }
        }
    }

    private void BuildSmoothedPositionsForRenderCached(List<Vector3> input, List<Vector3> output)
    {
        output.Clear();
        if (input.Count < 3)
        {
            output.AddRange(input);
            return;
        }

        output.Add(input[0]);
        float angleThreshold = 15f;

        for (int i = 1; i < input.Count - 1; i++)
        {
            Vector3 prev = input[i - 1];
            Vector3 curr = input[i];
            Vector3 next = input[i + 1];

            Vector3 dirIn = (curr - prev);
            Vector3 dirOut = (next - curr);

            if (dirIn.sqrMagnitude < 0.0001f || dirOut.sqrMagnitude < 0.0001f)
            {
                output.Add(curr);
                continue;
            }

            float angle = Vector3.Angle(dirIn, dirOut);

            if (angle > angleThreshold)
            {
                float distIn = dirIn.magnitude;
                float distOut = dirOut.magnitude;
                float r = Mathf.Min(cornerRadius, distIn * 0.4f, distOut * 0.4f);

                Vector3 p0 = curr - dirIn.normalized * r;
                Vector3 p1 = curr;
                Vector3 p2 = curr + dirOut.normalized * r;

                if (output.Count > 0 && Vector3.SqrMagnitude(output[output.Count - 1] - p0) < 0.001f)
                    output.RemoveAt(output.Count - 1);

                int steps = Mathf.Max(3, cornerSmoothSteps);
                for (int s = 0; s <= steps; s++)
                {
                    float t = (float)s / steps;
                    Vector3 pt = (1 - t) * (1 - t) * p0 + 2 * (1 - t) * t * p1 + t * t * p2;
                    output.Add(pt);
                }
            }
            else
            {
                output.Add(curr);
            }
        }
        output.Add(input[input.Count - 1]);
    }
}