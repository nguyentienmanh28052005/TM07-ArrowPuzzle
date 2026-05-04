using UnityEngine;
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

        ScreenType targetScreen = ScreenType.MainMenu;
        if (targetScene == "GameScene") targetScreen = ScreenType.Gameplay;
        else if (targetScene == "EditorScene") targetScreen = ScreenType.Editor;

        PlaytestSession.Clear();

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

        Debug.LogWarning("[PlaytestExitListener] No TransitionManager/ScreenManager found. Unable to exit playtest to screen.");
    }
}
