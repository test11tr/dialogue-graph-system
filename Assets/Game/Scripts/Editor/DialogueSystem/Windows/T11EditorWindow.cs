using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


namespace T11.Windows
{
    public class T11EditorWindow : EditorWindow
    {
        [MenuItem("T11/Dialogue Graph")]
        public static void Open()
        {
            GetWindow<T11EditorWindow>("Dialogue Graph");
        }

        private void CreateGUI()
        {
            AddGraphView();
            AddStyles();
        }

        private void AddGraphView()
        {
            T11GraphView graphView = new T11GraphView();
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void AddStyles()
        {
            StyleSheet styleSheet = (StyleSheet)EditorGUIUtility.Load("DialogueSystem/T11Variables.uss");
            rootVisualElement.styleSheets.Add(styleSheet);
        }
    }
}
