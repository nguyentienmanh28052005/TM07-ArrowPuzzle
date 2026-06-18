using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

// Lightweight transfer structs used by editor/importer/generator bridges.
// The source level asset is LevelDataV2, not the old mechanic-specific LevelDataSO.
[System.Serializable]
[MemoryPackable]
public partial class ElectricButtonSaveData
{
    public Vector2Int position;
    public Color color;
}

[System.Serializable]
[MemoryPackable]
public partial class RevealWaveButtonSaveData
{
    public Vector2Int position;
    public Color color;
}

[System.Serializable]
[MemoryPackable]
public partial class ElectricWallSaveData
{
    public Vector2Int start;
    public Vector2Int end;
    public Color color;
}

[System.Serializable]
[MemoryPackable]
public partial class SnakeSaveData
{
    public ArrowDir direction;
    public Color arrowColor;
    public bool hasArrowShadow;
    public List<Vector2Int> segmentPositions = new List<Vector2Int>();
}

[System.Serializable]
[MemoryPackable]
public partial class KeycardSaveData
{
    public Vector2Int position;
    public Color color;
}

[System.Serializable]
[MemoryPackable]
public partial class GateSaveData
{
    public Vector2Int position;
    public Color color;
}

[System.Serializable]
[MemoryPackable]
public partial class DeflectorSaveData
{
    public Vector2Int position;
    public ArrowDir direction;
}

[System.Serializable]
[MemoryPackable]
public partial class CountdownBlockSaveData
{
    public Vector2Int position;
    public int count;
}

[System.Serializable]
[MemoryPackable]
public partial class StopBlockSaveData
{
    public Vector2Int position;
    public int count;
}

[System.Serializable]
[MemoryPackable]
public partial class TurnStateBlockSaveData
{
    public Vector2Int position;
    public bool startsRed;
}

[System.Serializable]
[MemoryPackable]
public partial class BlackHoleSaveData
{
    public Vector2Int position;
    public ArrowDir direction;
}

[System.Serializable]
[MemoryPackable]
public partial class PortalData
{
    public Vector2Int entrance;
    public ArrowDir entranceDir;
    public Vector2Int exit;
    public ArrowDir exitDir;
    public Color portalColor;
}
