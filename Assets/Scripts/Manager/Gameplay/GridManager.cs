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

    private readonly Dictionary<Vector2Int, SnakeBlock> _gridMap = new Dictionary<Vector2Int, SnakeBlock>();
    private readonly Dictionary<Vector2Int, GridKeycard> _keycardMap = new Dictionary<Vector2Int, GridKeycard>();
    private readonly Dictionary<Vector2Int, GridLaserGate> _gateMap = new Dictionary<Vector2Int, GridLaserGate>();
    private readonly Dictionary<Vector2Int, GridElectricButton> _electricButtonMap = new Dictionary<Vector2Int, GridElectricButton>();
    private readonly Dictionary<Vector2Int, GridRevealWaveButton> _revealWaveButtonMap = new Dictionary<Vector2Int, GridRevealWaveButton>();
    private readonly Dictionary<Vector2Int, GridElectricWall> _electricWallMap = new Dictionary<Vector2Int, GridElectricWall>();
    private readonly Dictionary<Vector2Int, PortalLink> _portalMap = new Dictionary<Vector2Int, PortalLink>();
    private readonly Dictionary<Vector2Int, GridDeflector> _deflectorMap = new Dictionary<Vector2Int, GridDeflector>();
    private readonly Dictionary<Vector2Int, GridCountdownBlock> _countdownBlockMap = new Dictionary<Vector2Int, GridCountdownBlock>();
    private readonly Dictionary<Vector2Int, GridStopBlock> _stopBlockMap = new Dictionary<Vector2Int, GridStopBlock>();
    private readonly Dictionary<Vector2Int, ArrowShadowVisual> _arrowShadowMap = new Dictionary<Vector2Int, ArrowShadowVisual>();
    private readonly Dictionary<Vector2Int, GridTurnStateBlock> _turnStateBlockMap = new Dictionary<Vector2Int, GridTurnStateBlock>();
    private readonly Dictionary<Vector2Int, GridBlackHole> _blackHoleMap = new Dictionary<Vector2Int, GridBlackHole>();

    private readonly HashSet<IArrowExitListener> _arrowExitListeners = new HashSet<IArrowExitListener>();
    private readonly List<IArrowExitListener> _arrowExitDispatchBuffer = new List<IArrowExitListener>(16);

    private event Action<Color> _keyCollectedEvent;
    private event Action<Color> _electricButtonPressedEvent;

    public IEnumerable<GridLaserGate> Gates => _gateMap.Values;

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

    public void Register(IGridOccupant occupant)
    {
        if (occupant == null) return;

        Vector2Int position = occupant.GridPosition;
        if (occupant is GridKeycard keycard) _keycardMap[position] = keycard;
        else if (occupant is GridLaserGate gate) _gateMap[position] = gate;
        else if (occupant is GridElectricButton button) _electricButtonMap[position] = button;
        else if (occupant is GridRevealWaveButton revealButton) _revealWaveButtonMap[position] = revealButton;
        else if (occupant is GridDeflector deflector) _deflectorMap[position] = deflector;
        else if (occupant is GridCountdownBlock countdownBlock) _countdownBlockMap[position] = countdownBlock;
        else if (occupant is GridStopBlock stopBlock) _stopBlockMap[position] = stopBlock;
        else if (occupant is GridTurnStateBlock turnStateBlock) _turnStateBlockMap[position] = turnStateBlock;
        else if (occupant is GridBlackHole blackHole) _blackHoleMap[position] = blackHole;
        else
        {
            Debug.LogWarning($"[GridManager] Unsupported occupant type: {occupant.GetType().Name}");
        }
    }

    public void Unregister(IGridOccupant occupant)
    {
        if (occupant == null) return;
        Unregister(occupant, occupant.GridPosition);
    }

    public void Unregister(IGridOccupant occupant, Vector2Int position)
    {
        if (occupant == null) return;

        if (occupant is GridKeycard keycard) RemoveIfCurrent(_keycardMap, position, keycard);
        else if (occupant is GridLaserGate gate) RemoveIfCurrent(_gateMap, position, gate);
        else if (occupant is GridElectricButton button) RemoveIfCurrent(_electricButtonMap, position, button);
        else if (occupant is GridRevealWaveButton revealButton) RemoveIfCurrent(_revealWaveButtonMap, position, revealButton);
        else if (occupant is GridDeflector deflector) RemoveIfCurrent(_deflectorMap, position, deflector);
        else if (occupant is GridCountdownBlock countdownBlock) RemoveIfCurrent(_countdownBlockMap, position, countdownBlock);
        else if (occupant is GridStopBlock stopBlock) RemoveIfCurrent(_stopBlockMap, position, stopBlock);
        else if (occupant is GridTurnStateBlock turnStateBlock) RemoveIfCurrent(_turnStateBlockMap, position, turnStateBlock);
        else if (occupant is GridBlackHole blackHole) RemoveIfCurrent(_blackHoleMap, position, blackHole);
    }

    public void RegisterSnake(SnakeBlock snake)
    {
        if (snake == null || snake.LogicNodes == null) return;

        for (int i = 0; i < snake.LogicNodes.Count; i++)
        {
            Vector3 nodePos = snake.LogicNodes[i];
            Vector2Int pos = new Vector2Int(Mathf.RoundToInt(nodePos.x), Mathf.RoundToInt(nodePos.y));
            _gridMap[pos] = snake;
        }
    }

    public void RegisterSnakeCells(SnakeBlock snake, IEnumerable<Vector2Int> cells)
    {
        if (snake == null || cells == null) return;

        foreach (Vector2Int cell in cells)
        {
            _gridMap[cell] = snake;
        }
    }

    public void UnregisterSnake(SnakeBlock snake)
    {
        if (snake == null) return;

        List<Vector2Int> keysToRemove = new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, SnakeBlock> kvp in _gridMap)
        {
            if (kvp.Value == snake) keysToRemove.Add(kvp.Key);
        }

        for (int i = 0; i < keysToRemove.Count; i++)
        {
            _gridMap.Remove(keysToRemove[i]);
        }
    }

    public void UnregisterSnakeCells(SnakeBlock snake, IEnumerable<Vector2Int> cells)
    {
        if (snake == null || cells == null) return;

        foreach (Vector2Int cell in cells)
        {
            RemoveIfCurrent(_gridMap, cell, snake);
        }
    }

    public SnakeBlock GetSnakeAt(Vector2Int pos)
    {
        TryGetSnakeAt(pos, out SnakeBlock snake);
        return snake;
    }

    public bool TryGetSnakeAt(Vector2Int pos, out SnakeBlock snake)
    {
        return TryGetFromMap(_gridMap, pos, out snake);
    }

    public bool TryGetObstacle<T>(Vector2Int pos, out T obstacle) where T : class
    {
        obstacle = null;
        object found = null;

        if (typeof(T) == typeof(SnakeBlock))
        {
            if (TryGetSnakeAt(pos, out SnakeBlock snake)) found = snake;
        }
        else if (typeof(T) == typeof(GridKeycard))
        {
            if (TryGetKeycardAt(pos, out GridKeycard keycard)) found = keycard;
        }
        else if (typeof(T) == typeof(GridLaserGate))
        {
            if (TryGetGateAt(pos, out GridLaserGate gate)) found = gate;
        }
        else if (typeof(T) == typeof(GridElectricButton))
        {
            if (TryGetElectricButtonAt(pos, out GridElectricButton button)) found = button;
        }
        else if (typeof(T) == typeof(GridRevealWaveButton))
        {
            if (TryGetRevealWaveButtonAt(pos, out GridRevealWaveButton revealButton)) found = revealButton;
        }
        else if (typeof(T) == typeof(GridElectricWall))
        {
            if (TryGetElectricWallAt(pos, out GridElectricWall wall)) found = wall;
        }
        else if (typeof(T) == typeof(GridDeflector))
        {
            if (TryGetDeflectorAt(pos, out GridDeflector deflector)) found = deflector;
        }
        else if (typeof(T) == typeof(GridCountdownBlock))
        {
            if (TryGetActiveCountdownBlockAt(pos, out GridCountdownBlock countdownBlock)) found = countdownBlock;
        }
        else if (typeof(T) == typeof(GridStopBlock))
        {
            if (TryGetActiveStopBlockAt(pos, out GridStopBlock stopBlock)) found = stopBlock;
        }
        else if (typeof(T) == typeof(ArrowShadowVisual))
        {
            if (TryGetActiveArrowShadowAt(pos, out ArrowShadowVisual shadow)) found = shadow;
        }
        else if (typeof(T) == typeof(GridTurnStateBlock))
        {
            if (TryGetTurnStateBlockAt(pos, out GridTurnStateBlock turnStateBlock)) found = turnStateBlock;
        }
        else if (typeof(T) == typeof(GridBlackHole))
        {
            if (TryGetBlackHoleAt(pos, out GridBlackHole blackHole)) found = blackHole;
        }

        obstacle = found as T;
        return obstacle != null;
    }

    public bool TryGetTriggerAt(Vector2Int pos, out IGridTrigger trigger)
    {
        trigger = null;

        if (TryGetKeycardAt(pos, out GridKeycard keycard)) trigger = keycard;
        else if (TryGetElectricButtonAt(pos, out GridElectricButton electricButton)) trigger = electricButton;
        else if (TryGetRevealWaveButtonAt(pos, out GridRevealWaveButton revealButton)) trigger = revealButton;

        return trigger != null;
    }

    public int TriggerAt(Vector2Int pos)
    {
        int triggerCount = 0;

        if (TryGetKeycardAt(pos, out GridKeycard keycard))
        {
            keycard.TriggerFromGrid();
            triggerCount++;
        }

        if (TryGetElectricButtonAt(pos, out GridElectricButton electricButton))
        {
            electricButton.TriggerFromGrid();
            triggerCount++;
        }

        if (TryGetRevealWaveButtonAt(pos, out GridRevealWaveButton revealButton))
        {
            revealButton.TriggerFromGrid();
            triggerCount++;
        }

        return triggerCount;
    }

    public bool TryGetKeycardAt(Vector2Int pos, out GridKeycard keycard)
    {
        return TryGetFromMap(_keycardMap, pos, out keycard);
    }

    public bool TryGetGateAt(Vector2Int pos, out GridLaserGate gate)
    {
        return TryGetFromMap(_gateMap, pos, out gate);
    }

    public bool HasGateAt(Vector2Int pos)
    {
        return TryGetGateAt(pos, out _);
    }

    public bool TryGetElectricButtonAt(Vector2Int pos, out GridElectricButton button)
    {
        return TryGetFromMap(_electricButtonMap, pos, out button);
    }

    public bool TryGetRevealWaveButtonAt(Vector2Int pos, out GridRevealWaveButton button)
    {
        return TryGetFromMap(_revealWaveButtonMap, pos, out button);
    }

    public bool TryGetElectricWallAt(Vector2Int pos, out GridElectricWall wall)
    {
        return TryGetFromMap(_electricWallMap, pos, out wall);
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
            _electricWallMap[cell] = wall;
        }
    }

    public void UnregisterElectricWallCells(GridElectricWall wall, IEnumerable<Vector2Int> cells)
    {
        if (wall == null || cells == null) return;

        foreach (Vector2Int cell in cells)
        {
            RemoveIfCurrent(_electricWallMap, cell, wall);
        }
    }

    public void RegisterPortalLink(Vector2Int entrance, PortalLink link)
    {
        _portalMap[entrance] = link;
    }

    public void RegisterPortalLink(Vector2Int entrance, Vector2Int exit, ArrowDir exitDirection)
    {
        _portalMap[entrance] = new PortalLink { exit = exit, exitDir = exitDirection };
    }

    public bool TryGetPortalLink(Vector2Int pos, out PortalLink link)
    {
        return _portalMap.TryGetValue(pos, out link);
    }

    public bool TryGetDeflectorAt(Vector2Int pos, out GridDeflector deflector)
    {
        return TryGetFromMap(_deflectorMap, pos, out deflector);
    }

    public bool HasActiveCountdownBlockAt(Vector2Int pos)
    {
        return TryGetActiveCountdownBlockAt(pos, out _);
    }

    public bool TryGetActiveCountdownBlockAt(Vector2Int pos, out GridCountdownBlock block)
    {
        return TryGetFromMap(_countdownBlockMap, pos, out block);
    }

    public bool HasActiveStopBlockAt(Vector2Int pos)
    {
        return TryGetActiveStopBlockAt(pos, out _);
    }

    public bool TryGetActiveStopBlockAt(Vector2Int pos, out GridStopBlock block)
    {
        return TryGetFromMap(_stopBlockMap, pos, out block);
    }

    public bool HasActiveArrowShadowAt(Vector2Int pos)
    {
        return TryGetActiveArrowShadowAt(pos, out _);
    }

    public bool TryGetActiveArrowShadowAt(Vector2Int pos, out ArrowShadowVisual shadow)
    {
        return TryGetFromMap(_arrowShadowMap, pos, out shadow);
    }

    public void RegisterArrowShadowCells(ArrowShadowVisual shadow, IEnumerable<Vector2Int> cells)
    {
        if (shadow == null || cells == null) return;

        foreach (Vector2Int cell in cells)
        {
            _arrowShadowMap[cell] = shadow;
        }
    }

    public void UnregisterArrowShadowCells(ArrowShadowVisual shadow, IEnumerable<Vector2Int> cells)
    {
        if (shadow == null || cells == null) return;

        foreach (Vector2Int cell in cells)
        {
            RemoveIfCurrent(_arrowShadowMap, cell, shadow);
        }
    }

    public bool HasBlockingTurnStateBlockAt(Vector2Int pos)
    {
        return TryGetTurnStateBlockAt(pos, out GridTurnStateBlock block) && block.IsBlocking;
    }

    public bool TryGetTurnStateBlockAt(Vector2Int pos, out GridTurnStateBlock block)
    {
        return TryGetFromMap(_turnStateBlockMap, pos, out block);
    }

    public bool TryGetBlackHoleAt(Vector2Int pos, out GridBlackHole blackHole)
    {
        return TryGetFromMap(_blackHoleMap, pos, out blackHole);
    }

    public void ClearLevelState()
    {
        _gridMap.Clear();
        _keycardMap.Clear();
        _gateMap.Clear();
        _electricButtonMap.Clear();
        _revealWaveButtonMap.Clear();
        _electricWallMap.Clear();
        _portalMap.Clear();
        _deflectorMap.Clear();
        _countdownBlockMap.Clear();
        _stopBlockMap.Clear();
        _arrowShadowMap.Clear();
        _turnStateBlockMap.Clear();
        _blackHoleMap.Clear();

        _keyCollectedEvent = null;
        _electricButtonPressedEvent = null;
        _arrowExitListeners.Clear();
        _arrowExitDispatchBuffer.Clear();
    }

    private static bool TryGetFromMap<T>(Dictionary<Vector2Int, T> map, Vector2Int pos, out T value) where T : class
    {
        value = null;
        if (map == null || !map.TryGetValue(pos, out value)) return false;

        if (!IsActiveEntry(value))
        {
            map.Remove(pos);
            value = null;
            return false;
        }

        return true;
    }

    private static bool IsActiveEntry(object entry)
    {
        if (entry == null) return false;
        if (entry is UnityEngine.Object unityObject && unityObject == null) return false;
        if (entry is IGridOccupant occupant) return occupant.IsActiveOccupant;
        return true;
    }

    private static void RemoveIfCurrent<T>(Dictionary<Vector2Int, T> map, Vector2Int pos, T expected) where T : class
    {
        if (map == null || expected == null) return;
        if (map.TryGetValue(pos, out T existing) && ReferenceEquals(existing, expected))
        {
            map.Remove(pos);
        }
    }
}
