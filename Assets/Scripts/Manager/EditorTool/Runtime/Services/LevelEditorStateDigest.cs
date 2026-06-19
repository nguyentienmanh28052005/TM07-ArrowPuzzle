using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class LevelEditorStateDigest
{
    public static string Build(LevelEditorContext context, LevelDataV2 currentData)
    {
        StringBuilder builder = new StringBuilder(2048);
        builder.Append("difficulty=").Append(currentData != null ? (int)currentData.levelDifficulty : -1).Append(';');
        builder.Append("time=").Append(currentData != null ? currentData.timeLimit : 0f).Append(';');
        builder.Append("coins=").Append(currentData != null ? currentData.rewardCoins : 0f).Append(';');
        builder.Append("diamonds=").Append(currentData != null ? currentData.rewardDiamonds : 0f).Append(';');

        List<string> entries = new List<string>();

        if (context != null && context.levelContainer != null)
        {
            foreach (Transform child in context.levelContainer)
            {
                if (child == null) continue;
                if (context.currentSelectionGlowObj != null && child.gameObject == context.currentSelectionGlowObj) continue;
                AppendChildEntries(child, entries);
            }
        }

        if (context != null && context.currentDraftNodes != null && context.currentDraftNodes.Count > 0)
        {
            builder.Append("draftSnake=").Append((int)context.currentDir).Append('|').Append(ColorToHex(context.currentColor)).Append('|');
            for (int i = 0; i < context.currentDraftNodes.Count; i++)
            {
                Vector2Int node = context.currentDraftNodes[i];
                builder.Append(node.x).Append(',').Append(node.y).Append(';');
            }
        }

        if (context != null && context.currentDraftPortals != null)
        {
            for (int i = 0; i < context.currentDraftPortals.Count; i++)
            {
                PortalData portal = context.currentDraftPortals[i];
                entries.Add($"portal|{portal.entrance.x},{portal.entrance.y}|{(int)portal.entranceDir}|{portal.exit.x},{portal.exit.y}|{(int)portal.exitDir}|{ColorToHex(portal.portalColor)}");
            }
        }

        if (context != null && context.currentDraftElectricWalls != null)
        {
            for (int i = 0; i < context.currentDraftElectricWalls.Count; i++)
            {
                ElectricWallSaveData wall = context.currentDraftElectricWalls[i];
                entries.Add($"electricWall|{wall.start.x},{wall.start.y}|{wall.end.x},{wall.end.y}|{ColorToHex(wall.color)}");
            }
        }

        if (context != null)
        {
            builder.Append("isPlacingPortalExit=").Append(context.isPlacingPortalExit ? 1 : 0).Append(';');
            builder.Append("draftPortalEntrance=").Append(context.draftPortalEntrance.x).Append(',').Append(context.draftPortalEntrance.y).Append(';');
            builder.Append("draftPortalEntranceDir=").Append((int)context.draftPortalEntranceDir).Append(';');
            builder.Append("draftPortalColor=").Append(ColorToHex(context.draftPortalColor)).Append(';');

            builder.Append("isPlacingElectricWallEnd=").Append(context.isPlacingElectricWallEnd ? 1 : 0).Append(';');
            builder.Append("draftElectricWallStart=").Append(context.draftElectricWallStart.x).Append(',').Append(context.draftElectricWallStart.y).Append(';');
            builder.Append("draftElectricWallColor=").Append(ColorToHex(context.draftElectricWallColor)).Append(';');
        }

        entries.Sort();
        for (int i = 0; i < entries.Count; i++)
        {
            builder.Append(entries[i]).Append('\n');
        }

        return builder.ToString();
    }

    private static void AppendChildEntries(Transform child, List<string> entries)
    {
        EditorSnakeVisual snakeVisual = child.GetComponent<EditorSnakeVisual>();
        if (snakeVisual != null && snakeVisual.LogicNodes != null && snakeVisual.LogicNodes.Count > 0)
        {
            StringBuilder snakeBuilder = new StringBuilder();
            snakeBuilder.Append("snake|").Append((int)snakeVisual.direction).Append('|').Append(ColorToHex(snakeVisual.snakeColor)).Append('|').Append(snakeVisual.HasArrowShadow ? 1 : 0).Append('|');
            for (int i = 0; i < snakeVisual.LogicNodes.Count; i++)
            {
                Vector2Int node = snakeVisual.LogicNodes[i];
                snakeBuilder.Append(node.x).Append(',').Append(node.y).Append(';');
            }
            entries.Add(snakeBuilder.ToString());
        }

        Vector2Int childCell = new Vector2Int(Mathf.RoundToInt(child.position.x), Mathf.RoundToInt(child.position.y));

        if (child.TryGetComponent(out GridKeycard keycard))
            entries.Add($"keycard|{childCell.x},{childCell.y}|{ColorToHex(keycard.keyColor)}");
        if (child.TryGetComponent(out GridLaserGate gate))
            entries.Add($"gate|{childCell.x},{childCell.y}|{ColorToHex(gate.gateColor)}");
        if (child.TryGetComponent(out GridElectricButton electricButton))
            entries.Add($"electricButton|{childCell.x},{childCell.y}|{ColorToHex(electricButton.buttonColor)}");
        if (child.TryGetComponent(out GridRevealWaveButton revealWaveButton))
            entries.Add($"revealWave|{childCell.x},{childCell.y}|{ColorToHex(revealWaveButton.buttonColor)}");

        GridDeflector deflector = child.GetComponentInChildren<GridDeflector>();
        if (deflector != null)
        {
            Vector2Int deflectorCell = new Vector2Int(Mathf.RoundToInt(deflector.transform.position.x), Mathf.RoundToInt(deflector.transform.position.y));
            entries.Add($"deflector|{deflectorCell.x},{deflectorCell.y}|{(int)deflector.direction}");
        }

        if (child.TryGetComponent(out GridCountdownBlock countdownBlock))
            entries.Add($"countdown|{childCell.x},{childCell.y}|{countdownBlock.count}");
        if (child.TryGetComponent(out GridStopBlock stopBlock))
            entries.Add($"stopBlock|{childCell.x},{childCell.y}|{stopBlock.count}");
        if (child.TryGetComponent(out GridTurnStateBlock turnStateBlock))
            entries.Add($"turnState|{childCell.x},{childCell.y}|{(turnStateBlock.IsRed ? 1 : 0)}");
        if (child.TryGetComponent(out GridBlackHole blackHole))
            entries.Add($"blackHole|{childCell.x},{childCell.y}|{(int)blackHole.direction}");
        if (child.TryGetComponent(out GridElectricWall wall))
            entries.Add($"wallVisual|{childCell.x},{childCell.y}|{ColorToHex(wall.wallColor)}");
    }

    private static string ColorToHex(Color color)
    {
        return ColorUtility.ToHtmlStringRGBA(color);
    }
}
