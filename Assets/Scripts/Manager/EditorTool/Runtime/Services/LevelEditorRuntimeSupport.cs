using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LevelEditorContext
{
    public LevelDataV2 currentData;
    public Transform levelContainer;

    public GameObject snakePrefab;
    public GameObject portalPrefab;
    public GameObject keycardPrefab;
    public GameObject gatePrefab;
    public GameObject electricButtonPrefab;
    public GameObject revealWaveButtonPrefab;
    public GameObject electricWallPrefab;
    public GameObject deflectorPrefab;
    public GameObject countdownBlockPrefab;
    public GameObject stopBlockPrefab;
    public GameObject turnStateBlockPrefab;
    public GameObject blackHolePrefab;

    public GameObject currentSnakeObj;
    public EditorSnakeVisual currentSnakeScript;
    public GameObject currentSelectionGlowObj;

    public List<Vector2Int> currentDraftNodes;
    public List<PortalData> currentDraftPortals;
    public List<GameObject> spawnedPortalVisuals;
    public List<ElectricWallSaveData> currentDraftElectricWalls;
    public List<GameObject> spawnedElectricWallVisuals;
    public Stack<GameObject> finishedSnakesHistory;

    public TMP_InputField inputTimeLimit;
    public TMP_InputField inputRewardCoins;
    public TMP_InputField inputRewardDiamonds;

    public ArrowDir currentDir;
    public Color currentColor;

    public bool isPlacingPortalExit;
    public Vector2Int draftPortalEntrance;
    public ArrowDir draftPortalEntranceDir;
    public Color draftPortalColor;

    public bool isPlacingElectricWallEnd;
    public Vector2Int draftElectricWallStart;
    public Color draftElectricWallColor;
}

public static class LevelEditorRuntimeHelpers
{
    public static Quaternion GetRotationForDir(ArrowDir dir)
    {
        float angle = 0f;
        switch (dir)
        {
            case ArrowDir.Up: angle = 0f; break;
            case ArrowDir.Down: angle = 180f; break;
            case ArrowDir.Left: angle = 90f; break;
            case ArrowDir.Right: angle = -90f; break;
        }

        return Quaternion.Euler(0f, 0f, angle);
    }

    public static Vector2Int GetDirStep(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: return new Vector2Int(0, 1);
            case ArrowDir.Down: return new Vector2Int(0, -1);
            case ArrowDir.Left: return new Vector2Int(-1, 0);
            case ArrowDir.Right: return new Vector2Int(1, 0);
            default: return Vector2Int.zero;
        }
    }

    public static int GetStepKey(Vector2Int step)
    {
        if (step.y > 0) return 0;
        if (step.y < 0) return 1;
        if (step.x < 0) return 2;
        return 3;
    }

    public static bool IsElectricWallAligned(Vector2Int start, Vector2Int end)
    {
        return start.x == end.x || start.y == end.y;
    }

    public static bool IsCellOnElectricWall(Vector2Int cell, ElectricWallSaveData wall)
    {
        if (!IsElectricWallAligned(wall.start, wall.end)) return false;

        if (wall.start.x == wall.end.x)
        {
            if (cell.x != wall.start.x) return false;
            int minY = Mathf.Min(wall.start.y, wall.end.y);
            int maxY = Mathf.Max(wall.start.y, wall.end.y);
            return cell.y >= minY && cell.y <= maxY;
        }

        if (cell.y != wall.start.y) return false;
        int minX = Mathf.Min(wall.start.x, wall.end.x);
        int maxX = Mathf.Max(wall.start.x, wall.end.x);
        return cell.x >= minX && cell.x <= maxX;
    }

    public static ArrowDir GetClockwiseDirection(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: return ArrowDir.Right;
            case ArrowDir.Right: return ArrowDir.Down;
            case ArrowDir.Down: return ArrowDir.Left;
            case ArrowDir.Left: return ArrowDir.Up;
            default: return ArrowDir.Up;
        }
    }

    public static ArrowDir GetOppositeDirection(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: return ArrowDir.Down;
            case ArrowDir.Down: return ArrowDir.Up;
            case ArrowDir.Left: return ArrowDir.Right;
            case ArrowDir.Right: return ArrowDir.Left;
            default: return ArrowDir.Up;
        }
    }

    public static bool ColorsMatch(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.1f
            && Mathf.Abs(a.g - b.g) < 0.1f
            && Mathf.Abs(a.b - b.b) < 0.1f;
    }

    public static string FormatCell(Vector2Int cell)
    {
        return $"({cell.x}, {cell.y})";
    }

    public static string FormatColor(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGB(color)}";
    }
}
