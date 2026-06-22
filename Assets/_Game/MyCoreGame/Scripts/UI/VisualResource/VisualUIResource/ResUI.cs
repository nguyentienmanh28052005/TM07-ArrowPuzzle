using UnityEngine;
using System;
using System.Linq;

public class ResUI : MonoBehaviour
{
    [SerializeField] private ResUIBase[] visual;
    public DataResource data { get; private set; }
    private bool isInit = false;
    public ResUIBase CurrentResUI { get; private set; }

    public void Init(DataResource data, Action onClickRes = null)
    {
        if (data == null)
        {
            return;
        }

        init();
        this.data = data;
        var vs = getVisual(data);
        if (vs != null)
        {
            CurrentResUI = vs;
            vs.Init(data, onClickRes);
            vs.Show();
        }
    }

    public ResUIBase GetVisual()
    {
        return CurrentResUI;
        if (data == null) return null;
        return getVisual(data);
    }

    private void Awake()
    {
        init();
    }

    private void init()
    {
        if (!isInit)
        {
            isInit = true;
            for (int i = 0; i < visual.Length; i++)
            {
                visual[i].Hide();
            }
        }
    }

    private ResUIBase getVisual(DataResource data)
    {
        for (int i = 0; i < visual.Length; i++)
        {
            if (visual[i].CanVisual(data)) return visual[i];
        }

        return visual[0];
    }
}