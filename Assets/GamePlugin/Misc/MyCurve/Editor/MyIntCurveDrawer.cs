using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MyIntCurve))]
public class MyIntCurveDrawer : PropertyDrawer
{
    private const float HEIGHT = 20f;
    private const int TIME_MIN = 0;
    private const int TIME_MAX = 20;
    private const int VALUE_MIN = 0;
    private const int VALUE_MAX = 6;

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
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty keysProp = property.FindPropertyRelative("values");

        position = EditorGUI.PrefixLabel(position, label);

        Rect rect = new Rect(position.x, position.y, position.width, HEIGHT);

        // Draw background + border
        var color = Handles.color;
        EditorGUI.DrawRect(rect, new Color(0.16f, 0.16f, 0.16f));
        Handles.color = new Color(0f, 0f, 0f, 1f);
        Handles.DrawAAPolyLine(2f, new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMax, rect.yMin));
        // Handles.DrawAAPolyLine(2.5f, new Vector3(rect.xMin, rect.yMax), new Vector3(rect.xMax, rect.yMax));
        Handles.DrawAAPolyLine(2f, new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMin, rect.yMax));
        Handles.DrawAAPolyLine(2f, new Vector3(rect.xMax, rect.yMin), new Vector3(rect.xMax, rect.yMax));
        Handles.color = color;
        if (keysProp == null || keysProp.arraySize < 2)
        {
            EditorGUI.HelpBox(rect, "Add at least 2 keyframes", MessageType.Info);
            
            // Invisible button overlay to open full editor
            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                MyIntCurveEditorWindow.ShowWindow(property.serializedObject, property.propertyPath);
            }
            return;
        }

        // Convert keyframes
        Vector2[] points = new Vector2[keysProp.arraySize];
        int[] values = new int[keysProp.arraySize];
        for (int i = 0; i < keysProp.arraySize; i++)
        {
            var value = keysProp.GetArrayElementAtIndex(i).intValue;
            points[i] = ValueToRect(rect, i, value);
            values[i] = value;
        }

        var boxSize = 2.5f;

        for (int i = 0; i <= VALUE_MAX; i++)
        {
            var p0 = ValueToRect(rect, 0, i);
            Handles.color = difficultyColors[i];
            Rect boxRect = new Rect(p0.x - boxSize / 2f, p0.y - boxSize / 2f, boxSize, boxSize);
            EditorGUI.DrawRect(boxRect, difficultyColors[i]);
        }

        boxSize = 4f;
        Handles.color = Color.green;
        // Draw curve (Bezier between points)
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector2 p0 = points[i];
            Vector2 p1 = points[i + 1];

            float dx = (p1.x - p0.x) * 0.3f;
            Vector2 tangent0 = p0 + Vector2.right * dx;
            Vector2 tangent1 = p1 - Vector2.right * dx;

            
            Handles.DrawBezier(p0, p1, tangent0, tangent1, Color.green, null, 2f);

            Rect boxRect = new Rect(p0.x - boxSize / 2f, p0.y - boxSize / 2f, boxSize, boxSize);
            EditorGUI.DrawRect(boxRect, difficultyColors[values[i]]);
        }

        // Invisible button overlay to open full editor
        if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
        {
            MyIntCurveEditorWindow.ShowWindow(property.serializedObject, property.propertyPath);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return HEIGHT;
    }

    // Map value → rect
    private Vector2 ValueToRect(Rect rect, int time, int value)
    {
        float normX = Mathf.InverseLerp(TIME_MIN, TIME_MAX, time);
        float normY = Mathf.InverseLerp(VALUE_MIN, VALUE_MAX, value);
        return new Vector2(
            rect.xMin + normX * rect.width,
            rect.yMax - normY * rect.height
        );
    }
}
