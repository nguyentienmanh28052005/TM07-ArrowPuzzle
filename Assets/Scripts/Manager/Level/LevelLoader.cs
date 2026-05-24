using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour, IScreenLifecycle
{
    [Header("Data")]
    public LevelDataSO levelToPlay;

    [Header("Prefabs (Data-Driven)")]
    public GameObject snakePrefab; 
    public GameObject dotPrefab;   
    public GameObject keycardPrefab;
    public GameObject gatePrefab;
    public GameObject electricButtonPrefab;
    public GameObject electricWallPrefab;
    public GameObject portalPrefab;
    public GameObject deflectorPrefab;
    public GameObject countdownBlockPrefab;

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

    public bool editorMode = false;

    private List<SnakeBlock> _preloadedSnakes = new List<SnakeBlock>();
    private List<SnakeSaveData> _preloadedSnakeSaveData = new List<SnakeSaveData>();
    private GameObject _dotsContainer; 
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
        if (PlaytestSession.IsPlaytesting)
        {
            editorMode = true;
            levelToPlay = PlaytestSession.LevelData;
            PlaytestExitListener.EnsureExists();
        }

        if (!editorMode && GameManager.Instance != null)
            levelToPlay = GameManager.Instance.GetCurrentLevelData();

        if (levelToPlay == null)
        {
            ReleaseTransitionHold();
            CameraController.IsGameplayBlocking = false;
            yield break;
        }

        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.ResetForLevel(levelToPlay);
        }

        if (GridManager.Instance != null)
        {
            GridManager.Instance.ClearLevelState();
        }
        GridPortalVisual.ClearAll();
        GridDeflectorVisual.ClearAll();

        CameraController.IsGameplayBlocking = true;
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

        if (camController == null) 
            CameraController.IsGameplayBlocking = false;

        if (TutorialManager.Instance != null) 
            TutorialManager.Instance.CheckAndStartTutorial(levelToPlay);

        if (BoosterTutorialManager.Instance != null)
            BoosterTutorialManager.Instance.CheckAndStartBoosterTutorial(levelToPlay);
    }

    [ContextMenu("Reload Level (Instant)")]
    public void LoadGame()
    {
        ClearContainer();
        SpawnStaticObstacles();
        
        if (levelToPlay != null && levelToPlay.snakes != null)
        {
            foreach (var snakeData in levelToPlay.snakes)
            {
                if (dotPrefab != null)
                {
                    if (_dotsContainer == null)
                    {
                        _dotsContainer = new GameObject("Dots_Container");
                        _dotsContainer.transform.SetParent(gameContainer);
                    }

                    for (int i = 0; i < snakeData.segmentPositions.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            Vector2Int pos = snakeData.segmentPositions[i];
                            Instantiate(dotPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, _dotsContainer.transform);
                        }
                    }
                }
                PreSpawnSingleSnake(snakeData);
            }
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
        }

        if (_obstaclesContainer != null) 
        {
            if (Application.isPlaying) Destroy(_obstaclesContainer);
            else DestroyImmediate(_obstaclesContainer);
            _obstaclesContainer = null;
        }

        _preloadedSnakes.Clear();
        _preloadedSnakeSaveData.Clear();
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

        if (levelToPlay.keycards != null)
        {
            foreach (var k in levelToPlay.keycards)
            {
                GameObject obj = Instantiate(keycardPrefab, new Vector3(k.position.x, k.position.y, 0), Quaternion.identity, _obstaclesContainer.transform);
                if (obj.TryGetComponent(out GridKeycard script)) script.keyColor = k.color;
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = k.color;
            }
        }

        if (levelToPlay.gates != null)
        {
            foreach (var g in levelToPlay.gates)
            {
                GameObject obj = Instantiate(gatePrefab, new Vector3(g.position.x, g.position.y, 0), Quaternion.identity, _obstaclesContainer.transform);
                if (obj.TryGetComponent(out GridLaserGate script)) script.gateColor = g.color;
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = g.color;
            }
        }

        if (levelToPlay.electricButtons != null && electricButtonPrefab != null)
        {
            foreach (var b in levelToPlay.electricButtons)
            {
                GameObject obj = Instantiate(electricButtonPrefab, new Vector3(b.position.x, b.position.y, 0), Quaternion.identity, _obstaclesContainer.transform);
                if (obj.TryGetComponent(out GridElectricButton script)) script.SetColor(b.color);
            }
        }

        if (levelToPlay.electricWalls != null && electricWallPrefab != null)
        {
            foreach (var w in levelToPlay.electricWalls)
            {
                GameObject obj = Instantiate(electricWallPrefab, Vector3.zero, Quaternion.identity, _obstaclesContainer.transform);
                GridElectricWall wall = obj.GetComponent<GridElectricWall>();
                if (wall != null) wall.Initialize(w.start, w.end, w.color, true);
            }
        }

        if (levelToPlay.deflectors != null && deflectorPrefab != null)
        {
            foreach (var d in levelToPlay.deflectors)
            {
                GameObject obj = Instantiate(deflectorPrefab, new Vector3(d.position.x, d.position.y, 0), GetRotationForDir(d.direction), _obstaclesContainer.transform);
                GridDeflector deflector = obj.GetComponentInChildren<GridDeflector>();
                if (deflector != null) deflector.SetDirection(d.direction);
                if (obj.GetComponent<GridDeflectorVisual>() == null) obj.AddComponent<GridDeflectorVisual>();
            }
        }

        if (levelToPlay.countdownBlocks != null && countdownBlockPrefab != null)
        {
            foreach (var cb in levelToPlay.countdownBlocks)
            {
                GameObject obj = Instantiate(countdownBlockPrefab, new Vector3(cb.position.x, cb.position.y, 0), Quaternion.identity, _obstaclesContainer.transform);
                GridCountdownBlock script = obj.GetComponent<GridCountdownBlock>();
                if (script != null) script.SetCount(cb.count);
            }
        }

        if (levelToPlay.portals != null)
        {
            for (int i = 0; i < levelToPlay.portals.Count; i++)
            {
                var p = levelToPlay.portals[i];

                if (GridManager.Instance != null)
                {
                    GridManager.Instance.PortalMap[p.entrance] = new GridManager.PortalLink { exit = p.exit, exitDir = p.exitDir };
                    GridManager.Instance.PortalMap[p.exit] = new GridManager.PortalLink { exit = p.entrance, exitDir = p.entranceDir };
                }

                if (portalPrefab != null)
                {
                    GameObject inObj = Instantiate(portalPrefab, new Vector3(p.entrance.x, p.entrance.y, 0), GetRotationForDir(p.entranceDir), _obstaclesContainer.transform);
                    if (inObj.GetComponent<GridPortalVisual>() == null) inObj.AddComponent<GridPortalVisual>();
                    SpriteRenderer inSr = inObj.GetComponent<SpriteRenderer>();
                    if (inSr != null) inSr.color = p.portalColor;

                    GameObject outObj = Instantiate(portalPrefab, new Vector3(p.exit.x, p.exit.y, 0), GetRotationForDir(p.exitDir), _obstaclesContainer.transform);
                    if (outObj.GetComponent<GridPortalVisual>() == null) outObj.AddComponent<GridPortalVisual>();
                    SpriteRenderer outSr = outObj.GetComponent<SpriteRenderer>();
                    if (outSr != null) outSr.color = p.portalColor;
                }
            }
        }
    }

    private IEnumerator PreSpawnDotsCoroutine()
    {
        if (levelToPlay == null || levelToPlay.snakes == null || dotPrefab == null) yield break;

        if (_dotsContainer == null)
        {
            _dotsContainer = new GameObject("Dots_Container");
            _dotsContainer.transform.SetParent(gameContainer);
        }

        int dotsSpawnedThisFrame = 0;

        foreach (var snakeData in levelToPlay.snakes)
        {
            for (int i = 0; i < snakeData.segmentPositions.Count; i++)
            {
                if (i % 2 == 0)
                {
                    Vector2Int pos = snakeData.segmentPositions[i];
                    Vector3 currentPos = new Vector3(pos.x, pos.y, 0);
                    Instantiate(dotPrefab, currentPos, Quaternion.identity, _dotsContainer.transform);

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
    }

    private IEnumerator PreSpawnSnakesCoroutine()
    {
        if (levelToPlay == null || levelToPlay.snakes == null) yield break;

        for (int i = 0; i < levelToPlay.snakes.Count; i++)
        {
            PreSpawnSingleSnake(levelToPlay.snakes[i]);
            IncrementLoadingProgress();

            if (IsTransitionActive() && (i + 1) % snakesPerFrame == 0)
            {
                yield return null; 
            }
        }
    }

    private void PreSpawnSingleSnake(SnakeSaveData SnakeSaveData)
    {
        if (SnakeSaveData.segmentPositions.Count == 0) return;

        GameObject snakeObj = Instantiate(snakePrefab, gameContainer);
        snakeObj.name = "Snake_Preloaded";
        
        SnakeBlock snakeScript = snakeObj.GetComponent<SnakeBlock>();
        
        _preloadedSnakes.Add(snakeScript);
        _preloadedSnakeSaveData.Add(SnakeSaveData);
    }

    private void ActivateAndInitializeSnakes()
    {
        int resolution = subNodesCount + 1;

        for (int i = 0; i < _preloadedSnakes.Count; i++)
        {
            SnakeBlock snakeScript = _preloadedSnakes[i];
            SnakeSaveData data = _preloadedSnakeSaveData[i];

            snakeScript.Initialize(data.direction, data.segmentPositions, resolution, data.arrowColor, false);

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
        _preloadedSnakeSaveData.Clear();
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

        if (levelToPlay != null && levelToPlay.snakes != null)
        {
            snakesToSpawn = levelToPlay.snakes.Count;
            for (int i = 0; i < levelToPlay.snakes.Count; i++)
            {
                var snake = levelToPlay.snakes[i];
                if (snake == null || snake.segmentPositions == null) continue;
                dotsToSpawn += (snake.segmentPositions.Count + 1) / 2;
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
