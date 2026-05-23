using System;
using UnityEngine;

namespace Arrow3DMode.Core
{
    public enum CubeFace
    {
        PositiveX,
        NegativeX,
        PositiveY,
        NegativeY,
        PositiveZ,
        NegativeZ
    }

    public enum SurfaceDirection
    {
        PositiveU,
        NegativeU,
        PositiveV,
        NegativeV
    }

    [Serializable]
    public struct SurfacePoint
    {
        public CubeFace Face;
        [Range(0f, 1f)] public float U;
        [Range(0f, 1f)] public float V;

        public SurfacePoint(CubeFace face, float u, float v)
        {
            Face = face;
            U = u;
            V = v;
        }
    }

    public struct SurfaceFrame
    {
        public Vector3 Normal;
        public Vector3 AxisU;
        public Vector3 AxisV;
    }

    public struct SurfacePose
    {
        public SurfacePoint Point;
        public Vector3 LocalForward;

        public SurfacePose(SurfacePoint point, Vector3 localForward)
        {
            Point = point;
            LocalForward = localForward;
        }
    }

    public struct SurfaceSample
    {
        public SurfacePoint Point;
        public Vector3 LocalPosition;
        public Vector3 LocalNormal;
        public Vector3 LocalForward;

        public SurfaceSample(SurfacePoint point, Vector3 localPosition, Vector3 localNormal, Vector3 localForward)
        {
            Point = point;
            LocalPosition = localPosition;
            LocalNormal = localNormal;
            LocalForward = localForward;
        }
    }
}
