using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelEditor : MonoBehaviour
{
    [Header("Assets")]
    public GameObject headPrefab;
    public GameObject bodyPrefab;

    [Header("Data")]
    public LevelDataSO currentData;
    public Transform levelContainer;

    public GameObject currentSnakeObj;
    private SnakeBlock currentSnakeScript;
    private List<Transform> currentSegments = new List<Transform>();
    private ArrowDir currentDir = ArrowDir.Up;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentDir++;
            if ((int)currentDir > 3) currentDir = 0;
            if (currentSnakeScript != null)
            {
                currentSnakeScript.direction = currentDir;
                currentSnakeScript.UpdateVisualRotation();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) FinishCurrentSnake();
        if (Input.GetMouseButtonDown(0)) HandleLeftClick();
        if (Input.GetMouseButtonDown(1)) HandleRightClick();
    }

    void HandleLeftClick()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y));

        if (currentSnakeObj == null) CreateHead(gridPos);
        else CreateBodySegment(gridPos);
    }

    void HandleRightClick()
    {
        if (currentSnakeObj != null)
        {
            Destroy(currentSnakeObj);
            currentSnakeObj = null;
            currentSegments.Clear();
            return;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos, LayerMask.GetMask("Block"));

        if (hit != null)
        {
            SnakeBlock sb = hit.GetComponentInParent<SnakeBlock>();
            if (sb != null) Destroy(sb.gameObject);
        }
    }

    void CreateHead(Vector2Int pos)
    {
        currentSnakeObj = new GameObject("Snake_" + pos);
        currentSnakeObj.transform.parent = levelContainer;
        currentSnakeScript = currentSnakeObj.AddComponent<SnakeBlock>();

        GameObject headParams = Instantiate(headPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, currentSnakeObj.transform);
        currentSegments.Clear();
        currentSegments.Add(headParams.transform);

        currentSnakeScript.direction = currentDir;
        Transform arrowVis = headParams.transform.Find("Arrow");
        if (arrowVis)
        {
            float angle = 0;
            switch (currentDir)
            {
                case ArrowDir.Up: angle = 0; break;
                case ArrowDir.Down: angle = 180; break;
                case ArrowDir.Left: angle = 90; break;
                case ArrowDir.Right: angle = -90; break;
            }
            arrowVis.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void CreateBodySegment(Vector2Int pos)
    {
        Transform lastSeg = currentSegments[currentSegments.Count - 1];
        if (Vector3.Distance(lastSeg.position, new Vector3(pos.x, pos.y, 0)) > 1.1f) return;

        GameObject body = Instantiate(bodyPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, currentSnakeObj.transform);
        currentSegments.Add(body.transform);
    }

    void FinishCurrentSnake()
    {
        if (currentSnakeObj == null) return;
        currentSnakeScript.bodySegments = new List<Transform>(currentSegments);
        currentSnakeScript.obstacleLayer = LayerMask.GetMask("Block");

        currentSnakeObj = null;
        currentSnakeScript = null;
        currentSegments.Clear();
        Debug.Log("Finished");
    }

    [ContextMenu("Save Level")]
    public void SaveLevel()
    {
        if (currentData == null) return;
        currentData.snakes.Clear();

        foreach (Transform snakeParent in levelContainer)
        {
            SnakeBlock sb = snakeParent.GetComponent<SnakeBlock>();
            if (sb != null)
            {
                SnakeSaveData data = new SnakeSaveData();
                data.direction = sb.direction;
                foreach (Transform seg in sb.bodySegments)
                {
                    if (seg != null)
                        data.segmentPositions.Add(new Vector2Int(Mathf.RoundToInt(seg.position.x), Mathf.RoundToInt(seg.position.y)));
                }
                currentData.snakes.Add(data);
            }
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(currentData);
#endif
    }

    [ContextMenu("Load Level To Edit")]
    public void LoadLevelToEdit()
    {
        if (currentData == null) return;

        var children = new List<GameObject>();
        foreach (Transform child in levelContainer) children.Add(child.gameObject);
        children.ForEach(child => DestroyImmediate(child));

        foreach (var data in currentData.snakes)
        {
            if (data.segmentPositions.Count == 0) continue;

            GameObject snakeObj = new GameObject("Snake_Loaded");
            snakeObj.transform.parent = levelContainer;
            SnakeBlock sb = snakeObj.AddComponent<SnakeBlock>();
            sb.obstacleLayer = LayerMask.GetMask("Block");

            List<Transform> loadedSegments = new List<Transform>();

            for (int i = 0; i < data.segmentPositions.Count; i++)
            {
                Vector2Int pos = data.segmentPositions[i];
                GameObject prefab = (i == 0) ? headPrefab : bodyPrefab;
                GameObject seg = Instantiate(prefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, snakeObj.transform);
                loadedSegments.Add(seg.transform);
            }

            sb.Initialize(data.direction, loadedSegments, 9);
            sb.UpdateVisualRotation();
        }
    }
}