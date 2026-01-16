using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SnakeBlock : MonoBehaviour
{
    [Header("Settings")]
    public ArrowDir direction;
    // Tốc độ di chuyển
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
    private Vector3[] _startSnapshot; // Dùng làm đường ray tham chiếu
    private Vector3[] _originalState; // Lưu trạng thái gốc của level

    private int _totalPoints;
    private int _nodesPerUnit;

    private bool _isMoving = false;
    private LineRenderer lineRenderer;

    private List<Collider2D> _myColliders = new List<Collider2D>();

    // Biến tạm để lưu vị trí đầu mới khi di chuyển
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

        // Lưu trạng thái gốc
        System.Array.Copy(_allNodePositions, _originalState, _totalPoints);

        Vector3 moveDir = GetDirVector(direction);

        while (true)
        {
            float distToObstacle = CheckObstacleDistance(moveDir);

            if (distToObstacle < 0.9f)
            {
                // A. Lao vào tường (Dùng logic trượt tới)
                yield return StartCoroutine(HitObstacle(moveDir, distToObstacle));

                // B. Lùi về (Dùng logic trượt lui)
                yield return StartCoroutine(ReturnToOrigin());

                break;
            }

            // C. Đi tiếp 1 bước (Dùng logic trượt tới)
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

    // --- LOGIC TRƯỢT TỚI (SLIDE FORWARD) ---
    // Dùng chung cho cả MoveOneStep và HitObstacle (giai đoạn lao tới)
    private void UpdateForwardSlide(float shiftAmount)
    {
        // shiftAmount: Số lượng index muốn trượt tới (ví dụ 5.5 index)

        // Duyệt qua từng đốt
        for (int i = 0; i < _totalPoints; i++)
        {
            // Vị trí trên đường ray ảo
            // Đường ray ảo: [NewHead] .... [OldHead] .... [OldBody]
            // NewHead ở vị trí 0
            // OldHead ở vị trí nodesPerUnit
            // OldBody[k] ở vị trí nodesPerUnit + k

            // Khi shiftAmount = 0 -> Node 0 ở OldHead (trackPos = nodesPerUnit)
            // Khi shiftAmount = nodesPerUnit -> Node 0 ở NewHead (trackPos = 0)

            // Công thức: trackPosition = (nodesPerUnit + i) - shiftAmount
            float trackPos = (_nodesPerUnit + i) - shiftAmount;

            _allNodePositions[i] = SampleForwardTrack(trackPos);
        }
        SyncMainSegments();
    }

    // Lấy mẫu trên đường ray ảo hướng tới
    private Vector3 SampleForwardTrack(float trackIndex)
    {
        // TrackIndex < 0: Đã vượt quá đích đến (không nên xảy ra, nhưng clamp cho an toàn)
        if (trackIndex <= 0) return _currentNewHeadPos;

        // TrackIndex >= nodesPerUnit: Nằm trong phần thân cũ (StartSnapshot)
        if (trackIndex >= _nodesPerUnit)
        {
            // Lấy từ mảng StartSnapshot
            // Index trong mảng = trackIndex - nodesPerUnit
            float arrayIndexFloat = trackIndex - _nodesPerUnit;

            // Lerp trong mảng snapshot để lấy vị trí mịn
            return SampleArray(_startSnapshot, arrayIndexFloat);
        }

        // 0 < TrackIndex < nodesPerUnit: Nằm giữa NewHead và OldHead
        // Đoạn này là đường thẳng nối từ NewHead đến OldHead
        // Lerp(NewHead, OldHead, t)
        float t = trackIndex / _nodesPerUnit;
        return Vector3.Lerp(_currentNewHeadPos, _startSnapshot[0], t);
    }

    // --- HÀM DI CHUYỂN 1 BƯỚC (DÙNG LOGIC TRƯỢT MỚI) ---
    private IEnumerator MoveOneStep(Vector3 dir)
    {
        // 1. Snapshot vị trí hiện tại
        System.Array.Copy(_allNodePositions, _startSnapshot, _totalPoints);

        // 2. Xác định đích đến của đầu
        _currentNewHeadPos = _startSnapshot[0] + dir;

        // 3. Thực hiện trượt 1 đơn vị (tương đương nodesPerUnit index)
        float totalShift = _nodesPerUnit;
        float currentShift = 0f;

        while (currentShift < totalShift)
        {
            // Tăng lượng trượt (tính theo index/s)
            currentShift += Time.deltaTime * moveSpeed * _nodesPerUnit;

            // Clamp
            float applyShift = Mathf.Min(currentShift, totalShift);

            // Cập nhật vị trí
            UpdateForwardSlide(applyShift);

            yield return null;
        }

        // Chốt vị trí cuối cùng
        UpdateForwardSlide(totalShift);
    }

    // --- HÀM VA CHẠM (DÙNG LOGIC TRƯỢT TỚI RỒI LÙI) ---
    private IEnumerator HitObstacle(Vector3 dir, float distance)
    {
        System.Array.Copy(_allNodePositions, _startSnapshot, _totalPoints);
        _currentNewHeadPos = _startSnapshot[0] + dir;

        // Tính giới hạn trượt
        // distance là khoảng cách World Unit -> Đổi sang Index Unit
        // Trừ 0.1f world unit để không chạm tường
        float maxWorldDist = Mathf.Clamp(distance - 0.1f, 0.2f, 0.9f);
        float maxShiftIndex = maxWorldDist * _nodesPerUnit;

        // Giai đoạn 1: Trượt tới (Forward Slide)
        float currentShift = 0f;
        while (currentShift < maxShiftIndex)
        {
            currentShift += Time.deltaTime * moveSpeed * _nodesPerUnit;
            float applyShift = Mathf.Min(currentShift, maxShiftIndex);
            UpdateForwardSlide(applyShift);
            yield return null;
        }

        // Giai đoạn 2: Trượt lùi (Reverse Forward Slide)
        // Chỉ đơn giản là giảm currentShift về 0
        while (currentShift > 0f)
        {
            currentShift -= Time.deltaTime * moveSpeed * _nodesPerUnit;
            float applyShift = Mathf.Max(currentShift, 0f);
            UpdateForwardSlide(applyShift);
            yield return null;
        }

        UpdateForwardSlide(0f); // Reset về đúng vị trí trước khi lao vào
    }

    // --- HÀM LÙI VỀ GỐC (GIỮ NGUYÊN LOGIC TRƯỢT CŨ VÌ ĐÃ NGON) ---
    private IEnumerator ReturnToOrigin()
    {
        System.Array.Copy(_allNodePositions, _startSnapshot, _totalPoints);

        float headDistance = Vector3.Distance(_startSnapshot[0], _originalState[0]);
        float totalIndexShift = headDistance * _nodesPerUnit;
        float currentIndexShift = 0f;

        while (currentIndexShift < totalIndexShift)
        {
            currentIndexShift += Time.deltaTime * (int)(moveSpeed/2) * _nodesPerUnit;
            float shift = Mathf.Min(currentIndexShift, totalIndexShift);

            for (int i = 0; i < _totalPoints; i++)
            {
                _allNodePositions[i] = SampleReverseTrack(i + shift, totalIndexShift);
            }

            SyncMainSegments();
            yield return null;
        }

        System.Array.Copy(_originalState, _allNodePositions, _totalPoints);
        SyncMainSegments();
    }

    // Helper cho lùi về gốc
    private Vector3 SampleReverseTrack(float trackIndex, float separationIndices)
    {
        int floorIndex = Mathf.FloorToInt(trackIndex);
        int ceilIndex = Mathf.CeilToInt(trackIndex);
        float t = trackIndex - floorIndex;

        Vector3 p1 = GetReversePoint(floorIndex, separationIndices);
        Vector3 p2 = GetReversePoint(ceilIndex, separationIndices);

        return Vector3.Lerp(p1, p2, t);
    }

    private Vector3 GetReversePoint(int index, float separationIndices)
    {
        if (index < _totalPoints) return _startSnapshot[index];
        else
        {
            int originalIndex = Mathf.RoundToInt(index - separationIndices);
            originalIndex = Mathf.Clamp(originalIndex, 0, _totalPoints - 1);
            return _originalState[originalIndex];
        }
    }

    // Helper lấy mẫu mảng (Lerp giữa các phần tử mảng)
    private Vector3 SampleArray(Vector3[] arr, float floatIndex)
    {
        int count = arr.Length;
        if (count == 0) return Vector3.zero;
        if (count == 1) return arr[0];

        // Clamp
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