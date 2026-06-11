using System.Collections;
using UnityEngine;

public abstract class GridOccupantBehaviour : MonoBehaviour, IGridOccupant
{
    private Coroutine _registerRoutine;

    protected GridManager RegisteredGridManager { get; private set; }
    protected Vector2Int RegisteredGridPosition { get; private set; }

    public virtual Vector2Int GridPosition
    {
        get { return new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)); }
    }

    public virtual bool IsActiveOccupant
    {
        get { return this != null && gameObject != null && isActiveAndEnabled; }
    }

    protected void RegisterOccupantOrWait()
    {
        GridManager manager = GridManager.Instance;
        if (manager == null)
        {
            if (Application.isPlaying && _registerRoutine == null)
                _registerRoutine = StartCoroutine(WaitAndRegisterOccupant());
            return;
        }

        RegisterOccupant(manager);
    }

    protected void UnregisterOccupant()
    {
        if (RegisteredGridManager == null) return;

        RegisteredGridManager.Unregister(this, RegisteredGridPosition);
        RegisteredGridManager = null;
    }

    protected void StopPendingOccupantRegistration()
    {
        if (_registerRoutine == null) return;

        StopCoroutine(_registerRoutine);
        _registerRoutine = null;
    }

    private void RegisterOccupant(GridManager manager)
    {
        Vector2Int position = GridPosition;
        if (RegisteredGridManager != null
            && (RegisteredGridManager != manager || RegisteredGridPosition != position))
        {
            UnregisterOccupant();
        }

        manager.Register(this);
        RegisteredGridManager = manager;
        RegisteredGridPosition = position;
    }

    private IEnumerator WaitAndRegisterOccupant()
    {
        while (GridManager.Instance == null) yield return null;

        _registerRoutine = null;
        RegisterOccupantOrWait();
    }
}
