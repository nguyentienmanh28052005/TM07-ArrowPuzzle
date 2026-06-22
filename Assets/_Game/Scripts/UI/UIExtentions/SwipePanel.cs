using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Events;
using System;
using System.Linq;

[RequireComponent(typeof(RectTransform))]
public class SwipePanel : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler,
    IInitializePotentialDragHandler
{
    [Header("Config")] [SerializeField, Range(0, 10)]
    private int currentIndex = 3; // cho phép set trong Editor

    [SerializeField] private float swipeThreshold = 0.2f;
    [SerializeField] private float tweenDuration = 0.3f;
    [SerializeField] private RectTransform[] panelsReferent;
    [SerializeField] private bool includeDisablePanels = true;
    private RectTransform[] panels;

    public UnityEvent<int> OnPanelChanged { get; set; } = new(); // callback khi đổi panel xong (snap)
    public UnityEvent onBeginDrag = new ();
    public UnityEvent<float> OnPanelMoving { get; set; } =
        new(); //    // callback khi đang di chuyển (0 = panel 0, 1 = panel 1, ...)

    private RectTransform rectTransform;
    private Vector2 startDragPos;
    private Vector2 currentPos;
    private float panelWidth;
    public int panelCount => panels.Length;
    private bool isHorizontalDrag;
    public int CurrentIndex => currentIndex;
    public float TweenDuration => tweenDuration;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        /*UpdatePanelWidth();
        ArrangePanels();

        // setup ban đầu
        SnapImmediate(currentIndex);
        OnPanelChanged?.Invoke(currentIndex);*/
    }

    public void SetupPanel(RectTransform[] panels)
    {
        panelsReferent = panels;
    }

    public void Initialize(int startIndex = -1)
    {
        rectTransform = GetComponent<RectTransform>();
        setPanels();
        // cập nhật lại width theo size hiện tại
        UpdatePanelWidth();

        // sắp xếp panel
        ArrangePanels();

        // nếu có truyền index thì set vào
        if (startIndex >= 0)
            currentIndex = Mathf.Clamp(startIndex, 0, panelCount - 1);

        // snap ngay (không tween)
        SnapImmediate(currentIndex);
        OnPanelChanged?.Invoke(currentIndex);
    }

    /// <summary>
    /// Làm mới layout khi panel enable/disable hoặc thay đổi bố cục.
    /// Refresh KHÔNG thay đổi currentIndex.
    /// </summary>
    public void Refresh()
    {
        float oldWidth = panelWidth;
        setPanels();
        UpdatePanelWidth();

        // thay đổi width → cần sắp xếp lại panel
        ArrangePanels();

        // nếu width khác trước → snap lại vị trí
        if (!Mathf.Approximately(oldWidth, panelWidth))
            SnapImmediate(currentIndex);
        else
        {
            // width không đổi nhưng panel bật/tắt khiến vị trí lệch
            SnapImmediate(currentIndex);
        }

        OnPanelMoving?.Invoke(currentIndex);
        OnPanelChanged?.Invoke(currentIndex);
    }

    public void DisablePanels(int index)
    {
        panels[index].gameObject.SetActive(false);
        Refresh();
    }

    private void setPanels()
    {
        if (includeDisablePanels)
        {
            panels = panelsReferent;
        }
        else
        {
            panels = panelsReferent.Where(x => x.gameObject.activeInHierarchy).ToArray();
        }
    }

    private void UpdatePanelWidth()
    {
        panelWidth = rectTransform.rect.width;
    }

    private void ArrangePanels()
    {
        for (var i = 0; i < panelCount; i++)
        {
            var child = panels[i];
            child.anchorMin = new Vector2(0, 0);
            child.anchorMax = new Vector2(0, 1);
            child.pivot = new Vector2(0, 0.5f);
            child.sizeDelta = new Vector2(panelWidth, 0);
            child.anchoredPosition = new Vector2(i * panelWidth, 0);
        }
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startDragPos = eventData.position;
        currentPos = rectTransform.anchoredPosition;
        DOTween.Kill(rectTransform);
        isHorizontalDrag = false;
        onBeginDrag.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isHorizontalDrag)
        {
            var dx = Mathf.Abs(eventData.position.x - startDragPos.x);
            var dy = Mathf.Abs(eventData.position.y - startDragPos.y);

            if (dx > dy) isHorizontalDrag = true;
            else return; // vuốt dọc → để ScrollRect xử lý
        }

        var deltaX = eventData.position.x - startDragPos.x;
        rectTransform.anchoredPosition = currentPos + new Vector2(deltaX, 0);

        // báo vị trí tương đối để update tab
        float normalizedIndex = -rectTransform.anchoredPosition.x / panelWidth;
        OnPanelMoving?.Invoke(normalizedIndex);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float deltaX = eventData.position.x - startDragPos.x;
        float ratio = Mathf.Abs(deltaX) / panelWidth;

        if (isHorizontalDrag && ratio > swipeThreshold)
        {
            currentIndex += (deltaX < 0) ? 1 : -1;
            currentIndex = Mathf.Clamp(currentIndex, 0, panelCount - 1);
        }

        SnapToPanel(currentIndex);
        OnPanelChanged?.Invoke(currentIndex);
    }

    private void SnapToPanel(int index, System.Action onCompleted = null)
    {
        Vector2 targetPos = new Vector2(-index * panelWidth, 0);
        rectTransform.DOAnchorPos(targetPos, tweenDuration)
            .SetEase(Ease.OutCubic)
            .OnUpdate(() =>
            {
                // liên tục báo trong lúc tween
                float normalizedIndex = -rectTransform.anchoredPosition.x / panelWidth;
                OnPanelMoving?.Invoke(normalizedIndex);
            })
            .OnComplete(() => { onCompleted?.Invoke(); }).SetId(this);;
    }

    private void SnapImmediate(int index)
    {
        Vector2 targetPos = new Vector2(-index * panelWidth, 0);
        rectTransform.anchoredPosition = targetPos;
    }

    protected void OnRectTransformDimensionsChange()
    {
        if (!rectTransform) return;

        float oldWidth = panelWidth;
        UpdatePanelWidth();

        if (!Mathf.Approximately(oldWidth, panelWidth))
        {
            ArrangePanels();
            SnapImmediate(currentIndex);
        }
    }

    /// <summary>
    /// Chuyển tới panel theo index bằng code.
    /// </summary>
    public void GoToPanel(int index, bool animate = true, Action onCompleted = null)
    {
        index = Mathf.Clamp(index, 0, panelCount - 1);
        currentIndex = index;

        if (animate)
            SnapToPanel(index, onCompleted);
        else
            SnapImmediate(index);

        OnPanelChanged?.Invoke(index);
    }

    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}