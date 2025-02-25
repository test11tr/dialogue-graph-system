using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace T11.Windows
{
    using Elements;
    using Enumerations;
    using Utilities;
    using Data.Error;
    using Data.Save;
    using static UnityEditor.Experimental.GraphView.GraphView;
    using static UnityEngine.GraphicsBuffer;

    public class T11GraphView : GraphView
    {
        private T11EditorWindow editorWindow;
        private T11SearchWindow searchWindow;

        private MiniMap miniMap;

        private SerializableDictionary<string, T11NodeErrorData> ungroupedNodes;
        private SerializableDictionary<string, T11GroupErrorData> groups;
        private SerializableDictionary<Group, SerializableDictionary<string, T11NodeErrorData>> groupedNodes;

        private int nameErrorsAmount;

        public int NameErrorsAmount
        {
            get 
            { 
                return nameErrorsAmount; 
            }
            set 
            { 
                nameErrorsAmount = value;
                
                if(nameErrorsAmount == 0)
                {
                    editorWindow.EnableSaving();
                }

                if(nameErrorsAmount == 1)
                {
                    editorWindow.DisableSaving();
                }
            }
        }

        public T11GraphView(T11EditorWindow t11EditorWindow)
        {
            editorWindow = t11EditorWindow;

            ungroupedNodes = new SerializableDictionary<string, T11NodeErrorData>();
            groups = new SerializableDictionary<string, T11GroupErrorData>();
            groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, T11NodeErrorData>>();

            AddManipulators();
            AddGridBacground();
            AddSearchWindow();
            AddMiniMap();

            OnElementsDeleted();
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();
            OnGraphViewChanged();

            AddStyles();
            AddMiniMapStyles();
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

        private void AddMiniMapStyles()
        {
            StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new StyleColor(new Color32(51,51,51,255));

            miniMap.style.backgroundColor = backgroundColor;
            miniMap.style.borderTopColor = borderColor;
            miniMap.style.borderBottomColor = borderColor;
            miniMap.style.borderLeftColor = borderColor;
            miniMap.style.borderRightColor = borderColor;
        }

        private void AddSearchWindow()
        {
            if( searchWindow == null )
            {
                searchWindow = ScriptableObject.CreateInstance<T11SearchWindow>();
                searchWindow.Initialize(this);
            }

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        private void AddMiniMap()
        {
            miniMap = new MiniMap()
            {
                anchored = true,
            };

            miniMap.SetPosition(new Rect(15, 50, 200, 100));
            Add(miniMap);
            miniMap.visible = false;
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
                MenuEvent => MenuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode("Type a Dialogue Name", dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );
            return contextualMenuManipulator;
        }

        public T11Node CreateNode(string nodeName, T11DialogueType dialogueType, Vector2 position, bool shouldDraw = true)
        {
            Type nodeType = Type.GetType($"T11.Elements.T11{dialogueType}Node");
            T11Node node = (T11Node) Activator.CreateInstance( nodeType );
            node.Initialize(nodeName, this, position);

            if(shouldDraw )
            {
                node.Draw();
            }

            AddUngroupedNode(node);
            return node;
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                MenuEvent => MenuEvent.menu.AppendAction("Add Group", actionEvent => CreateGroup("Dialogue Group", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
            );
            return contextualMenuManipulator;
        }

        public T11Group CreateGroup(string title, Vector2 localMousePosition)
        {
            T11Group group = new T11Group(title, localMousePosition);

            AddGroup(group);

            AddElement(group);

            foreach(GraphElement selectedElement in selection)
            {
                if(!(selectedElement is T11Node))
                {
                    continue;
                }

                T11Node node = (T11Node) selectedElement;

                group.AddElement(node);
            }

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
            string nodeName = node.DialogueName.ToLower();

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
                ++NameErrorsAmount;
                ungroupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveUngroupedNode(T11Node node)
        {
            string nodeName = node.DialogueName.ToLower();

            List<T11Node> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;

            ungroupedNodesList.Remove(node);

            node.ResetStyle();

            if (ungroupedNodesList.Count == 1)
            {
                --NameErrorsAmount;
                ungroupedNodesList[0].ResetStyle();
                return;
            }

            if (ungroupedNodesList.Count == 0)
            {
                ungroupedNodes.Remove(nodeName);
            }
        }

        private void AddGroup(T11Group group)
        {
            string groupName = group.title.ToLower();
            if(!groups.ContainsKey(groupName))
            {
                T11GroupErrorData groupErrorData = new T11GroupErrorData();
                groupErrorData.Groups.Add(group);
                groups.Add(groupName, groupErrorData);
                return;
            }

            List<T11Group> groupsList = groups[groupName].Groups;

            groupsList.Add(group);
            Color errorColor = groups[groupName].ErrorData.Color;
            group.SetErrorStyle(errorColor);

            if(groupsList.Count == 2)
            {
                ++NameErrorsAmount;
                groupsList[0].SetErrorStyle(errorColor);
            }
        }

        private void RemoveGroup(T11Group group)
        {
            string oldGroupName = group.OldTitle.ToLower();
            List<T11Group> groupsList = groups[oldGroupName].Groups;
            groupsList.Remove(group);
            group.ResetStyle();

            if(groupsList.Count == 1)
            {
                --NameErrorsAmount;
                groupsList[0].ResetStyle();
                return;
            }

            if(groupsList.Count == 0) 
            {
                groups.Remove(oldGroupName);
            }
        }

        public void AddGroupedNode(T11Node node, T11Group group)
        {
            string nodeName = node.DialogueName.ToLower();

            node.Group = group;

            if(!groupedNodes.ContainsKey(group))
            {
                groupedNodes.Add(group, new SerializableDictionary<string, T11NodeErrorData>());
            }

            if (!groupedNodes[group].ContainsKey(nodeName))
            {
                T11NodeErrorData nodeErrorData = new T11NodeErrorData();
                
                nodeErrorData.Nodes.Add(node);

                groupedNodes[group].Add(nodeName, nodeErrorData);
                
                return;
            }

            List<T11Node> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            groupedNodesList.Add(node);

            Color errorColor = groupedNodes[group][nodeName].ErrorData.Color;

            node.SetErrorStyle(errorColor);

            if (groupedNodesList.Count == 2)
            {
                ++NameErrorsAmount;
                groupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveGroupedNode(T11Node node, T11Group group)
        {
            string nodeName = node.DialogueName.ToLower();

            node.Group = null;

            List<T11Node> groupNodesList = groupedNodes[group][nodeName].Nodes;

            groupNodesList.Remove(node);

            node.ResetStyle();

            if (groupNodesList.Count == 1)
            {
                --NameErrorsAmount;
                groupNodesList[0].ResetStyle();
                return;
            }

            if (groupNodesList.Count == 0)
            {
                groupedNodes[group].Remove(nodeName);

                if (groupedNodes[group].Count == 0)
                {
                    groupedNodes.Remove(group);
                }
            }
        }

        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                Type groupType = typeof(T11Group);
                Type edgeType = typeof(Edge);

                List<T11Group> groupsToDelete = new List<T11Group>();
                List<T11Node> nodesToDelete = new List<T11Node>();
                List<Edge> edgesToDelete = new List<Edge>();

                foreach(GraphElement element in selection)
                {
                    if(element is T11Node node)
                    {
                        nodesToDelete.Add(node);
                        continue;
                    }

                    if(element.GetType() == edgeType)
                    {
                        Edge edge = (Edge) element;

                        edgesToDelete.Add(edge);
                        continue;
                    }

                    if(element.GetType() != groupType)
                    {
                        continue;
                    }

                    T11Group group = (T11Group) element;
                    
                    groupsToDelete.Add(group);
                }

                foreach(T11Group group in groupsToDelete)
                {
                    List<T11Node> groupNodes = new List<T11Node>();

                    foreach(GraphElement groupElement in group.containedElements)
                    {
                        if(!(groupElement is T11Node))
                        {
                            continue;
                        }

                        T11Node groupNode = (T11Node) groupElement;
                        groupNodes.Add(groupNode);
                    }

                    group.RemoveElements(groupNodes);
                    RemoveGroup(group);
                    RemoveElement(group);
                }

                DeleteElements(edgesToDelete);

                foreach(T11Node node in nodesToDelete)
                {
                    if(node.Group != null)
                    {
                        node.Group.RemoveElement(node);
                    }

                    RemoveUngroupedNode(node);

                    node.DisconnectAllPorts();

                    RemoveElement(node);
                }
            };
        }

        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if(!(element is T11Node))
                    {
                        continue;
                    }

                    T11Group nodeGroup = (T11Group) group;
                    T11Node node = (T11Node) element;

                    RemoveUngroupedNode(node);
                    AddGroupedNode(node, nodeGroup);
                }
            };
        }

        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is T11Node))
                    {
                        continue;
                    }

                    T11Group t11Group = (T11Group)group;
                    T11Node node = (T11Node)element;

                    RemoveGroupedNode(node, t11Group);
                    AddUngroupedNode(node);
                }
            };
        }

        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                T11Group t11Group = (T11Group) group;
                t11Group.title = newTitle.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(t11Group.title))
                {
                    if (!string.IsNullOrEmpty(t11Group.OldTitle))
                    {
                        ++NameErrorsAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(t11Group.OldTitle))
                    {
                        --NameErrorsAmount;
                    }
                }

                RemoveGroup(t11Group);

                t11Group.OldTitle = t11Group.title;
                AddGroup(t11Group);
            };
        }

        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if(changes.edgesToCreate != null)
                {
                    foreach(Edge edge in changes.edgesToCreate)
                    {
                        T11Node nextNode = (T11Node) edge.input.node;
                        T11ChoiceSaveData choiceData = (T11ChoiceSaveData) edge.output.userData;
                        choiceData.NodeID = nextNode.ID;
                    }
                }

                if(changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);
                    foreach(GraphElement element in changes.elementsToRemove)
                    {
                        if(element.GetType() != edgeType)
                        {
                            continue;
                        }

                        Edge edge = (Edge) element;
                        T11ChoiceSaveData choiceData = (T11ChoiceSaveData) edge.output.userData;
                        choiceData.NodeID = "";
                    }
                }

                return changes;
            };
        }

        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));
            groups.Clear();
            groupedNodes.Clear();
            ungroupedNodes.Clear();
            nameErrorsAmount = 0;
        }

        public void ToggleMiniMap()
        {
            miniMap.visible = !miniMap.visible;
        }
    }
}
