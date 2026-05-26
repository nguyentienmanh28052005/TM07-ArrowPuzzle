using System.Collections;
using UnityEngine;

public class GridDeflector : MonoBehaviour
{
    public ArrowDir direction = ArrowDir.Up;

    private GridManager _registeredManager;
    private Coroutine _registerRoutine;
    private Vector2Int _registeredPos;
    private GridDeflectorVisual _visual;

    private void Start()
    {
        EnsureVisual();
        UpdateVisualRotation();
        if (_visual != null) _visual.RefreshDirectionState();
        TryRegister();
    }

    private void OnEnable()
    {
        EnsureVisual();
        UpdateVisualRotation();
        if (_visual != null) _visual.RefreshDirectionState();
        TryRegister();
    }

    private void OnDisable()
    {
        if (_registerRoutine != null)
        {
            StopCoroutine(_registerRoutine);
            _registerRoutine = null;
        }

        Unregister();
    }

    public void SetDirection(ArrowDir dir)
    {
        EnsureVisual();
        direction = dir;
        UpdateVisualRotation();
        if (_visual != null) _visual.RefreshDirectionState();
    }

    public void PlayInteractionFeedback()
    {
        EnsureVisual();
        if (_visual != null)
        {
            _visual.PlayInteractionPulse();
        }
    }

    private void EnsureVisual()
    {
        if (_visual != null) return;

        _visual = GetComponent<GridDeflectorVisual>();
        if (_visual == null) _visual = GetComponentInParent<GridDeflectorVisual>();
        if (_visual == null) _visual = GetComponentInChildren<GridDeflectorVisual>();
        if (_visual == null) _visual = gameObject.AddComponent<GridDeflectorVisual>();
    }

    private void UpdateVisualRotation()
    {
        float angle = 0f;
        switch (direction)
        {
            case ArrowDir.Up: angle = 0f; break;
            case ArrowDir.Down: angle = 180f; break;
            case ArrowDir.Left: angle = 90f; break;
            case ArrowDir.Right: angle = -90f; break;
        }
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
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
            Unregister();
        }

        _registeredPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        manager.DeflectorMap[_registeredPos] = this;
        _registeredManager = manager;
    }

    private IEnumerator WaitAndRegister()
    {
        while (GridManager.Instance == null) yield return null;
        _registerRoutine = null;
        TryRegister();
    }

    private void Unregister()
    {
        if (_registeredManager == null || _registeredManager.DeflectorMap == null) return;

        if (_registeredManager.DeflectorMap.TryGetValue(_registeredPos, out GridDeflector existing) && existing == this)
        {
            _registeredManager.DeflectorMap.Remove(_registeredPos);
        }

        _registeredManager = null;
    }
}
