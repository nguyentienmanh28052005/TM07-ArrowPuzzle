using UnityEngine;

public class PlaceElectricButtonState : EditorStateBase
{
    public PlaceElectricButtonState(LevelEditorWorkspace editor) : base(editor) {}

    public override void HandleMouseDown(Vector2Int gridPos)
    {
        var eb = editor.PlaceGridObject<GridElectricButton>(editor.electricButtonPrefab, gridPos);
        if (eb != null)
        {
            eb.SetColor(editor.currentColor);
        }
    }
}
