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
        public int originX;
        public int originY;
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

        if (settings.fillAvailableArea)
        {
            return GenerateBestFillResult(settings);
        }

        Result result = new Result();
        result.placementAreaCellCount = settings.width * settings.height;

        byte[] occupied = new byte[settings.width * settings.height];
        int[] candidateCells = new int[settings.maxSnakeLength];
        System.Random random = new System.Random(settings.seed);

        int failedAttempts = 0;
        for (int i = 0; i < settings.targetArrowCount; i++)
        {
            Candidate candidate;
            int[] selectedCells = candidateCells;
            if (!TryFindRandomCandidate(settings, occupied, random, candidateCells, out candidate))
            {
                if (!TryFindAnyCandidate(settings, occupied, random, candidateCells, out candidate))
                {
                    failedAttempts++;
                    break;
                }
            }

            SnakeSaveData snake = BuildSnake(candidate, settings, random, selectedCells);
            MarkSnake(occupied, selectedCells, candidate.length);
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

        int maxCells = settings.width * settings.height;
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

        for (int attempt = 0; attempt < settings.fillLayoutAttempts; attempt++)
        {
            int seed = settings.seed + attempt * 7919;
            Result current = GenerateDfsFillResult(settings, seed);
            if (bestResult == null || IsBetterFillResult(current, bestResult))
            {
                bestResult = current;
            }

            if (bestResult.occupiedCellCount >= settings.width * settings.height)
            {
                break;
            }
        }

        if (bestResult == null)
        {
            bestResult = new Result();
            bestResult.placementAreaCellCount = settings.width * settings.height;
        }

        bestResult.success = bestResult.placedArrowCount > 0;
        bestResult.message = bestResult.success
            ? "Generated the densest valid fill found for the selected area."
            : "No valid candidate could be placed with the current fill settings.";

        return bestResult;
    }

    private static Result GenerateDfsFillResult(Settings settings, int seed)
    {
        Result result = new Result();
        result.placementAreaCellCount = settings.width * settings.height;

        int totalCells = settings.width * settings.height;
        bool[] occupied = new bool[totalCells];
        List<int> zoneCells = new List<int>(totalCells);
        for (int i = 0; i < totalCells; i++)
        {
            zoneCells.Add(i);
        }

        System.Random random = new System.Random(seed);
        bool addedInPass = true;
        int passGuard = 0;

        while (addedInPass && passGuard < totalCells)
        {
            addedInPass = false;
            passGuard++;

            List<int> freeCells = GetFreeCells(zoneCells, occupied);
            if (freeCells.Count < settings.minSnakeLength)
            {
                break;
            }

            ShuffleList(freeCells, random);

            for (int i = 0; i < freeCells.Count; i++)
            {
                int startCell = freeCells[i];
                int longestLength = GetLastOddAtMost(Mathf.Min(settings.maxSnakeLength, freeCells.Count));

                for (int targetLength = longestLength; targetLength >= settings.minSnakeLength; targetLength -= 2)
                {
                    List<int> bestPath = null;
                    int attempts = Mathf.Max(1, settings.bodyAttemptsPerCandidate);

                    for (int attempt = 0; attempt < attempts; attempt++)
                    {
                        List<int> path = TryCreateDfsSnakePath(settings, startCell, targetLength, occupied, random);
                        if (path == null)
                        {
                            continue;
                        }

                        if (IsDfsExitBlocked(settings, path, occupied))
                        {
                            continue;
                        }

                        bestPath = path;
                        break;
                    }

                    if (bestPath == null)
                    {
                        continue;
                    }

                    SnakeSaveData snake = BuildDfsSnake(settings, random, bestPath);
                    MarkDfsSnake(occupied, bestPath);
                    AddGeneratedSnake(result, snake, IsBentDfsPath(settings.width, bestPath), IsBentDfsPath(settings.width, bestPath) ? ShapeKind.RandomBent : ShapeKind.Straight);
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

    private static List<int> GetFreeCells(List<int> zoneCells, bool[] occupied)
    {
        List<int> freeCells = new List<int>();
        for (int i = 0; i < zoneCells.Count; i++)
        {
            int cell = zoneCells[i];
            if (!occupied[cell])
            {
                freeCells.Add(cell);
            }
        }

        return freeCells;
    }

    private static List<int> TryCreateDfsSnakePath(Settings settings, int startCell, int targetLength, bool[] occupied, System.Random random)
    {
        if (!CanUseDfsCell(settings, occupied, null, startCell))
        {
            return null;
        }

        List<int> path = new List<int>(targetLength);
        bool[] pathVisited = new bool[settings.width * settings.height];

        if (!DfsSnake(settings, startCell, targetLength, occupied, pathVisited, path, random))
        {
            return null;
        }

        path.Reverse();
        return HasValidDfsSelfSpacing(settings, path) && HasValidDfsStraightRuns(settings, path) ? path : null;
    }

    private static bool DfsSnake(Settings settings, int currentCell, int targetLength, bool[] occupied, bool[] pathVisited, List<int> path, System.Random random)
    {
        path.Add(currentCell);
        pathVisited[currentCell] = true;

        if (path.Count == targetLength)
        {
            return true;
        }

        int[] directionOrder = { 0, 1, 2, 3 };
        Shuffle(directionOrder, random);

        int currentX = currentCell % settings.width;
        int currentY = currentCell / settings.width;

        for (int i = 0; i < directionOrder.Length; i++)
        {
            int dx;
            int dy;
            GetDfsDirectionStep(directionOrder[i], out dx, out dy);

            int nextX = currentX + dx;
            int nextY = currentY + dy;
            if (!IsInside(settings.width, settings.height, nextX, nextY))
            {
                continue;
            }

            int nextCell = ToIndex(settings.width, nextX, nextY);
            if (pathVisited[nextCell])
            {
                continue;
            }

            if (!CanUseDfsCell(settings, occupied, path, nextCell))
            {
                continue;
            }

            if (DfsSnake(settings, nextCell, targetLength, occupied, pathVisited, path, random))
            {
                return true;
            }
        }

        path.RemoveAt(path.Count - 1);
        pathVisited[currentCell] = false;
        return false;
    }

    private static bool IsDfsExitBlocked(Settings settings, List<int> path, bool[] occupied)
    {
        if (path == null || path.Count < 2)
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

    private static bool CanUseDfsCell(Settings settings, bool[] occupied, List<int> currentPath, int cell)
    {
        if (occupied[cell])
        {
            return false;
        }

        int x = cell % settings.width;
        int y = cell / settings.width;
        if (IsTooCloseToOccupied(settings, occupied, x, y))
        {
            return false;
        }

        if (currentPath == null)
        {
            return true;
        }

        for (int i = 0; i < currentPath.Count; i++)
        {
            if (currentPath[i] == cell)
            {
                return false;
            }
        }

        if (IsTooCloseToCurrentDfsPath(settings, currentPath, cell))
        {
            return false;
        }

        return true;
    }

    private static bool IsTooCloseToCurrentDfsPath(Settings settings, List<int> currentPath, int cell)
    {
        int exclusiveDistance = settings.minDistanceBetweenSnakes;
        if (exclusiveDistance <= 1 || currentPath == null || currentPath.Count <= 1)
        {
            return false;
        }

        // The last cell is the direct parent in DFS and must stay adjacent to the new cell.
        for (int i = 0; i < currentPath.Count - 1; i++)
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

        for (int offsetY = -exclusiveDistance + 1; offsetY < exclusiveDistance; offsetY++)
        {
            for (int offsetX = -exclusiveDistance + 1; offsetX < exclusiveDistance; offsetX++)
            {
                int distance = Mathf.Abs(offsetX) + Mathf.Abs(offsetY);
                if (distance >= exclusiveDistance)
                {
                    continue;
                }

                int checkX = x + offsetX;
                int checkY = y + offsetY;
                if (!IsInside(settings.width, settings.height, checkX, checkY))
                {
                    continue;
                }

                if (occupied[ToIndex(settings.width, checkX, checkY)])
                {
                    return true;
                }
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

    private static void MarkDfsSnake(bool[] occupied, List<int> path)
    {
        for (int i = 0; i < path.Count; i++)
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

    private static Result GenerateBestStripedFillResult(Settings settings)
    {
        Result bestResult = null;
        int laneStep = GetEvenLaneStep(settings);

        for (int offset = 0; offset < laneStep; offset++)
        {
            Result horizontalPairedRight = GeneratePairedLaneFillResult(settings, true, offset, true);
            if (bestResult == null || IsBetterFillResult(horizontalPairedRight, bestResult))
            {
                bestResult = horizontalPairedRight;
            }

            Result horizontalPairedLeft = GeneratePairedLaneFillResult(settings, true, offset, false);
            if (bestResult == null || IsBetterFillResult(horizontalPairedLeft, bestResult))
            {
                bestResult = horizontalPairedLeft;
            }

            Result verticalPairedUp = GeneratePairedLaneFillResult(settings, false, offset, true);
            if (bestResult == null || IsBetterFillResult(verticalPairedUp, bestResult))
            {
                bestResult = verticalPairedUp;
            }

            Result verticalPairedDown = GeneratePairedLaneFillResult(settings, false, offset, false);
            if (bestResult == null || IsBetterFillResult(verticalPairedDown, bestResult))
            {
                bestResult = verticalPairedDown;
            }

            Result horizontal = GenerateStripedFillResult(settings, true, offset);
            if (bestResult == null || IsBetterFillResult(horizontal, bestResult))
            {
                bestResult = horizontal;
            }

            Result vertical = GenerateStripedFillResult(settings, false, offset);
            if (bestResult == null || IsBetterFillResult(vertical, bestResult))
            {
                bestResult = vertical;
            }
        }

        return bestResult;
    }

    private static Result GenerateStripedFillResult(Settings settings, bool horizontal, int laneOffset)
    {
        Result result = new Result();
        result.placementAreaCellCount = settings.width * settings.height;

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
        result.placementAreaCellCount = settings.width * settings.height;

        byte[] occupied = new byte[settings.width * settings.height];
        int[] candidateCells = new int[settings.maxSnakeLength];
        int[] bestCells = new int[settings.maxSnakeLength];
        System.Random random = new System.Random(seed);

        int maxPlacements = settings.width * settings.height;
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
                if (!TryBuildTemplatePath(settings, occupied, random, shape, x, y, dir, candidateCells, out length, out actualShape))
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
            AddGeneratedSnake(result, snake, bestShape != ShapeKind.Straight, bestShape);
        }

        FillRemainderWithBestCandidates(settings, occupied, random, candidateCells, bestCells, result);
        Reverse(result.snakes);
        return result;
    }

    private static ShapeKind PickTemplateShape(System.Random random)
    {
        int roll = random.Next(0, 100);
        if (roll < 30) return ShapeKind.Straight;
        if (roll < 60) return ShapeKind.L;
        if (roll < 80) return ShapeKind.U;
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
            score += 400;
        }

        score += random.Next(0, 250);
        return score;
    }

    private static bool TryBuildTemplatePath(Settings settings, byte[] occupied, System.Random random, ShapeKind requestedShape, int headX, int headY, ArrowDir dir, int[] candidateCells, out int length, out ShapeKind actualShape)
    {
        length = 0;
        actualShape = requestedShape;

        int exitDx;
        int exitDy;
        GetStep(dir, out exitDx, out exitDy);

        if (!IsInside(settings.width, settings.height, headX, headY))
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
        int used = 1;
        int bodyDx = -exitDx;
        int bodyDy = -exitDy;

        int maxLength = settings.maxSnakeLength;
        int minRun = settings.minStraightCellsPerSegment;

        if (requestedShape == ShapeKind.Straight)
        {
            int runLength = GetRandomOddInRange(minRun, maxLength, random);
            if (!TryAppendRun(settings, occupied, candidateCells, ref used, headX, headY, exitDx, exitDy, bodyDx, bodyDy, runLength))
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

            int firstRun = GetRandomOddInRange(minRun, maxLength - minRun + 1, random);
            int secondMax = maxLength - firstRun + 1;
            int secondRun = GetRandomOddInRange(minRun, secondMax, random);
            int turnDir = random.Next(0, 2) == 0
                ? GetLeftTurnDirectionIndex(bodyDx, bodyDy)
                : GetRightTurnDirectionIndex(bodyDx, bodyDy);

            if (!TryAppendRun(settings, occupied, candidateCells, ref used, headX, headY, exitDx, exitDy, bodyDx, bodyDy, firstRun))
            {
                return false;
            }

            int turnDx;
            int turnDy;
            GetStep(turnDir, out turnDx, out turnDy);
            if (!TryAppendRun(settings, occupied, candidateCells, ref used, headX, headY, exitDx, exitDy, turnDx, turnDy, secondRun))
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

            int firstRun = GetRandomOddInRange(minRun, maxLength - (minRun * 2 - 2), random);
            int remainingAfterFirst = maxLength - firstRun + 1;
            int secondRun = GetRandomOddInRange(minRun, remainingAfterFirst - minRun + 1, random);
            int thirdMax = maxLength - firstRun - secondRun + 2;
            int thirdRun = GetRandomOddInRange(minRun, thirdMax, random);

            int turnDir = random.Next(0, 2) == 0
                ? GetLeftTurnDirectionIndex(bodyDx, bodyDy)
                : GetRightTurnDirectionIndex(bodyDx, bodyDy);
            int turnDx;
            int turnDy;
            GetStep(turnDir, out turnDx, out turnDy);

            int thirdDx = requestedShape == ShapeKind.U ? -bodyDx : bodyDx;
            int thirdDy = requestedShape == ShapeKind.U ? -bodyDy : bodyDy;

            if (!TryAppendRun(settings, occupied, candidateCells, ref used, headX, headY, exitDx, exitDy, bodyDx, bodyDy, firstRun))
            {
                return false;
            }

            if (!TryAppendRun(settings, occupied, candidateCells, ref used, headX, headY, exitDx, exitDy, turnDx, turnDy, secondRun))
            {
                return false;
            }

            if (!TryAppendRun(settings, occupied, candidateCells, ref used, headX, headY, exitDx, exitDy, thirdDx, thirdDy, thirdRun))
            {
                return false;
            }
        }

        if (!HasValidStraightRuns(settings, candidateCells, used))
        {
            return false;
        }

        length = used;
        return length >= settings.minSnakeLength;
    }

    private static bool TryAppendRun(Settings settings, byte[] occupied, int[] candidateCells, ref int used, int headX, int headY, int exitDx, int exitDy, int dx, int dy, int runCells)
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

            if (!CanUseBodyCell(settings, occupied, candidateCells, used, headX, headY, exitDx, exitDy, x, y))
            {
                return false;
            }

            if (used >= candidateCells.Length)
            {
                return false;
            }

            candidateCells[used] = ToIndex(settings.width, x, y);
            used++;
        }

        return true;
    }

    private static void FillRemainderWithBestCandidates(Settings settings, byte[] occupied, System.Random random, int[] candidateCells, int[] bestCells, Result result)
    {
        int maxPlacements = settings.width * settings.height;
        for (int i = 0; i < maxPlacements; i++)
        {
            Candidate candidate;
            if (!TryFindBestFillCandidate(settings, occupied, random, candidateCells, bestCells, out candidate))
            {
                break;
            }

            SnakeSaveData snake = BuildSnake(candidate, settings, random, bestCells);
            MarkSnake(occupied, bestCells, candidate.length);

            bool hasBentPath = HasBentPath(settings.width, bestCells, candidate.length);
            AddGeneratedSnake(result, snake, hasBentPath, hasBentPath ? ShapeKind.RandomBent : ShapeKind.Straight);
        }
    }

    private static Result GeneratePairedLaneFillResult(Settings settings, bool horizontal, int laneOffset, bool headAtPositiveEdge)
    {
        Result result = new Result();
        result.placementAreaCellCount = settings.width * settings.height;

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
        result.placementAreaCellCount = settings.width * settings.height;

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
        result.placementAreaCellCount = settings.width * settings.height;

        byte[] occupied = new byte[settings.width * settings.height];
        int[] candidateCells = new int[settings.maxSnakeLength];
        int[] bestCandidateCells = new int[settings.maxSnakeLength];
        System.Random random = new System.Random(seed);

        int maxPlacements = settings.width * settings.height;
        for (int i = 0; i < maxPlacements; i++)
        {
            Candidate candidate;
            if (!TryFindBestFillCandidate(settings, occupied, random, candidateCells, bestCandidateCells, out candidate))
            {
                break;
            }

            SnakeSaveData snake = BuildSnake(candidate, settings, random, bestCandidateCells);
            MarkSnake(occupied, bestCandidateCells, candidate.length);
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
        int fillTolerance = Mathf.Max(1, best.occupiedCellCount / 100);
        if (current.occupiedCellCount > best.occupiedCellCount + fillTolerance)
        {
            return true;
        }

        if (current.occupiedCellCount + fillTolerance < best.occupiedCellCount)
        {
            return false;
        }

        int currentDiversityScore = GetShapeDiversityScore(current);
        int bestDiversityScore = GetShapeDiversityScore(best);
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
        return result.shapeTypeCount * 1000
            + result.directionTypeCount * 200
            + balancedLaneShapes * 140
            + result.zigzagShapeCount * 80
            + result.randomBentShapeCount * 60
            - dominantPenalty * 120;
    }

    private static bool TryFindBestFillCandidate(Settings settings, byte[] occupied, System.Random random, int[] candidateCells, int[] bestCandidateCells, out Candidate bestCandidate)
    {
        bool hasBest = false;
        int bestScore = int.MinValue;
        bestCandidate = default;

        for (int i = 0; i < settings.fillSearchAttempts; i++)
        {
            Candidate candidate;
            candidate.x = random.Next(0, settings.width);
            candidate.y = random.Next(0, settings.height);
            candidate.dir = (ArrowDir)random.Next(0, 4);
            candidate.length = GetRandomOddLength(settings, random);

            if (!TryBuildPath(settings, occupied, random, candidate, candidateCells))
            {
                continue;
            }

            int score = GetFillCandidateScore(settings, candidate, occupied, candidateCells);
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

        return TryFindAnyBestFillCandidate(settings, occupied, random, candidateCells, bestCandidateCells, out bestCandidate);
    }

    private static bool TryFindAnyBestFillCandidate(Settings settings, byte[] occupied, System.Random random, int[] candidateCells, int[] bestCandidateCells, out Candidate bestCandidate)
    {
        bool hasBest = false;
        int bestScore = int.MinValue;
        bestCandidate = default;

        int dirOffset = random.Next(0, 4);
        int cellOffset = random.Next(0, settings.width * settings.height);

        for (int length = GetLastOddAtMost(settings.maxSnakeLength); length >= settings.minSnakeLength; length -= 2)
        {
            for (int cellPass = 0; cellPass < settings.width * settings.height; cellPass++)
            {
                int cellIndex = (cellOffset + cellPass) % (settings.width * settings.height);
                int x = cellIndex % settings.width;
                int y = cellIndex / settings.width;

                for (int dirPass = 0; dirPass < 4; dirPass++)
                {
                    Candidate candidate;
                    candidate.x = x;
                    candidate.y = y;
                    candidate.length = length;
                    candidate.dir = (ArrowDir)((dirOffset + dirPass) & 3);

                    if (!TryBuildPath(settings, occupied, random, candidate, candidateCells))
                    {
                        continue;
                    }

                    int score = GetFillCandidateScore(settings, candidate, occupied, candidateCells);
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
        }

        return false;
    }

    private static int GetFillCandidateScore(Settings settings, Candidate candidate, byte[] occupied, int[] candidateCells)
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
        if (HasAnyOccupiedCell(occupied))
        {
            score += exactSpacingContacts > 0 ? 2200 : -4800;
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

    private static bool HasAnyOccupiedCell(byte[] occupied)
    {
        for (int i = 0; i < occupied.Length; i++)
        {
            if (occupied[i] != 0)
            {
                return true;
            }
        }

        return false;
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

    private static bool TryFindRandomCandidate(Settings settings, byte[] occupied, System.Random random, int[] candidateCells, out Candidate candidate)
    {
        for (int i = 0; i < settings.maxAttemptsPerArrow; i++)
        {
            candidate.x = random.Next(0, settings.width);
            candidate.y = random.Next(0, settings.height);
            candidate.dir = (ArrowDir)random.Next(0, 4);
            candidate.length = GetRandomOddLength(settings, random);

            if (TryBuildPath(settings, occupied, random, candidate, candidateCells))
            {
                return true;
            }
        }

        candidate = default;
        return false;
    }

    private static bool TryFindAnyCandidate(Settings settings, byte[] occupied, System.Random random, int[] candidateCells, out Candidate candidate)
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

                    if (TryBuildPath(settings, occupied, random, candidate, candidateCells))
                    {
                        return true;
                    }
                }
            }
        }

        candidate = default;
        return false;
    }

    private static bool TryBuildPath(Settings settings, byte[] occupied, System.Random random, Candidate candidate, int[] candidateCells)
    {
        int exitDx;
        int exitDy;
        GetStep(candidate.dir, out exitDx, out exitDy);

        if (!IsInside(settings.width, settings.height, candidate.x, candidate.y))
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

        if (!IsExitPathClear(settings, occupied, candidate.x, candidate.y, exitDx, exitDy))
        {
            return false;
        }

        for (int attempt = 0; attempt < settings.bodyAttemptsPerCandidate; attempt++)
        {
            if (TryBuildPathOnce(settings, occupied, random, candidate, exitDx, exitDy, candidateCells))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryBuildPathOnce(Settings settings, byte[] occupied, System.Random random, Candidate candidate, int exitDx, int exitDy, int[] candidateCells)
    {
        candidateCells[0] = ToIndex(settings.width, candidate.x, candidate.y);

        if (candidate.length == 1)
        {
            return true;
        }

        int bodyDx = -exitDx;
        int bodyDy = -exitDy;
        int x = candidate.x + bodyDx;
        int y = candidate.y + bodyDy;

        if (!CanUseBodyCell(settings, occupied, candidateCells, 1, candidate.x, candidate.y, exitDx, exitDy, x, y))
        {
            return false;
        }

        candidateCells[1] = ToIndex(settings.width, x, y);
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
                if (!CanUseBodyCell(settings, occupied, candidateCells, i, candidate.x, candidate.y, exitDx, exitDy, nextX, nextY))
                {
                    continue;
                }

                candidateCells[i] = ToIndex(settings.width, nextX, nextY);
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

    private static bool CanUseBodyCell(Settings settings, byte[] occupied, int[] candidateCells, int usedCount, int headX, int headY, int exitDx, int exitDy, int x, int y)
    {
        if (!IsInside(settings.width, settings.height, x, y))
        {
            return false;
        }

        int index = ToIndex(settings.width, x, y);
        if (occupied[index] != 0)
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
            if (candidateCells[i] == index)
            {
                return false;
            }

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

        for (int offsetY = -exclusiveDistance + 1; offsetY < exclusiveDistance; offsetY++)
        {
            for (int offsetX = -exclusiveDistance + 1; offsetX < exclusiveDistance; offsetX++)
            {
                int distance = Mathf.Abs(offsetX) + Mathf.Abs(offsetY);
                if (distance >= exclusiveDistance)
                {
                    continue;
                }

                int checkX = x + offsetX;
                int checkY = y + offsetY;
                if (!IsInside(settings.width, settings.height, checkX, checkY))
                {
                    continue;
                }

                if (occupied[ToIndex(settings.width, checkX, checkY)] != 0)
                {
                    return true;
                }
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

    private static int GetRandomOddLength(Settings settings, System.Random random)
    {
        int oddLengthCount = GetOddLengthCount(settings);
        return GetOddLengthByOffset(settings, random.Next(0, oddLengthCount));
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
