// Self-contained TCP2 Property Drawers
// Replaces dependency on TCP2 plugin's drawer classes
// Used by XGame/TCP2 Hybrid shader

using UnityEditor;
using UnityEngine;

// ============================================================
// TCP2HeaderToggle - Toggle with orange bold label that enables/disables a keyword
// Usage: [TCP2HeaderToggle] or [TCP2HeaderToggle(KEYWORD_NAME)]
// ============================================================
public class TCP2HeaderToggleDrawer : MaterialPropertyDrawer
{
    string keyword;
    bool hasKeyword;

    public TCP2HeaderToggleDrawer() { hasKeyword = false; }
    public TCP2HeaderToggleDrawer(string kw) { keyword = kw; hasKeyword = true; }

    static GUIStyle _orangeBold;
    static GUIStyle OrangeBoldLabel
    {
        get
        {
            if (_orangeBold == null)
            {
                var color = EditorGUIUtility.isProSkin ? new Color32(250, 130, 0, 255) : new Color32(220, 100, 0, 255);
                _orangeBold = new GUIStyle(EditorStyles.label);
                _orangeBold.normal.textColor = color;
                _orangeBold.active.textColor = color;
                _orangeBold.focused.textColor = color;
                _orangeBold.hover.textColor = color;
                _orangeBold.fontStyle = FontStyle.Bold;
            }
            return _orangeBold;
        }
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
    {
        // Draw toggle with orange label
        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = prop.hasMixedValue;

        Rect toggleRect = EditorGUI.PrefixLabel(position, label, OrangeBoldLabel);
        bool newValue = EditorGUI.Toggle(toggleRect, prop.floatValue > 0);

        EditorGUI.showMixedValue = false;

        if (EditorGUI.EndChangeCheck())
        {
            prop.floatValue = newValue ? 1.0f : 0.0f;
            if (hasKeyword)
            {
                foreach (var target in editor.targets)
                {
                    var mat = target as Material;
                    if (newValue)
                        mat.EnableKeyword(keyword);
                    else
                        mat.DisableKeyword(keyword);
                }
            }
        }
    }

    public override void Apply(MaterialProperty prop)
    {
        base.Apply(prop);
        if (hasKeyword && !prop.hasMixedValue)
        {
            foreach (Material mat in prop.targets)
            {
                if (prop.floatValue > 0)
                    mat.EnableKeyword(keyword);
                else
                    mat.DisableKeyword(keyword);
            }
        }
    }
}

// ============================================================
// TCP2MaterialKeywordEnumNoPrefix - Enum popup that sets shader keywords
// The keywords are used directly (no prefix added)
// Usage: [TCP2MaterialKeywordEnumNoPrefix(Label1,Keyword1,Label2,Keyword2,...)]
// Use "_" as keyword for "no keyword" option
// ============================================================
public class TCP2MaterialKeywordEnumNoPrefixDrawer : MaterialPropertyDrawer
{
    string[] labels;
    string[] keywords;

    public TCP2MaterialKeywordEnumNoPrefixDrawer(string l1, string k1) : this(new[] { l1, k1 }) { }
    public TCP2MaterialKeywordEnumNoPrefixDrawer(string l1, string k1, string l2, string k2) : this(new[] { l1, k1, l2, k2 }) { }
    public TCP2MaterialKeywordEnumNoPrefixDrawer(string l1, string k1, string l2, string k2, string l3, string k3) : this(new[] { l1, k1, l2, k2, l3, k3 }) { }
    public TCP2MaterialKeywordEnumNoPrefixDrawer(string l1, string k1, string l2, string k2, string l3, string k3, string l4, string k4) : this(new[] { l1, k1, l2, k2, l3, k3, l4, k4 }) { }
    public TCP2MaterialKeywordEnumNoPrefixDrawer(string l1, string k1, string l2, string k2, string l3, string k3, string l4, string k4, string l5, string k5) : this(new[] { l1, k1, l2, k2, l3, k3, l4, k4, l5, k5 }) { }
    public TCP2MaterialKeywordEnumNoPrefixDrawer(string l1, string k1, string l2, string k2, string l3, string k3, string l4, string k4, string l5, string k5, string l6, string k6) : this(new[] { l1, k1, l2, k2, l3, k3, l4, k4, l5, k5, l6, k6 }) { }
    public TCP2MaterialKeywordEnumNoPrefixDrawer(string l1, string k1, string l2, string k2, string l3, string k3, string l4, string k4, string l5, string k5, string l6, string k6, string l7, string k7) : this(new[] { l1, k1, l2, k2, l3, k3, l4, k4, l5, k5, l6, k6, l7, k7 }) { }
    public TCP2MaterialKeywordEnumNoPrefixDrawer(string l1, string k1, string l2, string k2, string l3, string k3, string l4, string k4, string l5, string k5, string l6, string k6, string l7, string k7, string l8, string k8) : this(new[] { l1, k1, l2, k2, l3, k3, l4, k4, l5, k5, l6, k6, l7, k7, l8, k8 }) { }

    TCP2MaterialKeywordEnumNoPrefixDrawer(string[] args)
    {
        int count = args.Length / 2;
        labels = new string[count];
        keywords = new string[count];
        for (int i = 0; i < count; i++)
        {
            labels[i] = args[i * 2];
            keywords[i] = args[i * 2 + 1];
        }
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = prop.hasMixedValue;

        int index = (int)prop.floatValue;
        index = Mathf.Clamp(index, 0, labels.Length - 1);
        int newIndex = EditorGUI.Popup(position, label.text, index, labels);

        EditorGUI.showMixedValue = false;

        if (EditorGUI.EndChangeCheck())
        {
            prop.floatValue = newIndex;
            SetKeyword(editor, newIndex);
        }
    }

    void SetKeyword(MaterialEditor editor, int index)
    {
        foreach (var target in editor.targets)
        {
            var mat = target as Material;
            for (int i = 0; i < keywords.Length; i++)
            {
                string kw = keywords[i];
                if (kw == "_") continue;
                if (i == index)
                    mat.EnableKeyword(kw);
                else
                    mat.DisableKeyword(kw);
            }
        }
    }

    public override void Apply(MaterialProperty prop)
    {
        base.Apply(prop);
        if (!prop.hasMixedValue)
        {
            int index = (int)prop.floatValue;
            foreach (Material mat in prop.targets)
            {
                for (int i = 0; i < keywords.Length; i++)
                {
                    string kw = keywords[i];
                    if (kw == "_") continue;
                    if (i == index)
                        mat.EnableKeyword(kw);
                    else
                        mat.DisableKeyword(kw);
                }
            }
        }
    }
}

// ============================================================
// TCP2ColorNoAlpha - Color field without alpha channel
// ============================================================
public class TCP2ColorNoAlphaDrawer : MaterialPropertyDrawer
{
    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = prop.hasMixedValue;

        bool hdr = (prop.flags & MaterialProperty.PropFlags.HDR) != 0;
#if UNITY_2018_1_OR_NEWER
        Color newColor = EditorGUI.ColorField(position, label, prop.colorValue, true, false, hdr);
#else
        Color newColor = EditorGUI.ColorField(position, label, prop.colorValue, true, false, hdr, new ColorPickerHDRConfig(0, 99, 0.01f, 3));
#endif

        EditorGUI.showMixedValue = false;

        if (EditorGUI.EndChangeCheck())
        {
            prop.colorValue = newColor;
        }
    }
}

// ============================================================
// TCP2ToggleNoKeyword - Standard toggle, no keyword toggling
// ============================================================
public class TCP2ToggleNoKeywordDrawer : MaterialPropertyDrawer
{
    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = prop.hasMixedValue;

        bool newValue = EditorGUI.Toggle(position, label, prop.floatValue > 0);

        EditorGUI.showMixedValue = false;

        if (EditorGUI.EndChangeCheck())
        {
            prop.floatValue = newValue ? 1.0f : 0.0f;
        }
    }
}

// ============================================================
// TCP2Gradient - Texture field for gradient ramps
// ============================================================
public class TCP2GradientDrawer : MaterialPropertyDrawer
{
    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return EditorGUIUtility.singleLineHeight + 2;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
    {
        editor.TexturePropertyMiniThumbnail(position, prop, label.text, label.tooltip);
    }
}

// ============================================================
// TCP2Enum - Simple enum popup (no keyword toggling)
// Usage: [TCP2Enum(Label1,Value1,Label2,Value2,...)]
// ============================================================
public class TCP2EnumDrawer : MaterialPropertyDrawer
{
    string[] labels;
    float[] values;

    public TCP2EnumDrawer(string l1, float v1) : this(new object[] { l1, v1 }) { }
    public TCP2EnumDrawer(string l1, float v1, string l2, float v2) : this(new object[] { l1, v1, l2, v2 }) { }
    public TCP2EnumDrawer(string l1, float v1, string l2, float v2, string l3, float v3) : this(new object[] { l1, v1, l2, v2, l3, v3 }) { }
    public TCP2EnumDrawer(string l1, float v1, string l2, float v2, string l3, float v3, string l4, float v4) : this(new object[] { l1, v1, l2, v2, l3, v3, l4, v4 }) { }
    public TCP2EnumDrawer(string l1, float v1, string l2, float v2, string l3, float v3, string l4, float v4, string l5, float v5) : this(new object[] { l1, v1, l2, v2, l3, v3, l4, v4, l5, v5 }) { }
    public TCP2EnumDrawer(string l1, float v1, string l2, float v2, string l3, float v3, string l4, float v4, string l5, float v5, string l6, float v6) : this(new object[] { l1, v1, l2, v2, l3, v3, l4, v4, l5, v5, l6, v6 }) { }
    public TCP2EnumDrawer(string l1, float v1, string l2, float v2, string l3, float v3, string l4, float v4, string l5, float v5, string l6, float v6, string l7, float v7) : this(new object[] { l1, v1, l2, v2, l3, v3, l4, v4, l5, v5, l6, v6, l7, v7 }) { }
    public TCP2EnumDrawer(string l1, float v1, string l2, float v2, string l3, float v3, string l4, float v4, string l5, float v5, string l6, float v6, string l7, float v7, string l8, float v8) : this(new object[] { l1, v1, l2, v2, l3, v3, l4, v4, l5, v5, l6, v6, l7, v7, l8, v8 }) { }
    public TCP2EnumDrawer(string l1, float v1, string l2, float v2, string l3, float v3, string l4, float v4, string l5, float v5, string l6, float v6, string l7, float v7, string l8, float v8, string l9, float v9) : this(new object[] { l1, v1, l2, v2, l3, v3, l4, v4, l5, v5, l6, v6, l7, v7, l8, v8, l9, v9 }) { }
    public TCP2EnumDrawer(string l1, float v1, string l2, float v2, string l3, float v3, string l4, float v4, string l5, float v5, string l6, float v6, string l7, float v7, string l8, float v8, string l9, float v9, string l10, float v10) : this(new object[] { l1, v1, l2, v2, l3, v3, l4, v4, l5, v5, l6, v6, l7, v7, l8, v8, l9, v9, l10, v10 }) { }
    public TCP2EnumDrawer(string l1, float v1, string l2, float v2, string l3, float v3, string l4, float v4, string l5, float v5, string l6, float v6, string l7, float v7, string l8, float v8, string l9, float v9, string l10, float v10, string l11, float v11) : this(new object[] { l1, v1, l2, v2, l3, v3, l4, v4, l5, v5, l6, v6, l7, v7, l8, v8, l9, v9, l10, v10, l11, v11 }) { }

    TCP2EnumDrawer(object[] args)
    {
        int count = args.Length / 2;
        labels = new string[count];
        values = new float[count];
        for (int i = 0; i < count; i++)
        {
            labels[i] = (string)args[i * 2];
            values[i] = (float)args[i * 2 + 1];
        }
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = prop.hasMixedValue;

        // Find current index
        int currentIndex = 0;
        float currentValue = prop.floatValue;
        for (int i = 0; i < values.Length; i++)
        {
            if (Mathf.Approximately(values[i], currentValue))
            {
                currentIndex = i;
                break;
            }
        }

        int newIndex = EditorGUI.Popup(position, label.text, currentIndex, labels);

        EditorGUI.showMixedValue = false;

        if (EditorGUI.EndChangeCheck())
        {
            prop.floatValue = values[newIndex];
        }
    }
}
