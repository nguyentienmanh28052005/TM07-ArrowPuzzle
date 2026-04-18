using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelSelectItem : MonoBehaviour
{
    public TextMeshProUGUI levelNameText;
    public Button selectButton;
    private LevelDataSO linkedLevelData;
    private LevelEditor editorReference;

    public void Setup(LevelDataSO data, LevelEditor editor)
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