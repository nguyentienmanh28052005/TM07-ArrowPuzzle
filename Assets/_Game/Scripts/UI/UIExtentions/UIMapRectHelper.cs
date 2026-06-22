using System.Collections;
using mygame.sdk;
using UnityEngine;

public static class UIMapRectHelper
{
    public static void FitRectToMap(Bounds mapBounds, RectTransform rect, Canvas canvas, bool isScaleX = true)
    {
        Camera cam = canvas.worldCamera;
        if (cam == null) cam = Camera.main;
        Vector3[] worldCorners =
        {
            new Vector3(mapBounds.min.x, mapBounds.min.y, mapBounds.min.z),
            new Vector3(mapBounds.min.x, mapBounds.min.y, mapBounds.max.z),
            new Vector3(mapBounds.min.x, mapBounds.max.y, mapBounds.min.z),
            new Vector3(mapBounds.min.x, mapBounds.max.y, mapBounds.max.z),
            new Vector3(mapBounds.max.x, mapBounds.min.y, mapBounds.min.z),
            new Vector3(mapBounds.max.x, mapBounds.min.y, mapBounds.max.z),
            new Vector3(mapBounds.max.x, mapBounds.max.y, mapBounds.min.z),
            new Vector3(mapBounds.max.x, mapBounds.max.y, mapBounds.max.z),
        };

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        foreach (var wc in worldCorners)
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(CameraManager.Instance.mainCamera, wc);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect.parent as RectTransform, screenPos, cam, out var localPos);

            min = Vector2.Min(min, localPos);
            max = Vector2.Max(max, localPos);
        }

        rect.anchoredPosition = (min + max) * 0.5f;
        var deltaSize = max - min;
        if (!isScaleX)
        {
            var listCanvas = UIManager.Instance.CanvasScalers;
            if (listCanvas == null)
            {
                deltaSize.x = 1080;
            }
            else
            {
                deltaSize.x = listCanvas[0].referenceResolution.x;
            }
        }

        rect.sizeDelta = deltaSize;
    }
    
    public static Vector3 RectToVec(Camera mainCam, Canvas canvas, RectTransform rect, LayerMask layerMask)
    {
        Ray ray = mainCam.ScreenPointToRay(canvas.worldCamera.WorldToScreenPoint(rect.transform.position));
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
        {
            return hit.point;
        }

        return Vector3.zero;
    }
}