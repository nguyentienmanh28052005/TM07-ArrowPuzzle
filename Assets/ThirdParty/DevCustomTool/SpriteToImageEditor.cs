#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameTool.Editor
{
    public class SpriteToImageEditor : EditorWindow
    {
        [MenuItem("MyTools/Sprite To Image")]
        public static void ShowWindow()
        {
            GetWindow<SpriteToImageEditor>("Sprite To Image Editor");
        }

        private void OnGUI()
        {
            // Hiển thị danh sách các scene trong build settings
            GUILayout.Label("Warning: \n", EditorStyles.boldLabel);
            GUILayout.Label("Don't Close Window when edit UI.", EditorStyles.label);
        }

        private void Awake()
        {
            Init();
        }

        private void OnEnable()
        {
            Init();
        }

        private void Init()
        {
            Selection.selectionChanged -= SelectionChange;
            Selection.selectionChanged += SelectionChange;

            EditorApplication.hierarchyChanged -= SelectionChange;
            EditorApplication.hierarchyChanged += SelectionChange;
        }

        private void OnDisable()
        {
            Remove();
        }

        private void OnDestroy()
        {
            Remove();
        }

        private void Remove()
        {
            Selection.selectionChanged -= SelectionChange;

            EditorApplication.hierarchyChanged -= SelectionChange;
        }

        private void SelectionChange()
        {
            GameObject selectedObject = Selection.activeGameObject;

            if (selectedObject != null && selectedObject.GetComponentInParent<RectTransform>(true))
            {
                SpriteRenderer spriteRenderer = selectedObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Undo.RecordObject(selectedObject, "Convert Sprite to Image");
                    Image imageComponent = selectedObject.GetComponent<Image>();
                    if (imageComponent == null)
                    {
                        imageComponent = selectedObject.AddComponent<Image>();
                    }

                    imageComponent.sprite = spriteRenderer.sprite;
                    DestroyImmediate(spriteRenderer);
                    selectedObject.transform.localScale = Vector3.one;
                    imageComponent.SetNativeSize();

                    Camera sceneCamera = SceneView.lastActiveSceneView.camera;

                    if (sceneCamera != null && Selection.activeTransform != null)
                    {
                        Selection.activeTransform.position = sceneCamera.transform.position;
                        Selection.activeTransform.localPosition = new Vector3(Selection.activeTransform.localPosition.x,
                            Selection.activeTransform.localPosition.y, 0);
                    }
                }
            }
        }
    }
}

#endif