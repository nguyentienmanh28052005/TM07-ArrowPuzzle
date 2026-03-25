using UnityEngine;
using Solo.MOST_IN_ONE;

[RequireComponent(typeof(SnakeBlock))] 
public class ArrowGuideline : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lineLength = 3000f;
    [SerializeField] private float lineWidth = 8f;
    [SerializeField] private float startOffset = 0f;
    [SerializeField] private Color lineColor = Color.gray;

    private GameObject _guidelineRoot;
    private SnakeBlock _snakeBlock;

    /// <summary>
    /// Khởi tạo tham chiếu và gọi hàm sinh hình ảnh tia dóng lúc mới nạp.
    /// </summary>
    private void Awake()
    {
        _snakeBlock = GetComponent<SnakeBlock>();
        CreateGuidelineProcedurally();
        SetLineActive(false);
    }

    /// <summary>
    /// Đăng ký lắng nghe sự kiện từ MessageManager khi Object được bật.
    /// </summary>
    private void OnEnable()
    {
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnShowAllPaths, HandleShowAllPaths);
    }

    /// <summary>
    /// Hủy đăng ký lắng nghe sự kiện để chống lỗi tràn bộ nhớ khi Object bị tắt.
    /// </summary>
    private void OnDisable()
    {
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnShowAllPaths, HandleShowAllPaths);
    }

    /// <summary>
    /// Xử lý tín hiệu bật/tắt hiển thị toàn bộ đường đi từ hệ thống.
    /// </summary>
    private void HandleShowAllPaths(object data)
    {
        if (data is bool isShowing)
        {
            if (_snakeBlock != null && _snakeBlock.IsMoving) return;
            SetLineActive(isShowing);
        }
    }

    /// <summary>
    /// Tự động khởi tạo cấu trúc GameObject và SpriteRenderer cho tia dóng bằng code.
    /// </summary>
    private void CreateGuidelineProcedurally()
    {
        _guidelineRoot = new GameObject("Guideline_Root_Auto");
        _guidelineRoot.transform.SetParent(transform); 

        GameObject visual = new GameObject("Guideline_Visual_Auto");
        visual.transform.SetParent(_guidelineRoot.transform);

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        Texture2D tex = Texture2D.whiteTexture; 
        
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0f), 100f);
        sr.color = lineColor;
        sr.sortingOrder = -1; 

        visual.transform.localScale = new Vector3(lineWidth, lineLength, 1f);
        visual.transform.localPosition = Vector3.zero; 
    }

    /// <summary>
    /// Cập nhật liên tục vị trí của tia dóng vào cuối mỗi khung hình để chống giật lag.
    /// </summary>
    private void LateUpdate()
    {
        if (_guidelineRoot != null && _guidelineRoot.activeSelf && _snakeBlock != null)
        {
            UpdatePositionAndRotation();
        }
    }

    /// <summary>
    /// Tính toán và đồng bộ vị trí, góc xoay của tia dóng theo nốt đầu tiên của thân rắn.
    /// </summary>
    private void UpdatePositionAndRotation()
    {
        if (_snakeBlock.bodySegments.Count == 0 || _snakeBlock.bodySegments[0] == null) return;

        Vector3 headPos = _snakeBlock.bodySegments[0].position;
        Vector3 moveDir = Vector3.up;
        float angle = 0f;

        switch (_snakeBlock.direction)
        {
            case ArrowDir.Up:    moveDir = Vector3.up;    angle = 0f;   break;
            case ArrowDir.Down:  moveDir = Vector3.down;  angle = 180f; break;
            case ArrowDir.Left:  moveDir = Vector3.left;  angle = 90f;  break;
            case ArrowDir.Right: moveDir = Vector3.right; angle = -90f; break;
        }

        _guidelineRoot.transform.position = headPos + (moveDir * startOffset);
        _guidelineRoot.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Kích hoạt hoặc vô hiệu hóa trạng thái hiển thị của tia dóng.
    /// </summary>
    public void SetLineActive(bool isActive)
    {
        if (_guidelineRoot != null) _guidelineRoot.SetActive(isActive);
    }
}