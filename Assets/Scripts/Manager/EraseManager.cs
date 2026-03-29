using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class EraseManager : MonoBehaviour
{
    public static EraseManager Instance;

    private LevelController levelController;

    [Header("Erase Settings")]
    [Tooltip("Prefab hình Cục Tẩy (Nên là Sprite thuần 2D)")]
    public GameObject eraserPrefab;
    
    [Tooltip("Tốc độ bay từ Nút UI vào Đuôi rắn (giây)")]
    public float flyToTailDuration = 0.5f;
    
    [Tooltip("Tốc độ tẩy mỗi đốt rắn (giây/đốt)")]
    public float eraseSpeedPerNode = 0.5f;

    [Tooltip("Vị trí Nút Cục Tẩy trên UI (Kéo thả RectTransform của nút vào đây)")]
    public RectTransform uiButtonRect;

    // Trạng thái: Game có đang ở chế độ chờ người chơi chọn rắn để tẩy không?
    public bool IsEraseModeActive { get; private set; } = false;

    // BỌC THÉP: Cờ khóa toàn cục - Đang có cục tẩy chạy trên màn hình không?
    public bool IsExecutingErase { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Start()
    {
        levelController = FindObjectOfType<LevelController>();
    }

    public void ToggleEraseMode()
    {
        if (Time.timeScale == 0f) return;
        
        IsEraseModeActive = !IsEraseModeActive;
        
        if (IsEraseModeActive)
        {
            Debug.Log("CHẾ ĐỘ TẨY: Đã Bật. Hãy nhấp vào một con rắn!");
        }
        else
        {
            Debug.Log("CHẾ ĐỘ TẨY: Đã Tắt.");
        }
    }

    public void ExecuteErase(SnakeBlock targetSnake)
    {
        if (targetSnake == null || targetSnake.bodySegments.Count == 0) return;

        // 1. Tắt chế độ chờ 
        IsEraseModeActive = false;
        
        // 2. ĐÓNG Ổ KHÓA TOÀN CỤC: Bắt đầu tẩy
        IsExecutingErase = true; 

        Vector3 spawnWorldPos = Camera.main.ScreenToWorldPoint(uiButtonRect.position);
        spawnWorldPos.z = 0f;

        GameObject eraserObj = Instantiate(eraserPrefab, spawnWorldPos, Quaternion.identity);
        
        List<Transform> segments = targetSnake.bodySegments;
        Vector3 tailPos = segments[segments.Count - 1].position;

        Sequence eraseSeq = DOTween.Sequence();

        // Bay từ UI vào
        eraseSeq.Append(eraserObj.transform.DOJump(tailPos, jumpPower: 2f, numJumps: 1, flyToTailDuration));

        // Quỹ đạo chạy men theo rắn
        Vector3[] pathPositions = new Vector3[segments.Count];
        for (int i = 0; i < segments.Count; i++)
        {
            pathPositions[i] = segments[(segments.Count - 1) - i].position;
        }

        float totalEraseTime = segments.Count * eraseSpeedPerNode;
        Tween pathTween = eraserObj.transform.DOPath(pathPositions, totalEraseTime, PathType.Linear).SetEase(Ease.Linear);
        eraseSeq.Append(pathTween);

        // Mờ dần con rắn
        eraseSeq.Join(DOVirtual.Float(1f, 0.3f, totalEraseTime, (alpha) => {
            Color fadedColor = targetSnake.snakeColor;
            fadedColor.a = alpha;
            targetSnake.SetColorImmediate(fadedColor);
        }));

        // Kết thúc
        eraseSeq.OnComplete(() => {
            eraserObj.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack).OnComplete(() => {
                Destroy(eraserObj);
            });

            Destroy(targetSnake.gameObject);
            if (levelController != null) levelController.SetCountArrowInGame();
            
            // 3. MỞ KHÓA TOÀN CỤC: Tẩy xong, cho phép Hint và Click hoạt động lại
            IsExecutingErase = false;

            CameraController.IsGameplayBlocking = false;
        });
    }
}