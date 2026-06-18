using System;
using UnityEngine;

[Serializable]
public abstract class ArrowPayload
{
}

[Serializable]
public sealed class StandardArrowPayload : ArrowPayload
{
    public bool hasArrowShadow;
}

[Serializable]
public abstract class CellPayload
{
}

[Serializable]
public sealed class ColorCellPayload : CellPayload
{
}

[Serializable]
public sealed class ElectricWallPayload : CellPayload
{
    public Vector2Int start;
    public Vector2Int end;
}

[Serializable]
public sealed class CountCellPayload : CellPayload
{
    public int count;
}

[Serializable]
public sealed class TurnStatePayload : CellPayload
{
    public bool startsRed;
}

[Serializable]
public sealed class DirectionCellPayload : CellPayload
{
}

[Serializable]
public sealed class PortalEndpointPayload : CellPayload
{
    public ArrowDir exitDirection;
}

[Serializable]
public abstract class LinkPayload
{
}

[Serializable]
public sealed class PortalPairPayload : LinkPayload
{
    public Color color = Color.white;
}

[Serializable]
public sealed class ElectricButtonWallPayload : LinkPayload
{
    public Color color = Color.white;
}
