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

    [Header("End Game Vanish")]
    [SerializeField] private float endVanishDuration = 0.28f;
    [SerializeField] private Ease endVanishEase = Ease.InBack;

    private static readonly Dictionary<Vector2Int, GridPortalVisual> _portalByCell = new Dictionary<Vector2Int, GridPortalVisual>();

    private SpriteRenderer _spriteRenderer;
    private Vector3 _baseScale;
    private float _baseAlpha;
    private Vector2Int _cell;
    private bool _isVanishing;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _baseScale = transform.localScale;
        _baseAlpha = (_spriteRenderer != null) ? _spriteRenderer.color.a : 1f;
    }

    private void OnEnable()
    {
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
        Color c = _spriteRenderer.color;
        c.a = _baseAlpha;
        _spriteRenderer.color = c;

        transform.DOPunchScale(Vector3.one * pulseScaleAmount, pulseDuration * 2f, 6, 1f).SetLink(gameObject);
        _spriteRenderer.DOFade(Mathf.Clamp01(pulseMinAlpha), pulseDuration)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo)
            .SetLink(gameObject);
    }

    public static void PlayPulseAtCell(Vector2Int cell)
    {
        if (_portalByCell.TryGetValue(cell, out GridPortalVisual portal) && portal != null)
        {
            portal.PlayTeleportPulse();
        }
    }

    private float PlayEndGameVanish(float delay)
    {
        if (_spriteRenderer == null || !isActiveAndEnabled) return 0f;
        if (_isVanishing) return delay + endVanishDuration;

        _isVanishing = true;

        transform.DOKill();
        _spriteRenderer.DOKill();

        transform.localScale = _baseScale;
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