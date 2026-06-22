using UnityEngine;
using System.Collections.Generic;

public class EraseState : EditorStateBase
{
    public EraseState(LevelEditorWorkspace editor) : base(editor) {}

    public override void HandleMouseDown(Vector2Int gridPos)
    {
        bool trimFromHead = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // 1. If currently drawing a snake, erase parts of it
        if (editor.currentSnakeObj != null && editor.currentDraftNodes != null && editor.currentDraftNodes.Count > 0)
        {
            int draftIndex = editor.currentDraftNodes.FindIndex(n => n == gridPos);
            if (draftIndex >= 0)
            {
                editor.HistoryService?.RecordState(editor.CaptureSnapshot());
                if (trimFromHead)
                {
                    editor.currentDraftNodes.RemoveRange(0, draftIndex + 1);
                }
                else
                {
                    int removeCount = editor.currentDraftNodes.Count - draftIndex;
                    editor.currentDraftNodes.RemoveRange(draftIndex, removeCount);
                }

                if (editor.currentDraftNodes.Count == 0)
                {
                    editor.Recycle(editor.currentSnakeObj);
                    editor.currentSnakeObj = null;
                    editor.currentSnakeScript = null;
                }
                else
                {
                    if (trimFromHead)
                    {
                        Vector2Int newHead = editor.currentDraftNodes[0];
                        editor.currentSnakeObj.transform.position = new Vector3(newHead.x, newHead.y, 0);
                        if (editor.currentSnakeScript != null) editor.currentSnakeScript.SetArrowWorldPosition(newHead);
                    }
                    editor.UpdateSnakeLinePreview();
                }

                editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                editor.RebuildOccupantsCache();
                return;
            }
        }

        // 2. Erase finished snake segment
        EditorSnakeVisual sb = editor.GetSnakeAtGridPos(gridPos);
        if (sb != null)
        {
            if (sb.LogicNodes != null)
            {
                int index = sb.LogicNodes.FindIndex(n => n == gridPos);
                if (index >= 0)
                {
                    editor.HistoryService?.RecordState(editor.CaptureSnapshot());
                    if (trimFromHead)
                    {
                        sb.LogicNodes.RemoveRange(0, index + 1);
                    }
                    else
                    {
                        int removeCount = sb.LogicNodes.Count - index;
                        sb.LogicNodes.RemoveRange(index, removeCount);
                    }

                    if (sb.LogicNodes.Count == 0)
                    {
                        if (editor.selectedSnakeToModify == sb) { editor.selectedSnakeToModify = null; editor.ClearSelectionHighlight(); }
                        editor.Recycle(sb.gameObject);
                    }
                    else
                    {
                        sb.Initialize(sb.direction, new List<Vector2Int>(sb.LogicNodes), sb.snakeColor, sb.HasArrowShadow);
                        if (editor.selectedSnakeToModify == sb)
                        {
                            editor.UpdateSelectionHighlight(sb);
                        }
                    }

                    editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                    editor.RebuildOccupantsCache();
                    return;
                }
            }

            if (editor.selectedSnakeToModify == sb) { editor.selectedSnakeToModify = null; editor.ClearSelectionHighlight(); }
            editor.Recycle(sb.gameObject);
            editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
            editor.RebuildOccupantsCache();
            return;
        }

        // 3. Erase other objects in container
        foreach (Transform child in editor.levelContainer)
        {
            if (child == null) continue;
            GridDeflector deflector = child.GetComponentInChildren<GridDeflector>();
            if ((child.GetComponent<GridKeycard>() != null 
                 || child.GetComponent<GridLaserGate>() != null 
                 || child.GetComponent<GridElectricButton>() != null 
                 || child.GetComponent<GridRevealWaveButton>() != null 
                 || deflector != null 
                 || child.GetComponent<GridCountdownBlock>() != null 
                 || child.GetComponent<GridStopBlock>() != null 
                 || child.GetComponent<GridTurnStateBlock>() != null 
                 || child.GetComponent<GridBlackHole>() != null)
                && Mathf.RoundToInt(child.position.x) == gridPos.x 
                && Mathf.RoundToInt(child.position.y) == gridPos.y)
            {
                editor.HistoryService?.RecordState(editor.CaptureSnapshot());
                editor.Recycle(child.gameObject);
                editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                editor.RebuildOccupantsCache();
                return;
            }
        }

        // 4. Erase electric walls
        if (editor.TryRemoveElectricWallAtPos(gridPos))
        {
            editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
            editor.RebuildOccupantsCache();
            return;
        }

        // 5. Erase portals
        for (int i = editor.currentDraftPortals.Count - 1; i >= 0; i--)
        {
            if (editor.currentDraftPortals[i].entrance == gridPos || editor.currentDraftPortals[i].exit == gridPos)
            {
                editor.HistoryService?.RecordState(editor.CaptureSnapshot());
                editor.currentDraftPortals.RemoveAt(i);
                editor.RefreshPortalVisuals();
                editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                editor.RebuildOccupantsCache();
                return;
            }
        }
    }
}
