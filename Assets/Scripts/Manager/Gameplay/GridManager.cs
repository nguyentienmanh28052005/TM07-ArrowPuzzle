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

    public Dictionary<Vector2Int, SnakeBlock> GridMap = new Dictionary<Vector2Int, SnakeBlock>();
    public Dictionary<Vector2Int, GridKeycard> KeycardMap = new Dictionary<Vector2Int, GridKeycard>();
    public Dictionary<Vector2Int, GridLaserGate> GateMap = new Dictionary<Vector2Int, GridLaserGate>();
    public Dictionary<Vector2Int, GridElectricButton> ElectricButtonMap = new Dictionary<Vector2Int, GridElectricButton>();
    public Dictionary<Vector2Int, GridRevealWaveButton> RevealWaveButtonMap = new Dictionary<Vector2Int, GridRevealWaveButton>();
    public Dictionary<Vector2Int, GridElectricWall> ElectricWallMap = new Dictionary<Vector2Int, GridElectricWall>();
    public Dictionary<Vector2Int, PortalLink> PortalMap = new Dictionary<Vector2Int, PortalLink>();
    public Dictionary<Vector2Int, GridDeflector> DeflectorMap = new Dictionary<Vector2Int, GridDeflector>();
    public Dictionary<Vector2Int, GridCountdownBlock> CountdownBlockMap = new Dictionary<Vector2Int, GridCountdownBlock>();
    public Dictionary<Vector2Int, GridStopBlock> StopBlockMap = new Dictionary<Vector2Int, GridStopBlock>();
    public Dictionary<Vector2Int, ArrowShadowVisual> ArrowShadowMap = new Dictionary<Vector2Int, ArrowShadowVisual>();
    public Dictionary<Vector2Int, GridTurnStateBlock> TurnStateBlockMap = new Dictionary<Vector2Int, GridTurnStateBlock>();
    public Dictionary<Vector2Int, GridBlackHole> BlackHoleMap = new Dictionary<Vector2Int, GridBlackHole>();

    public Action<Color> OnKeyCollectedEvent;
    public Action<Color> OnElectricButtonPressedEvent;
    public Action OnArrowExitedEvent;

    public void RaiseKeyCollected(Color keyColor)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning($"[GridManager] RaiseKeyCollected color={keyColor} instanceId={GetInstanceID()}");
#endif
        OnKeyCollectedEvent?.Invoke(keyColor);
    }

    public void RaiseElectricButtonPressed(Color buttonColor)
    {
        OnElectricButtonPressedEvent?.Invoke(buttonColor);
    }

    public void RaiseArrowExited()
    {
        OnArrowExitedEvent?.Invoke();
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterSnake(SnakeBlock snake)
    {
        if (snake == null || snake.LogicNodes == null) return;
        
        foreach (Vector3 nodePos in snake.LogicNodes)
        {
            Vector2Int pos = new Vector2Int(Mathf.RoundToInt(nodePos.x), Mathf.RoundToInt(nodePos.y));
            GridMap[pos] = snake; 
        }
    }

    public void UnregisterSnake(SnakeBlock snake)
    {
        if (snake == null) return;
        List<Vector2Int> keysToRemove = new List<Vector2Int>();
        foreach (var kvp in GridMap)
        {
            if (kvp.Value == snake) keysToRemove.Add(kvp.Key);
        }

        foreach (var key in keysToRemove) GridMap.Remove(key);
    }

    public SnakeBlock GetSnakeAt(Vector2Int pos)
    {
        if (GridMap.TryGetValue(pos, out SnakeBlock snake)) return snake;
        return null;
    }

    public bool HasActiveCountdownBlockAt(Vector2Int pos)
    {
        if (CountdownBlockMap == null) return false;
        if (!CountdownBlockMap.TryGetValue(pos, out GridCountdownBlock block)) return false;

        if (block == null || block.IsDestroyed)
        {
            CountdownBlockMap.Remove(pos);
            return false;
        }

        return true;
    }

    public bool HasActiveStopBlockAt(Vector2Int pos)
    {
        return TryGetActiveStopBlockAt(pos, out _);
    }

    public bool TryGetActiveStopBlockAt(Vector2Int pos, out GridStopBlock block)
    {
        block = null;
        if (StopBlockMap == null) return false;
        if (!StopBlockMap.TryGetValue(pos, out block)) return false;

        if (block == null || block.IsDestroyed)
        {
            StopBlockMap.Remove(pos);
            block = null;
            return false;
        }

        return true;
    }

    public bool HasActiveArrowShadowAt(Vector2Int pos)
    {
        return TryGetActiveArrowShadowAt(pos, out _);
    }

    public bool TryGetActiveArrowShadowAt(Vector2Int pos, out ArrowShadowVisual shadow)
    {
        shadow = null;
        if (ArrowShadowMap == null) return false;
        if (!ArrowShadowMap.TryGetValue(pos, out shadow)) return false;

        if (shadow == null || shadow.IsDestroyed)
        {
            ArrowShadowMap.Remove(pos);
            shadow = null;
            return false;
        }

        return true;
    }

    public bool HasBlockingTurnStateBlockAt(Vector2Int pos)
    {
        return TryGetTurnStateBlockAt(pos, out GridTurnStateBlock block) && block.IsBlocking;
    }

    public bool TryGetTurnStateBlockAt(Vector2Int pos, out GridTurnStateBlock block)
    {
        block = null;
        if (TurnStateBlockMap == null) return false;
        if (!TurnStateBlockMap.TryGetValue(pos, out block)) return false;

        if (block == null || block.IsDestroyed)
        {
            TurnStateBlockMap.Remove(pos);
            block = null;
            return false;
        }

        return true;
    }

    public bool TryGetBlackHoleAt(Vector2Int pos, out GridBlackHole blackHole)
    {
        blackHole = null;
        if (BlackHoleMap == null) return false;
        if (!BlackHoleMap.TryGetValue(pos, out blackHole)) return false;

        if (blackHole == null || blackHole.IsDestroyed)
        {
            BlackHoleMap.Remove(pos);
            blackHole = null;
            return false;
        }

        return true;
    }

    public void ClearLevelState()
    {
        GridMap.Clear();
        KeycardMap.Clear();
        GateMap.Clear();
        ElectricButtonMap.Clear();
        RevealWaveButtonMap.Clear();
        ElectricWallMap.Clear();
        PortalMap.Clear();
        DeflectorMap.Clear();
        CountdownBlockMap.Clear();
        StopBlockMap.Clear();
        ArrowShadowMap.Clear();
        TurnStateBlockMap.Clear();
        BlackHoleMap.Clear();
    }
}
