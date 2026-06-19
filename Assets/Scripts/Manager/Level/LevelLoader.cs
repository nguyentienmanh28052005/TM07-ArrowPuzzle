using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour, IScreenLifecycle
{
    [Header("Data")]
    public LevelDataV2 levelToPlay;

    [Header("Prefabs (Data-Driven)")]
    public GameObject snakePrefab; 
    public GameObject dotPrefab;   
    public GameObject keycardPrefab;
    public GameObject gatePrefab;
    public GameObject electricButtonPrefab;
    public GameObject revealWaveButtonPrefab;
    public GameObject electricWallPrefab;
    public GameObject portalPrefab;
    public GameObject deflectorPrefab;
    public GameObject countdownBlockPrefab;
    public GameObject stopBlockPrefab;
    public GameObject turnStateBlockPrefab;
    public GameObject blackHolePrefab;

    [Header("Container")]
    public Transform gameContainer;

    [Header("Resolution Settings")]
    [Range(0, 20)]
    public int subNodesCount = 8;

    [Header("Optimization")]
    public int snakesPerFrame = 2; 
    public int dotsPerFrame = 15; 

    [Header("Transition Sync")]
    [SerializeField] private float snakeSpawnLeadBeforeTransitionRelease = 0.12f;

    [Header("Portal Visual")]
    [Tooltip("Optional material override for the temporary hole ripple effect spawned by GridPortalVisual.")]
    [SerializeField] private Material holeEffectMaterialOverride;

    public bool editorMode = false;

    private List<SnakeBlock> _preloadedSnakes = new List<SnakeBlock>();
    private List<ArrowEntityData> _preloadedArrowData = new List<ArrowEntityData>();
    private GameObject _dotsContainer; 
    private GridDotBatchRenderer _dotBatchRenderer;
    private GameObject _obstaclesContainer;
    private Coroutine _loadRoutine;
    private bool _requestedTransitionHold;
    private int _loadingTotalSteps;
    private int _loadingCompletedSteps;

    public void OnScreenShow()
    {
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.ResetComboImmediate();
        }

        if (TimeAttackManager.Instance != null)
        {
            TimeAttackManager.Instance.ResetTimer();
        }

        if (_loadRoutine != null)
        {
            StopCoroutine(_loadRoutine);
        }

        _loadRoutine = StartCoroutine(LoadRoutine());
    }

    public void OnScreenHide()
    {
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.ResetComboImmediate();
        }

        if (TimeAttackManager.Instance != null)
        {
            TimeAttackManager.Instance.ResetTimer();
        }

        if (_loadRoutine != null)
        {
            StopCoroutine(_loadRoutine);
            _loadRoutine = null;
        }

        ReleaseTransitionHold();

        StopAllCoroutines();
        ClearContainer();
    }

    private IEnumerator LoadRoutine()
    {
        GameplayInputLock.ClearAll();
        editorMode = PlaytestSession.IsPlaytesting;

        if (PlaytestSession.IsPlaytesting)
        {
            levelToPlay = PlaytestSession.LevelData;
            PlaytestExitListener.EnsureExists();
        }

        if (!editorMode && GameManager.Instance != null)
            levelToPlay = GameManager.Instance.GetCurrentLevelData();

        if (levelToPlay == null)
        {
            ReleaseTransitionHold();
            GameplayInputLock.SetLock(GameplayLockReason.LevelLoading, false);
            yield break;
        }

        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.ResetForLevel(levelToPlay);
        }

        if (GridManager.Instance != null)
        {
            GridManager.Instance.ClearLevelState();
            GridManager.Instance.InitializeLevelGrid(levelToPlay);
        }
        GridPortalVisual.ClearAll();
        GridDeflectorVisual.ClearAll();
        GridDot.GridMap.Clear();

        GameplayInputLock.SetLock(GameplayLockReason.LevelLoading, true);
        RequestTransitionHold();
        PrepareLoadingProgress();

        GameCanvas canvas = FindObjectOfType<GameCanvas>();
        if (canvas != null && levelToPlay != null)
        {
            canvas.SetupModeUI(levelToPlay.gameMode);
        }

        // --- TRONG LÚC TRANSITION FADE ĐANG CHẠY, GAME ÂM THẦM ĐẺ OBJECT ---
        SpawnStaticObstacles();
        yield return StartCoroutine(PreSpawnDotsCoroutine());
        yield return StartCoroutine(PreSpawnSnakesCoroutine());

        ActivateAndInitializeSnakes();

        FinishLoadingProgress();

        if (TimeAttackManager.Instance != null)
        {
            TimeAttackManager.Instance.InitializeTimer(levelToPlay.timeLimit);
        }

        CameraController camController = Camera.main != null ? Camera.main.GetComponent<CameraController>() : null;
        if (camController != null)
        {
            camController.PrepareDefaultForLevel(levelToPlay);
            camController.StartIntro();
        }

        StartSpawnAnimationOnSnakes();

        if (IsTransitionActive() && snakeSpawnLeadBeforeTransitionRelease > 0f)
        {
            yield return new WaitForSecondsRealtime(snakeSpawnLeadBeforeTransitionRelease);
        }

        ReleaseTransitionHold();

        // TRẠM KIỂM SOÁT: Bắt buộc đợi Transition Fade kết thúc mới cho đi tiếp!
        while (IsTransitionActive())
        {
            yield return null;
        }

        GameplayInputLock.SetLock(GameplayLockReason.LevelLoading, false);

        if (TutorialManager.Instance != null) 
            TutorialManager.Instance.CheckAndStartTutorial(levelToPlay);

        if (BoosterTutorialManager.Instance != null)
            BoosterTutorialManager.Instance.CheckAndStartBoosterTutorial(levelToPlay);
    }

    [ContextMenu("Reload Level (Instant)")]
    public void LoadGame()
    {
        GameplayInputLock.ClearAll();
        ClearContainer();
        if (GridManager.Instance != null && levelToPlay != null)
        {
            GridManager.Instance.InitializeLevelGrid(levelToPlay);
        }
        SpawnStaticObstacles();
        
        if (levelToPlay != null && levelToPlay.arrows != null)
        {
            foreach (ArrowEntityData arrowData in LevelDataV2Queries.GetStandardArrows(levelToPlay))
            {
                if (dotPrefab != null)
                {
                    GridDotBatchRenderer dotBatchRenderer = EnsureDotBatchRenderer();

                    if (arrowData.segmentPositions == null) continue;

                    for (int i = 0; i < arrowData.segmentPositions.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            Vector2Int pos = arrowData.segmentPositions[i];
                            dotBatchRenderer.RegisterDot(pos);
                        }
                    }
                }
                PreSpawnSingleSnake(arrowData);
            }
        }

        if (_dotBatchRenderer != null)
        {
            _dotBatchRenderer.RebuildMesh();
        }
        
        ActivateAndInitializeSnakes();
        StartSpawnAnimationOnSnakes();
    }

    private void ClearContainer()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.ClearLevelState();
        }
        GridPortalVisual.ClearAll();
        GridDeflectorVisual.ClearAll();

        if (gameContainer != null)
        {
            int childCount = gameContainer.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject child = gameContainer.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
        }
        
        if (_dotsContainer != null) 
        {
            if (Application.isPlaying) Destroy(_dotsContainer);
            else DestroyImmediate(_dotsContainer);
            _dotsContainer = null;
            _dotBatchRenderer = null;
        }

        if (_obstaclesContainer != null) 
        {
            if (Application.isPlaying) Destroy(_obstaclesContainer);
            else DestroyImmediate(_obstaclesContainer);
            _obstaclesContainer = null;
        }

        _preloadedSnakes.Clear();
        _preloadedArrowData.Clear();
    }

    private void SpawnStaticObstacles()
    {
        if (levelToPlay == null) return;

        if (_obstaclesContainer == null)
        {
            _obstaclesContainer = new GameObject("Obstacles_Container");
            _obstaclesContainer.transform.SetParent(gameContainer);
            _obstaclesContainer.SetActive(false); 
        }

        LevelRuntimeFactoryV2 factory = CreateRuntimeFactory();

        if (levelToPlay.cells != null)
        {
            foreach (CellEntityData cell in levelToPlay.cells)
            {
                factory.CreateCell(cell, _obstaclesContainer.transform);
            }
        }

        if (levelToPlay.links != null)
        {
            foreach (LinkEntityData link in levelToPlay.links)
            {
                factory.CreateLink(link, levelToPlay, _obstaclesContainer.transform);
            }
        }
    }

    private LevelRuntimeFactoryV2 CreateRuntimeFactory()
    {
        return new LevelRuntimeFactoryV2(
            keycardPrefab,
            gatePrefab,
            electricButtonPrefab,
            revealWaveButtonPrefab,
            electricWallPrefab,
            portalPrefab,
            deflectorPrefab,
            countdownBlockPrefab,
            stopBlockPrefab,
            turnStateBlockPrefab,
            blackHolePrefab,
            holeEffectMaterialOverride);
    }

    private IEnumerator PreSpawnDotsCoroutine()
    {
        if (levelToPlay == null || levelToPlay.arrows == null || dotPrefab == null) yield break;

        GridDotBatchRenderer dotBatchRenderer = EnsureDotBatchRenderer();

        int dotsSpawnedThisFrame = 0;

        foreach (ArrowEntityData arrowData in LevelDataV2Queries.GetStandardArrows(levelToPlay))
        {
            if (arrowData.segmentPositions == null) continue;

            for (int i = 0; i < arrowData.segmentPositions.Count; i++)
            {
                if (i % 2 == 0)
                {
                    Vector2Int pos = arrowData.segmentPositions[i];
                    dotBatchRenderer.RegisterDot(pos);

                    IncrementLoadingProgress();
                    
                    dotsSpawnedThisFrame++;

                    if (IsTransitionActive() && dotsSpawnedThisFrame >= dotsPerFrame)
                    {
                        dotsSpawnedThisFrame = 0;
                        yield return null; 
                    }
                }
            }
        }

        dotBatchRenderer.RebuildMesh();
    }

    private GridDotBatchRenderer EnsureDotBatchRenderer()
    {
        if (_dotBatchRenderer != null) return _dotBatchRenderer;

        if (_dotsContainer == null)
        {
            _dotsContainer = new GameObject("Dots_Container");
            _dotsContainer.transform.SetParent(gameContainer, false);
        }

        _dotBatchRenderer = _dotsContainer.GetComponent<GridDotBatchRenderer>();
        if (_dotBatchRenderer == null)
        {
            _dotBatchRenderer = _dotsContainer.AddComponent<GridDotBatchRenderer>();
        }

        _dotBatchRenderer.ConfigureFromPrefab(dotPrefab);
        return _dotBatchRenderer;
    }

    private IEnumerator PreSpawnSnakesCoroutine()
    {
        if (levelToPlay == null || levelToPlay.arrows == null) yield break;

        int arrowIndex = 0;
        foreach (ArrowEntityData arrowData in LevelDataV2Queries.GetStandardArrows(levelToPlay))
        {
            PreSpawnSingleSnake(arrowData);
            IncrementLoadingProgress();

            arrowIndex++;
            if (IsTransitionActive() && arrowIndex % snakesPerFrame == 0)
            {
                yield return null; 
            }
        }
    }

    private void PreSpawnSingleSnake(ArrowEntityData arrowData)
    {
        if (arrowData == null || arrowData.segmentPositions == null || arrowData.segmentPositions.Count == 0) return;

        GameObject snakeObj = Instantiate(snakePrefab, gameContainer);
        snakeObj.name = "Snake_Preloaded";
        
        SnakeBlock snakeScript = snakeObj.GetComponent<SnakeBlock>();
        
        _preloadedSnakes.Add(snakeScript);
        _preloadedArrowData.Add(arrowData);
    }

    private void ActivateAndInitializeSnakes()
    {
        int resolution = subNodesCount + 1;

        for (int i = 0; i < _preloadedSnakes.Count; i++)
        {
            SnakeBlock snakeScript = _preloadedSnakes[i];
            ArrowEntityData data = _preloadedArrowData[i];
            StandardArrowPayload payload = data.payload as StandardArrowPayload;

            snakeScript.Initialize(data.direction, data.segmentPositions, resolution, data.color, false, payload != null && payload.hasArrowShadow);

            if (GridManager.Instance != null) 
                GridManager.Instance.RegisterSnake(snakeScript);
        }

        if (_obstaclesContainer != null)
        {
            _obstaclesContainer.SetActive(true);
        }

    }

    private void StartSpawnAnimationOnSnakes()
    {
        if (_preloadedSnakes == null || _preloadedSnakes.Count == 0) return;

        for (int i = 0; i < _preloadedSnakes.Count; i++)
        {
            SnakeBlock snake = _preloadedSnakes[i];
            if (snake == null) continue;
            snake.StartSpawnAnimationFromTail();
        }

        _preloadedSnakes.Clear();
        _preloadedArrowData.Clear();
    }

    private static Quaternion GetRotationForDir(ArrowDir dir)
    {
        float angle = 0f;
        switch (dir)
        {
            case ArrowDir.Up: angle = 0f; break;
            case ArrowDir.Down: angle = 180f; break;
            case ArrowDir.Left: angle = 90f; break;
            case ArrowDir.Right: angle = -90f; break;
        }
        return Quaternion.Euler(0f, 0f, angle);
    }

    private void PrepareLoadingProgress()
    {
        int dotsToSpawn = 0;
        int snakesToSpawn = 0;

        if (levelToPlay != null && levelToPlay.arrows != null)
        {
            foreach (ArrowEntityData arrow in LevelDataV2Queries.GetStandardArrows(levelToPlay))
            {
                snakesToSpawn++;
                if (arrow == null || arrow.segmentPositions == null) continue;
                dotsToSpawn += (arrow.segmentPositions.Count + 1) / 2;
            }
        }

        _loadingCompletedSteps = 0;
        _loadingTotalSteps = Mathf.Max(1, dotsToSpawn + snakesToSpawn);
        ReportLoadingProgress();
    }

    private void IncrementLoadingProgress()
    {
        if (_loadingCompletedSteps < _loadingTotalSteps)
        {
            _loadingCompletedSteps++;
        }

        ReportLoadingProgress();
    }

    private void FinishLoadingProgress()
    {
        _loadingCompletedSteps = _loadingTotalSteps;
        ReportLoadingProgress();
    }

    private void ReportLoadingProgress()
    {
        if (TransitionManager.Instance == null) return;

        float normalized = (float)_loadingCompletedSteps / _loadingTotalSteps;
        TransitionManager.Instance.SetLoadingProgress(normalized);
    }

    private void RequestTransitionHold()
    {
        if (_requestedTransitionHold) return;
        if (TransitionManager.Instance == null) return;

        TransitionManager.Instance.RequestHold();
        _requestedTransitionHold = true;
    }

    private void ReleaseTransitionHold()
    {
        if (!_requestedTransitionHold) return;

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.ReleaseHold();
        }

        _requestedTransitionHold = false;
    }

    private static bool IsTransitionActive()
    {
        return TransitionManager.Instance != null && TransitionManager.Instance.IsTransitioning;
    }
}
