using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissingComponentRemove : MonoBehaviour
{
    private void Awake()
    {
        // Remove();
    }

    [MenuItem("GameObject/Remove Missing Scripts")]
    public static void Remove()
    {
        var objs = Resources.FindObjectsOfTypeAll<GameObject>();
        int count = objs.Sum(GameObjectUtility.RemoveMonoBehavioursWithMissingScript);
        foreach (var obj in objs)
        {
            EditorUtility.SetDirty(obj);
        }
        Debug.Log($"Removed {count} missing scripts");
    }
    
     [MenuItem("Window/TTT")]
     public static void Removegffgg()
     {
         var allNewPath = Directory.GetFiles(@"D:\GameUnity\Code\Assets\2DxFX\Resources");
         var allCurPath = Directory.GetFiles(@"D:\GameUnity\Code\Assets\Resources\2DxFX\Standard");
         foreach (var p in allCurPath)
         {
             if (p.Contains(".shader.meta"))
             {
                 var cfileInfo = new FileInfo(p);
                 foreach (var nP in allNewPath)
                 {
                     var nfileInfo =  new FileInfo(nP);
                     if (cfileInfo.Name.Replace("2DxFX_Standard_", "_2dxFX_") == nfileInfo.Name)
                     {
                         File.Delete(nP);
                         cfileInfo.MoveTo(nP);
                         File.Delete(p.Replace(".shader.meta", ".shader"));
                     }
                 }
             }
         }
     }

        // public static Vector2 UIGetElementDimension(GameObject mGameObject)
        // {
        //     Vector2 Res = -Vector2.one;
        //     RectTransform RectTransform = mGameObject.GetComponent<RectTransform>();
        //     if (RectTransform != null)
        //     {
        //         Res = RectTransform
        //             .sizeDelta; // Read GameObject Dimensions                                                     
        //     }
        //
        //     return Res;
        // }
        // [MenuItem("Window/Tffdff")]
        // public static void UIMaximizeElement(GameObject mGameObject, GameObject Parent = null, float Left = 0, float Right = 0, float Top = 0, float Bottom = 0) {
        //
        //     RectTransform RectTransform = mGameObject.GetComponent<RectTransform>();                    // Get Rect Transform Component from element
        //
        //     if (RectTransform != null) {
        //
        //         Vector2 ParentSize = -Vector2.one;
        //         if (mGameObject.transform.parent != null)                                               // Get Size of Parent
        //             ParentSize = UIGetElementDimension(mGameObject.transform.parent.gameObject);
        //         else if (Parent != null)
        //             ParentSize = UIGetElementDimension(Parent);
        //
        //         RectTransform.anchorMin = new Vector2(0, 0);                                            // Set Location respect to Axes, same thing then doit manually in Anchor Min and Max in inspector of Rect Transform
        //         RectTransform.anchorMax = new Vector2(1, 1);                                             
        //      
        //         RectTransform.pivot = new Vector2(0.5f, 0.5f);                                          // Pivot in the Middle
        //
        //         if (ParentSize != -Vector2.one) {
        //
        //             float SizeWidth = ParentSize.x - Left - Right;                                      // Calculate dimensions of Element;
        //             float SizeHeight = ParentSize.y - Top - Bottom;
        //
        //             RectTransform.offsetMin = Vector2.zero;                                             
        //             RectTransform.offsetMax = new Vector2(SizeWidth, SizeHeight);                       // Set dimensions 
        //          
        //             RectTransform.anchoredPosition = new Vector2(SizeWidth / 2 + Left, SizeHeight / 2 + Bottom);   // Anchored Position set automatically Left, Top, Right and Bottom
        //         }
        //     }
        // }

    /*[MenuItem("Window/move Sprite Spaceship")]
         public static void Remove()
         {
             var tmp = Resources.FindObjectsOfTypeAll<MGShipSkin>();
             var x = 0;
             for (int j = 0; j < tmp.Length; j++)
             {
                 x++;
                 var v = tmp[j];
                 var spriteRenderers = v.GetComponentsInChildren<SpriteRenderer>();
                 
                 for (int i = 0; i < spriteRenderers.Length; i++)
                 {
                     if(spriteRenderers[i].sprite == null) continue;
                     var fileName = spriteRenderers[i].sprite.name + ".png";
                     var path = AssetDatabase.GetAssetPath(spriteRenderers[i].sprite);
                     var newPath = $"Assets/Sprites/Spaceships/{v.transform.parent.gameObject.name}/{v.gameObject.name}/{fileName}";
                     try
                     {
                         // if(path.Contains("Assets/Sprites/Spaceships/")) continue;
                         if (!Directory.Exists($"Assets/Sprites/Spaceships/{v.transform.parent.gameObject.name}"))
                             Directory.CreateDirectory($"Assets/Sprites/Spaceships/{v.transform.parent.gameObject.name}");
                         if (!Directory.Exists($"Assets/Sprites/Spaceships/{v.transform.parent.gameObject.name}/{v.gameObject.name}"))
                             Directory.CreateDirectory($"Assets/Sprites/Spaceships/{v.transform.parent.gameObject.name}/{v.gameObject.name}");
                      
                         if (File.Exists(path) && !File.Exists(newPath) && path != newPath)
                         {
                             File.Move(path, newPath);
                             File.Move(path + ".meta", newPath + ".meta");
                         }
                     }
                     catch (Exception e)
                     {
                         Debug.LogError(e + "////" + path);
                     }
     //                Thread.Sleep(100);
                 }
             }
         }
         
         
         [MenuItem("Window/move Sprite Miniship")]
         public static void Remove22()
         {
             var tmp = GameObject.FindGameObjectsWithTag("Test");
             var x = 0;
             for (int j = 0; j < tmp.Length; j++)
             {
                 x++;
                 var v = tmp[j];
                 var spriteRenderers = v.GetComponentsInChildren<SpriteRenderer>();
                 
                 for (int i = 0; i < spriteRenderers.Length; i++)
                 {
                     if(spriteRenderers[i].sprite == null) continue;
                     var fileName = spriteRenderers[i].sprite.name + ".png";
                     var path = AssetDatabase.GetAssetPath(spriteRenderers[i].sprite);
                     var newPath = $"Assets/Sprites/MiniSpaceShip/{j + 1}/{fileName}";
                     try
                     {
                         if (!Directory.Exists($"Assets/Sprites/MiniSpaceShip/{j + 1}"))
                             Directory.CreateDirectory($"Assets/Sprites/MiniSpaceShip/{j + 1}");
                      
                         if (File.Exists(path) && !File.Exists(newPath) && path != newPath)
                         {
                             File.Move(path, newPath);
                             File.Move(path + ".meta", newPath + ".meta");
                         }
                     }
                     catch (Exception e)
                     {
                         Debug.LogError(e + "////" + path);
                     }
     //                Thread.Sleep(100);
                 }
             }
         }
         
         [MenuItem("Window/move Sprite Boss")]
         public static void RemoveBoss()
         {
             var tmp = Resources.LoadAll<GameObject>("databosscampaign");
             var x = 0;
             for (int j = 0; j < tmp.Length; j++)
             {
                 x++;
                 var v = tmp[j];
                 var spriteRenderers = v.GetComponentsInChildren<SpriteRenderer>();
                 
                 for (int i = 0; i < spriteRenderers.Length; i++)
                 {
                     if(spriteRenderers[i].sprite == null) continue;
                     var fileName = spriteRenderers[i].sprite.name + ".png";
                     var path = AssetDatabase.GetAssetPath(spriteRenderers[i].sprite);
                     var newPath = $"Assets/Sprites/Boss/{x}/{fileName}";
                     try
                     {
                         // if(path.Contains("Assets/Sprites/Spaceships/")) continue;
                         if (!Directory.Exists($"Assets/Sprites/Boss/{x}"))
                             Directory.CreateDirectory($"Assets/Sprites/Boss/{x}");
                         if (File.Exists(path) && !File.Exists(newPath) && path != newPath)
                         {
                             File.Move(path, newPath);
                             File.Move(path + ".meta", newPath + ".meta");
                         }
                     }
                     catch (Exception e)
                     {
                         Debug.LogError(e + "////" + path);
                     }
     //                Thread.Sleep(100);
                 }
             }
         }
         
           
         [MenuItem("Window/move Sprite Enemy")]
         public static void RemoveEnemy()
         {
             var tmp = Resources.LoadAll<GameObject>("");
             int x = 0;
             foreach (var v in tmp)
             {
                 x++;
                 foreach (var spriteRenderers in v.GetComponentsInChildren<SpriteRenderer>())
                 {
                     if (spriteRenderers.sprite == null) continue;
                     var fileName = spriteRenderers.sprite.name + ".png";
                     var path = AssetDatabase.GetAssetPath(spriteRenderers.sprite);
                     var newPath = $"Assets/Sprites/Enemy/{x}/{fileName}";
                     if (path.Contains($"Assets/Sprites/Boss")) continue;
                     try
                     {
                         // if(path.Contains("Assets/Sprites/Spaceships/")) continue;
                         if (!Directory.Exists($"Assets/Sprites/Enemy/{x}"))
                             Directory.CreateDirectory($"Assets/Sprites/Enemy/{x}");
                         if (File.Exists(path) && !File.Exists(newPath) && path != newPath)
                         {
                             File.Move(path, newPath);
                             File.Move(path + ".meta", newPath + ".meta");
                         }
                     }
                     catch (Exception e)
                     {
                         Debug.LogError(e + "////" + path);
                     }
                 }
             }
         }
         
         [MenuItem("Window/move Sprite EnemyNew")]
         public static void RemoveEnemyNew()
         {
             var tmp = GameObject.FindObjectsOfType<SpriteRenderer>();
             int x = 1;
             foreach (var v in tmp)
             {
                 foreach (var spriteRenderers in tmp)
                 {
                     if (spriteRenderers.sprite == null) continue;
                     var fileName = spriteRenderers.sprite.name + ".png";
                     var path = AssetDatabase.GetAssetPath(spriteRenderers.sprite);
                     var newPath = $"Assets/Sprites/Enemy/{x}/{fileName}";
                     if (path.Contains($"Assets/Sprites/Boss")) continue;
                     try
                     {
                         // if(path.Contains("Assets/Sprites/Spaceships/")) continue;
                         if (!Directory.Exists($"Assets/Sprites/Enemy/{x}"))
                             Directory.CreateDirectory($"Assets/Sprites/Enemy/{x}");
                         if (File.Exists(path) && !File.Exists(newPath) && path != newPath)
                         {
                             File.Move(path, newPath);
                             File.Move(path + ".meta", newPath + ".meta");
                         }
                     }
                     catch (Exception e)
                     {
                         Debug.LogError(e + "////" + path);
                     }
                 }
             }
         }
         
         [MenuItem("Window/Delete")]
         public static void Delete()
         {
             foreach (var v in Directory.GetDirectories("Assets/Sprites/Enemy"))
             {
                 if(Directory.GetFiles(v).Length == 0)
                     Directory.Delete(v);
             }
         }*/
}