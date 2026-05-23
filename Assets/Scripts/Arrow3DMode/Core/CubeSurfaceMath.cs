using System.Collections.Generic;
using UnityEngine;

namespace Arrow3DMode.Core
{
    public static class CubeSurfaceMath
    {
        private const float Epsilon = 0.00001f;
        private const float LargeDistance = 100000f;

        public static SurfaceFrame GetFrame(CubeFace face)
        {
            switch (face)
            {
                case CubeFace.PositiveX:
                    return new SurfaceFrame { Normal = Vector3.right, AxisU = Vector3.back, AxisV = Vector3.up };
                case CubeFace.NegativeX:
                    return new SurfaceFrame { Normal = Vector3.left, AxisU = Vector3.forward, AxisV = Vector3.up };
                case CubeFace.PositiveY:
                    return new SurfaceFrame { Normal = Vector3.up, AxisU = Vector3.right, AxisV = Vector3.back };
                case CubeFace.NegativeY:
                    return new SurfaceFrame { Normal = Vector3.down, AxisU = Vector3.right, AxisV = Vector3.forward };
                case CubeFace.PositiveZ:
                    return new SurfaceFrame { Normal = Vector3.forward, AxisU = Vector3.right, AxisV = Vector3.up };
                default:
                    return new SurfaceFrame { Normal = Vector3.back, AxisU = Vector3.left, AxisV = Vector3.up };
            }
        }

        public static Vector3 Direction(CubeFace face, SurfaceDirection direction)
        {
            SurfaceFrame frame = GetFrame(face);
            switch (direction)
            {
                case SurfaceDirection.NegativeU:
                    return -frame.AxisU;
                case SurfaceDirection.PositiveV:
                    return frame.AxisV;
                case SurfaceDirection.NegativeV:
                    return -frame.AxisV;
                default:
                    return frame.AxisU;
            }
        }

        public static SurfacePose CreatePose(SurfacePoint point, SurfaceDirection direction)
        {
            Vector3 forward = Direction(point.Face, direction);
            return new SurfacePose(point, forward);
        }

        public static SurfaceSample CreateSample(SurfacePose pose, float cubeSize)
        {
            SurfaceFrame frame = GetFrame(pose.Point.Face);
            Vector3 forward = ProjectDirection(pose.LocalForward, pose.Point.Face);
            return new SurfaceSample(
                pose.Point,
                SurfaceToLocal(pose.Point, cubeSize),
                frame.Normal,
                forward);
        }

        public static Vector3 SurfaceToLocal(SurfacePoint point, float cubeSize)
        {
            float half = cubeSize * 0.5f;
            SurfaceFrame frame = GetFrame(point.Face);
            float u = point.U;
            float v = point.V;
            return frame.Normal * half
                + frame.AxisU * ((u - 0.5f) * cubeSize)
                + frame.AxisV * ((v - 0.5f) * cubeSize);
        }

        public static SurfacePoint LocalToSurface(Vector3 localPosition, CubeFace face, float cubeSize)
        {
            SurfaceFrame frame = GetFrame(face);
            float invSize = cubeSize > Epsilon ? 1f / cubeSize : 1f;
            float u = Vector3.Dot(localPosition, frame.AxisU) * invSize + 0.5f;
            float v = Vector3.Dot(localPosition, frame.AxisV) * invSize + 0.5f;
            return new SurfacePoint(face, u, v);
        }

        public static Vector3 ProjectDirection(Vector3 localForward, CubeFace face)
        {
            SurfaceFrame frame = GetFrame(face);
            Vector3 projected = Vector3.ProjectOnPlane(localForward, frame.Normal);
            if (projected.sqrMagnitude <= Epsilon)
            {
                projected = frame.AxisU;
            }

            return projected.normalized;
        }

        public static Vector3 Turn(Vector3 localForward, CubeFace face, bool turnRight)
        {
            SurfaceFrame frame = GetFrame(face);
            Vector3 forward = ProjectDirection(localForward, face);
            Vector3 turned = turnRight
                ? Vector3.Cross(forward, frame.Normal)
                : Vector3.Cross(frame.Normal, forward);

            return ProjectDirection(turned, face);
        }

        public static void AdvanceOnCurrentPlane(ref SurfacePose pose, float cubeSize, float distance, List<SurfaceSample> samples)
        {
            if (distance <= 0f)
            {
                return;
            }

            Vector3 position = SurfaceToLocal(pose.Point, cubeSize);
            Vector3 forward = ProjectDirection(pose.LocalForward, pose.Point.Face);
            position += forward * distance;

            pose.Point = LocalToSurface(position, pose.Point.Face, cubeSize);
            pose.LocalForward = forward;
            AddSample(samples, pose, cubeSize);
        }

        public static void AdvanceAroundCube(ref SurfacePose pose, float cubeSize, float distance, List<SurfaceSample> samples)
        {
            if (distance <= 0f)
            {
                return;
            }

            float remaining = distance;
            int guard = 0;

            while (remaining > Epsilon && guard++ < 32)
            {
                SurfaceFrame frame = GetFrame(pose.Point.Face);
                Vector3 position = SurfaceToLocal(pose.Point, cubeSize);
                Vector3 forward = ProjectDirection(pose.LocalForward, pose.Point.Face);

                CubeFace nextFace;
                float distanceToEdge = FindDistanceToEdge(position, pose.Point.Face, forward, cubeSize, out nextFace);

                if (remaining <= distanceToEdge + Epsilon)
                {
                    position += forward * remaining;
                    pose.Point = LocalToSurface(position, pose.Point.Face, cubeSize);
                    pose.LocalForward = forward;
                    AddSample(samples, pose, cubeSize);
                    break;
                }

                position += forward * Mathf.Max(0f, distanceToEdge);
                pose.Point = LocalToSurface(position, pose.Point.Face, cubeSize);
                pose.LocalForward = forward;
                AddSample(samples, pose, cubeSize);

                Vector3 oldNormal = frame.Normal;
                pose.Point = LocalToSurface(position, nextFace, cubeSize);
                pose.LocalForward = ProjectDirection(-oldNormal, nextFace);
                remaining -= Mathf.Max(distanceToEdge, Epsilon);
            }
        }

        private static void AddSample(List<SurfaceSample> samples, SurfacePose pose, float cubeSize)
        {
            if (samples == null)
            {
                return;
            }

            SurfaceSample sample = CreateSample(pose, cubeSize);
            if (samples.Count > 0)
            {
                Vector3 delta = samples[samples.Count - 1].LocalPosition - sample.LocalPosition;
                if (delta.sqrMagnitude <= Epsilon * Epsilon)
                {
                    return;
                }
            }

            samples.Add(sample);
        }

        private static float FindDistanceToEdge(Vector3 position, CubeFace face, Vector3 forward, float cubeSize, out CubeFace nextFace)
        {
            float half = cubeSize * 0.5f;
            float best = LargeDistance;
            nextFace = face;

            ConsiderAxis(position.x, forward.x, half, CubeFace.PositiveX, CubeFace.NegativeX, ref best, ref nextFace);
            ConsiderAxis(position.y, forward.y, half, CubeFace.PositiveY, CubeFace.NegativeY, ref best, ref nextFace);
            ConsiderAxis(position.z, forward.z, half, CubeFace.PositiveZ, CubeFace.NegativeZ, ref best, ref nextFace);

            SurfaceFrame frame = GetFrame(face);
            if (nextFace == face || Mathf.Abs(Vector3.Dot(frame.Normal, GetFrame(nextFace).Normal)) > 0.5f)
            {
                nextFace = face;
                best = LargeDistance;
            }

            return best;
        }

        private static void ConsiderAxis(
            float coordinate,
            float direction,
            float half,
            CubeFace positiveFace,
            CubeFace negativeFace,
            ref float best,
            ref CubeFace nextFace)
        {
            if (Mathf.Abs(direction) <= Epsilon)
            {
                return;
            }

            float target = direction > 0f ? half : -half;
            float distance = (target - coordinate) / direction;
            if (distance >= -Epsilon && distance < best)
            {
                best = Mathf.Max(0f, distance);
                nextFace = direction > 0f ? positiveFace : negativeFace;
            }
        }
    }
}
