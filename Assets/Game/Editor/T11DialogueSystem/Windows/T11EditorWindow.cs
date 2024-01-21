using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace T11.Windows
{
    using System.IO;
    using Utilities;

    public class T11EditorWindow : EditorWindow
    {
        private T11GraphView graphView;
        private readonly string defaultFileName = "defaultFile";
        private static TextField fileNameTextField;
        private Button saveButton;
        private Button miniMapButton;

        [MenuItem("T11/Dialogue Graph")]
        public static void Open()
        {
            GetWindow<T11EditorWindow>("Dialogue Graph");
        }

        private void CreateGUI()
        {
            AddGraphView();
            AddToolbar();
            AddStyles();
        }

        private void AddGraphView()
        {
            graphView = new T11GraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();
            fileNameTextField = T11ElementUtility.CreateTextField(defaultFileName, "File Name:", callback =>
            {
                fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });
            
            saveButton = T11ElementUtility.CreateButton("Save", () => Save());
            Button loadButton = T11ElementUtility.CreateButton("Load", () => Load());
            Button clearButton = T11ElementUtility.CreateButton("Clear", () => Clear());
            Button resetButton = T11ElementUtility.CreateButton("Reset", () => ResetGraph());
            miniMapButton = T11ElementUtility.CreateButton("MiniMap", () => ToggleMiniMap());

            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(clearButton);
            toolbar.Add(resetButton);
            toolbar.Add(miniMapButton);

            toolbar.AddStyleSheets("DialogueSystem/T11ToolbarStyles.uss");

            rootVisualElement.Add(toolbar);
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DialogueSystem/T11Variables.uss");
        }

        private void Clear()
        {
            graphView.ClearGraph();
        }

        private void ResetGraph()
        {
            Clear();
            UpdateFileName(defaultFileName);
        }

        public static void UpdateFileName(string newFileName)
        {
            fileNameTextField.value = newFileName;  
        }

        private void Save()
        {
            if(string.IsNullOrEmpty(fileNameTextField.value))
            {
                EditorUtility.DisplayDialog(
                    "Invalid file name.",
                    "Please ensure that you give a proper name to name to the file.",
                    "Okay."
                );
                return;
            }

            T11IOUtility.Initialize(graphView, fileNameTextField.value);
            T11IOUtility.Save();
        }

        private void Load()
        {
            string filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", "Assets/Game/Editor/T11DialogueSystem/Graphs", "asset");
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            Clear();
            T11IOUtility.Initialize(graphView, Path.GetFileNameWithoutExtension(filePath));
            T11IOUtility.Load();
        }

        private void ToggleMiniMap()
        {
            graphView.ToggleMiniMap();
            miniMapButton.ToggleInClassList(".t11-toolbar__button__selected");
        }

        public void EnableSaving()
        {
            saveButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            saveButton.SetEnabled(false);
        }
    }
}
