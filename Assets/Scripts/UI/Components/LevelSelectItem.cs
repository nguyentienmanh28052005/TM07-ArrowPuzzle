using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelSelectItem : MonoBehaviour
{
    public TextMeshProUGUI levelNameText;
    public Button selectButton;
    private LevelDataV2 linkedLevelData;
    private LevelEditorWorkspace editorReference;

    public void Setup(LevelDataV2 data, LevelEditorWorkspace editor)
    {
        linkedLevelData = data;
        editorReference = editor;
        
        if (levelNameText != null) levelNameText.text = data.name;
        
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        editorReference.SelectLevelFromUI(linkedLevelData);
    }
}