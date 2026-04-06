using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

        if (levelToPlay.portals != null)
        {
            for (int i = 0; i < levelToPlay.portals.Count; i++)
            {
                var p = levelToPlay.portals[i];
                string pairLabel = GetPortalPairLabel(i);

                if (GridManager.Instance != null)
                {
                    GridManager.Instance.PortalMap[p.entrance] = new GridManager.PortalLink { exit = p.exit, exitDir = p.exitDir };
                    GridManager.Instance.PortalMap[p.exit] = new GridManager.PortalLink { exit = p.entrance, exitDir = p.entranceDir };
                }

                if (portalPrefab != null)
                {
                    GameObject inObj = Instantiate(portalPrefab, new Vector3(p.entrance.x, p.entrance.y, 0), GetRotationForDir(p.entranceDir), gameContainer);
                    SpriteRenderer inSr = inObj.GetComponent<SpriteRenderer>();
                    if (inSr != null) inSr.color = p.portalColor;
                    AttachPortalPairLabel(inObj, pairLabel);

                    GameObject outObj = Instantiate(portalPrefab, new Vector3(p.exit.x, p.exit.y, 0), GetRotationForDir(p.exitDir), gameContainer);
                    SpriteRenderer outSr = outObj.GetComponent<SpriteRenderer>();
                    if (outSr != null) outSr.color = p.portalColor;
                    AttachPortalPairLabel(outObj, pairLabel);
                }
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

    private static void AttachPortalPairLabel(GameObject portalObj, string pairLabel)
    {
        if (portalObj == null) return;

        GameObject labelObj = new GameObject("PortalPairLabel");
        labelObj.transform.SetParent(portalObj.transform, false);
        labelObj.transform.localPosition = new Vector3(0f, 0f, -0.05f);
        // Keep text upright in world space (do NOT rotate with the portal).
        labelObj.transform.rotation = Quaternion.identity;
        labelObj.transform.localScale = Vector3.one;

        TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
        tmp.text = pairLabel;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 6.5f;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        MeshRenderer mr = labelObj.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            SpriteRenderer sr = portalObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                mr.sortingLayerID = sr.sortingLayerID;
                mr.sortingOrder = sr.sortingOrder + 1;
            }
        }
    }

    private static string GetPortalPairLabel(int indexZeroBased)
    {
        // 0->A, 1->B, ... 25->Z, 26->AA ...
        int n = indexZeroBased;
        if (n < 0) return "?";

        string s = string.Empty;
        do
        {
            int r = n % 26;
            s = (char)('A' + r) + s;
            n = (n / 26) - 1;
        } while (n >= 0);

        return s;
    }
}