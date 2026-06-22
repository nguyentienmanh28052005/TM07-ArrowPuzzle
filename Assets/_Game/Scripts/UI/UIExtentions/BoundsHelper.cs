using System.Collections.Generic;
using UnityEngine;

public static class BoundsHelperUI
{
    public static Bounds GetBoundsFromPoints(List<Vector3> points, Vector3 sizeMap)
    {
        if (points == null || points.Count == 0)
            return new Bounds();

        Bounds totalBounds = new Bounds(points[0], Vector3.zero);
        for (int i = 1; i < points.Count; i++)
        {
            totalBounds.Encapsulate(points[i]);
        }
        totalBounds.Expand(sizeMap);
        return totalBounds;
    }
    
    public static Bounds GetBoundsFromPoints(List<Vector3> points, Bounds expand, Vector3 sizeMap)
    {
        if (points == null || points.Count == 0)
            return new Bounds();

        Bounds totalBounds = new Bounds(points[0], Vector3.zero);
        for (int i = 1; i < points.Count; i++)
        {
            totalBounds.Encapsulate(points[i]);
        }
        totalBounds.Expand(sizeMap);

        totalBounds.size += expand.size;
        totalBounds.center += expand.center;
        return totalBounds;
    }
}