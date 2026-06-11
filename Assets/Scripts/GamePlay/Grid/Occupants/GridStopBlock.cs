using UnityEngine;
using DG.Tweening;
using TMPro;

public class GridStopBlock : GridOccupantBehaviour, IArrowExitListener
{
    public int count = 3;

    [Header("Visuals")]
    [SerializeField] private TextMeshPro countText;
    [SerializeField] private SpriteRenderer visualRenderer;
    [SerializeField] private Color activatedColor = new Color(1f, 0.78f, 0.25f, 1f);

    [Header("Feedback")]
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private bool shakeCameraOnExplosion = true;
    [SerializeField] private float cameraShakeDuration = 0.2f;
    [SerializeField] private float cameraShakeStrength = 0.65f;
    [SerializeField] private float cameraShakeHitStop = 0f;
    [SerializeField] private Color cameraShakeFlashColor = new Color(1f, 1f, 1f, 0.12f);
    [SerializeField] private AudioClip explosionSound;
    [SerializeField, Range(0f, 1f)] private float explosionSoundVolume = 0.45f;
    [SerializeField, Range(0.1f, 3f)] private float explosionSoundPitch = 1f;

    private bool _isDestroyed;
    private bool _isActivated;
    private bool _isSubscribed;
    private Coroutine _waitForGridRoutine;
    private GridManager _subscribedManager;
    private SnakeBlock _heldSnake;
    private Color _idleColor = Color.white;

    public bool IsDestroyed => _isDestroyed;
    public bool IsActivated => _isActivated;
    public bool CanCapture => !_isDestroyed && !_isActivated && _heldSnake == null;
    public override bool IsActiveOccupant => base.IsActiveOccupant && !_isDestroyed;

    private void Awake()
    {
        CacheVisuals();
    }

    private void Start()
    {
        TryRegister();
        UpdateCountText();
    }

    private void OnEnable()
    {
        TryRegister();
    }

    private void OnDisable()
    {
        if (_waitForGridRoutine != null)
        {
            StopCoroutine(_waitForGridRoutine);
            _waitForGridRoutine = null;
        }

        Unsubscribe();
        StopPendingOccupantRegistration();
        UnregisterFromGrid();
    }

    private void OnDestroy()
    {
        Unsubscribe();
        UnregisterFromGrid();
    }

    public void SetCount(int value)
    {
        count = Mathf.Max(1, value);
        UpdateCountText();
    }

    public bool TryActivate(SnakeBlock snake)
    {
        if (!CanCapture || snake == null) return false;

        _isActivated = true;
        _heldSnake = snake;
        TrySubscribe();
        PlayActivatedFeedback();
        return true;
    }

    public void ClearHeldSnake(SnakeBlock snake)
    {
        if (_heldSnake == snake)
        {
            _heldSnake = null;
        }
    }

    private void CacheVisuals()
    {
        if (countText == null) countText = GetComponentInChildren<TextMeshPro>(true);
        if (visualRenderer == null) visualRenderer = GetComponentInChildren<SpriteRenderer>(true);
        if (visualRenderer != null) _idleColor = visualRenderer.color;
    }

    private void TryRegister()
    {
        RegisterOccupantOrWait();
    }

    private System.Collections.IEnumerator WaitForGridAndRegister()
    {
        while (GridManager.Instance == null) yield return null;
        _waitForGridRoutine = null;
        TryRegister();
        if (_isActivated) TrySubscribe();
    }

    private void UnregisterFromGrid()
    {
        UnregisterOccupant();
    }

    private void TrySubscribe()
    {
        if (!_isActivated) return;

        GridManager manager = GridManager.Instance;
        if (manager == null)
        {
            if (_waitForGridRoutine == null) _waitForGridRoutine = StartCoroutine(WaitForGridAndRegister());
            return;
        }

        if (_subscribedManager != null && _subscribedManager != manager)
        {
            _subscribedManager.UnregisterArrowExitListener(this);
            _isSubscribed = false;
        }

        if (_isSubscribed && _subscribedManager == manager) return;

        manager.RegisterArrowExitListener(this);
        _subscribedManager = manager;
        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed) return;

        if (_subscribedManager != null) _subscribedManager.UnregisterArrowExitListener(this);
        _subscribedManager = null;
        _isSubscribed = false;
    }

    public void OnArrowExited()
    {
        if (_isDestroyed || !_isActivated) return;

        count--;
        UpdateCountText();

        if (count > 0)
        {
            PlayTickFeedback();
            return;
        }

        DestroyStopBlock();
    }

    private void DestroyStopBlock()
    {
        if (_isDestroyed) return;

        _isDestroyed = true;
        UnregisterFromGrid();
        Unsubscribe();

        SnakeBlock snakeToRelease = _heldSnake;
        _heldSnake = null;
        if (snakeToRelease != null)
        {
            snakeToRelease.ReleaseFromStopBlock(this);
        }

        PlayExplosionCameraShake();
        PlayExplosionSound();

        transform.DOKill();
        if (visualRenderer != null) visualRenderer.DOKill();

        Sequence sequence = DOTween.Sequence().SetLink(gameObject);
        sequence.Append(transform.DOScale(transform.localScale * 1.2f, 0.18f).SetEase(Ease.OutBack));
        sequence.Append(transform.DOScale(Vector3.zero, 0.18f).SetEase(Ease.InBack));
        sequence.OnComplete(() =>
        {
            if (visualRenderer != null) visualRenderer.enabled = false;
            if (countText != null) countText.gameObject.SetActive(false);

            if (explosionEffect != null)
            {
                explosionEffect.transform.SetParent(null);
                explosionEffect.transform.localScale = Vector3.one;
                explosionEffect.Play();
                Destroy(explosionEffect.gameObject, 1.5f);
            }

            Destroy(gameObject);
        });
    }

    private void PlayActivatedFeedback()
    {
        CacheVisuals();
        transform.DOKill();
        transform.DOShakePosition(0.18f, 0.08f, 18, 90f, false, true).SetLink(gameObject);

        if (visualRenderer != null)
        {
            visualRenderer.DOKill();
            visualRenderer.DOColor(activatedColor, 0.12f).SetEase(Ease.OutQuad).SetLink(gameObject);
        }
    }

    private void PlayTickFeedback()
    {
        transform.DOKill();
        transform.DOShakePosition(0.16f, 0.08f, 18, 90f, false, true).SetLink(gameObject);

        if (visualRenderer == null) return;

        visualRenderer.DOKill();
        visualRenderer.DOColor(Color.white, 0.08f).OnComplete(() =>
        {
            if (visualRenderer != null)
            {
                Color targetColor = _isActivated ? activatedColor : _idleColor;
                visualRenderer.DOColor(targetColor, 0.1f).SetLink(gameObject);
            }
        }).SetLink(gameObject);
    }

    private void PlayExplosionSound()
    {
        if (explosionSound == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySfx(explosionSound, explosionSoundVolume, explosionSoundPitch);
    }

    private void PlayExplosionCameraShake()
    {
        if (!shakeCameraOnExplosion) return;

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

    private void UpdateCountText()
    {
        CacheVisuals();
        if (countText != null) countText.text = count.ToString();
    }
}
