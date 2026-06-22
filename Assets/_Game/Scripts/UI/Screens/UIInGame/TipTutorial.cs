using System.Collections;
using System.Collections.Generic;
using mygame.sdk;
using UnityEngine;
using UnityEngine.UI;

public class TipTutorial : MonoBehaviour
{
    [SerializeField] Text txtTip;
    [SerializeField] CanvasGroup canvasGroup;
    public void Show()
    {
        gameObject.SetActive(true);
        GetComponent<Animation>().Play();
    }
    public void Hide()
    {
        gameObject.SetActive(false);
        canvasGroup.alpha = 0;
    }
    public void SetTipText(string key)
    {
        txtTip.SetText(key, StateCapText.None, FormatText.None, null, false);
    }
}
