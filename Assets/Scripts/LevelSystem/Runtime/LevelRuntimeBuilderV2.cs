using UnityEngine;

public sealed class LevelRuntimeBuilderV2
{
    private readonly ILevelMechanicFactory factory;

    public LevelRuntimeBuilderV2(ILevelMechanicFactory factory)
    {
        this.factory = factory;
    }

    public void Build(LevelDataV2 level, Transform parent)
    {
        if (level == null || factory == null) return;

        if (level.cells != null)
        {
            foreach (CellEntityData cell in level.cells)
            {
                if (cell != null) factory.CreateCell(cell, parent);
            }
        }

        if (level.links != null)
        {
            foreach (LinkEntityData link in level.links)
            {
                if (link != null) factory.CreateLink(link, level, parent);
            }
        }

        if (level.arrows != null)
        {
            foreach (ArrowEntityData arrow in level.arrows)
            {
                if (arrow != null) factory.CreateArrow(arrow, parent);
            }
        }
    }
}
