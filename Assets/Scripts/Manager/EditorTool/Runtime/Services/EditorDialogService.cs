using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class EditorDialogService
{
    public bool ConfirmNew(string key, bool isDirty)
    {
        string message = $"This will switch the editor to a new level session with key '{key}'.";
        if (isDirty)
        {
            message += "\n\nUnsaved editor changes may be lost.";
        }

        return Confirm("Create New Level", message);
    }

    public bool ConfirmLoad(string key, bool isDirty, string assetPath)
    {
        string message = $"This will replace the current editor board with data loaded from '{assetPath}'.";
        if (isDirty)
        {
            message += "\n\nUnsaved editor changes will be replaced.";
        }

        return Confirm($"Load Level '{key}'", message);
    }

    public bool ConfirmSave(string key, bool isExistingAsset, string assetPath)
    {
        string message = isExistingAsset
            ? $"This will overwrite the existing level asset at:\n{assetPath}"
            : $"This will create a new level asset at:\n{assetPath}";
        return Confirm($"Save Level '{key}'", message);
    }

    public bool ConfirmClearData(string key, bool isDirty)
    {
        string message = $"This will clear the current board data for level '{key}'.";
        if (isDirty)
        {
            message += "\n\nUnsaved editor changes will be discarded.";
        }

        return Confirm($"Clear Level Data '{key}'", message);
    }

    public bool ConfirmDelete(string key, string assetPath)
    {
        string message = $"This will permanently delete the level asset at:\n{assetPath}";
        return Confirm($"Delete Level '{key}'", message);
    }

    public void ShowInfo(string title, string message)
    {
#if UNITY_EDITOR
        EditorUtility.DisplayDialog(title, message, "OK");
#else
        Debug.LogWarning($"[{title}] {message}");
#endif
    }

    private static bool Confirm(string title, string message)
    {
#if UNITY_EDITOR
        return EditorUtility.DisplayDialog(title, message, "Continue", "Cancel");
#else
        return true;
#endif
    }
}
