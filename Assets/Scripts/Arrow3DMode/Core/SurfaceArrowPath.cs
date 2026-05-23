using System.Collections.Generic;
using UnityEngine;

namespace Arrow3DMode.Core
{
    public sealed class SurfaceArrowPath
    {
        private const float MinSegmentDistance = 0.0001f;

        private readonly List<SurfaceSample> _samples = new List<SurfaceSample>(128);
        private readonly List<SurfaceSample> _moveBuffer = new List<SurfaceSample>(8);

        private float _cubeSize = 3f;
        private float _totalLength = 3f;

        public SurfacePose HeadPose { get; private set; }
        public IReadOnlyList<SurfaceSample> Samples => _samples;

        public void Reset(SurfacePose headPose, float cubeSize, float totalLength, float buildStep)
        {
            _cubeSize = Mathf.Max(0.1f, cubeSize);
            _totalLength = Mathf.Max(0.1f, totalLength);

            HeadPose = NormalizePose(headPose);
            _samples.Clear();
            _samples.Add(CubeSurfaceMath.CreateSample(HeadPose, _cubeSize));

            SurfacePose tailBuilder = HeadPose;
            tailBuilder.LocalForward = -HeadPose.LocalForward;

            float remaining = _totalLength;
            float step = Mathf.Clamp(buildStep, 0.02f, _cubeSize);

            while (remaining > MinSegmentDistance)
            {
                _moveBuffer.Clear();
                float distance = Mathf.Min(step, remaining);
                CubeSurfaceMath.AdvanceAroundCube(ref tailBuilder, _cubeSize, distance, _moveBuffer);
                AppendBuildSamples(_moveBuffer);
                remaining -= distance;
            }

            TrimToLength();
        }

        public void Move(float distance)
        {
            if (distance <= 0f || _samples.Count == 0)
            {
                return;
            }

            SurfacePose next = HeadPose;
            _moveBuffer.Clear();
            CubeSurfaceMath.AdvanceOnCurrentPlane(ref next, _cubeSize, distance, _moveBuffer);
            for (int i = 0; i < _moveBuffer.Count; i++)
            {
                InsertHeadSample(_moveBuffer[i]);
            }

            HeadPose = NormalizePose(next);
            TrimToLength();
        }

        public void TurnHead(bool turnRight)
        {
            HeadPose = new SurfacePose(
                HeadPose.Point,
                CubeSurfaceMath.Turn(HeadPose.LocalForward, HeadPose.Point.Face, turnRight));

            if (_samples.Count > 0)
            {
                SurfaceSample head = _samples[0];
                head.LocalForward = HeadPose.LocalForward;
                _samples[0] = head;
            }
        }

        public void GetBodySamples(int count, List<SurfaceSample> output)
        {
            if (output == null)
            {
                return;
            }

            output.Clear();
            if (count <= 0 || _samples.Count == 0)
            {
                return;
            }

            if (count == 1)
            {
                output.Add(_samples[0]);
                return;
            }

            float interval = _totalLength / (count - 1);
            for (int i = 0; i < count; i++)
            {
                output.Add(SampleAtDistance(interval * i));
            }
        }

        public SurfaceSample SampleAtDistance(float distance)
        {
            if (_samples.Count == 0)
            {
                return default;
            }

            if (distance <= 0f || _samples.Count == 1)
            {
                SurfaceSample head = _samples[0];
                head.LocalForward = HeadPose.LocalForward;
                return head;
            }

            float walked = 0f;
            for (int i = 0; i < _samples.Count - 1; i++)
            {
                SurfaceSample a = _samples[i];
                SurfaceSample b = _samples[i + 1];
                float segmentLength = Vector3.Distance(a.LocalPosition, b.LocalPosition);
                if (segmentLength <= MinSegmentDistance)
                {
                    continue;
                }

                if (walked + segmentLength >= distance)
                {
                    float t = Mathf.Clamp01((distance - walked) / segmentLength);
                    return Interpolate(a, b, t);
                }

                walked += segmentLength;
            }

            SurfaceSample tail = _samples[_samples.Count - 1];
            if (_samples.Count >= 2)
            {
                SurfaceSample beforeTail = _samples[_samples.Count - 2];
                tail.LocalForward = (beforeTail.LocalPosition - tail.LocalPosition).normalized;
            }

            return tail;
        }

        private SurfacePose NormalizePose(SurfacePose pose)
        {
            pose.LocalForward = CubeSurfaceMath.ProjectDirection(pose.LocalForward, pose.Point.Face);
            return pose;
        }

        private void AppendBuildSamples(List<SurfaceSample> samples)
        {
            for (int i = 0; i < samples.Count; i++)
            {
                SurfaceSample sample = samples[i];
                sample.LocalForward = -sample.LocalForward;

                if (_samples.Count > 0)
                {
                    Vector3 delta = _samples[_samples.Count - 1].LocalPosition - sample.LocalPosition;
                    if (delta.sqrMagnitude <= MinSegmentDistance * MinSegmentDistance)
                    {
                        continue;
                    }
                }

                _samples.Add(sample);
            }
        }

        private void InsertHeadSample(SurfaceSample sample)
        {
            if (_samples.Count > 0)
            {
                Vector3 delta = _samples[0].LocalPosition - sample.LocalPosition;
                if (delta.sqrMagnitude <= MinSegmentDistance * MinSegmentDistance)
                {
                    return;
                }
            }

            _samples.Insert(0, sample);
        }

        private void TrimToLength()
        {
            if (_samples.Count <= 1)
            {
                return;
            }

            float walked = 0f;
            for (int i = 0; i < _samples.Count - 1; i++)
            {
                SurfaceSample a = _samples[i];
                SurfaceSample b = _samples[i + 1];
                float segmentLength = Vector3.Distance(a.LocalPosition, b.LocalPosition);

                if (segmentLength <= MinSegmentDistance)
                {
                    continue;
                }

                if (walked + segmentLength >= _totalLength)
                {
                    float t = Mathf.Clamp01((_totalLength - walked) / segmentLength);
                    SurfaceSample tail = Interpolate(a, b, t);
                    _samples.RemoveRange(i + 1, _samples.Count - (i + 1));
                    _samples.Add(tail);
                    return;
                }

                walked += segmentLength;
            }
        }

        private static SurfaceSample Interpolate(SurfaceSample a, SurfaceSample b, float t)
        {
            Vector3 position = Vector3.Lerp(a.LocalPosition, b.LocalPosition, t);
            Vector3 normal = Vector3.Lerp(a.LocalNormal, b.LocalNormal, t).normalized;
            if (normal.sqrMagnitude <= MinSegmentDistance)
            {
                normal = a.LocalNormal;
            }

            Vector3 forward = (a.LocalPosition - b.LocalPosition).normalized;
            if (forward.sqrMagnitude <= MinSegmentDistance)
            {
                forward = a.LocalForward;
            }

            SurfacePoint point = t < 0.5f ? a.Point : b.Point;
            return new SurfaceSample(point, position, normal, forward);
        }
    }
}
