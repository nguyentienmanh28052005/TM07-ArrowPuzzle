//-------------------------------------------------
//			  NGUI: Next-Gen UI kit
// Copyright © 2011-2019 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class DoNotObfuscateNGUI : Attribute { }

/// <summary>
/// Helper class containing generic functions used throughout the UI library.
/// </summary>

static public class NGUITools
{
	static bool mLoaded = false;
	static float mGlobalVolume = 1f;
	

	/// <summary>
	/// Helper function that returns the string name of the type.
	/// </summary>

	static public string GetTypeName<T> ()
	{
		string s = typeof(T).ToString();
		if (s.StartsWith("UI")) s = s.Substring(2);
		else if (s.StartsWith("UnityEngine.")) s = s.Substring(12);
		return s;
	}

	/// <summary>
	/// Convenience function that marks the specified object as dirty in the Unity Editor.
	/// </summary>

	static public void SetDirty (UnityEngine.Object obj, string undoName = "last change")
	{
#if UNITY_EDITOR
#if UNITY_2018_3_OR_NEWER
		if (obj)
		{
			UnityEditor.EditorUtility.SetDirty(obj);

			if (!UnityEditor.AssetDatabase.Contains(obj) && !Application.isPlaying)
			{
				if (obj is Component)
				{
					var component = (Component)obj;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
				}
				else if (!(obj is UnityEditor.EditorWindow || obj is ScriptableObject))
				{
					UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
				}
			}
		}
#else
		if (obj) UnityEditor.EditorUtility.SetDirty(obj);
#endif
#endif
	}


	/// <summary>
	/// Destroy the specified object immediately, unless not in the editor, in which case the regular Destroy is used instead.
	/// </summary>

	static public void DestroyImmediate (UnityEngine.Object obj)
	{
		if (obj != null)
		{
			if (Application.isEditor) UnityEngine.Object.DestroyImmediate(obj);
			else UnityEngine.Object.Destroy(obj);
		}
	}

	/// <summary>
	/// Pre-multiply shaders result in a black outline if this operation is done in the shader. It's better to do it outside.
	/// </summary>

	static public Color ApplyPMA (Color c)
	{
		if (c.a != 1f)
		{
			c.r *= c.a;
			c.g *= c.a;
			c.b *= c.a;
		}
		return c;
	}
}
