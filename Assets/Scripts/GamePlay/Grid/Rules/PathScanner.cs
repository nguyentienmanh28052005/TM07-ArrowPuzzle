using System.Collections.Generic;
using UnityEngine;

public static class PathScanner
{
    private const int DefaultScanLimit = 512;
    private const int DefaultBoundaryAbs = int.MaxValue;

    public static MoveResult Scan(
        BoardState board,
        SnakeBlock self,
        Vector2Int startCell,
        ArrowDir startDirection,
        int scanLimit,
        bool stopAtBlockers = true,
        int boundaryAbs = DefaultBoundaryAbs)
    {
        List<PathWarp> warps = new List<PathWarp>(8);
        if (board == null || !board.IsValid)
            return new MoveResult(float.MaxValue, ObstacleHit.None, warps);

        int safeLimit = Mathf.Max(1, scanLimit);
        Vector2Int currentCell = startCell;
        Vector2Int step = GetDirStep(startDirection);
        if (step == Vector2Int.zero)
            return new MoveResult(float.MaxValue, ObstacleHit.None, warps);

        HashSet<Vector3Int> visitedStates = new HashSet<Vector3Int>();

        for (int distance = 1; distance <= safeLimit; distance++)
        {
            Vector3Int state = new Vector3Int(currentCell.x, currentCell.y, GetStepKey(step));
            if (!visitedStates.Add(state))
                return new MoveResult(float.MaxValue, ObstacleHit.None, warps);

            Vector2Int checkCell = currentCell + step;
            if (IsOutsideBoundary(checkCell, boundaryAbs))
                return new MoveResult(float.MaxValue, ObstacleHit.None, warps);

            if (stopAtBlockers && TryGetBlockingHit(board, self, checkCell, distance, out MoveResult blockingResult, warps))
                return blockingResult;

            if (board.Occupancy.TryGetBlackHoleAt(checkCell, out GridBlackHole blackHole))
            {
                ArrowDir incomingDirection = GetArrowDirFromStep(step);
                if (blackHole.CanEnter(incomingDirection))
                {
                    return new MoveResult(
                        distance,
                        new ObstacleHit(ObstacleHitType.BlackHole, checkCell, blackHole: blackHole),
                        warps);
                }

                return new MoveResult(
                    distance - 1,
                    new ObstacleHit(ObstacleHitType.BlackHoleBlocked, checkCell, blackHole: blackHole),
                    warps);
            }

            if (board.Occupancy.TryGetPortalLink(checkCell, out GridManager.PortalLink link))
            {
                Vector3 offset = new Vector3(link.exit.x - checkCell.x, link.exit.y - checkCell.y, 0f);
                warps.Add(new PathWarp(
                    distance,
                    offset,
                    link.exitDir,
                    new Vector3(checkCell.x, checkCell.y, 0f),
                    new Vector3(link.exit.x, link.exit.y, 0f),
                    true,
                    null));

                currentCell = link.exit;
                step = GetDirStep(link.exitDir);
                continue;
            }

            if (board.Occupancy.TryGetDeflectorAt(checkCell, out GridDeflector deflector))
            {
                ArrowDir newDirection = deflector.direction;
                warps.Add(new PathWarp(
                    distance,
                    Vector3.zero,
                    newDirection,
                    new Vector3(checkCell.x, checkCell.y, 0f),
                    new Vector3(checkCell.x, checkCell.y, 0f),
                    false,
                    deflector));

                currentCell = checkCell;
                step = GetDirStep(newDirection);
                continue;
            }

            currentCell = checkCell;
        }

        return new MoveResult(float.MaxValue, ObstacleHit.None, warps);
    }

    public static void BuildGuidelineSegments(
        BoardState board,
        SnakeBlock self,
        Vector3 startWorld,
        ArrowDir startDirection,
        int maxSteps,
        bool stopAtBlockers,
        int maxSegments,
        List<PathSegment> output)
    {
        if (output == null) return;

        output.Clear();
        if (board == null || !board.IsValid || maxSteps <= 0) return;

        int safeMaxSteps = Mathf.Max(1, maxSteps);
        int safeMaxSegments = Mathf.Max(1, maxSegments);
        Vector3 currentWorldStart = startWorld;
        Vector2Int currentCell = ToGridCell(currentWorldStart);
        ArrowDir currentDirection = startDirection;
        Vector2Int step = GetDirStep(currentDirection);

        PathSegmentBuilder currentSegment = new PathSegmentBuilder(currentWorldStart, currentDirection, false);
        int portalHops = 0;

        for (int used = 0; used < safeMaxSteps; used++)
        {
            Vector2Int nextCell = currentCell + step;

            if (IsOutsideBoundary(nextCell, 100))
            {
                currentSegment.Steps += safeMaxSteps - used;
                break;
            }

            if (stopAtBlockers && IsBlockingGuidelineCell(board, self, nextCell))
                break;

            if (board.Occupancy.TryGetBlackHoleAt(nextCell, out GridBlackHole blackHole))
            {
                if (blackHole.CanEnter(currentDirection))
                    currentSegment.Steps += 1;
                break;
            }

            if (board.Occupancy.TryGetPortalLink(nextCell, out GridManager.PortalLink link))
            {
                currentSegment.Steps += 1;
                currentSegment.EndsInPortal = true;
                AddSegment(output, currentSegment, safeMaxSegments);

                currentCell = link.exit;
                currentWorldStart = new Vector3(link.exit.x, link.exit.y, startWorld.z);
                currentDirection = link.exitDir;
                step = GetDirStep(currentDirection);
                currentSegment = new PathSegmentBuilder(currentWorldStart, currentDirection, true);

                portalHops++;
                if (portalHops >= safeMaxSegments || output.Count >= safeMaxSegments)
                    break;

                continue;
            }

            if (board.Occupancy.TryGetDeflectorAt(nextCell, out GridDeflector deflector))
            {
                currentSegment.Steps += 1;
                AddSegment(output, currentSegment, safeMaxSegments);

                currentCell = nextCell;
                currentWorldStart = new Vector3(currentCell.x, currentCell.y, startWorld.z);
                currentDirection = deflector.direction;
                step = GetDirStep(currentDirection);
                currentSegment = new PathSegmentBuilder(currentWorldStart, currentDirection, false);

                if (output.Count >= safeMaxSegments)
                    break;

                continue;
            }

            currentSegment.Steps += 1;
            currentCell = nextCell;
        }

        AddSegment(output, currentSegment, safeMaxSegments);
    }

    public static Vector2Int GetDirStep(ArrowDir direction)
    {
        switch (direction)
        {
            case ArrowDir.Up: return new Vector2Int(0, 1);
            case ArrowDir.Down: return new Vector2Int(0, -1);
            case ArrowDir.Left: return new Vector2Int(-1, 0);
            case ArrowDir.Right: return new Vector2Int(1, 0);
            default: return Vector2Int.zero;
        }
    }

    public static ArrowDir GetArrowDirFromStep(Vector2Int step)
    {
        if (step.y > 0) return ArrowDir.Up;
        if (step.y < 0) return ArrowDir.Down;
        if (step.x < 0) return ArrowDir.Left;
        return ArrowDir.Right;
    }

    public static Vector3 GetDirVector(ArrowDir direction)
    {
        switch (direction)
        {
            case ArrowDir.Up: return Vector3.up;
            case ArrowDir.Down: return Vector3.down;
            case ArrowDir.Left: return Vector3.left;
            case ArrowDir.Right: return Vector3.right;
            default: return Vector3.zero;
        }
    }

    public static Vector2Int ToGridCell(Vector3 worldPosition)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.y));
    }

    private static bool TryGetBlockingHit(
        BoardState board,
        SnakeBlock self,
        Vector2Int cell,
        int distance,
        out MoveResult result,
        List<PathWarp> warps)
    {
        SnakeBlock obstacle = board.Occupancy.GetSnakeAt(cell);
        if (obstacle != null && obstacle != self)
        {
            result = CreateBlockedResult(distance, ObstacleHitType.Snake, cell, warps, snake: obstacle);
            return true;
        }

        if (board.Occupancy.HasGateAt(cell))
        {
            result = CreateBlockedResult(distance, ObstacleHitType.Gate, cell, warps);
            return true;
        }

        if (board.Occupancy.HasElectricWallAt(cell))
        {
            result = CreateBlockedResult(distance, ObstacleHitType.ElectricWall, cell, warps);
            return true;
        }

        if (board.Occupancy.HasCountdownBlockAt(cell))
        {
            result = CreateBlockedResult(distance, ObstacleHitType.CountdownBlock, cell, warps);
            return true;
        }

        if (board.Occupancy.TryGetStopBlockAt(cell, out GridStopBlock stopBlock))
        {
            result = CreateBlockedResult(distance, ObstacleHitType.StopBlock, cell, warps, stopBlock: stopBlock);
            return true;
        }

        if (board.Occupancy.HasArrowShadowAt(cell))
        {
            result = CreateBlockedResult(distance, ObstacleHitType.ArrowShadow, cell, warps);
            return true;
        }

        if (board.Occupancy.HasBlockingTurnStateBlockAt(cell))
        {
            result = CreateBlockedResult(distance, ObstacleHitType.TurnStateBlock, cell, warps);
            return true;
        }

        result = null;
        return false;
    }

    private static MoveResult CreateBlockedResult(
        int distance,
        ObstacleHitType type,
        Vector2Int cell,
        List<PathWarp> warps,
        SnakeBlock snake = null,
        GridStopBlock stopBlock = null)
    {
        return new MoveResult(
            distance - 1,
            new ObstacleHit(type, cell, snake, stopBlock),
            warps);
    }

    private static bool IsBlockingGuidelineCell(BoardState board, SnakeBlock self, Vector2Int cell)
    {
        SnakeBlock obstacle = board.Occupancy.GetSnakeAt(cell);
        if (obstacle != null && obstacle != self) return true;
        if (board.Occupancy.HasGateAt(cell)) return true;
        if (board.Occupancy.HasElectricWallAt(cell)) return true;
        if (board.Occupancy.HasCountdownBlockAt(cell)) return true;
        if (board.Occupancy.TryGetStopBlockAt(cell, out _)) return true;
        if (board.Occupancy.HasArrowShadowAt(cell)) return true;
        if (board.Occupancy.HasBlockingTurnStateBlockAt(cell)) return true;
        return false;
    }

    private static void AddSegment(List<PathSegment> output, PathSegmentBuilder segment, int maxSegments)
    {
        if (output == null || output.Count >= maxSegments || segment.Steps <= 0) return;
        output.Add(segment.ToSegment());
    }

    private static bool IsOutsideBoundary(Vector2Int cell, int boundaryAbs)
    {
        if (boundaryAbs == DefaultBoundaryAbs) return false;
        return Mathf.Abs(cell.x) > boundaryAbs || Mathf.Abs(cell.y) > boundaryAbs;
    }

    private static int GetStepKey(Vector2Int step)
    {
        if (step.y > 0) return 0;
        if (step.y < 0) return 1;
        if (step.x < 0) return 2;
        return 3;
    }

    private struct PathSegmentBuilder
    {
        public Vector3 StartWorld;
        public ArrowDir Direction;
        public int Steps;
        public bool StartsFromPortal;
        public bool EndsInPortal;

        public PathSegmentBuilder(Vector3 startWorld, ArrowDir direction, bool startsFromPortal)
        {
            StartWorld = startWorld;
            Direction = direction;
            Steps = 0;
            StartsFromPortal = startsFromPortal;
            EndsInPortal = false;
        }

        public PathSegment ToSegment()
        {
            return new PathSegment(StartWorld, Direction, Steps, StartsFromPortal, EndsInPortal);
        }
    }
}
