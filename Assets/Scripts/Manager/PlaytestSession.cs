using UnityEngine;

public static class PlaytestSession
{
    public static bool IsActive;
    public static LevelDataSO LevelData;
    public static string ReturnSceneName;
    public static string ReturnScenePath;
    public static string RequestedSceneName;
    public static bool SuppressProgression;

    public static bool IsPlaytesting => IsActive && LevelData != null;

    public static void StartPlaytest(LevelDataSO levelData, string returnSceneName, string returnScenePath, string requestedSceneName)
    {
        if (levelData == null)
        {
            Debug.LogWarning("[PlaytestSession] StartPlaytest called with null levelData.");
            return;
        }

        IsActive = true;
        LevelData = levelData;
        ReturnSceneName = returnSceneName;
        ReturnScenePath = returnScenePath;
        RequestedSceneName = requestedSceneName;
        SuppressProgression = true;
    }

    public static void Clear()
    {
        IsActive = false;
        LevelData = null;
        ReturnSceneName = null;
        ReturnScenePath = null;
        RequestedSceneName = null;
        SuppressProgression = false;
    }
}
