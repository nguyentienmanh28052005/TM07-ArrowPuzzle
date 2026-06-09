using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class GridDot : MonoBehaviour
{
    public static Dictionary<Vector2Int, GridDot> GridMap = new Dictionary<Vector2Int, GridDot>();

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Color originalColor;

    private bool _isWinning = false;

    private void ResetVisualState()
    {
        transform.localScale = originalScale;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    private void KillTweens()
    {
        transform.DOKill();
        if (spriteRenderer != null) spriteRenderer.DOKill();
    }

    /// <summary>
    /// Lấy tham chiếu Component và lưu lại các thông số kích thước, màu sắc ban đầu.
    /// </summary>
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    /// <summary>
    /// Đăng ký tọa độ của Dot vào hệ thống lưới toàn cục và reset trạng thái an toàn.
    /// </summary>
    void OnEnable()
    {
        _isWinning = false;

        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        GridMap[pos] = this;

        ResetVisualState();
    }

    /// <summary>
    /// Dọn dẹp Animation và gỡ đăng ký khỏi hệ thống lưới khi bị vô hiệu hóa.
    /// </summary>
    void OnDisable()
    {
        KillTweens();
        ResetVisualState();

        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        if (GridMap.ContainsKey(pos) && GridMap[pos] == this)
        {
            GridMap.Remove(pos);
        }
    }

    /// <summary>
    /// Kích hoạt chuỗi hiệu ứng sóng Domino kết liễu bàn cờ khi người chơi giành chiến thắng.
    /// </summary>
    public void PlayWinAnimation(Color targetColor, float delay, float scaleAmount, float duration)
    {
        _isWinning = true;

        KillTweens();
        ResetVisualState();

        float halfDuration = duration / 2f;

        Sequence winSeq = DOTween.Sequence();

        if (delay > 0) winSeq.AppendInterval(delay);

        winSeq.Append(transform.DOScale(originalScale * scaleAmount, halfDuration).SetEase(Ease.OutQuad));
        winSeq.Append(transform.DOScale(Vector3.zero, halfDuration).SetEase(Ease.InBack));
        if (spriteRenderer != null) winSeq.Join(spriteRenderer.DOColor(targetColor, halfDuration));

        winSeq.SetLink(gameObject);
    }

    /// <summary>
    /// Kích hoạt hiệu ứng đàn hồi (Yoyo) khi có một con rắn trượt ngang qua nốt này.
    /// </summary>
    public void PlayLeaveEffect(float scaleAmount = 2f, float totalDuration = 0.4f)
    {
        if ((GameManager.Instance != null && GameManager.Instance.isGameOver) || _isWinning) return;

        KillTweens();
        ResetVisualState();

        float halfDuration = totalDuration / 2f;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(originalScale * scaleAmount, halfDuration).SetEase(Ease.OutQuad));
        if (spriteRenderer != null) seq.Join(spriteRenderer.DOColor(Color.white, halfDuration).SetEase(Ease.OutQuad));

        seq.Append(transform.DOScale(originalScale, halfDuration).SetEase(Ease.OutQuad));
        if (spriteRenderer != null) seq.Join(spriteRenderer.DOColor(originalColor, halfDuration).SetEase(Ease.OutQuad));

        seq.OnKill(ResetVisualState);
        seq.OnComplete(ResetVisualState);
        seq.SetLink(gameObject);
    }
}

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridDotBatchRenderer : MonoBehaviour
{
    private enum DotAnimationType
    {
        None,
        Leave,
        Win
    }

    private struct DotState
    {
        public Vector2Int gridPosition;
        public Vector3 localPosition;
        public Color color;
        public float scale;
        public bool isWinning;
        public DotAnimationType animationType;
        public float startTime;
        public float delay;
        public float duration;
        public float scaleAmount;
        public Color targetColor;
    }

    public static GridDotBatchRenderer Instance { get; private set; }

    private readonly Dictionary<Vector2Int, int> _dotIndexByGrid = new Dictionary<Vector2Int, int>(512);
    private readonly List<DotState> _dots = new List<DotState>(512);
    private readonly List<int> _waveSortBuffer = new List<int>(512);

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Mesh _mesh;
    private Material _ownedMaterial;
    private Vector3[] _vertices;
    private Vector2[] _uvs;
    private Color[] _colors;
    private int[] _triangles;
    private Vector3[] _quadOffsets;
    private Vector2[] _quadUvs;
    private Color _originalColor = Color.white;
    private Coroutine _waveRoutine;
    private bool _meshDirty;
    private int _activeAnimationCount;
    private Vector3 _waveCenter;

    public bool HasDots => _dots.Count > 0;

    private void Awake()
    {
        Instance = this;
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();

        _mesh = new Mesh { name = "Grid Dot Batch Mesh" };
        _mesh.MarkDynamic();
        _meshFilter.sharedMesh = _mesh;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (_mesh != null) Destroy(_mesh);
        if (_ownedMaterial != null) Destroy(_ownedMaterial);
    }

    public void ConfigureFromPrefab(GameObject dotPrefab)
    {
        SpriteRenderer templateRenderer = dotPrefab != null ? dotPrefab.GetComponentInChildren<SpriteRenderer>(true) : null;
        Sprite sprite = templateRenderer != null ? templateRenderer.sprite : null;

        if (dotPrefab != null)
        {
            gameObject.layer = dotPrefab.layer;
        }

        _originalColor = templateRenderer != null ? templateRenderer.color : Color.white;
        Vector3 prefabScale = dotPrefab != null ? dotPrefab.transform.localScale : Vector3.one;

        if (sprite == null)
        {
            ConfigureFallbackQuad();
            return;
        }

        Vector3 extents = sprite.bounds.extents;
        _quadOffsets = new[]
        {
            new Vector3(-extents.x * prefabScale.x, -extents.y * prefabScale.y, 0f),
            new Vector3(-extents.x * prefabScale.x, extents.y * prefabScale.y, 0f),
            new Vector3(extents.x * prefabScale.x, extents.y * prefabScale.y, 0f),
            new Vector3(extents.x * prefabScale.x, -extents.y * prefabScale.y, 0f)
        };

        Texture2D texture = sprite.texture;
        Rect rect = sprite.textureRect;
        float xMin = rect.xMin / texture.width;
        float xMax = rect.xMax / texture.width;
        float yMin = rect.yMin / texture.height;
        float yMax = rect.yMax / texture.height;
        _quadUvs = new[]
        {
            new Vector2(xMin, yMin),
            new Vector2(xMin, yMax),
            new Vector2(xMax, yMax),
            new Vector2(xMax, yMin)
        };

        if (_ownedMaterial != null) Destroy(_ownedMaterial);
        Material sourceMaterial = templateRenderer != null ? templateRenderer.sharedMaterial : null;
        _ownedMaterial = sourceMaterial != null
            ? new Material(sourceMaterial)
            : new Material(Shader.Find("Sprites/Default"));
        _ownedMaterial.mainTexture = texture;

        if (_ownedMaterial.HasProperty("_Color"))
        {
            _ownedMaterial.color = Color.white;
        }

        if (_ownedMaterial.HasProperty("_RendererColor"))
        {
            _ownedMaterial.SetColor("_RendererColor", Color.white);
        }

        _meshRenderer.sharedMaterial = _ownedMaterial;
        _meshRenderer.sortingLayerID = templateRenderer != null ? templateRenderer.sortingLayerID : 0;
        _meshRenderer.sortingOrder = templateRenderer != null ? templateRenderer.sortingOrder : 0;
        if (templateRenderer != null)
        {
            _meshRenderer.renderingLayerMask = templateRenderer.renderingLayerMask;
        }
    }

    private void ConfigureFallbackQuad()
    {
        float halfSize = 0.08f;
        _quadOffsets = new[]
        {
            new Vector3(-halfSize, -halfSize, 0f),
            new Vector3(-halfSize, halfSize, 0f),
            new Vector3(halfSize, halfSize, 0f),
            new Vector3(halfSize, -halfSize, 0f)
        };
        _quadUvs = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
    }

    public void StopInwardWave()
    {
        StopWaveRoutine();
    }

    public void RegisterDot(Vector2Int gridPosition)
    {
        if (_dotIndexByGrid.ContainsKey(gridPosition)) return;

        DotState dot = new DotState
        {
            gridPosition = gridPosition,
            localPosition = transform.InverseTransformPoint(new Vector3(gridPosition.x, gridPosition.y, 0f)),
            color = _originalColor,
            scale = 1f,
            targetColor = _originalColor
        };

        _dotIndexByGrid.Add(gridPosition, _dots.Count);
        _dots.Add(dot);
    }

    public void RebuildMesh()
    {
        if (_mesh == null) return;
        if (_quadOffsets == null || _quadUvs == null) ConfigureFallbackQuad();

        int dotCount = _dots.Count;
        _vertices = new Vector3[dotCount * 4];
        _uvs = new Vector2[dotCount * 4];
        _colors = new Color[dotCount * 4];
        _triangles = new int[dotCount * 6];

        for (int i = 0; i < dotCount; i++)
        {
            int vertexStart = i * 4;
            int triangleStart = i * 6;

            _uvs[vertexStart] = _quadUvs[0];
            _uvs[vertexStart + 1] = _quadUvs[1];
            _uvs[vertexStart + 2] = _quadUvs[2];
            _uvs[vertexStart + 3] = _quadUvs[3];

            _triangles[triangleStart] = vertexStart;
            _triangles[triangleStart + 1] = vertexStart + 1;
            _triangles[triangleStart + 2] = vertexStart + 2;
            _triangles[triangleStart + 3] = vertexStart;
            _triangles[triangleStart + 4] = vertexStart + 2;
            _triangles[triangleStart + 5] = vertexStart + 3;
        }

        WriteAllDotVerticesAndColors();
        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.uv = _uvs;
        _mesh.colors = _colors;
        _mesh.triangles = _triangles;
        _mesh.RecalculateBounds();
        _meshDirty = false;
    }

    public static bool TryPlayLeaveEffect(Vector2Int gridPosition, float scaleAmount = 2f, float totalDuration = 0.4f)
    {
        return Instance != null && Instance.PlayLeaveEffectAt(gridPosition, scaleAmount, totalDuration);
    }

    public bool PlayLeaveEffectAt(Vector2Int gridPosition, float scaleAmount = 2f, float totalDuration = 0.4f)
    {
        if ((GameManager.Instance != null && GameManager.Instance.isGameOver) || !_dotIndexByGrid.TryGetValue(gridPosition, out int index))
        {
            return false;
        }

        DotState dot = _dots[index];
        if (dot.isWinning) return true;

        bool wasInactive = dot.animationType == DotAnimationType.None;
        dot.animationType = DotAnimationType.Leave;
        dot.startTime = Time.time;
        dot.delay = 0f;
        dot.duration = Mathf.Max(0.001f, totalDuration);
        dot.scaleAmount = Mathf.Max(0f, scaleAmount);
        dot.targetColor = Color.white;
        dot.scale = 1f;
        dot.color = _originalColor;
        _dots[index] = dot;

        if (wasInactive) _activeAnimationCount++;
        _meshDirty = true;
        return true;
    }

    public float PlayWinEffect(Color targetColor, float waveSpeed, float animationDuration, float scaleAmount)
    {
        if (_dots.Count == 0) return 0f;

        StopWaveRoutine();
        Vector3 center = CalculateBoundsCenter();
        float maxDuration = 0f;

        for (int i = 0; i < _dots.Count; i++)
        {
            DotState dot = _dots[i];
            bool wasInactive = dot.animationType == DotAnimationType.None;
            float delay = Vector3.Distance(dot.localPosition, center) * Mathf.Max(0f, waveSpeed);

            dot.isWinning = true;
            dot.animationType = DotAnimationType.Win;
            dot.startTime = Time.time;
            dot.delay = delay;
            dot.duration = Mathf.Max(0.001f, animationDuration);
            dot.scaleAmount = Mathf.Max(0f, scaleAmount);
            dot.targetColor = targetColor;
            dot.scale = 1f;
            dot.color = _originalColor;
            _dots[i] = dot;

            if (wasInactive) _activeAnimationCount++;
            maxDuration = Mathf.Max(maxDuration, delay + animationDuration);
        }

        _meshDirty = true;
        return maxDuration;
    }

    public void PlayInwardWave(bool hasSourceCenter, Vector3 sourceCenter, float scaleAmount, float duration, int batchSize, float batchDelay)
    {
        if (_dots.Count == 0) return;

        StopWaveRoutine();
        _waveCenter = hasSourceCenter ? transform.InverseTransformPoint(sourceCenter) : CalculateBoundsCenter();
        _waveRoutine = StartCoroutine(InwardWaveRoutine(scaleAmount, duration, Mathf.Max(1, batchSize), Mathf.Max(0f, batchDelay)));
    }

    private IEnumerator InwardWaveRoutine(float scaleAmount, float duration, int batchSize, float batchDelay)
    {
        _waveSortBuffer.Clear();
        if (_waveSortBuffer.Capacity < _dots.Count) _waveSortBuffer.Capacity = _dots.Count;

        for (int i = 0; i < _dots.Count; i++) _waveSortBuffer.Add(i);
        _waveSortBuffer.Sort(CompareDotsByDistanceToWaveCenter);

        WaitForSecondsRealtime wait = batchDelay > 0f ? new WaitForSecondsRealtime(batchDelay) : null;
        int currentBatch = 0;

        for (int i = 0; i < _waveSortBuffer.Count; i++)
        {
            int dotIndex = _waveSortBuffer[i];
            if (dotIndex >= 0 && dotIndex < _dots.Count)
            {
                PlayLeaveEffectAt(_dots[dotIndex].gridPosition, scaleAmount, duration);
                currentBatch++;
            }

            if (currentBatch >= batchSize)
            {
                currentBatch = 0;
                if (wait != null) yield return wait;
                else yield return null;
            }
        }

        _waveRoutine = null;
    }

    private int CompareDotsByDistanceToWaveCenter(int a, int b)
    {
        float distA = (_dots[a].localPosition - _waveCenter).sqrMagnitude;
        float distB = (_dots[b].localPosition - _waveCenter).sqrMagnitude;
        return distA.CompareTo(distB);
    }

    private Vector3 CalculateBoundsCenter()
    {
        if (_dots.Count == 0) return Vector3.zero;

        Bounds bounds = new Bounds(_dots[0].localPosition, Vector3.zero);
        for (int i = 1; i < _dots.Count; i++) bounds.Encapsulate(_dots[i].localPosition);
        return bounds.center;
    }

    private void Update()
    {
        if (_activeAnimationCount <= 0)
        {
            if (_meshDirty) ApplyMeshChanges();
            return;
        }

        float now = Time.time;
        int activeCount = 0;

        for (int i = 0; i < _dots.Count; i++)
        {
            DotState dot = _dots[i];
            if (dot.animationType == DotAnimationType.None) continue;

            bool stillActive = EvaluateAnimation(ref dot, now);
            _dots[i] = dot;
            if (stillActive) activeCount++;
        }

        _activeAnimationCount = activeCount;
        _meshDirty = true;
        ApplyMeshChanges();
    }

    private bool EvaluateAnimation(ref DotState dot, float now)
    {
        float elapsed = now - dot.startTime - dot.delay;
        if (elapsed < 0f)
        {
            dot.scale = 1f;
            dot.color = _originalColor;
            return true;
        }

        float duration = Mathf.Max(0.001f, dot.duration);
        float halfDuration = duration * 0.5f;

        if (dot.animationType == DotAnimationType.Leave)
        {
            if (elapsed >= duration)
            {
                dot.scale = 1f;
                dot.color = _originalColor;
                dot.animationType = DotAnimationType.None;
                return false;
            }

            if (elapsed <= halfDuration)
            {
                float t = EaseOutQuad(elapsed / halfDuration);
                dot.scale = Mathf.Lerp(1f, dot.scaleAmount, t);
                dot.color = Color.Lerp(_originalColor, Color.white, t);
            }
            else
            {
                float t = EaseOutQuad((elapsed - halfDuration) / halfDuration);
                dot.scale = Mathf.Lerp(dot.scaleAmount, 1f, t);
                dot.color = Color.Lerp(Color.white, _originalColor, t);
            }

            return true;
        }

        if (dot.animationType == DotAnimationType.Win)
        {
            if (elapsed >= duration)
            {
                dot.scale = 0f;
                dot.color = dot.targetColor;
                dot.animationType = DotAnimationType.None;
                return false;
            }

            if (elapsed <= halfDuration)
            {
                float t = EaseOutQuad(elapsed / halfDuration);
                dot.scale = Mathf.Lerp(1f, dot.scaleAmount, t);
                dot.color = _originalColor;
            }
            else
            {
                float normalizedTime = (elapsed - halfDuration) / halfDuration;
                float scaleT = EaseInBack(normalizedTime);
                float colorT = EaseOutQuad(normalizedTime);
                dot.scale = Mathf.Lerp(dot.scaleAmount, 0f, scaleT);
                dot.color = Color.Lerp(_originalColor, dot.targetColor, colorT);
            }

            return true;
        }

        return false;
    }

    private void ApplyMeshChanges()
    {
        if (_mesh == null || _vertices == null || _colors == null) return;

        WriteAllDotVerticesAndColors();
        _mesh.vertices = _vertices;
        _mesh.colors = _colors;
        _mesh.RecalculateBounds();
        _meshDirty = false;
    }

    private void WriteAllDotVerticesAndColors()
    {
        for (int i = 0; i < _dots.Count; i++) WriteDotVerticesAndColors(i);
    }

    private void WriteDotVerticesAndColors(int dotIndex)
    {
        DotState dot = _dots[dotIndex];
        int vertexStart = dotIndex * 4;

        for (int i = 0; i < 4; i++)
        {
            _vertices[vertexStart + i] = dot.localPosition + _quadOffsets[i] * dot.scale;
            _colors[vertexStart + i] = dot.color;
        }
    }

    private void StopWaveRoutine()
    {
        if (_waveRoutine == null) return;
        StopCoroutine(_waveRoutine);
        _waveRoutine = null;
    }

    private static float EaseOutQuad(float t)
    {
        t = Mathf.Clamp01(t);
        return 1f - (1f - t) * (1f - t);
    }

    private static float EaseInBack(float t)
    {
        t = Mathf.Clamp01(t);
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }
}
