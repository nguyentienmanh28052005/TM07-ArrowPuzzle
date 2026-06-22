using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(LevelEditorWorkspace))]
public sealed class EditorRootController : MonoBehaviour
{
    private const float ValidationPollInterval = 0.2f;

    private readonly LevelCatalogService levelCatalog = new LevelCatalogService();
    private readonly LevelSessionService levelSession = new LevelSessionService();
    private readonly LevelValidationService levelValidation = new LevelValidationService();
    private readonly EditorHistoryService history = new EditorHistoryService();
    private readonly EditorDialogService dialogs = new EditorDialogService();
    private readonly EditorSessionState state = new EditorSessionState();

    [Header("Scene References")]
    [SerializeField] private LevelEditorWorkspace levelEditor;
    [SerializeField] private TopBarView topBarView;
    [SerializeField] private LevelListPanelView levelListPanel;

    private string savedDigest = string.Empty;
    private string lastObservedDigest = string.Empty;
    private float nextValidationTime;
    private bool subscribed;
    private bool initialized;

    public void Bind(LevelEditorWorkspace editor, TopBarView topBar)
    {
        if (editor == null || topBar == null) return;

        if (subscribed && topBarView != null)
        {
            Unsubscribe(topBarView);
        }

        levelEditor = editor;
        levelEditor.HistoryService = history;
        topBarView = topBar;
        topBarView.Initialize();
        Subscribe(topBarView);

        if (levelListPanel != null)
        {
            levelListPanel.OnLevelSelected -= LoadLevel;
            levelListPanel.OnLevelSelected += LoadLevel;
            levelListPanel.InitializeList();
        }

        InitializeStateFromEditor();
        initialized = true;
    }

    private void Awake()
    {
        TryInitializeFromSerializedReferences();
    }

    private void OnEnable()
    {
        TryInitializeFromSerializedReferences();
    }

    private void OnDisable()
    {
        if (subscribed && topBarView != null)
        {
            Unsubscribe(topBarView);
        }
        if (levelListPanel != null)
        {
            levelListPanel.OnLevelSelected -= LoadLevel;
        }
    }

    private void Update()
    {
        if (!initialized || levelEditor == null || topBarView == null) return;
        if (Time.unscaledTime < nextValidationTime) return;

        nextValidationTime = Time.unscaledTime + ValidationPollInterval;
        RefreshStateFromEditor();
    }

    public void NewLevel(string key)
    {
        if (!EnsureEditorReady()) return;

        string trimmedKey = levelCatalog.NormalizeKey(key);
        if (!levelCatalog.IsValidKey(trimmedKey, out string validationError))
        {
            SetError(validationError);
            dialogs.ShowInfo("Invalid Level Key", validationError);
            return;
        }

        if (levelCatalog.Exists(trimmedKey))
        {
            string message = $"A level asset with key '{trimmedKey}' already exists. Use Load instead of New.";
            SetError(message);
            dialogs.ShowInfo("Level Already Exists", message);
            return;
        }

        if (state.isDirty)
        {
            int choice = dialogs.ShowSavePrompt(state.levelKey);
            if (choice == 0) // Yes (Save)
            {
                if (!SaveCurrentLevel(true))
                {
                    Render();
                    return; // Save failed, abort
                }
            }
            else if (choice == 1) // Cancel
            {
                Render();
                return; // Abort
            }
            // choice == 2: No (Discard), fall through
        }
        else
        {
            if (!dialogs.ConfirmNew(trimmedKey, false))
            {
                Render();
                return;
            }
        }

        LevelDataV2 levelData = levelSession.CreateNewSession(trimmedKey, state.difficulty);
        levelEditor.SetCurrentDataAndLoad(levelData);
        history.ResetWithBaseline(levelEditor.CaptureSnapshot());
        state.levelKey = trimmedKey;
        state.isExistingLevel = false;
        savedDigest = string.Empty;
        lastObservedDigest = levelEditor.BuildEditorStateDigest();
        RefreshStateFromEditor(forceStatusText: "New level session created. Save to create the asset.");
        if (levelListPanel != null)
        {
            levelListPanel.SelectLevel(trimmedKey);
        }
    }

    public void LoadLevel(string key)
    {
        if (!EnsureEditorReady()) return;

        string trimmedKey = levelCatalog.NormalizeKey(key);
        if (!levelCatalog.IsValidKey(trimmedKey, out string validationError))
        {
            SetError(validationError);
            dialogs.ShowInfo("Invalid Level Key", validationError);
            return;
        }

        if (!levelCatalog.TryLoadExisting(trimmedKey, out LevelDataV2 levelData))
        {
            string message = $"Level '{trimmedKey}' was not found in Assets/Resources/Levels.";
            SetError(message);
            dialogs.ShowInfo("Level Not Found", message);
            return;
        }

        if (state.isDirty)
        {
            int choice = dialogs.ShowSavePrompt(state.levelKey);
            if (choice == 0) // Yes (Save)
            {
                if (!SaveCurrentLevel(true))
                {
                    Render();
                    return; // Save failed, abort
                }
            }
            else if (choice == 1) // Cancel
            {
                Render();
                return; // Abort
            }
            // choice == 2: No (Discard), fall through
        }
        else
        {
            if (!dialogs.ConfirmLoad(trimmedKey, false, levelCatalog.GetAssetPath(trimmedKey)))
            {
                Render();
                return;
            }
        }

        levelEditor.SetCurrentDataAndLoad(levelData);
        history.ResetWithBaseline(levelEditor.CaptureSnapshot());
        state.levelKey = trimmedKey;
        state.isExistingLevel = true;
        savedDigest = levelEditor.BuildEditorStateDigest();
        lastObservedDigest = savedDigest;
        RefreshStateFromEditor(forceStatusText: $"Loaded '{trimmedKey}'.");
        if (levelListPanel != null)
        {
            levelListPanel.SelectLevel(trimmedKey);
        }
    }

    public void SaveCurrentLevel()
    {
        SaveCurrentLevel(false);
    }

    public bool SaveCurrentLevel(bool force)
    {
        if (!EnsureEditorReady()) return false;

        string trimmedKey = levelCatalog.NormalizeKey(topBarView.LevelKey);
        if (!levelCatalog.IsValidKey(trimmedKey, out string validationError))
        {
            SetError(validationError);
            dialogs.ShowInfo("Invalid Level Key", validationError);
            return false;
        }

        bool targetExists = levelCatalog.Exists(trimmedKey);
        LevelDataV2 currentData = levelEditor.GetCurrentLevelData();
        if (currentData == null)
        {
            string message = "There is no active level session to save.";
            SetError(message);
            dialogs.ShowInfo("Nothing To Save", message);
            return false;
        }

        if (state.isExistingLevel && currentData.name != trimmedKey)
        {
            string message = $"Save-as is not supported in TopBar V1. Current session key is '{currentData.name}'. Restore that key or create a new level session.";
            SetError(message);
            dialogs.ShowInfo("Save Key Mismatch", message);
            return false;
        }

        if (!force && !dialogs.ConfirmSave(trimmedKey, targetExists, levelCatalog.GetAssetPath(trimmedKey)))
        {
            Render();
            return false;
        }

        levelEditor.SetCurrentLevelKey(trimmedKey);
        if (!levelSession.Save(levelEditor, trimmedKey, targetExists, levelCatalog, out string saveError))
        {
            SetError(saveError);
            dialogs.ShowInfo("Save Failed", saveError);
            return false;
        }

        if (levelListPanel != null)
        {
            levelListPanel.RefreshList();
            levelListPanel.SelectLevel(trimmedKey);
        }

        state.levelKey = trimmedKey;
        state.isExistingLevel = true;
        savedDigest = levelEditor.BuildEditorStateDigest();
        lastObservedDigest = savedDigest;
        RefreshStateFromEditor(forceStatusText: $"Saved '{trimmedKey}'.");
        return true;
    }

    public void ClearData()
    {
        if (!EnsureEditorReady()) return;

        LevelDataV2 currentData = levelEditor.GetCurrentLevelData();
        if (currentData == null)
        {
            string message = "There is no active level session to clear.";
            SetError(message);
            dialogs.ShowInfo("Nothing To Clear", message);
            return;
        }

        string key = string.IsNullOrWhiteSpace(topBarView.LevelKey) ? currentData.name : topBarView.LevelKey;
        if (!dialogs.ConfirmClearData(key, state.isDirty))
        {
            Render();
            return;
        }

        history.RecordState(levelEditor.CaptureSnapshot());
        levelEditor.ClearCurrentLevelData();
        state.levelKey = key;
        RefreshStateFromEditor(forceStatusText: "Level data cleared.");
    }

    public void DeleteLevel()
    {
        if (!EnsureEditorReady()) return;

        string trimmedKey = levelCatalog.NormalizeKey(topBarView.LevelKey);
        if (!levelCatalog.IsValidKey(trimmedKey, out string validationError))
        {
            SetError(validationError);
            dialogs.ShowInfo("Invalid Level Key", validationError);
            return;
        }

        if (!levelCatalog.Exists(trimmedKey))
        {
            string message = $"Level '{trimmedKey}' was not found in Assets/Resources/Levels.";
            SetError(message);
            dialogs.ShowInfo("Level Not Found", message);
            return;
        }

        if (!dialogs.ConfirmDelete(trimmedKey, levelCatalog.GetAssetPath(trimmedKey)))
        {
            Render();
            return;
        }

        if (!levelCatalog.Delete(trimmedKey, out string deleteError))
        {
            SetError(deleteError);
            dialogs.ShowInfo("Delete Failed", deleteError);
            return;
        }

        if (levelListPanel != null)
        {
            levelListPanel.RefreshList();
        }

        LevelDifficulty difficulty = levelEditor.GetCurrentLevelData() != null
            ? levelEditor.GetCurrentLevelData().levelDifficulty
            : state.difficulty;
        LevelDataV2 newSession = levelSession.CreateNewSession(trimmedKey, difficulty);
        levelEditor.SetCurrentDataAndLoad(newSession);
        history.ResetWithBaseline(levelEditor.CaptureSnapshot());
        state.levelKey = trimmedKey;
        state.isExistingLevel = false;
        savedDigest = string.Empty;
        lastObservedDigest = levelEditor.BuildEditorStateDigest();
        RefreshStateFromEditor(forceStatusText: $"Deleted '{trimmedKey}'.");
    }

    public void SetDifficulty(LevelDifficulty difficulty)
    {
        if (levelEditor != null)
        {
            history.RecordState(levelEditor.CaptureSnapshot());
            state.difficulty = difficulty;
            levelEditor.SetCurrentDifficulty(difficulty);
        }
        RefreshStateFromEditor();
    }

    public void Undo()
    {
        if (!EnsureEditorReady()) return;
        history.Undo(levelEditor);
        RefreshStateFromEditor(forceStatusText: "Undo applied.");
    }

    public void Redo()
    {
        if (!EnsureEditorReady()) return;
        history.Redo(levelEditor);
        RefreshStateFromEditor(forceStatusText: "Redo applied.");
    }

    public void Playtest()
    {
        if (!EnsureEditorReady()) return;

        if (!levelEditor.HasEditableContent())
        {
            SetError("Cannot playtest an empty level.");
            return;
        }

        if (!levelValidation.ValidateCurrent(levelEditor, out string message))
        {
            state.statusType = EditorStatusType.Invalid;
            state.statusText = message;
            Render();
            return;
        }

        levelEditor.UI_Playtest();
        state.statusType = EditorStatusType.Valid;
        state.statusText = message;
        Render();
    }

    private void InitializeStateFromEditor()
    {
        LevelDataV2 currentData = levelEditor != null ? levelEditor.GetCurrentLevelData() : null;
        state.levelKey = currentData != null ? currentData.name : string.Empty;
        state.difficulty = currentData != null ? currentData.levelDifficulty : LevelDifficulty.Easy;
        state.isExistingLevel = currentData != null && levelCatalog.Exists(currentData.name);
        savedDigest = levelEditor != null ? levelEditor.BuildEditorStateDigest() : string.Empty;
        lastObservedDigest = savedDigest;
        RefreshStateFromEditor(currentData != null ? $"Loaded '{state.levelKey}'." : "Ready");
        if (levelListPanel != null && currentData != null)
        {
            levelListPanel.SelectLevel(currentData.name);
        }
    }

    private void RefreshStateFromEditor(string forceStatusText = null)
    {
        if (levelEditor == null) return;

        string currentDigest = levelEditor.BuildEditorStateDigest();
        lastObservedDigest = currentDigest;

        LevelDataV2 currentData = levelEditor.GetCurrentLevelData();
        if (currentData != null)
        {
            state.difficulty = currentData.levelDifficulty;
        }

        state.isDirty = !state.isExistingLevel || currentDigest != savedDigest;
        state.canUndo = history.CanUndo(levelEditor);
        state.canRedo = history.CanRedo(levelEditor);
        state.canSave = currentData != null && !string.IsNullOrWhiteSpace(topBarView.LevelKey);

        if (!string.IsNullOrEmpty(forceStatusText))
        {
            state.statusText = forceStatusText;
            state.statusType = state.isDirty ? EditorStatusType.Dirty : EditorStatusType.Idle;
        }
        else if (!levelEditor.HasEditableContent())
        {
            state.statusType = state.isDirty ? EditorStatusType.Dirty : EditorStatusType.Idle;
            state.statusText = state.isExistingLevel ? "No content in current level." : "New level session is empty.";
        }
        else
        {
            state.statusType = EditorStatusType.Checking;
            state.statusText = "Checking...";
            Render();

            if (levelValidation.ValidateCurrent(levelEditor, out string validationMessage))
            {
                state.statusType = EditorStatusType.Valid;
                state.statusText = validationMessage;
            }
            else
            {
                state.statusType = EditorStatusType.Invalid;
                state.statusText = validationMessage;
            }
        }

        state.canPlaytest = levelEditor.HasEditableContent() && state.statusType == EditorStatusType.Valid;
        Render();
    }

    private void SetError(string message)
    {
        state.statusType = EditorStatusType.Error;
        state.statusText = message;
        Render();
    }

    private void Render()
    {
        if (topBarView != null)
        {
            topBarView.Render(state);
        }
    }

    private bool EnsureEditorReady()
    {
        return levelEditor != null && topBarView != null;
    }

    private void TryInitializeFromSerializedReferences()
    {
        if (initialized) return;
        if (levelEditor == null || topBarView == null) return;

        Bind(levelEditor, topBarView);
    }

    private void Subscribe(TopBarView view)
    {
        if (view == null || subscribed) return;

        view.OnNewRequested += HandleNewRequested;
        view.OnLoadRequested += HandleLoadRequested;
        view.OnSaveRequested += HandleSaveRequested;
        view.OnClearDataRequested += HandleClearDataRequested;
        view.OnDeleteRequested += HandleDeleteRequested;
        view.OnPlaytestRequested += HandlePlaytestRequested;
        view.OnUndoRequested += HandleUndoRequested;
        view.OnRedoRequested += HandleRedoRequested;
        view.OnDifficultyChanged += HandleDifficultyChanged;
        view.OnLevelKeySubmitted += HandleLevelKeySubmitted;
        subscribed = true;
    }

    private void Unsubscribe(TopBarView view)
    {
        if (view == null || !subscribed) return;

        view.OnNewRequested -= HandleNewRequested;
        view.OnLoadRequested -= HandleLoadRequested;
        view.OnSaveRequested -= HandleSaveRequested;
        view.OnClearDataRequested -= HandleClearDataRequested;
        view.OnDeleteRequested -= HandleDeleteRequested;
        view.OnPlaytestRequested -= HandlePlaytestRequested;
        view.OnUndoRequested -= HandleUndoRequested;
        view.OnRedoRequested -= HandleRedoRequested;
        view.OnDifficultyChanged -= HandleDifficultyChanged;
        view.OnLevelKeySubmitted -= HandleLevelKeySubmitted;
        subscribed = false;
    }

    private void HandleNewRequested() => NewLevel(topBarView.LevelKey);
    private void HandleLoadRequested() => LoadLevel(topBarView.LevelKey);
    private void HandleSaveRequested() => SaveCurrentLevel();
    private void HandleClearDataRequested() => ClearData();
    private void HandleDeleteRequested() => DeleteLevel();
    private void HandlePlaytestRequested() => Playtest();
    private void HandleUndoRequested() => Undo();
    private void HandleRedoRequested() => Redo();
    private void HandleDifficultyChanged(LevelDifficulty difficulty) => SetDifficulty(difficulty);

    private void HandleLevelKeySubmitted(string key)
    {
        state.levelKey = levelCatalog.NormalizeKey(key);
        Render();
    }
}
