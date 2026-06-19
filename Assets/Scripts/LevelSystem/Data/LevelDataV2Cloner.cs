using System;
using System.Collections.Generic;
using UnityEngine;

public static class LevelDataV2Cloner
{
    public static LevelDataV2 Clone(LevelDataV2 source)
    {
        if (source == null) return null;

        LevelDataV2 clone = ScriptableObject.CreateInstance<LevelDataV2>();
        clone.name = source.name;
        CopyData(source, clone);
        return clone;
    }

    public static void CopyData(LevelDataV2 source, LevelDataV2 destination)
    {
        if (source == null || destination == null) return;

        destination.levelIndex = source.levelIndex;
        destination.gameMode = source.gameMode;
        destination.levelDifficulty = source.levelDifficulty;
        destination.returnToDefaultZoomAfterIntro = source.returnToDefaultZoomAfterIntro;
        destination.timeLimit = source.timeLimit;
        destination.rewardCoins = source.rewardCoins;
        destination.rewardDiamonds = source.rewardDiamonds;

        destination.arrows.Clear();
        foreach (var arrow in source.arrows)
        {
            destination.arrows.Add(CloneArrow(arrow));
        }

        destination.cells.Clear();
        foreach (var cell in source.cells)
        {
            destination.cells.Add(CloneCell(cell));
        }

        destination.links.Clear();
        foreach (var link in source.links)
        {
            destination.links.Add(CloneLink(link));
        }
    }

    private static ArrowEntityData CloneArrow(ArrowEntityData source)
    {
        if (source == null) return null;

        return new ArrowEntityData
        {
            entityId = source.entityId,
            typeId = source.typeId,
            direction = source.direction,
            color = source.color,
            segmentPositions = source.segmentPositions != null ? new List<Vector2Int>(source.segmentPositions) : new List<Vector2Int>(),
            payload = CloneArrowPayload(source.payload)
        };
    }

    private static ArrowPayload CloneArrowPayload(ArrowPayload source)
    {
        if (source == null) return null;

        if (source is StandardArrowPayload standard)
        {
            return new StandardArrowPayload
            {
                hasArrowShadow = standard.hasArrowShadow
            };
        }

        // Fallback/Default
        return new StandardArrowPayload();
    }

    private static CellEntityData CloneCell(CellEntityData source)
    {
        if (source == null) return null;

        return new CellEntityData
        {
            entityId = source.entityId,
            typeId = source.typeId,
            position = source.position,
            direction = source.direction,
            color = source.color,
            payload = CloneCellPayload(source.payload)
        };
    }

    private static CellPayload CloneCellPayload(CellPayload source)
    {
        if (source == null) return null;

        if (source is ColorCellPayload)
        {
            return new ColorCellPayload();
        }
        else if (source is ElectricWallPayload ew)
        {
            return new ElectricWallPayload
            {
                start = ew.start,
                end = ew.end
            };
        }
        else if (source is CountCellPayload countPayload)
        {
            return new CountCellPayload
            {
                count = countPayload.count
            };
        }
        else if (source is TurnStatePayload turnState)
        {
            return new TurnStatePayload
            {
                startsRed = turnState.startsRed
            };
        }
        else if (source is DirectionCellPayload)
        {
            return new DirectionCellPayload();
        }
        else if (source is PortalEndpointPayload portalEndpoint)
        {
            return new PortalEndpointPayload
            {
                exitDirection = portalEndpoint.exitDirection
            };
        }

        return null;
    }

    private static LinkEntityData CloneLink(LinkEntityData source)
    {
        if (source == null) return null;

        return new LinkEntityData
        {
            linkId = source.linkId,
            typeId = source.typeId,
            fromEntityId = source.fromEntityId,
            toEntityId = source.toEntityId,
            fromPosition = source.fromPosition,
            toPosition = source.toPosition,
            usesPositions = source.usesPositions,
            payload = CloneLinkPayload(source.payload)
        };
    }

    private static LinkPayload CloneLinkPayload(LinkPayload source)
    {
        if (source == null) return null;

        if (source is PortalPairPayload portalPair)
        {
            return new PortalPairPayload
            {
                color = portalPair.color
            };
        }
        else if (source is ElectricButtonWallPayload buttonWall)
        {
            return new ElectricButtonWallPayload
            {
                color = buttonWall.color
            };
        }

        return null;
    }
}
