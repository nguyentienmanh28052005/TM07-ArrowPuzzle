using UnityEngine;

public class PlaceElectricWallState : EditorStateBase
{
    public PlaceElectricWallState(LevelEditorWorkspace editor) : base(editor) {}

    public override void OnEnter()
    {
        editor.isPlacingElectricWallEnd = false;
    }

    public override void OnExit()
    {
        Cancel();
    }

    public override void HandleMouseDown(Vector2Int gridPos)
    {
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
