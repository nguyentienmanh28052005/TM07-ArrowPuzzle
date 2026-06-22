using UnityEngine;

public class PlaceGateState : EditorStateBase
{
    public PlaceGateState(LevelEditorWorkspace editor) : base(editor) {}

    public override void HandleMouseDown(Vector2Int gridPos)
    {
        var g = editor.PlaceGridObject<GridLaserGate>(editor.gatePrefab, gridPos);
        if (g != null)
        {
            g.gateColor = editor.currentColor;
            var sr = g.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = editor.currentColor;
        }
    }
}
