using System.Collections.Generic;
using UnityEngine;

public static class LevelGeneratorCore
{
    public struct Settings
    {
        public int width;
        public int height;
        public int targetArrowCount;
        public int minSnakeLength;
        public int maxSnakeLength;
        public int seed;
        public int maxAttemptsPerArrow;
        public int bodyAttemptsPerCandidate;
        public int turnChancePercent;
        public int minDistanceBetweenSnakes;
        public int minStraightCellsPerSegment;
        public int fillSearchAttempts;
        public int fillLayoutAttempts;
        public bool allowBentSnakes;
        public bool fillAvailableArea;
        public bool requireFullFill;
        public int originX;
        public int originY;
        public bool[] placementMask;
        public Color[] colorPalette;
    }

    public sealed class Result
    {
        public bool success;
        public int placedArrowCount;
        public int bentArrowCount;
        public int directionTypeCount;
        public int shapeTypeCount;
        public int directionMask;
        public int shapeMask;
        public int occupiedCellCount;
        public int placementAreaCellCount;
        public int straightShapeCount;
        public int lShapeCount;
        public int uShapeCount;
        public int zigzagShapeCount;
        public int randomBentShapeCount;
        public string message;
        public readonly List<SnakeSaveData> snakes = new List<SnakeSaveData>();
    }

    private struct Candidate
    {
        public int x;
        public int y;
        public int length;
        public ArrowDir dir;
    }

    private enum ShapeKind
    {
        Straight,
        L,
        U,
        Zigzag,
        RandomBent
    }

    private struct CellOffset
    {
        public int x;
        public int y;

        public CellOffset(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    private sealed class DfsBeamBuffers
    {
        public readonly int[] currentPaths;
        public readonly int[] nextPaths;
        public readonly int[] currentScores;
        public readonly int[] nextScores;
        public readonly int[] currentLastDirections;
        public readonly int[] nextLastDirections;
        public readonly int[] currentRunCells;
        public readonly int[] nextRunCells;
        public readonly int[] currentTurnCounts;
        public readonly int[] nextTurnCounts;

        public DfsBeamBuffers(int beamCapacity, int pathCapacity)
        {
            int pathBufferLength = Mathf.Max(1, beamCapacity * pathCapacity);
            currentPaths = new int[pathBufferLength];
            nextPaths = new int[pathBufferLength];
            currentScores = new int[beamCapacity];
            nextScores = new int[beamCapacity];
            currentLastDirections = new int[beamCapacity];
            nextLastDirections = new int[beamCapacity];
            currentRunCells = new int[beamCapacity];
            nextRunCells = new int[beamCapacity];
            currentTurnCounts = new int[beamCapacity];
            nextTurnCounts = new int[beamCapacity];
        }
    }

    private const int DfsBeamWidthCap = 10;

    private static readonly Dictionary<int, CellOffset[]> SpacingOffsetCache = new Dictionary<int, CellOffset[]>();

    private static readonly Color[] DefaultPalette =
    {
        new Color(1f, 0f, 0f, 1f),
        new Color(1f, 0.69f, 0f, 1f),
        new Color(0.9f, 1f, 0f, 1f),
        new Color(0f, 1f, 0.31f, 1f),
        new Color(0f, 1f, 0.68f, 1f),
        new Color(0f, 0.77f, 1f, 1f),
        new Color(0f, 0.24f, 1f, 1f),
        new Color(0.81f, 0f, 1f, 1f),
        new Color(1f, 0f, 0.65f, 1f)
    };

    public static Result Generate(Settings settings)
    {
        NormalizeSettings(ref settings);
        int placementAreaCellCount = GetPlacementAreaCellCount(settings);
        if (placementAreaCellCount <= 0)
        {
            Result emptyResult = new Result();
            emptyResult.placementAreaCellCount = 0;
            emptyResult.success = false;
            emptyResult.message = "No placement cells are enabled for the generator.";
            return emptyResult;
        }

        if (settings.fillAvailableArea)
        {
            return GenerateBestFillResult(settings);
        }

        Result result = new Result();
        result.placementAreaCellCount = placementAreaCellCount;

        byte[] occupied = new byte[settings.width * settings.height];
        bool[] horizontalLineLanes = new bool[settings.height];
        bool[] verticalLineLanes = new bool[settings.width];
        int[] candidateCells = new int[settings.maxSnakeLength];
        int[] pathVisited = new int[settings.width * settings.height];
        int pathVisitGeneration = 1;
        System.Random random = new System.Random(settings.seed);

        int failedAttempts = 0;
        for (int i = 0; i < settings.targetArrowCount; i++)
        {
            Candidate candidate;
            int[] selectedCells = candidateCells;
            if (!TryFindRandomCandidate(settings, occupied, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, random, candidateCells, out candidate))
            {
                if (!TryFindAnyCandidate(settings, occupied, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, random, candidateCells, out candidate))
                {
                    failedAttempts++;
                    break;
                }
            }

            SnakeSaveData snake = BuildSnake(candidate, settings, random, selectedCells);
            MarkSnake(occupied, selectedCells, candidate.length);
            MarkParallelLineLanes(settings, selectedCells, candidate.length, horizontalLineLanes, verticalLineLanes);
            result.snakes.Add(snake);
            result.placedArrowCount++;
            bool hasBentPath = HasBentPath(settings.width, selectedCells, candidate.length);
            if (hasBentPath)
            {
                result.bentArrowCount++;
            }
            UpdateResultDiversity(result, snake.direction, hasBentPath ? ShapeKind.RandomBent : ShapeKind.Straight);
            result.occupiedCellCount += candidate.length;
        }

        Reverse(result.snakes);
        result.success = result.placedArrowCount >= settings.targetArrowCount;
        result.message = result.success
            ? "Generated a fully populated solvable level."
            : "Stopped early because no valid reverse placement remained. Increase grid size, reduce count, or reduce snake length.";

        if (failedAttempts > 0 && result.placedArrowCount == 0)
        {
            result.message = "No valid candidate could be placed with the current settings.";
        }

        return result;
    }

    private static void NormalizeSettings(ref Settings settings)
    {
        settings.width = Mathf.Max(1, settings.width);
        settings.height = Mathf.Max(1, settings.height);
        settings.targetArrowCount = Mathf.Max(1, settings.targetArrowCount);
        settings.minSnakeLength = Mathf.Max(1, settings.minSnakeLength);
        settings.maxSnakeLength = Mathf.Max(settings.minSnakeLength, settings.maxSnakeLength);
        settings.maxAttemptsPerArrow = Mathf.Max(32, settings.maxAttemptsPerArrow);
        settings.bodyAttemptsPerCandidate = Mathf.Clamp(settings.bodyAttemptsPerCandidate, 1, 64);
        settings.turnChancePercent = Mathf.Clamp(settings.turnChancePercent, 0, 100);
        settings.minDistanceBetweenSnakes = Mathf.Max(1, settings.minDistanceBetweenSnakes);
        settings.minStraightCellsPerSegment = MakeOddAtLeast(settings.minStraightCellsPerSegment, 3);
        settings.fillSearchAttempts = Mathf.Max(settings.maxAttemptsPerArrow, settings.fillSearchAttempts);
        settings.fillLayoutAttempts = Mathf.Clamp(settings.fillLayoutAttempts, 1, 64);
        if (settings.placementMask != null && settings.placementMask.Length != settings.width * settings.height)
        {
            settings.placementMask = null;
        }

        if (settings.fillAvailableArea)
        {
            settings.fillSearchAttempts = Mathf.Clamp(settings.fillSearchAttempts, 64, 1024);
            settings.fillLayoutAttempts = Mathf.Clamp(settings.fillLayoutAttempts, 1, 12);
            settings.bodyAttemptsPerCandidate = Mathf.Clamp(settings.bodyAttemptsPerCandidate, 1, 8);
        }

        int maxCells = Mathf.Max(1, GetPlacementAreaCellCount(settings));
        settings.maxSnakeLength = Mathf.Min(settings.maxSnakeLength, maxCells);
        settings.minSnakeLength = Mathf.Min(settings.minSnakeLength, settings.maxSnakeLength);

        if (settings.fillAvailableArea)
        {
            int minimumFillLength = maxCells < 3 ? 1 : 3;
            settings.minSnakeLength = Mathf.Max(minimumFillLength, settings.minSnakeLength);
            settings.maxSnakeLength = Mathf.Max(settings.minSnakeLength, settings.maxSnakeLength);
            settings.maxSnakeLength = Mathf.Min(settings.maxSnakeLength, maxCells);

            if (maxCells >= 3)
            {
                int firstOddFillLength = GetFirstOddAtLeast(settings.minSnakeLength);
                settings.minSnakeLength = firstOddFillLength > settings.maxSnakeLength
                    ? GetLastOddAtMost(settings.maxSnakeLength)
                    : firstOddFillLength;
                settings.maxSnakeLength = GetLastOddAtMost(settings.maxSnakeLength);
                settings.minSnakeLength = Mathf.Max(3, settings.minSnakeLength);
                settings.maxSnakeLength = Mathf.Max(settings.minSnakeLength, settings.maxSnakeLength);
            }
            else
            {
                settings.minSnakeLength = Mathf.Min(settings.minSnakeLength, maxCells);
            }

            return;
        }

        int firstOddLength = GetFirstOddAtLeast(settings.minSnakeLength);
        if (firstOddLength > settings.maxSnakeLength)
        {
            settings.minSnakeLength = GetLastOddAtMost(settings.maxSnakeLength);
        }
        else
        {
            settings.minSnakeLength = firstOddLength;
        }

        settings.maxSnakeLength = GetLastOddAtMost(settings.maxSnakeLength);
        settings.minSnakeLength = Mathf.Max(1, settings.minSnakeLength);
        settings.maxSnakeLength = Mathf.Max(settings.minSnakeLength, settings.maxSnakeLength);
    }

    private static Result GenerateBestFillResult(Settings settings)
    {
        Result bestResult = null;
        Result bestBentResult = null;

        for (int attempt = 0; attempt < settings.fillLayoutAttempts; attempt++)
        {
            int seed = settings.seed + attempt * 7919;
            Settings attemptSettings = settings;
            attemptSettings.seed = seed;

            if (attempt == 0 && ShouldUseDfsFill(settings))
            {
                Result dfsResult = GenerateDfsFillResult(attemptSettings, seed);
                ConsiderGeneratedFillResult(settings, ref bestResult, ref bestBentResult, dfsResult);
            }

            Result mixedResult = GenerateMixedTemplateFillResult(attemptSettings, seed + 104729);
            ConsiderGeneratedFillResult(settings, ref bestResult, ref bestBentResult, mixedResult);

            if (attempt < 2)
            {
                Result singleResult = GenerateSingleFillResult(attemptSettings, seed + 262147);
                ConsiderGeneratedFillResult(settings, ref bestResult, ref bestBentResult, singleResult);
            }

            if (bestResult != null
                && bestResult.occupiedCellCount >= GetPlacementAreaCellCount(settings)
                && bestResult.bentArrowCount > 0)
            {
                break;
            }
        }

        Result laneFillResult = GenerateBestStripedFillResult(settings);
        ConsiderGeneratedFillResult(settings, ref bestResult, ref bestBentResult, laneFillResult);

        if (bestResult != null
            && bestResult.bentArrowCount <= 0
            && bestBentResult != null
            && IsBentFillAcceptable(bestBentResult, bestResult, settings))
        {
            bestResult = bestBentResult;
        }
        else if (bestResult != null
            && bestBentResult != null
            && bestBentResult != bestResult
            && IsBentFillAcceptable(bestBentResult, bestResult, settings)
            && GetShapeDiversityScore(bestBentResult) > GetShapeDiversityScore(bestResult))
        {
            bestResult = bestBentResult;
        }

        bestResult = FillSolvableRemainder(settings, bestResult);
        bestResult = RepairSparseGaps(settings, bestResult);
        bestResult = ExtendSnakeEndsIntoRemainder(settings, bestResult);

        if (settings.requireFullFill)
        {
            bestResult = CompleteRequiredFullFill(settings, bestResult);
        }

        if (bestResult == null)
        {
            bestResult = new Result();
            bestResult.placementAreaCellCount = GetPlacementAreaCellCount(settings);
        }

        bool isFull = bestResult.occupiedCellCount >= bestResult.placementAreaCellCount;
        bestResult.success = bestResult.placedArrowCount > 0 && (!settings.requireFullFill || isFull);
        if (bestResult.success)
        {
            bestResult.message = settings.requireFullFill
                ? "Generated a full valid fill for the selected area."
                : "Generated the densest valid fill found for the selected area.";
        }
        else
        {
            bestResult.message = settings.requireFullFill
                ? "Could not fully fill the selected area with the current rules. Relax spacing/segment rules or redraw the mask."
                : "No valid candidate could be placed with the current fill settings.";
        }

        return bestResult;
    }

    private static void ConsiderGeneratedFillResult(Settings settings, ref Result bestResult, ref Result bestBentResult, Result current)
    {
        current = EnsureSolvableResult(settings, current);
        ConsiderFillResult(ref bestResult, current);
        ConsiderBentFillResult(ref bestBentResult, current);
    }

    private static Result EnsureSolvableResult(Settings settings, Result result)
    {
        if (result == null || result.snakes.Count == 0)
        {
            return result;
        }

        return TryOrderSnakesForSolvability(settings, result.snakes) ? result : null;
    }

    private static bool TryOrderSnakesForSolvability(Settings settings, List<SnakeSaveData> snakes)
    {
        int snakeCount = snakes.Count;
        int[] occupied = new int[settings.width * settings.height];

        for (int i = 0; i < snakeCount; i++)
        {
            SnakeSaveData snake = snakes[i];
            if (snake.segmentPositions == null || snake.segmentPositions.Count == 0)
            {
                return false;
            }

            for (int j = 0; j < snake.segmentPositions.Count; j++)
            {
                int index;
                if (!TryGetLocalCellIndex(settings, snake.segmentPositions[j], out index))
                {
                    return false;
                }

                if (occupied[index] != 0)
                {
                    return false;
                }

                occupied[index] = 1;
            }
        }

        bool[] remaining = new bool[snakeCount];
        for (int i = 0; i < remaining.Length; i++)
        {
            remaining[i] = true;
        }

        List<SnakeSaveData> orderedSnakes = new List<SnakeSaveData>(snakeCount);
        int remainingCount = snakeCount;
        while (remainingCount > 0)
        {
            bool foundMovableSnake = false;

            for (int i = 0; i < snakeCount; i++)
            {
                if (!remaining[i])
                {
                    continue;
                }

                SnakeSaveData snake = snakes[i];
                RemoveSnakeCells(settings, occupied, snake);
                if (IsSnakeExitRayClear(settings, occupied, snake))
                {
                    remaining[i] = false;
                    orderedSnakes.Add(snake);
                    remainingCount--;
                    foundMovableSnake = true;
                    break;
                }

                AddSnakeCells(settings, occupied, snake);
            }

            if (!foundMovableSnake)
            {
                return false;
            }
        }

        snakes.Clear();
        snakes.AddRange(orderedSnakes);
        return true;
    }

    private static void RemoveSnakeCells(Settings settings, int[] occupied, SnakeSaveData snake)
    {
        for (int i = 0; i < snake.segmentPositions.Count; i++)
        {
            int index;
            if (TryGetLocalCellIndex(settings, snake.segmentPositions[i], out index))
            {
                occupied[index]--;
            }
        }
    }

    private static void AddSnakeCells(Settings settings, int[] occupied, SnakeSaveData snake)
    {
        for (int i = 0; i < snake.segmentPositions.Count; i++)
        {
            int index;
            if (TryGetLocalCellIndex(settings, snake.segmentPositions[i], out index))
            {
                occupied[index]++;
            }
        }
    }

    private static bool IsSnakeExitRayClear(Settings settings, int[] occupied, SnakeSaveData snake)
    {
        Vector2Int head = snake.segmentPositions[0];
        int x = head.x - settings.originX;
        int y = head.y - settings.originY;

        int dx;
        int dy;
        GetStep(snake.direction, out dx, out dy);
        x += dx;
        y += dy;

        while (IsInside(settings.width, settings.height, x, y))
        {
            if (occupied[ToIndex(settings.width, x, y)] > 0)
            {
                return false;
            }

            x += dx;
            y += dy;
        }

        return true;
    }

    private static bool TryGetLocalCellIndex(Settings settings, Vector2Int position, out int index)
    {
        int x = position.x - settings.originX;
        int y = position.y - settings.originY;
        if (!IsInside(settings.width, settings.height, x, y))
        {
            index = -1;
            return false;
        }

        index = ToIndex(settings.width, x, y);
        return true;
    }

    private static Result FillSolvableRemainder(Settings settings, Result result)
    {
        if (result == null || result.snakes.Count == 0)
        {
            return result;
        }

        int totalCells = settings.width * settings.height;
        int placementArea = GetPlacementAreaCellCount(settings);
        if (result.occupiedCellCount >= placementArea)
        {
            return result;
        }

        byte[] occupied = new byte[totalCells];
        bool[] horizontalLineLanes = new bool[settings.height];
        bool[] verticalLineLanes = new bool[settings.width];
        int[] snakeCells = new int[totalCells];
        if (!BuildFillStateFromResult(settings, result, occupied, horizontalLineLanes, verticalLineLanes, snakeCells))
        {
            return result;
        }

        int[] candidateCells = new int[settings.maxSnakeLength];
        int[] bestCells = new int[settings.maxSnakeLength];
        int[] pathVisited = new int[totalCells];
        int pathVisitGeneration = 1;
        int[] freeCellBuffer = new int[totalCells];
        System.Random random = new System.Random(settings.seed + 730201 + result.occupiedCellCount * 31 + result.placedArrowCount * 131);

        int maxAdditions = totalCells;
        int failedSweeps = 0;
        while (result.occupiedCellCount < placementArea && maxAdditions > 0 && failedSweeps < 4)
        {
            maxAdditions--;

            Candidate candidate;
            int insertionIndex;
            if (!TryFindBestDeferredFillCandidate(settings, result, occupied, result.occupiedCellCount, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, freeCellBuffer, random, candidateCells, bestCells, out candidate, out insertionIndex))
            {
                failedSweeps++;
                continue;
            }

            ApplyDeferredFillCandidate(settings, result, occupied, horizontalLineLanes, verticalLineLanes, random, bestCells, candidate, insertionIndex);
            failedSweeps = 0;
        }

        int microSweeps = 0;
        while (result.occupiedCellCount < placementArea && microSweeps < 4)
        {
            microSweeps++;

            Candidate candidate;
            int insertionIndex;
            if (!TryFindSmallestDeferredFillCandidate(settings, result, occupied, result.occupiedCellCount, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, freeCellBuffer, random, candidateCells, bestCells, out candidate, out insertionIndex))
            {
                break;
            }

            ApplyDeferredFillCandidate(settings, result, occupied, horizontalLineLanes, verticalLineLanes, random, bestCells, candidate, insertionIndex);
            microSweeps = 0;
        }

        return result;
    }

    private static Result RepairSparseGaps(Settings settings, Result result)
    {
        if (result == null || result.snakes.Count == 0)
        {
            return result;
        }

        int totalCells = settings.width * settings.height;
        int placementArea = GetPlacementAreaCellCount(settings);
        if (result.occupiedCellCount >= placementArea)
        {
            return result;
        }

        int[] emptyCells = new int[totalCells];
        System.Random random = new System.Random(settings.seed + 99173 + result.occupiedCellCount * 17);
        int repairAttempts = Mathf.Clamp(totalCells / 12, 4, 18);

        for (int attempt = 0; attempt < repairAttempts && result.occupiedCellCount < placementArea; attempt++)
        {
            int emptyCount = CollectEmptyCells(settings, result, emptyCells);
            if (emptyCount <= 0)
            {
                break;
            }

            int targetCell = emptyCells[random.Next(0, emptyCount)];
            Result candidate = CloneResult(result);
            int removeBudget = Mathf.Clamp(2 + attempt % 4, 2, 5);
            int removedCount = RemoveSnakesNearCell(settings, candidate, targetCell, removeBudget);
            if (removedCount <= 0)
            {
                continue;
            }

            RebuildResultStats(settings, candidate);
            candidate = FillSolvableRemainder(settings, candidate);
            candidate = EnsureSolvableResult(settings, candidate);
            if (candidate != null && candidate.occupiedCellCount > result.occupiedCellCount)
            {
                result = candidate;
            }
        }

        return result;
    }

    private static Result CompleteRequiredFullFill(Settings settings, Result result)
    {
        if (result == null || result.snakes.Count == 0)
        {
            return result;
        }

        int placementArea = GetPlacementAreaCellCount(settings);
        int stagnantPasses = 0;
        int maxPasses = GetFullFillCompletionPassLimit(settings);
        for (int pass = 0; pass < maxPasses && result.occupiedCellCount < placementArea; pass++)
        {
            int before = result.occupiedCellCount;

            result = ExtendSnakeEndsIntoRemainder(settings, result, GetEndpointExtensionPassLimit(settings, true), true);
            result = FillSolvableRemainder(settings, result);
            result = RepairSparseGaps(settings, result);
            result = ExtendSnakeEndsIntoRemainder(settings, result, GetEndpointExtensionPassLimit(settings, true), true);

            Result solvable = EnsureSolvableResult(settings, result);
            if (solvable != null)
            {
                result = solvable;
            }

            if (result.occupiedCellCount >= placementArea)
            {
                break;
            }

            if (result.occupiedCellCount <= before)
            {
                stagnantPasses++;
                if (stagnantPasses >= 2)
                {
                    break;
                }
            }
            else
            {
                stagnantPasses = 0;
            }
        }

        return result;
    }

    private static int GetFullFillCompletionPassLimit(Settings settings)
    {
        int area = GetPlacementAreaCellCount(settings);
        if (area <= 180)
        {
            return 5;
        }

        if (area <= 720)
        {
            return 4;
        }

        return 3;
    }

    private static Result ExtendSnakeEndsIntoRemainder(Settings settings, Result result)
    {
        return ExtendSnakeEndsIntoRemainder(settings, result, GetEndpointExtensionPassLimit(settings, false), false);
    }

    private static Result ExtendSnakeEndsIntoRemainder(Settings settings, Result result, int passLimit, bool allowExceedMaxSnakeLength)
    {
        if (result == null || result.snakes.Count == 0 || passLimit <= 0)
        {
            return result;
        }

        int totalCells = settings.width * settings.height;
        int placementArea = GetPlacementAreaCellCount(settings);
        if (result.occupiedCellCount >= placementArea)
        {
            return result;
        }

        byte[] occupied = new byte[totalCells];
        bool[] horizontalLineLanes = new bool[settings.height];
        bool[] verticalLineLanes = new bool[settings.width];
        int[] snakeCells = new int[Mathf.Max(totalCells, settings.maxSnakeLength)];
        int[] extensionCells = new int[Mathf.Max(1, allowExceedMaxSnakeLength ? totalCells : settings.maxSnakeLength)];
        System.Random random = new System.Random(settings.seed + 450001 + result.occupiedCellCount * 43 + result.placedArrowCount * 997);

        for (int pass = 0; pass < passLimit && result.occupiedCellCount < placementArea; pass++)
        {
            Result extendedResult;
            if (!TryApplyBestEndpointExtension(settings, result, occupied, horizontalLineLanes, verticalLineLanes, snakeCells, extensionCells, random, allowExceedMaxSnakeLength, out extendedResult))
            {
                break;
            }

            result = extendedResult;
        }

        return result;
    }

    private static int GetEndpointExtensionPassLimit(Settings settings, bool fullFill)
    {
        int area = GetPlacementAreaCellCount(settings);
        if (fullFill)
        {
            return Mathf.Clamp(area / 6, 12, 192);
        }

        return Mathf.Clamp(area / 18, 4, 64);
    }

    private static bool TryApplyBestEndpointExtension(Settings settings, Result result, byte[] occupied, bool[] horizontalLineLanes, bool[] verticalLineLanes, int[] snakeCells, int[] extensionCells, System.Random random, bool allowExceedMaxSnakeLength, out Result extendedResult)
    {
        extendedResult = null;

        System.Array.Clear(occupied, 0, occupied.Length);
        System.Array.Clear(horizontalLineLanes, 0, horizontalLineLanes.Length);
        System.Array.Clear(verticalLineLanes, 0, verticalLineLanes.Length);
        if (!BuildFillStateFromResult(settings, result, occupied, horizontalLineLanes, verticalLineLanes, snakeCells))
        {
            return false;
        }

        int snakeCount = result.snakes.Count;
        if (snakeCount <= 0)
        {
            return false;
        }

        int startSnake = random.Next(0, snakeCount);
        int maxChecks = Mathf.Clamp(snakeCount * 8 * Mathf.Min(extensionCells.Length, 17), 128, allowExceedMaxSnakeLength ? 16384 : 4096);
        int checkedCandidates = 0;
        int bestScore = int.MinValue;

        for (int snakePass = 0; snakePass < snakeCount && checkedCandidates < maxChecks; snakePass++)
        {
            int snakeIndex = (startSnake + snakePass) % snakeCount;
            int endpointOffset = random.Next(0, 2);
            for (int endpointPass = 0; endpointPass < 2 && checkedCandidates < maxChecks; endpointPass++)
            {
                bool fromHead = ((endpointPass + endpointOffset) & 1) == 0;
                int dirOffset = random.Next(0, 4);
                for (int dirPass = 0; dirPass < 4 && checkedCandidates < maxChecks; dirPass++)
                {
                    int dirIndex = (dirOffset + dirPass) & 3;
                    ArrowDir newHeadDirection = fromHead ? (ArrowDir)dirIndex : result.snakes[snakeIndex].direction;
                    int maxExtension = CollectEndpointExtensionCells(settings, result.snakes[snakeIndex], occupied, fromHead, dirIndex, extensionCells, allowExceedMaxSnakeLength);
                    if (maxExtension <= 0)
                    {
                        continue;
                    }

                    for (int length = maxExtension; length >= 1 && checkedCandidates < maxChecks; length--)
                    {
                        checkedCandidates++;
                        Result candidate = CloneResult(result);
                        if (!ApplyEndpointExtension(settings, candidate.snakes[snakeIndex], fromHead, newHeadDirection, extensionCells, length, allowExceedMaxSnakeLength))
                        {
                            continue;
                        }

                        if (!HasValidSnakeStraightRuns(settings, candidate.snakes[snakeIndex]))
                        {
                            continue;
                        }

                        if (!TryCopySnakeCells(settings, candidate.snakes[snakeIndex], snakeCells)
                            || !HasValidParallelLineLaneParity(settings, snakeCells, candidate.snakes[snakeIndex].segmentPositions.Count, horizontalLineLanes, verticalLineLanes))
                        {
                            continue;
                        }

                        RebuildResultStats(settings, candidate);
                        if (candidate.occupiedCellCount <= result.occupiedCellCount)
                        {
                            continue;
                        }

                        candidate = EnsureSolvableResult(settings, candidate);
                        if (candidate == null)
                        {
                            continue;
                        }

                        int score = length * 10000
                            + GetEndpointExtensionContactScore(settings, occupied, extensionCells, length) * 120
                            + random.Next(0, 500);
                        if (extendedResult == null || score > bestScore)
                        {
                            extendedResult = candidate;
                            bestScore = score;
                        }

                        if (length >= maxExtension && length >= settings.minStraightCellsPerSegment)
                        {
                            break;
                        }
                    }
                }
            }
        }

        return extendedResult != null;
    }

    private static int CollectEndpointExtensionCells(Settings settings, SnakeSaveData snake, byte[] occupied, bool fromHead, int dirIndex, int[] extensionCells, bool allowExceedMaxSnakeLength)
    {
        if (snake == null || snake.segmentPositions == null || snake.segmentPositions.Count == 0)
        {
            return 0;
        }

        int dx;
        int dy;
        int startCell;
        if (!TryGetEndpointExtensionStart(settings, snake, fromHead, out startCell))
        {
            return 0;
        }

        GetStep(dirIndex, out dx, out dy);

        int maxExtension = allowExceedMaxSnakeLength
            ? extensionCells.Length
            : Mathf.Min(extensionCells.Length, settings.maxSnakeLength - snake.segmentPositions.Count);
        if (maxExtension <= 0)
        {
            return 0;
        }

        int x = startCell % settings.width;
        int y = startCell / settings.width;
        int count = 0;
        while (count < maxExtension)
        {
            x += dx;
            y += dy;
            if (!IsPlacementCell(settings, x, y))
            {
                break;
            }

            int cell = ToIndex(settings.width, x, y);
            if (occupied[cell] != 0)
            {
                break;
            }

            int directConnectionCell = count == 0 ? startCell : extensionCells[count - 1];
            if (!CanUseEndpointExtensionCell(settings, occupied, extensionCells, count, directConnectionCell, cell))
            {
                break;
            }

            extensionCells[count] = cell;
            count++;
        }

        return count;
    }

    private static bool TryGetEndpointExtensionStart(Settings settings, SnakeSaveData snake, bool fromHead, out int startCell)
    {
        startCell = 0;
        if (fromHead)
        {
            return TryGetLocalCellIndex(settings, snake.segmentPositions[0], out startCell);
        }

        int count = snake.segmentPositions.Count;
        if (count <= 0)
        {
            return false;
        }

        return TryGetLocalCellIndex(settings, snake.segmentPositions[count - 1], out startCell);
    }

    private static bool CanUseEndpointExtensionCell(Settings settings, byte[] occupied, int[] extensionCells, int usedExtensionCount, int directConnectionCell, int cell)
    {
        int x = cell % settings.width;
        int y = cell / settings.width;
        int exclusiveDistance = settings.minDistanceBetweenSnakes;
        if (exclusiveDistance > 1)
        {
            CellOffset[] offsets = GetSpacingOffsets(exclusiveDistance);
            for (int i = 0; i < offsets.Length; i++)
            {
                int checkX = x + offsets[i].x;
                int checkY = y + offsets[i].y;
                if (!IsInside(settings.width, settings.height, checkX, checkY))
                {
                    continue;
                }

                int checkCell = ToIndex(settings.width, checkX, checkY);
                if (occupied[checkCell] == 0)
                {
                    continue;
                }

                if (checkCell == directConnectionCell && GetManhattanDistance(settings.width, checkCell, cell) == 1)
                {
                    continue;
                }

                return false;
            }

            for (int i = 0; i < usedExtensionCount - 1; i++)
            {
                if (GetManhattanDistance(settings.width, extensionCells[i], cell) < exclusiveDistance)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool ApplyEndpointExtension(Settings settings, SnakeSaveData snake, bool fromHead, ArrowDir newHeadDirection, int[] extensionCells, int length, bool allowExceedMaxSnakeLength)
    {
        if (snake == null || snake.segmentPositions == null || length <= 0)
        {
            return false;
        }

        if (!allowExceedMaxSnakeLength && snake.segmentPositions.Count + length > settings.maxSnakeLength)
        {
            return false;
        }

        if (fromHead)
        {
            for (int i = 0; i < length; i++)
            {
                snake.segmentPositions.Insert(0, ToWorldPosition(settings, extensionCells[i]));
            }

            snake.direction = newHeadDirection;
        }
        else
        {
            for (int i = 0; i < length; i++)
            {
                snake.segmentPositions.Add(ToWorldPosition(settings, extensionCells[i]));
            }
        }

        return true;
    }

    private static bool TryCopySnakeCells(Settings settings, SnakeSaveData snake, int[] cells)
    {
        if (snake == null || snake.segmentPositions == null || snake.segmentPositions.Count <= 0 || snake.segmentPositions.Count > cells.Length)
        {
            return false;
        }

        for (int i = 0; i < snake.segmentPositions.Count; i++)
        {
            if (!TryGetLocalCellIndex(settings, snake.segmentPositions[i], out cells[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static Vector2Int ToWorldPosition(Settings settings, int cell)
    {
        return new Vector2Int(settings.originX + cell % settings.width, settings.originY + cell / settings.width);
    }

    private static bool HasValidSnakeStraightRuns(Settings settings, SnakeSaveData snake)
    {
        if (snake == null || snake.segmentPositions == null || snake.segmentPositions.Count < 2)
        {
            return true;
        }

        int prevDx;
        int prevDy;
        if (!TryGetSnakeStep(settings, snake, 0, 1, out prevDx, out prevDy)
            || Mathf.Abs(prevDx) + Mathf.Abs(prevDy) != 1)
        {
            return false;
        }

        int runCells = 2;
        bool hasTurn = false;
        for (int i = 2; i < snake.segmentPositions.Count; i++)
        {
            int dx;
            int dy;
            if (!TryGetSnakeStep(settings, snake, i - 1, i, out dx, out dy)
                || Mathf.Abs(dx) + Mathf.Abs(dy) != 1)
            {
                return false;
            }

            if (dx == prevDx && dy == prevDy)
            {
                runCells++;
                continue;
            }

            if (!settings.allowBentSnakes)
            {
                return false;
            }

            hasTurn = true;
            if (!IsValidStraightRun(settings, runCells))
            {
                return false;
            }

            prevDx = dx;
            prevDy = dy;
            runCells = 2;
        }

        return !hasTurn
            ? IsOdd(runCells)
            : IsValidStraightRun(settings, runCells);
    }

    private static int GetEndpointExtensionContactScore(Settings settings, byte[] occupied, int[] extensionCells, int length)
    {
        int score = 0;
        for (int i = 0; i < length; i++)
        {
            int cell = extensionCells[i];
            score += CountOccupiedAtExactSpacing(settings, occupied, cell % settings.width, cell / settings.width);
        }

        return score;
    }

    private static int CollectEmptyCells(Settings settings, Result result, int[] emptyCells)
    {
        bool[] occupied = new bool[settings.width * settings.height];
        for (int i = 0; i < result.snakes.Count; i++)
        {
            SnakeSaveData snake = result.snakes[i];
            if (snake.segmentPositions == null)
            {
                continue;
            }

            for (int j = 0; j < snake.segmentPositions.Count; j++)
            {
                int index;
                if (TryGetLocalCellIndex(settings, snake.segmentPositions[j], out index))
                {
                    occupied[index] = true;
                }
            }
        }

        int count = 0;
        for (int i = 0; i < occupied.Length; i++)
        {
            if (!occupied[i] && IsPlacementCell(settings, i))
            {
                emptyCells[count] = i;
                count++;
            }
        }

        return count;
    }

    private static Result CloneResult(Result result)
    {
        Result clone = new Result();
        clone.success = result.success;
        clone.placedArrowCount = result.placedArrowCount;
        clone.bentArrowCount = result.bentArrowCount;
        clone.directionTypeCount = result.directionTypeCount;
        clone.shapeTypeCount = result.shapeTypeCount;
        clone.directionMask = result.directionMask;
        clone.shapeMask = result.shapeMask;
        clone.occupiedCellCount = result.occupiedCellCount;
        clone.placementAreaCellCount = result.placementAreaCellCount;
        clone.straightShapeCount = result.straightShapeCount;
        clone.lShapeCount = result.lShapeCount;
        clone.uShapeCount = result.uShapeCount;
        clone.zigzagShapeCount = result.zigzagShapeCount;
        clone.randomBentShapeCount = result.randomBentShapeCount;
        clone.message = result.message;

        for (int i = 0; i < result.snakes.Count; i++)
        {
            clone.snakes.Add(CloneSnake(result.snakes[i]));
        }

        return clone;
    }

    private static SnakeSaveData CloneSnake(SnakeSaveData snake)
    {
        SnakeSaveData clone = new SnakeSaveData();
        clone.direction = snake.direction;
        clone.arrowColor = snake.arrowColor;
        clone.hasArrowShadow = snake.hasArrowShadow;
        clone.segmentPositions = snake.segmentPositions != null
            ? new List<Vector2Int>(snake.segmentPositions)
            : new List<Vector2Int>();
        return clone;
    }

    private static int RemoveSnakesNearCell(Settings settings, Result result, int targetCell, int removeBudget)
    {
        int removedCount = 0;
        for (int removePass = 0; removePass < removeBudget && result.snakes.Count > 0; removePass++)
        {
            int bestIndex = -1;
            int bestDistance = int.MaxValue;
            for (int i = 0; i < result.snakes.Count; i++)
            {
                int distance = GetSnakeMinDistanceToCell(settings, result.snakes[i], targetCell);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0 || bestDistance > settings.maxSnakeLength + settings.minDistanceBetweenSnakes + removePass)
            {
                break;
            }

            result.snakes.RemoveAt(bestIndex);
            removedCount++;
        }

        return removedCount;
    }

    private static int GetSnakeMinDistanceToCell(Settings settings, SnakeSaveData snake, int targetCell)
    {
        if (snake.segmentPositions == null || snake.segmentPositions.Count == 0)
        {
            return int.MaxValue;
        }

        int targetX = targetCell % settings.width;
        int targetY = targetCell / settings.width;
        int bestDistance = int.MaxValue;
        for (int i = 0; i < snake.segmentPositions.Count; i++)
        {
            int cell;
            if (!TryGetLocalCellIndex(settings, snake.segmentPositions[i], out cell))
            {
                continue;
            }

            int x = cell % settings.width;
            int y = cell / settings.width;
            int distance = Mathf.Abs(x - targetX) + Mathf.Abs(y - targetY);
            if (distance < bestDistance)
            {
                bestDistance = distance;
            }
        }

        return bestDistance;
    }

    private static void RebuildResultStats(Settings settings, Result result)
    {
        result.placedArrowCount = 0;
        result.bentArrowCount = 0;
        result.directionTypeCount = 0;
        result.shapeTypeCount = 0;
        result.directionMask = 0;
        result.shapeMask = 0;
        result.occupiedCellCount = 0;
        result.straightShapeCount = 0;
        result.lShapeCount = 0;
        result.uShapeCount = 0;
        result.zigzagShapeCount = 0;
        result.randomBentShapeCount = 0;
        result.placementAreaCellCount = GetPlacementAreaCellCount(settings);

        for (int i = 0; i < result.snakes.Count; i++)
        {
            SnakeSaveData snake = result.snakes[i];
            if (snake.segmentPositions == null || snake.segmentPositions.Count == 0)
            {
                continue;
            }

            ShapeKind shape = ClassifySnakeShape(settings, snake);
            bool isBent = shape != ShapeKind.Straight;
            result.placedArrowCount++;
            if (isBent)
            {
                result.bentArrowCount++;
            }

            UpdateResultDiversity(result, snake.direction, shape);
            result.occupiedCellCount += snake.segmentPositions.Count;
        }
    }

    private static ShapeKind ClassifySnakeShape(Settings settings, SnakeSaveData snake)
    {
        if (snake.segmentPositions == null || snake.segmentPositions.Count < 3)
        {
            return ShapeKind.Straight;
        }

        int prevDx;
        int prevDy;
        if (!TryGetSnakeStep(settings, snake, 0, 1, out prevDx, out prevDy))
        {
            return ShapeKind.Straight;
        }

        int turnCount = 0;
        int firstDx = prevDx;
        int firstDy = prevDy;
        int lastDx = prevDx;
        int lastDy = prevDy;

        for (int i = 2; i < snake.segmentPositions.Count; i++)
        {
            int dx;
            int dy;
            if (!TryGetSnakeStep(settings, snake, i - 1, i, out dx, out dy))
            {
                continue;
            }

            if (dx != prevDx || dy != prevDy)
            {
                turnCount++;
                lastDx = dx;
                lastDy = dy;
                prevDx = dx;
                prevDy = dy;
            }
        }

        if (turnCount <= 0)
        {
            return ShapeKind.Straight;
        }

        if (turnCount == 1)
        {
            return ShapeKind.L;
        }

        if (turnCount == 2)
        {
            return firstDx == -lastDx && firstDy == -lastDy
                ? ShapeKind.U
                : ShapeKind.Zigzag;
        }

        return ShapeKind.RandomBent;
    }

    private static bool TryGetSnakeStep(Settings settings, SnakeSaveData snake, int fromIndex, int toIndex, out int dx, out int dy)
    {
        dx = 0;
        dy = 0;

        int fromCell;
        int toCell;
        if (!TryGetLocalCellIndex(settings, snake.segmentPositions[fromIndex], out fromCell)
            || !TryGetLocalCellIndex(settings, snake.segmentPositions[toIndex], out toCell))
        {
            return false;
        }

        dx = (toCell % settings.width) - (fromCell % settings.width);
        dy = (toCell / settings.width) - (fromCell / settings.width);
        return true;
    }

    private static void ApplyDeferredFillCandidate(Settings settings, Result result, byte[] occupied, bool[] horizontalLineLanes, bool[] verticalLineLanes, System.Random random, int[] cells, Candidate candidate, int insertionIndex)
    {
        SnakeSaveData snake = BuildSnake(candidate, settings, random, cells);
        MarkSnake(occupied, cells, candidate.length);
        MarkParallelLineLanes(settings, cells, candidate.length, horizontalLineLanes, verticalLineLanes);

        bool hasBentPath = HasBentPath(settings.width, cells, candidate.length);
        AddGeneratedSnakeAtSolveIndex(result, snake, hasBentPath, hasBentPath ? ShapeKind.RandomBent : ShapeKind.Straight, insertionIndex);
    }

    private static bool BuildFillStateFromResult(Settings settings, Result result, byte[] occupied, bool[] horizontalLineLanes, bool[] verticalLineLanes, int[] scratchCells)
    {
        for (int i = 0; i < result.snakes.Count; i++)
        {
            SnakeSaveData snake = result.snakes[i];
            if (snake.segmentPositions == null || snake.segmentPositions.Count == 0 || snake.segmentPositions.Count > scratchCells.Length)
            {
                return false;
            }

            for (int j = 0; j < snake.segmentPositions.Count; j++)
            {
                int cell;
                if (!TryGetLocalCellIndex(settings, snake.segmentPositions[j], out cell))
                {
                    return false;
                }

                if (occupied[cell] != 0)
                {
                    return false;
                }

                occupied[cell] = 1;
                scratchCells[j] = cell;
            }

            MarkParallelLineLanes(settings, scratchCells, snake.segmentPositions.Count, horizontalLineLanes, verticalLineLanes);
        }

        return true;
    }

    private static void AddGeneratedSnakeAtSolveIndex(Result result, SnakeSaveData snake, bool isBent, ShapeKind shape, int insertionIndex)
    {
        int oldCount = result.snakes.Count;
        AddGeneratedSnake(result, snake, isBent, shape);
        if (result.snakes.Count <= oldCount)
        {
            return;
        }

        int lastIndex = result.snakes.Count - 1;
        SnakeSaveData addedSnake = result.snakes[lastIndex];
        result.snakes.RemoveAt(lastIndex);
        result.snakes.Insert(Mathf.Clamp(insertionIndex, 0, result.snakes.Count), addedSnake);
    }

    private static void ConsiderBentFillResult(ref Result bestBentResult, Result current)
    {
        if (current == null || current.bentArrowCount <= 0)
        {
            return;
        }

        if (bestBentResult == null || IsBetterFillResult(current, bestBentResult))
        {
            bestBentResult = current;
        }
    }

    private static bool IsBentFillAcceptable(Result bentResult, Result bestResult, Settings settings)
    {
        if (bentResult == null || bentResult.bentArrowCount <= 0)
        {
            return false;
        }

        if (bestResult == null)
        {
            return true;
        }

        int acceptableLoss = GetBentFillSlack(settings);
        return bentResult.occupiedCellCount + acceptableLoss >= bestResult.occupiedCellCount;
    }

    private static int GetBentFillSlack(Settings settings)
    {
        int area = GetPlacementAreaCellCount(settings);
        return Mathf.Max(settings.minSnakeLength, area / 40);
    }

    private static bool ShouldUseDfsFill(Settings settings)
    {
        int area = GetPlacementAreaCellCount(settings);
        return area <= 180 && settings.maxSnakeLength <= 31;
    }

    private static void ConsiderFillResult(ref Result bestResult, Result current)
    {
        if (current == null)
        {
            return;
        }

        if (bestResult == null || IsBetterFillResult(current, bestResult))
        {
            bestResult = current;
        }
    }

    private static Result GenerateDfsFillResult(Settings settings, int seed)
    {
        Result result = new Result();
        result.placementAreaCellCount = GetPlacementAreaCellCount(settings);

        int totalCells = settings.width * settings.height;
        bool[] occupied = new bool[totalCells];
        bool[] horizontalLineLanes = new bool[settings.height];
        bool[] verticalLineLanes = new bool[settings.width];
        int[] freeCells = new int[totalCells];
        int[] dfsPath = new int[settings.maxSnakeLength];
        DfsBeamBuffers beamBuffers = new DfsBeamBuffers(DfsBeamWidthCap, settings.maxSnakeLength);

        System.Random random = new System.Random(seed);
        bool addedInPass = true;
        int passGuard = 0;

        while (addedInPass && passGuard < totalCells)
        {
            addedInPass = false;
            passGuard++;

            int freeCount = FillFreeCells(settings, occupied, freeCells);
            if (freeCount < settings.minSnakeLength)
            {
                break;
            }

            ShuffleArrayPrefix(freeCells, freeCount, random);

            for (int i = 0; i < freeCount; i++)
            {
                int startCell = freeCells[i];
                int longestLength = GetLastOddAtMost(Mathf.Min(settings.maxSnakeLength, freeCount));

                for (int targetLength = longestLength; targetLength >= settings.minSnakeLength; targetLength -= 2)
                {
                    bool hasBestPath = false;
                    int attempts = Mathf.Clamp(settings.bodyAttemptsPerCandidate, 1, 2);

                    for (int attempt = 0; attempt < attempts; attempt++)
                    {
                        if (!TryCreateDfsSnakePath(settings, startCell, targetLength, occupied, horizontalLineLanes, verticalLineLanes, beamBuffers, dfsPath, random))
                        {
                            continue;
                        }

                        if (IsDfsExitBlocked(settings, dfsPath, targetLength, occupied))
                        {
                            continue;
                        }

                        hasBestPath = true;
                        break;
                    }

                    if (!hasBestPath)
                    {
                        continue;
                    }

                    SnakeSaveData snake = BuildDfsSnake(settings, random, dfsPath, targetLength);
                    MarkDfsSnake(occupied, dfsPath, targetLength);
                    MarkParallelLineLanes(settings, dfsPath, targetLength, horizontalLineLanes, verticalLineLanes);
                    bool isBentPath = HasBentPath(settings.width, dfsPath, targetLength);
                    AddGeneratedSnake(result, snake, isBentPath, isBentPath ? ShapeKind.RandomBent : ShapeKind.Straight);
                    addedInPass = true;
                    break;
                }

                if (addedInPass)
                {
                    break;
                }
            }
        }

        Reverse(result.snakes);
        return result;
    }

    private static int FillFreeCells(Settings settings, bool[] occupied, int[] freeCells)
    {
        int count = 0;
        for (int i = 0; i < occupied.Length; i++)
        {
            if (!occupied[i] && IsPlacementCell(settings, i))
            {
                freeCells[count] = i;
                count++;
            }
        }

        return count;
    }

    private static int FillFreeCells(Settings settings, byte[] occupied, int[] freeCells)
    {
        int count = 0;
        for (int i = 0; i < occupied.Length; i++)
        {
            if (occupied[i] == 0 && IsPlacementCell(settings, i))
            {
                freeCells[count] = i;
                count++;
            }
        }

        return count;
    }

    private static bool TryCreateDfsSnakePath(Settings settings, int startCell, int targetLength, bool[] occupied, bool[] horizontalLineLanes, bool[] verticalLineLanes, DfsBeamBuffers buffers, int[] path, System.Random random)
    {
        if (!CanUseDfsCell(settings, occupied, path, 0, startCell))
        {
            return false;
        }

        int beamWidth = GetDfsBeamWidth(settings, targetLength);
        int[] currentPaths = buffers.currentPaths;
        int[] nextPaths = buffers.nextPaths;
        int[] currentScores = buffers.currentScores;
        int[] nextScores = buffers.nextScores;
        int[] currentLastDirections = buffers.currentLastDirections;
        int[] nextLastDirections = buffers.nextLastDirections;
        int[] currentRunCells = buffers.currentRunCells;
        int[] nextRunCells = buffers.nextRunCells;
        int[] currentTurnCounts = buffers.currentTurnCounts;
        int[] nextTurnCounts = buffers.nextTurnCounts;

        currentPaths[0] = startCell;
        currentScores[0] = 0;
        currentLastDirections[0] = -1;
        currentRunCells[0] = 1;
        currentTurnCounts[0] = 0;
        int currentCount = 1;

        for (int pathLength = 1; pathLength < targetLength; pathLength++)
        {
            int nextCount = 0;
            for (int beamIndex = 0; beamIndex < currentCount; beamIndex++)
            {
                int pathBase = beamIndex * targetLength;
                int currentCell = currentPaths[pathBase + pathLength - 1];
                int currentX = currentCell % settings.width;
                int currentY = currentCell / settings.width;
                int directionOffset = random.Next(0, 4);
                int directionStep = random.Next(0, 2) == 0 ? 1 : 3;

                for (int i = 0; i < 4; i++)
                {
                    int directionIndex = (directionOffset + i * directionStep) & 3;
                    int dx;
                    int dy;
                    GetDfsDirectionStep(directionIndex, out dx, out dy);

                    int nextX = currentX + dx;
                    int nextY = currentY + dy;
                    if (!IsPlacementCell(settings, nextX, nextY))
                    {
                        continue;
                    }

                    int nextCell = ToIndex(settings.width, nextX, nextY);
                    if (!CanUseBeamCell(settings, occupied, currentPaths, targetLength, beamIndex, pathLength, nextCell))
                    {
                        continue;
                    }

                    int lastDirection = currentLastDirections[beamIndex];
                    bool isTurn = lastDirection >= 0 && directionIndex != lastDirection;
                    if (isTurn)
                    {
                        if (!settings.allowBentSnakes || !IsValidStraightRun(settings, currentRunCells[beamIndex]))
                        {
                            continue;
                        }

                        int remainingCells = targetLength - pathLength - 1;
                        if (remainingCells > 0 && remainingCells < settings.minStraightCellsPerSegment - 2)
                        {
                            continue;
                        }
                    }

                    int newRunCells = lastDirection < 0
                        ? 2
                        : isTurn ? 2 : currentRunCells[beamIndex] + 1;
                    int newTurnCount = currentTurnCounts[beamIndex] + (isTurn ? 1 : 0);
                    int onwardOptions = pathLength + 1 >= targetLength
                        ? 0
                        : CountBeamOnwardOptions(settings, occupied, currentPaths, targetLength, beamIndex, pathLength, nextCell);

                    if (pathLength + 1 < targetLength && onwardOptions <= 0)
                    {
                        continue;
                    }

                    int score = currentScores[beamIndex]
                        + GetBeamStepScore(settings, currentX, currentY, nextX, nextY, isTurn, newTurnCount, onwardOptions, random);
                    InsertBeamCandidate(
                        currentPaths,
                        nextPaths,
                        nextScores,
                        nextLastDirections,
                        nextRunCells,
                        nextTurnCounts,
                        targetLength,
                        beamIndex,
                        pathLength,
                        nextCell,
                        directionIndex,
                        newRunCells,
                        newTurnCount,
                        score,
                        beamWidth,
                        ref nextCount);
                }
            }

            if (nextCount <= 0)
            {
                return false;
            }

            Swap(ref currentPaths, ref nextPaths);
            Swap(ref currentScores, ref nextScores);
            Swap(ref currentLastDirections, ref nextLastDirections);
            Swap(ref currentRunCells, ref nextRunCells);
            Swap(ref currentTurnCounts, ref nextTurnCounts);
            currentCount = nextCount;
        }

        for (int i = 0; i < currentCount; i++)
        {
            int pathBase = i * targetLength;
            for (int j = 0; j < targetLength; j++)
            {
                path[j] = currentPaths[pathBase + j];
            }

            ReverseRange(path, targetLength);
            if (HasValidStraightRuns(settings, path, targetLength)
                && HasValidParallelLineLaneParity(settings, path, targetLength, horizontalLineLanes, verticalLineLanes))
            {
                return true;
            }
        }

        return false;
    }

    private static int GetDfsBeamWidth(Settings settings, int targetLength)
    {
        int baseWidth = Mathf.Clamp(settings.bodyAttemptsPerCandidate + 3, 4, 8);
        if (targetLength <= 9)
        {
            baseWidth += 2;
        }

        int area = GetPlacementAreaCellCount(settings);
        if (area <= 90)
        {
            baseWidth += 2;
        }

        return Mathf.Clamp(baseWidth, 4, DfsBeamWidthCap);
    }

    private static bool CanUseBeamCell(Settings settings, bool[] occupied, int[] paths, int stride, int beamIndex, int pathLength, int cell)
    {
        if (occupied[cell])
        {
            return false;
        }

        if (!IsPlacementCell(settings, cell))
        {
            return false;
        }

        int x = cell % settings.width;
        int y = cell / settings.width;
        if (IsTooCloseToOccupied(settings, occupied, x, y))
        {
            return false;
        }

        int pathBase = beamIndex * stride;
        int exclusiveDistance = settings.minDistanceBetweenSnakes;
        for (int i = 0; i < pathLength; i++)
        {
            int pathCell = paths[pathBase + i];
            if (pathCell == cell)
            {
                return false;
            }

            if (exclusiveDistance > 1
                && i < pathLength - 1
                && GetManhattanDistance(settings.width, pathCell, cell) < exclusiveDistance)
            {
                return false;
            }
        }

        return true;
    }

    private static int CountBeamOnwardOptions(Settings settings, bool[] occupied, int[] paths, int stride, int beamIndex, int pathLength, int nextCell)
    {
        int count = 0;
        int x = nextCell % settings.width;
        int y = nextCell / settings.width;

        for (int i = 0; i < 4; i++)
        {
            int dx;
            int dy;
            GetDfsDirectionStep(i, out dx, out dy);

            int checkX = x + dx;
            int checkY = y + dy;
            if (!IsPlacementCell(settings, checkX, checkY))
            {
                continue;
            }

            int checkCell = ToIndex(settings.width, checkX, checkY);
            if (CanUseBeamLookaheadCell(settings, occupied, paths, stride, beamIndex, pathLength, nextCell, checkCell))
            {
                count++;
            }
        }

        return count;
    }

    private static bool CanUseBeamLookaheadCell(Settings settings, bool[] occupied, int[] paths, int stride, int beamIndex, int pathLength, int parentCell, int cell)
    {
        if (cell == parentCell || occupied[cell])
        {
            return false;
        }

        int x = cell % settings.width;
        int y = cell / settings.width;
        if (IsTooCloseToOccupied(settings, occupied, x, y))
        {
            return false;
        }

        int pathBase = beamIndex * stride;
        for (int i = 0; i < pathLength; i++)
        {
            if (paths[pathBase + i] == cell)
            {
                return false;
            }
        }

        return true;
    }

    private static int GetBeamStepScore(Settings settings, int fromX, int fromY, int toX, int toY, bool isTurn, int turnCount, int onwardOptions, System.Random random)
    {
        int score = 1000;
        score -= onwardOptions * 120;

        if (isTurn)
        {
            score += 180;
        }

        if (turnCount > 0)
        {
            score += 30;
        }

        if (toX == 0 || toX == settings.width - 1)
        {
            score += 12;
        }

        if (toY == 0 || toY == settings.height - 1)
        {
            score += 12;
        }

        if (fromX != toX && fromY != toY)
        {
            score -= 500;
        }

        return score + random.Next(0, 45);
    }

    private static void InsertBeamCandidate(
        int[] currentPaths,
        int[] nextPaths,
        int[] nextScores,
        int[] nextLastDirections,
        int[] nextRunCells,
        int[] nextTurnCounts,
        int stride,
        int sourceBeamIndex,
        int pathLength,
        int nextCell,
        int directionIndex,
        int runCells,
        int turnCount,
        int score,
        int beamWidth,
        ref int nextCount)
    {
        int slot = nextCount;
        if (nextCount < beamWidth)
        {
            nextCount++;
        }
        else
        {
            int worstIndex = 0;
            int worstScore = nextScores[0];
            for (int i = 1; i < beamWidth; i++)
            {
                if (nextScores[i] < worstScore)
                {
                    worstScore = nextScores[i];
                    worstIndex = i;
                }
            }

            if (score <= worstScore)
            {
                return;
            }

            slot = worstIndex;
        }

        int sourceBase = sourceBeamIndex * stride;
        int targetBase = slot * stride;
        for (int i = 0; i < pathLength; i++)
        {
            nextPaths[targetBase + i] = currentPaths[sourceBase + i];
        }

        nextPaths[targetBase + pathLength] = nextCell;
        nextScores[slot] = score;
        nextLastDirections[slot] = directionIndex;
        nextRunCells[slot] = runCells;
        nextTurnCounts[slot] = turnCount;
    }

    private static void Swap(ref int[] left, ref int[] right)
    {
        int[] temp = left;
        left = right;
        right = temp;
    }

    private static bool IsDfsExitBlocked(Settings settings, int[] path, int length, bool[] occupied)
    {
        if (path == null || length < 2)
        {
            return true;
        }

        int headCell = path[0];
        int neckCell = path[1];
        int headX = headCell % settings.width;
        int headY = headCell / settings.width;
        int neckX = neckCell % settings.width;
        int neckY = neckCell / settings.width;

        int dx = headX - neckX;
        int dy = headY - neckY;
        int checkX = headX + dx;
        int checkY = headY + dy;

        while (IsInside(settings.width, settings.height, checkX, checkY))
        {
            if (occupied[ToIndex(settings.width, checkX, checkY)])
            {
                return true;
            }

            checkX += dx;
            checkY += dy;
        }

        return false;
    }

    private static bool CanUseDfsCell(Settings settings, bool[] occupied, int[] currentPath, int pathLength, int cell)
    {
        if (occupied[cell])
        {
            return false;
        }

        if (!IsPlacementCell(settings, cell))
        {
            return false;
        }

        int x = cell % settings.width;
        int y = cell / settings.width;
        if (IsTooCloseToOccupied(settings, occupied, x, y))
        {
            return false;
        }

        if (currentPath == null || pathLength <= 0)
        {
            return true;
        }

        if (IsTooCloseToCurrentDfsPath(settings, currentPath, pathLength, cell))
        {
            return false;
        }

        return true;
    }

    private static bool IsTooCloseToCurrentDfsPath(Settings settings, int[] currentPath, int pathLength, int cell)
    {
        int exclusiveDistance = settings.minDistanceBetweenSnakes;
        if (exclusiveDistance <= 1 || currentPath == null || pathLength <= 1)
        {
            return false;
        }

        // The last cell is the direct parent in DFS and must stay adjacent to the new cell.
        for (int i = 0; i < pathLength - 1; i++)
        {
            if (GetManhattanDistance(settings.width, currentPath[i], cell) < exclusiveDistance)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsTooCloseToOccupied(Settings settings, bool[] occupied, int x, int y)
    {
        int exclusiveDistance = settings.minDistanceBetweenSnakes;
        if (exclusiveDistance <= 1)
        {
            return false;
        }

        CellOffset[] offsets = GetSpacingOffsets(exclusiveDistance);
        for (int i = 0; i < offsets.Length; i++)
        {
            int checkX = x + offsets[i].x;
            int checkY = y + offsets[i].y;
            if (!IsInside(settings.width, settings.height, checkX, checkY))
            {
                continue;
            }

            if (occupied[ToIndex(settings.width, checkX, checkY)])
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasValidDfsStraightRuns(Settings settings, List<int> path)
    {
        if (path == null || path.Count < 2)
        {
            return false;
        }

        int prevDx = GetDfsCellX(settings.width, path[1]) - GetDfsCellX(settings.width, path[0]);
        int prevDy = GetDfsCellY(settings.width, path[1]) - GetDfsCellY(settings.width, path[0]);
        int runCells = 2;

        for (int i = 2; i < path.Count; i++)
        {
            int dx = GetDfsCellX(settings.width, path[i]) - GetDfsCellX(settings.width, path[i - 1]);
            int dy = GetDfsCellY(settings.width, path[i]) - GetDfsCellY(settings.width, path[i - 1]);
            if (dx == prevDx && dy == prevDy)
            {
                runCells++;
                continue;
            }

            if (!settings.allowBentSnakes || !IsValidStraightRun(settings, runCells))
            {
                return false;
            }

            prevDx = dx;
            prevDy = dy;
            runCells = 2;
        }

        return IsValidStraightRun(settings, runCells);
    }

    private static bool HasValidDfsSelfSpacing(Settings settings, List<int> path)
    {
        int exclusiveDistance = settings.minDistanceBetweenSnakes;
        if (exclusiveDistance <= 1 || path == null)
        {
            return true;
        }

        for (int i = 0; i < path.Count; i++)
        {
            for (int j = i + 2; j < path.Count; j++)
            {
                if (GetManhattanDistance(settings.width, path[i], path[j]) < exclusiveDistance)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool HasValidParallelLineLaneParity(Settings settings, List<int> path, bool[] horizontalLineLanes, bool[] verticalLineLanes)
    {
        if (path == null || path.Count < 2)
        {
            return false;
        }

        bool[] localHorizontal = new bool[settings.height];
        bool[] localVertical = new bool[settings.width];

        int prevDx = GetDfsCellX(settings.width, path[1]) - GetDfsCellX(settings.width, path[0]);
        int prevDy = GetDfsCellY(settings.width, path[1]) - GetDfsCellY(settings.width, path[0]);
        int runStartCell = path[0];

        for (int i = 2; i < path.Count; i++)
        {
            int dx = GetDfsCellX(settings.width, path[i]) - GetDfsCellX(settings.width, path[i - 1]);
            int dy = GetDfsCellY(settings.width, path[i]) - GetDfsCellY(settings.width, path[i - 1]);
            if (dx == prevDx && dy == prevDy)
            {
                continue;
            }

            if (!CanUseParallelLineLane(settings, runStartCell, prevDy == 0, horizontalLineLanes, verticalLineLanes, localHorizontal, localVertical))
            {
                return false;
            }

            MarkParallelLineLane(settings, runStartCell, prevDy == 0, localHorizontal, localVertical);
            runStartCell = path[i - 1];
            prevDx = dx;
            prevDy = dy;
        }

        return CanUseParallelLineLane(settings, runStartCell, prevDy == 0, horizontalLineLanes, verticalLineLanes, localHorizontal, localVertical);
    }

    private static bool HasValidParallelLineLaneParity(Settings settings, int[] cells, int length, bool[] horizontalLineLanes, bool[] verticalLineLanes)
    {
        if (cells == null || length < 2)
        {
            return false;
        }

        bool[] localHorizontal = new bool[settings.height];
        bool[] localVertical = new bool[settings.width];

        int prevDx = GetDfsCellX(settings.width, cells[1]) - GetDfsCellX(settings.width, cells[0]);
        int prevDy = GetDfsCellY(settings.width, cells[1]) - GetDfsCellY(settings.width, cells[0]);
        int runStartCell = cells[0];

        for (int i = 2; i < length; i++)
        {
            int dx = GetDfsCellX(settings.width, cells[i]) - GetDfsCellX(settings.width, cells[i - 1]);
            int dy = GetDfsCellY(settings.width, cells[i]) - GetDfsCellY(settings.width, cells[i - 1]);
            if (dx == prevDx && dy == prevDy)
            {
                continue;
            }

            if (!CanUseParallelLineLane(settings, runStartCell, prevDy == 0, horizontalLineLanes, verticalLineLanes, localHorizontal, localVertical))
            {
                return false;
            }

            MarkParallelLineLane(settings, runStartCell, prevDy == 0, localHorizontal, localVertical);
            runStartCell = cells[i - 1];
            prevDx = dx;
            prevDy = dy;
        }

        return CanUseParallelLineLane(settings, runStartCell, prevDy == 0, horizontalLineLanes, verticalLineLanes, localHorizontal, localVertical);
    }

    private static bool CanUseParallelLineLane(Settings settings, int runStartCell, bool horizontal, bool[] horizontalLineLanes, bool[] verticalLineLanes, bool[] localHorizontal, bool[] localVertical)
    {
        int lane = horizontal
            ? GetDfsCellY(settings.width, runStartCell)
            : GetDfsCellX(settings.width, runStartCell);

        bool[] globalLanes = horizontal ? horizontalLineLanes : verticalLineLanes;
        bool[] localLanes = horizontal ? localHorizontal : localVertical;
        return HasCompatibleParallelLineLane(globalLanes, lane)
            && HasCompatibleParallelLineLane(localLanes, lane);
    }

    private static bool HasCompatibleParallelLineLane(bool[] lanes, int lane)
    {
        if (lanes == null)
        {
            return true;
        }

        for (int i = 0; i < lanes.Length; i++)
        {
            if (!lanes[i])
            {
                continue;
            }

            int laneDistance = Mathf.Abs(i - lane);
            if ((laneDistance & 1) != 0)
            {
                return false;
            }
        }

        return true;
    }

    private static void MarkParallelLineLanes(Settings settings, List<int> path, bool[] horizontalLineLanes, bool[] verticalLineLanes)
    {
        if (path == null || path.Count < 2)
        {
            return;
        }

        int prevDx = GetDfsCellX(settings.width, path[1]) - GetDfsCellX(settings.width, path[0]);
        int prevDy = GetDfsCellY(settings.width, path[1]) - GetDfsCellY(settings.width, path[0]);
        int runStartCell = path[0];

        for (int i = 2; i < path.Count; i++)
        {
            int dx = GetDfsCellX(settings.width, path[i]) - GetDfsCellX(settings.width, path[i - 1]);
            int dy = GetDfsCellY(settings.width, path[i]) - GetDfsCellY(settings.width, path[i - 1]);
            if (dx == prevDx && dy == prevDy)
            {
                continue;
            }

            MarkParallelLineLane(settings, runStartCell, prevDy == 0, horizontalLineLanes, verticalLineLanes);
            runStartCell = path[i - 1];
            prevDx = dx;
            prevDy = dy;
        }

        MarkParallelLineLane(settings, runStartCell, prevDy == 0, horizontalLineLanes, verticalLineLanes);
    }

    private static void MarkParallelLineLanes(Settings settings, int[] cells, int length, bool[] horizontalLineLanes, bool[] verticalLineLanes)
    {
        if (cells == null || length < 2)
        {
            return;
        }

        int prevDx = GetDfsCellX(settings.width, cells[1]) - GetDfsCellX(settings.width, cells[0]);
        int prevDy = GetDfsCellY(settings.width, cells[1]) - GetDfsCellY(settings.width, cells[0]);
        int runStartCell = cells[0];

        for (int i = 2; i < length; i++)
        {
            int dx = GetDfsCellX(settings.width, cells[i]) - GetDfsCellX(settings.width, cells[i - 1]);
            int dy = GetDfsCellY(settings.width, cells[i]) - GetDfsCellY(settings.width, cells[i - 1]);
            if (dx == prevDx && dy == prevDy)
            {
                continue;
            }

            MarkParallelLineLane(settings, runStartCell, prevDy == 0, horizontalLineLanes, verticalLineLanes);
            runStartCell = cells[i - 1];
            prevDx = dx;
            prevDy = dy;
        }

        MarkParallelLineLane(settings, runStartCell, prevDy == 0, horizontalLineLanes, verticalLineLanes);
    }

    private static void MarkParallelLineLane(Settings settings, int runStartCell, bool horizontal, bool[] horizontalLineLanes, bool[] verticalLineLanes)
    {
        int lane = horizontal
            ? GetDfsCellY(settings.width, runStartCell)
            : GetDfsCellX(settings.width, runStartCell);

        if (horizontal)
        {
            if (horizontalLineLanes != null && lane >= 0 && lane < horizontalLineLanes.Length)
            {
                horizontalLineLanes[lane] = true;
            }
        }
        else if (verticalLineLanes != null && lane >= 0 && lane < verticalLineLanes.Length)
        {
            verticalLineLanes[lane] = true;
        }
    }

    private static int GetDfsCellX(int width, int cell)
    {
        return cell % width;
    }

    private static int GetDfsCellY(int width, int cell)
    {
        return cell / width;
    }

    private static SnakeSaveData BuildDfsSnake(Settings settings, System.Random random, List<int> path)
    {
        SnakeSaveData snake = new SnakeSaveData();
        snake.direction = GetDfsArrowDirection(settings.width, path);
        snake.arrowColor = PickColor(settings.colorPalette, random);
        snake.segmentPositions = new List<Vector2Int>(path.Count);

        for (int i = 0; i < path.Count; i++)
        {
            int cell = path[i];
            int x = cell % settings.width;
            int y = cell / settings.width;
            snake.segmentPositions.Add(new Vector2Int(settings.originX + x, settings.originY + y));
        }

        return snake;
    }

    private static SnakeSaveData BuildDfsSnake(Settings settings, System.Random random, int[] path, int length)
    {
        SnakeSaveData snake = new SnakeSaveData();
        snake.direction = GetDfsArrowDirection(settings.width, path, length);
        snake.arrowColor = PickColor(settings.colorPalette, random);
        snake.segmentPositions = new List<Vector2Int>(length);

        for (int i = 0; i < length; i++)
        {
            int cell = path[i];
            int x = cell % settings.width;
            int y = cell / settings.width;
            snake.segmentPositions.Add(new Vector2Int(settings.originX + x, settings.originY + y));
        }

        return snake;
    }

    private static ArrowDir GetDfsArrowDirection(int width, List<int> path)
    {
        if (path == null || path.Count < 2)
        {
            return ArrowDir.Up;
        }

        int headCell = path[0];
        int neckCell = path[1];
        int dx = (headCell % width) - (neckCell % width);
        int dy = (headCell / width) - (neckCell / width);

        if (dy > 0) return ArrowDir.Up;
        if (dy < 0) return ArrowDir.Down;
        if (dx < 0) return ArrowDir.Left;
        return ArrowDir.Right;
    }

    private static ArrowDir GetDfsArrowDirection(int width, int[] path, int length)
    {
        if (path == null || length < 2)
        {
            return ArrowDir.Up;
        }

        int headCell = path[0];
        int neckCell = path[1];
        int dx = (headCell % width) - (neckCell % width);
        int dy = (headCell / width) - (neckCell / width);

        if (dy > 0) return ArrowDir.Up;
        if (dy < 0) return ArrowDir.Down;
        if (dx < 0) return ArrowDir.Left;
        return ArrowDir.Right;
    }

    private static void MarkDfsSnake(bool[] occupied, List<int> path)
    {
        for (int i = 0; i < path.Count; i++)
        {
            occupied[path[i]] = true;
        }
    }

    private static void MarkDfsSnake(bool[] occupied, int[] path, int length)
    {
        for (int i = 0; i < length; i++)
        {
            occupied[path[i]] = true;
        }
    }

    private static bool IsBentDfsPath(int width, List<int> path)
    {
        if (path == null || path.Count < 3)
        {
            return false;
        }

        int prevDx = (path[1] % width) - (path[0] % width);
        int prevDy = (path[1] / width) - (path[0] / width);
        for (int i = 2; i < path.Count; i++)
        {
            int dx = (path[i] % width) - (path[i - 1] % width);
            int dy = (path[i] / width) - (path[i - 1] / width);
            if (dx != prevDx || dy != prevDy)
            {
                return true;
            }

            prevDx = dx;
            prevDy = dy;
        }

        return false;
    }

    private static void GetDfsDirectionStep(int directionIndex, out int dx, out int dy)
    {
        switch (directionIndex & 3)
        {
            case 0:
                dx = 1;
                dy = 0;
                return;
            case 1:
                dx = -1;
                dy = 0;
                return;
            case 2:
                dx = 0;
                dy = 1;
                return;
            default:
                dx = 0;
                dy = -1;
                return;
        }
    }

    private static void Shuffle(int[] values, System.Random random)
    {
        int count = values.Length;
        while (count > 1)
        {
            int index = random.Next(count--);
            int value = values[index];
            values[index] = values[count];
            values[count] = value;
        }
    }

    private static void ShuffleList<T>(IList<T> values, System.Random random)
    {
        int count = values.Count;
        while (count > 1)
        {
            int index = random.Next(count--);
            T value = values[index];
            values[index] = values[count];
            values[count] = value;
        }
    }

    private static void ShuffleArrayPrefix(int[] values, int length, System.Random random)
    {
        int count = length;
        while (count > 1)
        {
            int index = random.Next(count--);
            int value = values[index];
            values[index] = values[count];
            values[count] = value;
        }
    }

    private static int NextVisitGeneration(int[] visited, ref int generation)
    {
        generation++;
        if (generation == int.MaxValue)
        {
            System.Array.Clear(visited, 0, visited.Length);
            generation = 1;
        }

        return generation;
    }

    private static void ReverseRange(int[] values, int length)
    {
        int left = 0;
        int right = length - 1;
        while (left < right)
        {
            int temp = values[left];
            values[left] = values[right];
            values[right] = temp;
            left++;
            right--;
        }
    }

    private static Result GenerateBestStripedFillResult(Settings settings)
    {
        if (HasPlacementMask(settings))
        {
            return null;
        }

        Result bestResult = null;
        int laneStep = GetEvenLaneStep(settings);

        for (int offset = 0; offset < laneStep; offset++)
        {
            Result horizontalPairedRight = GeneratePairedLaneFillResult(settings, true, offset, true);
            ConsiderSolvableLaneFillResult(settings, ref bestResult, horizontalPairedRight);

            Result horizontalPairedLeft = GeneratePairedLaneFillResult(settings, true, offset, false);
            ConsiderSolvableLaneFillResult(settings, ref bestResult, horizontalPairedLeft);

            Result verticalPairedUp = GeneratePairedLaneFillResult(settings, false, offset, true);
            ConsiderSolvableLaneFillResult(settings, ref bestResult, verticalPairedUp);

            Result verticalPairedDown = GeneratePairedLaneFillResult(settings, false, offset, false);
            ConsiderSolvableLaneFillResult(settings, ref bestResult, verticalPairedDown);

            Result horizontal = GenerateStripedFillResult(settings, true, offset);
            ConsiderSolvableLaneFillResult(settings, ref bestResult, horizontal);

            Result vertical = GenerateStripedFillResult(settings, false, offset);
            ConsiderSolvableLaneFillResult(settings, ref bestResult, vertical);
        }

        return bestResult;
    }

    private static void ConsiderSolvableLaneFillResult(Settings settings, ref Result bestResult, Result current)
    {
        current = EnsureSolvableResult(settings, current);
        ConsiderFillResult(ref bestResult, current);
    }

    private static Result GenerateStripedFillResult(Settings settings, bool horizontal, int laneOffset)
    {
        Result result = new Result();
        result.placementAreaCellCount = GetPlacementAreaCellCount(settings);

        int laneStep = GetEvenLaneStep(settings);
        int laneCount = horizontal ? settings.height : settings.width;
        int laneLength = horizontal ? settings.width : settings.height;
        laneLength = Mathf.Min(settings.maxSnakeLength, GetLastOddAtMost(laneLength));

        if (laneLength < settings.minSnakeLength)
        {
            return result;
        }

        System.Random random = new System.Random(settings.seed + (horizontal ? 101 : 503) + laneOffset * 31);
        int generatedLaneIndex = 0;
        for (int lane = laneOffset; lane < laneCount; lane += laneStep)
        {
            bool reverseLane = (generatedLaneIndex & 1) != 0;
            SnakeSaveData snake = BuildStripedSnake(settings, random, horizontal, lane, laneLength, reverseLane);
            AddGeneratedSnake(result, snake, false, ShapeKind.Straight);
            generatedLaneIndex++;
        }

        return result;
    }

    private static Result GenerateMixedTemplateFillResult(Settings settings, int seed)
    {
        Result result = new Result();
        result.placementAreaCellCount = GetPlacementAreaCellCount(settings);

        byte[] occupied = new byte[settings.width * settings.height];
        bool[] horizontalLineLanes = new bool[settings.height];
        bool[] verticalLineLanes = new bool[settings.width];
        int[] candidateCells = new int[settings.maxSnakeLength];
        int[] bestCells = new int[settings.maxSnakeLength];
        int[] pathVisited = new int[settings.width * settings.height];
        int pathVisitGeneration = 1;
        int[] freeCellBuffer = new int[settings.width * settings.height];
        System.Random random = new System.Random(seed);

        int maxPlacements = GetPlacementAreaCellCount(settings);
        for (int placement = 0; placement < maxPlacements; placement++)
        {
            bool found = false;
            int bestScore = int.MinValue;
            int bestLength = 0;
            ArrowDir bestDirection = ArrowDir.Up;
            ShapeKind bestShape = ShapeKind.Straight;

            for (int attempt = 0; attempt < settings.fillSearchAttempts; attempt++)
            {
                ShapeKind shape = PickTemplateShape(random);
                ArrowDir dir = PickTemplateDirection(result, random);
                int x = random.Next(0, settings.width);
                int y = random.Next(0, settings.height);

                int length;
                ShapeKind actualShape;
                if (!TryBuildTemplatePath(settings, occupied, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, random, shape, x, y, dir, candidateCells, out length, out actualShape))
                {
                    continue;
                }

                int score = GetTemplateCandidateScore(result, dir, actualShape, length, random);
                if (!found || score > bestScore)
                {
                    found = true;
                    bestScore = score;
                    bestLength = length;
                    bestDirection = dir;
                    bestShape = actualShape;
                    CopyCells(candidateCells, bestCells, length);
                }
            }

            if (!found)
            {
                break;
            }

            SnakeSaveData snake = BuildSnake(bestDirection, settings, random, bestCells, bestLength);
            MarkSnake(occupied, bestCells, bestLength);
            MarkParallelLineLanes(settings, bestCells, bestLength, horizontalLineLanes, verticalLineLanes);
            AddGeneratedSnake(result, snake, bestShape != ShapeKind.Straight, bestShape);
        }

        FillRemainderWithBestCandidates(settings, occupied, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, freeCellBuffer, random, candidateCells, bestCells, result);
        Reverse(result.snakes);
        return result;
    }

    private static ShapeKind PickTemplateShape(System.Random random)
    {
        int roll = random.Next(0, 100);
        if (roll < 18) return ShapeKind.Straight;
        if (roll < 50) return ShapeKind.L;
        if (roll < 76) return ShapeKind.U;
        return ShapeKind.Zigzag;
    }

    private static ArrowDir PickTemplateDirection(Result result, System.Random random)
    {
        if (result.directionTypeCount < 4 && random.Next(0, 100) < 70)
        {
            int start = random.Next(0, 4);
            for (int i = 0; i < 4; i++)
            {
                int dirIndex = (start + i) & 3;
                if ((result.directionMask & (1 << dirIndex)) == 0)
                {
                    return (ArrowDir)dirIndex;
                }
            }
        }

        return (ArrowDir)random.Next(0, 4);
    }

    private static int GetTemplateCandidateScore(Result result, ArrowDir dir, ShapeKind shape, int length, System.Random random)
    {
        int dirIndex = (int)dir & 3;
        int shapeIndex = (int)shape;
        int score = length * 200;

        if ((result.directionMask & (1 << dirIndex)) == 0)
        {
            score += 1500;
        }

        if ((result.shapeMask & (1 << shapeIndex)) == 0)
        {
            score += 800;
        }

        if (shape != ShapeKind.Straight)
        {
            score += 1800;
        }

        if (result.placedArrowCount > 0)
        {
            int straightPercent = result.straightShapeCount * 100 / result.placedArrowCount;
            if (shape == ShapeKind.Straight && straightPercent >= 35)
            {
                score -= 1800;
            }

            int bentPercent = result.bentArrowCount * 100 / result.placedArrowCount;
            if (shape != ShapeKind.Straight && bentPercent < 45)
            {
                score += 1200;
            }
        }

        score += random.Next(0, 250);
        return score;
    }

    private static bool TryBuildTemplatePath(Settings settings, byte[] occupied, bool[] horizontalLineLanes, bool[] verticalLineLanes, int[] pathVisited, ref int pathVisitGeneration, System.Random random, ShapeKind requestedShape, int headX, int headY, ArrowDir dir, int[] candidateCells, out int length, out ShapeKind actualShape)
    {
        length = 0;
        actualShape = requestedShape;

        int exitDx;
        int exitDy;
        GetStep(dir, out exitDx, out exitDy);

        if (!IsPlacementCell(settings, headX, headY))
        {
            return false;
        }

        int headIndex = ToIndex(settings.width, headX, headY);
        if (occupied[headIndex] != 0 || IsTooCloseToOccupied(settings, occupied, headX, headY))
        {
            return false;
        }

        if (!IsExitPathClear(settings, occupied, headX, headY, exitDx, exitDy))
        {
            return false;
        }

        candidateCells[0] = headIndex;
        int visitGeneration = NextVisitGeneration(pathVisited, ref pathVisitGeneration);
        pathVisited[headIndex] = visitGeneration;
        int used = 1;
        int bodyDx = -exitDx;
        int bodyDy = -exitDy;

        int maxLength = settings.maxSnakeLength;
        int minRun = settings.minStraightCellsPerSegment;

        if (requestedShape == ShapeKind.Straight)
        {
            int runLength = GetWeightedOddInRange(minRun, maxLength, random);
            if (!TryAppendRun(settings, occupied, pathVisited, visitGeneration, candidateCells, ref used, headX, headY, exitDx, exitDy, bodyDx, bodyDy, runLength))
            {
                return false;
            }
        }
        else if (requestedShape == ShapeKind.L)
        {
            if (maxLength < minRun * 2 - 1)
            {
                return false;
            }

            int firstRun = GetWeightedOddInRange(minRun, maxLength - minRun + 1, random);
            int secondMax = maxLength - firstRun + 1;
            int secondRun = GetWeightedOddInRange(minRun, secondMax, random);
            int turnDir = random.Next(0, 2) == 0
                ? GetLeftTurnDirectionIndex(bodyDx, bodyDy)
                : GetRightTurnDirectionIndex(bodyDx, bodyDy);

            if (!TryAppendRun(settings, occupied, pathVisited, visitGeneration, candidateCells, ref used, headX, headY, exitDx, exitDy, bodyDx, bodyDy, firstRun))
            {
                return false;
            }

            int turnDx;
            int turnDy;
            GetStep(turnDir, out turnDx, out turnDy);
            if (!TryAppendRun(settings, occupied, pathVisited, visitGeneration, candidateCells, ref used, headX, headY, exitDx, exitDy, turnDx, turnDy, secondRun))
            {
                return false;
            }
        }
        else
        {
            int minimumTotal = minRun * 3 - 2;
            if (maxLength < minimumTotal)
            {
                return false;
            }

            int firstRun = GetWeightedOddInRange(minRun, maxLength - (minRun * 2 - 2), random);
            int remainingAfterFirst = maxLength - firstRun + 1;
            int secondRun = GetWeightedOddInRange(minRun, remainingAfterFirst - minRun + 1, random);
            int thirdMax = maxLength - firstRun - secondRun + 2;
            int thirdRun = GetWeightedOddInRange(minRun, thirdMax, random);

            int turnDir = random.Next(0, 2) == 0
                ? GetLeftTurnDirectionIndex(bodyDx, bodyDy)
                : GetRightTurnDirectionIndex(bodyDx, bodyDy);
            int turnDx;
            int turnDy;
            GetStep(turnDir, out turnDx, out turnDy);

            int thirdDx = requestedShape == ShapeKind.U ? -bodyDx : bodyDx;
            int thirdDy = requestedShape == ShapeKind.U ? -bodyDy : bodyDy;

            if (!TryAppendRun(settings, occupied, pathVisited, visitGeneration, candidateCells, ref used, headX, headY, exitDx, exitDy, bodyDx, bodyDy, firstRun))
            {
                return false;
            }

            if (!TryAppendRun(settings, occupied, pathVisited, visitGeneration, candidateCells, ref used, headX, headY, exitDx, exitDy, turnDx, turnDy, secondRun))
            {
                return false;
            }

            if (!TryAppendRun(settings, occupied, pathVisited, visitGeneration, candidateCells, ref used, headX, headY, exitDx, exitDy, thirdDx, thirdDy, thirdRun))
            {
                return false;
            }
        }

        if (!HasValidStraightRuns(settings, candidateCells, used))
        {
            return false;
        }

        if (!HasValidParallelLineLaneParity(settings, candidateCells, used, horizontalLineLanes, verticalLineLanes))
        {
            return false;
        }

        length = used;
        return length >= settings.minSnakeLength;
    }

    private static bool TryAppendRun(Settings settings, byte[] occupied, int[] pathVisited, int visitGeneration, int[] candidateCells, ref int used, int headX, int headY, int exitDx, int exitDy, int dx, int dy, int runCells)
    {
        if (runCells < 1)
        {
            return false;
        }

        int currentIndex = candidateCells[used - 1];
        int x = currentIndex % settings.width;
        int y = currentIndex / settings.width;

        int cellsToAdd = used == 1 ? runCells - 1 : runCells - 1;
        for (int i = 0; i < cellsToAdd; i++)
        {
            x += dx;
            y += dy;

            if (!CanUseBodyCell(settings, occupied, pathVisited, visitGeneration, candidateCells, used, headX, headY, exitDx, exitDy, x, y))
            {
                return false;
            }

            if (used >= candidateCells.Length)
            {
                return false;
            }

            candidateCells[used] = ToIndex(settings.width, x, y);
            pathVisited[candidateCells[used]] = visitGeneration;
            used++;
        }

        return true;
    }

    private static void FillRemainderWithBestCandidates(Settings settings, byte[] occupied, bool[] horizontalLineLanes, bool[] verticalLineLanes, int[] pathVisited, ref int pathVisitGeneration, int[] freeCellBuffer, System.Random random, int[] candidateCells, int[] bestCells, Result result)
    {
        int maxPlacements = GetPlacementAreaCellCount(settings);
        for (int i = 0; i < maxPlacements; i++)
        {
            Candidate candidate;
            if (!TryFindBestFillCandidate(settings, occupied, result.occupiedCellCount, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, freeCellBuffer, random, candidateCells, bestCells, out candidate))
            {
                break;
            }

            SnakeSaveData snake = BuildSnake(candidate, settings, random, bestCells);
            MarkSnake(occupied, bestCells, candidate.length);
            MarkParallelLineLanes(settings, bestCells, candidate.length, horizontalLineLanes, verticalLineLanes);

            bool hasBentPath = HasBentPath(settings.width, bestCells, candidate.length);
            AddGeneratedSnake(result, snake, hasBentPath, hasBentPath ? ShapeKind.RandomBent : ShapeKind.Straight);
        }
    }

    private static Result GeneratePairedLaneFillResult(Settings settings, bool horizontal, int laneOffset, bool headAtPositiveEdge)
    {
        Result result = new Result();
        result.placementAreaCellCount = GetPlacementAreaCellCount(settings);

        int laneStep = GetEvenLaneStep(settings);
        int laneCount = horizontal ? settings.height : settings.width;
        int axisLength = horizontal ? settings.width : settings.height;

        if (axisLength < settings.minStraightCellsPerSegment || laneOffset >= laneCount)
        {
            return result;
        }

        System.Random random = new System.Random(settings.seed + (horizontal ? 2203 : 2819) + laneOffset * 97 + (headAtPositiveEdge ? 17 : 43));
        int lane = laneOffset;
        int pairIndex = 0;
        while (lane < laneCount)
        {
            int nextLane = lane + laneStep;
            if (nextLane < laneCount)
            {
                bool reversePair = (pairIndex & 1) != 0;
                bool positiveEdge = reversePair ? !headAtPositiveEdge : headAtPositiveEdge;
                AddPairedLaneChunkSnakes(result, settings, random, horizontal, lane, nextLane, axisLength, positiveEdge, pairIndex);
                lane += laneStep * 2;
                pairIndex++;
                continue;
            }

            bool reverseLane = (pairIndex & 1) != 0;
            AddSingleLaneChunkSnakes(result, settings, random, horizontal, lane, axisLength, reverseLane);
            lane += laneStep;
        }

        return result;
    }

    private static void AddPairedLaneChunkSnakes(Result result, Settings settings, System.Random random, bool horizontal, int firstLane, int secondLane, int axisLength, bool headAtPositiveEdge, int pairIndex)
    {
        int laneStep = GetEvenLaneStep(settings);
        int maxUCellRun = GetLastOddAtMost(Mathf.Min(axisLength, (settings.maxSnakeLength - laneStep + 1) / 2));
        int maxLCellRun = GetLastOddAtMost(Mathf.Min(axisLength, settings.maxSnakeLength - laneStep));
        int maxStraightRun = GetLastOddAtMost(Mathf.Min(axisLength, settings.maxSnakeLength));

        int cursor = 0;
        int chunkIndex = 0;
        while (cursor + settings.minStraightCellsPerSegment <= axisLength)
        {
            int remaining = axisLength - cursor;
            int maxRun;
            ChunkShape shape = PickLaneChunkShape(result, settings, random, maxStraightRun, maxLCellRun, maxUCellRun, out maxRun);

            int runLength = GetLastOddAtMost(Mathf.Min(remaining, maxRun));
            if (runLength < settings.minStraightCellsPerSegment)
            {
                break;
            }

            int start = cursor;
            int end = cursor + runLength - 1;
            bool positiveEdge = ((chunkIndex + pairIndex) & 1) == 0 ? headAtPositiveEdge : !headAtPositiveEdge;
            SnakeSaveData snake = BuildLaneChunkSnake(settings, random, horizontal, firstLane, secondLane, start, end, positiveEdge, shape);
            AddGeneratedSnake(result, snake, shape != ChunkShape.Straight, ToShapeKind(shape));

            if (shape == ChunkShape.Straight && firstLane != secondLane)
            {
                SnakeSaveData companionSnake = BuildLaneChunkSnake(settings, random, horizontal, secondLane, secondLane, start, end, !positiveEdge, ChunkShape.Straight);
                AddGeneratedSnake(result, companionSnake, false);
            }

            cursor = end + settings.minDistanceBetweenSnakes;
            chunkIndex++;
        }
    }

    private static ChunkShape PickLaneChunkShape(Result result, Settings settings, System.Random random, int maxStraightRun, int maxLCellRun, int maxUCellRun, out int maxRun)
    {
        bool canStraight = maxStraightRun >= settings.minStraightCellsPerSegment;
        bool canL = maxLCellRun >= settings.minStraightCellsPerSegment;
        bool canU = maxUCellRun >= settings.minStraightCellsPerSegment;

        if (canStraight && result.straightShapeCount == 0)
        {
            maxRun = maxStraightRun;
            return ChunkShape.Straight;
        }

        if (canL && result.lShapeCount == 0 && result.placedArrowCount > 0)
        {
            maxRun = maxLCellRun;
            return ChunkShape.L;
        }

        if (canU && result.uShapeCount == 0 && result.placedArrowCount > 1)
        {
            maxRun = maxUCellRun;
            return ChunkShape.U;
        }

        int straightWeight = canStraight ? 38 : 0;
        int lWeight = canL ? 34 : 0;
        int uWeight = canU ? 24 : 0;

        if (result.placedArrowCount > 0)
        {
            int uPercent = result.uShapeCount * 100 / result.placedArrowCount;
            if (uPercent >= 35 && (canStraight || canL))
            {
                uWeight = Mathf.Min(uWeight, 4);
            }

            int straightPercent = result.straightShapeCount * 100 / result.placedArrowCount;
            if (straightPercent < 25)
            {
                straightWeight += 18;
            }

            int lPercent = result.lShapeCount * 100 / result.placedArrowCount;
            if (lPercent < 25)
            {
                lWeight += 14;
            }
        }

        int totalWeight = straightWeight + lWeight + uWeight;
        if (totalWeight <= 0)
        {
            maxRun = 0;
            return ChunkShape.Straight;
        }

        int roll = random.Next(0, totalWeight);
        if (roll < straightWeight)
        {
            maxRun = maxStraightRun;
            return ChunkShape.Straight;
        }

        roll -= straightWeight;
        if (roll < lWeight)
        {
            maxRun = maxLCellRun;
            return ChunkShape.L;
        }

        maxRun = maxUCellRun;
        return ChunkShape.U;
    }

    private static void AddSingleLaneChunkSnakes(Result result, Settings settings, System.Random random, bool horizontal, int lane, int axisLength, bool reverseLane)
    {
        int maxRun = GetLastOddAtMost(Mathf.Min(axisLength, settings.maxSnakeLength));
        if (maxRun < settings.minStraightCellsPerSegment)
        {
            return;
        }

        int cursor = 0;
        int chunkIndex = 0;
        while (cursor + settings.minStraightCellsPerSegment <= axisLength)
        {
            int remaining = axisLength - cursor;
            int runLength = GetLastOddAtMost(Mathf.Min(remaining, maxRun));
            if (runLength < settings.minStraightCellsPerSegment)
            {
                break;
            }

            int start = cursor;
            int end = cursor + runLength - 1;
            bool headAtPositiveEdge = ((chunkIndex & 1) == 0) != reverseLane;
            SnakeSaveData snake = BuildLaneChunkSnake(settings, random, horizontal, lane, lane, start, end, headAtPositiveEdge, ChunkShape.Straight);
            AddGeneratedSnake(result, snake, false);

            cursor = end + settings.minDistanceBetweenSnakes;
            chunkIndex++;
        }
    }

    private enum ChunkShape
    {
        Straight,
        L,
        U
    }

    private static SnakeSaveData BuildLaneChunkSnake(Settings settings, System.Random random, bool horizontal, int firstLane, int secondLane, int start, int end, bool headAtPositiveEdge, ChunkShape shape)
    {
        SnakeSaveData snake = new SnakeSaveData();
        snake.arrowColor = PickColor(settings.colorPalette, random);
        snake.segmentPositions = new List<Vector2Int>();

        if (horizontal)
        {
            snake.direction = headAtPositiveEdge ? ArrowDir.Right : ArrowDir.Left;
            bool moveNegative = headAtPositiveEdge;
            AddHorizontalRun(settings, snake.segmentPositions, start, end, firstLane, moveNegative, true);

            if (shape == ChunkShape.L || shape == ChunkShape.U)
            {
                int connectorX = moveNegative ? start : end;
                AddVerticalConnector(settings, snake.segmentPositions, connectorX, firstLane, secondLane);

                if (shape == ChunkShape.U)
                {
                    AddHorizontalRun(settings, snake.segmentPositions, start, end, secondLane, !moveNegative, false);
                }
            }
        }
        else
        {
            snake.direction = headAtPositiveEdge ? ArrowDir.Up : ArrowDir.Down;
            bool moveNegative = headAtPositiveEdge;
            AddVerticalRun(settings, snake.segmentPositions, firstLane, start, end, moveNegative, true);

            if (shape == ChunkShape.L || shape == ChunkShape.U)
            {
                int connectorY = moveNegative ? start : end;
                AddHorizontalConnector(settings, snake.segmentPositions, firstLane, secondLane, connectorY);

                if (shape == ChunkShape.U)
                {
                    AddVerticalRun(settings, snake.segmentPositions, secondLane, start, end, !moveNegative, false);
                }
            }
        }

        return snake;
    }

    private static void AddGeneratedSnake(Result result, SnakeSaveData snake, bool isBent)
    {
        AddGeneratedSnake(result, snake, isBent, isBent ? ShapeKind.RandomBent : ShapeKind.Straight);
    }

    private static void AddGeneratedSnake(Result result, SnakeSaveData snake, bool isBent, ShapeKind shape)
    {
        if (snake.segmentPositions == null || snake.segmentPositions.Count == 0)
        {
            return;
        }

        result.snakes.Add(snake);
        result.placedArrowCount++;
        if (isBent)
        {
            result.bentArrowCount++;
        }
        UpdateResultDiversity(result, snake.direction, shape);
        result.occupiedCellCount += snake.segmentPositions.Count;
    }

    private static ShapeKind ToShapeKind(ChunkShape shape)
    {
        switch (shape)
        {
            case ChunkShape.L:
                return ShapeKind.L;
            case ChunkShape.U:
                return ShapeKind.U;
            default:
                return ShapeKind.Straight;
        }
    }

    private static SnakeSaveData BuildPairedLaneSnake(Settings settings, System.Random random, bool horizontal, int firstLane, int secondLane, int laneLength, bool headAtPositiveEdge)
    {
        SnakeSaveData snake = new SnakeSaveData();
        snake.arrowColor = PickColor(settings.colorPalette, random);
        snake.segmentPositions = new List<Vector2Int>();

        if (horizontal)
        {
            snake.direction = headAtPositiveEdge ? ArrowDir.Right : ArrowDir.Left;

            int startX = headAtPositiveEdge ? settings.width - laneLength : 0;
            int endX = startX + laneLength - 1;
            bool moveNegative = headAtPositiveEdge;
            AddHorizontalRun(settings, snake.segmentPositions, startX, endX, firstLane, moveNegative, true);

            int connectorX = moveNegative ? startX : endX;
            AddVerticalConnector(settings, snake.segmentPositions, connectorX, firstLane, secondLane);
            AddHorizontalRun(settings, snake.segmentPositions, startX, endX, secondLane, !moveNegative, false);
        }
        else
        {
            snake.direction = headAtPositiveEdge ? ArrowDir.Up : ArrowDir.Down;

            int startY = headAtPositiveEdge ? settings.height - laneLength : 0;
            int endY = startY + laneLength - 1;
            bool moveNegative = headAtPositiveEdge;
            AddVerticalRun(settings, snake.segmentPositions, firstLane, startY, endY, moveNegative, true);

            int connectorY = moveNegative ? startY : endY;
            AddHorizontalConnector(settings, snake.segmentPositions, firstLane, secondLane, connectorY);
            AddVerticalRun(settings, snake.segmentPositions, secondLane, startY, endY, !moveNegative, false);
        }

        return snake;
    }

    private static Result GenerateSerpentineFillResult(Settings settings, bool horizontal, int laneOffset, bool headAtPositiveEdge)
    {
        Result result = new Result();
        result.placementAreaCellCount = GetPlacementAreaCellCount(settings);

        int laneStep = GetEvenLaneStep(settings);
        int laneCount = horizontal ? settings.height : settings.width;
        int laneLength = horizontal ? settings.width : settings.height;
        laneLength = GetLastOddAtMost(laneLength);

        if (laneLength < settings.minStraightCellsPerSegment || laneOffset >= laneCount)
        {
            return result;
        }

        int usedLaneCount = 0;
        for (int lane = laneOffset; lane < laneCount; lane += laneStep)
        {
            usedLaneCount++;
        }

        if (usedLaneCount < 2)
        {
            return result;
        }

        System.Random random = new System.Random(settings.seed + (horizontal ? 1409 : 1877) + laneOffset * 97 + (headAtPositiveEdge ? 17 : 43));
        SnakeSaveData snake = BuildSerpentineSnake(settings, random, horizontal, laneOffset, laneLength, headAtPositiveEdge);
        if (snake.segmentPositions.Count == 0)
        {
            return result;
        }

        result.snakes.Add(snake);
        result.placedArrowCount = 1;
        result.bentArrowCount = 1;
        result.occupiedCellCount = snake.segmentPositions.Count;
        return result;
    }

    private static SnakeSaveData BuildSerpentineSnake(Settings settings, System.Random random, bool horizontal, int laneOffset, int laneLength, bool headAtPositiveEdge)
    {
        SnakeSaveData snake = new SnakeSaveData();
        snake.arrowColor = PickColor(settings.colorPalette, random);
        snake.segmentPositions = new List<Vector2Int>();

        int laneStep = GetEvenLaneStep(settings);
        if (horizontal)
        {
            snake.direction = headAtPositiveEdge ? ArrowDir.Right : ArrowDir.Left;

            int startX = headAtPositiveEdge ? settings.width - laneLength : 0;
            int endX = startX + laneLength - 1;
            int y = laneOffset;
            bool moveNegative = headAtPositiveEdge;
            AddHorizontalRun(settings, snake.segmentPositions, startX, endX, y, moveNegative, true);

            int x = moveNegative ? startX : endX;
            for (int nextY = y + laneStep; nextY < settings.height; nextY += laneStep)
            {
                AddVerticalConnector(settings, snake.segmentPositions, x, y, nextY);
                y = nextY;
                moveNegative = !moveNegative;
                AddHorizontalRun(settings, snake.segmentPositions, startX, endX, y, moveNegative, false);
                x = moveNegative ? startX : endX;
            }
        }
        else
        {
            snake.direction = headAtPositiveEdge ? ArrowDir.Up : ArrowDir.Down;

            int startY = headAtPositiveEdge ? settings.height - laneLength : 0;
            int endY = startY + laneLength - 1;
            int x = laneOffset;
            bool moveNegative = headAtPositiveEdge;
            AddVerticalRun(settings, snake.segmentPositions, x, startY, endY, moveNegative, true);

            int y = moveNegative ? startY : endY;
            for (int nextX = x + laneStep; nextX < settings.width; nextX += laneStep)
            {
                AddHorizontalConnector(settings, snake.segmentPositions, x, nextX, y);
                x = nextX;
                moveNegative = !moveNegative;
                AddVerticalRun(settings, snake.segmentPositions, x, startY, endY, moveNegative, false);
                y = moveNegative ? startY : endY;
            }
        }

        return snake;
    }

    private static void AddHorizontalRun(Settings settings, List<Vector2Int> positions, int startX, int endX, int y, bool moveNegative, bool includeStart)
    {
        if (moveNegative)
        {
            int from = includeStart ? endX : endX - 1;
            for (int x = from; x >= startX; x--)
            {
                AddGridPosition(settings, positions, x, y);
            }
        }
        else
        {
            int from = includeStart ? startX : startX + 1;
            for (int x = from; x <= endX; x++)
            {
                AddGridPosition(settings, positions, x, y);
            }
        }
    }

    private static void AddVerticalRun(Settings settings, List<Vector2Int> positions, int x, int startY, int endY, bool moveNegative, bool includeStart)
    {
        if (moveNegative)
        {
            int from = includeStart ? endY : endY - 1;
            for (int y = from; y >= startY; y--)
            {
                AddGridPosition(settings, positions, x, y);
            }
        }
        else
        {
            int from = includeStart ? startY : startY + 1;
            for (int y = from; y <= endY; y++)
            {
                AddGridPosition(settings, positions, x, y);
            }
        }
    }

    private static void AddVerticalConnector(Settings settings, List<Vector2Int> positions, int x, int fromY, int toY)
    {
        int step = toY > fromY ? 1 : -1;
        for (int y = fromY + step; y != toY + step; y += step)
        {
            AddGridPosition(settings, positions, x, y);
        }
    }

    private static void AddHorizontalConnector(Settings settings, List<Vector2Int> positions, int fromX, int toX, int y)
    {
        int step = toX > fromX ? 1 : -1;
        for (int x = fromX + step; x != toX + step; x += step)
        {
            AddGridPosition(settings, positions, x, y);
        }
    }

    private static void AddGridPosition(Settings settings, List<Vector2Int> positions, int x, int y)
    {
        positions.Add(new Vector2Int(settings.originX + x, settings.originY + y));
    }

    private static SnakeSaveData BuildStripedSnake(Settings settings, System.Random random, bool horizontal, int lane, int length, bool reverseLane)
    {
        SnakeSaveData snake = new SnakeSaveData();
        snake.arrowColor = PickColor(settings.colorPalette, random);
        snake.segmentPositions = new List<Vector2Int>(length);

        if (horizontal)
        {
            int startX = reverseLane ? settings.width - length : 0;
            int y = lane;

            if (reverseLane)
            {
                snake.direction = ArrowDir.Left;
                for (int i = 0; i < length; i++)
                {
                    int x = startX + i;
                    snake.segmentPositions.Add(new Vector2Int(settings.originX + x, settings.originY + y));
                }
            }
            else
            {
                snake.direction = ArrowDir.Right;
                for (int i = length - 1; i >= 0; i--)
                {
                    int x = startX + i;
                    snake.segmentPositions.Add(new Vector2Int(settings.originX + x, settings.originY + y));
                }
            }
        }
        else
        {
            int startY = reverseLane ? settings.height - length : 0;
            int x = lane;

            if (reverseLane)
            {
                snake.direction = ArrowDir.Down;
                for (int i = 0; i < length; i++)
                {
                    int y = startY + i;
                    snake.segmentPositions.Add(new Vector2Int(settings.originX + x, settings.originY + y));
                }
            }
            else
            {
                snake.direction = ArrowDir.Up;
                for (int i = length - 1; i >= 0; i--)
                {
                    int y = startY + i;
                    snake.segmentPositions.Add(new Vector2Int(settings.originX + x, settings.originY + y));
                }
            }
        }

        return snake;
    }

    private static Result GenerateSingleFillResult(Settings settings, int seed)
    {
        Result result = new Result();
        result.placementAreaCellCount = GetPlacementAreaCellCount(settings);

        byte[] occupied = new byte[settings.width * settings.height];
        bool[] horizontalLineLanes = new bool[settings.height];
        bool[] verticalLineLanes = new bool[settings.width];
        int[] candidateCells = new int[settings.maxSnakeLength];
        int[] bestCandidateCells = new int[settings.maxSnakeLength];
        int[] pathVisited = new int[settings.width * settings.height];
        int pathVisitGeneration = 1;
        int[] freeCellBuffer = new int[settings.width * settings.height];
        System.Random random = new System.Random(seed);

        int maxPlacements = GetPlacementAreaCellCount(settings);
        for (int i = 0; i < maxPlacements; i++)
        {
            Candidate candidate;
            if (!TryFindBestFillCandidate(settings, occupied, result.occupiedCellCount, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, freeCellBuffer, random, candidateCells, bestCandidateCells, out candidate))
            {
                break;
            }

            SnakeSaveData snake = BuildSnake(candidate, settings, random, bestCandidateCells);
            MarkSnake(occupied, bestCandidateCells, candidate.length);
            MarkParallelLineLanes(settings, bestCandidateCells, candidate.length, horizontalLineLanes, verticalLineLanes);
            result.snakes.Add(snake);
            result.placedArrowCount++;
            bool hasBentPath = HasBentPath(settings.width, bestCandidateCells, candidate.length);
            if (hasBentPath)
            {
                result.bentArrowCount++;
            }
            UpdateResultDiversity(result, snake.direction, hasBentPath ? ShapeKind.RandomBent : ShapeKind.Straight);
            result.occupiedCellCount += candidate.length;
        }

        Reverse(result.snakes);
        return result;
    }

    private static bool IsBetterFillResult(Result current, Result best)
    {
        int fillTolerance = Mathf.Max(2, best.placementAreaCellCount / 50);
        int currentDiversityScore = GetShapeDiversityScore(current);
        int bestDiversityScore = GetShapeDiversityScore(best);

        if (current.occupiedCellCount > best.occupiedCellCount + fillTolerance)
        {
            return true;
        }

        if (current.occupiedCellCount + fillTolerance < best.occupiedCellCount)
        {
            return false;
        }

        if (currentDiversityScore != bestDiversityScore)
        {
            return currentDiversityScore > bestDiversityScore;
        }

        if (current.directionTypeCount != best.directionTypeCount)
        {
            return current.directionTypeCount > best.directionTypeCount;
        }

        if (current.shapeTypeCount != best.shapeTypeCount)
        {
            return current.shapeTypeCount > best.shapeTypeCount;
        }

        if (current.bentArrowCount != best.bentArrowCount)
        {
            return current.bentArrowCount > best.bentArrowCount;
        }

        if (current.occupiedCellCount != best.occupiedCellCount)
        {
            return current.occupiedCellCount > best.occupiedCellCount;
        }

        if (current.bentArrowCount != best.bentArrowCount)
        {
            return current.bentArrowCount > best.bentArrowCount;
        }

        return current.placedArrowCount > best.placedArrowCount;
    }

    private static int GetShapeDiversityScore(Result result)
    {
        if (result == null || result.placedArrowCount <= 0)
        {
            return 0;
        }

        int dominantShapeCount = Mathf.Max(
            result.straightShapeCount,
            Mathf.Max(result.lShapeCount, Mathf.Max(result.uShapeCount, Mathf.Max(result.zigzagShapeCount, result.randomBentShapeCount))));
        int dominantLimit = Mathf.CeilToInt(result.placedArrowCount * 0.45f);
        int dominantPenalty = Mathf.Max(0, dominantShapeCount - dominantLimit);

        int balancedLaneShapes = Mathf.Min(result.straightShapeCount, Mathf.Min(result.lShapeCount, result.uShapeCount));
        int bentPercent = result.bentArrowCount * 100 / result.placedArrowCount;
        int straightPercent = result.straightShapeCount * 100 / result.placedArrowCount;
        int bentBonus = Mathf.Min(result.bentArrowCount, result.placedArrowCount / 2) * 180;
        int straightDominancePenalty = straightPercent > 55 ? (straightPercent - 55) * 80 : 0;

        return result.shapeTypeCount * 1000
            + result.directionTypeCount * 200
            + balancedLaneShapes * 140
            + result.zigzagShapeCount * 80
            + result.randomBentShapeCount * 60
            + bentBonus
            + (bentPercent >= 35 ? 1200 : 0)
            - dominantPenalty * 160
            - straightDominancePenalty;
    }

    private static bool TryFindBestFillCandidate(Settings settings, byte[] occupied, int occupiedCount, bool[] horizontalLineLanes, bool[] verticalLineLanes, int[] pathVisited, ref int pathVisitGeneration, int[] freeCellBuffer, System.Random random, int[] candidateCells, int[] bestCandidateCells, out Candidate bestCandidate)
    {
        bool hasBest = false;
        int bestScore = int.MinValue;
        bestCandidate = default;

        int searchAttempts = GetPerPlacementSearchAttempts(settings);
        for (int i = 0; i < searchAttempts; i++)
        {
            Candidate candidate;
            candidate.x = random.Next(0, settings.width);
            candidate.y = random.Next(0, settings.height);
            candidate.dir = (ArrowDir)random.Next(0, 4);
            candidate.length = GetRandomOddLength(settings, random);

            if (!TryBuildPath(settings, occupied, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, random, candidate, candidateCells))
            {
                continue;
            }

            int score = GetFillCandidateScore(settings, candidate, occupied, occupiedCount, candidateCells);
            if (!hasBest || score > bestScore)
            {
                hasBest = true;
                bestScore = score;
                bestCandidate = candidate;
                CopyCells(candidateCells, bestCandidateCells, candidate.length);

            }
        }

        if (hasBest)
        {
            return true;
        }

        return TryFindAnyBestFillCandidate(settings, occupied, occupiedCount, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, freeCellBuffer, random, candidateCells, bestCandidateCells, out bestCandidate);
    }

    private static bool TryFindBestDeferredFillCandidate(Settings settings, Result result, byte[] occupied, int occupiedCount, bool[] horizontalLineLanes, bool[] verticalLineLanes, int[] pathVisited, ref int pathVisitGeneration, int[] freeCellBuffer, System.Random random, int[] candidateCells, int[] bestCandidateCells, out Candidate bestCandidate, out int bestInsertionIndex)
    {
        bool hasBest = false;
        int bestScore = int.MinValue;
        bestCandidate = default;
        bestInsertionIndex = 0;

        int searchAttempts = GetPerPlacementSearchAttempts(settings);
        for (int i = 0; i < searchAttempts; i++)
        {
            Candidate candidate;
            candidate.x = random.Next(0, settings.width);
            candidate.y = random.Next(0, settings.height);
            candidate.dir = (ArrowDir)random.Next(0, 4);
            candidate.length = GetRandomOddLength(settings, random);

            int insertionIndex;
            if (!TryBuildDeferredFillCandidate(settings, result, occupied, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, random, candidate, candidateCells, out insertionIndex))
            {
                continue;
            }

            int score = GetFillCandidateScore(settings, candidate, occupied, occupiedCount, candidateCells);
            if (!hasBest || score > bestScore)
            {
                hasBest = true;
                bestScore = score;
                bestCandidate = candidate;
                bestInsertionIndex = insertionIndex;
                CopyCells(candidateCells, bestCandidateCells, candidate.length);
            }
        }

        if (hasBest)
        {
            return true;
        }

        return TryFindAnyBestDeferredFillCandidate(settings, result, occupied, occupiedCount, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, freeCellBuffer, random, candidateCells, bestCandidateCells, out bestCandidate, out bestInsertionIndex);
    }

    private static bool TryFindAnyBestDeferredFillCandidate(Settings settings, Result result, byte[] occupied, int occupiedCount, bool[] horizontalLineLanes, bool[] verticalLineLanes, int[] pathVisited, ref int pathVisitGeneration, int[] freeCellBuffer, System.Random random, int[] candidateCells, int[] bestCandidateCells, out Candidate bestCandidate, out int bestInsertionIndex)
    {
        bool hasBest = false;
        int bestScore = int.MinValue;
        bestCandidate = default;
        bestInsertionIndex = 0;

        int dirOffset = random.Next(0, 4);
        int freeCount = FillFreeCells(settings, occupied, freeCellBuffer);
        if (freeCount <= 0)
        {
            return false;
        }

        ShuffleArrayPrefix(freeCellBuffer, freeCount, random);
        int checkedCandidates = 0;
        int oddLengthCount = Mathf.Max(1, GetOddLengthCount(settings));
        int exhaustiveCandidateCount = freeCount * 4 * oddLengthCount;
        int maxCheckedCandidates = Mathf.Clamp(exhaustiveCandidateCount, 512, GetFallbackCandidateScanLimit(settings) * 2);

        for (int length = GetLastOddAtMost(settings.maxSnakeLength); length >= settings.minSnakeLength; length -= 2)
        {
            for (int cellPass = 0; cellPass < freeCount; cellPass++)
            {
                int cellIndex = freeCellBuffer[cellPass];
                int x = cellIndex % settings.width;
                int y = cellIndex / settings.width;

                for (int dirPass = 0; dirPass < 4; dirPass++)
                {
                    Candidate candidate;
                    candidate.x = x;
                    candidate.y = y;
                    candidate.length = length;
                    candidate.dir = (ArrowDir)((dirOffset + dirPass) & 3);
                    checkedCandidates++;

                    int insertionIndex;
                    if (!TryBuildDeferredFillCandidate(settings, result, occupied, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, random, candidate, candidateCells, out insertionIndex))
                    {
                        if (checkedCandidates >= maxCheckedCandidates)
                        {
                            return hasBest;
                        }

                        continue;
                    }

                    int score = GetFillCandidateScore(settings, candidate, occupied, occupiedCount, candidateCells);
                    if (!hasBest || score > bestScore)
                    {
                        hasBest = true;
                        bestScore = score;
                        bestCandidate = candidate;
                        bestInsertionIndex = insertionIndex;
                        CopyCells(candidateCells, bestCandidateCells, candidate.length);
                    }
                }
            }

            if (hasBest)
            {
                return true;
            }

            if (checkedCandidates >= maxCheckedCandidates)
            {
                return false;
            }
        }

        return false;
    }

    private static bool TryFindSmallestDeferredFillCandidate(Settings settings, Result result, byte[] occupied, int occupiedCount, bool[] horizontalLineLanes, bool[] verticalLineLanes, int[] pathVisited, ref int pathVisitGeneration, int[] freeCellBuffer, System.Random random, int[] candidateCells, int[] bestCandidateCells, out Candidate bestCandidate, out int bestInsertionIndex)
    {
        bool hasBest = false;
        int bestScore = int.MinValue;
        bestCandidate = default;
        bestInsertionIndex = 0;

        int freeCount = FillFreeCells(settings, occupied, freeCellBuffer);
        if (freeCount <= 0)
        {
            return false;
        }

        ShuffleArrayPrefix(freeCellBuffer, freeCount, random);
        int dirOffset = random.Next(0, 4);
        int checkedCandidates = 0;
        int maxCheckedCandidates = Mathf.Clamp(freeCount * 4 * Mathf.Max(1, GetOddLengthCount(settings)), 512, GetFallbackCandidateScanLimit(settings) * 2);

        for (int length = GetFirstOddAtLeast(settings.minSnakeLength); length <= settings.maxSnakeLength; length += 2)
        {
            for (int cellPass = 0; cellPass < freeCount; cellPass++)
            {
                int cellIndex = freeCellBuffer[cellPass];
                int x = cellIndex % settings.width;
                int y = cellIndex / settings.width;

                for (int dirPass = 0; dirPass < 4; dirPass++)
                {
                    Candidate candidate;
                    candidate.x = x;
                    candidate.y = y;
                    candidate.length = length;
                    candidate.dir = (ArrowDir)((dirOffset + dirPass) & 3);
                    checkedCandidates++;

                    int insertionIndex;
                    if (!TryBuildDeferredFillCandidate(settings, result, occupied, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, random, candidate, candidateCells, out insertionIndex))
                    {
                        if (checkedCandidates >= maxCheckedCandidates)
                        {
                            return hasBest;
                        }

                        continue;
                    }

                    int score = GetLateFillCandidateScore(settings, candidate, occupied, occupiedCount, candidateCells);
                    if (!hasBest || score > bestScore)
                    {
                        hasBest = true;
                        bestScore = score;
                        bestCandidate = candidate;
                        bestInsertionIndex = insertionIndex;
                        CopyCells(candidateCells, bestCandidateCells, candidate.length);
                    }
                }
            }

            if (hasBest)
            {
                return true;
            }

            if (checkedCandidates >= maxCheckedCandidates)
            {
                return false;
            }
        }

        return false;
    }

    private static bool TryBuildDeferredFillCandidate(Settings settings, Result result, byte[] occupied, bool[] horizontalLineLanes, bool[] verticalLineLanes, int[] pathVisited, ref int pathVisitGeneration, System.Random random, Candidate candidate, int[] candidateCells, out int insertionIndex)
    {
        insertionIndex = 0;
        if (!TryBuildPath(settings, occupied, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, random, candidate, candidateCells, false))
        {
            return false;
        }

        return TryFindDeferredInsertionIndex(settings, result.snakes, candidateCells, candidate.length, candidate.dir, out insertionIndex);
    }

    private static bool TryFindDeferredInsertionIndex(Settings settings, List<SnakeSaveData> solvedSnakes, int[] candidateCells, int candidateLength, ArrowDir candidateDirection, out int insertionIndex)
    {
        int lowerBound = 0;
        int upperBound = solvedSnakes.Count;
        int headCell = candidateCells[0];
        int headX = headCell % settings.width;
        int headY = headCell / settings.width;
        int candidateDx;
        int candidateDy;
        GetStep(candidateDirection, out candidateDx, out candidateDy);

        for (int i = 0; i < solvedSnakes.Count; i++)
        {
            SnakeSaveData snake = solvedSnakes[i];
            if (SnakeHasCellOnExitRay(settings, snake, headX, headY, candidateDx, candidateDy))
            {
                lowerBound = Mathf.Max(lowerBound, i + 1);
            }

            if (CandidateBlocksSnakeExit(settings, candidateCells, candidateLength, snake))
            {
                upperBound = Mathf.Min(upperBound, i);
            }

            if (lowerBound > upperBound)
            {
                insertionIndex = 0;
                return false;
            }
        }

        insertionIndex = lowerBound;
        return true;
    }

    private static bool SnakeHasCellOnExitRay(Settings settings, SnakeSaveData snake, int headX, int headY, int dx, int dy)
    {
        if (snake.segmentPositions == null)
        {
            return false;
        }

        for (int i = 0; i < snake.segmentPositions.Count; i++)
        {
            Vector2Int position = snake.segmentPositions[i];
            int x = position.x - settings.originX;
            int y = position.y - settings.originY;
            if (IsOnExitRay(headX, headY, dx, dy, x, y))
            {
                return true;
            }
        }

        return false;
    }

    private static bool CandidateBlocksSnakeExit(Settings settings, int[] candidateCells, int candidateLength, SnakeSaveData snake)
    {
        if (snake.segmentPositions == null || snake.segmentPositions.Count == 0)
        {
            return false;
        }

        Vector2Int head = snake.segmentPositions[0];
        int headX = head.x - settings.originX;
        int headY = head.y - settings.originY;
        int dx;
        int dy;
        GetStep(snake.direction, out dx, out dy);

        for (int i = 0; i < candidateLength; i++)
        {
            int cell = candidateCells[i];
            int x = cell % settings.width;
            int y = cell / settings.width;
            if (IsOnExitRay(headX, headY, dx, dy, x, y))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryFindAnyBestFillCandidate(Settings settings, byte[] occupied, int occupiedCount, bool[] horizontalLineLanes, bool[] verticalLineLanes, int[] pathVisited, ref int pathVisitGeneration, int[] freeCellBuffer, System.Random random, int[] candidateCells, int[] bestCandidateCells, out Candidate bestCandidate)
    {
        bool hasBest = false;
        int bestScore = int.MinValue;
        bestCandidate = default;

        int dirOffset = random.Next(0, 4);
        int freeCount = FillFreeCells(settings, occupied, freeCellBuffer);
        if (freeCount <= 0)
        {
            return false;
        }

        ShuffleArrayPrefix(freeCellBuffer, freeCount, random);
        int checkedCandidates = 0;
        int oddLengthCount = Mathf.Max(1, GetOddLengthCount(settings));
        int exhaustiveCandidateCount = freeCount * 4 * oddLengthCount;
        int maxCheckedCandidates = Mathf.Clamp(exhaustiveCandidateCount, 256, GetFallbackCandidateScanLimit(settings));

        for (int length = GetLastOddAtMost(settings.maxSnakeLength); length >= settings.minSnakeLength; length -= 2)
        {
            for (int cellPass = 0; cellPass < freeCount; cellPass++)
            {
                int cellIndex = freeCellBuffer[cellPass];
                int x = cellIndex % settings.width;
                int y = cellIndex / settings.width;

                for (int dirPass = 0; dirPass < 4; dirPass++)
                {
                    Candidate candidate;
                    candidate.x = x;
                    candidate.y = y;
                    candidate.length = length;
                    candidate.dir = (ArrowDir)((dirOffset + dirPass) & 3);
                    checkedCandidates++;

                    if (!TryBuildPath(settings, occupied, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, random, candidate, candidateCells))
                    {
                        if (checkedCandidates >= maxCheckedCandidates)
                        {
                            return hasBest;
                        }

                        continue;
                    }

                    int score = GetFillCandidateScore(settings, candidate, occupied, occupiedCount, candidateCells);
                    if (!hasBest || score > bestScore)
                    {
                        hasBest = true;
                        bestScore = score;
                        bestCandidate = candidate;
                        CopyCells(candidateCells, bestCandidateCells, candidate.length);
                    }
                }
            }

            if (hasBest)
            {
                return true;
            }

            if (checkedCandidates >= maxCheckedCandidates)
            {
                return false;
            }
        }

        return false;
    }

    private static int GetPerPlacementSearchAttempts(Settings settings)
    {
        int area = GetPlacementAreaCellCount(settings);
        int cap = area <= 180 ? 512 : 384;
        return Mathf.Clamp(settings.fillSearchAttempts, 32, cap);
    }

    private static int GetFallbackCandidateScanLimit(Settings settings)
    {
        int area = GetPlacementAreaCellCount(settings);
        if (area <= 180)
        {
            return 4096;
        }

        if (area <= 420)
        {
            return 8192;
        }

        return 12288;
    }

    private static int GetFillCandidateScore(Settings settings, Candidate candidate, byte[] occupied, int occupiedCount, int[] candidateCells)
    {
        int score = candidate.length * 1000;
        int exactSpacingContacts = 0;

        for (int i = 0; i < candidate.length; i++)
        {
            int cell = candidateCells[i];
            int x = cell % settings.width;
            int y = cell / settings.width;

            if (x == 0 || x == settings.width - 1)
            {
                score += 2;
            }

            if (y == 0 || y == settings.height - 1)
            {
                score += 2;
            }

            exactSpacingContacts += CountOccupiedAtExactSpacing(settings, occupied, x, y);
        }

        score += exactSpacingContacts * 160;
        if (occupiedCount > 0)
        {
            score += exactSpacingContacts > 0 ? 2200 : -4800;
        }

        return score;
    }

    private static int GetLateFillCandidateScore(Settings settings, Candidate candidate, byte[] occupied, int occupiedCount, int[] candidateCells)
    {
        int score = candidate.length * 120;
        int exactSpacingContacts = 0;
        int edgeContacts = 0;

        for (int i = 0; i < candidate.length; i++)
        {
            int cell = candidateCells[i];
            int x = cell % settings.width;
            int y = cell / settings.width;

            if (x == 0 || x == settings.width - 1)
            {
                edgeContacts++;
            }

            if (y == 0 || y == settings.height - 1)
            {
                edgeContacts++;
            }

            exactSpacingContacts += CountOccupiedAtExactSpacing(settings, occupied, x, y);
        }

        score += exactSpacingContacts * 420;
        score += edgeContacts * 60;
        if (occupiedCount > 0 && exactSpacingContacts <= 0)
        {
            score -= 2400;
        }

        return score;
    }

    private static int CountOccupiedAtExactSpacing(Settings settings, byte[] occupied, int x, int y)
    {
        int count = 0;
        int distance = settings.minDistanceBetweenSnakes;

        for (int offsetY = -distance; offsetY <= distance; offsetY++)
        {
            int offsetXRange = distance - Mathf.Abs(offsetY);
            if (offsetXRange == 0)
            {
                if (IsOccupied(settings, occupied, x, y + offsetY))
                {
                    count++;
                }

                continue;
            }

            if (IsOccupied(settings, occupied, x - offsetXRange, y + offsetY))
            {
                count++;
            }

            if (IsOccupied(settings, occupied, x + offsetXRange, y + offsetY))
            {
                count++;
            }
        }

        return count;
    }

    private static bool IsOccupied(Settings settings, byte[] occupied, int x, int y)
    {
        return IsInside(settings.width, settings.height, x, y)
            && occupied[ToIndex(settings.width, x, y)] != 0;
    }

    private static void CopyCells(int[] source, int[] destination, int length)
    {
        for (int i = 0; i < length; i++)
        {
            destination[i] = source[i];
        }
    }

    private static bool TryFindRandomCandidate(Settings settings, byte[] occupied, bool[] horizontalLineLanes, bool[] verticalLineLanes, int[] pathVisited, ref int pathVisitGeneration, System.Random random, int[] candidateCells, out Candidate candidate)
    {
        for (int i = 0; i < settings.maxAttemptsPerArrow; i++)
        {
            candidate.x = random.Next(0, settings.width);
            candidate.y = random.Next(0, settings.height);
            candidate.dir = (ArrowDir)random.Next(0, 4);
            candidate.length = GetRandomOddLength(settings, random);

            if (TryBuildPath(settings, occupied, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, random, candidate, candidateCells))
            {
                return true;
            }
        }

        candidate = default;
        return false;
    }

    private static bool TryFindAnyCandidate(Settings settings, byte[] occupied, bool[] horizontalLineLanes, bool[] verticalLineLanes, int[] pathVisited, ref int pathVisitGeneration, System.Random random, int[] candidateCells, out Candidate candidate)
    {
        int dirOffset = random.Next(0, 4);
        int lengthRange = GetOddLengthCount(settings);
        int lengthOffset = random.Next(0, lengthRange);
        int cellOffset = random.Next(0, settings.width * settings.height);

        for (int cellPass = 0; cellPass < settings.width * settings.height; cellPass++)
        {
            int cellIndex = (cellOffset + cellPass) % (settings.width * settings.height);
            int x = cellIndex % settings.width;
            int y = cellIndex / settings.width;

            for (int lengthPass = 0; lengthPass < lengthRange; lengthPass++)
            {
                int length = GetOddLengthByOffset(settings, (lengthOffset + lengthPass) % lengthRange);

                for (int dirPass = 0; dirPass < 4; dirPass++)
                {
                    candidate.x = x;
                    candidate.y = y;
                    candidate.length = length;
                    candidate.dir = (ArrowDir)((dirOffset + dirPass) & 3);

                    if (TryBuildPath(settings, occupied, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, random, candidate, candidateCells))
                    {
                        return true;
                    }
                }
            }
        }

        candidate = default;
        return false;
    }

    private static bool TryBuildPath(Settings settings, byte[] occupied, bool[] horizontalLineLanes, bool[] verticalLineLanes, int[] pathVisited, ref int pathVisitGeneration, System.Random random, Candidate candidate, int[] candidateCells)
    {
        return TryBuildPath(settings, occupied, horizontalLineLanes, verticalLineLanes, pathVisited, ref pathVisitGeneration, random, candidate, candidateCells, true);
    }

    private static bool TryBuildPath(Settings settings, byte[] occupied, bool[] horizontalLineLanes, bool[] verticalLineLanes, int[] pathVisited, ref int pathVisitGeneration, System.Random random, Candidate candidate, int[] candidateCells, bool requireExitPathClear)
    {
        int exitDx;
        int exitDy;
        GetStep(candidate.dir, out exitDx, out exitDy);

        if (!IsPlacementCell(settings, candidate.x, candidate.y))
        {
            return false;
        }

        int headIndex = ToIndex(settings.width, candidate.x, candidate.y);
        if (occupied[headIndex] != 0)
        {
            return false;
        }

        if (IsTooCloseToOccupied(settings, occupied, candidate.x, candidate.y))
        {
            return false;
        }

        if (requireExitPathClear && !IsExitPathClear(settings, occupied, candidate.x, candidate.y, exitDx, exitDy))
        {
            return false;
        }

        for (int attempt = 0; attempt < settings.bodyAttemptsPerCandidate; attempt++)
        {
            int visitGeneration = NextVisitGeneration(pathVisited, ref pathVisitGeneration);
            if (TryBuildPathOnce(settings, occupied, pathVisited, visitGeneration, random, candidate, exitDx, exitDy, candidateCells))
            {
                if (HasValidParallelLineLaneParity(settings, candidateCells, candidate.length, horizontalLineLanes, verticalLineLanes))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryBuildPathOnce(Settings settings, byte[] occupied, int[] pathVisited, int visitGeneration, System.Random random, Candidate candidate, int exitDx, int exitDy, int[] candidateCells)
    {
        candidateCells[0] = ToIndex(settings.width, candidate.x, candidate.y);
        pathVisited[candidateCells[0]] = visitGeneration;

        if (candidate.length == 1)
        {
            return true;
        }

        int bodyDx = -exitDx;
        int bodyDy = -exitDy;
        int x = candidate.x + bodyDx;
        int y = candidate.y + bodyDy;

        if (!CanUseBodyCell(settings, occupied, pathVisited, visitGeneration, candidateCells, 1, candidate.x, candidate.y, exitDx, exitDy, x, y))
        {
            return false;
        }

        candidateCells[1] = ToIndex(settings.width, x, y);
        pathVisited[candidateCells[1]] = visitGeneration;
        int currentRunCells = 2;

        for (int i = 2; i < candidate.length; i++)
        {
            int straightDir = GetDirectionIndex(bodyDx, bodyDy);
            int preferredDir = straightDir;
            bool turnsAllowed = settings.allowBentSnakes && IsValidStraightRun(settings, currentRunCells);

            if (turnsAllowed && random.Next(0, 100) < settings.turnChancePercent)
            {
                preferredDir = random.Next(0, 2) == 0
                    ? GetLeftTurnDirectionIndex(bodyDx, bodyDy)
                    : GetRightTurnDirectionIndex(bodyDx, bodyDy);
            }

            bool foundNext = false;
            int alternateTurnFirst = random.Next(0, 2);
            for (int order = 0; order < 4; order++)
            {
                int dirIndex = GetBodyDirectionByOrder(preferredDir, bodyDx, bodyDy, alternateTurnFirst, order);
                bool isTurn = dirIndex != straightDir;
                if (!turnsAllowed && isTurn)
                {
                    continue;
                }

                if (isTurn && candidate.length - i - 1 < settings.minStraightCellsPerSegment - 2)
                {
                    continue;
                }

                int stepDx;
                int stepDy;
                GetStep(dirIndex, out stepDx, out stepDy);

                int nextX = x + stepDx;
                int nextY = y + stepDy;
                if (!CanUseBodyCell(settings, occupied, pathVisited, visitGeneration, candidateCells, i, candidate.x, candidate.y, exitDx, exitDy, nextX, nextY))
                {
                    continue;
                }

                candidateCells[i] = ToIndex(settings.width, nextX, nextY);
                pathVisited[candidateCells[i]] = visitGeneration;
                x = nextX;
                y = nextY;
                bodyDx = stepDx;
                bodyDy = stepDy;
                currentRunCells = isTurn ? 2 : currentRunCells + 1;
                foundNext = true;
                break;
            }

            if (!foundNext)
            {
                return false;
            }
        }

        return HasValidStraightRuns(settings, candidateCells, candidate.length);
    }

    private static int GetBodyDirectionByOrder(int preferredDir, int currentDx, int currentDy, int alternateTurnFirst, int order)
    {
        if (order == 0)
        {
            return preferredDir;
        }

        int straight = GetDirectionIndex(currentDx, currentDy);
        int left = GetLeftTurnDirectionIndex(currentDx, currentDy);
        int right = GetRightTurnDirectionIndex(currentDx, currentDy);
        int back = GetOppositeDirectionIndex(straight);

        if (preferredDir != straight)
        {
            if (order == 1) return straight;
            if (order == 2) return preferredDir == left ? right : left;
            return back;
        }

        if (order == 1) return alternateTurnFirst == 0 ? left : right;
        if (order == 2) return alternateTurnFirst == 0 ? right : left;
        return back;
    }

    private static bool CanUseBodyCell(Settings settings, byte[] occupied, int[] pathVisited, int visitGeneration, int[] candidateCells, int usedCount, int headX, int headY, int exitDx, int exitDy, int x, int y)
    {
        if (!IsPlacementCell(settings, x, y))
        {
            return false;
        }

        int index = ToIndex(settings.width, x, y);
        if (occupied[index] != 0)
        {
            return false;
        }

        if (pathVisited[index] == visitGeneration)
        {
            return false;
        }

        if (IsTooCloseToOccupied(settings, occupied, x, y))
        {
            return false;
        }

        if (IsOnExitRay(headX, headY, exitDx, exitDy, x, y))
        {
            return false;
        }

        for (int i = 0; i < usedCount; i++)
        {
            if (i < usedCount - 1 && GetManhattanDistance(settings.width, candidateCells[i], index) < settings.minDistanceBetweenSnakes)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsTooCloseToOccupied(Settings settings, byte[] occupied, int x, int y)
    {
        int exclusiveDistance = settings.minDistanceBetweenSnakes;
        if (exclusiveDistance <= 1)
        {
            return false;
        }

        CellOffset[] offsets = GetSpacingOffsets(exclusiveDistance);
        for (int i = 0; i < offsets.Length; i++)
        {
            int checkX = x + offsets[i].x;
            int checkY = y + offsets[i].y;
            if (!IsInside(settings.width, settings.height, checkX, checkY))
            {
                continue;
            }

            if (occupied[ToIndex(settings.width, checkX, checkY)] != 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasValidStraightRuns(Settings settings, int[] candidateCells, int length)
    {
        if (!settings.allowBentSnakes || length < 3)
        {
            return true;
        }

        int prevDx;
        int prevDy;
        GetCellStep(settings.width, candidateCells[0], candidateCells[1], out prevDx, out prevDy);

        int runCells = 2;
        bool hasTurn = false;
        for (int i = 2; i < length; i++)
        {
            int dx;
            int dy;
            GetCellStep(settings.width, candidateCells[i - 1], candidateCells[i], out dx, out dy);

            if (dx == prevDx && dy == prevDy)
            {
                runCells++;
                continue;
            }

            hasTurn = true;
            if (!IsValidStraightRun(settings, runCells))
            {
                return false;
            }

            prevDx = dx;
            prevDy = dy;
            runCells = 2;
        }

        return !hasTurn
            ? IsOdd(runCells)
            : IsValidStraightRun(settings, runCells);
    }

    private static bool IsExitPathClear(Settings settings, byte[] occupied, int headX, int headY, int dx, int dy)
    {
        int x = headX + dx;
        int y = headY + dy;

        while (IsInside(settings.width, settings.height, x, y))
        {
            if (occupied[ToIndex(settings.width, x, y)] != 0)
            {
                return false;
            }

            x += dx;
            y += dy;
        }

        return true;
    }

    private static SnakeSaveData BuildSnake(Candidate candidate, Settings settings, System.Random random, int[] candidateCells)
    {
        return BuildSnake(candidate.dir, settings, random, candidateCells, candidate.length);
    }

    private static SnakeSaveData BuildSnake(ArrowDir direction, Settings settings, System.Random random, int[] candidateCells, int length)
    {
        SnakeSaveData snake = new SnakeSaveData();
        snake.direction = direction;
        snake.arrowColor = PickColor(settings.colorPalette, random);
        snake.segmentPositions = new List<Vector2Int>(length);

        for (int i = 0; i < length; i++)
        {
            int cell = candidateCells[i];
            int gridX = cell % settings.width;
            int gridY = cell / settings.width;
            snake.segmentPositions.Add(new Vector2Int(settings.originX + gridX, settings.originY + gridY));
        }

        return snake;
    }

    private static void MarkSnake(byte[] occupied, int[] candidateCells, int length)
    {
        for (int i = 0; i < length; i++)
        {
            occupied[candidateCells[i]] = 1;
        }
    }

    private static bool IsOnExitRay(int headX, int headY, int dx, int dy, int x, int y)
    {
        if (dx != 0)
        {
            return y == headY && (x - headX) * dx > 0;
        }

        return x == headX && (y - headY) * dy > 0;
    }

    private static Color PickColor(Color[] palette, System.Random random)
    {
        Color[] usablePalette = palette != null && palette.Length > 0 ? palette : DefaultPalette;
        return usablePalette[random.Next(0, usablePalette.Length)];
    }

    private static void GetStep(ArrowDir dir, out int dx, out int dy)
    {
        switch (dir)
        {
            case ArrowDir.Up:
                dx = 0;
                dy = 1;
                return;
            case ArrowDir.Down:
                dx = 0;
                dy = -1;
                return;
            case ArrowDir.Left:
                dx = -1;
                dy = 0;
                return;
            default:
                dx = 1;
                dy = 0;
                return;
        }
    }

    private static void GetStep(int dirIndex, out int dx, out int dy)
    {
        switch (dirIndex & 3)
        {
            case 0:
                dx = 0;
                dy = 1;
                return;
            case 1:
                dx = 0;
                dy = -1;
                return;
            case 2:
                dx = -1;
                dy = 0;
                return;
            default:
                dx = 1;
                dy = 0;
                return;
        }
    }

    private static int GetDirectionIndex(int dx, int dy)
    {
        if (dy > 0) return 0;
        if (dy < 0) return 1;
        if (dx < 0) return 2;
        return 3;
    }

    private static int GetLeftTurnDirectionIndex(int dx, int dy)
    {
        if (dy > 0) return 2;
        if (dy < 0) return 3;
        if (dx < 0) return 1;
        return 0;
    }

    private static int GetRightTurnDirectionIndex(int dx, int dy)
    {
        if (dy > 0) return 3;
        if (dy < 0) return 2;
        if (dx < 0) return 0;
        return 1;
    }

    private static int GetOppositeDirectionIndex(int dirIndex)
    {
        switch (dirIndex & 3)
        {
            case 0: return 1;
            case 1: return 0;
            case 2: return 3;
            default: return 2;
        }
    }

    private static int GetManhattanDistance(int width, int aIndex, int bIndex)
    {
        int ax = aIndex % width;
        int ay = aIndex / width;
        int bx = bIndex % width;
        int by = bIndex / width;
        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }

    private static CellOffset[] GetSpacingOffsets(int exclusiveDistance)
    {
        CellOffset[] cachedOffsets;
        if (SpacingOffsetCache.TryGetValue(exclusiveDistance, out cachedOffsets))
        {
            return cachedOffsets;
        }

        List<CellOffset> offsets = new List<CellOffset>();
        for (int offsetY = -exclusiveDistance + 1; offsetY < exclusiveDistance; offsetY++)
        {
            for (int offsetX = -exclusiveDistance + 1; offsetX < exclusiveDistance; offsetX++)
            {
                if (Mathf.Abs(offsetX) + Mathf.Abs(offsetY) < exclusiveDistance)
                {
                    offsets.Add(new CellOffset(offsetX, offsetY));
                }
            }
        }

        cachedOffsets = offsets.ToArray();
        SpacingOffsetCache[exclusiveDistance] = cachedOffsets;
        return cachedOffsets;
    }

    private static int GetRandomOddLength(Settings settings, System.Random random)
    {
        return GetWeightedOddInRange(settings.minSnakeLength, settings.maxSnakeLength, random);
    }

    private static int GetRandomOddInRange(int minimum, int maximum, System.Random random)
    {
        int first = GetFirstOddAtLeast(minimum);
        int last = GetLastOddAtMost(maximum);
        if (last < first)
        {
            return first;
        }

        int count = ((last - first) / 2) + 1;
        return first + random.Next(0, count) * 2;
    }

    private static int GetWeightedOddInRange(int minimum, int maximum, System.Random random)
    {
        int first = GetFirstOddAtLeast(minimum);
        int last = GetLastOddAtMost(maximum);
        if (last < first)
        {
            return first;
        }

        int roll = random.Next(0, 100);
        int cappedLast = last;
        if (roll < 58)
        {
            cappedLast = Mathf.Min(last, GetLastOddAtMost(first + 12));
        }
        else if (roll < 86)
        {
            cappedLast = Mathf.Min(last, GetLastOddAtMost(first + 28));
        }
        else if (roll < 97)
        {
            cappedLast = Mathf.Min(last, GetLastOddAtMost(first + 56));
        }

        return GetRandomOddInRange(first, cappedLast, random);
    }

    private static int GetOddLengthCount(Settings settings)
    {
        int first = GetFirstOddAtLeast(settings.minSnakeLength);
        int last = GetLastOddAtMost(settings.maxSnakeLength);
        if (last < first)
        {
            return 1;
        }

        return ((last - first) / 2) + 1;
    }

    private static int GetOddLengthByOffset(Settings settings, int offset)
    {
        int first = GetFirstOddAtLeast(settings.minSnakeLength);
        return first + offset * 2;
    }

    private static int GetEvenLaneStep(Settings settings)
    {
        int step = Mathf.Max(2, settings.minDistanceBetweenSnakes);
        return (step & 1) == 0 ? step : step + 1;
    }

    private static int MakeOddAtLeast(int value, int minimum)
    {
        value = Mathf.Max(minimum, value);
        return IsOdd(value) ? value : value + 1;
    }

    private static int GetFirstOddAtLeast(int value)
    {
        value = Mathf.Max(1, value);
        return IsOdd(value) ? value : value + 1;
    }

    private static int GetLastOddAtMost(int value)
    {
        value = Mathf.Max(1, value);
        return IsOdd(value) ? value : value - 1;
    }

    private static bool IsValidStraightRun(Settings settings, int cellCount)
    {
        return cellCount >= settings.minStraightCellsPerSegment && IsOdd(cellCount);
    }

    private static bool IsOdd(int value)
    {
        return (value & 1) != 0;
    }

    private static void UpdateResultDiversity(Result result, ArrowDir direction, ShapeKind shape)
    {
        result.directionMask |= 1 << ((int)direction & 3);
        result.shapeMask |= 1 << (int)shape;
        IncrementShapeCount(result, shape);
        result.directionTypeCount = CountBits(result.directionMask);
        result.shapeTypeCount = CountBits(result.shapeMask);
    }

    private static void IncrementShapeCount(Result result, ShapeKind shape)
    {
        switch (shape)
        {
            case ShapeKind.Straight:
                result.straightShapeCount++;
                break;
            case ShapeKind.L:
                result.lShapeCount++;
                break;
            case ShapeKind.U:
                result.uShapeCount++;
                break;
            case ShapeKind.Zigzag:
                result.zigzagShapeCount++;
                break;
            case ShapeKind.RandomBent:
                result.randomBentShapeCount++;
                break;
        }
    }

    private static int CountBits(int value)
    {
        int count = 0;
        while (value != 0)
        {
            count += value & 1;
            value >>= 1;
        }

        return count;
    }

    private static bool HasBentPath(int width, int[] cells, int length)
    {
        if (length < 3)
        {
            return false;
        }

        int prevDx;
        int prevDy;
        GetCellStep(width, cells[0], cells[1], out prevDx, out prevDy);

        for (int i = 2; i < length; i++)
        {
            int dx;
            int dy;
            GetCellStep(width, cells[i - 1], cells[i], out dx, out dy);
            if (dx != prevDx || dy != prevDy)
            {
                return true;
            }
        }

        return false;
    }

    private static void GetCellStep(int width, int fromIndex, int toIndex, out int dx, out int dy)
    {
        int fromX = fromIndex % width;
        int fromY = fromIndex / width;
        int toX = toIndex % width;
        int toY = toIndex / width;
        dx = toX - fromX;
        dy = toY - fromY;
    }

    private static int GetPlacementAreaCellCount(Settings settings)
    {
        if (!HasPlacementMask(settings))
        {
            return settings.width * settings.height;
        }

        int count = 0;
        for (int i = 0; i < settings.placementMask.Length; i++)
        {
            if (settings.placementMask[i])
            {
                count++;
            }
        }

        return count;
    }

    private static bool HasPlacementMask(Settings settings)
    {
        return settings.placementMask != null
            && settings.placementMask.Length == settings.width * settings.height;
    }

    private static bool IsPlacementCell(Settings settings, int x, int y)
    {
        if (!IsInside(settings.width, settings.height, x, y))
        {
            return false;
        }

        return !HasPlacementMask(settings) || settings.placementMask[ToIndex(settings.width, x, y)];
    }

    private static bool IsPlacementCell(Settings settings, int cell)
    {
        if (cell < 0 || cell >= settings.width * settings.height)
        {
            return false;
        }

        return !HasPlacementMask(settings) || settings.placementMask[cell];
    }

    private static bool IsInside(int width, int height, int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private static int ToIndex(int width, int x, int y)
    {
        return y * width + x;
    }

    private static void Reverse(List<SnakeSaveData> snakes)
    {
        int left = 0;
        int right = snakes.Count - 1;
        while (left < right)
        {
            SnakeSaveData temp = snakes[left];
            snakes[left] = snakes[right];
            snakes[right] = temp;
            left++;
            right--;
        }
    }
}

