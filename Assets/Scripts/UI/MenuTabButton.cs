using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MenuTabButton : MonoBehaviour
{
    [SerializeField] private RectTransform iconRoot; 
    [SerializeField] private Graphic iconGraphic;
    [SerializeField] private string iconNameHint = "icon";

    [Header("Animation Settings")]
    [SerializeField] private float punchScaleAmount = 0.2f; 
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private float selectedScale = 1.15f;
    [SerializeField] private float unselectedScale = 0.95f;
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    private Tween _scaleTween;
    private Tween _colorTween;
    private Tween _punchTween;
    private Selectable _selectable;

    private void ResolveReferencesIfNeeded()
    {
        if (iconRoot == null)
        {
            iconRoot = transform as RectTransform;
        }

        if (iconGraphic == null && iconRoot != null)
        {
            // Auto-pick the best icon graphic.
            // Home tab often has multiple graphics (background + icon). Picking the wrong one causes inverted brightness.
            Graphic[] graphics = iconRoot.GetComponentsInChildren<Graphic>(true);

            Graphic best = null;
            int bestScore = int.MinValue;
            float bestArea = float.MaxValue;

            string hint = string.IsNullOrWhiteSpace(iconNameHint) ? "icon" : iconNameHint;
            hint = hint.ToLowerInvariant();

            for (int i = 0; i < graphics.Length; i++)
            {
                Graphic g = graphics[i];
                if (g == null) continue;
                if (g.transform == iconRoot) continue;

                // Avoid graphics controlled by button background scripts/transitions.
                if (g.GetComponent<Selectable>() != null) continue;
                if (g.GetComponent<ButtonClicky>() != null) continue;

                int score = 0;
                string n = g.gameObject.name;
                if (!string.IsNullOrEmpty(n) && n.ToLowerInvariant().Contains(hint)) score += 100;

                if (g is Image img && img.sprite != null) score += 25;
                if (!g.raycastTarget) score += 10;

                RectTransform rt = g.transform as RectTransform;
                float area = 0f;
                if (rt != null)
                {
                    area = Mathf.Abs(rt.rect.width * rt.rect.height);
                    // Smaller graphics are more likely to be the icon.
                    score += Mathf.Clamp(1000 - Mathf.RoundToInt(area), -500, 500);
                }

                if (score > bestScore || (score == bestScore && area < bestArea))
                {
                    best = g;
                    bestScore = score;
                    bestArea = area;
                }
            }

            iconGraphic = best;

            // Last resort: use whatever is on root.
            if (iconGraphic == null)
            {
                iconGraphic = iconRoot.GetComponent<Graphic>();
            }
        }

        if (_selectable == null)
        {
            _selectable = GetComponent<Selectable>();
        }

        NormalizeSelectableTint();
    }

    // Keep Unity's Selectable tint neutral so custom selected/unselected colors stay accurate.
    private void NormalizeSelectableTint()
    {
        if (_selectable == null) return;

        // We animate this tab manually, so Unity's built-in transition can be safely disabled.
        if (_selectable.transition != Selectable.Transition.None)
        {
            _selectable.transition = Selectable.Transition.None;
        }

        if (_selectable.transition != Selectable.Transition.ColorTint) return;

        ColorBlock colors = _selectable.colors;
        bool changed = false;

        if (colors.normalColor != Color.white) { colors.normalColor = Color.white; changed = true; }
        if (colors.highlightedColor != Color.white) { colors.highlightedColor = Color.white; changed = true; }
        if (colors.pressedColor != Color.white) { colors.pressedColor = Color.white; changed = true; }
        if (colors.selectedColor != Color.white) { colors.selectedColor = Color.white; changed = true; }
        if (colors.disabledColor != Color.white) { colors.disabledColor = Color.white; changed = true; }
        if (colors.colorMultiplier != 1f) { colors.colorMultiplier = 1f; changed = true; }
        if (colors.fadeDuration != 0f) { colors.fadeDuration = 0f; changed = true; }

        if (changed)
        {
            _selectable.colors = colors;
        }
    }

    public void SetSelected(bool isSelected, bool instant = false)
    {
        ResolveReferencesIfNeeded();
        if (iconRoot == null) return;

        float targetScale = isSelected ? selectedScale : unselectedScale;
        Color targetColor = isSelected ? selectedColor : unselectedColor;

        _scaleTween?.Kill();
        _colorTween?.Kill();
        _punchTween?.Kill();
        iconRoot.DOKill();

        if (instant)
        {
            iconRoot.localScale = Vector3.one * targetScale;
            if (iconGraphic != null)
            {
                iconGraphic.color = targetColor;
            }
            return;
        }

        _scaleTween = iconRoot.DOScale(targetScale, duration).SetEase(Ease.OutQuad).SetUpdate(true);
        if (iconGraphic != null)
        {
            _colorTween = iconGraphic.DOColor(targetColor, duration).SetEase(Ease.OutQuad).SetUpdate(true);
        }
    }

    /// <summary>
    /// Kích hoạt hoạt ảnh nảy (Punch) khi Tab này được chọn trên Menu.
    /// </summary>
    public void PlaySelectAnimation()
    {
        ResolveReferencesIfNeeded();
        if (iconRoot == null) return;

        _scaleTween?.Kill();
        _punchTween?.Kill();
        iconRoot.DOKill();
        iconRoot.localScale = Vector3.one * selectedScale;

        _punchTween = iconRoot.DOPunchScale(Vector3.one * punchScaleAmount, duration, 10, 1)
            .SetUpdate(true); 
    }
}