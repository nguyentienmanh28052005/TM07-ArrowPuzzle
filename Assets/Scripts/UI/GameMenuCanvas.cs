using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuCanvas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textLevel;

    [SerializeField] private GameObject panelMainMenu;
    [SerializeField] private GameObject panelSetting;

    [SerializeField] private List<GameObject> listLevelButtons;

    public void OnEnable()
    {
        UpdateLevelUI();
    }

    public void Setting()
    {   
        panelMainMenu.SetActive(false);
        panelSetting.SetActive(true);
    }

    public void BackToMenu()
    {
        panelSetting.SetActive(false);
        panelMainMenu.SetActive(true);
    }

    public void ResetGame()
    {
        GameManager.Instance.level = 1;
        SceneController.Instance.LoadScene("GameMenu", false, false);
    }

    // Hàm tổng hợp để gọi mỗi khi Level thay đổi
    public void UpdateLevelUI()
    {
        // 1. Cập nhật Text Level chính
        textLevel.text = "Level " + GameManager.Instance.level;

        // 2. Cập nhật 8 nút bấm
        UpdateLevelButtons();
    }

    private void UpdateLevelButtons()
    {
        int currentLevel = GameManager.Instance.level;
        int maxUnlockedLevel = GameManager.Instance.currentMaxLevel;
        
        // Lấy tổng số level game đang có (Ví dụ: Game mới làm tới level 20)
        int totalLevelsInGame = GameManager.Instance.levelDataSOs != null ? GameManager.Instance.levelDataSOs.Count : 999;

        for (int i = 0; i < listLevelButtons.Count; i++)
        {
            GameObject btnObj = listLevelButtons[i];
            
            // Tính toán số level tương ứng cho nút này
            int assignedLevel = currentLevel + i; 

            // 1. Luôn hiển thị Object, không bao giờ tắt (Xóa bỏ SetActive(false))
            if (!btnObj.activeSelf) 
            {
                btnObj.SetActive(true);
            }

            // 2. Cập nhật Text hiển thị
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = assignedLevel.ToString();
            }

            // 3. Xử lý Khóa/Mở nút (Logic cốt lõi)
            // Nút CHỈ được bấm khi thỏa mãn 2 điều kiện:
            // - Level đó đã được mở khóa (<= maxUnlockedLevel)
            // - Level đó thực sự đã được tạo ra trong game (<= totalLevelsInGame)
            bool isPlayable = (assignedLevel <= currentLevel) && (assignedLevel <= totalLevelsInGame);

            // Giao tiếp với script ButtonClicky để làm xám nút nếu isPlayable = false
            ButtonClicky customBtn = btnObj.GetComponent<ButtonClicky>();
            if (customBtn != null)
            {
                customBtn.SetInteractable(isPlayable);
            }
            else 
            {
                // Fallback nếu dùng Button mặc định của Unity
                Button unityBtn = btnObj.GetComponent<Button>();
                if (unityBtn != null)
                {
                    unityBtn.interactable = isPlayable;
                }
            }
        }
    }


    public void EnterGame()
    {
        SceneController.Instance.LoadScene("GameScene", false, false);
    }


    public void NextLevel()
    {
        if(GameManager.Instance.level < GameManager.Instance.currentMaxLevel)
        {
            GameManager.Instance.level++;
            UpdateLevelUI(); // Thay thế UpdateLevelText bằng hàm tổng hợp
        }
    }

    public void PreviousLevel()
    {
        if(GameManager.Instance.level > 1)
        {
            GameManager.Instance.level--;
            UpdateLevelUI(); // Thay thế UpdateLevelText bằng hàm tổng hợp
        }
    }
}