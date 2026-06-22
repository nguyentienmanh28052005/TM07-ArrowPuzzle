using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class FilterScriptableObject : EditorWindow
{
    public List<ScriptableObject> m_ScriptableObjects;
    public Dictionary<ScriptableObject, bool> m_Object = new Dictionary<ScriptableObject, bool>();
    public string[] m_ScriptableObject_Guild;
    private SerializedObject serializedObject;
    private Vector2 scrollPos;

    private void OnEnable()
    {
        m_ScriptableObjects = new List<ScriptableObject>();
        titleContent = new GUIContent("Filter ScriptableObject");
        serializedObject = new SerializedObject(this);
    }

    private void OnGUI()
    {
        EditorGUIUtility.labelWidth = 100f;
        GUILayout.BeginVertical();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginVertical();
        GUILayout.Space(6f);
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Space(6f);
        if (Selection.activeObject != null)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(Selection.activeObject, out var guid, out long file);
            EditorGUILayout.TextField("guild", !string.IsNullOrEmpty(guid) ? guid : "");
        }

        scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUIStyle.none, GUI.skin.verticalScrollbar);
        GUILayout.BeginVertical();
        GUILayout.Space(10f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ScriptableObjects"), true);
        serializedObject.ApplyModifiedProperties();
        m_ScriptableObject_Guild = new string[m_ScriptableObjects.Count];
        GUILayout.EndVertical();
        GUILayout.Space(6f);
        GUILayout.EndScrollView();
        
        if (GUILayout.Button("Find All"))
        {
            var paths = AssetDatabase.GetAllAssetPaths();

            for (int j = 0; j < paths.Length; j++)
            {
                if (j >= paths.Length) break;
                var s = paths[j];
                var ob = AssetDatabase.LoadAssetAtPath<ScriptableObject>(s);

                if (s.Contains(".prefab") && ob != null)
                {
                    m_ScriptableObjects.Add(ob);
                    m_ScriptableObject_Guild  = new string[m_ScriptableObjects.Count];
                }
            }
            
            FindAll();
        }
        
        if (GUILayout.Button("Find"))
        {
            FindAll();
        }


        GUILayout.EndVertical();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void OnSelectionChange()
    {
        Repaint();
    }

    public void FindAll()
    {
        var paths = AssetDatabase.GetAllAssetPaths();
        for (int j = 0; j < paths.Length; j++)
        {
            if (j >= paths.Length) break;
            var s = paths[j];
            Find(s);
        }
             
        var a = 0;
        foreach (var v in m_Object)
        {
            if (!v.Value)
            {
                var path = AssetDatabase.GetAssetPath(v.Key);
                if (string.IsNullOrEmpty(path) || path.Contains("GamePlugin") || path.Contains("Resources") || path.Contains(".fbx") || path.Contains(".obj") || !path.Contains(".prefab")) continue;
                File.Delete(path);
                a++;
            }
        }
        AssetDatabase.Refresh();
        Debug.Log($"Find all delete : {a} file");
    }

    public void Find(string s)
    {
        var t = s.Split('.')[s.Split('.').Length - 1];
        if (File.Exists(s) && s.StartsWith("Assets") && (t == "prefab" || t == "unity" || t == "asset"))
        {
            var tmp = File.ReadAllLines(s);
            foreach (var v in tmp)
            {
                if (v.Contains(" guid:"))
                {
                    for (int j = 0; j < m_ScriptableObjects.Count; j++)
                    {
                        if (!m_Object.ContainsKey(m_ScriptableObjects[j]))
                        {
                            m_Object.Add(m_ScriptableObjects[j], false);
                        }

                        if (!m_Object[m_ScriptableObjects[j]])
                        {
                            var guid = m_ScriptableObject_Guild[j];
                            if (string.IsNullOrEmpty(guid))
                            {
                                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(m_ScriptableObjects[j], out guid, out long file);
                                m_ScriptableObject_Guild[j] = guid;
                            }

                            if (v.Contains($" guid: {guid}"))
                            {
                                m_Object[m_ScriptableObjects[j]] = true;
                            }
                        }
                    }
                }
            }
        }
    }

    [MenuItem("Tool/Filter ScriptableObject")]
    public static void ShowWindow()
    {
        GetWindow(typeof(FilterScriptableObject));
    }
}