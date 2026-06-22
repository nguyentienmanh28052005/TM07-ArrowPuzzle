// Custom Material Inspector for "Custom/TCP2 Hybrid" shader
// Self-contained - no dependency on TCP2 editor scripts
// Replicates the GUI command parsing (//# IF_KEYWORD, //# IF_PROPERTY, etc.)

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomTCP2HybridInspector : ShaderGUI
{
        const string PROP_MOBILE_MODE = "_UseMobileMode";
        const string KEYWORD_MOBILE_MODE = "TCP2_MOBILE";
        const string PROP_RENDERING_MODE = "_RenderingMode";
        const string PROP_ZWRITE = "_ZWrite";
        const string PROP_BLEND_SRC = "_SrcBlend";
        const string PROP_BLEND_DST = "_DstBlend";
        const string PROP_OUTLINE = "_UseOutline";
        const string PROP_OUTLINE_LAST = "_IndirectIntensityOutline";

        public enum RenderingMode { Opaque, Fade, Transparent }
        public enum MobileMode { Disabled = 0, Enabled = 1 }

        MaterialEditor _materialEditor;
        MaterialProperty[] _properties;
        static bool _isURP;
        static bool _isMobile;

        // Show/Hide stack
        static Stack<bool> ShowStack = new Stack<bool>();
        static bool ShowNextProperty { get; set; }
        static void PushShowProperty(bool value) { ShowStack.Push(ShowNextProperty); ShowNextProperty &= value; }
        static void PopShowProperty() { ShowNextProperty = ShowStack.Pop(); }

        // Disable stack
        static Stack<bool> DisableStack = new Stack<bool>();
        static bool DisableNextProperty { get; set; }
        static void PushDisableProperty(bool value) { DisableStack.Push(DisableNextProperty); DisableNextProperty |= value; }
        static void PopDisableProperty() { DisableNextProperty = DisableStack.Pop(); }

        // GUI command constants
        const string kGuiCommandPrefix = "//#";
        const string kGC_IfURP = "IF_URP";
        const string kGC_IfKeyword = "IF_KEYWORD";
        const string kGC_IfProperty = "IF_PROPERTY";
        const string kGC_EndIf = "END_IF";
        const string kGC_IfDisableKeyword = "IF_KEYWORD_DISABLE";
        const string kGC_IfDisableProperty = "IF_PROPERTY_DISABLE";
        const string kGC_EndIfDisable = "END_IF_DISABLE";
        const string kGC_Else = "ELSE";
        const string kGC_Label = "LABEL";

        Dictionary<int, List<GUICommand>> guiCommands = new Dictionary<int, List<GUICommand>>();
        Dictionary<int, string[]> splitLabels = new Dictionary<int, string[]>();

        bool initialized = false;
        AssetImporter shaderImporter;
        ulong lastTimestamp;

        // ==================== Styles (matching original TCP2) ====================
        static GUIStyle _lineStyle;
        static GUIStyle LineStyle
        {
            get
            {
                if (_lineStyle == null)
                {
                    _lineStyle = new GUIStyle();
                    _lineStyle.normal.background = EditorGUIUtility.whiteTexture;
                    _lineStyle.stretchWidth = true;
                }
                return _lineStyle;
            }
        }

        static GUIStyle _orangeBoldLabel;
        static GUIStyle OrangeBoldLabel
        {
            get
            {
                if (_orangeBoldLabel == null)
                {
                    var color = EditorGUIUtility.isProSkin ? new Color32(250, 130, 0, 255) : new Color32(220, 100, 0, 255);
                    _orangeBoldLabel = new GUIStyle(EditorStyles.label);
                    _orangeBoldLabel.normal.textColor = color;
                    _orangeBoldLabel.active.textColor = color;
                    _orangeBoldLabel.focused.textColor = color;
                    _orangeBoldLabel.hover.textColor = color;
                    _orangeBoldLabel.fontStyle = FontStyle.Bold;
                }
                return _orangeBoldLabel;
            }
        }

        static GUIStyle _orangeHeader;
        static GUIStyle OrangeHeader
        {
            get
            {
                if (_orangeHeader == null)
                {
                    _orangeHeader = new GUIStyle(OrangeBoldLabel);
                    _orangeHeader.fontSize = 16;
                }
                return _orangeHeader;
            }
        }

        // ==================== Initialization ====================

        void Initialize(MaterialEditor editor, MaterialProperty[] properties, bool force)
        {
            if ((!initialized || force) && editor != null)
            {
                initialized = true;

                // Check for outline in shader name
                IterateMaterials(mat => UpdateOutlineProp(mat, mat.shader.name.Contains("Outline")));

                // Split labels
                splitLabels.Clear();
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i].displayName.Contains("#"))
                    {
                        splitLabels.Add(i, properties[i].displayName.Split('#'));
                    }
                }

                // GUI commands
                guiCommands.Clear();

                var materials = new List<Material>();
                foreach (var o in editor.targets)
                {
                    var m = o as Material;
                    if (m != null) materials.Add(m);
                }

                if (materials.Count > 0 && materials[0].shader != null)
                {
                    var path = AssetDatabase.GetAssetPath(materials[0].shader);
                    shaderImporter = AssetImporter.GetAtPath(path);
                    if (shaderImporter != null) lastTimestamp = shaderImporter.assetTimeStamp;

                    path = Application.dataPath + path.Substring(6);
                    path = path.Replace('/', Path.DirectorySeparatorChar);
                    var lines = File.ReadAllLines(path);

                    bool insideProperties = false;
                    var regex = new Regex("[a-zA-Z0-9_]+\\s*\\(\\\"[a-zA-Z0-9#\\-() ]+\\\"[^\\)]*\\)");
                    int propertyCount = 0;
                    bool insideCommentBlock = false;

                    foreach (var l in lines)
                    {
                        var line = l.TrimStart();

                        if (insideProperties)
                        {
                            bool isComment = line.StartsWith("//");

                            if (line.Contains("/*")) insideCommentBlock = true;
                            if (line.Contains("*/")) insideCommentBlock = false;

                            if (line.StartsWith("}")) break;

                            if (line.StartsWith(kGuiCommandPrefix))
                            {
                                string fullCommand = line.Substring(kGuiCommandPrefix.Length).TrimStart();
                                int spaceIndex = fullCommand.IndexOf(' ');
                                string command = spaceIndex >= 0 ? fullCommand.Substring(0, spaceIndex) : fullCommand;

                                if (string.IsNullOrEmpty(command))
                                    AddGUICommand(propertyCount, new GC_Space());
                                else if (command.StartsWith("---"))
                                    AddGUICommand(propertyCount, new GC_Separator());
                                else if (command.StartsWith("==="))
                                    AddGUICommand(propertyCount, new GC_SeparatorDouble());
                                else if (command == kGC_IfURP)
                                    AddGUICommand(propertyCount, new GC_IfURP());
                                else if (command == kGC_IfKeyword)
                                {
                                    var expr = fullCommand.Substring(fullCommand.LastIndexOf(kGC_IfKeyword) + kGC_IfKeyword.Length + 1);
                                    AddGUICommand(propertyCount, new GC_IfKeyword { expression = expr, materials = materials.ToArray() });
                                }
                                else if (command == kGC_IfDisableKeyword)
                                {
                                    var expr = fullCommand.Substring(fullCommand.LastIndexOf(kGC_IfDisableKeyword) + kGC_IfDisableKeyword.Length + 1);
                                    AddGUICommand(propertyCount, new GC_IfDisableKeyword { expression = expr, materials = materials.ToArray() });
                                }
                                else if (command == kGC_IfDisableProperty)
                                {
                                    var expr = fullCommand.Substring(fullCommand.LastIndexOf(kGC_IfDisableProperty) + kGC_IfDisableProperty.Length + 1);
                                    AddGUICommand(propertyCount, new GC_IfDisableProperty { expression = expr, materials = materials.ToArray() });
                                }
                                else if (command == kGC_IfProperty)
                                {
                                    var expr = fullCommand.Substring(fullCommand.LastIndexOf(kGC_IfProperty) + kGC_IfProperty.Length + 1);
                                    AddGUICommand(propertyCount, new GC_IfProperty { expression = expr, materials = materials.ToArray() });
                                }
                                else if (command == kGC_EndIfDisable)
                                    AddGUICommand(propertyCount, new GC_EndIfDisable());
                                else if (command == kGC_EndIf)
                                    AddGUICommand(propertyCount, new GC_EndIf());
                                else if (command == kGC_Else)
                                    AddGUICommand(propertyCount, new GC_Else());
                                else if (command == kGC_Label)
                                {
                                    var label = fullCommand.Substring(fullCommand.LastIndexOf(kGC_Label) + kGC_Label.Length + 1);
                                    AddGUICommand(propertyCount, new GC_Label { label = label });
                                }
                                else
                                    AddGUICommand(propertyCount, new GC_Header { label = fullCommand });
                            }
                            else
                            {
                                if (regex.IsMatch(line) && !insideCommentBlock && !isComment)
                                    propertyCount++;
                            }
                        }

                        if (line.StartsWith("Properties"))
                            insideProperties = true;
                    }
                }
            }
        }

        void AddGUICommand(int propertyIndex, GUICommand command)
        {
            if (!guiCommands.ContainsKey(propertyIndex))
                guiCommands.Add(propertyIndex, new List<GUICommand>());
            guiCommands[propertyIndex].Add(command);
        }

        // ==================== Main GUI ====================

        void UpdateOutlineProp(Material material, bool needsOutline)
        {
            material.SetFloat(PROP_OUTLINE, needsOutline ? 1.0f : 0.0f);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            bool needsOutline = newShader.name.Contains("Outline") && newShader.name.Contains("Hybrid");
            UpdateOutlineProp(material, needsOutline);
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            _materialEditor = materialEditor;
            _properties = properties;

#if UNITY_2019_3_OR_NEWER
            var srp = GraphicsSettings.currentRenderPipeline;
#else
            var srp = GraphicsSettings.renderPipelineAsset;
#endif
            _isURP = srp != null && srp.GetType().ToString().Contains("Universal");
            _isMobile = FindProperty(PROP_MOBILE_MODE, properties).floatValue > 0;

            if (Event.current.type == EventType.Repaint)
            {
                bool force = (shaderImporter != null && shaderImporter.assetTimeStamp != lastTimestamp);
                Initialize(materialEditor, properties, force);
            }

            materialEditor.SetDefaultGUIWidths();

            ShowNextProperty = true;
            DisableNextProperty = false;
            ShowStack.Clear();
            DisableStack.Clear();

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = labelWidth - 50;

            // Header
            GUILayout.Label(new GUIContent(EditorGUIUtility.currentViewWidth > 355f ? "Custom TCP2 Hybrid Shader" : "TCP2 Hybrid"), OrangeHeader);
            DrawSeparator();

            // Mobile mode
            HandleMobileMode();
            DrawSeparator();

            // Transparency / Rendering Mode
            GUILayout.Label(new GUIContent("Transparency"), OrangeBoldLabel);
            HandleRenderingMode();

            // Iterate properties
            MaterialProperty outlineProp = null;
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].type == MaterialProperty.PropType.Float)
                    EditorGUIUtility.labelWidth = labelWidth - 50;

                if (guiCommands.ContainsKey(i))
                {
                    for (int j = 0; j < guiCommands[i].Count; j++)
                        guiCommands[i][j].OnGUI();
                }

                if (ShowNextProperty)
                {
                    bool guiEnabled = GUI.enabled;
                    GUI.enabled = !DisableNextProperty;

                    if (properties[i].name == PROP_OUTLINE)
                    {
                        outlineProp = properties[i];
                        HandleOutlinePass(outlineProp);
                    }
                    else if ((properties[i].flags & (MaterialProperty.PropFlags.HideInInspector | MaterialProperty.PropFlags.PerRendererData)) == MaterialProperty.PropFlags.None)
                    {
                        string displayName = splitLabels.ContainsKey(i) ? splitLabels[i][_isMobile ? 1 : 0] : properties[i].displayName;
                        float propertyHeight = materialEditor.GetPropertyHeight(properties[i], displayName);
                        Rect controlRect = EditorGUILayout.GetControlRect(true, propertyHeight, EditorStyles.layerMaskField);
                        materialEditor.ShaderProperty(controlRect, properties[i], displayName);
                    }

                    GUI.enabled = guiEnabled;
                }

                EditorGUIUtility.labelWidth = labelWidth;
            }

            // Show trailing gui commands
            int index = properties.Length;
            if (guiCommands.ContainsKey(index))
            {
                for (int j = 0; j < guiCommands[index].Count; j++)
                    guiCommands[index][j].OnGUI();
            }

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            materialEditor.RenderQueueField();
            materialEditor.EnableInstancingField();
        }

        // ==================== Outline Handling ====================

        float[] materialsOutlineMapping;

        void InitOutlineMapping()
        {
            if (materialsOutlineMapping == null || materialsOutlineMapping.Length != _materialEditor.targets.Length)
            {
                materialsOutlineMapping = new float[_materialEditor.targets.Length];
                int i = 0;
                foreach (var target in _materialEditor.targets)
                {
                    materialsOutlineMapping[i] = (target as Material).GetFloat(PROP_OUTLINE);
                    i++;
                }
            }
        }

        void UpdateOutlineMapping(bool updateShader)
        {
            int i = 0;
            foreach (var target in _materialEditor.targets)
            {
                var mat = target as Material;
                materialsOutlineMapping[i] = mat.GetFloat(PROP_OUTLINE);

                if (updateShader)
                {
                    bool needsOutline = materialsOutlineMapping[i] > 0;
                    bool hasOutline = mat.shader.name.Contains("Outline");

                    if (needsOutline && !hasOutline)
                    {
                        var outlineShader = Shader.Find(mat.shader.name + " Outline");
                        if (outlineShader != null) mat.shader = outlineShader;
                    }
                    else if (!needsOutline && hasOutline)
                    {
                        var baseShader = Shader.Find(mat.shader.name.Replace(" Outline", ""));
                        if (baseShader != null) mat.shader = baseShader;
                    }
                }
                i++;
            }
        }

        void HandleOutlinePass(MaterialProperty outlineProp)
        {
            bool showMixed = EditorGUI.showMixedValue;

            // Detect changes from Reset context menu
            InitOutlineMapping();
            bool outlineValuesChanged = false;
            int idx = 0;
            foreach (var target in _materialEditor.targets)
            {
                outlineValuesChanged |= materialsOutlineMapping[idx] != (target as Material).GetFloat(PROP_OUTLINE);
                idx++;
            }
            if (outlineValuesChanged)
            {
                UpdateOutlineMapping(true);
            }

            EditorGUI.showMixedValue = outlineProp.hasMixedValue;
            {
                EditorGUI.BeginChangeCheck();
                _materialEditor.ShaderProperty(outlineProp, new GUIContent(outlineProp.displayName));
                if (EditorGUI.EndChangeCheck())
                {
                    bool enableOutline = outlineProp.floatValue > 0;
                    Undo.RecordObjects(_materialEditor.targets, (enableOutline ? "Enable" : "Disable") + " Outline on Material(s)");
                    UpdateOutlineMapping(true);
                }
            }
            EditorGUI.showMixedValue = showMixed;
        }

        // ==================== Helper Methods ====================

        void IterateMaterials(Action<Material> action)
        {
            foreach (var target in _materialEditor.targets)
                action(target as Material);
        }

        static void GUILine(Color color, float height = 2f)
        {
            var position = GUILayoutUtility.GetRect(0f, float.MaxValue, height, height, LineStyle);
            if (Event.current.type == EventType.Repaint)
            {
                var orgColor = GUI.color;
                GUI.color = orgColor * color;
                LineStyle.Draw(position, false, false, false, false);
                GUI.color = orgColor;
            }
        }

        static void SeparatorSimple()
        {
            var color = EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.65f, 0.65f, 0.65f);
            GUILine(color, 1);
            GUILayout.Space(1);
        }

        static void DrawSeparator()
        {
            var colorDark = EditorGUIUtility.isProSkin ? new Color(.1f, .1f, .1f) : new Color(.3f, .3f, .3f);
            var colorBright = EditorGUIUtility.isProSkin ? new Color(.3f, .3f, .3f) : new Color(.9f, .9f, .9f);
            GUILayout.Space(4);
            GUILine(colorDark, 1);
            GUILine(colorBright, 1);
            GUILayout.Space(4);
        }

        string mobileModeHelp = "'Mobile Mode' makes the shader faster by disabling some of the features, and doing more calculations in the vertex shader at the expense of precision.";

        void HandleMobileMode()
        {
            bool showMixed = EditorGUI.showMixedValue;
            var mobileModeProp = FindProperty(PROP_MOBILE_MODE, _properties);
            EditorGUI.showMixedValue = mobileModeProp.hasMixedValue;
            {
                var newMobileMode = (MobileMode)EditorGUILayout.EnumPopup(new GUIContent("Mobile Mode", mobileModeHelp), (MobileMode)mobileModeProp.floatValue);
                if ((float)newMobileMode != mobileModeProp.floatValue)
                {
                    Undo.RecordObjects(_materialEditor.targets, "Change Material Mobile Mode");
                    IterateMaterials(mat =>
                    {
                        mat.SetFloat(PROP_MOBILE_MODE, (float)newMobileMode);
                        if (newMobileMode == MobileMode.Enabled)
                            mat.EnableKeyword(KEYWORD_MOBILE_MODE);
                        else
                            mat.DisableKeyword(KEYWORD_MOBILE_MODE);
                    });
                }
            }
            EditorGUI.showMixedValue = showMixed;
            if (mobileModeProp.floatValue > 0)
                EditorGUILayout.HelpBox(mobileModeHelp, MessageType.Info);
        }

        void HandleRenderingMode()
        {
            bool showMixed = EditorGUI.showMixedValue;
            var renderingModeProp = FindProperty(PROP_RENDERING_MODE, _properties);
            EditorGUI.showMixedValue = renderingModeProp.hasMixedValue;
            {
                var newRenderingMode = (RenderingMode)EditorGUILayout.EnumPopup(new GUIContent("Rendering Mode"), (RenderingMode)renderingModeProp.floatValue);
                if ((float)newRenderingMode != renderingModeProp.floatValue)
                {
                    Undo.RecordObjects(_materialEditor.targets, "Change Material Rendering Mode");
                    SetRenderingMode(newRenderingMode);
                }
            }
            EditorGUI.showMixedValue = showMixed;
        }

        void SetRenderingMode(RenderingMode mode)
        {
            switch (mode)
            {
                case RenderingMode.Opaque:
                    IterateMaterials(mat => { mat.renderQueue = (int)RenderQueue.Geometry; mat.SetFloat(PROP_ZWRITE, 1); mat.SetFloat(PROP_BLEND_SRC, (float)BlendMode.One); mat.SetFloat(PROP_BLEND_DST, (float)BlendMode.Zero); mat.DisableKeyword("_ALPHAPREMULTIPLY_ON"); });
                    break;
                case RenderingMode.Fade:
                    IterateMaterials(mat => { mat.renderQueue = (int)RenderQueue.Transparent; mat.SetFloat(PROP_ZWRITE, 0); mat.SetFloat(PROP_BLEND_SRC, (float)BlendMode.SrcAlpha); mat.SetFloat(PROP_BLEND_DST, (float)BlendMode.OneMinusSrcAlpha); mat.DisableKeyword("_ALPHAPREMULTIPLY_ON"); });
                    break;
                case RenderingMode.Transparent:
                    IterateMaterials(mat => { mat.renderQueue = (int)RenderQueue.Transparent; mat.SetFloat(PROP_ZWRITE, 0); mat.SetFloat(PROP_BLEND_SRC, (float)BlendMode.One); mat.SetFloat(PROP_BLEND_DST, (float)BlendMode.OneMinusSrcAlpha); mat.EnableKeyword("_ALPHAPREMULTIPLY_ON"); });
                    break;
            }
            IterateMaterials(mat => mat.SetFloat(PROP_RENDERING_MODE, (float)mode));
        }

        // ==================== Expression Parser ====================

        static bool EvaluateExpression(string expression, Func<string, bool> evalFunc)
        {
            // Remove whitespace and double && ||
            var cleanExpr = new StringBuilder();
            for (var i = 0; i < expression.Length; i++)
            {
                switch (expression[i])
                {
                    case ' ': break;
                    case '&': cleanExpr.Append(expression[i]); i++; break;
                    case '|': cleanExpr.Append(expression[i]); i++; break;
                    default: cleanExpr.Append(expression[i]); break;
                }
            }

            var tokens = new List<ExprToken>();
            var reader = new StringReader(cleanExpr.ToString());
            ExprToken t;
            do
            {
                t = new ExprToken(reader);
                tokens.Add(t);
            } while (t.type != ExprToken.TokenType.EXPR_END);

            var polishNotation = ExprToken.TransformToPolishNotation(tokens);
            var enumerator = polishNotation.GetEnumerator();
            enumerator.MoveNext();

            var root = MakeExpression(ref enumerator, evalFunc);
            return root != null && root.Evaluate();
        }

        class ExprToken
        {
            public enum TokenType { OPEN_PAREN, CLOSE_PAREN, UNARY_OP, BINARY_OP, LITERAL, EXPR_END }
            public TokenType type;
            public string value;

            bool IsControlChar(char c) => c == '(' || c == ')' || c == '!' || c == '&' || c == '|';

            public ExprToken(StringReader s)
            {
                var c = s.Read();
                if (c == -1) { type = TokenType.EXPR_END; value = ""; return; }
                var ch = (char)c;
                bool embeddedNot = (ch == '!' && s.Peek() != '(');

                if (!embeddedNot)
                {
                    switch (ch)
                    {
                        case '(': type = TokenType.OPEN_PAREN; value = "("; return;
                        case ')': type = TokenType.CLOSE_PAREN; value = ")"; return;
                        case '!': type = TokenType.UNARY_OP; value = "NOT"; return;
                        case '&': type = TokenType.BINARY_OP; value = "AND"; return;
                        case '|': type = TokenType.BINARY_OP; value = "OR"; return;
                    }
                }

                var sb = new StringBuilder();
                sb.Append(ch);
                while (s.Peek() != -1 && !IsControlChar((char)s.Peek()))
                    sb.Append((char)s.Read());
                type = TokenType.LITERAL;
                value = sb.ToString();
            }

            public static List<ExprToken> TransformToPolishNotation(List<ExprToken> infixTokenList)
            {
                var outputQueue = new Queue<ExprToken>();
                var stack = new Stack<ExprToken>();
                foreach (var t in infixTokenList)
                {
                    switch (t.type)
                    {
                        case TokenType.LITERAL: outputQueue.Enqueue(t); break;
                        case TokenType.BINARY_OP:
                        case TokenType.UNARY_OP:
                        case TokenType.OPEN_PAREN: stack.Push(t); break;
                        case TokenType.CLOSE_PAREN:
                            while (stack.Peek().type != TokenType.OPEN_PAREN) outputQueue.Enqueue(stack.Pop());
                            stack.Pop();
                            if (stack.Count > 0 && stack.Peek().type == TokenType.UNARY_OP) outputQueue.Enqueue(stack.Pop());
                            break;
                    }
                }
                while (stack.Count > 0) outputQueue.Enqueue(stack.Pop());
                var list = new List<ExprToken>(outputQueue);
                list.Reverse();
                return list;
            }
        }

        abstract class BoolExpr { public abstract bool Evaluate(); }
        class ExprLeaf : BoolExpr
        {
            string content; Func<string, bool> evalFunc;
            public ExprLeaf(Func<string, bool> f, string c) { evalFunc = f; content = c; }
            public override bool Evaluate()
            {
                if (content.Length > 0 && content[0] == '!') return !evalFunc(content.Substring(1));
                return evalFunc(content);
            }
        }
        class ExprAnd : BoolExpr { BoolExpr l, r; public ExprAnd(BoolExpr a, BoolExpr b) { l = a; r = b; } public override bool Evaluate() => l.Evaluate() && r.Evaluate(); }
        class ExprOr : BoolExpr { BoolExpr l, r; public ExprOr(BoolExpr a, BoolExpr b) { l = a; r = b; } public override bool Evaluate() => l.Evaluate() || r.Evaluate(); }
        class ExprNot : BoolExpr { BoolExpr e; public ExprNot(BoolExpr a) { e = a; } public override bool Evaluate() => !e.Evaluate(); }

        static BoolExpr MakeExpression(ref List<ExprToken>.Enumerator en, Func<string, bool> evalFunc)
        {
            if (en.Current.type == ExprToken.TokenType.LITERAL) { var lit = new ExprLeaf(evalFunc, en.Current.value); en.MoveNext(); return lit; }
            if (en.Current.value == "NOT") { en.MoveNext(); return new ExprNot(MakeExpression(ref en, evalFunc)); }
            if (en.Current.value == "AND") { en.MoveNext(); var l = MakeExpression(ref en, evalFunc); var r = MakeExpression(ref en, evalFunc); return new ExprAnd(l, r); }
            if (en.Current.value == "OR") { en.MoveNext(); var l = MakeExpression(ref en, evalFunc); var r = MakeExpression(ref en, evalFunc); return new ExprOr(l, r); }
            return null;
        }

        // ==================== GUI Commands ====================

        abstract class GUICommand { public virtual void OnGUI() { } }

        class GC_Separator : GUICommand
        {
            public override void OnGUI()
            {
                if (ShowNextProperty) SeparatorSimple();
            }
        }

        class GC_SeparatorDouble : GUICommand
        {
            public override void OnGUI()
            {
                if (ShowNextProperty)
                    DrawSeparator();
            }
        }

        class GC_Space : GUICommand { public override void OnGUI() { if (ShowNextProperty) GUILayout.Space(8); } }

        class GC_Header : GUICommand
        {
            public string label;
            GUIContent guiContent;
            public override void OnGUI()
            {
                if (guiContent == null) guiContent = new GUIContent(label);
                if (ShowNextProperty) GUILayout.Label(guiContent, OrangeBoldLabel);
            }
        }

        class GC_Label : GUICommand
        {
            public string label;
            GUIContent guiContent;
            public override void OnGUI()
            {
                if (guiContent == null) guiContent = new GUIContent(label);
                if (ShowNextProperty) GUILayout.Label(guiContent);
            }
        }

        class GC_IfKeyword : GUICommand
        {
            public string expression;
            public Material[] materials;
            public override void OnGUI()
            {
                bool show = EvaluateExpression(expression, s =>
                {
                    foreach (var m in materials)
                        if (m.IsKeywordEnabled(s)) return true;
                    return false;
                });
                PushShowProperty(show);
            }
        }

        class GC_IfURP : GUICommand
        {
            public override void OnGUI() { PushShowProperty(_isURP); }
        }

        class GC_Else : GUICommand
        {
            public override void OnGUI()
            {
                bool invertCondition = !ShowNextProperty;
                PopShowProperty();
                PushShowProperty(invertCondition);
            }
        }

        class GC_EndIf : GUICommand { public override void OnGUI() { PopShowProperty(); } }
        class GC_EndIfDisable : GUICommand { public override void OnGUI() { PopDisableProperty(); } }

        class GC_IfProperty : GUICommand
        {
            string _expression;
            public string expression
            {
                get { return _expression; }
                set { _expression = value.Replace("!=", "<>"); }
            }
            public Material[] materials;

            public override void OnGUI()
            {
                bool show = EvaluateExpression(expression, EvaluatePropertyExpression);
                PushShowProperty(show);
            }

            protected bool EvaluatePropertyExpression(string expr)
            {
                var reader = new StringReader(expr);
                string property = "";
                string op = "";
                float value = 0f;
                int overflow = 0;

                while (true)
                {
                    char c = (char)reader.Read();
                    if (c == '=' || c == '>' || c == '<' || c == '!')
                    {
                        op += c;
                        char c2 = (char)reader.Peek();
                        if (c2 == '=' || c2 == '>')
                        {
                            reader.Read();
                            op += c2;
                        }
                        var end = reader.ReadToEnd();
                        if (!float.TryParse(end, out value)) return false;
                        break;
                    }
                    property += c;
                    overflow++;
                    if (overflow >= 9999) return false;
                }

                bool conditionMet = false;
                foreach (var m in materials)
                {
                    float propValue;
                    if (property.Contains(".x") || property.Contains(".y") || property.Contains(".z") || property.Contains(".w"))
                    {
                        string[] split = property.Split('.');
                        var vec = m.GetVector(split[0]);
                        switch (split[1])
                        {
                            case "x": propValue = vec.x; break;
                            case "y": propValue = vec.y; break;
                            case "z": propValue = vec.z; break;
                            case "w": propValue = vec.w; break;
                            default: propValue = 0; break;
                        }
                    }
                    else
                        propValue = m.GetFloat(property);

                    switch (op)
                    {
                        case ">=": conditionMet = propValue >= value; break;
                        case "<=": conditionMet = propValue <= value; break;
                        case ">": conditionMet = propValue > value; break;
                        case "<": conditionMet = propValue < value; break;
                        case "<>": conditionMet = propValue != value; break;
                        case "==": conditionMet = propValue == value; break;
                    }
                    if (conditionMet) return true;
                }
                return false;
            }
        }

        class GC_IfDisableProperty : GC_IfProperty
        {
            public override void OnGUI()
            {
                bool enable = EvaluateExpression(expression, EvaluatePropertyExpression);
                PushDisableProperty(!enable);
            }
        }

        class GC_IfDisableKeyword : GC_IfKeyword
        {
            public override void OnGUI()
            {
                bool enable = EvaluateExpression(expression, s =>
                {
                    foreach (var m in materials)
                        if (m.IsKeywordEnabled(s)) return true;
                    return false;
                });
                PushDisableProperty(!enable);
            }
    }
}
