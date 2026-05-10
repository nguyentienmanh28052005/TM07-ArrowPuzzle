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
public class Boostrap : Singleton<Boostrap>
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

        Scene bufferScene = SceneManager.GetSceneByName("Buffer");
        if (bufferScene.IsValid() && bufferScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(bufferScene);
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

        string startupScene = "GameScene";
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
        if (!string.IsNullOrEmpty(_startupScene))
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
        Scene sceneToUnload = SceneManager.GetActiveScene();

        // Transitions must not depend on gameplay time scale.
        Time.timeScale = 1f;

        loadCanvasGroup.gameObject.SetActive(true);
        Pixelplacement.Tween.Value(loadCanvasGroup.alpha, 1f, (a) => loadCanvasGroup.alpha = a, fadeDuration, 0f, Pixelplacement.Tween.EaseLinear);
        progressBar.value = 0f;

        if (textAnimationCoroutine != null) StopCoroutine(textAnimationCoroutine);
        textAnimationCoroutine = StartCoroutine(AnimateLoadingText());

        yield return new WaitForSecondsRealtime(fadeDuration);

        var loadLoadingSceneTask = Addressables.LoadSceneAsync("Buffer", LoadSceneMode.Additive);
        yield return loadLoadingSceneTask;

        // Ensure we keep a stable reference to what we intend to unload.
        // Some projects accidentally change active scene during additive loads.
        if (!sceneToUnload.IsValid() || !sceneToUnload.isLoaded)
        {
            sceneToUnload = SceneManager.GetActiveScene();
        }

        if (currentIsAddressable)
        {
            var unloadCurrentSceneTask = Addressables.UnloadSceneAsync(currentScene);
            yield return unloadCurrentSceneTask;
        }
        else
        {
            string unloadName = sceneToUnload.IsValid() ? sceneToUnload.name : SceneManager.GetActiveScene().name;
            // Never try to unload the buffer scene as the "current" scene.
            if (unloadName == "Buffer") unloadName = SceneManager.GetActiveScene().name;

            var unloadCurrentSceneTask = SceneManager.UnloadSceneAsync(unloadName);
            if (unloadCurrentSceneTask != null) yield return unloadCurrentSceneTask;
        }

        yield return new WaitForSecondsRealtime(0.05f);

        // ==========================================
        // BẢN VÁ: KHÓA TỐC ĐỘ PROGRESS BAR
        // ==========================================
        // Tốc độ làm đầy (1.5f có nghĩa là tốn ít nhất ~0.66 giây để thanh chạy từ 0 đến 1)
        float fillSpeed = 1.5f; 

        if (nextIsAddressable)
        {
            var asyncNextSceneTask = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive, activateOnLoad: false);

            // Thay đổi cốt lõi: Chờ Data tải xong (IsDone) VÀ thanh UI chạy đủ (value >= 0.99)
            while (!asyncNextSceneTask.IsDone || progressBar.value < 0.99f)
            {
                // Nếu Data tải xong tức thì, ép mục tiêu về 1.0. Nếu chưa, lấy phần trăm thật.
                float targetProgress = asyncNextSceneTask.IsDone ? 1f : asyncNextSceneTask.PercentComplete;
                
                // Dùng MoveTowards để thanh trượt đều đặn, không bị nhảy cóc hay đứng khựng
                progressBar.value = Mathf.MoveTowards(progressBar.value, targetProgress, Time.unscaledDeltaTime * fillSpeed);
                yield return null;
            }

            progressBar.value = 1f;
            yield return new WaitForSecondsRealtime(0.1f); // Dừng lại 1 chút ở mức 100% cho đẹp mắt

            asyncNextSceneTask.Result.ActivateAsync();
            currentScene = asyncNextSceneTask.Result;
            currentSceneName = sceneName;
        }
        else
        {
            AsyncOperation asyncNextSceneTask = null;

#if UNITY_EDITOR
            // In Editor play mode, allow loading by scene asset path when the scene isn't in Build Settings.
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                string scenePath = TryFindScenePathByName(sceneName);
                if (!string.IsNullOrEmpty(scenePath))
                {
                    asyncNextSceneTask = EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Additive));
                }
            }
#endif

            if (asyncNextSceneTask == null)
            {
                asyncNextSceneTask = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }

            asyncNextSceneTask.allowSceneActivation = false;

            // Tương tự với SceneManager thường (chỉ số progress dừng ở mức 0.9)
            while (asyncNextSceneTask.progress < 0.9f || progressBar.value < 0.99f)
            {
                float targetProgress = asyncNextSceneTask.progress / 0.9f; // Chuẩn hóa về 0 -> 1
                progressBar.value = Mathf.MoveTowards(progressBar.value, targetProgress, Time.unscaledDeltaTime * fillSpeed);
                yield return null;
            }

            progressBar.value = 1f;
            yield return new WaitForSecondsRealtime(0.1f);

            asyncNextSceneTask.allowSceneActivation = true;
            while (!asyncNextSceneTask.isDone) yield return null;

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                SceneManager.SetActiveScene(loadedScene);
                currentSceneName = loadedScene.name;
            }
            else
            {
                currentSceneName = SceneManager.GetActiveScene().name;
            }
        }
        // ==========================================

        var unloadBufferTask = Addressables.UnloadSceneAsync(loadLoadingSceneTask.Result);
        if (unloadBufferTask.IsValid())
        {
            yield return unloadBufferTask;
        }

        if (textAnimationCoroutine != null) StopCoroutine(textAnimationCoroutine);

        Pixelplacement.Tween.Value(1f, 0f, (a) => loadCanvasGroup.alpha = a, fadeDuration, 0f, Pixelplacement.Tween.EaseLinear, Pixelplacement.Tween.LoopType.None, null, () =>
        {
            loadCanvasGroup.gameObject.SetActive(false);
            isLoading = false;
        });
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