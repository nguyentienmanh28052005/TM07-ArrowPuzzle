using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class LevelCatalogService
{
    private const string AssetFolder = "Assets/Resources/Levels";

    public string NormalizeKey(string key)
    {
        return string.IsNullOrWhiteSpace(key) ? string.Empty : key.Trim();
    }

    public bool IsValidKey(string key, out string error)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            error = "Level Key cannot be empty.";
            return false;
        }

        char[] invalidChars = Path.GetInvalidFileNameChars();
        for (int i = 0; i < key.Length; i++)
        {
            if (System.Array.IndexOf(invalidChars, key[i]) >= 0)
            {
                error = $"Level Key contains an invalid file character: '{key[i]}'.";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }

    public string GetAssetPath(string key)
    {
#if UNITY_EDITOR
        // Search recursively under Assets/Resources/Levels for any existing asset with name matching the key
        string[] guids = AssetDatabase.FindAssets($"{key} t:LevelDataV2", new[] { AssetFolder });
        if (guids != null && guids.Length > 0)
        {
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == key)
                {
                    return path;
                }
            }
        }
#endif
        return $"{AssetFolder}/{key}.asset";
    }

    public bool Exists(string key)
    {
#if UNITY_EDITOR
        return LoadAssetAtPath(key) != null;
#else
        return Resources.Load<LevelDataV2>($"Levels/{key}") != null;
#endif
    }

    public bool TryLoadExisting(string key, out LevelDataV2 levelData)
    {
#if UNITY_EDITOR
        levelData = LoadAssetAtPath(key);
#else
        levelData = Resources.Load<LevelDataV2>($"Levels/{key}");
#endif
        return levelData != null;
    }

    public bool Delete(string key, out string error)
    {
#if UNITY_EDITOR
        error = string.Empty;
        string assetPath = GetAssetPath(key);
        if (!AssetDatabase.DeleteAsset(assetPath))
        {
            error = $"Failed to delete level asset at '{assetPath}'.";
            return false;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return true;
#else
        error = "Deleting levels is only supported in the Unity Editor.";
        return false;
#endif
    }

#if UNITY_EDITOR
    public void EnsureFolderExists()
    {
        if (AssetDatabase.IsValidFolder(AssetFolder)) return;
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        AssetDatabase.CreateFolder("Assets/Resources", "Levels");
    }
#endif

    private LevelDataV2 LoadAssetAtPath(string key)
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<LevelDataV2>(GetAssetPath(key));
#else
        return null;
#endif
    }
}
