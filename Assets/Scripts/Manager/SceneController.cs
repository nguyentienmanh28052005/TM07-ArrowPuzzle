using System;
using System.Collections;
using System.Collections.Generic;
using Pixelplacement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(Initialization))]
public class SceneController : Singleton<SceneController>
{
    private bool isLoading = false; // indicating if a scene is currently being loaded.
    private bool isLoadingBarRunning = false;
    private AsyncOperation asyncLoad;
    
    [Header("Some components")]
    [SerializeField] private Slider progressBar; //Reference to the Slider used for the loading bar.
    [SerializeField] private GameObject loadCanvas; //The canvas that contains the loading bar UI.
    
    //SceneInstance provides a wrapper for scene loading operations with Addressables, enabling delayed activation, reference counting, and better control over asynchronous scene management.
    public SceneInstance currentScene;
    public string currentSceneName = "Boostrap";
    
    private float originWidth, originHeight;
    public List<string> sceneLoadingHistory = new List<string>(); //Keeps a record of the scene names that have been loaded.
    
    public void Awake()
    {
        isLoadingBarRunning = true;
        //StartCoroutine(LoadYourAsync("MainMenu"));
        
        Debug.Log("Enter the sceneController");
        LoadScene("GameMenu", false, false);
    }

    private void DoLoadingBar(float loadTime, UnityEngine.Events.UnityAction onLoadCompleted)
    {
        isLoadingBarRunning = true;
        loadCanvas.SetActive(true);
        
        Pixelplacement.Tween.Value(0f, 1f, (float value) =>
        {
            progressBar.value = value;
        }, loadTime, 0, Pixelplacement.Tween.EaseLinear, Pixelplacement.Tween.LoopType.None, null, () =>
        {
            onLoadCompleted?.Invoke();
            isLoadingBarRunning = false;
        }, true);
    }
    
    
    public void LoadScene(string sceneName, bool currentIsAddressable = true, bool nextIsAddressable = true)
    {
        //prevent the function from being called multiple times while a scene is already in the process of loading.
        if (isLoading)
        {
            return;
        }
        
        isLoading = true;
        StopAllCoroutines();

        StartCoroutine(LoadSceneProgress(sceneName, currentIsAddressable, nextIsAddressable));
    }

    private IEnumerator LoadSceneProgress(string sceneName, bool currentIsAddressable = true, bool nextIsAddressable = true)
{
    // Bắt đầu hiển thị loading bar
    float duration = 1f; // Thời gian chạy thanh slider
    isLoadingBarRunning = true;
    loadCanvas.SetActive(true);

    // Chạy thanh slider
    DoLoadingBar(duration, () => { isLoadingBarRunning = false; });

    // Load scene "Buffer" để hiển thị trong lúc chuyển cảnh
    var loadLoadingSceneTask = Addressables.LoadSceneAsync("Buffer", LoadSceneMode.Additive);
    yield return loadLoadingSceneTask;

    // Dỡ scene hiện tại
    if (currentIsAddressable)
    {
        AsyncOperationHandle<SceneInstance> unloadCurrentSceneTask = Addressables.UnloadSceneAsync(currentScene);
        yield return unloadCurrentSceneTask;
    }
    else
    {
        AsyncOperation unloadCurrentSceneTask = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
        yield return unloadCurrentSceneTask;
    }

    // Đợi một chút để đảm bảo scene hiện tại được dỡ hoàn toàn
    yield return new WaitForSeconds(0.05f);

    // Load scene mới nhưng chưa kích hoạt
    if (nextIsAddressable)
    {
        AsyncOperationHandle<SceneInstance> asyncNextSceneTask = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive, activateOnLoad: false);
        yield return asyncNextSceneTask;

        // Đợi thanh slider hoàn tất trước khi kích hoạt scene
        while (isLoadingBarRunning)
        {
            yield return null;
        }

        // Kích hoạt scene mới
        asyncNextSceneTask.Result.ActivateAsync();
        currentScene = asyncNextSceneTask.Result;
    }
    else
    {
        AsyncOperation asyncNextSceneTask = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        asyncNextSceneTask.allowSceneActivation = false;

        // Đợi thanh slider hoàn tất trước khi cho phép kích hoạt scene
        while (isLoadingBarRunning)
        {
            yield return null;
        }

        asyncNextSceneTask.allowSceneActivation = true;
        while (!asyncNextSceneTask.isDone)
        {
            yield return null;
        }
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    // Đợi thêm một chút để đảm bảo giao diện mượt mà
    //yield return new WaitForSeconds(0.5f);

    // Dỡ scene "Buffer"
    Addressables.UnloadSceneAsync(loadLoadingSceneTask.Result);
    yield return null;

    // Kết thúc quá trình load
    isLoading = false;
    loadCanvas.SetActive(false);
}
}