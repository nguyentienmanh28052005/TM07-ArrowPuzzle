using UnityEngine;

public class PlaceBlackHoleState : EditorStateBase
{
    public PlaceBlackHoleState(LevelEditorWorkspace editor) : base(editor) {}

    public override void HandleMouseDown(Vector2Int gridPos)
    {
        foreach (Transform child in editor.levelContainer)
        {
            if (child == null) continue;
            if (child.TryGetComponent(out GridBlackHole existingBlackHole)
                && Mathf.RoundToInt(child.position.x) == gridPos.x
                && Mathf.RoundToInt(child.position.y) == gridPos.y)
            {
                editor.HistoryService?.RecordState(editor.CaptureSnapshot());
                existingBlackHole.SetDirection(editor.currentDir);
                child.rotation = LevelEditorRuntimeHelpers.GetRotationForDir(editor.currentDir);
                editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                return;
            }
        }

        if (editor.IsPositionOccupied(gridPos) || editor.blackHolePrefab == null) return;

        editor.HistoryService?.RecordState(editor.CaptureSnapshot());
        GameObject obj = editor.Spawn(editor.blackHolePrefab, new Vector3(gridPos.x, gridPos.y, 0), LevelEditorRuntimeHelpers.GetRotationForDir(editor.currentDir), editor.levelContainer);
        GridBlackHole blackHole = obj.GetComponent<GridBlackHole>();
        if (blackHole != null) blackHole.SetDirection(editor.currentDir);

        editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
        editor.RebuildOccupantsCache();
    }
}
