using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using mygame.sdk;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;

public class SDKUpdate : MonoBehaviour
{
#if UNITY_EDITOR
    public string pathSDK = "";

    public bool AdCanvasFolder = true;
    public bool AdCanvasTTDefault = false;
    public bool AdCanvasPrefab = false;
    public bool ApiAdd = false;
    public bool GameResAdd = false;
    public bool GamepluginPreb = false;
#endif
}
