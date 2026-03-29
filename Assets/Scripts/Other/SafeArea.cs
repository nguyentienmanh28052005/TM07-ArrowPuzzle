using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);
    private Vector2 lastScreenSize = Vector2.zero;
    private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        Rect safeArea = Screen.safeArea;

        // Chỉ tính toán lại nếu kích thước màn hình, vùng an toàn, hoặc hướng xoay thay đổi
        if (safeArea != lastSafeArea || Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y || Screen.orientation != lastOrientation)
        {
            lastScreenSize = new Vector2(Screen.width, Screen.height);
            lastSafeArea = safeArea;
            lastOrientation = Screen.orientation;

            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        // Quy đổi tọa độ Pixel sang tỷ lệ Anchor (từ 0 đến 1)
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}