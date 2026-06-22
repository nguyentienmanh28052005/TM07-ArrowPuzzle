using UnityEngine;

public class PlacePortalState : EditorStateBase
{
    public PlacePortalState(LevelEditorWorkspace editor) : base(editor) {}

    public override void OnEnter()
    {
        editor.isPlacingPortalExit = false;

        if (editor.HasPortalWithColor(editor.currentColor))
        {
            Color nextColor = editor.GetNextUnusedPortalColor();
            editor.UI_SetColor(nextColor);
        }
    }

    public override void OnExit()
    {
        Cancel();
    }

    public override void HandleMouseDown(Vector2Int gridPos)
    {
        if (editor.IsPositionOccupied(gridPos)) return;

        if (!editor.isPlacingPortalExit)
        {
            editor.draftPortalEntrance = gridPos;
            editor.draftPortalEntranceDir = editor.currentDir;
            editor.draftPortalColor = editor.currentColor;
            editor.isPlacingPortalExit = true;
        }
        else
        {
            if (gridPos == editor.draftPortalEntrance) return;
            editor.HistoryService?.RecordState(editor.CaptureSnapshot());
            PortalData newPortal = new PortalData
            {
                entrance = editor.draftPortalEntrance,
                entranceDir = editor.draftPortalEntranceDir,
                exit = gridPos,
                exitDir = editor.currentDir,
                portalColor = editor.draftPortalColor
            };
            editor.currentDraftPortals.Add(newPortal);
            editor.isPlacingPortalExit = false;
            editor.RefreshPortalVisuals();
            editor.RebuildOccupantsCache();

            // Tự động chuyển sang màu Portal tiếp theo chưa được sử dụng
            Color nextColor = editor.GetNextUnusedPortalColor();
            editor.UI_SetColor(nextColor);
        }
        editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }

    public override void Cancel()
    {
        if (editor.isPlacingPortalExit)
        {
            editor.isPlacingPortalExit = false;
            editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
        }
    }
}
