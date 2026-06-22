using UnityEngine;

public class PlaceElectricCircuitState : EditorStateBase
{
    public enum SubMode { Button, Wall }
    private SubMode currentSubMode = SubMode.Button;

    public PlaceElectricCircuitState(LevelEditorWorkspace editor) : base(editor) {}

    public SubMode CurrentSubMode 
    { 
        get => currentSubMode; 
        set 
        { 
            currentSubMode = value; 
            editor.UpdateToolText(); 
        } 
    }

    public override void OnEnter()
    {
        editor.isPlacingElectricWallEnd = false;
        CurrentSubMode = editor.HasElectricButtonWithColor(editor.currentColor) ? SubMode.Wall : SubMode.Button;
    }

    public override void OnExit()
    {
        Cancel();
    }

    public override string GetToolStatusText() => $"({currentSubMode})";

    public override void HandleSpaceKeyPressed()
    {
        CurrentSubMode = currentSubMode == SubMode.Button ? SubMode.Wall : SubMode.Button;
        editor.isPlacingElectricWallEnd = false;
        
        if (currentSubMode == SubMode.Button)
        {
            Color nextColor = editor.GetNextUnusedElectricButtonColor();
            editor.UI_SetColor(nextColor);
        }
    }

    public override void HandleColorSelected(Color color)
    {
        CurrentSubMode = editor.HasElectricButtonWithColor(color) ? SubMode.Wall : SubMode.Button;
    }

    public override void HandleMouseDown(Vector2Int gridPos)
    {
        if (currentSubMode == SubMode.Button)
        {
            var eb = editor.PlaceGridObject<GridElectricButton>(editor.electricButtonPrefab, gridPos);
            if (eb != null)
            {
                eb.SetColor(editor.currentColor);
                // Tự động chuyển sang đặt Tường điện (Wall) cùng màu
                CurrentSubMode = SubMode.Wall;
            }
        }
        else
        {
            // Wall placement (two-click flow)
            if (!editor.isPlacingElectricWallEnd)
            {
                if (editor.IsPositionOccupied(gridPos)) return;
                editor.draftElectricWallStart = gridPos;
                editor.draftElectricWallColor = editor.currentColor;
                editor.isPlacingElectricWallEnd = true;
            }
            else
            {
                if (gridPos == editor.draftElectricWallStart) return;
                if (!LevelEditorRuntimeHelpers.IsElectricWallAligned(editor.draftElectricWallStart, gridPos)) 
                { 
                    editor.isPlacingElectricWallEnd = false; 
                    return; 
                }
                if (!editor.IsElectricWallPathClear(editor.draftElectricWallStart, gridPos)) 
                { 
                    editor.isPlacingElectricWallEnd = false; 
                    return; 
                }

                editor.HistoryService?.RecordState(editor.CaptureSnapshot());
                ElectricWallSaveData newWall = new ElectricWallSaveData
                {
                    start = editor.draftElectricWallStart,
                    end = gridPos,
                    color = editor.draftElectricWallColor
                };

                editor.currentDraftElectricWalls.Add(newWall);
                editor.isPlacingElectricWallEnd = false;
                editor.RefreshElectricWallVisuals();
                editor.RebuildOccupantsCache();
            }
        }

        editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    public override void Cancel()
    {
        if (editor.isPlacingElectricWallEnd)
        {
            editor.isPlacingElectricWallEnd = false;
            editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
        }
    }
}
