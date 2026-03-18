using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(Image))]
public class UIColorButton : MonoBehaviour
{
    public Color myColor = Color.white;
    
    private LevelEditor editor;

    void Start()
    {
        editor = FindObjectOfType<LevelEditor>();

        GetComponent<Image>().color = myColor; 
        
        GetComponent<Button>().onClick.AddListener(OnColorButtonClicked);
    }

    private void OnColorButtonClicked()
    {
        if (editor != null)
        {
            editor.UI_SetColor(myColor);
        }
    }
}