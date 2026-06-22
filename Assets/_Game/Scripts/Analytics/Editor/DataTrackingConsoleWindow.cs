using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

public class DataTrackingConsoleWindow : EditorWindow
{
    private class LogEntry
    {
        public string Time;
        public string SequenceID;
        public string EventName;
        public Dictionary<string, object> Parameters;
        public string ParamString;
    }

    private List<LogEntry> logs = new List<LogEntry>();
    private Vector2 scrollPos;
    private GUIStyle headerStyle;
    private GUIStyle rowStyle;
    private GUIStyle altRowStyle;

    [MenuItem("Tools/Data Tracking Console")]
    public static void ShowWindow()
    {
        GetWindow<DataTrackingConsoleWindow>("Tracking Console");
    }

    private void OnEnable()
    {
        Application.logMessageReceived -= HandleUnityLog;
        Application.logMessageReceived += HandleUnityLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleUnityLog;
    }

    private void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Log && logString.StartsWith("databucket: "))
        {
            string data = logString.Substring("databucket: ".Length);
            ParseDataBucket(data);
        }
    }

    private void ParseDataBucket(string data)
    {
        if (string.IsNullOrEmpty(data)) return;
        string[] lines = data.Split('\n');
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;
            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(line);
                string eventName = dict.ContainsKey("eventName") ? dict["eventName"].ToString() : "unknown";
                string seqId = dict.ContainsKey("sequenceId") ? dict["sequenceId"].ToString() : "-";
                
                var parameters = new Dictionary<string, object>();
                if (dict.ContainsKey("eventData"))
                {
                    var eventDataJson = dict["eventData"].ToString();
                    parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(eventDataJson);
                }

                HandleLogEvent(eventName, parameters, seqId);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing log data: " + e.Message);
            }
        }
    }

    private void HandleLogEvent(string eventName, Dictionary<string, object> parameters, string seqId)
    {
        var sb = new StringBuilder();
        if (parameters != null)
        {
            foreach (var kvp in parameters)
            {
                sb.Append($"[{kvp.Key}: {kvp.Value}]  ");
            }
        }

        logs.Add(new LogEntry
        {
            Time = System.DateTime.Now.ToString("HH:mm:ss.fff"),
            SequenceID = seqId,
            EventName = eventName,
            Parameters = parameters,
            ParamString = sb.ToString()
        });

        // Giữ tối đa 1000 log gần nhất
        if (logs.Count > 1000)
            logs.RemoveAt(0);

        Repaint();
    }

    private void OnGUI()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(5, 5, 5, 5)
            };
            
            rowStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true,
                padding = new RectOffset(5, 5, 2, 2)
            };

            altRowStyle = new GUIStyle(rowStyle);
            Color altColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 0.5f) : new Color(0.8f, 0.8f, 0.8f, 0.5f);
            altRowStyle.normal.background = MakeTex(2, 2, altColor);
        }

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            logs.Clear();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Time", headerStyle, GUILayout.Width(100));
        GUILayout.Label("Seq ID", headerStyle, GUILayout.Width(60));
        GUILayout.Label("Event Name", headerStyle, GUILayout.Width(180));
        GUILayout.Label("Parameters", headerStyle);
        EditorGUILayout.EndHorizontal();

        GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        string colorHex = EditorGUIUtility.isProSkin ? "#00FFFF" : "#0000FF";

        for (int i = 0; i < logs.Count; i++)
        {
            var log = logs[logs.Count - 1 - i]; // Show newest first
            GUIStyle currentStyle = i % 2 == 0 ? rowStyle : altRowStyle;

            EditorGUILayout.BeginHorizontal(currentStyle);
            GUILayout.Label(log.Time, currentStyle, GUILayout.Width(100));
            GUILayout.Label(log.SequenceID, currentStyle, GUILayout.Width(60));
            GUILayout.Label($"<color={colorHex}><b>{log.EventName}</b></color>", currentStyle, GUILayout.Width(180));
            
            EditorGUILayout.SelectableLabel(log.ParamString, currentStyle, GUILayout.Height(20), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
