using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Solo.MOST_IN_ONE;

[RequireComponent(typeof(LineRenderer))]
public class SnakeBlock : MonoBehaviour
{
    [Header("Movement Settings")]
    public ArrowDir direction;
    [SerializeField] private float startMoveSpeed = 0f;  
    [SerializeField] private float maxMoveSpeed = 300f;   
    [SerializeField] private float acceleration = 160f;   
    [SerializeField] private float returnMoveSpeed = 32f;
    private float _currentMoveSpeed;                      

    [Header("Corner & Spawn Settings")]
    [SerializeField] private float cornerRadius = 1f;
    [SerializeField] private int cornerSmoothSteps = 10;
    [SerializeField] private float spawnSpeed = 100f;

    [Header("Main Segments")]
    public List<Transform> bodySegments = new List<Transform>();
    [SerializeField] private Transform arrowVisual;

    [Header("Visuals")]
    public Color snakeColor = Color.white;
    public Color snakeMoveColor = Color.white;
    public Color snakeTakeHitColor = new Color(254f / 255f, 104f / 255f, 104f / 255f, 1f);
    public float lineWidth = 0.35f;

    // THAY THẾ JOB SYSTEM BẰNG ARRAY THƯỜNG
    private Vector3[] _originalState;
    private Vector3[] _currentPositions;

    private int _totalPoints;
    private int _nodesPerUnit;
    private bool _isMoving = false;
    public bool IsMoving => _isMoving;
    
    private LineRenderer lineRenderer;
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

    private float _visiblePoints;
    private bool _isSpawning = false;
    private bool _hasDealtDamage = false;

    // QUẢN LÝ GRID LƯỚI ĐỘNG
    private HashSet<Vector2Int> _occupiedCells = new HashSet<Vector2Int>();

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
        ClearFromGrid();
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

    public void SetColorImmediatePublic(Color color)
    {
        SetColorImmediate(color);
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

    public void SetColorImmediate(Color color)
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

    public bool OnHeadClicked()
    {
        if (!_isMoving && !_isSpawning) 
        {
            StartCoroutine(ProcessMovement());
            return true;
        }
        return false;
    }

    // ==========================================
    // LOGIC ĐỒNG BỘ LƯỚI ĐỘNG (GRID LOGIC MỚI)
    // ==========================================
    private IEnumerator ProcessMovement()
    {
        _isMoving = true;
        SetFocusColor(false, 0.5f);
        lineRenderer.sortingOrder = 20;

        System.Array.Copy(_originalState, _currentPositions, _totalPoints);
        _accumulatedShift = 0f;
        Vector3 moveDir = GetDirVector(direction);
        _currentMoveSpeed = startMoveSpeed;
        
        int _lastProcessedGrid = 0;

        float distToObstacle = CheckObstacleDistance(moveDir);
        bool isGhostMode = (distToObstacle == float.MaxValue);
        float targetMaxShift = distToObstacle * _nodesPerUnit;

        if (isGhostMode)
        {
            ClearFromGrid(); 
            if (ComboManager.Instance != null) ComboManager.Instance.AddCombo();
            _currentMoveSpeed *= 1.3f; 
        }

        while (true)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            float nextShiftAmount = safeDeltaTime * _currentMoveSpeed * _nodesPerUnit;

            if (!isGhostMode && (_accumulatedShift + nextShiftAmount >= targetMaxShift))
            {
                yield return StartCoroutine(HandleCollision(moveDir, distToObstacle, _lastProcessedGrid));
                break; 
            }

            _currentMoveSpeed = Mathf.MoveTowards(_currentMoveSpeed, maxMoveSpeed, acceleration * safeDeltaTime);
            _accumulatedShift += nextShiftAmount;
            
            UpdateSnakePosition(_accumulatedShift, moveDir);

            int currentGridProgress = Mathf.FloorToInt((_accumulatedShift / _nodesPerUnit) + 0.5f);
            while (_lastProcessedGrid < currentGridProgress)
            {
                if (!isGhostMode) UpdateGridOccupancy(); 
                
                Vector2Int gridToLeave = GetTailGridPosAtProgress(_lastProcessedGrid);
                if (GridDot.GridMap.TryGetValue(gridToLeave, out GridDot dotToAnimate))
                {
                    dotToAnimate.PlayLeaveEffect();
                }
                
                _lastProcessedGrid++;
            }

            // Mốc 1: Phá hủy
            if (bodySegments.Count > 0 && bodySegments[0].position.sqrMagnitude > 22500f)
            {
                Destroy(gameObject);
                yield break;
            }

            // Mốc 2: Tính điểm
            if (bodySegments.Count > 0 && bodySegments[0].position.sqrMagnitude > 1600f && !outed)
            {
                if (levelController != null) levelController.SetCountArrowInGame();
                outed = true;
            }
            
            yield return null;
        }

        _isMoving = false;
        lineRenderer.sortingOrder = 10;
    }

    private IEnumerator HandleCollision(Vector3 dir, float dist, int lastProcessedGrid)
    {
        float targetShift = dist * _nodesPerUnit;
        int _lastBounceGrid = lastProcessedGrid;

        // 1. HitObstacle Logic
        while (_accumulatedShift < targetShift)
        {
            _accumulatedShift = Mathf.MoveTowards(_accumulatedShift, targetShift, 500f * Time.deltaTime);
            UpdateSnakePosition(_accumulatedShift, dir);
            
            int currentGridProgress = Mathf.FloorToInt((_accumulatedShift / _nodesPerUnit) + 0.5f);
            if (currentGridProgress > _lastBounceGrid) 
            {
                UpdateGridOccupancy();
                _lastBounceGrid = currentGridProgress;
            }
            yield return null;
        }

        // Gameplay Effects
        if (!_hasDealtDamage) 
        { 
            if (MessageManager.Instance != null) MessageManager.Instance.SendMessage(ManhMessageType.OnTakeDamage); 
            _hasDealtDamage = true; 
        }
        if (ComboManager.Instance != null) ComboManager.Instance.StopCombo();
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowHit, 0.8f);
        SetColorImmediate(snakeTakeHitColor);
        //MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.MediumImpact);
        SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.MediumImpact);   
        
        // 2. ReturnToOrigin Logic
        while (_accumulatedShift > 0f)
        {
            _accumulatedShift = Mathf.MoveTowards(_accumulatedShift, 0, returnMoveSpeed * _nodesPerUnit * Time.deltaTime);
            UpdateSnakePosition(_accumulatedShift, dir);
            
            int currentGridProgress = Mathf.FloorToInt((_accumulatedShift / _nodesPerUnit) + 0.5f);
            if (currentGridProgress < _lastBounceGrid) 
            {
                UpdateGridOccupancy();
                _lastBounceGrid = currentGridProgress;
            }
            yield return null;
        }

        // Chốt
        System.Array.Copy(_originalState, _currentPositions, _totalPoints);
        SyncMainSegments();
        UpdateGridOccupancy(); 
    }

    // ==========================================
    // HỆ THỐNG QUẢN LÝ "HỘ KHẨU"
    // ==========================================
    
    private void ClearFromGrid()
    {
        if (GridManager.Instance == null || GridManager.Instance.GridMap == null) return;
        
        foreach (var cell in _occupiedCells)
        {
            if (GridManager.Instance.GridMap.TryGetValue(cell, out SnakeBlock block) && block == this)
            {
                GridManager.Instance.GridMap.Remove(cell);
            }
        }
        _occupiedCells.Clear();
    }

    private void UpdateGridOccupancy()
    {
        if (GridManager.Instance == null || GridManager.Instance.GridMap == null || !_isInitialized) return;
        
        ClearFromGrid();

        float headIdx = -_accumulatedShift;
        for (int i = 0; i < _totalPoints; i++)
        {
            float trackIdx = headIdx + i;
            Vector2Int cell = GetGridPosFromTrackIndex(trackIdx);
            _occupiedCells.Add(cell);
        }

        foreach(var cell in _occupiedCells) 
        {
            GridManager.Instance.GridMap[cell] = this;
        }
    }

    private Vector2Int GetGridPosFromTrackIndex(float trackIndex)
    {
        Vector3 pos;
        if (trackIndex <= 0)
        {
            float distFromHead = Mathf.Abs(trackIndex) / _nodesPerUnit;
            pos = _originalState[0] + GetDirVector(direction) * distFromHead;
        }
        else if (trackIndex >= _totalPoints - 1) pos = _originalState[_totalPoints - 1];
        else
        {
            int idx = Mathf.FloorToInt(trackIndex);
            float t = trackIndex - idx;
            pos = Vector3.Lerp(_originalState[idx], _originalState[idx + 1], t);
        }
        return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
    }

    private float CheckObstacleDistance(Vector3 dir)
    {
        if (!_isInitialized || _currentPositions == null || GridManager.Instance == null) return float.MaxValue;

        Vector2Int headPos = new Vector2Int(Mathf.RoundToInt(_currentPositions[0].x), Mathf.RoundToInt(_currentPositions[0].y));
        Vector2Int step = new Vector2Int(Mathf.RoundToInt(dir.x), Mathf.RoundToInt(dir.y));

        for (int d = 1; d < 50; d++)
        {
            Vector2Int checkPos = headPos + (step * d);
            if (Mathf.Abs(checkPos.x) > 100 || Mathf.Abs(checkPos.y) > 100) return float.MaxValue;

            SnakeBlock obstacle = GridManager.Instance.GetSnakeAt(checkPos);
            if (obstacle != null && obstacle != this) 
            {
                return d - 1; 
            }
        }
        return float.MaxValue;
    }

    private Vector2Int GetTailGridPosAtProgress(int gridsMoved) 
    {
        float shiftAtThatTime = gridsMoved * _nodesPerUnit;
        float tailTrackIdx = -shiftAtThatTime + (_totalPoints - 1);
        Vector3 exactTailPos = GetPositionAtTrackIndex(tailTrackIdx);
        return new Vector2Int(Mathf.RoundToInt(exactTailPos.x), Mathf.RoundToInt(exactTailPos.y));
    }

    // ==========================================
    // KHỞI TẠO VÀ TÍNH TOÁN HÌNH ẢNH
    // ==========================================

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

            _originalState = new Vector3[_totalPoints];
            _currentPositions = new Vector3[_totalPoints];

            int arrayIndex = 0;
            for (int i = 0; i < segmentsCount; i++)
            {
                Vector3 start = bodySegments[i].position;
                Vector3 end = bodySegments[i + 1].position;
                int count = pointsPerSegment[i];
                for (int j = 0; j < count; j++)
                {
                    float t = (float)j / count;
                    _originalState[arrayIndex] = Vector3.Lerp(start, end, t);
                    arrayIndex++;
                }
            }
            _originalState[arrayIndex] = bodySegments[segmentsCount].position;
            System.Array.Copy(_originalState, _currentPositions, _totalPoints);
        }
        else if (bodySegments.Count == 1)
        {
            _totalPoints = 1;
            _originalState = new Vector3[] { bodySegments[0].position };
            _currentPositions = new Vector3[] { bodySegments[0].position };
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

        UpdateGridOccupancy(); // Ghi danh vào Grid ngay khi sinh ra

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
            yield return null;
        }

        if (bodySegments.Count > 0 && bodySegments[0] != null)
        {
            Transform head = bodySegments[0];
            SetSegmentVisible(head, true);
            Vector3 originalScale = _originalSegmentScales.Count > 0 ? _originalSegmentScales[0] : Vector3.one;
            head.DOKill();
            head.localScale = Vector3.zero;
            head.DOScale(originalScale, 0.4f).SetEase(Ease.OutBack).SetLink(head.gameObject); 
        }

        _visiblePoints = _totalPoints;
        _isSpawning = false;
        _forceRedraw = true;
    }

    private void UpdateSnakePosition(float shift, Vector3 moveDir)
    {
        if (!_isInitialized) return;
        for (int i = 0; i < _totalPoints; i++)
        {
            float trackIdx = -shift + i;
            if (trackIdx < 0) 
            {
                float distFromHead = Mathf.Abs(trackIdx) / _nodesPerUnit;
                _currentPositions[i] = _originalState[0] + moveDir * distFromHead;
            }
            else
            {
                int idx = Mathf.FloorToInt(trackIdx);
                if (idx >= _totalPoints - 1) _currentPositions[i] = _originalState[_totalPoints - 1];
                else 
                {
                    float t = trackIdx - idx;
                    _currentPositions[i] = Vector3.Lerp(_originalState[idx], _originalState[idx + 1], t);
                }
            }
        }
        SyncMainSegments();
    }

    private void SyncMainSegments()
    {
        for (int k = 0; k < bodySegments.Count; k++)
        {
            if (bodySegments[k] != null && k < _segmentStartIndices.Count)
            {
                int virtualIndex = _segmentStartIndices[k];
                if (virtualIndex < _totalPoints)
                {
                    bodySegments[k].position = _currentPositions[virtualIndex];
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (_isMoving || _forceRedraw || _isSpawning)
        {
            UpdateLineRenderer();
            if (!_isMoving && !_isSpawning) _forceRedraw = false;
        }
    }

    private void UpdateLineRenderer()
    {
        if (lineRenderer == null || _totalPoints <= 0 || !_isInitialized) return;

        float headTrackIdx = -_accumulatedShift;
        float tailTrackIdx = -_accumulatedShift + (_totalPoints - 1);

        if (_isSpawning)
        {
            tailTrackIdx = _totalPoints - 1; 
            headTrackIdx = tailTrackIdx - (_visiblePoints - 1); 
        }

        List<Vector3> renderPoints = new List<Vector3>();
        renderPoints.Add(GetPositionAtTrackIndex(headTrackIdx));

        int firstStatic = Mathf.CeilToInt(headTrackIdx);
        int lastStatic = Mathf.FloorToInt(tailTrackIdx);

        for (int i = firstStatic; i <= lastStatic; i++)
        {
            if (i > headTrackIdx + 0.001f && i < tailTrackIdx - 0.001f)
            {
                if (i >= 0 && i < _totalPoints) renderPoints.Add(_originalState[i]); 
            }
        }

        if (tailTrackIdx > headTrackIdx + 0.001f)
        {
            renderPoints.Add(GetPositionAtTrackIndex(tailTrackIdx));
        }

        Vector3[] finalPoints = renderPoints.ToArray();

        if (finalPoints.Length > 2 && cornerRadius > 0f)
        {
            finalPoints = BuildSmoothedPositionsForRender(finalPoints);
        }

        lineRenderer.positionCount = finalPoints.Length;
        lineRenderer.SetPositions(finalPoints);
    }

    private Vector3 GetPositionAtTrackIndex(float trackIndex)
    {
        if (trackIndex <= 0)
        {
            float distFromHead = Mathf.Abs(trackIndex) / _nodesPerUnit;
            return _originalState[0] + GetDirVector(direction) * distFromHead;
        }
        else if (trackIndex >= _totalPoints - 1) return _originalState[_totalPoints - 1];
        else
        {
            int idx = Mathf.FloorToInt(trackIndex);
            float t = trackIndex - idx;
            return Vector3.Lerp(_originalState[idx], _originalState[idx + 1], t);
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