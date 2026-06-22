#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class ColorPanelSetupUtility
{
    [MenuItem("Tools/Setup Color Panel UI")]
    public static void SetupColorPanel()
    {
        // 1. Find RightColorPanel
        GameObject rightColorPanel = GameObject.Find("RightColorPanel");
        if (rightColorPanel == null)
        {
            Debug.LogError("RightColorPanel not found in scene!");
            return;
        }

        ColorPalettePanelView view = rightColorPanel.GetComponent<ColorPalettePanelView>();
        if (view == null)
        {
            view = rightColorPanel.AddComponent<ColorPalettePanelView>();
        }

        // Find ColorListContent
        Transform colorListContent = rightColorPanel.transform.Find("Content/ColorListContent");
        if (colorListContent == null)
        {
            Debug.LogError("Content/ColorListContent not found under RightColorPanel!");
            return;
        }

        // 2. Clean existing Controls if any
        Transform existingControls = colorListContent.Find("ColorControls");
        if (existingControls != null)
        {
            Object.DestroyImmediate(existingControls.gameObject);
        }

        // 3. Create ColorControls Panel
        GameObject controlsObj = new GameObject("ColorControls", typeof(RectTransform));
        controlsObj.transform.SetParent(colorListContent, false);
        controlsObj.transform.SetSiblingIndex(1); // Below label, above scroll view
        controlsObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 40);

        VerticalLayoutGroup vGroup = controlsObj.AddComponent<VerticalLayoutGroup>();
        vGroup.spacing = 0;
        vGroup.childControlWidth = true;
        vGroup.childControlHeight = false;
        vGroup.childForceExpandWidth = true;
        vGroup.childForceExpandHeight = false;
        vGroup.padding = new RectOffset(8, 8, 4, 4);

        // Find template objects
        GameObject drdTemplate = GameObject.Find("Canvas/TopBar/CenterLevelInfor/Drd_Difficulty");
        GameObject ipfTemplate = GameObject.Find("Canvas/TopBar/CenterLevelInfor/Ipf_LevelName");
        GameObject btnTemplate = GameObject.Find("Canvas/TopBar/LeftToolsGroup/Btn_New");

        if (drdTemplate == null || ipfTemplate == null || btnTemplate == null)
        {
            Debug.LogError("Template UI components not found in TopBar! Please ensure TopBar has Drd_Difficulty, Ipf_LevelName, and Btn_New.");
            return;
        }

        // --- ROW 1 (Hex Input, Preview, Add, Remove) ---
        GameObject row1Obj = new GameObject("Row1", typeof(RectTransform));
        row1Obj.transform.SetParent(controlsObj.transform, false);
        row1Obj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 30);
        HorizontalLayoutGroup hGroup1 = row1Obj.AddComponent<HorizontalLayoutGroup>();
        hGroup1.spacing = 6;
        hGroup1.childControlWidth = false;
        hGroup1.childControlHeight = false;
        hGroup1.childForceExpandWidth = false;
        hGroup1.childForceExpandHeight = false;

        // 1. Hex Input Field
        GameObject hexIpfObj = Object.Instantiate(ipfTemplate, row1Obj.transform);
        hexIpfObj.name = "Ipf_HexColor";
        hexIpfObj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30);
        
        // Clean up any extraneous label/text inside the cloned input field
        foreach (TMP_Text t in hexIpfObj.GetComponentsInChildren<TMP_Text>(true))
        {
            if (t.text.Contains("Level Name") || t.gameObject.name.Contains("Label"))
            {
                Object.DestroyImmediate(t.gameObject);
            }
        }

        SetPlaceholderText(hexIpfObj, "#FFFFFF");
        TMP_InputField hexIpfComp = hexIpfObj.GetComponent<TMP_InputField>();
        if (hexIpfComp != null)
        {
            hexIpfComp.text = "#FFFFFF";
            hexIpfComp.characterLimit = 7;
        }

        // 2. Color Preview Image
        GameObject previewObj = new GameObject("Image_PreviewColor", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        previewObj.transform.SetParent(row1Obj.transform, false);
        
        RectTransform previewRect = previewObj.GetComponent<RectTransform>();
        previewRect.sizeDelta = new Vector2(30, 30);
        
        Outline outline = previewObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        outline.effectDistance = new Vector2(1, -1);

        // 3. Add Button
        GameObject btnAddObj = Object.Instantiate(btnTemplate, row1Obj.transform);
        btnAddObj.name = "Btn_AddColor";
        SetButtonText(btnAddObj, "+");
        btnAddObj.GetComponent<RectTransform>().sizeDelta = new Vector2(35, 30);

        // 4. Remove Button
        GameObject btnRemoveObj = Object.Instantiate(btnTemplate, row1Obj.transform);
        btnRemoveObj.name = "Btn_RemoveColor";
        SetButtonText(btnRemoveObj, "Remove");
        btnRemoveObj.GetComponent<RectTransform>().sizeDelta = new Vector2(75, 30);

        // 4. Assign references on ColorPalettePanelView
        SerializedObject so = new SerializedObject(view);
        so.Update();

        so.FindProperty("btnRemoveColor").objectReferenceValue = btnRemoveObj.GetComponent<Button>();
        so.FindProperty("ipfHexColor").objectReferenceValue = hexIpfComp;
        so.FindProperty("btnAddColor").objectReferenceValue = btnAddObj.GetComponent<Button>();
        so.FindProperty("localPreviewImage").objectReferenceValue = previewObj.GetComponent<Image>();

        // Unlink old controls safely
        var propEyeDropper = so.FindProperty("toggleEyeDropper");
        if (propEyeDropper != null) propEyeDropper.objectReferenceValue = null;
        var propDropdown = so.FindProperty("paletteDropdown");
        if (propDropdown != null) propDropdown.objectReferenceValue = null;
        var propSavePalette = so.FindProperty("btnSavePalette");
        if (propSavePalette != null) propSavePalette.objectReferenceValue = null;
        var propNewPalette = so.FindProperty("btnNewPalette");
        if (propNewPalette != null) propNewPalette.objectReferenceValue = null;
        var propPresetLibrary = so.FindProperty("presetLibraryGrid");
        if (propPresetLibrary != null) propPresetLibrary.objectReferenceValue = null;

        so.ApplyModifiedProperties();

        // 5. Adjust Scroll View height to fit the new controls
        Transform scrollView = colorListContent.Find("Scroll View");
        if (scrollView != null)
        {
            RectTransform svRect = scrollView.GetComponent<RectTransform>();
            // Adjust vertical size to match the smaller 40px controls container
            svRect.sizeDelta = new Vector2(svRect.sizeDelta.x, 190);
        }

        // Save Scene
        EditorUtility.SetDirty(rightColorPanel);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(rightColorPanel.scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(rightColorPanel.scene);

        Debug.Log("Color Panel UI controls successfully setup and linked!");
    }

    private static void SetButtonText(GameObject btnObj, string text)
    {
        TMP_Text tmp = btnObj.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = text;
        }
    }

    private static void SetPlaceholderText(GameObject ipfObj, string text)
    {
        Transform placeholder = ipfObj.transform.Find("TextArea/Placeholder");
        if (placeholder != null)
        {
            TMP_Text tmp = placeholder.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = text;
            }
        }
    }
}
#endif
