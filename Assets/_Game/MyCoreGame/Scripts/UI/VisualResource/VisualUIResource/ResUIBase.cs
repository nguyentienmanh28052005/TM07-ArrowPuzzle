using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ResUIBase : MonoBehaviour
{
    private bool isSetup = false;
    public abstract Text TextValue { get; }
    public abstract void Init(DataResource data, Action OnClickRes);
    public virtual void Show()
    {
        if (!isSetup)
        {
            isSetup = true;
            SetUp();
        }
        gameObject.SetActive(true);
    }
    public virtual void Hide() { gameObject.SetActive(false); }
    public abstract bool CanVisual(DataResource data);
    public abstract void VisualTextAmount(int value);
    public abstract Image Icon { get; }
    public abstract DataResource GetData();
    protected abstract void SetUp();

}
