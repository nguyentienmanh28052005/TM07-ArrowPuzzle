using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(Image))]
public class UIColorButton : MonoBehaviour
{
    public Color myColor = Color.white;
    
    private LevelEditor editor;

    /// <summary>
    /// Khởi tạo hiển thị màu cho nút và gắn kết sự kiện Click.
    /// </summary>
    void Start()
    {
        editor = FindObjectOfType<LevelEditor>();

        GetComponent<Image>().color = myColor; 
        
        GetComponent<Button>().onClick.AddListener(OnColorButtonClicked);
    }

    /// <summary>
    /// Ra lệnh cho Editor đổi màu vẽ khi người dùng chọn nút màu này.
    /// </summary>
    private void OnColorButtonClicked()
    {
        if (editor != null)
        {
            editor.UI_SetColor(myColor);
        }
    }
}