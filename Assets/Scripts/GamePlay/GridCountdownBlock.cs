using UnityEngine;
using DG.Tweening;
using TMPro;

public class GridCountdownBlock : MonoBehaviour
{
    public int count = 3;

    [Header("Visuals")]
    [SerializeField] private TextMeshPro countText;

    private bool _isDestroyed = false;
    private bool _isSubscribed = false;
    private Coroutine _subscribeRoutine;
    private GridManager _subscribedManager;

    [SerializeField] private ParticleSystem explosionEffect;

    [Header("Explosion Camera Shake")]
    [SerializeField] private bool shakeCameraOnExplosion = true;
    [SerializeField] private float cameraShakeDuration = 0.28f;
    [SerializeField] private float cameraShakeStrength = 0.55f;
    [SerializeField] private float cameraShakeHitStop = 0.03f;
    [SerializeField] private Color cameraShakeFlashColor = new Color(1f, 1f, 1f, 0.22f);

    private void Start()
    {
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        if (GridManager.Instance != null) GridManager.Instance.CountdownBlockMap[pos] = this;

        UpdateCountText();
        TrySubscribe();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (_subscribeRoutine != null)
        {
            StopCoroutine(_subscribeRoutine);
            _subscribeRoutine = null;
        }
        Unsubscribe();
    }

    public void SetCount(int value)
    {
        count = value;
        UpdateCountText();
    }

    private void TrySubscribe()
    {
        var manager = GridManager.Instance;
        if (manager == null)
        {
            if (_subscribeRoutine == null) _subscribeRoutine = StartCoroutine(WaitAndSubscribe());
            return;
        }

        if (_subscribedManager != null && _subscribedManager != manager)
        {
            _subscribedManager.OnArrowExitedEvent -= OnArrowExited;
            _isSubscribed = false;
        }

        if (_isSubscribed && _subscribedManager == manager) return;

        manager.OnArrowExitedEvent += OnArrowExited;
        _subscribedManager = manager;
        _isSubscribed = true;
    }

    private System.Collections.IEnumerator WaitAndSubscribe()
    {
        while (GridManager.Instance == null) yield return null;
        _subscribeRoutine = null;
        TrySubscribe();
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed) return;

        if (_subscribedManager != null) _subscribedManager.OnArrowExitedEvent -= OnArrowExited;
        _subscribedManager = null;
        _isSubscribed = false;
    }

    private void OnArrowExited()
    {
        if (_isDestroyed) return;

        count--;
        UpdateCountText();

        transform.DOKill();
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.DOKill();

        if (count > 0)
        {
            // --- BỊ TRỪ ĐIỂM (CHƯA NỔ) ---
            transform.DOShakePosition(0.2f, 0.1f, 20, 90f, false, true).SetLink(gameObject);
            if (sr != null)
            {
                Color originalColor = sr.color;
                sr.DOColor(Color.white, 0.1f).OnComplete(() =>
                {
                    sr.DOColor(originalColor, 0.1f).SetLink(gameObject);
                }).SetLink(gameObject);
            }
        }
        else
        {
            // --- CHUẨN BỊ NỔ TUNG ---
            _isDestroyed = true;

            // Xóa khỏi hệ thống Grid để các mũi tên khác có thể đi qua ngay
            Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
            if (GridManager.Instance != null && GridManager.Instance.CountdownBlockMap.ContainsKey(pos))
            {
                GridManager.Instance.CountdownBlockMap.Remove(pos);
            }
            Unsubscribe();

            // Chớp trắng liên tục trong suốt quá trình phình to
            if (sr != null) sr.DOColor(Color.white, 0.6f).SetLink(gameObject);

            Sequence explodeSequence = DOTween.Sequence().SetLink(gameObject);

            float anticipationTime = 0.6f; // Thời gian vừa rung vừa phình (0.35 giây)

            // Lệnh Append đầu tiên: Phình to 1.35 lần. 
            // Dùng Ease.InExpo để ban đầu phình chậm, càng về sau phình càng nhanh (tạo cảm giác sắp nổ).
            explodeSequence.Append(transform.DOScale(transform.localScale * 1.2f, anticipationTime).SetEase(Ease.InExpo));
            
            // DÙNG .Join() ĐỂ CHẠY CÙNG LÚC VỚI LỆNH APPEND Ở TRÊN
            // Rung vị trí và rung góc xoay bạo lực hơn (vibrato = 35)
            explodeSequence.Join(transform.DOShakePosition(anticipationTime, 0.15f, 35, 90f, false, true));
            //explodeSequence.Join(transform.DOShakeRotation(anticipationTime, new Vector3(0, 0, 15f), 30));

            // BÙM! 
            explodeSequence.OnComplete(() => 
            {
                PlayExplosionCameraShake();

                // Tắt hình ảnh ngay lập tức
                if (sr != null) sr.enabled = false;
                if (countText != null) countText.gameObject.SetActive(false);

                // Kích hoạt Particle Nổ
                if (explosionEffect != null)
                {
                    explosionEffect.transform.SetParent(null); 
                    explosionEffect.transform.localScale = Vector3.one; 
                    explosionEffect.Play();
                    
                    Destroy(explosionEffect.gameObject, 1.5f);
                }

                // Hủy Game Object
                Destroy(gameObject);
            });
        }
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
        if (countText == null)
        {
            countText = GetComponentInChildren<TextMeshPro>();
        }
        if (countText != null)
        {
            countText.text = count.ToString();
        }
    }
}
