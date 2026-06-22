using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(Image))]
public class UIColorButton : MonoBehaviour
{
    public Color myColor = Color.white;
    
    
    /// <summary>
    /// Khởi tạo hiển thị màu cho nút và gắn kết sự kiện Click.
    /// </summary>
    void Start()
    {
        GetComponent<Image>().color = myColor; 
        
        GetComponent<Button>().onClick.AddListener(OnColorButtonClicked);
    }

    /// <summary>
    /// Ra lệnh cho Editor đổi màu vẽ khi người dùng chọn nút màu này.
    /// </summary>
    private void OnColorButtonClicked()
    {
        if (LevelEditorWorkspace.Instance != null)
        {
            LevelEditorWorkspace.Instance.UI_SetColor(myColor);
            LevelEditorWorkspace.Instance.UI_SetTool((int)EditorToolType.Paint);
        }
    }
}