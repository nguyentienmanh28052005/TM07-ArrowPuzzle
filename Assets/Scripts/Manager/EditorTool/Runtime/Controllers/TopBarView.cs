using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TopBarView : MonoBehaviour
{
    public event Action OnNewRequested;
    public event Action OnLoadRequested;
    public event Action OnSaveRequested;
    public event Action OnClearDataRequested;
    public event Action OnDeleteRequested;
    public event Action OnPlaytestRequested;
    public event Action OnUndoRequested;
    public event Action OnRedoRequested;
    public event Action<LevelDifficulty> OnDifficultyChanged;
    public event Action<string> OnLevelKeySubmitted;

    [Header("Buttons")]
    [SerializeField] private Button newButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button clearDataButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button playtestButton;
    [SerializeField] private Button undoButton;
    [SerializeField] private Button redoButton;

    [Header("Inputs")]
    [SerializeField] private TMP_InputField levelKeyInput;
    [SerializeField] private TMP_Dropdown difficultyDropdown;

    [Header("Status")]
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private TMP_Text statusValue;
    [SerializeField] private TMP_Text saveStateLabel;
    [SerializeField] private TMP_Text saveStateValue;

    private bool initialized;

    public string LevelKey => levelKeyInput != null ? levelKeyInput.text.Trim() : string.Empty;

    public void Initialize()
    {
        if (initialized) return;

        if (!HasRequiredReferences())
        {
            Debug.LogWarning($"[{nameof(TopBarView)}] Missing required references on '{name}'. Assign them in the Inspector.");
            return;
        }

        ConfigureLevelKeyInput();
        ConfigureDifficultyDropdown();
        WireEvents();

        initialized = true;
    }

    public void Render(EditorSessionState state)
    {
        Initialize();
        if (state == null) return;

        if (levelKeyInput != null && !levelKeyInput.isFocused && levelKeyInput.text != state.levelKey)
        {
            levelKeyInput.SetTextWithoutNotify(state.levelKey ?? string.Empty);
        }

        if (difficultyDropdown != null)
        {
            int index = Mathf.Clamp((int)state.difficulty, 0, difficultyDropdown.options.Count - 1);
            if (difficultyDropdown.value != index)
            {
                difficultyDropdown.SetValueWithoutNotify(index);
            }
            difficultyDropdown.interactable = !state.isBusy;
        }

        if (newButton != null) newButton.interactable = !state.isBusy;
        if (loadButton != null) loadButton.interactable = !state.isBusy;
        if (saveButton != null) saveButton.interactable = state.canSave && !state.isBusy;
        if (clearDataButton != null) clearDataButton.interactable = currentHasSession(state) && !state.isBusy;
        if (deleteButton != null) deleteButton.interactable = state.isExistingLevel && !state.isBusy;
        if (undoButton != null) undoButton.interactable = state.canUndo && !state.isBusy;
        if (redoButton != null) redoButton.interactable = state.canRedo && !state.isBusy;
        if (playtestButton != null) playtestButton.interactable = state.canPlaytest && !state.isBusy;
        if (levelKeyInput != null) levelKeyInput.interactable = !state.isBusy;


        if (statusValue != null)
        {
            statusValue.text = GetStatusLabel(state.statusType);
            statusValue.color = GetStatusColor(state.statusType);
        }


        if (saveStateValue != null)
        {
            saveStateValue.text = GetSaveStateLabel(state);
            saveStateValue.color = GetSaveStateColor(state);
        }
    }

    private void WireEvents()
    {
        BindButton(newButton, () => OnNewRequested?.Invoke());
        BindButton(loadButton, () => OnLoadRequested?.Invoke());
        BindButton(saveButton, () => OnSaveRequested?.Invoke());
        BindButton(clearDataButton, () => OnClearDataRequested?.Invoke());
        BindButton(deleteButton, () => OnDeleteRequested?.Invoke());
        BindButton(playtestButton, () => OnPlaytestRequested?.Invoke());
        BindButton(undoButton, () => OnUndoRequested?.Invoke());
        BindButton(redoButton, () => OnRedoRequested?.Invoke());

        if (difficultyDropdown != null)
        {
            difficultyDropdown.onValueChanged.RemoveAllListeners();
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyDropdownChanged);
        }

        if (levelKeyInput != null)
        {
            levelKeyInput.onSubmit.RemoveAllListeners();
            levelKeyInput.onSubmit.AddListener(HandleLevelKeySubmitted);
            levelKeyInput.onEndEdit.RemoveAllListeners();
            levelKeyInput.onEndEdit.AddListener(HandleLevelKeySubmitted);
        }
    }

    private void HandleLevelKeySubmitted(string value)
    {
        OnLevelKeySubmitted?.Invoke((value ?? string.Empty).Trim());
    }

    private void OnDifficultyDropdownChanged(int index)
    {
        OnDifficultyChanged?.Invoke((LevelDifficulty)Mathf.Clamp(index, 0, Enum.GetValues(typeof(LevelDifficulty)).Length - 1));
    }

    private void ConfigureDifficultyDropdown()
    {
        if (difficultyDropdown == null) return;

        difficultyDropdown.ClearOptions();
        var options = new List<string>();
        foreach (LevelDifficulty value in Enum.GetValues(typeof(LevelDifficulty)))
        {
            options.Add(value.ToString());
        }
        difficultyDropdown.AddOptions(options);
    }

    private void ConfigureLevelKeyInput()
    {
        if (levelKeyInput == null) return;

        TMP_Text placeholderText = levelKeyInput.placeholder != null
            ? levelKeyInput.placeholder.GetComponent<TMP_Text>()
            : null;
        if (placeholderText != null)
        {
            placeholderText.text = "Level Key";
        }
    }

    private static string GetStatusLabel(EditorStatusType statusType)
    {
        switch (statusType)
        {
            case EditorStatusType.Valid:    return "Valid";
            case EditorStatusType.Invalid:  return "Warning";
            case EditorStatusType.Error:    return "Error";
            case EditorStatusType.Checking: return "Checking";
            default:                        return "Idle";
        }
    }

    private static string GetSaveStateLabel(EditorSessionState state)
    {
        if (state == null) return "—";
        if (!state.isExistingLevel && !state.isDirty) return "New";
        if (state.isDirty)  return "Unsaved";
        return "Saved";
    }

    private static Color GetSaveStateColor(EditorSessionState state)
    {
        if (state == null) return new Color32(90, 90, 90, 255);
        if (!state.isExistingLevel && !state.isDirty) return new Color32(90, 90, 90, 255);
        if (state.isDirty) return new Color32(210, 145, 70, 255);
        return new Color32(60, 170, 90, 255);
    }

    private static Color GetStatusColor(EditorStatusType statusType)
    {
        switch (statusType)
        {
            case EditorStatusType.Valid:
                return new Color32(60, 170, 90, 255);
            case EditorStatusType.Invalid:
            case EditorStatusType.Error:
                return new Color32(210, 80, 80, 255);
            case EditorStatusType.Checking:
                return new Color32(70, 130, 210, 255);
            default:
                return new Color32(90, 90, 90, 255);
        }
    }

    private bool HasRequiredReferences()
    {
        return newButton != null
            && loadButton != null
            && saveButton != null
            && clearDataButton != null
            && deleteButton != null
            && playtestButton != null
            && undoButton != null
            && redoButton != null
            && levelKeyInput != null
            && difficultyDropdown != null;
    }

    private static bool currentHasSession(EditorSessionState state)
    {
        return state != null && (!string.IsNullOrWhiteSpace(state.levelKey) || state.isExistingLevel || state.isDirty);
    }

    private static void BindButton(Button button, Action callback)
    {
        if (button == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => callback?.Invoke());
    }
}
