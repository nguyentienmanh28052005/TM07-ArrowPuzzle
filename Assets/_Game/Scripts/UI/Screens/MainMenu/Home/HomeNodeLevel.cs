using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeNodeLevel : MonoBehaviour
{
    [Serializable]
    public class BackgroundNode
    {
        public GameObject bg;
    }
    
    [SerializeField] BackgroundNode[] backgrounds;
    [SerializeField] BackgroundNode[] normalBackgrounds;
    [SerializeField] Text[] txtLevels;
    [SerializeField] GameObject fx;
    [SerializeField] GameObject fx2;
    public void SetUpLevel(int level)
    {
        // fx.SetActive(level == DataManager.Level);
        // fx2.SetActive(level == DataManager.Level);
        for (int i = 0; i < backgrounds.Length; i++)
        {
            backgrounds[i].bg.SetActive(false);
        }
        for (int i = 0; i < txtLevels.Length; i++)
        {
            txtLevels[i].SetValue(level);
        }
        var levelType = LevelManager.GetLevelType(level);
        backgrounds[Mathf.Clamp((int)levelType, 0, backgrounds.Length - 1)].bg.SetActive(true);
        if (levelType == LevelType.Easy)
        {
            for (int i = 0; i < normalBackgrounds.Length; i++)
            {
                normalBackgrounds[i].bg.SetActive(i%normalBackgrounds.Length == level%normalBackgrounds.Length);
            }
        }
    }
}