using mygame.sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UITryAgainLoadLevel : PopupUI
{
    [SerializeField] Button btnTryAgain;
    [SerializeField] Text txtLevel;
    
    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        btnTryAgain.onClick.AddListener(OnClickTryAgain);
    }
    public override void Show(Action onClose)
    {
        base.Show(onClose);
    }
    
    private void OnClickTryAgain()
    {
       Hide();
    }
}
