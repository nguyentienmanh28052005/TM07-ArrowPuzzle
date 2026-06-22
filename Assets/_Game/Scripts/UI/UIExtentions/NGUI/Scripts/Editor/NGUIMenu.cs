//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2019 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

/// <summary>
/// This script adds the NGUI menu options to the Unity Editor.
/// </summary>
static public class NGUIMenu
{
    #region Open

    [MenuItem("NGUI/Open/Atlas Maker", false, 9)]
    [MenuItem("Assets/NGUI/Open Atlas Maker", false, 0)]
    static public void OpenAtlasMaker()
    {
        EditorWindow.GetWindow<UIAtlasMaker>(false, "Atlas Maker", true).Show();
    }
    #endregion
}