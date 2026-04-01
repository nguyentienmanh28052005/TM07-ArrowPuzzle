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

    private void Awake()
    {
        _snakeBlock = GetComponent<SnakeBlock>();
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

    private void LateUpdate()
    {
        if (_guidelineRoot != null && _guidelineRoot.activeSelf && _snakeBlock != null)
        {
            UpdatePositionAndRotation();
        }
    }

    private void UpdatePositionAndRotation()
    {
        // ĐÃ SỬA LỖI: Kiểm tra danh sách LogicNodes thay vì bodySegments
        if (_snakeBlock.LogicNodes == null || _snakeBlock.LogicNodes.Count == 0) return;

        // ĐÃ SỬA LỖI: Lấy trực tiếp tọa độ đầu từ biến HeadPosition
        Vector3 headPos = _snakeBlock.HeadPosition;
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

    public void SetLineActive(bool isActive)
    {
        if (_guidelineRoot != null) _guidelineRoot.SetActive(isActive);
    }
}