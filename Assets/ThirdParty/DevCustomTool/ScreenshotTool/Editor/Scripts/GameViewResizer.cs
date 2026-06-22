#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;

public static class GameViewResizer
{
    private static Type GetType(string type)
    {
        return typeof(EditorWindow).Assembly.GetType("UnityEditor." + type);
    }

    private static object FetchProperty(object obj, string property)
    {
        return obj.GetType().GetProperty(property,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(obj, null);
    }

    private static object FetchMethod(object obj, string method, params object[] parameters)
    {
        if (obj == null)
        {
            UnityEngine.Debug.LogError($"[GameViewResizer] FetchMethod failed because obj is null when trying to call: {method}");
            return null;
        }
        var methodInfo = obj.GetType().GetMethod(method,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (methodInfo == null)
            methodInfo = obj.GetType().GetMethod(method,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                Array.ConvertAll(parameters, p => p.GetType()), null);
        return methodInfo?.Invoke(obj, parameters);
    }

    private static object GetGameViewSizesInstance()
    {
        var type = GetType("GameViewSizes");
        if (type == null) return null;
        var prop = type.GetProperty("instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        if (prop == null && type.BaseType != null)
            prop = type.BaseType.GetProperty("instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        return prop?.GetValue(null, null);
    }

    private static object GetCurrentGroupType()
    {
        var gameViewSizes = GetGameViewSizesInstance();
        if (gameViewSizes != null)
        {
            var currentGroupType = FetchProperty(gameViewSizes, "currentGroupType");
            if (currentGroupType != null)
                return currentGroupType;
        }
        
        var groupType = GetType("GameViewSizeGroupType");
        if (groupType != null)
        {
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            try
            {
                return Enum.Parse(groupType, buildTargetGroup.ToString());
            }
            catch { }
        }
        return (int)EditorUserBuildSettings.activeBuildTarget;
    }

    public static EditorWindow GameViewWindow
    {
        get
        {
            var gameViewType = GetType("GameView");
            return EditorWindow.GetWindow(gameViewType);
        }
    }

    public static object SizeSelectionCallback
    {
        get
        {
            var gameViewType = GetType("GameView");
            var gameView = EditorWindow.GetWindow(gameViewType);
            return FetchProperty(gameView, "selectedSizeIndex"); // In some older versions this was the property
        }
    }

    public static object GetGroup(int groupType)
    {
        var gameViewSizes = GetGameViewSizesInstance();
        return FetchMethod(gameViewSizes, "GetGroup", groupType);
    }

    public static int GetSelectedSizeIndex()
    {
        var gameViewType = GetType("GameView");
        var gameView = EditorWindow.GetWindow(gameViewType);
        var prop = gameView.GetType().GetProperty("selectedSizeIndex",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop != null)
        {
            return (int)prop.GetValue(gameView, null);
        }

        return -1;
    }

    public static void SetSelectedSizeIndex(int index)
    {
        var gameViewType = GetType("GameView");
        var gameView = EditorWindow.GetWindow(gameViewType);
        var prop = gameView.GetType().GetProperty("selectedSizeIndex",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop != null)
        {
            prop.SetValue(gameView, index, null);
        }
    }

    public static int AddCustomFixedSize(int width, int height, string name)
    {
        var gameViewSizes = GetGameViewSizesInstance();
        var currentGroup = FetchMethod(gameViewSizes, "GetGroup", GetCurrentGroupType());

        var gameViewSizeType = GetType("GameViewSize");
        var gameViewSizeSizeType = GetType("GameViewSizeType");
        var fixedResolutionValue = Enum.Parse(gameViewSizeSizeType, "FixedResolution");

        var newSize = Activator.CreateInstance(gameViewSizeType, fixedResolutionValue, width, height, name);
        FetchMethod(currentGroup, "AddCustomSize", newSize);

        return (int)FetchMethod(currentGroup, "GetTotalCount") - 1;
    }

    public static void RemoveCustomSize(int index)
    {
        var gameViewSizes = GetGameViewSizesInstance();
        var currentGroup = FetchMethod(gameViewSizes, "GetGroup", GetCurrentGroupType());
        FetchMethod(currentGroup, "RemoveCustomSize", index);
    }
}
#endif
