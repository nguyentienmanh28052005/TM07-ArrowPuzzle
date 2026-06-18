using System.Collections.Generic;
using UnityEngine;

public readonly struct PortalPairInfo
{
    public readonly CellEntityData entrance;
    public readonly CellEntityData exit;
    public readonly Color color;
    public readonly ArrowDir entranceExitDirection;
    public readonly ArrowDir exitExitDirection;

    public PortalPairInfo(CellEntityData entrance, CellEntityData exit, Color color)
    {
        this.entrance = entrance;
        this.exit = exit;
        this.color = color;
        entranceExitDirection = GetPortalDirection(entrance);
        exitExitDirection = GetPortalDirection(exit);
    }

    private static ArrowDir GetPortalDirection(CellEntityData cell)
    {
        return cell != null && cell.payload is PortalEndpointPayload payload ? payload.exitDirection : cell != null ? cell.direction : ArrowDir.Up;
    }
}

public static class LevelDataV2Queries
{
    public static int GetArrowCount(LevelDataV2 level)
    {
        return level != null && level.arrows != null ? level.arrows.Count : 0;
    }

    public static IEnumerable<ArrowEntityData> GetStandardArrows(LevelDataV2 level)
    {
        if (level == null || level.arrows == null) yield break;

        for (int i = 0; i < level.arrows.Count; i++)
        {
            ArrowEntityData arrow = level.arrows[i];
            if (arrow != null && arrow.typeId == ArrowTypeIds.Standard)
            {
                yield return arrow;
            }
        }
    }

    public static IEnumerable<CellEntityData> GetCells(LevelDataV2 level, string typeId)
    {
        if (level == null || level.cells == null) yield break;

        for (int i = 0; i < level.cells.Count; i++)
        {
            CellEntityData cell = level.cells[i];
            if (cell != null && cell.typeId == typeId)
            {
                yield return cell;
            }
        }
    }

    public static bool HasCell(LevelDataV2 level, string typeId)
    {
        foreach (CellEntityData _ in GetCells(level, typeId))
        {
            return true;
        }

        return false;
    }

    public static bool HasLink(LevelDataV2 level, string typeId)
    {
        if (level == null || level.links == null) return false;

        for (int i = 0; i < level.links.Count; i++)
        {
            LinkEntityData link = level.links[i];
            if (link != null && link.typeId == typeId) return true;
        }

        return false;
    }

    public static CellEntityData FindCell(LevelDataV2 level, string entityId)
    {
        if (level == null || level.cells == null || string.IsNullOrEmpty(entityId)) return null;

        for (int i = 0; i < level.cells.Count; i++)
        {
            CellEntityData cell = level.cells[i];
            if (cell != null && cell.entityId == entityId) return cell;
        }

        return null;
    }

    public static IEnumerable<PortalPairInfo> GetPortalPairs(LevelDataV2 level)
    {
        if (level == null || level.links == null) yield break;

        for (int i = 0; i < level.links.Count; i++)
        {
            LinkEntityData link = level.links[i];
            if (link == null || link.typeId != LinkTypeIds.PortalPair) continue;

            CellEntityData entrance = FindCell(level, link.fromEntityId);
            CellEntityData exit = FindCell(level, link.toEntityId);
            if (entrance == null || exit == null) continue;

            PortalPairPayload payload = link.payload as PortalPairPayload;
            Color color = payload != null ? payload.color : entrance.color;
            yield return new PortalPairInfo(entrance, exit, color);
        }
    }

    public static bool TryGetBounds(LevelDataV2 level, out Bounds bounds)
    {
        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;
        bool hasPosition = false;

        foreach (ArrowEntityData arrow in GetStandardArrows(level))
        {
            if (arrow.segmentPositions == null) continue;
            for (int i = 0; i < arrow.segmentPositions.Count; i++)
            {
                AddBoundsPoint(arrow.segmentPositions[i], ref min, ref max, ref hasPosition);
            }
        }

        if (level != null && level.cells != null)
        {
            for (int i = 0; i < level.cells.Count; i++)
            {
                CellEntityData cell = level.cells[i];
                if (cell == null) continue;

                if (cell.payload is ElectricWallPayload wall)
                {
                    AddBoundsPoint(wall.start, ref min, ref max, ref hasPosition);
                    AddBoundsPoint(wall.end, ref min, ref max, ref hasPosition);
                }
                else
                {
                    AddBoundsPoint(cell.position, ref min, ref max, ref hasPosition);
                }
            }
        }

        if (!hasPosition)
        {
            bounds = default;
            return false;
        }

        Vector3 center = new Vector3((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f, 0f);
        Vector3 size = new Vector3(Mathf.Max(1, max.x - min.x), Mathf.Max(1, max.y - min.y), 0f);
        bounds = new Bounds(center, size);
        return true;
    }

    private static void AddBoundsPoint(Vector2Int point, ref Vector2Int min, ref Vector2Int max, ref bool hasPosition)
    {
        if (!hasPosition)
        {
            min = point;
            max = point;
            hasPosition = true;
            return;
        }

        min = Vector2Int.Min(min, point);
        max = Vector2Int.Max(max, point);
    }
}
