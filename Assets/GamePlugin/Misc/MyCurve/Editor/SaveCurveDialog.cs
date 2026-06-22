#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using System.IO;

public class SaveCurveDialog : EditorWindow
{
    private Action<string> saveAction;
    private string presetName = "MyPreset";

    public static void ShowWindow(Vector3 ps,Action<string> action)
    {
        var window = GetWindow<SaveCurveDialog>(true, "Save Curve Preset");
        window.saveAction = action;
        window.position = new Rect(ps.x, ps.y, 300, 100);
        window.ShowUtility(); // show as small dialog
    }

    private void OnGUI()
    {
        GUILayout.Space(5);
        GUILayout.Label("Enter Preset Name:", EditorStyles.boldLabel);
        GUILayout.Space(10);

        presetName = EditorGUILayout.TextField("Name", presetName);

        GUILayout.Space(15);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save"))
        {
            saveAction?.Invoke(presetName);
            Close();
        }
        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
        GUILayout.EndHorizontal();
    }
}
#endif