using UnityEngine;
using DG.Tweening;

public class GridLaserGate : MonoBehaviour
{
    public Color gateColor = Color.white;
    private bool _isOpen = false;
    private bool _isSubscribed = false;
    private Coroutine _subscribeRoutine;
    private GridManager _subscribedManager;

    private void Start()
    {
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        if (GridManager.Instance != null) GridManager.Instance.GateMap[pos] = this;

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
            _subscribedManager.OnKeyCollectedEvent -= TryOpenGate;
            _isSubscribed = false;
        }

        if (_isSubscribed && _subscribedManager == manager) return;

        manager.OnKeyCollectedEvent += TryOpenGate;
        _subscribedManager = manager;
        _isSubscribed = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning($"[GridLaserGate] Subscribed gateColor={gateColor} gateId={GetInstanceID()} managerId={manager.GetInstanceID()}");
#endif
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

        if (_subscribedManager != null) _subscribedManager.OnKeyCollectedEvent -= TryOpenGate;
        _subscribedManager = null;
        _isSubscribed = false;
    }

    private void TryOpenGate(Color collectedColor)
    {

        Debug.LogWarning($"Gate {gateColor} received key event with color {collectedColor}");
        if (_isOpen) return;

        if (MatchesColor(collectedColor)) 
        {
            _isOpen = true;
            PlayInstantOpen();
        }
    }

    public bool MatchesColor(Color collectedColor)
    {
        float rDiff = Mathf.Abs(collectedColor.r - gateColor.r);
        float gDiff = Mathf.Abs(collectedColor.g - gateColor.g);
        float bDiff = Mathf.Abs(collectedColor.b - gateColor.b);
        return rDiff < 0.1f && gDiff < 0.1f && bDiff < 0.1f;
    }

    public bool TryReserveForCardGateEffect()
    {
        if (_isOpen) return false;
        _isOpen = true;
        return true;
    }

    public void RemoveAfterCardGateEffect(bool destroyGate, bool disableGate)
    {
        RemoveFromGateMap();

        if (destroyGate)
        {
            Destroy(gameObject);
            return;
        }

        if (disableGate) gameObject.SetActive(false);
    }

    private void PlayInstantOpen()
    {
        RemoveFromGateMap();
        transform.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() => Destroy(gameObject));
    }

    private void RemoveFromGateMap()
    {
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        if (GridManager.Instance != null && GridManager.Instance.GateMap.ContainsKey(pos))
        {
            GridManager.Instance.GateMap.Remove(pos);
        }
    }
}
