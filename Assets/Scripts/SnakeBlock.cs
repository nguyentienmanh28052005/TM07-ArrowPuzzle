using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SnakeBlock : MonoBehaviour
{
    [Header("Settings")]
    public ArrowDir direction;
    [SerializeField] private float moveSpeed = 40f;
    public LayerMask obstacleLayer;

    [Header("Main Segments")]
    public List<Transform> bodySegments = new List<Transform>();

    [SerializeField] private Transform arrowVisual;

    [Header("Visuals")]
    public Color snakeColor = Color.black;
    public float lineWidth = 0.4f;

    // --- DỮ LIỆU ---
    private Vector3[] _allNodePositions;
    private Vector3[] _startSnapshot; // Dùng cho HitObstacle & Move
    private Vector3[] _targetSnapshot;
    private Vector3[] _originalState; // Dùng làm khuôn mẫu để lùi về

    private int _totalPoints;
    private int _nodesPerUnit;

    private bool _isMoving = false;
    private LineRenderer lineRenderer;

    private List<Collider2D> _myColliders = new List<Collider2D>();

    // Biến tạm để lưu vị trí đầu mới khi di chuyển tới
    private Vector3 _currentNewHeadPos;

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

        lineRenderer.numCornerVertices = 0;
        lineRenderer.numCapVertices = 0;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = snakeColor;
        lineRenderer.endColor = snakeColor;
        lineRenderer.sortingOrder = 5;
    }

    public void Initialize(ArrowDir dir, List<Transform> mainSegments, int resolution)
    {
        direction = dir;
        bodySegments = mainSegments;
        _nodesPerUnit = resolution;

        _myColliders.Clear();
        _myColliders.AddRange(GetComponentsInChildren<Collider2D>());
        foreach (var t in mainSegments)
        {
            if (t) _myColliders.AddRange(t.GetComponentsInChildren<Collider2D>());
        }

        if (bodySegments.Count > 0 && bodySegments[0] != null && arrowVisual == null)
            arrowVisual = bodySegments[0].Find("Arrow");

        if (bodySegments.Count > 1)
        {
            int segmentsCount = bodySegments.Count - 1;
            _totalPoints = (segmentsCount * _nodesPerUnit) + 1;

            _allNodePositions = new Vector3[_totalPoints];
            _startSnapshot = new Vector3[_totalPoints];
            _targetSnapshot = new Vector3[_totalPoints]; // Khởi tạo mảng target
            _originalState = new Vector3[_totalPoints];

            int arrayIndex = 0;
            for (int i = 0; i < segmentsCount; i++)
            {
                Vector3 start = bodySegments[i].position;
                Vector3 end = bodySegments[i + 1].position;

                for (int j = 0; j < _nodesPerUnit; j++)
                {
                    float t = (float)j / _nodesPerUnit;
                    _allNodePositions[arrayIndex] = Vector3.Lerp(start, end, t);
                    arrayIndex++;
                }
            }
            _allNodePositions[arrayIndex] = bodySegments[segmentsCount].position;
        }
        else if (bodySegments.Count == 1)
        {
            _totalPoints = 1;
            _allNodePositions = new Vector3[] { bodySegments[0].position };
            _originalState = new Vector3[1];
        }

        UpdateSegmentVisuals();
        UpdateVisualRotation();
        UpdateLineRenderer();
    }

    void UpdateSegmentVisuals()
    {
        foreach (var seg in bodySegments)
        {
            if (seg == null) continue;
            SpriteRenderer sr = seg.GetComponentInChildren<SpriteRenderer>();

            if (sr != null)
            {
                if (sr.transform != arrowVisual)
                {
                    sr.enabled = true;
                    sr.transform.localScale = Vector3.one * 0.4f;
                    sr.color = snakeColor;
                }
                else
                {
                    sr.color = snakeColor;
                }
            }
        }
    }

    private void LateUpdate()
    {
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        if (lineRenderer != null && _totalPoints > 0)
        {
            lineRenderer.positionCount = _totalPoints;
            lineRenderer.SetPositions(_allNodePositions);
        }
    }

    public void OnHeadClicked()
    {
        if (!_isMoving) StartCoroutine(ProcessMovement());
    }

    private IEnumerator ProcessMovement()
    {
        _isMoving = true;

        // 1. Lưu vị trí gốc (Position Before Run)
        System.Array.Copy(_allNodePositions, _originalState, _totalPoints);

        Vector3 moveDir = GetDirVector(direction);

        while (true)
        {
            float distToObstacle = CheckObstacleDistance(moveDir);

            if (distToObstacle < 0.9f)
            {
                // A. Lao vào tường (Hiệu ứng va chạm)
                yield return StartCoroutine(HitObstacle(moveDir, distToObstacle));

                // B. Lùi về vị trí ban đầu (Dùng thuật toán tái tạo đường đi)
                yield return StartCoroutine(ReturnToOrigin(moveDir));

                break;
            }

            // C. Đi tiếp 1 bước
            yield return StartCoroutine(MoveOneStep(moveDir));

            if (bodySegments.Count > 0 && bodySegments[0].position.sqrMagnitude > 22500f)
            {
                Destroy(gameObject);
                break;
            }
        }
        _isMoving = false;
    }

    private float CheckObstacleDistance(Vector3 dir)
    {
        if (_totalPoints == 0) return 0f;
        Vector3 startPos = _allNodePositions[0];
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, dir, 20f, obstacleLayer);

        float closestDist = float.MaxValue;
        bool found = false;
        foreach (var hit in hits)
        {
            if (hit.collider != null && !IsMyCollider(hit.collider))
            {
                if (hit.distance < closestDist)
                {
                    closestDist = hit.distance;
                    found = true;
                }
            }
        }
        return found ? closestDist : float.MaxValue;
    }

    private bool IsMyCollider(Collider2D col)
    {
        if (_myColliders == null) return false;
        return _myColliders.Contains(col);
    }

    // --- LOGIC DI CHUYỂN TỚI (SLIDING FORWARD) ---
    private IEnumerator MoveOneStep(Vector3 dir)
    {
        System.Array.Copy(_allNodePositions, _startSnapshot, _totalPoints);
        _currentNewHeadPos = _startSnapshot[0] + dir;

        float totalShift = _nodesPerUnit;
        float currentShift = 0f;

        while (currentShift < totalShift)
        {
            currentShift += Time.deltaTime * moveSpeed * _nodesPerUnit;
            float applyShift = Mathf.Min(currentShift, totalShift);
            UpdateForwardSlide(applyShift);
            yield return null;
        }
        UpdateForwardSlide(totalShift);
    }

    // --- LOGIC VA CHẠM (HIT & BOUNCE) ---
    private IEnumerator HitObstacle(Vector3 dir, float distance)
    {
        System.Array.Copy(_allNodePositions, _startSnapshot, _totalPoints);
        _currentNewHeadPos = _startSnapshot[0] + dir;

        float maxWorldDist = Mathf.Clamp(distance - 0.1f, 0.2f, 0.9f);
        float maxShiftIndex = maxWorldDist * _nodesPerUnit;

        // Lao tới
        float currentShift = 0f;
        while (currentShift < maxShiftIndex)
        {
            currentShift += Time.deltaTime * moveSpeed * _nodesPerUnit;
            float applyShift = Mathf.Min(currentShift, maxShiftIndex);
            UpdateForwardSlide(applyShift);
            yield return null;
        }

        // Lùi lại (về vị trí trước va chạm)
        while (currentShift > 0f)
        {
            currentShift -= Time.deltaTime * moveSpeed * _nodesPerUnit;
            float applyShift = Mathf.Max(currentShift, 0f);
            UpdateForwardSlide(applyShift);
            yield return null;
        }
        UpdateForwardSlide(0f);
    }

    // --- LOGIC LÙI VỀ GỐC (SỬ DỤNG ĐƯỜNG RAY TÍNH TOÁN) ---
    // Fix lỗi teleport cho rắn ngắn
    private IEnumerator ReturnToOrigin(Vector3 dir)
    {
        // Tính khoảng cách cần lùi (Distance from Current Head to Original Head)
        float totalDistance = Vector3.Distance(_allNodePositions[0], _originalState[0]);
        float totalIndexShift = totalDistance * _nodesPerUnit;

        float currentShift = totalIndexShift;

        // Lặp lùi dần offset từ [Distance -> 0]
        while (currentShift > 0f)
        {
            currentShift -= Time.deltaTime * (int)(moveSpeed/3) * _nodesPerUnit;
            if (currentShift < 0f) currentShift = 0f;

            for (int i = 0; i < _totalPoints; i++)
            {
                // trackIndex: Vị trí trên đường ray ảo.
                // 0 = Đầu Gốc.
                // >0 = Thân Gốc.
                // <0 = Đường đi đã qua (Đường thẳng kéo dài từ đầu gốc).

                // Ví dụ: currentShift = 50. i = 0 (Đầu). trackIndex = -50.
                // -> Lấy điểm cách đầu gốc 50 index về phía trước.
                float trackIndex = -currentShift + i;

                _allNodePositions[i] = GetPointOnVirtualTrack(trackIndex, dir);
            }
            SyncMainSegments();
            yield return null;
        }

        // Chốt vị trí về gốc chính xác
        System.Array.Copy(_originalState, _allNodePositions, _totalPoints);
        SyncMainSegments();
    }

    // Hàm lấy tọa độ trên đường ray ảo (Kết hợp Mảng Gốc + Toán học Vector)
    private Vector3 GetPointOnVirtualTrack(float trackIndex, Vector3 moveDir)
    {
        // Nếu trackIndex < 0: Nghĩa là điểm này nằm trên đường thẳng mà rắn đã đi qua
        if (trackIndex < 0)
        {
            // Tính khoảng cách từ đầu gốc (đơn vị world unit)
            float distFromHead = Mathf.Abs(trackIndex) / _nodesPerUnit;
            // Vị trí = Đầu Gốc + Hướng * Khoảng cách
            return _originalState[0] + moveDir * distFromHead;
        }
        else
        {
            // Nếu trackIndex >= 0: Nghĩa là điểm này nằm trong thân rắn gốc
            return SampleArray(_originalState, trackIndex);
        }
    }

    // --- CÁC HÀM HELPER ---

    private void UpdateForwardSlide(float shiftAmount)
    {
        for (int i = 0; i < _totalPoints; i++)
        {
            // Logic trượt tới: Nối NewHead -> StartSnapshot
            float trackPos = (_nodesPerUnit + i) - shiftAmount;

            if (trackPos <= 0) _allNodePositions[i] = _currentNewHeadPos;
            else if (trackPos >= _nodesPerUnit)
            {
                float arrayIndex = trackPos - _nodesPerUnit;
                _allNodePositions[i] = SampleArray(_startSnapshot, arrayIndex);
            }
            else
            {
                float t = trackPos / _nodesPerUnit;
                _allNodePositions[i] = Vector3.Lerp(_currentNewHeadPos, _startSnapshot[0], t);
            }
        }
        SyncMainSegments();
    }

    private Vector3 SampleArray(Vector3[] arr, float floatIndex)
    {
        int count = arr.Length;
        if (count == 0) return Vector3.zero;

        // Clamp index
        if (floatIndex <= 0) return arr[0];
        if (floatIndex >= count - 1) return arr[count - 1];

        int i = Mathf.FloorToInt(floatIndex);
        float t = floatIndex - i;

        return Vector3.Lerp(arr[i], arr[i + 1], t);
    }

    private void SyncMainSegments()
    {
        for (int k = 0; k < bodySegments.Count; k++)
        {
            if (bodySegments[k] != null)
            {
                int virtualIndex = k * _nodesPerUnit;
                if (virtualIndex < _totalPoints)
                {
                    bodySegments[k].position = _allNodePositions[virtualIndex];
                }
            }
        }
    }

    public void UpdateVisualRotation()
    {
        if (arrowVisual == null) return;
        float angle = 0f;
        switch (direction)
        {
            case ArrowDir.Up: angle = 0f; break;
            case ArrowDir.Down: angle = 180f; break;
            case ArrowDir.Left: angle = 90f; break;
            case ArrowDir.Right: angle = -90f; break;
        }
        arrowVisual.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private Vector3 GetDirVector(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: return Vector3.up;
            case ArrowDir.Down: return Vector3.down;
            case ArrowDir.Left: return Vector3.left;
            case ArrowDir.Right: return Vector3.right;
            default: return Vector3.zero;
        }
    }
}