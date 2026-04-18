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
        if (Input.GetKeyDown(exitKey)) ExitPlaytest();
    }

    private void ExitPlaytest()
    {
        Time.timeScale = 1f;

        string targetScene = !string.IsNullOrEmpty(PlaytestSession.ReturnSceneName)
            ? PlaytestSession.ReturnSceneName
            : fallbackReturnSceneName;

        string targetScenePath = PlaytestSession.ReturnScenePath;

        // Prefer existing SceneController flow (Buffer/loading/unload logic).
        SceneController controller = FindObjectOfType<SceneController>();
        if (controller != null)
        {
            controller.ForceResetLoadingState();
            PlaytestSession.Clear();
            controller.LoadScene(targetScene, false, false);
            return;
        }

        PlaytestSession.Clear();

#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(targetScenePath))
        {
            EditorSceneManager.LoadSceneAsyncInPlayMode(targetScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            return;
        }
#endif

        SceneManager.LoadScene(targetScene);
    }
}
