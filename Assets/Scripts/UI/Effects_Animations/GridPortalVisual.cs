using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class GridPortalVisual : MonoBehaviour
{
    [Header("Spawn Effect")]
    [SerializeField] private float spawnDuration = 0.22f;
    [SerializeField] private Ease spawnEase = Ease.OutBack;

    [Header("Teleport Pulse")]
    [SerializeField] private float pulseDuration = 0.08f;
    [SerializeField, Range(0f, 1f)] private float pulseMinAlpha = 0.2f;
    [SerializeField, Range(0f, 1f)] private float pulseScaleAmount = 0.16f;

    [Header("Hole Enter Effect")]
    [SerializeField] private float enterEffectDuration = 0.3f;
    [SerializeField] private float enterRippleStartScale = 1.35f;
    [SerializeField] private float enterRippleEndScale = 0.35f;
    [SerializeField, Range(0f, 1f)] private float enterRippleAlpha = 0.5f;
    [SerializeField] private float enterHoleScale = 0.84f;
    [SerializeField] private Color enterEffectColor = new Color(0.35f, 0.75f, 1f, 1f);
    [Tooltip("Material used by the temporary ripple sprite when a snake enters/exits this hole. Leave empty to reuse the hole sprite material.")]
    [SerializeField] private Material holeEffectMaterial;

    [Header("Hole Exit Effect")]
    [SerializeField] private float exitEffectDuration = 0.32f;
    [SerializeField] private float exitRippleStartScale = 0.45f;
    [SerializeField] private float exitRippleEndScale = 1.45f;
    [SerializeField, Range(0f, 1f)] private float exitRippleAlpha = 0.55f;
    [SerializeField] private float exitHolePunchScale = 0.22f;
    [SerializeField] private Color exitEffectColor = new Color(0.7f, 0.95f, 1f, 1f);

    [Header("End Game Vanish")]
    [SerializeField] private float endVanishDuration = 0.28f;
    [SerializeField] private Ease endVanishEase = Ease.InBack;

    private static readonly Dictionary<Vector2Int, GridPortalVisual> _portalByCell = new Dictionary<Vector2Int, GridPortalVisual>();

    private SpriteRenderer _spriteRenderer;
    private Vector3 _baseScale;
    private Quaternion _baseLocalRotation;
    private float _baseAlpha;
    private Vector2Int _cell;
    private bool _isVanishing;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _baseScale = transform.localScale;
        _baseLocalRotation = transform.localRotation;
        _baseAlpha = (_spriteRenderer != null) ? _spriteRenderer.color.a : 1f;
    }

    private void OnEnable()
    {
        _baseLocalRotation = transform.localRotation;
        RegisterCell();
        PlaySpawnEffect();
    }

    private void OnDisable()
    {
        if (_portalByCell.TryGetValue(_cell, out GridPortalVisual current) && current == this)
        {
            _portalByCell.Remove(_cell);
        }
        transform.DOKill();
        if (_spriteRenderer != null) _spriteRenderer.DOKill();
    }

    private void RegisterCell()
    {
        _cell = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        _portalByCell[_cell] = this;
    }

    private void PlaySpawnEffect()
    {
        if (_spriteRenderer == null) return;

        transform.DOKill();
        _spriteRenderer.DOKill();

        transform.localScale = Vector3.zero;
        transform.localRotation = _baseLocalRotation;
        Color c = _spriteRenderer.color;
        c.a = 0f;
        _spriteRenderer.color = c;

        transform.DOScale(_baseScale, spawnDuration).SetEase(spawnEase).SetLink(gameObject);
        _spriteRenderer.DOFade(_baseAlpha, spawnDuration * 0.9f).SetEase(Ease.OutQuad).SetLink(gameObject);
    }

    public void PlayTeleportPulse()
    {
        if (_spriteRenderer == null || !isActiveAndEnabled || _isVanishing) return;

        transform.DOKill();
        _spriteRenderer.DOKill();

        transform.localScale = _baseScale;
        transform.localRotation = _baseLocalRotation;
        Color c = _spriteRenderer.color;
        c.a = _baseAlpha;
        _spriteRenderer.color = c;

        transform.DOPunchScale(Vector3.one * pulseScaleAmount, pulseDuration * 2f, 6, 1f).SetLink(gameObject);
        _spriteRenderer.DOFade(Mathf.Clamp01(pulseMinAlpha), pulseDuration)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo)
            .SetLink(gameObject);
    }

    public void PlayEnterEffect()
    {
        if (_spriteRenderer == null || !isActiveAndEnabled || _isVanishing) return;

        ResetVisualForInteraction();

        Sequence portalSequence = DOTween.Sequence().SetLink(gameObject);
        portalSequence.Join(transform.DOScale(_baseScale * Mathf.Max(0.01f, enterHoleScale), enterEffectDuration * 0.5f)
            .SetEase(Ease.InQuad)
            .SetLoops(2, LoopType.Yoyo));

        SpawnHoleSpriteRipple(enterRippleStartScale, enterRippleEndScale, enterEffectDuration, enterRippleAlpha, enterEffectColor, Ease.InCubic);
    }

    public void PlayExitEffect()
    {
        if (_spriteRenderer == null || !isActiveAndEnabled || _isVanishing) return;

        ResetVisualForInteraction();

        Sequence portalSequence = DOTween.Sequence().SetLink(gameObject);
        portalSequence.Join(transform.DOPunchScale(Vector3.one * Mathf.Max(0.01f, exitHolePunchScale), exitEffectDuration, 8, 0.8f));

        SpawnHoleSpriteRipple(exitRippleStartScale, exitRippleEndScale, exitEffectDuration, exitRippleAlpha, exitEffectColor, Ease.OutCubic);
    }

    public void SetHoleEffectMaterial(Material material)
    {
        holeEffectMaterial = material;
    }

    private void ResetVisualForInteraction()
    {
        transform.DOKill();
        _spriteRenderer.DOKill();

        transform.localScale = _baseScale;
        transform.localRotation = _baseLocalRotation;
        Color color = _spriteRenderer.color;
        color.a = _baseAlpha;
        _spriteRenderer.color = color;
    }

    public static void PlayPulseAtCell(Vector2Int cell)
    {
        if (_portalByCell.TryGetValue(cell, out GridPortalVisual portal) && portal != null)
        {
            portal.PlayTeleportPulse();
        }
    }

    public static void PlayEnterAtCell(Vector2Int cell)
    {
        if (_portalByCell.TryGetValue(cell, out GridPortalVisual portal) && portal != null)
        {
            portal.PlayEnterEffect();
        }
    }

    public static void PlayExitAtCell(Vector2Int cell)
    {
        if (_portalByCell.TryGetValue(cell, out GridPortalVisual portal) && portal != null)
        {
            portal.PlayExitEffect();
        }
    }

    private void SpawnHoleSpriteRipple(float startScale, float endScale, float duration, float alpha, Color color, Ease scaleEase)
    {
        if (_spriteRenderer == null || _spriteRenderer.sprite == null) return;

        Vector3 center = _spriteRenderer.bounds.center;
        Vector3 pivotToCenter = center - _spriteRenderer.transform.position;
        float safeStartScale = Mathf.Max(0.01f, startScale);
        float safeEndScale = Mathf.Max(0.01f, endScale);
        float safeDuration = Mathf.Max(0.01f, duration);

        GameObject rippleObject = new GameObject("HoleSpriteRipple");
        rippleObject.hideFlags = HideFlags.DontSave;
        rippleObject.transform.position = center - (pivotToCenter * safeStartScale);
        rippleObject.transform.rotation = _spriteRenderer.transform.rotation;
        rippleObject.transform.localScale = _spriteRenderer.transform.lossyScale * safeStartScale;

        SpriteRenderer rippleRenderer = rippleObject.AddComponent<SpriteRenderer>();
        rippleRenderer.sprite = _spriteRenderer.sprite;
        rippleRenderer.sharedMaterial = holeEffectMaterial != null ? holeEffectMaterial : _spriteRenderer.sharedMaterial;
        rippleRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
        rippleRenderer.sortingOrder = _spriteRenderer.sortingOrder + 120;

        Color rippleColor = color;
        rippleColor.a = Mathf.Clamp01(alpha);
        rippleRenderer.color = rippleColor;

        Vector3 endPosition = center - (pivotToCenter * safeEndScale);
        Vector3 endWorldScale = _spriteRenderer.transform.lossyScale * safeEndScale;

        Sequence rippleSequence = DOTween.Sequence().SetLink(rippleObject);
        rippleSequence.Join(rippleObject.transform.DOMove(endPosition, safeDuration).SetEase(scaleEase));
        rippleSequence.Join(rippleObject.transform.DOScale(endWorldScale, safeDuration).SetEase(scaleEase));
        rippleSequence.Join(rippleRenderer.DOFade(0f, safeDuration).SetEase(Ease.OutQuad));
        rippleSequence.OnComplete(() =>
        {
            if (rippleObject != null) Destroy(rippleObject);
        });
    }

    private float PlayEndGameVanish(float delay)
    {
        if (_spriteRenderer == null || !isActiveAndEnabled) return 0f;
        if (_isVanishing) return delay + endVanishDuration;

        _isVanishing = true;

        transform.DOKill();
        _spriteRenderer.DOKill();

        transform.localScale = _baseScale;
        transform.localRotation = _baseLocalRotation;
        Color c = _spriteRenderer.color;
        c.a = _baseAlpha;
        _spriteRenderer.color = c;

        transform.DOScale(0f, endVanishDuration)
            .SetEase(endVanishEase)
            .SetDelay(delay)
            .SetLink(gameObject);
        _spriteRenderer.DOFade(0f, endVanishDuration * 0.9f)
            .SetEase(Ease.InQuad)
            .SetDelay(delay)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                if (this != null && gameObject != null) Destroy(gameObject);
            });

        return delay + endVanishDuration;
    }

    public static float PlayEndGameVanishAll(float stepDelay = 0.03f)
    {
        HashSet<GridPortalVisual> uniquePortals = new HashSet<GridPortalVisual>();
        foreach (var kv in _portalByCell)
        {
            if (kv.Value != null) uniquePortals.Add(kv.Value);
        }

        if (uniquePortals.Count == 0) return 0f;

        float maxDuration = 0f;
        int index = 0;
        foreach (GridPortalVisual portal in uniquePortals)
        {
            if (portal == null) continue;
            float doneAt = portal.PlayEndGameVanish(index * stepDelay);
            if (doneAt > maxDuration) maxDuration = doneAt;
            index++;
        }

        return maxDuration;
    }

    public static void ClearAll()
    {
        _portalByCell.Clear();
    }
}
