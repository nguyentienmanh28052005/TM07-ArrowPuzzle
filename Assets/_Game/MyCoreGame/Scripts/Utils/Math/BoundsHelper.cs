using System;
using UnityEngine;

public static class BoundsHelper
{
    /// <summary>
    /// Calculate world-space bounds of a GameObject including all children MeshRenderers.
    /// This is usually the fastest and most reliable way for camera framing.
    /// </summary>
    public static Bounds CalculateRendererBounds(GameObject root)
    {
        MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(root.transform.position, Vector3.zero);
        }

        Bounds total = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            total.Encapsulate(renderers[i].bounds);
        }
        return total;
    }

    /// <summary>
    /// Calculate world-space bounds of a GameObject including all children MeshFilters.
    /// This version transforms MeshFilter.sharedMesh.bounds (local) into world space.
    /// </summary>
    public static Bounds CalculateMeshFilterBounds(GameObject root)
    {
        MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();
        if (filters.Length == 0)
        {
            return new Bounds(root.transform.position, Vector3.zero);
        }

        Bounds total = TransformBounds(filters[0].sharedMesh.bounds, filters[0].transform);
        for (int i = 1; i < filters.Length; i++)
        {
            if (filters[i].sharedMesh == null) continue;
            Bounds worldBound = TransformBounds(filters[i].sharedMesh.bounds, filters[i].transform);
            total.Encapsulate(worldBound);
        }
        return total;
    }

    /// <summary>
    /// Convert a local-space bounds to world-space bounds.
    /// </summary>
    private static Bounds TransformBounds(Bounds localBounds, Transform t)
    {
        Vector3 c = localBounds.center;
        Vector3 e = localBounds.extents;

        // Build 8 corner points
        Vector3[] pts = new Vector3[8];
        pts[0] = c + new Vector3(e.x, e.y, e.z);
        pts[1] = c + new Vector3(e.x, e.y, -e.z);
        pts[2] = c + new Vector3(e.x, -e.y, e.z);
        pts[3] = c + new Vector3(e.x, -e.y, -e.z);
        pts[4] = c + new Vector3(-e.x, e.y, e.z);
        pts[5] = c + new Vector3(-e.x, e.y, -e.z);
        pts[6] = c + new Vector3(-e.x, -e.y, e.z);
        pts[7] = c + new Vector3(-e.x, -e.y, -e.z);

        for (int i = 0; i < 8; i++)
        {
            pts[i] = t.TransformPoint(pts[i]);
        }

        Bounds world = new Bounds(pts[0], Vector3.zero);
        for (int i = 1; i < pts.Length; i++)
        {
            world.Encapsulate(pts[i]);
        }

        return world;
    }

    // ------------------------- OBB TYPE -------------------------
    /// <summary>
    /// Oriented Bounding Box aligned with a given Transform (space).
    /// sizeLocal is measured in that Transform's local space.
    /// </summary>
    [Serializable]
    public struct OrientedBounds
    {
        public Transform space;      // orientation & scale provider (usually the root)
        public Vector3 centerLocal;  // center in 'space' local coordinates
        public Vector3 sizeLocal;    // size in 'space' local axes (local units)

        public Vector3 CenterWorld => space ? space.TransformPoint(centerLocal) : centerLocal;
        public Quaternion RotationWorld => space ? space.rotation : Quaternion.identity;
        public Matrix4x4 MatrixAtCenter =>
            space ? (space.localToWorldMatrix * Matrix4x4.Translate(centerLocal))
                  : Matrix4x4.TRS(centerLocal, Quaternion.identity, Vector3.one);

        public Vector3 ExtentsLocal => sizeLocal * 0.5f;
    }

    // ------------------------- OBB BUILDERS -------------------------
    /// <summary>
    /// Exact OBB aligned with root.transform (recommended).
    /// Aggregates all MeshFilters by transforming their local mesh-bounds corners into root local space.
    /// Works with non-uniform scale and nested rotations.
    /// </summary>
    public static OrientedBounds CalculateOrientedBoundsFromMeshFilters(GameObject root)
    {
        Transform rt = root.transform;
        MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();

        // No meshes => zero OBB at root pivot
        if (filters.Length == 0)
        {
            return new OrientedBounds
            {
                space = rt,
                centerLocal = Vector3.zero,
                sizeLocal = Vector3.zero
            };
        }

        Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        // For each mesh, transform its 8 local AABB corners -> world -> root-local, then expand min/max
        for (int i = 0; i < filters.Length; i++)
        {
            MeshFilter mf = filters[i];
            Mesh mesh = mf.sharedMesh;
            if (mesh == null) continue;

            Bounds lb = mesh.bounds;               // local-space mesh bounds
            Vector3 c = lb.center;
            Vector3 e = lb.extents;

            // 8 local corners
            Vector3[] localCorners = new Vector3[8];
            localCorners[0] = c + new Vector3(e.x, e.y, e.z);
            localCorners[1] = c + new Vector3(e.x, e.y, -e.z);
            localCorners[2] = c + new Vector3(e.x, -e.y, e.z);
            localCorners[3] = c + new Vector3(e.x, -e.y, -e.z);
            localCorners[4] = c + new Vector3(-e.x, e.y, e.z);
            localCorners[5] = c + new Vector3(-e.x, e.y, -e.z);
            localCorners[6] = c + new Vector3(-e.x, -e.y, e.z);
            localCorners[7] = c + new Vector3(-e.x, -e.y, -e.z);

            for (int k = 0; k < 8; k++)
            {
                // to world then to root-local
                Vector3 w = mf.transform.TransformPoint(localCorners[k]);
                Vector3 rl = rt.InverseTransformPoint(w);

                // expand local min/max
                if (rl.x < min.x) min.x = rl.x;
                if (rl.y < min.y) min.y = rl.y;
                if (rl.z < min.z) min.z = rl.z;
                if (rl.x > max.x) max.x = rl.x;
                if (rl.y > max.y) max.y = rl.y;
                if (rl.z > max.z) max.z = rl.z;
            }
        }

        Vector3 sizeLocal = new Vector3(max.x - min.x, max.y - min.y, max.z - min.z);
        Vector3 centerLocal = (min + max) * 0.5f;

        return new OrientedBounds
        {
            space = rt,
            centerLocal = centerLocal,
            sizeLocal = sizeLocal
        };
    }

    /// <summary>
    /// Fast OBB using MeshRenderer.bounds (world AABB) converted to root local.
    /// Slightly more conservative than the MeshFilter method (because renderer.bounds is already AABB).
    /// </summary>
    public static OrientedBounds CalculateOrientedBoundsFromRenderers(GameObject root)
    {
        Transform rt = root.transform;
        MeshRenderer[] rends = root.GetComponentsInChildren<MeshRenderer>();
        if (rends.Length == 0)
        {
            return new OrientedBounds { space = rt, centerLocal = Vector3.zero, sizeLocal = Vector3.zero };
        }

        Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        for (int i = 0; i < rends.Length; i++)
        {
            Bounds wb = rends[i].bounds; // world AABB of that renderer
            Vector3 c = wb.center;
            Vector3 e = wb.extents;

            // 8 world corners of that AABB
            Vector3[] worldCorners = new Vector3[8];
            worldCorners[0] = c + new Vector3(e.x, e.y, e.z);
            worldCorners[1] = c + new Vector3(e.x, e.y, -e.z);
            worldCorners[2] = c + new Vector3(e.x, -e.y, e.z);
            worldCorners[3] = c + new Vector3(e.x, -e.y, -e.z);
            worldCorners[4] = c + new Vector3(-e.x, e.y, e.z);
            worldCorners[5] = c + new Vector3(-e.x, e.y, -e.z);
            worldCorners[6] = c + new Vector3(-e.x, -e.y, e.z);
            worldCorners[7] = c + new Vector3(-e.x, -e.y, -e.z);

            for (int k = 0; k < 8; k++)
            {
                Vector3 rl = rt.InverseTransformPoint(worldCorners[k]);
                if (rl.x < min.x) min.x = rl.x;
                if (rl.y < min.y) min.y = rl.y;
                if (rl.z < min.z) min.z = rl.z;
                if (rl.x > max.x) max.x = rl.x;
                if (rl.y > max.y) max.y = rl.y;
                if (rl.z > max.z) max.z = rl.z;
            }
        }

        Vector3 sizeLocal = new Vector3(max.x - min.x, max.y - min.y, max.z - min.z);
        Vector3 centerLocal = (min + max) * 0.5f;

        return new OrientedBounds
        {
            space = rt,
            centerLocal = centerLocal,
            sizeLocal = sizeLocal
        };
    }

    // ------------------------- OBB UTILITIES -------------------------
    /// <summary>
    /// Get 8 world corners of an OrientedBounds.
    /// </summary>
    public static void GetWorldCorners(in OrientedBounds obb, Vector3[] out8)
    {
        if (out8 == null || out8.Length < 8) return;

        Vector3 e = obb.ExtentsLocal;
        // 8 corners in local OBB space (center at zero)
        Vector3[] local = new Vector3[8];
        local[0] = new Vector3(e.x, e.y, e.z);
        local[1] = new Vector3(e.x, e.y, -e.z);
        local[2] = new Vector3(e.x, -e.y, e.z);
        local[3] = new Vector3(e.x, -e.y, -e.z);
        local[4] = new Vector3(-e.x, e.y, e.z);
        local[5] = new Vector3(-e.x, e.y, -e.z);
        local[6] = new Vector3(-e.x, -e.y, e.z);
        local[7] = new Vector3(-e.x, -e.y, -e.z);

        // Transform by matrix at center (includes root rotation & scale)
        Matrix4x4 M = obb.MatrixAtCenter;
        for (int i = 0; i < 8; i++)
        {
            out8[i] = M.MultiplyPoint3x4(local[i]);
        }
    }

    /// <summary>
    /// Convert an OrientedBounds to world AABB (Unity Bounds).
    /// </summary>
    public static Bounds ToWorldAABB(in OrientedBounds obb)
    {
        Vector3[] pts = new Vector3[8];
        GetWorldCorners(obb, pts);

        Bounds aabb = new Bounds(pts[0], Vector3.zero);
        for (int i = 1; i < 8; i++)
        {
            aabb.Encapsulate(pts[i]);
        }
        return aabb;
    }

    // ------------------------- (Optional) GIZMO DRAW -------------------------
    /// <summary>
    /// Draw an oriented wire/solid box for visualization (usable in play mode too).
    /// </summary>
    public static void DrawOrientedGizmo(in OrientedBounds obb, Color wireColor, float fillAlpha = 0.03f)
    {
        Color prev = Gizmos.color;
        Matrix4x4 prevM = Gizmos.matrix;

        // Place gizmo-space at OBB center using full root TRS
        Gizmos.matrix = obb.MatrixAtCenter;

        // Filled (tiny alpha) then wire
        Gizmos.color = new Color(wireColor.r, wireColor.g, wireColor.b, Mathf.Clamp01(fillAlpha));
        Gizmos.DrawCube(Vector3.zero, obb.sizeLocal);
        Gizmos.color = wireColor;
        Gizmos.DrawWireCube(Vector3.zero, obb.sizeLocal);

        Gizmos.matrix = prevM;
        Gizmos.color = prev;
    }
}
