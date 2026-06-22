                using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using DG.Tweening;

[RequireComponent(typeof(LineRenderer))]
public class SnakeBlock : MonoBehaviour, IGridOccupant
{
    public static IReadOnlyList<SnakeBlock> ActiveSnakes => SnakeRuntime.ActiveSnakes;

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
    [SerializeField] private float blackHoleConsumeShrinkDuration = 0.24f;
    [SerializeField] private float consecutiveReleaseTimeout = 2.0f;
    [SerializeField] private float hitFlashDuration = 0.3f;
    [SerializeField] private float autoReleaseDelay = 0.25f;

    [Header("Movement: DASH EXIT")]
    [SerializeField] private float dashExitStartSpeed = 40f;
    [SerializeField] private float dashExitMaxSpeed = 520f;
    [SerializeField] private float dashExitAcceleration = 260f;

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

    [Header("Arrow Shadow")]
    [SerializeField] private int arrowShadowTurnsToFade = 3;
    [SerializeField, Range(0.05f, 1f)] private float arrowShadowAlpha = 0.68f;
    [SerializeField, Min(1f)] private float arrowShadowWidthMultiplier = 2.8f;
    [SerializeField, Min(1f)] private float arrowShadowHeadScaleMultiplier = 1.65f;

    private SnakeRuntime _runtime;
    private SnakeMover _mover;
    private SnakeRenderer2D _renderer2D;
    private SnakeInteractions _interactions;
    private float _autoReleaseTimer = 0f;

    public bool IsMoving => _runtime != null && _runtime.IsMoving;
    public bool HasCollided => _runtime != null && _runtime.HasCollided;
    public string LastObstacleType => _runtime != null ? _runtime.LastObstacleType.ToString() : ObstacleHitType.None.ToString();
    public Vector2Int LastObstacleCell => _runtime != null ? _runtime.LastObstacleCell : new Vector2Int(int.MinValue, int.MinValue);
    public bool IsStoppedByStopBlock => _runtime != null && _runtime.IsStoppedByStopBlock;
    public bool HasArrowShadow => _runtime != null && _runtime.HasArrowShadow;
    public List<Vector3> LogicNodes => _runtime != null ? _runtime.LogicNodes : null;
    public Vector3 HeadPosition => _runtime != null ? _runtime.GetHeadPosition(transform.position, direction) : transform.position;
    public Vector2Int GridPosition => new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
    public bool IsActiveOccupant => this != null && gameObject != null && isActiveAndEnabled;

    internal float StartMoveSpeed => startMoveSpeed;
    internal float MaxMoveSpeed => maxMoveSpeed;
    internal float Acceleration => acceleration;
    internal float ReturnMoveSpeed => returnMoveSpeed;
    internal float ExitStartSpeed => exitStartSpeed;
    internal float ExitMaxSpeed => exitMaxSpeed;
    internal float ExitAcceleration => exitAcceleration;
    internal float ExitTravelDistance => exitTravelDistance;
    internal int MaxPathScanCells => maxPathScanCells;
    internal float BlackHoleConsumeShrinkDuration => blackHoleConsumeShrinkDuration;
    internal float ConsecutiveReleaseTimeout => consecutiveReleaseTimeout;
    public float HitFlashDuration => hitFlashDuration;
    internal float DashExitStartSpeed => dashExitStartSpeed;
    internal float DashExitMaxSpeed => dashExitMaxSpeed;
    internal float DashExitAcceleration => dashExitAcceleration;
    internal float CornerRadius => cornerRadius;
    internal int CornerSmoothSteps => cornerSmoothSteps;
    internal float SpawnSpeed => spawnSpeed;
    internal Transform ArrowVisual => arrowVisual;
    internal Material LinePressedMaterial => linePressedMaterial;
    internal float StopBlockAlpha => stopBlockAlpha;
    internal int ArrowShadowTurnsToFade => arrowShadowTurnsToFade;
    internal float ArrowShadowAlpha => arrowShadowAlpha;
    internal float ArrowShadowWidthMultiplier => arrowShadowWidthMultiplier;
    internal float ArrowShadowHeadScaleMultiplier => arrowShadowHeadScaleMultiplier;

    private void Awake()
    {
        _runtime = new SnakeRuntime();
        _interactions = new SnakeInteractions();
        _renderer2D = new SnakeRenderer2D(this, _runtime);
        _mover = new SnakeMover(this, _runtime, _renderer2D, _interactions);
        _renderer2D.SetupLineRenderer();
    }

    private void OnEnable()
    {
        SnakeRuntime.Register(this);
    }

    private void OnDisable()
    {
        SnakeRuntime.Unregister(this);
    }

    private void Start()
    {
        _mover?.SetLevelController(FindObjectOfType<LevelController>());
    }

    private void OnDestroy()
    {
        _mover?.OnOwnerDestroyed();
        _renderer2D?.OnOwnerDestroyed();
        _runtime?.ClearFromGrid(this);
        DOTween.Kill(GetInstanceID());
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;
        if (GameplayInputLock.IsLocked) return;
        if (EraseManager.Instance != null && (EraseManager.Instance.IsEraseModeActive || EraseManager.Instance.IsExecutingErase)) return;

        if (HasCollided)
        {
            if (CanReleaseNow())
            {
                _autoReleaseTimer += Time.deltaTime;
                if (_autoReleaseTimer >= autoReleaseDelay)
                {
                    _autoReleaseTimer = 0f;
                    if (OnHeadClicked())
                    {
                        if (AudioManager.Instance != null)
                        {
                            AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowTap, 0.8f);
                        }
                    }
                }
            }
            else
            {
                _autoReleaseTimer = 0f;
            }
        }
        else
        {
            _autoReleaseTimer = 0f;
        }
    }

    private void LateUpdate()
    {
        _renderer2D?.RedrawIfNeeded();
    }

    public void Initialize(ArrowDir dir, List<Vector2Int> gridPositions, int resolution, Color color, bool playSpawnAnimation = true, bool hasArrowShadow = false)
    {
        snakeColor = color;
        direction = dir;

        _runtime.Initialize(gridPositions, resolution, hasArrowShadow);
        _mover.ResetForInitializedSnake();
        _renderer2D.InitializeVisualState(color, direction);
        _runtime.UpdateGridOccupancy(this, _interactions);
        _renderer2D.CreateOrRefreshArrowShadow(direction, snakeColor);
        _renderer2D.PrepareArrowForSpawn();

        if (playSpawnAnimation)
            StartSpawnAnimationFromTail();
    }

    public void SetColorImmediatePublic(Color color)
    {
        SetColorImmediate(color);
    }

    public void SetColorImmediate(Color color)
    {
        _renderer2D?.SetColorImmediate(color);
    }

    public void FlashRed()
    {
        _renderer2D?.FlashRed(hitFlashDuration);
    }

    public void SetFocusEffect(bool isFocused, float scaleFactor, float duration)
    {
        _renderer2D?.SetFocusEffect(isFocused, scaleFactor, duration);
    }

    public void SetFocusColor(bool isFocusing, float duration)
    {
        _renderer2D?.SetFocusColor(isFocusing, duration);
    }

    public void PlayDashReadyVisual(Color highlightColor, float scaleFactor, float duration)
    {
        _renderer2D?.PlayDashReadyVisual(highlightColor, scaleFactor, duration);
    }

    public void BeginHintGlowVisual(float scaleFactor, float duration)
    {
        _renderer2D?.BeginHintGlowVisual(scaleFactor, duration);
    }

    public void EndHintGlowVisual(Color restoreColor, float duration)
    {
        _renderer2D?.EndHintGlowVisual(restoreColor, duration);
    }

    public void BeginEraseVisual()
    {
        _renderer2D?.BeginEraseVisual();
    }

    public void EraseVisualAtWorldPosition(Vector3 eraserWorldPosition, float brushRadius)
    {
        _renderer2D?.EraseVisualAtWorldPosition(eraserWorldPosition, brushRadius);
    }

    public void CompleteEraseVisual()
    {
        _renderer2D?.CompleteEraseVisual();
    }

    public bool IsVisualAlphaZero(float threshold = 0.01f)
    {
        return _renderer2D != null && _renderer2D.IsVisualAlphaZero(threshold);
    }

    public void FlashIfVisualAlphaZero(float flashAlpha, float duration, Color flashTint, float threshold = 0.01f)
    {
        _renderer2D?.FlashIfVisualAlphaZero(flashAlpha, duration, flashTint, threshold);
    }

    public bool OnHeadClicked()
    {
        return _mover != null && _mover.TryReleaseHead();
    }

    public bool CanReleaseNow()
    {
        return _mover != null && _mover.CanReleaseNow();
    }

    public void ForceDashRelease(bool keepCurrentVisual = true)
    {
        _mover?.ForceDashRelease(keepCurrentVisual);
    }

    public void ForceDashExit(bool keepCurrentVisual = false)
    {
        ForceDashRelease(keepCurrentVisual);
    }

    public void ForceSpinRelease(bool keepCurrentVisual = true)
    {
        _mover?.ForceSpinRelease(keepCurrentVisual);
    }

    public void ForceResetToOrigin()
    {
        _mover?.ForceResetToOrigin();
    }

    public void ReleaseFromStopBlock(GridStopBlock stopBlock)
    {
        _mover?.ReleaseFromStopBlock(stopBlock);
    }

    public void StartSpawnAnimationFromTail()
    {
        _mover?.StartSpawnAnimationFromTail();
    }

    public void UpdateVisualRotation()
    {
        _renderer2D?.UpdateVisualRotation(direction);
    }

    public void SetArrowShadowEnabled(bool enabled)
    {
        if (_runtime == null) return;

        _runtime.SetHasArrowShadow(enabled);
        _renderer2D?.CreateOrRefreshArrowShadow(direction, snakeColor);
    }
}
