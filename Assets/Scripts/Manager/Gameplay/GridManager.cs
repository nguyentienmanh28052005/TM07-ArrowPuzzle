using UnityEngine;
using System;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public struct PortalLink
    {
        public Vector2Int exit;
        public ArrowDir exitDir;
    }

    private List<IGridOccupant>[] _flatGrid;
    private PortalLink[] _portalGrid;
    private bool[] _hasPortalGrid;
    private int _width;
    private int _height;
    private int _minX;
    private int _maxX;
    private int _minY;
    private int _maxY;

    private readonly HashSet<IArrowExitListener> _arrowExitListeners = new HashSet<IArrowExitListener>();
    private readonly List<IArrowExitListener> _arrowExitDispatchBuffer = new List<IArrowExitListener>(16);

    private event Action<Color> _keyCollectedEvent;
    private event Action<Color> _electricButtonPressedEvent;

    public IEnumerable<GridLaserGate> Gates
    {
        get
        {
            if (_flatGrid == null) yield break;
            var yielded = new HashSet<GridLaserGate>();
            for (int i = 0; i < _flatGrid.Length; i++)
            {
                List<IGridOccupant> cellOccupants = _flatGrid[i];
                if (cellOccupants == null) continue;
                for (int j = 0; j < cellOccupants.Count; j++)
                {
                    if (cellOccupants[j] is GridLaserGate gate && IsActiveEntry(gate))
                    {
                        if (yielded.Add(gate))
                        {
                            yield return gate;
                        }
                    }
                }
            }
        }
    }

    public void RaiseKeyCollected(Color keyColor)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning($"[GridManager] RaiseKeyCollected color={keyColor} instanceId={GetInstanceID()}");
#endif
        _keyCollectedEvent?.Invoke(keyColor);
    }

    public void RegisterKeyCollectedListener(Action<Color> listener)
    {
        if (listener == null) return;
        _keyCollectedEvent -= listener;
        _keyCollectedEvent += listener;
    }

    public void UnregisterKeyCollectedListener(Action<Color> listener)
    {
        if (listener == null) return;
        _keyCollectedEvent -= listener;
    }

    public void RaiseElectricButtonPressed(Color buttonColor)
    {
        _electricButtonPressedEvent?.Invoke(buttonColor);
    }

    public void RegisterElectricButtonPressedListener(Action<Color> listener)
    {
        if (listener == null) return;
        _electricButtonPressedEvent -= listener;
        _electricButtonPressedEvent += listener;
    }

    public void UnregisterElectricButtonPressedListener(Action<Color> listener)
    {
        if (listener == null) return;
        _electricButtonPressedEvent -= listener;
    }

    public void RegisterArrowExitListener(IArrowExitListener listener)
    {
        if (listener == null) return;
        _arrowExitListeners.Add(listener);
    }

    public void UnregisterArrowExitListener(IArrowExitListener listener)
    {
        if (listener == null) return;
        _arrowExitListeners.Remove(listener);
    }

    public void RaiseArrowExited()
    {
        _arrowExitDispatchBuffer.Clear();
        _arrowExitDispatchBuffer.AddRange(_arrowExitListeners);

        for (int i = 0; i < _arrowExitDispatchBuffer.Count; i++)
        {
            IArrowExitListener listener = _arrowExitDispatchBuffer[i];
            if (listener is UnityEngine.Object unityObject && unityObject == null)
            {
                _arrowExitListeners.Remove(listener);
                continue;
            }

            if (listener != null) listener.OnArrowExited();
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void InitializeLevelGrid(LevelDataV2 level)
    {
        if (level == null) return;

        if (LevelDataV2Queries.TryGetBounds(level, out Bounds bounds))
        {
            _minX = Mathf.RoundToInt(bounds.min.x);
            _maxX = Mathf.RoundToInt(bounds.max.x);
            _minY = Mathf.RoundToInt(bounds.min.y);
            _maxY = Mathf.RoundToInt(bounds.max.y);

            _width = _maxX - _minX + 1;
            _height = _maxY - _minY + 1;
        }
        else
        {
            _width = 1;
            _height = 1;
            _minX = 0;
            _maxX = 0;
            _minY = 0;
            _maxY = 0;
        }

        int size = _width * _height;
        _flatGrid = new List<IGridOccupant>[size];
        for (int i = 0; i < size; i++)
        {
            _flatGrid[i] = new List<IGridOccupant>(4);
        }

        _portalGrid = new PortalLink[size];
        _hasPortalGrid = new bool[size];
    }

    private int GetFlatIndex(Vector2Int pos)
    {
        if (_flatGrid == null || pos.x < _minX || pos.x > _maxX || pos.y < _minY || pos.y > _maxY)
            return -1;
        return (pos.y - _minY) * _width + (pos.x - _minX);
    }

    public void RegisterAt(Vector2Int cell, IGridOccupant occupant)
    {
        if (occupant == null) return;
        int index = GetFlatIndex(cell);
        if (index >= 0)
        {
            List<IGridOccupant> cellOccupants = _flatGrid[index];
            if (!cellOccupants.Contains(occupant))
            {
                cellOccupants.Add(occupant);
            }
        }
    }

    public void UnregisterAt(Vector2Int cell, IGridOccupant occupant)
    {
        if (occupant == null) return;
        int index = GetFlatIndex(cell);
        if (index >= 0)
        {
            _flatGrid[index].Remove(occupant);
        }
    }

    public void Register(IGridOccupant occupant)
    {
        if (occupant == null) return;
        RegisterAt(occupant.GridPosition, occupant);
    }

    public void Unregister(IGridOccupant occupant)
    {
        if (occupant == null) return;
        UnregisterAt(occupant.GridPosition, occupant);
    }

    public void Unregister(IGridOccupant occupant, Vector2Int position)
    {
        if (occupant == null) return;
        UnregisterAt(position, occupant);
    }

    public void RegisterSnake(SnakeBlock snake)
    {
        if (snake == null || snake.LogicNodes == null) return;

        for (int i = 0; i < snake.LogicNodes.Count; i++)
        {
            Vector3 nodePos = snake.LogicNodes[i];
            Vector2Int pos = new Vector2Int(Mathf.RoundToInt(nodePos.x), Mathf.RoundToInt(nodePos.y));
            RegisterAt(pos, snake);
        }
    }

    public void RegisterSnakeCells(SnakeBlock snake, IEnumerable<Vector2Int> cells)
    {
        if (snake == null || cells == null) return;

        foreach (Vector2Int cell in cells)
        {
            RegisterAt(cell, snake);
        }
    }

    public void UnregisterSnake(SnakeBlock snake)
    {
        if (snake == null || _flatGrid == null) return;

        for (int i = 0; i < _flatGrid.Length; i++)
        {
            _flatGrid[i].Remove(snake);
        }
    }

    public void UnregisterSnakeCells(SnakeBlock snake, IEnumerable<Vector2Int> cells)
    {
        if (snake == null || cells == null) return;

        foreach (Vector2Int cell in cells)
        {
            UnregisterAt(cell, snake);
        }
    }

    public SnakeBlock GetSnakeAt(Vector2Int pos)
    {
        TryGetSnakeAt(pos, out SnakeBlock snake);
        return snake;
    }

    public bool TryGetSnakeAt(Vector2Int pos, out SnakeBlock snake)
    {
        return TryGetObstacle(pos, out snake);
    }

    public bool TryGetObstacle<T>(Vector2Int pos, out T obstacle) where T : class
    {
        obstacle = null;
        int index = GetFlatIndex(pos);
        if (index < 0) return false;

        List<IGridOccupant> cellOccupants = _flatGrid[index];
        for (int i = cellOccupants.Count - 1; i >= 0; i--)
        {
            IGridOccupant occ = cellOccupants[i];
            if (occ is T target)
            {
                if (IsActiveEntry(occ))
                {
                    obstacle = target;
                    return true;
                }
                else
                {
                    cellOccupants.RemoveAt(i);
                }
            }
        }

        return false;
    }

    public bool TryGetTriggerAt(Vector2Int pos, out IGridTrigger trigger)
    {
        trigger = null;
        int index = GetFlatIndex(pos);
        if (index < 0) return false;

        List<IGridOccupant> cellOccupants = _flatGrid[index];
        for (int i = cellOccupants.Count - 1; i >= 0; i--)
        {
            IGridOccupant occ = cellOccupants[i];
            if (occ is IGridTrigger target)
            {
                if (IsActiveEntry(occ))
                {
                    trigger = target;
                    return true;
                }
                else
                {
                    cellOccupants.RemoveAt(i);
                }
            }
        }

        return false;
    }

    public int TriggerAt(Vector2Int pos)
    {
        int triggerCount = 0;
        int index = GetFlatIndex(pos);
        if (index < 0) return 0;

        List<IGridOccupant> cellOccupants = _flatGrid[index];
        for (int i = cellOccupants.Count - 1; i >= 0; i--)
        {
            IGridOccupant occ = cellOccupants[i];
            if (occ is IGridTrigger trigger)
            {
                if (IsActiveEntry(occ))
                {
                    trigger.TriggerFromGrid();
                    triggerCount++;
                }
                else
                {
                    cellOccupants.RemoveAt(i);
                }
            }
        }

        return triggerCount;
    }

    public bool TryGetKeycardAt(Vector2Int pos, out GridKeycard keycard)
    {
        return TryGetObstacle(pos, out keycard);
    }

    public bool TryGetGateAt(Vector2Int pos, out GridLaserGate gate)
    {
        return TryGetObstacle(pos, out gate);
    }

    public bool HasGateAt(Vector2Int pos)
    {
        return TryGetGateAt(pos, out _);
    }

    public bool TryGetElectricButtonAt(Vector2Int pos, out GridElectricButton button)
    {
        return TryGetObstacle(pos, out button);
    }

    public bool TryGetRevealWaveButtonAt(Vector2Int pos, out GridRevealWaveButton button)
    {
        return TryGetObstacle(pos, out button);
    }

    public bool TryGetElectricWallAt(Vector2Int pos, out GridElectricWall wall)
    {
        return TryGetObstacle(pos, out wall);
    }

    public bool HasElectricWallAt(Vector2Int pos)
    {
        return TryGetElectricWallAt(pos, out _);
    }

    public void RegisterElectricWallCells(GridElectricWall wall, IEnumerable<Vector2Int> cells)
    {
        if (wall == null || cells == null) return;

        foreach (Vector2Int cell in cells)
        {
            RegisterAt(cell, wall);
        }
    }

    public void UnregisterElectricWallCells(GridElectricWall wall, IEnumerable<Vector2Int> cells)
    {
        if (wall == null || cells == null) return;

        foreach (Vector2Int cell in cells)
        {
            UnregisterAt(cell, wall);
        }
    }

    public void RegisterPortalLink(Vector2Int entrance, PortalLink link)
    {
        int index = GetFlatIndex(entrance);
        if (index >= 0)
        {
            _portalGrid[index] = link;
            _hasPortalGrid[index] = true;
        }
    }

    public void RegisterPortalLink(Vector2Int entrance, Vector2Int exit, ArrowDir exitDirection)
    {
        RegisterPortalLink(entrance, new PortalLink { exit = exit, exitDir = exitDirection });
    }

    public bool TryGetPortalLink(Vector2Int pos, out PortalLink link)
    {
        int index = GetFlatIndex(pos);
        if (index >= 0 && _hasPortalGrid[index])
        {
            link = _portalGrid[index];
            return true;
        }
        link = default(PortalLink);
        return false;
    }

    public bool TryGetDeflectorAt(Vector2Int pos, out GridDeflector deflector)
    {
        return TryGetObstacle(pos, out deflector);
    }

    public bool HasActiveCountdownBlockAt(Vector2Int pos)
    {
        return TryGetActiveCountdownBlockAt(pos, out _);
    }

    public bool TryGetActiveCountdownBlockAt(Vector2Int pos, out GridCountdownBlock block)
    {
        return TryGetObstacle(pos, out block);
    }

    public bool HasActiveStopBlockAt(Vector2Int pos)
    {
        return TryGetActiveStopBlockAt(pos, out _);
    }

    public bool TryGetActiveStopBlockAt(Vector2Int pos, out GridStopBlock block)
    {
        return TryGetObstacle(pos, out block);
    }

    public bool HasActiveArrowShadowAt(Vector2Int pos)
    {
        return TryGetActiveArrowShadowAt(pos, out _);
    }

    public bool TryGetActiveArrowShadowAt(Vector2Int pos, out ArrowShadowVisual shadow)
    {
        return TryGetObstacle(pos, out shadow);
    }

    public void RegisterArrowShadowCells(ArrowShadowVisual shadow, IEnumerable<Vector2Int> cells)
    {
        if (shadow == null || cells == null) return;

        foreach (Vector2Int cell in cells)
        {
            RegisterAt(cell, shadow);
        }
    }

    public void UnregisterArrowShadowCells(ArrowShadowVisual shadow, IEnumerable<Vector2Int> cells)
    {
        if (shadow == null || cells == null) return;

        foreach (Vector2Int cell in cells)
        {
            UnregisterAt(cell, shadow);
        }
    }

    public bool HasBlockingTurnStateBlockAt(Vector2Int pos)
    {
        return TryGetTurnStateBlockAt(pos, out GridTurnStateBlock block) && block.IsBlocking;
    }

    public bool TryGetTurnStateBlockAt(Vector2Int pos, out GridTurnStateBlock block)
    {
        return TryGetObstacle(pos, out block);
    }

    public bool TryGetBlackHoleAt(Vector2Int pos, out GridBlackHole blackHole)
    {
        return TryGetObstacle(pos, out blackHole);
    }

    public void ClearLevelState()
    {
        _flatGrid = null;
        _portalGrid = null;
        _hasPortalGrid = null;
        _width = 0;
        _height = 0;
        _minX = 0;
        _maxX = 0;
        _minY = 0;
        _maxY = 0;

        _keyCollectedEvent = null;
        _electricButtonPressedEvent = null;
        _arrowExitListeners.Clear();
        _arrowExitDispatchBuffer.Clear();
    }

    private static bool IsActiveEntry(object entry)
    {
        if (entry == null) return false;
        if (entry is UnityEngine.Object unityObject && unityObject == null) return false;
        if (entry is IGridOccupant occupant) return occupant.IsActiveOccupant;
        return true;
    }
}
