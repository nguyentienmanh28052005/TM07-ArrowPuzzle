using UnityEngine;

public class PlaceRevealWaveButtonState : EditorStateBase
{
    public PlaceRevealWaveButtonState(LevelEditorWorkspace editor) : base(editor) {}

    public override void HandleMouseDown(Vector2Int gridPos)
    {
        var rwb = editor.PlaceGridObject<GridRevealWaveButton>(editor.revealWaveButtonPrefab, gridPos);
        if (rwb != null)
        {
            rwb.SetColor(editor.currentColor);
        }
    }
}
