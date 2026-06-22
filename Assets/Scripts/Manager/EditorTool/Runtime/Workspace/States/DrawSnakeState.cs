using UnityEngine;
using System.Collections.Generic;

public class DrawSnakeState : EditorStateBase
{
    public DrawSnakeState(LevelEditorWorkspace editor) : base(editor) {}

    public override void HandleMouseDown(Vector2Int gridPos)
    {
        if (editor.IsPositionOccupied(gridPos) || editor.IsTooCloseToOtherSnakes(gridPos)) return;
        if (editor.currentSnakeObj == null)
        {
            editor.HistoryService?.RecordState(editor.CaptureSnapshot());
            editor.CreateHead(gridPos);
        }
        else 
        {
            Vector2Int headPos = editor.currentDraftNodes[0];
            Vector2Int lastPos = editor.currentDraftNodes[editor.currentDraftNodes.Count - 1];

            int distToTail = Mathf.Abs(gridPos.x - lastPos.x) + Mathf.Abs(gridPos.y - lastPos.y);
            int distToHead = Mathf.Abs(gridPos.x - headPos.x) + Mathf.Abs(gridPos.y - headPos.y);

            if (editor.currentDraftNodes.Count == 1)
            {
                editor.CreateHeadSegment(gridPos);
                editor.UpdateAutoDirection();
            }
            else if (distToTail == 1)
            {
                editor.CreateBodySegment(gridPos);
                editor.UpdateAutoDirection();
            }
            else if (distToHead == 1)
            {
                editor.CreateHeadSegment(gridPos);
                editor.UpdateAutoDirection();
            }
        }
        editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999); 
    }

    public override void HandleMouseHold(Vector2Int gridPos)
    {
        if (editor.currentSnakeObj == null || editor.currentDraftNodes.Count == 0) return;
        Vector2Int headPos = editor.currentDraftNodes[0];
        Vector2Int lastPos = editor.currentDraftNodes[editor.currentDraftNodes.Count - 1];
        if (gridPos == lastPos || gridPos == headPos) return; 

        if (editor.currentDraftNodes.Count >= 2)
        {
            if (gridPos == editor.currentDraftNodes[editor.currentDraftNodes.Count - 2])
            {
                editor.RetractTailSegment();
                return;
            }
            if (gridPos == editor.currentDraftNodes[1])
            {
                editor.RetractHeadSegment();
                return;
            }
        }

        int distToTail = Mathf.Abs(gridPos.x - lastPos.x) + Mathf.Abs(gridPos.y - lastPos.y);
        int distToHead = Mathf.Abs(gridPos.x - headPos.x) + Mathf.Abs(gridPos.y - headPos.y);

        if (editor.currentDraftNodes.Count == 1)
        {
            if (distToHead == 1)
            {
                if (!editor.IsPositionOccupied(gridPos) && !editor.IsTooCloseToOtherSnakes(gridPos))
                {
                    editor.CreateHeadSegment(gridPos);
                    editor.UpdateAutoDirection();
                    editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                }
            }
            else if (distToHead > 1)
            {
                List<Vector2Int> path = editor.GetInterpolatedPath(headPos, gridPos);
                bool addedAny = false;
                foreach (Vector2Int step in path)
                {
                    editor.CreateHeadSegment(step);
                    addedAny = true;
                }
                if (addedAny)
                {
                    editor.UpdateAutoDirection();
                    editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                }
            }
        }
        else if (distToTail <= distToHead)
        {
            if (distToTail == 1)
            {
                if (!editor.IsPositionOccupied(gridPos) && !editor.IsTooCloseToOtherSnakes(gridPos))
                {
                    editor.CreateBodySegment(gridPos);
                    editor.UpdateAutoDirection();
                    editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                }
            }
            else if (distToTail > 1)
            {
                List<Vector2Int> path = editor.GetInterpolatedPath(lastPos, gridPos);
                bool addedAny = false;
                foreach (Vector2Int step in path)
                {
                    editor.CreateBodySegment(step);
                    addedAny = true;
                }
                if (addedAny)
                {
                    editor.UpdateAutoDirection();
                    editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                }
            }
        }
        else
        {
            if (distToHead == 1)
            {
                if (!editor.IsPositionOccupied(gridPos) && !editor.IsTooCloseToOtherSnakes(gridPos))
                {
                    editor.CreateHeadSegment(gridPos);
                    editor.UpdateAutoDirection();
                    editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                }
            }
            else if (distToHead > 1)
            {
                List<Vector2Int> path = editor.GetInterpolatedPath(headPos, gridPos);
                bool addedAny = false;
                foreach (Vector2Int step in path)
                {
                    editor.CreateHeadSegment(step);
                    addedAny = true;
                }
                if (addedAny)
                {
                    editor.UpdateAutoDirection();
                    editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
                }
            }
        }
    }

    public override void Cancel()
    {
        if (editor.currentSnakeObj != null)
        {
            Object.Destroy(editor.currentSnakeObj);
            editor.currentSnakeObj = null;
            editor.currentSnakeScript = null;
            editor.currentDraftNodes.Clear();
            editor.lastCalculatedGridPos = new Vector2Int(-9999, -9999);
        }
    }

    public override void Finish()
    {
        editor.UI_FinishSnake();
    }
}
