using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelNodeHomeScroll : MonoBehaviour
{
    [SerializeField] HomeNodeLevel[] nodeLevels;

    private void OnEnable()
    {
        int curLevel = DataManager.Level;
        for (int i = 0; i < nodeLevels.Length; i++) {
            nodeLevels[i].SetUpLevel(curLevel + i);
        } 
    }
}
