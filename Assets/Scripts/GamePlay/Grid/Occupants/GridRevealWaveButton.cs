using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GridRevealWaveButton : GridOccupantBehaviour, IGridTrigger
{
    public Color buttonColor = new Color(0.25f, 0.85f, 1f, 1f);

    [Header("Spawn/Trigger FX")]
    [SerializeField] private float spawnDuration = 0.18f;
    [SerializeField] private float spawnPopScale = 1.18f;
    [SerializeField] private float triggerPopScale = 1.28f;
    [SerializeField] private float triggerPopDuration = 0.08f;
    [SerializeField] private float triggerDisappearDuration = 0.16f;
    [SerializeField] private Color triggerFlashColor = Color.white;

    [Header("Wave")]
    [SerializeField] private Color waveColor = new Color(0.35f, 0.9f, 1f, 0.85f);
    [SerializeField] private float waveUnitsPerSecond = 18f;
    [SerializeField] private float waveLineWidth = 0.08f;
    [SerializeField] private int waveSegments = 96;
    [SerializeField] private float wavePadding = 3f;
    [SerializeField] private Material waveMaterial;

    [Header("Hidden Arrow Flash")]
    [SerializeField] private float transparentAlphaThreshold = 0.01f;
    [SerializeField] private float revealFlashAlpha = 1f;
    [SerializeField] private float revealFlashDuration = 0.22f;
    [SerializeField] private Color revealFlashTint = Color.white;

    private static Material _fallbackWaveMaterial;

    private bool _isTriggered;
    private bool _spawnPlayed;
    private SpriteRenderer[] _spriteRenderers;
    private Vector3 _baseScale = Vector3.one;

    public override bool IsActiveOccupant => base.IsActiveOccupant && !_isTriggered;

    private class WaveTarget
    {
        public SnakeBlock snake;
        public float distance;
        public bool triggered;
    }

    private readonly List<WaveTarget> _targets = new List<WaveTarget>(32);

    private void Start()
    {
        CacheVisuals();
        RegisterOccupantOrWait();
        ApplyColor();
        if (Application.isPlaying) PlaySpawnEffect();
    }

    private void OnEnable()
    {
        CacheVisuals();
        RegisterOccupantOrWait();
        ApplyColor();
    }

    private void OnDisable()
    {
        transform.DOKill();
        KillRendererTweens();
        StopPendingOccupantRegistration();
        UnregisterOccupant();
    }

    private void OnDestroy()
    {
        UnregisterOccupant();
    }

    private void OnValidate()
    {
        CacheVisuals();
        ApplyColor();
    }

    public void SetColor(Color color)
    {
        buttonColor = color;
        ApplyColor();
    }

    public void Trigger()
    {
        if (_isTriggered) return;
        _isTriggered = true;
        UnregisterOccupant();
        StartCoroutine(TriggerRoutine());
    }

    private IEnumerator TriggerRoutine()
    {
        PlayTriggerDisappear();

        Vector3 center = transform.position;
        center.z = 0f;

        CollectWaveTargets(center);
        float maxRadius = ComputeMaxWaveRadius(center);
        LineRenderer wave = CreateWaveRenderer(center);

        float radius = 0f;
        while (radius < maxRadius)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            radius = Mathf.Min(maxRadius, radius + Mathf.Max(1f, waveUnitsPerSecond) * safeDeltaTime);
            UpdateWaveRenderer(wave, radius);
            TriggerReachedTargets(radius);

            if (wave != null)
            {
                Color color = waveColor;
                color.a = waveColor.a * Mathf.Clamp01(1f - (radius / Mathf.Max(0.01f, maxRadius)));
                wave.startColor = color;
                wave.endColor = color;
            }

            yield return null;
        }

        TriggerReachedTargets(float.MaxValue);
        if (wave != null) Destroy(wave.gameObject);
        Destroy(gameObject);
    }

    private void CollectWaveTargets(Vector3 center)
    {
        _targets.Clear();

        IReadOnlyList<SnakeBlock> snakes = SnakeBlock.ActiveSnakes;
        if (snakes == null) return;

        for (int i = 0; i < snakes.Count; i++)
        {
            SnakeBlock snake = snakes[i];
            if (snake == null || !snake.IsVisualAlphaZero(transparentAlphaThreshold)) continue;

            _targets.Add(new WaveTarget
            {
                snake = snake,
                distance = GetClosestSnakeDistance(snake, center),
                triggered = false
            });
        }
    }

    private void TriggerReachedTargets(float radius)
    {
        for (int i = 0; i < _targets.Count; i++)
        {
            WaveTarget target = _targets[i];
            if (target.triggered || target.snake == null) continue;
            if (radius + 0.001f < target.distance) continue;

            target.triggered = true;
            target.snake.FlashIfVisualAlphaZero(revealFlashAlpha, revealFlashDuration, revealFlashTint, transparentAlphaThreshold);
        }
    }

    private float GetClosestSnakeDistance(SnakeBlock snake, Vector3 center)
    {
        float best = Vector3.Distance(snake.HeadPosition, center);
        List<Vector3> nodes = snake.LogicNodes;
        if (nodes == null) return best;

        for (int i = 0; i < nodes.Count; i++)
        {
            float distance = Vector3.Distance(nodes[i], center);
            if (distance < best) best = distance;
        }

        return best;
    }

    private float ComputeMaxWaveRadius(Vector3 center)
    {
        float maxDistance = 8f;

        if (GridDot.GridMap != null)
        {
            foreach (GridDot dot in GridDot.GridMap.Values)
            {
                if (dot == null) continue;
                maxDistance = Mathf.Max(maxDistance, Vector3.Distance(dot.transform.position, center));
            }
        }

        IReadOnlyList<SnakeBlock> snakes = SnakeBlock.ActiveSnakes;
        if (snakes != null)
        {
            for (int i = 0; i < snakes.Count; i++)
            {
                SnakeBlock snake = snakes[i];
                if (snake == null || snake.LogicNodes == null) continue;
                for (int j = 0; j < snake.LogicNodes.Count; j++)
                {
                    maxDistance = Mathf.Max(maxDistance, Vector3.Distance(snake.LogicNodes[j], center));
                }
            }
        }

        Camera camera = Camera.main;
        if (camera != null)
        {
            float z = Mathf.Abs(camera.transform.position.z - center.z);
            Vector3[] corners =
            {
                camera.ViewportToWorldPoint(new Vector3(0f, 0f, z)),
                camera.ViewportToWorldPoint(new Vector3(0f, 1f, z)),
                camera.ViewportToWorldPoint(new Vector3(1f, 0f, z)),
                camera.ViewportToWorldPoint(new Vector3(1f, 1f, z))
            };

            for (int i = 0; i < corners.Length; i++)
            {
                corners[i].z = center.z;
                maxDistance = Mathf.Max(maxDistance, Vector3.Distance(corners[i], center));
            }
        }

        return maxDistance + Mathf.Max(0f, wavePadding);
    }

    private LineRenderer CreateWaveRenderer(Vector3 center)
    {
        GameObject waveObject = new GameObject("RevealWave");
        waveObject.transform.position = center;

        LineRenderer wave = waveObject.AddComponent<LineRenderer>();
        wave.useWorldSpace = false;
        wave.loop = true;
        wave.positionCount = Mathf.Max(12, waveSegments);
        wave.widthMultiplier = Mathf.Max(0.01f, waveLineWidth);
        wave.numCapVertices = 4;
        wave.numCornerVertices = 4;
        wave.alignment = LineAlignment.View;
        wave.sortingOrder = 200;
        wave.sharedMaterial = waveMaterial != null ? waveMaterial : GetFallbackWaveMaterial();
        wave.startColor = waveColor;
        wave.endColor = waveColor;
        UpdateWaveRenderer(wave, 0.01f);
        return wave;
    }

    private void UpdateWaveRenderer(LineRenderer wave, float radius)
    {
        if (wave == null) return;

        int segments = Mathf.Max(12, waveSegments);
        if (wave.positionCount != segments) wave.positionCount = segments;

        float safeRadius = Mathf.Max(0.01f, radius);
        for (int i = 0; i < segments; i++)
        {
            float angle = (Mathf.PI * 2f * i) / segments;
            wave.SetPosition(i, new Vector3(Mathf.Cos(angle) * safeRadius, Mathf.Sin(angle) * safeRadius, 0f));
        }
    }

    private static Material GetFallbackWaveMaterial()
    {
        if (_fallbackWaveMaterial != null) return _fallbackWaveMaterial;

        Shader shader = Shader.Find("Sprites/Default");
        _fallbackWaveMaterial = shader != null ? new Material(shader) : null;
        return _fallbackWaveMaterial;
    }

    private void CacheVisuals()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        if (_baseScale == Vector3.one && transform.localScale != Vector3.zero)
            _baseScale = transform.localScale;
    }

    private void PlaySpawnEffect()
    {
        if (_spawnPlayed || _isTriggered) return;
        _spawnPlayed = true;

        transform.DOKill();
        KillRendererTweens();
        transform.localScale = Vector3.zero;
        SetRendererColor(WithAlpha(buttonColor, 0f));

        Sequence sequence = DOTween.Sequence().SetLink(gameObject);
        sequence.Append(transform.DOScale(_baseScale * spawnPopScale, spawnDuration).SetEase(Ease.OutBack));
        sequence.Join(RendererColorTween(triggerFlashColor, spawnDuration * 0.55f));
        sequence.Append(transform.DOScale(_baseScale, spawnDuration * 0.35f).SetEase(Ease.OutQuad));
        sequence.Join(RendererColorTween(buttonColor, spawnDuration * 0.35f));
    }

    private void PlayTriggerDisappear()
    {
        transform.DOKill();
        KillRendererTweens();

        Sequence sequence = DOTween.Sequence().SetLink(gameObject);
        sequence.Append(transform.DOScale(_baseScale * triggerPopScale, triggerPopDuration).SetEase(Ease.OutQuad));
        sequence.Join(RendererColorTween(triggerFlashColor, triggerPopDuration));
        sequence.Append(transform.DOScale(Vector3.zero, triggerDisappearDuration).SetEase(Ease.InBack));
        sequence.Join(RendererColorTween(WithAlpha(buttonColor, 0f), triggerDisappearDuration));
    }

    private Tween RendererColorTween(Color color, float duration)
    {
        Sequence sequence = DOTween.Sequence();
        if (_spriteRenderers == null) return sequence;

        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            if (_spriteRenderers[i] == null) continue;
            sequence.Join(_spriteRenderers[i].DOColor(color, Mathf.Max(0.01f, duration)).SetEase(Ease.OutQuad));
        }

        return sequence;
    }

    private void ApplyColor()
    {
        SetRendererColor(buttonColor);
    }

    private void SetRendererColor(Color color)
    {
        if (_spriteRenderers == null) return;
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            if (_spriteRenderers[i] != null) _spriteRenderers[i].color = color;
        }
    }

    private void KillRendererTweens()
    {
        if (_spriteRenderers == null) return;
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            if (_spriteRenderers[i] != null) _spriteRenderers[i].DOKill();
        }
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = Mathf.Clamp01(alpha);
        return color;
    }

    public void TriggerFromGrid()
    {
        Trigger();
    }
}
