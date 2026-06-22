using UnityEngine;

public class PlaceKeycardGateState : EditorStateBase
{
    public enum SubMode { Keycard, Gate }
    private SubMode currentSubMode = SubMode.Keycard;

    public PlaceKeycardGateState(LevelEditorWorkspace editor) : base(editor) {}

    public SubMode CurrentSubMode 
    { 
        get => currentSubMode; 
        set 
        { 
            currentSubMode = value; 
            editor.UpdateToolText(); 
        } 
    }

    public override void OnEnter()
    {
        CurrentSubMode = editor.HasKeycardWithColor(editor.currentColor) ? SubMode.Gate : SubMode.Keycard;
    }

    public override string GetToolStatusText() => $"({currentSubMode})";

    public override void HandleSpaceKeyPressed()
    {
        CurrentSubMode = currentSubMode == SubMode.Keycard ? SubMode.Gate : SubMode.Keycard;
        if (currentSubMode == SubMode.Keycard)
        {
            Color nextColor = editor.GetNextUnusedKeycardColor();
            editor.UI_SetColor(nextColor);
        }
    }

    public override void HandleColorSelected(Color color)
    {
        CurrentSubMode = editor.HasKeycardWithColor(color) ? SubMode.Gate : SubMode.Keycard;
    }

    public override void HandleMouseDown(Vector2Int gridPos)
    {
        if (currentSubMode == SubMode.Keycard)
        {
            var k = editor.PlaceGridObject<GridKeycard>(editor.keycardPrefab, gridPos);
            if (k != null)
            {
                k.keyColor = editor.currentColor;
                var sr = k.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = editor.currentColor;

                // Tự động chuyển sang đặt Cổng Laser (Gate) cùng màu
                CurrentSubMode = SubMode.Gate;
            }
        }
        else
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
}
