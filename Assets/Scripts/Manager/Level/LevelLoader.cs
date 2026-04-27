using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [Header("Data")]
    public LevelDataSO levelToPlay;

    [Header("Prefabs (Data-Driven)")]
    public GameObject snakePrefab; 
    public GameObject dotPrefab;   
    public GameObject keycardPrefab;
    public GameObject gatePrefab;
    public GameObject portalPrefab;

    [Header("Container")]
    public Transform gameContainer;

    [Header("Resolution Settings")]
    [Range(0, 20)]
    public int subNodesCount = 8;

    [Header("Optimization")]
    public int snakesPerFrame = 2; 
    public int dotsPerFrame = 15; 

    public bool editorMode = false;

    private List<SnakeBlock> _preloadedSnakes = new List<SnakeBlock>();
    private List<SnakeSaveData> _preloadedSnakeSaveData = new List<SnakeSaveData>();
    private GameObject _dotsContainer; 
    private GameObject _obstaclesContainer;
    private bool _isTextDone = false; 

    private IEnumerator Start()
    {
        if (PlaytestSession.IsPlaytesting)
        {
            editorMode = true;
            levelToPlay = PlaytestSession.LevelData;
            PlaytestExitListener.EnsureExists();
        }

        if (!editorMode && GameManager.Instance != null)
            levelToPlay = GameManager.Instance.GetCurrentLevelData();

        CameraController.IsGameplayBlocking = true;
        _isTextDone = false;

        GameCanvas canvas = FindObjectOfType<GameCanvas>();
        if (canvas != null && levelToPlay != null)
        {
            canvas.SetupModeUI(levelToPlay.gameMode);
            
            string modeName = levelToPlay.gameMode.ToString().ToUpper();
            string difficultyName = levelToPlay.levelDifficulty.ToString().ToUpper();

            canvas.ShowText(modeName, Color.cyan, () => 
            {
                _isTextDone = true;
                // canvas.ShowText(difficultyName, new Color(1f, 0.8f, 0f, 1f), () => 
                // {
                //     _isTextDone = true;
                // });
            });
        }
        else
        {
            _isTextDone = true; 
        }

        SpawnStaticObstacles();

        yield return StartCoroutine(PreSpawnDotsCoroutine());

        yield return StartCoroutine(PreSpawnSnakesCoroutine());

        yield return new WaitUntil(() => _isTextDone);

        ActivateAndInitializeSnakes();

        if (TimeAttackManager.Instance != null)
        {
            if (levelToPlay != null && levelToPlay.gameMode == GameMode.TimeAttack)
                TimeAttackManager.Instance.InitializeTimer(levelToPlay.timeLimit);
            else
                TimeAttackManager.Instance.DisableTimer();
        }

        CameraController camController = Camera.main.GetComponent<CameraController>();
        if (camController != null) camController.StartIntro();
        else CameraController.IsGameplayBlocking = false;

        if (TutorialManager.Instance != null) TutorialManager.Instance.CheckAndStartTutorial(levelToPlay);
    }

    [ContextMenu("Reload Level (Instant)")]
    public void LoadGame()
    {
        _isTextDone = true; 
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
    }

    private void ClearContainer()
    {
        if (gameContainer != null)
        {
            int childCount = gameContainer.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(gameContainer.GetChild(i).gameObject);
            }
        }
        
        if (_dotsContainer != null) 
        {
            DestroyImmediate(_dotsContainer);
            _dotsContainer = null;
        }

        if (_obstaclesContainer != null) 
        {
            DestroyImmediate(_obstaclesContainer);
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
            _dotsContainer.SetActive(false); 
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
                    
                    dotsSpawnedThisFrame++;

                    if (!_isTextDone && dotsSpawnedThisFrame >= dotsPerFrame)
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

            if (!_isTextDone && (i + 1) % snakesPerFrame == 0)
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
        snakeObj.SetActive(false); 
        
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

            snakeScript.gameObject.SetActive(true);
            snakeScript.Initialize(data.direction, data.segmentPositions, resolution, data.arrowColor);

            if (GridManager.Instance != null) 
                GridManager.Instance.RegisterSnake(snakeScript);
        }

        if (_dotsContainer != null) 
        {
            _dotsContainer.SetActive(true);
        }

        if (_obstaclesContainer != null)
        {
            _obstaclesContainer.SetActive(true);
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
}