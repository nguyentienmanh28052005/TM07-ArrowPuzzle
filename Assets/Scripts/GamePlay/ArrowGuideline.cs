using UnityEngine;
using Solo.MOST_IN_ONE;

[RequireComponent(typeof(SnakeBlock))] // Ép script này phải nằm cùng nhà với SnakeBlock
public class ArrowGuideline : MonoBehaviour
{
    [Header("Settings (Không cần kéo thả gì cả)")]
    [SerializeField] private float lineLength = 3000f;
    [SerializeField] private float lineWidth = 8f;
    [SerializeField] private float startOffset = 0f;
    [SerializeField] private Color lineColor = Color.gray;

    private GameObject _guidelineRoot;
    private SnakeBlock _snakeBlock;

    private void Awake()
    {
        _snakeBlock = GetComponent<SnakeBlock>();
        
        // Tự động tạo hình ảnh Tia dóng bằng Code ngay khi sinh ra
        CreateGuidelineProcedurally();
        
        SetLineActive(false);
    }

    private void OnEnable()
    {
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnShowAllPaths, HandleShowAllPaths);
    }

    private void OnDisable()
    {
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnShowAllPaths, HandleShowAllPaths);
    }

    private void HandleShowAllPaths(object data)
    {
        if (data is bool isShowing)
        {
            if (_snakeBlock != null && _snakeBlock.IsMoving) return;
            SetLineActive(isShowing);
        }
    }

    private void CreateGuidelineProcedurally()
    {
        // 1. Tạo GameObject chứa trục xoay (Root)
        _guidelineRoot = new GameObject("Guideline_Root_Auto");
        _guidelineRoot.transform.SetParent(transform); // Bám theo nốt Head

        // 2. Tạo GameObject chứa Hình ảnh (Visual)
        GameObject visual = new GameObject("Guideline_Visual_Auto");
        visual.transform.SetParent(_guidelineRoot.transform);

        // 3. Tự động thêm SpriteRenderer và "hô biến" ra một tấm ảnh màu trắng
        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        Texture2D tex = Texture2D.whiteTexture; // Lấy ảnh trắng có sẵn trong lõi Unity
        
        // Đặt tâm Pivot ở dưới cùng (X = 0.5, Y = 0) để khi kéo dài nó chỉ mọc về 1 phía
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0f), 100f);
        sr.color = lineColor;
        sr.sortingOrder = -1; // Ép chìm xuống dưới con rắn

        // 4. Định hình lại thành thanh tia laser
        visual.transform.localScale = new Vector3(lineWidth, lineLength, 1f);
        visual.transform.localPosition = Vector3.zero; 
    }

    private void LateUpdate()
    {
        if (_guidelineRoot != null && _guidelineRoot.activeSelf && _snakeBlock != null)
        {
            UpdatePositionAndRotation();
        }
    }

    private void UpdatePositionAndRotation()
    {
        if (_snakeBlock.bodySegments.Count == 0 || _snakeBlock.bodySegments[0] == null) return;

        // Bám chặt vào Nốt Đầu Tiên (Head)
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

        // Đặt tia dóng nhích lên một chút cho khỏi đè vào đầu rắn
        _guidelineRoot.transform.position = headPos + (moveDir * startOffset);
        _guidelineRoot.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetLineActive(bool isActive)
    {
        if (_guidelineRoot != null) _guidelineRoot.SetActive(isActive);
    }
}