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


    [Header("Panel Slide Animation")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private Ease slideEase = Ease.InOutQuart;

    private Vector2 _originPosition;
    private Vector2 _hiddenPosition;
    private bool _isPanelVisible = true;
    private bool _isTweening;

    private List<LevelDataV2> cachedLevels = new List<LevelDataV2>();
    private string searchFilter = string.Empty;
    private string difficultyFilter = "All";
    private string mechanicFilter = "All";
    private List<Button> spawnedMechanicButtons = new List<Button>();
    private string selectedLevelName = string.Empty;
    private Dictionary<string, Button> spawnedLevelButtons = new Dictionary<string, Button>();

    private bool initialized;

    private void Awake()
    {
        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();

        if (panelRect != null)
        {
            _originPosition = panelRect.anchoredPosition;
            _hiddenPosition = _originPosition - new Vector2(panelRect.rect.width, 0f);
        }
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
        cachedLevels.Clear();
        LevelDataV2[] allLevels = Resources.LoadAll<LevelDataV2>("Levels");
        if (allLevels != null)
        {
            cachedLevels.AddRange(allLevels.OrderBy(l => l.name));
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
        BindFilterButton(btnDiffAll, () => SetDifficultyFilter("All"));
        BindFilterButton(btnDiffNormal, () => SetDifficultyFilter("Normal"));
        BindFilterButton(btnDiffHard, () => SetDifficultyFilter("Hard"));
        BindFilterButton(btnDiffCrazy, () => SetDifficultyFilter("Crazy"));
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

        SetButtonHighlight(btnDiffAll, difficultyFilter == "All", highlightColor);
        SetButtonHighlight(btnDiffNormal, difficultyFilter == "Normal", highlightColor);
        SetButtonHighlight(btnDiffHard, difficultyFilter == "Hard", highlightColor);
        SetButtonHighlight(btnDiffCrazy, difficultyFilter == "Crazy", highlightColor);
    }

    private void ConfigureMechanicButtons()
    {
        BindFilterButton(btnMechanicAll, () => SetMechanicFilter("All"));

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

            var mechanics = new Dictionary<string, Color>
            {
                { "Portal", new Color(0.9f, 0.3f, 0.9f) },
                { "Deflector", new Color(0.2f, 0.8f, 0.9f) },
                { "BlackHole", new Color(0.2f, 0.2f, 0.25f) },
                { "Countdown", new Color(0.9f, 0.8f, 0.2f) },
                { "Electricity", new Color(0.9f, 0.2f, 0.2f) },
                { "Keycard", new Color(0.9f, 0.5f, 0.2f) }
            };

            foreach (var item in mechanics)
            {
                string key = item.Key;
                Color color = item.Value;

                GameObject btnObj = Instantiate(btnMechanicTemplate.gameObject, mechanicScrollContent);
                btnObj.name = $"Btn_Mechanic_{key}";
                btnObj.SetActive(true);

                Button btn = btnObj.GetComponent<Button>();
                Image img = btnObj.transform.Find("Image")?.GetComponent<Image>();
                if (img != null)
                {
                    img.color = color;
                }

                BindFilterButton(btn, () => SetMechanicFilter(key));
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

        // Clean up previous rows
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }
        spawnedLevelButtons.Clear();

        // Apply combined filters
        var filtered = cachedLevels.Where(level =>
        {
            // 1. Search Filter
            if (!string.IsNullOrEmpty(searchFilter))
            {
                bool matchesName = level.name.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                bool matchesId = level.levelIndex.ToString().Contains(searchFilter);
                if (!matchesName && !matchesId) return false;
            }

            // 2. Difficulty Filter
            if (difficultyFilter != "All")
            {
                if (difficultyFilter == "Normal")
                {
                    if (level.levelDifficulty != LevelDifficulty.Easy && 
                        level.levelDifficulty != LevelDifficulty.Medium && 
                        level.levelDifficulty != LevelDifficulty.Normal)
                        return false;
                }
                else if (difficultyFilter == "Hard")
                {
                    if (level.levelDifficulty != LevelDifficulty.Hard)
                        return false;
                }
                else if (difficultyFilter == "Crazy")
                {
                    if (level.levelDifficulty != LevelDifficulty.Crazy)
                        return false;
                }
            }

            // 3. Mechanic Filter
            if (mechanicFilter != "All" && !LevelHasMechanic(level, mechanicFilter))
                return false;

            return true;
        }).ToList();

        // Spawn rows
        for (int i = 0; i < filtered.Count; i++)
        {
            LevelDataV2 data = filtered[i];
            GameObject itemObj = Instantiate(levelRowItemPrefab, scrollContent);
            itemObj.name = $"Row_{data.name}";

            // Set index (ID column)
            string indexStr = (i + 1).ToString("D2");
            SetText(itemObj.transform.Find("Col_Id"), indexStr);

            // Set level name
            SetText(itemObj.transform.Find("Col_LevelName"), data.name);

            // Set difficulty
            SetText(itemObj.transform.Find("Col_Difficulity"), data.levelDifficulty.ToString());

            // Detect and display mechanics
            List<string> detectedMechanics = GetLevelMechanicsList(data);
            string mechanicsStr = detectedMechanics.Count > 0 ? string.Join(", ", detectedMechanics) : "Classic";
            SetText(itemObj.transform.Find("Col_Mechanics"), mechanicsStr);

            // Add button dynamically if not exists, and bind selection
            Button btn = itemObj.GetComponent<Button>();
            if (btn == null)
            {
                btn = itemObj.gameObject.AddComponent<Button>();
            }

            spawnedLevelButtons[data.name] = btn;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                SelectLevel(data.name);
                OnLevelSelected?.Invoke(data.name);
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

    private bool LevelHasMechanic(LevelDataV2 level, string mechanic)
    {
        if (level.cells == null) return false;

        switch (mechanic)
        {
            case "Portal":
                return level.cells.Any(c => c.typeId == CellTypeIds.Portal);
            case "Deflector":
                return level.cells.Any(c => c.typeId == CellTypeIds.Deflector);
            case "BlackHole":
                return level.cells.Any(c => c.typeId == CellTypeIds.BlackHole);
            case "Countdown":
                return level.cells.Any(c => c.typeId == CellTypeIds.CountdownBlock || c.typeId == CellTypeIds.StopBlock);
            case "Electricity":
                return level.cells.Any(c => c.typeId == CellTypeIds.ElectricWall || c.typeId == CellTypeIds.ElectricButton);
            case "Keycard":
                return level.cells.Any(c => c.typeId == CellTypeIds.Keycard || c.typeId == CellTypeIds.Gate);
            default:
                return false;
        }
    }

    private List<string> GetLevelMechanicsList(LevelDataV2 level)
    {
        List<string> mechanics = new List<string>();
        if (level.cells == null) return mechanics;

        if (level.cells.Any(c => c.typeId == CellTypeIds.Portal)) mechanics.Add("Portal");
        if (level.cells.Any(c => c.typeId == CellTypeIds.Deflector)) mechanics.Add("Deflector");
        if (level.cells.Any(c => c.typeId == CellTypeIds.BlackHole)) mechanics.Add("BlackHole");
        if (level.cells.Any(c => c.typeId == CellTypeIds.CountdownBlock || c.typeId == CellTypeIds.StopBlock)) mechanics.Add("Countdown");
        if (level.cells.Any(c => c.typeId == CellTypeIds.ElectricWall || c.typeId == CellTypeIds.ElectricButton)) mechanics.Add("Electricity");
        if (level.cells.Any(c => c.typeId == CellTypeIds.Keycard || c.typeId == CellTypeIds.Gate)) mechanics.Add("Keycard/Gate");

        return mechanics;
    }
}
