using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace T11.Windows
{
    using Elements;
    using Enumerations;
    using Utilities;
    using Data.Error;
    using System.Collections.Generic;

    public class T11GraphView : GraphView
    {
        private T11EditorWindow editorWindow;
        private t11SearchWindow searchWindow;

        private SerializableDictionary<string, T11NodeErrorData> ungroupedNodes;

        public T11GraphView(T11EditorWindow t11EditorWindow)
        {
            editorWindow = t11EditorWindow;
            ungroupedNodes = new SerializableDictionary<string, T11NodeErrorData>();
            AddManipulators();
            AddSearchWindow();
            OnElementsDeleted();
            AddGridBacground();
            AddStyles();
        }

        private void AddGridBacground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddStyles()
        {
            this.AddStyleSheets(
                "DialogueSystem/T11GraphViewStyles.uss",
                "DialogueSystem/T11NodeStyles.uss"
            );
        }

        private void AddSearchWindow()
        {
            if( searchWindow == null )
            {
                searchWindow = ScriptableObject.CreateInstance<t11SearchWindow>();
                searchWindow.Initialize(this);
            }

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if(startPort == port)
                {
                    return;
                }

                if(startPort.node == port.node)
                {
                    return;
                }

                if(startPort.direction == port.direction)
                {
                    return;
                }

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu("Add Single Choice Dialogue Node", T11DialogueType.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Add Multiple Choice Dialogue Node", T11DialogueType.MultipleChoice));

            this.AddManipulator(CreateGroupContextualMenu());
        }

        private IManipulator CreateNodeContextualMenu(string actionTitle, T11DialogueType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                MenuEvent => MenuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode(dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );
            return contextualMenuManipulator;
        }

        public T11Node CreateNode(T11DialogueType dialogueType, Vector2 position)
        {
            Type nodeType = Type.GetType($"T11.Elements.T11{dialogueType}Node");
            T11Node node = (T11Node) Activator.CreateInstance( nodeType );
            node.Initialize(this, position);
            node.Draw();
            AddUngroupedNode(node);
            return node;
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                MenuEvent => MenuEvent.menu.AppendAction("Add Group", actionEvent => AddElement(CreateGroup("Dialogue Group", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );
            return contextualMenuManipulator;
        }

        public Group CreateGroup(string title, Vector2 localMousePosition)
        {
            Group group = new Group()
            {
                title = title,
            };

            group.SetPosition(new Rect(localMousePosition, Vector2.zero));
            return group;
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                worldMousePosition -= editorWindow.position.position;
            }

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);
            return localMousePosition;
        }

        public void AddUngroupedNode(T11Node node)
        {
            string nodeName = node.DialogueName;

            if(!ungroupedNodes.ContainsKey(nodeName))
            {
                T11NodeErrorData nodeErrorData = new T11NodeErrorData();
                nodeErrorData.Nodes.Add(node);
                ungroupedNodes.Add(nodeName, nodeErrorData);
                return;
            }

            List<T11Node> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;

            ungroupedNodesList.Add(node);

            Color errorColor = ungroupedNodes[nodeName].ErrorData.Color;

            node.SetErrorStyle(errorColor);

            if(ungroupedNodesList.Count == 2)
            {
                ungroupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveUngroupedNode(T11Node node)
        {
            string nodeName = node.DialogueName;

            List<T11Node> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;

            ungroupedNodesList.Remove(node);

            node.ResetStyle();

            if(ungroupedNodesList.Count == 1)
            {
                ungroupedNodesList[0].ResetStyle();
                return;
            }

            if(ungroupedNodesList.Count == 0)
            {
                ungroupedNodes.Remove(nodeName);
            }
        }

        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                List<T11Node> nodesToDelete = new List<T11Node>();

                foreach(GraphElement element in selection)
                {
                    if(element is T11Node node)
                    {
                        nodesToDelete.Add(node);
                        continue;
                    }
                }

                foreach(T11Node node in nodesToDelete)
                {
                    RemoveUngroupedNode(node);
                    RemoveElement(node);
                }
            };
        }
    }
}
