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

    public Color snakeMoveColor = new Color(0.31f, 0.99f, 1f, 1f);

    public Color snakeTakeHitColor = new Color(254f / 255f, 104f / 255f, 104f / 255f, 1f);

    public float lineWidth = 0.4f;

    // --- DỮ LIỆU ---
    private Vector3[] _allNodePositions;
    private Vector3[] _originalState; // Trạng thái gốc để tham chiếu

    private int _totalPoints;
    private int _nodesPerUnit; // Mật độ điểm (Points per Unit)

    private bool _isMoving = false;
    private LineRenderer lineRenderer;

    private List<Collider2D> _myColliders = new List<Collider2D>();

    // Biến lưu tổng quãng đường đã đi (tính bằng index)
    private float _accumulatedShift = 0f;

    // Danh sách lưu vị trí bắt đầu của từng đốt xương trong mảng node
    // Dùng để map đúng GameObject vào đúng vị trí trên đường ray
    private List<int> _segmentStartIndices = new List<int>();

    private LevelController levelController;

    private bool outed = false;

    private void Awake()
    {
        SetupLineRenderer();
    }

    private void Start()
    {
        levelController = FindObjectOfType<LevelController>();
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
        _nodesPerUnit = resolution; // Đây là mật độ chuẩn (số điểm trên 1 đơn vị độ dài)

        _myColliders.Clear();
        _myColliders.AddRange(GetComponentsInChildren<Collider2D>());
        foreach (var t in mainSegments)
        {
            if (t) _myColliders.AddRange(t.GetComponentsInChildren<Collider2D>());
        }

        if (bodySegments.Count > 0 && bodySegments[0] != null && arrowVisual == null)
            arrowVisual = bodySegments[0].Find("Arrow");

        // --- KHỞI TẠO DỮ LIỆU ĐIỂM (LOGIC MỚI: MẬT ĐỘ ĐỒNG NHẤT) ---
        if (bodySegments.Count > 1)
        {
            int segmentsCount = bodySegments.Count - 1;

            // 1. Tính toán tổng số điểm cần thiết dựa trên độ dài thực tế
            _segmentStartIndices.Clear();
            int currentTotalPoints = 0;

            // List tạm để lưu số điểm của từng đoạn
            List<int> pointsPerSegment = new List<int>();

            for (int i = 0; i < segmentsCount; i++)
            {
                // Lưu index bắt đầu của đoạn này
                _segmentStartIndices.Add(currentTotalPoints);

                // Tính khoảng cách thực tế giữa 2 đốt
                float dist = Vector3.Distance(bodySegments[i].position, bodySegments[i + 1].position);

                // Tính số điểm dựa trên độ dài * độ phân giải
                // Mathf.Max(1, ...) để đảm bảo ít nhất có 1 điểm nối
                int pointsCount = Mathf.Max(1, Mathf.RoundToInt(dist * _nodesPerUnit));

                pointsPerSegment.Add(pointsCount);
                currentTotalPoints += pointsCount;
            }
            // Add index cho đốt cuối cùng (để sync)
            _segmentStartIndices.Add(currentTotalPoints);

            // Tổng điểm = tổng các khoảng + 1 điểm chốt cuối
            _totalPoints = currentTotalPoints + 1;

            _allNodePositions = new Vector3[_totalPoints];
            _originalState = new Vector3[_totalPoints];

            // 2. Điền dữ liệu vị trí nội suy
            int arrayIndex = 0;
            for (int i = 0; i < segmentsCount; i++)
            {
                Vector3 start = bodySegments[i].position;
                Vector3 end = bodySegments[i + 1].position;
                int count = pointsPerSegment[i];

                for (int j = 0; j < count; j++)
                {
                    float t = (float)j / count;
                    _allNodePositions[arrayIndex] = Vector3.Lerp(start, end, t);
                    arrayIndex++;
                }
            }
            // Điểm cuối cùng
            _allNodePositions[arrayIndex] = bodySegments[segmentsCount].position;
        }
        else if (bodySegments.Count == 1)
        {
            _totalPoints = 1;
            _allNodePositions = new Vector3[] { bodySegments[0].position };
            _originalState = new Vector3[1];
            _segmentStartIndices.Clear();
            _segmentStartIndices.Add(0);
        }

        UpdateSegmentVisuals(snakeColor);
        UpdateVisualRotation();
        UpdateLineRenderer();
    }

    void UpdateSegmentVisuals(Color color)
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
                    sr.color = color;
                }
                else
                {
                    sr.color = color;
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
        if (!_isMoving)
        {
            StartCoroutine(ProcessMovement());
        }
    }

    private IEnumerator ProcessMovement()
    {
        _isMoving = true;
        lineRenderer.startColor = snakeMoveColor;
        lineRenderer.endColor = snakeMoveColor;
        arrowVisual.GetComponentInChildren<SpriteRenderer>().color = snakeMoveColor;
        UpdateSegmentVisuals(snakeMoveColor);
        // 1. Lưu trạng thái gốc & Reset bộ đếm quãng đường
        System.Array.Copy(_allNodePositions, _originalState, _totalPoints);
        _accumulatedShift = 0f;

        Vector3 moveDir = GetDirVector(direction);

        while (true)
        {
            float distToObstacle = CheckObstacleDistance(moveDir);

            if (distToObstacle < 0.9f)
            {
                lineRenderer.startColor = snakeTakeHitColor;
                lineRenderer.endColor = snakeTakeHitColor;
                arrowVisual.GetComponentInChildren<SpriteRenderer>().color = snakeTakeHitColor;
                UpdateSegmentVisuals(snakeTakeHitColor);
                // A. Lao vào tường (Chỉ là tăng shift tạm thời rồi giảm về cũ)
                yield return StartCoroutine(HitObstacle(moveDir, distToObstacle));

                // B. Lùi về vị trí ban đầu (Giảm shift về 0)
                yield return StartCoroutine(ReturnToOrigin(moveDir));

                break;
            }

            // C. Đi tiếp 1 bước (Tăng shift lên 1 đơn vị chuẩn)
            yield return StartCoroutine(MoveOneStep(moveDir));

            if (bodySegments.Count > 0 && bodySegments[0].position.sqrMagnitude > 22500f)
            {
                Destroy(gameObject);
                break;
            }

            if (bodySegments.Count > 0 && bodySegments[0].position.sqrMagnitude > 2500f && !outed)
            {
                levelController.SetCountArrowInGame();
                outed = true;
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

    // --- HÀM CẬP NHẬT VỊ TRÍ TOÀN BỘ RẮN DỰA TRÊN SHIFT ---
    // Đây là hàm cốt lõi giúp mọi thứ mượt mà
    private void UpdateSnakePosition(float shift, Vector3 moveDir)
    {
        for (int i = 0; i < _totalPoints; i++)
        {
            // trackIndex: Vị trí của đốt i trên đường ray ảo.
            // Công thức: -shift + i
            // Nếu shift tăng (đi tới), trackIndex giảm (đi vào vùng âm - vùng mở rộng thẳng).
            float trackIndex = -shift + i;

            _allNodePositions[i] = GetPointOnVirtualTrack(trackIndex, moveDir);
        }
        SyncMainSegments();
    }

    private Vector3 GetPointOnVirtualTrack(float trackIndex, Vector3 moveDir)
    {
        // trackIndex < 0: Nằm trên đường thẳng mở rộng từ đầu gốc
        if (trackIndex < 0)
        {
            float distFromHead = Mathf.Abs(trackIndex) / _nodesPerUnit;
            return _originalState[0] + moveDir * distFromHead;
        }
        // trackIndex >= 0: Nằm trong thân gốc (Original Body)
        else
        {
            return SampleArray(_originalState, trackIndex);
        }
    }

    // --- DI CHUYỂN 1 BƯỚC ---
    private IEnumerator MoveOneStep(Vector3 dir)
    {
        // Mục tiêu: Tăng shift thêm 1 đơn vị chuẩn (_nodesPerUnit)
        float startShift = _accumulatedShift;
        float targetShift = startShift + _nodesPerUnit;

        while (_accumulatedShift < targetShift)
        {
            _accumulatedShift += Time.deltaTime * moveSpeed * _nodesPerUnit;
            if (_accumulatedShift > targetShift) _accumulatedShift = targetShift;

            UpdateSnakePosition(_accumulatedShift, dir);
            yield return null;
        }
    }

    // --- VA CHẠM & NẢY ---
    private IEnumerator HitObstacle(Vector3 dir, float distance)
    {
        float startShift = _accumulatedShift;

        // Tính giới hạn nảy (tính bằng world unit -> index unit)
        float bounceDist = Mathf.Clamp(distance - 0.1f, 0.2f, 0.9f);
        float targetShift = startShift + (bounceDist * _nodesPerUnit);

        // Lao vào
        while (_accumulatedShift < targetShift)
        {
            _accumulatedShift += Time.deltaTime * moveSpeed * _nodesPerUnit;
            if (_accumulatedShift > targetShift) _accumulatedShift = targetShift;
            UpdateSnakePosition(_accumulatedShift, dir);
            yield return null;
        }

        // Lùi ra (về lại vị trí trước khi lao vào)
        while (_accumulatedShift > startShift)
        {
            _accumulatedShift -= Time.deltaTime * moveSpeed * _nodesPerUnit;
            if (_accumulatedShift < startShift) _accumulatedShift = startShift;
            UpdateSnakePosition(_accumulatedShift, dir);
            yield return null;
        }
    }

    // --- LÙI VỀ VỊ TRÍ GỐC ---
    private IEnumerator ReturnToOrigin(Vector3 dir)
    {
        // Mục tiêu: Giảm shift về 0
        while (_accumulatedShift > 0f)
        {
            // Lùi với tốc độ chậm hơn một chút (1/3 tốc độ đi) để tạo hiệu ứng
            _accumulatedShift -= Time.deltaTime * (moveSpeed / 3f) * _nodesPerUnit;
            if (_accumulatedShift < 0f) _accumulatedShift = 0f;

            UpdateSnakePosition(_accumulatedShift, dir);
            yield return null;
        }

        // Chốt cứng
        System.Array.Copy(_originalState, _allNodePositions, _totalPoints);
        SyncMainSegments();
    }

    private Vector3 SampleArray(Vector3[] arr, float floatIndex)
    {
        int count = arr.Length;
        if (count == 0) return Vector3.zero;
        if (floatIndex <= 0) return arr[0];
        if (floatIndex >= count - 1) return arr[count - 1];

        int i = Mathf.FloorToInt(floatIndex);
        float t = floatIndex - i;

        return Vector3.Lerp(arr[i], arr[i + 1], t);
    }

    // HÀM QUAN TRỌNG: Cập nhật vị trí các đốt xương (GameObject)
    private void SyncMainSegments()
    {
        for (int k = 0; k < bodySegments.Count; k++)
        {
            if (bodySegments[k] != null)
            {
                // Logic mới: Lấy index từ mảng đã lưu trong Initialize
                if (k < _segmentStartIndices.Count)
                {
                    int virtualIndex = _segmentStartIndices[k];
                    if (virtualIndex < _totalPoints)
                    {
                        bodySegments[k].position = _allNodePositions[virtualIndex];
                    }
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