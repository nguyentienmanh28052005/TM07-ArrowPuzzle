using System.Collections.Generic;
using UnityEngine;

public sealed class EditorSnapshot
{
    public LevelDataV2 levelDataClone;
    public string levelKey = string.Empty;
    public LevelDifficulty difficulty;

    public EditorToolType currentTool;
    public ArrowDir currentDir;
    public Color currentColor;

    public List<Vector2Int> currentDraftNodes = new List<Vector2Int>();
    public List<PortalData> currentDraftPortals = new List<PortalData>();
    public List<ElectricWallSaveData> currentDraftElectricWalls = new List<ElectricWallSaveData>();

    public bool isPlacingPortalExit;
    public Vector2Int draftPortalEntrance;
    public ArrowDir draftPortalEntranceDir;
    public Color draftPortalColor;

    public bool isPlacingElectricWallEnd;
    public Vector2Int draftElectricWallStart;
    public Color draftElectricWallColor;
}

public sealed class EditorHistoryService
{
    private readonly LinkedList<EditorSnapshot> undoStack = new LinkedList<EditorSnapshot>();
    private readonly LinkedList<EditorSnapshot> redoStack = new LinkedList<EditorSnapshot>();
    private const int MaxSnapshots = 50;

    private EditorSnapshot baselineSnapshot;

    public void Initialize(LevelEditorWorkspace editor)
    {
        ClearHistory();
    }

    public void ResetWithBaseline(EditorSnapshot baseline)
    {
        ClearHistory();
        baselineSnapshot = baseline;
    }

    public void RecordState(EditorSnapshot stateBeforeMutation)
    {
        if (stateBeforeMutation == null) return;

        undoStack.AddLast(stateBeforeMutation);
        redoStack.Clear();

        if (undoStack.Count > MaxSnapshots)
        {
            undoStack.RemoveFirst();
        }
    }

    public bool CanUndo(LevelEditorWorkspace editor)
    {
        return undoStack.Count > 0;
    }

    public bool CanRedo(LevelEditorWorkspace editor)
    {
        return redoStack.Count > 0;
    }

    public void Undo(LevelEditorWorkspace editor)
    {
        if (editor == null || !CanUndo(editor)) return;

        EditorSnapshot currentState = editor.CaptureSnapshot();
        EditorSnapshot previousState = undoStack.Last.Value;
        undoStack.RemoveLast();

        redoStack.AddLast(currentState);
        editor.RestoreSnapshot(previousState);
    }

    public void Redo(LevelEditorWorkspace editor)
    {
        if (editor == null || !CanRedo(editor)) return;

        EditorSnapshot currentState = editor.CaptureSnapshot();
        EditorSnapshot nextState = redoStack.Last.Value;
        redoStack.RemoveLast();

        undoStack.AddLast(currentState);
        editor.RestoreSnapshot(nextState);
    }

    public void ClearHistory()
    {
        undoStack.Clear();
        redoStack.Clear();
        baselineSnapshot = null;
    }
}
