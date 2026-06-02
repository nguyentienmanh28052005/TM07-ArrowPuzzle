using System.IO;
using UnityEditor;
using UnityEngine;

public class ArrowLevelGeneratorWindow : EditorWindow
{
    private const string DefaultOutputFolder = "Assets/ScriptableObjects/Levels";

    private int levelIndex = 1000;
    private GameMode gameMode = GameMode.Classic;
    private LevelDifficulty levelDifficulty = LevelDifficulty.Medium;
    private bool returnToDefaultZoomAfterIntro = true;
    private float timeLimit = 60f;
    private float rewardCoins = 100f;
    private float rewardDiamonds = 5f;

    private int width = 9;
    private int height = 9;
    private bool centerGridOnOrigin = true;
    private int originX = -4;
    private int originY = -4;

    private int targetArrowCount = 24;
    private int minSnakeLength = 3;
    private int maxSnakeLength = 5;
    private int maxAttemptsPerArrow = 512;
    private int bodyAttemptsPerCandidate = 8;
    private int minDistanceBetweenSnakes = 2;
    private int minStraightCellsPerSegment = 3;
    private bool fillAvailableArea = true;
    private int fillSearchAttempts = 2048;
    private int fillLayoutAttempts = 12;
    private bool allowBentSnakes = true;
    private int turnChancePercent = 65;
    private bool useRandomSeed = true;
    private int seed = 12345;

    private string outputFolder = DefaultOutputFolder;
    private string filePrefix = "GeneratedLevel";

    private LevelGeneratorCore.Result lastResult;
    private string lastAssetPath;

    [MenuItem("Tools/Arrow Escape/Procedural Level Generator")]
    public static void Open()
    {
        ArrowLevelGeneratorWindow window = GetWindow<ArrowLevelGeneratorWindow>("Arrow Level Generator");
        window.minSize = new Vector2(380f, 520f);
        window.Show();
    }

    private void OnGUI()
    {
        DrawLevelDataSection();
        EditorGUILayout.Space(8f);
        DrawGridSection();
        EditorGUILayout.Space(8f);
        DrawGenerationSection();
        EditorGUILayout.Space(8f);
        DrawOutputSection();
        EditorGUILayout.Space(12f);
        DrawActions();
        EditorGUILayout.Space(8f);
        DrawResult();
    }

    private void DrawLevelDataSection()
    {
        EditorGUILayout.LabelField("Level Data", EditorStyles.boldLabel);
        levelIndex = EditorGUILayout.IntField("Level Index", levelIndex);
        gameMode = (GameMode)EditorGUILayout.EnumPopup("Game Mode", gameMode);
        levelDifficulty = (LevelDifficulty)EditorGUILayout.EnumPopup("Difficulty", levelDifficulty);
        returnToDefaultZoomAfterIntro = EditorGUILayout.Toggle("Return To Default Zoom", returnToDefaultZoomAfterIntro);
        timeLimit = EditorGUILayout.FloatField("Time Limit", timeLimit);
        rewardCoins = EditorGUILayout.FloatField("Reward Coins", rewardCoins);
        rewardDiamonds = EditorGUILayout.FloatField("Reward Diamonds", rewardDiamonds);
    }

    private void DrawGridSection()
    {
        EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
        width = Mathf.Max(1, EditorGUILayout.IntField("Width", width));
        height = Mathf.Max(1, EditorGUILayout.IntField("Height", height));
        centerGridOnOrigin = EditorGUILayout.Toggle("Center Grid On Origin", centerGridOnOrigin);

        if (centerGridOnOrigin)
        {
            originX = -width / 2;
            originY = -height / 2;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField("Origin X", originX);
                EditorGUILayout.IntField("Origin Y", originY);
            }
        }
        else
        {
            originX = EditorGUILayout.IntField("Origin X", originX);
            originY = EditorGUILayout.IntField("Origin Y", originY);
        }
    }

    private void DrawGenerationSection()
    {
        EditorGUILayout.LabelField("Generation", EditorStyles.boldLabel);
        fillAvailableArea = EditorGUILayout.Toggle("Fill Placement Area", fillAvailableArea);
        using (new EditorGUI.DisabledScope(fillAvailableArea))
        {
            targetArrowCount = Mathf.Max(1, EditorGUILayout.IntField("Target Arrow Count", targetArrowCount));
        }

        minSnakeLength = MakeOddAtLeast(EditorGUILayout.IntField("Min Snake Length", minSnakeLength), 3);
        maxSnakeLength = MakeOddAtLeast(EditorGUILayout.IntField("Max Snake Length", maxSnakeLength), minSnakeLength);
        maxAttemptsPerArrow = Mathf.Max(32, EditorGUILayout.IntField("Max Attempts Per Arrow", maxAttemptsPerArrow));
        bodyAttemptsPerCandidate = Mathf.Clamp(EditorGUILayout.IntField("Body Attempts", bodyAttemptsPerCandidate), 1, 64);
        using (new EditorGUI.DisabledScope(!fillAvailableArea))
        {
            fillSearchAttempts = Mathf.Max(maxAttemptsPerArrow, EditorGUILayout.IntField("Fill Search Attempts", fillSearchAttempts));
            fillLayoutAttempts = Mathf.Clamp(EditorGUILayout.IntField("Fill Layout Attempts", fillLayoutAttempts), 1, 64);
        }

        minDistanceBetweenSnakes = Mathf.Max(2, EditorGUILayout.IntField("Snake Spacing", minDistanceBetweenSnakes));
        minStraightCellsPerSegment = MakeOddAtLeast(EditorGUILayout.IntField("Min Cells Per Turn Segment", minStraightCellsPerSegment), 3);
        allowBentSnakes = EditorGUILayout.Toggle("Allow Bent Snakes", allowBentSnakes);
        using (new EditorGUI.DisabledScope(!allowBentSnakes))
        {
            turnChancePercent = EditorGUILayout.IntSlider("Turn Chance", turnChancePercent, 0, 100);
        }
        useRandomSeed = EditorGUILayout.Toggle("Random Seed", useRandomSeed);

        using (new EditorGUI.DisabledScope(useRandomSeed))
        {
            seed = EditorGUILayout.IntField("Seed", seed);
        }
    }

    private void DrawOutputSection()
    {
        EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
        outputFolder = EditorGUILayout.TextField("Folder", outputFolder);
        filePrefix = EditorGUILayout.TextField("File Prefix", filePrefix);
    }

    private void DrawActions()
    {
        using (new EditorGUI.DisabledScope(!CanGenerate()))
        {
            if (GUILayout.Button("Generate Level Asset", GUILayout.Height(34f)))
            {
                GenerateAndSave();
            }
        }
    }

    private void DrawResult()
    {
        if (lastResult == null)
        {
            return;
        }

        MessageType type = lastResult.success ? MessageType.Info : MessageType.Warning;
        EditorGUILayout.HelpBox(lastResult.message, type);
        EditorGUILayout.LabelField("Placed Arrows", lastResult.placedArrowCount.ToString());
        EditorGUILayout.LabelField("Bent Arrows", lastResult.bentArrowCount.ToString());
        EditorGUILayout.LabelField("Direction Types", lastResult.directionTypeCount.ToString());
        EditorGUILayout.LabelField("Shape Types", lastResult.shapeTypeCount.ToString());
        EditorGUILayout.LabelField("Shape Mix", FormatShapeMix(lastResult));
        EditorGUILayout.LabelField("Occupied Cells", lastResult.occupiedCellCount.ToString());
        if (lastResult.placementAreaCellCount > 0)
        {
            float fillPercent = (float)lastResult.occupiedCellCount / lastResult.placementAreaCellCount * 100f;
            EditorGUILayout.LabelField("Placement Fill", $"{fillPercent:0.0}%");
        }

        if (!string.IsNullOrEmpty(lastAssetPath))
        {
            EditorGUILayout.SelectableLabel(lastAssetPath, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        }
    }

    private bool CanGenerate()
    {
        return width > 0
            && height > 0
            && targetArrowCount > 0
            && minSnakeLength > 0
            && maxSnakeLength >= minSnakeLength
            && !string.IsNullOrWhiteSpace(outputFolder)
            && !string.IsNullOrWhiteSpace(filePrefix);
    }

    private void GenerateAndSave()
    {
        int effectiveSeed = useRandomSeed ? unchecked((int)System.DateTime.UtcNow.Ticks) : seed;

        LevelGeneratorCore.Settings settings = new LevelGeneratorCore.Settings
        {
            width = width,
            height = height,
            targetArrowCount = targetArrowCount,
            minSnakeLength = minSnakeLength,
            maxSnakeLength = maxSnakeLength,
            maxAttemptsPerArrow = maxAttemptsPerArrow,
            bodyAttemptsPerCandidate = bodyAttemptsPerCandidate,
            minDistanceBetweenSnakes = minDistanceBetweenSnakes,
            minStraightCellsPerSegment = minStraightCellsPerSegment,
            fillAvailableArea = fillAvailableArea,
            fillSearchAttempts = fillSearchAttempts,
            fillLayoutAttempts = fillLayoutAttempts,
            allowBentSnakes = allowBentSnakes,
            turnChancePercent = turnChancePercent,
            seed = effectiveSeed,
            originX = originX,
            originY = originY
        };

        lastResult = LevelGeneratorCore.Generate(settings);
        seed = effectiveSeed;

        LevelDataSO level = CreateInstance<LevelDataSO>();
        level.levelIndex = levelIndex;
        level.gameMode = gameMode;
        level.levelDifficulty = levelDifficulty;
        level.returnToDefaultZoomAfterIntro = returnToDefaultZoomAfterIntro;
        level.timeLimit = Mathf.Max(0f, timeLimit);
        level.rewardCoins = Mathf.Max(0f, rewardCoins);
        level.rewardDiamonds = Mathf.Max(0f, rewardDiamonds);
        level.snakes = lastResult.snakes;

        level.keycards = new System.Collections.Generic.List<KeycardSaveData>();
        level.gates = new System.Collections.Generic.List<GateSaveData>();
        level.electricButtons = new System.Collections.Generic.List<ElectricButtonSaveData>();
        level.electricWalls = new System.Collections.Generic.List<ElectricWallSaveData>();
        level.portals = new System.Collections.Generic.List<PortalData>();
        level.deflectors = new System.Collections.Generic.List<DeflectorSaveData>();
        level.countdownBlocks = new System.Collections.Generic.List<CountdownBlockSaveData>();

        string folder = NormalizeAssetFolder(outputFolder);
        EnsureAssetFolder(folder);

        string safePrefix = MakeSafeFileName(filePrefix);
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{safePrefix}_{levelIndex}.asset");

        AssetDatabase.CreateAsset(level, assetPath);
        EditorUtility.SetDirty(level);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = level;
        EditorGUIUtility.PingObject(level);
        lastAssetPath = assetPath;
    }

    private static string NormalizeAssetFolder(string folder)
    {
        folder = folder.Replace('\\', '/').Trim();
        if (folder.EndsWith("/"))
        {
            folder = folder.Substring(0, folder.Length - 1);
        }

        return folder.StartsWith("Assets") ? folder : DefaultOutputFolder;
    }

    private static void EnsureAssetFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string[] parts = folder.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }

    private static string MakeSafeFileName(string value)
    {
        string fileName = value.Trim();
        char[] invalidChars = Path.GetInvalidFileNameChars();
        for (int i = 0; i < invalidChars.Length; i++)
        {
            fileName = fileName.Replace(invalidChars[i], '_');
        }

        return string.IsNullOrEmpty(fileName) ? "GeneratedLevel" : fileName;
    }

    private static int MakeOddAtLeast(int value, int minimum)
    {
        value = Mathf.Max(minimum, value);
        return (value & 1) == 0 ? value + 1 : value;
    }

    private static string FormatShapeMix(LevelGeneratorCore.Result result)
    {
        return $"Straight {result.straightShapeCount}, L {result.lShapeCount}, U {result.uShapeCount}, Zigzag {result.zigzagShapeCount}, Random {result.randomBentShapeCount}";
    }
}
