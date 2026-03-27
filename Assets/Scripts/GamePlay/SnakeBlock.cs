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
    [Header("Movement Settings (New)")]
    public ArrowDir direction;
    [SerializeField] private float startMoveSpeed = 0f;  
    [SerializeField] private float maxMoveSpeed = 300f;   
    [SerializeField] private float acceleration = 160f;   
    [SerializeField] private float returnMoveSpeed = 25f;
    private float _currentMoveSpeed;                      

    [Header("Corner & Spawn Settings")]
    [SerializeField] private float cornerRadius = 1f;
    [SerializeField] private int cornerSmoothSteps = 10;
    [SerializeField] private float spawnSpeed = 100f;
    public LayerMask obstacleLayer;

    [Header("Main Segments")]
    public List<Transform> bodySegments = new List<Transform>();
    [SerializeField] private Transform arrowVisual;

    [Header("Visuals")]
    public Color snakeColor = Color.white;
    public Color snakeMoveColor = Color.white;
    public Color snakeTakeHitColor = new Color(254f / 255f, 104f / 255f, 104f / 255f, 1f);
    public float lineWidth = 0.35f;

    private NativeArray<Vector3> _nativeOriginalState;
    private NativeArray<Vector3> _nativeAllNodePositions;
    private Vector3[] _managedOriginalState;

    private int _totalPoints;
    private int _nodesPerUnit;
    private bool _isMoving = false;
    public bool IsMoving => _isMoving;
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
    private bool _hasDealtDamage = false;

    /// <summary>
    /// Khởi tạo các cấu hình mặc định cho LineRenderer.
    /// </summary>
    private void Awake()
    {
        SetupLineRenderer();
    }

    /// <summary>
    /// Liên kết với LevelController khi bắt đầu vòng đời.
    /// </summary>
    private void Start()
    {
        levelController = FindObjectOfType<LevelController>();
    }

    /// <summary>
    /// Giải phóng Native Arrays và hoàn thành Job System để chống rò rỉ bộ nhớ khi bị Destroy.
    /// </summary>
    private void OnDestroy()
    {
        if (_isJobRunning) _jobHandle.Complete(); 
        if (_nativeOriginalState.IsCreated) _nativeOriginalState.Dispose();
        if (_nativeAllNodePositions.IsCreated) _nativeAllNodePositions.Dispose();
    }

    /// <summary>
    /// Cài đặt thông số chi tiết (Width, Material, Color, Alignment) cho LineRenderer.
    /// </summary>
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

    /// <summary>
    /// Hàm công khai cho phép các hệ thống khác ép thay đổi màu sắc ngay lập tức.
    /// </summary>
    public void SetColorImmediatePublic(Color color)
    {
        SetColorImmediate(color);
    }

    /// <summary>
    /// Tạo hiệu ứng thu phóng thân rắn khi người chơi chọn (Focus).
    /// </summary>
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

    /// <summary>
    /// Chuyển đổi mềm mại màu sắc của rắn dựa trên trạng thái Focus.
    /// </summary>
    public void SetFocusColor(bool isFocusing, float duration)
    {
        Color targetColor = isFocusing ? snakeMoveColor : snakeColor;
        RunColorTween(targetColor, duration);
    }

    /// <summary>
    /// Đảm nhiệm việc chạy DOTween chuyển màu cho tất cả các thành phần trực quan.
    /// </summary>
    private void RunColorTween(Color targetColor, float duration)
    {
        if (_colorTweener != null && _colorTweener.IsActive()) _colorTweener.Kill();

        _colorTweener = DOTween.To(() => _currentLineColor, x => _currentLineColor = x, targetColor, duration)
            .OnUpdate(() => ApplyColorToAll(_currentLineColor))
            .SetLink(gameObject);
    }

    /// <summary>
    /// Áp dụng ngay lập tức một màu sắc chỉ định, bỏ qua Tween.
    /// </summary>
    private void SetColorImmediate(Color color)
    {
        if (_colorTweener != null && _colorTweener.IsActive()) _colorTweener.Kill();
        _currentLineColor = color;
        ApplyColorToAll(color);
    }

    /// <summary>
    /// Cập nhật màu sắc cho LineRenderer và tất cả các nốt Sprite liên quan.
    /// </summary>
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

    /// <summary>
    /// Được gọi bởi Input khi người chơi bấm hợp lệ vào nốt đầu để kích hoạt di chuyển.
    /// </summary>
    public void OnHeadClicked()
    {
        if (!_isMoving && !_isSpawning) StartCoroutine(ProcessMovement());
    }

    /// <summary>
    /// Coroutine cốt lõi xử lý toàn bộ logic di chuyển, va chạm, và hoạt ảnh của rắn.
    /// </summary>
    private IEnumerator ProcessMovement()
    {
        _isMoving = true;
        SetFocusColor(false, 0.5f);

        _nativeAllNodePositions.CopyFrom(_nativeOriginalState);

        _accumulatedShift = 0f;
        Vector3 moveDir = GetDirVector(direction);
        _currentMoveSpeed = startMoveSpeed;

        int _lastProcessedGrid = 0;

        float initialPathCheck = CheckObstacleDistance(moveDir);
        bool isGhostMode = (initialPathCheck == float.MaxValue);

        if (isGhostMode)
        {
            ComboManager.Instance.AddCombo(); 
            foreach (var col in _myColliders)
            {
                if (col != null) col.enabled = false;
            }
        }

        while (true)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);

            if (!isGhostMode)
            {
                float distToObstacle = CheckObstacleDistance(moveDir);
                float stepDist = safeDeltaTime * _currentMoveSpeed;

                if (distToObstacle < stepDist + 0.9f)
                {
                    if (!_hasDealtDamage)
                    {
                        MessageManager.Instance.SendMessage(ManhMessageType.OnTakeDamage);
                        _hasDealtDamage = true; 
                    }
                    ComboManager.Instance.StopCombo();
                    AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowHit, 0.8f);
                    SetColorImmediate(snakeTakeHitColor);
                    //MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.MediumImpact);
                    SettingManager.Instance.PlayHaptic(Solo.MOST_IN_ONE.MOST_HapticFeedback.HapticTypes.MediumImpact);
                    yield return StartCoroutine(HitObstacle(moveDir, distToObstacle));
                    yield return StartCoroutine(ReturnToOrigin(moveDir));
                    break; 
                }
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

    /// <summary>
    /// Bắn tia Raycast để tìm khoảng cách tới vật cản gần nhất trên hướng đi.
    /// </summary>
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
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, dir, 100f, obstacleLayer);
        
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

    /// <summary>
    /// Coroutine xử lý va chạm sát tường khi không thể tiến thêm.
    /// </summary>
    private IEnumerator HitObstacle(Vector3 dir, float distance)
    {
        float startShift = _accumulatedShift;
        float travelDist = Mathf.Max(0f, distance - 0.1f); 
        float targetShift = startShift + (travelDist * _nodesPerUnit);
        
        while (_accumulatedShift < targetShift)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            
            _accumulatedShift += safeDeltaTime * _currentMoveSpeed * _nodesPerUnit;
            
            if (_accumulatedShift > targetShift) _accumulatedShift = targetShift;
            UpdateSnakePosition(_accumulatedShift, dir);
            
            yield return null;
        }
    }

    /// <summary>
    /// Đảo ngược tiến trình di chuyển, kéo rắn từ điểm va chạm dội ngược về vị trí xuất phát.
    /// </summary>
    private IEnumerator ReturnToOrigin(Vector3 dir)
    {
        while (_accumulatedShift > 0f)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            
            _accumulatedShift -= safeDeltaTime * returnMoveSpeed * _nodesPerUnit;
            
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

    /// <summary>
    /// Tính toán tọa độ lưới (Grid) hiện tại của cái đuôi dựa trên tiến độ di chuyển.
    /// </summary>
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

    /// <summary>
    /// Khởi tạo và cắt nội suy (Slicing) đường đi dựa trên các nốt cơ bản do Editor cung cấp.
    /// </summary>
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
        }
        else if (bodySegments.Count == 1)
        {
            _totalPoints = 1;
            if (_nativeOriginalState.IsCreated) _nativeOriginalState.Dispose();
            if (_nativeAllNodePositions.IsCreated) _nativeAllNodePositions.Dispose();

            _nativeOriginalState = new NativeArray<Vector3>(new Vector3[] { bodySegments[0].position }, Allocator.Persistent);
            _nativeAllNodePositions = new NativeArray<Vector3>(new Vector3[] { bodySegments[0].position }, Allocator.Persistent);

            _segmentStartIndices.Clear();
            _segmentStartIndices.Add(0);
        }

        _managedOriginalState = new Vector3[_totalPoints];
        _nativeOriginalState.CopyTo(_managedOriginalState);

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

    /// <summary>
    /// Vô hiệu hóa hoặc kích hoạt hiển thị của một nốt cụ thể.
    /// </summary>
    private void SetSegmentVisible(Transform seg, bool visible)
    {
        var renderers = seg.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers) sr.enabled = visible;
    }

    /// <summary>
    /// Hiệu ứng tuôn trào các nốt dọc theo thân rắn từ đuôi lên đầu lúc khởi tạo.
    /// </summary>
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
                if (currentStartIndex <= _segmentStartIndices[k]) { }
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
            head.DOScale(originalScale, 0.4f).SetEase(Ease.OutBack).SetLink(head.gameObject); 
        }

        _visiblePoints = _totalPoints;
        _isSpawning = false;
        _forceRedraw = true;
    }

    /// <summary>
    /// Lên lịch trình cho C# Job System để tính toán vị trí của hàng trăm nốt một cách song song.
    /// </summary>
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

    /// <summary>
    /// Đồng bộ Job System và vẽ lại LineRenderer vào cuối mỗi khung hình.
    /// </summary>
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

    /// <summary>
    /// Kiểm tra xem một Collider có thuộc về cơ thể của chính con rắn này hay không.
    /// </summary>
    private bool IsMyCollider(Collider2D col)
    {
        if (_myColliders == null) return false;
        return _myColliders.Contains(col);
    }

    /// <summary>
    /// Struct Job System tối ưu hóa khả năng tính toán quỹ đạo dựa trên chỉ số nội suy.
    /// </summary>
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

                if (trackIndex <= 0) currentPositions[i] = originalState[0];
                else if (trackIndex >= count - 1) currentPositions[i] = originalState[count - 1];
                else
                {
                    int idx = (int)trackIndex;
                    float t = trackIndex - idx;
                    currentPositions[i] = Vector3.Lerp(originalState[idx], originalState[idx + 1], t);
                }
            }
        }
    }

    /// <summary>
    /// Đồng bộ vị trí thực tế của các Transform (nốt gắn Component) với dữ liệu đã tính toán trong Job.
    /// </summary>
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

    /// <summary>
    /// Tính toán các điểm vẽ cho LineRenderer dựa trên quỹ đạo tĩnh (Path Slicing).
    /// </summary>
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
                if (i >= 0 && i < _totalPoints)
                {
                    renderPoints.Add(_managedOriginalState[i]); 
                }
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

    /// <summary>
    /// Nội suy và trả về tọa độ chính xác của một điểm bất kỳ trên quỹ đạo.
    /// </summary>
    private Vector3 GetPositionAtTrackIndex(float trackIndex)
    {
        if (trackIndex <= 0)
        {
            float distFromHead = Mathf.Abs(trackIndex) / _nodesPerUnit;
            return _managedOriginalState[0] + GetDirVector(direction) * distFromHead;
        }
        else if (trackIndex >= _totalPoints - 1)
        {
            return _managedOriginalState[_totalPoints - 1];
        }
        else
        {
            int idx = Mathf.FloorToInt(trackIndex);
            float t = trackIndex - idx;
            return Vector3.Lerp(_managedOriginalState[idx], _managedOriginalState[idx + 1], t);
        }
    }

    /// <summary>
    /// Phân tích quỹ đạo và tạo các đường cong mềm mại tại các nốt thắt (Góc vuông).
    /// </summary>
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

    /// <summary>
    /// Đồng bộ góc xoay của phần hình ảnh đầu rắn để luôn chỉ đúng hướng chuẩn.
    /// </summary>
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

    /// <summary>
    /// Chuyển đổi trạng thái enum ArrowDir thành Vector3 định hướng vật lý.
    /// </summary>
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