using UnityEngine;

public interface IArrowBehavior
{
    void Initialize(ArrowEntityData data);
}

public interface ISpecialCell
{
    void Initialize(CellEntityData data);
}

public interface ICellTrigger
{
    void TriggerFromCell();
}

public interface IRuleContributor
{
    void ContributeRules(LevelRuleContext context);
}

public interface ILevelMechanicFactory
{
    GameObject CreateArrow(ArrowEntityData data, Transform parent);
    GameObject CreateCell(CellEntityData data, Transform parent);
    void CreateLink(LinkEntityData data, LevelDataV2 level, Transform parent);
}

public sealed class LevelRuleContext
{
    public LevelDataV2 level;
}
