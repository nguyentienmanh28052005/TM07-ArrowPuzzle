using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEditor.UI;
using UnityEngine.UI;

namespace UIExtentions.UnityEditor.UI
{
    /// <summary>
    /// Editor class used to edit UI Sprites.
    /// </summary>

    [CustomEditor(typeof(UIExtensions.UITexture), true)]
    [CanEditMultipleObjects]
    public class UITextureEditor : GraphicEditor
    {
        SerializedProperty m_FillMethod;
        SerializedProperty m_FillOrigin;
        SerializedProperty m_FillAmount;
        SerializedProperty m_FillClockwise;
        SerializedProperty m_Type;
        SerializedProperty m_FillCenter;
        SerializedProperty m_Texture;
        SerializedProperty m_PreserveAspect;
        SerializedProperty m_UseSpriteMesh;
        SerializedProperty m_PixelsPerUnitMultiplier;
        GUIContent m_SpriteContent;
        GUIContent m_SpriteTypeContent;
        GUIContent m_ClockwiseContent;
        AnimBool m_ShowSlicedOrTiled;
        AnimBool m_ShowSliced;
        AnimBool m_ShowTiled;
        AnimBool m_ShowFilled;
        AnimBool m_ShowType;

        private static Texture2D mBackdropTex;
        public Texture2D backdropTexture
        {
            get
            {
                if (mBackdropTex == null) mBackdropTex = CreateCheckerTex(new Color(0.1f, 0.1f, 0.1f, 0.5f), new Color(0.2f, 0.2f, 0.2f, 0.5f));
                return mBackdropTex;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            m_SpriteContent = EditorGUIUtility.TrTextContent("Source Image");
            m_SpriteTypeContent     = EditorGUIUtility.TrTextContent("Image Type");
            m_ClockwiseContent      = EditorGUIUtility.TrTextContent("Clockwise");

            m_Texture                = serializedObject.FindProperty("m_Texture");
            m_Type                  = serializedObject.FindProperty("m_Type");
            m_FillCenter            = serializedObject.FindProperty("m_FillCenter");
            m_FillMethod            = serializedObject.FindProperty("m_FillMethod");
            m_FillOrigin            = serializedObject.FindProperty("m_FillOrigin");
            m_FillClockwise         = serializedObject.FindProperty("m_FillClockwise");
            m_FillAmount            = serializedObject.FindProperty("m_FillAmount");
            m_PreserveAspect        = serializedObject.FindProperty("m_PreserveAspect");
            m_UseSpriteMesh         = serializedObject.FindProperty("m_UseSpriteMesh");
            m_PixelsPerUnitMultiplier = serializedObject.FindProperty("m_PixelsPerUnitMultiplier");

            m_ShowType = new AnimBool(m_Texture.objectReferenceValue != null);
            m_ShowType.valueChanged.AddListener(Repaint);

            var typeEnum = (UIExtensions.UITexture.Type)m_Type.enumValueIndex;

            m_ShowSlicedOrTiled = new AnimBool(!m_Type.hasMultipleDifferentValues && typeEnum == UIExtensions.UITexture.Type.Sliced);
            m_ShowSliced = new AnimBool(!m_Type.hasMultipleDifferentValues && typeEnum == UIExtensions.UITexture.Type.Sliced);
            m_ShowTiled = new AnimBool(!m_Type.hasMultipleDifferentValues && typeEnum == UIExtensions.UITexture.Type.Tiled);
            m_ShowFilled = new AnimBool(!m_Type.hasMultipleDifferentValues && typeEnum == UIExtensions.UITexture.Type.Filled);
            m_ShowSlicedOrTiled.valueChanged.AddListener(Repaint);
            m_ShowSliced.valueChanged.AddListener(Repaint);
            m_ShowTiled.valueChanged.AddListener(Repaint);
            m_ShowFilled.valueChanged.AddListener(Repaint);

            SetShowNativeSize(true);
        }

        protected override void OnDisable()
        {
            m_ShowType.valueChanged.RemoveListener(Repaint);
            m_ShowSlicedOrTiled.valueChanged.RemoveListener(Repaint);
            m_ShowSliced.valueChanged.RemoveListener(Repaint);
            m_ShowTiled.valueChanged.RemoveListener(Repaint);
            m_ShowFilled.valueChanged.RemoveListener(Repaint);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SpriteGUI();
            AppearanceControlsGUI();
            DrawRectProperty("UV Rect", serializedObject, "m_EditorUvRect");
            RaycastControlsGUI();
            MaskableControlsGUI();
            m_ShowType.target = m_Texture.objectReferenceValue != null;
            if (EditorGUILayout.BeginFadeGroup(m_ShowType.faded))
                TypeGUI();
            EditorGUILayout.EndFadeGroup();

            SetShowNativeSize(false);
            if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded))
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(m_PreserveAspect);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
            NativeSizeButtonGUI();

            serializedObject.ApplyModifiedProperties();
        }

        private static Texture2D CreateCheckerTex (Color c0, Color c1)
        {
            Texture2D tex = new Texture2D(16, 16);
            tex.name = "[Generated] Checker Texture";
            tex.hideFlags = HideFlags.DontSave;

            for (int y = 0; y < 8; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c1);
            for (int y = 8; y < 16; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c0);
            for (int y = 0; y < 8; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c0);
            for (int y = 8; y < 16; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c1);

            tex.Apply();
            tex.filterMode = FilterMode.Point;
            return tex;
        }
        
        public void DrawRectProperty (string name, SerializedObject serializedObject, string field)
        {
            DrawRectProperty(name, serializedObject, field, 56f, 18f);
        }
        
        public void DrawRectProperty (string name, SerializedObject serializedObject, string field, float labelWidth, float spacing)
        {
            if (serializedObject.FindProperty(field) != null)
            {

                EditorGUILayout.Space(18);
                var lb = EditorGUIUtility.labelWidth;
                EditorGUILayout.BeginHorizontal();
                {

                    GUILayout.Label(name, GUILayout.Width(labelWidth));

                    EditorGUIUtility.labelWidth = 20f;
                    EditorGUILayout.BeginVertical();
                    var x = DrawProperty("X", serializedObject, field + ".x", GUILayout.MinWidth(50f));
                    var y = DrawProperty("Y", serializedObject, field + ".y", GUILayout.MinWidth(50f));
                    EditorGUILayout.EndVertical();
                    EditorGUIUtility.labelWidth = 50f;
                    EditorGUILayout.BeginVertical();
                    var w = DrawProperty("Width", serializedObject, field + ".width", GUILayout.MinWidth(80f));
                    var h = DrawProperty("Height", serializedObject, field + ".height", GUILayout.MinWidth(80f));
                    

                    EditorGUILayout.EndVertical();
                    EditorGUIUtility.labelWidth = 80f;
                    if (spacing != 0f) EditorGUILayout.Space(spacing);
                    UIExtensions.UITexture image = target as UIExtensions.UITexture;
                    var rct = image.m_EditorUvRect;
                    image.uvRect = new Rect(rct.x / 100f, rct.y / 100f, rct.width / 100f, rct.height / 100f);

                }

                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = lb;
                EditorGUILayout.Space(18);
            }
        }

        private void Refresh()
        {
           
        }


        public SerializedProperty DrawProperty (string label, SerializedObject serializedObject, string property, bool padding, params GUILayoutOption[] options)
        {
            SerializedProperty sp = serializedObject.FindProperty(property);

            if (sp != null)
            {
                if (NGUISettings.minimalisticLook) padding = false;

                if (padding) EditorGUILayout.BeginHorizontal();

                if (label != null) EditorGUILayout.PropertyField(sp, new GUIContent(label), options);
                else EditorGUILayout.PropertyField(sp, options);

                if (padding)
                {
                    GUILayout.Space(18f);
                    EditorGUILayout.EndHorizontal();
                }
            }
            else Debug.LogWarning("Unable to find property " + property);
            return sp;
        }
        
        public SerializedProperty DrawProperty (string label, SerializedObject serializedObject, string property, params GUILayoutOption[] options)
        {
            return DrawProperty(label, serializedObject, property, false, options);
        }
        
        void SetShowNativeSize(bool instant)
        {
            UIExtensions.UITexture.Type type = (UIExtensions.UITexture.Type)m_Type.enumValueIndex;
            bool showNativeSize = (type == UIExtensions.UITexture.Type.Simple || type == UIExtensions.UITexture.Type.Filled) && m_Texture.objectReferenceValue != null;
            base.SetShowNativeSize(showNativeSize, instant);
        }

        /// <summary>
        /// Draw the atlas and Image selection fields.
        /// </summary>

        protected void SpriteGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_Texture, m_SpriteContent);
            if (EditorGUI.EndChangeCheck())
            {
                var newSprite = m_Texture.objectReferenceValue as Sprite;
                if (newSprite)
                {
                    UIExtensions.UITexture.Type oldType = (UIExtensions.UITexture.Type)m_Type.enumValueIndex;
                    if (newSprite.border.SqrMagnitude() > 0)
                    {
                        m_Type.enumValueIndex = (int)UIExtensions.UITexture.Type.Sliced;
                    }
                    else if (oldType == UIExtensions.UITexture.Type.Sliced)
                    {
                        m_Type.enumValueIndex = (int)UIExtensions.UITexture.Type.Simple;
                    }
                }
                (serializedObject.targetObject as UIExtensions.UITexture).DisableSpriteOptimizations();
            }
        }

        /// <summary>
        /// Sprites's custom properties based on the type.
        /// </summary>

        protected void TypeGUI()
        {
            EditorGUILayout.PropertyField(m_Type, m_SpriteTypeContent);

            ++EditorGUI.indentLevel;
            {
                UIExtensions.UITexture.Type typeEnum = (UIExtensions.UITexture.Type)m_Type.enumValueIndex;

                bool showSlicedOrTiled = (!m_Type.hasMultipleDifferentValues && (typeEnum == UIExtensions.UITexture.Type.Sliced || typeEnum == UIExtensions.UITexture.Type.Tiled));
                if (showSlicedOrTiled && targets.Length > 1)
                    showSlicedOrTiled = targets.Select(obj => obj as UIExtensions.UITexture).All(img => img.hasBorder);

                m_ShowSlicedOrTiled.target = showSlicedOrTiled;
                m_ShowSliced.target = (showSlicedOrTiled && !m_Type.hasMultipleDifferentValues && typeEnum == UIExtensions.UITexture.Type.Sliced);
                m_ShowTiled.target = (showSlicedOrTiled && !m_Type.hasMultipleDifferentValues && typeEnum == UIExtensions.UITexture.Type.Tiled);
                m_ShowFilled.target = (!m_Type.hasMultipleDifferentValues && typeEnum == UIExtensions.UITexture.Type.Filled);

                UIExtensions.UITexture image = target as UIExtensions.UITexture;
                if (EditorGUILayout.BeginFadeGroup(m_ShowSlicedOrTiled.faded))
                {
                    EditorGUILayout.PropertyField(m_FillCenter);
                    EditorGUILayout.PropertyField(m_PixelsPerUnitMultiplier);
                    EditorGUILayout.Space();
                    DrawBorderProperty("Border", serializedObject, "m_border");
                }
                EditorGUILayout.EndFadeGroup();

                if (EditorGUILayout.BeginFadeGroup(m_ShowFilled.faded))
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(m_FillMethod);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_FillOrigin.intValue = 0;
                    }
                    switch ((Image.FillMethod)m_FillMethod.enumValueIndex)
                    {
                        case Image.FillMethod.Horizontal:
                            m_FillOrigin.intValue = (int)(Image.OriginHorizontal)EditorGUILayout.EnumPopup("Fill Origin", (Image.OriginHorizontal)m_FillOrigin.intValue);
                            break;
                        case Image.FillMethod.Vertical:
                            m_FillOrigin.intValue = (int)(Image.OriginVertical)EditorGUILayout.EnumPopup("Fill Origin", (Image.OriginVertical)m_FillOrigin.intValue);
                            break;
                        case Image.FillMethod.Radial90:
                            m_FillOrigin.intValue = (int)(Image.Origin90)EditorGUILayout.EnumPopup("Fill Origin", (Image.Origin90)m_FillOrigin.intValue);
                            break;
                        case Image.FillMethod.Radial180:
                            m_FillOrigin.intValue = (int)(Image.Origin180)EditorGUILayout.EnumPopup("Fill Origin", (Image.Origin180)m_FillOrigin.intValue);
                            break;
                        case Image.FillMethod.Radial360:
                            m_FillOrigin.intValue = (int)(Image.Origin360)EditorGUILayout.EnumPopup("Fill Origin", (Image.Origin360)m_FillOrigin.intValue);
                            break;
                    }
                    EditorGUILayout.PropertyField(m_FillAmount);
                    if ((UIExtensions.UITexture.FillMethod)m_FillMethod.enumValueIndex > UIExtensions.UITexture.FillMethod.Vertical)
                    {
                        EditorGUILayout.PropertyField(m_FillClockwise, m_ClockwiseContent);
                    }
                }
                EditorGUILayout.EndFadeGroup();
            }
            --EditorGUI.indentLevel;
        }

        
        public void DrawBorderProperty (string name, SerializedObject serializedObject, string field)
        {
            if (serializedObject.FindProperty(field) != null)
            {
                var lb = EditorGUIUtility.labelWidth;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(16);
                    GUILayout.Label(name, GUILayout.Width(75f));

                    SetLabelWidth(50f);
                    GUILayout.BeginVertical();
                    DrawProperty("Left", serializedObject, field + ".x", GUILayout.MinWidth(80f));
                    DrawProperty("Bottom", serializedObject, field + ".y", GUILayout.MinWidth(80f));
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    DrawProperty("Right", serializedObject, field + ".z", GUILayout.MinWidth(80f));
                    DrawProperty("Top", serializedObject, field + ".w", GUILayout.MinWidth(80f));
                    GUILayout.EndVertical();

                    SetLabelWidth(80f);
                }
                GUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = lb;
            }
        }

        private void SetLabelWidth(float p0)
        {
            EditorGUIUtility.labelWidth = 80f;
        }

        /// <summary>
        /// All graphics have a preview.
        /// </summary>

        public override bool HasPreviewGUI() { return true; }

        /// <summary>
        /// Draw the Image preview.
        /// </summary>

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            UIExtensions.UITexture image = target as UIExtensions.UITexture;
            if (image == null) return;

            Texture2D sf = image.texture;
            if (sf == null) return;

            if (sf != null)
            {
                Rect tc = image.uvRect;
                tc.x *= sf.width;
                
                tc.y *= sf.height;
                tc.width *= sf.width;
                tc.height *= sf.height;
                DrawSprite(sf, rect, image.canvasRenderer.GetColor(), tc, image.border);

            }
        }
        
        /// <summary>
        /// Draws the tiled texture. Like GUI.DrawTexture() but tiled instead of stretched.
        /// </summary>
        public void DrawTiledTexture (Rect rect, Texture tex)
        {
            GUI.BeginGroup(rect);
            {
                int width = Mathf.RoundToInt(rect.width);
                int height = Mathf.RoundToInt(rect.height);

                for (int y = 0; y < height; y += tex.height)
                {
                    for (int x = 0; x < width; x += tex.width)
                    {
                        GUI.DrawTexture(new Rect(x, y, tex.width, tex.height), tex);
                    }
                }
            }
            GUI.EndGroup();
        }
        
        /// <summary>
        /// Draw a sprite preview.
        /// </summary>
        public void DrawSprite(Texture2D tex, Rect drawRect, Color color, Rect textureRect, Vector4 border)
        {
	        DrawSprite(tex, drawRect, color, null, Mathf.RoundToInt(textureRect.x), Mathf.RoundToInt(tex.height - textureRect.y - textureRect.height), Mathf.RoundToInt(textureRect.width), Mathf.RoundToInt(textureRect.height), Mathf.RoundToInt(border.x), Mathf.RoundToInt(border.y), Mathf.RoundToInt(border.z), Mathf.RoundToInt(border.w));
        }

        /// <summary>
        /// Draw a sprite preview.
        /// </summary>
        public void DrawSprite(Texture2D tex, Rect drawRect, Color color, Material mat, int x, int y, int width, int height, int borderLeft, int borderBottom, int borderRight, int borderTop)
        {
	        if (!tex) return;

	        // Create the texture rectangle that is centered inside rect.
	        Rect outerRect = drawRect;
	        outerRect.width = width;
	        outerRect.height = height;

	        if (width > 0)
	        {
		        float f = drawRect.width / outerRect.width;
		        outerRect.width *= f;
		        outerRect.height *= f;
	        }

	        if (drawRect.height > outerRect.height)
	        {
		        outerRect.y += (drawRect.height - outerRect.height) * 0.5f;
	        }
	        else if (outerRect.height > drawRect.height)
	        {
		        float f = drawRect.height / outerRect.height;
		        outerRect.width *= f;
		        outerRect.height *= f;
	        }

	        if (drawRect.width > outerRect.width) outerRect.x += (drawRect.width - outerRect.width) * 0.5f;

	        // Draw the background
	        DrawTiledTexture(outerRect, backdropTexture);

	        // Draw the sprite
	        GUI.color = color;

	        if (mat == null)
	        {
		        Rect uv = new Rect(x, y, width, height);
		        uv = NGUIMath.ConvertToTexCoords(uv, tex.width, tex.height);
		        GUI.DrawTextureWithTexCoords(outerRect, tex, uv, true);
	        }
	        else
	        {
		        // NOTE: There is an issue in Unity that prevents it from clipping the drawn preview
		        // using BeginGroup/EndGroup, and there is no way to specify a UV rect... le'suq.
		        EditorGUI.DrawPreviewTexture(outerRect, tex, mat);
	        }

	        if (Selection.activeGameObject == null || Selection.gameObjects.Length == 1)
	        {
		        // Draw the border indicator lines
		        GUI.BeginGroup(outerRect);
		        {
			        tex = NGUIEditorTools.contrastTexture;
			        GUI.color = Color.white;

			        if (borderLeft > 0)
			        {
				        float x0 = (float) borderLeft / width * outerRect.width - 1;
				        DrawTiledTexture(new Rect(x0, 0f, 1f, outerRect.height), tex);
			        }

			        if (borderRight > 0)
			        {
				        float x1 = (float) (width - borderRight) / width * outerRect.width - 1;
				        DrawTiledTexture(new Rect(x1, 0f, 1f, outerRect.height), tex);
			        }

			        if (borderBottom > 0)
			        {
				        float y0 = (float) (height - borderBottom) / height * outerRect.height - 1;
				        DrawTiledTexture(new Rect(0f, y0, outerRect.width, 1f), tex);
			        }

			        if (borderTop > 0)
			        {
				        float y1 = (float) borderTop / height * outerRect.height - 1;
				        DrawTiledTexture(new Rect(0f, y1, outerRect.width, 1f), tex);
			        }
		        }
		        GUI.EndGroup();

		        // Draw the lines around the sprite
		        Handles.color = Color.black;
		        Handles.DrawLine(new Vector3(outerRect.xMin, outerRect.yMin), new Vector3(outerRect.xMin, outerRect.yMax));
		        Handles.DrawLine(new Vector3(outerRect.xMax, outerRect.yMin), new Vector3(outerRect.xMax, outerRect.yMax));
		        Handles.DrawLine(new Vector3(outerRect.xMin, outerRect.yMin), new Vector3(outerRect.xMax, outerRect.yMin));
		        Handles.DrawLine(new Vector3(outerRect.xMin, outerRect.yMax), new Vector3(outerRect.xMax, outerRect.yMax));

		        // Sprite size label
		        string text = string.Format("Sprite Size: {0}x{1}", Mathf.RoundToInt(width), Mathf.RoundToInt(height));
		        EditorGUI.DropShadowLabel(GUILayoutUtility.GetRect(Screen.width, 18f), text);
	        }
        }


        /// <summary>
        /// A string containing the Image details to be used as a overlay on the component Preview.
        /// </summary>
        /// <returns>
        /// The Image details.
        /// </returns>

        public override string GetInfoString()
        {
            UIExtensions.UITexture image = target as UIExtensions.UITexture;
            Texture2D sprite = image.texture;

            int x = (sprite != null) ? Mathf.RoundToInt(sprite.width) : 0;
            int y = (sprite != null) ? Mathf.RoundToInt(sprite.height) : 0;

            return string.Format("Image Size: {0}x{1}", x, y);
        }
    }
}