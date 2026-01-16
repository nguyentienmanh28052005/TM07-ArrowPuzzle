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


}
