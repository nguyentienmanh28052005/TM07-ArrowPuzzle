//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2019 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;

/// <summary>
/// This script can be used to restrict camera rendering to a specific part of the screen by specifying the two corners.
/// </summary>

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class UIViewport : MonoBehaviour
{
	public Camera sourceCamera;
	public RectTransform topLeft;
	public RectTransform bottomRight;
	public float fullSize = 1f;

	Camera mCam;

	void Start ()
	{
		mCam = GetComponent<Camera>();
		if (sourceCamera == null) sourceCamera = GetComponentInParent<Canvas>().worldCamera;
	}

	void LateUpdate ()
	{
		if (topLeft != null && bottomRight != null)
		{
			if (topLeft.gameObject.activeInHierarchy)
			{
				Vector3 tl = RectTransformUtility.WorldToScreenPoint(sourceCamera ,topLeft.position);
				Vector3 br = RectTransformUtility.WorldToScreenPoint(sourceCamera, bottomRight.position);

				Rect rect = new Rect(tl.x / Screen.width, br.y / Screen.height,
					(br.x - tl.x) / Screen.width, (tl.y - br.y) / Screen.height);

				float size = fullSize * rect.height;

				if (rect != mCam.rect) mCam.rect = rect;
				if (mCam.orthographicSize != size) mCam.orthographicSize = size;
				// mCam.enabled = true;
			}
			else
			{
				// mCam.enabled = false;
			}
		}
	}
}
