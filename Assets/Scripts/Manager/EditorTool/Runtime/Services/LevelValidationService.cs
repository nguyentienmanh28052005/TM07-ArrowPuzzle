public sealed class LevelValidationService
{
    public bool ValidateCurrent(LevelEditor editor, out string message)
    {
        if (editor == null)
        {
            message = "Level editor is unavailable.";
            return false;
        }

        if (!editor.HasEditableContent())
        {
            message = "No content to validate.";
            return false;
        }

        return editor.TryValidateCurrentEditorLevel(out message);
    }
}
