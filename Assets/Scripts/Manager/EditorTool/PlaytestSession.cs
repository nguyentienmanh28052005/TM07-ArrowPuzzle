using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class PlaytestSession
{
#if UNITY_EDITOR
    private const string EditorSessionActiveKey = "ArrowPuzzle.Playtest.Active";
    private const string EditorSessionLevelGuidKey = "ArrowPuzzle.Playtest.LevelGuid";
    private const string EditorSessionReturnSceneNameKey = "ArrowPuzzle.Playtest.ReturnSceneName";
    private const string EditorSessionReturnScenePathKey = "ArrowPuzzle.Playtest.ReturnScenePath";
    private const string EditorSessionRequestedSceneNameKey = "ArrowPuzzle.Playtest.RequestedSceneName";
    private const string EditorSessionReturnScreenKey = "ArrowPuzzle.Playtest.ReturnScreen";
#endif

    public static bool IsActive;
    public static LevelDataSO LevelData;
    public static string ReturnSceneName;
    public static string ReturnScenePath;
    public static string RequestedSceneName;
    public static ScreenType ReturnScreen = ScreenType.Editor;
    public static bool SuppressProgression;

    public static bool IsPlaytesting => IsActive && LevelData != null;

    public static LevelDataSO GetActiveLevelData()
    {
        if (IsPlaytesting) return LevelData;
        return GameManager.Instance != null ? GameManager.Instance.GetCurrentLevelData() : null;
    }

    public static void StartPlaytest(
        LevelDataSO levelData,
        string returnSceneName,
        string returnScenePath,
        string requestedSceneName,
        ScreenType returnScreen = ScreenType.Editor)
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
        ReturnScreen = returnScreen;
        SuppressProgression = true;
    }

#if UNITY_EDITOR
    public static void QueueEditorPlaytest(
        LevelDataSO levelData,
        string returnSceneName,
        string returnScenePath,
        string requestedSceneName,
        ScreenType returnScreen = ScreenType.Editor)
    {
        StartPlaytest(levelData, returnSceneName, returnScenePath, requestedSceneName, returnScreen);

        string levelPath = AssetDatabase.GetAssetPath(levelData);
        string levelGuid = AssetDatabase.AssetPathToGUID(levelPath);

        SessionState.SetBool(EditorSessionActiveKey, true);
        SessionState.SetString(EditorSessionLevelGuidKey, levelGuid);
        SessionState.SetString(EditorSessionReturnSceneNameKey, returnSceneName ?? string.Empty);
        SessionState.SetString(EditorSessionReturnScenePathKey, returnScenePath ?? string.Empty);
        SessionState.SetString(EditorSessionRequestedSceneNameKey, requestedSceneName ?? string.Empty);
        SessionState.SetInt(EditorSessionReturnScreenKey, (int)returnScreen);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RestoreQueuedEditorPlaytest()
    {
        if (IsActive) return;
        if (!SessionState.GetBool(EditorSessionActiveKey, false)) return;

        string levelGuid = SessionState.GetString(EditorSessionLevelGuidKey, string.Empty);
        string levelPath = AssetDatabase.GUIDToAssetPath(levelGuid);
        LevelDataSO levelData = AssetDatabase.LoadAssetAtPath<LevelDataSO>(levelPath);

        if (levelData == null)
        {
            ClearEditorQueue();
            Debug.LogWarning("[PlaytestSession] Unable to restore queued editor playtest level.");
            return;
        }

        StartPlaytest(
            levelData,
            SessionState.GetString(EditorSessionReturnSceneNameKey, string.Empty),
            SessionState.GetString(EditorSessionReturnScenePathKey, string.Empty),
            SessionState.GetString(EditorSessionRequestedSceneNameKey, "GameScene"),
            (ScreenType)SessionState.GetInt(EditorSessionReturnScreenKey, (int)ScreenType.Editor));
    }

    private static void ClearEditorQueue()
    {
        SessionState.SetBool(EditorSessionActiveKey, false);
        SessionState.SetString(EditorSessionLevelGuidKey, string.Empty);
        SessionState.SetString(EditorSessionReturnSceneNameKey, string.Empty);
        SessionState.SetString(EditorSessionReturnScenePathKey, string.Empty);
        SessionState.SetString(EditorSessionRequestedSceneNameKey, string.Empty);
        SessionState.SetInt(EditorSessionReturnScreenKey, (int)ScreenType.Editor);
    }
#endif

    public static void Clear()
    {
        IsActive = false;
        LevelData = null;
        ReturnSceneName = null;
        ReturnScenePath = null;
        RequestedSceneName = null;
        ReturnScreen = ScreenType.Editor;
        SuppressProgression = false;

#if UNITY_EDITOR
        ClearEditorQueue();
#endif
    }
}
