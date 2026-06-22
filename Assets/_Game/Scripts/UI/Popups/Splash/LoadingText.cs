using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LoadingText : MonoBehaviour
{

    [SerializeField] Text txtLoading;
    [SerializeField] List<TextKeyLoading> textKeyLoadings;
    [SerializeField] Slider imgFollow;

    private void Update()
    {
        var x = textKeyLoadings.LastOrDefault(x => imgFollow.value *100 > x.fillAmount);
        if(x == null)
        {
            txtLoading.SetText("_loading_wait");
        }
        else
        {
            txtLoading.SetText(x.keyTxt);
        }
    }


}

[Serializable]
class TextKeyLoading
{
    public int fillAmount;
    public string keyTxt;
}
