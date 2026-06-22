using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LevelListPanelView : MonoBehaviour
{
    public event Action<string> OnLevelSelected;

    [Header("Search & Visibility")]
    [SerializeField] private TMP_InputField ipfSearchLevel;
    [SerializeField] private Button btnFilterToggle;
    [SerializeField] private GameObject filterLevelsContainer;

    [Header("List Scroll View")]
    [SerializeField] private Transform scrollContent;
    [SerializeField] private GameObject levelRowItemPrefab;

    [Header("Difficulty Filters")]
    [SerializeField] private Button btnDiffAll;
    [SerializeField] private Button btnDiffNormal;
    [SerializeField] private Button btnDiffHard;
    [SerializeField] private Button btnDiffCrazy;

    [Header("Mechanic Filters")]
    [SerializeField] private Transform mechanicScrollContent;
    [SerializeField] private Button btnMechanicAll;
    [SerializeField] private Button btnMechanicTemplate; // Btn_Mechanic template in Content


    [Header("Difficulty Mappings")]
    [SerializeField] private List<DifficultyFilterMapping> difficultyFilters = new List<DifficultyFilterMapping>();

    [Header("Mechanic UI Configurations")]
    [SerializeField] private List<MechanicUIConfig> mechanicConfigs = new List<MechanicUIConfig>();

    [Header("Panel Slide Animation")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private Ease slideEase = Ease.InOutQuart;

    private Vector2 _originPosition;
    private Vector2 _hiddenPosition;
    private bool _isPanelVisible = true;
    private bool _isTweening;

    private List<LevelRowData> cachedLevelRows = new List<LevelRowData>();
    private List<GameObject> activeRows = new List<GameObject>();
    private List<GameObject> rowPool = new List<GameObject>();

    private string searchFilter = string.Empty;
    private string difficultyFilter = "All";
    private string mechanicFilter = "All";
    private List<Button> spawnedMechanicButtons = new List<Button>();
    private string selectedLevelName = string.Empty;
    private Dictionary<string, Button> spawnedLevelButtons = new Dictionary<string, Button>();

    private bool initialized;

#if UNITY_EDITOR
    private void Reset()
    {
        AutoPopulateConfigs();
    }

    private void OnValidate()
    {
        AutoPopulateConfigs();
    }

    private void AutoPopulateConfigs()
    {
        if (difficultyFilters == null || difficultyFilters.Count == 0)
        {
            difficultyFilters = new List<DifficultyFilterMapping>();
            if (btnDiffNormal != null)
            {
                difficultyFilters.Add(new DifficultyFilterMapping
                {
                    filterButton = btnDiffNormal,
                    matchedDifficulties = new List<LevelDifficulty> { LevelDifficulty.Easy, LevelDifficulty.Medium, LevelDifficulty.Normal }
                });
            }
            if (btnDiffHard != null)
            {
                difficultyFilters.Add(new DifficultyFilterMapping
                {
                    filterButton = btnDiffHard,
                    matchedDifficulties = new List<LevelDifficulty> { LevelDifficulty.Hard }
                });
            }
            if (btnDiffCrazy != null)
            {
                difficultyFilters.Add(new DifficultyFilterMapping
                {
                    filterButton = btnDiffCrazy,
                    matchedDifficulties = new List<LevelDifficulty> { LevelDifficulty.Crazy }
                });
            }
        }

        if (mechanicConfigs == null || mechanicConfigs.Count == 0)
        {
            mechanicConfigs = new List<MechanicUIConfig>
            {
                new MechanicUIConfig { cellTypeId = CellTypeIds.Portal, displayName = "Portal", themeColor = new Color(0.9f, 0.3f, 0.9f) },
                new MechanicUIConfig { cellTypeId = CellTypeIds.Deflector, displayName = "Deflector", themeColor = new Color(0.2f, 0.8f, 0.9f) },
                new MechanicUIConfig { cellTypeId = CellTypeIds.BlackHole, displayName = "BlackHole", themeColor = new Color(0.2f, 0.2f, 0.25f) },
                new MechanicUIConfig { cellTypeId = CellTypeIds.CountdownBlock, displayName = "Countdown", themeColor = new Color(0.9f, 0.8f, 0.2f) },
                new MechanicUIConfig { cellTypeId = CellTypeIds.StopBlock, displayName = "Countdown", themeColor = new Color(0.9f, 0.8f, 0.2f) },
                new MechanicUIConfig { cellTypeId = CellTypeIds.ElectricWall, displayName = "Electricity", themeColor = new Color(0.9f, 0.2f, 0.2f) },
                new MechanicUIConfig { cellTypeId = CellTypeIds.ElectricButton, displayName = "Electricity", themeColor = new Color(0.9f, 0.2f, 0.2f) },
                new MechanicUIConfig { cellTypeId = CellTypeIds.Keycard, displayName = "Keycard/Gate", themeColor = new Color(0.9f, 0.5f, 0.2f) },
                new MechanicUIConfig { cellTypeId = CellTypeIds.Gate, displayName = "Keycard/Gate", themeColor = new Color(0.9f, 0.5f, 0.2f) }
            };
        }
    }
#endif

    private void Awake()
    {
        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();

        if (panelRect != null)
        {
            _originPosition = panelRect.anchoredPosition;
            _hiddenPosition = _originPosition - new Vector2(panelRect.rect.width, 0f);
        }

        EditorInputManager.OnToggleLevelListPanelPressed += TogglePanel;
    }

    private void OnDestroy()
    {
        EditorInputManager.OnToggleLevelListPanelPressed -= TogglePanel;
    }




    public void TogglePanel()
    {
        if (_isTweening) return;

        if (_isPanelVisible)
            HidePanel();
        else
            ShowPanel();
    }

    public void ShowPanel()
    {
        if (panelRect == null) return;
        _isTweening = true;
        _isPanelVisible = true;
        panelRect.DOAnchorPos(_originPosition, slideDuration)
            .SetEase(slideEase)
            .OnComplete(() => _isTweening = false);
    }

    public void HidePanel()
    {
        if (panelRect == null) return;
        _isTweening = true;
        _isPanelVisible = false;
        panelRect.DOAnchorPos(_hiddenPosition, slideDuration)
            .SetEase(slideEase)
            .OnComplete(() => _isTweening = false);
    }

    public void InitializeList()
    {
        if (initialized) return;

        // Find/Cache levels
        RefreshCachedLevels();

        // Configure Search input
        if (ipfSearchLevel != null)
        {
            ipfSearchLevel.onValueChanged.RemoveAllListeners();
            ipfSearchLevel.onValueChanged.AddListener(OnSearchChanged);
        }

        // Configure Filter Visibility Toggle
        if (btnFilterToggle != null)
        {
            btnFilterToggle.onClick.RemoveAllListeners();
            btnFilterToggle.onClick.AddListener(ToggleFilterPanel);
        }

        // Configure Difficulty filter buttons
        ConfigureDifficultyButtons();

        // Configure Mechanic filter buttons
        ConfigureMechanicButtons();

        initialized = true;
        
        // Initialize highlights
        UpdateDifficultyButtonVisuals();
        UpdateMechanicButtonVisuals();
        RenderList();
    }

    public void RefreshList()
    {
        RefreshCachedLevels();
        RenderList();
    }

    private void RefreshCachedLevels()
    {
        cachedLevelRows.Clear();
        LevelDataV2[] allLevels = Resources.LoadAll<LevelDataV2>("Levels");
        if (allLevels == null) return;

        foreach (var level in allLevels.OrderBy(l => l.name))
        {
            HashSet<string> mechanics = new HashSet<string>();
            if (level.cells != null)
            {
                foreach (var cell in level.cells)
                {
                    var config = mechanicConfigs.Find(c => c.cellTypeId == cell.typeId);
                    if (config.displayName != null)
                    {
                        mechanics.Add(config.displayName);
                    }
                }
            }

            string mechanicsString = mechanics.Count > 0 ? string.Join(", ", mechanics) : "Classic";

            cachedLevelRows.Add(new LevelRowData
            {
                levelData = level,
                levelName = level.name,
                levelIndex = level.levelIndex,
                difficultyGroup = level.levelDifficulty.ToString(),
                mechanicNames = mechanics,
                mechanicsString = mechanicsString
            });
        }
    }

    private void OnSearchChanged(string value)
    {
        searchFilter = value ?? string.Empty;
        RenderList();
    }

    private void ToggleFilterPanel()
    {
        if (filterLevelsContainer != null)
        {
            filterLevelsContainer.SetActive(!filterLevelsContainer.activeSelf);
        }
    }

    private void ConfigureDifficultyButtons()
    {
        if (btnDiffAll != null)
        {
            BindFilterButton(btnDiffAll, () => SetDifficultyFilter("All"));
        }

        foreach (var mapping in difficultyFilters)
        {
            if (mapping.filterButton != null)
            {
                var btnName = mapping.filterButton.name;
                BindFilterButton(mapping.filterButton, () => SetDifficultyFilter(btnName));
            }
        }
    }

    private void SetDifficultyFilter(string difficulty)
    {
        difficultyFilter = difficulty;
        UpdateDifficultyButtonVisuals();
        RenderList();
    }

    private void UpdateDifficultyButtonVisuals()
    {
        Color highlightColor = new Color(0.75f, 0.9f, 1f, 1f);

        if (btnDiffAll != null)
        {
            SetButtonHighlight(btnDiffAll, difficultyFilter == "All", highlightColor);
        }

        foreach (var mapping in difficultyFilters)
        {
            if (mapping.filterButton != null)
            {
                SetButtonHighlight(mapping.filterButton, difficultyFilter == mapping.filterButton.name, highlightColor);
            }
        }
    }

    private void ConfigureMechanicButtons()
    {
        if (btnMechanicAll != null)
        {
            BindFilterButton(btnMechanicAll, () => SetMechanicFilter("All"));
        }

        if (btnMechanicTemplate != null && mechanicScrollContent != null)
        {
            btnMechanicTemplate.gameObject.SetActive(false);

            // Clean up existing instantiated mechanic buttons
            foreach (Transform child in mechanicScrollContent)
            {
                if (child.gameObject != btnMechanicTemplate.gameObject && child.gameObject != btnMechanicAll.gameObject)
                {
                    Destroy(child.gameObject);
                }
            }

            spawnedMechanicButtons.Clear();

            // Group configs by display name to avoid duplicates (e.g. keycard and gate both map to "Keycard/Gate")
            var groupedConfigs = mechanicConfigs
                .GroupBy(c => c.displayName)
                .ToList();

            foreach (var group in groupedConfigs)
            {
                string displayName = group.Key;
                if (string.IsNullOrEmpty(displayName)) continue;

                var firstConfig = group.First();
                Color color = firstConfig.themeColor;

                GameObject btnObj = Instantiate(btnMechanicTemplate.gameObject, mechanicScrollContent);
                btnObj.name = $"Btn_Mechanic_{displayName}";
                btnObj.SetActive(true);

                Button btn = btnObj.GetComponent<Button>();
                Image img = btnObj.transform.Find("Image")?.GetComponent<Image>();
                if (img != null)
                {
                    img.color = color;

                    // Resolve sprite dynamically from LevelEditorWorkspace
                    if (LevelEditorWorkspace.Instance != null)
                    {
                        Sprite mechanicSprite = null;
                        foreach (var cfg in group)
                        {
                            mechanicSprite = LevelEditorWorkspace.Instance.GetSpriteByCellTypeId(cfg.cellTypeId);
                            if (mechanicSprite != null) break;
                        }

                        if (mechanicSprite != null)
                        {
                            img.sprite = mechanicSprite;
                        }
                    }
                }

                BindFilterButton(btn, () => SetMechanicFilter(displayName));
                spawnedMechanicButtons.Add(btn);
            }
        }
    }

    private void SetMechanicFilter(string mechanic)
    {
        mechanicFilter = mechanic;
        UpdateMechanicButtonVisuals();
        RenderList();
    }

    private void UpdateMechanicButtonVisuals()
    {
        Color selectedColor = new Color(0.75f, 0.9f, 1f, 1f);

        if (btnMechanicAll != null)
        {
            SetButtonHighlight(btnMechanicAll, mechanicFilter == "All", selectedColor);
        }

        foreach (var btn in spawnedMechanicButtons)
        {
            if (btn == null) continue;
            string key = btn.name.Replace("Btn_Mechanic_", "");
            SetButtonHighlight(btn, mechanicFilter == key, selectedColor);
        }
    }

    private void SetButtonHighlight(Button button, bool isHighlighted, Color highlightColor)
    {
        if (button == null) return;
        Color targetColor = isHighlighted ? highlightColor : Color.white;

        var img = button.GetComponent<Image>();
        if (img != null)
        {
            img.color = targetColor;
        }

        var cb = button.colors;
        cb.normalColor = targetColor;
        cb.selectedColor = targetColor;
        // Keep a slightly darker color for hover when selected, otherwise default light gray hover
        cb.highlightedColor = isHighlighted
            ? new Color(targetColor.r * 0.95f, targetColor.g * 0.95f, targetColor.b * 0.95f, targetColor.a)
            : new Color(0.96f, 0.96f, 0.96f, 1f);
        button.colors = cb;
    }

    private void BindFilterButton(Button button, Action onClick)
    {
        if (button == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
    }

    private void RenderList()
    {
        if (scrollContent == null || levelRowItemPrefab == null) return;

        // 1. Recycle current active rows back to the pool
        foreach (var row in activeRows)
        {
            if (row != null)
            {
                row.SetActive(false);
                rowPool.Add(row);
            }
        }
        activeRows.Clear();
        spawnedLevelButtons.Clear();

        // 2. Filter cached level rows
        var filtered = cachedLevelRows.Where(row =>
        {
            // Search Filter
            if (!string.IsNullOrEmpty(searchFilter))
            {
                bool matchesName = row.levelName.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                bool matchesId = row.levelIndex.ToString().Contains(searchFilter);
                if (!matchesName && !matchesId) return false;
            }

            // Difficulty Filter
            if (difficultyFilter != "All")
            {
                var mapping = difficultyFilters.Find(m => m.filterButton != null && m.filterButton.name == difficultyFilter);
                if (mapping.filterButton != null)
                {
                    if (!mapping.matchedDifficulties.Contains(row.levelData.levelDifficulty))
                        return false;
                }
                else
                {
                    return false;
                }
            }

            // Mechanic Filter
            if (mechanicFilter != "All")
            {
                if (!row.mechanicNames.Contains(mechanicFilter))
                    return false;
            }

            return true;
        }).ToList();

        // 3. Render items using pool
        for (int i = 0; i < filtered.Count; i++)
        {
            LevelRowData data = filtered[i];
            GameObject itemObj = null;

            if (rowPool.Count > 0)
            {
                itemObj = rowPool[rowPool.Count - 1];
                rowPool.RemoveAt(rowPool.Count - 1);
            }
            else
            {
                itemObj = Instantiate(levelRowItemPrefab, scrollContent);
            }

            if (itemObj == null) continue;

            itemObj.name = $"Row_{data.levelName}";
            itemObj.SetActive(true);
            activeRows.Add(itemObj);

            // Set index (ID column)
            string indexStr = (i + 1).ToString("D2");
            SetText(itemObj.transform.Find("Col_Id"), indexStr);

            // Set level name
            SetText(itemObj.transform.Find("Col_LevelName"), data.levelName);

            // Set difficulty
            SetText(itemObj.transform.Find("Col_Difficulty"), data.levelData.levelDifficulty.ToString());

            // Set mechanics
            SetText(itemObj.transform.Find("Col_Mechanics"), data.mechanicsString);

            // Setup button event
            Button btn = itemObj.GetComponent<Button>();
            if (btn == null)
            {
                btn = itemObj.gameObject.AddComponent<Button>();
            }

            spawnedLevelButtons[data.levelName] = btn;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                SelectLevel(data.levelName);
                OnLevelSelected?.Invoke(data.levelName);
            });
        }

        UpdateLevelRowVisuals();
    }

    public void SelectLevel(string levelName)
    {
        selectedLevelName = levelName;
        UpdateLevelRowVisuals();
    }

    private void UpdateLevelRowVisuals()
    {
        Color highlightColor = new Color(0.7f, 0.85f, 1f, 1f); // slightly darker/richer blue for level selection
        foreach (var kvp in spawnedLevelButtons)
        {
            if (kvp.Value == null) continue;
            SetButtonHighlight(kvp.Value, kvp.Key == selectedLevelName, highlightColor);
        }
    }

    private void SetText(Transform parent, string text)
    {
        if (parent == null) return;
        TMP_Text tmp = parent.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = text;
        }
    }
}

[System.Serializable]
public struct DifficultyFilterMapping
{
    public Button filterButton;
    public List<LevelDifficulty> matchedDifficulties;
}

[System.Serializable]
public struct MechanicUIConfig
{
    public string cellTypeId;
    public string displayName;
    public Color themeColor;
}

public struct LevelRowData
{
    public LevelDataV2 levelData;
    public string levelName;
    public int levelIndex;
    public string difficultyGroup;
    public HashSet<string> mechanicNames;
    public string mechanicsString;
}
