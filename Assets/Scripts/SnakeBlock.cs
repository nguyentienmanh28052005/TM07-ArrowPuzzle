using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SnakeBlock : MonoBehaviour
{
    [Header("Settings")]
    public ArrowDir direction;
    [SerializeField] private float moveSpeed = 1f;
    public LayerMask obstacleLayer;

    [Header("Main Segments")]
    public List<Transform> bodySegments = new List<Transform>();

    [SerializeField] private Transform arrowVisual;

    [Header("Visuals")]
    public Color snakeColor = Color.black;
    public float lineWidth = 0.4f;

    // --- DỮ LIỆU TỐI ƯU ---
    private Vector3[] _allNodePositions;
    private Vector3[] _startSnapshot;
    private Vector3[] _targetSnapshot;

    private int _totalPoints;

    // QUAN TRỌNG: Biến này sẽ được set tự động từ LevelLoader, không chỉnh tay nữa
    private int _nodesPerUnit;

    private bool _isMoving = false;
    private LineRenderer lineRenderer;

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

        // CHỈNH VỀ 0 ĐỂ HẾT BỊ MÉO (WAVY)
        lineRenderer.numCornerVertices = 0;
        lineRenderer.numCapVertices = 0;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = snakeColor;
        lineRenderer.endColor = snakeColor;
        lineRenderer.sortingOrder = 5;
    }

    // --- INITIALIZE ---
    public void Initialize(ArrowDir dir, List<Transform> mainSegments, int resolution)
    {
        direction = dir;
        bodySegments = mainSegments;

        // ĐỒNG BỘ HÓA ĐỘ PHÂN GIẢI (Quan trọng nhất để không bị méo)
        _nodesPerUnit = resolution;

        if (bodySegments.Count > 0 && bodySegments[0] != null && arrowVisual == null)
            arrowVisual = bodySegments[0].Find("Arrow");

        // Tính toán mảng nốt ảo
        if (bodySegments.Count > 1)
        {
            int segmentsCount = bodySegments.Count - 1;
            // Công thức chuẩn xác: Tổng điểm = (Số đoạn * mật độ) + 1 điểm chốt
            _totalPoints = (segmentsCount * _nodesPerUnit) + 1;

            _allNodePositions = new Vector3[_totalPoints];
            _startSnapshot = new Vector3[_totalPoints];
            _targetSnapshot = new Vector3[_totalPoints];

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
            // Điền điểm cuối
            _allNodePositions[arrayIndex] = bodySegments[segmentsCount].position;
        }
        else if (bodySegments.Count == 1)
        {
            _totalPoints = 1;
            _allNodePositions = new Vector3[] { bodySegments[0].position };
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
                    sr.transform.localScale = Vector3.one * 0.4f; // Thu nhỏ thân
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
        Vector3 moveDir = GetDirVector(direction);

        while (true)
        {
            if (!CanMove(moveDir))
            {
                yield return StartCoroutine(ShakeEffect());
                break;
            }

            yield return StartCoroutine(MoveOneStep(moveDir));

            if (bodySegments.Count > 0 && bodySegments[0].position.sqrMagnitude > 22500f)
            {
                Destroy(gameObject);
                break;
            }
        }
        _isMoving = false;
    }

    private bool CanMove(Vector3 dir)
    {
        if (_totalPoints == 0) return false;
        Vector3 startPos = _allNodePositions[0] + dir * 0.6f;
        return !Physics2D.Raycast(startPos, dir, 30f, obstacleLayer);
    }

    // --- LOGIC DI CHUYỂN CHUẨN ---
    private IEnumerator MoveOneStep(Vector3 dir)
    {
        // 1. Snapshot
        System.Array.Copy(_allNodePositions, _startSnapshot, _totalPoints);

        Vector3 oldHead = _startSnapshot[0];
        Vector3 newHead = oldHead + dir;
        float invNodes = 1f / _nodesPerUnit;

        // 2. Tính toán Target
        for (int i = 0; i < _totalPoints; i++)
        {
            if (i < _nodesPerUnit)
            {
                // Nội suy đầu
                float ratio = (_nodesPerUnit - i) * invNodes;
                _targetSnapshot[i] = Vector3.Lerp(oldHead, newHead, ratio);
            }
            else
            {
                // Bám đuôi: Lấy vị trí cũ của điểm cách nó đúng 1 Unit (_nodesPerUnit)
                // Vì _nodesPerUnit được đồng bộ chính xác, phép trừ này sẽ ra đúng tọa độ hình học
                _targetSnapshot[i] = _startSnapshot[i - _nodesPerUnit];
            }
        }

        // 3. Lerp
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;

            for (int i = 0; i < _totalPoints; i++)
            {
                _allNodePositions[i] = Vector3.Lerp(_startSnapshot[i], _targetSnapshot[i], t);
            }

            // Đồng bộ ngược lại Nốt Gốc (Main Nodes)
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

            yield return null;
        }

        // Chốt vị trí
        System.Array.Copy(_targetSnapshot, _allNodePositions, _totalPoints);

        for (int k = 0; k < bodySegments.Count; k++)
        {
            if (bodySegments[k] != null)
            {
                int virtualIndex = k * _nodesPerUnit;
                if (virtualIndex < _totalPoints)
                    bodySegments[k].position = _allNodePositions[virtualIndex];
            }
        }
    }

    private IEnumerator ShakeEffect()
    {
        if (_totalPoints == 0) yield break;
        Vector3 original = _allNodePositions[0];
        for (float t = 0f; t < 0.2f; t += Time.deltaTime)
        {
            Vector3 shake = original + (Vector3)(Random.insideUnitCircle * 0.1f);
            _allNodePositions[0] = shake;
            if (bodySegments.Count > 0) bodySegments[0].position = shake;
            yield return null;
        }
        _allNodePositions[0] = original;
        if (bodySegments.Count > 0) bodySegments[0].position = original;
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