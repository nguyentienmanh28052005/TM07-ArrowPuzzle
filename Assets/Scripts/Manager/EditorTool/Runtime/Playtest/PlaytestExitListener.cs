using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public class PlaytestExitListener : MonoBehaviour
{
    [SerializeField] private KeyCode exitKey = KeyCode.Escape;
    [SerializeField] private string fallbackReturnSceneName = "EditorScene";

    public static void EnsureExists()
    {
        if (FindObjectOfType<PlaytestExitListener>() != null) return;

        GameObject go = new GameObject("PlaytestExitListener");
        DontDestroyOnLoad(go);
        go.AddComponent<PlaytestExitListener>();
    }

    private void Update()
    {
        if (!PlaytestSession.IsActive) return;
        if (Input.GetKeyDown(exitKey) || Input.GetKeyDown(KeyCode.F5)) ExitPlaytest();
    }

    public static void ExitPlaytest()
    {
        PlaytestExitListener listener = FindObjectOfType<PlaytestExitListener>();
        if (listener == null)
        {
            EnsureExists();
            listener = FindObjectOfType<PlaytestExitListener>();
        }

        if (listener != null)
        {
            listener.ExitPlaytestInternal();
        }
    }

    private void ExitPlaytestInternal()
    {
        Time.timeScale = 1f;

        string targetScene = !string.IsNullOrEmpty(PlaytestSession.ReturnSceneName)
            ? PlaytestSession.ReturnSceneName
            : fallbackReturnSceneName;
        string targetScenePath = PlaytestSession.ReturnScenePath;
        ScreenType targetScreen = PlaytestSession.ReturnScreen;

        PlaytestSession.Clear();

#if UNITY_EDITOR
        if (Application.isPlaying && !string.IsNullOrEmpty(targetScenePath) && targetScenePath != SceneManager.GetActiveScene().path)
        {
            EditorSceneManager.LoadSceneInPlayMode(targetScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            return;
        }
#endif

        if (!string.IsNullOrEmpty(targetScene) && targetScene != SceneManager.GetActiveScene().name)
        {
            SceneManager.LoadScene(targetScene);
            return;
        }

        TransitionManager transition = FindObjectOfType<TransitionManager>();
        if (transition != null)
        {
            transition.TransitionToScreen(targetScreen);
            return;
        }

        ScreenManager screenManager = FindObjectOfType<ScreenManager>();
        if (screenManager != null)
        {
            screenManager.ShowScreen(targetScreen);
            return;
        }

        SceneManager.LoadScene(targetScene);
    }
}
