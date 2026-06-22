using System.Collections;
using System.Collections.Generic;
using mygame.sdk;
using UnityEngine;
using UnityEngine.UI;

public class TextVersion : MonoBehaviour
{
    void Start()
    {
        Text txt = GetComponent<Text>();
        txt.text =  $"VERSION: {Application.version}";
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        GameHelper.Instance.gotoStore(AppConfig.appid);
    }
}
