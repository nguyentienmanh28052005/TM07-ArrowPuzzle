using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelPlaytestLauncher
{
    private const string BootstrapScenePath = "Assets/Scenes/Boostrap.unity";
    private const string RequestedSceneName = "GameScene";
    private const string StartSceneOverrideKey = "ArrowPuzzle.Playtest.StartSceneOverride";

    static LevelPlaytestLauncher()
    {
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
    }

    [MenuItem("ArrowPuzzle/Playtest Selected Level _F5")]
    public static void PlaytestSelectedLevel()
    {
        if (EditorApplication.isPlaying)
        {
            if (PlaytestSession.IsActive)
            {
                PlaytestExitListener.ExitPlaytest();
                return;
            }

            LevelEditor runtimeEditor = FindSceneLevelEditor();
            if (runtimeEditor != null)
            {
                runtimeEditor.UI_Playtest();
                return;
            }

            LevelDataSO runtimeLevel = ResolveLevelData();
            if (runtimeLevel == null)
            {
                Debug.LogWarning("[LevelPlaytestLauncher] No LevelEditor/current LevelDataSO found for playtest.");
                return;
            }

            PlaytestSession.StartPlaytest(
                runtimeLevel,
                SceneManager.GetActiveScene().name,
                SceneManager.GetActiveScene().path,
                RequestedSceneName,
                ScreenType.Editor);

            SceneManager.LoadScene(RequestedSceneName);
            return;
        }

        LevelDataSO levelData = ResolveLevelData();
        if (levelData == null)
        {
            EditorUtility.DisplayDialog(
                "Arrow Puzzle Playtest",
                "Select a LevelDataSO asset, or open a scene with a LevelEditor that has currentData assigned.",
                "OK");
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        PlaytestSession.QueueEditorPlaytest(
            levelData,
            activeScene.name,
            activeScene.path,
            RequestedSceneName,
            ScreenType.Editor);

        SceneAsset bootstrapScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScenePath);
        if (bootstrapScene == null)
        {
            Debug.LogError("[LevelPlaytestLauncher] Bootstrap scene not found at " + BootstrapScenePath);
            PlaytestSession.Clear();
            return;
        }

        EditorSceneManager.playModeStartScene = bootstrapScene;
        SessionState.SetBool(StartSceneOverrideKey, true);
        EditorApplication.isPlaying = true;
    }

    [MenuItem("ArrowPuzzle/Playtest Selected Level _F5", true)]
    private static bool CanPlaytestSelectedLevel()
    {
        return !EditorApplication.isCompiling &&
               (EditorApplication.isPlaying || !EditorApplication.isPlayingOrWillChangePlaymode);
    }

    private static void HandlePlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode) return;

        if (SessionState.GetBool(StartSceneOverrideKey, false))
        {
            EditorSceneManager.playModeStartScene = null;
            SessionState.SetBool(StartSceneOverrideKey, false);
        }

        PlaytestSession.Clear();
    }

    private static LevelDataSO ResolveLevelData()
    {
        if (Selection.activeObject is LevelDataSO selectedLevel)
        {
            return selectedLevel;
        }

        if (Selection.activeGameObject != null &&
            Selection.activeGameObject.TryGetComponent(out LevelEditor selectedEditor) &&
            selectedEditor.currentData != null)
        {
            return selectedEditor.currentData;
        }

        LevelEditor sceneEditor = FindSceneLevelEditor();
        return sceneEditor != null ? sceneEditor.currentData : null;
    }

    private static LevelEditor FindSceneLevelEditor()
    {
        LevelEditor[] editors = Resources.FindObjectsOfTypeAll<LevelEditor>();
        foreach (LevelEditor editor in editors)
        {
            if (editor == null) continue;
            if (EditorUtility.IsPersistent(editor.gameObject)) continue;
            if (!editor.gameObject.scene.IsValid()) continue;
            if (editor.currentData == null) continue;
            return editor;
        }

        return null;
    }
}
