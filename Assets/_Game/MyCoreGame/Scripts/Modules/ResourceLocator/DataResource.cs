using System;
using Newtonsoft.Json;
using UnityEngine;
using mygame.sdk;
using System.ComponentModel;

#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif
[System.Serializable]
public class DataResource
{
    public DataResource() { }
    public DataResource(RES_type type, int amount)
     {
         this.resType = type;
         this.amount = amount;
     }
    public DataResource(DataResource dataResource)
    {
        this.resType = dataResource.resType;
        this.amount = dataResource.amount;
        this._icon = dataResource._icon;
        this._bg = dataResource._bg;
        this._idIcon = dataResource._idIcon;
    }
    [DefaultValue("")] public string description;
    //public DataTypeResource dataTypeResource;
    public RES_type resType;
    public int amount;
    [JsonIgnore] [SerializeField] public Sprite _icon;
    [JsonIgnore] [SerializeField] public Sprite _bg;
    [JsonProperty][SerializeField] protected short _idIcon;
    [JsonProperty][SerializeField] protected short _idBg;
    [JsonIgnore]
    public Sprite icon
    {
        get
        {
            if (_icon == null)
            {
                if (_idIcon <= 0)
                {
                    return SpriteResourceManager.Instance.GetIcon(resType);
                }
                else
                {
                    return SpriteResourceManager.Instance.GetIcon(_idIcon);
                }

            }
            return _icon;
        }
        set
        {
            _icon = value;
        }
    }
    [JsonIgnore]
    public Sprite bg
    {
        get
        {
            if (_bg == null)
            {
                if (_idBg <= 0)
                {
                    return SpriteResourceManager.Instance.GetBg(resType);
                }
                else
                {
                    return SpriteResourceManager.Instance.GetBg(_idBg);
                }
            }
            return _bg;
        }
        set
        {
            _bg = value;
        }
    }
    public short GetIdIcon()
    {
        return _idIcon;
    }
    public short GetIdBg()
    {
        return _idBg;
    }
    public virtual int GetAmount()
    {
        return amount;
    }
    public void CopyData(DataResource from)
    {
        description = from.description;
        _icon = from._icon;
        _bg = from._bg;
        //dataTypeResource = new DataTypeResource(from.dataTypeResource.type, from.dataTypeResource.id);
        resType = from.resType;
        amount = from.amount;
    }
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(DataResource))]
public class DataResourceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var property_resType = property.FindPropertyRelative("resType");
        var property_amount = property.FindPropertyRelative("amount");
        var property_idIcon = property.FindPropertyRelative("_idIcon");
        var property_description = property.FindPropertyRelative("description");
        var property_icon = property.FindPropertyRelative("_icon");

        var type = (RES_type)property_resType.intValue;
        int id_icon = property_idIcon.intValue;
        Sprite spr_icon = property_icon.objectReferenceValue as Sprite;
        if (spr_icon == null)
            spr_icon = id_icon > 0
                ? SpriteResourceManager.Instance.GetIcon((short)id_icon)
                : SpriteResourceManager.Instance.GetIcon(type);

        // Set fixed icon frame (vuông)
        float maxIconH = 32f; // Đổi lên 36f, 40f nếu muốn icon to hơn!
        float maxIconW = 32f;
        float nativeW = 32f, nativeH = 32f;
        if (spr_icon && spr_icon.texture)
        {
            nativeW = spr_icon.textureRect.width;
            nativeH = spr_icon.textureRect.height;
        }

        // Scale sao cho icon không vượt quá frame vuông
        float scale = Mathf.Min(maxIconW / nativeW, maxIconH / nativeH);
        float drawW = nativeW * scale;
        float drawH = nativeH * scale;
        // Căn giữa icon trong khung
        float yOffset = position.y + (maxIconH - drawH) / 2f;
        float xOffset = (maxIconW - drawW) / 2f;

        float minRowHeight = maxIconH;

        float foldoutWidth = 16f;
        float spacing = 5f;
        float x = position.x;

        float iconX = x + foldoutWidth + spacing;
        float enumWidth = Mathf.Min(98f, position.width * 0.33f);
        float amountWidth = 38f; // amount nhỏ gọn

        Rect foldoutRect = new Rect(x, position.y, foldoutWidth, minRowHeight);
        Rect iconRect = new Rect(iconX + xOffset, yOffset, drawW, drawH);
        Rect enumRect = new Rect(iconRect.xMax + spacing,
            position.y + (minRowHeight - EditorGUIUtility.singleLineHeight) / 2f, enumWidth,
            EditorGUIUtility.singleLineHeight);
        Rect amountRect = new Rect(enumRect.xMax + spacing,
            position.y + (minRowHeight - EditorGUIUtility.singleLineHeight) / 2f, amountWidth,
            EditorGUIUtility.singleLineHeight);

        // Foldout
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none, false);

        // Vẽ icon luôn nằm giữa khung vuông, không bao giờ thò ra ngoài
        if (spr_icon && spr_icon.texture)
        {
            Rect texRect = spr_icon.textureRect;
            Rect atlasRect = new Rect(
                texRect.x / spr_icon.texture.width,
                texRect.y / spr_icon.texture.height,
                texRect.width / spr_icon.texture.width,
                texRect.height / spr_icon.texture.height);
            GUI.DrawTextureWithTexCoords(iconRect, spr_icon.texture, atlasRect, true);
        }
        else
        {
            EditorGUI.HelpBox(iconRect, "", MessageType.None);
        }

        // Enum popup
        EditorGUI.BeginProperty(enumRect, label, property_resType);
        property_resType.intValue = (int)(RES_type)EditorGUI.EnumPopup(enumRect, (RES_type)property_resType.intValue);
        EditorGUI.EndProperty();

        // Amount nhỏ gọn
        EditorGUI.BeginProperty(amountRect, label, property_amount);
        property_amount.intValue = EditorGUI.IntField(amountRect, property_amount.intValue);
        EditorGUI.EndProperty();

        // ----- Foldout mở rộng -----
        if (property.isExpanded)
        {
            float expandY = position.y + minRowHeight + 4;
            float labelWidth = 80f;
            float fieldWidth = position.width - labelWidth - 32f;

            // Description
            Rect descLabelRect = new Rect(x + foldoutWidth + spacing, expandY, labelWidth,
                EditorGUIUtility.singleLineHeight);
            Rect descFieldRect =
                new Rect(descLabelRect.xMax + 2, expandY, fieldWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(descLabelRect, "Description");
            EditorGUI.BeginProperty(descFieldRect, label, property_description);
            property_description.stringValue = EditorGUI.TextField(descFieldRect, property_description.stringValue);
            EditorGUI.EndProperty();

            expandY += EditorGUIUtility.singleLineHeight + 2;

            // IdIcon
            Rect idIconLabelRect = new Rect(x + foldoutWidth + spacing, expandY, labelWidth,
                EditorGUIUtility.singleLineHeight);
            Rect idIconFieldRect = new Rect(idIconLabelRect.xMax + 2, expandY, 50f, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(idIconLabelRect, "Id Icon");
            EditorGUI.BeginProperty(idIconFieldRect, label, property_idIcon);
            property_idIcon.intValue = EditorGUI.IntField(idIconFieldRect, property_idIcon.intValue);
            EditorGUI.EndProperty();

            expandY += EditorGUIUtility.singleLineHeight + 2;

            // Kéo thả sprite icon (_icon)
            Rect iconObjLabelRect = new Rect(x + foldoutWidth + spacing, expandY, labelWidth,
                EditorGUIUtility.singleLineHeight);
            Rect iconObjFieldRect =
                new Rect(iconObjLabelRect.xMax + 2, expandY, 120f, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(iconObjLabelRect, "Custom Icon");
            EditorGUI.PropertyField(iconObjFieldRect, property_icon, GUIContent.none);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float maxIconH = 32f; // Đồng bộ với trên!
        float baseHeight = maxIconH;
        if (property.isExpanded)
        {
            baseHeight += (EditorGUIUtility.singleLineHeight + 2) * 3 + 4;
        }

        return baseHeight;
    }
}
#endif

