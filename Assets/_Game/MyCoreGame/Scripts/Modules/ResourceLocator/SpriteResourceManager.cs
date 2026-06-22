using mygame.sdk;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(fileName = "Sprite_Resource_SO", menuName = "SO/Sprite Resource", order = 1)]
public class SpriteResourceManager : ScriptableObject
{
    private static SpriteResourceManager instance;
    public static SpriteResourceManager Instance
    {
        get
        {
#if UNITY_EDITOR
            if (instance == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:SpriteResourceManager");
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var so = AssetDatabase.LoadAssetAtPath<SpriteResourceManager>(path);
                    if (so != null)
                    {
                        instance = so;
                        break;
                    }
                }
            }

            if (instance != null)
            {
                instance.Initialize();
            }

#else
            if (instance == null)
            {
                instance = GameManager.Instance.SpriteResourceSO;
                instance.Initialize();
            }
#endif

            return instance;
        }
    }
    [SerializeField] private DB_SpriteResource[] matchTypeId;
    [SerializeField] private Sprite[] icon;
    [SerializeField] private Sprite[] bg;
    private Dictionary<RES_type, DB_SpriteResource> dicSpriteResource = new Dictionary<RES_type, DB_SpriteResource>();
    public DB_SpriteResource[] MatchTypeId => matchTypeId;
    public void Initialize()
    {
        dicSpriteResource.Clear();
        for (int i = 0; i < matchTypeId.Length; i++)
        {
            if (!dicSpriteResource.ContainsKey(matchTypeId[i].resType))
            {
                dicSpriteResource.Add(matchTypeId[i].resType, matchTypeId[i]);
            }
            else
            {
                Debug.LogError($"Check Error : Has key {matchTypeId[i].resType} in DB_SpriteResourceSO index {i}");
            }
        }
    }
    public string GetNameResource(RES_type type)
    {
        if (dicSpriteResource.TryGetValue(type, out DB_SpriteResource d))
        {
            return d.Name;
        }
        return null;
    }
    public void GetSprite(RES_type type, out Sprite icon, out Sprite bg)
    {
        if (dicSpriteResource.ContainsKey(type))
        {
            var d = dicSpriteResource[type];
            GetSprite(d.idIcon, d.idBg, out icon, out bg);
        }
        else
        {
            icon = null;
            bg = null;
        }
    }
    public void GetSprite(short idIcon, short idBg, out Sprite icon, out Sprite bg)
    {
        icon = getIcon(idIcon);
        bg = getIcon(idBg);
    }
    public Sprite GetIcon(RES_type type)
    {

        {
            //var k = type.ToKey();
            if (dicSpriteResource.ContainsKey(type)) { return getIcon(dicSpriteResource[type].idIcon); }
            return null;
        }

    }
    public Sprite GetIcon(short id)
    {
        return getIcon(id);
    }
    public Sprite GetBg(RES_type type)
    {
        if (dicSpriteResource.ContainsKey(type))
        { return getBg(dicSpriteResource[type].idBg); }
        return null;
    }
    public Sprite GetBg(short id)
    {
        return getBg(id);
    }
    private Sprite getIcon(short idx) { return icon[Mathf.Clamp(idx, 0, icon.Length - 1)]; }
    private Sprite getBg(short idx) { return bg[Mathf.Clamp(idx, 0, bg.Length - 1)]; }
}
[System.Serializable]
public struct DB_SpriteResource
{
    [SerializeField] private string name;
    public RES_type resType;
    public short idIcon;
    public short idBg;
    public string Name => name;
}
