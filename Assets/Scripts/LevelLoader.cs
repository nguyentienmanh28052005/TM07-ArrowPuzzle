using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [Header("Data")]
    public LevelDataSO levelToPlay;

    [Header("Prefabs")]
    public GameObject headPrefab;
    public GameObject bodyPrefab;

    [Header("Container")]
    public Transform gameContainer;

    [Header("Resolution Settings")]
    [Range(0, 20)]
    public int subNodesCount = 8;

    public bool editorMode = false;

    private void Start()
    {
        if(!editorMode)
            levelToPlay = GameManager.Instance.GetCurrentLevelData();
        LoadGame();
    }

    [ContextMenu("Reload Level")]
    public void LoadGame()
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

        foreach (var snakeData in levelToPlay.snakes)
        {
            if (snakeData.segmentPositions.Count == 0) continue;

            GameObject snakeObj = new GameObject("Snake");
            if (gameContainer != null) snakeObj.transform.parent = gameContainer;

            SnakeBlock snakeScript = snakeObj.AddComponent<SnakeBlock>();
            snakeScript.obstacleLayer = LayerMask.GetMask("Block");

            List<Transform> mainSegments = new List<Transform>();

            for (int i = 0; i < snakeData.segmentPositions.Count; i++)
            {
                Vector2Int pos = snakeData.segmentPositions[i];
                Vector3 currentPos = new Vector3(pos.x, pos.y, 0);

                GameObject prefab = (i == 0) ? headPrefab : bodyPrefab;
                GameObject mainSeg = Instantiate(prefab, currentPos, Quaternion.identity, snakeObj.transform);

                mainSeg.name = (i == 0) ? "Head" : $"Main_{i}";
                mainSegments.Add(mainSeg.transform);

                if (i == 0)
                {
                    Transform arrowVis = mainSeg.transform.Find("Arrow");
                    if (arrowVis)
                    {
                        float angle = 0;
                        switch (snakeData.direction)
                        {
                            case ArrowDir.Up: angle = 0; break;
                            case ArrowDir.Down: angle = 180; break;
                            case ArrowDir.Left: angle = 90; break;
                            case ArrowDir.Right: angle = -90; break;
                        }
                        arrowVis.localRotation = Quaternion.Euler(0, 0, angle);
                    }
                }
            }

            int resolution = subNodesCount + 1;
            snakeScript.Initialize(snakeData.direction, mainSegments, resolution);
        }

        Debug.Log("Load Game Success.");
    }
}