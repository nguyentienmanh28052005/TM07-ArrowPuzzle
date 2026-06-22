using UnityEngine;

public class PlaceArrowShadowState : EditorStateBase
{
    public PlaceArrowShadowState(LevelEditorWorkspace editor) : base(editor) {}

    public override void HandleMouseDown(Vector2Int gridPos)
    {
        EditorSnakeVisual snake = editor.GetSnakeAtGridPos(gridPos);
        if (snake == null) return;

        editor.HistoryService?.RecordState(editor.CaptureSnapshot());
        snake.SetArrowShadowEnabled(!snake.HasArrowShadow);
        editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
    }
}
