using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
// using UnityEditor;

 [Serializable]
public class ReplaceData
{
    public string Key;
    public string Value;
}

public class MergerMesh : MonoBehaviour
{
    public string mainGuide;

    public List<ReplaceData> replaceID = new List<ReplaceData>();

    public void Search(Dictionary<string, string> stringData)
    {
        // for (int k = 0; k < stringData.Count; k++)
        // {
        //     var str = stringData.ElementAt(k);
        //     for (int i = 0; i < replaceID.Count; i++)
        //     {
        //         var tmp = replaceID.ElementAt(i);
        //         stringData[str.Key] = stringData[str.Key].Replace(tmp.Value, mainGuide);
        //     }
        // }
        

        
        foreach(var rep in replaceID) 
        {
            // if (File.Exists(rep.Key) && AssetDatabase.LoadAllAssetsAtPath(rep.Key).Count(x => x is Mesh) == 1)
            // {
            //     File.Delete(rep.Key);
            // }
        } 

        // AssetDatabase.Refresh();
    }

    public void Search()
    {
        var sceneDir = Directory.GetFiles("Assets/- Project -/Scenes", "*.unity");
        var prefabDir = Directory.GetFiles("Assets/Prefab", "*.prefab");

        var scriptableDir = Directory.GetFiles("Assets/MonoBehaviour", "*.asset");

        Debug.Log($"sceneDir:{sceneDir.Length}, prefabDir:{prefabDir.Length}, scriptableDir:{scriptableDir.Length}");

        foreach(var rep in replaceID) 
        {
            File.Delete(rep.Key);
        } 

        Dictionary<string, string> stringData = new Dictionary<string, string>();
        for (var i = 0; i < sceneDir.Length; i++)
        {
            stringData.Add(sceneDir[i], File.ReadAllText(sceneDir[i]));
        }

        for (var i = 0; i < prefabDir.Length; i++)
        {
            stringData.Add(prefabDir[i], File.ReadAllText(prefabDir[i]));
        }
        
        for (var i = 0; i < scriptableDir.Length; i++)
        {
            stringData.Add(scriptableDir[i], File.ReadAllText(scriptableDir[i]));
        }

        foreach(var tmp in stringData)
        {
            var ss = tmp.Value;
            foreach(var rep in replaceID) 
            {
                ss = tmp.Value.Replace(rep.Value, mainGuide);
            }
            File.WriteAllText(tmp.Key, ss);
        }
        // AssetDatabase.Refresh();
    }
}

// [CustomEditor(typeof(MergerMesh))]
// public class MergerMeshInspector : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         MergerMesh myTarget = (MergerMesh) target;
//         base.OnInspectorGUI();
//         if(GUILayout.Button("Add Main"))
//         {
//             var ob = Selection.activeObject;
//             if (ob as Mesh)
//             {
//                 if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(ob, out string guid, out long file))
//                 {
//                     myTarget.mainGuide = $"fileID: {file}, guid: {guid}";
//                 }
//             }
//         }
//
//         if(GUILayout.Button("Add Replace"))
//         {
//             var ob = Selection.activeObject;
//             if (ob as Mesh)
//             {
//                 if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(ob, out string guid, out long file))
//                 {
//                     var r = new ReplaceData();
//                     r.Key = AssetDatabase.GetAssetPath(ob);
//                     r.Value = $"fileID: {file}, guid: {guid}";
//                     myTarget.replaceID.Add(r);
//                 }
//             }
//         }
//         
//         if(GUILayout.Button("Reset"))
//         {
//             myTarget.replaceID = new List<ReplaceData>();
//         }
//
//         if(GUILayout.Button("Replace"))
//         {
//             myTarget.Search();
//         }
//         serializedObject.Update();
//         serializedObject.ApplyModifiedProperties();
//     }
// }