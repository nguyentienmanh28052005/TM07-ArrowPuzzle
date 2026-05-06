using System;
using System.Collections.Generic;
using Pixelplacement;
using UnityEngine;

public class ScreenManager : Singleton<ScreenManager>
{
    [Serializable]
    private class ScreenEntry
    {
        public ScreenType type;
        public CanvasGroup root;
        public bool disableWhenHidden = true;
        public bool callLifecycle = true;

        [NonSerialized] public IScreenLifecycle[] lifecycleTargets;
    }

    [Header("Setup")]
    [SerializeField] private ScreenType defaultScreen = ScreenType.MainMenu;
    [SerializeField] private bool playTransitionOnStart = false;
    [SerializeField] private List<ScreenEntry> screens = new List<ScreenEntry>();

    public static event Action<ScreenType> ScreenShown;
    public static event Action<ScreenType> ScreenHidden;

    private readonly Dictionary<ScreenType, ScreenEntry> _lookup = new Dictionary<ScreenType, ScreenEntry>();
    private ScreenEntry _current;

    private void Awake()
    {
        BuildLookup();
        HideAllScreensInternal();
    }

    private void Start()
    {
        if (TransitionManager.Instance != null)
        {
            if (playTransitionOnStart)
            {
                TransitionManager.Instance.TransitionToScreen(defaultScreen, true);
            }
            else
            {
                ShowScreen(defaultScreen, true);
            }
            return;
        }

        ShowScreen(defaultScreen);
    }

    public void ShowScreen(ScreenType type)
    {
        ShowScreen(type, false);
    }

    public void ShowScreen(ScreenType type, bool force)
    {
        if (!_lookup.TryGetValue(type, out ScreenEntry next) || next.root == null)
        {
            Debug.LogWarning("[ScreenManager] Screen not found for type: " + type);
            return;
        }

        if (_current == next && !force) return;

        if (_current != null)
        {
            HideScreenInternal(_current);
        }

        _current = next;
        ShowScreenInternal(_current);
    }

    public void HideAllScreens()
    {
        _current = null;
        HideAllScreensInternal();
    }

    private void BuildLookup()
    {
        _lookup.Clear();

        for (int i = 0; i < screens.Count; i++)
        {
            ScreenEntry entry = screens[i];
            if (entry == null || entry.root == null) continue;

            if (!_lookup.ContainsKey(entry.type))
            {
                _lookup.Add(entry.type, entry);
            }

            CacheLifecycle(entry);
        }
    }

    private void CacheLifecycle(ScreenEntry entry)
    {
        if (!entry.callLifecycle || entry.root == null)
        {
            entry.lifecycleTargets = null;
            return;
        }

        MonoBehaviour[] behaviours = entry.root.GetComponentsInChildren<MonoBehaviour>(true);
        List<IScreenLifecycle> lifecycles = new List<IScreenLifecycle>(behaviours.Length);

        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null) continue;

            if (behaviour is IScreenLifecycle lifecycle)
            {
                lifecycles.Add(lifecycle);
            }
        }

        entry.lifecycleTargets = lifecycles.Count > 0 ? lifecycles.ToArray() : null;
    }

    private void ShowScreenInternal(ScreenEntry entry)
    {
        if (entry.root == null) return;

        entry.root.gameObject.SetActive(true);
        entry.root.alpha = 1f;
        entry.root.interactable = true;
        entry.root.blocksRaycasts = true;

        InvokeLifecycle(entry, true);
        ScreenShown?.Invoke(entry.type);
    }

    private void HideScreenInternal(ScreenEntry entry)
    {
        if (entry.root == null) return;

        InvokeLifecycle(entry, false);
        ScreenHidden?.Invoke(entry.type);

        entry.root.alpha = 0f;
        entry.root.interactable = false;
        entry.root.blocksRaycasts = false;

        if (entry.disableWhenHidden)
        {
            entry.root.gameObject.SetActive(false);
        }
    }

    private void HideAllScreensInternal()
    {
        for (int i = 0; i < screens.Count; i++)
        {
            ScreenEntry entry = screens[i];
            if (entry == null || entry.root == null) continue;

            entry.root.alpha = 0f;
            entry.root.interactable = false;
            entry.root.blocksRaycasts = false;

            if (entry.disableWhenHidden)
            {
                entry.root.gameObject.SetActive(false);
            }
        }
    }

    private void InvokeLifecycle(ScreenEntry entry, bool show)
    {
        IScreenLifecycle[] targets = entry.lifecycleTargets;
        if (targets == null || targets.Length == 0) return;

        for (int i = 0; i < targets.Length; i++)
        {
            IScreenLifecycle target = targets[i];
            if (target == null) continue;

            if (show) target.OnScreenShow();
            else target.OnScreenHide();
        }
    }
}
