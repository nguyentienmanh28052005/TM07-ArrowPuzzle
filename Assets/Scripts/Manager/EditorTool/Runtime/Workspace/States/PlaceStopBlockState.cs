using UnityEngine;

public class PlaceStopBlockState : EditorStateBase
{
    public PlaceStopBlockState(LevelEditorWorkspace editor) : base(editor) {}

    public override void HandleMouseDown(Vector2Int gridPos)
    {
        if (editor.IsPositionOccupied(gridPos) || editor.stopBlockPrefab == null) return;

        editor.HistoryService?.RecordState(editor.CaptureSnapshot());
        if (editor.inputCountdownValue != null)
            int.TryParse(editor.inputCountdownValue.text, out editor.editorCountdownValue);
        if (editor.editorCountdownValue < 1)
        {
            editor.editorCountdownValue = 1;
            if (editor.inputCountdownValue != null)
                editor.inputCountdownValue.text = "1";
        }

        GameObject obj = editor.Spawn(editor.stopBlockPrefab, new Vector3(gridPos.x, gridPos.y, 0), Quaternion.identity, editor.levelContainer);
        GridStopBlock block = obj.GetComponent<GridStopBlock>();
        if (block != null) block.SetCount(editor.editorCountdownValue);

        editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
        editor.RebuildOccupantsCache();
    }
}
