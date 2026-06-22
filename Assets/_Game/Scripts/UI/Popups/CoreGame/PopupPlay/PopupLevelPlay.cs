using EventGame;
using mygame.sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupLevelPlay : PopupUI
{
    [SerializeField] ButtonBoosterLevelPlay[] buttonBoosters;
    [SerializeField] Button btnPlay;
    [SerializeField] Text txtLevel;
    [SerializeField] Image buttonBGImg;
    [SerializeField] Animation _animation;
    [SerializeField] Text textLevelType;
    [SerializeField] GameObject tagObj;
    [SerializeField] BarBottomManager barBottomManager;
    [SerializeField] Vector2 posTarget;
    private Vector2 posCurrent;

    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        btnPlay.onClick.AddListener(OnClickPlayLevel);
        posCurrent = mainPopUp.anchoredPosition;
    }

    private void OnClickPlayLevel()
    {
        var lvl = GameRes.GetLevel();
   
        Hide();
       
    }

}
