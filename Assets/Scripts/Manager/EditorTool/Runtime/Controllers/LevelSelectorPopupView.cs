using System.Linq;
using UnityEngine;

public class LevelSelectorPopupView : MonoBehaviour
{
    [Header("UI References")]
    public GameObject levelButtonPrefab; 
    public Transform levelScrollContent; 
    public GameObject levelSelectorPanel; 

    private LevelEditorWorkspace workspace;

    public void Initialize(LevelEditorWorkspace workspace)
    {
        this.workspace = workspace;
    }

    public void UI_OpenLevelSelector()
    {
        if (levelSelectorPanel != null) levelSelectorPanel.SetActive(true);
        
        if (levelScrollContent != null)
        {
            foreach (Transform child in levelScrollContent)
            {
                Destroy(child.gameObject);
            }
        }
        
        LevelDataV2[] allLevels = Resources.LoadAll<LevelDataV2>("Levels");
        var sortedLevels = allLevels.OrderBy(l => l.name).ToList();
        foreach (LevelDataV2 level in sortedLevels)
        {
            if (levelButtonPrefab != null && levelScrollContent != null)
            {
                GameObject btnObj = Instantiate(levelButtonPrefab, levelScrollContent);
                LevelSelectItem itemScript = btnObj.GetComponent<LevelSelectItem>();
                if (itemScript != null)
                {
                    itemScript.Setup(level, workspace);
                }
            }
        }
    }

    public void SelectLevelFromUI(LevelDataV2 selectedLevel)
    {
        if (workspace == null) return;

        if (workspace.HasUnsavedChanges())
        {
#if UNITY_EDITOR
            int choice = UnityEditor.EditorUtility.DisplayDialogComplex(
                "Save Changes",
                $"Do you want to save changes to the current level '{workspace.currentData?.name}' before switching?",
                "Yes (Save)",
                "Cancel",
                "No (Discard)"
            );
            if (choice == 0) // Yes (Save)
            {
                workspace.SaveLevelPublic();
            }
            else if (choice == 1) // Cancel
            {
                return;
            }
#endif
        }

        workspace.SetCurrentData(selectedLevel);
        if (levelSelectorPanel != null) levelSelectorPanel.SetActive(false);
        workspace.LoadLevelToEditPublic();
    }
}
