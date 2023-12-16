using System;
using T11.Elements;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace T11.Windows
{
    using Elements;
    using Enumerations;

    public class T11GraphView : GraphView
    {
        public T11GraphView()
        {
            AddGridBacground();
            AddStyles();
            AddManipulators();
        }

        private void AddGridBacground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddStyles()
        {
            StyleSheet graphViewStyleSheet = (StyleSheet)EditorGUIUtility.Load("DialogueSystem/T11GraphViewStyles.uss");
            StyleSheet nodeStyleSheet = (StyleSheet)EditorGUIUtility.Load("DialogueSystem/T11NodeStyles.uss");
            styleSheets.Add(graphViewStyleSheet);
            styleSheets.Add(nodeStyleSheet);
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu("Add Single Choice Dialogue Node", T11DialogueType.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Add Multiple Choice Dialogue Node", T11DialogueType.MultipleChoice));
        }

        private IManipulator CreateNodeContextualMenu(string actionTitle, T11DialogueType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                MenuEvent => MenuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode(dialogueType, actionEvent.eventInfo.localMousePosition)))
            );
            return contextualMenuManipulator;
        }

        private T11Node CreateNode(T11DialogueType dialogueType, Vector2 position)
        {
            Type nodeType = Type.GetType($"T11.Elements.T11{dialogueType}Node");
            T11Node node = (T11Node) Activator.CreateInstance( nodeType );
            node.Initialize(position);
            node.Draw();
            return node;
        }
    }
}
