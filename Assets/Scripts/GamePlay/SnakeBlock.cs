using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using DG.Tweening;
using Solo.MOST_IN_ONE;

[RequireComponent(typeof(LineRenderer))]
public class SnakeBlock : MonoBehaviour
{
    private static readonly List<SnakeBlock> _activeSnakes = new List<SnakeBlock>();
    public static IReadOnlyList<SnakeBlock> ActiveSnakes => _activeSnakes;

    [Header("Movement: BLOCKED")]
    public ArrowDir direction;
    [SerializeField] private float startMoveSpeed = 0f;  
    [SerializeField] private float maxMoveSpeed = 300f;   
    [SerializeField] private float acceleration = 100f;   
    [SerializeField] private float returnMoveSpeed = 30f;

    [Header("Movement: EXIT")]
    [SerializeField] private float exitStartSpeed = 20f;
    [SerializeField] private float exitMaxSpeed = 400f;  
    [SerializeField] private float exitAcceleration = 180f;
    [SerializeField] private float exitTravelDistance = 150f;
    [SerializeField] private int maxPathScanCells = 180;

    [Header("Movement: DASH EXIT")]
    [SerializeField] private float dashExitStartSpeed = 40f;
    [SerializeField] private float dashExitMaxSpeed = 520f;
    [SerializeField] private float dashExitAcceleration = 260f;

    private float _currentMoveSpeed;                      

    [Header("Corner & Spawn Settings")]
    [SerializeField] private float cornerRadius = 1f;
    [SerializeField] private int cornerSmoothSteps = 10;
    [SerializeField] private float spawnSpeed = 100f;

    [Header("Visuals")]
    [SerializeField] private Transform arrowVisual;
    [FormerlySerializedAs("arrowPressedMaterial")]
    [SerializeField] private Material linePressedMaterial;
    public Color snakeColor = Color.white;
    public Color snakeMoveColor = Color.white;
    public Color snakeTakeHitColor = new Color(254f / 255f, 104f / 255f, 104f / 255f, 1f);
    public float lineWidth = 0.35f;
    [SerializeField, Range(0.1f, 1f)] private float stopBlockAlpha = 0.35f;

    private List<Vector3> _renderPointsCache = new List<Vector3>(100);
    private List<Vector3> _smoothedPointsCache = new List<Vector3>(200);
    private List<float> _renderTrackIdxCache = new List<float>(100);
    private readonly List<List<Vector3>> _visualSegmentsCache = new List<List<Vector3>>(8);
    private readonly List<Vector3[]> _linePositionsArrayCache = new List<Vector3[]>(8);

    private List<Vector3> _logicNodes = new List<Vector3>();
    private Vector3[] _originalState;
    private Vector3[] _currentPositions;

    private int _totalPoints;
    private int _nodesPerUnit;
    private bool _isMoving = false;
    public bool IsMoving => _isMoving;
    
    private LineRenderer lineRenderer;
    private List<LineRenderer> _lineRenderers = new List<LineRenderer>();
    
    private float _accumulatedShift = 0f;
    private LevelController levelController;
    private bool outed = false;
    private float _originalWidthMultiplier = 1f;
    private Vector3 _originalArrowScale = Vector3.one;
    private Tweener _colorTweener;
    private Color _currentLineColor;
    private Color _lastFocusTargetColor;
    private bool _hasFocusVisualState = false;
    private bool _isFocusVisualActive = false;
    private bool _forceRedraw = false;
    private bool _isInitialized = false;
    private SpriteRenderer _arrowSpriteRenderer;
    private Material _originalLineMaterial;
    private bool _isLinePressedMaterialActive = false;

    private float _visiblePoints;
    private bool _isSpawning = false;
    private bool _isBeingErased = false;
    private float _eraseTailTrackIdx = 0f;
    private bool _hasDealtDamage = false;

    private enum ObstacleType
    {
        None,
        Snake,
        Gate,
        ElectricWall,
        CountdownBlock,
        StopBlock
    }

    private ObstacleType _lastObstacleType = ObstacleType.None;
    private Vector2Int _lastObstacleCell = new Vector2Int(int.MinValue, int.MinValue);
    private bool _isStoppedByStopBlock = false;
    private GridStopBlock _holdingStopBlock;

    public string LastObstacleType => _lastObstacleType.ToString();
    public Vector2Int LastObstacleCell => _lastObstacleCell;
    public bool IsStoppedByStopBlock => _isStoppedByStopBlock;

    private HashSet<Vector2Int> _occupiedCells = new HashSet<Vector2Int>();
    
    private struct WarpEvent 
    {
        public float rawDistFromHead0;
        public Vector3 teleportOffset;
        public ArrowDir exitDir;
        public Vector3 portalWorldPos;
        public Vector3 exitWorldPos;
        public bool isPortal;
        public GridDeflector deflector;
    }
    private List<WarpEvent> _activeWarps = new List<WarpEvent>();
    private readonly HashSet<Vector3Int> _pathScanVisitedStates = new HashSet<Vector3Int>();
    private int _lastPassedPortalIndex = -1;
    private int _lastPassedDeflectorIndex = -1;
    
    public List<Vector3> LogicNodes => _logicNodes;
    public Vector3 HeadPosition => (_isInitialized && _originalState != null && _originalState.Length > 0) ? GetPositionAtTrackIndex(-_accumulatedShift) : transform.position;

    private void Awake()
    {
        SetupLineRenderer();
    }

    private void OnEnable()
    {
        if (!_activeSnakes.Contains(this)) _activeSnakes.Add(this);
    }

    private void OnDisable()
    {
        _activeSnakes.Remove(this);
    }

    private void Start() { levelController = FindObjectOfType<LevelController>(); }

    private void OnDestroy()
    {
        if (_holdingStopBlock != null)
        {
            _holdingStopBlock.ClearHeldSnake(this);
            _holdingStopBlock = null;
        }

        ClearFromGrid();
        DOTween.Kill(this.GetInstanceID());
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
        _currentLineColor = snakeColor;
        lineRenderer.startColor = snakeColor;
        lineRenderer.endColor = snakeColor;
        lineRenderer.sortingOrder = 10;
        _originalWidthMultiplier = lineRenderer.widthMultiplier;
        _originalLineMaterial = lineRenderer.sharedMaterial;
        
        _lineRenderers.Add(lineRenderer);
    }

    private void EnsureLineRenderersCount(int count)
    {
        if (_lineRenderers == null) _lineRenderers = new List<LineRenderer>();
        if (_lineRenderers.Count == 0 && lineRenderer != null) _lineRenderers.Add(lineRenderer);

        while (_lineRenderers.Count < count)
        {
            GameObject child = new GameObject("LineSegment_" + _lineRenderers.Count);
            // IMPORTANT: use worldPositionStays = false so the child keeps localScale = (1,1,1)
            // and doesn't get auto-scaled (e.g. 2.5) when parent has scale 0.4.
            child.transform.SetParent(transform, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            LineRenderer lr = child.AddComponent<LineRenderer>();
            
            lr.startWidth = lineRenderer.startWidth;
            lr.endWidth = lineRenderer.endWidth;
            lr.widthMultiplier = lineRenderer.widthMultiplier;
            lr.widthCurve = lineRenderer.widthCurve;
            lr.useWorldSpace = lineRenderer.useWorldSpace;
            lr.alignment = lineRenderer.alignment;
            lr.textureMode = lineRenderer.textureMode;
            lr.numCornerVertices = lineRenderer.numCornerVertices;
            lr.numCapVertices = lineRenderer.numCapVertices;
            lr.startColor = _currentLineColor;
            lr.endColor = _currentLineColor;
            lr.sortingOrder = lineRenderer.sortingOrder;
            lr.sharedMaterial = lineRenderer.sharedMaterial;
            
            _lineRenderers.Add(lr);
        }
    }

    public void SetColorImmediatePublic(Color color) { SetColorImmediate(color); }

    public void SetFocusEffect(bool isFocused, float scaleFactor, float duration)
    {
        if (_isStoppedByStopBlock) return;

        float targetWidth = isFocused ? (_originalWidthMultiplier * scaleFactor) : _originalWidthMultiplier;
        
        foreach (var lr in _lineRenderers)
        {
            if (lr != null)
            {
                lr.DOKill();
                DOTween.To(() => lr.widthMultiplier, x =>
                {
                    lr.widthMultiplier = x;
                    _forceRedraw = true;
                }, targetWidth, duration)
                .SetEase(isFocused ? Ease.OutBack : Ease.OutQuad)
                .SetTarget(lr).SetLink(gameObject);
            }
        }

        if (arrowVisual != null)
        {
            Vector3 targetScale = isFocused ? (_originalArrowScale * scaleFactor) : _originalArrowScale;
            arrowVisual.DOKill();
            arrowVisual.DOScale(targetScale, duration).SetEase(isFocused ? Ease.OutBack : Ease.OutQuad).SetLink(arrowVisual.gameObject);
        }
    }

    public void SetFocusColor(bool isFocusing, float duration)
    {
        if (_isStoppedByStopBlock) return;

        Color targetColor = isFocusing ? snakeMoveColor : snakeColor;
        SetLinePressedMaterial(isFocusing);

        if (_hasFocusVisualState && _isFocusVisualActive == isFocusing && _lastFocusTargetColor == targetColor)
        {
            return;
        }

        _hasFocusVisualState = true;
        _isFocusVisualActive = isFocusing;
        _lastFocusTargetColor = targetColor;
        RunColorTween(targetColor, duration);
    }

    public void PlayDashReadyVisual(Color highlightColor, float scaleFactor, float duration)
    {
        if (!_isInitialized || _isMoving || _isSpawning || _isStoppedByStopBlock) return;

        _hasFocusVisualState = false;
        SetLinePressedMaterial(true);
        SetFocusEffect(true, scaleFactor, duration);
        RunColorTween(highlightColor, duration);
    }

    public void BeginHintGlowVisual(float scaleFactor, float duration)
    {
        if (!_isInitialized || _isMoving || _isSpawning || _isStoppedByStopBlock) return;

        _hasFocusVisualState = false;
        SetLinePressedMaterial(true);
        SetFocusEffect(true, scaleFactor, duration);
    }

    public void EndHintGlowVisual(Color restoreColor, float duration)
    {
        if (!_isInitialized) return;

        _hasFocusVisualState = false;
        SetLinePressedMaterial(false);
        SetFocusEffect(false, 1f, duration);
        RunColorTween(restoreColor, duration);
    }

    private void RunColorTween(Color targetColor, float duration)
    {
        if (_colorTweener != null && _colorTweener.IsActive()) _colorTweener.Kill();

        if (duration <= 0f || _currentLineColor == targetColor)
        {
            _currentLineColor = targetColor;
            ApplyColorToAll(_currentLineColor);
            return;
        }

        _colorTweener = DOTween.To(() => _currentLineColor, x => _currentLineColor = x, targetColor, duration)
            .OnUpdate(() => ApplyColorToAll(_currentLineColor))
            .SetLink(gameObject);
    }

    public void SetColorImmediate(Color color)
    {
        if (_colorTweener != null && _colorTweener.IsActive()) _colorTweener.Kill();
        _hasFocusVisualState = false;
        _currentLineColor = color;
        ApplyColorToAll(color);
    }

    public void BeginEraseVisual()
    {
        if (!_isInitialized || _totalPoints <= 0) return;

        if (_colorTweener != null && _colorTweener.IsActive()) _colorTweener.Kill();
        SetLinePressedMaterial(false);

        _isBeingErased = true;
        _eraseTailTrackIdx = _totalPoints - 1;

        if (arrowVisual != null)
        {
            arrowVisual.gameObject.SetActive(true);
            arrowVisual.DOKill();
            arrowVisual.localScale = _originalArrowScale;
        }

        _forceRedraw = true;
    }

    public void EraseVisualAtWorldPosition(Vector3 eraserWorldPosition, float brushRadius)
    {
        if (!_isInitialized || _totalPoints <= 0) return;
        if (!_isBeingErased) BeginEraseVisual();

        float closestTrackIdx = GetClosestTrackIndex(eraserWorldPosition);
        float brushTrackRadius = Mathf.Max(0f, brushRadius) * Mathf.Max(1, _nodesPerUnit);
        float targetTailTrackIdx = Mathf.Clamp(closestTrackIdx - brushTrackRadius, 0f, _totalPoints - 1);

        _eraseTailTrackIdx = Mathf.Min(_eraseTailTrackIdx, targetTailTrackIdx);
        UpdateArrowEraseVisual(brushTrackRadius);
        _forceRedraw = true;
    }

    public void CompleteEraseVisual()
    {
        if (!_isInitialized) return;

        _isBeingErased = true;
        _eraseTailTrackIdx = 0f;
        HideAllLineRenderers();

        if (arrowVisual != null)
        {
            arrowVisual.DOKill();
            arrowVisual.localScale = Vector3.zero;
        }
    }

    private void ApplyColorToAll(Color color)
    {
        foreach (var lr in _lineRenderers)
        {
            if (lr != null) { lr.startColor = color; lr.endColor = color; }
        }
        CacheArrowRenderer();
        if (_arrowSpriteRenderer != null) _arrowSpriteRenderer.color = color;
    }

    private void CacheArrowRenderer()
    {
        if (_arrowSpriteRenderer != null || arrowVisual == null) return;

        _arrowSpriteRenderer = arrowVisual.GetComponentInChildren<SpriteRenderer>(true);
    }

    private void SetLinePressedMaterial(bool isPressed, bool force = false)
    {
        if (lineRenderer == null) return;

        bool shouldUsePressedMaterial = isPressed && linePressedMaterial != null;
        if (!force && _isLinePressedMaterialActive == shouldUsePressedMaterial) return;

        Material targetMaterial = shouldUsePressedMaterial ? linePressedMaterial : _originalLineMaterial;
        foreach (var lr in _lineRenderers)
        {
            if (lr != null) lr.sharedMaterial = targetMaterial;
        }

        _isLinePressedMaterialActive = shouldUsePressedMaterial;
    }

    public bool OnHeadClicked()
    {
        if (!_isMoving && !_isSpawning && !_isStoppedByStopBlock) 
        {
            StartCoroutine(ProcessMovementMaster());
            return true;
        }
        return false;
    }

    public bool CanReleaseNow()
    {
        if (!_isInitialized || _isMoving || _isSpawning || _isBeingErased || _isStoppedByStopBlock) return false;

        Vector3 moveDir = GetDirVector(direction);
        bool canRelease = CheckObstacleDistance(moveDir) == float.MaxValue;
        _activeWarps.Clear();
        _lastPassedPortalIndex = -1;
        _lastPassedDeflectorIndex = -1;
        return canRelease;
    }

    public void ForceDashRelease(bool keepCurrentVisual = true)
    {
        BeginForcedExitRelease(keepCurrentVisual, isSpinRelease: false);
    }

    public void ForceDashExit(bool keepCurrentVisual = false)
    {
        ForceDashRelease(keepCurrentVisual);
    }

    public void ForceSpinRelease(bool keepCurrentVisual = true)
    {
        BeginForcedExitRelease(keepCurrentVisual, isSpinRelease: true);
    }

    private void BeginForcedExitRelease(bool keepCurrentVisual, bool isSpinRelease)
    {
        if (_isMoving || _isSpawning || _isStoppedByStopBlock) return;

        _isMoving = true;
        if (!keepCurrentVisual)
        {
            SetFocusEffect(false, 1f, 0.2f);
            SetFocusColor(false, 0.5f);
        }
        foreach (var lr in _lineRenderers) lr.sortingOrder = 20;

        System.Array.Copy(_originalState, _currentPositions, _totalPoints);
        _accumulatedShift = 0f;

        Vector3 moveDir = GetDirVector(direction);
        StartCoroutine(isSpinRelease ? ProcessSpinExitMovement(moveDir) : ProcessDashExitMovement(moveDir));
    }

    private IEnumerator ProcessMovementMaster()
    {
        _isMoving = true;
        SetFocusColor(false, 0.5f);
        foreach (var lr in _lineRenderers) lr.sortingOrder = 20;

        System.Array.Copy(_originalState, _currentPositions, _totalPoints);
        _accumulatedShift = 0f;
        
        Vector3 moveDir = GetDirVector(direction);
        float distToObstacle = CheckObstacleDistance(moveDir);
        bool isGhostMode = (distToObstacle == float.MaxValue);

        if (isGhostMode) yield return StartCoroutine(ProcessExitMovement(moveDir));
        else
        {
            float targetMaxShift = distToObstacle * _nodesPerUnit;
            yield return StartCoroutine(ProcessBlockedMovement(moveDir, targetMaxShift, distToObstacle));
        }

        _isMoving = false;
        foreach (var lr in _lineRenderers) lr.sortingOrder = 10;
    }

    private IEnumerator ProcessExitMovement(Vector3 moveDir)
    {
        yield return StartCoroutine(ProcessExitMovementInternal(moveDir, exitStartSpeed, exitMaxSpeed, exitAcceleration));
    }

    private IEnumerator ProcessDashExitMovement(Vector3 moveDir)
    {
        yield return StartCoroutine(ProcessExitMovementInternal(moveDir, dashExitStartSpeed, dashExitMaxSpeed, dashExitAcceleration));
    }

    private IEnumerator ProcessSpinExitMovement(Vector3 moveDir)
    {
        yield return StartCoroutine(ProcessExitMovementInternal(moveDir, dashExitStartSpeed, dashExitMaxSpeed, dashExitAcceleration));
    }

    private IEnumerator ProcessExitMovementInternal(Vector3 moveDir, float startSpeed, float maxSpeedValue, float accelerationValue)
    {
        CheckObstacleDistance(moveDir);
        ClearFromGrid(); 
        if (ComboManager.Instance != null) ComboManager.Instance.AddCombo(this);
        
        _currentMoveSpeed = startSpeed;
        int _lastProcessedGrid = 0;
        outed = false;

        float exitDistance = Mathf.Max(1f, exitTravelDistance);
        float finalTargetShift = exitDistance * _nodesPerUnit;

        while (true)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            _currentMoveSpeed = Mathf.MoveTowards(_currentMoveSpeed, maxSpeedValue, accelerationValue * safeDeltaTime);
            _accumulatedShift += safeDeltaTime * _currentMoveSpeed * _nodesPerUnit;
            
            UpdateSnakePosition(_accumulatedShift, moveDir);

            int currentGridProgress = Mathf.FloorToInt((_accumulatedShift / _nodesPerUnit) + 0.5f);
            while (_lastProcessedGrid < currentGridProgress)
            {
                TryCollectKeycardAtGridProgress(_lastProcessedGrid + 1);

                Vector2Int gridToLeave = GetTailGridPosAtProgress(_lastProcessedGrid);
                PlayDotLeaveEffect(gridToLeave);
                _lastProcessedGrid++;
            }

            if (_accumulatedShift > 2f * _nodesPerUnit && !outed)
            {
                if (levelController != null) levelController.SetCountArrowInGame();
                outed = true;
            }

            if (_accumulatedShift >= finalTargetShift) { Destroy(gameObject); yield break; }
            yield return null;
        }
    }

    private void TryCollectKeycardAtGridProgress(int gridProgress)
    {
        if (GridManager.Instance == null) return;
        float headTrackIdx = -(gridProgress * _nodesPerUnit);
        Vector2Int headCell = GetGridPosFromTrackIndex(headTrackIdx);
        if (GridManager.Instance.KeycardMap != null && GridManager.Instance.KeycardMap.TryGetValue(headCell, out GridKeycard card)) card.Collect();
        if (GridManager.Instance.ElectricButtonMap != null && GridManager.Instance.ElectricButtonMap.TryGetValue(headCell, out GridElectricButton button)) button.Press();
    }

    private IEnumerator ProcessBlockedMovement(Vector3 moveDir, float targetMaxShift, float distToObstacle)
    {
        _currentMoveSpeed = startMoveSpeed;
        int _lastProcessedGrid = 0;

        while (true)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            float nextShiftAmount = safeDeltaTime * _currentMoveSpeed * _nodesPerUnit;

            if (_accumulatedShift + nextShiftAmount >= targetMaxShift)
            {
                float currentDist = CheckObstacleDistance(moveDir);
                if (currentDist > distToObstacle) 
                {
                    distToObstacle = currentDist;
                    targetMaxShift = distToObstacle * _nodesPerUnit;
                    if (currentDist == float.MaxValue) { yield return StartCoroutine(ProcessExitMovement(moveDir)); yield break; }
                }
                else
                {
                    if (_lastObstacleType == ObstacleType.StopBlock
                        && GridManager.Instance != null
                        && GridManager.Instance.TryGetActiveStopBlockAt(_lastObstacleCell, out GridStopBlock stopBlock)
                        && stopBlock.CanCapture)
                    {
                        yield return StartCoroutine(HandleStopBlockCollision(moveDir, distToObstacle, _lastProcessedGrid, stopBlock));
                    }
                    else
                    {
                        yield return StartCoroutine(HandleCollision(moveDir, distToObstacle, _lastProcessedGrid));
                    }
                    break;
                }
            }

            _currentMoveSpeed = Mathf.MoveTowards(_currentMoveSpeed, maxMoveSpeed, acceleration * safeDeltaTime);
            _accumulatedShift += nextShiftAmount;
            
            UpdateSnakePosition(_accumulatedShift, moveDir);

            int currentGridProgress = Mathf.FloorToInt((_accumulatedShift / _nodesPerUnit) + 0.5f);
            while (_lastProcessedGrid < currentGridProgress)
            {
                UpdateGridOccupancy(); 
                TryCollectKeycardAtGridProgress(_lastProcessedGrid + 1);
                
                Vector2Int gridToLeave = GetTailGridPosAtProgress(_lastProcessedGrid);
                PlayDotLeaveEffect(gridToLeave);
                _lastProcessedGrid++;
            }
            yield return null;
        }
    }

    private void PlayDotLeaveEffect(Vector2Int gridPosition)
    {
        if (GridDotBatchRenderer.TryPlayLeaveEffect(gridPosition)) return;

        if (GridDot.GridMap.TryGetValue(gridPosition, out GridDot dotToAnimate))
        {
            dotToAnimate.PlayLeaveEffect();
        }
    }

    private IEnumerator HandleStopBlockCollision(Vector3 dir, float dist, int lastProcessedGrid, GridStopBlock stopBlock)
    {
        float targetShift = Mathf.Max(0f, dist) * _nodesPerUnit;
        int lastStopGrid = lastProcessedGrid;

        while (_accumulatedShift < targetShift)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            float forwardStep = Mathf.Max(_currentMoveSpeed, startMoveSpeed) * _nodesPerUnit * safeDeltaTime;
            _accumulatedShift = Mathf.MoveTowards(_accumulatedShift, targetShift, forwardStep);

            UpdateSnakePosition(_accumulatedShift, dir);

            int currentGridProgress = Mathf.FloorToInt((_accumulatedShift / _nodesPerUnit) + 0.5f);
            while (lastStopGrid < currentGridProgress)
            {
                UpdateGridOccupancy();
                TryCollectKeycardAtGridProgress(lastStopGrid + 1);

                Vector2Int gridToLeave = GetTailGridPosAtProgress(lastStopGrid);
                PlayDotLeaveEffect(gridToLeave);
                lastStopGrid++;
            }

            yield return null;
        }

        _accumulatedShift = targetShift;
        UpdateSnakePosition(_accumulatedShift, dir);
        UpdateGridOccupancy();

        ArrowDir stoppedDirection = GetHeadDirectionAtDistance(dist);
        if (stopBlock == null || !stopBlock.TryActivate(this))
        {
            yield return StartCoroutine(HandleCollision(dir, dist, lastStopGrid));
            yield break;
        }

        if (ComboManager.Instance != null) ComboManager.Instance.StopCombo();
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowHit, 0.65f, 0.9f);
        if (SettingManager.Instance != null) SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.LightImpact);

        CommitCurrentPoseAsOrigin(stoppedDirection);
        _isStoppedByStopBlock = true;
        _holdingStopBlock = stopBlock;
        ApplyStopBlockVisual();
    }

    private IEnumerator HandleCollision(Vector3 dir, float dist, int lastProcessedGrid)
    {
        float bumpFraction = 0.35f; 
        float peakShift = (dist + bumpFraction) * _nodesPerUnit;
        int _lastBounceGrid = lastProcessedGrid;

        while (_accumulatedShift < peakShift)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            float forwardStep = _currentMoveSpeed * _nodesPerUnit * safeDeltaTime;
            _accumulatedShift = Mathf.MoveTowards(_accumulatedShift, peakShift, forwardStep);
            
            UpdateSnakePosition(_accumulatedShift, dir);
            
            int currentGridProgress = Mathf.FloorToInt((_accumulatedShift / _nodesPerUnit) + 0.5f);
            if (currentGridProgress > _lastBounceGrid) { UpdateGridOccupancy(); _lastBounceGrid = currentGridProgress; }
            yield return null;
        }

        if (!_hasDealtDamage) 
        { 
            if (MessageManager.Instance != null) MessageManager.Instance.SendMessage(ManhMessageType.OnTakeDamage, this); 
            _hasDealtDamage = true; 
        }
        
        if (ComboManager.Instance != null) ComboManager.Instance.StopCombo();
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowHit, 0.8f);
        SetColorImmediate(snakeTakeHitColor);
        if (SettingManager.Instance != null) SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.MediumImpact);   
        
        while (_accumulatedShift > 0f)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            float returnStep = returnMoveSpeed * _nodesPerUnit * safeDeltaTime;
            _accumulatedShift = Mathf.MoveTowards(_accumulatedShift, 0f, returnStep);
            
            UpdateSnakePosition(_accumulatedShift, dir);
            
            int currentGridProgress = Mathf.FloorToInt((_accumulatedShift / _nodesPerUnit) + 0.5f);
            if (currentGridProgress < _lastBounceGrid) { UpdateGridOccupancy(); _lastBounceGrid = currentGridProgress; }
            yield return null;
        }

        System.Array.Copy(_originalState, _currentPositions, _totalPoints);
        _activeWarps.Clear();
        _lastPassedPortalIndex = -1;
        SyncArrowVisualPosition();
        UpdateVisualRotation();
        UpdateGridOccupancy(); 
    }

    public void ForceResetToOrigin()
    {
        StopAllCoroutines(); 
        DOTween.Kill(this.GetInstanceID()); 

        if (_holdingStopBlock != null)
        {
            _holdingStopBlock.ClearHeldSnake(this);
            _holdingStopBlock = null;
        }
        
        _accumulatedShift = 0f;
        _isMoving = false;
        _isStoppedByStopBlock = false;
        _isBeingErased = false;
        _eraseTailTrackIdx = _totalPoints > 0 ? _totalPoints - 1 : 0f;
        _hasDealtDamage = false; 
        _activeWarps.Clear();
        _lastPassedPortalIndex = -1;

        System.Array.Copy(_originalState, _currentPositions, _totalPoints);
        SyncArrowVisualPosition();
        UpdateVisualRotation();
        UpdateGridOccupancy();
        
        _forceRedraw = true;

        SetColorImmediate(snakeColor);
        if (arrowVisual != null)
        {
            arrowVisual.gameObject.SetActive(true);
            arrowVisual.localScale = _originalArrowScale;
        }
        foreach (var lr in _lineRenderers) lr.sortingOrder = 10;
    }

    public void ReleaseFromStopBlock(GridStopBlock stopBlock)
    {
        if (!_isStoppedByStopBlock) return;
        if (stopBlock != null && _holdingStopBlock != stopBlock) return;

        _holdingStopBlock = null;
        _isStoppedByStopBlock = false;
        _hasDealtDamage = false;

        SetLinePressedMaterial(false, true);
        SetColorImmediate(snakeColor);
        UpdateGridOccupancy();
        _forceRedraw = true;
    }

    private void CommitCurrentPoseAsOrigin(ArrowDir newDirection)
    {
        if (_currentPositions == null || _totalPoints <= 0) return;

        Vector3[] committedState = new Vector3[_totalPoints];
        System.Array.Copy(_currentPositions, committedState, _totalPoints);
        _originalState = committedState;

        if (_currentPositions == null || _currentPositions.Length != _totalPoints)
        {
            _currentPositions = new Vector3[_totalPoints];
        }
        System.Array.Copy(_originalState, _currentPositions, _totalPoints);

        direction = newDirection;
        _accumulatedShift = 0f;
        _activeWarps.Clear();
        _lastPassedPortalIndex = -1;
        _lastPassedDeflectorIndex = -1;

        RebuildLogicNodesFromCurrentState();
        SyncArrowVisualPosition();
        UpdateVisualRotation();
        UpdateGridOccupancy();
        _forceRedraw = true;
    }

    private void RebuildLogicNodesFromCurrentState()
    {
        _logicNodes.Clear();
        if (_originalState == null || _originalState.Length == 0) return;

        Vector2Int lastCell = new Vector2Int(int.MinValue, int.MinValue);
        for (int i = 0; i < _originalState.Length; i++)
        {
            Vector2Int cell = new Vector2Int(Mathf.RoundToInt(_originalState[i].x), Mathf.RoundToInt(_originalState[i].y));
            if (cell == lastCell) continue;

            _logicNodes.Add(new Vector3(cell.x, cell.y, 0f));
            lastCell = cell;
        }

        SimplifyLogicNodes();
    }

    private void SimplifyLogicNodes()
    {
        if (_logicNodes.Count < 3) return;

        int i = 1;
        while (i < _logicNodes.Count - 1)
        {
            Vector2Int prev = ToGridCell(_logicNodes[i - 1]);
            Vector2Int current = ToGridCell(_logicNodes[i]);
            Vector2Int next = ToGridCell(_logicNodes[i + 1]);

            Vector2Int prevStep = NormalizeGridStep(current - prev);
            Vector2Int nextStep = NormalizeGridStep(next - current);

            if (prevStep == nextStep && prevStep != Vector2Int.zero)
            {
                _logicNodes.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
    }

    private static Vector2Int ToGridCell(Vector3 position)
    {
        return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }

    private static Vector2Int NormalizeGridStep(Vector2Int step)
    {
        return new Vector2Int(Mathf.Clamp(step.x, -1, 1), Mathf.Clamp(step.y, -1, 1));
    }

    private void ApplyStopBlockVisual()
    {
        SetLinePressedMaterial(false, true);
        Color faded = snakeColor;
        faded.a = Mathf.Clamp01(stopBlockAlpha);
        SetColorImmediate(faded);
    }
    
    private void ClearFromGrid()
    {
        if (GridManager.Instance == null || GridManager.Instance.GridMap == null) return;
        foreach (var cell in _occupiedCells)
        {
            if (GridManager.Instance.GridMap.TryGetValue(cell, out SnakeBlock block) && block == this)
                GridManager.Instance.GridMap.Remove(cell);
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

        foreach(var cell in _occupiedCells) GridManager.Instance.GridMap[cell] = this;

        Vector2Int headCell = GetGridPosFromTrackIndex(headIdx);
        if (GridManager.Instance.KeycardMap != null && GridManager.Instance.KeycardMap.TryGetValue(headCell, out GridKeycard card)) card.Collect();
        if (GridManager.Instance.ElectricButtonMap != null && GridManager.Instance.ElectricButtonMap.TryGetValue(headCell, out GridElectricButton button)) button.Press();
    }

    private Vector3 GetPositionAtTrackIndex(float trackIndex, bool snapToPortalEntryForRender = false)
    {
        Vector3 rawPos;
        float distForward = 0f;

        if (trackIndex <= 0)
        {
            distForward = Mathf.Abs(trackIndex) / _nodesPerUnit;
            rawPos = GetForwardPositionWithWarps(distForward, snapToPortalEntryForRender);
        }
        else if (trackIndex >= _totalPoints - 1)
        {
            distForward = -1f;
            rawPos = _originalState[_totalPoints - 1];
        }
        else
        {
            distForward = -1f;
            int idx = Mathf.FloorToInt(trackIndex);
            float t = trackIndex - idx;
            rawPos = Vector3.Lerp(_originalState[idx], _originalState[idx + 1], t);
        }

        return rawPos;
    }

    private float GetClosestTrackIndex(Vector3 worldPosition)
    {
        if (_originalState == null || _totalPoints <= 1) return 0f;

        Vector2 point = worldPosition;
        float bestSqrDistance = float.MaxValue;
        float bestTrackIdx = 0f;

        for (int i = 0; i < _totalPoints - 1; i++)
        {
            Vector2 start = _originalState[i];
            Vector2 end = _originalState[i + 1];
            Vector2 segment = end - start;
            float segmentSqrLength = segment.sqrMagnitude;
            float t = 0f;

            if (segmentSqrLength > 0.0001f)
                t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / segmentSqrLength);

            Vector2 closest = start + segment * t;
            float sqrDistance = (point - closest).sqrMagnitude;
            if (sqrDistance < bestSqrDistance)
            {
                bestSqrDistance = sqrDistance;
                bestTrackIdx = i + t;
            }
        }

        return bestTrackIdx;
    }

    private void UpdateArrowEraseVisual(float brushTrackRadius)
    {
        if (arrowVisual == null) return;

        float hideDistance = Mathf.Max(0.001f, brushTrackRadius);
        float visibleRatio = Mathf.Clamp01(_eraseTailTrackIdx / hideDistance);
        arrowVisual.localScale = _originalArrowScale * visibleRatio;
    }

    private void HideAllLineRenderers()
    {
        if (_lineRenderers == null) return;

        for (int i = 0; i < _lineRenderers.Count; i++)
        {
            LineRenderer lr = _lineRenderers[i];
            if (lr == null) continue;

            lr.positionCount = 0;
            lr.enabled = false;
            if (i > 0) lr.gameObject.SetActive(false);
        }
    }

    private Vector3 GetForwardPositionWithWarps(float distForward, bool snapToPortalEntryForRender)
    {
        Vector3 pos = _originalState[0];
        Vector3 dirVec = GetDirVector(direction);

        if (_activeWarps == null || _activeWarps.Count == 0)
        {
            return pos + dirVec * distForward;
        }

        float prevDist = 0f;
        float snapDist = 0f;
        if (snapToPortalEntryForRender)
        {
            // Keep the tail visually glued to portal center before teleport.
            // A wider snap window avoids "cut at rim" artifacts with low/high sampling rates.
            snapDist = Mathf.Clamp(Mathf.Max(1f / Mathf.Max(1, _nodesPerUnit), 0.6f), 0.05f, 0.95f);
        }
        for (int i = 0; i < _activeWarps.Count; i++)
        {
            WarpEvent warp = _activeWarps[i];
            if (warp.rawDistFromHead0 > distForward + 0.0001f)
            {
                if (warp.isPortal && snapToPortalEntryForRender && (warp.rawDistFromHead0 - distForward) <= snapDist)
                {
                    return new Vector3(warp.portalWorldPos.x, warp.portalWorldPos.y, pos.z);
                }
                break;
            }

            float segmentDist = Mathf.Max(0f, warp.rawDistFromHead0 - prevDist);
            pos += dirVec * segmentDist;
            pos += warp.teleportOffset;
            dirVec = GetDirVector(warp.exitDir);
            prevDist = warp.rawDistFromHead0;
        }

        pos += dirVec * Mathf.Max(0f, distForward - prevDist);
        return pos;
    }

    private Vector2Int GetGridPosFromTrackIndex(float trackIndex)
    {
        Vector3 pos = GetPositionAtTrackIndex(trackIndex);
        return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
    }

    private float CheckObstacleDistance(Vector3 dir)
    {
        if (!_isInitialized || _originalState == null || GridManager.Instance == null) return float.MaxValue;

        _activeWarps.Clear();
        _lastPassedPortalIndex = -1; 
        _lastPassedDeflectorIndex = -1;
        _lastObstacleType = ObstacleType.None;
        _lastObstacleCell = new Vector2Int(int.MinValue, int.MinValue);

        Vector2Int currentPos = new Vector2Int(Mathf.RoundToInt(_originalState[0].x), Mathf.RoundToInt(_originalState[0].y));
        Vector2Int step = new Vector2Int(Mathf.RoundToInt(dir.x), Mathf.RoundToInt(dir.y));
        if (step == Vector2Int.zero) return float.MaxValue;

        _pathScanVisitedStates.Clear();
        int scanLimit = Mathf.Max(50, maxPathScanCells, Mathf.CeilToInt(exitTravelDistance) + 5, 512);
        for (int d = 1; d <= scanLimit; d++)
        {
            Vector3Int scanState = new Vector3Int(currentPos.x, currentPos.y, GetStepKey(step));
            if (!_pathScanVisitedStates.Add(scanState)) return float.MaxValue;

            Vector2Int checkPos = currentPos + step;

            SnakeBlock obstacle = GridManager.Instance.GetSnakeAt(checkPos);
            if (obstacle != null && obstacle != this)
            {
                _lastObstacleType = ObstacleType.Snake;
                _lastObstacleCell = checkPos;
                return d - 1;
            }

            if (GridManager.Instance.GateMap.ContainsKey(checkPos))
            {
                _lastObstacleType = ObstacleType.Gate;
                _lastObstacleCell = checkPos;
                return d - 1;
            }

            if (GridManager.Instance.ElectricWallMap != null && GridManager.Instance.ElectricWallMap.ContainsKey(checkPos))
            {
                _lastObstacleType = ObstacleType.ElectricWall;
                _lastObstacleCell = checkPos;
                return d - 1;
            }

            if (GridManager.Instance.HasActiveCountdownBlockAt(checkPos))
            {
                _lastObstacleType = ObstacleType.CountdownBlock;
                _lastObstacleCell = checkPos;
                return d - 1;
            }

            if (GridManager.Instance.HasActiveStopBlockAt(checkPos))
            {
                _lastObstacleType = ObstacleType.StopBlock;
                _lastObstacleCell = checkPos;
                return d - 1;
            }

            if (GridManager.Instance.PortalMap.TryGetValue(checkPos, out GridManager.PortalLink link))
            {
                Vector3 offset = new Vector3(link.exit.x - checkPos.x, link.exit.y - checkPos.y, 0);
                _activeWarps.Add(new WarpEvent {
                    rawDistFromHead0 = d,
                    teleportOffset = offset,
                    exitDir = link.exitDir,
                    portalWorldPos = new Vector3(checkPos.x, checkPos.y, 0f),
                    exitWorldPos = new Vector3(link.exit.x, link.exit.y, 0f),
                    isPortal = true,
                    deflector = null
                });

                currentPos = link.exit;
                Vector3 newDir = GetDirVector(link.exitDir);
                step = new Vector2Int(Mathf.RoundToInt(newDir.x), Mathf.RoundToInt(newDir.y));
                continue;
            }
            if (GridManager.Instance.DeflectorMap != null && GridManager.Instance.DeflectorMap.TryGetValue(checkPos, out GridDeflector hitDeflector))
            {
                ArrowDir newDir = hitDeflector.direction;
                _activeWarps.Add(new WarpEvent {
                    rawDistFromHead0 = d,
                    teleportOffset = Vector3.zero,
                    exitDir = newDir,
                    portalWorldPos = new Vector3(checkPos.x, checkPos.y, 0f),
                    exitWorldPos = new Vector3(checkPos.x, checkPos.y, 0f),
                    isPortal = false,
                    deflector = hitDeflector
                });

                currentPos = checkPos;
                Vector3 newDirVec = GetDirVector(newDir);
                step = new Vector2Int(Mathf.RoundToInt(newDirVec.x), Mathf.RoundToInt(newDirVec.y));
                continue;
            }
            currentPos = checkPos;
        }
        return float.MaxValue;
    }

    private static int GetStepKey(Vector2Int step)
    {
        if (step.y > 0) return 0;
        if (step.y < 0) return 1;
        if (step.x < 0) return 2;
        return 3;
    }

    private Vector2Int GetTailGridPosAtProgress(int gridsMoved) 
    {
        float shiftAtThatTime = gridsMoved * _nodesPerUnit;
        float tailTrackIdx = -shiftAtThatTime + (_totalPoints - 1);
        Vector3 exactTailPos = GetPositionAtTrackIndex(tailTrackIdx);
        return new Vector2Int(Mathf.RoundToInt(exactTailPos.x), Mathf.RoundToInt(exactTailPos.y));
    }

    public void Initialize(ArrowDir dir, List<Vector2Int> gridPositions, int resolution, Color color, bool playSpawnAnimation = true)
    {
        snakeColor = color;
        direction = dir;
        _nodesPerUnit = resolution;

        _logicNodes.Clear();
        foreach(var pos in gridPositions) _logicNodes.Add(new Vector3(pos.x, pos.y, 0f));

        if (arrowVisual != null) _originalArrowScale = arrowVisual.localScale;
        else _originalArrowScale = Vector3.one;

        if (_logicNodes.Count > 1)
        {
            int segmentsCount = _logicNodes.Count - 1;
            int currentTotalPoints = 0;
            List<int> pointsPerSegment = new List<int>();

            for (int i = 0; i < segmentsCount; i++)
            {
                float dist = Vector3.Distance(_logicNodes[i], _logicNodes[i + 1]);
                int pointsCount = Mathf.Max(1, Mathf.RoundToInt(dist * _nodesPerUnit));
                pointsPerSegment.Add(pointsCount);
                currentTotalPoints += pointsCount;
            }
            _totalPoints = currentTotalPoints + 1;

            _originalState = new Vector3[_totalPoints];
            _currentPositions = new Vector3[_totalPoints];

            int arrayIndex = 0;
            for (int i = 0; i < segmentsCount; i++)
            {
                Vector3 start = _logicNodes[i];
                Vector3 end = _logicNodes[i + 1];
                int count = pointsPerSegment[i];
                for (int j = 0; j < count; j++)
                {
                    float t = (float)j / count;
                    _originalState[arrayIndex] = Vector3.Lerp(start, end, t);
                    arrayIndex++;
                }
            }
            _originalState[arrayIndex] = _logicNodes[segmentsCount];
            System.Array.Copy(_originalState, _currentPositions, _totalPoints);
        }
        else if (_logicNodes.Count == 1)
        {
            _totalPoints = 1;
            _originalState = new Vector3[] { _logicNodes[0] };
            _currentPositions = new Vector3[] { _logicNodes[0] };
        }

        _isInitialized = true;
        _visiblePoints = 0;
        _isBeingErased = false;
        _eraseTailTrackIdx = _totalPoints > 0 ? _totalPoints - 1 : 0f;
        
        outed = false;
        _accumulatedShift = 0f;
        _isMoving = false;
        _isStoppedByStopBlock = false;
        _holdingStopBlock = null;
        _hasDealtDamage = false;
        _hasFocusVisualState = false;

        ApplyColorToAll(color);
        SetLinePressedMaterial(false);
        UpdateVisualRotation();
        UpdateGridOccupancy(); 

        if (arrowVisual != null)
        {
            arrowVisual.position = _currentPositions[0];
            arrowVisual.localScale = Vector3.zero;
        }

        if (playSpawnAnimation)
        {
            StartSpawnAnimationFromTail();
        }
    }

    public void StartSpawnAnimationFromTail()
    {
        if (!_isInitialized) return;
        if (_isSpawning || _visiblePoints >= _totalPoints) return;

        StartCoroutine(PlaySpawnAnimationFromTail());
    }

    private IEnumerator PlaySpawnAnimationFromTail()
    {
        _isSpawning = true;
        _visiblePoints = Mathf.Min(2f, (float)_totalPoints); 

        float progress = _visiblePoints;
        while (_visiblePoints < _totalPoints)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            progress += safeDeltaTime * spawnSpeed;
            _visiblePoints = Mathf.Min(progress, (float)_totalPoints);
            yield return null;
        }

        if (arrowVisual != null)
        {
            SyncArrowVisualPosition();
            arrowVisual.DOKill();
            arrowVisual.localScale = Vector3.zero;
            arrowVisual.DOScale(_originalArrowScale, 0.4f).SetEase(Ease.OutBack).SetLink(arrowVisual.gameObject); 
        }

        _visiblePoints = _totalPoints;
        _isSpawning = false;
        _forceRedraw = true; 
    }

    private void UpdateSnakePosition(float shift, Vector3 moveDir)
    {
        if (!_isInitialized) return;

        float headDist = shift / _nodesPerUnit;
        int passedPortalIndex = -1;
        int passedDeflectorIndex = -1;
        for (int i = 0; i < _activeWarps.Count; i++) {
            if (headDist < _activeWarps[i].rawDistFromHead0) continue;
            if (_activeWarps[i].isPortal) passedPortalIndex = i;
            else passedDeflectorIndex = i;
        }

        if (passedDeflectorIndex > _lastPassedDeflectorIndex) {
            bool playedDeflectorFeedback = false;
            for (int i = _lastPassedDeflectorIndex + 1; i <= passedDeflectorIndex; i++)
            {
                if (i < 0 || i >= _activeWarps.Count) continue;
                if (_activeWarps[i].isPortal) continue;

                Vector2Int deflectorCell = new Vector2Int(
                    Mathf.RoundToInt(_activeWarps[i].portalWorldPos.x),
                    Mathf.RoundToInt(_activeWarps[i].portalWorldPos.y));

                if (_activeWarps[i].deflector != null)
                {
                    _activeWarps[i].deflector.PlayInteractionFeedback();
                }
                else
                {
                    GridDeflectorVisual.PlayInteractionAtCell(deflectorCell);
                }
                playedDeflectorFeedback = true;
            }

            _lastPassedDeflectorIndex = passedDeflectorIndex;
            if (playedDeflectorFeedback)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowHit, 0.35f, 1.35f);
                if (SettingManager.Instance != null) SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.LightImpact);
            }
        } else if (passedDeflectorIndex < _lastPassedDeflectorIndex) {
            _lastPassedDeflectorIndex = passedDeflectorIndex;
        }

        if (passedPortalIndex > _lastPassedPortalIndex) {
            for (int i = _lastPassedPortalIndex + 1; i <= passedPortalIndex; i++)
            {
                if (i < 0 || i >= _activeWarps.Count) continue;
                if (!_activeWarps[i].isPortal) continue;

                Vector2Int entryCell = new Vector2Int(
                    Mathf.RoundToInt(_activeWarps[i].portalWorldPos.x),
                    Mathf.RoundToInt(_activeWarps[i].portalWorldPos.y));
                Vector2Int exitCell = new Vector2Int(
                    Mathf.RoundToInt(_activeWarps[i].exitWorldPos.x),
                    Mathf.RoundToInt(_activeWarps[i].exitWorldPos.y));

                GridPortalVisual.PlayEnterAtCell(entryCell);
                GridPortalVisual.PlayExitAtCell(exitCell);
            }

            _lastPassedPortalIndex = passedPortalIndex;
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowHit, 0.5f, 1.8f);
            if (SettingManager.Instance != null) SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.LightImpact);
        } else if (passedPortalIndex < _lastPassedPortalIndex) {
            _lastPassedPortalIndex = passedPortalIndex;
        }

        for (int i = 0; i < _totalPoints; i++)
        {
            float trackIdx = -shift + i;
            _currentPositions[i] = GetPositionAtTrackIndex(trackIdx);
        }
        SyncArrowVisualPosition();

        ArrowDir currentHeadDir = GetHeadDirectionAtDistance(shift / _nodesPerUnit);
        UpdateArrowVisualRotation(currentHeadDir);
    }

    private ArrowDir GetHeadDirectionAtDistance(float headDist)
    {
        if (_activeWarps == null || _activeWarps.Count == 0) return direction;

        ArrowDir dirNow = direction;
        for (int i = 0; i < _activeWarps.Count; i++)
        {
            if (headDist + 0.0001f >= _activeWarps[i].rawDistFromHead0)
            {
                dirNow = _activeWarps[i].exitDir;
            }
            else break;
        }
        return dirNow;
    }

    private void UpdateArrowVisualRotation(ArrowDir dir)
    {
        if (arrowVisual == null) return;

        float angle = 0f;
        switch (dir)
        {
            case ArrowDir.Up: angle = 0f; break;
            case ArrowDir.Down: angle = 180f; break;
            case ArrowDir.Left: angle = 90f; break;
            case ArrowDir.Right: angle = -90f; break;
        }
        arrowVisual.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void SyncArrowVisualPosition()
    {
        if (arrowVisual != null && _currentPositions != null && _currentPositions.Length > 0)
            arrowVisual.position = _currentPositions[0];
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
        else if (_isBeingErased)
        {
            tailTrackIdx = Mathf.Min(tailTrackIdx, _eraseTailTrackIdx);
        }

        _renderPointsCache.Clear();
        _renderTrackIdxCache.Clear();

        _renderPointsCache.Add(GetPositionAtTrackIndex(headTrackIdx));
        _renderTrackIdxCache.Add(headTrackIdx);

        int firstStatic = Mathf.CeilToInt(headTrackIdx);
        int lastStatic = Mathf.FloorToInt(tailTrackIdx);

        for (int i = firstStatic; i <= lastStatic; i++)
        {
            if (i > headTrackIdx + 0.001f && i < tailTrackIdx - 0.001f)
            {
                if (i < _totalPoints) _renderPointsCache.Add(GetPositionAtTrackIndex(i)); 
                if (i < _totalPoints) _renderTrackIdxCache.Add(i);
            }
        }

        if (tailTrackIdx > headTrackIdx + 0.001f)
        {
            _renderPointsCache.Add(GetPositionAtTrackIndex(tailTrackIdx));
            _renderTrackIdxCache.Add(tailTrackIdx);
        }

        // Deterministic portal split points: if a warp boundary lies inside the visible track range,
        // always insert portal-center and exit-center nodes at that boundary.
        if (_activeWarps != null && _activeWarps.Count > 0 && _renderPointsCache.Count > 1)
        {
            const float epsilonTrack = 0.0001f;
            for (int w = 0; w < _activeWarps.Count; w++)
            {
                if (!_activeWarps[w].isPortal) continue;
                float warpTrackIdx = -_activeWarps[w].rawDistFromHead0 * _nodesPerUnit;
                if (warpTrackIdx <= headTrackIdx + epsilonTrack || warpTrackIdx >= tailTrackIdx - epsilonTrack)
                    continue;

                int insertAt = _renderTrackIdxCache.Count;
                for (int k = 0; k < _renderTrackIdxCache.Count; k++)
                {
                    if (_renderTrackIdxCache[k] > warpTrackIdx)
                    {
                        insertAt = k;
                        break;
                    }
                }

                Vector3 portalCenter = _activeWarps[w].portalWorldPos;
                portalCenter.z = _renderPointsCache[0].z;
                Vector3 exitCenter = _activeWarps[w].exitWorldPos;
                exitCenter.z = _renderPointsCache[0].z;

                _renderTrackIdxCache.Insert(insertAt, warpTrackIdx - epsilonTrack);
                _renderPointsCache.Insert(insertAt, exitCenter);

                _renderTrackIdxCache.Insert(insertAt + 1, warpTrackIdx + epsilonTrack);
                _renderPointsCache.Insert(insertAt + 1, portalCenter);
            }
        }

        // IMPORTANT: Split first, then smooth each segment.
        // Smoothing across a teleport gap (portal) can make the tail end at the rim instead of the center,
        // and produces incorrect extra LineSegment_* geometry.
        int visualSegmentCount = 0;
        List<Vector3> currentSegment = GetReusableVisualSegment(visualSegmentCount);

        for (int i = 0; i < _renderPointsCache.Count; i++)
        {
            if (currentSegment.Count == 0)
            {
                currentSegment.Add(_renderPointsCache[i]);
            }
            else
            {
                float dist = Vector3.Distance(currentSegment[currentSegment.Count - 1], _renderPointsCache[i]);
                if (dist > 1.5f)
                {
                    if (currentSegment.Count > 1) visualSegmentCount++;
                    currentSegment = GetReusableVisualSegment(visualSegmentCount);
                }
                currentSegment.Add(_renderPointsCache[i]);
            }
        }
        if (currentSegment.Count > 1) visualSegmentCount++;

        // Smooth each segment independently (never across portal gap).
        if (cornerRadius > 0f)
        {
            for (int s = 0; s < visualSegmentCount; s++)
            {
                List<Vector3> segment = _visualSegmentsCache[s];
                if (segment.Count <= 2) continue;

                BuildSmoothedPositionsForRenderCached(segment, _smoothedPointsCache);
                segment.Clear();
                for (int p = 0; p < _smoothedPointsCache.Count; p++)
                {
                    segment.Add(_smoothedPointsCache[p]);
                }
            }
        }

        EnsureLineRenderersCount(visualSegmentCount);

        for (int i = 0; i < _lineRenderers.Count; i++)
        {
            if (i > 0 && _lineRenderers[i] != null)
            {
                _lineRenderers[i].widthMultiplier = lineRenderer.widthMultiplier;
            }

            if (i < visualSegmentCount && _visualSegmentsCache[i].Count > 1)
            {
                if (!_lineRenderers[i].gameObject.activeSelf) _lineRenderers[i].gameObject.SetActive(true);
                _lineRenderers[i].enabled = true;
                ApplyCachedLinePositions(_lineRenderers[i], i, _visualSegmentsCache[i]);
            }
            else
            {
                _lineRenderers[i].positionCount = 0;
                _lineRenderers[i].enabled = false;
                if (i > 0) _lineRenderers[i].gameObject.SetActive(false);
            }
        }
    }

    private List<Vector3> GetReusableVisualSegment(int index)
    {
        while (_visualSegmentsCache.Count <= index)
        {
            _visualSegmentsCache.Add(new List<Vector3>(32));
        }

        List<Vector3> segment = _visualSegmentsCache[index];
        segment.Clear();
        return segment;
    }

    private void ApplyCachedLinePositions(LineRenderer targetRenderer, int segmentIndex, List<Vector3> positions)
    {
        int pointCount = positions.Count;
        targetRenderer.positionCount = pointCount;

        Vector3[] buffer = GetLinePositionsArray(segmentIndex, pointCount);
        for (int i = 0; i < pointCount; i++)
        {
            buffer[i] = positions[i];
        }

        targetRenderer.SetPositions(buffer);
    }

    private Vector3[] GetLinePositionsArray(int segmentIndex, int pointCount)
    {
        while (_linePositionsArrayCache.Count <= segmentIndex)
        {
            _linePositionsArrayCache.Add(null);
        }

        Vector3[] buffer = _linePositionsArrayCache[segmentIndex];
        if (buffer == null || buffer.Length != pointCount)
        {
            buffer = new Vector3[pointCount];
            _linePositionsArrayCache[segmentIndex] = buffer;
        }

        return buffer;
    }

    private void BuildSmoothedPositionsForRenderCached(List<Vector3> input, List<Vector3> output)
    {
        output.Clear();
        if (input.Count < 3) { output.AddRange(input); return; }

        output.Add(input[0]);
        float angleThreshold = 15f;

        for (int i = 1; i < input.Count - 1; i++)
        {
            Vector3 prev = input[i - 1];
            Vector3 curr = input[i];
            Vector3 next = input[i + 1];

            Vector3 dirIn = (curr - prev);
            Vector3 dirOut = (next - curr);

            if (dirIn.sqrMagnitude > 2.25f || dirOut.sqrMagnitude > 2.25f || dirIn.sqrMagnitude < 0.0001f || dirOut.sqrMagnitude < 0.0001f)
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
            else output.Add(curr);
        }
        output.Add(input[input.Count - 1]);
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
