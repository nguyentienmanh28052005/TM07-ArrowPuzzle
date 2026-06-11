using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ArrowShadowVisual : MonoBehaviour, IGridOccupant, IArrowExitListener
{
    private const float DefaultFadeDuration = 0.18f;

    private readonly List<Vector3> _rawPoints = new List<Vector3>();
    private readonly List<Vector3> _smoothedPoints = new List<Vector3>();
    private readonly HashSet<Vector2Int> _occupiedCells = new HashSet<Vector2Int>();

    private LineRenderer _lineRenderer;
    private SpriteRenderer _headRenderer;
    private GridManager _subscribedManager;
    private GridManager _registeredManager;
    private Tween _fadeTween;

    private Color _shadowColor;
    private int _turnsToLive = 3;
    private int _remainingTurns = 3;
    private float _initialAlpha = 0.7f;
    private bool _isCountingDown;
    private bool _isDestroyed;

    public bool IsCountingDown => _isCountingDown;
    public bool IsDestroyed => _isDestroyed;
    public Vector2Int GridPosition
    {
        get
        {
            foreach (Vector2Int cell in _occupiedCells) return cell;
            return ToGridCell(transform.position);
        }
    }
    public bool IsActiveOccupant => this != null && gameObject != null && isActiveAndEnabled && !_isDestroyed;

    private void Awake()
    {
        EnsureLineRenderer();
    }

    private void OnDisable()
    {
        Unsubscribe();
        UnregisterFromGrid();
    }

    private void OnDestroy()
    {
        Unsubscribe();
        UnregisterFromGrid();
        if (_fadeTween != null && _fadeTween.IsActive()) _fadeTween.Kill();
    }

    public void Initialize(
        IList<Vector3> pathPoints,
        ArrowDir direction,
        Color ownerColor,
        Transform sourceArrowVisual,
        LineRenderer sourceLineRenderer,
        float sourceLineWidth,
        float widthMultiplier,
        float alpha,
        float headScaleMultiplier,
        int turnsToLive)
    {
        EnsureLineRenderer();

        _turnsToLive = Mathf.Max(1, turnsToLive);
        _remainingTurns = _turnsToLive;
        _initialAlpha = Mathf.Clamp01(alpha);
        _shadowColor = Color.Lerp(Color.black, ownerColor, 0.65f);
        _shadowColor.a = _initialAlpha;

        ConfigureLineRenderer(sourceLineRenderer, sourceLineWidth, widthMultiplier);
        BuildPath(pathPoints);
        ApplyAlpha(_initialAlpha);
        ConfigureHead(sourceArrowVisual, direction, headScaleMultiplier);

        gameObject.SetActive(true);
    }

    public void BeginFadeAfterOwnerReleased(Transform stableParent)
    {
        if (_isDestroyed || _isCountingDown) return;

        if (stableParent != null)
        {
            transform.SetParent(stableParent, true);
        }

        _isCountingDown = true;
        RegisterToGrid();
        Subscribe();
    }

    public void DestroyIfNotCounting()
    {
        if (_isCountingDown || _isDestroyed) return;
        DestroyVisual();
    }

    private void EnsureLineRenderer()
    {
        if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();
    }

    private void ConfigureLineRenderer(LineRenderer sourceLineRenderer, float sourceLineWidth, float widthMultiplier)
    {
        if (_lineRenderer == null) return;

        _lineRenderer.useWorldSpace = true;
        _lineRenderer.startWidth = Mathf.Max(0.01f, sourceLineWidth * widthMultiplier);
        _lineRenderer.endWidth = _lineRenderer.startWidth;
        _lineRenderer.widthMultiplier = 1f;
        _lineRenderer.numCapVertices = 10;
        _lineRenderer.numCornerVertices = 8;

        if (sourceLineRenderer != null)
        {
            _lineRenderer.sharedMaterial = sourceLineRenderer.sharedMaterial;
            _lineRenderer.alignment = sourceLineRenderer.alignment;
            _lineRenderer.textureMode = sourceLineRenderer.textureMode;
            _lineRenderer.sortingLayerID = sourceLineRenderer.sortingLayerID;
            _lineRenderer.sortingOrder = sourceLineRenderer.sortingOrder - 1;
        }
        else
        {
            _lineRenderer.sortingOrder = 9;
        }
    }

    private void BuildPath(IList<Vector3> pathPoints)
    {
        _rawPoints.Clear();
        _occupiedCells.Clear();
        if (pathPoints != null)
        {
            for (int i = 0; i < pathPoints.Count; i++)
            {
                Vector3 point = pathPoints[i];
                point.z = transform.position.z;
                _rawPoints.Add(point);
            }
        }

        RebuildOccupiedCells();

        if (_lineRenderer == null) return;
        if (_rawPoints.Count == 0)
        {
            _lineRenderer.positionCount = 0;
            return;
        }

        BuildSmoothedPositions(_rawPoints, _smoothedPoints);
        _lineRenderer.positionCount = _smoothedPoints.Count;
        for (int i = 0; i < _smoothedPoints.Count; i++)
        {
            _lineRenderer.SetPosition(i, _smoothedPoints[i]);
        }
    }

    private void RebuildOccupiedCells()
    {
        _occupiedCells.Clear();
        if (_rawPoints.Count == 0) return;

        for (int i = 0; i < _rawPoints.Count; i++)
        {
            Vector2Int current = ToGridCell(_rawPoints[i]);
            _occupiedCells.Add(current);

            if (i >= _rawPoints.Count - 1) continue;

            Vector2Int next = ToGridCell(_rawPoints[i + 1]);
            Vector2Int delta = next - current;
            int steps = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
            if (steps <= 0) continue;

            Vector2Int step = new Vector2Int(Mathf.Clamp(delta.x, -1, 1), Mathf.Clamp(delta.y, -1, 1));
            Vector2Int cell = current;
            for (int s = 0; s < steps; s++)
            {
                cell += step;
                _occupiedCells.Add(cell);
            }
        }
    }

    private static Vector2Int ToGridCell(Vector3 point)
    {
        return new Vector2Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y));
    }

    private void RegisterToGrid()
    {
        UnregisterFromGrid();

        if (_isDestroyed || GridManager.Instance == null) return;

        _registeredManager = GridManager.Instance;
        _registeredManager.RegisterArrowShadowCells(this, _occupiedCells);
    }

    private void UnregisterFromGrid()
    {
        if (_registeredManager == null) return;

        _registeredManager.UnregisterArrowShadowCells(this, _occupiedCells);
        _registeredManager = null;
    }

    private void ConfigureHead(Transform sourceArrowVisual, ArrowDir direction, float headScaleMultiplier)
    {
        if (_headRenderer != null)
        {
            if (Application.isPlaying) Destroy(_headRenderer.gameObject);
            else DestroyImmediate(_headRenderer.gameObject);
            _headRenderer = null;
        }

        if (sourceArrowVisual == null || _rawPoints.Count == 0) return;

        SpriteRenderer sourceRenderer = sourceArrowVisual.GetComponentInChildren<SpriteRenderer>(true);
        if (sourceRenderer == null || sourceRenderer.sprite == null) return;

        GameObject headObject = new GameObject("ArrowShadowHead");
        headObject.transform.SetParent(transform, false);
        headObject.transform.position = _rawPoints[0];
        headObject.transform.localScale = sourceArrowVisual.localScale * Mathf.Max(0.1f, headScaleMultiplier);

        _headRenderer = headObject.AddComponent<SpriteRenderer>();
        _headRenderer.sprite = sourceRenderer.sprite;
        _headRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
        _headRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        _headRenderer.sortingOrder = sourceRenderer.sortingOrder - 1;
        _headRenderer.flipX = sourceRenderer.flipX;
        _headRenderer.flipY = sourceRenderer.flipY;

        ApplyHeadRotation(direction);
        ApplyAlpha(_initialAlpha);
    }

    private void ApplyHeadRotation(ArrowDir direction)
    {
        if (_headRenderer == null) return;

        float angle = 0f;
        switch (direction)
        {
            case ArrowDir.Up: angle = 0f; break;
            case ArrowDir.Down: angle = 180f; break;
            case ArrowDir.Left: angle = 90f; break;
            case ArrowDir.Right: angle = -90f; break;
        }

        _headRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Subscribe()
    {
        if (_subscribedManager != null) return;
        if (GridManager.Instance == null) return;

        _subscribedManager = GridManager.Instance;
        _subscribedManager.RegisterArrowExitListener(this);
    }

    private void Unsubscribe()
    {
        if (_subscribedManager == null) return;

        _subscribedManager.UnregisterArrowExitListener(this);
        _subscribedManager = null;
    }

    public void OnArrowExited()
    {
        if (!_isCountingDown || _isDestroyed) return;

        _remainingTurns--;
        float targetAlpha = _initialAlpha * Mathf.Clamp01((float)_remainingTurns / _turnsToLive);
        AnimateAlpha(targetAlpha);

        if (_remainingTurns <= 0)
        {
            _isDestroyed = true;
            Unsubscribe();
            DOVirtual.DelayedCall(DefaultFadeDuration, DestroyVisual).SetLink(gameObject);
        }
    }

    private void AnimateAlpha(float targetAlpha)
    {
        if (_fadeTween != null && _fadeTween.IsActive()) _fadeTween.Kill();

        float startAlpha = _shadowColor.a;
        _fadeTween = DOTween.To(() => startAlpha, value => ApplyAlpha(value), targetAlpha, DefaultFadeDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    private void ApplyAlpha(float alpha)
    {
        _shadowColor.a = Mathf.Clamp01(alpha);

        if (_lineRenderer != null)
        {
            _lineRenderer.startColor = _shadowColor;
            _lineRenderer.endColor = _shadowColor;
        }

        if (_headRenderer != null)
        {
            _headRenderer.color = _shadowColor;
        }
    }

    private void DestroyVisual()
    {
        if (this == null) return;
        _isDestroyed = true;
        Unsubscribe();
        UnregisterFromGrid();

        if (Application.isPlaying) Destroy(gameObject);
        else DestroyImmediate(gameObject);
    }

    private static void BuildSmoothedPositions(List<Vector3> input, List<Vector3> output)
    {
        output.Clear();
        if (input == null || input.Count == 0) return;
        if (input.Count < 3)
        {
            output.AddRange(input);
            return;
        }

        output.Add(input[0]);
        const float angleThreshold = 15f;
        const float cornerRadius = 1f;
        const int cornerSmoothSteps = 10;

        for (int i = 1; i < input.Count - 1; i++)
        {
            Vector3 prev = input[i - 1];
            Vector3 curr = input[i];
            Vector3 next = input[i + 1];

            Vector3 dirIn = curr - prev;
            Vector3 dirOut = next - curr;

            if (dirIn.sqrMagnitude < 0.0001f || dirOut.sqrMagnitude < 0.0001f)
            {
                output.Add(curr);
                continue;
            }

            float angle = Vector3.Angle(dirIn, dirOut);
            if (angle <= angleThreshold)
            {
                output.Add(curr);
                continue;
            }

            float distIn = dirIn.magnitude;
            float distOut = dirOut.magnitude;
            float radius = Mathf.Min(cornerRadius, distIn * 0.4f, distOut * 0.4f);

            Vector3 p0 = curr - dirIn.normalized * radius;
            Vector3 p1 = curr;
            Vector3 p2 = curr + dirOut.normalized * radius;

            if (output.Count > 0 && Vector3.SqrMagnitude(output[output.Count - 1] - p0) < 0.001f)
                output.RemoveAt(output.Count - 1);

            for (int s = 0; s <= cornerSmoothSteps; s++)
            {
                float t = (float)s / cornerSmoothSteps;
                Vector3 point = (1 - t) * (1 - t) * p0 + 2 * (1 - t) * t * p1 + t * t * p2;
                output.Add(point);
            }
        }

        output.Add(input[input.Count - 1]);
    }
}
