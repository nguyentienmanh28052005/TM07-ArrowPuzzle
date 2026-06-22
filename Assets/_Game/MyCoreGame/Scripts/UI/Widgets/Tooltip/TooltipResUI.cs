using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipResUI : MonoBehaviour
{
    [SerializeField] private GameObject plusSymbols;
    [SerializeField] private float deviatedOffsetX = 70f;

    [SerializeField] private ResUIBase visual;
    public DataResource data { get; private set; }
    private bool isInit = false;
    public void Init(DataResource data, bool deviated  = false, Action onClickRes = null)
    {
        if (data == null) { return; }
        init();
        this.data = data;
        if (visual != null)
        {
            if (deviated)
            {
                visual.transform.localPosition = new Vector3(deviatedOffsetX, 0, 0);
            }
            else
            {
                visual.transform.localPosition = new Vector3(0, 0, 0);
            }
            visual.Init(data, onClickRes);
            visual.Show();
        }
    }
    private void Awake()
    {
        init();
    }
    private void init()
    {
        if (!isInit)
        {
            isInit = true;
            visual.Hide();
        }
    }
    public void ShowPlusSymbols()
    {
        plusSymbols.SetActive(true);
    }
    public void HidePlusSymbols()
    {
        plusSymbols.SetActive(false);
    }
}
