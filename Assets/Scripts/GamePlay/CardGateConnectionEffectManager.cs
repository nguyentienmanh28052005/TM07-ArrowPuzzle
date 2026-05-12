using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CardGateConnectionEffectManager : MonoBehaviour
{
    private struct ConnectionKey : IEquatable<ConnectionKey>
    {
        public readonly int CardId;
        public readonly int GateId;

        public ConnectionKey(GridKeycard card, GridLaserGate gate)
        {
            CardId = card != null ? card.GetInstanceID() : 0;
            GateId = gate != null ? gate.GetInstanceID() : 0;
        }

        public bool Equals(ConnectionKey other)
        {
            return CardId == other.CardId && GateId == other.GateId;
        }

        public override bool Equals(object obj)
        {
            return obj is ConnectionKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (CardId * 397) ^ GateId;
            }
        }
    }

    private sealed class ConnectionWire
    {
        public GridKeycard Card;
        public GridLaserGate Gate;
        public LineRenderer Line;
        public bool IsAnimating;
    }

    public static CardGateConnectionEffectManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private LineRenderer lineRendererPrefab;
    [SerializeField] private GameObject travelingPulsePrefab;
    [SerializeField] private GameObject gateDisappearParticlePrefab;

    [Header("Timing")]
    [SerializeField] private float cardPulseDuration = 0.18f;
    [SerializeField] private float wireDrawDuration = 0.18f;
    [SerializeField] private float pulseTravelDuration = 0.28f;
    [SerializeField] private float gateDisappearDuration = 0.25f;

    [Header("Animation")]
    [SerializeField] private AnimationCurve wireDrawCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve gateScaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private float cardPulseScale = 1.18f;
    [SerializeField] private float gateFlashIntensity = 1.75f;

    [Header("Neon")]
    [SerializeField] private Gradient neonGradient;
    [SerializeField] private Color fallbackNeonColor = Color.white;
    [SerializeField] private float neonIntensity = 2.5f;
    [SerializeField] private Material wireMaterial;
    [SerializeField] private float wireStartWidth = 0.07f;
    [SerializeField] private float wireEndWidth = 0.035f;
    [SerializeField] private float activeWireWidthMultiplier = 1.65f;

    [Header("Removal")]
    [SerializeField] private bool destroyGateAfterEffect = true;
    [SerializeField] private bool disableGateAfterEffect = false;

    [Header("Hierarchy")]
    [SerializeField] private Transform effectRoot;

    private readonly HashSet<int> _activeGateIds = new HashSet<int>();
    private readonly Dictionary<ConnectionKey, ConnectionWire> _wires = new Dictionary<ConnectionKey, ConnectionWire>();
    private readonly HashSet<ConnectionKey> _seenConnections = new HashSet<ConnectionKey>();
    private readonly List<ConnectionKey> _staleConnections = new List<ConnectionKey>();
    private Sprite _fallbackPulseSprite;

    public bool DestroyGateAfterEffect => destroyGateAfterEffect;
    public bool DisableGateAfterEffect => disableGateAfterEffect;
    public float CardPulseDuration => cardPulseDuration;

    public static CardGateConnectionEffectManager GetOrCreateDefault()
    {
        if (Instance != null) return Instance;

        CardGateConnectionEffectManager existing = FindObjectOfType<CardGateConnectionEffectManager>();
        if (existing != null) return existing;

        GameObject managerObject = new GameObject("CardGateConnectionEffectManager_Runtime");
        return managerObject.AddComponent<CardGateConnectionEffectManager>();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            EnsureDefaultGradient();
            return;
        }

        if (Instance != this) Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void LateUpdate()
    {
        SyncPersistentWires();
    }

    public void PlayEffect(GridKeycard card, List<GridLaserGate> gates, Action<GridLaserGate> onGateShouldDisappear)
    {
        if (card == null) return;

        Color effectColor = GetEffectColor(card.keyColor);
        StartCoroutine(PlayCardPulse(card.transform, effectColor));

        if (gates == null || gates.Count == 0) return;

        foreach (GridLaserGate gate in gates)
        {
            if (gate == null || !gate.isActiveAndEnabled) continue;
            if (_activeGateIds.Contains(gate.GetInstanceID())) continue;
            if (!gate.TryReserveForCardGateEffect()) continue;

            _activeGateIds.Add(gate.GetInstanceID());
            ConnectionWire wire = EnsureWire(card, gate, effectColor);
            if (wire != null) wire.IsAnimating = true;
            StartCoroutine(PlayConnectionSequence(card.transform, gate, effectColor, onGateShouldDisappear, wire));
        }
    }

    private IEnumerator PlayConnectionSequence(Transform cardTransform, GridLaserGate gate, Color effectColor, Action<GridLaserGate> onGateShouldDisappear, ConnectionWire wire)
    {
        int gateId = gate != null ? gate.GetInstanceID() : 0;
        GameObject pulse = null;

        if (cardTransform != null && gate != null)
        {
            Vector3 start = cardTransform.position;
            Vector3 end = gate.transform.position;

            if (wire != null && wire.Line != null)
            {
                yield return HighlightWire(wire.Line);
            }

            pulse = CreatePulse(effectColor, start);
            yield return AnimatePulse(pulse, start, end);
        }

        if (pulse != null) Destroy(pulse);

        if (gate != null)
        {
            yield return PlayGateDisappear(gate, effectColor);
            // Existing Gate removal logic is called here, after the visual effect finishes.
            onGateShouldDisappear?.Invoke(gate);
        }

        if (wire != null)
        {
            wire.IsAnimating = false;
            RemoveWire(new ConnectionKey(wire.Card, wire.Gate));
        }

        if (gateId != 0) _activeGateIds.Remove(gateId);
    }

    private IEnumerator HighlightWire(LineRenderer line)
    {
        if (line == null) yield break;

        float duration = Mathf.Max(0.01f, wireDrawDuration);
        float originalStartWidth = line.startWidth;
        float originalEndWidth = line.endWidth;
        float boostedStartWidth = wireStartWidth * activeWireWidthMultiplier;
        float boostedEndWidth = wireEndWidth * activeWireWidthMultiplier;

        Tween tween = DOTween.To(() => 0f, value =>
        {
            line.startWidth = Mathf.Lerp(originalStartWidth, boostedStartWidth, value);
            line.endWidth = Mathf.Lerp(originalEndWidth, boostedEndWidth, value);
        }, 1f, duration);

        if (wireDrawCurve != null) tween.SetEase(wireDrawCurve);
        else tween.SetEase(Ease.Linear);

        yield return tween.WaitForCompletion();
    }

    private IEnumerator AnimatePulse(GameObject pulse, Vector3 start, Vector3 end)
    {
        if (pulse == null) yield break;

        float duration = Mathf.Max(0.01f, pulseTravelDuration);
        pulse.transform.position = start;
        yield return pulse.transform.DOMove(end, duration).SetEase(Ease.Linear).WaitForCompletion();
        pulse.transform.position = end;
    }

    private IEnumerator PlayCardPulse(Transform target, Color effectColor)
    {
        if (target == null) yield break;

        Vector3 originalScale = target.localScale;
        SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>(true);
        Color[] originalColors = CaptureColors(renderers);
        float duration = Mathf.Max(0.01f, cardPulseDuration);
        float progress = 0f;

        Tween tween = DOTween.To(() => progress, value =>
        {
            progress = value;
            float pulse = Mathf.Sin(progress * Mathf.PI);
            target.localScale = originalScale * Mathf.Lerp(1f, cardPulseScale, pulse);
            ApplyFlash(renderers, originalColors, effectColor, pulse);
        }, 1f, duration).SetEase(Ease.Linear);

        yield return tween.WaitForCompletion();

        target.localScale = originalScale;
        RestoreColors(renderers, originalColors);
    }

    private IEnumerator PlayGateDisappear(GridLaserGate gate, Color effectColor)
    {
        if (gate == null) yield break;

        Transform target = gate.transform;
        Vector3 originalScale = target.localScale;
        SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>(true);
        Color[] originalColors = CaptureColors(renderers);

        SpawnGateParticles(gate.transform.position, effectColor);

        float duration = Mathf.Max(0.01f, gateDisappearDuration);
        float progress = 0f;
        Tween tween = null;

        tween = DOTween.To(() => progress, value =>
        {
            if (gate == null)
            {
                tween?.Kill();
                return;
            }

            progress = value;
            float scaleRatio = gateScaleCurve != null ? gateScaleCurve.Evaluate(progress) : 1f - progress;
            float flash = Mathf.Clamp01(1f - progress) * gateFlashIntensity;

            target.localScale = originalScale * Mathf.Max(0f, scaleRatio);
            ApplyFadeAndFlash(renderers, originalColors, effectColor, progress, flash);
        }, 1f, duration).SetEase(Ease.Linear);

        yield return tween.WaitForCompletion();

        if (gate != null)
        {
            target.localScale = Vector3.zero;
            ApplyAlpha(renderers, 0f);
        }
    }

    private LineRenderer CreateWire(Color effectColor)
    {
        LineRenderer line = lineRendererPrefab != null
            ? Instantiate(lineRendererPrefab, GetRoot())
            : new GameObject("Card Gate Neon Wire").AddComponent<LineRenderer>();

        if (lineRendererPrefab == null) line.transform.SetParent(GetRoot(), true);

        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = wireStartWidth;
        line.endWidth = wireEndWidth;
        line.numCapVertices = 4;
        line.numCornerVertices = 4;
        line.sortingOrder = 30;
        line.startColor = effectColor;
        line.endColor = effectColor;

        Material material = wireMaterial != null ? new Material(wireMaterial) : CreateDefaultWireMaterial();
        ApplyMaterialColor(material, effectColor);
        line.material = material;

        return line;
    }

    private void SyncPersistentWires()
    {
        GridManager manager = GridManager.Instance;
        if (manager == null || manager.KeycardMap == null || manager.GateMap == null) return;

        _seenConnections.Clear();

        foreach (GridKeycard card in manager.KeycardMap.Values)
        {
            if (card == null || !card.isActiveAndEnabled) continue;

            foreach (GridLaserGate gate in manager.GateMap.Values)
            {
                if (gate == null || !gate.isActiveAndEnabled) continue;
                if (!gate.MatchesColor(card.keyColor)) continue;

                ConnectionWire wire = EnsureWire(card, gate, GetEffectColor(card.keyColor));
                if (wire == null) continue;

                _seenConnections.Add(new ConnectionKey(card, gate));
                UpdateWirePosition(wire);
            }
        }

        _staleConnections.Clear();
        foreach (KeyValuePair<ConnectionKey, ConnectionWire> pair in _wires)
        {
            if (_seenConnections.Contains(pair.Key)) continue;
            if (pair.Value != null && pair.Value.IsAnimating) continue;
            _staleConnections.Add(pair.Key);
        }

        for (int i = 0; i < _staleConnections.Count; i++)
        {
            RemoveWire(_staleConnections[i]);
        }
    }

    private ConnectionWire EnsureWire(GridKeycard card, GridLaserGate gate, Color effectColor)
    {
        if (card == null || gate == null) return null;

        ConnectionKey key = new ConnectionKey(card, gate);
        if (_wires.TryGetValue(key, out ConnectionWire existing) && existing != null && existing.Line != null)
        {
            existing.Card = card;
            existing.Gate = gate;
            UpdateWireColor(existing.Line, effectColor);
            UpdateWirePosition(existing);
            return existing;
        }

        LineRenderer line = CreateWire(effectColor);
        ConnectionWire wire = new ConnectionWire
        {
            Card = card,
            Gate = gate,
            Line = line
        };

        _wires[key] = wire;
        UpdateWirePosition(wire);
        return wire;
    }

    private void UpdateWirePosition(ConnectionWire wire)
    {
        if (wire == null || wire.Line == null || wire.Card == null || wire.Gate == null) return;

        wire.Line.positionCount = 2;
        wire.Line.SetPosition(0, wire.Card.transform.position);
        wire.Line.SetPosition(1, wire.Gate.transform.position);
    }

    private void UpdateWireColor(LineRenderer line, Color effectColor)
    {
        if (line == null) return;

        line.startColor = effectColor;
        line.endColor = effectColor;
        if (line.material != null) ApplyMaterialColor(line.material, effectColor);
    }

    private void RemoveWire(ConnectionKey key)
    {
        if (!_wires.TryGetValue(key, out ConnectionWire wire)) return;

        if (wire != null && wire.Line != null)
        {
            Destroy(wire.Line.gameObject);
        }

        _wires.Remove(key);
    }

    private GameObject CreatePulse(Color effectColor, Vector3 position)
    {
        GameObject pulse = travelingPulsePrefab != null
            ? Instantiate(travelingPulsePrefab, position, Quaternion.identity, GetRoot())
            : CreateFallbackPulse(position);

        TintRenderers(pulse, effectColor);
        return pulse;
    }

    private GameObject CreateFallbackPulse(Vector3 position)
    {
        GameObject pulse = new GameObject("Card Gate Energy Pulse");
        pulse.transform.SetParent(GetRoot(), true);
        pulse.transform.position = position;
        pulse.transform.localScale = Vector3.one * 0.28f;

        SpriteRenderer renderer = pulse.AddComponent<SpriteRenderer>();
        renderer.sprite = GetFallbackPulseSprite();
        renderer.color = Color.white;
        renderer.sortingOrder = 20;

        return pulse;
    }

    private void SpawnGateParticles(Vector3 position, Color effectColor)
    {
        if (gateDisappearParticlePrefab == null) return;

        GameObject particles = Instantiate(gateDisappearParticlePrefab, position, Quaternion.identity, GetRoot());
        TintRenderers(particles, effectColor);

        ParticleSystem[] systems = particles.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem system in systems)
        {
            ParticleSystem.MainModule main = system.main;
            main.startColor = effectColor;
            system.Play();
        }

        Destroy(particles, Mathf.Max(1f, gateDisappearDuration + 1f));
    }

    private Transform GetRoot()
    {
        return effectRoot != null ? effectRoot : transform;
    }

    private Color GetEffectColor(Color source)
    {
        Color baseColor = source;
        if (neonGradient != null && neonGradient.colorKeys.Length > 0)
        {
            baseColor = Color.Lerp(baseColor, neonGradient.Evaluate(1f), 0.2f);
        }

        return new Color(baseColor.r * neonIntensity, baseColor.g * neonIntensity, baseColor.b * neonIntensity, baseColor.a);
    }

    private void EnsureDefaultGradient()
    {
        if (neonGradient != null && neonGradient.colorKeys.Length > 0) return;

        neonGradient = new Gradient();
        neonGradient.SetKeys(
            new[]
            {
                new GradientColorKey(fallbackNeonColor, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            });
    }

    private Material CreateDefaultWireMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");

        return shader != null ? new Material(shader) : null;
    }

    private void ApplyMaterialColor(Material material, Color effectColor)
    {
        if (material == null) return;

        if (material.HasProperty("_Color")) material.SetColor("_Color", effectColor);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", effectColor);
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", effectColor);
        }
    }

    private void TintRenderers(GameObject target, Color effectColor)
    {
        if (target == null) return;

        SpriteRenderer[] spriteRenderers = target.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            renderer.color = effectColor;
        }

        LineRenderer[] lineRenderers = target.GetComponentsInChildren<LineRenderer>(true);
        foreach (LineRenderer renderer in lineRenderers)
        {
            renderer.startColor = effectColor;
            renderer.endColor = effectColor;
        }
    }

    private Color[] CaptureColors(SpriteRenderer[] renderers)
    {
        Color[] colors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) colors[i] = renderers[i].color;
        return colors;
    }

    private void RestoreColors(SpriteRenderer[] renderers, Color[] colors)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) renderers[i].color = colors[i];
        }
    }

    private void ApplyFlash(SpriteRenderer[] renderers, Color[] originalColors, Color effectColor, float amount)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            Color original = originalColors[i];
            Color flashed = Color.Lerp(original, effectColor, Mathf.Clamp01(amount));
            flashed.a = original.a;
            renderers[i].color = flashed;
        }
    }

    private void ApplyFadeAndFlash(SpriteRenderer[] renderers, Color[] originalColors, Color effectColor, float fadeRatio, float flashAmount)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            Color original = originalColors[i];
            Color color = Color.Lerp(original, effectColor, Mathf.Clamp01(flashAmount));
            color.a = Mathf.Lerp(original.a, 0f, fadeRatio);
            renderers[i].color = color;
        }
    }

    private void ApplyAlpha(SpriteRenderer[] renderers, float alpha)
    {
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null) continue;
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }

    private Sprite GetFallbackPulseSprite()
    {
        if (_fallbackPulseSprite != null) return _fallbackPulseSprite;

        const int size = 16;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "Runtime Card Gate Pulse";
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / (size * 0.5f);
                float alpha = Mathf.Clamp01(1f - distance);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha));
            }
        }

        texture.Apply();
        _fallbackPulseSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        return _fallbackPulseSprite;
    }
}
