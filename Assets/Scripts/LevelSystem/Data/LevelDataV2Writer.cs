using System.Collections.Generic;
using UnityEngine;

public static class LevelDataV2Writer
{
    public static void ClearContent(LevelDataV2 level)
    {
        if (level == null) return;
        level.arrows.Clear();
        level.cells.Clear();
        level.links.Clear();
    }

    public static void SetSnakes(LevelDataV2 level, IEnumerable<SnakeSaveData> snakes)
    {
        if (level == null) return;
        level.arrows.Clear();
        if (snakes == null) return;

        foreach (SnakeSaveData snake in snakes)
        {
            AddSnake(level, snake);
        }
    }

    public static ArrowEntityData AddSnake(LevelDataV2 level, SnakeSaveData snake)
    {
        if (level == null || snake == null) return null;

        ArrowEntityData arrow = new ArrowEntityData
        {
            entityId = CreateEntityId(ArrowTypeIds.Standard, level.arrows.Count),
            typeId = ArrowTypeIds.Standard,
            direction = snake.direction,
            color = snake.arrowColor,
            segmentPositions = snake.segmentPositions != null ? new List<Vector2Int>(snake.segmentPositions) : new List<Vector2Int>(),
            payload = new StandardArrowPayload { hasArrowShadow = snake.hasArrowShadow }
        };
        level.arrows.Add(arrow);
        return arrow;
    }

    public static CellEntityData AddCell(LevelDataV2 level, string typeId, Vector2Int position, ArrowDir direction, Color color, CellPayload payload)
    {
        if (level == null) return null;

        CellEntityData cell = new CellEntityData
        {
            entityId = CreateEntityId(typeId, level.cells.Count),
            typeId = typeId,
            position = position,
            direction = direction,
            color = color,
            payload = payload
        };
        level.cells.Add(cell);
        return cell;
    }

    public static LinkEntityData AddLink(LevelDataV2 level, string typeId, string fromEntityId, string toEntityId, LinkPayload payload)
    {
        if (level == null) return null;

        LinkEntityData link = new LinkEntityData
        {
            linkId = CreateEntityId(typeId, level.links.Count),
            typeId = typeId,
            fromEntityId = fromEntityId,
            toEntityId = toEntityId,
            payload = payload
        };
        level.links.Add(link);
        return link;
    }

    private static string CreateEntityId(string typeId, int index)
    {
        return $"{typeId}:{index}";
    }
}
