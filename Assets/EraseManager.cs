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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Start()
    {
        levelController = FindObjectOfType<LevelController>();
    }

    // Gắn hàm này vào nút Cục Tẩy trên màn hình UI
    public void ToggleEraseMode()
    {
        if (Time.timeScale == 0f) return;
        
        IsEraseModeActive = !IsEraseModeActive;
        
        if (IsEraseModeActive)
        {
            Debug.Log("CHẾ ĐỘ TẨY: Đã Bật. Hãy nhấp vào một con rắn!");
            // Tùy chọn: Đổi màu nền màn hình tối đi một chút hoặc đổi màu nút tẩy để báo hiệu
        }
        else
        {
            Debug.Log("CHẾ ĐỘ TẨY: Đã Tắt.");
        }
    }

    // Hàm này sẽ được SnakeInput gọi khi người chơi bấm vào rắn
    public void ExecuteErase(SnakeBlock targetSnake)
    {
        if (targetSnake == null || targetSnake.bodySegments.Count == 0) return;

        // 1. Tắt chế độ chờ để khóa Input
        IsEraseModeActive = false;

        // 2. Chuyển đổi tọa độ Nút UI thành tọa độ World 2D để sinh ra cục tẩy
        Vector3 spawnWorldPos = Camera.main.ScreenToWorldPoint(uiButtonRect.position);
        spawnWorldPos.z = 0f;

        // 3. Sinh ra cục tẩy trên Scene
        GameObject eraserObj = Instantiate(eraserPrefab, spawnWorldPos, Quaternion.identity);
        
        // Trích xuất danh sách các đốt rắn (Từ Đầu đến Đuôi)
        List<Transform> segments = targetSnake.bodySegments;
        Vector3 tailPos = segments[segments.Count - 1].position;

        // 4. KIẾN TRÚC HIỆU ỨNG LIÊN HOÀN (SEQUENCE)
        Sequence eraseSeq = DOTween.Sequence();

        // Bước A: Cục tẩy bay vòng cung (DOJump) từ Nút UI đến Đuôi rắn
        eraseSeq.Append(eraserObj.transform.DOJump(tailPos, jumpPower: 2f, numJumps: 1, flyToTailDuration));

        // Bước B: Lấy quỹ đạo con rắn (Từ Đuôi ngược lên Đầu)
        Vector3[] pathPositions = new Vector3[segments.Count];
        for (int i = 0; i < segments.Count; i++)
        {
            // Duyệt ngược list để lấy tọa độ từ Đuôi -> Đầu
            pathPositions[i] = segments[(segments.Count - 1) - i].position;
        }

        // Bước C: Cục tẩy chạy men theo đường cong của con rắn
        float totalEraseTime = segments.Count * eraseSpeedPerNode;
        Tween pathTween = eraserObj.transform.DOPath(pathPositions, totalEraseTime, PathType.Linear)
            .SetEase(Ease.Linear);
        
        eraseSeq.Append(pathTween);

        // Bước D: Hiệu ứng "Ăn mòn" con rắn (Fade mờ toàn bộ con rắn cùng lúc cục tẩy chạy)
        // Vì SnakeBlock của bạn dùng LineRenderer và Sprite, ta sẽ cho nó mờ dần đi
        eraseSeq.Join(DOVirtual.Float(1f, 0.3f, totalEraseTime, (alpha) => {
            Color fadedColor = targetSnake.snakeColor;
            fadedColor.a = alpha;
            targetSnake.SetColorImmediatePublic(fadedColor);
        }));

        // Bước E: Kết liễu
        eraseSeq.OnComplete(() => {
            // Hiệu ứng cục tẩy nổ tung/biến mất
            eraserObj.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack).OnComplete(() => {
                Destroy(eraserObj);
            });

            // Xóa sổ hoàn toàn con rắn khỏi bàn cờ
            Destroy(targetSnake.gameObject);
            levelController.SetCountArrowInGame();
            // Tùy chọn: Gọi LevelController để cập nhật số lượng mũi tên còn lại
            // FindObjectOfType<LevelController>()?.SetCountArrowInGame();
        });
    }
}