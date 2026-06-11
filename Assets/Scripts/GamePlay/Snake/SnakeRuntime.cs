using System.Collections.Generic;
using UnityEngine;

public sealed class SnakeRuntime
{
    private static readonly List<SnakeBlock> ActiveSnakeBuffer = new List<SnakeBlock>(128);

    private readonly HashSet<Vector2Int> _occupiedCells = new HashSet<Vector2Int>();

    public static IReadOnlyList<SnakeBlock> ActiveSnakes => ActiveSnakeBuffer;

    public List<Vector3> LogicNodes { get; } = new List<Vector3>();
    public Vector3[] OriginalState { get; private set; }
    public Vector3[] CurrentPositions { get; private set; }
    public int TotalPoints { get; private set; }
    public int NodesPerUnit { get; private set; }
    public bool IsInitialized { get; private set; }

    public float AccumulatedShift { get; set; }
    public bool IsMoving { get; set; }
    public bool IsSpawning { get; set; }
    public float VisiblePoints { get; set; }
    public bool IsBeingErased { get; set; }
    public float EraseTailTrackIdx { get; set; }
    public bool IsBeingConsumedByBlackHole { get; set; }
    public float BlackHoleConsumeHeadTrackIdx { get; set; }
    public float BlackHoleConsumeTailTrackIdx { get; set; }
    public bool HasDealtDamage { get; set; }
    public bool HasArrowShadow { get; private set; }
    public bool ArrowShadowReleased { get; set; }
    public bool Outed { get; set; }
    public bool IsStoppedByStopBlock { get; set; }
    public GridStopBlock HoldingStopBlock { get; set; }

    public ObstacleHitType LastObstacleType { get; private set; } = ObstacleHitType.None;
    public Vector2Int LastObstacleCell { get; private set; } = new Vector2Int(int.MinValue, int.MinValue);

    public List<PathWarp> ActiveWarps { get; } = new List<PathWarp>(8);
    public int LastPassedPortalIndex { get; set; } = -1;
    public int LastPassedDeflectorIndex { get; set; } = -1;

    public void Initialize(List<Vector2Int> gridPositions, int resolution, bool hasArrowShadow)
    {
        NodesPerUnit = resolution;
        HasArrowShadow = hasArrowShadow;

        LogicNodes.Clear();
        if (gridPositions != null)
        {
            foreach (Vector2Int pos in gridPositions)
                LogicNodes.Add(new Vector3(pos.x, pos.y, 0f));
        }

        BuildSampledState();
        IsInitialized = true;
        VisiblePoints = 0f;
        IsBeingErased = false;
        IsBeingConsumedByBlackHole = false;
        EraseTailTrackIdx = TotalPoints > 0 ? TotalPoints - 1 : 0f;
        Outed = false;
        AccumulatedShift = 0f;
        IsMoving = false;
        IsSpawning = false;
        IsStoppedByStopBlock = false;
        HoldingStopBlock = null;
        HasDealtDamage = false;
        ArrowShadowReleased = false;
        ResetHit();
        ClearWarps();
    }

    public void SetHasArrowShadow(bool enabled)
    {
        HasArrowShadow = enabled;
        ArrowShadowReleased = false;
    }

    public Vector3 GetHeadPosition(Vector3 fallback, ArrowDir direction)
    {
        if (!IsInitialized || OriginalState == null || OriginalState.Length == 0)
            return fallback;

        return GetPositionAtTrackIndex(-AccumulatedShift, direction);
    }

    public void CopyOriginalToCurrent()
    {
        if (OriginalState == null || CurrentPositions == null || TotalPoints <= 0) return;
        System.Array.Copy(OriginalState, CurrentPositions, TotalPoints);
    }

    public void ResetMovementPose()
    {
        CopyOriginalToCurrent();
        AccumulatedShift = 0f;
        ClearWarps();
    }

    public float ScanPath(SnakeBlock self, ArrowDir direction, int maxPathScanCells, float exitTravelDistance)
    {
        if (!IsInitialized || OriginalState == null || GridManager.Instance == null)
            return float.MaxValue;

        ClearWarps();
        ResetHit();

        Vector2Int currentPos = new Vector2Int(
            Mathf.RoundToInt(OriginalState[0].x),
            Mathf.RoundToInt(OriginalState[0].y));

        if (PathScanner.GetDirStep(direction) == Vector2Int.zero)
            return float.MaxValue;

        int scanLimit = Mathf.Max(50, maxPathScanCells, Mathf.CeilToInt(exitTravelDistance) + 5, 512);
        MoveResult result = PathScanner.Scan(
            BoardState.Active,
            self,
            currentPos,
            direction,
            scanLimit);

        ActiveWarps.AddRange(result.Warps);
        LastObstacleType = result.Hit.Type;
        LastObstacleCell = result.Hit.Cell;
        return result.DistanceToObstacle;
    }

    public Vector3 GetPositionAtTrackIndex(float trackIndex, ArrowDir direction, bool snapToPortalEntryForRender = false)
    {
        if (OriginalState == null || TotalPoints <= 0)
            return Vector3.zero;

        if (trackIndex <= 0f)
        {
            float distForward = Mathf.Abs(trackIndex) / Mathf.Max(1, NodesPerUnit);
            return GetForwardPositionWithWarps(distForward, direction, snapToPortalEntryForRender);
        }

        if (trackIndex >= TotalPoints - 1)
            return OriginalState[TotalPoints - 1];

        int idx = Mathf.FloorToInt(trackIndex);
        float t = trackIndex - idx;
        return Vector3.Lerp(OriginalState[idx], OriginalState[idx + 1], t);
    }

    public float GetClosestTrackIndex(Vector3 worldPosition)
    {
        if (OriginalState == null || TotalPoints <= 1) return 0f;

        Vector2 point = worldPosition;
        float bestSqrDistance = float.MaxValue;
        float bestTrackIdx = 0f;

        for (int i = 0; i < TotalPoints - 1; i++)
        {
            Vector2 start = OriginalState[i];
            Vector2 end = OriginalState[i + 1];
            Vector2 segment = end - start;
            float segmentSqrLength = segment.sqrMagnitude;
            float t = 0f;

            if (segmentSqrLength > 0.0001f)
                t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / segmentSqrLength);

            Vector2 closest = start + segment * t;
            float sqrDistance = (point - closest).sqrMagnitude;
            if (sqrDistance < bestSqrDistance)
            {
                bestSqrDistance = sqrDistance;
                bestTrackIdx = i + t;
            }
        }

        return bestTrackIdx;
    }

    public Vector2Int GetGridPosFromTrackIndex(float trackIndex, ArrowDir direction)
    {
        Vector3 pos = GetPositionAtTrackIndex(trackIndex, direction);
        return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
    }

    public Vector2Int GetTailGridPosAtProgress(int gridsMoved, ArrowDir direction)
    {
        float shiftAtThatTime = gridsMoved * NodesPerUnit;
        float tailTrackIdx = -shiftAtThatTime + (TotalPoints - 1);
        Vector3 exactTailPos = GetPositionAtTrackIndex(tailTrackIdx, direction);
        return new Vector2Int(Mathf.RoundToInt(exactTailPos.x), Mathf.RoundToInt(exactTailPos.y));
    }

    public ArrowDir GetHeadDirectionAtDistance(float headDist, ArrowDir fallbackDirection)
    {
        if (ActiveWarps == null || ActiveWarps.Count == 0) return fallbackDirection;

        ArrowDir dirNow = fallbackDirection;
        for (int i = 0; i < ActiveWarps.Count; i++)
        {
            if (headDist + 0.0001f >= ActiveWarps[i].rawDistFromHead0)
                dirNow = ActiveWarps[i].exitDir;
            else
                break;
        }

        return dirNow;
    }

    public void CommitCurrentPoseAsOrigin()
    {
        if (CurrentPositions == null || TotalPoints <= 0) return;

        Vector3[] committedState = new Vector3[TotalPoints];
        System.Array.Copy(CurrentPositions, committedState, TotalPoints);
        OriginalState = committedState;

        if (CurrentPositions == null || CurrentPositions.Length != TotalPoints)
            CurrentPositions = new Vector3[TotalPoints];

        System.Array.Copy(OriginalState, CurrentPositions, TotalPoints);
        AccumulatedShift = 0f;
        ClearWarps();
        RebuildLogicNodesFromCurrentState();
    }

    public void ClearFromGrid(SnakeBlock self)
    {
        if (GridManager.Instance == null) return;

        GridManager.Instance.UnregisterSnakeCells(self, _occupiedCells);
        _occupiedCells.Clear();
    }

    public void UpdateGridOccupancy(SnakeBlock self, SnakeInteractions interactions)
    {
        if (GridManager.Instance == null || !IsInitialized) return;

        ClearFromGrid(self);

        float headIdx = -AccumulatedShift;
        for (int i = 0; i < TotalPoints; i++)
        {
            float trackIdx = headIdx + i;
            Vector2Int cell = GetGridPosFromTrackIndex(trackIdx, self.direction);
            _occupiedCells.Add(cell);
        }

        GridManager.Instance.RegisterSnakeCells(self, _occupiedCells);

        Vector2Int headCell = GetGridPosFromTrackIndex(headIdx, self.direction);
        if (interactions != null) interactions.TriggerCellInteractions(headCell);
        else SnakeInteractions.TriggerBoardCell(headCell);
    }

    public void ClearWarps()
    {
        ActiveWarps.Clear();
        LastPassedPortalIndex = -1;
        LastPassedDeflectorIndex = -1;
    }

    public void ResetDamageAndStopState()
    {
        IsStoppedByStopBlock = false;
        HoldingStopBlock = null;
        HasDealtDamage = false;
    }

    public static void Register(SnakeBlock snake)
    {
        if (snake != null && !ActiveSnakeBuffer.Contains(snake))
            ActiveSnakeBuffer.Add(snake);
    }

    public static void Unregister(SnakeBlock snake)
    {
        ActiveSnakeBuffer.Remove(snake);
    }

    private void BuildSampledState()
    {
        TotalPoints = 0;
        OriginalState = null;
        CurrentPositions = null;

        if (LogicNodes.Count > 1)
        {
            int segmentsCount = LogicNodes.Count - 1;
            int currentTotalPoints = 0;
            List<int> pointsPerSegment = new List<int>(segmentsCount);

            for (int i = 0; i < segmentsCount; i++)
            {
                float dist = Vector3.Distance(LogicNodes[i], LogicNodes[i + 1]);
                int pointsCount = Mathf.Max(1, Mathf.RoundToInt(dist * NodesPerUnit));
                pointsPerSegment.Add(pointsCount);
                currentTotalPoints += pointsCount;
            }

            TotalPoints = currentTotalPoints + 1;
            OriginalState = new Vector3[TotalPoints];
            CurrentPositions = new Vector3[TotalPoints];

            int arrayIndex = 0;
            for (int i = 0; i < segmentsCount; i++)
            {
                Vector3 start = LogicNodes[i];
                Vector3 end = LogicNodes[i + 1];
                int count = pointsPerSegment[i];
                for (int j = 0; j < count; j++)
                {
                    float t = (float)j / count;
                    OriginalState[arrayIndex] = Vector3.Lerp(start, end, t);
                    arrayIndex++;
                }
            }

            OriginalState[arrayIndex] = LogicNodes[segmentsCount];
            System.Array.Copy(OriginalState, CurrentPositions, TotalPoints);
        }
        else if (LogicNodes.Count == 1)
        {
            TotalPoints = 1;
            OriginalState = new[] { LogicNodes[0] };
            CurrentPositions = new[] { LogicNodes[0] };
        }
    }

    private Vector3 GetForwardPositionWithWarps(float distForward, ArrowDir direction, bool snapToPortalEntryForRender)
    {
        Vector3 pos = OriginalState[0];
        Vector3 dirVec = PathScanner.GetDirVector(direction);

        if (ActiveWarps == null || ActiveWarps.Count == 0)
            return pos + dirVec * distForward;

        float prevDist = 0f;
        float snapDist = 0f;
        if (snapToPortalEntryForRender)
            snapDist = Mathf.Clamp(Mathf.Max(1f / Mathf.Max(1, NodesPerUnit), 0.6f), 0.05f, 0.95f);

        for (int i = 0; i < ActiveWarps.Count; i++)
        {
            PathWarp warp = ActiveWarps[i];
            if (warp.rawDistFromHead0 > distForward + 0.0001f)
            {
                if (warp.isPortal && snapToPortalEntryForRender && (warp.rawDistFromHead0 - distForward) <= snapDist)
                    return new Vector3(warp.portalWorldPos.x, warp.portalWorldPos.y, pos.z);

                break;
            }

            float segmentDist = Mathf.Max(0f, warp.rawDistFromHead0 - prevDist);
            pos += dirVec * segmentDist;
            pos += warp.teleportOffset;
            dirVec = PathScanner.GetDirVector(warp.exitDir);
            prevDist = warp.rawDistFromHead0;
        }

        pos += dirVec * Mathf.Max(0f, distForward - prevDist);
        return pos;
    }

    private void RebuildLogicNodesFromCurrentState()
    {
        LogicNodes.Clear();
        if (OriginalState == null || OriginalState.Length == 0) return;

        Vector2Int lastCell = new Vector2Int(int.MinValue, int.MinValue);
        for (int i = 0; i < OriginalState.Length; i++)
        {
            Vector2Int cell = new Vector2Int(Mathf.RoundToInt(OriginalState[i].x), Mathf.RoundToInt(OriginalState[i].y));
            if (cell == lastCell) continue;

            LogicNodes.Add(new Vector3(cell.x, cell.y, 0f));
            lastCell = cell;
        }

        SimplifyLogicNodes();
    }

    private void SimplifyLogicNodes()
    {
        if (LogicNodes.Count < 3) return;

        int i = 1;
        while (i < LogicNodes.Count - 1)
        {
            Vector2Int prev = PathScanner.ToGridCell(LogicNodes[i - 1]);
            Vector2Int current = PathScanner.ToGridCell(LogicNodes[i]);
            Vector2Int next = PathScanner.ToGridCell(LogicNodes[i + 1]);

            Vector2Int prevStep = NormalizeGridStep(current - prev);
            Vector2Int nextStep = NormalizeGridStep(next - current);

            if (prevStep == nextStep && prevStep != Vector2Int.zero)
                LogicNodes.RemoveAt(i);
            else
                i++;
        }
    }

    private void ResetHit()
    {
        LastObstacleType = ObstacleHitType.None;
        LastObstacleCell = new Vector2Int(int.MinValue, int.MinValue);
    }

    private static Vector2Int NormalizeGridStep(Vector2Int step)
    {
        return new Vector2Int(Mathf.Clamp(step.x, -1, 1), Mathf.Clamp(step.y, -1, 1));
    }
}
