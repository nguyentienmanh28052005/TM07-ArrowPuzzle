using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

[RequireComponent(typeof(LineRenderer))]
public class SnakeBlock : MonoBehaviour
{
    [Header("Settings")]
    public ArrowDir direction;
    [SerializeField] private float moveSpeed = 100f;
    public LayerMask obstacleLayer;

    [Header("Main Segments")]
    public List<Transform> bodySegments = new List<Transform>();
    [SerializeField] private Transform arrowVisual;

    [Header("Visuals")]
    public Color snakeColor = Color.black;
    public Color snakeMoveColor = new Color(0.172f, 0.125f, 1f, 1f);
    public Color snakeTakeHitColor = new Color(254f / 255f, 104f / 255f, 104f / 255f, 1f);
    public float lineWidth = 0.4f;

    private NativeArray<Vector3> _nativeOriginalState;
    private NativeArray<Vector3> _nativeAllNodePositions;

    private Vector3[] _managedAllNodePositions;
    private int _totalPoints;
    private int _nodesPerUnit;
    private bool _isMoving = false;
    private LineRenderer lineRenderer;
    private List<Collider2D> _myColliders = new List<Collider2D>();
    private float _accumulatedShift = 0f;
    private List<int> _segmentStartIndices = new List<int>();
    private LevelController levelController;
    private bool outed = false;
    private float _originalWidthMultiplier = 1f;
    private List<Vector3> _originalSegmentScales = new List<Vector3>();
    private Tweener _colorTweener;
    private Color _currentLineColor;
    private bool _forceRedraw = false;
    private bool _isInitialized = false;

    private void Awake()
    {
        SetupLineRenderer();
    }

    private void Start()
    {
        levelController = FindObjectOfType<LevelController>();
    }

    private void OnDestroy()
    {
        if (_nativeOriginalState.IsCreated) _nativeOriginalState.Dispose();
        if (_nativeAllNodePositions.IsCreated) _nativeAllNodePositions.Dispose();
    }

    private void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;
        lineRenderer.alignment = LineAlignment.TransformZ;
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.numCornerVertices = 6;
        lineRenderer.numCapVertices = 6;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _currentLineColor = snakeColor;
        lineRenderer.startColor = snakeColor;
        lineRenderer.endColor = snakeColor;
        lineRenderer.sortingOrder = 10;
        _originalWidthMultiplier = lineRenderer.widthMultiplier;
    }

    public void SetFocusEffect(bool isFocused, float scaleFactor, float duration)
    {
        if (lineRenderer != null)
        {
            float targetWidth = isFocused ? (_originalWidthMultiplier * scaleFactor) : _originalWidthMultiplier;
            lineRenderer.DOKill();
            DOTween.To(() => lineRenderer.widthMultiplier, x =>
            {
                lineRenderer.widthMultiplier = x;
                _forceRedraw = true;
            }, targetWidth, duration)
            .SetEase(isFocused ? Ease.OutBack : Ease.OutQuad)
            .SetTarget(lineRenderer).SetLink(gameObject);
        }

        for (int i = 0; i < bodySegments.Count; i++)
        {
            if (bodySegments[i] != null && i < _originalSegmentScales.Count)
            {
                Transform seg = bodySegments[i];
                Vector3 originalScale = _originalSegmentScales[i];
                Vector3 targetScale = isFocused ? (originalScale * scaleFactor) : originalScale;
                seg.DOKill();
                seg.DOScale(targetScale, duration).SetEase(isFocused ? Ease.OutBack : Ease.OutQuad).SetLink(seg.gameObject);
            }
        }
    }

    public void SetFocusColor(bool isFocusing, float duration)
    {
        Color targetColor = isFocusing ? snakeMoveColor : snakeColor;
        RunColorTween(targetColor, duration);
    }

    private void RunColorTween(Color targetColor, float duration)
    {
        if (_colorTweener != null && _colorTweener.IsActive()) _colorTweener.Kill();

        _colorTweener = DOTween.To(() => _currentLineColor, x => _currentLineColor = x, targetColor, duration)
            .OnUpdate(() =>
            {
                ApplyColorToAll(_currentLineColor);
            })
            .SetLink(gameObject);
    }

    private void SetColorImmediate(Color color)
    {
        if (_colorTweener != null && _colorTweener.IsActive()) _colorTweener.Kill();
        _currentLineColor = color;
        ApplyColorToAll(color);
    }

    private void ApplyColorToAll(Color color)
    {
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

        foreach (var seg in bodySegments)
        {
            if (seg == null) continue;
            var sr = seg.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && (arrowVisual == null || (sr.transform != arrowVisual && sr.transform.parent != arrowVisual)))
            {
                sr.color = color;
            }
        }
    }

    public void OnHeadClicked()
    {
        if (!_isMoving) StartCoroutine(ProcessMovement());
    }

    private IEnumerator ProcessMovement()
    {
        _isMoving = true;
        SetFocusColor(false, 0.5f);

        _nativeAllNodePositions.CopyFrom(_nativeOriginalState);

        _accumulatedShift = 0f;
        Vector3 moveDir = GetDirVector(direction);

        while (true)
        {
            float distToObstacle = CheckObstacleDistance(moveDir);

            if (distToObstacle < 0.9f)
            {
                MessageManager.Instance.SendMessage(ManhMessageType.OnTakeDamage);
                SetColorImmediate(snakeTakeHitColor);
                yield return StartCoroutine(HitObstacle(moveDir, distToObstacle));
                yield return StartCoroutine(ReturnToOrigin(moveDir));
                break;
            }

            yield return StartCoroutine(MoveOneStep(moveDir));

            if (bodySegments.Count > 0 && bodySegments[0].position.sqrMagnitude > 22500f)
            {
                Destroy(gameObject);
                yield break;
            }

            if (bodySegments.Count > 0 && bodySegments[0].position.sqrMagnitude > 1600f && !outed)
            {
                levelController.SetCountArrowInGame();
                outed = true;
            }
        }

        _isMoving = false;
    }

    public void Initialize(ArrowDir dir, List<Transform> mainSegments, int resolution)
    {
        direction = dir;
        bodySegments = mainSegments;
        _nodesPerUnit = resolution;

        _originalSegmentScales.Clear();
        foreach (var seg in bodySegments)
        {
            if (seg != null) _originalSegmentScales.Add(seg.localScale);
            else _originalSegmentScales.Add(Vector3.one);
        }

        _myColliders.Clear();
        _myColliders.AddRange(GetComponentsInChildren<Collider2D>());
        foreach (var t in mainSegments)
        {
            if (t) _myColliders.AddRange(t.GetComponentsInChildren<Collider2D>());
        }

        if (bodySegments.Count > 0 && bodySegments[0] != null && arrowVisual == null)
            arrowVisual = bodySegments[0].Find("Arrow");

        if (bodySegments.Count > 1)
        {
            int segmentsCount = bodySegments.Count - 1;
            _segmentStartIndices.Clear();
            int currentTotalPoints = 0;
            List<int> pointsPerSegment = new List<int>();

            for (int i = 0; i < segmentsCount; i++)
            {
                _segmentStartIndices.Add(currentTotalPoints);
                float dist = Vector3.Distance(bodySegments[i].position, bodySegments[i + 1].position);
                int pointsCount = Mathf.Max(1, Mathf.RoundToInt(dist * _nodesPerUnit));
                pointsPerSegment.Add(pointsCount);
                currentTotalPoints += pointsCount;
            }
            _segmentStartIndices.Add(currentTotalPoints);
            _totalPoints = currentTotalPoints + 1;

            _managedAllNodePositions = new Vector3[_totalPoints];

            if (_nativeOriginalState.IsCreated) _nativeOriginalState.Dispose();
            if (_nativeAllNodePositions.IsCreated) _nativeAllNodePositions.Dispose();

            _nativeOriginalState = new NativeArray<Vector3>(_totalPoints, Allocator.Persistent);
            _nativeAllNodePositions = new NativeArray<Vector3>(_totalPoints, Allocator.Persistent);

            int arrayIndex = 0;
            for (int i = 0; i < segmentsCount; i++)
            {
                Vector3 start = bodySegments[i].position;
                Vector3 end = bodySegments[i + 1].position;
                int count = pointsPerSegment[i];
                for (int j = 0; j < count; j++)
                {
                    float t = (float)j / count;
                    _nativeOriginalState[arrayIndex] = Vector3.Lerp(start, end, t);
                    arrayIndex++;
                }
            }
            _nativeOriginalState[arrayIndex] = bodySegments[segmentsCount].position;

            _nativeAllNodePositions.CopyFrom(_nativeOriginalState);
            _nativeAllNodePositions.CopyTo(_managedAllNodePositions);
        }
        else if (bodySegments.Count == 1)
        {
            _totalPoints = 1;
            _managedAllNodePositions = new Vector3[] { bodySegments[0].position };

            if (_nativeOriginalState.IsCreated) _nativeOriginalState.Dispose();
            if (_nativeAllNodePositions.IsCreated) _nativeAllNodePositions.Dispose();

            _nativeOriginalState = new NativeArray<Vector3>(new Vector3[] { bodySegments[0].position }, Allocator.Persistent);
            _nativeAllNodePositions = new NativeArray<Vector3>(new Vector3[] { bodySegments[0].position }, Allocator.Persistent);

            _segmentStartIndices.Clear();
            _segmentStartIndices.Add(0);
        }

        _isInitialized = true;

        ApplyColorToAll(snakeColor);
        UpdateVisualRotation();
        UpdateLineRenderer();

        _forceRedraw = true;
    }

    private void LateUpdate()
    {
        if (_isMoving || _forceRedraw)
        {
            UpdateLineRenderer();
            if (!_isMoving) _forceRedraw = false;
        }
    }

    private void UpdateLineRenderer()
    {
        if (lineRenderer != null && _totalPoints > 0 && _isInitialized)
        {
            lineRenderer.positionCount = _totalPoints;
            _nativeAllNodePositions.CopyTo(_managedAllNodePositions);
            lineRenderer.SetPositions(_managedAllNodePositions);
        }
    }

    private float CheckObstacleDistance(Vector3 dir)
    {
        if (_totalPoints == 0 || !_isInitialized) return 0f;
        Vector3 startPos = _nativeAllNodePositions[0];
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, dir, 20f, obstacleLayer);
        float closestDist = float.MaxValue;
        bool found = false;
        foreach (var hit in hits)
        {
            if (hit.collider != null && !IsMyCollider(hit.collider))
            {
                if (hit.distance < closestDist)
                {
                    closestDist = hit.distance;
                    found = true;
                }
            }
        }
        return found ? closestDist : float.MaxValue;
    }

    private bool IsMyCollider(Collider2D col)
    {
        if (_myColliders == null) return false;
        return _myColliders.Contains(col);
    }

    private void UpdateSnakePosition(float shift, Vector3 moveDir)
    {
        if (!_isInitialized) return;

        CalculateSnakePositionJob job = new CalculateSnakePositionJob
        {
            shift = shift,
            moveDir = moveDir,
            nodesPerUnit = _nodesPerUnit,
            originalState = _nativeOriginalState,
            currentPositions = _nativeAllNodePositions
        };

        JobHandle handle = job.Schedule(_totalPoints, 64);
        handle.Complete();

        SyncMainSegments();
    }

    [BurstCompile]
    struct CalculateSnakePositionJob : IJobParallelFor
    {
        public float shift;
        public Vector3 moveDir;
        public int nodesPerUnit;
        [ReadOnly] public NativeArray<Vector3> originalState;
        [WriteOnly] public NativeArray<Vector3> currentPositions;

        public void Execute(int i)
        {
            float trackIndex = -shift + i;
            if (trackIndex < 0)
            {
                float distFromHead = Mathf.Abs(trackIndex) / nodesPerUnit;
                currentPositions[i] = originalState[0] + moveDir * distFromHead;
            }
            else
            {
                int count = originalState.Length;
                if (count == 0)
                {
                    currentPositions[i] = Vector3.zero;
                    return;
                }

                if (trackIndex <= 0)
                {
                    currentPositions[i] = originalState[0];
                }
                else if (trackIndex >= count - 1)
                {
                    currentPositions[i] = originalState[count - 1];
                }
                else
                {
                    int idx = (int)trackIndex; // math.floor
                    float t = trackIndex - idx;
                    currentPositions[i] = Vector3.Lerp(originalState[idx], originalState[idx + 1], t);
                }
            }
        }
    }

    private IEnumerator MoveOneStep(Vector3 dir)
    {
        float startShift = _accumulatedShift;
        float targetShift = startShift + _nodesPerUnit;
        while (_accumulatedShift < targetShift)
        {
            _accumulatedShift += Time.deltaTime * moveSpeed * _nodesPerUnit;
            if (_accumulatedShift > targetShift) _accumulatedShift = targetShift;
            UpdateSnakePosition(_accumulatedShift, dir);
            yield return null;
        }
    }

    private IEnumerator HitObstacle(Vector3 dir, float distance)
    {
        float startShift = _accumulatedShift;
        float bounceDist = Mathf.Clamp(distance - 0.1f, 0.2f, 0.9f);
        float targetShift = startShift + (bounceDist * _nodesPerUnit);
        while (_accumulatedShift < targetShift)
        {
            _accumulatedShift += Time.deltaTime * moveSpeed * _nodesPerUnit;
            if (_accumulatedShift > targetShift) _accumulatedShift = targetShift;
            UpdateSnakePosition(_accumulatedShift, dir);
            yield return null;
        }
        while (_accumulatedShift > startShift)
        {
            _accumulatedShift -= Time.deltaTime * moveSpeed * _nodesPerUnit;
            if (_accumulatedShift < startShift) _accumulatedShift = startShift;
            UpdateSnakePosition(_accumulatedShift, dir);
            yield return null;
        }
    }

    private IEnumerator ReturnToOrigin(Vector3 dir)
    {
        while (_accumulatedShift > 0f)
        {
            _accumulatedShift -= Time.deltaTime * (moveSpeed / 3f) * _nodesPerUnit;
            if (_accumulatedShift < 0f) _accumulatedShift = 0f;
            UpdateSnakePosition(_accumulatedShift, dir);
            yield return null;
        }

        _nativeAllNodePositions.CopyFrom(_nativeOriginalState);
        SyncMainSegments();
    }

    private void SyncMainSegments()
    {
        for (int k = 0; k < bodySegments.Count; k++)
        {
            if (bodySegments[k] != null)
            {
                if (k < _segmentStartIndices.Count)
                {
                    int virtualIndex = _segmentStartIndices[k];
                    if (virtualIndex < _totalPoints)
                    {
                        bodySegments[k].position = _nativeAllNodePositions[virtualIndex];
                    }
                }
            }
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

    private Vector3 GetDirVector(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: return Vector3.up;
            case ArrowDir.Down: return Vector3.down;
            case ArrowDir.Left: return Vector3.left;
            case ArrowDir.Right: return Vector3.right;
            default: return Vector3.zero;
        }
    }
}