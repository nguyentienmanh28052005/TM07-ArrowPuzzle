using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GridElectricWall : MonoBehaviour
{
    public Color wallColor = Color.white;

    [Header("Endpoints")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private SpriteRenderer startRenderer;
    [SerializeField] private SpriteRenderer endRenderer;

    [Header("VFX")]
    [SerializeField] private LightningConnector lightning;

    private readonly List<Vector2Int> _cells = new List<Vector2Int>();
    private bool _isDisabled;
    private bool _isInitialized;
    private Vector2Int _startCell;
    private Vector2Int _endCell;
    private bool _allowGridRegistration = true;

    private bool _isSubscribed;
    private Coroutine _subscribeRoutine;
    private GridManager _subscribedManager;

    private void Awake()
    {
        EnsureReferences();
    }

    private void Start()
    {
        InitializeFromTransformsIfNeeded(true);
        TrySubscribe();
    }

    private void OnEnable()
    {
        EnsureReferences();
        InitializeFromTransformsIfNeeded(true);
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (_subscribeRoutine != null)
        {
            StopCoroutine(_subscribeRoutine);
            _subscribeRoutine = null;
        }
        UnregisterCells();
        Unsubscribe();
    }

    public void Initialize(Vector2Int startCell, Vector2Int endCell, Color color, bool registerToGrid = true)
    {
        wallColor = color;
        _startCell = startCell;
        _endCell = endCell;
        _isInitialized = true;
        _allowGridRegistration = registerToGrid;

        EnsureReferences();
        SetEndpointPositions();
        ApplyColor();

        if (registerToGrid) RegisterCells();
        else BuildCells();

        if (registerToGrid) TrySubscribe();
    }

    public void SetColor(Color color)
    {
        wallColor = color;
        ApplyColor();
    }

    public bool TryGetEndpoints(out Vector2Int startCell, out Vector2Int endCell)
    {
        if (!_isInitialized) InitializeFromTransformsIfNeeded(false);
        startCell = _startCell;
        endCell = _endCell;
        return _isInitialized;
    }

    public bool ContainsCell(Vector2Int cell)
    {
        if (_cells.Count == 0) BuildCells();
        for (int i = 0; i < _cells.Count; i++)
        {
            if (_cells[i] == cell) return true;
        }
        return false;
    }

    private void InitializeFromTransformsIfNeeded(bool registerToGrid)
    {
        if (_isInitialized) return;
        if (startPoint == null || endPoint == null) return;

        _startCell = new Vector2Int(Mathf.RoundToInt(startPoint.position.x), Mathf.RoundToInt(startPoint.position.y));
        _endCell = new Vector2Int(Mathf.RoundToInt(endPoint.position.x), Mathf.RoundToInt(endPoint.position.y));
        _isInitialized = true;

        ApplyColor();
        if (registerToGrid) RegisterCells();
        else BuildCells();
    }

    private void EnsureReferences()
    {
        if (lightning == null) lightning = GetComponent<LightningConnector>();

        if (startPoint == null)
        {
            GameObject go = new GameObject("ElectricWall_Start");
            go.transform.SetParent(transform, false);
            startPoint = go.transform;
        }

        if (endPoint == null)
        {
            GameObject go = new GameObject("ElectricWall_End");
            go.transform.SetParent(transform, false);
            endPoint = go.transform;
        }

        if (startRenderer == null && startPoint != null) startRenderer = startPoint.GetComponent<SpriteRenderer>();
        if (endRenderer == null && endPoint != null) endRenderer = endPoint.GetComponent<SpriteRenderer>();

        if (lightning != null)
        {
            lightning.SetTargets(startPoint, endPoint);
        }
    }

    private void SetEndpointPositions()
    {
        if (startPoint != null)
            startPoint.position = new Vector3(_startCell.x, _startCell.y, 0f);
        if (endPoint != null)
            endPoint.position = new Vector3(_endCell.x, _endCell.y, 0f);
    }

    private void ApplyColor()
    {
        if (startRenderer != null) startRenderer.color = wallColor;
        if (endRenderer != null) endRenderer.color = wallColor;
        if (lightning != null) lightning.SetColor(Color.white);
    }

    private void RegisterCells()
    {
        if (!_allowGridRegistration)
        {
            BuildCells();
            return;
        }
        UnregisterCells();
        BuildCells();

        if (GridManager.Instance == null) return;

        for (int i = 0; i < _cells.Count; i++)
        {
            GridManager.Instance.ElectricWallMap[_cells[i]] = this;
        }
    }

    private void BuildCells()
    {
        _cells.Clear();

        if (!IsAligned(_startCell, _endCell))
        {
            if (lightning != null) lightning.SetActive(false);
            return;
        }

        int stepX = _startCell.x == _endCell.x ? 0 : (_startCell.x < _endCell.x ? 1 : -1);
        int stepY = _startCell.y == _endCell.y ? 0 : (_startCell.y < _endCell.y ? 1 : -1);

        int length = Mathf.Max(Mathf.Abs(_endCell.x - _startCell.x), Mathf.Abs(_endCell.y - _startCell.y));
        for (int i = 0; i <= length; i++)
        {
            Vector2Int cell = new Vector2Int(_startCell.x + stepX * i, _startCell.y + stepY * i);
            _cells.Add(cell);
        }
    }

    private void UnregisterCells()
    {
        if (GridManager.Instance != null && GridManager.Instance.ElectricWallMap != null)
        {
            for (int i = 0; i < _cells.Count; i++)
            {
                Vector2Int cell = _cells[i];
                if (GridManager.Instance.ElectricWallMap.TryGetValue(cell, out GridElectricWall wall) && wall == this)
                {
                    GridManager.Instance.ElectricWallMap.Remove(cell);
                }
            }
        }
        _cells.Clear();
    }

    private void TrySubscribe()
    {
        if (!_allowGridRegistration) return;
        var manager = GridManager.Instance;
        if (manager == null)
        {
            if (_subscribeRoutine == null) _subscribeRoutine = StartCoroutine(WaitAndSubscribe());
            return;
        }

        if (_subscribedManager != null && _subscribedManager != manager)
        {
            _subscribedManager.OnElectricButtonPressedEvent -= TryDisableWall;
            _isSubscribed = false;
        }

        if (_isSubscribed && _subscribedManager == manager) return;

        manager.OnElectricButtonPressedEvent += TryDisableWall;
        _subscribedManager = manager;
        _isSubscribed = true;

        if (_cells.Count == 0) BuildCells();
        RegisterCells();
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
        if (_subscribedManager != null) _subscribedManager.OnElectricButtonPressedEvent -= TryDisableWall;
        _subscribedManager = null;
        _isSubscribed = false;
    }

    private void TryDisableWall(Color pressedColor)
    {
        if (_isDisabled) return;
        if (!ColorsMatch(pressedColor, wallColor)) return;

        _isDisabled = true;
        UnregisterCells();
        if (lightning != null) lightning.SetActive(false);
        transform.DOScale(0f, 0.25f).SetEase(Ease.InBack).OnComplete(() => Destroy(gameObject));
    }

    private static bool IsAligned(Vector2Int startCell, Vector2Int endCell)
    {
        return startCell.x == endCell.x || startCell.y == endCell.y;
    }

    private static bool ColorsMatch(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.1f
            && Mathf.Abs(a.g - b.g) < 0.1f
            && Mathf.Abs(a.b - b.b) < 0.1f;
    }
}
