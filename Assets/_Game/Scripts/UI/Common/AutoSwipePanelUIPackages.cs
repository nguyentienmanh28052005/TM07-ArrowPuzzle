using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AutoSwipePanelUIPackages : MonoBehaviour
{
    [System.Serializable]
    public class Group
    {
        public bool isActive;
        public int id;
        public UIPackage panel;
        public GameObject pointHolder;
        public GameObject pointActive;
    }

    [SerializeField] private SwipePanel swipePanel;
    public Group[] groups;
    private Tween tweenScroll;
    private bool isRevert;

    private void Awake()
    {
        foreach (var group in groups)
        {
            group.panel.onBuySuccess += () => Init(true);
        }

        swipePanel.OnPanelChanged.AddListener(OnPanelChanged);
        swipePanel.onBeginDrag.AddListener(OnBeginDrag);
    }


    private void OnEnable()
    {
        Init(false);
        AutoScroll();
    }

    private void OnBeginDrag()
    {
        tweenScroll?.Kill();
    }

    private void OnPanelChanged(int newIndex)
    {
        if (swipePanel.panelCount <= 1)
        {
            foreach (var t in groups)
            {
                t.pointHolder.SetActive(false);
            }

            return;
        }

        var cur = 0;
        foreach (var t in groups)
        {
            if (!t.isActive) continue;
            var active = cur == newIndex;
            t.pointActive.SetActive(active);
            cur++;
        }

        AutoScroll();
    }

    public void Init(bool isRefresh)
    {
        var listPanel = new List<RectTransform>();
        var packageData = DataManager.Instance.packageData;
        foreach (var g in groups)
        {
            var data = packageData.FindPackage(g.id);
            g.isActive = data.isActive;
            if (data.isActive)
            {
                listPanel.Add(g.panel.GetComponent<RectTransform>());
                g.panel.SetUp(g.id);
            }
            else
            {
                g.panel.gameObject.SetActive(false);
                g.pointHolder.SetActive(false);
            }
        }

        swipePanel.SetupPanel(listPanel.ToArray());
        if (isRefresh)
        {
            swipePanel.Refresh();
        }
        else
        {
            swipePanel.Initialize(0);
            AutoScroll();
        }
    }

    public void AutoScroll()
    {
        if (swipePanel.panelCount == 0) return;
        tweenScroll?.Kill();
        int dir = isRevert ? -1 : 1;
        int next = swipePanel.CurrentIndex + dir;
        if (next >= swipePanel.panelCount)
        {
            isRevert = true;
            next = swipePanel.panelCount - 1;
        }
        else if (next < 0)
        {
            isRevert = false;
            next = 0;
        }

        tweenScroll = DOVirtual.DelayedCall(6f, () => { swipePanel.GoToPanel(next); })
            .OnComplete(AutoScroll);
    }

    private void OnDisable()
    {
        tweenScroll?.Kill();
    }
}