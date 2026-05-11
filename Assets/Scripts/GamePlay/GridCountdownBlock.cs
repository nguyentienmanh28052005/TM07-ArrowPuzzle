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

        // Hiệu ứng shake + flash
        transform.DOKill();
        transform.DOShakePosition(0.3f, 0.15f, 20, 90f, false, true).SetLink(gameObject);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.DOKill();
            sr.DOColor(Color.white, 0.1f).OnComplete(() =>
            {
                sr.DOColor(originalColor, 0.2f).SetLink(gameObject);
            }).SetLink(gameObject);
        }

        if (count <= 0)
        {
            _isDestroyed = true;

            // Xóa khỏi GridManager
            Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
            if (GridManager.Instance != null && GridManager.Instance.CountdownBlockMap.ContainsKey(pos))
            {
                GridManager.Instance.CountdownBlockMap.Remove(pos);
            }

            Unsubscribe();

            // Hiệu ứng nổ tung
            transform.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() => Destroy(gameObject));
        }
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
