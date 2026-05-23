using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CardGateConnectionEffectManager : MonoBehaviour
{
    public static CardGateConnectionEffectManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject gateDisappearParticlePrefab;

    [Header("Timing")]
    [SerializeField] private float cardPulseDuration = 0.18f;
    [SerializeField] private float gateExplosionAnticipationDuration = 0.6f;
    [SerializeField] private float gateExplosionParticleLifetime = 1.5f;

    [Header("Animation")]
    [SerializeField] private float cardPulseScale = 1.18f;
    [SerializeField] private float gateExplosionScaleMultiplier = 1.2f;
    [SerializeField] private float gateExplosionShakeStrength = 0.15f;
    [SerializeField] private int gateExplosionShakeVibrato = 35;

    [Header("Explosion Camera Shake")]
    [SerializeField] private bool shakeCameraOnGateExplosion = true;
    [SerializeField] private float cameraShakeDuration = 0.6f;
    [SerializeField] private float cameraShakeStrength = 1f;
    [SerializeField] private float cameraShakeHitStop = 0f;
    [SerializeField] private Color cameraShakeFlashColor = new Color(1f, 1f, 1f, 0.22f);

    [Header("Neon")]
    [SerializeField] private Gradient neonGradient;
    [SerializeField] private Color fallbackNeonColor = Color.white;
    [SerializeField] private float neonIntensity = 2.5f;

    [Header("Removal")]
    [SerializeField] private bool destroyGateAfterEffect = true;
    [SerializeField] private bool disableGateAfterEffect = false;

    [Header("Hierarchy")]
    [SerializeField] private Transform effectRoot;

    private readonly HashSet<int> _activeGateIds = new HashSet<int>();

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
            StartCoroutine(PlayGateSequence(gate, effectColor, onGateShouldDisappear));
        }
    }

    private IEnumerator PlayGateSequence(GridLaserGate gate, Color effectColor, Action<GridLaserGate> onGateShouldDisappear)
    {
        int gateId = gate != null ? gate.GetInstanceID() : 0;

        float delay = Mathf.Max(0f, cardPulseDuration);
        if (delay > 0f) yield return new WaitForSeconds(delay);

        if (gate != null)
        {
            Color particleColor = GetGateParticleColor(gate);
            yield return PlayGateDisappear(gate, effectColor, particleColor);
            onGateShouldDisappear?.Invoke(gate);
        }

        if (gateId != 0) _activeGateIds.Remove(gateId);
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

    private IEnumerator PlayGateDisappear(GridLaserGate gate, Color effectColor, Color particleColor)
    {
        if (gate == null) yield break;

        Transform target = gate.transform;
        Vector3 originalScale = target.localScale;
        SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>(true);

        target.DOKill();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            renderers[i].DOKill();
        }

        FlashRenderersToWhite(renderers, gateExplosionAnticipationDuration);

        Sequence explodeSequence = DOTween.Sequence().SetLink(gate.gameObject);
        float anticipationTime = Mathf.Max(0.01f, gateExplosionAnticipationDuration);
        explodeSequence.Append(target.DOScale(originalScale * gateExplosionScaleMultiplier, anticipationTime).SetEase(Ease.InExpo));
        explodeSequence.Join(target.DOShakePosition(anticipationTime, gateExplosionShakeStrength, gateExplosionShakeVibrato, 90f, false, true));

        explodeSequence.OnComplete(() =>
        {
            PlayGateExplosionCameraShake();
            SetSpriteRenderersEnabled(renderers, false);
            SpawnGateParticles(target.position, particleColor);
        });

        yield return explodeSequence.WaitForCompletion();
    }

    private Color GetGateParticleColor(GridLaserGate gate)
    {
        if (gate == null) return Color.white;

        Color color = gate.gateColor;
        color.a = 1f;
        return color;
    }

    private void SpawnGateParticles(Vector3 position, Color effectColor)
    {
        GameObject particles = gateDisappearParticlePrefab != null
            ? Instantiate(gateDisappearParticlePrefab, position, Quaternion.identity, GetRoot())
            : CreateFallbackExplosion(position, effectColor);
        TintRenderers(particles, effectColor);

        ParticleSystem[] systems = particles.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem system in systems)
        {
            system.gameObject.SetActive(true);
            ParticleSystem.MainModule main = system.main;
            main.startColor = effectColor;
            system.Play();
        }

        Destroy(particles, Mathf.Max(1f, gateExplosionParticleLifetime));
    }

    private GameObject CreateFallbackExplosion(Vector3 position, Color effectColor)
    {
        GameObject explosion = new GameObject("Gate Explosion");
        explosion.transform.SetParent(GetRoot(), true);
        explosion.transform.position = position;

        ParticleSystem system = explosion.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = system.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.12f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.65f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.2f, 4.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = effectColor;

        ParticleSystem.EmissionModule emission = system.emission;
        emission.enabled = true;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });

        ParticleSystem.ShapeModule shape = system.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.12f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(effectColor, 0f),
                new GradientColorKey(Color.white, 0.35f),
                new GradientColorKey(effectColor, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.45f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = gradient;

        ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 40;
            renderer.material = CreateParticleMaterial();
        }

        return explosion;
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

    private Material CreateParticleMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");

        return shader != null ? new Material(shader) : null;
    }

    private void TintRenderers(GameObject target, Color effectColor)
    {
        if (target == null) return;

        SpriteRenderer[] spriteRenderers = target.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            renderer.color = effectColor;
        }
    }

    private void FlashRenderersToWhite(SpriteRenderer[] renderers, float duration)
    {
        if (renderers == null) return;

        float safeDuration = Mathf.Max(0.01f, duration);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            renderers[i].DOColor(Color.white, safeDuration).SetLink(renderers[i].gameObject);
        }
    }

    private void SetSpriteRenderersEnabled(SpriteRenderer[] renderers, bool isEnabled)
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) renderers[i].enabled = isEnabled;
        }
    }

    private void PlayGateExplosionCameraShake()
    {
        if (!shakeCameraOnGateExplosion) return;

        ScreenJuiceManager juiceManager = ScreenJuiceManager.Instance;
        if (juiceManager == null) juiceManager = FindObjectOfType<ScreenJuiceManager>();

        if (juiceManager != null)
        {
            juiceManager.PlayCustomJuice(cameraShakeDuration, cameraShakeStrength, cameraShakeHitStop, cameraShakeFlashColor);
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Transform cameraTransform = mainCamera.transform;
        cameraTransform.DOKill();
        Vector3 originalLocalPosition = cameraTransform.localPosition;
        cameraTransform.DOShakePosition(cameraShakeDuration, cameraShakeStrength, 20, 90f, false, true)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (cameraTransform != null) cameraTransform.localPosition = originalLocalPosition;
            });
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

}
