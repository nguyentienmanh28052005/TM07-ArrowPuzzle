using UnityEngine;

public class PlaceTurnStateBlockState : EditorStateBase
{
    public PlaceTurnStateBlockState(LevelEditorWorkspace editor) : base(editor) {}

    public override void HandleMouseDown(Vector2Int gridPos)
    {
        if (editor.IsPositionOccupied(gridPos) || editor.turnStateBlockPrefab == null) return;

        editor.HistoryService?.RecordState(editor.CaptureSnapshot());
        GameObject obj = editor.Spawn(editor.turnStateBlockPrefab, new Vector3(gridPos.x, gridPos.y, 0), Quaternion.identity, editor.levelContainer);
        GridTurnStateBlock block = obj.GetComponent<GridTurnStateBlock>();
        if (block != null) block.SetInitialState(editor.ShouldUseRedTurnState());

        editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
        editor.RebuildOccupantsCache();
    }
}
