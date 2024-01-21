using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace T11.Elements
{
    using Enumerations;
    using System;
    using Data.Save;
    using Utilities;
    using Windows;


    public class T11Node : Node
    {
        public string ID { get; set; }
        public string DialogueName { get; set; }
        public List<T11ChoiceSaveData> Choices { get; set; }
        public string Text { get; set; }
        public T11DialogueType DialogueType { get; set; }
        public T11Group Group { get; set; }
        protected T11GraphView graphView;
        private Color defaultBackgroundColor;

        public virtual void Initialize(T11GraphView t11GraphView, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            DialogueName = "Type Dialogue Name";
            Choices = new List<T11ChoiceSaveData>();
            Text = "Write dialogue here.";

            graphView = t11GraphView;
            defaultBackgroundColor = new Color(29f / 255, 29f / 255, 30f / 255);
            SetPosition(new Rect(position, Vector2.zero));

            mainContainer.AddToClassList("t11-node__main-container");
            extensionContainer.AddToClassList("t11-node__extension-container");
        }

        public virtual void Draw()
        {
            Label title = new Label("Dialogue Name:");
            /* TITLE Container */
            TextField DialogueNameTextField = T11ElementUtility.CreateTextField(DialogueName, null, callback =>
            {
                TextField target = (TextField)callback.target;

                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(target.value))
                {
                    if(!string.IsNullOrEmpty(DialogueName)) 
                    {
                        ++graphView.NameErrorsAmount;                    
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(DialogueName))
                    {
                        --graphView.NameErrorsAmount;
                    }
                }

                if (Group == null) 
                {
                    graphView.RemoveUngroupedNode(this);
                    DialogueName = target.value;
                    graphView.AddUngroupedNode(this);
                    return;
                }

                T11Group currentGroup = Group;

                graphView.RemoveGroupedNode(this, Group);
                DialogueName = callback.newValue;
                graphView.AddGroupedNode(this, currentGroup);
            });

            DialogueNameTextField.AddClasses(
                "t11-node__textfield",
                "t11-node__filename-textfield",
                "t11-node__textfield_hidden"
            );

            titleContainer.Insert(0, title);
            titleContainer.Insert(1, DialogueNameTextField);

            /* INPUT Container */
            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Dialogue Connection";
            inputContainer.Add(inputPort);

            /* EXTENSION Container */
            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("t11-node__custom-data-container");

            Foldout textFoldout = T11ElementUtility.CreateFoldout("Dialogue Details");

            TextField textTextField = T11ElementUtility.CreateTextArea(Text, null, callback =>
            {
                Text = callback.newValue;
            });

            textTextField.AddClasses(
                "t11-node__textfield",
                "t11-node__quote-textfield"
            );

            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);

            extensionContainer.Add(customDataContainer);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());
            base.BuildContextualMenu(evt);
        }

        public void DisconnectAllPorts()
        {
            DisconnectPorts(inputContainer);
            DisconnectPorts(outputContainer);
        }

        private void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        private void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }

        private void DisconnectPorts(VisualElement container)
        {
            foreach(Port port in container.Children())
            {
                if (!port.connected)
                {
                    continue;
                }

                graphView.DeleteElements(port.connections);
            }
        }

        public bool IsStartingNode()
        {
            Port inputPort = (Port)inputContainer.Children().First();
            return !inputPort.connected;
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }
    }
}
