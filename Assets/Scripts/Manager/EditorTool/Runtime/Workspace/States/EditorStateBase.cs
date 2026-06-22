using UnityEngine;

public abstract class EditorStateBase
{
    protected readonly LevelEditorWorkspace editor;

    protected EditorStateBase(LevelEditorWorkspace editor)
    {
        this.editor = editor;
    }

    public virtual void OnEnter() {}
    public virtual void OnExit() {}
    public virtual void HandleMouseDown(Vector2Int gridPos) {}
    public virtual void HandleMouseHold(Vector2Int gridPos) {}
    public virtual void UpdatePreview(Vector2Int gridPos) {}
    public virtual void Cancel() {}
    public virtual void Finish() {}

    public virtual string GetToolStatusText() => string.Empty;
    public virtual void HandleSpaceKeyPressed()
    {
        Finish();
    }
    public virtual void HandleColorSelected(Color color) {}
}
