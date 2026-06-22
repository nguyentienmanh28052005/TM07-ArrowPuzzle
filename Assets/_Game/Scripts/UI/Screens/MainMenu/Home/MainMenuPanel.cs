using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class MainMenuPanel : MonoBehaviour
{
    protected MainMenuScreen mainScreenUI;
    public ItemDisplay itemDisplay;
    public virtual void Initialize(MainMenuScreen screenUI)
    {
        mainScreenUI = screenUI;
    }
    public virtual void Active()
    {
        // gameObject.SetActive(true);
    }
    public virtual void Deactive()
    {
        // gameObject.SetActive(false);
    }
}
