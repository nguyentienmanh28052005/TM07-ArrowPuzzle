using System.Collections.Generic;
using System.Text;
using UnityEngine;

public sealed class LevelEditorDeadlockResult
{
    public bool hasDeadlock;
    public string message = string.Empty;
    public readonly List<int> releaseOrder = new List<int>();
    public readonly List<string> stuckReasons = new List<string>();
}

public sealed class LevelEditorDeadlockStateBuilder
{
    public LevelEditorDeadlockState Build(LevelDataV2 level)
    {
        LevelEditorDeadlockState state = new LevelEditorDeadlockState();
        if (level == null)
        {
            return state;
        }

        foreach (ArrowEntityData arrow in LevelDataV2Queries.GetStandardArrows(level))
        {
            if (arrow.segmentPositions == null || arrow.segmentPositions.Count == 0) continue;
            AddDeadlockSnake(state, arrow.direction, arrow.color, arrow.segmentPositions, $"Arrow #{state.snakes.Count + 1}");
        }

        if (level.cells != null)
        {
            for (int i = 0; i < level.cells.Count; i++)
            {
                AddDeadlockCell(state, level.cells[i]);
            }
        }

        foreach (PortalPairInfo portal in LevelDataV2Queries.GetPortalPairs(level))
        {
            state.portals[portal.entrance.position] = new LevelEditorDeadlockPortalLink { exit = portal.exit.position, exitDir = portal.exitExitDirection };
            state.portals[portal.exit.position] = new LevelEditorDeadlockPortalLink { exit = portal.entrance.position, exitDir = portal.entranceExitDirection };
        }

        return state;
    }

    public LevelEditorDeadlockState Build(LevelEditorContext context)
    {
        LevelEditorDeadlockState state = new LevelEditorDeadlockState();
        if (context == null || context.levelContainer == null)
        {
            return state;
        }

        foreach (Transform child in context.levelContainer)
        {
            if (child == null) continue;
            if (context.currentSnakeObj != null && child.gameObject == context.currentSnakeObj) continue;
            if (context.currentSelectionGlowObj != null && child.gameObject == context.currentSelectionGlowObj) continue;

            EditorSnakeVisual snakeVisual = child.GetComponent<EditorSnakeVisual>();
            if (snakeVisual == null || snakeVisual.LogicNodes == null || snakeVisual.LogicNodes.Count == 0) continue;

            AddDeadlockSnake(state, snakeVisual.direction, snakeVisual.snakeColor, snakeVisual.LogicNodes, $"Snake #{state.snakes.Count + 1}");
        }

        if (context.currentDraftNodes != null && context.currentDraftNodes.Count > 0)
        {
            AddDeadlockSnake(state, context.currentDir, context.currentColor, context.currentDraftNodes, $"Draft Snake #{state.snakes.Count + 1}");
        }

        foreach (Transform child in context.levelContainer)
        {
            if (child == null) continue;
            if (context.currentSelectionGlowObj != null && child.gameObject == context.currentSelectionGlowObj) continue;

            Vector2Int childCell = new Vector2Int(Mathf.RoundToInt(child.position.x), Mathf.RoundToInt(child.position.y));

            if (child.TryGetComponent(out GridKeycard keycard))
            {
                state.keycards[childCell] = keycard.keyColor;
            }

            if (child.TryGetComponent(out GridLaserGate gate))
            {
                state.gates[childCell] = gate.gateColor;
            }

            if (child.TryGetComponent(out GridElectricButton button))
            {
                state.electricButtons[childCell] = button.buttonColor;
            }

            GridDeflector deflector = child.GetComponentInChildren<GridDeflector>();
            if (deflector != null)
            {
                Vector2Int deflectorCell = new Vector2Int(Mathf.RoundToInt(deflector.transform.position.x), Mathf.RoundToInt(deflector.transform.position.y));
                state.deflectors[deflectorCell] = deflector.direction;
            }

            if (child.TryGetComponent(out GridCountdownBlock countdownBlock))
            {
                state.countdownBlocks[childCell] = Mathf.Max(1, countdownBlock.count);
            }

            if (child.TryGetComponent(out GridStopBlock stopBlock))
            {
                state.stopBlocks[childCell] = Mathf.Max(1, stopBlock.count);
            }

            if (child.TryGetComponent(out GridTurnStateBlock turnStateBlock))
            {
                state.turnStateBlocks[childCell] = turnStateBlock.IsRed;
            }

            if (child.TryGetComponent(out GridBlackHole blackHole))
            {
                state.blackHoles[childCell] = blackHole.direction;
            }
        }

        if (context.currentDraftPortals != null)
        {
            for (int i = 0; i < context.currentDraftPortals.Count; i++)
            {
                PortalData portal = context.currentDraftPortals[i];
                state.portals[portal.entrance] = new LevelEditorDeadlockPortalLink { exit = portal.exit, exitDir = portal.exitDir };
                state.portals[portal.exit] = new LevelEditorDeadlockPortalLink { exit = portal.entrance, exitDir = portal.entranceDir };
            }
        }

        if (context.currentDraftElectricWalls != null)
        {
            for (int i = 0; i < context.currentDraftElectricWalls.Count; i++)
            {
                AddDeadlockElectricWall(state, context.currentDraftElectricWalls[i]);
            }
        }

        return state;
    }

    private static void AddDeadlockCell(LevelEditorDeadlockState state, CellEntityData cell)
    {
        if (cell == null) return;

        switch (cell.typeId)
        {
            case CellTypeIds.Keycard:
                state.keycards[cell.position] = cell.color;
                break;
            case CellTypeIds.Gate:
                state.gates[cell.position] = cell.color;
                break;
            case CellTypeIds.ElectricButton:
                state.electricButtons[cell.position] = cell.color;
                break;
            case CellTypeIds.Deflector:
                state.deflectors[cell.position] = cell.direction;
                break;
            case CellTypeIds.CountdownBlock:
                if (cell.payload is CountCellPayload countdownPayload) state.countdownBlocks[cell.position] = Mathf.Max(1, countdownPayload.count);
                break;
            case CellTypeIds.StopBlock:
                if (cell.payload is CountCellPayload stopPayload) state.stopBlocks[cell.position] = Mathf.Max(1, stopPayload.count);
                break;
            case CellTypeIds.TurnStateBlock:
                if (cell.payload is TurnStatePayload turnStatePayload) state.turnStateBlocks[cell.position] = turnStatePayload.startsRed;
                break;
            case CellTypeIds.BlackHole:
                state.blackHoles[cell.position] = cell.direction;
                break;
            case CellTypeIds.ElectricWall:
                if (cell.payload is ElectricWallPayload wallPayload)
                {
                    AddDeadlockElectricWall(state, new ElectricWallSaveData { start = wallPayload.start, end = wallPayload.end, color = cell.color });
                }
                break;
        }
    }

    private static void AddDeadlockSnake(LevelEditorDeadlockState state, ArrowDir direction, Color color, List<Vector2Int> cells, string label)
    {
        LevelEditorDeadlockSnake snake = new LevelEditorDeadlockSnake
        {
            index = state.snakes.Count,
            label = label,
            direction = direction,
            color = color
        };

        for (int i = 0; i < cells.Count; i++)
        {
            Vector2Int cell = cells[i];
            snake.cells.Add(cell);
            state.snakeByCell[cell] = snake.index;
        }

        state.snakes.Add(snake);
    }

    private static void AddDeadlockElectricWall(LevelEditorDeadlockState state, ElectricWallSaveData wallData)
    {
        if (!LevelEditorRuntimeHelpers.IsElectricWallAligned(wallData.start, wallData.end)) return;

        LevelEditorDeadlockElectricWall wall = new LevelEditorDeadlockElectricWall
        {
            index = state.electricWalls.Count,
            data = wallData,
            color = wallData.color
        };

        int stepX = wallData.start.x == wallData.end.x ? 0 : (wallData.start.x < wallData.end.x ? 1 : -1);
        int stepY = wallData.start.y == wallData.end.y ? 0 : (wallData.start.y < wallData.end.y ? 1 : -1);
        int length = Mathf.Max(Mathf.Abs(wallData.end.x - wallData.start.x), Mathf.Abs(wallData.end.y - wallData.start.y));

        for (int i = 0; i <= length; i++)
        {
            Vector2Int cell = new Vector2Int(wallData.start.x + stepX * i, wallData.start.y + stepY * i);
            wall.cells.Add(cell);

            if (!state.electricWallIdsByCell.TryGetValue(cell, out List<int> wallIds))
            {
                wallIds = new List<int>(1);
                state.electricWallIdsByCell[cell] = wallIds;
            }

            wallIds.Add(wall.index);
        }

        state.electricWalls.Add(wall);
    }
}

public sealed class LevelEditorDeadlockValidator
{
    public LevelEditorDeadlockResult Validate(LevelEditorDeadlockState state, int scanLimit)
    {
        LevelEditorDeadlockResult result = new LevelEditorDeadlockResult();
        if (state == null || state.snakes.Count == 0)
        {
            result.message = "Deadlock check skipped: no snakes in current editor level.";
            return result;
        }

        bool madeProgress = true;
        while (state.releasedCount < state.snakes.Count && madeProgress)
        {
            madeProgress = false;

            for (int i = 0; i < state.snakes.Count; i++)
            {
                LevelEditorDeadlockSnake snake = state.snakes[i];
                if (snake.released) continue;

                LevelEditorDeadlockPathResult pathResult = CheckSnakeExitPath(snake, state, scanLimit);
                snake.lastBlockedReason = pathResult.blockedReason;
                if (!pathResult.canExit) continue;

                ApplyDeadlockRelease(snake, pathResult, state);
                result.releaseOrder.Add(snake.index);
                madeProgress = true;
            }
        }

        bool solved = state.releasedCount == state.snakes.Count;
        if (solved)
        {
            result.message = $"No deadlock. Release order: {BuildReleaseOrderText(result.releaseOrder)}";
            return result;
        }

        result.hasDeadlock = true;
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Deadlock detected. Stuck snakes:");
        for (int i = 0; i < state.snakes.Count; i++)
        {
            LevelEditorDeadlockSnake snake = state.snakes[i];
            if (snake.released) continue;

            string reason = string.IsNullOrEmpty(snake.lastBlockedReason) ? "no exit path" : snake.lastBlockedReason;
            result.stuckReasons.Add($"{snake.label}: {reason}");
            builder.Append("- ");
            builder.Append(snake.label);
            builder.Append(": ");
            builder.AppendLine(reason);
        }

        result.message = builder.ToString().TrimEnd();
        return result;
    }

    private static LevelEditorDeadlockPathResult CheckSnakeExitPath(LevelEditorDeadlockSnake snake, LevelEditorDeadlockState state, int deadlockScanLimit)
    {
        LevelEditorDeadlockPathResult result = new LevelEditorDeadlockPathResult();
        if (snake.cells == null || snake.cells.Count == 0)
        {
            result.blockedReason = "empty snake";
            return result;
        }

        Vector2Int currentPos = snake.cells[0];
        ArrowDir currentDir = snake.direction;
        Vector2Int step = LevelEditorRuntimeHelpers.GetDirStep(currentDir);
        if (step == Vector2Int.zero)
        {
            result.blockedReason = "invalid direction";
            return result;
        }

        HashSet<Vector3Int> visitedStates = new HashSet<Vector3Int>();
        HashSet<Vector2Int> locallyOpenedGateCells = new HashSet<Vector2Int>();
        HashSet<int> locallyDisabledWallIds = new HashSet<int>();

        int scanLimit = Mathf.Max(16, deadlockScanLimit);
        for (int scan = 1; scan <= scanLimit; scan++)
        {
            Vector3Int stateKey = new Vector3Int(currentPos.x, currentPos.y, LevelEditorRuntimeHelpers.GetStepKey(step));
            if (!visitedStates.Add(stateKey))
            {
                result.canExit = true;
                return result;
            }

            Vector2Int checkPos = currentPos + step;

            if (state.snakeByCell.TryGetValue(checkPos, out int blockerSnakeIndex)
                && blockerSnakeIndex != snake.index
                && blockerSnakeIndex >= 0
                && blockerSnakeIndex < state.snakes.Count
                && !state.snakes[blockerSnakeIndex].released)
            {
                result.blockedReason = $"blocked by {state.snakes[blockerSnakeIndex].label} at {LevelEditorRuntimeHelpers.FormatCell(checkPos)}";
                return result;
            }

            if (state.countdownBlocks.TryGetValue(checkPos, out int countdown) && countdown > 0)
            {
                result.blockedReason = $"blocked by countdown block ({countdown}) at {LevelEditorRuntimeHelpers.FormatCell(checkPos)}";
                return result;
            }

            if (state.stopBlocks.TryGetValue(checkPos, out int stopCount) && stopCount > 0)
            {
                result.blockedReason = $"blocked by stop block ({stopCount}) at {LevelEditorRuntimeHelpers.FormatCell(checkPos)}";
                return result;
            }

            if (state.turnStateBlocks.TryGetValue(checkPos, out bool isRedTurnState) && isRedTurnState)
            {
                result.blockedReason = $"blocked by red turn-state block at {LevelEditorRuntimeHelpers.FormatCell(checkPos)}";
                return result;
            }

            if (state.blackHoles.TryGetValue(checkPos, out ArrowDir blackHoleDir))
            {
                if (blackHoleDir == LevelEditorRuntimeHelpers.GetOppositeDirection(currentDir))
                {
                    result.canExit = true;
                    return result;
                }

                result.blockedReason = $"blocked by black hole facing {blackHoleDir} at {LevelEditorRuntimeHelpers.FormatCell(checkPos)}";
                return result;
            }

            if (state.gates.TryGetValue(checkPos, out Color gateColor) && !locallyOpenedGateCells.Contains(checkPos))
            {
                result.blockedReason = $"blocked by gate {LevelEditorRuntimeHelpers.FormatColor(gateColor)} at {LevelEditorRuntimeHelpers.FormatCell(checkPos)}";
                return result;
            }

            if (TryGetActiveElectricWallAt(checkPos, state, locallyDisabledWallIds, out LevelEditorDeadlockElectricWall wall))
            {
                result.blockedReason = $"blocked by electric wall {LevelEditorRuntimeHelpers.FormatColor(wall.color)} at {LevelEditorRuntimeHelpers.FormatCell(checkPos)}";
                return result;
            }

            if (state.keycards.TryGetValue(checkPos, out Color keyColor))
            {
                result.collectedKeyColors.Add(keyColor);
                MarkMatchingGatesOpened(state, keyColor, locallyOpenedGateCells);
            }

            if (state.electricButtons.TryGetValue(checkPos, out Color buttonColor))
            {
                result.pressedButtonColors.Add(buttonColor);
                MarkMatchingElectricWallsDisabled(state, buttonColor, locallyDisabledWallIds);
            }

            if (state.portals.TryGetValue(checkPos, out LevelEditorDeadlockPortalLink portalLink))
            {
                currentPos = portalLink.exit;
                currentDir = portalLink.exitDir;
                step = LevelEditorRuntimeHelpers.GetDirStep(currentDir);
                if (step == Vector2Int.zero)
                {
                    result.blockedReason = $"portal exits with invalid direction at {LevelEditorRuntimeHelpers.FormatCell(checkPos)}";
                    return result;
                }
                continue;
            }

            if (state.deflectors.TryGetValue(checkPos, out ArrowDir deflectedDir))
            {
                currentPos = checkPos;
                currentDir = deflectedDir;
                step = LevelEditorRuntimeHelpers.GetDirStep(currentDir);
                if (step == Vector2Int.zero)
                {
                    result.blockedReason = $"deflector has invalid direction at {LevelEditorRuntimeHelpers.FormatCell(checkPos)}";
                    return result;
                }
                continue;
            }

            currentPos = checkPos;
        }

        result.canExit = true;
        return result;
    }

    private static void ApplyDeadlockRelease(LevelEditorDeadlockSnake snake, LevelEditorDeadlockPathResult pathResult, LevelEditorDeadlockState state)
    {
        snake.released = true;
        state.releasedCount++;

        for (int i = 0; i < snake.cells.Count; i++)
        {
            Vector2Int cell = snake.cells[i];
            if (state.snakeByCell.TryGetValue(cell, out int snakeIndex) && snakeIndex == snake.index)
            {
                state.snakeByCell.Remove(cell);
            }
        }

        for (int i = 0; i < pathResult.collectedKeyColors.Count; i++)
        {
            RemoveMatchingGates(state, pathResult.collectedKeyColors[i]);
        }

        for (int i = 0; i < pathResult.pressedButtonColors.Count; i++)
        {
            DisableMatchingElectricWalls(state, pathResult.pressedButtonColors[i]);
        }

        DecrementCountdownBlocks(state);
        ToggleTurnStateBlocks(state);
        RotateBlackHoles(state);
    }

    private static void MarkMatchingGatesOpened(LevelEditorDeadlockState state, Color keyColor, HashSet<Vector2Int> locallyOpenedGateCells)
    {
        foreach (KeyValuePair<Vector2Int, Color> gate in state.gates)
        {
            if (LevelEditorRuntimeHelpers.ColorsMatch(keyColor, gate.Value))
            {
                locallyOpenedGateCells.Add(gate.Key);
            }
        }
    }

    private static void MarkMatchingElectricWallsDisabled(LevelEditorDeadlockState state, Color buttonColor, HashSet<int> locallyDisabledWallIds)
    {
        for (int i = 0; i < state.electricWalls.Count; i++)
        {
            LevelEditorDeadlockElectricWall wall = state.electricWalls[i];
            if (wall.active && LevelEditorRuntimeHelpers.ColorsMatch(buttonColor, wall.color))
            {
                locallyDisabledWallIds.Add(wall.index);
            }
        }
    }

    private static void RemoveMatchingGates(LevelEditorDeadlockState state, Color keyColor)
    {
        List<Vector2Int> gatesToRemove = new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, Color> gate in state.gates)
        {
            if (LevelEditorRuntimeHelpers.ColorsMatch(keyColor, gate.Value))
            {
                gatesToRemove.Add(gate.Key);
            }
        }

        for (int i = 0; i < gatesToRemove.Count; i++)
        {
            state.gates.Remove(gatesToRemove[i]);
        }
    }

    private static void DisableMatchingElectricWalls(LevelEditorDeadlockState state, Color buttonColor)
    {
        for (int i = 0; i < state.electricWalls.Count; i++)
        {
            LevelEditorDeadlockElectricWall wall = state.electricWalls[i];
            if (wall.active && LevelEditorRuntimeHelpers.ColorsMatch(buttonColor, wall.color))
            {
                wall.active = false;
            }
        }
    }

    private static void DecrementCountdownBlocks(LevelEditorDeadlockState state)
    {
        if (state.countdownBlocks.Count == 0) return;

        List<Vector2Int> cells = new List<Vector2Int>(state.countdownBlocks.Keys);
        for (int i = 0; i < cells.Count; i++)
        {
            Vector2Int cell = cells[i];
            int nextCount = state.countdownBlocks[cell] - 1;
            if (nextCount <= 0) state.countdownBlocks.Remove(cell);
            else state.countdownBlocks[cell] = nextCount;
        }
    }

    private static void ToggleTurnStateBlocks(LevelEditorDeadlockState state)
    {
        if (state.turnStateBlocks.Count == 0) return;

        List<Vector2Int> keys = new List<Vector2Int>(state.turnStateBlocks.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            Vector2Int key = keys[i];
            state.turnStateBlocks[key] = !state.turnStateBlocks[key];
        }
    }

    private static void RotateBlackHoles(LevelEditorDeadlockState state)
    {
        if (state.blackHoles.Count == 0) return;

        List<Vector2Int> keys = new List<Vector2Int>(state.blackHoles.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            Vector2Int key = keys[i];
            state.blackHoles[key] = LevelEditorRuntimeHelpers.GetClockwiseDirection(state.blackHoles[key]);
        }
    }

    private static bool TryGetActiveElectricWallAt(Vector2Int cell, LevelEditorDeadlockState state, HashSet<int> locallyDisabledWallIds, out LevelEditorDeadlockElectricWall wall)
    {
        wall = null;
        if (!state.electricWallIdsByCell.TryGetValue(cell, out List<int> wallIds)) return false;

        for (int i = 0; i < wallIds.Count; i++)
        {
            int wallIndex = wallIds[i];
            if (wallIndex < 0 || wallIndex >= state.electricWalls.Count) continue;
            if (locallyDisabledWallIds.Contains(wallIndex)) continue;

            LevelEditorDeadlockElectricWall candidate = state.electricWalls[wallIndex];
            if (!candidate.active) continue;

            wall = candidate;
            return true;
        }

        return false;
    }

    private static string BuildReleaseOrderText(List<int> releaseOrder)
    {
        if (releaseOrder == null || releaseOrder.Count == 0) return "(none)";

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < releaseOrder.Count; i++)
        {
            if (i > 0) builder.Append(" -> ");
            builder.Append(releaseOrder[i] + 1);
        }

        return builder.ToString();
    }
}

public sealed class LevelEditorDeadlockState
{
    public readonly List<LevelEditorDeadlockSnake> snakes = new List<LevelEditorDeadlockSnake>();
    public readonly Dictionary<Vector2Int, int> snakeByCell = new Dictionary<Vector2Int, int>();
    public readonly Dictionary<Vector2Int, Color> keycards = new Dictionary<Vector2Int, Color>();
    public readonly Dictionary<Vector2Int, Color> gates = new Dictionary<Vector2Int, Color>();
    public readonly Dictionary<Vector2Int, Color> electricButtons = new Dictionary<Vector2Int, Color>();
    public readonly List<LevelEditorDeadlockElectricWall> electricWalls = new List<LevelEditorDeadlockElectricWall>();
    public readonly Dictionary<Vector2Int, List<int>> electricWallIdsByCell = new Dictionary<Vector2Int, List<int>>();
    public readonly Dictionary<Vector2Int, int> countdownBlocks = new Dictionary<Vector2Int, int>();
    public readonly Dictionary<Vector2Int, int> stopBlocks = new Dictionary<Vector2Int, int>();
    public readonly Dictionary<Vector2Int, bool> turnStateBlocks = new Dictionary<Vector2Int, bool>();
    public readonly Dictionary<Vector2Int, ArrowDir> blackHoles = new Dictionary<Vector2Int, ArrowDir>();
    public readonly Dictionary<Vector2Int, LevelEditorDeadlockPortalLink> portals = new Dictionary<Vector2Int, LevelEditorDeadlockPortalLink>();
    public readonly Dictionary<Vector2Int, ArrowDir> deflectors = new Dictionary<Vector2Int, ArrowDir>();
    public int releasedCount;
}

public sealed class LevelEditorDeadlockSnake
{
    public int index;
    public string label;
    public ArrowDir direction;
    public Color color;
    public readonly List<Vector2Int> cells = new List<Vector2Int>();
    public bool released;
    public string lastBlockedReason;
}

public sealed class LevelEditorDeadlockElectricWall
{
    public int index;
    public ElectricWallSaveData data;
    public Color color;
    public bool active = true;
    public readonly List<Vector2Int> cells = new List<Vector2Int>();
}

public sealed class LevelEditorDeadlockPortalLink
{
    public Vector2Int exit;
    public ArrowDir exitDir;
}

public sealed class LevelEditorDeadlockPathResult
{
    public bool canExit;
    public string blockedReason;
    public readonly List<Color> collectedKeyColors = new List<Color>();
    public readonly List<Color> pressedButtonColors = new List<Color>();
}
