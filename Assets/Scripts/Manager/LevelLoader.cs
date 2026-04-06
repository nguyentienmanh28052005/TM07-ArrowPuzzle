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

    [Header("Container")]
    public Transform gameContainer;

    [Header("Resolution Settings")]
    [Range(0, 20)]
    public int subNodesCount = 8;

    public bool editorMode = false;

    private IEnumerator Start()
    {
        if (!editorMode && GameManager.Instance != null)
            levelToPlay = GameManager.Instance.GetCurrentLevelData();

        CameraController.IsGameplayBlocking = true;

        bool isTextDone = false;
        GameCanvas canvas = FindObjectOfType<GameCanvas>();
        
        if (canvas != null && levelToPlay != null)
        {
            canvas.SetupModeUI(levelToPlay.gameMode);
            string modeName = levelToPlay.gameMode.ToString().ToUpper();
            canvas.ShowText(modeName, Color.cyan, () => isTextDone = true);
        }
        else
        {
            isTextDone = true; 
        }

        yield return new WaitUntil(() => isTextDone);

        LoadGameInternal();

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
    }

    [ContextMenu("Reload Level")]
    public void LoadGame()
    {
        LoadGameInternal();
    }

    private void LoadGameInternal()
    {
        if (levelToPlay == null) return;

        if (gameContainer != null)
        {
            int childCount = gameContainer.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(gameContainer.GetChild(i).gameObject);
            }
        }

        if (levelToPlay.keycards != null)
        {
            foreach (var k in levelToPlay.keycards)
            {
                GameObject obj = Instantiate(keycardPrefab, new Vector3(k.position.x, k.position.y, 0), Quaternion.identity, gameContainer);
                if (obj.TryGetComponent(out GridKeycard script)) script.keyColor = k.color;
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = k.color;
            }
        }

        if (levelToPlay.gates != null)
        {
            foreach (var g in levelToPlay.gates)
            {
                GameObject obj = Instantiate(gatePrefab, new Vector3(g.position.x, g.position.y, 0), Quaternion.identity, gameContainer);
                if (obj.TryGetComponent(out GridLaserGate script)) script.gateColor = g.color;
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = g.color;
            }
        }

        foreach (var snakeData in levelToPlay.snakes)
        {
            if (snakeData.segmentPositions.Count == 0) continue;

            GameObject snakeObj = Instantiate(snakePrefab, gameContainer);
            snakeObj.name = "Snake";
            SnakeBlock snakeScript = snakeObj.GetComponent<SnakeBlock>();

            for (int i = 0; i < snakeData.segmentPositions.Count; i++)
            {
                Vector2Int pos = snakeData.segmentPositions[i];
                Vector3 currentPos = new Vector3(pos.x, pos.y, 0);

                if (dotPrefab != null && i % 2 == 0)
                {
                    Instantiate(dotPrefab, currentPos, Quaternion.identity, gameContainer);
                }
            }

            int resolution = subNodesCount + 1;
            snakeScript.Initialize(snakeData.direction, snakeData.segmentPositions, resolution, snakeData.arrowColor);

            if (GridManager.Instance != null) GridManager.Instance.RegisterSnake(snakeScript);
        }
    }
}