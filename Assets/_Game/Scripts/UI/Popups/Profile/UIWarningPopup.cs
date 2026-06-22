using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIWarningPopup : PopupUI
{
    public Text txtTitle;
    public Text txtContent;
    public Button btnYes;
    public Button btnNo;
    public Button btnClose;
    private Action onYes;
    private Action onNo;
    private void Start()
    {
        btnYes.onClick.AddListener(onClickYes);
        btnNo.onClick.AddListener(onClickNo);
        btnClose.onClick.AddListener(Hide);
    }

    private void onClickNo()
    {
        onNo?.Invoke();
        Hide();
    }

    private void onClickYes()
    {
        onYes?.Invoke();
        Hide();
    }

    public void Setup(string keyTitle, string keyContent, Action onYes, Action onNo)
    {
        txtTitle.SetText(keyTitle);
        txtContent.SetText(keyContent);
        this.onYes = onYes;
        this.onNo = onNo;
    }
    public void Setup1(string Title, string Content, Action onYes, Action onNo)
    {
        txtTitle.SetValue(Title);
        txtContent.SetValue(Content);
        this.onYes = onYes;
        this.onNo = onNo;
    }
}

