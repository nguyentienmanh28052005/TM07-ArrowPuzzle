using DG.Tweening;
using UnityEngine;

public class GridTurnStateBlock : GridOccupantBehaviour, IArrowExitListener
{
    [SerializeField] private bool startsRed = true;
    [SerializeField] private Color greenColor = new Color(0.1f, 0.95f, 0.35f, 1f);
    [SerializeField] private Color redColor = new Color(1f, 0.16f, 0.12f, 1f);
    [SerializeField] private float pulseScale = 1.15f;
    [SerializeField] private float pulseDuration = 0.12f;
    [SerializeField] private SpriteRenderer visualRenderer;

    private bool _isRed;
    private GridManager _subscribedManager;
    private Coroutine _waitRoutine;

    public bool IsRed => _isRed;
    public bool IsBlocking => _isRed;
    public bool IsDestroyed => this == null || gameObject == null;
    public override bool IsActiveOccupant => base.IsActiveOccupant && !IsDestroyed;

    private void Start()
    {
        _isRed = startsRed;
        CacheVisual();
        ApplyVisual(false);
        TryRegisterAndSubscribe();
    }

    private void OnEnable()
    {
        _isRed = startsRed;
        CacheVisual();
        ApplyVisual(false);
        TryRegisterAndSubscribe();
    }

    private void OnDisable()
    {
        if (_waitRoutine != null)
        {
            StopCoroutine(_waitRoutine);
            _waitRoutine = null;
        }

        Unsubscribe();
        Unregister();
        transform.DOKill();
        if (visualRenderer != null) visualRenderer.DOKill();
    }

    private void OnDestroy()
    {
        Unsubscribe();
        Unregister();
    }

    public void SetInitialState(bool red)
    {
        startsRed = red;
        _isRed = red;
        ApplyVisual(false);
    }

    private void TryRegisterAndSubscribe()
    {
        GridManager manager = GridManager.Instance;
        if (manager == null)
        {
            if (_waitRoutine == null) _waitRoutine = StartCoroutine(WaitForGridManager());
            return;
        }

        if (_subscribedManager != null && _subscribedManager != manager) Unsubscribe();

        RegisterOccupantOrWait();

        manager.UnregisterArrowExitListener(this);
        manager.RegisterArrowExitListener(this);
        _subscribedManager = manager;
    }

    private System.Collections.IEnumerator WaitForGridManager()
    {
        while (GridManager.Instance == null) yield return null;
        _waitRoutine = null;
        TryRegisterAndSubscribe();
    }

    private void Unregister()
    {
        UnregisterOccupant();
    }

    private void Unsubscribe()
    {
        if (_subscribedManager == null) return;

        _subscribedManager.UnregisterArrowExitListener(this);
        _subscribedManager = null;
    }

    public void OnArrowExited()
    {
        _isRed = !_isRed;
        ApplyVisual(true);
    }

    private void CacheVisual()
    {
        if (visualRenderer == null) visualRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }

    private void ApplyVisual(bool animate)
    {
        CacheVisual();
        Color targetColor = _isRed ? redColor : greenColor;

        if (visualRenderer != null)
        {
            visualRenderer.DOKill();
            if (animate && Application.isPlaying)
                visualRenderer.DOColor(targetColor, pulseDuration).SetEase(Ease.OutQuad).SetLink(gameObject);
            else
                visualRenderer.color = targetColor;
        }

        if (animate && Application.isPlaying)
        {
            transform.DOKill();
            Vector3 baseScale = Vector3.one;
            transform.localScale = baseScale;
            Sequence seq = DOTween.Sequence().SetLink(gameObject);
            seq.Append(transform.DOScale(baseScale * pulseScale, pulseDuration).SetEase(Ease.OutQuad));
            seq.Append(transform.DOScale(baseScale, pulseDuration).SetEase(Ease.OutBack));
        }
    }
}
