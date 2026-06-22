using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class ActionEditor : MonoBehaviour
{
    [MenuItem("Tool/SetPhysicMat")]
    private static void SetPhysicMat()
    {
        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            var cols = Selection.gameObjects[i].GetComponentsInChildren<Collider>(true);
            foreach (var c in cols)
            {
                if (c != null/* && c.sharedMaterial == null*/)
                {
                    c.sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicMaterial>("Assets/PhysicMaterials/Stone.physicMaterial");
                    EditorUtility.SetDirty(c);
                    EditorUtility.SetDirty(Selection.gameObjects[i]);
                } 
            }
           
        }
    }
    
    [MenuItem("Tool/SetAudioADPCM")]
    private static void SetAudioADPCM()
    {
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            var ob = Selection.objects[i] as AudioClip;
            if (ob != null && ob.length < 5)
            {
                var mPath = AssetDatabase.GetAssetPath(ob)+ ".meta";
                File.WriteAllText(mPath, File.ReadAllText(mPath).Replace("    compressionFormat: 1", "    compressionFormat: 2"));
            }
        }
    }

    [MenuItem("NGUI/Atlas To Spr NGUI")]
    private static void AtlasToNGUISpr()
    {
        var activeObject = Selection.activeObject;
        if (activeObject != null)
        {
            var atlas = activeObject as NGUIAtlas;
            // var atlas = AssetDatabase.LoadAssetAtPath<NGUIAtlas>("Assets/Images/UI/hairCutUI/New Atlas.asset");
            //atlas.texture = Selection.activeObject as Texture2D;

            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(atlas.texture)).OfType<Sprite>().ToArray();
            atlas.spriteList.RemoveAll(x => sprites.SingleOrDefault(a => a.name == x.name) == null);
            for (int i = 0; i < sprites.Length; i++)
            {
                var s = atlas.spriteList.FindIndex(x => x.name == sprites[i].name);
                if (s == -1)
                {
                    var spr = new UISpriteData();
                    spr.x = (int)sprites[i].rect.x;
                    spr.y = -(int)sprites[i].rect.y + atlas.texture.height - spr.height;

                    spr.width = (int)sprites[i].rect.width;
                    spr.height = (int)sprites[i].rect.height;
                    spr.name = sprites[i].name;
                    spr.borderTop = 0;
                    spr.borderBottom = 0;
                    spr.borderLeft = 0;
                    spr.borderRight = 0;

                    spr.paddingTop = 0;
                    spr.paddingBottom = 0;
                    spr.paddingLeft = 0;
                    spr.paddingRight = 0;

                    atlas.spriteList.Add(spr);
                }
                else
                {
                    // it.y = (atlas.texture.height - it.height) + y
                    atlas.spriteList[s].x = (int)sprites[i].rect.x;
                    atlas.spriteList[s].y = -(int)sprites[i].rect.y + atlas.texture.height - atlas.spriteList[s].height;
                }
            }
            EditorUtility.SetDirty(atlas);
            // }
            AssetDatabase.Refresh();

            atlas.spriteList.RemoveAll(x => sprites.SingleOrDefault(a => a.name == x.name) == null);
            for (int i = 0; i < sprites.Length; i++)
            {
                var s = atlas.spriteList.FindIndex(x => x.name == sprites[i].name);
                if (s == -1)
                {
                    var spr = new UISpriteData();
                    spr.x = (int)sprites[i].rect.x;
                    spr.y = -(int)sprites[i].rect.y + atlas.texture.height - spr.height;

                    spr.width = (int)sprites[i].rect.width;
                    spr.height = (int)sprites[i].rect.height;
                    spr.name = sprites[i].name;
                    spr.borderTop = 0;
                    spr.borderBottom = 0;
                    spr.borderLeft = 0;
                    spr.borderRight = 0;

                    spr.paddingTop = 0;
                    spr.paddingBottom = 0;
                    spr.paddingLeft = 0;
                    spr.paddingRight = 0;

                    atlas.spriteList.Add(spr);
                }
                else
                {
                    // it.y = (atlas.texture.height - it.height) + y
                    atlas.spriteList[s].x = (int)sprites[i].rect.x;
                    atlas.spriteList[s].y = -(int)sprites[i].rect.y + atlas.texture.height - atlas.spriteList[s].height;
                }
            }
            EditorUtility.SetDirty(atlas);
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("NGUI/NGUI To Spr Atlas")]
    public static void NGUIToSprAtlas()
    {

        var activeObject = Selection.activeObject;
        if (activeObject != null)
        {
            var atlas = activeObject as NGUIAtlas;
            // var atlas = AssetDatabase.LoadAssetAtPath<NGUIAtlas>("Assets/Images/UI/hairCutUI/New Atlas.asset");

            var d = "  - first:\n      213: {id}\n    second: {name}";
            var sp = "    - serializedVersion: 2\n      name: {name}\n      rect:\n        serializedVersion: 2\n        x: {x}\n        y: {y}\n        width: {w}\n        height: {h}\n      alignment: 0\n      pivot: {x: 0.5, y: 0.5}\n      border: {x: {bx}, y: {by}, z: {bz}, w: {bw}}\n      outline: []\n      physicsShape: []\n      tessellationDetail: 0\n      bones: []\n      spriteID: {sprID}\n      internalID: {interID}\n      vertices: []\n      indices: \n      edges: []\n      weights: []";
            var s = "";
            var data = "";
            var lsId = new List<string>();
            for (int i = 0; i < atlas.spriteList.Count; i++)
            {
                var it = atlas.spriteList[i];

                var id = Random.Range(-9, 10).ToString();
                for (int j = 0; j < 18; j++)
                {
                    id += Random.Range(0, 10);
                }
                lsId.Add(id);
                s += d.Replace("{id}", id).Replace("{name}", it.name);

                data += sp.Replace("{name}", it.name).Replace("{x}", it.x.ToString()).Replace("{y}", (atlas.texture.height - it.y - it.height).ToString()).Replace("{w}", it.width.ToString()).Replace("{h}", it.height.ToString())
                    .Replace("{bx}", it.borderLeft.ToString()).Replace("{by}", it.borderBottom.ToString()).Replace("{bz}", it.borderRight.ToString()).Replace("{bw}", it.borderTop.ToString())
                    .Replace("{sprID}", Guid.NewGuid().ToString().Replace("-", "")).Replace("{interID}", id);
                if (i < atlas.spriteList.Count - 1)
                {
                    s += "\n";
                    data += "\n";
                }

            }

            var txt = File.ReadAllLines(AssetDatabase.GetAssetPath(atlas.texture) + ".meta").ToList();


            var inter = 0;
            var lastI = 0;
            for (int i = 0; i < txt.Count; i++)
            {
                if (txt[i].Contains("internalIDToNameTable"))
                {
                    inter = i;
                    txt[i] = "  internalIDToNameTable:";
                }
                if (txt[i].Contains("externalObjects:"))
                {
                    var lll = txt.GetRange(inter + 1, i - inter - 1);
                    lll.RemoveAll(x => !x.Contains("      213: ") && !x.Contains("    second: "));
                    var ddd = s.Split('\n');
                    for (int k = 0; k < ddd.Length; k++)
                    {
                        if (ddd[k].Contains("    second:"))
                        {
                            for (int j = 0; j < lll.Count; j++)
                            {
                                if (lll[j] == ddd[k])
                                {
                                    ddd[k - 1] = lll[j - 1];
                                }
                            }
                        }
                    }

                    txt.RemoveRange(inter + 1, i - inter - 1);
                    txt.Insert(inter + 1, string.Join("\n", ddd));
                    i = inter + 2;
                }


                if (txt[i].Contains("    sprites:"))
                {
                    inter = i;
                    txt[i] = "    sprites:";
                }

                if (txt[i].Contains("mipmapLimitGroupName: "))
                {
                    lastI = i;
                    break;
                }
            }

            var ls = txt.GetRange(inter + 1, lastI - inter - 11);
            ls.RemoveAll(x => !x.Contains("      name: ") && !x.Contains("      spriteID: ") && !x.Contains("      internalID: "));
            var data2 = data.Split('\n');
            for (int k = 0; k < data2.Length; k++)
            {
                if (data2[k].Contains("      name: "))
                {
                    for (int j = 0; j < ls.Count; j++)
                    {
                        if (ls[j] == data2[k])
                        {
                            data2[k + 14] = ls[j + 1];
                            data2[k + 15] = ls[j + 2];
                        }
                    }
                }
            }

            data = string.Join("\n", data2);

            txt.RemoveRange(inter + 1, lastI - inter - 11);
            txt.Insert(inter + 1, data);

            File.WriteAllLines(AssetDatabase.GetAssetPath(atlas.texture) + ".meta", txt);
        }
        AssetDatabase.Refresh();

        // var activeGameObject = Selection.activeGameObject;
        // if (activeGameObject == null) return;
        //
        // for (var index = 0; index < Selection.gameObjects.Length; index++)
        // {
        //     var obj = Selection.gameObjects[index];
        //
        //     obj.gameObject.SetActive(!activeGameObject.activeSelf);
        //     EditorUtility.SetDirty(obj);
        // }
        //
        // Selection.activeGameObject = activeGameObject;
    }



    [MenuItem("Atlas/CutAtlas")]
    private static void CutAtlas()
    {
        var path = new FileInfo(AssetDatabase.GetAssetPath(Selection.activeObject));
        var spr = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(Selection.activeObject));
        var tTu = Selection.activeObject as Texture2D;
        var dir = path.DirectoryName + $"/{tTu.name}/";
        Directory.CreateDirectory(dir);

        foreach (var item in spr)
        {
            if (!(item is Sprite)) return;
            var texture = new Texture2D((int)((Sprite)item).rect.width, (int)((Sprite)item).rect.height);
            texture.SetPixels(tTu.GetPixels((int)(((Sprite)item).rect.x), (int)(((Sprite)item).rect.y), (int)((Sprite)item).rect.width, (int)((Sprite)item).rect.height));
            texture.Apply(true);
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(dir + item.name + ".png", bytes);
        }
        AssetDatabase.Refresh();
    }


    // [MenuItem("Atlas/MergerAtlas")]
    // private static void MergerAtlas()
    // {
    //     var path = new FileInfo(AssetDatabase.GetAssetPath(Selection.activeObject));
    //     var newMeta = File.ReadAllText(path + ".meta");
    //
    //     var preFile = path.DirectoryName.Replace("\\", "/").Split("/").Last() + ".png";
    //     var meta =  preFile.DirectoryName;
    //
    //     
    //     
    //     var idx = newMeta.IndexOf("  spriteSheet:");
    //     var lastIdx = newMeta.IndexOf("    nameFileIdTable:");
    //     var ss = newMeta.Substring(idx, lastIdx - idx);
    //     
    //     AssetDatabase.Refresh();
    // }



    [MenuItem("Tool/FormatSource")]
    private static void FormatSource()
    {
        // var objs = Resources.FindObjectsOfTypeAll<Material>();
        // var count = 0;
        // foreach (var ma in objs)
        // {
        //     var filePath = AssetDatabase.GetAssetPath(ma);
        //     if (filePath.StartsWith("Assets/Materials/InvItems") && File.Exists(filePath))
        //     {
        //         if (ma != null && ma.GetTexture("_BumpMap") != null)
        //         {
        //             filePath = AssetDatabase.GetAssetPath(ma.GetTexture("_BumpMap"));
        //             if (!string.IsNullOrEmpty(filePath))
        //             {
        //                 var fileInf = new FileInfo(filePath);
        //                 if (File.Exists(filePath))
        //                 {
        //                     File.Move(filePath, "Assets/Images/InvItems/" + fileInf.Name);
        //                     File.Move(filePath + ".meta", "Assets/Images/InvItems/" + fileInf.Name + ".meta");
        //                     count++;
        //                 }
        //             }
        //         }
        //
        //         if (ma != null && ma.GetTexture("_MainTex") != null)
        //         {
        //             filePath = AssetDatabase.GetAssetPath(ma.GetTexture("_MainTex"));
        //             if (!string.IsNullOrEmpty(filePath))
        //             {
        //                 var fileInf = new FileInfo(filePath);
        //                 if (File.Exists(filePath))
        //                 {
        //                     File.Move(filePath, "Assets/Images/InvItems/" + fileInf.Name);
        //                     File.Move(filePath + ".meta", "Assets/Images/InvItems/" + fileInf.Name + ".meta");
        //                     count++;
        //                 }
        //             }
        //         }
        //     }
        // }

        // var objs = Resources.FindObjectsOfTypeAll<Material>();
        // var count = 0;
        // foreach (var ma in objs)
        // {
        //     var filePath = AssetDatabase.GetAssetPath(ma);
        //     if (filePath.StartsWith("Assets/Materials/Environment/TileTypes") && File.Exists(filePath))
        //     {
        //         if (ma != null && ma.GetTexture("_BumpMap") != null)
        //         {
        //             filePath = AssetDatabase.GetAssetPath(ma.GetTexture("_BumpMap"));
        //             if (!string.IsNullOrEmpty(filePath))
        //             {
        //                 var fileInf = new FileInfo(filePath);
        //                 if (File.Exists(filePath))
        //                 {
        //                     File.Move(filePath, "Assets/Images/Environment/TileTypes/" + fileInf.Name);
        //                     File.Move(filePath + ".meta", "Assets/Images/Environment/TileTypes/" + fileInf.Name + ".meta");
        //                     count++;
        //                 }
        //             }
        //         }
        //
        //         if (ma != null && ma.GetTexture("_MainTex") != null)
        //         {
        //             filePath = AssetDatabase.GetAssetPath(ma.GetTexture("_MainTex"));
        //             if (!string.IsNullOrEmpty(filePath))
        //             {
        //                 var fileInf = new FileInfo(filePath);
        //                 if (File.Exists(filePath))
        //                 {
        //                     File.Move(filePath, "Assets/Images/Environment/TileTypes/" + fileInf.Name);
        //                     File.Move(filePath + ".meta", "Assets/Images/Environment/TileTypes/" + fileInf.Name + ".meta");
        //                     count++;
        //                 }
        //             }
        //         }
        //     }
        // }

        // var objs = Resources.FindObjectsOfTypeAll<MeshFilter>();
        // var count = 0;
        // foreach (var ob in objs)
        // {
        //     var filePath = AssetDatabase.GetAssetPath(ob);
        //     if (filePath.StartsWith("Assets/Prefab/Environment") && File.Exists(filePath))
        //     {
        //         filePath = AssetDatabase.GetAssetPath(ob.sharedMesh);
        //         if(string.IsNullOrEmpty(filePath)) continue;
        //             
        //         var fileInf = new FileInfo(filePath);
        //         if (File.Exists(filePath))
        //         {
        //             File.Move(filePath, "Assets/Meshs/Environment/" + fileInf.Name);
        //             File.Move(filePath + ".meta", "Assets/Meshs/Environment/" + fileInf.Name + ".meta");
        //             count++;
        //         }
        //     }
        // }

        // var objs = Resources.FindObjectsOfTypeAll<MeshRenderer>();
        // var count = 0;
        // foreach (var ob in objs)
        // {
        //     var filePath = AssetDatabase.GetAssetPath(ob);
        //     if (filePath.StartsWith("Assets/Prefab/Environment") && File.Exists(filePath))
        //     {
        //         foreach (var ma in ob.sharedMaterials)
        //         {
        //             filePath = AssetDatabase.GetAssetPath(ma);
        //             if(string.IsNullOrEmpty(filePath)) continue;
        //             
        //             var fileInf = new FileInfo(filePath);
        //             if (File.Exists(filePath))
        //             {
        //                 File.Move(filePath, "Assets/Materials/Environment/" + fileInf.Name);
        //                 File.Move(filePath + ".meta", "Assets/Materials/Environment/" + fileInf.Name + ".meta");
        //             }
        //             count++;
        //         }
        //     }
        // }

        // var objs = Resources.FindObjectsOfTypeAll<TileTypes>();
        // var count = 0;
        // foreach (var ob in objs)
        // {
        //     var filePath = AssetDatabase.GetAssetPath(ob);
        //     if (filePath.StartsWith("Assets/Prefab/Environment/TileTypes") && File.Exists(filePath))
        //     {
        //         filePath = AssetDatabase.GetAssetPath(ob.myTileMaterial);
        //         if (string.IsNullOrEmpty(filePath)) continue;
        //
        //         var fileInf = new FileInfo(filePath);
        //         if (File.Exists(filePath))
        //         {
        //             File.Move(filePath, "Assets/Materials/Environment/TileTypes/" + fileInf.Name);
        //             File.Move(filePath + ".meta", "Assets/Materials/Environment/TileTypes/" + fileInf.Name + ".meta");
        //             count++;
        //         }
        //     }
        // }

        // var objs = Resources.FindObjectsOfTypeAll<RectTransform>();
        // var count = 0;
        // foreach (var ob in objs)
        // {
        //     if (ob.root.GetComponent<RectTransform>() != null)
        //     {
        //         var filePath = AssetDatabase.GetAssetPath(ob);
        //         if (filePath.StartsWith("Assets/Prefab") && File.Exists(filePath))
        //         {
        //             var fileInf = new FileInfo(filePath);
        //             File.Move(filePath, "Assets/Prefab/UI/" + fileInf.Name);
        //             File.Move(filePath + ".meta", "Assets/Prefab/UI/" + fileInf.Name + ".meta");
        //             count++;
        //         }
        //     }
        //     
        // }
        // var objs = FindObjectOfType<WorldManager>();
        // var count = 0;
        // foreach (var ob in objs.allObjects)
        // {
        //     var filePath = AssetDatabase.GetAssetPath(ob);
        //     if (filePath.StartsWith("Assets/Prefab") && File.Exists(filePath))
        //     {
        //         var fileInf = new FileInfo(filePath);
        //         File.Move(filePath, "Assets/Prefab/Environment/" + fileInf.Name);
        //         File.Move(filePath + ".meta", "Assets/Prefab/Environment/" + fileInf.Name + ".meta");
        //         count++;
        //     }
        // }

        // var objs = Resources.FindObjectsOfTypeAll<RectTransform>();
        // var count = 0;
        // foreach (var ob in objs)
        // {
        //     var cps = ob.GetComponents<MonoBehaviour>();
        //     foreach (var mn in cps)
        //     {
        //         var filePath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(mn));
        //         if (filePath.StartsWith("Assets/Scripts") && File.Exists(filePath))
        //         {
        //             var fileInf = new FileInfo(filePath);
        //             File.Move(filePath, "Assets/Scripts/UI/" + fileInf.Name);
        //             File.Move(filePath + ".meta", "Assets/Scripts/UI/" + fileInf.Name + ".meta");
        //             count++;
        //         }
        //     }
        // }


        // var objs = Resources.FindObjectsOfTypeAll<BiomSpawnTable>();
        // var count = 0;
        // foreach (var ob in objs)
        // {
        //     var filePath = AssetDatabase.GetAssetPath(ob);
        //     if (filePath.StartsWith("Assets/Prefab") && File.Exists(filePath))
        //     {
        //         var fileInf = new FileInfo(filePath);
        //         File.Move(filePath, "Assets/Prefab/BiomeSpawnTable/" + fileInf.Name);
        //         File.Move(filePath + ".meta", "Assets/Prefab/BiomeSpawnTable/" + fileInf.Name + ".meta");
        //         count++;
        //     }
        // }

        // var objs = FindObjectOfType<Inventory>();
        // var count = 0;
        // foreach (var ob in objs.allItems)
        // {
        //     var filePath = AssetDatabase.GetAssetPath(ob);
        //     if (filePath.StartsWith("Assets/Prefab") && File.Exists(filePath))
        //     {
        //         var fileInf = new FileInfo(filePath);
        //         File.Move(filePath, "Assets/Prefab/InvItems/" + fileInf.Name);
        //         File.Move(filePath + ".meta", "Assets/Prefab/InvItems/" + fileInf.Name + ".meta");
        //         count++;
        //     }
        // }

        // var objs = Resources.FindObjectsOfTypeAll<InventoryItem>();
        // var count = 0;
        // foreach (var ob in objs)
        // {
        //     if (ob.equipable != null && ob.equipable.material != null)
        //     {
        //         var filePath = AssetDatabase.GetAssetPath(ob);
        //         if (filePath.StartsWith("Assets/Prefab/InvItems") && File.Exists(filePath))
        //         {
        //              filePath = AssetDatabase.GetAssetPath(ob.equipable.material);
        //              if (File.Exists(filePath))
        //              {
        //                  var fileInf = new FileInfo(filePath);
        //                  File.Move(filePath, "Assets/Materials/InvItems/" + fileInf.Name);
        //                  File.Move(filePath + ".meta", "Assets/Materials/InvItems/" + fileInf.Name + ".meta");
        //                  count++;
        //              }
        //         }
        //     }   
        //  
        // }

        // var objs = Resources.FindObjectsOfTypeAll<GameObject>();
        // var count = 0;
        // foreach (var go in objs)
        // {
        //     foreach (var ob in go.GetComponentsInChildren<MeshFilter>())
        //     {
        //         var filePath = AssetDatabase.GetAssetPath(ob);
        //         if (filePath.StartsWith("Assets/Prefab/Tools") && File.Exists(filePath))
        //         {
        //             filePath = AssetDatabase.GetAssetPath(ob.sharedMesh);
        //             if (string.IsNullOrEmpty(filePath)) continue;
        //
        //             var fileInf = new FileInfo(filePath);
        //             if (File.Exists(filePath))
        //             {
        //                 File.Move(filePath, "Assets/Meshs/Tools/" + fileInf.Name);
        //                 File.Move(filePath + ".meta", "Assets/Meshs/Tools/" + fileInf.Name + ".meta");
        //                 count++;
        //             }
        //         }
        //     }
        // }

        var count = 0;
        Debug.Log($"Moving:{count} File");
    }

    [MenuItem("Tool/a _a")]
    private static void ActiveObj()
    {
        Debug.LogError(256 >> 4);
    }
    //     // var activeGameObject = Selection.activeGameObject;
    //     // if (activeGameObject == null) return;
    //     //
    //     // for (var index = 0; index < Selection.gameObjects.Length; index++)
    //     // {
    //     //     var obj = Selection.gameObjects[index];
    //     //
    //     //     obj.transform.localScale = new Vector3(obj.transform.localScale.x, obj.transform.localScale.z, 1);
    //     //     EditorUtility.SetDirty(obj);
    //     // }
    //     //
    //     // Selection.activeGameObject = activeGameObject;
    //     if (Selection.activeGameObject != null)
    //     {
    //         r = (Transform) Selection.activeGameObject.transform;
    //     }
    //     else
    //     {
    //         AvatarMask a = (AvatarMask) Selection.activeObject;
    //         // var t = r.GetComponentsInChildren<Transform>();
    //         // foreach (var aa in t)
    //         // {
    //         //     a.AddTransformPath(aa);
    //         //
    //         // }
    //         a.AddTransformPath(r);
    //     }
    //
    // }

    [MenuItem("Tool/b _b")]
    private static void ReplaceMaterial()
    {
        var activeGameObject = Selection.objects;
        if (activeGameObject == null) return;

        for (var index = 0; index < activeGameObject.Length; index++)
        {
            var p = AssetDatabase.GetAssetPath(activeGameObject[index]).Replace("Materials", "Materials/New");
            var m = AssetDatabase.LoadAssetAtPath<Material>(p);
            ((Material)activeGameObject[index]).shader = m.shader;
            EditorUtility.SetDirty(activeGameObject[index]);
        }

    }

    public enum FaceDirection
    {
        North,
        East,
        South,
        West,
        Up,
        Down
    }

    [MenuItem("Tool/Mesh Data")]
    private static void MeshData()
    {
        var activeGameObject = Selection.objects;
        if (activeGameObject == null) return;

        for (var index = 0; index < activeGameObject.Length; index++)
        {
            var mesh = activeGameObject[index] as Mesh;
            if (mesh == null) continue;
            var vertices = mesh.vertices;
            var tris = mesh.triangles;
            var faceTris = new List<Vector3[]>();
            var xFaceSquare = new List<Vector3[]>();
            var yFaceSquare = new List<Vector3[]>();
            var zFaceSquare = new List<Vector3[]>();

            var xFace = new List<Vector3[]>();
            var yFace = new List<Vector3[]>();
            var zFace = new List<Vector3[]>();

            var xBox = new List<Vector3[]>();
            var yBox = new List<Vector3[]>();
            var zBox = new List<Vector3[]>();

            var box = new List<Vector3[]>();


            for (int i = 0; i < tris.Length; i += 3)
            {
                faceTris.Add(new[] { vertices[tris[i]], vertices[tris[i + 1]], vertices[tris[i + 2]] });
            }

            for (int i = 0; i < faceTris.Count; i++)
            {
                var face = faceTris[i];
                if (Mathf.Abs(face[0].y - face[1].y) < .1f && Mathf.Abs(face[0].y - face[2].y) < .1f)
                {
                    yFace.Add(face);
                }
                else if (Mathf.Abs(face[0].x - face[1].x) < .1f && Mathf.Abs(face[0].x - face[2].x) < .1f)
                {
                    xFace.Add(face);
                }
                else if (Mathf.Abs(face[0].z - face[1].z) < .1f && Mathf.Abs(face[0].z - face[2].z) < .1f)
                {
                    zFace.Add(face);
                }
            }

            // Debug.LogError(xFace.Count);
            // Debug.LogError(yFace.Count);
            // Debug.LogError(zFace.Count);
            // Debug.LogError(xFace.Count + yFace.Count + zFace.Count);


            var removeIdx = new List<int>();
            for (int i = 0; i < xFace.Count; i++)
            {
                if (removeIdx.Contains(i)) continue;
                var f1 = xFace[i];
                for (int j = 0; j < xFace.Count; j++)
                {
                    if (removeIdx.Contains(j)) continue;
                    var f2 = xFace[j];
                    if (j != i)
                    {
                        var mixF = 0;
                        var d1 = new Vector3();
                        var d2 = new Vector3();
                        for (int k = 0; k < f1.Length; k++)
                        {
                            for (int l = 0; l < f2.Length; l++)
                            {
                                if (Mathf.Abs(f1[k].x - f2[l].x) < .1f && Mathf.Abs(f1[k].y - f2[l].y) < .1f && Mathf.Abs(f1[k].z - f2[l].z) < .1f)
                                {
                                    if (mixF == 0)
                                    {
                                        d1 = f1[k];
                                    }
                                    else
                                    {
                                        d2 = f1[k];
                                    }
                                    mixF++;
                                    // break;
                                }
                            }
                        }

                        if (mixF == 2)
                        {
                            var d = Mathf.RoundToInt((d1 - d2).sqrMagnitude);
                            if (d == 2)
                            {
                                var lst = new List<Vector3>();
                                lst.AddRange(f1);
                                lst.AddRange(f2);

                                lst.RemoveAll(x => Mathf.Abs(x.x - d1.x) < .1f && Mathf.Abs(x.y - d1.y) < .1f && Mathf.Abs(x.z - d1.z) < .1f);
                                lst.RemoveAll(x => Mathf.Abs(x.x - d2.x) < .1f && Mathf.Abs(x.y - d2.y) < .1f && Mathf.Abs(x.z - d2.z) < .1f);
                                lst.Add(d1);
                                lst.Add(d2);

                                removeIdx.Add(i);
                                removeIdx.Add(j);
                                xFaceSquare.Add(lst.ToArray());
                            }

                            break;
                        }
                    }
                }
            }

            removeIdx = new List<int>();
            for (int i = 0; i < yFace.Count; i++)
            {
                if (removeIdx.Contains(i)) continue;
                var f1 = yFace[i];
                for (int j = 0; j < yFace.Count; j++)
                {
                    if (removeIdx.Contains(j)) continue;
                    var f2 = yFace[j];
                    if (j != i)
                    {
                        var mixF = 0;
                        var d1 = new Vector3();
                        var d2 = new Vector3();
                        for (int k = 0; k < f1.Length; k++)
                        {
                            for (int l = 0; l < f2.Length; l++)
                            {
                                if (Mathf.Abs(f1[k].x - f2[l].x) < .1f && Mathf.Abs(f1[k].y - f2[l].y) < .1f && Mathf.Abs(f1[k].z - f2[l].z) < .1f)
                                {
                                    if (mixF == 0)
                                    {
                                        d1 = f1[k];
                                    }
                                    else
                                    {
                                        d2 = f1[k];
                                    }
                                    mixF++;
                                    // break;
                                }
                            }
                        }

                        if (mixF == 2)
                        {
                            var d = Mathf.RoundToInt((d1 - d2).sqrMagnitude);
                            if (d == 2)
                            {
                                var lst = new List<Vector3>();
                                lst.AddRange(f1);
                                lst.AddRange(f2);

                                lst.RemoveAll(x => Mathf.Abs(x.x - d1.x) < .1f && Mathf.Abs(x.y - d1.y) < .1f && Mathf.Abs(x.z - d1.z) < .1f);
                                lst.RemoveAll(x => Mathf.Abs(x.x - d2.x) < .1f && Mathf.Abs(x.y - d2.y) < .1f && Mathf.Abs(x.z - d2.z) < .1f);
                                lst.Add(d1);
                                lst.Add(d2);

                                removeIdx.Add(i);
                                removeIdx.Add(j);
                                yFaceSquare.Add(lst.ToArray());
                            }

                            break;
                        }
                    }
                }
            }

            removeIdx = new List<int>();
            for (int i = 0; i < zFace.Count; i++)
            {
                if (removeIdx.Contains(i)) continue;
                var f1 = zFace[i];
                for (int j = 0; j < zFace.Count; j++)
                {
                    if (removeIdx.Contains(j)) continue;
                    var f2 = zFace[j];
                    if (j != i)
                    {
                        var mixF = 0;
                        var d1 = new Vector3();
                        var d2 = new Vector3();
                        for (int k = 0; k < f1.Length; k++)
                        {
                            for (int l = 0; l < f2.Length; l++)
                            {
                                if (Mathf.Abs(f1[k].x - f2[l].x) < .1f && Mathf.Abs(f1[k].y - f2[l].y) < .1f && Mathf.Abs(f1[k].z - f2[l].z) < .1f)
                                {
                                    if (mixF == 0)
                                    {
                                        d1 = f1[k];
                                    }
                                    else
                                    {
                                        d2 = f1[k];
                                    }
                                    mixF++;
                                    // break;
                                }
                            }
                        }

                        if (mixF == 2)
                        {
                            var d = Mathf.RoundToInt((d1 - d2).sqrMagnitude);
                            if (d == 2)
                            {
                                var lst = new List<Vector3>();
                                lst.AddRange(f1);
                                lst.AddRange(f2);

                                lst.RemoveAll(x => Mathf.Abs(x.x - d1.x) < .1f && Mathf.Abs(x.y - d1.y) < .1f && Mathf.Abs(x.z - d1.z) < .1f);
                                lst.RemoveAll(x => Mathf.Abs(x.x - d2.x) < .1f && Mathf.Abs(x.y - d2.y) < .1f && Mathf.Abs(x.z - d2.z) < .1f);
                                lst.Add(d1);
                                lst.Add(d2);

                                removeIdx.Add(i);
                                removeIdx.Add(j);
                                zFaceSquare.Add(lst.ToArray());
                            }

                            break;
                        }
                    }
                }
            }


            ///////////////////////
            for (int i = 0; i < yFaceSquare.Count; i++)
            {
                var f1 = yFaceSquare[i];
                for (int j = 0; j < yFaceSquare.Count; j++)
                {
                    var f2 = yFaceSquare[j];
                    if (j != i)
                    {
                        var mixF = 0;
                        for (int k = 0; k < f1.Length; k++)
                        {
                            for (int l = 0; l < f2.Length; l++)
                            {
                                if (Mathf.Abs(f1[k].x - f2[l].x) < .1f && Mathf.Abs(f1[k].y - (f2[l].y + 1)) < .1f && Mathf.Abs(f1[k].z - f2[l].z) < .1f)
                                {
                                    mixF++;
                                    // break;
                                }
                            }
                        }

                        if (mixF == 4)
                        {
                            var lst = new List<Vector3>();
                            lst.AddRange(f1);
                            lst.AddRange(f2);
                            // removeIdx.Add(i);
                            // removeIdx.Add(j);
                            f2 = lst.ToArray();
                            for (int m = 0; m < yBox.Count; m++)
                            {
                                mixF = 0;
                                f1 = yBox[m];
                                for (int k = 0; k < f1.Length; k++)
                                {
                                    for (int l = 0; l < f2.Length; l++)
                                    {
                                        if (Mathf.Abs(f1[k].x - f2[l].x) < .1f && Mathf.Abs(f1[k].y - f2[l].y) < .1f && Mathf.Abs(f1[k].z - f2[l].z) < .1f)
                                        {
                                            mixF++;
                                            // break;
                                        }
                                    }
                                }
                                if (mixF == 8) break;
                            }

                            if (mixF != 8)
                            {
                                yBox.Add(lst.ToArray());
                            }
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < xFaceSquare.Count; i++)
            {
                var f1 = xFaceSquare[i];
                for (int j = 0; j < xFaceSquare.Count; j++)
                {
                    var f2 = xFaceSquare[j];
                    if (j != i)
                    {
                        var mixF = 0;
                        for (int k = 0; k < f1.Length; k++)
                        {
                            for (int l = 0; l < f2.Length; l++)
                            {
                                if (Mathf.Abs(f1[k].x - (f2[l].x + 1)) < .1f && Mathf.Abs(f1[k].y - f2[l].y) < .1f && Mathf.Abs(f1[k].z - f2[l].z) < .1f)
                                {
                                    mixF++;
                                    // break;
                                }
                            }
                        }

                        if (mixF == 4)
                        {
                            var lst = new List<Vector3>();
                            lst.AddRange(f1);
                            lst.AddRange(f2);
                            // removeIdx.Add(i);
                            // removeIdx.Add(j);
                            f2 = lst.ToArray();
                            for (int m = 0; m < xBox.Count; m++)
                            {
                                mixF = 0;
                                f1 = xBox[m];
                                for (int k = 0; k < f1.Length; k++)
                                {
                                    for (int l = 0; l < f2.Length; l++)
                                    {
                                        if (Mathf.Abs(f1[k].x - f2[l].x) < .1f && Mathf.Abs(f1[k].y - f2[l].y) < .1f && Mathf.Abs(f1[k].z - f2[l].z) < .1f)
                                        {
                                            mixF++;
                                            // break;
                                        }
                                    }
                                }
                                if (mixF == 8) break;
                            }

                            if (mixF != 8)
                            {
                                xBox.Add(lst.ToArray());
                            }
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < zFaceSquare.Count; i++)
            {
                var f1 = zFaceSquare[i];
                for (int j = 0; j < zFaceSquare.Count; j++)
                {
                    var f2 = zFaceSquare[j];
                    if (j != i)
                    {
                        var mixF = 0;
                        for (int k = 0; k < f1.Length; k++)
                        {
                            for (int l = 0; l < f2.Length; l++)
                            {
                                if (Mathf.Abs(f1[k].x - f2[l].x) < .1f && Mathf.Abs(f1[k].y - f2[l].y) < .1f && Mathf.Abs(f1[k].z - (f2[l].z + 1)) < .1f)
                                {
                                    mixF++;
                                    // break;
                                }
                            }
                        }

                        if (mixF == 4)
                        {
                            var lst = new List<Vector3>();
                            lst.AddRange(f1);
                            lst.AddRange(f2);
                            // removeIdx.Add(i);
                            // removeIdx.Add(j);
                            mixF = 0;
                            f2 = lst.ToArray();
                            for (int m = 0; m < zBox.Count; m++)
                            {
                                mixF = 0;
                                f1 = zBox[m];
                                for (int k = 0; k < f1.Length; k++)
                                {
                                    for (int l = 0; l < f2.Length; l++)
                                    {
                                        if (Mathf.Abs(f1[k].x - f2[l].x) < .1f && Mathf.Abs(f1[k].y - f2[l].y) < .1f && Mathf.Abs(f1[k].z - f2[l].z) < .1f)
                                        {
                                            mixF++;
                                            // break;
                                        }
                                    }
                                }
                                if (mixF == 8) break;
                            }

                            if (mixF != 8)
                            {
                                zBox.Add(lst.ToArray());
                            }
                            break;
                        }
                    }
                }
            }


            for (int i = 0; i < yBox.Count; i++)
            {
                var f1 = yBox[i];
                for (int j = 0; j < xBox.Count; j++)
                {
                    var f2 = xBox[j];

                    var mixF = 0;
                    for (int k = 0; k < f1.Length; k++)
                    {
                        for (int l = 0; l < f2.Length; l++)
                        {
                            if (Mathf.Abs(f1[k].x - f2[l].x) < .1f && Mathf.Abs(f1[k].y - f2[l].y) < .1f && Mathf.Abs(f1[k].z - f2[l].z) < .1f)
                            {
                                mixF++;
                                // break;
                            }
                        }
                    }

                    if (mixF == 8)
                    {
                        for (int o = 0; o < zBox.Count; o++)
                        {
                            f2 = zBox[o];

                            mixF = 0;
                            for (int k = 0; k < f1.Length; k++)
                            {
                                for (int l = 0; l < f2.Length; l++)
                                {
                                    if (Mathf.Abs(f1[k].x - f2[l].x) < .1f && Mathf.Abs(f1[k].y - f2[l].y) < .1f && Mathf.Abs(f1[k].z - f2[l].z) < .1f)
                                    {
                                        mixF++;
                                        // break;
                                    }
                                }
                            }

                            if (mixF == 8)
                            {
                                box.Add(f1);
                            }
                        }

                    }
                }
            }


            var arr = new List<Vector3Int>();
            var delta = new Vector3();
            for (int i = 0; i < box.Count; i++)
            {
                var b1 = box[i];
                var f1 = b1[0];
                for (int j = 1; j < b1.Length; j++)
                {
                    var f2 = b1[j];
                    if (Mathf.Abs(f1.x - f2.x) > .1f && Mathf.Abs(f1.y - f2.y) > .1f && Mathf.Abs(f1.z - f2.z) > .1f)
                    {
                        var v = (f2 + f1) / 2;
                        arr.Add(new Vector3Int((int)(v.x + .99f), (int)(v.y + .99f), (int)(v.z + .99f)));
                        if (arr.Count == 1)
                        {
                            delta.x = (int)(v.x + .99f) - v.x;
                            delta.y = (int)(v.y + .99f) - v.y;
                            delta.z = (int)(v.z + .99f) - v.z;
                        }
                        Debug.LogError(v);
                        break;
                    }
                }
            }

            Debug.LogError("delta: " + delta);
            var xMin = Int32.MaxValue;
            var yMin = Int32.MaxValue;
            var zMin = Int32.MaxValue;
            for (int i = 0; i < arr.Count; i++)
            {
                if (arr[i].x < xMin)
                {
                    xMin = arr[i].x;
                }

                if (arr[i].y < yMin)
                {
                    yMin = arr[i].y;
                }

                if (arr[i].z < zMin)
                {
                    zMin = arr[i].z;
                }
            }

            delta = new Vector3(xMin, yMin, zMin) - delta;
            Debug.LogError("delta2: " + delta);
            for (int i = 0; i < arr.Count; i++)
            {
                arr[i] -= new Vector3Int(xMin, yMin, zMin);
                Debug.LogError(arr[i]);
            }
        }
    }




    [MenuItem("Tool/Save Mesh")]
    private static void SaveMesh()
    {
        var me = Selection.activeObject as Mesh;
        var ver = me.vertices;
        for (int j = 0; j < ver.Length; j++)
        {
            ver[j] += new Vector3(0, -0.5f, 0f);
        }
        me.SetVertices(ver);
        
        
        // // List<Vector4> uvs = new List<Vector4>();
        // // me.GetUVs(0, uvs);
        // // for (int j = 0; j < uvs.Count; j+=4)
        // // {
        // //     if (uvs[j].x == .375f && uvs[j].y == .78125f)
        // //     {
        // //         uvs[j] = new Vector4(0, 0, uvs[j].z, uvs[j].w);
        // //         uvs[j + 1] = new Vector4(0, .5f, uvs[j + 1].z, uvs[j + 1].w);
        // //         uvs[j + 2] = new Vector4(.5f, 0, uvs[j + 2].z, uvs[j + 2].w);
        // //         uvs[j + 3] = new Vector4(.5f, .5f, uvs[j + 3].z, uvs[j + 3].w);
        // //     }
        // //     else if (uvs[j].x == .40625f && uvs[j].y == .78125f)
        // //     {
        // //         uvs[j] = new Vector4(0, .5f, uvs[j].z, uvs[j].w);
        // //         uvs[j + 1] = new Vector4(0, 1f, uvs[j + 1].z, uvs[j + 1].w);
        // //         uvs[j + 2] = new Vector4(.5f, .5f, uvs[j + 2].z, uvs[j + 2].w);
        // //         uvs[j + 3] = new Vector4(.5f, 1f, uvs[j + 3].z, uvs[j + 3].w);
        // //     }
        // //     else if (uvs[j].x == .53125f && uvs[j].y == .78125f)
        // //     {
        // //         uvs[j] = new Vector4(.5f, .5f, uvs[j].z, uvs[j].w);
        // //         uvs[j + 1] = new Vector4(.5f, 1f, uvs[j + 1].z, uvs[j + 1].w);
        // //         uvs[j + 2] = new Vector4(1f, .5f, uvs[j + 2].z, uvs[j + 2].w);
        // //         uvs[j + 3] = new Vector4(1f, 1f, uvs[j + 3].z, uvs[j + 3].w);
        // //     }
        // //     else if (uvs[j].x == .5625f && uvs[j].y == .96875f)
        // //     {
        // //         uvs[j] = new Vector4(.5f, 0, uvs[j].z, uvs[j].w);
        // //         uvs[j + 1] = new Vector4(.5f, .5f, uvs[j + 1].z, uvs[j + 1].w);
        // //         uvs[j + 2] = new Vector4(1f, 0, uvs[j + 2].z, uvs[j + 2].w);
        // //         uvs[j + 3] = new Vector4(1f, .5f, uvs[j + 3].z, uvs[j + 3].w);
        // //     }
        // // }
        // //
        // // me.SetUVs(0, uvs);
        me.RecalculateBounds();
        AssetDatabase.CreateAsset(Instantiate(me), AssetDatabase.GetAssetPath(me));
        //
        // return;

        var i = 0;
        foreach (var va in Selection.gameObjects)
        {
            i++;
            //var p = AssetDatabase.GetAssetPath(va).Replace(".fbx", ".asset");
            Mesh mesh = va.GetComponent<MeshFilter>().mesh;//(Mesh)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(va), typeof(Mesh));
            AssetDatabase.CreateAsset(Instantiate(mesh), $"Assets/abc{i}.asset");

            va.GetComponent<MeshFilter>().mesh = AssetDatabase.LoadAssetAtPath<Mesh>($"Assets/abc{i}.asset");
        }
    }

    [MenuItem("Tool/Save CombineMesh")]
    private static void SaveCombineMesh()
    {
        var meshFilters = new List<MeshFilter>();


        foreach (var va in Selection.gameObjects)
        {
            //var p = AssetDatabase.GetAssetPath(va).Replace(".fbx", ".asset");
            var mesh = va.GetComponentsInChildren<MeshFilter>().Where(x => x != null && x.sharedMesh != null && x.sharedMesh.vertexCount > 0 && !meshFilters.Contains(x));//(Mesh)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(va), typeof(Mesh));
            meshFilters.AddRange(mesh);
        }

        var combineMeshInstanceDictionary = new Dictionary<Material, List<CombineInstance>>();

        foreach (var meshFilter in meshFilters)
        {
            var mesh = meshFilter.sharedMesh;
            var vertices = new List<Vector3>();
            var uvs = new List<List<Vector4>>();

            var materials = meshFilter.GetComponent<Renderer>().sharedMaterials;
            var subMeshCount = meshFilter.sharedMesh.subMeshCount;
            mesh.GetVertices(vertices);

            for (int i = 0; i < 8; i++)
            {
                var arr = new List<Vector4>();
                mesh.GetUVs(i, arr);
                uvs.Add(arr);
            }

            for (var i = 0; i < subMeshCount; i++)
            {
                var material = materials[i];
                var triangles = new List<int>();
                mesh.GetTriangles(triangles, i);

                var newMesh = new Mesh
                {
                    vertices = vertices.ToArray(),
                    triangles = triangles.ToArray(),
                    normals = mesh.normals,
                    colors = mesh.colors
                };

                for (int k = 0; k < 8; k++)
                {
                    newMesh.SetUVs(k, uvs[k]);
                }

                if (!combineMeshInstanceDictionary.ContainsKey(material))
                {
                    combineMeshInstanceDictionary.Add(material, new List<CombineInstance>());
                }

                var combineInstance = new CombineInstance { transform = meshFilter.transform.localToWorldMatrix, mesh = newMesh };
                combineMeshInstanceDictionary[material].Add(combineInstance);
            }
        }


        foreach (var kvp in combineMeshInstanceDictionary)
        {
            var mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.CombineMeshes(kvp.Value.ToArray());
            Unwrapping.GenerateSecondaryUVSet(mesh);

            AssetDatabase.CreateAsset(Instantiate(mesh), $"Assets/{kvp.Key.name}.asset");
        }
    }



    [MenuItem("Tool/CutSprite")]
    public static void CutSprite()
    {
        var spr = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(Selection.activeObject));
        var tTu = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GetAssetPath(Selection.activeObject));
        foreach (var item in spr)
        {
            if (!(item is Sprite)) return;
            var texture = new Texture2D((int)((Sprite)item).rect.width, (int)((Sprite)item).rect.height);
            texture.SetPixels(tTu.GetPixels((int)(((Sprite)item).rect.x), (int)(((Sprite)item).rect.y), (int)((Sprite)item).rect.width, (int)((Sprite)item).rect.height));
            texture.Apply(true);
            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes("Assets/CraftSmasher/Images/Cut/" + item.name + ".png", bytes);
        }
    }

    [MenuItem("Tool/ToMp3")]
    private static void ToMP3()
    {
        var t = Selection.objects;
        foreach (var VARIABLE in t)
        {
            AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(VARIABLE),
                AssetDatabase.GetAssetPath(VARIABLE).Replace(".wav", ".mp3"));
        }
    }

    [MenuItem("Tool/select sprite")]
    private static void Select()
    {
        var lis = new List<Object>();
        foreach (var obj in Selection.objects)
        {
            var s = ((GameObject)obj).GetComponent<SpriteRenderer>().sprite;
            DestroyImmediate(((GameObject)obj).GetComponent<SpriteRenderer>());
            EditorUtility.SetDirty(obj);
            //var t = (Texture2D) obj;
            //TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(t))as TextureImporter;
            //if(importer != null && importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single)
            //   lis.Add(t);
        }

        Selection.objects = lis.ToArray();
    }

    [MenuItem("Tools/Export Wizard")]
    static void SaveSprite()
    {

        // {
        //     UIAtlas obj = ((GameObject)Selection.activeObject).GetComponent<UIAtlas>();
        //     foreach (var spr in obj.GetListOfSprites())
        //     {
        //         UISpriteData sprite = obj.GetSprite(spr);

        //         // Create an export folder
        //         string outPath = Application.dataPath + "/outSprite/";
        //         System.IO.Directory.CreateDirectory(outPath);


        //         var se = UIAtlasMaker.ExtractSprite(obj, sprite.name);

        //         if (se != null)
        //         {
        //             var bytes = se.tex.EncodeToPNG();
        //             File.WriteAllBytes(outPath + "/" + sprite.name + ".png", bytes);
        //         }

        //         Debug.Log("SaveSprite to " + outPath);
        //     }
        // }
        Debug.Log("SaveSprite Finished");
    }

    [MenuItem("Assets/Open with pts")]
    private static void GetDataSkill()
    {
        Process photoViewer = new Process();
        photoViewer.StartInfo.FileName = @"C:\Program Files\Adobe\Adobe Photoshop CS6 (64 Bit)\Photoshop.exe";
        var inf = new FileInfo(AssetDatabase.GetAssetPath(Selection.activeObject));
        photoViewer.StartInfo.Arguments = inf.FullName;
        photoViewer.Start();
    }
}
