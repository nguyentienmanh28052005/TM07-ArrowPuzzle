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
    public Dictionary<Vector2Int, PortalLink> PortalMap = new Dictionary<Vector2Int, PortalLink>();

    public Action<Color> OnKeyCollectedEvent;

    public void RaiseKeyCollected(Color keyColor)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning($"[GridManager] RaiseKeyCollected color={keyColor} instanceId={GetInstanceID()}");
#endif
        OnKeyCollectedEvent?.Invoke(keyColor);
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
}