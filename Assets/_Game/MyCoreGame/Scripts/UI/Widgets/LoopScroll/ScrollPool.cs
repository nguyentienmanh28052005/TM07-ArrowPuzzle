using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollPool : PoolTemplate<PoolItemScroll>
{
    public ScrollRect scrollRect;
    public RectTransform content;
    public RectTransform parentContent;
    public int TotalCount { get; private set; }

    [Header("Layout Settings")] public RectOffset padding;
    public float itemSpacing;
    public bool controlChildSize = false;


    private float itemHeight;
    private int poolSize;
    private int topIndex = -1;
    private List<PoolItemScroll> pooledItems = new List<PoolItemScroll>();
    public List<PoolItemScroll> PooledItems => pooledItems;
    private Action<PoolItemScroll, int> OnProvideData;
    private bool isBlockUpdateVisibleItems = false;

    private void Start()
    {
        scrollRect.onValueChanged.AddListener(_ => UpdateVisibleItems());
    }

    public void Initialized(int totalCount, Action<PoolItemScroll, int> onProvideData)
    {
        isBlockUpdateVisibleItems = false;
        this.TotalCount = totalCount;
        OnProvideData = onProvideData;
        itemHeight = ((RectTransform)Prefab.transform).rect.height + itemSpacing;
        var viewHeight = (scrollRect.viewport).rect.height;
        poolSize = Mathf.CeilToInt(viewHeight / itemHeight) + 2;

        var totalHeight = totalCount * itemHeight - itemSpacing + padding.top + padding.bottom;
        content.sizeDelta = new Vector2(content.sizeDelta.x, totalHeight);
        UpdateParentContentSize();
        var max = Mathf.Min(poolSize, totalCount);
        foreach (var t in pooledItems)
        {
            Pool.Release(t);
        }

        pooledItems.Clear();
        for (var i = 0; i < max; i++)
        {
            var dataIndex = i;
            var item = Pool.Get();
            pooledItems.Add(item);
            item.SetScrollPool(this);
            item.SetIndex(dataIndex);
            OnProvideData.Invoke(item, dataIndex);
            var rect = ((RectTransform)item.transform);
            rect.anchoredPosition = new Vector2(padding.left - padding.right, -padding.top - dataIndex * itemHeight);
            ApplyChildSizing(rect);
        }

        topIndex = 0;
    }

    void UpdateVisibleItems(bool isRelease = true)
    {
        if (isBlockUpdateVisibleItems) return;
        var scrollY = content.anchoredPosition.y;
        if (parentContent)
        {
            scrollY = parentContent.anchoredPosition.y + scrollY;
        }

        var newTopIndex = Mathf.Clamp(Mathf.FloorToInt((scrollY - padding.top) / itemHeight), 0,
            Mathf.Max(0, TotalCount - poolSize));
        if (newTopIndex == topIndex) return;
        var del = Mathf.Min(poolSize, Mathf.Abs(newTopIndex - topIndex));
        if (topIndex < newTopIndex)
        {
            topIndex = newTopIndex;
            for (var i = 0; i < del; i++)
            {
                if (pooledItems.Count == 0) break;
                var item = pooledItems[0];
                pooledItems.Remove(item);
                Pool.Release(item);
            }

            for (var i = 0; i < del; i++)
            {
                CreateNewItemAtEnd();
            }
        }
        else
        {
            topIndex = newTopIndex;
            for (var i = 0; i < del; i++)
            {
                if (pooledItems.Count == 0) break;
                var item = pooledItems[^1];
                pooledItems.Remove(item);
                Pool.Release(item);
            }

            for (var i = 0; i < del; i++)
            {
                CreateNewItemAtTop();
            }
        }
    }

    void UpdateParentContentSize()
    {
        if (!parentContent) return;
        var sizeDelta = Vector2.zero;
        for (var i = 0; i < parentContent.childCount; i++)
        {
            if (parentContent.GetChild(i).gameObject.activeInHierarchy)
            {
                var rect1 = parentContent.GetChild(i).GetComponent<RectTransform>();
                sizeDelta += rect1.sizeDelta;
            }
        }

        parentContent.sizeDelta = new Vector2(parentContent.sizeDelta.x, sizeDelta.y);
    }

    private void ApplyChildSizing(RectTransform rt)
    {
        if (!controlChildSize) return;
        var targetWidth = scrollRect.viewport.rect.width - padding.left - padding.right;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
    }

    public void RemoveItemAt(int indexInData, Action onDone)
    {
        var itemToRemove = pooledItems.Find(x => x.Index == indexInData);
        if (itemToRemove == null)
        {
            onDone?.Invoke();
            return;
        }
        itemToRemove.AnimateRemove(() =>
        {
            pooledItems.Remove(itemToRemove);
            Destroy(itemToRemove.gameObject);
            TotalCount--;
            var totalHeight = TotalCount * itemHeight - itemSpacing + padding.top + padding.bottom;
            var newSizeDelta = new Vector2(content.sizeDelta.x, totalHeight);
            UpdateParentContentSize(); 
            for (var i = 0; i < pooledItems.Count; i++)
            {
                var idx = i;
                var endIndex = topIndex + i;
                var endPosition = new Vector2(padding.left-padding.right, -padding.top - endIndex * itemHeight);
                var item = pooledItems[idx];
                item.SetIndex(endIndex);
                item.GetComponent<RectTransform>().anchoredPosition = endPosition;
            }
            var isCreate = CreateNewItemAtEnd();
            if (!isCreate)
            {
                CreateNewItemAtTop();
                topIndex = pooledItems.Count > 0 ? pooledItems[0].Index : 0;
            }
            content.DOKill();
            content.DOSizeDelta(newSizeDelta, 0.2f).SetId(content).OnUpdate(UpdateParentContentSize)
                .OnComplete(UpdateParentContentSize);
            this.DOKill();
            var sq = DOTween.Sequence();
            sq.SetId(this);
            sq.AppendInterval(0.2f);
            sq.OnComplete(() => { onDone?.Invoke(); });
        });
    }

    public void ScrollToIndex(int index, bool jump = false, bool smooth = false, float duration = 0.25f,
        int skipThreshold = 100)
    {
        index = Mathf.Clamp(index, 0, TotalCount - 1);

        var newTopIndex = Mathf.Clamp(index, 0, Mathf.Max(0, TotalCount - poolSize));

        if (Mathf.Abs(newTopIndex - topIndex) > skipThreshold)
        {
            topIndex = newTopIndex;
        }
        else if (jump)
        {
            topIndex = newTopIndex;
        }

        float targetY = padding.top + index * itemHeight;

        if (smooth)
        {
            StopCoroutine("SmoothScrollTo");
            StartCoroutine(SmoothScrollTo(targetY, duration));
        }
        else
        {
            Vector2 anchoredPos = content.anchoredPosition;
            anchoredPos.y = targetY;
            content.anchoredPosition = anchoredPos;
            UpdateVisibleItems();
        }
    }

    IEnumerator SmoothScrollTo(float targetY, float duration)
    {
        float time = 0f;
        float startY = content.anchoredPosition.y;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float newY = Mathf.Lerp(startY, targetY, t);
            Vector2 anchoredPos = content.anchoredPosition;
            anchoredPos.y = newY;
            content.anchoredPosition = anchoredPos;
            UpdateVisibleItems();
            yield return null;
        }

        Vector2 finalPos = content.anchoredPosition;
        finalPos.y = targetY;
        content.anchoredPosition = finalPos;
        UpdateVisibleItems();
    }

    public void Refresh(int total)
    {
        content.DOKill();
        TotalCount = total;
        float totalHeight = TotalCount * itemHeight - itemSpacing + padding.top + padding.bottom;
        content.sizeDelta = new Vector2(content.sizeDelta.x, totalHeight);
        UpdateParentContentSize();
        var i = 0;
        while (i < pooledItems.Count)
        {
            if (topIndex + i < TotalCount)
            {
                var dataIndex = topIndex + i;
                pooledItems[i].SetIndex(dataIndex);
                OnProvideData.Invoke(pooledItems[i], dataIndex);
                i++;
            }
            else
            {
                var item = pooledItems[i];
                pooledItems.Remove(item);
                Pool.Release(item);
            }
        }

        var min = Mathf.Min(poolSize, TotalCount);
        if (poolSize > pooledItems.Count && (topIndex + min) <= TotalCount)
        {
            var del = TotalCount - pooledItems.Count;
            for (var j = 0; j < del; j++)
            {
                CreateNewItemAtEnd();
            }
        }
    }

    private bool CreateNewItemAtEnd()
    {
        var lastIndex = 0;
        if (pooledItems.Count > 0)
        {
            lastIndex = pooledItems[^1].Index;
        }
        else
        {
            lastIndex = topIndex + poolSize;
        }

        if (lastIndex >= (TotalCount - 1)) return false;
        var dataIndex = lastIndex + 1;
        var item = Pool.Get();
        pooledItems.Add(item);
        item.SetScrollPool(this);
        item.SetIndex(dataIndex);
        OnProvideData.Invoke(item, dataIndex);
        var rect = ((RectTransform)item.transform);
        rect.anchoredPosition = new Vector2(padding.left - padding.right, -padding.top - dataIndex * itemHeight);
        ApplyChildSizing(rect);
        return true;
    }

    private void CreateNewItemAt(int dataIndex, bool isLast)
    {
        var item = Pool.Get();
        if (isLast)
        {
            pooledItems.Add(item);
        }
        else
        {
            pooledItems.Insert(0, item);
        }

        item.SetScrollPool(this);
        item.SetIndex(dataIndex);
        OnProvideData.Invoke(item, dataIndex);
        var rect = ((RectTransform)item.transform);
        rect.anchoredPosition = new Vector2(padding.left - padding.right, -padding.top - dataIndex * itemHeight);
        ApplyChildSizing(rect);
    }

    private bool CreateNewItemAtTop()
    {
        var topIndex = 0;
        if (pooledItems.Count == 0)
        {
            topIndex = this.topIndex;
        }
        else
        {
            topIndex = pooledItems[0].Index;
        }

        if (topIndex == 0) return false;
        if(pooledItems.Count >= TotalCount) return false;
        var dataIndex = topIndex - 1;
        var item = Pool.Get();
        pooledItems.Insert(0, item);
        item.SetScrollPool(this);
        item.SetIndex(dataIndex);

        OnProvideData.Invoke(item, dataIndex);
        var rect = ((RectTransform)item.transform);
        rect.anchoredPosition = new Vector2(padding.left - padding.right, -padding.top - dataIndex * itemHeight);
        ApplyChildSizing(rect);
        return true;
    }

    private void OnDisable()
    {
        StopCoroutine("SmoothScrollTo");
        content.DOKill();
        this.DOKill();
        isBlockUpdateVisibleItems = false;
    }
}