#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class ScreenshotCaptureManager
{
    private static string ExtensionFileScreenshots = ".png";
    private static string NameFileScreenshots = "screenshots_{0}";

    public static bool forcePauseGame = true;
    public static bool useHighestQuality = false;
    public static SaveFolderFormat saveFolderFormat = SaveFolderFormat.Resolution;

    private static int originalQualityLevel = -1;
    private static int originalAntiAliasing = -1;
    private static int originalPixelLightCount = -1;

    // Queue system for capturing screenshots
    private static List<ResolutionItem> captureQueue = new List<ResolutionItem>();
    private static int originalSizeIndex;
    private static string currentCaptureFolderPath;
    public static bool isCapturing = false;
    private static bool isRestoring = false;
    private static int currentCustomIndex = -1;
    private static int currentBatchId = 0;
    private static int framesToWait = 0;

    private static CaptureState captureState = CaptureState.Idle;
    private static string currentCapturePath = "";

    private static int singleCaptureWaitFrames = 0;
    private static int singleCaptureState = 0;
    private static List<Canvas> singleCaptureHiddenCanvases = null;
    private static string currentSingleCapturePath = "";

    public static List<Canvas> HideAllCanvases()
    {
        List<Canvas> hiddenCanvases = new List<Canvas>();
        Canvas[] allCanvases = UnityEngine.Object.FindObjectsOfType<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            if (c.gameObject.activeSelf && c.enabled && c.isRootCanvas)
            {
                c.gameObject.SetActive(false);
                hiddenCanvases.Add(c);
            }
        }

        return hiddenCanvases;
    }

    public static void RestoreCanvases(List<Canvas> canvasesToRestore)
    {
        if (canvasesToRestore == null) return;
        foreach (var c in canvasesToRestore)
        {
            if (c != null)
            {
                c.gameObject.SetActive(true);
            }
        }
    }

    public static void StartSingleCapture(string saveFolder, bool hideCanvas)
    {
        if (hideCanvas)
        {
            singleCaptureHiddenCanvases = HideAllCanvases();
        }
        else
        {
            singleCaptureHiddenCanvases = null;
        }

        if (forcePauseGame && EditorApplication.isPlaying)
        {
            EditorApplication.isPaused = true;
        }

        ApplyHighestQuality();
        EnableFrameDebugger();

        singleCaptureState = 0;
        singleCaptureWaitFrames = 20; // Increased to 20 for safety on lagging machines
        currentCaptureFolderPath = saveFolder;
        
        EditorApplication.update -= SingleCaptureUpdate;
        EditorApplication.update += SingleCaptureUpdate;
    }

    private static void SingleCaptureUpdate()
    {
        // 1. Frame Delay Phase: Wait for Unity to actually process the FrameDebugger toggle or GameView repaint
        if (singleCaptureWaitFrames > 0)
        {
            singleCaptureWaitFrames--;
            if (GameViewResizer.GameViewWindow != null)
            {
                GameViewResizer.GameViewWindow.Repaint();
            }
            return;
        }

        // 2. Capture Trigger Phase
        if (singleCaptureState == 0)
        {
            // Focus Game View to ensure capture gets the correct window and not the tool window itself
            if (GameViewResizer.GameViewWindow != null)
            {
                GameViewResizer.GameViewWindow.Focus();
            }

            currentSingleCapturePath = CaptureScreenshot(currentCaptureFolderPath, NameFileScreenshots, ExtensionFileScreenshots);

            singleCaptureState = 1;
            singleCaptureWaitFrames = 150; // Wait for capture to process asynchronously
        }
        else if (singleCaptureState == 1)
        {
            // Wait an excessive amount of frames (100) or until the file is physically created on disk.
            // This guarantees we do not finish and revert canvas changes BEFORE the screenshot finishes saving.
            if (singleCaptureWaitFrames > 0 && !string.IsNullOrEmpty(currentSingleCapturePath) && !File.Exists(currentSingleCapturePath))
            {
                singleCaptureWaitFrames--;
                return;
            }

            // 4. Cleanup Phase: Restore all overridden settings
            RestoreQuality();
            DisableFrameDebugger();

            if (singleCaptureHiddenCanvases != null)
            {
                RestoreCanvases(singleCaptureHiddenCanvases);
                singleCaptureHiddenCanvases = null;
            }

            // Unregister update loop when fully finished
            EditorApplication.update -= SingleCaptureUpdate;
            singleCaptureState = 2; // Finished
            Debug.Log($"[ScreenshotTool] Single Capture Saved to {currentSingleCapturePath}");
        }
    }

    public static void StartCaptureQueue(List<ResolutionItem> activeResolutions, string saveFolder)
    {
        if (activeResolutions == null || activeResolutions.Count == 0) return;

        if (forcePauseGame && EditorApplication.isPlaying)
        {
            EditorApplication.isPaused = true;
        }

        captureQueue = new List<ResolutionItem>(activeResolutions);
        currentCaptureFolderPath = saveFolder;

        int targetBatchId = 0;
        bool foundValidBatchId = false;

        // Loop to find an available unique increment ID for the entire batch.
        while (!foundValidBatchId)
        {
            bool anyFileExists = false;
            foreach (var r in activeResolutions)
            {
                string subfolderName = GetBatchSubfolderName(r, r.width, r.height);
                string folderForThisScreen = Path.Combine(saveFolder, r.platform.ToString(), subfolderName).Replace("\\", "/");
                string fileName = GetBatchFileName(r, r.width, r.height);

                string testPath = GetFormattedPath(folderForThisScreen, fileName, ".png", targetBatchId);
                if (File.Exists(testPath))
                {
                    anyFileExists = true;
                    break;
                }
            }

            if (anyFileExists)
            {
                targetBatchId++;
            }
            else
            {
                foundValidBatchId = true;
            }
        }

        currentBatchId = targetBatchId;

        ApplyHighestQuality();

        originalSizeIndex = GameViewResizer.GetSelectedSizeIndex();
        isCapturing = true;
        isRestoring = false;
        currentCustomIndex = -1;
        framesToWait = 0;
        captureState = CaptureState.Idle;

        EditorApplication.update -= CaptureUpdateTick;
        EditorApplication.update += CaptureUpdateTick;
    }

    private static void CaptureUpdateTick()
    {
        if (captureQueue.Count == 0)
        {
            if (!isRestoring)
            {
                if (currentCustomIndex != -1)
                {
                    GameViewResizer.SetSelectedSizeIndex(originalSizeIndex);
                    GameViewResizer.RemoveCustomSize(currentCustomIndex);
                    currentCustomIndex = -1;
                }
                else
                {
                    GameViewResizer.SetSelectedSizeIndex(originalSizeIndex);
                }

                isRestoring = true;
                framesToWait = 20; // Give GameView time to physically resize back to normal
                return;
            }

            if (framesToWait > 0)
            {
                framesToWait--;
                GameViewResizer.GameViewWindow.Repaint();
                return;
            }

            // Finally, when the GameView is back to normal, force layout update so it doesn't look broken
            ForceCanvasUpdate();
            RestoreQuality();
            DisableFrameDebugger();

            isCapturing = false;
            isRestoring = false;
            captureState = CaptureState.Idle;

            EditorApplication.update -= CaptureUpdateTick;
            Debug.Log("<color=green><b>[ScreenshotTool]</b> Batch capture finished successfully!</color>");
            return;
        }

        var item = captureQueue[0];
        int targetW = item.width;
        int targetH = item.height;

        switch (captureState)
        {
            case CaptureState.Idle:
            {
                string sizeName = $"Temp_{item.platform}_{targetW}x{targetH}";
                currentCustomIndex = GameViewResizer.AddCustomFixedSize(targetW, targetH, sizeName);
                GameViewResizer.SetSelectedSizeIndex(currentCustomIndex);

                captureState = CaptureState.WaitingForResolution;
                framesToWait = 20; // Increased delay
                break;
            }

            // --- STATE 1: Waiting for GameView Resolution ---
            case CaptureState.WaitingForResolution:
            {
                if (framesToWait > 0)
                {
                    framesToWait--;
                    GameViewResizer.GameViewWindow.Repaint();
                    return;
                }

                // Use Reflection to peek into the GameView's internal RenderTexture to verify its true physical size
                var gameViewWindow = GameViewResizer.GameViewWindow;
                var mTargetTexture = gameViewWindow.GetType()
                        .GetField("m_TargetTexture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        ?.GetValue(gameViewWindow) as RenderTexture;

                if (mTargetTexture != null)
                {
                    // Check if the current physical size is close to our target dimension
                    float dist = Vector2.Distance(new Vector2(targetW, targetH),
                        new Vector2(mTargetTexture.width, mTargetTexture.height));
                    if (dist > 15f)
                    {
                        // The GameView hasn't physically updated its resolution yet. Skip this frame and try again next frame.
                        return;
                    }
                }

                captureState = CaptureState.ForceRendering;
                framesToWait = 5; // Wait 5 frames (and step them) to let scripts react to resolution change
                break;
            }

            // --- STATE 2: Force Frame Rendering ---
            case CaptureState.ForceRendering:
            {
                if (framesToWait > 0)
                {
                    framesToWait--;
                    return;
                }

                ForceCanvasUpdate();
                EnableFrameDebugger();

                captureState = CaptureState.WaitingForRender;
                framesToWait = 10; // Give Unity a few frames to flush the render queue
                break;
            }

            // --- STATE 3: Trigger Screenshot Capture ---
            case CaptureState.WaitingForRender:
            {
                if (framesToWait > 0)
                {
                    framesToWait--;
                    GameViewResizer.GameViewWindow.Repaint();
                    return;
                }

                try
                {
                    // Focus Game View so ScreenCapture doesn't capture the editor tool window by accident
                    GameViewResizer.GameViewWindow.Focus();

                    string subfolderName = GetBatchSubfolderName(item, targetW, targetH);
                    string folderForThisScreen = Path.Combine(currentCaptureFolderPath, item.platform.ToString(), subfolderName).Replace("\\", "/");
                    string finalName = GetBatchFileName(item, targetW, targetH);
                    
                    currentCapturePath = CaptureScreenshot(folderForThisScreen, finalName, ".png", currentBatchId);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ScreenshotTool] Error capturing {targetW}x{targetH}: {ex.Message}");
                }

                captureState = CaptureState.Capturing;
                framesToWait = 150; // Increased timeout for file write
                break;
            }

            // --- STATE 4: Wait For Async File Write ---
            case CaptureState.Capturing:
            {
                // ScreenCapture.CaptureScreenshot writes to disk asynchronously.
                // We poll File.Exists until it confirms the image is physically stored on the hard drive.
                if (framesToWait > 0 && !string.IsNullOrEmpty(currentCapturePath) && !File.Exists(currentCapturePath))
                {
                    framesToWait--;
                    return;
                }

                GameViewResizer.SetSelectedSizeIndex(originalSizeIndex);
                GameViewResizer.RemoveCustomSize(currentCustomIndex);
                currentCustomIndex = -1;

                captureQueue.RemoveAt(0);
                captureState = CaptureState.Idle;

                Debug.Log($"[ScreenshotTool] Completed {item.name} ({targetW}x{targetH}). Remaining: {captureQueue.Count}");
                break;
            }
        }
    }

    public static string CaptureScreenshot(string folderPath, string fileName, string extension, int forceId = -1)
    {
        if (!string.IsNullOrEmpty(extension) && !extension.StartsWith("."))
        {
            extension = "." + extension;
        }

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        int targetId = forceId;
        if (targetId == -1)
        {
            targetId = 0;
            while (File.Exists(GetFormattedPath(folderPath, fileName, extension, targetId)))
            {
                targetId++;
            }
        }

        string finalPath = GetFormattedPath(folderPath, fileName, extension, targetId);

        if (File.Exists(finalPath))
        {
            File.Delete(finalPath);
        }

        Debug.Log(finalPath);
        ScreenCapture.CaptureScreenshot(finalPath);
        
        return finalPath;
    }

    private static string GetFormattedPath(string folderPath, string fileName, string extension, int index)
    {
        string rawFileName = "";
        if (fileName.Contains("{0}"))
        {
            rawFileName = string.Format(fileName, index) + extension;
        }
        else
        {
            rawFileName = (index == 0 ? fileName : $"{fileName}_{index}") + extension;
        }

        return Path.Combine(folderPath, rawFileName).Replace("\\", "/");
    }

    private static void EnableFrameDebugger()
    {
        try
        {
            var assembly = typeof(EditorWindow).Assembly;
            Type frameDebuggerWindowType = assembly.GetType("UnityEditor.FrameDebuggerWindow");
            if (frameDebuggerWindowType != null)
            {
                var window = EditorWindow.GetWindow(frameDebuggerWindowType);

                bool isEnabled = false;
                Type fdType = typeof(UnityEngine.GameObject).Assembly.GetType("UnityEngine.Rendering.FrameDebugger");
                if (fdType == null)
                {
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        fdType = asm.GetType("UnityEngine.Rendering.FrameDebugger") ?? asm.GetType("UnityEditor.FrameDebugger");
                        if (fdType != null) break;
                    }
                }

                if (fdType != null)
                {
                    var prop = fdType.GetProperty("enabled", BindingFlags.Public | BindingFlags.Static);
                    if (prop != null)
                        isEnabled = (bool)prop.GetValue(null, null);
                }

                if (!isEnabled)
                {
                    MethodInfo toggleMethod = frameDebuggerWindowType.GetMethod("ToggleFrameDebuggerEnabled",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    if (toggleMethod == null)
                    {
                        toggleMethod = frameDebuggerWindowType.GetMethod("ClickEnableFrameDebugger",
                            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    }

                    if (toggleMethod != null)
                    {
                        try { toggleMethod.Invoke(window, null); } catch { }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ScreenshotTool] Frame Debugger enable failed: {ex.Message}");
        }

        if (GameViewResizer.GameViewWindow != null)
        {
            GameViewResizer.GameViewWindow.Focus();
            GameViewResizer.GameViewWindow.Repaint();
        }
    }

    private static void DisableFrameDebugger()
    {
        try
        {
            var assembly = typeof(EditorWindow).Assembly;
            Type frameDebuggerWindowType = assembly.GetType("UnityEditor.FrameDebuggerWindow");
            if (frameDebuggerWindowType != null)
            {
                var windows = Resources.FindObjectsOfTypeAll(frameDebuggerWindowType);
                if (windows.Length > 0)
                {
                    var window = (EditorWindow)windows[0];
                    bool isEnabled = false;
                    Type fdType = typeof(UnityEngine.GameObject).Assembly.GetType("UnityEngine.Rendering.FrameDebugger");
                    if (fdType == null)
                    {
                        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            fdType = asm.GetType("UnityEngine.Rendering.FrameDebugger") ?? asm.GetType("UnityEditor.FrameDebugger");
                            if (fdType != null) break;
                        }
                    }

                    if (fdType != null)
                    {
                        var prop = fdType.GetProperty("enabled", BindingFlags.Public | BindingFlags.Static);
                        if (prop != null)
                            isEnabled = (bool)prop.GetValue(null, null);
                    }

                    if (isEnabled)
                    {
                        MethodInfo toggleMethod = frameDebuggerWindowType.GetMethod("ToggleFrameDebuggerEnabled",
                            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                        if (toggleMethod == null)
                        {
                            toggleMethod = frameDebuggerWindowType.GetMethod("ClickEnableFrameDebugger",
                                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                        }

                        if (toggleMethod != null)
                        {
                            try { toggleMethod.Invoke(window, null); } catch { }
                        }
                    }
                    window.Close();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ScreenshotTool] Frame Debugger disable failed: {ex.Message}");
        }
    }

    private static void ForceCanvasUpdate()
    {
        var canvasScalers = Resources.FindObjectsOfTypeAll<UnityEngine.UI.CanvasScaler>();
        foreach (var scaler in canvasScalers)
        {
            if (scaler != null && scaler.gameObject.scene.IsValid() && scaler.isActiveAndEnabled)
            {
                scaler.enabled = false;
                scaler.enabled = true;
            }
        }

        // 2. Force Canvas to update layout calculations
        Canvas.ForceUpdateCanvases();

        // 3. Force Layout rebuild of all Canvas roots
        var canvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (var canvas in canvases)
        {
            if (canvas != null && canvas.gameObject.scene.IsValid() && canvas.isRootCanvas && canvas.gameObject.activeInHierarchy)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(canvas.GetComponent<RectTransform>());
            }
        }
    }

    private static void ApplyHighestQuality()
    {
        originalQualityLevel = QualitySettings.GetQualityLevel();
        originalAntiAliasing = QualitySettings.antiAliasing;
        originalPixelLightCount = QualitySettings.pixelLightCount;

        if (useHighestQuality)
        {
            QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1, true);
            QualitySettings.antiAliasing = 8;
            QualitySettings.pixelLightCount = 8;
        }
    }

    private static void RestoreQuality()
    {
        if (originalQualityLevel != -1)
        {
            QualitySettings.SetQualityLevel(originalQualityLevel, true);
            QualitySettings.antiAliasing = originalAntiAliasing;
            QualitySettings.pixelLightCount = originalPixelLightCount;
            originalQualityLevel = -1;
        }
    }

    private static string GetBatchSubfolderName(ResolutionItem item, int width, int height)
    {
        string safeScreenName = string.Join("_", item.name.Split(Path.GetInvalidFileNameChars()));
        return saveFolderFormat == SaveFolderFormat.Resolution ? $"{width}x{height}" : safeScreenName;
    }

    private static string GetBatchFileName(ResolutionItem item, int width, int height)
    {
        return $"{item.platform.ToString()}_{width}x{height}";
    }
}
#endif
