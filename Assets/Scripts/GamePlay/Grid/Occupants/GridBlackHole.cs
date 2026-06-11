using System.Collections;
using DG.Tweening;
using UnityEngine;

public class GridBlackHole : GridOccupantBehaviour, IArrowExitListener
{
    public ArrowDir direction = ArrowDir.Up;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer visualRenderer;
    [SerializeField] private SpriteRenderer directionRenderer;
    [SerializeField] private Transform directionIndicator;
    [SerializeField] private Color holeColor = new Color(0.04f, 0.04f, 0.06f, 1f);
    [SerializeField] private Color directionColor = new Color(0.35f, 0.85f, 1f, 1f);

    [Header("Feedback")]
    [SerializeField] private float rotateDuration = 0.18f;
    [SerializeField] private float pulseScale = 1.18f;
    [SerializeField] private float pulseDuration = 0.12f;

    private GridManager _subscribedManager;
    private Coroutine _waitRoutine;
    private Vector3 _baseScale = Vector3.one;

    public bool IsDestroyed => this == null || gameObject == null;
    public override bool IsActiveOccupant => base.IsActiveOccupant && !IsDestroyed;

    private void Awake()
    {
        _baseScale = transform.localScale;
        CacheVisuals();
        ApplyVisual(false);
    }

    private void Start()
    {
        TryRegisterAndSubscribe();
    }

    private void OnEnable()
    {
        _baseScale = transform.localScale;
        CacheVisuals();
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
    }

    private void OnDestroy()
    {
        Unsubscribe();
        Unregister();
    }

    private void OnValidate()
    {
        CacheVisuals();
        ApplyVisual(false);
    }

    public void SetDirection(ArrowDir dir)
    {
        direction = dir;
        ApplyVisual(false);
    }

    public bool CanEnter(ArrowDir incomingDirection)
    {
        return direction == GetOppositeDirection(incomingDirection);
    }

    public void PlayEnterFeedback()
    {
        if (!Application.isPlaying) return;

        transform.localScale = _baseScale;

        Sequence seq = DOTween.Sequence().SetLink(gameObject);
        seq.Append(transform.DOScale(_baseScale * pulseScale, pulseDuration).SetEase(Ease.OutQuad));
        seq.Append(transform.DOScale(_baseScale, pulseDuration).SetEase(Ease.OutBack));
    }

    private void TryRegisterAndSubscribe()
    {
        GridManager manager = GridManager.Instance;
        if (manager == null)
        {
            if (Application.isPlaying && _waitRoutine == null) _waitRoutine = StartCoroutine(WaitForGridManager());
            return;
        }

        if (_subscribedManager != null && _subscribedManager != manager) Unsubscribe();

        RegisterOccupantOrWait();

        manager.UnregisterArrowExitListener(this);
        manager.RegisterArrowExitListener(this);
        _subscribedManager = manager;
    }

    private IEnumerator WaitForGridManager()
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
        direction = GetClockwiseDirection(direction);
        ApplyVisual(Application.isPlaying);
    }

    private void CacheVisuals()
    {
        if (visualRenderer == null) visualRenderer = GetComponent<SpriteRenderer>();
        if (directionIndicator == null)
        {
            Transform found = transform.Find("Direction");
            if (found != null) directionIndicator = found;
        }
        if (directionRenderer == null && directionIndicator != null)
        {
            directionRenderer = directionIndicator.GetComponentInChildren<SpriteRenderer>(true);
        }
    }

    private void ApplyVisual(bool animate)
    {
        if (visualRenderer != null) visualRenderer.color = holeColor;
        if (directionRenderer != null) directionRenderer.color = directionColor;

        Quaternion targetRotation = GetRotationForDirection(direction);
        if (Application.isPlaying) transform.DOKill();
        if (animate && Application.isPlaying)
        {
            transform.DORotateQuaternion(targetRotation, rotateDuration).SetEase(Ease.OutBack).SetLink(gameObject);
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }

    private static ArrowDir GetClockwiseDirection(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: return ArrowDir.Right;
            case ArrowDir.Right: return ArrowDir.Down;
            case ArrowDir.Down: return ArrowDir.Left;
            case ArrowDir.Left: return ArrowDir.Up;
            default: return ArrowDir.Up;
        }
    }

    private static ArrowDir GetOppositeDirection(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: return ArrowDir.Down;
            case ArrowDir.Down: return ArrowDir.Up;
            case ArrowDir.Left: return ArrowDir.Right;
            case ArrowDir.Right: return ArrowDir.Left;
            default: return ArrowDir.Up;
        }
    }

    private static Quaternion GetRotationForDirection(ArrowDir dir)
    {
        float angle = 0f;
        switch (dir)
        {
            case ArrowDir.Up: angle = 0f; break;
            case ArrowDir.Down: angle = 180f; break;
            case ArrowDir.Left: angle = 90f; break;
            case ArrowDir.Right: angle = -90f; break;
        }
        return Quaternion.Euler(0f, 0f, angle);
    }
}
