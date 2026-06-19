using UnityEngine;
using UnityEngine.EventSystems;

// Force recompile comment
public sealed class EditorInputManager : MonoBehaviour
{
    [Header("Target References")]
    [SerializeField] private LevelEditor levelEditor;
    [SerializeField] private LevelListPanelView levelListPanel;

    [Header("Hotkeys - Tools & Actions")]
    public KeyCode playtestKey = KeyCode.F5;
    public KeyCode checkDeadlockKey = KeyCode.F6;
    public KeyCode togglePanelKey = KeyCode.Tab;
    public KeyCode finishSnakeKey = KeyCode.Space;
    public KeyCode rotateDirKey = KeyCode.R;
    public KeyCode undoKey = KeyCode.Z;
    public KeyCode redoKey = KeyCode.Y;

    [Header("Hotkeys - Editor Tools")]
    public KeyCode toolDrawKey = KeyCode.Alpha1;
    public KeyCode toolEraseKey = KeyCode.Alpha2;
    public KeyCode toolPaintKey = KeyCode.Alpha3;
    public KeyCode toolSelectKey = KeyCode.Alpha4;
    public KeyCode toolPortalKey = KeyCode.Alpha5;
    public KeyCode toolKeycardKey = KeyCode.Alpha6;
    public KeyCode toolGateKey = KeyCode.Alpha7;
    public KeyCode toolDeflectorKey = KeyCode.Alpha8;
    public KeyCode toolCountdownKey = KeyCode.Alpha9;
    public KeyCode toolStopBlockKey = KeyCode.Alpha0;
    public KeyCode toolArrowShadowKey = KeyCode.B;
    public KeyCode toolTurnStateKey = KeyCode.T;
    public KeyCode toolBlackHoleKey = KeyCode.H;
    public KeyCode toolRevealWaveKey = KeyCode.V;

    [Header("Hotkeys - Directions")]
    public KeyCode dirUpKey1 = KeyCode.W;
    public KeyCode dirUpKey2 = KeyCode.UpArrow;
    public KeyCode dirDownKey1 = KeyCode.S;
    public KeyCode dirDownKey2 = KeyCode.DownArrow;
    public KeyCode dirLeftKey1 = KeyCode.A;
    public KeyCode dirLeftKey2 = KeyCode.LeftArrow;
    public KeyCode dirRightKey1 = KeyCode.D;
    public KeyCode dirRightKey2 = KeyCode.RightArrow;

    private void Awake()
    {
        if (levelEditor == null)
        {
            levelEditor = FindObjectOfType<LevelEditor>();
        }
        if (levelListPanel == null)
        {
            levelListPanel = FindObjectOfType<LevelListPanelView>();
        }
    }

    private void Update()
    {
        // 1. Chặn phím tắt khi người dùng đang nhập liệu trong bất kỳ InputField nào
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            var inputField = EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>();
            if (inputField != null && inputField.isFocused)
            {
                return; // Đang gõ chữ -> Không kích hoạt phím tắt
            }
        }

        // 2. Phím tắt mở/đóng Panel danh sách (Không cần LevelEditor)
        if (levelListPanel != null && Input.GetKeyDown(togglePanelKey))
        {
            levelListPanel.TogglePanel();
        }

        // 3. Phím tắt trong Level Editor
        if (levelEditor != null)
        {
            HandleEditorShortcuts();
        }
    }

    private void HandleEditorShortcuts()
    {
        // Playtest & Check Deadlock
        if (Input.GetKeyDown(playtestKey)) levelEditor.UI_Playtest();
        if (Input.GetKeyDown(checkDeadlockKey)) levelEditor.UI_CheckDeadlock();

        // Tool Selection
        if (Input.GetKeyDown(toolDrawKey)) levelEditor.UI_SetTool(0);
        if (Input.GetKeyDown(toolEraseKey)) levelEditor.UI_SetTool(1);
        if (Input.GetKeyDown(toolPaintKey)) levelEditor.UI_SetTool(2);
        if (Input.GetKeyDown(toolSelectKey)) levelEditor.UI_SetTool(3);
        if (Input.GetKeyDown(toolPortalKey)) levelEditor.UI_SetTool(4);
        if (Input.GetKeyDown(toolKeycardKey)) levelEditor.UI_SetTool(5);
        if (Input.GetKeyDown(toolGateKey)) levelEditor.UI_SetTool(6);
        if (Input.GetKeyDown(toolDeflectorKey)) levelEditor.UI_SetTool(7);
        if (Input.GetKeyDown(toolCountdownKey)) levelEditor.UI_SetTool(8);
        if (Input.GetKeyDown(toolStopBlockKey)) levelEditor.UI_SetTool((int)EditorToolType.StopBlock);
        if (Input.GetKeyDown(toolArrowShadowKey)) levelEditor.UI_SetTool((int)EditorToolType.ArrowShadow);
        if (Input.GetKeyDown(toolTurnStateKey)) levelEditor.UI_SetTool((int)EditorToolType.TurnStateBlock);
        if (Input.GetKeyDown(toolBlackHoleKey)) levelEditor.UI_SetTool((int)EditorToolType.BlackHole);
        if (Input.GetKeyDown(toolRevealWaveKey)) levelEditor.UI_SetTool((int)EditorToolType.RevealWaveButton);

        // Direction Selection
        if (Input.GetKeyDown(dirUpKey1) || Input.GetKeyDown(dirUpKey2)) levelEditor.UI_SetDirection(0);
        if (Input.GetKeyDown(dirDownKey1) || Input.GetKeyDown(dirDownKey2)) levelEditor.UI_SetDirection(1);
        if (Input.GetKeyDown(dirLeftKey1) || Input.GetKeyDown(dirLeftKey2)) levelEditor.UI_SetDirection(2);
        if (Input.GetKeyDown(dirRightKey1) || Input.GetKeyDown(dirRightKey2)) levelEditor.UI_SetDirection(3);

        // General operations
        if (Input.GetKeyDown(finishSnakeKey)) levelEditor.UI_FinishSnake();
        if (Input.GetKeyDown(rotateDirKey)) levelEditor.RotateDirectionPublic();
        if (Input.GetKeyDown(undoKey)) levelEditor.TriggerUndo();
        if (Input.GetKeyDown(redoKey)) levelEditor.TriggerRedo();
    }
}
