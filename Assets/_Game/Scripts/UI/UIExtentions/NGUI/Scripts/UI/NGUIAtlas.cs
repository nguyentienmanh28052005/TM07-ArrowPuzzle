//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2019 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Generic interface for the atlas class, making it possible to support both the prefab-based UIAtlas and scriptable object-based NGUIAtlas.
/// </summary>

public interface INGUIAtlas
{
	/// <summary>
	/// List of sprites within the atlas.
	/// </summary>

	List<UISpriteData> spriteList { get; set; }

	/// <summary>
	/// Texture used by the atlas.
	/// </summary>

	Texture texture { get; set; }

	/// <summary>
	/// Pixel size is a multiplier applied to widgets dimensions when performing MakePixelPerfect() pixel correction.
	/// Most obvious use would be on retina screen displays. The resolution doubles, but with UIRoot staying the same
	/// for layout purposes, you can still get extra sharpness by switching to an HD atlas that has pixel size set to 0.5.
	/// </summary>

	float pixelSize { get; set; }

	/// <summary>
	/// Convenience function that retrieves a sprite by name.
	/// </summary>

	UISpriteData GetSprite (string name);

	/// <summary>
	/// Convenience function that retrieves a list of all sprite names.
	/// </summary>

	BetterList<string> GetListOfSprites ();

	/// <summary>
	/// Convenience function that retrieves a list of all sprite names that contain the specified phrase.
	/// </summary>

	BetterList<string> GetListOfSprites (string match);

	/// <summary>
	/// Mark all widgets associated with this atlas as having changed.
	/// </summary>

	void MarkAsChanged ();

	/// <summary>
	/// Sort the list of sprites within the atlas, making them alphabetical.
	/// </summary>

	void SortAlphabetically ();
}

/// <summary>
/// NGUI Atlas contains a collection of sprites inside one large texture atlas. It's saved as a ScriptableObject.
/// </summary>

public class NGUIAtlas : ScriptableObject, INGUIAtlas
{
	// List of all sprites inside the atlas. Name is kept only for backwards compatibility, it used to be public.
	[HideInInspector][SerializeField] List<UISpriteData> mSprites = new List<UISpriteData>();
	
	[HideInInspector][SerializeField] Texture mTexture;

	// Size in pixels for the sake of MakePixelPerfect functions.
	[HideInInspector][SerializeField] float mPixelSize = 1f;

	// Dictionary lookup to speed up sprite retrieval at run-time
	[System.NonSerialized] Dictionary<string, int> mSpriteIndices = new Dictionary<string, int>();

	/// <summary>
	/// List of sprites within the atlas.
	/// </summary>

	public List<UISpriteData> spriteList
	{
		get
		{
			return mSprites;
		}
		set
		{
			 mSprites = value;
		}
	}

	/// <summary>
	/// Texture used by the atlas.
	/// </summary>

	public Texture texture
	{
		get { return mTexture; }
		set { mTexture = value; }
	}

	/// <summary>
	/// Pixel size is a multiplier applied to widgets dimensions when performing MakePixelPerfect() pixel correction.
	/// Most obvious use would be on retina screen displays. The resolution doubles, but with UIRoot staying the same
	/// for layout purposes, you can still get extra sharpness by switching to an HD atlas that has pixel size set to 0.5.
	/// </summary>

	public float pixelSize
	{
		get
		{
			return mPixelSize;
		}
		set
		{
			float val = Mathf.Clamp(value, 0.25f, 4f);

			if (mPixelSize != val)
			{
				mPixelSize = val;
				MarkAsChanged();
			}
		}
	}

	/// <summary>
	/// Setting a replacement atlas value will cause everything using this atlas to use the replacement atlas instead.
	/// Suggested use: set up all your widgets to use a dummy atlas that points to the real atlas. Switching that atlas
	/// to another one (for example an HD atlas) is then a simple matter of setting this field on your dummy atlas.
	/// </summary>

	/// <summary>
	/// Convenience function that retrieves a sprite by name.
	/// </summary>

	public UISpriteData GetSprite (string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			if (mSprites.Count == 0) return null;

			// O(1) lookup via a dictionary
#if UNITY_EDITOR
			if (Application.isPlaying)
#endif
			{
				// The number of indices differs from the sprite list? Rebuild the indices.
				if (mSpriteIndices.Count != mSprites.Count)
					MarkSpriteListAsChanged();

				int index;
				if (mSpriteIndices.TryGetValue(name, out index))
				{
					// If the sprite is present, return it as-is
					if (index > -1 && index < mSprites.Count) return mSprites[index];

					// The sprite index was out of range -- perhaps the sprite was removed? Rebuild the indices.
					MarkSpriteListAsChanged();

					// Try to look up the index again
					return mSpriteIndices.TryGetValue(name, out index) ? mSprites[index] : null;
				}
			}

			// Sequential O(N) lookup.
			for (int i = 0, imax = mSprites.Count; i < imax; ++i)
			{
				UISpriteData s = mSprites[i];

				// string.Equals doesn't seem to work with Flash export
				if (!string.IsNullOrEmpty(s.name) && name == s.name)
				{
#if UNITY_EDITOR
					if (!Application.isPlaying) return s;
#endif
					// If this point was reached then the sprite is present in the non-indexed list,
					// so the sprite indices should be updated.
					MarkSpriteListAsChanged();
					return s;
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Rebuild the sprite indices. Call this after modifying the spriteList at run time.
	/// </summary>

	public void MarkSpriteListAsChanged ()
	{
#if UNITY_EDITOR
		if (Application.isPlaying)
#endif
		{
			mSpriteIndices.Clear();
			for (int i = 0, imax = mSprites.Count; i < imax; ++i)
				mSpriteIndices[mSprites[i].name] = i;
		}
	}

	/// <summary>
	/// Sort the list of sprites within the atlas, making them alphabetical.
	/// </summary>

	public void SortAlphabetically ()
	{
		mSprites.Sort(delegate(UISpriteData s1, UISpriteData s2) { return s1.name.CompareTo(s2.name); });
#if UNITY_EDITOR
		NGUITools.SetDirty(this);
#endif
	}

	/// <summary>
	/// Convenience function that retrieves a list of all sprite names.
	/// </summary>

	public BetterList<string> GetListOfSprites ()
	{
		var list = new BetterList<string>();

		for (int i = 0, imax = mSprites.Count; i < imax; ++i)
		{
			UISpriteData s = mSprites[i];
			if (s != null && !string.IsNullOrEmpty(s.name)) list.Add(s.name);
		}
		return list;
	}

	/// <summary>
	/// Convenience function that retrieves a list of all sprite names that contain the specified phrase.
	/// </summary>

	public BetterList<string> GetListOfSprites (string match)
	{
		if (string.IsNullOrEmpty(match)) return GetListOfSprites();

		var list = new BetterList<string>();

		// First try to find an exact match
		for (int i = 0, imax = mSprites.Count; i < imax; ++i)
		{
			var s = mSprites[i];

			if (s != null && !string.IsNullOrEmpty(s.name) && string.Equals(match, s.name, StringComparison.OrdinalIgnoreCase))
			{
				list.Add(s.name);
				return list;
			}
		}

		// No exact match found? Split up the search into space-separated components.
		var keywords = match.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < keywords.Length; ++i) keywords[i] = keywords[i].ToLower();

		// Try to find all sprites where all keywords are present
		for (int i = 0, imax = mSprites.Count; i < imax; ++i)
		{
			var s = mSprites[i];

			if (s != null && !string.IsNullOrEmpty(s.name))
			{
				var tl = s.name.ToLower();
				var matches = 0;

				for (int b = 0; b < keywords.Length; ++b)
				{
					if (tl.Contains(keywords[b])) ++matches;
				}
				if (matches == keywords.Length) list.Add(s.name);
			}
		}
		return list;
	}

	/// <summary>
	/// Mark all widgets associated with this atlas as having changed.
	/// </summary>

	public void MarkAsChanged ()
	{
#if UNITY_EDITOR
		NGUITools.SetDirty(this);
#endif
	}
}
