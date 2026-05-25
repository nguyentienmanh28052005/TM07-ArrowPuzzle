using UnityEngine;
using DG.Tweening;

public class GridElectricButton : MonoBehaviour
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
    private Coroutine _registerRoutine;
    private GridManager _registeredManager;
    private SpriteRenderer _spriteRenderer;
    private Vector3 _baseScale = Vector3.one;

    private void Start()
    {
        CacheVisuals();
        TryRegister();
        ApplyColor();
        if (Application.isPlaying) PlaySpawnEffect();
    }

    private void OnEnable()
    {
        CacheVisuals();
        TryRegister();
        ApplyColor();
        if (Application.isPlaying) PlaySpawnEffect();
    }

    private void OnDisable()
    {
        transform.DOKill();
        if (_spriteRenderer != null) _spriteRenderer.DOKill();

        if (_registerRoutine != null)
        {
            StopCoroutine(_registerRoutine);
            _registerRoutine = null;
        }
        Unregister();
    }

    public void Press()
    {
        if (_isPressed) return;
        _isPressed = true;

        Unregister();

        if (GridManager.Instance != null)
            GridManager.Instance.RaiseElectricButtonPressed(buttonColor);

        PlayPressedEffect();
    }

    public void SetColor(Color color)
    {
        buttonColor = color;
        ApplyColor();
    }

    private void ApplyColor()
    {
        CacheVisuals();
        if (_spriteRenderer != null) _spriteRenderer.color = buttonColor;
    }

    private void CacheVisuals()
    {
        if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_baseScale == Vector3.one && transform.localScale != Vector3.zero)
            _baseScale = transform.localScale;
    }

    private void PlaySpawnEffect()
    {
        if (_spawnPlayed || _isPressed) return;
        _spawnPlayed = true;

        CacheVisuals();
        transform.DOKill();
        if (_spriteRenderer != null) _spriteRenderer.DOKill();

        transform.localScale = Vector3.zero;
        if (_spriteRenderer != null) _spriteRenderer.color = WithAlpha(buttonColor, 0f);

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(_baseScale * spawnPopScale, spawnDuration).SetEase(Ease.OutBack));
        if (_spriteRenderer != null)
            seq.Join(_spriteRenderer.DOColor(flashColor, spawnDuration * 0.65f).SetEase(Ease.OutQuad));

        seq.Append(transform.DOScale(_baseScale, spawnSettleDuration).SetEase(Ease.OutQuad));
        if (_spriteRenderer != null)
            seq.Join(_spriteRenderer.DOColor(buttonColor, spawnSettleDuration).SetEase(Ease.OutQuad));
        seq.SetLink(gameObject);
    }

    private void PlayPressedEffect()
    {
        CacheVisuals();
        transform.DOKill();
        if (_spriteRenderer != null) _spriteRenderer.DOKill();

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(_baseScale * pressPopScale, pressAnticipationDuration).SetEase(Ease.OutQuad));
        if (_spriteRenderer != null)
            seq.Join(_spriteRenderer.DOColor(flashColor, pressAnticipationDuration).SetEase(Ease.OutQuad));

        seq.Append(transform.DOScale(Vector3.zero, pressDisappearDuration).SetEase(Ease.InBack));
        seq.Join(transform.DORotate(new Vector3(0f, 0f, pressRotateAmount), pressDisappearDuration, RotateMode.LocalAxisAdd).SetEase(Ease.InQuad));
        if (_spriteRenderer != null)
            seq.Join(_spriteRenderer.DOColor(WithAlpha(buttonColor, 0f), pressDisappearDuration).SetEase(Ease.InQuad));

        seq.SetLink(gameObject);
        seq.OnComplete(() => Destroy(gameObject));
    }

    private void TryRegister()
    {
        var manager = GridManager.Instance;
        if (manager == null)
        {
            if (_registerRoutine == null) _registerRoutine = StartCoroutine(WaitAndRegister());
            return;
        }

        if (_registeredManager != null && _registeredManager != manager)
        {
            UnregisterFromManager(_registeredManager);
        }

        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        manager.ElectricButtonMap[pos] = this;
        _registeredManager = manager;
    }

    private System.Collections.IEnumerator WaitAndRegister()
    {
        while (GridManager.Instance == null) yield return null;
        _registerRoutine = null;
        TryRegister();
    }

    private void Unregister()
    {
        if (_registeredManager == null) return;
        UnregisterFromManager(_registeredManager);
        _registeredManager = null;
    }

    private void UnregisterFromManager(GridManager manager)
    {
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        if (manager.ElectricButtonMap != null && manager.ElectricButtonMap.TryGetValue(pos, out GridElectricButton existing) && existing == this)
        {
            manager.ElectricButtonMap.Remove(pos);
        }
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
}
