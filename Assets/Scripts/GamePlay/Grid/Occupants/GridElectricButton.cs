using UnityEngine;
using DG.Tweening;

public class GridElectricButton : GridOccupantBehaviour, IGridTrigger
{
    public Color buttonColor = Color.white;

    [Header("Spawn/Press FX")]
    [SerializeField] private float spawnDuration = 0.18f;
    [SerializeField] private float spawnPopScale = 1.18f;
    [SerializeField] private float spawnSettleDuration = 0.08f;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float pressAnticipationDuration = 0.08f;
    [SerializeField] private float pressPopScale = 1.25f;
    [SerializeField] private float pressDisappearDuration = 0.18f;
    [SerializeField] private float pressRotateAmount = 18f;

    private bool _isPressed;
    private bool _spawnPlayed;
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer[] _spriteRenderers;
    private Vector3 _baseScale = Vector3.one;
    private bool _hasStarted;
    private Sequence _spawnSequence;
    private Sequence _pressSequence;

    public override bool IsActiveOccupant => base.IsActiveOccupant && !_isPressed;

    private void Start()
    {
        _hasStarted = true;
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
        if (_hasStarted && Application.isPlaying) PlaySpawnEffect();
    }

    private void OnDisable()
    {
        transform.DOKill();
        KillSequences();
        KillRendererTweens();

        StopPendingOccupantRegistration();
        UnregisterOccupant();
    }

    public void Press()
    {
        if (_isPressed) return;
        _isPressed = true;

        UnregisterOccupant();

        if (GridManager.Instance != null)
            GridManager.Instance.RaiseElectricButtonPressed(buttonColor);

        PlayPressedEffect();
    }

    public void SetColor(Color color)
    {
        buttonColor = color;
        if (_spawnSequence != null && _spawnSequence.IsActive())
        {
            _spawnSequence.Kill();
            _spawnSequence = null;
            _spawnPlayed = false;
        }
        ApplyColor();
        if (_hasStarted && Application.isPlaying && !_isPressed) PlaySpawnEffect();
    }

    private void ApplyColor()
    {
        CacheVisuals();
        if (_spriteRenderers == null) return;

        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            if (_spriteRenderers[i] != null) _spriteRenderers[i].color = buttonColor;
        }
    }

    private void CacheVisuals()
    {
        if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        if (_baseScale == Vector3.one && transform.localScale != Vector3.zero)
            _baseScale = transform.localScale;
    }

    private void PlaySpawnEffect()
    {
        if (_spawnPlayed || _isPressed) return;
        _spawnPlayed = true;

        CacheVisuals();
        transform.DOKill();
        KillSequences();
        KillRendererTweens();

        transform.localScale = Vector3.zero;
        SetRendererColor(WithAlpha(buttonColor, 0f));

        _spawnSequence = DOTween.Sequence();
        _spawnSequence.Append(transform.DOScale(_baseScale * spawnPopScale, spawnDuration).SetEase(Ease.OutBack));
        AppendRendererColorTween(_spawnSequence, flashColor, spawnDuration * 0.65f, true);

        _spawnSequence.Append(transform.DOScale(_baseScale, spawnSettleDuration).SetEase(Ease.OutQuad));
        AppendRendererColorTween(_spawnSequence, buttonColor, spawnSettleDuration, true);
        _spawnSequence.SetLink(gameObject);
        _spawnSequence.OnComplete(() => _spawnSequence = null);
    }

    private void PlayPressedEffect()
    {
        CacheVisuals();
        transform.DOKill();
        KillSequences();
        KillRendererTweens();

        _pressSequence = DOTween.Sequence();
        _pressSequence.Append(transform.DOScale(_baseScale * pressPopScale, pressAnticipationDuration).SetEase(Ease.OutQuad));
        AppendRendererColorTween(_pressSequence, flashColor, pressAnticipationDuration, true);

        _pressSequence.Append(transform.DOScale(Vector3.zero, pressDisappearDuration).SetEase(Ease.InBack));
        _pressSequence.Join(transform.DORotate(new Vector3(0f, 0f, pressRotateAmount), pressDisappearDuration, RotateMode.LocalAxisAdd).SetEase(Ease.InQuad));
        AppendRendererColorTween(_pressSequence, WithAlpha(buttonColor, 0f), pressDisappearDuration, true);

        _pressSequence.SetLink(gameObject);
        _pressSequence.OnComplete(() => Destroy(gameObject));
    }

    private void SetRendererColor(Color color)
    {
        if (_spriteRenderers == null) return;

        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            if (_spriteRenderers[i] != null) _spriteRenderers[i].color = color;
        }
    }

    private void AppendRendererColorTween(Sequence sequence, Color color, float duration, bool join)
    {
        if (sequence == null || _spriteRenderers == null) return;

        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            if (_spriteRenderers[i] == null) continue;

            Tween tween = _spriteRenderers[i].DOColor(color, duration).SetEase(Ease.OutQuad);
            if (join) sequence.Join(tween);
            else sequence.Append(tween);
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

    private void KillSequences()
    {
        if (_spawnSequence != null && _spawnSequence.IsActive()) _spawnSequence.Kill();
        if (_pressSequence != null && _pressSequence.IsActive()) _pressSequence.Kill();
        _spawnSequence = null;
        _pressSequence = null;
    }

    private void OnValidate()
    {
        CacheVisuals();
        ApplyColor();
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    public void TriggerFromGrid()
    {
        Press();
    }
}
