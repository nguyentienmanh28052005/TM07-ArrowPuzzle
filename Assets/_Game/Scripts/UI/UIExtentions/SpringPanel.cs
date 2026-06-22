//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2019 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;

/// <summary>
/// Similar to SpringPosition, but also moves the panel's clipping. Works in local coordinates.
/// </summary>

public class SpringPanel : MonoBehaviour
{
	/// <summary>
	/// Target position to spring the panel to.
	/// </summary>

	public Vector3 target = Vector3.zero;

	/// <summary>
	/// Strength of the spring. The higher the value, the faster the movement.
	/// </summary>

	public float strength = 10f;

	public delegate void OnFinished ();

	/// <summary>
	/// Delegate function to call when the operation finishes.
	/// </summary>

	public OnFinished onFinished;

	[System.NonSerialized] Transform mTrans;
	[System.NonSerialized] float mDelta = 0f;

	/// <summary>
	/// Cache the transform.
	/// </summary>

	void Start ()
	{
		mTrans = transform;
	}

	/// <summary>
	/// Advance toward the target position.
	/// </summary>

	void Update () { AdvanceTowardsPosition(); }

	/// <summary>
	/// Advance toward the target position.
	/// </summary>

	protected virtual void AdvanceTowardsPosition ()
	{
		mDelta += Time.unscaledDeltaTime;

		var trigger = false;
		var before = mTrans.localPosition;
		var after = SpringLerp(before, target, strength, mDelta);

		if ((before - target).sqrMagnitude < 0.01f)
		{
			after = target;
			enabled = false;
			trigger = true;
			mDelta = 0f;
		}
		else
		{
			after.x = Mathf.Round(after.x);
			after.y = Mathf.Round(after.y);
			after.z = Mathf.Round(after.z);

			if ((after - before).sqrMagnitude < 0.01f) return;
			else mDelta = 0f;
		}

		mTrans.localPosition = after;
	}

	/// <summary>
	/// Start the tweening process.
	/// </summary>

	static public SpringPanel Begin (GameObject go, Vector3 pos, float strength)
	{
		var sp = go.GetComponent<SpringPanel>();
		if (sp == null) sp = go.AddComponent<SpringPanel>();
		sp.target = pos;
		sp.strength = strength;
		sp.onFinished = null;
		sp.enabled = true;
		return sp;
	}

	/// <summary>
	/// Stop the tweening process.
	/// </summary>

	static public SpringPanel Stop (GameObject go)
	{
		var sp = go.GetComponent<SpringPanel>();

		if (sp != null && sp.enabled)
		{
			if (sp.onFinished != null) sp.onFinished();
			sp.enabled = false;
		}
		return sp;
	}
	
	static public Vector3 SpringLerp (Vector3 from, Vector3 to, float strength, float deltaTime)
	{
		return Vector3.Lerp(from, to, SpringLerp(strength, deltaTime));
	}
	
	static public float SpringLerp (float strength, float deltaTime)
	{
		if (deltaTime > 1f) deltaTime = 1f;
		int ms = Mathf.RoundToInt(deltaTime * 1000f);
		deltaTime = 0.001f * strength;
		float cumulative = 0f;
		for (int i = 0; i < ms; ++i) cumulative = Mathf.Lerp(cumulative, 1f, deltaTime);
		return cumulative;
	}
}
