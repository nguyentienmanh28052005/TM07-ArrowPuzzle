//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2019 Tasharen Entertainment Inc
//-------------------------------------------------

using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Object = UnityEngine.Object;

/// <summary>
/// Unity doesn't keep the values of static variables after scripts change get recompiled. One way around this
/// is to store the references in EditorPrefs -- retrieve them at start, and save them whenever something changes.
/// </summary>

public class NGUISettings
{
	[DoNotObfuscateNGUI] public enum ColorMode
	{
		Orange,
		Green,
		Blue,
	}

#region Generic Get and Set methods
	/// <summary>
	/// Save the specified boolean value in settings.
	/// </summary>

	static public void SetBool (string name, bool val) { EditorPrefs.SetBool(name, val); }

	/// <summary>
	/// Save the specified integer value in settings.
	/// </summary>

	static public void SetInt (string name, int val) { EditorPrefs.SetInt(name, val); }

	/// <summary>
	/// Save the specified float value in settings.
	/// </summary>

	static public void SetFloat (string name, float val) { EditorPrefs.SetFloat(name, val); }

	/// <summary>
	/// Save the specified string value in settings.
	/// </summary>

	static public void SetString (string name, string val) { EditorPrefs.SetString(name, val); }

	/// <summary>
	/// Save the specified color value in settings.
	/// </summary>

	static public void SetColor (string name, Color c) { SetString(name, c.r + " " + c.g + " " + c.b + " " + c.a); }

	/// <summary>
	/// Save the specified enum value to settings.
	/// </summary>

	static public void SetEnum (string name, System.Enum val) { SetString(name, val.ToString()); }

	/// <summary>
	/// Save the specified object in settings.
	/// </summary>

	static public void Set (string name, Object obj)
	{
		if (obj == null)
		{
			EditorPrefs.DeleteKey(name);
		}
		else
		{
			if (obj != null)
			{
				string path = AssetDatabase.GetAssetPath(obj);

				if (!string.IsNullOrEmpty(path))
				{
					EditorPrefs.SetString(name, path);
				}
				else
				{
					EditorPrefs.SetString(name, obj.GetInstanceID().ToString());
				}
			}
			else EditorPrefs.DeleteKey(name);
		}
	}

	/// <summary>
	/// Get the previously saved boolean value.
	/// </summary>

	static public bool GetBool (string name, bool defaultValue) { return EditorPrefs.GetBool(name, defaultValue); }

	/// <summary>
	/// Get the previously saved integer value.
	/// </summary>

	static public int GetInt (string name, int defaultValue) { return EditorPrefs.GetInt(name, defaultValue); }

	/// <summary>
	/// Get the previously saved float value.
	/// </summary>

	static public float GetFloat (string name, float defaultValue) { return EditorPrefs.GetFloat(name, defaultValue); }

	/// <summary>
	/// Get the previously saved string value.
	/// </summary>

	static public string GetString (string name, string defaultValue) { return EditorPrefs.GetString(name, defaultValue); }

	/// <summary>
	/// Get a previously saved color value.
	/// </summary>

	static public Color GetColor (string name, Color c)
	{
		string strVal = GetString(name, c.r + " " + c.g + " " + c.b + " " + c.a);
		string[] parts = strVal.Split(' ');

		if (parts.Length == 4)
		{
			float.TryParse(parts[0], out c.r);
			float.TryParse(parts[1], out c.g);
			float.TryParse(parts[2], out c.b);
			float.TryParse(parts[3], out c.a);
		}
		return c;
	}

	/// <summary>
	/// Get a previously saved enum from settings.
	/// </summary>

	static public T GetEnum<T> (string name, T defaultValue)
	{
		string val = GetString(name, defaultValue.ToString());
		string[] names = System.Enum.GetNames(typeof(T));
		System.Array values = System.Enum.GetValues(typeof(T));

		for (int i = 0; i < names.Length; ++i)
		{
			if (names[i] == val)
				return (T)values.GetValue(i);
		}
		return defaultValue;
	}

	/// <summary>
	/// Get a previously saved object from settings.
	/// </summary>

	static public T Get<T> (string name, T defaultValue) where T : Object
	{
		string path = EditorPrefs.GetString(name);
		if (string.IsNullOrEmpty(path)) return null;

		T retVal = NGUIEditorTools.LoadAsset<T>(path);

		if (retVal == null)
		{
			int id;
			if (int.TryParse(path, out id))
				return EditorUtility.InstanceIDToObject(id) as T;
		}
		return retVal;
	}
#endregion

#region Convenience accessor properties

	static public bool minimalisticLook
	{
		get { return GetBool("NGUI Minimalistic", false); }
		set { SetBool("NGUI Minimalistic", value); }
	}
	static public string partialSprite
	{
		get { return GetString("NGUI Partial", null); }
		set { SetString("NGUI Partial", value); }
	}
	
	static public Color backgroundColor
	{
		get { return GetColor("NGUI BG Color", Color.black); }
		set { SetColor("NGUI BG Color", value); }
	}
	

	static public INGUIAtlas atlas
	{
		get
		{
			var atl = Get<NGUIAtlas>("NGUI Atlas", null);
			return atl;
		}
		set
		{
			Set("NGUI Atlas", value as Object);
		}
	}

	static public string selectedSprite
	{
		get { return GetString("NGUI Sprite", null); }
		set { SetString("NGUI Sprite", value); }
	}

	static public Action selectedSpriteChange;
	
	static public int atlasPadding
	{
		get { return GetInt("NGUI Padding", 1); }
		set { SetInt("NGUI Padding", value); }
	}

	static public bool atlasTrimming
	{
		get { return GetBool("NGUI Trim", true); }
		set { SetBool("NGUI Trim", value); }
	}

	static public bool atlasPMA
	{
		get { return GetBool("NGUI PMA", false); }
		set { SetBool("NGUI PMA", value); }
	}

	static public bool unityPacking
	{
		get { return GetBool("NGUI Atlas Packing", false); }
		set { SetBool("NGUI Atlas Packing", value); }
	}

	static public bool trueColorAtlas
	{
		get { return GetBool("NGUI Truecolor", true); }
		set { SetBool("NGUI Truecolor", value); }
	}

	static public bool autoUpgradeSprites
	{
		get { return GetBool("NGUI AutoUpgrade", false); }
		set { SetBool("NGUI AutoUpgrade", value); }
	}

	static public bool forceSquareAtlas
	{
		get { return GetBool("NGUI Square", false); }
		set { SetBool("NGUI Square", value); }
	}

	static public bool allow4096
	{
		get { return GetBool("NGUI 4096", true); }
		set { SetBool("NGUI 4096", value); }
	}

	static public string currentPath
	{
		get { return GetString("NGUI Path", "Assets/"); }
		set { SetString("NGUI Path", value); }
	}
#endregion
}
