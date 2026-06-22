using UnityEngine;

public enum EditorStatusType
{
    Idle,
    Dirty,
    Checking,
    Valid,
    Invalid,
    Error
}

public sealed class EditorSessionState
{
    public string levelKey = string.Empty;
    public LevelDifficulty difficulty = LevelDifficulty.Easy;
    public EditorStatusType statusType = EditorStatusType.Idle;
    public string statusText = "Ready";
    public bool canSave;
    public bool canUndo;
    public bool canRedo;
    public bool canPlaytest;
    public bool isBusy;
    public bool isExistingLevel;
    public bool isDirty;
}
