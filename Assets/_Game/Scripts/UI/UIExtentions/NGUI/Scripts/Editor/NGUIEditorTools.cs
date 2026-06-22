//-------------------------------------------------
//			  NGUI: Next-Gen UI kit
// Copyright © 2011-2019 Tasharen Entertainment Inc
//-------------------------------------------------

using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tools for the editor
/// </summary>

static public class NGUIEditorTools
{
	private static Texture2D mBackdropTex;
	private static Texture2D mContrastTex;
	private static Texture2D mGradientTex;
	private static Object mPrevious;

	/// <summary>
	/// Returns a blank usable 1x1 white texture.
	/// </summary>

	static public Texture2D blankTexture
	{
		get
		{
			return EditorGUIUtility.whiteTexture;
		}
	}

	/// <summary>
	/// Returns a usable texture that looks like a dark checker board.
	/// </summary>

	static public Texture2D backdropTexture
	{
		get
		{
			if (mBackdropTex == null) mBackdropTex = CreateCheckerTex(
				new Color(0.1f, 0.1f, 0.1f, 0.5f),
				new Color(0.2f, 0.2f, 0.2f, 0.5f));
			return mBackdropTex;
		}
	}

	/// <summary>
	/// Returns a usable texture that looks like a high-contrast checker board.
	/// </summary>

	static public Texture2D contrastTexture
	{
		get
		{
			if (mContrastTex == null) mContrastTex = CreateCheckerTex(
				new Color(0f, 0f, 0f, 0.5f),
				new Color(1f, 1f, 1f, 0.5f));
			return mContrastTex;
		}
	}
	/// <summary>
	/// Create a checker-background texture
	/// </summary>

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
	/// <summary>
	/// Draws the tiled texture. Like GUI.DrawTexture() but tiled instead of stretched.
	/// </summary>

	static public void DrawTiledTexture (Rect rect, Texture tex)
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
	/// Draw a single-pixel outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect, Color color)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = blankTexture;
			GUI.color = color;
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), tex);
			GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, 1f, rect.height), tex);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
			GUI.color = Color.white;
		}
	}


	/// <summary>
	/// Draw a visible separator in addition to adding some padding.
	/// </summary>

	static public void DrawSeparator ()
	{
		GUILayout.Space(12f);

		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = blankTexture;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
			GUI.color = Color.white;
		}
	}

	/// <summary>
	/// Returns 'true' if the specified object is a prefab.
	/// </summary>

	static public bool IsPrefab (GameObject go)
	{
#if UNITY_2018_3_OR_NEWER
		return go != null && PrefabUtility.GetPrefabAssetType(go) == PrefabAssetType.Regular;
#else
		return go != null && PrefabUtility.GetPrefabType(go) == PrefabType.Prefab;
#endif
	}


	/// <summary>
	/// Change the import settings of the specified texture asset, making it readable.
	/// </summary>

	static public bool MakeTextureReadable (string path, bool force)
	{
		if (string.IsNullOrEmpty(path)) return false;
		TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
		if (ti == null) return false;

		TextureImporterSettings settings = new TextureImporterSettings();
		ti.ReadTextureSettings(settings);

		if (force || !settings.readable || settings.npotScale != TextureImporterNPOTScale.None)
		{
			settings.readable = true;
#if !UNITY_4_7 && !UNITY_5_3 && !UNITY_5_4
			if (NGUISettings.trueColorAtlas)
			{
				var platform = ti.GetDefaultPlatformTextureSettings();
				platform.format = TextureImporterFormat.RGBA32;
			}
#else
			if (NGUISettings.trueColorAtlas) settings.textureFormat = TextureImporterFormat.AutomaticTruecolor;
#endif
			settings.npotScale = TextureImporterNPOTScale.None;
			ti.SetTextureSettings(settings);
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
		}
		return true;
	}

	/// <summary>
	/// Change the import settings of the specified texture asset, making it suitable to be used as a texture atlas.
	/// </summary>

	private static bool MakeTextureAnAtlas (string path, bool force, bool alphaTransparency)
	{
		if (string.IsNullOrEmpty(path)) return false;
		var ti = AssetImporter.GetAtPath(path) as TextureImporter;
		if (ti == null) return false;

		var settings = new TextureImporterSettings();
		ti.ReadTextureSettings(settings);

		if (force || settings.readable ||
#if UNITY_5_5_OR_NEWER
			ti.maxTextureSize < 4096 ||
			(NGUISettings.trueColorAtlas && ti.textureCompression != TextureImporterCompression.Uncompressed) ||
#else
			settings.maxTextureSize < 4096 ||
#endif
			settings.wrapMode != TextureWrapMode.Clamp ||
			settings.npotScale != TextureImporterNPOTScale.ToNearest)
		{
			settings.readable = false;
#if !UNITY_4_7 && !UNITY_5_3 && !UNITY_5_4
			ti.maxTextureSize = 4096;
#else
			settings.maxTextureSize = 4096;
#endif
			settings.wrapMode = TextureWrapMode.Clamp;
			settings.npotScale = TextureImporterNPOTScale.ToNearest;

			if (NGUISettings.trueColorAtlas)
			{
#if UNITY_5_5_OR_NEWER
				ti.textureCompression = TextureImporterCompression.Uncompressed;
#else
				settings.textureFormat = TextureImporterFormat.ARGB32;
#endif
				settings.filterMode = FilterMode.Trilinear;
			}

			settings.aniso = 4;
			settings.alphaIsTransparency = alphaTransparency;
			ti.SetTextureSettings(settings);
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
		}
		return true;
	}

	/// <summary>
	/// Fix the import settings for the specified texture, re-importing it if necessary.
	/// </summary>

	static public Texture2D ImportTexture (string path, bool forInput, bool force, bool alphaTransparency)
	{
		if (!string.IsNullOrEmpty(path))
		{
			if (forInput) { if (!MakeTextureReadable(path, force)) return null; }
			else if (!MakeTextureAnAtlas(path, force, alphaTransparency)) return null;
			//return AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;

			Texture2D tex = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
			return tex;
		}
		return null;
	}

	/// <summary>
	/// Fix the import settings for the specified texture, re-importing it if necessary.
	/// </summary>

	static public Texture2D ImportTexture (Texture tex, bool forInput, bool force, bool alphaTransparency)
	{
		if (tex != null)
		{
			string path = AssetDatabase.GetAssetPath(tex.GetInstanceID());
			return ImportTexture(path, forInput, force, alphaTransparency);
		}
		return null;
	}

	/// <summary>
	/// Figures out the saveable filename for the texture of the specified atlas.
	/// </summary>

	static public string GetSaveableTexturePath (INGUIAtlas atlas)
	{
		if (atlas == null) return "";
		return GetSaveableTexturePath(atlas as Object, atlas.texture as Texture2D);
	}

	/// <summary>
	/// Figures out the saveable filename for the texture of the specified atlas.
	/// </summary>

	static public string GetSaveableTexturePath (Object obj, Texture2D texture)
	{
		// Path where the texture atlas will be saved
		string path = "";

		if (texture != null)
		{
			path = AssetDatabase.GetAssetPath(texture.GetInstanceID());

			if (!string.IsNullOrEmpty(path))
			{
				int dot = path.LastIndexOf('.');
				return path.Substring(0, dot) + ".png";
			}
		}

		// No texture to use -- figure out a name using the atlas
		path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
		path = string.IsNullOrEmpty(path) ? "Assets/" + obj.name + ".png" : path.Replace(".asset", ".png");
		return path;
	}

	/// <summary>
	/// Struct type for the integer vector field below.
	/// </summary>

	public struct IntVector
	{
		public int x;
		public int y;
	}

	/// <summary>
	/// Integer vector field.
	/// </summary>

	static public IntVector IntPair (string prefix, string leftCaption, string rightCaption, int x, int y)
	{
		GUILayout.BeginHorizontal();

		if (string.IsNullOrEmpty(prefix))
		{
			GUILayout.Space(82f);
		}
		else
		{
			GUILayout.Label(prefix, GUILayout.Width(74f));
		}

		NGUIEditorTools.SetLabelWidth(48f);

		IntVector retVal;
		retVal.x = EditorGUILayout.IntField(leftCaption, x, GUILayout.MinWidth(30f));
		retVal.y = EditorGUILayout.IntField(rightCaption, y, GUILayout.MinWidth(30f));

		NGUIEditorTools.SetLabelWidth(80f);

		GUILayout.EndHorizontal();
		return retVal;
	}

	static public bool DrawPrefixButton (string text)
	{
		return GUILayout.Button(text, "DropDown", GUILayout.Width(76f));
	}

	/// <summary>
	/// Draw a sprite preview.
	/// </summary>

	static public void DrawSprite (Texture2D tex, Rect rect, UISpriteData sprite, Color color)
	{
		DrawSprite(tex, rect, sprite, color, null);
	}

	/// <summary>
	/// Draw a sprite preview.
	/// </summary>

	static public void DrawSprite (Texture2D tex, Rect drawRect, UISpriteData sprite, Color color, Material mat)
	{
		if (!tex || sprite == null) return;
		DrawSprite(tex, drawRect, color, mat, sprite.x, sprite.y, sprite.width, sprite.height,
			sprite.borderLeft, sprite.borderBottom, sprite.borderRight, sprite.borderTop);
	}

	/// <summary>
	/// Draw a sprite preview.
	/// </summary>

	static public void DrawSprite (Texture2D tex, Rect drawRect, Color color, Material mat,
		int x, int y, int width, int height, int borderLeft, int borderBottom, int borderRight, int borderTop)
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
		NGUIEditorTools.DrawTiledTexture(outerRect, NGUIEditorTools.backdropTexture);

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
			UnityEditor.EditorGUI.DrawPreviewTexture(outerRect, tex, mat);
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
					float x0 = (float)borderLeft / width * outerRect.width - 1;
					NGUIEditorTools.DrawTiledTexture(new Rect(x0, 0f, 1f, outerRect.height), tex);
				}

				if (borderRight > 0)
				{
					float x1 = (float)(width - borderRight) / width * outerRect.width - 1;
					NGUIEditorTools.DrawTiledTexture(new Rect(x1, 0f, 1f, outerRect.height), tex);
				}

				if (borderBottom > 0)
				{
					float y0 = (float)(height - borderBottom) / height * outerRect.height - 1;
					NGUIEditorTools.DrawTiledTexture(new Rect(0f, y0, outerRect.width, 1f), tex);
				}

				if (borderTop > 0)
				{
					float y1 = (float)borderTop / height * outerRect.height - 1;
					NGUIEditorTools.DrawTiledTexture(new Rect(0f, y1, outerRect.width, 1f), tex);
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


	private static string mEditedName = null;
	private static string mLastSprite = null;

	/// <summary>
	/// Select the specified game object and remember what was selected before.
	/// </summary>

	static public void Select (Object obj)
	{
		mPrevious = Selection.activeGameObject;
		Selection.activeObject = obj;
#if UNITY_2018_3_OR_NEWER
		OpenAsset(obj as GameObject);
#endif
	}

#if UNITY_2018_3_OR_NEWER
	// Contributed by B9 of https://discord.gg/tasharen
	static void OpenAsset (GameObject go)
	{
		// Supporting opening of prefabs in Play mode is a bit of a can of worms if target might have ExecuteInEditMode
		if (!go || Application.isPlaying) return;

		// No point continuing if we're dealing with a traditional main stage object
		bool partOfPrefabInstance = PrefabUtility.IsPartOfPrefabInstance (go);
		bool partOfPrefabAsset = PrefabUtility.IsPartOfPrefabAsset (go);
		if (!partOfPrefabInstance && !partOfPrefabAsset) return;

		var asset = partOfPrefabInstance ? PrefabUtility.GetCorrespondingObjectFromSource (go) : go;
		string path = AssetDatabase.GetAssetPath (asset);

		// var assetRoot = PrefabUtility.LoadPrefabContents (path);
		// This API call above loads the prefab to an invisible scene and allows direct inspection without leaving main stage.
		// Except it would require us to manage saving and disposing that temporary prefab stage scene and root and that's very hard
		// when the user still has full access to main stage hierarchy and can select anything again, leaving us with no way
		// to detect when cleanup is required. So, for now, I'd just load the selected asset exclusively and take over Editor view.

		// Last second check to confirm we're definitely targeting an in-project prefab asset and not some random type like an image
		if (PrefabUtility.IsPartOfAnyPrefab (asset)) AssetDatabase.OpenAsset (AssetDatabase.LoadAssetAtPath (path, asset.GetType ()));
	}
#endif

	/// <summary>
	/// Select the previous game object.
	/// </summary>

	static public void SelectPrevious ()
	{
		if (mPrevious != null)
		{
			Selection.activeObject = mPrevious;
#if UNITY_2018_3_OR_NEWER
			OpenAsset(mPrevious as GameObject);
#endif
			mPrevious = null;
		}
	}

	/// <summary>
	/// Previously selected game object.
	/// </summary>

	static public Object previousSelection { get { return mPrevious; } }

	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public bool DrawHeader (string text)
	{
		return DrawHeader(text, text, false, NGUISettings.minimalisticLook);
	}


	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public bool DrawHeader (string text, bool detailed)
	{
		return DrawHeader(text, text, detailed, !detailed);
	}

	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public bool DrawHeader (string text, string key, bool forceOn, bool minimalistic)
	{
		bool state = EditorPrefs.GetBool(key, true);

		if (!minimalistic) GUILayout.Space(3f);
		if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
		GUILayout.BeginHorizontal();
		GUI.changed = false;

		if (minimalistic)
		{
			if (state) text = "\u25BC" + (char)0x200a + text;
			else text = "\u25BA" + (char)0x200a + text;

			GUILayout.BeginHorizontal();
			GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
			if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
			GUI.contentColor = Color.white;
			GUILayout.EndHorizontal();
		}
		else
		{
			text = "<b><size=11>" + text + "</size></b>";
			if (state) text = "\u25BC " + text;
			else text = "\u25BA " + text;
			if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
		}

		if (GUI.changed) EditorPrefs.SetBool(key, state);

		if (!minimalistic) GUILayout.Space(2f);
		GUILayout.EndHorizontal();
		GUI.backgroundColor = Color.white;
		if (!forceOn && !state) GUILayout.Space(3f);
		return state;
	}

	/// <summary>
	/// Begin drawing the content area.
	/// </summary>

	static public void BeginContents ()
	{
		BeginContents(NGUISettings.minimalisticLook);
	}

	private static bool mEndHorizontal = false;

#if UNITY_4_7 || UNITY_5_5 || UNITY_5_6
	static public string textArea = "AS TextArea";
#else
	static public string textArea = "TextArea";
#endif

	/// <summary>
	/// Begin drawing the content area.
	/// </summary>

	static public void BeginContents (bool minimalistic)
	{
		if (!minimalistic)
		{
			mEndHorizontal = true;
			GUILayout.BeginHorizontal();
			EditorGUILayout.BeginHorizontal(textArea, GUILayout.MinHeight(10f));
		}
		else
		{
			mEndHorizontal = false;
			EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
			GUILayout.Space(10f);
		}
		GUILayout.BeginVertical();
		GUILayout.Space(2f);
	}

	/// <summary>
	/// End drawing the content area.
	/// </summary>

	static public void EndContents ()
	{
		GUILayout.Space(3f);
		GUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();

		if (mEndHorizontal)
		{
			GUILayout.Space(3f);
			GUILayout.EndHorizontal();
		}

		GUILayout.Space(3f);
	}

	static public void RepaintSprites ()
	{
		if (UIAtlasMaker.instance != null)
			UIAtlasMaker.instance.Repaint();

		if (SpriteSelector.instance != null)
			SpriteSelector.instance.Repaint();
	}
	
	
	static public void SelectSprite (string spriteName)
	{
		if (NGUISettings.atlas != null)
		{
			NGUISettings.selectedSprite = spriteName;
			NGUIEditorTools.Select(NGUISettings.atlas as Object);
			RepaintSprites();
		}
	}
	
	/// <summary>
	/// Unity 4.3 changed the way LookLikeControls works.
	/// </summary>

	static public void SetLabelWidth (float width)
	{
		EditorGUIUtility.labelWidth = width;
	}

	/// <summary>
	/// Create an undo point for the specified object.
	/// </summary>

	static public void RegisterUndo (string name, Object obj) { if (obj != null) UnityEditor.Undo.RecordObject(obj, name); }



/// <summary>
	/// Convenience function that displays a list of sprites and returns the selected value.
	/// </summary>

	static public void DrawAdvancedSpriteField (INGUIAtlas atlas, string spriteName, SpriteSelector.Callback callback, bool editable, params GUILayoutOption[] options)
	{
		if (atlas == null) return;

		// Give the user a warning if there are no sprites in the atlas
		if (atlas.spriteList.Count == 0)
		{
			EditorGUILayout.HelpBox("No sprites found", MessageType.Warning);
			return;
		}

		// Sprite selection drop-down list
		GUILayout.BeginHorizontal();
		{
			if (NGUIEditorTools.DrawPrefixButton("Sprite"))
			{
				NGUISettings.atlas = atlas;
				NGUISettings.selectedSprite = spriteName;
				SpriteSelector.Show(callback);
			}

			if (editable)
			{
				if (!string.Equals(spriteName, mLastSprite))
				{
					mLastSprite = spriteName;
					mEditedName = null;
				}

				string newName = GUILayout.TextField(string.IsNullOrEmpty(mEditedName) ? spriteName : mEditedName);

				if (newName != spriteName)
				{
					mEditedName = newName;

					if (GUILayout.Button("Rename", GUILayout.Width(60f)))
					{
						var sprite = atlas.GetSprite(spriteName);

						if (sprite != null)
						{
							RegisterUndo("Edit Sprite Name", atlas as Object);
							sprite.name = newName;
							
							mLastSprite = newName;
							spriteName = newName;
							mEditedName = null;

							NGUITools.SetDirty(atlas as Object, "Edit Sprite Name");
							NGUISettings.atlas = atlas;
							NGUISettings.selectedSprite = spriteName;
						}
					}
				}
			}
			else
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(spriteName, "HelpBox", GUILayout.Height(18f));
				NGUIEditorTools.DrawPadding();
				GUILayout.EndHorizontal();

				if (GUILayout.Button("Edit", GUILayout.Width(40f)))
				{
					NGUISettings.atlas = atlas;
					NGUISettings.selectedSprite = spriteName;
					Select(atlas as UnityEngine.Object);
				}
			}
		}
		GUILayout.EndHorizontal();
	}

	/// <summary>
	/// Load the asset at the specified path.
	/// </summary>

	static public Object LoadAsset (string path)
	{
		if (string.IsNullOrEmpty(path)) return null;
		return AssetDatabase.LoadMainAssetAtPath(path);
	}

	/// <summary>
	/// Convenience function to load an asset of specified type, given the full path to it.
	/// </summary>

	static public T LoadAsset<T> (string path) where T : Object
	{
		Object obj = LoadAsset(path);
		if (obj == null) return null;

		T val = obj as T;
		if (val != null) return val;

		if (typeof(T).IsSubclassOf(typeof(Component)))
		{
			if (obj.GetType() == typeof(GameObject))
			{
				GameObject go = obj as GameObject;
				return go.GetComponent(typeof(T)) as T;
			}
		}
		return null;
	}


	private static MethodInfo s_GetInstanceIDFromGUID;


	/// <summary>
	/// Add a border around the specified color buffer with the width and height of a single pixel all around.
	/// The returned color buffer will have its width and height increased by 2.
	/// </summary>

	static public Color32[] AddBorder (Color32[] colors, int width, int height)
	{
		int w2 = width + 2;
		int h2 = height + 2;

		Color32[] c2 = new Color32[w2 * h2];

		for (int y2 = 0; y2 < h2; ++y2)
		{
			int y1 = NGUIMath.ClampIndex(y2 - 1, height);

			for (int x2 = 0; x2 < w2; ++x2)
			{
				int x1 = NGUIMath.ClampIndex(x2 - 1, width);
				int i2 = x2 + y2 * w2;
				c2[i2] = colors[x1 + y1 * width];

				if (x2 == 0 || x2 + 1 == w2 || y2 == 0 || y2 + 1 == h2)
					c2[i2].a = 0;
			}
		}
		return c2;
	}

	/// <summary>
	/// Add a soft shadow to the specified color buffer.
	/// The buffer must have some padding around the edges in order for this to work properly.
	/// </summary>

	static public void AddShadow (Color32[] colors, int width, int height, Color shadow)
	{
		Color sh = shadow;
		sh.a = 1f;

		for (int y2 = 0; y2 < height; ++y2)
		{
			for (int x2 = 0; x2 < width; ++x2)
			{
				int index = x2 + y2 * width;
				Color32 uc = colors[index];
				if (uc.a == 255) continue;

				Color original = uc;
				float val = original.a;
				int count = 1;
				float div1 = 1f / 255f;
				float div2 = 2f / 255f;
				float div3 = 3f / 255f;

				// Left
				if (x2 != 0)
				{
					val += colors[x2 - 1 + y2 * width].a * div1;
					count += 1;
				}

				// Top
				if (y2 + 1 != height)
				{
					val += colors[x2 + (y2 + 1) * width].a * div2;
					count += 2;
				}

				// Top-left
				if (x2 != 0 && y2 + 1 != height)
				{
					val += colors[x2 - 1 + (y2 + 1) * width].a * div3;
					count += 3;
				}

				val /= count;

				Color c = Color.Lerp(original, sh, shadow.a * val);
				colors[index] = Color.Lerp(c, original, original.a);
			}
		}
	}

	/// <summary>
	/// Add a visual depth effect to the specified color buffer.
	/// The buffer must have some padding around the edges in order for this to work properly.
	/// </summary>

	static public void AddDepth (Color32[] colors, int width, int height, Color shadow)
	{
		Color sh = shadow;
		sh.a = 1f;

		for (int y2 = 0; y2 < height; ++y2)
		{
			for (int x2 = 0; x2 < width; ++x2)
			{
				int index = x2 + y2 * width;
				Color32 uc = colors[index];
				if (uc.a == 255) continue;

				Color original = uc;
				float val = original.a * 4f;
				int count = 4;
				float div1 = 1f / 255f;
				float div2 = 2f / 255f;

				if (x2 != 0)
				{
					val += colors[x2 - 1 + y2 * width].a * div2;
					count += 2;
				}

				if (x2 + 1 != width)
				{
					val += colors[x2 + 1 + y2 * width].a * div2;
					count += 2;
				}

				if (y2 != 0)
				{
					val += colors[x2 + (y2 - 1) * width].a * div2;
					count += 2;
				}

				if (y2 + 1 != height)
				{
					val += colors[x2 + (y2 + 1) * width].a * div2;
					count += 2;
				}

				if (x2 != 0 && y2 != 0)
				{
					val += colors[x2 - 1 + (y2 - 1) * width].a * div1;
					++count;
				}

				if (x2 != 0 && y2 + 1 != height)
				{
					val += colors[x2 - 1 + (y2 + 1) * width].a * div1;
					++count;
				}

				if (x2 + 1 != width && y2 != 0)
				{
					val += colors[x2 + 1 + (y2 - 1) * width].a * div1;
					++count;
				}

				if (x2 + 1 != width && y2 + 1 != height)
				{
					val += colors[x2 + 1 + (y2 + 1) * width].a * div1;
					++count;
				}

				val /= count;

				Color c = Color.Lerp(original, sh, shadow.a * val);
				colors[index] = Color.Lerp(c, original, original.a);
			}
		}
	}

	/// <summary>
	/// Draw 18 pixel padding on the right-hand side. Used to align fields.
	/// </summary>

	static public void DrawPadding ()
	{
		if (!NGUISettings.minimalisticLook)
			GUILayout.Space(18f);
	}
}