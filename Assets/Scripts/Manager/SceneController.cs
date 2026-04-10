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

[RequireComponent(typeof(Initialization))]
public class SceneController : Singleton<SceneController>
{
    private bool isLoading = false;

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

    public void Awake()
    {
        if (loadCanvasGroup != null)
        {
            loadCanvasGroup.alpha = 0;
            loadCanvasGroup.gameObject.SetActive(false);
        }
        LoadScene("GameMenu", false, false);
    }

    public void LoadScene(string sceneName, bool currentIsAddressable = true, bool nextIsAddressable = true)
    {
        if (isLoading) return;
        isLoading = true;
        StopAllCoroutines();
        StartCoroutine(LoadSceneProgress(sceneName, currentIsAddressable, nextIsAddressable));
    }

    private IEnumerator LoadSceneProgress(string sceneName, bool currentIsAddressable = true, bool nextIsAddressable = true)
    {
        loadCanvasGroup.gameObject.SetActive(true);
        Pixelplacement.Tween.Value(loadCanvasGroup.alpha, 1f, (a) => loadCanvasGroup.alpha = a, fadeDuration, 0f, Pixelplacement.Tween.EaseLinear);
        progressBar.value = 0f;

        if (textAnimationCoroutine != null) StopCoroutine(textAnimationCoroutine);
        textAnimationCoroutine = StartCoroutine(AnimateLoadingText());

        yield return new WaitForSeconds(fadeDuration);

        var loadLoadingSceneTask = Addressables.LoadSceneAsync("Buffer", LoadSceneMode.Additive);
        yield return loadLoadingSceneTask;

        if (currentIsAddressable)
        {
            var unloadCurrentSceneTask = Addressables.UnloadSceneAsync(currentScene);
            yield return unloadCurrentSceneTask;
        }
        else
        {
            var unloadCurrentSceneTask = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
            if (unloadCurrentSceneTask != null) yield return unloadCurrentSceneTask;
        }

        yield return new WaitForSeconds(0.05f);

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
                progressBar.value = Mathf.MoveTowards(progressBar.value, targetProgress, Time.deltaTime * fillSpeed);
                yield return null;
            }

            progressBar.value = 1f;
            yield return new WaitForSeconds(0.1f); // Dừng lại 1 chút ở mức 100% cho đẹp mắt

            asyncNextSceneTask.Result.ActivateAsync();
            currentScene = asyncNextSceneTask.Result;
            currentSceneName = sceneName;
        }
        else
        {
            var asyncNextSceneTask = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncNextSceneTask.allowSceneActivation = false;

            // Tương tự với SceneManager thường (chỉ số progress dừng ở mức 0.9)
            while (asyncNextSceneTask.progress < 0.9f || progressBar.value < 0.99f)
            {
                float targetProgress = asyncNextSceneTask.progress / 0.9f; // Chuẩn hóa về 0 -> 1
                progressBar.value = Mathf.MoveTowards(progressBar.value, targetProgress, Time.deltaTime * fillSpeed);
                yield return null;
            }

            progressBar.value = 1f;
            yield return new WaitForSeconds(0.1f);

            asyncNextSceneTask.allowSceneActivation = true;
            while (!asyncNextSceneTask.isDone) yield return null;
            currentSceneName = SceneManager.GetActiveScene().name;
        }
        // ==========================================

        Addressables.UnloadSceneAsync(loadLoadingSceneTask.Result);

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