#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;

[CustomEditor(typeof(PopupTutorialMechanic))]
public class PopupTutorialMechanicEditor : Editor
{
    private string inputText = "";
    private bool duplicateIfNotFound = true;
    private Transform rootTransform;
    private bool clearExisting = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("EDITOR CUSTOM", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Auto Setup Tutorial Objects", EditorStyles.boldLabel);

        rootTransform = (Transform)EditorGUILayout.ObjectField("Root Transform", rootTransform, typeof(Transform), true);

        EditorGUILayout.LabelField("Paste enum types here (e.g., HiddenObject = 1,):");
        inputText = EditorGUILayout.TextArea(inputText, GUILayout.Height(150));
        
        GUILayout.BeginHorizontal();
        duplicateIfNotFound = EditorGUILayout.ToggleLeft("Duplicate from Root[0]", duplicateIfNotFound, GUILayout.Width(160));
        clearExisting = EditorGUILayout.ToggleLeft("Clear Existing List", clearExisting);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Parse and Setup", GUILayout.Height(30)))
        {
            SetupTutorialObjects();
        }
    }

    private void SetupTutorialObjects()
    {
        PopupTutorialMechanic script = (PopupTutorialMechanic)target;

        if (rootTransform == null)
        {
            rootTransform = FindDeepChild(script.transform, "root");
            if (rootTransform == null)
            {
                Debug.LogError("Could not find 'root' transform in children. Please assign it manually.");
                return;
            }
        }

        FieldInfo field = typeof(PopupTutorialMechanic).GetField("tutorialObjects", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null)
        {
            Debug.LogError("Could not find tutorialObjects field in PopupTutorialMechanic.");
            return;
        }

        List<MechanicTutorialInfo> existingList = (List<MechanicTutorialInfo>)field.GetValue(script);
        if (existingList == null || clearExisting) 
        {
            existingList = new List<MechanicTutorialInfo>();
        }

        Undo.RecordObject(script, "Setup Tutorial Objects");

        string[] lines = inputText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        int addedOrUpdated = 0;

        foreach (var line in lines)
        {
            string trimLine = line.Trim();
            if (string.IsNullOrEmpty(trimLine)) continue;

            string goName = trimLine.Replace(",", "").Trim();
            
            string[] parts = goName.Split('=');
            if (parts.Length < 2) continue;

            string enumName = parts[0].Trim();
            string idPart = parts[1].Trim();

            Transform child = null;
            foreach (Transform t in rootTransform)
            {
                if (t.name == goName || t.name.Contains($"= {idPart}") || t.name.Contains($"={idPart}"))
                {
                    child = t;
                    break;
                }
            }

            if (child == null && duplicateIfNotFound)
            {
                if (rootTransform.childCount > 0)
                {
                    GameObject newGo = Instantiate(rootTransform.GetChild(0).gameObject, rootTransform);
                    newGo.name = goName;
                    child = newGo.transform;
                    Undo.RegisterCreatedObjectUndo(newGo, "Duplicate Tutorial Object");
                }
                else
                {
                    Debug.LogWarning("Root has no children to duplicate for " + goName);
                    continue;
                }
            }

            if (child != null)
            {
                MechanicTutorialInfo info = new MechanicTutorialInfo();
                MechanicTutorialType enumVal = MechanicTutorialType.None;

                if (Enum.TryParse<MechanicTutorialType>(enumName, out var parsedEnum))
                {
                    enumVal = parsedEnum;
                }
                else
                {
                    Debug.LogWarning("Could not parse MechanicTutorialType enum: " + enumName);
                }
                
                info.mechanicTutorialType = enumVal;

                int existingIndex = existingList.FindIndex(x => x.mechanicTutorialType == enumVal && enumVal != MechanicTutorialType.None);
                if (existingIndex >= 0)
                {
                    info = existingList[existingIndex];
                }

                info.objectTutorial = child.gameObject;
                info.skeletonAnimation = child.GetComponent<SkeletonGraphic>();
                if (info.skeletonAnimation == null)
                {
                    info.skeletonAnimation = child.GetComponentInChildren<SkeletonGraphic>(true);
                }

                string snakeCase = ToSnakeCase(enumName);
                info.descTutorial = "_desc_unlock_" + snakeCase;
                info.nameTutorial = "_unlock_" + snakeCase;

                if (existingIndex >= 0)
                {
                    existingList[existingIndex] = info;
                }
                else
                {
                    existingList.Add(info);
                }
                addedOrUpdated++;
            }
        }

        field.SetValue(script, existingList);
        EditorUtility.SetDirty(script);
        serializedObject.Update(); // Force inspector update
        Debug.Log($"Successfully setup {addedOrUpdated} tutorial objects.");
    }

    private Transform FindDeepChild(Transform aParent, string aName)
    {
        if (aParent.name == aName) return aParent;

        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == aName)
                return c;
            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }

    private string ToSnakeCase(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        if (text.Length < 2) return text.ToLowerInvariant();
        var sb = new System.Text.StringBuilder();
        sb.Append(char.ToLowerInvariant(text[0]));
        for (int i = 1; i < text.Length; ++i)
        {
            char c = text[i];
            if (char.IsUpper(c))
            {
                sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}
#endif