using UnityEngine;

public class GridDeflector : GridOccupantBehaviour
{
    public ArrowDir direction = ArrowDir.Up;

    private GridDeflectorVisual _visual;

    private void Start()
    {
        EnsureVisual();
        UpdateVisualRotation();
        if (_visual != null) _visual.RefreshDirectionState();
        RegisterOccupantOrWait();
    }

    private void OnEnable()
    {
        EnsureVisual();
        UpdateVisualRotation();
        if (_visual != null) _visual.RefreshDirectionState();
        RegisterOccupantOrWait();
    }

    private void OnDisable()
    {
        StopPendingOccupantRegistration();
        UnregisterOccupant();
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

}
