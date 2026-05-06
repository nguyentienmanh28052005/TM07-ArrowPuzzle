using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(GridDeflector))]
public class GridDeflectorVisual : MonoBehaviour
{
    [Header("Spawn Effect")]
    [SerializeField] private float spawnDuration = 0.22f;
    [SerializeField] private Ease spawnEase = Ease.OutBack;

    [Header("End Game Vanish")]
    [SerializeField] private float endVanishDuration = 0.28f;
    [SerializeField] private Ease endVanishEase = Ease.InBack;

    private static readonly HashSet<GridDeflectorVisual> ActiveDeflectors = new HashSet<GridDeflectorVisual>();

    [SerializeField] private SpriteRenderer targetRenderer;

    private Vector3 _baseScale;
    private float _baseAlpha = 1f;
    private bool _isVanishing;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        _baseScale = transform.localScale;
        if (targetRenderer != null)
        {
            _baseAlpha = targetRenderer.color.a;
        }
    }

    private void OnEnable()
    {
        ActiveDeflectors.Add(this);
        PlaySpawnEffect();
    }

    private void OnDisable()
    {
        ActiveDeflectors.Remove(this);
        transform.DOKill();
        if (targetRenderer != null)
        {
            targetRenderer.DOKill();
        }
    }

    private void PlaySpawnEffect()
    {
        if (targetRenderer == null) return;

        _isVanishing = false;

        transform.DOKill();
        targetRenderer.DOKill();

        transform.localScale = Vector3.zero;

        Color color = targetRenderer.color;
        color.a = 0f;
        targetRenderer.color = color;

        transform.DOScale(_baseScale, spawnDuration)
            .SetEase(spawnEase)
            .SetLink(gameObject);

        targetRenderer.DOFade(_baseAlpha, spawnDuration * 0.9f)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    private float PlayEndGameVanish(float delay)
    {
        if (targetRenderer == null || !isActiveAndEnabled) return 0f;
        if (_isVanishing) return delay + endVanishDuration;

        _isVanishing = true;

        transform.DOKill();
        targetRenderer.DOKill();

        transform.localScale = _baseScale;

        Color color = targetRenderer.color;
        color.a = _baseAlpha;
        targetRenderer.color = color;

        transform.DOScale(0f, endVanishDuration)
            .SetEase(endVanishEase)
            .SetDelay(delay)
            .SetLink(gameObject);

        targetRenderer.DOFade(0f, endVanishDuration * 0.9f)
            .SetEase(Ease.InQuad)
            .SetDelay(delay)
            .SetLink(gameObject);

        return delay + endVanishDuration;
    }

    public static float PlayEndGameVanishAll(float stepDelay = 0.03f)
    {
        if (ActiveDeflectors.Count == 0) return 0f;

        GridDeflectorVisual[] visuals = new GridDeflectorVisual[ActiveDeflectors.Count];
        ActiveDeflectors.CopyTo(visuals);

        float maxDuration = 0f;
        int index = 0;
        for (int i = 0; i < visuals.Length; i++)
        {
            GridDeflectorVisual visual = visuals[i];
            if (visual == null) continue;

            float doneAt = visual.PlayEndGameVanish(index * stepDelay);
            if (doneAt > maxDuration) maxDuration = doneAt;
            index++;
        }

        return maxDuration;
    }

    public static void ClearAll()
    {
        ActiveDeflectors.Clear();
    }
}
