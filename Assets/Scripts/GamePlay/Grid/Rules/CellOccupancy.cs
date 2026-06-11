using UnityEngine;

public sealed class CellOccupancy
{
    private readonly GridManager _grid;

    public bool IsValid => _grid != null;

    public CellOccupancy(GridManager grid)
    {
        _grid = grid;
    }

    public SnakeBlock GetSnakeAt(Vector2Int cell)
    {
        return _grid != null ? _grid.GetSnakeAt(cell) : null;
    }

    public bool HasGateAt(Vector2Int cell)
    {
        return _grid != null && _grid.HasGateAt(cell);
    }

    public bool HasElectricWallAt(Vector2Int cell)
    {
        return _grid != null && _grid.HasElectricWallAt(cell);
    }

    public bool HasCountdownBlockAt(Vector2Int cell)
    {
        return _grid != null && _grid.HasActiveCountdownBlockAt(cell);
    }

    public bool TryGetStopBlockAt(Vector2Int cell, out GridStopBlock block)
    {
        block = null;
        return _grid != null && _grid.TryGetActiveStopBlockAt(cell, out block);
    }

    public bool HasArrowShadowAt(Vector2Int cell)
    {
        return _grid != null && _grid.HasActiveArrowShadowAt(cell);
    }

    public bool HasBlockingTurnStateBlockAt(Vector2Int cell)
    {
        return _grid != null && _grid.HasBlockingTurnStateBlockAt(cell);
    }

    public bool TryGetBlackHoleAt(Vector2Int cell, out GridBlackHole blackHole)
    {
        blackHole = null;
        return _grid != null && _grid.TryGetBlackHoleAt(cell, out blackHole);
    }

    public bool TryGetPortalLink(Vector2Int cell, out GridManager.PortalLink link)
    {
        link = default(GridManager.PortalLink);
        return _grid != null && _grid.TryGetPortalLink(cell, out link);
    }

    public bool TryGetDeflectorAt(Vector2Int cell, out GridDeflector deflector)
    {
        deflector = null;
        return _grid != null && _grid.TryGetDeflectorAt(cell, out deflector);
    }
}
