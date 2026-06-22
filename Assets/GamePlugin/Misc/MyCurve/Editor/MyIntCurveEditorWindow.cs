#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MyIntCurveEditorWindow : EditorWindow
{
    [System.Serializable]
    public class CurveKey
    {
        public int time;
        public int value;
    }

    [System.Serializable]
    public class CurveData
    {
        public string name;
        public List<CurveKey> keys = new List<CurveKey>();
    }

    [System.Serializable]
    public class CurveLibrary
    {
        public List<CurveData> curves = new List<CurveData>();
    }

    
    private SerializedObject serializedObject;
    private SerializedProperty curveProperty;

    private const int X_MIN = 0;
    private const int X_MAX = 20;
    private const int Y_MIN = 0;
    private const int Y_MAX = 6;
    private const float MARGIN = 40f;
    private const float POINT_SIZE = 10f;
    
    private const float LIB_HEIGHT = 85f;
    private Vector2 scrollPos; // nhớ khai báo ở class Editor

    private readonly Color32[] difficultyColors =
    {
        new(0, 255, 128, 255), // Rất dễ – Xanh ngọc sáng
        new(0, 128, 255, 255), // Dễ – Xanh dương
        new(255, 255, 0, 255), // Trung bình dễ – Vàng sáng
        new(255, 128, 0, 255), // Trung bình – Cam
        new(255, 0, 0, 255), // Trung bình khó – Đỏ
        new(191, 64, 255, 255), // Khó – Tím sáng (lavender violet)
        new(255, 64, 64, 255) // Rất khó – Đỏ sáng (light red)
    };
    
    private string LibraryPath => Path.Combine($"Assets/GamePlugin/Misc/MyCurve/Editor", "CurveLibrary.json");

    public static void ShowWindow(SerializedObject targetObject, string propertyPath)
    {
        var window = GetWindow<MyIntCurveEditorWindow>("My Int Curve");
        window.serializedObject = targetObject;
        window.curveProperty = targetObject.FindProperty(propertyPath);
        window.Show();
    }

    private void OnGUI()
    {
        if (serializedObject == null || curveProperty == null)
        {
            EditorGUILayout.HelpBox("No curve selected", MessageType.Info);
            return;
        }

        serializedObject.Update();
        
        // Lấy MyIntCurve thực tế ra (bên trong object gốc)
        var curveField = curveProperty.FindPropertyRelative("values");
        if (curveField == null)
        {
            EditorGUILayout.HelpBox("Invalid curve data", MessageType.Error);
            return;
        }
        
        GUILayout.BeginVertical();
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        CurveLibrary lib = LoadLibrary();
        if (GUILayout.Button("Delete all", GUILayout.Height(20)))
        {
            lib.curves.Clear();
            SaveLibrary(lib);
        }
        if (GUILayout.Button("Reset", GUILayout.Height(20)))
        {
            ResetCurve(curveField);
        }
        if (GUILayout.Button("Save", GUILayout.Height(20)))
        {
            SaveCurveDialog.ShowWindow(position.position, nm => SaveCurveToLibrary(curveField, nm));
        }
        GUILayout.Space(15);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        Rect rect = new Rect(MARGIN + MARGIN, MARGIN, position.width - MARGIN, position.height - MARGIN * 2);
        rect.width -= MARGIN * 1.5f;
        rect.height -= LIB_HEIGHT;
        DrawBackground(rect);
        DrawGrid(rect);

        // Vẽ keyframes trực tiếp từ SerializedProperty
        Handles.color = Color.green;
        for (int i = 0; i < curveField.arraySize - 1; i++)
        {
            int t1 = i;
            int t2 = i + 1;
            
            var v1 = curveField.GetArrayElementAtIndex(t1).intValue;
            var v2 = curveField.GetArrayElementAtIndex(t2).intValue;

            Vector2 pa = ValueToScreen(rect, t1, v1);
            Vector2 pb = ValueToScreen(rect, t2, v2);
            Handles.DrawLine(pa, pb);
        }

        for (int i = 0; i < curveField.arraySize; i++)
        {
            int t = i;
            var v = curveField.GetArrayElementAtIndex(t).intValue;
            Handles.color = difficultyColors[v];
            Vector3 pos = ValueToScreen(rect, t, v);
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.Slider2D(
                pos,
                Vector3.forward,
                Vector3.right,
                Vector3.up,
                POINT_SIZE,
                Handles.RectangleHandleCap,
                Vector2.zero
            );

            if (EditorGUI.EndChangeCheck())
            {
                ScreenToValue(rect, newPos, out int newT, out int newV);
                curveField.GetArrayElementAtIndex(t).intValue = newV;
            }

            if (t < X_MAX)
            {
                string label = $"{((LevelDifficulty)v).ToString()}";
                Handles.Label(pos + new Vector3(15, -10, 0), label, EditorStyles.miniLabel);
            }
        }

        serializedObject.ApplyModifiedProperties();

        // --- Curve Presets Toolbar (Bottom Left) ---

        Rect bottomLeftRect = new Rect(10, position.height - LIB_HEIGHT - 10, position.width - 20, LIB_HEIGHT);
        GUILayout.BeginArea(bottomLeftRect, EditorStyles.helpBox);
        GUILayout.Label("Presets:", EditorStyles.boldLabel);

        Rect scrollRect = GUILayoutUtility.GetRect(0, LIB_HEIGHT, GUILayout.ExpandWidth(true));

        Rect buttonRect = GUILayoutUtility.GetRect(100, 50, GUILayout.Width(80), GUILayout.Height(50));
        float contentWidth = lib.curves.Count * buttonRect.width + (lib.curves.Count - 1) * 10; // ví dụ mỗi preset ~80 + space
        float viewWidth = scrollRect.width;
        GUI.BeginGroup(scrollRect);

        if (Event.current.type == EventType.MouseDrag && scrollRect.Contains(Event.current.mousePosition))
        {
            scrollPos.x -= Event.current.delta.x;
            Event.current.Use();
        }
        scrollPos.x = Mathf.Clamp(scrollPos.x, 0, Mathf.Max(0, contentWidth - viewWidth));
        
        GUILayout.BeginArea(new Rect(-scrollPos.x, 0, contentWidth, 60));

        GUILayout.Space(5);
        if (lib.curves.Count > 0)
        {
            // scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Height(60));
            GUILayout.BeginHorizontal();

            foreach (var preset in lib.curves)
            {
                DrawPresetButton(preset, curveField, lib);
                GUILayout.Space(10);
            }

            GUILayout.EndHorizontal();
            // GUILayout.EndScrollView(); // kết thúc scroll
        }
        
        GUILayout.EndArea();
        GUI.EndGroup();
        
        GUILayout.EndArea();

    }

    private void DrawPresetButton(CurveData preset, SerializedProperty curveField, CurveLibrary lib)
    {
        Rect buttonRect = GUILayoutUtility.GetRect(100, 50, GUILayout.Width(80), GUILayout.Height(50));
        
        // if (GUI.Button(buttonRect, GUIContent.none))
        // {
        //     ApplyPreset(curveField, preset);
        // }

        // Vẽ background
        EditorGUI.DrawRect(buttonRect, new Color(0.15f, 0.15f, 0.15f));

        // Nếu preset có key
        if (preset.keys != null && preset.keys.Count > 1)
        {
            Handles.BeginGUI();
            Handles.color = Color.white;

            Vector3 prev = ValueToPreview(buttonRect, preset.keys[0].time, preset.keys[0].value);
            for (int i = 1; i < preset.keys.Count; i++)
            {
                Vector3 curr = ValueToPreview(buttonRect, preset.keys[i].time, preset.keys[i].value);
                Handles.DrawLine(prev, curr);
                prev = curr;
            }

            Handles.EndGUI();
        }
        Rect labelRect = new Rect(buttonRect.x, buttonRect.yMax - 50, buttonRect.width, 15);
        GUI.Label(labelRect, preset.name, EditorStyles.centeredGreyMiniLabel);
        
        Event e = Event.current;
        if (e.type == EventType.MouseDown && buttonRect.Contains(e.mousePosition))
        {
            if (e.button == 0) // Left click -> load preset
            {
                ApplyPreset(curveField, preset);
                e.Use();
            }
            else if (e.button == 1) // Right click -> context menu
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Delete"), false, () =>
                {
                    if (EditorUtility.DisplayDialog("Delete Preset", $"Xóa preset \"{preset.name}\" ?", "Yes", "No"))
                    {
                        lib.curves.Remove(preset);
                        SaveLibrary(lib);
                        Debug.Log($"delete curve '{preset.name}' to library");
                    }
                });
                // sau này có thể thêm: menu.AddItem(new GUIContent("Rename"), false, () => { ... });
                menu.ShowAsContext();
                e.Use();
            }
        }
    }

    /// <summary>
    /// Convert time/value sang rect mini preview
    /// </summary>
    private Vector2 ValueToPreview(Rect rect, int time, int value)
    {
        float nx = Mathf.InverseLerp(X_MIN, X_MAX, time);   // X_MIN, X_MAX
        float ny = Mathf.InverseLerp(Y_MIN, Y_MAX, value);   // Y_MIN, Y_MAX

        float x = rect.xMin + nx * rect.width;
        float y = rect.yMax - ny * rect.height;
        return new Vector2(x, y);
    }

    
    private void ApplyPreset(SerializedProperty curveField, CurveData preset)
    {
        curveField.ClearArray();
        for (int i = 0; i < preset.keys.Count; i++)
        {
            curveField.InsertArrayElementAtIndex(i);
            var elem = curveField.GetArrayElementAtIndex(i);
            elem.intValue = preset.keys[i].value;
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void SaveCurveToLibrary(SerializedProperty curveField, string curveName)
    {
        // Load library cũ
        CurveLibrary lib = LoadLibrary();

        // Tạo curve mới
        CurveData data = new CurveData();
        data.name = curveName;
        for (int i = 0; i < curveField.arraySize; i++)
        {
            var k = curveField.GetArrayElementAtIndex(i);
            data.keys.Add(new CurveKey
            {
                time = i,
                value = k.intValue
            });
        }

        // Thêm vào thư viện
        lib.curves.Add(data);

        // Ghi ra file
        SaveLibrary(lib);
        Debug.Log($"Saved curve '{curveName}' to library");
    }

    private void SaveLibrary(CurveLibrary lib)
    {
        string json = JsonUtility.ToJson(lib, true);
        File.WriteAllText(LibraryPath, json);
        AssetDatabase.Refresh();
    }
    
    private CurveLibrary LoadLibrary()
    {
        if (File.Exists(LibraryPath))
        {
            string json = File.ReadAllText(LibraryPath);
            return JsonUtility.FromJson<CurveLibrary>(json);
        }
        return new CurveLibrary();
    }

    private void ResetCurve(SerializedProperty curveField)
    {
        curveField.ClearArray();
        for (int i = 0; i < 21; i++)
        {
            int value = 0;
            curveField.InsertArrayElementAtIndex(i);
            var elem = curveField.GetArrayElementAtIndex(i);
            elem.intValue = value;
        }
        serializedObject.ApplyModifiedProperties();
    }
    
    private void InsertKey(SerializedProperty curveField, int index, int time, int value)
    {
        curveField.InsertArrayElementAtIndex(index);
        var elem = curveField.GetArrayElementAtIndex(index);
        elem.intValue = value;
    }
    
    private void DrawBackground(Rect rect)
    {
        EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f));
    }

    private void DrawGrid(Rect rect)
    {
        Handles.color = new Color(1f, 1f, 1f, 0.1f);

        // Vẽ grid dọc (time 1–20)
        for (int x = X_MIN; x <= X_MAX; x++)
        {
            Vector2 p1 = ValueToScreen(rect, x, Y_MIN);
            Vector2 p2 = ValueToScreen(rect, x, Y_MAX);
            Handles.DrawLine(p1, p2);

            Vector2 labelPos = new Vector2(p1.x, rect.yMax + 5);
            GUI.Label(new Rect(labelPos.x - (x == X_MAX ? 20 : 10), labelPos.y, 35, 20), $"{x * 5}%".ToString());
        }

        // Vẽ grid ngang (value 0–7)
        for (int y = Y_MIN; y <= Y_MAX; y++)
        {
            Vector2 p1 = ValueToScreen(rect, X_MIN, y);
            Vector2 p2 = ValueToScreen(rect, X_MAX, y);
            Handles.DrawLine(p1, p2);

            var color = GUI.color;
            GUI.color = difficultyColors[y];
            Vector2 labelPos = new Vector2(rect.xMin - 65, p1.y - 8);
            GUI.Label(new Rect(labelPos.x, labelPos.y, 65, 20), ((LevelDifficulty)y).ToString());
            GUI.color = color;
        }
    }

    private Vector2 ValueToScreen(Rect rect, int time, int value)
    {
        float nx = Mathf.InverseLerp(X_MIN, X_MAX, time);
        float ny = Mathf.InverseLerp(Y_MIN, Y_MAX, value);

        float x = rect.xMin + nx * (rect.width);
        float y = rect.yMax - ny * rect.height;
        return new Vector2(x, y);
    }

    private void ScreenToValue(Rect rect, Vector2 pos, out int time, out int value)
    {
        float nx = Mathf.InverseLerp(rect.xMin, rect.xMax, pos.x);
        float ny = Mathf.InverseLerp(rect.yMax, rect.yMin, pos.y);

        time = Mathf.RoundToInt(Mathf.Lerp(X_MIN, X_MAX, nx));
        value = Mathf.RoundToInt(Mathf.Lerp(Y_MIN, Y_MAX, ny));
    }
}
#endif
