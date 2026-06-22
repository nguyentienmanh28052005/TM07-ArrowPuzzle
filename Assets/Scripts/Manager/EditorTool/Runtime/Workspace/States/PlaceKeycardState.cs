using UnityEngine;

public class PlaceKeycardState : EditorStateBase
{
    public PlaceKeycardState(LevelEditorWorkspace editor) : base(editor) {}

    public override void HandleMouseDown(Vector2Int gridPos)
    {
        var k = editor.PlaceGridObject<GridKeycard>(editor.keycardPrefab, gridPos);
        if (k != null)
        {
            k.keyColor = editor.currentColor;
            var sr = k.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = editor.currentColor;
        }
    }
}
