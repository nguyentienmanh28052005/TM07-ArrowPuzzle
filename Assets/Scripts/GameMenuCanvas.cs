using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameMenuCanvas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textLevel;

    [SerializeField] private GameObject panelMainMenu;
    [SerializeField] private GameObject panelSetting;
   

    public void OnEnable()
    {
        textLevel.text = "Level " + GameManager.Instance.level;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Setting()
    {   panelMainMenu.SetActive(false);
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

    public void UpdateLevelText()
    {
        textLevel.text = "Level " + GameManager.Instance.level;
    }

    public void NextLevel()
    {
        if(GameManager.Instance.level < GameManager.Instance.currentMaxLevel)
        {
            GameManager.Instance.level++;
            UpdateLevelText();
        }
    }

    public void PreviousLevel()
    {
        if(GameManager.Instance.level > 1)
        {
            GameManager.Instance.level--;
            UpdateLevelText();
        }
    }


}
