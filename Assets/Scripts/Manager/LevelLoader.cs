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

        // 1. KHÓA GAMEPLAY NGAY LẬP TỨC
        CameraController.IsGameplayBlocking = true;

        // ========================================================
        // BẢN VÁ: KHÔI PHỤC FEEDBACK TEXT GIỚI THIỆU MÀN CHƠI
        // ========================================================
        bool isTextDone = false;
        GameCanvas canvas = FindObjectOfType<GameCanvas>();
        
        if (canvas != null && levelToPlay != null)
        {
            // Cài đặt hiển thị UI (Tim/Timer) dựa trên Mode
            canvas.SetupModeUI(levelToPlay.gameMode);

            string modeName = levelToPlay.gameMode.ToString().ToUpper();
            // Gọi hàm ShowText và truyền Callback để biết khi nào Animation chữ chạy xong
            canvas.ShowText(modeName, Color.cyan, () => isTextDone = true);
        }
        else
        {
            // Nếu không có Canvas (chạy test), bỏ qua đoạn chờ
            isTextDone = true; 
        }

        // Chờ đến khi cờ isTextDone được bật lên bởi Callback của GameCanvas
        yield return new WaitUntil(() => isTextDone);
        // ========================================================

        // 2. CHỮ BAY XONG -> BẮT ĐẦU LOAD RẮN THEO DATA-DRIVEN
        LoadGameInternal();

        // 3. ĐIỀU PHỐI ĐỒNG HỒ ĐẾM NGƯỢC
        if (TimeAttackManager.Instance != null)
        {
            if (levelToPlay != null && levelToPlay.gameMode == GameMode.TimeAttack)
                TimeAttackManager.Instance.InitializeTimer(levelToPlay.timeLimit);
            else
                TimeAttackManager.Instance.DisableTimer();
        }

        // 4. KÍCH HOẠT HIỆU ỨNG CAMERA (Camera Intro xong mới mở khóa Input)
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