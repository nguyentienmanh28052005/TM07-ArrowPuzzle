using UnityEngine;

public class PlaceDeflectorState : EditorStateBase
{
    public PlaceDeflectorState(LevelEditorWorkspace editor) : base(editor) {}

    public override void HandleMouseDown(Vector2Int gridPos)
    {
        if (editor.IsPositionOccupied(gridPos) || editor.deflectorPrefab == null) return;

        editor.HistoryService?.RecordState(editor.CaptureSnapshot());
        GameObject obj = editor.Spawn(editor.deflectorPrefab, new Vector3(gridPos.x, gridPos.y, 0), LevelEditorRuntimeHelpers.GetRotationForDir(editor.currentDir), editor.levelContainer);
        GridDeflector deflector = obj.GetComponentInChildren<GridDeflector>();
        if (deflector != null) deflector.SetDirection(editor.currentDir);

        editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
        editor.RebuildOccupantsCache();
    }
}
