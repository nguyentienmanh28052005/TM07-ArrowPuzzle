using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;

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

    public bool IsEraseModeActive { get; private set; } = false;
    public bool IsExecutingErase { get; private set; } = false;

    public GameObject erasePanel;
    public TextMeshProUGUI textEraseCount;

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
        
        if(CurrencyManager.Instance.SpendEraseTool(1))
        {
            erasePanel.gameObject.SetActive(true);
            IsEraseModeActive = !IsEraseModeActive;
            
            if (IsEraseModeActive) Debug.Log("CHẾ ĐỘ TẨY: Đã Bật. Hãy nhấp vào một con rắn!");
            else Debug.Log("CHẾ ĐỘ TẨY: Đã Tắt.");
        }
    }

    public void ExecuteErase(SnakeBlock targetSnake)
    {
        // ĐÃ SỬA: Kiểm tra LogicNodes thay vì bodySegments
        if (targetSnake == null || targetSnake.LogicNodes == null || targetSnake.LogicNodes.Count == 0) return;

        IsEraseModeActive = false;
        IsExecutingErase = true; 

        Vector3 spawnWorldPos = Camera.main.ScreenToWorldPoint(uiButtonRect.position);
        spawnWorldPos.z = 0f;

        GameObject eraserObj = Instantiate(eraserPrefab, spawnWorldPos, Quaternion.identity);
        
        // ĐÃ SỬA: Lấy danh sách tọa độ Toán học
        List<Vector3> nodes = targetSnake.LogicNodes;
        Vector3 tailPos = nodes[nodes.Count - 1];

        Sequence eraseSeq = DOTween.Sequence();

        eraseSeq.Append(eraserObj.transform.DOJump(tailPos, jumpPower: 2f, numJumps: 1, flyToTailDuration));

        Vector3[] pathPositions = new Vector3[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            pathPositions[i] = nodes[(nodes.Count - 1) - i];
        }

        float totalEraseTime = nodes.Count * eraseSpeedPerNode;
        Tween pathTween = eraserObj.transform.DOPath(pathPositions, totalEraseTime, PathType.Linear).SetEase(Ease.Linear);
        eraseSeq.Append(pathTween);

        eraseSeq.Join(DOVirtual.Float(1f, 0.3f, totalEraseTime, (alpha) => {
            Color fadedColor = targetSnake.snakeColor;
            fadedColor.a = alpha;
            targetSnake.SetColorImmediate(fadedColor);
        }));

        eraseSeq.OnComplete(() => {
            eraserObj.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack).OnComplete(() => {
                Destroy(eraserObj);
            });

            Destroy(targetSnake.gameObject);
            if (levelController != null) levelController.SetCountArrowInGame();
            
            IsExecutingErase = false;
            erasePanel.gameObject.SetActive(false);
            CameraController.IsGameplayBlocking = false;
        });
    }
}