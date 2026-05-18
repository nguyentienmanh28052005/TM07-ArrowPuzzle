using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ThemeVisualState
{
    public GameThemeMode theme = GameThemeMode.Light;

    [Header("Color")]
    public bool overrideColor;
    public Color color = Color.white;

    [Header("Sprite")]
    public bool overrideSprite;
    public Sprite sprite;

    [Header("ButtonClicky Sprites")]
    public bool overridePressedSprite;
    public Sprite pressedSprite;
    public bool overrideDisabledSprite;
    public Sprite disabledSprite;
}

[DisallowMultipleComponent]
public class ThemeVisualApplier : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private bool autoFindTargets = true;
    [SerializeField] private Image targetImage;
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private LineRenderer targetLineRenderer;
    [SerializeField] private ButtonClicky targetButtonClicky;

    [Header("Theme States")]
    [SerializeField] private bool applySavedThemeOnEnable = true;
    [SerializeField] private List<ThemeVisualState> states = new List<ThemeVisualState>
    {
        new ThemeVisualState { theme = GameThemeMode.Light }
    };

    private Color _defaultImageColor;
    private Color _defaultSpriteRendererColor;
    private Color _defaultTextColor;
    private Color _defaultLineStartColor;
    private Color _defaultLineEndColor;
    private Sprite _defaultImageSprite;
    private Sprite _defaultSpriteRendererSprite;
    private Sprite _defaultButtonSprite;
    private Sprite _defaultPressedSprite;
    private Sprite _defaultDisabledSprite;
    private bool _hasDefaults;
    private bool _hasTargetDefaults;
    private bool _isSubscribed;
    private Coroutine _subscribeRoutine;

    private void Awake()
    {
        CacheTargets();
        CacheDefaults();
    }

    private void OnEnable()
    {
        CacheTargets();
        CacheDefaults();
        TrySubscribe();

        if (applySavedThemeOnEnable)
        {
            ApplyTheme(ThemeChangeMessage.CurrentTheme);
        }
    }

    private void Start()
    {
        if (applySavedThemeOnEnable)
        {
            ApplyTheme(ThemeChangeMessage.CurrentTheme);
        }
    }

    private void OnDisable()
    {
        if (_subscribeRoutine != null)
        {
            StopCoroutine(_subscribeRoutine);
            _subscribeRoutine = null;
        }

        Unsubscribe();
    }

    [ContextMenu("Apply Dark Theme")]
    public void ApplyDarkTheme()
    {
        ApplyTheme(GameThemeMode.Dark);
    }

    [ContextMenu("Apply Light Theme")]
    public void ApplyLightTheme()
    {
        ApplyTheme(GameThemeMode.Light);
    }

    public void ApplyTheme(GameThemeMode theme)
    {
        CacheTargets();
        CacheDefaults();

        ThemeVisualState state = FindState(theme);
        if (state == null)
        {
            RestoreDefaults();
            return;
        }

        ApplySprites(state);
        ApplyColors(state);
    }

    private void HandleThemeChanged(object data)
    {
        GameThemeMode theme;
        if (!ThemeChangeMessage.TryRead(data, out theme)) return;
        ApplyTheme(theme);
    }

    private void TrySubscribe()
    {
        if (_isSubscribed) return;

        MessageManager manager = FindMessageManager();
        if (manager == null)
        {
            if (_subscribeRoutine == null) _subscribeRoutine = StartCoroutine(WaitAndSubscribe());
            return;
        }

        manager.AddSubscriber(ManhMessageType.OnThemeChanged, HandleThemeChanged);
        _isSubscribed = true;
    }

    private IEnumerator WaitAndSubscribe()
    {
        while (enabled && gameObject.activeInHierarchy && FindMessageManager() == null)
        {
            yield return null;
        }

        _subscribeRoutine = null;
        TrySubscribe();
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed) return;

        MessageManager manager = FindMessageManager();
        if (manager != null)
        {
            manager.RemoveSubscriber(ManhMessageType.OnThemeChanged, HandleThemeChanged);
        }

        _isSubscribed = false;
    }

    private MessageManager FindMessageManager()
    {
        return FindObjectOfType<MessageManager>();
    }

    private ThemeVisualState FindState(GameThemeMode theme)
    {
        for (int i = 0; i < states.Count; i++)
        {
            if (states[i] != null && states[i].theme == theme) return states[i];
        }

        return null;
    }

    private void ApplyColors(ThemeVisualState state)
    {
        Color imageColor = state.overrideColor ? state.color : _defaultImageColor;
        Color spriteColor = state.overrideColor ? state.color : _defaultSpriteRendererColor;
        Color textColor = state.overrideColor ? state.color : _defaultTextColor;
        Color lineStartColor = state.overrideColor ? state.color : _defaultLineStartColor;
        Color lineEndColor = state.overrideColor ? state.color : _defaultLineEndColor;

        if (targetImage != null) targetImage.color = imageColor;
        if (targetSpriteRenderer != null) targetSpriteRenderer.color = spriteColor;
        if (targetText != null) targetText.color = textColor;
        if (targetLineRenderer != null)
        {
            targetLineRenderer.startColor = lineStartColor;
            targetLineRenderer.endColor = lineEndColor;
        }

        if (targetButtonClicky != null)
        {
            targetButtonClicky.SetColor(imageColor);
        }
    }

    private void ApplySprites(ThemeVisualState state)
    {
        Sprite imageSprite = state.overrideSprite ? state.sprite : _defaultImageSprite;
        Sprite spriteRendererSprite = state.overrideSprite ? state.sprite : _defaultSpriteRendererSprite;

        if (targetImage != null) targetImage.sprite = imageSprite;
        if (targetSpriteRenderer != null) targetSpriteRenderer.sprite = spriteRendererSprite;

        if (targetButtonClicky == null) return;

        Sprite buttonSprite = state.overrideSprite ? state.sprite : _defaultButtonSprite;
        if (buttonSprite == null) buttonSprite = imageSprite;

        Sprite pressedSprite = state.overridePressedSprite ? state.pressedSprite : _defaultPressedSprite;
        if (state.overrideSprite && !state.overridePressedSprite) pressedSprite = buttonSprite;

        Sprite disabledSprite = state.overrideDisabledSprite ? state.disabledSprite : _defaultDisabledSprite;
        if (state.overrideSprite && !state.overrideDisabledSprite) disabledSprite = buttonSprite;

        targetButtonClicky.SetSprites(buttonSprite, pressedSprite, disabledSprite);
    }

    private void RestoreDefaults()
    {
        if (targetImage != null)
        {
            targetImage.color = _defaultImageColor;
            targetImage.sprite = _defaultImageSprite;
        }

        if (targetSpriteRenderer != null)
        {
            targetSpriteRenderer.color = _defaultSpriteRendererColor;
            targetSpriteRenderer.sprite = _defaultSpriteRendererSprite;
        }

        if (targetText != null) targetText.color = _defaultTextColor;

        if (targetLineRenderer != null)
        {
            targetLineRenderer.startColor = _defaultLineStartColor;
            targetLineRenderer.endColor = _defaultLineEndColor;
        }

        if (targetButtonClicky != null)
        {
            targetButtonClicky.SetSprites(_defaultButtonSprite, _defaultPressedSprite, _defaultDisabledSprite);
            targetButtonClicky.SetColor(_defaultImageColor);
        }
    }

    private void CacheTargets()
    {
        if (!autoFindTargets && HasAnyTarget()) return;

        if (targetImage == null) targetImage = GetComponent<Image>();
        if (targetSpriteRenderer == null) targetSpriteRenderer = GetComponent<SpriteRenderer>();
        if (targetText == null) targetText = GetComponent<TMP_Text>();
        if (targetLineRenderer == null) targetLineRenderer = GetComponent<LineRenderer>();
        if (targetButtonClicky == null) targetButtonClicky = GetComponent<ButtonClicky>();
    }

    private bool HasAnyTarget()
    {
        return targetImage != null
            || targetSpriteRenderer != null
            || targetText != null
            || targetLineRenderer != null
            || targetButtonClicky != null;
    }

    private void CacheDefaults()
    {
        if (!HasAnyTarget()) return;
        if (_hasDefaults && _hasTargetDefaults) return;

        if (targetImage != null)
        {
            _defaultImageColor = targetImage.color;
            _defaultImageSprite = targetImage.sprite;
        }

        if (targetSpriteRenderer != null)
        {
            _defaultSpriteRendererColor = targetSpriteRenderer.color;
            _defaultSpriteRendererSprite = targetSpriteRenderer.sprite;
        }

        if (targetText != null) _defaultTextColor = targetText.color;

        if (targetLineRenderer != null)
        {
            _defaultLineStartColor = targetLineRenderer.startColor;
            _defaultLineEndColor = targetLineRenderer.endColor;
        }

        if (targetButtonClicky != null)
        {
            _defaultButtonSprite = targetButtonClicky.DefaultSprite;
            _defaultPressedSprite = targetButtonClicky.PressedSprite;
            _defaultDisabledSprite = targetButtonClicky.DisabledSprite;
        }

        _hasDefaults = true;
        _hasTargetDefaults = true;
    }
}
