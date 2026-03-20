using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Solo.MOST_IN_ONE;

[RequireComponent(typeof(LineRenderer))]
public class SnakeBlock : MonoBehaviour
{
    public ArrowDir direction;
    [SerializeField] private float startMoveSpeed = 30f;  
    [SerializeField] private float maxMoveSpeed = 300f;   
    [SerializeField] private float acceleration = 150f;   
    private float _currentMoveSpeed;                      

    [SerializeField] private float cornerRadius = 1f;
    [SerializeField] private int cornerSmoothSteps = 10;
    [SerializeField] private float spawnSpeed = 200f;
    public LayerMask obstacleLayer;

    public List<Transform> bodySegments = new List<Transform>();
    [SerializeField] private Transform arrowVisual;

    public Color snakeColor = Color.white;
    public Color snakeMoveColor = Color.white;
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

    private JobHandle _jobHandle;
    private bool _isJobRunning = false;

    private int _visiblePoints;
    private bool _isSpawning = false;

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
        if (_isJobRunning) _jobHandle.Complete();
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
        lineRenderer.numCornerVertices = 0;
        lineRenderer.numCapVertices = 10;
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
            .OnUpdate(() => ApplyColorToAll(_currentLineColor))
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
        if (!_isMoving && !_isSpawning) StartCoroutine(ProcessMovement());
    }

    private IEnumerator ProcessMovement()
    {
        _isMoving = true;
        SetFocusColor(false, 0.5f);

        _nativeAllNodePositions.CopyFrom(_nativeOriginalState);

        _accumulatedShift = 0f;
        Vector3 moveDir = GetDirVector(direction);
        _currentMoveSpeed = startMoveSpeed;

        int _lastProcessedGrid = 0;

        while (true)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            float distToObstacle = CheckObstacleDistance(moveDir);
            float stepDist = safeDeltaTime * _currentMoveSpeed;

            if (distToObstacle < stepDist + 0.9f)
            {
                MessageManager.Instance.SendMessage(ManhMessageType.OnTakeDamage);
                AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowHit, 0.8f);
                SetColorImmediate(snakeTakeHitColor);

                MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.MediumImpact);

                yield return StartCoroutine(HitObstacle(moveDir, distToObstacle));
                yield return StartCoroutine(ReturnToOrigin(moveDir));
                break;
            }

            _currentMoveSpeed = Mathf.MoveTowards(_currentMoveSpeed, maxMoveSpeed, acceleration * safeDeltaTime);
            _accumulatedShift += safeDeltaTime * _currentMoveSpeed * _nodesPerUnit;
            
            UpdateSnakePosition(_accumulatedShift, moveDir);

            int currentGridProgress = Mathf.FloorToInt((_accumulatedShift / _nodesPerUnit) + 0.5f);
            while (_lastProcessedGrid < currentGridProgress)
            {
                Vector2Int gridToLeave = GetTailGridPosAtProgress(_lastProcessedGrid);
                if (GridDot.GridMap.TryGetValue(gridToLeave, out GridDot dotToAnimate))
                {
                    dotToAnimate.PlayLeaveEffect();
                }
                _lastProcessedGrid++;
            }

            if (bodySegments.Count > 0 && bodySegments[0].position.sqrMagnitude > 22500f)
            {
                Destroy(gameObject);
                yield break;
            }

            if (bodySegments.Count > 0 && bodySegments[0].position.sqrMagnitude > 1600f && !outed)
            {
                if (levelController != null) levelController.SetCountArrowInGame();
                outed = true;
            }

            yield return null;
        }

        _isMoving = false;
    }

    private IEnumerator HitObstacle(Vector3 dir, float distance)
    {
        float startShift = _accumulatedShift;
        float travelDist = Mathf.Max(0f, distance - 0.1f); 
        float targetShift = startShift + (travelDist * _nodesPerUnit);
        
        while (_accumulatedShift < targetShift)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            _accumulatedShift += safeDeltaTime * maxMoveSpeed * _nodesPerUnit;
            if (_accumulatedShift > targetShift) _accumulatedShift = targetShift;
            UpdateSnakePosition(_accumulatedShift, dir);
            yield return null;
        }

        float recoilShift = Mathf.Max(0f, targetShift - (0.8f * _nodesPerUnit));
        while (_accumulatedShift > recoilShift)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            _accumulatedShift -= safeDeltaTime * maxMoveSpeed * _nodesPerUnit;
            if (_accumulatedShift < recoilShift) _accumulatedShift = recoilShift;
            UpdateSnakePosition(_accumulatedShift, dir);
            yield return null;
        }
    }

    private Vector2Int GetTailGridPosAtProgress(int gridsMoved)
    {
        int nodesMoved = gridsMoved * _nodesPerUnit;
        int trackIndex = (_totalPoints - 1) - nodesMoved;
        
        Vector3 pos;
        if (trackIndex >= 0 && trackIndex < _totalPoints)
        {
            pos = _nativeOriginalState[trackIndex];
        }
        else
        {
            int overstepNodes = -trackIndex;
            float overstepUnits = (float)overstepNodes / _nodesPerUnit;
            pos = _nativeOriginalState[0] + GetDirVector(direction) * overstepUnits;
        }
        
        return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
    }

    private Vector2Int GetCurrentTailGridPos()
    {
        if (bodySegments.Count > 0 && bodySegments[bodySegments.Count - 1] != null)
        {
            Vector3 pos = bodySegments[bodySegments.Count - 1].position;
            return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
        }
        return Vector2Int.zero;
    }

    public void Initialize(ArrowDir dir, List<Transform> mainSegments, int resolution, Color color)
    {
        snakeColor = color;
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
        _visiblePoints = 0;

        ApplyColorToAll(color);
        UpdateVisualRotation();

        for (int i = 0; i < bodySegments.Count; i++)
        {
            if (bodySegments[i] != null) SetSegmentVisible(bodySegments[i], false);
        }

        StartCoroutine(PlaySpawnAnimationFromTail());
    }

    private void SetSegmentVisible(Transform seg, bool visible)
    {
        var renderers = seg.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers) sr.enabled = visible;
    }

    private IEnumerator PlaySpawnAnimationFromTail()
    {
        _isSpawning = true;
        _visiblePoints = Mathf.Min(2, _totalPoints);

        yield return null;

        float progress = _visiblePoints;
        while (_visiblePoints < _totalPoints)
        {
            progress += Time.deltaTime * spawnSpeed;
            _visiblePoints = Mathf.Min(Mathf.FloorToInt(progress), _totalPoints);

            int currentStartIndex = _totalPoints - _visiblePoints;

            for (int k = 1; k < bodySegments.Count; k++)
            {
                if (bodySegments[k] == null) continue;
                
                if (currentStartIndex <= _segmentStartIndices[k])
                {
                }
            }
            yield return null;
        }

        if (bodySegments.Count > 0 && bodySegments[0] != null)
        {
            Transform head = bodySegments[0];
            SetSegmentVisible(head, true);

            Vector3 originalScale = _originalSegmentScales.Count > 0 ? _originalSegmentScales[0] : Vector3.one;

            head.DOKill();
            head.localScale = Vector3.zero;
            head.DOScale(originalScale, 0.4f)
                .SetEase(Ease.OutBack) 
                .SetLink(head.gameObject); 
        }

        _visiblePoints = _totalPoints;
        _isSpawning = false;
        _forceRedraw = true;
    }

    private void UpdateSnakePosition(float shift, Vector3 moveDir)
    {
        if (!_isInitialized) return;

        if (_isJobRunning)
        {
            _jobHandle.Complete();
            _isJobRunning = false;
        }

        CalculateSnakePositionJob job = new CalculateSnakePositionJob
        {
            shift = shift,
            moveDir = moveDir,
            nodesPerUnit = _nodesPerUnit,
            originalState = _nativeOriginalState,
            currentPositions = _nativeAllNodePositions
        };

        _jobHandle = job.Schedule(_totalPoints, 64);
        _isJobRunning = true;
    }

    private void LateUpdate()
    {
        if (_isJobRunning)
        {
            _jobHandle.Complete();
            _isJobRunning = false;
            SyncMainSegments();
        }

        if (_isMoving || _forceRedraw || _isSpawning)
        {
            UpdateLineRenderer();
            if (!_isMoving && !_isSpawning) _forceRedraw = false;
        }
    }

    private float CheckObstacleDistance(Vector3 dir)
    {
        if (_totalPoints == 0 || !_isInitialized) return 0f;

        if (_isJobRunning)
        {
            _jobHandle.Complete();
            _isJobRunning = false;
            SyncMainSegments();
        }

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
                    int idx = (int)trackIndex;
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
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);

            _currentMoveSpeed = Mathf.MoveTowards(_currentMoveSpeed, maxMoveSpeed, acceleration * safeDeltaTime);

            _accumulatedShift += safeDeltaTime * _currentMoveSpeed * _nodesPerUnit;
            if (_accumulatedShift > targetShift) _accumulatedShift = targetShift;
            
            UpdateSnakePosition(_accumulatedShift, dir);
            
            yield return null;
        }
    }

    private IEnumerator ReturnToOrigin(Vector3 dir)
    {
        while (_accumulatedShift > 0f)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            _accumulatedShift -= safeDeltaTime * (maxMoveSpeed / 3f) * _nodesPerUnit;
            if (_accumulatedShift < 0f) _accumulatedShift = 0f;
            UpdateSnakePosition(_accumulatedShift, dir);
            yield return null;
        }

        if (_isJobRunning)
        {
            _jobHandle.Complete();
            _isJobRunning = false;
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

    private void UpdateLineRenderer()
    {
        if (lineRenderer == null || _totalPoints <= 0 || !_isInitialized) return;

        _nativeAllNodePositions.CopyTo(_managedAllNodePositions);

        int pointCount = _isSpawning ? Mathf.Min(_visiblePoints, _totalPoints) : _totalPoints;
        if (pointCount <= 0)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        Vector3[] renderPoints = new Vector3[pointCount];
        
        if (_isSpawning && pointCount < _totalPoints)
        {
            int startIndex = _totalPoints - pointCount;
            System.Array.Copy(_managedAllNodePositions, startIndex, renderPoints, 0, pointCount);
        }
        else
        {
            renderPoints = _managedAllNodePositions;
        }

        if (pointCount > 2 && cornerRadius > 0f)
        {
            Vector3[] smoothed = BuildSmoothedPositionsForRender(renderPoints);
            lineRenderer.positionCount = smoothed.Length;
            lineRenderer.SetPositions(smoothed);
        }
        else
        {
            lineRenderer.positionCount = pointCount;
            lineRenderer.SetPositions(renderPoints);
        }
    }

    private Vector3[] BuildSmoothedPositionsForRender(Vector3[] positions)
    {
        if (positions.Length < 3) return positions;

        List<Vector3> result = new List<Vector3>(positions.Length + cornerSmoothSteps * 4);
        result.Add(positions[0]);

        float angleThreshold = 15f;

        for (int i = 1; i < positions.Length - 1; i++)
        {
            Vector3 prev = positions[i - 1];
            Vector3 curr = positions[i];
            Vector3 next = positions[i + 1];

            Vector3 dirIn = (curr - prev);
            Vector3 dirOut = (next - curr);

            if (dirIn.sqrMagnitude < 0.0001f || dirOut.sqrMagnitude < 0.0001f)
            {
                result.Add(curr);
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

                if (result.Count > 0 && Vector3.SqrMagnitude(result[result.Count - 1] - p0) < 0.001f)
                    result.RemoveAt(result.Count - 1);

                int steps = Mathf.Max(3, cornerSmoothSteps);
                for (int s = 0; s <= steps; s++)
                {
                    float t = (float)s / steps;
                    Vector3 pt = (1 - t) * (1 - t) * p0 + 2 * (1 - t) * t * p1 + t * t * p2;
                    result.Add(pt);
                }
            }
            else
            {
                result.Add(curr);
            }
        }

        result.Add(positions[positions.Length - 1]);
        return result.ToArray();
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
