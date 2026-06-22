using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PopupMechanicGuide : PopupUI
{
    [SerializeField] GuideMechanic[] guideMechanics;
    [SerializeField] Button btnNext;
    [SerializeField] Button btnPrev;
    [SerializeField] Text txtPage;
    int currentPage;
    public static bool IsOpened
    {
        get
        {
            return PlayerPrefs.GetInt("is_opened_guide", 0) != 0;
        }
        set
        {
            PlayerPrefs.SetInt("is_opened_guide", value?1:0);
        }
    }
    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        btnPrev.onClick.AddListener(PrevPage);
        btnNext.onClick.AddListener(NextPage);
    }
    public override void Show(Action onClose)
    {
        base.Show(onClose);
        guideMechanics = guideMechanics.OrderBy(x => x.GetLevelUnlock()).ToArray();
        if (IsOpened)
        {
            var guideFit = guideMechanics.Where(x => x.GetLevelUnlock() < DataManager.Level).OrderBy(x => x.GetLevelUnlock()).Last();
            if (guideFit != null)
            {
                int page = guideMechanics.IndexOf(x => x == guideFit);
                LoadPage(page);
            }
        }
        else
        {
            int page = guideMechanics.IndexOf(x=>x.guideType == MechanicGuideType.PeakInside);

            LoadPage(page);
        }
        IsOpened = true;
    }
    public void NextPage()
    {
        if (currentPage < guideMechanics.Length - 1)
        {
            LoadPage(currentPage + 1);
        }
        else
        {
            LoadPage(0);
        }
    }
    public void PrevPage()
    {
        if (currentPage > 0)
        {
            LoadPage(currentPage - 1);
        }
        else
        {
            LoadPage(guideMechanics.Length-1);
        }
    }
    public void LoadPage(int page)
    {
        for (int i = 0; i < guideMechanics.Length; i++)
        {
            guideMechanics[i].gameObject.SetActive(false);
        }
        guideMechanics[page].gameObject.SetActive(true);
        currentPage = page;
        txtPage.SetValue($"{(currentPage) + 1}/{guideMechanics.Length}");
    }

}
[Serializable]
public struct MechanicGuideInfo
{
    public MechanicGuideType mechanicGuideType;
    public GameObject objectGuide;
    public string descGuide;
    public string nameGuide;
}
public enum MechanicGuideType
{
    None = 0,
    PeakInside = 1,
    AddHole = 2,
    ClearHole = 3,
    BreakObject = 4,
    UnlockBox = 5,
}
