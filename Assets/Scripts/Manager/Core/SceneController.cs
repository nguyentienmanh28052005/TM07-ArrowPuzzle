using System.Collections;
using System.Collections.Generic;
using Pixelplacement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[RequireComponent(typeof(Initialization))]
public class SceneController : Singleton<SceneController>
{
    private bool isLoading = false;
    private string _startupScene;

    [Header("UI Components")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private CanvasGroup loadCanvasGroup;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Settings")]
    [SerializeField] private string baseLoadingText = "Đang vạch đường";
    [SerializeField] private float fadeDuration = 0.3f;

    public SceneInstance currentScene;
    public string currentSceneName = "Bootstrap";
    public List<string> sceneLoadingHistory = new List<string>();

    private Coroutine textAnimationCoroutine;

    public void ForceResetLoadingState()
    {
        isLoading = false;
        StopAllCoroutines();

        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
            textAnimationCoroutine = null;
        }

        if (progressBar != null) progressBar.value = 0f;
        if (loadCanvasGroup != null)
        {
            loadCanvasGroup.alpha = 0f;
            loadCanvasGroup.gameObject.SetActive(false);
        }
    }

    private void Awake()
    {
        if (loadCanvasGroup != null)
        {
            loadCanvasGroup.alpha = 0;
            loadCanvasGroup.gameObject.SetActive(false);
        }

        string startupScene = "GameMenu";
        if (PlaytestSession.IsActive && !string.IsNullOrEmpty(PlaytestSession.RequestedSceneName))
        {
            startupScene = PlaytestSession.RequestedSceneName;
        }

        // Defer the actual load until Start() to avoid Unity warnings
        // like "SendMessage cannot be called during Awake".
        _startupScene = startupScene;
    }

    private void Start()
    {
        if (TransitionManager.Instance == null && ScreenManager.Instance == null && !string.IsNullOrEmpty(_startupScene))
        {
            LoadScene(_startupScene, false, false);
        }
    }

    public void LoadScene(string sceneName, bool currentIsAddressable = true, bool nextIsAddressable = true)
    {
        // Make this method re-entrant: playtest uses rapid back-and-forth transitions.
        // If we get called while already loading (e.g., a previous transition got interrupted), reset and continue.
        if (isLoading)
        {
            ForceResetLoadingState();
        }

        isLoading = true;
        StopAllCoroutines();
        StartCoroutine(LoadSceneProgress(sceneName, currentIsAddressable, nextIsAddressable));
    }

#if UNITY_EDITOR
    private static string TryFindScenePathByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return null;

        string[] guids = AssetDatabase.FindAssets(sceneName + " t:Scene");
        if (guids == null || guids.Length == 0) return null;

        string expectedFileName = "/" + sceneName + ".unity";
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(path) && path.EndsWith(expectedFileName))
                return path;
        }

        // Fallback: first match.
        return AssetDatabase.GUIDToAssetPath(guids[0]);
    }
#endif

    private IEnumerator LoadSceneProgress(string sceneName, bool currentIsAddressable = true, bool nextIsAddressable = true)
    {
        // Single-scene flow: forward scene name to ScreenType transition.
        ScreenType screenType;
        if (!TryResolveScreenType(sceneName, out screenType))
        {
            Debug.LogWarning("[SceneController] Unknown screen for scene name: " + sceneName);
            isLoading = false;
            yield break;
        }

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TransitionToScreen(screenType);
        }
        else if (ScreenManager.Instance != null)
        {
            ScreenManager.Instance.ShowScreen(screenType);
        }

        isLoading = false;
        yield break;
    }

    private static bool TryResolveScreenType(string sceneName, out ScreenType screenType)
    {
        screenType = ScreenType.MainMenu;

        if (string.IsNullOrEmpty(sceneName)) return false;

        if (sceneName == "GameMenu")
        {
            screenType = ScreenType.MainMenu;
            return true;
        }

        if (sceneName == "GameScene")
        {
            screenType = ScreenType.Gameplay;
            return true;
        }

        if (sceneName == "EditorScene")
        {
            screenType = ScreenType.Editor;
            return true;
        }

        if (sceneName == "Bootstrap")
        {
            screenType = ScreenType.Bootstrap;
            return true;
        }

        if (sceneName == "Buffer")
        {
            screenType = ScreenType.Loading;
            return true;
        }

        return false;
    }
    
    private IEnumerator AnimateLoadingText()
    {
        if (loadingText == null) yield break;

        int dotCount = 0;
        while (true)
        {
            loadingText.text = baseLoadingText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4;
            yield return new WaitForSeconds(0.4f);
        }
    }
}