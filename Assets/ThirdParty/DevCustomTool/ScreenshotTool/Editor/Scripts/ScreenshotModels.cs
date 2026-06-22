#if UNITY_EDITOR
using System.Collections.Generic;

[System.Serializable]
public enum ScreenshotOrientation
{
    Portrait = 0,
    Landscape = 1
}

[System.Serializable]
public enum TargetPlatform
{
    iOS = 0,
    Android = 1,
    Custom = 2
}

[System.Serializable]
public enum SaveFolderFormat
{
    Resolution = 0,
    ScreenName = 1,
}

[System.Serializable]
public enum CapturePlatformFilter
{
    All = 0,
    iOS = 1,
    Android = 2,
    Custom = 3
}

[System.Serializable]
public enum CaptureOrientationFilter
{
    Both = 0,
    PortraitOnly = 1,
    LandscapeOnly = 2
}

[System.Serializable]
public enum CaptureState
{
    Idle = 0,
    WaitingForResolution = 1,
    ForceRendering = 2,
    WaitingForRender = 3,
    Capturing = 4,
}

[System.Serializable]
public class ResolutionItem
{
    public string name;
    public int width;
    public int height;
    public ScreenshotOrientation orientation;
    public bool enabled = true;
    public TargetPlatform platform;

    public ResolutionItem(string name, int width, int height, ScreenshotOrientation orientation, TargetPlatform platform)
    {
        this.name = name;
        this.width = width;
        this.height = height;
        this.orientation = orientation;
        this.platform = platform;
    }
}

[System.Serializable]
public class SerializationWrapper<T>
{
    public List<T> items;

    public SerializationWrapper(List<T> items)
    {
        this.items = items;
    }
}
#endif
