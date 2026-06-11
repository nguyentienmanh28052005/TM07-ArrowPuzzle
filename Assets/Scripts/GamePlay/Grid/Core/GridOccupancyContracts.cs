using UnityEngine;

public interface IGridOccupant
{
    Vector2Int GridPosition { get; }
    bool IsActiveOccupant { get; }
}

public interface IGridTrigger : IGridOccupant
{
    void TriggerFromGrid();
}

public interface IArrowExitListener
{
    void OnArrowExited();
}
