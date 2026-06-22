using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonLanguage : MonoBehaviour
{
    [SerializeField] Text txtLang;
    [SerializeField] Text txtLangChoosen;
    [SerializeField] Button btnChoose;
    string langCode = "default";
    private bool isSelected;
    public bool IsSelected => isSelected;
    public Action OnClickAction;
    private void Awake()
    {
        btnChoose.onClick.AddListener(OnClick);
    }
    public void Initialize(string langCode,string langText)
    {
        this.langCode = langCode;
        txtLang.SetValue(langText);
        txtLangChoosen.SetValue(langText);
    }
    public void OnClick()
    {
        OnClickAction?.Invoke();
    }
    public void OnSelect()
    {
        isSelected = true;
        UpdateVisual();
    }
    public void OnDeSelect()
    {
        isSelected = false;
        UpdateVisual();
    }
    void UpdateVisual()
    {
        txtLang.gameObject.SetActive(!isSelected);
        txtLangChoosen.gameObject.SetActive(isSelected);
    }
}
