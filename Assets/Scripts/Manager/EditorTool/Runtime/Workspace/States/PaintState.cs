using UnityEngine;
using System.Collections.Generic;

public class PaintState : EditorStateBase
{
    public PaintState(LevelEditorWorkspace editor) : base(editor) {}

    public override void HandleMouseDown(Vector2Int gridPos)
    {
        EditorSnakeVisual sb = editor.GetSnakeAtGridPos(gridPos);
        if (sb != null) sb.SetColorImmediatePublic(editor.currentColor);

        bool hasLinkedGroupColor = false;
        Color linkedGroupColor = Color.white;

        bool willPaint = sb != null;
        if (!willPaint)
        {
            foreach (Transform child in editor.levelContainer)
            {
                if (Mathf.RoundToInt(child.position.x) == gridPos.x && Mathf.RoundToInt(child.position.y) == gridPos.y)
                {
                    willPaint = true;
                    break;
                }
            }
        }
        if (!willPaint)
        {
            for (int i = 0; i < editor.currentDraftPortals.Count; i++)
            {
                if (editor.currentDraftPortals[i].entrance == gridPos || editor.currentDraftPortals[i].exit == gridPos)
                {
                    willPaint = true;
                    break;
                }
            }
        }
        if (!willPaint)
        {
            for (int i = 0; i < editor.currentDraftElectricWalls.Count; i++)
            {
                if (LevelEditorRuntimeHelpers.IsCellOnElectricWall(gridPos, editor.currentDraftElectricWalls[i]))
                {
                    willPaint = true;
                    break;
                }
            }
        }

        if (willPaint)
        {
            editor.HistoryService?.RecordState(editor.CaptureSnapshot());
        }

        foreach (Transform child in editor.levelContainer)
        {
            if (Mathf.RoundToInt(child.position.x) == gridPos.x && Mathf.RoundToInt(child.position.y) == gridPos.y)
            {
                if (child.TryGetComponent(out GridTurnStateBlock turnStateBlock))
                {
                    turnStateBlock.SetInitialState(editor.ShouldUseRedTurnState());
                    return;
                }

                if (child.TryGetComponent(out GridKeycard k))
                {
                    hasLinkedGroupColor = true;
                    linkedGroupColor = k.keyColor;
                    break;
                }

                if (child.TryGetComponent(out GridElectricButton eb))
                {
                    hasLinkedGroupColor = true;
                    linkedGroupColor = eb.buttonColor;
                    break;
                }

                if (child.TryGetComponent(out GridRevealWaveButton revealWaveButton))
                {
                    revealWaveButton.SetColor(editor.currentColor);
                    return;
                }

                if (child.TryGetComponent(out GridLaserGate g))
                {
                    hasLinkedGroupColor = true;
                    linkedGroupColor = g.gateColor;
                    break;
                }

                GridElectricWall ew = child.GetComponent<GridElectricWall>();
                if (ew != null)
                {
                    hasLinkedGroupColor = true;
                    linkedGroupColor = ew.wallColor;
                    break;
                }
            }
        }

        if (hasLinkedGroupColor)
        {
            foreach (Transform child in editor.levelContainer)
            {
                if (child.TryGetComponent(out GridKeycard k) && LevelEditorRuntimeHelpers.ColorsMatch(k.keyColor, linkedGroupColor))
                {
                    k.keyColor = editor.currentColor;
                    SpriteRenderer keySr = child.GetComponent<SpriteRenderer>();
                    if (keySr != null) keySr.color = editor.currentColor;
                }

                if (child.TryGetComponent(out GridElectricButton eb) && LevelEditorRuntimeHelpers.ColorsMatch(eb.buttonColor, linkedGroupColor))
                {
                    eb.SetColor(editor.currentColor);
                }

                if (child.TryGetComponent(out GridLaserGate g) && LevelEditorRuntimeHelpers.ColorsMatch(g.gateColor, linkedGroupColor))
                {
                    g.gateColor = editor.currentColor;
                    SpriteRenderer gateSr = child.GetComponent<SpriteRenderer>();
                    if (gateSr != null) gateSr.color = editor.currentColor;
                }

                GridElectricWall ew = child.GetComponent<GridElectricWall>();
                if (ew != null && LevelEditorRuntimeHelpers.ColorsMatch(ew.wallColor, linkedGroupColor))
                {
                    ew.SetColor(editor.currentColor);
                }
            }
        }
        
        for (int i = 0; i < editor.currentDraftPortals.Count; i++)
        {
            if (editor.currentDraftPortals[i].entrance == gridPos || editor.currentDraftPortals[i].exit == gridPos)
            {
                editor.currentDraftPortals[i].portalColor = editor.currentColor;
                editor.RefreshPortalVisuals();
                return;
            }
        }

        for (int i = 0; i < editor.currentDraftElectricWalls.Count; i++)
        {
            if (LevelEditorRuntimeHelpers.IsCellOnElectricWall(gridPos, editor.currentDraftElectricWalls[i]))
            {
                ElectricWallSaveData wall = editor.currentDraftElectricWalls[i];
                wall.color = editor.currentColor;
                editor.currentDraftElectricWalls[i] = wall;
                editor.RefreshElectricWallVisuals();
                return;
            }
        }
    }
}
