using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelAutoBuilderWindow : EditorWindow
{
    private const string DefaultOutputFolder = "Assets/Resources/Levels/Generated";
    private const string DefaultLearnFolder = "Assets/Resources/Levels";
    private static readonly Color[] Palette =
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        new Color(1f, 0.35f, 1f, 1f),
        new Color(0f, 1f, 1f, 1f),
        Color.white
    };

    [SerializeField] private string outputFolder = DefaultOutputFolder;
    [SerializeField] private string learnFolder = DefaultLearnFolder;
    [SerializeField] private string levelNamePrefix = "AutoLevel";
    [SerializeField] private int levelIndex = 1000;
    [SerializeField] private GameMode gameMode = GameMode.Classic;
    [SerializeField] private LevelDifficulty difficulty = LevelDifficulty.Medium;
    [SerializeField] private int arrowCount = 12;
    [SerializeField] private int keyGatePairs = 2;
    [SerializeField] private int electricPairs = 1;
    [SerializeField] private int portalPairs = 1;
    [SerializeField] private int deflectorCount = 1;
    [SerializeField] private int countdownBlockCount = 0;
    [SerializeField] private bool autoLearnBeforeGenerate = true;
    [SerializeField] private bool useLearnedLayout = true;
    [SerializeField] private int minDistanceBetweenSnakes = 2;
    [SerializeField] private int minDistanceWithinSnake = 2;
    [SerializeField] private int boardHalfWidth = 18;
    [SerializeField] private int boardHalfHeight = 18;
    [SerializeField] private int maxGeneratedSnakeLength = 32;
    [SerializeField] private int maxAttempts = 100;
    [SerializeField] private int solverNodeLimit = 200000;
    [SerializeField] private int batchCount = 1;
    [SerializeField] private int randomSeed = 12345;
    [SerializeField] private bool useRandomSeed = true;

    private Vector2 _scroll;
    private string _lastReport = "Ready.";
    private LevelProfile _profile;

    [MenuItem("Tools/Arrow Puzzle/Auto Level Builder")]
    public static void Open()
    {
        GetWindow<LevelAutoBuilderWindow>("Auto Level Builder");
    }

    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        learnFolder = EditorGUILayout.TextField("Learn Folder", learnFolder);
        levelNamePrefix = EditorGUILayout.TextField("Name Prefix", levelNamePrefix);
        levelIndex = EditorGUILayout.IntField("Start Level Index", levelIndex);
        gameMode = (GameMode)EditorGUILayout.EnumPopup("Game Mode", gameMode);
        difficulty = (LevelDifficulty)EditorGUILayout.EnumPopup("Difficulty", difficulty);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Requirements", EditorStyles.boldLabel);
        arrowCount = Mathf.Max(1, EditorGUILayout.IntField("Arrow Count", arrowCount));
        keyGatePairs = Mathf.Max(0, EditorGUILayout.IntField("Key/Gate Pairs", keyGatePairs));
        electricPairs = Mathf.Max(0, EditorGUILayout.IntField("Electric Pairs", electricPairs));
        portalPairs = Mathf.Max(0, EditorGUILayout.IntField("Portal Pairs", portalPairs));
        deflectorCount = Mathf.Max(0, EditorGUILayout.IntField("Deflectors", deflectorCount));
        countdownBlockCount = Mathf.Max(0, EditorGUILayout.IntField("Countdown Blocks", countdownBlockCount));

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Learned Layout", EditorStyles.boldLabel);
        autoLearnBeforeGenerate = EditorGUILayout.Toggle("Auto Learn Before Generate", autoLearnBeforeGenerate);
        useLearnedLayout = EditorGUILayout.Toggle("Use Learned Layout", useLearnedLayout);
        minDistanceBetweenSnakes = Mathf.Max(1, EditorGUILayout.IntField("Min Snake Distance", minDistanceBetweenSnakes));
        minDistanceWithinSnake = Mathf.Max(1, EditorGUILayout.IntField("Min Self Distance", minDistanceWithinSnake));
        boardHalfWidth = Mathf.Max(4, EditorGUILayout.IntField("Board Half Width", boardHalfWidth));
        boardHalfHeight = Mathf.Max(4, EditorGUILayout.IntField("Board Half Height", boardHalfHeight));
        maxGeneratedSnakeLength = Mathf.Max(3, EditorGUILayout.IntField("Max Snake Length", maxGeneratedSnakeLength));

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Relearn From Existing Levels", GUILayout.Height(24f))) RelearnFromExistingLevels(true);
            if (GUILayout.Button("Validate Source Levels", GUILayout.Height(24f))) ValidateSourceLevels();
        }

        if (_profile != null)
        {
            EditorGUILayout.HelpBox(_profile.GetSummary(), MessageType.None);
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Generation", EditorStyles.boldLabel);
        maxAttempts = Mathf.Max(1, EditorGUILayout.IntField("Max Attempts", maxAttempts));
        solverNodeLimit = Mathf.Max(1000, EditorGUILayout.IntField("Solver Node Limit", solverNodeLimit));
        batchCount = Mathf.Max(1, EditorGUILayout.IntField("Batch Count", batchCount));
        useRandomSeed = EditorGUILayout.Toggle("Use Random Seed", useRandomSeed);
        using (new EditorGUI.DisabledScope(useRandomSeed))
        {
            randomSeed = EditorGUILayout.IntField("Seed", randomSeed);
        }

        EditorGUILayout.Space(10f);
        if (GUILayout.Button("Generate Solvable Level", GUILayout.Height(30f))) GenerateBatch(1);
        if (GUILayout.Button("Generate Batch", GUILayout.Height(26f))) GenerateBatch(batchCount);
        if (GUILayout.Button("Validate Selected LevelDataSO", GUILayout.Height(24f))) ValidateSelectedLevel();

        EditorGUILayout.Space(10f);
        EditorGUILayout.HelpBox(_lastReport, MessageType.Info);
        EditorGUILayout.EndScrollView();
    }

    private void GenerateBatch(int count)
    {
        EnsureFolder(outputFolder);
        if (autoLearnBeforeGenerate || _profile == null) RelearnFromExistingLevels(false);

        int baseSeed = useRandomSeed ? System.Environment.TickCount : randomSeed;
        int saved = 0;
        List<string> reports = new List<string>();

        for (int i = 0; i < count; i++)
        {
            LevelDataSO level = null;
            LevelSolveReport report = default;
            int usedSeed = baseSeed + i * 9973;
            bool success = false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                level = GenerateCandidate(levelIndex + saved, usedSeed + attempt);
                if (level == null) continue;

                string validationMessage;
                if (!LevelAutoValidator.Validate(level, minDistanceBetweenSnakes, minDistanceWithinSnake, out validationMessage))
                {
                    report = new LevelSolveReport { solved = false, message = validationMessage };
                    continue;
                }

                report = LevelAutoSolver.Solve(level, solverNodeLimit);
                if (report.solved)
                {
                    success = true;
                    break;
                }
            }

            if (!success)
            {
                reports.Add("Failed slot " + (i + 1) + ": " + report.message);
                continue;
            }

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(outputFolder + "/" + levelNamePrefix + "_" + level.levelIndex.ToString("0000") + ".asset");
            AssetDatabase.CreateAsset(level, assetPath);
            saved++;
            reports.Add("Saved " + assetPath + " | steps=" + report.steps + " | seed=" + usedSeed);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        _lastReport = "Generated " + saved + "/" + count + " level(s).\n" + string.Join("\n", reports);
    }

    private LevelDataSO GenerateCandidate(int targetLevelIndex, int seed)
    {
        if (useLearnedLayout && _profile != null && _profile.HasEnoughData)
        {
            LevelDataSO learned = GenerateLearnedCandidate(targetLevelIndex, seed);
            if (learned != null) return learned;
        }

        return GenerateFallbackCandidate(targetLevelIndex, seed);
    }

    private LevelDataSO GenerateLearnedCandidate(int targetLevelIndex, int seed)
    {
        System.Random rng = new System.Random(seed);
        LevelDataSO level = CreateBaseLevel(targetLevelIndex);
        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

        int halfWidth = Mathf.Max(boardHalfWidth, Mathf.CeilToInt(Mathf.Sqrt(arrowCount) * 5f));
        int halfHeight = Mathf.Max(boardHalfHeight, Mathf.CeilToInt(Mathf.Sqrt(arrowCount) * 5f));
        RectInt bounds = new RectInt(-halfWidth, -halfHeight, halfWidth * 2 + 1, halfHeight * 2 + 1);

        for (int i = 0; i < arrowCount; i++)
        {
            bool placed = false;
            for (int attempt = 0; attempt < 120; attempt++)
            {
                ArrowDir dir = _profile.SampleDirection(rng, i);
                Vector2Int head = GetRandomHeadOnEdge(rng, bounds, dir);
                int length = Mathf.Clamp(_profile.SampleSnakeLength(rng), 3, maxGeneratedSnakeLength);
                List<Vector2Int> cells = new List<Vector2Int>();

                if (!TryBuildSnakeBody(rng, bounds, occupied, head, dir, length, cells)) continue;

                for (int c = 0; c < cells.Count; c++) occupied.Add(cells[c]);
                level.snakes.Add(new SnakeSaveData
                {
                    direction = dir,
                    arrowColor = _profile.SampleColor(rng, i),
                    segmentPositions = cells
                });
                placed = true;
                break;
            }

            if (!placed) return null;
        }

        AddRequestedMechanics(level, rng, occupied);
        return level;
    }

    private LevelDataSO GenerateFallbackCandidate(int targetLevelIndex, int seed)
    {
        System.Random rng = new System.Random(seed);
        LevelDataSO level = CreateBaseLevel(targetLevelIndex);
        int spacing = 3;
        int startX = -((arrowCount - 1) * spacing) / 2;

        for (int i = 0; i < arrowCount; i++)
        {
            int laneX = startX + i * spacing;
            level.snakes.Add(new SnakeSaveData
            {
                direction = ArrowDir.Up,
                arrowColor = Palette[i % Palette.Length],
                segmentPositions = new List<Vector2Int>
                {
                    new Vector2Int(laneX, -4),
                    new Vector2Int(laneX, -5),
                    new Vector2Int(laneX, -6)
                }
            });
        }

        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
        for (int i = 0; i < level.snakes.Count; i++)
            for (int c = 0; c < level.snakes[i].segmentPositions.Count; c++)
                occupied.Add(level.snakes[i].segmentPositions[c]);

        AddRequestedMechanics(level, rng, occupied);
        return level;
    }

    private LevelDataSO CreateBaseLevel(int targetLevelIndex)
    {
        LevelDataSO level = CreateInstance<LevelDataSO>();
        level.levelIndex = targetLevelIndex;
        level.gameMode = gameMode;
        level.levelDifficulty = difficulty;
        level.timeLimit = gameMode == GameMode.TimeAttack ? 120f : 60f;
        level.rewardCoins = difficulty == LevelDifficulty.Hard ? 30f : 10f;
        level.rewardDiamonds = difficulty == LevelDifficulty.Hard ? 2f : 1f;
        return level;
    }

    private bool TryBuildSnakeBody(System.Random rng, RectInt bounds, HashSet<Vector2Int> occupied, Vector2Int head, ArrowDir releaseDir, int targetLength, List<Vector2Int> cells)
    {
        if (occupied.Contains(head)) return false;
        if (IsTooCloseToOccupiedSnake(head, occupied)) return false;

        cells.Clear();
        cells.Add(head);

        Vector2Int current = head;
        Vector2Int lastStep = Invert(Step(releaseDir));
        int guard = targetLength * 16;

        while (cells.Count < targetLength && guard-- > 0)
        {
            List<Vector2Int> candidates = BuildWeightedBodySteps(rng, lastStep);
            bool moved = false;

            for (int i = 0; i < candidates.Count; i++)
            {
                Vector2Int next = current + candidates[i];
                if (!bounds.Contains(next)) continue;
                if (occupied.Contains(next) || cells.Contains(next)) continue;
                if (IsTooCloseToOccupiedSnake(next, occupied)) continue;
                if (IsTooCloseToCurrentSnake(next, cells)) continue;

                cells.Add(next);
                current = next;
                lastStep = candidates[i];
                moved = true;
                break;
            }

            if (!moved)
            {
                if (cells.Count >= 3) return true;
                return false;
            }
        }

        return cells.Count >= 3;
    }

    private bool IsTooCloseToOccupiedSnake(Vector2Int pos, HashSet<Vector2Int> occupiedSnakeCells)
    {
        if (minDistanceBetweenSnakes <= 1) return false;
        foreach (Vector2Int occupied in occupiedSnakeCells)
        {
            int dist = Mathf.Abs(occupied.x - pos.x) + Mathf.Abs(occupied.y - pos.y);
            if (dist < minDistanceBetweenSnakes) return true;
        }

        return false;
    }

    private bool IsTooCloseToCurrentSnake(Vector2Int pos, List<Vector2Int> cells)
    {
        if (minDistanceWithinSnake <= 1 || cells == null || cells.Count <= 1) return false;

        for (int i = 0; i < cells.Count - 1; i++)
        {
            int dist = Mathf.Abs(cells[i].x - pos.x) + Mathf.Abs(cells[i].y - pos.y);
            if (dist < minDistanceWithinSnake) return true;
        }

        return false;
    }

    private List<Vector2Int> BuildWeightedBodySteps(System.Random rng, Vector2Int lastStep)
    {
        List<Vector2Int> steps = new List<Vector2Int>(8);
        int straightWeight = _profile != null ? _profile.StraightWeight : 4;
        int turnWeight = _profile != null ? _profile.TurnWeight : 3;

        for (int i = 0; i < straightWeight; i++) steps.Add(lastStep);
        for (int i = 0; i < turnWeight; i++)
        {
            steps.Add(RotateLeft(lastStep));
            steps.Add(RotateRight(lastStep));
        }

        for (int i = 0; i < steps.Count; i++)
        {
            int swap = rng.Next(i, steps.Count);
            Vector2Int temp = steps[i];
            steps[i] = steps[swap];
            steps[swap] = temp;
        }

        return steps;
    }

    private Vector2Int GetRandomHeadOnEdge(System.Random rng, RectInt bounds, ArrowDir dir)
    {
        int margin = 2;
        switch (dir)
        {
            case ArrowDir.Up:
                return new Vector2Int(rng.Next(bounds.xMin + margin, bounds.xMax - margin), bounds.yMax - 1);
            case ArrowDir.Down:
                return new Vector2Int(rng.Next(bounds.xMin + margin, bounds.xMax - margin), bounds.yMin);
            case ArrowDir.Left:
                return new Vector2Int(bounds.xMin, rng.Next(bounds.yMin + margin, bounds.yMax - margin));
            case ArrowDir.Right:
                return new Vector2Int(bounds.xMax - 1, rng.Next(bounds.yMin + margin, bounds.yMax - margin));
            default:
                return Vector2Int.zero;
        }
    }

    private void AddRequestedMechanics(LevelDataSO level, System.Random rng, HashSet<Vector2Int> occupied)
    {
        HashSet<Vector2Int> reserved = new HashSet<Vector2Int>(occupied);
        int[] nextDistance = new int[level.snakes.Count];
        for (int i = 0; i < nextDistance.Length; i++) nextDistance[i] = 1;
        int snakeCursor = 0;

        for (int i = 0; i < keyGatePairs; i++)
        {
            Color color = _profile != null ? _profile.SampleColor(rng, i) : Palette[i % Palette.Length];
            Vector2Int key;
            Vector2Int gate;
            if (!TryReserveRayPair(level, reserved, nextDistance, ref snakeCursor, 2, out key, out gate))
            {
                key = FindFreeCell(rng, reserved);
                gate = FindFreeCell(rng, reserved);
            }

            level.keycards.Add(new KeycardSaveData { position = key, color = color });
            level.gates.Add(new GateSaveData { position = gate, color = color });
        }

        for (int i = 0; i < electricPairs; i++)
        {
            Color color = _profile != null ? _profile.SampleColor(rng, i + keyGatePairs) : Palette[(i + keyGatePairs) % Palette.Length];
            Vector2Int button;
            Vector2Int wallStart;
            Vector2Int wallEnd;
            if (!TryReserveElectricPair(level, reserved, nextDistance, ref snakeCursor, out button, out wallStart, out wallEnd))
            {
                button = FindFreeCell(rng, reserved);
                if (!TryFindFreeElectricWall(rng, reserved, out wallStart, out wallEnd))
                {
                    wallStart = FindFreeCell(rng, reserved);
                    wallEnd = FindAdjacentFreeCell(wallStart, reserved);
                }
            }

            level.electricButtons.Add(new ElectricButtonSaveData { position = button, color = color });
            level.electricWalls.Add(new ElectricWallSaveData { start = wallStart, end = wallEnd, color = color });
        }

        for (int i = 0; i < portalPairs; i++)
        {
            Vector2Int entrance;
            Vector2Int exit;
            int snakeIndex;
            if (!TryReserveRayPair(level, reserved, nextDistance, ref snakeCursor, 3, out entrance, out exit, out snakeIndex))
            {
                entrance = FindFreeCell(rng, reserved);
                exit = FindFreeCell(rng, reserved);
                snakeIndex = level.snakes.Count > 0 ? rng.Next(0, level.snakes.Count) : 0;
            }

            ArrowDir exitDir = level.snakes.Count > 0 ? level.snakes[snakeIndex].direction : ArrowDir.Up;
            level.portals.Add(new PortalData
            {
                entrance = entrance,
                entranceDir = exitDir,
                exit = exit,
                exitDir = exitDir,
                portalColor = _profile != null ? _profile.SampleColor(rng, i + 2) : Palette[(i + 2) % Palette.Length]
            });
        }

        for (int i = 0; i < deflectorCount; i++)
        {
            Vector2Int position;
            int snakeIndex;
            if (!TryReserveRayCell(level, reserved, nextDistance, ref snakeCursor, out position, out snakeIndex))
            {
                position = FindFreeCell(rng, reserved);
                snakeIndex = level.snakes.Count > 0 ? rng.Next(0, level.snakes.Count) : 0;
            }

            ArrowDir dir = level.snakes.Count > 0 ? level.snakes[snakeIndex].direction : ArrowDir.Up;
            level.deflectors.Add(new DeflectorSaveData { position = position, direction = dir });
        }

        int pathCountdowns = Mathf.Min(countdownBlockCount, Mathf.Max(0, level.snakes.Count - 1));
        for (int i = 0; i < countdownBlockCount; i++)
        {
            Vector2Int position;
            if (i < pathCountdowns)
            {
                int blockedSnake = Mathf.Clamp(level.snakes.Count - 1 - i, 0, level.snakes.Count - 1);
                if (!TryReserveRayCellForSnake(level, reserved, nextDistance, blockedSnake, out position))
                    position = FindFreeCell(rng, reserved);
            }
            else
            {
                position = FindFreeCell(rng, reserved);
            }

            int maxCount = Mathf.Max(1, Mathf.Min(3, level.snakes.Count - 1));
            level.countdownBlocks.Add(new CountdownBlockSaveData { position = position, count = rng.Next(1, maxCount + 1) });
        }
    }

    private bool TryReserveRayPair(LevelDataSO level, HashSet<Vector2Int> reserved, int[] nextDistance, ref int snakeCursor, int gap, out Vector2Int first, out Vector2Int second)
    {
        int snakeIndex;
        return TryReserveRayPair(level, reserved, nextDistance, ref snakeCursor, gap, out first, out second, out snakeIndex);
    }

    private bool TryReserveRayPair(LevelDataSO level, HashSet<Vector2Int> reserved, int[] nextDistance, ref int snakeCursor, int gap, out Vector2Int first, out Vector2Int second, out int snakeIndex)
    {
        first = Vector2Int.zero;
        second = Vector2Int.zero;
        snakeIndex = 0;
        if (level.snakes == null || level.snakes.Count == 0) return false;

        for (int tries = 0; tries < level.snakes.Count; tries++)
        {
            snakeIndex = snakeCursor % level.snakes.Count;
            snakeCursor++;
            SnakeSaveData snake = level.snakes[snakeIndex];
            Vector2Int step = Step(snake.direction);
            Vector2Int head = snake.segmentPositions[0];

            for (int distance = nextDistance[snakeIndex]; distance < 24; distance++)
            {
                Vector2Int a = head + step * distance;
                Vector2Int b = head + step * (distance + gap);
                if (reserved.Contains(a) || reserved.Contains(b)) continue;

                reserved.Add(a);
                reserved.Add(b);
                nextDistance[snakeIndex] = distance + gap + 2;
                first = a;
                second = b;
                return true;
            }
        }

        return false;
    }

    private bool TryReserveElectricPair(LevelDataSO level, HashSet<Vector2Int> reserved, int[] nextDistance, ref int snakeCursor, out Vector2Int button, out Vector2Int wallStart, out Vector2Int wallEnd)
    {
        button = Vector2Int.zero;
        wallStart = Vector2Int.zero;
        wallEnd = Vector2Int.zero;
        if (level.snakes == null || level.snakes.Count == 0) return false;

        for (int tries = 0; tries < level.snakes.Count; tries++)
        {
            int snakeIndex = snakeCursor % level.snakes.Count;
            snakeCursor++;
            SnakeSaveData snake = level.snakes[snakeIndex];
            Vector2Int step = Step(snake.direction);
            Vector2Int head = snake.segmentPositions[0];

            for (int distance = nextDistance[snakeIndex]; distance < 24; distance++)
            {
                Vector2Int candidateButton = head + step * distance;
                Vector2Int candidateStart = head + step * (distance + 2);
                if (reserved.Contains(candidateButton) || reserved.Contains(candidateStart)) continue;

                Vector2Int leftEnd = candidateStart + RotateLeft(step);
                Vector2Int rightEnd = candidateStart + RotateRight(step);
                if (TryReserveElectricWallCells(reserved, candidateStart, leftEnd))
                {
                    reserved.Add(candidateButton);
                    nextDistance[snakeIndex] = distance + 5;
                    button = candidateButton;
                    wallStart = candidateStart;
                    wallEnd = leftEnd;
                    return true;
                }

                if (TryReserveElectricWallCells(reserved, candidateStart, rightEnd))
                {
                    reserved.Add(candidateButton);
                    nextDistance[snakeIndex] = distance + 5;
                    button = candidateButton;
                    wallStart = candidateStart;
                    wallEnd = rightEnd;
                    return true;
                }
            }
        }

        return false;
    }

    private bool TryReserveRayCell(LevelDataSO level, HashSet<Vector2Int> reserved, int[] nextDistance, ref int snakeCursor, out Vector2Int cell, out int snakeIndex)
    {
        cell = Vector2Int.zero;
        snakeIndex = 0;
        if (level.snakes == null || level.snakes.Count == 0) return false;

        for (int tries = 0; tries < level.snakes.Count; tries++)
        {
            snakeIndex = snakeCursor % level.snakes.Count;
            snakeCursor++;
            if (TryReserveRayCellForSnake(level, reserved, nextDistance, snakeIndex, out cell)) return true;
        }

        return false;
    }

    private bool TryReserveRayCellForSnake(LevelDataSO level, HashSet<Vector2Int> reserved, int[] nextDistance, int snakeIndex, out Vector2Int cell)
    {
        cell = Vector2Int.zero;
        if (level.snakes == null || snakeIndex < 0 || snakeIndex >= level.snakes.Count) return false;

        SnakeSaveData snake = level.snakes[snakeIndex];
        Vector2Int step = Step(snake.direction);
        Vector2Int head = snake.segmentPositions[0];

        for (int distance = Mathf.Max(1, nextDistance[snakeIndex]); distance < 24; distance++)
        {
            Vector2Int candidate = head + step * distance;
            if (reserved.Contains(candidate)) continue;

            reserved.Add(candidate);
            nextDistance[snakeIndex] = distance + 2;
            cell = candidate;
            return true;
        }

        return false;
    }

    private Vector2Int FindFreeCell(System.Random rng, HashSet<Vector2Int> reserved)
    {
        for (int i = 0; i < 200; i++)
        {
            Vector2Int candidate = new Vector2Int(rng.Next(-boardHalfWidth - 8, boardHalfWidth + 9), rng.Next(-boardHalfHeight - 8, boardHalfHeight + 9));
            if (reserved.Contains(candidate)) continue;
            reserved.Add(candidate);
            return candidate;
        }

        Vector2Int fallback = new Vector2Int(boardHalfWidth + reserved.Count + 10, boardHalfHeight + 10);
        reserved.Add(fallback);
        return fallback;
    }

    private bool TryFindFreeElectricWall(System.Random rng, HashSet<Vector2Int> reserved, out Vector2Int start, out Vector2Int end)
    {
        for (int i = 0; i < 200; i++)
        {
            start = new Vector2Int(rng.Next(-boardHalfWidth - 8, boardHalfWidth + 9), rng.Next(-boardHalfHeight - 8, boardHalfHeight + 9));
            Vector2Int dir = rng.Next(0, 2) == 0 ? Vector2Int.right : Vector2Int.up;
            end = start + dir;
            if (TryReserveElectricWallCells(reserved, start, end)) return true;
        }

        start = Vector2Int.zero;
        end = Vector2Int.right;
        return false;
    }

    private Vector2Int FindAdjacentFreeCell(Vector2Int start, HashSet<Vector2Int> reserved)
    {
        Vector2Int[] steps = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };
        for (int i = 0; i < steps.Length; i++)
        {
            Vector2Int candidate = start + steps[i];
            if (reserved.Contains(candidate)) continue;
            reserved.Add(candidate);
            return candidate;
        }

        return start + Vector2Int.right;
    }

    private static bool TryReserveElectricWallCells(HashSet<Vector2Int> reserved, Vector2Int start, Vector2Int end)
    {
        if (!IsElectricWallAligned(start, end) || start == end) return false;

        List<Vector2Int> cells = new List<Vector2Int>();
        foreach (Vector2Int cell in CellsOnLine(start, end))
        {
            if (reserved.Contains(cell)) return false;
            cells.Add(cell);
        }

        for (int i = 0; i < cells.Count; i++) reserved.Add(cells[i]);
        return true;
    }

    private void RelearnFromExistingLevels(bool report)
    {
        _profile = LevelProfile.Build(learnFolder, outputFolder);
        if (report) _lastReport = _profile.GetSummary();
    }

    private void ValidateSelectedLevel()
    {
        LevelDataSO level = Selection.activeObject as LevelDataSO;
        if (level == null)
        {
            _lastReport = "Select a LevelDataSO asset first.";
            return;
        }

        LevelSolveReport report = LevelAutoSolver.Solve(level, solverNodeLimit);
        string validationMessage;
        bool valid = LevelAutoValidator.Validate(level, minDistanceBetweenSnakes, minDistanceWithinSnake, out validationMessage);
        _lastReport = (valid ? "STRUCTURE OK" : "STRUCTURE FAILED") + ": " + validationMessage + "\n"
            + (report.solved
                ? "SOLVED: " + level.name + " | steps=" + report.steps + "\n" + report.message
                : "FAILED: " + level.name + " | steps=" + report.steps + "\n" + report.message);
    }

    private void ValidateSourceLevels()
    {
        string[] guids = AssetDatabase.FindAssets("t:LevelDataSO", new[] { learnFolder });
        int total = 0;
        int solved = 0;
        List<string> failed = new List<string>();

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (ShouldSkipLearnPath(path, outputFolder)) continue;

            LevelDataSO level = AssetDatabase.LoadAssetAtPath<LevelDataSO>(path);
            if (level == null) continue;

            total++;
            string validationMessage;
            if (!LevelAutoValidator.Validate(level, minDistanceBetweenSnakes, minDistanceWithinSnake, out validationMessage))
            {
                failed.Add(level.name + ": invalid structure: " + validationMessage);
                continue;
            }

            LevelSolveReport report = LevelAutoSolver.Solve(level, solverNodeLimit);
            if (report.solved) solved++;
            else failed.Add(level.name + ": " + report.message);
        }

        _lastReport = "Validated source levels: " + solved + "/" + total + " solved by editor solver."
            + (failed.Count > 0 ? "\nFailed:\n" + string.Join("\n", failed) : string.Empty);
    }

    private static bool ShouldSkipLearnPath(string path, string outputFolder)
    {
        string normalized = path.Replace('\\', '/');
        string output = outputFolder.Replace('\\', '/').TrimEnd('/');
        return normalized.Contains("/Generated/") || normalized.StartsWith(output + "/");
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;

        string[] parts = folder.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private static Vector2Int RotateLeft(Vector2Int v)
    {
        return new Vector2Int(-v.y, v.x);
    }

    private static Vector2Int RotateRight(Vector2Int v)
    {
        return new Vector2Int(v.y, -v.x);
    }

    private static Vector2Int Invert(Vector2Int v)
    {
        return new Vector2Int(-v.x, -v.y);
    }

    private static Vector2Int Step(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: return Vector2Int.up;
            case ArrowDir.Down: return Vector2Int.down;
            case ArrowDir.Left: return Vector2Int.left;
            case ArrowDir.Right: return Vector2Int.right;
            default: return Vector2Int.zero;
        }
    }

    private static bool IsElectricWallAligned(Vector2Int start, Vector2Int end)
    {
        return start.x == end.x || start.y == end.y;
    }

    private static IEnumerable<Vector2Int> CellsOnLine(Vector2Int start, Vector2Int end)
    {
        Vector2Int delta = end - start;
        int steps = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
        if (steps == 0)
        {
            yield return start;
            yield break;
        }

        Vector2Int step = new Vector2Int(delta.x == 0 ? 0 : delta.x / Mathf.Abs(delta.x), delta.y == 0 ? 0 : delta.y / Mathf.Abs(delta.y));
        Vector2Int current = start;
        for (int i = 0; i <= steps; i++)
        {
            yield return current;
            current += step;
        }
    }

    private sealed class LevelProfile
    {
        private readonly List<int> _lengthSamples = new List<int>();
        private readonly List<Color> _colors = new List<Color>();
        private readonly int[] _directionCounts = new int[4];
        private int _turnCount;
        private int _straightCount;
        private int _minX = int.MaxValue;
        private int _minY = int.MaxValue;
        private int _maxX = int.MinValue;
        private int _maxY = int.MinValue;

        public int SourceLevelCount { get; private set; }
        public int SourceSnakeCount { get; private set; }
        public int SourceKeycards { get; private set; }
        public int SourceGates { get; private set; }
        public int SourceElectricButtons { get; private set; }
        public int SourceElectricWalls { get; private set; }
        public int SourcePortals { get; private set; }
        public int SourceDeflectors { get; private set; }
        public int SourceCountdownBlocks { get; private set; }
        public bool HasEnoughData { get { return SourceSnakeCount > 0 && _lengthSamples.Count > 0; } }
        public int StraightWeight { get { return _straightCount >= _turnCount ? 5 : 3; } }
        public int TurnWeight { get { return _turnCount > 0 ? Mathf.Clamp(Mathf.RoundToInt((float)_turnCount / Mathf.Max(1, _straightCount) * 3f), 2, 5) : 2; } }

        public static LevelProfile Build(string learnFolder, string outputFolder)
        {
            LevelProfile profile = new LevelProfile();
            string[] guids = AssetDatabase.FindAssets("t:LevelDataSO", new[] { learnFolder });

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (ShouldSkipLearnPath(path, outputFolder)) continue;

                LevelDataSO level = AssetDatabase.LoadAssetAtPath<LevelDataSO>(path);
                if (level == null || level.snakes == null || level.snakes.Count == 0) continue;

                profile.SourceLevelCount++;
                profile.SourceKeycards += level.keycards != null ? level.keycards.Count : 0;
                profile.SourceGates += level.gates != null ? level.gates.Count : 0;
                profile.SourceElectricButtons += level.electricButtons != null ? level.electricButtons.Count : 0;
                profile.SourceElectricWalls += level.electricWalls != null ? level.electricWalls.Count : 0;
                profile.SourcePortals += level.portals != null ? level.portals.Count : 0;
                profile.SourceDeflectors += level.deflectors != null ? level.deflectors.Count : 0;
                profile.SourceCountdownBlocks += level.countdownBlocks != null ? level.countdownBlocks.Count : 0;

                for (int s = 0; s < level.snakes.Count; s++)
                {
                    SnakeSaveData snake = level.snakes[s];
                    if (snake == null || snake.segmentPositions == null || snake.segmentPositions.Count == 0) continue;

                    profile.SourceSnakeCount++;
                    profile._lengthSamples.Add(snake.segmentPositions.Count);
                    int dirIndex = Mathf.Clamp((int)snake.direction, 0, 3);
                    profile._directionCounts[dirIndex]++;
                    profile.AddColor(snake.arrowColor);
                    profile.CollectTurns(snake.segmentPositions);

                    for (int p = 0; p < snake.segmentPositions.Count; p++)
                    {
                        Vector2Int cell = snake.segmentPositions[p];
                        profile._minX = Mathf.Min(profile._minX, cell.x);
                        profile._minY = Mathf.Min(profile._minY, cell.y);
                        profile._maxX = Mathf.Max(profile._maxX, cell.x);
                        profile._maxY = Mathf.Max(profile._maxY, cell.y);
                    }
                }

                if (level.keycards != null)
                    for (int k = 0; k < level.keycards.Count; k++) profile.AddColor(level.keycards[k].color);
                if (level.gates != null)
                    for (int g = 0; g < level.gates.Count; g++) profile.AddColor(level.gates[g].color);
            }

            if (profile._colors.Count == 0)
                for (int i = 0; i < Palette.Length; i++) profile._colors.Add(Palette[i]);

            return profile;
        }

        public ArrowDir SampleDirection(System.Random rng, int fallbackIndex)
        {
            int total = 0;
            for (int i = 0; i < _directionCounts.Length; i++) total += _directionCounts[i];
            if (total <= 0) return (ArrowDir)(fallbackIndex % 4);

            int roll = rng.Next(0, total);
            for (int i = 0; i < _directionCounts.Length; i++)
            {
                roll -= _directionCounts[i];
                if (roll < 0) return (ArrowDir)i;
            }

            return ArrowDir.Up;
        }

        public int SampleSnakeLength(System.Random rng)
        {
            if (_lengthSamples.Count == 0) return 8;
            return _lengthSamples[rng.Next(0, _lengthSamples.Count)];
        }

        public Color SampleColor(System.Random rng, int fallbackIndex)
        {
            if (_colors.Count == 0) return Palette[fallbackIndex % Palette.Length];
            return _colors[rng.Next(0, _colors.Count)];
        }

        public string GetSummary()
        {
            float avgLength = SourceSnakeCount > 0 ? Average(_lengthSamples) : 0f;
            string bounds = _minX == int.MaxValue ? "n/a" : "(" + _minX + "," + _minY + ")..(" + _maxX + "," + _maxY + ")";
            return "Learned source levels: " + SourceLevelCount
                + "\nSnakes: " + SourceSnakeCount + " | avg length: " + avgLength.ToString("0.0")
                + "\nDirection counts U/D/L/R: " + _directionCounts[0] + "/" + _directionCounts[1] + "/" + _directionCounts[2] + "/" + _directionCounts[3]
                + "\nMechanics key/gate/electric/portal/deflector/countdown: "
                + SourceKeycards + "/" + SourceGates + "/" + SourceElectricWalls + "/" + SourcePortals + "/" + SourceDeflectors + "/" + SourceCountdownBlocks
                + "\nBounds: " + bounds;
        }

        private void AddColor(Color color)
        {
            for (int i = 0; i < _colors.Count; i++)
                if (LevelAutoSolver.ColorsMatch(_colors[i], color)) return;
            _colors.Add(color);
        }

        private void CollectTurns(List<Vector2Int> cells)
        {
            if (cells == null || cells.Count < 3) return;

            Vector2Int previous = NormalizeStep(cells[1] - cells[0]);
            for (int i = 2; i < cells.Count; i++)
            {
                Vector2Int current = NormalizeStep(cells[i] - cells[i - 1]);
                if (current == Vector2Int.zero) continue;
                if (current == previous) _straightCount++;
                else _turnCount++;
                previous = current;
            }
        }

        private static Vector2Int NormalizeStep(Vector2Int delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) return new Vector2Int(delta.x > 0 ? 1 : -1, 0);
            if (delta.y != 0) return new Vector2Int(0, delta.y > 0 ? 1 : -1);
            return Vector2Int.zero;
        }

        private static float Average(List<int> values)
        {
            if (values == null || values.Count == 0) return 0f;
            int sum = 0;
            for (int i = 0; i < values.Count; i++) sum += values[i];
            return (float)sum / values.Count;
        }
    }
}

public struct LevelSolveReport
{
    public bool solved;
    public int steps;
    public int searchedNodes;
    public string message;
}

public static class LevelAutoValidator
{
    public static bool Validate(LevelDataSO level, int minDistanceBetweenSnakes, out string message)
    {
        return Validate(level, minDistanceBetweenSnakes, 2, out message);
    }

    public static bool Validate(LevelDataSO level, int minDistanceBetweenSnakes, int minDistanceWithinSnake, out string message)
    {
        List<string> errors = new List<string>();
        Dictionary<Vector2Int, string> occupied = new Dictionary<Vector2Int, string>();
        List<Vector2Int> previousSnakeCells = new List<Vector2Int>();

        if (level == null)
        {
            message = "Level is null.";
            return false;
        }

        if (level.snakes == null || level.snakes.Count == 0)
        {
            errors.Add("Level must contain at least one snake.");
        }
        else
        {
            for (int i = 0; i < level.snakes.Count; i++)
            {
                SnakeSaveData snake = level.snakes[i];
                ValidateSnake(snake, i, minDistanceBetweenSnakes, minDistanceWithinSnake, occupied, previousSnakeCells, errors);
            }
        }

        if (level.keycards != null)
        {
            for (int i = 0; i < level.keycards.Count; i++)
                TryOccupy(occupied, level.keycards[i].position, "keycard " + i, errors);
        }

        if (level.gates != null)
        {
            for (int i = 0; i < level.gates.Count; i++)
                TryOccupy(occupied, level.gates[i].position, "gate " + i, errors);
        }

        if (level.electricButtons != null)
        {
            for (int i = 0; i < level.electricButtons.Count; i++)
                TryOccupy(occupied, level.electricButtons[i].position, "electric button " + i, errors);
        }

        if (level.deflectors != null)
        {
            for (int i = 0; i < level.deflectors.Count; i++)
            {
                if (!IsDirectionValid(level.deflectors[i].direction))
                    errors.Add("deflector " + i + " has invalid direction.");
                TryOccupy(occupied, level.deflectors[i].position, "deflector " + i, errors);
            }
        }

        if (level.countdownBlocks != null)
        {
            for (int i = 0; i < level.countdownBlocks.Count; i++)
            {
                if (level.countdownBlocks[i].count < 1)
                    errors.Add("countdown block " + i + " must have count >= 1.");
                TryOccupy(occupied, level.countdownBlocks[i].position, "countdown block " + i, errors);
            }
        }

        if (level.portals != null)
        {
            for (int i = 0; i < level.portals.Count; i++)
            {
                PortalData portal = level.portals[i];
                if (portal.entrance == portal.exit)
                    errors.Add("portal " + i + " entrance and exit cannot be the same cell.");
                if (!IsDirectionValid(portal.entranceDir))
                    errors.Add("portal " + i + " has invalid entrance direction.");
                if (!IsDirectionValid(portal.exitDir))
                    errors.Add("portal " + i + " has invalid exit direction.");

                TryOccupy(occupied, portal.entrance, "portal " + i + " entrance", errors);
                TryOccupy(occupied, portal.exit, "portal " + i + " exit", errors);
            }
        }

        if (level.electricWalls != null)
        {
            for (int i = 0; i < level.electricWalls.Count; i++)
                ValidateElectricWall(level.electricWalls[i], i, occupied, errors);
        }

        message = errors.Count == 0
            ? "Matches LevelEditor placement rules."
            : string.Join("\n", errors);
        return errors.Count == 0;
    }

    private static void ValidateSnake(SnakeSaveData snake, int index, int minDistanceBetweenSnakes, int minDistanceWithinSnake, Dictionary<Vector2Int, string> occupied, List<Vector2Int> previousSnakeCells, List<string> errors)
    {
        if (snake == null || snake.segmentPositions == null || snake.segmentPositions.Count == 0)
        {
            errors.Add("snake " + index + " has no nodes.");
            return;
        }

        if (!IsDirectionValid(snake.direction))
            errors.Add("snake " + index + " has invalid direction.");

        HashSet<Vector2Int> selfCells = new HashSet<Vector2Int>();
        for (int i = 0; i < snake.segmentPositions.Count; i++)
        {
            Vector2Int cell = snake.segmentPositions[i];
            if (!selfCells.Add(cell))
                errors.Add("snake " + index + " overlaps itself at " + cell + ".");

            if (i > 0)
            {
                Vector2Int previous = snake.segmentPositions[i - 1];
                int distance = Mathf.Abs(cell.x - previous.x) + Mathf.Abs(cell.y - previous.y);
                if (distance != 1)
                    errors.Add("snake " + index + " has non-adjacent nodes at segment " + (i - 1) + " -> " + i + ".");
            }

            if (minDistanceBetweenSnakes > 1)
            {
                for (int s = 0; s < previousSnakeCells.Count; s++)
                {
                    Vector2Int other = previousSnakeCells[s];
                    int distance = Mathf.Abs(cell.x - other.x) + Mathf.Abs(cell.y - other.y);
                    if (distance < minDistanceBetweenSnakes)
                    {
                        errors.Add("snake " + index + " is too close to another snake at " + cell + ".");
                        break;
                    }
                }
            }

            if (minDistanceWithinSnake > 1)
            {
                for (int s = 0; s < i - 1; s++)
                {
                    Vector2Int other = snake.segmentPositions[s];
                    int distance = Mathf.Abs(cell.x - other.x) + Mathf.Abs(cell.y - other.y);
                    if (distance < minDistanceWithinSnake)
                    {
                        errors.Add("snake " + index + " is too close to itself at " + cell + ".");
                        break;
                    }
                }
            }

            TryOccupy(occupied, cell, "snake " + index, errors);
        }

        previousSnakeCells.AddRange(snake.segmentPositions);
    }

    private static void ValidateElectricWall(ElectricWallSaveData wall, int index, Dictionary<Vector2Int, string> occupied, List<string> errors)
    {
        if (!IsElectricWallAligned(wall.start, wall.end))
        {
            errors.Add("electric wall " + index + " must be horizontal or vertical.");
            return;
        }

        if (wall.start == wall.end)
        {
            errors.Add("electric wall " + index + " start and end cannot be the same cell.");
            return;
        }

        foreach (Vector2Int cell in CellsOnLine(wall.start, wall.end))
            TryOccupy(occupied, cell, "electric wall " + index, errors);
    }

    private static void TryOccupy(Dictionary<Vector2Int, string> occupied, Vector2Int cell, string label, List<string> errors)
    {
        if (occupied.TryGetValue(cell, out string existing))
        {
            errors.Add(label + " overlaps " + existing + " at " + cell + ".");
            return;
        }

        occupied[cell] = label;
    }

    private static bool IsDirectionValid(ArrowDir direction)
    {
        int value = (int)direction;
        return value >= 0 && value <= 3;
    }

    private static bool IsElectricWallAligned(Vector2Int start, Vector2Int end)
    {
        return start.x == end.x || start.y == end.y;
    }

    private static IEnumerable<Vector2Int> CellsOnLine(Vector2Int start, Vector2Int end)
    {
        Vector2Int delta = end - start;
        int steps = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
        if (steps == 0)
        {
            yield return start;
            yield break;
        }

        Vector2Int step = new Vector2Int(delta.x == 0 ? 0 : delta.x / Mathf.Abs(delta.x), delta.y == 0 ? 0 : delta.y / Mathf.Abs(delta.y));
        Vector2Int current = start;
        for (int i = 0; i <= steps; i++)
        {
            yield return current;
            current += step;
        }
    }
}

public sealed class LevelAutoSolver
{
    private const int DefaultSearchNodeLimit = 200000;
    private readonly List<SnakeState> _snakes = new List<SnakeState>();
    private readonly Dictionary<Vector2Int, int> _snakeCells = new Dictionary<Vector2Int, int>();
    private readonly Dictionary<Vector2Int, Color> _keycardColors = new Dictionary<Vector2Int, Color>();
    private readonly Dictionary<Vector2Int, Color> _gateColors = new Dictionary<Vector2Int, Color>();
    private readonly Dictionary<Vector2Int, Color> _buttonColors = new Dictionary<Vector2Int, Color>();
    private readonly Dictionary<Vector2Int, Color> _wallColors = new Dictionary<Vector2Int, Color>();
    private readonly Dictionary<Vector2Int, PortalNode> _portals = new Dictionary<Vector2Int, PortalNode>();
    private readonly Dictionary<Vector2Int, ArrowDir> _deflectors = new Dictionary<Vector2Int, ArrowDir>();
    private readonly Dictionary<Vector2Int, int> _initialCountdowns = new Dictionary<Vector2Int, int>();
    private readonly HashSet<string> _visitedStates = new HashSet<string>();
    private readonly List<int> _bestPartial = new List<int>();
    private int _searchedNodes;
    private int _nodeLimit;

    public static LevelSolveReport Solve(LevelDataSO level)
    {
        return Solve(level, DefaultSearchNodeLimit);
    }

    public static LevelSolveReport Solve(LevelDataSO level, int nodeLimit)
    {
        if (level == null) return new LevelSolveReport { solved = false, message = "Level is null." };

        LevelAutoSolver solver = new LevelAutoSolver();
        solver.Load(level);
        return solver.Run(Mathf.Max(1000, nodeLimit));
    }

    private void Load(LevelDataSO level)
    {
        if (level.snakes != null)
        {
            for (int i = 0; i < level.snakes.Count; i++)
            {
                SnakeSaveData snake = level.snakes[i];
                if (snake == null || snake.segmentPositions == null || snake.segmentPositions.Count == 0) continue;

                SnakeState state = new SnakeState
                {
                    id = _snakes.Count,
                    direction = snake.direction,
                    cells = new List<Vector2Int>(snake.segmentPositions)
                };
                _snakes.Add(state);
                foreach (Vector2Int cell in state.cells) _snakeCells[cell] = state.id;
            }
        }

        if (level.keycards != null)
            foreach (KeycardSaveData item in level.keycards)
                _keycardColors[item.position] = item.color;

        if (level.gates != null)
            foreach (GateSaveData item in level.gates)
                _gateColors[item.position] = item.color;

        if (level.electricButtons != null)
            foreach (ElectricButtonSaveData item in level.electricButtons)
                _buttonColors[item.position] = item.color;

        if (level.electricWalls != null)
        {
            foreach (ElectricWallSaveData wall in level.electricWalls)
            {
                foreach (Vector2Int cell in CellsOnLine(wall.start, wall.end))
                    _wallColors[cell] = wall.color;
            }
        }

        if (level.portals != null)
        {
            foreach (PortalData portal in level.portals)
            {
                _portals[portal.entrance] = new PortalNode { exit = portal.exit, exitDir = portal.exitDir };
                _portals[portal.exit] = new PortalNode { exit = portal.entrance, exitDir = portal.entranceDir };
            }
        }

        if (level.deflectors != null)
            foreach (DeflectorSaveData item in level.deflectors)
                _deflectors[item.position] = item.direction;

        if (level.countdownBlocks != null)
            foreach (CountdownBlockSaveData item in level.countdownBlocks)
                if (item.count > 0) _initialCountdowns[item.position] = item.count;
    }

    private LevelSolveReport Run(int nodeLimit)
    {
        _nodeLimit = nodeLimit;
        _searchedNodes = 0;
        _visitedStates.Clear();
        _bestPartial.Clear();

        SolveState state = SolveState.Create(_snakes.Count, _keycardColors.Keys, _gateColors.Keys, _buttonColors.Keys, _wallColors.Keys, _initialCountdowns);
        LevelSolveReport greedyReport = RunGreedy(state.Clone());
        if (greedyReport.solved || IsMonotonicModel())
        {
            return greedyReport;
        }

        List<int> solution = new List<int>();

        if (Search(state, solution))
        {
            return new LevelSolveReport
            {
                solved = true,
                steps = solution.Count,
                searchedNodes = _searchedNodes,
                message = "Release order: " + string.Join(", ", solution)
            };
        }

        string reason = _searchedNodes >= _nodeLimit ? "Solver node limit reached." : "No complete release order found.";
        return new LevelSolveReport
        {
            solved = false,
            steps = _bestPartial.Count,
            searchedNodes = _searchedNodes,
            message = reason + " Best partial: " + string.Join(", ", _bestPartial)
        };
    }

    private LevelSolveReport RunGreedy(SolveState state)
    {
        List<int> solution = new List<int>();
        _searchedNodes = 0;
        _bestPartial.Clear();

        while (state.exitedCount < _snakes.Count)
        {
            _searchedNodes++;
            List<ReleaseCandidate> candidates = FindReleaseCandidates(state);
            if (candidates.Count == 0)
            {
                _bestPartial.Clear();
                _bestPartial.AddRange(solution);
                return new LevelSolveReport
                {
                    solved = false,
                    steps = solution.Count,
                    searchedNodes = _searchedNodes,
                    message = "No releasable arrow found. Released: " + string.Join(", ", solution)
                };
            }

            candidates.Sort(CompareCandidates);
            int snakeIndex = candidates[0].snakeIndex;
            state = ApplyRelease(state, snakeIndex);
            solution.Add(snakeIndex);
        }

        return new LevelSolveReport
        {
            solved = true,
            steps = solution.Count,
            searchedNodes = _searchedNodes,
            message = "Release order: " + string.Join(", ", solution)
        };
    }

    private bool IsMonotonicModel()
    {
        // Current gameplay rules only remove blockers: snakes exit, gates/walls open,
        // keycards/buttons are consumed, and countdown blocks tick down. Greedy is
        // therefore enough and avoids exploding on large classic levels.
        return true;
    }

    private bool Search(SolveState state, List<int> solution)
    {
        _searchedNodes++;
        if (_searchedNodes > _nodeLimit) return false;

        if (state.exitedCount >= _snakes.Count) return true;
        if (solution.Count > _bestPartial.Count)
        {
            _bestPartial.Clear();
            _bestPartial.AddRange(solution);
        }

        string hash = BuildStateHash(state);
        if (!_visitedStates.Add(hash)) return false;

        List<ReleaseCandidate> candidates = FindReleaseCandidates(state);
        if (candidates.Count == 0) return false;
        candidates.Sort(CompareCandidates);

        for (int i = 0; i < candidates.Count; i++)
        {
            int snakeIndex = candidates[i].snakeIndex;
            SolveState next = ApplyRelease(state, snakeIndex);
            solution.Add(snakeIndex);

            if (Search(next, solution)) return true;

            solution.RemoveAt(solution.Count - 1);
            if (_searchedNodes > _nodeLimit) return false;
        }

        return false;
    }

    private List<ReleaseCandidate> FindReleaseCandidates(SolveState state)
    {
        List<ReleaseCandidate> candidates = new List<ReleaseCandidate>();
        for (int i = 0; i < _snakes.Count; i++)
        {
            if (state.exited[i]) continue;
            ReleaseCandidate candidate;
            if (CanRelease(state, _snakes[i], out candidate)) candidates.Add(candidate);
        }

        return candidates;
    }

    private bool CanRelease(SolveState state, SnakeState snake, out ReleaseCandidate candidate)
    {
        candidate = new ReleaseCandidate { snakeIndex = snake.id };
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Vector2Int current = snake.cells[0];
        ArrowDir dir = snake.direction;
        HashSet<Vector2Int> previewGates = new HashSet<Vector2Int>(state.gates);
        HashSet<Vector2Int> previewWalls = new HashSet<Vector2Int>(state.walls);
        int scanLimit = GetScanLimit();

        for (int i = 0; i < scanLimit; i++)
        {
            Vector2Int step = Step(dir);
            Vector3Int scanState = new Vector3Int(current.x, current.y, DirectionKey(dir));
            if (!visited.Add(scanState)) return false;

            Vector2Int next = current + step;
            if (IsOutsidePlayArea(next)) return true;

            if (_snakeCells.TryGetValue(next, out int snakeId) && snakeId != snake.id && !state.exited[snakeId]) return false;
            if (previewGates.Contains(next)) return false;
            if (previewWalls.Contains(next)) return false;
            if (state.countdowns.ContainsKey(next)) return false;

            if (state.keycards.Contains(next) && _keycardColors.TryGetValue(next, out Color keyColor))
            {
                int removed = RemoveMatchingColors(previewGates, _gateColors, keyColor);
                candidate.openedBlockers += removed;
                candidate.collectedMechanics++;
            }

            if (state.buttons.Contains(next) && _buttonColors.TryGetValue(next, out Color buttonColor))
            {
                int removed = RemoveMatchingColors(previewWalls, _wallColors, buttonColor);
                candidate.openedBlockers += removed;
                candidate.collectedMechanics++;
            }

            if (_portals.TryGetValue(next, out PortalNode portal))
            {
                candidate.usedMechanics++;
                current = portal.exit;
                dir = portal.exitDir;
                continue;
            }

            if (_deflectors.TryGetValue(next, out ArrowDir deflectDir))
            {
                candidate.usedMechanics++;
                dir = deflectDir;
            }

            current = next;
        }

        return true;
    }

    private SolveState ApplyRelease(SolveState state, int snakeIndex)
    {
        SolveState nextState = state.Clone();
        nextState.exited[snakeIndex] = true;
        nextState.exitedCount++;

        SnakeState snake = _snakes[snakeIndex];
        Vector2Int current = snake.cells[0];
        ArrowDir dir = snake.direction;
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        int scanLimit = GetScanLimit();

        for (int i = 0; i < scanLimit; i++)
        {
            Vector2Int step = Step(dir);
            Vector3Int scanState = new Vector3Int(current.x, current.y, DirectionKey(dir));
            if (!visited.Add(scanState)) break;

            Vector2Int next = current + step;
            if (IsOutsidePlayArea(next)) break;

            if (nextState.keycards.Contains(next) && _keycardColors.TryGetValue(next, out Color keyColor))
            {
                nextState.keycards.Remove(next);
                RemoveMatchingColors(nextState.gates, _gateColors, keyColor);
            }

            if (nextState.buttons.Contains(next) && _buttonColors.TryGetValue(next, out Color buttonColor))
            {
                nextState.buttons.Remove(next);
                RemoveMatchingColors(nextState.walls, _wallColors, buttonColor);
            }

            if (_portals.TryGetValue(next, out PortalNode portal))
            {
                current = portal.exit;
                dir = portal.exitDir;
                continue;
            }

            if (_deflectors.TryGetValue(next, out ArrowDir deflectDir)) dir = deflectDir;
            current = next;
        }

        TickCountdownBlocks(nextState);
        return nextState;
    }

    private void TickCountdownBlocks(SolveState state)
    {
        if (state.countdowns.Count == 0) return;

        List<Vector2Int> keys = new List<Vector2Int>(state.countdowns.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            Vector2Int key = keys[i];
            int count = state.countdowns[key] - 1;
            if (count <= 0) state.countdowns.Remove(key);
            else state.countdowns[key] = count;
        }
    }

    private string BuildStateHash(SolveState state)
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int i = 0; i < state.exited.Length; i++) builder.Append(state.exited[i] ? '1' : '0');
        AppendCells(builder, state.gates, "g");
        AppendCells(builder, state.walls, "w");
        AppendCells(builder, state.keycards, "k");
        AppendCells(builder, state.buttons, "b");
        AppendCountdowns(builder, state.countdowns);
        return builder.ToString();
    }

    private static void AppendCells(System.Text.StringBuilder builder, HashSet<Vector2Int> cells, string prefix)
    {
        List<Vector2Int> sorted = new List<Vector2Int>(cells);
        sorted.Sort(CompareCells);
        builder.Append('|').Append(prefix);
        for (int i = 0; i < sorted.Count; i++) builder.Append(sorted[i].x).Append(',').Append(sorted[i].y).Append(';');
    }

    private static void AppendCountdowns(System.Text.StringBuilder builder, Dictionary<Vector2Int, int> countdowns)
    {
        List<Vector2Int> sorted = new List<Vector2Int>(countdowns.Keys);
        sorted.Sort(CompareCells);
        builder.Append("|c");
        for (int i = 0; i < sorted.Count; i++)
        {
            Vector2Int cell = sorted[i];
            builder.Append(cell.x).Append(',').Append(cell.y).Append(':').Append(countdowns[cell]).Append(';');
        }
    }

    private static int CompareCandidates(ReleaseCandidate a, ReleaseCandidate b)
    {
        int scoreA = a.openedBlockers * 10 + a.collectedMechanics * 3 + a.usedMechanics;
        int scoreB = b.openedBlockers * 10 + b.collectedMechanics * 3 + b.usedMechanics;
        int scoreCompare = scoreB.CompareTo(scoreA);
        if (scoreCompare != 0) return scoreCompare;
        return a.snakeIndex.CompareTo(b.snakeIndex);
    }

    private static int CompareCells(Vector2Int a, Vector2Int b)
    {
        int x = a.x.CompareTo(b.x);
        return x != 0 ? x : a.y.CompareTo(b.y);
    }

    private int GetScanLimit()
    {
        return 240;
    }

    private static bool IsOutsidePlayArea(Vector2Int cell)
    {
        return Mathf.Abs(cell.x) > 100 || Mathf.Abs(cell.y) > 100;
    }

    private static int RemoveMatchingColors(HashSet<Vector2Int> activeCells, Dictionary<Vector2Int, Color> colorMap, Color color)
    {
        List<Vector2Int> remove = new List<Vector2Int>();
        foreach (Vector2Int cell in activeCells)
        {
            if (colorMap.TryGetValue(cell, out Color targetColor) && ColorsMatch(targetColor, color)) remove.Add(cell);
        }

        for (int i = 0; i < remove.Count; i++) activeCells.Remove(remove[i]);
        return remove.Count;
    }

    public static bool ColorsMatch(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.1f && Mathf.Abs(a.g - b.g) < 0.1f && Mathf.Abs(a.b - b.b) < 0.1f;
    }

    private static IEnumerable<Vector2Int> CellsOnLine(Vector2Int start, Vector2Int end)
    {
        Vector2Int delta = end - start;
        int steps = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
        if (steps == 0)
        {
            yield return start;
            yield break;
        }

        Vector2Int step = new Vector2Int(delta.x == 0 ? 0 : delta.x / Mathf.Abs(delta.x), delta.y == 0 ? 0 : delta.y / Mathf.Abs(delta.y));
        Vector2Int current = start;
        for (int i = 0; i <= steps; i++)
        {
            yield return current;
            current += step;
        }
    }

    private static Vector2Int Step(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: return Vector2Int.up;
            case ArrowDir.Down: return Vector2Int.down;
            case ArrowDir.Left: return Vector2Int.left;
            case ArrowDir.Right: return Vector2Int.right;
            default: return Vector2Int.zero;
        }
    }

    private static int DirectionKey(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: return 0;
            case ArrowDir.Down: return 1;
            case ArrowDir.Left: return 2;
            case ArrowDir.Right: return 3;
            default: return 4;
        }
    }

    private struct SnakeState
    {
        public int id;
        public ArrowDir direction;
        public List<Vector2Int> cells;
    }

    private struct PortalNode
    {
        public Vector2Int exit;
        public ArrowDir exitDir;
    }

    private struct ReleaseCandidate
    {
        public int snakeIndex;
        public int openedBlockers;
        public int collectedMechanics;
        public int usedMechanics;
    }

    private sealed class SolveState
    {
        public bool[] exited;
        public int exitedCount;
        public HashSet<Vector2Int> keycards;
        public HashSet<Vector2Int> gates;
        public HashSet<Vector2Int> buttons;
        public HashSet<Vector2Int> walls;
        public Dictionary<Vector2Int, int> countdowns;

        public static SolveState Create(int snakeCount, ICollection<Vector2Int> keycards, ICollection<Vector2Int> gates, ICollection<Vector2Int> buttons, ICollection<Vector2Int> walls, Dictionary<Vector2Int, int> countdowns)
        {
            SolveState state = new SolveState();
            state.exited = new bool[snakeCount];
            state.exitedCount = 0;
            state.keycards = new HashSet<Vector2Int>(keycards);
            state.gates = new HashSet<Vector2Int>(gates);
            state.buttons = new HashSet<Vector2Int>(buttons);
            state.walls = new HashSet<Vector2Int>(walls);
            state.countdowns = new Dictionary<Vector2Int, int>(countdowns);
            return state;
        }

        public SolveState Clone()
        {
            SolveState clone = new SolveState();
            clone.exited = new bool[exited.Length];
            System.Array.Copy(exited, clone.exited, exited.Length);
            clone.exitedCount = exitedCount;
            clone.keycards = new HashSet<Vector2Int>(keycards);
            clone.gates = new HashSet<Vector2Int>(gates);
            clone.buttons = new HashSet<Vector2Int>(buttons);
            clone.walls = new HashSet<Vector2Int>(walls);
            clone.countdowns = new Dictionary<Vector2Int, int>(countdowns);
            return clone;
        }
    }
}
