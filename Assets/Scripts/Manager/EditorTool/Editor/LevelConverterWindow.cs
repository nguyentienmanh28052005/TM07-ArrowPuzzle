using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class LevelConverterWindow : EditorWindow
{
    private string sourceFolder = "Assets/Resources/LevelOld";
    private string targetFolder = "Assets/Resources/Levels";

    [MenuItem("Tools/Level Converter")]
    public static void ShowWindow()
    {
        GetWindow<LevelConverterWindow>("Level Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Convert LevelDataSO (Old) to LevelDataV2 (New)", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        sourceFolder = EditorGUILayout.TextField("Source Folder", sourceFolder);
        if (GUILayout.Button("Browse Source"))
        {
            string path = EditorUtility.OpenFolderPanel("Select Source Folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                sourceFolder = GetRelativePath(path);
            }
        }

        EditorGUILayout.Space();

        targetFolder = EditorGUILayout.TextField("Target Folder", targetFolder);
        if (GUILayout.Button("Browse Target"))
        {
            string path = EditorUtility.OpenFolderPanel("Select Target Folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                targetFolder = GetRelativePath(path);
            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Convert All in Source Folder"))
        {
            ConvertAll();
        }

        if (GUILayout.Button("Convert Selected Assets"))
        {
            ConvertSelected();
        }
    }

    private string GetRelativePath(string absolutePath)
    {
        string normalizedAppPath = Application.dataPath.Replace("\\", "/");
        string normalizedAbsPath = absolutePath.Replace("\\", "/");

        if (normalizedAbsPath.StartsWith(normalizedAppPath))
        {
            return "Assets" + normalizedAbsPath.Substring(normalizedAppPath.Length);
        }
        return absolutePath;
    }

    private void ConvertAll()
    {
        if (!Directory.Exists(sourceFolder))
        {
            Debug.LogError($"Source folder does not exist: {sourceFolder}");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:LevelDataSO", new[] { sourceFolder });
        int count = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            LevelDataSO oldLevel = AssetDatabase.LoadAssetAtPath<LevelDataSO>(assetPath);
            if (oldLevel != null)
            {
                ConvertSingle(oldLevel, assetPath);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Successfully converted {count} levels!");
    }

    private void ConvertSelected()
    {
        var selectedObjects = Selection.objects;
        int count = 0;

        foreach (var obj in selectedObjects)
        {
            if (obj is LevelDataSO oldLevel)
            {
                string assetPath = AssetDatabase.GetAssetPath(oldLevel);
                ConvertSingle(oldLevel, assetPath);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Successfully converted {count} selected levels!");
    }

    private void ConvertSingle(LevelDataSO oldLevel, string oldAssetPath)
    {
        // Normalize paths for matching
        string normSource = sourceFolder.Replace("\\", "/").TrimEnd('/');
        string normTarget = targetFolder.Replace("\\", "/").TrimEnd('/');
        string normOldPath = oldAssetPath.Replace("\\", "/");
        string normSourcePrefix = normSource + "/";

        if (!normOldPath.StartsWith(normSourcePrefix))
        {
            Debug.LogError($"Asset {oldAssetPath} is not within source folder {sourceFolder}");
            return;
        }

        string relativePath = normOldPath.Substring(normSourcePrefix.Length);
        string newAssetPath = Path.Combine(normTarget, relativePath).Replace("\\", "/");

        // Ensure directory exists
        string directory = Path.GetDirectoryName(newAssetPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }

        // Try load existing LevelDataV2 at path or create new one
        LevelDataV2 newLevel = AssetDatabase.LoadAssetAtPath<LevelDataV2>(newAssetPath);
        bool isNew = false;
        if (newLevel == null)
        {
            newLevel = ScriptableObject.CreateInstance<LevelDataV2>();
            isNew = true;
        }

        // Copy metadata
        newLevel.levelIndex = oldLevel.levelIndex;
        newLevel.gameMode = oldLevel.gameMode;
        newLevel.levelDifficulty = oldLevel.levelDifficulty;
        newLevel.returnToDefaultZoomAfterIntro = oldLevel.returnToDefaultZoomAfterIntro;
        newLevel.timeLimit = oldLevel.timeLimit;
        newLevel.rewardCoins = oldLevel.rewardCoins;
        newLevel.rewardDiamonds = oldLevel.rewardDiamonds;

        // Clear existing new content
        LevelDataV2Writer.ClearContent(newLevel);

        // Convert Snakes -> Arrows
        if (oldLevel.snakes != null)
        {
            foreach (var snake in oldLevel.snakes)
            {
                LevelDataV2Writer.AddSnake(newLevel, snake);
            }
        }

        // Convert Keycards -> Cells
        if (oldLevel.keycards != null)
        {
            foreach (var keycard in oldLevel.keycards)
            {
                LevelDataV2Writer.AddCell(newLevel, CellTypeIds.Keycard, keycard.position, ArrowDir.Up, keycard.color, new ColorCellPayload());
            }
        }

        // Convert Gates -> Cells
        if (oldLevel.gates != null)
        {
            foreach (var gate in oldLevel.gates)
            {
                LevelDataV2Writer.AddCell(newLevel, CellTypeIds.Gate, gate.position, ArrowDir.Up, gate.color, new ColorCellPayload());
            }
        }

        // List to collect buttons and walls for linking later
        List<CellEntityData> electricButtons = new List<CellEntityData>();
        List<CellEntityData> electricWalls = new List<CellEntityData>();

        // Convert Electric Buttons -> Cells
        if (oldLevel.electricButtons != null)
        {
            foreach (var btn in oldLevel.electricButtons)
            {
                var btnCell = LevelDataV2Writer.AddCell(newLevel, CellTypeIds.ElectricButton, btn.position, ArrowDir.Up, btn.color, new ColorCellPayload());
                electricButtons.Add(btnCell);
            }
        }

        // Convert Reveal Wave Buttons -> Cells
        if (oldLevel.revealWaveButtons != null)
        {
            foreach (var btn in oldLevel.revealWaveButtons)
            {
                LevelDataV2Writer.AddCell(newLevel, CellTypeIds.RevealWaveButton, btn.position, ArrowDir.Up, btn.color, new ColorCellPayload());
            }
        }

        // Convert Deflectors -> Cells
        if (oldLevel.deflectors != null)
        {
            foreach (var def in oldLevel.deflectors)
            {
                LevelDataV2Writer.AddCell(newLevel, CellTypeIds.Deflector, def.position, def.direction, Color.white, new DirectionCellPayload());
            }
        }

        // Convert Countdown Blocks -> Cells
        if (oldLevel.countdownBlocks != null)
        {
            foreach (var block in oldLevel.countdownBlocks)
            {
                LevelDataV2Writer.AddCell(newLevel, CellTypeIds.CountdownBlock, block.position, ArrowDir.Up, Color.white, new CountCellPayload { count = block.count });
            }
        }

        // Convert Stop Blocks -> Cells
        if (oldLevel.stopBlocks != null)
        {
            foreach (var block in oldLevel.stopBlocks)
            {
                LevelDataV2Writer.AddCell(newLevel, CellTypeIds.StopBlock, block.position, ArrowDir.Up, Color.white, new CountCellPayload { count = block.count });
            }
        }

        // Convert Turn State Blocks -> Cells
        if (oldLevel.turnStateBlocks != null)
        {
            foreach (var block in oldLevel.turnStateBlocks)
            {
                LevelDataV2Writer.AddCell(newLevel, CellTypeIds.TurnStateBlock, block.position, ArrowDir.Up, Color.white, new TurnStatePayload { startsRed = block.startsRed });
            }
        }

        // Convert Black Holes -> Cells
        if (oldLevel.blackHoles != null)
        {
            foreach (var bh in oldLevel.blackHoles)
            {
                LevelDataV2Writer.AddCell(newLevel, CellTypeIds.BlackHole, bh.position, bh.direction, Color.white, new DirectionCellPayload());
            }
        }

        // Convert Electric Walls -> Cells
        if (oldLevel.electricWalls != null)
        {
            foreach (var wall in oldLevel.electricWalls)
            {
                var wallCell = LevelDataV2Writer.AddCell(newLevel, CellTypeIds.ElectricWall, wall.start, ArrowDir.Up, wall.color, new ElectricWallPayload { start = wall.start, end = wall.end });
                electricWalls.Add(wallCell);
            }
        }

        // Link Electric Buttons & Walls
        for (int i = 0; i < electricButtons.Count; i++)
        {
            for (int j = 0; j < electricWalls.Count; j++)
            {
                if (!LevelEditorRuntimeHelpers.ColorsMatch(electricButtons[i].color, electricWalls[j].color)) continue;
                LevelDataV2Writer.AddLink(newLevel, LinkTypeIds.ElectricButtonWall, electricButtons[i].entityId, electricWalls[j].entityId, new ElectricButtonWallPayload { color = electricWalls[j].color });
            }
        }

        // Convert Portals -> Cells & Links
        if (oldLevel.portals != null)
        {
            foreach (var portal in oldLevel.portals)
            {
                CellEntityData entrance = LevelDataV2Writer.AddCell(newLevel, CellTypeIds.Portal, portal.entrance, portal.entranceDir, portal.portalColor, new PortalEndpointPayload { exitDirection = portal.entranceDir });
                CellEntityData exit = LevelDataV2Writer.AddCell(newLevel, CellTypeIds.Portal, portal.exit, portal.exitDir, portal.portalColor, new PortalEndpointPayload { exitDirection = portal.exitDir });
                LevelDataV2Writer.AddLink(newLevel, LinkTypeIds.PortalPair, entrance.entityId, exit.entityId, new PortalPairPayload { color = portal.portalColor });
            }
        }

        if (isNew)
        {
            AssetDatabase.CreateAsset(newLevel, newAssetPath);
        }
        else
        {
            EditorUtility.SetDirty(newLevel);
        }
    }
}
