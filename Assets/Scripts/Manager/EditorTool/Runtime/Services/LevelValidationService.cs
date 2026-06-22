using UnityEngine;

public sealed class LevelValidationService
{
    public bool ValidateCurrent(LevelEditorWorkspace editor, out string message)
    {
        if (editor == null)
        {
            message = "Level editor workspace is unavailable.";
            return false;
        }

        if (!editor.HasEditableContent())
        {
            message = "No content to validate.";
            return false;
        }

        // Rule 1: Tất cả các Arrow (Rắn/Mũi tên) phải có độ dài tối thiểu 2 ô
        if (!ValidateArrowMinLength(editor, out message))
        {
            return false;
        }

        // Rule 2: Tất cả các Arrow phải thoát được (Không bị kẹt/deadlock)
        if (!editor.TryValidateCurrentEditorLevel(out message))
        {
            return false;
        }

        return true;
    }

    private bool ValidateArrowMinLength(LevelEditorWorkspace editor, out string message)
    {
        message = string.Empty;
        if (editor.levelContainer != null)
        {
            foreach (Transform child in editor.levelContainer)
            {
                if (child == null) continue;
                var snake = child.GetComponent<EditorSnakeVisual>();
                if (snake != null)
                {
                    if (snake.LogicNodes == null || snake.LogicNodes.Count < 2)
                    {
                        Vector2Int pos = (snake.LogicNodes != null && snake.LogicNodes.Count > 0)
                            ? snake.LogicNodes[0]
                            : new Vector2Int(Mathf.RoundToInt(child.position.x), Mathf.RoundToInt(child.position.y));
                        message = $"Mũi tên tại vị trí ({pos.x}, {pos.y}) có độ dài không hợp lệ (phải có ít nhất 2 ô).";
                        return false;
                    }
                }
            }
        }
        return true;
    }
}
