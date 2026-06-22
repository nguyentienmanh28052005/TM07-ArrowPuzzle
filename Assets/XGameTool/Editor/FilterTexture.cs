using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class FilterTexture : EditorWindow
{
    public List<Texture2D> m_Textures;
    public Dictionary<Texture2D, bool> m_Object = new Dictionary<Texture2D, bool>();
    public string[] m_Textures_Guild;
    private SerializedObject serializedObject;
    private Vector2 scrollPos;

    // Fields to copy between platform blocks
    private static readonly string[] PlatformFields = new string[]
    {
        "maxTextureSize",
        "textureFormat",
        "textureCompression",
        "compressionQuality",
        "crunchedCompression",
        "overridden"
    };

    private void OnEnable()
    {
        m_Textures = new List<Texture2D>();
        titleContent = new GUIContent("Filter Texture");
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
            if (Selection.activeObject as Sprite)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(Selection.activeObject, out var guid, out long file);
                EditorGUILayout.TextField("guild", !string.IsNullOrEmpty(guid) ? $"fileID: {file}, guid: {guid}" : "");
            }
            else
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(Selection.activeObject, out var guid, out long file);
                EditorGUILayout.TextField("guild", !string.IsNullOrEmpty(guid) ? guid : "");
            }

        }
        if (Selection.activeObject != null)
        {
           var p = Selection.activeGameObject;
           if(p != null){
               EditorGUILayout.TextField("path", GetGameObjectPath(p) == null ? "" : GetGameObjectPath(p));
           }
            //
        }

        scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUIStyle.none, GUI.skin.verticalScrollbar);
        GUILayout.BeginVertical();
        GUILayout.Space(10f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Textures"), true);
        serializedObject.ApplyModifiedProperties();
        m_Textures_Guild = new string[m_Textures.Count];
        GUILayout.EndVertical();
        GUILayout.Space(6f);
        GUILayout.EndScrollView();

        if (GUILayout.Button("ClaimSize&DisableMipmap"))
        {
            foreach (var vv in m_Textures)
            {
                if (vv != null)
                {
                    try
                    {
                        var ttip = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(vv));
      
                        if (vv.width == vv.height && vv.width == 1024 || vv.width == 2048 || ttip.maxTextureSize == 2048 || ttip.mipmapEnabled)
                        {
                            var tmp = File.ReadAllText(AssetDatabase.GetAssetPath(vv) + ".meta");


                            if (vv.width == vv.height && vv.width == 1024 || vv.width == 2048)
                            {
                                if (ttip.maxTextureSize == 1024)
                                {
                                    tmp = tmp.Replace("maxTextureSize: 1024", "maxTextureSize: 512");
                                }
                            }

                            if (ttip.mipmapEnabled)
                            {
                                tmp = tmp.Replace("enableMipMap: 1", "enableMipMap: 0");
                            }
                            File.WriteAllText(AssetDatabase.GetAssetPath(vv) + ".meta", tmp);
                        }
                    }
                    catch (Exception e)
                    {
                    }
                
                }

            }
        }

        GUILayout.Space(4f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply Android → iOS"))
        {
            ApplyPlatformSettings("Android", "iPhone");
        }
        if (GUILayout.Button("Apply iOS → Android"))
        {
            ApplyPlatformSettings("iPhone", "Android");
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(4f);
        
        if (GUILayout.Button("Find All"))
        {
            var paths = AssetDatabase.GetAllAssetPaths();

            for (int j = 0; j < paths.Length; j++)
            {
                if (j >= paths.Length) break;
                var s = paths[j];
                var ob = AssetDatabase.LoadAssetAtPath<Texture2D>(s);

                if (ob != null)
                {
                    m_Textures.Add(ob);
                    m_Textures_Guild  = new string[m_Textures.Count];
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

    /// <summary>
    /// Copy platform texture settings from sourcePlatform to targetPlatform via .meta file text manipulation.
    /// </summary>
    private void ApplyPlatformSettings(string sourcePlatform, string targetPlatform)
    {
        int applied = 0;
        int skipped = 0;

        foreach (var tex in m_Textures)
        {
            if (tex == null) continue;

            try
            {
                var assetPath = AssetDatabase.GetAssetPath(tex);
                var metaPath = assetPath + ".meta";
                if (!File.Exists(metaPath)) continue;

                var metaContent = File.ReadAllText(metaPath);

                // Extract source platform block values
                var sourceValues = ExtractPlatformBlock(metaContent, sourcePlatform);
                if (sourceValues == null)
                {
                    Debug.LogWarning($"[FilterTexture] No {sourcePlatform} block found in: {assetPath}");
                    skipped++;
                    continue;
                }

                // Only apply if source has overridden: 1
                if (sourceValues.ContainsKey("overridden") && sourceValues["overridden"] != "1")
                {
                    Debug.Log($"[FilterTexture] {sourcePlatform} not overridden, skip: {assetPath}");
                    skipped++;
                    continue;
                }

                // Check if target block exists
                var targetValues = ExtractPlatformBlock(metaContent, targetPlatform);

                if (targetValues != null)
                {
                    // Update existing target block fields
                    metaContent = UpdatePlatformBlock(metaContent, targetPlatform, sourceValues);
                }
                else
                {
                    // Insert new target block by cloning source block
                    metaContent = InsertPlatformBlock(metaContent, sourcePlatform, targetPlatform);
                }

                File.WriteAllText(metaPath, metaContent);
                // AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                applied++;
            }
            catch (Exception e)
            {
                Debug.LogError($"[FilterTexture] Error processing {tex.name}: {e.Message}");
                skipped++;
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"[FilterTexture] Apply {sourcePlatform} → {targetPlatform} done. Applied: {applied}, Skipped: {skipped}");
    }

    /// <summary>
    /// Extract field values from a platform block in .meta text.
    /// Returns null if platform block not found.
    /// </summary>
    private Dictionary<string, string> ExtractPlatformBlock(string metaContent, string platform)
    {
        // Match the block starting with "buildTarget: {platform}" until the next "- serializedVersion" or end of platformSettings
        var pattern = $@"- serializedVersion: \d+\s*\n\s*buildTarget: {Regex.Escape(platform)}\s*\n([\s\S]*?)(?=\n  - serializedVersion:|\n  sprite|\n  mipmap)";
        var match = Regex.Match(metaContent, pattern);
        if (!match.Success) return null;

        var block = match.Value;
        var values = new Dictionary<string, string>();

        foreach (var field in PlatformFields)
        {
            var fieldMatch = Regex.Match(block, $@"{field}: (.+)");
            if (fieldMatch.Success)
            {
                values[field] = fieldMatch.Groups[1].Value.Trim();
            }
        }

        return values.Count > 0 ? values : null;
    }

    /// <summary>
    /// Update fields in an existing platform block.
    /// </summary>
    private string UpdatePlatformBlock(string metaContent, string targetPlatform, Dictionary<string, string> sourceValues)
    {
        // Find the target block
        var pattern = $@"(- serializedVersion: \d+\s*\n\s*buildTarget: {Regex.Escape(targetPlatform)}\s*\n[\s\S]*?)(?=\n  - serializedVersion:|\n  sprite|\n  mipmap)";
        var match = Regex.Match(metaContent, pattern);
        if (!match.Success) return metaContent;

        var originalBlock = match.Groups[1].Value;
        var updatedBlock = originalBlock;

        foreach (var field in PlatformFields)
        {
            if (!sourceValues.ContainsKey(field)) continue;
            var fieldPattern = $@"({field}: ).+";
            updatedBlock = Regex.Replace(updatedBlock, fieldPattern, $"${{1}}{sourceValues[field]}");
        }

        return metaContent.Replace(originalBlock, updatedBlock);
    }

    /// <summary>
    /// Insert a new platform block by cloning the source block and changing buildTarget.
    /// </summary>
    private string InsertPlatformBlock(string metaContent, string sourcePlatform, string targetPlatform)
    {
        var pattern = $@"(  - serializedVersion: \d+\s*\n\s*buildTarget: {Regex.Escape(sourcePlatform)}\s*\n[\s\S]*?)(?=\n  - serializedVersion:|\n  sprite|\n  mipmap)";
        var match = Regex.Match(metaContent, pattern);
        if (!match.Success) return metaContent;

        var sourceBlock = match.Groups[1].Value;
        var newBlock = sourceBlock.Replace($"buildTarget: {sourcePlatform}", $"buildTarget: {targetPlatform}");

        // Insert after the source block
        var insertPos = match.Index + match.Length;
        return metaContent.Insert(insertPos, "\n" + newBlock);
    }

    private void OnSelectionChange()
    {
        Repaint();
    }
    public static string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
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
                if (string.IsNullOrEmpty(path) || path.Contains("GamePlugin") || path.Contains("Resources") || path.Contains("IconSplash") || path.Contains(".fbx") || path.Contains(".obj")) continue;
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
        if (File.Exists(s) && s.StartsWith("Assets") && (t == "prefab" || t == "unity" || t == "asset" || t == "mat" || t == "anim"))
        {
            var tmp = File.ReadAllLines(s);
            foreach (var v in tmp)
            {
                if (v.Contains(" guid:"))
                {
                    for (int j = 0; j < m_Textures.Count; j++)
                    {
                        if (!m_Object.ContainsKey(m_Textures[j]))
                        {
                            m_Object.Add(m_Textures[j], false);
                        }

                        if (!m_Object[m_Textures[j]])
                        {
                            var guid = m_Textures_Guild[j];
                            if (string.IsNullOrEmpty(guid))
                            {
                                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(m_Textures[j], out guid, out long file);
                                m_Textures_Guild[j] = guid;
                            }

                            if (v.Contains($" guid: {guid}"))
                            {
                                m_Object[m_Textures[j]] = true;
                            }
                        }
                    }
                }
            }
        }
    }

    [MenuItem("Tool/Filter Texture")]
    public static void ShowWindow()
    {
        GetWindow(typeof(FilterTexture));
    }
}