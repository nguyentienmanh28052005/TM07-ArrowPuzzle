using System;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public sealed class LevelEditorPlaytestBridge
{
    public LevelDataV2 PrepareCurrentLevelForPlaytest(
        LevelEditorContext context,
        bool hasPendingPortalPlacement,
        bool hasPendingElectricWallPlacement,
        Action finishDraftSnake,
        Action cancelPendingPortalPlacement,
        Action cancelPendingElectricWallPlacement,
        Action saveLevel)
    {
        if (context == null || context.currentData == null)
        {
            Debug.LogWarning("[LevelEditor] PrepareCurrentLevelForPlaytest aborted: currentData is null.");
            return null;
        }

        if (context.currentSnakeObj != null && context.currentDraftNodes != null && context.currentDraftNodes.Count > 0)
        {
            finishDraftSnake?.Invoke();
        }

        if (hasPendingPortalPlacement)
        {
            cancelPendingPortalPlacement?.Invoke();
        }

        if (hasPendingElectricWallPlacement)
        {
            cancelPendingElectricWallPlacement?.Invoke();
        }

        saveLevel?.Invoke();
        return context.currentData;
    }

    public void StartPlaytest(LevelDataV2 levelData, string fallbackSceneName, string fallbackScenePath, string targetSceneName, string targetScenePath)
    {
        PlaytestSession.StartPlaytest(
            levelData,
            SceneManager.GetActiveScene().name,
            SceneManager.GetActiveScene().path,
            targetSceneName);

#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            EditorSceneManager.LoadSceneInPlayMode(targetScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            return;
        }
#endif

        TransitionManager transition = UnityEngine.Object.FindObjectOfType<TransitionManager>();
        if (transition != null)
        {
            transition.TransitionToScreen(ScreenType.Gameplay);
            return;
        }

        ScreenManager screenManager = UnityEngine.Object.FindObjectOfType<ScreenManager>();
        if (screenManager != null)
        {
            screenManager.ShowScreen(ScreenType.Gameplay);
            return;
        }

        SceneManager.LoadScene(string.IsNullOrEmpty(targetSceneName) ? fallbackSceneName : targetSceneName);
    }
}
