using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorPalettePanelView : MonoBehaviour
{
    [Header("Default Palette")]
    [SerializeField] private ColorPaletteAsset activePalette;

    [Header("UI Grid References")]
    [SerializeField] private Transform gridContent;
    [SerializeField] private Button colorButtonTemplate;
    [SerializeField] private Image colorPreviewImage;

    [Header("Runtime Controls")]
    [SerializeField] private Button btnRemoveColor;
    [SerializeField] private TMP_InputField ipfHexColor;
    [SerializeField] private Button btnAddColor;
    [SerializeField] private Image localPreviewImage;

    private List<Button> spawnedButtons = new List<Button>();
    private Color selectedColor = Color.white;

    private void Start()
    {
        InitializePanel();
    }

    private void OnDestroy()
    {
        if (LevelEditorWorkspace.Instance != null)
        {
            LevelEditorWorkspace.Instance.OnColorChanged -= OnWorkspaceColorChanged;
        }
    }

    public void InitializePanel()
    {
        if (colorButtonTemplate != null)
        {
            colorButtonTemplate.gameObject.SetActive(false);
        }

        // Register with Workspace
        if (LevelEditorWorkspace.Instance != null)
        {
            LevelEditorWorkspace.Instance.OnColorChanged += OnWorkspaceColorChanged;
            SetSelectedColor(LevelEditorWorkspace.Instance.currentColor);
        }

        // Set up button listeners
        if (btnRemoveColor != null)
        {
            btnRemoveColor.onClick.RemoveAllListeners();
            btnRemoveColor.onClick.AddListener(RemoveCurrentColor);
        }

        if (btnAddColor != null)
        {
            btnAddColor.onClick.RemoveAllListeners();
            btnAddColor.onClick.AddListener(AddCurrentColor);
        }

        if (ipfHexColor != null)
        {
            ipfHexColor.onValueChanged.RemoveAllListeners();
            ipfHexColor.onValueChanged.AddListener(OnHexColorValueChanged);
            ipfHexColor.onEndEdit.RemoveAllListeners();
            ipfHexColor.onEndEdit.AddListener(OnHexColorSubmitted);
        }

        RenderPaletteGrid();
    }

    private void SavePaletteAtRuntime()
    {
        if (activePalette != null)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(activePalette);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }
    }

    private void RenderPaletteGrid()
    {
        if (gridContent == null || colorButtonTemplate == null) return;

        // Clean up spawned buttons
        foreach (var btn in spawnedButtons)
        {
            if (btn != null && btn.gameObject != colorButtonTemplate.gameObject)
            {
                Destroy(btn.gameObject);
            }
        }
        spawnedButtons.Clear();

        if (activePalette == null) return;

        foreach (Color color in activePalette.colors)
        {
            GameObject btnObj = Instantiate(colorButtonTemplate.gameObject, gridContent);
            btnObj.SetActive(true);
            btnObj.name = $"BtnColor_{ColorUtility.ToHtmlStringRGB(color)}";

            // Set Color
            Image img = btnObj.GetComponent<Image>();
            if (img != null)
            {
                img.color = color;
            }

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                Color c = color;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (LevelEditorWorkspace.Instance != null)
                    {
                        LevelEditorWorkspace.Instance.UI_SetColor(c);
                    }
                });
                spawnedButtons.Add(btn);
            }
        }

        UpdateSelectionHighlight();

        ScrollRect sr = gridContent.GetComponentInParent<ScrollRect>();
        if (sr != null)
        {
            sr.normalizedPosition = Vector2.up;
        }
    }

    private void UpdateSelectionHighlight()
    {
        foreach (var btn in spawnedButtons)
        {
            if (btn == null) continue;
            Image img = btn.GetComponent<Image>();
            if (img == null) continue;

            bool isMatched = ColorsMatch(img.color, selectedColor);

            Transform border = btn.transform.Find("HighlightBorder");
            if (border != null)
            {
                border.gameObject.SetActive(isMatched);
            }

            // Add/Configure programmatic Outline component for guaranteed visibility and high contrast
            Outline outline = btn.GetComponent<Outline>();
            if (outline == null)
            {
                outline = btn.gameObject.AddComponent<Outline>();
            }
            
            if (isMatched)
            {
                // Calculate luminance to decide between black or white outline for contrast
                float luminance = 0.2126f * img.color.r + 0.7152f * img.color.g + 0.0722f * img.color.b;
                outline.effectColor = luminance > 0.5f ? Color.black : Color.white;
                outline.effectDistance = new Vector2(3, -3);
                outline.useGraphicAlpha = false;
                outline.enabled = true;
            }
            else
            {
                outline.enabled = false;
            }
        }
    }

    private bool ColorsMatch(Color a, Color b)
    {
        Color32 a32 = a;
        Color32 b32 = b;
        return a32.r == b32.r && a32.g == b32.g && a32.b == b32.b;
    }

    private void SetSelectedColor(Color color)
    {
        selectedColor = color;
        if (colorPreviewImage != null)
        {
            colorPreviewImage.color = color;
        }
        if (localPreviewImage != null)
        {
            localPreviewImage.color = color;
        }

        if (ipfHexColor != null)
        {
            ipfHexColor.SetTextWithoutNotify("#" + ColorUtility.ToHtmlStringRGB(color));
        }

        UpdateSelectionHighlight();
    }

    private void OnWorkspaceColorChanged(Color newColor)
    {
        Debug.Log($"[ColorPanel] Workspace color changed to: {newColor} (RGB: {newColor.r}, {newColor.g}, {newColor.b})");
        SetSelectedColor(newColor);
    }

    private void OnHexColorValueChanged(string hexText)
    {
        if (string.IsNullOrEmpty(hexText)) return;
        string cleanHex = hexText;
        if (!cleanHex.StartsWith("#"))
        {
            cleanHex = "#" + cleanHex;
        }

        if (ColorUtility.TryParseHtmlString(cleanHex, out Color parsedColor))
        {
            parsedColor.a = 1.0f;
            if (localPreviewImage != null)
            {
                localPreviewImage.color = parsedColor;
            }
        }
    }

    private void OnHexColorSubmitted(string hexText)
    {
        if (string.IsNullOrEmpty(hexText)) return;

        if (!hexText.StartsWith("#"))
        {
            hexText = "#" + hexText;
        }

        if (ColorUtility.TryParseHtmlString(hexText, out Color parsedColor))
        {
            parsedColor.a = 1.0f;
            if (LevelEditorWorkspace.Instance != null)
            {
                LevelEditorWorkspace.Instance.UI_SetColor(parsedColor);
            }
        }
        else
        {
            Debug.LogWarning($"Invalid hex color code: {hexText}");
        }
    }

    private void AddCurrentColor()
    {
        if (activePalette == null) return;

        Color colorToAdd = selectedColor;

        // Try to parse the input field text first (in case they typed it but didn't submit)
        if (ipfHexColor != null && !string.IsNullOrEmpty(ipfHexColor.text))
        {
            string hexText = ipfHexColor.text;
            if (!hexText.StartsWith("#")) hexText = "#" + hexText;
            if (ColorUtility.TryParseHtmlString(hexText, out Color parsedColor))
            {
                parsedColor.a = 1.0f;
                colorToAdd = parsedColor;
                if (LevelEditorWorkspace.Instance != null)
                {
                    LevelEditorWorkspace.Instance.UI_SetColor(parsedColor);
                }
            }
        }

        if (!activePalette.colors.Exists(c => ColorsMatch(c, colorToAdd)))
        {
            activePalette.colors.Add(colorToAdd);
            SavePaletteAtRuntime();
            RenderPaletteGrid();
            Debug.Log($"[ColorPanel] Color {ColorUtility.ToHtmlStringRGB(colorToAdd)} added to palette via '+' button.");
        }
    }

    private void RemoveCurrentColor()
    {
        if (activePalette == null) return;

        int index = activePalette.colors.FindIndex(c => ColorsMatch(c, selectedColor));
        if (index >= 0)
        {
            activePalette.colors.RemoveAt(index);
            SavePaletteAtRuntime();
            RenderPaletteGrid();

            Color fallbackColor = Color.white;
            if (activePalette.colors.Count > 0)
            {
                fallbackColor = activePalette.colors[0];
            }

            if (LevelEditorWorkspace.Instance != null)
            {
                LevelEditorWorkspace.Instance.UI_SetColor(fallbackColor);
            }
        }
    }
}
