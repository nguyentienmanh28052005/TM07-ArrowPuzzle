using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Hỗ trợ xử lý drag khi lồng các ScrollRect vào nhau (ví dụ: ScrollRect con nằm ngang lồng trong ScrollRect cha nằm dọc).
/// Gắn script này vào GameObject chứa ScrollRect con.
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class NestedScrollRectHelper : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler, IScrollHandler
{
    private ScrollRect parentScrollRect;
    private ScrollRect childScrollRect;
    private void Awake()
    {
        childScrollRect = GetComponent<ScrollRect>();
        
        // Tự động tìm ScrollRect cha ở các tầng UI phía trên
        if (transform.parent != null)
        {
            parentScrollRect = transform.parent.GetComponentInParent<ScrollRect>();
        }
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null)
        {
            // Kích hoạt tiềm năng kéo cho tất cả các component trên parent GameObject
            ExecuteEvents.Execute(parentScrollRect.gameObject, eventData, ExecuteEvents.initializePotentialDrag);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (parentScrollRect == null) return;

        // Tính toán vector kéo tổng thể tính từ điểm chạm ban đầu
        Vector2 dragVector = eventData.position - eventData.pressPosition;
        float dx = Mathf.Abs(dragVector.x);
        float dy = Mathf.Abs(dragVector.y);

        bool routeToParent = false;

        // Kiểm tra cấu hình của ScrollRect con để phân loại hướng kéo
        if (childScrollRect.horizontal && !childScrollRect.vertical)
        {
            // ScrollRect con chỉ cuộn Ngang, cha sẽ cuộn Dọc. Nếu vuốt dọc nhiều hơn vuốt ngang -> chuyển lên cha
            routeToParent = dy > dx;
        }
        else if (childScrollRect.vertical && !childScrollRect.horizontal)
        {
            // ScrollRect con chỉ cuộn Dọc, cha sẽ cuộn Ngang. Nếu vuốt ngang nhiều hơn vuốt dọc -> chuyển lên cha
            routeToParent = dx > dy;
        }

        if (routeToParent)
        {
            // 1. Tắt và bật lại ScrollRect con ngay lập tức để hủy trạng thái Drag hiện tại của nó
            childScrollRect.enabled = false;
            childScrollRect.enabled = true;

            // 2. Chuyển đối tượng kéo chính thức trong EventSystem sang cho ScrollRect cha
            eventData.pointerDrag = parentScrollRect.gameObject;

            // 3. Kích hoạt sự kiện OnBeginDrag trên TOÀN BỘ các component của parent GameObject (bao gồm cả ScrollRect và SimpleScrollSnap)
            ExecuteEvents.Execute(parentScrollRect.gameObject, eventData, ExecuteEvents.beginDragHandler);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Rỗng: Sau khi đã gán eventData.pointerDrag = parentScrollRect.gameObject ở OnBeginDrag,
        // EventSystem của Unity sẽ tự động gửi sự kiện OnDrag trực tiếp đến ScrollRect cha.
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Rỗng: EventSystem sẽ tự động gửi sự kiện OnEndDrag trực tiếp đến ScrollRect cha.
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (parentScrollRect != null)
        {
            // Xử lý sự kiện cuộn chuột hoặc chuột giữa/trackpad
            // float sx = Mathf.Abs(eventData.scrollDelta.x);
            // float sy = Mathf.Abs(eventData.scrollDelta.y);
            //
            // if (childScrollRect.horizontal && !childScrollRect.vertical && sy > sx)
            // {
            //     parentScrollRect.OnScroll(eventData);
            // }
            // else if (childScrollRect.vertical && !childScrollRect.horizontal && sx > sy)
            // {
            //     parentScrollRect.OnScroll(eventData);
            // }
        }
    }
}
