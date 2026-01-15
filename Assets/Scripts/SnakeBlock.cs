using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))] 
public class SnakeBlock : MonoBehaviour
{
    [Header("Settings")]
    public ArrowDir direction;     
    [SerializeField] private float moveSpeed = 220f; // Giữ nguyên tốc độ của bạn
    public LayerMask obstacleLayer;

    [Header("Segments")]
    public List<Transform> bodySegments = new List<Transform>(); 
    [SerializeField] private Transform arrowVisual; 

    [Header("Line Visuals (New)")]
    public Color snakeColor = Color.black; // Giữ nguyên màu đen
    public float lineWidth = 0.4f;         // Giữ nguyên độ dày

    private bool isMoving = false;
    private LineRenderer lineRenderer; 

    // --- SỬA LỖI Ở ĐÂY: Dùng Awake để luôn tìm thấy LineRenderer ---
    private void Awake()
    {
        SetupLineRenderer();
    }
    
    void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        
        // Cài đặt cơ bản
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;
        
        // Ép vẽ 2D phẳng (Không xoay theo Camera)
        lineRenderer.alignment = LineAlignment.TransformZ;
        
        // Tránh bị dãn hình ở góc cua
        lineRenderer.textureMode = LineTextureMode.Tile; 
        
        // Bo tròn góc cua (Số càng lớn càng tròn, 0 là vuông góc)
        // Nếu bạn muốn giống mê cung vuông vức trong ảnh mẫu -> Để = 0
        // Nếu muốn mượt -> Để = 5 hoặc 10
        lineRenderer.numCornerVertices = 5; 
        lineRenderer.numCapVertices = 5;

        // Material & Màu
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = snakeColor;
        lineRenderer.endColor = snakeColor;

        // Vẽ dây nằm lên trên cùng (để che lấp các khớp nối nếu có)
        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder = 5; 
    }

    public void Initialize(ArrowDir dir, List<Transform> segments)
    {
        direction = dir;
        bodySegments = segments;
        
        // Logic tìm Arrow của bạn
        if (bodySegments.Count > 0 && bodySegments[0] != null)
        {
            if (arrowVisual == null)
            {
                arrowVisual = bodySegments[0].Find("Arrow");
            }
        }

        //Cập nhật lại thông số LineRenderer (phòng trường hợp Awake chạy trước khi bạn set màu)
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

    // (Đã bỏ hàm SetupLineRenderer vì logic đã chuyển vào Awake và Initialize)

    void UpdateSegmentVisuals()
    {
        foreach (var seg in bodySegments)
        {
            if (seg == null) continue; // Check an toàn

            SpriteRenderer sr = seg.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = snakeColor; 
                
                if (sr.transform != arrowVisual) 
                {
                    sr.transform.localScale = Vector3.one * 0.4f; 
                }
                
                sr.sortingOrder = 1; 
            }
        }
    }

    private void LateUpdate()
    {
        // Thêm check null an toàn tuyệt đối
        if (bodySegments == null || bodySegments.Count == 0 || lineRenderer == null) return;

        lineRenderer.positionCount = bodySegments.Count;
        
        for (int i = 0; i < bodySegments.Count; i++)
        {
            if (bodySegments[i] != null)
            {
                lineRenderer.SetPosition(i, bodySegments[i].position);
            }
        }
    }



    // --- CÁC LOGIC CŨ CỦA BẠN (GIỮ NGUYÊN 100%) ---

    public void UpdateVisualRotation()
    {
        if (arrowVisual == null) return;
        float angle = 0;
        switch (direction)
        {
            case ArrowDir.Up: angle = 0; break;
            case ArrowDir.Down: angle = 180; break;
            case ArrowDir.Left: angle = 90; break;
            case ArrowDir.Right: angle = -90; break;
        }
        arrowVisual.localRotation = Quaternion.Euler(0, 0, angle);
    }

    public void OnHeadClicked() 
    {
        Debug.Log("Hi");
        if (!isMoving) StartCoroutine(ProcessMovement());
    }

    IEnumerator ProcessMovement()
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

            if (bodySegments.Count > 0 && Vector3.Distance(bodySegments[0].position, Vector3.zero) > 150f) 
            {
                Destroy(gameObject);
                break;
            }
        }
        isMoving = false;
    }

    bool CanMove(Vector3 dir)
    {
        if (bodySegments.Count == 0) return false;
        // Logic Raycast 30f của bạn
        Vector3 startPos = bodySegments[0].position + dir * 0.6f;
        return !Physics2D.Raycast(startPos, dir, 30f, obstacleLayer);
    }

    IEnumerator MoveOneStep(Vector3 dir)
    {
        List<Vector3> startPos = new List<Vector3>();
        List<Vector3> targetPos = new List<Vector3>();

        foreach (var seg in bodySegments) startPos.Add(seg.position);

        targetPos.Add(startPos[0] + dir);
        for (int i = 1; i < bodySegments.Count; i++)
            targetPos.Add(startPos[i - 1]);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed; // Tốc độ 220f
            for (int i = 0; i < bodySegments.Count; i++)
            {
                if(bodySegments[i] != null)
                    bodySegments[i].position = Vector3.Lerp(startPos[i], targetPos[i], t);
            }
            yield return null;
        }

        for (int i = 0; i < bodySegments.Count; i++)
        {
            if(bodySegments[i] != null)
                bodySegments[i].position = targetPos[i];
        }
    }

    IEnumerator ShakeEffect()
    {
        Vector3 original = bodySegments[0].position;
        for (float t = 0; t < 0.2f; t += Time.deltaTime)
        {
            bodySegments[0].position = original + (Vector3)(Random.insideUnitCircle * 0.1f);
            yield return null;
        }
        bodySegments[0].position = original;
    }

    Vector3 GetDirVector(ArrowDir dir)
    {
        switch (dir) {
            case ArrowDir.Up: return Vector3.up;
            case ArrowDir.Down: return Vector3.down;
            case ArrowDir.Left: return Vector3.left;
            case ArrowDir.Right: return Vector3.right;
            default: return Vector3.zero;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (bodySegments != null && bodySegments.Count > 0 && bodySegments[0] != null)
        {
            Vector3 dir = Vector3.zero;
            switch (direction)
            {
                case ArrowDir.Up: dir = Vector3.up; break;
                case ArrowDir.Down: dir = Vector3.down; break;
                case ArrowDir.Left: dir = Vector3.left; break;
                case ArrowDir.Right: dir = Vector3.right; break;
            }

            // Logic Gizmo 30f của bạn
            Vector3 startPos = bodySegments[0].position + dir * 0.6f;
            bool isBlocked = Physics2D.Raycast(startPos, dir, 30f, obstacleLayer);
            
            Gizmos.color = isBlocked ? Color.red : Color.green;
            Gizmos.DrawRay(startPos, dir * 30f); 
        }
    }
}