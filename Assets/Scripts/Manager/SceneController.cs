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
    private bool isLoading = false;
    private bool isLoadingBarRunning = false;
    private AsyncOperation asyncLoad;
    
    [Header("Some components")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private GameObject loadCanvas;
    
   
    public SceneInstance currentScene;
    public string currentSceneName = "Boostrap";
    
    private float originWidth, originHeight;
    public List<string> sceneLoadingHistory = new List<string>(); 
    
    public void Awake()
    {
        isLoadingBarRunning = true;
        
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
    float duration = 1f;
    isLoadingBarRunning = true;
    loadCanvas.SetActive(true);

    DoLoadingBar(duration, () => { isLoadingBarRunning = false; });

    var loadLoadingSceneTask = Addressables.LoadSceneAsync("Buffer", LoadSceneMode.Additive);
    yield return loadLoadingSceneTask;

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

    yield return new WaitForSeconds(0.05f);

    if (nextIsAddressable)
    {
        AsyncOperationHandle<SceneInstance> asyncNextSceneTask = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive, activateOnLoad: false);
        yield return asyncNextSceneTask;

        while (isLoadingBarRunning)
        {
            yield return null;
        }

        asyncNextSceneTask.Result.ActivateAsync();
        currentScene = asyncNextSceneTask.Result;
    }
    else
    {
        AsyncOperation asyncNextSceneTask = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        asyncNextSceneTask.allowSceneActivation = false;

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

    Addressables.UnloadSceneAsync(loadLoadingSceneTask.Result);
    yield return null;

    isLoading = false;
    loadCanvas.SetActive(false);
}
}