using UnityEngine;

public enum ObstacleHitType
{
    None,
    Snake,
    Gate,
    ElectricWall,
    CountdownBlock,
    StopBlock,
    ArrowShadow,
    TurnStateBlock,
    BlackHole,
    BlackHoleBlocked
}

public readonly struct ObstacleHit
{
    public static readonly ObstacleHit None = new ObstacleHit(ObstacleHitType.None, new Vector2Int(int.MinValue, int.MinValue));

    public readonly ObstacleHitType Type;
    public readonly Vector2Int Cell;
    public readonly SnakeBlock Snake;
    public readonly GridStopBlock StopBlock;
    public readonly GridBlackHole BlackHole;

    public bool HasHit => Type != ObstacleHitType.None;
    public bool IsReleaseExit => Type == ObstacleHitType.None || Type == ObstacleHitType.BlackHole;

    public ObstacleHit(
        ObstacleHitType type,
        Vector2Int cell,
        SnakeBlock snake = null,
        GridStopBlock stopBlock = null,
        GridBlackHole blackHole = null)
    {
        Type = type;
        Cell = cell;
        Snake = snake;
        StopBlock = stopBlock;
        BlackHole = blackHole;
    }
}
