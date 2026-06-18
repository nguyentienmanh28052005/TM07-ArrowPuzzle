using System.Collections.Generic;
using UnityEngine;

public sealed class LevelEditorSerializer
{
    public void Save(LevelEditorContext context)
    {
        if (context == null || context.currentData == null)
        {
            return;
        }

        if (context.inputTimeLimit != null)
        {
            float.TryParse(context.inputTimeLimit.text, out context.currentData.timeLimit);
        }

        if (context.inputRewardCoins != null)
        {
            float.TryParse(context.inputRewardCoins.text, out context.currentData.rewardCoins);
        }

        if (context.inputRewardDiamonds != null)
        {
            float.TryParse(context.inputRewardDiamonds.text, out context.currentData.rewardDiamonds);
        }

        LevelDataV2Writer.ClearContent(context.currentData);

        List<CellEntityData> electricButtons = new List<CellEntityData>();
        List<CellEntityData> electricWalls = new List<CellEntityData>();

        foreach (Transform child in context.levelContainer)
        {
            EditorSnakeVisual snakeVisual = child.GetComponent<EditorSnakeVisual>();
            if (snakeVisual != null
                && child.gameObject != context.currentSelectionGlowObj
                && snakeVisual.LogicNodes != null
                && snakeVisual.LogicNodes.Count > 0)
            {
                LevelDataV2Writer.AddSnake(context.currentData, new SnakeSaveData
                {
                    direction = snakeVisual.direction,
                    arrowColor = snakeVisual.snakeColor,
                    segmentPositions = new List<Vector2Int>(snakeVisual.LogicNodes),
                    hasArrowShadow = snakeVisual.HasArrowShadow
                });
            }

            if (child.TryGetComponent(out GridKeycard keycard))
                LevelDataV2Writer.AddCell(context.currentData, CellTypeIds.Keycard, GetGridPosition(child), ArrowDir.Up, keycard.keyColor, new ColorCellPayload());

            if (child.TryGetComponent(out GridLaserGate gate))
                LevelDataV2Writer.AddCell(context.currentData, CellTypeIds.Gate, GetGridPosition(child), ArrowDir.Up, gate.gateColor, new ColorCellPayload());

            if (child.TryGetComponent(out GridElectricButton button))
                electricButtons.Add(LevelDataV2Writer.AddCell(context.currentData, CellTypeIds.ElectricButton, GetGridPosition(child), ArrowDir.Up, button.buttonColor, new ColorCellPayload()));

            if (child.TryGetComponent(out GridRevealWaveButton revealWaveButton))
                LevelDataV2Writer.AddCell(context.currentData, CellTypeIds.RevealWaveButton, GetGridPosition(child), ArrowDir.Up, revealWaveButton.buttonColor, new ColorCellPayload());

            GridDeflector deflector = child.GetComponentInChildren<GridDeflector>();
            if (deflector != null)
                LevelDataV2Writer.AddCell(context.currentData, CellTypeIds.Deflector, new Vector2Int((int)deflector.transform.position.x, (int)deflector.transform.position.y), deflector.direction, Color.white, new DirectionCellPayload());

            if (child.TryGetComponent(out GridCountdownBlock countdownBlock))
                LevelDataV2Writer.AddCell(context.currentData, CellTypeIds.CountdownBlock, GetGridPosition(child), ArrowDir.Up, Color.white, new CountCellPayload { count = countdownBlock.count });

            if (child.TryGetComponent(out GridStopBlock stopBlock))
                LevelDataV2Writer.AddCell(context.currentData, CellTypeIds.StopBlock, GetGridPosition(child), ArrowDir.Up, Color.white, new CountCellPayload { count = stopBlock.count });

            if (child.TryGetComponent(out GridTurnStateBlock turnStateBlock))
                LevelDataV2Writer.AddCell(context.currentData, CellTypeIds.TurnStateBlock, GetGridPosition(child), ArrowDir.Up, Color.white, new TurnStatePayload { startsRed = turnStateBlock.IsRed });

            if (child.TryGetComponent(out GridBlackHole blackHole))
                LevelDataV2Writer.AddCell(context.currentData, CellTypeIds.BlackHole, GetGridPosition(child), blackHole.direction, Color.white, new DirectionCellPayload());
        }

        for (int i = 0; i < context.currentDraftElectricWalls.Count; i++)
        {
            ElectricWallSaveData wall = context.currentDraftElectricWalls[i];
            CellEntityData wallCell = LevelDataV2Writer.AddCell(
                context.currentData,
                CellTypeIds.ElectricWall,
                wall.start,
                ArrowDir.Up,
                wall.color,
                new ElectricWallPayload { start = wall.start, end = wall.end });
            electricWalls.Add(wallCell);
        }

        for (int i = 0; i < context.currentDraftPortals.Count; i++)
        {
            PortalData portal = context.currentDraftPortals[i];
            CellEntityData entrance = LevelDataV2Writer.AddCell(
                context.currentData,
                CellTypeIds.Portal,
                portal.entrance,
                portal.entranceDir,
                portal.portalColor,
                new PortalEndpointPayload { exitDirection = portal.entranceDir });
            CellEntityData exit = LevelDataV2Writer.AddCell(
                context.currentData,
                CellTypeIds.Portal,
                portal.exit,
                portal.exitDir,
                portal.portalColor,
                new PortalEndpointPayload { exitDirection = portal.exitDir });

            LevelDataV2Writer.AddLink(context.currentData, LinkTypeIds.PortalPair, entrance.entityId, exit.entityId, new PortalPairPayload { color = portal.portalColor });
        }

        for (int i = 0; i < electricButtons.Count; i++)
        {
            for (int j = 0; j < electricWalls.Count; j++)
            {
                if (!LevelEditorRuntimeHelpers.ColorsMatch(electricButtons[i].color, electricWalls[j].color)) continue;
                LevelDataV2Writer.AddLink(context.currentData, LinkTypeIds.ElectricButtonWall, electricButtons[i].entityId, electricWalls[j].entityId, new ElectricButtonWallPayload { color = electricWalls[j].color });
            }
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(context.currentData);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }

    private static Vector2Int GetGridPosition(Transform transform)
    {
        return new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
    }

    public void Load(LevelEditorContext context)
    {
        if (context == null || context.currentData == null)
        {
            return;
        }

        for (int i = context.levelContainer.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(context.levelContainer.GetChild(i).gameObject);
        }

        context.finishedSnakesHistory.Clear();
        context.spawnedPortalVisuals.Clear();
        context.spawnedElectricWallVisuals.Clear();

        if (context.inputTimeLimit != null) context.inputTimeLimit.text = context.currentData.timeLimit.ToString();
        if (context.inputRewardCoins != null) context.inputRewardCoins.text = context.currentData.rewardCoins.ToString();
        if (context.inputRewardDiamonds != null) context.inputRewardDiamonds.text = context.currentData.rewardDiamonds.ToString();

        LoadArrows(context);
        LoadCells(context);
        LoadDraftLinks(context);
        SpawnPortalVisuals(context);
        SpawnElectricWallVisuals(context);
    }

    private static void LoadArrows(LevelEditorContext context)
    {
        if (context.snakePrefab == null) return;

        foreach (ArrowEntityData arrowData in LevelDataV2Queries.GetStandardArrows(context.currentData))
        {
            StandardArrowPayload payload = arrowData.payload as StandardArrowPayload;
            GameObject snakeObject = Object.Instantiate(context.snakePrefab, context.levelContainer);
            EditorSnakeVisual snakeVisual = snakeObject.GetComponent<EditorSnakeVisual>();
            snakeVisual.Initialize(arrowData.direction, arrowData.segmentPositions, arrowData.color, payload != null && payload.hasArrowShadow);
            context.finishedSnakesHistory.Push(snakeObject);
        }
    }

    private static void LoadCells(LevelEditorContext context)
    {
        if (context.currentData.cells == null) return;

        foreach (CellEntityData cell in context.currentData.cells)
        {
            if (cell == null || cell.typeId == CellTypeIds.Portal || cell.typeId == CellTypeIds.ElectricWall) continue;
            SpawnEditorCell(context, cell);
        }
    }

    private static void SpawnEditorCell(LevelEditorContext context, CellEntityData cell)
    {
        GameObject prefab = GetEditorPrefab(context, cell.typeId);
        if (prefab == null) return;

        Quaternion rotation = cell.typeId == CellTypeIds.Deflector || cell.typeId == CellTypeIds.BlackHole
            ? LevelEditorRuntimeHelpers.GetRotationForDir(cell.direction)
            : Quaternion.identity;

        GameObject obj = Object.Instantiate(prefab, new Vector3(cell.position.x, cell.position.y, 0f), rotation, context.levelContainer);

        if (cell.typeId == CellTypeIds.Keycard && obj.TryGetComponent(out GridKeycard keycard))
        {
            keycard.keyColor = cell.color;
            SetSpriteColor(obj, cell.color);
        }
        else if (cell.typeId == CellTypeIds.Gate && obj.TryGetComponent(out GridLaserGate gate))
        {
            gate.gateColor = cell.color;
            SetSpriteColor(obj, cell.color);
        }
        else if (cell.typeId == CellTypeIds.ElectricButton && obj.TryGetComponent(out GridElectricButton electricButton))
        {
            electricButton.SetColor(cell.color);
        }
        else if (cell.typeId == CellTypeIds.RevealWaveButton && obj.TryGetComponent(out GridRevealWaveButton revealWaveButton))
        {
            revealWaveButton.SetColor(cell.color);
        }
        else if (cell.typeId == CellTypeIds.Deflector)
        {
            GridDeflector deflector = obj.GetComponentInChildren<GridDeflector>();
            if (deflector != null) deflector.SetDirection(cell.direction);
        }
        else if (cell.typeId == CellTypeIds.CountdownBlock && obj.TryGetComponent(out GridCountdownBlock countdownBlock))
        {
            CountCellPayload payload = cell.payload as CountCellPayload;
            countdownBlock.SetCount(payload != null ? payload.count : 0);
        }
        else if (cell.typeId == CellTypeIds.StopBlock && obj.TryGetComponent(out GridStopBlock stopBlock))
        {
            CountCellPayload payload = cell.payload as CountCellPayload;
            stopBlock.SetCount(payload != null ? payload.count : 0);
        }
        else if (cell.typeId == CellTypeIds.TurnStateBlock && obj.TryGetComponent(out GridTurnStateBlock turnStateBlock))
        {
            TurnStatePayload payload = cell.payload as TurnStatePayload;
            turnStateBlock.SetInitialState(payload != null && payload.startsRed);
        }
        else if (cell.typeId == CellTypeIds.BlackHole && obj.TryGetComponent(out GridBlackHole blackHole))
        {
            blackHole.SetDirection(cell.direction);
        }
    }

    private static void LoadDraftLinks(LevelEditorContext context)
    {
        context.currentDraftPortals.Clear();
        foreach (PortalPairInfo portal in LevelDataV2Queries.GetPortalPairs(context.currentData))
        {
            context.currentDraftPortals.Add(new PortalData
            {
                entrance = portal.entrance.position,
                entranceDir = portal.entranceExitDirection,
                exit = portal.exit.position,
                exitDir = portal.exitExitDirection,
                portalColor = portal.color
            });
        }

        context.currentDraftElectricWalls.Clear();
        foreach (CellEntityData wallCell in LevelDataV2Queries.GetCells(context.currentData, CellTypeIds.ElectricWall))
        {
            if (wallCell.payload is ElectricWallPayload wallPayload)
            {
                context.currentDraftElectricWalls.Add(new ElectricWallSaveData
                {
                    start = wallPayload.start,
                    end = wallPayload.end,
                    color = wallCell.color
                });
            }
        }
    }

    private static GameObject GetEditorPrefab(LevelEditorContext context, string typeId)
    {
        switch (typeId)
        {
            case CellTypeIds.Keycard: return context.keycardPrefab;
            case CellTypeIds.Gate: return context.gatePrefab;
            case CellTypeIds.ElectricButton: return context.electricButtonPrefab;
            case CellTypeIds.RevealWaveButton: return context.revealWaveButtonPrefab;
            case CellTypeIds.Deflector: return context.deflectorPrefab;
            case CellTypeIds.CountdownBlock: return context.countdownBlockPrefab;
            case CellTypeIds.StopBlock: return context.stopBlockPrefab;
            case CellTypeIds.TurnStateBlock: return context.turnStateBlockPrefab;
            case CellTypeIds.BlackHole: return context.blackHolePrefab;
            default: return null;
        }
    }

    private static void SetSpriteColor(GameObject obj, Color color)
    {
        SpriteRenderer spriteRenderer = obj != null ? obj.GetComponent<SpriteRenderer>() : null;
        if (spriteRenderer != null) spriteRenderer.color = color;
    }

    private static void SpawnPortalVisuals(LevelEditorContext context)
    {
        context.spawnedPortalVisuals.Clear();
        if (context.portalPrefab == null)
        {
            return;
        }

        for (int i = 0; i < context.currentDraftPortals.Count; i++)
        {
            PortalData portal = context.currentDraftPortals[i];
            GameObject entranceObject = Object.Instantiate(context.portalPrefab, new Vector3(portal.entrance.x, portal.entrance.y, 0f), LevelEditorRuntimeHelpers.GetRotationForDir(portal.entranceDir), context.levelContainer);
            GameObject exitObject = Object.Instantiate(context.portalPrefab, new Vector3(portal.exit.x, portal.exit.y, 0f), LevelEditorRuntimeHelpers.GetRotationForDir(portal.exitDir), context.levelContainer);

            SpriteRenderer entranceRenderer = entranceObject.GetComponent<SpriteRenderer>();
            if (entranceRenderer != null) entranceRenderer.color = portal.portalColor;

            SpriteRenderer exitRenderer = exitObject.GetComponent<SpriteRenderer>();
            if (exitRenderer != null) exitRenderer.color = portal.portalColor;

            context.spawnedPortalVisuals.Add(entranceObject);
            context.spawnedPortalVisuals.Add(exitObject);
        }
    }

    private static void SpawnElectricWallVisuals(LevelEditorContext context)
    {
        context.spawnedElectricWallVisuals.Clear();
        if (context.electricWallPrefab == null)
        {
            return;
        }

        for (int i = 0; i < context.currentDraftElectricWalls.Count; i++)
        {
            ElectricWallSaveData wall = context.currentDraftElectricWalls[i];
            GameObject wallObject = Object.Instantiate(context.electricWallPrefab, Vector3.zero, Quaternion.identity, context.levelContainer);
            GridElectricWall wallScript = wallObject.GetComponent<GridElectricWall>();
            if (wallScript != null) wallScript.Initialize(wall.start, wall.end, wall.color, false);
            context.spawnedElectricWallVisuals.Add(wallObject);
        }
    }
}
