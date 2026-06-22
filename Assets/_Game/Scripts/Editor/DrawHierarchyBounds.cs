using UnityEngine;
using static BoundsHelper;



#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Draw world-space bounds for a root object by aggregating all children MeshRenderers or MeshFilters.
/// Attach this to the root GameObject you want to visualize.
/// </summary>
[ExecuteAlways]
public class DrawHierarchyBounds : MonoBehaviour
{
    public enum Source
    {
        MeshRenderer,   // Use renderer.bounds (already in world space) - recommended
        MeshFilter      // Use sharedMesh.bounds (local) transformed to world
    }

    [Header("Bounds Source")]
    public Source source = Source.MeshRenderer;

    [Header("Gizmo Options")]
    public bool onlyWhenSelected = true;      // Draw only when this object is selected
    public Color wireColor = new Color(1f, 0.92f, 0.016f, 1f); // yellow-ish
    [Range(0f, 1f)] public float fillAlpha = 0.03f; // small alpha to see overlap
    public bool showSizeLabel = true;         // Show size text in Scene view
    public bool drawCorners = false;          // Draw 8 corner spheres

    [Header("Debug")]
    public bool logOnRecalc = false;          // Print bounds after recalculation

    // Cached bounds for quick access and to inspect in Inspector
    [SerializeField, Tooltip("World-space center of aggregated bounds")]
    private Vector3 cachedCenter;
    [SerializeField, Tooltip("World-space size of aggregated bounds")]
    private Vector3 cachedSize;

    private Bounds _cachedBounds;
    private int _lastHierarchyHash = -1;      // Simple change detector
#if UNITY_EDITOR
    private void OnEnable()
    {
        RecalculateIfNeeded();
    }

    private void Update()
    {
        // In Editor, hierarchy or transform changes are common => cheap guard
        RecalculateIfNeeded();
    }

    private void OnDrawGizmos()
    {
        if (onlyWhenSelected) return; // skip here; handled by OnDrawGizmosSelected
        DrawGizmoInternal();
    }

    private void OnDrawGizmosSelected()
    {
        if (!onlyWhenSelected) return;
        DrawGizmoInternal();
        OrientedBounds obb ;
        switch (source)
        {
            case Source.MeshRenderer:
                obb = CalculateOrientedBoundsFromMeshFilters(gameObject);
                break;
            case Source.MeshFilter:
                obb = CalculateOrientedBoundsFromMeshFilters(gameObject);
                break;
            default:
                obb = CalculateOrientedBoundsFromMeshFilters(gameObject);
                break;
        }

        // Draw oriented box in Scene view
        DrawOrientedGizmo(obb, new Color(1f, 0.6f, 0.1f, 1f), 0.04f);

        // If you need a classic AABB for Physics/Camera fit:
        Bounds worldAABB = ToWorldAABB(obb);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(worldAABB.center, worldAABB.size);
    }

    private void DrawGizmoInternal()
    {
        // Ensure cache is up-to-date before drawing
        RecalculateIfNeeded();

        // Configure colors
        Color prev = Gizmos.color;
        Gizmos.color = new Color(wireColor.r, wireColor.g, wireColor.b, Mathf.Clamp01(fillAlpha));

        // Draw filled cube (very low alpha)
        Gizmos.DrawCube(_cachedBounds.center, _cachedBounds.size);

        // Draw wireframe on top
        Gizmos.color = wireColor;
        Gizmos.DrawWireCube(_cachedBounds.center, _cachedBounds.size);

        // Optionally draw 8 corner markers
        if (drawCorners)
        {
            Vector3 c = _cachedBounds.center;
            Vector3 e = _cachedBounds.extents;
            Vector3[] pts = new Vector3[8];
            // Build 8 corners with for-loop as user preference
            pts[0] = c + new Vector3(e.x, e.y, e.z);
            pts[1] = c + new Vector3(e.x, e.y, -e.z);
            pts[2] = c + new Vector3(e.x, -e.y, e.z);
            pts[3] = c + new Vector3(e.x, -e.y, -e.z);
            pts[4] = c + new Vector3(-e.x, e.y, e.z);
            pts[5] = c + new Vector3(-e.x, e.y, -e.z);
            pts[6] = c + new Vector3(-e.x, -e.y, e.z);
            pts[7] = c + new Vector3(-e.x, -e.y, -e.z);

            float r = HandleUtility.GetHandleSize(_cachedBounds.center) * 0.02f;
            for (int i = 0; i < pts.Length; i++)
            {
                Gizmos.DrawSphere(pts[i], r);
            }
        }

        // SceneView size label
        if (showSizeLabel)
        {
            Handles.color = wireColor;
            string label = $"Bounds size:\nX={_cachedBounds.size.x:F3}\nY={_cachedBounds.size.y:F3}\nZ={_cachedBounds.size.z:F3}";
            Vector3 labelPos = _cachedBounds.center + Vector3.up * (_cachedBounds.extents.y + 0.02f * HandleUtility.GetHandleSize(_cachedBounds.center));
            Handles.Label(labelPos, label);
        }

        Gizmos.color = prev;
    }
#endif

    /// <summary>
    /// Public API: get latest world bounds (forces recompute).
    /// </summary>
    public Bounds GetWorldBounds()
    {
        RecalculateBounds();
        return _cachedBounds;
    }

    /// <summary>
    /// Recalculate only when hierarchy/transform likely changed.
    /// </summary>
    private void RecalculateIfNeeded()
    {
        int hashNow = ComputeHierarchyHash();
        if (hashNow != _lastHierarchyHash)
        {
            RecalculateBounds();
            _lastHierarchyHash = hashNow;
        }
    }

    /// <summary>
    /// Compute an inexpensive hash from child count and local transforms.
    /// </summary>
    private int ComputeHierarchyHash()
    {
        unchecked
        {
            int hash = 17;
            Transform[] all = GetComponentsInChildren<Transform>(true);
            hash = hash * 31 + all.Length;

            // Use a limited sample to keep it cheap in large hierarchies
            for (int i = 0; i < all.Length; i++)
            {
                Transform t = all[i];
                // Incorporate position/rotation/scale roughly
                Vector3 p = t.position;
                Vector3 s = t.lossyScale;
                Quaternion r = t.rotation;

                hash = hash * 31 + p.GetHashCode();
                hash = hash * 31 + s.GetHashCode();
                hash = hash * 31 + r.GetHashCode();
            }
            return hash;
        }
    }

    /// <summary>
    /// Recalculate aggregated world bounds from chosen source.
    /// </summary>
    [ContextMenu("Recalculate Bounds Now")]
    public void RecalculateBounds()
    {
        Bounds b;

        if (source == Source.MeshRenderer)
        {
            b = BoundsHelper.CalculateRendererBounds(gameObject);

        }
        else
        {
            b = BoundsHelper.CalculateMeshFilterBounds(gameObject);
        }

        _cachedBounds = b;
        cachedCenter = b.center;
        cachedSize = b.size;

        if (logOnRecalc)
        {
            Debug.Log($"[DrawHierarchyBounds] center={b.center}, size={b.size}", this);
        }
    }


    
}
