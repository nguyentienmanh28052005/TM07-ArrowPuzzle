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
                var keycard = child.GetComponent<GridKeycard>();
                if (keycard != null) 
                { 
                    editor.UI_SetColor(keycard.keyColor);
                    editor.UI_SetTool((int)EditorToolType.KeycardGate); 
                    var keycardState = editor.GetCachedState(EditorToolType.KeycardGate) as PlaceKeycardGateState;
                    if (keycardState != null) keycardState.CurrentSubMode = PlaceKeycardGateState.SubMode.Keycard;
                    return; 
                }
                var gate = child.GetComponent<GridLaserGate>();
                if (gate != null) 
                { 
                    editor.UI_SetColor(gate.gateColor);
                    editor.UI_SetTool((int)EditorToolType.KeycardGate); 
                    var keycardState = editor.GetCachedState(EditorToolType.KeycardGate) as PlaceKeycardGateState;
                    if (keycardState != null) keycardState.CurrentSubMode = PlaceKeycardGateState.SubMode.Gate;
                    return; 
                }
                if (child.GetComponentInChildren<GridDeflector>() != null) { editor.UI_SetTool((int)EditorToolType.Deflector); return; }
                if (child.GetComponent<GridCountdownBlock>() != null) { editor.UI_SetTool((int)EditorToolType.CountdownBlock); return; }
                var electricButton = child.GetComponent<GridElectricButton>();
                if (electricButton != null) 
                { 
                    editor.UI_SetColor(electricButton.buttonColor);
                    editor.UI_SetTool((int)EditorToolType.ElectricCircuit); 
                    var electricState = editor.GetCachedState(EditorToolType.ElectricCircuit) as PlaceElectricCircuitState;
                    if (electricState != null) electricState.CurrentSubMode = PlaceElectricCircuitState.SubMode.Button;
                    return; 
                }
                if (child.GetComponent<GridStopBlock>() != null) { editor.UI_SetTool((int)EditorToolType.StopBlock); return; }
                if (child.GetComponent<GridTurnStateBlock>() != null) { editor.UI_SetTool((int)EditorToolType.TurnStateBlock); return; }
                if (child.GetComponent<GridBlackHole>() != null) { editor.UI_SetTool((int)EditorToolType.BlackHole); return; }
                if (child.GetComponent<GridRevealWaveButton>() != null) { editor.UI_SetTool((int)EditorToolType.RevealWaveButton); return; }
            }

            // ElectricWall check
            GridElectricWall ew = child.GetComponent<GridElectricWall>();
            if (ew != null && ew.ContainsCell(gridPos))
            {
                editor.UI_SetColor(ew.wallColor);
                editor.UI_SetTool((int)EditorToolType.ElectricCircuit);
                var electricState = editor.GetCachedState(EditorToolType.ElectricCircuit) as PlaceElectricCircuitState;
                if (electricState != null) electricState.CurrentSubMode = PlaceElectricCircuitState.SubMode.Wall;
                return;
            }
        }

        // 3. Check Portals (Draft)
        for (int i = 0; i < editor.currentDraftPortals.Count; i++)
        {
            if (editor.currentDraftPortals[i].entrance == gridPos || editor.currentDraftPortals[i].exit == gridPos)
            {
                editor.UI_SetColor(editor.currentDraftPortals[i].portalColor);
                editor.UI_SetTool((int)EditorToolType.Portal);
                return;
            }
        }

        // 4. Check ElectricWalls (Draft)
        for (int i = 0; i < editor.currentDraftElectricWalls.Count; i++)
        {
            if (LevelEditorRuntimeHelpers.IsCellOnElectricWall(gridPos, editor.currentDraftElectricWalls[i]))
            {
                editor.UI_SetColor(editor.currentDraftElectricWalls[i].color);
                editor.UI_SetTool((int)EditorToolType.ElectricCircuit);
                var electricState = editor.GetCachedState(EditorToolType.ElectricCircuit) as PlaceElectricCircuitState;
                if (electricState != null) electricState.CurrentSubMode = PlaceElectricCircuitState.SubMode.Wall;
                return;
            }
        }

        // If clicked on empty space, deselect
        editor.selectedSnakeToModify = null; 
        editor.ClearSelectionHighlight();
    }
}
