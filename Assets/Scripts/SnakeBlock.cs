using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SnakeBlock : MonoBehaviour
{
    [Header("Settings")]
    public ArrowDir direction;

    [SerializeField]
    private float moveSpeed = 220f; // Giữ nguyên tốc độ của bạn

    public LayerMask obstacleLayer;

    [Header("Segments")]
    public List<Transform> bodySegments = new List<Transform>();

    [SerializeField]
    private Transform arrowVisual;

    [Header("Line Visuals (New)")]
    public Color snakeColor = Color.black;
    public float lineWidth = 0.4f;

    private bool isMoving = false;
    private LineRenderer lineRenderer;

    [Tooltip("Phải bằng số SubNodes bên LevelLoader + 1")]
    public int nodesPerUnit = 11;

    // --- BIẾN TỐI ƯU (THÊM VÀO ĐỂ KHÔNG SINH RÁC) ---
    // Dùng mảng cố định để lưu vị trí thay vì tạo List mới mỗi bước đi
    private Vector3[] _startPosBuffer;
    private Vector3[] _targetPosBuffer;
    private int _segmentCount;

    private void Awake()
    {
        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;
        lineRenderer.alignment = LineAlignment.TransformZ;
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.numCornerVertices = 10;
        lineRenderer.numCapVertices = 10;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = snakeColor;
        lineRenderer.endColor = snakeColor;

        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder = 5;
    }

    public void Initialize(ArrowDir dir, List<Transform> segments)
    {
        direction = dir;
        bodySegments = segments;
        _segmentCount = bodySegments.Count;

        // --- TỐI ƯU: KHỞI TẠO MẢNG ĐỆM 1 LẦN DUY NHẤT ---
        // Chỉ tạo mảng mới khi số lượng đốt thay đổi hoặc chưa khởi tạo
        if (_startPosBuffer == null || _startPosBuffer.Length != _segmentCount)
        {
            _startPosBuffer = new Vector3[_segmentCount];
            _targetPosBuffer = new Vector3[_segmentCount];
        }

        if (bodySegments.Count > 0 && bodySegments[0] != null && arrowVisual == null)
        {
            arrowVisual = bodySegments[0].Find("Arrow");
        }

        if (lineRenderer != null)
        {
            lineRenderer.startColor = snakeColor;
            lineRenderer.endColor = snakeColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
        }

        UpdateVisualRotation();
        UpdateSegmentVisuals();
    }

    private void UpdateSegmentVisuals()
    {
        foreach (Transform seg in bodySegments)
        {
            if (seg == null) continue;

            SpriteRenderer sr = seg.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) continue;

            sr.color = snakeColor;

            if (sr.transform != arrowVisual)
            {
                sr.transform.localScale = Vector3.one * 0.4f;
            }

            sr.sortingOrder = 1;
        }
    }

    private void LateUpdate()
    {
        if (bodySegments == null || bodySegments.Count == 0 || lineRenderer == null)
            return;

        lineRenderer.positionCount = bodySegments.Count;

        for (int i = 0; i < bodySegments.Count; i++)
        {
            if (bodySegments[i] != null)
            {
                lineRenderer.SetPosition(i, bodySegments[i].position);
            }
        }
    }

    public void UpdateVisualRotation()
    {
        if (arrowVisual == null) return;

        float angle = 0f;

        switch (direction)
        {
            case ArrowDir.Up:
                angle = 0f;
                break;
            case ArrowDir.Down:
                angle = 180f;
                break;
            case ArrowDir.Left:
                angle = 90f;
                break;
            case ArrowDir.Right:
                angle = -90f;
                break;
        }

        arrowVisual.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void OnHeadClicked()
    {
        Debug.Log("Hi");

        if (!isMoving)
        {
            StartCoroutine(ProcessMovement());
        }
    }

    private IEnumerator ProcessMovement()
    {
        isMoving = true;
        Vector3 moveDir = GetDirVector(direction);

        while (true)
        {
            if (!CanMove(moveDir))
            {
                yield return StartCoroutine(ShakeEffect());
                break;
            }

            yield return StartCoroutine(MoveOneStep(moveDir));

            if (bodySegments.Count > 0 &&
                Vector3.Distance(bodySegments[0].position, Vector3.zero) > 150f)
            {
                Destroy(gameObject);
                break;
            }
        }

        isMoving = false;
    }

    private bool CanMove(Vector3 dir)
    {
        if (bodySegments.Count == 0) return false;

        Vector3 startPos = bodySegments[0].position + dir * 0.6f;
        return !Physics2D.Raycast(startPos, dir, 30f, obstacleLayer);
    }

    // --- ĐÂY LÀ HÀM ĐƯỢC TỐI ƯU BỘ NHỚ ---
    private IEnumerator MoveOneStep(Vector3 dir)
    {
        // 1. Thay vì tạo List mới (new List<Vector3>), ta dùng Mảng Đệm (_startPosBuffer)
        for (int i = 0; i < _segmentCount; i++)
        {
            if (bodySegments[i] != null)
            {
                _startPosBuffer[i] = bodySegments[i].position;
            }
        }

        Vector3 oldHead = _startPosBuffer[0];
        Vector3 newHead = oldHead + dir;

        // 2. Tính toán đích đến và lưu vào Mảng Đệm (_targetPosBuffer)
        // LOGIC TÍNH TOÁN GIỮ NGUYÊN 100% NHƯ CŨ
        for (int i = 0; i < _segmentCount; i++)
        {
            Vector3 target;

            if (i < nodesPerUnit)
            {
                // Logic cũ của bạn: Nội suy đốt đầu
                float ratio = (float)(nodesPerUnit - i) / nodesPerUnit;
                target = Vector3.Lerp(oldHead, newHead, ratio);
            }
            else
            {
                // Logic cũ của bạn: Đuôi bám theo (startPos[i - nodesPerUnit])
                target = _startPosBuffer[i - nodesPerUnit];
            }

            _targetPosBuffer[i] = target;
        }

        // 3. Chạy Lerp (Logic giữ nguyên)
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;

            for (int i = 0; i < _segmentCount; i++)
            {
                if (bodySegments[i] != null)
                {
                    // Lấy dữ liệu từ Mảng Đệm
                    bodySegments[i].position =
                        Vector3.Lerp(_startPosBuffer[i], _targetPosBuffer[i], t);
                }
            }

            yield return null;
        }

        // Chốt vị trí cuối
        for (int i = 0; i < _segmentCount; i++)
        {
            if (bodySegments[i] != null)
            {
                bodySegments[i].position = _targetPosBuffer[i];
            }
        }
    }

    private IEnumerator ShakeEffect()
    {
        Vector3 original = bodySegments[0].position;

        for (float t = 0f; t < 0.2f; t += Time.deltaTime)
        {
            bodySegments[0].position =
                original + (Vector3)(Random.insideUnitCircle * 0.1f);
            yield return null;
        }

        bodySegments[0].position = original;
    }

    private Vector3 GetDirVector(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up:
                return Vector3.up;
            case ArrowDir.Down:
                return Vector3.down;
            case ArrowDir.Left:
                return Vector3.left;
            case ArrowDir.Right:
                return Vector3.right;
            default:
                return Vector3.zero;
        }
    }

    private void OnDrawGizmos()
    {
        if (bodySegments == null ||
            bodySegments.Count == 0 ||
            bodySegments[0] == null)
            return;

        Vector3 dir = Vector3.zero;

        switch (direction)
        {
            case ArrowDir.Up:
                dir = Vector3.up;
                break;
            case ArrowDir.Down:
                dir = Vector3.down;
                break;
            case ArrowDir.Left:
                dir = Vector3.left;
                break;
            case ArrowDir.Right:
                dir = Vector3.right;
                break;
        }

        Vector3 startPos = bodySegments[0].position + dir * 0.6f;
        bool isBlocked =
            Physics2D.Raycast(startPos, dir, 30f, obstacleLayer);

        Gizmos.color = isBlocked ? Color.red : Color.green;
        Gizmos.DrawRay(startPos, dir * 30f);
    }
}