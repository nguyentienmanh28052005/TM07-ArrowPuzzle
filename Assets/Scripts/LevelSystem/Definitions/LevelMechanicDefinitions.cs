using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class ArrowDefinition
{
    public string typeId = ArrowTypeIds.Standard;
    public GameObject prefab;
}

[Serializable]
public sealed class CellMechanicDefinition
{
    public string typeId;
    public GameObject prefab;
}

[Serializable]
public sealed class LinkMechanicDefinition
{
    public string typeId;
}

[CreateAssetMenu(fileName = "LevelMechanicRegistry", menuName = "ArrowPuzzle/Level Mechanic Registry")]
public sealed class LevelMechanicRegistry : ScriptableObject
{
    public List<ArrowDefinition> arrows = new List<ArrowDefinition>();
    public List<CellMechanicDefinition> cells = new List<CellMechanicDefinition>();
    public List<LinkMechanicDefinition> links = new List<LinkMechanicDefinition>();

    public bool TryGetArrow(string typeId, out ArrowDefinition definition)
    {
        definition = arrows != null ? arrows.Find(item => item != null && item.typeId == typeId) : null;
        return definition != null;
    }

    public bool TryGetCell(string typeId, out CellMechanicDefinition definition)
    {
        definition = cells != null ? cells.Find(item => item != null && item.typeId == typeId) : null;
        return definition != null;
    }

    public bool TryGetLink(string typeId, out LinkMechanicDefinition definition)
    {
        definition = links != null ? links.Find(item => item != null && item.typeId == typeId) : null;
        return definition != null;
    }
}
