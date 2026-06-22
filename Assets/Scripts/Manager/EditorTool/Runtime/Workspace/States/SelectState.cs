using UnityEngine;

public class SelectState : EditorStateBase
{
    public SelectState(LevelEditorWorkspace editor) : base(editor) {}

    public override void HandleMouseDown(Vector2Int gridPos)
    {
        // 1. Check if it's a snake/arrow
        EditorSnakeVisual sb = editor.GetSnakeAtGridPos(gridPos);
        if (sb != null)
        {
            editor.selectedSnakeToModify = sb;
            editor.currentDir = sb.direction;
            editor.currentColor = sb.snakeColor;
            editor.currentColor.a = 1.0f;
            editor.UpdateToolText();
            if (editor.colorPreviewImage != null) editor.colorPreviewImage.color = editor.currentColor;
            editor.UpdateSelectionHighlight(sb);
            
            // Automatically switch to Draw tool for editing!
            editor.UI_SetTool((int)EditorToolType.Draw);
            return;
        }

        // 2. Check other object types under levelContainer
        foreach (Transform child in editor.levelContainer)
        {
            if (child == null) continue;
            if (Mathf.RoundToInt(child.position.x) == gridPos.x && Mathf.RoundToInt(child.position.y) == gridPos.y)
            {
                if (child.GetComponent<GridKeycard>() != null) { editor.UI_SetTool((int)EditorToolType.Keycard); return; }
                if (child.GetComponent<GridLaserGate>() != null) { editor.UI_SetTool((int)EditorToolType.Gate); return; }
                if (child.GetComponentInChildren<GridDeflector>() != null) { editor.UI_SetTool((int)EditorToolType.Deflector); return; }
                if (child.GetComponent<GridCountdownBlock>() != null) { editor.UI_SetTool((int)EditorToolType.CountdownBlock); return; }
                if (child.GetComponent<GridElectricButton>() != null) { editor.UI_SetTool((int)EditorToolType.ElectricButton); return; }
                if (child.GetComponent<GridStopBlock>() != null) { editor.UI_SetTool((int)EditorToolType.StopBlock); return; }
                if (child.GetComponent<GridTurnStateBlock>() != null) { editor.UI_SetTool((int)EditorToolType.TurnStateBlock); return; }
                if (child.GetComponent<GridBlackHole>() != null) { editor.UI_SetTool((int)EditorToolType.BlackHole); return; }
                if (child.GetComponent<GridRevealWaveButton>() != null) { editor.UI_SetTool((int)EditorToolType.RevealWaveButton); return; }
            }

            // ElectricWall check
            GridElectricWall ew = child.GetComponent<GridElectricWall>();
            if (ew != null && ew.ContainsCell(gridPos))
            {
                editor.UI_SetTool((int)EditorToolType.ElectricWall);
                return;
            }
        }

        // 3. Check Portals (Draft)
        for (int i = 0; i < editor.currentDraftPortals.Count; i++)
        {
            if (editor.currentDraftPortals[i].entrance == gridPos || editor.currentDraftPortals[i].exit == gridPos)
            {
                editor.UI_SetTool((int)EditorToolType.Portal);
                return;
            }
        }

        // 4. Check ElectricWalls (Draft)
        for (int i = 0; i < editor.currentDraftElectricWalls.Count; i++)
        {
            if (LevelEditorRuntimeHelpers.IsCellOnElectricWall(gridPos, editor.currentDraftElectricWalls[i]))
            {
                editor.UI_SetTool((int)EditorToolType.ElectricWall);
                return;
            }
        }

        // If clicked on empty space, deselect
        editor.selectedSnakeToModify = null; 
        editor.ClearSelectionHighlight();
    }
}
