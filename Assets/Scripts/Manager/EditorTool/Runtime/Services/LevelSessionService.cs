using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class LevelSessionService
{
    public LevelDataV2 CreateNewSession(string key, LevelDifficulty difficulty)
    {
        LevelDataV2 levelData = ScriptableObject.CreateInstance<LevelDataV2>();
        levelData.name = key;
        levelData.levelDifficulty = difficulty;
        levelData.gameMode = GameMode.Classic;
        levelData.returnToDefaultZoomAfterIntro = true;
        levelData.timeLimit = 60f;
        return levelData;
    }

    public bool Save(LevelEditor editor, string key, bool targetExists, LevelCatalogService catalog, out string error)
    {
        error = string.Empty;
        if (editor == null)
        {
            error = "LevelEditor is missing.";
            return false;
        }

        LevelDataV2 currentData = editor.GetCurrentLevelData();
        if (currentData == null)
        {
            error = "There is no active level data to save.";
            return false;
        }

        currentData.name = key;
        editor.SaveCurrentEditorLevel();

#if UNITY_EDITOR
        catalog.EnsureFolderExists();
        string assetPath = catalog.GetAssetPath(key);
        if (!AssetDatabase.Contains(currentData))
        {
            if (targetExists)
            {
                error = $"Cannot create a new asset because '{assetPath}' already exists.";
                return false;
            }

            AssetDatabase.CreateAsset(currentData, assetPath);
        }

        EditorUtility.SetDirty(currentData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return true;
#else
        return false;
#endif
    }
}
